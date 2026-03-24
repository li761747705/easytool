using System;
using System.Security.Cryptography;
using System.Text;

namespace EasyTool.CodeCategory
{
    /// <summary>
    /// Argon2 密码哈希工具类
    /// Argon2 是2015年密码哈希竞赛的获胜者，是目前最安全的密码哈希算法
    /// 分为 Argon2d（抗GPU）、Argon2i（抗侧信道）、Argon2id（混合）
    /// </summary>
    public static class Argon2Util
    {
        // 默认参数
        private const int DefaultMemorySize = 65536;  // 64 MB
        private const int DefaultIterations = 3;
        private const int DefaultParallelism = 4;
        private const int DefaultHashLength = 32;
        private const int DefaultSaltLength = 16;

        /// <summary>
        /// 使用 Argon2id 哈希密码
        /// </summary>
        /// <param name="password">密码</param>
        /// <param name="salt">盐值（可选，默认自动生成）</param>
        /// <param name="memorySize">内存大小（KB，默认65536）</param>
        /// <param name="iterations">迭代次数（默认3）</param>
        /// <param name="parallelism">并行度（默认4）</param>
        /// <param name="hashLength">哈希长度（默认32）</param>
        /// <returns>哈希后的密码字符串</returns>
        public static string Hash(string password, byte[] salt = null, int memorySize = DefaultMemorySize,
            int iterations = DefaultIterations, int parallelism = DefaultParallelism, int hashLength = DefaultHashLength)
        {
            return Hash(password, salt, Argon2Type.Argon2id, memorySize, iterations, parallelism, hashLength);
        }

        /// <summary>
        /// 使用指定类型的 Argon2 哈希密码
        /// </summary>
        /// <param name="password">密码</param>
        /// <param name="salt">盐值</param>
        /// <param name="type">Argon2 类型</param>
        /// <param name="memorySize">内存大小（KB）</param>
        /// <param name="iterations">迭代次数</param>
        /// <param name="parallelism">并行度</param>
        /// <param name="hashLength">哈希长度</param>
        /// <returns>哈希后的密码字符串</returns>
        public static string Hash(string password, byte[] salt, Argon2Type type, int memorySize,
            int iterations, int parallelism, int hashLength)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty", nameof(password));

            ValidateParameters(memorySize, iterations, parallelism, hashLength);

            salt ??= GenerateSalt();

            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte[] hash = DeriveKey(passwordBytes, salt, type, memorySize, iterations, parallelism, hashLength);

            // 格式：$argon2<type>$v=19$m=<memory>,t=<iterations>,p=<parallelism>$<base64(salt)>$<base64(hash)>
            string typeStr = type switch
            {
                Argon2Type.Argon2d => "d",
                Argon2Type.Argon2i => "i",
                Argon2Type.Argon2id => "id",
                _ => "id"
            };

            return $"$argon2{typeStr}$v=19$m={memorySize},t={iterations},p={parallelism}" +
                   $"${Base64Encode(salt)}${Base64Encode(hash)}";
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
                var (type, memorySize, iterations, parallelism, salt, expectedHash) = ParseHash(hash);
                if (salt == null || expectedHash == null)
                    return false;

                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                byte[] computedHash = DeriveKey(passwordBytes, salt, type, memorySize, iterations, parallelism, expectedHash.Length);

                return ConstantTimeEquals(computedHash, expectedHash);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 使用 Argon2 派生密钥
        /// </summary>
        /// <param name="password">密码</param>
        /// <param name="salt">盐值</param>
        /// <param name="type">Argon2 类型</param>
        /// <param name="memorySize">内存大小（KB）</param>
        /// <param name="iterations">迭代次数</param>
        /// <param name="parallelism">并行度</param>
        /// <param name="hashLength">哈希长度</param>
        /// <returns>派生密钥</returns>
        public static byte[] DeriveKey(byte[] password, byte[] salt, Argon2Type type = Argon2Type.Argon2id,
            int memorySize = DefaultMemorySize, int iterations = DefaultIterations,
            int parallelism = DefaultParallelism, int hashLength = DefaultHashLength)
        {
            if (password == null || password.Length == 0)
                throw new ArgumentException("Password cannot be null or empty", nameof(password));

            if (salt == null || salt.Length < 8)
                throw new ArgumentException("Salt must be at least 8 bytes", nameof(salt));

            ValidateParameters(memorySize, iterations, parallelism, hashLength);

            // 简化版 Argon2 实现
            return SimplifiedArgon2(password, salt, type, memorySize, iterations, parallelism, hashLength);
        }

        /// <summary>
        /// 生成随机盐值
        /// </summary>
        /// <param name="length">盐值长度</param>
        /// <returns>盐值</returns>
        public static byte[] GenerateSalt(int length = DefaultSaltLength)
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
        /// <param name="memorySize">新的内存大小</param>
        /// <param name="iterations">新的迭代次数</param>
        /// <param name="parallelism">新的并行度</param>
        /// <returns>是否需要重新哈希</returns>
        public static bool NeedsRehash(string hash, int memorySize = DefaultMemorySize,
            int iterations = DefaultIterations, int parallelism = DefaultParallelism)
        {
            if (string.IsNullOrEmpty(hash))
                return true;

            try
            {
                var (_, oldMemory, oldIterations, oldParallelism, _, _) = ParseHash(hash);
                return oldMemory != memorySize || oldIterations != iterations || oldParallelism != parallelism;
            }
            catch
            {
                return true;
            }
        }

        #region 私有方法

        private static void ValidateParameters(int memorySize, int iterations, int parallelism, int hashLength)
        {
            if (memorySize < 8)
                throw new ArgumentException("Memory size must be at least 8 KB", nameof(memorySize));

            if (iterations < 1)
                throw new ArgumentException("Iterations must be at least 1", nameof(iterations));

            if (parallelism < 1)
                throw new ArgumentException("Parallelism must be at least 1", nameof(parallelism));

            if (hashLength < 4)
                throw new ArgumentException("Hash length must be at least 4 bytes", nameof(hashLength));
        }

        private static (Argon2Type type, int memory, int iterations, int parallelism, byte[] salt, byte[] hash) ParseHash(string hash)
        {
            if (!hash.StartsWith("$argon2"))
                throw new FormatException("Invalid Argon2 hash format");

            string[] parts = hash.Split('$');
            if (parts.Length < 6)
                throw new FormatException("Invalid Argon2 hash format");

            // 解析类型
            Argon2Type type = parts[1] switch
            {
                "argon2d" => Argon2Type.Argon2d,
                "argon2i" => Argon2Type.Argon2i,
                "argon2id" => Argon2Type.Argon2id,
                _ => throw new FormatException("Unknown Argon2 type")
            };

            // 解析版本（跳过 v=19）
            // 解析参数
            int memory = 0, iterations = 0, parallelism = 0;

            string[] parameters = parts[3].Split(',');
            foreach (string param in parameters)
            {
                string[] kv = param.Split('=');
                if (kv.Length != 2)
                    continue;

                switch (kv[0])
                {
                    case "m":
                        memory = int.Parse(kv[1]);
                        break;
                    case "t":
                        iterations = int.Parse(kv[1]);
                        break;
                    case "p":
                        parallelism = int.Parse(kv[1]);
                        break;
                }
            }

            byte[] salt = Base64Decode(parts[4]);
            byte[] expectedHash = Base64Decode(parts[5]);

            return (type, memory, iterations, parallelism, salt, expectedHash);
        }

        private static byte[] SimplifiedArgon2(byte[] password, byte[] salt, Argon2Type type,
            int memorySize, int iterations, int parallelism, int hashLength)
        {
            // 简化版 Argon2 实现 - 使用 HMAC-SHA256 作为核心
            // 注：这是一个简化的实现，不是完整的 Argon2 规范

            int segmentLength = memorySize * 1024 / (4 * parallelism);
            int blockCount = 4 * parallelism * segmentLength / 64;

            // 初始化内存块
            byte[][] memory = new byte[blockCount][];
            for (int i = 0; i < blockCount; i++)
            {
                memory[i] = new byte[64];
            }

            // 使用 HMAC-SHA256 生成初始块
            using var hmac = new HMACSHA256(password);

            // 填充第一个块
            byte[] initialInput = new byte[salt.Length + 8];
            Array.Copy(salt, initialInput, salt.Length);
            BitConverter.GetBytes((long)0).CopyTo(initialInput, salt.Length);
            byte[] hash1 = hmac.ComputeHash(initialInput);
            Array.Copy(hash1, memory[0], 32);

            BitConverter.GetBytes((long)1).CopyTo(initialInput, salt.Length);
            byte[] hash2 = hmac.ComputeHash(initialInput);
            Array.Copy(hash2, 0, memory[0], 32, 32);

            // 填充其余块
            for (int i = 1; i < blockCount; i++)
            {
                byte[] input = new byte[64 + 8];
                Array.Copy(memory[i - 1], input, 64);
                BitConverter.GetBytes((long)i).CopyTo(input, 64);

                byte[] h1 = hmac.ComputeHash(input);
                byte[] h2 = hmac.ComputeHash(h1);

                Array.Copy(h1, memory[i], 32);
                Array.Copy(h2, 0, memory[i], 32, 32);
            }

            // 执行迭代
            for (int iter = 0; iter < iterations; iter++)
            {
                for (int i = 0; i < blockCount; i++)
                {
                    // 根据类型选择引用块
                    int refIndex = type switch
                    {
                        Argon2Type.Argon2d => (int)(BitConverter.ToUInt32(memory[i], 0) % (uint)i),
                        Argon2Type.Argon2i => (iter * blockCount + i) % (i + 1),
                        Argon2Type.Argon2id => i % 2 == 0 ? (int)(BitConverter.ToUInt32(memory[i], 0) % (uint)(i + 1)) : (iter * blockCount + i) % (i + 1),
                        _ => i > 0 ? i - 1 : 0
                    };

                    if (refIndex < 0) refIndex = 0;
                    if (refIndex >= blockCount) refIndex = blockCount - 1;

                    // XOR 操作
                    for (int j = 0; j < 64; j++)
                    {
                        memory[i][j] ^= memory[refIndex][j];
                    }

                    // 压缩
                    byte[] compressed = hmac.ComputeHash(memory[i]);
                    Array.Copy(compressed, memory[i], 32);
                    byte[] compressed2 = hmac.ComputeHash(compressed);
                    Array.Copy(compressed2, 0, memory[i], 32, 32);
                }
            }

            // 生成最终哈希
            byte[] result = new byte[hashLength];
            byte[] finalBlock = memory[blockCount - 1];

            for (int i = 0; i < hashLength; i++)
            {
                result[i] = finalBlock[i % 64];
            }

            // 额外的混合
            for (int i = 0; i < hashLength; i++)
            {
                byte[] input = new byte[64 + 4];
                Array.Copy(finalBlock, input, 64);
                BitConverter.GetBytes(i).CopyTo(input, 64);
                byte[] mixed = hmac.ComputeHash(input);
                result[i] = mixed[i % 32];
            }

            return result;
        }

        private static string Base64Encode(byte[] data)
        {
            return Convert.ToBase64String(data)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        private static byte[] Base64Decode(string data)
        {
            string output = data
                .Replace('-', '+')
                .Replace('_', '/');

            switch (output.Length % 4)
            {
                case 2: output += "=="; break;
                case 3: output += "="; break;
            }

            return Convert.FromBase64String(output);
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

    /// <summary>
    /// Argon2 类型
    /// </summary>
    public enum Argon2Type
    {
        /// <summary>
        /// Argon2d - 抗 GPU 攻击
        /// </summary>
        Argon2d,

        /// <summary>
        /// Argon2i - 抗侧信道攻击
        /// </summary>
        Argon2i,

        /// <summary>
        /// Argon2id - 混合模式（推荐）
        /// </summary>
        Argon2id
    }
}
