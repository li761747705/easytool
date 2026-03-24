using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace EasyTool.BusinessCategory
{
    /// <summary>
    /// 社保号工具类
    /// </summary>
    public static class SocialSecurityUtil
    {
        #region 常量与私有字段

        /// <summary>
        /// 社保号正则表达式（18位，与身份证号格式相同）
        /// </summary>
        private static readonly Regex SSN18Regex = new(
            @"^[1-9]\d{5}(19|20)\d{2}(0[1-9]|1[0-2])(0[1-9]|[12]\d|3[01])\d{3}[\dXx]$",
            RegexOptions.Compiled);

        /// <summary>
        /// 社保号正则表达式（部分省市为15位或16位）
        /// </summary>
        private static readonly Regex SSN15Regex = new(@"^\d{15,16}$", RegexOptions.Compiled);

        /// <summary>
        /// 社会保障卡号正则（带字母）
        /// </summary>
        private static readonly Regex SSNCardRegex = new(
            @"^[A-Za-z]\d{17}$",
            RegexOptions.Compiled);

        /// <summary>
        /// 校验码权重
        /// </summary>
        private static readonly int[] Weights = { 7, 9, 10, 5, 8, 4, 2, 1, 6, 3, 7, 9, 10, 5, 8, 4, 2 };

        /// <summary>
        /// 校验码对照表
        /// </summary>
        private static readonly char[] CheckCodes = { '1', '0', 'X', '9', '8', '7', '6', '5', '4', '3', '2' };

        /// <summary>
        /// 省份社保号规则（简化版）
        /// </summary>
        private static readonly Dictionary<string, int> ProvinceLengthMap = new()
        {
            { "北京", 18 }, { "上海", 18 }, { "天津", 18 }, { "重庆", 18 },
            { "广东", 18 }, { "浙江", 18 }, { "江苏", 18 }, { "山东", 18 },
            { "四川", 18 }, { "湖北", 18 }, { "河南", 18 }, { "河北", 18 },
            { "福建", 18 }, { "安徽", 18 }, { "辽宁", 18 }, { "陕西", 18 },
            { "湖南", 18 }, { "江西", 18 }, { "云南", 18 }, { "贵州", 18 },
            { "甘肃", 18 }, { "青海", 18 }, { "宁夏", 18 }, { "新疆", 18 },
            { "西藏", 18 }, { "内蒙古", 18 }, { "广西", 18 }, { "黑龙江", 18 },
            { "吉林", 18 }, { "山西", 18 }, { "海南", 18 }
        };

        #endregion

        #region 验证方法

        /// <summary>
        /// 验证社保号是否有效
        /// </summary>
        /// <param name="ssn">社保号</param>
        /// <returns>是否有效</returns>
        public static bool IsValid(string? ssn)
        {
            if (string.IsNullOrWhiteSpace(ssn))
            {
                return false;
            }

            string cleaned = ssn.Trim().ToUpper();

            // 18位（身份证号格式）
            if (cleaned.Length == 18 && SSN18Regex.IsMatch(cleaned))
            {
                return ValidateCheckDigit(cleaned);
            }

            // 15-16位纯数字
            if (SSN15Regex.IsMatch(cleaned))
            {
                return true;
            }

            // 带字母的卡号
            if (SSNCardRegex.IsMatch(cleaned))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 验证是否为18位社保号（身份证号格式）
        /// </summary>
        /// <param name="ssn">社保号</param>
        /// <returns>是否为18位</returns>
        public static bool Is18Digit(string? ssn)
        {
            if (string.IsNullOrWhiteSpace(ssn))
            {
                return false;
            }

            string cleaned = ssn.Trim().ToUpper();
            return cleaned.Length == 18 && SSN18Regex.IsMatch(cleaned) && ValidateCheckDigit(cleaned);
        }

        /// <summary>
        /// 验证格式是否正确（不校验校验位）
        /// </summary>
        /// <param name="ssn">社保号</param>
        /// <returns>格式是否正确</returns>
        public static bool IsValidFormat(string? ssn)
        {
            if (string.IsNullOrWhiteSpace(ssn))
            {
                return false;
            }

            string cleaned = ssn.Trim().ToUpper();
            return SSN18Regex.IsMatch(cleaned) || SSN15Regex.IsMatch(cleaned) || SSNCardRegex.IsMatch(cleaned);
        }

        /// <summary>
        /// 验证校验位
        /// </summary>
        private static bool ValidateCheckDigit(string ssn)
        {
            if (ssn.Length != 18) return false;

            int sum = 0;
            for (int i = 0; i < 17; i++)
            {
                sum += (ssn[i] - '0') * Weights[i];
            }

            char expectedCheckCode = CheckCodes[sum % 11];
            return char.ToUpper(ssn[17]) == expectedCheckCode;
        }

        #endregion

        #region 信息提取

        /// <summary>
        /// 获取出生日期（仅18位格式）
        /// </summary>
        /// <param name="ssn">社保号</param>
        /// <returns>出生日期</returns>
        public static DateTime? GetBirthday(string? ssn)
        {
            if (!Is18Digit(ssn))
            {
                return null;
            }

            string cleaned = ssn!.Trim();
            int year = int.Parse(cleaned.Substring(6, 4));
            int month = int.Parse(cleaned.Substring(10, 2));
            int day = int.Parse(cleaned.Substring(12, 2));

            return new DateTime(year, month, day);
        }

        /// <summary>
        /// 获取性别（仅18位格式）
        /// </summary>
        /// <param name="ssn">社保号</param>
        /// <returns>性别（1男2女）</returns>
        public static int? GetGender(string? ssn)
        {
            if (!Is18Digit(ssn))
            {
                return null;
            }

            int genderDigit = ssn![16] - '0';
            return genderDigit % 2 == 1 ? 1 : 2;
        }

        /// <summary>
        /// 获取性别字符串
        /// </summary>
        /// <param name="ssn">社保号</param>
        /// <returns>性别</returns>
        public static string? GetGenderString(string? ssn)
        {
            int? gender = GetGender(ssn);
            return gender switch
            {
                1 => "男",
                2 => "女",
                _ => null
            };
        }

        /// <summary>
        /// 获取行政区划代码（仅18位格式）
        /// </summary>
        /// <param name="ssn">社保号</param>
        /// <returns>行政区划代码</returns>
        public static string? GetAreaCode(string? ssn)
        {
            if (!Is18Digit(ssn))
            {
                return null;
            }

            return ssn!.Substring(0, 6);
        }

        /// <summary>
        /// 获取年龄（仅18位格式）
        /// </summary>
        /// <param name="ssn">社保号</param>
        /// <returns>年龄</returns>
        public static int? GetAge(string? ssn)
        {
            DateTime? birthday = GetBirthday(ssn);
            if (!birthday.HasValue)
            {
                return null;
            }

            DateTime today = DateTime.Today;
            int age = today.Year - birthday.Value.Year;
            if (today < birthday.Value.AddYears(age))
            {
                age--;
            }

            return age;
        }

        #endregion

        #region 格式化方法

        /// <summary>
        /// 格式化社保号
        /// </summary>
        /// <param name="ssn">社保号</param>
        /// <returns>格式化后的社保号</returns>
        public static string? Normalize(string? ssn)
        {
            if (string.IsNullOrWhiteSpace(ssn))
            {
                return null;
            }

            string cleaned = ssn.Trim().ToUpper();
            return IsValidFormat(cleaned) ? cleaned : null;
        }

        /// <summary>
        /// 社保号脱敏：110***********1234
        /// </summary>
        /// <param name="ssn">社保号</param>
        /// <returns>脱敏后的社保号</returns>
        public static string? Mask(string? ssn)
        {
            if (!IsValid(ssn))
            {
                return null;
            }

            string cleaned = ssn!.Trim().ToUpper();

            if (cleaned.Length == 18)
            {
                return cleaned.Substring(0, 3) + "***********" + cleaned.Substring(14);
            }

            if (cleaned.Length >= 15)
            {
                int prefixLen = 3;
                int suffixLen = 4;
                return cleaned.Substring(0, prefixLen) +
                       new string('*', cleaned.Length - prefixLen - suffixLen) +
                       cleaned.Substring(cleaned.Length - suffixLen);
            }

            return cleaned[0] + new string('*', cleaned.Length - 2) + cleaned[^1];
        }

        #endregion
    }
}
