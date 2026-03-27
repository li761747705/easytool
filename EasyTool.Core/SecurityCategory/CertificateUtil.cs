using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace EasyTool.SecurityCategory
{
    /// <summary>
    /// 证书工具类
    /// 提供证书生成、加载和验证功能
    /// </summary>
    public static class CertificateUtil
    {
        /// <summary>
        /// 生成自签名证书
        /// </summary>
        /// <param name="subjectName">主题名称</param>
        /// <param name="validYears">有效期（年）</param>
        /// <param name="keySize">密钥大小</param>
        /// <returns>X509 证书</returns>
        public static X509Certificate2 GenerateSelfSignedCertificate(
            string subjectName,
            int validYears = 1,
            int keySize = 2048)
        {
            using var rsa = RSA.Create(keySize);
            var request = new CertificateRequest(
                $"CN={subjectName}",
                rsa,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);

            // 添加基本约束
            request.CertificateExtensions.Add(
                new X509BasicConstraintsExtension(true, true, 0, false));

            // 添加密钥用法
            request.CertificateExtensions.Add(
                new X509KeyUsageExtension(
                    X509KeyUsageFlags.DigitalSignature |
                    X509KeyUsageFlags.KeyEncipherment |
                    X509KeyUsageFlags.CrlSign |
                    X509KeyUsageFlags.KeyCertSign,
                    false));

            // 添加增强密钥用法
            request.CertificateExtensions.Add(
                new X509EnhancedKeyUsageExtension(
                    new OidCollection
                    {
                        new Oid("1.3.6.1.5.5.7.3.1"), // Server Authentication
                        new Oid("1.3.6.1.5.5.7.3.2")  // Client Authentication
                    },
                    false));

            // 添加主题备用名称
            var sanBuilder = new SubjectAlternativeNameBuilder();
            sanBuilder.AddDnsName(subjectName);
            sanBuilder.AddDnsName("localhost");
            request.CertificateExtensions.Add(sanBuilder.Build());

            // 创建证书
            var notBefore = DateTimeOffset.UtcNow.AddDays(-1);
            var notAfter = DateTimeOffset.UtcNow.AddYears(validYears);

            var certificate = request.CreateSelfSigned(notBefore, notAfter);

            return new X509Certificate2(
                certificate.Export(X509ContentType.Pfx),
                (string?)null,
                X509KeyStorageFlags.MachineKeySet |
                X509KeyStorageFlags.PersistKeySet |
                X509KeyStorageFlags.Exportable);
        }

        /// <summary>
        /// 生成客户端证书
        /// </summary>
        /// <param name="subjectName">主题名称</param>
        /// <param name="issuerCertificate">颁发者证书</param>
        /// <param name="validDays">有效期（天）</param>
        /// <returns>客户端证书</returns>
        public static X509Certificate2 GenerateClientCertificate(
            string subjectName,
            X509Certificate2 issuerCertificate,
            int validDays = 365)
        {
            using var rsa = RSA.Create(2048);
            var request = new CertificateRequest(
                $"CN={subjectName}",
                rsa,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);

            // 添加基本约束
            request.CertificateExtensions.Add(
                new X509BasicConstraintsExtension(false, false, 0, false));

            // 添加密钥用法
            request.CertificateExtensions.Add(
                new X509KeyUsageExtension(
                    X509KeyUsageFlags.DigitalSignature |
                    X509KeyUsageFlags.KeyEncipherment,
                    false));

            // 添加增强密钥用法
            request.CertificateExtensions.Add(
                new X509EnhancedKeyUsageExtension(
                    new OidCollection
                    {
                        new Oid("1.3.6.1.5.5.7.3.2") // Client Authentication
                    },
                    false));

            // 使用颁发者证书签名
            var notBefore = DateTimeOffset.UtcNow.AddDays(-1);
            var notAfter = DateTimeOffset.UtcNow.AddDays(validDays);

            var serialNumber = new byte[16];
            RandomNumberGenerator.Fill(serialNumber);

            var certificate = request.Create(
                issuerCertificate,
                notBefore,
                notAfter,
                serialNumber);

            return certificate.CopyWithPrivateKey(rsa);
        }

        /// <summary>
        /// 从文件加载证书
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="password">密码</param>
        /// <returns>证书</returns>
        public static X509Certificate2 LoadFromFile(string filePath, string? password = null)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"证书文件不存在: {filePath}");

            var bytes = File.ReadAllBytes(filePath);
            return new X509Certificate2(bytes, password);
        }

        /// <summary>
        /// 从 PFX 文件加载证书
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="password">密码</param>
        /// <returns>证书</returns>
        public static X509Certificate2 LoadPfx(string filePath, string? password = null)
        {
            return LoadFromFile(filePath, password);
        }

        /// <summary>
        /// 从 PEM 文件加载证书
        /// </summary>
        /// <param name="certPath">证书文件路径</param>
        /// <param name="keyPath">私钥文件路径（可选）</param>
        /// <returns>证书</returns>
        public static X509Certificate2 LoadPem(string certPath, string? keyPath = null)
        {
            var certPem = File.ReadAllText(certPath);
#if NET5_0_OR_GREATER
            var cert = X509Certificate2.CreateFromPem(certPem);

            if (!string.IsNullOrEmpty(keyPath) && File.Exists(keyPath))
            {
                var keyPem = File.ReadAllText(keyPath);
                using var rsa = RSA.Create();
                rsa.ImportFromPem(keyPem);
                return cert.CopyWithPrivateKey(rsa);
            }

            return cert;
#else
            // netstandard2.1 不支持 PEM 格式，使用 Pfx 格式
            throw new PlatformNotSupportedException("PEM 格式证书需要 .NET 5.0 或更高版本");
#endif
        }

        /// <summary>
        /// 保存证书到文件
        /// </summary>
        /// <param name="certificate">证书</param>
        /// <param name="filePath">文件路径</param>
        /// <param name="password">密码</param>
        /// <param name="format">格式</param>
        public static void SaveToFile(
            X509Certificate2 certificate,
            string filePath,
            string? password = null,
            CertificateFormat format = CertificateFormat.Pfx)
        {
            byte[] data;

            switch (format)
            {
                case CertificateFormat.Pfx:
                    data = certificate.Export(X509ContentType.Pfx, password);
                    break;
                case CertificateFormat.Pem:
#if NET5_0_OR_GREATER
                    var pem = certificate.ExportCertificatePem();
                    data = Encoding.UTF8.GetBytes(pem);
#else
                    // netstandard2.1 不支持 PEM 导出
                    throw new PlatformNotSupportedException("PEM 格式导出需要 .NET 5.0 或更高版本");
#endif
                    break;
                case CertificateFormat.Cer:
                    data = certificate.Export(X509ContentType.Cert);
                    break;
                default:
                    throw new ArgumentException($"不支持的证书格式: {format}");
            }

            File.WriteAllBytes(filePath, data);
        }

        /// <summary>
        /// 导出证书为 PEM 格式
        /// </summary>
        /// <param name="certificate">证书</param>
        /// <returns>PEM 字符串</returns>
        public static string ExportToPem(X509Certificate2 certificate)
        {
#if NET5_0_OR_GREATER
            return certificate.ExportCertificatePem();
#else
            throw new PlatformNotSupportedException("PEM 格式导出需要 .NET 5.0 或更高版本");
#endif
        }

        /// <summary>
        /// 导出私钥为 PEM 格式
        /// </summary>
        /// <param name="certificate">证书</param>
        /// <returns>PEM 字符串</returns>
        public static string ExportPrivateKeyToPem(X509Certificate2 certificate)
        {
            var rsa = certificate.GetRSAPrivateKey();
            if (rsa == null)
                throw new InvalidOperationException("证书不包含私钥");

#if NET5_0_OR_GREATER
            return rsa.ExportRSAPrivateKeyPem();
#else
            throw new PlatformNotSupportedException("PEM 格式导出需要 .NET 5.0 或更高版本");
#endif
        }

        /// <summary>
        /// 验证证书
        /// </summary>
        /// <param name="certificate">证书</param>
        /// <param name="chain">证书链（可选）</param>
        /// <returns>验证结果</returns>
        public static CertificateValidationResult Validate(
            X509Certificate2 certificate,
            X509Certificate2Collection? chain = null)
        {
            var result = new CertificateValidationResult
            {
                Subject = certificate.Subject,
                Issuer = certificate.Issuer,
                NotBefore = certificate.NotBefore,
                NotAfter = certificate.NotAfter,
                Thumbprint = certificate.Thumbprint
            };

            // 检查有效期
            var now = DateTime.UtcNow;

            if (now < certificate.NotBefore)
            {
                result.IsValid = false;
                result.Errors.Add($"证书尚未生效（生效时间: {certificate.NotBefore}）");
            }

            if (now > certificate.NotAfter)
            {
                result.IsValid = false;
                result.Errors.Add($"证书已过期（过期时间: {certificate.NotAfter}）");
            }

            // 验证证书链
            using var chainBuilder = new X509Chain();
            chainBuilder.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
            chainBuilder.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;

            if (chain != null)
            {
                foreach (var cert in chain)
                {
                    chainBuilder.ChainPolicy.ExtraStore.Add(cert);
                }
            }

            var chainValid = chainBuilder.Build(certificate);

            if (!chainValid)
            {
                foreach (var status in chainBuilder.ChainStatus)
                {
                    result.Warnings.Add($"证书链状态: {status.StatusInformation}");
                }
            }

            result.HasPrivateKey = certificate.HasPrivateKey;
            result.IsValid ??= true;

            return result;
        }

        /// <summary>
        /// 验证证书链
        /// </summary>
        /// <param name="certificate">证书</param>
        /// <param name="issuerCertificate">颁发者证书</param>
        /// <returns>是否有效</returns>
        public static bool ValidateChain(X509Certificate2 certificate, X509Certificate2 issuerCertificate)
        {
            using var chain = new X509Chain();
            chain.ChainPolicy.ExtraStore.Add(issuerCertificate);
            chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
            chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;

            return chain.Build(certificate);
        }

        /// <summary>
        /// 从证书存储区获取证书
        /// </summary>
        /// <param name="storeName">存储区名称</param>
        /// <param name="storeLocation">存储区位置</param>
        /// <param name="thumbprint">证书指纹</param>
        /// <returns>证书</returns>
        public static X509Certificate2? GetFromStore(
            StoreName storeName,
            StoreLocation storeLocation,
            string thumbprint)
        {
            using var store = new X509Store(storeName, storeLocation);
            store.Open(OpenFlags.ReadOnly);

            var certificates = store.Certificates.Find(
                X509FindType.FindByThumbprint,
                thumbprint,
                false);

            return certificates.Count > 0 ? certificates[0] : null;
        }

        /// <summary>
        /// 将证书添加到存储区
        /// </summary>
        /// <param name="certificate">证书</param>
        /// <param name="storeName">存储区名称</param>
        /// <param name="storeLocation">存储区位置</param>
        public static void AddToStore(
            X509Certificate2 certificate,
            StoreName storeName,
            StoreLocation storeLocation)
        {
            using var store = new X509Store(storeName, storeLocation);
            store.Open(OpenFlags.ReadWrite);
            store.Add(certificate);
        }

        /// <summary>
        /// 从存储区移除证书
        /// </summary>
        /// <param name="certificate">证书</param>
        /// <param name="storeName">存储区名称</param>
        /// <param name="storeLocation">存储区位置</param>
        public static void RemoveFromStore(
            X509Certificate2 certificate,
            StoreName storeName,
            StoreLocation storeLocation)
        {
            using var store = new X509Store(storeName, storeLocation);
            store.Open(OpenFlags.ReadWrite);
            store.Remove(certificate);
        }

        /// <summary>
        /// 获取证书信息
        /// </summary>
        /// <param name="certificate">证书</param>
        /// <returns>证书信息</returns>
        public static CertificateInfo GetCertificateInfo(X509Certificate2 certificate)
        {
            return new CertificateInfo
            {
                Subject = certificate.Subject,
                Issuer = certificate.Issuer,
                NotBefore = certificate.NotBefore,
                NotAfter = certificate.NotAfter,
                Thumbprint = certificate.Thumbprint,
                SerialNumber = certificate.SerialNumber,
                HasPrivateKey = certificate.HasPrivateKey,
                KeySize = certificate.GetRSAPublicKey()?.KeySize ?? 0,
                SignatureAlgorithm = certificate.SignatureAlgorithm.FriendlyName ?? "Unknown"
            };
        }
    }

    /// <summary>
    /// 证书格式
    /// </summary>
    public enum CertificateFormat
    {
        /// <summary>
        /// PFX/P12 格式
        /// </summary>
        Pfx,

        /// <summary>
        /// PEM 格式
        /// </summary>
        Pem,

        /// <summary>
        /// CER/DER 格式
        /// </summary>
        Cer
    }

    /// <summary>
    /// 证书验证结果
    /// </summary>
    public class CertificateValidationResult
    {
        /// <summary>
        /// 是否有效
        /// </summary>
        public bool? IsValid { get; set; }

        /// <summary>
        /// 主题
        /// </summary>
        public string Subject { get; set; } = string.Empty;

        /// <summary>
        /// 颁发者
        /// </summary>
        public string Issuer { get; set; } = string.Empty;

        /// <summary>
        /// 生效时间
        /// </summary>
        public DateTime NotBefore { get; set; }

        /// <summary>
        /// 过期时间
        /// </summary>
        public DateTime NotAfter { get; set; }

        /// <summary>
        /// 指纹
        /// </summary>
        public string Thumbprint { get; set; } = string.Empty;

        /// <summary>
        /// 是否有私钥
        /// </summary>
        public bool HasPrivateKey { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public List<string> Errors { get; set; } = new();

        /// <summary>
        /// 警告信息
        /// </summary>
        public List<string> Warnings { get; set; } = new();
    }

    /// <summary>
    /// 证书信息
    /// </summary>
    public class CertificateInfo
    {
        /// <summary>
        /// 主题
        /// </summary>
        public string Subject { get; set; } = string.Empty;

        /// <summary>
        /// 颁发者
        /// </summary>
        public string Issuer { get; set; } = string.Empty;

        /// <summary>
        /// 生效时间
        /// </summary>
        public DateTime NotBefore { get; set; }

        /// <summary>
        /// 过期时间
        /// </summary>
        public DateTime NotAfter { get; set; }

        /// <summary>
        /// 指纹
        /// </summary>
        public string Thumbprint { get; set; } = string.Empty;

        /// <summary>
        /// 序列号
        /// </summary>
        public string SerialNumber { get; set; } = string.Empty;

        /// <summary>
        /// 是否有私钥
        /// </summary>
        public bool HasPrivateKey { get; set; }

        /// <summary>
        /// 密钥大小
        /// </summary>
        public int KeySize { get; set; }

        /// <summary>
        /// 签名算法
        /// </summary>
        public string SignatureAlgorithm { get; set; } = string.Empty;
    }
}
