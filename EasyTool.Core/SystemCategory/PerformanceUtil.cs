using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace EasyTool.SystemCategory
{
    /// <summary>
    /// 性能监控工具类
    /// </summary>
    public static class PerformanceUtil
    {
        private static readonly PerformanceCounter? CpuCounter;
        private static readonly PerformanceCounter? MemoryCounter;
        private static readonly PerformanceCounter? DiskReadCounter;
        private static readonly PerformanceCounter? DiskWriteCounter;
        private static readonly PerformanceCounter? NetworkSentCounter;
        private static readonly PerformanceCounter? NetworkReceivedCounter;

        static PerformanceUtil()
        {
            try
            {
                CpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                MemoryCounter = new PerformanceCounter("Memory", "Available MBytes");

                // 获取第一个物理磁盘
                var diskInstance = GetFirstDiskInstance();
                if (diskInstance != null)
                {
                    DiskReadCounter = new PerformanceCounter("PhysicalDisk", "Disk Read Bytes/sec", diskInstance);
                    DiskWriteCounter = new PerformanceCounter("PhysicalDisk", "Disk Write Bytes/sec", diskInstance);
                }

                // 获取第一个网络接口
                var networkInstance = GetFirstNetworkInstance();
                if (networkInstance != null)
                {
                    NetworkSentCounter = new PerformanceCounter("Network Interface", "Bytes Sent/sec", networkInstance);
                    NetworkReceivedCounter = new PerformanceCounter("Network Interface", "Bytes Received/sec", networkInstance);
                }
            }
            catch
            {
                // 性能计数器可能在某些环境不可用
            }
        }

        private static string? GetFirstDiskInstance()
        {
            try
            {
                var category = new PerformanceCounterCategory("PhysicalDisk");
                var instances = category.GetInstanceNames();
                return instances.Length > 0 ? instances[0] : null;
            }
            catch
            {
                return null;
            }
        }

        private static string? GetFirstNetworkInstance()
        {
            try
            {
                var category = new PerformanceCounterCategory("Network Interface");
                var instances = category.GetInstanceNames();
                return instances.Length > 0 ? instances[0] : null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 获取CPU使用率
        /// </summary>
        public static float GetCpuUsage()
        {
            try
            {
                CpuCounter?.NextValue(); // 第一次调用返回0
                Thread.Sleep(100);
                return CpuCounter?.NextValue() ?? 0;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 获取可用内存（MB）
        /// </summary>
        public static float GetAvailableMemoryMB()
        {
            try
            {
                return MemoryCounter?.NextValue() ?? 0;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 获取总物理内存（字节）
        /// </summary>
        public static long GetTotalPhysicalMemory()
        {
            var memStatus = new MEMORYSTATUSEX();
            if (GlobalMemoryStatusEx(memStatus))
            {
                return (long)memStatus.ullTotalPhys;
            }
            return 0;
        }

        /// <summary>
        /// 获取已用内存百分比
        /// </summary>
        public static float GetMemoryUsagePercent()
        {
            var total = GetTotalPhysicalMemory();
            var available = GetAvailableMemoryMB() * 1024 * 1024;
            if (total == 0) return 0;
            return (float)((total - available) / (double)total * 100);
        }

        /// <summary>
        /// 获取磁盘读取速度（字节/秒）
        /// </summary>
        public static float GetDiskReadSpeed()
        {
            try
            {
                DiskReadCounter?.NextValue();
                Thread.Sleep(100);
                return DiskReadCounter?.NextValue() ?? 0;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 获取磁盘写入速度（字节/秒）
        /// </summary>
        public static float GetDiskWriteSpeed()
        {
            try
            {
                DiskWriteCounter?.NextValue();
                Thread.Sleep(100);
                return DiskWriteCounter?.NextValue() ?? 0;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 获取网络发送速度（字节/秒）
        /// </summary>
        public static float GetNetworkSentSpeed()
        {
            try
            {
                NetworkSentCounter?.NextValue();
                Thread.Sleep(100);
                return NetworkSentCounter?.NextValue() ?? 0;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 获取网络接收速度（字节/秒）
        /// </summary>
        public static float GetNetworkReceivedSpeed()
        {
            try
            {
                NetworkReceivedCounter?.NextValue();
                Thread.Sleep(100);
                return NetworkReceivedCounter?.NextValue() ?? 0;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 获取进程数量
        /// </summary>
        public static int GetProcessCount()
        {
            return Process.GetProcesses().Length;
        }

        /// <summary>
        /// 获取系统启动时间
        /// </summary>
        public static DateTime GetSystemUptime()
        {
#if NET5_0_OR_GREATER
            return DateTime.Now - TimeSpan.FromMilliseconds(Environment.TickCount64);
#else
            return DateTime.Now - TimeSpan.FromMilliseconds(Environment.TickCount);
#endif
        }

        /// <summary>
        /// 获取系统运行时长
        /// </summary>
        public static TimeSpan GetSystemUptimeDuration()
        {
#if NET5_0_OR_GREATER
            return TimeSpan.FromMilliseconds(Environment.TickCount64);
#else
            return TimeSpan.FromMilliseconds(Environment.TickCount);
#endif
        }

        /// <summary>
        /// 获取完整的性能数据
        /// </summary>
        public static PerformanceData GetPerformanceData()
        {
            return new PerformanceData
            {
                CpuUsage = GetCpuUsage(),
                MemoryUsagePercent = GetMemoryUsagePercent(),
                TotalPhysicalMemory = GetTotalPhysicalMemory(),
                AvailableMemoryMB = GetAvailableMemoryMB(),
                DiskReadSpeed = GetDiskReadSpeed(),
                DiskWriteSpeed = GetDiskWriteSpeed(),
                NetworkSentSpeed = GetNetworkSentSpeed(),
                NetworkReceivedSpeed = GetNetworkReceivedSpeed(),
                ProcessCount = GetProcessCount(),
                SystemUptime = GetSystemUptimeDuration()
            };
        }

        /// <summary>
        /// 监控进程CPU使用率
        /// </summary>
        public static float GetProcessCpuUsage(int processId)
        {
            try
            {
                using var process = Process.GetProcessById(processId);
                var cpuCounter = new PerformanceCounter("Process", "% Processor Time", process.ProcessName);
                cpuCounter.NextValue();
                Thread.Sleep(100);
                return cpuCounter.NextValue() / Environment.ProcessorCount;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 监控进程内存使用
        /// </summary>
        public static long GetProcessMemoryUsage(int processId)
        {
            try
            {
                using var process = Process.GetProcessById(processId);
                return process.WorkingSet64;
            }
            catch
            {
                return 0;
            }
        }

        #region P/Invoke

        [StructLayout(LayoutKind.Sequential)]
        private class MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;

            public MEMORYSTATUSEX()
            {
                dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
            }
        }

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);

        #endregion
    }

    /// <summary>
    /// 性能数据
    /// </summary>
    public class PerformanceData
    {
        public float CpuUsage { get; set; }
        public float MemoryUsagePercent { get; set; }
        public long TotalPhysicalMemory { get; set; }
        public float AvailableMemoryMB { get; set; }
        public float DiskReadSpeed { get; set; }
        public float DiskWriteSpeed { get; set; }
        public float NetworkSentSpeed { get; set; }
        public float NetworkReceivedSpeed { get; set; }
        public int ProcessCount { get; set; }
        public TimeSpan SystemUptime { get; set; }

        public double TotalPhysicalMemoryGB => TotalPhysicalMemory / (1024.0 * 1024 * 1024);
        public double DiskReadSpeedMB => DiskReadSpeed / (1024.0 * 1024);
        public double DiskWriteSpeedMB => DiskWriteSpeed / (1024.0 * 1024);
        public double NetworkSentSpeedMB => NetworkSentSpeed / (1024.0 * 1024);
        public double NetworkReceivedSpeedMB => NetworkReceivedSpeed / (1024.0 * 1024);
    }
}
