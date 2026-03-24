using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace EasyTool.MathCategory
{
    /// <summary>
    /// 金额工具类
    /// 提供精确的金额计算、格式化、大写转换等功能
    /// </summary>
    public static class MoneyUtil
    {
        #region 金额计算

        /// <summary>
        /// 精确加法运算
        /// </summary>
        /// <param name="amount1">金额1</param>
        /// <param name="amount2">金额2</param>
        /// <returns>结果</returns>
        public static decimal Add(decimal amount1, decimal amount2)
        {
            return decimal.Add(amount1, amount2);
        }

        /// <summary>
        /// 精确减法运算
        /// </summary>
        /// <param name="amount1">金额1</param>
        /// <param name="amount2">金额2</param>
        /// <returns>结果</returns>
        public static decimal Subtract(decimal amount1, decimal amount2)
        {
            return decimal.Subtract(amount1, amount2);
        }

        /// <summary>
        /// 精确乘法运算
        /// </summary>
        /// <param name="amount">金额</param>
        /// <param name="multiplier">乘数</param>
        /// <returns>结果</returns>
        public static decimal Multiply(decimal amount, decimal multiplier)
        {
            return decimal.Multiply(amount, multiplier);
        }

        /// <summary>
        /// 精确除法运算
        /// </summary>
        /// <param name="amount">金额</param>
        /// <param name="divisor">除数</param>
        /// <param name="decimals">保留小数位数（默认2）</param>
        /// <returns>结果</returns>
        public static decimal Divide(decimal amount, decimal divisor, int decimals = 2)
        {
            if (divisor == 0)
                throw new DivideByZeroException("除数不能为0");

            return Math.Round(amount / divisor, decimals, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// 四舍五入
        /// </summary>
        /// <param name="amount">金额</param>
        /// <param name="decimals">保留小数位数</param>
        /// <returns>结果</returns>
        public static decimal Round(decimal amount, int decimals = 2)
        {
            return Math.Round(amount, decimals, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// 向上取整
        /// </summary>
        /// <param name="amount">金额</param>
        /// <param name="decimals">保留小数位数</param>
        /// <returns>结果</returns>
        public static decimal Ceiling(decimal amount, int decimals = 0)
        {
            var factor = (decimal)Math.Pow(10, decimals);
            return Math.Ceiling(amount * factor) / factor;
        }

        /// <summary>
        /// 向下取整
        /// </summary>
        /// <param name="amount">金额</param>
        /// <param name="decimals">保留小数位数</param>
        /// <returns>结果</returns>
        public static decimal Floor(decimal amount, int decimals = 0)
        {
            var factor = (decimal)Math.Pow(10, decimals);
            return Math.Floor(amount * factor) / factor;
        }

        /// <summary>
        /// 计算百分比
        /// </summary>
        /// <param name="amount">金额</param>
        /// <param name="percentage">百分比（如25表示25%）</param>
        /// <param name="decimals">保留小数位数</param>
        /// <returns>结果</returns>
        public static decimal Percentage(decimal amount, decimal percentage, int decimals = 2)
        {
            return Round(amount * percentage / 100, decimals);
        }

        /// <summary>
        /// 计算折扣金额
        /// </summary>
        /// <param name="originalPrice">原价</param>
        /// <param name="discount">折扣（如8表示8折）</param>
        /// <param name="decimals">保留小数位数</param>
        /// <returns>折后价</returns>
        public static decimal Discount(decimal originalPrice, decimal discount, int decimals = 2)
        {
            return Round(originalPrice * discount / 10, decimals);
        }

        /// <summary>
        /// 计算利息
        /// </summary>
        /// <param name="principal">本金</param>
        /// <param name="rate">年利率（如5.5表示5.5%）</param>
        /// <param name="days">天数</param>
        /// <param name="decimals">保留小数位数</param>
        /// <returns>利息</returns>
        public static decimal Interest(decimal principal, decimal rate, int days, int decimals = 2)
        {
            return Round(principal * rate / 100 * days / 365, decimals);
        }

        #endregion

        #region 格式化

        /// <summary>
        /// 格式化金额（默认2位小数，千分位）
        /// </summary>
        /// <param name="amount">金额</param>
        /// <param name="decimals">小数位数</param>
        /// <param name="symbol">货币符号</param>
        /// <returns>格式化后的字符串</returns>
        public static string Format(decimal amount, int decimals = 2, string symbol = "¥")
        {
            return $"{symbol}{amount.ToString("N" + decimals)}";
        }

        /// <summary>
        /// 格式化为人民币格式
        /// </summary>
        /// <param name="amount">金额</param>
        /// <returns>格式化后的字符串</returns>
        public static string FormatCNY(decimal amount)
        {
            return Format(amount, 2, "¥");
        }

        /// <summary>
        /// 格式化为美元格式
        /// </summary>
        /// <param name="amount">金额</param>
        /// <returns>格式化后的字符串</returns>
        public static string FormatUSD(decimal amount)
        {
            return Format(amount, 2, "$");
        }

        /// <summary>
        /// 格式化为欧元格式
        /// </summary>
        /// <param name="amount">金额</param>
        /// <returns>格式化后的字符串</returns>
        public static string FormatEUR(decimal amount)
        {
            return Format(amount, 2, "€");
        }

        #endregion

        #region 金额大写

        private static readonly string[] ChineseDigits = { "零", "壹", "贰", "叁", "肆", "伍", "陆", "柒", "捌", "玖" };
        private static readonly string[] ChineseUnits = { "", "拾", "佰", "仟" };
        private static readonly string[] ChineseBigUnits = { "", "万", "亿", "万亿" };

        /// <summary>
        /// 转换为人民币大写金额
        /// </summary>
        /// <param name="amount">金额</param>
        /// <returns>大写金额</returns>
        public static string ToChineseUpper(decimal amount)
        {
            if (amount < 0)
                return "负" + ToChineseUpper(-amount);

            if (amount == 0)
                return "零元整";

            // 处理超出范围的金额
            if (amount >= 10000000000000000m)
                throw new ArgumentOutOfRangeException(nameof(amount), "金额超出转换范围");

            var result = new StringBuilder();
            var amountStr = amount.ToString("F2");
            var parts = amountStr.Split('.');

            // 整数部分
            var integerPart = long.Parse(parts[0]);
            if (integerPart > 0)
            {
                result.Append(ConvertIntegerToChinese(integerPart));
                result.Append("元");
            }

            // 小数部分
            if (parts.Length > 1)
            {
                var decimalPart = parts[1];
                var jiao = int.Parse(decimalPart[0].ToString());
                var fen = int.Parse(decimalPart[1].ToString());

                if (jiao > 0)
                {
                    result.Append(ChineseDigits[jiao]);
                    result.Append("角");
                }

                if (fen > 0)
                {
                    if (jiao == 0 && integerPart > 0)
                        result.Append("零");
                    result.Append(ChineseDigits[fen]);
                    result.Append("分");
                }
            }

            // 只有整数部分
            if (result.ToString().EndsWith("元"))
            {
                result.Append("整");
            }

            return result.ToString();
        }

        private static string ConvertIntegerToChinese(long number)
        {
            if (number == 0)
                return ChineseDigits[0];

            var result = new StringBuilder();
            var unitIndex = 0;
            var zeroFlag = false;

            while (number > 0)
            {
                var section = (int)(number % 10000);
                var sectionStr = ConvertSectionToChinese(section, zeroFlag);

                if (section > 0)
                {
                    result.Insert(0, ChineseBigUnits[unitIndex]);
                    result.Insert(0, sectionStr);
                    zeroFlag = false;
                }
                else
                {
                    zeroFlag = true;
                }

                number /= 10000;
                unitIndex++;
            }

            return result.ToString();
        }

        private static string ConvertSectionToChinese(int section, bool zeroFlag)
        {
            var result = new StringBuilder();
            var unitIndex = 0;
            var hasZero = zeroFlag;

            while (section > 0)
            {
                var digit = section % 10;

                if (digit > 0)
                {
                    result.Insert(0, ChineseUnits[unitIndex]);
                    result.Insert(0, ChineseDigits[digit]);
                    hasZero = false;
                }
                else if (!hasZero && unitIndex > 0)
                {
                    result.Insert(0, ChineseDigits[0]);
                    hasZero = true;
                }

                section /= 10;
                unitIndex++;
            }

            return result.ToString();
        }

        /// <summary>
        /// 人民币大写金额转数字
        /// </summary>
        /// <param name="chineseAmount">大写金额</param>
        /// <returns>数字金额</returns>
        public static decimal FromChineseUpper(string chineseAmount)
        {
            if (string.IsNullOrWhiteSpace(chineseAmount))
                return 0;

            // 移除"人民币"、"整"等
            chineseAmount = chineseAmount.Replace("人民币", "").Replace("整", "").Trim();

            if (chineseAmount == "零元")
                return 0;

            var digitMap = new Dictionary<char, int>
            {
                {'零', 0}, {'壹', 1}, {'贰', 2}, {'叁', 3}, {'肆', 4},
                {'伍', 5}, {'陆', 6}, {'柒', 7}, {'捌', 8}, {'玖', 9}
            };

            var unitMap = new Dictionary<char, int>
            {
                {'拾', 10}, {'佰', 100}, {'仟', 1000},
                {'万', 10000}, {'亿', 100000000}
            };

            decimal result = 0;
            decimal temp = 0;
            decimal section = 0;

            foreach (var c in chineseAmount)
            {
                if (c == '元')
                {
                    result += temp + section;
                    temp = 0;
                    section = 0;
                }
                else if (c == '角')
                {
                    result += temp / 10m;
                    temp = 0;
                }
                else if (c == '分')
                {
                    result += temp / 100m;
                    temp = 0;
                }
                else if (digitMap.ContainsKey(c))
                {
                    temp = digitMap[c];
                }
                else if (c == '拾' || c == '佰' || c == '仟')
                {
                    section += temp * unitMap[c];
                    temp = 0;
                }
                else if (c == '万' || c == '亿')
                {
                    section = (section + temp) * unitMap[c];
                    temp = 0;
                }
            }

            return result + section + temp;
        }

        #endregion

        #region 汇率转换（简化版）

        /// <summary>
        /// 常用货币汇率（相对于人民币，仅供参考）
        /// </summary>
        private static readonly Dictionary<string, decimal> ExchangeRates = new()
        {
            { "CNY", 1.0m },
            { "USD", 7.2m },
            { "EUR", 7.8m },
            { "GBP", 9.1m },
            { "JPY", 0.048m },
            { "KRW", 0.0054m },
            { "HKD", 0.92m },
            { "TWD", 0.22m }
        };

        /// <summary>
        /// 货币转换
        /// </summary>
        /// <param name="amount">金额</param>
        /// <param name="fromCurrency">源货币代码</param>
        /// <param name="toCurrency">目标货币代码</param>
        /// <param name="decimals">保留小数位数</param>
        /// <returns>转换后的金额</returns>
        public static decimal Convert(decimal amount, string fromCurrency, string toCurrency, int decimals = 2)
        {
            fromCurrency = fromCurrency.ToUpperInvariant();
            toCurrency = toCurrency.ToUpperInvariant();

            if (!ExchangeRates.ContainsKey(fromCurrency))
                throw new ArgumentException($"不支持的货币: {fromCurrency}");

            if (!ExchangeRates.ContainsKey(toCurrency))
                throw new ArgumentException($"不支持的货币: {toCurrency}");

            // 先转为人民币，再转为目标货币
            var cny = amount * ExchangeRates[fromCurrency];
            var result = cny / ExchangeRates[toCurrency];

            return Round(result, decimals);
        }

        /// <summary>
        /// 获取支持的货币列表
        /// </summary>
        /// <returns>货币代码列表</returns>
        public static IEnumerable<string> GetSupportedCurrencies()
        {
            return ExchangeRates.Keys;
        }

        /// <summary>
        /// 更新汇率
        /// </summary>
        /// <param name="currency">货币代码</param>
        /// <param name="rateToCNY">对人民币汇率</param>
        public static void UpdateExchangeRate(string currency, decimal rateToCNY)
        {
            ExchangeRates[currency.ToUpperInvariant()] = rateToCNY;
        }

        #endregion

        #region 分转元

        /// <summary>
        /// 分转元
        /// </summary>
        /// <param name="fen">分</param>
        /// <returns>元</returns>
        public static decimal FenToYuan(long fen)
        {
            return fen / 100m;
        }

        /// <summary>
        /// 元转分
        /// </summary>
        /// <param name="yuan">元</param>
        /// <returns>分</returns>
        public static long YuanToFen(decimal yuan)
        {
            return (long)Math.Round(yuan * 100, MidpointRounding.AwayFromZero);
        }

        #endregion
    }
}
