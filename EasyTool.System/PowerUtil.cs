using System;
using System.Runtime.InteropServices;

namespace EasyTool.System
{
    /// <summary>
    /// 电源状态
    /// </summary>
    public class PowerStatus
    {
        /// <summary>
        /// 是否在使用交流电源
        /// </summary>
        public bool IsAcConnected { get; set; }

        /// <summary>
        /// 电池充电状态
        /// </summary>
        public BatteryChargeStatus BatteryChargeStatus { get; set; }

        /// <summary>
        /// 电池剩余电量百分比（0-100）
        /// </summary>
        public int BatteryLifePercent { get; set; }

        /// <summary>
        /// 电池剩余时间（秒）
        /// </summary>
        public int BatteryLifeRemaining { get; set; }

        /// <summary>
        /// 电池充满时间（秒）
        /// </summary>
        public int BatteryFullLifeTime { get; set; }

        /// <summary>
        /// 电源线状态
        /// </summary>
        public PowerLineStatus PowerLineStatus { get; set; }

        public override string ToString()
        {
            return $"电源状态: {(IsAcConnected ? "交流电源" : "电池")}, 电量: {BatteryLifePercent}%, 剩余时间: {BatteryLifeRemaining}s";
        }
    }

    /// <summary>
    /// 电池充电状态
    /// </summary>
    [Flags]
    public enum BatteryChargeStatus
    {
        /// <summary>
        /// 充电状态未知
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// 正在充电
        /// </summary>
        Charging = 1,

        /// <summary>
        /// 未充电
        /// </summary>
        NoCharging = 2,

        /// <summary>
        /// 电量低
        /// </summary>
        Low = 4,

        /// <summary>
        /// 电量严重不足
        /// </summary>
        Critical = 8,

        /// <summary>
        /// 无电池
        /// </summary>
        NoBattery = 128,

        /// <summary>
        /// 电池已充满
        /// </summary>
        Full = 255
    }

    /// <summary>
    /// 电源线状态
    /// </summary>
    public enum PowerLineStatus
    {
        /// <summary>
        /// 离线（电池供电）
        /// </summary>
        Offline = 0,

        /// <summary>
        /// 在线（交流电源）
        /// </summary>
        Online = 1,

        /// <summary>
        /// 未知
        /// </summary>
        Unknown = 255
    }

    /// <summary>
    /// 电源管理工具类
    /// </summary>
    public static class PowerUtil
    {
        #region Windows API

        [StructLayout(LayoutKind.Sequential)]
        private struct SYSTEM_POWER_STATUS
        {
            public byte ACLineStatus;
            public byte BatteryFlag;
            public byte BatteryLifePercent;
            public byte SystemStatusFlag;
            public uint BatteryLifeTime;
            public uint BatteryFullLifeTime;
        }

        [DllImport("kernel32.dll")]
        private static extern bool GetSystemPowerStatus(ref SYSTEM_POWER_STATUS lpSystemPowerStatus);

        [DllImport("kernel32.dll")]
        private static extern bool SetSystemPowerState(bool hibernate, bool force);

        [DllImport("kernel32.dll")]
        private static extern bool SetSuspendState(bool hibernate, bool forceCritical, bool disableWakeEvent);

        [DllImport("powrprof.dll")]
        private static extern bool SetSuspendState2(bool hibernate, bool force, bool disableWakeEvent);

        #endregion

        /// <summary>
        /// 获取电源状态
        /// </summary>
        /// <returns>电源状态信息</returns>
        public static PowerStatus GetPowerStatus()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                throw new PlatformNotSupportedException("此功能仅支持 Windows 平台");
            }

            var status = new SYSTEM_POWER_STATUS();
            GetSystemPowerStatus(ref status);

            return new PowerStatus
            {
                IsAcConnected = status.ACLineStatus == 1,
                BatteryChargeStatus = (BatteryChargeStatus)status.BatteryFlag,
                BatteryLifePercent = status.BatteryLifePercent > 100 ? 100 : status.BatteryLifePercent,
                BatteryLifeRemaining = (int)status.BatteryLifeTime,
                BatteryFullLifeTime = (int)status.BatteryFullLifeTime,
                PowerLineStatus = (PowerLineStatus)status.ACLineStatus
            };
        }

        /// <summary>
        /// 是否使用交流电源
        /// </summary>
        /// <returns>true表示使用交流电源</returns>
        public static bool IsAcConnected()
        {
            var status = GetPowerStatus();
            return status.IsAcConnected;
        }

        /// <summary>
        /// 是否使用电池
        /// </summary>
        /// <returns>true表示使用电池</returns>
        public static bool IsOnBattery()
        {
            return !IsAcConnected();
        }

        /// <summary>
        /// 获取电池电量百分比
        /// </summary>
        /// <returns>电量百分比（0-100），无电池返回-1</returns>
        public static int GetBatteryPercent()
        {
            var status = GetPowerStatus();
            return status.BatteryLifePercent;
        }

        /// <summary>
        /// 获取电池剩余时间
        /// </summary>
        /// <returns>剩余时间，未知返回TimeSpan.Zero</returns>
        public static TimeSpan GetBatteryLifeRemaining()
        {
            var status = GetPowerStatus();
            return status.BatteryLifeRemaining > 0 
                ? TimeSpan.FromSeconds(status.BatteryLifeRemaining) 
                : TimeSpan.Zero;
        }

        /// <summary>
        /// 是否电量低
        /// </summary>
        /// <param name="threshold">阈值百分比（默认20%）</param>
        /// <returns>true表示电量低</returns>
        public static bool IsLowBattery(int threshold = 20)
        {
            var percent = GetBatteryPercent();
            return percent > 0 && percent <= threshold && IsOnBattery();
        }

        /// <summary>
        /// 是否电量严重不足
        /// </summary>
        /// <param name="threshold">阈值百分比（默认10%）</param>
        /// <returns>true表示电量严重不足</returns>
        public static bool IsCriticalBattery(int threshold = 10)
        {
            var percent = GetBatteryPercent();
            return percent > 0 && percent <= threshold && IsOnBattery();
        }

        /// <summary>
        /// 是否正在充电
        /// </summary>
        /// <returns>true表示正在充电</returns>
        public static bool IsCharging()
        {
            var status = GetPowerStatus();
            return status.BatteryChargeStatus.HasFlag(BatteryChargeStatus.Charging);
        }

        /// <summary>
        /// 是否电池已充满
        /// </summary>
        /// <returns>true表示电池已充满</returns>
        public static bool IsBatteryFull()
        {
            var status = GetPowerStatus();
            return status.BatteryLifePercent >= 100;
        }

        /// <summary>
        /// 是否有电池
        /// </summary>
        /// <returns>true表示有电池</returns>
        public static bool HasBattery()
        {
            var status = GetPowerStatus();
            return !status.BatteryChargeStatus.HasFlag(BatteryChargeStatus.NoBattery);
        }

        /// <summary>
        /// 使系统进入睡眠状态
        /// </summary>
        /// <param name="force">是否强制进入</param>
        /// <returns>是否成功</returns>
        public static bool Sleep(bool force = false)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                throw new PlatformNotSupportedException("此功能仅支持 Windows 平台");
            }

            try
            {
                return SetSuspendState(false, force, false);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 使系统进入休眠状态
        /// </summary>
        /// <param name="force">是否强制进入</param>
        /// <returns>是否成功</returns>
        public static bool Hibernate(bool force = false)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                throw new PlatformNotSupportedException("此功能仅支持 Windows 平台");
            }

            try
            {
                return SetSuspendState(true, force, false);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 获取电源状态描述
        /// </summary>
        /// <returns>电源状态描述字符串</returns>
        public static string GetPowerStatusDescription()
        {
            var status = GetPowerStatus();
            var sb = new global::System.Text.StringBuilder();

            sb.AppendLine($"电源线状态: {status.PowerLineStatus}");
            sb.AppendLine($"是否使用交流电源: {(status.IsAcConnected ? "是" : "否")}");

            if (HasBattery())
            {
                sb.AppendLine($"电池电量: {status.BatteryLifePercent}%");
                sb.AppendLine($"充电状态: {status.BatteryChargeStatus}");

                if (status.BatteryLifeRemaining > 0)
                {
                    var time = TimeSpan.FromSeconds(status.BatteryLifeRemaining);
                    sb.AppendLine($"剩余时间: {time.Hours}小时{time.Minutes}分钟");
                }
            }
            else
            {
                sb.AppendLine("无电池");
            }

            return sb.ToString();
        }

        /// <summary>
        /// 监听电源状态变化
        /// </summary>
        public static event Action<PowerStatus>? PowerStatusChanged;

        private static global::System.Threading.Timer? _monitorTimer;
        private static PowerStatus? _lastStatus;

        /// <summary>
        /// 开始监控电源状态
        /// </summary>
        /// <param name="interval">检查间隔（毫秒）</param>
        public static void StartMonitoring(int interval = 5000)
        {
            _lastStatus = GetPowerStatus();
            _monitorTimer = new global::System.Threading.Timer(_ =>
            {
                var currentStatus = GetPowerStatus();
                if (HasPowerStatusChanged(_lastStatus, currentStatus))
                {
                    PowerStatusChanged?.Invoke(currentStatus);
                    _lastStatus = currentStatus;
                }
            }, null, interval, interval);
        }

        /// <summary>
        /// 停止监控电源状态
        /// </summary>
        public static void StopMonitoring()
        {
            _monitorTimer?.Dispose();
            _monitorTimer = null;
        }

        private static bool HasPowerStatusChanged(PowerStatus? old, PowerStatus current)
        {
            if (old == null) return true;

            return old.IsAcConnected != current.IsAcConnected ||
                   old.BatteryLifePercent != current.BatteryLifePercent ||
                   old.BatteryChargeStatus != current.BatteryChargeStatus;
        }
    }
}
