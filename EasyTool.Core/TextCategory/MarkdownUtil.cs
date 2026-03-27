using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace EasyTool.TextCategory
{
    /// <summary>
    /// Markdown工具类
    /// 提供Markdown解析和转换功能
    /// </summary>
    public static class MarkdownUtil
    {
        /// <summary>
        /// Markdown转HTML
        /// </summary>
        public static string ToHtml(string markdown)
        {
            if (string.IsNullOrEmpty(markdown))
                return string.Empty;

            var html = markdown;

            // 转义HTML特殊字符
            html = EscapeHtml(html);

            // 标题
            html = Regex.Replace(html, @"^###### (.+)$", "<h6>$1</h6>", RegexOptions.Multiline);
            html = Regex.Replace(html, @"^##### (.+)$", "<h5>$1</h5>", RegexOptions.Multiline);
            html = Regex.Replace(html, @"^#### (.+)$", "<h4>$1</h4>", RegexOptions.Multiline);
            html = Regex.Replace(html, @"^### (.+)$", "<h3>$1</h3>", RegexOptions.Multiline);
            html = Regex.Replace(html, @"^## (.+)$", "<h2>$1</h2>", RegexOptions.Multiline);
            html = Regex.Replace(html, @"^# (.+)$", "<h1>$1</h1>", RegexOptions.Multiline);

            // 代码块
            html = Regex.Replace(html, @"```(\w*)\n([\s\S]*?)```", "<pre><code class=\"language-$1\">$2</code></pre>");
            html = Regex.Replace(html, @"`([^`]+)`", "<code>$1</code>");

            // 粗体和斜体
            html = Regex.Replace(html, @"\*\*\*(.+?)\*\*\*", "<strong><em>$1</em></strong>");
            html = Regex.Replace(html, @"\*\*(.+?)\*\*", "<strong>$1</strong>");
            html = Regex.Replace(html, @"\*(.+?)\*", "<em>$1</em>");
            html = Regex.Replace(html, @"___(.+?)___", "<strong><em>$1</em></strong>");
            html = Regex.Replace(html, @"__(.+?)__", "<strong>$1</strong>");
            html = Regex.Replace(html, @"_(.+?)_", "<em>$1</em>");
            html = Regex.Replace(html, @"~~(.+?)~~", "<del>$1</del>");

            // 链接和图片
            html = Regex.Replace(html, @"!\[([^\]]*)\]\(([^)]+)\)", "<img src=\"$2\" alt=\"$1\">");
            html = Regex.Replace(html, @"\[([^\]]+)\]\(([^)]+)\)", "<a href=\"$2\">$1</a>");

            // 引用
            html = Regex.Replace(html, @"^> (.+)$", "<blockquote>$1</blockquote>", RegexOptions.Multiline);

            // 水平线
            html = Regex.Replace(html, @"^---$", "<hr>", RegexOptions.Multiline);
            html = Regex.Replace(html, @"^\*\*\*$", "<hr>", RegexOptions.Multiline);
            html = Regex.Replace(html, @"^___$", "<hr>", RegexOptions.Multiline);

            // 无序列表
            html = ProcessUnorderedList(html);

            // 有序列表
            html = ProcessOrderedList(html);

            // 表格
            html = ProcessTable(html);

            // 段落
            html = ProcessParagraphs(html);

            return html;
        }

        /// <summary>
        /// HTML转Markdown（基础实现）
        /// </summary>
        public static string FromHtml(string html)
        {
            if (string.IsNullOrEmpty(html))
                return string.Empty;

            var markdown = html;

            // 标题
            markdown = Regex.Replace(markdown, @"<h1[^>]*>(.*?)</h1>", "# $1", RegexOptions.IgnoreCase);
            markdown = Regex.Replace(markdown, @"<h2[^>]*>(.*?)</h2>", "## $1", RegexOptions.IgnoreCase);
            markdown = Regex.Replace(markdown, @"<h3[^>]*>(.*?)</h3>", "### $1", RegexOptions.IgnoreCase);
            markdown = Regex.Replace(markdown, @"<h4[^>]*>(.*?)</h4>", "#### $1", RegexOptions.IgnoreCase);
            markdown = Regex.Replace(markdown, @"<h5[^>]*>(.*?)</h5>", "##### $1", RegexOptions.IgnoreCase);
            markdown = Regex.Replace(markdown, @"<h6[^>]*>(.*?)</h6>", "###### $1", RegexOptions.IgnoreCase);

            // 链接
            markdown = Regex.Replace(markdown, @"<a[^>]*href=""([^""]+)""[^>]*>(.*?)</a>", "[$2]($1)", RegexOptions.IgnoreCase);

            // 图片
            markdown = Regex.Replace(markdown, @"<img[^>]*src=""([^""]+)""[^>]*alt=""([^""]*)""[^>]*>", "![$2]($1)", RegexOptions.IgnoreCase);

            // 粗体和斜体
            markdown = Regex.Replace(markdown, @"<strong[^>]*>(.*?)</strong>", "**$1**", RegexOptions.IgnoreCase);
            markdown = Regex.Replace(markdown, @"<b[^>]*>(.*?)</b>", "**$1**", RegexOptions.IgnoreCase);
            markdown = Regex.Replace(markdown, @"<em[^>]*>(.*?)</em>", "*$1*", RegexOptions.IgnoreCase);
            markdown = Regex.Replace(markdown, @"<i[^>]*>(.*?)</i>", "*$1*", RegexOptions.IgnoreCase);

            // 代码
            markdown = Regex.Replace(markdown, @"<code[^>]*>(.*?)</code>", "`$1`", RegexOptions.IgnoreCase);
            markdown = Regex.Replace(markdown, @"<pre[^>]*><code[^>]*>([\s\S]*?)</code></pre>", "```\n$1\n```", RegexOptions.IgnoreCase);

            // 引用
            markdown = Regex.Replace(markdown, @"<blockquote[^>]*>(.*?)</blockquote>", "> $1", RegexOptions.IgnoreCase);

            // 列表
            markdown = Regex.Replace(markdown, @"<li[^>]*>(.*?)</li>", "- $1", RegexOptions.IgnoreCase);
            markdown = Regex.Replace(markdown, @"<ul[^>]*>([\s\S]*?)</ul>", "$1", RegexOptions.IgnoreCase);
            markdown = Regex.Replace(markdown, @"<ol[^>]*>([\s\S]*?)</ol>", "$1", RegexOptions.IgnoreCase);

            // 段落和换行
            markdown = Regex.Replace(markdown, @"<br\s*/?>", "\n", RegexOptions.IgnoreCase);
            markdown = Regex.Replace(markdown, @"<p[^>]*>(.*?)</p>", "$1\n\n", RegexOptions.IgnoreCase);

            // 清理其他标签
            markdown = Regex.Replace(markdown, @"<[^>]+>", "");

            // 解码HTML实体
            markdown = UnescapeHtml(markdown);

            return markdown.Trim();
        }

        /// <summary>
        /// 提取Markdown标题
        /// </summary>
        public static List<MarkdownHeading> ExtractHeadings(string markdown)
        {
            var headings = new List<MarkdownHeading>();

            if (string.IsNullOrEmpty(markdown))
                return headings;

            var lines = markdown.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                var match = Regex.Match(line, @"^(#{1,6})\s+(.+)$");
                if (match.Success)
                {
                    headings.Add(new MarkdownHeading
                    {
                        Level = match.Groups[1].Value.Length,
                        Text = match.Groups[2].Value.Trim(),
                        LineNumber = i + 1
                    });
                }
            }

            return headings;
        }

        /// <summary>
        /// 提取Markdown中的所有链接
        /// </summary>
        public static List<MarkdownLink> ExtractLinks(string markdown)
        {
            var links = new List<MarkdownLink>();

            if (string.IsNullOrEmpty(markdown))
                return links;

            var regex = new Regex(@"\[([^\]]+)\]\(([^)]+)\)");
            var matches = regex.Matches(markdown);

            foreach (Match match in matches)
            {
                links.Add(new MarkdownLink
                {
                    Text = match.Groups[1].Value,
                    Url = match.Groups[2].Value
                });
            }

            return links;
        }

        /// <summary>
        /// 提取Markdown中的所有图片
        /// </summary>
        public static List<MarkdownImage> ExtractImages(string markdown)
        {
            var images = new List<MarkdownImage>();

            if (string.IsNullOrEmpty(markdown))
                return images;

            var regex = new Regex(@"!\[([^\]]*)\]\(([^)]+)\)");
            var matches = regex.Matches(markdown);

            foreach (Match match in matches)
            {
                images.Add(new MarkdownImage
                {
                    Alt = match.Groups[1].Value,
                    Url = match.Groups[2].Value
                });
            }

            return images;
        }

        /// <summary>
        /// 生成目录（TOC）
        /// </summary>
        public static string GenerateToc(string markdown)
        {
            var headings = ExtractHeadings(markdown);
            var toc = new StringBuilder();

            foreach (var heading in headings)
            {
                var indent = new string(' ', (heading.Level - 1) * 2);
                var anchor = GenerateAnchor(heading.Text);
                toc.AppendLine($"{indent}- [{heading.Text}](#{anchor})");
            }

            return toc.ToString();
        }

        /// <summary>
        /// 简化Markdown（移除格式）
        /// </summary>
        public static string StripFormatting(string markdown)
        {
            if (string.IsNullOrEmpty(markdown))
                return string.Empty;

            var text = markdown;

            // 移除代码块
            text = Regex.Replace(text, @"```\w*\n[\s\S]*?```", "");
            text = Regex.Replace(text, @"`([^`]+)`", "$1");

            // 移除标题标记
            text = Regex.Replace(text, @"^#{1,6}\s+", "", RegexOptions.Multiline);

            // 移除粗体、斜体、删除线
            text = Regex.Replace(text, @"\*\*\*(.+?)\*\*\*", "$1");
            text = Regex.Replace(text, @"\*\*(.+?)\*\*", "$1");
            text = Regex.Replace(text, @"\*(.+?)\*", "$1");
            text = Regex.Replace(text, @"~~(.+?)~~", "$1");

            // 移除链接，保留文本
            text = Regex.Replace(text, @"!\[([^\]]*)\]\([^)]+\)", "$1");
            text = Regex.Replace(text, @"\[([^\]]+)\]\([^)]+\)", "$1");

            // 移除引用标记
            text = Regex.Replace(text, @"^>\s+", "", RegexOptions.Multiline);

            // 移除列表标记
            text = Regex.Replace(text, @"^[\*\-\+]\s+", "", RegexOptions.Multiline);
            text = Regex.Replace(text, @"^\d+\.\s+", "", RegexOptions.Multiline);

            return text.Trim();
        }

        private static string EscapeHtml(string text)
        {
            return text
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;");
        }

        private static string UnescapeHtml(string text)
        {
            return text
                .Replace("&amp;", "&")
                .Replace("&lt;", "<")
                .Replace("&gt;", ">")
                .Replace("&quot;", "\"")
                .Replace("&#39;", "'");
        }

        private static string ProcessUnorderedList(string html)
        {
            var lines = html.Split('\n');
            var result = new StringBuilder();
            var inList = false;

            foreach (var line in lines)
            {
                var match = Regex.Match(line, @"^[\*\-\+]\s+(.+)$");
                if (match.Success)
                {
                    if (!inList)
                    {
                        result.AppendLine("<ul>");
                        inList = true;
                    }
                    result.AppendLine($"<li>{match.Groups[1].Value}</li>");
                }
                else
                {
                    if (inList)
                    {
                        result.AppendLine("</ul>");
                        inList = false;
                    }
                    result.AppendLine(line);
                }
            }

            if (inList)
                result.AppendLine("</ul>");

            return result.ToString();
        }

        private static string ProcessOrderedList(string html)
        {
            var lines = html.Split('\n');
            var result = new StringBuilder();
            var inList = false;

            foreach (var line in lines)
            {
                var match = Regex.Match(line, @"^\d+\.\s+(.+)$");
                if (match.Success)
                {
                    if (!inList)
                    {
                        result.AppendLine("<ol>");
                        inList = true;
                    }
                    result.AppendLine($"<li>{match.Groups[1].Value}</li>");
                }
                else
                {
                    if (inList)
                    {
                        result.AppendLine("</ol>");
                        inList = false;
                    }
                    result.AppendLine(line);
                }
            }

            if (inList)
                result.AppendLine("</ol>");

            return result.ToString();
        }

        private static string ProcessTable(string html)
        {
            // 简单的表格处理
            var lines = html.Split('\n');
            var result = new StringBuilder();
            var inTable = false;

            foreach (var line in lines)
            {
                if (line.Trim().StartsWith("|") && line.Trim().EndsWith("|"))
                {
                    if (!inTable)
                    {
                        result.AppendLine("<table>");
                        inTable = true;
                    }

                    var cells = line.Trim('|').Split('|');
                    var isHeader = line.Contains("---");

                    if (!isHeader)
                    {
                        result.AppendLine("<tr>");
                        foreach (var cell in cells)
                        {
                            result.AppendLine($"<td>{cell.Trim()}</td>");
                        }
                        result.AppendLine("</tr>");
                    }
                }
                else
                {
                    if (inTable)
                    {
                        result.AppendLine("</table>");
                        inTable = false;
                    }
                    result.AppendLine(line);
                }
            }

            return result.ToString();
        }

        private static string ProcessParagraphs(string html)
        {
            var lines = html.Split(new[] { "\n\n" }, StringSplitOptions.None);
            var result = new StringBuilder();

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (!string.IsNullOrEmpty(trimmed) &&
                    !trimmed.StartsWith("<h") &&
                    !trimmed.StartsWith("<ul") &&
                    !trimmed.StartsWith("<ol") &&
                    !trimmed.StartsWith("<table") &&
                    !trimmed.StartsWith("<blockquote") &&
                    !trimmed.StartsWith("<pre") &&
                    !trimmed.StartsWith("<hr"))
                {
                    result.AppendLine($"<p>{trimmed}</p>");
                }
                else
                {
                    result.AppendLine(trimmed);
                }
            }

            return result.ToString();
        }

        private static string GenerateAnchor(string text)
        {
            var anchor = text.ToLower();
            anchor = Regex.Replace(anchor, @"[^\w\s-]", "");
            anchor = Regex.Replace(anchor, @"\s+", "-");
            return anchor;
        }
    }

    /// <summary>
    /// Markdown标题
    /// </summary>
    public class MarkdownHeading
    {
        /// <summary>
        /// 标题级别（1-6）
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// 标题文本
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// 行号
        /// </summary>
        public int LineNumber { get; set; }
    }

    /// <summary>
    /// Markdown链接
    /// </summary>
    public class MarkdownLink
    {
        /// <summary>
        /// 链接文本
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// 链接URL
        /// </summary>
        public string Url { get; set; } = string.Empty;
    }

    /// <summary>
    /// Markdown图片
    /// </summary>
    public class MarkdownImage
    {
        /// <summary>
        /// 替代文本
        /// </summary>
        public string Alt { get; set; } = string.Empty;

        /// <summary>
        /// 图片URL
        /// </summary>
        public string Url { get; set; } = string.Empty;
    }
}
