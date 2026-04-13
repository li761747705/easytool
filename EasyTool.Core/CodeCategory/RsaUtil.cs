using System;
using System.Security.Cryptography;
using System.Text;

namespace EasyTool.CodeCategory
{
    /// <summary>
    /// RSA 非对称加密工具类
    /// </summary>
    public static class RsaUtil
    {
        #region 密钥生成

        /// <summary>
        /// 生成 RSA 密钥对
        /// </summary>
        /// <param name="keySize">密钥长度（512、1024、2048、4096），默认2048</param>
        /// <returns>包含公钥和私钥的元组</returns>
        public static (string PublicKey, string PrivateKey) GenerateKeyPair(int keySize = 2048)
        {
            using var rsa = RSA.Create(keySize);
            var publicKey = Convert.ToBase64String(rsa.ExportSubjectPublicKeyInfo());
            var privateKey = Convert.ToBase64String(rsa.ExportPkcs8PrivateKey());
            return (publicKey, privateKey);
        }

        /// <summary>
        /// 生成 XML 格式的 RSA 密钥对
        /// </summary>
        /// <param name="keySize">密钥长度，默认2048</param>
        /// <param name="includePrivate">是否包含私钥</param>
        /// <returns>XML 格式的密钥</returns>
        public static string GenerateXmlKey(int keySize = 2048, bool includePrivate = true)
        {
            using var rsa = RSA.Create(keySize);
            return rsa.ToXmlString(includePrivate);
        }

        #endregion

        #region 加密解密

        /// <summary>
        /// RSA 加密（使用公钥）
        /// </summary>
        /// <param name="data">待加密数据</param>
        /// <param name="publicKey">公钥（Base64格式）</param>
        /// <returns>Base64 编码的加密结果</returns>
        public static string Encrypt(string data, string publicKey)
        {
            if (string.IsNullOrEmpty(data))
                return string.Empty;

            var dataBytes = Encoding.UTF8.GetBytes(data);
            var encryptedBytes = Encrypt(dataBytes, publicKey);
            return Convert.ToBase64String(encryptedBytes);
        }

        /// <summary>
        /// RSA 加密（使用公钥，字节数组版本）
        /// </summary>
        /// <param name="data">待加密数据</param>
        /// <param name="publicKey">公钥（Base64格式）</param>
        /// <returns>加密后的字节数组</returns>
        public static byte[] Encrypt(byte[] data, string publicKey)
        {
            if (data == null || data.Length == 0)
                return Array.Empty<byte>();

            using var rsa = RSA.Create();
            rsa.ImportSubjectPublicKeyInfo(Convert.FromBase64String(publicKey), out _);

            // RSA 加密有长度限制，需要分块加密
            var keySize = rsa.KeySize;
            var maxBlockSize = (keySize / 8) - 42; // OAEP padding

            if (data.Length <= maxBlockSize)
            {
                return rsa.Encrypt(data, RSAEncryptionPadding.OaepSHA256);
            }

            // 分块加密
            using var outputStream = new System.IO.MemoryStream();
            var offset = 0;
            while (offset < data.Length)
            {
                var blockSize = Math.Min(maxBlockSize, data.Length - offset);
                var block = new byte[blockSize];
                Array.Copy(data, offset, block, 0, blockSize);
                var encryptedBlock = rsa.Encrypt(block, RSAEncryptionPadding.OaepSHA256);
                outputStream.Write(encryptedBlock, 0, encryptedBlock.Length);
                offset += blockSize;
            }
            return outputStream.ToArray();
        }

        /// <summary>
        /// RSA 解密（使用私钥）
        /// </summary>
        /// <param name="encryptedData">Base64 编码的加密数据</param>
        /// <param name="privateKey">私钥（Base64格式）</param>
        /// <returns>解密后的原始字符串</returns>
        public static string Decrypt(string encryptedData, string privateKey)
        {
            if (string.IsNullOrEmpty(encryptedData))
                return string.Empty;

            var dataBytes = Convert.FromBase64String(encryptedData);
            var decryptedBytes = Decrypt(dataBytes, privateKey);
            return Encoding.UTF8.GetString(decryptedBytes);
        }

        /// <summary>
        /// RSA 解密（使用私钥，字节数组版本）
        /// </summary>
        /// <param name="data">加密数据</param>
        /// <param name="privateKey">私钥（Base64格式）</param>
        /// <returns>解密后的字节数组</returns>
        public static byte[] Decrypt(byte[] data, string privateKey)
        {
            if (data == null || data.Length == 0)
                return Array.Empty<byte>();

            using var rsa = RSA.Create();
            rsa.ImportPkcs8PrivateKey(Convert.FromBase64String(privateKey), out _);

            var keySize = rsa.KeySize;
            var blockSize = keySize / 8;

            if (data.Length <= blockSize)
            {
                return rsa.Decrypt(data, RSAEncryptionPadding.OaepSHA256);
            }

            // 分块解密
            using var outputStream = new System.IO.MemoryStream();
            var offset = 0;
            while (offset < data.Length)
            {
                var currentBlockSize = Math.Min(blockSize, data.Length - offset);
                var block = new byte[currentBlockSize];
                Array.Copy(data, offset, block, 0, currentBlockSize);
                var decryptedBlock = rsa.Decrypt(block, RSAEncryptionPadding.OaepSHA256);
                outputStream.Write(decryptedBlock, 0, decryptedBlock.Length);
                offset += currentBlockSize;
            }
            return outputStream.ToArray();
        }

        #endregion

        #region 签名验签

        /// <summary>
        /// RSA 签名（使用私钥）
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
        /// RSA 签名（使用私钥，字节数组版本）
        /// </summary>
        /// <param name="data">待签名数据</param>
        /// <param name="privateKey">私钥（Base64格式）</param>
        /// <param name="hashAlgorithm">哈希算法，默认SHA256</param>
        /// <returns>签名字节数组</returns>
        public static byte[] Sign(byte[] data, string privateKey, string hashAlgorithm = "SHA256")
        {
            if (data == null || data.Length == 0)
                return Array.Empty<byte>();

            using var rsa = RSA.Create();
            rsa.ImportPkcs8PrivateKey(Convert.FromBase64String(privateKey), out _);

            var hashAlgo = GetHashAlgorithm(hashAlgorithm);
            var padding = GetSignaturePadding();
            return rsa.SignData(data, hashAlgo, padding);
        }

        /// <summary>
        /// RSA 验签（使用公钥）
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
        /// RSA 验签（使用公钥，字节数组版本）
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
                using var rsa = RSA.Create();
                rsa.ImportSubjectPublicKeyInfo(Convert.FromBase64String(publicKey), out _);

                var hashAlgo = GetHashAlgorithm(hashAlgorithm);
                var padding = GetSignaturePadding();
                return rsa.VerifyData(data, signature, hashAlgo, padding);
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

        #region 私有方法

        private static HashAlgorithmName GetHashAlgorithm(string hashAlgorithm)
        {
            return hashAlgorithm.ToUpperInvariant() switch
            {
                "MD5" => HashAlgorithmName.MD5,
                "SHA1" => HashAlgorithmName.SHA1,
                "SHA256" => HashAlgorithmName.SHA256,
                "SHA384" => HashAlgorithmName.SHA384,
                "SHA512" => HashAlgorithmName.SHA512,
                _ => HashAlgorithmName.SHA256
            };
        }

        private static RSASignaturePadding GetSignaturePadding()
        {
            return RSASignaturePadding.Pkcs1;
        }

        #endregion
    }
}