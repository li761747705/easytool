using System;
using System.Text;

namespace EasyTool.CodeCategory
{
    /// <summary>
    /// SM3 密码哈希算法工具类
    /// SM3 是中国国家密码管理局发布的密码哈希函数标准
    /// 输出256位（32字节）哈希值，安全性类似于 SHA-256
    /// </summary>
    public static class Sm3Util
    {
        // SM3 初始向量
        private static readonly uint[] IV = new uint[]
        {
            0x7380166f, 0x4914b2b9, 0x172442d7, 0xda8a0600,
            0xa96f30bc, 0x163138aa, 0xe38dee4d, 0xb0fb0e4e
        };

        /// <summary>
        /// 计算数据的 SM3 哈希值
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <returns>32字节的哈希值</returns>
        public static byte[] ComputeHash(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            return ComputeHash(data, 0, data.Length);
        }

        /// <summary>
        /// 计算数据的 SM3 哈希值
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <param name="offset">起始位置</param>
        /// <param name="length">长度</param>
        /// <returns>32字节的哈希值</returns>
        public static byte[] ComputeHash(byte[] data, int offset, int length)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (offset < 0 || offset >= data.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (length < 0 || offset + length > data.Length)
                throw new ArgumentOutOfRangeException(nameof(length));

            // 填充消息
            byte[] padded = PadMessage(data, offset, length);

            // 初始化哈希值
            uint[] v = new uint[8];
            Array.Copy(IV, v, 8);

            // 处理每个512位块
            for (int i = 0; i < padded.Length; i += 64)
            {
                ProcessBlock(padded, i, v);
            }

            // 转换为字节数组
            byte[] result = new byte[32];
            for (int i = 0; i < 8; i++)
            {
                result[i * 4] = (byte)(v[i] >> 24);
                result[i * 4 + 1] = (byte)(v[i] >> 16);
                result[i * 4 + 2] = (byte)(v[i] >> 8);
                result[i * 4 + 3] = (byte)v[i];
            }

            return result;
        }

        /// <summary>
        /// 计算字符串的 SM3 哈希值
        /// </summary>
        /// <param name="text">输入字符串</param>
        /// <param name="encoding">编码方式（默认UTF-8）</param>
        /// <returns>32字节的哈希值</returns>
        public static byte[] ComputeHash(string text, Encoding encoding = null)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            encoding ??= Encoding.UTF8;
            return ComputeHash(encoding.GetBytes(text));
        }

        /// <summary>
        /// 计算数据的 SM3 哈希值并返回十六进制字符串
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <returns>64字符的十六进制字符串</returns>
        public static string ComputeHashHex(byte[] data)
        {
            byte[] hash = ComputeHash(data);
            return BytesToHex(hash);
        }

        /// <summary>
        /// 计算字符串的 SM3 哈希值并返回十六进制字符串
        /// </summary>
        /// <param name="text">输入字符串</param>
        /// <param name="encoding">编码方式（默认UTF-8）</param>
        /// <returns>64字符的十六进制字符串</returns>
        public static string ComputeHashHex(string text, Encoding encoding = null)
        {
            byte[] hash = ComputeHash(text, encoding);
            return BytesToHex(hash);
        }

        /// <summary>
        /// 验证数据哈希值
        /// </summary>
        /// <param name="data">原始数据</param>
        /// <param name="expectedHash">预期的哈希值</param>
        /// <returns>是否匹配</returns>
        public static bool Verify(byte[] data, byte[] expectedHash)
        {
            if (expectedHash == null || expectedHash.Length != 32)
                return false;

            byte[] computed = ComputeHash(data);
            return ConstantTimeEquals(computed, expectedHash);
        }

        /// <summary>
        /// 验证数据哈希值（十六进制格式）
        /// </summary>
        /// <param name="data">原始数据</param>
        /// <param name="expectedHashHex">预期的哈希值（十六进制）</param>
        /// <returns>是否匹配</returns>
        public static bool VerifyHex(byte[] data, string expectedHashHex)
        {
            if (string.IsNullOrEmpty(expectedHashHex) || expectedHashHex.Length != 64)
                return false;

            string computed = ComputeHashHex(data);
            return string.Equals(computed, expectedHashHex, StringComparison.OrdinalIgnoreCase);
        }

        #region 私有方法

        private static byte[] PadMessage(byte[] data, int offset, int length)
        {
            // 计算填充后的长度
            long bitLength = (long)length * 8;
            int paddedLength = length + 1 + 8;

            // 使长度为64的倍数
            while (paddedLength % 64 != 0)
            {
                paddedLength++;
            }

            byte[] padded = new byte[paddedLength];
            Array.Copy(data, offset, padded, 0, length);

            // 添加1位和7个0位（0x80）
            padded[length] = 0x80;

            // 添加长度（大端序，64位）
            padded[paddedLength - 8] = (byte)(bitLength >> 56);
            padded[paddedLength - 7] = (byte)(bitLength >> 48);
            padded[paddedLength - 6] = (byte)(bitLength >> 40);
            padded[paddedLength - 5] = (byte)(bitLength >> 32);
            padded[paddedLength - 4] = (byte)(bitLength >> 24);
            padded[paddedLength - 3] = (byte)(bitLength >> 16);
            padded[paddedLength - 2] = (byte)(bitLength >> 8);
            padded[paddedLength - 1] = (byte)bitLength;

            return padded;
        }

        private static void ProcessBlock(byte[] block, int offset, uint[] v)
        {
            uint[] w = new uint[68];
            uint[] w1 = new uint[64];

            // 准备消息扩展
            for (int i = 0; i < 16; i++)
            {
                w[i] = ((uint)block[offset + i * 4] << 24) |
                       ((uint)block[offset + i * 4 + 1] << 16) |
                       ((uint)block[offset + i * 4 + 2] << 8) |
                       block[offset + i * 4 + 3];
            }

            for (int i = 16; i < 68; i++)
            {
                w[i] = P1(w[i - 16] ^ w[i - 9] ^ RotateLeft(w[i - 3], 15)) ^
                       RotateLeft(w[i - 13], 7) ^ w[i - 6];
                if (w[i] < 0) w[i] = (uint)(int)w[i];
            }

            for (int i = 0; i < 64; i++)
            {
                w1[i] = w[i] ^ w[i + 4];
            }

            // 压缩函数
            uint a = v[0], b = v[1], c = v[2], d = v[3];
            uint e = v[4], f = v[5], g = v[6], h = v[7];

            for (int i = 0; i < 64; i++)
            {
                uint ss1 = RotateLeft(RotateLeft(a, 12) + e + RotateLeft(T(i), i % 32), 7);
                uint ss2 = ss1 ^ RotateLeft(a, 12);
                uint tt1 = FF(a, b, c, i) + d + ss2 + w1[i];
                uint tt2 = GG(e, f, g, i) + h + ss1 + w[i];

                d = c;
                c = RotateLeft(b, 9);
                b = a;
                a = tt1;
                h = g;
                g = RotateLeft(f, 19);
                f = e;
                e = P0(tt2);
            }

            v[0] ^= a;
            v[1] ^= b;
            v[2] ^= c;
            v[3] ^= d;
            v[4] ^= e;
            v[5] ^= f;
            v[6] ^= g;
            v[7] ^= h;
        }

        private static uint RotateLeft(uint x, int n)
        {
            return (x << n) | (x >> (32 - n));
        }

        private static uint T(int j)
        {
            return j < 16 ? 0x79cc4519u : 0x7a879d8au;
        }

        private static uint FF(uint x, uint y, uint z, int j)
        {
            if (j < 16)
            {
                return x ^ y ^ z;
            }
            return (x & y) | (x & z) | (y & z);
        }

        private static uint GG(uint x, uint y, uint z, int j)
        {
            if (j < 16)
            {
                return x ^ y ^ z;
            }
            return (x & y) | (~x & z);
        }

        private static uint P0(uint x)
        {
            return x ^ RotateLeft(x, 9) ^ RotateLeft(x, 17);
        }

        private static uint P1(uint x)
        {
            return x ^ RotateLeft(x, 15) ^ RotateLeft(x, 23);
        }

        private static string BytesToHex(byte[] bytes)
        {
            var sb = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }

        private static bool ConstantTimeEquals(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
                return false;

            int result = 0;
            for (int i = 0; i < a.Length; i++)
            {
                result |= a[i] ^ b[i];
            }
            return result == 0;
        }

        #endregion

        #region HMAC-SM3

        /// <summary>
        /// 计算 HMAC-SM3
        /// </summary>
        /// <param name="key">密钥</param>
        /// <param name="data">数据</param>
        /// <returns>32字节的HMAC值</returns>
        public static byte[] Hmac(byte[] key, byte[] data)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            // 如果密钥太长，先哈希
            if (key.Length > 64)
            {
                key = ComputeHash(key);
            }

            // 填充密钥到64字节
            byte[] paddedKey = new byte[64];
            Array.Copy(key, paddedKey, key.Length);

            // 计算内部和外部填充
            byte[] innerPad = new byte[64];
            byte[] outerPad = new byte[64];

            for (int i = 0; i < 64; i++)
            {
                innerPad[i] = (byte)(paddedKey[i] ^ 0x36);
                outerPad[i] = (byte)(paddedKey[i] ^ 0x5c);
            }

            // 计算 HMAC
            byte[] innerData = new byte[64 + data.Length];
            Array.Copy(innerPad, innerData, 64);
            Array.Copy(data, 0, innerData, 64, data.Length);
            byte[] innerHash = ComputeHash(innerData);

            byte[] outerData = new byte[64 + 32];
            Array.Copy(outerPad, outerData, 64);
            Array.Copy(innerHash, 0, outerData, 64, 32);

            return ComputeHash(outerData);
        }

        /// <summary>
        /// 计算 HMAC-SM3 并返回十六进制字符串
        /// </summary>
        /// <param name="key">密钥</param>
        /// <param name="data">数据</param>
        /// <returns>64字符的十六进制字符串</returns>
        public static string HmacHex(byte[] key, byte[] data)
        {
            byte[] hmac = Hmac(key, data);
            return BytesToHex(hmac);
        }

        #endregion
    }
}
