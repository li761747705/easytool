using System;

namespace EasyTool.CodeCategory
{
    /// <summary>
    /// RC4 流加密工具类
    /// RC4 是一种广泛使用的流密码，由 Ron Rivest 设计
    /// 注意：RC4 已被认为不安全，建议使用 ChaCha20 替代
    /// 保留用于兼容旧系统
    /// </summary>
    public static class RC4Util
    {
        /// <summary>
        /// 使用 RC4 加密/解密数据（对称操作）
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <param name="key">密钥（1-256字节）</param>
        /// <returns>加密/解密后的数据</returns>
        public static byte[] Process(byte[] data, byte[] key)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (key == null || key.Length < 1 || key.Length > 256)
                throw new ArgumentException("Key must be between 1 and 256 bytes", nameof(key));

            return Process(data, 0, data.Length, key);
        }

        /// <summary>
        /// 使用 RC4 加密/解密数据
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <param name="offset">起始偏移</param>
        /// <param name="length">数据长度</param>
        /// <param name="key">密钥</param>
        /// <returns>加密/解密后的数据</returns>
        public static byte[] Process(byte[] data, int offset, int length, byte[] key)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (key == null || key.Length < 1 || key.Length > 256)
                throw new ArgumentException("Key must be between 1 and 256 bytes", nameof(key));
            if (offset < 0 || offset > data.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (length < 0 || offset + length > data.Length)
                throw new ArgumentOutOfRangeException(nameof(length));

            byte[] result = new byte[length];
            byte[] s = new byte[256];
            byte[] k = new byte[256];

            // 密钥调度算法（KSA）
            for (int i = 0; i < 256; i++)
            {
                s[i] = (byte)i;
                k[i] = key[i % key.Length];
            }

            int j = 0;
            for (int i = 0; i < 256; i++)
            {
                j = (j + s[i] + k[i]) & 0xFF;
                Swap(ref s[i], ref s[j]);
            }

            // 伪随机生成算法（PRGA）
            int a = 0;
            int b = 0;

            for (int i = 0; i < length; i++)
            {
                a = (a + 1) & 0xFF;
                b = (b + s[a]) & 0xFF;
                Swap(ref s[a], ref s[b]);
                byte t = (byte)((s[a] + s[b]) & 0xFF);
                result[i] = (byte)(data[offset + i] ^ s[t]);
            }

            return result;
        }

        /// <summary>
        /// 加密字符串并返回 Base64
        /// </summary>
        /// <param name="plainText">明文</param>
        /// <param name="key">密钥</param>
        /// <returns>Base64 密文</returns>
        public static string EncryptToBase64(string plainText, byte[] key)
        {
            if (string.IsNullOrEmpty(plainText))
                return string.Empty;

            byte[] data = System.Text.Encoding.UTF8.GetBytes(plainText);
            byte[] encrypted = Process(data, key);
            return Convert.ToBase64String(encrypted);
        }

        /// <summary>
        /// 从 Base64 解密字符串
        /// </summary>
        /// <param name="cipherText">Base64 密文</param>
        /// <param name="key">密钥</param>
        /// <returns>明文字符串</returns>
        public static string DecryptFromBase64(string cipherText, byte[] key)
        {
            if (string.IsNullOrEmpty(cipherText))
                return string.Empty;

            byte[] data = Convert.FromBase64String(cipherText);
            byte[] decrypted = Process(data, key);
            return System.Text.Encoding.UTF8.GetString(decrypted);
        }

        /// <summary>
        /// 加密字符串并返回十六进制
        /// </summary>
        /// <param name="plainText">明文</param>
        /// <param name="key">密钥</param>
        /// <returns>十六进制密文</returns>
        public static string EncryptToHex(string plainText, byte[] key)
        {
            if (string.IsNullOrEmpty(plainText))
                return string.Empty;

            byte[] data = System.Text.Encoding.UTF8.GetBytes(plainText);
            byte[] encrypted = Process(data, key);
            return BitConverter.ToString(encrypted).Replace("-", "").ToLower();
        }

        /// <summary>
        /// 从十六进制解密字符串
        /// </summary>
        /// <param name="cipherHex">十六进制密文</param>
        /// <param name="key">密钥</param>
        /// <returns>明文字符串</returns>
        public static string DecryptFromHex(string cipherHex, byte[] key)
        {
            if (string.IsNullOrEmpty(cipherHex))
                return string.Empty;

            byte[] data = HexToBytes(cipherHex);
            byte[] decrypted = Process(data, key);
            return System.Text.Encoding.UTF8.GetString(decrypted);
        }

        /// <summary>
        /// 生成随机密钥
        /// </summary>
        /// <param name="length">密钥长度（1-256）</param>
        /// <returns>随机密钥</returns>
        public static byte[] GenerateKey(int length = 16)
        {
            if (length < 1 || length > 256)
                throw new ArgumentException("Key length must be between 1 and 256", nameof(length));

            byte[] key = new byte[length];
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            rng.GetBytes(key);
            return key;
        }

        /// <summary>
        /// 生成随机密钥并返回十六进制
        /// </summary>
        /// <param name="length">密钥长度</param>
        /// <returns>十六进制密钥</returns>
        public static string GenerateKeyHex(int length = 16)
        {
            byte[] key = GenerateKey(length);
            return BitConverter.ToString(key).Replace("-", "").ToLower();
        }

        /// <summary>
        /// 创建 RC4 流处理器（用于流式处理）
        /// </summary>
        /// <param name="key">密钥</param>
        /// <returns>RC4 处理器</returns>
        public static RC4Processor CreateProcessor(byte[] key)
        {
            return new RC4Processor(key);
        }

        private static void Swap(ref byte a, ref byte b)
        {
            byte temp = a;
            a = b;
            b = temp;
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

    /// <summary>
    /// RC4 流处理器（支持状态保持）
    /// </summary>
    public class RC4Processor
    {
        private readonly byte[] _s = new byte[256];
        private int _i;
        private int _j;

        /// <summary>
        /// 创建 RC4 处理器
        /// </summary>
        /// <param name="key">密钥</param>
        public RC4Processor(byte[] key)
        {
            if (key == null || key.Length < 1 || key.Length > 256)
                throw new ArgumentException("Key must be between 1 and 256 bytes", nameof(key));

            // KSA
            for (int i = 0; i < 256; i++)
            {
                _s[i] = (byte)i;
            }

            int j = 0;
            for (int i = 0; i < 256; i++)
            {
                j = (j + _s[i] + key[i % key.Length]) & 0xFF;
                Swap(ref _s[i], ref _s[j]);
            }

            _i = 0;
            _j = 0;
        }

        /// <summary>
        /// 处理一个字节
        /// </summary>
        /// <param name="input">输入字节</param>
        /// <returns>输出字节</returns>
        public byte ProcessByte(byte input)
        {
            _i = (_i + 1) & 0xFF;
            _j = (_j + _s[_i]) & 0xFF;
            Swap(ref _s[_i], ref _s[_j]);
            byte t = (byte)((_s[_i] + _s[_j]) & 0xFF);
            return (byte)(input ^ _s[t]);
        }

        /// <summary>
        /// 处理多个字节
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <returns>输出数据</returns>
        public byte[] Process(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            byte[] result = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                result[i] = ProcessByte(data[i]);
            }
            return result;
        }

        /// <summary>
        /// 重置处理器状态
        /// </summary>
        /// <param name="key">密钥</param>
        public void Reset(byte[] key)
        {
            if (key == null || key.Length < 1 || key.Length > 256)
                throw new ArgumentException("Key must be between 1 and 256 bytes", nameof(key));

            for (int i = 0; i < 256; i++)
            {
                _s[i] = (byte)i;
            }

            int j = 0;
            for (int i = 0; i < 256; i++)
            {
                j = (j + _s[i] + key[i % key.Length]) & 0xFF;
                Swap(ref _s[i], ref _s[j]);
            }

            _i = 0;
            _j = 0;
        }

        private static void Swap(ref byte a, ref byte b)
        {
            byte temp = a;
            a = b;
            b = temp;
        }
    }
}
