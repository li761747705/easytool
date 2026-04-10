using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EasyTool.CacheCategory
{
    /// <summary>
    /// 内存缓存项
    /// </summary>
    internal class MemoryCacheItem
    {
        public object? Value { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime? AbsoluteExpiration { get; set; }
        public TimeSpan? SlidingExpiration { get; set; }
        public DateTime LastAccess { get; set; }
        public CachePriority Priority { get; set; }
        public Type ValueType { get; set; } = typeof(object);
    }

    /// <summary>
    /// 内存缓存提供者
    /// 提供高性能的内存缓存实现，支持过期策略和优先级
    /// </summary>
    public class MemoryCacheProvider : ICacheProvider, IDisposable
    {
        private readonly ConcurrentDictionary<string, MemoryCacheItem> _cache;
        private readonly Timer? _cleanupTimer;
        private readonly long? _sizeLimit;
        private long _currentSize;
        private bool _disposed;

        /// <summary>
        /// 创建内存缓存提供者
        /// </summary>
        /// <param name="cleanupInterval">清理间隔</param>
        /// <param name="sizeLimit">大小限制（项数）</param>
        public MemoryCacheProvider(TimeSpan? cleanupInterval = null, long? sizeLimit = null)
        {
            _cache = new ConcurrentDictionary<string, MemoryCacheItem>();
            _sizeLimit = sizeLimit;
            _currentSize = 0;

            // 定期清理过期缓存
            var interval = cleanupInterval ?? TimeSpan.FromMinutes(1);
            _cleanupTimer = new Timer(CleanupExpired, null, interval, interval);
        }

        /// <inheritdoc/>
        public Task SetAsync<T>(string key, T value, CacheOptions? options = null, CancellationToken cancellationToken = default)
        {
            Set(key, value, options);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public void Set<T>(string key, T value, CacheOptions? options = null)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            options ??= new CacheOptions();

            var item = new MemoryCacheItem
            {
                Value = value,
                ValueType = typeof(T),
                CreateTime = DateTime.UtcNow,
                LastAccess = DateTime.UtcNow,
                Priority = options.Priority,
                SlidingExpiration = options.SlidingExpiration
            };

            // 计算过期时间
            if (options.AbsoluteExpiration.HasValue)
            {
                item.AbsoluteExpiration = options.AbsoluteExpiration.Value.ToUniversalTime();
            }
            else if (options.AbsoluteExpirationRelativeToNow.HasValue)
            {
                item.AbsoluteExpiration = DateTime.UtcNow.Add(options.AbsoluteExpirationRelativeToNow.Value);
            }

            // 添加键前缀
            var cacheKey = options.KeyPrefix != null
                ? $"{options.KeyPrefix}:{key}"
                : key;

            _cache.AddOrUpdate(cacheKey, item, (k, old) =>
            {
                Interlocked.Decrement(ref _currentSize);
                return item;
            });

            Interlocked.Increment(ref _currentSize);

            // 检查容量限制
            if (_sizeLimit.HasValue && _currentSize > _sizeLimit.Value)
            {
                EvictLowPriorityItems();
            }
        }

        /// <inheritdoc/>
        public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Get<T>(key));
        }

        /// <inheritdoc/>
        public T? Get<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
                return default;

            if (!_cache.TryGetValue(key, out var item))
                return default;

            if (IsExpired(item))
            {
                _cache.TryRemove(key, out _);
                Interlocked.Decrement(ref _currentSize);
                return default;
            }

            // 更新滑动过期
            if (item.SlidingExpiration.HasValue)
            {
                item.LastAccess = DateTime.UtcNow;
            }

            return (T?)item.Value;
        }

        /// <inheritdoc/>
        public async Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> factory, CacheOptions? options = null, CancellationToken cancellationToken = default)
        {
            var value = Get<T>(key);
            if (value != null || typeof(T).IsValueType)
            {
                if (value != null)
                    return value;
            }

            value = await factory().ConfigureAwait(false);
            Set(key, value, options);
            return value;
        }

        /// <inheritdoc/>
        public T GetOrAdd<T>(string key, Func<T> factory, CacheOptions? options = null)
        {
            var value = Get<T>(key);
            if (value != null || typeof(T).IsValueType)
            {
                if (value != null)
                    return value;
            }

            value = factory();
            Set(key, value, options);
            return value;
        }

        /// <inheritdoc/>
        public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Exists(key));
        }

        /// <inheritdoc/>
        public bool Exists(string key)
        {
            if (string.IsNullOrEmpty(key))
                return false;

            if (!_cache.TryGetValue(key, out var item))
                return false;

            if (IsExpired(item))
            {
                _cache.TryRemove(key, out _);
                Interlocked.Decrement(ref _currentSize);
                return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            Remove(key);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public void Remove(string key)
        {
            if (_cache.TryRemove(key, out _))
            {
                Interlocked.Decrement(ref _currentSize);
            }
        }

        /// <inheritdoc/>
        public Task RemoveAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default)
        {
            Remove(keys);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public void Remove(IEnumerable<string> keys)
        {
            foreach (var key in keys)
            {
                Remove(key);
            }
        }

        /// <inheritdoc/>
        public Task<bool> SetExpirationAsync(string key, TimeSpan expiration, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(SetExpiration(key, expiration));
        }

        /// <inheritdoc/>
        public bool SetExpiration(string key, TimeSpan expiration)
        {
            if (!_cache.TryGetValue(key, out var item))
                return false;

            item.AbsoluteExpiration = DateTime.UtcNow.Add(expiration);
            return true;
        }

        /// <inheritdoc/>
        public Task ClearAsync(CancellationToken cancellationToken = default)
        {
            Clear();
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public void Clear()
        {
            _cache.Clear();
            Interlocked.Exchange(ref _currentSize, 0);
        }

        /// <inheritdoc/>
        public Task<long> CountAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Count());
        }

        /// <inheritdoc/>
        public long Count()
        {
            return _cache.Count;
        }

        /// <summary>
        /// 获取所有缓存键
        /// </summary>
        /// <returns>缓存键集合</returns>
        public IEnumerable<string> GetKeys()
        {
            return _cache.Keys.ToList();
        }

        /// <summary>
        /// 获取缓存统计信息
        /// </summary>
        /// <returns>统计信息</returns>
        public CacheStatistics GetStatistics()
        {
            var now = DateTime.UtcNow;
            var items = _cache.Values.ToList();

            return new CacheStatistics
            {
                TotalCount = items.Count,
                ExpiredCount = items.Count(i => IsExpired(i)),
                HighPriorityCount = items.Count(i => i.Priority == CachePriority.High),
                LowPriorityCount = items.Count(i => i.Priority == CachePriority.Low),
                EstimatedSize = _currentSize
            };
        }

        private bool IsExpired(MemoryCacheItem item)
        {
            var now = DateTime.UtcNow;

            // 检查绝对过期
            if (item.AbsoluteExpiration.HasValue && now >= item.AbsoluteExpiration.Value)
                return true;

            // 检查滑动过期
            if (item.SlidingExpiration.HasValue)
            {
                var expireTime = item.LastAccess.Add(item.SlidingExpiration.Value);
                if (now >= expireTime)
                    return true;
            }

            return false;
        }

        private void CleanupExpired(object? state)
        {
            var keysToRemove = new List<string>();

            foreach (var kvp in _cache)
            {
                if (IsExpired(kvp.Value))
                {
                    keysToRemove.Add(kvp.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                if (_cache.TryRemove(key, out _))
                {
                    Interlocked.Decrement(ref _currentSize);
                }
            }
        }

        private void EvictLowPriorityItems()
        {
            // 按优先级和访问时间排序，移除低优先级的项
            var itemsToEvict = _cache
                .Where(kvp => kvp.Value.Priority != CachePriority.NeverRemove)
                .OrderBy(kvp => (int)kvp.Value.Priority)
                .ThenBy(kvp => kvp.Value.LastAccess)
                .Take((int)(_currentSize - _sizeLimit!.Value + 10))
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in itemsToEvict)
            {
                if (_cache.TryRemove(key, out _))
                {
                    Interlocked.Decrement(ref _currentSize);
                }
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                _cleanupTimer?.Dispose();
                _cache.Clear();
                _disposed = true;
            }
        }
    }

    /// <summary>
    /// 缓存统计信息
    /// </summary>
    public class CacheStatistics
    {
        /// <summary>
        /// 总缓存项数
        /// </summary>
        public long TotalCount { get; set; }

        /// <summary>
        /// 已过期项数
        /// </summary>
        public long ExpiredCount { get; set; }

        /// <summary>
        /// 高优先级项数
        /// </summary>
        public long HighPriorityCount { get; set; }

        /// <summary>
        /// 低优先级项数
        /// </summary>
        public long LowPriorityCount { get; set; }

        /// <summary>
        /// 估计大小
        /// </summary>
        public long EstimatedSize { get; set; }
    }
}
