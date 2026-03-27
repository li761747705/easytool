using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace EasyTool.IOCategory
{
    /// <summary>
    /// 压缩包工具类
    /// 支持 ZIP、TAR、GZip 等格式
    /// </summary>
    public static class ArchiveUtil
    {
        #region ZIP 操作

        /// <summary>
        /// 创建 ZIP 压缩包
        /// </summary>
        /// <param name="sourcePath">源文件或目录路径</param>
        /// <param name="zipPath">ZIP 文件路径</param>
        /// <param name="compressionLevel">压缩级别</param>
        /// <param name="includeBaseDirectory">是否包含根目录</param>
        public static void CreateZip(string sourcePath, string zipPath, CompressionLevel compressionLevel = CompressionLevel.Optimal, bool includeBaseDirectory = false)
        {
            if (File.Exists(sourcePath))
            {
                // 压缩单个文件
                using var archive = ZipFile.Open(zipPath, ZipArchiveMode.Create);
                archive.CreateEntryFromFile(sourcePath, Path.GetFileName(sourcePath), compressionLevel);
            }
            else if (Directory.Exists(sourcePath))
            {
                // 压缩目录
                ZipFile.CreateFromDirectory(sourcePath, zipPath, compressionLevel, includeBaseDirectory);
            }
            else
            {
                throw new FileNotFoundException("源路径不存在", sourcePath);
            }
        }

        /// <summary>
        /// 解压 ZIP 文件
        /// </summary>
        /// <param name="zipPath">ZIP 文件路径</param>
        /// <param name="extractPath">解压目录</param>
        /// <param name="overwrite">是否覆盖已存在的文件</param>
        public static void ExtractZip(string zipPath, string extractPath, bool overwrite = false)
        {
            ZipFile.ExtractToDirectory(zipPath, extractPath, overwrite);
        }

        /// <summary>
        /// 列出 ZIP 文件内容
        /// </summary>
        /// <param name="zipPath">ZIP 文件路径</param>
        /// <returns>文件条目列表</returns>
        public static List<ArchiveEntry> ListZip(string zipPath)
        {
            using var archive = ZipFile.OpenRead(zipPath);
            return archive.Entries.Select(e => new ArchiveEntry
            {
                Name = e.Name,
                FullName = e.FullName,
                Length = e.Length,
                CompressedLength = e.CompressedLength,
                LastWriteTime = e.LastWriteTime.DateTime,
                IsDirectory = string.IsNullOrEmpty(e.Name)
            }).ToList();
        }

        /// <summary>
        /// 从 ZIP 中提取单个文件
        /// </summary>
        /// <param name="zipPath">ZIP 文件路径</param>
        /// <param name="entryName">条目名称</param>
        /// <param name="destinationPath">目标路径</param>
        public static void ExtractFileFromZip(string zipPath, string entryName, string destinationPath)
        {
            using var archive = ZipFile.OpenRead(zipPath);
            var entry = archive.GetEntry(entryName)
                ?? throw new FileNotFoundException($"ZIP 中找不到条目: {entryName}");

            entry.ExtractToFile(destinationPath, true);
        }

        /// <summary>
        /// 向 ZIP 添加文件
        /// </summary>
        /// <param name="zipPath">ZIP 文件路径</param>
        /// <param name="filePath">要添加的文件路径</param>
        /// <param name="entryName">ZIP 中的条目名称</param>
        public static void AddFileToZip(string zipPath, string filePath, string? entryName = null)
        {
            using var archive = ZipFile.Open(zipPath, ZipArchiveMode.Update);
            archive.CreateEntryFromFile(filePath, entryName ?? Path.GetFileName(filePath));
        }

        /// <summary>
        /// 从 ZIP 删除文件
        /// </summary>
        /// <param name="zipPath">ZIP 文件路径</param>
        /// <param name="entryName">要删除的条目名称</param>
        public static void RemoveFileFromZip(string zipPath, string entryName)
        {
            using var archive = ZipFile.Open(zipPath, ZipArchiveMode.Update);
            var entry = archive.GetEntry(entryName)
                ?? throw new FileNotFoundException($"ZIP 中找不到条目: {entryName}");
            entry.Delete();
        }

        #endregion

        #region GZip 操作

        /// <summary>
        /// 使用 GZip 压缩文件
        /// </summary>
        /// <param name="sourcePath">源文件路径</param>
        /// <param name="destinationPath">目标文件路径（可选，默认添加 .gz 后缀）</param>
        public static void CompressGZip(string sourcePath, string? destinationPath = null)
        {
            destinationPath ??= sourcePath + ".gz";

            using var sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read);
            using var destinationStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write);
            using var gzipStream = new GZipStream(destinationStream, CompressionMode.Compress);

            sourceStream.CopyTo(gzipStream);
        }

        /// <summary>
        /// 解压 GZip 文件
        /// </summary>
        /// <param name="sourcePath">GZip 文件路径</param>
        /// <param name="destinationPath">目标文件路径（可选，默认移除 .gz 后缀）</param>
        public static void DecompressGZip(string sourcePath, string? destinationPath = null)
        {
            if (destinationPath == null)
            {
                destinationPath = sourcePath.EndsWith(".gz", StringComparison.OrdinalIgnoreCase)
                    ? sourcePath.Substring(0, sourcePath.Length - 3)
                    : sourcePath + ".out";
            }

            using var sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read);
            using var destinationStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write);
            using var gzipStream = new GZipStream(sourceStream, CompressionMode.Decompress);

            gzipStream.CopyTo(destinationStream);
        }

        /// <summary>
        /// 压缩字节数组
        /// </summary>
        /// <param name="data">原始数据</param>
        /// <returns>压缩后的数据</returns>
        public static byte[] CompressGZip(byte[] data)
        {
            using var output = new MemoryStream();
            using (var gzip = new GZipStream(output, CompressionMode.Compress))
            {
                gzip.Write(data, 0, data.Length);
            }
            return output.ToArray();
        }

        /// <summary>
        /// 解压字节数组
        /// </summary>
        /// <param name="compressedData">压缩数据</param>
        /// <returns>解压后的数据</returns>
        public static byte[] DecompressGZip(byte[] compressedData)
        {
            using var input = new MemoryStream(compressedData);
            using var gzip = new GZipStream(input, CompressionMode.Decompress);
            using var output = new MemoryStream();
            gzip.CopyTo(output);
            return output.ToArray();
        }

        #endregion

        #region Tar 操作

        /// <summary>
        /// 创建 TAR 归档
        /// </summary>
        /// <param name="sourcePath">源目录路径</param>
        /// <param name="tarPath">TAR 文件路径</param>
        public static void CreateTar(string sourcePath, string tarPath)
        {
            using var output = new FileStream(tarPath, FileMode.Create, FileAccess.Write);
            using var tar = new TarWriter(output);

            if (Directory.Exists(sourcePath))
            {
                var dir = new DirectoryInfo(sourcePath);
                foreach (var file in dir.GetFiles("*", SearchOption.AllDirectories))
                {
                    var relativePath = GetRelativePath(dir.FullName, file.FullName);
                    tar.Write(file.FullName, relativePath);
                }
            }
            else if (File.Exists(sourcePath))
            {
                tar.Write(sourcePath, Path.GetFileName(sourcePath));
            }
        }

        /// <summary>
        /// 创建 TAR.GZ 归档
        /// </summary>
        /// <param name="sourcePath">源目录路径</param>
        /// <param name="tarGzPath">TAR.GZ 文件路径</param>
        public static void CreateTarGz(string sourcePath, string tarGzPath)
        {
            using var output = new FileStream(tarGzPath, FileMode.Create, FileAccess.Write);
            using var gzip = new GZipStream(output, CompressionMode.Compress);
            using var tar = new TarWriter(gzip);

            if (Directory.Exists(sourcePath))
            {
                var dir = new DirectoryInfo(sourcePath);
                foreach (var file in dir.GetFiles("*", SearchOption.AllDirectories))
                {
                    var relativePath = GetRelativePath(dir.FullName, file.FullName);
                    tar.Write(file.FullName, relativePath);
                }
            }
            else if (File.Exists(sourcePath))
            {
                tar.Write(sourcePath, Path.GetFileName(sourcePath));
            }
        }

        private static string GetRelativePath(string basePath, string fullPath)
        {
            if (fullPath.StartsWith(basePath))
            {
                var relative = fullPath.Substring(basePath.Length);
                return relative.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            }
            return fullPath;
        }

        #endregion

        #region 内存压缩

        /// <summary>
        /// 将多个文件压缩到内存
        /// </summary>
        /// <param name="files">文件字典（文件名 -> 文件内容）</param>
        /// <returns>压缩后的数据</returns>
        public static byte[] CreateZipInMemory(Dictionary<string, byte[]> files)
        {
            using var output = new MemoryStream();
            using (var archive = new ZipArchive(output, ZipArchiveMode.Create, true))
            {
                foreach (var kvp in files)
                {
                    var entry = archive.CreateEntry(kvp.Key);
                    using var entryStream = entry.Open();
                    entryStream.Write(kvp.Value, 0, kvp.Value.Length);
                }
            }
            return output.ToArray();
        }

        /// <summary>
        /// 从内存中解压文件
        /// </summary>
        /// <param name="zipData">ZIP 数据</param>
        /// <returns>文件字典</returns>
        public static Dictionary<string, byte[]> ExtractZipFromMemory(byte[] zipData)
        {
            var result = new Dictionary<string, byte[]>();

            using var input = new MemoryStream(zipData);
            using var archive = new ZipArchive(input, ZipArchiveMode.Read);

            foreach (var entry in archive.Entries)
            {
                if (!string.IsNullOrEmpty(entry.Name))
                {
                    using var entryStream = entry.Open();
                    using var output = new MemoryStream();
                    entryStream.CopyTo(output);
                    result[entry.FullName] = output.ToArray();
                }
            }

            return result;
        }

        #endregion

        #region 流式压缩

        /// <summary>
        /// 创建压缩流
        /// </summary>
        /// <param name="outputStream">输出流</param>
        /// <returns>压缩流</returns>
        public static Stream CreateCompressStream(Stream outputStream)
        {
            return new GZipStream(outputStream, CompressionMode.Compress);
        }

        /// <summary>
        /// 创建解压流
        /// </summary>
        /// <param name="inputStream">输入流</param>
        /// <returns>解压流</returns>
        public static Stream CreateDecompressStream(Stream inputStream)
        {
            return new GZipStream(inputStream, CompressionMode.Decompress);
        }

        #endregion
    }

    /// <summary>
    /// 压缩包条目
    /// </summary>
    public class ArchiveEntry
    {
        /// <summary>
        /// 文件名
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 完整路径
        /// </summary>
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// 原始大小
        /// </summary>
        public long Length { get; set; }

        /// <summary>
        /// 压缩后大小
        /// </summary>
        public long CompressedLength { get; set; }

        /// <summary>
        /// 最后修改时间
        /// </summary>
        public DateTime LastWriteTime { get; set; }

        /// <summary>
        /// 是否为目录
        /// </summary>
        public bool IsDirectory { get; set; }

        /// <summary>
        /// 压缩率
        /// </summary>
        public double CompressionRatio => Length > 0 ? (1 - (double)CompressedLength / Length) * 100 : 0;
    }

    #region TarWriter 简单实现

    internal class TarWriter : IDisposable
    {
        private readonly Stream _stream;
        private static readonly byte[] EmptyBlock = new byte[512];

        public TarWriter(Stream stream)
        {
            _stream = stream;
        }

        public void Write(string filePath, string entryName)
        {
            var fileInfo = new FileInfo(filePath);
            var header = CreateHeader(entryName, fileInfo.Length, fileInfo.LastWriteTime);
            _stream.Write(header, 0, header.Length);

            using var fileStream = fileInfo.OpenRead();
            fileStream.CopyTo(_stream);

            // 填充到 512 字节边界
            var remainder = fileInfo.Length % 512;
            if (remainder > 0)
            {
                _stream.Write(EmptyBlock, 0, (int)(512 - remainder));
            }
        }

        private byte[] CreateHeader(string name, long size, DateTime mtime)
        {
            var header = new byte[512];
            var nameBytes = System.Text.Encoding.UTF8.GetBytes(name);

            // 名称
            Array.Copy(nameBytes, header, Math.Min(nameBytes.Length, 100));

            // 文件模式
            var mode = "0000644\0"u8.ToArray();
            Array.Copy(mode, 0, header, 100, mode.Length);

            // UID/GID
            var uid = "0000000\0"u8.ToArray();
            Array.Copy(uid, 0, header, 108, uid.Length);
            Array.Copy(uid, 0, header, 116, uid.Length);

            // 大小（八进制）
            var sizeStr = Convert.ToString(size, 8).PadLeft(11, '0') + "\0";
            var sizeBytes = System.Text.Encoding.ASCII.GetBytes(sizeStr);
            Array.Copy(sizeBytes, 0, header, 124, sizeBytes.Length);

            // 修改时间
            var unixTime = new DateTimeOffset(mtime).ToUnixTimeSeconds();
            var mtimeStr = Convert.ToString(unixTime, 8).PadLeft(11, '0') + "\0";
            var mtimeBytes = System.Text.Encoding.ASCII.GetBytes(mtimeStr);
            Array.Copy(mtimeBytes, 0, header, 136, mtimeBytes.Length);

            // 类型标志
            header[156] = (byte)'0'; // 普通文件

            // 校验和（先填空格）
            for (int i = 148; i < 156; i++) header[i] = (byte)' ';

            // 计算校验和
            int checksum = 0;
            foreach (var b in header) checksum += b;

            var checksumStr = Convert.ToString(checksum, 8).PadLeft(6, '0') + "\0 ";
            var checksumBytes = System.Text.Encoding.ASCII.GetBytes(checksumStr);
            Array.Copy(checksumBytes, 0, header, 148, checksumBytes.Length);

            return header;
        }

        public void Dispose()
        {
            // 写入两个空块作为文件结束
            _stream.Write(EmptyBlock, 0, EmptyBlock.Length);
            _stream.Write(EmptyBlock, 0, EmptyBlock.Length);
        }
    }

    #endregion
}
