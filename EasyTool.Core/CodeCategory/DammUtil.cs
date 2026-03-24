using System;

namespace EasyTool.CodeCategory
{
    /// <summary>
    /// Damm 算法校验和工具类
    /// Damm 算法是一种检测单个数字错误的校验和算法
    /// 由 H. Michael Damm 发明，类似于 Verhoeff 算法
    /// 使用弱完全反对称群（quasigroup）
    /// </summary>
    public static class DammUtil
    {
        // Damm 算法的运算表（10x10 弱完全反对称群）
        private static readonly byte[,] Matrix = new byte[,]
        {
            {0, 3, 1, 7, 5, 9, 8, 6, 4, 2},
            {7, 0, 9, 2, 1, 5, 4, 8, 6, 3},
            {4, 2, 0, 6, 8, 7, 1, 3, 5, 9},
            {1, 7, 5, 0, 9, 8, 3, 4, 2, 6},
            {6, 1, 2, 3, 0, 4, 5, 9, 7, 8},
            {3, 6, 7, 4, 2, 0, 9, 5, 8, 1},
            {5, 8, 6, 9, 7, 2, 0, 1, 3, 4},
            {8, 9, 4, 5, 3, 6, 2, 0, 1, 7},
            {9, 4, 3, 8, 6, 1, 7, 2, 0, 5},
            {2, 5, 8, 1, 4, 3, 6, 7, 9, 0}
        };

        /// <summary>
        /// 计算数字字符串的 Damm 校验位
        /// </summary>
        /// <param name="number">数字字符串</param>
        /// <returns>校验位（0-9）</returns>
        public static int CalculateCheckDigit(string number)
        {
            if (string.IsNullOrEmpty(number))
                throw new ArgumentException("Number cannot be empty", nameof(number));

            return CalculateCheckDigit(GetDigits(number));
        }

        /// <summary>
        /// 计算数字数组的 Damm 校验位
        /// </summary>
        /// <param name="digits">数字数组</param>
        /// <returns>校验位（0-9）</returns>
        public static int CalculateCheckDigit(int[] digits)
        {
            if (digits == null || digits.Length == 0)
                throw new ArgumentException("Digits cannot be empty", nameof(digits));

            int interim = 0;

            foreach (int digit in digits)
            {
                if (digit < 0 || digit > 9)
                    throw new ArgumentException($"Invalid digit: {digit}", nameof(digits));

                interim = Matrix[interim, digit];
            }

            return interim;
        }

        /// <summary>
        /// 生成带校验位的数字字符串
        /// </summary>
        /// <param name="number">原始数字字符串</param>
        /// <returns>带校验位的数字字符串</returns>
        public static string AppendCheckDigit(string number)
        {
            if (string.IsNullOrEmpty(number))
                throw new ArgumentException("Number cannot be empty", nameof(number));

            int checkDigit = CalculateCheckDigit(number);
            return number + checkDigit;
        }

        /// <summary>
        /// 验证带校验位的数字字符串是否有效
        /// </summary>
        /// <param name="numberWithCheckDigit">带校验位的数字字符串</param>
        /// <returns>是否有效</returns>
        public static bool Validate(string numberWithCheckDigit)
        {
            if (string.IsNullOrEmpty(numberWithCheckDigit) || numberWithCheckDigit.Length < 2)
                return false;

            return Validate(GetDigits(numberWithCheckDigit));
        }

        /// <summary>
        /// 验证带校验位的数字数组是否有效
        /// </summary>
        /// <param name="digitsWithCheckDigit">带校验位的数字数组</param>
        /// <returns>是否有效</returns>
        public static bool Validate(int[] digitsWithCheckDigit)
        {
            if (digitsWithCheckDigit == null || digitsWithCheckDigit.Length < 2)
                return false;

            int interim = 0;

            foreach (int digit in digitsWithCheckDigit)
            {
                if (digit < 0 || digit > 9)
                    return false;

                interim = Matrix[interim, digit];
            }

            return interim == 0;
        }

        /// <summary>
        /// 从带校验位的字符串中提取原始数字
        /// </summary>
        /// <param name="numberWithCheckDigit">带校验位的数字字符串</param>
        /// <returns>原始数字字符串，如果无效则返回 null</returns>
        public static string ExtractNumber(string numberWithCheckDigit)
        {
            if (!Validate(numberWithCheckDigit))
                return null;

            return numberWithCheckDigit.Substring(0, numberWithCheckDigit.Length - 1);
        }

        /// <summary>
        /// 获取校验位
        /// </summary>
        /// <param name="numberWithCheckDigit">带校验位的数字字符串</param>
        /// <returns>校验位，如果格式无效则返回 -1</returns>
        public static int GetCheckDigit(string numberWithCheckDigit)
        {
            if (string.IsNullOrEmpty(numberWithCheckDigit) || numberWithCheckDigit.Length < 2)
                return -1;

            if (!int.TryParse(numberWithCheckDigit[numberWithCheckDigit.Length - 1].ToString(), out int digit))
                return -1;

            return digit;
        }

        /// <summary>
        /// 生成随机数字序列并添加校验位
        /// </summary>
        /// <param name="length">数字序列长度（不含校验位）</param>
        /// <returns>带校验位的随机数字字符串</returns>
        public static string GenerateRandom(int length)
        {
            if (length < 1)
                throw new ArgumentException("Length must be at least 1", nameof(length));

            var random = new Random();
            var digits = new int[length];

            for (int i = 0; i < length; i++)
            {
                digits[i] = random.Next(10);
            }

            int checkDigit = CalculateCheckDigit(digits);

            var result = new System.Text.StringBuilder(length + 1);
            foreach (int digit in digits)
            {
                result.Append(digit);
            }
            result.Append(checkDigit);

            return result.ToString();
        }

        /// <summary>
        /// 批量验证多个数字字符串
        /// </summary>
        /// <param name="numbers">数字字符串数组</param>
        /// <returns>验证结果数组</returns>
        public static bool[] ValidateBatch(string[] numbers)
        {
            if (numbers == null)
                throw new ArgumentNullException(nameof(numbers));

            var results = new bool[numbers.Length];
            for (int i = 0; i < numbers.Length; i++)
            {
                results[i] = Validate(numbers[i]);
            }
            return results;
        }

        /// <summary>
        /// 检测并纠正单个数字错误
        /// </summary>
        /// <param name="numberWithCheckDigit">带校验位的数字字符串</param>
        /// <returns>纠正后的字符串，如果无法纠正则返回 null</returns>
        public static string DetectAndCorrect(string numberWithCheckDigit)
        {
            if (string.IsNullOrEmpty(numberWithCheckDigit) || numberWithCheckDigit.Length < 2)
                return null;

            // 首先验证
            if (Validate(numberWithCheckDigit))
                return numberWithCheckDigit;

            // 尝试纠正每个位置的错误
            for (int pos = 0; pos < numberWithCheckDigit.Length; pos++)
            {
                for (int newDigit = 0; newDigit <= 9; newDigit++)
                {
                    var corrected = numberWithCheckDigit.ToCharArray();
                    corrected[pos] = (char)('0' + newDigit);

                    string correctedStr = new string(corrected);
                    if (Validate(correctedStr))
                        return correctedStr;
                }
            }

            return null;
        }

        private static int[] GetDigits(string number)
        {
            var digits = new int[number.Length];
            for (int i = 0; i < number.Length; i++)
            {
                if (!char.IsDigit(number[i]))
                    throw new ArgumentException($"Invalid character: {number[i]}", nameof(number));

                digits[i] = number[i] - '0';
            }
            return digits;
        }
    }
}
