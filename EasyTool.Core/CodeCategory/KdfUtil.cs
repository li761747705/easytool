using System;
using System.Security.Cryptography;
using System.Text;

namespace EasyTool.CodeCategory
{
    /// <summary>
    /// 密钥派生函数（KDF）工具类
    /// 提供安全的密钥派生方法
    /// </summary>
    public static class KdfUtil
    {
        #region PBKDF2

        /// <summary>
        /// 使用 PBKDF2 派生密钥
        /// </summary>
        /// <param name="password">密码</param>
        /// <param name="salt">盐值</param>
        /// <param name="iterations">迭代次数</param>
        /// <param name="keySize">密钥大小（字节）</param>
        /// <param name="hashAlgorithm">哈希算法</param>
        /// <returns>派生的密钥</returns>
        public static byte[] Pbkdf2(byte[] password, byte[] salt, int iterations = 100000, int keySize = 32, HashAlgorithmName hashAlgorithm = default)
        {
            if (hashAlgorithm == default)
                hashAlgorithm = HashAlgorithmName.SHA256;

            using var kdf = new Rfc2898DeriveBytes(password, salt, iterations, hashAlgorithm);
            return kdf.GetBytes(keySize);
        }

        /// <summary>
        /// 使用 PBKDF2 派生密钥
        /// </summary>
        /// <param name="password">密码</param>
        /// <param name="salt">盐值</param>
        /// <param name="iterations">迭代次数</param>
        /// <param name="keySize">密钥大小（字节）</param>
        /// <param name="hashAlgorithm">哈希算法</param>
        /// <returns>派生的密钥（十六进制字符串）</returns>
        public static string Pbkdf2Hex(string password, string salt, int iterations = 100000, int keySize = 32, HashAlgorithmName hashAlgorithm = default)
        {
            var passwordBytes = Encoding.UTF8.GetBytes(password);
            var saltBytes = Encoding.UTF8.GetBytes(salt);
            var key = Pbkdf2(passwordBytes, saltBytes, iterations, keySize, hashAlgorithm);
            return HexUtil.BytesToHex(key);
        }

        /// <summary>
        /// 使用 PBKDF2 派生密钥（Base64 编码）
        /// </summary>
        public static string Pbkdf2Base64(string password, string salt, int iterations = 100000, int keySize = 32, HashAlgorithmName hashAlgorithm = default)
        {
            var passwordBytes = Encoding.UTF8.GetBytes(password);
            var saltBytes = Encoding.UTF8.GetBytes(salt);
            var key = Pbkdf2(passwordBytes, saltBytes, iterations, keySize, hashAlgorithm);
            return Convert.ToBase64String(key);
        }

        /// <summary>
        /// 生成 PBKDF2 盐值
        /// </summary>
        /// <param name="size">盐值大小（字节）</param>
        /// <returns>盐值</returns>
        public static byte[] GenerateSalt(int size = 16)
        {
            using var rng = RandomNumberGenerator.Create();
            var salt = new byte[size];
            rng.GetBytes(salt);
            return salt;
        }

        /// <summary>
        /// 生成 PBKDF2 盐值（Base64 编码）
        /// </summary>
        public static string GenerateSaltBase64(int size = 16)
        {
            var salt = GenerateSalt(size);
            return Convert.ToBase64String(salt);
        }

        #endregion

        #region HKDF

        /// <summary>
        /// 使用 HKDF 派生密钥
        /// </summary>
        /// <param name="ikm">输入密钥材料</param>
        /// <param name="salt">盐值</param>
        /// <param name="info">上下文信息</param>
        /// <param name="keySize">输出密钥大小（字节）</param>
        /// <param name="hashAlgorithm">哈希算法</param>
        /// <returns>派生的密钥</returns>
        public static byte[] Hkdf(byte[] ikm, byte[]? salt = null, byte[]? info = null, int keySize = 32, HashAlgorithmName hashAlgorithm = default)
        {
            if (hashAlgorithm == default)
                hashAlgorithm = HashAlgorithmName.SHA256;

            salt ??= Array.Empty<byte>();
            info ??= Array.Empty<byte>();

            // Extract
            var prk = HmacExtract(ikm, salt, hashAlgorithm);

            // Expand
            return HkdfExpand(prk, info, keySize, hashAlgorithm);
        }

        /// <summary>
        /// HKDF Extract 步骤
        /// </summary>
        private static byte[] HmacExtract(byte[] ikm, byte[] salt, HashAlgorithmName hashAlgorithm)
        {
            int hashSize = GetHashSize(hashAlgorithm);
            if (salt.Length == 0)
                salt = new byte[hashSize];

            return ComputeHmac(ikm, salt, hashAlgorithm);
        }

        /// <summary>
        /// HKDF Expand 步骤
        /// </summary>
        private static byte[] HkdfExpand(byte[] prk, byte[] info, int keySize, HashAlgorithmName hashAlgorithm)
        {
            int hashSize = GetHashSize(hashAlgorithm);
            int n = (keySize + hashSize - 1) / hashSize;

            var result = new byte[keySize];
            var t = Array.Empty<byte>();
            int offset = 0;

            for (int i = 1; i <= n; i++)
            {
                var data = new byte[t.Length + info.Length + 1];
                Buffer.BlockCopy(t, 0, data, 0, t.Length);
                Buffer.BlockCopy(info, 0, data, t.Length, info.Length);
                data[data.Length - 1] = (byte)i;

                t = ComputeHmac(data, prk, hashAlgorithm);
                int toCopy = Math.Min(hashSize, keySize - offset);
                Buffer.BlockCopy(t, 0, result, offset, toCopy);
                offset += toCopy;
            }

            return result;
        }

        /// <summary>
        /// 计算 HMAC
        /// </summary>
        private static byte[] ComputeHmac(byte[] data, byte[] key, HashAlgorithmName hashAlgorithm)
        {
            using var hmac = CreateHmac(key, hashAlgorithm);
            return hmac.ComputeHash(data);
        }

        /// <summary>
        /// 创建 HMAC 实例
        /// </summary>
        private static HMAC CreateHmac(byte[] key, HashAlgorithmName hashAlgorithm)
        {
            if (hashAlgorithm == HashAlgorithmName.SHA256)
                return new HMACSHA256(key);
            if (hashAlgorithm == HashAlgorithmName.SHA384)
                return new HMACSHA384(key);
            if (hashAlgorithm == HashAlgorithmName.SHA512)
                return new HMACSHA512(key);
            if (hashAlgorithm == HashAlgorithmName.SHA1)
                return new HMACSHA1(key);

            throw new NotSupportedException($"不支持的哈希算法: {hashAlgorithm.Name}");
        }

        /// <summary>
        /// 获取哈希大小
        /// </summary>
        private static int GetHashSize(HashAlgorithmName hashAlgorithm)
        {
            if (hashAlgorithm == HashAlgorithmName.SHA256) return 32;
            if (hashAlgorithm == HashAlgorithmName.SHA384) return 48;
            if (hashAlgorithm == HashAlgorithmName.SHA512) return 64;
            if (hashAlgorithm == HashAlgorithmName.SHA1) return 20;

            throw new NotSupportedException($"不支持的哈希算法: {hashAlgorithm.Name}");
        }

        #endregion

        #region SP 800-108 Counter Mode KDF

        /// <summary>
        /// 使用 NIST SP 800-108 Counter Mode 派生密钥
        /// </summary>
        /// <param name="keyDerivationKey">密钥派生密钥</param>
        /// <param name="label">标签</param>
        /// <param name="context">上下文</param>
        /// <param name="keySize">输出密钥大小（字节）</param>
        /// <param name="hashAlgorithm">哈希算法</param>
        /// <returns>派生的密钥</returns>
        public static byte[] Sp800_108_Counter(byte[] keyDerivationKey, byte[] label, byte[] context, int keySize = 32, HashAlgorithmName hashAlgorithm = default)
        {
            if (hashAlgorithm == default)
                hashAlgorithm = HashAlgorithmName.SHA256;

            int hashSize = GetHashSize(hashAlgorithm);
            int n = (keySize + hashSize - 1) / hashSize;

            var result = new byte[keySize];
            int offset = 0;

            for (int i = 1; i <= n; i++)
            {
                // [i] || Label || 0x00 || Context || [L]
                var data = new byte[4 + label.Length + 1 + context.Length + 4];
                int pos = 0;

                // Counter (4 bytes, big-endian)
                data[pos++] = (byte)(i >> 24);
                data[pos++] = (byte)(i >> 16);
                data[pos++] = (byte)(i >> 8);
                data[pos++] = (byte)i;

                // Label
                Buffer.BlockCopy(label, 0, data, pos, label.Length);
                pos += label.Length;

                // Separator
                data[pos++] = 0x00;

                // Context
                Buffer.BlockCopy(context, 0, data, pos, context.Length);
                pos += context.Length;

                // Length in bits (4 bytes, big-endian)
                int l = keySize * 8;
                data[pos++] = (byte)(l >> 24);
                data[pos++] = (byte)(l >> 16);
                data[pos++] = (byte)(l >> 8);
                data[pos++] = (byte)l;

                var hash = ComputeHmac(data, keyDerivationKey, hashAlgorithm);
                int toCopy = Math.Min(hashSize, keySize - offset);
                Buffer.BlockCopy(hash, 0, result, offset, toCopy);
                offset += toCopy;
            }

            return result;
        }

        #endregion

        #region 静态工具方法

        /// <summary>
        /// 从密码生成加密密钥
        /// </summary>
        /// <param name="password">密码</param>
        /// <param name="salt">盐值</param>
        /// <param name="iterations">迭代次数</param>
        /// <returns>生成的密钥信息</returns>
        public static KeyDerivationResult DeriveKeyFromPassword(string password, byte[]? salt = null, int iterations = 100000)
        {
            salt ??= GenerateSalt(16);
            var key = Pbkdf2(Encoding.UTF8.GetBytes(password), salt, iterations, 32);
            var iv = Pbkdf2(Encoding.UTF8.GetBytes(password), salt, iterations, 16);

            return new KeyDerivationResult
            {
                Key = key,
                IV = iv,
                Salt = salt,
                Iterations = iterations
            };
        }

        #endregion
    }

    /// <summary>
    /// 密钥派生结果
    /// </summary>
    public class KeyDerivationResult
    {
        /// <summary>
        /// 派生的密钥
        /// </summary>
        public byte[] Key { get; set; } = Array.Empty<byte>();

        /// <summary>
        /// 初始化向量
        /// </summary>
        public byte[] IV { get; set; } = Array.Empty<byte>();

        /// <summary>
        /// 使用的盐值
        /// </summary>
        public byte[] Salt { get; set; } = Array.Empty<byte>();

        /// <summary>
        /// 迭代次数
        /// </summary>
        public int Iterations { get; set; }

        /// <summary>
        /// 密钥（Base64 编码）
        /// </summary>
        public string KeyBase64 => Convert.ToBase64String(Key);

        /// <summary>
        /// IV（Base64 编码）
        /// </summary>
        public string IVBase64 => Convert.ToBase64String(IV);

        /// <summary>
        /// 盐值（Base64 编码）
        /// </summary>
        public string SaltBase64 => Convert.ToBase64String(Salt);
    }
}
