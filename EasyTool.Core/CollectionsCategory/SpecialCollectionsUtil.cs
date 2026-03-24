using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace EasyTool.CollectionsCategory
{
    /// <summary>
    /// 位集合工具类
    /// </summary>
    public static class BitSetUtil
    {
        /// <summary>
        /// 创建位集合
        /// </summary>
        public static BitSet Create(int capacity)
        {
            return new BitSet(capacity);
        }

        /// <summary>
        /// 从数组创建位集合
        /// </summary>
        public static BitSet FromArray(bool[] values)
        {
            var bitSet = new BitSet(values.Length);
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i])
                    bitSet.Set(i);
            }
            return bitSet;
        }
    }

    /// <summary>
    /// 位集合实现
    /// </summary>
    public class BitSet
    {
        private readonly int[] _data;
        private readonly int _capacity;

        /// <summary>
        /// 位数
        /// </summary>
        public int Capacity => _capacity;

        /// <summary>
        /// 设置为 1 的位数
        /// </summary>
        public int Cardinality
        {
            get
            {
                int count = 0;
                for (int i = 0; i < _data.Length; i++)
                {
                    count += PopCount(_data[i]);
                }
                return count;
            }
        }

        /// <summary>
        /// 是否为空
        /// </summary>
        public bool IsEmpty => Cardinality == 0;

        /// <summary>
        /// 访问指定位
        /// </summary>
        public bool this[int index]
        {
            get => Get(index);
            set
            {
                if (value)
                    Set(index);
                else
                    Clear(index);
            }
        }

        /// <summary>
        /// 创建位集合
        /// </summary>
        public BitSet(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity));

            _capacity = capacity;
            _data = new int[(capacity + 31) / 32];
        }

        /// <summary>
        /// 设置指定位为 1
        /// </summary>
        public void Set(int index)
        {
            if (index < 0 || index >= _capacity)
                throw new ArgumentOutOfRangeException(nameof(index));

            _data[index / 32] |= 1 << (index % 32);
        }

        /// <summary>
        /// 设置指定位为 0
        /// </summary>
        public void Clear(int index)
        {
            if (index < 0 || index >= _capacity)
                throw new ArgumentOutOfRangeException(nameof(index));

            _data[index / 32] &= ~(1 << (index % 32));
        }

        /// <summary>
        /// 翻转指定位
        /// </summary>
        public void Flip(int index)
        {
            if (index < 0 || index >= _capacity)
                throw new ArgumentOutOfRangeException(nameof(index));

            _data[index / 32] ^= 1 << (index % 32);
        }

        /// <summary>
        /// 获取指定位的值
        /// </summary>
        public bool Get(int index)
        {
            if (index < 0 || index >= _capacity)
                throw new ArgumentOutOfRangeException(nameof(index));

            return (_data[index / 32] & (1 << (index % 32))) != 0;
        }

        /// <summary>
        /// 设置所有位为 1
        /// </summary>
        public void SetAll()
        {
            for (int i = 0; i < _data.Length; i++)
            {
                _data[i] = -1;
            }
            ClearExtraBits();
        }

        /// <summary>
        /// 设置所有位为 0
        /// </summary>
        public void ClearAll()
        {
            Array.Clear(_data, 0, _data.Length);
        }

        /// <summary>
        /// 与操作
        /// </summary>
        public void And(BitSet other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            for (int i = 0; i < Math.Min(_data.Length, other._data.Length); i++)
            {
                _data[i] &= other._data[i];
            }
        }

        /// <summary>
        /// 或操作
        /// </summary>
        public void Or(BitSet other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            for (int i = 0; i < Math.Min(_data.Length, other._data.Length); i++)
            {
                _data[i] |= other._data[i];
            }
        }

        /// <summary>
        /// 异或操作
        /// </summary>
        public void Xor(BitSet other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            for (int i = 0; i < Math.Min(_data.Length, other._data.Length); i++)
            {
                _data[i] ^= other._data[i];
            }
        }

        /// <summary>
        /// 与非操作
        /// </summary>
        public void AndNot(BitSet other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            for (int i = 0; i < Math.Min(_data.Length, other._data.Length); i++)
            {
                _data[i] &= ~other._data[i];
            }
        }

        /// <summary>
        /// 获取下一个设置为 1 的位
        /// </summary>
        public int NextSetBit(int fromIndex)
        {
            if (fromIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(fromIndex));

            int wordIndex = fromIndex / 32;
            if (wordIndex >= _data.Length)
                return -1;

            int word = _data[wordIndex] & (~0 << (fromIndex % 32));

            while (true)
            {
                if (word != 0)
                {
                    int result = wordIndex * 32 + TrailingZeroCount(word);
                    return result < _capacity ? result : -1;
                }

                wordIndex++;
                if (wordIndex >= _data.Length)
                    return -1;

                word = _data[wordIndex];
            }
        }

        /// <summary>
        /// 获取下一个设置为 0 的位
        /// </summary>
        public int NextClearBit(int fromIndex)
        {
            if (fromIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(fromIndex));

            int wordIndex = fromIndex / 32;
            if (wordIndex >= _data.Length)
                return -1;

            int word = ~_data[wordIndex] & (~0 << (fromIndex % 32));

            while (true)
            {
                if (word != 0)
                {
                    int result = wordIndex * 32 + TrailingZeroCount(word);
                    return result < _capacity ? result : -1;
                }

                wordIndex++;
                if (wordIndex >= _data.Length)
                    return fromIndex < _capacity ? fromIndex : -1;

                word = ~_data[wordIndex];
            }
        }

        /// <summary>
        /// 克隆
        /// </summary>
        public BitSet Clone()
        {
            var clone = new BitSet(_capacity);
            Array.Copy(_data, clone._data, _data.Length);
            return clone;
        }

        /// <summary>
        /// 转换为布尔数组
        /// </summary>
        public bool[] ToArray()
        {
            var result = new bool[_capacity];
            for (int i = 0; i < _capacity; i++)
            {
                result[i] = Get(i);
            }
            return result;
        }

        private void ClearExtraBits()
        {
            int extraBits = _data.Length * 32 - _capacity;
            if (extraBits > 0)
            {
                _data[_data.Length - 1] &= ~(-1 << (32 - extraBits));
            }
        }

        private static int PopCount(int x)
        {
            x = x - ((x >> 1) & 0x55555555);
            x = (x & 0x33333333) + ((x >> 2) & 0x33333333);
            x = (x + (x >> 4)) & 0x0F0F0F0F;
            return (x * 0x01010101) >> 24;
        }

        private static int TrailingZeroCount(int x)
        {
            if (x == 0)
                return 32;

            int count = 0;
            while ((x & 1) == 0)
            {
                count++;
                x >>= 1;
            }
            return count;
        }
    }

    /// <summary>
    /// 稀疏数组工具类
    /// </summary>
    public static class SparseArrayUtil
    {
        /// <summary>
        /// 创建稀疏数组
        /// </summary>
        public static SparseArray<T> Create<T>(int capacity = 16)
        {
            return new SparseArray<T>(capacity);
        }
    }

    /// <summary>
    /// 稀疏数组实现
    /// 使用字典存储非默认值元素，节省内存
    /// </summary>
    public class SparseArray<T>
    {
        private readonly T _defaultValue;
        private readonly Dictionary<int, T> _data;
        private int _length;

        /// <summary>
        /// 逻辑长度
        /// </summary>
        public int Length => _length;

        /// <summary>
        /// 非默认值元素数量
        /// </summary>
        public int NonDefaultCount => _data.Count;

        /// <summary>
        /// 访问元素
        /// </summary>
        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= _length)
                    throw new ArgumentOutOfRangeException(nameof(index));

                return _data.TryGetValue(index, out var value) ? value : _defaultValue;
            }
            set
            {
                if (index < 0)
                    throw new ArgumentOutOfRangeException(nameof(index));

                if (index >= _length)
                    _length = index + 1;

                if (EqualityComparer<T>.Default.Equals(value, _defaultValue))
                {
                    _data.Remove(index);
                }
                else
                {
                    _data[index] = value;
                }
            }
        }

        /// <summary>
        /// 创建稀疏数组
        /// </summary>
        public SparseArray(int capacity = 16) : this(default, capacity)
        {
        }

        /// <summary>
        /// 创建稀疏数组（指定默认值）
        /// </summary>
        public SparseArray(T defaultValue, int capacity = 16)
        {
            _defaultValue = defaultValue;
            _data = new Dictionary<int, T>(capacity);
            _length = 0;
        }

        /// <summary>
        /// 设置长度
        /// </summary>
        public void SetLength(int length)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length));

            _length = length;

            // 移除超出长度的元素
            var keysToRemove = _data.Keys.Where(k => k >= length).ToList();
            foreach (var key in keysToRemove)
            {
                _data.Remove(key);
            }
        }

        /// <summary>
        /// 获取所有非默认值索引
        /// </summary>
        public IEnumerable<int> GetNonDefaultIndices()
        {
            return _data.Keys;
        }

        /// <summary>
        /// 转换为常规数组
        /// </summary>
        public T[] ToArray()
        {
            var result = new T[_length];
            for (int i = 0; i < _length; i++)
            {
                result[i] = this[i];
            }
            return result;
        }

        /// <summary>
        /// 清空
        /// </summary>
        public void Clear()
        {
            _data.Clear();
            _length = 0;
        }
    }

    /// <summary>
    /// 有界队列工具类
    /// </summary>
    public static class BoundedQueueUtil
    {
        /// <summary>
        /// 创建有界队列
        /// </summary>
        public static BoundedQueue<T> Create<T>(int capacity)
        {
            return new BoundedQueue<T>(capacity);
        }
    }

    /// <summary>
    /// 有界队列实现
    /// 当队列满时，可选择阻塞、丢弃新元素或丢弃旧元素
    /// </summary>
    public class BoundedQueue<T>
    {
        private readonly Queue<T> _queue;
        private readonly int _capacity;
        private readonly object _lock = new object();

        /// <summary>
        /// 容量
        /// </summary>
        public int Capacity => _capacity;

        /// <summary>
        /// 当前数量
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
        /// 是否已满
        /// </summary>
        public bool IsFull => Count >= _capacity;

        /// <summary>
        /// 是否为空
        /// </summary>
        public bool IsEmpty => Count == 0;

        /// <summary>
        /// 溢出策略
        /// </summary>
        public OverflowPolicy Policy { get; set; }

        /// <summary>
        /// 创建有界队列
        /// </summary>
        public BoundedQueue(int capacity, OverflowPolicy policy = OverflowPolicy.Block)
        {
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(capacity));

            _capacity = capacity;
            _queue = new Queue<T>(capacity);
            Policy = policy;
        }

        /// <summary>
        /// 尝试入队
        /// </summary>
        public bool TryEnqueue(T item)
        {
            lock (_lock)
            {
                switch (Policy)
                {
                    case OverflowPolicy.Block:
                        if (_queue.Count >= _capacity)
                            return false;
                        break;

                    case OverflowPolicy.DropNewest:
                        if (_queue.Count >= _capacity)
                            return false;
                        break;

                    case OverflowPolicy.DropOldest:
                        if (_queue.Count >= _capacity)
                            _queue.Dequeue();
                        break;
                }

                _queue.Enqueue(item);
                return true;
            }
        }

        /// <summary>
        /// 尝试出队
        /// </summary>
        public bool TryDequeue(out T item)
        {
            lock (_lock)
            {
                if (_queue.Count == 0)
                {
                    item = default;
                    return false;
                }

                item = _queue.Dequeue();
                return true;
            }
        }

        /// <summary>
        /// 尝试查看队首
        /// </summary>
        public bool TryPeek(out T item)
        {
            lock (_lock)
            {
                if (_queue.Count == 0)
                {
                    item = default;
                    return false;
                }

                item = _queue.Peek();
                return true;
            }
        }

        /// <summary>
        /// 清空
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                _queue.Clear();
            }
        }
    }

    /// <summary>
    /// 溢出策略
    /// </summary>
    public enum OverflowPolicy
    {
        /// <summary>
        /// 阻塞（拒绝新元素）
        /// </summary>
        Block,

        /// <summary>
        /// 丢弃最新元素
        /// </summary>
        DropNewest,

        /// <summary>
        /// 丢弃最旧元素
        /// </summary>
        DropOldest
    }

    /// <summary>
    /// 延迟队列工具类
    /// </summary>
    public static class DelayedQueueUtil
    {
        /// <summary>
        /// 创建延迟队列
        /// </summary>
        public static DelayedQueue<T> Create<T>()
        {
            return new DelayedQueue<T>();
        }
    }

    /// <summary>
    /// 延迟队列实现
    /// 元素在指定时间后才能被取出
    /// </summary>
    public class DelayedQueue<T>
    {
        private readonly List<DelayedItem> _items;
        private readonly object _lock = new object();

        /// <summary>
        /// 当前元素数量
        /// </summary>
        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return _items.Count;
                }
            }
        }

        /// <summary>
        /// 可用元素数量
        /// </summary>
        public int AvailableCount
        {
            get
            {
                lock (_lock)
                {
                    return _items.Count(i => i.IsAvailable);
                }
            }
        }

        /// <summary>
        /// 创建延迟队列
        /// </summary>
        public DelayedQueue()
        {
            _items = new List<DelayedItem>();
        }

        /// <summary>
        /// 入队（延迟指定时间）
        /// </summary>
        public void Enqueue(T item, TimeSpan delay)
        {
            lock (_lock)
            {
                var availableAt = DateTime.UtcNow.Add(delay);
                _items.Add(new DelayedItem(item, availableAt));
            }
        }

        /// <summary>
        /// 入队（指定可用时间）
        /// </summary>
        public void EnqueueAt(T item, DateTime availableAt)
        {
            lock (_lock)
            {
                _items.Add(new DelayedItem(item, availableAt));
            }
        }

        /// <summary>
        /// 尝试出队（仅返回已到期的元素）
        /// </summary>
        public bool TryDequeue(out T item)
        {
            lock (_lock)
            {
                var now = DateTime.UtcNow;
                var index = _items.FindIndex(i => i.AvailableAt <= now);

                if (index >= 0)
                {
                    item = _items[index].Value;
                    _items.RemoveAt(index);
                    return true;
                }

                item = default;
                return false;
            }
        }

        /// <summary>
        /// 尝试查看队首
        /// </summary>
        public bool TryPeek(out T item, out TimeSpan remainingDelay)
        {
            lock (_lock)
            {
                CleanupExpired();

                if (_items.Count == 0)
                {
                    item = default;
                    remainingDelay = TimeSpan.Zero;
                    return false;
                }

                var first = _items.OrderBy(i => i.AvailableAt).First();
                item = first.Value;
                remainingDelay = first.AvailableAt - DateTime.UtcNow;

                if (remainingDelay < TimeSpan.Zero)
                    remainingDelay = TimeSpan.Zero;

                return true;
            }
        }

        /// <summary>
        /// 清空
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                _items.Clear();
            }
        }

        private void CleanupExpired()
        {
            var now = DateTime.UtcNow;
            _items.RemoveAll(i => i.AvailableAt <= now);
        }

        private class DelayedItem
        {
            public T Value { get; }
            public DateTime AvailableAt { get; }
            public bool IsAvailable => DateTime.UtcNow >= AvailableAt;

            public DelayedItem(T value, DateTime availableAt)
            {
                Value = value;
                AvailableAt = availableAt;
            }
        }
    }

    /// <summary>
    /// 区间树工具类
    /// </summary>
    public static class IntervalTreeUtil
    {
        /// <summary>
        /// 创建区间树
        /// </summary>
        public static IntervalTree<T> Create<T>() where T : IComparable<T>
        {
            return new IntervalTree<T>();
        }
    }

    /// <summary>
    /// 区间树实现
    /// 高效查询与指定区间重叠的所有区间
    /// </summary>
    public class IntervalTree<T> where T : IComparable<T>
    {
        private IntervalNode _root;

        /// <summary>
        /// 区间数量
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// 创建区间树
        /// </summary>
        public IntervalTree()
        {
            Count = 0;
        }

        /// <summary>
        /// 添加区间
        /// </summary>
        public void Add(T start, T end, object data = null)
        {
            if (start.CompareTo(end) > 0)
                throw new ArgumentException("Start must be less than or equal to end");

            var interval = new Interval(start, end, data);
            _root = Insert(_root, interval);
            Count++;
        }

        /// <summary>
        /// 查询与指定点重叠的区间
        /// </summary>
        public List<Interval> Query(T point)
        {
            var result = new List<Interval>();
            Query(_root, point, result);
            return result;
        }

        /// <summary>
        /// 查询与指定区间重叠的区间
        /// </summary>
        public List<Interval> Query(T start, T end)
        {
            var result = new List<Interval>();
            Query(_root, start, end, result);
            return result;
        }

        /// <summary>
        /// 清空
        /// </summary>
        public void Clear()
        {
            _root = null;
            Count = 0;
        }

        private IntervalNode Insert(IntervalNode node, Interval interval)
        {
            if (node == null)
            {
                return new IntervalNode(interval);
            }

            int cmp = interval.Start.CompareTo(node.Interval.Start);
            if (cmp < 0)
            {
                node.Left = Insert(node.Left, interval);
            }
            else
            {
                node.Right = Insert(node.Right, interval);
            }

            // 更新最大值
            if (interval.End.CompareTo(node.MaxEnd) > 0)
            {
                node.MaxEnd = interval.End;
            }

            return node;
        }

        private void Query(IntervalNode node, T point, List<Interval> result)
        {
            if (node == null)
                return;

            // 如果点小于区间起点，且大于最大终点，则没有重叠
            if (point.CompareTo(node.Interval.Start) < 0 &&
                point.CompareTo(node.MaxEnd) > 0)
            {
                return;
            }

            // 检查左子树
            if (node.Left != null && point.CompareTo(node.Left.MaxEnd) <= 0)
            {
                Query(node.Left, point, result);
            }

            // 检查当前节点
            if (node.Interval.Contains(point))
            {
                result.Add(node.Interval);
            }

            // 检查右子树（如果点 >= 当前区间起点）
            if (point.CompareTo(node.Interval.Start) >= 0)
            {
                Query(node.Right, point, result);
            }
        }

        private void Query(IntervalNode node, T start, T end, List<Interval> result)
        {
            if (node == null)
                return;

            // 如果查询区间完全在最大终点之后，无需继续
            if (start.CompareTo(node.MaxEnd) > 0)
                return;

            // 检查左子树
            Query(node.Left, start, end, result);

            // 检查当前节点
            if (node.Interval.Overlaps(start, end))
            {
                result.Add(node.Interval);
            }

            // 如果查询区间完全在当前区间之前，无需检查右子树
            if (end.CompareTo(node.Interval.Start) < 0)
                return;

            // 检查右子树
            Query(node.Right, start, end, result);
        }

        private class IntervalNode
        {
            public Interval Interval { get; }
            public IntervalNode Left { get; set; }
            public IntervalNode Right { get; set; }
            public T MaxEnd { get; set; }

            public IntervalNode(Interval interval)
            {
                Interval = interval;
                MaxEnd = interval.End;
            }
        }

        /// <summary>
        /// 区间
        /// </summary>
        public class Interval
        {
            /// <summary>
            /// 起点
            /// </summary>
            public T Start { get; }

            /// <summary>
            /// 终点
            /// </summary>
            public T End { get; }

            /// <summary>
            /// 关联数据
            /// </summary>
            public object Data { get; }

            /// <summary>
            /// 创建区间
            /// </summary>
            public Interval(T start, T end, object data = null)
            {
                Start = start;
                End = end;
                Data = data;
            }

            /// <summary>
            /// 是否包含指定点
            /// </summary>
            public bool Contains(T point)
            {
                return Start.CompareTo(point) <= 0 && End.CompareTo(point) >= 0;
            }

            /// <summary>
            /// 是否与指定区间重叠
            /// </summary>
            public bool Overlaps(T start, T end)
            {
                return Start.CompareTo(end) <= 0 && End.CompareTo(start) >= 0;
            }

            /// <summary>
            /// 是否与指定区间重叠
            /// </summary>
            public bool Overlaps(Interval other)
            {
                return Overlaps(other.Start, other.End);
            }

            public override string ToString()
            {
                return $"[{Start}, {End}]";
            }
        }
    }

    /// <summary>
    /// 有序多重集工具类
    /// </summary>
    public static class SortedMultiSetUtil
    {
        /// <summary>
        /// 创建有序多重集
        /// </summary>
        public static SortedMultiSet<T> Create<T>() where T : IComparable<T>
        {
            return new SortedMultiSet<T>();
        }
    }

    /// <summary>
    /// 有序多重集实现
    /// 允许重复元素，保持排序
    /// </summary>
    public class SortedMultiSet<T> : IEnumerable<T> where T : IComparable<T>
    {
        private readonly SortedDictionary<T, int> _dict;
        private int _count;

        /// <summary>
        /// 元素总数
        /// </summary>
        public int Count => _count;

        /// <summary>
        /// 不同元素数量
        /// </summary>
        public int UniqueCount => _dict.Count;

        /// <summary>
        /// 最小值
        /// </summary>
        public T Min => _dict.Count > 0 ? _dict.First().Key : throw new InvalidOperationException("Set is empty");

        /// <summary>
        /// 最大值
        /// </summary>
        public T Max => _dict.Count > 0 ? _dict.Last().Key : throw new InvalidOperationException("Set is empty");

        /// <summary>
        /// 创建有序多重集
        /// </summary>
        public SortedMultiSet()
        {
            _dict = new SortedDictionary<T, int>();
            _count = 0;
        }

        /// <summary>
        /// 添加元素
        /// </summary>
        public void Add(T item)
        {
            if (_dict.ContainsKey(item))
            {
                _dict[item]++;
            }
            else
            {
                _dict[item] = 1;
            }
            _count++;
        }

        /// <summary>
        /// 移除一个元素
        /// </summary>
        public bool Remove(T item)
        {
            if (!_dict.TryGetValue(item, out var count))
                return false;

            if (count == 1)
            {
                _dict.Remove(item);
            }
            else
            {
                _dict[item] = count - 1;
            }
            _count--;
            return true;
        }

        /// <summary>
        /// 移除所有指定元素
        /// </summary>
        public int RemoveAll(T item)
        {
            if (!_dict.TryGetValue(item, out var count))
                return 0;

            _dict.Remove(item);
            _count -= count;
            return count;
        }

        /// <summary>
        /// 获取元素数量
        /// </summary>
        public int GetCount(T item)
        {
            return _dict.TryGetValue(item, out var count) ? count : 0;
        }

        /// <summary>
        /// 是否包含元素
        /// </summary>
        public bool Contains(T item)
        {
            return _dict.ContainsKey(item);
        }

        /// <summary>
        /// 清空
        /// </summary>
        public void Clear()
        {
            _dict.Clear();
            _count = 0;
        }

        /// <summary>
        /// 获取小于指定值的元素数量
        /// </summary>
        public int CountLessThan(T value)
        {
            int count = 0;
            foreach (var kvp in _dict)
            {
                if (kvp.Key.CompareTo(value) >= 0)
                    break;
                count += kvp.Value;
            }
            return count;
        }

        /// <summary>
        /// 获取大于指定值的元素数量
        /// </summary>
        public int CountGreaterThan(T value)
        {
            int count = 0;
            foreach (var kvp in _dict.Reverse())
            {
                if (kvp.Key.CompareTo(value) <= 0)
                    break;
                count += kvp.Value;
            }
            return count;
        }

        /// <summary>
        /// 获取指定范围内的元素数量
        /// </summary>
        public int CountInRange(T min, T max)
        {
            int count = 0;
            foreach (var kvp in _dict)
            {
                if (kvp.Key.CompareTo(max) > 0)
                    break;
                if (kvp.Key.CompareTo(min) >= 0)
                    count += kvp.Value;
            }
            return count;
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach (var kvp in _dict)
            {
                for (int i = 0; i < kvp.Value; i++)
                {
                    yield return kvp.Key;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
