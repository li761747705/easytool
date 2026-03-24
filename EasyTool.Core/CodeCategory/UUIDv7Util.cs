using System;
using System.Security.Cryptography;
using System.Text;

namespace EasyTool.CodeCategory
{
    /// <summary>
    /// UUID v7 工具类
    /// UUID v7 是一种时间排序的 UUID，使用 Unix 时间戳（毫秒）
    /// 格式：48位时间戳 + 4位版本 + 12位随机 + 2位变体 + 62位随机
    /// 兼容 RFC 9562 标准
    /// </summary>
    public static class UUIDv7Util
    {
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private static readonly object _lock = new object();
        private static long _lastTimestamp = -1;
        private static byte[] _lastRandom = new byte[8];
        private static int _sequence = 0;

        /// <summary>
        /// 生成 UUID v7
        /// </summary>
        /// <returns>UUID 字节数组（16字节）</returns>
        public static byte[] Generate()
        {
            return Generate(DateTimeOffset.UtcNow);
        }

        /// <summary>
        /// 生成指定时间的 UUID v7
        /// </summary>
        /// <param name="timestamp">时间戳</param>
        /// <returns>UUID 字节数组（16字节）</returns>
        public static byte[] Generate(DateTimeOffset timestamp)
        {
            long unixMs = (long)(timestamp.ToUniversalTime() - UnixEpoch).TotalMilliseconds;

            byte[] uuid = new byte[16];
            using var rng = RandomNumberGenerator.Create();

            lock (_lock)
            {
                // 48位时间戳（大端序）
                uuid[0] = (byte)(unixMs >> 40);
                uuid[1] = (byte)(unixMs >> 32);
                uuid[2] = (byte)(unixMs >> 24);
                uuid[3] = (byte)(unixMs >> 16);
                uuid[4] = (byte)(unixMs >> 8);
                uuid[5] = (byte)unixMs;

                if (unixMs == _lastTimestamp)
                {
                    // 同一毫秒内递增序列
                    _sequence++;
                    if (_sequence > 0xFFF)
                    {
                        // 序列溢出，等待下一毫秒
                        unixMs = WaitForNextMs(unixMs);
                        _sequence = 0;

                        uuid[4] = (byte)(unixMs >> 8);
                        uuid[5] = (byte)unixMs;
                    }

                    // 使用递增的序列
                    uuid[6] = (byte)((0x70 | ((_sequence >> 8) & 0x0F))); // 版本 7
                    uuid[7] = (byte)_sequence;
                }
                else
                {
                    _sequence = 0;
                    _lastTimestamp = unixMs;

                    // 新的随机部分
                    rng.GetBytes(_lastRandom);

                    uuid[6] = (byte)((_lastRandom[0] & 0x0F) | 0x70); // 版本 7
                    uuid[7] = _lastRandom[1];
                }

                // 随机部分（62位）
                rng.GetBytes(uuid, 8, 8);

                // 设置变体（10xx）
                uuid[8] = (byte)((uuid[8] & 0x3F) | 0x80);
            }

            return uuid;
        }

        /// <summary>
        /// 生成 UUID v7 字符串
        /// </summary>
        /// <returns>36字符的 UUID 字符串</returns>
        public static string GenerateString()
        {
            byte[] uuid = Generate();
            return Format(uuid);
        }

        /// <summary>
        /// 生成不带连字符的 UUID v7 字符串
        /// </summary>
        /// <returns>32字符的 UUID 字符串</returns>
        public static string GenerateStringNoHyphens()
        {
            byte[] uuid = Generate();
            return BitConverter.ToString(uuid).Replace("-", "").ToLower();
        }

        /// <summary>
        /// 批量生成 UUID v7
        /// </summary>
        /// <param name="count">数量</param>
        /// <returns>UUID 字符串数组</returns>
        public static string[] GenerateBatch(int count)
        {
            if (count <= 0)
                throw new ArgumentException("Count must be greater than 0", nameof(count));

            var result = new string[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = GenerateString();
            }
            return result;
        }

        /// <summary>
        /// 从 UUID v7 提取时间戳
        /// </summary>
        /// <param name="uuid">UUID 字节数组或字符串</param>
        /// <returns>UTC 时间</returns>
        public static DateTimeOffset ExtractTimestamp(byte[] uuid)
        {
            if (uuid == null || uuid.Length != 16)
                throw new ArgumentException("UUID must be 16 bytes", nameof(uuid));

            // 提取 48 位时间戳
            long unixMs = ((long)uuid[0] << 40) | ((long)uuid[1] << 32) |
                          ((long)uuid[2] << 24) | ((long)uuid[3] << 16) |
                          ((long)uuid[4] << 8) | uuid[5];

            return UnixEpoch.AddMilliseconds(unixMs);
        }

        /// <summary>
        /// 从 UUID v7 字符串提取时间戳
        /// </summary>
        /// <param name="uuid">UUID 字符串</param>
        /// <returns>UTC 时间</returns>
        public static DateTimeOffset ExtractTimestamp(string uuid)
        {
            byte[] bytes = Parse(uuid);
            return ExtractTimestamp(bytes);
        }

        /// <summary>
        /// 验证 UUID v7 是否有效
        /// </summary>
        /// <param name="uuid">UUID 字符串</param>
        /// <returns>是否有效</returns>
        public static bool IsValid(string uuid)
        {
            if (string.IsNullOrEmpty(uuid))
                return false;

            // 移除连字符
            string clean = uuid.Replace("-", "");
            if (clean.Length != 32)
                return false;

            // 检查字符
            foreach (char c in clean)
            {
                if (!Uri.IsHexDigit(c))
                    return false;
            }

            // 检查版本
            byte version = Convert.ToByte(clean.Substring(12, 2), 16);
            if ((version & 0xF0) != 0x70)
                return false;

            // 检查变体
            byte variant = Convert.ToByte(clean.Substring(16, 2), 16);
            if ((variant & 0xC0) != 0x80)
                return false;

            return true;
        }

        /// <summary>
        /// 验证 UUID v7 字节数组是否有效
        /// </summary>
        /// <param name="uuid">UUID 字节数组</param>
        /// <returns>是否有效</returns>
        public static bool IsValid(byte[] uuid)
        {
            if (uuid == null || uuid.Length != 16)
                return false;

            // 检查版本（必须是 7）
            if ((uuid[6] & 0xF0) != 0x70)
                return false;

            // 检查变体（必须是 10xx）
            if ((uuid[8] & 0xC0) != 0x80)
                return false;

            return true;
        }

        /// <summary>
        /// 解析 UUID 字符串为字节数组
        /// </summary>
        /// <param name="uuid">UUID 字符串</param>
        /// <returns>16字节数组</returns>
        public static byte[] Parse(string uuid)
        {
            if (string.IsNullOrEmpty(uuid))
                throw new ArgumentException("UUID cannot be empty", nameof(uuid));

            string clean = uuid.Replace("-", "");
            if (clean.Length != 32)
                throw new ArgumentException("Invalid UUID format", nameof(uuid));

            byte[] bytes = new byte[16];
            for (int i = 0; i < 16; i++)
            {
                bytes[i] = Convert.ToByte(clean.Substring(i * 2, 2), 16);
            }

            return bytes;
        }

        /// <summary>
        /// 格式化 UUID 字节数组为字符串
        /// </summary>
        /// <param name="uuid">UUID 字节数组</param>
        /// <returns>36字符的 UUID 字符串</returns>
        public static string Format(byte[] uuid)
        {
            if (uuid == null || uuid.Length != 16)
                throw new ArgumentException("UUID must be 16 bytes", nameof(uuid));

            return $"{uuid[0]:x2}{uuid[1]:x2}{uuid[2]:x2}{uuid[3]:x2}-" +
                   $"{uuid[4]:x2}{uuid[5]:x2}-" +
                   $"{uuid[6]:x2}{uuid[7]:x2}-" +
                   $"{uuid[8]:x2}{uuid[9]:x2}-" +
                   $"{uuid[10]:x2}{uuid[11]:x2}{uuid[12]:x2}{uuid[13]:x2}{uuid[14]:x2}{uuid[15]:x2}";
        }

        /// <summary>
        /// 比较 UUID v7 的时间顺序
        /// </summary>
        /// <param name="uuid1">第一个 UUID</param>
        /// <param name="uuid2">第二个 UUID</param>
        /// <returns>-1: uuid1早于uuid2, 0: 相同, 1: uuid1晚于uuid2</returns>
        public static int Compare(string uuid1, string uuid2)
        {
            byte[] bytes1 = Parse(uuid1);
            byte[] bytes2 = Parse(uuid2);

            // 比较前 6 字节（时间戳）
            for (int i = 0; i < 6; i++)
            {
                if (bytes1[i] < bytes2[i]) return -1;
                if (bytes1[i] > bytes2[i]) return 1;
            }

            // 比较序列部分
            for (int i = 6; i < 16; i++)
            {
                if (bytes1[i] < bytes2[i]) return -1;
                if (bytes1[i] > bytes2[i]) return 1;
            }

            return 0;
        }

        private static long WaitForNextMs(long lastTimestamp)
        {
            long timestamp = (long)(DateTimeOffset.UtcNow - UnixEpoch).TotalMilliseconds;
            while (timestamp <= lastTimestamp)
            {
                timestamp = (long)(DateTimeOffset.UtcNow - UnixEpoch).TotalMilliseconds;
            }
            return timestamp;
        }
    }
}
