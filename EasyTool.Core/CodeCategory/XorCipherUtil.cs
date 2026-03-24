using System;
using System.Security.Cryptography;
using System.Text;

namespace EasyTool.CodeCategory
{
    /// <summary>
    /// 异或加密工具类
    /// XOR 加密是一种简单的对称加密
    /// 加密和解密使用相同的操作
    /// 注意：这不是安全的加密方式，仅用于简单混淆
    /// </summary>
    public static class XorCipherUtil
    {
        /// <summary>
        /// 使用单字节密钥进行异或加密/解密
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="key">单字节密钥</param>
        /// <returns>处理后的数据</returns>
        public static byte[] Process(byte[] data, byte key)
        {
            if (data == null || data.Length == 0)
                return Array.Empty<byte>();

            byte[] result = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                result[i] = (byte)(data[i] ^ key);
            }
            return result;
        }

        /// <summary>
        /// 使用字节数组密钥进行异或加密/解密
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="key">密钥</param>
        /// <returns>处理后的数据</returns>
        public static byte[] Process(byte[] data, byte[] key)
        {
            if (data == null || data.Length == 0)
                return Array.Empty<byte>();
            if (key == null || key.Length == 0)
                throw new ArgumentException("Key cannot be empty", nameof(key));

            byte[] result = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                result[i] = (byte)(data[i] ^ key[i % key.Length]);
            }
            return result;
        }

        /// <summary>
        /// 使用字符串密钥进行异或加密/解密
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="key">字符串密钥</param>
        /// <returns>处理后的数据</returns>
        public static byte[] Process(byte[] data, string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Key cannot be empty", nameof(key));

            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            return Process(data, keyBytes);
        }

        /// <summary>
        /// 加密字符串并返回 Base64
        /// </summary>
        /// <param name="text">明文</param>
        /// <param name="key">密钥</param>
        /// <returns>Base64 密文</returns>
        public static string EncryptToBase64(string text, string key)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            byte[] data = Encoding.UTF8.GetBytes(text);
            byte[] encrypted = Process(data, key);
            return Convert.ToBase64String(encrypted);
        }

        /// <summary>
        /// 从 Base64 解密字符串
        /// </summary>
        /// <param name="cipherText">Base64 密文</param>
        /// <param name="key">密钥</param>
        /// <returns>明文</returns>
        public static string DecryptFromBase64(string cipherText, string key)
        {
            if (string.IsNullOrEmpty(cipherText))
                return string.Empty;

            byte[] data = Convert.FromBase64String(cipherText);
            byte[] decrypted = Process(data, key);
            return Encoding.UTF8.GetString(decrypted);
        }

        /// <summary>
        /// 加密字符串并返回十六进制
        /// </summary>
        /// <param name="text">明文</param>
        /// <param name="key">密钥</param>
        /// <returns>十六进制密文</returns>
        public static string EncryptToHex(string text, string key)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            byte[] data = Encoding.UTF8.GetBytes(text);
            byte[] encrypted = Process(data, key);
            return BitConverter.ToString(encrypted).Replace("-", "").ToLower();
        }

        /// <summary>
        /// 从十六进制解密字符串
        /// </summary>
        /// <param name="cipherHex">十六进制密文</param>
        /// <param name="key">密钥</param>
        /// <returns>明文</returns>
        public static string DecryptFromHex(string cipherHex, string key)
        {
            if (string.IsNullOrEmpty(cipherHex))
                return string.Empty;

            byte[] data = HexToBytes(cipherHex);
            byte[] decrypted = Process(data, key);
            return Encoding.UTF8.GetString(decrypted);
        }

        /// <summary>
        /// 生成随机密钥
        /// </summary>
        /// <param name="length">密钥长度</param>
        /// <returns>随机密钥</returns>
        public static byte[] GenerateKey(int length)
        {
            if (length < 1)
                throw new ArgumentException("Key length must be at least 1", nameof(length));

            byte[] key = new byte[length];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(key);
            return key;
        }

        /// <summary>
        /// 生成随机密钥字符串
        /// </summary>
        /// <param name="length">密钥长度</param>
        /// <returns>随机密钥字符串</returns>
        public static string GenerateKeyString(int length)
        {
            byte[] key = GenerateKey(length);
            return Convert.ToBase64String(key);
        }

        private static byte[] HexToBytes(string hex)
        {
            if (hex.Length % 2 != 0)
                hex = "0" + hex;

            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return bytes;
        }
    }
}
