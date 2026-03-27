using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace EasyTool.SecurityCategory
{
    /// <summary>
    /// 输入净化器
    /// </summary>
    public static class InputSanitizer
    {
        /// <summary>
        /// 净化选项
        /// </summary>
        public class SanitizeOptions
        {
            /// <summary>
            /// 是否移除HTML标签
            /// </summary>
            public bool RemoveHtmlTags { get; set; } = true;

            /// <summary>
            /// 是否移除脚本
            /// </summary>
            public bool RemoveScripts { get; set; } = true;

            /// <summary>
            /// 是否转义HTML特殊字符
            /// </summary>
            public bool EscapeHtml { get; set; } = true;

            /// <summary>
            /// 是否移除SQL注入
            /// </summary>
            public bool RemoveSqlInjection { get; set; } = true;

            /// <summary>
            /// 是否移除路径遍历
            /// </summary>
            public bool RemovePathTraversal { get; set; } = true;

            /// <summary>
            /// 是否移除控制字符
            /// </summary>
            public bool RemoveControlChars { get; set; } = true;

            /// <summary>
            /// 是否标准化空白
            /// </summary>
            public bool NormalizeWhitespace { get; set; } = false;

            /// <summary>
            /// 最大长度（0表示不限制）
            /// </summary>
            public int MaxLength { get; set; } = 0;

            /// <summary>
            /// 允许的字符正则（为空表示不限制）
            /// </summary>
            public string? AllowedCharsPattern { get; set; }

            /// <summary>
            /// 获取默认选项
            /// </summary>
            public static SanitizeOptions Default => new();

            /// <summary>
            /// 获取严格选项
            /// </summary>
            public static SanitizeOptions Strict => new()
            {
                RemoveHtmlTags = true,
                RemoveScripts = true,
                EscapeHtml = true,
                RemoveSqlInjection = true,
                RemovePathTraversal = true,
                RemoveControlChars = true,
                NormalizeWhitespace = true
            };

            /// <summary>
            /// 获取宽松选项（仅移除脚本）
            /// </summary>
            public static SanitizeOptions Lenient => new()
            {
                RemoveHtmlTags = false,
                RemoveScripts = true,
                EscapeHtml = false,
                RemoveSqlInjection = false,
                RemovePathTraversal = true,
                RemoveControlChars = true
            };
        }

        private static readonly Regex HtmlTagPattern = new(@"<[^>]*>", RegexOptions.Compiled);
        private static readonly Regex ScriptPattern = new(@"<script[^>]*>.*?</script>", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
        private static readonly Regex ControlCharPattern = new(@"[\x00-\x08\x0B\x0C\x0E-\x1F]", RegexOptions.Compiled);
        private static readonly Regex PathTraversalPattern = new(@"(\.\.[\\/])|([\\/]\.\.)", RegexOptions.Compiled);
        private static readonly Regex MultiWhitespacePattern = new(@"\s+", RegexOptions.Compiled);

        /// <summary>
        /// 净化输入字符串
        /// </summary>
        public static string Sanitize(string? input, SanitizeOptions? options = null)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            options ??= SanitizeOptions.Default;
            var result = input;

            // 移除控制字符
            if (options.RemoveControlChars)
            {
                result = ControlCharPattern.Replace(result, "");
            }

            // 移除脚本
            if (options.RemoveScripts)
            {
                result = ScriptPattern.Replace(result, "");
            }

            // 移除HTML标签
            if (options.RemoveHtmlTags)
            {
                result = HtmlTagPattern.Replace(result, "");
            }

            // 转义HTML
            if (options.EscapeHtml)
            {
                result = EscapeHtml(result);
            }

            // 移除SQL注入
            if (options.RemoveSqlInjection)
            {
                result = SqlInjectionUtil.Sanitize(result);
            }

            // 移除路径遍历
            if (options.RemovePathTraversal)
            {
                result = PathTraversalPattern.Replace(result, "");
            }

            // 标准化空白
            if (options.NormalizeWhitespace)
            {
                result = MultiWhitespacePattern.Replace(result, " ").Trim();
            }

            // 过滤允许的字符
            if (!string.IsNullOrEmpty(options.AllowedCharsPattern))
            {
                result = Regex.Replace(result, options.AllowedCharsPattern, "");
            }

            // 限制长度
            if (options.MaxLength > 0 && result.Length > options.MaxLength)
            {
                result = result.Substring(0, options.MaxLength);
            }

            return result;
        }

        /// <summary>
        /// 净化文件名
        /// </summary>
        public static string SanitizeFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return string.Empty;

            var invalidChars = new string(System.IO.Path.GetInvalidFileNameChars());
            var pattern = $"[{Regex.Escape(invalidChars)}]";
            var result = Regex.Replace(fileName, pattern, "_");

            // 移除路径遍历
            result = PathTraversalPattern.Replace(result, "");

            // 移除前导/尾随空格和点
            result = result.Trim().Trim('.');

            return result;
        }

        /// <summary>
        /// 净化路径
        /// </summary>
        public static string SanitizePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return string.Empty;

            var invalidChars = new string(System.IO.Path.GetInvalidPathChars());
            var pattern = $"[{Regex.Escape(invalidChars)}]";
            var result = Regex.Replace(path, pattern, "_");

            // 移除路径遍历
            result = PathTraversalPattern.Replace(result, "");

            return result;
        }

        /// <summary>
        /// 净化URL
        /// </summary>
        public static string SanitizeUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                return string.Empty;

            // 只允许http/https协议
            var lower = url.ToLower().Trim();
            if (!lower.StartsWith("http://") && !lower.StartsWith("https://"))
            {
                return string.Empty;
            }

            // 移除危险字符
            var result = url.Replace("<", "").Replace(">", "").Replace("\"", "");
            result = PathTraversalPattern.Replace(result, "");

            return result;
        }

        /// <summary>
        /// 净化邮箱
        /// </summary>
        public static string SanitizeEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                return string.Empty;

            var result = email.ToLowerInvariant().Trim();
            
            // 移除控制字符
            result = ControlCharPattern.Replace(result, "");

            // 基本验证
            if (!Regex.IsMatch(result, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                return string.Empty;

            return result;
        }

        /// <summary>
        /// 净化电话号码
        /// </summary>
        public static string SanitizePhone(string phone)
        {
            if (string.IsNullOrEmpty(phone))
                return string.Empty;

            // 只保留数字和+
            var result = Regex.Replace(phone, @"[^\d+]", "");
            
            // 验证格式
            if (!Regex.IsMatch(result, @"^\+?\d{6,15}$"))
                return string.Empty;

            return result;
        }

        /// <summary>
        /// 净化数字字符串
        /// </summary>
        public static string SanitizeNumber(string input, bool allowDecimal = false, bool allowNegative = false)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            var pattern = allowDecimal
                ? (allowNegative ? @"[^0-9.\-]" : @"[^0-9.]")
                : (allowNegative ? @"[^0-9\-]" : @"[^0-9]");

            var result = Regex.Replace(input, pattern, "");

            // 验证格式
            if (allowDecimal)
            {
                if (!decimal.TryParse(result, out _))
                    return string.Empty;
            }
            else
            {
                if (!long.TryParse(result, out _))
                    return string.Empty;
            }

            return result;
        }

        /// <summary>
        /// 净化JSON字符串
        /// </summary>
        public static string SanitizeJson(string json)
        {
            if (string.IsNullOrEmpty(json))
                return string.Empty;

            var result = new StringBuilder(json.Length);
            foreach (var c in json)
            {
                switch (c)
                {
                    case '"':
                        result.Append("\\\"");
                        break;
                    case '\\':
                        result.Append("\\\\");
                        break;
                    case '\b':
                        result.Append("\\b");
                        break;
                    case '\f':
                        result.Append("\\f");
                        break;
                    case '\n':
                        result.Append("\\n");
                        break;
                    case '\r':
                        result.Append("\\r");
                        break;
                    case '\t':
                        result.Append("\\t");
                        break;
                    default:
                        if (c < 32)
                        {
                            result.Append($"\\u{(int)c:X4}");
                        }
                        else
                        {
                            result.Append(c);
                        }
                        break;
                }
            }
            return result.ToString();
        }

        /// <summary>
        /// 净化XML字符串
        /// </summary>
        public static string SanitizeXml(string xml)
        {
            if (string.IsNullOrEmpty(xml))
                return string.Empty;

            var result = new StringBuilder(xml.Length);
            foreach (var c in xml)
            {
                result.Append(c switch
                {
                    '<' => "&lt;",
                    '>' => "&gt;",
                    '&' => "&amp;",
                    '"' => "&quot;",
                    '\'' => "&apos;",
                    _ => c
                });
            }
            return result.ToString();
        }

        private static string EscapeHtml(string input)
        {
            var result = new StringBuilder(input.Length);
            foreach (var c in input)
            {
                result.Append(c switch
                {
                    '<' => "&lt;",
                    '>' => "&gt;",
                    '&' => "&amp;",
                    '"' => "&quot;",
                    '\'' => "&#x27;",
                    '/' => "&#x2F;",
                    _ => c
                });
            }
            return result.ToString();
        }

        /// <summary>
        /// 批量净化
        /// </summary>
        public static Dictionary<string, string> SanitizeMultiple(IDictionary<string, string> inputs, SanitizeOptions? options = null)
        {
            var results = new Dictionary<string, string>();
            foreach (var kvp in inputs)
            {
                results[kvp.Key] = Sanitize(kvp.Value, options);
            }
            return results;
        }
    }
}
