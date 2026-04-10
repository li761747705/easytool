using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EasyTool.CollectionsCategory
{
    /// <summary>
    /// 缓存项
    /// </summary>
    /// <typeparam name="T">缓存值类型</typeparam>
    internal class CacheItem<T>
    {
        public T Value { get; set; } = default!;
        public DateTime CreateTime { get; set; }
        public DateTime? ExpireTime { get; set; }
        public TimeSpan? SlidingExpiration { get; set; }
        public DateTime LastAccess { get; set; }
    }

    /// <summary>
    /// 内存缓存工具类
    /// 提供线程安全的内存缓存功能，支持过期时间和滑动过期
    /// </summary>
    public static class CacheUtil
    {
        private static readonly ConcurrentDictionary<string, object> _cache = new();
        private static readonly Timer _cleanupTimer;
        private static readonly object _lock = new();

        static CacheUtil()
        {
            // 每分钟清理一次过期缓存
            _cleanupTimer = new Timer(_ => CleanupExpired(), null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
        }

        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="key">缓存键</param>
        /// <param name="value">缓存值</param>
        /// <param name="absoluteExpiration">绝对过期时间</param>
        public static void Set<T>(string key, T value, DateTime? absoluteExpiration = null)
        {
            var item = new CacheItem<T>
            {
                Value = value,
                CreateTime = DateTime.UtcNow,
                ExpireTime = absoluteExpiration,
                LastAccess = DateTime.UtcNow
            };
            _cache[key] = item;
        }

        /// <summary>
        /// 设置缓存（相对过期）
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="key">缓存键</param>
        /// <param name="value">缓存值</param>
        /// <param name="expiration">过期时间间隔</param>
        public static void Set<T>(string key, T value, TimeSpan expiration)
        {
            Set(key, value, DateTime.UtcNow.Add(expiration));
        }

        /// <summary>
        /// 设置缓存（滑动过期）
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="key">缓存键</param>
        /// <param name="value">缓存值</param>
        /// <param name="slidingExpiration">滑动过期时间</param>
        /// <param name="absoluteExpiration">最大过期时间</param>
        public static void SetSliding<T>(string key, T value, TimeSpan slidingExpiration, DateTime? absoluteExpiration = null)
        {
            var item = new CacheItem<T>
            {
                Value = value,
                CreateTime = DateTime.UtcNow,
                SlidingExpiration = slidingExpiration,
                ExpireTime = absoluteExpiration,
                LastAccess = DateTime.UtcNow
            };
            _cache[key] = item;
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="key">缓存键</param>
        /// <returns>缓存值，如果不存在或已过期则返回默认值</returns>
        public static T? Get<T>(string key)
        {
            if (!_cache.TryGetValue(key, out var obj))
                return default;

            var item = (CacheItem<T>)obj;

            if (IsExpired(item))
            {
                _cache.TryRemove(key, out _);
                return default;
            }

            // 更新滑动过期
            if (item.SlidingExpiration.HasValue)
            {
                item.LastAccess = DateTime.UtcNow;
            }

            return item.Value;
        }

        /// <summary>
        /// 获取或添加缓存
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="key">缓存键</param>
        /// <param name="factory">值工厂</param>
        /// <param name="expiration">过期时间</param>
        /// <returns>缓存值</returns>
        public static T GetOrAdd<T>(string key, Func<T> factory, TimeSpan? expiration = null)
        {
            var value = Get<T>(key);
            if (value != null || typeof(T).IsValueType)
            {
                if (value != null)
                    return value;
            }

            lock (_lock)
            {
                value = Get<T>(key);
                if (value != null || (typeof(T).IsValueType && value != null))
                    return value!;

                value = factory();
                if (expiration.HasValue)
                    Set(key, value, expiration.Value);
                else
                    Set(key, value);

                return value;
            }
        }

        /// <summary>
        /// 异步获取或添加缓存
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="key">缓存键</param>
        /// <param name="factory">值工厂</param>
        /// <param name="expiration">过期时间</param>
        /// <returns>缓存值</returns>
        public static async Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
        {
            var value = Get<T>(key);
            if (value != null || typeof(T).IsValueType)
            {
                if (value != null)
                    return value;
            }

            lock (_lock)
            {
                value = Get<T>(key);
                if (value != null || (typeof(T).IsValueType && value != null))
                    return value!;
            }

            value = await factory().ConfigureAwait(false);

            if (expiration.HasValue)
                Set(key, value, expiration.Value);
            else
                Set(key, value);

            return value;
        }

        /// <summary>
        /// 检查缓存是否存在
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <returns>是否存在</returns>
        public static bool Contains(string key)
        {
            if (!_cache.TryGetValue(key, out var obj))
                return false;

            var itemType = obj.GetType();
            var isExpired = (bool)itemType.GetMethod("IsExpired")!.Invoke(null, new[] { obj })!;

            if (isExpired)
            {
                _cache.TryRemove(key, out _);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 移除缓存
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <returns>是否移除成功</returns>
        public static bool Remove(string key)
        {
            return _cache.TryRemove(key, out _);
        }

        /// <summary>
        /// 清空所有缓存
        /// </summary>
        public static void Clear()
        {
            _cache.Clear();
        }

        /// <summary>
        /// 获取缓存数量
        /// </summary>
        /// <returns>缓存项数量</returns>
        public static int Count()
        {
            return _cache.Count;
        }

        /// <summary>
        /// 获取所有缓存键
        /// </summary>
        /// <returns>缓存键集合</returns>
        public static IEnumerable<string> GetKeys()
        {
            return _cache.Keys.ToList();
        }

        /// <summary>
        /// 设置缓存过期时间
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="expiration">过期时间</param>
        /// <returns>是否设置成功</returns>
        public static bool SetExpiration(string key, TimeSpan expiration)
        {
            if (!_cache.TryGetValue(key, out var obj))
                return false;

            var itemType = obj.GetType();
            var expireTimeProperty = itemType.GetProperty("ExpireTime");
            if (expireTimeProperty != null)
            {
                expireTimeProperty.SetValue(obj, DateTime.UtcNow.Add(expiration));
                return true;
            }

            return false;
        }

        private static bool IsExpired<T>(CacheItem<T> item)
        {
            var now = DateTime.UtcNow;

            // 检查绝对过期
            if (item.ExpireTime.HasValue && now >= item.ExpireTime.Value)
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

        private static void CleanupExpired()
        {
            var keysToRemove = new List<string>();

            foreach (var kvp in _cache)
            {
                var itemType = kvp.Value.GetType();
                var expireTimeProperty = itemType.GetProperty("ExpireTime");
                var slidingExpirationProperty = itemType.GetProperty("SlidingExpiration");
                var lastAccessProperty = itemType.GetProperty("LastAccess");

                if (expireTimeProperty != null)
                {
                    var expireTime = (DateTime?)expireTimeProperty.GetValue(kvp.Value);
                    var slidingExpiration = (TimeSpan?)slidingExpirationProperty?.GetValue(kvp.Value);
                    var lastAccess = (DateTime)lastAccessProperty!.GetValue(kvp.Value)!;

                    var now = DateTime.UtcNow;

                    if (expireTime.HasValue && now >= expireTime.Value)
                    {
                        keysToRemove.Add(kvp.Key);
                    }
                    else if (slidingExpiration.HasValue)
                    {
                        var slidingExpire = lastAccess.Add(slidingExpiration.Value);
                        if (now >= slidingExpire)
                        {
                            keysToRemove.Add(kvp.Key);
                        }
                    }
                }
            }

            foreach (var key in keysToRemove)
            {
                _cache.TryRemove(key, out _);
            }
        }
    }
}