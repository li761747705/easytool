using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace EasyTool.BusinessCategory
{
    /// <summary>
    /// 银行卡类型枚举
    /// </summary>
    public enum BankType
    {
        /// <summary>
        /// 未知类型
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// 借记卡
        /// </summary>
        Debit = 1,

        /// <summary>
        /// 信用卡
        /// </summary>
        Credit = 2
    }

    /// <summary>
    /// 银行信息
    /// </summary>
    public class BankInfo
    {
        /// <summary>
        /// 银行名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 卡类型
        /// </summary>
        public BankType Type { get; set; }

        /// <summary>
        /// 银行缩写代码
        /// </summary>
        public string Code { get; set; } = string.Empty;
    }

    /// <summary>
    /// 银行卡工具类
    /// </summary>
    public static class BankCardUtil
    {
        #region 常量与私有字段

        /// <summary>
        /// 银行卡号正则表达式（13-19位数字）
        /// </summary>
        private static readonly Regex BankCardRegex = new(@"^\d{13,19}$", RegexOptions.Compiled);

        /// <summary>
        /// 非数字字符正则表达式
        /// </summary>
        private static readonly Regex NonDigitRegex = new(@"\D", RegexOptions.Compiled);

        /// <summary>
        /// 银行BIN码映射（前6位 -> 银行信息）
        /// 注：此处仅包含部分常见银行BIN码，实际应用中应使用完整的BIN码库
        /// </summary>
        private static readonly Dictionary<string, BankInfo> BinCodeMapping = new()
        {
            // 工商银行
            { "622202", new BankInfo { Name = "中国工商银行", Type = BankType.Debit, Code = "ICBC" } },
            { "622203", new BankInfo { Name = "中国工商银行", Type = BankType.Debit, Code = "ICBC" } },
            { "622204", new BankInfo { Name = "中国工商银行", Type = BankType.Debit, Code = "ICBC" } },
            { "622205", new BankInfo { Name = "中国工商银行", Type = BankType.Debit, Code = "ICBC" } },
            { "622206", new BankInfo { Name = "中国工商银行", Type = BankType.Debit, Code = "ICBC" } },
            { "622207", new BankInfo { Name = "中国工商银行", Type = BankType.Debit, Code = "ICBC" } },
            { "622208", new BankInfo { Name = "中国工商银行", Type = BankType.Debit, Code = "ICBC" } },
            { "622209", new BankInfo { Name = "中国工商银行", Type = BankType.Debit, Code = "ICBC" } },
            { "622210", new BankInfo { Name = "中国工商银行", Type = BankType.Debit, Code = "ICBC" } },

            // 农业银行
            { "622848", new BankInfo { Name = "中国农业银行", Type = BankType.Debit, Code = "ABC" } },
            { "622849", new BankInfo { Name = "中国农业银行", Type = BankType.Debit, Code = "ABC" } },
            { "622845", new BankInfo { Name = "中国农业银行", Type = BankType.Debit, Code = "ABC" } },
            { "622846", new BankInfo { Name = "中国农业银行", Type = BankType.Debit, Code = "ABC" } },
            { "622847", new BankInfo { Name = "中国农业银行", Type = BankType.Debit, Code = "ABC" } },
            { "622821", new BankInfo { Name = "中国农业银行", Type = BankType.Debit, Code = "ABC" } },
            { "622822", new BankInfo { Name = "中国农业银行", Type = BankType.Debit, Code = "ABC" } },
            { "622823", new BankInfo { Name = "中国农业银行", Type = BankType.Debit, Code = "ABC" } },
            { "622824", new BankInfo { Name = "中国农业银行", Type = BankType.Debit, Code = "ABC" } },
            { "622825", new BankInfo { Name = "中国农业银行", Type = BankType.Debit, Code = "ABC" } },

            // 中国银行
            { "621660", new BankInfo { Name = "中国银行", Type = BankType.Debit, Code = "BOC" } },
            { "621661", new BankInfo { Name = "中国银行", Type = BankType.Debit, Code = "BOC" } },
            { "621662", new BankInfo { Name = "中国银行", Type = BankType.Debit, Code = "BOC" } },
            { "621663", new BankInfo { Name = "中国银行", Type = BankType.Debit, Code = "BOC" } },
            { "621665", new BankInfo { Name = "中国银行", Type = BankType.Debit, Code = "BOC" } },
            { "621666", new BankInfo { Name = "中国银行", Type = BankType.Debit, Code = "BOC" } },
            { "621667", new BankInfo { Name = "中国银行", Type = BankType.Debit, Code = "BOC" } },
            { "621668", new BankInfo { Name = "中国银行", Type = BankType.Debit, Code = "BOC" } },
            { "621669", new BankInfo { Name = "中国银行", Type = BankType.Debit, Code = "BOC" } },

            // 建设银行
            { "622700", new BankInfo { Name = "中国建设银行", Type = BankType.Debit, Code = "CCB" } },
            { "622707", new BankInfo { Name = "中国建设银行", Type = BankType.Debit, Code = "CCB" } },
            { "622708", new BankInfo { Name = "中国建设银行", Type = BankType.Debit, Code = "CCB" } },
            { "621081", new BankInfo { Name = "中国建设银行", Type = BankType.Debit, Code = "CCB" } },
            { "436742", new BankInfo { Name = "中国建设银行", Type = BankType.Credit, Code = "CCB" } },
            { "436745", new BankInfo { Name = "中国建设银行", Type = BankType.Credit, Code = "CCB" } },

            // 交通银行
            { "622260", new BankInfo { Name = "交通银行", Type = BankType.Debit, Code = "BOCOM" } },
            { "622261", new BankInfo { Name = "交通银行", Type = BankType.Debit, Code = "BOCOM" } },
            { "622262", new BankInfo { Name = "交通银行", Type = BankType.Debit, Code = "BOCOM" } },
            { "622521", new BankInfo { Name = "交通银行", Type = BankType.Credit, Code = "BOCOM" } },
            { "622522", new BankInfo { Name = "交通银行", Type = BankType.Credit, Code = "BOCOM" } },

            // 招商银行
            { "622580", new BankInfo { Name = "招商银行", Type = BankType.Debit, Code = "CMB" } },
            { "622581", new BankInfo { Name = "招商银行", Type = BankType.Debit, Code = "CMB" } },
            { "622582", new BankInfo { Name = "招商银行", Type = BankType.Debit, Code = "CMB" } },
            { "622588", new BankInfo { Name = "招商银行", Type = BankType.Credit, Code = "CMB" } },
            { "622589", new BankInfo { Name = "招商银行", Type = BankType.Credit, Code = "CMB" } },
            { "621286", new BankInfo { Name = "招商银行", Type = BankType.Debit, Code = "CMB" } },

            // 浦发银行
            { "622518", new BankInfo { Name = "浦发银行", Type = BankType.Debit, Code = "SPDB" } },
            { "622519", new BankInfo { Name = "浦发银行", Type = BankType.Debit, Code = "SPDB" } },
            { "622520", new BankInfo { Name = "浦发银行", Type = BankType.Credit, Code = "SPDB" } },
            { "621228", new BankInfo { Name = "浦发银行", Type = BankType.Debit, Code = "SPDB" } },

            // 民生银行
            { "622615", new BankInfo { Name = "民生银行", Type = BankType.Debit, Code = "CMBC" } },
            { "622617", new BankInfo { Name = "民生银行", Type = BankType.Debit, Code = "CMBC" } },
            { "622618", new BankInfo { Name = "民生银行", Type = BankType.Debit, Code = "CMBC" } },
            { "622620", new BankInfo { Name = "民生银行", Type = BankType.Credit, Code = "CMBC" } },
            { "622622", new BankInfo { Name = "民生银行", Type = BankType.Credit, Code = "CMBC" } },

            // 兴业银行
            { "622909", new BankInfo { Name = "兴业银行", Type = BankType.Debit, Code = "CIB" } },
            { "622910", new BankInfo { Name = "兴业银行", Type = BankType.Debit, Code = "CIB" } },
            { "622911", new BankInfo { Name = "兴业银行", Type = BankType.Debit, Code = "CIB" } },
            { "622912", new BankInfo { Name = "兴业银行", Type = BankType.Debit, Code = "CIB" } },
            { "622918", new BankInfo { Name = "兴业银行", Type = BankType.Credit, Code = "CIB" } },

            // 中信银行
            { "622690", new BankInfo { Name = "中信银行", Type = BankType.Debit, Code = "CITIC" } },
            { "622691", new BankInfo { Name = "中信银行", Type = BankType.Debit, Code = "CITIC" } },
            { "622692", new BankInfo { Name = "中信银行", Type = BankType.Debit, Code = "CITIC" } },
            { "622696", new BankInfo { Name = "中信银行", Type = BankType.Credit, Code = "CITIC" } },
            { "622698", new BankInfo { Name = "中信银行", Type = BankType.Credit, Code = "CITIC" } },

            // 光大银行
            { "622655", new BankInfo { Name = "光大银行", Type = BankType.Debit, Code = "CEB" } },
            { "622656", new BankInfo { Name = "光大银行", Type = BankType.Debit, Code = "CEB" } },
            { "622657", new BankInfo { Name = "光大银行", Type = BankType.Debit, Code = "CEB" } },
            { "622658", new BankInfo { Name = "光大银行", Type = BankType.Credit, Code = "CEB" } },
            { "622685", new BankInfo { Name = "光大银行", Type = BankType.Credit, Code = "CEB" } },

            // 平安银行
            { "622155", new BankInfo { Name = "平安银行", Type = BankType.Debit, Code = "PAB" } },
            { "622156", new BankInfo { Name = "平安银行", Type = BankType.Debit, Code = "PAB" } },
            { "622157", new BankInfo { Name = "平安银行", Type = BankType.Debit, Code = "PAB" } },
            { "622525", new BankInfo { Name = "平安银行", Type = BankType.Credit, Code = "PAB" } },
            { "622526", new BankInfo { Name = "平安银行", Type = BankType.Credit, Code = "PAB" } },

            // 华夏银行
            { "622630", new BankInfo { Name = "华夏银行", Type = BankType.Debit, Code = "HXB" } },
            { "622631", new BankInfo { Name = "华夏银行", Type = BankType.Debit, Code = "HXB" } },
            { "622632", new BankInfo { Name = "华夏银行", Type = BankType.Debit, Code = "HXB" } },

            // 广发银行
            { "622568", new BankInfo { Name = "广发银行", Type = BankType.Debit, Code = "CGB" } },
            { "622569", new BankInfo { Name = "广发银行", Type = BankType.Debit, Code = "CGB" } },
            { "622570", new BankInfo { Name = "广发银行", Type = BankType.Credit, Code = "CGB" } },
            { "622575", new BankInfo { Name = "广发银行", Type = BankType.Credit, Code = "CGB" } },

            // 邮储银行
            { "622150", new BankInfo { Name = "邮储银行", Type = BankType.Debit, Code = "PSBC" } },
            { "622151", new BankInfo { Name = "邮储银行", Type = BankType.Debit, Code = "PSBC" } },
            { "622180", new BankInfo { Name = "邮储银行", Type = BankType.Debit, Code = "PSBC" } },
            { "622181", new BankInfo { Name = "邮储银行", Type = BankType.Debit, Code = "PSBC" } },
            { "622188", new BankInfo { Name = "邮储银行", Type = BankType.Debit, Code = "PSBC" } },

            // 北京银行
            { "622309", new BankInfo { Name = "北京银行", Type = BankType.Debit, Code = "BJBANK" } },
            { "622310", new BankInfo { Name = "北京银行", Type = BankType.Debit, Code = "BJBANK" } },
            { "622311", new BankInfo { Name = "北京银行", Type = BankType.Debit, Code = "BJBANK" } },

            // 上海银行
            { "622462", new BankInfo { Name = "上海银行", Type = BankType.Debit, Code = "SHBANK" } },
            { "622463", new BankInfo { Name = "上海银行", Type = BankType.Debit, Code = "SHBANK" } },
        };

        #endregion

        #region 验证方法

        /// <summary>
        /// 验证银行卡号是否有效（格式 + Luhn校验）
        /// </summary>
        /// <param name="cardNumber">银行卡号</param>
        /// <returns>是否有效</returns>
        public static bool IsValid(string? cardNumber)
        {
            if (!IsValidFormat(cardNumber))
            {
                return false;
            }

            return ValidateLuhn(cardNumber);
        }

        /// <summary>
        /// 仅验证银行卡号格式（不包含Luhn校验）
        /// </summary>
        /// <param name="cardNumber">银行卡号</param>
        /// <returns>格式是否有效</returns>
        public static bool IsValidFormat(string? cardNumber)
        {
            if (string.IsNullOrWhiteSpace(cardNumber))
            {
                return false;
            }

            return BankCardRegex.IsMatch(cardNumber);
        }

        /// <summary>
        /// 使用Luhn算法验证银行卡号
        /// </summary>
        /// <param name="cardNumber">银行卡号</param>
        /// <returns>是否通过Luhn校验</returns>
        public static bool ValidateLuhn(string? cardNumber)
        {
            if (string.IsNullOrWhiteSpace(cardNumber))
            {
                return false;
            }

            int sum = 0;
            int length = cardNumber.Length;
            bool isEvenPosition = false;

            // 从右向左遍历
            for (int i = length - 1; i >= 0; i--)
            {
                if (!char.IsDigit(cardNumber[i]))
                {
                    return false;
                }

                int digit = cardNumber[i] - '0';

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

        /// <summary>
        /// 计算Luhn校验位
        /// </summary>
        /// <param name="cardNumberWithoutCheckDigit">不含校验位的银行卡号</param>
        /// <returns>校验位（0-9），计算失败返回-1</returns>
        public static int CalculateLuhnCheckDigit(string? cardNumberWithoutCheckDigit)
        {
            if (string.IsNullOrWhiteSpace(cardNumberWithoutCheckDigit))
            {
                return -1;
            }

            // 在末尾添加一个临时校验位0
            string tempCardNumber = cardNumberWithoutCheckDigit + "0";
            int sum = 0;
            int length = tempCardNumber.Length;
            bool isEvenPosition = false;

            for (int i = length - 1; i >= 0; i--)
            {
                if (!char.IsDigit(tempCardNumber[i]))
                {
                    return -1;
                }

                int digit = tempCardNumber[i] - '0';

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

            return (10 - (sum % 10)) % 10;
        }

        #endregion

        #region 银行信息查询

        /// <summary>
        /// 获取银行信息
        /// </summary>
        /// <param name="cardNumber">银行卡号</param>
        /// <returns>银行信息，未找到返回null</returns>
        public static BankInfo? GetBankInfo(string? cardNumber)
        {
            if (string.IsNullOrWhiteSpace(cardNumber) || cardNumber.Length < 6)
            {
                return null;
            }

            // 尝试匹配6位BIN码
            string bin6 = cardNumber.Substring(0, 6);
            if (BinCodeMapping.TryGetValue(bin6, out BankInfo? info))
            {
                return info;
            }

            // 尝试匹配5位BIN码
            if (cardNumber.Length >= 5)
            {
                string bin5 = cardNumber.Substring(0, 5);
                if (BinCodeMapping.TryGetValue(bin5, out info))
                {
                    return info;
                }
            }

            // 尝试匹配4位BIN码
            if (cardNumber.Length >= 4)
            {
                string bin4 = cardNumber.Substring(0, 4);
                if (BinCodeMapping.TryGetValue(bin4, out info))
                {
                    return info;
                }
            }

            return null;
        }

        /// <summary>
        /// 获取银行名称
        /// </summary>
        /// <param name="cardNumber">银行卡号</param>
        /// <returns>银行名称</returns>
        public static string? GetBankName(string? cardNumber)
        {
            return GetBankInfo(cardNumber)?.Name;
        }

        /// <summary>
        /// 获取银行缩写代码
        /// </summary>
        /// <param name="cardNumber">银行卡号</param>
        /// <returns>银行缩写代码</returns>
        public static string? GetBankCode(string? cardNumber)
        {
            return GetBankInfo(cardNumber)?.Code;
        }

        /// <summary>
        /// 获取卡类型
        /// </summary>
        /// <param name="cardNumber">银行卡号</param>
        /// <returns>卡类型</returns>
        public static BankType GetBankType(string? cardNumber)
        {
            return GetBankInfo(cardNumber)?.Type ?? BankType.Unknown;
        }

        /// <summary>
        /// 判断是否为借记卡
        /// </summary>
        /// <param name="cardNumber">银行卡号</param>
        /// <returns>是否为借记卡</returns>
        public static bool IsDebitCard(string? cardNumber)
        {
            return GetBankType(cardNumber) == BankType.Debit;
        }

        /// <summary>
        /// 判断是否为信用卡
        /// </summary>
        /// <param name="cardNumber">银行卡号</param>
        /// <returns>是否为信用卡</returns>
        public static bool IsCreditCard(string? cardNumber)
        {
            return GetBankType(cardNumber) == BankType.Credit;
        }

        /// <summary>
        /// 获取BIN码（前6位）
        /// </summary>
        /// <param name="cardNumber">银行卡号</param>
        /// <returns>BIN码</returns>
        public static string? GetBinCode(string? cardNumber)
        {
            if (string.IsNullOrWhiteSpace(cardNumber) || cardNumber.Length < 6)
            {
                return null;
            }

            return cardNumber.Substring(0, 6);
        }

        #endregion

        #region 格式化方法

        /// <summary>
        /// 格式化银行卡号（每4位一组，空格分隔）
        /// </summary>
        /// <param name="cardNumber">银行卡号</param>
        /// <returns>格式化后的卡号</returns>
        public static string? Format(string? cardNumber)
        {
            if (string.IsNullOrWhiteSpace(cardNumber))
            {
                return null;
            }

            // 移除非数字字符
            string cleaned = NonDigitRegex.Replace(cardNumber, "");

            if (!IsValidFormat(cleaned))
            {
                return null;
            }

            // 每4位分组
            var groups = new List<string>();
            for (int i = 0; i < cleaned.Length; i += 4)
            {
                int length = Math.Min(4, cleaned.Length - i);
                groups.Add(cleaned.Substring(i, length));
            }

            return string.Join(" ", groups);
        }

        /// <summary>
        /// 银行卡号脱敏：6222****5678
        /// </summary>
        /// <param name="cardNumber">银行卡号</param>
        /// <returns>脱敏后的卡号</returns>
        public static string? Mask(string? cardNumber)
        {
            if (string.IsNullOrWhiteSpace(cardNumber))
            {
                return null;
            }

            // 移除非数字字符
            string cleaned = NonDigitRegex.Replace(cardNumber, "");

            if (cleaned.Length < 8)
            {
                return null;
            }

            int prefixLength = 4;
            int suffixLength = 4;

            string prefix = cleaned.Substring(0, prefixLength);
            string suffix = cleaned.Substring(cleaned.Length - suffixLength, suffixLength);
            int maskLength = cleaned.Length - prefixLength - suffixLength;

            return prefix + new string('*', maskLength) + suffix;
        }

        #endregion
    }
}
