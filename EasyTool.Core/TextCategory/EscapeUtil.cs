using System;
using System.Text;
using System.Web;

namespace EasyTool
{
    /// <summary>
    /// 转义工具类
    /// 对标 Hutool 的 EscapeUtil
    /// 提供HTML、URL、XML、JSON等转义和反转义
    /// </summary>
    public static class EscapeUtil
    {
        #region HTML 转义

        /// <summary>
        /// HTML 转义
        /// </summary>
        /// <param name="html">HTML字符串</param>
        /// <returns>转义后的字符串</returns>
        public static string EscapeHtml(string? html)
        {
            if (string.IsNullOrEmpty(html))
                return string.Empty;

            return HttpUtility.HtmlEncode(html);
        }

        /// <summary>
        /// HTML 反转义
        /// </summary>
        /// <param name="html">已转义的HTML字符串</param>
        /// <returns>反转义后的字符串</returns>
        public static string UnescapeHtml(string? html)
        {
            if (string.IsNullOrEmpty(html))
                return string.Empty;

            return HttpUtility.HtmlDecode(html);
        }

        #endregion

        #region URL 转义

        /// <summary>
        /// URL 编码
        /// </summary>
        /// <param name="url">URL字符串</param>
        /// <returns>编码后的字符串</returns>
        public static string EscapeUrl(string? url)
        {
            if (string.IsNullOrEmpty(url))
                return string.Empty;

            return Uri.EscapeDataString(url);
        }

        /// <summary>
        /// URL 编码（使用 UTF-8）
        /// </summary>
        /// <param name="url">URL字符串</param>
        /// <returns>编码后的字符串</returns>
        public static string EscapeUrl(string? url, Encoding encoding)
        {
            if (string.IsNullOrEmpty(url))
                return string.Empty;

            return HttpUtility.UrlEncode(url, encoding ?? Encoding.UTF8) ?? string.Empty;
        }

        /// <summary>
        /// URL 解码
        /// </summary>
        /// <param name="url">已编码的URL字符串</param>
        /// <returns>解码后的字符串</returns>
        public static string UnescapeUrl(string? url)
        {
            if (string.IsNullOrEmpty(url))
                return string.Empty;

            return Uri.UnescapeDataString(url);
        }

        /// <summary>
        /// URL 解码（使用指定编码）
        /// </summary>
        /// <param name="url">已编码的URL字符串</param>
        /// <param name="encoding">编码</param>
        /// <returns>解码后的字符串</returns>
        public static string UnescapeUrl(string? url, Encoding encoding)
        {
            if (string.IsNullOrEmpty(url))
                return string.Empty;

            return HttpUtility.UrlDecode(url, encoding ?? Encoding.UTF8) ?? string.Empty;
        }

        #endregion

        #region XML 转义

        /// <summary>
        /// XML 转义
        /// </summary>
        /// <param name="xml">XML字符串</param>
        /// <returns>转义后的字符串</returns>
        public static string EscapeXml(string? xml)
        {
            if (string.IsNullOrEmpty(xml))
                return string.Empty;

            var sb = new StringBuilder();
            foreach (char c in xml)
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
                        sb.Append("&apos;");
                        break;
                    default:
                        sb.Append(c);
                        break;
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// XML 反转义
        /// </summary>
        /// <param name="xml">已转义的XML字符串</param>
        /// <returns>反转义后的字符串</returns>
        public static string UnescapeXml(string? xml)
        {
            if (string.IsNullOrEmpty(xml))
                return string.Empty;

            return xml
                .Replace("&lt;", "<")
                .Replace("&gt;", ">")
                .Replace("&amp;", "&")
                .Replace("&quot;", "\"")
                .Replace("&apos;", "'");
        }

        #endregion

        #region JSON 转义

        /// <summary>
        /// JSON 字符串转义
        /// </summary>
        /// <param name="json">JSON字符串</param>
        /// <returns>转义后的字符串</returns>
        public static string EscapeJson(string? json)
        {
            if (string.IsNullOrEmpty(json))
                return string.Empty;

            var sb = new StringBuilder();
            foreach (char c in json)
            {
                switch (c)
                {
                    case '"':
                        sb.Append("\\\"");
                        break;
                    case '\\':
                        sb.Append("\\\\");
                        break;
                    case '/':
                        sb.Append("\\/");
                        break;
                    case '\b':
                        sb.Append("\\b");
                        break;
                    case '\f':
                        sb.Append("\\f");
                        break;
                    case '\n':
                        sb.Append("\\n");
                        break;
                    case '\r':
                        sb.Append("\\r");
                        break;
                    case '\t':
                        sb.Append("\\t");
                        break;
                    default:
                        if (char.IsControl(c))
                        {
                            sb.Append("\\u");
                            sb.Append(((int)c).ToString("x4"));
                        }
                        else
                        {
                            sb.Append(c);
                        }
                        break;
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// JSON 字符串反转义
        /// </summary>
        /// <param name="json">已转义的JSON字符串</param>
        /// <returns>反转义后的字符串</returns>
        public static string UnescapeJson(string? json)
        {
            if (string.IsNullOrEmpty(json))
                return string.Empty;

            var sb = new StringBuilder();
            int i = 0;

            while (i < json.Length)
            {
                if (json[i] == '\\' && i + 1 < json.Length)
                {
                    char next = json[i + 1];
                    switch (next)
                    {
                        case '"':
                            sb.Append('"');
                            i += 2;
                            break;
                        case '\\':
                            sb.Append('\\');
                            i += 2;
                            break;
                        case '/':
                            sb.Append('/');
                            i += 2;
                            break;
                        case 'b':
                            sb.Append('\b');
                            i += 2;
                            break;
                        case 'f':
                            sb.Append('\f');
                            i += 2;
                            break;
                        case 'n':
                            sb.Append('\n');
                            i += 2;
                            break;
                        case 'r':
                            sb.Append('\r');
                            i += 2;
                            break;
                        case 't':
                            sb.Append('\t');
                            i += 2;
                            break;
                        case 'u':
                            if (i + 5 < json.Length)
                            {
                                string hex = json.Substring(i + 2, 4);
                                if (int.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out int code))
                                {
                                    sb.Append((char)code);
                                    i += 6;
                                    break;
                                }
                            }
                            sb.Append('\\');
                            i++;
                            break;
                        default:
                            sb.Append('\\');
                            i++;
                            break;
                    }
                }
                else
                {
                    sb.Append(json[i]);
                    i++;
                }
            }

            return sb.ToString();
        }

        #endregion

        #region Unicode 转义

        /// <summary>
        /// Unicode 转义
        /// </summary>
        /// <param name="text">文本</param>
        /// <returns>转义后的字符串</returns>
        public static string EscapeUnicode(string? text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            var sb = new StringBuilder();
            foreach (char c in text)
            {
                if (c > 127)
                {
                    sb.Append("\\u");
                    sb.Append(((int)c).ToString("x4"));
                }
                else
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Unicode 反转义
        /// </summary>
        /// <param name="text">已转义的文本</param>
        /// <returns>反转义后的字符串</returns>
        public static string UnescapeUnicode(string? text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            var sb = new StringBuilder();
            int i = 0;

            while (i < text.Length)
            {
                if (i + 5 < text.Length && text[i] == '\\' && text[i + 1] == 'u')
                {
                    string hex = text.Substring(i + 2, 4);
                    if (int.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out int code))
                    {
                        sb.Append((char)code);
                        i += 6;
                        continue;
                    }
                }

                sb.Append(text[i]);
                i++;
            }

            return sb.ToString();
        }

        #endregion

        #region Java 风格转义

        /// <summary>
        /// Java 风格字符串转义
        /// </summary>
        /// <param name="text">文本</param>
        /// <returns>转义后的字符串</returns>
        public static string EscapeJava(string? text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            var sb = new StringBuilder();
            foreach (char c in text)
            {
                switch (c)
                {
                    case '\\':
                        sb.Append("\\\\");
                        break;
                    case '"':
                        sb.Append("\\\"");
                        break;
                    case '\'':
                        sb.Append("\\'");
                        break;
                    case '\n':
                        sb.Append("\\n");
                        break;
                    case '\r':
                        sb.Append("\\r");
                        break;
                    case '\t':
                        sb.Append("\\t");
                        break;
                    case '\b':
                        sb.Append("\\b");
                        break;
                    case '\f':
                        sb.Append("\\f");
                        break;
                    default:
                        sb.Append(c);
                        break;
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Java 风格字符串反转义
        /// </summary>
        /// <param name="text">已转义的文本</param>
        /// <returns>反转义后的字符串</returns>
        public static string UnescapeJava(string? text)
        {
            return UnescapeJson(text);
        }

        #endregion

        #region 正则表达式转义

        /// <summary>
        /// 正则表达式特殊字符转义
        /// </summary>
        /// <param name="pattern">正则表达式</param>
        /// <returns>转义后的字符串</returns>
        public static string EscapeRegex(string? pattern)
        {
            if (string.IsNullOrEmpty(pattern))
                return string.Empty;

            return System.Text.RegularExpressions.Regex.Escape(pattern);
        }

        #endregion

        #region SQL 转义

        /// <summary>
        /// SQL 字符串转义（基础防注入）
        /// 注意：这只是一个简单的转义，实际应使用参数化查询
        /// </summary>
        /// <param name="sql">SQL字符串</param>
        /// <returns>转义后的字符串</returns>
        public static string EscapeSql(string? sql)
        {
            if (string.IsNullOrEmpty(sql))
                return string.Empty;

            return sql.Replace("'", "''");
        }

        #endregion
    }
}