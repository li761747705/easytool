using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyTool.ToolCategory
{
    /// <summary>
    /// 生产者消费者模式工具类
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    public class ProducerConsumer<T>
    {
        private readonly System.Collections.Concurrent.BlockingCollection<T> _collection;
        private readonly Action<T> _consumer;
        private readonly int _maxConsumers;
        private readonly List<System.Threading.Tasks.Task> _consumerTasks;
        private bool _isRunning;
        private readonly object _lock = new();

        /// <summary>
        /// 队列中元素数量
        /// </summary>
        public int Count => _collection.Count;

        /// <summary>
        /// 是否正在运行
        /// </summary>
        public bool IsRunning => _isRunning;

        /// <summary>
        /// 是否已完成添加
        /// </summary>
        public bool IsAddingCompleted => _collection.IsAddingCompleted;

        /// <summary>
        /// 创建生产者消费者
        /// </summary>
        /// <param name="consumer">消费者处理函数</param>
        /// <param name="boundedCapacity">队列容量（0表示无限制）</param>
        /// <param name="maxConsumers">最大消费者数量</param>
        public ProducerConsumer(Action<T> consumer, int boundedCapacity = 0, int maxConsumers = 1)
        {
            _consumer = consumer ?? throw new ArgumentNullException(nameof(consumer));
            _maxConsumers = maxConsumers;
            _collection = boundedCapacity > 0
                ? new System.Collections.Concurrent.BlockingCollection<T>(boundedCapacity)
                : new System.Collections.Concurrent.BlockingCollection<T>();
            _consumerTasks = new List<System.Threading.Tasks.Task>();
        }

        /// <summary>
        /// 启动消费者
        /// </summary>
        public void Start()
        {
            lock (_lock)
            {
                if (_isRunning) return;
                _isRunning = true;

                for (int i = 0; i < _maxConsumers; i++)
                {
                    _consumerTasks.Add(System.Threading.Tasks.Task.Run(ConsumeLoop));
                }
            }
        }

        /// <summary>
        /// 生产数据
        /// </summary>
        public void Produce(T item)
        {
            _collection.Add(item);
        }

        /// <summary>
        /// 批量生产数据
        /// </summary>
        public void ProduceRange(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                _collection.Add(item);
            }
        }

        /// <summary>
        /// 尝试生产数据（不阻塞）
        /// </summary>
        public bool TryProduce(T item, TimeSpan timeout)
        {
            return _collection.TryAdd(item, timeout);
        }

        /// <summary>
        /// 标记添加完成
        /// </summary>
        public void CompleteAdding()
        {
            _collection.CompleteAdding();
        }

        /// <summary>
        /// 停止并等待完成
        /// </summary>
        public void Stop()
        {
            lock (_lock)
            {
                if (!_isRunning) return;
                _collection.CompleteAdding();
                System.Threading.Tasks.Task.WaitAll(_consumerTasks.ToArray());
                _isRunning = false;
            }
        }

        /// <summary>
        /// 异步停止
        /// </summary>
        public async System.Threading.Tasks.Task StopAsync()
        {
            lock (_lock)
            {
                if (!_isRunning) return;
                _collection.CompleteAdding();
            }

            await System.Threading.Tasks.Task.WhenAll(_consumerTasks).ConfigureAwait(false);

            lock (_lock)
            {
                _isRunning = false;
            }
        }

        private void ConsumeLoop()
        {
            foreach (var item in _collection.GetConsumingEnumerable())
            {
                try
                {
                    _consumer(item);
                }
                catch
                {
                    // 忽略消费者异常，继续处理下一个
                }
            }
        }
    }

    /// <summary>
    /// 异步通道（类似Go的channel）
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    public class Channel<T>
    {
        private readonly System.Collections.Concurrent.ConcurrentQueue<T> _queue = new();
        private readonly System.Threading.SemaphoreSlim _signal = new(0);
        private readonly int? _capacity;
        private readonly System.Threading.SemaphoreSlim? _capacitySemaphore;
        private bool _closed;

        /// <summary>
        /// 队列中元素数量
        /// </summary>
        public int Count => _queue.Count;

        /// <summary>
        /// 是否已关闭
        /// </summary>
        public bool IsClosed => _closed;

        /// <summary>
        /// 创建通道
        /// </summary>
        /// <param name="capacity">容量（0或null表示无限制）</param>
        public Channel(int capacity = 0)
        {
            _capacity = capacity > 0 ? capacity : null;
            if (_capacity.HasValue)
            {
                _capacitySemaphore = new System.Threading.SemaphoreSlim(_capacity.Value, _capacity.Value);
            }
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        public async System.Threading.Tasks.Task SendAsync(T item)
        {
            if (_closed)
                throw new InvalidOperationException("通道已关闭");

            if (_capacitySemaphore != null)
            {
                await _capacitySemaphore.WaitAsync().ConfigureAwait(false);
            }

            _queue.Enqueue(item);
            _signal.Release();
        }

        /// <summary>
        /// 尝试发送数据
        /// </summary>
        public async System.Threading.Tasks.Task<bool> TrySendAsync(T item, TimeSpan timeout)
        {
            if (_closed)
                return false;

            if (_capacitySemaphore != null)
            {
                if (!await _capacitySemaphore.WaitAsync(timeout).ConfigureAwait(false))
                    return false;
            }

            _queue.Enqueue(item);
            _signal.Release();
            return true;
        }

        /// <summary>
        /// 接收数据
        /// </summary>
        public async System.Threading.Tasks.Task<T> ReceiveAsync()
        {
            await _signal.WaitAsync().ConfigureAwait(false);

            if (_queue.TryDequeue(out var item))
            {
                _capacitySemaphore?.Release();
                return item;
            }

            throw new InvalidOperationException("通道状态异常");
        }

        /// <summary>
        /// 尝试接收数据
        /// </summary>
        public async System.Threading.Tasks.Task<(bool Success, T? Item)> TryReceiveAsync(TimeSpan timeout)
        {
            if (!await _signal.WaitAsync(timeout).ConfigureAwait(false))
                return (false, default);

            if (_queue.TryDequeue(out var item))
            {
                _capacitySemaphore?.Release();
                return (true, item);
            }

            return (false, default);
        }

        /// <summary>
        /// 关闭通道
        /// </summary>
        public void Close()
        {
            _closed = true;
        }

        /// <summary>
        /// 获取所有剩余数据
        /// </summary>
        public IEnumerable<T> GetAllRemaining()
        {
            while (_queue.TryDequeue(out var item))
            {
                yield return item;
            }
        }
    }

    /// <summary>
    /// 并行执行工具
    /// </summary>
    public static class ParallelUtil
    {
        /// <summary>
        /// 并行执行任务并收集结果
        /// </summary>
        public static async System.Threading.Tasks.Task<List<TResult>> WhenAllAsync<TSource, TResult>(
            IEnumerable<TSource> sources,
            Func<TSource, System.Threading.Tasks.Task<TResult>> selector,
            int maxDegreeOfParallelism)
        {
            var semaphore = new System.Threading.SemaphoreSlim(maxDegreeOfParallelism);
            var tasks = sources.Select(async source =>
            {
                await semaphore.WaitAsync().ConfigureAwait(false);
                try
                {
                    return await selector(source).ConfigureAwait(false);
                }
                finally
                {
                    semaphore.Release();
                }
            });

            var results = await System.Threading.Tasks.Task.WhenAll(tasks).ConfigureAwait(false);
            return results.ToList();
        }

        /// <summary>
        /// 并行执行任务
        /// </summary>
        public static async System.Threading.Tasks.Task WhenAllAsync<TSource>(
            IEnumerable<TSource> sources,
            Func<TSource, System.Threading.Tasks.Task> action,
            int maxDegreeOfParallelism)
        {
            var semaphore = new System.Threading.SemaphoreSlim(maxDegreeOfParallelism);
            var tasks = sources.Select(async source =>
            {
                await semaphore.WaitAsync().ConfigureAwait(false);
                try
                {
                    await action(source).ConfigureAwait(false);
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await System.Threading.Tasks.Task.WhenAll(tasks).ConfigureAwait(false);
        }

        /// <summary>
        /// 分批并行执行
        /// </summary>
        public static async System.Threading.Tasks.Task WhenAllBatchedAsync<TSource>(
            IEnumerable<TSource> sources,
            Func<IEnumerable<TSource>, System.Threading.Tasks.Task> batchAction,
            int batchSize)
        {
            var batch = new List<TSource>(batchSize);

            foreach (var source in sources)
            {
                batch.Add(source);
                if (batch.Count >= batchSize)
                {
                    await batchAction(batch).ConfigureAwait(false);
                    batch = new List<TSource>(batchSize);
                }
            }

            if (batch.Count > 0)
            {
                await batchAction(batch).ConfigureAwait(false);
            }
        }
    }
}
