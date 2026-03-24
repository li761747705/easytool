using System;
using System.Security.Cryptography;
using System.Text;

namespace EasyTool.CodeCategory
{
    /// <summary>
    /// KSUID（K-Sortable Unique Identifier）工具类
    /// KSUID 是一种时间排序的唯一标识符，由 Svix 开发
    /// 格式：4字节时间戳 + 16字节随机数 = 20字节
    /// </summary>
    public static class KSUIDUtil
    {
        private static readonly DateTime Epoch = new DateTime(2014, 5, 13, 0, 0, 0, DateTimeKind.Utc);
        private const int TimestampBytes = 4;
        private const int PayloadBytes = 16;
        private const int TotalBytes = 20;
        private const int EncodedLength = 27;

        // Base62 字符集
        private const string Base62Chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

        /// <summary>
        /// 生成新的 KSUID
        /// </summary>
        /// <returns>20字节的 KSUID</returns>
        public static byte[] Generate()
        {
            return Generate(DateTimeOffset.UtcNow);
        }

        /// <summary>
        /// 生成指定时间的 KSUID
        /// </summary>
        /// <param name="timestamp">时间戳</param>
        /// <returns>20字节的 KSUID</returns>
        public static byte[] Generate(DateTimeOffset timestamp)
        {
            var bytes = new byte[TotalBytes];

            // 4字节时间戳（大端序，自2014-05-13起的秒数）
            uint seconds = (uint)(timestamp.ToUniversalTime() - Epoch).TotalSeconds;
            bytes[0] = (byte)(seconds >> 24);
            bytes[1] = (byte)(seconds >> 16);
            bytes[2] = (byte)(seconds >> 8);
            bytes[3] = (byte)seconds;

            // 16字节随机载荷
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes, 4, 16);

            return bytes;
        }

        /// <summary>
        /// 生成 KSUID 字符串（27个字符的 Base62 编码）
        /// </summary>
        /// <returns>27字符的 KSUID 字符串</returns>
        public static string GenerateString()
        {
            byte[] bytes = Generate();
            return Encode(bytes);
        }

        /// <summary>
        /// 生成指定时间的 KSUID 字符串
        /// </summary>
        /// <param name="timestamp">时间戳</param>
        /// <returns>27字符的 KSUID 字符串</returns>
        public static string GenerateString(DateTimeOffset timestamp)
        {
            byte[] bytes = Generate(timestamp);
            return Encode(bytes);
        }

        /// <summary>
        /// 将 KSUID 编码为字符串
        /// </summary>
        /// <param name="bytes">20字节的 KSUID</param>
        /// <returns>27字符的 Base62 字符串</returns>
        public static string Encode(byte[] bytes)
        {
            if (bytes == null || bytes.Length != TotalBytes)
                throw new ArgumentException($"KSUID must be {TotalBytes} bytes", nameof(bytes));

            // 转换为大整数
            byte[] paddedBytes = new byte[TotalBytes + 1];
            Array.Copy(bytes, 0, paddedBytes, 1, TotalBytes);

            var number = new System.Numerics.BigInteger(paddedBytes);
            var result = new char[EncodedLength];

            for (int i = EncodedLength - 1; i >= 0; i--)
            {
                number = System.Numerics.BigInteger.DivRem(number, 62, out var remainder);
                result[i] = Base62Chars[(int)remainder];
            }

            return new string(result);
        }

        /// <summary>
        /// 将 KSUID 字符串解码为字节数组
        /// </summary>
        /// <param name="ksuid">27字符的 KSUID 字符串</param>
        /// <returns>20字节的 KSUID</returns>
        public static byte[] Decode(string ksuid)
        {
            if (string.IsNullOrEmpty(ksuid) || ksuid.Length != EncodedLength)
                throw new ArgumentException($"KSUID string must be {EncodedLength} characters", nameof(ksuid));

            // 构建 Base62 解码映射
            var decodeMap = new int[128];
            for (int i = 0; i < 128; i++) decodeMap[i] = -1;
            for (int i = 0; i < Base62Chars.Length; i++)
            {
                decodeMap[Base62Chars[i]] = i;
            }

            // 转换为大整数
            var number = System.Numerics.BigInteger.Zero;

            foreach (char c in ksuid)
            {
                if (c >= 128 || decodeMap[c] < 0)
                    throw new ArgumentException($"Invalid character: {c}");

                number = number * 62 + decodeMap[c];
            }

            // 转换为字节数组
            byte[] allBytes = number.ToByteArray();
            byte[] result = new byte[TotalBytes];

            // 处理可能的符号位
            int copyLength = Math.Min(allBytes.Length, TotalBytes);
            if (allBytes.Length > TotalBytes && allBytes[allBytes.Length - 1] == 0)
            {
                copyLength = Math.Min(allBytes.Length - 1, TotalBytes);
            }

            // 从右侧开始复制（大端序）
            int sourceIndex = allBytes.Length - copyLength;
            if (sourceIndex < 0) sourceIndex = 0;
            int destIndex = TotalBytes - copyLength;
            if (destIndex < 0) destIndex = 0;

            for (int i = 0; i < copyLength && sourceIndex + i < allBytes.Length; i++)
            {
                result[destIndex + i] = allBytes[sourceIndex + i];
            }

            return result;
        }

        /// <summary>
        /// 从 KSUID 提取时间戳
        /// </summary>
        /// <param name="ksuid">KSUID 字节数组</param>
        /// <returns>UTC 时间</returns>
        public static DateTimeOffset ExtractTimestamp(byte[] ksuid)
        {
            if (ksuid == null || ksuid.Length != TotalBytes)
                throw new ArgumentException($"KSUID must be {TotalBytes} bytes", nameof(ksuid));

            uint seconds = ((uint)ksuid[0] << 24) | ((uint)ksuid[1] << 16) |
                           ((uint)ksuid[2] << 8) | ksuid[3];

            return Epoch.AddSeconds(seconds);
        }

        /// <summary>
        /// 从 KSUID 字符串提取时间戳
        /// </summary>
        /// <param name="ksuid">KSUID 字符串</param>
        /// <returns>UTC 时间</returns>
        public static DateTimeOffset ExtractTimestamp(string ksuid)
        {
            byte[] bytes = Decode(ksuid);
            return ExtractTimestamp(bytes);
        }

        /// <summary>
        /// 验证 KSUID 字符串是否有效
        /// </summary>
        /// <param name="ksuid">KSUID 字符串</param>
        /// <returns>是否有效</returns>
        public static bool IsValid(string ksuid)
        {
            if (string.IsNullOrEmpty(ksuid) || ksuid.Length != EncodedLength)
                return false;

            foreach (char c in ksuid)
            {
                if (!Base62Chars.Contains(c))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 尝试解析 KSUID 字符串
        /// </summary>
        /// <param name="ksuid">KSUID 字符串</param>
        /// <param name="bytes">输出的字节数组</param>
        /// <returns>是否解析成功</returns>
        public static bool TryParse(string ksuid, out byte[] bytes)
        {
            bytes = null;
            if (!IsValid(ksuid))
                return false;

            try
            {
                bytes = Decode(ksuid);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 比较 KSUID 的时间顺序
        /// </summary>
        /// <param name="ksuid1">第一个 KSUID</param>
        /// <param name="ksuid2">第二个 KSUID</param>
        /// <returns>-1: ksuid1早于ksuid2, 0: 相同, 1: ksuid1晚于ksuid2</returns>
        public static int Compare(string ksuid1, string ksuid2)
        {
            return string.Compare(ksuid1, ksuid2, StringComparison.Ordinal);
        }

        /// <summary>
        /// 批量生成 KSUID
        /// </summary>
        /// <param name="count">生成数量</param>
        /// <returns>KSUID 字符串数组</returns>
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
        /// 生成指定时间范围内的 KSUID
        /// </summary>
        /// <param name="minTimestamp">最小时间</param>
        /// <param name="maxTimestamp">最大时间</param>
        /// <returns>KSUID 字符串</returns>
        public static string GenerateInRange(DateTimeOffset minTimestamp, DateTimeOffset maxTimestamp)
        {
            if (minTimestamp > maxTimestamp)
                throw new ArgumentException("Min timestamp must be less than or equal to max timestamp");

            var random = new Random();
            long minSeconds = (long)(minTimestamp.ToUniversalTime() - Epoch).TotalSeconds;
            long maxSeconds = (long)(maxTimestamp.ToUniversalTime() - Epoch).TotalSeconds;
            long randomSeconds = minSeconds + (long)(random.NextDouble() * (maxSeconds - minSeconds));

            var timestamp = Epoch.AddSeconds(randomSeconds);
            return GenerateString(timestamp);
        }

        /// <summary>
        /// 获取 KSUID 的最小有效时间
        /// </summary>
        /// <returns>KSUID 纪元时间</returns>
        public static DateTime GetEpoch()
        {
            return Epoch;
        }

        /// <summary>
        /// 解析 KSUID 的各个组成部分
        /// </summary>
        /// <param name="ksuid">KSUID 字符串</param>
        /// <returns>时间戳和载荷</returns>
        public static (DateTimeOffset Timestamp, byte[] Payload) Parse(string ksuid)
        {
            byte[] bytes = Decode(ksuid);
            DateTimeOffset timestamp = ExtractTimestamp(bytes);

            byte[] payload = new byte[PayloadBytes];
            Array.Copy(bytes, 4, payload, 0, PayloadBytes);

            return (timestamp, payload);
        }
    }
}
