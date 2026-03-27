using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EasyTool.NetCategory
{
    /// <summary>
    /// HttpClient 构建器
    /// 提供流畅的 HttpClient 配置接口
    /// </summary>
    public class HttpClientBuilder
    {
        private readonly HttpClientHandler _handler;
        private readonly List<DelegatingHandler> _handlers;
        private TimeSpan _timeout = TimeSpan.FromSeconds(100);
        private long _maxResponseContentBufferSize = int.MaxValue;
        private Dictionary<string, string> _defaultHeaders = new();
        private Dictionary<string, string> _defaultRequestHeaders = new();
        private AuthenticationHeaderValue? _authorizationHeader;
        private string? _baseAddress;
        private TimeSpan? _pipeliningPolicy;
        private bool _allowAutoRedirect = true;
        private int _maxAutomaticRedirections = 50;
        private DecompressionMethods _automaticDecompression = DecompressionMethods.None;
        private ICredentials? _credentials;
        private IWebProxy? _proxy;
        private bool _useDefaultCredentials;
        private TimeSpan? _connectionTimeout;
        private int _maxConnectionsPerServer = int.MaxValue;
        private int _maxResponseHeadersLength = 64;

        /// <summary>
        /// 创建 HttpClient 构建器
        /// </summary>
        public HttpClientBuilder()
        {
            _handler = new HttpClientHandler();
            _handlers = new List<DelegatingHandler>();
        }

        #region 基础配置

        /// <summary>
        /// 设置基础地址
        /// </summary>
        /// <param name="baseAddress">基础 URL</param>
        /// <returns>HttpClientBuilder</returns>
        public HttpClientBuilder WithBaseAddress(string baseAddress)
        {
            _baseAddress = baseAddress;
            return this;
        }

        /// <summary>
        /// 设置超时时间
        /// </summary>
        /// <param name="timeout">超时时间</param>
        /// <returns>HttpClientBuilder</returns>
        public HttpClientBuilder WithTimeout(TimeSpan timeout)
        {
            _timeout = timeout;
            return this;
        }

        /// <summary>
        /// 设置最大响应内容缓冲区大小
        /// </summary>
        /// <param name="size">大小（字节）</param>
        /// <returns>HttpClientBuilder</returns>
        public HttpClientBuilder WithMaxResponseContentBufferSize(long size)
        {
            _maxResponseContentBufferSize = size;
            return this;
        }

        #endregion

        #region 请求头

        /// <summary>
        /// 添加默认请求头
        /// </summary>
        /// <param name="name">头名称</param>
        /// <param name="value">头值</param>
        /// <returns>HttpClientBuilder</returns>
        public HttpClientBuilder WithDefaultHeader(string name, string value)
        {
            _defaultHeaders[name] = value;
            return this;
        }

        /// <summary>
        /// 批量添加默认请求头
        /// </summary>
        /// <param name="headers">请求头字典</param>
        /// <returns>HttpClientBuilder</returns>
        public HttpClientBuilder WithDefaultHeaders(Dictionary<string, string> headers)
        {
            foreach (var header in headers)
            {
                _defaultHeaders[header.Key] = header.Value;
            }
            return this;
        }

        /// <summary>
        /// 设置 Accept 头
        /// </summary>
        /// <param name="mediaType">媒体类型</param>
        /// <returns>HttpClientBuilder</returns>
        public HttpClientBuilder WithAccept(string mediaType)
        {
            _defaultRequestHeaders["Accept"] = mediaType;
            return this;
        }

        /// <summary>
        /// 设置 Content-Type 头
        /// </summary>
        /// <param name="mediaType">媒体类型</param>
        /// <returns>HttpClientBuilder</returns>
        public HttpClientBuilder WithContentType(string mediaType)
        {
            _defaultRequestHeaders["Content-Type"] = mediaType;
            return this;
        }

        /// <summary>
        /// 设置 User-Agent 头
        /// </summary>
        /// <param name="userAgent">User-Agent 字符串</param>
        /// <returns>HttpClientBuilder</returns>
        public HttpClientBuilder WithUserAgent(string userAgent)
        {
            _defaultRequestHeaders["User-Agent"] = userAgent;
            return this;
        }

        /// <summary>
        /// 设置 Bearer Token 认证
        /// </summary>
        /// <param name="token">Token</param>
        /// <returns>HttpClientBuilder</returns>
        public HttpClientBuilder WithBearerToken(string token)
        {
            _authorizationHeader = new AuthenticationHeaderValue("Bearer", token);
            return this;
        }

        /// <summary>
        /// 设置 Basic 认证
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <returns>HttpClientBuilder</returns>
        public HttpClientBuilder WithBasicAuth(string username, string password)
        {
            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
            _authorizationHeader = new AuthenticationHeaderValue("Basic", credentials);
            return this;
        }

        /// <summary>
        /// 设置自定义认证头
        /// </summary>
        /// <param name="scheme">认证方案</param>
        /// <param name="parameter">参数</param>
        /// <returns>HttpClientBuilder</returns>
        public HttpClientBuilder WithAuthorization(string scheme, string parameter)
        {
            _authorizationHeader = new AuthenticationHeaderValue(scheme, parameter);
            return this;
        }

        #endregion

        #region 代理和安全

        /// <summary>
        /// 设置代理
        /// </summary>
        /// <param name="proxyUrl">代理 URL</param>
        /// <returns>HttpClientBuilder</returns>
        public HttpClientBuilder WithProxy(string proxyUrl)
        {
            _proxy = new WebProxy(proxyUrl);
            _handler.Proxy = _proxy;
            _handler.UseProxy = true;
            return this;
        }

        /// <summary>
        /// 设置代理
        /// </summary>
        /// <param name="proxy">代理对象</param>
        /// <returns>HttpClientBuilder</returns>
        public HttpClientBuilder WithProxy(IWebProxy proxy)
        {
            _proxy = proxy;
            _handler.Proxy = proxy;
            _handler.UseProxy = true;
            return this;
        }

        /// <summary>
        /// 设置代理凭据
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <returns>HttpClientBuilder</returns>
        public HttpClientBuilder WithProxyCredentials(string username, string password)
        {
            if (_proxy != null)
            {
                _proxy.Credentials = new NetworkCredential(username, password);
            }
            return this;
        }

        /// <summary>
        /// 忽略 SSL 证书错误
        /// </summary>
        /// <returns>HttpClientBuilder</returns>
        public HttpClientBuilder IgnoreSslErrors()
        {
            _handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
            return this;
        }

        /// <summary>
        /// 设置客户端证书
        /// </summary>
        /// <param name="certificates">证书集合</param>
        /// <returns>HttpClientBuilder</returns>
        public HttpClientBuilder WithClientCertificates(System.Security.Cryptography.X509Certificates.X509CertificateCollection certificates)
        {
            _handler.ClientCertificates.AddRange(certificates);
            return this;
        }

        #endregion

        #region 重定向和压缩

        /// <summary>
        /// 设置是否允许自动重定向
        /// </summary>
        /// <param name="allow">是否允许</param>
        /// <returns>HttpClientBuilder</returns>
        public HttpClientBuilder WithAutoRedirect(bool allow)
        {
            _allowAutoRedirect = allow;
            _handler.AllowAutoRedirect = allow;
            return this;
        }

        /// <summary>
        /// 设置最大自动重定向次数
        /// </summary>
        /// <param name="count">次数</param>
        /// <returns>HttpClientBuilder</returns>
        public HttpClientBuilder WithMaxAutomaticRedirections(int count)
        {
            _maxAutomaticRedirections = count;
            _handler.MaxAutomaticRedirections = count;
            return this;
        }

        /// <summary>
        /// 启用自动解压缩
        /// </summary>
        /// <param name="methods">解压缩方法</param>
        /// <returns>HttpClientBuilder</returns>
        public HttpClientBuilder WithAutomaticDecompression(DecompressionMethods methods)
        {
            _automaticDecompression = methods;
            _handler.AutomaticDecompression = methods;
            return this;
        }

        /// <summary>
        /// 启用 Gzip 解压缩
        /// </summary>
        /// <returns>HttpClientBuilder</returns>
        public HttpClientBuilder WithGzipDecompression()
        {
            return WithAutomaticDecompression(DecompressionMethods.GZip);
        }

        /// <summary>
        /// 启用 Deflate 解压缩
        /// </summary>
        /// <returns>HttpClientBuilder</returns>
        public HttpClientBuilder WithDeflateDecompression()
        {
            return WithAutomaticDecompression(DecompressionMethods.Deflate);
        }

        /// <summary>
        /// 启用所有解压缩
        /// </summary>
        /// <returns>HttpClientBuilder</returns>
        public HttpClientBuilder WithAllDecompression()
        {
            return WithAutomaticDecompression(DecompressionMethods.GZip | DecompressionMethods.Deflate);
        }

        #endregion

        #region 连接配置

        /// <summary>
        /// 设置连接超时
        /// </summary>
        /// <param name="timeout">超时时间</param>
        /// <returns>HttpClientBuilder</returns>
        public HttpClientBuilder WithConnectionTimeout(TimeSpan timeout)
        {
            _connectionTimeout = timeout;
            return this;
        }

        /// <summary>
        /// 设置每服务器最大连接数
        /// </summary>
        /// <param name="count">连接数</param>
        /// <returns>HttpClientBuilder</returns>
        public HttpClientBuilder WithMaxConnectionsPerServer(int count)
        {
            _maxConnectionsPerServer = count;
            _handler.MaxConnectionsPerServer = count;
            return this;
        }

        /// <summary>
        /// 设置最大响应头长度
        /// </summary>
        /// <param name="length">长度（KB）</param>
        /// <returns>HttpClientBuilder</returns>
        public HttpClientBuilder WithMaxResponseHeadersLength(int length)
        {
            _maxResponseHeadersLength = length;
            _handler.MaxResponseHeadersLength = length;
            return this;
        }

        /// <summary>
        /// 使用默认凭据
        /// </summary>
        /// <returns>HttpClientBuilder</returns>
        public HttpClientBuilder WithDefaultCredentials()
        {
            _useDefaultCredentials = true;
            _handler.UseDefaultCredentials = true;
            return this;
        }

        /// <summary>
        /// 设置凭据
        /// </summary>
        /// <param name="credentials">凭据</param>
        /// <returns>HttpClientBuilder</returns>
        public HttpClientBuilder WithCredentials(ICredentials credentials)
        {
            _credentials = credentials;
            _handler.Credentials = credentials;
            return this;
        }

        #endregion

        #region 中间件

        /// <summary>
        /// 添加委托处理器
        /// </summary>
        /// <param name="handler">处理器</param>
        /// <returns>HttpClientBuilder</returns>
        public HttpClientBuilder AddHandler(DelegatingHandler handler)
        {
            _handlers.Add(handler);
            return this;
        }

        /// <summary>
        /// 添加重试中间件
        /// </summary>
        /// <param name="retryCount">重试次数</param>
        /// <param name="retryDelay">重试延迟</param>
        /// <returns>HttpClientBuilder</returns>
        public HttpClientBuilder AddRetry(int retryCount, TimeSpan? retryDelay = null)
        {
            _handlers.Add(new RetryHandler(retryCount, retryDelay ?? TimeSpan.FromSeconds(1)));
            return this;
        }

        /// <summary>
        /// 添加超时中间件
        /// </summary>
        /// <param name="timeout">超时时间</param>
        /// <returns>HttpClientBuilder</returns>
        public HttpClientBuilder AddTimeout(TimeSpan timeout)
        {
            _handlers.Add(new TimeoutHandler(timeout));
            return this;
        }

        /// <summary>
        /// 添加日志中间件
        /// </summary>
        /// <param name="logger">日志记录器</param>
        /// <returns>HttpClientBuilder</returns>
        public HttpClientBuilder AddLogging(Action<string> logger)
        {
            _handlers.Add(new LoggingHandler(logger));
            return this;
        }

        #endregion

        #region 构建

        /// <summary>
        /// 构建 HttpClient
        /// </summary>
        /// <returns>HttpClient 实例</returns>
        public HttpClient Build()
        {
            HttpMessageHandler handler = _handler;

            // 反向添加处理器以形成正确的链
            for (int i = _handlers.Count - 1; i >= 0; i--)
            {
                _handlers[i].InnerHandler = handler;
                handler = _handlers[i];
            }

            var client = new HttpClient(handler);

            // 应用配置
            if (!string.IsNullOrEmpty(_baseAddress))
            {
                client.BaseAddress = new Uri(_baseAddress);
            }

            client.Timeout = _timeout;
            client.MaxResponseContentBufferSize = _maxResponseContentBufferSize;

            // 添加默认头
            foreach (var header in _defaultHeaders)
            {
                client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
            }

            foreach (var header in _defaultRequestHeaders)
            {
                client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
            }

            // 设置认证头
            if (_authorizationHeader != null)
            {
                client.DefaultRequestHeaders.Authorization = _authorizationHeader;
            }

            return client;
        }

        /// <summary>
        /// 构建并返回一次性使用的 HttpClient（自动释放 Handler）
        /// </summary>
        /// <returns>HttpClient 实例</returns>
        public HttpClient BuildDisposable()
        {
            return Build();
        }

        #endregion
    }

    #region 中间件处理器

    /// <summary>
    /// 重试处理器
    /// </summary>
    internal class RetryHandler : DelegatingHandler
    {
        private readonly int _retryCount;
        private readonly TimeSpan _retryDelay;

        public RetryHandler(int retryCount, TimeSpan retryDelay)
        {
            _retryCount = retryCount;
            _retryDelay = retryDelay;
            InnerHandler = new HttpClientHandler();
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage? response = null;
            Exception? lastException = null;

            for (int i = 0; i <= _retryCount; i++)
            {
                try
                {
                    response = await base.SendAsync(request, cancellationToken);

                    if (response.IsSuccessStatusCode)
                    {
                        return response;
                    }

                    // 服务器错误时重试
                    if ((int)response.StatusCode >= 500)
                    {
                        lastException = new HttpRequestException($"服务器返回错误: {response.StatusCode}");
                        response.Dispose();
                    }
                    else
                    {
                        return response;
                    }
                }
                catch (Exception ex)
                {
                    lastException = ex;
                }

                if (i < _retryCount)
                {
                    await Task.Delay(_retryDelay, cancellationToken);
                }
            }

            throw lastException ?? new HttpRequestException("重试次数已用尽");
        }
    }

    /// <summary>
    /// 超时处理器
    /// </summary>
    internal class TimeoutHandler : DelegatingHandler
    {
        private readonly TimeSpan _timeout;

        public TimeoutHandler(TimeSpan timeout)
        {
            _timeout = timeout;
            InnerHandler = new HttpClientHandler();
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(_timeout);

            try
            {
                return await base.SendAsync(request, cts.Token);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                throw new TimeoutException($"请求超时: {_timeout}");
            }
        }
    }

    /// <summary>
    /// 日志处理器
    /// </summary>
    internal class LoggingHandler : DelegatingHandler
    {
        private readonly Action<string> _logger;

        public LoggingHandler(Action<string> logger)
        {
            _logger = logger;
            InnerHandler = new HttpClientHandler();
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            _logger($"[{DateTime.UtcNow:HH:mm:ss.fff}] HTTP {request.Method} {request.RequestUri}");

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                var response = await base.SendAsync(request, cancellationToken);
                stopwatch.Stop();

                _logger($"[{DateTime.UtcNow:HH:mm:ss.fff}] HTTP {request.Method} {request.RequestUri} -> {(int)response.StatusCode} ({stopwatch.ElapsedMilliseconds}ms)");

                return response;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger($"[{DateTime.UtcNow:HH:mm:ss.fff}] HTTP {request.Method} {request.RequestUri} -> ERROR: {ex.Message} ({stopwatch.ElapsedMilliseconds}ms)");
                throw;
            }
        }
    }

    #endregion

    /// <summary>
    /// HttpClient 构建工具类
    /// </summary>
    public static class HttpClientBuilderUtil
    {
        /// <summary>
        /// 创建 HttpClient 构建器
        /// </summary>
        /// <returns>HttpClientBuilder</returns>
        public static HttpClientBuilder Create()
        {
            return new HttpClientBuilder();
        }

        /// <summary>
        /// 创建默认 HttpClient
        /// </summary>
        /// <returns>HttpClient</returns>
        public static HttpClient CreateDefault()
        {
            return new HttpClientBuilder()
                .WithAllDecompression()
                .WithTimeout(TimeSpan.FromSeconds(30))
                .Build();
        }

        /// <summary>
        /// 创建 JSON API HttpClient
        /// </summary>
        /// <param name="baseAddress">基础地址</param>
        /// <returns>HttpClient</returns>
        public static HttpClient CreateForJsonApi(string baseAddress)
        {
            return new HttpClientBuilder()
                .WithBaseAddress(baseAddress)
                .WithAccept("application/json")
                .WithContentType("application/json")
                .WithAllDecompression()
                .WithTimeout(TimeSpan.FromSeconds(30))
                .Build();
        }

        /// <summary>
        /// 创建带重试的 HttpClient
        /// </summary>
        /// <param name="retryCount">重试次数</param>
        /// <returns>HttpClient</returns>
        public static HttpClient CreateWithRetry(int retryCount = 3)
        {
            return new HttpClientBuilder()
                .WithAllDecompression()
                .WithTimeout(TimeSpan.FromSeconds(30))
                .AddRetry(retryCount)
                .Build();
        }

        /// <summary>
        /// 创建忽略 SSL 的 HttpClient
        /// </summary>
        /// <returns>HttpClient</returns>
        public static HttpClient CreateIgnoringSsl()
        {
            return new HttpClientBuilder()
                .IgnoreSslErrors()
                .WithAllDecompression()
                .Build();
        }
    }
}
