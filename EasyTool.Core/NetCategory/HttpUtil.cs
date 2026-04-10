using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace EasyTool.NetCategory
{
    /// <summary>
    /// HTTP工具类
    /// 提供HTTP请求的便捷操作
    /// </summary>
    public static class HttpUtil
    {
        private static readonly HttpClient _sharedClient = new();
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        /// <summary>
        /// 获取共享HttpClient
        /// </summary>
        public static HttpClient SharedClient => _sharedClient;

        /// <summary>
        /// 创建HttpClient
        /// </summary>
        /// <param name="baseAddress">基础地址</param>
        /// <param name="timeout">超时时间</param>
        /// <returns>HttpClient实例</returns>
        public static HttpClient CreateClient(string? baseAddress = null, TimeSpan? timeout = null)
        {
            var client = new HttpClient();

            if (!string.IsNullOrEmpty(baseAddress))
            {
                client.BaseAddress = new Uri(baseAddress);
            }

            if (timeout.HasValue)
            {
                client.Timeout = timeout.Value;
            }

            return client;
        }

        #region GET请求

        /// <summary>
        /// GET请求
        /// </summary>
        public static async Task<string> GetStringAsync(string url, CancellationToken cancellationToken = default)
        {
#if NET5_0_OR_GREATER
            return await _sharedClient.GetStringAsync(url, cancellationToken).ConfigureAwait(false);
#else
            return await _sharedClient.GetStringAsync(url).ConfigureAwait(false);
#endif
        }

        /// <summary>
        /// GET请求
        /// </summary>
        public static async Task<string> GetStringAsync(string url, Dictionary<string, string>? headers, CancellationToken cancellationToken = default)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            using var response = await _sharedClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
#if NET5_0_OR_GREATER
            return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
#else
            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
#endif
        }

        /// <summary>
        /// GET请求（返回字节数组）
        /// </summary>
        public static async Task<byte[]> GetBytesAsync(string url, CancellationToken cancellationToken = default)
        {
#if NET5_0_OR_GREATER
            return await _sharedClient.GetByteArrayAsync(url, cancellationToken).ConfigureAwait(false);
#else
            return await _sharedClient.GetByteArrayAsync(url).ConfigureAwait(false);
#endif
        }

        /// <summary>
        /// GET请求（返回流）
        /// </summary>
        public static async Task<Stream> GetStreamAsync(string url, CancellationToken cancellationToken = default)
        {
#if NET5_0_OR_GREATER
            return await _sharedClient.GetStreamAsync(url, cancellationToken).ConfigureAwait(false);
#else
            return await _sharedClient.GetStreamAsync(url).ConfigureAwait(false);
#endif
        }

        /// <summary>
        /// GET请求（反序列化为对象）
        /// </summary>
        public static async Task<T?> GetJsonAsync<T>(string url, CancellationToken cancellationToken = default)
        {
            var json = await GetStringAsync(url, cancellationToken).ConfigureAwait(false);
            return JsonSerializer.Deserialize<T>(json, _jsonOptions);
        }

        #endregion

        #region POST请求

        /// <summary>
        /// POST请求（字符串内容）
        /// </summary>
        public static async Task<string> PostStringAsync(string url, string content, string? contentType = null, CancellationToken cancellationToken = default)
        {
            using var httpContent = new StringContent(content, Encoding.UTF8, contentType ?? "text/plain");
            using var response = await _sharedClient.PostAsync(url, httpContent, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
#if NET5_0_OR_GREATER
            return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
#else
            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
#endif
        }

        /// <summary>
        /// POST请求（JSON内容）
        /// </summary>
        public static async Task<string> PostJsonAsync<T>(string url, T data, CancellationToken cancellationToken = default)
        {
            var json = JsonSerializer.Serialize(data, _jsonOptions);
            using var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
            using var response = await _sharedClient.PostAsync(url, httpContent, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
#if NET5_0_OR_GREATER
            return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
#else
            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
#endif
        }

        /// <summary>
        /// POST请求（JSON内容，返回反序列化对象）
        /// </summary>
        public static async Task<TResponse?> PostJsonAsync<TRequest, TResponse>(string url, TRequest data, CancellationToken cancellationToken = default)
        {
            var json = await PostJsonAsync(url, data, cancellationToken).ConfigureAwait(false);
            return JsonSerializer.Deserialize<TResponse>(json, _jsonOptions);
        }

        /// <summary>
        /// POST请求（表单数据）
        /// </summary>
        public static async Task<string> PostFormAsync(string url, Dictionary<string, string> formData, CancellationToken cancellationToken = default)
        {
            using var httpContent = new FormUrlEncodedContent(formData);
            using var response = await _sharedClient.PostAsync(url, httpContent, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
#if NET5_0_OR_GREATER
            return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
#else
            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
#endif
        }

        #endregion

        #region PUT请求

        /// <summary>
        /// PUT请求
        /// </summary>
        public static async Task<string> PutStringAsync(string url, string content, string? contentType = null, CancellationToken cancellationToken = default)
        {
            using var httpContent = new StringContent(content, Encoding.UTF8, contentType ?? "text/plain");
            using var response = await _sharedClient.PutAsync(url, httpContent, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
#if NET5_0_OR_GREATER
            return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
#else
            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
#endif
        }

        /// <summary>
        /// PUT请求（JSON内容）
        /// </summary>
        public static async Task<string> PutJsonAsync<T>(string url, T data, CancellationToken cancellationToken = default)
        {
            var json = JsonSerializer.Serialize(data, _jsonOptions);
            using var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
            using var response = await _sharedClient.PutAsync(url, httpContent, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
#if NET5_0_OR_GREATER
            return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
#else
            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
#endif
        }

        #endregion

        #region DELETE请求

        /// <summary>
        /// DELETE请求
        /// </summary>
        public static async Task<string> DeleteAsync(string url, CancellationToken cancellationToken = default)
        {
            using var response = await _sharedClient.DeleteAsync(url, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
#if NET5_0_OR_GREATER
            return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
#else
            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
#endif
        }

        #endregion

        #region 通用请求

        /// <summary>
        /// 发送请求
        /// </summary>
        public static async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
        {
            return await _sharedClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// 发送请求并返回字符串
        /// </summary>
        public static async Task<string> SendAsStringAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
        {
            using var response = await _sharedClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
#if NET5_0_OR_GREATER
            return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
#else
            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
#endif
        }

        /// <summary>
        /// 下载文件
        /// </summary>
        public static async Task DownloadFileAsync(string url, string filePath, CancellationToken cancellationToken = default)
        {
            using var response = await _sharedClient.GetAsync(url, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using var fileStream = File.Create(filePath);
#if NET5_0_OR_GREATER
            await response.Content.CopyToAsync(fileStream, cancellationToken).ConfigureAwait(false);
#else
            await response.Content.CopyToAsync(fileStream).ConfigureAwait(false);
#endif
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        public static async Task<string> UploadFileAsync(string url, string filePath, string? formFieldName = null, CancellationToken cancellationToken = default)
        {
            using var fileStream = File.OpenRead(filePath);
            using var streamContent = new StreamContent(fileStream);
            using var formData = new MultipartFormDataContent();

            var fieldName = formFieldName ?? "file";
            var fileName = Path.GetFileName(filePath);

            formData.Add(streamContent, fieldName, fileName);

            using var response = await _sharedClient.PostAsync(url, formData, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
#if NET5_0_OR_GREATER
            return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
#else
            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
#endif
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 构建查询字符串
        /// </summary>
        public static string BuildQueryString(Dictionary<string, string?> parameters)
        {
            if (parameters == null || parameters.Count == 0)
                return string.Empty;

            var sb = new StringBuilder();
            foreach (var kvp in parameters)
            {
                if (kvp.Value != null)
                {
                    if (sb.Length > 0)
                        sb.Append('&');
                    sb.Append(Uri.EscapeDataString(kvp.Key));
                    sb.Append('=');
                    sb.Append(Uri.EscapeDataString(kvp.Value));
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// 解析查询字符串
        /// </summary>
        public static Dictionary<string, string> ParseQueryString(string query)
        {
            var result = new Dictionary<string, string>();

            if (string.IsNullOrEmpty(query))
                return result;

            if (query.StartsWith("?"))
                query = query.Substring(1);

            foreach (var pair in query.Split('&'))
            {
                var index = pair.IndexOf('=');
                if (index > 0)
                {
                    var key = Uri.UnescapeDataString(pair.Substring(0, index));
                    var value = Uri.UnescapeDataString(pair.Substring(index + 1));
                    result[key] = value;
                }
                else if (pair.Length > 0)
                {
                    result[Uri.UnescapeDataString(pair)] = string.Empty;
                }
            }

            return result;
        }

        /// <summary>
        /// 组合URL和查询参数
        /// </summary>
        public static string CombineUrl(string baseUrl, Dictionary<string, string?> parameters)
        {
            var queryString = BuildQueryString(parameters);
            if (string.IsNullOrEmpty(queryString))
                return baseUrl;

            var separator = baseUrl.Contains('?') ? "&" : "?";
            return baseUrl + separator + queryString;
        }

        #endregion

        #region 重试机制

        /// <summary>
        /// 带重试的HTTP请求
        /// </summary>
        /// <param name="requestFactory">请求工厂（每次重试创建新请求）</param>
        /// <param name="maxRetries">最大重试次数</param>
        /// <param name="retryDelay">重试延迟</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>HTTP响应</returns>
        public static async Task<HttpResponseMessage> WithRetryAsync(
            Func<HttpRequestMessage> requestFactory,
            int maxRetries = 3,
            TimeSpan? retryDelay = null,
            CancellationToken cancellationToken = default)
        {
            var delay = retryDelay ?? TimeSpan.FromSeconds(1);
            Exception? lastException = null;

            for (int i = 0; i <= maxRetries; i++)
            {
                try
                {
                    using var request = requestFactory();
                    var response = await _sharedClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

                    if (response.IsSuccessStatusCode || i == maxRetries)
                    {
                        return response;
                    }

                    // 服务器错误时重试
                    if ((int)response.StatusCode >= 500)
                    {
                        await Task.Delay(delay * (i + 1), cancellationToken).ConfigureAwait(false);
                        continue;
                    }

                    return response;
                }
                catch (HttpRequestException ex)
                {
                    lastException = ex;
                    if (i < maxRetries)
                    {
                        await Task.Delay(delay * (i + 1), cancellationToken).ConfigureAwait(false);
                    }
                }
                catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
                {
                    lastException = ex;
                    if (i < maxRetries)
                    {
                        await Task.Delay(delay * (i + 1), cancellationToken).ConfigureAwait(false);
                    }
                }
            }

            throw lastException ?? new HttpRequestException("请求失败");
        }

        /// <summary>
        /// 带指数退避的重试
        /// </summary>
        /// <param name="requestFactory">请求工厂</param>
        /// <param name="maxRetries">最大重试次数</param>
        /// <param name="baseDelay">基础延迟</param>
        /// <param name="maxDelay">最大延迟</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>HTTP响应</returns>
        public static async Task<HttpResponseMessage> WithExponentialBackoffAsync(
            Func<HttpRequestMessage> requestFactory,
            int maxRetries = 5,
            TimeSpan? baseDelay = null,
            TimeSpan? maxDelay = null,
            CancellationToken cancellationToken = default)
        {
            var baseDelayTime = baseDelay ?? TimeSpan.FromSeconds(1);
            var maxDelayTime = maxDelay ?? TimeSpan.FromMinutes(1);
            var random = new Random();
            Exception? lastException = null;

            for (int i = 0; i <= maxRetries; i++)
            {
                try
                {
                    using var request = requestFactory();
                    var response = await _sharedClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

                    if (response.IsSuccessStatusCode || i == maxRetries)
                    {
                        return response;
                    }

                    if ((int)response.StatusCode >= 500)
                    {
                        var delay = CalculateExponentialDelay(i, baseDelayTime, maxDelayTime, random);
                        await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                        continue;
                    }

                    return response;
                }
                catch (HttpRequestException ex)
                {
                    lastException = ex;
                    if (i < maxRetries)
                    {
                        var delay = CalculateExponentialDelay(i, baseDelayTime, maxDelayTime, random);
                        await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                    }
                }
                catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
                {
                    lastException = ex;
                    if (i < maxRetries)
                    {
                        var delay = CalculateExponentialDelay(i, baseDelayTime, maxDelayTime, random);
                        await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                    }
                }
            }

            throw lastException ?? new HttpRequestException("请求失败");
        }

        private static TimeSpan CalculateExponentialDelay(int retryCount, TimeSpan baseDelay, TimeSpan maxDelay, Random random)
        {
            // 指数退避 + 抖动
            var exponentialDelay = TimeSpan.FromTicks(baseDelay.Ticks * (long)Math.Pow(2, retryCount));
            var jitter = TimeSpan.FromMilliseconds(random.Next(0, 1000));
            var totalDelay = exponentialDelay + jitter;

            return totalDelay > maxDelay ? maxDelay : totalDelay;
        }

        /// <summary>
        /// 创建带重试策略的HttpClient包装器
        /// </summary>
        /// <param name="client">HttpClient实例</param>
        /// <param name="maxRetries">最大重试次数</param>
        /// <param name="retryDelay">重试延迟</param>
        /// <returns>重试客户端包装器</returns>
        public static RetryHttpClientWrapper CreateRetryWrapper(HttpClient client, int maxRetries = 3, TimeSpan? retryDelay = null)
        {
            return new RetryHttpClientWrapper(client, maxRetries, retryDelay);
        }

        #endregion
    }

    /// <summary>
    /// 重试HttpClient包装器
    /// </summary>
    public class RetryHttpClientWrapper
    {
        private readonly HttpClient _client;
        private readonly int _maxRetries;
        private readonly TimeSpan _retryDelay;

        /// <summary>
        /// 创建重试包装器
        /// </summary>
        public RetryHttpClientWrapper(HttpClient client, int maxRetries = 3, TimeSpan? retryDelay = null)
        {
            _client = client;
            _maxRetries = maxRetries;
            _retryDelay = retryDelay ?? TimeSpan.FromSeconds(1);
        }

        /// <summary>
        /// 发送带重试的请求
        /// </summary>
        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
        {
            Exception? lastException = null;

            for (int i = 0; i <= _maxRetries; i++)
            {
                try
                {
                    // 克隆请求（因为HttpRequestMessage只能发送一次）
                    var clonedRequest = await CloneRequestAsync(request).ConfigureAwait(false);
                    var response = await _client.SendAsync(clonedRequest, cancellationToken).ConfigureAwait(false);

                    if (response.IsSuccessStatusCode || i == _maxRetries)
                    {
                        return response;
                    }

                    if ((int)response.StatusCode >= 500)
                    {
                        await Task.Delay(_retryDelay * (i + 1), cancellationToken).ConfigureAwait(false);
                        continue;
                    }

                    return response;
                }
                catch (HttpRequestException ex)
                {
                    lastException = ex;
                    if (i < _maxRetries)
                    {
                        await Task.Delay(_retryDelay * (i + 1), cancellationToken).ConfigureAwait(false);
                    }
                }
                catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
                {
                    lastException = ex;
                    if (i < _maxRetries)
                    {
                        await Task.Delay(_retryDelay * (i + 1), cancellationToken).ConfigureAwait(false);
                    }
                }
            }

            throw lastException ?? new HttpRequestException("请求失败");
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

        /// <summary>
        /// GET请求
        /// </summary>
        public async Task<string> GetStringAsync(string url, CancellationToken cancellationToken = default)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            using var response = await SendAsync(request, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// POST请求
        /// </summary>
        public async Task<string> PostJsonAsync<T>(string url, T data, CancellationToken cancellationToken = default)
        {
            var json = JsonSerializer.Serialize(data);
            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            using var response = await SendAsync(request, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        }
    }
}