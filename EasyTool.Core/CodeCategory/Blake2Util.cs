using System;
using System.Security.Cryptography;
using System.Text;

namespace EasyTool.CodeCategory
{
    /// <summary>
    /// BLAKE2 哈希工具类
    /// BLAKE2 是一种快速、安全的加密哈希函数
    /// 比 MD5、SHA-1、SHA-2 更快，同时提供更高的安全性
    /// 包含 BLAKE2b（64位优化）和 BLAKE2s（32位优化）两个版本
    /// </summary>
    public static class Blake2Util
    {
        #region BLAKE2b

        /// <summary>
        /// 计算 BLAKE2b-256 哈希值
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <returns>32字节哈希值</returns>
        public static byte[] ComputeBlake2b256(byte[] data)
        {
            return ComputeBlake2b(data, 32);
        }

        /// <summary>
        /// 计算 BLAKE2b-384 哈希值
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <returns>48字节哈希值</returns>
        public static byte[] ComputeBlake2b384(byte[] data)
        {
            return ComputeBlake2b(data, 48);
        }

        /// <summary>
        /// 计算 BLAKE2b-512 哈希值
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <returns>64字节哈希值</returns>
        public static byte[] ComputeBlake2b512(byte[] data)
        {
            return ComputeBlake2b(data, 64);
        }

        /// <summary>
        /// 计算指定长度的 BLAKE2b 哈希值
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <param name="hashLength">哈希长度（1-64字节）</param>
        /// <returns>指定长度的哈希值</returns>
        public static byte[] ComputeBlake2b(byte[] data, int hashLength)
        {
            return ComputeBlake2b(data, 0, data?.Length ?? 0, null, null, hashLength);
        }

        /// <summary>
        /// 使用密钥计算 BLAKE2b 哈希值（MAC）
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <param name="key">密钥（最多64字节）</param>
        /// <param name="hashLength">哈希长度</param>
        /// <returns>哈希值</returns>
        public static byte[] ComputeBlake2bWithKey(byte[] data, byte[] key, int hashLength = 64)
        {
            return ComputeBlake2b(data, 0, data?.Length ?? 0, key, null, hashLength);
        }

        /// <summary>
        /// 计算 BLAKE2b 哈希值（完整参数）
        /// </summary>
        public static byte[] ComputeBlake2b(byte[] data, int offset, int length, byte[] key, byte[] salt, int hashLength)
        {
            if (data == null)
                data = Array.Empty<byte>();
            if (hashLength < 1 || hashLength > 64)
                throw new ArgumentOutOfRangeException(nameof(hashLength), "Hash length must be between 1 and 64 bytes");
            if (key != null && key.Length > 64)
                throw new ArgumentException("Key must be at most 64 bytes", nameof(key));

            var hasher = new Blake2bHasher(key, salt, hashLength);
            hasher.Update(data, offset, length);
            return hasher.Final();
        }

        /// <summary>
        /// 计算字符串的 BLAKE2b-256 哈希值
        /// </summary>
        /// <param name="text">文本</param>
        /// <returns>32字节哈希值</returns>
        public static byte[] ComputeBlake2b256String(string text)
        {
            if (string.IsNullOrEmpty(text))
                return ComputeBlake2b256(Array.Empty<byte>());

            byte[] data = Encoding.UTF8.GetBytes(text);
            return ComputeBlake2b256(data);
        }

        /// <summary>
        /// 获取 BLAKE2b-512 哈希的十六进制表示
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <returns>128字符的十六进制字符串</returns>
        public static string ComputeBlake2b512Hex(byte[] data)
        {
            byte[] hash = ComputeBlake2b512(data);
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

        #endregion

        #region BLAKE2s

        /// <summary>
        /// 计算 BLAKE2s-128 哈希值
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <returns>16字节哈希值</returns>
        public static byte[] ComputeBlake2s128(byte[] data)
        {
            return ComputeBlake2s(data, 16);
        }

        /// <summary>
        /// 计算 BLAKE2s-256 哈希值
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <returns>32字节哈希值</returns>
        public static byte[] ComputeBlake2s256(byte[] data)
        {
            return ComputeBlake2s(data, 32);
        }

        /// <summary>
        /// 计算指定长度的 BLAKE2s 哈希值
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <param name="hashLength">哈希长度（1-32字节）</param>
        /// <returns>指定长度的哈希值</returns>
        public static byte[] ComputeBlake2s(byte[] data, int hashLength)
        {
            return ComputeBlake2s(data, 0, data?.Length ?? 0, null, null, hashLength);
        }

        /// <summary>
        /// 使用密钥计算 BLAKE2s 哈希值（MAC）
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <param name="key">密钥（最多32字节）</param>
        /// <param name="hashLength">哈希长度</param>
        /// <returns>哈希值</returns>
        public static byte[] ComputeBlake2sWithKey(byte[] data, byte[] key, int hashLength = 32)
        {
            return ComputeBlake2s(data, 0, data?.Length ?? 0, key, null, hashLength);
        }

        /// <summary>
        /// 计算 BLAKE2s 哈希值（完整参数）
        /// </summary>
        public static byte[] ComputeBlake2s(byte[] data, int offset, int length, byte[] key, byte[] salt, int hashLength)
        {
            if (data == null)
                data = Array.Empty<byte>();
            if (hashLength < 1 || hashLength > 32)
                throw new ArgumentOutOfRangeException(nameof(hashLength), "Hash length must be between 1 and 32 bytes");
            if (key != null && key.Length > 32)
                throw new ArgumentException("Key must be at most 32 bytes", nameof(key));

            var hasher = new Blake2sHasher(key, salt, hashLength);
            hasher.Update(data, offset, length);
            return hasher.Final();
        }

        /// <summary>
        /// 计算字符串的 BLAKE2s-256 哈希值
        /// </summary>
        /// <param name="text">文本</param>
        /// <returns>32字节哈希值</returns>
        public static byte[] ComputeBlake2s256String(string text)
        {
            if (string.IsNullOrEmpty(text))
                return ComputeBlake2s256(Array.Empty<byte>());

            byte[] data = Encoding.UTF8.GetBytes(text);
            return ComputeBlake2s256(data);
        }

        /// <summary>
        /// 获取 BLAKE2s-256 哈希的十六进制表示
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <returns>64字符的十六进制字符串</returns>
        public static string ComputeBlake2s256Hex(byte[] data)
        {
            byte[] hash = ComputeBlake2s256(data);
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

        #endregion

        #region 密钥生成

        /// <summary>
        /// 生成适合 BLAKE2b 的随机密钥
        /// </summary>
        /// <param name="length">密钥长度（最多64字节）</param>
        /// <returns>随机密钥</returns>
        public static byte[] GenerateKey(int length = 32)
        {
            if (length < 1 || length > 64)
                throw new ArgumentOutOfRangeException(nameof(length), "Key length must be between 1 and 64 bytes");

            byte[] key = new byte[length];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(key);
            return key;
        }

        /// <summary>
        /// 生成密钥并返回十六进制
        /// </summary>
        /// <param name="length">密钥长度</param>
        /// <returns>十六进制密钥字符串</returns>
        public static string GenerateKeyHex(int length = 32)
        {
            byte[] key = GenerateKey(length);
            return BitConverter.ToString(key).Replace("-", "").ToLower();
        }

        #endregion
    }

    #region BLAKE2b 实现类

    internal class Blake2bHasher
    {
        private static readonly ulong[] IV = new ulong[]
        {
            0x6a09e667f3bcc908, 0xbb67ae8584caa73b,
            0x3c6ef372fe94f82b, 0xa54ff53a5f1d36f1,
            0x510e527fade682d1, 0x9b05688c2b3e6c1f,
            0x1f83d9abfb41bd6b, 0x5be0cd19137e2179
        };

        private static readonly int[] Sigma = new int[]
        {
            0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15,
            14, 10, 4, 8, 9, 15, 13, 6, 1, 12, 0, 2, 11, 7, 5, 3,
            11, 8, 12, 0, 5, 2, 15, 13, 10, 14, 3, 6, 7, 1, 9, 4,
            7, 9, 3, 1, 13, 12, 11, 14, 2, 6, 5, 10, 4, 0, 15, 8,
            9, 0, 5, 7, 2, 4, 10, 15, 14, 1, 11, 12, 6, 8, 3, 13,
            2, 12, 6, 10, 0, 11, 8, 3, 4, 13, 7, 5, 15, 14, 1, 9,
            12, 5, 1, 15, 14, 13, 4, 10, 0, 7, 6, 3, 9, 2, 8, 11,
            13, 11, 7, 14, 12, 1, 3, 9, 5, 0, 15, 4, 8, 6, 2, 10,
            6, 15, 14, 9, 11, 3, 0, 8, 12, 2, 13, 7, 1, 4, 10, 5,
            10, 2, 8, 4, 7, 6, 1, 5, 15, 11, 9, 14, 3, 12, 13, 0,
            0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15,
            14, 10, 4, 8, 9, 15, 13, 6, 1, 12, 0, 2, 11, 7, 5, 3
        };

        private ulong[] h = new ulong[8];
        private ulong[] m = new ulong[16];
        private byte[] buffer = new byte[128];
        private int bufferLength;
        private ulong totalLength;
        private readonly int hashLength;

        public Blake2bHasher(byte[] key, byte[] salt, int hashLength)
        {
            this.hashLength = hashLength;

            // 初始化
            for (int i = 0; i < 8; i++)
                h[i] = IV[i];

            // 参数块
            h[0] ^= 0x01010000UL ^ ((ulong)(key?.Length ?? 0) << 8) ^ (ulong)hashLength;

            // 盐
            if (salt != null && salt.Length >= 8)
            {
                h[4] ^= BitConverter.ToUInt64(salt, 0);
                h[5] ^= BitConverter.ToUInt64(salt, 8);
            }

            // 处理密钥
            if (key != null && key.Length > 0)
            {
                Array.Copy(key, buffer, key.Length);
                bufferLength = 128;
            }
            else
            {
                bufferLength = 0;
            }

            totalLength = 0;
        }

        public void Update(byte[] data, int offset, int length)
        {
            totalLength += (ulong)length;

            int pos = 0;
            if (bufferLength > 0)
            {
                int copy = Math.Min(128 - bufferLength, length);
                Array.Copy(data, offset, buffer, bufferLength, copy);
                bufferLength += copy;
                pos = copy;

                if (bufferLength == 128)
                {
                    Compress(buffer, 0);
                    bufferLength = 0;
                }
            }

            while (pos + 128 <= length)
            {
                Compress(data, offset + pos);
                pos += 128;
            }

            if (pos < length)
            {
                Array.Copy(data, offset + pos, buffer, 0, length - pos);
                bufferLength = length - pos;
            }
        }

        public byte[] Final()
        {
            // 填充
            for (int i = bufferLength; i < 128; i++)
                buffer[i] = 0;

            // 最后一轮
            h[6] ^= ~0UL;

            Compress(buffer, 0, true);

            byte[] result = new byte[hashLength];
            for (int i = 0; i < hashLength; i++)
            {
                result[i] = (byte)(h[i / 8] >> ((i % 8) * 8));
            }

            return result;
        }

        private void Compress(byte[] data, int offset, bool isLast = false)
        {
            ulong[] v = new ulong[16];
            for (int i = 0; i < 8; i++)
            {
                v[i] = h[i];
                v[i + 8] = IV[i];
            }

            for (int i = 0; i < 16; i++)
            {
                v[i] ^= BitConverter.ToUInt64(data, offset + i * 8);
            }

            ulong counter = totalLength;
            v[12] ^= counter;
            v[13] ^= counter >> 56;
            if (isLast) v[14] = ~0UL;

            for (int round = 0; round < 12; round++)
            {
                int s = round * 16;
                Mix(v, 0, 4, 8, 12, Sigma[s], Sigma[s + 1]);
                Mix(v, 1, 5, 9, 13, Sigma[s + 2], Sigma[s + 3]);
                Mix(v, 2, 6, 10, 14, Sigma[s + 4], Sigma[s + 5]);
                Mix(v, 3, 7, 11, 15, Sigma[s + 6], Sigma[s + 7]);

                Mix(v, 0, 5, 10, 15, Sigma[s + 8], Sigma[s + 9]);
                Mix(v, 1, 6, 11, 12, Sigma[s + 10], Sigma[s + 11]);
                Mix(v, 2, 7, 8, 13, Sigma[s + 12], Sigma[s + 13]);
                Mix(v, 3, 4, 9, 14, Sigma[s + 14], Sigma[s + 15]);
            }

            for (int i = 0; i < 8; i++)
                h[i] ^= v[i] ^ v[i + 8];
        }

        private static void Mix(ulong[] v, int a, int b, int c, int d, int x, int y)
        {
            v[a] += v[b] + (ulong)x;
            v[d] = RotateRight(v[d] ^ v[a], 32);
            v[c] += v[d];
            v[b] = RotateRight(v[b] ^ v[c], 24);
            v[a] += v[b] + (ulong)y;
            v[d] = RotateRight(v[d] ^ v[a], 16);
            v[c] += v[d];
            v[b] = RotateRight(v[b] ^ v[c], 63);
        }

        private static ulong RotateRight(ulong x, int n) => (x >> n) | (x << (64 - n));
    }

    #endregion

    #region BLAKE2s 实现类

    internal class Blake2sHasher
    {
        private static readonly uint[] IV = new uint[]
        {
            0x6a09e667, 0xbb67ae85, 0x3c6ef372, 0xa54ff53a,
            0x510e527f, 0x9b05688c, 0x1f83d9ab, 0x5be0cd19
        };

        private static readonly int[] Sigma = new int[]
        {
            0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15,
            14, 10, 4, 8, 9, 15, 13, 6, 1, 12, 0, 2, 11, 7, 5, 3,
            11, 8, 12, 0, 5, 2, 15, 13, 10, 14, 3, 6, 7, 1, 9, 4,
            7, 9, 3, 1, 13, 12, 11, 14, 2, 6, 5, 10, 4, 0, 15, 8,
            9, 0, 5, 7, 2, 4, 10, 15, 14, 1, 11, 12, 6, 8, 3, 13,
            2, 12, 6, 10, 0, 11, 8, 3, 4, 13, 7, 5, 15, 14, 1, 9,
            12, 5, 1, 15, 14, 13, 4, 10, 0, 7, 6, 3, 9, 2, 8, 11,
            13, 11, 7, 14, 12, 1, 3, 9, 5, 0, 15, 4, 8, 6, 2, 10
        };

        private uint[] h = new uint[8];
        private uint[] m = new uint[16];
        private byte[] buffer = new byte[64];
        private int bufferLength;
        private ulong totalLength;
        private readonly int hashLength;

        public Blake2sHasher(byte[] key, byte[] salt, int hashLength)
        {
            this.hashLength = hashLength;

            for (int i = 0; i < 8; i++)
                h[i] = IV[i];

            h[0] ^= 0x01010000U ^ ((uint)(key?.Length ?? 0) << 8) ^ (uint)hashLength;

            if (salt != null && salt.Length >= 8)
            {
                h[4] ^= BitConverter.ToUInt32(salt, 0);
                h[5] ^= BitConverter.ToUInt32(salt, 4);
                h[6] ^= BitConverter.ToUInt32(salt, 8);
                h[7] ^= BitConverter.ToUInt32(salt, 12);
            }

            if (key != null && key.Length > 0)
            {
                Array.Copy(key, buffer, key.Length);
                bufferLength = 64;
            }
            else
            {
                bufferLength = 0;
            }

            totalLength = 0;
        }

        public void Update(byte[] data, int offset, int length)
        {
            totalLength += (ulong)length;

            int pos = 0;
            if (bufferLength > 0)
            {
                int copy = Math.Min(64 - bufferLength, length);
                Array.Copy(data, offset, buffer, bufferLength, copy);
                bufferLength += copy;
                pos = copy;

                if (bufferLength == 64)
                {
                    Compress(buffer, 0);
                    bufferLength = 0;
                }
            }

            while (pos + 64 <= length)
            {
                Compress(data, offset + pos);
                pos += 64;
            }

            if (pos < length)
            {
                Array.Copy(data, offset + pos, buffer, 0, length - pos);
                bufferLength = length - pos;
            }
        }

        public byte[] Final()
        {
            for (int i = bufferLength; i < 64; i++)
                buffer[i] = 0;

            h[6] ^= ~0U;

            Compress(buffer, 0, true);

            byte[] result = new byte[hashLength];
            for (int i = 0; i < hashLength; i++)
            {
                result[i] = (byte)(h[i / 4] >> ((i % 4) * 8));
            }

            return result;
        }

        private void Compress(byte[] data, int offset, bool isLast = false)
        {
            uint[] v = new uint[16];
            for (int i = 0; i < 8; i++)
            {
                v[i] = h[i];
                v[i + 8] = IV[i];
            }

            for (int i = 0; i < 16; i++)
            {
                v[i] ^= BitConverter.ToUInt32(data, offset + i * 4);
            }

            uint counter = (uint)totalLength;
            v[12] ^= counter;
            if (isLast) v[14] = ~0U;

            for (int round = 0; round < 10; round++)
            {
                int s = round * 16;
                Mix(v, 0, 4, 8, 12, Sigma[s], Sigma[s + 1]);
                Mix(v, 1, 5, 9, 13, Sigma[s + 2], Sigma[s + 3]);
                Mix(v, 2, 6, 10, 14, Sigma[s + 4], Sigma[s + 5]);
                Mix(v, 3, 7, 11, 15, Sigma[s + 6], Sigma[s + 7]);

                Mix(v, 0, 5, 10, 15, Sigma[s + 8], Sigma[s + 9]);
                Mix(v, 1, 6, 11, 12, Sigma[s + 10], Sigma[s + 11]);
                Mix(v, 2, 7, 8, 13, Sigma[s + 12], Sigma[s + 13]);
                Mix(v, 3, 4, 9, 14, Sigma[s + 14], Sigma[s + 15]);
            }

            for (int i = 0; i < 8; i++)
                h[i] ^= v[i] ^ v[i + 8];
        }

        private static void Mix(uint[] v, int a, int b, int c, int d, int x, int y)
        {
            v[a] += v[b] + (uint)x;
            v[d] = RotateRight(v[d] ^ v[a], 16);
            v[c] += v[d];
            v[b] = RotateRight(v[b] ^ v[c], 12);
            v[a] += v[b] + (uint)y;
            v[d] = RotateRight(v[d] ^ v[a], 8);
            v[c] += v[d];
            v[b] = RotateRight(v[b] ^ v[c], 7);
        }

        private static uint RotateRight(uint x, int n) => (x >> n) | (x << (32 - n));
    }

    #endregion
}
