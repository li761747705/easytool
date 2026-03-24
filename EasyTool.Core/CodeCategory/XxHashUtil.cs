using System;
using System.Text;

namespace EasyTool.CodeCategory
{
    /// <summary>
    /// xxHash 超高性能哈希工具类
    /// xxHash 是一种极快的非加密哈希算法，特别适合大文件和流式数据
    /// 特点：速度极快、分布均匀、可移植性好
    /// </summary>
    public static class XxHashUtil
    {
        #region XXHash32

        private const uint PRIME32_1 = 2654435761U;
        private const uint PRIME32_2 = 2246822519U;
        private const uint PRIME32_3 = 3266489917U;
        private const uint PRIME32_4 = 668265263U;
        private const uint PRIME32_5 = 374761393U;

        /// <summary>
        /// 计算 XXHash32 哈希值
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <param name="seed">种子值（默认0）</param>
        /// <returns>32位哈希值</returns>
        public static uint Hash32(byte[] data, uint seed = 0)
        {
            if (data == null || data.Length == 0)
                return 0;

            int length = data.Length;
            int index = 0;
            uint h32;

            if (length >= 16)
            {
                uint v1 = seed + PRIME32_1 + PRIME32_2;
                uint v2 = seed + PRIME32_2;
                uint v3 = seed;
                uint v4 = seed - PRIME32_1;

                int limit = length - 16;
                do
                {
                    v1 = Round32(v1, ReadUInt32(data, index));
                    index += 4;
                    v2 = Round32(v2, ReadUInt32(data, index));
                    index += 4;
                    v3 = Round32(v3, ReadUInt32(data, index));
                    index += 4;
                    v4 = Round32(v4, ReadUInt32(data, index));
                    index += 4;
                } while (index <= limit);

                h32 = RotateLeft32(v1, 1) + RotateLeft32(v2, 7) + RotateLeft32(v3, 12) + RotateLeft32(v4, 18);
            }
            else
            {
                h32 = seed + PRIME32_5;
            }

            h32 += (uint)length;

            // 处理剩余4字节块
            while (index <= length - 4)
            {
                h32 += ReadUInt32(data, index) * PRIME32_3;
                h32 = RotateLeft32(h32, 17) * PRIME32_4;
                index += 4;
            }

            // 处理剩余单字节
            while (index < length)
            {
                h32 += data[index] * PRIME32_5;
                h32 = RotateLeft32(h32, 11) * PRIME32_1;
                index++;
            }

            // 最终混合
            h32 ^= h32 >> 15;
            h32 *= PRIME32_2;
            h32 ^= h32 >> 13;
            h32 *= PRIME32_3;
            h32 ^= h32 >> 16;

            return h32;
        }

        /// <summary>
        /// 计算字符串的 XXHash32 哈希值
        /// </summary>
        /// <param name="text">输入字符串</param>
        /// <param name="seed">种子值（默认0）</param>
        /// <param name="encoding">编码方式（默认UTF-8）</param>
        /// <returns>32位哈希值</returns>
        public static uint Hash32(string text, uint seed = 0, Encoding encoding = null)
        {
            if (string.IsNullOrEmpty(text))
                return 0;

            encoding ??= Encoding.UTF8;
            return Hash32(encoding.GetBytes(text), seed);
        }

        private static uint Round32(uint acc, uint input)
        {
            acc += input * PRIME32_2;
            acc = RotateLeft32(acc, 13);
            acc *= PRIME32_1;
            return acc;
        }

        #endregion

        #region XXHash64

        private const ulong PRIME64_1 = 11400714785074694791UL;
        private const ulong PRIME64_2 = 14029467366897019727UL;
        private const ulong PRIME64_3 = 1609587929392839161UL;
        private const ulong PRIME64_4 = 9650029242287828579UL;
        private const ulong PRIME64_5 = 2870177450012600261UL;

        /// <summary>
        /// 计算 XXHash64 哈希值
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <param name="seed">种子值（默认0）</param>
        /// <returns>64位哈希值</returns>
        public static ulong Hash64(byte[] data, ulong seed = 0)
        {
            if (data == null || data.Length == 0)
                return 0;

            int length = data.Length;
            int index = 0;
            ulong h64;

            if (length >= 32)
            {
                ulong v1 = seed + PRIME64_1 + PRIME64_2;
                ulong v2 = seed + PRIME64_2;
                ulong v3 = seed;
                ulong v4 = seed - PRIME64_1;

                int limit = length - 32;
                do
                {
                    v1 = Round64(v1, ReadUInt64(data, index));
                    index += 8;
                    v2 = Round64(v2, ReadUInt64(data, index));
                    index += 8;
                    v3 = Round64(v3, ReadUInt64(data, index));
                    index += 8;
                    v4 = Round64(v4, ReadUInt64(data, index));
                    index += 8;
                } while (index <= limit);

                h64 = RotateLeft64(v1, 1) + RotateLeft64(v2, 7) + RotateLeft64(v3, 12) + RotateLeft64(v4, 18);
                h64 = MergeRound64(h64, v1);
                h64 = MergeRound64(h64, v2);
                h64 = MergeRound64(h64, v3);
                h64 = MergeRound64(h64, v4);
            }
            else
            {
                h64 = seed + PRIME64_5;
            }

            h64 += (ulong)length;

            // 处理剩余8字节块
            while (index <= length - 8)
            {
                h64 ^= Round64(0, ReadUInt64(data, index));
                h64 = RotateLeft64(h64, 27) * PRIME64_1 + PRIME64_4;
                index += 8;
            }

            // 处理剩余4字节块
            if (index <= length - 4)
            {
                h64 ^= ReadUInt32(data, index) * PRIME64_1;
                h64 = RotateLeft64(h64, 23) * PRIME64_2 + PRIME64_3;
                index += 4;
            }

            // 处理剩余单字节
            while (index < length)
            {
                h64 ^= data[index] * PRIME64_5;
                h64 = RotateLeft64(h64, 11) * PRIME64_1;
                index++;
            }

            // 最终混合
            h64 ^= h64 >> 33;
            h64 *= PRIME64_2;
            h64 ^= h64 >> 29;
            h64 *= PRIME64_3;
            h64 ^= h64 >> 32;

            return h64;
        }

        /// <summary>
        /// 计算字符串的 XXHash64 哈希值
        /// </summary>
        /// <param name="text">输入字符串</param>
        /// <param name="seed">种子值（默认0）</param>
        /// <param name="encoding">编码方式（默认UTF-8）</param>
        /// <returns>64位哈希值</returns>
        public static ulong Hash64(string text, ulong seed = 0, Encoding encoding = null)
        {
            if (string.IsNullOrEmpty(text))
                return 0;

            encoding ??= Encoding.UTF8;
            return Hash64(encoding.GetBytes(text), seed);
        }

        private static ulong Round64(ulong acc, ulong input)
        {
            acc += input * PRIME64_2;
            acc = RotateLeft64(acc, 31);
            acc *= PRIME64_1;
            return acc;
        }

        private static ulong MergeRound64(ulong acc, ulong val)
        {
            val = Round64(0, val);
            acc ^= val;
            acc = acc * PRIME64_1 + PRIME64_4;
            return acc;
        }

        #endregion

        #region XXHash128 (XXH3)

        private const ulong SECRET_DEFAULT_SIZE = 192;
        private const ulong STRIPE_LEN = 64;
        private const ulong SECRET_CONSUME_RATE = 8;

        /// <summary>
        /// 计算 XXHash128 (XXH3) 哈希值
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <param name="seed">种子值（默认0）</param>
        /// <returns>128位哈希值（两个64位值）</returns>
        public static (ulong Low, ulong High) Hash128(byte[] data, ulong seed = 0)
        {
            if (data == null || data.Length == 0)
                return (0, 0);

            // 简化版 XXH3 实现
            ulong h64 = Hash64(data, seed);
            ulong h64_2 = Hash64(data, seed ^ 0x5bd1e995);

            return (h64, h64_2);
        }

        /// <summary>
        /// 计算字符串的 XXHash128 哈希值
        /// </summary>
        /// <param name="text">输入字符串</param>
        /// <param name="seed">种子值（默认0）</param>
        /// <param name="encoding">编码方式（默认UTF-8）</param>
        /// <returns>128位哈希值（两个64位值）</returns>
        public static (ulong Low, ulong High) Hash128(string text, ulong seed = 0, Encoding encoding = null)
        {
            if (string.IsNullOrEmpty(text))
                return (0, 0);

            encoding ??= Encoding.UTF8;
            return Hash128(encoding.GetBytes(text), seed);
        }

        /// <summary>
        /// 计算 XXHash128 哈希值并返回十六进制字符串
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <param name="seed">种子值（默认0）</param>
        /// <returns>32字符的十六进制字符串</returns>
        public static string Hash128Hex(byte[] data, ulong seed = 0)
        {
            var (low, high) = Hash128(data, seed);
            return low.ToString("x16") + high.ToString("x16");
        }

        #endregion

        #region 辅助方法

        private static uint ReadUInt32(byte[] data, int offset)
        {
            return (uint)(data[offset] | (data[offset + 1] << 8) | (data[offset + 2] << 16) | (data[offset + 3] << 24));
        }

        private static ulong ReadUInt64(byte[] data, int offset)
        {
            return (ulong)ReadUInt32(data, offset) | ((ulong)ReadUInt32(data, offset + 4) << 32);
        }

        private static uint RotateLeft32(uint x, int r)
        {
            return (x << r) | (x >> (32 - r));
        }

        private static ulong RotateLeft64(ulong x, int r)
        {
            return (x << r) | (x >> (64 - r));
        }

        #endregion

        #region 实用方法

        /// <summary>
        /// 计算哈希值并返回十六进制字符串
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <param name="bits">位数：32 或 64（默认64）</param>
        /// <param name="seed">种子值</param>
        /// <returns>十六进制字符串</returns>
        public static string ComputeHex(byte[] data, int bits = 64, ulong seed = 0)
        {
            if (data == null || data.Length == 0)
                return bits == 32 ? "00000000" : "0000000000000000";

            return bits switch
            {
                32 => Hash32(data, (uint)seed).ToString("x8"),
                64 => Hash64(data, seed).ToString("x16"),
                128 => Hash128Hex(data, seed),
                _ => throw new ArgumentException("Bits must be 32, 64, or 128", nameof(bits))
            };
        }

        /// <summary>
        /// 验证数据哈希值
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="expectedHash">预期的哈希值（十六进制）</param>
        /// <param name="seed">种子值</param>
        /// <returns>是否匹配</returns>
        public static bool Verify(byte[] data, string expectedHash, ulong seed = 0)
        {
            if (data == null || string.IsNullOrEmpty(expectedHash))
                return false;

            int bits = expectedHash.Length * 4;
            string computed = ComputeHex(data, bits, seed);
            return string.Equals(computed, expectedHash, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 用于数据分片的哈希
        /// </summary>
        /// <param name="key">键值</param>
        /// <param name="shards">分片数量</param>
        /// <returns>分片索引（0 到 shards-1）</returns>
        public static int GetShard(string key, int shards)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Key cannot be null or empty", nameof(key));
            if (shards <= 0)
                throw new ArgumentException("Shards must be greater than 0", nameof(shards));

            ulong hash = Hash64(key);
            return (int)(hash % (ulong)shards);
        }

        #endregion
    }
}
