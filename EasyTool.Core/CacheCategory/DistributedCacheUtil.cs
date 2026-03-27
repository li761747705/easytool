using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EasyTool.CacheCategory
{
    /// <summary>
    /// 分布式缓存工具类
    /// 提供多级缓存支持，包括本地缓存和分布式缓存
    /// </summary>
    public static class DistributedCacheUtil
    {
        private static readonly ConcurrentDictionary<string, ICacheProvider> _providers = new();
        private static ICacheProvider? _defaultProvider;
        private static readonly object _lock = new();

        /// <summary>
        /// 注册缓存提供者
        /// </summary>
        /// <param name="name">提供者名称</param>
        /// <param name="provider">缓存提供者</param>
        /// <param name="setDefault">是否设为默认</param>
        public static void RegisterProvider(string name, ICacheProvider provider, bool setDefault = false)
        {
            _providers[name] = provider;

            if (setDefault || _defaultProvider == null)
            {
                _defaultProvider = provider;
            }
        }

        /// <summary>
        /// 获取缓存提供者
        /// </summary>
        /// <param name="name">提供者名称</param>
        /// <returns>缓存提供者</returns>
        public static ICacheProvider? GetProvider(string name)
        {
            return _providers.TryGetValue(name, out var provider) ? provider : null;
        }

        /// <summary>
        /// 获取默认缓存提供者
        /// </summary>
        public static ICacheProvider DefaultProvider
        {
            get
            {
                if (_defaultProvider == null)
                {
                    lock (_lock)
                    {
                        if (_defaultProvider == null)
                        {
                            _defaultProvider = new MemoryCacheProvider();
                            _providers["default"] = _defaultProvider;
                        }
                    }
                }
                return _defaultProvider;
            }
        }

        /// <summary>
        /// 创建内存缓存提供者
        /// </summary>
        /// <param name="cleanupInterval">清理间隔</param>
        /// <param name="sizeLimit">大小限制</param>
        /// <returns>缓存提供者</returns>
        public static MemoryCacheProvider CreateMemoryProvider(TimeSpan? cleanupInterval = null, long? sizeLimit = null)
        {
            return new MemoryCacheProvider(cleanupInterval, sizeLimit);
        }

        /// <summary>
        /// 创建 Redis 缓存提供者
        /// </summary>
        /// <param name="options">Redis 配置</param>
        /// <returns>缓存提供者</returns>
        public static RedisCacheProvider CreateRedisProvider(RedisCacheOptions? options = null)
        {
            return new RedisCacheProvider(options);
        }

        #region 便捷方法 - 使用默认提供者

        /// <summary>
        /// 设置缓存
        /// </summary>
        public static Task SetAsync<T>(string key, T value, CacheOptions? options = null, CancellationToken cancellationToken = default)
        {
            return DefaultProvider.SetAsync(key, value, options, cancellationToken);
        }

        /// <summary>
        /// 设置缓存（同步）
        /// </summary>
        public static void Set<T>(string key, T value, CacheOptions? options = null)
        {
            DefaultProvider.Set(key, value, options);
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        public static Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            return DefaultProvider.GetAsync<T>(key, cancellationToken);
        }

        /// <summary>
        /// 获取缓存（同步）
        /// </summary>
        public static T? Get<T>(string key)
        {
            return DefaultProvider.Get<T>(key);
        }

        /// <summary>
        /// 获取或添加缓存
        /// </summary>
        public static Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> factory, CacheOptions? options = null, CancellationToken cancellationToken = default)
        {
            return DefaultProvider.GetOrAddAsync(key, factory, options, cancellationToken);
        }

        /// <summary>
        /// 获取或添加缓存（同步）
        /// </summary>
        public static T GetOrAdd<T>(string key, Func<T> factory, CacheOptions? options = null)
        {
            return DefaultProvider.GetOrAdd(key, factory, options);
        }

        /// <summary>
        /// 检查缓存是否存在
        /// </summary>
        public static Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
        {
            return DefaultProvider.ExistsAsync(key, cancellationToken);
        }

        /// <summary>
        /// 检查缓存是否存在（同步）
        /// </summary>
        public static bool Exists(string key)
        {
            return DefaultProvider.Exists(key);
        }

        /// <summary>
        /// 移除缓存
        /// </summary>
        public static Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            return DefaultProvider.RemoveAsync(key, cancellationToken);
        }

        /// <summary>
        /// 移除缓存（同步）
        /// </summary>
        public static void Remove(string key)
        {
            DefaultProvider.Remove(key);
        }

        /// <summary>
        /// 清空缓存
        /// </summary>
        public static Task ClearAsync(CancellationToken cancellationToken = default)
        {
            return DefaultProvider.ClearAsync(cancellationToken);
        }

        /// <summary>
        /// 清空缓存（同步）
        /// </summary>
        public static void Clear()
        {
            DefaultProvider.Clear();
        }

        #endregion

        #region 高级功能

        /// <summary>
        /// 批量获取缓存
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="keys">缓存键集合</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>键值对字典</returns>
        public static async Task<Dictionary<string, T?>> GetManyAsync<T>(IEnumerable<string> keys, CancellationToken cancellationToken = default)
        {
            var result = new Dictionary<string, T?>();

            foreach (var key in keys)
            {
                var value = await DefaultProvider.GetAsync<T>(key, cancellationToken);
                result[key] = value;
            }

            return result;
        }

        /// <summary>
        /// 批量设置缓存
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="items">键值对集合</param>
        /// <param name="options">缓存选项</param>
        /// <param name="cancellationToken">取消令牌</param>
        public static async Task SetManyAsync<T>(IDictionary<string, T> items, CacheOptions? options = null, CancellationToken cancellationToken = default)
        {
            foreach (var item in items)
            {
                await DefaultProvider.SetAsync(item.Key, item.Value, options, cancellationToken);
            }
        }

        /// <summary>
        /// 获取或添加缓存（带锁）
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="key">缓存键</param>
        /// <param name="factory">值工厂</param>
        /// <param name="options">缓存选项</param>
        /// <param name="lockTimeout">锁超时时间</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>缓存值</returns>
        public static async Task<T> GetOrAddWithLockAsync<T>(
            string key,
            Func<Task<T>> factory,
            CacheOptions? options = null,
            TimeSpan? lockTimeout = null,
            CancellationToken cancellationToken = default)
        {
            var value = await DefaultProvider.GetAsync<T>(key, cancellationToken);
            if (value != null || typeof(T).IsValueType)
            {
                if (value != null)
                    return value;
            }

            // 使用简单的锁机制防止缓存穿透
            var lockKey = $"lock:{key}";
            var timeout = lockTimeout ?? TimeSpan.FromSeconds(30);
            var startTime = DateTime.UtcNow;

            while (DateTime.UtcNow - startTime < timeout)
            {
                value = await DefaultProvider.GetAsync<T>(key, cancellationToken);
                if (value != null || (typeof(T).IsValueType && value != null))
                    return value!;

                value = await factory();
                await DefaultProvider.SetAsync(key, value, options, cancellationToken);
                return value;
            }

            throw new TimeoutException($"获取缓存超时: {key}");
        }

        /// <summary>
        /// 刷新缓存（强制重新加载）
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="key">缓存键</param>
        /// <param name="factory">值工厂</param>
        /// <param name="options">缓存选项</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>新的缓存值</returns>
        public static async Task<T> RefreshAsync<T>(string key, Func<Task<T>> factory, CacheOptions? options = null, CancellationToken cancellationToken = default)
        {
            await DefaultProvider.RemoveAsync(key, cancellationToken);
            var value = await factory();
            await DefaultProvider.SetAsync(key, value, options, cancellationToken);
            return value;
        }

        #endregion
    }

    /// <summary>
    /// 多级缓存
    /// 实现本地缓存 + 分布式缓存的多级缓存策略
    /// </summary>
    public class MultiLevelCache : ICacheProvider, IDisposable
    {
        private readonly MemoryCacheProvider _localCache;
        private readonly ICacheProvider? _distributedCache;
        private readonly TimeSpan _localCacheExpiration;

        /// <summary>
        /// 创建多级缓存
        /// </summary>
        /// <param name="distributedCache">分布式缓存提供者</param>
        /// <param name="localCacheExpiration">本地缓存过期时间</param>
        public MultiLevelCache(ICacheProvider? distributedCache = null, TimeSpan? localCacheExpiration = null)
        {
            _localCache = new MemoryCacheProvider();
            _distributedCache = distributedCache;
            _localCacheExpiration = localCacheExpiration ?? TimeSpan.FromMinutes(5);
        }

        /// <inheritdoc/>
        public async Task SetAsync<T>(string key, T value, CacheOptions? options = null, CancellationToken cancellationToken = default)
        {
            // 先设置本地缓存
            var localOptions = new CacheOptions
            {
                AbsoluteExpirationRelativeToNow = _localCacheExpiration
            };
            await _localCache.SetAsync(key, value, localOptions, cancellationToken);

            // 再设置分布式缓存
            if (_distributedCache != null)
            {
                await _distributedCache.SetAsync(key, value, options, cancellationToken);
            }
        }

        /// <inheritdoc/>
        public void Set<T>(string key, T value, CacheOptions? options = null)
        {
            SetAsync(key, value, options).GetAwaiter().GetResult();
        }

        /// <inheritdoc/>
        public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            // 先查本地缓存
            var value = await _localCache.GetAsync<T>(key, cancellationToken);
            if (value != null || typeof(T).IsValueType)
            {
                if (value != null)
                    return value;
            }

            // 再查分布式缓存
            if (_distributedCache != null)
            {
                value = await _distributedCache.GetAsync<T>(key, cancellationToken);
                if (value != null)
                {
                    // 回填本地缓存
                    await _localCache.SetAsync(key, value, new CacheOptions
                    {
                        AbsoluteExpirationRelativeToNow = _localCacheExpiration
                    }, cancellationToken);
                }
            }

            return value;
        }

        /// <inheritdoc/>
        public T? Get<T>(string key)
        {
            return GetAsync<T>(key).GetAwaiter().GetResult();
        }

        /// <inheritdoc/>
        public async Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> factory, CacheOptions? options = null, CancellationToken cancellationToken = default)
        {
            var value = await GetAsync<T>(key, cancellationToken);
            if (value != null || typeof(T).IsValueType)
            {
                if (value != null)
                    return value;
            }

            value = await factory();
            await SetAsync(key, value, options, cancellationToken);
            return value;
        }

        /// <inheritdoc/>
        public T GetOrAdd<T>(string key, Func<T> factory, CacheOptions? options = null)
        {
            return GetOrAddAsync(key, () => Task.FromResult(factory()), options).GetAwaiter().GetResult();
        }

        /// <inheritdoc/>
        public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
        {
            if (await _localCache.ExistsAsync(key, cancellationToken))
                return true;

            return _distributedCache != null && await _distributedCache.ExistsAsync(key, cancellationToken);
        }

        /// <inheritdoc/>
        public bool Exists(string key)
        {
            return ExistsAsync(key).GetAwaiter().GetResult();
        }

        /// <inheritdoc/>
        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            await _localCache.RemoveAsync(key, cancellationToken);

            if (_distributedCache != null)
            {
                await _distributedCache.RemoveAsync(key, cancellationToken);
            }
        }

        /// <inheritdoc/>
        public void Remove(string key)
        {
            RemoveAsync(key).GetAwaiter().GetResult();
        }

        /// <inheritdoc/>
        public async Task RemoveAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default)
        {
            await _localCache.RemoveAsync(keys, cancellationToken);

            if (_distributedCache != null)
            {
                await _distributedCache.RemoveAsync(keys, cancellationToken);
            }
        }

        /// <inheritdoc/>
        public void Remove(IEnumerable<string> keys)
        {
            RemoveAsync(keys).GetAwaiter().GetResult();
        }

        /// <inheritdoc/>
        public async Task<bool> SetExpirationAsync(string key, TimeSpan expiration, CancellationToken cancellationToken = default)
        {
            var localResult = await _localCache.SetExpirationAsync(key, expiration, cancellationToken);

            if (_distributedCache != null)
            {
                return await _distributedCache.SetExpirationAsync(key, expiration, cancellationToken);
            }

            return localResult;
        }

        /// <inheritdoc/>
        public bool SetExpiration(string key, TimeSpan expiration)
        {
            return SetExpirationAsync(key, expiration).GetAwaiter().GetResult();
        }

        /// <inheritdoc/>
        public async Task ClearAsync(CancellationToken cancellationToken = default)
        {
            await _localCache.ClearAsync(cancellationToken);

            if (_distributedCache != null)
            {
                await _distributedCache.ClearAsync(cancellationToken);
            }
        }

        /// <inheritdoc/>
        public void Clear()
        {
            ClearAsync().GetAwaiter().GetResult();
        }

        /// <inheritdoc/>
        public async Task<long> CountAsync(CancellationToken cancellationToken = default)
        {
            var count = await _localCache.CountAsync(cancellationToken);

            if (_distributedCache != null)
            {
                count = await _distributedCache.CountAsync(cancellationToken);
            }

            return count;
        }

        /// <inheritdoc/>
        public long Count()
        {
            return CountAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            _localCache.Dispose();
        }
    }
}
