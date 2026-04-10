using System;
using System.Security.Cryptography;
using System.Text;

namespace EasyTool.CodeCategory
{
    /// <summary>
    /// TypeID 工具类
    /// TypeID 是一种类型化的唯一标识符，将类型前缀与 UUIDv7 结合
    /// 格式：{prefix}_{base32-encoded-uuidv7}
    /// 例如：user_01ARZ3NDEKTSV4RRFFQ69G5FAV
    /// </summary>
    public static class TypeIdUtil
    {
        private const string Base32Chars = "0123456789ABCDEFGHJKMNPQRSTVWXYZ";
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// 生成带类型前缀的 TypeID
        /// </summary>
        /// <param name="prefix">类型前缀（小写字母，1-63字符）</param>
        /// <returns>TypeID 字符串</returns>
        public static string Generate(string prefix)
        {
            ValidatePrefix(prefix);
            byte[] uuid = GenerateUUIDv7();
            string encoded = EncodeBase32(uuid);
            return $"{prefix}_{encoded}";
        }

        /// <summary>
        /// 生成不带前缀的 TypeID（仅 UUIDv7 的 Base32 编码）
        /// </summary>
        /// <returns>TypeID 字符串</returns>
        public static string Generate()
        {
            byte[] uuid = GenerateUUIDv7();
            return EncodeBase32(uuid);
        }

        /// <summary>
        /// 从 TypeID 提取前缀
        /// </summary>
        /// <param name="typeId">TypeID 字符串</param>
        /// <returns>类型前缀</returns>
        public static string ExtractPrefix(string typeId)
        {
            if (string.IsNullOrEmpty(typeId))
                throw new ArgumentException("TypeID cannot be empty", nameof(typeId));

            int separatorIndex = typeId.IndexOf('_');
            if (separatorIndex < 0)
                return string.Empty;

            return typeId.Substring(0, separatorIndex);
        }

        /// <summary>
        /// 从 TypeID 提取 UUID 字节数组
        /// </summary>
        /// <param name="typeId">TypeID 字符串</param>
        /// <returns>16字节 UUID</returns>
        public static byte[] ExtractUUID(string typeId)
        {
            if (string.IsNullOrEmpty(typeId))
                throw new ArgumentException("TypeID cannot be empty", nameof(typeId));

            int separatorIndex = typeId.IndexOf('_');
            string encoded = separatorIndex >= 0 ? typeId.Substring(separatorIndex + 1) : typeId;

            return DecodeBase32(encoded);
        }

        /// <summary>
        /// 从 TypeID 提取时间戳
        /// </summary>
        /// <param name="typeId">TypeID 字符串</param>
        /// <returns>UTC 时间</returns>
        public static DateTimeOffset ExtractTimestamp(string typeId)
        {
            byte[] uuid = ExtractUUID(typeId);

            // 提取 48 位时间戳（前 6 字节）
            long unixMs = ((long)uuid[0] << 40) | ((long)uuid[1] << 32) |
                          ((long)uuid[2] << 24) | ((long)uuid[3] << 16) |
                          ((long)uuid[4] << 8) | uuid[5];

            return UnixEpoch.AddMilliseconds(unixMs);
        }

        /// <summary>
        /// 验证 TypeID 格式是否有效
        /// </summary>
        /// <param name="typeId">TypeID 字符串</param>
        /// <returns>是否有效</returns>
        public static bool IsValid(string typeId)
        {
            if (string.IsNullOrEmpty(typeId))
                return false;

            int separatorIndex = typeId.IndexOf('_');

            if (separatorIndex < 0)
            {
                // 无前缀，仅检查 Base32 编码
                return IsValidBase32(typeId) && typeId.Length == 26;
            }

            string prefix = typeId.Substring(0, separatorIndex);
            string encoded = typeId.Substring(separatorIndex + 1);

            return IsValidPrefix(prefix) && IsValidBase32(encoded) && encoded.Length == 26;
        }

        /// <summary>
        /// 解析 TypeID
        /// </summary>
        /// <param name="typeId">TypeID 字符串</param>
        /// <returns>前缀和 UUID</returns>
        public static (string Prefix, byte[] UUID) Parse(string typeId)
        {
            if (!IsValid(typeId))
                throw new ArgumentException("Invalid TypeID format", nameof(typeId));

            string prefix = ExtractPrefix(typeId);
            byte[] uuid = ExtractUUID(typeId);

            return (prefix, uuid);
        }

        /// <summary>
        /// 从 UUID 和前缀创建 TypeID
        /// </summary>
        /// <param name="prefix">类型前缀</param>
        /// <param name="uuid">16字节 UUID</param>
        /// <returns>TypeID 字符串</returns>
        public static string FromUUID(string prefix, byte[] uuid)
        {
            if (uuid == null || uuid.Length != 16)
                throw new ArgumentException("UUID must be 16 bytes", nameof(uuid));

            if (!string.IsNullOrEmpty(prefix))
            {
                ValidatePrefix(prefix);
                return $"{prefix}_{EncodeBase32(uuid)}";
            }

            return EncodeBase32(uuid);
        }

        #region 私有方法

        private static byte[] GenerateUUIDv7()
        {
            byte[] uuid = new byte[16];
            using var rng = RandomNumberGenerator.Create();

            long unixMs = (long)(DateTimeOffset.UtcNow - UnixEpoch).TotalMilliseconds;

            // 48位时间戳
            uuid[0] = (byte)(unixMs >> 40);
            uuid[1] = (byte)(unixMs >> 32);
            uuid[2] = (byte)(unixMs >> 24);
            uuid[3] = (byte)(unixMs >> 16);
            uuid[4] = (byte)(unixMs >> 8);
            uuid[5] = (byte)unixMs;

            // 随机部分
            rng.GetBytes(uuid, 6, 10);

            // 设置版本 (7) 和变体
            uuid[6] = (byte)((uuid[6] & 0x0F) | 0x70);
            uuid[8] = (byte)((uuid[8] & 0x3F) | 0x80);

            return uuid;
        }

        private static string EncodeBase32(byte[] data)
        {
            var result = new StringBuilder(26);

            // 将 16 字节转换为 26 个 Base32 字符
            // 128 位 = 26 * 5 位 - 2 位（最后 2 位忽略）
            ulong high = ((ulong)data[0] << 56) | ((ulong)data[1] << 48) |
                         ((ulong)data[2] << 40) | ((ulong)data[3] << 32) |
                         ((ulong)data[4] << 24) | ((ulong)data[5] << 16) |
                         ((ulong)data[6] << 8) | data[7];

            ulong low = ((ulong)data[8] << 56) | ((ulong)data[9] << 48) |
                        ((ulong)data[10] << 40) | ((ulong)data[11] << 32) |
                        ((ulong)data[12] << 24) | ((ulong)data[13] << 16) |
                        ((ulong)data[14] << 8) | data[15];

            result.Append(Base32Chars[(int)((high >> 59) & 0x1F)]);
            result.Append(Base32Chars[(int)((high >> 54) & 0x1F)]);
            result.Append(Base32Chars[(int)((high >> 49) & 0x1F)]);
            result.Append(Base32Chars[(int)((high >> 44) & 0x1F)]);
            result.Append(Base32Chars[(int)((high >> 39) & 0x1F)]);
            result.Append(Base32Chars[(int)((high >> 34) & 0x1F)]);
            result.Append(Base32Chars[(int)((high >> 29) & 0x1F)]);
            result.Append(Base32Chars[(int)((high >> 24) & 0x1F)]);
            result.Append(Base32Chars[(int)((high >> 19) & 0x1F)]);
            result.Append(Base32Chars[(int)((high >> 14) & 0x1F)]);
            result.Append(Base32Chars[(int)((high >> 9) & 0x1F)]);
            result.Append(Base32Chars[(int)((high >> 4) & 0x1F)]);
            result.Append(Base32Chars[(int)(((high << 1) | (low >> 63)) & 0x1F)]);
            result.Append(Base32Chars[(int)((low >> 58) & 0x1F)]);
            result.Append(Base32Chars[(int)((low >> 53) & 0x1F)]);
            result.Append(Base32Chars[(int)((low >> 48) & 0x1F)]);
            result.Append(Base32Chars[(int)((low >> 43) & 0x1F)]);
            result.Append(Base32Chars[(int)((low >> 38) & 0x1F)]);
            result.Append(Base32Chars[(int)((low >> 33) & 0x1F)]);
            result.Append(Base32Chars[(int)((low >> 28) & 0x1F)]);
            result.Append(Base32Chars[(int)((low >> 23) & 0x1F)]);
            result.Append(Base32Chars[(int)((low >> 18) & 0x1F)]);
            result.Append(Base32Chars[(int)((low >> 13) & 0x1F)]);
            result.Append(Base32Chars[(int)((low >> 8) & 0x1F)]);
            result.Append(Base32Chars[(int)((low >> 3) & 0x1F)]);
            result.Append(Base32Chars[(int)((low << 2) & 0x1F)]);

            return result.ToString();
        }

        private static byte[] DecodeBase32(string encoded)
        {
            if (encoded.Length != 26)
                throw new ArgumentException("Encoded string must be 26 characters", nameof(encoded));

            // 构建解码映射
            var decodeMap = new int[128];
            for (int i = 0; i < 128; i++) decodeMap[i] = -1;
            for (int i = 0; i < Base32Chars.Length; i++)
            {
                decodeMap[Base32Chars[i]] = i;
                decodeMap[char.ToLowerInvariant(Base32Chars[i])] = i;
            }

            // 解码每个字符
            int[] values = new int[26];
            for (int i = 0; i < 26; i++)
            {
                char c = encoded[i];
                if (c >= 128 || decodeMap[c] < 0)
                    throw new ArgumentException($"Invalid Base32 character: {c}", nameof(encoded));
                values[i] = decodeMap[c];
            }

            // 重组为 16 字节
            byte[] result = new byte[16];

            // 使用 BigInteger 进行重组
            System.Numerics.BigInteger bigInt = 0;
            for (int i = 0; i < 26; i++)
            {
                bigInt = (bigInt << 5) | values[i];
            }

            byte[] bytes = bigInt.ToByteArray();
            int copyLength = Math.Min(bytes.Length, 16);
            int startIdx = bytes.Length > 16 ? bytes.Length - 16 : 0;

            for (int i = 0; i < copyLength; i++)
            {
                result[16 - copyLength + i] = bytes[startIdx + i];
            }

            return result;
        }

        private static void ValidatePrefix(string prefix)
        {
            if (string.IsNullOrEmpty(prefix))
                throw new ArgumentException("Prefix cannot be empty", nameof(prefix));

            if (prefix.Length > 63)
                throw new ArgumentException("Prefix must be at most 63 characters", nameof(prefix));

            foreach (char c in prefix)
            {
                if (c < 'a' || c > 'z')
                    throw new ArgumentException("Prefix must contain only lowercase letters", nameof(prefix));
            }
        }

        private static bool IsValidPrefix(string prefix)
        {
            if (string.IsNullOrEmpty(prefix) || prefix.Length > 63)
                return false;

            foreach (char c in prefix)
            {
                if (c < 'a' || c > 'z')
                    return false;
            }

            return true;
        }

        private static bool IsValidBase32(string encoded)
        {
            if (string.IsNullOrEmpty(encoded))
                return false;

            foreach (char c in encoded)
            {
                if (!Base32Chars.Contains(c) && !Base32Chars.Contains(char.ToUpperInvariant(c)))
                    return false;
            }

            return true;
        }

        #endregion
    }
}
