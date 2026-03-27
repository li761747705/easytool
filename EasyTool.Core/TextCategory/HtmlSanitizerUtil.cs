using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace EasyTool.TextCategory
{
    /// <summary>
    /// HTML 清理工具类
    /// 提供安全的 HTML 过滤和清理功能
    /// </summary>
    public static class HtmlSanitizerUtil
    {
        // 允许的安全标签
        private static readonly HashSet<string> SafeTags = new(StringComparer.OrdinalIgnoreCase)
        {
            "a", "abbr", "acronym", "address", "area", "article", "aside", "b", "bdi", "bdo",
            "blockquote", "br", "caption", "center", "cite", "code", "col", "colgroup", "dd",
            "details", "div", "dl", "dt", "em", "figcaption", "figure", "font", "footer", "h1",
            "h2", "h3", "h4", "h5", "h6", "header", "hr", "i", "img", "li", "main", "mark",
            "nav", "ol", "p", "pre", "q", "s", "section", "small", "span", "strike", "strong",
            "sub", "summary", "sup", "table", "tbody", "td", "tfoot", "th", "thead", "tr",
            "tt", "u", "ul", "var", "wbr"
        };

        // 允许的安全属性
        private static readonly Dictionary<string, HashSet<string>> SafeAttributes = new(StringComparer.OrdinalIgnoreCase)
        {
            ["*"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "class", "id", "title", "lang", "dir" },
            ["a"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "href", "target", "rel", "name" },
            ["img"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "src", "alt", "width", "height", "loading" },
            ["font"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "color", "size", "face" },
            ["table"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "border", "cellpadding", "cellspacing", "width" },
            ["td"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "colspan", "rowspan", "width", "height", "align", "valign" },
            ["th"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "colspan", "rowspan", "width", "height", "align", "valign" },
            ["col"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "span", "width" },
            ["colgroup"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "span" },
            ["ol"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "start", "type" },
            ["ul"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "type" },
            ["li"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "value" },
            ["blockquote"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "cite" },
            ["q"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "cite" }
        };

        // 危险属性模式
        private static readonly Regex DangerousAttributePattern = new Regex(
            @"^\s*(on\w+|data-[^d]|formaction|xlink:href)\s*$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // 危险 URL 协议
        private static readonly Regex DangerousUrlPattern = new Regex(
            @"^\s*(javascript|vbscript|data(?!:image/))\s*:",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// 清理 HTML，移除危险标签和属性
        /// </summary>
        /// <param name="html">原始 HTML</param>
        /// <param name="allowTags">允许的标签（可选，默认使用安全列表）</param>
        /// <param name="allowAttributes">允许的属性（可选）</param>
        /// <returns>清理后的安全 HTML</returns>
        public static string Sanitize(string html, IEnumerable<string>? allowTags = null, IDictionary<string, IEnumerable<string>>? allowAttributes = null)
        {
            if (string.IsNullOrEmpty(html))
                return string.Empty;

            var safeTags = allowTags != null
                ? new HashSet<string>(allowTags, StringComparer.OrdinalIgnoreCase)
                : SafeTags;

            var safeAttributes = allowAttributes?.ToDictionary(
                k => k.Key,
                v => new HashSet<string>(v.Value, StringComparer.OrdinalIgnoreCase),
                StringComparer.OrdinalIgnoreCase) ?? SafeAttributes;

            return ProcessHtml(html, safeTags, safeAttributes);
        }

        /// <summary>
        /// 移除所有 HTML 标签，只保留文本
        /// </summary>
        /// <param name="html">原始 HTML</param>
        /// <returns>纯文本</returns>
        public static string StripTags(string html)
        {
            if (string.IsNullOrEmpty(html))
                return string.Empty;

            // 移除脚本和样式
            html = Regex.Replace(html, @"<script[^>]*>.*?</script>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            html = Regex.Replace(html, @"<style[^>]*>.*?</style>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            // 移除所有标签
            html = Regex.Replace(html, @"<[^>]+>", "");

            // 解码 HTML 实体
            html = System.Net.WebUtility.HtmlDecode(html);

            // 清理多余空白
            html = Regex.Replace(html, @"\s+", " ");

            return html.Trim();
        }

        /// <summary>
        /// 转义 HTML 特殊字符
        /// </summary>
        /// <param name="text">原始文本</param>
        /// <returns>转义后的 HTML</returns>
        public static string Escape(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            return System.Net.WebUtility.HtmlEncode(text);
        }

        /// <summary>
        /// 反转义 HTML 实体
        /// </summary>
        /// <param name="html">HTML 文本</param>
        /// <returns>解码后的文本</returns>
        public static string Unescape(string html)
        {
            if (string.IsNullOrEmpty(html))
                return string.Empty;

            return System.Net.WebUtility.HtmlDecode(html);
        }

        /// <summary>
        /// 检查 HTML 是否包含潜在危险内容
        /// </summary>
        /// <param name="html">HTML 内容</param>
        /// <returns>是否包含危险内容</returns>
        public static bool ContainsDangerousContent(string html)
        {
            if (string.IsNullOrEmpty(html))
                return false;

            // 检查脚本标签
            if (Regex.IsMatch(html, @"<script", RegexOptions.IgnoreCase))
                return true;

            // 检查事件处理属性
            if (Regex.IsMatch(html, @"\bon\w+\s*=", RegexOptions.IgnoreCase))
                return true;

            // 检查危险 URL 协议
            if (DangerousUrlPattern.IsMatch(html))
                return true;

            // 检查 iframe、embed、object
            if (Regex.IsMatch(html, @"<(iframe|embed|object|applet)", RegexOptions.IgnoreCase))
                return true;

            return false;
        }

        /// <summary>
        /// 提取所有链接
        /// </summary>
        /// <param name="html">HTML 内容</param>
        /// <returns>链接列表</returns>
        public static List<HtmlLink> ExtractLinks(string html)
        {
            var links = new List<HtmlLink>();

            if (string.IsNullOrEmpty(html))
                return links;

            var pattern = @"<a\s+[^>]*href\s*=\s*[""']([^""']+)[""'][^>]*>(.*?)</a>";
            var matches = Regex.Matches(html, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);

            foreach (Match match in matches)
            {
                links.Add(new HtmlLink
                {
                    Url = match.Groups[1].Value,
                    Text = StripTags(match.Groups[2].Value)
                });
            }

            return links;
        }

        /// <summary>
        /// 提取所有图片
        /// </summary>
        /// <param name="html">HTML 内容</param>
        /// <returns>图片列表</returns>
        public static List<HtmlImage> ExtractImages(string html)
        {
            var images = new List<HtmlImage>();

            if (string.IsNullOrEmpty(html))
                return images;

            var pattern = @"<img\s+[^>]*src\s*=\s*[""']([^""']+)[""'][^>]*>";
            var matches = Regex.Matches(html, pattern, RegexOptions.IgnoreCase);

            foreach (Match match in matches)
            {
                var img = new HtmlImage { Src = match.Groups[1].Value };

                // 提取 alt
                var altMatch = Regex.Match(match.Value, @"alt\s*=\s*[""']([^""']*)[""']", RegexOptions.IgnoreCase);
                if (altMatch.Success)
                    img.Alt = altMatch.Groups[1].Value;

                images.Add(img);
            }

            return images;
        }

        private static string ProcessHtml(string html, HashSet<string> safeTags, Dictionary<string, HashSet<string>> safeAttributes)
        {
            var result = new StringBuilder();
            var pos = 0;

            while (pos < html.Length)
            {
                var tagStart = html.IndexOf('<', pos);

                if (tagStart < 0)
                {
                    result.Append(html.Substring(pos));
                    break;
                }

                result.Append(html.Substring(pos, tagStart - pos));

                var tagEnd = html.IndexOf('>', tagStart);
                if (tagEnd < 0)
                {
                    result.Append(html.Substring(tagStart));
                    break;
                }

                var tagContent = html.Substring(tagStart + 1, tagEnd - tagStart - 1);

                // 处理注释
                if (tagContent.StartsWith("!--"))
                {
                    var commentEnd = html.IndexOf("-->", tagStart);
                    if (commentEnd > 0)
                    {
                        pos = commentEnd + 3;
                        continue;
                    }
                }

                // 处理标签
                if (tagContent.StartsWith("/"))
                {
                    // 结束标签
                    var tagName = GetTagName(tagContent.Substring(1));
                    if (safeTags.Contains(tagName))
                    {
                        result.Append($"</{tagName}>");
                    }
                }
                else
                {
                    // 开始标签
                    var tagName = GetTagName(tagContent);
                    if (safeTags.Contains(tagName))
                    {
                        var cleanedAttributes = CleanAttributes(tagName, tagContent, safeAttributes);
                        var selfClosing = tagContent.TrimEnd().EndsWith("/");
                        result.Append(selfClosing
                            ? $"<{tagName}{cleanedAttributes}/>"
                            : $"<{tagName}{cleanedAttributes}>");
                    }
                }

                pos = tagEnd + 1;
            }

            return result.ToString();
        }

        private static string GetTagName(string tagContent)
        {
            var match = Regex.Match(tagContent, @"^(\w+)");
            return match.Success ? match.Groups[1].Value.ToLowerInvariant() : string.Empty;
        }

        private static string CleanAttributes(string tagName, string tagContent, Dictionary<string, HashSet<string>> safeAttributes)
        {
            var safeAttrs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (safeAttributes.TryGetValue("*", out var globalAttrs))
                safeAttrs.UnionWith(globalAttrs);
            if (safeAttributes.TryGetValue(tagName, out var tagAttrs))
                safeAttrs.UnionWith(tagAttrs);

            var result = new StringBuilder();
            var matches = Regex.Matches(tagContent, @"(\w+)\s*=\s*[""']([^""']*)[""']");

            foreach (Match match in matches)
            {
                var attrName = match.Groups[1].Value.ToLowerInvariant();
                var attrValue = match.Groups[2].Value;

                if (!safeAttrs.Contains(attrName))
                    continue;

                if (DangerousAttributePattern.IsMatch(attrName))
                    continue;

                // 检查 URL 属性
                if (attrName == "href" || attrName == "src")
                {
                    if (DangerousUrlPattern.IsMatch(attrValue))
                        continue;
                }

                result.Append($" {attrName}=\"{Escape(attrValue)}\"");
            }

            return result.ToString();
        }
    }

    /// <summary>
    /// HTML 链接
    /// </summary>
    public class HtmlLink
    {
        /// <summary>
        /// URL
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// 链接文本
        /// </summary>
        public string Text { get; set; } = string.Empty;
    }

    /// <summary>
    /// HTML 图片
    /// </summary>
    public class HtmlImage
    {
        /// <summary>
        /// 图片 URL
        /// </summary>
        public string Src { get; set; } = string.Empty;

        /// <summary>
        /// 替代文本
        /// </summary>
        public string Alt { get; set; } = string.Empty;
    }
}
