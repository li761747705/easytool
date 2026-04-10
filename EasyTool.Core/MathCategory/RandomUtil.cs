using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace EasyTool.MathCategory
{
    /// <summary>
    /// 随机数工具类
    /// 提供各种随机数生成功能，包括安全随机数
    /// </summary>
    public static class RandomUtil
    {
        private static readonly Random _random = new();
        private static readonly object _lock = new();
        private static readonly char[] _alphaChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz".ToCharArray();
        private static readonly char[] _numericChars = "0123456789".ToCharArray();
        private static readonly char[] _alphanumericChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();
        private static readonly char[] _hexChars = "0123456789ABCDEF".ToCharArray();

        /// <summary>
        /// 生成指定范围内的随机整数
        /// </summary>
        /// <param name="min">最小值（包含）</param>
        /// <param name="max">最大值（不包含）</param>
        /// <returns>随机整数</returns>
        public static int Next(int min, int max)
        {
            lock (_lock)
            {
                return _random.Next(min, max);
            }
        }

        /// <summary>
        /// 生成非负随机整数
        /// </summary>
        /// <returns>随机整数</returns>
        public static int Next()
        {
            lock (_lock)
            {
                return _random.Next();
            }
        }

        /// <summary>
        /// 生成指定范围内的随机浮点数
        /// </summary>
        /// <param name="min">最小值</param>
        /// <param name="max">最大值</param>
        /// <returns>随机浮点数</returns>
        public static double NextDouble(double min, double max)
        {
            lock (_lock)
            {
                return _random.NextDouble() * (max - min) + min;
            }
        }

        /// <summary>
        /// 生成随机布尔值
        /// </summary>
        /// <returns>随机布尔值</returns>
        public static bool NextBool()
        {
            lock (_lock)
            {
                return _random.Next(2) == 1;
            }
        }

        /// <summary>
        /// 生成随机字节数组
        /// </summary>
        /// <param name="length">长度</param>
        /// <returns>字节数组</returns>
        public static byte[] NextBytes(int length)
        {
            var bytes = new byte[length];
            lock (_lock)
            {
                _random.NextBytes(bytes);
            }
            return bytes;
        }

        /// <summary>
        /// 生成随机字母字符串
        /// </summary>
        /// <param name="length">长度</param>
        /// <returns>随机字符串</returns>
        public static string NextAlphaString(int length)
        {
            return NextString(length, _alphaChars);
        }

        /// <summary>
        /// 生成随机数字字符串
        /// </summary>
        /// <param name="length">长度</param>
        /// <returns>随机字符串</returns>
        public static string NextNumericString(int length)
        {
            return NextString(length, _numericChars);
        }

        /// <summary>
        /// 生成随机字母数字字符串
        /// </summary>
        /// <param name="length">长度</param>
        /// <returns>随机字符串</returns>
        public static string NextAlphanumericString(int length)
        {
            return NextString(length, _alphanumericChars);
        }

        /// <summary>
        /// 生成随机十六进制字符串
        /// </summary>
        /// <param name="length">长度</param>
        /// <returns>随机字符串</returns>
        public static string NextHexString(int length)
        {
            return NextString(length, _hexChars);
        }

        /// <summary>
        /// 使用指定字符生成随机字符串
        /// </summary>
        /// <param name="length">长度</param>
        /// <param name="chars">字符集</param>
        /// <returns>随机字符串</returns>
        public static string NextString(int length, char[] chars)
        {
            var result = new StringBuilder(length);
            lock (_lock)
            {
                for (int i = 0; i < length; i++)
                {
                    result.Append(chars[_random.Next(chars.Length)]);
                }
            }
            return result.ToString();
        }

        /// <summary>
        /// 使用指定字符生成随机字符串
        /// </summary>
        /// <param name="length">长度</param>
        /// <param name="chars">字符集</param>
        /// <returns>随机字符串</returns>
        public static string NextString(int length, string chars)
        {
            return NextString(length, chars.ToCharArray());
        }

        /// <summary>
        /// 从数组中随机选择一个元素
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="array">数组</param>
        /// <returns>随机元素</returns>
        public static T? NextItem<T>(T[] array)
        {
            if (array == null || array.Length == 0)
                return default;

            lock (_lock)
            {
                return array[_random.Next(array.Length)];
            }
        }

        /// <summary>
        /// 随机打乱数组
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="array">数组</param>
        /// <returns>打乱后的数组</returns>
        public static T[] Shuffle<T>(T[] array)
        {
            if (array == null || array.Length <= 1)
                return array;

            var result = (T[])array.Clone();
            lock (_lock)
            {
                for (int i = result.Length - 1; i > 0; i--)
                {
                    int j = _random.Next(i + 1);
                    (result[i], result[j]) = (result[j], result[i]);
                }
            }
            return result;
        }

        /// <summary>
        /// 生成安全随机整数
        /// </summary>
        /// <param name="min">最小值</param>
        /// <param name="max">最大值（不包含）</param>
        /// <returns>安全随机整数</returns>
        public static int NextSecure(int min, int max)
        {
            if (min >= max)
                throw new ArgumentException("max must be greater than min");

            var range = (long)max - min;
            var bytes = new byte[4];

            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            var randomValue = BitConverter.ToUInt32(bytes, 0);

            return (int)(min + (randomValue % range));
        }

        /// <summary>
        /// 生成安全随机字节数组
        /// </summary>
        /// <param name="length">长度</param>
        /// <returns>安全随机字节数组</returns>
        public static byte[] NextSecureBytes(int length)
        {
            var bytes = new byte[length];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return bytes;
        }

        /// <summary>
        /// 生成安全随机字符串
        /// </summary>
        /// <param name="length">长度</param>
        /// <param name="chars">字符集</param>
        /// <returns>安全随机字符串</returns>
        public static string NextSecureString(int length, string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789")
        {
            var result = new StringBuilder(length);
            var charArray = chars.ToCharArray();

            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[4];

            for (int i = 0; i < length; i++)
            {
                rng.GetBytes(bytes);
                var randomIndex = BitConverter.ToUInt32(bytes, 0) % (uint)charArray.Length;
                result.Append(charArray[randomIndex]);
            }

            return result.ToString();
        }

        /// <summary>
        /// 生成随机 GUID（不带连字符）
        /// </summary>
        /// <returns>随机 GUID 字符串</returns>
        public static string NextGuid()
        {
            return Guid.NewGuid().ToString("N");
        }

        /// <summary>
        /// 生成随机 UUID
        /// </summary>
        /// <returns>UUID 字符串</returns>
        public static string NextUuid()
        {
            return Guid.NewGuid().ToString();
        }

        /// <summary>
        /// 随机选择多个不重复的元素
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="array">数组</param>
        /// <param name="count">选择数量</param>
        /// <returns>随机选择的元素数组</returns>
        public static T[] NextItems<T>(T[] array, int count)
        {
            if (array == null || array.Length == 0)
                return Array.Empty<T>();

            if (count >= array.Length)
                return Shuffle(array);

            var shuffled = Shuffle(array);
            var result = new T[count];
            Array.Copy(shuffled, result, count);
            return result;
        }

        /// <summary>
        /// 根据权重随机选择
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="items">元素数组</param>
        /// <param name="weights">权重数组</param>
        /// <returns>随机选择的元素</returns>
        public static T? NextWeighted<T>(T[] items, int[] weights)
        {
            if (items == null || items.Length == 0)
                return default;

            if (weights == null || weights.Length != items.Length)
                throw new ArgumentException("Weights array must have the same length as items array");

            var totalWeight = 0;
            foreach (var w in weights)
            {
                if (w < 0)
                    throw new ArgumentException("Weights must be non-negative");
                totalWeight += w;
            }

            if (totalWeight == 0)
                return default;

            int randomValue;
            lock (_lock)
            {
                randomValue = _random.Next(totalWeight);
            }

            var currentSum = 0;
            for (int i = 0; i < items.Length; i++)
            {
                currentSum += weights[i];
                if (randomValue < currentSum)
                    return items[i];
            }

            return items[^1];
        }

        #region 向后兼容方法别名

        /// <summary>
        /// 生成随机整数（Next 的别名）
        /// </summary>
        public static int RandomInt(int min, int max) => Next(min, max);

        /// <summary>
        /// 生成随机整数（Next 的别名）
        /// </summary>
        public static int RandomInt() => Next();

        /// <summary>
        /// 从数组中随机选择一个元素（NextItem 的别名）
        /// </summary>
        public static T? GetRandomElement<T>(T[] array) => NextItem(array);

        /// <summary>
        /// 从列表中随机选择一个元素
        /// </summary>
        public static T? GetRandomElement<T>(IList<T> list)
        {
            if (list == null || list.Count == 0)
                return default;

            lock (_lock)
            {
                return list[_random.Next(list.Count)];
            }
        }

        /// <summary>
        /// 从集合中随机选择一个元素
        /// </summary>
        public static T? GetRandomElement<T>(IEnumerable<T> collection)
        {
            if (collection == null)
                return default;

            var list = collection.ToList();
            if (list.Count == 0)
                return default;

            return GetRandomElement(list);
        }

        /// <summary>
        /// 生成随机数字字符串（NextNumericString 的别名）
        /// </summary>
        public static string RandomDigitString(int length) => NextNumericString(length);

        /// <summary>
        /// 生成随机字符串（NextAlphanumericString 的别名）
        /// </summary>
        public static string RandomString(int length) => NextAlphanumericString(length);

        /// <summary>
        /// 生成随机字母字符串（NextAlphaString 的别名）
        /// </summary>
        public static string RandomAlphaString(int length) => NextAlphaString(length);

        /// <summary>
        /// 生成随机布尔值（NextBool 的别名）
        /// </summary>
        public static bool RandomBool() => NextBool();

        /// <summary>
        /// 生成随机日期时间
        /// </summary>
        /// <param name="minDate">最小日期</param>
        /// <param name="maxDate">最大日期</param>
        /// <returns>随机日期时间</returns>
        public static DateTime GetRandomDateTime(DateTime minDate, DateTime maxDate)
        {
            if (minDate >= maxDate)
                throw new ArgumentException("minDate must be less than maxDate");

            var range = (maxDate - minDate).Ticks;
            lock (_lock)
            {
                var ticks = (long)(_random.NextDouble() * range);
                return minDate.AddTicks(ticks);
            }
        }

        /// <summary>
        /// 生成随机日期时间（默认1970年至今）
        /// </summary>
        /// <returns>随机日期时间</returns>
        public static DateTime GetRandomDateTime()
        {
            return GetRandomDateTime(new DateTime(1970, 1, 1), DateTime.UtcNow);
        }

        /// <summary>
        /// 生成随机日期（不含时间）
        /// </summary>
        /// <param name="minDate">最小日期</param>
        /// <param name="maxDate">最大日期</param>
        /// <returns>随机日期</returns>
        public static DateTime GetRandomDate(DateTime minDate, DateTime maxDate)
        {
            return GetRandomDateTime(minDate, maxDate).Date;
        }

        #endregion
    }
}
