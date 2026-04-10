using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyTool.CollectionsCategory
{
    /// <summary>
    /// 数组操作工具类
    /// 对标 Hutool 的 ArrayUtil
    /// 提供数组的创建、判空、合并、查找等常用操作
    /// </summary>
    public static class ArrayUtil
    {
        #region 数组判空

        /// <summary>
        /// 判断数组是否为空
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="array">数组</param>
        /// <returns>是否为空</returns>
        public static bool IsEmpty<T>(T[]? array)
        {
            return array == null || array.Length == 0;
        }

        /// <summary>
        /// 判断数组是否不为空
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="array">数组</param>
        /// <returns>是否不为空</returns>
        public static bool IsNotEmpty<T>(T[]? array)
        {
            return !IsEmpty(array);
        }

        /// <summary>
        /// 获取数组长度
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="array">数组</param>
        /// <returns>长度</returns>
        public static int Length<T>(T[]? array)
        {
            return array?.Length ?? 0;
        }

        /// <summary>
        /// 判断数组中是否包含 null 元素
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="array">数组</param>
        /// <returns>是否包含 null</returns>
        public static bool HasNull<T>(T[]? array)
        {
            if (array == null)
                return true;

            return array.Any(item => item == null);
        }

        #endregion

        #region 数组创建

        /// <summary>
        /// 创建数组
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="elements">元素</param>
        /// <returns>数组</returns>
        public static T[] NewArray<T>(params T[] elements)
        {
            return elements ?? Array.Empty<T>();
        }

        /// <summary>
        /// 创建指定大小的数组
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="size">大小</param>
        /// <returns>数组</returns>
        public static T[] NewArray<T>(int size)
        {
            return new T[size];
        }

        /// <summary>
        /// 创建指定大小的数组（填充默认值）
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="size">大小</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>数组</returns>
        public static T[] NewArray<T>(int size, T defaultValue)
        {
            var array = new T[size];
            for (int i = 0; i < size; i++)
            {
                array[i] = defaultValue;
            }
            return array;
        }

        /// <summary>
        /// 创建指定大小的数组（填充工厂函数值）
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="size">大小</param>
        /// <param name="factory">工厂函数</param>
        /// <returns>数组</returns>
        public static T[] NewArray<T>(int size, Func<int, T> factory)
        {
            if (factory == null)
                return new T[size];

            var array = new T[size];
            for (int i = 0; i < size; i++)
            {
                array[i] = factory(i);
            }
            return array;
        }

        /// <summary>
        /// 创建范围数组
        /// </summary>
        /// <param name="start">起始值</param>
        /// <param name="count">数量</param>
        /// <returns>数组</returns>
        public static int[] Range(int start, int count)
        {
            var array = new int[count];
            for (int i = 0; i < count; i++)
            {
                array[i] = start + i;
            }
            return array;
        }

        #endregion

        #region 数组合并

        /// <summary>
        /// 合并多个数组
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="arrays">数组</param>
        /// <returns>合并后的数组</returns>
        public static T[] Merge<T>(params T[][] arrays)
        {
            if (arrays == null || arrays.Length == 0)
                return Array.Empty<T>();

            var totalLength = arrays.Sum(a => a?.Length ?? 0);
            var result = new T[totalLength];
            int offset = 0;

            foreach (var array in arrays)
            {
                if (array != null && array.Length > 0)
                {
                    Array.Copy(array, 0, result, offset, array.Length);
                    offset += array.Length;
                }
            }

            return result;
        }

        /// <summary>
        /// 合并两个数组
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="first">第一个数组</param>
        /// <param name="second">第二个数组</param>
        /// <returns>合并后的数组</returns>
        public static T[] Merge<T>(T[]? first, T[]? second)
        {
            var firstLength = first?.Length ?? 0;
            var secondLength = second?.Length ?? 0;

            if (firstLength == 0 && secondLength == 0)
                return Array.Empty<T>();

            var result = new T[firstLength + secondLength];

            if (first != null && firstLength > 0)
                Array.Copy(first, 0, result, 0, firstLength);

            if (second != null && secondLength > 0)
                Array.Copy(second, 0, result, firstLength, secondLength);

            return result;
        }

        #endregion

        #region 数组操作

        /// <summary>
        /// 反转数组
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="array">数组</param>
        /// <returns>反转后的数组</returns>
        public static T[] Reverse<T>(T[]? array)
        {
            if (array == null)
                return Array.Empty<T>();

            var result = new T[array.Length];
            Array.Copy(array, result, array.Length);
            Array.Reverse(result);
            return result;
        }

        /// <summary>
        /// 反转数组（原地）
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="array">数组</param>
        public static void ReverseInPlace<T>(T[]? array)
        {
            if (array != null)
            {
                Array.Reverse(array);
            }
        }

        /// <summary>
        /// 随机打乱数组
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="array">数组</param>
        /// <returns>打乱后的数组</returns>
        public static T[] Shuffle<T>(T[]? array)
        {
            if (array == null)
                return Array.Empty<T>();

            var result = new T[array.Length];
            Array.Copy(array, result, array.Length);

            var random = new Random();
            int n = result.Length;

            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                (result[k], result[n]) = (result[n], result[k]);
            }

            return result;
        }

        /// <summary>
        /// 去重
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="array">数组</param>
        /// <returns>去重后的数组</returns>
        public static T[] Distinct<T>(T[]? array)
        {
            if (array == null)
                return Array.Empty<T>();

            return array.Distinct().ToArray();
        }

        /// <summary>
        /// 排序
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="array">数组</param>
        /// <returns>排序后的数组</returns>
        public static T[] Sort<T>(T[]? array) where T : IComparable<T>
        {
            if (array == null)
                return Array.Empty<T>();

            var result = new T[array.Length];
            Array.Copy(array, result, array.Length);
            Array.Sort(result);
            return result;
        }

        /// <summary>
        /// 截取子数组
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="array">数组</param>
        /// <param name="start">起始索引</param>
        /// <param name="length">长度</param>
        /// <returns>子数组</returns>
        public static T[] Sub<T>(T[]? array, int start, int length)
        {
            if (array == null || start < 0 || length <= 0)
                return Array.Empty<T>();

            if (start >= array.Length)
                return Array.Empty<T>();

            length = Math.Min(length, array.Length - start);
            var result = new T[length];
            Array.Copy(array, start, result, 0, length);
            return result;
        }

        #endregion

        #region 数组查找

        /// <summary>
        /// 获取指定索引的元素（安全）
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="array">数组</param>
        /// <param name="index">索引</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>元素</returns>
        public static T? Get<T>(T[]? array, int index, T? defaultValue = default)
        {
            if (array == null || index < 0 || index >= array.Length)
                return defaultValue;

            return array[index];
        }

        /// <summary>
        /// 获取第一个元素
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="array">数组</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>第一个元素</returns>
        public static T? First<T>(T[]? array, T? defaultValue = default)
        {
            if (IsEmpty(array))
                return defaultValue;

            return array![0];
        }

        /// <summary>
        /// 获取最后一个元素
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="array">数组</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>最后一个元素</returns>
        public static T? Last<T>(T[]? array, T? defaultValue = default)
        {
            if (IsEmpty(array))
                return defaultValue;

            return array![array.Length - 1];
        }

        /// <summary>
        /// 查找元素的索引
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="array">数组</param>
        /// <param name="item">元素</param>
        /// <returns>索引（未找到返回 -1）</returns>
        public static int IndexOf<T>(T[]? array, T item)
        {
            if (array == null)
                return -1;

            return Array.IndexOf(array, item);
        }

        /// <summary>
        /// 查找最后一个匹配元素的索引
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="array">数组</param>
        /// <param name="item">元素</param>
        /// <returns>索引（未找到返回 -1）</returns>
        public static int LastIndexOf<T>(T[]? array, T item)
        {
            if (array == null)
                return -1;

            return Array.LastIndexOf(array, item);
        }

        /// <summary>
        /// 查找满足条件的元素索引
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="array">数组</param>
        /// <param name="predicate">条件</param>
        /// <returns>索引（未找到返回 -1）</returns>
        public static int FindIndex<T>(T[]? array, Func<T, bool> predicate)
        {
            if (array == null || predicate == null)
                return -1;

            for (int i = 0; i < array.Length; i++)
            {
                if (predicate(array[i]))
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// 判断是否包含元素
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="array">数组</param>
        /// <param name="item">元素</param>
        /// <returns>是否包含</returns>
        public static bool Contains<T>(T[]? array, T item)
        {
            return IndexOf(array, item) >= 0;
        }

        /// <summary>
        /// 随机获取一个元素
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="array">数组</param>
        /// <returns>随机元素</returns>
        public static T? Random<T>(T[]? array)
        {
            if (IsEmpty(array))
                return default;

            var random = new Random();
            return array![random.Next(array.Length)];
        }

        #endregion

        #region 数组转换

        /// <summary>
        /// 数组转列表
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="array">数组</param>
        /// <returns>列表</returns>
        public static List<T> ToList<T>(T[]? array)
        {
            if (array == null)
                return new List<T>();

            return new List<T>(array);
        }

        /// <summary>
        /// 映射数组元素
        /// </summary>
        /// <typeparam name="T">原类型</typeparam>
        /// <typeparam name="TResult">结果类型</typeparam>
        /// <param name="array">数组</param>
        /// <param name="selector">选择器</param>
        /// <returns>新数组</returns>
        public static TResult[] Map<T, TResult>(T[]? array, Func<T, TResult> selector)
        {
            if (array == null || selector == null)
                return Array.Empty<TResult>();

            return array.Select(selector).ToArray();
        }

        /// <summary>
        /// 过滤数组元素
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="array">数组</param>
        /// <param name="predicate">条件</param>
        /// <returns>新数组</returns>
        public static T[] Filter<T>(T[]? array, Func<T, bool> predicate)
        {
            if (array == null || predicate == null)
                return Array.Empty<T>();

            return array.Where(predicate).ToArray();
        }

        #endregion

        #region 数组填充

        /// <summary>
        /// 填充数组
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="array">数组</param>
        /// <param name="value">值</param>
        public static void Fill<T>(T[]? array, T value)
        {
            if (array == null)
                return;

            for (int i = 0; i < array.Length; i++)
            {
                array[i] = value;
            }
        }

        /// <summary>
        /// 填充数组（指定范围）
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="array">数组</param>
        /// <param name="value">值</param>
        /// <param name="start">起始索引</param>
        /// <param name="length">长度</param>
        public static void Fill<T>(T[]? array, T value, int start, int length)
        {
            if (array == null || start < 0)
                return;

            int end = Math.Min(start + length, array.Length);
            for (int i = start; i < end; i++)
            {
                array[i] = value;
            }
        }

        #endregion

        #region 数组比较

        /// <summary>
        /// 比较两个数组是否相等
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="first">第一个数组</param>
        /// <param name="second">第二个数组</param>
        /// <returns>是否相等</returns>
        public static bool Equals<T>(T[]? first, T[]? second)
        {
            if (ReferenceEquals(first, second))
                return true;

            if (first == null || second == null)
                return false;

            if (first.Length != second.Length)
                return false;

            for (int i = 0; i < first.Length; i++)
            {
                if (!EqualityComparer<T>.Default.Equals(first[i], second[i]))
                    return false;
            }

            return true;
        }

        #endregion
    }
}