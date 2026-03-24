using System;
using System.Threading;
using System.Threading.Tasks;

namespace EasyTool.ToolCategory
{
    /// <summary>
    /// 重试工具类
    /// 提供可配置的重试机制
    /// </summary>
    public static class RetryUtil
    {
        /// <summary>
        /// 执行带重试的操作
        /// </summary>
        /// <param name="action">要执行的操作</param>
        /// <param name="maxRetries">最大重试次数</param>
        /// <param name="delay">重试间隔（毫秒）</param>
        /// <param name="exponentialBackoff">是否使用指数退避</param>
        public static void Execute(Action action, int maxRetries = 3, int delay = 1000, bool exponentialBackoff = true)
        {
            Execute<object>(() =>
            {
                action();
                return null;
            }, maxRetries, delay, exponentialBackoff);
        }

        /// <summary>
        /// 执行带重试的函数
        /// </summary>
        public static T Execute<T>(Func<T> func, int maxRetries = 3, int delay = 1000, bool exponentialBackoff = true)
        {
            Exception lastException = null;

            for (int attempt = 0; attempt <= maxRetries; attempt++)
            {
                try
                {
                    return func();
                }
                catch (Exception ex)
                {
                    lastException = ex;

                    if (attempt < maxRetries)
                    {
                        int currentDelay = exponentialBackoff ? delay * (int)Math.Pow(2, attempt) : delay;
                        Thread.Sleep(currentDelay);
                    }
                }
            }

            throw new RetryException($"Operation failed after {maxRetries + 1} attempts", lastException);
        }

        /// <summary>
        /// 异步执行带重试的操作
        /// </summary>
        public static async Task ExecuteAsync(Func<Task> action, int maxRetries = 3, int delay = 1000, bool exponentialBackoff = true)
        {
            await ExecuteAsync<object>(async () =>
            {
                await action();
                return null;
            }, maxRetries, delay, exponentialBackoff);
        }

        /// <summary>
        /// 异步执行带重试的函数
        /// </summary>
        public static async Task<T> ExecuteAsync<T>(Func<Task<T>> func, int maxRetries = 3, int delay = 1000, bool exponentialBackoff = true)
        {
            Exception lastException = null;

            for (int attempt = 0; attempt <= maxRetries; attempt++)
            {
                try
                {
                    return await func();
                }
                catch (Exception ex)
                {
                    lastException = ex;

                    if (attempt < maxRetries)
                    {
                        int currentDelay = exponentialBackoff ? delay * (int)Math.Pow(2, attempt) : delay;
                        await Task.Delay(currentDelay);
                    }
                }
            }

            throw new RetryException($"Operation failed after {maxRetries + 1} attempts", lastException);
        }

        /// <summary>
        /// 创建重试策略
        /// </summary>
        public static RetryPolicy CreatePolicy()
        {
            return new RetryPolicy();
        }
    }

    /// <summary>
    /// 重试策略
    /// </summary>
    public class RetryPolicy
    {
        private int _maxRetries = 3;
        private int _initialDelay = 1000;
        private int _maxDelay = 60000;
        private double _backoffMultiplier = 2.0;
        private bool _useJitter = true;
        private Type[] _retryOnExceptions = { typeof(Exception) };
        private Action<Exception, int, TimeSpan> _onRetry;

        /// <summary>
        /// 设置最大重试次数
        /// </summary>
        public RetryPolicy WithMaxRetries(int maxRetries)
        {
            _maxRetries = maxRetries;
            return this;
        }

        /// <summary>
        /// 设置初始延迟
        /// </summary>
        public RetryPolicy WithInitialDelay(int milliseconds)
        {
            _initialDelay = milliseconds;
            return this;
        }

        /// <summary>
        /// 设置最大延迟
        /// </summary>
        public RetryPolicy WithMaxDelay(int milliseconds)
        {
            _maxDelay = milliseconds;
            return this;
        }

        /// <summary>
        /// 设置退避倍数
        /// </summary>
        public RetryPolicy WithBackoffMultiplier(double multiplier)
        {
            _backoffMultiplier = multiplier;
            return this;
        }

        /// <summary>
        /// 启用或禁用抖动
        /// </summary>
        public RetryPolicy WithJitter(bool enable = true)
        {
            _useJitter = enable;
            return this;
        }

        /// <summary>
        /// 设置要重试的异常类型
        /// </summary>
        public RetryPolicy RetryOn<TException>() where TException : Exception
        {
            var list = new System.Collections.Generic.List<Type>(_retryOnExceptions) { typeof(TException) };
            _retryOnExceptions = list.ToArray();
            return this;
        }

        /// <summary>
        /// 设置重试回调
        /// </summary>
        public RetryPolicy OnRetry(Action<Exception, int, TimeSpan> onRetry)
        {
            _onRetry = onRetry;
            return this;
        }

        /// <summary>
        /// 执行操作
        /// </summary>
        public void Execute(Action action)
        {
            Execute<object>(() =>
            {
                action();
                return null;
            });
        }

        /// <summary>
        /// 执行函数
        /// </summary>
        public T Execute<T>(Func<T> func)
        {
            Exception lastException = null;

            for (int attempt = 0; attempt <= _maxRetries; attempt++)
            {
                try
                {
                    return func();
                }
                catch (Exception ex)
                {
                    lastException = ex;

                    if (!ShouldRetry(ex) || attempt >= _maxRetries)
                        break;

                    TimeSpan delay = CalculateDelay(attempt);
                    _onRetry?.Invoke(ex, attempt + 1, delay);
                    Thread.Sleep(delay);
                }
            }

            throw new RetryException($"Operation failed after {_maxRetries + 1} attempts", lastException);
        }

        /// <summary>
        /// 异步执行操作
        /// </summary>
        public async Task ExecuteAsync(Func<Task> action)
        {
            await ExecuteAsync<object>(async () =>
            {
                await action();
                return null;
            });
        }

        /// <summary>
        /// 异步执行函数
        /// </summary>
        public async Task<T> ExecuteAsync<T>(Func<Task<T>> func)
        {
            Exception lastException = null;

            for (int attempt = 0; attempt <= _maxRetries; attempt++)
            {
                try
                {
                    return await func();
                }
                catch (Exception ex)
                {
                    lastException = ex;

                    if (!ShouldRetry(ex) || attempt >= _maxRetries)
                        break;

                    TimeSpan delay = CalculateDelay(attempt);
                    _onRetry?.Invoke(ex, attempt + 1, delay);
                    await Task.Delay(delay);
                }
            }

            throw new RetryException($"Operation failed after {_maxRetries + 1} attempts", lastException);
        }

        private bool ShouldRetry(Exception ex)
        {
            foreach (var type in _retryOnExceptions)
            {
                if (type.IsAssignableFrom(ex.GetType()))
                    return true;
            }
            return false;
        }

        private TimeSpan CalculateDelay(int attempt)
        {
            double delay = _initialDelay * Math.Pow(_backoffMultiplier, attempt);

            if (_useJitter)
            {
                // 添加随机抖动 (±20%)
                var random = new Random();
                double jitter = 0.8 + random.NextDouble() * 0.4;
                delay *= jitter;
            }

            return TimeSpan.FromMilliseconds(Math.Min(delay, _maxDelay));
        }
    }

    /// <summary>
    /// 重试异常
    /// </summary>
    public class RetryException : Exception
    {
        /// <summary>
        /// 创建重试异常
        /// </summary>
        public RetryException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
