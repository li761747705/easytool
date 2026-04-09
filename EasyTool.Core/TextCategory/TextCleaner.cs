using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace EasyTool
{
    /// <summary>
    /// 文本清洗器
    /// 支持 HTML 标签清理、特殊字符处理、空白符规范化
    /// </summary>
    public static class TextCleaner
    {
        #region HTML 清理

        /// <summary>
        /// 移除 HTML 标签
        /// </summary>
        /// <param name="html">HTML 文本</param>
        /// <returns>纯文本</returns>
        public static string RemoveHtmlTags(string html)
        {
            if (string.IsNullOrEmpty(html))
                return string.Empty;

            // 移除 script 和 style 标签及其内容
            var result = Regex.Replace(html, @"<script[^>]*>.*?</script>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            result = Regex.Replace(result, @"<style[^>]*>.*?</style>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            // 移除 HTML 注释
            result = Regex.Replace(result, @"<!--.*?-->", "", RegexOptions.Singleline);

            // 移除所有 HTML 标签
            result = Regex.Replace(result, @"<[^>]+>", "");

            // 解码 HTML 实体
            result = System.Net.WebUtility.HtmlDecode(result);

            return result;
        }

        /// <summary>
        /// 仅保留允许的 HTML 标签
        /// </summary>
        /// <param name="html">HTML 文本</param>
        /// <param name="allowedTags">允许的标签列表</param>
        /// <returns>清理后的 HTML</returns>
        public static string SanitizeHtml(string html, params string[] allowedTags)
        {
            if (string.IsNullOrEmpty(html))
                return string.Empty;

            var allowedSet = new HashSet<string>(allowedTags, StringComparer.OrdinalIgnoreCase);
            var result = new StringBuilder();

            // 移除危险标签
            var sanitized = Regex.Replace(html, @"<script[^>]*>.*?</script>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            sanitized = Regex.Replace(sanitized, @"<iframe[^>]*>.*?</iframe>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            sanitized = Regex.Replace(sanitized, @"<object[^>]*>.*?</object>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            sanitized = Regex.Replace(sanitized, @"<embed[^>]*>.*?</embed>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            sanitized = Regex.Replace(sanitized, @"<!--.*?-->", "", RegexOptions.Singleline);

            // 移除事件属性
            sanitized = Regex.Replace(sanitized, @"\s+on\w+\s*=\s*""[^""]*""", "", RegexOptions.IgnoreCase);
            sanitized = Regex.Replace(sanitized, @"\s+on\w+\s*=\s*'[^']*'", "", RegexOptions.IgnoreCase);

            // 处理标签
            var tagPattern = @"</?([a-zA-Z][a-zA-Z0-9]*)[^>]*>";
            var lastIndex = 0;

            foreach (Match match in Regex.Matches(sanitized, tagPattern))
            {
                result.Append(sanitized.Substring(lastIndex, match.Index - lastIndex));

                if (allowedSet.Contains(match.Groups[1].Value))
                {
                    result.Append(match.Value);
                }

                lastIndex = match.Index + match.Length;
            }

            if (lastIndex < sanitized.Length)
            {
                result.Append(sanitized.Substring(lastIndex));
            }

            return result.ToString();
        }

        #endregion

        #region 特殊字符处理

        /// <summary>
        /// 移除特殊字符
        /// </summary>
        /// <param name="text">文本</param>
        /// <param name="keepLetters">保留字母</param>
        /// <param name="keepDigits">保留数字</param>
        /// <param name="keepChinese">保留中文</param>
        /// <param name="additionalChars">额外保留的字符</param>
        /// <returns>清理后的文本</returns>
        public static string RemoveSpecialChars(
            string text,
            bool keepLetters = true,
            bool keepDigits = true,
            bool keepChinese = true,
            string? additionalChars = null)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            var pattern = new StringBuilder("[^");

            if (keepLetters)
            {
                pattern.Append("a-zA-Z");
            }

            if (keepDigits)
            {
                pattern.Append("0-9");
            }

            if (keepChinese)
            {
                pattern.Append(@"\u4e00-\u9fa5");
            }

            if (!string.IsNullOrEmpty(additionalChars))
            {
                pattern.Append(Regex.Escape(additionalChars));
            }

            pattern.Append("]");

            return Regex.Replace(text, pattern.ToString(), "");
        }

        /// <summary>
        /// 移除控制字符
        /// </summary>
        /// <param name="text">文本</param>
        /// <returns>清理后的文本</returns>
        public static string RemoveControlChars(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            var result = new StringBuilder();
            foreach (var c in text)
            {
                if (!char.IsControl(c) || c == '\r' || c == '\n' || c == '\t')
                {
                    result.Append(c);
                }
            }
            return result.ToString();
        }

        /// <summary>
        /// 移除表情符号
        /// </summary>
        /// <param name="text">文本</param>
        /// <returns>清理后的文本</returns>
        public static string RemoveEmojis(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            // 移除常见表情符号范围
            var result = Regex.Replace(text, @"[\uD800-\uDBFF][\uDC00-\uDFFF]", "");
            result = Regex.Replace(result, @"[\u2600-\u26FF\u2700-\u27BF]", "");
            result = Regex.Replace(result, @"[\uFE00-\uFE0F]", "");
            result = Regex.Replace(result, @"[\u1F600-\u1F64F]", "");
            result = Regex.Replace(result, @"[\u1F300-\u1F5FF]", "");
            result = Regex.Replace(result, @"[\u1F680-\u1F6FF]", "");
            result = Regex.Replace(result, @"[\u1F1E0-\u1F1FF]", "");

            return result;
        }

        /// <summary>
        /// 转义 SQL 特殊字符
        /// </summary>
        /// <param name="text">文本</param>
        /// <returns>转义后的文本</returns>
        public static string EscapeSql(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            return text.Replace("'", "''");
        }

        /// <summary>
        /// 转义 JSON 特殊字符
        /// </summary>
        /// <param name="text">文本</param>
        /// <returns>转义后的文本</returns>
        public static string EscapeJson(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            var result = new StringBuilder();
            foreach (var c in text)
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
                        result.Append(c);
                        break;
                }
            }
            return result.ToString();
        }

        /// <summary>
        /// 转义 XML 特殊字符
        /// </summary>
        /// <param name="text">文本</param>
        /// <returns>转义后的文本</returns>
        public static string EscapeXml(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            return text.Replace("&", "&amp;")
                       .Replace("<", "&lt;")
                       .Replace(">", "&gt;")
                       .Replace("\"", "&quot;")
                       .Replace("'", "&apos;");
        }

        /// <summary>
        /// 反转义 XML 特殊字符
        /// </summary>
        /// <param name="text">文本</param>
        /// <returns>反转义后的文本</returns>
        public static string UnescapeXml(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            return text.Replace("&apos;", "'")
                       .Replace("&quot;", "\"")
                       .Replace("&gt;", ">")
                       .Replace("&lt;", "<")
                       .Replace("&amp;", "&");
        }

        #endregion

        #region 空白符处理

        /// <summary>
        /// 规范化空白符（多个空白符合并为一个空格）
        /// </summary>
        /// <param name="text">文本</param>
        /// <returns>规范化后的文本</returns>
        public static string NormalizeWhitespace(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            return Regex.Replace(text, @"\s+", " ").Trim();
        }

        /// <summary>
        /// 移除所有空白符
        /// </summary>
        /// <param name="text">文本</param>
        /// <returns>无空白符的文本</returns>
        public static string RemoveWhitespace(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            return Regex.Replace(text, @"\s+", "");
        }

        /// <summary>
        /// 移除多余的空行（保留一个空行）
        /// </summary>
        /// <param name="text">文本</param>
        /// <returns>处理后的文本</returns>
        public static string RemoveEmptyLines(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            return Regex.Replace(text, @"(\r?\n\s*){2,}", "\r\n\r\n").Trim();
        }

        /// <summary>
        /// 移除所有空行
        /// </summary>
        /// <param name="text">文本</param>
        /// <returns>处理后的文本</returns>
        public static string RemoveAllEmptyLines(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            var lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var nonEmptyLines = lines.Where(line => !string.IsNullOrWhiteSpace(line));
            return string.Join("\r\n", nonEmptyLines);
        }

        /// <summary>
        /// 统一行尾符
        /// </summary>
        /// <param name="text">文本</param>
        /// <param name="lineEnding">行尾符类型</param>
        /// <returns>处理后的文本</returns>
        public static string NormalizeLineEndings(string text, LineEnding lineEnding = LineEnding.CRLF)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            // 先统一为 LF
            var normalized = text.Replace("\r\n", "\n").Replace("\r", "\n");

            // 再转换为目标行尾符
            return lineEnding switch
            {
                LineEnding.LF => normalized,
                LineEnding.CRLF => normalized.Replace("\n", "\r\n"),
                LineEnding.CR => normalized.Replace("\n", "\r"),
                _ => normalized
            };
        }

        /// <summary>
        /// 去除首尾空白（包括中文全角空格）
        /// </summary>
        /// <param name="text">文本</param>
        /// <returns>处理后的文本</returns>
        public static string TrimFull(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            return text.Trim().Trim('\u3000');
        }

        /// <summary>
        /// 去除所有行首尾空白
        /// </summary>
        /// <param name="text">文本</param>
        /// <returns>处理后的文本</returns>
        public static string TrimLines(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            var lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var trimmedLines = lines.Select(line => line.Trim());
            return string.Join("\r\n", trimmedLines);
        }

        #endregion

        #region 大小写转换

        /// <summary>
        /// 转换为驼峰命名
        /// </summary>
        /// <param name="text">文本</param>
        /// <param name="separator">分隔符</param>
        /// <returns>驼峰命名</returns>
        public static string ToCamelCase(string text, params char[] separator)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            var separators = separator.Length > 0 ? separator : new[] { '_', '-', ' ' };
            var parts = text.Split(separators, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 0)
                return string.Empty;

            var result = new StringBuilder();
            result.Append(parts[0].ToLowerInvariant());

            for (int i = 1; i < parts.Length; i++)
            {
                if (parts[i].Length > 0)
                {
                    result.Append(char.ToUpperInvariant(parts[i][0]));
                    if (parts[i].Length > 1)
                    {
                        result.Append(parts[i].Substring(1).ToLowerInvariant());
                    }
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// 转换为帕斯卡命名
        /// </summary>
        /// <param name="text">文本</param>
        /// <param name="separator">分隔符</param>
        /// <returns>帕斯卡命名</returns>
        public static string ToPascalCase(string text, params char[] separator)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            var separators = separator.Length > 0 ? separator : new[] { '_', '-', ' ' };
            var parts = text.Split(separators, StringSplitOptions.RemoveEmptyEntries);

            var result = new StringBuilder();
            foreach (var part in parts)
            {
                if (part.Length > 0)
                {
                    result.Append(char.ToUpperInvariant(part[0]));
                    if (part.Length > 1)
                    {
                        result.Append(part.Substring(1).ToLowerInvariant());
                    }
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// 转换为下划线命名
        /// </summary>
        /// <param name="text">文本</param>
        /// <returns>下划线命名</returns>
        public static string ToSnakeCase(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            var result = new StringBuilder();
            for (int i = 0; i < text.Length; i++)
            {
                var c = text[i];
                if (char.IsUpper(c))
                {
                    if (i > 0)
                    {
                        result.Append('_');
                    }
                    result.Append(char.ToLowerInvariant(c));
                }
                else
                {
                    result.Append(c);
                }
            }
            return result.ToString();
        }

        /// <summary>
        /// 转换为短横线命名
        /// </summary>
        /// <param name="text">文本</param>
        /// <returns>短横线命名</returns>
        public static string ToKebabCase(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            var result = new StringBuilder();
            for (int i = 0; i < text.Length; i++)
            {
                var c = text[i];
                if (char.IsUpper(c))
                {
                    if (i > 0)
                    {
                        result.Append('-');
                    }
                    result.Append(char.ToLowerInvariant(c));
                }
                else
                {
                    result.Append(c);
                }
            }
            return result.ToString();
        }

        #endregion

        #region 其他清理

        /// <summary>
        /// 移除重复字符
        /// </summary>
        /// <param name="text">文本</param>
        /// <param name="chars">要移除重复的字符</param>
        /// <returns>处理后的文本</returns>
        public static string RemoveDuplicateChars(string text, params char[] chars)
        {
            if (string.IsNullOrEmpty(text) || chars.Length == 0)
                return text ?? string.Empty;

            var result = text;
            foreach (var c in chars)
            {
                var pattern = $"{Regex.Escape(c.ToString())}{{2,}}";
                result = Regex.Replace(result, pattern, c.ToString());
            }
            return result;
        }

        /// <summary>
        /// 仅保留数字
        /// </summary>
        /// <param name="text">文本</param>
        /// <returns>数字字符串</returns>
        public static string KeepOnlyDigits(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            return Regex.Replace(text, @"[^\d]", "");
        }

        /// <summary>
        /// 仅保留字母
        /// </summary>
        /// <param name="text">文本</param>
        /// <returns>字母字符串</returns>
        public static string KeepOnlyLetters(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            return Regex.Replace(text, @"[^a-zA-Z]", "");
        }

        /// <summary>
        /// 仅保留字母和数字
        /// </summary>
        /// <param name="text">文本</param>
        /// <returns>字母数字字符串</returns>
        public static string KeepOnlyAlphanumeric(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            return Regex.Replace(text, @"[^a-zA-Z0-9]", "");
        }

        /// <summary>
        /// 清理文件名（移除非法字符）
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <returns>合法文件名</returns>
        public static string CleanFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return string.Empty;

            var invalidChars = new string(System.IO.Path.GetInvalidFileNameChars());
            var pattern = $"[{Regex.Escape(invalidChars)}]";
            return Regex.Replace(fileName, pattern, "_");
        }

        /// <summary>
        /// 清理路径（移除非法字符）
        /// </summary>
        /// <param name="path">路径</param>
        /// <returns>合法路径</returns>
        public static string CleanPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return string.Empty;

            var invalidChars = new string(System.IO.Path.GetInvalidPathChars());
            var pattern = $"[{Regex.Escape(invalidChars)}]";
            return Regex.Replace(path, pattern, "_");
        }

        /// <summary>
        /// 综合清理
        /// </summary>
        /// <param name="text">文本</param>
        /// <returns>清理后的文本</returns>
        public static string Clean(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            // 1. 移除 HTML 标签
            var result = RemoveHtmlTags(text);

            // 2. 移除控制字符
            result = RemoveControlChars(result);

            // 3. 规范化空白符
            result = NormalizeWhitespace(result);

            // 4. 移除多余的空行
            result = RemoveEmptyLines(result);

            return result;
        }

        #endregion
    }

    /// <summary>
    /// 行尾符类型
    /// </summary>
    public enum LineEnding
    {
        /// <summary>
        /// Windows 风格 (CRLF)
        /// </summary>
        CRLF,

        /// <summary>
        /// Unix/Linux 风格 (LF)
        /// </summary>
        LF,

        /// <summary>
        /// Mac 风格 (CR)
        /// </summary>
        CR
    }
}