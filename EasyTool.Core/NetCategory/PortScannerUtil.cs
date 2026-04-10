using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace EasyTool.NetCategory
{
    /// <summary>
    /// 端口扫描工具类
    /// </summary>
    public static class PortScannerUtil
    {
        /// <summary>
        /// 检查端口是否开放
        /// </summary>
        public static async Task<bool> IsPortOpenAsync(string host, int port, int timeoutMs = 1000)
        {
            try
            {
                using var client = new TcpClient();
                var connectTask = client.ConnectAsync(host, port);
                var timeoutTask = Task.Delay(timeoutMs);

                var completedTask = await Task.WhenAny(connectTask, timeoutTask).ConfigureAwait(false);
                return completedTask == connectTask && client.Connected;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 检查端口是否开放
        /// </summary>
        public static bool IsPortOpen(string host, int port, int timeoutMs = 1000)
        {
            return IsPortOpenAsync(host, port, timeoutMs).GetAwaiter().GetResult();
        }

        /// <summary>
        /// 扫描单个端口
        /// </summary>
        public static PortScanResult ScanPort(string host, int port, int timeoutMs = 1000)
        {
            var startTime = DateTime.UtcNow;
            var isOpen = IsPortOpen(host, port, timeoutMs);
            var duration = DateTime.UtcNow - startTime;

            return new PortScanResult
            {
                Host = host,
                Port = port,
                IsOpen = isOpen,
                ResponseTime = duration,
                ServiceName = GetServiceName(port)
            };
        }

        /// <summary>
        /// 扫描多个端口
        /// </summary>
        public static List<PortScanResult> ScanPorts(string host, IEnumerable<int> ports, int timeoutMs = 1000)
        {
            var results = new List<PortScanResult>();
            foreach (var port in ports)
            {
                results.Add(ScanPort(host, port, timeoutMs));
            }
            return results;
        }

        /// <summary>
        /// 异步扫描多个端口
        /// </summary>
        public static async Task<List<PortScanResult>> ScanPortsAsync(string host, IEnumerable<int> ports, int timeoutMs = 1000, int maxConcurrent = 100)
        {
            var results = new List<PortScanResult>();
            var semaphore = new SemaphoreSlim(maxConcurrent);
            var tasks = new List<Task<PortScanResult>>();

            foreach (var port in ports)
            {
                tasks.Add(ScanPortAsync(host, port, timeoutMs, semaphore));
            }

            var completedResults = await Task.WhenAll(tasks).ConfigureAwait(false);
            results.AddRange(completedResults);
            return results;
        }

        private static async Task<PortScanResult> ScanPortAsync(string host, int port, int timeoutMs, SemaphoreSlim semaphore)
        {
            await semaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                var startTime = DateTime.UtcNow;
                var isOpen = await IsPortOpenAsync(host, port, timeoutMs).ConfigureAwait(false);
                var duration = DateTime.UtcNow - startTime;

                return new PortScanResult
                {
                    Host = host,
                    Port = port,
                    IsOpen = isOpen,
                    ResponseTime = duration,
                    ServiceName = GetServiceName(port)
                };
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <summary>
        /// 扫描端口范围
        /// </summary>
        public static List<PortScanResult> ScanPortRange(string host, int startPort, int endPort, int timeoutMs = 1000)
        {
            var ports = new List<int>();
            for (int i = startPort; i <= endPort; i++)
            {
                ports.Add(i);
            }
            return ScanPorts(host, ports, timeoutMs);
        }

        /// <summary>
        /// 异步扫描端口范围
        /// </summary>
        public static Task<List<PortScanResult>> ScanPortRangeAsync(string host, int startPort, int endPort, int timeoutMs = 1000, int maxConcurrent = 100)
        {
            var ports = new List<int>();
            for (int i = startPort; i <= endPort; i++)
            {
                ports.Add(i);
            }
            return ScanPortsAsync(host, ports, timeoutMs, maxConcurrent);
        }

        /// <summary>
        /// 扫描常用端口
        /// </summary>
        public static List<PortScanResult> ScanCommonPorts(string host, int timeoutMs = 1000)
        {
            return ScanPorts(host, CommonPorts.Keys, timeoutMs);
        }

        /// <summary>
        /// 异步扫描常用端口
        /// </summary>
        public static Task<List<PortScanResult>> ScanCommonPortsAsync(string host, int timeoutMs = 1000, int maxConcurrent = 100)
        {
            return ScanPortsAsync(host, CommonPorts.Keys, timeoutMs, maxConcurrent);
        }

        /// <summary>
        /// 获取服务名称
        /// </summary>
        public static string GetServiceName(int port)
        {
            return CommonPorts.TryGetValue(port, out var name) ? name : "unknown";
        }

        /// <summary>
        /// 常用端口映射
        /// </summary>
        public static readonly Dictionary<int, string> CommonPorts = new()
        {
            { 20, "FTP Data" },
            { 21, "FTP" },
            { 22, "SSH" },
            { 23, "Telnet" },
            { 25, "SMTP" },
            { 53, "DNS" },
            { 80, "HTTP" },
            { 110, "POP3" },
            { 119, "NNTP" },
            { 123, "NTP" },
            { 135, "RPC" },
            { 137, "NetBIOS Name" },
            { 138, "NetBIOS Datagram" },
            { 139, "NetBIOS Session" },
            { 143, "IMAP" },
            { 161, "SNMP" },
            { 162, "SNMP Trap" },
            { 389, "LDAP" },
            { 443, "HTTPS" },
            { 445, "SMB" },
            { 465, "SMTPS" },
            { 514, "Syslog" },
            { 587, "SMTP(TLS)" },
            { 636, "LDAPS" },
            { 993, "IMAPS" },
            { 995, "POP3S" },
            { 1080, "SOCKS" },
            { 1433, "MSSQL" },
            { 1434, "MSSQL Monitor" },
            { 1521, "Oracle" },
            { 1723, "PPTP" },
            { 2049, "NFS" },
            { 3306, "MySQL" },
            { 3389, "RDP" },
            { 5432, "PostgreSQL" },
            { 5900, "VNC" },
            { 5901, "VNC-1" },
            { 5902, "VNC-2" },
            { 6379, "Redis" },
            { 8080, "HTTP-Alt" },
            { 8443, "HTTPS-Alt" },
            { 9000, "PHP-FPM" },
            { 9200, "Elasticsearch" },
            { 9300, "Elasticsearch Transport" },
            { 11211, "Memcached" },
            { 27017, "MongoDB" }
        };
    }

    /// <summary>
    /// 端口扫描结果
    /// </summary>
    public class PortScanResult
    {
        /// <summary>
        /// 主机
        /// </summary>
        public string Host { get; set; } = string.Empty;

        /// <summary>
        /// 端口
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 是否开放
        /// </summary>
        public bool IsOpen { get; set; }

        /// <summary>
        /// 响应时间
        /// </summary>
        public TimeSpan ResponseTime { get; set; }

        /// <summary>
        /// 服务名称
        /// </summary>
        public string ServiceName { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"{Host}:{Port} - {(IsOpen ? "Open" : "Closed")} ({ServiceName})";
        }
    }
}
