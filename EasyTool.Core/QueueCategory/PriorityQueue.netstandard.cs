#if NETSTANDARD2_1
using System;
using System.Collections;
using System.Collections.Generic;

namespace System.Collections.Generic
{
    /// <summary>
    /// 优先队列 polyfill for netstandard2.1
    /// </summary>
    /// <typeparam name="TElement">元素类型</typeparam>
    /// <typeparam name="TPriority">优先级类型</typeparam>
    public class PriorityQueue<TElement, TPriority>
    {
        private readonly List<(TElement Element, TPriority Priority)> _items;
        private readonly IComparer<TPriority>? _comparer;

        /// <summary>
        /// 获取队列中的元素数量
        /// </summary>
        public int Count => _items.Count;

        /// <summary>
        /// 创建优先队列
        /// </summary>
        public PriorityQueue()
        {
            _items = new List<(TElement, TPriority)>();
            _comparer = null;
        }

        /// <summary>
        /// 创建优先队列
        /// </summary>
        /// <param name="initialCapacity">初始容量</param>
        public PriorityQueue(int initialCapacity)
        {
            _items = new List<(TElement, TPriority)>(initialCapacity);
            _comparer = null;
        }

        /// <summary>
        /// 创建优先队列
        /// </summary>
        /// <param name="comparer">优先级比较器</param>
        public PriorityQueue(IComparer<TPriority>? comparer)
        {
            _items = new List<(TElement, TPriority)>();
            _comparer = comparer;
        }

        /// <summary>
        /// 创建优先队列
        /// </summary>
        /// <param name="initialCapacity">初始容量</param>
        /// <param name="comparer">优先级比较器</param>
        public PriorityQueue(int initialCapacity, IComparer<TPriority>? comparer)
        {
            _items = new List<(TElement, TPriority)>(initialCapacity);
            _comparer = comparer;
        }

        /// <summary>
        /// 入队
        /// </summary>
        /// <param name="element">元素</param>
        /// <param name="priority">优先级</param>
        public void Enqueue(TElement element, TPriority priority)
        {
            _items.Add((element, priority));
            HeapifyUp(_items.Count - 1);
        }

        /// <summary>
        /// 出队
        /// </summary>
        /// <returns>元素</returns>
        public TElement Dequeue()
        {
            if (_items.Count == 0)
                throw new InvalidOperationException("Queue is empty");

            var result = _items[0].Element;
            var lastIndex = _items.Count - 1;
            _items[0] = _items[lastIndex];
            _items.RemoveAt(lastIndex);

            if (_items.Count > 0)
                HeapifyDown(0);

            return result;
        }

        /// <summary>
        /// 尝试出队
        /// </summary>
        /// <param name="element">元素</param>
        /// <param name="priority">优先级</param>
        /// <returns>是否成功</returns>
        public bool TryDequeue(out TElement element, out TPriority priority)
        {
            if (_items.Count == 0)
            {
                element = default!;
                priority = default!;
                return false;
            }

            var item = _items[0];
            element = item.Element;
            priority = item.Priority;

            var lastIndex = _items.Count - 1;
            _items[0] = _items[lastIndex];
            _items.RemoveAt(lastIndex);

            if (_items.Count > 0)
                HeapifyDown(0);

            return true;
        }

        /// <summary>
        /// 查看队首元素
        /// </summary>
        /// <returns>元素</returns>
        public TElement Peek()
        {
            if (_items.Count == 0)
                throw new InvalidOperationException("Queue is empty");

            return _items[0].Element;
        }

        /// <summary>
        /// 尝试查看队首元素
        /// </summary>
        /// <param name="element">元素</param>
        /// <param name="priority">优先级</param>
        /// <returns>是否成功</returns>
        public bool TryPeek(out TElement element, out TPriority priority)
        {
            if (_items.Count == 0)
            {
                element = default!;
                priority = default!;
                return false;
            }

            var item = _items[0];
            element = item.Element;
            priority = item.Priority;
            return true;
        }

        /// <summary>
        /// 清空队列
        /// </summary>
        public void Clear()
        {
            _items.Clear();
        }

        private void HeapifyUp(int index)
        {
            var comparer = _comparer ?? Comparer<TPriority>.Default;
            while (index > 0)
            {
                var parentIndex = (index - 1) / 2;
                if (comparer.Compare(_items[index].Priority, _items[parentIndex].Priority) >= 0)
                    break;

                Swap(index, parentIndex);
                index = parentIndex;
            }
        }

        private void HeapifyDown(int index)
        {
            var comparer = _comparer ?? Comparer<TPriority>.Default;
            var count = _items.Count;

            while (true)
            {
                var leftChild = 2 * index + 1;
                var rightChild = 2 * index + 2;
                var smallest = index;

                if (leftChild < count && comparer.Compare(_items[leftChild].Priority, _items[smallest].Priority) < 0)
                    smallest = leftChild;

                if (rightChild < count && comparer.Compare(_items[rightChild].Priority, _items[smallest].Priority) < 0)
                    smallest = rightChild;

                if (smallest == index)
                    break;

                Swap(index, smallest);
                index = smallest;
            }
        }

        private void Swap(int i, int j)
        {
            var temp = _items[i];
            _items[i] = _items[j];
            _items[j] = temp;
        }
    }
}
#endif
