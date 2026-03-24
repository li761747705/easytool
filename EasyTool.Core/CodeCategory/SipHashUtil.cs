using System;

namespace EasyTool.CodeCategory
{
    /// <summary>
    /// SipHash 哈希工具类
    /// SipHash 是一种快速、安全的哈希算法，专为哈希表设计
    /// 由 Jean-Philippe Aumasson 和 Daniel J. Bernstein 开发
    /// 用于防止哈希碰撞攻击（HashDoS）
    /// </summary>
    public static class SipHashUtil
    {
        /// <summary>
        /// 使用 SipHash-2-4 计算 64 位哈希值
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <param name="key">密钥（16字节）</param>
        /// <returns>64位哈希值</returns>
        public static ulong ComputeHash64(byte[] data, byte[] key)
        {
            if (data == null)
                data = Array.Empty<byte>();
            if (key == null || key.Length != 16)
                throw new ArgumentException("Key must be 16 bytes", nameof(key));

            return Compute(data, 0, data.Length, key, 2, 4);
        }

        /// <summary>
        /// 使用 SipHash-2-4 计算 64 位哈希值（指定偏移和长度）
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <param name="offset">起始偏移</param>
        /// <param name="length">数据长度</param>
        /// <param name="key">密钥（16字节）</param>
        /// <returns>64位哈希值</returns>
        public static ulong ComputeHash64(byte[] data, int offset, int length, byte[] key)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (offset < 0 || offset > data.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (length < 0 || offset + length > data.Length)
                throw new ArgumentOutOfRangeException(nameof(length));
            if (key == null || key.Length != 16)
                throw new ArgumentException("Key must be 16 bytes", nameof(key));

            return Compute(data, offset, length, key, 2, 4);
        }

        /// <summary>
        /// 使用 SipHash-4-8 计算 64 位哈希值（更安全，更慢）
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <param name="key">密钥（16字节）</param>
        /// <returns>64位哈希值</returns>
        public static ulong ComputeHash64Secure(byte[] data, byte[] key)
        {
            if (data == null)
                data = Array.Empty<byte>();
            if (key == null || key.Length != 16)
                throw new ArgumentException("Key must be 16 bytes", nameof(key));

            return Compute(data, 0, data.Length, key, 4, 8);
        }

        /// <summary>
        /// 使用 SipHash 计算 128 位哈希值（SipHash-2-4）
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <param name="key">密钥（16字节）</param>
        /// <returns>128位哈希值（两个64位值）</returns>
        public static (ulong Low, ulong High) ComputeHash128(byte[] data, byte[] key)
        {
            if (data == null)
                data = Array.Empty<byte>();
            if (key == null || key.Length != 16)
                throw new ArgumentException("Key must be 16 bytes", nameof(key));

            return Compute128(data, 0, data.Length, key, 2, 4);
        }

        /// <summary>
        /// 使用 SipHash 计算 128 位哈希值（SipHash-4-8，更安全）
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <param name="key">密钥（16字节）</param>
        /// <returns>128位哈希值（两个64位值）</returns>
        public static (ulong Low, ulong High) ComputeHash128Secure(byte[] data, byte[] key)
        {
            if (data == null)
                data = Array.Empty<byte>();
            if (key == null || key.Length != 16)
                throw new ArgumentException("Key must be 16 bytes", nameof(key));

            return Compute128(data, 0, data.Length, key, 4, 8);
        }

        /// <summary>
        /// 计算字符串的 SipHash-2-4 哈希值
        /// </summary>
        /// <param name="text">文本</param>
        /// <param name="key">密钥（16字节）</param>
        /// <returns>64位哈希值</returns>
        public static ulong ComputeString64(string text, byte[] key)
        {
            if (string.IsNullOrEmpty(text))
                return ComputeHash64(Array.Empty<byte>(), key);

            byte[] data = System.Text.Encoding.UTF8.GetBytes(text);
            return ComputeHash64(data, key);
        }

        /// <summary>
        /// 获取 SipHash-2-4 哈希值的十六进制表示
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <param name="key">密钥（16字节）</param>
        /// <returns>16字符的十六进制字符串</returns>
        public static string ComputeHex64(byte[] data, byte[] key)
        {
            ulong hash = ComputeHash64(data, key);
            return hash.ToString("x16");
        }

        /// <summary>
        /// 获取 SipHash-128 哈希值的十六进制表示
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <param name="key">密钥（16字节）</param>
        /// <returns>32字符的十六进制字符串</returns>
        public static string ComputeHex128(byte[] data, byte[] key)
        {
            var (low, high) = ComputeHash128(data, key);
            return high.ToString("x16") + low.ToString("x16");
        }

        /// <summary>
        /// 生成随机密钥
        /// </summary>
        /// <returns>16字节密钥</returns>
        public static byte[] GenerateKey()
        {
            byte[] key = new byte[16];
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            rng.GetBytes(key);
            return key;
        }

        /// <summary>
        /// 生成随机密钥并返回十六进制
        /// </summary>
        /// <returns>32字符的十六进制密钥</returns>
        public static string GenerateKeyHex()
        {
            byte[] key = GenerateKey();
            return BitConverter.ToString(key).Replace("-", "").ToLower();
        }

        /// <summary>
        /// 从十六进制字符串解析密钥
        /// </summary>
        /// <param name="hex">32字符的十六进制字符串</param>
        /// <returns>16字节密钥</returns>
        public static byte[] ParseKeyHex(string hex)
        {
            if (string.IsNullOrEmpty(hex) || hex.Length != 32)
                throw new ArgumentException("Hex key must be 32 characters", nameof(hex));

            byte[] key = new byte[16];
            for (int i = 0; i < 16; i++)
            {
                key[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return key;
        }

        #region 私有方法

        private static ulong Compute(byte[] data, int offset, int length, byte[] key, int cRounds, int dRounds)
        {
            // 初始化
            ulong k0 = BitConverter.ToUInt64(key, 0);
            ulong k1 = BitConverter.ToUInt64(key, 8);

            ulong v0 = k0 ^ 0x736f6d6570736575;
            ulong v1 = k1 ^ 0x646f72616e646f6d;
            ulong v2 = k0 ^ 0x6c7967656e657261;
            ulong v3 = k1 ^ 0x7465646279746573;

            int end = offset + length;
            int current = offset;

            // 处理完整的 8 字节块
            while (current + 8 <= end)
            {
                ulong m = BitConverter.ToUInt64(data, current);
                v3 ^= m;

                for (int i = 0; i < cRounds; i++)
                {
                    SipRound(ref v0, ref v1, ref v2, ref v3);
                }

                v0 ^= m;
                current += 8;
            }

            // 处理最后一个块
            ulong lastBlock = (ulong)length << 56;
            int remaining = end - current;
            int shift = 0;

            for (int i = 0; i < remaining; i++)
            {
                lastBlock |= (ulong)data[current + i] << shift;
                shift += 8;
            }

            v3 ^= lastBlock;

            for (int i = 0; i < cRounds; i++)
            {
                SipRound(ref v0, ref v1, ref v2, ref v3);
            }

            v0 ^= lastBlock;

            // 最终化
            v2 ^= 0xFF;

            for (int i = 0; i < dRounds; i++)
            {
                SipRound(ref v0, ref v1, ref v2, ref v3);
            }

            return v0 ^ v1 ^ v2 ^ v3;
        }

        private static (ulong Low, ulong High) Compute128(byte[] data, int offset, int length, byte[] key, int cRounds, int dRounds)
        {
            // 初始化
            ulong k0 = BitConverter.ToUInt64(key, 0);
            ulong k1 = BitConverter.ToUInt64(key, 8);

            ulong v0 = k0 ^ 0x736f6d6570736575;
            ulong v1 = k1 ^ 0x646f72616e646f6d;
            ulong v2 = k0 ^ 0x6c7967656e657261;
            ulong v3 = k1 ^ 0x7465646279746573;

            int end = offset + length;
            int current = offset;

            // 处理完整的 8 字节块
            while (current + 8 <= end)
            {
                ulong m = BitConverter.ToUInt64(data, current);
                v3 ^= m;

                for (int i = 0; i < cRounds; i++)
                {
                    SipRound(ref v0, ref v1, ref v2, ref v3);
                }

                v0 ^= m;
                current += 8;
            }

            // 处理最后一个块
            ulong lastBlock = (ulong)length << 56;
            int remaining = end - current;
            int shift = 0;

            for (int i = 0; i < remaining; i++)
            {
                lastBlock |= (ulong)data[current + i] << shift;
                shift += 8;
            }

            v3 ^= lastBlock;

            for (int i = 0; i < cRounds; i++)
            {
                SipRound(ref v0, ref v1, ref v2, ref v3);
            }

            v0 ^= lastBlock;

            // 最终化（128位输出）
            v2 ^= 0xEE;

            for (int i = 0; i < dRounds; i++)
            {
                SipRound(ref v0, ref v1, ref v2, ref v3);
            }

            ulong low = v0 ^ v1 ^ v2 ^ v3;

            // 第二轮
            v1 ^= 0xDD;

            for (int i = 0; i < dRounds; i++)
            {
                SipRound(ref v0, ref v1, ref v2, ref v3);
            }

            ulong high = v0 ^ v1 ^ v2 ^ v3;

            return (low, high);
        }

        private static void SipRound(ref ulong v0, ref ulong v1, ref ulong v2, ref ulong v3)
        {
            v0 += v1;
            v1 = RotateLeft(v1, 13);
            v1 ^= v0;
            v0 = RotateLeft(v0, 32);

            v2 += v3;
            v3 = RotateLeft(v3, 16);
            v3 ^= v2;

            v0 += v3;
            v3 = RotateLeft(v3, 21);
            v3 ^= v0;

            v2 += v1;
            v1 = RotateLeft(v1, 17);
            v1 ^= v2;
            v2 = RotateLeft(v2, 32);
        }

        private static ulong RotateLeft(ulong x, int n)
        {
            return (x << n) | (x >> (64 - n));
        }

        #endregion
    }
}
