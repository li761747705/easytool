using System;
using System.Security.Cryptography;
using System.Text;

namespace EasyTool.CodeCategory
{
    /// <summary>
    /// Scrypt 密码哈希工具类
    /// Scrypt 是一种内存密集型的密钥派生函数，专门设计用于抵抗硬件攻击
    /// 常用于加密货币钱包和密码存储
    /// </summary>
    public static class ScryptUtil
    {
        // 默认参数
        private const int DefaultN = 32768;  // CPU/内存成本参数（必须为2的幂）
        private const int DefaultR = 8;      // 块大小参数
        private const int DefaultP = 1;      // 并行化参数
        private const int DefaultDkLen = 32; // 派生密钥长度

        /// <summary>
        /// 使用 Scrypt 哈希密码
        /// </summary>
        /// <param name="password">密码</param>
        /// <param name="salt">盐值（可选，默认自动生成）</param>
        /// <param name="n">CPU/内存成本参数（必须为2的幂，默认32768）</param>
        /// <param name="r">块大小参数（默认8）</param>
        /// <param name="p">并行化参数（默认1）</param>
        /// <param name="dkLen">派生密钥长度（默认32字节）</param>
        /// <returns>哈希后的密码字符串</returns>
        public static string Hash(string password, byte[] salt = null, int n = DefaultN, int r = DefaultR, int p = DefaultP, int dkLen = DefaultDkLen)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty", nameof(password));

            ValidateParameters(n, r, p);

            salt ??= GenerateSalt();
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

            byte[] hash = DeriveKey(passwordBytes, salt, n, r, p, dkLen);

            // 格式：$scrypt$N=<n>,r=<r>,p=<p>$<base64(salt)>$<base64(hash)>
            return $"$scrypt$N={n},r={r},p={p}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
        }

        /// <summary>
        /// 验证密码
        /// </summary>
        /// <param name="password">密码</param>
        /// <param name="hash">哈希字符串</param>
        /// <returns>是否匹配</returns>
        public static bool Verify(string password, string hash)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hash))
                return false;

            try
            {
                var (n, r, p, salt, expectedHash) = ParseHash(hash);
                if (salt == null || expectedHash == null)
                    return false;

                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                byte[] computedHash = DeriveKey(passwordBytes, salt, n, r, p, expectedHash.Length);

                return ConstantTimeEquals(computedHash, expectedHash);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 使用 Scrypt 派生密钥
        /// </summary>
        /// <param name="password">密码</param>
        /// <param name="salt">盐值</param>
        /// <param name="n">CPU/内存成本参数</param>
        /// <param name="r">块大小参数</param>
        /// <param name="p">并行化参数</param>
        /// <param name="dkLen">派生密钥长度</param>
        /// <returns>派生密钥</returns>
        public static byte[] DeriveKey(byte[] password, byte[] salt, int n = DefaultN, int r = DefaultR, int p = DefaultP, int dkLen = DefaultDkLen)
        {
            if (password == null || password.Length == 0)
                throw new ArgumentException("Password cannot be null or empty", nameof(password));

            if (salt == null || salt.Length == 0)
                throw new ArgumentException("Salt cannot be null or empty", nameof(salt));

            ValidateParameters(n, r, p);

            // 使用 PBKDF2-HMAC-SHA256 进行初始密钥派生
            byte[] b = PBKDF2(password, salt, 1, p * 128 * r);

            // 对每个块执行 ROMix
            for (int i = 0; i < p; i++)
            {
                int offset = i * 128 * r;
                ROMix(b, offset, n, r);
            }

            // 再次使用 PBKDF2 派生最终密钥
            return PBKDF2(password, b, 1, dkLen);
        }

        /// <summary>
        /// 生成随机盐值
        /// </summary>
        /// <param name="length">盐值长度（默认16字节）</param>
        /// <returns>盐值</returns>
        public static byte[] GenerateSalt(int length = 16)
        {
            byte[] salt = new byte[length];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(salt);
            return salt;
        }

        /// <summary>
        /// 检查是否需要重新哈希
        /// </summary>
        /// <param name="hash">现有哈希</param>
        /// <param name="n">新的CPU/内存成本</param>
        /// <param name="r">新的块大小</param>
        /// <param name="p">新的并行化参数</param>
        /// <returns>是否需要重新哈希</returns>
        public static bool NeedsRehash(string hash, int n = DefaultN, int r = DefaultR, int p = DefaultP)
        {
            if (string.IsNullOrEmpty(hash))
                return true;

            try
            {
                var (oldN, oldR, oldP, _, _) = ParseHash(hash);
                return oldN != n || oldR != r || oldP != p;
            }
            catch
            {
                return true;
            }
        }

        #region 私有方法

        private static void ValidateParameters(int n, int r, int p)
        {
            if (n <= 1 || (n & (n - 1)) != 0)
                throw new ArgumentException("N must be a power of 2 greater than 1", nameof(n));

            if (r <= 0)
                throw new ArgumentException("R must be greater than 0", nameof(r));

            if (p <= 0)
                throw new ArgumentException("P must be greater than 0", nameof(p));

            // 检查内存使用限制
            long blockSize = 128 * r * p;
            long totalMemory = blockSize * n;

            if (totalMemory > int.MaxValue)
                throw new ArgumentException("Parameters would use too much memory");
        }

        private static (int n, int r, int p, byte[] salt, byte[] hash) ParseHash(string hash)
        {
            if (!hash.StartsWith("$scrypt$"))
                return (0, 0, 0, null, null);

            string[] parts = hash.Split('$');
            if (parts.Length < 5)
                return (0, 0, 0, null, null);

            // 解析参数
            string[] parameters = parts[2].Split(',');
            int n = 0, r = 0, p = 0;

            foreach (string param in parameters)
            {
                string[] kv = param.Split('=');
                if (kv.Length != 2)
                    continue;

                switch (kv[0])
                {
                    case "N":
                        n = int.Parse(kv[1]);
                        break;
                    case "r":
                        r = int.Parse(kv[1]);
                        break;
                    case "p":
                        p = int.Parse(kv[1]);
                        break;
                }
            }

            byte[] salt = Convert.FromBase64String(parts[3]);
            byte[] expectedHash = Convert.FromBase64String(parts[4]);

            return (n, r, p, salt, expectedHash);
        }

        private static byte[] PBKDF2(byte[] password, byte[] salt, int iterations, int dkLen)
        {
            using var hmac = new HMACSHA256(password);
            byte[] result = new byte[dkLen];
            int hashLen = hmac.HashSize / 8;
            int blocks = (dkLen + hashLen - 1) / hashLen;

            for (int block = 1; block <= blocks; block++)
            {
                byte[] blockBytes = BitConverter.GetBytes(block);
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(blockBytes);

                byte[] input = new byte[salt.Length + 4];
                Array.Copy(salt, input, salt.Length);
                Array.Copy(blockBytes, 0, input, salt.Length, 4);

                byte[] u = hmac.ComputeHash(input);
                byte[] output = new byte[u.Length];
                Array.Copy(u, output, u.Length);

                for (int i = 1; i < iterations; i++)
                {
                    u = hmac.ComputeHash(u);
                    for (int j = 0; j < output.Length; j++)
                    {
                        output[j] ^= u[j];
                    }
                }

                int offset = (block - 1) * hashLen;
                int length = Math.Min(hashLen, dkLen - offset);
                Array.Copy(output, 0, result, offset, length);
            }

            return result;
        }

        private static void ROMix(byte[] b, int offset, int n, int r)
        {
            int blockSize = 128 * r;
            uint[] v = new uint[n * blockSize / 4];
            uint[] x = new uint[blockSize / 4];

            // 将字节转换为 uint 数组
            for (int i = 0; i < blockSize / 4; i++)
            {
                x[i] = BitConverter.ToUInt32(b, offset + i * 4);
            }

            // 第一步：填充 V
            for (int i = 0; i < n; i++)
            {
                Array.Copy(x, 0, v, i * blockSize / 4, blockSize / 4);
                BlockMix(x, r);
            }

            // 第二步：混合
            for (int i = 0; i < n; i++)
            {
                int j = (int)(Integerify(x) % (ulong)n);
                for (int k = 0; k < blockSize / 4; k++)
                {
                    x[k] ^= v[j * blockSize / 4 + k];
                }
                BlockMix(x, r);
            }

            // 将结果写回
            for (int i = 0; i < blockSize / 4; i++)
            {
                byte[] bytes = BitConverter.GetBytes(x[i]);
                Array.Copy(bytes, 0, b, offset + i * 4, 4);
            }
        }

        private static void BlockMix(uint[] b, int r)
        {
            int blockSize = 128 * r;
            uint[] x = new uint[64];
            uint[] y = new uint[blockSize];

            // 复制最后一个块到 x
            Array.Copy(b, b.Length - 64, x, 0, 64);

            // 混合每个块
            for (int i = 0; i < blockSize / 64; i++)
            {
                for (int j = 0; j < 64; j++)
                {
                    x[j] ^= b[i * 64 + j];
                }
                Salsa20_8(x);

                // 根据位置决定输出位置
                if (i % 2 == 0)
                {
                    Array.Copy(x, 0, y, i / 2 * 64, 64);
                }
                else
                {
                    Array.Copy(x, 0, y, (blockSize / 64 / 2 + i / 2) * 64, 64);
                }
            }

            Array.Copy(y, b, blockSize);
        }

        private static void Salsa20_8(uint[] x)
        {
            uint[] z = new uint[16];
            Array.Copy(x, z, 16);

            for (int i = 0; i < 8; i += 2)
            {
                z[4] ^= RotateLeft(z[0] + z[12], 7);
                z[8] ^= RotateLeft(z[4] + z[0], 9);
                z[12] ^= RotateLeft(z[8] + z[4], 13);
                z[0] ^= RotateLeft(z[12] + z[8], 18);
                z[9] ^= RotateLeft(z[5] + z[1], 7);
                z[13] ^= RotateLeft(z[9] + z[5], 9);
                z[1] ^= RotateLeft(z[13] + z[9], 13);
                z[5] ^= RotateLeft(z[1] + z[13], 18);
                z[14] ^= RotateLeft(z[10] + z[6], 7);
                z[2] ^= RotateLeft(z[14] + z[10], 9);
                z[6] ^= RotateLeft(z[2] + z[14], 13);
                z[10] ^= RotateLeft(z[6] + z[2], 18);
                z[3] ^= RotateLeft(z[15] + z[11], 7);
                z[7] ^= RotateLeft(z[3] + z[15], 9);
                z[11] ^= RotateLeft(z[7] + z[3], 13);
                z[15] ^= RotateLeft(z[11] + z[7], 18);

                z[1] ^= RotateLeft(z[0] + z[3], 7);
                z[2] ^= RotateLeft(z[1] + z[0], 9);
                z[3] ^= RotateLeft(z[2] + z[1], 13);
                z[0] ^= RotateLeft(z[3] + z[2], 18);
                z[6] ^= RotateLeft(z[5] + z[4], 7);
                z[7] ^= RotateLeft(z[6] + z[5], 9);
                z[4] ^= RotateLeft(z[7] + z[6], 13);
                z[5] ^= RotateLeft(z[4] + z[7], 18);
                z[11] ^= RotateLeft(z[10] + z[9], 7);
                z[8] ^= RotateLeft(z[11] + z[10], 9);
                z[9] ^= RotateLeft(z[8] + z[11], 13);
                z[10] ^= RotateLeft(z[9] + z[8], 18);
                z[12] ^= RotateLeft(z[15] + z[14], 7);
                z[13] ^= RotateLeft(z[12] + z[15], 9);
                z[14] ^= RotateLeft(z[13] + z[12], 13);
                z[15] ^= RotateLeft(z[14] + z[13], 18);
            }

            for (int i = 0; i < 16; i++)
            {
                x[i] += z[i];
            }
        }

        private static ulong Integerify(uint[] b)
        {
            return ((ulong)b[19] << 32) | b[0];
        }

        private static uint RotateLeft(uint x, int n)
        {
            return (x << n) | (x >> (32 - n));
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
    }
}
