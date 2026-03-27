using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace EasyTool.TextCategory
{
    /// <summary>
    /// 中文处理工具类
    /// 提供简繁转换、中文数字转换、中文字符判断等功能
    /// </summary>
    public static class ChineseUtil
    {
        #region 简繁转换

        private static readonly Dictionary<char, char> SimplifiedToTraditionalMap = new();
        private static readonly Dictionary<char, char> TraditionalToSimplifiedMap = new();

        static ChineseUtil()
        {
            InitSimplifiedTraditionalMap();
        }

        /// <summary>
        /// 简体转繁体
        /// </summary>
        public static string ToTraditional(string simplified)
        {
            if (string.IsNullOrEmpty(simplified))
                return simplified;

            var sb = new StringBuilder(simplified.Length);
            foreach (var c in simplified)
            {
                sb.Append(SimplifiedToTraditionalMap.TryGetValue(c, out var traditional) ? traditional : c);
            }
            return sb.ToString();
        }

        /// <summary>
        /// 繁体转简体
        /// </summary>
        public static string ToSimplified(string traditional)
        {
            if (string.IsNullOrEmpty(traditional))
                return traditional;

            var sb = new StringBuilder(traditional.Length);
            foreach (var c in traditional)
            {
                sb.Append(TraditionalToSimplifiedMap.TryGetValue(c, out var simplified) ? simplified : c);
            }
            return sb.ToString();
        }

        private static void InitSimplifiedTraditionalMap()
        {
            var pairs = "几幾,发發,历歷,后後,里裡,面麵,松鬆,干乾,干幹,于於,才纔,台臺,云雲,术術,板闆,表錶,别彆,卜蔔,布佈,冲衝,虫蟲,丑醜,党黨,斗鬥,范範,谷穀,胡鬍,回迴,汇匯,伙夥,饥饑,家傢,价價,姜薑,借藉,局侷,据據,克剋,夸誇,困睏,腊臘,蜡蠟,累纍,漓灕,厘釐,帘簾,蒙矇,弥彌,蔑衊,千韆,签籤,秋鞦,曲麯,舍捨,沈瀋,胜勝,松鬆,体體,涂塗,万萬,系係,纤纖,咸鹹,向嚮,吁籲,叶葉,佣傭,余餘,御禦,郁鬱,愿願,岳嶽,扎紮,制製,致緻,钟鐘,种種,周週,注註";

            foreach (var pair in pairs.Split(','))
            {
                var chars = pair.ToCharArray();
                if (chars.Length >= 2)
                {
                    SimplifiedToTraditionalMap[chars[0]] = chars[1];
                    TraditionalToSimplifiedMap[chars[1]] = chars[0];
                }
            }
        }

        #endregion

        #region 中文数字

        private static readonly string[] ChineseDigits = { "零", "一", "二", "三", "四", "五", "六", "七", "八", "九" };
        private static readonly string[] ChineseUnits = { "", "十", "百", "千" };
        private static readonly string[] ChineseBigUnits = { "", "万", "亿", "兆" };

        /// <summary>
        /// 数字转中文数字
        /// </summary>
        public static string ToChineseNumber(long number)
        {
            if (number == 0)
                return "零";

            var result = new StringBuilder();
            var isNegative = number < 0;
            if (isNegative) number = -number;

            var parts = new List<string>();
            int unitIndex = 0;

            while (number > 0)
            {
                var part = number % 10000;
                var partStr = ConvertPart((int)part);
                if (!string.IsNullOrEmpty(partStr))
                {
                    if (unitIndex > 0) partStr += ChineseBigUnits[unitIndex];
                    parts.Insert(0, partStr);
                }
                number /= 10000;
                unitIndex++;
            }

            result.Append(string.Join("", parts));
            var final = result.ToString();
            while (final.Contains("零零")) final = final.Replace("零零", "零");
            final = final.TrimEnd('零');
            if (final.StartsWith("一十")) final = final.Substring(1);

            return (isNegative ? "负" : "") + final;
        }

        private static string ConvertPart(int number)
        {
            if (number == 0) return "";
            var result = new StringBuilder();
            var needZero = false;

            for (int i = 3; i >= 0; i--)
            {
                var digit = (number / (int)Math.Pow(10, i)) % 10;
                if (digit == 0) needZero = true;
                else
                {
                    if (needZero) { result.Append("零"); needZero = false; }
                    result.Append(ChineseDigits[digit]);
                    if (i > 0) result.Append(ChineseUnits[i]);
                }
            }
            return result.ToString();
        }

        /// <summary>
        /// 数字转中文金额（大写）
        /// </summary>
        public static string ToChineseMoney(decimal amount)
        {
            var moneyDigits = new[] { "零", "壹", "贰", "叁", "肆", "伍", "陆", "柒", "捌", "玖" };

            if (amount == 0) return "零元整";

            var result = new StringBuilder();
            var isNegative = amount < 0;
            if (isNegative) amount = -amount;

            var intPart = (long)amount;
            if (intPart > 0)
            {
                result.Append(ConvertMoneyPart(intPart, moneyDigits));
                result.Append("元");
            }

            var decPart = (int)((amount - intPart) * 100);
            if (decPart > 0)
            {
                var jiao = decPart / 10;
                var fen = decPart % 10;
                if (jiao > 0) { result.Append(moneyDigits[jiao]); result.Append("角"); }
                if (fen > 0) { result.Append(moneyDigits[fen]); result.Append("分"); }
            }
            else
            {
                result.Append("整");
            }

            return (isNegative ? "负" : "") + result.ToString();
        }

        private static string ConvertMoneyPart(long number, string[] digits)
        {
            var result = new StringBuilder();
            var parts = new List<string>();
            int unitIndex = 0;
            var units = new[] { "", "拾", "佰", "仟" };
            var bigUnits = new[] { "", "万", "亿" };

            while (number > 0)
            {
                var part = number % 10000;
                var partStr = new StringBuilder();

                for (int i = 3; i >= 0; i--)
                {
                    var digit = (int)((part / Math.Pow(10, i)) % 10);
                    if (digit > 0)
                    {
                        partStr.Append(digits[digit]);
                        if (i > 0) partStr.Append(units[i]);
                    }
                }

                if (partStr.Length > 0)
                {
                    if (unitIndex > 0) partStr.Append(bigUnits[unitIndex]);
                    parts.Insert(0, partStr.ToString());
                }
                number /= 10000;
                unitIndex++;
            }

            return string.Join("", parts);
        }

        #endregion

        #region 中文字符判断

        /// <summary>
        /// 判断是否为中文字符
        /// </summary>
        public static bool IsChinese(char c)
        {
            return c >= 0x4E00 && c <= 0x9FA5;
        }

        /// <summary>
        /// 判断字符串是否全部为中文
        /// </summary>
        public static bool IsAllChinese(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;
            foreach (var c in text)
                if (!IsChinese(c)) return false;
            return true;
        }

        /// <summary>
        /// 判断字符串是否包含中文
        /// </summary>
        public static bool ContainsChinese(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;
            foreach (var c in text)
                if (IsChinese(c)) return true;
            return false;
        }

        /// <summary>
        /// 获取中文字符数量
        /// </summary>
        public static int CountChinese(string text)
        {
            if (string.IsNullOrEmpty(text)) return 0;
            int count = 0;
            foreach (var c in text)
                if (IsChinese(c)) count++;
            return count;
        }

        #endregion

        #region 中文标点转换

        /// <summary>
        /// 中文标点转英文标点
        /// </summary>
        public static string ToEnglishPunctuation(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            var map = new Dictionary<char, char>
            {
                {'，', ','}, {'。', '.'}, {'！', '!'}, {'？', '?'},
                {'；', ';'}, {'：', ':'}, {'（', '('}, {'）', ')'},
                {'【', '['}, {'】', ']'}, {'《', '<'}, {'》', '>'}
            };
            var sb = new StringBuilder(text.Length);
            foreach (var c in text)
                sb.Append(map.TryGetValue(c, out var en) ? en : c);
            return sb.ToString();
        }

        /// <summary>
        /// 英文标点转中文标点
        /// </summary>
        public static string ToChinesePunctuation(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            var map = new Dictionary<char, char>
            {
                {',', '，'}, {'.', '。'}, {'!', '！'}, {'?', '？'},
                {';', '；'}, {':', '：'}, {'(', '（'}, {')', '）'},
                {'[', '【'}, {']', '】'}
            };
            var sb = new StringBuilder(text.Length);
            foreach (var c in text)
                sb.Append(map.TryGetValue(c, out var cn) ? cn : c);
            return sb.ToString();
        }

        #endregion

        #region 获取中文长度

        /// <summary>
        /// 获取字符串显示宽度（中文为2，英文为1）
        /// </summary>
        public static int GetDisplayWidth(string text)
        {
            if (string.IsNullOrEmpty(text)) return 0;
            int width = 0;
            foreach (var c in text)
            {
                if (IsChinese(c) || c > 0xFF)
                    width += 2;
                else
                    width += 1;
            }
            return width;
        }

        /// <summary>
        /// 按显示宽度截取字符串
        /// </summary>
        public static string SubstringByWidth(string text, int width)
        {
            if (string.IsNullOrEmpty(text) || width <= 0) return "";
            int currentWidth = 0;
            var result = new StringBuilder();

            foreach (var c in text)
            {
                var charWidth = IsChinese(c) || c > 0xFF ? 2 : 1;
                if (currentWidth + charWidth > width)
                    break;
                result.Append(c);
                currentWidth += charWidth;
            }
            return result.ToString();
        }

        #endregion
    }
}