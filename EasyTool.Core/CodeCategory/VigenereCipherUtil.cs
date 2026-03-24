using System;
using System.Text;

namespace EasyTool.CodeCategory
{
    /// <summary>
    /// 维吉尼亚密码工具类
    /// 维吉尼亚密码是一种多表替换密码
    /// 使用关键词进行加密，比凯撒密码更安全
    /// </summary>
    public static class VigenereCipherUtil
    {
        /// <summary>
        /// 使用维吉尼亚密码加密
        /// </summary>
        /// <param name="text">明文</param>
        /// <param name="key">密钥</param>
        /// <returns>密文</returns>
        public static string Encrypt(string text, string key)
        {
            return Process(text, key, true);
        }

        /// <summary>
        /// 使用维吉尼亚密码解密
        /// </summary>
        /// <param name="text">密文</param>
        /// <param name="key">密钥</param>
        /// <returns>明文</returns>
        public static string Decrypt(string text, string key)
        {
            return Process(text, key, false);
        }

        private static string Process(string text, string key, bool encrypt)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Key cannot be empty", nameof(key));

            // 准备密钥（只保留字母）
            var cleanKey = new StringBuilder();
            foreach (char c in key)
            {
                if (char.IsLetter(c))
                    cleanKey.Append(char.ToUpperInvariant(c));
            }

            if (cleanKey.Length == 0)
                throw new ArgumentException("Key must contain at least one letter", nameof(key));

            var result = new StringBuilder(text.Length);
            int keyIndex = 0;

            foreach (char c in text)
            {
                if (char.IsLetter(c))
                {
                    char baseChar = char.IsUpper(c) ? 'A' : 'a';
                    int textValue = c - baseChar;
                    int keyValue = cleanKey[keyIndex % cleanKey.Length] - 'A';

                    int resultValue;
                    if (encrypt)
                    {
                        resultValue = (textValue + keyValue) % 26;
                    }
                    else
                    {
                        resultValue = (textValue - keyValue + 26) % 26;
                    }

                    result.Append((char)(baseChar + resultValue));
                    keyIndex++;
                }
                else
                {
                    result.Append(c);
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// 生成随机密钥
        /// </summary>
        /// <param name="length">密钥长度</param>
        /// <returns>随机密钥</returns>
        public static string GenerateKey(int length)
        {
            if (length < 1)
                throw new ArgumentException("Key length must be at least 1", nameof(length));

            var random = new Random();
            var key = new StringBuilder(length);

            for (int i = 0; i < length; i++)
            {
                key.Append((char)('A' + random.Next(26)));
            }

            return key.ToString();
        }
    }
}
