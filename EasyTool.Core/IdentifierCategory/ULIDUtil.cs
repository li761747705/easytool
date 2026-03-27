using System;
using System.Security.Cryptography;
using System.Text;

namespace EasyTool.IdentifierCategory
{
    /// <summary>
    /// ULID（Universally Unique Lexicographically Sortable Identifier）生成器
    /// ULID 是一种可排序的唯一标识符，由 48 位时间戳和 80 位随机数组成
    /// </summary>
    public static class UlidUtil
    {
        private static readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();
        private static readonly char[] EncodingChars = "0123456789ABCDEFGHJKMNPQRSTVWXYZ".ToCharArray();
        private static readonly byte[] DecodingMap = BuildDecodingMap();

        private const int TimestampLength = 6;
        private const int RandomnessLength = 10;
        private const int UlidLength = 16;
        private const int StringLength = 26;

        /// <summary>
        /// 生成新的 ULID
        /// </summary>
        /// <returns>ULID 字节数组</returns>
        public static byte[] Generate()
        {
            return Generate(DateTimeOffset.UtcNow);
        }

        /// <summary>
        /// 生成指定时间的 ULID
        /// </summary>
        /// <param name="timestamp">时间戳</param>
        /// <returns>ULID 字节数组</returns>
        public static byte[] Generate(DateTimeOffset timestamp)
        {
            var ulid = new byte[UlidLength];
            var timestampMs = timestamp.ToUnixTimeMilliseconds();

            // 写入时间戳（6字节，大端序）
            ulid[0] = (byte)(timestampMs >> 40);
            ulid[1] = (byte)(timestampMs >> 32);
            ulid[2] = (byte)(timestampMs >> 24);
            ulid[3] = (byte)(timestampMs >> 16);
            ulid[4] = (byte)(timestampMs >> 8);
            ulid[5] = (byte)timestampMs;

            // 写入随机数（10字节）
            _rng.GetBytes(ulid, TimestampLength, RandomnessLength);

            return ulid;
        }

        /// <summary>
        /// 生成新的 ULID 字符串
        /// </summary>
        /// <returns>ULID 字符串（26字符）</returns>
        public static string GenerateString()
        {
            return Encode(Generate());
        }

        /// <summary>
        /// 生成指定时间的 ULID 字符串
        /// </summary>
        /// <param name="timestamp">时间戳</param>
        /// <returns>ULID 字符串（26字符）</returns>
        public static string GenerateString(DateTimeOffset timestamp)
        {
            return Encode(Generate(timestamp));
        }

        /// <summary>
        /// 生成 ULID 结构体
        /// </summary>
        /// <returns>ULID 结构体</returns>
        public static Ulid GenerateUlid()
        {
            return new Ulid(Generate());
        }

        /// <summary>
        /// 生成指定时间的 ULID 结构体
        /// </summary>
        /// <param name="timestamp">时间戳</param>
        /// <returns>ULID 结构体</returns>
        public static Ulid GenerateUlid(DateTimeOffset timestamp)
        {
            return new Ulid(Generate(timestamp));
        }

        /// <summary>
        /// 将 ULID 字节数组编码为字符串
        /// </summary>
        /// <param name="bytes">ULID 字节数组</param>
        /// <returns>ULID 字符串</returns>
        public static string Encode(byte[] bytes)
        {
            if (bytes == null || bytes.Length != UlidLength)
            {
                throw new ArgumentException($"ULID 字节数组长度必须为 {UlidLength}", nameof(bytes));
            }

            var result = new char[StringLength];
            var buffer = 0;
            var bufferBits = 0;
            var index = StringLength - 1;

            for (int i = UlidLength - 1; i >= 0; i--)
            {
                buffer = (buffer << 8) | bytes[i];
                bufferBits += 8;

                while (bufferBits >= 5)
                {
                    result[index--] = EncodingChars[(buffer >> (bufferBits - 5)) & 0x1F];
                    bufferBits -= 5;
                }
            }

            if (bufferBits > 0)
            {
                result[index] = EncodingChars[buffer & 0x1F];
            }

            return new string(result);
        }

        /// <summary>
        /// 将 ULID 字符串解码为字节数组
        /// </summary>
        /// <param name="ulid">ULID 字符串</param>
        /// <returns>ULID 字节数组</returns>
        public static byte[] Decode(string ulid)
        {
            if (string.IsNullOrEmpty(ulid) || ulid.Length != StringLength)
            {
                throw new ArgumentException($"ULID 字符串长度必须为 {StringLength}", nameof(ulid));
            }

            var result = new byte[UlidLength];
            var buffer = 0;
            var bufferBits = 0;
            var index = UlidLength - 1;

            for (int i = StringLength - 1; i >= 0; i--)
            {
                var c = ulid[i];
                var value = DecodingMap[c];

                if (value == 0xFF)
                {
                    throw new ArgumentException($"无效的 ULID 字符: {c}", nameof(ulid));
                }

                buffer = (buffer << 5) | value;
                bufferBits += 5;

                while (bufferBits >= 8)
                {
                    result[index--] = (byte)((buffer >> (bufferBits - 8)) & 0xFF);
                    bufferBits -= 8;
                }
            }

            return result;
        }

        /// <summary>
        /// 从 ULID 提取时间戳
        /// </summary>
        /// <param name="ulid">ULID 字节数组</param>
        /// <returns>时间戳</returns>
        public static DateTimeOffset ExtractTimestamp(byte[] ulid)
        {
            if (ulid == null || ulid.Length != UlidLength)
            {
                throw new ArgumentException($"ULID 字节数组长度必须为 {UlidLength}", nameof(ulid));
            }

            var timestampMs = ((long)ulid[0] << 40) |
                              ((long)ulid[1] << 32) |
                              ((long)ulid[2] << 24) |
                              ((long)ulid[3] << 16) |
                              ((long)ulid[4] << 8) |
                              ulid[5];

            return DateTimeOffset.FromUnixTimeMilliseconds(timestampMs);
        }

        /// <summary>
        /// 从 ULID 字符串提取时间戳
        /// </summary>
        /// <param name="ulid">ULID 字符串</param>
        /// <returns>时间戳</returns>
        public static DateTimeOffset ExtractTimestamp(string ulid)
        {
            return ExtractTimestamp(Decode(ulid));
        }

        /// <summary>
        /// 验证 ULID 字符串是否有效
        /// </summary>
        /// <param name="ulid">ULID 字符串</param>
        /// <returns>是否有效</returns>
        public static bool IsValid(string ulid)
        {
            if (string.IsNullOrEmpty(ulid) || ulid.Length != StringLength)
            {
                return false;
            }

            foreach (var c in ulid)
            {
                if (c >= DecodingMap.Length || DecodingMap[c] == 0xFF)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 比较两个 ULID 的大小
        /// </summary>
        /// <param name="a">第一个 ULID</param>
        /// <param name="b">第二个 ULID</param>
        /// <returns>比较结果</returns>
        public static int Compare(string a, string b)
        {
            return string.CompareOrdinal(a, b);
        }

        /// <summary>
        /// 获取最小 ULID（指定时间）
        /// </summary>
        /// <param name="timestamp">时间戳</param>
        /// <returns>最小 ULID 字符串</returns>
        public static string Min(DateTimeOffset timestamp)
        {
            var ulid = new byte[UlidLength];
            var timestampMs = timestamp.ToUnixTimeMilliseconds();

            ulid[0] = (byte)(timestampMs >> 40);
            ulid[1] = (byte)(timestampMs >> 32);
            ulid[2] = (byte)(timestampMs >> 24);
            ulid[3] = (byte)(timestampMs >> 16);
            ulid[4] = (byte)(timestampMs >> 8);
            ulid[5] = (byte)timestampMs;
            // 随机部分全部为 0

            return Encode(ulid);
        }

        /// <summary>
        /// 获取最大 ULID（指定时间）
        /// </summary>
        /// <param name="timestamp">时间戳</param>
        /// <returns>最大 ULID 字符串</returns>
        public static string Max(DateTimeOffset timestamp)
        {
            var ulid = new byte[UlidLength];
            var timestampMs = timestamp.ToUnixTimeMilliseconds();

            ulid[0] = (byte)(timestampMs >> 40);
            ulid[1] = (byte)(timestampMs >> 32);
            ulid[2] = (byte)(timestampMs >> 24);
            ulid[3] = (byte)(timestampMs >> 16);
            ulid[4] = (byte)(timestampMs >> 8);
            ulid[5] = (byte)timestampMs;
            // 随机部分全部为 0xFF
            for (int i = TimestampLength; i < UlidLength; i++)
            {
                ulid[i] = 0xFF;
            }

            return Encode(ulid);
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
            map['j'] = map['J'];
            map['k'] = map['K'];
            map['m'] = map['M'];
            map['n'] = map['N'];
            map['p'] = map['P'];
            map['q'] = map['Q'];
            map['r'] = map['R'];
            map['s'] = map['S'];
            map['t'] = map['T'];
            map['v'] = map['V'];
            map['w'] = map['W'];
            map['x'] = map['X'];
            map['y'] = map['Y'];
            map['z'] = map['Z'];

            return map;
        }
    }

    /// <summary>
    /// ULID 结构体
    /// </summary>
    public readonly struct Ulid : IComparable<Ulid>, IEquatable<Ulid>
    {
        private readonly byte[] _bytes;

        /// <summary>
        /// 创建 ULID
        /// </summary>
        /// <param name="bytes">字节数组</param>
        public Ulid(byte[] bytes)
        {
            if (bytes == null || bytes.Length != 16)
            {
                throw new ArgumentException("ULID 字节数组长度必须为 16", nameof(bytes));
            }
            _bytes = bytes;
        }

        /// <summary>
        /// 时间戳
        /// </summary>
        public DateTimeOffset Timestamp => UlidUtil.ExtractTimestamp(_bytes);

        /// <summary>
        /// 字节数组
        /// </summary>
        public byte[] ToByteArray() => (byte[])_bytes.Clone();

        /// <summary>
        /// 转换为字符串
        /// </summary>
        public override string ToString() => UlidUtil.Encode(_bytes);

        /// <summary>
        /// 比较大小
        /// </summary>
        public int CompareTo(Ulid other)
        {
            for (int i = 0; i < 16; i++)
            {
                if (_bytes[i] != other._bytes[i])
                {
                    return _bytes[i].CompareTo(other._bytes[i]);
                }
            }
            return 0;
        }

        /// <summary>
        /// 判断相等
        /// </summary>
        public bool Equals(Ulid other)
        {
            for (int i = 0; i < 16; i++)
            {
                if (_bytes[i] != other._bytes[i])
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 判断相等
        /// </summary>
        public override bool Equals(object? obj)
        {
            return obj is Ulid other && Equals(other);
        }

        /// <summary>
        /// 获取哈希码
        /// </summary>
        public override int GetHashCode()
        {
            var hash = 0;
            for (int i = 0; i < 16; i++)
            {
                hash = (hash << 2) ^ _bytes[i];
            }
            return hash;
        }

        /// <summary>
        /// 等于运算符
        /// </summary>
        public static bool operator ==(Ulid left, Ulid right) => left.Equals(right);

        /// <summary>
        /// 不等于运算符
        /// </summary>
        public static bool operator !=(Ulid left, Ulid right) => !left.Equals(right);

        /// <summary>
        /// 小于运算符
        /// </summary>
        public static bool operator <(Ulid left, Ulid right) => left.CompareTo(right) < 0;

        /// <summary>
        /// 大于运算符
        /// </summary>
        public static bool operator >(Ulid left, Ulid right) => left.CompareTo(right) > 0;

        /// <summary>
        /// 小于等于运算符
        /// </summary>
        public static bool operator <=(Ulid left, Ulid right) => left.CompareTo(right) <= 0;

        /// <summary>
        /// 大于等于运算符
        /// </summary>
        public static bool operator >=(Ulid left, Ulid right) => left.CompareTo(right) >= 0;

        /// <summary>
        /// 生成新 ULID
        /// </summary>
        public static Ulid NewUlid() => UlidUtil.GenerateUlid();

        /// <summary>
        /// 解析字符串
        /// </summary>
        public static Ulid Parse(string ulid) => new Ulid(UlidUtil.Decode(ulid));

        /// <summary>
        /// 尝试解析字符串
        /// </summary>
        public static bool TryParse(string ulid, out Ulid result)
        {
            if (UlidUtil.IsValid(ulid))
            {
                result = Parse(ulid);
                return true;
            }
            result = default;
            return false;
        }
    }
}
