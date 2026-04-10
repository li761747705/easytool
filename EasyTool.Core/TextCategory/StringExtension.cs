using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace EasyTool.TextCategory
{
    /// <summary>
    /// 字符串扩展方法
    /// </summary>
    public static class StrExtension
    {
        #region 编译缓存的正则表达式

        private static readonly Regex EmailRegex = new(
            @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
            RegexOptions.Compiled);

        private static readonly Regex PhoneRegex = new(
            @"^1[3-9]\d{9}$",
            RegexOptions.Compiled);

        private static readonly Regex UrlRegex = new(
            @"^(https?|ftp)://[^\s/$.?#].[^\s]*$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex IPv4Regex = new(
            @"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$",
            RegexOptions.Compiled);

        private static readonly Regex IdCardRegex = new(
            @"^[1-9]\d{5}(18|19|20)\d{2}(0[1-9]|1[0-2])(0[1-9]|[12]\d|3[01])\d{3}[\dXx]$",
            RegexOptions.Compiled);

        #endregion

        #region 字符串验证

        /// <summary>
        /// 判断字符串是否是有效的电子邮件地址
        /// </summary>
        public static bool IsEmail(this string value)
        {
            return !string.IsNullOrWhiteSpace(value) && EmailRegex.IsMatch(value);
        }

        /// <summary>
        /// 判断字符串是否是有效的手机号（中国大陆）
        /// </summary>
        public static bool IsPhoneNumber(this string value)
        {
            return !string.IsNullOrWhiteSpace(value) && PhoneRegex.IsMatch(value);
        }

        /// <summary>
        /// 判断字符串是否是有效的 URL
        /// </summary>
        public static bool IsUrl(this string value)
        {
            return !string.IsNullOrWhiteSpace(value) && UrlRegex.IsMatch(value);
        }

        /// <summary>
        /// 判断字符串是否是有效的 IPv4 地址
        /// </summary>
        public static bool IsIPv4(this string value)
        {
            return !string.IsNullOrWhiteSpace(value) && IPv4Regex.IsMatch(value);
        }

        /// <summary>
        /// 判断字符串是否是有效的身份证号（中国大陆）
        /// </summary>
        public static bool IsIdCard(this string value)
        {
            return !string.IsNullOrWhiteSpace(value) && IdCardRegex.IsMatch(value);
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

    /// <summary>
    /// 集合扩展方法
    /// </summary>
    public static class CollectionExtensions
    {
        /// <summary>
        /// 遍历集合执行操作（支持链式调用）
        /// </summary>
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source)
            {
                action(item);
            }
            return source;
        }

        /// <summary>
        /// 判断集合是否为空或 null
        /// </summary>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T>? source)
        {
            return source == null || !source.Any();
        }

        /// <summary>
        /// 判断集合是否不为空
        /// </summary>
        public static bool IsNotNullOrEmpty<T>(this IEnumerable<T>? source)
        {
            return source != null && source.Any();
        }

        /// <summary>
        /// 将集合连接为字符串
        /// </summary>
        public static string JoinAsString<T>(this IEnumerable<T> source, string separator = ",")
        {
            return string.Join(separator, source);
        }

        /// <summary>
        /// 根据属性去重
        /// </summary>
        public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector)
        {
            return source.GroupBy(keySelector).Select(g => g.First());
        }

        /// <summary>
        /// 批量处理
        /// </summary>
        public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> source, int batchSize)
        {
            var batch = new List<T>(batchSize);
            foreach (var item in source)
            {
                batch.Add(item);
                if (batch.Count == batchSize)
                {
                    yield return batch;
                    batch = new List<T>(batchSize);
                }
            }
            if (batch.Count > 0)
            {
                yield return batch;
            }
        }

        /// <summary>
        /// 随机选择元素
        /// </summary>
        public static T RandomElement<T>(this IEnumerable<T> source)
        {
            var list = source as IList<T> ?? source.ToList();
            if (list.Count == 0)
            {
                throw new ArgumentException("集合不能为空");
            }
            return list[MathCategory.RandomUtil.RandomInt(0, list.Count)];
        }

        /// <summary>
        /// 打乱顺序
        /// </summary>
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            var random = new Random();
            return source.OrderBy(_ => random.Next());
        }
    }

    /// <summary>
    /// 日期时间扩展方法
    /// </summary>
    public static class DateTimeExtensions
    {
        /// <summary>
        /// 格式化为标准日期字符串
        /// </summary>
        public static string ToDateString(this DateTime date, string format = "yyyy-MM-dd")
        {
            return date.ToString(format);
        }

        /// <summary>
        /// 格式化为标准日期时间字符串
        /// </summary>
        public static string ToDateTimeString(this DateTime date, string format = "yyyy-MM-dd HH:mm:ss")
        {
            return date.ToString(format);
        }

        /// <summary>
        /// 判断是否为今天
        /// </summary>
        public static bool IsToday(this DateTime date)
        {
            return date.Date == DateTime.Today;
        }

        /// <summary>
        /// 判断是否为工作日（周一到周五）
        /// </summary>
        public static bool IsWeekday(this DateTime date)
        {
            return date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday;
        }

        /// <summary>
        /// 获取年龄
        /// </summary>
        public static int GetAge(this DateTime birthDate)
        {
            var today = DateTime.Today;
            var age = today.Year - birthDate.Year;
            if (birthDate.Date > today.AddYears(-age))
            {
                age--;
            }
            return age;
        }

        /// <summary>
        /// 获取季度
        /// </summary>
        public static int GetQuarter(this DateTime date)
        {
            return (date.Month - 1) / 3 + 1;
        }

        /// <summary>
        /// 转换为时间戳（秒）
        /// </summary>
        public static long ToTimestamp(this DateTime date)
        {
            return new DateTimeOffset(date).ToUnixTimeSeconds();
        }

        /// <summary>
        /// 转换为时间戳（毫秒）
        /// </summary>
        public static long ToTimestampMs(this DateTime date)
        {
            return new DateTimeOffset(date).ToUnixTimeMilliseconds();
        }
    }

    /// <summary>
    /// 数字扩展方法
    /// </summary>
    public static class NumberExtensions
    {
        /// <summary>
        /// 判断是否在范围内
        /// </summary>
        public static bool InRange(this int value, int min, int max)
        {
            return value >= min && value <= max;
        }

        /// <summary>
        /// 判断是否在范围内
        /// </summary>
        public static bool InRange(this double value, double min, double max)
        {
            return value >= min && value <= max;
        }

        /// <summary>
        /// 限制在范围内
        /// </summary>
        public static int Clamp(this int value, int min, int max)
        {
            return Math.Max(min, Math.Min(max, value));
        }

        /// <summary>
        /// 限制在范围内
        /// </summary>
        public static double Clamp(this double value, double min, double max)
        {
            return Math.Max(min, Math.Min(max, value));
        }

        /// <summary>
        /// 转换为中文数字
        /// </summary>
        public static string ToChinese(this int number)
        {
            return ChineseNumberUtil.ToChinese(number);
        }

        /// <summary>
        /// 转换为金额大写
        /// </summary>
        public static string ToMoneyChinese(this decimal amount)
        {
            return ChineseNumberUtil.ToMoney(amount);
        }

        /// <summary>
        /// 转换为文件大小字符串
        /// </summary>
        public static string ToFileSize(this long bytes)
        {
            string[] units = { "B", "KB", "MB", "GB", "TB" };
            int unitIndex = 0;
            double size = bytes;

            while (size >= 1024 && unitIndex < units.Length - 1)
            {
                size /= 1024;
                unitIndex++;
            }

            return $"{size:F2} {units[unitIndex]}";
        }
    }
}
