using System;
using System.Threading;
using System.Threading.Tasks;

namespace EasyTool.ToolCategory
{
    /// <summary>
    /// 限流算法
    /// </summary>
    public enum RateLimitAlgorithm
    {
        /// <summary>
        /// 固定窗口
        /// </summary>
        FixedWindow,

        /// <summary>
        /// 滑动窗口
        /// </summary>
        SlidingWindow,

        /// <summary>
        /// 令牌桶
        /// </summary>
        TokenBucket,

        /// <summary>
        /// 漏桶
        /// </summary>
        LeakyBucket
    }

    /// <summary>
    /// 固定窗口限流器
    /// </summary>
    public class FixedWindowRateLimiter
    {
        private readonly int _limit;
        private readonly TimeSpan _window;
        private int _count;
        private DateTime _windowStart;
        private readonly object _lock = new();

        public FixedWindowRateLimiter(int limit, TimeSpan window)
        {
            _limit = limit;
            _window = window;
            _count = 0;
            _windowStart = DateTime.UtcNow;
        }

        public bool TryAcquire()
        {
            lock (_lock)
            {
                var now = DateTime.UtcNow;
                if (now - _windowStart >= _window)
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

        public TimeSpan GetWaitTime()
        {
            lock (_lock)
            {
                var remaining = _window - (DateTime.UtcNow - _windowStart);
                return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
            }
        }
    }

    /// <summary>
    /// 滑动窗口限流器
    /// </summary>
    public class SlidingWindowRateLimiter
    {
        private readonly int _limit;
        private readonly TimeSpan _window;
        private readonly System.Collections.Generic.Queue<DateTime> _timestamps;
        private readonly object _lock = new();

        public SlidingWindowRateLimiter(int limit, TimeSpan window)
        {
            _limit = limit;
            _window = window;
            _timestamps = new();
        }

        public bool TryAcquire()
        {
            lock (_lock)
            {
                var now = DateTime.UtcNow;
                var cutoff = now - _window;

                while (_timestamps.Count > 0 && _timestamps.Peek() < cutoff)
                {
                    _timestamps.Dequeue();
                }

                if (_timestamps.Count < _limit)
                {
                    _timestamps.Enqueue(now);
                    return true;
                }
                return false;
            }
        }

        public TimeSpan GetWaitTime()
        {
            lock (_lock)
            {
                if (_timestamps.Count == 0)
                    return TimeSpan.Zero;

                var oldest = _timestamps.Peek();
                var waitTime = oldest + _window - DateTime.UtcNow;
                return waitTime > TimeSpan.Zero ? waitTime : TimeSpan.Zero;
            }
        }
    }

    /// <summary>
    /// 令牌桶限流器
    /// </summary>
    public class TokenBucketRateLimiter
    {
        private readonly int _capacity;
        private readonly int _refillRate;
        private int _tokens;
        private DateTime _lastRefill;
        private readonly object _lock = new();

        public TokenBucketRateLimiter(int capacity, int refillRate)
        {
            _capacity = capacity;
            _refillRate = refillRate;
            _tokens = capacity;
            _lastRefill = DateTime.UtcNow;
        }

        public bool TryAcquire(int tokens = 1)
        {
            lock (_lock)
            {
                RefillTokens();

                if (_tokens >= tokens)
                {
                    _tokens -= tokens;
                    return true;
                }
                return false;
            }
        }

        private void RefillTokens()
        {
            var now = DateTime.UtcNow;
            var elapsed = (now - _lastRefill).TotalSeconds;
            var tokensToAdd = (int)(elapsed * _refillRate);

            if (tokensToAdd > 0)
            {
                _tokens = Math.Min(_capacity, _tokens + tokensToAdd);
                _lastRefill = now;
            }
        }

        public TimeSpan GetWaitTime(int tokens = 1)
        {
            lock (_lock)
            {
                RefillTokens();
                if (_tokens >= tokens)
                    return TimeSpan.Zero;

                var tokensNeeded = tokens - _tokens;
                return TimeSpan.FromSeconds((double)tokensNeeded / _refillRate);
            }
        }
    }

    /// <summary>
    /// 漏桶限流器
    /// </summary>
    public class LeakyBucketRateLimiter
    {
        private readonly int _capacity;
        private readonly int _leakRate;
        private int _water;
        private DateTime _lastLeak;
        private readonly object _lock = new();

        public LeakyBucketRateLimiter(int capacity, int leakRate)
        {
            _capacity = capacity;
            _leakRate = leakRate;
            _water = 0;
            _lastLeak = DateTime.UtcNow;
        }

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

        private void Leak()
        {
            var now = DateTime.UtcNow;
            var elapsed = (now - _lastLeak).TotalSeconds;
            var leaked = (int)(elapsed * _leakRate);

            if (leaked > 0)
            {
                _water = Math.Max(0, _water - leaked);
                _lastLeak = now;
            }
        }

        public TimeSpan GetWaitTime()
        {
            lock (_lock)
            {
                Leak();
                if (_water < _capacity)
                    return TimeSpan.Zero;

                return TimeSpan.FromSeconds((double)1 / _leakRate);
            }
        }
    }

    /// <summary>
    /// 限流器工具类
    /// </summary>
    public static class RateLimiter
    {
        /// <summary>
        /// 创建限流器
        /// </summary>
        public static IRateLimiter Create(RateLimitAlgorithm algorithm, int limit, TimeSpan window)
        {
            return algorithm switch
            {
                RateLimitAlgorithm.FixedWindow => new FixedWindowRateLimiterWrapper(limit, window),
                RateLimitAlgorithm.SlidingWindow => new SlidingWindowRateLimiterWrapper(limit, window),
                RateLimitAlgorithm.TokenBucket => new TokenBucketRateLimiterWrapper(limit, (int)(limit / window.TotalSeconds)),
                RateLimitAlgorithm.LeakyBucket => new LeakyBucketRateLimiterWrapper(limit, (int)(limit / window.TotalSeconds)),
                _ => throw new ArgumentException("不支持的限流算法")
            };
        }

        /// <summary>
        /// 使用限流器执行操作
        /// </summary>
        public static async Task<T> ExecuteWithRateLimitAsync<T>(IRateLimiter limiter, Func<Task<T>> action)
        {
            while (!limiter.TryAcquire())
            {
                await Task.Delay(limiter.GetWaitTime());
            }
            return await action();
        }

        /// <summary>
        /// 使用限流器执行操作
        /// </summary>
        public static async Task ExecuteWithRateLimitAsync(IRateLimiter limiter, Func<Task> action)
        {
            while (!limiter.TryAcquire())
            {
                await Task.Delay(limiter.GetWaitTime());
            }
            await action();
        }
    }

    /// <summary>
    /// 限流器接口
    /// </summary>
    public interface IRateLimiter
    {
        bool TryAcquire();
        TimeSpan GetWaitTime();
    }

    internal class FixedWindowRateLimiterWrapper : IRateLimiter
    {
        private readonly FixedWindowRateLimiter _limiter;
        public FixedWindowRateLimiterWrapper(int limit, TimeSpan window) => _limiter = new FixedWindowRateLimiter(limit, window);
        public bool TryAcquire() => _limiter.TryAcquire();
        public TimeSpan GetWaitTime() => _limiter.GetWaitTime();
    }

    internal class SlidingWindowRateLimiterWrapper : IRateLimiter
    {
        private readonly SlidingWindowRateLimiter _limiter;
        public SlidingWindowRateLimiterWrapper(int limit, TimeSpan window) => _limiter = new SlidingWindowRateLimiter(limit, window);
        public bool TryAcquire() => _limiter.TryAcquire();
        public TimeSpan GetWaitTime() => _limiter.GetWaitTime();
    }

    internal class TokenBucketRateLimiterWrapper : IRateLimiter
    {
        private readonly TokenBucketRateLimiter _limiter;
        public TokenBucketRateLimiterWrapper(int capacity, int refillRate) => _limiter = new TokenBucketRateLimiter(capacity, refillRate);
        public bool TryAcquire() => _limiter.TryAcquire();
        public TimeSpan GetWaitTime() => _limiter.GetWaitTime();
    }

    internal class LeakyBucketRateLimiterWrapper : IRateLimiter
    {
        private readonly LeakyBucketRateLimiter _limiter;
        public LeakyBucketRateLimiterWrapper(int capacity, int leakRate) => _limiter = new LeakyBucketRateLimiter(capacity, leakRate);
        public bool TryAcquire() => _limiter.TryAcquire();
        public TimeSpan GetWaitTime() => _limiter.GetWaitTime();
    }
}
