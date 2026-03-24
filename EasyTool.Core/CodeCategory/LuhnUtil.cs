using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyTool.CodeCategory
{
    /// <summary>
    /// Luhn 校验算法工具类
    /// Luhn 算法是一种简单的校验和算法，用于验证信用卡号、IMEI号、银行卡号等
    /// </summary>
    public static class LuhnUtil
    {
        /// <summary>
        /// 验证数字字符串是否符合 Luhn 算法
        /// </summary>
        /// <param name="number">数字字符串</param>
        /// <returns>是否有效</returns>
        public static bool IsValid(string number)
        {
            if (string.IsNullOrEmpty(number))
                return false;

            // 移除空格和连字符
            number = CleanNumber(number);

            if (!IsAllDigits(number))
                return false;

            int sum = CalculateLuhnSum(number);
            return sum % 10 == 0;
        }

        /// <summary>
        /// 验证数字数组是否符合 Luhn 算法
        /// </summary>
        /// <param name="digits">数字数组</param>
        /// <returns>是否有效</returns>
        public static bool IsValid(int[] digits)
        {
            if (digits == null || digits.Length == 0)
                return false;

            // 验证所有数字都在 0-9 范围内
            foreach (int d in digits)
            {
                if (d < 0 || d > 9)
                    return false;
            }

            int sum = CalculateLuhnSum(digits);
            return sum % 10 == 0;
        }

        /// <summary>
        /// 计算 Luhn 校验位
        /// </summary>
        /// <param name="number">不含校验位的数字字符串</param>
        /// <returns>校验位（0-9）</returns>
        public static int CalculateCheckDigit(string number)
        {
            if (string.IsNullOrEmpty(number))
                throw new ArgumentException("Number cannot be null or empty", nameof(number));

            number = CleanNumber(number);

            if (!IsAllDigits(number))
                throw new ArgumentException("Number must contain only digits", nameof(number));

            return CalculateCheckDigitImpl(number);
        }

        /// <summary>
        /// 计算 Luhn 校验位
        /// </summary>
        /// <param name="digits">不含校验位的数字数组</param>
        /// <returns>校验位（0-9）</returns>
        public static int CalculateCheckDigit(int[] digits)
        {
            if (digits == null || digits.Length == 0)
                throw new ArgumentException("Digits cannot be null or empty", nameof(digits));

            foreach (int d in digits)
            {
                if (d < 0 || d > 9)
                    throw new ArgumentException("All digits must be between 0 and 9", nameof(digits));
            }

            return CalculateCheckDigitImpl(digits);
        }

        /// <summary>
        /// 生成带校验位的完整数字
        /// </summary>
        /// <param name="number">不含校验位的数字字符串</param>
        /// <returns>带校验位的完整数字</returns>
        public static string AppendCheckDigit(string number)
        {
            if (string.IsNullOrEmpty(number))
                throw new ArgumentException("Number cannot be null or empty", nameof(number));

            number = CleanNumber(number);
            int checkDigit = CalculateCheckDigit(number);
            return number + checkDigit;
        }

        /// <summary>
        /// 生成带校验位的完整数字数组
        /// </summary>
        /// <param name="digits">不含校验位的数字数组</param>
        /// <returns>带校验位的完整数字数组</returns>
        public static int[] AppendCheckDigit(int[] digits)
        {
            if (digits == null || digits.Length == 0)
                throw new ArgumentException("Digits cannot be null or empty", nameof(digits));

            int checkDigit = CalculateCheckDigit(digits);
            int[] result = new int[digits.Length + 1];
            Array.Copy(digits, result, digits.Length);
            result[digits.Length] = checkDigit;
            return result;
        }

        /// <summary>
        /// 生成指定长度的有效 Luhn 数字
        /// </summary>
        /// <param name="length">总长度（包括校验位）</param>
        /// <returns>有效的 Luhn 数字字符串</returns>
        public static string Generate(int length)
        {
            if (length < 2)
                throw new ArgumentException("Length must be at least 2", nameof(length));

            var random = new Random();
            var digits = new int[length - 1];

            // 生成随机数字（第一位不能为0）
            digits[0] = random.Next(1, 10);
            for (int i = 1; i < digits.Length; i++)
            {
                digits[i] = random.Next(0, 10);
            }

            return AppendCheckDigit(string.Join("", digits));
        }

        /// <        /// 生成指定前缀的有效 Luhn 数字
        /// </summary>
        /// <param name="prefix">前缀</param>
        /// <param name="totalLength">总长度（包括校验位）</param>
        /// <returns>有效的 Luhn 数字字符串</returns>
        public static string GenerateWithPrefix(string prefix, int totalLength)
        {
            if (string.IsNullOrEmpty(prefix))
                throw new ArgumentException("Prefix cannot be null or empty", nameof(prefix));
            if (totalLength < prefix.Length + 1)
                throw new ArgumentException("Total length must be greater than prefix length", nameof(totalLength));

            prefix = CleanNumber(prefix);
            if (!IsAllDigits(prefix))
                throw new ArgumentException("Prefix must contain only digits", nameof(prefix));

            var random = new Random();
            int remainingLength = totalLength - prefix.Length - 1;
            var sb = new System.Text.StringBuilder(prefix);

            for (int i = 0; i < remainingLength; i++)
            {
                sb.Append(random.Next(0, 10));
            }

            return AppendCheckDigit(sb.ToString());
        }

        /// <summary>
        /// 获取校验位
        /// </summary>
        /// <param name="number">带校验位的完整数字字符串</param>
        /// <returns>校验位</returns>
        public static int GetCheckDigit(string number)
        {
            if (string.IsNullOrEmpty(number))
                throw new ArgumentException("Number cannot be null or empty", nameof(number));

            number = CleanNumber(number);
            if (!IsAllDigits(number))
                throw new ArgumentException("Number must contain only digits", nameof(number));

            return number[number.Length - 1] - '0';
        }

        /// <summary>
        /// 移除校验位
        /// </summary>
        /// <param name="number">带校验位的完整数字字符串</param>
        /// <returns>不含校验位的数字字符串</returns>
        public static string RemoveCheckDigit(string number)
        {
            if (string.IsNullOrEmpty(number))
                throw new ArgumentException("Number cannot be null or empty", nameof(number));

            number = CleanNumber(number);
            if (number.Length < 2)
                throw new ArgumentException("Number must have at least 2 digits", nameof(number));

            return number.Substring(0, number.Length - 1);
        }

        /// <summary>
        /// 计算两个有效 Luhn 数字之间的编辑距离（需要改变多少位才能从一个变成另一个）
        /// </summary>
        /// <param name="number1">第一个数字</param>
        /// <param name="number2">第二个数字</param>
        /// <returns>编辑距离</returns>
        public static int Distance(string number1, string number2)
        {
            number1 = CleanNumber(number1);
            number2 = CleanNumber(number2);

            if (number1.Length != number2.Length)
                throw new ArgumentException("Both numbers must have the same length");

            int distance = 0;
            for (int i = 0; i < number1.Length; i++)
            {
                if (number1[i] != number2[i])
                    distance++;
            }

            return distance;
        }

        /// <summary>
        /// 查找可能的错误（单字符错误）
        /// </summary>
        /// <param name="invalidNumber">无效的数字字符串</param>
        /// <returns>可能的修正列表（位置和正确值）</returns>
        public static List<(int Position, int CorrectDigit)> FindPossibleErrors(string invalidNumber)
        {
            var result = new List<(int Position, int CorrectDigit)>();

            if (string.IsNullOrEmpty(invalidNumber))
                return result;

            invalidNumber = CleanNumber(invalidNumber);
            if (!IsAllDigits(invalidNumber))
                return result;

            var digits = invalidNumber.Select(c => c - '0').ToArray();

            for (int i = 0; i < digits.Length; i++)
            {
                int original = digits[i];
                for (int newDigit = 0; newDigit <= 9; newDigit++)
                {
                    if (newDigit == original)
                        continue;

                    digits[i] = newDigit;
                    if (IsValid(digits))
                    {
                        result.Add((i, newDigit));
                    }
                }
                digits[i] = original;
            }

            return result;
        }

        #region 私有方法

        private static string CleanNumber(string number)
        {
            return number.Replace(" ", "").Replace("-", "").Replace("\t", "");
        }

        private static bool IsAllDigits(string s)
        {
            foreach (char c in s)
            {
                if (c < '0' || c > '9')
                    return false;
            }
            return true;
        }

        private static int CalculateLuhnSum(string number)
        {
            int sum = 0;
            bool doubleDigit = true;

            for (int i = number.Length - 2; i >= 0; i--)
            {
                int digit = number[i] - '0';

                if (doubleDigit)
                {
                    digit *= 2;
                    if (digit > 9)
                        digit -= 9;
                }

                sum += digit;
                doubleDigit = !doubleDigit;
            }

            // 加上校验位
            sum += number[number.Length - 1] - '0';

            return sum;
        }

        private static int CalculateLuhnSum(int[] digits)
        {
            int sum = 0;
            bool doubleDigit = true;

            for (int i = digits.Length - 2; i >= 0; i--)
            {
                int digit = digits[i];

                if (doubleDigit)
                {
                    digit *= 2;
                    if (digit > 9)
                        digit -= 9;
                }

                sum += digit;
                doubleDigit = !doubleDigit;
            }

            sum += digits[digits.Length - 1];

            return sum;
        }

        private static int CalculateCheckDigitImpl(string number)
        {
            int sum = 0;
            bool doubleDigit = false;

            for (int i = number.Length - 1; i >= 0; i--)
            {
                int digit = number[i] - '0';

                if (doubleDigit)
                {
                    digit *= 2;
                    if (digit > 9)
                        digit -= 9;
                }

                sum += digit;
                doubleDigit = !doubleDigit;
            }

            return (10 - (sum % 10)) % 10;
        }

        private static int CalculateCheckDigitImpl(int[] digits)
        {
            int sum = 0;
            bool doubleDigit = false;

            for (int i = digits.Length - 1; i >= 0; i--)
            {
                int digit = digits[i];

                if (doubleDigit)
                {
                    digit *= 2;
                    if (digit > 9)
                        digit -= 9;
                }

                sum += digit;
                doubleDigit = !doubleDigit;
            }

            return (10 - (sum % 10)) % 10;
        }

        #endregion
    }
}
