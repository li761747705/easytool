using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace EasyTool.IOCategory
{
    /// <summary>
    /// 文件锁选项
    /// </summary>
    public class FileLockOptions
    {
        /// <summary>
        /// 锁超时时间
        /// </summary>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// 重试间隔
        /// </summary>
        public TimeSpan RetryInterval { get; set; } = TimeSpan.FromMilliseconds(100);

        /// <summary>
        /// 锁文件目录
        /// </summary>
        public string LockDirectory { get; set; } = Path.GetTempPath();
    }

    /// <summary>
    /// 文件锁工具类
    /// 提供跨进程的文件锁定机制
    /// </summary>
    public static class FileLockUtil
    {
        private static readonly FileLockOptions _defaultOptions = new();

        /// <summary>
        /// 获取文件锁
        /// </summary>
        /// <param name="filePath">要锁定的文件路径</param>
        /// <param name="options">锁选项</param>
        /// <returns>文件锁</returns>
        public static FileLock Acquire(string filePath, FileLockOptions? options = null)
        {
            options ??= _defaultOptions;

            var lockFilePath = GetLockFilePath(filePath, options);
            var startTime = DateTime.UtcNow;

            while (true)
            {
                try
                {
                    var fileStream = new FileStream(
                        lockFilePath,
                        FileMode.CreateNew,
                        FileAccess.Write,
                        FileShare.None,
                        1,
                        FileOptions.DeleteOnClose);

                    // 写入锁信息
                    var lockInfo = $"{Environment.MachineName}|{Process.GetCurrentProcess().Id}|{DateTime.UtcNow:O}";
                    var bytes = System.Text.Encoding.UTF8.GetBytes(lockInfo);
                    fileStream.Write(bytes, 0, bytes.Length);
                    fileStream.Flush();

                    return new FileLock(lockFilePath, fileStream);
                }
                catch (IOException)
                {
                    // 检查是否超时
                    if (DateTime.UtcNow - startTime >= options.Timeout)
                    {
                        throw new TimeoutException($"获取文件锁超时: {filePath}");
                    }

                    Thread.Sleep(options.RetryInterval);
                }
            }
        }

        /// <summary>
        /// 异步获取文件锁
        /// </summary>
        /// <param name="filePath">要锁定的文件路径</param>
        /// <param name="options">锁选项</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>文件锁</returns>
        public static async Task<FileLock> AcquireAsync(string filePath, FileLockOptions? options = null, CancellationToken cancellationToken = default)
        {
            options ??= _defaultOptions;

            var lockFilePath = GetLockFilePath(filePath, options);
            var startTime = DateTime.UtcNow;

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var fileStream = new FileStream(
                        lockFilePath,
                        FileMode.CreateNew,
                        FileAccess.Write,
                        FileShare.None,
                        1,
                        FileOptions.DeleteOnClose);

                    var lockInfo = $"{Environment.MachineName}|{Process.GetCurrentProcess().Id}|{DateTime.UtcNow:O}";
                    var bytes = System.Text.Encoding.UTF8.GetBytes(lockInfo);
                    await fileStream.WriteAsync(bytes, 0, bytes.Length, cancellationToken);
                    await fileStream.FlushAsync(cancellationToken);

                    return new FileLock(lockFilePath, fileStream);
                }
                catch (IOException)
                {
                    if (DateTime.UtcNow - startTime >= options.Timeout)
                    {
                        throw new TimeoutException($"获取文件锁超时: {filePath}");
                    }

                    await Task.Delay(options.RetryInterval, cancellationToken);
                }
            }
        }

        /// <summary>
        /// 尝试获取文件锁
        /// </summary>
        /// <param name="filePath">要锁定的文件路径</param>
        /// <param name="fileLock">文件锁</param>
        /// <param name="options">锁选项</param>
        /// <returns>是否成功获取</returns>
        public static bool TryAcquire(string filePath, out FileLock? fileLock, FileLockOptions? options = null)
        {
            options ??= _defaultOptions;

            try
            {
                var lockFilePath = GetLockFilePath(filePath, options);
                var fileStream = new FileStream(
                    lockFilePath,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None,
                    1,
                    FileOptions.DeleteOnClose);

                var lockInfo = $"{Environment.MachineName}|{Process.GetCurrentProcess().Id}|{DateTime.UtcNow:O}";
                var bytes = System.Text.Encoding.UTF8.GetBytes(lockInfo);
                fileStream.Write(bytes, 0, bytes.Length);
                fileStream.Flush();

                fileLock = new FileLock(lockFilePath, fileStream);
                return true;
            }
            catch
            {
                fileLock = null;
                return false;
            }
        }

        /// <summary>
        /// 检查文件是否被锁定
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="options">锁选项</param>
        /// <returns>是否被锁定</returns>
        public static bool IsLocked(string filePath, FileLockOptions? options = null)
        {
            options ??= _defaultOptions;
            var lockFilePath = GetLockFilePath(filePath, options);

            if (!File.Exists(lockFilePath))
                return false;

            // 尝试打开锁文件
            try
            {
                using var stream = new FileStream(
                    lockFilePath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.None);

                return false;
            }
            catch
            {
                return true;
            }
        }

        /// <summary>
        /// 强制释放文件锁
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="options">锁选项</param>
        /// <returns>是否成功释放</returns>
        public static bool ForceRelease(string filePath, FileLockOptions? options = null)
        {
            options ??= _defaultOptions;
            var lockFilePath = GetLockFilePath(filePath, options);

            try
            {
                if (File.Exists(lockFilePath))
                {
                    File.Delete(lockFilePath);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 使用文件锁执行操作
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="action">操作</param>
        /// <param name="options">锁选项</param>
        public static void WithLock(string filePath, Action action, FileLockOptions? options = null)
        {
            using var fileLock = Acquire(filePath, options);
            action();
        }

        /// <summary>
        /// 使用文件锁执行操作并返回结果
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="filePath">文件路径</param>
        /// <param name="func">操作</param>
        /// <param name="options">锁选项</param>
        /// <returns>操作结果</returns>
        public static T WithLock<T>(string filePath, Func<T> func, FileLockOptions? options = null)
        {
            using var fileLock = Acquire(filePath, options);
            return func();
        }

        /// <summary>
        /// 异步使用文件锁执行操作
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="action">操作</param>
        /// <param name="options">锁选项</param>
        /// <param name="cancellationToken">取消令牌</param>
        public static async Task WithLockAsync(string filePath, Func<Task> action, FileLockOptions? options = null, CancellationToken cancellationToken = default)
        {
            using var fileLock = await AcquireAsync(filePath, options, cancellationToken);
            await action();
        }

        /// <summary>
        /// 异步使用文件锁执行操作并返回结果
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="filePath">文件路径</param>
        /// <param name="func">操作</param>
        /// <param name="options">锁选项</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>操作结果</returns>
        public static async Task<T> WithLockAsync<T>(string filePath, Func<Task<T>> func, FileLockOptions? options = null, CancellationToken cancellationToken = default)
        {
            using var fileLock = await AcquireAsync(filePath, options, cancellationToken);
            return await func();
        }

        private static string GetLockFilePath(string filePath, FileLockOptions options)
        {
            var fileName = Path.GetFileName(filePath);
            var directory = Path.GetDirectoryName(Path.GetFullPath(filePath));

            // 使用文件路径的哈希作为锁文件名的一部分
            var hash = Math.Abs(directory?.GetHashCode() ?? 0);
            var lockFileName = $"{fileName}.{hash}.lock";

            return Path.Combine(options.LockDirectory, lockFileName);
        }
    }

    /// <summary>
    /// 文件锁
    /// </summary>
    public class FileLock : IDisposable
    {
        private readonly string _lockFilePath;
        private readonly FileStream _fileStream;
        private bool _disposed;

        internal FileLock(string lockFilePath, FileStream fileStream)
        {
            _lockFilePath = lockFilePath;
            _fileStream = fileStream;
        }

        /// <summary>
        /// 锁文件路径
        /// </summary>
        public string LockFilePath => _lockFilePath;

        /// <summary>
        /// 释放锁
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                _fileStream?.Dispose();
                _disposed = true;
            }
        }

        /// <summary>
        /// 释放锁
        /// </summary>
        public void Release()
        {
            Dispose();
        }
    }
}
