using System;
using System.Text;

namespace EasyTool.CodeCategory
{
    /// <summary>
    /// Baudot 编码工具类
    /// Baudot 码（也称为 Murrey 码或 ITA2）是一种5位字符编码
    /// 用于电报和 TTY 通信，支持字母和数字两个字符集
    /// </summary>
    public static class BaudotUtil
    {
        // 字母表模式（LTRS - Letters）
        private static readonly char[] LettersMode = new char[]
        {
            '\0', 'E', '\n', 'A', ' ', 'S', 'I', 'U',
            '\r', 'D', 'R', 'J', 'N', 'F', 'C', 'K',
            'T', 'Z', 'L', 'W', 'H', 'Y', 'P', 'Q',
            'O', 'B', 'G', '\0', 'M', 'X', 'V', '\0'
        };

        // 数字/符号模式（FIGS - Figures）
        private static readonly char[] FiguresMode = new char[]
        {
            '\0', '3', '\n', '-', ' ', '\'', '8', '7',
            '\r', '$', '4', '\a', ',', '!', ':', '(',
            '5', '+', ')', '2', '$', '6', '0', '1',
            '9', '?', '=', '\0', '.', '/', '=', '\0'
        };

        // 切换到字母表的代码
        private const byte LTRS = 0x1F;

        // 切换到数字表的代码
        private const byte FIGS = 0x1B;

        /// <summary>
        /// 将文本编码为 Baudot 码字节数组
        /// </summary>
        /// <param name="text">要编码的文本</param>
        /// <returns>Baudot 码字节数组</returns>
        public static byte[] Encode(string text)
        {
            if (string.IsNullOrEmpty(text))
                return Array.Empty<byte>();

            var result = new System.Collections.Generic.List<byte>();
            bool inLettersMode = true;

            foreach (char c in text.ToUpperInvariant())
            {
                // 在字母表中查找
                int lettersIndex = Array.IndexOf(LettersMode, c);
                int figuresIndex = Array.IndexOf(FiguresMode, c);

                if (lettersIndex >= 0 && figuresIndex >= 0)
                {
                    // 字符在两个表中都存在，保持当前模式
                    result.Add((byte)lettersIndex);
                }
                else if (lettersIndex >= 0)
                {
                    // 只在字母表中
                    if (!inLettersMode)
                    {
                        result.Add(LTRS);
                        inLettersMode = true;
                    }
                    result.Add((byte)lettersIndex);
                }
                else if (figuresIndex >= 0)
                {
                    // 只在数字表中
                    if (inLettersMode)
                    {
                        result.Add(FIGS);
                        inLettersMode = false;
                    }
                    result.Add((byte)figuresIndex);
                }
                // 忽略不支持的字符
            }

            return result.ToArray();
        }

        /// <summary>
        /// 将 Baudot 码字节数组解码为文本
        /// </summary>
        /// <param name="data">Baudot 码字节数组</param>
        /// <returns>解码后的文本</returns>
        public static string Decode(byte[] data)
        {
            if (data == null || data.Length == 0)
                return string.Empty;

            var result = new StringBuilder();
            bool inLettersMode = true;

            foreach (byte b in data)
            {
                if (b == LTRS)
                {
                    inLettersMode = true;
                }
                else if (b == FIGS)
                {
                    inLettersMode = false;
                }
                else
                {
                    char[] table = inLettersMode ? LettersMode : FiguresMode;
                    if (b < table.Length)
                    {
                        char c = table[b];
                        if (c != '\0')
                        {
                            result.Append(c);
                        }
                    }
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// 将 Baudot 码编码为二进制字符串表示
        /// </summary>
        /// <param name="data">Baudot 码字节数组</param>
        /// <returns>二进制字符串数组</returns>
        public static string[] ToBinaryStrings(byte[] data)
        {
            if (data == null || data.Length == 0)
                return Array.Empty<string>();

            var result = new string[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                result[i] = Convert.ToString(data[i] & 0x1F, 2).PadLeft(5, '0');
            }

            return result;
        }

        /// <summary>
        /// 从二进制字符串创建 Baudot 码
        /// </summary>
        /// <param name="binaryStrings">二进制字符串数组</param>
        /// <returns>Baudot 码字节数组</returns>
        public static byte[] FromBinaryStrings(string[] binaryStrings)
        {
            if (binaryStrings == null || binaryStrings.Length == 0)
                return Array.Empty<byte>();

            var result = new byte[binaryStrings.Length];
            for (int i = 0; i < binaryStrings.Length; i++)
            {
                result[i] = (byte)Convert.ToByte(binaryStrings[i], 2);
            }

            return result;
        }

        /// <summary>
        /// 将 Baudot 码编码为十六进制字符串
        /// </summary>
        /// <param name="data">Baudot 码字节数组</param>
        /// <returns>十六进制字符串</returns>
        public static string ToHexString(byte[] data)
        {
            if (data == null || data.Length == 0)
                return string.Empty;

            var result = new StringBuilder();
            foreach (byte b in data)
            {
                result.Append((b & 0x1F).ToString("X2"));
            }

            return result.ToString();
        }

        /// <summary>
        /// 从十六进制字符串解码 Baudot 码
        /// </summary>
        /// <param name="hex">十六进制字符串</param>
        /// <returns>Baudot 码字节数组</returns>
        public static byte[] FromHexString(string hex)
        {
            if (string.IsNullOrEmpty(hex)
                || hex.Length % 2 != 0)
                return Array.Empty<byte>();

            var result = new byte[hex.Length / 2];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = (byte)(Convert.ToByte(hex.Substring(i * 2, 2), 16) & 0x1F);
            }

            return result;
        }

        /// <summary>
        /// 验证文本是否可以用 Baudot 码表示
        /// </summary>
        /// <param name="text">文本</param>
        /// <returns>是否可以编码</returns>
        public static bool CanEncode(string text)
        {
            if (string.IsNullOrEmpty(text))
                return true;

            foreach (char c in text.ToUpperInvariant())
            {
                if (Array.IndexOf(LettersMode, c) < 0 &&
                    Array.IndexOf(FiguresMode, c) < 0)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 获取不支持的字符
        /// </summary>
        /// <param name="text">文本</param>
        /// <returns>不支持的字符数组</returns>
        public static char[] GetUnsupportedChars(string text)
        {
            if (string.IsNullOrEmpty(text))
                return Array.Empty<char>();

            var unsupported = new System.Collections.Generic.List<char>();

            foreach (char c in text.ToUpperInvariant())
            {
                if (Array.IndexOf(LettersMode, c) < 0 &&
                    Array.IndexOf(FiguresMode, c) < 0 &&
                    !unsupported.Contains(c))
                {
                    unsupported.Add(c);
                }
            }

            return unsupported.ToArray();
        }

        /// <summary>
        /// 获取字母表模式字符
        /// </summary>
        /// <param name="code">Baudot 码（0-31）</param>
        /// <returns>字符，如果无效则返回 '\0'</returns>
        public static char GetLettersChar(byte code)
        {
            if (code > 31)
                return '\0';

            return LettersMode[code];
        }

        /// <summary>
        /// 获取数字模式字符
        /// </summary>
        /// <param name="code">Baudot 码（0-31）</param>
        /// <returns>字符，如果无效则返回 '\0'</returns>
        public static char GetFiguresChar(byte code)
        {
            if (code > 31)
                return '\0';

            return FiguresMode[code];
        }

        /// <summary>
        /// 获取字符的 Baudot 码（字母模式）
        /// </summary>
        /// <param name="c">字符</param>
        /// <returns>Baudot 码，如果不存在则返回 -1</returns>
        public static int GetLettersCode(char c)
        {
            return Array.IndexOf(LettersMode, char.ToUpperInvariant(c));
        }

        /// <summary>
        /// 获取字符的 Baudot 码（数字模式）
        /// </summary>
        /// <param name="c">字符</param>
        /// <returns>Baudot 码，如果不存在则返回 -1</returns>
        public static int GetFiguresCode(char c)
        {
            return Array.IndexOf(FiguresMode, c);
        }

        /// <summary>
        /// 获取完整的字母表
        /// </summary>
        /// <returns>字母表字符数组</returns>
        public static char[] GetLettersTable()
        {
            return (char[])LettersMode.Clone();
        }

        /// <summary>
        /// 获取完整的数字表
        /// </summary>
        /// <returns>数字表字符数组</returns>
        public static char[] GetFiguresTable()
        {
            return (char[])FiguresMode.Clone();
        }
    }
}
