using System;
using System.Text;

namespace EasyTool.CodeCategory
{
    /// <summary>
    /// ROT13/ROT47 编码工具类
    /// ROT13 是一种简单的字母替换加密（凯撒密码的特例）
    /// ROT47 扩展到所有 ASCII 可打印字符
    /// 注意：这不是真正的加密，只是一种混淆方式
    /// </summary>
    public static class ROT13Util
    {
        /// <summary>
        /// 使用 ROT13 编码文本
        /// </summary>
        /// <param name="text">文本</param>
        /// <returns>编码后的文本</returns>
        public static string Encode(string text)
        {
            return Rotate(text, 13);
        }

        /// <summary>
        /// 使用 ROT13 解码文本（编码和解码相同）
        /// </summary>
        /// <param name="text">文本</param>
        /// <returns>解码后的文本</returns>
        public static string Decode(string text)
        {
            return Rotate(text, 13);
        }

        /// <summary>
        /// 使用 ROT13 编码文本（Encode 的别名）
        /// </summary>
        /// <param name="text">文本</param>
        /// <returns>编码后的文本</returns>
        public static string ROT13(string text)
        {
            return Rotate(text, 13);
        }

        /// <summary>
        /// 使用 ROT47 编码文本
        /// </summary>
        /// <param name="text">文本</param>
        /// <returns>编码后的文本</returns>
        public static string ROT47(string text)
        {
            return Rotate47(text);
        }

        /// <summary>
        /// 使用指定偏移量旋转字母
        /// </summary>
        /// <param name="text">文本</param>
        /// <param name="shift">偏移量</param>
        /// <returns>旋转后的文本</returns>
        public static string Rotate(string text, int shift)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            shift = ((shift % 26) + 26) % 26; // 标准化偏移量

            var result = new StringBuilder(text.Length);

            foreach (char c in text)
            {
                if (c >= 'A' && c <= 'Z')
                {
                    result.Append((char)('A' + (c - 'A' + shift) % 26));
                }
                else if (c >= 'a' && c <= 'z')
                {
                    result.Append((char)('a' + (c - 'a' + shift) % 26));
                }
                else
                {
                    result.Append(c);
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// 使用 ROT47 旋转字符
        /// </summary>
        /// <param name="text">文本</param>
        /// <returns>旋转后的文本</returns>
        public static string Rotate47(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            var result = new StringBuilder(text.Length);

            foreach (char c in text)
            {
                if (c >= 33 && c <= 126)
                {
                    result.Append((char)(33 + ((c - 33 + 47) % 94)));
                }
                else
                {
                    result.Append(c);
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// 检测文本是否可能是 ROT13 编码（启发式）
        /// </summary>
        /// <param name="text">文本</param>
        /// <returns>可能性评分（0-1）</returns>
        public static double DetectROT13(string text)
        {
            if (string.IsNullOrEmpty(text))
                return 0;

            int letterCount = 0;
            int nonLetterCount = 0;

            foreach (char c in text)
            {
                if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'))
                    letterCount++;
                else if (c >= 33 && c <= 126)
                    nonLetterCount++;
            }

            if (letterCount == 0)
                return 0;

            // ROT13 编码的文本通常有较高的字母比例
            return (double)letterCount / (letterCount + nonLetterCount);
        }
    }
}
