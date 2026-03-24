using System;
using System.Text;

namespace EasyTool.CodeCategory
{
    /// <summary>
    /// UUEncode 编码工具类
    /// UUEncode 是一种将二进制数据编码为 ASCII 文本的编码方式
    /// 早期用于 Unix 系统之间的文件传输
    /// </summary>
    public static class UUEncodeUtil
    {
        /// <summary>
        /// 将字节数组编码为 UUEncode 格式
        /// </summary>
        /// <param name="data">要编码的数据</param>
        /// <param name="fileName">文件名（可选）</param>
        /// <param name="mode">文件权限（默认 644）</param>
        /// <returns>UUEncode 编码字符串</returns>
        public static string Encode(byte[] data, string fileName = null, int mode = 644)
        {
            if (data == null || data.Length == 0)
                return string.Empty;

            var result = new StringBuilder();

            // 写入头行
            result.AppendLine($"begin {mode} {fileName ?? "file.bin"}");

            int offset = 0;
            while (offset < data.Length)
            {
                int lineLength = Math.Min(45, data.Length - offset);
                EncodeLine(data, offset, lineLength, result);
                offset += lineLength;
            }

            // 写入结束行
            result.AppendLine("`");
            result.AppendLine("end");

            return result.ToString();
        }

        /// <summary>
        /// 将 UUEncode 字符串解码为字节数组
        /// </summary>
        /// <param name="encoded">UUEncode 编码字符串</param>
        /// <returns>解码后的字节数组</returns>
        public static byte[] Decode(string encoded)
        {
            if (string.IsNullOrEmpty(encoded))
                return Array.Empty<byte>();

            var lines = encoded.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            var result = new System.Collections.Generic.List<byte>();
            bool inData = false;

            foreach (string line in lines)
            {
                if (line.StartsWith("begin "))
                {
                    inData = true;
                    continue;
                }

                if (line == "`" || line == "end")
                {
                    inData = false;
                    continue;
                }

                if (inData && line.Length > 0)
                {
                    DecodeLine(line, result);
                }
            }

            return result.ToArray();
        }

        /// <summary>
        /// 将字符串编码为 UUEncode（使用 UTF-8）
        /// </summary>
        public static string EncodeString(string text, string fileName = null)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            byte[] data = Encoding.UTF8.GetBytes(text);
            return Encode(data, fileName);
        }

        /// <summary>
        /// 将 UUEncode 字符串解码为文本（使用 UTF-8）
        /// </summary>
        public static string DecodeToString(string encoded)
        {
            byte[] data = Decode(encoded);
            return data.Length > 0 ? Encoding.UTF8.GetString(data) : string.Empty;
        }

        /// <summary>
        /// 验证 UUEncode 字符串是否有效
        /// </summary>
        public static bool IsValid(string encoded)
        {
            if (string.IsNullOrEmpty(encoded))
                return false;

            var lines = encoded.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            bool foundBegin = false;
            bool foundEnd = false;

            foreach (string line in lines)
            {
                if (line.StartsWith("begin "))
                {
                    foundBegin = true;
                    continue;
                }

                if (line == "end")
                {
                    foundEnd = true;
                    break;
                }

                if (foundBegin && line == "`")
                {
                    continue;
                }

                if (foundBegin && line.Length > 0)
                {
                    char lengthChar = line[0];
                    if (lengthChar < ' ' || lengthChar > 'M')
                        return false;
                }
            }

            return foundBegin && foundEnd;
        }

        private static void EncodeLine(byte[] data, int offset, int length, StringBuilder result)
        {
            // 行长度字符
            result.Append((char)(' ' + length));

            int i = 0;
            while (i < length)
            {
                byte b0 = data[offset + i];
                byte b1 = (i + 1 < length) ? data[offset + i + 1] : (byte)0;
                byte b2 = (i + 2 < length) ? data[offset + i + 2] : (byte)0;

                result.Append((char)(' ' + ((b0 >> 2) & 0x3F)));
                result.Append((char)(' ' + (((b0 << 4) | (b1 >> 4)) & 0x3F)));
                result.Append((char)(' ' + (((b1 << 2) | (b2 >> 6)) & 0x3F)));
                result.Append((char)(' ' + (b2 & 0x3F)));

                i += 3;
            }

            result.AppendLine();
        }

        private static void DecodeLine(string line, System.Collections.Generic.List<byte> result)
        {
            if (string.IsNullOrEmpty(line) || line[0] == '`')
                return;

            int length = line[0] - ' ';
            if (length < 0 || length > 45)
                return;

            int decodedLength = 0;

            for (int i = 1; i < line.Length && decodedLength < length; i += 4)
            {
                byte c0 = (byte)((i < line.Length ? line[i] : ' ') - ' ');
                byte c1 = (byte)((i + 1 < line.Length ? line[i + 1] : ' ') - ' ');
                byte c2 = (byte)((i + 2 < line.Length ? line[i + 2] : ' ') - ' ');
                byte c3 = (byte)((i + 3 < line.Length ? line[i + 3] : ' ') - ' ');

                byte b0 = (byte)((c0 << 2) | (c1 >> 4));
                byte b1 = (byte)((c1 << 4) | (c2 >> 2));
                byte b2 = (byte)((c2 << 6) | c3);

                if (decodedLength < length)
                {
                    result.Add(b0);
                    decodedLength++;
                }
                if (decodedLength < length)
                {
                    result.Add(b1);
                    decodedLength++;
                }
                if (decodedLength < length)
                {
                    result.Add(b2);
                    decodedLength++;
                }
            }
        }
    }
}
