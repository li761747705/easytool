using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EasyTool.IOCategory
{
    /// <summary>
    /// 路径工具类
    /// 提供路径操作的增强功能
    /// </summary>
    public static class PathUtil
    {
        /// <summary>
        /// 获取相对路径
        /// </summary>
        public static string GetRelativePath(string basePath, string targetPath)
        {
            if (string.IsNullOrEmpty(basePath))
                throw new ArgumentException("Base path cannot be null or empty", nameof(basePath));
            if (string.IsNullOrEmpty(targetPath))
                throw new ArgumentException("Target path cannot be null or empty", nameof(targetPath));

            // 规范化路径
            basePath = Normalize(basePath);
            targetPath = Normalize(targetPath);

            // 确保基础路径以分隔符结尾
            if (!basePath.EndsWith(Path.DirectorySeparatorChar.ToString()) &&
                !basePath.EndsWith(Path.AltDirectorySeparatorChar.ToString()))
            {
                basePath += Path.DirectorySeparatorChar;
            }

            var baseParts = basePath.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
            var targetParts = targetPath.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);

            // 处理 Windows 盘符
            if (baseParts.Length > 0 && targetParts.Length > 0)
            {
                string baseRoot = GetRoot(basePath);
                string targetRoot = GetRoot(targetPath);
                if (!string.IsNullOrEmpty(baseRoot) && !string.IsNullOrEmpty(targetRoot) &&
                    !baseRoot.Equals(targetRoot, StringComparison.OrdinalIgnoreCase))
                {
                    return targetPath; // 不同盘符，返回绝对路径
                }
            }

            // 找到公共前缀
            int commonLength = 0;
            int minLen = Math.Min(baseParts.Length, targetParts.Length);
            while (commonLength < minLen &&
                   baseParts[commonLength].Equals(targetParts[commonLength], StringComparison.OrdinalIgnoreCase))
            {
                commonLength++;
            }

            // 构建相对路径
            var result = new StringBuilder();

            // 添加向上回溯
            for (int i = commonLength; i < baseParts.Length - (basePath.EndsWith(Path.DirectorySeparatorChar.ToString()) ? 0 : 1); i++)
            {
                if (result.Length > 0)
                    result.Append(Path.DirectorySeparatorChar);
                result.Append("..");
            }

            // 添加目标路径的剩余部分
            for (int i = commonLength; i < targetParts.Length; i++)
            {
                if (result.Length > 0)
                    result.Append(Path.DirectorySeparatorChar);
                result.Append(targetParts[i]);
            }

            return result.Length == 0 ? "." : result.ToString();
        }

        private static string GetRoot(string path)
        {
            if (path.Length >= 2 && path[1] == ':')
                return path.Substring(0, 2).ToUpperInvariant();
            if (path.StartsWith("/") || path.StartsWith("\\"))
                return path[0].ToString();
            return "";
        }

        /// <summary>
        /// 规范化路径（统一分隔符，移除多余的点和分隔符）
        /// </summary>
        public static string Normalize(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;

            // 替换分隔符
            path = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

            // 处理 ./ 和 ../
            var parts = path.Split(new[] { Path.DirectorySeparatorChar }, StringSplitOptions.None);
            var result = new List<string>();

            foreach (var part in parts)
            {
                if (part == ".")
                    continue;
                else if (part == "..")
                {
                    if (result.Count > 0 && result[result.Count - 1] != "..")
                        result.RemoveAt(result.Count - 1);
                    else if (!IsAbsolute(path))
                        result.Add("..");
                }
                else
                {
                    result.Add(part);
                }
            }

            string normalized = string.Join(Path.DirectorySeparatorChar.ToString(), result);

            // 处理根路径
            if (path.StartsWith(Path.DirectorySeparatorChar.ToString()) && !normalized.StartsWith(Path.DirectorySeparatorChar.ToString()))
            {
                if (path.Length >= 2 && path[1] == ':')
                    normalized = path.Substring(0, 2) + Path.DirectorySeparatorChar + normalized;
                else
                    normalized = Path.DirectorySeparatorChar + normalized;
            }

            return normalized;
        }

        /// <summary>
        /// 判断是否为绝对路径
        /// </summary>
        public static bool IsAbsolute(string path)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            // Windows: C:\ 或 \
            // Unix: /
            return Path.IsPathRooted(path) ||
                   (path.Length >= 2 && path[1] == ':') ||
                   path.StartsWith("/") ||
                   path.StartsWith("\\\\");
        }

        /// <summary>
        /// 获取文件扩展名（不带点）
        /// </summary>
        public static string GetExtensionWithoutDot(string path)
        {
            string ext = Path.GetExtension(path);
            return string.IsNullOrEmpty(ext) ? "" : ext.Substring(1);
        }

        /// <summary>
        /// 更改文件扩展名
        /// </summary>
        public static string ChangeExtension(string path, string newExtension)
        {
            if (string.IsNullOrEmpty(path))
                return path;

            newExtension = newExtension?.StartsWith(".") == true ? newExtension : "." + newExtension;
            return Path.ChangeExtension(path, newExtension);
        }

        /// <summary>
        /// 获取文件名（不带扩展名）
        /// </summary>
        public static string GetFileNameWithoutExtension(string path)
        {
            return Path.GetFileNameWithoutExtension(path);
        }

        /// <summary>
        /// 获取父目录路径
        /// </summary>
        public static string GetParent(string path)
        {
            return Path.GetDirectoryName(path);
        }

        /// <summary>
        /// 获取所有父目录路径
        /// </summary>
        public static List<string> GetParents(string path)
        {
            var parents = new List<string>();
            string current = path;

            while (!string.IsNullOrEmpty(current))
            {
                string parent = Path.GetDirectoryName(current);
                if (string.IsNullOrEmpty(parent))
                    break;
                parents.Add(parent);
                current = parent;
            }

            return parents;
        }

        /// <summary>
        /// 连接路径片段
        /// </summary>
        public static string Combine(params string[] paths)
        {
            return Path.Combine(paths);
        }

        /// <summary>
        /// 获取临时文件路径
        /// </summary>
        public static string GetTempFilePath(string extension = null)
        {
            string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            if (!string.IsNullOrEmpty(extension))
            {
                extension = extension.StartsWith(".") ? extension : "." + extension;
                path += extension;
            }
            return path;
        }

        /// <summary>
        /// 获取临时目录路径
        /// </summary>
        public static string GetTempDirectoryPath()
        {
            return Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        }

        /// <summary>
        /// 确保目录存在
        /// </summary>
        public static string EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }

        /// <summary>
        /// 确保文件所在目录存在
        /// </summary>
        public static string EnsureParentDirectoryExists(string filePath)
        {
            string dir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            return filePath;
        }

        /// <summary>
        /// 获取唯一文件名（如果文件存在则添加序号）
        /// </summary>
        public static string GetUniqueFilePath(string basePath)
        {
            if (!File.Exists(basePath))
                return basePath;

            string dir = Path.GetDirectoryName(basePath);
            string name = Path.GetFileNameWithoutExtension(basePath);
            string ext = Path.GetExtension(basePath);

            int count = 1;
            string newPath;
            do
            {
                newPath = Path.Combine(dir ?? "", $"{name} ({count}){ext}");
                count++;
            }
            while (File.Exists(newPath));

            return newPath;
        }

        /// <summary>
        /// 获取唯一目录名
        /// </summary>
        public static string GetUniqueDirectoryPath(string basePath)
        {
            if (!Directory.Exists(basePath))
                return basePath;

            int count = 1;
            string newPath;
            do
            {
                newPath = $"{basePath} ({count})";
                count++;
            }
            while (Directory.Exists(newPath));

            return newPath;
        }

        /// <summary>
        /// 获取路径深度
        /// </summary>
        public static int GetDepth(string path)
        {
            if (string.IsNullOrEmpty(path))
                return 0;

            path = Normalize(path);
            return path.Split(new[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries).Length;
        }

        /// <summary>
        /// 路径是否在指定目录下
        /// </summary>
        public static bool IsInDirectory(string path, string directory)
        {
            string normalizedPath = Normalize(Path.GetFullPath(path));
            string normalizedDir = Normalize(Path.GetFullPath(directory));

            return normalizedPath.StartsWith(normalizedDir, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 获取路径层级（相对于基础路径）
        /// </summary>
        public static string GetPathLevel(string basePath, string targetPath, int level)
        {
            string relative = GetRelativePath(basePath, targetPath);
            var parts = relative.Split(new[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);

            if (level < 0 || level >= parts.Length)
                return null;

            return parts[level];
        }
    }
}
