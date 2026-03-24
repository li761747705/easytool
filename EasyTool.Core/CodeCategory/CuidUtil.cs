using System;
using System.Security.Cryptography;
using System.Text;

namespace EasyTool.CodeCategory
{
    /// <summary>
    /// CUID/CUID2 碰撞抵抗ID工具类
    /// CUID 是一种水平可扩展、碰撞抵抗的ID生成方案
    /// CUID2 是更新版本，更安全、更符合标准
    /// </summary>
    public static class CuidUtil
    {
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private static readonly object _lock = new object();
        private static int _counter = 0;
        private static string _fingerprint = null;

        // Base36 字符集
        private const string Base36Chars = "0123456789abcdefghijklmnopqrstuvwxyz";
        private const string Base62Chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

        #region CUID（原始版本）

        /// <summary>
        /// 生成 CUID
        /// </summary>
        /// <returns>25字符的 CUID 字符串</returns>
        public static string GenerateCuid()
        {
            var sb = new StringBuilder(25);

            // 1. 以 'c' 开头
            sb.Append('c');

            // 2. 时间戳（Base36）
            long timestamp = (long)(DateTime.UtcNow - Epoch).TotalMilliseconds;
            sb.Append(ToBase36(timestamp));

            // 3. 计数器（Base36，4字符）
            int counter;
            lock (_lock)
            {
                counter = _counter++;
                if (_counter > 1679615) _counter = 0; // 36^4 - 1
            }
            sb.Append(ToBase36(counter, 4));

            // 4. 指纹（8字符）
            sb.Append(GetFingerprint());

            // 5. 随机字符（4字符）
            sb.Append(RandomBase36(4));

            // 6. 随机字符（4字符）
            sb.Append(RandomBase36(4));

            return sb.ToString();
        }

        /// <summary>
        /// 生成带前缀的 CUID（用于分布式系统）
        /// </summary>
        /// <param name="prefix">前缀（1-4字符）</param>
        /// <returns>带前缀的 CUID</returns>
        public static string GenerateCuid(string prefix)
        {
            if (string.IsNullOrEmpty(prefix))
                return GenerateCuid();

            if (prefix.Length > 4)
                prefix = prefix.Substring(0, 4);

            return prefix + "_" + GenerateCuid().Substring(1);
        }

        /// <summary>
        /// 验证 CUID 是否有效
        /// </summary>
        /// <param name="cuid">CUID 字符串</param>
        /// <returns>是否有效</returns>
        public static bool IsValidCuid(string cuid)
        {
            if (string.IsNullOrEmpty(cuid) || cuid.Length < 25)
                return false;

            // 检查是否以 'c' 开头或带前缀
            if (cuid[0] != 'c' && !cuid.Contains("_"))
                return false;

            // 检查字符是否有效
            foreach (char c in cuid)
            {
                if (c == '_') continue;
                if (!Base36Chars.Contains(char.ToLowerInvariant(c)))
                    return false;
            }

            return true;
        }

        #endregion

        #region CUID2

        /// <summary>
        /// 生成 CUID2（更安全）
        /// </summary>
        /// <returns>24字符的 CUID2</returns>
        public static string GenerateCuid2()
        {
            return GenerateCuid2(24);
        }

        /// <summary>
        /// 生成指定长度的 CUID2
        /// </summary>
        /// <param name="length">长度（2-32）</param>
        /// <returns>CUID2 字符串</returns>
        public static string GenerateCuid2(int length)
        {
            if (length < 2 || length > 32)
                throw new ArgumentException("Length must be between 2 and 32", nameof(length));

            // 第一部分：时间戳 + 计数器 + 指纹的哈希
            long timestamp = (long)(DateTime.UtcNow - Epoch).TotalMilliseconds;

            int counter;
            lock (_lock)
            {
                counter = _counter++;
            }

            string entropy = RandomBase36(32);
            string fingerprint = GetFingerprint();

            string input = $"{timestamp}{counter}{fingerprint}{entropy}";

            // 使用 SHA3 或 SHA256 哈希
            byte[] hash;
            using (var sha256 = SHA256.Create())
            {
                hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            }

            // 转换为 Base62
            string result = ToBase62(hash).Substring(0, length);

            return result;
        }

        /// <summary>
        /// 生成带熵的 CUID2
        /// </summary>
        /// <param name="length">长度</param>
        /// <param name="entropy">额外熵值</param>
        /// <returns>CUID2 字符串</returns>
        public static string GenerateCuid2(int length, string entropy)
        {
            if (length < 2 || length > 32)
                throw new ArgumentException("Length must be between 2 and 32", nameof(length));

            long timestamp = (long)(DateTime.UtcNow - Epoch).TotalMilliseconds;

            int counter;
            lock (_lock)
            {
                counter = _counter++;
            }

            string fingerprint = GetFingerprint();
            string randomEntropy = RandomBase36(32);

            string input = $"{timestamp}{counter}{fingerprint}{entropy}{randomEntropy}";

            byte[] hash;
            using (var sha256 = SHA256.Create())
            {
                hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            }

            string result = ToBase62(hash).Substring(0, length);

            return result;
        }

        /// <summary>
        /// 验证 CUID2 是否有效
        /// </summary>
        /// <param name="cuid2">CUID2 字符串</param>
        /// <returns>是否有效</returns>
        public static bool IsValidCuid2(string cuid2)
        {
            if (string.IsNullOrEmpty(cuid2) || cuid2.Length < 2 || cuid2.Length > 32)
                return false;

            foreach (char c in cuid2)
            {
                if (!Base62Chars.Contains(c))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 判断是 CUID 还是 CUID2
        /// </summary>
        /// <param name="cuid">CUID 字符串</param>
        /// <returns>CUID 类型</returns>
        public static CuidType GetCuidType(string cuid)
        {
            if (string.IsNullOrEmpty(cuid))
                return CuidType.Invalid;

            if (cuid.Length == 24 && IsValidCuid2(cuid))
                return CuidType.CUID2;

            if (cuid.Length >= 25 && (cuid[0] == 'c' || cuid.Contains("_")))
                return CuidType.CUID;

            return CuidType.Invalid;
        }

        #endregion

        #region Slug（短版本）

        /// <summary>
        /// 生成 Slug ID（更短的唯一ID）
        /// </summary>
        /// <returns>7-10字符的 Slug</returns>
        public static string GenerateSlug()
        {
            long timestamp = (long)(DateTime.UtcNow - Epoch).TotalMilliseconds;
            string random = RandomBase36(4);
            string counter = ToBase36(Environment.CurrentManagedThreadId % 1296, 2);

            return ToBase36(timestamp).Substring(5) + counter + random;
        }

        /// <summary>
        /// 验证 Slug 是否有效
        /// </summary>
        /// <param name="slug">Slug 字符串</param>
        /// <returns>是否有效</returns>
        public static bool IsValidSlug(string slug)
        {
            if (string.IsNullOrEmpty(slug) || slug.Length < 7 || slug.Length > 10)
                return false;

            foreach (char c in slug)
            {
                if (!Base36Chars.Contains(char.ToLowerInvariant(c)))
                    return false;
            }

            return true;
        }

        #endregion

        #region 批量生成

        /// <summary>
        /// 批量生成 CUID
        /// </summary>
        /// <param name="count">数量</param>
        /// <returns>CUID 数组</returns>
        public static string[] GenerateCuidBatch(int count)
        {
            if (count <= 0)
                throw new ArgumentException("Count must be greater than 0", nameof(count));

            var result = new string[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = GenerateCuid();
            }
            return result;
        }

        /// <summary>
        /// 批量生成 CUID2
        /// </summary>
        /// <param name="count">数量</param>
        /// <param name="length">每个 CUID2 的长度</param>
        /// <returns>CUID2 数组</returns>
        public static string[] GenerateCuid2Batch(int count, int length = 24)
        {
            if (count <= 0)
                throw new ArgumentException("Count must be greater than 0", nameof(count));

            var result = new string[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = GenerateCuid2(length);
            }
            return result;
        }

        #endregion

        #region 私有方法

        private static string GetFingerprint()
        {
            if (_fingerprint != null)
                return _fingerprint;

            var sb = new StringBuilder();

            // 机器标识
            sb.Append(Environment.MachineName.GetHashCode());

            // 进程ID
            sb.Append(Environment.CurrentManagedThreadId);

            // 随机部分
            sb.Append(new Random().Next(1000));

            _fingerprint = ToBase36(Math.Abs(sb.ToString().GetHashCode()), 8);

            return _fingerprint;
        }

        private static string ToBase36(long value, int padLength = 0)
        {
            if (value < 0) value = -value;

            var result = new StringBuilder();
            while (value > 0)
            {
                result.Insert(0, Base36Chars[(int)(value % 36)]);
                value /= 36;
            }

            if (padLength > 0 && result.Length < padLength)
            {
                result.Insert(0, new string('0', padLength - result.Length));
            }

            return result.ToString();
        }

        private static string ToBase62(byte[] bytes)
        {
            // 将字节数组转换为大整数，然后转换为 Base62
            var result = new StringBuilder();

            // 简化处理：直接使用字节的值
            for (int i = 0; i < bytes.Length; i++)
            {
                int value = bytes[i] % 62;
                result.Append(Base62Chars[value]);
            }

            return result.ToString();
        }

        private static string RandomBase36(int length)
        {
            var result = new StringBuilder(length);
            byte[] randomBytes = new byte[length];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);

            foreach (byte b in randomBytes)
            {
                result.Append(Base36Chars[b % 36]);
            }

            return result.ToString();
        }

        #endregion
    }

    /// <summary>
    /// CUID 类型
    /// </summary>
    public enum CuidType
    {
        /// <summary>
        /// 无效
        /// </summary>
        Invalid,

        /// <summary>
        /// 原始 CUID
        /// </summary>
        CUID,

        /// <summary>
        /// CUID2
        /// </summary>
        CUID2
    }
}
