using System;
using System.Threading;

namespace EasyTool
{
    /// <summary>
    /// 环形缓冲区
    /// 高性能、线程安全的数据缓冲结构
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    public class RingBuffer<T>
    {
        private readonly T[] _buffer;
        private int _head;
        private int _tail;
        private int _count;
        private readonly object _lock = new object();
        private readonly bool _overwriteWhenFull;

        /// <summary>
        /// 缓冲区容量
        /// </summary>
        public int Capacity { get; }

        /// <summary>
        /// 当前数据数量
        /// </summary>
        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return _count;
                }
            }
        }

        /// <summary>
        /// 是否为空
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                lock (_lock)
                {
                    return _count == 0;
                }
            }
        }

        /// <summary>
        /// 是否已满
        /// </summary>
        public bool IsFull
        {
            get
            {
                lock (_lock)
                {
                    return _count == Capacity;
                }
            }
        }

        /// <summary>
        /// 创建环形缓冲区
        /// </summary>
        /// <param name="capacity">容量</param>
        /// <param name="overwriteWhenFull">满时是否覆盖旧数据</param>
        public RingBuffer(int capacity, bool overwriteWhenFull = true)
        {
            if (capacity <= 0)
                throw new ArgumentException("Capacity must be greater than 0", nameof(capacity));

            Capacity = capacity;
            _buffer = new T[capacity];
            _head = 0;
            _tail = 0;
            _count = 0;
            _overwriteWhenFull = overwriteWhenFull;
        }

        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="item">数据项</param>
        /// <returns>是否写入成功</returns>
        public bool Write(T item)
        {
            lock (_lock)
            {
                if (_count == Capacity)
                {
                    if (!_overwriteWhenFull)
                        return false;

                    // 覆盖最旧的数据
                    _head = (_head + 1) % Capacity;
                    _count--;
                }

                _buffer[_tail] = item;
                _tail = (_tail + 1) % Capacity;
                _count++;
                return true;
            }
        }

        /// <summary>
        /// 批量写入数据
        /// </summary>
        /// <param name="items">数据项数组</param>
        /// <returns>实际写入的数量</returns>
        public int Write(T[] items)
        {
            if (items == null || items.Length == 0)
                return 0;

            lock (_lock)
            {
                int written = 0;
                foreach (var item in items)
                {
                    if (_count == Capacity && !_overwriteWhenFull)
                        break;

                    if (_count == Capacity)
                    {
                        _head = (_head + 1) % Capacity;
                        _count--;
                    }

                    _buffer[_tail] = item;
                    _tail = (_tail + 1) % Capacity;
                    _count++;
                    written++;
                }
                return written;
            }
        }

        /// <summary>
        /// 读取数据（不移除）
        /// </summary>
        /// <returns>数据项</returns>
        public T? Peek()
        {
            lock (_lock)
            {
                if (_count == 0)
                    return default;

                return _buffer[_head];
            }
        }

        /// <summary>
        /// 读取并移除数据
        /// </summary>
        /// <returns>数据项</returns>
        public T? Read()
        {
            lock (_lock)
            {
                if (_count == 0)
                    return default;

                var item = _buffer[_head];
                _buffer[_head] = default!;
                _head = (_head + 1) % Capacity;
                _count--;
                return item;
            }
        }

        /// <summary>
        /// 批量读取并移除数据
        /// </summary>
        /// <param name="maxCount">最大读取数量</param>
        /// <returns>数据项数组</returns>
        public T[] Read(int maxCount)
        {
            lock (_lock)
            {
                if (_count == 0)
                    return Array.Empty<T>();

                var actualCount = Math.Min(maxCount, _count);
                var result = new T[actualCount];

                for (int i = 0; i < actualCount; i++)
                {
                    result[i] = _buffer[_head];
                    _buffer[_head] = default!;
                    _head = (_head + 1) % Capacity;
                }

                _count -= actualCount;
                return result;
            }
        }

        /// <summary>
        /// 读取所有数据并清空缓冲区
        /// </summary>
        /// <returns>数据项数组</returns>
        public T[] ReadAll()
        {
            lock (_lock)
            {
                if (_count == 0)
                    return Array.Empty<T>();

                var result = new T[_count];
                for (int i = 0; i < _count; i++)
                {
                    var index = (_head + i) % Capacity;
                    result[i] = _buffer[index];
                    _buffer[index] = default!;
                }

                _head = 0;
                _tail = 0;
                _count = 0;
                return result;
            }
        }

        /// <summary>
        /// 尝试读取数据
        /// </summary>
        /// <param name="item">数据项</param>
        /// <returns>是否读取成功</returns>
        public bool TryRead(out T? item)
        {
            item = Read();
            return _count >= 0 || !Equals(item, default(T));
        }

        /// <summary>
        /// 清空缓冲区
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                Array.Clear(_buffer, 0, _buffer.Length);
                _head = 0;
                _tail = 0;
                _count = 0;
            }
        }

        /// <summary>
        /// 复制当前缓冲区数据（不移除）
        /// </summary>
        /// <returns>数据副本</returns>
        public T[] ToArray()
        {
            lock (_lock)
            {
                if (_count == 0)
                    return Array.Empty<T>();

                var result = new T[_count];
                for (int i = 0; i < _count; i++)
                {
                    var index = (_head + i) % Capacity;
                    result[i] = _buffer[index];
                }
                return result;
            }
        }

        /// <summary>
        /// 获取指定索引的数据（不移除）
        /// </summary>
        /// <param name="index">索引（从最旧的数据开始）</param>
        /// <returns>数据项</returns>
        public T GetAt(int index)
        {
            lock (_lock)
            {
                if (index < 0 || index >= _count)
                    throw new IndexOutOfRangeException($"Index {index} is out of range. Buffer contains {_count} items.");

                var actualIndex = (_head + index) % Capacity;
                return _buffer[actualIndex];
            }
        }
    }

    /// <summary>
    /// 环形缓冲区扩展方法
    /// </summary>
    public static class RingBufferExtensions
    {
        /// <summary>
        /// 创建环形缓冲区
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="capacity">容量</param>
        /// <param name="overwriteWhenFull">满时是否覆盖旧数据</param>
        /// <returns>环形缓冲区实例</returns>
        public static RingBuffer<T> CreateRingBuffer<T>(int capacity, bool overwriteWhenFull = true)
        {
            return new RingBuffer<T>(capacity, overwriteWhenFull);
        }
    }
}