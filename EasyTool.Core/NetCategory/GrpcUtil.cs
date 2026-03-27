using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EasyTool.NetCategory
{
    /// <summary>
    /// gRPC 配置选项
    /// </summary>
    public class GrpcOptions
    {
        /// <summary>
        /// 服务地址
        /// </summary>
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// 是否使用 SSL
        /// </summary>
        public bool UseSsl { get; set; } = true;

        /// <summary>
        /// 是否忽略 SSL 证书错误
        /// </summary>
        public bool IgnoreSslErrors { get; set; }

        /// <summary>
        /// 最大接收消息大小（字节）
        /// </summary>
        public int? MaxReceiveMessageSize { get; set; }

        /// <summary>
        /// 最大发送消息大小（字节）
        /// </summary>
        public int? MaxSendMessageSize { get; set; }

        /// <summary>
        /// 超时时间
        /// </summary>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// 请求头
        /// </summary>
        public Dictionary<string, string> Headers { get; set; } = new();

        /// <summary>
        /// 是否启用压缩
        /// </summary>
        public bool EnableCompression { get; set; }

        /// <summary>
        /// 重试次数
        /// </summary>
        public int RetryCount { get; set; }

        /// <summary>
        /// 重试延迟
        /// </summary>
        public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(1);
    }

    /// <summary>
    /// gRPC 工具类
    /// 注意：此类提供 gRPC 调用的抽象接口，实际使用需要引入 Grpc.Net.Client 包
    /// </summary>
    public static class GrpcUtil
    {
        /// <summary>
        /// 创建 gRPC 通道配置
        /// </summary>
        /// <param name="options">gRPC 配置</param>
        /// <returns>配置对象</returns>
        public static GrpcChannelConfiguration CreateChannelConfiguration(GrpcOptions options)
        {
            return new GrpcChannelConfiguration
            {
                Address = options.Address,
                UseSsl = options.UseSsl,
                IgnoreSslErrors = options.IgnoreSslErrors,
                MaxReceiveMessageSize = options.MaxReceiveMessageSize,
                MaxSendMessageSize = options.MaxSendMessageSize,
                Timeout = options.Timeout,
                Headers = options.Headers,
                EnableCompression = options.EnableCompression
            };
        }

        /// <summary>
        /// 构建 gRPC 服务 URL
        /// </summary>
        /// <param name="host">主机地址</param>
        /// <param name="port">端口</param>
        /// <param name="useSsl">是否使用 SSL</param>
        /// <returns>服务 URL</returns>
        public static string BuildServiceUrl(string host, int port, bool useSsl = true)
        {
            var scheme = useSsl ? "https" : "http";
            return $"{scheme}://{host}:{port}";
        }

        /// <summary>
        /// 创建 gRPC 元数据
        /// </summary>
        /// <param name="headers">请求头</param>
        /// <returns>元数据</returns>
        public static GrpcMetadata CreateMetadata(Dictionary<string, string> headers)
        {
            return new GrpcMetadata
            {
                Headers = headers
            };
        }

        /// <summary>
        /// 创建带认证的元数据
        /// </summary>
        /// <param name="token">Bearer Token</param>
        /// <param name="additionalHeaders">额外请求头</param>
        /// <returns>元数据</returns>
        public static GrpcMetadata CreateAuthenticatedMetadata(string token, Dictionary<string, string>? additionalHeaders = null)
        {
            var headers = additionalHeaders ?? new Dictionary<string, string>();
            headers["Authorization"] = $"Bearer {token}";
            return new GrpcMetadata { Headers = headers };
        }

        /// <summary>
        /// 创建 API Key 认证元数据
        /// </summary>
        /// <param name="apiKey">API Key</param>
        /// <param name="headerName">请求头名称</param>
        /// <returns>元数据</returns>
        public static GrpcMetadata CreateApiKeyMetadata(string apiKey, string headerName = "x-api-key")
        {
            return new GrpcMetadata
            {
                Headers = new Dictionary<string, string>
                {
                    [headerName] = apiKey
                }
            };
        }

        /// <summary>
        /// 执行带重试的 gRPC 调用
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="call">gRPC 调用</param>
        /// <param name="retryCount">重试次数</param>
        /// <param name="retryDelay">重试延迟</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>调用结果</returns>
        public static async Task<T> ExecuteWithRetryAsync<T>(
            Func<Task<T>> call,
            int retryCount = 3,
            TimeSpan? retryDelay = null,
            CancellationToken cancellationToken = default)
        {
            var delay = retryDelay ?? TimeSpan.FromSeconds(1);
            Exception? lastException = null;

            for (int i = 0; i <= retryCount; i++)
            {
                try
                {
                    return await call();
                }
                catch (Exception ex) when (IsRetryableError(ex))
                {
                    lastException = ex;

                    if (i < retryCount)
                    {
                        await Task.Delay(delay, cancellationToken);
                    }
                }
            }

            throw lastException ?? new Exception("gRPC 调用失败");
        }

        /// <summary>
        /// 执行带超时的 gRPC 调用
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="call">gRPC 调用</param>
        /// <param name="timeout">超时时间</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>调用结果</returns>
        public static async Task<T> ExecuteWithTimeoutAsync<T>(
            Func<CancellationToken, Task<T>> call,
            TimeSpan timeout,
            CancellationToken cancellationToken = default)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(timeout);

            try
            {
                return await call(cts.Token);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                throw new TimeoutException($"gRPC 调用超时: {timeout}");
            }
        }

        private static bool IsRetryableError(Exception ex)
        {
            // 判断是否为可重试的错误
            var message = ex.Message.ToLowerInvariant();
            return message.Contains("unavailable") ||
                   message.Contains("deadline exceeded") ||
                   message.Contains("resource exhausted") ||
                   message.Contains("internal") ||
                   message.Contains("unknown");
        }
    }

    /// <summary>
    /// gRPC 通道配置
    /// </summary>
    public class GrpcChannelConfiguration
    {
        /// <summary>
        /// 服务地址
        /// </summary>
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// 是否使用 SSL
        /// </summary>
        public bool UseSsl { get; set; } = true;

        /// <summary>
        /// 是否忽略 SSL 证书错误
        /// </summary>
        public bool IgnoreSslErrors { get; set; }

        /// <summary>
        /// 最大接收消息大小
        /// </summary>
        public int? MaxReceiveMessageSize { get; set; }

        /// <summary>
        /// 最大发送消息大小
        /// </summary>
        public int? MaxSendMessageSize { get; set; }

        /// <summary>
        /// 超时时间
        /// </summary>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// 请求头
        /// </summary>
        public Dictionary<string, string> Headers { get; set; } = new();

        /// <summary>
        /// 是否启用压缩
        /// </summary>
        public bool EnableCompression { get; set; }
    }

    /// <summary>
    /// gRPC 元数据
    /// </summary>
    public class GrpcMetadata
    {
        /// <summary>
        /// 请求头
        /// </summary>
        public Dictionary<string, string> Headers { get; set; } = new();

        /// <summary>
        /// 添加请求头
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public void Add(string key, string value)
        {
            Headers[key] = value;
        }

        /// <summary>
        /// 获取请求头
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>值</returns>
        public string? Get(string key)
        {
            return Headers.TryGetValue(key, out var value) ? value : null;
        }
    }

    /// <summary>
    /// gRPC 响应状态
    /// </summary>
    public class GrpcResponseStatus
    {
        /// <summary>
        /// 状态码
        /// </summary>
        public GrpcStatusCode StatusCode { get; set; }

        /// <summary>
        /// 错误详情
        /// </summary>
        public string? Detail { get; set; }

        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccess => StatusCode == GrpcStatusCode.OK;

        /// <summary>
        /// 创建成功状态
        /// </summary>
        public static GrpcResponseStatus Success => new() { StatusCode = GrpcStatusCode.OK };

        /// <summary>
        /// 创建错误状态
        /// </summary>
        public static GrpcResponseStatus Error(GrpcStatusCode code, string detail) => new()
        {
            StatusCode = code,
            Detail = detail
        };
    }

    /// <summary>
    /// gRPC 状态码
    /// </summary>
    public enum GrpcStatusCode
    {
        /// <summary>
        /// 成功
        /// </summary>
        OK = 0,

        /// <summary>
        /// 取消
        /// </summary>
        Cancelled = 1,

        /// <summary>
        /// 未知错误
        /// </summary>
        Unknown = 2,

        /// <summary>
        /// 参数无效
        /// </summary>
        InvalidArgument = 3,

        /// <summary>
        /// 超时
        /// </summary>
        DeadlineExceeded = 4,

        /// <summary>
        /// 未找到
        /// </summary>
        NotFound = 5,

        /// <summary>
        /// 已存在
        /// </summary>
        AlreadyExists = 6,

        /// <summary>
        /// 权限不足
        /// </summary>
        PermissionDenied = 7,

        /// <summary>
        /// 资源耗尽
        /// </summary>
        ResourceExhausted = 8,

        /// <summary>
        /// 前置条件失败
        /// </summary>
        FailedPrecondition = 9,

        /// <summary>
        /// 请求中止
        /// </summary>
        Aborted = 10,

        /// <summary>
        /// 超出范围
        /// </summary>
        OutOfRange = 11,

        /// <summary>
        /// 未实现
        /// </summary>
        Unimplemented = 12,

        /// <summary>
        /// 内部错误
        /// </summary>
        Internal = 13,

        /// <summary>
        /// 不可用
        /// </summary>
        Unavailable = 14,

        /// <summary>
        /// 数据丢失
        /// </summary>
        DataLoss = 15,

        /// <summary>
        /// 未认证
        /// </summary>
        Unauthenticated = 16
    }
}
