using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace EasyTool.SecurityCategory
{
    /// <summary>
    /// XSS防护工具类
    /// </summary>
    public static class XssUtil
    {
        private static readonly Dictionary<string, string> HtmlEntityEncodeMap = new()
        {
            { "<", "&lt;" },
            { ">", "&gt;" },
            { "&", "&amp;" },
            { "\"", "&quot;" },
            { "'", "&#x27;" },
            { "/", "&#x2F;" },
            { "`", "&#x60;" },
            { "=", "&#x3D;" }
        };

        private static readonly HashSet<string> AllowedTags = new(StringComparer.OrdinalIgnoreCase)
        {
            "b", "i", "u", "strong", "em", "p", "br", "span", "div", "a", "img",
            "ul", "ol", "li", "h1", "h2", "h3", "h4", "h5", "h6",
            "table", "thead", "tbody", "tr", "td", "th", "blockquote", "pre", "code"
        };

        private static readonly HashSet<string> AllowedAttributes = new(StringComparer.OrdinalIgnoreCase)
        {
            "href", "src", "alt", "title", "class", "id", "style"
        };

        private static readonly Regex ScriptPattern = new(@"<script[^>]*>.*?</script>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        private static readonly Regex EventPattern = new(@"\s*on\w+\s*=", RegexOptions.IgnoreCase);
        private static readonly Regex JavaScriptPattern = new(@"javascript\s*:", RegexOptions.IgnoreCase);
        private static readonly Regex VbscriptPattern = new(@"vbscript\s*:", RegexOptions.IgnoreCase);
        private static readonly Regex DataUrlPattern = new(@"data\s*:", RegexOptions.IgnoreCase);
        private static readonly Regex HtmlCommentPattern = new(@"<!--.*?-->", RegexOptions.Singleline);
        private static readonly Regex SvgPattern = new(@"<svg[^>]*>.*?</svg>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        private static readonly Regex IframePattern = new(@"<iframe[^>]*>.*?</iframe>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        private static readonly Regex ObjectPattern = new(@"<object[^>]*>.*?</object>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        private static readonly Regex EmbedPattern = new(@"<embed[^>]*>", RegexOptions.IgnoreCase);
        private static readonly Regex ExpressionPattern = new(@"expression\s*\(", RegexOptions.IgnoreCase);

        /// <summary>
        /// HTML实体编码
        /// </summary>
        public static string HtmlEncode(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var result = new StringBuilder(input.Length);
            foreach (var c in input)
            {
                var str = c.ToString();
                result.Append(HtmlEntityEncodeMap.TryGetValue(str, out var encoded) ? encoded : str);
            }
            return result.ToString();
        }

        /// <summary>
        /// HTML实体解码
        /// </summary>
        public static string HtmlDecode(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var result = input;
            foreach (var kvp in HtmlEntityEncodeMap)
            {
                result = result.Replace(kvp.Value, kvp.Key);
            }

            // 解码数字实体
            result = Regex.Replace(result, @"&#(\d+);", m =>
            {
                var code = int.Parse(m.Groups[1].Value);
                return ((char)code).ToString();
            });

            // 解码十六进制实体
            result = Regex.Replace(result, @"&#x([0-9a-fA-F]+);", m =>
            {
                var code = Convert.ToInt32(m.Groups[1].Value, 16);
                return ((char)code).ToString();
            }, RegexOptions.IgnoreCase);

            return result;
        }

        /// <summary>
        /// 过滤XSS攻击代码
        /// </summary>
        public static string Sanitize(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var result = input;

            // 移除脚本标签
            result = ScriptPattern.Replace(result, "");
            result = SvgPattern.Replace(result, "");
            result = IframePattern.Replace(result, "");
            result = ObjectPattern.Replace(result, "");
            result = EmbedPattern.Replace(result, "");

            // 移除事件处理器
            result = EventPattern.Replace(result, "");

            // 移除危险协议
            result = JavaScriptPattern.Replace(result, "");
            result = VbscriptPattern.Replace(result, "");
            result = DataUrlPattern.Replace(result, "");

            // 移除CSS表达式
            result = ExpressionPattern.Replace(result, "");

            // 移除HTML注释
            result = HtmlCommentPattern.Replace(result, "");

            return result;
        }

        /// <summary>
        /// 清理HTML标签（只保留允许的标签和属性）
        /// </summary>
        public static string CleanHtml(string input, IEnumerable<string>? allowedTags = null, IEnumerable<string>? allowedAttributes = null)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var tags = allowedTags != null ? new HashSet<string>(allowedTags, StringComparer.OrdinalIgnoreCase) : AllowedTags;
            var attrs = allowedAttributes != null ? new HashSet<string>(allowedAttributes, StringComparer.OrdinalIgnoreCase) : AllowedAttributes;

            // 先进行基本清理
            var result = Sanitize(input);

            // 移除不允许的标签
            result = Regex.Replace(result, @"</?(\w+)[^>]*>", m =>
            {
                var tagName = m.Groups[1].Value;
                if (!tags.Contains(tagName))
                {
                    return "";
                }

                // 保留标签但移除不允许的属性
                var tagContent = m.Value;
                tagContent = Regex.Replace(tagContent, @"(\w+)\s*=\s*[""'][^""']*[""']", attrMatch =>
                {
                    var attrName = Regex.Match(attrMatch.Value, @"\w+").Value;
                    return attrs.Contains(attrName) ? attrMatch.Value : "";
                });

                return tagContent;
            }, RegexOptions.IgnoreCase);

            return result;
        }

        /// <summary>
        /// 移除所有HTML标签
        /// </summary>
        public static string StripHtml(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return Regex.Replace(input, @"<[^>]*>", "");
        }

        /// <summary>
        /// 验证是否包含XSS攻击代码
        /// </summary>
        public static bool ContainsXss(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;

            return ScriptPattern.IsMatch(input) ||
                   EventPattern.IsMatch(input) ||
                   JavaScriptPattern.IsMatch(input) ||
                   VbscriptPattern.IsMatch(input) ||
                   DataUrlPattern.IsMatch(input) ||
                   ExpressionPattern.IsMatch(input) ||
                   SvgPattern.IsMatch(input) ||
                   IframePattern.IsMatch(input) ||
                   ObjectPattern.IsMatch(input) ||
                   EmbedPattern.IsMatch(input);
        }

        /// <summary>
        /// 安全的URL编码
        /// </summary>
        public static string SafeUrlEncode(string url)
        {
            if (string.IsNullOrEmpty(url))
                return url;

            // 检查危险的协议
            var lowerUrl = url.ToLower();
            if (lowerUrl.StartsWith("javascript:") || lowerUrl.StartsWith("vbscript:") || lowerUrl.StartsWith("data:"))
            {
                return "";
            }

            return Uri.EscapeUriString(url);
        }

        /// <summary>
        /// 验证URL是否安全
        /// </summary>
        public static bool IsUrlSafe(string url)
        {
            if (string.IsNullOrEmpty(url))
                return false;

            var lowerUrl = url.ToLower();
            if (lowerUrl.StartsWith("javascript:") || lowerUrl.StartsWith("vbscript:") || lowerUrl.StartsWith("data:"))
            {
                return false;
            }

            return Uri.TryCreate(url, UriKind.Absolute, out _);
        }

        /// <summary>
        /// 清理CSS样式（移除表达式和URL）
        /// </summary>
        public static string CleanCss(string css)
        {
            if (string.IsNullOrEmpty(css))
                return css;

            var result = css;

            // 移除expression
            result = ExpressionPattern.Replace(result, "");

            // 移除url()
            result = Regex.Replace(result, @"url\s*\([^)]*\)", "", RegexOptions.IgnoreCase);

            // 移除behavior
            result = Regex.Replace(result, @"behavior\s*:", "", RegexOptions.IgnoreCase);

            // 移除-moz-binding
            result = Regex.Replace(result, @"-moz-binding\s*:", "", RegexOptions.IgnoreCase);

            return result;
        }

        /// <summary>
        /// 安全的JSON字符串
        /// </summary>
        public static string SafeJsonString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var result = new StringBuilder();
            foreach (var c in input)
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
        /// 属性值转义
        /// </summary>
        public static string EscapeAttribute(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var result = new StringBuilder();
            foreach (var c in input)
            {
                switch (c)
                {
                    case '<':
                        result.Append("&lt;");
                        break;
                    case '>':
                        result.Append("&gt;");
                        break;
                    case '"':
                        result.Append("&quot;");
                        break;
                    case '\'':
                        result.Append("&#x27;");
                        break;
                    case '&':
                        result.Append("&amp;");
                        break;
                    default:
                        result.Append(c);
                        break;
                }
            }
            return result.ToString();
        }
    }
}
