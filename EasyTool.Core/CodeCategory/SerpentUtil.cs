using System;
using System.Security.Cryptography;
using System.Text;

namespace EasyTool.CodeCategory
{
    /// <summary>
    /// Serpent 对称加密工具类
    /// Serpent 是 AES 的最终候选算法之一
    /// 128位分组密码，支持128/192/256位密钥
    /// 使用 32 轮加密，安全性极高
    /// </summary>
    public static class SerpentUtil
    {
        private const int BlockSize = 16;
        private const int Rounds = 32;

        // S-boxes (8个 4x4 S-box)
        private static readonly byte[,] SBox = new byte[,]
        {
            { 3, 8, 15, 1, 10, 6, 5, 11, 14, 13, 4, 2, 7, 0, 9, 12 },
            { 15, 12, 2, 7, 9, 0, 5, 10, 1, 11, 14, 8, 6, 13, 3, 4 },
            { 8, 6, 7, 9, 3, 12, 10, 15, 13, 1, 14, 4, 0, 11, 5, 2 },
            { 0, 15, 11, 8, 12, 9, 6, 3, 13, 1, 2, 4, 10, 7, 5, 14 },
            { 1, 15, 8, 3, 12, 0, 11, 6, 2, 5, 4, 10, 9, 14, 7, 13 },
            { 15, 5, 2, 11, 4, 10, 9, 12, 0, 3, 14, 8, 13, 6, 7, 1 },
            { 7, 2, 12, 5, 8, 4, 6, 11, 14, 9, 1, 15, 13, 3, 10, 0 },
            { 1, 13, 15, 0, 14, 8, 2, 11, 7, 4, 12, 10, 9, 3, 5, 6 }
        };

        private static readonly byte[,] SBoxInv = new byte[,]
        {
            { 13, 3, 11, 0, 10, 6, 5, 12, 1, 14, 4, 7, 15, 9, 8, 2 },
            { 5, 8, 2, 14, 15, 6, 12, 3, 11, 4, 7, 9, 1, 13, 10, 0 },
            { 12, 9, 15, 4, 11, 14, 1, 2, 0, 3, 6, 13, 5, 8, 10, 7 },
            { 0, 9, 10, 7, 11, 14, 6, 13, 3, 5, 12, 2, 4, 8, 15, 1 },
            { 5, 0, 8, 3, 10, 9, 7, 14, 2, 12, 11, 6, 4, 15, 13, 1 },
            { 8, 15, 2, 9, 4, 1, 13, 14, 11, 6, 5, 3, 7, 12, 10, 0 },
            { 15, 10, 1, 13, 5, 3, 6, 0, 4, 9, 14, 7, 2, 12, 8, 11 },
            { 3, 0, 6, 13, 9, 14, 15, 8, 5, 12, 11, 7, 10, 1, 4, 2 }
        };

        /// <summary>
        /// 加密数据（ECB模式）
        /// </summary>
        /// <param name="plainText">明文</param>
        /// <param name="key">密钥（16/24/32字节）</param>
        /// <returns>密文</returns>
        public static byte[] Encrypt(byte[] plainText, byte[] key)
        {
            if (plainText == null)
                throw new ArgumentNullException(nameof(plainText));
            if (key == null || (key.Length != 16 && key.Length != 24 && key.Length != 32))
                throw new ArgumentException("Key must be 16, 24, or 32 bytes", nameof(key));

            uint[] subkeys = GenerateSubkeys(key);

            int paddedLength = ((plainText.Length + BlockSize - 1) / BlockSize) * BlockSize;
            byte[] padded = new byte[paddedLength];
            Array.Copy(plainText, padded, plainText.Length);

            byte[] result = new byte[paddedLength];

            for (int i = 0; i < paddedLength; i += BlockSize)
            {
                EncryptBlock(padded, i, result, i, subkeys);
            }

            return result;
        }

        /// <summary>
        /// 解密数据（ECB模式）
        /// </summary>
        /// <param name="cipherText">密文</param>
        /// <param name="key">密钥</param>
        /// <returns>明文</returns>
        public static byte[] Decrypt(byte[] cipherText, byte[] key)
        {
            if (cipherText == null)
                throw new ArgumentNullException(nameof(cipherText));
            if (key == null || (key.Length != 16 && key.Length != 24 && key.Length != 32))
                throw new ArgumentException("Key must be 16, 24, or 32 bytes", nameof(key));
            if (cipherText.Length % BlockSize != 0)
                throw new ArgumentException("Cipher text length must be multiple of block size", nameof(cipherText));

            uint[] subkeys = GenerateSubkeys(key);
            byte[] result = new byte[cipherText.Length];

            for (int i = 0; i < cipherText.Length; i += BlockSize)
            {
                DecryptBlock(cipherText, i, result, i, subkeys);
            }

            return result;
        }

        /// <summary>
        /// 加密字符串并返回 Base64
        /// </summary>
        public static string EncryptToBase64(string plainText, byte[] key)
        {
            if (string.IsNullOrEmpty(plainText))
                return string.Empty;

            byte[] data = Encoding.UTF8.GetBytes(plainText);
            byte[] encrypted = Encrypt(data, key);
            return Convert.ToBase64String(encrypted);
        }

        /// <summary>
        /// 从 Base64 解密字符串
        /// </summary>
        public static string DecryptFromBase64(string cipherText, byte[] key)
        {
            if (string.IsNullOrEmpty(cipherText))
                return string.Empty;

            byte[] data = Convert.FromBase64String(cipherText);
            byte[] decrypted = Decrypt(data, key);
            return Encoding.UTF8.GetString(decrypted).TrimEnd('\0');
        }

        /// <summary>
        /// 生成随机密钥
        /// </summary>
        public static byte[] GenerateKey(int length = 32)
        {
            if (length != 16 && length != 24 && length != 32)
                throw new ArgumentException("Key length must be 16, 24, or 32 bytes", nameof(length));

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

        private static uint[] GenerateSubkeys(byte[] key)
        {
            uint[] subkeys = new uint[132]; // 33 * 4 = 132

            // 扩展密钥到 256 位
            uint[] expandedKey = new uint[8];
            for (int i = 0; i < Math.Min(key.Length / 4, 8); i++)
            {
                expandedKey[i] = BitConverter.ToUInt32(key, i * 4);
            }

            // 填充剩余部分
            if (key.Length < 32)
            {
                for (int i = key.Length / 4; i < 8; i++)
                {
                    expandedKey[i] = 0;
                }
            }

            // 生成子密钥
            uint phi = 0x9E3779B9;
            uint[] w = new uint[140];

            for (int i = 0; i < 8; i++)
                w[i] = expandedKey[i];

            for (int i = 8; i < 140; i++)
            {
                uint x = w[i - 8] ^ w[i - 5] ^ w[i - 3] ^ w[i - 1] ^ phi ^ (uint)i;
                w[i] = RotateLeft(x, 11);
            }

            // 应用 S-box
            for (int i = 0; i < 33; i++)
            {
                int sboxIdx = (35 - i) % 8;

                for (int j = 0; j < 4; j++)
                {
                    uint val = w[i * 4 + j + 8];
                    byte b0 = (byte)(val & 0xFF);
                    byte b1 = (byte)((val >> 8) & 0xFF);
                    byte b2 = (byte)((val >> 16) & 0xFF);
                    byte b3 = (byte)((val >> 24) & 0xFF);

                    b0 = SBox[sboxIdx, b0];
                    b1 = SBox[sboxIdx, b1];
                    b2 = SBox[sboxIdx, b2];
                    b3 = SBox[sboxIdx, b3];

                    subkeys[i * 4 + j] = (uint)(b0 | (b1 << 8) | (b2 << 16) | (b3 << 24));
                }
            }

            return subkeys;
        }

        private static void EncryptBlock(byte[] input, int inOffset, byte[] output, int outOffset, uint[] subkeys)
        {
            uint[] block = new uint[4];
            for (int i = 0; i < 4; i++)
                block[i] = BitConverter.ToUInt32(input, inOffset + i * 4);

            // 32轮加密
            for (int i = 0; i < Rounds; i++)
            {
                // 密钥加
                for (int j = 0; j < 4; j++)
                    block[j] ^= subkeys[i * 4 + j];

                // S-box 替换
                int sboxIdx = i % 8;
                for (int j = 0; j < 4; j++)
                {
                    block[j] = ApplySBox(block[j], sboxIdx);
                }

                // 线性变换（最后一轮除外）
                if (i < Rounds - 1)
                {
                    block[0] = RotateLeft(block[0], 13);
                    block[2] = RotateLeft(block[2], 3);
                    block[1] = RotateLeft(block[1] ^ block[0] ^ block[2], 1);
                    block[3] = RotateLeft(block[3] ^ block[2] ^ (block[0] << 3), 7);
                    block[0] ^= block[1] ^ block[3];
                    block[2] ^= block[3] ^ (block[1] << 7);
                }
            }

            // 最后一轮密钥加
            for (int i = 0; i < 4; i++)
                block[i] ^= subkeys[Rounds * 4 + i];

            for (int i = 0; i < 4; i++)
                BitConverter.GetBytes(block[i]).CopyTo(output, outOffset + i * 4);
        }

        private static void DecryptBlock(byte[] input, int inOffset, byte[] output, int outOffset, uint[] subkeys)
        {
            uint[] block = new uint[4];
            for (int i = 0; i < 4; i++)
                block[i] = BitConverter.ToUInt32(input, inOffset + i * 4);

            // 逆向密钥加
            for (int i = 0; i < 4; i++)
                block[i] ^= subkeys[Rounds * 4 + i];

            // 32轮解密
            for (int i = Rounds - 1; i >= 0; i--)
            {
                // 逆向 S-box
                int sboxIdx = i % 8;
                for (int j = 0; j < 4; j++)
                {
                    block[j] = ApplySBoxInv(block[j], sboxIdx);
                }

                // 逆向密钥加
                for (int j = 0; j < 4; j++)
                    block[j] ^= subkeys[i * 4 + j];

                // 逆向线性变换（第一轮除外）
                if (i > 0)
                {
                    block[2] ^= block[3] ^ (block[1] << 7);
                    block[0] ^= block[1] ^ block[3];
                    block[3] = RotateRight(block[3] ^ block[2] ^ (block[0] << 3), 7);
                    block[1] = RotateRight(block[1] ^ block[0] ^ block[2], 1);
                    block[2] = RotateRight(block[2], 3);
                    block[0] = RotateRight(block[0], 13);
                }
            }

            for (int i = 0; i < 4; i++)
                BitConverter.GetBytes(block[i]).CopyTo(output, outOffset + i * 4);
        }

        private static uint ApplySBox(uint val, int sboxIdx)
        {
            byte b0 = (byte)(val & 0xFF);
            byte b1 = (byte)((val >> 8) & 0xFF);
            byte b2 = (byte)((val >> 16) & 0xFF);
            byte b3 = (byte)((val >> 24) & 0xFF);

            b0 = SBox[sboxIdx, b0];
            b1 = SBox[sboxIdx, b1];
            b2 = SBox[sboxIdx, b2];
            b3 = SBox[sboxIdx, b3];

            return (uint)(b0 | (b1 << 8) | (b2 << 16) | (b3 << 24));
        }

        private static uint ApplySBoxInv(uint val, int sboxIdx)
        {
            byte b0 = (byte)(val & 0xFF);
            byte b1 = (byte)((val >> 8) & 0xFF);
            byte b2 = (byte)((val >> 16) & 0xFF);
            byte b3 = (byte)((val >> 24) & 0xFF);

            b0 = SBoxInv[sboxIdx, b0];
            b1 = SBoxInv[sboxIdx, b1];
            b2 = SBoxInv[sboxIdx, b2];
            b3 = SBoxInv[sboxIdx, b3];

            return (uint)(b0 | (b1 << 8) | (b2 << 16) | (b3 << 24));
        }

        private static uint RotateLeft(uint x, int n) => (x << n) | (x >> (32 - n));
        private static uint RotateRight(uint x, int n) => (x >> n) | (x << (32 - n));
    }
}
