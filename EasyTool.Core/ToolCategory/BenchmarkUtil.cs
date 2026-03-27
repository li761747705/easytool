using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace EasyTool.ToolCategory
{
    /// <summary>
    /// 性能测试结果
    /// </summary>
    public class BenchmarkResult
    {
        /// <summary>
        /// 操作名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 执行次数
        /// </summary>
        public int Iterations { get; set; }

        /// <summary>
        /// 总耗时
        /// </summary>
        public TimeSpan TotalTime { get; set; }

        /// <summary>
        /// 平均耗时
        /// </summary>
        public TimeSpan AverageTime => Iterations > 0 ? TimeSpan.FromTicks(TotalTime.Ticks / Iterations) : TimeSpan.Zero;

        /// <summary>
        /// 最小耗时
        /// </summary>
        public TimeSpan MinTime { get; set; }

        /// <summary>
        /// 最大耗时
        /// </summary>
        public TimeSpan MaxTime { get; set; }

        /// <summary>
        /// 每秒操作数
        /// </summary>
        public double OperationsPerSecond => TotalTime.TotalSeconds > 0 ? Iterations / TotalTime.TotalSeconds : 0;

        /// <summary>
        /// 详细耗时记录
        /// </summary>
        public List<TimeSpan> DetailedTimes { get; set; } = new();

        public override string ToString()
        {
            return $"[{Name}] {Iterations} 次, 总计: {TotalTime.TotalMilliseconds:F2}ms, 平均: {AverageTime.TotalMilliseconds:F4}ms, " +
                   $"最小: {MinTime.TotalMilliseconds:F4}ms, 最大: {MaxTime.TotalMilliseconds:F4}ms, {OperationsPerSecond:F0} ops/s";
        }
    }

    /// <summary>
    /// 性能测试工具类
    /// 提供代码执行性能测量功能
    /// </summary>
    public static class BenchmarkUtil
    {
        /// <summary>
        /// 测量单次执行时间
        /// </summary>
        /// <param name="action">要测量的操作</param>
        /// <returns>执行时间</returns>
        public static TimeSpan Measure(Action action)
        {
            var stopwatch = Stopwatch.StartNew();
            action();
            stopwatch.Stop();
            return stopwatch.Elapsed;
        }

        /// <summary>
        /// 测量单次执行时间（带返回值）
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <param name="func">要测量的操作</param>
        /// <param name="result">执行结果</param>
        /// <returns>执行时间</returns>
        public static TimeSpan Measure<T>(Func<T> func, out T result)
        {
            var stopwatch = Stopwatch.StartNew();
            result = func();
            stopwatch.Stop();
            return stopwatch.Elapsed;
        }

        /// <summary>
        /// 异步测量单次执行时间
        /// </summary>
        /// <param name="action">要测量的操作</param>
        /// <returns>执行时间</returns>
        public static async Task<TimeSpan> MeasureAsync(Func<Task> action)
        {
            var stopwatch = Stopwatch.StartNew();
            await action();
            stopwatch.Stop();
            return stopwatch.Elapsed;
        }

        /// <summary>
        /// 异步测量单次执行时间（带返回值）
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <param name="func">要测量的操作</param>
        /// <returns>执行时间和结果</returns>
        public static async Task<(TimeSpan Elapsed, T Result)> MeasureAsync<T>(Func<Task<T>> func)
        {
            var stopwatch = Stopwatch.StartNew();
            var result = await func();
            stopwatch.Stop();
            return (stopwatch.Elapsed, result);
        }

        /// <summary>
        /// 基准测试
        /// </summary>
        /// <param name="name">测试名称</param>
        /// <param name="action">要测试的操作</param>
        /// <param name="iterations">迭代次数</param>
        /// <param name="warmupIterations">预热次数</param>
        /// <returns>测试结果</returns>
        public static BenchmarkResult Run(string name, Action action, int iterations = 1000, int warmupIterations = 10)
        {
            // 预热
            for (int i = 0; i < warmupIterations; i++)
            {
                action();
            }

            // 正式测试
            var times = new List<TimeSpan>(iterations);
            var totalTime = TimeSpan.Zero;
            var minTime = TimeSpan.MaxValue;
            var maxTime = TimeSpan.Zero;

            for (int i = 0; i < iterations; i++)
            {
                var time = Measure(action);
                times.Add(time);
                totalTime += time;

                if (time < minTime) minTime = time;
                if (time > maxTime) maxTime = time;
            }

            return new BenchmarkResult
            {
                Name = name,
                Iterations = iterations,
                TotalTime = totalTime,
                MinTime = minTime,
                MaxTime = maxTime,
                DetailedTimes = times
            };
        }

        /// <summary>
        /// 异步基准测试
        /// </summary>
        /// <param name="name">测试名称</param>
        /// <param name="action">要测试的操作</param>
        /// <param name="iterations">迭代次数</param>
        /// <param name="warmupIterations">预热次数</param>
        /// <returns>测试结果</returns>
        public static async Task<BenchmarkResult> RunAsync(string name, Func<Task> action, int iterations = 1000, int warmupIterations = 10)
        {
            // 预热
            for (int i = 0; i < warmupIterations; i++)
            {
                await action();
            }

            // 正式测试
            var times = new List<TimeSpan>(iterations);
            var totalTime = TimeSpan.Zero;
            var minTime = TimeSpan.MaxValue;
            var maxTime = TimeSpan.Zero;

            for (int i = 0; i < iterations; i++)
            {
                var time = await MeasureAsync(action);
                times.Add(time);
                totalTime += time;

                if (time < minTime) minTime = time;
                if (time > maxTime) maxTime = time;
            }

            return new BenchmarkResult
            {
                Name = name,
                Iterations = iterations,
                TotalTime = totalTime,
                MinTime = minTime,
                MaxTime = maxTime,
                DetailedTimes = times
            };
        }

        /// <summary>
        /// 比较多个操作的性能
        /// </summary>
        /// <param name="iterations">迭代次数</param>
        /// <param name="actions">操作列表</param>
        /// <returns>测试结果列表</returns>
        public static List<BenchmarkResult> Compare(int iterations, params (string Name, Action Action)[] actions)
        {
            var results = new List<BenchmarkResult>();

            foreach (var (name, action) in actions)
            {
                results.Add(Run(name, action, iterations));
            }

            return results.OrderBy(r => r.AverageTime).ToList();
        }

        /// <summary>
        /// 异步比较多个操作的性能
        /// </summary>
        /// <param name="iterations">迭代次数</param>
        /// <param name="actions">操作列表</param>
        /// <returns>测试结果列表</returns>
        public static async Task<List<BenchmarkResult>> CompareAsync(int iterations, params (string Name, Func<Task> Action)[] actions)
        {
            var results = new List<BenchmarkResult>();

            foreach (var (name, action) in actions)
            {
                results.Add(await RunAsync(name, action, iterations));
            }

            return results.OrderBy(r => r.AverageTime).ToList();
        }

        /// <summary>
        /// 使用计时器测量操作
        /// </summary>
        /// <param name="action">要测量的操作</param>
        /// <param name="elapsed">耗时回调</param>
        public static void WithTimer(Action action, Action<TimeSpan> elapsed)
        {
            var time = Measure(action);
            elapsed(time);
        }

        /// <summary>
        /// 异步使用计时器测量操作
        /// </summary>
        /// <param name="action">要测量的操作</param>
        /// <param name="elapsed">耗时回调</param>
        public static async Task WithTimerAsync(Func<Task> action, Action<TimeSpan> elapsed)
        {
            var time = await MeasureAsync(action);
            elapsed(time);
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
        /// 打印比较结果
        /// </summary>
        /// <param name="results">测试结果列表</param>
        public static void PrintComparison(List<BenchmarkResult> results)
        {
            if (results.Count == 0)
                return;

            Console.WriteLine("=== 性能比较结果 ===");
            Console.WriteLine();

            var baseline = results[0].AverageTime;

            for (int i = 0; i < results.Count; i++)
            {
                var result = results[i];
                var ratio = i == 0 ? 1.0 : result.AverageTime.TotalMilliseconds / baseline.TotalMilliseconds;

                Console.WriteLine($"{i + 1}. {result.Name}");
                Console.WriteLine($"   平均: {FormatTime(result.AverageTime)}");
                Console.WriteLine($"   比率: {ratio:F2}x");
                Console.WriteLine($"   吞吐: {result.OperationsPerSecond:F0} ops/s");
                Console.WriteLine();
            }
        }
    }
}