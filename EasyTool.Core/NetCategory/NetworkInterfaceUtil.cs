using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace EasyTool.NetCategory
{
    /// <summary>
    /// 网络接口工具类
    /// </summary>
    public static class NetworkInterfaceUtil
    {
        /// <summary>
        /// 获取所有网络接口
        /// </summary>
        public static List<NetworkInterfaceInfo> GetAllInterfaces()
        {
            var result = new List<NetworkInterfaceInfo>();
            var interfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (var ni in interfaces)
            {
                var info = new NetworkInterfaceInfo
                {
                    Id = ni.Id,
                    Name = ni.Name,
                    Description = ni.Description,
                    InterfaceType = ni.NetworkInterfaceType.ToString(),
                    OperationalStatus = ni.OperationalStatus.ToString(),
                    Speed = ni.Speed,
                    IsReceiveOnly = ni.IsReceiveOnly,
                    SupportsMulticast = ni.SupportsMulticast
                };

                // 获取MAC地址
                var mac = ni.GetPhysicalAddress();
                info.MacAddress = mac.ToString();

                // 获取IP地址
                var ipProps = ni.GetIPProperties();
                foreach (var addr in ipProps.UnicastAddresses)
                {
                    if (addr.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        info.IPv4Addresses.Add(new IPAddressInfo
                        {
                            Address = addr.Address.ToString(),
                            SubnetMask = addr.IPv4Mask?.ToString() ?? ""
                        });
                    }
                    else if (addr.Address.AddressFamily == AddressFamily.InterNetworkV6)
                    {
                        info.IPv6Addresses.Add(addr.Address.ToString());
                    }
                }

                // 获取网关
                foreach (var gateway in ipProps.GatewayAddresses)
                {
                    if (gateway.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        info.Gateway = gateway.Address.ToString();
                        break;
                    }
                }

                // 获取DNS服务器
                foreach (var dns in ipProps.DnsAddresses)
                {
                    info.DnsServers.Add(dns.ToString());
                }

                // 获取DHCP服务器
                foreach (var dhcp in ipProps.DhcpServerAddresses)
                {
                    info.DhcpServers.Add(dhcp.ToString());
                }

                // 获取统计信息
                try
                {
                    var stats = ni.GetIPv4Statistics();
                    info.BytesReceived = stats.BytesReceived;
                    info.BytesSent = stats.BytesSent;
                    info.UnicastPacketsReceived = stats.UnicastPacketsReceived;
                    info.UnicastPacketsSent = stats.UnicastPacketsSent;
                    info.NonUnicastPacketsReceived = stats.NonUnicastPacketsReceived;
                    info.NonUnicastPacketsSent = stats.NonUnicastPacketsSent;
                }
                catch
                {
                }

                result.Add(info);
            }

            return result;
        }

        /// <summary>
        /// 获取活动网络接口
        /// </summary>
        public static List<NetworkInterfaceInfo> GetActiveInterfaces()
        {
            return GetAllInterfaces()
                .Where(i => i.OperationalStatus == "Up")
                .ToList();
        }

        /// <summary>
        /// 获取本机IP地址
        /// </summary>
        public static string GetLocalIPAddress()
        {
            using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
            socket.Connect("8.8.8.8", 65530);
            var endPoint = socket.LocalEndPoint as IPEndPoint;
            return endPoint?.Address.ToString() ?? "";
        }

        /// <summary>
        /// 获取本机所有IPv4地址
        /// </summary>
        public static List<string> GetAllLocalIPv4Addresses()
        {
            var result = new List<string>();
            var host = Dns.GetHostEntry(Dns.GetHostName());

            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    result.Add(ip.ToString());
                }
            }

            return result;
        }

        /// <summary>
        /// 获取本机MAC地址
        /// </summary>
        public static string GetMacAddress()
        {
            var interfaces = NetworkInterface.GetAllNetworkInterfaces()
                .Where(ni => ni.OperationalStatus == OperationalStatus.Up)
                .OrderByDescending(ni => ni.Speed);

            foreach (var ni in interfaces)
            {
                var mac = ni.GetPhysicalAddress();
                if (!string.IsNullOrEmpty(mac.ToString()))
                {
                    return mac.ToString();
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// 获取默认网关
        /// </summary>
        public static string GetDefaultGateway()
        {
            var interfaces = NetworkInterface.GetAllNetworkInterfaces()
                .Where(ni => ni.OperationalStatus == OperationalStatus.Up);

            foreach (var ni in interfaces)
            {
                var ipProps = ni.GetIPProperties();
                foreach (var gateway in ipProps.GatewayAddresses)
                {
                    if (gateway.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        return gateway.Address.ToString();
                    }
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// 获取DNS服务器
        /// </summary>
        public static List<string> GetDnsServers()
        {
            var result = new List<string>();
            var interfaces = NetworkInterface.GetAllNetworkInterfaces()
                .Where(ni => ni.OperationalStatus == OperationalStatus.Up);

            foreach (var ni in interfaces)
            {
                var ipProps = ni.GetIPProperties();
                foreach (var dns in ipProps.DnsAddresses)
                {
                    if (!result.Contains(dns.ToString()))
                    {
                        result.Add(dns.ToString());
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 检查是否联网
        /// </summary>
        public static bool IsNetworkAvailable()
        {
            return NetworkInterface.GetIsNetworkAvailable();
        }

        /// <summary>
        /// 获取网络流量统计
        /// </summary>
        public static NetworkTrafficStats GetNetworkTrafficStats()
        {
            var stats = new NetworkTrafficStats();
            var interfaces = NetworkInterface.GetAllNetworkInterfaces()
                .Where(ni => ni.OperationalStatus == OperationalStatus.Up);

            foreach (var ni in interfaces)
            {
                try
                {
                    var ipv4Stats = ni.GetIPv4Statistics();
                    stats.TotalBytesReceived += ipv4Stats.BytesReceived;
                    stats.TotalBytesSent += ipv4Stats.BytesSent;
                    stats.TotalPacketsReceived += ipv4Stats.UnicastPacketsReceived + ipv4Stats.NonUnicastPacketsReceived;
                    stats.TotalPacketsSent += ipv4Stats.UnicastPacketsSent + ipv4Stats.NonUnicastPacketsSent;
                }
                catch
                {
                }
            }

            return stats;
        }

        /// <summary>
        /// 刷新DNS缓存
        /// </summary>
        public static bool FlushDnsCache()
        {
            try
            {
                var process = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "ipconfig",
                        Arguments = "/flushdns",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                process.WaitForExit();
                return process.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 释放并续订DHCP
        /// </summary>
        public static bool RenewDhcp()
        {
            try
            {
                var process = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "ipconfig",
                        Arguments = "/renew",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                process.WaitForExit();
                return process.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 获取主机名
        /// </summary>
        public static string GetHostName()
        {
            return Dns.GetHostName();
        }

        /// <summary>
        /// 根据主机名获取IP地址
        /// </summary>
        public static string[] GetHostAddresses(string hostName)
        {
            try
            {
                var addresses = Dns.GetHostAddresses(hostName);
                return addresses.Select(a => a.ToString()).ToArray();
            }
            catch
            {
                return Array.Empty<string>();
            }
        }
    }

    /// <summary>
    /// 网络接口信息
    /// </summary>
    public class NetworkInterfaceInfo
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string InterfaceType { get; set; } = "";
        public string OperationalStatus { get; set; } = "";
        public string MacAddress { get; set; } = "";
        public long Speed { get; set; }
        public bool IsReceiveOnly { get; set; }
        public bool SupportsMulticast { get; set; }
        public List<IPAddressInfo> IPv4Addresses { get; set; } = new();
        public List<string> IPv6Addresses { get; set; } = new();
        public string Gateway { get; set; } = "";
        public List<string> DnsServers { get; set; } = new();
        public List<string> DhcpServers { get; set; } = new();
        public long BytesReceived { get; set; }
        public long BytesSent { get; set; }
        public long UnicastPacketsReceived { get; set; }
        public long UnicastPacketsSent { get; set; }
        public long NonUnicastPacketsReceived { get; set; }
        public long NonUnicastPacketsSent { get; set; }

        public double SpeedMbps => Speed / 1_000_000.0;
        public double SpeedGbps => Speed / 1_000_000_000.0;
    }

    /// <summary>
    /// IP地址信息
    /// </summary>
    public class IPAddressInfo
    {
        public string Address { get; set; } = "";
        public string SubnetMask { get; set; } = "";
    }

    /// <summary>
    /// 网络流量统计
    /// </summary>
    public class NetworkTrafficStats
    {
        public long TotalBytesReceived { get; set; }
        public long TotalBytesSent { get; set; }
        public long TotalPacketsReceived { get; set; }
        public long TotalPacketsSent { get; set; }

        public double TotalGBReceived => TotalBytesReceived / (1024.0 * 1024 * 1024);
        public double TotalGBSent => TotalBytesSent / (1024.0 * 1024 * 1024);
    }
}
