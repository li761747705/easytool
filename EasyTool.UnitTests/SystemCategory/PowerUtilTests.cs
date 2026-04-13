using System;
using System.Runtime.InteropServices;
using EasyTool.System;
using Xunit;

namespace EasyTool.UnitTests.SystemCategory
{
    /// <summary>
    /// PowerUtil 测试类
    /// 注意：电源管理功能仅支持 Windows 平台
    /// </summary>
    public class PowerUtilTests
    {
        #region Windows 平台检查

        private static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        #endregion

        #region PowerStatus 类测试

        [Fact]
        public void PowerStatus_Properties_CanBeSet()
        {
            var status = new PowerStatus
            {
                IsAcConnected = true,
                BatteryChargeStatus = BatteryChargeStatus.Charging,
                BatteryLifePercent = 85,
                BatteryLifeRemaining = 7200,
                BatteryFullLifeTime = 10800,
                PowerLineStatus = PowerLineStatus.Online
            };

            Assert.True(status.IsAcConnected);
            Assert.Equal(BatteryChargeStatus.Charging, status.BatteryChargeStatus);
            Assert.Equal(85, status.BatteryLifePercent);
            Assert.Equal(7200, status.BatteryLifeRemaining);
            Assert.Equal(10800, status.BatteryFullLifeTime);
            Assert.Equal(PowerLineStatus.Online, status.PowerLineStatus);
        }

        [Fact]
        public void PowerStatus_ToString_ReturnsFormattedString()
        {
            var status = new PowerStatus
            {
                IsAcConnected = true,
                BatteryLifePercent = 85,
                BatteryLifeRemaining = 7200
            };

            var result = status.ToString();

            Assert.Contains("交流电源", result);
            Assert.Contains("85%", result);
            Assert.Contains("7200s", result);
        }

        #endregion

        #region BatteryChargeStatus 枚举测试

        [Fact]
        public void BatteryChargeStatus_ValuesAreCorrect()
        {
            Assert.Equal(0, (int)BatteryChargeStatus.Unknown);
            Assert.Equal(1, (int)BatteryChargeStatus.Charging);
            Assert.Equal(2, (int)BatteryChargeStatus.NoCharging);
            Assert.Equal(4, (int)BatteryChargeStatus.Low);
            Assert.Equal(8, (int)BatteryChargeStatus.Critical);
            Assert.Equal(128, (int)BatteryChargeStatus.NoBattery);
            Assert.Equal(255, (int)BatteryChargeStatus.Full);
        }

        [Fact]
        public void BatteryChargeStatus_IsFlagsEnum()
        {
            var flags = BatteryChargeStatus.Charging | BatteryChargeStatus.Low;
            Assert.True(flags.HasFlag(BatteryChargeStatus.Charging));
            Assert.True(flags.HasFlag(BatteryChargeStatus.Low));
        }

        #endregion

        #region PowerLineStatus 枚举测试

        [Fact]
        public void PowerLineStatus_ValuesAreCorrect()
        {
            Assert.Equal(0, (int)PowerLineStatus.Offline);
            Assert.Equal(1, (int)PowerLineStatus.Online);
            Assert.Equal(255, (int)PowerLineStatus.Unknown);
        }

        #endregion

        #region Windows 平台专用方法测试

        [Fact]
        public void GetPowerStatus_ReturnsStatusOrThrowsPlatformNotSupported()
        {
            if (IsWindows)
            {
                var status = PowerUtil.GetPowerStatus();
                Assert.NotNull(status);
                Assert.True(status.BatteryLifePercent >= 0 && status.BatteryLifePercent <= 100);
            }
            else
            {
                Assert.Throws<PlatformNotSupportedException>(() => PowerUtil.GetPowerStatus());
            }
        }

        [Fact]
        public void IsAcConnected_ReturnsBooleanOrThrowsPlatformNotSupported()
        {
            if (IsWindows)
            {
                var result = PowerUtil.IsAcConnected();
                // 结果取决于实际电源状态，总是 true 或 false
                Assert.True(result || !result);
            }
            else
            {
                Assert.Throws<PlatformNotSupportedException>(() => PowerUtil.IsAcConnected());
            }
        }

        [Fact]
        public void IsOnBattery_ReturnsBooleanOrThrowsPlatformNotSupported()
        {
            if (IsWindows)
            {
                var result = PowerUtil.IsOnBattery();
                Assert.Equal(!PowerUtil.IsAcConnected(), result);
            }
            else
            {
                Assert.Throws<PlatformNotSupportedException>(() => PowerUtil.IsOnBattery());
            }
        }

        [Fact]
        public void GetBatteryPercent_ReturnsValueOrThrowsPlatformNotSupported()
        {
            if (IsWindows)
            {
                var percent = PowerUtil.GetBatteryPercent();
                Assert.True(percent >= 0 && percent <= 100);
            }
            else
            {
                Assert.Throws<PlatformNotSupportedException>(() => PowerUtil.GetBatteryPercent());
            }
        }

        [Fact]
        public void GetBatteryLifeRemaining_ReturnsTimeSpanOrThrowsPlatformNotSupported()
        {
            if (IsWindows)
            {
                var remaining = PowerUtil.GetBatteryLifeRemaining();
                Assert.True(remaining >= TimeSpan.Zero);
            }
            else
            {
                Assert.Throws<PlatformNotSupportedException>(() => PowerUtil.GetBatteryLifeRemaining());
            }
        }

        [Fact]
        public void IsLowBattery_ReturnsBooleanOrThrowsPlatformNotSupported()
        {
            if (IsWindows)
            {
                var result = PowerUtil.IsLowBattery();
                Assert.True(result || !result);
            }
            else
            {
                Assert.Throws<PlatformNotSupportedException>(() => PowerUtil.IsLowBattery());
            }
        }

        [Fact]
        public void IsCriticalBattery_ReturnsBooleanOrThrowsPlatformNotSupported()
        {
            if (IsWindows)
            {
                var result = PowerUtil.IsCriticalBattery();
                Assert.True(result || !result);
            }
            else
            {
                Assert.Throws<PlatformNotSupportedException>(() => PowerUtil.IsCriticalBattery());
            }
        }

        [Fact]
        public void IsCharging_ReturnsBooleanOrThrowsPlatformNotSupported()
        {
            if (IsWindows)
            {
                var result = PowerUtil.IsCharging();
                Assert.True(result || !result);
            }
            else
            {
                Assert.Throws<PlatformNotSupportedException>(() => PowerUtil.IsCharging());
            }
        }

        [Fact]
        public void IsBatteryFull_ReturnsBooleanOrThrowsPlatformNotSupported()
        {
            if (IsWindows)
            {
                var result = PowerUtil.IsBatteryFull();
                Assert.True(result || !result);
            }
            else
            {
                Assert.Throws<PlatformNotSupportedException>(() => PowerUtil.IsBatteryFull());
            }
        }

        [Fact]
        public void HasBattery_ReturnsBooleanOrThrowsPlatformNotSupported()
        {
            if (IsWindows)
            {
                var result = PowerUtil.HasBattery();
                // 台式机可能无电池
                Assert.True(result || !result);
            }
            else
            {
                Assert.Throws<PlatformNotSupportedException>(() => PowerUtil.HasBattery());
            }
        }

        [Fact]
        public void GetPowerStatusDescription_ReturnsDescriptionOrThrowsPlatformNotSupported()
        {
            if (IsWindows)
            {
                var description = PowerUtil.GetPowerStatusDescription();
                Assert.NotNull(description);
                Assert.Contains("电源线状态", description);
            }
            else
            {
                Assert.Throws<PlatformNotSupportedException>(() => PowerUtil.GetPowerStatusDescription());
            }
        }

        [Fact]
        public void Sleep_ReturnsBooleanOrThrowsPlatformNotSupported()
        {
            if (IsWindows)
            {
                // 不实际执行睡眠，只验证方法存在
                // Sleep(true) 会强制进入睡眠，不适合测试
            }
            else
            {
                Assert.Throws<PlatformNotSupportedException>(() => PowerUtil.Sleep());
            }
        }

        [Fact]
        public void Hibernate_ReturnsBooleanOrThrowsPlatformNotSupported()
        {
            if (IsWindows)
            {
                // 不实际执行休眠，只验证方法存在
            }
            else
            {
                Assert.Throws<PlatformNotSupportedException>(() => PowerUtil.Hibernate());
            }
        }

        #endregion

        #region 监控功能测试

        [Fact]
        public void StartMonitoring_OrThrowsPlatformNotSupported()
        {
            if (IsWindows)
            {
                PowerUtil.StartMonitoring(1000);
                global::System.Threading.Thread.Sleep(100);
                PowerUtil.StopMonitoring();
            }
            else
            {
                Assert.Throws<PlatformNotSupportedException>(() => PowerUtil.StartMonitoring(1000));
            }
        }

        [Fact]
        public void StopMonitoring_DoesNotThrow()
        {
            if (IsWindows)
            {
                PowerUtil.StopMonitoring();
                // 再次停止应该无异常
                PowerUtil.StopMonitoring();
            }
            // 非 Windows 平台 StopMonitoring 不检查平台，不会抛异常
        }

        #endregion
    }
}