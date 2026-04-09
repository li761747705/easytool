using System;
using System.Collections.Generic;
using System.Management;
using System.Runtime.InteropServices;

namespace EasyTool.System
{
    /// <summary>
    /// 硬件信息工具类
    /// </summary>
    public static class HardwareInfoUtil
    {
        /// <summary>
        /// 获取CPU信息
        /// </summary>
        public static CpuInfo GetCpuInfo()
        {
            var info = new CpuInfo();

            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
                foreach (ManagementObject obj in searcher.Get())
                {
                    info.Name = obj["Name"]?.ToString()?.Trim() ?? "";
                    info.Manufacturer = obj["Manufacturer"]?.ToString() ?? "";
                    info.MaxClockSpeed = Convert.ToInt32(obj["MaxClockSpeed"]);
                    info.NumberOfCores = Convert.ToInt32(obj["NumberOfCores"]);
                    info.NumberOfLogicalProcessors = Convert.ToInt32(obj["NumberOfLogicalProcessors"]);
                    info.L2CacheSize = Convert.ToInt32(obj["L2CacheSize"]);
                    info.L3CacheSize = Convert.ToInt32(obj["L3CacheSize"]);
                    info.Architecture = obj["Architecture"]?.ToString() ?? "";
                    info.ProcessorId = obj["ProcessorId"]?.ToString() ?? "";
                    break;
                }
            }
            catch
            {
                // 在某些环境可能无法访问WMI
            }

            return info;
        }

        /// <summary>
        /// 获取内存信息
        /// </summary>
        public static MemoryInfo GetMemoryInfo()
        {
            var info = new MemoryInfo();

            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemory");
                long totalCapacity = 0;
                var modules = new List<MemoryModule>();

                foreach (ManagementObject obj in searcher.Get())
                {
                    var capacity = Convert.ToInt64(obj["Capacity"]);
                    totalCapacity += capacity;

                    modules.Add(new MemoryModule
                    {
                        Capacity = capacity,
                        Speed = Convert.ToInt32(obj["Speed"]),
                        Manufacturer = obj["Manufacturer"]?.ToString() ?? "",
                        PartNumber = obj["PartNumber"]?.ToString()?.Trim() ?? "",
                        MemoryType = obj["MemoryType"]?.ToString() ?? ""
                    });
                }

                info.TotalCapacity = totalCapacity;
                info.Modules = modules;
            }
            catch
            {
            }

            // 使用GC获取可用内存
            try
            {
#if NET5_0_OR_GREATER
                var gcMemoryInfo = GC.GetGCMemoryInfo();
#if NET10_0_OR_GREATER
                // .NET 10+ 使用 TotalAvailableMemoryBytes 属性
                info.AvailableMemory = gcMemoryInfo.TotalAvailableMemoryBytes;
#else
                info.AvailableMemory = gcMemoryInfo.TotalAvailableMemoryPages * Environment.SystemPageSize;
#endif
#else
                // 对于 netstandard2.1，使用另一种方式获取可用内存
                var memCounter = new global::System.Diagnostics.PerformanceCounter("Memory", "Available Bytes");
                info.AvailableMemory = (long)memCounter.NextValue();
#endif
            }
            catch
            {
                // 如果无法获取，使用0作为默认值
                info.AvailableMemory = 0;
            }

            return info;
        }

        /// <summary>
        /// 获取磁盘信息
        /// </summary>
        public static List<DiskInfo> GetDiskInfo()
        {
            var disks = new List<DiskInfo>();

            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_LogicalDisk");
                foreach (ManagementObject obj in searcher.Get())
                {
                    disks.Add(new DiskInfo
                    {
                        DeviceId = obj["DeviceID"]?.ToString() ?? "",
                        VolumeName = obj["VolumeName"]?.ToString() ?? "",
                        FileSystem = obj["FileSystem"]?.ToString() ?? "",
                        Size = Convert.ToInt64(obj["Size"]),
                        FreeSpace = Convert.ToInt64(obj["FreeSpace"]),
                        DriveType = Convert.ToInt32(obj["DriveType"])
                    });
                }
            }
            catch
            {
            }

            return disks;
        }

        /// <summary>
        /// 获取显卡信息
        /// </summary>
        public static List<GpuInfo> GetGpuInfo()
        {
            var gpus = new List<GpuInfo>();

            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController");
                foreach (ManagementObject obj in searcher.Get())
                {
                    gpus.Add(new GpuInfo
                    {
                        Name = obj["Name"]?.ToString() ?? "",
                        DriverVersion = obj["DriverVersion"]?.ToString() ?? "",
                        DriverDate = obj["DriverDate"]?.ToString() ?? "",
                        VideoProcessor = obj["VideoProcessor"]?.ToString() ?? "",
                        AdapterRAM = Convert.ToInt64(obj["AdapterRAM"]),
                        CurrentHorizontalResolution = Convert.ToInt32(obj["CurrentHorizontalResolution"]),
                        CurrentVerticalResolution = Convert.ToInt32(obj["CurrentVerticalResolution"]),
                        CurrentRefreshRate = Convert.ToInt32(obj["CurrentRefreshRate"])
                    });
                }
            }
            catch
            {
            }

            return gpus;
        }

        /// <summary>
        /// 获取主板信息
        /// </summary>
        public static MotherboardInfo GetMotherboardInfo()
        {
            var info = new MotherboardInfo();

            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_BaseBoard");
                foreach (ManagementObject obj in searcher.Get())
                {
                    info.Manufacturer = obj["Manufacturer"]?.ToString() ?? "";
                    info.Product = obj["Product"]?.ToString() ?? "";
                    info.SerialNumber = obj["SerialNumber"]?.ToString() ?? "";
                    info.Version = obj["Version"]?.ToString() ?? "";
                    break;
                }
            }
            catch
            {
            }

            return info;
        }

        /// <summary>
        /// 获取BIOS信息
        /// </summary>
        public static BiosInfo GetBiosInfo()
        {
            var info = new BiosInfo();

            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_BIOS");
                foreach (ManagementObject obj in searcher.Get())
                {
                    info.Manufacturer = obj["Manufacturer"]?.ToString() ?? "";
                    info.Version = obj["Version"]?.ToString() ?? "";
                    info.ReleaseDate = obj["ReleaseDate"]?.ToString() ?? "";
                    info.SerialNumber = obj["SerialNumber"]?.ToString() ?? "";
                    info.SMBIOSBIOSVersion = obj["SMBIOSBIOSVersion"]?.ToString() ?? "";
                    break;
                }
            }
            catch
            {
            }

            return info;
        }

        /// <summary>
        /// 获取操作系统信息
        /// </summary>
        public static OsInfo GetOsInfo()
        {
            var info = new OsInfo();

            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
                foreach (ManagementObject obj in searcher.Get())
                {
                    info.Caption = obj["Caption"]?.ToString() ?? "";
                    info.Version = obj["Version"]?.ToString() ?? "";
                    info.BuildNumber = obj["BuildNumber"]?.ToString() ?? "";
                    info.OSArchitecture = obj["OSArchitecture"]?.ToString() ?? "";
                    info.SerialNumber = obj["SerialNumber"]?.ToString() ?? "";
                    info.InstallDate = obj["InstallDate"]?.ToString() ?? "";
                    info.LastBootUpTime = obj["LastBootUpTime"]?.ToString() ?? "";
                    info.TotalVisibleMemorySize = Convert.ToInt64(obj["TotalVisibleMemorySize"]) * 1024;
                    info.FreePhysicalMemory = Convert.ToInt64(obj["FreePhysicalMemory"]) * 1024;
                    break;
                }
            }
            catch
            {
            }

            return info;
        }

        /// <summary>
        /// 获取网络适配器信息
        /// </summary>
        public static List<NetworkAdapterInfo> GetNetworkAdapters()
        {
            var adapters = new List<NetworkAdapterInfo>();

            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapter WHERE NetEnabled = true");
                foreach (ManagementObject obj in searcher.Get())
                {
                    adapters.Add(new NetworkAdapterInfo
                    {
                        Name = obj["Name"]?.ToString() ?? "",
                        Description = obj["Description"]?.ToString() ?? "",
                        MACAddress = obj["MACAddress"]?.ToString() ?? "",
                        Speed = Convert.ToInt64(obj["Speed"]),
                        NetConnectionStatus = obj["NetConnectionStatus"]?.ToString() ?? "",
                        AdapterType = obj["AdapterType"]?.ToString() ?? ""
                    });
                }
            }
            catch
            {
            }

            return adapters;
        }

        /// <summary>
        /// 获取计算机系统信息
        /// </summary>
        public static ComputerSystemInfo GetComputerSystemInfo()
        {
            var info = new ComputerSystemInfo();

            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem");
                foreach (ManagementObject obj in searcher.Get())
                {
                    info.Manufacturer = obj["Manufacturer"]?.ToString() ?? "";
                    info.Model = obj["Model"]?.ToString() ?? "";
                    info.TotalPhysicalMemory = Convert.ToInt64(obj["TotalPhysicalMemory"]);
                    info.NumberOfProcessors = Convert.ToInt32(obj["NumberOfProcessors"]);
                    info.NumberOfLogicalProcessors = Convert.ToInt32(obj["NumberOfLogicalProcessors"]);
                    info.SystemType = obj["SystemType"]?.ToString() ?? "";
                    info.PCSystemType = obj["PCSystemType"]?.ToString() ?? "";
                    break;
                }
            }
            catch
            {
            }

            return info;
        }
    }

    #region 信息类

    public class CpuInfo
    {
        public string Name { get; set; } = "";
        public string Manufacturer { get; set; } = "";
        public int MaxClockSpeed { get; set; }
        public int NumberOfCores { get; set; }
        public int NumberOfLogicalProcessors { get; set; }
        public int L2CacheSize { get; set; }
        public int L3CacheSize { get; set; }
        public string Architecture { get; set; } = "";
        public string ProcessorId { get; set; } = "";

        public double MaxClockSpeedGHz => MaxClockSpeed / 1000.0;
    }

    public class MemoryInfo
    {
        public long TotalCapacity { get; set; }
        public long AvailableMemory { get; set; }
        public List<MemoryModule> Modules { get; set; } = new();

        public double TotalCapacityGB => TotalCapacity / (1024.0 * 1024 * 1024);
        public double UsedMemory => TotalCapacity - AvailableMemory;
        public double UsedMemoryGB => UsedMemory / (1024.0 * 1024 * 1024);
        public double UsagePercent => TotalCapacity > 0 ? (double)UsedMemory / TotalCapacity * 100 : 0;
    }

    public class MemoryModule
    {
        public long Capacity { get; set; }
        public int Speed { get; set; }
        public string Manufacturer { get; set; } = "";
        public string PartNumber { get; set; } = "";
        public string MemoryType { get; set; } = "";

        public double CapacityGB => Capacity / (1024.0 * 1024 * 1024);
    }

    public class DiskInfo
    {
        public string DeviceId { get; set; } = "";
        public string VolumeName { get; set; } = "";
        public string FileSystem { get; set; } = "";
        public long Size { get; set; }
        public long FreeSpace { get; set; }
        public int DriveType { get; set; }

        public double SizeGB => Size / (1024.0 * 1024 * 1024);
        public double FreeSpaceGB => FreeSpace / (1024.0 * 1024 * 1024);
        public double UsedSpace => Size - FreeSpace;
        public double UsedSpaceGB => UsedSpace / (1024.0 * 1024 * 1024);
        public double UsagePercent => Size > 0 ? (double)UsedSpace / Size * 100 : 0;
        public string DriveTypeName => DriveType switch
        {
            1 => "可移动磁盘",
            2 => "本地磁盘",
            3 => "网络驱动器",
            4 => "光盘驱动器",
            5 => "RAM磁盘",
            _ => "未知"
        };
    }

    public class GpuInfo
    {
        public string Name { get; set; } = "";
        public string DriverVersion { get; set; } = "";
        public string DriverDate { get; set; } = "";
        public string VideoProcessor { get; set; } = "";
        public long AdapterRAM { get; set; }
        public int CurrentHorizontalResolution { get; set; }
        public int CurrentVerticalResolution { get; set; }
        public int CurrentRefreshRate { get; set; }

        public double AdapterRAMGB => AdapterRAM / (1024.0 * 1024 * 1024);
        public string Resolution => $"{CurrentHorizontalResolution} x {CurrentVerticalResolution}";
    }

    public class MotherboardInfo
    {
        public string Manufacturer { get; set; } = "";
        public string Product { get; set; } = "";
        public string SerialNumber { get; set; } = "";
        public string Version { get; set; } = "";
    }

    public class BiosInfo
    {
        public string Manufacturer { get; set; } = "";
        public string Version { get; set; } = "";
        public string ReleaseDate { get; set; } = "";
        public string SerialNumber { get; set; } = "";
        public string SMBIOSBIOSVersion { get; set; } = "";
    }

    public class OsInfo
    {
        public string Caption { get; set; } = "";
        public string Version { get; set; } = "";
        public string BuildNumber { get; set; } = "";
        public string OSArchitecture { get; set; } = "";
        public string SerialNumber { get; set; } = "";
        public string InstallDate { get; set; } = "";
        public string LastBootUpTime { get; set; } = "";
        public long TotalVisibleMemorySize { get; set; }
        public long FreePhysicalMemory { get; set; }

        public string DisplayName => $"{Caption} {OSArchitecture}";
    }

    public class NetworkAdapterInfo
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string MACAddress { get; set; } = "";
        public long Speed { get; set; }
        public string NetConnectionStatus { get; set; } = "";
        public string AdapterType { get; set; } = "";

        public double SpeedMbps => Speed / 1_000_000.0;
    }

    public class ComputerSystemInfo
    {
        public string Manufacturer { get; set; } = "";
        public string Model { get; set; } = "";
        public long TotalPhysicalMemory { get; set; }
        public int NumberOfProcessors { get; set; }
        public int NumberOfLogicalProcessors { get; set; }
        public string SystemType { get; set; } = "";
        public string PCSystemType { get; set; } = "";

        public double TotalPhysicalMemoryGB => TotalPhysicalMemory / (1024.0 * 1024 * 1024);
    }

    #endregion
}
