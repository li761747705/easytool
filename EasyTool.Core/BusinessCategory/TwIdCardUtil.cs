using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace EasyTool.BusinessCategory
{
    /// <summary>
    /// 台湾身份证工具类
    /// </summary>
    public static class TwIdCardUtil
    {
        #region 常量与私有字段

        /// <summary>
        /// 台湾身份证正则表达式
        /// 格式：1个英文字母（县市代码）+ 1位数字（性别）+ 7位数字 + 1位校验码
        /// 例如：A123456789
        /// </summary>
        private static readonly Regex TwIdCardRegex = new(
            @"^[A-Z]\d{9}$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// 首字母对应数值（台湾身份证特殊编码）
        /// </summary>
        private static readonly Dictionary<char, (int Value1, int Value2)> LetterValues = new()
        {
            { 'A', (10, 0) }, { 'B', (11, 1) }, { 'C', (12, 2) }, { 'D', (13, 3) },
            { 'E', (14, 4) }, { 'F', (15, 5) }, { 'G', (16, 6) }, { 'H', (17, 7) },
            { 'I', (34, 4) }, { 'J', (18, 8) }, { 'K', (19, 9) }, { 'L', (20, 0) },
            { 'M', (21, 1) }, { 'N', (22, 2) }, { 'O', (35, 5) }, { 'P', (23, 3) },
            { 'Q', (24, 4) }, { 'R', (25, 5) }, { 'S', (26, 6) }, { 'T', (27, 7) },
            { 'U', (28, 8) }, { 'V', (29, 9) }, { 'W', (32, 2) }, { 'X', (30, 0) },
            { 'Y', (31, 1) }, { 'Z', (33, 3) }
        };

        /// <summary>
        /// 县市代码与名称映射
        /// </summary>
        private static readonly Dictionary<char, string> CountyMap = new()
        {
            { 'A', "台北市" }, { 'B', "台中市" }, { 'C', "基隆市" }, { 'D', "台南市" },
            { 'E', "高雄市" }, { 'F', "台北县" }, { 'G', "宜兰县" }, { 'H', "桃园县" },
            { 'I', "嘉义市" }, { 'J', "新竹县" }, { 'K', "苗栗县" }, { 'L', "台中县" },
            { 'M', "南投县" }, { 'N', "彰化县" }, { 'O', "新竹市" }, { 'P', "云林县" },
            { 'Q', "嘉义县" }, { 'R', "台南县" }, { 'S', "高雄县" }, { 'T', "屏东县" },
            { 'U', "花莲县" }, { 'V', "台东县" }, { 'W', "金门县" }, { 'X', "澎湖县" },
            { 'Y', "阳明山" }, { 'Z', "连江县" }
        };

        #endregion

        #region 验证方法

        /// <summary>
        /// 验证台湾身份证是否有效
        /// </summary>
        /// <param name="idCard">台湾身份证号</param>
        /// <returns>是否有效</returns>
        public static bool IsValid(string? idCard)
        {
            if (string.IsNullOrWhiteSpace(idCard))
            {
                return false;
            }

            string cleaned = idCard.ToUpper().Trim();

            // 检查格式
            if (!TwIdCardRegex.IsMatch(cleaned))
            {
                return false;
            }

            // 检查字母是否有效
            if (!LetterValues.ContainsKey(cleaned[0]))
            {
                return false;
            }

            // 验证校验码
            return ValidateCheckDigit(cleaned);
        }

        /// <summary>
        /// 验证格式是否正确（不校验校验位）
        /// </summary>
        /// <param name="idCard">台湾身份证号</param>
        /// <returns>格式是否正确</returns>
        public static bool IsValidFormat(string? idCard)
        {
            if (string.IsNullOrWhiteSpace(idCard))
            {
                return false;
            }

            string cleaned = idCard.ToUpper().Trim();
            return TwIdCardRegex.IsMatch(cleaned) && LetterValues.ContainsKey(cleaned[0]);
        }

        /// <summary>
        /// 验证校验码
        /// </summary>
        private static bool ValidateCheckDigit(string idCard)
        {
            if (idCard.Length != 10)
            {
                return false;
            }

            char letter = char.ToUpper(idCard[0]);
            if (!LetterValues.TryGetValue(letter, out var values))
            {
                return false;
            }

            // 计算加权和
            int sum = values.Value1;

            // 第2-9位权重为9到1
            int[] weights = { 8, 7, 6, 5, 4, 3, 2, 1 };
            for (int i = 0; i < 8; i++)
            {
                sum += (idCard[i + 1] - '0') * weights[i];
            }

            // 计算校验码
            int remainder = sum % 10;
            int expectedCheck = remainder == 0 ? 0 : 10 - remainder;

            int actualCheck = idCard[9] - '0';

            return expectedCheck == actualCheck;
        }

        #endregion

        #region 信息提取

        /// <summary>
        /// 获取县市名称
        /// </summary>
        /// <param name="idCard">台湾身份证号</param>
        /// <returns>县市名称</returns>
        public static string? GetCounty(string? idCard)
        {
            if (!IsValidFormat(idCard))
            {
                return null;
            }

            char letter = char.ToUpper(idCard![0]);
            return CountyMap.TryGetValue(letter, out string? county) ? county : null;
        }

        /// <summary>
        /// 获取县市代码（首字母）
        /// </summary>
        /// <param name="idCard">台湾身份证号</param>
        /// <returns>县市代码</returns>
        public static char? GetCountyCode(string? idCard)
        {
            if (!IsValidFormat(idCard))
            {
                return null;
            }

            return char.ToUpper(idCard![0]);
        }

        /// <summary>
        /// 获取性别
        /// </summary>
        /// <param name="idCard">台湾身份证号</param>
        /// <returns>性别（1男2女）</returns>
        public static int? GetGender(string? idCard)
        {
            if (!IsValidFormat(idCard))
            {
                return null;
            }

            int genderDigit = idCard![1] - '0';
            // 1为男性，2为女性
            return genderDigit == 1 ? 1 : (genderDigit == 2 ? 2 : null);
        }

        /// <summary>
        /// 获取性别字符串
        /// </summary>
        /// <param name="idCard">台湾身份证号</param>
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

        /// <summary>
        /// 获取数字部分（后9位）
        /// </summary>
        /// <param name="idCard">台湾身份证号</param>
        /// <returns>数字部分</returns>
        public static string? GetDigitPart(string? idCard)
        {
            if (!IsValidFormat(idCard))
            {
                return null;
            }

            return idCard!.Substring(1);
        }

        /// <summary>
        /// 获取校验码（最后一位）
        /// </summary>
        /// <param name="idCard">台湾身份证号</param>
        /// <returns>校验码</returns>
        public static int? GetCheckDigit(string? idCard)
        {
            if (!IsValidFormat(idCard))
            {
                return null;
            }

            return idCard![9] - '0';
        }

        #endregion

        #region 格式化方法

        /// <summary>
        /// 格式化台湾身份证（统一大写）
        /// </summary>
        /// <param name="idCard">台湾身份证号</param>
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
        /// 台湾身份证脱敏：A123****89
        /// </summary>
        /// <param name="idCard">台湾身份证号</param>
        /// <returns>脱敏后的身份证号</returns>
        public static string? Mask(string? idCard)
        {
            if (!IsValidFormat(idCard))
            {
                return null;
            }

            string cleaned = idCard!.ToUpper().Trim();
            // 保留前4位和后2位
            return cleaned.Substring(0, 4) + "****" + cleaned.Substring(8);
        }

        #endregion

        #region 生成方法

        /// <summary>
        /// 生成随机台湾身份证号（仅供测试使用）
        /// </summary>
        /// <param name="countyCode">县市代码（可选，默认随机）</param>
        /// <param name="gender">性别（1男2女，可选）</param>
        /// <returns>台湾身份证号</returns>
        public static string GenerateRandom(char? countyCode = null, int? gender = null)
        {
            const string letters = "ABCDEFGHJKLMNPQRSTUVXYWZIO";
            const string digits = "0123456789";

            // 县市代码
            char letter;
            if (countyCode.HasValue && LetterValues.ContainsKey(char.ToUpper(countyCode.Value)))
            {
                letter = char.ToUpper(countyCode.Value);
            }
            else
            {
                letter = MathCategory.RandomUtil.GetRandomElement(letters.ToCharArray());
            }

            // 性别（第2位）
            int genderDigit;
            if (gender == 1)
            {
                genderDigit = 1;
            }
            else if (gender == 2)
            {
                genderDigit = 2;
            }
            else
            {
                genderDigit = MathCategory.RandomUtil.RandomInt(1, 3);
            }

            // 第3-9位随机数字
            string middleDigits = "";
            for (int i = 0; i < 7; i++)
            {
                middleDigits += MathCategory.RandomUtil.GetRandomElement(digits.ToCharArray());
            }

            // 计算校验码
            string tempId = letter + genderDigit.ToString() + middleDigits + "0";
            char? checkDigit = CalculateCheckDigit(tempId);

            return $"{letter}{genderDigit}{middleDigits}{checkDigit ?? '0'}";
        }

        /// <summary>
        /// 计算校验码
        /// </summary>
        private static char CalculateCheckDigit(string idCard)
        {
            if (idCard.Length < 10)
            {
                return '0';
            }

            char letter = char.ToUpper(idCard[0]);
            if (!LetterValues.TryGetValue(letter, out var values))
            {
                return '0';
            }

            int sum = values.Value1;
            int[] weights = { 8, 7, 6, 5, 4, 3, 2, 1 };

            for (int i = 0; i < 8; i++)
            {
                sum += (idCard[i + 1] - '0') * weights[i];
            }

            int remainder = sum % 10;
            int checkValue = remainder == 0 ? 0 : 10 - remainder;

            return (char)('0' + checkValue);
        }

        #endregion
    }
}
