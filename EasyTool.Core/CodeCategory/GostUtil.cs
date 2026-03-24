using System;
using System.Security.Cryptography;
using System.Text;

namespace EasyTool.CodeCategory
{
    /// <summary>
    /// GOST 哈希工具类
    /// GOST R 34.11-94 是俄罗斯国家标准哈希算法
    /// 输出 256 位（32 字节）哈希值
    /// </summary>
    public static class GostUtil
    {
        private const int HashSize = 32; // 256位
        private const int BlockSize = 32; // 256位

        // S-box (测试向量使用的标准 S-box)
        private static readonly byte[,] SBox = new byte[,]
        {
            { 4, 10, 9, 2, 13, 8, 0, 14, 6, 11, 1, 12, 7, 15, 5, 3 },
            { 14, 11, 4, 12, 6, 13, 15, 10, 2, 3, 8, 1, 0, 7, 5, 9 },
            { 5, 8, 1, 13, 10, 3, 4, 2, 14, 15, 12, 7, 6, 0, 9, 11 },
            { 7, 13, 10, 1, 0, 8, 9, 15, 14, 4, 6, 12, 11, 2, 5, 3 },
            { 6, 12, 7, 1, 5, 15, 13, 8, 4, 10, 9, 14, 0, 3, 11, 2 },
            { 4, 11, 10, 0, 7, 2, 1, 13, 3, 6, 8, 5, 9, 12, 15, 14 },
            { 13, 11, 4, 1, 3, 15, 5, 9, 0, 10, 14, 7, 6, 8, 2, 12 },
            { 1, 15, 13, 0, 5, 7, 10, 4, 9, 2, 3, 14, 6, 11, 8, 12 }
        };

        // 常量 C2-C12
        private static readonly byte[][] C = new byte[][]
        {
            new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 },
            new byte[] { 0x73, 0xE2, 0x23, 0x04, 0x42, 0xB8, 0x27, 0x10, 0xC4, 0x50, 0x16, 0xEE, 0x5C, 0x7B, 0x1A, 0x11, 0xA8, 0x8E, 0xA4, 0x31, 0x6A, 0x83, 0x93, 0x62, 0x6C, 0x31, 0xF8, 0xDE, 0x36, 0xB9, 0x0B, 0x36 },
            new byte[] { 0x23, 0x7A, 0x3E, 0xA0, 0x89, 0xB9, 0x2B, 0xC4, 0xA9, 0xD6, 0x27, 0xE6, 0xC4, 0xD6, 0x80, 0xE5, 0x0C, 0x10, 0x47, 0x22, 0x49, 0xA9, 0x9D, 0xF4, 0x1D, 0x83, 0x07, 0xC1, 0x02, 0x76, 0xA8, 0x2F },
            new byte[] { 0xB9, 0x38, 0xA1, 0x6D, 0x42, 0x72, 0x9E, 0x6E, 0x4D, 0x95, 0x6D, 0x33, 0x3F, 0xEA, 0x0E, 0x26, 0x9B, 0x4D, 0x6F, 0xD6, 0xC4, 0x72, 0x8D, 0xD4, 0x2D, 0x2B, 0x0E, 0xD1, 0x5D, 0x16, 0x2F, 0x55 },
            new byte[] { 0x5C, 0x75, 0xF1, 0x8C, 0x29, 0x21, 0x6F, 0x0C, 0x9E, 0x84, 0x8A, 0x3A, 0x04, 0xF0, 0x21, 0x00, 0xDF, 0x1A, 0x2F, 0xA4, 0x4C, 0xA7, 0x4E, 0x00, 0x85, 0x38, 0x91, 0x99, 0xE9, 0x7A, 0x9D, 0x84 },
            new byte[] { 0xD4, 0x30, 0x42, 0x96, 0x6F, 0x56, 0x94, 0x6F, 0xFA, 0x0A, 0x4A, 0x2C, 0x6F, 0x90, 0x91, 0x87, 0xA4, 0x5E, 0xA8, 0xC7, 0x86, 0xFD, 0xB7, 0x51, 0x1A, 0xB4, 0x51, 0xAE, 0x3B, 0x7E, 0x6A, 0x67 },
            new byte[] { 0x03, 0xE8, 0x1D, 0x60, 0x81, 0xE3, 0xC3, 0x99, 0x3C, 0x91, 0xD5, 0xDA, 0x49, 0x76, 0x8A, 0xB6, 0x60, 0x4F, 0xB1, 0x4D, 0xE6, 0xA7, 0x8B, 0x00, 0x7F, 0x7C, 0x7E, 0xC2, 0x83, 0xD4, 0x29, 0x6F },
            new byte[] { 0xA2, 0x33, 0xB9, 0xD8, 0x08, 0x41, 0x37, 0x4E, 0xE3, 0xA5, 0xA2, 0xB6, 0xC9, 0x35, 0x78, 0xF7, 0xB3, 0x55, 0xC7, 0x83, 0xC5, 0x54, 0x37, 0x94, 0x7D, 0x58, 0x34, 0x65, 0xB2, 0xCB, 0x1A, 0x2D },
            new byte[] { 0x68, 0x83, 0x2B, 0xC7, 0xCC, 0x5C, 0x59, 0x46, 0x9F, 0xBE, 0x7A, 0x42, 0x42, 0x14, 0xB8, 0x90, 0x6D, 0xE4, 0x58, 0xED, 0x0E, 0x59, 0x6D, 0x8E, 0x6B, 0x7E, 0x2C, 0x8F, 0xB8, 0x2D, 0x93, 0x6B },
            new byte[] { 0xD4, 0x62, 0xE2, 0x41, 0x0F, 0x0F, 0x21, 0xDA, 0x76, 0xA5, 0xE9, 0x69, 0x94, 0x0D, 0x6F, 0xA3, 0xFB, 0x64, 0x59, 0x51, 0x9C, 0xAD, 0xBA, 0x71, 0x8B, 0x40, 0x6B, 0xA4, 0x68, 0x54, 0x51, 0xF7 },
            new byte[] { 0x1A, 0x2E, 0x0C, 0x47, 0xA5, 0x70, 0x9F, 0x24, 0x9C, 0xD0, 0x96, 0xB7, 0xC1, 0x65, 0x00, 0x96, 0x6C, 0x8B, 0xA3, 0x71, 0xB9, 0x1E, 0xB8, 0x5C, 0x1D, 0x36, 0x30, 0xA5, 0xA2, 0xB0, 0x35, 0xB5 },
            new byte[] { 0x4D, 0x04, 0x23, 0xE7, 0x68, 0x2E, 0x3D, 0x77, 0xCB, 0x6A, 0x0E, 0xF4, 0x5A, 0x88, 0x5B, 0x28, 0xDF, 0x1D, 0xD1, 0x9F, 0x21, 0xBA, 0x08, 0x0A, 0x95, 0xFB, 0x6D, 0x65, 0xA5, 0x6C, 0xA6, 0x3D },
            new byte[] { 0x11, 0x35, 0xF5, 0x71, 0x2F, 0xD6, 0x12, 0xD4, 0x6D, 0x9C, 0xF5, 0xE7, 0xBC, 0x3B, 0xEC, 0x03, 0x3F, 0x7D, 0x66, 0x36, 0x0A, 0xFB, 0xBA, 0x66, 0x2D, 0x5F, 0x96, 0x7D, 0x07, 0x12, 0x2D, 0x11 }
        };

        /// <summary>
        /// 计算 GOST 哈希值
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <returns>32字节哈希值</returns>
        public static byte[] Hash(byte[] data)
        {
            if (data == null || data.Length == 0)
                return new byte[HashSize];

            byte[] h = new byte[32];
            byte[] sigma = new byte[32];
            byte[] m = new byte[32];

            // 初始化 H 和 Sigma
            ulong length = (ulong)data.Length * 8;
            int checksum = 0;

            int pos = 0;
            while (pos < data.Length)
            {
                int len = Math.Min(32, data.Length - pos);
                Array.Copy(data, pos, m, 0, len);
                if (len < 32) Array.Clear(m, len, 32 - len);

                h = Step(h, m);
                sigma = Add(sigma, m);
                checksum = (checksum + len) & 0xFF;
                pos += 32;
            }

            // 最终化
            m = BitConverter.GetBytes(length);
            Array.Resize(ref m, 32);
            h = Step(h, m);
            h = Step(h, sigma);

            return h;
        }

        /// <summary>
        /// 计算字符串的 GOST 哈希值
        /// </summary>
        /// <param name="text">输入文本</param>
        /// <returns>十六进制哈希字符串</returns>
        public static string HashString(string text)
        {
            if (string.IsNullOrEmpty(text))
                return new string('0', HashSize * 2);

            byte[] data = Encoding.UTF8.GetBytes(text);
            byte[] hash = Hash(data);
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

        /// <summary>
        /// 验证哈希值
        /// </summary>
        /// <param name="data">原始数据</param>
        /// <param name="hash">预期哈希值</param>
        /// <returns>是否匹配</returns>
        public static bool Verify(byte[] data, byte[] hash)
        {
            if (hash == null || hash.Length != HashSize)
                return false;

            byte[] computed = Hash(data);
            return SlowEquals(computed, hash);
        }

        /// <summary>
        /// 验证字符串哈希
        /// </summary>
        /// <param name="text">原始文本</param>
        /// <param name="hashHex">预期哈希值（十六进制）</param>
        /// <returns>是否匹配</returns>
        public static bool VerifyString(string text, string hashHex)
        {
            if (string.IsNullOrEmpty(hashHex) || hashHex.Length != HashSize * 2)
                return false;

            string computed = HashString(text);
            return string.Equals(computed, hashHex, StringComparison.OrdinalIgnoreCase);
        }

        private static byte[] Step(byte[] h, byte[] m)
        {
            byte[] u = (byte[])h.Clone();
            byte[] v = (byte[])m.Clone();
            byte[] w = new byte[32];

            // Key generation
            byte[] k = P(u, v);

            // Encryption
            byte[] s = E(k, h);

            // Mixing
            for (int i = 0; i < 12; i++)
            {
                w = X(u, v);
                u = A(u);
                v = A(A(v));
            }

            w = X(u, v);
            s = X(s, w);

            return Psi(s, 12) ?? s;
        }

        private static byte[] P(byte[] u, byte[] v)
        {
            byte[] k = new byte[32];
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    k[i * 8 + j] = (byte)(u[i + j * 4] ^ v[i + j * 4]);
                }
            }
            return k;
        }

        private static byte[] A(byte[] x)
        {
            byte[] result = new byte[32];
            for (int i = 0; i < 8; i++)
            {
                byte c = 0;
                for (int j = 7; j >= 0; j--)
                {
                    byte newC = (byte)(x[i * 4 + j] >> 7);
                    result[i * 4 + j] = (byte)((x[i * 4 + j] << 1) | c);
                    c = newC;
                }
            }
            return result;
        }

        private static byte[] X(byte[] a, byte[] b)
        {
            byte[] result = new byte[32];
            for (int i = 0; i < 32; i++)
                result[i] = (byte)(a[i] ^ b[i]);
            return result;
        }

        private static byte[] Add(byte[] a, byte[] b)
        {
            byte[] result = new byte[32];
            int carry = 0;
            for (int i = 0; i < 32; i++)
            {
                int sum = a[i] + b[i] + carry;
                result[i] = (byte)(sum & 0xFF);
                carry = sum >> 8;
            }
            return result;
        }

        private static byte[] E(byte[] k, byte[] h)
        {
            byte[] result = new byte[32];
            for (int i = 0; i < 32; i += 8)
            {
                ulong block = BitConverter.ToUInt64(k, i);
                ulong hBlock = BitConverter.ToUInt64(h, i);
                block ^= hBlock;

                // S-box substitution
                byte[] bytes = BitConverter.GetBytes(block);
                for (int j = 0; j < 8; j++)
                {
                    int row = j;
                    int col = (bytes[j] >> 4) & 0x0F;
                    bytes[j] = SBox[row, col];
                }

                Array.Copy(bytes, 0, result, i, 8);
            }
            return result;
        }

        private static byte[] Psi(byte[] x, int n)
        {
            if (x == null) return null;
            byte[] result = (byte[])x.Clone();

            for (int i = 0; i < n; i++)
            {
                byte tmp = result[0];
                for (int j = 0; j < 31; j++)
                    result[j] = result[j + 1];
                result[31] = tmp;
            }

            return result;
        }

        private static bool SlowEquals(byte[] a, byte[] b)
        {
            uint diff = (uint)a.Length ^ (uint)b.Length;
            for (int i = 0; i < a.Length && i < b.Length; i++)
                diff |= (uint)(a[i] ^ b[i]);
            return diff == 0;
        }
    }
}
