using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EasyTool
{
    /// <summary>
    /// 编码工具类，提供各种编码格式的转换功能
    /// </summary>
    public static class EncodingUtil
    {
        #region Base32 编码

        // Base32 字符集，共 32 个字符
        private static readonly char[] BASE32_CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567".ToCharArray();

        // Base32 填充字符
        private const char BASE32_PADDING_CHAR = '=';

        /// <summary>
        /// 将给定的字节数组转换为 Base32 编码字符串
        /// </summary>
        /// <param name="bytes">要转换的字节数组</param>
        /// <returns>转换后的 Base32 编码字符串</returns>
        public static string Base32Encode(byte[] bytes)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }

            int length = bytes.Length;
            if (length == 0)
            {
                return string.Empty;
            }

            char[] chars = new char[(length + 4) / 5 * 8];
            int index = 0;
            for (int i = 0; i < length; i += 5)
            {
                int val = (bytes[i] << 24) + ((i + 1 < length ? bytes[i + 1] : 0) << 16) +
                          ((i + 2 < length ? bytes[i + 2] : 0) << 8) + ((i + 3 < length ? bytes[i + 3] : 0) << 0);
                chars[index++] = BASE32_CHARS[(val >> 35) & 0x1F];
                chars[index++] = BASE32_CHARS[(val >> 30) & 0x1F];
                chars[index++] = BASE32_CHARS[(val >> 25) & 0x1F];
                chars[index++] = BASE32_CHARS[(val >> 20) & 0x1F];
                chars[index++] = BASE32_CHARS[(val >> 15) & 0x1F];
                chars[index++] = BASE32_CHARS[(val >> 10) & 0x1F];
                chars[index++] = BASE32_CHARS[(val >> 5) & 0x1F];
                chars[index++] = BASE32_CHARS[val & 0x1F];
            }

            // 添加填充字符
            int paddingCount = length % 5;
            if (paddingCount > 0)
            {
                chars[chars.Length - 1] = BASE32_PADDING_CHAR;
                if (paddingCount == 1)
                {
                    chars[chars.Length - 2] = BASE32_PADDING_CHAR;
                }
                else if (paddingCount <= 2)
                {
                    chars[chars.Length - 3] = BASE32_PADDING_CHAR;
                }
                else if (paddingCount <= 3)
                {
                    chars[chars.Length - 4] = BASE32_PADDING_CHAR;
                }
                else if (paddingCount <= 4)
                {
                    chars[chars.Length - 5] = BASE32_PADDING_CHAR;
                }
            }

            return new string(chars);
        }

        /// <summary>
        /// 将给定的 Base32 编码字符串转换为字节数组
        /// </summary>
        /// <param name="str">要转换的 Base32 编码字符串</param>
        /// <returns>转换后的字节数组</returns>
        public static byte[] Base32Decode(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                throw new ArgumentException("String is null or empty.", nameof(str));
            }

            // 移除填充字符
            str = str.TrimEnd('=');

            int length = str.Length;
            if (length % 8 != 0)
            {
                throw new ArgumentException("Invalid length of input string: " + length, nameof(str));
            }

            int paddingCount = 0;
            if (length > 0 && str[length - 1] == BASE32_PADDING_CHAR)
            {
                paddingCount++;
            }
            if (length > 1 && str[length - 2] == BASE32_PADDING_CHAR)
            {
                paddingCount++;
            }
            if (length > 3 && str[length - 3] == BASE32_PADDING_CHAR)
            {
                paddingCount++;
            }
            if (length > 4 && str[length - 4] == BASE32_PADDING_CHAR)
            {
                paddingCount++;
            }
            if (length > 5 && str[length - 5] == BASE32_PADDING_CHAR)
            {
                paddingCount++;
            }
            if (length > 6 && str[length - 6] == BASE32_PADDING_CHAR)
            {
                paddingCount++;
            }

            byte[] bytes = new byte[length / 8 * 5 - paddingCount];
            int index = 0;
            for (int i = 0; i < length; i += 8)
            {
                int val = (DecodeBase32Char(str[i]) << 35) +
                          (DecodeBase32Char(str[i + 1]) << 30) +
                          (DecodeBase32Char(str[i + 2]) << 25) +
                          (DecodeBase32Char(str[i + 3]) << 20) +
                          (DecodeBase32Char(str[i + 4]) << 15) +
                          (DecodeBase32Char(str[i + 5]) << 10) +
                          (DecodeBase32Char(str[i + 6]) << 5) +
                          DecodeBase32Char(str[i + 7]);
                bytes[index++] = (byte)(val >> 24);
                if (index < bytes.Length)
                {
                    bytes[index++] = (byte)(val >> 16);
                }
                if (index < bytes.Length)
                {
                    bytes[index++] = (byte)(val >> 8);
                }
                if (index < bytes.Length)
                {
                    bytes[index++] = (byte)val;
                }
            }

            return bytes;
        }

        // 解码 Base32 字符
        private static int DecodeBase32Char(char c)
        {
            if (c >= 'A' && c <= 'Z')
            {
                return c - 'A';
            }
            if (c >= '2' && c <= '7')
            {
                return c - '2' + 26;
            }
            throw new ArgumentException("Invalid character in input string: " + c, nameof(c));
        }

        #endregion

        #region Base62 编码

        // Base62 字符集，共 62 个字符
        private static readonly char[] BASE62_CHARS =
            "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz".ToCharArray();

        /// <summary>
        /// 将给定的整数转换为 Base62 编码字符串
        /// </summary>
        /// <param name="number">要转换的整数</param>
        /// <returns>转换后的 Base62 编码字符串</returns>
        public static string Base62Encode(long number)
        {
            if (number < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(number), "Number must be non-negative.");
            }

            if (number == 0)
            {
                return BASE62_CHARS[0].ToString();
            }

            List<char> chars = new List<char>();
            int targetBase = BASE62_CHARS.Length;
            while (number > 0)
            {
                int index = (int)(number % targetBase);
                chars.Add(BASE62_CHARS[index]);
                number = number / targetBase;
            }
            chars.Reverse();
            return new string(chars.ToArray());
        }

        /// <summary>
        /// 将给定的 Base62 编码字符串转换为整数
        /// </summary>
        /// <param name="str">要转换的 Base62 编码字符串</param>
        /// <returns>转换后的整数</returns>
        public static long Base62Decode(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                throw new ArgumentException("String is null or empty.", nameof(str));
            }

            long result = 0;
            int sourceBase = BASE62_CHARS.Length;
            long multiplier = 1;
            for (int i = str.Length - 1; i >= 0; i--)
            {
                int digit = Array.IndexOf(BASE62_CHARS, str[i]);
                if (digit == -1)
                {
                    throw new ArgumentException("Invalid character in string: " + str[i], nameof(str));
                }
                result += digit * multiplier;
                multiplier *= sourceBase;
            }
            return result;
        }

        #endregion

        #region ROT 加密

        /// <summary>
        /// 将给定的字符串按照 ROT 加密算法进行加密
        /// </summary>
        /// <param name="text">要加密的字符串</param>
        /// <param name="n">偏移量</param>
        /// <returns>加密后的字符串</returns>
        public static string RotEncrypt(string text, int n)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            string upperCaseText = text.ToUpper();
            return new string(upperCaseText.Select(c =>
            {
                if (!char.IsLetter(c))
                {
                    return c;
                }
                int x = c - 'A';
                int y = (x + n) % 26;
                return (char)(y + 'A');
            }).ToArray());
        }

        /// <summary>
        /// 将给定的字符串按照 ROT 加密算法进行解密
        /// </summary>
        /// <param name="text">要解密的字符串</param>
        /// <param name="n">偏移量</param>
        /// <returns>解密后的字符串</returns>
        public static string RotDecrypt(string text, int n)
        {
            return RotEncrypt(text, 26 - n);
        }

        #endregion

        #region Morse 电码

        // Morse 电码表
        private static readonly Dictionary<char, string> MORSE_TABLE = new Dictionary<char, string>
        {
            {'A', ".-"},
            {'B', "-..."},
            {'C', "-.-."},
            {'D', "-.."},
            {'E', "."},
            {'F', "..-."},
            {'G', "--."},
            {'H', "...."},
            {'I', ".."},
            {'J', ".---"},
            {'K', "-.-"},
            {'L', ".-.."},
            {'M', "--"},
            {'N', "-."},
            {'O', "---"},
            {'P', ".--."},
            {'Q', "--.-"},
            {'R', ".-."},
            {'S', "..."},
            {'T', "-"},
            {'U', "..-"},
            {'V', "...-"},
            {'W', ".--"},
            {'X', "-.."},
            {'Y', "-.--"},
            {'Z', "--.."},
            {'0', "-----"},
            {'1', ".----"},
            {'2', "..---"},
            {'3', "...--"},
            {'4', "....-"},
            {'5', "....."},
            {'6', "-...."},
            {'7', "--..."},
            {'8', "---.."},
            {'9', "----."},
            {' ', " "}
        };

        /// <summary>
        /// 将给定的字符串转换为 Morse 电码字符串
        /// </summary>
        /// <param name="str">要转换的字符串</param>
        /// <returns>转换后的 Morse 电码字符串</returns>
        public static string MorseEncode(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return string.Empty;
            }

            List<string> morseCodes = new List<string>();
            foreach (char c in str.ToUpper())
            {
                if (MORSE_TABLE.ContainsKey(c))
                {
                    morseCodes.Add(MORSE_TABLE[c]);
                }
            }
            return string.Join(" ", morseCodes);
        }

        /// <summary>
        /// 将给定的 Morse 电码字符串转换为原始字符串
        /// </summary>
        /// <param name="morseCode">要转换的 Morse 电码字符串</param>
        /// <returns>转换后的原始字符串</returns>
        public static string MorseDecode(string morseCode)
        {
            if (string.IsNullOrEmpty(morseCode))
            {
                return string.Empty;
            }

            string[] codes = morseCode.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            List<char> chars = new List<char>();
            foreach (string code in codes)
            {
                foreach (KeyValuePair<char, string> kvp in MORSE_TABLE)
                {
                    if (kvp.Value == code)
                    {
                        chars.Add(kvp.Key);
                        break;
                    }
                }
            }
            return new string(chars.ToArray());
        }

        #endregion
    }
}
