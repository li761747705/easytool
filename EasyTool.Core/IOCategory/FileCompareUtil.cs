using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace EasyTool.IOCategory
{
    /// <summary>
    /// 文件比较工具类
    /// </summary>
    public static class FileCompareUtil
    {
        /// <summary>
        /// 比较两个文件内容是否相同
        /// </summary>
        public static bool AreContentsEqual(string filePath1, string filePath2)
        {
            if (!File.Exists(filePath1) || !File.Exists(filePath2))
                return false;

            var fileInfo1 = new FileInfo(filePath1);
            var fileInfo2 = new FileInfo(filePath2);

            // 大小不同，内容肯定不同
            if (fileInfo1.Length != fileInfo2.Length)
                return false;

            // 逐字节比较
            using var stream1 = File.OpenRead(filePath1);
            using var stream2 = File.OpenRead(filePath2);

            var buffer1 = new byte[4096];
            var buffer2 = new byte[4096];

            while (true)
            {
                var count1 = stream1.Read(buffer1, 0, buffer1.Length);
                var count2 = stream2.Read(buffer2, 0, buffer2.Length);

                if (count1 != count2)
                    return false;

                if (count1 == 0)
                    return true;

                for (int i = 0; i < count1; i++)
                {
                    if (buffer1[i] != buffer2[i])
                        return false;
                }
            }
        }

        /// <summary>
        /// 比较两个文件内容是否相同（使用哈希）
        /// </summary>
        public static bool AreContentsEqualByHash(string filePath1, string filePath2)
        {
            if (!File.Exists(filePath1) || !File.Exists(filePath2))
                return false;

            var hash1 = ComputeFileHash(filePath1);
            var hash2 = ComputeFileHash(filePath2);

            return hash1 == hash2;
        }

        /// <summary>
        /// 计算文件哈希值
        /// </summary>
        public static string ComputeFileHash(string filePath, string algorithm = "MD5")
        {
            using var stream = File.OpenRead(filePath);
            using HashAlgorithm hasher = algorithm.ToUpper() switch
            {
                "MD5" => MD5.Create(),
                "SHA1" => SHA1.Create(),
                "SHA256" => SHA256.Create(),
                "SHA512" => SHA512.Create(),
                _ => MD5.Create()
            };

            var hash = hasher.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

        /// <summary>
        /// 比较两个目录
        /// </summary>
        public static DirectoryCompareResult CompareDirectories(string directory1, string directory2, string searchPattern = "*")
        {
            var result = new DirectoryCompareResult();

            var files1 = Directory.GetFiles(directory1, searchPattern, SearchOption.AllDirectories);
            var files2 = Directory.GetFiles(directory2, searchPattern, SearchOption.AllDirectories);

            var relativePath1 = new Dictionary<string, string>();
            var relativePath2 = new Dictionary<string, string>();

            foreach (var file in files1)
            {
                var relative = file.Substring(directory1.Length).TrimStart(Path.DirectorySeparatorChar);
                relativePath1[relative] = file;
            }

            foreach (var file in files2)
            {
                var relative = file.Substring(directory2.Length).TrimStart(Path.DirectorySeparatorChar);
                relativePath2[relative] = file;
            }

            // 只在目录1中的文件
            foreach (var kvp in relativePath1)
            {
                if (!relativePath2.ContainsKey(kvp.Key))
                {
                    result.OnlyInDirectory1.Add(kvp.Value);
                }
            }

            // 只在目录2中的文件
            foreach (var kvp in relativePath2)
            {
                if (!relativePath1.ContainsKey(kvp.Key))
                {
                    result.OnlyInDirectory2.Add(kvp.Value);
                }
            }

            // 两边都有的文件
            foreach (var kvp in relativePath1)
            {
                if (relativePath2.TryGetValue(kvp.Key, out var file2))
                {
                    if (AreContentsEqual(kvp.Value, file2))
                    {
                        result.IdenticalFiles.Add(kvp.Value);
                    }
                    else
                    {
                        result.DifferentFiles.Add(new FileDifference
                        {
                            File1 = kvp.Value,
                            File2 = file2
                        });
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 查找重复文件
        /// </summary>
        public static List<List<string>> FindDuplicateFiles(string directory, string searchPattern = "*")
        {
            var files = Directory.GetFiles(directory, searchPattern, SearchOption.AllDirectories);
            var hashGroups = new Dictionary<string, List<string>>();

            foreach (var file in files)
            {
                try
                {
                    var hash = ComputeFileHash(file);
                    if (!hashGroups.ContainsKey(hash))
                        hashGroups[hash] = new List<string>();
                    hashGroups[hash].Add(file);
                }
                catch
                {
                    // 忽略无法读取的文件
                }
            }

            return hashGroups.Values.Where(g => g.Count > 1).ToList();
        }

        /// <summary>
        /// 查找相似文件（大小相同）
        /// </summary>
        public static List<List<string>> FindSimilarSizedFiles(string directory, string searchPattern = "*")
        {
            var files = Directory.GetFiles(directory, searchPattern, SearchOption.AllDirectories);
            var sizeGroups = new Dictionary<long, List<string>>();

            foreach (var file in files)
            {
                try
                {
                    var size = new FileInfo(file).Length;
                    if (!sizeGroups.ContainsKey(size))
                        sizeGroups[size] = new List<string>();
                    sizeGroups[size].Add(file);
                }
                catch
                {
                    // 忽略无法读取的文件
                }
            }

            return sizeGroups.Values.Where(g => g.Count > 1).ToList();
        }
    }

    /// <summary>
    /// 目录比较结果
    /// </summary>
    public class DirectoryCompareResult
    {
        /// <summary>
        /// 只在目录1中的文件
        /// </summary>
        public List<string> OnlyInDirectory1 { get; } = new();

        /// <summary>
        /// 只在目录2中的文件
        /// </summary>
        public List<string> OnlyInDirectory2 { get; } = new();

        /// <summary>
        /// 相同的文件
        /// </summary>
        public List<string> IdenticalFiles { get; } = new();

        /// <summary>
        /// 不同的文件
        /// </summary>
        public List<FileDifference> DifferentFiles { get; } = new();

        /// <summary>
        /// 是否完全相同
        /// </summary>
        public bool AreIdentical => OnlyInDirectory1.Count == 0 && OnlyInDirectory2.Count == 0 && DifferentFiles.Count == 0;
    }

    /// <summary>
    /// 文件差异
    /// </summary>
    public class FileDifference
    {
        /// <summary>
        /// 文件1路径
        /// </summary>
        public string File1 { get; set; } = string.Empty;

        /// <summary>
        /// 文件2路径
        /// </summary>
        public string File2 { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"{File1} <-> {File2}";
        }
    }
}