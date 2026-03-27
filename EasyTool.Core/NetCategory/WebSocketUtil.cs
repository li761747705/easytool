using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EasyTool.NetCategory
{
    /// <summary>
    /// WebSocket客户端配置
    /// </summary>
    public class WebSocketClientOptions
    {
        /// <summary>
        /// 连接超时时间
        /// </summary>
        public TimeSpan ConnectTimeout { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// 接收缓冲区大小
        /// </summary>
        public int ReceiveBufferSize { get; set; } = 8192;

        /// <summary>
        /// 发送缓冲区大小
        /// </summary>
        public int SendBufferSize { get; set; } = 8192;

        /// <summary>
        /// 是否保持连接
        /// </summary>
        public bool KeepAlive { get; set; } = true;

        /// <summary>
        /// 保持连接间隔
        /// </summary>
        public TimeSpan KeepAliveInterval { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// 请求头
        /// </summary>
        public Dictionary<string, string> Headers { get; set; } = new();

        /// <summary>
        /// 子协议
        /// </summary>
        public List<string>? SubProtocols { get; set; }
    }

    /// <summary>
    /// WebSocket消息
    /// </summary>
    public class WebSocketMessage
    {
        /// <summary>
        /// 消息类型
        /// </summary>
        public WebSocketMessageType MessageType { get; set; }

        /// <summary>
        /// 文本内容
        /// </summary>
        public string? Text { get; set; }

        /// <summary>
        /// 二进制内容
        /// </summary>
        public byte[]? Binary { get; set; }

        /// <summary>
        /// 是否为结束消息
        /// </summary>
        public bool EndOfMessage { get; set; } = true;
    }

    /// <summary>
    /// WebSocket客户端
    /// </summary>
    public class WebSocketClient : IDisposable
    {
        private readonly ClientWebSocket _webSocket;
        private readonly WebSocketClientOptions _options;
        private readonly CancellationTokenSource _cts;
        private readonly ConcurrentQueue<WebSocketMessage> _sendQueue = new();
        private Task? _receiveTask;
        private Task? _sendTask;
        private bool _disposed;

        /// <summary>
        /// 是否已连接
        /// </summary>
        public bool IsConnected => _webSocket.State == WebSocketState.Open;

        /// <summary>
        /// 当前状态
        /// </summary>
        public WebSocketState State => _webSocket.State;

        /// <summary>
        /// 接收到消息时触发
        /// </summary>
        public event Action<WebSocketMessage>? OnMessage;

        /// <summary>
        /// 连接关闭时触发
        /// </summary>
        public event Action<WebSocketCloseStatus?, string?>? OnClosed;

        /// <summary>
        /// 发生错误时触发
        /// </summary>
        public event Action<Exception>? OnError;

        /// <summary>
        /// 创建WebSocket客户端
        /// </summary>
        /// <param name="options">配置</param>
        public WebSocketClient(WebSocketClientOptions? options = null)
        {
            _options = options ?? new WebSocketClientOptions();
            _webSocket = new ClientWebSocket();
            _cts = new CancellationTokenSource();

            // 设置子协议
            if (_options.SubProtocols != null)
            {
                foreach (var protocol in _options.SubProtocols)
                {
                    _webSocket.Options.AddSubProtocol(protocol);
                }
            }

            // 设置请求头
            foreach (var header in _options.Headers)
            {
                _webSocket.Options.SetRequestHeader(header.Key, header.Value);
            }

            _webSocket.Options.KeepAliveInterval = _options.KeepAlive ? _options.KeepAliveInterval : TimeSpan.Zero;
        }

        /// <summary>
        /// 连接到服务器
        /// </summary>
        /// <param name="uri">服务器地址</param>
        /// <param name="cancellationToken">取消令牌</param>
        public async Task ConnectAsync(Uri uri, CancellationToken cancellationToken = default)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cts.Token);
            cts.CancelAfter(_options.ConnectTimeout);

            await _webSocket.ConnectAsync(uri, cts.Token);

            // 启动接收和发送任务
            _receiveTask = ReceiveLoopAsync(_cts.Token);
            _sendTask = SendLoopAsync(_cts.Token);
        }

        /// <summary>
        /// 连接到服务器
        /// </summary>
        /// <param name="url">服务器地址</param>
        /// <param name="cancellationToken">取消令牌</param>
        public async Task ConnectAsync(string url, CancellationToken cancellationToken = default)
        {
            await ConnectAsync(new Uri(url), cancellationToken);
        }

        /// <summary>
        /// 发送文本消息
        /// </summary>
        /// <param name="message">消息内容</param>
        public void Send(string message)
        {
            _sendQueue.Enqueue(new WebSocketMessage
            {
                MessageType = WebSocketMessageType.Text,
                Text = message
            });
        }

        /// <summary>
        /// 发送二进制消息
        /// </summary>
        /// <param name="data">二进制数据</param>
        public void Send(byte[] data)
        {
            _sendQueue.Enqueue(new WebSocketMessage
            {
                MessageType = WebSocketMessageType.Binary,
                Binary = data
            });
        }

        /// <summary>
        /// 异步发送文本消息
        /// </summary>
        /// <param name="message">消息内容</param>
        /// <param name="cancellationToken">取消令牌</param>
        public async Task SendAsync(string message, CancellationToken cancellationToken = default)
        {
            var bytes = Encoding.UTF8.GetBytes(message);
            await _webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, cancellationToken);
        }

        /// <summary>
        /// 异步发送二进制消息
        /// </summary>
        /// <param name="data">二进制数据</param>
        /// <param name="cancellationToken">取消令牌</param>
        public async Task SendAsync(byte[] data, CancellationToken cancellationToken = default)
        {
            await _webSocket.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Binary, true, cancellationToken);
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        /// <param name="closeStatus">关闭状态</param>
        /// <param name="reason">关闭原因</param>
        /// <param name="cancellationToken">取消令牌</param>
        public async Task CloseAsync(WebSocketCloseStatus closeStatus = WebSocketCloseStatus.NormalClosure, string reason = "", CancellationToken cancellationToken = default)
        {
            if (_webSocket.State == WebSocketState.Open)
            {
                await _webSocket.CloseAsync(closeStatus, reason, cancellationToken);
            }
            _cts.Cancel();
        }

        private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
        {
            var buffer = new byte[_options.ReceiveBufferSize];

            try
            {
                while (!cancellationToken.IsCancellationRequested && _webSocket.State == WebSocketState.Open)
                {
                    var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await CloseAsync(WebSocketCloseStatus.NormalClosure, "Server closed", cancellationToken);
                        OnClosed?.Invoke(result.CloseStatus, result.CloseStatusDescription);
                        break;
                    }

                    var message = new WebSocketMessage
                    {
                        MessageType = result.MessageType,
                        EndOfMessage = result.EndOfMessage
                    };

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        message.Text = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    }
                    else
                    {
                        message.Binary = new byte[result.Count];
                        Array.Copy(buffer, message.Binary, result.Count);
                    }

                    OnMessage?.Invoke(message);
                }
            }
            catch (OperationCanceledException)
            {
                // 正常取消
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex);
            }
        }

        private async Task SendLoopAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested && _webSocket.State == WebSocketState.Open)
                {
                    if (_sendQueue.TryDequeue(out var message))
                    {
                        if (message.MessageType == WebSocketMessageType.Text && message.Text != null)
                        {
                            var bytes = Encoding.UTF8.GetBytes(message.Text);
                            await _webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, message.EndOfMessage, cancellationToken);
                        }
                        else if (message.MessageType == WebSocketMessageType.Binary && message.Binary != null)
                        {
                            await _webSocket.SendAsync(new ArraySegment<byte>(message.Binary), WebSocketMessageType.Binary, message.EndOfMessage, cancellationToken);
                        }
                    }
                    else
                    {
                        await Task.Delay(10, cancellationToken);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // 正常取消
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex);
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _cts.Cancel();
                _webSocket.Dispose();
                _cts.Dispose();
            }
        }
    }

    /// <summary>
    /// WebSocket工具类
    /// </summary>
    public static class WebSocketUtil
    {
        /// <summary>
        /// 创建WebSocket客户端
        /// </summary>
        /// <param name="options">配置</param>
        /// <returns>客户端实例</returns>
        public static WebSocketClient CreateClient(WebSocketClientOptions? options = null)
        {
            return new WebSocketClient(options);
        }

        /// <summary>
        /// 连接并发送消息（一次性通信）
        /// </summary>
        /// <param name="url">服务器地址</param>
        /// <param name="message">消息内容</param>
        /// <param name="timeout">超时时间</param>
        /// <returns>响应消息列表</returns>
        public static async Task<List<WebSocketMessage>> SendAndReceiveAsync(string url, string message, TimeSpan? timeout = null)
        {
            var responses = new List<WebSocketMessage>();
            var tcs = new TaskCompletionSource<bool>();

            using var client = new WebSocketClient();
            client.OnMessage += msg =>
            {
                responses.Add(msg);
                if (msg.EndOfMessage)
                {
                    tcs.TrySetResult(true);
                }
            };

            await client.ConnectAsync(url);

            using var cts = new CancellationTokenSource(timeout ?? TimeSpan.FromSeconds(30));
            cts.Token.Register(() => tcs.TrySetCanceled());

            client.Send(message);

            await Task.WhenAny(tcs.Task, Task.Delay(timeout ?? TimeSpan.FromSeconds(30)));
            await client.CloseAsync();

            return responses;
        }

        /// <summary>
        /// 检查WebSocket URL是否有效
        /// </summary>
        /// <param name="url">URL字符串</param>
        /// <returns>是否有效</returns>
        public static bool IsValidWebSocketUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                return false;

            return uri.Scheme == "ws" || uri.Scheme == "wss";
        }

        /// <summary>
        /// 获取WebSocket状态描述
        /// </summary>
        /// <param name="state">状态</param>
        /// <returns>状态描述</returns>
        public static string GetStateDescription(WebSocketState state)
        {
            return state switch
            {
                WebSocketState.None => "无连接",
                WebSocketState.Connecting => "连接中",
                WebSocketState.Open => "已连接",
                WebSocketState.CloseSent => "已发送关闭请求",
                WebSocketState.CloseReceived => "已接收关闭请求",
                WebSocketState.Closed => "已关闭",
                WebSocketState.Aborted => "已中止",
                _ => "未知状态"
            };
        }
    }
}
