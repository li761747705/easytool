using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EasyTool.IOCategory
{
    /// <summary>
    /// 文件搜索工具类
    /// </summary>
    public static class FileSearch
    {
        /// <summary>
        /// 搜索文件
        /// </summary>
        /// <param name="directory">搜索目录</param>
        /// <param name="pattern">搜索模式</param>
        /// <param name="searchSubdirectories">是否搜索子目录</param>
        /// <returns>文件路径列表</returns>
        public static List<string> SearchFiles(string directory, string pattern = "*", bool searchSubdirectories = true)
        {
            var option = searchSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            try
            {
                return Directory.GetFiles(directory, pattern, option).ToList();
            }
            catch (UnauthorizedAccessException)
            {
                return new List<string>();
            }
        }

        /// <summary>
        /// 搜索文件（多个模式）
        /// </summary>
        public static List<string> SearchFiles(string directory, string[] patterns, bool searchSubdirectories = true)
        {
            var results = new List<string>();

            foreach (var pattern in patterns)
            {
                results.AddRange(SearchFiles(directory, pattern, searchSubdirectories));
            }

            return results.Distinct().ToList();
        }

        /// <summary>
        /// 搜索目录
        /// </summary>
        public static List<string> SearchDirectories(string directory, string pattern = "*", bool searchSubdirectories = true)
        {
            var option = searchSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            try
            {
                return Directory.GetDirectories(directory, pattern, option).ToList();
            }
            catch (UnauthorizedAccessException)
            {
                return new List<string>();
            }
        }

        /// <summary>
        /// 按大小搜索文件
        /// </summary>
        public static List<string> SearchBySize(string directory, long minSize = 0, long maxSize = long.MaxValue, bool searchSubdirectories = true)
        {
            var files = SearchFiles(directory, "*", searchSubdirectories);
            var results = new List<string>();

            foreach (var file in files)
            {
                try
                {
                    var info = new FileInfo(file);
                    if (info.Length >= minSize && info.Length <= maxSize)
                    {
                        results.Add(file);
                    }
                }
                catch
                {
                    // 忽略无法访问的文件
                }
            }

            return results;
        }

        /// <summary>
        /// 按修改时间搜索文件
        /// </summary>
        public static List<string> SearchByDate(string directory, DateTime? startTime = null, DateTime? endTime = null, bool searchSubdirectories = true)
        {
            var files = SearchFiles(directory, "*", searchSubdirectories);
            var results = new List<string>();

            foreach (var file in files)
            {
                try
                {
                    var info = new FileInfo(file);
                    var writeTime = info.LastWriteTime;

                    var afterStart = !startTime.HasValue || writeTime >= startTime.Value;
                    var beforeEnd = !endTime.HasValue || writeTime <= endTime.Value;

                    if (afterStart && beforeEnd)
                    {
                        results.Add(file);
                    }
                }
                catch
                {
                    // 忽略无法访问的文件
                }
            }

            return results;
        }

        /// <summary>
        /// 按内容搜索文件
        /// </summary>
        public static async Task<List<string>> SearchByContent(string directory, string searchText, bool searchSubdirectories = true, bool ignoreCase = true)
        {
            var files = SearchFiles(directory, "*.txt", searchSubdirectories);
            files.AddRange(SearchFiles(directory, "*.log", searchSubdirectories));
            files.AddRange(SearchFiles(directory, "*.json", searchSubdirectories));
            files.AddRange(SearchFiles(directory, "*.xml", searchSubdirectories));
            files.AddRange(SearchFiles(directory, "*.cs", searchSubdirectories));

            var results = new List<string>();
            var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            foreach (var file in files.Distinct())
            {
                try
                {
                    var content = await File.ReadAllTextAsync(file);
                    if (content.Contains(searchText, comparison))
                    {
                        results.Add(file);
                    }
                }
                catch
                {
                    // 忽略无法读取的文件
                }
            }

            return results;
        }

        /// <summary>
        /// 查找重复文件
        /// </summary>
        public static List<List<string>> FindDuplicates(string directory, bool searchSubdirectories = true)
        {
            var files = SearchFiles(directory, "*", searchSubdirectories);
            var sizeGroups = files.GroupBy(f =>
            {
                try
                {
                    return new FileInfo(f).Length;
                }
                catch
                {
                    return -1L;
                }
            }).Where(g => g.Key > 0 && g.Count() > 1);

            var duplicates = new List<List<string>>();

            foreach (var group in sizeGroups)
            {
                var sameSizeFiles = group.ToList();
                var hashGroups = sameSizeFiles.GroupBy(f =>
                {
                    try
                    {
                        using var stream = File.OpenRead(f);
                        using var md5 = System.Security.Cryptography.MD5.Create();
                        var hash = md5.ComputeHash(stream);
                        return Convert.ToBase64String(hash);
                    }
                    catch
                    {
                        return string.Empty;
                    }
                }).Where(g => !string.IsNullOrEmpty(g.Key) && g.Count() > 1);

                foreach (var hashGroup in hashGroups)
                {
                    duplicates.Add(hashGroup.ToList());
                }
            }

            return duplicates;
        }

        /// <summary>
        /// 查找空目录
        /// </summary>
        public static List<string> FindEmptyDirectories(string directory)
        {
            var emptyDirs = new List<string>();

            try
            {
                var subDirs = Directory.GetDirectories(directory, "*", SearchOption.AllDirectories);

                foreach (var dir in subDirs)
                {
                    try
                    {
                        if (!Directory.EnumerateFileSystemEntries(dir).Any())
                        {
                            emptyDirs.Add(dir);
                        }
                    }
                    catch
                    {
                        // 忽略无法访问的目录
                    }
                }
            }
            catch
            {
                // 忽略无法访问的目录
            }

            return emptyDirs;
        }

        /// <summary>
        /// 获取目录大小
        /// </summary>
        public static long GetDirectorySize(string directory)
        {
            var files = SearchFiles(directory, "*", true);
            long size = 0;

            foreach (var file in files)
            {
                try
                {
                    size += new FileInfo(file).Length;
                }
                catch
                {
                    // 忽略无法访问的文件
                }
            }

            return size;
        }

        /// <summary>
        /// 获取文件统计信息
        /// </summary>
        public static Dictionary<string, int> GetFileStatistics(string directory)
        {
            var files = SearchFiles(directory, "*", true);
            var stats = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (var file in files)
            {
                var ext = Path.GetExtension(file);
                if (string.IsNullOrEmpty(ext))
                    ext = "(无扩展名)";

                if (stats.ContainsKey(ext))
                    stats[ext]++;
                else
                    stats[ext] = 1;
            }

            return stats;
        }
    }
}
