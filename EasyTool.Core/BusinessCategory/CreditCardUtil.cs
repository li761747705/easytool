using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace EasyTool.BusinessCategory
{
    /// <summary>
    /// 国际信用卡类型枚举
    /// </summary>
    public enum CreditCardType
    {
        /// <summary>
        /// 未知类型
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Visa
        /// </summary>
        Visa = 1,

        /// <summary>
        /// MasterCard
        /// </summary>
        MasterCard = 2,

        /// <summary>
        /// American Express
        /// </summary>
        Amex = 3,

        /// <summary>
        /// Discover
        /// </summary>
        Discover = 4,

        /// <summary>
        /// JCB
        /// </summary>
        JCB = 5,

        /// <summary>
        /// Diners Club
        /// </summary>
        DinersClub = 6,

        /// <summary>
        /// UnionPay（银联）
        /// </summary>
        UnionPay = 7,

        /// <summary>
        /// Maestro
        /// </summary>
        Maestro = 8
    }

    /// <summary>
    /// 国际信用卡信息
    /// </summary>
    public class CreditCardInfo
    {
        /// <summary>
        /// 卡类型
        /// </summary>
        public CreditCardType Type { get; set; }

        /// <summary>
        /// 卡名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 发卡组织
        /// </summary>
        public string Issuer { get; set; } = string.Empty;
    }

    /// <summary>
    /// 国际信用卡工具类
    /// </summary>
    public static class CreditCardUtil
    {
        #region 常量与私有字段

        /// <summary>
        /// 信用卡号正则表达式（13-19位数字）
        /// </summary>
        private static readonly Regex CardNumberRegex = new(@"^\d{13,19}$", RegexOptions.Compiled);

        /// <summary>
        /// 非数字字符正则表达式
        /// </summary>
        private static readonly Regex NonDigitRegex = new(@"\D", RegexOptions.Compiled);

        /// <summary>
        /// 卡类型识别规则（前缀 -> 卡类型）
        /// </summary>
        private static readonly (string Prefix, CreditCardType Type, string Name, string Issuer)[] CardTypeRules =
        {
            // Visa: 4开头，13或16位
            ("4", CreditCardType.Visa, "Visa", "Visa International"),

            // MasterCard: 51-55, 2221-2720开头，16位
            ("51", CreditCardType.MasterCard, "MasterCard", "MasterCard Worldwide"),
            ("52", CreditCardType.MasterCard, "MasterCard", "MasterCard Worldwide"),
            ("53", CreditCardType.MasterCard, "MasterCard", "MasterCard Worldwide"),
            ("54", CreditCardType.MasterCard, "MasterCard", "MasterCard Worldwide"),
            ("55", CreditCardType.MasterCard, "MasterCard", "MasterCard Worldwide"),
            ("2221", CreditCardType.MasterCard, "MasterCard", "MasterCard Worldwide"),
            ("2222", CreditCardType.MasterCard, "MasterCard", "MasterCard Worldwide"),
            ("2223", CreditCardType.MasterCard, "MasterCard", "MasterCard Worldwide"),
            ("2224", CreditCardType.MasterCard, "MasterCard", "MasterCard Worldwide"),
            ("2225", CreditCardType.MasterCard, "MasterCard", "MasterCard Worldwide"),
            ("2226", CreditCardType.MasterCard, "MasterCard", "MasterCard Worldwide"),
            ("2227", CreditCardType.MasterCard, "MasterCard", "MasterCard Worldwide"),
            ("2228", CreditCardType.MasterCard, "MasterCard", "MasterCard Worldwide"),
            ("2229", CreditCardType.MasterCard, "MasterCard", "MasterCard Worldwide"),
            ("223", CreditCardType.MasterCard, "MasterCard", "MasterCard Worldwide"),
            ("224", CreditCardType.MasterCard, "MasterCard", "MasterCard Worldwide"),
            ("225", CreditCardType.MasterCard, "MasterCard", "MasterCard Worldwide"),
            ("226", CreditCardType.MasterCard, "MasterCard", "MasterCard Worldwide"),
            ("227", CreditCardType.MasterCard, "MasterCard", "MasterCard Worldwide"),
            ("228", CreditCardType.MasterCard, "MasterCard", "MasterCard Worldwide"),
            ("229", CreditCardType.MasterCard, "MasterCard", "MasterCard Worldwide"),
            ("23", CreditCardType.MasterCard, "MasterCard", "MasterCard Worldwide"),
            ("24", CreditCardType.MasterCard, "MasterCard", "MasterCard Worldwide"),
            ("25", CreditCardType.MasterCard, "MasterCard", "MasterCard Worldwide"),
            ("26", CreditCardType.MasterCard, "MasterCard", "MasterCard Worldwide"),
            ("270", CreditCardType.MasterCard, "MasterCard", "MasterCard Worldwide"),
            ("271", CreditCardType.MasterCard, "MasterCard", "MasterCard Worldwide"),
            ("2720", CreditCardType.MasterCard, "MasterCard", "MasterCard Worldwide"),

            // American Express: 34或37开头，15位
            ("34", CreditCardType.Amex, "American Express", "American Express Company"),
            ("37", CreditCardType.Amex, "American Express", "American Express Company"),

            // Discover: 6011, 622126-622925, 644-649, 65开头，16位
            ("6011", CreditCardType.Discover, "Discover", "Discover Financial Services"),
            ("65", CreditCardType.Discover, "Discover", "Discover Financial Services"),
            ("644", CreditCardType.Discover, "Discover", "Discover Financial Services"),
            ("645", CreditCardType.Discover, "Discover", "Discover Financial Services"),
            ("646", CreditCardType.Discover, "Discover", "Discover Financial Services"),
            ("647", CreditCardType.Discover, "Discover", "Discover Financial Services"),
            ("648", CreditCardType.Discover, "Discover", "Discover Financial Services"),
            ("649", CreditCardType.Discover, "Discover", "Discover Financial Services"),
            ("622126", CreditCardType.Discover, "Discover", "Discover Financial Services"),
            ("622127", CreditCardType.Discover, "Discover", "Discover Financial Services"),
            ("622128", CreditCardType.Discover, "Discover", "Discover Financial Services"),
            ("622129", CreditCardType.Discover, "Discover", "Discover Financial Services"),
            ("62213", CreditCardType.Discover, "Discover", "Discover Financial Services"),
            ("62214", CreditCardType.Discover, "Discover", "Discover Financial Services"),
            ("62215", CreditCardType.Discover, "Discover", "Discover Financial Services"),
            ("62216", CreditCardType.Discover, "Discover", "Discover Financial Services"),
            ("62217", CreditCardType.Discover, "Discover", "Discover Financial Services"),
            ("62218", CreditCardType.Discover, "Discover", "Discover Financial Services"),
            ("62219", CreditCardType.Discover, "Discover", "Discover Financial Services"),
            ("6222", CreditCardType.Discover, "Discover", "Discover Financial Services"),
            ("6223", CreditCardType.Discover, "Discover", "Discover Financial Services"),
            ("6224", CreditCardType.Discover, "Discover", "Discover Financial Services"),
            ("6225", CreditCardType.Discover, "Discover", "Discover Financial Services"),
            ("6226", CreditCardType.Discover, "Discover", "Discover Financial Services"),
            ("6227", CreditCardType.Discover, "Discover", "Discover Financial Services"),
            ("6228", CreditCardType.Discover, "Discover", "Discover Financial Services"),
            ("62290", CreditCardType.Discover, "Discover", "Discover Financial Services"),
            ("62291", CreditCardType.Discover, "Discover", "Discover Financial Services"),
            ("622920", CreditCardType.Discover, "Discover", "Discover Financial Services"),
            ("622921", CreditCardType.Discover, "Discover", "Discover Financial Services"),
            ("622922", CreditCardType.Discover, "Discover", "Discover Financial Services"),
            ("622923", CreditCardType.Discover, "Discover", "Discover Financial Services"),
            ("622924", CreditCardType.Discover, "Discover", "Discover Financial Services"),
            ("622925", CreditCardType.Discover, "Discover", "Discover Financial Services"),

            // JCB: 3528-3589开头，16位
            ("3528", CreditCardType.JCB, "JCB", "JCB Co., Ltd."),
            ("3529", CreditCardType.JCB, "JCB", "JCB Co., Ltd."),
            ("353", CreditCardType.JCB, "JCB", "JCB Co., Ltd."),
            ("354", CreditCardType.JCB, "JCB", "JCB Co., Ltd."),
            ("355", CreditCardType.JCB, "JCB", "JCB Co., Ltd."),
            ("356", CreditCardType.JCB, "JCB", "JCB Co., Ltd."),
            ("357", CreditCardType.JCB, "JCB", "JCB Co., Ltd."),
            ("358", CreditCardType.JCB, "JCB", "JCB Co., Ltd."),

            // Diners Club: 300-305, 309, 36, 38-39开头，14位
            ("300", CreditCardType.DinersClub, "Diners Club", "Diners Club International"),
            ("301", CreditCardType.DinersClub, "Diners Club", "Diners Club International"),
            ("302", CreditCardType.DinersClub, "Diners Club", "Diners Club International"),
            ("303", CreditCardType.DinersClub, "Diners Club", "Diners Club International"),
            ("304", CreditCardType.DinersClub, "Diners Club", "Diners Club International"),
            ("305", CreditCardType.DinersClub, "Diners Club", "Diners Club International"),
            ("309", CreditCardType.DinersClub, "Diners Club", "Diners Club International"),
            ("36", CreditCardType.DinersClub, "Diners Club", "Diners Club International"),
            ("38", CreditCardType.DinersClub, "Diners Club", "Diners Club International"),
            ("39", CreditCardType.DinersClub, "Diners Club", "Diners Club International"),

            // UnionPay: 62开头，16-19位
            ("62", CreditCardType.UnionPay, "UnionPay", "China UnionPay"),

            // Maestro: 5018, 5020, 5038, 5893, 6304, 6759, 6761-6763开头，12-19位
            ("5018", CreditCardType.Maestro, "Maestro", "MasterCard Worldwide"),
            ("5020", CreditCardType.Maestro, "Maestro", "MasterCard Worldwide"),
            ("5038", CreditCardType.Maestro, "Maestro", "MasterCard Worldwide"),
            ("5893", CreditCardType.Maestro, "Maestro", "MasterCard Worldwide"),
            ("6304", CreditCardType.Maestro, "Maestro", "MasterCard Worldwide"),
            ("6759", CreditCardType.Maestro, "Maestro", "MasterCard Worldwide"),
            ("6761", CreditCardType.Maestro, "Maestro", "MasterCard Worldwide"),
            ("6762", CreditCardType.Maestro, "Maestro", "MasterCard Worldwide"),
            ("6763", CreditCardType.Maestro, "Maestro", "MasterCard Worldwide")
        };

        #endregion

        #region 验证方法

        /// <summary>
        /// 验证信用卡号是否有效（格式 + Luhn校验）
        /// </summary>
        /// <param name="cardNumber">信用卡号</param>
        /// <returns>是否有效</returns>
        public static bool IsValid(string? cardNumber)
        {
            if (!IsValidFormat(cardNumber))
            {
                return false;
            }

            return ValidateLuhn(cardNumber!);
        }

        /// <summary>
        /// 验证信用卡号格式
        /// </summary>
        /// <param name="cardNumber">信用卡号</param>
        /// <returns>格式是否正确</returns>
        public static bool IsValidFormat(string? cardNumber)
        {
            if (string.IsNullOrWhiteSpace(cardNumber))
            {
                return false;
            }

            string cleaned = NonDigitRegex.Replace(cardNumber, "");
            return CardNumberRegex.IsMatch(cleaned);
        }

        /// <summary>
        /// 使用Luhn算法验证信用卡号
        /// </summary>
        /// <param name="cardNumber">信用卡号</param>
        /// <returns>是否通过Luhn校验</returns>
        public static bool ValidateLuhn(string? cardNumber)
        {
            if (string.IsNullOrWhiteSpace(cardNumber))
            {
                return false;
            }

            string cleaned = NonDigitRegex.Replace(cardNumber, "");
            int sum = 0;
            int length = cleaned.Length;
            bool isEvenPosition = false;

            for (int i = length - 1; i >= 0; i--)
            {
                if (!char.IsDigit(cleaned[i]))
                {
                    return false;
                }

                int digit = cleaned[i] - '0';

                if (isEvenPosition)
                {
                    digit *= 2;
                    if (digit > 9)
                    {
                        digit -= 9;
                    }
                }

                sum += digit;
                isEvenPosition = !isEvenPosition;
            }

            return sum % 10 == 0;
        }

        #endregion

        #region 类型识别

        /// <summary>
        /// 获取信用卡类型
        /// </summary>
        /// <param name="cardNumber">信用卡号</param>
        /// <returns>信用卡类型</returns>
        public static CreditCardType GetCardType(string? cardNumber)
        {
            if (!IsValidFormat(cardNumber))
            {
                return CreditCardType.Unknown;
            }

            string cleaned = NonDigitRegex.Replace(cardNumber!, "");

            // 从最长前缀开始匹配
            for (int len = 6; len >= 1; len--)
            {
                if (cleaned.Length < len) continue;

                string prefix = cleaned.Substring(0, len);
                foreach (var rule in CardTypeRules)
                {
                    if (rule.Prefix == prefix)
                    {
                        return rule.Type;
                    }
                }
            }

            return CreditCardType.Unknown;
        }

        /// <summary>
        /// 获取信用卡信息
        /// </summary>
        /// <param name="cardNumber">信用卡号</param>
        /// <returns>信用卡信息</returns>
        public static CreditCardInfo? GetCardInfo(string? cardNumber)
        {
            if (!IsValidFormat(cardNumber))
            {
                return null;
            }

            string cleaned = NonDigitRegex.Replace(cardNumber!, "");

            for (int len = 6; len >= 1; len--)
            {
                if (cleaned.Length < len) continue;

                string prefix = cleaned.Substring(0, len);
                foreach (var rule in CardTypeRules)
                {
                    if (rule.Prefix == prefix)
                    {
                        return new CreditCardInfo
                        {
                            Type = rule.Type,
                            Name = rule.Name,
                            Issuer = rule.Issuer
                        };
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 获取信用卡类型名称
        /// </summary>
        /// <param name="cardNumber">信用卡号</param>
        /// <returns>类型名称</returns>
        public static string? GetCardTypeName(string? cardNumber)
        {
            return GetCardInfo(cardNumber)?.Name;
        }

        /// <summary>
        /// 判断是否为Visa卡
        /// </summary>
        public static bool IsVisa(string? cardNumber) => GetCardType(cardNumber) == CreditCardType.Visa;

        /// <summary>
        /// 判断是否为MasterCard
        /// </summary>
        public static bool IsMasterCard(string? cardNumber) => GetCardType(cardNumber) == CreditCardType.MasterCard;

        /// <summary>
        /// 判断是否为American Express
        /// </summary>
        public static bool IsAmex(string? cardNumber) => GetCardType(cardNumber) == CreditCardType.Amex;

        /// <summary>
        /// 判断是否为Discover
        /// </summary>
        public static bool IsDiscover(string? cardNumber) => GetCardType(cardNumber) == CreditCardType.Discover;

        /// <summary>
        /// 判断是否为JCB
        /// </summary>
        public static bool IsJCB(string? cardNumber) => GetCardType(cardNumber) == CreditCardType.JCB;

        /// <summary>
        /// 判断是否为银联
        /// </summary>
        public static bool IsUnionPay(string? cardNumber) => GetCardType(cardNumber) == CreditCardType.UnionPay;

        #endregion

        #region 格式化方法

        /// <summary>
        /// 格式化信用卡号（每4位一组）
        /// </summary>
        /// <param name="cardNumber">信用卡号</param>
        /// <returns>格式化后的卡号</returns>
        public static string? Format(string? cardNumber)
        {
            if (!IsValidFormat(cardNumber))
            {
                return null;
            }

            string cleaned = NonDigitRegex.Replace(cardNumber!, "");
            var groups = new List<string>();

            for (int i = 0; i < cleaned.Length; i += 4)
            {
                int len = Math.Min(4, cleaned.Length - i);
                groups.Add(cleaned.Substring(i, len));
            }

            return string.Join(" ", groups);
        }

        /// <summary>
        /// 格式化信用卡号（根据卡类型自动选择格式）
        /// </summary>
        /// <param name="cardNumber">信用卡号</param>
        /// <returns>格式化后的卡号</returns>
        public static string? FormatByType(string? cardNumber)
        {
            if (!IsValidFormat(cardNumber))
            {
                return null;
            }

            string cleaned = NonDigitRegex.Replace(cardNumber!, "");
            CreditCardType type = GetCardType(cleaned);

            // Amex特殊格式：4-6-5
            if (type == CreditCardType.Amex && cleaned.Length == 15)
            {
                return $"{cleaned.Substring(0, 4)} {cleaned.Substring(4, 6)} {cleaned.Substring(10, 5)}";
            }

            // 默认4位一组
            return Format(cleaned);
        }

        /// <summary>
        /// 信用卡号脱敏：**** **** **** 1234
        /// </summary>
        /// <param name="cardNumber">信用卡号</param>
        /// <returns>脱敏后的卡号</returns>
        public static string? Mask(string? cardNumber)
        {
            if (!IsValidFormat(cardNumber))
            {
                return null;
            }

            string cleaned = NonDigitRegex.Replace(cardNumber!, "");

            if (cleaned.Length < 8)
            {
                return null;
            }

            int suffixLen = 4;
            string suffix = cleaned.Substring(cleaned.Length - suffixLen);
            int maskLen = cleaned.Length - suffixLen;
            string masked = new string('*', maskLen);

            // 格式化输出
            CreditCardType type = GetCardType(cleaned);
            if (type == CreditCardType.Amex && cleaned.Length == 15)
            {
                return $"**** ****** {suffix}";
            }

            return $"**** **** **** {suffix}";
        }

        #endregion
    }
}
