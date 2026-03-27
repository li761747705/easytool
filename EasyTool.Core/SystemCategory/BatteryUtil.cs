using System;
using System.Runtime.InteropServices;

namespace EasyTool.SystemCategory
{
    /// <summary>
    /// 电池工具类
    /// 提供电池状态信息查询
    /// </summary>
    public static class BatteryUtil
    {
        /// <summary>
        /// 获取电池状态
        /// </summary>
        /// <returns>电池状态信息</returns>
        public static BatteryInfo GetStatus()
        {
            var status = new BatteryInfo();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                GetWindowsBatteryStatus(status);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                GetLinuxBatteryStatus(status);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                GetMacOsBatteryStatus(status);
            }

            return status;
        }

        /// <summary>
        /// 是否使用电池供电
        /// </summary>
        /// <returns>是否使用电池</returns>
        public static bool IsOnBattery()
        {
            var status = GetStatus();
            return status.PowerSource == PowerSource.Battery;
        }

        /// <summary>
        /// 是否正在充电
        /// </summary>
        /// <returns>是否正在充电</returns>
        public static bool IsCharging()
        {
            var status = GetStatus();
            return status.ChargeState == BatteryChargeState.Charging;
        }

        /// <summary>
        /// 电池电量是否低
        /// </summary>
        /// <param name="threshold">阈值（默认 20%）</param>
        /// <returns>是否电量低</returns>
        public static bool IsLowBattery(double threshold = 20)
        {
            var status = GetStatus();
            return status.PercentRemaining <= threshold;
        }

        /// <summary>
        /// 获取剩余电量百分比
        /// </summary>
        /// <returns>剩余电量（0-100）</returns>
        public static double GetBatteryPercent()
        {
            return GetStatus().PercentRemaining;
        }

        /// <summary>
        /// 获取剩余时间
        /// </summary>
        /// <returns>剩余时间</returns>
        public static TimeSpan GetRemainingTime()
        {
            return GetStatus().RemainingTime;
        }

        #region Windows 实现

        [StructLayout(LayoutKind.Sequential)]
        private struct SYSTEM_POWER_STATUS
        {
            public byte ACLineStatus;
            public byte BatteryFlag;
            public byte BatteryLifePercent;
            public byte SystemStatusFlag;
            public int BatteryLifeTime;
            public int BatteryFullLifeTime;
        }

        [DllImport("kernel32.dll")]
        private static extern bool GetSystemPowerStatus(ref SYSTEM_POWER_STATUS lpSystemPowerStatus);

        private static void GetWindowsBatteryStatus(BatteryInfo status)
        {
            var sps = new SYSTEM_POWER_STATUS();
            if (GetSystemPowerStatus(ref sps))
            {
                status.PercentRemaining = sps.BatteryLifePercent == 255 ? 100 : sps.BatteryLifePercent;
                status.PowerSource = sps.ACLineStatus switch
                {
                    0 => PowerSource.Battery,
                    1 => PowerSource.AC,
                    _ => PowerSource.Unknown
                };

                status.ChargeState = sps.BatteryFlag switch
                {
                    1 => BatteryChargeState.Discharging,
                    2 => BatteryChargeState.Charging,
                    8 => BatteryChargeState.Charging,
                    9 => BatteryChargeState.Full,
                    _ => BatteryChargeState.Unknown
                };

                if (sps.BatteryLifeTime > 0)
                {
                    status.RemainingTime = TimeSpan.FromSeconds(sps.BatteryLifeTime);
                }

                status.IsBatteryPresent = sps.BatteryFlag != 128;
            }
        }

        #endregion

        #region Linux 实现

        private static void GetLinuxBatteryStatus(BatteryInfo status)
        {
            try
            {
                var batteryDir = "/sys/class/power_supply/BAT0";
                if (!System.IO.Directory.Exists(batteryDir))
                {
                    batteryDir = "/sys/class/power_supply/BAT1";
                }

                if (System.IO.Directory.Exists(batteryDir))
                {
                    // 读取电量百分比
                    var capacityFile = System.IO.Path.Combine(batteryDir, "capacity");
                    if (System.IO.File.Exists(capacityFile))
                    {
                        var capacity = System.IO.File.ReadAllText(capacityFile).Trim();
                        if (int.TryParse(capacity, out var percent))
                        {
                            status.PercentRemaining = percent;
                        }
                    }

                    // 读取状态
                    var statusFile = System.IO.Path.Combine(batteryDir, "status");
                    if (System.IO.File.Exists(statusFile))
                    {
                        var batteryStatus = System.IO.File.ReadAllText(statusFile).Trim();
                        status.ChargeState = batteryStatus switch
                        {
                            "Charging" => BatteryChargeState.Charging,
                            "Discharging" => BatteryChargeState.Discharging,
                            "Full" => BatteryChargeState.Full,
                            _ => BatteryChargeState.Unknown
                        };

                        status.PowerSource = batteryStatus == "Discharging"
                            ? PowerSource.Battery
                            : PowerSource.AC;
                    }

                    status.IsBatteryPresent = true;
                }
            }
            catch
            {
                // 忽略异常
            }
        }

        #endregion

        #region macOS 实现

        private static void GetMacOsBatteryStatus(BatteryInfo status)
        {
            try
            {
                var info = RunCommand("pmset", "-g batt");
                if (!string.IsNullOrEmpty(info))
                {
                    // 解析 pmset 输出
                    // 示例: -InternalBattery-0 (id=...); 100%; charging; 0:00 remaining
                    if (info.Contains("charging"))
                    {
                        status.ChargeState = BatteryChargeState.Charging;
                        status.PowerSource = PowerSource.AC;
                    }
                    else if (info.Contains("discharging"))
                    {
                        status.ChargeState = BatteryChargeState.Discharging;
                        status.PowerSource = PowerSource.Battery;
                    }
                    else if (info.Contains("charged"))
                    {
                        status.ChargeState = BatteryChargeState.Full;
                        status.PowerSource = PowerSource.AC;
                    }

                    // 提取百分比
                    var percentIndex = info.IndexOf('%');
                    if (percentIndex > 0)
                    {
                        var start = percentIndex - 1;
                        while (start > 0 && char.IsDigit(info[start - 1]))
                            start--;

                        if (int.TryParse(info.Substring(start, percentIndex - start), out var percent))
                        {
                            status.PercentRemaining = percent;
                        }
                    }

                    status.IsBatteryPresent = info.Contains("Battery");
                }
            }
            catch
            {
                // 忽略异常
            }
        }

        private static string RunCommand(string command, string args)
        {
            try
            {
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = command,
                    Arguments = args,
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                };

                using var process = System.Diagnostics.Process.Start(psi);
                if (process != null)
                {
                    var output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();
                    return output;
                }
            }
            catch { }

            return string.Empty;
        }

        #endregion
    }

    /// <summary>
    /// 电池状态信息
    /// </summary>
    public class BatteryInfo
    {
        /// <summary>
        /// 剩余电量百分比
        /// </summary>
        public double PercentRemaining { get; set; }

        /// <summary>
        /// 电源来源
        /// </summary>
        public PowerSource PowerSource { get; set; }

        /// <summary>
        /// 充电状态
        /// </summary>
        public BatteryChargeState ChargeState { get; set; }

        /// <summary>
        /// 剩余时间
        /// </summary>
        public TimeSpan RemainingTime { get; set; }

        /// <summary>
        /// 是否有电池
        /// </summary>
        public bool IsBatteryPresent { get; set; }

        /// <summary>
        /// 是否电量低（低于 20%）
        /// </summary>
        public bool IsLow => PercentRemaining <= 20;

        /// <summary>
        /// 是否充满
        /// </summary>
        public bool IsFull => PercentRemaining >= 95;

        public override string ToString()
        {
            return $"电量: {PercentRemaining:F1}%, 状态: {ChargeState}, 电源: {PowerSource}" +
                   (RemainingTime.TotalSeconds > 0 ? $", 剩余时间: {RemainingTime:hh\\:mm}" : "");
        }
    }

    /// <summary>
    /// 电源来源类型
    /// </summary>
    public enum PowerSource
    {
        /// <summary>
        /// 未知
        /// </summary>
        Unknown,

        /// <summary>
        /// 电池供电
        /// </summary>
        Battery,

        /// <summary>
        /// 交流电供电
        /// </summary>
        AC
    }

    /// <summary>
    /// 电池充电状态
    /// </summary>
    public enum BatteryChargeState
    {
        /// <summary>
        /// 未知
        /// </summary>
        Unknown,

        /// <summary>
        /// 正在充电
        /// </summary>
        Charging,

        /// <summary>
        /// 正在放电
        /// </summary>
        Discharging,

        /// <summary>
        /// 已充满
        /// </summary>
        Full
    }
}