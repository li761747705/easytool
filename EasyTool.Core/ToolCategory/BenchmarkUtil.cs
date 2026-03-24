using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EasyTool.ToolCategory
{
    /// <summary>
    /// 性能计时工具类
    /// 提供代码执行时间测量和性能分析功能
    /// </summary>
    public static class BenchmarkUtil
    {
        /// <summary>
        /// 测量操作执行时间
        /// </summary>
        /// <param name="action">要测量的操作</param>
        /// <param name="name">操作名称</param>
        /// <returns>测量结果</returns>
        public static BenchmarkResult Measure(Action action, string? name = null)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            var stopwatch = Stopwatch.StartNew();
            action();
            stopwatch.Stop();

            return new BenchmarkResult
            {
                Name = name ?? "Operation",
                ElapsedTicks = stopwatch.ElapsedTicks,
                ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
                ElapsedTime = stopwatch.Elapsed
            };
        }

        /// <summary>
        /// 测量异步操作执行时间
        /// </summary>
        /// <param name="func">要测量的异步操作</param>
        /// <param name="name">操作名称</param>
        /// <returns>测量结果</returns>
        public static async Task<BenchmarkResult> MeasureAsync(Func<Task> func, string? name = null)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            var stopwatch = Stopwatch.StartNew();
            await func();
            stopwatch.Stop();

            return new BenchmarkResult
            {
                Name = name ?? "Operation",
                ElapsedTicks = stopwatch.ElapsedTicks,
                ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
                ElapsedTime = stopwatch.Elapsed
            };
        }

        /// <summary>
        /// 测量带返回值的操作执行时间
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <param name="func">要测量的操作</param>
        /// <param name="name">操作名称</param>
        /// <returns>带返回值的测量结果</returns>
        public static BenchmarkResult<T> Measure<T>(Func<T> func, string? name = null)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            var stopwatch = Stopwatch.StartNew();
            var result = func();
            stopwatch.Stop();

            return new BenchmarkResult<T>
            {
                Name = name ?? "Operation",
                ElapsedTicks = stopwatch.ElapsedTicks,
                ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
                ElapsedTime = stopwatch.Elapsed,
                Value = result
            };
        }

        /// <summary>
        /// 测量带返回值的异步操作执行时间
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <param name="func">要测量的异步操作</param>
        /// <param name="name">操作名称</param>
        /// <returns>带返回值的测量结果</returns>
        public static async Task<BenchmarkResult<T>> MeasureAsync<T>(Func<Task<T>> func, string? name = null)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            var stopwatch = Stopwatch.StartNew();
            var result = await func();
            stopwatch.Stop();

            return new BenchmarkResult<T>
            {
                Name = name ?? "Operation",
                ElapsedTicks = stopwatch.ElapsedTicks,
                ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
                ElapsedTime = stopwatch.Elapsed,
                Value = result
            };
        }

        /// <summary>
        /// 多次执行并计算平均执行时间
        /// </summary>
        /// <param name="action">要测量的操作</param>
        /// <param name="iterations">迭代次数</param>
        /// <param name="name">操作名称</param>
        /// <returns>统计结果</returns>
        public static BenchmarkStatistics Benchmark(Action action, int iterations = 100, string? name = null)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            if (iterations < 1)
                throw new ArgumentOutOfRangeException(nameof(iterations));

            // 预热
            action();

            var times = new List<long>(iterations);
            var stopwatch = new Stopwatch();

            for (int i = 0; i < iterations; i++)
            {
                stopwatch.Restart();
                action();
                stopwatch.Stop();
                times.Add(stopwatch.ElapsedTicks);
            }

            return CalculateStatistics(name ?? "Operation", times, iterations);
        }

        /// <summary>
        /// 多次执行异步操作并计算平均执行时间
        /// </summary>
        /// <param name="func">要测量的异步操作</param>
        /// <param name="iterations">迭代次数</param>
        /// <param name="name">操作名称</param>
        /// <returns>统计结果</returns>
        public static async Task<BenchmarkStatistics> BenchmarkAsync(Func<Task> func, int iterations = 100, string? name = null)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));
            if (iterations < 1)
                throw new ArgumentOutOfRangeException(nameof(iterations));

            // 预热
            await func();

            var times = new List<long>(iterations);
            var stopwatch = new Stopwatch();

            for (int i = 0; i < iterations; i++)
            {
                stopwatch.Restart();
                await func();
                stopwatch.Stop();
                times.Add(stopwatch.ElapsedTicks);
            }

            return CalculateStatistics(name ?? "Operation", times, iterations);
        }

        /// <summary>
        /// 比较多个操作的执行时间
        /// </summary>
        /// <param name="operations">操作列表（名称和操作）</param>
        /// <param name="iterations">每个操作的迭代次数</param>
        /// <returns>比较结果列表</returns>
        public static List<BenchmarkStatistics> Compare(IEnumerable<(string Name, Action Action)> operations, int iterations = 100)
        {
            if (operations == null)
                throw new ArgumentNullException(nameof(operations));

            var results = new List<BenchmarkStatistics>();
            foreach (var (name, action) in operations)
            {
                results.Add(Benchmark(action, iterations, name));
            }

            return results.OrderBy(r => r.AverageMilliseconds).ToList();
        }

        /// <summary>
        /// 比较多个异步操作的执行时间
        /// </summary>
        /// <param name="operations">操作列表（名称和操作）</param>
        /// <param name="iterations">每个操作的迭代次数</param>
        /// <returns>比较结果列表</returns>
        public static async Task<List<BenchmarkStatistics>> CompareAsync(
            IEnumerable<(string Name, Func<Task> Func)> operations,
            int iterations = 100)
        {
            if (operations == null)
                throw new ArgumentNullException(nameof(operations));

            var results = new List<BenchmarkStatistics>();
            foreach (var (name, func) in operations)
            {
                results.Add(await BenchmarkAsync(func, iterations, name));
            }

            return results.OrderBy(r => r.AverageMilliseconds).ToList();
        }

        /// <summary>
        /// 创建一个可多次记录的计时器
        /// </summary>
        /// <param name="name">计时器名称</param>
        /// <returns>计时器实例</returns>
        public static BenchmarkTimer CreateTimer(string? name = null)
        {
            return new BenchmarkTimer(name);
        }

        /// <summary>
        /// 内存使用测量
        /// </summary>
        /// <param name="action">要测量的操作</param>
        /// <param name="name">操作名称</param>
        /// <returns>测量结果</returns>
        public static MemoryBenchmarkResult MeasureMemory(Action action, string? name = null)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            // 强制GC以获得更准确的内存测量
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            var beforeMemory = GC.GetTotalMemory(forceFullCollection: true);

            var stopwatch = Stopwatch.StartNew();
            action();
            stopwatch.Stop();

            var afterMemory = GC.GetTotalMemory(forceFullCollection: false);

            return new MemoryBenchmarkResult
            {
                Name = name ?? "Operation",
                ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
                MemoryBefore = beforeMemory,
                MemoryAfter = afterMemory,
                MemoryDelta = afterMemory - beforeMemory
            };
        }

        private static BenchmarkStatistics CalculateStatistics(string name, List<long> times, int iterations)
        {
            times.Sort();
            var frequency = Stopwatch.Frequency;

            var avgTicks = times.Average();
            var minTicks = times[0];
            var maxTicks = times[times.Count - 1];
            var medianTicks = times[times.Count / 2];
            var stdDev = Math.Sqrt(times.Average(t => Math.Pow(t - avgTicks, 2)));

            var p95Index = (int)(iterations * 0.95);
            var p99Index = (int)(iterations * 0.99);

            return new BenchmarkStatistics
            {
                Name = name,
                Iterations = iterations,
                TotalMilliseconds = (long)times.Sum(t => t * 1000.0 / frequency),
                AverageMilliseconds = avgTicks * 1000.0 / frequency,
                MinMilliseconds = minTicks * 1000.0 / frequency,
                MaxMilliseconds = maxTicks * 1000.0 / frequency,
                MedianMilliseconds = medianTicks * 1000.0 / frequency,
                StdDevMilliseconds = stdDev * 1000.0 / frequency,
                P95Milliseconds = times[Math.Min(p95Index, times.Count - 1)] * 1000.0 / frequency,
                P99Milliseconds = times[Math.Min(p99Index, times.Count - 1)] * 1000.0 / frequency,
                OperationsPerSecond = 1000.0 / (avgTicks * 1000.0 / frequency)
            };
        }
    }

    /// <summary>
    /// 基准测试结果
    /// </summary>
    public class BenchmarkResult
    {
        /// <summary>
        /// 操作名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 执行时间（Tick数）
        /// </summary>
        public long ElapsedTicks { get; set; }

        /// <summary>
        /// 执行时间（毫秒）
        /// </summary>
        public long ElapsedMilliseconds { get; set; }

        /// <summary>
        /// 执行时间
        /// </summary>
        public TimeSpan ElapsedTime { get; set; }

        /// <summary>
        /// 执行时间（微秒）
        /// </summary>
        public double ElapsedMicroseconds => ElapsedTicks * 1_000_000.0 / Stopwatch.Frequency;

        /// <summary>
        /// 执行时间（纳秒）
        /// </summary>
        public double ElapsedNanoseconds => ElapsedTicks * 1_000_000_000.0 / Stopwatch.Frequency;

        public override string ToString()
        {
            if (ElapsedMilliseconds > 0)
                return $"{Name}: {ElapsedMilliseconds}ms";
            return $"{Name}: {ElapsedMicroseconds:F2}μs";
        }
    }

    /// <summary>
    /// 带返回值的基准测试结果
    /// </summary>
    /// <typeparam name="T">返回值类型</typeparam>
    public class BenchmarkResult<T> : BenchmarkResult
    {
        /// <summary>
        /// 返回值
        /// </summary>
        public T? Value { get; set; }
    }

    /// <summary>
    /// 基准测试统计信息
    /// </summary>
    public class BenchmarkStatistics
    {
        /// <summary>
        /// 操作名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 迭代次数
        /// </summary>
        public int Iterations { get; set; }

        /// <summary>
        /// 总执行时间（毫秒）
        /// </summary>
        public long TotalMilliseconds { get; set; }

        /// <summary>
        /// 平均执行时间（毫秒）
        /// </summary>
        public double AverageMilliseconds { get; set; }

        /// <summary>
        /// 最小执行时间（毫秒）
        /// </summary>
        public double MinMilliseconds { get; set; }

        /// <summary>
        /// 最大执行时间（毫秒）
        /// </summary>
        public double MaxMilliseconds { get; set; }

        /// <summary>
        /// 中位数执行时间（毫秒）
        /// </summary>
        public double MedianMilliseconds { get; set; }

        /// <summary>
        /// 标准差（毫秒）
        /// </summary>
        public double StdDevMilliseconds { get; set; }

        /// <summary>
        /// 第95百分位执行时间（毫秒）
        /// </summary>
        public double P95Milliseconds { get; set; }

        /// <summary>
        /// 第99百分位执行时间（毫秒）
        /// </summary>
        public double P99Milliseconds { get; set; }

        /// <summary>
        /// 每秒操作数
        /// </summary>
        public double OperationsPerSecond { get; set; }

        public override string ToString()
        {
            return $"{Name}: Avg={AverageMilliseconds:F3}ms, Min={MinMilliseconds:F3}ms, Max={MaxMilliseconds:F3}ms, Ops/s={OperationsPerSecond:F0}";
        }
    }

    /// <summary>
    /// 内存基准测试结果
    /// </summary>
    public class MemoryBenchmarkResult
    {
        /// <summary>
        /// 操作名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 执行时间（毫秒）
        /// </summary>
        public long ElapsedMilliseconds { get; set; }

        /// <summary>
        /// 执行前内存（字节）
        /// </summary>
        public long MemoryBefore { get; set; }

        /// <summary>
        /// 执行后内存（字节）
        /// </summary>
        public long MemoryAfter { get; set; }

        /// <summary>
        /// 内存变化（字节）
        /// </summary>
        public long MemoryDelta { get; set; }

        /// <summary>
        /// 内存变化（MB）
        /// </summary>
        public double MemoryDeltaMB => MemoryDelta / (1024.0 * 1024.0);

        public override string ToString()
        {
            var sign = MemoryDelta >= 0 ? "+" : "";
            return $"{Name}: {ElapsedMilliseconds}ms, Memory: {sign}{MemoryDeltaMB:F2}MB";
        }
    }

    /// <summary>
    /// 可记录多个时间点的计时器
    /// </summary>
    public class BenchmarkTimer
    {
        private readonly Stopwatch _stopwatch;
        private readonly List<(string Label, long Ticks)> _laps;
        private readonly string? _name;

        public BenchmarkTimer(string? name = null)
        {
            _name = name;
            _stopwatch = new Stopwatch();
            _laps = new List<(string, long)>();
        }

        /// <summary>
        /// 开始计时
        /// </summary>
        public BenchmarkTimer Start()
        {
            _stopwatch.Start();
            return this;
        }

        /// <summary>
        /// 记录一个时间点
        /// </summary>
        /// <param name="label">标签</param>
        public BenchmarkTimer Lap(string label)
        {
            _laps.Add((label, _stopwatch.ElapsedTicks));
            return this;
        }

        /// <summary>
        /// 停止计时
        /// </summary>
        public BenchmarkTimer Stop()
        {
            _stopwatch.Stop();
            return this;
        }

        /// <summary>
        /// 重置计时器
        /// </summary>
        public BenchmarkTimer Reset()
        {
            _stopwatch.Reset();
            _laps.Clear();
            return this;
        }

        /// <summary>
        /// 获取所有记录点
        /// </summary>
        /// <returns>记录点列表</returns>
        public List<(string Label, double Milliseconds)> GetLaps()
        {
            return _laps.Select(l => (l.Label, l.Ticks * 1000.0 / Stopwatch.Frequency)).ToList();
        }

        /// <summary>
        /// 获取总执行时间（毫秒）
        /// </summary>
        public double TotalMilliseconds => _stopwatch.ElapsedTicks * 1000.0 / Stopwatch.Frequency;

        /// <summary>
        /// 获取执行时间
        /// </summary>
        public TimeSpan Elapsed => _stopwatch.Elapsed;

        /// <summary>
        /// 是否正在运行
        /// </summary>
        public bool IsRunning => _stopwatch.IsRunning;

        public override string ToString()
        {
            return _name != null
                ? $"{_name}: {TotalMilliseconds:F2}ms"
                : $"{TotalMilliseconds:F2}ms";
        }
    }
}
