using System;
using System.Text.RegularExpressions;

namespace EasyTool.BusinessCategory
{
    /// <summary>
    /// IPv6 地址工具类
    /// 用于验证和处理 IPv6 地址
    /// </summary>
    public static class IPv6Util
    {
        /// <summary>
        /// IPv6 正则表达式
        /// </summary>
        private static readonly Regex IPv6Regex = new(
            @"^(([0-9a-fA-F]{1,4}:){7}[0-9a-fA-F]{1,4}|([0-9a-fA-F]{1,4}:){1,7}:|([0-9a-fA-F]{1,4}:){1,6}:[0-9a-fA-F]{1,4}|([0-9a-fA-F]{1,4}:){1,5}(:[0-9a-fA-F]{1,4}){1,2}|([0-9a-fA-F]{1,4}:){1,4}(:[0-9a-fA-F]{1,4}){1,3}|([0-9a-fA-F]{1,4}:){1,3}(:[0-9a-fA-F]{1,4}){1,4}|([0-9a-fA-F]{1,4}:){1,2}(:[0-9a-fA-F]{1,4}){1,5}|[0-9a-fA-F]{1,4}:((:[0-9a-fA-F]{1,4}){1,6})|:((:[0-9a-fA-F]{1,4}){1,7}|:)|fe80:(:[0-9a-fA-F]{0,4}){0,4}%[0-9a-zA-Z]+|::(ffff(:0{1,4})?:)?((25[0-5]|(2[0-4]|1?[0-9])?[0-9])\.){3}(25[0-5]|(2[0-4]|1?[0-9])?[0-9])|([0-9a-fA-F]{1,4}:){1,4}:((25[0-5]|(2[0-4]|1?[0-9])?[0-9])\.){3}(25[0-5]|(2[0-4]|1?[0-9])?[0-9]))$",
            RegexOptions.Compiled);

        /// <summary>
        /// 验证是否为有效的 IPv6 地址
        /// </summary>
        /// <param name="address">IP 地址</param>
        /// <returns>是否有效</returns>
        public static bool IsValid(string? address)
        {
            if (string.IsNullOrWhiteSpace(address))
                return false;

            return IPv6Regex.IsMatch(address.Trim());
        }

        /// <summary>
        /// 压缩 IPv6 地址（移除前导零和连续零块）
        /// </summary>
        /// <param name="address">IPv6 地址</param>
        /// <returns>压缩后的地址</returns>
        public static string Compress(string address)
        {
            if (!IsValid(address))
                throw new ArgumentException("无效的 IPv6 地址", nameof(address));

            try
            {
                var ip = System.Net.IPAddress.Parse(address);
                return ip.IsIPv6LinkLocal ? address : ip.ToString();
            }
            catch
            {
                return address;
            }
        }

        /// <summary>
        /// 展开 IPv6 地址（补全省略的零）
        /// </summary>
        /// <param name="address">IPv6 地址</param>
        /// <returns>展开后的地址</returns>
        public static string Expand(string address)
        {
            if (!IsValid(address))
                throw new ArgumentException("无效的 IPv6 地址", nameof(address));

            try
            {
                var ip = System.Net.IPAddress.Parse(address);
                var bytes = ip.GetAddressBytes();

                var result = new System.Text.StringBuilder();
                for (int i = 0; i < 16; i += 2)
                {
                    if (i > 0) result.Append(':');
                    result.Append($"{bytes[i]:x2}{bytes[i + 1]:x2}");
                }

                return result.ToString();
            }
            catch
            {
                return address;
            }
        }

        /// <summary>
        /// 判断是否为本地链接地址（fe80::/10）
        /// </summary>
        /// <param name="address">IPv6 地址</param>
        /// <returns>是否为本地链接地址</returns>
        public static bool IsLinkLocal(string? address)
        {
            if (!IsValid(address))
                return false;

            try
            {
                var ip = System.Net.IPAddress.Parse(address);
                return ip.IsIPv6LinkLocal;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 判断是否为回环地址（::1）
        /// </summary>
        /// <param name="address">IPv6 地址</param>
        /// <returns>是否为回环地址</returns>
        public static bool IsLoopback(string? address)
        {
            if (!IsValid(address))
                return false;

            return System.Net.IPAddress.TryParse(address, out var ip) &&
                   System.Net.IPAddress.IsLoopback(ip);
        }

        /// <summary>
        /// 判断是否为私有地址
        /// fc00::/7 (Unique Local Address)
        /// </summary>
        /// <param name="address">IPv6 地址</param>
        /// <returns>是否为私有地址</returns>
        public static bool IsPrivate(string? address)
        {
            if (!IsValid(address))
                return false;

            var expanded = Expand(address).Replace(":", "").ToLower();
            return expanded.StartsWith("fc") || expanded.StartsWith("fd");
        }

        /// <summary>
        /// 判断是否为多播地址（ff00::/8）
        /// </summary>
        /// <param name="address">IPv6 地址</param>
        /// <returns>是否为多播地址</returns>
        public static bool IsMulticast(string? address)
        {
            if (!IsValid(address))
                return false;

            return address!.TrimStart().StartsWith("ff", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// IPv4 映射的 IPv6 地址转换为 IPv4
        /// </summary>
        /// <param name="address">IPv6 地址（::ffff:192.168.1.1 格式）</param>
        /// <returns>IPv4 地址</returns>
        public static string? ToIPv4(string? address)
        {
            if (!IsValid(address))
                return null;

            try
            {
                var ip = System.Net.IPAddress.Parse(address);
                if (ip.IsIPv4MappedToIPv6)
                {
                    return ip.MapToIPv4().ToString();
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// IPv4 转换为 IPv6 映射地址
        /// </summary>
        /// <param name="ipv4Address">IPv4 地址</param>
        /// <returns>IPv6 映射地址</returns>
        public static string? FromIPv4(string? ipv4Address)
        {
            if (string.IsNullOrWhiteSpace(ipv4Address))
                return null;

            try
            {
                var ip = System.Net.IPAddress.Parse(ipv4Address);
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return ip.MapToIPv6().ToString();
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 获取 IPv6 地址类型
        /// </summary>
        /// <param name="address">IPv6 地址</param>
        /// <returns>地址类型</returns>
        public static IPv6AddressType GetAddressType(string? address)
        {
            if (!IsValid(address))
                return IPv6AddressType.Unknown;

            if (IsLoopback(address))
                return IPv6AddressType.Loopback;

            if (IsLinkLocal(address))
                return IPv6AddressType.LinkLocal;

            if (IsPrivate(address))
                return IPv6AddressType.UniqueLocal;

            if (IsMulticast(address))
                return IPv6AddressType.Multicast;

            if (address!.StartsWith("2", StringComparison.OrdinalIgnoreCase) ||
                address.StartsWith("3", StringComparison.OrdinalIgnoreCase))
                return IPv6AddressType.GlobalUnicast;

            if (address.StartsWith("::", StringComparison.Ordinal))
                return IPv6AddressType.Unspecified;

            return IPv6AddressType.GlobalUnicast;
        }
    }

    /// <summary>
    /// IPv6 地址类型
    /// </summary>
    public enum IPv6AddressType
    {
        /// <summary>
        /// 未知
        /// </summary>
        Unknown,

        /// <summary>
        /// 未指定地址（::）
        /// </summary>
        Unspecified,

        /// <summary>
        /// 回环地址（::1）
        /// </summary>
        Loopback,

        /// <summary>
        /// 本地链接地址（fe80::/10）
        /// </summary>
        LinkLocal,

        /// <summary>
        /// 唯一本地地址（fc00::/7）
        /// </summary>
        UniqueLocal,

        /// <summary>
        /// 全球单播地址
        /// </summary>
        GlobalUnicast,

        /// <summary>
        /// 多播地址（ff00::/8）
        /// </summary>
        Multicast
    }
}
