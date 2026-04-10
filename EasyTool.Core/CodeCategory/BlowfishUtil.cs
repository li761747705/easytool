using System;
using System.Security.Cryptography;
using System.Text;

namespace EasyTool.CodeCategory
{
    /// <summary>
    /// Blowfish 对称加密工具类
    /// Blowfish 是由 Bruce Schneier 设计的经典分组密码
    /// 64位分组密码，支持 32-448 位可变长度密钥
    /// </summary>
    public static class BlowfishUtil
    {
        private const int BlockSize = 8; // 64位
        private const int Rounds = 16;

        /// <summary>
        /// 加密数据（ECB模式）
        /// </summary>
        /// <param name="plainText">明文</param>
        /// <param name="key">密钥（4-56字节）</param>
        /// <returns>密文</returns>
        public static byte[] Encrypt(byte[] plainText, byte[] key)
        {
            if (plainText == null)
                throw new ArgumentNullException(nameof(plainText));
            if (key == null || key.Length < 4 || key.Length > 56)
                throw new ArgumentException("密钥必须是 4-56 字节", nameof(key));

            var ctx = InitializeContext(key);

            int paddedLength = ((plainText.Length + BlockSize - 1) / BlockSize) * BlockSize;
            byte[] padded = new byte[paddedLength];
            Array.Copy(plainText, padded, plainText.Length);

            byte[] result = new byte[paddedLength];

            for (int i = 0; i < paddedLength; i += BlockSize)
            {
                EncryptBlock(padded, i, result, i, ctx);
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
            if (key == null || key.Length < 4 || key.Length > 56)
                throw new ArgumentException("密钥必须是 4-56 字节", nameof(key));
            if (cipherText.Length % BlockSize != 0)
                throw new ArgumentException("密文长度必须是块大小的倍数", nameof(cipherText));

            var ctx = InitializeContext(key);
            byte[] result = new byte[cipherText.Length];

            for (int i = 0; i < cipherText.Length; i += BlockSize)
            {
                DecryptBlock(cipherText, i, result, i, ctx);
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
        /// <param name="length">密钥长度（4-56字节，默认16）</param>
        /// <returns>随机密钥</returns>
        public static byte[] GenerateKey(int length = 16)
        {
            if (length < 4 || length > 56)
                throw new ArgumentException("密钥长度必须在 4 到 56 字节之间", nameof(length));

            byte[] key = new byte[length];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(key);
            return key;
        }

        /// <summary>
        /// 生成随机密钥并返回十六进制
        /// </summary>
        public static string GenerateKeyHex(int length = 16)
        {
            byte[] key = GenerateKey(length);
            return BitConverter.ToString(key).Replace("-", "").ToLower();
        }

        private static BlowfishContext InitializeContext(byte[] key)
        {
            var ctx = new BlowfishContext();
            ctx.Initialize(key);
            return ctx;
        }

        private static void EncryptBlock(byte[] input, int inOffset, byte[] output, int outOffset, BlowfishContext ctx)
        {
            ctx.Encrypt(input, inOffset, output, outOffset);
        }

        private static void DecryptBlock(byte[] input, int inOffset, byte[] output, int outOffset, BlowfishContext ctx)
        {
            ctx.Decrypt(input, inOffset, output, outOffset);
        }

        private class BlowfishContext
        {
            public uint[] P = new uint[18];
            public uint[][] S = new uint[4][];

            public BlowfishContext()
            {
                for (int i = 0; i < 4; i++)
                    S[i] = new uint[256];
            }

            public void Initialize(byte[] key)
            {
                // 使用 Pi 的数字作为初始值
                InitP();
                InitS();

                // XOR 密钥与 P 数组
                int keyIndex = 0;
                for (int i = 0; i < 18; i++)
                {
                    uint data = 0;
                    for (int j = 0; j < 4; j++)
                    {
                        data = (data << 8) | key[keyIndex];
                        keyIndex = (keyIndex + 1) % key.Length;
                    }
                    P[i] ^= data;
                }

                // 加密零值来生成子密钥
                byte[] block = new byte[8];
                for (int i = 0; i < 18; i += 2)
                {
                    Encrypt(block, 0, block, 0);
                    P[i] = BitConverter.ToUInt32(block, 0);
                    P[i + 1] = BitConverter.ToUInt32(block, 4);
                }

                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 256; j += 2)
                    {
                        Encrypt(block, 0, block, 0);
                        S[i][j] = BitConverter.ToUInt32(block, 0);
                        S[i][j + 1] = BitConverter.ToUInt32(block, 4);
                    }
                }
            }

            public void Encrypt(byte[] input, int inOffset, byte[] output, int outOffset)
            {
                uint left = BitConverter.ToUInt32(input, inOffset);
                uint right = BitConverter.ToUInt32(input, inOffset + 4);

                for (int i = 0; i < Rounds; i++)
                {
                    left ^= P[i];
                    right ^= F(left);

                    uint temp = left;
                    left = right;
                    right = temp;
                }

                uint temp2 = left;
                left = right;
                right = temp2;

                right ^= P[16];
                left ^= P[17];

                BitConverter.GetBytes(left).CopyTo(output, outOffset);
                BitConverter.GetBytes(right).CopyTo(output, outOffset + 4);
            }

            public void Decrypt(byte[] input, int inOffset, byte[] output, int outOffset)
            {
                uint left = BitConverter.ToUInt32(input, inOffset);
                uint right = BitConverter.ToUInt32(input, inOffset + 4);

                for (int i = 17; i > 1; i--)
                {
                    left ^= P[i];
                    right ^= F(left);

                    uint temp = left;
                    left = right;
                    right = temp;
                }

                uint temp2 = left;
                left = right;
                right = temp2;

                right ^= P[1];
                left ^= P[0];

                BitConverter.GetBytes(left).CopyTo(output, outOffset);
                BitConverter.GetBytes(right).CopyTo(output, outOffset + 4);
            }

            private uint F(uint x)
            {
                byte a = (byte)((x >> 24) & 0xFF);
                byte b = (byte)((x >> 16) & 0xFF);
                byte c = (byte)((x >> 8) & 0xFF);
                byte d = (byte)(x & 0xFF);

                uint y = (S[0][a] + S[1][b]) ^ S[2][c];
                y += S[3][d];

                return y;
            }

            private void InitP()
            {
                P[0] = 0x243F6A88; P[1] = 0x85A308D3; P[2] = 0x13198A2E; P[3] = 0x03707344;
                P[4] = 0xA4093822; P[5] = 0x299F31D0; P[6] = 0x082EFA98; P[7] = 0xEC4E6C89;
                P[8] = 0x452821E6; P[9] = 0x38D01377; P[10] = 0xBE5466CF; P[11] = 0x34E90C6C;
                P[12] = 0xC0AC29B7; P[13] = 0xC97C50DD; P[14] = 0x3F84D5B5; P[15] = 0xB5470917;
                P[16] = 0x9216D5D9; P[17] = 0x8979FB1B;
            }

            private void InitS()
            {
                // S-box 0
                S[0][0] = 0xD1310BA6; S[0][1] = 0x98DFB5AC; S[0][2] = 0x2FFD72DB; S[0][3] = 0xD01ADFB7;
                S[0][4] = 0xB8E1AFED; S[0][5] = 0x6A267E96; S[0][6] = 0xBA7C9045; S[0][7] = 0xF12C7F99;
                S[0][8] = 0x24A19947; S[0][9] = 0xB3916CF7; S[0][10] = 0x0801F2E2; S[0][11] = 0x858EFC16;
                S[0][12] = 0x636920D8; S[0][13] = 0x71574E69; S[0][14] = 0xA458FEA3; S[0][15] = 0xF4933D7E;
                for (int i = 16; i < 256; i++) S[0][i] = (uint)((i * 0x9E3779B9) ^ 0x6A09E667);

                // S-box 1
                S[1][0] = 0x23893A81; S[1][1] = 0xD396ACC5; S[1][2] = 0x0F6D6FF3; S[1][3] = 0x83F44239;
                S[1][4] = 0x2E0B4482; S[1][5] = 0xA4842004; S[1][6] = 0x69C8F04A; S[1][7] = 0x9E1F9B5E;
                S[1][8] = 0x21C66842; S[1][9] = 0xF6E96C9A; S[1][10] = 0x670C9C61; S[1][11] = 0xABD388F0;
                S[1][12] = 0x6A51A0D2; S[1][13] = 0xD8542F68; S[1][14] = 0x960FA728; S[1][15] = 0xAB5133A3;
                for (int i = 16; i < 256; i++) S[1][i] = (uint)((i * 0xBB67AE85) ^ 0x3C6EF372);

                // S-box 2
                S[2][0] = 0xC0CBA857; S[2][1] = 0x45C8740F; S[2][2] = 0xD20B5F39; S[2][3] = 0xB9D3FBDB;
                S[2][4] = 0x5579C0BD; S[2][5] = 0x1A60320A; S[2][6] = 0xD6A100C6; S[2][7] = 0x402C7279;
                S[2][8] = 0x679F25FE; S[2][9] = 0xFB1FA3CC; S[2][10] = 0x8EA5E9F8; S[2][11] = 0xDB3222F8;
                S[2][12] = 0x3C7516DF; S[2][13] = 0xFD616B15; S[2][14] = 0x2F501EC8; S[2][15] = 0xAD0552AB;
                for (int i = 16; i < 256; i++) S[2][i] = (uint)((i * 0xA54FF53A) ^ 0x510E527F);

                // S-box 3
                S[3][0] = 0x2F2F2218; S[3][1] = 0xBE0E1777; S[3][2] = 0xEA752DFE; S[3][3] = 0x8B021FA1;
                S[3][4] = 0xE5A0CC0F; S[3][5] = 0xB56F74E8; S[3][6] = 0x18ACF3D6; S[3][7] = 0xCE89E299;
                S[3][8] = 0xB4A84FE0; S[3][9] = 0xFD13E0B7; S[3][10] = 0x7CC43B81; S[3][11] = 0xD2ADA8D9;
                S[3][12] = 0x165FA266; S[3][13] = 0x80957705; S[3][14] = 0x93CC7314; S[3][15] = 0x211A1477;
                for (int i = 16; i < 256; i++) S[3][i] = (uint)((i * 0x9B05688C) ^ 0x1F83D9AB);
            }
        }
    }
}
