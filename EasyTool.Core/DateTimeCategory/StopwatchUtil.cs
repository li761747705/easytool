using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace EasyTool.DateTimeCategory
{
    /// <summary>
    /// 计时器工具类
    /// 提供便捷的计时功能
    /// </summary>
    public static class StopwatchUtil
    {
        /// <summary>
        /// 创建并启动计时器
        /// </summary>
        /// <returns>计时器</returns>
        public static Stopwatch StartNew()
        {
            return Stopwatch.StartNew();
        }

        /// <summary>
        /// 测量操作执行时间
        /// </summary>
        /// <param name="action">操作</param>
        /// <returns>执行时间</returns>
        public static TimeSpan Measure(Action action)
        {
            var stopwatch = StartNew();
            action();
            stopwatch.Stop();
            return stopwatch.Elapsed;
        }

        /// <summary>
        /// 测量操作执行时间（带返回值）
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <param name="func">操作</param>
        /// <returns>执行时间和结果</returns>
        public static (TimeSpan Elapsed, T Result) Measure<T>(Func<T> func)
        {
            var stopwatch = StartNew();
            var result = func();
            stopwatch.Stop();
            return (stopwatch.Elapsed, result);
        }

        /// <summary>
        /// 异步测量操作执行时间
        /// </summary>
        /// <param name="action">操作</param>
        /// <returns>执行时间</returns>
        public static async Task<TimeSpan> MeasureAsync(Func<Task> action)
        {
            var stopwatch = StartNew();
            await action();
            stopwatch.Stop();
            return stopwatch.Elapsed;
        }

        /// <summary>
        /// 异步测量操作执行时间（带返回值）
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <param name="func">操作</param>
        /// <returns>执行时间和结果</returns>
        public static async Task<(TimeSpan Elapsed, T Result)> MeasureAsync<T>(Func<Task<T>> func)
        {
            var stopwatch = StartNew();
            var result = await func();
            stopwatch.Stop();
            return (stopwatch.Elapsed, result);
        }

        /// <summary>
        /// 使用计时器执行操作
        /// </summary>
        /// <param name="action">操作</param>
        /// <param name="callback">计时回调</param>
        public static void WithTimer(Action action, Action<TimeSpan> callback)
        {
            var elapsed = Measure(action);
            callback(elapsed);
        }

        /// <summary>
        /// 使用计时器执行操作
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <param name="func">操作</param>
        /// <param name="callback">计时回调</param>
        /// <returns>操作结果</returns>
        public static T WithTimer<T>(Func<T> func, Action<TimeSpan> callback)
        {
            var (elapsed, result) = Measure(func);
            callback(elapsed);
            return result;
        }

        /// <summary>
        /// 异步使用计时器执行操作
        /// </summary>
        /// <param name="action">操作</param>
        /// <param name="callback">计时回调</param>
        public static async Task WithTimerAsync(Func<Task> action, Action<TimeSpan> callback)
        {
            var elapsed = await MeasureAsync(action);
            callback(elapsed);
        }

        /// <summary>
        /// 异步使用计时器执行操作
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <param name="func">操作</param>
        /// <param name="callback">计时回调</param>
        /// <returns>操作结果</returns>
        public static async Task<T> WithTimerAsync<T>(Func<Task<T>> func, Action<TimeSpan> callback)
        {
            var (elapsed, result) = await MeasureAsync(func);
            callback(elapsed);
            return result;
        }

        /// <summary>
        /// 等待指定时间
        /// </summary>
        /// <param name="duration">等待时间</param>
        public static void Wait(TimeSpan duration)
        {
            Thread.Sleep(duration);
        }

        /// <summary>
        /// 异步等待指定时间
        /// </summary>
        /// <param name="duration">等待时间</param>
        /// <param name="cancellationToken">取消令牌</param>
        public static Task WaitAsync(TimeSpan duration, CancellationToken cancellationToken = default)
        {
            return Task.Delay(duration, cancellationToken);
        }

        /// <summary>
        /// 执行带超时的操作
        /// </summary>
        /// <param name="action">操作</param>
        /// <param name="timeout">超时时间</param>
        /// <returns>是否在超时前完成</returns>
        public static bool TryExecute(Action action, TimeSpan timeout)
        {
            var task = Task.Run(action);
            return task.Wait(timeout);
        }

        /// <summary>
        /// 异步执行带超时的操作
        /// </summary>
        /// <param name="action">操作</param>
        /// <param name="timeout">超时时间</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>是否在超时前完成</returns>
        public static async Task<bool> TryExecuteAsync(Func<Task> action, TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(timeout);

            try
            {
                await action();
                return true;
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                return false;
            }
        }

        /// <summary>
        /// 执行带超时的操作（带返回值）
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <param name="func">操作</param>
        /// <param name="timeout">超时时间</param>
        /// <param name="result">结果</param>
        /// <returns>是否在超时前完成</returns>
        public static bool TryExecute<T>(Func<T> func, TimeSpan timeout, out T? result)
        {
            result = default;
            var task = Task.Run(func);

            if (task.Wait(timeout))
            {
                result = task.Result;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 异步执行带超时的操作（带返回值）
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <param name="func">操作</param>
        /// <param name="timeout">超时时间</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>结果或默认值</returns>
        public static async Task<(bool Success, T? Result)> TryExecuteAsync<T>(Func<Task<T>> func, TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(timeout);

            try
            {
                var result = await func();
                return (true, result);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                return (false, default);
            }
        }

        /// <summary>
        /// 格式化时间输出
        /// </summary>
        /// <param name="time">时间</param>
        /// <returns>格式化字符串</returns>
        public static string FormatTime(TimeSpan time)
        {
            if (time.TotalSeconds >= 1)
                return $"{time.TotalSeconds:F2}s";
            if (time.TotalMilliseconds >= 1)
                return $"{time.TotalMilliseconds:F2}ms";
#if NET7_0_OR_GREATER
            if (time.TotalMicroseconds >= 1)
                return $"{time.TotalMicroseconds:F2}μs";
            return $"{time.TotalNanoseconds:F2}ns";
#else
            // For older frameworks, use ticks for sub-millisecond precision
            var ticks = time.Ticks;
            if (ticks >= 10) // >= 1 microsecond (10 ticks = 1 μs)
                return $"{ticks / 10.0:F2}μs";
            return $"{ticks * 100.0:F2}ns";
#endif
        }

        /// <summary>
        /// 格式化时间为详细字符串
        /// </summary>
        /// <param name="time">时间</param>
        /// <returns>格式化字符串</returns>
        public static string FormatTimeDetailed(TimeSpan time)
        {
            var parts = new List<string>();

            if (time.Days > 0)
                parts.Add($"{time.Days}天");
            if (time.Hours > 0)
                parts.Add($"{time.Hours}小时");
            if (time.Minutes > 0)
                parts.Add($"{time.Minutes}分钟");
            if (time.Seconds > 0)
                parts.Add($"{time.Seconds}秒");
            if (time.Milliseconds > 0)
                parts.Add($"{time.Milliseconds}毫秒");

            return parts.Count > 0 ? string.Join(" ", parts) : "0毫秒";
        }
    }
}
