using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyTool.CollectionsCategory
{
    /// <summary>
    /// 复合字典工具类
    /// </summary>
    public static class MultiKeyDictionaryUtil
    {
        /// <summary>
        /// 创建双键字典
        /// </summary>
        public static TwoKeyDictionary<TKey1, TKey2, TValue> CreateTwoKey<TKey1, TKey2, TValue>()
            where TKey1 : notnull
            where TKey2 : notnull
        {
            return new TwoKeyDictionary<TKey1, TKey2, TValue>();
        }

        /// <summary>
        /// 创建复合键字典
        /// </summary>
        public static CompositeKeyDictionary<TKey, TValue> CreateComposite<TKey, TValue>()
            where TKey : notnull
        {
            return new CompositeKeyDictionary<TKey, TValue>();
        }

        /// <summary>
        /// 创建区间映射
        /// </summary>
        public static RangeMap<T> CreateRangeMap<T>() where T : IComparable<T>
        {
            return new RangeMap<T>();
        }
    }

    /// <summary>
    /// 双键字典
    /// 通过两个键可以分别查找值
    /// </summary>
    public class TwoKeyDictionary<TKey1, TKey2, TValue>
        where TKey1 : notnull
        where TKey2 : notnull
    {
        private readonly Dictionary<TKey1, Dictionary<TKey2, TValue>> _data;
        private readonly Dictionary<TKey2, Dictionary<TKey1, TValue>> _reverseData;
        private int _count;

        /// <summary>
        /// 元素数量
        /// </summary>
        public int Count => _count;

        /// <summary>
        /// 第一个键的集合
        /// </summary>
        public ICollection<TKey1> Keys1 => _data.Keys;

        /// <summary>
        /// 第二个键的集合
        /// </summary>
        public ICollection<TKey2> Keys2 => _reverseData.Keys;

        /// <summary>
        /// 通过第一个键访问
        /// </summary>
        public Dictionary<TKey2, TValue> this[TKey1 key1]
        {
            get
            {
                if (!_data.TryGetValue(key1, out var inner))
                {
                    inner = new Dictionary<TKey2, TValue>();
                    _data[key1] = inner;
                }
                return inner;
            }
        }

        /// <summary>
        /// 创建双键字典
        /// </summary>
        public TwoKeyDictionary()
        {
            _data = new Dictionary<TKey1, Dictionary<TKey2, TValue>>();
            _reverseData = new Dictionary<TKey2, Dictionary<TKey1, TValue>>();
            _count = 0;
        }

        /// <summary>
        /// 添加元素
        /// </summary>
        public void Add(TKey1 key1, TKey2 key2, TValue value)
        {
            if (!_data.TryGetValue(key1, out var inner1))
            {
                inner1 = new Dictionary<TKey2, TValue>();
                _data[key1] = inner1;
            }

            if (!inner1.ContainsKey(key2))
            {
                _count++;
            }

            inner1[key2] = value;

            if (!_reverseData.TryGetValue(key2, out var inner2))
            {
                inner2 = new Dictionary<TKey1, TValue>();
                _reverseData[key2] = inner2;
            }
            inner2[key1] = value;
        }

        /// <summary>
        /// 通过双键获取值
        /// </summary>
        public bool TryGetValue(TKey1 key1, TKey2 key2, out TValue value)
        {
            value = default;
            if (!_data.TryGetValue(key1, out var inner))
                return false;
            return inner.TryGetValue(key2, out value);
        }

        /// <summary>
        /// 通过双键获取值
        /// </summary>
        public TValue GetValue(TKey1 key1, TKey2 key2)
        {
            if (!TryGetValue(key1, key2, out var value))
                throw new KeyNotFoundException($"Key pair ({key1}, {key2}) not found");
            return value;
        }

        /// <summary>
        /// 通过第一个键获取所有值
        /// </summary>
        public bool TryGetByKey1(TKey1 key1, out Dictionary<TKey2, TValue> values)
        {
            return _data.TryGetValue(key1, out values);
        }

        /// <summary>
        /// 通过第二个键获取所有值
        /// </summary>
        public bool TryGetByKey2(TKey2 key2, out Dictionary<TKey1, TValue> values)
        {
            return _reverseData.TryGetValue(key2, out values);
        }

        /// <summary>
        /// 移除元素
        /// </summary>
        public bool Remove(TKey1 key1, TKey2 key2)
        {
            if (!_data.TryGetValue(key1, out var inner1))
                return false;

            if (!inner1.Remove(key2))
                return false;

            _count--;

            if (inner1.Count == 0)
                _data.Remove(key1);

            if (_reverseData.TryGetValue(key2, out var inner2))
            {
                inner2.Remove(key1);
                if (inner2.Count == 0)
                    _reverseData.Remove(key2);
            }

            return true;
        }

        /// <summary>
        /// 移除第一个键的所有元素
        /// </summary>
        public bool RemoveByKey1(TKey1 key1)
        {
            if (!_data.TryGetValue(key1, out var inner))
                return false;

            _count -= inner.Count;

            foreach (var key2 in inner.Keys)
            {
                if (_reverseData.TryGetValue(key2, out var reverseInner))
                {
                    reverseInner.Remove(key1);
                    if (reverseInner.Count == 0)
                        _reverseData.Remove(key2);
                }
            }

            _data.Remove(key1);
            return true;
        }

        /// <summary>
        /// 移除第二个键的所有元素
        /// </summary>
        public bool RemoveByKey2(TKey2 key2)
        {
            if (!_reverseData.TryGetValue(key2, out var inner))
                return false;

            _count -= inner.Count;

            foreach (var key1 in inner.Keys)
            {
                if (_data.TryGetValue(key1, out var forwardInner))
                {
                    forwardInner.Remove(key2);
                    if (forwardInner.Count == 0)
                        _data.Remove(key1);
                }
            }

            _reverseData.Remove(key2);
            return true;
        }

        /// <summary>
        /// 是否包含键对
        /// </summary>
        public bool ContainsKey(TKey1 key1, TKey2 key2)
        {
            return _data.TryGetValue(key1, out var inner) && inner.ContainsKey(key2);
        }

        /// <summary>
        /// 是否包含第一个键
        /// </summary>
        public bool ContainsKey1(TKey1 key1)
        {
            return _data.ContainsKey(key1);
        }

        /// <summary>
        /// 是否包含第二个键
        /// </summary>
        public bool ContainsKey2(TKey2 key2)
        {
            return _reverseData.ContainsKey(key2);
        }

        /// <summary>
        /// 清空
        /// </summary>
        public void Clear()
        {
            _data.Clear();
            _reverseData.Clear();
            _count = 0;
        }

        /// <summary>
        /// 获取所有键值对
        /// </summary>
        public IEnumerable<(TKey1 Key1, TKey2 Key2, TValue Value)> GetAll()
        {
            foreach (var kvp1 in _data)
            {
                foreach (var kvp2 in kvp1.Value)
                {
                    yield return (kvp1.Key, kvp2.Key, kvp2.Value);
                }
            }
        }
    }

    /// <summary>
    /// 复合键字典
    /// 使用多个键组成的元组作为键
    /// </summary>
    public class CompositeKeyDictionary<TKey, TValue> where TKey : notnull
    {
        private readonly Dictionary<TKey[], TValue> _data;
        private readonly IEqualityComparer<TKey> _keyComparer;
        private readonly List<Dictionary<TKey, List<TKey[]>>> _indexes;

        /// <summary>
        /// 元素数量
        /// </summary>
        public int Count => _data.Count;

        /// <summary>
        /// 创建复合键字典
        /// </summary>
        public CompositeKeyDictionary()
        {
            _data = new Dictionary<TKey[], TValue>(new ArrayEqualityComparer<TKey>());
            _keyComparer = EqualityComparer<TKey>.Default;
            _indexes = new List<Dictionary<TKey, List<TKey[]>>>();
        }

        /// <summary>
        /// 创建具有指定键数量的复合键字典
        /// </summary>
        public CompositeKeyDictionary(int keyCount) : this()
        {
            for (int i = 0; i < keyCount; i++)
            {
                _indexes.Add(new Dictionary<TKey, List<TKey[]>>());
            }
        }

        /// <summary>
        /// 添加元素
        /// </summary>
        public void Add(TValue value, params TKey[] keys)
        {
            if (keys == null || keys.Length == 0)
                throw new ArgumentException("At least one key is required");

            _data[keys] = value;

            // 建立索引
            while (_indexes.Count < keys.Length)
            {
                _indexes.Add(new Dictionary<TKey, List<TKey[]>>());
            }

            for (int i = 0; i < keys.Length; i++)
            {
                var index = _indexes[i];
                if (!index.TryGetValue(keys[i], out var list))
                {
                    list = new List<TKey[]>();
                    index[keys[i]] = list;
                }
                list.Add(keys);
            }
        }

        /// <summary>
        /// 获取值
        /// </summary>
        public bool TryGetValue(out TValue value, params TKey[] keys)
        {
            return _data.TryGetValue(keys, out value);
        }

        /// <summary>
        /// 获取值
        /// </summary>
        public TValue Get(params TKey[] keys)
        {
            if (!_data.TryGetValue(keys, out var value))
                throw new KeyNotFoundException();
            return value;
        }

        /// <summary>
        /// 通过部分键查找
        /// </summary>
        public List<TValue> FindByKey(int keyIndex, TKey key)
        {
            var result = new List<TValue>();

            if (keyIndex < 0 || keyIndex >= _indexes.Count)
                return result;

            if (!_indexes[keyIndex].TryGetValue(key, out var keyLists))
                return result;

            foreach (var keys in keyLists)
            {
                if (_data.TryGetValue(keys, out var value))
                {
                    result.Add(value);
                }
            }

            return result;
        }

        /// <summary>
        /// 移除
        /// </summary>
        public bool Remove(params TKey[] keys)
        {
            if (!_data.Remove(keys))
                return false;

            for (int i = 0; i < keys.Length && i < _indexes.Count; i++)
            {
                if (_indexes[i].TryGetValue(keys[i], out var list))
                {
                    list.RemoveAll(k => KeysEqual(k, keys));
                    if (list.Count == 0)
                        _indexes[i].Remove(keys[i]);
                }
            }

            return true;
        }

        /// <summary>
        /// 是否包含键
        /// </summary>
        public bool ContainsKey(params TKey[] keys)
        {
            return _data.ContainsKey(keys);
        }

        /// <summary>
        /// 清空
        /// </summary>
        public void Clear()
        {
            _data.Clear();
            foreach (var index in _indexes)
            {
                index.Clear();
            }
        }

        private bool KeysEqual(TKey[] a, TKey[] b)
        {
            if (a.Length != b.Length)
                return false;
            for (int i = 0; i < a.Length; i++)
            {
                if (!_keyComparer.Equals(a[i], b[i]))
                    return false;
            }
            return true;
        }

        private class ArrayEqualityComparer<T> : IEqualityComparer<T[]>
        {
            public bool Equals(T[] x, T[] y)
            {
                if (x == null || y == null)
                    return x == y;
                if (x.Length != y.Length)
                    return false;
                var comparer = EqualityComparer<T>.Default;
                for (int i = 0; i < x.Length; i++)
                {
                    if (!comparer.Equals(x[i], y[i]))
                        return false;
                }
                return true;
            }

            public int GetHashCode(T[] obj)
            {
                if (obj == null)
                    return 0;
                int hash = 17;
                var comparer = EqualityComparer<T>.Default;
                foreach (var item in obj)
                {
                    hash = hash * 31 + (item == null ? 0 : comparer.GetHashCode(item));
                }
                return hash;
            }
        }
    }

    /// <summary>
    /// 区间映射
    /// 将键范围映射到值
    /// </summary>
    public class RangeMap<T> where T : IComparable<T>
    {
        private readonly List<RangeEntry> _entries;

        private class RangeEntry
        {
            public T Min { get; set; }
            public T Max { get; set; }
            public object Value { get; set; }
            public bool MinInclusive { get; set; }
            public bool MaxInclusive { get; set; }
        }

        /// <summary>
        /// 区间数量
        /// </summary>
        public int Count => _entries.Count;

        /// <summary>
        /// 创建区间映射
        /// </summary>
        public RangeMap()
        {
            _entries = new List<RangeEntry>();
        }

        /// <summary>
        /// 添加区间映射（闭区间）
        /// </summary>
        public void Add(T min, T max, object value)
        {
            Add(min, max, value, true, true);
        }

        /// <summary>
        /// 添加区间映射
        /// </summary>
        public void Add(T min, T max, object value, bool minInclusive, bool maxInclusive)
        {
            if (min.CompareTo(max) > 0)
                throw new ArgumentException("Min must be less than or equal to max");

            _entries.Add(new RangeEntry
            {
                Min = min,
                Max = max,
                Value = value,
                MinInclusive = minInclusive,
                MaxInclusive = maxInclusive
            });
        }

        /// <summary>
        /// 添加单点映射
        /// </summary>
        public void Add(T point, object value)
        {
            Add(point, point, value, true, true);
        }

        /// <summary>
        /// 添加无穷下界区间
        /// </summary>
        public void AddBelow(T max, object value, bool inclusive = false)
        {
            _entries.Add(new RangeEntry
            {
                Min = default,
                Max = max,
                Value = value,
                MinInclusive = false,
                MaxInclusive = inclusive
            });
        }

        /// <summary>
        /// 添加无穷上界区间
        /// </summary>
        public void AddAbove(T min, object value, bool inclusive = false)
        {
            _entries.Add(new RangeEntry
            {
                Min = min,
                Max = default,
                Value = value,
                MinInclusive = inclusive,
                MaxInclusive = false
            });
        }

        /// <summary>
        /// 查找值
        /// </summary>
        public object Find(T key)
        {
            foreach (var entry in _entries)
            {
                if (Contains(entry, key))
                    return entry.Value;
            }
            return null;
        }

        /// <summary>
        /// 查找所有匹配值
        /// </summary>
        public List<object> FindAll(T key)
        {
            var result = new List<object>();
            foreach (var entry in _entries)
            {
                if (Contains(entry, key))
                    result.Add(entry.Value);
            }
            return result;
        }

        /// <summary>
        /// 泛型查找
        /// </summary>
        public TValue Find<TValue>(T key)
        {
            var result = Find(key);
            return result == null ? default : (TValue)result;
        }

        private bool Contains(RangeEntry entry, T key)
        {
            int minCmp = entry.Min == null || entry.Min.Equals(default) ? -1 : key.CompareTo(entry.Min);
            int maxCmp = entry.Max == null || entry.Max.Equals(default) ? 1 : key.CompareTo(entry.Max);

            bool minOk = entry.MinInclusive ? minCmp >= 0 : minCmp > 0;
            bool maxOk = entry.MaxInclusive ? maxCmp <= 0 : maxCmp < 0;

            return minOk && maxOk;
        }

        /// <summary>
        /// 移除区间
        /// </summary>
        public bool Remove(T min, T max)
        {
            return _entries.RemoveAll(e =>
                e.Min.CompareTo(min) == 0 && e.Max.CompareTo(max) == 0) > 0;
        }

        /// <summary>
        /// 清空
        /// </summary>
        public void Clear()
        {
            _entries.Clear();
        }

        /// <summary>
        /// 获取所有区间
        /// </summary>
        public IEnumerable<(T Min, T Max, object Value)> GetAllRanges()
        {
            return _entries.Select(e => (e.Min, e.Max, e.Value));
        }
    }

    /// <summary>
    /// 类型化区间映射
    /// </summary>
    public class RangeMap<T, TValue> where T : IComparable<T>
    {
        private readonly List<RangeEntry> _entries;

        private class RangeEntry
        {
            public T Min { get; set; }
            public T Max { get; set; }
            public TValue Value { get; set; }
            public bool MinInclusive { get; set; }
            public bool MaxInclusive { get; set; }
        }

        /// <summary>
        /// 区间数量
        /// </summary>
        public int Count => _entries.Count;

        /// <summary>
        /// 创建区间映射
        /// </summary>
        public RangeMap()
        {
            _entries = new List<RangeEntry>();
        }

        /// <summary>
        /// 添加区间映射
        /// </summary>
        public void Add(T min, T max, TValue value)
        {
            Add(min, max, value, true, true);
        }

        /// <summary>
        /// 添加区间映射
        /// </summary>
        public void Add(T min, T max, TValue value, bool minInclusive, bool maxInclusive)
        {
            if (min.CompareTo(max) > 0)
                throw new ArgumentException("Min must be less than or equal to max");

            _entries.Add(new RangeEntry
            {
                Min = min,
                Max = max,
                Value = value,
                MinInclusive = minInclusive,
                MaxInclusive = maxInclusive
            });
        }

        /// <summary>
        /// 查找值
        /// </summary>
        public TValue Find(T key)
        {
            foreach (var entry in _entries)
            {
                if (Contains(entry, key))
                    return entry.Value;
            }
            return default;
        }

        /// <summary>
        /// 查找所有匹配值
        /// </summary>
        public List<TValue> FindAll(T key)
        {
            var result = new List<TValue>();
            foreach (var entry in _entries)
            {
                if (Contains(entry, key))
                    result.Add(entry.Value);
            }
            return result;
        }

        /// <summary>
        /// 尝试查找
        /// </summary>
        public bool TryFind(T key, out TValue value)
        {
            value = Find(key);
            return !EqualityComparer<TValue>.Default.Equals(value, default);
        }

        private bool Contains(RangeEntry entry, T key)
        {
            int minCmp = key.CompareTo(entry.Min);
            int maxCmp = key.CompareTo(entry.Max);

            bool minOk = entry.MinInclusive ? minCmp >= 0 : minCmp > 0;
            bool maxOk = entry.MaxInclusive ? maxCmp <= 0 : maxCmp < 0;

            return minOk && maxOk;
        }

        /// <summary>
        /// 清空
        /// </summary>
        public void Clear()
        {
            _entries.Clear();
        }
    }
}
