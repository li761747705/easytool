using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace EasyTool.IOCategory
{
    /// <summary>
    /// 压缩工具类
    /// </summary>
    public static class CompressionUtil
    {
        #region GZip

        /// <summary>
        /// GZip压缩
        /// </summary>
        public static byte[] GZipCompress(byte[] data)
        {
            using var output = new MemoryStream();
            using (var gzip = new GZipStream(output, CompressionMode.Compress))
            {
                gzip.Write(data, 0, data.Length);
            }
            return output.ToArray();
        }

        /// <summary>
        /// GZip解压
        /// </summary>
        public static byte[] GZipDecompress(byte[] data)
        {
            using var input = new MemoryStream(data);
            using var gzip = new GZipStream(input, CompressionMode.Decompress);
            using var output = new MemoryStream();
            gzip.CopyTo(output);
            return output.ToArray();
        }

        /// <summary>
        /// GZip压缩字符串
        /// </summary>
        public static string GZipCompressString(string text, Encoding? encoding = null)
        {
            encoding ??= Encoding.UTF8;
            var data = encoding.GetBytes(text);
            var compressed = GZipCompress(data);
            return Convert.ToBase64String(compressed);
        }

        /// <summary>
        /// GZip解压字符串
        /// </summary>
        public static string GZipDecompressString(string compressedText, Encoding? encoding = null)
        {
            encoding ??= Encoding.UTF8;
            var data = Convert.FromBase64String(compressedText);
            var decompressed = GZipDecompress(data);
            return encoding.GetString(decompressed);
        }

        #endregion

        #region Deflate

        /// <summary>
        /// Deflate压缩
        /// </summary>
        public static byte[] DeflateCompress(byte[] data)
        {
            using var output = new MemoryStream();
            using (var deflate = new DeflateStream(output, CompressionMode.Compress))
            {
                deflate.Write(data, 0, data.Length);
            }
            return output.ToArray();
        }

        /// <summary>
        /// Deflate解压
        /// </summary>
        public static byte[] DeflateDecompress(byte[] data)
        {
            using var input = new MemoryStream(data);
            using var deflate = new DeflateStream(input, CompressionMode.Decompress);
            using var output = new MemoryStream();
            deflate.CopyTo(output);
            return output.ToArray();
        }

        #endregion

        #region Zip

        /// <summary>
        /// 压缩文件到Zip
        /// </summary>
        public static void ZipFile(string sourceFilePath, string zipFilePath)
        {
            var directory = Path.GetDirectoryName(zipFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            System.IO.Compression.ZipFile.CreateFromDirectory(
                Path.GetDirectoryName(sourceFilePath) ?? "",
                zipFilePath);
        }

        /// <summary>
        /// 压缩目录到Zip
        /// </summary>
        public static void ZipDirectory(string sourceDirectory, string zipFilePath, bool includeBaseDirectory = true)
        {
            var directory = Path.GetDirectoryName(zipFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            System.IO.Compression.ZipFile.CreateFromDirectory(sourceDirectory, zipFilePath,
                CompressionLevel.Optimal, includeBaseDirectory);
        }

        /// <summary>
        /// 解压Zip文件
        /// </summary>
        public static void Unzip(string zipFilePath, string destinationDirectory)
        {
            System.IO.Compression.ZipFile.ExtractToDirectory(zipFilePath, destinationDirectory);
        }

        /// <summary>
        /// 解压Zip文件（覆盖已存在的文件）
        /// </summary>
        public static void Unzip(string zipFilePath, string destinationDirectory, bool overwrite)
        {
            if (overwrite)
            {
                // 先删除目标目录中的文件
                if (Directory.Exists(destinationDirectory))
                {
                    Directory.Delete(destinationDirectory, true);
                }
                Directory.CreateDirectory(destinationDirectory);
                System.IO.Compression.ZipFile.ExtractToDirectory(zipFilePath, destinationDirectory);
            }
            else
            {
                System.IO.Compression.ZipFile.ExtractToDirectory(zipFilePath, destinationDirectory);
            }
        }

        /// <summary>
        /// 压缩文件列表到Zip
        /// </summary>
        public static void ZipFiles(IEnumerable<string> filePaths, string zipFilePath, string? basePath = null)
        {
            var directory = Path.GetDirectoryName(zipFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            using var archive = System.IO.Compression.ZipFile.Open(zipFilePath, ZipArchiveMode.Create);
            foreach (var filePath in filePaths)
            {
                var entryName = basePath != null
                    ? filePath.Substring(basePath.Length).TrimStart(Path.DirectorySeparatorChar)
                    : Path.GetFileName(filePath);
                archive.CreateEntryFromFile(filePath, entryName);
            }
        }

        /// <summary>
        /// 获取Zip文件中的文件列表
        /// </summary>
        public static List<string> GetZipEntries(string zipFilePath)
        {
            using var archive = System.IO.Compression.ZipFile.OpenRead(zipFilePath);
            return archive.Entries.Select(e => e.FullName).ToList();
        }

        /// <summary>
        /// 从Zip中提取单个文件
        /// </summary>
        public static void ExtractFile(string zipFilePath, string entryName, string destinationPath)
        {
            using var archive = System.IO.Compression.ZipFile.OpenRead(zipFilePath);
            var entry = archive.GetEntry(entryName)
                ?? throw new FileNotFoundException($"Zip中未找到文件: {entryName}");
            entry.ExtractToFile(destinationPath, true);
        }

        /// <summary>
        /// 向Zip添加文件
        /// </summary>
        public static void AddFileToZip(string zipFilePath, string filePath, string? entryName = null)
        {
            using var archive = System.IO.Compression.ZipFile.Open(zipFilePath, ZipArchiveMode.Update);
            archive.CreateEntryFromFile(filePath, entryName ?? Path.GetFileName(filePath));
        }

        /// <summary>
        /// 从Zip删除文件
        /// </summary>
        public static void RemoveFileFromZip(string zipFilePath, string entryName)
        {
            using var archive = System.IO.Compression.ZipFile.Open(zipFilePath, ZipArchiveMode.Update);
            var entry = archive.GetEntry(entryName)
                ?? throw new FileNotFoundException($"Zip中未找到文件: {entryName}");
            entry.Delete();
        }

        #endregion

        #region Brotli

        /// <summary>
        /// Brotli压缩
        /// </summary>
        public static byte[] BrotliCompress(byte[] data)
        {
            using var output = new MemoryStream();
            using (var brotli = new BrotliStream(output, CompressionMode.Compress))
            {
                brotli.Write(data, 0, data.Length);
            }
            return output.ToArray();
        }

        /// <summary>
        /// Brotli解压
        /// </summary>
        public static byte[] BrotliDecompress(byte[] data)
        {
            using var input = new MemoryStream(data);
            using var brotli = new BrotliStream(input, CompressionMode.Decompress);
            using var output = new MemoryStream();
            brotli.CopyTo(output);
            return output.ToArray();
        }

        #endregion

        #region 压缩率计算

        /// <summary>
        /// 计算压缩率
        /// </summary>
        public static double CalculateCompressionRatio(long originalSize, long compressedSize)
        {
            if (originalSize == 0)
                return 0;
            return (double)(originalSize - compressedSize) / originalSize * 100;
        }

        /// <summary>
        /// 获取最佳压缩级别
        /// </summary>
        public static CompressionLevel GetOptimalCompressionLevel(double targetRatio)
        {
            return targetRatio switch
            {
                > 80 => CompressionLevel.Optimal,
                > 50 => CompressionLevel.Optimal,
                > 20 => CompressionLevel.Fastest,
                _ => CompressionLevel.NoCompression
            };
        }

        #endregion
    }
}