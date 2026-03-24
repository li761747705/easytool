using System;
using System.Collections.Generic;

namespace EasyTool.CollectionsCategory
{
    /// <summary>
    /// 双向字典工具类
    /// 支持键值双向查找的字典
    /// </summary>
    public static class BiDictionaryUtil
    {
        /// <summary>
        /// 创建双向字典
        /// </summary>
        public static BiDictionary<TKey, TValue> Create<TKey, TValue>()
            where TKey : notnull
            where TValue : notnull
        {
            return new BiDictionary<TKey, TValue>();
        }

        /// <summary>
        /// 从字典创建双向字典
        /// </summary>
        public static BiDictionary<TKey, TValue> FromDictionary<TKey, TValue>(IDictionary<TKey, TValue> dictionary)
            where TKey : notnull
            where TValue : notnull
        {
            return new BiDictionary<TKey, TValue>(dictionary);
        }
    }

    /// <summary>
    /// 双向字典实现
    /// </summary>
    /// <typeparam name="TKey">键类型</typeparam>
    /// <typeparam name="TValue">值类型</typeparam>
    public class BiDictionary<TKey, TValue> : IDictionary<TKey, TValue>
        where TKey : notnull
        where TValue : notnull
    {
        private readonly Dictionary<TKey, TValue> _forward;
        private readonly Dictionary<TValue, TKey> _reverse;

        /// <summary>
        /// 反向查找字典（值->键）
        /// </summary>
        public IReadOnlyDictionary<TValue, TKey> Reverse => _reverse;

        /// <summary>
        /// 键集合
        /// </summary>
        public ICollection<TKey> Keys => _forward.Keys;

        /// <summary>
        /// 值集合
        /// </summary>
        public ICollection<TValue> Values => _forward.Values;

        /// <summary>
        /// 元素数量
        /// </summary>
        public int Count => _forward.Count;

        /// <summary>
        /// 是否只读
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// 通过键访问值
        /// </summary>
        public TValue this[TKey key]
        {
            get => _forward[key];
            set
            {
                if (_forward.TryGetValue(key, out var oldValue))
                {
                    _reverse.Remove(oldValue);
                }
                _forward[key] = value;
                _reverse[value] = key;
            }
        }

        /// <summary>
        /// 创建双向字典
        /// </summary>
        public BiDictionary()
        {
            _forward = new Dictionary<TKey, TValue>();
            _reverse = new Dictionary<TValue, TKey>();
        }

        /// <summary>
        /// 从字典创建双向字典
        /// </summary>
        public BiDictionary(IDictionary<TKey, TValue> dictionary) : this()
        {
            if (dictionary == null)
                throw new ArgumentNullException(nameof(dictionary));

            foreach (var pair in dictionary)
            {
                Add(pair.Key, pair.Value);
            }
        }

        /// <summary>
        /// 添加键值对
        /// </summary>
        public void Add(TKey key, TValue value)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (_forward.ContainsKey(key))
                throw new ArgumentException($"Key '{key}' already exists", nameof(key));
            if (_reverse.ContainsKey(value))
                throw new ArgumentException($"Value '{value}' already exists", nameof(value));

            _forward.Add(key, value);
            _reverse.Add(value, key);
        }

        /// <summary>
        /// 添加键值对
        /// </summary>
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        /// <summary>
        /// 尝试添加键值对
        /// </summary>
        public bool TryAdd(TKey key, TValue value)
        {
            if (_forward.ContainsKey(key) || _reverse.ContainsKey(value))
                return false;

            Add(key, value);
            return true;
        }

        /// <summary>
        /// 通过键查找值
        /// </summary>
        public bool TryGetValue(TKey key, out TValue value)
        {
            return _forward.TryGetValue(key, out value);
        }

        /// <summary>
        /// 通过值查找键
        /// </summary>
        public bool TryGetKey(TValue value, out TKey key)
        {
            return _reverse.TryGetValue(value, out key);
        }

        /// <summary>
        /// 通过键获取值，不存在则返回默认值
        /// </summary>
        public TValue GetValueOrDefault(TKey key, TValue defaultValue = default)
        {
            return _forward.TryGetValue(key, out var value) ? value : defaultValue;
        }

        /// <summary>
        /// 通过值获取键，不存在则返回默认值
        /// </summary>
        public TKey GetKeyOrDefault(TValue value, TKey defaultKey = default)
        {
            return _reverse.TryGetValue(value, out var key) ? key : defaultKey;
        }

        /// <summary>
        /// 是否包含键
        /// </summary>
        public bool ContainsKey(TKey key)
        {
            return _forward.ContainsKey(key);
        }

        /// <summary>
        /// 是否包含值
        /// </summary>
        public bool ContainsValue(TValue value)
        {
            return _reverse.ContainsKey(value);
        }

        /// <summary>
        /// 是否包含键值对
        /// </summary>
        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return _forward.TryGetValue(item.Key, out var value) &&
                   EqualityComparer<TValue>.Default.Equals(value, item.Value);
        }

        /// <summary>
        /// 移除键值对
        /// </summary>
        public bool Remove(TKey key)
        {
            if (!_forward.TryGetValue(key, out var value))
                return false;

            _forward.Remove(key);
            _reverse.Remove(value);
            return true;
        }

        /// <summary>
        /// 通过值移除键值对
        /// </summary>
        public bool RemoveByValue(TValue value)
        {
            if (!_reverse.TryGetValue(value, out var key))
                return false;

            _forward.Remove(key);
            _reverse.Remove(value);
            return true;
        }

        /// <summary>
        /// 移除键值对
        /// </summary>
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            if (!Contains(item))
                return false;

            return Remove(item.Key);
        }

        /// <summary>
        /// 清空
        /// </summary>
        public void Clear()
        {
            _forward.Clear();
            _reverse.Clear();
        }

        /// <summary>
        /// 复制到数组
        /// </summary>
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (arrayIndex < 0 || arrayIndex + Count > array.Length)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));

            int i = arrayIndex;
            foreach (var pair in _forward)
            {
                array[i++] = pair;
            }
        }

        /// <summary>
        /// 交换键值（创建新的 Value->Key 映射）
        /// </summary>
        public BiDictionary<TValue, TKey> Inverse()
        {
            return new BiDictionary<TValue, TKey>(_reverse);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _forward.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
