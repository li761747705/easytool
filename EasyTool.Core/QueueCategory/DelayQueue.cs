using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EasyTool.QueueCategory
{
    /// <summary>
    /// 延迟队列项
    /// </summary>
    /// <typeparam name="T">元素类型</typeparam>
    internal class DelayQueueItem<T>
    {
        public T Value { get; set; } = default!;
        public DateTime ExecuteTime { get; set; }
        public Guid Id { get; set; } = Guid.NewGuid();
    }

    /// <summary>
    /// 延迟队列
    /// 支持按时间延迟执行任务
    /// </summary>
    /// <typeparam name="T">元素类型</typeparam>
    public class DelayQueue<T> : IDisposable
    {
        private readonly ConcurrentDictionary<Guid, DelayQueueItem<T>> _items;
        private readonly List<DelayQueueItem<T>> _sortedItems;
        private readonly SemaphoreSlim _signal;
        private readonly CancellationTokenSource _cts;
        private readonly Task _processTask;
        private readonly object _lock = new();
        private bool _disposed;

        /// <summary>
        /// 获取队列长度
        /// </summary>
        public int Count => _items.Count;

        /// <summary>
        /// 是否为空
        /// </summary>
        public bool IsEmpty => _items.IsEmpty;

        /// <summary>
        /// 创建延迟队列
        /// </summary>
        public DelayQueue()
        {
            _items = new ConcurrentDictionary<Guid, DelayQueueItem<T>>();
            _sortedItems = new List<DelayQueueItem<T>>();
            _signal = new SemaphoreSlim(0);
            _cts = new CancellationTokenSource();
            _processTask = ProcessAsync(_cts.Token);
        }

        /// <summary>
        /// 添加延迟元素
        /// </summary>
        /// <param name="value">元素值</param>
        /// <param name="delay">延迟时间</param>
        /// <returns>元素ID，可用于取消</returns>
        public Guid Add(T value, TimeSpan delay)
        {
            return Add(value, DateTime.UtcNow.Add(delay));
        }

        /// <summary>
        /// 添加延迟元素
        /// </summary>
        /// <param name="value">元素值</param>
        /// <param name="executeTime">执行时间</param>
        /// <returns>元素ID，可用于取消</returns>
        public Guid Add(T value, DateTime executeTime)
        {
            var item = new DelayQueueItem<T>
            {
                Value = value,
                ExecuteTime = executeTime
            };

            lock (_lock)
            {
                _items[item.Id] = item;
                InsertSorted(_sortedItems, item);
            }

            _signal.Release();
            return item.Id;
        }

        /// <summary>
        /// 尝试取消元素
        /// </summary>
        /// <param name="id">元素ID</param>
        /// <returns>是否取消成功</returns>
        public bool TryCancel(Guid id)
        {
            lock (_lock)
            {
                if (_items.TryRemove(id, out var item))
                {
                    _sortedItems.Remove(item);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 异步等待并获取到期元素
        /// </summary>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>到期元素</returns>
        public async Task<T> TakeAsync(CancellationToken cancellationToken = default)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                lock (_lock)
                {
                    if (_sortedItems.Count > 0)
                    {
                        var first = _sortedItems[0];
                        var now = DateTime.UtcNow;

                        if (now >= first.ExecuteTime)
                        {
                            _sortedItems.RemoveAt(0);
                            _items.TryRemove(first.Id, out _);
                            return first.Value;
                        }
                    }
                }

                // 计算等待时间
                var waitTime = GetWaitTime();

                if (waitTime > TimeSpan.Zero)
                {
                    await Task.Delay(waitTime, cancellationToken).ConfigureAwait(false);
                }
            }

            throw new OperationCanceledException();
        }

        /// <summary>
        /// 尝试获取到期元素（非阻塞）
        /// </summary>
        /// <param name="value">元素值</param>
        /// <returns>是否成功获取</returns>
        public bool TryTake(out T? value)
        {
            value = default;

            lock (_lock)
            {
                if (_sortedItems.Count > 0)
                {
                    var first = _sortedItems[0];

                    if (DateTime.UtcNow >= first.ExecuteTime)
                    {
                        _sortedItems.RemoveAt(0);
                        _items.TryRemove(first.Id, out _);
                        value = first.Value;
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 尝试在指定时间内获取到期元素
        /// </summary>
        /// <param name="timeout">超时时间</param>
        /// <param name="value">元素值</param>
        /// <returns>是否成功获取</returns>
        public bool TryTake(TimeSpan timeout, out T? value)
        {
            value = default;
            var endTime = DateTime.UtcNow.Add(timeout);

            while (DateTime.UtcNow < endTime)
            {
                if (TryTake(out value))
                {
                    return true;
                }

                var remaining = endTime - DateTime.UtcNow;
                if (remaining <= TimeSpan.Zero)
                    break;

                var waitTime = GetWaitTime();
                if (waitTime > TimeSpan.Zero)
                {
                    Thread.Sleep((int)Math.Min(waitTime.TotalMilliseconds, remaining.TotalMilliseconds));
                }
            }

            return false;
        }

        /// <summary>
        /// 获取所有元素（不等待到期）
        /// </summary>
        /// <returns>元素列表</returns>
        public List<(T Value, DateTime ExecuteTime)> GetAll()
        {
            lock (_lock)
            {
                return _sortedItems
                    .Select(i => (i.Value, i.ExecuteTime))
                    .ToList();
            }
        }

        /// <summary>
        /// 清空队列
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                _items.Clear();
                _sortedItems.Clear();
            }
        }

        /// <summary>
        /// 创建处理器
        /// </summary>
        /// <param name="handler">处理函数</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>处理任务</returns>
        public Task ProcessAsync(Func<T, Task> handler, CancellationToken cancellationToken = default)
        {
            return Task.Run(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var value = await TakeAsync(cancellationToken).ConfigureAwait(false);
                        await handler(value).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }
            }, cancellationToken);
        }

        private TimeSpan GetWaitTime()
        {
            lock (_lock)
            {
                if (_sortedItems.Count == 0)
                    return TimeSpan.FromSeconds(1);

                var first = _sortedItems[0];
                var waitTime = first.ExecuteTime - DateTime.UtcNow;
                return waitTime > TimeSpan.Zero ? waitTime : TimeSpan.Zero;
            }
        }

        private void InsertSorted(List<DelayQueueItem<T>> list, DelayQueueItem<T> item)
        {
            var index = list.BinarySearch(item, Comparer<DelayQueueItem<T>>.Create((a, b) =>
                a.ExecuteTime.CompareTo(b.ExecuteTime)));

            if (index < 0)
                index = ~index;

            list.Insert(index, item);
        }

        private async Task ProcessAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await _signal.WaitAsync(cancellationToken).ConfigureAwait(false);
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
                _cts.Cancel();
                _signal.Dispose();
                _cts.Dispose();
                _disposed = true;
            }
        }
    }
}
