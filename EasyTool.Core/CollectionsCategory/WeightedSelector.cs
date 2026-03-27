using System;
using System.Collections.Generic;
using System.Threading;

namespace EasyTool.CollectionsCategory
{
    /// <summary>
    /// 带权重的选择器
    /// 根据权重随机选择元素
    /// </summary>
    /// <typeparam name="T">元素类型</typeparam>
    public class WeightedSelector<T>
    {
        private readonly List<WeightedItem<T>> _items = new();
        private readonly Random _random;
        private double _totalWeight;
        private readonly object _lock = new();

        /// <summary>
        /// 创建权重选择器
        /// </summary>
        public WeightedSelector()
        {
            _random = new Random();
            _totalWeight = 0;
        }

        /// <summary>
        /// 创建权重选择器（指定随机种子）
        /// </summary>
        public WeightedSelector(int seed)
        {
            _random = new Random(seed);
            _totalWeight = 0;
        }

        /// <summary>
        /// 元素数量
        /// </summary>
        public int Count => _items.Count;

        /// <summary>
        /// 是否为空
        /// </summary>
        public bool IsEmpty => _items.Count == 0;

        /// <summary>
        /// 总权重
        /// </summary>
        public double TotalWeight => _totalWeight;

        /// <summary>
        /// 添加元素
        /// </summary>
        /// <param name="item">元素</param>
        /// <param name="weight">权重（必须大于0）</param>
        public void Add(T item, double weight)
        {
            if (weight <= 0)
                throw new ArgumentException("权重必须大于0", nameof(weight));

            lock (_lock)
            {
                _items.Add(new WeightedItem<T>(item, weight, _totalWeight));
                _totalWeight += weight;
            }
        }

        /// <summary>
        /// 添加多个元素
        /// </summary>
        public void AddRange(IEnumerable<(T Item, double Weight)> items)
        {
            foreach (var (item, weight) in items)
            {
                Add(item, weight);
            }
        }

        /// <summary>
        /// 移除元素
        /// </summary>
        public bool Remove(T item)
        {
            lock (_lock)
            {
                var index = _items.FindIndex(i => EqualityComparer<T>.Default.Equals(i.Item, item));
                if (index < 0)
                    return false;

                var removed = _items[index];
                _items.RemoveAt(index);
                _totalWeight -= removed.Weight;

                // 重新计算累计权重
                var cumulative = 0.0;
                foreach (var i in _items)
                {
                    cumulative += i.Weight;
                }

                return true;
            }
        }

        /// <summary>
        /// 清空所有元素
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                _items.Clear();
                _totalWeight = 0;
            }
        }

        /// <summary>
        /// 根据权重随机选择一个元素
        /// </summary>
        public T? Select()
        {
            lock (_lock)
            {
                if (_items.Count == 0)
                    return default;

                var value = _random.NextDouble() * _totalWeight;

                foreach (var item in _items)
                {
                    if (value < item.CumulativeWeight + item.Weight)
                        return item.Item;
                }

                return _items[^1].Item;
            }
        }

        /// <summary>
        /// 根据权重随机选择多个元素（可重复）
        /// </summary>
        public List<T> SelectMultiple(int count)
        {
            var result = new List<T>();
            for (int i = 0; i < count; i++)
            {
                var item = Select();
                if (item != null)
                    result.Add(item);
            }
            return result;
        }

        /// <summary>
        /// 根据权重随机选择多个不重复元素
        /// </summary>
        public List<T> SelectDistinct(int count)
        {
            lock (_lock)
            {
                if (count >= _items.Count)
                    return _items.ConvertAll(i => i.Item);

                var result = new List<T>();
                var tempItems = new List<WeightedItem<T>>(_items);
                var tempTotalWeight = _totalWeight;

                while (result.Count < count && tempItems.Count > 0)
                {
                    var value = _random.NextDouble() * tempTotalWeight;
                    double cumulative = 0;

                    for (int i = 0; i < tempItems.Count; i++)
                    {
                        cumulative += tempItems[i].Weight;
                        if (value < cumulative)
                        {
                            result.Add(tempItems[i].Item);
                            tempTotalWeight -= tempItems[i].Weight;
                            tempItems.RemoveAt(i);
                            break;
                        }
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// 获取元素权重
        /// </summary>
        public double GetWeight(T item)
        {
            var found = _items.Find(i => EqualityComparer<T>.Default.Equals(i.Item, item));
            return found?.Weight ?? 0;
        }

        /// <summary>
        /// 设置元素权重
        /// </summary>
        public bool SetWeight(T item, double newWeight)
        {
            if (newWeight <= 0)
                throw new ArgumentException("权重必须大于0", nameof(newWeight));

            lock (_lock)
            {
                var index = _items.FindIndex(i => EqualityComparer<T>.Default.Equals(i.Item, item));
                if (index < 0)
                    return false;

                var oldWeight = _items[index].Weight;
                _totalWeight = _totalWeight - oldWeight + newWeight;

                var cumulative = 0.0;
                foreach (var i in _items)
                {
                    if (i == _items[index])
                    {
                        _items[index] = new WeightedItem<T>(item, newWeight, cumulative);
                        cumulative += newWeight;
                    }
                    else
                    {
                        cumulative += i.Weight;
                    }
                }

                return true;
            }
        }

        /// <summary>
        /// 获取选择概率
        /// </summary>
        public double GetProbability(T item)
        {
            if (_totalWeight == 0)
                return 0;

            var weight = GetWeight(item);
            return weight / _totalWeight;
        }

        /// <summary>
        /// 获取所有元素及其权重
        /// </summary>
        public IEnumerable<(T Item, double Weight, double Probability)> GetAll()
        {
            lock (_lock)
            {
                foreach (var item in _items)
                {
                    var probability = _totalWeight > 0 ? item.Weight / _totalWeight : 0;
                    yield return (item.Item, item.Weight, probability);
                }
            }
        }
    }

    /// <summary>
    /// 带权重的元素
    /// </summary>
    internal class WeightedItem<T>
    {
        public T Item { get; }
        public double Weight { get; }
        public double CumulativeWeight { get; }

        public WeightedItem(T item, double weight, double cumulativeWeight)
        {
            Item = item;
            Weight = weight;
            CumulativeWeight = cumulativeWeight;
        }
    }
}
