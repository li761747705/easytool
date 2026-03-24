using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EasyTool.IOCategory
{
    /// <summary>
    /// 文件签名（魔数）检测工具类
    /// 通过文件头字节判断真实文件类型
    /// </summary>
    public static class FileSignatureUtil
    {
        /// <summary>
        /// 常见文件签名
        /// </summary>
        private static readonly List<FileSignature> Signatures = new()
        {
            // 图片
            new("jpg", "JPEG Image", new[] { "FF D8 FF" }, new[] { ".jpg", ".jpeg" }),
            new("png", "PNG Image", new[] { "89 50 4E 47 0D 0A 1A 0A" }, new[] { ".png" }),
            new("gif", "GIF Image", new[] { "47 49 46 38 37 61", "47 49 46 38 39 61" }, new[] { ".gif" }),
            new("bmp", "BMP Image", new[] { "42 4D" }, new[] { ".bmp" }),
            new("webp", "WebP Image", new[] { "52 49 46 46 ?? ?? ?? ?? 57 45 42 50" }, new[] { ".webp" }),
            new("ico", "ICO Image", new[] { "00 00 01 00" }, new[] { ".ico" }),
            new("svg", "SVG Image", new[] { "3C 3F 78 6D 6C", "3C 73 76 67" }, new[] { ".svg" }),
            new("tiff", "TIFF Image", new[] { "49 49 2A 00", "4D 4D 00 2A" }, new[] { ".tiff", ".tif" }),

            // 文档
            new("pdf", "PDF Document", new[] { "25 50 44 46" }, new[] { ".pdf" }),
            new("doc", "Word Document (old)", new[] { "D0 CF 11 E0 A1 B1 1A E1" }, new[] { ".doc", ".xls", ".ppt" }),
            new("docx", "Word Document", new[] { "50 4B 03 04 14 00 06 00" }, new[] { ".docx", ".xlsx", ".pptx" }),
            new("rtf", "RTF Document", new[] { "7B 5C 72 74 66 31" }, new[] { ".rtf" }),

            // 压缩
            new("zip", "ZIP Archive", new[] { "50 4B 03 04", "50 4B 05 06", "50 4B 07 08" }, new[] { ".zip" }),
            new("rar", "RAR Archive", new[] { "52 61 72 21 1A 07" }, new[] { ".rar" }),
            new("7z", "7-Zip Archive", new[] { "37 7A BC AF 27 1C" }, new[] { ".7z" }),
            new("tar", "TAR Archive", new[] { "75 73 74 61 72" }, new[] { ".tar" }, 257),
            new("gz", "GZIP Archive", new[] { "1F 8B" }, new[] { ".gz", ".gzip" }),
            new("bz2", "BZIP2 Archive", new[] { "42 5A 68" }, new[] { ".bz2" }),

            // 音频
            new("mp3", "MP3 Audio", new[] { "FF FB", "FF FA", "FF F3", "FF F2", "49 44 33" }, new[] { ".mp3" }),
            new("wav", "WAV Audio", new[] { "52 49 46 46 ?? ?? ?? ?? 57 41 56 45" }, new[] { ".wav" }),
            new("flac", "FLAC Audio", new[] { "66 4C 61 43" }, new[] { ".flac" }),
            new("m4a", "M4A Audio", new[] { "66 74 79 70 4D 34 41" }, new[] { ".m4a" }),
            new("ogg", "OGG Audio", new[] { "4F 67 67 53" }, new[] { ".ogg" }),

            // 视频
            new("mp4", "MP4 Video", new[] { "66 74 79 70 69 73 6F 6D", "66 74 79 70 6D 70 34 32" }, new[] { ".mp4" }),
            new("avi", "AVI Video", new[] { "52 49 46 46 ?? ?? ?? ?? 41 56 49 20" }, new[] { ".avi" }),
            new("mkv", "MKV Video", new[] { "1A 45 DF A3" }, new[] { ".mkv", ".webm" }),
            new("mov", "MOV Video", new[] { "66 74 79 70 71 74 20 20" }, new[] { ".mov" }),
            new("flv", "FLV Video", new[] { "46 4C 56" }, new[] { ".flv" }),
            new("wmv", "WMV Video", new[] { "30 26 B2 75 8E 66 CF 11" }, new[] { ".wmv", ".asf" }),

            // 可执行
            new("exe", "Windows Executable", new[] { "4D 5A" }, new[] { ".exe", ".dll" }),
            new("elf", "Linux Executable", new[] { "7F 45 4C 46" }, new[] { "" }),
            new("class", "Java Class", new[] { "CA FE BA BE" }, new[] { ".class" }),
            new("dex", "Android DEX", new[] { "64 65 78 0A 30 33 35" }, new[] { ".dex" }),
            new("apk", "Android APK", new[] { "50 4B 03 04" }, new[] { ".apk" }),

            // 其他
            new("sqlite", "SQLite Database", new[] { "53 51 4C 69 74 65 21" }, new[] { ".sqlite", ".db" }),
            new("psd", "Photoshop Document", new[] { "38 42 50 53" }, new[] { ".psd" }),
            new("ai", "Adobe Illustrator", new[] { "25 50 44 46" }, new[] { ".ai" }),
            new("swf", "Flash SWF", new[] { "46 57 53", "43 57 53" }, new[] { ".swf" }),
            new("torrent", "Torrent File", new[] { "64 38 3A 61 6E 6E 6F 75 6E 63 65" }, new[] { ".torrent" }),
        };

        /// <summary>
        /// 检测文件类型
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>文件类型信息</returns>
        public static FileTypeInfo? Detect(string filePath)
        {
            if (!File.Exists(filePath))
                return null;

            using var stream = File.OpenRead(filePath);
            return Detect(stream);
        }

        /// <summary>
        /// 检测文件类型
        /// </summary>
        /// <param name="stream">文件流</param>
        /// <returns>文件类型信息</returns>
        public static FileTypeInfo? Detect(Stream stream)
        {
            if (stream == null || stream.Length == 0)
                return null;

            var header = new byte[Math.Min(32, stream.Length)];
            var originalPosition = stream.Position;
            stream.Position = 0;
            stream.Read(header, 0, header.Length);
            stream.Position = originalPosition;

            return DetectFromHeader(header);
        }

        /// <summary>
        /// 从字节数组检测文件类型
        /// </summary>
        /// <param name="bytes">文件字节数组</param>
        /// <returns>文件类型信息</returns>
        public static FileTypeInfo? DetectFromBytes(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
                return null;

            var header = new byte[Math.Min(32, bytes.Length)];
            Array.Copy(bytes, header, header.Length);

            return DetectFromHeader(header);
        }

        /// <summary>
        /// 从文件头检测文件类型
        /// </summary>
        /// <param name="header">文件头字节</param>
        /// <returns>文件类型信息</returns>
        public static FileTypeInfo? DetectFromHeader(byte[] header)
        {
            if (header == null || header.Length == 0)
                return null;

            foreach (var signature in Signatures)
            {
                foreach (var pattern in signature.Patterns)
                {
                    if (MatchesPattern(header, pattern, signature.Offset))
                    {
                        return new FileTypeInfo
                        {
                            TypeId = signature.TypeId,
                            Description = signature.Description,
                            Extensions = signature.Extensions
                        };
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 验证文件扩展名是否与实际内容匹配
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>是否匹配</returns>
        public static bool ValidateExtension(string filePath)
        {
            var detected = Detect(filePath);
            if (detected == null)
                return true; // 无法检测时默认通过

            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            return detected.Extensions.Contains(extension);
        }

        /// <summary>
        /// 获取文件的真实扩展名
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>扩展名（包含点号）</returns>
        public static string? GetRealExtension(string filePath)
        {
            var detected = Detect(filePath);
            return detected?.Extensions.FirstOrDefault();
        }

        /// <summary>
        /// 检查文件是否为图片
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>是否为图片</returns>
        public static bool IsImage(string filePath)
        {
            var detected = Detect(filePath);
            return detected?.TypeId is "jpg" or "png" or "gif" or "bmp" or "webp" or "ico" or "svg" or "tiff";
        }

        /// <summary>
        /// 检查文件是否为视频
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>是否为视频</returns>
        public static bool IsVideo(string filePath)
        {
            var detected = Detect(filePath);
            return detected?.TypeId is "mp4" or "avi" or "mkv" or "mov" or "flv" or "wmv";
        }

        /// <summary>
        /// 检查文件是否为音频
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>是否为音频</returns>
        public static bool IsAudio(string filePath)
        {
            var detected = Detect(filePath);
            return detected?.TypeId is "mp3" or "wav" or "flac" or "m4a" or "ogg";
        }

        /// <summary>
        /// 检查文件是否为文档
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>是否为文档</returns>
        public static bool IsDocument(string filePath)
        {
            var detected = Detect(filePath);
            return detected?.TypeId is "pdf" or "doc" or "docx" or "rtf";
        }

        /// <summary>
        /// 检查文件是否为压缩包
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>是否为压缩包</returns>
        public static bool IsArchive(string filePath)
        {
            var detected = Detect(filePath);
            return detected?.TypeId is "zip" or "rar" or "7z" or "tar" or "gz" or "bz2";
        }

        /// <summary>
        /// 检查文件是否为可执行文件
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>是否为可执行文件</returns>
        public static bool IsExecutable(string filePath)
        {
            var detected = Detect(filePath);
            return detected?.TypeId is "exe" or "elf" or "class" or "dex";
        }

        private static bool MatchesPattern(byte[] header, string pattern, int offset = 0)
        {
            var patternBytes = pattern.Split(' ');
            var requiredLength = offset + patternBytes.Length;

            if (header.Length < requiredLength)
                return false;

            for (int i = 0; i < patternBytes.Length; i++)
            {
                var patternByte = patternBytes[i];
                var headerByte = header[offset + i];

                if (patternByte == "??")
                    continue;

                if (!byte.TryParse(patternByte, System.Globalization.NumberStyles.HexNumber, null, out var expectedByte))
                    continue;

                if (headerByte != expectedByte)
                    return false;
            }

            return true;
        }

        #region 内部类

        private class FileSignature
        {
            public string TypeId { get; }
            public string Description { get; }
            public string[] Patterns { get; }
            public string[] Extensions { get; }
            public int Offset { get; }

            public FileSignature(string typeId, string description, string[] patterns, string[] extensions, int offset = 0)
            {
                TypeId = typeId;
                Description = description;
                Patterns = patterns;
                Extensions = extensions;
                Offset = offset;
            }
        }

        #endregion
    }

    /// <summary>
    /// 文件类型信息
    /// </summary>
    public class FileTypeInfo
    {
        /// <summary>
        /// 类型标识
        /// </summary>
        public string TypeId { get; set; } = string.Empty;

        /// <summary>
        /// 类型描述
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 可能的文件扩展名
        /// </summary>
        public string[] Extensions { get; set; } = Array.Empty<string>();

        public override string ToString() => $"{TypeId} ({Description})";
    }
}
