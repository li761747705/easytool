using System;

namespace EasyTool.CollectionsCategory
{
    /// <summary>
    /// Count-Min Sketch 工具类
    /// 概率数据结构，用于估计元素频率
    /// 空间复杂度远小于精确计数，但有少量误差
    /// </summary>
    public static class CountMinSketchUtil
    {
        /// <summary>
        /// 创建 Count-Min Sketch
        /// </summary>
        /// <param name="width">宽度（哈希桶数量）</param>
        /// <param name="depth">深度（哈希函数数量）</param>
        public static CountMinSketch Create(int width = 1000, int depth = 5)
        {
            return new CountMinSketch(width, depth);
        }

        /// <summary>
        /// 根据期望误差率和置信度创建
        /// </summary>
        /// <param name="errorRate">期望误差率（0-1）</param>
        /// <param name="confidence">置信度（0-1）</param>
        public static CountMinSketch CreateWithAccuracy(double errorRate = 0.01, double confidence = 0.99)
        {
            if (errorRate <= 0 || errorRate >= 1)
                throw new ArgumentOutOfRangeException(nameof(errorRate), "Error rate must be between 0 and 1");
            if (confidence <= 0 || confidence >= 1)
                throw new ArgumentOutOfRangeException(nameof(confidence), "Confidence must be between 0 and 1");

            int width = (int)Math.Ceiling(Math.E / errorRate);
            int depth = (int)Math.Ceiling(-Math.Log(1 - confidence));
            return new CountMinSketch(width, depth);
        }
    }

    /// <summary>
    /// Count-Min Sketch 实现
    /// </summary>
    public class CountMinSketch
    {
        private readonly int _width;
        private readonly int _depth;
        private readonly ulong[,] _counters;
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
        /// 总计数
        /// </summary>
        public ulong TotalCount => _totalCount;

        /// <summary>
        /// 创建 Count-Min Sketch
        /// </summary>
        public CountMinSketch(int width, int depth)
        {
            if (width <= 0)
                throw new ArgumentOutOfRangeException(nameof(width));
            if (depth <= 0)
                throw new ArgumentOutOfRangeException(nameof(depth));

            _width = width;
            _depth = depth;
            _counters = new ulong[depth, width];
            _seeds = new int[depth];
            _totalCount = 0;

            // 初始化不同的哈希种子
            var random = new Random(12345);
            for (int i = 0; i < depth; i++)
            {
                _seeds[i] = random.Next();
            }
        }

        /// <summary>
        /// 添加元素
        /// </summary>
        public void Add(byte[] data)
        {
            for (int i = 0; i < _depth; i++)
            {
                int hash = Hash(data, _seeds[i]);
                int index = Math.Abs(hash) % _width;
                _counters[i, index]++;
            }
            _totalCount++;
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
        /// 估计元素频率
        /// </summary>
        public ulong Estimate(byte[] data)
        {
            ulong min = ulong.MaxValue;
            for (int i = 0; i < _depth; i++)
            {
                int hash = Hash(data, _seeds[i]);
                int index = Math.Abs(hash) % _width;
                if (_counters[i, index] < min)
                    min = _counters[i, index];
            }
            return min;
        }

        /// <summary>
        /// 估计字符串频率
        /// </summary>
        public ulong Estimate(string value)
        {
            return Estimate(System.Text.Encoding.UTF8.GetBytes(value));
        }

        /// <summary>
        /// 估计整数频率
        /// </summary>
        public ulong Estimate(int value)
        {
            return Estimate(BitConverter.GetBytes(value));
        }

        /// <summary>
        /// 合并另一个 Count-Min Sketch
        /// </summary>
        public void Merge(CountMinSketch other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));
            if (other._width != _width || other._depth != _depth)
                throw new ArgumentException("Cannot merge Count-Min Sketch with different dimensions");

            for (int i = 0; i < _depth; i++)
            {
                for (int j = 0; j < _width; j++)
                {
                    _counters[i, j] += other._counters[i, j];
                }
            }
            _totalCount += other._totalCount;
        }

        /// <summary>
        /// 清空
        /// </summary>
        public void Clear()
        {
            Array.Clear(_counters, 0, _counters.Length);
            _totalCount = 0;
        }

        /// <summary>
        /// 获取估计误差上限
        /// </summary>
        public double GetErrorBound()
        {
            if (_totalCount == 0) return 0;
            return (double)_totalCount / _width;
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
    }
}
