using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyTool.CollectionsCategory
{
    /// <summary>
    /// 统计工具类
    /// 提供常用的统计分析功能
    /// </summary>
    public static class StatisticsUtil
    {
        /// <summary>
        /// 计算中位数
        /// </summary>
        public static double Median(IEnumerable<double> values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            var sorted = values.OrderBy(x => x).ToList();
            if (sorted.Count == 0)
                throw new ArgumentException("Collection is empty");

            int count = sorted.Count;
            int mid = count / 2;

            if (count % 2 == 0)
            {
                return (sorted[mid - 1] + sorted[mid]) / 2.0;
            }
            return sorted[mid];
        }

        /// <summary>
        /// 计算中位数（泛型版本）
        /// </summary>
        public static double Median<T>(IEnumerable<T> values, Func<T, double> selector)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            return Median(values.Select(selector));
        }

        /// <summary>
        /// 计算百分位数
        /// </summary>
        /// <param name="values">数据集合</param>
        /// <param name="percentile">百分位数（0-100）</param>
        public static double Percentile(IEnumerable<double> values, double percentile)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));
            if (percentile < 0 || percentile > 100)
                throw new ArgumentOutOfRangeException(nameof(percentile), "Percentile must be between 0 and 100");

            var sorted = values.OrderBy(x => x).ToList();
            if (sorted.Count == 0)
                throw new ArgumentException("Collection is empty");

            if (percentile == 0)
                return sorted[0];
            if (percentile == 100)
                return sorted[sorted.Count - 1];

            double position = (sorted.Count - 1) * percentile / 100.0;
            int lower = (int)Math.Floor(position);
            int upper = (int)Math.Ceiling(position);

            if (lower == upper)
                return sorted[lower];

            return sorted[lower] + (position - lower) * (sorted[upper] - sorted[lower]);
        }

        /// <summary>
        /// 计算百分位数（泛型版本）
        /// </summary>
        public static double Percentile<T>(IEnumerable<T> values, Func<T, double> selector, double percentile)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            return Percentile(values.Select(selector), percentile);
        }

        /// <summary>
        /// 计算四分位数
        /// </summary>
        /// <returns>Q1, Q2(中位数), Q3</returns>
        public static (double Q1, double Q2, double Q3) Quartiles(IEnumerable<double> values)
        {
            var sorted = values.OrderBy(x => x).ToList();
            if (sorted.Count == 0)
                throw new ArgumentException("Collection is empty");

            return (
                Percentile(sorted, 25),
                Percentile(sorted, 50),
                Percentile(sorted, 75)
            );
        }

        /// <summary>
        /// 计算标准差（总体标准差）
        /// </summary>
        public static double StandardDeviation(IEnumerable<double> values)
        {
            return StandardDeviation(values, false);
        }

        /// <summary>
        /// 计算标准差
        /// </summary>
        /// <param name="values">数据集合</param>
        /// <param name="isSample">是否为样本标准差（使用 n-1）</param>
        public static double StandardDeviation(IEnumerable<double> values, bool isSample)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            var list = values.ToList();
            if (list.Count == 0)
                throw new ArgumentException("Collection is empty");
            if (list.Count == 1 && isSample)
                throw new ArgumentException("Sample standard deviation requires at least 2 values");

            double mean = list.Average();
            double sumSquaredDiff = list.Sum(x => Math.Pow(x - mean, 2));
            int divisor = isSample ? list.Count - 1 : list.Count;

            return Math.Sqrt(sumSquaredDiff / divisor);
        }

        /// <summary>
        /// 计算标准差（泛型版本）
        /// </summary>
        public static double StandardDeviation<T>(IEnumerable<T> values, Func<T, double> selector, bool isSample = false)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            return StandardDeviation(values.Select(selector), isSample);
        }

        /// <summary>
        /// 计算方差
        /// </summary>
        /// <param name="values">数据集合</param>
        /// <param name="isSample">是否为样本方差</param>
        public static double Variance(IEnumerable<double> values, bool isSample = false)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            var list = values.ToList();
            if (list.Count == 0)
                throw new ArgumentException("Collection is empty");
            if (list.Count == 1 && isSample)
                throw new ArgumentException("Sample variance requires at least 2 values");

            double mean = list.Average();
            double sumSquaredDiff = list.Sum(x => Math.Pow(x - mean, 2));
            int divisor = isSample ? list.Count - 1 : list.Count;

            return sumSquaredDiff / divisor;
        }

        /// <summary>
        /// 计算方差（泛型版本）
        /// </summary>
        public static double Variance<T>(IEnumerable<T> values, Func<T, double> selector, bool isSample = false)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            return Variance(values.Select(selector), isSample);
        }

        /// <summary>
        /// 计算众数（出现次数最多的值）
        /// </summary>
        public static T Mode<T>(IEnumerable<T> values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            var groups = values.GroupBy(x => x).ToList();
            if (groups.Count == 0)
                throw new ArgumentException("Collection is empty");

            int maxCount = groups.Max(g => g.Count());
            var modes = groups.Where(g => g.Count() == maxCount).Select(g => g.Key).ToList();

            if (modes.Count > 1)
                throw new ArgumentException("Multiple modes exist");

            return modes[0];
        }

        /// <summary>
        /// 获取所有众数
        /// </summary>
        public static List<T> Modes<T>(IEnumerable<T> values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            var groups = values.GroupBy(x => x).ToList();
            if (groups.Count == 0)
                throw new ArgumentException("Collection is empty");

            int maxCount = groups.Max(g => g.Count());
            return groups.Where(g => g.Count() == maxCount).Select(g => g.Key).ToList();
        }

        /// <summary>
        /// 计算频率分布
        /// </summary>
        public static Dictionary<T, int> Frequency<T>(IEnumerable<T> values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            return values.GroupBy(x => x)
                        .ToDictionary(g => g.Key, g => g.Count());
        }

        /// <summary>
        /// 计算相对频率分布
        /// </summary>
        public static Dictionary<T, double> RelativeFrequency<T>(IEnumerable<T> values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            var list = values.ToList();
            if (list.Count == 0)
                throw new ArgumentException("Collection is empty");

            return list.GroupBy(x => x)
                      .ToDictionary(g => g.Key, g => (double)g.Count() / list.Count);
        }

        /// <summary>
        /// 计算累计频率分布
        /// </summary>
        public static Dictionary<T, int> CumulativeFrequency<T>(IEnumerable<T> values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            var freq = Frequency(values);
            var sorted = freq.OrderBy(x => x.Key).ToList();
            var result = new Dictionary<T, int>();
            int cumulative = 0;

            foreach (var kvp in sorted)
            {
                cumulative += kvp.Value;
                result[kvp.Key] = cumulative;
            }

            return result;
        }

        /// <summary>
        /// 计算范围（极差）
        /// </summary>
        public static double Range(IEnumerable<double> values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            var list = values.ToList();
            if (list.Count == 0)
                throw new ArgumentException("Collection is empty");

            return list.Max() - list.Min();
        }

        /// <summary>
        /// 计算范围（泛型版本）
        /// </summary>
        public static double Range<T>(IEnumerable<T> values, Func<T, double> selector)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            return Range(values.Select(selector));
        }

        /// <summary>
        /// 计算四分位距（IQR）
        /// </summary>
        public static double InterquartileRange(IEnumerable<double> values)
        {
            var (q1, q2, q3) = Quartiles(values);
            return q3 - q1;
        }

        /// <summary>
        /// 计算变异系数（CV）
        /// </summary>
        public static double CoefficientOfVariation(IEnumerable<double> values, bool isSample = false)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            var list = values.ToList();
            if (list.Count == 0)
                throw new ArgumentException("Collection is empty");

            double mean = list.Average();
            if (mean == 0)
                throw new ArgumentException("Mean is zero, cannot calculate coefficient of variation");

            return StandardDeviation(list, isSample) / Math.Abs(mean);
        }

        /// <summary>
        /// 计算偏度
        /// </summary>
        public static double Skewness(IEnumerable<double> values, bool isSample = false)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            var list = values.ToList();
            if (list.Count < 3)
                throw new ArgumentException("Skewness requires at least 3 values");

            double mean = list.Average();
            double stdDev = StandardDeviation(list, isSample);
            if (stdDev == 0)
                return 0;

            int n = list.Count;
            double skew = list.Sum(x => Math.Pow((x - mean) / stdDev, 3));

            if (isSample)
            {
                return skew * n / ((n - 1) * (n - 2));
            }
            return skew / n;
        }

        /// <summary>
        /// 计算峰度
        /// </summary>
        public static double Kurtosis(IEnumerable<double> values, bool isSample = false)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            var list = values.ToList();
            if (list.Count < 4)
                throw new ArgumentException("Kurtosis requires at least 4 values");

            double mean = list.Average();
            double stdDev = StandardDeviation(list, isSample);
            if (stdDev == 0)
                return 0;

            int n = list.Count;
            double kurt = list.Sum(x => Math.Pow((x - mean) / stdDev, 4));

            if (isSample)
            {
                return kurt * n * (n + 1) / ((n - 1) * (n - 2) * (n - 3)) - 3.0 * (n - 1) * (n - 1) / ((n - 2) * (n - 3));
            }
            return kurt / n - 3;
        }

        /// <summary>
        /// 计算协方差
        /// </summary>
        public static double Covariance(IEnumerable<double> x, IEnumerable<double> y, bool isSample = false)
        {
            if (x == null)
                throw new ArgumentNullException(nameof(x));
            if (y == null)
                throw new ArgumentNullException(nameof(y));

            var xList = x.ToList();
            var yList = y.ToList();

            if (xList.Count != yList.Count)
                throw new ArgumentException("Collections must have the same length");
            if (xList.Count == 0)
                throw new ArgumentException("Collections are empty");
            if (xList.Count == 1 && isSample)
                throw new ArgumentException("Sample covariance requires at least 2 values");

            double meanX = xList.Average();
            double meanY = yList.Average();

            double sum = 0;
            for (int i = 0; i < xList.Count; i++)
            {
                sum += (xList[i] - meanX) * (yList[i] - meanY);
            }

            int divisor = isSample ? xList.Count - 1 : xList.Count;
            return sum / divisor;
        }

        /// <summary>
        /// 计算皮尔逊相关系数
        /// </summary>
        public static double Correlation(IEnumerable<double> x, IEnumerable<double> y)
        {
            if (x == null)
                throw new ArgumentNullException(nameof(x));
            if (y == null)
                throw new ArgumentNullException(nameof(y));

            var xList = x.ToList();
            var yList = y.ToList();

            if (xList.Count != yList.Count)
                throw new ArgumentException("Collections must have the same length");
            if (xList.Count < 2)
                throw new ArgumentException("Correlation requires at least 2 values");

            double stdDevX = StandardDeviation(xList, true);
            double stdDevY = StandardDeviation(yList, true);

            if (stdDevX == 0 || stdDevY == 0)
                return 0;

            return Covariance(xList, yList, true) / (stdDevX * stdDevY);
        }

        /// <summary>
        /// 计算几何平均数
        /// </summary>
        public static double GeometricMean(IEnumerable<double> values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            var list = values.ToList();
            if (list.Count == 0)
                throw new ArgumentException("Collection is empty");
            if (list.Any(x => x <= 0))
                throw new ArgumentException("All values must be positive for geometric mean");

            double logSum = list.Sum(x => Math.Log(x));
            return Math.Exp(logSum / list.Count);
        }

        /// <summary>
        /// 计算调和平均数
        /// </summary>
        public static double HarmonicMean(IEnumerable<double> values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            var list = values.ToList();
            if (list.Count == 0)
                throw new ArgumentException("Collection is empty");
            if (list.Any(x => x <= 0))
                throw new ArgumentException("All values must be positive for harmonic mean");

            double sumReciprocals = list.Sum(x => 1.0 / x);
            return list.Count / sumReciprocals;
        }

        /// <summary>
        /// 计算移动平均
        /// </summary>
        public static List<double> MovingAverage(IEnumerable<double> values, int windowSize)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));
            if (windowSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(windowSize));

            var list = values.ToList();
            if (list.Count < windowSize)
                throw new ArgumentException("Window size cannot be larger than collection size");

            var result = new List<double>();
            double sum = 0;

            for (int i = 0; i < list.Count; i++)
            {
                sum += list[i];
                if (i >= windowSize)
                {
                    sum -= list[i - windowSize];
                }
                if (i >= windowSize - 1)
                {
                    result.Add(sum / windowSize);
                }
            }

            return result;
        }

        /// <summary>
        /// 计算指数移动平均
        /// </summary>
        public static List<double> ExponentialMovingAverage(IEnumerable<double> values, double alpha)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));
            if (alpha <= 0 || alpha > 1)
                throw new ArgumentOutOfRangeException(nameof(alpha), "Alpha must be between 0 and 1");

            var list = values.ToList();
            if (list.Count == 0)
                throw new ArgumentException("Collection is empty");

            var result = new List<double> { list[0] };

            for (int i = 1; i < list.Count; i++)
            {
                double ema = alpha * list[i] + (1 - alpha) * result[i - 1];
                result.Add(ema);
            }

            return result;
        }

        /// <summary>
        /// 计算加权平均
        /// </summary>
        public static double WeightedAverage(IEnumerable<double> values, IEnumerable<double> weights)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));
            if (weights == null)
                throw new ArgumentNullException(nameof(weights));

            var valueList = values.ToList();
            var weightList = weights.ToList();

            if (valueList.Count != weightList.Count)
                throw new ArgumentException("Values and weights must have the same length");
            if (valueList.Count == 0)
                throw new ArgumentException("Collections are empty");

            double sumWeighted = 0;
            double sumWeights = 0;

            for (int i = 0; i < valueList.Count; i++)
            {
                sumWeighted += valueList[i] * weightList[i];
                sumWeights += weightList[i];
            }

            if (sumWeights == 0)
                throw new ArgumentException("Sum of weights cannot be zero");

            return sumWeighted / sumWeights;
        }

        /// <summary>
        /// 计算Z分数（标准化）
        /// </summary>
        public static List<double> ZScore(IEnumerable<double> values, bool isSample = false)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            var list = values.ToList();
            if (list.Count == 0)
                throw new ArgumentException("Collection is empty");

            double mean = list.Average();
            double stdDev = StandardDeviation(list, isSample);

            if (stdDev == 0)
                return list.Select(_ => 0.0).ToList();

            return list.Select(x => (x - mean) / stdDev).ToList();
        }

        /// <summary>
        /// 计算百分等级
        /// </summary>
        public static double PercentileRank(IEnumerable<double> values, double value)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            var list = values.ToList();
            if (list.Count == 0)
                throw new ArgumentException("Collection is empty");

            int below = list.Count(x => x < value);
            int equal = list.Count(x => x == value);

            return (below + 0.5 * equal) / list.Count * 100;
        }

        /// <summary>
        /// 计算描述性统计摘要
        /// </summary>
        public static StatisticSummary Summary(IEnumerable<double> values, bool isSample = false)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            var list = values.ToList();
            if (list.Count == 0)
                throw new ArgumentException("Collection is empty");

            var (q1, q2, q3) = Quartiles(list);

            return new StatisticSummary
            {
                Count = list.Count,
                Min = list.Min(),
                Max = list.Max(),
                Range = list.Max() - list.Min(),
                Sum = list.Sum(),
                Mean = list.Average(),
                Median = q2,
                Mode = list.GroupBy(x => x).OrderByDescending(g => g.Count()).First().Key,
                StandardDeviation = StandardDeviation(list, isSample),
                Variance = Variance(list, isSample),
                Q1 = q1,
                Q3 = q3,
                IQR = q3 - q1
            };
        }
    }

    /// <summary>
    /// 统计摘要
    /// </summary>
    public class StatisticSummary
    {
        /// <summary>
        /// 元素数量
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// 最小值
        /// </summary>
        public double Min { get; set; }

        /// <summary>
        /// 最大值
        /// </summary>
        public double Max { get; set; }

        /// <summary>
        /// 范围（极差）
        /// </summary>
        public double Range { get; set; }

        /// <summary>
        /// 总和
        /// </summary>
        public double Sum { get; set; }

        /// <summary>
        /// 平均值
        /// </summary>
        public double Mean { get; set; }

        /// <summary>
        /// 中位数
        /// </summary>
        public double Median { get; set; }

        /// <summary>
        /// 众数
        /// </summary>
        public double Mode { get; set; }

        /// <summary>
        /// 标准差
        /// </summary>
        public double StandardDeviation { get; set; }

        /// <summary>
        /// 方差
        /// </summary>
        public double Variance { get; set; }

        /// <summary>
        /// 第一四分位数
        /// </summary>
        public double Q1 { get; set; }

        /// <summary>
        /// 第三四分位数
        /// </summary>
        public double Q3 { get; set; }

        /// <summary>
        /// 四分位距
        /// </summary>
        public double IQR { get; set; }

        /// <summary>
        /// 返回字符串表示
        /// </summary>
        public override string ToString()
        {
            return $"Count: {Count}, Min: {Min:F4}, Max: {Max:F4}, Mean: {Mean:F4}, Median: {Median:F4}, StdDev: {StandardDeviation:F4}";
        }
    }
}
