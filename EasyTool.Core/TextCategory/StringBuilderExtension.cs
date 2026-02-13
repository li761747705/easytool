using System;
using System.IO;
using System.Text;

namespace EasyTool.TextCategory
{
    /// <summary>
    /// StringBuilder 扩展方法
    /// </summary>
    public static class StringBuilderExtension
    {
        #region 追加操作

        /// <summary>
        /// 追加带格式的字符串（忽略 null 值）
        /// </summary>
        public static StringBuilder AppendFormatIfNotNull(this StringBuilder sb, string format, object? arg)
        {
            if (sb == null)
                throw new ArgumentNullException(nameof(sb));

            if (arg != null)
            {
                sb.AppendFormat(format, arg);
            }
            return sb;
        }

        /// <summary>
        /// 追加带格式的字符串（忽略空字符串）
        /// </summary>
        public static StringBuilder AppendFormatIfNotEmpty(this StringBuilder sb, string format, string? arg)
        {
            if (sb == null)
                throw new ArgumentNullException(nameof(sb));

            if (!string.IsNullOrEmpty(arg))
            {
                sb.AppendFormat(format, arg);
            }
            return sb;
        }

        /// <summary>
        /// 追加带格式的字符串（忽略空白字符串）
        /// </summary>
        public static StringBuilder AppendFormatIfNotBlank(this StringBuilder sb, string format, string? arg)
        {
            if (sb == null)
                throw new ArgumentNullException(nameof(sb));

            if (!string.IsNullOrWhiteSpace(arg))
            {
                sb.AppendFormat(format, arg);
            }
            return sb;
        }

        /// <summary>
        /// 条件追加字符串
        /// </summary>
        public static StringBuilder AppendIf(this StringBuilder sb, string? value, bool condition)
        {
            if (sb == null)
                throw new ArgumentNullException(nameof(sb));

            if (condition)
            {
                sb.Append(value);
            }
            return sb;
        }

        /// <summary>
        /// 条件追加字符串（带分隔符）
        /// </summary>
        public static StringBuilder AppendWithSeparator(this StringBuilder sb, string? value, string separator = ", ")
        {
            if (sb == null)
                throw new ArgumentNullException(nameof(sb));

            if (sb.Length > 0 && !string.IsNullOrEmpty(separator))
            {
                sb.Append(separator);
            }
            sb.Append(value);
            return sb;
        }

        /// <summary>
        /// 条件追加字符串（带分隔符，仅在非空时追加）
        /// </summary>
        public static StringBuilder AppendWithSeparatorIfNotEmpty(this StringBuilder sb, string? value, string separator = ", ")
        {
            if (sb == null)
                throw new ArgumentNullException(nameof(sb));

            if (!string.IsNullOrEmpty(value))
            {
                sb.AppendWithSeparator(value, separator);
            }
            return sb;
        }

        /// <summary>
        /// 追加行（仅当值非空时）
        /// </summary>
        public static StringBuilder AppendLineIfNotEmpty(this StringBuilder sb, string? value)
        {
            if (sb == null)
                throw new ArgumentNullException(nameof(sb));

            if (!string.IsNullOrEmpty(value))
            {
                sb.AppendLine(value);
            }
            return sb;
        }

        /// <summary>
        /// 条件追加行
        /// </summary>
        public static StringBuilder AppendLineIf(this StringBuilder sb, string? value, bool condition)
        {
            if (sb == null)
                throw new ArgumentNullException(nameof(sb));

            if (condition)
            {
                sb.AppendLine(value);
            }
            return sb;
        }

        #endregion

        #region 括号包裹

        /// <summary>
        /// 用括号包裹内容（如果非空）
        /// </summary>
        public static StringBuilder AppendInParentheses(this StringBuilder sb, string? value, string open = "(", string close = ")")
        {
            if (sb == null)
                throw new ArgumentNullException(nameof(sb));

            if (!string.IsNullOrEmpty(value))
            {
                sb.Append(open).Append(value).Append(close);
            }
            return sb;
        }

        /// <summary>
        /// 用方括号包裹内容（如果非空）
        /// </summary>
        public static StringBuilder AppendInBrackets(this StringBuilder sb, string? value)
        {
            return sb.AppendInParentheses(value, "[", "]");
        }

        /// <summary>
        /// 用花括号包裹内容（如果非空）
        /// </summary>
        public static StringBuilder AppendInBraces(this StringBuilder sb, string? value)
        {
            return sb.AppendInParentheses(value, "{", "}");
        }

        /// <summary>
        /// 用引号包裹内容（如果非空）
        /// </summary>
        public static StringBuilder AppendInQuotes(this StringBuilder sb, string? value, string quote = "\"")
        {
            return sb.AppendInParentheses(value, quote, quote);
        }

        #endregion

        #region 缩进操作

        /// <summary>
        /// 增加缩进
        /// </summary>
        /// <param name="sb">StringBuilder</param>
        /// <param name="indent">缩进字符串，默认为两个空格</param>
        public static StringBuilder Indent(this StringBuilder sb, string indent = "  ")
        {
            if (sb == null)
                throw new ArgumentNullException(nameof(sb));

            return sb.Append(indent);
        }

        /// <summary>
        /// 增加指定层数的缩进
        /// </summary>
        public static StringBuilder Indent(this StringBuilder sb, int level, string indent = "  ")
        {
            if (sb == null)
                throw new ArgumentNullException(nameof(sb));

            for (int i = 0; i < level; i++)
            {
                sb.Append(indent);
            }
            return sb;
        }

        /// <summary>
        /// 添加缩进行
        /// </summary>
        public static StringBuilder AppendIndentedLine(this StringBuilder sb, string value, int level = 1, string indent = "  ")
        {
            if (sb == null)
                throw new ArgumentNullException(nameof(sb));

            return sb.Indent(level, indent).AppendLine(value);
        }

        #endregion

        #region 清除操作

        /// <summary>
        /// 清除最后 N 个字符
        /// </summary>
        public static StringBuilder RemoveLast(this StringBuilder sb, int count)
        {
            if (sb == null)
                throw new ArgumentNullException(nameof(sb));

            if (count > 0 && sb.Length >= count)
            {
                sb.Remove(sb.Length - count, count);
            }
            return sb;
        }

        /// <summary>
        /// 清除最后的一个字符
        /// </summary>
        public static StringBuilder RemoveLastChar(this StringBuilder sb)
        {
            return sb.RemoveLast(1);
        }

        /// <summary>
        /// 清除最后指定的字符串（如果匹配）
        /// </summary>
        public static StringBuilder RemoveLastIf(this StringBuilder sb, string? value)
        {
            if (sb == null)
                throw new ArgumentNullException(nameof(sb));

            if (!string.IsNullOrEmpty(value) && sb.Length >= value.Length)
            {
                int startIndex = sb.Length - value.Length;
                string end = sb.ToString(startIndex, value.Length);
                if (end == value)
                {
                    sb.Remove(startIndex, value.Length);
                }
            }
            return sb;
        }

        /// <summary>
        /// 清除最后的空白字符
        /// </summary>
        public static StringBuilder TrimEnd(this StringBuilder sb)
        {
            if (sb == null)
                throw new ArgumentNullException(nameof(sb));

            while (sb.Length > 0 && char.IsWhiteSpace(sb[sb.Length - 1]))
            {
                sb.Remove(sb.Length - 1, 1);
            }
            return sb;
        }

        /// <summary>
        /// 清除开头的空白字符
        /// </summary>
        public static StringBuilder TrimStart(this StringBuilder sb)
        {
            if (sb == null)
                throw new ArgumentNullException(nameof(sb));

            int index = 0;
            while (index < sb.Length && char.IsWhiteSpace(sb[index]))
            {
                index++;
            }

            if (index > 0)
            {
                sb.Remove(0, index);
            }
            return sb;
        }

        /// <summary>
        /// 清除开头和结尾的空白字符
        /// </summary>
        public static StringBuilder Trim(this StringBuilder sb)
        {
            return sb.TrimStart().TrimEnd();
        }

        #endregion

        #region 转换操作

        /// <summary>
        /// 转换为 MemoryStream
        /// </summary>
        public static MemoryStream ToMemoryStream(this StringBuilder sb, Encoding? encoding = null)
        {
            if (sb == null)
                throw new ArgumentNullException(nameof(sb));

            encoding ??= Encoding.UTF8;
            var bytes = encoding.GetBytes(sb.ToString());
            return new MemoryStream(bytes);
        }

        #endregion

        #region 检查操作

        /// <summary>
        /// 判断是否为空
        /// </summary>
        public static bool IsNullOrEmpty(this StringBuilder? sb)
        {
            return sb == null || sb.Length == 0;
        }

        /// <summary>
        /// 判断是否包含指定字符串
        /// </summary>
        public static bool Contains(this StringBuilder? sb, string? value)
        {
            if (sb == null)
                return false;

            return sb.IndexOf(value) >= 0;
        }

        /// <summary>
        /// 查找字符串的位置
        /// </summary>
        public static int IndexOf(this StringBuilder? sb, string? value, int startIndex = 0, bool ignoreCase = false)
        {
            if (sb == null || string.IsNullOrEmpty(value))
                return -1;

            if (startIndex < 0 || startIndex >= sb.Length)
                return -1;

            var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            for (int i = startIndex; i <= sb.Length - value.Length; i++)
            {
                bool match = true;
                for (int j = 0; j < value.Length; j++)
                {
                    if (char.ToLower(sb[i + j]) != char.ToLower(value[j]))
                    {
                        match = false;
                        break;
                    }
                }

                if (match)
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// 替换字符串
        /// </summary>
        public static StringBuilder Replace(this StringBuilder? sb, string oldValue, string? newValue, bool ignoreCase = false)
        {
            if (sb == null || string.IsNullOrEmpty(oldValue))
                return sb ?? throw new ArgumentNullException(nameof(sb));

            int index;
            int searchIndex = 0;

            while ((index = sb.IndexOf(oldValue, searchIndex, ignoreCase)) >= 0)
            {
                sb.Remove(index, oldValue.Length);
                sb.Insert(index, newValue);
                searchIndex = index + (newValue?.Length ?? 0);
            }

            return sb;
        }

        #endregion
    }
}
