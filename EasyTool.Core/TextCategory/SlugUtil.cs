using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace EasyTool.TextCategory
{
    /// <summary>
    /// URL Slug 生成工具类
    /// 用于生成友好的 URL 路径
    /// </summary>
    public static class SlugUtil
    {
        /// <summary>
        /// 生成 URL Slug
        /// </summary>
        /// <param name="text">原始文本</param>
        /// <param name="options">生成选项</param>
        /// <returns>Slug 字符串</returns>
        public static string Generate(string text, SlugOptions? options = null)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            options ??= new SlugOptions();

            var result = text;

            // 转换为小写
            if (options.Lowercase)
            {
                result = result.ToLowerInvariant();
            }

            // 音译非拉丁字符
            result = Transliterate(result);

            // 移除 HTML 标签
            if (options.StripHtml)
            {
                result = Regex.Replace(result, @"<[^>]+>", "");
            }

            // 移除特殊字符
            result = Regex.Replace(result, @"[^\w\s\-]", "");

            // 替换空格
            result = Regex.Replace(result, @"\s+", options.Delimiter.ToString());

            // 替换多个分隔符
            var delimiterPattern = Regex.Escape(options.Delimiter.ToString());
            result = Regex.Replace(result, $@"{delimiterPattern}+", options.Delimiter.ToString());

            // 裁剪首尾分隔符
            result = result.Trim(options.Delimiter);

            // 限制长度
            if (options.MaxLength > 0 && result.Length > options.MaxLength)
            {
                result = result.Substring(0, options.MaxLength);

                // 确保不在单词中间截断
                var lastDelimiter = result.LastIndexOf(options.Delimiter);
                if (lastDelimiter > options.MaxLength * 0.5)
                {
                    result = result.Substring(0, lastDelimiter);
                }
            }

            return result;
        }

        /// <summary>
        /// 从标题生成 Slug
        /// </summary>
        /// <param name="title">标题</param>
        /// <returns>Slug 字符串</returns>
        public static string FromTitle(string title)
        {
            return Generate(title, new SlugOptions { MaxLength = 100 });
        }

        /// <summary>
        /// 生成唯一 Slug（添加数字后缀）
        /// </summary>
        /// <param name="text">原始文本</param>
        /// <param name="existingSlugs">已存在的 Slug 集合</param>
        /// <param name="options">生成选项</param>
        /// <returns>唯一的 Slug</returns>
        public static string GenerateUnique(string text, System.Collections.Generic.ISet<string> existingSlugs, SlugOptions? options = null)
        {
            var baseSlug = Generate(text, options);

            if (!existingSlugs.Contains(baseSlug))
                return baseSlug;

            var counter = 1;
            string uniqueSlug;

            do
            {
                uniqueSlug = $"{baseSlug}-{counter}";
                counter++;
            }
            while (existingSlugs.Contains(uniqueSlug));

            return uniqueSlug;
        }

        /// <summary>
        /// 验证 Slug 格式
        /// </summary>
        /// <param name="slug">Slug 字符串</param>
        /// <returns>是否有效</returns>
        public static bool IsValid(string slug)
        {
            if (string.IsNullOrEmpty(slug))
                return false;

            // 只允许小写字母、数字、连字符
            return Regex.IsMatch(slug, @"^[a-z0-9]+(?:-[a-z0-9]+)*$");
        }

        /// <summary>
        /// 从 Slug 还原可读文本
        /// </summary>
        /// <param name="slug">Slug 字符串</param>
        /// <returns>可读文本</returns>
        public static string ToReadable(string slug)
        {
            if (string.IsNullOrEmpty(slug))
                return string.Empty;

            var result = slug.Replace('-', ' ');

            // 首字母大写
            var words = result.Split(' ');
            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Length > 0)
                {
                    words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1);
                }
            }

            return string.Join(" ", words);
        }

        /// <summary>
        /// 音译非拉丁字符
        /// </summary>
        private static string Transliterate(string text)
        {
            var result = new StringBuilder();

            foreach (var c in text.Normalize(NormalizationForm.FormD))
            {
                // 移除变音符号
                var category = char.GetUnicodeCategory(c);
                if (category != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    result.Append(c);
                }
            }

            text = result.ToString().Normalize(NormalizationForm.FormC);

            // 中文拼音映射（常用字）
            text = TransliterateChinese(text);

            // 其他常见音译
            text = text
                .Replace("ß", "ss")
                .Replace("æ", "ae")
                .Replace("ø", "o")
                .Replace("å", "a")
                .Replace("ł", "l")
                .Replace("ń", "n")
                .Replace("ś", "s")
                .Replace("ż", "z")
                .Replace("ź", "z");

            return text;
        }

        /// <summary>
        /// 中文转拼音（简化版）
        /// </summary>
        private static string TransliterateChinese(string text)
        {
            var result = new StringBuilder();

            foreach (var c in text)
            {
                var pinyin = GetPinyin(c);
                if (!string.IsNullOrEmpty(pinyin))
                {
                    result.Append(pinyin);
                    result.Append('-');
                }
                else
                {
                    result.Append(c);
                }
            }

            return result.ToString().TrimEnd('-');
        }

        /// <summary>
        /// 获取汉字拼音（简化映射）
        /// </summary>
        private static string GetPinyin(char c)
        {
            // 这里只提供一些常用字的映射，实际应用可以使用完整的拼音库
            var pinyinMap = new System.Collections.Generic.Dictionary<char, string>
            {
                {'你', "ni"}, {'好', "hao"}, {'是', "shi"}, {'我', "wo"}, {'他', "ta"},
                {'她', "ta"}, {'们', "men"}, {'的', "de"}, {'了', "le"}, {'在', "zai"},
                {'有', "you"}, {'和', "he"}, {'人', "ren"}, {'这', "zhe"}, {'中', "zhong"},
                {'大', "da"}, {'为', "wei"}, {'上', "shang"}, {'个', "ge"}, {'国', "guo"},
                {'到', "dao"}, {'说', "shuo"}, {'要', "yao"}, {'也', "ye"}, {'出', "chu"},
                {'会', "hui"}, {'可', "ke"}, {'能', "neng"}, {'对', "dui"}, {'生', "sheng"},
                {'而', "er"}, {'子', "zi"}, {'那', "na"}, {'得', "de"}, {'于', "yu"},
                {'着', "zhe"}, {'下', "xia"}, {'自', "zi"}, {'之', "zhi"}, {'年', "nian"},
                {'过', "guo"}, {'发', "fa"}, {'后', "hou"}, {'作', "zuo"}, {'里', "li"},
                {'用', "yong"}, {'道', "dao"}, {'行', "xing"}, {'所', "suo"}, {'然', "ran"},
                {'家', "jia"}, {'种', "zhong"}, {'事', "shi"}, {'成', "cheng"}, {'方', "fang"},
                {'多', "duo"}, {'经', "jing"}, {'么', "me"}, {'去', "qu"}, {'法', "fa"},
                {'学', "xue"}, {'如', "ru"}, {'都', "dou"}, {'同', "tong"}, {'现', "xian"},
                {'当', "dang"}, {'没', "mei"}, {'动', "dong"}, {'面', "mian"}, {'起', "qi"},
                {'看', "kan"}, {'定', "ding"}, {'天', "tian"}, {'分', "fen"}, {'还', "hai"},
                {'进', "jin"}, {'小', "xiao"}, {'其', "qi"}
            };

            return pinyinMap.TryGetValue(c, out var pinyin) ? pinyin : string.Empty;
        }
    }

    /// <summary>
    /// Slug 生成选项
    /// </summary>
    public class SlugOptions
    {
        /// <summary>
        /// 是否转换为小写
        /// </summary>
        public bool Lowercase { get; set; } = true;

        /// <summary>
        /// 分隔符
        /// </summary>
        public char Delimiter { get; set; } = '-';

        /// <summary>
        /// 最大长度
        /// </summary>
        public int MaxLength { get; set; } = 0;

        /// <summary>
        /// 是否移除 HTML 标签
        /// </summary>
        public bool StripHtml { get; set; } = true;
    }
}
