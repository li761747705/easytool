using System;

namespace EasyTool.CodeCategory
{
    /// <summary>
    /// CityHash 哈希工具类
    /// CityHash 是 Google 开发的高性能哈希算法
    /// 适用于字符串哈希，不适用于密码存储
    /// </summary>
    public static class CityHashUtil
    {
        private const ulong K0 = 0xc3a5c85c97cb3127;
        private const ulong K1 = 0xb492b66fbe98f273;
        private const ulong K2 = 0x9ae16a3b2f90404f;
        private const ulong K3 = 0xc949d7c7509e6557;

        /// <summary>
        /// 计算 CityHash64 哈希值
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <returns>64位哈希值</returns>
        public static ulong ComputeHash64(byte[] data)
        {
            if (data == null || data.Length == 0)
                return 0;

            return CityHash64(data, 0, (uint)data.Length);
        }

        /// <summary>
        /// 计算 CityHash64 哈希值（指定偏移和长度）
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <param name="offset">起始偏移</param>
        /// <param name="length">数据长度</param>
        /// <returns>64位哈希值</returns>
        public static ulong ComputeHash64(byte[] data, int offset, int length)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (offset < 0 || offset > data.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (length < 0 || offset + length > data.Length)
                throw new ArgumentOutOfRangeException(nameof(length));

            if (length == 0)
                return 0;

            return CityHash64(data, (uint)offset, (uint)length);
        }

        /// <summary>
        /// 计算 CityHash128 哈希值
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <returns>128位哈希值（两个64位值）</returns>
        public static (ulong Low, ulong High) ComputeHash128(byte[] data)
        {
            if (data == null || data.Length == 0)
                return (0, 0);

            return CityHash128(data, 0, (uint)data.Length);
        }

        /// <summary>
        /// 计算字符串的 CityHash64 哈希值
        /// </summary>
        /// <param name="text">文本</param>
        /// <returns>64位哈希值</returns>
        public static ulong ComputeString64(string text)
        {
            if (string.IsNullOrEmpty(text))
                return 0;

            byte[] data = System.Text.Encoding.UTF8.GetBytes(text);
            return ComputeHash64(data);
        }

        /// <summary>
        /// 计算字符串的 CityHash128 哈希值
        /// </summary>
        /// <param name="text">文本</param>
        /// <returns>128位哈希值</returns>
        public static (ulong Low, ulong High) ComputeString128(string text)
        {
            if (string.IsNullOrEmpty(text))
                return (0, 0);

            byte[] data = System.Text.Encoding.UTF8.GetBytes(text);
            return ComputeHash128(data);
        }

        /// <summary>
        /// 获取 CityHash64 哈希值的十六进制表示
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <returns>16字符的十六进制字符串</returns>
        public static string ComputeHex64(byte[] data)
        {
            ulong hash = ComputeHash64(data);
            return hash.ToString("x16");
        }

        /// <summary>
        /// 获取 CityHash128 哈希值的十六进制表示
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <returns>32字符的十六进制字符串</returns>
        public static string ComputeHex128(byte[] data)
        {
            var (low, high) = ComputeHash128(data);
            return high.ToString("x16") + low.ToString("x16");
        }

        /// <summary>
        /// 使用种子计算 CityHash64 哈希值
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <param name="seed">种子值</param>
        /// <returns>64位哈希值</returns>
        public static ulong ComputeHash64WithSeed(byte[] data, ulong seed)
        {
            if (data == null || data.Length == 0)
                return seed;

            return CityHash64WithSeed(data, 0, (uint)data.Length, seed);
        }

        #region 私有方法

        private static ulong CityHash64(byte[] data, uint offset, uint length)
        {
            if (length <= 16)
            {
                return HashLen0to16(data, offset, length);
            }
            else if (length <= 32)
            {
                return HashLen17to32(data, offset, length);
            }
            else if (length <= 64)
            {
                return HashLen33to64(data, offset, length);
            }
            else
            {
                return HashLenOver64(data, offset, length);
            }
        }

        private static (ulong Low, ulong High) CityHash128(byte[] data, uint offset, uint length)
        {
            if (length >= 16)
            {
                return CityHash128WithSeed(data, offset, length,
                    ReadUInt64(data, offset),
                    ReadUInt64(data, offset + 8));
            }
            else
            {
                return CityHash128WithSeed(data, offset, length, K0, K1);
            }
        }

        private static ulong CityHash64WithSeed(byte[] data, uint offset, uint length, ulong seed)
        {
            return CityHash64WithSeeds(data, offset, length, K2, seed);
        }

        private static ulong CityHash64WithSeeds(byte[] data, uint offset, uint length, ulong seed0, ulong seed1)
        {
            return HashLen16(CityHash64(data, offset, length) - seed0, seed1);
        }

        private static (ulong Low, ulong High) CityHash128WithSeed(byte[] data, uint offset, uint length, ulong seed0, ulong seed1)
        {
            if (length < 128)
            {
                return CityMurmur(data, offset, length, seed0, seed1);
            }

            ulong x = seed0;
            ulong y = seed1;
            ulong z = length * K1;

            uint end = offset + length;
            uint pos = offset;

            while (pos + 128 <= end)
            {
                x = RotateRight(x + y + ReadUInt64(data, pos + 8) + ReadUInt64(data, pos + 48), 43) +
                    RotateRight(ReadUInt64(data, pos + 40), 25) + ReadUInt64(data, pos);
                y = RotateRight(y + ReadUInt64(data, pos + 16) + ReadUInt64(data, pos + 56), 36) +
                    RotateRight(ReadUInt64(data, pos + 24) + ReadUInt64(data, pos + 32), 19) +
                    ReadUInt64(data, pos + 8);
                z = RotateRight(z + ReadUInt64(data, pos + 64) + ReadUInt64(data, pos + 88), 27) +
                    RotateRight(ReadUInt64(data, pos + 72) + ReadUInt64(data, pos + 104), 31) +
                    ReadUInt64(data, pos + 80);
                pos += 128;
            }

            z += RotateRight(z, 55);
            z += RotateRight(x, 25);
            z += RotateRight(y, 36);

            return CityMurmur(data, pos, end - pos, z, K2);
        }

        private static (ulong Low, ulong High) CityMurmur(byte[] data, uint offset, uint length, ulong seed0, ulong seed1)
        {
            ulong a = seed0;
            ulong b = seed1;
            ulong c = 0;
            ulong d = 0;

            if (length <= 16)
            {
                a = ShiftMix(a * K1) * K1;
                c = b * K1 + HashLen0to16(data, offset, length);
                d = ShiftMix(a + (length >= 8 ? ReadUInt64(data, offset) : c));
            }
            else
            {
                c = HashLen16(ReadUInt64(data, offset + length - 8) + K1, a);
                d = HashLen16(b + length, c + ReadUInt64(data, offset + length - 16));
                a += d;

                uint end = offset + length - 16;
                uint pos = offset;

                do
                {
                    a ^= ShiftMix(ReadUInt64(data, pos) * K1) * K1;
                    a *= K1;
                    b ^= a;
                    c ^= ShiftMix(ReadUInt64(data, pos + 8) * K1) * K1;
                    c *= K1;
                    d ^= c;
                    pos += 16;
                } while (pos < end);
            }

            a = HashLen16(a, c);
            b = HashLen16(d, b);

            return (a ^ b, HashLen16(c, a));
        }

        private static ulong HashLen0to16(byte[] data, uint offset, uint length)
        {
            if (length >= 8)
            {
                ulong mul = K2 + length * 2;
                ulong a = ReadUInt64(data, offset) + K2;
                ulong b = ReadUInt64(data, offset + length - 8);
                ulong c = RotateRight(b, 37) * mul + a;
                ulong d = (RotateRight(a, 25) + b) * mul;
                return HashLen16(c, d, mul);
            }

            if (length >= 4)
            {
                ulong mul = K2 + length * 2;
                ulong a = ReadUInt32(data, offset);
                return HashLen16(length + (a << 3), ReadUInt32(data, offset + length - 4), mul);
            }

            if (length > 0)
            {
                byte a = data[offset];
                byte b = data[offset + (length >> 1)];
                byte c = data[offset + length - 1];
                uint y = a + ((uint)b << 8);
                uint z = length + ((uint)c << 2);
                return ShiftMix(y * K2 ^ z * K3) * K2;
            }

            return K2;
        }

        private static ulong HashLen17to32(byte[] data, uint offset, uint length)
        {
            ulong mul = K2 + length * 2;
            ulong a = ReadUInt64(data, offset) * K1;
            ulong b = ReadUInt64(data, offset + 8);
            ulong c = ReadUInt64(data, offset + length - 8) * mul;
            ulong d = ReadUInt64(data, offset + length - 16) * K2;

            return HashLen16(RotateRight(a + b, 43) + RotateRight(c, 30) + d,
                            a + RotateRight(b + K2, 18) + c, mul);
        }

        private static ulong HashLen33to64(byte[] data, uint offset, uint length)
        {
            ulong mul = K2 + length * 2;
            ulong a = ReadUInt64(data, offset) * K2;
            ulong b = ReadUInt64(data, offset + 8);
            ulong c = ReadUInt64(data, offset + length - 8) * mul;
            ulong d = ReadUInt64(data, offset + length - 16) * K2;
            ulong y = ReadUInt64(data, offset + 16) * mul;
            ulong z = ReadUInt64(data, offset + 24) * 9;
            ulong e = RotateRight(a + y, 43) + RotateRight(b, 30) + c;
            ulong f = a + RotateRight(y + z, 18) + d;

            return HashLen16(e + f, HashLen16(e, f, mul), mul);
        }

        private static ulong HashLenOver64(byte[] data, uint offset, uint length)
        {
            ulong x = ReadUInt64(data, offset);
            ulong y = ReadUInt64(data, offset + length - 16) ^ K1;
            ulong z = ReadUInt64(data, offset + length - 8);

            uint pos = offset;
            uint end = offset + length - 16;

            while (pos + 16 <= end)
            {
                x = RotateRight(x + ReadUInt64(data, pos + 8), 43) + RotateRight(ReadUInt64(data, pos), 25);
                y = RotateRight(y + ReadUInt64(data, pos + 8), 36);
                z = RotateRight(z + ReadUInt64(data, pos), 27);
                pos += 16;
            }

            return HashLen16(HashLen16(x, z, K2) + y, HashLen16(y, K2 + length, K2), K2);
        }

        private static ulong HashLen16(ulong u, ulong v)
        {
            return HashLen16(u, v, K2);
        }

        private static ulong HashLen16(ulong u, ulong v, ulong mul)
        {
            ulong a = (u ^ v) * mul;
            a ^= (a >> 47);
            ulong b = (v ^ a) * mul;
            b ^= (b >> 47);
            b *= mul;
            return b;
        }

        private static ulong ReadUInt64(byte[] data, uint offset)
        {
            return BitConverter.ToUInt64(data, (int)offset);
        }

        private static uint ReadUInt32(byte[] data, uint offset)
        {
            return BitConverter.ToUInt32(data, (int)offset);
        }

        private static ulong RotateRight(ulong x, int n)
        {
            return (x >> n) | (x << (64 - n));
        }

        private static ulong ShiftMix(ulong x)
        {
            return x ^ (x >> 47);
        }

        #endregion
    }
}
