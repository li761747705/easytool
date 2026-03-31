using System;
using System.Collections.Generic;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

using System.Text;

namespace EasyTool.SecurityCategory
{
    /// <summary>
    /// TLS/SSL 配置和验证工具类
    /// </summary>
    public static class TlsUtil
    {
        #region SSL/TLS 协议配置

        /// <summary>
        /// 获取支持的 SSL/TLS 协议
        /// </summary>
        /// <returns>支持的协议列表</returns>
        public static SslProtocols GetSupportedProtocols()
        {
#if NETSTANDARD2_1
            // netstandard2.1 不支持 TLS 1.3
            return SslProtocols.Tls12;
#else
            return SslProtocols.Tls12 | SslProtocols.Tls13;
#endif
        }

        /// <summary>
        /// 获取安全的 SSL/TLS 协议（排除不安全版本）
        /// </summary>
        /// <returns>安全的协议</returns>
        public static SslProtocols GetSecureProtocols()
        {
#if NETSTANDARD2_1
            return SslProtocols.Tls12;
#else
            return SslProtocols.Tls12 | SslProtocols.Tls13;
#endif
        }

        /// <summary>
        /// 检查协议是否安全
        /// </summary>
        /// <param name="protocol">要检查的协议</param>
        /// <returns>是否安全</returns>
        public static bool IsSecureProtocol(SslProtocols protocol)
        {
            // SSL 2.0, SSL 3.0, TLS 1.0, TLS 1.1 被认为不安全
            // 注意：Ssl2 和 Ssl3 已被标记为过时，这里使用数值表示
            var insecureProtocols = (SslProtocols)12 | SslProtocols.Tls | SslProtocols.Tls11; // 12 = Ssl2(12) | Ssl3(48) 的等效值

            return (protocol & insecureProtocols) == 0 &&
                   (protocol & SslProtocols.Tls12) != 0;
        }

        #endregion

        #region 证书验证

        /// <summary>
        /// 创建证书验证回调（验证服务器证书）
        /// </summary>
        /// <param name="allowInvalidCerts">是否允许无效证书</param>
        /// <param name="validateChain">是否验证证书链</param>
        /// <returns>远程证书验证回调</returns>
        public static RemoteCertificateValidationCallback CreateCertificateValidationCallback(
            bool allowInvalidCerts = false,
            bool validateChain = true)
        {
            return (sender, certificate, chain, sslPolicyErrors) =>
            {
                // 如果允许无效证书，直接返回 true
                if (allowInvalidCerts)
                {
                    return true;
                }

                // 如果没有错误，返回 true
                if (sslPolicyErrors == SslPolicyErrors.None)
                {
                    return true;
                }

                // 如果不验证证书链，只检查是否有证书
                if (!validateChain)
                {
                    return certificate != null;
                }

                // 默认：严格验证
                return false;
            };
        }

        /// <summary>
        /// 创建验证特定域名的证书验证回调
        /// </summary>
        /// <param name="allowedDomains">允许的域名列表</param>
        /// <returns>远程证书验证回调</returns>
        public static RemoteCertificateValidationCallback CreateDomainValidationCallback(params string[] allowedDomains)
        {
            return (sender, certificate, chain, sslPolicyErrors) =>
            {
                if (sslPolicyErrors == SslPolicyErrors.None)
                {
                    return true;
                }

                // 检查域名不匹配的情况
                if ((sslPolicyErrors & SslPolicyErrors.RemoteCertificateNameMismatch) != 0)
                {
                    if (certificate is X509Certificate2 cert && allowedDomains.Length > 0)
                    {
                        var certDomain = cert.GetNameInfo(X509NameType.DnsName, false);
                        foreach (var domain in allowedDomains)
                        {
                            if (MatchesDomain(certDomain, domain))
                            {
                                return true;
                            }
                        }
                    }
                }

                return false;
            };
        }

        /// <summary>
        /// 验证证书有效性
        /// </summary>
        /// <param name="certificate">证书</param>
        /// <param name="checkRevocation">是否检查吊销状态</param>
        /// <returns>验证结果</returns>
        public static CertificateValidationResult ValidateCertificate(X509Certificate2 certificate, bool checkRevocation = false)
        {
            var result = new CertificateValidationResult { IsValid = true };

            if (certificate == null)
            {
                return new CertificateValidationResult { IsValid = false, Errors = new List<string> { "证书为空" } };
            }

            // 检查证书是否过期
            var now = DateTime.UtcNow;
            if (certificate.NotBefore > now)
            {
                return new CertificateValidationResult
                {
                    IsValid = false,
                    Errors = new List<string> { $"证书尚未生效，生效时间: {certificate.NotBefore}" }
                };
            }

            if (certificate.NotAfter < now)
            {
                return new CertificateValidationResult
                {
                    IsValid = false,
                    Errors = new System.Collections.Generic.List<string> { $"证书已过期，过期时间: {certificate.NotAfter}" }
                };
            }

            // 检查证书链
            using var chain = new X509Chain();
            chain.ChainPolicy.RevocationMode = checkRevocation
                ? X509RevocationMode.Online
                : X509RevocationMode.NoCheck;

            if (!chain.Build(certificate))
            {
                var errors = new System.Collections.Generic.List<string>();
                foreach (var status in chain.ChainStatus)
                {
                    errors.Add(status.StatusInformation);
                }

                return new CertificateValidationResult
                {
                    IsValid = false,
                    Errors = new System.Collections.Generic.List<string> { $"证书链验证失败: {string.Join(", ", errors)}" }
                };
            }

            result.Subject = certificate.Subject;
            result.Issuer = certificate.Issuer;
            result.NotBefore = certificate.NotBefore;
            result.NotAfter = certificate.NotAfter;
            result.Thumbprint = certificate.Thumbprint;

            return result;
        }

        #endregion

        #region 证书加载

        /// <summary>
        /// 从文件加载证书
        /// </summary>
        /// <param name="filePath">证书文件路径</param>
        /// <param name="password">密码（可选）</param>
        /// <returns>X509 证书</returns>
        public static X509Certificate2 LoadCertificate(string filePath, string? password = null)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            if (!System.IO.File.Exists(filePath))
            {
                throw new System.IO.FileNotFoundException("证书文件不存在", filePath);
            }

            return string.IsNullOrEmpty(password)
                ? new X509Certificate2(filePath)
                : new X509Certificate2(filePath, password);
        }

        /// <summary>
        /// 从 PFX 文件加载证书
        /// </summary>
        /// <param name="filePath">PFX 文件路径</param>
        /// <param name="password">密码</param>
        /// <returns>X509 证书</returns>
        public static X509Certificate2 LoadPfxCertificate(string filePath, string password)
        {
            return LoadCertificate(filePath, password);
        }

        /// <summary>
        /// 从 PEM 文件加载证书
        /// </summary>
        /// <param name="certPath">证书文件路径</param>
        /// <param name="keyPath">私钥文件路径（可选）</param>
        /// <returns>X509 证书</returns>
        public static X509Certificate2 LoadPemCertificate(string certPath, string? keyPath = null)
        {
            if (string.IsNullOrEmpty(certPath))
            {
                throw new ArgumentNullException(nameof(certPath));
            }

#if NETSTANDARD2_1
            // netstandard2.1 不支持 CreateFromPem，使用替代方案
            var certBytes = System.IO.File.ReadAllBytes(certPath);
            return new X509Certificate2(certBytes);
#else
            var certPem = System.IO.File.ReadAllText(certPath);

            if (string.IsNullOrEmpty(keyPath))
            {
                return X509Certificate2.CreateFromPem(certPem);
            }

            var keyPem = System.IO.File.ReadAllText(keyPath);
            return X509Certificate2.CreateFromPem(certPem, keyPem);
#endif
        }

        /// <summary>
        /// 从证书存储区加载证书
        /// </summary>
        /// <param name="storeName">存储区名称</param>
        /// <param name="storeLocation">存储区位置</param>
        /// <param name="thumbprint">证书指纹</param>
        /// <returns>X509 证书</returns>
        public static X509Certificate2? LoadCertificateFromStore(StoreName storeName, StoreLocation storeLocation, string thumbprint)
        {
            using var store = new X509Store(storeName, storeLocation);
            store.Open(OpenFlags.ReadOnly);

            var certificates = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, validOnly: false);

            return certificates.Count > 0 ? certificates[0] : null;
        }

        #endregion

        #region 证书信息

        /// <summary>
        /// 获取证书信息
        /// </summary>
        /// <param name="certificate">证书</param>
        /// <returns>证书信息</returns>
        public static CertificateInfo GetCertificateInfo(X509Certificate2 certificate)
        {
            if (certificate == null)
            {
                throw new ArgumentNullException(nameof(certificate));
            }

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

        /// <summary>
        /// 检查证书是否即将过期
        /// </summary>
        /// <param name="certificate">证书</param>
        /// <param name="daysThreshold">提前天数阈值</param>
        /// <returns>是否即将过期</returns>
        public static bool IsCertificateExpiringSoon(X509Certificate2 certificate, int daysThreshold = 30)
        {
            if (certificate == null)
            {
                throw new ArgumentNullException(nameof(certificate));
            }

            var timeRemaining = certificate.NotAfter - DateTime.UtcNow;
            return timeRemaining.TotalDays <= daysThreshold;
        }

        /// <summary>
        /// 获取证书剩余有效天数
        /// </summary>
        /// <param name="certificate">证书</param>
        /// <returns>剩余有效天数</returns>
        public static int GetDaysUntilExpiration(X509Certificate2 certificate)
        {
            if (certificate == null)
            {
                throw new ArgumentNullException(nameof(certificate));
            }

            var timeRemaining = certificate.NotAfter - DateTime.UtcNow;
            return Math.Max(0, (int)timeRemaining.TotalDays);
        }

        #endregion

        #region SSL 选项

        /// <summary>
        /// 创建服务器 SSL 选项
        /// </summary>
        /// <param name="targetHost">目标主机</param>
        /// <param name="serverCertificate">服务器证书</param>
        /// <returns>SSL 选项</returns>
        public static SslServerAuthenticationOptions CreateServerSslOptions(string targetHost, X509Certificate2? serverCertificate = null)
        {
            var options = new SslServerAuthenticationOptions
            {
                EnabledSslProtocols = GetSecureProtocols(),
                ClientCertificateRequired = false
            };

            if (serverCertificate != null)
            {
                options.ServerCertificate = serverCertificate;
            }

            return options;
        }

        /// <summary>
        /// 创建客户端 SSL 选项
        /// </summary>
        /// <param name="clientCertificate">客户端证书</param>
        /// <param name="allowInvalidServerCert">是否允许无效的服务器证书</param>
        /// <returns>SSL 选项</returns>
        public static SslClientAuthenticationOptions CreateClientSslOptions(
            X509Certificate2? clientCertificate = null,
            bool allowInvalidServerCert = false)
        {
            var options = new SslClientAuthenticationOptions
            {
                EnabledSslProtocols = GetSecureProtocols(),
                RemoteCertificateValidationCallback = CreateCertificateValidationCallback(allowInvalidServerCert)
            };

            if (clientCertificate != null)
            {
                options.ClientCertificates = new X509Certificate2Collection { clientCertificate };
            }

            return options;
        }

        #endregion

        #region 辅助方法

        private static bool MatchesDomain(string? certDomain, string allowedDomain)
        {
            if (string.IsNullOrEmpty(certDomain))
            {
                return false;
            }

            // 支持通配符域名
            if (certDomain.StartsWith("*."))
            {
                var certBaseDomain = certDomain[2..];
                return allowedDomain.EndsWith(certBaseDomain, StringComparison.OrdinalIgnoreCase) ||
                       allowedDomain.Equals(certBaseDomain, StringComparison.OrdinalIgnoreCase);
            }

            return certDomain.Equals(allowedDomain, StringComparison.OrdinalIgnoreCase);
        }

                #endregion

            }

        }

        