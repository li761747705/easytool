using System;
using System.Collections.Generic;

namespace EasyTool.CollectionsCategory
{
    /// <summary>
    /// LRU 缓存工具类
    /// 最近最少使用淘汰策略的缓存
    /// </summary>
    public static class LRUCacheUtil
    {
        /// <summary>
        /// 创建 LRU 缓存
        /// </summary>
        /// <typeparam name="TKey">键类型</typeparam>
        /// <typeparam name="TValue">值类型</typeparam>
        /// <param name="capacity">容量</param>
        /// <returns>LRU 缓存实例</returns>
        public static LRUCache<TKey, TValue> Create<TKey, TValue>(int capacity)
            where TKey : notnull
        {
            return new LRUCache<TKey, TValue>(capacity);
        }
    }

    /// <summary>
    /// LRU 缓存实现
    /// </summary>
    /// <typeparam name="TKey">键类型</typeparam>
    /// <typeparam name="TValue">值类型</typeparam>
    public class LRUCache<TKey, TValue> where TKey : notnull
    {
        private readonly int _capacity;
        private readonly Dictionary<TKey, LinkedListNode<CacheItem>> _cache;
        private readonly LinkedList<CacheItem> _lruList;
        private readonly object _lock = new();

        /// <summary>
        /// 当前缓存数量
        /// </summary>
        public int Count
        {
            get
            {
                lock (_lock) { return _cache.Count; }
            }
        }

        /// <summary>
        /// 缓存容量
        /// </summary>
        public int Capacity => _capacity;

        /// <summary>
        /// 缓存命中率
        /// </summary>
        public double HitRate
        {
            get
            {
                lock (_lock) { return _totalRequests == 0 ? 0 : (double)_hits / _totalRequests; }
            }
        }

        private long _hits;
        private long _totalRequests;

        /// <summary>
        /// 创建 LRU 缓存
        /// </summary>
        /// <param name="capacity">容量</param>
        public LRUCache(int capacity)
        {
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be greater than 0");

            _capacity = capacity;
            _cache = new Dictionary<TKey, LinkedListNode<CacheItem>>();
            _lruList = new LinkedList<CacheItem>();
        }

        /// <summary>
        /// 获取或设置缓存值
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>缓存值</returns>
        /// <exception cref="KeyNotFoundException">当键不存在时抛出</exception>
        public TValue this[TKey key]
        {
            get => Get(key);
            set => Put(key, value);
        }

        /// <summary>
        /// 获取缓存值
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>缓存值</returns>
        /// <exception cref="KeyNotFoundException">当键不存在时抛出</exception>
        public TValue Get(TKey key)
        {
            lock (_lock)
            {
                _totalRequests++;

                if (_cache.TryGetValue(key, out var node))
                {
                    _hits++;
                    // 移动到链表头部（最近使用）
                    _lruList.Remove(node);
                    _lruList.AddFirst(node);
                    return node.Value.Value;
                }

                throw new KeyNotFoundException($"Key '{key}' not found in cache");
            }
        }

        /// <summary>
        /// 尝试获取缓存值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">缓存值（如果找到）</param>
        /// <returns>如果找到缓存返回 true，否则返回 false</returns>
        public bool TryGet(TKey key, out TValue value)
        {
            lock (_lock)
            {
                _totalRequests++;

                if (_cache.TryGetValue(key, out var node))
                {
                    _hits++;
                    _lruList.Remove(node);
                    _lruList.AddFirst(node);
                    value = node.Value.Value;
                    return true;
                }

                value = default;
                return false;
            }
        }

        /// <summary>
        /// 获取缓存值，不存在则通过工厂创建并缓存
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="factory">用于创建值的工厂函数</param>
        /// <returns>缓存值</returns>
        /// <exception cref="ArgumentNullException">当 factory 为 null 时抛出</exception>
        public TValue GetOrAdd(TKey key, Func<TKey, TValue> factory)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            lock (_lock)
            {
                _totalRequests++;

                if (_cache.TryGetValue(key, out var node))
                {
                    _hits++;
                    _lruList.Remove(node);
                    _lruList.AddFirst(node);
                    return node.Value.Value;
                }
            }

            // 在锁外执行工厂方法，避免在锁内执行用户代码导致死锁
            var value = factory(key);
            Put(key, value);
            return value;
        }

        /// <summary>
        /// 添加或更新缓存
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public void Put(TKey key, TValue value)
        {
            lock (_lock)
            {
                if (_cache.TryGetValue(key, out var existingNode))
                {
                    // 更新已存在的键
                    _lruList.Remove(existingNode);
                    existingNode.Value.Value = value;
                    _lruList.AddFirst(existingNode);
                }
                else
                {
                    // 添加新键
                    if (_cache.Count >= _capacity)
                    {
                        // 淘汰最久未使用的项
                        var last = _lruList.Last;
                        _lruList.RemoveLast();
                        _cache.Remove(last.Value.Key);
                    }

                    var cacheItem = new CacheItem { Key = key, Value = value };
                    var node = _lruList.AddFirst(cacheItem);
                    _cache[key] = node;
                }
            }
        }

        /// <summary>
        /// 移除缓存
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>如果移除成功返回 true，否则返回 false</returns>
        public bool Remove(TKey key)
        {
            lock (_lock)
            {
                if (_cache.TryGetValue(key, out var node))
                {
                    _lruList.Remove(node);
                    _cache.Remove(key);
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// 是否包含键
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>如果包含返回 true，否则返回 false</returns>
        public bool ContainsKey(TKey key)
        {
            lock (_lock) { return _cache.ContainsKey(key); }
        }

        /// <summary>
        /// 清空缓存
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                _cache.Clear();
                _lruList.Clear();
                _hits = 0;
                _totalRequests = 0;
            }
        }

        /// <summary>
        /// 获取所有键
        /// </summary>
        /// <returns>键的集合（按 LRU 顺序）</returns>
        public IEnumerable<TKey> GetKeys()
        {
            lock (_lock)
            {
                var node = _lruList.First;
                while (node != null)
                {
                    yield return node.Value.Key;
                    node = node.Next;
                }
            }
        }

        /// <summary>
        /// 获取所有值（按LRU顺序）
        /// </summary>
        /// <returns>值的集合（按 LRU 顺序）</returns>
        public IEnumerable<TValue> GetValues()
        {
            lock (_lock)
            {
                var node = _lruList.First;
                while (node != null)
                {
                    yield return node.Value.Value;
                    node = node.Next;
                }
            }
        }

        /// <summary>
        /// 重置统计信息
        /// </summary>
        public void ResetStatistics()
        {
            lock (_lock)
            {
                _hits = 0;
                _totalRequests = 0;
            }
        }

        private class CacheItem
        {
            public TKey Key { get; set; }
            public TValue Value { get; set; }
        }
    }
}
