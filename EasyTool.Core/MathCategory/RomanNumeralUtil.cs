using System;
using System.Collections.Generic;
using System.Text;

namespace EasyTool.MathCategory
{
    /// <summary>
    /// 罗马数字工具类
    /// 提供阿拉伯数字与罗马数字之间的转换
    /// </summary>
    public static class RomanNumeralUtil
    {
        private static readonly Dictionary<int, string> RomanMap = new()
        {
            { 1000, "M" },
            { 900, "CM" },
            { 500, "D" },
            { 400, "CD" },
            { 100, "C" },
            { 90, "XC" },
            { 50, "L" },
            { 40, "XL" },
            { 10, "X" },
            { 9, "IX" },
            { 5, "V" },
            { 4, "IV" },
            { 1, "I" }
        };

        private static readonly Dictionary<char, int> RomanValues = new()
        {
            { 'I', 1 },
            { 'V', 5 },
            { 'X', 10 },
            { 'L', 50 },
            { 'C', 100 },
            { 'D', 500 },
            { 'M', 1000 }
        };

        /// <summary>
        /// 将整数转换为罗马数字
        /// </summary>
        public static string ToRoman(int number)
        {
            if (number < 1 || number > 3999)
                throw new ArgumentOutOfRangeException(nameof(number), "数字必须在 1 到 3999 之间");

            var result = new StringBuilder();

            foreach (var kvp in RomanMap)
            {
                while (number >= kvp.Key)
                {
                    result.Append(kvp.Value);
                    number -= kvp.Key;
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// 将罗马数字转换为整数
        /// </summary>
        public static int FromRoman(string roman)
        {
            if (string.IsNullOrWhiteSpace(roman))
                throw new ArgumentException("罗马数字不能为空");

            roman = roman.ToUpperInvariant().Trim();
            int result = 0;
            int prevValue = 0;

            for (int i = roman.Length - 1; i >= 0; i--)
            {
                if (!RomanValues.TryGetValue(roman[i], out int value))
                    throw new ArgumentException($"无效的罗马数字字符: {roman[i]}");

                if (value < prevValue)
                    result -= value;
                else
                    result += value;

                prevValue = value;
            }

            // 验证结果是否有效
            if (ToRoman(result) != roman)
                throw new ArgumentException($"无效的罗马数字: {roman}");

            return result;
        }

        /// <summary>
        /// 尝试将罗马数字转换为整数
        /// </summary>
        public static bool TryParse(string roman, out int result)
        {
            result = 0;
            try
            {
                result = FromRoman(roman);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 验证罗马数字是否有效
        /// </summary>
        public static bool IsValid(string roman)
        {
            return TryParse(roman, out _);
        }
    }
}
