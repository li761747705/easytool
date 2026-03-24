using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyTool.CollectionsCategory
{
    /// <summary>
    /// Top-K 选择工具类
    /// 高效地从集合中选出前K个最大/最小元素
    /// 使用快速选择算法，平均时间复杂度 O(n)
    /// </summary>
    public static class TopKUtil
    {
        /// <summary>
        /// 获取前K个最大元素
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="source">源集合</param>
        /// <param name="k">数量</param>
        /// <returns>前K个最大元素（降序）</returns>
        public static IEnumerable<T> TopK<T>(IEnumerable<T> source, int k) where T : IComparable<T>
        {
            return TopK(source, k, Comparer<T>.Default, false);
        }

        /// <summary>
        /// 获取前K个最小元素
        /// </summary>
        public static IEnumerable<T> BottomK<T>(IEnumerable<T> source, int k) where T : IComparable<T>
        {
            return TopK(source, k, Comparer<T>.Default, true);
        }

        /// <summary>
        /// 获取前K个元素（使用比较器）
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="source">源集合</param>
        /// <param name="k">数量</param>
        /// <param name="comparer">比较器</param>
        /// <param name="ascending">是否升序（true=最小K个，false=最大K个）</param>
        /// <returns>前K个元素</returns>
        public static IEnumerable<T> TopK<T>(IEnumerable<T> source, int k, IComparer<T> comparer, bool ascending)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (k <= 0)
                return Enumerable.Empty<T>();

            var list = source.ToList();
            if (k >= list.Count)
                return ascending ? list.OrderBy(x => x, comparer) : list.OrderByDescending(x => x, comparer);

            // 使用快速选择算法
            int actualK = Math.Min(k, list.Count);

            if (ascending)
            {
                QuickSelect(list, 0, list.Count - 1, actualK - 1, comparer);
                var result = list.Take(actualK).ToList();
                result.Sort(comparer);
                return result;
            }
            else
            {
                // 对于最大K个，我们找第(n-k)小的元素
                int targetIndex = list.Count - actualK;
                QuickSelect(list, 0, list.Count - 1, targetIndex, comparer);
                var result = list.Skip(targetIndex).ToList();
                result.Sort(comparer);
                result.Reverse();
                return result;
            }
        }

        /// <summary>
        /// 获取前K个最大元素（使用选择器）
        /// </summary>
        public static IEnumerable<T> TopKBy<T, TKey>(IEnumerable<T> source, int k, Func<T, TKey> keySelector)
            where TKey : IComparable<TKey>
        {
            return TopKBy(source, k, keySelector, Comparer<TKey>.Default, false);
        }

        /// <summary>
        /// 获取前K个最小元素（使用选择器）
        /// </summary>
        public static IEnumerable<T> BottomKBy<T, TKey>(IEnumerable<T> source, int k, Func<T, TKey> keySelector)
            where TKey : IComparable<TKey>
        {
            return TopKBy(source, k, keySelector, Comparer<TKey>.Default, true);
        }

        /// <summary>
        /// 获取前K个元素（使用选择器和比较器）
        /// </summary>
        public static IEnumerable<T> TopKBy<T, TKey>(IEnumerable<T> source, int k, Func<T, TKey> keySelector,
            IComparer<TKey> comparer, bool ascending)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));

            var list = source.ToList();
            if (k <= 0 || list.Count == 0)
                return Enumerable.Empty<T>();

            // 带索引的快速选择
            var indexed = list.Select((item, index) => new { Item = item, Key = keySelector(item), Index = index }).ToList();

            if (ascending)
            {
                indexed = indexed.OrderBy(x => x.Key, comparer).Take(k).ToList();
            }
            else
            {
                indexed = indexed.OrderByDescending(x => x.Key, comparer).Take(k).ToList();
            }

            return indexed.Select(x => x.Item);
        }

        /// <summary>
        /// 使用堆获取前K个元素（适用于大数据流）
        /// </summary>
        public static IEnumerable<T> TopKUsingHeap<T>(IEnumerable<T> source, int k) where T : IComparable<T>
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (k <= 0)
                return Enumerable.Empty<T>();

#if NETSTANDARD2_1
            var minHeap = new PriorityQueue<T, T>(Comparer<T>.Default, false);

            foreach (var item in source)
            {
                if (minHeap.Count < k)
                {
                    minHeap.Enqueue(item, item);
                }
                else if (item.CompareTo(minHeap.Peek()) > 0)
                {
                    minHeap.Dequeue();
                    minHeap.Enqueue(item, item);
                }
            }

            var result = new List<T>();
            while (minHeap.Count > 0)
            {
                result.Add(minHeap.Dequeue());
            }
#else
            var minHeap = new System.Collections.Generic.PriorityQueue<T, T>();

            foreach (var item in source)
            {
                if (minHeap.Count < k)
                {
                    minHeap.Enqueue(item, item);
                }
                else if (item.CompareTo(minHeap.Peek()) > 0)
                {
                    minHeap.Dequeue();
                    minHeap.Enqueue(item, item);
                }
            }

            var result = new List<T>();
            while (minHeap.Count > 0)
            {
                result.Add(minHeap.Dequeue());
            }
#endif

            result.Reverse();
            return result;
        }

        private static void QuickSelect<T>(List<T> list, int left, int right, int k, IComparer<T> comparer)
        {
            while (left < right)
            {
                int pivotIndex = Partition(list, left, right, comparer);

                if (k == pivotIndex)
                    return;
                else if (k < pivotIndex)
                    right = pivotIndex - 1;
                else
                    left = pivotIndex + 1;
            }
        }

        private static int Partition<T>(List<T> list, int left, int right, IComparer<T> comparer)
        {
            T pivot = list[right];
            int i = left;

            for (int j = left; j < right; j++)
            {
                if (comparer.Compare(list[j], pivot) <= 0)
                {
                    Swap(list, i, j);
                    i++;
                }
            }

            Swap(list, i, right);
            return i;
        }

        private static void Swap<T>(List<T> list, int i, int j)
        {
            if (i != j)
            {
                T temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }
    }
}
