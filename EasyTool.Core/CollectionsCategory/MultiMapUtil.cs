using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyTool.CollectionsCategory
{
    /// <summary>
    /// 多值字典工具类
    /// 一个键可以对应多个值
    /// </summary>
    public static class MultiMapUtil
    {
        /// <summary>
        /// 创建多值字典
        /// </summary>
        public static MultiMap<TKey, TValue> Create<TKey, TValue>()
        {
            return new MultiMap<TKey, TValue>();
        }

        /// <summary>
        /// 创建多值字典（使用指定的集合工厂）
        /// </summary>
        public static MultiMap<TKey, TValue> Create<TKey, TValue>(Func<ICollection<TValue>> collectionFactory)
        {
            return new MultiMap<TKey, TValue>(collectionFactory);
        }
    }

    /// <summary>
    /// 多值字典实现
    /// </summary>
    /// <typeparam name="TKey">键类型</typeparam>
    /// <typeparam name="TValue">值类型</typeparam>
    public class MultiMap<TKey, TValue>
    {
        private readonly Dictionary<TKey, ICollection<TValue>> _dictionary;
        private readonly Func<ICollection<TValue>> _collectionFactory;
        private int _valueCount;

        /// <summary>
        /// 键数量
        /// </summary>
        public int KeyCount => _dictionary.Count;

        /// <summary>
        /// 值总数
        /// </summary>
        public int ValueCount => _valueCount;

        /// <summary>
        /// 所有键
        /// </summary>
        public ICollection<TKey> Keys => _dictionary.Keys;

        /// <summary>
        /// 获取指定键的所有值
        /// </summary>
        public ICollection<TValue> this[TKey key]
        {
            get
            {
                if (_dictionary.TryGetValue(key, out var values))
                    return values;
                return new List<TValue>();
            }
        }

        /// <summary>
        /// 创建多值字典（默认使用 List）
        /// </summary>
        public MultiMap() : this(() => new List<TValue>()) { }

        /// <summary>
        /// 创建多值字典（使用指定集合工厂）
        /// </summary>
        public MultiMap(Func<ICollection<TValue>> collectionFactory)
        {
            _dictionary = new Dictionary<TKey, ICollection<TValue>>();
            _collectionFactory = collectionFactory ?? (() => new List<TValue>());
            _valueCount = 0;
        }

        /// <summary>
        /// 添加键值对
        /// </summary>
        public void Add(TKey key, TValue value)
        {
            if (!_dictionary.TryGetValue(key, out var values))
            {
                values = _collectionFactory();
                _dictionary[key] = values;
            }
            values.Add(value);
            _valueCount++;
        }

        /// <summary>
        /// 批量添加值到指定键
        /// </summary>
        public void AddRange(TKey key, IEnumerable<TValue> values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            foreach (var value in values)
            {
                Add(key, value);
            }
        }

        /// <summary>
        /// 移除指定键的指定值
        /// </summary>
        public bool Remove(TKey key, TValue value)
        {
            if (_dictionary.TryGetValue(key, out var values))
            {
                if (values.Remove(value))
                {
                    _valueCount--;
                    if (values.Count == 0)
                    {
                        _dictionary.Remove(key);
                    }
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 移除指定键的所有值
        /// </summary>
        public bool RemoveAll(TKey key)
        {
            if (_dictionary.TryGetValue(key, out var values))
            {
                _valueCount -= values.Count;
                _dictionary.Remove(key);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 是否包含键
        /// </summary>
        public bool ContainsKey(TKey key)
        {
            return _dictionary.ContainsKey(key);
        }

        /// <summary>
        /// 是否包含指定键值对
        /// </summary>
        public bool Contains(TKey key, TValue value)
        {
            if (_dictionary.TryGetValue(key, out var values))
            {
                return values.Contains(value);
            }
            return false;
        }

        /// <summary>
        /// 获取指定键的值数量
        /// </summary>
        public int GetValueCount(TKey key)
        {
            if (_dictionary.TryGetValue(key, out var values))
                return values.Count;
            return 0;
        }

        /// <summary>
        /// 尝试获取值
        /// </summary>
        public bool TryGetValues(TKey key, out ICollection<TValue> values)
        {
            return _dictionary.TryGetValue(key, out values);
        }

        /// <summary>
        /// 清空
        /// </summary>
        public void Clear()
        {
            _dictionary.Clear();
            _valueCount = 0;
        }

        /// <summary>
        /// 获取所有键值对
        /// </summary>
        public IEnumerable<KeyValuePair<TKey, TValue>> GetAllKeyValuePairs()
        {
            foreach (var kvp in _dictionary)
            {
                foreach (var value in kvp.Value)
                {
                    yield return new KeyValuePair<TKey, TValue>(kvp.Key, value);
                }
            }
        }

        /// <summary>
        /// 获取所有值
        /// </summary>
        public IEnumerable<TValue> GetAllValues()
        {
            return _dictionary.Values.SelectMany(v => v);
        }
    }
}
