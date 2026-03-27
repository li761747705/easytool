using System;
using System.Security.Cryptography;
using System.Text;

namespace EasyTool.SecurityCategory
{
    /// <summary>
    /// 密钥和Token生成工具类
    /// </summary>
    public static class KeyGeneratorUtil
    {
        private static readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();

        #region API Key 生成

        /// <summary>
        /// 生成 API Key
        /// </summary>
        /// <param name="length">密钥长度（字节数）</param>
        /// <param name="prefix">前缀</param>
        /// <returns>API Key</returns>
        public static string GenerateApiKey(int length = 32, string? prefix = null)
        {
            var bytes = new byte[length];
            _rng.GetBytes(bytes);
            var key = Convert.ToBase64String(bytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .TrimEnd('=');

            return string.IsNullOrEmpty(prefix) ? key : $"{prefix}_{key}";
        }

        /// <summary>
        /// 生成标准格式的 API Key（如 sk_xxx）
        /// </summary>
        /// <param name="prefix">前缀（默认 sk）</param>
        /// <returns>API Key</returns>
        public static string GenerateStandardApiKey(string prefix = "sk")
        {
            return GenerateApiKey(32, prefix);
        }

        /// <summary>
        /// 生成带有校验位的 API Key
        /// </summary>
        /// <param name="prefix">前缀</param>
        /// <param name="secret">签名密钥</param>
        /// <returns>带校验位的 API Key</returns>
        public static string GenerateApiKeyWithChecksum(string? prefix = null, string? secret = null)
        {
            var bytes = new byte[24];
            _rng.GetBytes(bytes);
            var keyPart = Convert.ToBase64String(bytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .TrimEnd('=');

            // 计算校验位
            var checksum = ComputeChecksum(keyPart, secret);
            var fullKey = $"{keyPart}_{checksum}";

            return string.IsNullOrEmpty(prefix) ? fullKey : $"{prefix}_{fullKey}";
        }

        /// <summary>
        /// 验证带校验位的 API Key
        /// </summary>
        /// <param name="apiKey">API Key</param>
        /// <param name="secret">签名密钥</param>
        /// <returns>是否有效</returns>
        public static bool ValidateApiKeyChecksum(string apiKey, string? secret = null)
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                return false;
            }

            var parts = apiKey.Split('_');
            if (parts.Length < 2)
            {
                return false;
            }

            var checksum = parts[^1];
            var keyPart = string.Join("_", parts[..^1]);

            var expectedChecksum = ComputeChecksum(keyPart, secret);
            return ConstantTimeEquals(checksum, expectedChecksum);
        }

        #endregion

        #region Token 生成

        /// <summary>
        /// 生成访问令牌
        /// </summary>
        /// <param name="length">Token 长度（字节数）</param>
        /// <returns>访问令牌</returns>
        public static string GenerateAccessToken(int length = 32)
        {
            var bytes = new byte[length];
            _rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// 生成刷新令牌
        /// </summary>
        /// <returns>刷新令牌</returns>
        public static string GenerateRefreshToken()
        {
            var bytes = new byte[64];
            _rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// 生成一次性令牌（OTP）
        /// </summary>
        /// <param name="length">OTP 长度</param>
        /// <returns>一次性令牌</returns>
        public static string GenerateOneTimePassword(int length = 6)
        {
            var bytes = new byte[length];
            _rng.GetBytes(bytes);

            var result = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                result.Append(bytes[i] % 10);
            }

            return result.ToString();
        }

        /// <summary>
        /// 生成一次性令牌（字母数字）
        /// </summary>
        /// <param name="length">Token 长度</param>
        /// <returns>一次性令牌</returns>
        public static string GenerateOneTimeToken(int length = 16)
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // 排除易混淆字符
            var bytes = new byte[length];
            _rng.GetBytes(bytes);

            var result = new char[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = chars[bytes[i] % chars.Length];
            }

            return new string(result);
        }

        /// <summary>
        /// 生成验证码
        /// </summary>
        /// <param name="length">验证码长度</param>
        /// <returns>验证码</returns>
        public static string GenerateVerificationCode(int length = 6)
        {
            return GenerateOneTimePassword(length);
        }

        #endregion

        #region 密钥生成

        /// <summary>
        /// 生成对称加密密钥
        /// </summary>
        /// <param name="keySize">密钥大小（位）</param>
        /// <returns>Base64 编码的密钥</returns>
        public static string GenerateSymmetricKey(int keySize = 256)
        {
            using var aes = Aes.Create();
            aes.KeySize = keySize;
            aes.GenerateKey();
            return Convert.ToBase64String(aes.Key);
        }

        /// <summary>
        /// 生成对称加密密钥（字节数组）
        /// </summary>
        /// <param name="keySize">密钥大小（位）</param>
        /// <returns>密钥字节数组</returns>
        public static byte[] GenerateSymmetricKeyBytes(int keySize = 256)
        {
            using var aes = Aes.Create();
            aes.KeySize = keySize;
            aes.GenerateKey();
            return aes.Key;
        }

        /// <summary>
        /// 生成 RSA 密钥对
        /// </summary>
        /// <param name="keySize">密钥大小（位）</param>
        /// <returns>包含公钥和私钥的元组</returns>
        public static (string PublicKey, string PrivateKey) GenerateRsaKeyPair(int keySize = 2048)
        {
            using var rsa = RSA.Create(keySize);
#if NET5_0_OR_GREATER
            var publicKey = rsa.ExportRSAPublicKeyPem();
            var privateKey = rsa.ExportRSAPrivateKeyPem();
#else
            // netstandard2.1 使用 ToXmlString 或手动转换为 PEM
            var publicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey());
            var privateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey());
#endif
            return (publicKey, privateKey);
        }

        /// <summary>
        /// 生成 RSA 密钥对（XML 格式）
        /// </summary>
        /// <param name="keySize">密钥大小（位）</param>
        /// <returns>包含公钥和私钥的元组</returns>
        public static (string PublicKey, string PrivateKey) GenerateRsaKeyPairXml(int keySize = 2048)
        {
            using var rsa = RSA.Create(keySize);
            var publicKey = rsa.ToXmlString(false);
            var privateKey = rsa.ToXmlString(true);
            return (publicKey, privateKey);
        }

        /// <summary>
        /// 生成 ECDSA 密钥对
        /// </summary>
        /// <param name="curve">椭圆曲线</param>
        /// <returns>包含公钥和私钥的元组</returns>
        public static (string PublicKey, string PrivateKey) GenerateEcdsaKeyPair(ECCurve? curve = null)
        {
            using var ecdsa = ECDsa.Create(curve ?? ECCurve.NamedCurves.nistP256);
#if NET5_0_OR_GREATER
            var publicKey = ecdsa.ExportSubjectPublicKeyInfoPem();
            var privateKey = ecdsa.ExportECPrivateKeyPem();
#else
            // netstandard2.1 使用 Base64 格式
            var publicKey = Convert.ToBase64String(ecdsa.ExportSubjectPublicKeyInfo());
            var privateKey = Convert.ToBase64String(ecdsa.ExportECPrivateKey());
#endif
            return (publicKey, privateKey);
        }

        /// <summary>
        /// 生成 HMAC 密钥
        /// </summary>
        /// <param name="length">密钥长度（字节数）</param>
        /// <returns>Base64 编码的密钥</returns>
        public static string GenerateHmacKey(int length = 64)
        {
            var bytes = new byte[length];
            _rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// 生成 IV（初始化向量）
        /// </summary>
        /// <param name="length">IV 长度（字节数）</param>
        /// <returns>Base64 编码的 IV</returns>
        public static string GenerateIV(int length = 16)
        {
            var bytes = new byte[length];
            _rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// 生成 Salt（盐值）
        /// </summary>
        /// <param name="length">Salt 长度（字节数）</param>
        /// <returns>Base64 编码的 Salt</returns>
        public static string GenerateSalt(int length = 16)
        {
            var bytes = new byte[length];
            _rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }

        #endregion

        #region 密钥派生

        /// <summary>
        /// 从密码派生密钥
        /// </summary>
        /// <param name="password">密码</param>
        /// <param name="salt">盐值</param>
        /// <param name="keyLength">密钥长度（字节数）</param>
        /// <param name="iterations">迭代次数</param>
        /// <returns>派生的密钥</returns>
        public static byte[] DeriveKeyFromPassword(string password, byte[] salt, int keyLength = 32, int iterations = 100000)
        {
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
            return pbkdf2.GetBytes(keyLength);
        }

        /// <summary>
        /// 从密码派生密钥（Base64 输出）
        /// </summary>
        /// <param name="password">密码</param>
        /// <param name="salt">盐值（Base64）</param>
        /// <param name="keyLength">密钥长度（字节数）</param>
        /// <param name="iterations">迭代次数</param>
        /// <returns>派生的密钥（Base64）</returns>
        public static string DeriveKeyFromPasswordBase64(string password, string salt, int keyLength = 32, int iterations = 100000)
        {
            var saltBytes = Convert.FromBase64String(salt);
            var key = DeriveKeyFromPassword(password, saltBytes, keyLength, iterations);
            return Convert.ToBase64String(key);
        }

        /// <summary>
        /// 使用 HKDF 派生密钥
        /// </summary>
        /// <param name="inputKeyMaterial">输入密钥材料</param>
        /// <param name="salt">盐值</param>
        /// <param name="info">上下文信息</param>
        /// <param name="outputLength">输出长度</param>
        /// <returns>派生的密钥</returns>
        public static byte[] DeriveKeyHKDF(byte[] inputKeyMaterial, byte[] salt, byte[]? info = null, int outputLength = 32)
        {
            using var hkdf = new HKDFSHA256();
            return hkdf.DeriveKey(inputKeyMaterial, salt, info ?? Array.Empty<byte>(), outputLength);
        }

        #endregion

        #region 辅助方法

        private static string ComputeChecksum(string data, string? secret)
        {
            var bytes = Encoding.UTF8.GetBytes(data);
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret ?? "default_checksum_key"));
            var hash = hmac.ComputeHash(bytes);
            return Convert.ToBase64String(hash)[..8];
        }

        private static bool ConstantTimeEquals(string a, string b)
        {
            if (a.Length != b.Length)
            {
                return false;
            }

            var result = 0;
            for (int i = 0; i < a.Length; i++)
            {
                result |= a[i] ^ b[i];
            }
            return result == 0;
        }

        #endregion
    }

    /// <summary>
    /// HKDF (HMAC-based Key Derivation Function) 实现
    /// </summary>
    internal class HKDFSHA256 : IDisposable
    {
        private const int HashLength = 32;

        public byte[] DeriveKey(byte[] inputKeyMaterial, byte[] salt, byte[] info, int outputLength)
        {
            if (outputLength > 255 * HashLength)
            {
                throw new ArgumentOutOfRangeException(nameof(outputLength), $"输出长度不能超过 {255 * HashLength} 字节");
            }

            // Extract
            var prk = Extract(inputKeyMaterial, salt);

            // Expand
            return Expand(prk, info, outputLength);
        }

        private byte[] Extract(byte[] inputKeyMaterial, byte[] salt)
        {
            using var hmac = new HMACSHA256(salt.Length == 0 ? new byte[HashLength] : salt);
            return hmac.ComputeHash(inputKeyMaterial);
        }

        private byte[] Expand(byte[] prk, byte[] info, int outputLength)
        {
            var result = new byte[outputLength];
            var blockCount = (int)Math.Ceiling((double)outputLength / HashLength);

            var previousBlock = Array.Empty<byte>();

            using var hmac = new HMACSHA256(prk);

            for (int i = 1; i <= blockCount; i++)
            {
                var input = new byte[previousBlock.Length + info.Length + 1];
                Buffer.BlockCopy(previousBlock, 0, input, 0, previousBlock.Length);
                Buffer.BlockCopy(info, 0, input, previousBlock.Length, info.Length);
                input[^1] = (byte)i;

                previousBlock = hmac.ComputeHash(input);

                var bytesToCopy = Math.Min(HashLength, outputLength - (i - 1) * HashLength);
                Buffer.BlockCopy(previousBlock, 0, result, (i - 1) * HashLength, bytesToCopy);
            }

            return result;
        }

        public void Dispose()
        {
            // HMACSHA256 会在 using 块中自动释放
        }
    }
}
