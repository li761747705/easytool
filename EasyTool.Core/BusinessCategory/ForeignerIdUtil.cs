using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace EasyTool.BusinessCategory
{
    /// <summary>
    /// 外国人永久居留身份证工具类
    /// </summary>
    public static class ForeignerIdUtil
    {
        #region 常量与私有字段

        /// <summary>
        /// 外国人永久居留身份证正则表达式（15位）
        /// </summary>
        private static readonly Regex ForeignerId15Regex = new(
            @"^[A-Z]{3}\d{12}$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// 新版外国人永久居留身份证正则表达式（18位）
        /// 格式与普通身份证相同，但用于外国人
        /// </summary>
        private static readonly Regex ForeignerId18Regex = new(
            @"^[A-Z]\d{17}$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// 校验码权重（18位版本）
        /// </summary>
        private static readonly int[] Weights = { 7, 9, 10, 5, 8, 4, 2, 1, 6, 3, 7, 9, 10, 5, 8, 4, 2 };

        /// <summary>
        /// 校验码对照表（18位版本）
        /// </summary>
        private static readonly char[] CheckCodes = { '1', '0', 'X', '9', '8', '7', '6', '5', '4', '3', '2' };

        /// <summary>
        /// 国籍代码映射（部分常见国家）
        /// </summary>
        private static readonly Dictionary<string, string> NationalityMap = new(StringComparer.OrdinalIgnoreCase)
        {
            { "USA", "美国" }, { "GBR", "英国" }, { "JPN", "日本" }, { "KOR", "韩国" },
            { "DEU", "德国" }, { "FRA", "法国" }, { "ITA", "意大利" }, { "ESP", "西班牙" },
            { "CAN", "加拿大" }, { "AUS", "澳大利亚" }, { "NZL", "新西兰" }, { "RUS", "俄罗斯" },
            { "IND", "印度" }, { "THA", "泰国" }, { "VNM", "越南" }, { "MYS", "马来西亚" },
            { "SGP", "新加坡" }, { "IDN", "印度尼西亚" }, { "PHL", "菲律宾" }, { "MMR", "缅甸" },
            { "PAK", "巴基斯坦" }, { "BGD", "孟加拉国" }, { "BRA", "巴西" }, { "MEX", "墨西哥" },
            { "ZAF", "南非" }, { "EGY", "埃及" }, { "NGA", "尼日利亚" }, { "KEN", "肯尼亚" },
            { "CHN", "中国" }, { "HKG", "香港" }, { "MAC", "澳门" }, { "TWN", "台湾" }
        };

        /// <summary>
        /// 省份代码映射
        /// </summary>
        private static readonly Dictionary<string, string> ProvinceCodeMap = new()
        {
            { "11", "北京" }, { "12", "天津" }, { "13", "河北" }, { "14", "山西" },
            { "15", "内蒙古" }, { "21", "辽宁" }, { "22", "吉林" }, { "23", "黑龙江" },
            { "31", "上海" }, { "32", "江苏" }, { "33", "浙江" }, { "34", "安徽" },
            { "35", "福建" }, { "36", "江西" }, { "37", "山东" }, { "41", "河南" },
            { "42", "湖北" }, { "43", "湖南" }, { "44", "广东" }, { "45", "广西" },
            { "46", "海南" }, { "50", "重庆" }, { "51", "四川" }, { "52", "贵州" },
            { "53", "云南" }, { "54", "西藏" }, { "61", "陕西" }, { "62", "甘肃" },
            { "63", "青海" }, { "64", "宁夏" }, { "65", "新疆" }
        };

        #endregion

        #region 验证方法

        /// <summary>
        /// 验证外国人永久居留身份证是否有效
        /// </summary>
        /// <param name="idCard">外国人永久居留身份证号</param>
        /// <returns>是否有效</returns>
        public static bool IsValid(string? idCard)
        {
            if (string.IsNullOrWhiteSpace(idCard))
            {
                return false;
            }

            string cleaned = idCard.ToUpper().Trim();

            // 15位格式（旧版）
            if (cleaned.Length == 15 && ForeignerId15Regex.IsMatch(cleaned))
            {
                return true;
            }

            // 18位格式（新版）
            if (cleaned.Length == 18 && ForeignerId18Regex.IsMatch(cleaned))
            {
                return ValidateCheckDigit18(cleaned);
            }

            return false;
        }

        /// <summary>
        /// 验证格式是否正确（不校验校验位）
        /// </summary>
        /// <param name="idCard">外国人永久居留身份证号</param>
        /// <returns>格式是否正确</returns>
        public static bool IsValidFormat(string? idCard)
        {
            if (string.IsNullOrWhiteSpace(idCard))
            {
                return false;
            }

            string cleaned = idCard.ToUpper().Trim();
            return ForeignerId15Regex.IsMatch(cleaned) || ForeignerId18Regex.IsMatch(cleaned);
        }

        /// <summary>
        /// 验证是否为15位格式
        /// </summary>
        /// <param name="idCard">外国人永久居留身份证号</param>
        /// <returns>是否为15位格式</returns>
        public static bool Is15Digit(string? idCard)
        {
            if (string.IsNullOrWhiteSpace(idCard))
            {
                return false;
            }

            return ForeignerId15Regex.IsMatch(idCard.ToUpper().Trim());
        }

        /// <summary>
        /// 验证是否为18位格式
        /// </summary>
        /// <param name="idCard">外国人永久居留身份证号</param>
        /// <returns>是否为18位格式</returns>
        public static bool Is18Digit(string? idCard)
        {
            if (string.IsNullOrWhiteSpace(idCard))
            {
                return false;
            }

            string cleaned = idCard.ToUpper().Trim();
            return cleaned.Length == 18 && ForeignerId18Regex.IsMatch(cleaned);
        }

        /// <summary>
        /// 验证18位校验码
        /// </summary>
        private static bool ValidateCheckDigit18(string idCard)
        {
            if (idCard.Length != 18)
            {
                return false;
            }

            // 字母转换（A=10, B=11, ..., Z=35）
            char firstChar = char.ToUpper(idCard[0]);
            int firstValue;
            if (firstChar >= 'A' && firstChar <= 'Z')
            {
                firstValue = firstChar - 'A' + 10;
            }
            else
            {
                return false;
            }

            // 计算加权和
            int sum = 0;

            // 第一位字母的权重处理
            sum += (firstValue / 10) * Weights[0];
            sum += (firstValue % 10) * Weights[1];

            // 数字部分
            for (int i = 1; i < 17; i++)
            {
                if (!char.IsDigit(idCard[i]))
                {
                    return false;
                }
                sum += (idCard[i] - '0') * Weights[i + 1];
            }

            // 计算校验码
            char expectedCheck = CheckCodes[sum % 11];
            return char.ToUpper(idCard[17]) == expectedCheck;
        }

        #endregion

        #region 信息提取

        /// <summary>
        /// 获取国籍代码（15位格式前3位）
        /// </summary>
        /// <param name="idCard">外国人永久居留身份证号</param>
        /// <returns>国籍代码</returns>
        public static string? GetNationalityCode(string? idCard)
        {
            if (Is15Digit(idCard))
            {
                return idCard!.Substring(0, 3).ToUpper();
            }

            return null;
        }

        /// <summary>
        /// 获取国籍名称
        /// </summary>
        /// <param name="idCard">外国人永久居留身份证号</param>
        /// <returns>国籍名称</returns>
        public static string? GetNationality(string? idCard)
        {
            string? code = GetNationalityCode(idCard);
            if (code == null)
            {
                return null;
            }

            return NationalityMap.TryGetValue(code, out string? nationality) ? nationality : code;
        }

        /// <summary>
        /// 获取省份代码（18位格式的第2-3位）
        /// </summary>
        /// <param name="idCard">外国人永久居留身份证号</param>
        /// <returns>省份代码</returns>
        public static string? GetProvinceCode(string? idCard)
        {
            if (!Is18Digit(idCard))
            {
                return null;
            }

            return idCard!.Substring(1, 2);
        }

        /// <summary>
        /// 获取省份名称
        /// </summary>
        /// <param name="idCard">外国人永久居留身份证号</param>
        /// <returns>省份名称</returns>
        public static string? GetProvince(string? idCard)
        {
            string? code = GetProvinceCode(idCard);
            if (code == null)
            {
                return null;
            }

            return ProvinceCodeMap.TryGetValue(code, out string? province) ? province : null;
        }

        /// <summary>
        /// 获取出生日期（18位格式）
        /// </summary>
        /// <param name="idCard">外国人永久居留身份证号</param>
        /// <returns>出生日期</returns>
        public static DateTime? GetBirthday(string? idCard)
        {
            if (!Is18Digit(idCard))
            {
                return null;
            }

            string cleaned = idCard!.Substring(1); // 去掉首字母
            int year = int.Parse(cleaned.Substring(5, 4));
            int month = int.Parse(cleaned.Substring(9, 2));
            int day = int.Parse(cleaned.Substring(11, 2));

            try
            {
                return new DateTime(year, month, day);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 获取性别（18位格式）
        /// </summary>
        /// <param name="idCard">外国人永久居留身份证号</param>
        /// <returns>性别（1男2女）</returns>
        public static int? GetGender(string? idCard)
        {
            if (!Is18Digit(idCard))
            {
                return null;
            }

            int genderDigit = idCard![16] - '0';
            return genderDigit % 2 == 1 ? 1 : 2;
        }

        /// <summary>
        /// 获取性别字符串
        /// </summary>
        /// <param name="idCard">外国人永久居留身份证号</param>
        /// <returns>性别</returns>
        public static string? GetGenderString(string? idCard)
        {
            int? gender = GetGender(idCard);
            return gender switch
            {
                1 => "男",
                2 => "女",
                _ => null
            };
        }

        #endregion

        #region 格式化方法

        /// <summary>
        /// 格式化外国人永久居留身份证（统一大写）
        /// </summary>
        /// <param name="idCard">外国人永久居留身份证号</param>
        /// <returns>格式化后的身份证号</returns>
        public static string? Normalize(string? idCard)
        {
            if (!IsValidFormat(idCard))
            {
                return null;
            }

            return idCard!.ToUpper().Trim();
        }

        /// <summary>
        /// 外国人永久居留身份证脱敏
        /// 15位：USA*********
        /// 18位：A110**********1
        /// </summary>
        /// <param name="idCard">外国人永久居留身份证号</param>
        /// <returns>脱敏后的身份证号</returns>
        public static string? Mask(string? idCard)
        {
            if (!IsValidFormat(idCard))
            {
                return null;
            }

            string cleaned = idCard!.ToUpper().Trim();

            if (cleaned.Length == 15)
            {
                return cleaned.Substring(0, 3) + "***********" + cleaned.Substring(14);
            }

            if (cleaned.Length == 18)
            {
                return cleaned.Substring(0, 4) + "***********" + cleaned.Substring(15);
            }

            return null;
        }

        #endregion
    }
}
