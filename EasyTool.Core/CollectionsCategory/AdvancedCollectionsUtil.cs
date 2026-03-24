using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace EasyTool.CollectionsCategory
{
    /// <summary>
    /// 高级集合工具类
    /// </summary>
    public static class AdvancedCollectionsUtil
    {
        /// <summary>
        /// 创建Roaring Bitmap
        /// </summary>
        public static RoaringBitmap CreateRoaringBitmap()
        {
            return new RoaringBitmap();
        }

        /// <summary>
        /// 从整数集合创建Roaring Bitmap
        /// </summary>
        public static RoaringBitmap CreateRoaringBitmap(IEnumerable<int> values)
        {
            var bitmap = new RoaringBitmap();
            foreach (var value in values)
            {
                bitmap.Add(value);
            }
            return bitmap;
        }

        /// <summary>
        /// 创建流式处理器
        /// </summary>
        public static StreamProcessor<T> CreateStreamProcessor<T>(int windowSize)
        {
            return new StreamProcessor<T>(windowSize);
        }
    }

    /// <summary>
    /// Roaring Bitmap
    /// 压缩位图，高效存储和操作整数集合
    /// </summary>
    public class RoaringBitmap : IEnumerable<int>
    {
        private readonly Dictionary<ushort, Container> _containers;

        private abstract class Container
        {
            public abstract int Count { get; }
            public abstract bool Contains(ushort value);
            public abstract void Add(ushort value);
            public abstract void Remove(ushort value);
            public abstract IEnumerator<ushort> GetEnumerator();
        }

        private class ArrayContainer : Container
        {
            private readonly List<ushort> _values;
            private const int MaxSize = 4096;

            public override int Count => _values.Count;

            public ArrayContainer()
            {
                _values = new List<ushort>();
            }

            public override bool Contains(ushort value)
            {
                return _values.BinarySearch(value) >= 0;
            }

            public override void Add(ushort value)
            {
                int index = _values.BinarySearch(value);
                if (index < 0)
                {
                    _values.Insert(~index, value);
                }
            }

            public override void Remove(ushort value)
            {
                int index = _values.BinarySearch(value);
                if (index >= 0)
                {
                    _values.RemoveAt(index);
                }
            }

            public override IEnumerator<ushort> GetEnumerator()
            {
                return _values.GetEnumerator();
            }

            public bool IsFull => _values.Count >= MaxSize;

            public BitmapContainer ToBitmapContainer()
            {
                var bitmap = new BitmapContainer();
                foreach (var value in _values)
                {
                    bitmap.Add(value);
                }
                return bitmap;
            }
        }

        private class BitmapContainer : Container
        {
            private readonly ulong[] _bitmap;
            private int _count;

            public override int Count => _count;

            public BitmapContainer()
            {
                _bitmap = new ulong[1024]; // 65536 bits / 64 = 1024
                _count = 0;
            }

            public override bool Contains(ushort value)
            {
                int index = value / 64;
                int bit = value % 64;
                return (_bitmap[index] & (1UL << bit)) != 0;
            }

            public override void Add(ushort value)
            {
                int index = value / 64;
                int bit = value % 64;
                if ((_bitmap[index] & (1UL << bit)) == 0)
                {
                    _bitmap[index] |= 1UL << bit;
                    _count++;
                }
            }

            public override void Remove(ushort value)
            {
                int index = value / 64;
                int bit = value % 64;
                if ((_bitmap[index] & (1UL << bit)) != 0)
                {
                    _bitmap[index] &= ~(1UL << bit);
                    _count--;
                }
            }

            public override IEnumerator<ushort> GetEnumerator()
            {
                for (int i = 0; i < _bitmap.Length; i++)
                {
                    if (_bitmap[i] == 0)
                        continue;

                    for (int bit = 0; bit < 64; bit++)
                    {
                        if ((_bitmap[i] & (1UL << bit)) != 0)
                        {
                            yield return (ushort)(i * 64 + bit);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 元素数量
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// 是否为空
        /// </summary>
        public bool IsEmpty => Count == 0;

        /// <summary>
        /// 创建Roaring Bitmap
        /// </summary>
        public RoaringBitmap()
        {
            _containers = new Dictionary<ushort, Container>();
            Count = 0;
        }

        /// <summary>
        /// 添加值
        /// </summary>
        public void Add(int value)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "值不能为负数");

            ushort high = (ushort)(value >> 16);
            ushort low = (ushort)(value & 0xFFFF);

            if (!_containers.TryGetValue(high, out var container))
            {
                container = new ArrayContainer();
                _containers[high] = container;
            }

            int oldCount = container.Count;
            container.Add(low);

            if (container.Count > oldCount)
                Count++;

            // 检查是否需要转换为位图容器
            if (container is ArrayContainer ac && ac.IsFull)
            {
                _containers[high] = ac.ToBitmapContainer();
            }
        }

        /// <summary>
        /// 批量添加
        /// </summary>
        public void AddRange(IEnumerable<int> values)
        {
            foreach (var value in values)
            {
                Add(value);
            }
        }

        /// <summary>
        /// 移除值
        /// </summary>
        public bool Remove(int value)
        {
            if (value < 0)
                return false;

            ushort high = (ushort)(value >> 16);
            ushort low = (ushort)(value & 0xFFFF);

            if (!_containers.TryGetValue(high, out var container))
                return false;

            int oldCount = container.Count;
            container.Remove(low);

            if (container.Count < oldCount)
            {
                Count--;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 是否包含值
        /// </summary>
        public bool Contains(int value)
        {
            if (value < 0)
                return false;

            ushort high = (ushort)(value >> 16);
            ushort low = (ushort)(value & 0xFFFF);

            return _containers.TryGetValue(high, out var container) && container.Contains(low);
        }

        /// <summary>
        /// 与操作
        /// </summary>
        public void And(RoaringBitmap other)
        {
            if (other == null)
                return;

            var keysToRemove = new List<ushort>();

            foreach (var kvp in _containers)
            {
                if (!other._containers.TryGetValue(kvp.Key, out var otherContainer))
                {
                    keysToRemove.Add(kvp.Key);
                }
                else
                {
                    // 简化实现：创建新的位图容器
                    var result = new BitmapContainer();
                    foreach (var value in kvp.Value)
                    {
                        if (otherContainer.Contains(value))
                        {
                            result.Add(value);
                        }
                    }

                    if (result.Count > 0)
                    {
                        _containers[kvp.Key] = result;
                    }
                    else
                    {
                        keysToRemove.Add(kvp.Key);
                    }
                }
            }

            foreach (var key in keysToRemove)
            {
                var container = _containers[key];
                Count -= container.Count;
                _containers.Remove(key);
            }
        }

        /// <summary>
        /// 或操作
        /// </summary>
        public void Or(RoaringBitmap other)
        {
            if (other == null)
                return;

            foreach (var kvp in other._containers)
            {
                if (!_containers.TryGetValue(kvp.Key, out var container))
                {
                    container = new BitmapContainer();
                    _containers[kvp.Key] = container;
                }

                foreach (var value in kvp.Value)
                {
                    int oldCount = container.Count;
                    container.Add(value);
                    if (container.Count > oldCount)
                        Count++;
                }
            }
        }

        /// <summary>
        /// 清空
        /// </summary>
        public void Clear()
        {
            _containers.Clear();
            Count = 0;
        }

        public IEnumerator<int> GetEnumerator()
        {
            foreach (var kvp in _containers.OrderBy(x => x.Key))
            {
                int high = kvp.Key << 16;
                foreach (var low in kvp.Value)
                {
                    yield return high | low;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>
    /// 流式数据处理器
    /// 支持滑动窗口聚合
    /// </summary>
    public class StreamProcessor<T>
    {
        private readonly int _windowSize;
        private readonly Queue<T> _window;
        private readonly List<IAggregator<T>> _aggregators;
        private long _totalCount;

        /// <summary>
        /// 窗口大小
        /// </summary>
        public int WindowSize => _windowSize;

        /// <summary>
        /// 当前窗口内元素数量
        /// </summary>
        public int WindowCount => _window.Count;

        /// <summary>
        /// 总处理元素数量
        /// </summary>
        public long TotalCount => _totalCount;

        /// <summary>
        /// 创建流式处理器
        /// </summary>
        public StreamProcessor(int windowSize)
        {
            if (windowSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(windowSize));

            _windowSize = windowSize;
            _window = new Queue<T>();
            _aggregators = new List<IAggregator<T>>();
            _totalCount = 0;
        }

        /// <summary>
        /// 添加聚合器
        /// </summary>
        public void AddAggregator(IAggregator<T> aggregator)
        {
            if (aggregator == null)
                throw new ArgumentNullException(nameof(aggregator));
            _aggregators.Add(aggregator);
        }

        /// <summary>
        /// 处理元素
        /// </summary>
        public void Process(T item)
        {
            // 通知聚合器有新元素
            foreach (var aggregator in _aggregators)
            {
                aggregator.Add(item);
            }

            _window.Enqueue(item);
            _totalCount++;

            // 如果窗口满了，移除最旧的元素
            if (_window.Count > _windowSize)
            {
                var removed = _window.Dequeue();
                foreach (var aggregator in _aggregators)
                {
                    aggregator.Remove(removed);
                }
            }
        }

        /// <summary>
        /// 批量处理
        /// </summary>
        public void ProcessRange(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                Process(item);
            }
        }

        /// <summary>
        /// 获取聚合结果
        /// </summary>
        public TResult GetResult<TResult>(string aggregatorName)
        {
            var aggregator = _aggregators.FirstOrDefault(a => a.Name == aggregatorName);
            if (aggregator is IAggregator<T, TResult> typedAggregator)
            {
                return typedAggregator.GetResult();
            }
            throw new ArgumentException($"Aggregator '{aggregatorName}' not found or has different result type");
        }

        /// <summary>
        /// 获取所有聚合结果
        /// </summary>
        public Dictionary<string, object> GetAllResults()
        {
            return _aggregators.ToDictionary(a => a.Name, a => a.GetResultObject());
        }

        /// <summary>
        /// 清空窗口
        /// </summary>
        public void Clear()
        {
            _window.Clear();
            foreach (var aggregator in _aggregators)
            {
                aggregator.Reset();
            }
            _totalCount = 0;
        }

        /// <summary>
        /// 获取窗口内元素
        /// </summary>
        public IReadOnlyCollection<T> GetWindow()
        {
            return _window.ToArray();
        }
    }

    /// <summary>
    /// 聚合器接口
    /// </summary>
    public interface IAggregator<T>
    {
        /// <summary>
        /// 名称
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 添加元素
        /// </summary>
        void Add(T item);

        /// <summary>
        /// 移除元素
        /// </summary>
        void Remove(T item);

        /// <summary>
        /// 重置
        /// </summary>
        void Reset();

        /// <summary>
        /// 获取结果（对象形式）
        /// </summary>
        object GetResultObject();
    }

    /// <summary>
    /// 聚合器接口（带结果类型）
    /// </summary>
    public interface IAggregator<T, TResult> : IAggregator<T>
    {
        /// <summary>
        /// 获取结果
        /// </summary>
        TResult GetResult();
    }

    /// <summary>
    /// 计数聚合器
    /// </summary>
    public class CountAggregator<T> : IAggregator<T, long>
    {
        private long _count;

        public string Name => "Count";

        public void Add(T item) => _count++;

        public void Remove(T item) => _count--;

        public void Reset() => _count = 0;

        public long GetResult() => _count;

        public object GetResultObject() => GetResult();
    }

    /// <summary>
    /// 求和聚合器
    /// </summary>
    public class SumAggregator : IAggregator<double, double>
    {
        private double _sum;

        public string Name => "Sum";

        public void Add(double item) => _sum += item;

        public void Remove(double item) => _sum -= item;

        public void Reset() => _sum = 0;

        public double GetResult() => _sum;

        public object GetResultObject() => GetResult();
    }

    /// <summary>
    /// 平均值聚合器
    /// </summary>
    public class AverageAggregator : IAggregator<double, double>
    {
        private double _sum;
        private long _count;

        public string Name => "Average";

        public void Add(double item)
        {
            _sum += item;
            _count++;
        }

        public void Remove(double item)
        {
            _sum -= item;
            _count--;
        }

        public void Reset()
        {
            _sum = 0;
            _count = 0;
        }

        public double GetResult() => _count > 0 ? _sum / _count : 0;

        public object GetResultObject() => GetResult();
    }

    /// <summary>
    /// 最小值聚合器
    /// </summary>
    public class MinAggregator<T> : IAggregator<T, T> where T : IComparable<T>
    {
        private readonly List<T> _items = new List<T>();

        public string Name => "Min";

        public void Add(T item) => _items.Add(item);

        public void Remove(T item) => _items.Remove(item);

        public void Reset() => _items.Clear();

        public T GetResult() => _items.Count > 0 ? _items.Min() : default;

        public object GetResultObject() => GetResult();
    }

    /// <summary>
    /// 最大值聚合器
    /// </summary>
    public class MaxAggregator<T> : IAggregator<T, T> where T : IComparable<T>
    {
        private readonly List<T> _items = new List<T>();

        public string Name => "Max";

        public void Add(T item) => _items.Add(item);

        public void Remove(T item) => _items.Remove(item);

        public void Reset() => _items.Clear();

        public T GetResult() => _items.Count > 0 ? _items.Max() : default;

        public object GetResultObject() => GetResult();
    }

    /// <summary>
    /// 频率聚合器
    /// </summary>
    public class FrequencyAggregator<T> : IAggregator<T, Dictionary<T, int>>
    {
        private readonly Dictionary<T, int> _frequency = new Dictionary<T, int>();

        public string Name => "Frequency";

        public void Add(T item)
        {
            if (_frequency.ContainsKey(item))
                _frequency[item]++;
            else
                _frequency[item] = 1;
        }

        public void Remove(T item)
        {
            if (_frequency.ContainsKey(item))
            {
                _frequency[item]--;
                if (_frequency[item] == 0)
                    _frequency.Remove(item);
            }
        }

        public void Reset() => _frequency.Clear();

        public Dictionary<T, int> GetResult() => new Dictionary<T, int>(_frequency);

        public object GetResultObject() => GetResult();
    }
}
