using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace EasyTool.MathCategory
{
    public static class RandomUtil
    {
#if NET6_0_OR_GREATER
        // .NET 6+ 使用 Random.Shared，线程安全且高性能
        private static Random SharedRandom => Random.Shared;
#else
        // .NET Standard 2.1 使用 ThreadLocal 确保线程安全
        private static readonly ThreadLocal<Random> ThreadLocalRandom = new(() => new Random(Guid.NewGuid().GetHashCode()));
        private static Random SharedRandom => ThreadLocalRandom.Value!;
#endif

        /// <summary>
        /// 生成指定范围内的随机整数
        /// <para>注意：返回值为 [min, max) 区间，即包含 min 但不包含 max</para>
        /// </summary>
        /// <param name="min">随机整数的最小值（包含）</param>
        /// <param name="max">随机整数的最大值（不包含）</param>
        /// <returns>生成的随机整数</returns>
        public static int RandomInt(int min, int max)
        {
            return SharedRandom.Next(min, max);
        }

        /// <summary>
        /// 生成指定位数的随机数字字符串
        /// <para>仅包含数字 0-9</para>
        /// </summary>
        /// <param name="length">生成的随机数字字符串的长度</param>
        /// <returns>生成的随机数字字符串</returns>
        public static string RandomDigitString(int length)
        {
            var sb = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                sb.Append(SharedRandom.Next(10));
            }
            return sb.ToString();
        }

        /// <summary>
        /// 生成指定位数的随机字母数字字符串
        /// <para>包含大小写字母 A-Z, a-z 和数字 0-9</para>
        /// </summary>
        /// <param name="length">生成的随机字母数字字符串的长度</param>
        /// <returns>生成的随机字母数字字符串</returns>
        public static string RandomAlphanumericString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var sb = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                sb.Append(chars[SharedRandom.Next(chars.Length)]);
            }
            return sb.ToString();
        }

        /// <summary>
        /// 生成指定长度的随机字母字符串
        /// </summary>
        /// <param name="length">生成的随机字母字符串的长度</param>
        /// <returns>生成的随机字母字符串</returns>
        public static string RandomLetterString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            var sb = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                sb.Append(chars[SharedRandom.Next(chars.Length)]);
            }
            return sb.ToString();
        }

        /// <summary>
        /// 生成随机的布尔值
        /// </summary>
        /// <returns>生成的随机布尔值</returns>
        public static bool RandomBool()
        {
            return SharedRandom.Next(2) == 0;
        }

        /// <summary>
        /// 生成指定长度的随机数组
        /// </summary>
        /// <param name="length">生成的随机数组的长度</param>
        /// <returns>生成的随机数组</returns>
        public static int[] RandomIntArray(int length)
        {
            int[] result = new int[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = SharedRandom.Next();
            }
            return result;
        }

        /// <summary>
        /// 生成指定长度的随机双精度浮点数数组
        /// </summary>
        /// <param name="length">生成的随机数组的长度</param>
        /// <returns>生成的随机双精度浮点数数组</returns>
        public static double[] RandomDoubleArray(int length)
        {
            double[] result = new double[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = SharedRandom.NextDouble();
            }
            return result;
        }

        /// <summary>
        /// 生成指定长度的随机字符串数组
        /// </summary>
        /// <param name="length">生成的随机数组的长度</param>
        /// <param name="strLength">每个随机字符串的长度</param>
        /// <returns>生成的随机字符串数组</returns>
        public static string[] RandomStringArray(int length, int strLength)
        {
            string[] result = new string[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = RandomAlphanumericString(strLength);
            }
            return result;
        }

        /// <summary>
        /// 生成随机日期
        /// </summary>
        /// <param name="startDate">随机日期的最早时间</param>
        /// <param name="endDate">随机日期的最晚时间</param>
        /// <returns>生成的随机日期</returns>
        public static DateTime RandomDate(DateTime startDate, DateTime endDate)
        {
            TimeSpan timeSpan = endDate - startDate;
            TimeSpan newSpan = new TimeSpan(0, 0, SharedRandom.Next(0, (int)timeSpan.TotalSeconds));
            return startDate + newSpan;
        }

        /// <summary>
        /// 生成随机枚举值
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <returns>生成的随机枚举值</returns>
        public static T RandomEnumValue<T>()
        {
            Array values = Enum.GetValues(typeof(T));
            return (T)values.GetValue(SharedRandom.Next(values.Length));
        }

        /// <summary>
        /// 获取一个指定范围内的随机整数
        /// <para>注意：返回值为 [minValue, maxValue] 闭区间，即同时包含最小值和最大值</para>
        /// <para>与 RandomInt 方法的区别：RandomInt 使用左闭右开区间 [min, max)，本方法使用闭区间</para>
        /// </summary>
        /// <param name="minValue">最小值（包含）</param>
        /// <param name="maxValue">最大值（包含）</param>
        /// <returns>随机整数</returns>
        public static int GetRandomInt(int minValue, int maxValue)
        {
            return SharedRandom.Next(minValue, maxValue + 1);
        }

        /// <summary>
        /// 获取一个指定范围内的随机双精度浮点数
        /// </summary>
        /// <param name="minValue">最小值</param>
        /// <param name="maxValue">最大值</param>
        /// <returns>随机双精度浮点数</returns>
        public static double GetRandomDouble(double minValue, double maxValue)
        {
            return minValue + (maxValue - minValue) * SharedRandom.NextDouble();
        }

        /// <summary>
        /// 获取一个指定范围内的随机日期时间
        /// </summary>
        /// <param name="minValue">最小值</param>
        /// <param name="maxValue">最大值</param>
        /// <returns>随机日期时间</returns>
        public static DateTime GetRandomDateTime(DateTime minValue, DateTime maxValue)
        {
            TimeSpan timeSpan = maxValue - minValue;
            double totalSeconds = timeSpan.TotalSeconds;
            int randomSeconds = GetRandomInt(0, (int)totalSeconds);
            return minValue.AddSeconds(randomSeconds);
        }

        /// <summary>
        /// 从给定的集合中随机选取一个元素
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="source">集合</param>
        /// <returns>随机选取的元素</returns>
        public static T GetRandomElement<T>(IEnumerable<T> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            int count = source.Count();
            if (count == 0)
            {
                throw new ArgumentException("集合中必须至少有一个元素", nameof(source));
            }

            int index = GetRandomInt(0, count - 1);
            return source.ElementAt(index);
        }

        /// <summary>
        /// 生成指定长度的随机数字字符串
        /// </summary>
        /// <param name="length">字符串长度</param>
        /// <returns>随机数字字符串</returns>
        [Obsolete("请使用 RandomDigitString 替代，两者功能相同")]
        public static string RandomNumberString(int length)
        {
            var sb = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                sb.Append(SharedRandom.Next(10));
            }
            return sb.ToString();
        }

        /// <summary>
        /// 生成指定长度的随机字母数字字符串
        /// </summary>
        /// <param name="length">字符串长度</param>
        /// <returns>随机字母数字字符串</returns>
        [Obsolete("请使用 RandomAlphanumericString 替代，该方法实现较复杂且性能较差")]
        public static string RandomString(int length)
        {
            var sb = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                int code = SharedRandom.Next(36) + 48;
                if (code >= 58 && code <= 64)
                {
                    code += 7;
                }
                if (code >= 91 && code <= 96)
                {
                    code += 6;
                }
                sb.Append(Convert.ToChar(code));
            }
            return sb.ToString();
        }
    }
}