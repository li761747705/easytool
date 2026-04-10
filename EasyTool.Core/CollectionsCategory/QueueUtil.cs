using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EasyTool.CollectionsCategory
{
    /// <summary>
    /// 线程安全队列工具类
    /// 提供生产者-消费者模式的队列操作
    /// </summary>
    /// <typeparam name="T">元素类型</typeparam>
    public class QueueUtil<T>
    {
        private readonly Queue<T> _queue = new();
        private readonly object _lock = new();
        private readonly SemaphoreSlim _signal = new(0);

        /// <summary>
        /// 获取队列元素数量
        /// </summary>
        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return _queue.Count;
                }
            }
        }

        /// <summary>
        /// 检查队列是否为空
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                lock (_lock)
                {
                    return _queue.Count == 0;
                }
            }
        }

        /// <summary>
        /// 入队
        /// </summary>
        /// <param name="item">元素</param>
        public void Enqueue(T item)
        {
            lock (_lock)
            {
                _queue.Enqueue(item);
                _signal.Release();
            }
        }

        /// <summary>
        /// 批量入队
        /// </summary>
        /// <param name="items">元素集合</param>
        public void EnqueueRange(IEnumerable<T> items)
        {
            lock (_lock)
            {
                foreach (var item in items)
                {
                    _queue.Enqueue(item);
                    _signal.Release();
                }
            }
        }

        /// <summary>
        /// 出队
        /// </summary>
        /// <returns>元素</returns>
        public T? Dequeue()
        {
            _signal.Wait();

            lock (_lock)
            {
                return _queue.Count > 0 ? _queue.Dequeue() : default;
            }
        }

        /// <summary>
        /// 尝试出队
        /// </summary>
        /// <param name="item">元素</param>
        /// <returns>是否成功</returns>
        public bool TryDequeue(out T? item)
        {
            lock (_lock)
            {
                if (_queue.Count > 0)
                {
                    item = _queue.Dequeue();
                    _signal.Wait(0);
                    return true;
                }

                item = default;
                return false;
            }
        }

        /// <summary>
        /// 尝试出队（带超时）
        /// </summary>
        /// <param name="timeout">超时时间</param>
        /// <param name="item">元素</param>
        /// <returns>是否成功</returns>
        public bool TryDequeue(TimeSpan timeout, out T? item)
        {
            if (_signal.Wait(timeout))
            {
                lock (_lock)
                {
                    if (_queue.Count > 0)
                    {
                        item = _queue.Dequeue();
                        return true;
                    }
                }
            }

            item = default;
            return false;
        }

        /// <summary>
        /// 异步出队
        /// </summary>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>元素</returns>
        public async Task<T?> DequeueAsync(CancellationToken cancellationToken = default)
        {
            await _signal.WaitAsync(cancellationToken).ConfigureAwait(false);

            lock (_lock)
            {
                return _queue.Count > 0 ? _queue.Dequeue() : default;
            }
        }

        /// <summary>
        /// 异步尝试出队
        /// </summary>
        /// <param name="timeout">超时时间</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>元素或默认值</returns>
        public async Task<(bool Success, T? Item)> TryDequeueAsync(TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            if (await _signal.WaitAsync(timeout, cancellationToken).ConfigureAwait(false))
            {
                lock (_lock)
                {
                    if (_queue.Count > 0)
                    {
                        return (true, _queue.Dequeue());
                    }
                }
            }

            return (false, default);
        }

        /// <summary>
        /// 查看队首元素（不出队）
        /// </summary>
        /// <returns>队首元素</returns>
        public T? Peek()
        {
            lock (_lock)
            {
                return _queue.Count > 0 ? _queue.Peek() : default;
            }
        }

        /// <summary>
        /// 尝试查看队首元素
        /// </summary>
        /// <param name="item">元素</param>
        /// <returns>是否成功</returns>
        public bool TryPeek(out T? item)
        {
            lock (_lock)
            {
                if (_queue.Count > 0)
                {
                    item = _queue.Peek();
                    return true;
                }

                item = default;
                return false;
            }
        }

        /// <summary>
        /// 清空队列
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                while (_signal.CurrentCount > 0)
                {
                    _signal.Wait(0);
                }
                _queue.Clear();
            }
        }

        /// <summary>
        /// 获取所有元素（不出队）
        /// </summary>
        /// <returns>元素数组</returns>
        public T[] ToArray()
        {
            lock (_lock)
            {
                return _queue.ToArray();
            }
        }

        /// <summary>
        /// 获取所有元素并清空队列
        /// </summary>
        /// <returns>元素数组</returns>
        public T[] Drain()
        {
            lock (_lock)
            {
                var items = _queue.ToArray();
                _queue.Clear();
                while (_signal.CurrentCount > 0)
                {
                    _signal.Wait(0);
                }
                return items;
            }
        }
    }

    /// <summary>
    /// 优先级队列工具类
    /// </summary>
    /// <typeparam name="T">元素类型</typeparam>
    public class PriorityQueue<T>
    {
        private readonly SortedDictionary<int, Queue<T>> _queues = new();
        private readonly object _lock = new();

        /// <summary>
        /// 获取元素数量
        /// </summary>
        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return _queues.Sum(q => q.Value.Count);
                }
            }
        }

        /// <summary>
        /// 入队
        /// </summary>
        /// <param name="item">元素</param>
        /// <param name="priority">优先级（数字越小优先级越高）</param>
        public void Enqueue(T item, int priority = 0)
        {
            lock (_lock)
            {
                if (!_queues.TryGetValue(priority, out var queue))
                {
                    queue = new Queue<T>();
                    _queues[priority] = queue;
                }

                queue.Enqueue(item);
            }
        }

        /// <summary>
        /// 出队
        /// </summary>
        /// <returns>元素</returns>
        public T? Dequeue()
        {
            lock (_lock)
            {
                foreach (var kvp in _queues)
                {
                    if (kvp.Value.Count > 0)
                    {
                        return kvp.Value.Dequeue();
                    }
                }

                return default;
            }
        }

        /// <summary>
        /// 尝试出队
        /// </summary>
        /// <param name="item">元素</param>
        /// <returns>是否成功</returns>
        public bool TryDequeue(out T? item)
        {
            lock (_lock)
            {
                foreach (var kvp in _queues)
                {
                    if (kvp.Value.Count > 0)
                    {
                        item = kvp.Value.Dequeue();
                        return true;
                    }
                }

                item = default;
                return false;
            }
        }

        /// <summary>
        /// 查看队首元素
        /// </summary>
        /// <returns>元素</returns>
        public T? Peek()
        {
            lock (_lock)
            {
                foreach (var kvp in _queues)
                {
                    if (kvp.Value.Count > 0)
                    {
                        return kvp.Value.Peek();
                    }
                }

                return default;
            }
        }

        /// <summary>
        /// 清空队列
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                _queues.Clear();
            }
        }
    }

}