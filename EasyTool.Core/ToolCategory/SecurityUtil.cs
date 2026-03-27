using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace EasyTool.ToolCategory
{
    /// <summary>
    /// 安全工具类
    /// 提供XSS防护、SQL注入检测、HTML净化等功能
    /// </summary>
    public static class SecurityUtil
    {
        #region HTML编码/解码

        /// <summary>
        /// HTML编码
        /// </summary>
        public static string HtmlEncode(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var sb = new StringBuilder(input.Length);
            foreach (var c in input)
            {
                switch (c)
                {
                    case '<': sb.Append("&lt;"); break;
                    case '>': sb.Append("&gt;"); break;
                    case '&': sb.Append("&amp;"); break;
                    case '"': sb.Append("&quot;"); break;
                    case '\'': sb.Append("&#39;"); break;
                    default: sb.Append(c); break;
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// HTML解码
        /// </summary>
        public static string HtmlDecode(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return input
                .Replace("&lt;", "<")
                .Replace("&gt;", ">")
                .Replace("&amp;", "&")
                .Replace("&quot;", "\"")
                .Replace("&#39;", "'")
                .Replace("&nbsp;", " ")
                .Replace("&copy;", "©")
                .Replace("&reg;", "®")
                .Replace("&trade;", "™");
        }

        #endregion

        #region XSS防护

        /// <summary>
        /// 检测是否包含XSS攻击
        /// </summary>
        public static bool ContainsXss(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;

            var patterns = new[]
            {
                @"<script[^>]*>.*?</script>",
                @"javascript\s*:",
                @"on\w+\s*=",
                @"<iframe",
                @"<object",
                @"<embed",
                @"<form",
                @"expression\s*\(",
                @"vbscript\s*:",
                @"<link",
                @"<style",
                @"<base",
                @"data\s*:",
                @"<meta"
            };

            return patterns.Any(p => Regex.IsMatch(input, p, RegexOptions.IgnoreCase));
        }

        /// <summary>
        /// 清理XSS攻击代码
        /// </summary>
        public static string SanitizeXss(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // 移除危险标签
            var sanitized = Regex.Replace(input, @"<script[^>]*>.*?</script>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            sanitized = Regex.Replace(sanitized, @"<iframe[^>]*>.*?</iframe>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            sanitized = Regex.Replace(sanitized, @"<object[^>]*>.*?</object>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            sanitized = Regex.Replace(sanitized, @"<embed[^>]*>", "", RegexOptions.IgnoreCase);
            sanitized = Regex.Replace(sanitized, @"<form[^>]*>", "", RegexOptions.IgnoreCase);

            // 移除危险属性
            sanitized = Regex.Replace(sanitized, @"javascript\s*:", "", RegexOptions.IgnoreCase);
            sanitized = Regex.Replace(sanitized, @"vbscript\s*:", "", RegexOptions.IgnoreCase);
            sanitized = Regex.Replace(sanitized, @"on\w+\s*=\s*[""'][^""']*[""']", "", RegexOptions.IgnoreCase);
            sanitized = Regex.Replace(sanitized, @"expression\s*\([^)]*\)", "", RegexOptions.IgnoreCase);

            return sanitized;
        }

        #endregion

        #region SQL注入检测

        private static readonly string[] SqlKeywords = new[]
        {
            "select", "insert", "update", "delete", "drop", "create", "alter", "truncate",
            "exec", "execute", "xp_", "sp_", "union", "join", "where", "from", "into",
            "having", "group by", "order by", "--", "/*", "*/", ";", "declare", "cursor"
        };

        private static readonly string[] SqlFunctions = new[]
        {
            "char(", "nchar(", "varchar(", "nvarchar(", "cast(", "convert(",
            "concat(", "substring(", "len(", "count(", "sum(", "avg(", "max(", "min("
        };

        /// <summary>
        /// 检测是否包含SQL注入
        /// </summary>
        public static bool ContainsSqlInjection(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;

            var lowerInput = input.ToLower();

            // 检查关键字
            foreach (var keyword in SqlKeywords)
            {
                if (lowerInput.Contains(keyword))
                {
                    // 检查是否是单词边界
                    var pattern = $@"\b{Regex.Escape(keyword)}\b";
                    if (Regex.IsMatch(lowerInput, pattern, RegexOptions.IgnoreCase))
                        return true;
                }
            }

            // 检查函数
            foreach (var func in SqlFunctions)
            {
                if (lowerInput.Contains(func))
                    return true;
            }

            // 检查单引号
            if (input.Contains("'") && (input.Contains("''") || input.Contains("' or ") || input.Contains("' and ")))
                return true;

            // 检查等号注入
            if (Regex.IsMatch(input, @"'?\s*=\s*'?", RegexOptions.IgnoreCase))
                return true;

            return false;
        }

        /// <summary>
        /// 清理SQL注入代码
        /// </summary>
        public static string SanitizeSqlInjection(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // 转义单引号
            var sanitized = input.Replace("'", "''");

            return sanitized;
        }

        #endregion

        #region 密码强度检测

        /// <summary>
        /// 检测密码强度
        /// </summary>
        public static PasswordStrength CheckPasswordStrength(string password)
        {
            if (string.IsNullOrEmpty(password))
                return PasswordStrength.VeryWeak;

            var score = 0;

            // 长度评分
            if (password.Length >= 8) score++;
            if (password.Length >= 12) score++;
            if (password.Length >= 16) score++;

            // 包含小写字母
            if (Regex.IsMatch(password, @"[a-z]")) score++;

            // 包含大写字母
            if (Regex.IsMatch(password, @"[A-Z]")) score++;

            // 包含数字
            if (Regex.IsMatch(password, @"\d")) score++;

            // 包含特殊字符
            if (Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>/?]")) score++;

            // 不包含连续字符
            if (!HasConsecutiveChars(password)) score++;

            // 不包含常见模式
            if (!HasCommonPatterns(password)) score++;

            return score switch
            {
                <= 2 => PasswordStrength.VeryWeak,
                3 => PasswordStrength.Weak,
                4 => PasswordStrength.Fair,
                5 => PasswordStrength.Good,
                6 => PasswordStrength.Strong,
                _ => PasswordStrength.VeryStrong
            };
        }

        private static bool HasConsecutiveChars(string input)
        {
            for (int i = 0; i < input.Length - 2; i++)
            {
                if (input[i] + 1 == input[i + 1] && input[i + 1] + 1 == input[i + 2])
                    return true;
            }
            return false;
        }

        private static bool HasCommonPatterns(string input)
        {
            var patterns = new[] { "123", "abc", "qwe", "password", "admin", "111", "000" };
            var lowerInput = input.ToLower();
            return patterns.Any(p => lowerInput.Contains(p));
        }

        #endregion

        #region HTML净化

        private static readonly HashSet<string> AllowedTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "p", "br", "b", "i", "u", "strong", "em", "span", "div", "a", "img",
            "ul", "ol", "li", "table", "tr", "td", "th", "thead", "tbody", "h1", "h2", "h3", "h4", "h5", "h6",
            "blockquote", "pre", "code", "hr"
        };

        private static readonly HashSet<string> AllowedAttributes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "href", "src", "alt", "title", "class", "id", "style", "target", "rel"
        };

        /// <summary>
        /// 净化HTML
        /// </summary>
        public static string SanitizeHtml(string input, IEnumerable<string>? allowedTags = null, IEnumerable<string>? allowedAttributes = null)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var tags = allowedTags != null ? new HashSet<string>(allowedTags, StringComparer.OrdinalIgnoreCase) : AllowedTags;
            var attrs = allowedAttributes != null ? new HashSet<string>(allowedAttributes, StringComparer.OrdinalIgnoreCase) : AllowedAttributes;

            // 移除注释
            var result = Regex.Replace(input, @"<!--.*?-->", "", RegexOptions.Singleline);

            // 处理标签
            result = Regex.Replace(result, @"<(/?)(\w+)([^>]*)>", match =>
            {
                var isClosing = match.Groups[1].Value == "/";
                var tagName = match.Groups[2].Value.ToLower();
                var attributes = match.Groups[3].Value;

                if (!tags.Contains(tagName))
                    return "";

                if (isClosing)
                    return $"</{tagName}>";

                // 过滤属性
                var filteredAttrs = FilterAttributes(attributes, attrs);
                return $"<{tagName}{filteredAttrs}>";
            }, RegexOptions.Singleline);

            return result;
        }

        private static string FilterAttributes(string attributes, HashSet<string> allowedAttrs)
        {
            var result = new StringBuilder();
            var matches = Regex.Matches(attributes, @"(\w+)\s*=\s*[""']([^""']*)[""']");

            foreach (Match match in matches)
            {
                var attrName = match.Groups[1].Value.ToLower();
                var attrValue = match.Groups[2].Value;

                if (allowedAttrs.Contains(attrName))
                {
                    // 检查危险属性值
                    if (attrValue.ToLower().Contains("javascript:") ||
                        attrValue.ToLower().Contains("vbscript:") ||
                        attrValue.ToLower().Contains("expression("))
                        continue;

                    result.Append($" {attrName}=\"{attrValue}\"");
                }
            }

            return result.ToString();
        }

        #endregion

        #region 路径安全

        /// <summary>
        /// 检测路径是否安全
        /// </summary>
        public static bool IsPathSafe(string path)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            // 检查路径遍历攻击
            if (path.Contains("..") || path.Contains("~"))
                return false;

            // 检查绝对路径
            if (Path.IsPathRooted(path))
                return false;

            // 检查无效字符
            var invalidChars = Path.GetInvalidFileNameChars();
            var fileName = Path.GetFileName(path);
            if (fileName.IndexOfAny(invalidChars) >= 0)
                return false;

            return true;
        }

        /// <summary>
        /// 安全路径拼接
        /// </summary>
        public static string SafeCombine(string basePath, string relativePath)
        {
            if (string.IsNullOrEmpty(basePath) || string.IsNullOrEmpty(relativePath))
                throw new ArgumentException("路径不能为空");

            if (!IsPathSafe(relativePath))
                throw new ArgumentException("相对路径不安全");

            var fullPath = Path.GetFullPath(Path.Combine(basePath, relativePath));
            var normalizedBase = Path.GetFullPath(basePath);

            if (!fullPath.StartsWith(normalizedBase, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("路径遍历攻击检测");

            return fullPath;
        }

        #endregion

        #region 敏感信息脱敏

        /// <summary>
        /// 手机号脱敏
        /// </summary>
        public static string MaskPhone(string phone)
        {
            if (string.IsNullOrEmpty(phone) || phone.Length < 7)
                return phone;

            return phone.Substring(0, 3) + "****" + phone.Substring(phone.Length - 4);
        }

        /// <summary>
        /// 身份证号脱敏
        /// </summary>
        public static string MaskIdCard(string idCard)
        {
            if (string.IsNullOrEmpty(idCard) || idCard.Length < 8)
                return idCard;

            return idCard.Substring(0, 4) + "**********" + idCard.Substring(idCard.Length - 4);
        }

        /// <summary>
        /// 邮箱脱敏
        /// </summary>
        public static string MaskEmail(string email)
        {
            if (string.IsNullOrEmpty(email) || !email.Contains("@"))
                return email;

            var parts = email.Split('@');
            var name = parts[0];
            var domain = parts[1];

            if (name.Length <= 2)
                return name[0] + "***@" + domain;

            return name.Substring(0, 2) + "***@" + domain;
        }

        /// <summary>
        /// 银行卡号脱敏
        /// </summary>
        public static string MaskBankCard(string cardNumber)
        {
            if (string.IsNullOrEmpty(cardNumber) || cardNumber.Length < 8)
                return cardNumber;

            return cardNumber.Substring(0, 4) + " **** **** " + cardNumber.Substring(cardNumber.Length - 4);
        }

        #endregion
    }

    /// <summary>
    /// 密码强度等级
    /// </summary>
    public enum PasswordStrength
    {
        /// <summary>
        /// 非常弱
        /// </summary>
        VeryWeak,

        /// <summary>
        /// 弱
        /// </summary>
        Weak,

        /// <summary>
        /// 一般
        /// </summary>
        Fair,

        /// <summary>
        /// 好
        /// </summary>
        Good,

        /// <summary>
        /// 强
        /// </summary>
        Strong,

        /// <summary>
        /// 非常强
        /// </summary>
        VeryStrong
    }
}