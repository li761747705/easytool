using System;
using System.Text;
using System.Text.RegularExpressions;

namespace EasyTool.TextCategory
{
    /// <summary>
    /// URL Slug 工具类
    /// 用于生成 URL 友好的字符串标识符
    /// </summary>
    public static class SlugUtil
    {
        /// <summary>
        /// 默认最大长度
        /// </summary>
        private const int DefaultMaxLength = 100;

        /// <summary>
        /// 生成 URL 友好的 Slug
        /// </summary>
        /// <param name="text">原始文本</param>
        /// <param name="maxLength">最大长度</param>
        /// <returns>Slug 字符串</returns>
        public static string Generate(string? text, int maxLength = DefaultMaxLength)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            // 转小写
            var result = text.ToLowerInvariant();

            // 中文转拼音首字母（简化处理）
            result = ConvertChineseToPinyin(result);

            // 移除特殊字符，保留字母、数字、中文
            result = Regex.Replace(result, @"[^a-z0-9\u4e00-\u9fa5\s-]", "");

            // 将空格和多个连续空格替换为单个连字符
            result = Regex.Replace(result, @"\s+", "-");

            // 将多个连字符合并为一个
            result = Regex.Replace(result, @"-+", "-");

            // 移除首尾的连字符
            result = result.Trim('-');

            // 截断到指定长度
            if (result.Length > maxLength)
            {
                result = result.Substring(0, maxLength).TrimEnd('-');
            }

            return result;
        }

        /// <summary>
        /// 生成带时间戳的 Slug
        /// </summary>
        /// <param name="text">原始文本</param>
        /// <param name="maxLength">最大长度</param>
        /// <returns>带时间戳的 Slug</returns>
        public static string GenerateWithTimestamp(string? text, int maxLength = DefaultMaxLength)
        {
            var slug = Generate(text, maxLength - 15);
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");

            return string.IsNullOrEmpty(slug) ? timestamp : $"{slug}-{timestamp}";
        }

        /// <summary>
        /// 生成带随机后缀的 Slug
        /// </summary>
        /// <param name="text">原始文本</param>
        /// <param name="suffixLength">后缀长度</param>
        /// <param name="maxLength">最大长度</param>
        /// <returns>带随机后缀的 Slug</returns>
        public static string GenerateWithRandomSuffix(string? text, int suffixLength = 6, int maxLength = DefaultMaxLength)
        {
            var slug = Generate(text, maxLength - suffixLength - 1);
            var suffix = GenerateRandomString(suffixLength);

            return string.IsNullOrEmpty(slug) ? suffix : $"{slug}-{suffix}";
        }

        /// <summary>
        /// 生成唯一 Slug（检查重复时使用）
        /// </summary>
        /// <param name="text">原始文本</param>
        /// <param name="exists">检查是否存在的函数</param>
        /// <param name="maxLength">最大长度</param>
        /// <returns>唯一的 Slug</returns>
        public static string GenerateUnique(string? text, Func<string, bool> exists, int maxLength = DefaultMaxLength)
        {
            var baseSlug = Generate(text, maxLength);

            if (string.IsNullOrEmpty(baseSlug))
                baseSlug = GenerateRandomString(8);

            if (!exists(baseSlug))
                return baseSlug;

            for (int i = 1; i <= 100; i++)
            {
                var suffix = i == 1 ? "" : $"-{i}";
                var newSlug = baseSlug.Length + suffix.Length > maxLength
                    ? baseSlug.Substring(0, maxLength - suffix.Length) + suffix
                    : baseSlug + suffix;

                if (!exists(newSlug))
                    return newSlug;
            }

            // 如果还是冲突，添加随机后缀
            return GenerateWithRandomSuffix(baseSlug, 8, maxLength);
        }

        /// <summary>
        /// 验证 Slug 是否有效
        /// </summary>
        /// <param name="slug">Slug 字符串</param>
        /// <returns>是否有效</returns>
        public static bool IsValid(string? slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
                return false;

            // 只允许小写字母、数字、连字符
            return Regex.IsMatch(slug, @"^[a-z0-9]+(?:-[a-z0-9]+)*$");
        }

        /// <summary>
        /// 规范化 Slug
        /// </summary>
        /// <param name="slug">原始 Slug</param>
        /// <returns>规范化后的 Slug</returns>
        public static string Normalize(string? slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
                return string.Empty;

            // 转小写
            var result = slug.ToLowerInvariant();

            // 移除非法字符
            result = Regex.Replace(result, @"[^a-z0-9\s-]", "");

            // 空格转连字符
            result = Regex.Replace(result, @"\s+", "-");

            // 合并多个连字符
            result = Regex.Replace(result, @"-+", "-");

            // 移除首尾连字符
            result = result.Trim('-');

            return result;
        }

        /// <summary>
        /// 将 Slug 转换为标题格式
        /// </summary>
        /// <param name="slug">Slug 字符串</param>
        /// <returns>标题格式字符串</returns>
        public static string ToTitle(string? slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
                return string.Empty;

            var words = slug.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
            var result = new StringBuilder();

            foreach (var word in words)
            {
                if (result.Length > 0)
                    result.Append(' ');

                result.Append(char.ToUpperInvariant(word[0]) + word.Substring(1));
            }

            return result.ToString();
        }

        /// <summary>
        /// 从标题生成 Slug（保留更多语义）
        /// </summary>
        /// <param name="title">标题</param>
        /// <param name="maxLength">最大长度</param>
        /// <returns>Slug</returns>
        public static string FromTitle(string? title, int maxLength = DefaultMaxLength)
        {
            if (string.IsNullOrWhiteSpace(title))
                return string.Empty;

            // 移除常见停用词（可选）
            var stopWords = new[] { "a", "an", "the", "and", "or", "but", "in", "on", "at", "to", "for", "of", "with", "by" };

            var words = title.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var filteredWords = new System.Collections.Generic.List<string>();

            foreach (var word in words)
            {
                var lower = word.ToLowerInvariant();
                if (Array.IndexOf(stopWords, lower) == -1)
                {
                    filteredWords.Add(lower);
                }
            }

            var result = string.Join("-", filteredWords);
            return Generate(result, maxLength);
        }

        #region 私有方法

        private static string ConvertChineseToPinyin(string text)
        {
            // 简化处理：移除中文字符（实际项目中可引入拼音库）
            // 这里只做基础处理，实际应用建议使用 PinyinUtil
            var result = new StringBuilder();

            foreach (var c in text)
            {
                if (c >= 0x4E00 && c <= 0x9FA5)
                {
                    // 中文字符，可以调用 PinyinUtil 获取拼音
                    // 这里简化处理，移除中文
                    continue;
                }
                result.Append(c);
            }

            return result.ToString();
        }

        private static string GenerateRandomString(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            var result = new char[length];

            for (int i = 0; i < length; i++)
            {
                result[i] = chars[random.Next(chars.Length)];
            }

            return new string(result);
        }

        #endregion
    }
}
