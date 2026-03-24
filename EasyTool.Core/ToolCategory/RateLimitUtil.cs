using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace EasyTool.ToolCategory
{
    /// <summary>
    /// 限流工具类
    /// 提供令牌桶、漏桶、滑动窗口等限流算法
    /// </summary>
    public static class RateLimitUtil
    {
        #region 令牌桶限流器

        /// <summary>
        /// 创建令牌桶限流器
        /// </summary>
        /// <param name="capacity">桶容量（最大令牌数）</param>
        /// <param name="refillRate">每秒补充的令牌数</param>
        /// <returns>令牌桶限流器</returns>
        public static TokenBucketLimiter CreateTokenBucket(int capacity, double refillRate)
        {
            return new TokenBucketLimiter(capacity, refillRate);
        }

        /// <summary>
        /// 令牌桶限流器
        /// </summary>
        public class TokenBucketLimiter
        {
            private readonly int _capacity;
            private readonly double _refillRate;
            private double _tokens;
            private long _lastRefillTime;
            private readonly object _lock = new();

            public TokenBucketLimiter(int capacity, double refillRate)
            {
                if (capacity <= 0)
                    throw new ArgumentOutOfRangeException(nameof(capacity), "容量必须大于0");
                if (refillRate <= 0)
                    throw new ArgumentOutOfRangeException(nameof(refillRate), "补充速率必须大于0");

                _capacity = capacity;
                _refillRate = refillRate;
                _tokens = capacity;
                _lastRefillTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            }

            /// <summary>
            /// 尝试获取令牌
            /// </summary>
            /// <param name="tokens">请求的令牌数</param>
            /// <returns>是否获取成功</returns>
            public bool TryAcquire(int tokens = 1)
            {
                lock (_lock)
                {
                    Refill();

                    if (_tokens >= tokens)
                    {
                        _tokens -= tokens;
                        return true;
                    }

                    return false;
                }
            }

            /// <summary>
            /// 异步等待获取令牌
            /// </summary>
            /// <param name="tokens">请求的令牌数</param>
            /// <param name="cancellationToken">取消令牌</param>
            public async Task WaitAsync(int tokens = 1, CancellationToken cancellationToken = default)
            {
                while (!TryAcquire(tokens))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await Task.Delay(10, cancellationToken);
                }
            }

            /// <summary>
            /// 获取当前可用令牌数
            /// </summary>
            public double AvailableTokens
            {
                get
                {
                    lock (_lock)
                    {
                        Refill();
                        return _tokens;
                    }
                }
            }

            private void Refill()
            {
                var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                var elapsed = now - _lastRefillTime;
                var refill = elapsed * _refillRate / 1000.0;

                if (refill > 0)
                {
                    _tokens = Math.Min(_capacity, _tokens + refill);
                    _lastRefillTime = now;
                }
            }
        }

        #endregion

        #region 漏桶限流器

        /// <summary>
        /// 创建漏桶限流器
        /// </summary>
        /// <param name="capacity">桶容量</param>
        /// <param name="leakRate">每秒漏出的请求数</param>
        /// <returns>漏桶限流器</returns>
        public static LeakyBucketLimiter CreateLeakyBucket(int capacity, double leakRate)
        {
            return new LeakyBucketLimiter(capacity, leakRate);
        }

        /// <summary>
        /// 漏桶限流器
        /// </summary>
        public class LeakyBucketLimiter
        {
            private readonly int _capacity;
            private readonly double _leakRate;
            private double _water;
            private long _lastLeakTime;
            private readonly object _lock = new();

            public LeakyBucketLimiter(int capacity, double leakRate)
            {
                if (capacity <= 0)
                    throw new ArgumentOutOfRangeException(nameof(capacity), "容量必须大于0");
                if (leakRate <= 0)
                    throw new ArgumentOutOfRangeException(nameof(leakRate), "漏出速率必须大于0");

                _capacity = capacity;
                _leakRate = leakRate;
                _water = 0;
                _lastLeakTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            }

            /// <summary>
            /// 尝试添加请求到桶中
            /// </summary>
            /// <returns>是否添加成功</returns>
            public bool TryAcquire()
            {
                lock (_lock)
                {
                    Leak();

                    if (_water < _capacity)
                    {
                        _water++;
                        return true;
                    }

                    return false;
                }
            }

            /// <summary>
            /// 异步等待
            /// </summary>
            public async Task WaitAsync(CancellationToken cancellationToken = default)
            {
                while (!TryAcquire())
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await Task.Delay(10, cancellationToken);
                }
            }

            private void Leak()
            {
                var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                var elapsed = now - _lastLeakTime;
                var leaked = elapsed * _leakRate / 1000.0;

                if (leaked > 0)
                {
                    _water = Math.Max(0, _water - leaked);
                    _lastLeakTime = now;
                }
            }
        }

        #endregion

        #region 滑动窗口限流器

        /// <summary>
        /// 创建滑动窗口限流器
        /// </summary>
        /// <param name="limit">窗口内最大请求数</param>
        /// <param name="windowSeconds">窗口大小（秒）</param>
        /// <returns>滑动窗口限流器</returns>
        public static SlidingWindowLimiter CreateSlidingWindow(int limit, int windowSeconds)
        {
            return new SlidingWindowLimiter(limit, windowSeconds);
        }

        /// <summary>
        /// 滑动窗口限流器
        /// </summary>
        public class SlidingWindowLimiter
        {
            private readonly int _limit;
            private readonly long _windowTicks;
            private readonly ConcurrentQueue<long> _timestamps = new();
            private readonly object _lock = new();

            public SlidingWindowLimiter(int limit, int windowSeconds)
            {
                if (limit <= 0)
                    throw new ArgumentOutOfRangeException(nameof(limit), "限制必须大于0");
                if (windowSeconds <= 0)
                    throw new ArgumentOutOfRangeException(nameof(windowSeconds), "窗口大小必须大于0");

                _limit = limit;
                _windowTicks = windowSeconds * 1000L;
            }

            /// <summary>
            /// 尝试通过请求
            /// </summary>
            /// <returns>是否允许通过</returns>
            public bool TryAcquire()
            {
                lock (_lock)
                {
                    var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    var windowStart = now - _windowTicks;

                    // 移除过期的请求记录
                    while (_timestamps.TryPeek(out var timestamp) && timestamp < windowStart)
                    {
                        _timestamps.TryDequeue(out _);
                    }

                    if (_timestamps.Count < _limit)
                    {
                        _timestamps.Enqueue(now);
                        return true;
                    }

                    return false;
                }
            }

            /// <summary>
            /// 异步等待
            /// </summary>
            public async Task WaitAsync(CancellationToken cancellationToken = default)
            {
                while (!TryAcquire())
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await Task.Delay(10, cancellationToken);
                }
            }

            /// <summary>
            /// 获取当前窗口内的请求数
            /// </summary>
            public int CurrentCount
            {
                get
                {
                    lock (_lock)
                    {
                        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                        var windowStart = now - _windowTicks;

                        while (_timestamps.TryPeek(out var timestamp) && timestamp < windowStart)
                        {
                            _timestamps.TryDequeue(out _);
                        }

                        return _timestamps.Count;
                    }
                }
            }
        }

        #endregion

        #region 固定窗口限流器

        /// <summary>
        /// 创建固定窗口限流器
        /// </summary>
        /// <param name="limit">窗口内最大请求数</param>
        /// <param name="windowSeconds">窗口大小（秒）</param>
        /// <returns>固定窗口限流器</returns>
        public static FixedWindowLimiter CreateFixedWindow(int limit, int windowSeconds)
        {
            return new FixedWindowLimiter(limit, windowSeconds);
        }

        /// <summary>
        /// 固定窗口限流器
        /// </summary>
        public class FixedWindowLimiter
        {
            private readonly int _limit;
            private readonly long _windowTicks;
            private int _count;
            private long _windowStart;
            private readonly object _lock = new();

            public FixedWindowLimiter(int limit, int windowSeconds)
            {
                if (limit <= 0)
                    throw new ArgumentOutOfRangeException(nameof(limit), "限制必须大于0");
                if (windowSeconds <= 0)
                    throw new ArgumentOutOfRangeException(nameof(windowSeconds), "窗口大小必须大于0");

                _limit = limit;
                _windowTicks = windowSeconds * 1000L;
                _count = 0;
                _windowStart = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            }

            /// <summary>
            /// 尝试通过请求
            /// </summary>
            /// <returns>是否允许通过</returns>
            public bool TryAcquire()
            {
                lock (_lock)
                {
                    var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                    // 检查是否需要重置窗口
                    if (now - _windowStart >= _windowTicks)
                    {
                        _windowStart = now;
                        _count = 0;
                    }

                    if (_count < _limit)
                    {
                        _count++;
                        return true;
                    }

                    return false;
                }
            }

            /// <summary>
            /// 异步等待
            /// </summary>
            public async Task WaitAsync(CancellationToken cancellationToken = default)
            {
                while (!TryAcquire())
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await Task.Delay(10, cancellationToken);
                }
            }

            /// <summary>
            /// 获取当前窗口内的请求数
            /// </summary>
            public int CurrentCount
            {
                get
                {
                    lock (_lock)
                    {
                        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                        if (now - _windowStart >= _windowTicks)
                        {
                            return 0;
                        }

                        return _count;
                    }
                }
            }

            /// <summary>
            /// 获取窗口重置剩余时间（毫秒）
            /// </summary>
            public long ResetIn
            {
                get
                {
                    lock (_lock)
                    {
                        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                        var elapsed = now - _windowStart;
                        return Math.Max(0, _windowTicks - elapsed);
                    }
                }
            }
        }

        #endregion

        #region 并发限流器

        /// <summary>
        /// 创建并发限流器
        /// </summary>
        /// <param name="maxConcurrency">最大并发数</param>
        /// <returns>并发限流器</returns>
        public static ConcurrencyLimiter CreateConcurrency(int maxConcurrency)
        {
            return new ConcurrencyLimiter(maxConcurrency);
        }

        /// <summary>
        /// 并发限流器
        /// </summary>
        public class ConcurrencyLimiter
        {
            private readonly SemaphoreSlim _semaphore;

            public ConcurrencyLimiter(int maxConcurrency)
            {
                if (maxConcurrency <= 0)
                    throw new ArgumentOutOfRangeException(nameof(maxConcurrency), "最大并发数必须大于0");

                _semaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);
            }

            /// <summary>
            /// 获取执行许可
            /// </summary>
            public async Task<IDisposable> AcquireAsync(CancellationToken cancellationToken = default)
            {
                await _semaphore.WaitAsync(cancellationToken);
                return new ReleaseDisposable(_semaphore);
            }

            /// <summary>
            /// 尝试获取执行许可
            /// </summary>
            public IDisposable? TryAcquire()
            {
                if (_semaphore.Wait(0))
                {
                    return new ReleaseDisposable(_semaphore);
                }
                return null;
            }

            /// <summary>
            /// 当前可用许可数
            /// </summary>
            public int AvailablePermits => _semaphore.CurrentCount;

            private class ReleaseDisposable : IDisposable
            {
                private readonly SemaphoreSlim _semaphore;

                public ReleaseDisposable(SemaphoreSlim semaphore)
                {
                    _semaphore = semaphore;
                }

                public void Dispose()
                {
                    _semaphore.Release();
                }
            }
        }

        #endregion

        #region 分布式限流器（内存模拟版）

        /// <summary>
        /// 创建分布式限流器（基于内存的键值对）
        /// </summary>
        /// <param name="defaultLimit">默认限制</param>
        /// <param name="windowSeconds">窗口大小（秒）</param>
        /// <returns>分布式限流器</returns>
        public static DistributedLimiter CreateDistributed(int defaultLimit, int windowSeconds)
        {
            return new DistributedLimiter(defaultLimit, windowSeconds);
        }

        /// <summary>
        /// 分布式限流器（内存模拟）
        /// </summary>
        public class DistributedLimiter
        {
            private readonly int _defaultLimit;
            private readonly int _windowSeconds;
            private readonly ConcurrentDictionary<string, FixedWindowLimiter> _limiters = new();

            public DistributedLimiter(int defaultLimit, int windowSeconds)
            {
                _defaultLimit = defaultLimit;
                _windowSeconds = windowSeconds;
            }

            /// <summary>
            /// 尝试通过请求
            /// </summary>
            /// <param name="key">限流键（如用户ID、IP等）</param>
            /// <returns>是否允许通过</returns>
            public bool TryAcquire(string key)
            {
                var limiter = _limiters.GetOrAdd(key, _ => new FixedWindowLimiter(_defaultLimit, _windowSeconds));
                return limiter.TryAcquire();
            }

            /// <summary>
            /// 异步等待
            /// </summary>
            public async Task WaitAsync(string key, CancellationToken cancellationToken = default)
            {
                while (!TryAcquire(key))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await Task.Delay(10, cancellationToken);
                }
            }

            /// <summary>
            /// 移除指定键的限流器
            /// </summary>
            public void Remove(string key)
            {
                _limiters.TryRemove(key, out _);
            }

            /// <summary>
            /// 清除所有限流器
            /// </summary>
            public void Clear()
            {
                _limiters.Clear();
            }
        }

        #endregion
    }
}
