using System;
using System.Security.Cryptography;
using System.Text;

namespace EasyTool.CodeCategory
{
    /// <summary>
    /// IDEA (International Data Encryption Algorithm) 对称加密工具类
    /// IDEA 是一种分组密码，曾用于 PGP
    /// 64位分组密码，使用 128 位密钥
    /// </summary>
    public static class IDEAUtil
    {
        private const int BlockSize = 8; // 64位
        private const int KeySize = 16; // 128位
        private const int Rounds = 8;

        /// <summary>
        /// 加密数据（ECB模式）
        /// </summary>
        /// <param name="plainText">明文</param>
        /// <param name="key">密钥（16字节）</param>
        /// <returns>密文</returns>
        public static byte[] Encrypt(byte[] plainText, byte[] key)
        {
            if (plainText == null)
                throw new ArgumentNullException(nameof(plainText));
            if (key == null || key.Length != KeySize)
                throw new ArgumentException("密钥必须是 16 字节", nameof(key));

            ushort[] subkeys = GenerateEncryptionSubkeys(key);

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
            if (key == null || key.Length != KeySize)
                throw new ArgumentException("密钥必须是 16 字节", nameof(key));
            if (cipherText.Length % BlockSize != 0)
                throw new ArgumentException("密文长度必须是块大小的倍数", nameof(cipherText));

            ushort[] subkeys = GenerateDecryptionSubkeys(key);
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
        public static byte[] GenerateKey()
        {
            byte[] key = new byte[KeySize];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(key);
            return key;
        }

        /// <summary>
        /// 生成随机密钥并返回十六进制
        /// </summary>
        public static string GenerateKeyHex()
        {
            byte[] key = GenerateKey();
            return BitConverter.ToString(key).Replace("-", "").ToLower();
        }

        private static ushort[] GenerateEncryptionSubkeys(byte[] key)
        {
            ushort[] subkeys = new ushort[52];

            // 将密钥分为8个16位子密钥
            for (int i = 0; i < 8; i++)
            {
                subkeys[i] = (ushort)((key[i * 2] << 8) | key[i * 2 + 1]);
            }

            // 循环移位生成更多子密钥
            for (int i = 8; i < 52; i++)
            {
                if ((i % 8) == 0)
                {
                    // 左移25位
                    byte[] temp = new byte[KeySize];
                    for (int j = 0; j < KeySize; j++)
                    {
                        int srcIdx = ((j + 3) % KeySize);
                        int bitPos = (j + 3) >= KeySize ? (j + 3 - KeySize) * 8 % 128 : (j + 3 - 8) * 8 % 128;
                        temp[j] = key[srcIdx];
                    }

                    for (int j = 0; j < KeySize; j++)
                        key[j] = temp[j];
                }

                subkeys[i] = (ushort)((key[((i % 8) * 2) % KeySize] << 8) |
                                       key[((i % 8) * 2 + 1) % KeySize]);
            }

            return subkeys;
        }

        private static ushort[] GenerateDecryptionSubkeys(byte[] key)
        {
            ushort[] encSubkeys = GenerateEncryptionSubkeys(key);
            ushort[] decSubkeys = new ushort[52];

            // 解密子密钥是加密子密钥的逆
            for (int i = 0; i < Rounds; i++)
            {
                int idx = i * 6;

                if (i == 0)
                {
                    decSubkeys[idx] = MulInv(encSubkeys[48 - i * 6]);
                    decSubkeys[idx + 1] = AddInv(encSubkeys[49 - i * 6]);
                    decSubkeys[idx + 2] = AddInv(encSubkeys[50 - i * 6]);
                    decSubkeys[idx + 3] = MulInv(encSubkeys[51 - i * 6]);
                }
                else
                {
                    decSubkeys[idx] = MulInv(encSubkeys[48 - i * 6]);
                    decSubkeys[idx + 1] = AddInv(encSubkeys[49 - i * 6]);
                    decSubkeys[idx + 2] = AddInv(encSubkeys[50 - i * 6]);
                    decSubkeys[idx + 3] = MulInv(encSubkeys[51 - i * 6]);
                }

                decSubkeys[idx + 4] = encSubkeys[46 - i * 6];
                decSubkeys[idx + 5] = encSubkeys[47 - i * 6];
            }

            // 最后一轮的子密钥
            decSubkeys[48] = MulInv(encSubkeys[0]);
            decSubkeys[49] = AddInv(encSubkeys[1]);
            decSubkeys[50] = AddInv(encSubkeys[2]);
            decSubkeys[51] = MulInv(encSubkeys[3]);

            return decSubkeys;
        }

        private static void EncryptBlock(byte[] input, int inOffset, byte[] output, int outOffset, ushort[] subkeys)
        {
            // 将64位分为4个16位
            ushort x0 = (ushort)((input[inOffset] << 8) | input[inOffset + 1]);
            ushort x1 = (ushort)((input[inOffset + 2] << 8) | input[inOffset + 3]);
            ushort x2 = (ushort)((input[inOffset + 4] << 8) | input[inOffset + 5]);
            ushort x3 = (ushort)((input[inOffset + 6] << 8) | input[inOffset + 7]);

            int keyIdx = 0;

            for (int round = 0; round < Rounds; round++)
            {
                ushort y0 = Mul(x0, subkeys[keyIdx++]);
                ushort y1 = Add(x1, subkeys[keyIdx++]);
                ushort y2 = Add(x2, subkeys[keyIdx++]);
                ushort y3 = Mul(x3, subkeys[keyIdx++]);

                ushort t0 = Mul((ushort)(y0 ^ y2), subkeys[keyIdx++]);
                ushort t1 = Add((ushort)(y1 ^ y3), t0);
                ushort t2 = Mul(t1, subkeys[keyIdx++]);
                ushort t3 = Add(t0, t2);

                x0 = (ushort)(y0 ^ t2);
                x1 = (ushort)(y2 ^ t2);
                x2 = (ushort)(y1 ^ t3);
                x3 = (ushort)(y3 ^ t3);
            }

            // 最终输出变换
            ushort r0 = Mul(x0, subkeys[keyIdx++]);
            ushort r1 = Add(x2, subkeys[keyIdx++]);
            ushort r2 = Add(x1, subkeys[keyIdx++]);
            ushort r3 = Mul(x3, subkeys[keyIdx++]);

            output[outOffset] = (byte)(r0 >> 8);
            output[outOffset + 1] = (byte)r0;
            output[outOffset + 2] = (byte)(r1 >> 8);
            output[outOffset + 3] = (byte)r1;
            output[outOffset + 4] = (byte)(r2 >> 8);
            output[outOffset + 5] = (byte)r2;
            output[outOffset + 6] = (byte)(r3 >> 8);
            output[outOffset + 7] = (byte)r3;
        }

        private static void DecryptBlock(byte[] input, int inOffset, byte[] output, int outOffset, ushort[] subkeys)
        {
            // 解密使用相同的结构，只是子密钥不同
            EncryptBlock(input, inOffset, output, outOffset, subkeys);
        }

        // IDEA 的三种基本运算
        private static ushort Mul(ushort a, ushort b)
        {
            uint result = (uint)(a * b);
            if (result != 0)
            {
                result = (uint)((ushort)(result & 0xFFFF) - (ushort)(result >> 16));
                if (result < 0x10000)
                    result = (uint)(result + 1);
                return (ushort)result;
            }
            return (ushort)(1 - a - b);
        }

        private static ushort Add(ushort a, ushort b)
        {
            return (ushort)((a + b) & 0xFFFF);
        }

        private static ushort MulInv(ushort x)
        {
            if (x == 0)
                return 0;

            int n = 0x10001;
            int a = x;
            int b = n;
            int q, r;
            int t1 = 0, t2 = 1;

            while (b > 0)
            {
                q = a / b;
                r = a % b;
                int t = t1 - q * t2;
                a = b;
                b = r;
                t1 = t2;
                t2 = t;
            }

            if (t1 < 0)
                t1 += n;

            return (ushort)t1;
        }

        private static ushort AddInv(ushort x)
        {
            return (ushort)(0x10000 - x);
        }
    }
}
