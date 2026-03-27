using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EasyTool.DatabaseCategory
{
    /// <summary>
    /// 数据库连接池选项
    /// </summary>
    public class ConnectionPoolOptions
    {
        /// <summary>
        /// 最小连接数
        /// </summary>
        public int MinPoolSize { get; set; } = 5;

        /// <summary>
        /// 最大连接数
        /// </summary>
        public int MaxPoolSize { get; set; } = 100;

        /// <summary>
        /// 连接超时时间
        /// </summary>
        public TimeSpan ConnectionTimeout { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// 连接最大空闲时间
        /// </summary>
        public TimeSpan MaxIdleTime { get; set; } = TimeSpan.FromMinutes(5);

        /// <summary>
        /// 连接最大生存时间
        /// </summary>
        public TimeSpan MaxLifetime { get; set; } = TimeSpan.FromHours(8);

        /// <summary>
        /// 健康检查间隔
        /// </summary>
        public TimeSpan HealthCheckInterval { get; set; } = TimeSpan.FromMinutes(1);

        /// <summary>
        /// 获取连接重试次数
        /// </summary>
        public int RetryCount { get; set; } = 3;

        /// <summary>
        /// 重试延迟
        /// </summary>
        public TimeSpan RetryDelay { get; set; } = TimeSpan.FromMilliseconds(100);
    }

    /// <summary>
    /// 池化连接包装器
    /// </summary>
    internal class PooledConnection : IDisposable
    {
        public DbConnection Connection { get; }
        public DateTime CreateTime { get; }
        public DateTime LastAccessTime { get; set; }
        public bool IsInUse { get; set; }
        public bool IsValid { get; set; } = true;

        public PooledConnection(DbConnection connection)
        {
            Connection = connection;
            CreateTime = DateTime.UtcNow;
            LastAccessTime = DateTime.UtcNow;
        }

        public void Dispose()
        {
            Connection?.Dispose();
        }
    }

    /// <summary>
    /// 数据库连接池
    /// 提供高效的数据库连接管理和复用
    /// </summary>
    public class ConnectionPool : IAsyncDisposable, IDisposable
    {
        private readonly string _connectionString;
        private readonly DbProviderFactory _providerFactory;
        private readonly ConnectionPoolOptions _options;
        private readonly ConcurrentBag<PooledConnection> _pool;
        private readonly SemaphoreSlim _semaphore;
        private readonly Timer _healthCheckTimer;
        private readonly Timer _cleanupTimer;
        private int _totalConnections;
        private bool _disposed;

        /// <summary>
        /// 当前池中连接数
        /// </summary>
        public int PoolSize => _totalConnections;

        /// <summary>
        /// 可用连接数
        /// </summary>
        public int AvailableConnections => _pool.Count;

        /// <summary>
        /// 正在使用的连接数
        /// </summary>
        public int InUseConnections => _totalConnections - _pool.Count;

        /// <summary>
        /// 创建数据库连接池
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        /// <param name="providerFactory">数据库提供者工厂</param>
        /// <param name="options">连接池选项</param>
        public ConnectionPool(
            string connectionString,
            DbProviderFactory providerFactory,
            ConnectionPoolOptions? options = null)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _providerFactory = providerFactory ?? throw new ArgumentNullException(nameof(providerFactory));
            _options = options ?? new ConnectionPoolOptions();
            _pool = new ConcurrentBag<PooledConnection>();
            _semaphore = new SemaphoreSlim(_options.MaxPoolSize, _options.MaxPoolSize);

            // 初始化最小连接数
            InitializeMinConnections();

            // 启动健康检查定时器
            _healthCheckTimer = new Timer(HealthCheck, null,
                _options.HealthCheckInterval, _options.HealthCheckInterval);

            // 启动清理定时器
            _cleanupTimer = new Timer(CleanupIdleConnections, null,
                _options.MaxIdleTime, _options.MaxIdleTime);
        }

        private void InitializeMinConnections()
        {
            for (int i = 0; i < _options.MinPoolSize; i++)
            {
                var connection = CreateNewConnection();
                if (connection != null)
                {
                    _pool.Add(connection);
                    Interlocked.Increment(ref _totalConnections);
                }
            }
        }

        private PooledConnection? CreateNewConnection()
        {
            try
            {
                var connection = _providerFactory.CreateConnection();
                if (connection == null)
                    return null;

                connection.ConnectionString = _connectionString;
                connection.Open();
                return new PooledConnection(connection);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 获取连接
        /// </summary>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>数据库连接</returns>
        public async Task<DbConnection> GetConnectionAsync(CancellationToken cancellationToken = default)
        {
            for (int retry = 0; retry < _options.RetryCount; retry++)
            {
                if (await _semaphore.WaitAsync(_options.ConnectionTimeout, cancellationToken))
                {
                    try
                    {
                        // 尝试从池中获取
                        while (_pool.TryTake(out var pooledConnection))
                        {
                            if (IsConnectionValid(pooledConnection))
                            {
                                pooledConnection.IsInUse = true;
                                pooledConnection.LastAccessTime = DateTime.UtcNow;
                                return new PooledConnectionWrapper(pooledConnection, this).Connection;
                            }
                            else
                            {
                                // 连接无效，释放并减少计数
                                pooledConnection.Dispose();
                                Interlocked.Decrement(ref _totalConnections);
                            }
                        }

                        // 池中没有可用连接，创建新连接
                        var newConnection = CreateNewConnection();
                        if (newConnection != null)
                        {
                            newConnection.IsInUse = true;
                            Interlocked.Increment(ref _totalConnections);
                            return new PooledConnectionWrapper(newConnection, this).Connection;
                        }
                    }
                    catch
                    {
                        _semaphore.Release();
                        throw;
                    }
                }

                if (retry < _options.RetryCount - 1)
                {
                    await Task.Delay(_options.RetryDelay, cancellationToken);
                }
            }

            throw new TimeoutException($"无法在 {_options.ConnectionTimeout} 内获取数据库连接");
        }

        /// <summary>
        /// 获取连接（同步）
        /// </summary>
        /// <returns>数据库连接</returns>
        public DbConnection GetConnection()
        {
            return GetConnectionAsync().GetAwaiter().GetResult();
        }

        internal void ReturnConnection(PooledConnection connection)
        {
            if (_disposed)
            {
                connection.Dispose();
                Interlocked.Decrement(ref _totalConnections);
                return;
            }

            if (IsConnectionValid(connection))
            {
                connection.IsInUse = false;
                connection.LastAccessTime = DateTime.UtcNow;
                _pool.Add(connection);
            }
            else
            {
                connection.Dispose();
                Interlocked.Decrement(ref _totalConnections);
            }

            _semaphore.Release();
        }

        private bool IsConnectionValid(PooledConnection connection)
        {
            if (!connection.IsValid || connection.Connection == null)
                return false;

            if (connection.Connection.State != ConnectionState.Open)
                return false;

            // 检查最大生存时间
            if (DateTime.UtcNow - connection.CreateTime > _options.MaxLifetime)
                return false;

            return true;
        }

        private void HealthCheck(object? state)
        {
            var invalidConnections = new List<PooledConnection>();

            foreach (var connection in _pool)
            {
                if (!IsConnectionValid(connection))
                {
                    invalidConnections.Add(connection);
                }
            }

            // 注意：由于 ConcurrentBag 的特性，这里只是标记连接无效
            // 实际移除会在 ReturnConnection 时进行
        }

        private void CleanupIdleConnections(object? state)
        {
            var now = DateTime.UtcNow;
            var connectionsToKeep = new List<PooledConnection>();
            var connectionsToRemove = new List<PooledConnection>();

            // 收集需要保留和移除的连接
            while (_pool.TryTake(out var connection))
            {
                if (!connection.IsInUse &&
                    now - connection.LastAccessTime > _options.MaxIdleTime &&
                    _totalConnections - connectionsToRemove.Count > _options.MinPoolSize)
                {
                    connectionsToRemove.Add(connection);
                }
                else
                {
                    connectionsToKeep.Add(connection);
                }
            }

            // 放回需要保留的连接
            foreach (var connection in connectionsToKeep)
            {
                _pool.Add(connection);
            }

            // 移除空闲连接
            foreach (var connection in connectionsToRemove)
            {
                connection.Dispose();
                Interlocked.Decrement(ref _totalConnections);
            }
        }

        /// <summary>
        /// 清空连接池
        /// </summary>
        public void Clear()
        {
            while (_pool.TryTake(out var connection))
            {
                connection.Dispose();
                Interlocked.Decrement(ref _totalConnections);
            }
        }

        /// <summary>
        /// 获取连接池统计信息
        /// </summary>
        /// <returns>统计信息</returns>
        public ConnectionPoolStatistics GetStatistics()
        {
            return new ConnectionPoolStatistics
            {
                TotalConnections = _totalConnections,
                AvailableConnections = _pool.Count,
                InUseConnections = _totalConnections - _pool.Count,
                MaxPoolSize = _options.MaxPoolSize,
                MinPoolSize = _options.MinPoolSize
            };
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _healthCheckTimer.Dispose();
                _cleanupTimer.Dispose();
                Clear();
                _semaphore.Dispose();
            }
        }

        /// <summary>
        /// 异步释放资源
        /// </summary>
        public ValueTask DisposeAsync()
        {
            if (!_disposed)
            {
                _disposed = true;
                _healthCheckTimer.Dispose();
                _cleanupTimer.Dispose();
                Clear();
                _semaphore.Dispose();
            }
            return default(ValueTask);
        }
    }

    /// <summary>
    /// 池化连接包装器
    /// </summary>
    internal class PooledConnectionWrapper : IDisposable
    {
        private readonly PooledConnection _pooledConnection;
        private readonly ConnectionPool _pool;
        private bool _disposed;

        public DbConnection Connection => _pooledConnection.Connection;

        public PooledConnectionWrapper(PooledConnection pooledConnection, ConnectionPool pool)
        {
            _pooledConnection = pooledConnection;
            _pool = pool;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _pool.ReturnConnection(_pooledConnection);
                _disposed = true;
            }
        }
    }

    /// <summary>
    /// 连接池统计信息
    /// </summary>
    public class ConnectionPoolStatistics
    {
        /// <summary>
        /// 总连接数
        /// </summary>
        public int TotalConnections { get; set; }

        /// <summary>
        /// 可用连接数
        /// </summary>
        public int AvailableConnections { get; set; }

        /// <summary>
        /// 正在使用的连接数
        /// </summary>
        public int InUseConnections { get; set; }

        /// <summary>
        /// 最大连接数
        /// </summary>
        public int MaxPoolSize { get; set; }

        /// <summary>
        /// 最小连接数
        /// </summary>
        public int MinPoolSize { get; set; }
    }
}
