using System;
using System.Security.Cryptography;
using System.Text;

namespace EasyTool.CodeCategory
{
    /// <summary>
    /// ECDSA 椭圆曲线签名算法工具类
    /// </summary>
    public static class EcdsaUtil
    {
        #region 密钥生成

        /// <summary>
        /// 生成 ECDSA 密钥对
        /// </summary>
        /// <param name="curve">椭圆曲线类型（可选，默认 P256）</param>
        /// <returns>包含公钥和私钥的元组</returns>
        public static (string PublicKey, string PrivateKey) GenerateKeyPair(ECCurve? curve = null)
        {
            using var ecdsa = curve.HasValue ? ECDsa.Create(curve.Value) : ECDsa.Create(ECCurve.NamedCurves.nistP256);
            var publicKey = Convert.ToBase64String(ecdsa.ExportSubjectPublicKeyInfo());
            var privateKey = Convert.ToBase64String(ecdsa.ExportPkcs8PrivateKey());
            return (publicKey, privateKey);
        }

        /// <summary>
        /// 使用指定曲线名称生成 ECDSA 密钥对
        /// </summary>
        /// <param name="curveName">曲线名称：P256、P384、P521</param>
        /// <returns>包含公钥和私钥的元组</returns>
        public static (string PublicKey, string PrivateKey) GenerateKeyPair(string curveName)
        {
            var curve = curveName.ToUpperInvariant() switch
            {
                "P256" => ECCurve.NamedCurves.nistP256,
                "P384" => ECCurve.NamedCurves.nistP384,
                "P521" => ECCurve.NamedCurves.nistP521,
                _ => ECCurve.NamedCurves.nistP256
            };

            using var ecdsa = ECDsa.Create(curve);
            var publicKey = Convert.ToBase64String(ecdsa.ExportSubjectPublicKeyInfo());
            var privateKey = Convert.ToBase64String(ecdsa.ExportPkcs8PrivateKey());
            return (publicKey, privateKey);
        }

        #endregion

        #region 签名

        /// <summary>
        /// ECDSA 签名（使用私钥）
        /// </summary>
        /// <param name="data">待签名数据</param>
        /// <param name="privateKey">私钥（Base64格式）</param>
        /// <param name="hashAlgorithm">哈希算法，默认SHA256</param>
        /// <returns>Base64 编码的签名</returns>
        public static string Sign(string data, string privateKey, string hashAlgorithm = "SHA256")
        {
            if (string.IsNullOrEmpty(data))
                return string.Empty;

            var dataBytes = Encoding.UTF8.GetBytes(data);
            var signature = Sign(dataBytes, privateKey, hashAlgorithm);
            return Convert.ToBase64String(signature);
        }

        /// <summary>
        /// ECDSA 签名（使用私钥，字节数组版本）
        /// </summary>
        /// <param name="data">待签名数据</param>
        /// <param name="privateKey">私钥（Base64格式）</param>
        /// <param name="hashAlgorithm">哈希算法，默认SHA256</param>
        /// <returns>签名字节数组</returns>
        public static byte[] Sign(byte[] data, string privateKey, string hashAlgorithm = "SHA256")
        {
            if (data == null || data.Length == 0)
                return Array.Empty<byte>();

            using var ecdsa = ECDsa.Create();
            ecdsa.ImportPkcs8PrivateKey(Convert.FromBase64String(privateKey), out _);

            var hashAlgo = GetHashAlgorithm(hashAlgorithm);
            return ecdsa.SignData(data, hashAlgo);
        }

        #endregion

        #region 验签

        /// <summary>
        /// ECDSA 验签（使用公钥）
        /// </summary>
        /// <param name="data">原始数据</param>
        /// <param name="signature">Base64 编码的签名</param>
        /// <param name="publicKey">公钥（Base64格式）</param>
        /// <param name="hashAlgorithm">哈希算法，默认SHA256</param>
        /// <returns>签名是否有效</returns>
        public static bool Verify(string data, string signature, string publicKey, string hashAlgorithm = "SHA256")
        {
            if (string.IsNullOrEmpty(data) || string.IsNullOrEmpty(signature))
                return false;

            var dataBytes = Encoding.UTF8.GetBytes(data);
            var signatureBytes = Convert.FromBase64String(signature);
            return Verify(dataBytes, signatureBytes, publicKey, hashAlgorithm);
        }

        /// <summary>
        /// ECDSA 验签（使用公钥，字节数组版本）
        /// </summary>
        /// <param name="data">原始数据</param>
        /// <param name="signature">签名字节数组</param>
        /// <param name="publicKey">公钥（Base64格式）</param>
        /// <param name="hashAlgorithm">哈希算法，默认SHA256</param>
        /// <returns>签名是否有效</returns>
        public static bool Verify(byte[] data, byte[] signature, string publicKey, string hashAlgorithm = "SHA256")
        {
            if (data == null || data.Length == 0 || signature == null || signature.Length == 0)
                return false;

            try
            {
                using var ecdsa = ECDsa.Create();
                ecdsa.ImportSubjectPublicKeyInfo(Convert.FromBase64String(publicKey), out _);

                var hashAlgo = GetHashAlgorithm(hashAlgorithm);
                return ecdsa.VerifyData(data, signature, hashAlgo);
            }
            // 捕获密钥导入和签名验证异常
            catch (FormatException)
            {
                return false;
            }
            catch (System.Security.Cryptography.CryptographicException)
            {
                return false;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        #endregion

        #region 密钥格式转换

        /// <summary>
        /// 将 PEM 格式公钥转换为 Base64 格式
        /// </summary>
        /// <param name="pemPublicKey">PEM 格式公钥</param>
        /// <returns>Base64 格式公钥</returns>
        public static string PemToBase64PublicKey(string pemPublicKey)
        {
            var pemContent = ExtractPemContent(pemPublicKey, "PUBLIC KEY");
            return pemContent;
        }

        /// <summary>
        /// 将 PEM 格式私钥转换为 Base64 格式
        /// </summary>
        /// <param name="pemPrivateKey">PEM 格式私钥</param>
        /// <returns>Base64 格式私钥</returns>
        public static string PemToBase64PrivateKey(string pemPrivateKey)
        {
            var pemContent = ExtractPemContent(pemPrivateKey, "PRIVATE KEY");
            return pemContent;
        }

        /// <summary>
        /// 将 Base64 公钥转换为 PEM 格式
        /// </summary>
        /// <param name="base64PublicKey">Base64 格式公钥</param>
        /// <returns>PEM 格式公钥</returns>
        public static string Base64ToPemPublicKey(string base64PublicKey)
        {
            return $"-----BEGIN PUBLIC KEY-----\n{InsertLineBreaks(base64PublicKey, 64)}\n-----END PUBLIC KEY-----";
        }

        /// <summary>
        /// 将 Base64 私钥转换为 PEM 格式
        /// </summary>
        /// <param name="base64PrivateKey">Base64 格式私钥</param>
        /// <returns>PEM 格式私钥</returns>
        public static string Base64ToPemPrivateKey(string base64PrivateKey)
        {
            return $"-----BEGIN PRIVATE KEY-----\n{InsertLineBreaks(base64PrivateKey, 64)}\n-----END PRIVATE KEY-----";
        }

        #endregion

        #region 私有方法

        private static HashAlgorithmName GetHashAlgorithm(string hashAlgorithm)
        {
            return hashAlgorithm.ToUpperInvariant() switch
            {
                "SHA1" => HashAlgorithmName.SHA1,
                "SHA256" => HashAlgorithmName.SHA256,
                "SHA384" => HashAlgorithmName.SHA384,
                "SHA512" => HashAlgorithmName.SHA512,
                _ => HashAlgorithmName.SHA256
            };
        }

        private static string ExtractPemContent(string pem, string label)
        {
            var startMarker = $"-----BEGIN {label}-----";
            var endMarker = $"-----END {label}-----";

            var startIndex = pem.IndexOf(startMarker);
            var endIndex = pem.IndexOf(endMarker);

            if (startIndex < 0 || endIndex < 0)
                throw new ArgumentException($"Invalid PEM format for {label}");

            startIndex += startMarker.Length;
            var content = pem.Substring(startIndex, endIndex - startIndex);
            return content.Replace("\n", "").Replace("\r", "").Trim();
        }

        private static string InsertLineBreaks(string input, int lineLength)
        {
            var result = new StringBuilder();
            for (int i = 0; i < input.Length; i += lineLength)
            {
                var length = Math.Min(lineLength, input.Length - i);
                result.AppendLine(input.Substring(i, length));
            }
            return result.ToString().TrimEnd();
        }

        #endregion
    }
}