using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EasyTool.QueueCategory
{
    /// <summary>
    /// 消息类型
    /// </summary>
    public enum MessageType
    {
        /// <summary>
        /// 普通消息
        /// </summary>
        Normal,

        /// <summary>
        /// 延迟消息
        /// </summary>
        Delayed,

        /// <summary>
        /// 优先级消息
        /// </summary>
        Priority
    }

    /// <summary>
    /// 消息封装
    /// </summary>
    /// <typeparam name="T">消息体类型</typeparam>
    public class Message<T>
    {
        /// <summary>
        /// 消息ID
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString("N");

        /// <summary>
        /// 消息体
        /// </summary>
        public T Body { get; set; } = default!;

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 过期时间
        /// </summary>
        public DateTime? ExpireTime { get; set; }

        /// <summary>
        /// 延迟执行时间
        /// </summary>
        public DateTime? DelayTo { get; set; }

        /// <summary>
        /// 优先级（越大越优先）
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// 重试次数
        /// </summary>
        public int RetryCount { get; set; }

        /// <summary>
        /// 最大重试次数
        /// </summary>
        public int MaxRetryCount { get; set; } = 3;

        /// <summary>
        /// 消息头
        /// </summary>
        public Dictionary<string, string> Headers { get; set; } = new();

        /// <summary>
        /// 是否已过期
        /// </summary>
        public bool IsExpired => ExpireTime.HasValue && DateTime.UtcNow >= ExpireTime.Value;

        /// <summary>
        /// 是否可以处理（延迟消息检查）
        /// </summary>
        public bool CanProcess => DelayTo == null || DateTime.UtcNow >= DelayTo.Value;
    }

    /// <summary>
    /// 消息队列选项
    /// </summary>
    public class MessageQueueOptions
    {
        /// <summary>
        /// 队列名称
        /// </summary>
        public string Name { get; set; } = "default";

        /// <summary>
        /// 最大容量
        /// </summary>
        public int MaxCapacity { get; set; } = 10000;

        /// <summary>
        /// 消费者数量
        /// </summary>
        public int ConsumerCount { get; set; } = 1;

        /// <summary>
        /// 默认消息过期时间
        /// </summary>
        public TimeSpan? DefaultMessageTtl { get; set; }

        /// <summary>
        /// 默认重试延迟
        /// </summary>
        public TimeSpan DefaultRetryDelay { get; set; } = TimeSpan.FromSeconds(5);

        /// <summary>
        /// 死信队列是否启用
        /// </summary>
        public bool EnableDeadLetterQueue { get; set; } = true;

        /// <summary>
        /// 是否启用持久化
        /// </summary>
        public bool EnablePersistence { get; set; } = false;

        /// <summary>
        /// 持久化文件路径
        /// </summary>
        public string? PersistenceFilePath { get; set; }
    }

    /// <summary>
    /// 内存消息队列
    /// </summary>
    /// <typeparam name="T">消息体类型</typeparam>
    public class MessageQueue<T> : IDisposable
    {
        private readonly MessageQueueOptions _options;
        private readonly ConcurrentQueue<Message<T>> _normalQueue;
        private readonly PriorityQueue<Message<T>, int> _priorityQueue;
        private readonly ConcurrentQueue<Message<T>> _delayedQueue;
        private readonly ConcurrentQueue<Message<T>> _deadLetterQueue;
        private readonly SemaphoreSlim _signal;
        private readonly CancellationTokenSource _cts;
        private readonly List<Task> _consumerTasks;
        private readonly Func<Message<T>, Task<ProcessResult>> _handler;
        private bool _disposed;
        private bool _started;

        /// <summary>
        /// 队列名称
        /// </summary>
        public string Name => _options.Name;

        /// <summary>
        /// 普通队列长度
        /// </summary>
        public int NormalQueueCount => _normalQueue.Count;

        /// <summary>
        /// 优先级队列长度
        /// </summary>
        public int PriorityQueueCount => _priorityQueue.Count;

        /// <summary>
        /// 延迟队列长度
        /// </summary>
        public int DelayedQueueCount => _delayedQueue.Count;

        /// <summary>
        /// 死信队列长度
        /// </summary>
        public int DeadLetterQueueCount => _deadLetterQueue.Count;

        /// <summary>
        /// 创建消息队列
        /// </summary>
        /// <param name="handler">消息处理器</param>
        /// <param name="options">队列选项</param>
        public MessageQueue(Func<Message<T>, Task<ProcessResult>> handler, MessageQueueOptions? options = null)
        {
            _options = options ?? new MessageQueueOptions();
            _handler = handler;
            _normalQueue = new ConcurrentQueue<Message<T>>();
            _priorityQueue = new PriorityQueue<Message<T>, int>();
            _delayedQueue = new ConcurrentQueue<Message<T>>();
            _deadLetterQueue = new ConcurrentQueue<Message<T>>();
            _signal = new SemaphoreSlim(0);
            _cts = new CancellationTokenSource();
            _consumerTasks = new List<Task>();
        }

        /// <summary>
        /// 启动队列消费者
        /// </summary>
        public void Start()
        {
            if (_started)
                return;

            _started = true;

            for (int i = 0; i < _options.ConsumerCount; i++)
            {
                _consumerTasks.Add(ConsumeAsync(_cts.Token));
            }

            // 启动延迟消息检查任务
            _consumerTasks.Add(ProcessDelayedMessagesAsync(_cts.Token));
        }

        /// <summary>
        /// 停止队列消费者
        /// </summary>
        /// <param name="waitForCompletion">是否等待处理完成</param>
        public async Task StopAsync(bool waitForCompletion = true)
        {
            _cts.Cancel();

            if (waitForCompletion)
            {
                await Task.WhenAll(_consumerTasks);
            }
        }

        /// <summary>
        /// 发布消息
        /// </summary>
        /// <param name="body">消息体</param>
        /// <param name="type">消息类型</param>
        /// <param name="priority">优先级</param>
        /// <param name="delay">延迟时间</param>
        /// <returns>消息ID</returns>
        public string Publish(T body, MessageType type = MessageType.Normal, int priority = 0, TimeSpan? delay = null)
        {
            var message = new Message<T>
            {
                Body = body,
                Priority = priority,
                ExpireTime = _options.DefaultMessageTtl.HasValue
                    ? DateTime.UtcNow.Add(_options.DefaultMessageTtl.Value)
                    : null
            };

            if (delay.HasValue)
            {
                message.DelayTo = DateTime.UtcNow.Add(delay.Value);
                type = MessageType.Delayed;
            }

            switch (type)
            {
                case MessageType.Priority:
                    _priorityQueue.Enqueue(message, -priority); // 负数让高优先级先出
                    break;
                case MessageType.Delayed:
                    _delayedQueue.Enqueue(message);
                    break;
                default:
                    _normalQueue.Enqueue(message);
                    break;
            }

            _signal.Release();
            return message.Id;
        }

        /// <summary>
        /// 批量发布消息
        /// </summary>
        /// <param name="bodies">消息体集合</param>
        /// <returns>消息ID列表</returns>
        public List<string> PublishMany(IEnumerable<T> bodies)
        {
            var ids = new List<string>();

            foreach (var body in bodies)
            {
                ids.Add(Publish(body));
            }

            return ids;
        }

        /// <summary>
        /// 获取死信队列消息
        /// </summary>
        /// <returns>消息列表</returns>
        public List<Message<T>> GetDeadLetterMessages()
        {
            var messages = new List<Message<T>>();

            while (_deadLetterQueue.TryDequeue(out var message))
            {
                messages.Add(message);
            }

            return messages;
        }

        /// <summary>
        /// 重试死信消息
        /// </summary>
        public void RetryDeadLetterMessages()
        {
            var messages = GetDeadLetterMessages();

            foreach (var message in messages)
            {
                message.RetryCount = 0;
                Publish(message.Body);
            }
        }

        private async Task ConsumeAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await _signal.WaitAsync(cancellationToken);

                    Message<T>? message = null;

                    // 优先处理优先级队列
                    if (_priorityQueue.TryDequeue(out var priorityMessage, out _))
                    {
                        message = priorityMessage;
                    }
                    // 再处理普通队列
                    else if (_normalQueue.TryDequeue(out var normalMessage))
                    {
                        message = normalMessage;
                    }

                    if (message == null)
                        continue;

                    // 检查是否过期
                    if (message.IsExpired)
                        continue;

                    // 处理消息
                    var result = await _handler(message);

                    switch (result.Action)
                    {
                        case ProcessAction.Complete:
                            // 消息处理完成，无需操作
                            break;

                        case ProcessAction.Retry:
                            message.RetryCount++;
                            if (message.RetryCount < message.MaxRetryCount)
                            {
                                await Task.Delay(_options.DefaultRetryDelay, cancellationToken);
                                Publish(message.Body, MessageType.Normal);
                            }
                            else if (_options.EnableDeadLetterQueue)
                            {
                                _deadLetterQueue.Enqueue(message);
                            }
                            break;

                        case ProcessAction.DeadLetter:
                            if (_options.EnableDeadLetterQueue)
                            {
                                _deadLetterQueue.Enqueue(message);
                            }
                            break;
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception)
                {
                    // 记录错误，继续处理
                }
            }
        }

        private async Task ProcessDelayedMessagesAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);

                    var now = DateTime.UtcNow;
                    var readyMessages = new List<Message<T>>();

                    // 检查延迟消息
                    while (_delayedQueue.TryDequeue(out var message))
                    {
                        if (message.CanProcess && !message.IsExpired)
                        {
                            readyMessages.Add(message);
                        }
                        else if (!message.IsExpired)
                        {
                            // 未到处理时间，重新入队
                            _delayedQueue.Enqueue(message);
                            break;
                        }
                    }

                    // 将就绪的消息发送到普通队列
                    foreach (var message in readyMessages)
                    {
                        _normalQueue.Enqueue(message);
                        _signal.Release();
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                // 自动保存持久化
                if (_options.EnablePersistence)
                {
                    SaveToPersistenceAsync().GetAwaiter().GetResult();
                }
                _cts.Cancel();
                _signal.Dispose();
                _cts.Dispose();
                _disposed = true;
            }
        }

        #region 持久化

        /// <summary>
        /// 保存消息到持久化文件
        /// </summary>
        public async Task SaveToPersistenceAsync()
        {
            if (!_options.EnablePersistence || string.IsNullOrEmpty(_options.PersistenceFilePath))
                return;

            var allMessages = new List<Message<T>>();

            // 收集所有队列中的消息
            while (_normalQueue.TryDequeue(out var msg)) allMessages.Add(msg);
            while (_priorityQueue.TryDequeue(out var pMsg, out _)) allMessages.Add(pMsg);
            while (_delayedQueue.TryDequeue(out var dMsg)) allMessages.Add(dMsg);

            try
            {
                var json = System.Text.Json.JsonSerializer.Serialize(allMessages);
                var directory = System.IO.Path.GetDirectoryName(_options.PersistenceFilePath);
                if (!string.IsNullOrEmpty(directory) && !System.IO.Directory.Exists(directory))
                {
                    System.IO.Directory.CreateDirectory(directory);
                }
                await System.IO.File.WriteAllTextAsync(_options.PersistenceFilePath, json);
            }
            catch
            {
                // 忽略持久化错误
            }
        }

        /// <summary>
        /// 从持久化文件加载消息
        /// </summary>
        public async Task LoadFromPersistenceAsync()
        {
            if (!_options.EnablePersistence || string.IsNullOrEmpty(_options.PersistenceFilePath))
                return;

            if (!System.IO.File.Exists(_options.PersistenceFilePath))
                return;

            try
            {
                var json = await System.IO.File.ReadAllTextAsync(_options.PersistenceFilePath);
                var messages = System.Text.Json.JsonSerializer.Deserialize<List<Message<T>>>(json);

                if (messages != null)
                {
                    foreach (var message in messages)
                    {
                        if (message.IsExpired) continue;

                        if (message.DelayTo != null && message.DelayTo > DateTime.UtcNow)
                        {
                            _delayedQueue.Enqueue(message);
                        }
                        else if (message.Priority > 0)
                        {
                            _priorityQueue.Enqueue(message, -message.Priority);
                        }
                        else
                        {
                            _normalQueue.Enqueue(message);
                        }
                        _signal.Release();
                    }
                }
            }
            catch
            {
                // 忽略加载错误
            }
        }

        #endregion
    }

    /// <summary>
    /// 处理结果
    /// </summary>
    public class ProcessResult
    {
        /// <summary>
        /// 处理动作
        /// </summary>
        public ProcessAction Action { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 创建完成结果
        /// </summary>
        public static ProcessResult Complete => new() { Action = ProcessAction.Complete };

        /// <summary>
        /// 创建重试结果
        /// </summary>
        public static ProcessResult Retry => new() { Action = ProcessAction.Retry };

        /// <summary>
        /// 创建死信结果
        /// </summary>
        public static ProcessResult DeadLetter => new() { Action = ProcessAction.DeadLetter };

        /// <summary>
        /// 创建错误结果
        /// </summary>
        public static ProcessResult Error(string message) => new() { Action = ProcessAction.Retry, ErrorMessage = message };
    }

    /// <summary>
    /// 处理动作
    /// </summary>
    public enum ProcessAction
    {
        /// <summary>
        /// 完成
        /// </summary>
        Complete,

        /// <summary>
        /// 重试
        /// </summary>
        Retry,

        /// <summary>
        /// 死信
        /// </summary>
        DeadLetter
    }

    /// <summary>
    /// 消息队列工具类
    /// </summary>
    public static class MessageQueueUtil
    {
        private static readonly ConcurrentDictionary<string, object> _queues = new();

        /// <summary>
        /// 创建或获取消息队列
        /// </summary>
        /// <typeparam name="T">消息体类型</typeparam>
        /// <param name="name">队列名称</param>
        /// <param name="handler">消息处理器</param>
        /// <param name="options">队列选项</param>
        /// <returns>消息队列</returns>
        public static MessageQueue<T> GetOrCreate<T>(
            string name,
            Func<Message<T>, Task<ProcessResult>> handler,
            MessageQueueOptions? options = null)
        {
            options ??= new MessageQueueOptions { Name = name };

            return (MessageQueue<T>)_queues.GetOrAdd(name, _ => new MessageQueue<T>(handler, options));
        }

        /// <summary>
        /// 移除消息队列
        /// </summary>
        /// <param name="name">队列名称</param>
        public static void Remove(string name)
        {
            if (_queues.TryRemove(name, out var queue))
            {
                (queue as IDisposable)?.Dispose();
            }
        }

        /// <summary>
        /// 清空所有队列
        /// </summary>
        public static void ClearAll()
        {
            foreach (var queue in _queues.Values)
            {
                (queue as IDisposable)?.Dispose();
            }

            _queues.Clear();
        }
    }
}
