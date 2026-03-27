using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace EasyTool.NetCategory
{
    /// <summary>
    /// HttpClient 连接池管理器
    /// 正确管理 HttpClient 的生命周期，避免 socket 耗尽问题
    /// </summary>
    public sealed class HttpClientPool : IDisposable
    {
        private static readonly Lazy<HttpClientPool> _default = new(() => new HttpClientPool());
        private readonly ConcurrentDictionary<string, HttpClient> _clients = new();
        private readonly ConcurrentDictionary<string, HttpMessageHandler> _handlers = new();
        private readonly object _lock = new();
        private bool _disposed;

        /// <summary>
        /// 默认 HttpClient 池实例
        /// </summary>
        public static HttpClientPool Default => _default.Value;

        /// <summary>
        /// 获取或创建 HttpClient
        /// </summary>
        /// <param name="name">客户端名称</param>
        /// <param name="configure">配置操作</param>
        /// <returns>HttpClient 实例</returns>
        public HttpClient GetClient(string name = "default", Action<HttpClientOptions>? configure = null)
        {
            ThrowIfDisposed();

            return _clients.GetOrAdd(name, key =>
            {
                var options = new HttpClientOptions();
                configure?.Invoke(options);

                var handler = CreateHandler(options);
                _handlers[key] = handler;

                var client = new HttpClient(handler, disposeHandler: false);
                ConfigureClient(client, options);

                return client;
            });
        }

        /// <summary>
        /// 获取或创建 HttpClient（异步）
        /// </summary>
        /// <param name="name">客户端名称</param>
        /// <param name="configure">配置操作</param>
        /// <returns>HttpClient 实例</returns>
        public Task<HttpClient> GetClientAsync(string name = "default", Action<HttpClientOptions>? configure = null)
        {
            return Task.FromResult(GetClient(name, configure));
        }

        /// <summary>
        /// 移除并释放指定的 HttpClient
        /// </summary>
        /// <param name="name">客户端名称</param>
        public void RemoveClient(string name)
        {
            ThrowIfDisposed();

            if (_clients.TryRemove(name, out var client))
            {
                client.CancelPendingRequests();
                client.Dispose();
            }

            if (_handlers.TryRemove(name, out var handler))
            {
                handler.Dispose();
            }
        }

        /// <summary>
        /// 获取所有客户端名称
        /// </summary>
        /// <returns>客户端名称集合</returns>
        public string[] GetClientNames()
        {
            return _clients.Keys.ToArray();
        }

        /// <summary>
        /// 获取客户端数量
        /// </summary>
        public int ClientCount => _clients.Count;

        /// <summary>
        /// 设置默认请求头
        /// </summary>
        /// <param name="name">客户端名称</param>
        /// <param name="headers">请求头</param>
        public void SetDefaultHeaders(string name, params (string name, string value)[] headers)
        {
            var client = GetClient(name);
            foreach (var (headerName, headerValue) in headers)
            {
                client.DefaultRequestHeaders.Remove(headerName);
                client.DefaultRequestHeaders.TryAddWithoutValidation(headerName, headerValue);
            }
        }

        /// <summary>
        /// 清除所有客户端
        /// </summary>
        public void Clear()
        {
            ThrowIfDisposed();

            foreach (var client in _clients.Values)
            {
                client.CancelPendingRequests();
                client.Dispose();
            }
            _clients.Clear();

            foreach (var handler in _handlers.Values)
            {
                handler.Dispose();
            }
            _handlers.Clear();
        }

        /// <summary>
        /// 为所有客户端设置代理
        /// </summary>
        /// <param name="proxyAddress">代理地址</param>
        public void SetProxyForAll(string proxyAddress)
        {
            ThrowIfDisposed();

            foreach (var kvp in _handlers)
            {
#if NET5_0_OR_GREATER
                if (kvp.Value is SocketsHttpHandler socketsHandler)
                {
                    if (!string.IsNullOrEmpty(proxyAddress))
                    {
                        socketsHandler.Proxy = new WebProxy(proxyAddress);
                        socketsHandler.UseProxy = true;
                    }
                    else
                    {
                        socketsHandler.UseProxy = false;
                    }
                }
#else
                if (kvp.Value is HttpClientHandler httpHandler)
                {
                    if (!string.IsNullOrEmpty(proxyAddress))
                    {
                        httpHandler.Proxy = new WebProxy(proxyAddress);
                        httpHandler.UseProxy = true;
                    }
                    else
                    {
                        httpHandler.UseProxy = false;
                    }
                }
#endif
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            lock (_lock)
            {
                if (_disposed)
                    return;

                Clear();
                _disposed = true;
            }
        }

        private HttpMessageHandler CreateHandler(HttpClientOptions options)
        {
#if NET5_0_OR_GREATER
            // 在 .NET 5+ 中使用 SocketsHttpHandler 以支持连接池设置
            var handler = new SocketsHttpHandler
            {
                AllowAutoRedirect = options.AllowAutoRedirect,
                MaxAutomaticRedirections = options.MaxAutomaticRedirections,
                AutomaticDecompression = options.AutomaticDecompression,
                UseCookies = options.UseCookies,
                UseProxy = options.UseProxy,
                MaxConnectionsPerServer = options.MaxConnectionsPerServer,
                PooledConnectionLifetime = options.PooledConnectionLifetime,
                PooledConnectionIdleTimeout = options.PooledConnectionIdleTimeout
            };

            if (options.Proxy != null)
            {
                handler.Proxy = options.Proxy;
            }

            // SocketsHttpHandler 使用不同的证书验证方式
            if (options.ServerCertificateCustomValidationCallback != null)
            {
#if NET10_0_OR_GREATER
                // .NET 10 中 RemoteCertificateValidationCallback 需要不同的委托签名
                var callback = options.ServerCertificateCustomValidationCallback;
                handler.SslOptions = new SslClientAuthenticationOptions
                {
                    RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) =>
                    {
                        return callback(null, certificate as X509Certificate2, chain, sslPolicyErrors);
                    }
                };
#else
                handler.SslOptions = new SslClientAuthenticationOptions
                {
                    RemoteCertificateValidationCallback = options.ServerCertificateCustomValidationCallback
                };
#endif
            }

            // SocketsHttpHandler 不直接支持 ClientCertificates，需要通过 SslOptions 配置
#else
            // 在 netstandard2.1 中使用 HttpClientHandler（不支持连接池设置）
            var handler = new HttpClientHandler
            {
                AllowAutoRedirect = options.AllowAutoRedirect,
                MaxAutomaticRedirections = options.MaxAutomaticRedirections,
                AutomaticDecompression = options.AutomaticDecompression,
                UseCookies = options.UseCookies,
                UseProxy = options.UseProxy,
                MaxConnectionsPerServer = options.MaxConnectionsPerServer
            };

            if (options.Proxy != null)
            {
                handler.Proxy = options.Proxy;
            }

            if (options.ServerCertificateCustomValidationCallback != null)
            {
                handler.ServerCertificateCustomValidationCallback = options.ServerCertificateCustomValidationCallback;
            }

            if (options.ClientCertificates?.Count > 0)
            {
                handler.ClientCertificates.AddRange(options.ClientCertificates);
            }
#endif

            return handler;
        }

        private void ConfigureClient(HttpClient client, HttpClientOptions options)
        {
            client.Timeout = options.Timeout;
            client.BaseAddress = options.BaseAddress;

            if (options.DefaultRequestHeaders != null)
            {
                foreach (var header in options.DefaultRequestHeaders)
                {
                    client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            if (!string.IsNullOrEmpty(options.UserAgent))
            {
                client.DefaultRequestHeaders.UserAgent.TryParseAdd(options.UserAgent);
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(HttpClientPool));
            }
        }
    }

    /// <summary>
    /// HttpClient 配置选项
    /// </summary>
    public class HttpClientOptions
    {
        /// <summary>
        /// 基础地址
        /// </summary>
        public Uri? BaseAddress { get; set; }

        /// <summary>
        /// 超时时间
        /// </summary>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(100);

        /// <summary>
        /// 是否允许自动重定向
        /// </summary>
        public bool AllowAutoRedirect { get; set; } = true;

        /// <summary>
        /// 最大自动重定向次数
        /// </summary>
        public int MaxAutomaticRedirections { get; set; } = 50;

        /// <summary>
        /// 自动解压缩方式
        /// </summary>
        public DecompressionMethods AutomaticDecompression { get; set; } = DecompressionMethods.GZip | DecompressionMethods.Deflate;

        /// <summary>
        /// 是否使用 Cookie
        /// </summary>
        public bool UseCookies { get; set; } = false;

        /// <summary>
        /// 是否使用代理
        /// </summary>
        public bool UseProxy { get; set; } = false;

        /// <summary>
        /// 代理设置
        /// </summary>
        public IWebProxy? Proxy { get; set; }

        /// <summary>
        /// 每个服务器最大连接数
        /// </summary>
        public int MaxConnectionsPerServer { get; set; } = int.MaxValue;

        /// <summary>
        /// 连接池连接生存期
        /// </summary>
        public TimeSpan PooledConnectionLifetime { get; set; } = TimeSpan.FromMinutes(15);

        /// <summary>
        /// 连接池空闲超时
        /// </summary>
        public TimeSpan PooledConnectionIdleTimeout { get; set; } = TimeSpan.FromMinutes(5);

        /// <summary>
        /// 默认请求头
        /// </summary>
        public Dictionary<string, string>? DefaultRequestHeaders { get; set; }

        /// <summary>
        /// User-Agent
        /// </summary>
        public string? UserAgent { get; set; }

        /// <summary>
        /// 服务器证书验证回调
        /// </summary>
        public Func<HttpRequestMessage, X509Certificate2?, X509Chain?, SslPolicyErrors, bool>?
            ServerCertificateCustomValidationCallback { get; set; }

        /// <summary>
        /// 客户端证书集合
        /// </summary>
        public X509CertificateCollection? ClientCertificates { get; set; }
    }

    /// <summary>
    /// HttpClient 扩展方法
    /// </summary>
    public static class HttpClientPoolExtensions
    {
        /// <summary>
        /// 创建配置好的 HttpClient
        /// </summary>
        public static HttpClient CreateClient(Action<HttpClientOptions>? configure = null)
        {
            return HttpClientPool.Default.GetClient(Guid.NewGuid().ToString(), configure);
        }

        /// <summary>
        /// 创建用于 JSON API 的 HttpClient
        /// </summary>
        public static HttpClient CreateJsonClient(string? baseUrl = null)
        {
            return HttpClientPool.Default.GetClient("json_" + (baseUrl ?? "default"), options =>
            {
                if (!string.IsNullOrEmpty(baseUrl))
                {
                    options.BaseAddress = new Uri(baseUrl);
                }
                options.DefaultRequestHeaders = new Dictionary<string, string>
                {
                    ["Accept"] = "application/json",
                    ["Content-Type"] = "application/json"
                };
            });
        }

        /// <summary>
        /// 创建用于下载大文件的 HttpClient
        /// </summary>
        public static HttpClient CreateDownloadClient()
        {
            return HttpClientPool.Default.GetClient("download", options =>
            {
                options.Timeout = TimeSpan.FromMinutes(30);
                options.AutomaticDecompression = DecompressionMethods.None;
            });
        }

        /// <summary>
        /// 创建允许无效证书的 HttpClient（仅用于开发环境）
        /// </summary>
        public static HttpClient CreateInsecureClient()
        {
            return HttpClientPool.Default.GetClient("insecure_" + Guid.NewGuid(), options =>
            {
                options.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
            });
        }

        /// <summary>
        /// 创建带代理的 HttpClient
        /// </summary>
        public static HttpClient CreateProxyClient(string proxyAddress)
        {
            return HttpClientPool.Default.GetClient("proxy_" + proxyAddress.GetHashCode(), options =>
            {
                options.UseProxy = true;
                options.Proxy = new WebProxy(proxyAddress);
            });
        }
    }
}
