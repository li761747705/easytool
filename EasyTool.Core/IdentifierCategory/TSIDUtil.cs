using System;
using System.Security.Cryptography;
using System.Threading;

namespace EasyTool.IdentifierCategory
{
    /// <summary>
    /// TSID（Time-Sorted Identifier）生成器
    /// 生成可按时间排序的唯一标识符，支持分布式环境
    /// </summary>
    public static class TsidUtil
    {
        private static readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();
        private static readonly char[] EncodingChars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
        private static readonly byte[] DecodingMap = BuildDecodingMap();

        private const int TimestampBits = 42;
        private const int NodeIdBits = 8;
        private const int SequenceBits = 14;

        private const long MaxTimestamp = (1L << TimestampBits) - 1;
        private const int MaxNodeId = (1 << NodeIdBits) - 1;
        private const int MaxSequence = (1 << SequenceBits) - 1;

        private static readonly long CustomEpoch = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero).ToUnixTimeMilliseconds();
        private static int _nodeId;
        private static int _sequence;
        private static long _lastTimestamp = -1;
        private static readonly object _lock = new();

        static TsidUtil()
        {
            _nodeId = GenerateNodeId();
            _sequence = GetRandomSequence();
        }

        /// <summary>
        /// 生成新的 TSID
        /// </summary>
        /// <returns>TSID 长整型</returns>
        public static long Generate()
        {
            lock (_lock)
            {
                var timestamp = GetCurrentTimestamp();

                if (timestamp == _lastTimestamp)
                {
                    _sequence = (_sequence + 1) & MaxSequence;
                    if (_sequence == 0)
                    {
                        // 序列号溢出，等待下一毫秒
                        timestamp = WaitNextMillis(timestamp);
                    }
                }
                else
                {
                    _sequence = GetRandomSequence();
                }

                _lastTimestamp = timestamp;

                return ((timestamp & MaxTimestamp) << (NodeIdBits + SequenceBits))
                       | ((long)_nodeId << SequenceBits)
                       | _sequence;
            }
        }

        /// <summary>
        /// 生成新的 TSID 字符串
        /// </summary>
        /// <returns>TSID 字符串（13字符）</returns>
        public static string GenerateString()
        {
            return Encode(Generate());
        }

        /// <summary>
        /// 使用指定节点 ID 生成 TSID
        /// </summary>
        /// <param name="nodeId">节点 ID（0-255）</param>
        /// <returns>TSID 长整型</returns>
        public static long Generate(int nodeId)
        {
            if (nodeId < 0 || nodeId > MaxNodeId)
            {
                throw new ArgumentOutOfRangeException(nameof(nodeId), $"节点 ID 必须在 0 到 {MaxNodeId} 之间");
            }

            lock (_lock)
            {
                var timestamp = GetCurrentTimestamp();

                if (timestamp == _lastTimestamp)
                {
                    _sequence = (_sequence + 1) & MaxSequence;
                    if (_sequence == 0)
                    {
                        timestamp = WaitNextMillis(timestamp);
                    }
                }
                else
                {
                    _sequence = GetRandomSequence();
                }

                _lastTimestamp = timestamp;

                return ((timestamp & MaxTimestamp) << (NodeIdBits + SequenceBits))
                       | ((long)nodeId << SequenceBits)
                       | _sequence;
            }
        }

        /// <summary>
        /// 使用指定节点 ID 生成 TSID 字符串
        /// </summary>
        /// <param name="nodeId">节点 ID</param>
        /// <returns>TSID 字符串</returns>
        public static string GenerateString(int nodeId)
        {
            return Encode(Generate(nodeId));
        }

        /// <summary>
        /// 将 TSID 长整型编码为字符串
        /// </summary>
        /// <param name="tsid">TSID 值</param>
        /// <returns>TSID 字符串</returns>
        public static string Encode(long tsid)
        {
            var chars = new char[13];

            for (int i = 12; i >= 0; i--)
            {
                chars[i] = EncodingChars[(int)(tsid & 0x1F)];
                tsid >>= 5;
            }

            return new string(chars);
        }

        /// <summary>
        /// 将 TSID 字符串解码为长整型
        /// </summary>
        /// <param name="tsid">TSID 字符串</param>
        /// <returns>TSID 长整型</returns>
        public static long Decode(string tsid)
        {
            if (string.IsNullOrEmpty(tsid) || tsid.Length != 13)
            {
                throw new ArgumentException("TSID 字符串长度必须为 13", nameof(tsid));
            }

            long result = 0;

            foreach (var c in tsid)
            {
                if (c >= DecodingMap.Length || DecodingMap[c] == 0xFF)
                {
                    throw new ArgumentException($"无效的 TSID 字符: {c}", nameof(tsid));
                }

                result = (result << 5) | DecodingMap[c];
            }

            return result;
        }

        /// <summary>
        /// 从 TSID 提取时间戳
        /// </summary>
        /// <param name="tsid">TSID 值</param>
        /// <returns>时间戳</returns>
        public static DateTimeOffset ExtractTimestamp(long tsid)
        {
            var timestamp = tsid >> (NodeIdBits + SequenceBits);
            var milliseconds = timestamp + CustomEpoch;
            return DateTimeOffset.FromUnixTimeMilliseconds(milliseconds);
        }

        /// <summary>
        /// 从 TSID 字符串提取时间戳
        /// </summary>
        /// <param name="tsid">TSID 字符串</param>
        /// <returns>时间戳</returns>
        public static DateTimeOffset ExtractTimestamp(string tsid)
        {
            return ExtractTimestamp(Decode(tsid));
        }

        /// <summary>
        /// 从 TSID 提取节点 ID
        /// </summary>
        /// <param name="tsid">TSID 值</param>
        /// <returns>节点 ID</returns>
        public static int ExtractNodeId(long tsid)
        {
            return (int)((tsid >> SequenceBits) & MaxNodeId);
        }

        /// <summary>
        /// 从 TSID 字符串提取节点 ID
        /// </summary>
        /// <param name="tsid">TSID 字符串</param>
        /// <returns>节点 ID</returns>
        public static int ExtractNodeId(string tsid)
        {
            return ExtractNodeId(Decode(tsid));
        }

        /// <summary>
        /// 从 TSID 提取序列号
        /// </summary>
        /// <param name="tsid">TSID 值</param>
        /// <returns>序列号</returns>
        public static int ExtractSequence(long tsid)
        {
            return (int)(tsid & MaxSequence);
        }

        /// <summary>
        /// 从 TSID 字符串提取序列号
        /// </summary>
        /// <param name="tsid">TSID 字符串</param>
        /// <returns>序列号</returns>
        public static int ExtractSequence(string tsid)
        {
            return ExtractSequence(Decode(tsid));
        }

        /// <summary>
        /// 验证 TSID 字符串是否有效
        /// </summary>
        /// <param name="tsid">TSID 字符串</param>
        /// <returns>是否有效</returns>
        public static bool IsValid(string tsid)
        {
            if (string.IsNullOrEmpty(tsid) || tsid.Length != 13)
            {
                return false;
            }

            foreach (var c in tsid)
            {
                if (c >= DecodingMap.Length || DecodingMap[c] == 0xFF)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 设置节点 ID
        /// </summary>
        /// <param name="nodeId">节点 ID（0-255）</param>
        public static void SetNodeId(int nodeId)
        {
            if (nodeId < 0 || nodeId > MaxNodeId)
            {
                throw new ArgumentOutOfRangeException(nameof(nodeId), $"节点 ID 必须在 0 到 {MaxNodeId} 之间");
            }
            _nodeId = nodeId;
        }

        /// <summary>
        /// 获取当前节点 ID
        /// </summary>
        /// <returns>节点 ID</returns>
        public static int GetNodeId()
        {
            return _nodeId;
        }

        /// <summary>
        /// 比较两个 TSID 的大小
        /// </summary>
        /// <param name="a">第一个 TSID</param>
        /// <param name="b">第二个 TSID</param>
        /// <returns>比较结果</returns>
        public static int Compare(long a, long b)
        {
            return a.CompareTo(b);
        }

        /// <summary>
        /// 比较两个 TSID 字符串的大小
        /// </summary>
        /// <param name="a">第一个 TSID 字符串</param>
        /// <param name="b">第二个 TSID 字符串</param>
        /// <returns>比较结果</returns>
        public static int Compare(string a, string b)
        {
            return string.CompareOrdinal(a, b);
        }

        #region 私有方法

        private static long GetCurrentTimestamp()
        {
            var currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var timestamp = currentTime - CustomEpoch;

            if (timestamp < 0)
            {
                throw new InvalidOperationException("当前时间早于自定义纪元时间");
            }

            if (timestamp > MaxTimestamp)
            {
                throw new OverflowException("时间戳溢出");
            }

            return timestamp;
        }

        private static long WaitNextMillis(long currentTimestamp)
        {
            var timestamp = GetCurrentTimestamp();
            while (timestamp <= currentTimestamp)
            {
                Thread.SpinWait(10);
                timestamp = GetCurrentTimestamp();
            }
            return timestamp;
        }

        private static int GenerateNodeId()
        {
            var bytes = new byte[1];
            _rng.GetBytes(bytes);
            return bytes[0];
        }

        private static int GetRandomSequence()
        {
            var bytes = new byte[2];
            _rng.GetBytes(bytes);
            return ((bytes[0] << 6) | (bytes[1] >> 2)) & MaxSequence;
        }

        private static byte[] BuildDecodingMap()
        {
            var map = new byte[256];
            Array.Fill(map, (byte)0xFF);

            for (int i = 0; i < EncodingChars.Length; i++)
            {
                map[EncodingChars[i]] = (byte)i;
            }

            // 支持小写字母
            map['a'] = map['A'];
            map['b'] = map['B'];
            map['c'] = map['C'];
            map['d'] = map['D'];
            map['e'] = map['E'];
            map['f'] = map['F'];
            map['g'] = map['G'];
            map['h'] = map['H'];
            map['i'] = map['I'];
            map['j'] = map['J'];
            map['k'] = map['K'];
            map['l'] = map['L'];
            map['m'] = map['M'];
            map['n'] = map['N'];
            map['o'] = map['O'];
            map['p'] = map['P'];
            map['q'] = map['Q'];
            map['r'] = map['R'];
            map['s'] = map['S'];
            map['t'] = map['T'];
            map['u'] = map['U'];
            map['v'] = map['V'];
            map['w'] = map['W'];
            map['x'] = map['X'];
            map['y'] = map['Y'];
            map['z'] = map['Z'];

            return map;
        }

        #endregion
    }

    /// <summary>
    /// TSID 结构体
    /// </summary>
    public readonly struct Tsid : IComparable<Tsid>, IEquatable<Tsid>
    {
        private readonly long _value;

        /// <summary>
        /// 创建 TSID
        /// </summary>
        /// <param name="value">TSID 值</param>
        public Tsid(long value)
        {
            _value = value;
        }

        /// <summary>
        /// TSID 值
        /// </summary>
        public long Value => _value;

        /// <summary>
        /// 时间戳
        /// </summary>
        public DateTimeOffset Timestamp => TsidUtil.ExtractTimestamp(_value);

        /// <summary>
        /// 节点 ID
        /// </summary>
        public int NodeId => TsidUtil.ExtractNodeId(_value);

        /// <summary>
        /// 序列号
        /// </summary>
        public int Sequence => TsidUtil.ExtractSequence(_value);

        /// <summary>
        /// 转换为字符串
        /// </summary>
        public override string ToString() => TsidUtil.Encode(_value);

        /// <summary>
        /// 比较大小
        /// </summary>
        public int CompareTo(Tsid other) => _value.CompareTo(other._value);

        /// <summary>
        /// 判断相等
        /// </summary>
        public bool Equals(Tsid other) => _value == other._value;

        /// <summary>
        /// 判断相等
        /// </summary>
        public override bool Equals(object? obj) => obj is Tsid other && Equals(other);

        /// <summary>
        /// 获取哈希码
        /// </summary>
        public override int GetHashCode() => _value.GetHashCode();

        /// <summary>
        /// 等于运算符
        /// </summary>
        public static bool operator ==(Tsid left, Tsid right) => left.Equals(right);

        /// <summary>
        /// 不等于运算符
        /// </summary>
        public static bool operator !=(Tsid left, Tsid right) => !left.Equals(right);

        /// <summary>
        /// 小于运算符
        /// </summary>
        public static bool operator <(Tsid left, Tsid right) => left.CompareTo(right) < 0;

        /// <summary>
        /// 大于运算符
        /// </summary>
        public static bool operator >(Tsid left, Tsid right) => left.CompareTo(right) > 0;

        /// <summary>
        /// 小于等于运算符
        /// </summary>
        public static bool operator <=(Tsid left, Tsid right) => left.CompareTo(right) <= 0;

        /// <summary>
        /// 大于等于运算符
        /// </summary>
        public static bool operator >=(Tsid left, Tsid right) => left.CompareTo(right) >= 0;

        /// <summary>
        /// 生成新 TSID
        /// </summary>
        public static Tsid NewTsid() => new Tsid(TsidUtil.Generate());

        /// <summary>
        /// 解析字符串
        /// </summary>
        public static Tsid Parse(string tsid) => new Tsid(TsidUtil.Decode(tsid));

        /// <summary>
        /// 尝试解析字符串
        /// </summary>
        public static bool TryParse(string tsid, out Tsid result)
        {
            if (TsidUtil.IsValid(tsid))
            {
                result = Parse(tsid);
                return true;
            }
            result = default;
            return false;
        }
    }
}
