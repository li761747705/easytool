using System;
using System.Runtime.InteropServices;
using EasyTool.System;
using Xunit;

namespace EasyTool.UnitTests.SystemCategory
{
    /// <summary>
    /// PerformanceUtil 测试类
    /// 注意：性能监控功能仅支持 Windows 平台
    /// </summary>
    public class PerformanceUtilTests
    {
        #region Windows 平台检查

        private static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        #endregion

        #region PerformanceData 类测试

        [Fact]
        public void PerformanceData_Properties_CanBeSet()
        {
            var data = new PerformanceData
            {
                CpuUsage = 45.5f,
                MemoryUsagePercent = 60.0f,
                TotalPhysicalMemory = 16L * 1024 * 1024 * 1024,
                AvailableMemoryMB = 4096f,
                DiskReadSpeed = 100_000_000f,
                DiskWriteSpeed = 50_000_000f,
                NetworkSentSpeed = 10_000_000f,
                NetworkReceivedSpeed = 20_000_000f,
                ProcessCount = 150,
                SystemUptime = TimeSpan.FromHours(24)
            };

            Assert.Equal(45.5f, data.CpuUsage);
            Assert.Equal(60.0f, data.MemoryUsagePercent);
            Assert.Equal(16L * 1024 * 1024 * 1024, data.TotalPhysicalMemory);
            Assert.Equal(4096f, data.AvailableMemoryMB);
            Assert.Equal(100_000_000f, data.DiskReadSpeed);
            Assert.Equal(50_000_000f, data.DiskWriteSpeed);
            Assert.Equal(10_000_000f, data.NetworkSentSpeed);
            Assert.Equal(20_000_000f, data.NetworkReceivedSpeed);
            Assert.Equal(150, data.ProcessCount);
            Assert.Equal(TimeSpan.FromHours(24), data.SystemUptime);
        }

        [Fact]
        public void PerformanceData_TotalPhysicalMemoryGB_CalculatesCorrectly()
        {
            var data = new PerformanceData { TotalPhysicalMemory = 16L * 1024 * 1024 * 1024 };
            Assert.Equal(16.0, data.TotalPhysicalMemoryGB);
        }

        [Fact]
        public void PerformanceData_DiskReadSpeedMB_CalculatesCorrectly()
        {
            var data = new PerformanceData { DiskReadSpeed = 100 * 1024 * 1024 };
            Assert.Equal(100.0, data.DiskReadSpeedMB);
        }

        [Fact]
        public void PerformanceData_DiskWriteSpeedMB_CalculatesCorrectly()
        {
            var data = new PerformanceData { DiskWriteSpeed = 50 * 1024 * 1024 };
            Assert.Equal(50.0, data.DiskWriteSpeedMB);
        }

        [Fact]
        public void PerformanceData_NetworkSentSpeedMB_CalculatesCorrectly()
        {
            var data = new PerformanceData { NetworkSentSpeed = 10 * 1024 * 1024 };
            Assert.Equal(10.0, data.NetworkSentSpeedMB);
        }

        [Fact]
        public void PerformanceData_NetworkReceivedSpeedMB_CalculatesCorrectly()
        {
            var data = new PerformanceData { NetworkReceivedSpeed = 20 * 1024 * 1024 };
            Assert.Equal(20.0, data.NetworkReceivedSpeedMB);
        }

        [Fact]
        public void PerformanceData_DefaultValues_AreZero()
        {
            var data = new PerformanceData();

            Assert.Equal(0f, data.CpuUsage);
            Assert.Equal(0f, data.MemoryUsagePercent);
            Assert.Equal(0, data.TotalPhysicalMemory);
            Assert.Equal(0f, data.AvailableMemoryMB);
            Assert.Equal(0, data.ProcessCount);
            Assert.Equal(TimeSpan.Zero, data.SystemUptime);
        }

        #endregion

        #region 跨平台方法测试

        // GetProcessCount 使用 Process.GetProcesses()，跨平台
        [Fact]
        public void GetProcessCount_ReturnsPositiveValue()
        {
            var count = PerformanceUtil.GetProcessCount();
            Assert.True(count > 0);
        }

        // GetSystemUptimeDuration 使用 Environment.TickCount，跨平台
        [Fact]
        public void GetSystemUptimeDuration_ReturnsPositiveTimeSpan()
        {
            var duration = PerformanceUtil.GetSystemUptimeDuration();
            Assert.True(duration > TimeSpan.Zero);
        }

        // GetSystemUptime 使用 Environment.TickCount，跨平台
        [Fact]
        public void GetSystemUptime_ReturnsDateTimeBeforeNow()
        {
            var uptime = PerformanceUtil.GetSystemUptime();
            Assert.True(uptime < DateTime.Now);
        }

        #endregion

        #region Windows 平台专用方法测试

        [Fact]
        public void GetCpuUsage_ReturnsValueOrZero()
        {
            if (IsWindows)
            {
                var usage = PerformanceUtil.GetCpuUsage();
                Assert.True(usage >= 0 && usage <= 100);
            }
            else
            {
                // 非 Windows 平台返回 0
                var usage = PerformanceUtil.GetCpuUsage();
                Assert.Equal(0, usage);
            }
        }

        [Fact]
        public void GetAvailableMemoryMB_ReturnsValueOrZero()
        {
            if (IsWindows)
            {
                var memory = PerformanceUtil.GetAvailableMemoryMB();
                Assert.True(memory >= 0);
            }
            else
            {
                var memory = PerformanceUtil.GetAvailableMemoryMB();
                Assert.Equal(0, memory);
            }
        }

        [Fact]
        public void GetTotalPhysicalMemory_ReturnsPositiveValueOrZero()
        {
            if (IsWindows)
            {
                var memory = PerformanceUtil.GetTotalPhysicalMemory();
                Assert.True(memory > 0);
            }
            else
            {
                // 非 Windows 平台可能返回 0（P/Invoke 不工作）
                var memory = PerformanceUtil.GetTotalPhysicalMemory();
                Assert.True(memory >= 0);
            }
        }

        [Fact]
        public void GetMemoryUsagePercent_ReturnsValueOrZero()
        {
            if (IsWindows)
            {
                var usage = PerformanceUtil.GetMemoryUsagePercent();
                Assert.True(usage >= 0 && usage <= 100);
            }
            else
            {
                var usage = PerformanceUtil.GetMemoryUsagePercent();
                Assert.Equal(0, usage);
            }
        }

        [Fact]
        public void GetDiskReadSpeed_ReturnsValueOrZero()
        {
            var speed = PerformanceUtil.GetDiskReadSpeed();
            Assert.True(speed >= 0);
        }

        [Fact]
        public void GetDiskWriteSpeed_ReturnsValueOrZero()
        {
            var speed = PerformanceUtil.GetDiskWriteSpeed();
            Assert.True(speed >= 0);
        }

        [Fact]
        public void GetNetworkSentSpeed_ReturnsValueOrZero()
        {
            var speed = PerformanceUtil.GetNetworkSentSpeed();
            Assert.True(speed >= 0);
        }

        [Fact]
        public void GetNetworkReceivedSpeed_ReturnsValueOrZero()
        {
            var speed = PerformanceUtil.GetNetworkReceivedSpeed();
            Assert.True(speed >= 0);
        }

        [Fact]
        public void GetPerformanceData_ReturnsCompleteData()
        {
            var data = PerformanceUtil.GetPerformanceData();

            Assert.NotNull(data);
            Assert.True(data.ProcessCount > 0);
            Assert.True(data.SystemUptime > TimeSpan.Zero);
        }

        [Fact]
        public void GetProcessCpuUsage_ReturnsValueOrZero()
        {
            if (IsWindows)
            {
                var processId = global::System.Diagnostics.Process.GetCurrentProcess().Id;
                var usage = PerformanceUtil.GetProcessCpuUsage(processId);
                Assert.True(usage >= 0);
            }
            else
            {
                var usage = PerformanceUtil.GetProcessCpuUsage(-1);
                Assert.Equal(0, usage);
            }
        }

        [Fact]
        public void GetProcessMemoryUsage_ReturnsPositiveValueOrZero()
        {
            if (IsWindows)
            {
                var processId = global::System.Diagnostics.Process.GetCurrentProcess().Id;
                var memory = PerformanceUtil.GetProcessMemoryUsage(processId);
                Assert.True(memory > 0);
            }
            else
            {
                var memory = PerformanceUtil.GetProcessMemoryUsage(-1);
                Assert.Equal(0, memory);
            }
        }

        [Fact]
        public void GetProcessCpuUsage_InvalidProcessId_ReturnsZero()
        {
            var usage = PerformanceUtil.GetProcessCpuUsage(-1);
            Assert.Equal(0, usage);
        }

        [Fact]
        public void GetProcessMemoryUsage_InvalidProcessId_ReturnsZero()
        {
            var memory = PerformanceUtil.GetProcessMemoryUsage(-1);
            Assert.Equal(0, memory);
        }

        #endregion
    }
}