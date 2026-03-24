using System;
using System.Security.Cryptography;

namespace EasyTool.CodeCategory
{
    /// <summary>
    /// NanoId 生成器工具类
    /// NanoId 是一个小巧、安全、URL友好的唯一字符串ID生成器
    /// </summary>
    public static class NanoIdUtil
    {
        // 默认字母表（URL安全）
        private const string DefaultAlphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

        // 数字字母表（仅数字）
        private const string NumbersAlphabet = "0123456789";

        // 小写字母表
        private const string LowercaseAlphabet = "abcdefghijklmnopqrstuvwxyz0123456789";

        // 无歧义字符字母表（排除 l, 1, I, O, 0 等）
        private const string NoDoppelgangersAlphabet = "23456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz";

        // 密码安全字母表（包含特殊字符）
        private const string PasswordAlphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz!@#$%^&*()_+-=[]{}|;:,.<>?";

        /// <summary>
        /// 生成默认长度的 NanoId（21位）
        /// </summary>
        /// <returns>NanoId 字符串</returns>
        public static string Generate()
        {
            return Generate(21);
        }

        /// <summary>
        /// 生成指定长度的 NanoId
        /// </summary>
        /// <param name="size">ID长度</param>
        /// <returns>NanoId 字符串</returns>
        public static string Generate(int size)
        {
            return Generate(size, DefaultAlphabet);
        }

        /// <summary>
        /// 生成指定长度和字母表的 NanoId
        /// </summary>
        /// <param name="size">ID长度</param>
        /// <param name="alphabet">自定义字母表</param>
        /// <returns>NanoId 字符串</returns>
        public static string Generate(int size, string alphabet)
        {
            if (size <= 0)
                throw new ArgumentException("Size must be greater than 0", nameof(size));
            if (string.IsNullOrEmpty(alphabet))
                throw new ArgumentException("Alphabet cannot be null or empty", nameof(alphabet));

            return GenerateImpl(size, alphabet);
        }

        /// <summary>
        /// 生成仅数字的 NanoId
        /// </summary>
        /// <param name="size">ID长度（默认21）</param>
        /// <returns>仅数字的 ID</returns>
        public static string GenerateNumbers(int size = 21)
        {
            return Generate(size, NumbersAlphabet);
        }

        /// <summary>
        /// 生成小写字母+数字的 NanoId
        /// </summary>
        /// <param name="size">ID长度（默认21）</param>
        /// <returns>小写字母数字 ID</returns>
        public static string GenerateLowercase(int size = 21)
        {
            return Generate(size, LowercaseAlphabet);
        }

        /// <summary>
        /// 生成无歧义字符的 NanoId（排除 l, 1, I, O, 0 等）
        /// </summary>
        /// <param name="size">ID长度（默认21）</param>
        /// <returns>无歧义字符的 ID</returns>
        public static string GenerateNoDoppelgangers(int size = 21)
        {
            return Generate(size, NoDoppelgangersAlphabet);
        }

        /// <summary>
        /// 生成密码安全的 NanoId（包含特殊字符）
        /// </summary>
        /// <param name="size">ID长度（默认21）</param>
        /// <returns>包含特殊字符的 ID</returns>
        public static string GeneratePassword(int size = 21)
        {
            return Generate(size, PasswordAlphabet);
        }

        /// <summary>
        /// 生成指定长度的自定义 NanoId（使用自定义随机数生成器）
        /// </summary>
        /// <param name="size">ID长度</param>
        /// <param name="alphabet">自定义字母表</param>
        /// <param name="random">自定义随机数生成器</param>
        /// <returns>NanoId 字符串</returns>
        public static string Generate(int size, string alphabet, Random random)
        {
            if (size <= 0)
                throw new ArgumentException("Size must be greater than 0", nameof(size));
            if (string.IsNullOrEmpty(alphabet))
                throw new ArgumentException("Alphabet cannot be null or empty", nameof(alphabet));
            if (random == null)
                throw new ArgumentNullException(nameof(random));

            var chars = new char[size];
            for (int i = 0; i < size; i++)
            {
                chars[i] = alphabet[random.Next(alphabet.Length)];
            }
            return new string(chars);
        }

        /// <summary>
        /// 异步生成 NanoId（适用于大量生成场景）
        /// </summary>
        /// <param name="size">ID长度（默认21）</param>
        /// <returns>NanoId 字符串</returns>
        public static string GenerateAsync(int size = 21)
        {
            return Generate(size);
        }

        /// <summary>
        /// 批量生成 NanoId
        /// </summary>
        /// <param name="count">生成数量</param>
        /// <param name="size">每个ID的长度（默认21）</param>
        /// <returns>NanoId 数组</returns>
        public static string[] GenerateBatch(int count, int size = 21)
        {
            if (count <= 0)
                throw new ArgumentException("Count must be greater than 0", nameof(count));

            var result = new string[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = Generate(size);
            }
            return result;
        }

        #region 私有实现

        private static string GenerateImpl(int size, string alphabet)
        {
            // 计算掩码
            int mask = (2 << (int)Math.Floor(Math.Log(alphabet.Length - 1) / Math.Log(2))) - 1;
            // 计算每个字符需要的平均字节数
            int step = (int)Math.Ceiling(1.6 * mask * size / alphabet.Length);

            var result = new char[size];
            int pos = 0;

            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] buffer = new byte[step];

                while (true)
                {
                    rng.GetBytes(buffer);

                    for (int i = 0; i < step && pos < size; i++)
                    {
                        int index = buffer[i] & mask;

                        if (index < alphabet.Length)
                        {
                            result[pos++] = alphabet[index];
                        }
                    }

                    if (pos >= size)
                    {
                        break;
                    }
                }
            }

            return new string(result);
        }

        #endregion
    }
}
