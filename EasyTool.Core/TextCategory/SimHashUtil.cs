using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace EasyTool.TextCategory
{
    /// <summary>
    /// SimHash 工具类
    /// 用于计算文本相似度的局部敏感哈希
    /// </summary>
    public static class SimHashUtil
    {
        /// <summary>
        /// 计算 SimHash 值
        /// </summary>
        public static ulong Compute(string text, int hashBits = 64)
        {
            if (string.IsNullOrEmpty(text))
                return 0;

            // 分词
            var tokens = Tokenize(text);
            if (tokens.Count == 0)
                return 0;

            // 计算每个位的权重
            int[] v = new int[hashBits];

            foreach (var token in tokens)
            {
                ulong hash = HashToken(token);

                for (int i = 0; i < hashBits; i++)
                {
                    if (((hash >> i) & 1) == 1)
                    {
                        v[i]++;
                    }
                    else
                    {
                        v[i]--;
                    }
                }
            }

            // 生成 SimHash
            ulong simHash = 0;
            for (int i = 0; i < hashBits; i++)
            {
                if (v[i] > 0)
                {
                    simHash |= (1UL << i);
                }
            }

            return simHash;
        }

        /// <summary>
        /// 计算 Hamming 距离
        /// </summary>
        public static int HammingDistance(ulong hash1, ulong hash2)
        {
            ulong xor = hash1 ^ hash2;
            int distance = 0;

            while (xor != 0)
            {
                distance++;
                xor &= xor - 1;
            }

            return distance;
        }

        /// <summary>
        /// 计算两个文本的相似度（基于 SimHash）
        /// </summary>
        public static double Similarity(string text1, string text2, int hashBits = 64)
        {
            ulong hash1 = Compute(text1, hashBits);
            ulong hash2 = Compute(text2, hashBits);

            int distance = HammingDistance(hash1, hash2);
            return 1.0 - (double)distance / hashBits;
        }

        /// <summary>
        /// 判断两个文本是否相似
        /// </summary>
        public static bool IsSimilar(string text1, string text2, int threshold = 3)
        {
            ulong hash1 = Compute(text1);
            ulong hash2 = Compute(text2);

            return HammingDistance(hash1, hash2) <= threshold;
        }

        /// <summary>
        /// 计算 SimHash 并返回十六进制字符串
        /// </summary>
        public static string ComputeHex(string text)
        {
            ulong hash = Compute(text);
            return hash.ToString("X16");
        }

        /// <summary>
        /// 从十六进制字符串解析 SimHash
        /// </summary>
        public static ulong ParseHex(string hex)
        {
            return ulong.Parse(hex, System.Globalization.NumberStyles.HexNumber);
        }

        private static List<string> Tokenize(string text)
        {
            var tokens = new List<string>();

            // 简单分词：按空格和非字母数字字符分割
            var words = text.Split(new[] { ' ', '\t', '\n', '\r', '.', ',', '!', '?', ';', ':', '"', '\'', '(', ')', '[', ']', '{', '}' },
                StringSplitOptions.RemoveEmptyEntries);

            // 添加单词
            foreach (var word in words)
            {
                string lower = word.ToLowerInvariant();
                if (lower.Length >= 2) // 忽略单字符
                {
                    tokens.Add(lower);
                }
            }

            // 对于中文，添加2-gram和3-gram
            foreach (char c in text)
            {
                if (c >= 0x4E00 && c <= 0x9FA5)
                {
                    tokens.Add(c.ToString());
                }
            }

            // 添加字符n-gram
            if (text.Length >= 2)
            {
                for (int i = 0; i < text.Length - 1; i++)
                {
                    tokens.Add(text.Substring(i, 2));
                }
            }

            return tokens;
        }

        private static ulong HashToken(string token)
        {
            // 使用 MurmurHash3 简化版
            byte[] data = Encoding.UTF8.GetBytes(token);

            unchecked
            {
                const ulong c1 = 0x87c37b91114253d5;
                const ulong c2 = 0x4cf5ad432745937f;

                ulong h1 = 0;
                int length = data.Length;
                int blocks = length / 8;
                int i = 0;

                for (int j = 0; j < blocks; j++)
                {
                    ulong k1 = BitConverter.ToUInt64(data, i);
                    i += 8;

                    k1 *= c1;
                    k1 = (k1 << 31) | (k1 >> 33);
                    k1 *= c2;
                    h1 ^= k1;

                    h1 = (h1 << 27) | (h1 >> 37);
                    h1 = h1 * 5 + 0x52dce729;
                }

                ulong remaining = 0;
                int remainingLength = length - blocks * 8;
                if (remainingLength > 0)
                {
                    for (int j = 0; j < remainingLength; j++)
                    {
                        remaining |= (ulong)data[i + j] << (j * 8);
                    }

                    remaining *= c1;
                    remaining = (remaining << 31) | (remaining >> 33);
                    remaining *= c2;
                    h1 ^= remaining;
                }

                h1 ^= (ulong)length;
                h1 ^= h1 >> 33;
                h1 *= 0xff51afd7ed558ccd;
                h1 ^= h1 >> 33;
                h1 *= 0xc4ceb9fe1a85ec53;
                h1 ^= h1 >> 33;

                return h1;
            }
        }
    }

    /// <summary>
    /// MinHash 工具类
    /// 用于集合相似度计算
    /// </summary>
    public class MinHash
    {
        private readonly int _numHashes;
        private readonly uint[] _seeds;

        /// <summary>
        /// 哈希函数数量
        /// </summary>
        public int NumHashes => _numHashes;

        /// <summary>
        /// 创建 MinHash
        /// </summary>
        public MinHash(int numHashes = 128)
        {
            _numHashes = numHashes;
            _seeds = new uint[numHashes];

            var random = new Random(42);
            for (int i = 0; i < numHashes; i++)
            {
                _seeds[i] = (uint)random.Next();
            }
        }

        /// <summary>
        /// 计算集合的 MinHash 签名
        /// </summary>
        public uint[] ComputeSignature(HashSet<string> set)
        {
            var signature = new uint[_numHashes];

            for (int i = 0; i < _numHashes; i++)
            {
                uint minHash = uint.MaxValue;

                foreach (var item in set)
                {
                    uint hash = Hash(item, _seeds[i]);
                    if (hash < minHash)
                    {
                        minHash = hash;
                    }
                }

                signature[i] = minHash;
            }

            return signature;
        }

        /// <summary>
        /// 计算文本的 MinHash 签名
        /// </summary>
        public uint[] ComputeSignature(string text)
        {
            var set = new HashSet<string>();
            var words = text.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var word in words)
            {
                set.Add(word.ToLowerInvariant());
            }

            // 添加n-gram
            if (text.Length >= 2)
            {
                for (int i = 0; i < text.Length - 1; i++)
                {
                    set.Add(text.Substring(i, 2));
                }
            }

            return ComputeSignature(set);
        }

        /// <summary>
        /// 计算两个签名的 Jaccard 相似度估计
        /// </summary>
        public static double EstimateSimilarity(uint[] signature1, uint[] signature2)
        {
            if (signature1.Length != signature2.Length)
                throw new ArgumentException("Signatures must have the same length");

            int matches = 0;
            for (int i = 0; i < signature1.Length; i++)
            {
                if (signature1[i] == signature2[i])
                {
                    matches++;
                }
            }

            return (double)matches / signature1.Length;
        }

        private static uint Hash(string s, uint seed)
        {
            unchecked
            {
                uint hash = seed;
                foreach (char c in s)
                {
                    hash = hash * 31 + c;
                }
                return hash;
            }
        }
    }
}
