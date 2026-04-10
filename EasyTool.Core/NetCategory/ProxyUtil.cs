using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace EasyTool.NetCategory
{
    /// <summary>
    /// 代理类型
    /// </summary>
    public enum ProxyType
    {
        /// <summary>
        /// HTTP代理
        /// </summary>
        Http,

        /// <summary>
        /// HTTPS代理
        /// </summary>
        Https,

        /// <summary>
        /// SOCKS4代理
        /// </summary>
        Socks4,

        /// <summary>
        /// SOCKS4a代理
        /// </summary>
        Socks4a,

        /// <summary>
        /// SOCKS5代理
        /// </summary>
        Socks5
    }

    /// <summary>
    /// 代理信息
    /// </summary>
    public class ProxyInfo
    {
        /// <summary>
        /// 代理地址
        /// </summary>
        public string Host { get; set; } = string.Empty;

        /// <summary>
        /// 代理端口
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string? Password { get; set; }

        /// <summary>
        /// 代理类型
        /// </summary>
        public ProxyType Type { get; set; } = ProxyType.Http;

        /// <summary>
        /// 是否需要认证
        /// </summary>
        public bool RequiresAuthentication => !string.IsNullOrEmpty(Username);

        /// <summary>
        /// 代理地址（格式：host:port）
        /// </summary>
        public string Address => $"{Host}:{Port}";

        /// <summary>
        /// 代理URL
        /// </summary>
        public string ProxyUrl
        {
            get
            {
                var scheme = Type switch
                {
                    ProxyType.Http => "http",
                    ProxyType.Https => "https",
                    ProxyType.Socks4 => "socks4",
                    ProxyType.Socks4a => "socks4a",
                    ProxyType.Socks5 => "socks5",
                    _ => "http"
                };

                if (RequiresAuthentication)
                {
                    return $"{scheme}://{Username}:{Password}@{Host}:{Port}";
                }
                return $"{scheme}://{Host}:{Port}";
            }
        }

        public override string ToString()
        {
            return $"{Type}://{Address}";
        }
    }

    /// <summary>
    /// 代理工具类
    /// </summary>
    public static class ProxyUtil
    {
        /// <summary>
        /// 解析代理字符串
        /// 支持格式：
        /// - host:port
        /// - http://host:port
        /// - http://user:pass@host:port
        /// - socks5://host:port
        /// </summary>
        /// <param name="proxyString">代理字符串</param>
        /// <returns>代理信息</returns>
        public static ProxyInfo Parse(string proxyString)
        {
            if (string.IsNullOrWhiteSpace(proxyString))
                throw new ArgumentException("代理字符串不能为空", nameof(proxyString));

            var info = new ProxyInfo();

            // 解析协议
            if (proxyString.Contains("://"))
            {
                var parts = proxyString.Split(new[] { "://" }, 2, StringSplitOptions.None);
                var scheme = parts[0].ToLower();
                info.Type = scheme switch
                {
                    "http" => ProxyType.Http,
                    "https" => ProxyType.Https,
                    "socks4" => ProxyType.Socks4,
                    "socks4a" => ProxyType.Socks4a,
                    "socks5" => ProxyType.Socks5,
                    _ => ProxyType.Http
                };
                proxyString = parts[1];
            }

            // 解析认证信息
            if (proxyString.Contains("@"))
            {
                var authParts = proxyString.Split('@');
                var credentials = authParts[0].Split(':');
                if (credentials.Length == 2)
                {
                    info.Username = credentials[0];
                    info.Password = credentials[1];
                }
                proxyString = authParts[1];
            }

            // 解析主机和端口
            var hostPort = proxyString.Split(':');
            if (hostPort.Length >= 2)
            {
                info.Host = hostPort[0];
                if (int.TryParse(hostPort[1], out var port))
                {
                    info.Port = port;
                }
            }
            else
            {
                info.Host = proxyString;
            }

            return info;
        }

        /// <summary>
        /// 创建WebProxy
        /// </summary>
        /// <param name="proxyInfo">代理信息</param>
        /// <returns>WebProxy</returns>
        public static WebProxy CreateWebProxy(ProxyInfo proxyInfo)
        {
            var proxy = new WebProxy(proxyInfo.Host, proxyInfo.Port);

            if (proxyInfo.RequiresAuthentication)
            {
                proxy.Credentials = new NetworkCredential(proxyInfo.Username, proxyInfo.Password);
            }

            return proxy;
        }

        /// <summary>
        /// 创建WebProxy
        /// </summary>
        /// <param name="host">主机</param>
        /// <param name="port">端口</param>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <returns>WebProxy</returns>
        public static WebProxy CreateWebProxy(string host, int port, string? username = null, string? password = null)
        {
            return CreateWebProxy(new ProxyInfo
            {
                Host = host,
                Port = port,
                Username = username,
                Password = password
            });
        }

        /// <summary>
        /// 创建HttpClientHandler（带代理）
        /// </summary>
        /// <param name="proxyInfo">代理信息</param>
        /// <returns>HttpClientHandler</returns>
        public static HttpClientHandler CreateHttpClientHandler(ProxyInfo proxyInfo)
        {
            var handler = new HttpClientHandler
            {
                Proxy = CreateWebProxy(proxyInfo),
                UseProxy = true
            };

            if (proxyInfo.RequiresAuthentication)
            {
                handler.PreAuthenticate = true;
            }

            return handler;
        }

        /// <summary>
        /// 创建HttpClient（带代理）
        /// </summary>
        /// <param name="proxyInfo">代理信息</param>
        /// <returns>HttpClient</returns>
        public static HttpClient CreateHttpClient(ProxyInfo proxyInfo)
        {
            var handler = CreateHttpClientHandler(proxyInfo);
            return new HttpClient(handler);
        }

        /// <summary>
        /// 创建HttpClient（带代理）
        /// </summary>
        /// <param name="proxyString">代理字符串</param>
        /// <returns>HttpClient</returns>
        public static HttpClient CreateHttpClient(string proxyString)
        {
            var proxyInfo = Parse(proxyString);
            return CreateHttpClient(proxyInfo);
        }

        /// <summary>
        /// 测试代理连接
        /// </summary>
        /// <param name="proxyInfo">代理信息</param>
        /// <param name="testUrl">测试URL</param>
        /// <param name="timeout">超时时间</param>
        /// <returns>是否可用</returns>
        public static async Task<bool> TestProxyAsync(ProxyInfo proxyInfo, string testUrl = "http://www.google.com", TimeSpan? timeout = null)
        {
            try
            {
                using var client = CreateHttpClient(proxyInfo);
                client.Timeout = timeout ?? TimeSpan.FromSeconds(30);

                var response = await client.GetAsync(testUrl).ConfigureAwait(false);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 测试代理连接
        /// </summary>
        /// <param name="proxyString">代理字符串</param>
        /// <param name="testUrl">测试URL</param>
        /// <param name="timeout">超时时间</param>
        /// <returns>是否可用</returns>
        public static async Task<bool> TestProxyAsync(string proxyString, string testUrl = "http://www.google.com", TimeSpan? timeout = null)
        {
            var proxyInfo = Parse(proxyString);
            return await TestProxyAsync(proxyInfo, testUrl, timeout).ConfigureAwait(false);
        }

        /// <summary>
        /// 获取代理响应时间
        /// </summary>
        /// <param name="proxyInfo">代理信息</param>
        /// <param name="testUrl">测试URL</param>
        /// <returns>响应时间（毫秒），失败返回-1</returns>
        public static async Task<long> GetResponseTimeAsync(ProxyInfo proxyInfo, string testUrl = "http://www.google.com")
        {
            try
            {
                using var client = CreateHttpClient(proxyInfo);
                client.Timeout = TimeSpan.FromSeconds(30);

                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                await client.GetAsync(testUrl).ConfigureAwait(false);
                stopwatch.Stop();

                return stopwatch.ElapsedMilliseconds;
            }
            catch
            {
                return -1;
            }
        }

        /// <summary>
        /// 获取系统代理设置
        /// </summary>
        /// <returns>代理信息（无代理返回null）</returns>
        public static ProxyInfo? GetSystemProxy()
        {
            var proxy = WebRequest.GetSystemWebProxy();

            if (proxy == null)
                return null;

            var defaultProxy = proxy.GetProxy(new Uri("http://example.com"));
            if (defaultProxy == null || defaultProxy.Host == "example.com")
                return null;

            return new ProxyInfo
            {
                Host = defaultProxy.Host,
                Port = defaultProxy.Port,
                Type = ProxyType.Http
            };
        }

        /// <summary>
        /// 是否使用系统代理
        /// </summary>
        /// <returns>是否使用系统代理</returns>
        public static bool IsSystemProxyEnabled()
        {
            return GetSystemProxy() != null;
        }
    }

    /// <summary>
    /// 代理池
    /// </summary>
    public class ProxyPool
    {
        private readonly List<ProxyInfo> _proxies = new();
        private readonly Dictionary<string, ProxyStats> _stats = new();
        private int _currentIndex = 0;
        private readonly object _lock = new();

        /// <summary>
        /// 代理数量
        /// </summary>
        public int Count => _proxies.Count;

        /// <summary>
        /// 添加代理
        /// </summary>
        /// <param name="proxyInfo">代理信息</param>
        public void Add(ProxyInfo proxyInfo)
        {
            lock (_lock)
            {
                _proxies.Add(proxyInfo);
                _stats[proxyInfo.Address] = new ProxyStats { ProxyAddress = proxyInfo.Address };
            }
        }

        /// <summary>
        /// 添加代理
        /// </summary>
        /// <param name="proxyString">代理字符串</param>
        public void Add(string proxyString)
        {
            Add(ProxyUtil.Parse(proxyString));
        }

        /// <summary>
        /// 批量添加代理
        /// </summary>
        /// <param name="proxyStrings">代理字符串列表</param>
        public void AddRange(IEnumerable<string> proxyStrings)
        {
            foreach (var proxyString in proxyStrings)
            {
                Add(proxyString);
            }
        }

        /// <summary>
        /// 移除代理
        /// </summary>
        /// <param name="proxyInfo">代理信息</param>
        public void Remove(ProxyInfo proxyInfo)
        {
            lock (_lock)
            {
                _proxies.RemoveAll(p => p.Address == proxyInfo.Address);
                _stats.Remove(proxyInfo.Address);
            }
        }

        /// <summary>
        /// 清空代理池
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                _proxies.Clear();
                _stats.Clear();
                _currentIndex = 0;
            }
        }

        /// <summary>
        /// 获取下一个代理（轮询）
        /// </summary>
        /// <returns>代理信息</returns>
        public ProxyInfo? GetNext()
        {
            lock (_lock)
            {
                if (_proxies.Count == 0)
                    return null;

                var proxy = _proxies[_currentIndex];
                _currentIndex = (_currentIndex + 1) % _proxies.Count;
                return proxy;
            }
        }

        /// <summary>
        /// 获取随机代理
        /// </summary>
        /// <returns>代理信息</returns>
        public ProxyInfo? GetRandom()
        {
            lock (_lock)
            {
                if (_proxies.Count == 0)
                    return null;

                var random = new Random();
                return _proxies[random.Next(_proxies.Count)];
            }
        }

        /// <summary>
        /// 获取最快的代理
        /// </summary>
        /// <returns>代理信息</returns>
        public ProxyInfo? GetFastest()
        {
            lock (_lock)
            {
                if (_proxies.Count == 0)
                    return null;

                return _proxies
                    .Where(p => _stats.ContainsKey(p.Address))
                    .OrderBy(p => _stats[p.Address].AverageResponseTime)
                    .FirstOrDefault() ?? GetRandom();
            }
        }

        /// <summary>
        /// 报告代理使用结果
        /// </summary>
        /// <param name="proxyAddress">代理地址</param>
        /// <param name="success">是否成功</param>
        /// <param name="responseTime">响应时间</param>
        public void ReportResult(string proxyAddress, bool success, long responseTime = 0)
        {
            lock (_lock)
            {
                if (_stats.TryGetValue(proxyAddress, out var stats))
                {
                    stats.TotalRequests++;
                    if (success)
                    {
                        stats.SuccessCount++;
                        if (responseTime > 0)
                        {
                            stats.TotalResponseTime += responseTime;
                            stats.AverageResponseTime = stats.TotalResponseTime / stats.SuccessCount;
                        }
                    }
                    else
                    {
                        stats.FailureCount++;
                    }
                }
            }
        }

        /// <summary>
        /// 获取代理统计信息
        /// </summary>
        /// <param name="proxyAddress">代理地址</param>
        /// <returns>统计信息</returns>
        public ProxyStats? GetStats(string proxyAddress)
        {
            lock (_lock)
            {
                return _stats.TryGetValue(proxyAddress, out var stats) ? stats : null;
            }
        }

        /// <summary>
        /// 移除失败率高的代理
        /// </summary>
        /// <param name="maxFailureRate">最大失败率（0-1）</param>
        /// <returns>移除的代理数量</returns>
        public int RemoveHighFailureProxies(double maxFailureRate = 0.5)
        {
            lock (_lock)
            {
                var toRemove = _stats
                    .Where(s => s.Value.TotalRequests >= 5 && s.Value.FailureRate > maxFailureRate)
                    .Select(s => s.Key)
                    .ToList();

                foreach (var address in toRemove)
                {
                    _proxies.RemoveAll(p => p.Address == address);
                    _stats.Remove(address);
                }

                return toRemove.Count;
            }
        }

        /// <summary>
        /// 测试所有代理
        /// </summary>
        /// <param name="testUrl">测试URL</param>
        /// <param name="timeout">超时时间</param>
        /// <returns>可用代理数量</returns>
        public async Task<int> TestAllAsync(string testUrl = "http://www.google.com", TimeSpan? timeout = null)
        {
            var tasks = _proxies.Select(async proxy =>
            {
                var responseTime = await ProxyUtil.GetResponseTimeAsync(proxy, testUrl).ConfigureAwait(false);
                var success = responseTime >= 0;
                ReportResult(proxy.Address, success, responseTime);
                return success;
            });

            var results = await Task.WhenAll(tasks).ConfigureAwait(false);
            return results.Count(r => r);
        }
    }

    /// <summary>
    /// 代理统计信息
    /// </summary>
    public class ProxyStats
    {
        /// <summary>
        /// 代理地址
        /// </summary>
        public string ProxyAddress { get; set; } = string.Empty;

        /// <summary>
        /// 总请求数
        /// </summary>
        public int TotalRequests { get; set; }

        /// <summary>
        /// 成功次数
        /// </summary>
        public int SuccessCount { get; set; }

        /// <summary>
        /// 失败次数
        /// </summary>
        public int FailureCount { get; set; }

        /// <summary>
        /// 总响应时间
        /// </summary>
        public long TotalResponseTime { get; set; }

        /// <summary>
        /// 平均响应时间
        /// </summary>
        public long AverageResponseTime { get; set; }

        /// <summary>
        /// 成功率
        /// </summary>
        public double SuccessRate => TotalRequests > 0 ? (double)SuccessCount / TotalRequests : 0;

        /// <summary>
        /// 失败率
        /// </summary>
        public double FailureRate => TotalRequests > 0 ? (double)FailureCount / TotalRequests : 0;

        public override string ToString()
        {
            return $"代理: {ProxyAddress}, 总请求: {TotalRequests}, 成功率: {SuccessRate:P2}, 平均响应: {AverageResponseTime}ms";
        }
    }
}
