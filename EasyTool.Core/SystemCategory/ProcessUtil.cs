using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EasyTool.SystemCategory
{
    /// <summary>
    /// 进程管理工具类
    /// 提供进程的启动、停止、监控等功能
    /// </summary>
    public static class ProcessUtil
    {
        #region 进程启动

        /// <summary>
        /// 启动进程
        /// </summary>
        /// <param name="fileName">可执行文件名或路径</param>
        /// <param name="arguments">命令行参数</param>
        /// <returns>启动的进程</returns>
        public static Process Start(string fileName, string? arguments = null)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments ?? string.Empty,
                UseShellExecute = true
            };

            return Process.Start(startInfo) ?? throw new InvalidOperationException($"无法启动进程: {fileName}");
        }

        /// <summary>
        /// 启动进程并等待完成
        /// </summary>
        /// <param name="fileName">可执行文件名或路径</param>
        /// <param name="arguments">命令行参数</param>
        /// <param name="timeout">超时时间</param>
        /// <returns>进程退出代码</returns>
        public static int StartAndWait(string fileName, string? arguments = null, TimeSpan? timeout = null)
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments ?? string.Empty,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();

            if (timeout.HasValue)
            {
                if (!process.WaitForExit((int)timeout.Value.TotalMilliseconds))
                {
                    process.Kill();
                    throw new TimeoutException($"进程执行超时: {fileName}");
                }
            }
            else
            {
                process.WaitForExit();
            }

            return process.ExitCode;
        }

        /// <summary>
        /// 启动进程并获取输出
        /// </summary>
        /// <param name="fileName">可执行文件名或路径</param>
        /// <param name="arguments">命令行参数</param>
        /// <param name="timeout">超时时间</param>
        /// <returns>执行结果（退出代码、标准输出、标准错误）</returns>
        public static ProcessResult Execute(string fileName, string? arguments = null, TimeSpan? timeout = null)
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments ?? string.Empty,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8
                }
            };

            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();

            process.OutputDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                    outputBuilder.AppendLine(e.Data);
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                    errorBuilder.AppendLine(e.Data);
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            bool exited;
            if (timeout.HasValue)
            {
                exited = process.WaitForExit((int)timeout.Value.TotalMilliseconds);
                if (!exited)
                {
                    process.Kill();
                    process.WaitForExit();
                }
            }
            else
            {
                process.WaitForExit();
                exited = true;
            }

            // 确保异步输出完成
            process.WaitForExit();

            return new ProcessResult
            {
                ExitCode = process.ExitCode,
                StandardOutput = outputBuilder.ToString(),
                StandardError = errorBuilder.ToString(),
                TimedOut = !exited
            };
        }

        /// <summary>
        /// 异步执行进程并获取输出
        /// </summary>
        /// <param name="fileName">可执行文件名或路径</param>
        /// <param name="arguments">命令行参数</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>执行结果</returns>
        public static async Task<ProcessResult> ExecuteAsync(string fileName, string? arguments = null, CancellationToken cancellationToken = default)
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments ?? string.Empty,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8
                },
                EnableRaisingEvents = true
            };

            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();
            var tcs = new TaskCompletionSource<bool>();

            process.OutputDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                    outputBuilder.AppendLine(e.Data);
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                    errorBuilder.AppendLine(e.Data);
            };

            process.Exited += (sender, e) =>
            {
                tcs.TrySetResult(true);
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            using (cancellationToken.Register(() =>
            {
                try
                {
                    process.Kill();
                }
                catch { }
                tcs.TrySetCanceled(cancellationToken);
            }))
            {
                await tcs.Task.ConfigureAwait(false);
            }

            return new ProcessResult
            {
                ExitCode = process.ExitCode,
                StandardOutput = outputBuilder.ToString(),
                StandardError = errorBuilder.ToString(),
                TimedOut = cancellationToken.IsCancellationRequested
            };
        }

        /// <summary>
        /// 以管理员权限启动进程
        /// </summary>
        /// <param name="fileName">可执行文件名或路径</param>
        /// <param name="arguments">命令行参数</param>
        /// <returns>启动的进程</returns>
        public static Process StartAsAdmin(string fileName, string? arguments = null)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments ?? string.Empty,
                Verb = "runas",
                UseShellExecute = true
            };

            return Process.Start(startInfo) ?? throw new InvalidOperationException($"无法启动进程: {fileName}");
        }

        #endregion

        #region 进程查找

        /// <summary>
        /// 根据名称查找进程
        /// </summary>
        /// <param name="processName">进程名称（不含扩展名）</param>
        /// <returns>进程数组</returns>
        public static Process[] FindByName(string processName)
        {
            return Process.GetProcessesByName(processName);
        }

        /// <summary>
        /// 根据 ID 获取进程
        /// </summary>
        /// <param name="processId">进程 ID</param>
        /// <returns>进程对象</returns>
        public static Process? GetById(int processId)
        {
            try
            {
                return Process.GetProcessById(processId);
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        /// <summary>
        /// 获取所有进程
        /// </summary>
        /// <returns>进程数组</returns>
        public static Process[] GetAll()
        {
            return Process.GetProcesses();
        }

        /// <summary>
        /// 检查进程是否在运行
        /// </summary>
        /// <param name="processName">进程名称</param>
        /// <returns>是否在运行</returns>
        public static bool IsRunning(string processName)
        {
            return Process.GetProcessesByName(processName).Length > 0;
        }

        /// <summary>
        /// 检查进程 ID 是否存在
        /// </summary>
        /// <param name="processId">进程 ID</param>
        /// <returns>是否存在</returns>
        public static bool Exists(int processId)
        {
            try
            {
                Process.GetProcessById(processId);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region 进程终止

        /// <summary>
        /// 终止进程
        /// </summary>
        /// <param name="process">进程对象</param>
        /// <param name="entireProcessTree">是否终止整个进程树</param>
        public static void Kill(Process process, bool entireProcessTree = false)
        {
            if (process == null)
                throw new ArgumentNullException(nameof(process));

            try
            {
#if NET5_0_OR_GREATER
                process.Kill(entireProcessTree);
#else
                process.Kill();
#endif
            }
            catch (Win32Exception)
            {
                // 进程正在终止或无法访问
            }
            catch (InvalidOperationException)
            {
                // 进程已经退出
            }
        }

        /// <summary>
        /// 根据名称终止所有同名进程
        /// </summary>
        /// <param name="processName">进程名称</param>
        /// <returns>终止的进程数量</returns>
        public static int KillByName(string processName)
        {
            var processes = Process.GetProcessesByName(processName);
            int killed = 0;

            foreach (var process in processes)
            {
                try
                {
                    process.Kill();
                    killed++;
                }
                catch
                {
                    // 忽略终止失败的进程
                }
                finally
                {
                    process.Dispose();
                }
            }

            return killed;
        }

        /// <summary>
        /// 尝试优雅关闭进程，超时后强制终止
        /// </summary>
        /// <param name="process">进程对象</param>
        /// <param name="timeout">等待超时时间</param>
        /// <returns>是否优雅关闭</returns>
        public static bool CloseOrKill(Process process, TimeSpan timeout)
        {
            if (process == null)
                throw new ArgumentNullException(nameof(process));

            try
            {
                process.CloseMainWindow();
                if (process.WaitForExit((int)timeout.TotalMilliseconds))
                {
                    return true;
                }

                process.Kill();
                return false;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region 进程信息

        /// <summary>
        /// 获取当前进程
        /// </summary>
        /// <returns>当前进程</returns>
        public static Process GetCurrent()
        {
            return Process.GetCurrentProcess();
        }

        /// <summary>
        /// 获取进程信息
        /// </summary>
        /// <param name="process">进程对象</param>
        /// <returns>进程信息</returns>
        public static ProcessInfo GetInfo(Process process)
        {
            if (process == null)
                throw new ArgumentNullException(nameof(process));

            try
            {
                process.Refresh();
                return new ProcessInfo
                {
                    Id = process.Id,
                    ProcessName = process.ProcessName,
                    MainWindowTitle = process.MainWindowTitle,
                    StartTime = process.StartTime,
                    TotalProcessorTime = process.TotalProcessorTime,
                    WorkingSet64 = process.WorkingSet64,
                    VirtualMemorySize64 = process.VirtualMemorySize64,
                    PagedMemorySize64 = process.PagedMemorySize64,
                    NonpagedSystemMemorySize64 = process.NonpagedSystemMemorySize64,
                    ThreadsCount = process.Threads.Count,
                    HandlesCount = process.HandleCount,
                    Responding = process.Responding
                };
            }
            catch (Win32Exception)
            {
                return new ProcessInfo { Id = process.Id, ProcessName = process.ProcessName };
            }
        }

        /// <summary>
        /// 获取进程的命令行参数
        /// </summary>
        /// <param name="processId">进程 ID</param>
        /// <returns>命令行参数</returns>
        public static string? GetCommandLine(int processId)
        {
            try
            {
                using var process = Process.GetProcessById(processId);
                // 在 Windows 上通过 WMI 获取命令行
                // 这里简化实现，返回空
                return null;
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region 进程监控

        /// <summary>
        /// 等待进程退出
        /// </summary>
        /// <param name="process">进程对象</param>
        /// <param name="timeout">超时时间</param>
        /// <returns>是否在超时前退出</returns>
        public static bool WaitForExit(Process process, TimeSpan? timeout = null)
        {
            if (process == null)
                throw new ArgumentNullException(nameof(process));

            if (timeout.HasValue)
            {
                return process.WaitForExit((int)timeout.Value.TotalMilliseconds);
            }

            process.WaitForExit();
            return true;
        }

        /// <summary>
        /// 异步等待进程退出
        /// </summary>
        /// <param name="process">进程对象</param>
        /// <param name="cancellationToken">取消令牌</param>
        public static async Task WaitForExitAsync(Process process, CancellationToken cancellationToken = default)
        {
            if (process == null)
                throw new ArgumentNullException(nameof(process));

            process.EnableRaisingEvents = true;

            var tcs = new TaskCompletionSource<bool>();
            process.Exited += (sender, e) => tcs.TrySetResult(true);

            if (process.HasExited)
            {
                return;
            }

            using (cancellationToken.Register(() => tcs.TrySetCanceled()))
            {
                await tcs.Task.ConfigureAwait(false);
            }
        }

        /// <summary>
        /// 创建进程监控器
        /// </summary>
        /// <param name="processName">要监控的进程名称</param>
        /// <param name="interval">检查间隔</param>
        /// <returns>进程监控器</returns>
        public static ProcessMonitor CreateMonitor(string processName, TimeSpan? interval = null)
        {
            return new ProcessMonitor(processName, interval ?? TimeSpan.FromSeconds(1));
        }

        #endregion
    }

    #region 辅助类

    /// <summary>
    /// 进程执行结果
    /// </summary>
    public class ProcessResult
    {
        /// <summary>
        /// 退出代码
        /// </summary>
        public int ExitCode { get; set; }

        /// <summary>
        /// 标准输出
        /// </summary>
        public string StandardOutput { get; set; } = string.Empty;

        /// <summary>
        /// 标准错误
        /// </summary>
        public string StandardError { get; set; } = string.Empty;

        /// <summary>
        /// 是否超时
        /// </summary>
        public bool TimedOut { get; set; }

        /// <summary>
        /// 是否成功（退出代码为0）
        /// </summary>
        public bool Success => ExitCode == 0 && !TimedOut;

        public override string ToString()
        {
            return $"ExitCode: {ExitCode}, Success: {Success}, TimedOut: {TimedOut}";
        }
    }

    /// <summary>
    /// 进程信息
    /// </summary>
    public class ProcessInfo
    {
        /// <summary>
        /// 进程 ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 进程名称
        /// </summary>
        public string? ProcessName { get; set; }

        /// <summary>
        /// 主窗口标题
        /// </summary>
        public string? MainWindowTitle { get; set; }

        /// <summary>
        /// 启动时间
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 总处理器时间
        /// </summary>
        public TimeSpan TotalProcessorTime { get; set; }

        /// <summary>
        /// 工作集大小（物理内存）
        /// </summary>
        public long WorkingSet64 { get; set; }

        /// <summary>
        /// 虚拟内存大小
        /// </summary>
        public long VirtualMemorySize64 { get; set; }

        /// <summary>
        /// 分页内存大小
        /// </summary>
        public long PagedMemorySize64 { get; set; }

        /// <summary>
        /// 非分页系统内存大小
        /// </summary>
        public long NonpagedSystemMemorySize64 { get; set; }

        /// <summary>
        /// 线程数
        /// </summary>
        public int ThreadsCount { get; set; }

        /// <summary>
        /// 句柄数
        /// </summary>
        public int HandlesCount { get; set; }

        /// <summary>
        /// 是否响应
        /// </summary>
        public bool Responding { get; set; }

        /// <summary>
        /// 内存使用量（MB）
        /// </summary>
        public double MemoryMB => WorkingSet64 / 1024.0 / 1024.0;

        /// <summary>
        /// CPU 使用时间（秒）
        /// </summary>
        public double CpuTimeSeconds => TotalProcessorTime.TotalSeconds;

        public override string ToString()
        {
            return $"[{Id}] {ProcessName} - Memory: {MemoryMB:F2}MB, CPU: {CpuTimeSeconds:F2}s, Threads: {ThreadsCount}";
        }
    }

    /// <summary>
    /// 进程监控器
    /// </summary>
    public class ProcessMonitor : IDisposable
    {
        private readonly string _processName;
        private readonly TimeSpan _interval;
        private Timer? _timer;
        private bool _disposed;
        private bool _isRunning;

        /// <summary>
        /// 进程启动事件
        /// </summary>
        public event EventHandler<ProcessEventArgs>? ProcessStarted;

        /// <summary>
        /// 进程退出事件
        /// </summary>
        public event EventHandler<ProcessEventArgs>? ProcessExited;

        /// <summary>
        /// 进程状态变化事件
        /// </summary>
        public event EventHandler<ProcessStatusEventArgs>? StatusChanged;

        /// <summary>
        /// 监控的进程名称
        /// </summary>
        public string ProcessName => _processName;

        /// <summary>
        /// 是否正在监控
        /// </summary>
        public bool IsMonitoring => _isRunning;

        /// <summary>
        /// 当前运行的进程数量
        /// </summary>
        public int RunningCount { get; private set; }

        internal ProcessMonitor(string processName, TimeSpan interval)
        {
            _processName = processName;
            _interval = interval;
        }

        /// <summary>
        /// 开始监控
        /// </summary>
        public void Start()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(ProcessMonitor));

            if (_isRunning)
                return;

            _isRunning = true;
            RunningCount = Process.GetProcessesByName(_processName).Length;
            _timer = new Timer(CheckProcesses, null, _interval, _interval);
        }

        /// <summary>
        /// 停止监控
        /// </summary>
        public void Stop()
        {
            if (!_isRunning)
                return;

            _isRunning = false;
            _timer?.Dispose();
            _timer = null;
        }

        private void CheckProcesses(object? state)
        {
            try
            {
                var currentProcesses = Process.GetProcessesByName(_processName);
                int currentCount = currentProcesses.Length;

                if (currentCount != RunningCount)
                {
                    if (currentCount > RunningCount)
                    {
                        // 有新进程启动
                        ProcessStarted?.Invoke(this, new ProcessEventArgs
                        {
                            ProcessName = _processName,
                            Count = currentCount
                        });
                    }
                    else
                    {
                        // 有进程退出
                        ProcessExited?.Invoke(this, new ProcessEventArgs
                        {
                            ProcessName = _processName,
                            Count = currentCount
                        });
                    }

                    StatusChanged?.Invoke(this, new ProcessStatusEventArgs
                    {
                        ProcessName = _processName,
                        PreviousCount = RunningCount,
                        CurrentCount = currentCount
                    });

                    RunningCount = currentCount;
                }

                foreach (var process in currentProcesses)
                {
                    process.Dispose();
                }
            }
            catch
            {
                // 忽略监控过程中的异常
            }
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            Stop();
            _disposed = true;
        }
    }

    /// <summary>
    /// 进程事件参数
    /// </summary>
    public class ProcessEventArgs : EventArgs
    {
        /// <summary>
        /// 进程名称
        /// </summary>
        public string? ProcessName { get; set; }

        /// <summary>
        /// 当前数量
        /// </summary>
        public int Count { get; set; }
    }

    /// <summary>
    /// 进程状态事件参数
    /// </summary>
    public class ProcessStatusEventArgs : EventArgs
    {
        /// <summary>
        /// 进程名称
        /// </summary>
        public string? ProcessName { get; set; }

        /// <summary>
        /// 之前的数量
        /// </summary>
        public int PreviousCount { get; set; }

        /// <summary>
        /// 当前的数量
        /// </summary>
        public int CurrentCount { get; set; }
    }

    #endregion
}
