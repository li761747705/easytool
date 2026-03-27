using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyTool.MathCategory
{
    /// <summary>
    /// 统计分析工具类
    /// 提供常用的统计分析功能
    /// </summary>
    public static class StatisticsUtil
    {
        #region 基础统计

        /// <summary>
        /// 计算总和
        /// </summary>
        public static double Sum(IEnumerable<double> values)
        {
            return values.Sum();
        }

        /// <summary>
        /// 计算平均值
        /// </summary>
        public static double Mean(IEnumerable<double> values)
        {
            var list = values.ToList();
            if (list.Count == 0) return 0;
            return list.Sum() / list.Count;
        }

        /// <summary>
        /// 计算中位数
        /// </summary>
        public static double Median(IEnumerable<double> values)
        {
            var sorted = values.OrderBy(v => v).ToList();
            var count = sorted.Count;

            if (count == 0) return 0;

            if (count % 2 == 0)
            {
                return (sorted[count / 2 - 1] + sorted[count / 2]) / 2.0;
            }

            return sorted[count / 2];
        }

        /// <summary>
        /// 计算众数（出现频率最高的值）
        /// </summary>
        public static List<double> Mode(IEnumerable<double> values)
        {
            var groups = values.GroupBy(v => v)
                              .OrderByDescending(g => g.Count())
                              .ToList();

            if (groups.Count == 0) return new List<double>();

            var maxCount = groups[0].Count();
            return groups.Where(g => g.Count() == maxCount)
                        .Select(g => g.Key)
                        .ToList();
        }

        /// <summary>
        /// 计算极差（最大值-最小值）
        /// </summary>
        public static double Range(IEnumerable<double> values)
        {
            var list = values.ToList();
            if (list.Count == 0) return 0;
            return list.Max() - list.Min();
        }

        /// <summary>
        /// 计算最小值
        /// </summary>
        public static double Min(IEnumerable<double> values)
        {
            return values.Min();
        }

        /// <summary>
        /// 计算最大值
        /// </summary>
        public static double Max(IEnumerable<double> values)
        {
            return values.Max();
        }

        /// <summary>
        /// 计算计数
        /// </summary>
        public static int Count(IEnumerable<double> values)
        {
            return values.Count();
        }

        #endregion

        #region 离散程度

        /// <summary>
        /// 计算方差（总体方差）
        /// </summary>
        public static double Variance(IEnumerable<double> values)
        {
            var list = values.ToList();
            if (list.Count == 0) return 0;

            var mean = Mean(list);
            var sumSquaredDiff = list.Sum(v => Math.Pow(v - mean, 2));
            return sumSquaredDiff / list.Count;
        }

        /// <summary>
        /// 计算样本方差
        /// </summary>
        public static double SampleVariance(IEnumerable<double> values)
        {
            var list = values.ToList();
            if (list.Count <= 1) return 0;

            var mean = Mean(list);
            var sumSquaredDiff = list.Sum(v => Math.Pow(v - mean, 2));
            return sumSquaredDiff / (list.Count - 1);
        }

        /// <summary>
        /// 计算标准差（总体标准差）
        /// </summary>
        public static double StandardDeviation(IEnumerable<double> values)
        {
            return Math.Sqrt(Variance(values));
        }

        /// <summary>
        /// 计算样本标准差
        /// </summary>
        public static double SampleStandardDeviation(IEnumerable<double> values)
        {
            return Math.Sqrt(SampleVariance(values));
        }

        /// <summary>
        /// 计算变异系数（标准差/平均值）
        /// </summary>
        public static double CoefficientOfVariation(IEnumerable<double> values)
        {
            var list = values.ToList();
            var mean = Mean(list);
            if (mean == 0) return 0;
            return StandardDeviation(list) / mean;
        }

        /// <summary>
        /// 计算平均绝对偏差
        /// </summary>
        public static double MeanAbsoluteDeviation(IEnumerable<double> values)
        {
            var list = values.ToList();
            if (list.Count == 0) return 0;

            var mean = Mean(list);
            return list.Average(v => Math.Abs(v - mean));
        }

        /// <summary>
        /// 计算四分位数
        /// </summary>
        /// <param name="values">数据集</param>
        /// <param name="q">四分位数类型（1=Q1, 2=Q2/中位数, 3=Q3）</param>
        /// <returns>四分位数值</returns>
        public static double Quartile(IEnumerable<double> values, int q)
        {
            if (q < 1 || q > 3)
                throw new ArgumentException("四分位数参数q必须为1、2或3", nameof(q));

            var sorted = values.OrderBy(v => v).ToList();
            var count = sorted.Count;

            if (count == 0) return 0;

            if (q == 2) return Median(sorted);

            double position;
            if (q == 1)
                position = (count + 1) / 4.0;
            else
                position = 3 * (count + 1) / 4.0;

            var lowerIndex = (int)Math.Floor(position) - 1;
            var upperIndex = (int)Math.Ceiling(position) - 1;
            var fraction = position - Math.Floor(position);

            if (lowerIndex == upperIndex || fraction == 0)
                return sorted[Math.Max(0, Math.Min(count - 1, lowerIndex))];

            lowerIndex = Math.Max(0, Math.Min(count - 1, lowerIndex));
            upperIndex = Math.Max(0, Math.Min(count - 1, upperIndex));

            return sorted[lowerIndex] * (1 - fraction) + sorted[upperIndex] * fraction;
        }

        /// <summary>
        /// 计算四分位距（IQR = Q3 - Q1）
        /// </summary>
        public static double InterquartileRange(IEnumerable<double> values)
        {
            return Quartile(values, 3) - Quartile(values, 1);
        }

        #endregion

        #region 百分位数

        /// <summary>
        /// 计算百分位数
        /// </summary>
        /// <param name="values">数据集</param>
        /// <param name="percentile">百分位（0-100）</param>
        /// <returns>百分位数值</returns>
        public static double Percentile(IEnumerable<double> values, double percentile)
        {
            if (percentile < 0 || percentile > 100)
                throw new ArgumentException("百分位必须在0-100之间", nameof(percentile));

            var sorted = values.OrderBy(v => v).ToList();
            var count = sorted.Count;

            if (count == 0) return 0;

            var position = (percentile / 100.0) * (count - 1);
            var lowerIndex = (int)Math.Floor(position);
            var upperIndex = (int)Math.Ceiling(position);
            var fraction = position - lowerIndex;

            if (lowerIndex == upperIndex)
                return sorted[lowerIndex];

            return sorted[lowerIndex] * (1 - fraction) + sorted[upperIndex] * fraction;
        }

        /// <summary>
        /// 计算百分等级（某个值在数据集中的百分位）
        /// </summary>
        public static double PercentileRank(IEnumerable<double> values, double value)
        {
            var list = values.ToList();
            var lessCount = list.Count(v => v < value);
            var equalCount = list.Count(v => v == value);
            var totalCount = list.Count;

            if (totalCount == 0) return 0;

            // 使用线性插值法
            return (lessCount + 0.5 * equalCount) / totalCount * 100;
        }

        #endregion

        #region 分布形状

        /// <summary>
        /// 计算偏度（Skewness）
        /// 正偏度表示右偏，负偏度表示左偏
        /// </summary>
        public static double Skewness(IEnumerable<double> values)
        {
            var list = values.ToList();
            if (list.Count < 3) return 0;

            var mean = Mean(list);
            var stdDev = StandardDeviation(list);
            if (stdDev == 0) return 0;

            var n = list.Count;
            var sumCubedDiff = list.Sum(v => Math.Pow((v - mean) / stdDev, 3));
            
            return (n / ((n - 1) * (n - 2))) * sumCubedDiff;
        }

        /// <summary>
        /// 计算峰度（Kurtosis）
        /// 正态分布峰度为0，大于0表示尖峰，小于0表示平峰
        /// </summary>
        public static double Kurtosis(IEnumerable<double> values)
        {
            var list = values.ToList();
            if (list.Count < 4) return 0;

            var mean = Mean(list);
            var stdDev = StandardDeviation(list);
            if (stdDev == 0) return 0;

            var n = list.Count;
            var sumFourthPower = list.Sum(v => Math.Pow((v - mean) / stdDev, 4));

            return (n * (n + 1) / ((n - 1) * (n - 2) * (n - 3))) * sumFourthPower 
                   - (3 * Math.Pow(n - 1, 2)) / ((n - 2) * (n - 3));
        }

        #endregion

        #region 协方差和相关系数

        /// <summary>
        /// 计算协方差
        /// </summary>
        public static double Covariance(IEnumerable<double> x, IEnumerable<double> y)
        {
            var xList = x.ToList();
            var yList = y.ToList();

            if (xList.Count != yList.Count)
                throw new ArgumentException("两个数据集的长度必须相同");

            if (xList.Count == 0) return 0;

            var meanX = Mean(xList);
            var meanY = Mean(yList);
            var n = xList.Count;

            return xList.Zip(yList, (xi, yi) => (xi - meanX) * (yi - meanY)).Sum() / n;
        }

        /// <summary>
        /// 计算皮尔逊相关系数
        /// </summary>
        public static double PearsonCorrelation(IEnumerable<double> x, IEnumerable<double> y)
        {
            var xList = x.ToList();
            var yList = y.ToList();

            if (xList.Count != yList.Count)
                throw new ArgumentException("两个数据集的长度必须相同");

            if (xList.Count == 0) return 0;

            var stdDevX = StandardDeviation(xList);
            var stdDevY = StandardDeviation(yList);

            if (stdDevX == 0 || stdDevY == 0) return 0;

            return Covariance(xList, yList) / (stdDevX * stdDevY);
        }

        /// <summary>
        /// 计算斯皮尔曼等级相关系数
        /// </summary>
        public static double SpearmanCorrelation(IEnumerable<double> x, IEnumerable<double> y)
        {
            var xList = x.ToList();
            var yList = y.ToList();

            if (xList.Count != yList.Count)
                throw new ArgumentException("两个数据集的长度必须相同");

            // 转换为秩
            var xRanks = GetRanks(xList);
            var yRanks = GetRanks(yList);

            return PearsonCorrelation(xRanks, yRanks);
        }

        private static List<double> GetRanks(List<double> values)
        {
            var sorted = values.Select((v, i) => new { Value = v, Index = i })
                              .OrderBy(x => x.Value)
                              .ToList();

            var ranks = new double[values.Count];
            for (int i = 0; i < sorted.Count; i++)
            {
                // 处理相同值的平均秩
                var sameValues = sorted.Where(s => s.Value == sorted[i].Value).ToList();
                var avgRank = sameValues.Select(s => s.Index).Average();
                ranks[sorted[i].Index] = avgRank + 1;
            }

            return ranks.ToList();
        }

        #endregion

        #region 回归分析

        /// <summary>
        /// 简单线性回归
        /// </summary>
        /// <returns>斜率和截距</returns>
        public static (double Slope, double Intercept) LinearRegression(IEnumerable<double> x, IEnumerable<double> y)
        {
            var xList = x.ToList();
            var yList = y.ToList();

            if (xList.Count != yList.Count)
                throw new ArgumentException("两个数据集的长度必须相同");

            var n = xList.Count;
            if (n == 0) return (0, 0);

            var meanX = Mean(xList);
            var meanY = Mean(yList);
            var stdDevX = StandardDeviation(xList);
            var stdDevY = StandardDeviation(yList);

            if (stdDevX == 0) return (0, meanY);

            var correlation = PearsonCorrelation(xList, yList);
            var slope = correlation * stdDevY / stdDevX;
            var intercept = meanY - slope * meanX;

            return (slope, intercept);
        }

        /// <summary>
        /// 使用回归模型预测
        /// </summary>
        public static double Predict(double x, double slope, double intercept)
        {
            return slope * x + intercept;
        }

        /// <summary>
        /// 计算R平方（决定系数）
        /// </summary>
        public static double RSquared(IEnumerable<double> actual, IEnumerable<double> predicted)
        {
            var actualList = actual.ToList();
            var predictedList = predicted.ToList();

            if (actualList.Count != predictedList.Count)
                throw new ArgumentException("两个数据集的长度必须相同");

            var mean = Mean(actualList);
            var ssTotal = actualList.Sum(a => Math.Pow(a - mean, 2));
            var ssResidual = actualList.Zip(predictedList, (a, p) => Math.Pow(a - p, 2)).Sum();

            if (ssTotal == 0) return 1;

            return 1 - (ssResidual / ssTotal);
        }

        #endregion

        #region 描述性统计

        /// <summary>
        /// 获取完整统计摘要
        /// </summary>
        public static StatisticsSummary GetSummary(IEnumerable<double> values)
        {
            var list = values.ToList();

            return new StatisticsSummary
            {
                Count = list.Count,
                Sum = Sum(list),
                Mean = Mean(list),
                Median = Median(list),
                Mode = Mode(list),
                Min = Min(list),
                Max = Max(list),
                Range = Range(list),
                Variance = Variance(list),
                StandardDeviation = StandardDeviation(list),
                SampleVariance = SampleVariance(list),
                SampleStandardDeviation = SampleStandardDeviation(list),
                Q1 = Quartile(list, 1),
                Q3 = Quartile(list, 3),
                IQR = InterquartileRange(list),
                Skewness = Skewness(list),
                Kurtosis = Kurtosis(list),
                CoefficientOfVariation = CoefficientOfVariation(list)
            };
        }

        #endregion

        #region 异常值检测

        /// <summary>
        /// 使用IQR方法检测异常值
        /// </summary>
        public static List<double> DetectOutliersIQR(IEnumerable<double> values, double multiplier = 1.5)
        {
            var list = values.ToList();
            var q1 = Quartile(list, 1);
            var q3 = Quartile(list, 3);
            var iqr = q3 - q1;

            var lowerBound = q1 - multiplier * iqr;
            var upperBound = q3 + multiplier * iqr;

            return list.Where(v => v < lowerBound || v > upperBound).ToList();
        }

        /// <summary>
        /// 使用Z-Score方法检测异常值
        /// </summary>
        public static List<double> DetectOutliersZScore(IEnumerable<double> values, double threshold = 3.0)
        {
            var list = values.ToList();
            var mean = Mean(list);
            var stdDev = StandardDeviation(list);

            if (stdDev == 0) return new List<double>();

            return list.Where(v => Math.Abs((v - mean) / stdDev) > threshold).ToList();
        }

        /// <summary>
        /// 计算Z-Score
        /// </summary>
        public static List<double> ZScore(IEnumerable<double> values)
        {
            var list = values.ToList();
            var mean = Mean(list);
            var stdDev = StandardDeviation(list);

            if (stdDev == 0) return list.Select(_ => 0.0).ToList();

            return list.Select(v => (v - mean) / stdDev).ToList();
        }

        #endregion
    }

    /// <summary>
    /// 统计摘要
    /// </summary>
    public class StatisticsSummary
    {
        /// <summary>
        /// 计数
        /// </summary>
        public int Count { get; set; }

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
        public List<double> Mode { get; set; } = new();

        /// <summary>
        /// 最小值
        /// </summary>
        public double Min { get; set; }

        /// <summary>
        /// 最大值
        /// </summary>
        public double Max { get; set; }

        /// <summary>
        /// 极差
        /// </summary>
        public double Range { get; set; }

        /// <summary>
        /// 总体方差
        /// </summary>
        public double Variance { get; set; }

        /// <summary>
        /// 总体标准差
        /// </summary>
        public double StandardDeviation { get; set; }

        /// <summary>
        /// 样本方差
        /// </summary>
        public double SampleVariance { get; set; }

        /// <summary>
        /// 样本标准差
        /// </summary>
        public double SampleStandardDeviation { get; set; }

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
        /// 偏度
        /// </summary>
        public double Skewness { get; set; }

        /// <summary>
        /// 峰度
        /// </summary>
        public double Kurtosis { get; set; }

        /// <summary>
        /// 变异系数
        /// </summary>
        public double CoefficientOfVariation { get; set; }

        public override string ToString()
        {
            return $"统计摘要: N={Count}, 均值={Mean:F4}, 标准差={StandardDeviation:F4}, 中位数={Median:F4}, 范围=[{Min:F4}, {Max:F4}]";
        }
    }
}
