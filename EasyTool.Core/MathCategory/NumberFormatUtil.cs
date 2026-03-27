using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace EasyTool.MathCategory
{
    /// <summary>
    /// 数字格式化工具类
    /// 提供数字转换为大写金额、中文数字等功能
    /// </summary>
    public static class NumberFormatUtil
    {
        #region 中文大写金额

        private static readonly string[] ChineseDigits = { "零", "壹", "贰", "叁", "肆", "伍", "陆", "柒", "捌", "玖" };
        private static readonly string[] ChineseUnits = { "", "拾", "佰", "仟" };
        private static readonly string[] ChineseBigUnits = { "", "万", "亿", "兆" };

        /// <summary>
        /// 数字转中文大写金额
        /// </summary>
        /// <param name="amount">金额</param>
        /// <returns>中文大写金额</returns>
        public static string ToChineseAmount(decimal amount)
        {
            if (amount == 0)
                return "零元整";

            var result = new StringBuilder();
            var isNegative = amount < 0;

            if (isNegative)
            {
                result.Append("负");
                amount = -amount;
            }

            // 四舍五入到分
            amount = Math.Round(amount, 2);

            var intPart = (long)amount;
            var decPart = (int)((amount - intPart) * 100);

            // 处理整数部分
            if (intPart > 0)
            {
                result.Append(ConvertToChineseAmount(intPart));
                result.Append("元");
            }

            // 处理小数部分
            if (decPart > 0)
            {
                var jiao = decPart / 10;
                var fen = decPart % 10;

                if (jiao > 0)
                {
                    result.Append(ChineseDigits[jiao]);
                    result.Append("角");
                }

                if (fen > 0)
                {
                    result.Append(ChineseDigits[fen]);
                    result.Append("分");
                }
            }
            else
            {
                result.Append("整");
            }

            return result.ToString();
        }

        private static string ConvertToChineseAmount(long number)
        {
            var result = new StringBuilder();
            var parts = new List<string>();
            int unitIndex = 0;

            while (number > 0)
            {
                var part = (int)(number % 10000);
                var partStr = ConvertPartToChinese(part);

                if (!string.IsNullOrEmpty(partStr))
                {
                    if (unitIndex > 0)
                        partStr += ChineseBigUnits[unitIndex];
                    parts.Insert(0, partStr);
                }
                else if (parts.Count > 0)
                {
                    parts.Insert(0, "零");
                }

                number /= 10000;
                unitIndex++;
            }

            result.Append(string.Join("", parts));

            // 处理连续的零
            var final = result.ToString();
            while (final.Contains("零零"))
                final = final.Replace("零零", "零");

            // 去掉末尾的零
            final = final.TrimEnd('零');

            return final;
        }

        private static string ConvertPartToChinese(int number)
        {
            if (number == 0)
                return "";

            var result = new StringBuilder();
            var needZero = false;

            for (int i = 3; i >= 0; i--)
            {
                var digit = (int)(number / Math.Pow(10, i)) % 10;

                if (digit == 0)
                {
                    needZero = true;
                }
                else
                {
                    if (needZero)
                    {
                        result.Append("零");
                        needZero = false;
                    }
                    result.Append(ChineseDigits[digit]);
                    if (i > 0)
                        result.Append(ChineseUnits[i]);
                }
            }

            return result.ToString();
        }

        #endregion

        #region 中文数字

        private static readonly string[] SimpleChineseDigits = { "零", "一", "二", "三", "四", "五", "六", "七", "八", "九" };

        /// <summary>
        /// 数字转中文数字
        /// </summary>
        public static string ToChineseNumber(long number)
        {
            if (number == 0)
                return "零";

            var result = new StringBuilder();
            var isNegative = number < 0;

            if (isNegative)
            {
                result.Append("负");
                number = -number;
            }

            var parts = new List<string>();
            int unitIndex = 0;

            while (number > 0)
            {
                var part = (int)(number % 10000);
                var partStr = ConvertPartToSimpleChinese(part);

                if (!string.IsNullOrEmpty(partStr))
                {
                    if (unitIndex > 0)
                        partStr += ChineseBigUnits[unitIndex];
                    parts.Insert(0, partStr);
                }
                else if (parts.Count > 0)
                {
                    parts.Insert(0, "零");
                }

                number /= 10000;
                unitIndex++;
            }

            result.Append(string.Join("", parts));

            var final = result.ToString();
            while (final.Contains("零零"))
                final = final.Replace("零零", "零");

            final = final.TrimEnd('零');

            // 处理"一十"开头的特殊情况
            if (final.StartsWith("一十"))
                final = final.Substring(1);

            return final;
        }

        private static string ConvertPartToSimpleChinese(int number)
        {
            if (number == 0)
                return "";

            var result = new StringBuilder();
            var needZero = false;

            for (int i = 3; i >= 0; i--)
            {
                var digit = (int)(number / Math.Pow(10, i)) % 10;

                if (digit == 0)
                {
                    needZero = true;
                }
                else
                {
                    if (needZero)
                    {
                        result.Append("零");
                        needZero = false;
                    }
                    result.Append(SimpleChineseDigits[digit]);
                    if (i > 0)
                        result.Append(ChineseUnits[i]);
                }
            }

            return result.ToString();
        }

        #endregion

        #region 英文数字

        private static readonly string[] Ones = { "", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };
        private static readonly string[] Tens = { "", "", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };
        private static readonly string[] Thousands = { "", "thousand", "million", "billion", "trillion" };

        /// <summary>
        /// 数字转英文单词
        /// </summary>
        public static string ToEnglishWords(long number)
        {
            if (number == 0)
                return "zero";

            var result = new StringBuilder();
            var isNegative = number < 0;

            if (isNegative)
            {
                result.Append("negative ");
                number = -number;
            }

            var groups = new List<int>();
            while (number > 0)
            {
                groups.Add((int)(number % 1000));
                number /= 1000;
            }

            for (int i = groups.Count - 1; i >= 0; i--)
            {
                if (groups[i] > 0)
                {
                    result.Append(ConvertGroupToEnglish(groups[i]));
                    if (i > 0)
                        result.Append(" " + Thousands[i] + " ");
                }
            }

            return result.ToString().Trim();
        }

        private static string ConvertGroupToEnglish(int number)
        {
            var result = new StringBuilder();

            if (number >= 100)
            {
                result.Append(Ones[number / 100] + " hundred");
                number %= 100;
                if (number > 0)
                    result.Append(" ");
            }

            if (number >= 20)
            {
                result.Append(Tens[number / 10]);
                number %= 10;
                if (number > 0)
                    result.Append("-" + Ones[number]);
            }
            else if (number > 0)
            {
                result.Append(Ones[number]);
            }

            return result.ToString();
        }

        /// <summary>
        /// 数字转英文金额
        /// </summary>
        public static string ToEnglishAmount(decimal amount)
        {
            if (amount == 0)
                return "zero dollars";

            var result = new StringBuilder();
            var isNegative = amount < 0;

            if (isNegative)
            {
                result.Append("negative ");
                amount = -amount;
            }

            amount = Math.Round(amount, 2);
            var intPart = (long)amount;
            var decPart = (int)((amount - intPart) * 100);

            if (intPart > 0)
            {
                result.Append(ToEnglishWords(intPart));
                result.Append(intPart == 1 ? " dollar" : " dollars");
            }

            if (decPart > 0)
            {
                if (intPart > 0)
                    result.Append(" and ");
                result.Append(ToEnglishWords(decPart));
                result.Append(decPart == 1 ? " cent" : " cents");
            }

            return result.ToString();
        }

        #endregion

        #region 罗马数字

        private static readonly (int Value, string Symbol)[] RomanSymbols = 
        {
            (1000, "M"), (900, "CM"), (500, "D"), (400, "CD"),
            (100, "C"), (90, "XC"), (50, "L"), (40, "XL"),
            (10, "X"), (9, "IX"), (5, "V"), (4, "IV"), (1, "I")
        };

        /// <summary>
        /// 数字转罗马数字
        /// </summary>
        public static string ToRoman(int number)
        {
            if (number < 1 || number > 3999)
                throw new ArgumentOutOfRangeException(nameof(number), "罗马数字范围是1-3999");

            var result = new StringBuilder();

            foreach (var (value, symbol) in RomanSymbols)
            {
                while (number >= value)
                {
                    result.Append(symbol);
                    number -= value;
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// 罗马数字转数字
        /// </summary>
        public static int FromRoman(string roman)
        {
            if (string.IsNullOrWhiteSpace(roman))
                throw new ArgumentException("罗马数字不能为空");

            var values = new Dictionary<char, int>
            {
                {'I', 1}, {'V', 5}, {'X', 10}, {'L', 50},
                {'C', 100}, {'D', 500}, {'M', 1000}
            };

            roman = roman.ToUpper();
            int result = 0;
            int prevValue = 0;

            for (int i = roman.Length - 1; i >= 0; i--)
            {
                if (!values.TryGetValue(roman[i], out var value))
                    throw new ArgumentException($"无效的罗马数字字符: {roman[i]}");

                if (value < prevValue)
                    result -= value;
                else
                    result += value;

                prevValue = value;
            }

            return result;
        }

        #endregion

        #region 格式化

        /// <summary>
        /// 格式化为百分比
        /// </summary>
        public static string ToPercent(double value, int decimals = 2)
        {
            return (value * 100).ToString($"F{decimals}") + "%";
        }

        /// <summary>
        /// 格式化为货币
        /// </summary>
        public static string ToCurrency(decimal amount, string currencySymbol = "¥")
        {
            return currencySymbol + amount.ToString("N2");
        }

        /// <summary>
        /// 格式化为科学计数法
        /// </summary>
        public static string ToScientific(double value, int decimals = 2)
        {
            return value.ToString($"E{decimals}");
        }

        /// <summary>
        /// 格式化为千分位
        /// </summary>
        public static string ToThousands(long number, string separator = ",")
        {
            return number.ToString("N0").Replace(",", separator);
        }

        /// <summary>
        /// 格式化文件大小
        /// </summary>
        public static string ToFileSize(long bytes, int decimals = 2)
        {
            string[] units = { "B", "KB", "MB", "GB", "TB", "PB" };
            double size = bytes;
            int unitIndex = 0;

            while (size >= 1024 && unitIndex < units.Length - 1)
            {
                size /= 1024;
                unitIndex++;
            }

            return $"{Math.Round(size, decimals)} {units[unitIndex]}";
        }

        /// <summary>
        /// 格式化序数词（1st, 2nd, 3rd, 4th...）
        /// </summary>
        public static string ToOrdinal(int number)
        {
            if (number <= 0)
                return number.ToString();

            string suffix;
            int mod100 = number % 100;
            
            if (mod100 == 11 || mod100 == 12 || mod100 == 13)
            {
                suffix = "th";
            }
            else
            {
                int mod10 = number % 10;
                suffix = mod10 switch
                {
                    1 => "st",
                    2 => "nd",
                    3 => "rd",
                    _ => "th"
                };
            }

            return number + suffix;
        }

        #endregion
    }
}
