using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EasyTool.NetCategory
{
    /// <summary>
    /// Server-Sent Events (SSE) 客户端
    /// 用于接收服务器推送的事件流
    /// </summary>
    public class SseClient : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly Uri _endpoint;
        private readonly Dictionary<string, string> _headers;
        private CancellationTokenSource? _cts;
        private bool _disposed;
        private bool _isConnected;

        /// <summary>
        /// 接收到事件时触发
        /// </summary>
        public event EventHandler<SseEvent>? EventReceived;

        /// <summary>
        /// 连接打开时触发
        /// </summary>
        public event EventHandler? Connected;

        /// <summary>
        /// 连接关闭时触发
        /// </summary>
        public event EventHandler<SseDisconnectEventArgs>? Disconnected;

        /// <summary>
        /// 发生错误时触发
        /// </summary>
        public event EventHandler<Exception>? Error;

        /// <summary>
        /// 是否已连接
        /// </summary>
        public bool IsConnected => _isConnected;

        /// <summary>
        /// 最后接收的事件 ID
        /// </summary>
        public string? LastEventId { get; private set; }

        /// <summary>
        /// 重连等待时间（毫秒）
        /// </summary>
        public int ReconnectDelay { get; set; } = 3000;

        /// <summary>
        /// 最大重连次数
        /// </summary>
        public int MaxReconnectAttempts { get; set; } = 5;

        /// <summary>
        /// 是否自动重连
        /// </summary>
        public bool AutoReconnect { get; set; } = true;

        /// <summary>
        /// 创建 SSE 客户端
        /// </summary>
        /// <param name="endpoint">SSE 端点 URL</param>
        /// <param name="headers">请求头</param>
        public SseClient(string endpoint, Dictionary<string, string>? headers = null)
            : this(new Uri(endpoint), headers)
        {
        }

        /// <summary>
        /// 创建 SSE 客户端
        /// </summary>
        /// <param name="endpoint">SSE 端点 URL</param>
        /// <param name="headers">请求头</param>
        public SseClient(Uri endpoint, Dictionary<string, string>? headers = null)
            : this(new HttpClient(), endpoint, headers)
        {
        }

        /// <summary>
        /// 创建 SSE 客户端
        /// </summary>
        /// <param name="httpClient">HttpClient 实例</param>
        /// <param name="endpoint">SSE 端点 URL</param>
        /// <param name="headers">请求头</param>
        public SseClient(HttpClient httpClient, Uri endpoint, Dictionary<string, string>? headers = null)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
            _headers = headers ?? new Dictionary<string, string>();
        }

        /// <summary>
        /// 连接并开始接收事件
        /// </summary>
        /// <param name="cancellationToken">取消令牌</param>
        public async Task ConnectAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();

            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            try
            {
                await ConnectInternalAsync(_cts.Token);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // 正常取消，不触发错误
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        public async Task DisconnectAsync()
        {
            if (_cts != null && !_cts.IsCancellationRequested)
            {
                _cts.Cancel();
            }

            _isConnected = false;
            OnDisconnected(null);
            await Task.CompletedTask;
        }

        /// <summary>
        /// 异步获取所有事件（直到连接关闭）
        /// </summary>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>事件集合</returns>
        public async IAsyncEnumerable<SseEvent> GetEventsAsync(
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var events = new System.Collections.Concurrent.BlockingCollection<SseEvent>();
            var completed = new TaskCompletionSource<bool>();

            EventReceived += (_, e) => events.Add(e);
            Disconnected += (_, _) => completed.TrySetResult(true);
            Error += (_, _) => completed.TrySetResult(true);

            _ = ConnectAsync(cancellationToken);

            while (!cancellationToken.IsCancellationRequested)
            {
                if (events.TryTake(out var sseEvent, 100, cancellationToken))
                {
                    yield return sseEvent;
                }

                if (completed.Task.IsCompleted)
                {
                    while (events.TryTake(out var remainingEvent))
                    {
                        yield return remainingEvent;
                    }
                    break;
                }
            }
        }

        private async Task ConnectInternalAsync(CancellationToken cancellationToken)
        {
            var reconnectAttempts = 0;

            while (!cancellationToken.IsCancellationRequested && (reconnectAttempts < MaxReconnectAttempts || !AutoReconnect))
            {
                try
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, _endpoint);
                    request.Headers.Accept.ParseAdd("text/event-stream");
                    request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

                    foreach (var header in _headers)
                    {
                        request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }

                    // 添加 Last-Event-ID 头
                    if (!string.IsNullOrEmpty(LastEventId))
                    {
                        request.Headers.TryAddWithoutValidation("Last-Event-ID", LastEventId);
                    }

                    using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                    response.EnsureSuccessStatusCode();

                    _isConnected = true;
                    reconnectAttempts = 0;
                    OnConnected();

#if NETSTANDARD2_1
                    using var stream = await response.Content.ReadAsStreamAsync();
#else
                    using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
#endif
                    using var reader = new StreamReader(stream, Encoding.UTF8, true, 1024, true);

                    await ProcessEventStreamAsync(reader, cancellationToken);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    OnError(ex);
                }

                _isConnected = false;

                if (AutoReconnect && reconnectAttempts < MaxReconnectAttempts && !cancellationToken.IsCancellationRequested)
                {
                    reconnectAttempts++;
                    OnDisconnected(reconnectAttempts);

                    try
                    {
                        await Task.Delay(ReconnectDelay * reconnectAttempts, cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
        }

        private async Task ProcessEventStreamAsync(StreamReader reader, CancellationToken cancellationToken)
        {
            var currentEvent = new SseEventBuilder();

            while (!cancellationToken.IsCancellationRequested)
            {
                var line = await reader.ReadLineAsync();
                if (line == null)
                {
                    // 流结束
                    if (currentEvent.HasData)
                    {
                        OnEventReceived(currentEvent.Build());
                    }
                    break;
                }

                if (string.IsNullOrEmpty(line))
                {
                    // 空行表示事件结束
                    if (currentEvent.HasData)
                    {
                        var sseEvent = currentEvent.Build();
                        LastEventId = sseEvent.Id;
                        OnEventReceived(sseEvent);
                        currentEvent = new SseEventBuilder();
                    }
                    continue;
                }

                if (line.StartsWith(':'))
                {
                    // 注释行，忽略
                    continue;
                }

                var colonIndex = line.IndexOf(':');
                string field, value;

                if (colonIndex < 0)
                {
                    field = line;
                    value = string.Empty;
                }
                else
                {
                    field = line[..colonIndex];
                    value = line[(colonIndex + 1)..];
                    if (value.StartsWith(' '))
                    {
                        value = value[1..];
                    }
                }

                switch (field)
                {
                    case "event":
                        currentEvent.EventType = value;
                        break;
                    case "data":
                        currentEvent.AppendData(value);
                        break;
                    case "id":
                        currentEvent.Id = value;
                        break;
                    case "retry":
                        if (int.TryParse(value, out var retryMs))
                        {
                            ReconnectDelay = retryMs;
                        }
                        break;
                }
            }
        }

        protected virtual void OnEventReceived(SseEvent sseEvent)
        {
            EventReceived?.Invoke(this, sseEvent);
        }

        protected virtual void OnConnected()
        {
            Connected?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnDisconnected(int? reconnectAttempt)
        {
            Disconnected?.Invoke(this, new SseDisconnectEventArgs(reconnectAttempt));
        }

        protected virtual void OnError(Exception ex)
        {
            Error?.Invoke(this, ex);
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(SseClient));
            }
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _cts?.Cancel();
            _cts?.Dispose();
            _disposed = true;
        }
    }

    /// <summary>
    /// SSE 事件
    /// </summary>
    public class SseEvent
    {
        /// <summary>
        /// 事件 ID
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// 事件类型
        /// </summary>
        public string? EventType { get; set; }

        /// <summary>
        /// 事件数据
        /// </summary>
        public string Data { get; set; } = string.Empty;

        /// <summary>
        /// 接收时间
        /// </summary>
        public DateTimeOffset ReceivedAt { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// 尝试解析数据为 JSON
        /// </summary>
        public T? ParseJson<T>()
        {
            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<T>(Data);
            }
            catch
            {
                return default;
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(Id))
                sb.Append($"[{Id}] ");
            if (!string.IsNullOrEmpty(EventType))
                sb.Append($"({EventType}) ");
            sb.Append(Data);
            return sb.ToString();
        }
    }

    /// <summary>
    /// SSE 断开连接事件参数
    /// </summary>
    public class SseDisconnectEventArgs : EventArgs
    {
        /// <summary>
        /// 重连尝试次数（如果正在重连）
        /// </summary>
        public int? ReconnectAttempt { get; }

        public SseDisconnectEventArgs(int? reconnectAttempt)
        {
            ReconnectAttempt = reconnectAttempt;
        }
    }

    /// <summary>
    /// SSE 事件构建器
    /// </summary>
    internal class SseEventBuilder
    {
        public string? Id { get; set; }
        public string? EventType { get; set; }
        private readonly StringBuilder _data = new();

        public bool HasData => _data.Length > 0;

        public void AppendData(string data)
        {
            if (_data.Length > 0)
            {
                _data.AppendLine();
            }
            _data.Append(data);
        }

        public SseEvent Build()
        {
            return new SseEvent
            {
                Id = Id,
                EventType = EventType,
                Data = _data.ToString()
            };
        }
    }

    /// <summary>
    /// SSE 客户端扩展方法
    /// </summary>
    public static class SseClientExtensions
    {
        /// <summary>
        /// 创建 SSE 客户端
        /// </summary>
        /// <param name="endpoint">SSE 端点 URL</param>
        /// <param name="headers">请求头</param>
        /// <returns>SSE 客户端实例</returns>
        public static SseClient CreateSseClient(string endpoint, Dictionary<string, string>? headers = null)
        {
            return new SseClient(endpoint, headers);
        }

        /// <summary>
        /// 创建 SSE 客户端（带认证）
        /// </summary>
        /// <param name="endpoint">SSE 端点 URL</param>
        /// <param name="bearerToken">Bearer Token</param>
        /// <returns>SSE 客户端实例</returns>
        public static SseClient CreateSseClientWithAuth(string endpoint, string bearerToken)
        {
            return new SseClient(endpoint, new Dictionary<string, string>
            {
                ["Authorization"] = $"Bearer {bearerToken}"
            });
        }

        /// <summary>
        /// 异步获取单个 SSE 事件
        /// </summary>
        /// <param name="endpoint">SSE 端点 URL</param>
        /// <param name="timeout">超时时间</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>SSE 事件</returns>
        public static async Task<SseEvent?> GetSingleEventAsync(string endpoint, TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(timeout);

            using var client = new SseClient(endpoint);
            SseEvent? result = null;
            var tcs = new TaskCompletionSource<SseEvent>();

            client.EventReceived += (_, e) =>
            {
                result = e;
                tcs.TrySetResult(e);
            };

            _ = client.ConnectAsync(cts.Token);

            var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(timeout, cts.Token));

            await client.DisconnectAsync();

            return completedTask == tcs.Task ? result : null;
        }
    }
}
