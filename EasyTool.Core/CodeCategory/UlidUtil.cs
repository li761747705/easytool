using System;
using System.Security.Cryptography;
using System.Text;

namespace EasyTool.CodeCategory
{
    /// <summary>
    /// ULID（Universally Unique Lexicographically Sortable Identifier）工具类
    /// ULID 是一个48位时间戳 + 80位随机数的128位唯一标识符
    /// 特点：时间排序、128位兼容UUID、URL安全、大小写不敏感
    /// </summary>
    public static class UlidUtil
    {
        // ULID 时间戳起始时间（1970-01-01）
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Base32 编码字符集（Crockford's Base32）
        private const string EncodingChars = "0123456789ABCDEFGHJKMNPQRSTVWXYZ";

        // Base32 解码字符映射
        private static readonly int[] DecodingMap = BuildDecodingMap();

        /// <summary>
        /// 生成新的 ULID
        /// </summary>
        /// <returns>26个字符的 ULID 字符串</returns>
        public static string Generate()
        {
            return Generate(DateTimeOffset.UtcNow);
        }

        /// <summary>
        /// 基于指定时间生成 ULID
        /// </summary>
        /// <param name="timestamp">时间戳</param>
        /// <returns>26个字符的 ULID 字符串</returns>
        public static string Generate(DateTimeOffset timestamp)
        {
            var bytes = new byte[16];
            WriteTimestamp(bytes, timestamp.ToUniversalTime().ToUnixTimeMilliseconds());
            WriteRandomness(bytes, 6);
            return Encode(bytes);
        }

        /// <summary>
        /// 生成 ULID 字节数组
        /// </summary>
        /// <returns>16字节的 ULID</returns>
        public static byte[] GenerateBytes()
        {
            return GenerateBytes(DateTimeOffset.UtcNow);
        }

        /// <summary>
        /// 基于指定时间生成 ULID 字节数组
        /// </summary>
        /// <param name="timestamp">时间戳</param>
        /// <returns>16字节的 ULID</returns>
        public static byte[] GenerateBytes(DateTimeOffset timestamp)
        {
            var bytes = new byte[16];
            WriteTimestamp(bytes, timestamp.ToUniversalTime().ToUnixTimeMilliseconds());
            WriteRandomness(bytes, 6);
            return bytes;
        }

        /// <summary>
        /// 生成 GUID 格式的 ULID
        /// </summary>
        /// <returns>GUID</returns>
        public static Guid GenerateGuid()
        {
            var bytes = GenerateBytes();
            return new Guid(bytes);
        }

        /// <summary>
        /// 将 ULID 字符串转换为字节数组
        /// </summary>
        /// <param name="ulid">ULID 字符串</param>
        /// <returns>16字节的数组</returns>
        public static byte[] Decode(string ulid)
        {
            if (string.IsNullOrEmpty(ulid))
                throw new ArgumentException("ULID cannot be null or empty", nameof(ulid));
            if (ulid.Length != 26)
                throw new ArgumentException("ULID must be exactly 26 characters", nameof(ulid));

            return DecodeImpl(ulid);
        }

        /// <summary>
        /// 将字节数组编码为 ULID 字符串
        /// </summary>
        /// <param name="bytes">16字节的数组</param>
        /// <returns>26个字符的 ULID 字符串</returns>
        public static string Encode(byte[] bytes)
        {
            if (bytes == null || bytes.Length != 16)
                throw new ArgumentException("Bytes must be exactly 16 bytes", nameof(bytes));

            return EncodeImpl(bytes);
        }

        /// <summary>
        /// 从 ULID 字符串提取时间戳
        /// </summary>
        /// <param name="ulid">ULID 字符串</param>
        /// <returns>UTC 时间</returns>
        public static DateTimeOffset ExtractTimestamp(string ulid)
        {
            var bytes = Decode(ulid);
            return ExtractTimestamp(bytes);
        }

        /// <        /// 从 ULID 字节数组提取时间戳
        /// </summary>
        /// <param name="bytes">16字节的 ULID</param>
        /// <returns>UTC 时间</returns>
        public static DateTimeOffset ExtractTimestamp(byte[] bytes)
        {
            if (bytes == null || bytes.Length != 16)
                throw new ArgumentException("Bytes must be exactly 16 bytes", nameof(bytes));

            long timestamp = ((long)bytes[0] << 40) |
                             ((long)bytes[1] << 32) |
                             ((long)bytes[2] << 24) |
                             ((long)bytes[3] << 16) |
                             ((long)bytes[4] << 8) |
                             bytes[5];

            return DateTimeOffset.FromUnixTimeMilliseconds(timestamp);
        }

        /// <summary>
        /// 验证 ULID 字符串是否有效
        /// </summary>
        /// <param name="ulid">ULID 字符串</param>
        /// <returns>是否有效</returns>
        public static bool IsValid(string ulid)
        {
            if (string.IsNullOrEmpty(ulid) || ulid.Length != 26)
                return false;

            foreach (char c in ulid)
            {
                if (c < '0' || c > 'Z')
                    return false;
                if (c > '9' && c < 'A')
                    return false;
                if (c == 'I' || c == 'L' || c == 'O' || c == 'U')
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 尝试解析 ULID 字符串
        /// </summary>
        /// <param name="ulid">ULID 字符串</param>
        /// <param name="bytes">输出的字节数组</param>
        /// <returns>是否解析成功</returns>
        public static bool TryParse(string ulid, out byte[] bytes)
        {
            bytes = null;
            if (!IsValid(ulid))
                return false;

            try
            {
                bytes = DecodeImpl(ulid);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 比较两个 ULID 的时间顺序
        /// </summary>
        /// <param name="ulid1">第一个 ULID</param>
        /// <param name="ulid2">第二个 ULID</param>
        /// <returns>比较结果：-1表示ulid1早于ulid2，0表示相同，1表示ulid1晚于ulid2</returns>
        public static int Compare(string ulid1, string ulid2)
        {
            return string.Compare(ulid1, ulid2, StringComparison.Ordinal);
        }

        /// <summary>
        /// 生成指定时间范围内的随机 ULID
        /// </summary>
        /// <param name="minTimestamp">最小时间</param>
        /// <param name="maxTimestamp">最大时间</param>
        /// <returns>ULID 字符串</returns>
        public static string GenerateInRange(DateTimeOffset minTimestamp, DateTimeOffset maxTimestamp)
        {
            if (minTimestamp > maxTimestamp)
                throw new ArgumentException("Min timestamp must be less than or equal to max timestamp");

#if NET6_0_OR_GREATER
            var random = Random.Shared;
#else
            var random = new Random(Guid.NewGuid().GetHashCode());
#endif
            long minMs = minTimestamp.ToUniversalTime().ToUnixTimeMilliseconds();
            long maxMs = maxTimestamp.ToUniversalTime().ToUnixTimeMilliseconds();
            long randomMs = minMs + (long)(random.NextDouble() * (maxMs - minMs));

            var bytes = new byte[16];
            WriteTimestamp(bytes, randomMs);
            WriteRandomness(bytes, 6);
            return Encode(bytes);
        }

        /// <summary>
        /// 批量生成 ULID
        /// </summary>
        /// <param name="count">生成数量</param>
        /// <returns>ULID 数组</returns>
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
        /// 将 ULID 转换为小写
        /// </summary>
        /// <param name="ulid">ULID 字符串</param>
        /// <returns>小写的 ULID</returns>
        public static string ToLower(string ulid)
        {
            return ulid?.ToLowerInvariant();
        }

        /// <summary>
        /// 将 ULID 转换为大写
        /// </summary>
        /// <param name="ulid">ULID 字符串</param>
        /// <returns>大写的 ULID</returns>
        public static string ToUpper(string ulid)
        {
            return ulid?.ToUpperInvariant();
        }

        #region 私有方法

        private static int[] BuildDecodingMap()
        {
            var map = new int[256];
            for (int i = 0; i < 256; i++)
            {
                map[i] = -1;
            }
            for (int i = 0; i < EncodingChars.Length; i++)
            {
                map[(byte)EncodingChars[i]] = i;
                map[(byte)char.ToLowerInvariant(EncodingChars[i])] = i;
            }
            // 处理可能出现的歧义字符
            map[(byte)'i'] = 1; map[(byte)'I'] = 1;
            map[(byte)'l'] = 1; map[(byte)'L'] = 1;
            map[(byte)'o'] = 0; map[(byte)'O'] = 0;
            map[(byte)'u'] = 32; map[(byte)'U'] = 32;
            return map;
        }

        private static void WriteTimestamp(byte[] bytes, long timestamp)
        {
            bytes[0] = (byte)(timestamp >> 40);
            bytes[1] = (byte)(timestamp >> 32);
            bytes[2] = (byte)(timestamp >> 24);
            bytes[3] = (byte)(timestamp >> 16);
            bytes[4] = (byte)(timestamp >> 8);
            bytes[5] = (byte)timestamp;
        }

        private static void WriteRandomness(byte[] bytes, int offset)
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes, offset, 10);
            }
        }

        private static string EncodeImpl(byte[] bytes)
        {
            var result = new char[26];

            // 编码时间戳（6字节 -> 10字符）
            result[0] = EncodingChars[(bytes[0] >> 5) & 0x07];
            result[1] = EncodingChars[bytes[0] & 0x1F];
            result[2] = EncodingChars[(bytes[1] >> 3) & 0x1F];
            result[3] = EncodingChars[((bytes[1] << 2) | (bytes[2] >> 6)) & 0x1F];
            result[4] = EncodingChars[(bytes[2] >> 1) & 0x1F];
            result[5] = EncodingChars[((bytes[2] << 4) | (bytes[3] >> 4)) & 0x1F];
            result[6] = EncodingChars[((bytes[3] << 1) | (bytes[4] >> 7)) & 0x1F];
            result[7] = EncodingChars[(bytes[4] >> 2) & 0x1F];
            result[8] = EncodingChars[((bytes[4] << 3) | (bytes[5] >> 5)) & 0x1F];
            result[9] = EncodingChars[bytes[5] & 0x1F];

            // 编码随机数（10字节 -> 16字符）
            result[10] = EncodingChars[(bytes[6] >> 3) & 0x1F];
            result[11] = EncodingChars[((bytes[6] << 2) | (bytes[7] >> 6)) & 0x1F];
            result[12] = EncodingChars[(bytes[7] >> 1) & 0x1F];
            result[13] = EncodingChars[((bytes[7] << 4) | (bytes[8] >> 4)) & 0x1F];
            result[14] = EncodingChars[((bytes[8] << 1) | (bytes[9] >> 7)) & 0x1F];
            result[15] = EncodingChars[(bytes[9] >> 2) & 0x1F];
            result[16] = EncodingChars[((bytes[9] << 3) | (bytes[10] >> 5)) & 0x1F];
            result[17] = EncodingChars[bytes[10] & 0x1F];
            result[18] = EncodingChars[(bytes[11] >> 3) & 0x1F];
            result[19] = EncodingChars[((bytes[11] << 2) | (bytes[12] >> 6)) & 0x1F];
            result[20] = EncodingChars[(bytes[12] >> 1) & 0x1F];
            result[21] = EncodingChars[((bytes[12] << 4) | (bytes[13] >> 4)) & 0x1F];
            result[22] = EncodingChars[((bytes[13] << 1) | (bytes[14] >> 7)) & 0x1F];
            result[23] = EncodingChars[(bytes[14] >> 2) & 0x1F];
            result[24] = EncodingChars[((bytes[14] << 3) | (bytes[15] >> 5)) & 0x1F];
            result[25] = EncodingChars[bytes[15] & 0x1F];

            return new string(result);
        }

        private static byte[] DecodeImpl(string ulid)
        {
            var bytes = new byte[16];

            // 解码时间戳（10字符 -> 6字节）
            bytes[0] = (byte)((DecodingMap[ulid[0]] << 5) | DecodingMap[ulid[1]]);
            bytes[1] = (byte)((DecodingMap[ulid[2]] << 3) | (DecodingMap[ulid[3]] >> 2));
            bytes[2] = (byte)((DecodingMap[ulid[3]] << 6) | (DecodingMap[ulid[4]] << 1) | (DecodingMap[ulid[5]] >> 4));
            bytes[3] = (byte)((DecodingMap[ulid[5]] << 4) | (DecodingMap[ulid[6]] >> 1));
            bytes[4] = (byte)((DecodingMap[ulid[6]] << 7) | (DecodingMap[ulid[7]] << 2) | (DecodingMap[ulid[8]] >> 3));
            bytes[5] = (byte)((DecodingMap[ulid[8]] << 5) | DecodingMap[ulid[9]]);

            // 解码随机数（16字符 -> 10字节）
            bytes[6] = (byte)((DecodingMap[ulid[10]] << 3) | (DecodingMap[ulid[11]] >> 2));
            bytes[7] = (byte)((DecodingMap[ulid[11]] << 6) | (DecodingMap[ulid[12]] << 1) | (DecodingMap[ulid[13]] >> 4));
            bytes[8] = (byte)((DecodingMap[ulid[13]] << 4) | (DecodingMap[ulid[14]] >> 1));
            bytes[9] = (byte)((DecodingMap[ulid[14]] << 7) | (DecodingMap[ulid[15]] << 2) | (DecodingMap[ulid[16]] >> 3));
            bytes[10] = (byte)((DecodingMap[ulid[16]] << 5) | DecodingMap[ulid[17]]);
            bytes[11] = (byte)((DecodingMap[ulid[18]] << 3) | (DecodingMap[ulid[19]] >> 2));
            bytes[12] = (byte)((DecodingMap[ulid[19]] << 6) | (DecodingMap[ulid[20]] << 1) | (DecodingMap[ulid[21]] >> 4));
            bytes[13] = (byte)((DecodingMap[ulid[21]] << 4) | (DecodingMap[ulid[22]] >> 1));
            bytes[14] = (byte)((DecodingMap[ulid[22]] << 7) | (DecodingMap[ulid[23]] << 2) | (DecodingMap[ulid[24]] >> 3));
            bytes[15] = (byte)((DecodingMap[ulid[24]] << 5) | DecodingMap[ulid[25]]);

            return bytes;
        }

        #endregion
    }
}
