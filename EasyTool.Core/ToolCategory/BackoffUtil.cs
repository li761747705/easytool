using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EasyTool.ToolCategory
{
    /// <summary>
    /// 退避策略工具类
    /// 提供指数退避、线性退避等重试间隔计算
    /// </summary>
    public static class BackoffUtil
    {
        /// <summary>
        /// 指数退避
        /// </summary>
        /// <param name="attempt">尝试次数（从0开始）</param>
        /// <param name="baseDelay">基础延迟</param>
        /// <param name="maxDelay">最大延迟</param>
        /// <param name="jitter">是否添加随机抖动</param>
        public static TimeSpan Exponential(int attempt, TimeSpan baseDelay, TimeSpan? maxDelay = null, bool jitter = true)
        {
            var delay = TimeSpan.FromTicks(baseDelay.Ticks * (long)Math.Pow(2, attempt));
            
            if (maxDelay.HasValue && delay > maxDelay.Value)
                delay = maxDelay.Value;

            if (jitter)
            {
                var random = new Random();
                var jitterRange = delay.TotalMilliseconds * 0.1;
                delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds + random.NextDouble() * jitterRange);
            }

            return delay;
        }

        /// <summary>
        /// 线性退避
        /// </summary>
        public static TimeSpan Linear(int attempt, TimeSpan baseDelay, TimeSpan? maxDelay = null)
        {
            var delay = TimeSpan.FromTicks(baseDelay.Ticks * (attempt + 1));
            
            if (maxDelay.HasValue && delay > maxDelay.Value)
                delay = maxDelay.Value;

            return delay;
        }

        /// <summary>
        /// 固定延迟
        /// </summary>
        public static TimeSpan Fixed(TimeSpan delay)
        {
            return delay;
        }

        /// <summary>
        /// 装饰退避（Decorrelated Jitter）
        /// </summary>
        public static TimeSpan DecorrelatedJitter(int attempt, TimeSpan baseDelay, TimeSpan maxDelay, TimeSpan? previousDelay = null)
        {
            var random = new Random();
            var prev = previousDelay ?? baseDelay;
            var delay = TimeSpan.FromTicks((long)(prev.TotalMilliseconds * random.NextDouble() * 3));
            
            if (delay < baseDelay)
                delay = baseDelay;
            
            if (delay > maxDelay)
                delay = maxDelay;

            return delay;
        }

        /// <summary>
        /// 等距退避
        /// </summary>
        public static TimeSpan EqualJitter(int attempt, TimeSpan baseDelay, TimeSpan maxDelay)
        {
            var random = new Random();
            var exponentialDelay = Exponential(attempt, baseDelay, maxDelay, false);
            var half = exponentialDelay.TotalMilliseconds / 2;
            var delay = TimeSpan.FromMilliseconds(half + random.NextDouble() * half);
            return delay;
        }

        /// <summary>
        /// 创建退避策略生成器
        /// </summary>
        public static BackoffGenerator CreateGenerator(BackoffStrategy strategy, TimeSpan baseDelay, TimeSpan? maxDelay = null, bool jitter = true)
        {
            return new BackoffGenerator(strategy, baseDelay, maxDelay, jitter);
        }

        /// <summary>
        /// 使用退避策略执行操作
        /// </summary>
        public static async Task<T> ExecuteWithBackoffAsync<T>(
            Func<Task<T>> action,
            int maxRetries,
            BackoffStrategy strategy = BackoffStrategy.Exponential,
            TimeSpan? baseDelay = null,
            TimeSpan? maxDelay = null,
            Func<Exception, int, bool>? shouldRetry = null)
        {
            var delay = baseDelay ?? TimeSpan.FromSeconds(1);
            var max = maxDelay ?? TimeSpan.FromMinutes(1);
            Exception? lastException = null;

            for (int attempt = 0; attempt <= maxRetries; attempt++)
            {
                try
                {
                    return await action();
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    
                    if (attempt == maxRetries || (shouldRetry != null && !shouldRetry(ex, attempt)))
                        break;

                    var waitTime = strategy switch
                    {
                        BackoffStrategy.Exponential => Exponential(attempt, delay, max),
                        BackoffStrategy.Linear => Linear(attempt, delay, max),
                        BackoffStrategy.Fixed => delay,
                        _ => Exponential(attempt, delay, max)
                    };

                    await Task.Delay(waitTime);
                }
            }

            throw lastException ?? new Exception("操作失败");
        }

        /// <summary>
        /// 使用退避策略执行操作
        /// </summary>
        public static async Task ExecuteWithBackoffAsync(
            Func<Task> action,
            int maxRetries,
            BackoffStrategy strategy = BackoffStrategy.Exponential,
            TimeSpan? baseDelay = null,
            TimeSpan? maxDelay = null,
            Func<Exception, int, bool>? shouldRetry = null)
        {
            await ExecuteWithBackoffAsync(async () =>
            {
                await action();
                return true;
            }, maxRetries, strategy, baseDelay, maxDelay, shouldRetry);
        }
    }

    /// <summary>
    /// 退避策略生成器
    /// </summary>
    public class BackoffGenerator
    {
        private readonly BackoffStrategy _strategy;
        private readonly TimeSpan _baseDelay;
        private readonly TimeSpan? _maxDelay;
        private readonly bool _jitter;
        private int _attempt;
        private TimeSpan? _previousDelay;

        public BackoffGenerator(BackoffStrategy strategy, TimeSpan baseDelay, TimeSpan? maxDelay = null, bool jitter = true)
        {
            _strategy = strategy;
            _baseDelay = baseDelay;
            _maxDelay = maxDelay;
            _jitter = jitter;
            _attempt = 0;
        }

        /// <summary>
        /// 获取下一个延迟时间
        /// </summary>
        public TimeSpan Next()
        {
            var delay = _strategy switch
            {
                BackoffStrategy.Exponential => BackoffUtil.Exponential(_attempt, _baseDelay, _maxDelay, _jitter),
                BackoffStrategy.Linear => BackoffUtil.Linear(_attempt, _baseDelay, _maxDelay),
                BackoffStrategy.Fixed => _baseDelay,
                _ => _baseDelay
            };

            _previousDelay = delay;
            _attempt++;
            return delay;
        }

        /// <summary>
        /// 重置生成器
        /// </summary>
        public void Reset()
        {
            _attempt = 0;
            _previousDelay = null;
        }

        /// <summary>
        /// 获取当前尝试次数
        /// </summary>
        public int Attempt => _attempt;
    }
}