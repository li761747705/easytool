using System;
using System.Security.Cryptography;
using System.Text;

namespace EasyTool.CodeCategory
{
    /// <summary>
    /// Salsa20 流加密工具类
    /// Salsa20 是一种高速流密码，由 Daniel J. Bernstein 设计
    /// ChaCha20 是 Salsa20 的改进版本
    /// </summary>
    public static class Salsa20Util
    {
        private static readonly uint[] Sigma = new uint[] { 0x61707865, 0x3320646e, 0x79622d32, 0x6b206574 };

        /// <summary>
        /// 使用 Salsa20 加密数据
        /// </summary>
        /// <param name="plainText">明文</param>
        /// <param name="key">密钥（16或32字节）</param>
        /// <param name="nonce">随机数（8字节）</param>
        /// <returns>密文</returns>
        public static byte[] Encrypt(byte[] plainText, byte[] key, byte[] nonce)
        {
            return Encrypt(plainText, 0, plainText?.Length ?? 0, key, nonce, 0);
        }

        /// <summary>
        /// 使用 Salsa20 加密数据
        /// </summary>
        public static byte[] Encrypt(byte[] plainText, int offset, int length, byte[] key, byte[] nonce, uint initialCounter = 0)
        {
            if (plainText == null)
                throw new ArgumentNullException(nameof(plainText));
            if (key == null || (key.Length != 16 && key.Length != 32))
                throw new ArgumentException("Key must be 16 or 32 bytes", nameof(key));
            if (nonce == null || nonce.Length != 8)
                throw new ArgumentException("Nonce must be 8 bytes", nameof(nonce));

            byte[] cipherText = new byte[length];
            Process(plainText, offset, length, cipherText, 0, key, nonce, initialCounter);
            return cipherText;
        }

        /// <summary>
        /// 使用 Salsa20 解密数据（加密和解密相同）
        /// </summary>
        public static byte[] Decrypt(byte[] cipherText, byte[] key, byte[] nonce)
        {
            return Decrypt(cipherText, 0, cipherText?.Length ?? 0, key, nonce, 0);
        }

        /// <summary>
        /// 使用 Salsa20 解密数据
        /// </summary>
        public static byte[] Decrypt(byte[] cipherText, int offset, int length, byte[] key, byte[] nonce, uint initialCounter = 0)
        {
            return Encrypt(cipherText, offset, length, key, nonce, initialCounter);
        }

        /// <summary>
        /// 加密字符串并返回 Base64
        /// </summary>
        public static string EncryptToBase64(string plainText, byte[] key)
        {
            if (string.IsNullOrEmpty(plainText))
                return string.Empty;

            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] nonce = new byte[8];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(nonce);

            byte[] cipherBytes = Encrypt(plainBytes, key, nonce);

            byte[] result = new byte[8 + cipherBytes.Length];
            Array.Copy(nonce, result, 8);
            Array.Copy(cipherBytes, 0, result, 8, cipherBytes.Length);

            return Convert.ToBase64String(result);
        }

        /// <summary>
        /// 从 Base64 解密字符串
        /// </summary>
        public static string DecryptFromBase64(string cipherText, byte[] key)
        {
            if (string.IsNullOrEmpty(cipherText))
                return string.Empty;

            byte[] data = Convert.FromBase64String(cipherText);
            if (data.Length < 8)
                throw new ArgumentException("Invalid cipher text");

            byte[] nonce = new byte[8];
            Array.Copy(data, nonce, 8);

            byte[] cipherBytes = new byte[data.Length - 8];
            Array.Copy(data, 8, cipherBytes, 0, cipherBytes.Length);

            byte[] plainBytes = Decrypt(cipherBytes, key, nonce);
            return Encoding.UTF8.GetString(plainBytes);
        }

        /// <summary>
        /// 生成随机密钥
        /// </summary>
        public static byte[] GenerateKey(int length = 32)
        {
            if (length != 16 && length != 32)
                throw new ArgumentException("Key length must be 16 or 32 bytes", nameof(length));

            byte[] key = new byte[length];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(key);
            return key;
        }

        /// <summary>
        /// 生成随机密钥并返回十六进制
        /// </summary>
        public static string GenerateKeyHex(int length = 32)
        {
            byte[] key = GenerateKey(length);
            return BitConverter.ToString(key).Replace("-", "").ToLower();
        }

        private static void Process(byte[] input, int inputOffset, int inputLength, byte[] output, int outputOffset, byte[] key, byte[] nonce, uint counter)
        {
            uint[] state = new uint[16];
            uint[] block = new uint[16];

            // 初始化状态
            state[0] = Sigma[0];
            state[1] = (key.Length == 32) ? BitConverter.ToUInt32(key, 0) : Sigma[0];
            state[2] = (key.Length == 32) ? BitConverter.ToUInt32(key, 4) : Sigma[1];
            state[3] = (key.Length == 32) ? BitConverter.ToUInt32(key, 8) : Sigma[2];
            state[4] = (key.Length == 32) ? BitConverter.ToUInt32(key, 12) : Sigma[3];
            state[5] = (key.Length == 32) ? Sigma[1] : BitConverter.ToUInt32(key, 0);
            state[6] = (key.Length == 32) ? BitConverter.ToUInt32(key, 16) : BitConverter.ToUInt32(key, 4);
            state[7] = (key.Length == 32) ? BitConverter.ToUInt32(key, 20) : BitConverter.ToUInt32(key, 8);
            state[8] = (key.Length == 32) ? BitConverter.ToUInt32(key, 24) : BitConverter.ToUInt32(key, 12);
            state[9] = (key.Length == 32) ? BitConverter.ToUInt32(key, 28) : Sigma[0];
            state[10] = Sigma[2];
            state[11] = BitConverter.ToUInt32(nonce, 0);
            state[12] = BitConverter.ToUInt32(nonce, 4);
            state[13] = counter;
            state[14] = Sigma[3];
            state[15] = (key.Length == 32) ? Sigma[3] : Sigma[1];

            int processed = 0;
            while (processed < inputLength)
            {
                Array.Copy(state, block, 16);

                // 20 轮
                for (int i = 0; i < 10; i++)
                {
                    QuarterRound(ref block[0], ref block[4], ref block[8], ref block[12]);
                    QuarterRound(ref block[5], ref block[9], ref block[13], ref block[1]);
                    QuarterRound(ref block[10], ref block[14], ref block[2], ref block[6]);
                    QuarterRound(ref block[15], ref block[3], ref block[7], ref block[11]);
                }

                for (int i = 0; i < 16; i++)
                    block[i] += state[i];

                int blockSize = Math.Min(64, inputLength - processed);
                for (int i = 0; i < blockSize; i++)
                {
                    output[outputOffset + processed + i] = (byte)(input[inputOffset + processed + i] ^ (block[i / 4] >> ((i % 4) * 8)));
                }

                processed += blockSize;
                state[13]++;
            }
        }

        private static void QuarterRound(ref uint a, ref uint b, ref uint c, ref uint d)
        {
            b ^= RotateLeft(a + d, 7);
            c ^= RotateLeft(b + a, 9);
            d ^= RotateLeft(c + b, 13);
            a ^= RotateLeft(d + c, 18);
        }

        private static uint RotateLeft(uint x, int n) => (x << n) | (x >> (32 - n));
    }
}
