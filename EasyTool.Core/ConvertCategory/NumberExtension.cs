using System;

namespace EasyTool.ConvertCategory
{
    /// <summary>
    /// 数字类型扩展方法
    /// </summary>
    public static class NumberExtension
    {
        #region 整数扩展

        /// <summary>
        /// 判断整数是否为偶数
        /// </summary>
        public static bool IsEven(this int value)
        {
            return value % 2 == 0;
        }

        /// <summary>
        /// 判断整数是否为奇数
        /// </summary>
        public static bool IsOdd(this int value)
        {
            return value % 2 != 0;
        }

        /// <summary>
        /// 判断长整数是否为偶数
        /// </summary>
        public static bool IsEven(this long value)
        {
            return value % 2 == 0;
        }

        /// <summary>
        /// 判断长整数是否为奇数
        /// </summary>
        public static bool IsOdd(this long value)
        {
            return value % 2 != 0;
        }

        /// <summary>
        /// 判断短整数是否为偶数
        /// </summary>
        public static bool IsEven(this short value)
        {
            return value % 2 == 0;
        }

        /// <summary>
        /// 判断短整数是否为奇数
        /// </summary>
        public static bool IsOdd(this short value)
        {
            return value % 2 != 0;
        }

        #endregion

        #region 浮点数扩展

        /// <summary>
        /// 判断浮点数是否在指定范围内（包含边界）
        /// </summary>
        public static bool IsBetween(this float value, float min, float max)
        {
            return value >= min && value <= max;
        }

        /// <summary>
        /// 判断双精度浮点数是否在指定范围内（包含边界）
        /// </summary>
        public static bool IsBetween(this double value, double min, double max)
        {
            return value >= min && value <= max;
        }

        /// <summary>
        /// 判断小数是否在指定范围内（包含边界）
        /// </summary>
        public static bool IsBetween(this decimal value, decimal min, decimal max)
        {
            return value >= min && value <= max;
        }

        /// <summary>
        /// 限制浮点数在指定范围内
        /// </summary>
        public static float Clamp(this float value, float min, float max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        /// <summary>
        /// 限制双精度浮点数在指定范围内
        /// </summary>
        public static double Clamp(this double value, double min, double max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        /// <summary>
        /// 限制小数在指定范围内
        /// </summary>
        public static decimal Clamp(this decimal value, decimal min, decimal max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        /// <summary>
        /// 限制整数在指定范围内
        /// </summary>
        public static int Clamp(this int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        /// <summary>
        /// 限制长整数在指定范围内
        /// </summary>
        public static long Clamp(this long value, long min, long max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        #endregion

        #region 百分比转换

        /// <summary>
        /// 将小数转换为百分比字符串
        /// </summary>
        /// <param name="value">数值</param>
        /// <param name="decimals">小数位数，默认2位</param>
        public static string ToPercentage(this double value, int decimals = 2)
        {
            return (value * 100).ToString($"F{decimals}") + "%";
        }

        /// <summary>
        /// 将小数转换为百分比字符串
        /// </summary>
        /// <param name="value">数值</param>
        /// <param name="decimals">小数位数，默认2位</param>
        public static string ToPercentage(this float value, int decimals = 2)
        {
            return (value * 100).ToString($"F{decimals}") + "%";
        }

        /// <summary>
        /// 将小数转换为百分比字符串
        /// </summary>
        /// <param name="value">数值</param>
        /// <param name="decimals">小数位数，默认2位</param>
        public static string ToPercentage(this decimal value, int decimals = 2)
        {
            return (value * 100).ToString($"F{decimals}") + "%";
        }

        #endregion

        #region 文件大小转换

        /// <summary>
        /// 将字节数转换为人类可读的文件大小格式
        /// </summary>
        /// <param name="bytes">字节数</param>
        public static string ToFileSize(this long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }

        /// <summary>
        /// 将字节数转换为人类可读的文件大小格式
        /// </summary>
        /// <param name="bytes">字节数</param>
        public static string ToFileSize(this int bytes)
        {
            return ((long)bytes).ToFileSize();
        }

        #endregion

        #region 时间转换

        /// <summary>
        /// 将秒数转换为时间跨度
        /// </summary>
        public static TimeSpan ToTimeSpan(this double seconds)
        {
            return TimeSpan.FromSeconds(seconds);
        }

        /// <summary>
        /// 将秒数转换为时间跨度
        /// </summary>
        public static TimeSpan ToTimeSpan(this int seconds)
        {
            return TimeSpan.FromSeconds(seconds);
        }

        /// <summary>
        /// 将毫秒数转换为时间跨度
        /// </summary>
        public static TimeSpan ToTimeSpanFromMilliseconds(this long milliseconds)
        {
            return TimeSpan.FromMilliseconds(milliseconds);
        }

        /// <summary>
        /// 将毫秒数转换为时间跨度
        /// </summary>
        public static TimeSpan ToTimeSpanFromMilliseconds(this int milliseconds)
        {
            return TimeSpan.FromMilliseconds(milliseconds);
        }

        #endregion

        #region 数学运算

        /// <summary>
        /// 计算数值的平方
        /// </summary>
        public static int Square(this int value)
        {
            return value * value;
        }

        /// <summary>
        /// 计算数值的平方
        /// </summary>
        public static long Square(this long value)
        {
            return value * value;
        }

        /// <summary>
        /// 计算数值的平方
        /// </summary>
        public static double Square(this double value)
        {
            return value * value;
        }

        /// <summary>
        /// 计算数值的立方
        /// </summary>
        public static int Cube(this int value)
        {
            return value * value * value;
        }

        /// <summary>
        /// 计算数值的立方
        /// </summary>
        public static long Cube(this long value)
        {
            return value * value * value;
        }

        /// <summary>
        /// 计算数值的立方
        /// </summary>
        public static double Cube(this double value)
        {
            return value * value * value;
        }


        #endregion

        #region 数值判断


        #endregion

        #region 数值格式化

        /// <summary>
        /// 将数字格式化为带千分位的字符串
        /// </summary>
        public static string ToThousandsSeparator(this int value)
        {
            return value.ToString("#,##0");
        }

        /// <summary>
        /// 将数字格式化为带千分位的字符串
        /// </summary>
        public static string ToThousandsSeparator(this long value)
        {
            return value.ToString("#,##0");
        }

        /// <summary>
        /// 将数字格式化为带千分位的字符串
        /// </summary>
        /// <param name="decimals">小数位数</param>
        public static string ToThousandsSeparator(this double value, int decimals = 2)
        {
            return value.ToString($"#,##0.{new string('0', decimals)}");
        }

        /// <summary>
        /// 将数字格式化为带千分位的字符串
        /// </summary>
        /// <param name="decimals">小数位数</param>
        public static string ToThousandsSeparator(this float value, int decimals = 2)
        {
            return value.ToString($"#,##0.{new string('0', decimals)}");
        }

        /// <summary>
        /// 将数字格式化为带千分位的字符串
        /// </summary>
        /// <param name="decimals">小数位数</param>
        public static string ToThousandsSeparator(this decimal value, int decimals = 2)
        {
            return value.ToString($"#,##0.{new string('0', decimals)}");
        }

        /// <summary>
        /// 将数字转换为中文大写金额
        /// </summary>
        public static string ToChineseMoney(this decimal value)
        {
            if (value == 0)
                return "零元整";

            string[] digits = { "零", "壹", "贰", "叁", "肆", "伍", "陆", "柒", "捌", "玖" };
            string[] units = { "", "拾", "佰", "仟", "万", "拾", "佰", "仟", "亿", "拾", "佰", "仟" };

            string result = string.Empty;
            bool hasYuan = false;
            bool hasJiao = false;
            bool hasFen = false;

            // 处理整数部分
            long integerPart = (long)value;
            if (integerPart > 0)
            {
                string integerStr = integerPart.ToString();
                int length = integerStr.Length;

                for (int i = 0; i < length; i++)
                {
                    int digit = integerStr[i] - '0';
                    int pos = length - i - 1;

                    if (digit != 0)
                    {
                        result += digits[digit] + units[pos];
                        hasYuan = true;
                    }
                    else if (result.Length > 0 && result[result.Length - 1] != '零')
                    {
                        result += '零';
                    }
                }

                if (hasYuan)
                    result += '元';
            }

            // 处理小数部分
            decimal decimalPart = value - integerPart;
            int jiao = (int)(decimalPart * 10);
            int fen = (int)(decimalPart * 100) % 10;

            if (jiao > 0)
            {
                result += digits[jiao] + "角";
                hasJiao = true;
            }

            if (fen > 0)
            {
                result += digits[fen] + "分";
                hasFen = true;
            }

            if (!hasJiao && !hasFen && hasYuan)
                result += "整";

            // 清理多余的零
            result = result.Replace("零零", "零");
            if (result.EndsWith("零元"))
                result = result.Substring(0, result.Length - 2) + "元";

            return result;
        }

        #endregion
    }
}
