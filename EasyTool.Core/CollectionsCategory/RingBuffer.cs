using System;
using System.Collections.Generic;

namespace EasyTool.CollectionsCategory
{
    /// <summary>
    /// 环形缓冲区
    /// 线程安全，支持固定大小的循环队列
    /// </summary>
    /// <typeparam name="T">元素类型</typeparam>
    public class RingBuffer<T>
    {
        private readonly T[] _buffer;
        private int _head;
        private int _tail;
        private int _count;
        private readonly object _lock = new();

        /// <summary>
        /// 创建环形缓冲区
        /// </summary>
        /// <param name="capacity">容量</param>
        public RingBuffer(int capacity)
        {
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), "容量必须大于0");

            _buffer = new T[capacity];
            _head = 0;
            _tail = 0;
            _count = 0;
        }

        /// <summary>
        /// 容量
        /// </summary>
        public int Capacity => _buffer.Length;

        /// <summary>
        /// 当前元素数量
        /// </summary>
        public int Count
        {
            get { lock (_lock) { return _count; } }
        }

        /// <summary>
        /// 是否为空
        /// </summary>
        public bool IsEmpty => Count == 0;

        /// <summary>
        /// 是否已满
        /// </summary>
        public bool IsFull => Count == Capacity;

        /// <summary>
        /// 添加元素
        /// </summary>
        public void Add(T item)
        {
            lock (_lock)
            {
                _buffer[_tail] = item;
                _tail = (_tail + 1) % Capacity;

                if (_count == Capacity)
                {
                    // 缓冲区已满，覆盖最旧的元素
                    _head = (_head + 1) % Capacity;
                }
                else
                {
                    _count++;
                }
            }
        }

        /// <summary>
        /// 尝试添加元素（如果已满则返回false）
        /// </summary>
        public bool TryAdd(T item)
        {
            lock (_lock)
            {
                if (_count == Capacity)
                    return false;

                _buffer[_tail] = item;
                _tail = (_tail + 1) % Capacity;
                _count++;
                return true;
            }
        }

        /// <summary>
        /// 获取并移除最旧的元素
        /// </summary>
        public T? Take()
        {
            lock (_lock)
            {
                if (_count == 0)
                    throw new InvalidOperationException("缓冲区为空");

                var item = _buffer[_head];
                _buffer[_head] = default!;
                _head = (_head + 1) % Capacity;
                _count--;
                return item;
            }
        }

        /// <summary>
        /// 尝试获取并移除最旧的元素
        /// </summary>
        public bool TryTake(out T? item)
        {
            lock (_lock)
            {
                if (_count == 0)
                {
                    item = default;
                    return false;
                }

                item = _buffer[_head];
                _buffer[_head] = default!;
                _head = (_head + 1) % Capacity;
                _count--;
                return true;
            }
        }

        /// <summary>
        /// 查看最旧的元素（不移除）
        /// </summary>
        public T? Peek()
        {
            lock (_lock)
            {
                if (_count == 0)
                    throw new InvalidOperationException("缓冲区为空");
                return _buffer[_head];
            }
        }

        /// <summary>
        /// 尝试查看最旧的元素
        /// </summary>
        public bool TryPeek(out T? item)
        {
            lock (_lock)
            {
                if (_count == 0)
                {
                    item = default;
                    return false;
                }
                item = _buffer[_head];
                return true;
            }
        }

        /// <summary>
        /// 查看最新的元素（不移除）
        /// </summary>
        public T? PeekLatest()
        {
            lock (_lock)
            {
                if (_count == 0)
                    throw new InvalidOperationException("缓冲区为空");
                var index = (_tail - 1 + Capacity) % Capacity;
                return _buffer[index];
            }
        }

        /// <summary>
        /// 获取指定索引的元素（从最旧的开始）
        /// </summary>
        public T? GetAt(int index)
        {
            lock (_lock)
            {
                if (index < 0 || index >= _count)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return _buffer[(_head + index) % Capacity];
            }
        }

        /// <summary>
        /// 获取所有元素（从最旧到最新）
        /// </summary>
        public T[] ToArray()
        {
            lock (_lock)
            {
                var result = new T[_count];
                for (int i = 0; i < _count; i++)
                {
                    result[i] = _buffer[(_head + i) % Capacity];
                }
                return result;
            }
        }

        /// <summary>
        /// 清空缓冲区
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                Array.Clear(_buffer, 0, Capacity);
                _head = 0;
                _tail = 0;
                _count = 0;
            }
        }

        /// <summary>
        /// 遍历所有元素
        /// </summary>
        public IEnumerator<T> GetEnumerator()
        {
            T[] array;
            lock (_lock)
            {
                array = ToArray();
            }
            foreach (var item in array)
            {
                yield return item;
            }
        }

        /// <summary>
        /// 复制到数组
        /// </summary>
        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (_lock)
            {
                for (int i = 0; i < _count && arrayIndex + i < array.Length; i++)
                {
                    array[arrayIndex + i] = _buffer[(_head + i) % Capacity];
                }
            }
        }

        /// <summary>
        /// 查找元素
        /// </summary>
        public bool Contains(T item)
        {
            lock (_lock)
            {
                for (int i = 0; i < _count; i++)
                {
                    if (EqualityComparer<T>.Default.Equals(_buffer[(_head + i) % Capacity], item))
                        return true;
                }
                return false;
            }
        }

        /// <summary>
        /// 查找元素索引
        /// </summary>
        public int IndexOf(T item)
        {
            lock (_lock)
            {
                for (int i = 0; i < _count; i++)
                {
                    if (EqualityComparer<T>.Default.Equals(_buffer[(_head + i) % Capacity], item))
                        return i;
                }
                return -1;
            }
        }

        /// <summary>
        /// 获取最新的N个元素
        /// </summary>
        public T[] GetLatest(int count)
        {
            lock (_lock)
            {
                count = Math.Min(count, _count);
                var result = new T[count];
                for (int i = 0; i < count; i++)
                {
                    var index = (_tail - count + i + Capacity) % Capacity;
                    result[i] = _buffer[index];
                }
                return result;
            }
        }

        /// <summary>
        /// 获取最旧的N个元素
        /// </summary>
        public T[] GetOldest(int count)
        {
            lock (_lock)
            {
                count = Math.Min(count, _count);
                var result = new T[count];
                for (int i = 0; i < count; i++)
                {
                    result[i] = _buffer[(_head + i) % Capacity];
                }
                return result;
            }
        }
    }
}
