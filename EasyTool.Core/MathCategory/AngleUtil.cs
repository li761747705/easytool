using System;

namespace EasyTool.MathCategory
{
    /// <summary>
    /// 角度运算工具类
    /// </summary>
    public static class AngleUtil
    {
        /// <summary>
        /// 弧度转角度
        /// </summary>
        public static double RadiansToDegrees(double radians)
        {
            return radians * (180.0 / Math.PI);
        }

        /// <summary>
        /// 角度转弧度
        /// </summary>
        public static double DegreesToRadians(double degrees)
        {
            return degrees * (Math.PI / 180.0);
        }

        /// <summary>
        /// 角度转弧度
        /// </summary>
        public static Angle Radians(double radians)
        {
            return Angle.FromRadians(radians);
        }

        /// <summary>
        /// 角度转弧度
        /// </summary>
        public static Angle Degrees(double degrees)
        {
            return Angle.FromDegrees(degrees);
        }

        /// <summary>
        /// 规范化角度到 [0, 360) 范围
        /// </summary>
        public static double NormalizeDegrees(double degrees)
        {
            degrees %= 360;
            return degrees < 0 ? degrees + 360 : degrees;
        }

        /// <summary>
        /// 规范化弧度到 [0, 2π) 范围
        /// </summary>
        public static double NormalizeRadians(double radians)
        {
            radians %= (2 * Math.PI);
            return radians < 0 ? radians + (2 * Math.PI) : radians;
        }

        /// <summary>
        /// 角度加法
        /// </summary>
        public static double AddDegrees(double a, double b)
        {
            return NormalizeDegrees(a + b);
        }

        /// <summary>
        /// 角度减法
        /// </summary>
        public static double SubtractDegrees(double a, double b)
        {
            return NormalizeDegrees(a - b);
        }

        /// <summary>
        /// 计算两个角度的最小差值
        /// </summary>
        public static double MinimumAngleDifference(double a, double b)
        {
            var diff = NormalizeDegrees(a - b);
            return diff > 180 ? 360 - diff : diff;
        }

        /// <summary>
        /// 角度线性插值
        /// </summary>
        public static double LerpDegrees(double from, double to, double t)
        {
            var diff = to - from;
            if (diff > 180) diff -= 360;
            else if (diff < -180) diff += 360;
            return NormalizeDegrees(from + diff * t);
        }

        /// <summary>
        /// 度分秒转十进制度
        /// </summary>
        public static double DmsToDecimal(int degrees, int minutes, double seconds)
        {
            var sign = degrees < 0 ? -1 : 1;
            return sign * (Math.Abs(degrees) + minutes / 60.0 + seconds / 3600.0);
        }

        /// <summary>
        /// 十进制度转度分秒
        /// </summary>
        public static (int Degrees, int Minutes, double Seconds) DecimalToDms(double decimalDegrees)
        {
            var sign = decimalDegrees < 0 ? -1 : 1;
            decimalDegrees = Math.Abs(decimalDegrees);

            var degrees = (int)decimalDegrees;
            var minutes = (int)((decimalDegrees - degrees) * 60);
            var seconds = ((decimalDegrees - degrees) * 60 - minutes) * 60;

            return (sign * degrees, minutes, seconds);
        }

        /// <summary>
        /// 格式化度分秒
        /// </summary>
        public static string FormatDms(double decimalDegrees)
        {
            var (degrees, minutes, seconds) = DecimalToDms(decimalDegrees);
            return $"{degrees}°{minutes}'{seconds:F2}″";
        }

        /// <summary>
        /// 解析度分秒字符串
        /// </summary>
        public static double ParseDms(string dms)
        {
            var parts = dms.Split(new[] { '°', '\'', '″', '"' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
                throw new ArgumentException("无效的度分秒格式");

            var degrees = double.Parse(parts[0].Trim());
            var minutes = parts.Length > 1 ? double.Parse(parts[1].Trim()) : 0;
            var seconds = parts.Length > 2 ? double.Parse(parts[2].Trim()) : 0;

            return DmsToDecimal((int)degrees, (int)minutes, seconds);
        }

        #region 三角函数（角度版本）

        /// <summary>
        /// 正弦（角度）
        /// </summary>
        public static double Sin(double degrees)
        {
            return Math.Sin(DegreesToRadians(degrees));
        }

        /// <summary>
        /// 余弦（角度）
        /// </summary>
        public static double Cos(double degrees)
        {
            return Math.Cos(DegreesToRadians(degrees));
        }

        /// <summary>
        /// 正切（角度）
        /// </summary>
        public static double Tan(double degrees)
        {
            return Math.Tan(DegreesToRadians(degrees));
        }

        /// <summary>
        /// 反正弦（返回角度）
        /// </summary>
        public static double Asin(double value)
        {
            return RadiansToDegrees(Math.Asin(value));
        }

        /// <summary>
        /// 反余弦（返回角度）
        /// </summary>
        public static double Acos(double value)
        {
            return RadiansToDegrees(Math.Acos(value));
        }

        /// <summary>
        /// 反正切（返回角度）
        /// </summary>
        public static double Atan(double value)
        {
            return RadiansToDegrees(Math.Atan(value));
        }

        /// <summary>
        /// 反正切2（返回角度）
        /// </summary>
        public static double Atan2(double y, double x)
        {
            return RadiansToDegrees(Math.Atan2(y, x));
        }

        #endregion
    }

    /// <summary>
    /// 角度结构
    /// </summary>
    public readonly struct Angle : IEquatable<Angle>, IComparable<Angle>
    {
        private readonly double _degrees;

        private Angle(double degrees)
        {
            _degrees = AngleUtil.NormalizeDegrees(degrees);
        }

        /// <summary>
        /// 角度值
        /// </summary>
        public double Degrees => _degrees;

        /// <summary>
        /// 弧度值
        /// </summary>
        public double Radians => AngleUtil.DegreesToRadians(_degrees);

        /// <summary>
        /// 从度创建角度
        /// </summary>
        public static Angle FromDegrees(double degrees) => new Angle(degrees);

        /// <summary>
        /// 从弧度创建角度
        /// </summary>
        public static Angle FromRadians(double radians) => new Angle(AngleUtil.RadiansToDegrees(radians));

        /// <summary>
        /// 从度分秒创建角度
        /// </summary>
        public static Angle FromDms(int degrees, int minutes, double seconds)
            => new Angle(AngleUtil.DmsToDecimal(degrees, minutes, seconds));

        /// <summary>
        /// 零度
        /// </summary>
        public static Angle Zero => new Angle(0);

        /// <summary>
        /// 直角 (90°)
        /// </summary>
        public static Angle Right => new Angle(90);

        /// <summary>
        /// 平角 (180°)
        /// </summary>
        public static Angle Straight => new Angle(180);

        /// <summary>
        /// 周角 (360°)
        /// </summary>
        public static Angle Full => new Angle(360);

        #region 运算符

        public static Angle operator +(Angle a, Angle b) => new Angle(a._degrees + b._degrees);
        public static Angle operator -(Angle a, Angle b) => new Angle(a._degrees - b._degrees);
        public static Angle operator *(Angle a, double scalar) => new Angle(a._degrees * scalar);
        public static Angle operator *(double scalar, Angle a) => new Angle(a._degrees * scalar);
        public static Angle operator /(Angle a, double scalar) => new Angle(a._degrees / scalar);
        public static Angle operator -(Angle a) => new Angle(-a._degrees);
        public static bool operator ==(Angle a, Angle b) => a.Equals(b);
        public static bool operator !=(Angle a, Angle b) => !a.Equals(b);
        public static bool operator <(Angle a, Angle b) => a._degrees < b._degrees;
        public static bool operator >(Angle a, Angle b) => a._degrees > b._degrees;
        public static bool operator <=(Angle a, Angle b) => a._degrees <= b._degrees;
        public static bool operator >=(Angle a, Angle b) => a._degrees >= b._degrees;

        public static implicit operator double(Angle angle) => angle._degrees;

        #endregion

        #region 三角函数

        public double Sin() => AngleUtil.Sin(_degrees);
        public double Cos() => AngleUtil.Cos(_degrees);
        public double Tan() => AngleUtil.Tan(_degrees);

        #endregion

        #region 接口实现

        public bool Equals(Angle other) => Math.Abs(_degrees - other._degrees) < double.Epsilon;
        public override bool Equals(object? obj) => obj is Angle other && Equals(other);
        public override int GetHashCode() => _degrees.GetHashCode();
        public int CompareTo(Angle other) => _degrees.CompareTo(other._degrees);

        public override string ToString() => $"{_degrees:F2}°";

        public string ToString(string format)
        {
            if (format == "DMS")
            {
                var (degrees, minutes, seconds) = AngleUtil.DecimalToDms(_degrees);
                return $"{degrees}°{minutes}'{seconds:F2}″";
            }
            return $"{_degrees.ToString(format)}°";
        }

        #endregion
    }
}
