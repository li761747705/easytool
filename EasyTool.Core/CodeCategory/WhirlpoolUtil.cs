using System;
using System.Security.Cryptography;
using System.Text;

namespace EasyTool.CodeCategory
{
    /// <summary>
    /// Whirlpool 哈希工具类
    /// Whirlpool 是一种 512 位加密哈希函数
    /// 由 Vincent Rijmen（AES 共同设计者）和 Paulo S. L. M. Barreto 设计
    /// 被 ISO/IEC 10118-3 标准采纳
    /// </summary>
    public static class WhirlpoolUtil
    {
        private static readonly ulong[] C = new ulong[]
        {
            0x1823c6e2579a4e1a, 0x36a6d2f57adc6a4e, 0x60bc9b8ea30c7b35, 0x1de0d7c22e4bfe57,
            0x157737e59ff04ada, 0x58c9290ab1a06b85, 0xbd5d10f4cb3e0567, 0xe427418ba77d95d8,
            0xfbbee7c66dd58145, 0xca67c695f24b1292, 0x15c8b35a11a3a085, 0x38de11c0b9d4e859,
            0xae96d0d8a14f9f56, 0x7e42927360e92d49, 0x89b38c2355b7cb40, 0x6b19c2786b1a6f45,
            0x37a476c642dfb251, 0xa6c78a5a9686ebfd, 0xd236904cc73ca1e2, 0x4fa8cf51f68e2a95,
            0x2b92986c68a2d3eb, 0x644b992a0c907e92, 0x76a78a5a9686ebfd, 0x49ae3ac61aebc0ad,
            0x6a8e6b1a9686ebfd, 0x5b92986c68a2d3eb, 0x4fa8cf51f68e2a95, 0x39a1db7e58c0e2f8,
            0x9b1c8c6bbfb21a4d, 0xc6bca1db58c0e2f8, 0x4fa8cf51f68e2a95, 0x9b1c8c6bbfb21a4d,
            0x6b19c2786b1a6f45, 0x37a476c642dfb251, 0xa6c78a5a9686ebfd, 0xd236904cc73ca1e2,
            0x4fa8cf51f68e2a95, 0x2b92986c68a2d3eb, 0x644b992a0c907e92, 0x76a78a5a9686ebfd,
            0x49ae3ac61aebc0ad, 0x6a8e6b1a9686ebfd, 0x5b92986c68a2d3eb, 0x4fa8cf51f68e2a95,
            0x39a1db7e58c0e2f8, 0x9b1c8c6bbfb21a4d, 0xc6bca1db58c0e2f8, 0x4fa8cf51f68e2a95,
            0x9b1c8c6bbfb21a4d, 0x6b19c2786b1a6f45, 0x37a476c642dfb251, 0xa6c78a5a9686ebfd,
            0xd236904cc73ca1e2, 0x4fa8cf51f68e2a95, 0x2b92986c68a2d3eb, 0x644b992a0c907e92,
            0x76a78a5a9686ebfd, 0x49ae3ac61aebc0ad, 0x6a8e6b1a9686ebfd, 0x5b92986c68a2d3eb,
            0x4fa8cf51f68e2a95, 0x39a1db7e58c0e2f8, 0x9b1c8c6bbfb21a4d, 0xc6bca1db58c0e2f8
        };

        private const int DigestSize = 64;
        private const int BlockSize = 64;
        private const int Rounds = 10;

        /// <summary>
        /// 计算 Whirlpool 哈希值
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <returns>64字节哈希值</returns>
        public static byte[] ComputeHash(byte[] data)
        {
            if (data == null)
                data = Array.Empty<byte>();

            return ComputeHash(data, 0, data.Length);
        }

        /// <summary>
        /// 计算 Whirlpool 哈希值
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <param name="offset">起始偏移</param>
        /// <param name="length">数据长度</param>
        /// <returns>64字节哈希值</returns>
        public static byte[] ComputeHash(byte[] data, int offset, int length)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (offset < 0 || offset > data.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (length < 0 || offset + length > data.Length)
                throw new ArgumentOutOfRangeException(nameof(length));

            var hasher = new WhirlpoolHasher();
            hasher.Update(data, offset, length);
            return hasher.Final();
        }

        /// <summary>
        /// 计算字符串的 Whirlpool 哈希值
        /// </summary>
        /// <param name="text">文本</param>
        /// <returns>64字节哈希值</returns>
        public static byte[] ComputeString(string text)
        {
            if (string.IsNullOrEmpty(text))
                return ComputeHash(Array.Empty<byte>());

            byte[] data = Encoding.UTF8.GetBytes(text);
            return ComputeHash(data);
        }

        /// <summary>
        /// 获取 Whirlpool 哈希的十六进制表示
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <returns>128字符的十六进制字符串</returns>
        public static string ComputeHex(byte[] data)
        {
            byte[] hash = ComputeHash(data);
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

        /// <summary>
        /// 计算字符串的 Whirlpool 哈希十六进制表示
        /// </summary>
        /// <param name="text">文本</param>
        /// <returns>128字符的十六进制字符串</returns>
        public static string ComputeStringHex(string text)
        {
            byte[] hash = ComputeString(text);
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

        /// <summary>
        /// 创建 Whirlpool 哈希器（用于流式处理）
        /// </summary>
        /// <returns>哈希器实例</returns>
        public static WhirlpoolHasher CreateHasher()
        {
            return new WhirlpoolHasher();
        }
    }

    /// <summary>
    /// Whirlpool 哈希器
    /// </summary>
    public class WhirlpoolHasher
    {
        private const int DigestSize = 64;
        private const int BlockSize = 64;
        private const int Rounds = 10;

        private ulong[] hash = new ulong[8];
        private byte[] buffer = new byte[BlockSize];
        private int bufferLength;
        private ulong totalBits;

        private static readonly ulong[] C = new ulong[]
        {
            0x1823c6e2579a4e1a, 0x36a6d2f57adc6a4e, 0x60bc9b8ea30c7b35, 0x1de0d7c22e4bfe57,
            0x157737e59ff04ada, 0x58c9290ab1a06b85, 0xbd5d10f4cb3e0567, 0xe427418ba77d95d8,
            0xfbbee7c66dd58145, 0xca67c695f24b1292, 0x15c8b35a11a3a085, 0x38de11c0b9d4e859,
            0xae96d0d8a14f9f56, 0x7e42927360e92d49, 0x89b38c2355b7cb40, 0x6b19c2786b1a6f45,
            0x37a476c642dfb251, 0xa6c78a5a9686ebfd, 0xd236904cc73ca1e2, 0x4fa8cf51f68e2a95,
            0x2b92986c68a2d3eb, 0x644b992a0c907e92, 0x76a78a5a9686ebfd, 0x49ae3ac61aebc0ad,
            0x6a8e6b1a9686ebfd, 0x5b92986c68a2d3eb, 0x4fa8cf51f68e2a95, 0x39a1db7e58c0e2f8,
            0x9b1c8c6bbfb21a4d, 0xc6bca1db58c0e2f8, 0x4fa8cf51f68e2a95, 0x9b1c8c6bbfb21a4d,
            0x6b19c2786b1a6f45, 0x37a476c642dfb251, 0xa6c78a5a9686ebfd, 0xd236904cc73ca1e2,
            0x4fa8cf51f68e2a95, 0x2b92986c68a2d3eb, 0x644b992a0c907e92, 0x76a78a5a9686ebfd,
            0x49ae3ac61aebc0ad, 0x6a8e6b1a9686ebfd, 0x5b92986c68a2d3eb, 0x4fa8cf51f68e2a95,
            0x39a1db7e58c0e2f8, 0x9b1c8c6bbfb21a4d, 0xc6bca1db58c0e2f8, 0x4fa8cf51f68e2a95,
            0x9b1c8c6bbfb21a4d, 0x6b19c2786b1a6f45, 0x37a476c642dfb251, 0xa6c78a5a9686ebfd,
            0xd236904cc73ca1e2, 0x4fa8cf51f68e2a95, 0x2b92986c68a2d3eb, 0x644b992a0c907e92,
            0x76a78a5a9686ebfd, 0x49ae3ac61aebc0ad, 0x6a8e6b1a9686ebfd, 0x5b92986c68a2d3eb,
            0x4fa8cf51f68e2a95, 0x39a1db7e58c0e2f8, 0x9b1c8c6bbfb21a4d, 0xc6bca1db58c0e2f8
        };

        private static readonly ulong[] RC = new ulong[]
        {
            0x1823c6e2579a4e1a, 0x36a6d2f57adc6a4e, 0x60bc9b8ea30c7b35, 0x1de0d7c22e4bfe57,
            0x157737e59ff04ada, 0x58c9290ab1a06b85, 0xbd5d10f4cb3e0567, 0xe427418ba77d95d8,
            0xfbbee7c66dd58145, 0xca67c695f24b1292
        };

        public WhirlpoolHasher()
        {
            Array.Clear(hash, 0, 8);
            bufferLength = 0;
            totalBits = 0;
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

            totalBits += (ulong)length * 8;

            int pos = 0;

            if (bufferLength > 0)
            {
                int copy = Math.Min(BlockSize - bufferLength, length);
                Array.Copy(data, offset, buffer, bufferLength, copy);
                bufferLength += copy;
                pos = copy;

                if (bufferLength == BlockSize)
                {
                    ProcessBlock(buffer, 0);
                    bufferLength = 0;
                }
            }

            while (pos + BlockSize <= length)
            {
                ProcessBlock(data, offset + pos);
                pos += BlockSize;
            }

            if (pos < length)
            {
                Array.Copy(data, offset + pos, buffer, 0, length - pos);
                bufferLength = length - pos;
            }
        }

        /// <summary>
        /// 完成哈希计算
        /// </summary>
        /// <returns>64字节哈希值</returns>
        public byte[] Final()
        {
            // 填充
            buffer[bufferLength++] = 0x80;

            if (bufferLength > 32)
            {
                while (bufferLength < BlockSize)
                    buffer[bufferLength++] = 0;
                ProcessBlock(buffer, 0);
                bufferLength = 0;
            }

            while (bufferLength < 32)
                buffer[bufferLength++] = 0;

            // 添加长度
            for (int i = 0; i < 8; i++)
            {
                buffer[56 + i] = (byte)(totalBits >> (56 - i * 8));
            }

            ProcessBlock(buffer, 0);

            // 输出
            byte[] result = new byte[DigestSize];
            for (int i = 0; i < 8; i++)
            {
                byte[] bytes = BitConverter.GetBytes(hash[i]);
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(bytes);
                Array.Copy(bytes, 0, result, i * 8, 8);
            }

            return result;
        }

        private void ProcessBlock(byte[] block, int offset)
        {
            ulong[] K = new ulong[8];
            ulong[] L = new ulong[8];
            ulong[] M = new ulong[8];
            ulong[] state = new ulong[8];

            // 读取块
            for (int i = 0; i < 8; i++)
            {
                K[i] = hash[i];
                state[i] = ReadUInt64(block, offset + i * 8);
                M[i] = state[i];
            }

            // 初始变换
            for (int i = 0; i < 8; i++)
            {
                state[i] ^= K[i];
            }

            // 轮函数
            for (int r = 0; r < Rounds; r++)
            {
                // 计算 L
                for (int i = 0; i < 8; i++)
                {
                    L[i] = 0;
                    for (int j = 0; j < 8; j++)
                    {
                        L[i] ^= Multiply(C[(r * 8 + i) % 64], K[j]);
                    }
                }

                // 更新 K
                Array.Copy(L, K, 8);
                K[0] ^= RC[r];

                // 计算 state
                for (int i = 0; i < 8; i++)
                {
                    L[i] = 0;
                    for (int j = 0; j < 8; j++)
                    {
                        L[i] ^= Multiply(C[(r * 8 + i) % 64], state[j]);
                    }
                }

                Array.Copy(L, state, 8);

                for (int i = 0; i < 8; i++)
                {
                    state[i] ^= K[i];
                }
            }

            // 更新哈希
            for (int i = 0; i < 8; i++)
            {
                hash[i] ^= state[i] ^ M[i];
            }
        }

        private static ulong ReadUInt64(byte[] data, int offset)
        {
            return ((ulong)data[offset] << 56) |
                   ((ulong)data[offset + 1] << 48) |
                   ((ulong)data[offset + 2] << 40) |
                   ((ulong)data[offset + 3] << 32) |
                   ((ulong)data[offset + 4] << 24) |
                   ((ulong)data[offset + 5] << 16) |
                   ((ulong)data[offset + 6] << 8) |
                   data[offset + 7];
        }

        private static ulong Multiply(ulong a, ulong b)
        {
            ulong result = 0;
            ulong hi = 0x0100000000000000; // x^63 的模约简

            for (int i = 0; i < 64; i++)
            {
                if ((b & 1) != 0)
                    result ^= a;

                bool carry = (a & 0x8000000000000000) != 0;
                a <<= 1;

                if (carry)
                    a ^= hi;

                b >>= 1;
            }

            return result;
        }
    }
}
