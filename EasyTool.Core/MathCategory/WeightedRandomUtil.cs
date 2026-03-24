using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace EasyTool.MathCategory
{
    /// <summary>
    /// 加权随机选择工具类
    /// 根据权重随机选择元素
    /// </summary>
    public static class WeightedRandomUtil
    {
#if NET6_0_OR_GREATER
        private static Random SharedRandom => Random.Shared;
#else
        private static readonly ThreadLocal<Random> ThreadLocalRandom = new(() => new Random(Guid.NewGuid().GetHashCode()));
        private static Random SharedRandom => ThreadLocalRandom.Value!;
#endif

        /// <summary>
        /// 创建加权随机选择器
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <returns>加权随机选择器构建器</returns>
        public static WeightedRandomBuilder<T> CreateBuilder<T>()
        {
            return new WeightedRandomBuilder<T>();
        }

        /// <summary>
        /// 从字典中按权重随机选择
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="items">元素和权重的字典</param>
        /// <returns>随机选中的元素</returns>
        public static T Select<T>(IDictionary<T, double> items)
        {
            if (items == null || items.Count == 0)
                throw new ArgumentException("元素集合不能为空");

            var totalWeight = items.Values.Sum();
            var random = SharedRandom.NextDouble() * totalWeight;

            double cumulative = 0;
            foreach (var kvp in items)
            {
                cumulative += kvp.Value;
                if (random < cumulative)
                    return kvp.Key;
            }

            return items.Last().Key;
        }

        /// <summary>
        /// 从列表中按权重随机选择
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="items">元素列表</param>
        /// <param name="weightSelector">权重选择器</param>
        /// <returns>随机选中的元素</returns>
        public static T Select<T>(IEnumerable<T> items, Func<T, double> weightSelector)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            var itemList = items.ToList();
            if (itemList.Count == 0)
                throw new ArgumentException("元素集合不能为空");

            var totalWeight = itemList.Sum(weightSelector);
            var random = SharedRandom.NextDouble() * totalWeight;

            double cumulative = 0;
            foreach (var item in itemList)
            {
                cumulative += weightSelector(item);
                if (random < cumulative)
                    return item;
            }

            return itemList.Last();
        }

        /// <summary>
        /// 按权重随机选择多个元素（可重复）
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="items">元素和权重的字典</param>
        /// <param name="count">选择数量</param>
        /// <returns>随机选中的元素列表</returns>
        public static List<T> SelectMany<T>(IDictionary<T, double> items, int count)
        {
            var result = new List<T>();
            for (int i = 0; i < count; i++)
            {
                result.Add(Select(items));
            }
            return result;
        }

        /// <summary>
        /// 按权重随机选择多个不重复元素
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="items">元素和权重的字典</param>
        /// <param name="count">选择数量</param>
        /// <returns>随机选中的元素列表</returns>
        public static List<T> SelectDistinct<T>(IDictionary<T, double> items, int count)
        {
            if (items == null || items.Count == 0)
                throw new ArgumentException("元素集合不能为空");

            count = Math.Min(count, items.Count);

            var remaining = new Dictionary<T, double>(items);
            var result = new List<T>();

            for (int i = 0; i < count; i++)
            {
                var selected = Select(remaining);
                result.Add(selected);
                remaining.Remove(selected);
            }

            return result;
        }

        /// <summary>
        /// 使用别名方法进行O(1)时间复杂度的加权随机选择
        /// 适用于元素数量多、需要频繁选择的场景
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="items">元素和权重的字典</param>
        /// <returns>别名方法选择器</returns>
        public static AliasMethodSelector<T> CreateAliasSelector<T>(IDictionary<T, double> items)
        {
            return new AliasMethodSelector<T>(items);
        }
    }

    /// <summary>
    /// 加权随机选择器构建器
    /// </summary>
    /// <typeparam name="T">元素类型</typeparam>
    public class WeightedRandomBuilder<T>
    {
        private readonly Dictionary<T, double> _items = new();

        /// <summary>
        /// 添加元素
        /// </summary>
        /// <param name="item">元素</param>
        /// <param name="weight">权重</param>
        /// <returns>构建器</returns>
        public WeightedRandomBuilder<T> Add(T item, double weight)
        {
            if (weight < 0)
                throw new ArgumentOutOfRangeException(nameof(weight), "权重不能为负数");

            _items[item] = weight;
            return this;
        }

        /// <summary>
        /// 添加多个元素
        /// </summary>
        /// <param name="items">元素和权重</param>
        /// <returns>构建器</returns>
        public WeightedRandomBuilder<T> AddRange(IDictionary<T, double> items)
        {
            foreach (var kvp in items)
            {
                _items[kvp.Key] = kvp.Value;
            }
            return this;
        }

        /// <summary>
        /// 构建选择器
        /// </summary>
        /// <returns>选择器</returns>
        public Func<T> Build()
        {
            if (_items.Count == 0)
                throw new InvalidOperationException("没有添加任何元素");

            var items = new Dictionary<T, double>(_items);
            return () => WeightedRandomUtil.Select(items);
        }

        /// <summary>
        /// 构建别名方法选择器（高性能）
        /// </summary>
        /// <returns>别名方法选择器</returns>
        public AliasMethodSelector<T> BuildAliasSelector()
        {
            if (_items.Count == 0)
                throw new InvalidOperationException("没有添加任何元素");

            return new AliasMethodSelector<T>(_items);
        }
    }

    /// <summary>
    /// 别名方法选择器（O(1)时间复杂度）
    /// </summary>
    /// <typeparam name="T">元素类型</typeparam>
    public class AliasMethodSelector<T>
    {
#if NET6_0_OR_GREATER
        private static Random SharedRandom => Random.Shared;
#else
        private static readonly ThreadLocal<Random> ThreadLocalRandom = new(() => new Random(Guid.NewGuid().GetHashCode()));
        private static Random GetSharedRandom() => ThreadLocalRandom.Value!;
#endif

        private readonly T[] _items;
        private readonly double[] _probabilities;
        private readonly int[] _alias;
        private readonly int _count;

        public AliasMethodSelector(IDictionary<T, double> items)
        {
            if (items == null || items.Count == 0)
                throw new ArgumentException("元素集合不能为空");

            _count = items.Count;
            _items = items.Keys.ToArray();
            _probabilities = new double[_count];
            _alias = new int[_count];

            Initialize(items.Values.ToArray());
        }

        private void Initialize(double[] weights)
        {
            var totalWeight = weights.Sum();
            var scale = _count / totalWeight;

            // 标准化权重
            var scaledWeights = weights.Select(w => w * scale).ToArray();

            var small = new Queue<int>();
            var large = new Queue<int>();

            for (int i = 0; i < _count; i++)
            {
                if (scaledWeights[i] < 1.0)
                    small.Enqueue(i);
                else
                    large.Enqueue(i);
            }

            while (small.Count > 0 && large.Count > 0)
            {
                var smallIndex = small.Dequeue();
                var largeIndex = large.Dequeue();

                _probabilities[smallIndex] = scaledWeights[smallIndex];
                _alias[smallIndex] = largeIndex;

                scaledWeights[largeIndex] = scaledWeights[largeIndex] + scaledWeights[smallIndex] - 1.0;

                if (scaledWeights[largeIndex] < 1.0)
                    small.Enqueue(largeIndex);
                else
                    large.Enqueue(largeIndex);
            }

            while (large.Count > 0)
            {
                _probabilities[large.Dequeue()] = 1.0;
            }

            while (small.Count > 0)
            {
                _probabilities[small.Dequeue()] = 1.0;
            }
        }

        /// <summary>
        /// 随机选择一个元素
        /// </summary>
        /// <returns>选中的元素</returns>
        public T Select()
        {
#if NET6_0_OR_GREATER
            var index = SharedRandom.Next(_count);
            var r = SharedRandom.NextDouble();
#else
            var random = GetSharedRandom();
            var index = random.Next(_count);
            var r = random.NextDouble();
#endif

            if (r < _probabilities[index])
                return _items[index];
            else
                return _items[_alias[index]];
        }

        /// <summary>
        /// 选择多个元素
        /// </summary>
        /// <param name="count">数量</param>
        /// <returns>选中的元素列表</returns>
        public List<T> SelectMany(int count)
        {
            var result = new List<T>(count);
            for (int i = 0; i < count; i++)
            {
                result.Add(Select());
            }
            return result;
        }
    }
}
