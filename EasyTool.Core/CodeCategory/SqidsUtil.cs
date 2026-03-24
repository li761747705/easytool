using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace EasyTool.CodeCategory
{
    /// <summary>
    /// Sqids（以前叫 Hashids）工具类
    /// Sqids 是一种将数字数组编码为短字符串的算法
    /// 可逆、可配置字母表、无碰撞
    /// 常用于生成短 URL、混淆 ID 等
    /// </summary>
    public static class SqidsUtil
    {
        private const string DefaultAlphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private const int MinAlphabetLength = 3;

        private static readonly byte[] DefaultSalt = Array.Empty<byte>();

        #region 默认实例方法

        /// <summary>
        /// 使用默认配置编码单个数字
        /// </summary>
        /// <param name="number">要编码的数字</param>
        /// <returns>编码字符串</returns>
        public static string Encode(ulong number)
        {
            return Encode(new[] { number });
        }

        /// <summary>
        /// 使用默认配置编码数字数组
        /// </summary>
        /// <param name="numbers">要编码的数字数组</param>
        /// <returns>编码字符串</returns>
        public static string Encode(ulong[] numbers)
        {
            return Encode(numbers, DefaultAlphabet, DefaultSalt, 0);
        }

        /// <summary>
        /// 使用默认配置解码为单个数字
        /// </summary>
        /// <param name="encoded">编码字符串</param>
        /// <returns>解码的数字</returns>
        public static ulong DecodeSingle(string encoded)
        {
            var numbers = Decode(encoded);
            if (numbers.Length == 0)
                throw new ArgumentException("Invalid encoded string");
            return numbers[0];
        }

        /// <summary>
        /// 使用默认配置解码
        /// </summary>
        /// <param name="encoded">编码字符串</param>
        /// <returns>解码的数字数组</returns>
        public static ulong[] Decode(string encoded)
        {
            return Decode(encoded, DefaultAlphabet, DefaultSalt);
        }

        #endregion

        #region 自定义配置方法

        /// <summary>
        /// 使用自定义配置编码
        /// </summary>
        /// <param name="numbers">要编码的数字数组</param>
        /// <param name="alphabet">自定义字母表</param>
        /// <param name="salt">盐值</param>
        /// <param name="minLength">最小长度</param>
        /// <returns>编码字符串</returns>
        public static string Encode(ulong[] numbers, string alphabet, byte[] salt = null, int minLength = 0)
        {
            if (numbers == null || numbers.Length == 0)
                throw new ArgumentException("Numbers cannot be empty", nameof(numbers));
            if (string.IsNullOrEmpty(alphabet) || alphabet.Length < MinAlphabetLength)
                throw new ArgumentException($"Alphabet must be at least {MinAlphabetLength} characters", nameof(alphabet));

            salt ??= Array.Empty<byte>();
            alphabet = ShuffleAlphabet(alphabet, salt);

            // 计算前缀
            char prefix = alphabet[0];
            string alphabetWithoutPrefix = alphabet.Substring(1) + alphabet[0];

            var result = new StringBuilder();
            result.Append(prefix);

            // 编码每个数字
            for (int i = 0; i < numbers.Length; i++)
            {
                ulong number = numbers[i];
                string currentAlphabet = ConsistentShuffle(alphabetWithoutPrefix, salt, i);

                string encoded = EncodeNumber(number, currentAlphabet);
                result.Append(encoded);

                if (i < numbers.Length - 1)
                {
                    char separator = currentAlphabet[(int)(number % (ulong)(currentAlphabet.Length - 1))];
                    result.Append(separator);
                    alphabetWithoutPrefix = RotateAlphabet(alphabetWithoutPrefix, encoded[encoded.Length - 1]);
                }
            }

            // 填充到最小长度
            string finalResult = result.ToString();
            if (minLength > 0 && finalResult.Length < minLength)
            {
                int diff = minLength - finalResult.Length;
                string paddedAlphabet = ConsistentShuffle(alphabet, salt, 0);
                finalResult = paddedAlphabet.Substring(0, diff / 2) + finalResult + paddedAlphabet.Substring(paddedAlphabet.Length - (diff - diff / 2));
            }

            return finalResult;
        }

        /// <summary>
        /// 使用自定义配置解码
        /// </summary>
        /// <param name="encoded">编码字符串</param>
        /// <param name="alphabet">自定义字母表</param>
        /// <param name="salt">盐值</param>
        /// <returns>解码的数字数组</returns>
        public static ulong[] Decode(string encoded, string alphabet, byte[] salt = null)
        {
            if (string.IsNullOrEmpty(encoded))
                throw new ArgumentException("Encoded string cannot be empty", nameof(encoded));
            if (string.IsNullOrEmpty(alphabet) || alphabet.Length < MinAlphabetLength)
                throw new ArgumentException($"Alphabet must be at least {MinAlphabetLength} characters", nameof(alphabet));

            salt ??= Array.Empty<byte>();
            alphabet = ShuffleAlphabet(alphabet, salt);

            // 移除填充
            encoded = RemovePadding(encoded, alphabet, salt);

            // 获取前缀
            char prefix = encoded[0];
            if (!alphabet.Contains(prefix))
                throw new ArgumentException("Invalid encoded string: unknown prefix");

            string alphabetWithoutPrefix = alphabet.Substring(1) + alphabet[0];

            var numbers = new List<ulong>();
            string remaining = encoded.Substring(1);

            for (int i = 0; remaining.Length > 0; i++)
            {
                string currentAlphabet = ConsistentShuffle(alphabetWithoutPrefix, salt, i);

                // 找到分隔符
                int separatorIndex = -1;
                for (int j = 0; j < remaining.Length; j++)
                {
                    if (!currentAlphabet.Contains(remaining[j]))
                    {
                        separatorIndex = j;
                        break;
                    }
                }

                string encodedNumber;
                if (separatorIndex >= 0)
                {
                    encodedNumber = remaining.Substring(0, separatorIndex);
                    remaining = remaining.Substring(separatorIndex + 1);
                }
                else
                {
                    encodedNumber = remaining;
                    remaining = "";
                }

                ulong number = DecodeNumber(encodedNumber, currentAlphabet);
                numbers.Add(number);

                if (encodedNumber.Length > 0)
                {
                    alphabetWithoutPrefix = RotateAlphabet(alphabetWithoutPrefix, encodedNumber[encodedNumber.Length - 1]);
                }
            }

            return numbers.ToArray();
        }

        #endregion

        #region Sqids 实例

        /// <summary>
        /// 创建 Sqids 编码器实例
        /// </summary>
        /// <param name="alphabet">字母表</param>
        /// <param name="salt">盐值</param>
        /// <param name="minLength">最小长度</param>
        /// <returns>Sqids 实例</returns>
        public static SqidsEncoder Create(string alphabet = null, byte[] salt = null, int minLength = 0)
        {
            return new SqidsEncoder(alphabet ?? DefaultAlphabet, salt, minLength);
        }

        #endregion

        #region 私有方法

        private static string ShuffleAlphabet(string alphabet, byte[] salt)
        {
            char[] chars = alphabet.ToCharArray();

            if (salt.Length == 0)
                return new string(chars);

            int j = chars.Length - 1;
            int v = 0;
            int p = 0;

            for (int i = chars.Length - 1; i > 0; i--, j--)
            {
                v %= salt.Length;
                p += salt[v];
                int k = (salt[v] + p + i) % (i + 1);

                char temp = chars[i];
                chars[i] = chars[k];
                chars[k] = temp;

                v++;
            }

            return new string(chars);
        }

        private static string ConsistentShuffle(string alphabet, byte[] salt, int iteration)
        {
            if (salt.Length == 0)
                return alphabet;

            char[] chars = alphabet.ToCharArray();
            int v = iteration % salt.Length;

            for (int i = chars.Length - 1; i > 0; i--)
            {
                int k = (salt[v] + i) % (i + 1);

                char temp = chars[i];
                chars[i] = chars[k];
                chars[k] = temp;

                v = (v + 1) % salt.Length;
            }

            return new string(chars);
        }

        private static string RotateAlphabet(string alphabet, char c)
        {
            int index = alphabet.IndexOf(c);
            if (index < 0)
                return alphabet;

            return alphabet.Substring(index + 1) + alphabet.Substring(0, index + 1);
        }

        private static string EncodeNumber(ulong number, string alphabet)
        {
            var result = new StringBuilder();
            int baseLength = alphabet.Length;

            do
            {
                result.Insert(0, alphabet[(int)(number % (ulong)baseLength)]);
                number /= (ulong)baseLength;
            } while (number > 0);

            return result.ToString();
        }

        private static ulong DecodeNumber(string encoded, string alphabet)
        {
            ulong result = 0;
            int baseLength = alphabet.Length;

            foreach (char c in encoded)
            {
                int index = alphabet.IndexOf(c);
                if (index < 0)
                    throw new ArgumentException($"Invalid character: {c}");

                result = result * (ulong)baseLength + (ulong)index;
            }

            return result;
        }

        private static string RemovePadding(string encoded, string alphabet, byte[] salt)
        {
            // 检查是否有有效的数字字符
            for (int i = 1; i < encoded.Length; i++)
            {
                if (alphabet.Contains(encoded[i]))
                    return encoded.Substring(i - 1);
            }

            return encoded;
        }

        #endregion
    }

    /// <summary>
    /// Sqids 编码器实例
    /// </summary>
    public class SqidsEncoder
    {
        private readonly string _alphabet;
        private readonly byte[] _salt;
        private readonly int _minLength;

        /// <summary>
        /// 创建 Sqids 编码器
        /// </summary>
        /// <param name="alphabet">字母表</param>
        /// <param name="salt">盐值</param>
        /// <param name="minLength">最小长度</param>
        public SqidsEncoder(string alphabet, byte[] salt, int minLength)
        {
            _alphabet = alphabet;
            _salt = salt ?? Array.Empty<byte>();
            _minLength = minLength;
        }

        /// <summary>
        /// 编码单个数字
        /// </summary>
        /// <param name="number">数字</param>
        /// <returns>编码字符串</returns>
        public string Encode(ulong number)
        {
            return SqidsUtil.Encode(new[] { number }, _alphabet, _salt, _minLength);
        }

        /// <summary>
        /// 编码数字数组
        /// </summary>
        /// <param name="numbers">数字数组</param>
        /// <returns>编码字符串</returns>
        public string Encode(ulong[] numbers)
        {
            return SqidsUtil.Encode(numbers, _alphabet, _salt, _minLength);
        }

        /// <summary>
        /// 解码为单个数字
        /// </summary>
        /// <param name="encoded">编码字符串</param>
        /// <returns>数字</returns>
        public ulong DecodeSingle(string encoded)
        {
            var numbers = SqidsUtil.Decode(encoded, _alphabet, _salt);
            if (numbers.Length == 0)
                throw new ArgumentException("Invalid encoded string");
            return numbers[0];
        }

        /// <summary>
        /// 解码
        /// </summary>
        /// <param name="encoded">编码字符串</param>
        /// <returns>数字数组</returns>
        public ulong[] Decode(string encoded)
        {
            return SqidsUtil.Decode(encoded, _alphabet, _salt);
        }
    }
}
