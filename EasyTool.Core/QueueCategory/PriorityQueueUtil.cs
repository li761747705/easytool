using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EasyTool.QueueCategory
{
    /// <summary>
    /// 优先级队列工具类
    /// </summary>
    public static class PriorityQueueUtil
    {
        /// <summary>
        /// 创建最小堆优先队列
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="comparer">比较器</param>
        /// <returns>优先队列</returns>
        public static PriorityQueue<T, T> CreateMin<T>(IComparer<T>? comparer = null)
        {
            return new PriorityQueue<T, T>(comparer ?? Comparer<T>.Default);
        }

        /// <summary>
        /// 创建最大堆优先队列
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="comparer">比较器</param>
        /// <returns>优先队列</returns>
        public static PriorityQueue<T, T> CreateMax<T>(IComparer<T>? comparer = null)
        {
            var reverseComparer = Comparer<T>.Create((a, b) =>
                (comparer ?? Comparer<T>.Default).Compare(b, a));
            return new PriorityQueue<T, T>(reverseComparer);
        }

        /// <summary>
        /// 从集合创建优先队列
        /// </summary>
        /// <typeparam name="TElement">元素类型</typeparam>
        /// <typeparam name="TPriority">优先级类型</typeparam>
        /// <param name="items">元素集合</param>
        /// <param name="prioritySelector">优先级选择器</param>
        /// <returns>优先队列</returns>
        public static PriorityQueue<TElement, TPriority> FromCollection<TElement, TPriority>(
            IEnumerable<TElement> items,
            Func<TElement, TPriority> prioritySelector)
        {
            var queue = new PriorityQueue<TElement, TPriority>();
            foreach (var item in items)
            {
                queue.Enqueue(item, prioritySelector(item));
            }
            return queue;
        }

        /// <summary>
        /// 批量入队
        /// </summary>
        /// <typeparam name="TElement">元素类型</typeparam>
        /// <typeparam name="TPriority">优先级类型</typeparam>
        /// <param name="queue">优先队列</param>
        /// <param name="items">元素集合</param>
        /// <param name="prioritySelector">优先级选择器</param>
        public static void EnqueueRange<TElement, TPriority>(
            this PriorityQueue<TElement, TPriority> queue,
            IEnumerable<TElement> items,
            Func<TElement, TPriority> prioritySelector)
        {
            foreach (var item in items)
            {
                queue.Enqueue(item, prioritySelector(item));
            }
        }

        /// <summary>
        /// 批量出队
        /// </summary>
        /// <typeparam name="TElement">元素类型</typeparam>
        /// <typeparam name="TPriority">优先级类型</typeparam>
        /// <param name="queue">优先队列</param>
        /// <param name="count">数量</param>
        /// <returns>元素列表</returns>
        public static List<TElement> DequeueRange<TElement, TPriority>(
            this PriorityQueue<TElement, TPriority> queue,
            int count)
        {
            var result = new List<TElement>();

            for (int i = 0; i < count && queue.Count > 0; i++)
            {
                if (queue.TryDequeue(out var element, out _))
                {
                    result.Add(element);
                }
            }

            return result;
        }

        /// <summary>
        /// 查看队首元素但不移除
        /// </summary>
        /// <typeparam name="TElement">元素类型</typeparam>
        /// <typeparam name="TPriority">优先级类型</typeparam>
        /// <param name="queue">优先队列</param>
        /// <param name="element">元素</param>
        /// <param name="priority">优先级</param>
        /// <returns>是否成功</returns>
        public static bool TryPeek<TElement, TPriority>(
            this PriorityQueue<TElement, TPriority> queue,
            out TElement? element,
            out TPriority? priority)
        {
            element = default;
            priority = default;

            if (queue.Count == 0)
                return false;

            // 通过出队再入队的方式实现 Peek
            if (queue.TryDequeue(out element, out priority))
            {
                queue.Enqueue(element!, priority!);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 获取所有元素（按优先级排序，不移除）
        /// </summary>
        /// <typeparam name="TElement">元素类型</typeparam>
        /// <typeparam name="TPriority">优先级类型</typeparam>
        /// <param name="queue">优先队列</param>
        /// <returns>元素列表</returns>
        public static List<(TElement Element, TPriority Priority)> ToSortedList<TElement, TPriority>(
            this PriorityQueue<TElement, TPriority> queue)
        {
            var tempQueue = new PriorityQueue<TElement, TPriority>();
            var result = new List<(TElement, TPriority)>();

            while (queue.TryDequeue(out var element, out var priority))
            {
                result.Add((element!, priority!));
                tempQueue.Enqueue(element!, priority!);
            }

            // 恢复队列
            foreach (var (element, priority) in result)
            {
                queue.Enqueue(element, priority);
            }

            return result;
        }
    }

    /// <summary>
    /// 线程安全的优先队列
    /// </summary>
    /// <typeparam name="TElement">元素类型</typeparam>
    /// <typeparam name="TPriority">优先级类型</typeparam>
    public class ConcurrentPriorityQueue<TElement, TPriority> where TPriority : IComparable<TPriority>
    {
        private readonly PriorityQueue<TElement, TPriority> _queue;
        private readonly object _lock = new();

        /// <summary>
        /// 获取队列长度
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
        /// 是否为空
        /// </summary>
        public bool IsEmpty => Count == 0;

        /// <summary>
        /// 创建线程安全的优先队列
        /// </summary>
        /// <param name="comparer">优先级比较器</param>
        public ConcurrentPriorityQueue(IComparer<TPriority>? comparer = null)
        {
            _queue = new PriorityQueue<TElement, TPriority>(comparer ?? Comparer<TPriority>.Default);
        }

        /// <summary>
        /// 入队
        /// </summary>
        /// <param name="element">元素</param>
        /// <param name="priority">优先级</param>
        public void Enqueue(TElement element, TPriority priority)
        {
            lock (_lock)
            {
                _queue.Enqueue(element, priority);
            }
        }

        /// <summary>
        /// 批量入队
        /// </summary>
        /// <param name="items">元素集合</param>
        public void EnqueueRange(IEnumerable<(TElement Element, TPriority Priority)> items)
        {
            lock (_lock)
            {
                foreach (var (element, priority) in items)
                {
                    _queue.Enqueue(element, priority);
                }
            }
        }

        /// <summary>
        /// 出队
        /// </summary>
        /// <param name="element">元素</param>
        /// <param name="priority">优先级</param>
        /// <returns>是否成功</returns>
        public bool TryDequeue(out TElement? element, out TPriority? priority)
        {
            lock (_lock)
            {
                return _queue.TryDequeue(out element, out priority);
            }
        }

        /// <summary>
        /// 批量出队
        /// </summary>
        /// <param name="count">数量</param>
        /// <returns>元素列表</returns>
        public List<(TElement Element, TPriority Priority)> DequeueRange(int count)
        {
            var result = new List<(TElement, TPriority)>();

            lock (_lock)
            {
                for (int i = 0; i < count && _queue.Count > 0; i++)
                {
                    if (_queue.TryDequeue(out var element, out var priority))
                    {
                        result.Add((element!, priority!));
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 查看队首元素
        /// </summary>
        /// <param name="element">元素</param>
        /// <param name="priority">优先级</param>
        /// <returns>是否成功</returns>
        public bool TryPeek(out TElement? element, out TPriority? priority)
        {
            lock (_lock)
            {
                if (_queue.Count == 0)
                {
                    element = default;
                    priority = default;
                    return false;
                }

                if (_queue.TryDequeue(out element, out priority))
                {
                    _queue.Enqueue(element!, priority!);
                    return true;
                }

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
                while (_queue.TryDequeue(out _, out _)) { }
            }
        }

        /// <summary>
        /// 转换为数组
        /// </summary>
        /// <returns>数组</returns>
        public (TElement Element, TPriority Priority)[] ToArray()
        {
            lock (_lock)
            {
                var tempQueue = new PriorityQueue<TElement, TPriority>();
                var result = new List<(TElement, TPriority)>();

                while (_queue.TryDequeue(out var element, out var priority))
                {
                    result.Add((element!, priority!));
                    tempQueue.Enqueue(element!, priority!);
                }

                // 恢复队列
                foreach (var (element, priority) in result)
                {
                    _queue.Enqueue(element, priority);
                }

                return result.ToArray();
            }
        }
    }
}
