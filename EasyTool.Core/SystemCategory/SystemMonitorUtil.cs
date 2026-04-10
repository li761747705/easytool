using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace EasyTool.SystemCategory
{
    /// <summary>
    /// 系统监控工具类
    /// 提供 CPU、内存、磁盘等系统资源的监控功能
    /// </summary>
    public static class SystemMonitorUtil
    {
        #region CPU 监控

        /// <summary>
        /// 获取 CPU 使用率
        /// </summary>
        /// <returns>CPU 使用率（0-100）</returns>
        public static float GetCpuUsage()
        {
            using var cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            cpuCounter.NextValue(); // 第一次调用返回 0
            Thread.Sleep(1000);
            return cpuCounter.NextValue();
        }

        /// <summary>
        /// 异步获取 CPU 使用率
        /// </summary>
        /// <returns>CPU 使用率</returns>
        public static async Task<float> GetCpuUsageAsync()
        {
            return await Task.Run(() => GetCpuUsage()).ConfigureAwait(false);
        }

        /// <summary>
        /// 获取各核心 CPU 使用率
        /// </summary>
        /// <returns>各核心使用率数组</returns>
        public static float[] GetCpuCoreUsage()
        {
            var coreCount = Environment.ProcessorCount;
            var counters = new PerformanceCounter[coreCount];
            var result = new float[coreCount];

            for (int i = 0; i < coreCount; i++)
            {
                counters[i] = new PerformanceCounter("Processor", "% Processor Time", i.ToString());
                counters[i].NextValue();
            }

            Thread.Sleep(1000);

            for (int i = 0; i < coreCount; i++)
            {
                result[i] = counters[i].NextValue();
                counters[i].Dispose();
            }

            return result;
        }

        /// <summary>
        /// 获取 CPU 信息
        /// </summary>
        /// <returns>CPU 信息</returns>
        public static CpuMetrics GetCpuMetrics()
        {
            return new CpuMetrics
            {
                ProcessorCount = Environment.ProcessorCount,
                CurrentUsage = GetCpuUsage()
            };
        }

        #endregion

        #region 内存监控

        /// <summary>
        /// 获取可用内存大小
        /// </summary>
        /// <returns>可用内存（MB）</returns>
        public static long GetAvailableMemory()
        {
            using var memCounter = new PerformanceCounter("Memory", "Available MBytes");
            return (long)memCounter.NextValue();
        }

        /// <summary>
        /// 获取总物理内存大小
        /// </summary>
        /// <returns>总物理内存（字节）</returns>
        public static long GetTotalPhysicalMemory()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return GetTotalPhysicalMemoryWindows();
            }
            return 0;
        }

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetPhysicallyInstalledSystemMemory(out ulong TotalMemoryInKilobytes);

        private static long GetTotalPhysicalMemoryWindows()
        {
            try
            {
                if (GetPhysicallyInstalledSystemMemory(out var totalMemoryKB))
                {
                    return (long)(totalMemoryKB * 1024); // 转换为字节
                }
            }
            catch { }

            // 备用方法
            using var memCounter = new PerformanceCounter("Memory", "Available MBytes");
            var available = memCounter.NextValue();
            // 估算（不准确）
            return (long)(available * 1024 * 1024 * 2); // 假设使用了一半
        }

        /// <summary>
        /// 获取内存使用率
        /// </summary>
        /// <returns>内存使用率（0-100）</returns>
        public static float GetMemoryUsage()
        {
            var totalMemory = GetTotalPhysicalMemory();
            if (totalMemory == 0)
                return 0;

            var availableMemory = GetAvailableMemory() * 1024 * 1024; // MB 转 Bytes
            var usedMemory = totalMemory - availableMemory;
            return (float)usedMemory / totalMemory * 100;
        }

        /// <summary>
        /// 获取当前进程内存使用
        /// </summary>
        /// <returns>内存使用（字节）</returns>
        public static long GetCurrentProcessMemory()
        {
            using var process = Process.GetCurrentProcess();
            process.Refresh();
            return process.WorkingSet64;
        }

        /// <summary>
        /// 获取内存信息
        /// </summary>
        /// <returns>内存信息</returns>
        public static MemoryMetrics GetMemoryMetrics()
        {
            var totalPhysical = GetTotalPhysicalMemory();
            var availableMB = GetAvailableMemory();
            var availableBytes = availableMB * 1024 * 1024;

            return new MemoryMetrics
            {
                TotalPhysicalMemory = totalPhysical,
                AvailablePhysicalMemory = availableBytes,
                UsedPhysicalMemory = totalPhysical - availableBytes,
                MemoryUsagePercent = totalPhysical > 0 ? (float)(totalPhysical - availableBytes) / totalPhysical * 100 : 0,
                CurrentProcessMemory = GetCurrentProcessMemory()
            };
        }

        #endregion

        #region 磁盘监控

        /// <summary>
        /// 获取所有驱动器信息
        /// </summary>
        /// <returns>驱动器信息列表</returns>
        public static List<DiskMetrics> GetDiskMetrics()
        {
            var drives = DriveInfo.GetDrives();
            var result = new List<DiskMetrics>();

            foreach (var drive in drives)
            {
                try
                {
                    var info = new DiskMetrics
                    {
                        Name = drive.Name,
                        DriveType = drive.DriveType.ToString(),
                        VolumeLabel = drive.VolumeLabel,
                        FileSystem = drive.DriveFormat,
                        TotalSize = drive.TotalSize,
                        TotalFreeSpace = drive.TotalFreeSpace,
                        AvailableFreeSpace = drive.AvailableFreeSpace
                    };
                    info.UsedSpace = info.TotalSize - info.TotalFreeSpace;
                    info.UsagePercent = info.TotalSize > 0 ? (float)info.UsedSpace / info.TotalSize * 100 : 0;
                    result.Add(info);
                }
                catch
                {
                    // 跳过无法访问的驱动器
                }
            }

            return result;
        }

        /// <summary>
        /// 获取指定驱动器信息
        /// </summary>
        /// <param name="driveName">驱动器名称（如 "C:"）</param>
        /// <returns>驱动器信息</returns>
        public static DiskMetrics? GetDiskMetrics(string driveName)
        {
            try
            {
                var drive = new DriveInfo(driveName);
                var info = new DiskMetrics
                {
                    Name = drive.Name,
                    DriveType = drive.DriveType.ToString(),
                    VolumeLabel = drive.VolumeLabel,
                    FileSystem = drive.DriveFormat,
                    TotalSize = drive.TotalSize,
                    TotalFreeSpace = drive.TotalFreeSpace,
                    AvailableFreeSpace = drive.AvailableFreeSpace
                };
                info.UsedSpace = info.TotalSize - info.TotalFreeSpace;
                info.UsagePercent = info.TotalSize > 0 ? (float)info.UsedSpace / info.TotalSize * 100 : 0;
                return info;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 获取磁盘读取速度
        /// </summary>
        /// <param name="driveName">驱动器名称</param>
        /// <returns>读取速度（字节/秒）</returns>
        public static long GetDiskReadSpeed(string driveName = null)
        {
            try
            {
                var instance = driveName != null && driveName.Length >= 2
                    ? driveName.Substring(0, 2) + ":"
                    : "_Total";

                using var counter = new PerformanceCounter("PhysicalDisk", "Disk Read Bytes/sec", instance);
                counter.NextValue();
                Thread.Sleep(1000);
                return (long)counter.NextValue();
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 获取磁盘写入速度
        /// </summary>
        /// <param name="driveName">驱动器名称</param>
        /// <returns>写入速度（字节/秒）</returns>
        public static long GetDiskWriteSpeed(string driveName = null)
        {
            try
            {
                var instance = driveName != null && driveName.Length >= 2
                    ? driveName.Substring(0, 2) + ":"
                    : "_Total";

                using var counter = new PerformanceCounter("PhysicalDisk", "Disk Write Bytes/sec", instance);
                counter.NextValue();
                Thread.Sleep(1000);
                return (long)counter.NextValue();
            }
            catch
            {
                return 0;
            }
        }

        #endregion

        #region 网络监控

        /// <summary>
        /// 获取网络接口信息
        /// </summary>
        /// <returns>网络接口列表</returns>
        public static List<NetworkInterfaceInfo> GetNetworkInterfaces()
        {
            var result = new List<NetworkInterfaceInfo>();

            try
            {
                var interfaces = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();

                foreach (var ni in interfaces)
                {
                    var info = new NetworkInterfaceInfo
                    {
                        Name = ni.Name,
                        Description = ni.Description,
                        Id = ni.Id,
                        Type = ni.NetworkInterfaceType.ToString(),
                        Status = ni.OperationalStatus.ToString(),
                        Speed = ni.Speed,
                        IsUp = ni.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up
                    };

                    // 获取 IP 地址
                    var ipProps = ni.GetIPProperties();
                    info.IpAddresses = ipProps.UnicastAddresses
                        .Where(a => a.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        .Select(a => a.Address.ToString())
                        .ToList();

                    result.Add(info);
                }
            }
            catch
            {
                // 忽略异常
            }

            return result;
        }

        /// <summary>
        /// 获取网络下载速度
        /// </summary>
        /// <param name="interfaceName">网络接口名称</param>
        /// <returns>下载速度（字节/秒）</returns>
        public static long GetNetworkDownloadSpeed(string interfaceName = null)
        {
            try
            {
                var instance = interfaceName ?? GetFirstNetworkInterfaceName();
                if (instance == null)
                    return 0;

                using var counter = new PerformanceCounter("Network Interface", "Bytes Received/sec", instance);
                counter.NextValue();
                Thread.Sleep(1000);
                return (long)counter.NextValue();
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 获取网络上传速度
        /// </summary>
        /// <param name="interfaceName">网络接口名称</param>
        /// <returns>上传速度（字节/秒）</returns>
        public static long GetNetworkUploadSpeed(string interfaceName = null)
        {
            try
            {
                var instance = interfaceName ?? GetFirstNetworkInterfaceName();
                if (instance == null)
                    return 0;

                using var counter = new PerformanceCounter("Network Interface", "Bytes Sent/sec", instance);
                counter.NextValue();
                Thread.Sleep(1000);
                return (long)counter.NextValue();
            }
            catch
            {
                return 0;
            }
        }

        private static string? GetFirstNetworkInterfaceName()
        {
            try
            {
                var interfaces = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();
                var first = interfaces.FirstOrDefault(ni =>
                    ni.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up &&
                    ni.NetworkInterfaceType != System.Net.NetworkInformation.NetworkInterfaceType.Loopback);

                return first?.Description;
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region 进程监控

        /// <summary>
        /// 获取占用 CPU 最高的进程
        /// </summary>
        /// <param name="topN">返回数量</param>
        /// <returns>进程列表</returns>
        public static List<ProcessUsageInfo> GetTopCpuProcesses(int topN = 10)
        {
            var processes = Process.GetProcesses();
            var result = new List<ProcessUsageInfo>();

            // 获取第一次采样
            var cpuCounters = new Dictionary<int, PerformanceCounter>();
            foreach (var p in processes)
            {
                try
                {
                    var counter = new PerformanceCounter("Process", "% Processor Time", p.ProcessName);
                    counter.NextValue();
                    cpuCounters[p.Id] = counter;
                }
                catch
                {
                    p.Dispose();
                }
            }

            Thread.Sleep(1000);

            // 获取第二次采样并计算
            foreach (var p in processes)
            {
                try
                {
                    if (cpuCounters.TryGetValue(p.Id, out var counter))
                    {
                        result.Add(new ProcessUsageInfo
                        {
                            Id = p.Id,
                            Name = p.ProcessName,
                            CpuUsage = counter.NextValue() / Environment.ProcessorCount,
                            MemoryUsage = p.WorkingSet64
                        });
                        counter.Dispose();
                    }
                }
                catch { }
                finally
                {
                    p.Dispose();
                }
            }

            return result.OrderByDescending(p => p.CpuUsage).Take(topN).ToList();
        }

        /// <summary>
        /// 获取占用内存最高的进程
        /// </summary>
        /// <param name="topN">返回数量</param>
        /// <returns>进程列表</returns>
        public static List<ProcessUsageInfo> GetTopMemoryProcesses(int topN = 10)
        {
            var processes = Process.GetProcesses();
            var result = new List<ProcessUsageInfo>();

            foreach (var p in processes)
            {
                try
                {
                    result.Add(new ProcessUsageInfo
                    {
                        Id = p.Id,
                        Name = p.ProcessName,
                        MemoryUsage = p.WorkingSet64
                    });
                }
                catch { }
                finally
                {
                    p.Dispose();
                }
            }

            return result.OrderByDescending(p => p.MemoryUsage).Take(topN).ToList();
        }

        /// <summary>
        /// 获取运行中的进程数量
        /// </summary>
        /// <returns>进程数量</returns>
        public static int GetRunningProcessCount()
        {
            return Process.GetProcesses().Length;
        }

        #endregion

        #region 系统信息

        /// <summary>
        /// 获取系统综合信息
        /// </summary>
        /// <returns>系统信息</returns>
        public static SystemInfo GetSystemInfo()
        {
            return new SystemInfo
            {
                MachineName = Environment.MachineName,
                UserName = Environment.UserName,
                OsVersion = RuntimeInformation.OSDescription,
                RuntimeVersion = RuntimeInformation.FrameworkDescription,
                ProcessorCount = Environment.ProcessorCount,
                SystemDirectory = Environment.SystemDirectory,
                CurrentDirectory = Environment.CurrentDirectory,
                SystemUptime = GetSystemUptime(),
                CpuMetrics = GetCpuMetrics(),
                MemoryMetrics = GetMemoryMetrics(),
                DiskMetrics = GetDiskMetrics()
            };
        }

        /// <summary>
        /// 获取系统运行时间
        /// </summary>
        /// <returns>运行时间</returns>
        public static TimeSpan GetSystemUptime()
        {
#if NET5_0_OR_GREATER
            return TimeSpan.FromMilliseconds(Environment.TickCount64);
#else
            // 使用 Environment.TickCount 作为备选（会有溢出问题，但兼容性更好）
            return TimeSpan.FromMilliseconds(Environment.TickCount);
#endif
        }

        #endregion

        #region 实时监控

        /// <summary>
        /// 创建系统监控器
        /// </summary>
        /// <param name="interval">监控间隔</param>
        /// <returns>系统监控器实例</returns>
        public static SystemMonitor CreateMonitor(TimeSpan? interval = null)
        {
            return new SystemMonitor(interval ?? TimeSpan.FromSeconds(1));
        }

        #endregion
    }

    #region 数据类

    /// <summary>
    /// CPU 监控指标
    /// </summary>
    public class CpuMetrics
    {
        /// <summary>
        /// 处理器核心数
        /// </summary>
        public int ProcessorCount { get; set; }

        /// <summary>
        /// 当前使用率（%）
        /// </summary>
        public float CurrentUsage { get; set; }

        public override string ToString()
        {
            return $"核心数: {ProcessorCount}, 使用率: {CurrentUsage:F1}%";
        }
    }

    /// <summary>
    /// 内存监控指标
    /// </summary>
    public class MemoryMetrics
    {
        /// <summary>
        /// 总物理内存（字节）
        /// </summary>
        public long TotalPhysicalMemory { get; set; }

        /// <summary>
        /// 可用物理内存（字节）
        /// </summary>
        public long AvailablePhysicalMemory { get; set; }

        /// <summary>
        /// 已用物理内存（字节）
        /// </summary>
        public long UsedPhysicalMemory { get; set; }

        /// <summary>
        /// 内存使用率（%）
        /// </summary>
        public float MemoryUsagePercent { get; set; }

        /// <summary>
        /// 当前进程内存（字节）
        /// </summary>
        public long CurrentProcessMemory { get; set; }

        /// <summary>
        /// 总物理内存（GB）
        /// </summary>
        public double TotalPhysicalMemoryGB => TotalPhysicalMemory / 1024.0 / 1024 / 1024;

        /// <summary>
        /// 可用物理内存（GB）
        /// </summary>
        public double AvailablePhysicalMemoryGB => AvailablePhysicalMemory / 1024.0 / 1024 / 1024;

        /// <summary>
        /// 已用物理内存（GB）
        /// </summary>
        public double UsedPhysicalMemoryGB => UsedPhysicalMemory / 1024.0 / 1024 / 1024;

        /// <summary>
        /// 当前进程内存（MB）
        /// </summary>
        public double CurrentProcessMemoryMB => CurrentProcessMemory / 1024.0 / 1024;

        public override string ToString()
        {
            return $"总内存: {TotalPhysicalMemoryGB:F2}GB, 可用: {AvailablePhysicalMemoryGB:F2}GB, 使用率: {MemoryUsagePercent:F1}%";
        }
    }

    /// <summary>
    /// 磁盘监控指标
    /// </summary>
    public class DiskMetrics
    {
        /// <summary>
        /// 驱动器名称
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// 驱动器类型
        /// </summary>
        public string? DriveType { get; set; }

        /// <summary>
        /// 卷标
        /// </summary>
        public string? VolumeLabel { get; set; }

        /// <summary>
        /// 文件系统
        /// </summary>
        public string? FileSystem { get; set; }

        /// <summary>
        /// 总大小（字节）
        /// </summary>
        public long TotalSize { get; set; }

        /// <summary>
        /// 总可用空间（字节）
        /// </summary>
        public long TotalFreeSpace { get; set; }

        /// <summary>
        /// 可用空间（字节）
        /// </summary>
        public long AvailableFreeSpace { get; set; }

        /// <summary>
        /// 已用空间（字节）
        /// </summary>
        public long UsedSpace { get; set; }

        /// <summary>
        /// 使用率（%）
        /// </summary>
        public float UsagePercent { get; set; }

        /// <summary>
        /// 总大小（GB）
        /// </summary>
        public double TotalSizeGB => TotalSize / 1024.0 / 1024 / 1024;

        /// <summary>
        /// 可用空间（GB）
        /// </summary>
        public double AvailableFreeSpaceGB => AvailableFreeSpace / 1024.0 / 1024 / 1024;

        public override string ToString()
        {
            return $"{Name} [{VolumeLabel}] - 总: {TotalSizeGB:F2}GB, 可用: {AvailableFreeSpaceGB:F2}GB, 使用率: {UsagePercent:F1}%";
        }
    }

    /// <summary>
    /// 网络接口信息
    /// </summary>
    public class NetworkInterfaceInfo
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// ID
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// 速度（bps）
        /// </summary>
        public long Speed { get; set; }

        /// <summary>
        /// 是否在线
        /// </summary>
        public bool IsUp { get; set; }

        /// <summary>
        /// IP 地址列表
        /// </summary>
        public List<string>? IpAddresses { get; set; }

        /// <summary>
        /// 速度（Mbps）
        /// </summary>
        public double SpeedMbps => Speed / 1000000.0;

        public override string ToString()
        {
            return $"{Name} ({Type}) - {Status}, 速度: {SpeedMbps:F0}Mbps";
        }
    }

    /// <summary>
    /// 进程使用信息
    /// </summary>
    public class ProcessUsageInfo
    {
        /// <summary>
        /// 进程 ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 进程名称
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// CPU 使用率（%）
        /// </summary>
        public float CpuUsage { get; set; }

        /// <summary>
        /// 内存使用（字节）
        /// </summary>
        public long MemoryUsage { get; set; }

        /// <summary>
        /// 内存使用（MB）
        /// </summary>
        public double MemoryUsageMB => MemoryUsage / 1024.0 / 1024;

        public override string ToString()
        {
            return $"[{Id}] {Name} - CPU: {CpuUsage:F1}%, 内存: {MemoryUsageMB:F1}MB";
        }
    }

    /// <summary>
    /// 系统综合信息
    /// </summary>
    public class SystemInfo
    {
        /// <summary>
        /// 机器名
        /// </summary>
        public string? MachineName { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string? UserName { get; set; }

        /// <summary>
        /// 操作系统版本
        /// </summary>
        public string? OsVersion { get; set; }

        /// <summary>
        /// 运行时版本
        /// </summary>
        public string? RuntimeVersion { get; set; }

        /// <summary>
        /// 处理器核心数
        /// </summary>
        public int ProcessorCount { get; set; }

        /// <summary>
        /// 系统目录
        /// </summary>
        public string? SystemDirectory { get; set; }

        /// <summary>
        /// 当前目录
        /// </summary>
        public string? CurrentDirectory { get; set; }

        /// <summary>
        /// 系统运行时间
        /// </summary>
        public TimeSpan SystemUptime { get; set; }

        /// <summary>
        /// CPU 监控指标
        /// </summary>
        public CpuMetrics? CpuMetrics { get; set; }

        /// <summary>
        /// 内存监控指标
        /// </summary>
        public MemoryMetrics? MemoryMetrics { get; set; }

        /// <summary>
        /// 磁盘监控指标
        /// </summary>
        public List<DiskMetrics>? DiskMetrics { get; set; }
    }

    /// <summary>
    /// 系统监控器
    /// </summary>
    public class SystemMonitor : IDisposable
    {
        private readonly TimeSpan _interval;
        private Timer? _timer;
        private bool _disposed;

        /// <summary>
        /// 监控数据更新事件
        /// </summary>
        public event EventHandler<MonitorDataEventArgs>? DataUpdated;

        /// <summary>
        /// 监控间隔
        /// </summary>
        public TimeSpan Interval => _interval;

        /// <summary>
        /// 是否正在监控
        /// </summary>
        public bool IsMonitoring { get; private set; }

        internal SystemMonitor(TimeSpan interval)
        {
            _interval = interval;
        }

        /// <summary>
        /// 开始监控
        /// </summary>
        public void Start()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(SystemMonitor));

            if (IsMonitoring)
                return;

            IsMonitoring = true;
            _timer = new Timer(OnTimerCallback, null, _interval, _interval);
        }

        /// <summary>
        /// 停止监控
        /// </summary>
        public void Stop()
        {
            if (!IsMonitoring)
                return;

            IsMonitoring = false;
            _timer?.Dispose();
            _timer = null;
        }

        private void OnTimerCallback(object? state)
        {
            try
            {
                var data = new MonitorData
                {
                    Timestamp = DateTime.UtcNow,
                    CpuUsage = SystemMonitorUtil.GetCpuUsage(),
                    MemoryUsage = SystemMonitorUtil.GetMemoryUsage(),
                    CurrentProcessMemory = SystemMonitorUtil.GetCurrentProcessMemory(),
                    ProcessCount = SystemMonitorUtil.GetRunningProcessCount()
                };

                DataUpdated?.Invoke(this, new MonitorDataEventArgs { Data = data });
            }
            catch
            {
                // 忽略监控异常
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
    /// 监控数据
    /// </summary>
    public class MonitorData
    {
        /// <summary>
        /// 时间戳
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// CPU 使用率（%）
        /// </summary>
        public float CpuUsage { get; set; }

        /// <summary>
        /// 内存使用率（%）
        /// </summary>
        public float MemoryUsage { get; set; }

        /// <summary>
        /// 当前进程内存（字节）
        /// </summary>
        public long CurrentProcessMemory { get; set; }

        /// <summary>
        /// 进程数量
        /// </summary>
        public int ProcessCount { get; set; }
    }

    /// <summary>
    /// 监控数据事件参数
    /// </summary>
    public class MonitorDataEventArgs : EventArgs
    {
        /// <summary>
        /// 监控数据
        /// </summary>
        public MonitorData? Data { get; set; }
    }

    #endregion
}
