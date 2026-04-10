using System;
using System.Text.RegularExpressions;

namespace EasyTool.NetCategory
{
    /// <summary>
    /// User-Agent 解析工具类
    /// 用于解析浏览器、操作系统、设备等信息
    /// </summary>
    public static class UserAgentUtil
    {
        #region 常见浏览器正则

        private static readonly Regex BrowserRegex = new(
            @"(Edge|Edg|OPR|Opera|Chrome|Safari|Firefox|MSIE|Trident|SamsungBrowser|UCBrowser|QQBrowser|MicroMessenger|WeChat|Alipay|WeiBo|DingTalk)[/\s]?(\d+[.\d]*)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex OsRegex = new(
            @"(Windows NT|Windows Phone|Android|iPhone|iPad|iPod|Mac OS X|Linux|Ubuntu|Fedora|FreeBSD|Chrome OS)[/\s]?(\d+[.\d]*)?",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex DeviceRegex = new(
            @"(Mobile|Android|iPhone|iPad|iPod|Tablet|Kindle|BlackBerry|PlayBook|Nokia|Samsung|HTC|Motorola|LG|Sony|Xiaomi|Huawei|OPPO|Vivo|OnePlus)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex BotRegex = new(
            @"(Googlebot|Bingbot|Slurp|DuckDuckBot|Baiduspider|YandexBot|Sogou|Exabot|facebot|facebookexternalhit|ia_archiver|Twitterbot|LinkedInBot|Embedly|Quora Link Preview|ShowyouBot|outbrain|pinterest|applebot|SemrushBot|AhrefsBot)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// 设备型号提取正则表达式
        /// </summary>
        private static readonly Regex DeviceModelRegex = new Regex(@"\(([^)]+)\)", RegexOptions.Compiled);

        #endregion

        /// <summary>
        /// 解析 User-Agent 字符串
        /// </summary>
        /// <param name="userAgent">User-Agent 字符串</param>
        /// <returns>解析结果</returns>
        public static UserAgentInfo Parse(string? userAgent)
        {
            if (string.IsNullOrWhiteSpace(userAgent))
            {
                return new UserAgentInfo
                {
                    Browser = BrowserInfo.Unknown,
                    Os = OsInfo.Unknown,
                    Device = DeviceInfo.Unknown,
                    IsBot = false
                };
            }

            return new UserAgentInfo
            {
                Browser = ParseBrowser(userAgent),
                Os = ParseOs(userAgent),
                Device = ParseDevice(userAgent),
                IsBot = IsBot(userAgent)
            };
        }

        /// <summary>
        /// 解析浏览器信息
        /// </summary>
        public static BrowserInfo ParseBrowser(string? userAgent)
        {
            if (string.IsNullOrWhiteSpace(userAgent))
                return BrowserInfo.Unknown;

            var match = BrowserRegex.Match(userAgent);
            if (!match.Success)
                return BrowserInfo.Unknown;

            var name = match.Groups[1].Value.ToLowerInvariant();
            var version = match.Groups[2].Value;

            // 规范化浏览器名称
            var browserName = name switch
            {
                "edg" or "edge" => "Edge",
                "opr" or "opera" => "Opera",
                "chrome" => "Chrome",
                "safari" => "Safari",
                "firefox" => "Firefox",
                "msie" or "trident" => "Internet Explorer",
                "samsungbrowser" => "Samsung Browser",
                "ucbrowser" => "UC Browser",
                "qqbrowser" => "QQ Browser",
                "micromessenger" or "wechat" => "WeChat",
                "alipay" => "Alipay",
                "weibo" => "Weibo",
                "dingtalk" => "DingTalk",
                _ => char.ToUpperInvariant(name[0]) + name.Substring(1)
            };

            return new BrowserInfo
            {
                Name = browserName,
                Version = version,
                VersionNumber = ParseVersionNumber(version)
            };
        }

        /// <summary>
        /// 解析操作系统信息
        /// </summary>
        public static OsInfo ParseOs(string? userAgent)
        {
            if (string.IsNullOrWhiteSpace(userAgent))
                return OsInfo.Unknown;

            var match = OsRegex.Match(userAgent);
            if (!match.Success)
                return OsInfo.Unknown;

            var name = match.Groups[1].Value.ToLowerInvariant();
            var version = match.Groups[2].Value;

            var osName = name switch
            {
                "windows nt" => ParseWindowsVersion(version),
                "windows phone" => "Windows Phone",
                "android" => "Android",
                "iphone" or "ipad" or "ipod" => "iOS",
                "mac os x" => "macOS",
                "linux" => "Linux",
                "ubuntu" => "Ubuntu",
                "fedora" => "Fedora",
                "freebsd" => "FreeBSD",
                "chrome os" => "Chrome OS",
                _ => char.ToUpperInvariant(name[0]) + name.Substring(1)
            };

            return new OsInfo
            {
                Name = osName,
                Version = version,
                VersionNumber = ParseVersionNumber(version)
            };
        }

        /// <summary>
        /// 解析设备信息
        /// </summary>
        public static DeviceInfo ParseDevice(string? userAgent)
        {
            if (string.IsNullOrWhiteSpace(userAgent))
                return DeviceInfo.Unknown;

            var deviceType = DeviceType.Desktop;
            string? vendor = null;
            string? model = null;

            // 判断设备类型
            if (userAgent.Contains("Mobile", StringComparison.OrdinalIgnoreCase) ||
                userAgent.Contains("iPhone", StringComparison.OrdinalIgnoreCase) ||
                userAgent.Contains("Android", StringComparison.OrdinalIgnoreCase) && !userAgent.Contains("Tablet", StringComparison.OrdinalIgnoreCase))
            {
                deviceType = DeviceType.Mobile;
            }
            else if (userAgent.Contains("Tablet", StringComparison.OrdinalIgnoreCase) ||
                     userAgent.Contains("iPad", StringComparison.OrdinalIgnoreCase))
            {
                deviceType = DeviceType.Tablet;
            }
            else if (userAgent.Contains("SmartTV", StringComparison.OrdinalIgnoreCase) ||
                     userAgent.Contains("TV", StringComparison.OrdinalIgnoreCase))
            {
                deviceType = DeviceType.TV;
            }

            // 提取设备厂商
            var match = DeviceRegex.Match(userAgent);
            if (match.Success)
            {
                var matched = match.Groups[1].Value.ToLowerInvariant();
                vendor = matched switch
                {
                    "iphone" or "ipad" or "ipod" => "Apple",
                    "samsung" => "Samsung",
                    "huawei" => "Huawei",
                    "xiaomi" => "Xiaomi",
                    "oppo" => "OPPO",
                    "vivo" => "Vivo",
                    "oneplus" => "OnePlus",
                    "htc" => "HTC",
                    "motorola" => "Motorola",
                    "lg" => "LG",
                    "sony" => "Sony",
                    "nokia" => "Nokia",
                    "blackberry" => "BlackBerry",
                    "kindle" => "Amazon",
                    _ => char.ToUpperInvariant(matched[0]) + matched.Substring(1)
                };
            }

            // 提取设备型号（简化处理）
            var modelMatch = DeviceModelRegex.Match(userAgent);
            if (modelMatch.Success)
            {
                var parts = modelMatch.Groups[1].Value.Split(';');
                foreach (var part in parts)
                {
                    var trimmed = part.Trim();
                    if (trimmed.Contains("Build") || trimmed.Contains(" "))
                    {
                        model = trimmed.Split(' ')[0];
                        break;
                    }
                }
            }

            return new DeviceInfo
            {
                Type = deviceType,
                Vendor = vendor,
                Model = model
            };
        }

        /// <summary>
        /// 判断是否为机器人/爬虫
        /// </summary>
        public static bool IsBot(string? userAgent)
        {
            if (string.IsNullOrWhiteSpace(userAgent))
                return false;

            return BotRegex.IsMatch(userAgent);
        }

        /// <summary>
        /// 判断是否为移动设备
        /// </summary>
        public static bool IsMobile(string? userAgent)
        {
            if (string.IsNullOrWhiteSpace(userAgent))
                return false;

            return userAgent.Contains("Mobile", StringComparison.OrdinalIgnoreCase) ||
                   userAgent.Contains("iPhone", StringComparison.OrdinalIgnoreCase) ||
                   userAgent.Contains("Android", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 判断是否为微信内置浏览器
        /// </summary>
        public static bool IsWeChat(string? userAgent)
        {
            if (string.IsNullOrWhiteSpace(userAgent))
                return false;

            return userAgent.Contains("MicroMessenger", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 判断是否为支付宝内置浏览器
        /// </summary>
        public static bool IsAlipay(string? userAgent)
        {
            if (string.IsNullOrWhiteSpace(userAgent))
                return false;

            return userAgent.Contains("Alipay", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 获取浏览器简短描述
        /// </summary>
        public static string GetBrowserDescription(string? userAgent)
        {
            var info = Parse(userAgent);
            var parts = new System.Collections.Generic.List<string>();

            if (info.Browser.Name != "Unknown")
            {
                parts.Add($"{info.Browser.Name} {info.Browser.Version}".Trim());
            }

            if (info.Os.Name != "Unknown")
            {
                parts.Add($"{info.Os.Name} {info.Os.Version}".Trim());
            }

            if (info.Device.Type != DeviceType.Desktop)
            {
                parts.Add(info.Device.Type.ToString());
            }

            return string.Join(" / ", parts);
        }

        #region 私有方法

        private static string ParseWindowsVersion(string version)
        {
            return version switch
            {
                "10.0" => "Windows 10/11",
                "6.3" => "Windows 8.1",
                "6.2" => "Windows 8",
                "6.1" => "Windows 7",
                "6.0" => "Windows Vista",
                "5.1" or "5.2" => "Windows XP",
                _ => $"Windows NT {version}"
            };
        }

        private static Version ParseVersionNumber(string version)
        {
            if (string.IsNullOrEmpty(version))
                return new Version(0, 0);

            var parts = version.Split('.');
            var major = parts.Length > 0 && int.TryParse(parts[0], out var m) ? m : 0;
            var minor = parts.Length > 1 && int.TryParse(parts[1], out var mi) ? mi : 0;
            var build = parts.Length > 2 && int.TryParse(parts[2], out var b) ? b : 0;

            return new Version(major, minor, build);
        }

        #endregion
    }

    #region 数据类

    /// <summary>
    /// User-Agent 解析结果
    /// </summary>
    public class UserAgentInfo
    {
        /// <summary>
        /// 浏览器信息
        /// </summary>
        public BrowserInfo Browser { get; set; } = BrowserInfo.Unknown;

        /// <summary>
        /// 操作系统信息
        /// </summary>
        public OsInfo Os { get; set; } = OsInfo.Unknown;

        /// <summary>
        /// 设备信息
        /// </summary>
        public DeviceInfo Device { get; set; } = DeviceInfo.Unknown;

        /// <summary>
        /// 是否为机器人/爬虫
        /// </summary>
        public bool IsBot { get; set; }
    }

    /// <summary>
    /// 浏览器信息
    /// </summary>
    public class BrowserInfo
    {
        /// <summary>
        /// 浏览器名称
        /// </summary>
        public string Name { get; set; } = "Unknown";

        /// <summary>
        /// 版本字符串
        /// </summary>
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// 版本号
        /// </summary>
        public Version VersionNumber { get; set; } = new Version(0, 0);

        public static BrowserInfo Unknown => new();

        public override string ToString() => $"{Name} {Version}".Trim();
    }

    /// <summary>
    /// 操作系统信息
    /// </summary>
    public class OsInfo
    {
        /// <summary>
        /// 操作系统名称
        /// </summary>
        public string Name { get; set; } = "Unknown";

        /// <summary>
        /// 版本字符串
        /// </summary>
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// 版本号
        /// </summary>
        public Version VersionNumber { get; set; } = new Version(0, 0);

        public static OsInfo Unknown => new();

        public override string ToString() => $"{Name} {Version}".Trim();
    }

    /// <summary>
    /// 设备信息
    /// </summary>
    public class DeviceInfo
    {
        /// <summary>
        /// 设备类型
        /// </summary>
        public DeviceType Type { get; set; } = DeviceType.Desktop;

        /// <summary>
        /// 设备厂商
        /// </summary>
        public string? Vendor { get; set; }

        /// <summary>
        /// 设备型号
        /// </summary>
        public string? Model { get; set; }

        public static DeviceInfo Unknown => new();

        public override string ToString()
        {
            var parts = new System.Collections.Generic.List<string> { Type.ToString() };
            if (!string.IsNullOrEmpty(Vendor)) parts.Add(Vendor);
            if (!string.IsNullOrEmpty(Model)) parts.Add(Model);
            return string.Join(" ", parts);
        }
    }

    /// <summary>
    /// 设备类型
    /// </summary>
    public enum DeviceType
    {
        /// <summary>
        /// 桌面设备
        /// </summary>
        Desktop,

        /// <summary>
        /// 手机
        /// </summary>
        Mobile,

        /// <summary>
        /// 平板
        /// </summary>
        Tablet,

        /// <summary>
        /// 智能电视
        /// </summary>
        TV,

        /// <summary>
        /// 其他
        /// </summary>
        Other
    }

    #endregion
}
