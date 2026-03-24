using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyTool.CollectionsCategory
{
    /// <summary>
    /// Heavy Keeper 工具类
    /// 用于检测数据流中的 Heavy Hitters（高频元素）
    /// 基于概率衰减的计数器，适合实时数据流分析
    /// </summary>
    public static class HeavyKeeperUtil
    {
        /// <summary>
        /// 创建 Heavy Keeper
        /// </summary>
        /// <param name="width">宽度（哈希桶数量）</param>
        /// <param name="depth">深度（哈希函数数量）</param>
        /// <param name="decay">衰减因子（0-1）</param>
        public static HeavyKeeper Create(int width = 1000, int depth = 5, double decay = 0.9)
        {
            return new HeavyKeeper(width, depth, decay);
        }
    }

    /// <summary>
    /// Heavy Keeper 实现
    /// </summary>
    public class HeavyKeeper
    {
        private readonly int _width;
        private readonly int _depth;
        private readonly double _decay;
        private readonly ulong[,] _counters;
        private readonly ulong[,] _fingerprints;
        private readonly int[] _seeds;
        private ulong _totalCount;

        /// <summary>
        /// 宽度
        /// </summary>
        public int Width => _width;

        /// <summary>
        /// 深度
        /// </summary>
        public int Depth => _depth;

        /// <summary>
        /// 衰减因子
        /// </summary>
        public double Decay => _decay;

        /// <summary>
        /// 总计数
        /// </summary>
        public ulong TotalCount => _totalCount;

        /// <summary>
        /// 创建 Heavy Keeper
        /// </summary>
        public HeavyKeeper(int width, int depth, double decay = 0.9)
        {
            if (width <= 0)
                throw new ArgumentOutOfRangeException(nameof(width));
            if (depth <= 0)
                throw new ArgumentOutOfRangeException(nameof(depth));
            if (decay <= 0 || decay >= 1)
                throw new ArgumentOutOfRangeException(nameof(decay), "Decay must be between 0 and 1");

            _width = width;
            _depth = depth;
            _decay = decay;
            _counters = new ulong[depth, width];
            _fingerprints = new ulong[depth, width];
            _seeds = new int[depth];
            _totalCount = 0;

            var random = new Random(12345);
            for (int i = 0; i < depth; i++)
            {
                _seeds[i] = random.Next();
            }
        }

        /// <summary>
        /// 添加元素
        /// </summary>
        /// <returns>估计的频率</returns>
        public ulong Add(byte[] data)
        {
            ulong fingerprint = ComputeFingerprint(data);
            ulong maxCount = 0;

            for (int i = 0; i < _depth; i++)
            {
                int hash = Hash(data, _seeds[i]);
                int index = Math.Abs(hash) % _width;

                if (_fingerprints[i, index] == fingerprint)
                {
                    // 匹配，增加计数
                    _counters[i, index]++;
                    if (_counters[i, index] > maxCount)
                        maxCount = _counters[i, index];
                }
                else if (_counters[i, index] == 0)
                {
                    // 空桶，直接放入
                    _fingerprints[i, index] = fingerprint;
                    _counters[i, index] = 1;
                    if (maxCount == 0) maxCount = 1;
                }
                else
                {
                    // 不匹配，以一定概率衰减并替换
                    double probability = Math.Pow(_decay, _counters[i, index]);
                    if (RandomDouble() < probability)
                    {
                        _counters[i, index]--;
                        if (_counters[i, index] == 0)
                        {
                            _fingerprints[i, index] = fingerprint;
                            _counters[i, index] = 1;
                            if (maxCount == 0) maxCount = 1;
                        }
                    }
                }
            }

            _totalCount++;
            return maxCount;
        }

        /// <summary>
        /// 添加字符串
        /// </summary>
        public ulong Add(string value)
        {
            return Add(System.Text.Encoding.UTF8.GetBytes(value));
        }

        /// <summary>
        /// 添加整数
        /// </summary>
        public ulong Add(int value)
        {
            return Add(BitConverter.GetBytes(value));
        }

        /// <summary>
        /// 估计元素频率
        /// </summary>
        public ulong Estimate(byte[] data)
        {
            ulong fingerprint = ComputeFingerprint(data);
            ulong maxCount = 0;

            for (int i = 0; i < _depth; i++)
            {
                int hash = Hash(data, _seeds[i]);
                int index = Math.Abs(hash) % _width;

                if (_fingerprints[i, index] == fingerprint)
                {
                    if (_counters[i, index] > maxCount)
                        maxCount = _counters[i, index];
                }
            }

            return maxCount;
        }

        /// <summary>
        /// 估计字符串频率
        /// </summary>
        public ulong Estimate(string value)
        {
            return Estimate(System.Text.Encoding.UTF8.GetBytes(value));
        }

        /// <summary>
        /// 获取 Top-K 高频元素
        /// </summary>
        public List<(T Item, ulong Count)> GetTopK<T>(IEnumerable<T> items, int k)
        {
            var counts = new Dictionary<T, ulong>();

            foreach (var item in items)
            {
                byte[] data;
                if (typeof(T) == typeof(string))
                    data = System.Text.Encoding.UTF8.GetBytes(item.ToString());
                else if (typeof(T) == typeof(int))
                    data = BitConverter.GetBytes(Convert.ToInt32(item));
                else
                    data = System.Text.Encoding.UTF8.GetBytes(item.ToString());

                ulong count = Estimate(data);
                if (count > 0)
                {
                    counts[item] = count;
                }
            }

            return counts.OrderByDescending(x => x.Value)
                        .Take(k)
                        .Select(x => (x.Key, x.Value))
                        .ToList();
        }

        /// <summary>
        /// 清空
        /// </summary>
        public void Clear()
        {
            Array.Clear(_counters, 0, _counters.Length);
            Array.Clear(_fingerprints, 0, _fingerprints.Length);
            _totalCount = 0;
        }

        private static ulong ComputeFingerprint(byte[] data)
        {
            unchecked
            {
                ulong hash = 14695981039346656037;
                foreach (byte b in data)
                {
                    hash ^= b;
                    hash *= 1099511628211;
                }
                return hash;
            }
        }

        private static int Hash(byte[] data, int seed)
        {
            unchecked
            {
                int hash = seed;
                foreach (byte b in data)
                {
                    hash = hash * 31 + b;
                }
                return hash;
            }
        }

        private static double RandomDouble()
        {
#if NETSTANDARD2_1
            return _random.NextDouble();
#else
            return Random.Shared.NextDouble();
#endif
        }

        private static readonly Random _random = new Random();
    }

    /// <summary>
    /// 流式 Top-K 工具
    /// 使用最小堆维护 Top-K 元素
    /// </summary>
    public class StreamTopK<T>
    {
        private readonly int _k;
        private readonly Dictionary<T, ulong> _counts;
        private readonly HeavyKeeperPriorityQueue<T, ulong> _minHeap;

        /// <summary>
        /// K值
        /// </summary>
        public int K => _k;

        /// <summary>
        /// 当前元素数量
        /// </summary>
        public int Count => _counts.Count;

        /// <summary>
        /// 创建流式 Top-K
        /// </summary>
        public StreamTopK(int k)
        {
            if (k <= 0)
                throw new ArgumentOutOfRangeException(nameof(k));

            _k = k;
            _counts = new Dictionary<T, ulong>();
            _minHeap = new HeavyKeeperPriorityQueue<T, ulong>();
        }

        /// <summary>
        /// 添加元素
        /// </summary>
        public void Add(T item)
        {
            if (_counts.ContainsKey(item))
            {
                _counts[item]++;
            }
            else
            {
                _counts[item] = 1;
            }
        }

        /// <summary>
        /// 获取当前 Top-K
        /// </summary>
        public List<(T Item, ulong Count)> GetTopK()
        {
            var heap = new HeavyKeeperPriorityQueue<T, ulong>();
            foreach (var kvp in _counts)
            {
                if (heap.Count < _k)
                {
                    heap.Enqueue(kvp.Key, kvp.Value);
                }
                else if (kvp.Value > heap.Peek().Priority)
                {
                    heap.Dequeue();
                    heap.Enqueue(kvp.Key, kvp.Value);
                }
            }

            var result = new List<(T, ulong)>();
            while (heap.Count > 0)
            {
                var item = heap.Dequeue();
                result.Add((item.Element, item.Priority));
            }

            result.Reverse();
            return result;
        }

        /// <summary>
        /// 清空
        /// </summary>
        public void Clear()
        {
            _counts.Clear();
            _minHeap.Clear();
        }
    }

    // 内部使用的优先队列元素
    internal struct PriorityQueueElement<T>
    {
        public T Element { get; }
        public ulong Value { get; }

        public PriorityQueueElement(T element, ulong value)
        {
            Element = element;
            Value = value;
        }
    }

    // 简单的优先队列实现（内部使用，避免与 PriorityQueueUtil 中的 PriorityQueue 冲突）
    internal class HeavyKeeperPriorityQueue<T, TPriority> where TPriority : IComparable<TPriority>
    {
        private readonly List<(T Element, TPriority Priority)> _heap = new();

        public int Count => _heap.Count;

        public void Enqueue(T element, TPriority priority)
        {
            _heap.Add((element, priority));
            int i = _heap.Count - 1;
            while (i > 0)
            {
                int parent = (i - 1) / 2;
                if (_heap[parent].Priority.CompareTo(priority) <= 0) break;
                _heap[i] = _heap[parent];
                i = parent;
            }
            _heap[i] = (element, priority);
        }

        public (T Element, TPriority Priority) Dequeue()
        {
            if (_heap.Count == 0) throw new InvalidOperationException("Queue is empty");
            var result = _heap[0];
            var last = _heap[_heap.Count - 1];
            _heap.RemoveAt(_heap.Count - 1);

            if (_heap.Count > 0)
            {
                int i = 0;
                while (true)
                {
                    int left = 2 * i + 1;
                    if (left >= _heap.Count) break;
                    int right = left + 1;
                    int smallest = left;
                    if (right < _heap.Count && _heap[right].Priority.CompareTo(_heap[left].Priority) < 0)
                        smallest = right;
                    if (last.Priority.CompareTo(_heap[smallest].Priority) <= 0) break;
                    _heap[i] = _heap[smallest];
                    i = smallest;
                }
                _heap[i] = last;
            }

            return result;
        }

        public (T Element, TPriority Priority) Peek()
        {
            if (_heap.Count == 0) throw new InvalidOperationException("Queue is empty");
            return _heap[0];
        }

        public void Clear() => _heap.Clear();
    }
}
