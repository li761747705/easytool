using System;

namespace EasyTool.MathCategory
{
    /// <summary>
    /// 复数结构
    /// </summary>
    public struct ComplexNumber : IEquatable<ComplexNumber>, IFormattable
    {
        /// <summary>
        /// 实部
        /// </summary>
        public double Real { get; }

        /// <summary>
        /// 虚部
        /// </summary>
        public double Imaginary { get; }

        /// <summary>
        /// 模（绝对值）
        /// </summary>
        public double Magnitude => Math.Sqrt(Real * Real + Imaginary * Imaginary);

        /// <summary>
        /// 相位角（弧度）
        /// </summary>
        public double Phase => Math.Atan2(Imaginary, Real);

        /// <summary>
        /// 共轭复数
        /// </summary>
        public ComplexNumber Conjugate => new ComplexNumber(Real, -Imaginary);

        /// <summary>
        /// 创建复数
        /// </summary>
        public ComplexNumber(double real, double imaginary)
        {
            Real = real;
            Imaginary = imaginary;
        }

        #region 静态属性

        /// <summary>
        /// 零
        /// </summary>
        public static ComplexNumber Zero => new ComplexNumber(0, 0);

        /// <summary>
        /// 一
        /// </summary>
        public static ComplexNumber One => new ComplexNumber(1, 0);

        /// <summary>
        /// 虚数单位 i
        /// </summary>
        public static ComplexNumber ImaginaryOne => new ComplexNumber(0, 1);

        #endregion

        #region 静态方法

        /// <summary>
        /// 从极坐标创建复数
        /// </summary>
        public static ComplexNumber FromPolarCoordinates(double magnitude, double phase)
        {
            return new ComplexNumber(magnitude * Math.Cos(phase), magnitude * Math.Sin(phase));
        }

        /// <summary>
        /// 解析字符串为复数（支持格式: "a+bi", "a-bi", "a", "bi"）
        /// </summary>
        public static ComplexNumber Parse(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                throw new ArgumentException("字符串不能为空");

            s = s.Trim().Replace(" ", "");

            // 尝试解析纯实数
            if (double.TryParse(s, out var real))
                return new ComplexNumber(real, 0);

            // 解析复数
            int iIndex = s.LastIndexOf('i');
            if (iIndex < 0)
                throw new FormatException("无效的复数格式");

            int signIndex = s.LastIndexOfAny(new[] { '+', '-' }, iIndex - 1, iIndex);

            if (signIndex < 0)
            {
                // 只有虚部
                var imaginaryStr = s.Substring(0, iIndex);
                if (string.IsNullOrEmpty(imaginaryStr) || imaginaryStr == "+")
                    return new ComplexNumber(0, 1);
                if (imaginaryStr == "-")
                    return new ComplexNumber(0, -1);
                return new ComplexNumber(0, double.Parse(imaginaryStr));
            }

            var realStr = s.Substring(0, signIndex);
            var imagPartStr = s.Substring(signIndex, iIndex - signIndex);

            var realPart = string.IsNullOrEmpty(realStr) ? 0 : double.Parse(realStr);
            var imagPart = imagPartStr == "+" || imagPartStr == "" ? 1 : (imagPartStr == "-" ? -1 : double.Parse(imagPartStr));

            return new ComplexNumber(realPart, imagPart);
        }

        /// <summary>
        /// 尝试解析字符串为复数
        /// </summary>
        public static bool TryParse(string s, out ComplexNumber result)
        {
            try
            {
                result = Parse(s);
                return true;
            }
            catch
            {
                result = Zero;
                return false;
            }
        }

        #endregion

        #region 运算符重载

        public static ComplexNumber operator +(ComplexNumber a, ComplexNumber b)
            => new ComplexNumber(a.Real + b.Real, a.Imaginary + b.Imaginary);

        public static ComplexNumber operator -(ComplexNumber a, ComplexNumber b)
            => new ComplexNumber(a.Real - b.Real, a.Imaginary - b.Imaginary);

        public static ComplexNumber operator *(ComplexNumber a, ComplexNumber b)
            => new ComplexNumber(a.Real * b.Real - a.Imaginary * b.Imaginary, a.Real * b.Imaginary + a.Imaginary * b.Real);

        public static ComplexNumber operator /(ComplexNumber a, ComplexNumber b)
        {
            var denom = b.Real * b.Real + b.Imaginary * b.Imaginary;
            if (denom == 0) throw new DivideByZeroException();
            return new ComplexNumber(
                (a.Real * b.Real + a.Imaginary * b.Imaginary) / denom,
                (a.Imaginary * b.Real - a.Real * b.Imaginary) / denom);
        }

        public static ComplexNumber operator -(ComplexNumber a)
            => new ComplexNumber(-a.Real, -a.Imaginary);

        public static bool operator ==(ComplexNumber a, ComplexNumber b)
            => a.Equals(b);

        public static bool operator !=(ComplexNumber a, ComplexNumber b)
            => !a.Equals(b);

        public static implicit operator ComplexNumber(double value)
            => new ComplexNumber(value, 0);

        #endregion

        #region 数学运算

        /// <summary>
        /// 平方根
        /// </summary>
        public ComplexNumber Sqrt()
        {
            var m = Magnitude;
            var r = Math.Sqrt((m + Real) / 2);
            var i = Math.Sign(Imaginary) * Math.Sqrt((m - Real) / 2);
            return new ComplexNumber(r, i);
        }

        /// <summary>
        /// 幂运算
        /// </summary>
        public ComplexNumber Pow(double exponent)
        {
            var m = Math.Pow(Magnitude, exponent);
            var p = Phase * exponent;
            return FromPolarCoordinates(m, p);
        }

        /// <summary>
        /// 幂运算
        /// </summary>
        public ComplexNumber Pow(ComplexNumber exponent)
        {
            return (exponent * Log()).Exp();
        }

        /// <summary>
        /// 自然对数
        /// </summary>
        public ComplexNumber Log()
        {
            return new ComplexNumber(Math.Log(Magnitude), Phase);
        }

        /// <summary>
        /// 指数函数
        /// </summary>
        public ComplexNumber Exp()
        {
            return FromPolarCoordinates(Math.Exp(Real), Imaginary);
        }

        /// <summary>
        /// 正弦
        /// </summary>
        public ComplexNumber Sin()
        {
            return new ComplexNumber(
                Math.Sin(Real) * Math.Cosh(Imaginary),
                Math.Cos(Real) * Math.Sinh(Imaginary));
        }

        /// <summary>
        /// 余弦
        /// </summary>
        public ComplexNumber Cos()
        {
            return new ComplexNumber(
                Math.Cos(Real) * Math.Cosh(Imaginary),
                -Math.Sin(Real) * Math.Sinh(Imaginary));
        }

        /// <summary>
        /// 正切
        /// </summary>
        public ComplexNumber Tan()
        {
            return Sin() / Cos();
        }

        #endregion

        #region 接口实现

        public bool Equals(ComplexNumber other)
            => Real.Equals(other.Real) && Imaginary.Equals(other.Imaginary);

        public override bool Equals(object? obj)
            => obj is ComplexNumber other && Equals(other);

        public override int GetHashCode()
            => HashCode.Combine(Real, Imaginary);

        public string ToString(string? format, IFormatProvider? formatProvider)
            => $"({Real.ToString(format, formatProvider)}, {Imaginary.ToString(format, formatProvider)}i)";

        public override string ToString()
            => Imaginary >= 0 ? $"{Real}+{Imaginary}i" : $"{Real}{Imaginary}i";

        #endregion
    }

    /// <summary>
    /// 复数运算工具类
    /// </summary>
    public static class ComplexUtil
    {
        /// <summary>
        /// 创建复数
        /// </summary>
        public static ComplexNumber Create(double real, double imaginary)
            => new ComplexNumber(real, imaginary);

        /// <summary>
        /// 从极坐标创建复数
        /// </summary>
        public static ComplexNumber FromPolar(double magnitude, double phase)
            => ComplexNumber.FromPolarCoordinates(magnitude, phase);

        /// <summary>
        /// 求和
        /// </summary>
        public static ComplexNumber Sum(params ComplexNumber[] numbers)
        {
            var sum = ComplexNumber.Zero;
            foreach (var n in numbers)
                sum += n;
            return sum;
        }

        /// <summary>
        /// 求积
        /// </summary>
        public static ComplexNumber Product(params ComplexNumber[] numbers)
        {
            var product = ComplexNumber.One;
            foreach (var n in numbers)
                product *= n;
            return product;
        }

        /// <summary>
        /// 平均值
        /// </summary>
        public static ComplexNumber Average(params ComplexNumber[] numbers)
        {
            if (numbers.Length == 0) return ComplexNumber.Zero;
            return Sum(numbers) / numbers.Length;
        }

        /// <summary>
        /// 欧拉公式 e^(ix) = cos(x) + i*sin(x)
        /// </summary>
        public static ComplexNumber Euler(double x)
            => ComplexNumber.FromPolarCoordinates(1, x);

        /// <summary>
        /// 解析字符串
        /// </summary>
        public static ComplexNumber Parse(string s)
            => ComplexNumber.Parse(s);

        /// <summary>
        /// 尝试解析
        /// </summary>
        public static bool TryParse(string s, out ComplexNumber result)
            => ComplexNumber.TryParse(s, out result);
    }
}
