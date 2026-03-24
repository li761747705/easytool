using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyTool.CollectionsCategory
{
    /// <summary>
    /// 基数估计和一致性哈希工具类
    /// </summary>
    public static class CardinalityAndHashUtil
    {
        /// <summary>
        /// 创建 HyperLogLog
        /// </summary>
        public static HyperLogLog CreateHyperLogLog(int precision = 14)
        {
            return new HyperLogLog(precision);
        }

        /// <summary>
        /// 创建一致性哈希
        /// </summary>
        public static ConsistentHash<TNode> CreateConsistentHash<TNode>(int virtualNodes = 150)
        {
            return new ConsistentHash<TNode>(virtualNodes);
        }

        /// <summary>
        /// 创建线性计数器
        /// </summary>
        public static LinearCounter CreateLinearCounter(int size)
        {
            return new LinearCounter(size);
        }
    }

    /// <summary>
    /// HyperLogLog 基数估计器
    /// 使用极小内存估计超大集合的不同元素数量
    /// </summary>
    public class HyperLogLog
    {
        private readonly byte[] _registers;
        private readonly int _precision;
        private readonly int _m;
        private readonly double _alpha;

        /// <summary>
        /// 精度参数
        /// </summary>
        public int Precision => _precision;

        /// <summary>
        /// 寄存器数量
        /// </summary>
        public int RegisterCount => _m;

        /// <summary>
        /// 内存使用（字节）
        /// </summary>
        public int MemoryBytes => _registers.Length;

        /// <summary>
        /// 创建 HyperLogLog
        /// </summary>
        /// <param name="precision">精度参数（4-16），越大越精确但占用更多内存</param>
        public HyperLogLog(int precision = 14)
        {
            if (precision < 4 || precision > 16)
                throw new ArgumentOutOfRangeException(nameof(precision), "Precision must be between 4 and 16");

            _precision = precision;
            _m = 1 << precision;
            _registers = new byte[_m];

            // 计算 alpha 常数
            switch (_m)
            {
                case 16:
                    _alpha = 0.673;
                    break;
                case 32:
                    _alpha = 0.697;
                    break;
                case 64:
                    _alpha = 0.709;
                    break;
                default:
                    _alpha = 0.7213 / (1 + 1.079 / _m);
                    break;
            }
        }

        /// <summary>
        /// 添加元素
        /// </summary>
        public void Add(byte[] data)
        {
            ulong hash = MurmurHash3(data);
            int index = (int)(hash >> (64 - _precision));
            int leadingZeros = CountLeadingZeros(hash << _precision) + 1;

            if (leadingZeros > _registers[index])
            {
                _registers[index] = (byte)leadingZeros;
            }
        }

        /// <summary>
        /// 添加字符串
        /// </summary>
        public void Add(string value)
        {
            Add(System.Text.Encoding.UTF8.GetBytes(value));
        }

        /// <summary>
        /// 添加整数
        /// </summary>
        public void Add(int value)
        {
            Add(BitConverter.GetBytes(value));
        }

        /// <summary>
        /// 添加长整数
        /// </summary>
        public void Add(long value)
        {
            Add(BitConverter.GetBytes(value));
        }

        /// <summary>
        /// 估计基数
        /// </summary>
        public long Estimate()
        {
            double sum = 0;
            int zeros = 0;

            foreach (var reg in _registers)
            {
                sum += Math.Pow(2, -reg);
                if (reg == 0)
                    zeros++;
            }

            double estimate = _alpha * _m * _m / sum;

            // 小范围修正
            if (estimate <= 2.5 * _m)
            {
                if (zeros > 0)
                {
                    estimate = _m * Math.Log((double)_m / zeros);
                }
            }
            // 大范围修正
            else if (estimate > (1L << 32) / 30.0)
            {
                estimate = -(1L << 32) * Math.Log(1 - estimate / (1L << 32));
            }

            return (long)estimate;
        }

        /// <summary>
        /// 合并另一个 HyperLogLog
        /// </summary>
        public void Merge(HyperLogLog other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));
            if (other._precision != _precision)
                throw new ArgumentException("Cannot merge HyperLogLog with different precision");

            for (int i = 0; i < _m; i++)
            {
                if (other._registers[i] > _registers[i])
                {
                    _registers[i] = other._registers[i];
                }
            }
        }

        /// <summary>
        /// 清空
        /// </summary>
        public void Clear()
        {
            Array.Clear(_registers, 0, _registers.Length);
        }

        private static ulong MurmurHash3(byte[] data)
        {
            const ulong c1 = 0x87c37b91114253d5;
            const ulong c2 = 0x4cf5ad432745937f;

            int length = data.Length;
            int blocks = length / 8;

            ulong h1 = 0;
            int i = 0;

            for (int j = 0; j < blocks; j++)
            {
                ulong k1 = BitConverter.ToUInt64(data, i);
                i += 8;

                k1 *= c1;
                k1 = RotateLeft(k1, 31);
                k1 *= c2;
                h1 ^= k1;

                h1 = RotateLeft(h1, 27);
                h1 = h1 * 5 + 0x52dce729;
            }

            ulong remaining = 0;
            int remainingLength = length - blocks * 8;
            if (remainingLength > 0)
            {
                for (int j = 0; j < remainingLength; j++)
                {
                    remaining |= (ulong)data[i + j] << (j * 8);
                }

                remaining *= c1;
                remaining = RotateLeft(remaining, 31);
                remaining *= c2;
                h1 ^= remaining;
            }

            h1 ^= (ulong)length;
            h1 ^= h1 >> 33;
            h1 *= 0xff51afd7ed558ccd;
            h1 ^= h1 >> 33;
            h1 *= 0xc4ceb9fe1a85ec53;
            h1 ^= h1 >> 33;

            return h1;
        }

        private static ulong RotateLeft(ulong x, int k)
        {
            return (x << k) | (x >> (64 - k));
        }

        private static int CountLeadingZeros(ulong x)
        {
            if (x == 0)
                return 64;

            int n = 0;
            if ((x & 0xFFFFFFFF00000000) == 0) { n += 32; x <<= 32; }
            if ((x & 0xFFFF000000000000) == 0) { n += 16; x <<= 16; }
            if ((x & 0xFF00000000000000) == 0) { n += 8; x <<= 8; }
            if ((x & 0xF000000000000000) == 0) { n += 4; x <<= 4; }
            if ((x & 0xC000000000000000) == 0) { n += 2; x <<= 2; }
            if ((x & 0x8000000000000000) == 0) { n += 1; }

            return n;
        }
    }

    /// <summary>
    /// 一致性哈希
    /// 用于分布式系统中的负载均衡
    /// </summary>
    public class ConsistentHash<TNode>
    {
        private readonly SortedDictionary<ulong, TNode> _ring;
        private readonly int _virtualNodes;
        private readonly HashSet<TNode> _nodes;

        /// <summary>
        /// 节点数量
        /// </summary>
        public int NodeCount => _nodes.Count;

        /// <summary>
        /// 虚拟节点数量
        /// </summary>
        public int VirtualNodeCount => _virtualNodes;

        /// <summary>
        /// 环上总位置数
        /// </summary>
        public int RingSize => _ring.Count;

        /// <summary>
        /// 创建一致性哈希
        /// </summary>
        /// <param name="virtualNodes">每个物理节点的虚拟节点数</param>
        public ConsistentHash(int virtualNodes = 150)
        {
            if (virtualNodes <= 0)
                throw new ArgumentOutOfRangeException(nameof(virtualNodes));

            _ring = new SortedDictionary<ulong, TNode>();
            _virtualNodes = virtualNodes;
            _nodes = new HashSet<TNode>();
        }

        /// <summary>
        /// 添加节点
        /// </summary>
        public void AddNode(TNode node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            if (_nodes.Contains(node))
                return;

            _nodes.Add(node);

            for (int i = 0; i < _virtualNodes; i++)
            {
                ulong hash = HashNode(node, i);
                _ring[hash] = node;
            }
        }

        /// <summary>
        /// 移除节点
        /// </summary>
        public bool RemoveNode(TNode node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            if (!_nodes.Remove(node))
                return false;

            for (int i = 0; i < _virtualNodes; i++)
            {
                ulong hash = HashNode(node, i);
                _ring.Remove(hash);
            }

            return true;
        }

        /// <summary>
        /// 获取键对应的节点
        /// </summary>
        public TNode GetNode(string key)
        {
            if (_ring.Count == 0)
                throw new InvalidOperationException("No nodes available");

            ulong hash = HashKey(key);

            // 查找第一个大于等于 hash 的节点
            foreach (var kvp in _ring)
            {
                if (kvp.Key >= hash)
                    return kvp.Value;
            }

            // 如果没有找到，返回第一个节点（环形）
            return _ring.First().Value;
        }

        /// <summary>
        /// 获取键对应的多个节点（用于复制）
        /// </summary>
        public List<TNode> GetNodes(string key, int count)
        {
            if (count <= 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (_ring.Count == 0)
                throw new InvalidOperationException("No nodes available");

            var result = new List<TNode>();
            var uniqueNodes = new HashSet<TNode>();
            ulong hash = HashKey(key);

            // 找到起始位置
            var candidates = _ring.Where(kvp => kvp.Key >= hash).ToList();
            if (candidates.Count == 0)
            {
                candidates = _ring.ToList();
            }

            int startIndex = 0;
            for (int i = 0; i < candidates.Count; i++)
            {
                if (candidates[i].Key >= hash)
                {
                    startIndex = i;
                    break;
                }
            }

            // 收集不同的节点
            int index = startIndex;
            while (uniqueNodes.Count < count && uniqueNodes.Count < _nodes.Count)
            {
                var node = candidates[index % candidates.Count].Value;
                if (uniqueNodes.Add(node))
                {
                    result.Add(node);
                }
                index++;
            }

            return result;
        }

        /// <summary>
        /// 获取节点负责的键范围
        /// </summary>
        public List<ulong> GetNodeRanges(TNode node)
        {
            var ranges = new List<ulong>();

            foreach (var kvp in _ring)
            {
                if (EqualityComparer<TNode>.Default.Equals(kvp.Value, node))
                {
                    ranges.Add(kvp.Key);
                }
            }

            return ranges;
        }

        /// <summary>
        /// 清空所有节点
        /// </summary>
        public void Clear()
        {
            _ring.Clear();
            _nodes.Clear();
        }

        /// <summary>
        /// 获取所有节点
        /// </summary>
        public IReadOnlyCollection<TNode> GetNodes()
        {
            return _nodes;
        }

        private ulong HashNode(TNode node, int replicaIndex)
        {
            string key = $"{node}:#{replicaIndex}";
            return HashKey(key);
        }

        private ulong HashKey(string key)
        {
            byte[] data = System.Text.Encoding.UTF8.GetBytes(key);
            return MurmurHash2(data);
        }

        private static ulong MurmurHash2(byte[] data)
        {
            const ulong m = 0xc6a4a7935bd1e995;
            const int r = 47;

            ulong h = 0 ^ (ulong)data.Length * m;

            int length = data.Length;
            int i = 0;

            while (i + 8 <= length)
            {
                ulong k = BitConverter.ToUInt64(data, i);
                i += 8;

                k *= m;
                k ^= k >> r;
                k *= m;

                h ^= k;
                h *= m;
            }

            switch (length - i)
            {
                case 7: h ^= (ulong)data[i + 6] << 48; goto case 6;
                case 6: h ^= (ulong)data[i + 5] << 40; goto case 5;
                case 5: h ^= (ulong)data[i + 4] << 32; goto case 4;
                case 4: h ^= (ulong)data[i + 3] << 24; goto case 3;
                case 3: h ^= (ulong)data[i + 2] << 16; goto case 2;
                case 2: h ^= (ulong)data[i + 1] << 8; goto case 1;
                case 1:
                    h ^= data[i];
                    h *= m;
                    break;
            }

            h ^= h >> r;
            h *= m;
            h ^= h >> r;

            return h;
        }
    }

    /// <summary>
    /// 线性计数器
    /// 简单的基数估计，适合小到中等规模数据
    /// </summary>
    public class LinearCounter
    {
        private readonly BitSet _bits;
        private readonly int _size;

        /// <summary>
        /// 位大小
        /// </summary>
        public int Size => _size;

        /// <summary>
        /// 创建线性计数器
        /// </summary>
        public LinearCounter(int size)
        {
            if (size <= 0)
                throw new ArgumentOutOfRangeException(nameof(size));

            _size = size;
            _bits = new BitSet(size);
        }

        /// <summary>
        /// 添加元素
        /// </summary>
        public void Add(byte[] data)
        {
            int hash = Math.Abs(Hash(data)) % _size;
            _bits.Set(hash);
        }

        /// <summary>
        /// 添加字符串
        /// </summary>
        public void Add(string value)
        {
            Add(System.Text.Encoding.UTF8.GetBytes(value));
        }

        /// <summary>
        /// 估计基数
        /// </summary>
        public long Estimate()
        {
            int setBits = _bits.Cardinality;
            if (setBits == 0)
                return 0;

            // 使用最大似然估计
            double ratio = (double)(_size - setBits) / _size;
            if (ratio <= 0)
                return _size;

            return (long)(-_size * Math.Log(ratio));
        }

        /// <summary>
        /// 合并另一个计数器
        /// </summary>
        public void Merge(LinearCounter other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));
            if (other._size != _size)
                throw new ArgumentException("Cannot merge counters with different sizes");

            _bits.Or(other._bits);
        }

        /// <summary>
        /// 清空
        /// </summary>
        public void Clear()
        {
            _bits.ClearAll();
        }

        private static int Hash(byte[] data)
        {
            unchecked
            {
                int hash = 17;
                foreach (byte b in data)
                {
                    hash = hash * 31 + b;
                }
                return hash;
            }
        }
    }
}
