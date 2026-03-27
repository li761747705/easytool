using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyTool.CollectionsCategory
{
    /// <summary>
    /// 分组工具类
    /// </summary>
    public static class GroupUtil
    {
        /// <summary>
        /// 按指定数量分组
        /// </summary>
        public static List<List<T>> Chunk<T>(IEnumerable<T> source, int size)
        {
            if (size <= 0)
                throw new ArgumentException("分组大小必须大于0", nameof(size));

            var result = new List<List<T>>();
            var current = new List<T>(size);

            foreach (var item in source)
            {
                current.Add(item);

                if (current.Count == size)
                {
                    result.Add(current);
                    current = new List<T>(size);
                }
            }

            if (current.Count > 0)
            {
                result.Add(current);
            }

            return result;
        }

        /// <summary>
        /// 按条件分组
        /// </summary>
        public static List<List<T>> GroupWhile<T>(IEnumerable<T> source, Func<T, bool> predicate)
        {
            var result = new List<List<T>>();
            var current = new List<T>();

            foreach (var item in source)
            {
                if (predicate(item) && current.Count > 0)
                {
                    result.Add(current);
                    current = new List<T>();
                }

                current.Add(item);
            }

            if (current.Count > 0)
            {
                result.Add(current);
            }

            return result;
        }

        /// <summary>
        /// 按相邻相同元素分组
        /// </summary>
        public static List<List<T>> GroupAdjacent<T>(IEnumerable<T> source)
        {
            var result = new List<List<T>>();
            List<T>? current = null;

            foreach (var item in source)
            {
                if (current == null || !EqualityComparer<T>.Default.Equals(current[0], item))
                {
                    current = new List<T> { item };
                    result.Add(current);
                }
                else
                {
                    current.Add(item);
                }
            }

            return result;
        }

        /// <summary>
        /// 按相邻相同元素分组（使用比较器）
        /// </summary>
        public static List<List<T>> GroupAdjacent<T>(IEnumerable<T> source, IEqualityComparer<T> comparer)
        {
            var result = new List<List<T>>();
            List<T>? current = null;

            foreach (var item in source)
            {
                if (current == null || !comparer.Equals(current[0], item))
                {
                    current = new List<T> { item };
                    result.Add(current);
                }
                else
                {
                    current.Add(item);
                }
            }

            return result;
        }

        /// <summary>
        /// 交替分组
        /// </summary>
        public static (List<T> First, List<T> Second) Alternate<T>(IEnumerable<T> source)
        {
            var first = new List<T>();
            var second = new List<T>();
            var isFirst = true;

            foreach (var item in source)
            {
                if (isFirst)
                    first.Add(item);
                else
                    second.Add(item);

                isFirst = !isFirst;
            }

            return (first, second);
        }

        /// <summary>
        /// 分割集合
        /// </summary>
        public static (List<T> True, List<T> False) Partition<T>(IEnumerable<T> source, Func<T, bool> predicate)
        {
            var trueItems = new List<T>();
            var falseItems = new List<T>();

            foreach (var item in source)
            {
                if (predicate(item))
                    trueItems.Add(item);
                else
                    falseItems.Add(item);
            }

            return (trueItems, falseItems);
        }

        /// <summary>
        /// 交错合并两个集合
        /// </summary>
        public static IEnumerable<T> Interleave<T>(IEnumerable<T> first, IEnumerable<T> second)
        {
            using var e1 = first.GetEnumerator();
            using var e2 = second.GetEnumerator();

            while (e1.MoveNext())
            {
                yield return e1.Current;

                if (e2.MoveNext())
                    yield return e2.Current;
            }

            while (e2.MoveNext())
            {
                yield return e2.Current;
            }
        }

        /// <summary>
        /// 按滑动窗口分组
        /// </summary>
        public static List<List<T>> Window<T>(IEnumerable<T> source, int size, int step = 1)
        {
            if (size <= 0)
                throw new ArgumentException("窗口大小必须大于0", nameof(size));

            if (step <= 0)
                throw new ArgumentException("步进必须大于0", nameof(step));

            var list = source.ToList();
            var result = new List<List<T>>();

            for (int i = 0; i <= list.Count - size; i += step)
            {
                result.Add(list.Skip(i).Take(size).ToList());
            }

            return result;
        }

        /// <summary>
        /// 按累积条件分组
        /// </summary>
        public static List<List<T>> GroupByAccumulator<T>(IEnumerable<T> source, Func<T, T, bool> shouldGroup)
        {
            var result = new List<List<T>>();
            var current = new List<T>();
            T? lastItem = default;

            foreach (var item in source)
            {
                if (lastItem == null || shouldGroup(lastItem, item))
                {
                    current.Add(item);
                }
                else
                {
                    if (current.Count > 0)
                        result.Add(current);
                    current = new List<T> { item };
                }

                lastItem = item;
            }

            if (current.Count > 0)
            {
                result.Add(current);
            }

            return result;
        }

        /// <summary>
        /// 获取笛卡尔积
        /// </summary>
        public static IEnumerable<(T1 First, T2 Second)> CartesianProduct<T1, T2>(
            IEnumerable<T1> first, IEnumerable<T2> second)
        {
            foreach (var item1 in first)
            {
                foreach (var item2 in second)
                {
                    yield return (item1, item2);
                }
            }
        }

        /// <summary>
        /// 获取多个集合的笛卡尔积
        /// </summary>
        public static IEnumerable<List<T>> CartesianProduct<T>(IEnumerable<IEnumerable<T>> sources)
        {
            var sourceList = sources.ToList();

            if (sourceList.Count == 0)
            {
                yield return new List<T>();
                yield break;
            }

            var first = sourceList[0];
            var rest = sourceList.Skip(1);

            foreach (var item in first)
            {
                foreach (var restCombination in CartesianProduct(rest))
                {
                    var combination = new List<T> { item };
                    combination.AddRange(restCombination);
                    yield return combination;
                }
            }
        }

        /// <summary>
        /// 获取排列组合
        /// </summary>
        public static IEnumerable<List<T>> Combinations<T>(IEnumerable<T> source, int count)
        {
            var list = source.ToList();

            if (count > list.Count)
                yield break;

            if (count == 0)
            {
                yield return new List<T>();
                yield break;
            }

            if (count == 1)
            {
                foreach (var item in list)
                {
                    yield return new List<T> { item };
                }
                yield break;
            }

            for (int i = 0; i <= list.Count - count; i++)
            {
                foreach (var restCombination in Combinations(list.Skip(i + 1), count - 1))
                {
                    var combination = new List<T> { list[i] };
                    combination.AddRange(restCombination);
                    yield return combination;
                }
            }
        }

        /// <summary>
        /// 获取全排列
        /// </summary>
        public static IEnumerable<List<T>> Permutations<T>(IEnumerable<T> source)
        {
            var list = source.ToList();

            if (list.Count == 0)
            {
                yield return new List<T>();
                yield break;
            }

            if (list.Count == 1)
            {
                yield return new List<T>(list);
                yield break;
            }

            for (int i = 0; i < list.Count; i++)
            {
                var current = list[i];
                var remaining = list.Take(i).Concat(list.Skip(i + 1));

                foreach (var permutation in Permutations(remaining))
                {
                    var result = new List<T> { current };
                    result.AddRange(permutation);
                    yield return result;
                }
            }
        }
    }
}
