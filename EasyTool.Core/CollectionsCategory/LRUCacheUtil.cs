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

        /// <summary>
        /// 当前缓存数量
        /// </summary>
        public int Count => _cache.Count;

        /// <summary>
        /// 缓存容量
        /// </summary>
        public int Capacity => _capacity;

        /// <summary>
        /// 缓存命中率
        /// </summary>
        public double HitRate => _totalRequests == 0 ? 0 : (double)_hits / _totalRequests;

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
        public TValue this[TKey key]
        {
            get => Get(key);
            set => Put(key, value);
        }

        /// <summary>
        /// 获取缓存值
        /// </summary>
        public TValue Get(TKey key)
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

        /// <summary>
        /// 尝试获取缓存值
        /// </summary>
        public bool TryGet(TKey key, out TValue value)
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

        /// <summary>
        /// 获取缓存值，不存在则通过工厂创建并缓存
        /// </summary>
        public TValue GetOrAdd(TKey key, Func<TKey, TValue> factory)
        {
            if (TryGet(key, out var value))
                return value;

            value = factory(key);
            Put(key, value);
            return value;
        }

        /// <summary>
        /// 添加或更新缓存
        /// </summary>
        public void Put(TKey key, TValue value)
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

        /// <summary>
        /// 移除缓存
        /// </summary>
        public bool Remove(TKey key)
        {
            if (_cache.TryGetValue(key, out var node))
            {
                _lruList.Remove(node);
                _cache.Remove(key);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 是否包含键
        /// </summary>
        public bool ContainsKey(TKey key)
        {
            return _cache.ContainsKey(key);
        }

        /// <summary>
        /// 清空缓存
        /// </summary>
        public void Clear()
        {
            _cache.Clear();
            _lruList.Clear();
            _hits = 0;
            _totalRequests = 0;
        }

        /// <summary>
        /// 获取所有键
        /// </summary>
        public IEnumerable<TKey> GetKeys()
        {
            var node = _lruList.First;
            while (node != null)
            {
                yield return node.Value.Key;
                node = node.Next;
            }
        }

        /// <summary>
        /// 获取所有值（按LRU顺序）
        /// </summary>
        public IEnumerable<TValue> GetValues()
        {
            var node = _lruList.First;
            while (node != null)
            {
                yield return node.Value.Value;
                node = node.Next;
            }
        }

        /// <summary>
        /// 重置统计信息
        /// </summary>
        public void ResetStatistics()
        {
            _hits = 0;
            _totalRequests = 0;
        }

        private class CacheItem
        {
            public TKey Key { get; set; }
            public TValue Value { get; set; }
        }
    }
}
