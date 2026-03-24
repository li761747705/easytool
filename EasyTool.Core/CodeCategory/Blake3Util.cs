using System;
using System.Security.Cryptography;
using System.Text;

namespace EasyTool.CodeCategory
{
    /// <summary>
    /// BLAKE3 哈希工具类
    /// BLAKE3 是目前最快的加密哈希函数
    /// 基于 BLAKE2，采用 Merkle Tree 结构，支持并行计算
    /// 输出长度可变，默认 32 字节（256位）
    /// </summary>
    public static class Blake3Util
    {
        private static readonly uint[] IV = new uint[]
        {
            0x6a09e667, 0xbb67ae85, 0x3c6ef372, 0xa54ff53a,
            0x510e527f, 0x9b05688c, 0x1f83d9ab, 0x5be0cd19
        };

        private const int BlockLen = 64;
        private const int ChunkLen = 1024;

        /// <summary>
        /// 计算 BLAKE3 哈希值（默认32字节）
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <returns>32字节哈希值</returns>
        public static byte[] ComputeHash(byte[] data)
        {
            return ComputeHash(data, 32);
        }

        /// <summary>
        /// 计算指定长度的 BLAKE3 哈希值
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <param name="hashLength">哈希长度</param>
        /// <returns>指定长度的哈希值</returns>
        public static byte[] ComputeHash(byte[] data, int hashLength)
        {
            if (data == null)
                data = Array.Empty<byte>();
            if (hashLength < 1)
                throw new ArgumentOutOfRangeException(nameof(hashLength), "Hash length must be at least 1 byte");

            var hasher = new Blake3Hasher(null);
            hasher.Update(data, 0, data.Length);
            return hasher.Finalize(hashLength);
        }

        /// <summary>
        /// 使用密钥计算 BLAKE3 哈希值（MAC）
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <param name="key">密钥（32字节）</param>
        /// <param name="hashLength">哈希长度</param>
        /// <returns>哈希值</returns>
        public static byte[] ComputeHashWithKey(byte[] data, byte[] key, int hashLength = 32)
        {
            if (key == null || key.Length != 32)
                throw new ArgumentException("Key must be 32 bytes", nameof(key));

            var hasher = new Blake3Hasher(key);
            hasher.Update(data, 0, data?.Length ?? 0);
            return hasher.Finalize(hashLength);
        }

        /// <summary>
        /// 计算 BLAKE3-256 哈希值
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <returns>32字节哈希值</returns>
        public static byte[] Compute256(byte[] data)
        {
            return ComputeHash(data, 32);
        }

        /// <summary>
        /// 计算 BLAKE3-512 哈希值
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <returns>64字节哈希值</returns>
        public static byte[] Compute512(byte[] data)
        {
            return ComputeHash(data, 64);
        }

        /// <summary>
        /// 计算字符串的 BLAKE3 哈希值
        /// </summary>
        /// <param name="text">文本</param>
        /// <returns>32字节哈希值</returns>
        public static byte[] ComputeString(string text)
        {
            if (string.IsNullOrEmpty(text))
                return ComputeHash(Array.Empty<byte>());

            byte[] data = Encoding.UTF8.GetBytes(text);
            return ComputeHash(data);
        }

        /// <summary>
        /// 获取 BLAKE3 哈希的十六进制表示
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <param name="hashLength">哈希长度</param>
        /// <returns>十六进制字符串</returns>
        public static string ComputeHex(byte[] data, int hashLength = 32)
        {
            byte[] hash = ComputeHash(data, hashLength);
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

        /// <summary>
        /// 生成随机密钥
        /// </summary>
        /// <returns>32字节密钥</returns>
        public static byte[] GenerateKey()
        {
            byte[] key = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(key);
            return key;
        }

        /// <summary>
        /// 生成密钥并返回十六进制
        /// </summary>
        /// <returns>64字符的十六进制密钥</returns>
        public static string GenerateKeyHex()
        {
            byte[] key = GenerateKey();
            return BitConverter.ToString(key).Replace("-", "").ToLower();
        }

        /// <summary>
        /// 创建 BLAKE3 哈希器（用于流式处理）
        /// </summary>
        /// <param name="key">密钥（可选）</param>
        /// <returns>哈希器实例</returns>
        public static Blake3Hasher CreateHasher(byte[] key = null)
        {
            return new Blake3Hasher(key);
        }
    }

    /// <summary>
    /// BLAKE3 哈希器
    /// </summary>
    public class Blake3Hasher
    {
        private static readonly uint[] IV = new uint[]
        {
            0x6a09e667, 0xbb67ae85, 0x3c6ef372, 0xa54ff53a,
            0x510e527f, 0x9b05688c, 0x1f83d9ab, 0x5be0cd19
        };

        private const int BlockLen = 64;
        private const int ChunkLen = 1024;

        private uint[] key;
        private byte[] buffer = new byte[ChunkLen];
        private int bufferLength;
        private ulong totalChunks;
        private uint[] chunkState = new uint[16];
        private uint[] cvStack = new uint[54 * 8]; // 最多 2^54 个 chunk
        private int cvStackLen;

        public Blake3Hasher(byte[] key)
        {
            if (key == null)
            {
                this.key = new uint[8];
                Array.Copy(IV, this.key, 8);
            }
            else if (key.Length == 32)
            {
                this.key = new uint[8];
                for (int i = 0; i < 8; i++)
                    this.key[i] = BitConverter.ToUInt32(key, i * 4);
            }
            else
            {
                throw new ArgumentException("Key must be 32 bytes", nameof(key));
            }

            bufferLength = 0;
            totalChunks = 0;
            cvStackLen = 0;
            InitChunkState();
        }

        private void InitChunkState()
        {
            for (int i = 0; i < 8; i++)
                chunkState[i] = key[i];
            for (int i = 8; i < 16; i++)
                chunkState[i] = IV[i - 8];
            chunkState[12] = (uint)(totalChunks & 0xFFFFFFFF);
            chunkState[13] = (uint)(totalChunks >> 32);
            chunkState[14] = 0;
            chunkState[15] = 0;
        }

        /// <summary>
        /// 更新哈希器数据
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <param name="offset">偏移</param>
        /// <param name="length">长度</param>
        public void Update(byte[] data, int offset, int length)
        {
            if (data == null || length == 0)
                return;

            int pos = 0;

            // 填满缓冲区
            if (bufferLength > 0)
            {
                int copy = Math.Min(ChunkLen - bufferLength, length);
                Array.Copy(data, offset, buffer, bufferLength, copy);
                bufferLength += copy;
                pos = copy;

                if (bufferLength == ChunkLen)
                {
                    ProcessChunk(buffer, 0);
                    bufferLength = 0;
                }
            }

            // 处理完整块
            while (pos + ChunkLen <= length)
            {
                ProcessChunk(data, offset + pos);
                pos += ChunkLen;
            }

            // 保存剩余
            if (pos < length)
            {
                Array.Copy(data, offset + pos, buffer, 0, length - pos);
                bufferLength = length - pos;
            }
        }

        private void ProcessChunk(byte[] data, int offset)
        {
            uint[] cv = new uint[8];
            CompressChunk(data, offset, cv);

            // 合并到 CV 栈
            PushCv(cv);

            totalChunks++;
            InitChunkState();
        }

        private void CompressChunk(byte[] data, int offset, uint[] output)
        {
            uint[] state = new uint[16];
            Array.Copy(chunkState, state, 16);

            for (int block = 0; block < 16; block++)
            {
                int blockOffset = offset + block * BlockLen;
                if (blockOffset + BlockLen > data.Length)
                    break;

                uint[] blockWords = new uint[16];
                for (int i = 0; i < 16; i++)
                    blockWords[i] = BitConverter.ToUInt32(data, blockOffset + i * 4);

                state[15] = (uint)(block + 1);
                Compress(state, blockWords);

                if (blockOffset + BlockLen == data.Length)
                {
                    state[14] ^= 0xFFFFFFFF;
                }
            }

            Array.Copy(state, 0, output, 0, 8);
        }

        private void PushCv(uint[] cv)
        {
            int pos = cvStackLen;
            while (pos > 0 && (totalChunks & ((1UL << pos) - 1)) == 0)
            {
                uint[] parentCv = new uint[8];
                uint[] block = new uint[16];
                Array.Copy(cvStack, (pos - 1) * 8, block, 0, 8);
                Array.Copy(cv, 0, block, 8, 8);

                uint[] state = new uint[16];
                for (int i = 0; i < 8; i++)
                    state[i] = key[i];
                for (int i = 0; i < 8; i++)
                    state[i + 8] = IV[i];
                state[12] = 0;
                state[13] = 0;
                state[14] = 0xFFFFFFFF;
                state[15] = 0;

                Compress(state, block);
                Array.Copy(state, 0, parentCv, 0, 8);

                cv = parentCv;
                pos--;
            }

            Array.Copy(cv, 0, cvStack, pos * 8, 8);
            cvStackLen = pos + 1;
        }

        private void Compress(uint[] state, uint[] block)
        {
            // BLAKE3 轮函数
            uint[] v = new uint[16];
            Array.Copy(state, v, 16);
            for (int i = 0; i < 16; i++)
                v[i] ^= block[i];

            for (int round = 0; round < 7; round++)
            {
                Round(v, round);
            }

            for (int i = 0; i < 8; i++)
                state[i] ^= v[i] ^ v[i + 8];
        }

        private void Round(uint[] v, int round)
        {
            // 置换
            int[] p = Permutation(round);

            // 混合
            Mix(v, p[0], p[4], p[8], p[12]);
            Mix(v, p[1], p[5], p[9], p[13]);
            Mix(v, p[2], p[6], p[10], p[14]);
            Mix(v, p[3], p[7], p[11], p[15]);
        }

        private int[] Permutation(int round)
        {
            int[][] perms = new int[][]
            {
                new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 },
                new int[] { 2, 6, 3, 10, 7, 0, 4, 13, 1, 11, 12, 5, 9, 14, 15, 8 },
                new int[] { 3, 4, 10, 12, 13, 2, 7, 14, 6, 5, 9, 0, 11, 15, 8, 1 },
                new int[] { 10, 7, 12, 9, 14, 3, 13, 15, 4, 0, 11, 2, 5, 8, 1, 6 },
                new int[] { 12, 13, 9, 11, 15, 10, 14, 8, 7, 2, 5, 3, 0, 1, 6, 4 },
                new int[] { 9, 14, 11, 5, 8, 12, 15, 1, 13, 3, 0, 10, 2, 6, 4, 7 },
                new int[] { 11, 15, 5, 0, 1, 9, 8, 6, 14, 10, 2, 12, 3, 4, 7, 13 }
            };

            return perms[round % 7];
        }

        private void Mix(uint[] v, int a, int b, int c, int d)
        {
            v[a] += v[b];
            v[d] = RotateRight(v[d] ^ v[a], 16);
            v[c] += v[d];
            v[b] = RotateRight(v[b] ^ v[c], 12);
            v[a] += v[b];
            v[d] = RotateRight(v[d] ^ v[a], 8);
            v[c] += v[d];
            v[b] = RotateRight(v[b] ^ v[c], 7);
        }

        private static uint RotateRight(uint x, int n) => (x >> n) | (x << (32 - n));

        /// <summary>
        /// 完成哈希计算
        /// </summary>
        /// <param name="hashLength">哈希长度</param>
        /// <returns>哈希值</returns>
        public byte[] Finalize(int hashLength = 32)
        {
            // 处理最后一个不完整的块
            uint[] finalCv;
            if (bufferLength > 0)
            {
                byte[] lastChunk = new byte[ChunkLen];
                Array.Copy(buffer, lastChunk, bufferLength);
                finalCv = new uint[8];
                CompressChunkFinal(lastChunk, 0, bufferLength, finalCv);
            }
            else
            {
                finalCv = new uint[8];
                Array.Copy(chunkState, finalCv, 8);
            }

            // 合并所有 CV
            uint[] rootCv = finalCv;
            while (cvStackLen > 0)
            {
                cvStackLen--;
                uint[] parentCv = new uint[8];
                uint[] block = new uint[16];
                Array.Copy(cvStack, cvStackLen * 8, block, 0, 8);
                Array.Copy(rootCv, 0, block, 8, 8);

                uint[] state = new uint[16];
                for (int i = 0; i < 8; i++)
                    state[i] = key[i];
                for (int i = 0; i < 8; i++)
                    state[i + 8] = IV[i];
                state[12] = 0;
                state[13] = 0;
                state[14] = 0xFFFFFFFF;
                state[15] = 0;

                Compress(state, block);
                Array.Copy(state, 0, parentCv, 0, 8);
                rootCv = parentCv;
            }

            // 输出
            byte[] result = new byte[hashLength];
            for (int i = 0; i < Math.Min(hashLength, 32); i++)
            {
                result[i] = (byte)(rootCv[i / 4] >> ((i % 4) * 8));
            }

            // 如果需要更多输出，使用输出扩展
            if (hashLength > 32)
            {
                int outputBlock = 1;
                int pos = 32;
                while (pos < hashLength)
                {
                    uint[] state = new uint[16];
                    Array.Copy(rootCv, state, 8);
                    for (int i = 0; i < 8; i++)
                        state[i + 8] = IV[i];
                    state[12] = (uint)outputBlock;
                    state[13] = (uint)(outputBlock >> 32);
                    state[14] = 0xFFFFFFFF;
                    state[15] = 0;

                    uint[] zeroBlock = new uint[16];
                    Compress(state, zeroBlock);

                    for (int i = 0; i < 32 && pos < hashLength; i++)
                    {
                        result[pos++] = (byte)(state[i / 4] >> ((i % 4) * 8));
                    }
                    outputBlock++;
                }
            }

            return result;
        }

        private void CompressChunkFinal(byte[] data, int offset, int length, uint[] output)
        {
            uint[] state = new uint[16];
            Array.Copy(chunkState, state, 16);

            int blocks = (length + BlockLen - 1) / BlockLen;
            for (int block = 0; block < blocks; block++)
            {
                int blockOffset = offset + block * BlockLen;
                int blockLen = Math.Min(BlockLen, length - block * BlockLen);

                uint[] blockWords = new uint[16];
                for (int i = 0; i < blockLen / 4; i++)
                    blockWords[i] = BitConverter.ToUInt32(data, blockOffset + i * 4);

                for (int i = blockLen / 4; i < 16; i++)
                    blockWords[i] = 0;

                state[15] = (uint)(block + 1);

                if (block == blocks - 1)
                {
                    state[14] ^= 0xFFFFFFFF;
                }

                Compress(state, blockWords);
            }

            Array.Copy(state, 0, output, 0, 8);
        }
    }
}
