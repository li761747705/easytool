using System;
using System.Globalization;

namespace EasyTool.ToolCategory
{
    /// <summary>
    /// String 字符串比较扩展方法
    /// </summary>
    public static class StringComparisonExtension
    {
        #region 忽略大小写比较

        /// <summary>
        /// 忽略大小写判断字符串相等
        /// </summary>
        public static bool EqualsIgnoreCase(this string? str, string? value)
        {
            return string.Equals(str, value, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 忽略大小写判断字符串包含
        /// </summary>
        public static bool ContainsIgnoreCase(this string? str, string? value)
        {
            if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(value))
                return false;

            return CultureInfo.InvariantCulture.CompareInfo.IndexOf(str, value, CompareOptions.IgnoreCase) >= 0;
        }

        /// <summary>
        /// 忽略大小写判断字符串是否以指定字符串开头
        /// </summary>
        public static bool StartsWithIgnoreCase(this string? str, string? value)
        {
            if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(value))
                return false;

            return str.StartsWith(value, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 忽略大小写判断字符串是否以指定字符串结尾
        /// </summary>
        public static bool EndsWithIgnoreCase(this string? str, string? value)
        {
            if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(value))
                return false;

            return str.EndsWith(value, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region 模糊匹配

        /// <summary>
        /// 判断字符串是否包含指定字符（忽略大小写）
        /// </summary>
        public static bool ContainsCharIgnoreCase(this string? str, char value)
        {
            if (string.IsNullOrEmpty(str))
                return false;

            return str.IndexOf(char.ToLower(value), StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>
        /// 判断字符串是否包含任意指定字符串
        /// </summary>
        public static bool ContainsAny(this string? str, params string?[] values)
        {
            if (string.IsNullOrEmpty(str) || values == null || values.Length == 0)
                return false;

            foreach (var value in values)
            {
                if (str.Contains(value))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 忽略大小写判断字符串是否包含任意指定字符串
        /// </summary>
        public static bool ContainsAnyIgnoreCase(this string? str, params string?[] values)
        {
            if (string.IsNullOrEmpty(str) || values == null || values.Length == 0)
                return false;

            foreach (var value in values)
            {
                if (str.ContainsIgnoreCase(value))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 判断字符串是否包含所有指定字符串
        /// </summary>
        public static bool ContainsAll(this string? str, params string?[] values)
        {
            if (string.IsNullOrEmpty(str) || values == null || values.Length == 0)
                return false;

            foreach (var value in values)
            {
                if (!str.Contains(value))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 忽略大小写判断字符串是否包含所有指定字符串
        /// </summary>
        public static bool ContainsAllIgnoreCase(this string? str, params string?[] values)
        {
            if (string.IsNullOrEmpty(str) || values == null || values.Length == 0)
                return false;

            foreach (var value in values)
            {
                if (!str.ContainsIgnoreCase(value))
                    return false;
            }

            return true;
        }

        #endregion

        #region 通配符匹配

        /// <summary>
        /// 使用通配符匹配字符串
        /// </summary>
        public static bool Like(this string? str, string? pattern, bool ignoreCase = true)
        {
            if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(pattern))
                return false;

            var comparisonType = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            // 转换通配符模式为正则表达式
            var regexPattern = "^" + System.Text.RegularExpressions.Regex.Escape(pattern)
                .Replace("\\*", ".*")
                .Replace("\\?", ".") + "$";

            var regex = new System.Text.RegularExpressions.Regex(
                regexPattern,
                ignoreCase ? System.Text.RegularExpressions.RegexOptions.IgnoreCase : System.Text.RegularExpressions.RegexOptions.None);

            return regex.IsMatch(str);
        }

        #endregion

        #region 前缀/后缀检查

        /// <summary>
        /// 判断字符串是否以任意指定前缀开头
        /// </summary>
        public static bool StartsWithAny(this string? str, params string?[] prefixes)
        {
            if (string.IsNullOrEmpty(str) || prefixes == null || prefixes.Length == 0)
                return false;

            foreach (var prefix in prefixes)
            {
                if (!string.IsNullOrEmpty(prefix) && str.StartsWith(prefix))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 忽略大小写判断字符串是否以任意指定前缀开头
        /// </summary>
        public static bool StartsWithAnyIgnoreCase(this string? str, params string?[] prefixes)
        {
            if (string.IsNullOrEmpty(str) || prefixes == null || prefixes.Length == 0)
                return false;

            foreach (var prefix in prefixes)
            {
                if (!string.IsNullOrEmpty(prefix) && str.StartsWithIgnoreCase(prefix))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 判断字符串是否以任意指定后缀结尾
        /// </summary>
        public static bool EndsWithAny(this string? str, params string?[] suffixes)
        {
            if (string.IsNullOrEmpty(str) || suffixes == null || suffixes.Length == 0)
                return false;

            foreach (var suffix in suffixes)
            {
                if (!string.IsNullOrEmpty(suffix) && str.EndsWith(suffix))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 忽略大小写判断字符串是否以任意指定后缀结尾
        /// </summary>
        public static bool EndsWithAnyIgnoreCase(this string? str, params string?[] suffixes)
        {
            if (string.IsNullOrEmpty(str) || suffixes == null || suffixes.Length == 0)
                return false;

            foreach (var suffix in suffixes)
            {
                if (!string.IsNullOrEmpty(suffix) && str.EndsWithIgnoreCase(suffix))
                    return true;
            }

            return false;
        }

        #endregion

        #region 字符串相似度

        /// <summary>
        /// 计算字符串相似度（0-1之间，1表示完全相同）
        /// 使用 Levenshtein 距离算法
        /// </summary>
        public static double Similarity(this string? str, string? value)
        {
            if (string.IsNullOrEmpty(str) && string.IsNullOrEmpty(value))
                return 1;

            if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(value))
                return 0;

            int distance = LevenshteinDistance(str, value);
            int maxLength = Math.Max(str.Length, value.Length);

            return 1 - (double)distance / maxLength;
        }

        /// <summary>
        /// 计算 Levenshtein 距离
        /// </summary>
        private static int LevenshteinDistance(string str1, string str2)
        {
            int[,] distance = new int[str1.Length + 1, str2.Length + 1];

            for (int i = 0; i <= str1.Length; i++)
                distance[i, 0] = i;

            for (int j = 0; j <= str2.Length; j++)
                distance[0, j] = j;

            for (int i = 1; i <= str1.Length; i++)
            {
                for (int j = 1; j <= str2.Length; j++)
                {
                    int cost = str1[i - 1] == str2[j - 1] ? 0 : 1;

                    distance[i, j] = Math.Min(
                        Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1),
                        distance[i - 1, j - 1] + cost);
                }
            }

            return distance[str1.Length, str2.Length];
        }

        /// <summary>
        /// 判断字符串相似度是否超过指定阈值
        /// </summary>
        public static bool IsSimilarTo(this string? str, string? value, double threshold = 0.8)
        {
            return str.Similarity(value) >= threshold;
        }

        #endregion

        #region 字符串比较

        /// <summary>
        /// 比较字符串（忽略大小写）
        /// </summary>
        public static int CompareToIgnoreCase(this string? str, string? value)
        {
            return string.Compare(str, value, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 比较字符串（使用指定的文化信息）
        /// </summary>
        public static int CompareToCulture(this string? str, string? value, CultureInfo culture, bool ignoreCase = false)
        {
            return string.Compare(str, value, culture, ignoreCase ? CompareOptions.IgnoreCase : CompareOptions.None);
        }

        #endregion

        #region 首字母大小写

        /// <summary>
        /// 首字母大写
        /// </summary>
        public static string ToTitleCase(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            return char.ToUpper(str[0]) + str.Substring(1);
        }

        /// <summary>
        /// 首字母小写
        /// </summary>
        public static string ToCamelCase(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            return char.ToLower(str[0]) + str.Substring(1);
        }

        /// <summary>
        /// 将字符串转换为标题格式（每个单词首字母大写）
        /// </summary>
        public static string ToTitleCase(this string str, CultureInfo? culture = null)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            culture ??= CultureInfo.CurrentCulture;
            return culture.TextInfo.ToTitleCase(str);
        }

        #endregion

        #region 大小写转换


        /// <summary>
        /// 转换为单词首字母大写（如：helloWorld -> HelloWorld）
        /// </summary>
        public static string ToPascalCase(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            var words = str.Split(new[] { ' ', '-', '_' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Length > 0)
                {
                    words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1).ToLower();
                }
            }

            return string.Join("", words);
        }

        #endregion
    }
}
