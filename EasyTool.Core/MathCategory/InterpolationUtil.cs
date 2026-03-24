using System;
using System.Collections.Generic;

namespace EasyTool.MathCategory
{
    /// <summary>
    /// 插值工具类
    /// 提供各种插值算法
    /// </summary>
    public static class InterpolationUtil
    {
        /// <summary>
        /// 线性插值
        /// </summary>
        public static double Linear(double x0, double y0, double x1, double y1, double x)
        {
            if (Math.Abs(x1 - x0) < double.Epsilon)
                return y0;

            return y0 + (y1 - y0) * (x - x0) / (x1 - x0);
        }

        /// <summary>
        /// 双线性插值
        /// </summary>
        public static double Bilinear(double x, double y,
            double x1, double y1, double v11,
            double x2, double y2, double v12,
            double x3, double y3, double v21,
            double x4, double y4, double v22)
        {
            double r1 = Linear(x1, v11, x2, v12, x);
            double r2 = Linear(x3, v21, x4, v22, x);
            return Linear(y1, r1, y3, r2, y);
        }

        /// <summary>
        /// 拉格朗日插值
        /// </summary>
        public static double Lagrange(double[] xValues, double[] yValues, double x)
        {
            if (xValues == null || yValues == null)
                throw new ArgumentNullException();
            if (xValues.Length != yValues.Length)
                throw new ArgumentException("Arrays must have the same length");
            if (xValues.Length == 0)
                throw new ArgumentException("Arrays cannot be empty");

            int n = xValues.Length;
            double result = 0;

            for (int i = 0; i < n; i++)
            {
                double term = yValues[i];
                for (int j = 0; j < n; j++)
                {
                    if (i != j)
                    {
                        term *= (x - xValues[j]) / (xValues[i] - xValues[j]);
                    }
                }
                result += term;
            }

            return result;
        }

        /// <summary>
        /// 牛顿插值
        /// </summary>
        public static double Newton(double[] xValues, double[] yValues, double x)
        {
            if (xValues == null || yValues == null)
                throw new ArgumentNullException();
            if (xValues.Length != yValues.Length)
                throw new ArgumentException("Arrays must have the same length");
            if (xValues.Length == 0)
                throw new ArgumentException("Arrays cannot be empty");

            int n = xValues.Length;

            // 计算差商表
            double[,] dividedDiff = new double[n, n];
            for (int i = 0; i < n; i++)
            {
                dividedDiff[i, 0] = yValues[i];
            }

            for (int j = 1; j < n; j++)
            {
                for (int i = 0; i < n - j; i++)
                {
                    dividedDiff[i, j] = (dividedDiff[i + 1, j - 1] - dividedDiff[i, j - 1]) /
                                       (xValues[i + j] - xValues[i]);
                }
            }

            // 计算插值
            double result = dividedDiff[0, 0];
            double term = 1;

            for (int i = 1; i < n; i++)
            {
                term *= (x - xValues[i - 1]);
                result += term * dividedDiff[0, i];
            }

            return result;
        }

        /// <summary>
        /// 创建三次样条插值器
        /// </summary>
        public static CubicSpline CreateCubicSpline(double[] xValues, double[] yValues)
        {
            return new CubicSpline(xValues, yValues);
        }

        /// <summary>
        /// 创建线性插值器
        /// </summary>
        public static LinearInterpolator CreateLinearInterpolator(double[] xValues, double[] yValues)
        {
            return new LinearInterpolator(xValues, yValues);
        }
    }

    /// <summary>
    /// 三次样条插值
    /// </summary>
    public class CubicSpline
    {
        private readonly double[] _x;
        private readonly double[] _y;
        private readonly double[] _m; // 二阶导数

        /// <summary>
        /// 数据点数量
        /// </summary>
        public int Count => _x.Length;

        /// <summary>
        /// 创建三次样条插值
        /// </summary>
        public CubicSpline(double[] xValues, double[] yValues)
        {
            if (xValues == null || yValues == null)
                throw new ArgumentNullException();
            if (xValues.Length != yValues.Length)
                throw new ArgumentException("Arrays must have the same length");
            if (xValues.Length < 2)
                throw new ArgumentException("At least 2 points required");

            _x = (double[])xValues.Clone();
            _y = (double[])yValues.Clone();
            _m = ComputeSecondDerivatives();
        }

        private double[] ComputeSecondDerivatives()
        {
            int n = _x.Length;
            double[] m = new double[n];
            double[] u = new double[n - 1];
            double[] y = new double[n - 1];

            // 自然边界条件
            m[0] = 0;
            m[n - 1] = 0;

            // 追赶法求解三对角方程组
            for (int i = 1; i < n - 1; i++)
            {
                double hi = _x[i] - _x[i - 1];
                double hi1 = _x[i + 1] - _x[i];
                double alpha = hi / (hi + hi1);
                double beta = (3 * (1 - alpha) * (_y[i] - _y[i - 1]) / hi +
                              3 * alpha * (_y[i + 1] - _y[i]) / hi1) / (hi + hi1);

                double p = alpha * m[i - 1] + 2;
                m[i] = (alpha - 1) / p;
                u[i] = (beta - alpha * u[i - 1]) / p;
            }

            for (int i = n - 2; i > 0; i--)
            {
                m[i] = m[i] * m[i + 1] + u[i];
            }

            return m;
        }

        /// <summary>
        /// 插值计算
        /// </summary>
        public double Interpolate(double x)
        {
            int n = _x.Length;

            // 二分查找区间
            int i = Array.BinarySearch(_x, x);
            if (i < 0) i = ~i;
            if (i == 0) i = 1;
            if (i >= n) i = n - 1;

            double h = _x[i] - _x[i - 1];
            double t = (x - _x[i - 1]) / h;

            // 三次样条公式
            double a = _y[i - 1];
            double b = (_y[i] - _y[i - 1]) / h - h * (_m[i] + 2 * _m[i - 1]) / 6;
            double c = _m[i - 1] / 2;
            double d = (_m[i] - _m[i - 1]) / (6 * h);

            return a + b * t * h + c * t * t * h * h + d * t * t * t * h * h * h;
        }

        /// <summary>
        /// 计算导数
        /// </summary>
        public double Derivative(double x)
        {
            int n = _x.Length;

            int i = Array.BinarySearch(_x, x);
            if (i < 0) i = ~i;
            if (i == 0) i = 1;
            if (i >= n) i = n - 1;

            double h = _x[i] - _x[i - 1];
            double t = (x - _x[i - 1]) / h;

            double b = (_y[i] - _y[i - 1]) / h - h * (_m[i] + 2 * _m[i - 1]) / 6;
            double c = _m[i - 1];
            double d = (_m[i] - _m[i - 1]) / (2 * h);

            return b + c * t * h + d * t * t * h * h;
        }
    }

    /// <summary>
    /// 线性插值器
    /// </summary>
    public class LinearInterpolator
    {
        private readonly double[] _x;
        private readonly double[] _y;

        /// <summary>
        /// 数据点数量
        /// </summary>
        public int Count => _x.Length;

        /// <summary>
        /// 创建线性插值器
        /// </summary>
        public LinearInterpolator(double[] xValues, double[] yValues)
        {
            if (xValues == null || yValues == null)
                throw new ArgumentNullException();
            if (xValues.Length != yValues.Length)
                throw new ArgumentException("Arrays must have the same length");
            if (xValues.Length < 2)
                throw new ArgumentException("At least 2 points required");

            _x = (double[])xValues.Clone();
            _y = (double[])yValues.Clone();
        }

        /// <summary>
        /// 插值计算
        /// </summary>
        public double Interpolate(double x)
        {
            int n = _x.Length;

            int i = Array.BinarySearch(_x, x);
            if (i < 0) i = ~i;
            if (i == 0) return _y[0];
            if (i >= n) return _y[n - 1];

            return InterpolationUtil.Linear(_x[i - 1], _y[i - 1], _x[i], _y[i], x);
        }
    }
}
