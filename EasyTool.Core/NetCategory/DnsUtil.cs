using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace EasyTool.NetCategory
{
    /// <summary>
    /// DNS 解析工具类
    /// 提供域名解析、反向解析、DNS 查询等功能
    /// </summary>
    public static class DnsUtil
    {
        #region 正向解析

        /// <summary>
        /// 解析域名获取 IP 地址
        /// </summary>
        /// <param name="hostName">主机名或域名</param>
        /// <returns>IP 地址列表</returns>
        public static string[] GetIPAddresses(string hostName)
        {
            try
            {
                var entry = Dns.GetHostEntry(hostName);
                return entry.AddressList
                    .Where(ip => ip.AddressFamily == AddressFamily.InterNetwork || ip.AddressFamily == AddressFamily.InterNetworkV6)
                    .Select(ip => ip.ToString())
                    .ToArray();
            }
            catch
            {
                return Array.Empty<string>();
            }
        }

        /// <summary>
        /// 解析域名获取 IPv4 地址
        /// </summary>
        /// <param name="hostName">主机名或域名</param>
        /// <returns>IPv4 地址列表</returns>
        public static string[] GetIPv4Addresses(string hostName)
        {
            try
            {
                var entry = Dns.GetHostEntry(hostName);
                return entry.AddressList
                    .Where(ip => ip.AddressFamily == AddressFamily.InterNetwork)
                    .Select(ip => ip.ToString())
                    .ToArray();
            }
            catch
            {
                return Array.Empty<string>();
            }
        }

        /// <summary>
        /// 解析域名获取 IPv6 地址
        /// </summary>
        /// <param name="hostName">主机名或域名</param>
        /// <returns>IPv6 地址列表</returns>
        public static string[] GetIPv6Addresses(string hostName)
        {
            try
            {
                var entry = Dns.GetHostEntry(hostName);
                return entry.AddressList
                    .Where(ip => ip.AddressFamily == AddressFamily.InterNetworkV6)
                    .Select(ip => ip.ToString())
                    .ToArray();
            }
            catch
            {
                return Array.Empty<string>();
            }
        }

        /// <summary>
        /// 获取第一个 IP 地址
        /// </summary>
        /// <param name="hostName">主机名或域名</param>
        /// <returns>IP 地址，解析失败返回 null</returns>
        public static string? GetFirstIPAddress(string hostName)
        {
            var ips = GetIPAddresses(hostName);
            return ips.Length > 0 ? ips[0] : null;
        }

        /// <summary>
        /// 异步解析域名获取 IP 地址
        /// </summary>
        /// <param name="hostName">主机名或域名</param>
        /// <returns>IP 地址列表</returns>
        public static async Task<string[]> GetIPAddressesAsync(string hostName)
        {
            try
            {
                var entry = await Dns.GetHostEntryAsync(hostName).ConfigureAwait(false);
                return entry.AddressList
                    .Where(ip => ip.AddressFamily == AddressFamily.InterNetwork || ip.AddressFamily == AddressFamily.InterNetworkV6)
                    .Select(ip => ip.ToString())
                    .ToArray();
            }
            catch
            {
                return Array.Empty<string>();
            }
        }

        #endregion

        #region 反向解析

        /// <summary>
        /// 反向解析 IP 地址获取主机名
        /// </summary>
        /// <param name="ipAddress">IP 地址</param>
        /// <returns>主机名</returns>
        public static string? GetHostName(string ipAddress)
        {
            try
            {
                var entry = Dns.GetHostEntry(ipAddress);
                return entry.HostName;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 反向解析 IP 地址获取主机名
        /// </summary>
        /// <param name="ipAddress">IP 地址对象</param>
        /// <returns>主机名</returns>
        public static string? GetHostName(IPAddress ipAddress)
        {
            try
            {
                var entry = Dns.GetHostEntry(ipAddress);
                return entry.HostName;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 异步反向解析 IP 地址获取主机名
        /// </summary>
        /// <param name="ipAddress">IP 地址</param>
        /// <returns>主机名</returns>
        public static async Task<string?> GetHostNameAsync(string ipAddress)
        {
            try
            {
                var entry = await Dns.GetHostEntryAsync(ipAddress).ConfigureAwait(false);
                return entry.HostName;
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region 本机信息

        /// <summary>
        /// 获取本机主机名
        /// </summary>
        /// <returns>主机名</returns>
        public static string GetLocalHostName()
        {
            return Dns.GetHostName();
        }

        /// <summary>
        /// 获取本机 IP 地址
        /// </summary>
        /// <returns>IP 地址列表</returns>
        public static string[] GetLocalIPAddresses()
        {
            return GetIPAddresses(Dns.GetHostName());
        }

        /// <summary>
        /// 获取本机 IPv4 地址
        /// </summary>
        /// <returns>IPv4 地址列表</returns>
        public static string[] GetLocalIPv4Addresses()
        {
            return GetIPv4Addresses(Dns.GetHostName());
        }

        /// <summary>
        /// 获取本机主要 IP 地址（优先返回内网地址）
        /// </summary>
        /// <returns>IP 地址</returns>
        public static string? GetLocalMainIPAddress()
        {
            var hostName = Dns.GetHostName();
            var entry = Dns.GetHostEntry(hostName);

            // 优先返回非回环、非链路本地地址
            var ip = entry.AddressList
                .Where(a => a.AddressFamily == AddressFamily.InterNetwork)
                .FirstOrDefault(a => !IPAddress.IsLoopback(a) && !IsLinkLocal(a));

            return ip?.ToString();
        }

        private static bool IsLinkLocal(IPAddress ip)
        {
            if (ip.AddressFamily != AddressFamily.InterNetwork)
                return false;

            var bytes = ip.GetAddressBytes();
            return bytes[0] == 169 && bytes[1] == 254; // 169.254.x.x
        }

        #endregion

        #region DNS 记录查询

        /// <summary>
        /// 获取 MX 记录（邮件交换记录）
        /// 注意：需要使用外部库或自定义实现，这里返回空
        /// </summary>
        /// <param name="domain">域名</param>
        /// <returns>MX 记录列表</returns>
        public static List<MxRecord> GetMxRecords(string domain)
        {
            // 简化实现，实际需要使用 DnsClient 等库
            return new List<MxRecord>();
        }

        /// <summary>
        /// 获取 TXT 记录
        /// </summary>
        /// <param name="domain">域名</param>
        /// <returns>TXT 记录列表</returns>
        public static List<string> GetTxtRecords(string domain)
        {
            // 简化实现
            return new List<string>();
        }

        /// <summary>
        /// 获取 CNAME 记录
        /// </summary>
        /// <param name="domain">域名</param>
        /// <returns>CNAME 记录</returns>
        public static string? GetCnameRecord(string domain)
        {
            // 简化实现
            return null;
        }

        #endregion

        #region 验证方法

        /// <summary>
        /// 验证域名是否可以解析
        /// </summary>
        /// <param name="hostName">主机名或域名</param>
        /// <returns>是否可以解析</returns>
        public static bool CanResolve(string hostName)
        {
            try
            {
                var entry = Dns.GetHostEntry(hostName);
                return entry.AddressList.Length > 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 异步验证域名是否可以解析
        /// </summary>
        /// <param name="hostName">主机名或域名</param>
        /// <returns>是否可以解析</returns>
        public static async Task<bool> CanResolveAsync(string hostName)
        {
            try
            {
                var entry = await Dns.GetHostEntryAsync(hostName).ConfigureAwait(false);
                return entry.AddressList.Length > 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 验证 IP 地址格式是否有效
        /// </summary>
        /// <param name="ipString">IP 地址字符串</param>
        /// <returns>是否有效</returns>
        public static bool IsValidIPAddress(string ipString)
        {
            return IPAddress.TryParse(ipString, out _);
        }

        /// <summary>
        /// 验证是否为 IPv4 地址
        /// </summary>
        /// <param name="ipString">IP 地址字符串</param>
        /// <returns>是否为 IPv4</returns>
        public static bool IsIPv4(string ipString)
        {
            if (IPAddress.TryParse(ipString, out var ip))
            {
                return ip.AddressFamily == AddressFamily.InterNetwork;
            }
            return false;
        }

        /// <summary>
        /// 验证是否为 IPv6 地址
        /// </summary>
        /// <param name="ipString">IP 地址字符串</param>
        /// <returns>是否为 IPv6</returns>
        public static bool IsIPv6(string ipString)
        {
            if (IPAddress.TryParse(ipString, out var ip))
            {
                return ip.AddressFamily == AddressFamily.InterNetworkV6;
            }
            return false;
        }

        /// <summary>
        /// 验证是否为内网 IP 地址
        /// </summary>
        /// <param name="ipString">IP 地址字符串</param>
        /// <returns>是否为内网 IP</returns>
        public static bool IsPrivateIP(string ipString)
        {
            if (!IPAddress.TryParse(ipString, out var ip))
                return false;

            if (ip.AddressFamily != AddressFamily.InterNetwork)
                return false;

            var bytes = ip.GetAddressBytes();

            // 10.0.0.0 - 10.255.255.255
            if (bytes[0] == 10)
                return true;

            // 172.16.0.0 - 172.31.255.255
            if (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31)
                return true;

            // 192.168.0.0 - 192.168.255.255
            if (bytes[0] == 192 && bytes[1] == 168)
                return true;

            return false;
        }

        /// <summary>
        /// 验证是否为回环地址
        /// </summary>
        /// <param name="ipString">IP 地址字符串</param>
        /// <returns>是否为回环地址</returns>
        public static bool IsLoopback(string ipString)
        {
            if (IPAddress.TryParse(ipString, out var ip))
            {
                return IPAddress.IsLoopback(ip);
            }
            return false;
        }

        #endregion

        #region 工具方法

        /// <summary>
        /// 将 IP 地址转换为长整型
        /// </summary>
        /// <param name="ipString">IP 地址字符串</param>
        /// <returns>长整型值</returns>
        public static long IPToLong(string ipString)
        {
            if (!IPAddress.TryParse(ipString, out var ip))
                return 0;

            var bytes = ip.GetAddressBytes();
            if (bytes.Length != 4)
                return 0;

            return ((long)bytes[0] << 24) | ((long)bytes[1] << 16) | ((long)bytes[2] << 8) | bytes[3];
        }

        /// <summary>
        /// 将长整型转换为 IP 地址
        /// </summary>
        /// <param name="ipLong">长整型值</param>
        /// <returns>IP 地址字符串</returns>
        public static string LongToIP(long ipLong)
        {
            return $"{(ipLong >> 24) & 0xFF}.{(ipLong >> 16) & 0xFF}.{(ipLong >> 8) & 0xFF}.{ipLong & 0xFF}";
        }

        /// <summary>
        /// 获取 IP 地址类型描述
        /// </summary>
        /// <param name="ipString">IP 地址字符串</param>
        /// <returns>类型描述</returns>
        public static string GetIPType(string ipString)
        {
            if (!IPAddress.TryParse(ipString, out var ip))
                return "无效IP";

            if (IPAddress.IsLoopback(ip))
                return "回环地址";

            if (IsPrivateIP(ipString))
                return "内网地址";

            return "公网地址";
        }

        #endregion
    }

    /// <summary>
    /// MX 记录
    /// </summary>
    public class MxRecord
    {
        /// <summary>
        /// 优先级
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// 邮件服务器地址
        /// </summary>
        public string? Exchange { get; set; }

        public override string ToString()
        {
            return $"[{Priority}] {Exchange}";
        }
    }
}
