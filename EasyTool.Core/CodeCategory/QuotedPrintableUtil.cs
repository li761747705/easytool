using System;
using System.Text;

namespace EasyTool.CodeCategory
{
    /// <summary>
    /// Quoted-Printable 编码工具类
    /// Quoted-Printable 是一种将 8 位数据编码为 7 位 ASCII 的编码方式
    /// 常用于电子邮件（MIME），将非 ASCII 字符编码为 =XX 格式
    /// RFC 2045 标准
    /// </summary>
    public static class QuotedPrintableUtil
    {
        private const int MaxLineLength = 76;

        /// <summary>
        /// 将字节数组编码为 Quoted-Printable 字符串
        /// </summary>
        /// <param name="data">要编码的数据</param>
        /// <param name="lineLength">每行最大长度</param>
        /// <returns>Quoted-Printable 编码字符串</returns>
        public static string Encode(byte[] data, int lineLength = 76)
        {
            if (data == null || data.Length == 0)
                return string.Empty;

            var result = new StringBuilder();
            int currentLineLength = 0;

            foreach (byte b in data)
            {
                string encoded = EncodeByte(b);
                int encodedLength = encoded.Length;

                // 检查是否需要换行
                if (currentLineLength + encodedLength > lineLength - 1)
                {
                    result.Append("=\r\n");
                    currentLineLength = 0;
                }

                result.Append(encoded);
                currentLineLength += encodedLength;
            }

            return result.ToString();
        }

        /// <summary>
        /// 将 Quoted-Printable 字符串解码为字节数组
        /// </summary>
        /// <param name="encoded">Quoted-Printable 编码字符串</param>
        /// <returns>解码后的字节数组</returns>
        public static byte[] Decode(string encoded)
        {
            if (string.IsNullOrEmpty(encoded))
                return Array.Empty<byte>();

            var result = new System.Collections.Generic.List<byte>();

            for (int i = 0; i < encoded.Length; i++)
            {
                char c = encoded[i];

                if (c == '=')
                {
                    if (i + 1 >= encoded.Length)
                        break;

                    char next = encoded[i + 1];

                    // 软换行
                    if (next == '\r' || next == '\n')
                    {
                        i++;
                        if (next == '\r' && i + 1 < encoded.Length && encoded[i + 1] == '\n')
                            i++;
                        continue;
                    }

                    // 编码字符
                    if (i + 2 < encoded.Length)
                    {
                        string hex = encoded.Substring(i + 1, 2);
                        if (TryParseHex(hex, out byte b))
                        {
                            result.Add(b);
                            i += 2;
                            continue;
                        }
                    }
                }
                else if (c != '\r' && c != '\n')
                {
                    result.Add((byte)c);
                }
            }

            return result.ToArray();
        }

        /// <summary>
        /// 将字符串编码为 Quoted-Printable（使用指定编码）
        /// </summary>
        /// <param name="text">文本</param>
        /// <param name="encoding">编码方式</param>
        /// <param name="lineLength">每行最大长度</param>
        /// <returns>Quoted-Printable 编码字符串</returns>
        public static string EncodeString(string text, Encoding encoding = null, int lineLength = 76)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            encoding ??= Encoding.UTF8;
            byte[] bytes = encoding.GetBytes(text);
            return Encode(bytes, lineLength);
        }

        /// <summary>
        /// 将 Quoted-Printable 字符串解码为文本（使用指定编码）
        /// </summary>
        /// <param name="encoded">Quoted-Printable 编码字符串</param>
        /// <param name="encoding">编码方式</param>
        /// <returns>解码后的文本</returns>
        public static string DecodeToString(string encoded, Encoding encoding = null)
        {
            if (string.IsNullOrEmpty(encoded))
                return string.Empty;

            byte[] bytes = Decode(encoded);
            encoding ??= Encoding.UTF8;
            return encoding.GetString(bytes);
        }

        /// <summary>
        /// 验证 Quoted-Printable 字符串是否有效
        /// </summary>
        /// <param name="encoded">Quoted-Printable 编码字符串</param>
        /// <returns>是否有效</returns>
        public static bool IsValid(string encoded)
        {
            if (string.IsNullOrEmpty(encoded))
                return true;

            for (int i = 0; i < encoded.Length; i++)
            {
                char c = encoded[i];

                if (c == '=')
                {
                    if (i + 1 >= encoded.Length)
                        return false;

                    char next = encoded[i + 1];

                    // 软换行
                    if (next == '\r' || next == '\n')
                        continue;

                    // 编码字符
                    if (i + 2 >= encoded.Length)
                        return false;

                    string hex = encoded.Substring(i + 1, 2);
                    if (!IsHexDigit(hex[0]) || !IsHexDigit(hex[1]))
                        return false;

                    i += 2;
                }
                else if (c < 32 && c != '\r' && c != '\n' && c != '\t')
                {
                    return false;
                }
                else if (c > 126)
                {
                    return false;
                }
            }

            return true;
        }

        private static string EncodeByte(byte b)
        {
            // 可打印 ASCII 字符（33-126，除了 61 '='）
            if (b >= 33 && b <= 126 && b != 61)
            {
                return ((char)b).ToString();
            }

            // 制表符和空格（特殊处理）
            if (b == 9 || b == 32)
            {
                return "=" + b.ToString("X2");
            }

            // 其他字符编码为 =XX
            return "=" + b.ToString("X2");
        }

        private static bool TryParseHex(string hex, out byte result)
        {
            result = 0;

            if (hex.Length != 2)
                return false;

            if (!IsHexDigit(hex[0]) || !IsHexDigit(hex[1]))
                return false;

            result = Convert.ToByte(hex, 16);
            return true;
        }

        private static bool IsHexDigit(char c)
        {
            return (c >= '0' && c <= '9') ||
                   (c >= 'A' && c <= 'F') ||
                   (c >= 'a' && c <= 'f');
        }

        /// <summary>
        /// 获取 Quoted-Printable 编码后的预计最大长度
        /// </summary>
        /// <param name="inputLength">输入长度</param>
        /// <returns>最大输出长度</returns>
        public static int CalculateMaxEncodedLength(int inputLength)
        {
            if (inputLength == 0)
                return 0;

            // 最坏情况：每个字符都编码为 =XX，加上软换行
            return inputLength * 3 + (inputLength * 3 / MaxLineLength) * 3;
        }
    }
}
