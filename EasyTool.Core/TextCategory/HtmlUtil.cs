using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace EasyTool.TextCategory
{
    /// <summary>
    /// HTML 工具类
    /// 提供 HTML 转义、清理、提取等功能
    /// </summary>
    public static class HtmlUtil
    {
        #region 常量

        /// <summary>
        /// HTML 实体编码映射
        /// </summary>
        private static readonly Dictionary<string, string> HtmlEntities = new Dictionary<string, string>
        {
            { "&nbsp;", " " }, { "&lt;", "<" }, { "&gt;", ">" },
            { "&amp;", "&" }, { "&quot;", "\"" }, { "&apos;", "'" },
            { "&copy;", "©" }, { "&reg;", "®" }, { "&trade;", "™" },
            { "&ndash;", "–" }, { "&mdash;", "—" }, { "&lsquo;", "'" },
            { "&rsquo;", "'" }, { "&ldquo;", "\"" }, { "&rdquo;", "\"" },
            { "&bull;", "•" }, { "&hellip;", "…" }, { "&deg;", "°" },
            { "&plusmn;", "±" }, { "&times;", "×" }, { "&divide;", "÷" },
            { "&euro;", "€" }, { "&pound;", "£" }, { "&yen;", "¥" },
            { "&cent;", "¢" }, { "&sect;", "§" }, { "&para;", "¶" },
            { "&dagger;", "†" }, { "&Dagger;", "‡" }, { "&permil;", "‰" },
            { "&laquo;", "«" }, { "&raquo;", "»" }, { "&iexcl;", "¡" },
            { "&iquest;", "¿" }, { "&micro;", "µ" }, { "&middot;", "·" }
        };

        /// <summary>
        /// HTML 标签正则
        /// </summary>
        private static readonly Regex HtmlTagRegex = new Regex(@"<[^>]+>", RegexOptions.Compiled);

        /// <summary>
        /// 脚本标签正则
        /// </summary>
        private static readonly Regex ScriptRegex = new Regex(@"<script[^>]*>[\s\S]*?</script>", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// 样式标签正则
        /// </summary>
        private static readonly Regex StyleRegex = new Regex(@"<style[^>]*>[\s\S]*?</style>", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// HTML 注释正则
        /// </summary>
        private static readonly Regex CommentRegex = new Regex(@"<!--[\s\S]*?-->", RegexOptions.Compiled);

        /// <summary>
        /// 数字实体正则
        /// </summary>
        private static readonly Regex NumericEntityRegex = new Regex(@"&#(\d+);", RegexOptions.Compiled);

        /// <summary>
        /// 十六进制实体正则
        /// </summary>
        private static readonly Regex HexEntityRegex = new Regex(@"&#[xX]([0-9a-fA-F]+);", RegexOptions.Compiled);

        /// <summary>
        /// 安全标签白名单
        /// </summary>
        private static readonly HashSet<string> SafeTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "p", "br", "hr", "h1", "h2", "h3", "h4", "h5", "h6",
            "div", "span", "a", "img", "ul", "ol", "li", "table", "tr", "td", "th", "thead", "tbody",
            "strong", "em", "b", "i", "u", "s", "sub", "sup",
            "blockquote", "pre", "code", "kbd", "samp", "var"
        };

        #endregion

        #region 转义方法

        /// <summary>
        /// HTML 转义
        /// </summary>
        /// <param name="text">原始文本</param>
        /// <returns>转义后的文本</returns>
        public static string Escape(string? text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            var sb = new StringBuilder(text.Length * 2);
            foreach (char c in text)
            {
                switch (c)
                {
                    case '<':
                        sb.Append("&lt;");
                        break;
                    case '>':
                        sb.Append("&gt;");
                        break;
                    case '&':
                        sb.Append("&amp;");
                        break;
                    case '"':
                        sb.Append("&quot;");
                        break;
                    case '\'':
                        sb.Append("&#39;");
                        break;
                    default:
                        sb.Append(c);
                        break;
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// HTML 反转义
        /// </summary>
        /// <param name="html">HTML 文本</param>
        /// <returns>反转义后的文本</returns>
        public static string Unescape(string? html)
        {
            if (string.IsNullOrEmpty(html))
                return string.Empty;

            string result = html;

            // 先处理数字实体
            result = NumericEntityRegex.Replace(result, match =>
            {
                if (int.TryParse(match.Groups[1].Value, out int code))
                {
                    return ((char)code).ToString();
                }
                return match.Value;
            });

            // 处理十六进制实体
            result = HexEntityRegex.Replace(result, match =>
            {
                try
                {
                    int code = Convert.ToInt32(match.Groups[1].Value, 16);
                    return ((char)code).ToString();
                }
                catch
                {
                    return match.Value;
                }
            });

            // 处理命名实体
            foreach (var entity in HtmlEntities)
            {
                result = result.Replace(entity.Key, entity.Value);
            }

            return result;
        }

        /// <summary>
        /// URL 编码
        /// </summary>
        /// <param name="text">原始文本</param>
        /// <param name="encoding">编码方式（默认UTF-8）</param>
        /// <returns>编码后的文本</returns>
        public static string UrlEncode(string? text, Encoding? encoding = null)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            encoding ??= Encoding.UTF8;
            return WebUtility.UrlEncode(text);
        }

        /// <summary>
        /// URL 解码
        /// </summary>
        /// <param name="encodedText">编码的文本</param>
        /// <returns>解码后的文本</returns>
        public static string UrlDecode(string? encodedText)
        {
            if (string.IsNullOrEmpty(encodedText))
                return string.Empty;

            return WebUtility.UrlDecode(encodedText);
        }

        #endregion

        #region 清理方法

        /// <summary>
        /// 移除所有 HTML 标签
        /// </summary>
        /// <param name="html">HTML 文本</param>
        /// <returns>纯文本</returns>
        public static string StripTags(string? html)
        {
            if (string.IsNullOrEmpty(html))
                return string.Empty;

            return HtmlTagRegex.Replace(html, string.Empty);
        }

        /// <summary>
        /// 清理 HTML（移除脚本、样式、注释）
        /// </summary>
        /// <param name="html">HTML 文本</param>
        /// <returns>清理后的 HTML</returns>
        public static string Clean(string? html)
        {
            if (string.IsNullOrEmpty(html))
                return string.Empty;

            string result = html;

            // 移除脚本
            result = ScriptRegex.Replace(result, string.Empty);

            // 移除样式
            result = StyleRegex.Replace(result, string.Empty);

            // 移除注释
            result = CommentRegex.Replace(result, string.Empty);

            return result;
        }

        /// <summary>
        /// 清理 HTML 并保留安全标签
        /// </summary>
        /// <param name="html">HTML 文本</param>
        /// <param name="allowedTags">允许的标签（为空则使用默认白名单）</param>
        /// <returns>清理后的 HTML</returns>
        public static string SafeClean(string? html, IEnumerable<string>? allowedTags = null)
        {
            if (string.IsNullOrEmpty(html))
                return string.Empty;

            // 先进行基本清理
            string result = Clean(html);

            // 获取允许的标签集合
            var allowed = allowedTags != null
                ? new HashSet<string>(allowedTags, StringComparer.OrdinalIgnoreCase)
                : SafeTags;

            // 移除不允许的标签（保留内容）
            result = Regex.Replace(result, @"</?([a-zA-Z][a-zA-Z0-9]*)[^>]*>", match =>
            {
                string tagName = match.Groups[1].Value;
                if (allowed.Contains(tagName))
                {
                    return match.Value;
                }
                return string.Empty;
            }, RegexOptions.IgnoreCase);

            return result;
        }

        /// <summary>
        /// 移除所有 HTML 并获取纯文本
        /// </summary>
        /// <param name="html">HTML 文本</param>
        /// <returns>纯文本</returns>
        public static string ToPlainText(string? html)
        {
            if (string.IsNullOrEmpty(html))
                return string.Empty;

            string result = Clean(html);

            // 处理常见块级元素
            result = Regex.Replace(result, @"<br\s*/?>", "\n", RegexOptions.IgnoreCase);
            result = Regex.Replace(result, @"</p>", "\n", RegexOptions.IgnoreCase);
            result = Regex.Replace(result, @"</div>", "\n", RegexOptions.IgnoreCase);
            result = Regex.Replace(result, @"</li>", "\n", RegexOptions.IgnoreCase);
            result = Regex.Replace(result, @"</tr>", "\n", RegexOptions.IgnoreCase);
            result = Regex.Replace(result, @"</td>", " ", RegexOptions.IgnoreCase);
            result = Regex.Replace(result, @"</th>", " ", RegexOptions.IgnoreCase);

            // 移除剩余标签
            result = StripTags(result);

            // 反转义 HTML 实体
            result = Unescape(result);

            // 清理多余空白
            result = Regex.Replace(result, @"[ \t]+", " ");
            result = Regex.Replace(result, @"\n\s*\n", "\n\n");
            result = result.Trim();

            return result;
        }

        #endregion

        #region 提取方法

        /// <summary>
        /// 提取所有链接
        /// </summary>
        /// <param name="html">HTML 文本</param>
        /// <returns>链接列表（URL, 文本）</returns>
        public static List<(string Url, string Text)> ExtractLinks(string? html)
        {
            var links = new List<(string, string)>();

            if (string.IsNullOrEmpty(html))
                return links;

            var regex = new Regex(@"<a[^>]+href=""([^""]+)""[^>]*>([^<]*)</a>", RegexOptions.IgnoreCase);
            var matches = regex.Matches(html);

            foreach (Match match in matches)
            {
                string url = match.Groups[1].Value;
                string text = Unescape(match.Groups[2].Value).Trim();
                links.Add((url, text));
            }

            return links;
        }

        /// <summary>
        /// 提取所有图片
        /// </summary>
        /// <param name="html">HTML 文本</param>
        /// <returns>图片列表（URL, Alt）</returns>
        public static List<(string Src, string Alt)> ExtractImages(string? html)
        {
            var images = new List<(string, string)>();

            if (string.IsNullOrEmpty(html))
                return images;

            var regex = new Regex(@"<img[^>]+src=""([^""]+)""[^>]*(?:alt=""([^""]*)"")?[^>]*/?>", RegexOptions.IgnoreCase);
            var matches = regex.Matches(html);

            foreach (Match match in matches)
            {
                string src = match.Groups[1].Value;
                string alt = match.Groups[2].Success ? match.Groups[2].Value : string.Empty;
                images.Add((src, alt));
            }

            return images;
        }

        /// <summary>
        /// 提取页面标题
        /// </summary>
        /// <param name="html">HTML 文本</param>
        /// <returns>标题</returns>
        public static string? ExtractTitle(string? html)
        {
            if (string.IsNullOrEmpty(html))
                return null;

            var match = Regex.Match(html, @"<title[^>]*>([^<]*)</title>", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                return Unescape(match.Groups[1].Value).Trim();
            }
            return null;
        }

        /// <summary>
        /// 提取 Meta 标签内容
        /// </summary>
        /// <param name="html">HTML 文本</param>
        /// <param name="name">Meta 名称</param>
        /// <returns>内容</returns>
        public static string? ExtractMeta(string? html, string name)
        {
            if (string.IsNullOrEmpty(html) || string.IsNullOrEmpty(name))
                return null;

            var match = Regex.Match(html, $@"<meta[^>]+name=""{Regex.Escape(name)}""[^>]+content=""([^""]*)""", RegexOptions.IgnoreCase);
            if (!match.Success)
            {
                match = Regex.Match(html, $@"<meta[^>]+content=""([^""]*)""[^>]+name=""{Regex.Escape(name)}""", RegexOptions.IgnoreCase);
            }

            if (match.Success)
            {
                return Unescape(match.Groups[1].Value);
            }
            return null;
        }

        /// <summary>
        /// 提取所有文本内容
        /// </summary>
        /// <param name="html">HTML 文本</param>
        /// <param name="selector">CSS 选择器（简化版，仅支持标签名）</param>
        /// <returns>匹配的文本列表</returns>
        public static List<string> ExtractTexts(string? html, string? selector = null)
        {
            var texts = new List<string>();

            if (string.IsNullOrEmpty(html))
                return texts;

            if (string.IsNullOrEmpty(selector))
            {
                texts.Add(ToPlainText(html));
                return texts;
            }

            var regex = new Regex($@"<{Regex.Escape(selector)}[^>]*>([\s\S]*?)</{Regex.Escape(selector)}>", RegexOptions.IgnoreCase);
            var matches = regex.Matches(html);

            foreach (Match match in matches)
            {
                string text = ToPlainText(match.Groups[1].Value);
                if (!string.IsNullOrWhiteSpace(text))
                {
                    texts.Add(text);
                }
            }

            return texts;
        }

        #endregion

        #region 格式化方法

        /// <summary>
        /// 压缩 HTML（移除多余空白）
        /// </summary>
        /// <param name="html">HTML 文本</param>
        /// <returns>压缩后的 HTML</returns>
        public static string Minify(string? html)
        {
            if (string.IsNullOrEmpty(html))
                return string.Empty;

            string result = html;

            // 移除注释
            result = CommentRegex.Replace(result, string.Empty);

            // 移除多余空白
            result = Regex.Replace(result, @">\s+<", "><");
            result = Regex.Replace(result, @"\s+", " ");
            result = result.Trim();

            return result;
        }

        /// <summary>
        /// 格式化 HTML（添加缩进）
        /// </summary>
        /// <param name="html">HTML 文本</param>
        /// <param name="indentString">缩进字符串（默认两个空格）</param>
        /// <returns>格式化后的 HTML</returns>
        public static string Format(string? html, string indentString = "  ")
        {
            if (string.IsNullOrEmpty(html))
                return string.Empty;

            var result = new StringBuilder();
            int indent = 0;
            bool inPre = false;

            var tokens = Regex.Split(html, @"(<[^>]+>)");

            foreach (var token in tokens)
            {
                if (string.IsNullOrEmpty(token))
                    continue;

                // 检测 pre 标签
                if (Regex.IsMatch(token, @"<pre[^>]*>", RegexOptions.IgnoreCase))
                {
                    inPre = true;
                }
                else if (Regex.IsMatch(token, @"</pre>", RegexOptions.IgnoreCase))
                {
                    inPre = false;
                }

                if (inPre)
                {
                    result.Append(token);
                    continue;
                }

                string trimmed = token.Trim();

                // 自闭合标签或文本
                if (trimmed.StartsWith("<") && !trimmed.StartsWith("</") && !trimmed.EndsWith("/>"))
                {
                    // 开始标签
                    if (!IsInlineTag(trimmed))
                    {
                        result.AppendLine();
                        result.Append(string.Concat(Enumerable.Repeat(indentString, indent)));
                        indent++;
                    }
                    result.Append(trimmed);
                }
                else if (trimmed.StartsWith("</"))
                {
                    // 结束标签
                    if (!IsInlineTag(trimmed))
                    {
                        indent = Math.Max(0, indent - 1);
                        result.AppendLine();
                        result.Append(string.Concat(Enumerable.Repeat(indentString, indent)));
                    }
                    result.Append(trimmed);
                }
                else if (!string.IsNullOrWhiteSpace(trimmed))
                {
                    // 文本内容
                    result.Append(trimmed);
                }
            }

            return result.ToString().Trim();
        }

        private static bool IsInlineTag(string tag)
        {
            var match = Regex.Match(tag, @"</?([a-zA-Z][a-zA-Z0-9]*)");
            if (match.Success)
            {
                string tagName = match.Groups[1].Value.ToLower();
                return tagName == "span" || tagName == "a" || tagName == "strong" ||
                       tagName == "em" || tagName == "b" || tagName == "i" ||
                       tagName == "u" || tagName == "s" || tagName == "sub" ||
                       tagName == "sup" || tagName == "code" || tagName == "img";
            }
            return false;
        }

        #endregion

        #region 验证方法

        /// <summary>
        /// 检查是否为有效的 HTML 片段
        /// </summary>
        /// <param name="html">HTML 文本</param>
        /// <returns>是否有效</returns>
        public static bool IsValidHtml(string? html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return false;

            // 检查基本 HTML 标签
            return HtmlTagRegex.IsMatch(html);
        }

        /// <summary>
        /// 检查 HTML 标签是否匹配
        /// </summary>
        /// <param name="html">HTML 文本</param>
        /// <returns>是否匹配</returns>
        public static bool AreTagsBalanced(string? html)
        {
            if (string.IsNullOrEmpty(html))
                return true;

            var stack = new Stack<string>();
            var selfClosing = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "br", "hr", "img", "input", "meta", "link", "area", "base", "col",
                "embed", "param", "source", "track", "wbr"
            };

            var matches = Regex.Matches(html, @"</?([a-zA-Z][a-zA-Z0-9]*)[^>]*>", RegexOptions.IgnoreCase);

            foreach (Match match in matches)
            {
                string tagName = match.Groups[1].Value.ToLower();

                if (selfClosing.Contains(tagName))
                    continue;

                if (match.Value.StartsWith("</"))
                {
                    // 结束标签
                    if (stack.Count == 0 || stack.Pop() != tagName)
                        return false;
                }
                else if (!match.Value.EndsWith("/>"))
                {
                    // 开始标签
                    stack.Push(tagName);
                }
            }

            return stack.Count == 0;
        }

        #endregion
    }
}
