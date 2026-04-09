using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EasyTool
{
    /// <summary>
    /// 集合操作工具类
    /// 对标 Hutool 的 CollUtil
    /// 提供集合的创建、判空、转换、排序、查找等常用操作
    /// </summary>
    public static class CollUtil
    {
        #region 集合创建

        /// <summary>
        /// 创建 ArrayList
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="elements">元素</param>
        /// <returns>列表</returns>
        public static List<T> NewList<T>(params T[] elements)
        {
            return elements == null ? new List<T>() : new List<T>(elements);
        }

        /// <summary>
        /// 创建 ArrayList
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="elements">元素集合</param>
        /// <returns>列表</returns>
        public static List<T> NewList<T>(IEnumerable<T>? elements)
        {
            return elements == null ? new List<T>() : new List<T>(elements);
        }

        /// <summary>
        /// 创建 HashSet
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="elements">元素</param>
        /// <returns>哈希集合</returns>
        public static HashSet<T> NewHashSet<T>(params T[] elements)
        {
            return elements == null ? new HashSet<T>() : new HashSet<T>(elements);
        }

        /// <summary>
        /// 创建 HashSet
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="elements">元素集合</param>
        /// <returns>哈希集合</returns>
        public static HashSet<T> NewHashSet<T>(IEnumerable<T>? elements)
        {
            return elements == null ? new HashSet<T>() : new HashSet<T>(elements);
        }

        /// <summary>
        /// 创建 LinkedList
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="elements">元素</param>
        /// <returns>链表</returns>
        public static LinkedList<T> NewLinkedList<T>(params T[] elements)
        {
            var list = new LinkedList<T>();
            if (elements != null)
            {
                foreach (var element in elements)
                {
                    list.AddLast(element);
                }
            }
            return list;
        }

        /// <summary>
        /// 创建指定大小的列表（填充默认值）
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="size">大小</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>列表</returns>
        public static List<T> NewList<T>(int size, T defaultValue)
        {
            var list = new List<T>(size);
            for (int i = 0; i < size; i++)
            {
                list.Add(defaultValue);
            }
            return list;
        }

        #endregion

        #region 集合判空

        /// <summary>
        /// 判断集合是否为空
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="collection">集合</param>
        /// <returns>是否为空</returns>
        public static bool IsEmpty<T>(IEnumerable<T>? collection)
        {
            if (collection == null)
                return true;

            if (collection is ICollection<T> col)
                return col.Count == 0;

            return !collection.Any();
        }

        /// <summary>
        /// 判断集合是否不为空
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="collection">集合</param>
        /// <returns>是否不为空</returns>
        public static bool IsNotEmpty<T>(IEnumerable<T>? collection)
        {
            return !IsEmpty(collection);
        }

        /// <summary>
        /// 判断集合中是否包含 null 元素
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="collection">集合</param>
        /// <returns>是否包含 null</returns>
        public static bool HasNull<T>(IEnumerable<T>? collection)
        {
            if (collection == null)
                return true;

            return collection.Any(item => item == null);
        }

        /// <summary>
        /// 获取集合大小
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="collection">集合</param>
        /// <returns>大小</returns>
        public static int Size<T>(IEnumerable<T>? collection)
        {
            if (collection == null)
                return 0;

            if (collection is ICollection<T> col)
                return col.Count;

            return collection.Count();
        }

        #endregion

        #region 集合操作

        /// <summary>
        /// 去重
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="collection">集合</param>
        /// <returns>去重后的列表</returns>
        public static List<T> Distinct<T>(IEnumerable<T>? collection)
        {
            if (collection == null)
                return new List<T>();

            return collection.Distinct().ToList();
        }

        /// <summary>
        /// 根据属性去重
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <typeparam name="TKey">属性类型</typeparam>
        /// <param name="collection">集合</param>
        /// <param name="keySelector">属性选择器</param>
        /// <returns>去重后的列表</returns>
        public static List<T> DistinctBy<T, TKey>(IEnumerable<T>? collection, Func<T, TKey> keySelector)
        {
            if (collection == null || keySelector == null)
                return new List<T>();

            return collection.GroupBy(keySelector).Select(g => g.First()).ToList();
        }

        /// <summary>
        /// 连接集合元素为字符串
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="collection">集合</param>
        /// <param name="separator">分隔符</param>
        /// <returns>连接后的字符串</returns>
        public static string Join<T>(IEnumerable<T>? collection, string separator = ",")
        {
            if (collection == null)
                return string.Empty;

            return string.Join(separator, collection);
        }

        /// <summary>
        /// 连接集合元素为字符串（带前后缀）
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="collection">集合</param>
        /// <param name="separator">分隔符</param>
        /// <param name="prefix">前缀</param>
        /// <param name="suffix">后缀</param>
        /// <returns>连接后的字符串</returns>
        public static string Join<T>(IEnumerable<T>? collection, string separator, string prefix, string suffix)
        {
            if (collection == null)
                return prefix + suffix;

            return prefix + string.Join(separator, collection) + suffix;
        }

        /// <summary>
        /// 分割集合为多个子列表
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="collection">集合</param>
        /// <param name="batchSize">每批大小</param>
        /// <returns>分割后的列表</returns>
        public static List<List<T>> Split<T>(IEnumerable<T>? collection, int batchSize)
        {
            if (collection == null || batchSize <= 0)
                return new List<List<T>>();

            var result = new List<List<T>>();
            var batch = new List<T>(batchSize);

            foreach (var item in collection)
            {
                batch.Add(item);
                if (batch.Count == batchSize)
                {
                    result.Add(batch);
                    batch = new List<T>(batchSize);
                }
            }

            if (batch.Count > 0)
                result.Add(batch);

            return result;
        }

        /// <summary>
        /// 反转集合
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="collection">集合</param>
        /// <returns>反转后的列表</returns>
        public static List<T> Reverse<T>(IEnumerable<T>? collection)
        {
            if (collection == null)
                return new List<T>();

            var list = collection.ToList();
            list.Reverse();
            return list;
        }

        /// <summary>
        /// 随机打乱集合
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="collection">集合</param>
        /// <returns>打乱后的列表</returns>
        public static List<T> Shuffle<T>(IEnumerable<T>? collection)
        {
            if (collection == null)
                return new List<T>();

            var list = collection.ToList();
            var random = new Random();
            int n = list.Count;

            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                (list[k], list[n]) = (list[n], list[k]);
            }

            return list;
        }

        /// <summary>
        /// 排序集合
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="collection">集合</param>
        /// <param name="comparer">比较器</param>
        /// <returns>排序后的列表</returns>
        public static List<T> Sort<T>(IEnumerable<T>? collection, IComparer<T>? comparer = null)
        {
            if (collection == null)
                return new List<T>();

            var list = collection.ToList();
            list.Sort(comparer);
            return list;
        }

        /// <summary>
        /// 按属性排序
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <typeparam name="TKey">属性类型</typeparam>
        /// <param name="collection">集合</param>
        /// <param name="keySelector">属性选择器</param>
        /// <param name="descending">是否降序</param>
        /// <returns>排序后的列表</returns>
        public static List<T> SortBy<T, TKey>(IEnumerable<T>? collection, Func<T, TKey> keySelector, bool descending = false)
        {
            if (collection == null || keySelector == null)
                return new List<T>();

            return descending
                ? collection.OrderByDescending(keySelector).ToList()
                : collection.OrderBy(keySelector).ToList();
        }

        #endregion

        #region 集合查找

        /// <summary>
        /// 获取第一个元素
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="collection">集合</param>
        /// <returns>第一个元素</returns>
        public static T? First<T>(IEnumerable<T>? collection)
        {
            if (collection == null)
                return default;

            return collection.FirstOrDefault();
        }

        /// <summary>
        /// 获取最后一个元素
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="collection">集合</param>
        /// <returns>最后一个元素</returns>
        public static T? Last<T>(IEnumerable<T>? collection)
        {
            if (collection == null)
                return default;

            return collection.LastOrDefault();
        }

        /// <summary>
        /// 获取指定索引的元素
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="collection">集合</param>
        /// <param name="index">索引</param>
        /// <returns>元素</returns>
        public static T? Get<T>(IEnumerable<T>? collection, int index)
        {
            if (collection == null)
                return default;

            if (index < 0)
                return default;

            if (collection is IList<T> list)
            {
                if (index < list.Count)
                    return list[index];
                return default;
            }

            return collection.ElementAtOrDefault(index);
        }

        /// <summary>
        /// 查找第一个匹配的元素
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="collection">集合</param>
        /// <param name="predicate">条件</param>
        /// <returns>匹配的元素</returns>
        public static T? FindFirst<T>(IEnumerable<T>? collection, Func<T, bool> predicate)
        {
            if (collection == null || predicate == null)
                return default;

            return collection.FirstOrDefault(predicate);
        }

        /// <summary>
        /// 查找所有匹配的元素
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="collection">集合</param>
        /// <param name="predicate">条件</param>
        /// <returns>匹配的元素列表</returns>
        public static List<T> FindAll<T>(IEnumerable<T>? collection, Func<T, bool> predicate)
        {
            if (collection == null || predicate == null)
                return new List<T>();

            return collection.Where(predicate).ToList();
        }

        /// <summary>
        /// 随机获取一个元素
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="collection">集合</param>
        /// <returns>随机元素</returns>
        public static T? Random<T>(IEnumerable<T>? collection)
        {
            if (collection == null)
                return default;

            var list = collection.ToList();
            if (list.Count == 0)
                return default;

            var random = new Random();
            return list[random.Next(list.Count)];
        }

        /// <summary>
        /// 随机获取多个元素
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="collection">集合</param>
        /// <param name="count">数量</param>
        /// <returns>随机元素列表</returns>
        public static List<T> Random<T>(IEnumerable<T>? collection, int count)
        {
            if (collection == null || count <= 0)
                return new List<T>();

            var list = collection.ToList();
            if (list.Count == 0)
                return new List<T>();

            var random = new Random();
            return list.OrderBy(x => random.Next()).Take(count).ToList();
        }

        #endregion

        #region 集合转换

        /// <summary>
        /// 集合转数组
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="collection">集合</param>
        /// <returns>数组</returns>
        public static T[] ToArray<T>(IEnumerable<T>? collection)
        {
            if (collection == null)
                return Array.Empty<T>();

            return collection.ToArray();
        }

        /// <summary>
        /// 提取属性列表
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <typeparam name="TResult">结果类型</typeparam>
        /// <param name="collection">集合</param>
        /// <param name="selector">属性选择器</param>
        /// <returns>属性列表</returns>
        public static List<TResult> Map<T, TResult>(IEnumerable<T>? collection, Func<T, TResult> selector)
        {
            if (collection == null || selector == null)
                return new List<TResult>();

            return collection.Select(selector).ToList();
        }

        /// <summary>
        /// 提取属性列表（通过反射）
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <typeparam name="TResult">结果类型</typeparam>
        /// <param name="collection">集合</param>
        /// <param name="propertyName">属性名</param>
        /// <returns>属性列表</returns>
        public static List<TResult?> GetFieldValues<T, TResult>(IEnumerable<T>? collection, string propertyName)
        {
            if (collection == null || string.IsNullOrEmpty(propertyName))
                return new List<TResult?>();

            var prop = typeof(T).GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (prop == null)
                return new List<TResult?>();

            return collection.Select(item =>
            {
                if (item == null)
                    return default;
                var value = prop.GetValue(item);
                return value is TResult result ? result : default;
            }).ToList();
        }

        /// <summary>
        /// 过滤集合
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="collection">集合</param>
        /// <param name="predicate">条件</param>
        /// <returns>过滤后的列表</returns>
        public static List<T> Filter<T>(IEnumerable<T>? collection, Func<T, bool> predicate)
        {
            return FindAll(collection, predicate);
        }

        #endregion

        #region 集合运算

        /// <summary>
        /// 并集
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="first">第一个集合</param>
        /// <param name="second">第二个集合</param>
        /// <returns>并集</returns>
        public static List<T> Union<T>(IEnumerable<T>? first, IEnumerable<T>? second)
        {
            var result = new List<T>();

            if (first != null)
                result.AddRange(first);

            if (second != null)
                result.AddRange(second);

            return result.Distinct().ToList();
        }

        /// <summary>
        /// 交集
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="first">第一个集合</param>
        /// <param name="second">第二个集合</param>
        /// <returns>交集</returns>
        public static List<T> Intersect<T>(IEnumerable<T>? first, IEnumerable<T>? second)
        {
            if (first == null || second == null)
                return new List<T>();

            return first.Intersect(second).ToList();
        }

        /// <summary>
        /// 差集
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="first">第一个集合</param>
        /// <param name="second">第二个集合</param>
        /// <returns>差集</returns>
        public static List<T> Except<T>(IEnumerable<T>? first, IEnumerable<T>? second)
        {
            if (first == null)
                return new List<T>();

            if (second == null)
                return first.ToList();

            return first.Except(second).ToList();
        }

        /// <summary>
        /// 判断是否包含所有元素
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="collection">集合</param>
        /// <param name="items">要检查的元素</param>
        /// <returns>是否包含</returns>
        public static bool ContainsAll<T>(IEnumerable<T>? collection, params T[] items)
        {
            if (collection == null || items == null)
                return false;

            var set = new HashSet<T>(collection);
            return items.All(item => set.Contains(item));
        }

        /// <summary>
        /// 判断是否包含任意元素
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="collection">集合</param>
        /// <param name="items">要检查的元素</param>
        /// <returns>是否包含</returns>
        public static bool ContainsAny<T>(IEnumerable<T>? collection, params T[] items)
        {
            if (collection == null || items == null)
                return false;

            var set = new HashSet<T>(collection);
            return items.Any(item => set.Contains(item));
        }

        #endregion
    }
}