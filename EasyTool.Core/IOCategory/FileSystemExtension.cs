using System;
using System.IO;
using System.Linq;

namespace EasyTool.Extension
{
    /// <summary>
    /// 文件系统扩展方法
    /// </summary>
    public static class FileSystemExtension
    {
        #region FileInfo 扩展

        /// <summary>
        /// 获取文件大小（格式化字符串）
        /// </summary>
        public static string GetSizeFormatted(this FileInfo file)
        {
            if (file == null || !file.Exists)
                return "0 B";

            return file.Length.ToFileSize();
        }

        /// <summary>
        /// 获取相对路径
        /// </summary>
        /// <param name="file">源文件</param>
        /// <param name="relativeTo">参考路径</param>
        public static string GetRelativePath(this FileInfo file, string relativeTo)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            return GetRelativePath(file.FullName, relativeTo);
        }

        /// <summary>
        /// 获取文件的 MIME 类型
        /// </summary>
        public static string GetMimeType(this FileInfo file)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            return file.Extension.GetMimeType();
        }

        /// <summary>
        /// 判断文件是否为图片
        /// </summary>
        public static bool IsImage(this FileInfo file)
        {
            if (file == null)
                return false;

            string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".ico", ".svg" };
            return imageExtensions.Contains(file.Extension.ToLowerInvariant());
        }

        /// <summary>
        /// 判断文件是否为文档
        /// </summary>
        public static bool IsDocument(this FileInfo file)
        {
            if (file == null)
                return false;

            string[] docExtensions = { ".doc", ".docx", ".pdf", ".txt", ".rtf", ".odt", ".xls", ".xlsx", ".ppt", ".pptx" };
            return docExtensions.Contains(file.Extension.ToLowerInvariant());
        }

        /// <summary>
        /// 判断文件是否为视频
        /// </summary>
        public static bool IsVideo(this FileInfo file)
        {
            if (file == null)
                return false;

            string[] videoExtensions = { ".mp4", ".avi", ".mov", ".wmv", ".flv", ".mkv", ".webm", ".m4v" };
            return videoExtensions.Contains(file.Extension.ToLowerInvariant());
        }

        /// <summary>
        /// 判断文件是否为音频
        /// </summary>
        public static bool IsAudio(this FileInfo file)
        {
            if (file == null)
                return false;

            string[] audioExtensions = { ".mp3", ".wav", ".flac", ".aac", ".ogg", ".wma", ".m4a" };
            return audioExtensions.Contains(file.Extension.ToLowerInvariant());
        }

        /// <summary>
        /// 判断文件是否被锁定（正在使用）
        /// </summary>
        public static bool IsLocked(this FileInfo file)
        {
            if (file == null || !file.Exists)
                return false;

            try
            {
                using (var stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    return false;
                }
            }
            catch (IOException)
            {
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return true;
            }
        }

        /// <summary>
        /// 安全删除文件（如果存在）
        /// </summary>
        public static bool DeleteIfExists(this FileInfo file)
        {
            if (file == null || !file.Exists)
                return false;

            try
            {
                file.Delete();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 移动文件到指定目录
        /// </summary>
        public static FileInfo MoveToDirectory(this FileInfo file, string targetDirectory)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            if (!Directory.Exists(targetDirectory))
                Directory.CreateDirectory(targetDirectory);

            string targetPath = Path.Combine(targetDirectory, file.Name);
            file.MoveTo(targetPath);
            return new FileInfo(targetPath);
        }

        /// <summary>
        /// 复制文件到指定目录
        /// </summary>
        public static FileInfo CopyToDirectory(this FileInfo file, string targetDirectory, bool overwrite = false)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            if (!Directory.Exists(targetDirectory))
                Directory.CreateDirectory(targetDirectory);

            string targetPath = Path.Combine(targetDirectory, file.Name);
            file.CopyTo(targetPath, overwrite);
            return new FileInfo(targetPath);
        }


        #endregion

        #region DirectoryInfo 扩展

        /// <summary>
        /// 获取目录的总大小（包含所有子目录）
        /// </summary>
        public static long GetTotalSize(this DirectoryInfo directory)
        {
            if (directory == null || !directory.Exists)
                return 0;

            long size = 0;

            try
            {
                size += directory.GetFiles().Sum(f => f.Length);
                size += directory.GetDirectories().Sum(d => GetTotalSize(d));
            }
            catch (UnauthorizedAccessException)
            {
                // 忽略无权限访问的目录
            }

            return size;
        }

        /// <summary>
        /// 获取目录的总大小（格式化字符串）
        /// </summary>
        public static string GetTotalSizeFormatted(this DirectoryInfo directory)
        {
            return directory.GetTotalSize().ToFileSize();
        }

        /// <summary>
        /// 获取目录中的所有文件（包含子目录）
        /// </summary>
        public static FileInfo[] GetAllFiles(this DirectoryInfo directory, string searchPattern = "*.*")
        {
            if (directory == null || !directory.Exists)
                return Array.Empty<FileInfo>();

            try
            {
                var files = directory.GetFiles(searchPattern, SearchOption.AllDirectories);
                return files;
            }
            catch (UnauthorizedAccessException)
            {
                return Array.Empty<FileInfo>();
            }
        }

        /// <summary>
        /// 清空目录（删除所有文件和子目录）
        /// </summary>
        public static void Clear(this DirectoryInfo directory)
        {
            if (directory == null || !directory.Exists)
                return;

            foreach (var file in directory.GetFiles())
            {
                file.Delete();
            }

            foreach (var subDir in directory.GetDirectories())
            {
                subDir.Delete(true);
            }
        }

        /// <summary>
        /// 安全删除目录（如果存在）
        /// </summary>
        public static bool DeleteIfExists(this DirectoryInfo directory, bool recursive = false)
        {
            if (directory == null || !directory.Exists)
                return false;

            try
            {
                directory.Delete(recursive);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 确保目录存在，不存在则创建
        /// </summary>
        public static DirectoryInfo EnsureExists(this DirectoryInfo directory)
        {
            if (directory == null)
                throw new ArgumentNullException(nameof(directory));

            if (!directory.Exists)
                directory.Create();

            return directory;
        }

        /// <summary>
        /// 复制目录到指定位置
        /// </summary>
        public static DirectoryInfo CopyTo(this DirectoryInfo sourceDir, string targetPath)
        {
            if (sourceDir == null)
                throw new ArgumentNullException(nameof(sourceDir));

            var targetDir = Directory.CreateDirectory(targetPath);

            // 复制文件
            foreach (var file in sourceDir.GetFiles())
            {
                string targetFilePath = Path.Combine(targetPath, file.Name);
                file.CopyTo(targetFilePath, true);
            }

            // 递归复制子目录
            foreach (var subDir in sourceDir.GetDirectories())
            {
                string targetSubDirPath = Path.Combine(targetPath, subDir.Name);
                subDir.CopyTo(targetSubDirPath);
            }

            return targetDir;
        }

        #endregion

        #region 路径扩展

        /// <summary>
        /// 获取相对路径
        /// </summary>
        /// <param name="absolutePath">绝对路径</param>
        /// <param name="relativeTo">参考路径</param>
        public static string GetRelativePath(string absolutePath, string relativeTo)
        {
            if (string.IsNullOrEmpty(absolutePath))
                throw new ArgumentNullException(nameof(absolutePath));

            if (string.IsNullOrEmpty(relativeTo))
                throw new ArgumentNullException(nameof(relativeTo));

            absolutePath = Path.GetFullPath(absolutePath);
            relativeTo = Path.GetFullPath(relativeTo);

            // 从 .NET Core 2.0 / .NET Standard 2.1 开始，可以使用 Path.GetRelativePath
            // 这里提供一个兼容的实现
            var absolutePathParts = absolutePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var relativeToParts = relativeTo.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            int length = Math.Min(absolutePathParts.Length, relativeToParts.Length);
            int lastCommonRoot = -1;

            for (int i = 0; i < length; i++)
            {
                if (string.Equals(absolutePathParts[i], relativeToParts[i], StringComparison.OrdinalIgnoreCase))
                {
                    lastCommonRoot = i;
                }
                else
                {
                    break;
                }
            }

            if (lastCommonRoot == -1)
                return absolutePath;

            var relativePath = new System.Text.StringBuilder();

            // 添加 ..
            for (int i = lastCommonRoot + 1; i < relativeToParts.Length; i++)
            {
                if (relativePath.Length > 0)
                    relativePath.Append(Path.DirectorySeparatorChar);

                relativePath.Append("..");
            }

            // 添加目标路径的剩余部分
            for (int i = lastCommonRoot + 1; i < absolutePathParts.Length; i++)
            {
                if (relativePath.Length > 0)
                    relativePath.Append(Path.DirectorySeparatorChar);

                relativePath.Append(absolutePathParts[i]);
            }

            return relativePath.ToString();
        }

        /// <summary>
        /// 确保路径以目录分隔符结尾
        /// </summary>
        public static string EnsureEndsWithSeparator(this string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;

            char lastChar = path[path.Length - 1];
            if (lastChar != Path.DirectorySeparatorChar && lastChar != Path.AltDirectorySeparatorChar)
            {
                return path + Path.DirectorySeparatorChar;
            }

            return path;
        }

        #endregion

        #region 文件扩展名扩展

        /// <summary>
        /// 获取文件扩展名对应的 MIME 类型
        /// </summary>
        public static string GetMimeType(this string extension)
        {
            if (string.IsNullOrEmpty(extension))
                return "application/octet-stream";

            // 确保扩展名以 . 开头
            if (!extension.StartsWith("."))
                extension = "." + extension;

            return extension.ToLowerInvariant() switch
            {
                // 文本
                ".html" => "text/html",
                ".htm" => "text/html",
                ".css" => "text/css",
                ".js" => "application/javascript",
                ".json" => "application/json",
                ".xml" => "application/xml",
                ".txt" => "text/plain",

                // 图片
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".webp" => "image/webp",
                ".ico" => "image/x-icon",
                ".svg" => "image/svg+xml",

                // 视频
                ".mp4" => "video/mp4",
                ".avi" => "video/x-msvideo",
                ".mov" => "video/quicktime",
                ".wmv" => "video/x-ms-wmv",
                ".flv" => "video/x-flv",
                ".mkv" => "video/x-matroska",
                ".webm" => "video/webm",

                // 音频
                ".mp3" => "audio/mpeg",
                ".wav" => "audio/wav",
                ".flac" => "audio/flac",
                ".aac" => "audio/aac",
                ".ogg" => "audio/ogg",
                ".wma" => "audio/x-ms-wma",
                ".m4a" => "audio/mp4",

                // 文档
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".ppt" => "application/vnd.ms-powerpoint",
                ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                ".zip" => "application/zip",
                ".rar" => "application/x-rar-compressed",
                ".7z" => "application/x-7z-compressed",

                _ => "application/octet-stream"
            };
        }

        #endregion
    }
}
