using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace EasyTool.BusinessCategory
{
    /// <summary>
    /// 邮箱服务提供商枚举
    /// </summary>
    public enum EmailProvider
    {
        /// <summary>
        /// 未知服务商
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// QQ邮箱
        /// </summary>
        QQ = 1,

        /// <summary>
        /// 网易163邮箱
        /// </summary>
        NetEase163 = 2,

        /// <summary>
        /// 网易126邮箱
        /// </summary>
        NetEase126 = 3,

        /// <summary>
        /// 网易yeah邮箱
        /// </summary>
        NetEaseYeah = 4,

        /// <summary>
        /// 新浪邮箱
        /// </summary>
        Sina = 5,

        /// <summary>
        /// 搜狐邮箱
        /// </summary>
        Sohu = 6,

        /// <summary>
        /// Gmail
        /// </summary>
        Gmail = 7,

        /// <summary>
        /// Outlook/Hotmail
        /// </summary>
        Outlook = 8,

        /// <summary>
        /// Yahoo
        /// </summary>
        Yahoo = 9,

        /// <summary>
        /// iCloud
        /// </summary>
        ICloud = 10,

        /// <summary>
        /// 阿里云邮箱
        /// </summary>
        Aliyun = 11,

        /// <summary>
        /// 企业邮箱
        /// </summary>
        Enterprise = 12
    }

    /// <summary>
    /// 邮箱工具类
    /// </summary>
    public static class EmailUtil
    {
        #region 常量与私有字段

        /// <summary>
        /// 邮箱正则表达式（标准格式）
        /// </summary>
        private static readonly Regex EmailRegex = new Regex(
            @"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$",
            RegexOptions.Compiled);

        /// <summary>
        /// 简单邮箱正则表达式（用于快速验证）
        /// </summary>
        private static readonly Regex SimpleEmailRegex = new Regex(
            @"^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$",
            RegexOptions.Compiled);

        /// <summary>
        /// 邮箱服务商域名映射
        /// </summary>
        private static readonly Dictionary<string, EmailProvider> ProviderDomainMap = new Dictionary<string, EmailProvider>(StringComparer.OrdinalIgnoreCase)
        {
            // QQ邮箱
            { "qq.com", EmailProvider.QQ },
            { "foxmail.com", EmailProvider.QQ },
            { "vip.qq.com", EmailProvider.QQ },

            // 网易邮箱
            { "163.com", EmailProvider.NetEase163 },
            { "vip.163.com", EmailProvider.NetEase163 },
            { "126.com", EmailProvider.NetEase126 },
            { "vip.126.com", EmailProvider.NetEase126 },
            { "yeah.net", EmailProvider.NetEaseYeah },

            // 新浪邮箱
            { "sina.com", EmailProvider.Sina },
            { "sina.cn", EmailProvider.Sina },
            { "vip.sina.com", EmailProvider.Sina },

            // 搜狐邮箱
            { "sohu.com", EmailProvider.Sohu },
            { "vip.sohu.com", EmailProvider.Sohu },

            // Gmail
            { "gmail.com", EmailProvider.Gmail },
            { "googlemail.com", EmailProvider.Gmail },

            // Outlook/Hotmail
            { "outlook.com", EmailProvider.Outlook },
            { "hotmail.com", EmailProvider.Outlook },
            { "live.com", EmailProvider.Outlook },
            { "msn.com", EmailProvider.Outlook },

            // Yahoo
            { "yahoo.com", EmailProvider.Yahoo },
            { "yahoo.cn", EmailProvider.Yahoo },
            { "yahoo.com.cn", EmailProvider.Yahoo },
            { "yahoo.co.jp", EmailProvider.Yahoo },
            { "ymail.com", EmailProvider.Yahoo },

            // iCloud
            { "icloud.com", EmailProvider.ICloud },
            { "me.com", EmailProvider.ICloud },
            { "mac.com", EmailProvider.ICloud },

            // 阿里云邮箱
            { "aliyun.com", EmailProvider.Aliyun },
            { "aliyuncs.com", EmailProvider.Aliyun }
        };

        /// <summary>
        /// 常见企业邮箱域名
        /// </summary>
        private static readonly HashSet<string> EnterpriseDomains = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "exmail.qq.com", // 腾讯企业邮
            "qiye.163.com",  // 网易企业邮
            "qiye.aliyun.com", // 阿里企业邮
            "corp.sina.com", // 新浪企业邮
        };

        #endregion

        #region 验证方法

        /// <summary>
        /// 验证邮箱格式是否有效（标准验证）
        /// </summary>
        /// <param name="email">邮箱地址</param>
        /// <returns>是否有效</returns>
        public static bool IsValid(string? email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            // 长度检查（RFC 5321规定最大254字符）
            if (email.Length > 254)
            {
                return false;
            }

            return EmailRegex.IsMatch(email);
        }

        /// <summary>
        /// 快速验证邮箱格式（简单验证，性能更好）
        /// </summary>
        /// <param name="email">邮箱地址</param>
        /// <returns>是否有效</returns>
        public static bool IsValidQuick(string? email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            if (email.Length > 254)
            {
                return false;
            }

            return SimpleEmailRegex.IsMatch(email);
        }

        /// <summary>
        /// 验证邮箱格式并规范化
        /// </summary>
        /// <param name="email">邮箱地址</param>
        /// <returns>规范化后的邮箱地址，无效返回null</returns>
        public static string? Normalize(string? email)
        {
            if (!IsValid(email))
            {
                return null;
            }

            // 转小写，去除首尾空格
            return email!.Trim().ToLower();
        }

        #endregion

        #region 解析方法

        /// <summary>
        /// 获取邮箱用户名（@之前的部分）
        /// </summary>
        /// <param name="email">邮箱地址</param>
        /// <returns>用户名，无效邮箱返回null</returns>
        public static string? GetUsername(string? email)
        {
            if (!IsValidQuick(email))
            {
                return null;
            }

            int atIndex = email!.IndexOf('@');
            return email.Substring(0, atIndex);
        }

        /// <summary>
        /// 获取邮箱域名（@之后的部分）
        /// </summary>
        /// <param name="email">邮箱地址</param>
        /// <returns>域名，无效邮箱返回null</returns>
        public static string? GetDomain(string? email)
        {
            if (!IsValidQuick(email))
            {
                return null;
            }

            int atIndex = email!.IndexOf('@');
            return email.Substring(atIndex + 1).ToLower();
        }

        /// <summary>
        /// 获取邮箱顶级域名
        /// </summary>
        /// <param name="email">邮箱地址</param>
        /// <returns>顶级域名（如.com、.cn），无效邮箱返回null</returns>
        public static string? GetTopLevelDomain(string? email)
        {
            string? domain = GetDomain(email);
            if (domain == null)
            {
                return null;
            }

            int lastDotIndex = domain.LastIndexOf('.');
            if (lastDotIndex < 0)
            {
                return null;
            }

            return domain.Substring(lastDotIndex);
        }

        #endregion

        #region 服务商识别

        /// <summary>
        /// 获取邮箱服务商
        /// </summary>
        /// <param name="email">邮箱地址</param>
        /// <returns>邮箱服务商枚举</returns>
        public static EmailProvider GetProvider(string? email)
        {
            string? domain = GetDomain(email);
            if (domain == null)
            {
                return EmailProvider.Unknown;
            }

            // 检查企业邮箱
            if (EnterpriseDomains.Contains(domain))
            {
                return EmailProvider.Enterprise;
            }

            // 检查已知服务商
            if (ProviderDomainMap.TryGetValue(domain, out EmailProvider provider))
            {
                return provider;
            }

            // 检查子域名（如 vip.qq.com）
            foreach (var kvp in ProviderDomainMap)
            {
                if (domain.EndsWith("." + kvp.Key, StringComparison.OrdinalIgnoreCase))
                {
                    return kvp.Value;
                }
            }

            return EmailProvider.Unknown;
        }

        /// <summary>
        /// 获取邮箱服务商名称
        /// </summary>
        /// <param name="email">邮箱地址</param>
        /// <returns>服务商名称</returns>
        public static string? GetProviderName(string? email)
        {
            EmailProvider provider = GetProvider(email);
            return provider switch
            {
                EmailProvider.QQ => "QQ邮箱",
                EmailProvider.NetEase163 => "163邮箱",
                EmailProvider.NetEase126 => "126邮箱",
                EmailProvider.NetEaseYeah => "Yeah邮箱",
                EmailProvider.Sina => "新浪邮箱",
                EmailProvider.Sohu => "搜狐邮箱",
                EmailProvider.Gmail => "Gmail",
                EmailProvider.Outlook => "Outlook",
                EmailProvider.Yahoo => "Yahoo邮箱",
                EmailProvider.ICloud => "iCloud",
                EmailProvider.Aliyun => "阿里云邮箱",
                EmailProvider.Enterprise => "企业邮箱",
                _ => null
            };
        }

        /// <summary>
        /// 判断是否为企业邮箱
        /// </summary>
        /// <param name="email">邮箱地址</param>
        /// <returns>是否为企业邮箱</returns>
        public static bool IsEnterpriseEmail(string? email)
        {
            EmailProvider provider = GetProvider(email);

            // 已知企业邮箱域名
            if (provider == EmailProvider.Enterprise)
            {
                return true;
            }

            // 未知服务商可能是企业邮箱
            if (provider == EmailProvider.Unknown)
            {
                string? domain = GetDomain(email);
                // 排除常见个人邮箱域名后的其他域名可能是企业邮箱
                return domain != null && !IsCommonPublicDomain(domain);
            }

            return false;
        }

        /// <summary>
        /// 判断是否为常见公共邮箱域名
        /// </summary>
        private static bool IsCommonPublicDomain(string domain)
        {
            return ProviderDomainMap.ContainsKey(domain);
        }

        #endregion

        #region 格式化方法

        /// <summary>
        /// 邮箱脱敏：t***@qq.com
        /// </summary>
        /// <param name="email">邮箱地址</param>
        /// <returns>脱敏后的邮箱地址</returns>
        public static string? Mask(string? email)
        {
            if (!IsValidQuick(email))
            {
                return null;
            }

            int atIndex = email!.IndexOf('@');
            string username = email.Substring(0, atIndex);
            string domain = email.Substring(atIndex);

            if (username.Length <= 1)
            {
                return "*" + domain;
            }
            else if (username.Length <= 3)
            {
                return username[0] + new string('*', username.Length - 1) + domain;
            }
            else
            {
                return username.Substring(0, 2) + new string('*', username.Length - 2) + domain;
            }
        }

        /// <summary>
        /// 邮箱脱敏（自定义脱敏字符数）
        /// </summary>
        /// <param name="email">邮箱地址</param>
        /// <param name="visibleChars">用户名可见字符数</param>
        /// <returns>脱敏后的邮箱地址</returns>
        public static string? Mask(string? email, int visibleChars)
        {
            if (!IsValidQuick(email))
            {
                return null;
            }

            int atIndex = email!.IndexOf('@');
            string username = email.Substring(0, atIndex);
            string domain = email.Substring(atIndex);

            if (visibleChars <= 0)
            {
                return new string('*', Math.Min(username.Length, 3)) + domain;
            }

            if (visibleChars >= username.Length)
            {
                return email;
            }

            return username.Substring(0, visibleChars) + new string('*', username.Length - visibleChars) + domain;
        }

        #endregion

        #region 生成方法

        /// <summary>
        /// 生成随机邮箱（仅供测试使用）
        /// </summary>
        /// <param name="provider">邮箱服务商（可选，默认随机）</param>
        /// <returns>随机邮箱地址</returns>
        public static string GenerateRandom(EmailProvider? provider = null)
        {
            string username = GenerateRandomUsername(8);
            string domain;

            if (provider.HasValue && provider.Value != EmailProvider.Unknown && provider.Value != EmailProvider.Enterprise)
            {
                domain = GetDomainByProvider(provider.Value);
            }
            else
            {
                // 随机选择一个服务商
                var providers = new[] { EmailProvider.QQ, EmailProvider.NetEase163, EmailProvider.Gmail, EmailProvider.Outlook };
                var randomProvider = EasyTool.MathCategory.RandomUtil.GetRandomElement(providers);
                domain = GetDomainByProvider(randomProvider);
            }

            return username + "@" + domain;
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 根据服务商获取域名
        /// </summary>
        private static string GetDomainByProvider(EmailProvider provider)
        {
            return provider switch
            {
                EmailProvider.QQ => "qq.com",
                EmailProvider.NetEase163 => "163.com",
                EmailProvider.NetEase126 => "126.com",
                EmailProvider.NetEaseYeah => "yeah.net",
                EmailProvider.Sina => "sina.com",
                EmailProvider.Sohu => "sohu.com",
                EmailProvider.Gmail => "gmail.com",
                EmailProvider.Outlook => "outlook.com",
                EmailProvider.Yahoo => "yahoo.com",
                EmailProvider.ICloud => "icloud.com",
                EmailProvider.Aliyun => "aliyun.com",
                _ => "example.com"
            };
        }

        /// <summary>
        /// 生成随机用户名
        /// </summary>
        private static string GenerateRandomUsername(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            var sb = new System.Text.StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                sb.Append(EasyTool.MathCategory.RandomUtil.GetRandomElement(chars.ToCharArray()));
            }
            return sb.ToString();
        }

        #endregion
    }
}
