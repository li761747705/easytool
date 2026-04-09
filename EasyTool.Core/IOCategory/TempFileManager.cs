using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EasyTool.IOCategory
{
    /// <summary>
    /// 临时文件管理器
    /// 提供临时文件的创建、跟踪和自动清理功能
    /// </summary>
    public class TempFileManager : IDisposable
    {
        private readonly string _baseDirectory;
        private readonly ConcurrentDictionary<string, TempFileInfo> _trackedFiles;
        private readonly Timer? _cleanupTimer;
        private readonly TimeSpan _defaultExpiration;
        private readonly bool _autoCleanup;
        private bool _disposed;

        /// <summary>
        /// 创建临时文件管理器
        /// </summary>
        /// <param name="baseDirectory">临时文件基础目录，默认为系统临时目录</param>
        /// <param name="autoCleanup">是否自动清理过期文件</param>
        /// <param name="cleanupInterval">清理间隔</param>
        /// <param name="defaultExpiration">默认过期时间</param>
        public TempFileManager(
            string? baseDirectory = null,
            bool autoCleanup = true,
            TimeSpan? cleanupInterval = null,
            TimeSpan? defaultExpiration = null)
        {
            _baseDirectory = baseDirectory ?? Path.GetTempPath();
            _trackedFiles = new ConcurrentDictionary<string, TempFileInfo>();
            _autoCleanup = autoCleanup;
            _defaultExpiration = defaultExpiration ?? TimeSpan.FromHours(1);

            // 确保基础目录存在
            if (!Directory.Exists(_baseDirectory))
            {
                Directory.CreateDirectory(_baseDirectory);
            }

            // 启动自动清理定时器
            if (autoCleanup)
            {
                var interval = cleanupInterval ?? TimeSpan.FromMinutes(5);
                _cleanupTimer = new Timer(CleanupCallback, null, interval, interval);
            }
        }

        /// <summary>
        /// 跟踪的文件数量
        /// </summary>
        public int TrackedFileCount => _trackedFiles.Count;

        /// <summary>
        /// 创建临时文件
        /// </summary>
        /// <param name="extension">文件扩展名（包含点号，如 ".txt"）</param>
        /// <param name="prefix">文件名前缀</param>
        /// <param name="expiration">过期时间，null使用默认值</param>
        /// <returns>临时文件完整路径</returns>
        public string CreateFile(string? extension = null, string? prefix = null, TimeSpan? expiration = null)
        {
            var fileName = GenerateFileName(prefix, extension);
            var filePath = Path.Combine(_baseDirectory, fileName);
            var expireAt = DateTime.UtcNow.Add(expiration ?? _defaultExpiration);

            // 创建空文件
            File.Create(filePath).Dispose();

            // 跟踪文件
            var info = new TempFileInfo
            {
                FilePath = filePath,
                CreatedAt = DateTime.UtcNow,
                ExpireAt = expireAt,
                Size = 0
            };
            _trackedFiles[filePath] = info;

            return filePath;
        }

        /// <summary>
        /// 创建临时目录
        /// </summary>
        /// <param name="prefix">目录名前缀</param>
        /// <param name="expiration">过期时间</param>
        /// <returns>临时目录完整路径</returns>
        public string CreateDirectory(string? prefix = null, TimeSpan? expiration = null)
        {
            var dirName = GenerateFileName(prefix, null);
            var dirPath = Path.Combine(_baseDirectory, dirName);
            var expireAt = DateTime.UtcNow.Add(expiration ?? _defaultExpiration);

            // 创建目录
            Directory.CreateDirectory(dirPath);

            // 跟踪目录
            var info = new TempFileInfo
            {
                FilePath = dirPath,
                CreatedAt = DateTime.UtcNow,
                ExpireAt = expireAt,
                IsDirectory = true
            };
            _trackedFiles[dirPath] = info;

            return dirPath;
        }

        /// <summary>
        /// 跟踪现有文件
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="expiration">过期时间</param>
        public void TrackFile(string filePath, TimeSpan? expiration = null)
        {
            if (!File.Exists(filePath) && !Directory.Exists(filePath))
                throw new FileNotFoundException("文件不存在", filePath);

            var isDirectory = Directory.Exists(filePath);
            var expireAt = DateTime.UtcNow.Add(expiration ?? _defaultExpiration);

            var info = new TempFileInfo
            {
                FilePath = filePath,
                CreatedAt = DateTime.UtcNow,
                ExpireAt = expireAt,
                IsDirectory = isDirectory,
                Size = isDirectory ? 0 : new FileInfo(filePath).Length
            };
            _trackedFiles[filePath] = info;
        }

        /// <summary>
        /// 取消跟踪文件（不会删除文件）
        /// </summary>
        /// <param name="filePath">文件路径</param>
        public void UntrackFile(string filePath)
        {
            _trackedFiles.TryRemove(filePath, out _);
        }

        /// <summary>
        /// 删除指定文件
        /// </summary>
        /// <param name="filePath">文件路径</param>
        public void DeleteFile(string filePath)
        {
            if (_trackedFiles.TryRemove(filePath, out var info))
            {
                SafeDelete(info);
            }
        }

        /// <summary>
        /// 清理所有过期文件
        /// </summary>
        /// <returns>清理的文件数量</returns>
        public int CleanupExpired()
        {
            var now = DateTime.UtcNow;
            var expiredFiles = _trackedFiles
                .Where(kvp => kvp.Value.ExpireAt <= now)
                .Select(kvp => kvp.Key)
                .ToList();

            var count = 0;
            foreach (var filePath in expiredFiles)
            {
                if (_trackedFiles.TryRemove(filePath, out var info))
                {
                    if (SafeDelete(info))
                        count++;
                }
            }

            return count;
        }

        /// <summary>
        /// 清理所有跟踪的文件
        /// </summary>
        /// <returns>清理的文件数量</returns>
        public int CleanupAll()
        {
            var count = 0;
            foreach (var kvp in _trackedFiles)
            {
                if (SafeDelete(kvp.Value))
                    count++;
            }
            _trackedFiles.Clear();
            return count;
        }

        /// <summary>
        /// 获取所有跟踪的文件信息
        /// </summary>
        public IReadOnlyList<TempFileInfo> GetTrackedFiles()
        {
            return _trackedFiles.Values.ToList();
        }

        /// <summary>
        /// 获取跟踪文件的总大小
        /// </summary>
        public long GetTotalSize()
        {
            return _trackedFiles.Values.Sum(f => f.Size);
        }

        /// <summary>
        /// 刷新文件大小信息
        /// </summary>
        public void RefreshSizes()
        {
            foreach (var kvp in _trackedFiles.ToList())
            {
                try
                {
                    if (kvp.Value.IsDirectory)
                    {
                        kvp.Value.Size = GetDirectorySize(kvp.Key);
                    }
                    else if (File.Exists(kvp.Key))
                    {
                        kvp.Value.Size = new FileInfo(kvp.Key).Length;
                    }
                }
                catch
                {
                    // 忽略错误
                }
            }
        }

        /// <summary>
        /// 延长文件过期时间
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="extension">延长时间</param>
        public void ExtendExpiration(string filePath, TimeSpan extension)
        {
            if (_trackedFiles.TryGetValue(filePath, out var info))
            {
                info.ExpireAt = info.ExpireAt.Add(extension);
            }
        }

        private string GenerateFileName(string? prefix, string? extension)
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var guid = Guid.NewGuid().ToString("N").Substring(0, 8);
            var name = string.IsNullOrEmpty(prefix) ? $"{timestamp}_{guid}" : $"{prefix}_{timestamp}_{guid}";
            return extension != null ? name + extension : name;
        }

        private bool SafeDelete(TempFileInfo info)
        {
            try
            {
                if (info.IsDirectory)
                {
                    if (Directory.Exists(info.FilePath))
                    {
                        Directory.Delete(info.FilePath, true);
                        return true;
                    }
                }
                else
                {
                    if (File.Exists(info.FilePath))
                    {
                        File.Delete(info.FilePath);
                        return true;
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        private long GetDirectorySize(string path)
        {
            try
            {
                var dirInfo = new DirectoryInfo(path);
                return dirInfo.EnumerateFiles("*", SearchOption.AllDirectories).Sum(f => f.Length);
            }
            catch
            {
                return 0;
            }
        }

        private void CleanupCallback(object? state)
        {
            CleanupExpired();
        }

        /// <summary>
        /// 释放资源并清理所有文件
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            _cleanupTimer?.Dispose();
            CleanupAll();
            _disposed = true;
        }
    }

    /// <summary>
    /// 临时文件信息
    /// </summary>
    public class TempFileInfo
    {
        /// <summary>
        /// 文件路径
        /// </summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 过期时间
        /// </summary>
        public DateTime ExpireAt { get; set; }

        /// <summary>
        /// 文件大小（字节）
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// 是否为目录
        /// </summary>
        public bool IsDirectory { get; set; }

        /// <summary>
        /// 是否已过期
        /// </summary>
        public bool IsExpired => DateTime.UtcNow >= ExpireAt;

        /// <summary>
        /// 剩余时间
        /// </summary>
        public TimeSpan RemainingTime => ExpireAt - DateTime.UtcNow;
    }
}