using System;
using System.Collections.Generic;

namespace EasyTool.CollectionsCategory
{
    /// <summary>
    /// 概率分布工具类
    /// </summary>
    public static class DistributionUtil
    {
        /// <summary>
        /// 创建离散分布
        /// </summary>
        public static DiscreteDistribution<T> CreateDiscrete<T>() where T : notnull
        {
            return new DiscreteDistribution<T>();
        }

        /// <summary>
        /// 从权重创建离散分布
        /// </summary>
        public static DiscreteDistribution<T> CreateDiscrete<T>(IEnumerable<T> items, IEnumerable<double> weights) where T : notnull
        {
            var dist = new DiscreteDistribution<T>();
            var itemList = new List<T>(items);
            var weightList = new List<double>(weights);

            if (itemList.Count != weightList.Count)
                throw new ArgumentException("Items and weights must have the same length");

            for (int i = 0; i < itemList.Count; i++)
            {
                dist.Add(itemList[i], weightList[i]);
            }

            return dist;
        }

        /// <summary>
        /// 创建正态分布
        /// </summary>
        public static NormalDistribution CreateNormal(double mean = 0, double stdDev = 1)
        {
            return new NormalDistribution(mean, stdDev);
        }

        /// <summary>
        /// 创建泊松分布
        /// </summary>
        public static PoissonDistribution CreatePoisson(double lambda)
        {
            return new PoissonDistribution(lambda);
        }

        /// <summary>
        /// 创建指数分布
        /// </summary>
        public static ExponentialDistribution CreateExponential(double rate)
        {
            return new ExponentialDistribution(rate);
        }

        /// <summary>
        /// 创建二项分布
        /// </summary>
        public static BinomialDistribution CreateBinomial(int n, double p)
        {
            return new BinomialDistribution(n, p);
        }

        /// <summary>
        /// 创建几何分布
        /// </summary>
        public static GeometricDistribution CreateGeometric(double p)
        {
            return new GeometricDistribution(p);
        }

        /// <summary>
        /// 创建均匀分布
        /// </summary>
        public static UniformDistribution CreateUniform(double min, double max)
        {
            return new UniformDistribution(min, max);
        }

        /// <summary>
        /// 创建均匀整数分布
        /// </summary>
        public static UniformIntDistribution CreateUniformInt(int min, int max)
        {
            return new UniformIntDistribution(min, max);
        }
    }

    /// <summary>
    /// 离散概率分布
    /// </summary>
    public class DiscreteDistribution<T> where T : notnull
    {
        private readonly List<T> _items;
        private readonly List<double> _cumulativeWeights;
        private double _totalWeight;
        private readonly Random _random;

        /// <summary>
        /// 项目数量
        /// </summary>
        public int Count => _items.Count;

        /// <summary>
        /// 总权重
        /// </summary>
        public double TotalWeight => _totalWeight;

        /// <summary>
        /// 创建离散分布
        /// </summary>
        public DiscreteDistribution()
        {
            _items = new List<T>();
            _cumulativeWeights = new List<double>();
            _totalWeight = 0;
            _random = new Random();
        }

        /// <summary>
        /// 添加项目
        /// </summary>
        public void Add(T item, double weight)
        {
            if (weight < 0)
                throw new ArgumentOutOfRangeException(nameof(weight), "Weight must be non-negative");

            _items.Add(item);
            _totalWeight += weight;
            _cumulativeWeights.Add(_totalWeight);
        }

        /// <summary>
        /// 采样一个项目
        /// </summary>
        public T Sample()
        {
            if (_items.Count == 0)
                throw new InvalidOperationException("Distribution is empty");

            double r = _random.NextDouble() * _totalWeight;

            int index = _cumulativeWeights.BinarySearch(r);
            if (index < 0)
                index = ~index;

            return _items[index];
        }

        /// <summary>
        /// 采样多个项目
        /// </summary>
        public List<T> Sample(int count)
        {
            var result = new List<T>(count);
            for (int i = 0; i < count; i++)
            {
                result.Add(Sample());
            }
            return result;
        }

        /// <summary>
        /// 获取项目的概率
        /// </summary>
        public double GetProbability(T item)
        {
            int index = _items.IndexOf(item);
            if (index < 0)
                return 0;

            double weight = index == 0
                ? _cumulativeWeights[0]
                : _cumulativeWeights[index] - _cumulativeWeights[index - 1];

            return weight / _totalWeight;
        }

        /// <summary>
        /// 清空分布
        /// </summary>
        public void Clear()
        {
            _items.Clear();
            _cumulativeWeights.Clear();
            _totalWeight = 0;
        }
    }

    /// <summary>
    /// 正态分布（高斯分布）
    /// </summary>
    public class NormalDistribution
    {
        private readonly double _mean;
        private readonly double _stdDev;
        private readonly Random _random;
        private double? _spare;

        /// <summary>
        /// 均值
        /// </summary>
        public double Mean => _mean;

        /// <summary>
        /// 标准差
        /// </summary>
        public double StdDev => _stdDev;

        /// <summary>
        /// 方差
        /// </summary>
        public double Variance => _stdDev * _stdDev;

        /// <summary>
        /// 创建正态分布
        /// </summary>
        public NormalDistribution(double mean = 0, double stdDev = 1)
        {
            if (stdDev <= 0)
                throw new ArgumentOutOfRangeException(nameof(stdDev), "Standard deviation must be positive");

            _mean = mean;
            _stdDev = stdDev;
            _random = new Random();
            _spare = null;
        }

        /// <summary>
        /// 采样一个值（Box-Muller 变换）
        /// </summary>
        public double Sample()
        {
            if (_spare.HasValue)
            {
                double result = _spare.Value;
                _spare = null;
                return result;
            }

            double u1, u2, s;
            do
            {
                u1 = 2.0 * _random.NextDouble() - 1.0;
                u2 = 2.0 * _random.NextDouble() - 1.0;
                s = u1 * u1 + u2 * u2;
            } while (s >= 1.0 || s == 0);

            double mul = Math.Sqrt(-2.0 * Math.Log(s) / s);
            _spare = _mean + _stdDev * u2 * mul;
            return _mean + _stdDev * u1 * mul;
        }

        /// <summary>
        /// 采样多个值
        /// </summary>
        public List<double> Sample(int count)
        {
            var result = new List<double>(count);
            for (int i = 0; i < count; i++)
            {
                result.Add(Sample());
            }
            return result;
        }

        /// <summary>
        /// 概率密度函数
        /// </summary>
        public double PDF(double x)
        {
            double exp = -0.5 * Math.Pow((x - _mean) / _stdDev, 2);
            return Math.Exp(exp) / (_stdDev * Math.Sqrt(2 * Math.PI));
        }

        /// <summary>
        /// 累积分布函数
        /// </summary>
        public double CDF(double x)
        {
            return 0.5 * (1 + Erf((x - _mean) / (_stdDev * Math.Sqrt(2))));
        }

        private static double Erf(double x)
        {
            // Abramowitz and Stegun approximation
            double a1 = 0.254829592;
            double a2 = -0.284496736;
            double a3 = 1.421413741;
            double a4 = -1.453152027;
            double a5 = 1.061405429;
            double p = 0.3275911;

            int sign = x >= 0 ? 1 : -1;
            x = Math.Abs(x);

            double t = 1.0 / (1.0 + p * x);
            double y = 1.0 - (((((a5 * t + a4) * t) + a3) * t + a2) * t + a1) * t * Math.Exp(-x * x);

            return sign * y;
        }
    }

    /// <summary>
    /// 泊松分布
    /// </summary>
    public class PoissonDistribution
    {
        private readonly double _lambda;
        private readonly Random _random;

        /// <summary>
        /// Lambda 参数（期望值）
        /// </summary>
        public double Lambda => _lambda;

        /// <summary>
        /// 均值
        /// </summary>
        public double Mean => _lambda;

        /// <summary>
        /// 方差
        /// </summary>
        public double Variance => _lambda;

        /// <summary>
        /// 创建泊松分布
        /// </summary>
        public PoissonDistribution(double lambda)
        {
            if (lambda <= 0)
                throw new ArgumentOutOfRangeException(nameof(lambda), "Lambda must be positive");

            _lambda = lambda;
            _random = new Random();
        }

        /// <summary>
        /// 采样一个值（Knuth 算法）
        /// </summary>
        public int Sample()
        {
            double L = Math.Exp(-_lambda);
            int k = 0;
            double p = 1.0;

            do
            {
                k++;
                p *= _random.NextDouble();
            } while (p > L);

            return k - 1;
        }

        /// <summary>
        /// 采样多个值
        /// </summary>
        public List<int> Sample(int count)
        {
            var result = new List<int>(count);
            for (int i = 0; i < count; i++)
            {
                result.Add(Sample());
            }
            return result;
        }

        /// <summary>
        /// 概率质量函数
        /// </summary>
        public double PMF(int k)
        {
            if (k < 0)
                return 0;
            return Math.Pow(_lambda, k) * Math.Exp(-_lambda) / Factorial(k);
        }

        private static long Factorial(int n)
        {
            if (n <= 1)
                return 1;
            long result = 1;
            for (int i = 2; i <= n; i++)
                result *= i;
            return result;
        }
    }

    /// <summary>
    /// 指数分布
    /// </summary>
    public class ExponentialDistribution
    {
        private readonly double _rate;
        private readonly Random _random;

        /// <summary>
        /// 速率参数（λ）
        /// </summary>
        public double Rate => _rate;

        /// <summary>
        /// 均值
        /// </summary>
        public double Mean => 1.0 / _rate;

        /// <summary>
        /// 方差
        /// </summary>
        public double Variance => 1.0 / (_rate * _rate);

        /// <summary>
        /// 创建指数分布
        /// </summary>
        public ExponentialDistribution(double rate)
        {
            if (rate <= 0)
                throw new ArgumentOutOfRangeException(nameof(rate), "Rate must be positive");

            _rate = rate;
            _random = new Random();
        }

        /// <summary>
        /// 采样一个值
        /// </summary>
        public double Sample()
        {
            return -Math.Log(1 - _random.NextDouble()) / _rate;
        }

        /// <summary>
        /// 采样多个值
        /// </summary>
        public List<double> Sample(int count)
        {
            var result = new List<double>(count);
            for (int i = 0; i < count; i++)
            {
                result.Add(Sample());
            }
            return result;
        }

        /// <summary>
        /// 概率密度函数
        /// </summary>
        public double PDF(double x)
        {
            if (x < 0)
                return 0;
            return _rate * Math.Exp(-_rate * x);
        }

        /// <summary>
        /// 累积分布函数
        /// </summary>
        public double CDF(double x)
        {
            if (x < 0)
                return 0;
            return 1 - Math.Exp(-_rate * x);
        }
    }

    /// <summary>
    /// 二项分布
    /// </summary>
    public class BinomialDistribution
    {
        private readonly int _n;
        private readonly double _p;
        private readonly Random _random;

        /// <summary>
        /// 试验次数
        /// </summary>
        public int N => _n;

        /// <summary>
        /// 成功概率
        /// </summary>
        public double P => _p;

        /// <summary>
        /// 均值
        /// </summary>
        public double Mean => _n * _p;

        /// <summary>
        /// 方差
        /// </summary>
        public double Variance => _n * _p * (1 - _p);

        /// <summary>
        /// 创建二项分布
        /// </summary>
        public BinomialDistribution(int n, double p)
        {
            if (n <= 0)
                throw new ArgumentOutOfRangeException(nameof(n), "N must be positive");
            if (p < 0 || p > 1)
                throw new ArgumentOutOfRangeException(nameof(p), "P must be between 0 and 1");

            _n = n;
            _p = p;
            _random = new Random();
        }

        /// <summary>
        /// 采样一个值
        /// </summary>
        public int Sample()
        {
            int successes = 0;
            for (int i = 0; i < _n; i++)
            {
                if (_random.NextDouble() < _p)
                    successes++;
            }
            return successes;
        }

        /// <summary>
        /// 采样多个值
        /// </summary>
        public List<int> Sample(int count)
        {
            var result = new List<int>(count);
            for (int i = 0; i < count; i++)
            {
                result.Add(Sample());
            }
            return result;
        }

        /// <summary>
        /// 概率质量函数
        /// </summary>
        public double PMF(int k)
        {
            if (k < 0 || k > _n)
                return 0;
            return BinomialCoefficient(_n, k) * Math.Pow(_p, k) * Math.Pow(1 - _p, _n - k);
        }

        private static long BinomialCoefficient(int n, int k)
        {
            if (k > n - k)
                k = n - k;

            long result = 1;
            for (int i = 0; i < k; i++)
            {
                result = result * (n - i) / (i + 1);
            }
            return result;
        }
    }

    /// <summary>
    /// 几何分布
    /// </summary>
    public class GeometricDistribution
    {
        private readonly double _p;
        private readonly Random _random;

        /// <summary>
        /// 成功概率
        /// </summary>
        public double P => _p;

        /// <summary>
        /// 均值
        /// </summary>
        public double Mean => 1.0 / _p;

        /// <summary>
        /// 方差
        /// </summary>
        public double Variance => (1 - _p) / (_p * _p);

        /// <summary>
        /// 创建几何分布
        /// </summary>
        public GeometricDistribution(double p)
        {
            if (p <= 0 || p > 1)
                throw new ArgumentOutOfRangeException(nameof(p), "P must be between 0 and 1");

            _p = p;
            _random = new Random();
        }

        /// <summary>
        /// 采样一个值（返回第一次成功前的失败次数）
        /// </summary>
        public int Sample()
        {
            double u = _random.NextDouble();
            return (int)Math.Floor(Math.Log(1 - u) / Math.Log(1 - _p));
        }

        /// <summary>
        /// 采样多个值
        /// </summary>
        public List<int> Sample(int count)
        {
            var result = new List<int>(count);
            for (int i = 0; i < count; i++)
            {
                result.Add(Sample());
            }
            return result;
        }

        /// <summary>
        /// 概率质量函数
        /// </summary>
        public double PMF(int k)
        {
            if (k < 0)
                return 0;
            return Math.Pow(1 - _p, k) * _p;
        }

        /// <summary>
        /// 累积分布函数
        /// </summary>
        public double CDF(int k)
        {
            if (k < 0)
                return 0;
            return 1 - Math.Pow(1 - _p, k + 1);
        }
    }

    /// <summary>
    /// 均匀分布（连续）
    /// </summary>
    public class UniformDistribution
    {
        private readonly double _min;
        private readonly double _max;
        private readonly Random _random;

        /// <summary>
        /// 最小值
        /// </summary>
        public double Min => _min;

        /// <summary>
        /// 最大值
        /// </summary>
        public double Max => _max;

        /// <summary>
        /// 均值
        /// </summary>
        public double Mean => (_min + _max) / 2;

        /// <summary>
        /// 方差
        /// </summary>
        public double Variance => (_max - _min) * (_max - _min) / 12;

        /// <summary>
        /// 创建均匀分布
        /// </summary>
        public UniformDistribution(double min, double max)
        {
            if (max <= min)
                throw new ArgumentException("Max must be greater than min");

            _min = min;
            _max = max;
            _random = new Random();
        }

        /// <summary>
        /// 采样一个值
        /// </summary>
        public double Sample()
        {
            return _min + _random.NextDouble() * (_max - _min);
        }

        /// <summary>
        /// 采样多个值
        /// </summary>
        public List<double> Sample(int count)
        {
            var result = new List<double>(count);
            for (int i = 0; i < count; i++)
            {
                result.Add(Sample());
            }
            return result;
        }

        /// <summary>
        /// 概率密度函数
        /// </summary>
        public double PDF(double x)
        {
            if (x < _min || x > _max)
                return 0;
            return 1.0 / (_max - _min);
        }

        /// <summary>
        /// 累积分布函数
        /// </summary>
        public double CDF(double x)
        {
            if (x < _min)
                return 0;
            if (x > _max)
                return 1;
            return (x - _min) / (_max - _min);
        }
    }

    /// <summary>
    /// 均匀整数分布
    /// </summary>
    public class UniformIntDistribution
    {
        private readonly int _min;
        private readonly int _max;
        private readonly Random _random;

        /// <summary>
        /// 最小值（包含）
        /// </summary>
        public int Min => _min;

        /// <summary>
        /// 最大值（包含）
        /// </summary>
        public int Max => _max;

        /// <summary>
        /// 均值
        /// </summary>
        public double Mean => (_min + _max) / 2.0;

        /// <summary>
        /// 方差
        /// </summary>
        public double Variance => ((_max - _min + 1) * (_max - _min + 1) - 1) / 12.0;

        /// <summary>
        /// 创建均匀整数分布
        /// </summary>
        public UniformIntDistribution(int min, int max)
        {
            if (max < min)
                throw new ArgumentException("Max must be greater than or equal to min");

            _min = min;
            _max = max;
            _random = new Random();
        }

        /// <summary>
        /// 采样一个值
        /// </summary>
        public int Sample()
        {
            return _random.Next(_min, _max + 1);
        }

        /// <summary>
        /// 采样多个值
        /// </summary>
        public List<int> Sample(int count)
        {
            var result = new List<int>(count);
            for (int i = 0; i < count; i++)
            {
                result.Add(Sample());
            }
            return result;
        }

        /// <summary>
        /// 概率质量函数
        /// </summary>
        public double PMF(int k)
        {
            if (k < _min || k > _max)
                return 0;
            return 1.0 / (_max - _min + 1);
        }

        /// <summary>
        /// 累积分布函数
        /// </summary>
        public double CDF(int k)
        {
            if (k < _min)
                return 0;
            if (k > _max)
                return 1;
            return (double)(k - _min + 1) / (_max - _min + 1);
        }
    }
}
