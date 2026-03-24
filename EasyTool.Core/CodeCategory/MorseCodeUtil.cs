using System;
using System.Collections.Generic;
using System.Text;

namespace EasyTool.CodeCategory
{
    /// <summary>
    /// 摩尔斯电码工具类
    /// 摩尔斯电码是一种将文本字符编码为点（.）和划（-）序列的编码方式
    /// 支持字母、数字和常用标点符号
    /// </summary>
    public static class MorseCodeUtil
    {
        private static readonly Dictionary<char, string> CharToMorse = new Dictionary<char, string>
        {
            // 字母
            {'A', ".-"}, {'B', "-..."}, {'C', "-.-."}, {'D', "-.."}, {'E', "."},
            {'F', "..-."}, {'G', "--."}, {'H', "...."}, {'I', ".."}, {'J', ".---"},
            {'K', "-.-"}, {'L', ".-.."}, {'M', "--"}, {'N', "-."}, {'O', "---"},
            {'P', ".--."}, {'Q', "--.-"}, {'R', ".-."}, {'S', "..."}, {'T', "-"},
            {'U', "..-"}, {'V', "...-"}, {'W', ".--"}, {'X', "-..-"}, {'Y', "-.--"},
            {'Z', "--.."},

            // 数字
            {'0', "-----"}, {'1', ".----"}, {'2', "..---"}, {'3', "...--"},
            {'4', "....-"}, {'5', "....."}, {'6', "-...."}, {'7', "--..."},
            {'8', "---.."}, {'9', "----."},

            // 标点符号
            {'.', ".-.-.-"}, {',', "--..--"}, {'?', "..--.."}, {'\'', ".----."},
            {'!', "-.-.--"}, {'/', "-..-."}, {'(', "-.--."}, {')', "-.--.-"},
            {'&', ".-..."}, {':', "---..."}, {';', "-.-.-."}, {'=', "-...-"},
            {'+', ".-.-."}, {'-', "-....-"}, {'_', "..--.-"}, {'"', ".-..-."},
            {'$', "...-..-"}, {'@', ".--.-."}
        };

        private static readonly Dictionary<string, char> MorseToChar = new Dictionary<string, char>();

        static MorseCodeUtil()
        {
            // 构建反向映射
            foreach (var kvp in CharToMorse)
            {
                MorseToChar[kvp.Value] = kvp.Key;
            }
        }

        /// <summary>
        /// 将文本编码为摩尔斯电码
        /// </summary>
        /// <param name="text">文本</param>
        /// <param name="letterSeparator">字母分隔符（默认空格）</param>
        /// <param name="wordSeparator">单词分隔符（默认斜杠）</param>
        /// <returns>摩尔斯电码</returns>
        public static string Encode(string text, string letterSeparator = " ", string wordSeparator = " / ")
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            var result = new StringBuilder();
            bool prevWasSpace = false;

            foreach (char c in text)
            {
                if (c == ' ')
                {
                    if (!prevWasSpace)
                    {
                        result.Append(wordSeparator);
                        prevWasSpace = true;
                    }
                }
                else
                {
                    char upper = char.ToUpperInvariant(c);
                    if (CharToMorse.TryGetValue(upper, out string morse))
                    {
                        if (result.Length > 0 && !prevWasSpace)
                        {
                            result.Append(letterSeparator);
                        }
                        result.Append(morse);
                        prevWasSpace = false;
                    }
                    // 忽略不支持的字符
                }
            }

            return result.ToString().Trim();
        }

        /// <summary>
        /// 将摩尔斯电码解码为文本
        /// </summary>
        /// <param name="morse">摩尔斯电码</param>
        /// <param name="letterSeparator">字母分隔符（默认空格）</param>
        /// <param name="wordSeparator">单词分隔符（默认斜杠）</param>
        /// <returns>文本</returns>
        public static string Decode(string morse, string letterSeparator = " ", string wordSeparator = " / ")
        {
            if (string.IsNullOrEmpty(morse))
                return string.Empty;

            var result = new StringBuilder();

            // 标准化分隔符
            string normalized = morse.Replace(wordSeparator, " / ");
            normalized = normalized.Replace(letterSeparator, " ");

            // 替换多个空格为单个空格（除了单词分隔符）
            while (normalized.Contains("  "))
            {
                normalized = normalized.Replace("  ", " ");
            }

            string[] parts = normalized.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string part in parts)
            {
                if (part == "/")
                {
                    result.Append(' ');
                }
                else if (MorseToChar.TryGetValue(part, out char c))
                {
                    result.Append(c);
                }
                else
                {
                    // 未知码，保留原样
                    result.Append(part);
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// 将文本编码为摩尔斯电码（使用标准分隔符）
        /// </summary>
        /// <param name="text">文本</param>
        /// <returns>摩尔斯电码</returns>
        public static string TextToMorse(string text)
        {
            return Encode(text);
        }

        /// <summary>
        /// 将摩尔斯电码解码为文本
        /// </summary>
        /// <param name="morse">摩尔斯电码</param>
        /// <returns>文本</returns>
        public static string MorseToText(string morse)
        {
            return Decode(morse);
        }

        /// <summary>
        /// 获取单个字符的摩尔斯电码
        /// </summary>
        /// <param name="c">字符</param>
        /// <returns>摩尔斯电码，如果不支持返回 null</returns>
        public static string GetMorse(char c)
        {
            c = char.ToUpperInvariant(c);
            return CharToMorse.TryGetValue(c, out string morse) ? morse : null;
        }

        /// <summary>
        /// 获取摩尔斯电码对应的字符
        /// </summary>
        /// <param name="morse">摩尔斯电码</param>
        /// <returns>字符，如果无效返回 null</returns>
        public static char? GetChar(string morse)
        {
            return MorseToChar.TryGetValue(morse, out char c) ? c : (char?)null;
        }

        /// <summary>
        /// 验证摩尔斯电码字符串是否有效
        /// </summary>
        /// <param name="morse">摩尔斯电码</param>
        /// <returns>是否有效</returns>
        public static bool IsValidMorse(string morse)
        {
            if (string.IsNullOrEmpty(morse))
                return false;

            foreach (char c in morse)
            {
                if (c != '.' && c != '-' && c != ' ' && c != '/')
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 验证文本是否可以完全编码为摩尔斯电码
        /// </summary>
        /// <param name="text">文本</param>
        /// <returns>是否可以编码</returns>
        public static bool CanEncode(string text)
        {
            if (string.IsNullOrEmpty(text))
                return true;

            foreach (char c in text)
            {
                if (c == ' ')
                    continue;

                if (!CharToMorse.ContainsKey(char.ToUpperInvariant(c)))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 获取不支持的字符
        /// </summary>
        /// <param name="text">文本</param>
        /// <returns>不支持的字符列表</returns>
        public static List<char> GetUnsupportedChars(string text)
        {
            var unsupported = new List<char>();

            if (string.IsNullOrEmpty(text))
                return unsupported;

            foreach (char c in text)
            {
                if (c == ' ')
                    continue;

                if (!CharToMorse.ContainsKey(char.ToUpperInvariant(c)) && !unsupported.Contains(c))
                {
                    unsupported.Add(c);
                }
            }

            return unsupported;
        }

        /// <summary>
        /// 将摩尔斯电码转换为音频信号参数
        /// </summary>
        /// <param name="morse">摩尔斯电码</param>
        /// <param name="dotDuration">点持续时间（毫秒）</param>
        /// <returns>信号参数列表（true = 信号，false = 停顿，后面跟持续时间）</returns>
        public static List<(bool Signal, int DurationMs)> ToSignalTiming(string morse, int dotDuration = 100)
        {
            var timing = new List<(bool Signal, int DurationMs)>();

            if (string.IsNullOrEmpty(morse))
                return timing;

            foreach (char c in morse)
            {
                switch (c)
                {
                    case '.':
                        timing.Add((true, dotDuration)); // 点
                        timing.Add((false, dotDuration)); // 点间停顿
                        break;
                    case '-':
                        timing.Add((true, dotDuration * 3)); // 划
                        timing.Add((false, dotDuration)); // 点间停顿
                        break;
                    case ' ':
                        // 单词间停顿（减去前面的点间停顿）
                        if (timing.Count > 0 && !timing[timing.Count - 1].Signal)
                        {
                            timing[timing.Count - 1] = (false, dotDuration * 6);
                        }
                        else
                        {
                            timing.Add((false, dotDuration * 7));
                        }
                        break;
                    case '/':
                        timing.Add((false, dotDuration * 7)); // 单词间停顿
                        break;
                }
            }

            return timing;
        }

        /// <summary>
        /// 获取支持的字符列表
        /// </summary>
        /// <returns>支持的字符</returns>
        public static string GetSupportedChars()
        {
            var chars = new StringBuilder();
            foreach (var c in CharToMorse.Keys)
            {
                chars.Append(c);
            }
            return chars.ToString();
        }

        /// <summary>
        /// 获取摩尔斯电码表
        /// </summary>
        /// <returns>字符到摩尔斯电码的映射</returns>
        public static Dictionary<char, string> GetMorseTable()
        {
            return new Dictionary<char, string>(CharToMorse);
        }
    }
}
