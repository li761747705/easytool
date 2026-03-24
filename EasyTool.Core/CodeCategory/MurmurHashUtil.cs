using System;
using System.Text;

namespace EasyTool.CodeCategory
{
    /// <summary>
    /// MurmurHash 高性能非加密哈希工具类
    /// MurmurHash 是一种非加密型哈希函数，适用于一般的哈希检索操作
    /// 特点：高随机分布、高性能、低碰撞率
    /// </summary>
    public static class MurmurHashUtil
    {
        #region MurmurHash3 32-bit

        private const uint C1_32 = 0xcc9e2d51;
        private const uint C2_32 = 0x1b873593;
        private const uint R1_32 = 15;
        private const uint R2_32 = 13;
        private const uint M_32 = 5;
        private const uint N_32 = 0xe6546b64;

        /// <summary>
        /// 计算 MurmurHash3 32位哈希值
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <param name="seed">种子值（默认0）</param>
        /// <returns>32位哈希值</returns>
        public static uint Hash32(byte[] data, uint seed = 0)
        {
            if (data == null || data.Length == 0)
                return 0;

            uint h = seed;
            int length = data.Length;
            int blocks = length / 4;

            // 处理4字节块
            for (int i = 0; i < blocks; i++)
            {
                int blockOffset = i * 4;
                uint k = (uint)(data[blockOffset] | (data[blockOffset + 1] << 8) | (data[blockOffset + 2] << 16) | (data[blockOffset + 3] << 24));
                k *= C1_32;
                k = RotateLeft32(k, (int)R1_32);
                k *= C2_32;

                h ^= k;
                h = RotateLeft32(h, (int)R2_32);
                h = h * M_32 + N_32;
            }

            // 处理剩余字节
            int remaining = length % 4;
            int offset = blocks * 4;
            uint tail = 0;

            switch (remaining)
            {
                case 3:
                    tail ^= (uint)data[offset + 2] << 16;
                    goto case 2;
                case 2:
                    tail ^= (uint)data[offset + 1] << 8;
                    goto case 1;
                case 1:
                    tail ^= data[offset];
                    tail *= C1_32;
                    tail = RotateLeft32(tail, (int)R1_32);
                    tail *= C2_32;
                    h ^= tail;
                    break;
            }

            // 最终混合
            h ^= (uint)length;
            h = FinalMix32(h);

            return h;
        }

        /// <summary>
        /// 计算字符串的 MurmurHash3 32位哈希值
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

        private static uint FinalMix32(uint h)
        {
            h ^= h >> 16;
            h *= 0x85ebca6b;
            h ^= h >> 13;
            h *= 0xc2b2ae35;
            h ^= h >> 16;
            return h;
        }

        #endregion

        #region MurmurHash3 64-bit

        private const ulong C1_64 = 0x87c37b91114253d5;
        private const ulong C2_64 = 0x4cf5ad432745937f;
        private const int R1_64 = 31;
        private const int R2_64 = 27;
        private const ulong M_64 = 5;
        private const ulong N1_64 = 0x52dce729;
        private const ulong N2_64 = 0x38495ab5;

        /// <summary>
        /// 计算 MurmurHash3 64位哈希值（128位截断为64位）
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <param name="seed">种子值（默认0）</param>
        /// <returns>64位哈希值</returns>
        public static ulong Hash64(byte[] data, ulong seed = 0)
        {
            var (h1, h2) = Hash128(data, seed);
            return h1 ^ h2;
        }

        /// <summary>
        /// 计算字符串的 MurmurHash3 64位哈希值
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

        #endregion

        #region MurmurHash3 128-bit

        /// <summary>
        /// 计算 MurmurHash3 128位哈希值
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <param name="seed">种子值（默认0）</param>
        /// <returns>128位哈希值（两个64位值）</returns>
        public static (ulong H1, ulong H2) Hash128(byte[] data, ulong seed = 0)
        {
            if (data == null || data.Length == 0)
                return (0, 0);

            ulong h1 = seed;
            ulong h2 = seed;
            int length = data.Length;
            int blocks = length / 16;

            // 处理16字节块
            for (int i = 0; i < blocks; i++)
            {
                ulong k1 = BitConverter.ToUInt64(data, i * 16);
                ulong k2 = BitConverter.ToUInt64(data, i * 16 + 8);

                k1 *= C1_64;
                k1 = RotateLeft64(k1, R1_64);
                k1 *= C2_64;
                h1 ^= k1;

                h1 = RotateLeft64(h1, R2_64);
                h1 += h2;
                h1 = h1 * M_64 + N1_64;

                k2 *= C2_64;
                k2 = RotateLeft64(k2, R2_64);
                k2 *= C1_64;
                h2 ^= k2;

                h2 = RotateLeft64(h2, R1_64);
                h2 += h1;
                h2 = h2 * M_64 + N2_64;
            }

            // 处理剩余字节
            int remaining = length % 16;
            int offset = blocks * 16;
            ulong tail1 = 0;
            ulong tail2 = 0;

            switch (remaining)
            {
                case 15:
                    tail2 ^= (ulong)data[offset + 14] << 48;
                    goto case 14;
                case 14:
                    tail2 ^= (ulong)data[offset + 13] << 40;
                    goto case 13;
                case 13:
                    tail2 ^= (ulong)data[offset + 12] << 32;
                    goto case 12;
                case 12:
                    tail2 ^= (ulong)data[offset + 11] << 24;
                    goto case 11;
                case 11:
                    tail2 ^= (ulong)data[offset + 10] << 16;
                    goto case 10;
                case 10:
                    tail2 ^= (ulong)data[offset + 9] << 8;
                    goto case 9;
                case 9:
                    tail2 ^= data[offset + 8];
                    tail2 *= C2_64;
                    tail2 = RotateLeft64(tail2, R2_64);
                    tail2 *= C1_64;
                    h2 ^= tail2;
                    goto case 8;
                case 8:
                    tail1 ^= (ulong)data[offset + 7] << 56;
                    goto case 7;
                case 7:
                    tail1 ^= (ulong)data[offset + 6] << 48;
                    goto case 6;
                case 6:
                    tail1 ^= (ulong)data[offset + 5] << 40;
                    goto case 5;
                case 5:
                    tail1 ^= (ulong)data[offset + 4] << 32;
                    goto case 4;
                case 4:
                    tail1 ^= (ulong)data[offset + 3] << 24;
                    goto case 3;
                case 3:
                    tail1 ^= (ulong)data[offset + 2] << 16;
                    goto case 2;
                case 2:
                    tail1 ^= (ulong)data[offset + 1] << 8;
                    goto case 1;
                case 1:
                    tail1 ^= data[offset];
                    tail1 *= C1_64;
                    tail1 = RotateLeft64(tail1, R1_64);
                    tail1 *= C2_64;
                    h1 ^= tail1;
                    break;
            }

            // 最终混合
            h1 ^= (ulong)length;
            h2 ^= (ulong)length;

            h1 += h2;
            h2 += h1;

            h1 = FinalMix64(h1);
            h2 = FinalMix64(h2);

            h1 += h2;
            h2 += h1;

            return (h1, h2);
        }

        /// <summary>
        /// 计算字符串的 MurmurHash3 128位哈希值
        /// </summary>
        /// <param name="text">输入字符串</param>
        /// <param name="seed">种子值（默认0）</param>
        /// <param name="encoding">编码方式（默认UTF-8）</param>
        /// <returns>128位哈希值（两个64位值）</returns>
        public static (ulong H1, ulong H2) Hash128(string text, ulong seed = 0, Encoding encoding = null)
        {
            if (string.IsNullOrEmpty(text))
                return (0, 0);

            encoding ??= Encoding.UTF8;
            return Hash128(encoding.GetBytes(text), seed);
        }

        /// <summary>
        /// 计算 MurmurHash3 128位哈希值并返回字节数组
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <param name="seed">种子值（默认0）</param>
        /// <returns>16字节的哈希值</returns>
        public static byte[] Hash128Bytes(byte[] data, ulong seed = 0)
        {
            var (h1, h2) = Hash128(data, seed);
            var result = new byte[16];
            Array.Copy(BitConverter.GetBytes(h1), 0, result, 0, 8);
            Array.Copy(BitConverter.GetBytes(h2), 0, result, 8, 8);
            return result;
        }

        /// <summary>
        /// 计算 MurmurHash3 128位哈希值并返回十六进制字符串
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <param name="seed">种子值（默认0）</param>
        /// <returns>32字符的十六进制字符串</returns>
        public static string Hash128Hex(byte[] data, ulong seed = 0)
        {
            var (h1, h2) = Hash128(data, seed);
            return h1.ToString("x16") + h2.ToString("x16");
        }

        private static ulong FinalMix64(ulong k)
        {
            k ^= k >> 33;
            k *= 0xff51afd7ed558ccd;
            k ^= k >> 33;
            k *= 0xc4ceb9fe1a85ec53;
            k ^= k >> 33;
            return k;
        }

        #endregion

        #region 辅助方法

        private static uint RotateLeft32(uint x, int r)
        {
            return (x << r) | (x >> (32 - r));
        }

        private static ulong RotateLeft64(ulong x, int r)
        {
            return (x << r) | (x >> (64 - r));
        }

        #endregion

        #region 一致性哈希支持

        /// <summary>
        /// 计算一致性哈希位置（用于分布式系统）
        /// </summary>
        /// <param name="key">键值</param>
        /// <param name="buckets">桶的数量</param>
        /// <returns>桶的索引（0 到 buckets-1）</returns>
        public static int ConsistentHash(string key, int buckets)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Key cannot be null or empty", nameof(key));
            if (buckets <= 0)
                throw new ArgumentException("Buckets must be greater than 0", nameof(buckets));

            uint hash = Hash32(key);
            return (int)(hash % (uint)buckets);
        }

        /// <summary>
        /// 计算一致性哈希位置（带虚拟节点）
        /// </summary>
        /// <param name="key">键值</param>
        /// <param name="buckets">桶的数量</param>
        /// <param name="virtualNodes">每个桶的虚拟节点数</param>
        /// <returns>桶的索引（0 到 buckets-1）</returns>
        public static int ConsistentHash(string key, int buckets, int virtualNodes)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Key cannot be null or empty", nameof(key));
            if (buckets <= 0)
                throw new ArgumentException("Buckets must be greater than 0", nameof(buckets));
            if (virtualNodes <= 0)
                throw new ArgumentException("Virtual nodes must be greater than 0", nameof(virtualNodes));

            uint hash = Hash32(key);
            int totalNodes = buckets * virtualNodes;
            int nodeIndex = (int)(hash % (uint)totalNodes);
            return nodeIndex / virtualNodes;
        }

        #endregion
    }
}
