using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace EasyTool.NetCategory
{
    /// <summary>
    /// HTTP重试工具类
    /// 提供HTTP请求的重试、熔断、超时等功能
    /// </summary>
    public static class HttpRetryUtil
    {
        #region 配置

        /// <summary>
        /// 重试配置
        /// </summary>
        public class RetryOptions
        {
            /// <summary>
            /// 最大重试次数
            /// </summary>
            public int MaxRetries { get; set; } = 3;

            /// <summary>
            /// 初始延迟（毫秒）
            /// </summary>
            public int InitialDelayMs { get; set; } = 1000;

            /// <summary>
            /// 最大延迟（毫秒）
            /// </summary>
            public int MaxDelayMs { get; set; } = 30000;

            /// <summary>
            /// 延迟倍数（指数退避）
            /// </summary>
            public double BackoffMultiplier { get; set; } = 2.0;

            /// <summary>
            /// 是否使用抖动
            /// </summary>
            public bool UseJitter { get; set; } = true;

            /// <summary>
            /// 超时时间（毫秒）
            /// </summary>
            public int TimeoutMs { get; set; } = 30000;

            /// <summary>
            /// 需要重试的HTTP状态码
            /// </summary>
            public HttpStatusCode[] RetryStatusCodes { get; set; } = new[]
            {
                HttpStatusCode.RequestTimeout,        // 408
                HttpStatusCode.TooManyRequests,       // 429
                HttpStatusCode.InternalServerError,   // 500
                HttpStatusCode.BadGateway,            // 502
                HttpStatusCode.ServiceUnavailable,    // 503
                HttpStatusCode.GatewayTimeout         // 504
            };
        }

        #endregion

        #region 重试执行

        /// <summary>
        /// 执行带重试的HTTP请求
        /// </summary>
        /// <param name="httpClient">HttpClient实例</param>
        /// <param name="request">HTTP请求</param>
        /// <param name="options">重试选项</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>HTTP响应</returns>
        public static async Task<HttpResponseMessage> ExecuteWithRetryAsync(
            HttpClient httpClient,
            HttpRequestMessage request,
            RetryOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            options ??= new RetryOptions();
            HttpResponseMessage? response = null;
            Exception? lastException = null;

            for (int attempt = 0; attempt <= options.MaxRetries; attempt++)
            {
                try
                {
                    using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    cts.CancelAfter(options.TimeoutMs);

                    response = await httpClient.SendAsync(request, cts.Token).ConfigureAwait(false);

                    // 如果成功或不需要重试的状态码，直接返回
                    if (response.IsSuccessStatusCode || !ShouldRetry(response.StatusCode, options))
                    {
                        return response;
                    }

                    lastException = new HttpRequestException($"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}");
                }
                catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
                {
                    lastException = new TimeoutException("请求超时", ex);
                }
                catch (HttpRequestException ex)
                {
                    lastException = ex;
                }

                // 如果还有重试机会，等待后重试
                if (attempt < options.MaxRetries)
                {
                    var delay = CalculateDelay(attempt, options);
                    await Task.Delay(delay, cancellationToken).ConfigureAwait(false);

                    // 克隆请求以支持重试
                    request = await CloneRequestAsync(request).ConfigureAwait(false);
                }
            }

            throw lastException ?? new HttpRequestException("请求失败");
        }

        /// <summary>
        /// 执行带重试的GET请求
        /// </summary>
        public static async Task<string> GetStringWithRetryAsync(
            HttpClient httpClient,
            string url,
            RetryOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            using var response = await ExecuteWithRetryAsync(httpClient, request, options, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// 执行带重试的POST请求
        /// </summary>
        public static async Task<HttpResponseMessage> PostWithRetryAsync(
            HttpClient httpClient,
            string url,
            HttpContent content,
            RetryOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };
            return await ExecuteWithRetryAsync(httpClient, request, options, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// 执行带重试的JSON POST请求
        /// </summary>
        public static async Task<TResponse?> PostJsonWithRetryAsync<TRequest, TResponse>(
            HttpClient httpClient,
            string url,
            TRequest data,
            RetryOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await PostWithRetryAsync(httpClient, url, content, options, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var responseText = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonSerializer.Deserialize<TResponse>(responseText, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        #endregion

        #region 熔断器

        /// <summary>
        /// 简单熔断器
        /// </summary>
        public class CircuitBreaker
        {
            private readonly int _failureThreshold;
            private readonly TimeSpan _resetTimeout;
            private int _failureCount;
            private DateTime _lastFailureTime;
            private CircuitState _state = CircuitState.Closed;

            /// <summary>
            /// 当前状态
            /// </summary>
            public CircuitState State => _state;

            /// <summary>
            /// 创建熔断器
            /// </summary>
            /// <param name="failureThreshold">失败阈值</param>
            /// <param name="resetTimeout">重置超时</param>
            public CircuitBreaker(int failureThreshold = 5, TimeSpan? resetTimeout = null)
            {
                _failureThreshold = failureThreshold;
                _resetTimeout = resetTimeout ?? TimeSpan.FromMinutes(1);
            }

            /// <summary>
            /// 执行操作（带熔断保护）
            /// </summary>
            public async Task<T> ExecuteAsync<T>(Func<Task<T>> action)
            {
                if (_state == CircuitState.Open)
                {
                    if (DateTime.UtcNow - _lastFailureTime > _resetTimeout)
                    {
                        _state = CircuitState.HalfOpen;
                    }
                    else
                    {
                        throw new CircuitBreakerOpenException("熔断器已打开");
                    }
                }

                try
                {
                    var result = await action().ConfigureAwait(false);
                    OnSuccess();
                    return result;
                }
                catch (Exception)
                {
                    OnFailure();
                    throw;
                }
            }

            private void OnSuccess()
            {
                _failureCount = 0;
                _state = CircuitState.Closed;
            }

            private void OnFailure()
            {
                _failureCount++;
                _lastFailureTime = DateTime.UtcNow;

                if (_failureCount >= _failureThreshold)
                {
                    _state = CircuitState.Open;
                }
            }
        }

        /// <summary>
        /// 熔断器状态
        /// </summary>
        public enum CircuitState
        {
            /// <summary>
            /// 关闭（正常）
            /// </summary>
            Closed,
            /// <summary>
            /// 打开（熔断）
            /// </summary>
            Open,
            /// <summary>
            /// 半开（尝试恢复）
            /// </summary>
            HalfOpen
        }

        /// <summary>
        /// 熔断器打开异常
        /// </summary>
        public class CircuitBreakerOpenException : Exception
        {
            public CircuitBreakerOpenException(string message) : base(message) { }
        }

        #endregion

        #region 辅助方法

        private static bool ShouldRetry(HttpStatusCode statusCode, RetryOptions options)
        {
            return Array.IndexOf(options.RetryStatusCodes, statusCode) >= 0;
        }

        private static int CalculateDelay(int attempt, RetryOptions options)
        {
            var delay = (int)(options.InitialDelayMs * Math.Pow(options.BackoffMultiplier, attempt));
            delay = Math.Min(delay, options.MaxDelayMs);

            if (options.UseJitter)
            {
                var random = new Random();
                delay = (int)(delay * (0.5 + random.NextDouble()));
            }

            return delay;
        }

        private static async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage request)
        {
            var clone = new HttpRequestMessage(request.Method, request.RequestUri);

            if (request.Content != null)
            {
                var content = await request.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                clone.Content = new ByteArrayContent(content);

                foreach (var header in request.Content.Headers)
                {
                    clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            foreach (var header in request.Headers)
            {
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            return clone;
        }

        #endregion
    }
}