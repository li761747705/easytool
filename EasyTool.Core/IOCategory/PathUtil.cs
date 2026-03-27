using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EasyTool.IOCategory
{
    /// <summary>
    /// 路径工具类
    /// </summary>
    public static class PathUtil
    {
        /// <summary>
        /// 合并路径
        /// </summary>
        public static string Combine(params string[] paths)
        {
            return Path.Combine(paths);
        }

        /// <summary>
        /// 获取绝对路径
        /// </summary>
        public static string GetFullPath(string path, string? basePath = null)
        {
            if (string.IsNullOrEmpty(path))
                return path;

            if (Path.IsPathRooted(path))
                return Path.GetFullPath(path);

            basePath ??= Directory.GetCurrentDirectory();
            return Path.GetFullPath(Path.Combine(basePath, path));
        }

        /// <summary>
        /// 获取相对路径
        /// </summary>
        public static string GetRelativePath(string relativeTo, string path)
        {
            return Path.GetRelativePath(relativeTo, path);
        }

        /// <summary>
        /// 获取文件名（包含扩展名）
        /// </summary>
        public static string GetFileName(string path)
        {
            return Path.GetFileName(path);
        }

        /// <summary>
        /// 获取文件名（不含扩展名）
        /// </summary>
        public static string GetFileNameWithoutExtension(string path)
        {
            return Path.GetFileNameWithoutExtension(path);
        }

        /// <summary>
        /// 获取扩展名
        /// </summary>
        public static string GetExtension(string path)
        {
            return Path.GetExtension(path);
        }

        /// <summary>
        /// 获取目录路径
        /// </summary>
        public static string? GetDirectoryName(string path)
        {
            return Path.GetDirectoryName(path);
        }

        /// <summary>
        /// 更改扩展名
        /// </summary>
        public static string ChangeExtension(string path, string extension)
        {
            return Path.ChangeExtension(path, extension);
        }

        /// <summary>
        /// 移除扩展名
        /// </summary>
        public static string RemoveExtension(string path)
        {
            return Path.ChangeExtension(path, null) ?? path;
        }

        /// <summary>
        /// 检查是否是绝对路径
        /// </summary>
        public static bool IsAbsolute(string path)
        {
            return Path.IsPathRooted(path);
        }

        /// <summary>
        /// 检查是否是相对路径
        /// </summary>
        public static bool IsRelative(string path)
        {
            return !Path.IsPathRooted(path);
        }

        /// <summary>
        /// 规范化路径（统一分隔符）
        /// </summary>
        public static string Normalize(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;

            return path.Replace('/', Path.DirectorySeparatorChar)
                      .Replace('\\', Path.DirectorySeparatorChar)
                      .TrimEnd(Path.DirectorySeparatorChar);
        }

        /// <summary>
        /// 确保以分隔符结尾
        /// </summary>
        public static string EnsureTrailingSeparator(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;

            if (!path.EndsWith(Path.DirectorySeparatorChar.ToString()))
                return path + Path.DirectorySeparatorChar;

            return path;
        }

        /// <summary>
        /// 移除尾部分隔符
        /// </summary>
        public static string TrimTrailingSeparator(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;

            return path.TrimEnd(Path.DirectorySeparatorChar, '/');
        }

        /// <summary>
        /// 获取父目录
        /// </summary>
        public static string? GetParent(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            var dir = Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(dir))
                return null;

            return dir;
        }

        /// <summary>
        /// 获取所有父目录
        /// </summary>
        public static IEnumerable<string> GetParents(string path)
        {
            var current = path;
            while (!string.IsNullOrEmpty(current))
            {
                var parent = Path.GetDirectoryName(current);
                if (string.IsNullOrEmpty(parent))
                    yield break;

                yield return parent;
                current = parent;
            }
        }

        /// <summary>
        /// 获取目录深度
        /// </summary>
        public static int GetDepth(string path)
        {
            if (string.IsNullOrEmpty(path))
                return 0;

            path = Normalize(path);
            return path.Split(Path.DirectorySeparatorChar).Length - 1;
        }

        /// <summary>
        /// 检查路径是否在指定目录下
        /// </summary>
        public static bool IsInDirectory(string path, string directory)
        {
            if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(directory))
                return false;

            var fullPath = GetFullPath(path);
            var fullDirectory = GetFullPath(directory);

            return fullPath.StartsWith(EnsureTrailingSeparator(fullDirectory), StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 获取唯一文件名（避免冲突）
        /// </summary>
        public static string GetUniqueFileName(string directory, string fileName)
        {
            var fullPath = Path.Combine(directory, fileName);

            if (!File.Exists(fullPath))
                return fileName;

            var name = Path.GetFileNameWithoutExtension(fileName);
            var ext = Path.GetExtension(fileName);
            var counter = 1;

            while (true)
            {
                var newName = $"{name} ({counter}){ext}";
                fullPath = Path.Combine(directory, newName);

                if (!File.Exists(fullPath))
                    return newName;

                counter++;
            }
        }

        /// <summary>
        /// 获取临时文件路径
        /// </summary>
        public static string GetTempFilePath(string? extension = null)
        {
            var path = Path.GetTempFileName();

            if (!string.IsNullOrEmpty(extension))
            {
                var newPath = Path.ChangeExtension(path, extension);
                File.Move(path, newPath);
                return newPath;
            }

            return path;
        }

        /// <summary>
        /// 获取临时目录路径
        /// </summary>
        public static string GetTempDirectoryPath()
        {
            var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            return path;
        }

        /// <summary>
        /// 分割路径为各部分
        /// </summary>
        public static string[] Split(string path)
        {
            if (string.IsNullOrEmpty(path))
                return Array.Empty<string>();

            path = Normalize(path);

            // 处理根目录
            var root = Path.GetPathRoot(path);
            if (!string.IsNullOrEmpty(root))
            {
                path = path.Substring(root.Length);
                var parts = path.Split(new[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
                var result = new string[parts.Length + 1];
                result[0] = root.TrimEnd(Path.DirectorySeparatorChar);
                Array.Copy(parts, 0, result, 1, parts.Length);
                return result;
            }

            return path.Split(new[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// 构建路径
        /// </summary>
        public static string Build(params string[] parts)
        {
            return Path.Combine(parts.Where(p => !string.IsNullOrEmpty(p)).ToArray());
        }

        /// <summary>
        /// 验证路径是否有效
        /// </summary>
        public static bool IsValid(string path)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            var invalidChars = Path.GetInvalidPathChars();
            return path.IndexOfAny(invalidChars) < 0;
        }

        /// <summary>
        /// 验证文件名是否有效
        /// </summary>
        public static bool IsValidFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return false;

            var invalidChars = Path.GetInvalidFileNameChars();
            return fileName.IndexOfAny(invalidChars) < 0;
        }

        /// <summary>
        /// 清理文件名（移除无效字符）
        /// </summary>
        public static string SanitizeFileName(string fileName, char replacement = '_')
        {
            if (string.IsNullOrEmpty(fileName))
                return fileName;

            var invalidChars = Path.GetInvalidFileNameChars();
            var result = new StringBuilder(fileName);

            foreach (var c in invalidChars)
            {
                result.Replace(c, replacement);
            }

            return result.ToString();
        }

        /// <summary>
        /// 获取路径大小（文件或目录）
        /// </summary>
        public static long GetSize(string path)
        {
            if (File.Exists(path))
            {
                return new FileInfo(path).Length;
            }

            if (Directory.Exists(path))
            {
                var dirInfo = new DirectoryInfo(path);
                return dirInfo.EnumerateFiles("*", SearchOption.AllDirectories).Sum(f => f.Length);
            }

            return 0;
        }
    }
}
