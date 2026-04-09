using System;
using System.Collections.Generic;
using System.IO;

namespace EasyTool
{
    /// <summary>
    /// 文件类型工具类
    /// </summary>
    public static class FileTypeUtil
    {
        private static readonly Dictionary<string, string> FileTypeDict = new Dictionary<string, string>
        {
            { "FFD8FF", ".jpg" },
            { "89504E47", ".png" },
            { "47494638", ".gif" },
            { "424D", ".bmp" },
            { "4D5A", ".exe" },
            { "3C3F786D", ".xml" },
            { "3C21644F", ".html" },
            { "25504446", ".pdf" },
            { "504B0304", ".zip" },
            { "52617221", ".rar" },
            { "D0CF11E0", ".doc" },
            { "00000100", ".ico" },
            { "494433", ".mp3" },
            { "00000018667479", ".mp4" },
            { "66747970", ".mp4" },
            { "00000020", ".mp4" },
        };

        /// <summary>
        /// 通过文件流头部信息获得文件类型
        /// </summary>
        /// <param name="file">文件信息</param>
        /// <returns>文件扩展名，未找到则返回原始扩展名</returns>
        public static string? GetType(FileInfo file)
        {
            if (!file.Exists)
            {
                return file.Extension;
            }

            byte[] buffer = new byte[8];
            using (FileStream fs = file.OpenRead())
            {
                int readLength = fs.Read(buffer, 0, buffer.Length);
                if (readLength < 2)
                {
                    return file.Extension;
                }
            }

            string header = BitConverter.ToString(buffer).Replace("-", "").ToUpperInvariant();

            foreach (var kvp in FileTypeDict)
            {
                if (header.StartsWith(kvp.Key))
                {
                    return kvp.Value;
                }
            }

            return file.Extension;
        }

        /// <summary>
        /// 通过文件路径获得文件类型
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>文件扩展名</returns>
        public static string? GetType(string filePath)
        {
            FileInfo file = new FileInfo(filePath);
            return GetType(file);
        }

        /// <summary>
        /// 通过文件字节流获得文件类型
        /// </summary>
        /// <param name="fileBytes">文件字节流</param>
        /// <returns>文件扩展名</returns>
        public static string? GetType(byte[] fileBytes)
        {
            if (fileBytes == null || fileBytes.Length < 2)
            {
                return null;
            }

            byte[] buffer = new byte[Math.Min(8, fileBytes.Length)];
            Array.Copy(fileBytes, buffer, buffer.Length);

            string header = BitConverter.ToString(buffer).Replace("-", "").ToUpperInvariant();

            foreach (var kvp in FileTypeDict)
            {
                if (header.StartsWith(kvp.Key))
                {
                    return kvp.Value;
                }
            }

            return null;
        }

        /// <summary>
        /// 检查文件是否为图片
        /// </summary>
        /// <param name="file">文件信息</param>
        /// <returns>是否为图片</returns>
        public static bool IsImage(FileInfo file)
        {
            var type = GetType(file);
            if (type == null) return false;

            var imageTypes = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tif", ".tiff", ".webp", ".svg", ".ico" };
            return Array.IndexOf(imageTypes, type.ToLowerInvariant()) >= 0;
        }

        /// <summary>
        /// 检查文件是否为视频
        /// </summary>
        /// <param name="file">文件信息</param>
        /// <returns>是否为视频</returns>
        public static bool IsVideo(FileInfo file)
        {
            var type = GetType(file);
            if (type == null) return false;

            var videoTypes = new[] { ".mp4", ".avi", ".mov", ".wmv", ".flv", ".mkv", ".webm", ".m4v", ".mpeg", ".mpg" };
            return Array.IndexOf(videoTypes, type.ToLowerInvariant()) >= 0;
        }

        /// <summary>
        /// 检查文件是否为音频
        /// </summary>
        /// <param name="file">文件信息</param>
        /// <returns>是否为音频</returns>
        public static bool IsAudio(FileInfo file)
        {
            var type = GetType(file);
            if (type == null) return false;

            var audioTypes = new[] { ".mp3", ".wav", ".flac", ".aac", ".ogg", ".wma", ".m4a", ".ape", ".mid", ".midi" };
            return Array.IndexOf(audioTypes, type.ToLowerInvariant()) >= 0;
        }

        /// <summary>
        /// 检查文件是否为文档
        /// </summary>
        /// <param name="file">文件信息</param>
        /// <returns>是否为文档</returns>
        public static bool IsDocument(FileInfo file)
        {
            var type = GetType(file);
            if (type == null) return false;

            var docTypes = new[] { ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".pdf", ".txt", ".rtf", ".odt", ".ods", ".odp" };
            return Array.IndexOf(docTypes, type.ToLowerInvariant()) >= 0;
        }

        /// <summary>
        /// 检查文件是否为压缩文件
        /// </summary>
        /// <param name="file">文件信息</param>
        /// <returns>是否为压缩文件</returns>
        public static bool IsArchive(FileInfo file)
        {
            var type = GetType(file);
            if (type == null) return false;

            var archiveTypes = new[] { ".zip", ".rar", ".7z", ".tar", ".gz", ".bz2", ".xz", ".jar", ".war" };
            return Array.IndexOf(archiveTypes, type.ToLowerInvariant()) >= 0;
        }

        /// <summary>
        /// 检查文件是否为可执行文件
        /// </summary>
        /// <param name="file">文件信息</param>
        /// <returns>是否为可执行文件</returns>
        public static bool IsExecutable(FileInfo file)
        {
            var type = GetType(file);
            if (type == null) return false;

            var execTypes = new[] { ".exe", ".dll", ".sys", ".com", ".bat", ".cmd", ".ps1", ".sh" };
            return Array.IndexOf(execTypes, type.ToLowerInvariant()) >= 0;
        }

        /// <summary>
        /// 获取文件的MIME类型
        /// </summary>
        /// <param name="file">文件信息</param>
        /// <returns>MIME类型</returns>
        public static string GetMimeType(FileInfo file)
        {
            var type = GetType(file)?.ToLowerInvariant();

            return type switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".webp" => "image/webp",
                ".svg" => "image/svg+xml",
                ".ico" => "image/x-icon",
                ".mp4" => "video/mp4",
                ".avi" => "video/x-msvideo",
                ".mov" => "video/quicktime",
                ".wmv" => "video/x-ms-wmv",
                ".flv" => "video/x-flv",
                ".mkv" => "video/x-matroska",
                ".webm" => "video/webm",
                ".mp3" => "audio/mpeg",
                ".wav" => "audio/wav",
                ".flac" => "audio/flac",
                ".aac" => "audio/aac",
                ".ogg" => "audio/ogg",
                ".m4a" => "audio/mp4",
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".ppt" => "application/vnd.ms-powerpoint",
                ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                ".zip" => "application/zip",
                ".rar" => "application/vnd.rar",
                ".7z" => "application/x-7z-compressed",
                ".tar" => "application/x-tar",
                ".gz" => "application/gzip",
                ".txt" => "text/plain",
                ".html" or ".htm" => "text/html",
                ".css" => "text/css",
                ".js" => "application/javascript",
                ".json" => "application/json",
                ".xml" => "application/xml",
                ".exe" or ".dll" => "application/octet-stream",
                _ => "application/octet-stream"
            };
        }
    }
}