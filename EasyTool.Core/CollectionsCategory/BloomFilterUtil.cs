using System;
using System.Collections;
using System.Collections.Generic;

namespace EasyTool.CollectionsCategory
{
    /// <summary>
    /// 布隆过滤器工具类
    /// 一种空间效率很高的概率型数据结构，用于判断元素是否在集合中
    /// 可能存在假阳性（误报），但不存在假阴性
    /// </summary>
    public static class BloomFilterUtil
    {
        /// <summary>
        /// 创建布隆过滤器
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="expectedItemCount">预期元素数量</param>
        /// <param name="falsePositiveProbability">可接受的假阳性概率（0-1）</param>
        /// <returns>布隆过滤器实例</returns>
        public static BloomFilter<T> Create<T>(int expectedItemCount, double falsePositiveProbability = 0.01)
        {
            return new BloomFilter<T>(expectedItemCount, falsePositiveProbability);
        }

        /// <summary>
        /// 计算最佳位数组大小
        /// </summary>
        /// <param name="expectedItemCount">预期元素数量</param>
        /// <param name="falsePositiveProbability">可接受的假阳性概率（0-1）</param>
        /// <returns>最佳位数组大小</returns>
        public static int CalculateOptimalBitSize(int expectedItemCount, double falsePositiveProbability)
        {
            return (int)Math.Ceiling(-expectedItemCount * Math.Log(falsePositiveProbability) / Math.Pow(Math.Log(2), 2));
        }

        /// <summary>
        /// 计算最佳哈希函数数量
        /// </summary>
        /// <param name="bitSize">位数组大小</param>
        /// <param name="expectedItemCount">预期元素数量</param>
        /// <returns>最佳哈希函数数量</returns>
        public static int CalculateOptimalHashCount(int bitSize, int expectedItemCount)
        {
            return (int)Math.Ceiling(bitSize / (double)expectedItemCount * Math.Log(2));
        }
    }

    /// <summary>
    /// 布隆过滤器实现
    /// </summary>
    /// <typeparam name="T">元素类型</typeparam>
    public class BloomFilter<T>
    {
        private readonly BitArray _bits;
        private readonly int _hashCount;
        private readonly Func<T, int>[] _hashFunctions;
        private int _itemCount;
        private readonly object _lock = new();

        /// <summary>
        /// 位数组大小
        /// </summary>
        public int BitSize => _bits.Length;

        /// <summary>
        /// 哈希函数数量
        /// </summary>
        public int HashCount => _hashCount;

        /// <summary>
        /// 已添加元素数量
        /// </summary>
        public int ItemCount
        {
            get
            {
                lock (_lock) { return _itemCount; }
            }
        }

        /// <summary>
        /// 当前估计的假阳性概率
        /// </summary>
        public double CurrentFalsePositiveProbability
        {
            get
            {
                lock (_lock)
                {
                    if (_itemCount == 0) return 0;
                    double ratio = (double)_itemCount * _hashCount / BitSize;
                    return Math.Pow(1 - Math.Exp(-ratio), _hashCount);
                }
            }
        }

        /// <summary>
        /// 创建布隆过滤器
        /// </summary>
        /// <param name="expectedItemCount">预期元素数量</param>
        /// <param name="falsePositiveProbability">可接受的假阳性概率</param>
        public BloomFilter(int expectedItemCount, double falsePositiveProbability = 0.01)
        {
            if (expectedItemCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(expectedItemCount));
            if (falsePositiveProbability <= 0 || falsePositiveProbability >= 1)
                throw new ArgumentOutOfRangeException(nameof(falsePositiveProbability));

            int bitSize = BloomFilterUtil.CalculateOptimalBitSize(expectedItemCount, falsePositiveProbability);
            _hashCount = BloomFilterUtil.CalculateOptimalHashCount(bitSize, expectedItemCount);

            _bits = new BitArray(bitSize);
            _hashFunctions = CreateHashFunctions(_hashCount);
            _itemCount = 0;
        }

        /// <summary>
        /// 添加元素
        /// </summary>
        /// <param name="item">要添加的元素</param>
        /// <exception cref="ArgumentNullException">当 item 为 null 时抛出</exception>
        public void Add(T item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            lock (_lock)
            {
                foreach (var hashFunc in _hashFunctions)
                {
                    int index = Math.Abs(hashFunc(item)) % BitSize;
                    _bits[index] = true;
                }
                _itemCount++;
            }
        }

        /// <summary>
        /// 批量添加元素
        /// </summary>
        /// <param name="items">要添加的元素集合</param>
        /// <exception cref="ArgumentNullException">当 items 为 null 时抛出</exception>
        public void AddRange(IEnumerable<T> items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            foreach (var item in items)
            {
                Add(item);
            }
        }

        /// <summary>
        /// 检查元素可能存在
        /// </summary>
        /// <param name="item">要检查的元素</param>
        /// <returns>true 表示可能存在（可能有假阳性），false 表示一定不存在</returns>
        public bool MightContain(T item)
        {
            if (item == null)
                return false;

            lock (_lock)
            {
                foreach (var hashFunc in _hashFunctions)
                {
                    int index = Math.Abs(hashFunc(item)) % BitSize;
                    if (!_bits[index])
                        return false;
                }
                return true;
            }
        }

        /// <summary>
        /// 清空过滤器
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                _bits.SetAll(false);
                _itemCount = 0;
            }
        }

        /// <summary>
        /// 获取位数组数据
        /// </summary>
        /// <returns>位数组的字节数组表示</returns>
        public byte[] GetBytes()
        {
            lock (_lock)
            {
                byte[] bytes = new byte[(_bits.Length + 7) / 8];
                _bits.CopyTo(bytes, 0);
                return bytes;
            }
        }

        /// <summary>
        /// 从字节数组恢复位数组
        /// </summary>
        /// <param name="bytes">字节数组</param>
        /// <exception cref="ArgumentNullException">当 bytes 为 null 时抛出</exception>
        /// <exception cref="ArgumentException">当字节数组长度不匹配时抛出</exception>
        public void SetBytes(byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            lock (_lock)
            {
                int expectedByteLength = (_bits.Length + 7) / 8;
                if (bytes.Length != expectedByteLength)
                    throw new ArgumentException($"Byte array length ({bytes.Length}) does not match filter size (expected {expectedByteLength} bytes for {_bits.Length} bits)", nameof(bytes));

                var newBits = new BitArray(bytes);
                for (int i = 0; i < _bits.Length; i++)
                {
                    _bits[i] = newBits[i];
                }
            }
        }

        private static Func<T, int>[] CreateHashFunctions(int count)
        {
            var functions = new Func<T, int>[count];

            // 使用双重哈希技术生成多个哈希函数
            // h(i) = hash1(x) + i * hash2(x)
            for (int i = 0; i < count; i++)
            {
                int seed = i * 31 + 17;
                functions[i] = item =>
                {
                    int hash1 = item?.GetHashCode() ?? 0;
                    int hash2 = ((hash1 >> 16) ^ hash1) * seed;
                    return hash1 + hash2 * seed;
                };
            }

            return functions;
        }
    }
}
