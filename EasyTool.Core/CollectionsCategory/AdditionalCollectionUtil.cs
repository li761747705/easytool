using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyTool.CollectionsCategory
{
    /// <summary>
    /// 集合展平工具类
    /// </summary>
    public static class FlattenUtil
    {
        /// <summary>
        /// 展平嵌套集合
        /// </summary>
        public static IEnumerable<T> Flatten<T>(IEnumerable<IEnumerable<T>> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return source.SelectMany(x => x);
        }

        /// <summary>
        /// 递归展平
        /// </summary>
        public static IEnumerable<T> FlattenRecursive<T>(IEnumerable<object> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            foreach (var item in source)
            {
                if (item is IEnumerable<T> enumerable)
                {
                    foreach (var subItem in enumerable)
                    {
                        yield return subItem;
                    }
                }
                else if (item is T value)
                {
                    yield return value;
                }
                else if (item is IEnumerable<object> nested)
                {
                    foreach (var subItem in FlattenRecursive<T>(nested))
                    {
                        yield return subItem;
                    }
                }
            }
        }

        /// <summary>
        /// 展平字典
        /// </summary>
        public static IEnumerable<KeyValuePair<TKey, TValue>> Flatten<TKey, TValue>(
            IEnumerable<IDictionary<TKey, TValue>> dictionaries)
        {
            if (dictionaries == null)
                throw new ArgumentNullException(nameof(dictionaries));

            return dictionaries.SelectMany(d => d);
        }
    }

    /// <summary>
    /// 集合分组工具类
    /// </summary>
    public static class GroupingUtil
    {
        /// <summary>
        /// 将连续相同的元素分组
        /// </summary>
        public static IEnumerable<List<T>> GroupConsecutive<T>(IEnumerable<T> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var enumerator = source.GetEnumerator();
            if (!enumerator.MoveNext())
                yield break;

            var currentGroup = new List<T> { enumerator.Current };
            var current = enumerator.Current;

            while (enumerator.MoveNext())
            {
                if (EqualityComparer<T>.Default.Equals(enumerator.Current, current))
                {
                    currentGroup.Add(enumerator.Current);
                }
                else
                {
                    yield return currentGroup;
                    currentGroup = new List<T> { enumerator.Current };
                    current = enumerator.Current;
                }
            }

            yield return currentGroup;
        }

        /// <summary>
        /// 将连续满足条件的元素分组
        /// </summary>
        public static IEnumerable<List<T>> GroupConsecutive<T>(IEnumerable<T> source, Func<T, T, bool> belongsToSameGroup)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (belongsToSameGroup == null)
                throw new ArgumentNullException(nameof(belongsToSameGroup));

            var enumerator = source.GetEnumerator();
            if (!enumerator.MoveNext())
                yield break;

            var currentGroup = new List<T> { enumerator.Current };
            var current = enumerator.Current;

            while (enumerator.MoveNext())
            {
                if (belongsToSameGroup(current, enumerator.Current))
                {
                    currentGroup.Add(enumerator.Current);
                }
                else
                {
                    yield return currentGroup;
                    currentGroup = new List<T> { enumerator.Current };
                }
                current = enumerator.Current;
            }

            yield return currentGroup;
        }

        /// <summary>
        /// 按固定大小分组
        /// </summary>
        public static IEnumerable<List<T>> GroupBySize<T>(IEnumerable<T> source, int groupSize)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (groupSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(groupSize));

            var group = new List<T>(groupSize);
            foreach (var item in source)
            {
                group.Add(item);
                if (group.Count == groupSize)
                {
                    yield return group;
                    group = new List<T>(groupSize);
                }
            }

            if (group.Count > 0)
            {
                yield return group;
            }
        }

        /// <summary>
        /// 按条件分组的数量分组
        /// </summary>
        public static IEnumerable<List<T>> GroupWhile<T>(IEnumerable<T> source, Func<List<T>, T, bool> shouldInclude)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (shouldInclude == null)
                throw new ArgumentNullException(nameof(shouldInclude));

            var group = new List<T>();
            foreach (var item in source)
            {
                if (group.Count == 0 || shouldInclude(group, item))
                {
                    group.Add(item);
                }
                else
                {
                    yield return group;
                    group = new List<T> { item };
                }
            }

            if (group.Count > 0)
            {
                yield return group;
            }
        }
    }

    /// <summary>
    /// 集合合并工具类
    /// </summary>
    public static class MergeUtil
    {
        /// <summary>
        /// 合并两个有序集合
        /// </summary>
        public static IEnumerable<T> MergeOrdered<T>(IEnumerable<T> first, IEnumerable<T> second) where T : IComparable<T>
        {
            if (first == null)
                throw new ArgumentNullException(nameof(first));
            if (second == null)
                throw new ArgumentNullException(nameof(second));

            using var enum1 = first.GetEnumerator();
            using var enum2 = second.GetEnumerator();

            bool hasFirst = enum1.MoveNext();
            bool hasSecond = enum2.MoveNext();

            while (hasFirst && hasSecond)
            {
                if (enum1.Current.CompareTo(enum2.Current) <= 0)
                {
                    yield return enum1.Current;
                    hasFirst = enum1.MoveNext();
                }
                else
                {
                    yield return enum2.Current;
                    hasSecond = enum2.MoveNext();
                }
            }

            while (hasFirst)
            {
                yield return enum1.Current;
                hasFirst = enum1.MoveNext();
            }

            while (hasSecond)
            {
                yield return enum2.Current;
                hasSecond = enum2.MoveNext();
            }
        }

        /// <summary>
        /// 合并多个有序集合
        /// </summary>
        public static IEnumerable<T> MergeOrdered<T>(params IEnumerable<T>[] sources) where T : IComparable<T>
        {
            if (sources == null || sources.Length == 0)
                yield break;

            var enumerators = sources
                .Select(s => s?.GetEnumerator())
                .Where(e => e != null)
                .ToList();

            var hasMore = new bool[enumerators.Count];
            for (int i = 0; i < enumerators.Count; i++)
            {
                hasMore[i] = enumerators[i].MoveNext();
            }

            while (hasMore.Any(x => x))
            {
                int minIndex = -1;
                T minValue = default;

                for (int i = 0; i < enumerators.Count; i++)
                {
                    if (!hasMore[i])
                        continue;

                    if (minIndex == -1 || enumerators[i].Current.CompareTo(minValue) < 0)
                    {
                        minIndex = i;
                        minValue = enumerators[i].Current;
                    }
                }

                if (minIndex >= 0)
                {
                    yield return minValue;
                    hasMore[minIndex] = enumerators[minIndex].MoveNext();
                }
            }

            foreach (var e in enumerators)
            {
                e.Dispose();
            }
        }

        /// <summary>
        /// 合并字典（后者覆盖前者）
        /// </summary>
        public static Dictionary<TKey, TValue> Merge<TKey, TValue>(
            IDictionary<TKey, TValue> first,
            IDictionary<TKey, TValue> second)
        {
            if (first == null)
                throw new ArgumentNullException(nameof(first));
            if (second == null)
                throw new ArgumentNullException(nameof(second));

            var result = new Dictionary<TKey, TValue>(first);
            foreach (var kvp in second)
            {
                result[kvp.Key] = kvp.Value;
            }
            return result;
        }

        /// <summary>
        /// 合并字典（自定义冲突解决）
        /// </summary>
        public static Dictionary<TKey, TValue> Merge<TKey, TValue>(
            IDictionary<TKey, TValue> first,
            IDictionary<TKey, TValue> second,
            Func<TKey, TValue, TValue, TValue> conflictResolver)
        {
            if (first == null)
                throw new ArgumentNullException(nameof(first));
            if (second == null)
                throw new ArgumentNullException(nameof(second));
            if (conflictResolver == null)
                throw new ArgumentNullException(nameof(conflictResolver));

            var result = new Dictionary<TKey, TValue>(first);
            foreach (var kvp in second)
            {
                if (result.TryGetValue(kvp.Key, out var existing))
                {
                    result[kvp.Key] = conflictResolver(kvp.Key, existing, kvp.Value);
                }
                else
                {
                    result[kvp.Key] = kvp.Value;
                }
            }
            return result;
        }
    }

    /// <summary>
    /// 集合查找工具类
    /// </summary>
    public static class SearchUtil
    {
        /// <summary>
        /// 二分查找
        /// </summary>
        public static int BinarySearch<T>(IList<T> list, T value) where T : IComparable<T>
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));

            int left = 0;
            int right = list.Count - 1;

            while (left <= right)
            {
                int mid = left + (right - left) / 2;
                int cmp = list[mid].CompareTo(value);

                if (cmp == 0)
                    return mid;
                if (cmp < 0)
                    left = mid + 1;
                else
                    right = mid - 1;
            }

            return ~left; // 返回插入点的补码
        }

        /// <summary>
        /// 查找第一个大于等于指定值的元素索引
        /// </summary>
        public static int LowerBound<T>(IList<T> list, T value) where T : IComparable<T>
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));

            int left = 0;
            int right = list.Count;

            while (left < right)
            {
                int mid = left + (right - left) / 2;
                if (list[mid].CompareTo(value) < 0)
                    left = mid + 1;
                else
                    right = mid;
            }

            return left;
        }

        /// <summary>
        /// 查找第一个大于指定值的元素索引
        /// </summary>
        public static int UpperBound<T>(IList<T> list, T value) where T : IComparable<T>
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));

            int left = 0;
            int right = list.Count;

            while (left < right)
            {
                int mid = left + (right - left) / 2;
                if (list[mid].CompareTo(value) <= 0)
                    left = mid + 1;
                else
                    right = mid;
            }

            return left;
        }

        /// <summary>
        /// 查找范围内的元素数量
        /// </summary>
        public static int CountInRange<T>(IList<T> list, T min, T max) where T : IComparable<T>
        {
            return UpperBound(list, max) - LowerBound(list, min);
        }

        /// <summary>
        /// 查找众数（出现次数最多的元素）
        /// </summary>
        public static T FindMajority<T>(IEnumerable<T> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var list = source.ToList();
            if (list.Count == 0)
                throw new ArgumentException("Collection is empty");

            // Boyer-Moore 多数投票算法
            T candidate = default;
            int count = 0;

            foreach (var item in list)
            {
                if (count == 0)
                {
                    candidate = item;
                    count = 1;
                }
                else if (EqualityComparer<T>.Default.Equals(item, candidate))
                {
                    count++;
                }
                else
                {
                    count--;
                }
            }

            // 验证
            count = list.Count(x => EqualityComparer<T>.Default.Equals(x, candidate));
            if (count > list.Count / 2)
                return candidate;

            throw new InvalidOperationException("No majority element found");
        }

        /// <summary>
        /// 尝试查找众数
        /// </summary>
        public static bool TryFindMajority<T>(IEnumerable<T> source, out T majority)
        {
            try
            {
                majority = FindMajority(source);
                return true;
            }
            catch
            {
                majority = default;
                return false;
            }
        }
    }

    /// <summary>
    /// 集合序列工具类
    /// </summary>
    public static class SequenceUtil
    {
        /// <summary>
        /// 生成等差数列
        /// </summary>
        public static IEnumerable<int> Range(int start, int count, int step = 1)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            for (int i = 0; i < count; i++)
            {
                yield return start + i * step;
            }
        }

        /// <summary>
        /// 生成等差数列（浮点数）
        /// </summary>
        public static IEnumerable<double> Range(double start, int count, double step)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            for (int i = 0; i < count; i++)
            {
                yield return start + i * step;
            }
        }

        /// <summary>
        /// 生成重复序列
        /// </summary>
        public static IEnumerable<T> Repeat<T>(T value, int count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            for (int i = 0; i < count; i++)
            {
                yield return value;
            }
        }

        /// <summary>
        /// 循环生成序列
        /// </summary>
        public static IEnumerable<T> Cycle<T>(IEnumerable<T> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var list = source.ToList();
            if (list.Count == 0)
                yield break;

            int index = 0;
            while (true)
            {
                yield return list[index];
                index = (index + 1) % list.Count;
            }
        }

        /// <summary>
        /// 循环生成指定次数
        /// </summary>
        public static IEnumerable<T> Cycle<T>(IEnumerable<T> source, int count)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            var list = source.ToList();
            if (list.Count == 0)
                yield break;

            for (int i = 0; i < count; i++)
            {
                yield return list[i % list.Count];
            }
        }

        /// <summary>
        /// 生成斐波那契数列
        /// </summary>
        public static IEnumerable<long> Fibonacci(int count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            long a = 0, b = 1;
            for (int i = 0; i < count; i++)
            {
                yield return a;
                (a, b) = (b, a + b);
            }
        }

        /// <summary>
        /// 生成迭代序列
        /// </summary>
        public static IEnumerable<T> Iterate<T>(T initial, Func<T, T> next, int count)
        {
            if (next == null)
                throw new ArgumentNullException(nameof(next));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            T current = initial;
            for (int i = 0; i < count; i++)
            {
                yield return current;
                current = next(current);
            }
        }
    }

    /// <summary>
    /// 集合集合操作工具类
    /// </summary>
    public static class SetOperationUtil
    {
        /// <summary>
        /// 笛卡尔积
        /// </summary>
        public static IEnumerable<(T1, T2)> CartesianProduct<T1, T2>(
            IEnumerable<T1> first,
            IEnumerable<T2> second)
        {
            if (first == null)
                throw new ArgumentNullException(nameof(first));
            if (second == null)
                throw new ArgumentNullException(nameof(second));

            return from a in first
                   from b in second
                   select (a, b);
        }

        /// <summary>
        /// 多集合笛卡尔积
        /// </summary>
        public static IEnumerable<List<T>> CartesianProduct<T>(IEnumerable<IEnumerable<T>> sources)
        {
            if (sources == null)
                throw new ArgumentNullException(nameof(sources));

            var lists = sources.Select(s => s.ToList()).ToList();
            if (lists.Count == 0)
            {
                yield return new List<T>();
                yield break;
            }

            var indices = new int[lists.Count];
            var counts = lists.Select(l => l.Count).ToArray();

            if (counts.Any(c => c == 0))
                yield break;

            while (true)
            {
                var result = new List<T>();
                for (int i = 0; i < lists.Count; i++)
                {
                    result.Add(lists[i][indices[i]]);
                }
                yield return result;

                // 增加索引
                int j = lists.Count - 1;
                while (j >= 0)
                {
                    indices[j]++;
                    if (indices[j] < counts[j])
                        break;
                    indices[j] = 0;
                    j--;
                }

                if (j < 0)
                    break;
            }
        }

        /// <summary>
        /// 幂集（所有子集）
        /// </summary>
        public static IEnumerable<List<T>> PowerSet<T>(IEnumerable<T> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var list = source.ToList();
            int count = 1 << list.Count; // 2^n

            for (int i = 0; i < count; i++)
            {
                var subset = new List<T>();
                for (int j = 0; j < list.Count; j++)
                {
                    if ((i & (1 << j)) != 0)
                    {
                        subset.Add(list[j]);
                    }
                }
                yield return subset;
            }
        }

        /// <summary>
        /// 获取指定大小的所有子集
        /// </summary>
        public static IEnumerable<List<T>> SubsetsOfSize<T>(IEnumerable<T> source, int size)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (size < 0)
                throw new ArgumentOutOfRangeException(nameof(size));

            var list = source.ToList();
            if (size > list.Count)
                yield break;

            var indices = Enumerable.Range(0, size).ToArray();

            while (true)
            {
                yield return indices.Select(i => list[i]).ToList();

                // 找到可以增加的索引
                int i = size - 1;
                while (i >= 0 && indices[i] == list.Count - size + i)
                    i--;

                if (i < 0)
                    break;

                indices[i]++;
                for (int j = i + 1; j < size; j++)
                {
                    indices[j] = indices[j - 1] + 1;
                }
            }
        }
    }

    /// <summary>
    /// 集合排序工具类
    /// </summary>
    public static class SortingUtil
    {
        /// <summary>
        /// 快速选择（找到第 k 小的元素）
        /// </summary>
        public static T QuickSelect<T>(IList<T> list, int k) where T : IComparable<T>
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));
            if (k < 0 || k >= list.Count)
                throw new ArgumentOutOfRangeException(nameof(k));

            var arr = list.ToArray();
            return QuickSelectInternal(arr, 0, arr.Length - 1, k);
        }

        private static T QuickSelectInternal<T>(T[] arr, int left, int right, int k) where T : IComparable<T>
        {
            if (left == right)
                return arr[left];

            int pivotIndex = Partition(arr, left, right);

            if (k == pivotIndex)
                return arr[k];
            if (k < pivotIndex)
                return QuickSelectInternal(arr, left, pivotIndex - 1, k);
            return QuickSelectInternal(arr, pivotIndex + 1, right, k);
        }

        private static int Partition<T>(T[] arr, int left, int right) where T : IComparable<T>
        {
            T pivot = arr[right];
            int i = left;

            for (int j = left; j < right; j++)
            {
                if (arr[j].CompareTo(pivot) <= 0)
                {
                    Swap(arr, i, j);
                    i++;
                }
            }

            Swap(arr, i, right);
            return i;
        }

        private static void Swap<T>(T[] arr, int i, int j)
        {
            T temp = arr[i];
            arr[i] = arr[j];
            arr[j] = temp;
        }

        /// <summary>
        /// 多键排序
        /// </summary>
        public static List<T> SortByMultiple<T>(IEnumerable<T> source, params Func<T, IComparable>[] selectors)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (selectors == null || selectors.Length == 0)
                return source.ToList();

            var list = source.ToList();
            list.Sort((a, b) =>
            {
                foreach (var selector in selectors)
                {
                    int cmp = selector(a).CompareTo(selector(b));
                    if (cmp != 0)
                        return cmp;
                }
                return 0;
            });
            return list;
        }

        /// <summary>
        /// 稳定排序
        /// </summary>
        public static List<T> StableSort<T>(IEnumerable<T> source, Comparison<T> comparison)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (comparison == null)
                throw new ArgumentNullException(nameof(comparison));

            var list = source.Select((x, i) => new { Value = x, Index = i }).ToList();
            list.Sort((a, b) =>
            {
                int cmp = comparison(a.Value, b.Value);
                return cmp != 0 ? cmp : a.Index.CompareTo(b.Index);
            });
            return list.Select(x => x.Value).ToList();
        }
    }
}
