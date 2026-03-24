using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace EasyTool.CollectionsCategory
{
    /// <summary>
    /// LFU（最不经常使用）缓存工具类
    /// </summary>
    public static class LFUCacheUtil
    {
        /// <summary>
        /// 创建 LFU 缓存
        /// </summary>
        public static LFUCache<TKey, TValue> Create<TKey, TValue>(int capacity)
        {
            return new LFUCache<TKey, TValue>(capacity);
        }
    }

    /// <summary>
    /// LFU 缓存实现
    /// </summary>
    public class LFUCache<TKey, TValue>
    {
        private readonly int _capacity;
        private readonly Dictionary<TKey, CacheItem> _cache;
        private readonly Dictionary<int, LinkedList<TKey>> _frequencyLists;
        private int _minFrequency;

        private class CacheItem
        {
            public TValue Value { get; set; }
            public int Frequency { get; set; }
            public LinkedListNode<TKey> Node { get; set; }
        }

        /// <summary>
        /// 当前元素数量
        /// </summary>
        public int Count => _cache.Count;

        /// <summary>
        /// 缓存容量
        /// </summary>
        public int Capacity => _capacity;

        /// <summary>
        /// 创建 LFU 缓存
        /// </summary>
        public LFUCache(int capacity)
        {
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(capacity));

            _capacity = capacity;
            _cache = new Dictionary<TKey, CacheItem>();
            _frequencyLists = new Dictionary<int, LinkedList<TKey>>();
            _minFrequency = 0;
        }

        /// <summary>
        /// 获取值
        /// </summary>
        public bool TryGet(TKey key, out TValue value)
        {
            if (_cache.TryGetValue(key, out var item))
            {
                UpdateFrequency(item);
                value = item.Value;
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// 获取或添加值
        /// </summary>
        public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
        {
            if (TryGet(key, out var value))
                return value;

            value = valueFactory(key);
            Add(key, value);
            return value;
        }

        /// <summary>
        /// 添加或更新值
        /// </summary>
        public void Add(TKey key, TValue value)
        {
            if (_cache.TryGetValue(key, out var item))
            {
                item.Value = value;
                UpdateFrequency(item);
                return;
            }

            if (_cache.Count >= _capacity)
            {
                Evict();
            }

            var newNode = new CacheItem { Value = value, Frequency = 1 };
            if (!_frequencyLists.ContainsKey(1))
            {
                _frequencyLists[1] = new LinkedList<TKey>();
            }
            newNode.Node = _frequencyLists[1].AddLast(key);
            _cache[key] = newNode;
            _minFrequency = 1;
        }

        /// <summary>
        /// 移除指定键
        /// </summary>
        public bool Remove(TKey key)
        {
            if (!_cache.TryGetValue(key, out var item))
                return false;

            _frequencyLists[item.Frequency].Remove(item.Node);
            _cache.Remove(key);
            return true;
        }

        /// <summary>
        /// 清空缓存
        /// </summary>
        public void Clear()
        {
            _cache.Clear();
            _frequencyLists.Clear();
            _minFrequency = 0;
        }

        /// <summary>
        /// 是否包含键
        /// </summary>
        public bool ContainsKey(TKey key)
        {
            return _cache.ContainsKey(key);
        }

        private void UpdateFrequency(CacheItem item)
        {
            int oldFreq = item.Frequency;
            int newFreq = oldFreq + 1;

            _frequencyLists[oldFreq].Remove(item.Node);
            if (_frequencyLists[oldFreq].Count == 0)
            {
                _frequencyLists.Remove(oldFreq);
                if (_minFrequency == oldFreq)
                {
                    _minFrequency = newFreq;
                }
            }

            item.Frequency = newFreq;
            if (!_frequencyLists.ContainsKey(newFreq))
            {
                _frequencyLists[newFreq] = new LinkedList<TKey>();
            }
            item.Node = _frequencyLists[newFreq].AddLast(_cache.First(x => x.Value == item).Key);
        }

        private void Evict()
        {
            if (_minFrequency == 0 || !_frequencyLists.ContainsKey(_minFrequency))
                return;

            var list = _frequencyLists[_minFrequency];
            var keyToRemove = list.First.Value;
            list.RemoveFirst();
            _cache.Remove(keyToRemove);

            if (list.Count == 0)
            {
                _frequencyLists.Remove(_minFrequency);
            }
        }
    }

    /// <summary>
    /// FIFO（先进先出）缓存工具类
    /// </summary>
    public static class FIFOCacheUtil
    {
        /// <summary>
        /// 创建 FIFO 缓存
        /// </summary>
        public static FIFOCache<TKey, TValue> Create<TKey, TValue>(int capacity)
        {
            return new FIFOCache<TKey, TValue>(capacity);
        }
    }

    /// <summary>
    /// FIFO 缓存实现
    /// </summary>
    public class FIFOCache<TKey, TValue>
    {
        private readonly int _capacity;
        private readonly Dictionary<TKey, TValue> _cache;
        private readonly Queue<TKey> _queue;

        /// <summary>
        /// 当前元素数量
        /// </summary>
        public int Count => _cache.Count;

        /// <summary>
        /// 缓存容量
        /// </summary>
        public int Capacity => _capacity;

        /// <summary>
        /// 创建 FIFO 缓存
        /// </summary>
        public FIFOCache(int capacity)
        {
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(capacity));

            _capacity = capacity;
            _cache = new Dictionary<TKey, TValue>();
            _queue = new Queue<TKey>();
        }

        /// <summary>
        /// 获取值
        /// </summary>
        public bool TryGet(TKey key, out TValue value)
        {
            return _cache.TryGetValue(key, out value);
        }

        /// <summary>
        /// 获取或添加值
        /// </summary>
        public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
        {
            if (_cache.TryGetValue(key, out var value))
                return value;

            value = valueFactory(key);
            Add(key, value);
            return value;
        }

        /// <summary>
        /// 添加或更新值
        /// </summary>
        public void Add(TKey key, TValue value)
        {
            if (_cache.ContainsKey(key))
            {
                _cache[key] = value;
                return;
            }

            if (_cache.Count >= _capacity)
            {
                var oldestKey = _queue.Dequeue();
                _cache.Remove(oldestKey);
            }

            _cache[key] = value;
            _queue.Enqueue(key);
        }

        /// <summary>
        /// 移除指定键
        /// </summary>
        public bool Remove(TKey key)
        {
            if (!_cache.Remove(key))
                return false;

            // 需要重建队列以移除中间元素
            var newQueue = new Queue<TKey>();
            foreach (var k in _queue)
            {
                if (!EqualityComparer<TKey>.Default.Equals(k, key))
                {
                    newQueue.Enqueue(k);
                }
            }
            return true;
        }

        /// <summary>
        /// 清空缓存
        /// </summary>
        public void Clear()
        {
            _cache.Clear();
            _queue.Clear();
        }

        /// <summary>
        /// 是否包含键
        /// </summary>
        public bool ContainsKey(TKey key)
        {
            return _cache.ContainsKey(key);
        }
    }

    /// <summary>
    /// 定时缓存工具类
    /// </summary>
    public static class TimedCacheUtil
    {
        /// <summary>
        /// 创建定时缓存
        /// </summary>
        public static TimedCache<TKey, TValue> Create<TKey, TValue>(TimeSpan expiration)
        {
            return new TimedCache<TKey, TValue>(expiration);
        }

        /// <summary>
        /// 创建滑动过期缓存
        /// </summary>
        public static TimedCache<TKey, TValue> CreateSliding<TKey, TValue>(TimeSpan expiration)
        {
            return new TimedCache<TKey, TValue>(expiration, true);
        }
    }

    /// <summary>
    /// 定时缓存实现
    /// </summary>
    public class TimedCache<TKey, TValue>
    {
        private readonly TimeSpan _expiration;
        private readonly bool _slidingExpiration;
        private readonly Dictionary<TKey, CacheItem> _cache;

        private class CacheItem
        {
            public TValue Value { get; set; }
            public DateTime ExpirationTime { get; set; }
        }

        /// <summary>
        /// 当前元素数量
        /// </summary>
        public int Count => _cache.Count;

        /// <summary>
        /// 过期时间
        /// </summary>
        public TimeSpan Expiration => _expiration;

        /// <summary>
        /// 创建定时缓存
        /// </summary>
        public TimedCache(TimeSpan expiration, bool slidingExpiration = false)
        {
            if (expiration <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(expiration));

            _expiration = expiration;
            _slidingExpiration = slidingExpiration;
            _cache = new Dictionary<TKey, CacheItem>();
        }

        /// <summary>
        /// 获取值
        /// </summary>
        public bool TryGet(TKey key, out TValue value)
        {
            CleanupExpired();

            if (_cache.TryGetValue(key, out var item))
            {
                if (DateTime.UtcNow < item.ExpirationTime)
                {
                    if (_slidingExpiration)
                    {
                        item.ExpirationTime = DateTime.UtcNow.Add(_expiration);
                    }
                    value = item.Value;
                    return true;
                }
                _cache.Remove(key);
            }

            value = default;
            return false;
        }

        /// <summary>
        /// 获取或添加值
        /// </summary>
        public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
        {
            if (TryGet(key, out var value))
                return value;

            value = valueFactory(key);
            Add(key, value);
            return value;
        }

        /// <summary>
        /// 添加或更新值
        /// </summary>
        public void Add(TKey key, TValue value)
        {
            _cache[key] = new CacheItem
            {
                Value = value,
                ExpirationTime = DateTime.UtcNow.Add(_expiration)
            };
        }

        /// <summary>
        /// 添加带自定义过期时间的值
        /// </summary>
        public void Add(TKey key, TValue value, TimeSpan customExpiration)
        {
            _cache[key] = new CacheItem
            {
                Value = value,
                ExpirationTime = DateTime.UtcNow.Add(customExpiration)
            };
        }

        /// <summary>
        /// 移除指定键
        /// </summary>
        public bool Remove(TKey key)
        {
            return _cache.Remove(key);
        }

        /// <summary>
        /// 清空缓存
        /// </summary>
        public void Clear()
        {
            _cache.Clear();
        }

        /// <summary>
        /// 是否包含键（未过期）
        /// </summary>
        public bool ContainsKey(TKey key)
        {
            CleanupExpired();
            return _cache.ContainsKey(key);
        }

        /// <summary>
        /// 清理过期项
        /// </summary>
        public void CleanupExpired()
        {
            var now = DateTime.UtcNow;
            var expired = _cache.Where(x => x.Value.ExpirationTime <= now).Select(x => x.Key).ToList();
            foreach (var key in expired)
            {
                _cache.Remove(key);
            }
        }
    }

    /// <summary>
    /// 限流器工具类
    /// </summary>
    public static class RateLimiterUtil
    {
        /// <summary>
        /// 创建令牌桶限流器
        /// </summary>
        public static TokenBucketRateLimiter CreateTokenBucket(int capacity, int refillRate, TimeSpan refillPeriod)
        {
            return new TokenBucketRateLimiter(capacity, refillRate, refillPeriod);
        }

        /// <summary>
        /// 创建滑动窗口限流器
        /// </summary>
        public static SlidingWindowRateLimiter CreateSlidingWindow(int limit, TimeSpan window)
        {
            return new SlidingWindowRateLimiter(limit, window);
        }

        /// <summary>
        /// 创建固定窗口限流器
        /// </summary>
        public static FixedWindowRateLimiter CreateFixedWindow(int limit, TimeSpan window)
        {
            return new FixedWindowRateLimiter(limit, window);
        }
    }

    /// <summary>
    /// 令牌桶限流器
    /// </summary>
    public class TokenBucketRateLimiter
    {
        private readonly int _capacity;
        private readonly int _refillRate;
        private readonly TimeSpan _refillPeriod;
        private double _tokens;
        private DateTime _lastRefill;
        private readonly object _lock = new object();

        /// <summary>
        /// 桶容量
        /// </summary>
        public int Capacity => _capacity;

        /// <summary>
        /// 当前令牌数
        /// </summary>
        public int AvailableTokens
        {
            get
            {
                lock (_lock)
                {
                    Refill();
                    return (int)_tokens;
                }
            }
        }

        /// <summary>
        /// 创建令牌桶限流器
        /// </summary>
        public TokenBucketRateLimiter(int capacity, int refillRate, TimeSpan refillPeriod)
        {
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(capacity));
            if (refillRate <= 0)
                throw new ArgumentOutOfRangeException(nameof(refillRate));

            _capacity = capacity;
            _refillRate = refillRate;
            _refillPeriod = refillPeriod;
            _tokens = capacity;
            _lastRefill = DateTime.UtcNow;
        }

        /// <summary>
        /// 尝试获取令牌
        /// </summary>
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
        /// 等待获取令牌
        /// </summary>
        public void Acquire(int tokens = 1)
        {
            while (!TryAcquire(tokens))
            {
                Thread.Sleep(10);
            }
        }

        private void Refill()
        {
            var now = DateTime.UtcNow;
            var elapsed = now - _lastRefill;
            var tokensToAdd = elapsed.TotalMilliseconds / _refillPeriod.TotalMilliseconds * _refillRate;

            if (tokensToAdd > 0)
            {
                _tokens = Math.Min(_capacity, _tokens + tokensToAdd);
                _lastRefill = now;
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
        private readonly Queue<DateTime> _timestamps;
        private readonly object _lock = new object();

        /// <summary>
        /// 限制
        /// </summary>
        public int Limit => _limit;

        /// <summary>
        /// 窗口大小
        /// </summary>
        public TimeSpan Window => _window;

        /// <summary>
        /// 创建滑动窗口限流器
        /// </summary>
        public SlidingWindowRateLimiter(int limit, TimeSpan window)
        {
            if (limit <= 0)
                throw new ArgumentOutOfRangeException(nameof(limit));

            _limit = limit;
            _window = window;
            _timestamps = new Queue<DateTime>();
        }

        /// <summary>
        /// 尝试获取许可
        /// </summary>
        public bool TryAcquire()
        {
            lock (_lock)
            {
                Cleanup();
                if (_timestamps.Count < _limit)
                {
                    _timestamps.Enqueue(DateTime.UtcNow);
                    return true;
                }
                return false;
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
                    Cleanup();
                    return _timestamps.Count;
                }
            }
        }

        private void Cleanup()
        {
            var cutoff = DateTime.UtcNow - _window;
            while (_timestamps.Count > 0 && _timestamps.Peek() < cutoff)
            {
                _timestamps.Dequeue();
            }
        }
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
        private readonly object _lock = new object();

        /// <summary>
        /// 限制
        /// </summary>
        public int Limit => _limit;

        /// <summary>
        /// 窗口大小
        /// </summary>
        public TimeSpan Window => _window;

        /// <summary>
        /// 创建固定窗口限流器
        /// </summary>
        public FixedWindowRateLimiter(int limit, TimeSpan window)
        {
            if (limit <= 0)
                throw new ArgumentOutOfRangeException(nameof(limit));

            _limit = limit;
            _window = window;
            _count = 0;
            _windowStart = DateTime.UtcNow;
        }

        /// <summary>
        /// 尝试获取许可
        /// </summary>
        public bool TryAcquire()
        {
            lock (_lock)
            {
                var now = DateTime.UtcNow;
                if (now - _windowStart >= _window)
                {
                    _count = 0;
                    _windowStart = now;
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
        /// 获取当前窗口内的请求数
        /// </summary>
        public int CurrentCount
        {
            get
            {
                lock (_lock)
                {
                    var now = DateTime.UtcNow;
                    if (now - _windowStart >= _window)
                        return 0;
                    return _count;
                }
            }
        }

        /// <summary>
        /// 获取距离下一个窗口的时间
        /// </summary>
        public TimeSpan TimeUntilNextWindow
        {
            get
            {
                lock (_lock)
                {
                    var elapsed = DateTime.UtcNow - _windowStart;
                    return _window - elapsed;
                }
            }
        }
    }

    /// <summary>
    /// 对象池工具类
    /// </summary>
    public static class ObjectPoolUtil
    {
        /// <summary>
        /// 创建对象池
        /// </summary>
        public static ObjectPool<T> Create<T>(Func<T> factory, int maxSize = 100) where T : class
        {
            return new ObjectPool<T>(factory, maxSize);
        }
    }

    /// <summary>
    /// 对象池实现
    /// </summary>
    public class ObjectPool<T> where T : class
    {
        private readonly Func<T> _factory;
        private readonly int _maxSize;
        private readonly Stack<T> _pool;
        private readonly object _lock = new object();

        /// <summary>
        /// 池中可用对象数
        /// </summary>
        public int AvailableCount
        {
            get
            {
                lock (_lock)
                {
                    return _pool.Count;
                }
            }
        }

        /// <summary>
        /// 最大大小
        /// </summary>
        public int MaxSize => _maxSize;

        /// <summary>
        /// 创建对象池
        /// </summary>
        public ObjectPool(Func<T> factory, int maxSize = 100)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _maxSize = maxSize > 0 ? maxSize : throw new ArgumentOutOfRangeException(nameof(maxSize));
            _pool = new Stack<T>();
        }

        /// <summary>
        /// 获取对象
        /// </summary>
        public T Get()
        {
            lock (_lock)
            {
                if (_pool.Count > 0)
                {
                    return _pool.Pop();
                }
            }
            return _factory();
        }

        /// <summary>
        /// 归还对象
        /// </summary>
        public void Return(T obj)
        {
            if (obj == null)
                return;

            // 如果对象实现了 IResettable，重置它
            if (obj is IResettable resettable)
            {
                resettable.Reset();
            }

            lock (_lock)
            {
                if (_pool.Count < _maxSize)
                {
                    _pool.Push(obj);
                }
            }
        }

        /// <summary>
        /// 清空池
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                _pool.Clear();
            }
        }

        /// <summary>
        /// 预热池
        /// </summary>
        public void Warmup(int count)
        {
            for (int i = 0; i < count && i < _maxSize; i++)
            {
                Return(_factory());
            }
        }
    }

    /// <summary>
    /// 可重置接口
    /// </summary>
    public interface IResettable
    {
        /// <summary>
        /// 重置对象状态
        /// </summary>
        void Reset();
    }
}
