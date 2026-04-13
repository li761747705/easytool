using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EasyTool.CacheCategory
{
    /// <summary>
    /// Redis 缓存配置
    /// </summary>
    public class RedisCacheOptions
    {
        /// <summary>
        /// Redis 连接字符串
        /// </summary>
        public string ConnectionString { get; set; } = "localhost:6379";

        /// <summary>
        /// 实例名称
        /// </summary>
        public string InstanceName { get; set; } = "";

        /// <summary>
        /// 默认数据库
        /// </summary>
        public int DefaultDatabase { get; set; } = 0;

        /// <summary>
        /// 默认过期时间
        /// </summary>
        public TimeSpan? DefaultExpiration { get; set; }

        /// <summary>
        /// 连接超时
        /// </summary>
        public TimeSpan ConnectTimeout { get; set; } = TimeSpan.FromSeconds(5);

        /// <summary>
        /// 是否允许管理员操作
        /// </summary>
        public bool AllowAdmin { get; set; }

        /// <summary>
        /// 是否使用 SSL
        /// </summary>
        public bool UseSsl { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string? Password { get; set; }
    }

    /// <summary>
    /// Redis 缓存提供者
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>重要说明：</b>此类为抽象扩展点，核心功能需要引入 StackExchange.Redis 包并继承实现。
    /// EasyTool.Core 遵循零外部依赖原则，因此 Redis 相关依赖需要用户自行引入。
    /// </para>
    /// <para>
    /// <b>使用方式：</b>
    /// 1. 安装 NuGet 包：Install-Package StackExchange.Redis
    /// 2. 创建子类继承 RedisCacheProvider，实现 Redis 连接逻辑
    /// 3. 或使用 <see cref="MemoryCacheProvider"/> 作为零依赖的替代方案
    /// </para>
    /// <para>
    /// <b>子类实现示例：</b>
    /// <code>
    /// public class MyRedisCacheProvider : RedisCacheProvider
    /// {
    ///     private readonly ConnectionMultiplexer _connection;
    ///     private readonly IDatabase _db;
    ///
    ///     public MyRedisCacheProvider(RedisCacheOptions options) : base(options)
    ///     {
    ///         _connection = ConnectionMultiplexer.Connect(options.ConnectionString);
    ///         _db = _connection.GetDatabase(options.DefaultDatabase);
    ///     }
    ///
    ///     public override async Task&lt;T?&gt; GetAsync&lt;T&gt;(string key, CancellationToken ct = default)
    ///     {
    ///         var value = await _db.StringGetAsync(GetFullKey(key));
    ///         return value.HasValue ? JsonSerializer.Deserialize&lt;T&gt;(value) : default;
    ///     }
    ///
    ///     // 实现其他方法...
    /// }
    /// </code>
    /// </para>
    /// <para>
    /// <b>推荐替代方案：</b>如果不需要分布式缓存，建议使用 <see cref="MemoryCacheProvider"/>，
    /// 它是完整实现的本地内存缓存，无需任何外部依赖。
    /// </para>
    /// </remarks>
    public class RedisCacheProvider : ICacheProvider, IAsyncDisposable, IDisposable
    {
        private readonly RedisCacheOptions _options;
        private readonly string _keyPrefix;
#pragma warning disable CS0169, CS0649 // 字段保留供扩展使用
        private object? _connectionMultiplexer;
#pragma warning restore CS0169, CS0649
#pragma warning disable CS0169 // 字段保留供扩展使用
        private object? _database;
#pragma warning restore CS0169
        private bool _disposed;

        /// <summary>
        /// 创建 Redis 缓存提供者
        /// </summary>
        /// <param name="options">Redis 配置</param>
        public RedisCacheProvider(RedisCacheOptions? options = null)
        {
            _options = options ?? new RedisCacheOptions();
            _keyPrefix = string.IsNullOrEmpty(_options.InstanceName)
                ? ""
                : _options.InstanceName + ":";
        }

        /// <summary>
        /// 获取 Redis 连接（需要 StackExchange.Redis）
        /// 此方法为扩展点，子类可重写以实现具体的 Redis 连接逻辑
        /// </summary>
        [Obsolete("请引入 StackExchange.Redis 包并实现 Redis 连接逻辑")]
        protected virtual object? GetConnection()
        {
            throw new NotSupportedException(
                "请引入 StackExchange.Redis 包并实现 Redis 连接逻辑，" +
                "或使用 DistributedCacheUtil.CreateRedisProvider 方法");
        }

        private string GetFullKey(string key) => $"{_keyPrefix}{key}";

        /// <inheritdoc/>
        public async Task SetAsync<T>(string key, T value, CacheOptions? options = null, CancellationToken cancellationToken = default)
        {
            ThrowIfNotImplemented();

            var fullKey = GetFullKey(key);
            var expiration = GetExpiration(options);

            // 这里需要实际的 Redis 实现来设置值
            await Task.CompletedTask.ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public void Set<T>(string key, T value, CacheOptions? options = null)
        {
            SetAsync(key, value, options).GetAwaiter().GetResult();
        }

        /// <inheritdoc/>
        public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            ThrowIfNotImplemented();
            await Task.CompletedTask.ConfigureAwait(false);
            return default;
        }

        /// <inheritdoc/>
        public T? Get<T>(string key)
        {
            return GetAsync<T>(key).GetAwaiter().GetResult();
        }

        /// <inheritdoc/>
        public async Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> factory, CacheOptions? options = null, CancellationToken cancellationToken = default)
        {
            var value = await GetAsync<T>(key, cancellationToken).ConfigureAwait(false);
            if (value != null || typeof(T).IsValueType)
            {
                if (value != null)
                    return value;
            }

            value = await factory().ConfigureAwait(false);
            await SetAsync(key, value, options, cancellationToken).ConfigureAwait(false);
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
            ThrowIfNotImplemented();
            await Task.CompletedTask.ConfigureAwait(false);
            return false;
        }

        /// <inheritdoc/>
        public bool Exists(string key)
        {
            return ExistsAsync(key).GetAwaiter().GetResult();
        }

        /// <inheritdoc/>
        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            ThrowIfNotImplemented();
            await Task.CompletedTask.ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public void Remove(string key)
        {
            RemoveAsync(key).GetAwaiter().GetResult();
        }

        /// <inheritdoc/>
        public async Task RemoveAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default)
        {
            ThrowIfNotImplemented();
            await Task.CompletedTask.ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public void Remove(IEnumerable<string> keys)
        {
            RemoveAsync(keys).GetAwaiter().GetResult();
        }

        /// <inheritdoc/>
        public async Task<bool> SetExpirationAsync(string key, TimeSpan expiration, CancellationToken cancellationToken = default)
        {
            ThrowIfNotImplemented();
            await Task.CompletedTask.ConfigureAwait(false);
            return false;
        }

        /// <inheritdoc/>
        public bool SetExpiration(string key, TimeSpan expiration)
        {
            return SetExpirationAsync(key, expiration).GetAwaiter().GetResult();
        }

        /// <inheritdoc/>
        public async Task ClearAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfNotImplemented();
            await Task.CompletedTask.ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public void Clear()
        {
            ClearAsync().GetAwaiter().GetResult();
        }

        /// <inheritdoc/>
        public async Task<long> CountAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfNotImplemented();
            await Task.CompletedTask.ConfigureAwait(false);
            return 0;
        }

        /// <inheritdoc/>
        public long Count()
        {
            return CountAsync().GetAwaiter().GetResult();
        }

        private TimeSpan? GetExpiration(CacheOptions? options)
        {
            if (options?.AbsoluteExpirationRelativeToNow != null)
                return options.AbsoluteExpirationRelativeToNow;

            if (options?.AbsoluteExpiration != null)
                return options.AbsoluteExpiration.Value - DateTime.UtcNow;

            if (options?.SlidingExpiration != null)
                return options.SlidingExpiration;

            return _options.DefaultExpiration;
        }

        private void ThrowIfNotImplemented()
        {
            if (_connectionMultiplexer == null)
            {
                throw new NotSupportedException(
                    "Redis 缓存提供者需要实际实现。请引入 StackExchange.Redis 包，" +
                    "或使用 MemoryCacheProvider 作为替代。");
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                (_connectionMultiplexer as IDisposable)?.Dispose();
                _disposed = true;
            }
        }

        /// <summary>
        /// 异步释放资源
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            if (!_disposed)
            {
                if (_connectionMultiplexer is IAsyncDisposable asyncDisposable)
                {
                    await asyncDisposable.DisposeAsync().ConfigureAwait(false);
                }
                else if (_connectionMultiplexer is IDisposable disposable)
                {
                    disposable.Dispose();
                }
                _disposed = true;
            }
        }
    }
}
