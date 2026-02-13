using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EasyTool.Extension
{
    /// <summary>
    /// 数组扩展方法
    /// </summary>
    public static class ArrayExtension
    {
        #region 空值判断

        /// <summary>
        /// 判断数组是否为空或 null
        /// </summary>
        public static bool IsEmpty<T>(this T[]? array)
        {
            return array == null || array.Length == 0;
        }

        /// <summary>
        /// 判断数组是否非空
        /// </summary>
        public static bool IsNotEmpty<T>(this T[]? array)
        {
            return array != null && array.Length > 0;
        }

        #endregion

        #region 数组操作

        /// <summary>
        /// 数组排序
        /// </summary>
        public static T[] Sort<T>(this T[]? array) where T : IComparable<T>
        {
            if (array.IsEmpty())
            {
                throw new ArgumentException("Array is empty.");
            }

            T[] sortedArray = new T[array.Length];
            array.CopyTo(sortedArray, 0);
            Array.Sort(sortedArray);

            return sortedArray;
        }

        /// <summary>
        /// 数组反转
        /// </summary>
        public static T[] Reverse<T>(this T[]? array)
        {
            if (array.IsEmpty())
            {
                throw new ArgumentException("Array is empty.");
            }

            T[] reversedArray = new T[array.Length];
            array.CopyTo(reversedArray, 0);
            Array.Reverse(reversedArray);

            return reversedArray;
        }

        /// <summary>
        /// 判断两个数组是否完全相等
        /// </summary>
        public static bool EqualsTo<T>(this T[]? array, T[]? other)
        {
            if (array.IsEmpty() && other.IsEmpty())
            {
                return true;
            }

            if (array.IsEmpty() || other.IsEmpty())
            {
                return false;
            }

            if (array!.Length != other!.Length)
            {
                return false;
            }

            for (int i = 0; i < array.Length; i++)
            {
                if (!array[i].Equals(other[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 合并两个数组
        /// </summary>
        public static T[] Concat<T>(this T[]? array, T[]? other)
        {
            if (array.IsEmpty())
            {
                return other ?? Array.Empty<T>();
            }

            if (other.IsEmpty())
            {
                return array;
            }

            T[] result = new T[array.Length + other.Length];
            array.CopyTo(result, 0);
            other.CopyTo(result, array.Length);

            return result;
        }

        /// <summary>
        /// 随机打乱数组顺序（Fisher-Yates 洗牌算法）
        /// </summary>
        public static T[]? Shuffle<T>(this T[]? array)
        {
            if (array == null || array.Length < 2)
                return array;

            var random = new Random();
            var result = (T[])array.Clone();

            for (int i = result.Length - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (result[i], result[j]) = (result[j], result[i]);
            }

            return result;
        }

        /// <summary>
        /// 将数组分割成指定大小的块
        /// </summary>
        /// <param name="array">原始数组</param>
        /// <param name="chunkSize">每块的大小</param>
        public static IEnumerable<T[]> Chunk<T>(this T[]? array, int chunkSize)
        {
            if (array == null)
                yield break;

            if (chunkSize <= 0)
                throw new ArgumentException("chunkSize must be greater than 0", nameof(chunkSize));

            for (int i = 0; i < array.Length; i += chunkSize)
            {
                int remaining = array.Length - i;
                int size = Math.Min(chunkSize, remaining);
                var chunk = new T[size];
                Array.Copy(array, i, chunk, 0, size);
                yield return chunk;
            }
        }

        public static T[]? Distinct<T>(this T[]? array)
        {
            if (array == null)
                return null;

            return array.Distinct().ToArray();
        }

        /// <summary>
        /// 按指定键清除数组中的重复元素
        /// </summary>
        public static T[]? DistinctBy<T, TKey>(this T[]? array, Func<T, TKey> keySelector)
        {
            if (array == null)
                return null;

            return array.GroupBy(keySelector).Select(g => g.First()).ToArray();
        }

        /// <summary>
        /// 将数组元素拼接成字符串（支持格式化）
        /// </summary>
        /// <param name="array">数组</param>
        /// <param name="separator">分隔符</param>
        /// <param name="format">格式化字符串</param>
        public static string JoinFormat<T>(this T[]? array, string separator, string format)
        {
            if (array == null || array.Length == 0)
                return string.Empty;

            var formatted = array.Select(item => string.Format(format, item));
            return string.Join(separator, formatted);
        }

        #endregion

        #region 数组查找


        /// <summary>
        /// 查找数组中满足条件的所有元素的索引
        /// </summary>
        public static int[] FindAllIndexes<T>(this T[]? array, Func<T, bool> predicate)
        {
            if (array == null)
                return Array.Empty<int>();

            var indexes = new List<int>();
            for (int i = 0; i < array.Length; i++)
            {
                if (predicate(array[i]))
                {
                    indexes.Add(i);
                }
            }
            return indexes.ToArray();
        }

        /// <summary>
        /// 判断数组是否包含指定元素（使用自定义比较器）
        /// </summary>
        public static bool Contains<T>(this T[]? array, T value, IEqualityComparer<T> comparer)
        {
            if (array == null)
                return false;

            return Array.Exists(array, item => comparer.Equals(item, value));
        }

        #endregion

        #region 数组转换


        /// <summary>
        /// 将二维数组展平为一维数组
        /// </summary>
        public static T[]? Flatten<T>(this T[,]? array)
        {
            if (array == null)
                return null;

            int width = array.GetLength(0);
            int height = array.GetLength(1);
            var result = new T[width * height];

            int index = 0;
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    result[index++] = array[i, j];
                }
            }

            return result;
        }

        #endregion

        #region 数组切片

        /// <summary>
        /// 获取数组中指定范围的元素
        /// </summary>
        /// <param name="array">数组</param>
        /// <param name="startIndex">起始索引</param>
        /// <param name="length">长度</param>
        public static T[]? Slice<T>(this T[]? array, int startIndex, int length)
        {
            if (array == null)
                return null;

            if (startIndex < 0 || startIndex >= array.Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            if (length < 0 || startIndex + length > array.Length)
                throw new ArgumentOutOfRangeException(nameof(length));

            var result = new T[length];
            Array.Copy(array, startIndex, result, 0, length);
            return result;
        }

        /// <summary>
        /// 获取数组从指定索引开始到末尾的元素
        /// </summary>
        public static T[]? Slice<T>(this T[]? array, int startIndex)
        {
            if (array == null)
                return null;

            if (startIndex < 0)
                startIndex = 0;

            if (startIndex >= array.Length)
                return Array.Empty<T>();

            int length = array.Length - startIndex;
            return Slice(array, startIndex, length);
        }

        #endregion

        #region 数组合并

        /// <summary>
        /// 合并多个数组
        /// </summary>
        public static T[] Merge<T>(params T[][]? arrays)
        {
            if (arrays == null || arrays.Length == 0)
                return Array.Empty<T>();

            int totalLength = 0;
            foreach (var array in arrays)
            {
                if (array != null)
                    totalLength += array.Length;
            }

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
        /// 在数组开头添加元素
        /// </summary>
        public static T[] Prepend<T>(this T[]? array, params T[]? items)
        {
            if (array == null)
                return items ?? Array.Empty<T>();

            if (items == null || items.Length == 0)
                return array;

            var result = new T[array.Length + items.Length];
            Array.Copy(items, 0, result, 0, items.Length);
            Array.Copy(array, 0, result, items.Length, array.Length);
            return result;
        }

        /// <summary>
        /// 在数组末尾添加元素
        /// </summary>
        public static T[] Append<T>(this T[]? array, params T[]? items)
        {
            if (array == null)
                return items ?? Array.Empty<T>();

            if (items == null || items.Length == 0)
                return array;

            var result = new T[array.Length + items.Length];
            Array.Copy(array, 0, result, 0, array.Length);
            Array.Copy(items, 0, result, array.Length, items.Length);
            return result;
        }

        #endregion

        #region 数组遍历


        /// <summary>
        /// 遍历数组并对每个元素及其索引执行指定操作
        /// </summary>
        public static void ForEach<T>(this T[]? array, Action<T, int> action)
        {
            if (array == null || action == null)
                return;

            for (int i = 0; i < array.Length; i++)
            {
                action(array[i], i);
            }
        }

        #endregion

        #region 数组统计


        #endregion
    }
}
