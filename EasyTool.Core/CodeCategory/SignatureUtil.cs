using System;
using System.Security.Cryptography;
using System.Text;

namespace EasyTool.CodeCategory
{
    /// <summary>
    /// 数字签名工具类
    /// 提供 RSA、ECDSA、DSA 等签名和验证功能
    /// </summary>
    public static class SignatureUtil
    {
        #region RSA 签名

        /// <summary>
        /// 使用 RSA 创建签名
        /// </summary>
        /// <param name="data">要签名的数据</param>
        /// <param name="privateKey">RSA 私钥（PKCS#8 或 XML 格式）</param>
        /// <param name="hashAlgorithm">哈希算法</param>
        /// <param name="padding">签名填充模式</param>
        /// <returns>签名</returns>
        public static byte[] SignWithRsa(byte[] data, string privateKey, HashAlgorithmName hashAlgorithm, RSASignaturePadding? padding = null)
        {
            padding ??= RSASignaturePadding.Pkcs1;

            using var rsa = CreateRsaFromKey(privateKey);
            return rsa.SignData(data, hashAlgorithm, padding);
        }

        /// <summary>
        /// 使用 RSA 创建签名（PSS 填充）
        /// </summary>
        public static byte[] SignWithRsaPss(byte[] data, string privateKey, HashAlgorithmName hashAlgorithm = default)
        {
            if (hashAlgorithm == default)
                hashAlgorithm = HashAlgorithmName.SHA256;

            return SignWithRsa(data, privateKey, hashAlgorithm, RSASignaturePadding.Pss);
        }

        /// <summary>
        /// 使用 RSA 创建签名（PKCS#1 填充）
        /// </summary>
        public static byte[] SignWithRsaPkcs1(byte[] data, string privateKey, HashAlgorithmName hashAlgorithm = default)
        {
            if (hashAlgorithm == default)
                hashAlgorithm = HashAlgorithmName.SHA256;

            return SignWithRsa(data, privateKey, hashAlgorithm, RSASignaturePadding.Pkcs1);
        }

        /// <summary>
        /// 验证 RSA 签名
        /// </summary>
        /// <param name="data">原始数据</param>
        /// <param name="signature">签名</param>
        /// <param name="publicKey">RSA 公钥</param>
        /// <param name="hashAlgorithm">哈希算法</param>
        /// <param name="padding">签名填充模式</param>
        /// <returns>签名是否有效</returns>
        public static bool VerifyRsaSignature(byte[] data, byte[] signature, string publicKey, HashAlgorithmName hashAlgorithm, RSASignaturePadding? padding = null)
        {
            padding ??= RSASignaturePadding.Pkcs1;

            try
            {
                using var rsa = CreateRsaFromKey(publicKey);
                return rsa.VerifyData(data, signature, hashAlgorithm, padding);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 验证 RSA-PSS 签名
        /// </summary>
        public static bool VerifyRsaPssSignature(byte[] data, byte[] signature, string publicKey, HashAlgorithmName hashAlgorithm = default)
        {
            if (hashAlgorithm == default)
                hashAlgorithm = HashAlgorithmName.SHA256;

            return VerifyRsaSignature(data, signature, publicKey, hashAlgorithm, RSASignaturePadding.Pss);
        }

        /// <summary>
        /// 验证 RSA-PKCS1 签名
        /// </summary>
        public static bool VerifyRsaPkcs1Signature(byte[] data, byte[] signature, string publicKey, HashAlgorithmName hashAlgorithm = default)
        {
            if (hashAlgorithm == default)
                hashAlgorithm = HashAlgorithmName.SHA256;

            return VerifyRsaSignature(data, signature, publicKey, hashAlgorithm, RSASignaturePadding.Pkcs1);
        }

        #endregion

        #region ECDSA 签名

        /// <summary>
        /// 使用 ECDSA 创建签名
        /// </summary>
        /// <param name="data">要签名的数据</param>
        /// <param name="privateKey">ECDSA 私钥</param>
        /// <param name="hashAlgorithm">哈希算法</param>
        /// <returns>签名</returns>
        public static byte[] SignWithEcdsa(byte[] data, string privateKey, HashAlgorithmName hashAlgorithm = default)
        {
            if (hashAlgorithm == default)
                hashAlgorithm = HashAlgorithmName.SHA256;

            using var ecdsa = CreateEcdsaFromKey(privateKey);
            return ecdsa.SignData(data, hashAlgorithm);
        }

        /// <summary>
        /// 使用 ECDSA 创建签名（DER 格式）
        /// </summary>
        public static byte[] SignWithEcdsaDer(byte[] data, string privateKey, HashAlgorithmName hashAlgorithm = default)
        {
            if (hashAlgorithm == default)
                hashAlgorithm = HashAlgorithmName.SHA256;

            using var ecdsa = CreateEcdsaFromKey(privateKey);
            // 在 netstandard2.1 中使用默认签名格式
            return ecdsa.SignData(data, hashAlgorithm);
        }

        /// <summary>
        /// 验证 ECDSA 签名
        /// </summary>
        /// <param name="data">原始数据</param>
        /// <param name="signature">签名</param>
        /// <param name="publicKey">ECDSA 公钥</param>
        /// <param name="hashAlgorithm">哈希算法</param>
        /// <returns>签名是否有效</returns>
        public static bool VerifyEcdsaSignature(byte[] data, byte[] signature, string publicKey, HashAlgorithmName hashAlgorithm = default)
        {
            if (hashAlgorithm == default)
                hashAlgorithm = HashAlgorithmName.SHA256;

            try
            {
                using var ecdsa = CreateEcdsaFromKey(publicKey);
                return ecdsa.VerifyData(data, signature, hashAlgorithm);
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region DSA 签名

        /// <summary>
        /// 使用 DSA 创建签名
        /// </summary>
        /// <param name="data">要签名的数据</param>
        /// <param name="privateKey">DSA 私钥</param>
        /// <param name="hashAlgorithm">哈希算法</param>
        /// <returns>签名</returns>
        public static byte[] SignWithDsa(byte[] data, string privateKey, HashAlgorithmName hashAlgorithm = default)
        {
            if (hashAlgorithm == default)
                hashAlgorithm = HashAlgorithmName.SHA256;

            using var dsa = CreateDsaFromKey(privateKey);
            return dsa.SignData(data, hashAlgorithm);
        }

        /// <summary>
        /// 验证 DSA 签名
        /// </summary>
        public static bool VerifyDsaSignature(byte[] data, byte[] signature, string publicKey, HashAlgorithmName hashAlgorithm = default)
        {
            if (hashAlgorithm == default)
                hashAlgorithm = HashAlgorithmName.SHA256;

            try
            {
                using var dsa = CreateDsaFromKey(publicKey);
                return dsa.VerifyData(data, signature, hashAlgorithm);
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region HMAC 签名

        /// <summary>
        /// 创建 HMAC 签名
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="key">密钥</param>
        /// <param name="hashAlgorithm">哈希算法</param>
        /// <returns>签名</returns>
        public static byte[] SignWithHmac(byte[] data, byte[] key, HashAlgorithmName hashAlgorithm = default)
        {
            if (hashAlgorithm == default)
                hashAlgorithm = HashAlgorithmName.SHA256;

            using var hmac = CreateHmac(key, hashAlgorithm);
            return hmac.ComputeHash(data);
        }

        /// <summary>
        /// 验证 HMAC 签名
        /// </summary>
        public static bool VerifyHmacSignature(byte[] data, byte[] signature, byte[] key, HashAlgorithmName hashAlgorithm = default)
        {
            if (hashAlgorithm == default)
                hashAlgorithm = HashAlgorithmName.SHA256;

            var computed = SignWithHmac(data, key, hashAlgorithm);
            return HmacUtil.ConstantTimeEquals(computed, signature);
        }

        #endregion

        #region 密钥生成

        /// <summary>
        /// 生成 RSA 密钥对
        /// </summary>
        /// <param name="keySize">密钥大小（位）</param>
        /// <returns>密钥对</returns>
        public static KeyPair GenerateRsaKeyPair(int keySize = 2048)
        {
            using var rsa = RSA.Create(keySize);
            return new KeyPair
            {
                PrivateKey = ExportRsaPrivateKeyPem(rsa),
                PublicKey = ExportRsaPublicKeyPem(rsa),
                PrivateKeyPkcs8 = ExportPkcs8PrivateKeyPem(rsa)
            };
        }

        /// <summary>
        /// 生成 ECDSA 密钥对
        /// </summary>
        /// <param name="curve">椭圆曲线</param>
        /// <returns>密钥对</returns>
        public static KeyPair GenerateEcdsaKeyPair(ECCurve? curve = null)
        {
            using var ecdsa = curve.HasValue
                ? ECDsa.Create(curve.Value)
                : ECDsa.Create(ECCurve.NamedCurves.nistP256);

            return new KeyPair
            {
                PrivateKey = ExportEcPrivateKeyPem(ecdsa),
                PublicKey = ExportSubjectPublicKeyInfoPem(ecdsa)
            };
        }

        #endregion

        #region 辅助方法

        private static RSA CreateRsaFromKey(string key)
        {
            var rsa = RSA.Create();

            if (key.StartsWith("-----BEGIN"))
            {
                // PEM 格式
                ImportFromPem(rsa, key);
            }
            else if (key.TrimStart().StartsWith("<RSAKeyValue"))
            {
                // XML 格式
                rsa.FromXmlString(key);
            }
            else
            {
                // 尝试作为原始 Base64
                var keyBytes = Convert.FromBase64String(key);
                rsa.ImportRSAPrivateKey(keyBytes, out _);
            }

            return rsa;
        }

        private static ECDsa CreateEcdsaFromKey(string key)
        {
            var ecdsa = ECDsa.Create();

            if (key.StartsWith("-----BEGIN"))
            {
                ImportFromPem(ecdsa, key);
            }
            else
            {
                var keyBytes = Convert.FromBase64String(key);
                ecdsa.ImportECPrivateKey(keyBytes, out _);
            }

            return ecdsa;
        }

        private static DSA CreateDsaFromKey(string key)
        {
            var dsa = DSA.Create();

            if (key.StartsWith("-----BEGIN"))
            {
                ImportFromPem(dsa, key);
            }
            else
            {
                var keyBytes = Convert.FromBase64String(key);
                dsa.ImportPkcs8PrivateKey(keyBytes, out _);
            }

            return dsa;
        }

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

        #region PEM 导出辅助方法

        private static string ExportRsaPrivateKeyPem(RSA rsa)
        {
#if NETSTANDARD2_1
            var keyBytes = rsa.ExportRSAPrivateKey();
            return FormatPem("RSA PRIVATE KEY", keyBytes);
#else
            return rsa.ExportRSAPrivateKeyPem();
#endif
        }

        private static string ExportRsaPublicKeyPem(RSA rsa)
        {
#if NETSTANDARD2_1
            var keyBytes = rsa.ExportRSAPublicKey();
            return FormatPem("RSA PUBLIC KEY", keyBytes);
#else
            return rsa.ExportRSAPublicKeyPem();
#endif
        }

        private static string ExportPkcs8PrivateKeyPem(RSA rsa)
        {
#if NETSTANDARD2_1
            var keyBytes = rsa.ExportPkcs8PrivateKey();
            return FormatPem("PRIVATE KEY", keyBytes);
#else
            return rsa.ExportPkcs8PrivateKeyPem();
#endif
        }

        private static string ExportEcPrivateKeyPem(ECDsa ecdsa)
        {
#if NETSTANDARD2_1
            var keyBytes = ecdsa.ExportECPrivateKey();
            return FormatPem("EC PRIVATE KEY", keyBytes);
#else
            return ecdsa.ExportECPrivateKeyPem();
#endif
        }

        private static string ExportSubjectPublicKeyInfoPem(ECDsa ecdsa)
        {
#if NETSTANDARD2_1
            var keyBytes = ecdsa.ExportSubjectPublicKeyInfo();
            return FormatPem("PUBLIC KEY", keyBytes);
#else
            return ecdsa.ExportSubjectPublicKeyInfoPem();
#endif
        }

        private static void ImportFromPem(AsymmetricAlgorithm algorithm, string pem)
        {
#if NETSTANDARD2_1
            // 手动解析 PEM 格式
            var lines = pem.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var base64 = new StringBuilder();
            var inKey = false;
            string? keyType = null;

            foreach (var line in lines)
            {
                if (line.StartsWith("-----BEGIN "))
                {
                    inKey = true;
                    keyType = line.Substring(11).TrimEnd('-');
                }
                else if (line.StartsWith("-----END "))
                {
                    break;
                }
                else if (inKey)
                {
                    base64.Append(line);
                }
            }

            var keyBytes = Convert.FromBase64String(base64.ToString());

            if (algorithm is RSA rsa)
            {
                if (keyType == "RSA PRIVATE KEY")
                    rsa.ImportRSAPrivateKey(keyBytes, out _);
                else if (keyType == "PRIVATE KEY")
                    rsa.ImportPkcs8PrivateKey(keyBytes, out _);
                else if (keyType == "RSA PUBLIC KEY")
                    rsa.ImportRSAPublicKey(keyBytes, out _);
                else if (keyType == "PUBLIC KEY")
                    rsa.ImportSubjectPublicKeyInfo(keyBytes, out _);
                else
                    throw new NotSupportedException($"不支持的密钥类型: {keyType}");
            }
            else if (algorithm is ECDsa ecdsa)
            {
                if (keyType == "EC PRIVATE KEY")
                    ecdsa.ImportECPrivateKey(keyBytes, out _);
                else if (keyType == "PRIVATE KEY")
                    ecdsa.ImportPkcs8PrivateKey(keyBytes, out _);
                else if (keyType == "PUBLIC KEY")
                    ecdsa.ImportSubjectPublicKeyInfo(keyBytes, out _);
                else
                    throw new NotSupportedException($"不支持的密钥类型: {keyType}");
            }
            else if (algorithm is DSA dsa)
            {
                if (keyType == "PRIVATE KEY")
                    dsa.ImportPkcs8PrivateKey(keyBytes, out _);
                else if (keyType == "PUBLIC KEY")
                    dsa.ImportSubjectPublicKeyInfo(keyBytes, out _);
                else
                    throw new NotSupportedException($"不支持的密钥类型: {keyType}");
            }
#else
            algorithm.ImportFromPem(pem);
#endif
        }

        private static string FormatPem(string label, byte[] data)
        {
            var base64 = Convert.ToBase64String(data);
            var sb = new StringBuilder();
            sb.AppendLine($"-----BEGIN {label}-----");
            for (int i = 0; i < base64.Length; i += 64)
            {
                var lineLength = Math.Min(64, base64.Length - i);
                sb.AppendLine(base64.Substring(i, lineLength));
            }
            sb.AppendLine($"-----END {label}-----");
            return sb.ToString();
        }

        #endregion

        #endregion

        #region 便捷方法

        /// <summary>
        /// 签名字符串
        /// </summary>
        /// <param name="text">文本</param>
        /// <param name="privateKey">私钥</param>
        /// <param name="algorithm">签名算法</param>
        /// <returns>Base64 编码的签名</returns>
        public static string SignString(string text, string privateKey, SignatureAlgorithm algorithm = SignatureAlgorithm.RsaSha256)
        {
            var data = Encoding.UTF8.GetBytes(text);
            byte[] signature;

            switch (algorithm)
            {
                case SignatureAlgorithm.RsaSha256:
                    signature = SignWithRsaPkcs1(data, privateKey, HashAlgorithmName.SHA256);
                    break;
                case SignatureAlgorithm.RsaSha384:
                    signature = SignWithRsaPkcs1(data, privateKey, HashAlgorithmName.SHA384);
                    break;
                case SignatureAlgorithm.RsaSha512:
                    signature = SignWithRsaPkcs1(data, privateKey, HashAlgorithmName.SHA512);
                    break;
                case SignatureAlgorithm.RsaPssSha256:
                    signature = SignWithRsaPss(data, privateKey, HashAlgorithmName.SHA256);
                    break;
                case SignatureAlgorithm.EcdsaSha256:
                    signature = SignWithEcdsa(data, privateKey, HashAlgorithmName.SHA256);
                    break;
                default:
                    throw new NotSupportedException($"不支持的签名算法: {algorithm}");
            }

            return Convert.ToBase64String(signature);
        }

        /// <summary>
        /// 验证字符串签名
        /// </summary>
        public static bool VerifyStringSignature(string text, string signatureBase64, string publicKey, SignatureAlgorithm algorithm = SignatureAlgorithm.RsaSha256)
        {
            var data = Encoding.UTF8.GetBytes(text);
            var signature = Convert.FromBase64String(signatureBase64);

            switch (algorithm)
            {
                case SignatureAlgorithm.RsaSha256:
                    return VerifyRsaPkcs1Signature(data, signature, publicKey, HashAlgorithmName.SHA256);
                case SignatureAlgorithm.RsaSha384:
                    return VerifyRsaPkcs1Signature(data, signature, publicKey, HashAlgorithmName.SHA384);
                case SignatureAlgorithm.RsaSha512:
                    return VerifyRsaPkcs1Signature(data, signature, publicKey, HashAlgorithmName.SHA512);
                case SignatureAlgorithm.RsaPssSha256:
                    return VerifyRsaPssSignature(data, signature, publicKey, HashAlgorithmName.SHA256);
                case SignatureAlgorithm.EcdsaSha256:
                    return VerifyEcdsaSignature(data, signature, publicKey, HashAlgorithmName.SHA256);
                default:
                    throw new NotSupportedException($"不支持的签名算法: {algorithm}");
            }
        }

        #endregion
    }

    /// <summary>
    /// 密钥对
    /// </summary>
    public class KeyPair
    {
        /// <summary>
        /// 私钥（PEM 格式）
        /// </summary>
        public string PrivateKey { get; set; } = string.Empty;

        /// <summary>
        /// 公钥（PEM 格式）
        /// </summary>
        public string PublicKey { get; set; } = string.Empty;

        /// <summary>
        /// 私钥（PKCS#8 格式）
        /// </summary>
        public string? PrivateKeyPkcs8 { get; set; }
    }

    /// <summary>
    /// 签名算法
    /// </summary>
    public enum SignatureAlgorithm
    {
        /// <summary>
        /// RSA + SHA256 (PKCS#1)
        /// </summary>
        RsaSha256,

        /// <summary>
        /// RSA + SHA384 (PKCS#1)
        /// </summary>
        RsaSha384,

        /// <summary>
        /// RSA + SHA512 (PKCS#1)
        /// </summary>
        RsaSha512,

        /// <summary>
        /// RSA + SHA256 (PSS)
        /// </summary>
        RsaPssSha256,

        /// <summary>
        /// RSA + SHA384 (PSS)
        /// </summary>
        RsaPssSha384,

        /// <summary>
        /// RSA + SHA512 (PSS)
        /// </summary>
        RsaPssSha512,

        /// <summary>
        /// ECDSA + SHA256
        /// </summary>
        EcdsaSha256,

        /// <summary>
        /// ECDSA + SHA384
        /// </summary>
        EcdsaSha384,

        /// <summary>
        /// ECDSA + SHA512
        /// </summary>
        EcdsaSha512
    }
}