using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace EasyTool.ToolCategory
{
    /// <summary>
    /// 重试工具类
    /// </summary>
    public static class RetryUtil
    {
        /// <summary>
        /// 判断异常是否为可重试的异常
        /// </summary>
        private static bool IsRetryableException(Exception ex)
        {
            return ex is IOException ||
                   ex is HttpRequestException ||
                   ex is TimeoutException ||
                   ex is SocketException ||
                   ex is OperationCanceledException;
        }

        /// <summary>
        /// 重试执行操作
        /// </summary>
        /// <param name="action">要执行的操作</param>
        /// <param name="maxRetries">最大重试次数</param>
        /// <param name="delay">重试间隔</param>
        /// <param name="onRetry">重试时的回调</param>
        /// <param name="shouldRetry">判断异常是否应该重试的函数，null时默认重试网络和IO相关的临时异常</param>
        /// <exception cref="ArgumentNullException">当 action 为 null 时抛出</exception>
        public static void Execute(
            Action action,
            int maxRetries = 3,
            TimeSpan? delay = null,
            Action<Exception, int>? onRetry = null,
            Func<Exception, bool>? shouldRetry = null)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            Exception? lastException = null;
            var delayValue = delay ?? TimeSpan.FromSeconds(1);

            for (int i = 0; i <= maxRetries; i++)
            {
                try
                {
                    action();
                    return;
                }
                // 捕获所有异常以支持重试（用户委托可能抛出任意异常）
                catch (Exception ex)
                {
                    // 判断是否应该重试此异常
                    bool canRetry = shouldRetry != null
                        ? shouldRetry(ex)
                        : IsRetryableException(ex);

                    if (!canRetry)
                    {
                        // 非可重试异常，直接抛出
                        throw;
                    }

                    lastException = ex;

                    if (i < maxRetries)
                    {
                        onRetry?.Invoke(ex, i + 1);

                        if (delayValue > TimeSpan.Zero)
                        {
                            Thread.Sleep(delayValue);
                        }
                    }
                }
            }

            throw lastException ?? new Exception("重试失败");
        }

        /// <summary>
        /// 重试执行操作（带返回值）
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <param name="func">要执行的函数</param>
        /// <param name="maxRetries">最大重试次数</param>
        /// <param name="delay">重试间隔</param>
        /// <param name="onRetry">重试时的回调</param>
        /// <param name="shouldRetry">判断异常是否应该重试的函数</param>
        /// <returns>函数的返回值</returns>
        /// <exception cref="ArgumentNullException">当 func 为 null 时抛出</exception>
        public static T Execute<T>(
            Func<T> func,
            int maxRetries = 3,
            TimeSpan? delay = null,
            Action<Exception, int>? onRetry = null,
            Func<Exception, bool>? shouldRetry = null)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            Exception? lastException = null;
            var delayValue = delay ?? TimeSpan.FromSeconds(1);

            for (int i = 0; i <= maxRetries; i++)
            {
                try
                {
                    return func();
                }
                // 捕获所有异常以支持重试（用户委托可能抛出任意异常）
                catch (Exception ex)
                {
                    // 判断是否应该重试此异常
                    bool canRetry = shouldRetry != null
                        ? shouldRetry(ex)
                        : IsRetryableException(ex);

                    if (!canRetry)
                    {
                        // 非可重试异常，直接抛出
                        throw;
                    }

                    lastException = ex;

                    if (i < maxRetries)
                    {
                        onRetry?.Invoke(ex, i + 1);

                        if (delayValue > TimeSpan.Zero)
                        {
                            Thread.Sleep(delayValue);
                        }
                    }
                }
            }

            throw lastException ?? new Exception("重试失败");
        }

        /// <summary>
        /// 异步重试执行
        /// </summary>
        /// <param name="action">要执行的异步操作</param>
        /// <param name="maxRetries">最大重试次数</param>
        /// <param name="delay">重试间隔</param>
        /// <param name="onRetry">重试时的异步回调</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <param name="shouldRetry">判断异常是否应该重试的函数</param>
        /// <returns>表示异步操作的 Task</returns>
        /// <exception cref="ArgumentNullException">当 action 为 null 时抛出</exception>
        /// <exception cref="OperationCanceledException">当操作被取消时抛出</exception>
        public static async Task ExecuteAsync(
            Func<Task> action,
            int maxRetries = 3,
            TimeSpan? delay = null,
            Func<Exception, int, Task>? onRetry = null,
            CancellationToken cancellationToken = default,
            Func<Exception, bool>? shouldRetry = null)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            Exception? lastException = null;
            var delayValue = delay ?? TimeSpan.FromSeconds(1);

            for (int i = 0; i <= maxRetries; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    await action().ConfigureAwait(false);
                    return;
                }
                // 捕获所有异常以支持重试（用户委托可能抛出任意异常）
                catch (Exception ex)
                {
                    // 判断是否应该重试此异常
                    bool canRetry = shouldRetry != null
                        ? shouldRetry(ex)
                        : IsRetryableException(ex);

                    if (!canRetry)
                    {
                        // 非可重试异常，直接抛出
                        throw;
                    }

                    lastException = ex;

                    if (i < maxRetries)
                    {
                        if (onRetry != null)
                            await onRetry(ex, i + 1).ConfigureAwait(false);

                        if (delayValue > TimeSpan.Zero)
                        {
                            await Task.Delay(delayValue, cancellationToken).ConfigureAwait(false);
                        }
                    }
                }
            }

            throw lastException ?? new Exception("重试失败");
        }

        /// <summary>
        /// 异步重试执行（带返回值）
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <param name="func">要执行的异步函数</param>
        /// <param name="maxRetries">最大重试次数</param>
        /// <param name="delay">重试间隔</param>
        /// <param name="onRetry">重试时的异步回调</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <param name="shouldRetry">判断异常是否应该重试的函数</param>
        /// <returns>函数返回值的 Task</returns>
        /// <exception cref="ArgumentNullException">当 func 为 null 时抛出</exception>
        /// <exception cref="OperationCanceledException">当操作被取消时抛出</exception>
        public static async Task<T> ExecuteAsync<T>(
            Func<Task<T>> func,
            int maxRetries = 3,
            TimeSpan? delay = null,
            Func<Exception, int, Task>? onRetry = null,
            CancellationToken cancellationToken = default,
            Func<Exception, bool>? shouldRetry = null)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            Exception? lastException = null;
            var delayValue = delay ?? TimeSpan.FromSeconds(1);

            for (int i = 0; i <= maxRetries; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    return await func().ConfigureAwait(false);
                }
                // 捕获所有异常以支持重试（用户委托可能抛出任意异常）
                catch (Exception ex)
                {
                    // 判断是否应该重试此异常
                    bool canRetry = shouldRetry != null
                        ? shouldRetry(ex)
                        : IsRetryableException(ex);

                    if (!canRetry)
                    {
                        // 非可重试异常，直接抛出
                        throw;
                    }

                    lastException = ex;

                    if (i < maxRetries)
                    {
                        if (onRetry != null)
                            await onRetry(ex, i + 1).ConfigureAwait(false);

                        if (delayValue > TimeSpan.Zero)
                        {
                            await Task.Delay(delayValue, cancellationToken).ConfigureAwait(false);
                        }
                    }
                }
            }

            throw lastException ?? new Exception("重试失败");
        }

        /// <summary>
        /// 指数退避重试
        /// </summary>
        /// <param name="action">要执行的异步操作</param>
        /// <param name="maxRetries">最大重试次数</param>
        /// <param name="initialDelay">初始延迟</param>
        /// <param name="multiplier">延迟倍数（指数增长因子）</param>
        /// <param name="maxDelay">最大延迟</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <param name="shouldRetry">判断异常是否应该重试的函数</param>
        /// <returns>表示异步操作的 Task</returns>
        /// <exception cref="ArgumentNullException">当 action 为 null 时抛出</exception>
        /// <exception cref="OperationCanceledException">当操作被取消时抛出</exception>
        public static async Task ExecuteWithBackoffAsync(
            Func<Task> action,
            int maxRetries = 5,
            TimeSpan? initialDelay = null,
            double multiplier = 2.0,
            TimeSpan? maxDelay = null,
            CancellationToken cancellationToken = default,
            Func<Exception, bool>? shouldRetry = null)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            var delay = initialDelay ?? TimeSpan.FromSeconds(1);
            var max = maxDelay ?? TimeSpan.FromMinutes(5);
            Exception? lastException = null;

            for (int i = 0; i <= maxRetries; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    await action().ConfigureAwait(false);
                    return;
                }
                // 捕获所有异常以支持重试（用户委托可能抛出任意异常）
                catch (Exception ex)
                {
                    // 判断是否应该重试此异常
                    bool canRetry = shouldRetry != null
                        ? shouldRetry(ex)
                        : IsRetryableException(ex);

                    if (!canRetry)
                    {
                        // 非可重试异常，直接抛出
                        throw;
                    }

                    lastException = ex;

                    if (i < maxRetries)
                    {
                        var currentDelay = delay * Math.Pow(multiplier, i);
                        currentDelay = TimeSpan.FromTicks(Math.Min(currentDelay.Ticks, max.Ticks));

                        await Task.Delay(currentDelay, cancellationToken).ConfigureAwait(false);
                    }
                }
            }

            throw lastException ?? new Exception("重试失败");
        }

        /// <summary>
        /// 带条件判断的重试
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <param name="func">要执行的函数</param>
        /// <param name="shouldRetry">判断结果是否需要重试的函数</param>
        /// <param name="maxRetries">最大重试次数</param>
        /// <param name="delay">重试间隔</param>
        /// <returns>函数的返回值</returns>
        /// <exception cref="ArgumentNullException">当 func 或 shouldRetry 为 null 时抛出</exception>
        public static T Execute<T>(
            Func<T> func,
            Func<T, bool> shouldRetry,
            int maxRetries = 3,
            TimeSpan? delay = null)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));
            if (shouldRetry == null)
                throw new ArgumentNullException(nameof(shouldRetry));

            var delayValue = delay ?? TimeSpan.FromSeconds(1);

            for (int i = 0; i <= maxRetries; i++)
            {
                var result = func();

                if (!shouldRetry(result))
                    return result;

                if (i < maxRetries && delayValue > TimeSpan.Zero)
                {
                    Thread.Sleep(delayValue);
                }
            }

            return func();
        }

        /// <summary>
        /// 使用重试策略执行
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <param name="func">要执行的异步函数</param>
        /// <param name="policy">重试策略</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>函数返回值的 Task</returns>
        /// <exception cref="ArgumentNullException">当 func 或 policy 为 null 时抛出</exception>
        /// <exception cref="OperationCanceledException">当操作被取消时抛出</exception>
        public static async Task<T> ExecuteAsync<T>(
            Func<Task<T>> func,
            RetryPolicy policy,
            CancellationToken cancellationToken = default)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));
            if (policy == null)
                throw new ArgumentNullException(nameof(policy));

            Exception? lastException = null;
            var delay = policy.InitialDelay;

            for (int i = 0; i <= policy.MaxRetries; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    return await func().ConfigureAwait(false);
                }
                // 捕获所有异常以支持重试策略（用户委托可能抛出任意异常）
                catch (Exception ex)
                {
                    if (!policy.ShouldRetry(ex))
                        throw;

                    lastException = ex;

                    if (i < policy.MaxRetries)
                    {
                        await Task.Delay(delay, cancellationToken).ConfigureAwait(false);

                        // 计算下次延迟
                        delay = policy.BackoffStrategy switch
                        {
                            BackoffStrategy.Linear => policy.InitialDelay,
                            BackoffStrategy.Exponential => TimeSpan.FromTicks(delay.Ticks * 2),
                            BackoffStrategy.Fixed => policy.InitialDelay,
                            _ => policy.InitialDelay
                        };

                        delay = TimeSpan.FromTicks(Math.Min(delay.Ticks, policy.MaxDelay.Ticks));
                    }
                }
            }

            throw lastException ?? new Exception("重试失败");
        }
    }

    /// <summary>
    /// 重试策略
    /// </summary>
    public class RetryPolicy
    {
        /// <summary>
        /// 最大重试次数
        /// </summary>
        public int MaxRetries { get; set; } = 3;

        /// <summary>
        /// 初始延迟
        /// </summary>
        public TimeSpan InitialDelay { get; set; } = TimeSpan.FromSeconds(1);

        /// <summary>
        /// 最大延迟
        /// </summary>
        public TimeSpan MaxDelay { get; set; } = TimeSpan.FromMinutes(1);

        /// <summary>
        /// 退避策略
        /// </summary>
        public BackoffStrategy BackoffStrategy { get; set; } = BackoffStrategy.Exponential;

        /// <summary>
        /// 判断是否应该重试
        /// </summary>
        public Func<Exception, bool>? ShouldRetry { get; set; }

        /// <summary>
        /// 创建默认策略
        /// </summary>
        public static RetryPolicy Default => new();

        /// <summary>
        /// 创建快速重试策略
        /// </summary>
        public static RetryPolicy Fast => new()
        {
            MaxRetries = 3,
            InitialDelay = TimeSpan.FromMilliseconds(100),
            MaxDelay = TimeSpan.FromSeconds(5),
            BackoffStrategy = BackoffStrategy.Linear
        };

        /// <summary>
        /// 创建网络重试策略
        /// </summary>
        public static RetryPolicy Network => new()
        {
            MaxRetries = 5,
            InitialDelay = TimeSpan.FromSeconds(1),
            MaxDelay = TimeSpan.FromSeconds(30),
            BackoffStrategy = BackoffStrategy.Exponential,
            ShouldRetry = ex => ex is TimeoutException ||
                               ex is System.Net.WebException ||
                               ex is System.Net.Http.HttpRequestException
        };
    }

    /// <summary>
    /// 退避策略
    /// </summary>
    public enum BackoffStrategy
    {
        /// <summary>
        /// 固定延迟
        /// </summary>
        Fixed,

        /// <summary>
        /// 线性递增
        /// </summary>
        Linear,

        /// <summary>
        /// 指数递增
        /// </summary>
        Exponential
    }
}
