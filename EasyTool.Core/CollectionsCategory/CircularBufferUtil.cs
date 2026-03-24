using System;
using System.Collections;
using System.Collections.Generic;

namespace EasyTool.CollectionsCategory
{
    /// <summary>
    /// 环形缓冲区工具类
    /// 固定大小的循环缓冲区，当满时自动覆盖最旧的数据
    /// 适用于日志缓冲、事件队列、滑动窗口等场景
    /// </summary>
    public static class CircularBufferUtil
    {
        /// <summary>
        /// 创建环形缓冲区
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="capacity">容量</param>
        /// <returns>环形缓冲区实例</returns>
        public static CircularBuffer<T> Create<T>(int capacity)
        {
            return new CircularBuffer<T>(capacity);
        }

        /// <summary>
        /// 从集合创建环形缓冲区
        /// </summary>
        public static CircularBuffer<T> FromEnumerable<T>(IEnumerable<T> collection, int capacity)
        {
            return new CircularBuffer<T>(capacity, collection);
        }
    }

    /// <summary>
    /// 环形缓冲区实现
    /// </summary>
    /// <typeparam name="T">元素类型</typeparam>
    public class CircularBuffer<T> : IEnumerable<T>, IReadOnlyCollection<T>
    {
        private readonly T[] _buffer;
        private int _head;
        private int _tail;
        private int _count;

        /// <summary>
        /// 缓冲区容量
        /// </summary>
        public int Capacity => _buffer.Length;

        /// <summary>
        /// 当前元素数量
        /// </summary>
        public int Count => _count;

        /// <summary>
        /// 是否为空
        /// </summary>
        public bool IsEmpty => _count == 0;

        /// <summary>
        /// 是否已满
        /// </summary>
        public bool IsFull => _count == Capacity;

        /// <summary>
        /// 索引访问元素
        /// </summary>
        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= _count)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return _buffer[(_head + index) % Capacity];
            }
        }

        /// <summary>
        /// 创建环形缓冲区
        /// </summary>
        /// <param name="capacity">容量（必须大于0）</param>
        public CircularBuffer(int capacity)
        {
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be greater than 0");

            _buffer = new T[capacity];
            _head = 0;
            _tail = 0;
            _count = 0;
        }

        /// <summary>
        /// 从集合创建环形缓冲区
        /// </summary>
        public CircularBuffer(int capacity, IEnumerable<T> collection) : this(capacity)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            foreach (var item in collection)
            {
                Push(item);
            }
        }

        /// <summary>
        /// 添加元素到尾部
        /// </summary>
        public void Push(T item)
        {
            _buffer[_tail] = item;
            _tail = (_tail + 1) % Capacity;

            if (IsFull)
            {
                _head = (_head + 1) % Capacity;
            }
            else
            {
                _count++;
            }
        }

        /// <summary>
        /// 批量添加元素
        /// </summary>
        public void PushRange(IEnumerable<T> items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            foreach (var item in items)
            {
                Push(item);
            }
        }

        /// <summary>
        /// 从头部移除并返回元素
        /// </summary>
        public T Pop()
        {
            if (IsEmpty)
                throw new InvalidOperationException("Buffer is empty");

            var item = _buffer[_head];
            _buffer[_head] = default;
            _head = (_head + 1) % Capacity;
            _count--;

            return item;
        }

        /// <summary>
        /// 从尾部移除并返回元素
        /// </summary>
        public T PopLast()
        {
            if (IsEmpty)
                throw new InvalidOperationException("Buffer is empty");

            _tail = (_tail - 1 + Capacity) % Capacity;
            var item = _buffer[_tail];
            _buffer[_tail] = default;
            _count--;

            return item;
        }

        /// <summary>
        /// 查看头部元素（不移除）
        /// </summary>
        public T Peek()
        {
            if (IsEmpty)
                throw new InvalidOperationException("Buffer is empty");

            return _buffer[_head];
        }

        /// <summary>
        /// 查看尾部元素（不移除）
        /// </summary>
        public T PeekLast()
        {
            if (IsEmpty)
                throw new InvalidOperationException("Buffer is empty");

            int index = (_tail - 1 + Capacity) % Capacity;
            return _buffer[index];
        }

        /// <summary>
        /// 清空缓冲区
        /// </summary>
        public void Clear()
        {
            Array.Clear(_buffer, 0, Capacity);
            _head = 0;
            _tail = 0;
            _count = 0;
        }

        /// <summary>
        /// 判断是否包含指定元素
        /// </summary>
        public bool Contains(T item)
        {
            for (int i = 0; i < _count; i++)
            {
                int index = (_head + i) % Capacity;
                if (EqualityComparer<T>.Default.Equals(_buffer[index], item))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 复制到数组
        /// </summary>
        public T[] ToArray()
        {
            var result = new T[_count];
            for (int i = 0; i < _count; i++)
            {
                result[i] = _buffer[(_head + i) % Capacity];
            }
            return result;
        }

        /// <summary>
        /// 获取最新的N个元素
        /// </summary>
        public T[] GetLatest(int count)
        {
            count = Math.Min(count, _count);
            var result = new T[count];

            for (int i = 0; i < count; i++)
            {
                int sourceIndex = (_head + _count - count + i) % Capacity;
                result[i] = _buffer[sourceIndex];
            }

            return result;
        }

        /// <summary>
        /// 获取最旧的N个元素
        /// </summary>
        public T[] GetOldest(int count)
        {
            count = Math.Min(count, _count);
            var result = new T[count];

            for (int i = 0; i < count; i++)
            {
                result[i] = _buffer[(_head + i) % Capacity];
            }

            return result;
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < _count; i++)
            {
                yield return _buffer[(_head + i) % Capacity];
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
