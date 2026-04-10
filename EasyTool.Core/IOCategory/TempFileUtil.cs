using System;
using System.IO;

namespace EasyTool.IOCategory
{
    /// <summary>
    /// 临时文件工具类
    /// </summary>
    public static class TempFileUtil
    {
        private static readonly object _lock = new();
        private static readonly string _tempDirectory = Path.Combine(Path.GetTempPath(), "EasyTool_Temp");

        /// <summary>
        /// 临时目录
        /// </summary>
        public static string TempDirectory
        {
            get
            {
                if (!Directory.Exists(_tempDirectory))
                    Directory.CreateDirectory(_tempDirectory);
                return _tempDirectory;
            }
        }

        /// <summary>
        /// 创建临时文件
        /// </summary>
        public static string CreateTempFile(string? extension = null, string? prefix = null)
        {
            var fileName = $"{prefix ?? "temp"}_{Guid.NewGuid():N}{extension ?? ".tmp"}";
            var filePath = Path.Combine(TempDirectory, fileName);
            File.Create(filePath).Dispose();
            return filePath;
        }

        /// <summary>
        /// 创建临时目录
        /// </summary>
        public static string CreateTempDirectory(string? prefix = null)
        {
            var dirName = $"{prefix ?? "temp"}_{Guid.NewGuid():N}";
            var dirPath = Path.Combine(TempDirectory, dirName);
            Directory.CreateDirectory(dirPath);
            return dirPath;
        }

        /// <summary>
        /// 创建临时文件并写入内容
        /// </summary>
        public static string CreateTempFileWithContent(string content, string? extension = null, string? prefix = null)
        {
            var filePath = CreateTempFile(extension, prefix);
            File.WriteAllText(filePath, content);
            return filePath;
        }

        /// <summary>
        /// 创建临时文件并写入二进制内容
        /// </summary>
        public static string CreateTempFileWithBytes(byte[] bytes, string? extension = null, string? prefix = null)
        {
            var filePath = CreateTempFile(extension, prefix);
            File.WriteAllBytes(filePath, bytes);
            return filePath;
        }

        /// <summary>
        /// 删除临时文件
        /// </summary>
        public static bool DeleteTempFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 删除临时目录
        /// </summary>
        public static bool DeleteTempDirectory(string dirPath)
        {
            try
            {
                if (Directory.Exists(dirPath))
                {
                    Directory.Delete(dirPath, true);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 清理所有临时文件
        /// </summary>
        public static void CleanupAll()
        {
            lock (_lock)
            {
                if (Directory.Exists(_tempDirectory))
                {
                    try
                    {
                        Directory.Delete(_tempDirectory, true);
                    }
                    catch
                    {
                        // 忽略清理错误
                    }
                }
            }
        }

        /// <summary>
        /// 清理过期的临时文件
        /// </summary>
        public static void CleanupExpired(TimeSpan expiration)
        {
            if (!Directory.Exists(_tempDirectory))
                return;

            var cutoff = DateTime.UtcNow - expiration;

            foreach (var file in Directory.GetFiles(_tempDirectory))
            {
                try
                {
                    if (File.GetCreationTime(file) < cutoff)
                        File.Delete(file);
                }
                catch
                {
                    // 忽略单个文件删除错误
                }
            }

            foreach (var dir in Directory.GetDirectories(_tempDirectory))
            {
                try
                {
                    if (Directory.GetCreationTime(dir) < cutoff)
                        Directory.Delete(dir, true);
                }
                catch
                {
                    // 忽略单个目录删除错误
                }
            }
        }

        /// <summary>
        /// 获取临时文件大小
        /// </summary>
        public static long GetTempDirectorySize()
        {
            if (!Directory.Exists(_tempDirectory))
                return 0;

            long size = 0;
            foreach (var file in Directory.GetFiles(_tempDirectory, "*", SearchOption.AllDirectories))
            {
                try
                {
                    size += new FileInfo(file).Length;
                }
                catch
                {
                    // 忽略单个文件错误
                }
            }
            return size;
        }

        /// <summary>
        /// 获取临时文件数量
        /// </summary>
        public static int GetTempFileCount()
        {
            if (!Directory.Exists(_tempDirectory))
                return 0;

            return Directory.GetFiles(_tempDirectory, "*", SearchOption.AllDirectories).Length;
        }
    }

    /// <summary>
    /// 临时文件自动清理器
    /// </summary>
    public class TempFileScope : IDisposable
    {
        private string? _filePath;
        private string? _directoryPath;
        private bool _disposed;
#pragma warning disable CS0414 // 字段保留供扩展使用
        private readonly bool _isDirectory;
#pragma warning restore CS0414

        /// <summary>
        /// 创建临时文件作用域
        /// </summary>
        public TempFileScope(string? extension = null, string? prefix = null)
        {
            _filePath = TempFileUtil.CreateTempFile(extension, prefix);
            _isDirectory = false;
        }

        private TempFileScope(bool isDirectory, string? prefix)
        {
            if (isDirectory)
            {
                _directoryPath = TempFileUtil.CreateTempDirectory(prefix);
                _isDirectory = true;
            }
            else
            {
                _filePath = TempFileUtil.CreateTempFile(null, prefix);
                _isDirectory = false;
            }
        }

        /// <summary>
        /// 创建临时目录作用域
        /// </summary>
        public static TempFileScope CreateDirectoryScope(string? prefix = null)
        {
            return new TempFileScope(true, prefix);
        }

        /// <summary>
        /// 临时文件路径
        /// </summary>
        public string FilePath => _filePath ?? throw new InvalidOperationException("这不是文件作用域");

        /// <summary>
        /// 临时目录路径
        /// </summary>
        public string DirectoryPath => _directoryPath ?? throw new InvalidOperationException("这不是目录作用域");

        public void Dispose()
        {
            if (_disposed)
                return;

            if (_filePath != null)
                TempFileUtil.DeleteTempFile(_filePath);

            if (_directoryPath != null)
                TempFileUtil.DeleteTempDirectory(_directoryPath);

            _disposed = true;
        }
    }
}