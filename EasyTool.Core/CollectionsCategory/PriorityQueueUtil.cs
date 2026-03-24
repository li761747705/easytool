using System;
using System.Collections.Generic;

namespace EasyTool.CollectionsCategory
{
    /// <summary>
    /// 优先队列工具类
    /// 基于 binary heap 实现的优先队列，支持自定义优先级比较器
    /// 兼容 netstandard2.1（.NET 6 才内置 PriorityQueue）
    /// </summary>
    public static class PriorityQueueUtil
    {
#if NETSTANDARD2_1
        /// <summary>
        /// 创建最小堆优先队列（最小值优先出队）
        /// </summary>
        public static PriorityQueue<TElement, TPriority> CreateMin<TElement, TPriority>()
            where TPriority : IComparable<TPriority>
        {
            return new PriorityQueue<TElement, TPriority>(Comparer<TPriority>.Default);
        }

        /// <summary>
        /// 创建最大堆优先队列（最大值优先出队）
        /// </summary>
        public static PriorityQueue<TElement, TPriority> CreateMax<TElement, TPriority>()
            where TPriority : IComparable<TPriority>
        {
            return new PriorityQueue<TElement, TPriority>(Comparer<TPriority>.Default, true);
        }

        /// <summary>
        /// 创建自定义比较器的优先队列
        /// </summary>
        public static PriorityQueue<TElement, TPriority> Create<TElement, TPriority>(
            IComparer<TPriority> comparer, bool maxHeap = false)
        {
            return new PriorityQueue<TElement, TPriority>(comparer, maxHeap);
        }
#else
        /// <summary>
        /// 创建最小堆优先队列（最小值优先出队）
        /// </summary>
        public static System.Collections.Generic.PriorityQueue<TElement, TPriority> CreateMin<TElement, TPriority>()
            where TPriority : IComparable<TPriority>
        {
            return new System.Collections.Generic.PriorityQueue<TElement, TPriority>();
        }

        /// <summary>
        /// 创建最大堆优先队列（最大值优先出队）
        /// </summary>
        public static System.Collections.Generic.PriorityQueue<TElement, TPriority> CreateMax<TElement, TPriority>()
            where TPriority : IComparable<TPriority>
        {
            // 使用反向比较器实现最大堆
            var comparer = Comparer<TPriority>.Default;
            var reverseComparer = Comparer<TPriority>.Create((x, y) => comparer.Compare(y, x));
            return new System.Collections.Generic.PriorityQueue<TElement, TPriority>(reverseComparer);
        }

        /// <summary>
        /// 创建自定义比较器的优先队列
        /// </summary>
        public static System.Collections.Generic.PriorityQueue<TElement, TPriority> Create<TElement, TPriority>(
            IComparer<TPriority> comparer, bool maxHeap = false)
        {
            if (maxHeap)
            {
                var reverseComparer = Comparer<TPriority>.Create((x, y) => comparer.Compare(y, x));
                return new System.Collections.Generic.PriorityQueue<TElement, TPriority>(reverseComparer);
            }
            return new System.Collections.Generic.PriorityQueue<TElement, TPriority>(comparer);
        }
#endif
    }

#if NETSTANDARD2_1
    /// <summary>
    /// 优先队列实现（仅用于 netstandard2.1，.NET 6+ 使用内置实现）
    /// </summary>
    /// <typeparam name="TElement">元素类型</typeparam>
    /// <typeparam name="TPriority">优先级类型</typeparam>
    public class PriorityQueue<TElement, TPriority>
    {
        private readonly List<(TElement Element, TPriority Priority)> _heap;
        private readonly IComparer<TPriority> _comparer;
        private readonly bool _isMaxHeap;

        /// <summary>
        /// 元素数量
        /// </summary>
        public int Count => _heap.Count;

        /// <summary>
        /// 是否为空
        /// </summary>
        public bool IsEmpty => _heap.Count == 0;

        /// <summary>
        /// 创建优先队列
        /// </summary>
        /// <param name="comparer">优先级比较器</param>
        /// <param name="maxHeap">是否为最大堆（默认最小堆）</param>
        public PriorityQueue(IComparer<TPriority> comparer, bool maxHeap = false)
        {
            _heap = new List<(TElement, TPriority)>();
            _comparer = comparer ?? Comparer<TPriority>.Default;
            _isMaxHeap = maxHeap;
        }

        /// <summary>
        /// 创建带初始容量的优先队列
        /// </summary>
        public PriorityQueue(int initialCapacity, IComparer<TPriority> comparer, bool maxHeap = false)
        {
            _heap = new List<(TElement, TPriority)>(initialCapacity);
            _comparer = comparer ?? Comparer<TPriority>.Default;
            _isMaxHeap = maxHeap;
        }

        /// <summary>
        /// 入队
        /// </summary>
        public void Enqueue(TElement element, TPriority priority)
        {
            _heap.Add((element, priority));
            SiftUp(_heap.Count - 1);
        }

        /// <summary>
        /// 批量入队
        /// </summary>
        public void EnqueueRange(IEnumerable<(TElement Element, TPriority Priority)> items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            foreach (var item in items)
            {
                Enqueue(item.Element, item.Priority);
            }
        }

        /// <summary>
        /// 出队（返回优先级最高/最低的元素）
        /// </summary>
        public TElement Dequeue()
        {
            if (_heap.Count == 0)
                throw new InvalidOperationException("Queue is empty");

            var result = _heap[0].Element;
            int lastIndex = _heap.Count - 1;

            _heap[0] = _heap[lastIndex];
            _heap.RemoveAt(lastIndex);

            if (_heap.Count > 0)
            {
                SiftDown(0);
            }

            return result;
        }

        /// <summary>
        /// 出队并返回元素和优先级
        /// </summary>
        public (TElement Element, TPriority Priority) DequeueWithPriority()
        {
            if (_heap.Count == 0)
                throw new InvalidOperationException("Queue is empty");

            var result = _heap[0];
            int lastIndex = _heap.Count - 1;

            _heap[0] = _heap[lastIndex];
            _heap.RemoveAt(lastIndex);

            if (_heap.Count > 0)
            {
                SiftDown(0);
            }

            return result;
        }

        /// <summary>
        /// 查看队首元素（不移除）
        /// </summary>
        public TElement Peek()
        {
            if (_heap.Count == 0)
                throw new InvalidOperationException("Queue is empty");

            return _heap[0].Element;
        }

        /// <summary>
        /// 查看队首元素和优先级（不移除）
        /// </summary>
        public (TElement Element, TPriority Priority) PeekWithPriority()
        {
            if (_heap.Count == 0)
                throw new InvalidOperationException("Queue is empty");

            return _heap[0];
        }

        /// <summary>
        /// 尝试出队
        /// </summary>
        public bool TryDequeue(out TElement element, out TPriority priority)
        {
            if (_heap.Count == 0)
            {
                element = default;
                priority = default;
                return false;
            }

            var result = DequeueWithPriority();
            element = result.Element;
            priority = result.Priority;
            return true;
        }

        /// <summary>
        /// 尝试查看队首
        /// </summary>
        public bool TryPeek(out TElement element, out TPriority priority)
        {
            if (_heap.Count == 0)
            {
                element = default;
                priority = default;
                return false;
            }

            element = _heap[0].Element;
            priority = _heap[0].Priority;
            return true;
        }

        /// <summary>
        /// 清空队列
        /// </summary>
        public void Clear()
        {
            _heap.Clear();
        }

        /// <summary>
        /// 获取所有元素（不保证顺序）
        /// </summary>
        public IEnumerable<TElement> UnorderedItems()
        {
            foreach (var item in _heap)
            {
                yield return item.Element;
            }
        }

        /// <summary>
        /// 获取所有元素和优先级（不保证顺序）
        /// </summary>
        public IEnumerable<(TElement Element, TPriority Priority)> UnorderedItemsWithPriority()
        {
            return _heap;
        }

        private void SiftUp(int index)
        {
            while (index > 0)
            {
                int parentIndex = (index - 1) / 2;
                if (Compare(index, parentIndex) <= 0)
                    break;

                Swap(index, parentIndex);
                index = parentIndex;
            }
        }

        private void SiftDown(int index)
        {
            int count = _heap.Count;

            while (true)
            {
                int leftChild = index * 2 + 1;
                int rightChild = index * 2 + 2;
                int extremeIndex = index;

                if (leftChild < count && Compare(leftChild, extremeIndex) > 0)
                    extremeIndex = leftChild;

                if (rightChild < count && Compare(rightChild, extremeIndex) > 0)
                    extremeIndex = rightChild;

                if (extremeIndex == index)
                    break;

                Swap(index, extremeIndex);
                index = extremeIndex;
            }
        }

        private int Compare(int i, int j)
        {
            int result = _comparer.Compare(_heap[i].Priority, _heap[j].Priority);
            return _isMaxHeap ? result : -result;
        }

        private void Swap(int i, int j)
        {
            var temp = _heap[i];
            _heap[i] = _heap[j];
            _heap[j] = temp;
        }
    }
#endif
}
