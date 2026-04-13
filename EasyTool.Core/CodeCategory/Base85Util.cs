using System;
using System.Text;

namespace EasyTool.CodeCategory
{
    /// <summary>
    /// Base85（Ascii85）编解码工具类
    /// Base85 是一种将二进制数据编码为ASCII字符的编码方式
    /// 每4个字节编码为5个字符，比Base64更高效
    /// </summary>
    public static class Base85Util
    {
        // Ascii85 字符集
        private const string Ascii85Chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz!#$%&()*+-;<=>?@^_`{|}~";

        // Z85 字符集（ZeroMQ）
        private const string Z85Chars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ.-:+=^!/*?&<>()[]{}@%$#";

        // RFC1924 字符集（IPv6地址）
        private const string Rfc1924Chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz!#$%&()*+-;<=>?@^_`{|}~";

        // Adobe 分隔符
        private const string AdobePrefix = "<~";
        private const string AdobeSuffix = "~>";

        #region Ascii85 编解码

        /// <summary>
        /// 将字节数组编码为 Ascii85 字符串
        /// </summary>
        /// <param name="data">要编码的字节数组</param>
        /// <returns>Ascii85 编码字符串</returns>
        public static string Encode(byte[] data)
        {
            return Encode(data, false);
        }

        /// <summary>
        /// 将字节数组编码为 Ascii85 字符串
        /// </summary>
        /// <param name="data">要编码的字节数组</param>
        /// <param name="useAdobeFormat">是否使用Adobe格式（添加 &lt;~ 和 ~&gt;）</param>
        /// <returns>Ascii85 编码字符串</returns>
        public static string Encode(byte[] data, bool useAdobeFormat)
        {
            if (data == null || data.Length == 0)
                return string.Empty;

            var result = new StringBuilder();

            // 处理完整的4字节块
            int fullBlocks = data.Length / 4;
            int remainder = data.Length % 4;

            for (int i = 0; i < fullBlocks; i++)
            {
                uint value = (uint)((data[i * 4] << 24) |
                                    (data[i * 4 + 1] << 16) |
                                    (data[i * 4 + 2] << 8) |
                                    data[i * 4 + 3]);

                if (value == 0 && useAdobeFormat)
                {
                    result.Append('z'); // 特殊压缩：4个零字节编码为 'z'
                }
                else
                {
                    EncodeBlock(result, value, 5);
                }
            }

            // 处理剩余字节
            if (remainder > 0)
            {
                uint value = 0;
                int padding = 4 - remainder;

                for (int i = 0; i < remainder; i++)
                {
                    value = (value << 8) | data[fullBlocks * 4 + i];
                }
                value <<= (padding * 8);

                EncodeBlock(result, value, remainder + 1);
            }

            if (useAdobeFormat)
            {
                return AdobePrefix + result.ToString() + AdobeSuffix;
            }

            return result.ToString();
        }

        /// <summary>
        /// 将 Ascii85 字符串解码为字节数组
        /// </summary>
        /// <param name="value">Ascii85 编码字符串</param>
        /// <returns>解码后的字节数组</returns>
        public static byte[] Decode(string value)
        {
            return Decode(value, false);
        }

        /// <summary>
        /// 将 Ascii85 字符串解码为字节数组
        /// </summary>
        /// <param name="value">Ascii85 编码字符串</param>
        /// <param name="adobeFormat">是否为Adobe格式</param>
        /// <returns>解码后的字节数组</returns>
        public static byte[] Decode(string value, bool adobeFormat)
        {
            if (string.IsNullOrEmpty(value))
                return Array.Empty<byte>();

            // 移除 Adobe 格式的分隔符
            if (adobeFormat)
            {
                if (value.StartsWith(AdobePrefix))
                    value = value.Substring(AdobePrefix.Length);
                if (value.EndsWith(AdobeSuffix))
                    value = value.Substring(0, value.Length - AdobeSuffix.Length);
            }

            // 移除空白字符
            value = RemoveWhitespace(value);

            var result = new byte[CalculateDecodedLength(value)];
            int resultIndex = 0;

            int i = 0;
            while (i < value.Length)
            {
                if (value[i] == 'z')
                {
                    // 'z' 表示4个零字节
                    result[resultIndex++] = 0;
                    result[resultIndex++] = 0;
                    result[resultIndex++] = 0;
                    result[resultIndex++] = 0;
                    i++;
                }
                else if (value[i] == 'y')
                {
                    // 'y' 表示4个空格字节（某些变体）
                    result[resultIndex++] = 0x20;
                    result[resultIndex++] = 0x20;
                    result[resultIndex++] = 0x20;
                    result[resultIndex++] = 0x20;
                    i++;
                }
                else
                {
                    // 处理5字符块
                    int blockLength = Math.Min(5, value.Length - i);
                    uint decoded = DecodeBlock(value, i, blockLength);

                    int bytesToWrite = blockLength - 1;
                    for (int j = 3; j >= 4 - bytesToWrite; j--)
                    {
                        result[resultIndex++] = (byte)((decoded >> (j * 8)) & 0xFF);
                    }

                    i += blockLength;
                }
            }

            // 调整数组大小
            if (resultIndex < result.Length)
            {
                Array.Resize(ref result, resultIndex);
            }

            return result;
        }

        private static void EncodeBlock(StringBuilder result, uint value, int chars)
        {
            for (int i = chars - 1; i >= 0; i--)
            {
                result.Append((char)(value % 85 + 33));
                value /= 85;
            }
        }

        private static uint DecodeBlock(string value, int offset, int length)
        {
            uint result = 0;
            for (int i = 0; i < length; i++)
            {
                char c = value[offset + i];
                if (c < 33 || c > 117)
                {
                    throw new ArgumentException($"Invalid Ascii85 character: {c}", nameof(value));
                }
                result = result * 85 + (uint)(c - 33);
            }

            // 填充剩余字符的影响
            for (int i = length; i < 5; i++)
            {
                result = result * 85 + 84; // 'u' - 33 = 84
            }

            return result;
        }

        private static int CalculateDecodedLength(string value)
        {
            int length = 0;
            for (int i = 0; i < value.Length; i++)
            {
                if (value[i] == 'z' || value[i] == 'y')
                {
                    length += 4;
                }
                else if (value[i] >= 33 && value[i] <= 117)
                {
                    length++;
                }
            }
            return (length / 5) * 4 + 4; // 估算，后续会调整
        }

        private static string RemoveWhitespace(string value)
        {
            var result = new StringBuilder(value.Length);
            foreach (char c in value)
            {
                if (!char.IsWhiteSpace(c))
                {
                    result.Append(c);
                }
            }
            return result.ToString();
        }

        #endregion

        #region Z85 编解码

        private static readonly int[] Z85DecodeMap = BuildZ85DecodeMap();

        private static int[] BuildZ85DecodeMap()
        {
            var map = new int[256];
            for (int i = 0; i < 256; i++)
                map[i] = -1;
            for (int i = 0; i < Z85Chars.Length; i++)
                map[Z85Chars[i]] = i;
            return map;
        }

        /// <summary>
        /// 将字节数组编码为 Z85 字符串
        /// </summary>
        /// <param name="data">要编码的字节数组（长度必须是4的倍数）</param>
        /// <returns>Z85 编码字符串</returns>
        public static string EncodeZ85(byte[] data)
        {
            if (data == null || data.Length == 0)
                return string.Empty;

            if (data.Length % 4 != 0)
                throw new ArgumentException("Z85 编码的数据长度必须是 4 的倍数", nameof(data));

            var result = new StringBuilder(data.Length * 5 / 4);

            for (int i = 0; i < data.Length; i += 4)
            {
                uint value = (uint)((data[i] << 24) |
                                    (data[i + 1] << 16) |
                                    (data[i + 2] << 8) |
                                    data[i + 3]);

                char[] block = new char[5];
                for (int j = 4; j >= 0; j--)
                {
                    block[j] = Z85Chars[(int)(value % 85)];
                    value /= 85;
                }
                result.Append(block);
            }

            return result.ToString();
        }

        /// <summary>
        /// 将 Z85 字符串解码为字节数组
        /// </summary>
        /// <param name="value">Z85 编码字符串（长度必须是5的倍数）</param>
        /// <returns>解码后的字节数组</returns>
        public static byte[] DecodeZ85(string value)
        {
            if (string.IsNullOrEmpty(value))
                return Array.Empty<byte>();

            if (value.Length % 5 != 0)
                throw new ArgumentException("Z85 解码的字符串长度必须是 5 的倍数", nameof(value));

            var result = new byte[value.Length * 4 / 5];
            int resultIndex = 0;

            for (int i = 0; i < value.Length; i += 5)
            {
                uint decoded = 0;
                for (int j = 0; j < 5; j++)
                {
                    int index = Z85DecodeMap[value[i + j]];
                    if (index < 0)
                        throw new ArgumentException($"Invalid Z85 character: {value[i + j]}", nameof(value));
                    decoded = decoded * 85 + (uint)index;
                }

                result[resultIndex++] = (byte)((decoded >> 24) & 0xFF);
                result[resultIndex++] = (byte)((decoded >> 16) & 0xFF);
                result[resultIndex++] = (byte)((decoded >> 8) & 0xFF);
                result[resultIndex++] = (byte)(decoded & 0xFF);
            }

            return result;
        }

        #endregion

        #region 通用方法

        /// <summary>
        /// 将字符串编码为 Ascii85
        /// </summary>
        /// <param name="text">要编码的字符串</param>
        /// <param name="encoding">编码方式（默认UTF-8）</param>
        /// <returns>Ascii85 编码字符串</returns>
        public static string EncodeString(string text, Encoding encoding = null)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            encoding ??= Encoding.UTF8;
            return Encode(encoding.GetBytes(text));
        }

        /// <summary>
        /// 将 Ascii85 字符串解码为原始字符串
        /// </summary>
        /// <param name="value">Ascii85 编码字符串</param>
        /// <param name="encoding">编码方式（默认UTF-8）</param>
        /// <returns>解码后的字符串</returns>
        public static string DecodeString(string value, Encoding encoding = null)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            encoding ??= Encoding.UTF8;
            byte[] bytes = Decode(value);
            return encoding.GetString(bytes);
        }

        /// <summary>
        /// 验证是否是有效的 Ascii85 字符串
        /// </summary>
        /// <param name="value">要验证的字符串</param>
        /// <returns>是否有效</returns>
        public static bool IsValid(string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            // 移除 Adobe 格式的分隔符
            if (value.StartsWith(AdobePrefix))
                value = value.Substring(AdobePrefix.Length);
            if (value.EndsWith(AdobeSuffix))
                value = value.Substring(0, value.Length - AdobeSuffix.Length);

            value = RemoveWhitespace(value);

            foreach (char c in value)
            {
                if (c != 'z' && c != 'y' && (c < 33 || c > 117))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 尝试解码 Ascii85 字符串
        /// </summary>
        /// <param name="value">Ascii85 编码字符串</param>
        /// <param name="bytes">解码后的字节数组</param>
        /// <returns>是否解码成功</returns>
        public static bool TryDecode(string value, out byte[] bytes)
        {
            bytes = null;
            if (!IsValid(value))
                return false;

            try
            {
                bytes = Decode(value);
                return true;
            }
            // 捕获 Base85 解码格式异常
            catch (FormatException)
            {
                return false;
            }
        }

        /// <summary>
        /// 计算编码后的长度
        /// </summary>
        /// <param name="dataLength">原始数据长度</param>
        /// <returns>编码后长度</returns>
        public static int GetEncodedLength(int dataLength)
        {
            if (dataLength == 0)
                return 0;

            return (dataLength + 3) / 4 * 5;
        }

        /// <summary>
        /// 计算解码后的最大长度
        /// </summary>
        /// <param name="encodedLength">编码后长度</param>
        /// <returns>解码后最大长度</returns>
        public static int GetMaxDecodedLength(int encodedLength)
        {
            if (encodedLength == 0)
                return 0;

            return encodedLength / 5 * 4 + 4;
        }

        #endregion
    }
}
