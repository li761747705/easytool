using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace EasyTool.TextCategory
{
    /// <summary>
    /// 字符串处理工具类
    /// </summary>
    public static class StrUtil
    {
        /// <summary>
        /// 移除字符串中的所有空格
        /// </summary>
        /// <param name="str">要处理的字符串</param>
        /// <returns>处理后的字符串</returns>
        public static string RemoveAllSpaces(string str)
        {
            return Regex.Replace(str, @"\s+", "");
        }


        /// <summary>
        /// 检查字符串是否为数字
        /// </summary>
        /// <param name="str">要检查的字符串</param>
        /// <returns>如果是数字，则返回true，否则返回false</returns>
        public static bool IsNumeric(string str)
        {
            double result;
            return double.TryParse(str, out result);
        }

        /// <summary>
        /// 检查字符串是否为整数
        /// </summary>
        /// <param name="str">要检查的字符串</param>
        /// <returns>如果是整数，则返回true，否则返回false</returns>
        public static bool IsInteger(string str)
        {
            int result;
            return int.TryParse(str, out result);
        }

        /// <summary>
        /// 检查字符串是否为日期
        /// </summary>
        /// <param name="str">要检查的字符串</param>
        /// <returns>如果是日期，则返回true，否则返回false</returns>
        public static bool IsDate(string str)
        {
            DateTime result;
            return DateTime.TryParse(str, out result);
        }








        /// <summary>
        /// 将字符串转换为驼峰命名法
        /// </summary>
        /// <param name="str">要处理的字符串</param>
        /// <returns>转换后的字符串</returns>
        public static string ToCamelCase(string str)
        {
            string[] words = str.Split(new char[] { ' ', '-', '_' }, StringSplitOptions.RemoveEmptyEntries);
            var sb = new StringBuilder();
            for (int i = 0; i < words.Length; i++)
            {
                if (i == 0)
                {
                    sb.Append(words[i].ToLower());
                }
                else
                {
                    sb.Append(words[i].Substring(0, 1).ToUpper());
                    sb.Append(words[i].Substring(1).ToLower());
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// 将字符串转换为帕斯卡命名法（大驼峰命名法）
        /// </summary>
        /// <param name="str">要处理的字符串</param>
        /// <returns>转换后的字符串</returns>
        public static string ToPascalCase(string str)
        {
            string[] words = str.Split(new char[] { ' ', '-', '_' }, StringSplitOptions.RemoveEmptyEntries);
            var sb = new StringBuilder();
            for (int i = 0; i < words.Length; i++)
            {
                sb.Append(words[i].Substring(0, 1).ToUpper());
                sb.Append(words[i].Substring(1).ToLower());
            }
            return sb.ToString();
        }

        /// <summary>
        /// 将字符串转换为下划线命名法
        /// </summary>
        /// <param name="str">要处理的字符串</param>
        /// <returns>转换后的字符串</returns>
        public static string ToSnakeCase(string str)
        {
            string[] words = str.Split(new char[] { ' ', '-', '_' }, StringSplitOptions.RemoveEmptyEntries);
            var sb = new StringBuilder();
            for (int i = 0; i < words.Length; i++)
            {
                if (i == 0)
                {
                    sb.Append(words[i].ToLower());
                }
                else
                {
                    sb.Append('_');
                    sb.Append(words[i].ToLower());
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// 将字符串转换为连字符命名法（短横线命名法）
        /// </summary>
        /// <param name="str">要处理的字符串</param>
        /// <returns>转换后的字符串</returns>
        public static string ToKebabCase(string str)
        {
            string[] words = str.Split(new char[] { ' ', '-', '_' }, StringSplitOptions.RemoveEmptyEntries);
            var sb = new StringBuilder();
            for (int i = 0; i < words.Length; i++)
            {
                if (i == 0)
                {
                    sb.Append(words[i].ToLower());
                }
                else
                {
                    sb.Append('-');
                    sb.Append(words[i].ToLower());
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// 将字符串中的 HTML 标记去除
        /// </summary>
        /// <param name="str">要处理的字符串</param>
        /// <returns>去除 HTML 标记后的字符串</returns>
        public static string StripHtml(string str)
        {
            return Regex.Replace(str, "<.*?>", "");
        }

        /// <summary>
        /// 比较两个字符串是否相等，忽略大小写和空格
        /// </summary>
        /// <param name="str1">第一个字符串</param>
        /// <param name="str2">第二个字符串</param>
        /// <returns>如果相等，则返回true，否则返回false</returns>
        public static bool EqualsIgnoreCaseAndWhiteSpace(string str1, string str2)
        {
            return string.Equals(RemoveAllSpaces(str1), RemoveAllSpaces(str2), StringComparison.OrdinalIgnoreCase);
        }



        /// <summary>
        /// 将字符串中的某些字符替换成指定的字符
        /// </summary>
        /// <param name="str">要处理的字符串</param>
        /// <param name="chars">要替换的字符数组</param>
        /// <param name="newChar">新的字符</param>
        /// <returns>处理后的字符串</returns>
        public static string ReplaceChars(string str, char[] chars, char newChar)
        {
            for (int i = 0; i < chars.Length; i++)
            {
                str = str.Replace(chars[i], newChar);
            }
            return str;
        }

        /// <summary>
        /// 将字符串中的某些子字符串替换成指定的子字符串
        /// </summary>
        /// <param name="str">要处理的字符串</param>
        /// <param name="oldValues">要替换的子字符串数组</param>
        /// <param name="newValue">新的子字符串</param>
        /// <returns>处理后的字符串</returns>
        public static string ReplaceStrings(string str, string[] oldValues, string newValue)
        {
            for (int i = 0; i < oldValues.Length; i++)
            {
                str = str.Replace(oldValues[i], newValue);
            }
            return str;
        }

        /// <summary>
        /// 将字符串中的某些子字符串替换成指定的子字符串，忽略大小写
        /// </summary>
        /// <param name="str">要处理的字符串</param>
        /// <param name="oldValues">要替换的子字符串数组</param>
        /// <param name="newValue">新的子字符串</param>
        /// <returns>处理后的字符串</returns>
        public static string ReplaceStringsIgnoreCase(string str, string[] oldValues, string newValue)
        {
            for (int i = 0; i < oldValues.Length; i++)
            {
                str = Regex.Replace(str, oldValues[i], newValue, RegexOptions.IgnoreCase);
            }
            return str;
        }

        /// <summary>
        /// 将字符串转换为 Title Case 格式，即每个单词的首字母大写
        /// </summary>
        /// <param name="str">要处理的字符串</param>
        /// <returns>转换后的字符串</returns>
        public static string ToTitleCase(string str)
        {
            return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str.ToLower());
        }

        /// <summary>
        /// 将字符串中的首字母大写
        /// </summary>
        /// <param name="str">要处理的字符串</param>
        /// <returns>处理后的字符串</returns>
        public static string ToFirstLetterUpperCase(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }
            char[] chars = str.ToCharArray();
            chars[0] = char.ToUpper(chars[0]);
            return new string(chars);
        }

        /// <summary>
        /// 将字符串中的首字母小写
        /// </summary>
        /// <param name="str">要处理的字符串</param>
        /// <returns>处理后的字符串</returns>
        public static string ToFirstLetterLowerCase(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }
            char[] chars = str.ToCharArray();
            chars[0] = char.ToLower(chars[0]);
            return new string(chars);
        }
    }
}
