using System;

namespace EasyTool.MathCategory
{
    /// <summary>
    /// 分数工具类
    /// 提供精确的有理数运算
    /// </summary>
    public static class FractionUtil
    {
        /// <summary>
        /// 创建分数
        /// </summary>
        public static Fraction Create(long numerator, long denominator = 1)
        {
            return new Fraction(numerator, denominator);
        }

        /// <summary>
        /// 从小数创建分数
        /// </summary>
        public static Fraction FromDouble(double value, long maxDenominator = 1000000)
        {
            return Fraction.FromDouble(value, maxDenominator);
        }

        /// <summary>
        /// 解析分数字符串（如 "3/4"）
        /// </summary>
        public static Fraction Parse(string s)
        {
            return Fraction.Parse(s);
        }

        /// <summary>
        /// 尝试解析分数字符串
        /// </summary>
        public static bool TryParse(string s, out Fraction result)
        {
            return Fraction.TryParse(s, out result);
        }

        /// <summary>
        /// 获取最小公倍数
        /// </summary>
        public static long LCM(long a, long b)
        {
            return Math.Abs(a * b) / GCD(a, b);
        }

        /// <summary>
        /// 获取最大公约数
        /// </summary>
        public static long GCD(long a, long b)
        {
            a = Math.Abs(a);
            b = Math.Abs(b);
            while (b != 0)
            {
                long temp = b;
                b = a % b;
                a = temp;
            }
            return a;
        }
    }

    /// <summary>
    /// 分数（有理数）
    /// </summary>
    public readonly struct Fraction : IComparable<Fraction>, IEquatable<Fraction>
    {
        /// <summary>
        /// 分子
        /// </summary>
        public long Numerator { get; }

        /// <summary>
        /// 分母
        /// </summary>
        public long Denominator { get; }

        /// <summary>
        /// 零
        /// </summary>
        public static Fraction Zero => new(0, 1);

        /// <summary>
        /// 一
        /// </summary>
        public static Fraction One => new(1, 1);

        /// <summary>
        /// 二分之一
        /// </summary>
        public static Fraction Half => new(1, 2);

        /// <summary>
        /// 创建分数
        /// </summary>
        public Fraction(long numerator, long denominator = 1)
        {
            if (denominator == 0)
                throw new DivideByZeroException("Denominator cannot be zero");

            // 约分
            long gcd = FractionUtil.GCD(numerator, denominator);
            numerator /= gcd;
            denominator /= gcd;

            // 确保分母为正
            if (denominator < 0)
            {
                numerator = -numerator;
                denominator = -denominator;
            }

            Numerator = numerator;
            Denominator = denominator;
        }

        /// <summary>
        /// 从小数创建分数
        /// </summary>
        public static Fraction FromDouble(double value, long maxDenominator = 1000000)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
                throw new ArgumentException("Cannot convert NaN or infinity to fraction");

            long sign = value < 0 ? -1 : 1;
            value = Math.Abs(value);

            long wholePart = (long)value;
            double fractionalPart = value - wholePart;

            if (fractionalPart < 1e-15)
            {
                return new Fraction(sign * wholePart, 1);
            }

            // 使用连分数算法
            long numerator = 1;
            long denominator = (long)(1 / fractionalPart);
            double remainder = 1 / fractionalPart - denominator;

            while (Math.Abs(fractionalPart - (double)numerator / denominator) > 1e-15 && denominator < maxDenominator)
            {
                long newNumerator = denominator;
                long newDenominator = (long)(1 / remainder);
                remainder = 1 / remainder - newDenominator;

                if (newDenominator == 0 || denominator + newDenominator > maxDenominator)
                    break;

                numerator = newNumerator;
                denominator = denominator + newDenominator;
            }

            // 简化计算
            long bestDen = 1;
            double bestError = Math.Abs(fractionalPart);

            for (long d = 1; d <= Math.Min(maxDenominator, 10000); d++)
            {
                long n = (long)Math.Round(fractionalPart * d);
                double error = Math.Abs(fractionalPart - (double)n / d);
                if (error < bestError)
                {
                    bestError = error;
                    bestDen = d;
                }
            }

            long finalNumerator = (long)Math.Round(fractionalPart * bestDen);
            return new Fraction(sign * (wholePart * bestDen + finalNumerator), bestDen);
        }

        /// <summary>
        /// 解析分数字符串
        /// </summary>
        public static Fraction Parse(string s)
        {
            if (!TryParse(s, out var result))
                throw new FormatException($"Cannot parse '{s}' as fraction");
            return result;
        }

        /// <summary>
        /// 尝试解析分数字符串
        /// </summary>
        public static bool TryParse(string s, out Fraction result)
        {
            result = Zero;

            if (string.IsNullOrWhiteSpace(s))
                return false;

            s = s.Trim();

            // 处理负号
            int sign = 1;
            if (s.StartsWith("-"))
            {
                sign = -1;
                s = s.Substring(1);
            }

            // 尝试解析为纯数字
            if (long.TryParse(s, out long whole))
            {
                result = new Fraction(sign * whole, 1);
                return true;
            }

            // 尝试解析为分数
            if (s.Contains("/"))
            {
                var parts = s.Split('/');
                if (parts.Length == 2 &&
                    long.TryParse(parts[0], out long num) &&
                    long.TryParse(parts[1], out long den))
                {
                    result = new Fraction(sign * num, den);
                    return true;
                }
            }

            // 尝试解析为带分数（如 "1 1/2"）
            if (s.Contains(" "))
            {
                var parts = s.Split(' ');
                if (parts.Length == 2 &&
                    long.TryParse(parts[0], out long whole2) &&
                    parts[1].Contains("/"))
                {
                    var fracParts = parts[1].Split('/');
                    if (fracParts.Length == 2 &&
                        long.TryParse(fracParts[0], out long num) &&
                        long.TryParse(fracParts[1], out long den))
                    {
                        result = new Fraction(sign * (whole2 * den + num), den);
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 转换为小数
        /// </summary>
        public double ToDouble() => (double)Numerator / Denominator;

        /// <summary>
        /// 转换为小数（decimal）
        /// </summary>
        public decimal ToDecimal() => (decimal)Numerator / Denominator;

        /// <summary>
        /// 获取倒数
        /// </summary>
        public Fraction Reciprocal => new(Denominator, Numerator);

        /// <summary>
        /// 获取绝对值
        /// </summary>
        public Fraction Abs => new(Math.Abs(Numerator), Denominator);

        /// <summary>
        /// 取反
        /// </summary>
        public Fraction Negate => new(-Numerator, Denominator);

        /// <summary>
        /// 约分
        /// </summary>
        public Fraction Simplify()
        {
            if (Numerator == 0) return Zero;

            long gcd = FractionUtil.GCD(Numerator, Denominator);
            return new Fraction(Numerator / gcd, Denominator / gcd);
        }

        /// <summary>
        /// 转换为带分数
        /// </summary>
        public (long Whole, Fraction Fractional) ToMixedNumber()
        {
            long whole = Numerator / Denominator;
            long remainder = Numerator % Denominator;
            return (whole, new Fraction(remainder, Denominator));
        }

        #region 运算符

        public static Fraction operator +(Fraction a, Fraction b)
        {
            long den = FractionUtil.LCM(a.Denominator, b.Denominator);
            long num = a.Numerator * (den / a.Denominator) + b.Numerator * (den / b.Denominator);
            return new Fraction(num, den);
        }

        public static Fraction operator -(Fraction a, Fraction b)
        {
            long den = FractionUtil.LCM(a.Denominator, b.Denominator);
            long num = a.Numerator * (den / a.Denominator) - b.Numerator * (den / b.Denominator);
            return new Fraction(num, den);
        }

        public static Fraction operator *(Fraction a, Fraction b)
        {
            return new Fraction(a.Numerator * b.Numerator, a.Denominator * b.Denominator);
        }

        public static Fraction operator /(Fraction a, Fraction b)
        {
            if (b.Numerator == 0)
                throw new DivideByZeroException();
            return new Fraction(a.Numerator * b.Denominator, a.Denominator * b.Numerator);
        }

        public static Fraction operator %(Fraction a, Fraction b)
        {
            return a - (a / b).Floor * b;
        }

        public static Fraction operator +(Fraction a) => a;
        public static Fraction operator -(Fraction a) => a.Negate;

        public static bool operator ==(Fraction a, Fraction b) => a.Equals(b);
        public static bool operator !=(Fraction a, Fraction b) => !a.Equals(b);
        public static bool operator <(Fraction a, Fraction b) => a.CompareTo(b) < 0;
        public static bool operator >(Fraction a, Fraction b) => a.CompareTo(b) > 0;
        public static bool operator <=(Fraction a, Fraction b) => a.CompareTo(b) <= 0;
        public static bool operator >=(Fraction a, Fraction b) => a.CompareTo(b) >= 0;

        public static implicit operator Fraction(long value) => new(value, 1);
        public static implicit operator Fraction(int value) => new(value, 1);
        public static explicit operator double(Fraction f) => f.ToDouble();
        public static explicit operator decimal(Fraction f) => f.ToDecimal();

        #endregion

        /// <summary>
        /// 向下取整
        /// </summary>
        public Fraction Floor => new(Numerator / Denominator, 1);

        /// <summary>
        /// 向上取整
        /// </summary>
        public Fraction Ceiling => new((Numerator + Denominator - 1) / Denominator, 1);

        /// <summary>
        /// 四舍五入
        /// </summary>
        public Fraction Round()
        {
            var mixed = ToMixedNumber();
            if (mixed.Fractional >= Half)
                return new Fraction(mixed.Whole + 1, 1);
            return new Fraction(mixed.Whole, 1);
        }

        public int CompareTo(Fraction other)
        {
            return (Numerator * other.Denominator).CompareTo(other.Numerator * Denominator);
        }

        public bool Equals(Fraction other)
        {
            return Numerator == other.Numerator && Denominator == other.Denominator;
        }

        public override bool Equals(object obj)
        {
            return obj is Fraction other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Numerator, Denominator);
        }

        public override string ToString()
        {
            if (Denominator == 1)
                return Numerator.ToString();
            return $"{Numerator}/{Denominator}";
        }

        /// <summary>
        /// 转换为带分数字符串
        /// </summary>
        public string ToMixedString()
        {
            var (whole, frac) = ToMixedNumber();
            if (whole == 0) return frac.ToString();
            if (frac.Numerator == 0) return whole.ToString();
            return $"{whole} {frac}";
        }
    }
}
