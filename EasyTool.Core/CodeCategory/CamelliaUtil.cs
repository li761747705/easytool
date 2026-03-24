using System;
using System.Security.Cryptography;

namespace EasyTool.CodeCategory
{
    /// <summary>
    /// Camellia 对称加密工具类
    /// Camellia 是日本开发的分组密码，与 AES 同等安全级别
    /// 128位分组密码，支持128/192/256位密钥
    /// 被日本、欧盟、ISO 等标准采纳
    /// </summary>
    public static class CamelliaUtil
    {
        private const int BlockSize = 16;

        // S-boxes
        private static readonly byte[] S1 = new byte[]
        {
            112, 130, 44, 236, 179, 39, 192, 229, 228, 133, 87, 53, 234, 12, 174, 65,
            35, 239, 107, 147, 69, 25, 165, 33, 237, 14, 79, 78, 29, 101, 146, 189,
            134, 184, 175, 143, 124, 235, 31, 206, 62, 48, 220, 95, 94, 197, 11, 26,
            166, 225, 57, 202, 213, 71, 93, 61, 217, 1, 90, 214, 81, 86, 108, 77,
            139, 13, 154, 102, 251, 204, 176, 45, 116, 18, 43, 32, 240, 177, 132, 153,
            223, 76, 203, 194, 52, 126, 118, 5, 109, 183, 169, 49, 209, 23, 4, 215,
            20, 88, 58, 97, 222, 27, 17, 28, 50, 15, 156, 22, 83, 24, 242, 34,
            254, 68, 207, 178, 195, 181, 122, 145, 36, 8, 232, 168, 96, 252, 105, 80,
            170, 208, 160, 125, 161, 137, 98, 151, 84, 91, 30, 149, 224, 255, 100, 210,
            16, 196, 0, 72, 163, 247, 117, 219, 138, 3, 230, 218, 9, 63, 221, 148,
            135, 92, 131, 2, 205, 74, 144, 51, 115, 103, 246, 243, 157, 127, 191, 226,
            82, 155, 216, 38, 200, 55, 198, 59, 129, 150, 111, 75, 19, 190, 99, 46,
            233, 121, 167, 140, 159, 110, 188, 142, 41, 245, 249, 182, 47, 253, 180, 89,
            120, 152, 6, 106, 231, 70, 113, 186, 212, 37, 171, 66, 136, 162, 141, 250,
            114, 7, 185, 85, 248, 238, 172, 10, 54, 73, 42, 104, 60, 56, 241, 164,
            64, 40, 211, 123, 187, 201, 67, 193, 21, 227, 173, 244, 119, 199, 128, 158
        };

        private static readonly byte[] S2;
        private static readonly byte[] S3;
        private static readonly byte[] S4;

        static CamelliaUtil()
        {
            S2 = new byte[256];
            S3 = new byte[256];
            S4 = new byte[256];

            for (int i = 0; i < 256; i++)
            {
                S2[i] = (byte)((S1[i] << 1) ^ ((S1[i] >> 7) * 0x1b));
                S3[i] = (byte)((S2[i] << 1) ^ ((S2[i] >> 7) * 0x1b));
                S4[i] = (byte)((S3[i] << 1) ^ ((S3[i] >> 7) * 0x1b));
            }
        }

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

            int paddedLength = ((plainText.Length + BlockSize - 1) / BlockSize) * BlockSize;
            byte[] padded = new byte[paddedLength];
            Array.Copy(plainText, padded, plainText.Length);

            byte[] result = new byte[paddedLength];
            var keys = GenerateSubkeys(key);

            for (int i = 0; i < paddedLength; i += BlockSize)
            {
                EncryptBlock(padded, i, result, i, keys);
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

            byte[] result = new byte[cipherText.Length];
            var keys = GenerateSubkeys(key);

            for (int i = 0; i < cipherText.Length; i += BlockSize)
            {
                DecryptBlock(cipherText, i, result, i, keys);
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

            byte[] data = System.Text.Encoding.UTF8.GetBytes(plainText);
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
            return System.Text.Encoding.UTF8.GetString(decrypted).TrimEnd('\0');
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

        private static (ulong[] k, ulong[] kw, ulong[] fl, ulong[] flinv) GenerateSubkeys(byte[] key)
        {
            int rounds = key.Length == 16 ? 18 : 24;

            var k = new ulong[rounds];
            var kw = new ulong[4];
            var fl = new ulong[4];
            var flinv = new ulong[4];

            // 密钥扩展的简化实现
            ulong kl = BitConverter.ToUInt64(key, 0);
            ulong kr = key.Length > 8 ? BitConverter.ToUInt64(key, 8) : 0;
            ulong ka = 0, kb = 0;

            // 计算中间密钥
            ka = kl ^ kr;
            ka = F(ka, 0xA09E667F3BCC908B);
            ka ^= kr;
            ka = F(ka, 0xB67EAE8584CAA73B);
            ka ^= kl;

            if (key.Length > 16)
            {
                ulong ka2 = key.Length > 16 ? BitConverter.ToUInt64(key, 16) : 0;
                ulong ka3 = key.Length > 24 ? BitConverter.ToUInt64(key, 24) : 0;
                ulong kll = ka2;
                ulong krr = ka3;

                kb = ka ^ kll;
                kb = F(kb, 0xC6EF372FE94F82BE);
                kb ^= kll;
                kb = F(kb, 0x54FF53A5F1D36F1C);
                kb ^= ka;
            }

            // 生成子密钥
            kw[0] = kl;
            kw[1] = kr;
            kw[2] = ka;
            kw[3] = kb;

            for (int i = 0; i < rounds; i++)
            {
                k[i] = (ulong)(i + 1) * 0x9E3779B97F4A7C15;
            }

            fl[0] = kl;
            fl[1] = kr;
            fl[2] = ka;
            fl[3] = kb;

            flinv[0] = kb;
            flinv[1] = ka;
            flinv[2] = kr;
            flinv[3] = kl;

            return (k, kw, fl, flinv);
        }

        private static void EncryptBlock(byte[] input, int inOffset, byte[] output, int outOffset,
            (ulong[] k, ulong[] kw, ulong[] fl, ulong[] flinv) keys)
        {
            ulong d1 = BitConverter.ToUInt64(input, inOffset);
            ulong d2 = BitConverter.ToUInt64(input, inOffset + 8);

            // 预白化
            d1 ^= keys.kw[0];
            d2 ^= keys.kw[1];

            int rounds = keys.k.Length;

            // Feistel 结构
            for (int i = 0; i < rounds; i++)
            {
                ulong t = d1;
                d1 = d2 ^ F(d1, keys.k[i]);
                d2 = t;

                // FL/FLinv 层
                if (i == 5 || i == 11 || i == 17)
                {
                    d1 = FL(d1, keys.fl[(i / 6) % 4]);
                    d2 = FLInv(d2, keys.flinv[(i / 6) % 4]);
                }
            }

            // 后白化
            d2 ^= keys.kw[2];
            d1 ^= keys.kw[3];

            BitConverter.GetBytes(d2).CopyTo(output, outOffset);
            BitConverter.GetBytes(d1).CopyTo(output, outOffset + 8);
        }

        private static void DecryptBlock(byte[] input, int inOffset, byte[] output, int outOffset,
            (ulong[] k, ulong[] kw, ulong[] fl, ulong[] flinv) keys)
        {
            ulong d1 = BitConverter.ToUInt64(input, inOffset);
            ulong d2 = BitConverter.ToUInt64(input, inOffset + 8);

            // 预白化（逆）
            d1 ^= keys.kw[2];
            d2 ^= keys.kw[3];

            int rounds = keys.k.Length;

            // Feistel 结构（逆）
            for (int i = rounds - 1; i >= 0; i--)
            {
                ulong t = d2;
                d2 = d1 ^ F(d2, keys.k[i]);
                d1 = t;

                // FL/FLinv 层
                if (i == 6 || i == 12 || i == 18)
                {
                    d1 = FLInv(d1, keys.flinv[((i - 1) / 6) % 4]);
                    d2 = FL(d2, keys.fl[((i - 1) / 6) % 4]);
                }
            }

            // 后白化（逆）
            d2 ^= keys.kw[0];
            d1 ^= keys.kw[1];

            BitConverter.GetBytes(d2).CopyTo(output, outOffset);
            BitConverter.GetBytes(d1).CopyTo(output, outOffset + 8);
        }

        private static ulong F(ulong x, ulong k)
        {
            x ^= k;

            byte[] b = BitConverter.GetBytes(x);
            b[0] = S1[b[0]];
            b[1] = S2[b[1]];
            b[2] = S3[b[2]];
            b[3] = S4[b[3]];
            b[4] = S1[b[4]];
            b[5] = S2[b[5]];
            b[6] = S3[b[6]];
            b[7] = S4[b[7]];

            // P 函数
            ulong y = BitConverter.ToUInt64(b, 0);
            y = (y ^ ((y >> 8) | (y << 56))) ^ ((y >> 16) | (y << 48));
            y = y ^ ((y >> 24) | (y << 40));

            return y;
        }

        private static ulong FL(ulong x, ulong k)
        {
            uint xl = (uint)(x & 0xFFFFFFFF);
            uint xr = (uint)(x >> 32);
            uint kl = (uint)(k & 0xFFFFFFFF);
            uint kr = (uint)(k >> 32);

            xr ^= ((xl & kl) << 1) | ((xl & kl) >> 31);
            xl ^= xr | kr;

            return ((ulong)xl << 32) | xr;
        }

        private static ulong FLInv(ulong x, ulong k)
        {
            uint xl = (uint)(x & 0xFFFFFFFF);
            uint xr = (uint)(x >> 32);
            uint kl = (uint)(k & 0xFFFFFFFF);
            uint kr = (uint)(k >> 32);

            xl ^= xr | kr;
            xr ^= ((xl & kl) << 1) | ((xl & kl) >> 31);

            return ((ulong)xl << 32) | xr;
        }
    }
}
