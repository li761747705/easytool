using System;

namespace EasyTool.CodeCategory
{
    /// <summary>
    /// Verhoeff 算法校验和工具类
    /// Verhoeff 算法是一种检测单个数字错误的校验和算法
    /// 由 Dutch mathematician Jacobus Verhoeff 发明
    /// 能检测所有单个数字错误和所有相邻数字交换错误
    /// 使用二面体群 D5
    /// </summary>
    public static class VerhoeffUtil
    {
        // 乘法表（二面体群 D5）
        private static readonly int[,] MultiplicationTable = new int[,]
        {
            {0, 1, 2, 3, 4, 5, 6, 7, 8, 9},
            {1, 2, 3, 4, 0, 6, 7, 8, 9, 5},
            {2, 3, 4, 0, 1, 7, 8, 9, 5, 6},
            {3, 4, 0, 1, 2, 8, 9, 5, 6, 7},
            {4, 0, 1, 2, 3, 9, 5, 6, 7, 8},
            {5, 9, 8, 7, 6, 0, 4, 3, 2, 1},
            {6, 5, 9, 8, 7, 1, 0, 4, 3, 2},
            {7, 6, 5, 9, 8, 2, 1, 0, 4, 3},
            {8, 7, 6, 5, 9, 3, 2, 1, 0, 4},
            {9, 8, 7, 6, 5, 4, 3, 2, 1, 0}
        };

        // 置换表
        private static readonly int[,] PermutationTable = new int[,]
        {
            {0, 1, 2, 3, 4, 5, 6, 7, 8, 9},
            {1, 5, 7, 6, 2, 8, 3, 0, 9, 4},
            {5, 8, 0, 3, 7, 9, 6, 1, 4, 2},
            {8, 9, 1, 6, 0, 4, 3, 5, 2, 7},
            {9, 4, 5, 3, 1, 2, 6, 8, 7, 0},
            {4, 2, 8, 6, 5, 7, 3, 9, 0, 1},
            {2, 7, 9, 3, 8, 0, 6, 4, 1, 5},
            {7, 0, 4, 6, 9, 1, 3, 2, 5, 8}
        };

        // 逆元表（用于查找校验位）
        private static readonly int[] InverseTable = new int[] { 0, 4, 3, 2, 1, 5, 6, 7, 8, 9 };

        /// <summary>
        /// 计算数字字符串的 Verhoeff 校验位
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
        /// 计算数字数组的 Verhoeff 校验位
        /// </summary>
        /// <param name="digits">数字数组</param>
        /// <returns>校验位（0-9）</returns>
        public static int CalculateCheckDigit(int[] digits)
        {
            if (digits == null || digits.Length == 0)
                throw new ArgumentException("Digits cannot be empty", nameof(digits));

            int checksum = 0;
            int length = digits.Length;

            for (int i = 0; i < length; i++)
            {
                int digit = digits[length - 1 - i];
                if (digit < 0 || digit > 9)
                    throw new ArgumentException($"Invalid digit: {digit}", nameof(digits));

                int permIndex = (i + 1) % 8;
                int permutedDigit = PermutationTable[permIndex, digit];
                checksum = MultiplicationTable[checksum, permutedDigit];
            }

            return InverseTable[checksum];
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

            int checksum = 0;
            int length = digitsWithCheckDigit.Length;

            for (int i = 0; i < length; i++)
            {
                int digit = digitsWithCheckDigit[length - 1 - i];
                if (digit < 0 || digit > 9)
                    return false;

                int permIndex = i % 8;
                int permutedDigit = PermutationTable[permIndex, digit];
                checksum = MultiplicationTable[checksum, permutedDigit];
            }

            return checksum == 0;
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

        /// <summary>
        /// 检测相邻数字交换错误
        /// </summary>
        /// <param name="numberWithCheckDigit">带校验位的数字字符串</param>
        /// <returns>纠正后的字符串，如果无法纠正则返回 null</returns>
        public static string DetectAndCorrectTransposition(string numberWithCheckDigit)
        {
            if (string.IsNullOrEmpty(numberWithCheckDigit) || numberWithCheckDigit.Length < 3)
                return null;

            // 首先验证
            if (Validate(numberWithCheckDigit))
                return numberWithCheckDigit;

            // 尝试交换相邻数字
            for (int i = 0; i < numberWithCheckDigit.Length - 1; i++)
            {
                var corrected = numberWithCheckDigit.ToCharArray();
                char temp = corrected[i];
                corrected[i] = corrected[i + 1];
                corrected[i + 1] = temp;

                string correctedStr = new string(corrected);
                if (Validate(correctedStr))
                    return correctedStr;
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
