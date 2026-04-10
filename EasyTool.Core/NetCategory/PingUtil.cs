using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace EasyTool.NetCategory
{
    /// <summary>
    /// Ping结果
    /// </summary>
    public class PingResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 目标地址
        /// </summary>
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// 解析后的IP地址
        /// </summary>
        public IPAddress? IpAddress { get; set; }

        /// <summary>
        /// 响应时间（毫秒）
        /// </summary>
        public long RoundtripTime { get; set; }

        /// <summary>
        /// TTL（生存时间）
        /// </summary>
        public int Ttl { get; set; }

        /// <summary>
        /// 缓冲区大小
        /// </summary>
        public int BufferSize { get; set; }

        /// <summary>
        /// IP状态
        /// </summary>
        public IPStatus Status { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 时间戳
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public override string ToString()
        {
            return Success
                ? $"回复来自 {IpAddress}: 字节={BufferSize} 时间={RoundtripTime}ms TTL={Ttl}"
                : $"请求超时: {Status}";
        }
    }

    /// <summary>
    /// Ping统计信息
    /// </summary>
    public class PingStatistics
    {
        /// <summary>
        /// 目标地址
        /// </summary>
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// 总发包数
        /// </summary>
        public int PacketsSent { get; set; }

        /// <summary>
        /// 收包数
        /// </summary>
        public int PacketsReceived { get; set; }

        /// <summary>
        /// 丢包数
        /// </summary>
        public int PacketsLost => PacketsSent - PacketsReceived;

        /// <summary>
        /// 丢包率
        /// </summary>
        public double LossRate => PacketsSent > 0 ? (double)PacketsLost / PacketsSent : 0;

        /// <summary>
        /// 最小延迟（毫秒）
        /// </summary>
        public long MinRoundtripTime { get; set; }

        /// <summary>
        /// 最大延迟（毫秒）
        /// </summary>
        public long MaxRoundtripTime { get; set; }

        /// <summary>
        /// 平均延迟（毫秒）
        /// </summary>
        public double AverageRoundtripTime { get; set; }

        /// <summary>
        /// 结果列表
        /// </summary>
        public List<PingResult> Results { get; set; } = new();

        public override string ToString()
        {
            return $"Ping {Address}: 已发送={PacketsSent}, 已接收={PacketsReceived}, 丢失={PacketsLost}({LossRate:P0}丢失), " +
                   $"延迟: 最小={MinRoundtripTime}ms, 最大={MaxRoundtripTime}ms, 平均={AverageRoundtripTime:F2}ms";
        }
    }

    /// <summary>
    /// Ping配置
    /// </summary>
    public class PingOptions
    {
        /// <summary>
        /// 超时时间（毫秒）
        /// </summary>
        public int Timeout { get; set; } = 5000;

        /// <summary>
        /// 缓冲区大小
        /// </summary>
        public int BufferSize { get; set; } = 32;

        /// <summary>
        /// TTL
        /// </summary>
        public int Ttl { get; set; } = 128;

        /// <summary>
        /// 是否允许分片
        /// </summary>
        public bool DontFragment { get; set; } = true;

        /// <summary>
        /// 发送次数
        /// </summary>
        public int Count { get; set; } = 4;

        /// <summary>
        /// 发送间隔（毫秒）
        /// </summary>
        public int Interval { get; set; } = 1000;
    }

    /// <summary>
    /// Ping工具类
    /// </summary>
    public static class PingUtil
    {
        /// <summary>
        /// Ping指定主机
        /// </summary>
        /// <param name="hostNameOrAddress">主机名或IP地址</param>
        /// <param name="timeout">超时时间（毫秒）</param>
        /// <returns>Ping结果</returns>
        public static PingResult Ping(string hostNameOrAddress, int timeout = 5000)
        {
            return PingAsync(hostNameOrAddress, timeout).GetAwaiter().GetResult();
        }

        /// <summary>
        /// 异步Ping指定主机
        /// </summary>
        /// <param name="hostNameOrAddress">主机名或IP地址</param>
        /// <param name="timeout">超时时间（毫秒）</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>Ping结果</returns>
        public static async Task<PingResult> PingAsync(string hostNameOrAddress, int timeout = 5000, CancellationToken cancellationToken = default)
        {
            var result = new PingResult
            {
                Address = hostNameOrAddress,
                Timestamp = DateTime.UtcNow
            };

            try
            {
                // 解析IP地址
                IPAddress ipAddress;
                if (IPAddress.TryParse(hostNameOrAddress, out var parsedIp))
                {
                    ipAddress = parsedIp;
                }
                else
                {
#if NET5_0_OR_GREATER
                    var addresses = await Dns.GetHostAddressesAsync(hostNameOrAddress, cancellationToken).ConfigureAwait(false);
#else
                    var addresses = await Dns.GetHostAddressesAsync(hostNameOrAddress).ConfigureAwait(false);
#endif
                    if (addresses.Length == 0)
                    {
                        result.Status = IPStatus.Unknown;
                        result.ErrorMessage = "无法解析主机名";
                        return result;
                    }
                    ipAddress = addresses[0];
                }

                result.IpAddress = ipAddress;

                using var ping = new System.Net.NetworkInformation.Ping();
                var buffer = new byte[32];
                for (int i = 0; i < buffer.Length; i++)
                {
                    buffer[i] = (byte)('a' + (i % 26));
                }

                var options = new System.Net.NetworkInformation.PingOptions(128, true);
                var reply = await ping.SendPingAsync(ipAddress, timeout, buffer, options).ConfigureAwait(false);

                result.Status = reply.Status;
                result.Success = reply.Status == IPStatus.Success;

                if (reply.Status == IPStatus.Success)
                {
                    result.RoundtripTime = reply.RoundtripTime;
                    result.Ttl = reply.Options?.Ttl ?? 0;
                    result.BufferSize = reply.Buffer.Length;
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
                result.Status = IPStatus.Unknown;
            }

            return result;
        }

        /// <summary>
        /// Ping指定主机（带完整配置）
        /// </summary>
        /// <param name="hostNameOrAddress">主机名或IP地址</param>
        /// <param name="options">配置</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>Ping结果</returns>
        public static async Task<PingResult> PingAsync(string hostNameOrAddress, PingOptions options, CancellationToken cancellationToken = default)
        {
            var result = new PingResult
            {
                Address = hostNameOrAddress,
                Timestamp = DateTime.UtcNow
            };

            try
            {
                IPAddress ipAddress;
                if (IPAddress.TryParse(hostNameOrAddress, out var parsedIp))
                {
                    ipAddress = parsedIp;
                }
                else
                {
#if NET5_0_OR_GREATER
                    var addresses = await Dns.GetHostAddressesAsync(hostNameOrAddress, cancellationToken).ConfigureAwait(false);
#else
                    var addresses = await Dns.GetHostAddressesAsync(hostNameOrAddress).ConfigureAwait(false);
#endif
                    if (addresses.Length == 0)
                    {
                        result.Status = IPStatus.Unknown;
                        result.ErrorMessage = "无法解析主机名";
                        return result;
                    }
                    ipAddress = addresses[0];
                }

                result.IpAddress = ipAddress;

                using var ping = new System.Net.NetworkInformation.Ping();
                var buffer = new byte[options.BufferSize];
                for (int i = 0; i < buffer.Length; i++)
                {
                    buffer[i] = (byte)('a' + (i % 26));
                }

                var pingOptions = new System.Net.NetworkInformation.PingOptions(options.Ttl, options.DontFragment);
                var reply = await ping.SendPingAsync(ipAddress, options.Timeout, buffer, pingOptions).ConfigureAwait(false);

                result.Status = reply.Status;
                result.Success = reply.Status == IPStatus.Success;

                if (reply.Status == IPStatus.Success)
                {
                    result.RoundtripTime = reply.RoundtripTime;
                    result.Ttl = reply.Options?.Ttl ?? 0;
                    result.BufferSize = reply.Buffer.Length;
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
                result.Status = IPStatus.Unknown;
            }

            return result;
        }

        /// <summary>
        /// 持续Ping指定主机
        /// </summary>
        /// <param name="hostNameOrAddress">主机名或IP地址</param>
        /// <param name="options">配置</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>Ping统计信息</returns>
        public static async Task<PingStatistics> PingContinuousAsync(string hostNameOrAddress, PingOptions? options = null, CancellationToken cancellationToken = default)
        {
            options ??= new PingOptions();
            var stats = new PingStatistics { Address = hostNameOrAddress };

            long totalTime = 0;
            long minTime = long.MaxValue;
            long maxTime = long.MinValue;

            for (int i = 0; i < options.Count; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                var result = await PingAsync(hostNameOrAddress, options, cancellationToken).ConfigureAwait(false);
                stats.Results.Add(result);
                stats.PacketsSent++;

                if (result.Success)
                {
                    stats.PacketsReceived++;
                    totalTime += result.RoundtripTime;

                    if (result.RoundtripTime < minTime)
                        minTime = result.RoundtripTime;

                    if (result.RoundtripTime > maxTime)
                        maxTime = result.RoundtripTime;
                }

                if (i < options.Count - 1)
                {
                    await Task.Delay(options.Interval, cancellationToken).ConfigureAwait(false);
                }
            }

            if (stats.PacketsReceived > 0)
            {
                stats.MinRoundtripTime = minTime;
                stats.MaxRoundtripTime = maxTime;
                stats.AverageRoundtripTime = (double)totalTime / stats.PacketsReceived;
            }

            return stats;
        }

        /// <summary>
        /// 批量Ping多个主机
        /// </summary>
        /// <param name="hosts">主机列表</param>
        /// <param name="timeout">超时时间（毫秒）</param>
        /// <returns>主机与结果的字典</returns>
        public static async Task<Dictionary<string, PingResult>> PingMultipleAsync(IEnumerable<string> hosts, int timeout = 5000)
        {
            var tasks = hosts.Select(h => PingAsync(h, timeout));
            var results = await Task.WhenAll(tasks).ConfigureAwait(false);

            return hosts.Zip(results, (h, r) => new { Host = h, Result = r })
                       .ToDictionary(x => x.Host, x => x.Result);
        }

        /// <summary>
        /// 检测主机是否可达
        /// </summary>
        /// <param name="hostNameOrAddress">主机名或IP地址</param>
        /// <param name="timeout">超时时间（毫秒）</param>
        /// <returns>是否可达</returns>
        public static async Task<bool> IsReachableAsync(string hostNameOrAddress, int timeout = 5000)
        {
            var result = await PingAsync(hostNameOrAddress, timeout).ConfigureAwait(false);
            return result.Success;
        }

        /// <summary>
        /// 检测主机是否可达
        /// </summary>
        /// <param name="hostNameOrAddress">主机名或IP地址</param>
        /// <param name="timeout">超时时间（毫秒）</param>
        /// <returns>是否可达</returns>
        public static bool IsReachable(string hostNameOrAddress, int timeout = 5000)
        {
            return IsReachableAsync(hostNameOrAddress, timeout).GetAwaiter().GetResult();
        }

        /// <summary>
        /// 检测TCP端口是否开放
        /// </summary>
        /// <param name="hostNameOrAddress">主机名或IP地址</param>
        /// <param name="port">端口号</param>
        /// <param name="timeout">超时时间（毫秒）</param>
        /// <returns>端口是否开放</returns>
        public static async Task<bool> IsPortOpenAsync(string hostNameOrAddress, int port, int timeout = 5000)
        {
            try
            {
                using var client = new TcpClient();
                var connectTask = client.ConnectAsync(hostNameOrAddress, port);
                var timeoutTask = Task.Delay(timeout);

                var completedTask = await Task.WhenAny(connectTask, timeoutTask).ConfigureAwait(false);

                if (completedTask == connectTask && client.Connected)
                {
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 检测TCP端口是否开放
        /// </summary>
        /// <param name="hostNameOrAddress">主机名或IP地址</param>
        /// <param name="port">端口号</param>
        /// <param name="timeout">超时时间（毫秒）</param>
        /// <returns>端口是否开放</returns>
        public static bool IsPortOpen(string hostNameOrAddress, int port, int timeout = 5000)
        {
            return IsPortOpenAsync(hostNameOrAddress, port, timeout).GetAwaiter().GetResult();
        }

        /// <summary>
        /// 测试网络连接速度
        /// </summary>
        /// <param name="hostNameOrAddress">主机名或IP地址</param>
        /// <param name="count">测试次数</param>
        /// <returns>平均延迟（毫秒）</returns>
        public static async Task<double> TestLatencyAsync(string hostNameOrAddress, int count = 5)
        {
            var options = new PingOptions { Count = count };
            var stats = await PingContinuousAsync(hostNameOrAddress, options).ConfigureAwait(false);
            return stats.AverageRoundtripTime;
        }

        /// <summary>
        /// 获取本机IP地址
        /// </summary>
        /// <returns>IP地址列表</returns>
        public static List<IPAddress> GetLocalIPAddresses()
        {
            var result = new List<IPAddress>();
            var interfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (var ni in interfaces)
            {
                if (ni.OperationalStatus != OperationalStatus.Up)
                    continue;

                if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                    continue;

                var ipProperties = ni.GetIPProperties();
                foreach (var ip in ipProperties.UnicastAddresses)
                {
                    if (ip.Address.AddressFamily == AddressFamily.InterNetwork ||
                        ip.Address.AddressFamily == AddressFamily.InterNetworkV6)
                    {
                        result.Add(ip.Address);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 获取默认网关
        /// </summary>
        /// <returns>默认网关地址</returns>
        public static IPAddress? GetDefaultGateway()
        {
            var interfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (var ni in interfaces)
            {
                if (ni.OperationalStatus != OperationalStatus.Up)
                    continue;

                var ipProperties = ni.GetIPProperties();
                foreach (var gateway in ipProperties.GatewayAddresses)
                {
                    if (gateway.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        return gateway.Address;
                    }
                }
            }

            return null;
        }
    }
}
