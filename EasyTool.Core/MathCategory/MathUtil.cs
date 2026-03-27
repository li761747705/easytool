using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyTool.MathCategory
{
    /// <summary>
    /// 数学计算工具类
    /// </summary>
    public static class MathUtil
    {
        /// <summary>
        /// 计算平均值
        /// </summary>
        public static double Average(IEnumerable<double> values)
        {
            var list = values.ToList();
            return list.Count == 0 ? 0 : list.Sum() / list.Count;
        }

        /// <summary>
        /// 计算标准差
        /// </summary>
        public static double StandardDeviation(IEnumerable<double> values)
        {
            var list = values.ToList();
            if (list.Count == 0) return 0;

            var avg = Average(list);
            var sumOfSquares = list.Sum(v => Math.Pow(v - avg, 2));
            return Math.Sqrt(sumOfSquares / list.Count);
        }

        /// <summary>
        /// 计算方差
        /// </summary>
        public static double Variance(IEnumerable<double> values)
        {
            var list = values.ToList();
            if (list.Count == 0) return 0;

            var avg = Average(list);
            return list.Sum(v => Math.Pow(v - avg, 2)) / list.Count;
        }

        /// <summary>
        /// 计算中位数
        /// </summary>
        public static double Median(IEnumerable<double> values)
        {
            var sorted = values.OrderBy(v => v).ToList();
            if (sorted.Count == 0) return 0;

            var mid = sorted.Count / 2;
            return sorted.Count % 2 == 0
                ? (sorted[mid - 1] + sorted[mid]) / 2
                : sorted[mid];
        }

        /// <summary>
        /// 计算众数
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
        /// 计算百分位数
        /// </summary>
        public static double Percentile(IEnumerable<double> values, double percentile)
        {
            var sorted = values.OrderBy(v => v).ToList();
            if (sorted.Count == 0) return 0;

            var index = (percentile / 100) * (sorted.Count - 1);
            var lower = (int)Math.Floor(index);
            var upper = (int)Math.Ceiling(index);

            if (lower == upper) return sorted[lower];

            return sorted[lower] + (index - lower) * (sorted[upper] - sorted[lower]);
        }

        /// <summary>
        /// 限制值在指定范围内
        /// </summary>
        public static double Clamp(double value, double min, double max)
        {
            return Math.Max(min, Math.Min(max, value));
        }

        /// <summary>
        /// 线性插值
        /// </summary>
        public static double Lerp(double a, double b, double t)
        {
            return a + (b - a) * Clamp(t, 0, 1);
        }

        /// <summary>
        /// 反向线性插值
        /// </summary>
        public static double InverseLerp(double a, double b, double value)
        {
            if (a == b) return 0;
            return Clamp((value - a) / (b - a), 0, 1);
        }

        /// <summary>
        /// 映射值从一个范围到另一个范围
        /// </summary>
        public static double Remap(double value, double fromMin, double fromMax, double toMin, double toMax)
        {
            var t = InverseLerp(fromMin, fromMax, value);
            return Lerp(toMin, toMax, t);
        }

        /// <summary>
        /// 计算最大公约数
        /// </summary>
        public static long GCD(long a, long b)
        {
            a = Math.Abs(a);
            b = Math.Abs(b);

            while (b != 0)
            {
                var temp = b;
                b = a % b;
                a = temp;
            }

            return a;
        }

        /// <summary>
        /// 计算最大公约数（别名）
        /// </summary>
        public static long Gcd(long a, long b) => GCD(a, b);

        /// <summary>
        /// 计算最小公倍数
        /// </summary>
        public static long LCM(long a, long b)
        {
            if (a == 0 || b == 0) return 0;
            return Math.Abs(a * b) / GCD(a, b);
        }

        /// <summary>
        /// 计算最小公倍数（别名）
        /// </summary>
        public static long Lcm(long a, long b) => LCM(a, b);

        /// <summary>
        /// 判断是否为素数
        /// </summary>
        public static bool IsPrime(long n)
        {
            if (n < 2) return false;
            if (n == 2) return true;
            if (n % 2 == 0) return false;

            var sqrt = (long)Math.Sqrt(n);
            for (long i = 3; i <= sqrt; i += 2)
            {
                if (n % i == 0) return false;
            }

            return true;
        }

        /// <summary>
        /// 获取所有素数因子
        /// </summary>
        public static List<long> GetPrimeFactors(long n)
        {
            var factors = new List<long>();
            n = Math.Abs(n);

            while (n % 2 == 0)
            {
                factors.Add(2);
                n /= 2;
            }

            for (long i = 3; i * i <= n; i += 2)
            {
                while (n % i == 0)
                {
                    factors.Add(i);
                    n /= i;
                }
            }

            if (n > 2) factors.Add(n);

            return factors;
        }

        /// <summary>
        /// 计算阶乘
        /// </summary>
        public static long Factorial(int n)
        {
            if (n < 0) throw new ArgumentException("阶乘不支持负数");
            if (n <= 1) return 1;

            long result = 1;
            for (int i = 2; i <= n; i++)
            {
                result *= i;
            }

            return result;
        }

        /// <summary>
        /// 计算排列数 A(n, m)
        /// </summary>
        public static long Permutation(int n, int m)
        {
            if (m > n) return 0;
            if (m == 0) return 1;

            long result = 1;
            for (int i = 0; i < m; i++)
            {
                result *= (n - i);
            }

            return result;
        }

        /// <summary>
        /// 计算组合数 C(n, m)
        /// </summary>
        public static long Combination(int n, int m)
        {
            if (m > n) return 0;
            if (m == 0 || m == n) return 1;

            m = Math.Min(m, n - m);

            long result = 1;
            for (int i = 0; i < m; i++)
            {
                result = result * (n - i) / (i + 1);
            }

            return result;
        }

        /// <summary>
        /// 计算斐波那契数
        /// </summary>
        public static long Fibonacci(int n)
        {
            if (n < 0) throw new ArgumentException("斐波那契数不支持负数");
            if (n <= 1) return n;

            long a = 0, b = 1;
            for (int i = 2; i <= n; i++)
            {
                var temp = a + b;
                a = b;
                b = temp;
            }

            return b;
        }

        /// <summary>
        /// 判断是否在范围内
        /// </summary>
        public static bool InRange(double value, double min, double max)
        {
            return value >= min && value <= max;
        }

        /// <summary>
        /// 判断两个浮点数是否近似相等
        /// </summary>
        public static bool Approximately(double a, double b, double epsilon = 1e-10)
        {
            return Math.Abs(a - b) < epsilon;
        }

        /// <summary>
        /// 计算两点之间的距离
        /// </summary>
        public static double Distance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
        }

        /// <summary>
        /// 计算两点之间的角度（弧度）
        /// </summary>
        public static double Angle(double x1, double y1, double x2, double y2)
        {
            return Math.Atan2(y2 - y1, x2 - x1);
        }

        /// <summary>
        /// 弧度转角度
        /// </summary>
        public static double ToDegrees(double radians)
        {
            return radians * 180 / Math.PI;
        }

        /// <summary>
        /// 角度转弧度
        /// </summary>
        public static double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }
    }
}