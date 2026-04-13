using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using EasyTool.System;
using Xunit;

namespace EasyTool.UnitTests.SystemCategory
{
    /// <summary>
    /// HardwareInfoUtil 测试类
    /// 注意：硬件信息获取方法仅支持 Windows 平台
    /// </summary>
    public class HardwareInfoUtilTests
    {
        #region Windows 平台检查

        private static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        #endregion

        #region 信息类属性测试

        [Fact]
        public void CpuInfo_Properties_CanBeSet()
        {
            var info = new CpuInfo
            {
                Name = "Intel Core i7-10700K",
                Manufacturer = "Intel",
                MaxClockSpeed = 3800,
                NumberOfCores = 8,
                NumberOfLogicalProcessors = 16,
                L2CacheSize = 256,
                L3CacheSize = 16384,
                Architecture = "x64",
                ProcessorId = "BFEBFBFF000906ED"
            };

            Assert.Equal("Intel Core i7-10700K", info.Name);
            Assert.Equal("Intel", info.Manufacturer);
            Assert.Equal(3800, info.MaxClockSpeed);
            Assert.Equal(8, info.NumberOfCores);
            Assert.Equal(16, info.NumberOfLogicalProcessors);
            Assert.Equal(256, info.L2CacheSize);
            Assert.Equal(16384, info.L3CacheSize);
            Assert.Equal("x64", info.Architecture);
            Assert.Equal("BFEBFBFF000906ED", info.ProcessorId);
        }

        [Fact]
        public void CpuInfo_MaxClockSpeedGHz_CalculatesCorrectly()
        {
            var info = new CpuInfo { MaxClockSpeed = 3800 };
            Assert.Equal(3.8, info.MaxClockSpeedGHz);
        }

        [Fact]
        public void CpuInfo_DefaultValues_AreEmptyOrZero()
        {
            var info = new CpuInfo();

            Assert.Equal("", info.Name);
            Assert.Equal("", info.Manufacturer);
            Assert.Equal(0, info.MaxClockSpeed);
            Assert.Equal(0, info.NumberOfCores);
            Assert.Equal("", info.Architecture);
        }

        [Fact]
        public void MemoryInfo_Properties_CanBeSet()
        {
            var info = new MemoryInfo
            {
                TotalCapacity = 16L * 1024 * 1024 * 1024, // 16GB
                AvailableMemory = 8L * 1024 * 1024 * 1024 // 8GB
            };

            Assert.Equal(16L * 1024 * 1024 * 1024, info.TotalCapacity);
            Assert.Equal(8L * 1024 * 1024 * 1024, info.AvailableMemory);
        }

        [Fact]
        public void MemoryInfo_TotalCapacityGB_CalculatesCorrectly()
        {
            var info = new MemoryInfo { TotalCapacity = 16L * 1024 * 1024 * 1024 };
            Assert.Equal(16.0, info.TotalCapacityGB);
        }

        [Fact]
        public void MemoryInfo_UsedMemory_CalculatesCorrectly()
        {
            var info = new MemoryInfo
            {
                TotalCapacity = 16L * 1024 * 1024 * 1024,
                AvailableMemory = 8L * 1024 * 1024 * 1024
            };

            Assert.Equal(8L * 1024 * 1024 * 1024, info.UsedMemory);
            Assert.Equal(8.0, info.UsedMemoryGB);
        }

        [Fact]
        public void MemoryInfo_UsagePercent_CalculatesCorrectly()
        {
            var info = new MemoryInfo
            {
                TotalCapacity = 16L * 1024 * 1024 * 1024,
                AvailableMemory = 8L * 1024 * 1024 * 1024
            };

            Assert.Equal(50.0, info.UsagePercent);
        }

        [Fact]
        public void MemoryInfo_UsagePercent_ZeroTotal_ReturnsZero()
        {
            var info = new MemoryInfo { TotalCapacity = 0 };
            Assert.Equal(0, info.UsagePercent);
        }

        [Fact]
        public void MemoryModule_Properties_CanBeSet()
        {
            var module = new MemoryModule
            {
                Capacity = 8L * 1024 * 1024 * 1024,
                Speed = 3200,
                Manufacturer = "Samsung",
                PartNumber = "M393A2K43CB2",
                MemoryType = "DDR4"
            };

            Assert.Equal(8L * 1024 * 1024 * 1024, module.Capacity);
            Assert.Equal(3200, module.Speed);
            Assert.Equal("Samsung", module.Manufacturer);
            Assert.Equal("M393A2K43CB2", module.PartNumber);
            Assert.Equal("DDR4", module.MemoryType);
        }

        [Fact]
        public void MemoryModule_CapacityGB_CalculatesCorrectly()
        {
            var module = new MemoryModule { Capacity = 8L * 1024 * 1024 * 1024 };
            Assert.Equal(8.0, module.CapacityGB);
        }

        [Fact]
        public void DiskInfo_Properties_CanBeSet()
        {
            var info = new DiskInfo
            {
                DeviceId = "C:",
                VolumeName = "System",
                FileSystem = "NTFS",
                Size = 500L * 1024 * 1024 * 1024,
                FreeSpace = 200L * 1024 * 1024 * 1024,
                DriveType = 2
            };

            Assert.Equal("C:", info.DeviceId);
            Assert.Equal("System", info.VolumeName);
            Assert.Equal("NTFS", info.FileSystem);
            Assert.Equal(500L * 1024 * 1024 * 1024, info.Size);
            Assert.Equal(200L * 1024 * 1024 * 1024, info.FreeSpace);
            Assert.Equal(2, info.DriveType);
        }

        [Fact]
        public void DiskInfo_SizeGB_CalculatesCorrectly()
        {
            var info = new DiskInfo { Size = 500L * 1024 * 1024 * 1024 };
            Assert.Equal(500.0, info.SizeGB);
        }

        [Fact]
        public void DiskInfo_UsedSpace_CalculatesCorrectly()
        {
            var info = new DiskInfo
            {
                Size = 500L * 1024 * 1024 * 1024,
                FreeSpace = 200L * 1024 * 1024 * 1024
            };

            Assert.Equal(300L * 1024 * 1024 * 1024, info.UsedSpace);
            Assert.Equal(300.0, info.UsedSpaceGB);
        }

        [Fact]
        public void DiskInfo_UsagePercent_CalculatesCorrectly()
        {
            var info = new DiskInfo
            {
                Size = 500L * 1024 * 1024 * 1024,
                FreeSpace = 200L * 1024 * 1024 * 1024
            };

            Assert.Equal(60.0, info.UsagePercent);
        }

        [Theory]
        [InlineData(1, "可移动磁盘")]
        [InlineData(2, "本地磁盘")]
        [InlineData(3, "网络驱动器")]
        [InlineData(4, "光盘驱动器")]
        [InlineData(5, "RAM磁盘")]
        [InlineData(99, "未知")]
        public void DiskInfo_DriveTypeName_ReturnsCorrectName(int driveType, string expectedName)
        {
            var info = new DiskInfo { DriveType = driveType };
            Assert.Equal(expectedName, info.DriveTypeName);
        }

        [Fact]
        public void GpuInfo_Properties_CanBeSet()
        {
            var info = new GpuInfo
            {
                Name = "NVIDIA GeForce RTX 3080",
                DriverVersion = "472.12",
                DriverDate = "20210820",
                VideoProcessor = "GA102",
                AdapterRAM = 10L * 1024 * 1024 * 1024,
                CurrentHorizontalResolution = 1920,
                CurrentVerticalResolution = 1080,
                CurrentRefreshRate = 60
            };

            Assert.Equal("NVIDIA GeForce RTX 3080", info.Name);
            Assert.Equal("472.12", info.DriverVersion);
            Assert.Equal(10L * 1024 * 1024 * 1024, info.AdapterRAM);
        }

        [Fact]
        public void GpuInfo_AdapterRAMGB_CalculatesCorrectly()
        {
            var info = new GpuInfo { AdapterRAM = 10L * 1024 * 1024 * 1024 };
            Assert.Equal(10.0, info.AdapterRAMGB);
        }

        [Fact]
        public void GpuInfo_Resolution_ReturnsCorrectString()
        {
            var info = new GpuInfo
            {
                CurrentHorizontalResolution = 1920,
                CurrentVerticalResolution = 1080
            };

            Assert.Equal("1920 x 1080", info.Resolution);
        }

        [Fact]
        public void MotherboardInfo_Properties_CanBeSet()
        {
            var info = new MotherboardInfo
            {
                Manufacturer = "ASUS",
                Product = "ROG STRIX B550-F",
                SerialNumber = "MF70B123456",
                Version = "Rev 1.0"
            };

            Assert.Equal("ASUS", info.Manufacturer);
            Assert.Equal("ROG STRIX B550-F", info.Product);
            Assert.Equal("MF70B123456", info.SerialNumber);
            Assert.Equal("Rev 1.0", info.Version);
        }

        [Fact]
        public void BiosInfo_Properties_CanBeSet()
        {
            var info = new BiosInfo
            {
                Manufacturer = "American Megatrends Inc.",
                Version = "2.50",
                ReleaseDate = "20210701",
                SerialNumber = "123456789",
                SMBIOSBIOSVersion = "2.50"
            };

            Assert.Equal("American Megatrends Inc.", info.Manufacturer);
            Assert.Equal("2.50", info.Version);
            Assert.Equal("20210701", info.ReleaseDate);
        }

        [Fact]
        public void OsInfo_Properties_CanBeSet()
        {
            var info = new OsInfo
            {
                Caption = "Microsoft Windows 11 Pro",
                Version = "10.0.22000",
                BuildNumber = "22000",
                OSArchitecture = "64-bit",
                SerialNumber = "12345-67890",
                TotalVisibleMemorySize = 16L * 1024 * 1024 * 1024,
                FreePhysicalMemory = 8L * 1024 * 1024 * 1024
            };

            Assert.Equal("Microsoft Windows 11 Pro", info.Caption);
            Assert.Equal("10.0.22000", info.Version);
            Assert.Equal("64-bit", info.OSArchitecture);
        }

        [Fact]
        public void OsInfo_DisplayName_ReturnsCorrectString()
        {
            var info = new OsInfo
            {
                Caption = "Microsoft Windows 11 Pro",
                OSArchitecture = "64-bit"
            };

            Assert.Equal("Microsoft Windows 11 Pro 64-bit", info.DisplayName);
        }

        [Fact]
        public void NetworkAdapterInfo_Properties_CanBeSet()
        {
            var info = new NetworkAdapterInfo
            {
                Name = "Intel Ethernet Controller",
                Description = "Intel(R) Ethernet Connection",
                MACAddress = "00:1A:2B:3C:4D:5E",
                Speed = 1_000_000_000, // 1Gbps
                NetConnectionStatus = "Connected",
                AdapterType = "Ethernet"
            };

            Assert.Equal("Intel Ethernet Controller", info.Name);
            Assert.Equal("00:1A:2B:3C:4D:5E", info.MACAddress);
            Assert.Equal(1_000_000_000, info.Speed);
        }

        [Fact]
        public void NetworkAdapterInfo_SpeedMbps_CalculatesCorrectly()
        {
            var info = new NetworkAdapterInfo { Speed = 1_000_000_000 };
            Assert.Equal(1000.0, info.SpeedMbps);
        }

        [Fact]
        public void ComputerSystemInfo_Properties_CanBeSet()
        {
            var info = new ComputerSystemInfo
            {
                Manufacturer = "Dell Inc.",
                Model = "Precision 5560",
                TotalPhysicalMemory = 32L * 1024 * 1024 * 1024,
                NumberOfProcessors = 1,
                NumberOfLogicalProcessors = 16,
                SystemType = "x64-based PC",
                PCSystemType = "1"
            };

            Assert.Equal("Dell Inc.", info.Manufacturer);
            Assert.Equal("Precision 5560", info.Model);
            Assert.Equal(32L * 1024 * 1024 * 1024, info.TotalPhysicalMemory);
            Assert.Equal(1, info.NumberOfProcessors);
            Assert.Equal(16, info.NumberOfLogicalProcessors);
        }

        [Fact]
        public void ComputerSystemInfo_TotalPhysicalMemoryGB_CalculatesCorrectly()
        {
            var info = new ComputerSystemInfo { TotalPhysicalMemory = 32L * 1024 * 1024 * 1024 };
            Assert.Equal(32.0, info.TotalPhysicalMemoryGB);
        }

        #endregion

        #region Windows 平台专用方法测试

        [Fact]
        public void GetCpuInfo_ReturnsInfoOrThrowsPlatformNotSupported()
        {
            if (IsWindows)
            {
                var info = HardwareInfoUtil.GetCpuInfo();
                Assert.NotNull(info);
            }
            else
            {
                Assert.Throws<PlatformNotSupportedException>(() => HardwareInfoUtil.GetCpuInfo());
            }
        }

        [Fact]
        public void GetMemoryInfo_ReturnsInfoOrThrowsPlatformNotSupported()
        {
            if (IsWindows)
            {
                var info = HardwareInfoUtil.GetMemoryInfo();
                Assert.NotNull(info);
                Assert.NotNull(info.Modules);
            }
            else
            {
                Assert.Throws<PlatformNotSupportedException>(() => HardwareInfoUtil.GetMemoryInfo());
            }
        }

        [Fact]
        public void GetDiskInfo_ReturnsDiskListOrThrowsPlatformNotSupported()
        {
            if (IsWindows)
            {
                var disks = HardwareInfoUtil.GetDiskInfo();
                Assert.NotNull(disks);
            }
            else
            {
                Assert.Throws<PlatformNotSupportedException>(() => HardwareInfoUtil.GetDiskInfo());
            }
        }

        [Fact]
        public void GetGpuInfo_ReturnsGpuListOrThrowsPlatformNotSupported()
        {
            if (IsWindows)
            {
                var gpus = HardwareInfoUtil.GetGpuInfo();
                Assert.NotNull(gpus);
            }
            else
            {
                Assert.Throws<PlatformNotSupportedException>(() => HardwareInfoUtil.GetGpuInfo());
            }
        }

        [Fact]
        public void GetMotherboardInfo_ReturnsInfoOrThrowsPlatformNotSupported()
        {
            if (IsWindows)
            {
                var info = HardwareInfoUtil.GetMotherboardInfo();
                Assert.NotNull(info);
            }
            else
            {
                Assert.Throws<PlatformNotSupportedException>(() => HardwareInfoUtil.GetMotherboardInfo());
            }
        }

        [Fact]
        public void GetBiosInfo_ReturnsInfoOrThrowsPlatformNotSupported()
        {
            if (IsWindows)
            {
                var info = HardwareInfoUtil.GetBiosInfo();
                Assert.NotNull(info);
            }
            else
            {
                Assert.Throws<PlatformNotSupportedException>(() => HardwareInfoUtil.GetBiosInfo());
            }
        }

        [Fact]
        public void GetOsInfo_ReturnsInfoOrThrowsPlatformNotSupported()
        {
            if (IsWindows)
            {
                var info = HardwareInfoUtil.GetOsInfo();
                Assert.NotNull(info);
            }
            else
            {
                Assert.Throws<PlatformNotSupportedException>(() => HardwareInfoUtil.GetOsInfo());
            }
        }

        [Fact]
        public void GetNetworkAdapters_ReturnsAdapterListOrThrowsPlatformNotSupported()
        {
            if (IsWindows)
            {
                var adapters = HardwareInfoUtil.GetNetworkAdapters();
                Assert.NotNull(adapters);
            }
            else
            {
                Assert.Throws<PlatformNotSupportedException>(() => HardwareInfoUtil.GetNetworkAdapters());
            }
        }

        [Fact]
        public void GetComputerSystemInfo_ReturnsInfoOrThrowsPlatformNotSupported()
        {
            if (IsWindows)
            {
                var info = HardwareInfoUtil.GetComputerSystemInfo();
                Assert.NotNull(info);
            }
            else
            {
                Assert.Throws<PlatformNotSupportedException>(() => HardwareInfoUtil.GetComputerSystemInfo());
            }
        }

        #endregion
    }
}