using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace EasyTool.NetCategory
{
    /// <summary>
    /// HTTP 请求配置
    /// </summary>
    public class HttpRequestConfig
    {
        /// <summary>
        /// 请求超时时间（毫秒）
        /// </summary>
        public int Timeout { get; set; } = 30000;

        /// <summary>
        /// 请求头
        /// </summary>
        public Dictionary<string, string> Headers { get; set; } = new();

        /// <summary>
        /// URL 参数
        /// </summary>
        public Dictionary<string, string> QueryParams { get; set; } = new();

        /// <summary>
        /// 内容类型
        /// </summary>
        public string ContentType { get; set; } = "application/json";

        /// <summary>
        /// 字符编码
        /// </summary>
        public Encoding Encoding { get; set; } = Encoding.UTF8;

        /// <summary>
        /// 是否跟随重定向
        /// </summary>
        public bool AllowRedirect { get; set; } = true;

        /// <summary>
        /// 重试次数
        /// </summary>
        public int RetryCount { get; set; } = 0;

        /// <summary>
        /// 重试间隔（毫秒）
        /// </summary>
        public int RetryInterval { get; set; } = 1000;

        /// <summary>
        /// Basic 认证
        /// </summary>
        public (string Username, string Password)? BasicAuth { get; set; }

        /// <summary>
        /// Bearer Token
        /// </summary>
        public string? BearerToken { get; set; }
    }

    /// <summary>
    /// HTTP 响应结果
    /// </summary>
    public class HttpResponse
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// HTTP 状态码
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// 响应内容
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// 响应头
        /// </summary>
        public Dictionary<string, string> Headers { get; set; } = new();

        /// <summary>
        /// 错误信息
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 异常
        /// </summary>
        public Exception? Exception { get; set; }
    }

    /// <summary>
    /// HTTP 响应结果（泛型）
    /// </summary>
    public class HttpResponse<T> : HttpResponse
    {
        /// <summary>
        /// 反序列化后的数据
        /// </summary>
        public T? Data { get; set; }
    }

    /// <summary>
    /// HTTP 工具类
    /// 提供便捷的 HTTP 请求方法
    /// </summary>
    public static class HttpUtil
    {
        private static readonly Lazy<HttpClient> _sharedClient = new(() => CreateDefaultClient());
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        /// <summary>
        /// 获取共享的 HttpClient 实例
        /// </summary>
        public static HttpClient SharedClient => _sharedClient.Value;

        private static HttpClient CreateDefaultClient()
        {
            var handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                AllowAutoRedirect = true,
                MaxAutomaticRedirections = 10
            };

            var client = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

            client.DefaultRequestHeaders.UserAgent.ParseAdd("EasyTool/1.0");
            return client;
        }

        #region GET 请求

        /// <summary>
        /// 发送 GET 请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="config">请求配置（可选）</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>响应结果</returns>
        public static async Task<HttpResponse> GetAsync(
            string url,
            HttpRequestConfig? config = null,
            CancellationToken cancellationToken = default)
        {
            return await SendAsync(url, HttpMethod.Get, null, config, cancellationToken);
        }

        /// <summary>
        /// 发送 GET 请求并反序列化响应
        /// </summary>
        /// <typeparam name="T">响应类型</typeparam>
        /// <param name="url">请求地址</param>
        /// <param name="config">请求配置（可选）</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>响应结果</returns>
        public static async Task<HttpResponse<T>> GetAsync<T>(
            string url,
            HttpRequestConfig? config = null,
            CancellationToken cancellationToken = default)
        {
            return await SendAsync<T>(url, HttpMethod.Get, null, config, cancellationToken);
        }

        /// <summary>
        /// 发送 GET 请求（同步）
        /// </summary>
        public static HttpResponse Get(string url, HttpRequestConfig? config = null)
        {
            return GetAsync(url, config).GetAwaiter().GetResult();
        }

        #endregion

        #region POST 请求

        /// <summary>
        /// 发送 POST 请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="body">请求体</param>
        /// <param name="config">请求配置（可选）</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>响应结果</returns>
        public static async Task<HttpResponse> PostAsync(
            string url,
            object? body = null,
            HttpRequestConfig? config = null,
            CancellationToken cancellationToken = default)
        {
            return await SendAsync(url, HttpMethod.Post, body, config, cancellationToken);
        }

        /// <summary>
        /// 发送 POST 请求并反序列化响应
        /// </summary>
        public static async Task<HttpResponse<T>> PostAsync<T>(
            string url,
            object? body = null,
            HttpRequestConfig? config = null,
            CancellationToken cancellationToken = default)
        {
            return await SendAsync<T>(url, HttpMethod.Post, body, config, cancellationToken);
        }

        /// <summary>
        /// 发送 POST 请求（同步）
        /// </summary>
        public static HttpResponse Post(string url, object? body = null, HttpRequestConfig? config = null)
        {
            return PostAsync(url, body, config).GetAwaiter().GetResult();
        }

        /// <summary>
        /// 发送 JSON POST 请求
        /// </summary>
        public static async Task<HttpResponse> PostJsonAsync<T>(
            string url,
            T data,
            HttpRequestConfig? config = null,
            CancellationToken cancellationToken = default)
        {
            config ??= new HttpRequestConfig();
            config.ContentType = "application/json";
            return await PostAsync(url, data, config, cancellationToken);
        }

        /// <summary>
        /// 发送表单 POST 请求
        /// </summary>
        public static async Task<HttpResponse> PostFormAsync(
            string url,
            Dictionary<string, string> formData,
            HttpRequestConfig? config = null,
            CancellationToken cancellationToken = default)
        {
            config ??= new HttpRequestConfig();
            config.ContentType = "application/x-www-form-urlencoded";
            return await PostAsync(url, formData, config, cancellationToken);
        }

        #endregion

        #region PUT 请求

        /// <summary>
        /// 发送 PUT 请求
        /// </summary>
        public static async Task<HttpResponse> PutAsync(
            string url,
            object? body = null,
            HttpRequestConfig? config = null,
            CancellationToken cancellationToken = default)
        {
            return await SendAsync(url, HttpMethod.Put, body, config, cancellationToken);
        }

        /// <summary>
        /// 发送 PUT 请求并反序列化响应
        /// </summary>
        public static async Task<HttpResponse<T>> PutAsync<T>(
            string url,
            object? body = null,
            HttpRequestConfig? config = null,
            CancellationToken cancellationToken = default)
        {
            return await SendAsync<T>(url, HttpMethod.Put, body, config, cancellationToken);
        }

        /// <summary>
        /// 发送 PUT 请求（同步）
        /// </summary>
        public static HttpResponse Put(string url, object? body = null, HttpRequestConfig? config = null)
        {
            return PutAsync(url, body, config).GetAwaiter().GetResult();
        }

        #endregion

        #region DELETE 请求

        /// <summary>
        /// 发送 DELETE 请求
        /// </summary>
        public static async Task<HttpResponse> DeleteAsync(
            string url,
            HttpRequestConfig? config = null,
            CancellationToken cancellationToken = default)
        {
            return await SendAsync(url, HttpMethod.Delete, null, config, cancellationToken);
        }

        /// <summary>
        /// 发送 DELETE 请求并反序列化响应
        /// </summary>
        public static async Task<HttpResponse<T>> DeleteAsync<T>(
            string url,
            HttpRequestConfig? config = null,
            CancellationToken cancellationToken = default)
        {
            return await SendAsync<T>(url, HttpMethod.Delete, null, config, cancellationToken);
        }

        /// <summary>
        /// 发送 DELETE 请求（同步）
        /// </summary>
        public static HttpResponse Delete(string url, HttpRequestConfig? config = null)
        {
            return DeleteAsync(url, config).GetAwaiter().GetResult();
        }

        #endregion

        #region PATCH 请求

        /// <summary>
        /// 发送 PATCH 请求
        /// </summary>
        public static async Task<HttpResponse> PatchAsync(
            string url,
            object? body = null,
            HttpRequestConfig? config = null,
            CancellationToken cancellationToken = default)
        {
            return await SendAsync(url, HttpMethod.Patch, body, config, cancellationToken);
        }

        /// <summary>
        /// 发送 PATCH 请求（同步）
        /// </summary>
        public static HttpResponse Patch(string url, object? body = null, HttpRequestConfig? config = null)
        {
            return PatchAsync(url, body, config).GetAwaiter().GetResult();
        }

        #endregion

        #region 文件操作

        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="url">文件地址</param>
        /// <param name="savePath">保存路径</param>
        /// <param name="config">请求配置（可选）</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>是否成功</returns>
        public static async Task<bool> DownloadFileAsync(
            string url,
            string savePath,
            HttpRequestConfig? config = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var directory = Path.GetDirectoryName(savePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                using var client = CreateClient(config);
                using var response = await client.GetAsync(BuildUrl(url, config), cancellationToken);
                response.EnsureSuccessStatusCode();

                using var fs = new FileStream(savePath, FileMode.Create, FileAccess.Write, FileShare.None);
                await response.Content.CopyToAsync(fs);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="url">上传地址</param>
        /// <param name="filePath">文件路径</param>
        /// <param name="fieldName">表单字段名</param>
        /// <param name="config">请求配置（可选）</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>响应结果</returns>
        public static async Task<HttpResponse> UploadFileAsync(
            string url,
            string filePath,
            string fieldName = "file",
            HttpRequestConfig? config = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return new HttpResponse
                    {
                        IsSuccess = false,
                        ErrorMessage = $"文件不存在: {filePath}"
                    };
                }

                using var client = CreateClient(config);
                using var content = new MultipartFormDataContent();

                var fileBytes = await File.ReadAllBytesAsync(filePath, cancellationToken);
                var fileContent = new ByteArrayContent(fileBytes);
                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(GetMimeType(filePath));

                content.Add(fileContent, fieldName, Path.GetFileName(filePath));

                // 添加其他表单字段
                if (config?.QueryParams != null)
                {
                    foreach (var param in config.QueryParams)
                    {
                        content.Add(new StringContent(param.Value), param.Key);
                    }
                }

                using var response = await client.PostAsync(BuildUrl(url, config), content, cancellationToken);
                var responseContent = await response.Content.ReadAsStringAsync();

                return new HttpResponse
                {
                    IsSuccess = response.IsSuccessStatusCode,
                    StatusCode = response.StatusCode,
                    Content = responseContent
                };
            }
            catch (Exception ex)
            {
                return new HttpResponse
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message,
                    Exception = ex
                };
            }
        }

        #endregion

        #region 核心方法

        /// <summary>
        /// 发送 HTTP 请求
        /// </summary>
        private static async Task<HttpResponse> SendAsync(
            string url,
            HttpMethod method,
            object? body,
            HttpRequestConfig? config,
            CancellationToken cancellationToken)
        {
            var result = new HttpResponse();
            int retryCount = config?.RetryCount ?? 0;
            int retryInterval = config?.RetryInterval ?? 1000;

            for (int attempt = 0; attempt <= retryCount; attempt++)
            {
                try
                {
                    using var client = CreateClient(config);
                    using var request = CreateRequest(url, method, body, config);

                    using var response = await client.SendAsync(request, cancellationToken);
                    var content = await response.Content.ReadAsStringAsync();

                    result.IsSuccess = response.IsSuccessStatusCode;
                    result.StatusCode = response.StatusCode;
                    result.Content = content;

                    foreach (var header in response.Headers)
                    {
                        result.Headers[header.Key] = string.Join(",", header.Value);
                    }

                    if (result.IsSuccess || attempt == retryCount)
                    {
                        return result;
                    }
                }
                catch (Exception ex)
                {
                    result.Exception = ex;
                    result.ErrorMessage = ex.Message;

                    if (attempt == retryCount)
                    {
                        return result;
                    }
                }

                if (attempt < retryCount)
                {
                    await Task.Delay(retryInterval, cancellationToken);
                }
            }

            return result;
        }

        /// <summary>
        /// 发送 HTTP 请求并反序列化响应
        /// </summary>
        private static async Task<HttpResponse<T>> SendAsync<T>(
            string url,
            HttpMethod method,
            object? body,
            HttpRequestConfig? config,
            CancellationToken cancellationToken)
        {
            var response = await SendAsync(url, method, body, config, cancellationToken);
            var result = new HttpResponse<T>
            {
                IsSuccess = response.IsSuccess,
                StatusCode = response.StatusCode,
                Content = response.Content,
                Headers = response.Headers,
                ErrorMessage = response.ErrorMessage,
                Exception = response.Exception
            };

            if (response.IsSuccess && !string.IsNullOrEmpty(response.Content))
            {
                try
                {
                    result.Data = JsonSerializer.Deserialize<T>(response.Content, _jsonOptions);
                }
                catch (Exception ex)
                {
                    result.ErrorMessage = $"反序列化失败: {ex.Message}";
                }
            }

            return result;
        }

        private static HttpClient CreateClient(HttpRequestConfig? config)
        {
            if (config == null)
            {
                return _sharedClient.Value;
            }

            var handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                AllowAutoRedirect = config.AllowRedirect
            };

            var client = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromMilliseconds(config.Timeout)
            };

            // 添加请求头
            foreach (var header in config.Headers)
            {
                client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
            }

            // Basic 认证
            if (config.BasicAuth.HasValue)
            {
                var authValue = Convert.ToBase64String(
                    config.Encoding.GetBytes($"{config.BasicAuth.Value.Username}:{config.BasicAuth.Value.Password}"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);
            }

            // Bearer Token
            if (!string.IsNullOrEmpty(config.BearerToken))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", config.BearerToken);
            }

            return client;
        }

        private static HttpRequestMessage CreateRequest(
            string url,
            HttpMethod method,
            object? body,
            HttpRequestConfig? config)
        {
            var request = new HttpRequestMessage(method, BuildUrl(url, config));

            if (body != null)
            {
                string content;
                string contentType = config?.ContentType ?? "application/json";

                if (body is string str)
                {
                    content = str;
                }
                else if (body is Dictionary<string, string> formData &&
                         contentType.Contains("x-www-form-urlencoded"))
                {
                    content = string.Join("&", formData.Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));
                }
                else
                {
                    content = JsonSerializer.Serialize(body, _jsonOptions);
                }

                request.Content = new StringContent(content, config?.Encoding ?? Encoding.UTF8, contentType);
            }

            return request;
        }

        private static string BuildUrl(string url, HttpRequestConfig? config)
        {
            if (config?.QueryParams == null || config.QueryParams.Count == 0)
            {
                return url;
            }

            var queryParts = new List<string>();
            foreach (var param in config.QueryParams)
            {
                queryParts.Add($"{Uri.EscapeDataString(param.Key)}={Uri.EscapeDataString(param.Value)}");
            }

            var queryString = string.Join("&", queryParts);
            return url.Contains('?') ? $"{url}&{queryString}" : $"{url}?{queryString}";
        }

        private static string GetMimeType(string filePath)
        {
            var ext = Path.GetExtension(filePath).ToLowerInvariant();
            return ext switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".txt" => "text/plain",
                ".zip" => "application/zip",
                _ => "application/octet-stream"
            };
        }

        #endregion
    }
}
