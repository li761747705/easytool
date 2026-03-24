using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace EasyTool.CodeCategory
{
    /// <summary>
    /// TSID（Time-Sorted ID）工具类
    /// TSID 是一种时间排序的唯一标识符
    /// 支持多种格式：TSID-256（8字符）、TSID-512（13字符）、TSID-1024（18字符）
    /// </summary>
    public static class TSIDUtil
    {
        private static readonly DateTime Epoch = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private static long _lastTimestamp = -1L;
        private static int _sequence = 0;
        private static readonly object _lock = new object();

        // Base32 编码字符集（Crockford）
        private const string Base32Chars = "0123456789ABCDEFGHJKMNPQRSTVWXYZ";

        // 节点ID（自动生成）
        private static readonly int _nodeId;

        static TSIDUtil()
        {
            // 自动生成节点ID（0-31）
            byte[] nodeIdBytes = new byte[1];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(nodeIdBytes);
            _nodeId = nodeIdBytes[0] & 0x1F;
        }

        #region TSID-256（8字符，32位）

        /// <summary>
        /// 生成 TSID-256（8字符）
        /// </summary>
        /// <returns>8字符的 TSID-256</returns>
        public static string GenerateTsid256()
        {
            var bytes = GenerateTsid256Bytes();
            return EncodeBase32(bytes, 5);
        }

        /// <summary>
        /// 生成 TSID-256 字节数组
        /// </summary>
        /// <returns>4字节的 TSID-256</returns>
        public static byte[] GenerateTsid256Bytes()
        {
            long timestamp = GetCurrentTimestamp();
            int sequence;

            lock (_lock)
            {
                if (timestamp == _lastTimestamp)
                {
                    _sequence = (_sequence + 1) & 0xFF;
                    if (_sequence == 0)
                    {
                        timestamp = WaitForNextTimestamp(_lastTimestamp);
                    }
                }
                else
                {
                    _sequence = 0;
                }
                _lastTimestamp = timestamp;
                sequence = _sequence;
            }

            // 32位：24位时间戳 + 8位序列号
            uint value = ((uint)(timestamp & 0xFFFFFF) << 8) | (uint)sequence;

            return BitConverter.GetBytes(value);
        }

        #endregion

        #region TSID-512（13字符，51位）

        /// <summary>
        /// 生成 TSID-512（13字符）
        /// </summary>
        /// <returns>13字符的 TSID-512</returns>
        public static string GenerateTsid512()
        {
            return GenerateTsid512(_nodeId);
        }

        /// <summary>
        /// 生成 TSID-512（指定节点ID）
        /// </summary>
        /// <param name="nodeId">节点ID（0-31）</param>
        /// <returns>13字符的 TSID-512</returns>
        public static string GenerateTsid512(int nodeId)
        {
            var bytes = GenerateTsid512Bytes(nodeId);
            return EncodeBase32(bytes, 8);
        }

        /// <summary>
        /// 生成 TSID-512 字节数组
        /// </summary>
        /// <param name="nodeId">节点ID（0-31）</param>
        /// <returns>8字节的 TSID-512</returns>
        public static byte[] GenerateTsid512Bytes(int nodeId)
        {
            if (nodeId < 0 || nodeId > 31)
                throw new ArgumentException("Node ID must be between 0 and 31", nameof(nodeId));

            long timestamp = GetCurrentTimestamp();
            int sequence;

            lock (_lock)
            {
                if (timestamp == _lastTimestamp)
                {
                    _sequence = (_sequence + 1) & 0x7FFF;
                    if (_sequence == 0)
                    {
                        timestamp = WaitForNextTimestamp(_lastTimestamp);
                    }
                }
                else
                {
                    _sequence = 0;
                }
                _lastTimestamp = timestamp;
                sequence = _sequence;
            }

            // 使用序列号：42位时间戳 + 5位节点ID + 16位序列号
            ulong value = ((ulong)(timestamp & 0x3FFFFFFFFFF) << 21) |
                          ((ulong)((uint)nodeId & 0x1F) << 16) |
                          (ulong)((uint)sequence & 0xFFFF);

            return BitConverter.GetBytes(value);
        }

        #endregion

        #region TSID-1024（18字符，90位）

        /// <summary>
        /// 生成 TSID-1024（18字符）
        /// </summary>
        /// <returns>18字符的 TSID-1024</returns>
        public static string GenerateTsid1024()
        {
            return GenerateTsid1024(_nodeId);
        }

        /// <summary>
        /// 生成 TSID-1024（指定节点ID）
        /// </summary>
        /// <param name="nodeId">节点ID（0-31）</param>
        /// <returns>18字符的 TSID-1024</returns>
        public static string GenerateTsid1024(int nodeId)
        {
            var bytes = GenerateTsid1024Bytes(nodeId);
            return EncodeBase32(bytes, 12);
        }

        /// <summary>
        /// 生成 TSID-1024 字节数组
        /// </summary>
        /// <param name="nodeId">节点ID（0-31）</param>
        /// <returns>12字节的 TSID-1024</returns>
        public static byte[] GenerateTsid1024Bytes(int nodeId)
        {
            if (nodeId < 0 || nodeId > 31)
                throw new ArgumentException("Node ID must be between 0 and 31", nameof(nodeId));

            long timestamp = GetCurrentTimestamp();
            int sequence;
            byte[] random;

            lock (_lock)
            {
                if (timestamp == _lastTimestamp)
                {
                    _sequence = (_sequence + 1) & 0x3FFFFF;
                    if (_sequence == 0)
                    {
                        timestamp = WaitForNextTimestamp(_lastTimestamp);
                    }
                }
                else
                {
                    _sequence = 0;
                }
                _lastTimestamp = timestamp;
                sequence = _sequence;
            }

            // 生成随机部分
            random = new byte[4];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(random);

            var result = new byte[12];

            // 时间戳（48位，6字节）
            result[0] = (byte)(timestamp >> 40);
            result[1] = (byte)(timestamp >> 32);
            result[2] = (byte)(timestamp >> 24);
            result[3] = (byte)(timestamp >> 16);
            result[4] = (byte)(timestamp >> 8);
            result[5] = (byte)timestamp;

            // 节点ID + 随机（32位，4字节）
            result[6] = (byte)(nodeId << 3 | (random[0] >> 5));
            result[7] = (byte)((random[0] << 3) | (random[1] >> 5));
            result[8] = (byte)((random[1] << 3) | (random[2] >> 5));
            result[9] = (byte)((random[2] << 3) | (random[3] >> 5));
            result[10] = (byte)(random[3] << 3);

            // 序列号（16位，2字节）
            result[10] |= (byte)(sequence >> 13);
            result[11] = (byte)(sequence >> 5);

            return result;
        }

        #endregion

        #region 通用方法

        /// <summary>
        /// 生成默认 TSID（TSID-512）
        /// </summary>
        /// <returns>13字符的 TSID</returns>
        public static string Generate()
        {
            return GenerateTsid512();
        }

        /// <summary>
        /// 从 TSID 提取时间戳
        /// </summary>
        /// <param name="tsid">TSID 字符串</param>
        /// <returns>UTC 时间</returns>
        public static DateTimeOffset ExtractTimestamp(string tsid)
        {
            byte[] bytes = DecodeBase32(tsid);

            // 提取时间戳（前42位）
            long timestamp = 0;
            for (int i = 0; i < Math.Min(6, bytes.Length); i++)
            {
                timestamp = (timestamp << 8) | bytes[i];
            }

            // 根据长度调整
            if (tsid.Length <= 8)
            {
                timestamp = (timestamp >> 8) & 0xFFFFFF;
            }

            return Epoch.AddMilliseconds(timestamp);
        }

        /// <summary>
        /// 验证 TSID 是否有效
        /// </summary>
        /// <param name="tsid">TSID 字符串</param>
        /// <returns>是否有效</returns>
        public static bool IsValid(string tsid)
        {
            if (string.IsNullOrEmpty(tsid))
                return false;

            int len = tsid.Length;
            if (len != 8 && len != 13 && len != 18)
                return false;

            foreach (char c in tsid)
            {
                if (!Base32Chars.Contains(c))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 尝试解析 TSID
        /// </summary>
        /// <param name="tsid">TSID 字符串</param>
        /// <param name="bytes">输出的字节数组</param>
        /// <returns>是否解析成功</returns>
        public static bool TryParse(string tsid, out byte[] bytes)
        {
            bytes = null;
            if (!IsValid(tsid))
                return false;

            try
            {
                bytes = DecodeBase32(tsid);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 批量生成 TSID
        /// </summary>
        /// <param name="count">数量</param>
        /// <returns>TSID 数组</returns>
        public static string[] GenerateBatch(int count)
        {
            if (count <= 0)
                throw new ArgumentException("Count must be greater than 0", nameof(count));

            var result = new string[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = Generate();
            }
            return result;
        }

        /// <summary>
        /// 比较 TSID 的时间顺序
        /// </summary>
        /// <param name="tsid1">第一个 TSID</param>
        /// <param name="tsid2">第二个 TSID</param>
        /// <returns>-1: tsid1早于tsid2, 0: 相同, 1: tsid1晚于tsid2</returns>
        public static int Compare(string tsid1, string tsid2)
        {
            return string.Compare(tsid1, tsid2, StringComparison.Ordinal);
        }

        /// <summary>
        /// 获取或设置节点ID
        /// </summary>
        public static int NodeId => _nodeId;

        #endregion

        #region 私有方法

        private static long GetCurrentTimestamp()
        {
            return (long)(DateTime.UtcNow - Epoch).TotalMilliseconds;
        }

        private static long WaitForNextTimestamp(long lastTimestamp)
        {
            long timestamp = GetCurrentTimestamp();
            while (timestamp <= lastTimestamp)
            {
                Thread.SpinWait(10);
                timestamp = GetCurrentTimestamp();
            }
            return timestamp;
        }

        private static string EncodeBase32(byte[] bytes, int length)
        {
            var result = new StringBuilder(length);

            int bits = 0;
            int value = 0;

            foreach (byte b in bytes)
            {
                value = (value << 8) | b;
                bits += 8;

                while (bits >= 5)
                {
                    result.Append(Base32Chars[(value >> (bits - 5)) & 0x1F]);
                    bits -= 5;
                }
            }

            if (bits > 0)
            {
                result.Append(Base32Chars[(value << (5 - bits)) & 0x1F]);
            }

            return result.ToString().PadLeft(length, '0');
        }

        private static byte[] DecodeBase32(string encoded)
        {
            var result = new List<byte>();

            int bits = 0;
            int value = 0;

            foreach (char c in encoded)
            {
                int index = Base32Chars.IndexOf(char.ToUpperInvariant(c));
                if (index < 0)
                    throw new ArgumentException($"Invalid character: {c}");

                value = (value << 5) | index;
                bits += 5;

                while (bits >= 8)
                {
                    result.Add((byte)((value >> (bits - 8)) & 0xFF));
                    bits -= 8;
                }
            }

            return result.ToArray();
        }

        #endregion
    }
}
