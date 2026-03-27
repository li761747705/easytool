using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EasyTool.CacheCategory
{
    /// <summary>
    /// 缓存提供者接口
    /// </summary>
    public interface ICacheProvider
    {
        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="key">缓存键</param>
        /// <param name="value">缓存值</param>
        /// <param name="options">缓存选项</param>
        /// <param name="cancellationToken">取消令牌</param>
        Task SetAsync<T>(string key, T value, CacheOptions? options = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// 设置缓存（同步）
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="key">缓存键</param>
        /// <param name="value">缓存值</param>
        /// <param name="options">缓存选项</param>
        void Set<T>(string key, T value, CacheOptions? options = null);

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="key">缓存键</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>缓存值</returns>
        Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取缓存（同步）
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="key">缓存键</param>
        /// <returns>缓存值</returns>
        T? Get<T>(string key);

        /// <summary>
        /// 获取或添加缓存
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="key">缓存键</param>
        /// <param name="factory">值工厂</param>
        /// <param name="options">缓存选项</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>缓存值</returns>
        Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> factory, CacheOptions? options = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取或添加缓存（同步）
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="key">缓存键</param>
        /// <param name="factory">值工厂</param>
        /// <param name="options">缓存选项</param>
        /// <returns>缓存值</returns>
        T GetOrAdd<T>(string key, Func<T> factory, CacheOptions? options = null);

        /// <summary>
        /// 检查缓存是否存在
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>是否存在</returns>
        Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// 检查缓存是否存在（同步）
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <returns>是否存在</returns>
        bool Exists(string key);

        /// <summary>
        /// 移除缓存
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="cancellationToken">取消令牌</param>
        Task RemoveAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// 移除缓存（同步）
        /// </summary>
        /// <param name="key">缓存键</param>
        void Remove(string key);

        /// <summary>
        /// 批量移除缓存
        /// </summary>
        /// <param name="keys">缓存键集合</param>
        /// <param name="cancellationToken">取消令牌</param>
        Task RemoveAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default);

        /// <summary>
        /// 批量移除缓存（同步）
        /// </summary>
        /// <param name="keys">缓存键集合</param>
        void Remove(IEnumerable<string> keys);

        /// <summary>
        /// 设置过期时间
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="expiration">过期时间</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>是否设置成功</returns>
        Task<bool> SetExpirationAsync(string key, TimeSpan expiration, CancellationToken cancellationToken = default);

        /// <summary>
        /// 设置过期时间（同步）
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="expiration">过期时间</param>
        /// <returns>是否设置成功</returns>
        bool SetExpiration(string key, TimeSpan expiration);

        /// <summary>
        /// 清空所有缓存
        /// </summary>
        /// <param name="cancellationToken">取消令牌</param>
        Task ClearAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// 清空所有缓存（同步）
        /// </summary>
        void Clear();

        /// <summary>
        /// 获取缓存数量
        /// </summary>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>缓存项数量</returns>
        Task<long> CountAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取缓存数量（同步）
        /// </summary>
        /// <returns>缓存项数量</returns>
        long Count();
    }
}
