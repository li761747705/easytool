using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace EasyTool.NetCategory
{
    /// <summary>
    /// Webhook配置
    /// </summary>
    public class WebhookOptions
    {
        /// <summary>
        /// Webhook URL
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// HTTP方法（默认POST）
        /// </summary>
        public HttpMethod Method { get; set; } = HttpMethod.Post;

        /// <summary>
        /// 请求头
        /// </summary>
        public Dictionary<string, string> Headers { get; set; } = new();

        /// <summary>
        /// 超时时间
        /// </summary>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// 重试次数
        /// </summary>
        public int MaxRetries { get; set; } = 3;

        /// <summary>
        /// 重试延迟（毫秒）
        /// </summary>
        public int RetryDelayMs { get; set; } = 1000;

        /// <summary>
        /// 是否验证SSL
        /// </summary>
        public bool ValidateSsl { get; set; } = true;

        /// <summary>
        /// 成功响应回调
        /// </summary>
        public Action<WebhookResponse>? OnSuccess { get; set; }

        /// <summary>
        /// 失败响应回调
        /// </summary>
        public Action<WebhookResponse, Exception>? OnFailure { get; set; }
    }

    /// <summary>
    /// Webhook响应
    /// </summary>
    public class WebhookResponse
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// HTTP状态码
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// 响应内容
        /// </summary>
        public string? Content { get; set; }

        /// <summary>
        /// 响应头
        /// </summary>
        public Dictionary<string, string> Headers { get; set; } = new();

        /// <summary>
        /// 错误信息
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 请求耗时
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// 重试次数
        /// </summary>
        public int RetryCount { get; set; }
    }

    /// <summary>
    /// Webhook发送结果
    /// </summary>
    public class WebhookResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 响应
        /// </summary>
        public WebhookResponse? Response { get; set; }

        /// <summary>
        /// 异常
        /// </summary>
        public Exception? Exception { get; set; }
    }

    /// <summary>
    /// Webhook工具类
    /// </summary>
    public static class WebhookUtil
    {
        private static readonly HttpClient _httpClient;

        static WebhookUtil()
        {
            _httpClient = new HttpClient();
        }

        /// <summary>
        /// 发送Webhook
        /// </summary>
        /// <param name="options">配置</param>
        /// <param name="payload">负载数据</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>发送结果</returns>
        public static async Task<WebhookResult> SendAsync(WebhookOptions options, object payload, CancellationToken cancellationToken = default)
        {
            var json = JsonSerializer.Serialize(payload);
            return await SendAsync(options, json, "application/json", cancellationToken);
        }

        /// <summary>
        /// 发送Webhook
        /// </summary>
        /// <param name="options">配置</param>
        /// <param name="content">内容</param>
        /// <param name="contentType">内容类型</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>发送结果</returns>
        public static async Task<WebhookResult> SendAsync(WebhookOptions options, string content, string contentType = "application/json", CancellationToken cancellationToken = default)
        {
            var result = new WebhookResult();
            Exception? lastException = null;
            WebhookResponse? lastResponse = null;

            for (int retry = 0; retry <= options.MaxRetries; retry++)
            {
                try
                {
                    var response = await SendRequestAsync(options, content, contentType, cancellationToken);
                    response.RetryCount = retry;
                    lastResponse = response;

                    if (response.IsSuccess)
                    {
                        result.Success = true;
                        result.Response = response;
                        options.OnSuccess?.Invoke(response);
                        return result;
                    }
                }
                catch (Exception ex)
                {
                    lastException = ex;
                }

                // 延迟重试
                if (retry < options.MaxRetries)
                {
                    await Task.Delay(options.RetryDelayMs * (retry + 1), cancellationToken);
                }
            }

            result.Success = false;
            result.Response = lastResponse;
            result.Exception = lastException;

            if (lastException != null)
            {
                options.OnFailure?.Invoke(lastResponse ?? new WebhookResponse(), lastException);
            }

            return result;
        }

        private static async Task<WebhookResponse> SendRequestAsync(WebhookOptions options, string content, string contentType, CancellationToken cancellationToken)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var response = new WebhookResponse();

            try
            {
                using var request = new HttpRequestMessage(options.Method, options.Url);
                request.Content = new StringContent(content, Encoding.UTF8, contentType);

                // 添加自定义请求头
                foreach (var header in options.Headers)
                {
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }

                // 配置HttpClient
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(options.Timeout);

                var handler = new HttpClientHandler();
                if (!options.ValidateSsl)
                {
                    handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
                }

                using var client = new HttpClient(handler);
                client.Timeout = options.Timeout;

                var httpResponse = await client.SendAsync(request, cts.Token);
                stopwatch.Stop();

                response.StatusCode = (int)httpResponse.StatusCode;
                response.IsSuccess = httpResponse.IsSuccessStatusCode;
#if NETSTANDARD2_1
                response.Content = await httpResponse.Content.ReadAsStringAsync();
#else
                response.Content = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
#endif
                response.Duration = stopwatch.Elapsed;

                foreach (var header in httpResponse.Headers)
                {
                    response.Headers[header.Key] = string.Join(",", header.Value);
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                response.IsSuccess = false;
                response.ErrorMessage = ex.Message;
                response.Duration = stopwatch.Elapsed;
            }

            return response;
        }

        /// <summary>
        /// 发送JSON Webhook
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="payload">负载数据</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>发送结果</returns>
        public static async Task<WebhookResult> SendJsonAsync(string url, object payload, CancellationToken cancellationToken = default)
        {
            var options = new WebhookOptions { Url = url };
            return await SendAsync(options, payload, cancellationToken);
        }

        /// <summary>
        /// 发送文本Webhook
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="content">内容</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>发送结果</returns>
        public static async Task<WebhookResult> SendTextAsync(string url, string content, CancellationToken cancellationToken = default)
        {
            var options = new WebhookOptions { Url = url };
            return await SendAsync(options, content, "text/plain", cancellationToken);
        }

        /// <summary>
        /// 发送表单Webhook
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="formData">表单数据</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>发送结果</returns>
        public static async Task<WebhookResult> SendFormAsync(string url, Dictionary<string, string> formData, CancellationToken cancellationToken = default)
        {
            var options = new WebhookOptions { Url = url };
            var content = new FormUrlEncodedContent(formData);
            var contentString = await content.ReadAsStringAsync();
            return await SendAsync(options, contentString, "application/x-www-form-urlencoded", cancellationToken);
        }
    }

    /// <summary>
    /// Webhook客户端
    /// </summary>
    public class WebhookClient
    {
        private readonly WebhookOptions _options;

        /// <summary>
        /// 创建Webhook客户端
        /// </summary>
        /// <param name="options">配置</param>
        public WebhookClient(WebhookOptions options)
        {
            _options = options;
        }

        /// <summary>
        /// 创建Webhook客户端
        /// </summary>
        /// <param name="url">URL</param>
        public WebhookClient(string url)
        {
            _options = new WebhookOptions { Url = url };
        }

        /// <summary>
        /// 发送Webhook
        /// </summary>
        /// <param name="payload">负载数据</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>发送结果</returns>
        public async Task<WebhookResult> SendAsync(object payload, CancellationToken cancellationToken = default)
        {
            return await WebhookUtil.SendAsync(_options, payload, cancellationToken);
        }

        /// <summary>
        /// 发送Webhook
        /// </summary>
        /// <param name="content">内容</param>
        /// <param name="contentType">内容类型</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>发送结果</returns>
        public async Task<WebhookResult> SendAsync(string content, string contentType = "application/json", CancellationToken cancellationToken = default)
        {
            return await WebhookUtil.SendAsync(_options, content, contentType, cancellationToken);
        }

        /// <summary>
        /// 添加请求头
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <returns>this</returns>
        public WebhookClient WithHeader(string key, string value)
        {
            _options.Headers[key] = value;
            return this;
        }

        /// <summary>
        /// 设置超时时间
        /// </summary>
        /// <param name="timeout">超时时间</param>
        /// <returns>this</returns>
        public WebhookClient WithTimeout(TimeSpan timeout)
        {
            _options.Timeout = timeout;
            return this;
        }

        /// <summary>
        /// 设置重试次数
        /// </summary>
        /// <param name="maxRetries">最大重试次数</param>
        /// <returns>this</returns>
        public WebhookClient WithRetry(int maxRetries)
        {
            _options.MaxRetries = maxRetries;
            return this;
        }

        /// <summary>
        /// 设置成功回调
        /// </summary>
        /// <param name="onSuccess">回调</param>
        /// <returns>this</returns>
        public WebhookClient OnSuccess(Action<WebhookResponse> onSuccess)
        {
            _options.OnSuccess = onSuccess;
            return this;
        }

        /// <summary>
        /// 设置失败回调
        /// </summary>
        /// <param name="onFailure">回调</param>
        /// <returns>this</returns>
        public WebhookClient OnFailure(Action<WebhookResponse, Exception> onFailure)
        {
            _options.OnFailure = onFailure;
            return this;
        }
    }

    /// <summary>
    /// Webhook管理器
    /// </summary>
    public static class WebhookManager
    {
        private static readonly Dictionary<string, WebhookOptions> _webhooks = new();

        /// <summary>
        /// 注册Webhook
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="options">配置</param>
        public static void Register(string name, WebhookOptions options)
        {
            _webhooks[name] = options;
        }

        /// <summary>
        /// 注册Webhook
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="url">URL</param>
        public static void Register(string name, string url)
        {
            _webhooks[name] = new WebhookOptions { Url = url };
        }

        /// <summary>
        /// 获取Webhook配置
        /// </summary>
        /// <param name="name">名称</param>
        /// <returns>配置</returns>
        public static WebhookOptions? Get(string name)
        {
            return _webhooks.TryGetValue(name, out var options) ? options : null;
        }

        /// <summary>
        /// 移除Webhook
        /// </summary>
        /// <param name="name">名称</param>
        /// <returns>是否成功移除</returns>
        public static bool Remove(string name)
        {
            return _webhooks.Remove(name);
        }

        /// <summary>
        /// 发送Webhook
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="payload">负载数据</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>发送结果</returns>
        public static async Task<WebhookResult> SendAsync(string name, object payload, CancellationToken cancellationToken = default)
        {
            var options = Get(name);
            if (options == null)
            {
                return new WebhookResult
                {
                    Success = false,
                    Exception = new Exception($"未找到名为 '{name}' 的Webhook配置")
                };
            }

            return await WebhookUtil.SendAsync(options, payload, cancellationToken);
        }

        /// <summary>
        /// 发送到所有Webhook
        /// </summary>
        /// <param name="payload">负载数据</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>所有结果</returns>
        public static async Task<Dictionary<string, WebhookResult>> SendToAllAsync(object payload, CancellationToken cancellationToken = default)
        {
            var results = new Dictionary<string, WebhookResult>();

            foreach (var kvp in _webhooks)
            {
                results[kvp.Key] = await WebhookUtil.SendAsync(kvp.Value, payload, cancellationToken);
            }

            return results;
        }

        /// <summary>
        /// 获取所有已注册的Webhook名称
        /// </summary>
        /// <returns>名称列表</returns>
        public static IEnumerable<string> GetNames()
        {
            return _webhooks.Keys;
        }

        /// <summary>
        /// 清空所有Webhook
        /// </summary>
        public static void Clear()
        {
            _webhooks.Clear();
        }
    }
}
