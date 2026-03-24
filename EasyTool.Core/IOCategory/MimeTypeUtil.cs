using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EasyTool.IOCategory
{
    /// <summary>
    /// MIME 类型工具类
    /// 提供根据文件扩展名和文件内容检测 MIME 类型的功能
    /// </summary>
    public static class MimeTypeUtil
    {
        private static readonly Dictionary<string, string> ExtensionToMimeType = new(StringComparer.OrdinalIgnoreCase)
        {
            // 文本
            {".txt", "text/plain"},
            {".html", "text/html"},
            {".htm", "text/html"},
            {".css", "text/css"},
            {".js", "application/javascript"},
            {".json", "application/json"},
            {".xml", "application/xml"},
            {".csv", "text/csv"},
            {".md", "text/markdown"},
            {".yaml", "text/yaml"},
            {".yml", "text/yaml"},

            // 图片
            {".jpg", "image/jpeg"},
            {".jpeg", "image/jpeg"},
            {".png", "image/png"},
            {".gif", "image/gif"},
            {".bmp", "image/bmp"},
            {".ico", "image/x-icon"},
            {".svg", "image/svg+xml"},
            {".webp", "image/webp"},
            {".tiff", "image/tiff"},
            {".tif", "image/tiff"},

            // 音频
            {".mp3", "audio/mpeg"},
            {".wav", "audio/wav"},
            {".ogg", "audio/ogg"},
            {".flac", "audio/flac"},
            {".aac", "audio/aac"},
            {".wma", "audio/x-ms-wma"},
            {".m4a", "audio/mp4"},

            // 视频
            {".mp4", "video/mp4"},
            {".avi", "video/x-msvideo"},
            {".mkv", "video/x-matroska"},
            {".mov", "video/quicktime"},
            {".wmv", "video/x-ms-wmv"},
            {".flv", "video/x-flv"},
            {".webm", "video/webm"},
            {".m4v", "video/mp4"},

            // 文档
            {".pdf", "application/pdf"},
            {".doc", "application/msword"},
            {".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"},
            {".xls", "application/vnd.ms-excel"},
            {".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},
            {".ppt", "application/vnd.ms-powerpoint"},
            {".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation"},

            // 压缩
            {".zip", "application/zip"},
            {".rar", "application/x-rar-compressed"},
            {".7z", "application/x-7z-compressed"},
            {".tar", "application/x-tar"},
            {".gz", "application/gzip"},
            {".bz2", "application/x-bzip2"},

            // 可执行
            {".exe", "application/x-msdownload"},
            {".msi", "application/x-msi"},
            {".dll", "application/x-msdownload"},
            {".so", "application/x-sharedlib"},
            {".dylib", "application/x-sharedlib"},
            {".jar", "application/java-archive"},
            {".apk", "application/vnd.android.package-archive"},

            // 代码
            {".cs", "text/x-csharp"},
            {".java", "text/x-java-source"},
            {".py", "text/x-python"},
            {".rb", "text/x-ruby"},
            {".php", "text/x-php"},
            {".cpp", "text/x-c++src"},
            {".c", "text/x-csrc"},
            {".h", "text/x-chdr"},
            {".go", "text/x-go"},
            {".rs", "text/x-rust"},
            {".swift", "text/x-swift"},
            {".kt", "text/x-kotlin"},
            {".ts", "text/typescript"},
            {".tsx", "text/typescript-jsx"},

            // 字体
            {".ttf", "font/ttf"},
            {".otf", "font/otf"},
            {".woff", "font/woff"},
            {".woff2", "font/woff2"},
            {".eot", "application/vnd.ms-fontobject"},

            // 其他
            {".bin", "application/octet-stream"},
            {".dat", "application/octet-stream"},
        };

        private static readonly Dictionary<string, byte[]> FileSignatures = new()
        {
            {"image/jpeg", new byte[] {0xFF, 0xD8, 0xFF}},
            {"image/png", new byte[] {0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A}},
            {"image/gif", new byte[] {0x47, 0x49, 0x46, 0x38}},
            {"image/bmp", new byte[] {0x42, 0x4D}},
            {"application/pdf", new byte[] {0x25, 0x50, 0x44, 0x46}},
            {"application/zip", new byte[] {0x50, 0x4B, 0x03, 0x04}},
            {"application/x-rar-compressed", new byte[] {0x52, 0x61, 0x72, 0x21}},
            {"application/x-7z-compressed", new byte[] {0x37, 0x7A, 0xBC, 0xAF}},
            {"video/mp4", new byte[] {0x00, 0x00, 0x00, 0x18, 0x66, 0x74, 0x79, 0x70}},
            {"audio/mpeg", new byte[] {0xFF, 0xFB}},
            {"application/java-archive", new byte[] {0x50, 0x4B, 0x03, 0x04}},
        };

        /// <summary>
        /// 根据文件扩展名获取 MIME 类型
        /// </summary>
        public static string GetByExtension(string extension)
        {
            if (string.IsNullOrEmpty(extension))
                return "application/octet-stream";

            if (!extension.StartsWith("."))
                extension = "." + extension;

            return ExtensionToMimeType.TryGetValue(extension, out string mime)
                ? mime
                : "application/octet-stream";
        }

        /// <summary>
        /// 根据文件路径获取 MIME 类型
        /// </summary>
        public static string GetByPath(string filePath)
        {
            return GetByExtension(Path.GetExtension(filePath));
        }

        /// <summary>
        /// 根据文件内容检测 MIME 类型
        /// </summary>
        public static string DetectByContent(string filePath)
        {
            if (!File.Exists(filePath))
                return "application/octet-stream";

            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return DetectByContent(stream);
        }

        /// <summary>
        /// 根据流内容检测 MIME 类型
        /// </summary>
        public static string DetectByContent(Stream stream)
        {
            byte[] header = new byte[16];
            int bytesRead = stream.Read(header, 0, header.Length);

            if (bytesRead == 0)
                return "application/octet-stream";

            foreach (var signature in FileSignatures)
            {
                if (header.Length >= signature.Value.Length)
                {
                    bool match = true;
                    for (int i = 0; i < signature.Value.Length; i++)
                    {
                        if (header[i] != signature.Value[i])
                        {
                            match = false;
                            break;
                        }
                    }
                    if (match)
                        return signature.Key;
                }
            }

            // 检查是否为文本
            if (IsTextContent(header, bytesRead))
                return "text/plain";

            return "application/octet-stream";
        }

        private static bool IsTextContent(byte[] data, int length)
        {
            for (int i = 0; i < length; i++)
            {
                byte b = data[i];
                // 允许的控制字符：换行、回车、制表符
                if (b < 32 && b != 9 && b != 10 && b != 13)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 组合检测（先检测内容，再根据扩展名补充）
        /// </summary>
        public static string Detect(string filePath)
        {
            string byContent = DetectByContent(filePath);
            if (byContent != "application/octet-stream")
                return byContent;

            return GetByPath(filePath);
        }

        /// <summary>
        /// 根据 MIME 类型获取文件扩展名
        /// </summary>
        public static string GetExtension(string mimeType)
        {
            if (string.IsNullOrEmpty(mimeType))
                return ".bin";

            var entry = ExtensionToMimeType.FirstOrDefault(x =>
                x.Value.Equals(mimeType, StringComparison.OrdinalIgnoreCase));

            return entry.Key ?? ".bin";
        }

        /// <summary>
        /// 判断是否为图片
        /// </summary>
        public static bool IsImage(string mimeType)
        {
            return mimeType?.StartsWith("image/") == true;
        }

        /// <summary>
        /// 判断是否为音频
        /// </summary>
        public static bool IsAudio(string mimeType)
        {
            return mimeType?.StartsWith("audio/") == true;
        }

        /// <summary>
        /// 判断是否为视频
        /// </summary>
        public static bool IsVideo(string mimeType)
        {
            return mimeType?.StartsWith("video/") == true;
        }

        /// <summary>
        /// 判断是否为文本
        /// </summary>
        public static bool IsText(string mimeType)
        {
            return mimeType?.StartsWith("text/") == true ||
                   mimeType == "application/json" ||
                   mimeType == "application/xml" ||
                   mimeType == "application/javascript";
        }

        /// <summary>
        /// 注册自定义 MIME 类型
        /// </summary>
        public static void Register(string extension, string mimeType)
        {
            if (!extension.StartsWith("."))
                extension = "." + extension;

            ExtensionToMimeType[extension] = mimeType;
        }
    }
}
