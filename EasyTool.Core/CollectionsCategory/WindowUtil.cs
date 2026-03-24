using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyTool.CollectionsCategory
{
    /// <summary>
    /// 滑动窗口工具类
    /// 提供滑动窗口、滑动平均等功能
    /// </summary>
    public static class SlidingWindowUtil
    {
        /// <summary>
        /// 创建滑动窗口枚举器
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="source">源集合</param>
        /// <param name="windowSize">窗口大小</param>
        /// <returns>每个窗口的元素数组</returns>
        public static IEnumerable<T[]> Windows<T>(IEnumerable<T> source, int windowSize)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (windowSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(windowSize));

            var window = new Queue<T>();
            foreach (var item in source)
            {
                window.Enqueue(item);
                if (window.Count > windowSize)
                {
                    window.Dequeue();
                }
                if (window.Count == windowSize)
                {
                    yield return window.ToArray();
                }
            }
        }

        /// <summary>
        /// 创建滑动窗口（带步长）
        /// </summary>
        public static IEnumerable<T[]> Windows<T>(IEnumerable<T> source, int windowSize, int step)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (windowSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(windowSize));
            if (step <= 0)
                throw new ArgumentOutOfRangeException(nameof(step));

            var list = source.ToList();
            for (int i = 0; i <= list.Count - windowSize; i += step)
            {
                var window = new T[windowSize];
                for (int j = 0; j < windowSize; j++)
                {
                    window[j] = list[i + j];
                }
                yield return window;
            }
        }

        /// <summary>
        /// 滑动求和
        /// </summary>
        public static IEnumerable<double> SlidingSum(IEnumerable<double> source, int windowSize)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (windowSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(windowSize));

            double sum = 0;
            var queue = new Queue<double>();

            foreach (var item in source)
            {
                queue.Enqueue(item);
                sum += item;

                if (queue.Count > windowSize)
                {
                    sum -= queue.Dequeue();
                }

                if (queue.Count == windowSize)
                {
                    yield return sum;
                }
            }
        }

        /// <summary>
        /// 滑动平均
        /// </summary>
        public static IEnumerable<double> SlidingAverage(IEnumerable<double> source, int windowSize)
        {
            return SlidingSum(source, windowSize).Select(sum => sum / windowSize);
        }

        /// <summary>
        /// 滑动最大值
        /// </summary>
        public static IEnumerable<double> SlidingMax(IEnumerable<double> source, int windowSize)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (windowSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(windowSize));

            var deque = new LinkedList<int>(); // 存储索引
            var list = source.ToList();

            for (int i = 0; i < list.Count; i++)
            {
                // 移除窗口外的元素
                while (deque.Count > 0 && deque.First.Value <= i - windowSize)
                {
                    deque.RemoveFirst();
                }

                // 移除比当前元素小的元素（它们不可能是最大值）
                while (deque.Count > 0 && list[deque.Last.Value] <= list[i])
                {
                    deque.RemoveLast();
                }

                deque.AddLast(i);

                if (i >= windowSize - 1)
                {
                    yield return list[deque.First.Value];
                }
            }
        }

        /// <summary>
        /// 滑动最小值
        /// </summary>
        public static IEnumerable<double> SlidingMin(IEnumerable<double> source, int windowSize)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (windowSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(windowSize));

            var deque = new LinkedList<int>();
            var list = source.ToList();

            for (int i = 0; i < list.Count; i++)
            {
                while (deque.Count > 0 && deque.First.Value <= i - windowSize)
                {
                    deque.RemoveFirst();
                }

                while (deque.Count > 0 && list[deque.Last.Value] >= list[i])
                {
                    deque.RemoveLast();
                }

                deque.AddLast(i);

                if (i >= windowSize - 1)
                {
                    yield return list[deque.First.Value];
                }
            }
        }
    }

    /// <summary>
    /// 分块工具类
    /// </summary>
    public static class ChunkUtil
    {
        /// <summary>
        /// 将集合分块
        /// </summary>
        public static IEnumerable<List<T>> Chunk<T>(IEnumerable<T> source, int chunkSize)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (chunkSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(chunkSize));

            var chunk = new List<T>(chunkSize);
            foreach (var item in source)
            {
                chunk.Add(item);
                if (chunk.Count == chunkSize)
                {
                    yield return chunk;
                    chunk = new List<T>(chunkSize);
                }
            }

            if (chunk.Count > 0)
            {
                yield return chunk;
            }
        }

        /// <summary>
        /// 将集合分成指定数量的块
        /// </summary>
        public static List<List<T>> SplitInto<T>(IEnumerable<T> source, int chunkCount)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (chunkCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(chunkCount));

            var list = source.ToList();
            if (list.Count == 0)
                return new List<List<T>>();

            var result = new List<List<T>>();
            int baseSize = list.Count / chunkCount;
            int extra = list.Count % chunkCount;

            int index = 0;
            for (int i = 0; i < chunkCount; i++)
            {
                int size = baseSize + (i < extra ? 1 : 0);
                var chunk = new List<T>();
                for (int j = 0; j < size && index < list.Count; j++)
                {
                    chunk.Add(list[index++]);
                }
                result.Add(chunk);
            }

            return result;
        }

        /// <summary>
        /// 按条件分块
        /// </summary>
        public static IEnumerable<List<T>> ChunkBy<T>(IEnumerable<T> source, Func<T, bool> predicate)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            var chunk = new List<T>();
            foreach (var item in source)
            {
                if (predicate(item))
                {
                    if (chunk.Count > 0)
                    {
                        yield return chunk;
                        chunk = new List<T>();
                    }
                }
                else
                {
                    chunk.Add(item);
                }
            }

            if (chunk.Count > 0)
            {
                yield return chunk;
            }
        }
    }

    /// <summary>
    /// 分区工具类
    /// </summary>
    public static class PartitionUtil
    {
        /// <summary>
        /// 按谓词将集合分成两部分
        /// </summary>
        public static (List<T> True, List<T> False) Partition<T>(IEnumerable<T> source, Func<T, bool> predicate)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            var trueList = new List<T>();
            var falseList = new List<T>();

            foreach (var item in source)
            {
                if (predicate(item))
                    trueList.Add(item);
                else
                    falseList.Add(item);
            }

            return (trueList, falseList);
        }

        /// <summary>
        /// 将集合分成多个分区
        /// </summary>
        public static List<List<T>> PartitionBy<T>(IEnumerable<T> source, params Func<T, bool>[] predicates)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (predicates == null || predicates.Length == 0)
                throw new ArgumentException("At least one predicate is required");

            var result = new List<List<T>>();
            for (int i = 0; i < predicates.Length; i++)
            {
                result.Add(new List<T>());
            }
            result.Add(new List<T>()); // 默认分区（不满足任何谓词）

            foreach (var item in source)
            {
                bool matched = false;
                for (int i = 0; i < predicates.Length; i++)
                {
                    if (predicates[i](item))
                    {
                        result[i].Add(item);
                        matched = true;
                        break;
                    }
                }
                if (!matched)
                {
                    result[predicates.Length].Add(item);
                }
            }

            return result;
        }

        /// <summary>
        /// 交替分区
        /// </summary>
        public static (List<T> First, List<T> Second) Alternate<T>(IEnumerable<T> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var first = new List<T>();
            var second = new List<T>();
            bool isFirst = true;

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
        /// 按比例分割
        /// </summary>
        public static (List<T> First, List<T> Second) SplitByRatio<T>(IEnumerable<T> source, double firstRatio)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (firstRatio < 0 || firstRatio > 1)
                throw new ArgumentOutOfRangeException(nameof(firstRatio));

            var list = source.ToList();
            int firstCount = (int)(list.Count * firstRatio);

            var first = new List<T>();
            var second = new List<T>();

            for (int i = 0; i < list.Count; i++)
            {
                if (i < firstCount)
                    first.Add(list[i]);
                else
                    second.Add(list[i]);
            }

            return (first, second);
        }
    }

    /// <summary>
    /// 交错工具类
    /// </summary>
    public static class InterleaveUtil
    {
        /// <summary>
        /// 交错合并两个集合
        /// </summary>
        public static IEnumerable<T> Interleave<T>(IEnumerable<T> first, IEnumerable<T> second)
        {
            if (first == null)
                throw new ArgumentNullException(nameof(first));
            if (second == null)
                throw new ArgumentNullException(nameof(second));

            using var enum1 = first.GetEnumerator();
            using var enum2 = second.GetEnumerator();

            bool hasFirst, hasSecond;
            while (true)
            {
                hasFirst = enum1.MoveNext();
                hasSecond = enum2.MoveNext();

                if (!hasFirst && !hasSecond)
                    break;

                if (hasFirst)
                    yield return enum1.Current;
                if (hasSecond)
                    yield return enum2.Current;
            }
        }

        /// <summary>
        /// 交错合并多个集合
        /// </summary>
        public static IEnumerable<T> Interleave<T>(params IEnumerable<T>[] sources)
        {
            if (sources == null || sources.Length == 0)
                yield break;

            var enumerators = new List<IEnumerator<T>>();
            try
            {
                foreach (var source in sources)
                {
                    if (source != null)
                        enumerators.Add(source.GetEnumerator());
                }

                bool anyHasNext = true;
                while (anyHasNext)
                {
                    anyHasNext = false;
                    foreach (var e in enumerators)
                    {
                        if (e.MoveNext())
                        {
                            yield return e.Current;
                            anyHasNext = true;
                        }
                    }
                }
            }
            finally
            {
                foreach (var e in enumerators)
                {
                    e.Dispose();
                }
            }
        }

        /// <summary>
        /// 以指定元素为分隔交错
        /// </summary>
        public static IEnumerable<T> Intersperse<T>(IEnumerable<T> source, T separator)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            bool first = true;
            foreach (var item in source)
            {
                if (!first)
                    yield return separator;
                first = false;
                yield return item;
            }
        }
    }

    /// <summary>
    /// 旋转工具类
    /// </summary>
    public static class RotateUtil
    {
        /// <summary>
        /// 左旋转
        /// </summary>
        public static List<T> RotateLeft<T>(IEnumerable<T> source, int positions)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var list = source.ToList();
            if (list.Count == 0)
                return list;

            positions = positions % list.Count;
            if (positions < 0)
                positions += list.Count;

            if (positions == 0)
                return list;

            var result = new List<T>(list.Count);
            result.AddRange(list.Skip(positions));
            result.AddRange(list.Take(positions));
            return result;
        }

        /// <summary>
        /// 右旋转
        /// </summary>
        public static List<T> RotateRight<T>(IEnumerable<T> source, int positions)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var list = source.ToList();
            if (list.Count == 0)
                return list;

            positions = positions % list.Count;
            if (positions < 0)
                positions += list.Count;

            if (positions == 0)
                return list;

            return RotateLeft(list, list.Count - positions);
        }
    }

    /// <summary>
    /// 水库采样工具类
    /// </summary>
    public static class ReservoirSamplingUtil
    {
        /// <summary>
        /// 从集合中随机采样指定数量的元素
        /// </summary>
        public static List<T> Sample<T>(IEnumerable<T> source, int sampleSize, Random random = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (sampleSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(sampleSize));

            random ??= new Random();
            var reservoir = new List<T>();
            int index = 0;

            foreach (var item in source)
            {
                if (index < sampleSize)
                {
                    reservoir.Add(item);
                }
                else
                {
                    int j = random.Next(index + 1);
                    if (j < sampleSize)
                    {
                        reservoir[j] = item;
                    }
                }
                index++;
            }

            return reservoir;
        }

        /// <summary>
        /// 加权随机采样
        /// </summary>
        public static List<T> WeightedSample<T>(IEnumerable<T> source, Func<T, double> weightSelector, int sampleSize, Random random = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (weightSelector == null)
                throw new ArgumentNullException(nameof(weightSelector));
            if (sampleSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(sampleSize));

            random ??= new Random();
            var list = source.ToList();

            // 使用别名方法采样
            var weights = list.Select(weightSelector).ToList();
            double totalWeight = weights.Sum();

            var result = new List<T>();
            var used = new HashSet<int>();

            while (result.Count < sampleSize && used.Count < list.Count)
            {
                double r = random.NextDouble() * totalWeight;
                double cumulative = 0;

                for (int i = 0; i < list.Count; i++)
                {
                    if (used.Contains(i))
                        continue;

                    cumulative += weights[i];
                    if (r <= cumulative)
                    {
                        result.Add(list[i]);
                        used.Add(i);
                        totalWeight -= weights[i];
                        break;
                    }
                }
            }

            return result;
        }
    }

    /// <summary>
    /// 集合差异工具类
    /// </summary>
    public static class CollectionDiffUtil
    {
        /// <summary>
        /// 计算集合差异
        /// </summary>
        public static CollectionDiff<T> Diff<T>(IEnumerable<T> oldCollection, IEnumerable<T> newCollection)
        {
            if (oldCollection == null)
                throw new ArgumentNullException(nameof(oldCollection));
            if (newCollection == null)
                throw new ArgumentNullException(nameof(newCollection));

            var oldSet = oldCollection.ToHashSet();
            var newSet = newCollection.ToHashSet();

            var added = newSet.Except(oldSet).ToList();
            var removed = oldSet.Except(newSet).ToList();
            var unchanged = oldSet.Intersect(newSet).ToList();

            return new CollectionDiff<T>
            {
                Added = added,
                Removed = removed,
                Unchanged = unchanged
            };
        }

        /// <summary>
        /// 计算集合差异（使用键选择器）
        /// </summary>
        public static CollectionDiffByKey<T, TKey> DiffByKey<T, TKey>(
            IEnumerable<T> oldCollection,
            IEnumerable<T> newCollection,
            Func<T, TKey> keySelector) where TKey : IEquatable<TKey>
        {
            if (oldCollection == null)
                throw new ArgumentNullException(nameof(oldCollection));
            if (newCollection == null)
                throw new ArgumentNullException(nameof(newCollection));
            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));

            var oldDict = oldCollection.ToDictionary(keySelector);
            var newDict = newCollection.ToDictionary(keySelector);

            var oldKeys = oldDict.Keys.ToHashSet();
            var newKeys = newDict.Keys.ToHashSet();

            var added = newKeys.Except(oldKeys).Select(k => newDict[k]).ToList();
            var removed = oldKeys.Except(newKeys).Select(k => oldDict[k]).ToList();
            var unchanged = oldKeys.Intersect(newKeys).ToList();

            // 检测修改的项
            var modified = new List<CollectionDiffItem<T, TKey>>();
            foreach (var key in unchanged)
            {
                if (!EqualityComparer<T>.Default.Equals(oldDict[key], newDict[key]))
                {
                    modified.Add(new CollectionDiffItem<T, TKey>
                    {
                        Key = key,
                        OldValue = oldDict[key],
                        NewValue = newDict[key]
                    });
                }
            }

            return new CollectionDiffByKey<T, TKey>
            {
                Added = added,
                Removed = removed,
                Modified = modified,
                UnchangedKeys = unchanged
            };
        }

        /// <summary>
        /// 同步集合
        /// </summary>
        public static void Sync<T>(
            ICollection<T> target,
            IEnumerable<T> source,
            IEqualityComparer<T> comparer = null)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            comparer ??= EqualityComparer<T>.Default;
            var sourceSet = source.ToHashSet(comparer);

            // 移除不在源中的项
            var toRemove = target.Where(t => !sourceSet.Contains(t)).ToList();
            foreach (var item in toRemove)
            {
                target.Remove(item);
            }

            // 添加不在目标中的项
            var targetSet = target.ToHashSet(comparer);
            foreach (var item in sourceSet)
            {
                if (!targetSet.Contains(item))
                {
                    target.Add(item);
                }
            }
        }
    }

    /// <summary>
    /// 集合差异结果
    /// </summary>
    public class CollectionDiff<T>
    {
        /// <summary>
        /// 新增的元素
        /// </summary>
        public List<T> Added { get; set; }

        /// <summary>
        /// 移除的元素
        /// </summary>
        public List<T> Removed { get; set; }

        /// <summary>
        /// 未变化的元素
        /// </summary>
        public List<T> Unchanged { get; set; }

        /// <summary>
        /// 是否有变化
        /// </summary>
        public bool HasChanges => Added.Count > 0 || Removed.Count > 0;
    }

    /// <summary>
    /// 按键的集合差异结果
    /// </summary>
    public class CollectionDiffByKey<T, TKey>
    {
        /// <summary>
        /// 新增的元素
        /// </summary>
        public List<T> Added { get; set; }

        /// <summary>
        /// 移除的元素
        /// </summary>
        public List<T> Removed { get; set; }

        /// <summary>
        /// 修改的元素
        /// </summary>
        public List<CollectionDiffItem<T, TKey>> Modified { get; set; }

        /// <summary>
        /// 未变化的键
        /// </summary>
        public List<TKey> UnchangedKeys { get; set; }

        /// <summary>
        /// 是否有变化
        /// </summary>
        public bool HasChanges => Added.Count > 0 || Removed.Count > 0 || Modified.Count > 0;
    }

    /// <summary>
    /// 差异项
    /// </summary>
    public class CollectionDiffItem<T, TKey>
    {
        /// <summary>
        /// 键
        /// </summary>
        public TKey Key { get; set; }

        /// <summary>
        /// 旧值
        /// </summary>
        public T OldValue { get; set; }

        /// <summary>
        /// 新值
        /// </summary>
        public T NewValue { get; set; }
    }

    /// <summary>
    /// 加权随机工具类
    /// </summary>
    public static class WeightedRandomUtil
    {
        /// <summary>
        /// 按权重随机选择一个元素
        /// </summary>
        public static T Select<T>(IEnumerable<T> source, Func<T, double> weightSelector, Random random = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (weightSelector == null)
                throw new ArgumentNullException(nameof(weightSelector));

            random ??= new Random();
            var list = source.ToList();

            if (list.Count == 0)
                throw new ArgumentException("Collection is empty");

            var weights = list.Select(weightSelector).ToList();
            double totalWeight = weights.Sum();

            if (totalWeight <= 0)
                throw new ArgumentException("Total weight must be positive");

            double r = random.NextDouble() * totalWeight;
            double cumulative = 0;

            for (int i = 0; i < list.Count; i++)
            {
                cumulative += weights[i];
                if (r <= cumulative)
                {
                    return list[i];
                }
            }

            return list[list.Count - 1];
        }

        /// <summary>
        /// 按权重随机选择多个元素（不放回）
        /// </summary>
        public static List<T> SelectMultiple<T>(IEnumerable<T> source, Func<T, double> weightSelector, int count, Random random = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (weightSelector == null)
                throw new ArgumentNullException(nameof(weightSelector));
            if (count <= 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            random ??= new Random();
            var list = source.ToList();
            var weights = list.Select(weightSelector).ToList();
            var result = new List<T>();
            var selected = new HashSet<int>();

            while (result.Count < count && selected.Count < list.Count)
            {
                double totalWeight = 0;
                for (int i = 0; i < list.Count; i++)
                {
                    if (!selected.Contains(i))
                        totalWeight += weights[i];
                }

                if (totalWeight <= 0)
                    break;

                double r = random.NextDouble() * totalWeight;
                double cumulative = 0;

                for (int i = 0; i < list.Count; i++)
                {
                    if (selected.Contains(i))
                        continue;

                    cumulative += weights[i];
                    if (r <= cumulative)
                    {
                        result.Add(list[i]);
                        selected.Add(i);
                        break;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 创建别名表以进行高效加权随机采样
        /// </summary>
        public static AliasTable<T> CreateAliasTable<T>(IEnumerable<T> source, Func<T, double> weightSelector)
        {
            return new AliasTable<T>(source, weightSelector);
        }
    }

    /// <summary>
    /// 别名表（用于 O(1) 加权随机采样）
    /// </summary>
    public class AliasTable<T>
    {
        private readonly T[] _items;
        private readonly double[] _prob;
        private readonly int[] _alias;
        private readonly Random _random;

        /// <summary>
        /// 创建别名表
        /// </summary>
        public AliasTable(IEnumerable<T> source, Func<T, double> weightSelector)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (weightSelector == null)
                throw new ArgumentNullException(nameof(weightSelector));

            _items = source.ToArray();
            if (_items.Length == 0)
                throw new ArgumentException("Collection is empty");

            int n = _items.Length;
            var weights = _items.Select(weightSelector).ToArray();
            double totalWeight = weights.Sum();

            _prob = new double[n];
            _alias = new int[n];
            _random = new Random();

            // 归一化权重
            for (int i = 0; i < n; i++)
            {
                _prob[i] = weights[i] * n / totalWeight;
            }

            var small = new Stack<int>();
            var large = new Stack<int>();

            for (int i = 0; i < n; i++)
            {
                if (_prob[i] < 1.0)
                    small.Push(i);
                else
                    large.Push(i);
            }

            while (small.Count > 0 && large.Count > 0)
            {
                int l = small.Pop();
                int g = large.Pop();

                _alias[l] = g;
                _prob[g] = _prob[g] + _prob[l] - 1.0;

                if (_prob[g] < 1.0)
                    small.Push(g);
                else
                    large.Push(g);
            }

            while (large.Count > 0)
            {
                _prob[large.Pop()] = 1.0;
            }

            while (small.Count > 0)
            {
                _prob[small.Pop()] = 1.0;
            }
        }

        /// <summary>
        /// 随机选择一个元素
        /// </summary>
        public T Next()
        {
            int i = _random.Next(_items.Length);
            return _random.NextDouble() < _prob[i] ? _items[i] : _items[_alias[i]];
        }

        /// <summary>
        /// 随机选择多个元素（可能重复）
        /// </summary>
        public List<T> NextMultiple(int count)
        {
            var result = new List<T>(count);
            for (int i = 0; i < count; i++)
            {
                result.Add(Next());
            }
            return result;
        }
    }
}
