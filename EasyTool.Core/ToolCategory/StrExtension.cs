using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace EasyTool.Extension
{
    /// <summary>
    /// 字符串扩展方法
    /// </summary>
    public static class StrExtension
    {
        #region 文本可为空判断

        /// <summary>
        /// 判断字符串是否为 null 或空
        /// [Obsolete("请直接使用 string.IsNullOrEmpty(value)")]
        /// </summary>
        [Obsolete("请直接使用 string.IsNullOrEmpty(value)", false)]
        public static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }

        /// <summary>
        /// 判断字符串是否为 null 或空白字符
        /// [Obsolete("请直接使用 string.IsNullOrWhiteSpace(value)")]
        /// </summary>
        [Obsolete("请直接使用 string.IsNullOrWhiteSpace(value)", false)]
        public static bool IsNullOrWhiteSpace(this string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        #endregion

        #region 字符串验证

        /// <summary>
        /// 判断字符串是否是有效的电子邮件地址
        /// </summary>
        public static bool IsEmail(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            const string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            return Regex.IsMatch(value, pattern);
        }

        /// <summary>
        /// 判断字符串是否是有效的手机号（中国大陆）
        /// </summary>
        public static bool IsPhoneNumber(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            // 中国大陆手机号：1开头，11位数字
            const string pattern = @"^1[3-9]\d{9}$";
            return Regex.IsMatch(value, pattern);
        }

        /// <summary>
        /// 判断字符串是否是有效的 URL
        /// </summary>
        public static bool IsUrl(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            const string pattern = @"^(https?|ftp)://[^\s/$.?#].[^\s]*$";
            return Regex.IsMatch(value, pattern, RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// 判断字符串是否是有效的 IPv4 地址
        /// </summary>
        public static bool IsIPv4(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            const string pattern = @"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";
            return Regex.IsMatch(value, pattern);
        }

        /// <summary>
        /// 判断字符串是否是有效的身份证号（中国大陆）
        /// </summary>
        public static bool IsIdCard(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            // 18位身份证号码
            const string pattern = @"^[1-9]\d{5}(18|19|20)\d{2}(0[1-9]|1[0-2])(0[1-9]|[12]\d|3[01])\d{3}[\dXx]$";
            return Regex.IsMatch(value, pattern);
        }

        #endregion

        #region 字符串转换

        /// <summary>
        /// 将字符串转换为 Base64 编码
        /// </summary>
        public static string ToBase64(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            var bytes = Encoding.UTF8.GetBytes(value);
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// 将 Base64 编码的字符串解码
        /// </summary>
        public static string FromBase64(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            var bytes = Convert.FromBase64String(value);
            return Encoding.UTF8.GetString(bytes);
        }

        /// <summary>
        /// 计算字符串的 MD5 哈希值
        /// </summary>
        public static string ToMd5(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            using var md5 = System.Security.Cryptography.MD5.Create();
            var bytes = Encoding.UTF8.GetBytes(value);
            var hash = md5.ComputeHash(bytes);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        /// <summary>
        /// 计算字符串的 SHA256 哈希值
        /// </summary>
        public static string ToSha256(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(value);
            var hash = sha256.ComputeHash(bytes);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        /// <summary>
        /// 将字符串转换为16进制表示
        /// </summary>
        public static string ToHex(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            var bytes = Encoding.UTF8.GetBytes(value);
            return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
        }

        #endregion

        #region 字符串处理

        /// <summary>
        /// 截断字符串到指定长度，超出部分用省略号代替
        /// </summary>
        /// <param name="value">原始字符串</param>
        /// <param name="maxLength">最大长度</param>
        /// <param name="suffix">后缀，默认为"..."</param>
        public static string Truncate(this string value, int maxLength, string suffix = "...")
        {
            if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
                return value;

            return value.Substring(0, maxLength) + suffix;
        }

        /// <summary>
        /// 移除字符串中的音调符号（如 é -> e）
        /// </summary>
        public static string RemoveDiacritics(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            var normalizedString = value.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

        /// <summary>
        /// 生成 URL 友好的 slug（例如："Hello World" -> "hello-world"）
        /// </summary>
        public static string GenerateSlug(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            // 移除音调符号
            var slug = value.RemoveDiacritics();

            // 转换为小写
            slug = slug.ToLowerInvariant();

            // 替换空格和特殊字符为连字符
            slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
            slug = Regex.Replace(slug, @"\s+", "-");
            slug = Regex.Replace(slug, @"-+", "-");
            slug = slug.Trim('-');

            return slug;
        }

        /// <summary>
        /// 反转字符串
        /// [Obsolete("请直接使用 new string(value.Reverse().ToArray()) 或通过 LINQ")]
        /// </summary>
        [Obsolete("请直接使用 new string(value.Reverse().ToArray())", false)]
        public static string Reverse(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            var charArray = value.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        /// <summary>
        /// 获取字符串的字节数（UTF-8编码）
        /// [Obsolete("请直接使用 Encoding.UTF8.GetByteCount(value)")]
        /// </summary>
        [Obsolete("请直接使用 Encoding.UTF8.GetByteCount(value)", false)]
        public static int GetByteCount(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return 0;

            return Encoding.UTF8.GetByteCount(value);
        }

        /// <summary>
        /// 隐藏字符串的中间部分（例如：手机号、身份证号）
        /// </summary>
        /// <param name="value">原始字符串</param>
        /// <param name="visibleStart">开头保留字符数</param>
        /// <param name="visibleEnd">结尾保留字符数</param>
        /// <param name="maskChar">掩码字符，默认为'*'</param>
        public static string Mask(this string value, int visibleStart = 3, int visibleEnd = 4, char maskChar = '*')
        {
            if (string.IsNullOrEmpty(value))
                return value;

            if (value.Length <= visibleStart + visibleEnd)
                return value;

            var start = value.Substring(0, visibleStart);
            var end = value.Substring(value.Length - visibleEnd);
            var maskLength = value.Length - visibleStart - visibleEnd;
            var mask = new string(maskChar, maskLength);

            return start + mask + end;
        }

        #endregion

        #region 字符串操作

        /// <summary>
        /// 移除字符串中指定的字符
        /// </summary>
        public static string RemoveChars(this string value, params char[] charsToRemove)
        {
            if (string.IsNullOrEmpty(value) || charsToRemove == null || charsToRemove.Length == 0)
                return value;

            var result = new StringBuilder(value.Length);
            foreach (var c in value)
            {
                if (Array.IndexOf(charsToRemove, c) < 0)
                {
                    result.Append(c);
                }
            }
            return result.ToString();
        }

        /// <summary>
        /// 确保字符串以指定后缀结尾
        /// </summary>
        public static string EnsureEndsWith(this string value, string suffix)
        {
            if (string.IsNullOrEmpty(value))
                return suffix ?? string.Empty;

            if (string.IsNullOrEmpty(suffix))
                return value;

            return value.EndsWith(suffix) ? value : value + suffix;
        }

        /// <summary>
        /// 确保字符串以指定前缀开头
        /// </summary>
        public static string EnsureStartsWith(this string value, string prefix)
        {
            if (string.IsNullOrEmpty(value))
                return prefix ?? string.Empty;

            if (string.IsNullOrEmpty(prefix))
                return value;

            return value.StartsWith(prefix) ? value : prefix + value;
        }

        #endregion
    }
}
