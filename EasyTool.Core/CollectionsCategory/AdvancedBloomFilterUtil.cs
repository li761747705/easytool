using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyTool.CollectionsCategory
{
    /// <summary>
    /// 高级布隆过滤器工具类
    /// </summary>
    public static class AdvancedBloomFilterUtil
    {
        /// <summary>
        /// 创建计数布隆过滤器
        /// </summary>
        public static CountingBloomFilter CreateCounting(int capacity, double falsePositiveRate = 0.01)
        {
            return new CountingBloomFilter(capacity, falsePositiveRate);
        }

        /// <summary>
        /// 创建布谷鸟过滤器
        /// </summary>
        public static CuckooFilter CreateCuckoo(int capacity, int fingerprintSize = 8)
        {
            return new CuckooFilter(capacity, fingerprintSize);
        }
    }

    /// <summary>
    /// 计数布隆过滤器
    /// 支持删除操作
    /// </summary>
    public class CountingBloomFilter
    {
        private readonly byte[] _counters;
        private readonly int _hashCount;
        private readonly int _size;
        private readonly HashFunction[] _hashFunctions;
        private int _count;

        private delegate int HashFunction(byte[] data);

        /// <summary>
        /// 已添加元素数量
        /// </summary>
        public int Count => _count;

        /// <summary>
        /// 容量
        /// </summary>
        public int Capacity { get; }

        /// <summary>
        /// 位大小
        /// </summary>
        public int Size => _size;

        /// <summary>
        /// 哈希函数数量
        /// </summary>
        public int HashCount => _hashCount;

        /// <summary>
        /// 创建计数布隆过滤器
        /// </summary>
        public CountingBloomFilter(int capacity, double falsePositiveRate = 0.01)
        {
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(capacity));

            Capacity = capacity;

            // 计算最优参数
            _size = (int)Math.Ceiling(-capacity * Math.Log(falsePositiveRate) / Math.Pow(Math.Log(2), 2));
            _hashCount = (int)Math.Ceiling(_size * Math.Log(2) / capacity);

            _counters = new byte[_size];
            _hashFunctions = new HashFunction[_hashCount];
            _count = 0;

            // 初始化哈希函数
            for (int i = 0; i < _hashCount; i++)
            {
                int seed = (int)(i * 0x9e3779b9);
                _hashFunctions[i] = data => HashWithSeed(data, seed);
            }
        }

        /// <summary>
        /// 添加元素
        /// </summary>
        public void Add(byte[] data)
        {
            foreach (var hash in _hashFunctions)
            {
                int index = Math.Abs(hash(data)) % _size;
                if (_counters[index] < byte.MaxValue)
                {
                    _counters[index]++;
                }
            }
            _count++;
        }

        /// <summary>
        /// 添加字符串
        /// </summary>
        public void Add(string value)
        {
            Add(System.Text.Encoding.UTF8.GetBytes(value));
        }

        /// <summary>
        /// 移除元素
        /// </summary>
        public bool Remove(byte[] data)
        {
            if (!Contains(data))
                return false;

            foreach (var hash in _hashFunctions)
            {
                int index = Math.Abs(hash(data)) % _size;
                if (_counters[index] > 0)
                {
                    _counters[index]--;
                }
            }
            _count--;
            return true;
        }

        /// <summary>
        /// 移除字符串
        /// </summary>
        public bool Remove(string value)
        {
            return Remove(System.Text.Encoding.UTF8.GetBytes(value));
        }

        /// <summary>
        /// 是否可能包含
        /// </summary>
        public bool Contains(byte[] data)
        {
            foreach (var hash in _hashFunctions)
            {
                int index = Math.Abs(hash(data)) % _size;
                if (_counters[index] == 0)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 是否可能包含字符串
        /// </summary>
        public bool Contains(string value)
        {
            return Contains(System.Text.Encoding.UTF8.GetBytes(value));
        }

        /// <summary>
        /// 清空
        /// </summary>
        public void Clear()
        {
            Array.Clear(_counters, 0, _counters.Length);
            _count = 0;
        }

        /// <summary>
        /// 估计假阳率
        /// </summary>
        public double EstimatedFalsePositiveRate()
        {
            if (_count == 0)
                return 0;

            double ratio = (double)_count / Capacity;
            return Math.Pow(1 - Math.Exp(-_hashCount * ratio), _hashCount);
        }

        private static int HashWithSeed(byte[] data, int seed)
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
    }

    /// <summary>
    /// 布谷鸟过滤器
    /// 支持删除，比布隆过滤器更低的空间占用
    /// </summary>
    public class CuckooFilter
    {
        private readonly byte[][][] _buckets;
        private readonly int _bucketCount;
        private readonly int _fingerprintSize;
        private readonly int _maxKickOuts;
        private int _count;
        private readonly Random _random;

        /// <summary>
        /// 已添加元素数量
        /// </summary>
        public int Count => _count;

        /// <summary>
        /// 容量
        /// </summary>
        public int Capacity => _bucketCount;

        /// <summary>
        /// 负载因子
        /// </summary>
        public double LoadFactor => (double)_count / _bucketCount;

        /// <summary>
        /// 创建布谷鸟过滤器
        /// </summary>
        public CuckooFilter(int capacity, int fingerprintSize = 8)
        {
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(capacity));
            if (fingerprintSize < 1 || fingerprintSize > 16)
                throw new ArgumentOutOfRangeException(nameof(fingerprintSize), "Fingerprint size must be between 1 and 16");

            _bucketCount = capacity;
            _fingerprintSize = fingerprintSize;
            _maxKickOuts = 500;
            _count = 0;
            _random = new Random();

            _buckets = new byte[capacity][][];
            for (int i = 0; i < capacity; i++)
            {
                _buckets[i] = new byte[4][]; // 每个桶4个槽
            }
        }

        /// <summary>
        /// 添加元素
        /// </summary>
        public bool Add(byte[] data)
        {
            var fingerprint = ComputeFingerprint(data);
            int i1 = Hash1(data);
            int i2 = AltIndex(i1, fingerprint);

            // 尝试添加到任一桶
            if (TryAddToBucket(i1, fingerprint))
            {
                _count++;
                return true;
            }
            if (TryAddToBucket(i2, fingerprint))
            {
                _count++;
                return true;
            }

            // 需要踢出
            int i = _random.Next(2) == 0 ? i1 : i2;

            for (int n = 0; n < _maxKickOuts; n++)
            {
                // 随机选择一个槽踢出
                int slot = _random.Next(4);
                var oldFingerprint = _buckets[i][slot];
                _buckets[i][slot] = fingerprint;
                fingerprint = oldFingerprint;

                i = AltIndex(i, fingerprint);

                if (TryAddToBucket(i, fingerprint))
                {
                    _count++;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 添加字符串
        /// </summary>
        public bool Add(string value)
        {
            return Add(System.Text.Encoding.UTF8.GetBytes(value));
        }

        /// <summary>
        /// 移除元素
        /// </summary>
        public bool Remove(byte[] data)
        {
            var fingerprint = ComputeFingerprint(data);
            int i1 = Hash1(data);
            int i2 = AltIndex(i1, fingerprint);

            if (RemoveFromBucket(i1, fingerprint) || RemoveFromBucket(i2, fingerprint))
            {
                _count--;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 移除字符串
        /// </summary>
        public bool Remove(string value)
        {
            return Remove(System.Text.Encoding.UTF8.GetBytes(value));
        }

        /// <summary>
        /// 是否可能包含
        /// </summary>
        public bool Contains(byte[] data)
        {
            var fingerprint = ComputeFingerprint(data);
            int i1 = Hash1(data);
            int i2 = AltIndex(i1, fingerprint);

            return BucketContains(i1, fingerprint) || BucketContains(i2, fingerprint);
        }

        /// <summary>
        /// 是否可能包含字符串
        /// </summary>
        public bool Contains(string value)
        {
            return Contains(System.Text.Encoding.UTF8.GetBytes(value));
        }

        /// <summary>
        /// 清空
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < _bucketCount; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    _buckets[i][j] = null;
                }
            }
            _count = 0;
        }

        private byte[] ComputeFingerprint(byte[] data)
        {
            unchecked
            {
                int hash = 17;
                foreach (byte b in data)
                {
                    hash = hash * 31 + b;
                }

                var fingerprint = new byte[_fingerprintSize];
                for (int i = 0; i < _fingerprintSize; i++)
                {
                    fingerprint[i] = (byte)((hash >> (i * 8)) & 0xFF);
                }

                // 确保不为空
                if (fingerprint.All(b => b == 0))
                {
                    fingerprint[0] = 1;
                }

                return fingerprint;
            }
        }

        private int Hash1(byte[] data)
        {
            unchecked
            {
                int hash = 0;
                foreach (byte b in data)
                {
                    hash = hash * 31 + b;
                }
                return Math.Abs(hash) % _bucketCount;
            }
        }

        private int AltIndex(int index, byte[] fingerprint)
        {
            unchecked
            {
                int hash = 0;
                foreach (byte b in fingerprint)
                {
                    hash = hash * 31 + b;
                }
                return (index ^ hash) % _bucketCount;
            }
        }

        private bool TryAddToBucket(int index, byte[] fingerprint)
        {
            for (int i = 0; i < 4; i++)
            {
                if (_buckets[index][i] == null)
                {
                    _buckets[index][i] = fingerprint;
                    return true;
                }
            }
            return false;
        }

        private bool RemoveFromBucket(int index, byte[] fingerprint)
        {
            for (int i = 0; i < 4; i++)
            {
                if (FingerprintEquals(_buckets[index][i], fingerprint))
                {
                    _buckets[index][i] = null;
                    return true;
                }
            }
            return false;
        }

        private bool BucketContains(int index, byte[] fingerprint)
        {
            for (int i = 0; i < 4; i++)
            {
                if (FingerprintEquals(_buckets[index][i], fingerprint))
                {
                    return true;
                }
            }
            return false;
        }

        private bool FingerprintEquals(byte[] a, byte[] b)
        {
            if (a == null || b == null)
                return false;

            if (a.Length != b.Length)
                return false;

            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                    return false;
            }

            return true;
        }
    }

    /// <summary>
    /// 可扩展布隆过滤器
    /// 当填满时自动扩展容量
    /// </summary>
    public class ScalableBloomFilter
    {
        private readonly List<CountingBloomFilter> _filters;
        private readonly double _initialFalsePositiveRate;
        private readonly double _scalingFactor;
        private readonly int _initialCapacity;
        private int _totalCapacity;

        /// <summary>
        /// 已添加元素数量
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// 当前容量
        /// </summary>
        public int Capacity => _totalCapacity;

        /// <summary>
        /// 过滤器数量
        /// </summary>
        public int FilterCount => _filters.Count;

        /// <summary>
        /// 创建可扩展布隆过滤器
        /// </summary>
        public ScalableBloomFilter(int initialCapacity = 1000, double falsePositiveRate = 0.01, double scalingFactor = 2)
        {
            _initialCapacity = initialCapacity;
            _initialFalsePositiveRate = falsePositiveRate;
            _scalingFactor = scalingFactor;

            _filters = new List<CountingBloomFilter>();
            _totalCapacity = 0;
            Count = 0;

            AddFilter();
        }

        /// <summary>
        /// 添加元素
        /// </summary>
        public void Add(byte[] data)
        {
            // 查找有空位的过滤器
            foreach (var filter in _filters)
            {
                if (filter.Count < filter.Capacity)
                {
                    filter.Add(data);
                    Count++;
                    return;
                }
            }

            // 需要添加新过滤器
            AddFilter();
            _filters[_filters.Count - 1].Add(data);
            Count++;
        }

        /// <summary>
        /// 添加字符串
        /// </summary>
        public void Add(string value)
        {
            Add(System.Text.Encoding.UTF8.GetBytes(value));
        }

        /// <summary>
        /// 移除元素
        /// </summary>
        public bool Remove(byte[] data)
        {
            for (int i = _filters.Count - 1; i >= 0; i--)
            {
                if (_filters[i].Contains(data))
                {
                    if (_filters[i].Remove(data))
                    {
                        Count--;
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 移除字符串
        /// </summary>
        public bool Remove(string value)
        {
            return Remove(System.Text.Encoding.UTF8.GetBytes(value));
        }

        /// <summary>
        /// 是否可能包含
        /// </summary>
        public bool Contains(byte[] data)
        {
            return _filters.Any(f => f.Contains(data));
        }

        /// <summary>
        /// 是否可能包含字符串
        /// </summary>
        public bool Contains(string value)
        {
            return Contains(System.Text.Encoding.UTF8.GetBytes(value));
        }

        /// <summary>
        /// 清空
        /// </summary>
        public void Clear()
        {
            _filters.Clear();
            _totalCapacity = 0;
            Count = 0;
            AddFilter();
        }

        private void AddFilter()
        {
            int capacity = (int)(_initialCapacity * Math.Pow(_scalingFactor, _filters.Count));
            double fpr = _initialFalsePositiveRate / Math.Pow(_scalingFactor, _filters.Count);

            var filter = new CountingBloomFilter(capacity, fpr);
            _filters.Add(filter);
            _totalCapacity += capacity;
        }
    }
}
