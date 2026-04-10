using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace EasyTool.IOCategory
{
    /// <summary>
    /// 文件分片工具类
    /// 支持大文件分片上传、下载和合并
    /// </summary>
    public static class FileChunkUtil
    {
        /// <summary>
        /// 分片文件
        /// </summary>
        /// <param name="filePath">源文件路径</param>
        /// <param name="outputDir">输出目录</param>
        /// <param name="chunkSize">分片大小（字节）</param>
        /// <param name="progress">进度回调</param>
        /// <returns>分片信息</returns>
        public static ChunkInfo Split(string filePath, string outputDir, long chunkSize = 5 * 1024 * 1024, Action<double>? progress = null)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("文件不存在", filePath);

            Directory.CreateDirectory(outputDir);

            var fileInfo = new FileInfo(filePath);
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var extension = Path.GetExtension(filePath);
            var totalChunks = (int)Math.Ceiling((double)fileInfo.Length / chunkSize);
            var fileId = Guid.NewGuid().ToString("N");

            var chunkInfo = new ChunkInfo
            {
                FileId = fileId,
                FileName = fileInfo.Name,
                FileSize = fileInfo.Length,
                ChunkSize = chunkSize,
                TotalChunks = totalChunks,
                FileHash = ComputeFileHash(filePath),
                Chunks = new List<ChunkDetail>()
            };

            using var sourceStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var buffer = new byte[Math.Min(chunkSize, int.MaxValue)];

            for (int i = 0; i < totalChunks; i++)
            {
                var chunkFileName = $"{fileName}_{i + 1:D5}{extension}.chunk";
                var chunkFilePath = Path.Combine(outputDir, chunkFileName);

                var bytesRead = sourceStream.Read(buffer, 0, buffer.Length);

                using var chunkStream = new FileStream(chunkFilePath, FileMode.Create, FileAccess.Write);
                chunkStream.Write(buffer, 0, bytesRead);

                var chunkHash = ComputeHash(buffer, 0, bytesRead);

                chunkInfo.Chunks.Add(new ChunkDetail
                {
                    Index = i + 1,
                    ChunkFile = chunkFileName,
                    Size = bytesRead,
                    Hash = chunkHash
                });

                progress?.Invoke((double)(i + 1) / totalChunks * 100);
            }

            // 保存分片信息文件
            var infoPath = Path.Combine(outputDir, $"{fileName}.chunkinfo");
            SaveChunkInfo(chunkInfo, infoPath);

            return chunkInfo;
        }

        /// <summary>
        /// 异步分片文件
        /// </summary>
        public static async Task<ChunkInfo> SplitAsync(string filePath, string outputDir, long chunkSize = 5 * 1024 * 1024, Action<double>? progress = null)
        {
            return await Task.Run(() => Split(filePath, outputDir, chunkSize, progress)).ConfigureAwait(false);
        }

        /// <summary>
        /// 合并分片文件
        /// </summary>
        /// <param name="chunkInfo">分片信息</param>
        /// <param name="chunkDir">分片文件目录</param>
        /// <param name="outputPath">输出文件路径</param>
        /// <param name="progress">进度回调</param>
        public static void Merge(ChunkInfo chunkInfo, string chunkDir, string outputPath, Action<double>? progress = null)
        {
            var dir = new DirectoryInfo(chunkDir);

            using var outputStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write);

            foreach (var chunk in chunkInfo.Chunks.OrderBy(c => c.Index))
            {
                var chunkFilePath = Path.Combine(chunkDir, chunk.ChunkFile);

                if (!File.Exists(chunkFilePath))
                    throw new FileNotFoundException($"分片文件不存在: {chunk.ChunkFile}");

                using var chunkStream = new FileStream(chunkFilePath, FileMode.Open, FileAccess.Read);
                var buffer = new byte[chunk.Size];
                chunkStream.Read(buffer, 0, buffer.Length);

                // 验证分片哈希
                var hash = ComputeHash(buffer, 0, buffer.Length);
                if (hash != chunk.Hash)
                    throw new InvalidDataException($"分片 {chunk.Index} 哈希验证失败");

                outputStream.Write(buffer, 0, buffer.Length);

                progress?.Invoke((double)chunk.Index / chunkInfo.TotalChunks * 100);
            }

            // 验证最终文件哈希
            var finalHash = ComputeFileHash(outputPath);
            if (finalHash != chunkInfo.FileHash)
                throw new InvalidDataException("合并后文件哈希验证失败");
        }

        /// <summary>
        /// 异步合并分片文件
        /// </summary>
        public static async Task MergeAsync(ChunkInfo chunkInfo, string chunkDir, string outputPath, Action<double>? progress = null)
        {
            await Task.Run(() => Merge(chunkInfo, chunkDir, outputPath, progress)).ConfigureAwait(false);
        }

        /// <summary>
        /// 从信息文件加载分片信息
        /// </summary>
        /// <param name="infoFilePath">信息文件路径</param>
        /// <returns>分片信息</returns>
        public static ChunkInfo LoadChunkInfo(string infoFilePath)
        {
            var json = File.ReadAllText(infoFilePath);
            return System.Text.Json.JsonSerializer.Deserialize<ChunkInfo>(json)
                ?? throw new InvalidDataException("无效的分片信息文件");
        }

        /// <summary>
        /// 保存分片信息到文件
        /// </summary>
        private static void SaveChunkInfo(ChunkInfo chunkInfo, string infoFilePath)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(chunkInfo, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(infoFilePath, json);
        }

        /// <summary>
        /// 验证分片完整性
        /// </summary>
        /// <param name="chunkInfo">分片信息</param>
        /// <param name="chunkDir">分片目录</param>
        /// <returns>验证结果</returns>
        public static ChunkValidationResult Validate(ChunkInfo chunkInfo, string chunkDir)
        {
            var result = new ChunkValidationResult
            {
                IsValid = true,
                MissingChunks = new List<int>(),
                CorruptedChunks = new List<int>()
            };

            foreach (var chunk in chunkInfo.Chunks)
            {
                var chunkFilePath = Path.Combine(chunkDir, chunk.ChunkFile);

                if (!File.Exists(chunkFilePath))
                {
                    result.IsValid = false;
                    result.MissingChunks.Add(chunk.Index);
                    continue;
                }

                var fileInfo = new FileInfo(chunkFilePath);
                if (fileInfo.Length != chunk.Size)
                {
                    result.IsValid = false;
                    result.CorruptedChunks.Add(chunk.Index);
                    continue;
                }

                var hash = ComputeFileHash(chunkFilePath);
                if (hash != chunk.Hash)
                {
                    result.IsValid = false;
                    result.CorruptedChunks.Add(chunk.Index);
                }
            }

            return result;
        }

        /// <summary>
        /// 获取上传进度
        /// </summary>
        /// <param name="chunkInfo">分片信息</param>
        /// <param name="uploadedChunks">已上传的分片索引</param>
        /// <returns>上传进度（百分比）</returns>
        public static double GetUploadProgress(ChunkInfo chunkInfo, HashSet<int> uploadedChunks)
        {
            if (chunkInfo.TotalChunks == 0)
                return 0;

            return (double)uploadedChunks.Count / chunkInfo.TotalChunks * 100;
        }

        /// <summary>
        /// 计算文件哈希
        /// </summary>
        private static string ComputeFileHash(string filePath)
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            using var md5 = MD5.Create();
            var hash = md5.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        /// <summary>
        /// 计算数据哈希
        /// </summary>
        private static string ComputeHash(byte[] buffer, int offset, int count)
        {
            using var md5 = MD5.Create();
            var hash = md5.ComputeHash(buffer, offset, count);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        #region 流式分片

        /// <summary>
        /// 流式读取分片
        /// </summary>
        /// <param name="stream">输入流</param>
        /// <param name="chunkSize">分片大小</param>
        /// <returns>分片数据枚举</returns>
        public static IEnumerable<ChunkData> ReadChunks(Stream stream, long chunkSize = 5 * 1024 * 1024)
        {
            var buffer = new byte[Math.Min(chunkSize, int.MaxValue)];
            int index = 1;

            while (true)
            {
                var bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                    break;

                var chunk = new byte[bytesRead];
                Buffer.BlockCopy(buffer, 0, chunk, 0, bytesRead);

                yield return new ChunkData
                {
                    Index = index++,
                    Data = chunk
                };
            }
        }

        /// <summary>
        /// 流式写入分片
        /// </summary>
        /// <param name="stream">输出流</param>
        /// <param name="chunks">分片数据</param>
        public static void WriteChunks(Stream stream, IEnumerable<ChunkData> chunks)
        {
            foreach (var chunk in chunks.OrderBy(c => c.Index))
            {
                stream.Write(chunk.Data, 0, chunk.Data.Length);
            }
        }

        #endregion
    }

    /// <summary>
    /// 分片信息
    /// </summary>
    public class ChunkInfo
    {
        /// <summary>
        /// 文件唯一标识
        /// </summary>
        public string FileId { get; set; } = string.Empty;

        /// <summary>
        /// 原始文件名
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// 文件大小
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// 分片大小
        /// </summary>
        public long ChunkSize { get; set; }

        /// <summary>
        /// 总分片数
        /// </summary>
        public int TotalChunks { get; set; }

        /// <summary>
        /// 文件哈希
        /// </summary>
        public string FileHash { get; set; } = string.Empty;

        /// <summary>
        /// 分片详情列表
        /// </summary>
        public List<ChunkDetail> Chunks { get; set; } = new();
    }

    /// <summary>
    /// 分片详情
    /// </summary>
    public class ChunkDetail
    {
        /// <summary>
        /// 分片索引（从1开始）
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// 分片文件名
        /// </summary>
        public string ChunkFile { get; set; } = string.Empty;

        /// <summary>
        /// 分片大小
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        /// 分片哈希
        /// </summary>
        public string Hash { get; set; } = string.Empty;
    }

    /// <summary>
    /// 分片数据
    /// </summary>
    public class ChunkData
    {
        /// <summary>
        /// 分片索引
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// 分片数据
        /// </summary>
        public byte[] Data { get; set; } = Array.Empty<byte>();

        /// <summary>
        /// 大小
        /// </summary>
        public int Size => Data.Length;
    }

    /// <summary>
    /// 分片验证结果
    /// </summary>
    public class ChunkValidationResult
    {
        /// <summary>
        /// 是否完整有效
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// 缺失的分片索引
        /// </summary>
        public List<int> MissingChunks { get; set; } = new();

        /// <summary>
        /// 损坏的分片索引
        /// </summary>
        public List<int> CorruptedChunks { get; set; } = new();
    }
}
