using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace EasyTool.QueueCategory
{
    /// <summary>
    /// Channel 工具类
    /// 提供线程安全的生产者-消费者队列实现
    /// </summary>
    public static class ChannelUtil
    {
        /// <summary>
        /// 创建无界 Channel
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <returns>Channel 实例</returns>
        public static Channel<T> CreateUnbounded<T>()
        {
            return Channel.CreateUnbounded<T>();
        }

        /// <summary>
        /// 创建无界 Channel（带选项）
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="options">Channel 选项</param>
        /// <returns>Channel 实例</returns>
        public static Channel<T> CreateUnbounded<T>(UnboundedChannelOptions options)
        {
            return Channel.CreateUnbounded<T>(options);
        }

        /// <summary>
        /// 创建有界 Channel
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="capacity">容量</param>
        /// <returns>Channel 实例</returns>
        public static Channel<T> CreateBounded<T>(int capacity)
        {
            return Channel.CreateBounded<T>(capacity);
        }

        /// <summary>
        /// 创建有界 Channel（带选项）
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="options">Channel 选项</param>
        /// <returns>Channel 实例</returns>
        public static Channel<T> CreateBounded<T>(BoundedChannelOptions options)
        {
            return Channel.CreateBounded<T>(options);
        }

        /// <summary>
        /// 批量写入数据
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="channel">Channel 实例</param>
        /// <param name="items">数据集合</param>
        /// <param name="cancellationToken">取消令牌</param>
        public static async Task WriteManyAsync<T>(Channel<T> channel, IEnumerable<T> items, CancellationToken cancellationToken = default)
        {
            foreach (var item in items)
            {
                await channel.Writer.WriteAsync(item, cancellationToken);
            }
        }

        /// <summary>
        /// 批量读取数据
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="channel">Channel 实例</param>
        /// <param name="count">读取数量</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>数据列表</returns>
        public static async Task<List<T>> ReadManyAsync<T>(Channel<T> channel, int count, CancellationToken cancellationToken = default)
        {
            var result = new List<T>();

            for (int i = 0; i < count; i++)
            {
                if (await channel.Reader.WaitToReadAsync(cancellationToken))
                {
                    if (channel.Reader.TryRead(out var item))
                    {
                        result.Add(item);
                    }
                }
                else
                {
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// 读取所有数据直到完成
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="channel">Channel 实例</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>数据列表</returns>
        public static async Task<List<T>> ReadAllAsync<T>(Channel<T> channel, CancellationToken cancellationToken = default)
        {
            var result = new List<T>();

            await foreach (var item in channel.Reader.ReadAllAsync(cancellationToken))
            {
                result.Add(item);
            }

            return result;
        }

        /// <summary>
        /// 创建异步生产者-消费者处理器
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="capacity">容量（null 表示无界）</param>
        /// <param name="processAction">处理函数</param>
        /// <param name="consumerCount">消费者数量</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>生产者写入器和完成任务</returns>
        public static (ChannelWriter<T> Writer, Task Completion) CreateProcessor<T>(
            int? capacity,
            Func<T, Task> processAction,
            int consumerCount = 1,
            CancellationToken cancellationToken = default)
        {
            var channel = capacity.HasValue
                ? Channel.CreateBounded<T>(capacity.Value)
                : Channel.CreateUnbounded<T>();

            var consumers = new Task[consumerCount];

            for (int i = 0; i < consumerCount; i++)
            {
                consumers[i] = ConsumeAsync(channel.Reader, processAction, cancellationToken);
            }

            var completion = Task.WhenAll(consumers);

            return (channel.Writer, completion);
        }

        /// <summary>
        /// 创建带批处理的消费者
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="capacity">容量</param>
        /// <param name="batchSize">批处理大小</param>
        /// <param name="batchTimeout">批处理超时</param>
        /// <param name="processAction">处理函数</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>生产者写入器和完成任务</returns>
        public static (ChannelWriter<T> Writer, Task Completion) CreateBatchProcessor<T>(
            int capacity,
            int batchSize,
            TimeSpan batchTimeout,
            Func<IReadOnlyList<T>, Task> processAction,
            CancellationToken cancellationToken = default)
        {
            var channel = Channel.CreateBounded<T>(new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.Wait
            });

            var completion = ProcessBatchAsync(channel.Reader, batchSize, batchTimeout, processAction, cancellationToken);

            return (channel.Writer, completion);
        }

        private static async Task ConsumeAsync<T>(ChannelReader<T> reader, Func<T, Task> processAction, CancellationToken cancellationToken)
        {
            await foreach (var item in reader.ReadAllAsync(cancellationToken))
            {
                await processAction(item);
            }
        }

        private static async Task ProcessBatchAsync<T>(
            ChannelReader<T> reader,
            int batchSize,
            TimeSpan batchTimeout,
            Func<IReadOnlyList<T>, Task> processAction,
            CancellationToken cancellationToken)
        {
            var batch = new List<T>(batchSize);

            while (await reader.WaitToReadAsync(cancellationToken))
            {
                batch.Clear();

                // 尝试收集一批数据
                while (batch.Count < batchSize && reader.TryRead(out var item))
                {
                    batch.Add(item);
                }

                if (batch.Count > 0)
                {
                    await processAction(batch);
                }
            }

            // 处理剩余数据
            if (batch.Count > 0)
            {
                await processAction(batch);
            }
        }
    }

    /// <summary>
    /// 异步队列
    /// 提供简单的异步队列操作封装
    /// </summary>
    /// <typeparam name="T">元素类型</typeparam>
    public class AsyncQueue<T> : IDisposable
    {
        private readonly Channel<T> _channel;
        private bool _disposed;

        /// <summary>
        /// 获取当前队列长度
        /// </summary>
        public int Count => _channel.Reader.Count;

        /// <summary>
        /// 是否完成写入
        /// </summary>
        public bool IsCompleted => _channel.Reader.Completion.IsCompleted;

        /// <summary>
        /// 创建异步队列（无界）
        /// </summary>
        public AsyncQueue()
        {
            _channel = Channel.CreateUnbounded<T>();
        }

        /// <summary>
        /// 创建异步队列（有界）
        /// </summary>
        /// <param name="capacity">容量</param>
        /// <param name="fullMode">满时策略</param>
        public AsyncQueue(int capacity, BoundedChannelFullMode fullMode = BoundedChannelFullMode.Wait)
        {
            _channel = Channel.CreateBounded<T>(new BoundedChannelOptions(capacity)
            {
                FullMode = fullMode
            });
        }

        /// <summary>
        /// 入队
        /// </summary>
        /// <param name="item">元素</param>
        /// <param name="cancellationToken">取消令牌</param>
        public async ValueTask EnqueueAsync(T item, CancellationToken cancellationToken = default)
        {
            await _channel.Writer.WriteAsync(item, cancellationToken);
        }

        /// <summary>
        /// 入队（同步）
        /// </summary>
        /// <param name="item">元素</param>
        /// <returns>是否成功</returns>
        public bool Enqueue(T item)
        {
            return _channel.Writer.TryWrite(item);
        }

        /// <summary>
        /// 出队
        /// </summary>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>元素</returns>
        public async ValueTask<T> DequeueAsync(CancellationToken cancellationToken = default)
        {
            return await _channel.Reader.ReadAsync(cancellationToken);
        }

        /// <summary>
        /// 尝试出队
        /// </summary>
        /// <param name="item">元素</param>
        /// <returns>是否成功</returns>
        public bool TryDequeue(out T? item)
        {
            return _channel.Reader.TryRead(out item);
        }

        /// <summary>
        /// 尝试查看队首元素
        /// </summary>
        /// <param name="item">元素</param>
        /// <returns>是否成功</returns>
        public bool TryPeek(out T? item)
        {
            return _channel.Reader.TryPeek(out item);
        }

        /// <summary>
        /// 等待有数据可读
        /// </summary>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>是否有数据</returns>
        public ValueTask<bool> WaitToReadAsync(CancellationToken cancellationToken = default)
        {
            return _channel.Reader.WaitToReadAsync(cancellationToken);
        }

        /// <summary>
        /// 获取所有数据（异步迭代）
        /// </summary>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>异步迭代器</returns>
        public IAsyncEnumerable<T> ReadAllAsync(CancellationToken cancellationToken = default)
        {
            return _channel.Reader.ReadAllAsync(cancellationToken);
        }

        /// <summary>
        /// 完成写入
        /// </summary>
        public void Complete()
        {
            _channel.Writer.Complete();
        }

        /// <summary>
        /// 等待完成
        /// </summary>
        /// <returns>完成任务</returns>
        public Task Completion => _channel.Reader.Completion;

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                _channel.Writer.TryComplete();
                _disposed = true;
            }
        }
    }
}
