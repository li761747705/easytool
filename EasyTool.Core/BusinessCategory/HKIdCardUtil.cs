using System;
using System.Text.RegularExpressions;

namespace EasyTool.BusinessCategory
{
    /// <summary>
    /// 香港身份证工具类
    /// </summary>
    public static class HKIdCardUtil
    {
        #region 常量与私有字段

        /// <summary>
        /// 香港身份证正则表达式
        /// 格式：1-2个英文字母 + 6位数字 + 括号内1位校验码
        /// 例如：A123456(7), AB123456(7)
        /// </summary>
        private static readonly Regex HKIdCardRegex = new(
            @"^[A-Z]{1,2}\d{6}\([\dA]\)$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// 香港身份证前缀与含义映射
        /// </summary>
        private static readonly string[] PrefixMeanings = new string[]
        {
            "A", "B", "C", "D", "E", "F", "G", "H", "J", "K", "L", "M", "N", "P", "R", "S", "T", "V", "W", "Y", "Z"
        };

        /// <summary>
        /// 首字母对应的数字值（A=1, B=2, ..., Z=26）
        /// </summary>
        private static int GetLetterValue(char letter)
        {
            return char.ToUpper(letter) - 'A' + 1;
        }

        #endregion

        #region 验证方法

        /// <summary>
        /// 验证香港身份证是否有效
        /// </summary>
        /// <param name="idCard">香港身份证号</param>
        /// <returns>是否有效</returns>
        public static bool IsValid(string? idCard)
        {
            if (string.IsNullOrWhiteSpace(idCard))
            {
                return false;
            }

            string cleaned = idCard.ToUpper().Trim();

            // 检查格式
            if (!HKIdCardRegex.IsMatch(cleaned))
            {
                return false;
            }

            // 验证校验码
            return ValidateCheckDigit(cleaned);
        }

        /// <summary>
        /// 验证格式是否正确（不校验校验位）
        /// </summary>
        /// <param name="idCard">香港身份证号</param>
        /// <returns>格式是否正确</returns>
        public static bool IsValidFormat(string? idCard)
        {
            if (string.IsNullOrWhiteSpace(idCard))
            {
                return false;
            }

            return HKIdCardRegex.IsMatch(idCard.ToUpper().Trim());
        }

        /// <summary>
        /// 验证校验码
        /// </summary>
        private static bool ValidateCheckDigit(string idCard)
        {
            // 提取校验码（括号内的字符）
            int parenStart = idCard.IndexOf('(');
            int parenEnd = idCard.IndexOf(')');
            if (parenStart < 0 || parenEnd < 0 || parenEnd <= parenStart)
            {
                return false;
            }

            string checkChar = idCard.Substring(parenStart + 1, parenEnd - parenStart - 1);
            if (checkChar.Length != 1)
            {
                return false;
            }

            // 计算校验码
            char? expectedCheck = CalculateCheckDigit(idCard);
            if (expectedCheck == null)
            {
                return false;
            }

            return char.ToUpper(checkChar[0]) == expectedCheck.Value;
        }

        /// <summary>
        /// 计算校验码
        /// </summary>
        /// <param name="idCard">香港身份证号（含括号格式）</param>
        /// <returns>校验码字符</returns>
        public static char? CalculateCheckDigit(string? idCard)
        {
            if (string.IsNullOrWhiteSpace(idCard))
            {
                return null;
            }

            string cleaned = idCard.ToUpper().Trim();

            // 提取字母部分和数字部分
            int digitStart = -1;
            for (int i = 0; i < cleaned.Length; i++)
            {
                if (char.IsDigit(cleaned[i]))
                {
                    digitStart = i;
                    break;
                }
            }

            if (digitStart < 0)
            {
                return null;
            }

            string letters = cleaned.Substring(0, digitStart);
            string digits = cleaned.Substring(digitStart, 6);

            // 计算加权和
            int sum = 0;
            int weight = 9 - (2 - letters.Length); // 根据字母数量调整起始权重

            // 如果只有一个字母，第一位按36处理（相当于前面有一个空位，值为36）
            if (letters.Length == 1)
            {
                sum += 36 * 9;
                sum += GetLetterValue(letters[0]) * 8;
            }
            else if (letters.Length == 2)
            {
                sum += GetLetterValue(letters[0]) * 9;
                sum += GetLetterValue(letters[1]) * 8;
            }
            else
            {
                return null;
            }

            // 数字部分权重为7到2
            int[] digitWeights = { 7, 6, 5, 4, 3, 2 };
            for (int i = 0; i < 6; i++)
            {
                sum += (digits[i] - '0') * digitWeights[i];
            }

            // 计算校验码
            int remainder = sum % 11;
            int checkValue;

            if (remainder == 0)
            {
                checkValue = 0;
            }
            else
            {
                checkValue = 11 - remainder;
            }

            // 返回校验码字符
            if (checkValue == 10)
            {
                return 'A';
            }
            else
            {
                return (char)('0' + checkValue);
            }
        }

        #endregion

        #region 信息提取

        /// <summary>
        /// 获取身份证前缀字母
        /// </summary>
        /// <param name="idCard">香港身份证号</param>
        /// <returns>前缀字母</returns>
        public static string? GetPrefix(string? idCard)
        {
            if (!IsValidFormat(idCard))
            {
                return null;
            }

            string cleaned = idCard!.ToUpper().Trim();
            int digitStart = -1;
            for (int i = 0; i < cleaned.Length; i++)
            {
                if (char.IsDigit(cleaned[i]))
                {
                    digitStart = i;
                    break;
                }
            }

            return digitStart > 0 ? cleaned.Substring(0, digitStart) : null;
        }

        /// <summary>
        /// 获取数字部分（6位）
        /// </summary>
        /// <param name="idCard">香港身份证号</param>
        /// <returns>6位数字</returns>
        public static string? GetDigitPart(string? idCard)
        {
            if (!IsValidFormat(idCard))
            {
                return null;
            }

            string cleaned = idCard!.ToUpper().Trim();
            int digitStart = -1;
            for (int i = 0; i < cleaned.Length; i++)
            {
                if (char.IsDigit(cleaned[i]))
                {
                    digitStart = i;
                    break;
                }
            }

            return digitStart >= 0 ? cleaned.Substring(digitStart, 6) : null;
        }

        /// <summary>
        /// 获取校验码
        /// </summary>
        /// <param name="idCard">香港身份证号</param>
        /// <returns>校验码字符</returns>
        public static char? GetCheckDigit(string? idCard)
        {
            if (!IsValidFormat(idCard))
            {
                return null;
            }

            string cleaned = idCard!.ToUpper().Trim();
            int parenStart = cleaned.IndexOf('(');
            int parenEnd = cleaned.IndexOf(')');

            if (parenStart < 0 || parenEnd < 0 || parenEnd <= parenStart)
            {
                return null;
            }

            return cleaned[parenStart + 1];
        }

        #endregion

        #region 格式化方法

        /// <summary>
        /// 格式化香港身份证（统一大写，带括号）
        /// </summary>
        /// <param name="idCard">香港身份证号</param>
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
        /// 格式化为标准格式（确保括号正确）
        /// </summary>
        /// <param name="idCard">香港身份证号</param>
        /// <returns>标准格式的身份证号</returns>
        public static string? Format(string? idCard)
        {
            if (!IsValid(idCard))
            {
                return null;
            }

            string cleaned = idCard!.ToUpper().Trim();
            return cleaned;
        }

        /// <summary>
        /// 香港身份证脱敏：A12***(7)
        /// </summary>
        /// <param name="idCard">香港身份证号</param>
        /// <returns>脱敏后的身份证号</returns>
        public static string? Mask(string? idCard)
        {
            if (!IsValidFormat(idCard))
            {
                return null;
            }

            string cleaned = idCard!.ToUpper().Trim();
            int parenStart = cleaned.IndexOf('(');

            if (parenStart < 7)
            {
                return null;
            }

            // 保留前缀+2位数字，中间用*替代，保留校验码
            string prefix = cleaned.Substring(0, parenStart - 4);
            string suffix = cleaned.Substring(parenStart);

            return prefix + "****" + suffix;
        }

        #endregion

        #region 生成方法

        /// <summary>
        /// 生成随机香港身份证号（仅供测试使用）
        /// </summary>
        /// <param name="prefix">前缀字母（可选，默认随机）</param>
        /// <returns>香港身份证号</returns>
        public static string GenerateRandom(string? prefix = null)
        {
            const string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string digits = "0123456789";

            // 前缀
            string prefixLetters;
            if (string.IsNullOrEmpty(prefix))
            {
                int letterCount = MathCategory.RandomUtil.RandomInt(1, 3);
                prefixLetters = "";
                for (int i = 0; i < letterCount; i++)
                {
                    prefixLetters += MathCategory.RandomUtil.GetRandomElement(letters.ToCharArray());
                }
            }
            else
            {
                prefixLetters = prefix.ToUpper();
            }

            // 6位数字
            string numberPart = "";
            for (int i = 0; i < 6; i++)
            {
                numberPart += MathCategory.RandomUtil.GetRandomElement(digits.ToCharArray());
            }

            // 计算校验码
            string tempId = prefixLetters + numberPart + "(0)";
            char? checkDigit = CalculateCheckDigit(tempId);

            return $"{prefixLetters}{numberPart}({checkDigit ?? '0'})";
        }

        #endregion
    }
}
