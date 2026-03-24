using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyTool.CollectionsCategory
{
    /// <summary>
    /// T-Digest 工具类
    /// 用于流式数据的分位数估计
    /// 特别适合大数据集和分布式环境
    /// </summary>
    public static class TDigestUtil
    {
        /// <summary>
        /// 创建 T-Digest
        /// </summary>
        /// <param name="compression">压缩参数，越大越精确但占用更多内存</param>
        public static TDigest Create(double compression = 100)
        {
            return new TDigest(compression);
        }
    }

    /// <summary>
    /// T-Digest 实现
    /// </summary>
    public class TDigest
    {
        private readonly double _compression;
        private readonly List<Centroid> _centroids;
        private long _count;
        private double _min;
        private double _max;

        private class Centroid
        {
            public double Mean { get; set; }
            public double Weight { get; set; }

            public Centroid(double mean, double weight)
            {
                Mean = mean;
                Weight = weight;
            }
        }

        /// <summary>
        /// 压缩参数
        /// </summary>
        public double Compression => _compression;

        /// <summary>
        /// 已添加的数据点数量
        /// </summary>
        public long Count => _count;

        /// <summary>
        /// 最小值
        /// </summary>
        public double Min => _min;

        /// <summary>
        /// 最大值
        /// </summary>
        public double Max => _max;

        /// <summary>
        /// 质心数量
        /// </summary>
        public int CentroidCount => _centroids.Count;

        /// <summary>
        /// 创建 T-Digest
        /// </summary>
        public TDigest(double compression = 100)
        {
            if (compression <= 0)
                throw new ArgumentOutOfRangeException(nameof(compression));

            _compression = compression;
            _centroids = new List<Centroid>();
            _count = 0;
            _min = double.MaxValue;
            _max = double.MinValue;
        }

        /// <summary>
        /// 添加数据点
        /// </summary>
        public void Add(double value)
        {
            Add(value, 1);
        }

        /// <summary>
        /// 添加带权重的数据点
        /// </summary>
        public void Add(double value, double weight)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
                throw new ArgumentException("Value cannot be NaN or infinity");

            _count++;
            if (value < _min) _min = value;
            if (value > _max) _max = value;

            // 找到最近的质心
            int nearestIndex = -1;
            double nearestDistance = double.MaxValue;

            for (int i = 0; i < _centroids.Count; i++)
            {
                double dist = Math.Abs(_centroids[i].Mean - value);
                if (dist < nearestDistance)
                {
                    nearestDistance = dist;
                    nearestIndex = i;
                }
            }

            // 如果没有质心或距离太远，创建新质心
            if (nearestIndex < 0 || _centroids.Count == 0)
            {
                _centroids.Add(new Centroid(value, weight));
            }
            else
            {
                // 检查是否可以合并
                double q = GetQuantile(_centroids[nearestIndex].Mean);
                double k = 4 * _count * q * (1 - q) / _compression;
                double maxWeight = Math.Max(1, k);

                if (_centroids[nearestIndex].Weight + weight <= maxWeight)
                {
                    // 合并到最近的质心
                    var centroid = _centroids[nearestIndex];
                    double newWeight = centroid.Weight + weight;
                    centroid.Mean = (centroid.Mean * centroid.Weight + value * weight) / newWeight;
                    centroid.Weight = newWeight;
                }
                else
                {
                    // 创建新质心
                    _centroids.Add(new Centroid(value, weight));
                }
            }

            // 定期压缩
            if (_centroids.Count > 3 * _compression)
            {
                Compress();
            }
        }

        /// <summary>
        /// 批量添加数据
        /// </summary>
        public void AddRange(IEnumerable<double> values)
        {
            foreach (var value in values)
            {
                Add(value);
            }
        }

        /// <summary>
        /// 压缩质心
        /// </summary>
        public void Compress()
        {
            if (_centroids.Count <= 1) return;

            // 按均值排序
            _centroids.Sort((a, b) => a.Mean.CompareTo(b.Mean));

            var newCentroids = new List<Centroid>();
            double cumulativeWeight = 0;

            foreach (var centroid in _centroids)
            {
                if (newCentroids.Count == 0)
                {
                    newCentroids.Add(new Centroid(centroid.Mean, centroid.Weight));
                    cumulativeWeight = centroid.Weight;
                    continue;
                }

                double q = cumulativeWeight / _count;
                double k = 4 * _count * q * (1 - q) / _compression;
                double maxWeight = Math.Max(1, k);

                var last = newCentroids[newCentroids.Count - 1];
                if (last.Weight + centroid.Weight <= maxWeight)
                {
                    // 合并
                    double newWeight = last.Weight + centroid.Weight;
                    last.Mean = (last.Mean * last.Weight + centroid.Mean * centroid.Weight) / newWeight;
                    last.Weight = newWeight;
                }
                else
                {
                    newCentroids.Add(new Centroid(centroid.Mean, centroid.Weight));
                }

                cumulativeWeight += centroid.Weight;
            }

            _centroids.Clear();
            _centroids.AddRange(newCentroids);
        }

        /// <summary>
        /// 估计分位数
        /// </summary>
        /// <param name="q">分位数（0-1）</param>
        public double Quantile(double q)
        {
            if (_count == 0)
                throw new InvalidOperationException("No data has been added");
            if (q < 0 || q > 1)
                throw new ArgumentOutOfRangeException(nameof(q), "Quantile must be between 0 and 1");

            if (_centroids.Count == 0) return 0;
            if (_centroids.Count == 1) return _centroids[0].Mean;

            // 确保已压缩
            Compress();

            double targetWeight = q * _count;

            if (q <= 0) return _min;
            if (q >= 1) return _max;

            // 按均值排序
            _centroids.Sort((a, b) => a.Mean.CompareTo(b.Mean));

            double cumulativeWeight = 0;

            for (int i = 0; i < _centroids.Count; i++)
            {
                double nextWeight = cumulativeWeight + _centroids[i].Weight;

                if (nextWeight > targetWeight)
                {
                    // 在当前质心范围内
                    double prevWeight = cumulativeWeight;
                    double deltaWeight = targetWeight - prevWeight;
                    double fraction = deltaWeight / _centroids[i].Weight;

                    if (i == 0)
                    {
                        return _min + fraction * (_centroids[i].Mean - _min);
                    }
                    else
                    {
                        double prevMean = _centroids[i - 1].Mean;
                        return prevMean + fraction * (_centroids[i].Mean - prevMean);
                    }
                }

                cumulativeWeight = nextWeight;
            }

            return _max;
        }

        /// <summary>
        /// 获取值对应的分位数位置
        /// </summary>
        public double GetQuantile(double value)
        {
            if (_count == 0)
                return 0;

            Compress();
            _centroids.Sort((a, b) => a.Mean.CompareTo(b.Mean));

            if (value <= _min) return 0;
            if (value >= _max) return 1;

            double cumulativeWeight = 0;

            for (int i = 0; i < _centroids.Count; i++)
            {
                if (_centroids[i].Mean >= value)
                {
                    if (i == 0)
                    {
                        double fraction = (value - _min) / (_centroids[i].Mean - _min);
                        return (cumulativeWeight + fraction * _centroids[i].Weight / 2) / _count;
                    }
                    else
                    {
                        double prevMean = _centroids[i - 1].Mean;
                        double fraction = (value - prevMean) / (_centroids[i].Mean - prevMean);
                        double prevWeight = cumulativeWeight - _centroids[i - 1].Weight / 2;
                        return (prevWeight + fraction * _centroids[i].Weight) / _count;
                    }
                }

                cumulativeWeight += _centroids[i].Weight;
            }

            return 1;
        }

        /// <summary>
        /// 估计中位数
        /// </summary>
        public double Median() => Quantile(0.5);

        /// <summary>
        /// 估计第25百分位数
        /// </summary>
        public double Q1() => Quantile(0.25);

        /// <summary>
        /// 估计第75百分位数
        /// </summary>
        public double Q3() => Quantile(0.75);

        /// <summary>
        /// 估计四分位距
        /// </summary>
        public double IQR() => Q3() - Q1();

        /// <summary>
        /// 合并另一个 T-Digest
        /// </summary>
        public void Merge(TDigest other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            foreach (var centroid in other._centroids)
            {
                Add(centroid.Mean, centroid.Weight);
            }
        }

        /// <summary>
        /// 清空
        /// </summary>
        public void Clear()
        {
            _centroids.Clear();
            _count = 0;
            _min = double.MaxValue;
            _max = double.MinValue;
        }
    }
}
