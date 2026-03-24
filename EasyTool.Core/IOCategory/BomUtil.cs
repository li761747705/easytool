using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EasyTool.IOCategory
{
    /// <summary>
    /// BOM（字节顺序标记）工具类
    /// 处理不同编码的 BOM
    /// </summary>
    public static class BomUtil
    {
        /// <summary>
        /// BOM 定义
        /// </summary>
        public static readonly Dictionary<string, byte[]> BomDefinitions = new()
        {
            {"UTF-8", new byte[] {0xEF, 0xBB, 0xBF}},
            {"UTF-16BE", new byte[] {0xFE, 0xFF}},
            {"UTF-16LE", new byte[] {0xFF, 0xFE}},
            {"UTF-32BE", new byte[] {0x00, 0x00, 0xFE, 0xFF}},
            {"UTF-32LE", new byte[] {0xFF, 0xFE, 0x00, 0x00}},
            {"UTF-7", new byte[] {0x2B, 0x2F, 0x76}},
            {"UTF-1", new byte[] {0xF7, 0x64, 0x4C}},
            {"UTF-EBCDIC", new byte[] {0xDD, 0x73, 0x66, 0x73}},
            {"SCSU", new byte[] {0x0E, 0xFE, 0xFF}},
            {"BOCU-1", new byte[] {0xFB, 0xEE, 0x28}},
            {"GB-18030", new byte[] {0x84, 0x31, 0x95, 0x33}},
        };

        /// <summary>
        /// 检测文件的 BOM
        /// </summary>
        public static BomInfo Detect(string filePath)
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return Detect(stream);
        }

        /// <summary>
        /// 检测流的 BOM
        /// </summary>
        public static BomInfo Detect(Stream stream)
        {
            byte[] buffer = new byte[4];
            int bytesRead = stream.Read(buffer, 0, 4);

            // 重置流位置
            if (stream.CanSeek)
                stream.Position = 0;

            return Detect(buffer, bytesRead);
        }

        /// <summary>
        /// 检测字节数组的 BOM
        /// </summary>
        public static BomInfo Detect(byte[] bytes)
        {
            return Detect(bytes, bytes.Length);
        }

        private static BomInfo Detect(byte[] bytes, int length)
        {
            if (length < 2)
                return new BomInfo { HasBom = false, Encoding = null, BomLength = 0 };

            // UTF-8
            if (length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
            {
                return new BomInfo { HasBom = true, Encoding = Encoding.UTF8, BomLength = 3 };
            }

            // UTF-32BE
            if (length >= 4 && bytes[0] == 0x00 && bytes[1] == 0x00 && bytes[2] == 0xFE && bytes[3] == 0xFF)
            {
                return new BomInfo { HasBom = true, Encoding = Encoding.GetEncoding("utf-32BE"), BomLength = 4 };
            }

            // UTF-32LE
            if (length >= 4 && bytes[0] == 0xFF && bytes[1] == 0xFE && bytes[2] == 0x00 && bytes[3] == 0x00)
            {
                return new BomInfo { HasBom = true, Encoding = Encoding.GetEncoding("utf-32LE"), BomLength = 4 };
            }

            // UTF-16BE
            if (length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF)
            {
                return new BomInfo { HasBom = true, Encoding = Encoding.BigEndianUnicode, BomLength = 2 };
            }

            // UTF-16LE
            if (length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE)
            {
                return new BomInfo { HasBom = true, Encoding = Encoding.Unicode, BomLength = 2 };
            }

            // UTF-7 (可能有变体)
            if (length >= 3 && bytes[0] == 0x2B && bytes[1] == 0x2F && bytes[2] == 0x76)
            {
                return new BomInfo { HasBom = true, Encoding = Encoding.UTF7, BomLength = 3 };
            }

            return new BomInfo { HasBom = false, Encoding = null, BomLength = 0 };
        }

        /// <summary>
        /// 移除文件的 BOM
        /// </summary>
        public static void Remove(string filePath)
        {
            var bom = Detect(filePath);
            if (!bom.HasBom) return;

            byte[] content = File.ReadAllBytes(filePath);
            byte[] newContent = new byte[content.Length - bom.BomLength];
            Array.Copy(content, bom.BomLength, newContent, 0, newContent.Length);
            File.WriteAllBytes(filePath, newContent);
        }

        /// <summary>
        /// 为文件添加 BOM
        /// </summary>
        public static void Add(string filePath, Encoding encoding)
        {
            byte[] bom = GetBom(encoding);
            if (bom == null) return;

            var bomInfo = Detect(filePath);
            if (bomInfo.HasBom) return;

            byte[] content = File.ReadAllBytes(filePath);
            byte[] newContent = new byte[bom.Length + content.Length];
            Array.Copy(bom, 0, newContent, 0, bom.Length);
            Array.Copy(content, 0, newContent, bom.Length, content.Length);
            File.WriteAllBytes(filePath, newContent);
        }

        /// <summary>
        /// 获取指定编码的 BOM
        /// </summary>
        public static byte[] GetBom(Encoding encoding)
        {
            if (encoding == null)
                return null;

            // 使用内置方法获取 BOM
            if (encoding.Equals(Encoding.UTF8))
                return Encoding.UTF8.GetPreamble();
            if (encoding.Equals(Encoding.Unicode))
                return Encoding.Unicode.GetPreamble();
            if (encoding.Equals(Encoding.BigEndianUnicode))
                return Encoding.BigEndianUnicode.GetPreamble();
            if (encoding.Equals(Encoding.UTF32))
                return Encoding.UTF32.GetPreamble();

            return null;
        }

        /// <summary>
        /// 读取文件内容（自动处理 BOM）
        /// </summary>
        public static string ReadAllText(string filePath)
        {
            var bom = Detect(filePath);
            Encoding encoding = bom.Encoding ?? Encoding.UTF8;

            byte[] bytes = File.ReadAllBytes(filePath);
            int offset = bom.HasBom ? bom.BomLength : 0;
            int length = bytes.Length - offset;

            return encoding.GetString(bytes, offset, length);
        }

        /// <summary>
        /// 写入文件内容（可选是否添加 BOM）
        /// </summary>
        public static void WriteAllText(string filePath, string content, Encoding encoding, bool includeBom = true)
        {
            if (includeBom)
            {
                byte[] bom = GetBom(encoding);
                byte[] contentBytes = encoding.GetBytes(content);

                if (bom != null && bom.Length > 0)
                {
                    byte[] allBytes = new byte[bom.Length + contentBytes.Length];
                    Array.Copy(bom, 0, allBytes, 0, bom.Length);
                    Array.Copy(contentBytes, 0, allBytes, bom.Length, contentBytes.Length);
                    File.WriteAllBytes(filePath, allBytes);
                    return;
                }
            }

            File.WriteAllText(filePath, content, encoding);
        }

        /// <summary>
        /// 转换文件编码（处理 BOM）
        /// </summary>
        public static void Convert(string filePath, Encoding targetEncoding, bool includeBom = true)
        {
            string content = ReadAllText(filePath);
            WriteAllText(filePath, content, targetEncoding, includeBom);
        }
    }

    /// <summary>
    /// BOM 信息
    /// </summary>
    public class BomInfo
    {
        /// <summary>
        /// 是否有 BOM
        /// </summary>
        public bool HasBom { get; set; }

        /// <summary>
        /// 编码
        /// </summary>
        public Encoding Encoding { get; set; }

        /// <summary>
        /// BOM 长度（字节）
        /// </summary>
        public int BomLength { get; set; }

        public override string ToString()
        {
            return HasBom
                ? $"Has BOM: {Encoding?.WebName ?? "Unknown"}, Length: {BomLength}"
                : "No BOM detected";
        }
    }
}
