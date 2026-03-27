using System;
using System.Collections.Generic;
using System.Text;

namespace EasyTool.TextCategory
{
    /// <summary>
    /// 文本编码检测工具类
    /// </summary>
    public static class EncodingDetector
    {
        /// <summary>
        /// 检测字节数组的编码
        /// </summary>
        public static Encoding Detect(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
                return Encoding.Default;

            // 检查BOM标记
            var bomEncoding = DetectByBom(bytes);
            if (bomEncoding != null)
                return bomEncoding;

            // 检查UTF-8
            if (IsValidUtf8(bytes))
                return Encoding.UTF8;

            // 检查是否为纯ASCII
            if (IsAscii(bytes))
                return Encoding.ASCII;

            // 尝试检测中文编码
            var chineseEncoding = DetectChineseEncoding(bytes);
            if (chineseEncoding != null)
                return chineseEncoding;

            // 默认返回系统默认编码
            return Encoding.Default;
        }

        /// <summary>
        /// 通过BOM标记检测编码
        /// </summary>
        public static Encoding? DetectByBom(byte[] bytes)
        {
            if (bytes.Length >= 3)
            {
                // UTF-8 BOM: EF BB BF
                if (bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
                    return Encoding.UTF8;

                // UTF-32 BE BOM: 00 00 FE FF
                if (bytes[0] == 0x00 && bytes[1] == 0x00 && bytes[2] == 0xFE && bytes[3] == 0xFF)
                    return Encoding.GetEncoding("UTF-32BE");

                // UTF-32 LE BOM: FF FE 00 00
                if (bytes[0] == 0xFF && bytes[1] == 0xFE && bytes[2] == 0x00 && bytes[3] == 0x00)
                    return Encoding.GetEncoding("UTF-32LE");
            }

            if (bytes.Length >= 2)
            {
                // UTF-16 BE BOM: FE FF
                if (bytes[0] == 0xFE && bytes[1] == 0xFF)
                    return Encoding.BigEndianUnicode;

                // UTF-16 LE BOM: FF FE
                if (bytes[0] == 0xFF && bytes[1] == 0xFE)
                    return Encoding.Unicode;
            }

            return null;
        }

        /// <summary>
        /// 验证是否为有效的UTF-8
        /// </summary>
        public static bool IsValidUtf8(byte[] bytes)
        {
            int i = 0;
            while (i < bytes.Length)
            {
                byte b = bytes[i];

                if (b <= 0x7F)
                {
                    // ASCII字符
                    i++;
                }
                else if ((b & 0xE0) == 0xC0)
                {
                    // 2字节序列
                    if (i + 1 >= bytes.Length) return false;
                    if ((bytes[i + 1] & 0xC0) != 0x80) return false;
                    i += 2;
                }
                else if ((b & 0xF0) == 0xE0)
                {
                    // 3字节序列
                    if (i + 2 >= bytes.Length) return false;
                    if ((bytes[i + 1] & 0xC0) != 0x80) return false;
                    if ((bytes[i + 2] & 0xC0) != 0x80) return false;
                    i += 3;
                }
                else if ((b & 0xF8) == 0xF0)
                {
                    // 4字节序列
                    if (i + 3 >= bytes.Length) return false;
                    if ((bytes[i + 1] & 0xC0) != 0x80) return false;
                    if ((bytes[i + 2] & 0xC0) != 0x80) return false;
                    if ((bytes[i + 3] & 0xC0) != 0x80) return false;
                    i += 4;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 检测是否为纯ASCII
        /// </summary>
        public static bool IsAscii(byte[] bytes)
        {
            foreach (var b in bytes)
            {
                if (b > 0x7F)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 检测中文编码（GB2312、GBK、Big5）
        /// </summary>
        public static Encoding? DetectChineseEncoding(byte[] bytes)
        {
            // 尝试GB2312
            if (TryDecode(bytes, Encoding.GetEncoding("GB2312"), out var gb2312Text))
            {
                if (ContainsValidChinese(gb2312Text))
                {
                    // 进一步检查是否更可能是GBK
                    if (IsLikelyGbk(bytes))
                        return Encoding.GetEncoding("GBK");
                    return Encoding.GetEncoding("GB2312");
                }
            }

            // 尝试Big5（繁体中文）
            if (TryDecode(bytes, Encoding.GetEncoding("Big5"), out var big5Text))
            {
                if (ContainsValidChinese(big5Text))
                    return Encoding.GetEncoding("Big5");
            }

            return null;
        }

        private static bool TryDecode(byte[] bytes, Encoding encoding, out string text)
        {
            try
            {
                text = encoding.GetString(bytes);
                return true;
            }
            catch
            {
                text = "";
                return false;
            }
        }

        private static bool ContainsValidChinese(string text)
        {
            int chineseCount = 0;
            int otherCount = 0;

            foreach (var c in text)
            {
                if (IsChineseCharacter(c))
                    chineseCount++;
                else if (c > 127)
                    otherCount++;
            }

            // 如果中文字符占比高，认为是有效的中文编码
            return chineseCount > 0 && (chineseCount > otherCount || otherCount == 0);
        }

        private static bool IsChineseCharacter(char c)
        {
            // CJK统一汉字范围（BMP内的字符）
            return c >= '\u4E00' && c <= '\u9FFF' ||
                   c >= '\u3400' && c <= '\u4DBF' ||
                   c >= '\uF900' && c <= '\uFAFF';
        }

        private static bool IsLikelyGbk(byte[] bytes)
        {
            int gbkSpecific = 0;

            for (int i = 0; i < bytes.Length - 1; i++)
            {
                byte b1 = bytes[i];
                byte b2 = bytes[i + 1];

                // GBK扩展字符范围
                if (b1 >= 0x81 && b1 <= 0xFE)
                {
                    // GB2312范围
                    if (b2 >= 0x40 && b2 <= 0xFE && b2 != 0x7F)
                    {
                        // GBK特有字符（GB2312之外的）
                        if (b2 >= 0x40 && b2 <= 0x7E)
                        {
                            gbkSpecific++;
                        }
                    }
                }
            }

            return gbkSpecific > bytes.Length / 50; // 约2%的GBK特有字符
        }

        /// <summary>
        /// 尝试将字节数组转换为字符串
        /// </summary>
        public static string GetString(byte[] bytes, Encoding? preferredEncoding = null)
        {
            var encoding = preferredEncoding ?? Detect(bytes);
            return encoding.GetString(bytes);
        }

        /// <summary>
        /// 从文件读取文本（自动检测编码）
        /// </summary>
        public static string ReadFileText(string filePath, Encoding? preferredEncoding = null)
        {
            var bytes = System.IO.File.ReadAllBytes(filePath);
            return GetString(bytes, preferredEncoding);
        }

        /// <summary>
        /// 获取编码名称
        /// </summary>
        public static string GetEncodingName(Encoding encoding)
        {
            return encoding.WebName.ToUpperInvariant() switch
            {
                "UTF-8" => "UTF-8",
                "UTF-16" => "UTF-16 LE",
                "UTF-16BE" => "UTF-16 BE",
                "UTF-32" => "UTF-32 LE",
                "UTF-32BE" => "UTF-32 BE",
                "GB2312" => "GB2312",
                "GBK" => "GBK",
                "BIG5" => "Big5",
                "US-ASCII" => "ASCII",
                "ISO-8859-1" => "ISO-8859-1",
                _ => encoding.WebName.ToUpperInvariant()
            };
        }
    }
}
