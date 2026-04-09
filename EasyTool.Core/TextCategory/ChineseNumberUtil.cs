using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace EasyTool.TextCategory
{
    /// <summary>
    /// 中文数字转换工具类
    /// 支持数字与中文数字互转、金额大写转换
    /// </summary>
    public static class ChineseNumberUtil
    {
        #region 常量定义

        private static readonly string[] ChineseDigits = { "零", "一", "二", "三", "四", "五", "六", "七", "八", "九" };
        private static readonly string[] ChineseUpperDigits = { "零", "壹", "贰", "叁", "肆", "伍", "陆", "柒", "捌", "玖" };
        private static readonly string[] ChineseUnits = { "", "十", "百", "千" };
        private static readonly string[] ChineseUpperUnits = { "", "拾", "佰", "仟" };
        private static readonly string[] ChineseBigUnits = { "", "万", "亿", "兆" };
        private static readonly string[] MoneyUnits = { "元", "角", "分" };
        private static readonly string[] MoneyIntUnits = { "", "拾", "佰", "仟", "万", "拾", "佰", "仟", "亿", "拾", "佰", "仟", "兆" };

        // 中文数字到阿拉伯数字的映射
        private static readonly Dictionary<char, int> ChineseToDigitMap = new Dictionary<char, int>
        {
            {'零', 0}, {'〇', 0}, {'一', 1}, {'二', 2}, {'三', 3}, {'四', 4},
            {'五', 5}, {'六', 6}, {'七', 7}, {'八', 8}, {'九', 9},
            {'壹', 1}, {'贰', 2}, {'叁', 3}, {'肆', 4}, {'伍', 5},
            {'陆', 6}, {'柒', 7}, {'捌', 8}, {'玖', 9},
            {'两', 2}
        };

        private static readonly Dictionary<char, long> ChineseUnitToValueMap = new Dictionary<char, long>
        {
            {'十', 10}, {'拾', 10},
            {'百', 100}, {'佰', 100},
            {'千', 1000}, {'仟', 1000},
            {'万', 10000},
            {'亿', 100000000},
            {'兆', 1000000000000}
        };

        #endregion

        #region 数字转中文

        /// <summary>
        /// 将数字转换为中文数字（小写）
        /// </summary>
        /// <param name="number">数字</param>
        /// <returns>中文数字字符串</returns>
        public static string ToChinese(long number)
        {
            return ToChinese(number, false);
        }

        /// <summary>
        /// 将数字转换为中文数字（大写）
        /// </summary>
        /// <param name="number">数字</param>
        /// <returns>中文大写数字字符串</returns>
        public static string ToChineseUpper(long number)
        {
            return ToChinese(number, true);
        }

        /// <summary>
        /// 将数字转换为中文数字
        /// </summary>
        /// <param name="number">数字</param>
        /// <param name="isUpper">是否大写</param>
        /// <returns>中文数字字符串</returns>
        private static string ToChinese(long number, bool isUpper)
        {
            if (number == 0)
                return isUpper ? ChineseUpperDigits[0] : ChineseDigits[0];

            var result = new StringBuilder();
            var isNegative = number < 0;

            if (isNegative)
            {
                result.Append("负");
                number = -number;
            }

            var digits = isUpper ? ChineseUpperDigits : ChineseDigits;
            var units = isUpper ? ChineseUpperUnits : ChineseUnits;

            // 处理每一级（个、万、亿、兆）
            var unitIndex = 0;
            var needZero = false;

            while (number > 0)
            {
                var section = (int)(number % 10000);
                if (section > 0)
                {
                    var sectionStr = ConvertSection(section, digits, units);
                    if (needZero)
                        result.Insert(0, digits[0]);

                    result.Insert(0, sectionStr + ChineseBigUnits[unitIndex]);
                    needZero = false;
                }
                else
                {
                    needZero = true;
                }

                number /= 10000;
                unitIndex++;
            }

            return result.ToString();
        }

        /// <summary>
        /// 转换四位数的部分
        /// </summary>
        private static string ConvertSection(int section, string[] digits, string[] units)
        {
            var result = new StringBuilder();
            var unitIndex = 0;
            var needZero = false;

            while (section > 0)
            {
                var digit = section % 10;
                if (digit == 0)
                {
                    if (result.Length > 0)
                        needZero = true;
                }
                else
                {
                    if (needZero)
                        result.Insert(0, digits[0]);

                    result.Insert(0, digits[digit] + units[unitIndex]);
                    needZero = false;
                }

                section /= 10;
                unitIndex++;
            }

            return result.ToString();
        }

        /// <summary>
        /// 将小数转换为中文数字
        /// </summary>
        /// <param name="number">数字</param>
        /// <param name="decimalPlaces">小数位数（默认全部）</param>
        /// <returns>中文数字字符串</returns>
        public static string ToChinese(double number, int? decimalPlaces = null)
        {
            var isNegative = number < 0;
            if (isNegative)
                number = -number;

            var longPart = (long)number;
            var result = new StringBuilder();

            if (isNegative)
                result.Append("负");

            result.Append(ToChinese(longPart));

            var decimalPart = number - longPart;
            if (decimalPart > 0)
            {
                result.Append("点");
                var decimalStr = decimalPart.ToString().Substring(2); // 去掉"0."

                if (decimalPlaces.HasValue && decimalPlaces.Value < decimalStr.Length)
                    decimalStr = decimalStr.Substring(0, decimalPlaces.Value);

                foreach (var c in decimalStr)
                {
                    var digit = c - '0';
                    result.Append(ChineseDigits[digit]);
                }
            }

            return result.ToString();
        }

        #endregion

        #region 中文转数字

        /// <summary>
        /// 将中文数字转换为数字
        /// </summary>
        /// <param name="chinese">中文数字字符串</param>
        /// <returns>数字</returns>
        public static long FromChinese(string chinese)
        {
            if (string.IsNullOrWhiteSpace(chinese))
                return 0;

            chinese = chinese.Trim();

            // 处理简单数字
            if (chinese.Length == 1 && ChineseToDigitMap.ContainsKey(chinese[0]))
                return ChineseToDigitMap[chinese[0]];

            long result = 0;
            var temp = 0L;
            var isNegative = false;
            var hasDecimal = false;
            var decimalValue = 0.0;
            var decimalMultiplier = 0.1;

            for (var i = 0; i < chinese.Length; i++)
            {
                var c = chinese[i];

                // 处理负号
                if (c == '负')
                {
                    isNegative = true;
                    continue;
                }

                // 处理小数点
                if (c == '点')
                {
                    hasDecimal = true;
                    continue;
                }

                if (hasDecimal)
                {
                    // 处理小数部分
                    if (ChineseToDigitMap.TryGetValue(c, out var digit))
                    {
                        decimalValue += digit * decimalMultiplier;
                        decimalMultiplier *= 0.1;
                    }
                    continue;
                }

                // 处理单位
                if (ChineseUnitToValueMap.TryGetValue(c, out var unitValue))
                {
                    if (unitValue >= 10000) // 万、亿、兆
                    {
                        temp = temp == 0 ? 1 : temp;
                        result += temp * unitValue;
                        temp = 0;
                    }
                    else // 十、百、千
                    {
                        temp = temp == 0 ? unitValue : temp * unitValue;
                    }
                }
                else if (ChineseToDigitMap.TryGetValue(c, out var digit))
                {
                    temp = temp * 10 + digit;
                }
            }

            result += temp;

            if (hasDecimal)
                result = (long)(result + decimalValue);

            return isNegative ? -result : result;
        }

        /// <summary>
        /// 尝试将中文数字转换为数字
        /// </summary>
        /// <param name="chinese">中文数字字符串</param>
        /// <param name="result">转换结果</param>
        /// <returns>是否转换成功</returns>
        public static bool TryFromChinese(string chinese, out long result)
        {
            try
            {
                result = FromChinese(chinese);
                return true;
            }
            catch
            {
                result = 0;
                return false;
            }
        }

        #endregion

        #region 金额大写

        /// <summary>
        /// 将金额转换为中文大写金额
        /// </summary>
        /// <param name="money">金额</param>
        /// <returns>中文大写金额</returns>
        public static string ToMoney(decimal money)
        {
            if (money == 0)
                return "零元整";

            var result = new StringBuilder();
            var isNegative = money < 0;

            if (isNegative)
            {
                result.Append("负");
                money = -money;
            }

            // 分离整数和小数部分
            var intPart = (long)money;
            var decimalPart = (int)((money - intPart) * 100 + 0.5m); // 四舍五入到分

            // 处理整数部分
            if (intPart > 0)
            {
                var intStr = intPart.ToString();
                var zeroFlag = false;

                for (var i = 0; i < intStr.Length; i++)
                {
                    var digit = intStr[i] - '0';
                    var unitIndex = intStr.Length - 1 - i;

                    if (digit == 0)
                    {
                        zeroFlag = true;
                        // 万、亿位置需要添加单位
                        if (unitIndex == 4 || unitIndex == 8)
                        {
                            result.Append(MoneyIntUnits[unitIndex]);
                            zeroFlag = false;
                        }
                    }
                    else
                    {
                        if (zeroFlag)
                        {
                            result.Append(ChineseUpperDigits[0]);
                            zeroFlag = false;
                        }
                        result.Append(ChineseUpperDigits[digit]);
                        result.Append(MoneyIntUnits[unitIndex]);
                    }
                }

                result.Append("元");
            }

            // 处理小数部分
            if (decimalPart > 0)
            {
                var jiao = decimalPart / 10;
                var fen = decimalPart % 10;

                if (jiao > 0)
                {
                    result.Append(ChineseUpperDigits[jiao]);
                    result.Append("角");
                }

                if (fen > 0)
                {
                    if (jiao == 0 && intPart > 0)
                        result.Append(ChineseUpperDigits[0]);
                    result.Append(ChineseUpperDigits[fen]);
                    result.Append("分");
                }
            }
            else
            {
                result.Append("整");
            }

            return result.ToString();
        }

        /// <summary>
        /// 将金额转换为中文大写金额（double版本）
        /// </summary>
        /// <param name="money">金额</param>
        /// <returns>中文大写金额</returns>
        public static string ToMoney(double money)
        {
            return ToMoney((decimal)money);
        }

        #endregion

        #region 简体/繁体数字转换

        /// <summary>
        /// 将简体数字转换为繁体数字
        /// </summary>
        /// <param name="simple">简体数字</param>
        /// <returns>繁体数字</returns>
        public static string SimpleToTraditional(string simple)
        {
            return simple
                .Replace("零", "零")
                .Replace("一", "壹")
                .Replace("二", "贰")
                .Replace("三", "叁")
                .Replace("四", "肆")
                .Replace("五", "伍")
                .Replace("六", "陆")
                .Replace("七", "柒")
                .Replace("八", "捌")
                .Replace("九", "玖")
                .Replace("十", "拾")
                .Replace("百", "佰")
                .Replace("千", "仟");
        }

        /// <summary>
        /// 将繁体数字转换为简体数字
        /// </summary>
        /// <param name="traditional">繁体数字</param>
        /// <returns>简体数字</returns>
        public static string TraditionalToSimple(string traditional)
        {
            return traditional
                .Replace("壹", "一")
                .Replace("贰", "二")
                .Replace("叁", "三")
                .Replace("肆", "四")
                .Replace("伍", "五")
                .Replace("陆", "六")
                .Replace("柒", "七")
                .Replace("捌", "八")
                .Replace("玖", "九")
                .Replace("拾", "十")
                .Replace("佰", "百")
                .Replace("仟", "千");
        }

        #endregion

        #region 判断方法

        /// <summary>
        /// 判断字符串是否为中文数字
        /// </summary>
        /// <param name="text">字符串</param>
        /// <returns>是否为中文数字</returns>
        public static bool IsChineseNumber(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            foreach (var c in text)
            {
                if (!ChineseToDigitMap.ContainsKey(c) &&
                    !ChineseUnitToValueMap.ContainsKey(c) &&
                    c != '负' && c != '点')
                    return false;
            }

            return true;
        }

        #endregion
    }
}