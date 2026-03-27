using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EasyTool.DateTimeCategory
{
    /// <summary>
    /// 定时器工具类
    /// 提供增强的定时器功能
    /// </summary>
    public static class TimerUtil
    {
        /// <summary>
        /// 创建一次性定时器
        /// </summary>
        /// <param name="delay">延迟时间</param>
        /// <param name="callback">回调</param>
        /// <returns>定时器</returns>
        public static Timer Once(TimeSpan delay, Action callback)
        {
            return new Timer(_ => callback(), null, delay, Timeout.InfiniteTimeSpan);
        }

        /// <summary>
        /// 创建一次性定时器（异步）
        /// </summary>
        /// <param name="delay">延迟时间</param>
        /// <param name="callback">回调</param>
        /// <returns>定时器</returns>
        public static Timer OnceAsync(TimeSpan delay, Func<Task> callback)
        {
            Timer? timer = null;
            timer = new Timer(async _ =>
            {
                timer?.Dispose();
                await callback();
            }, null, delay, Timeout.InfiniteTimeSpan);
            return timer;
        }

        /// <summary>
        /// 创建周期性定时器
        /// </summary>
        /// <param name="interval">间隔时间</param>
        /// <param name="callback">回调</param>
        /// <returns>定时器</returns>
        public static Timer Interval(TimeSpan interval, Action callback)
        {
            return new Timer(_ => callback(), null, interval, interval);
        }

        /// <summary>
        /// 创建周期性定时器（异步）
        /// </summary>
        /// <param name="interval">间隔时间</param>
        /// <param name="callback">回调</param>
        /// <returns>定时器</returns>
        public static Timer IntervalAsync(TimeSpan interval, Func<Task> callback)
        {
            Timer? timer = null;
            timer = new Timer(async _ =>
            {
                await callback();
            }, null, interval, interval);
            return timer;
        }

        /// <summary>
        /// 创建带延迟的周期性定时器
        /// </summary>
        /// <param name="dueTime">首次执行延迟</param>
        /// <param name="period">间隔时间</param>
        /// <param name="callback">回调</param>
        /// <returns>定时器</returns>
        public static Timer DelayedInterval(TimeSpan dueTime, TimeSpan period, Action callback)
        {
            return new Timer(_ => callback(), null, dueTime, period);
        }

        /// <summary>
        /// 等待指定时间后执行
        /// </summary>
        /// <param name="delay">延迟时间</param>
        /// <param name="callback">回调</param>
        /// <returns>可取消令牌</returns>
        public static CancellationTokenSource RunAfter(TimeSpan delay, Action callback)
        {
            var cts = new CancellationTokenSource();

            Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(delay, cts.Token);
                    if (!cts.Token.IsCancellationRequested)
                    {
                        callback();
                    }
                }
                catch (OperationCanceledException)
                {
                    // 取消时不执行回调
                }
            }, cts.Token);

            return cts;
        }

        /// <summary>
        /// 异步等待指定时间后执行
        /// </summary>
        /// <param name="delay">延迟时间</param>
        /// <param name="callback">回调</param>
        /// <returns>可取消令牌</returns>
        public static CancellationTokenSource RunAfterAsync(TimeSpan delay, Func<Task> callback)
        {
            var cts = new CancellationTokenSource();

            Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(delay, cts.Token);
                    if (!cts.Token.IsCancellationRequested)
                    {
                        await callback();
                    }
                }
                catch (OperationCanceledException)
                {
                    // 取消时不执行回调
                }
            }, cts.Token);

            return cts;
        }

        /// <summary>
        /// 重复执行直到条件满足
        /// </summary>
        /// <param name="interval">间隔时间</param>
        /// <param name="action">执行操作，返回是否继续</param>
        /// <param name="maxCount">最大执行次数（0表示无限）</param>
        /// <returns>可取消令牌</returns>
        public static CancellationTokenSource RepeatUntil(TimeSpan interval, Func<bool> action, int maxCount = 0)
        {
            var cts = new CancellationTokenSource();
            var count = 0;

            Task.Run(async () =>
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    if (maxCount > 0 && count >= maxCount)
                        break;

                    if (!action())
                        break;

                    count++;

                    try
                    {
                        await Task.Delay(interval, cts.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }
            }, cts.Token);

            return cts;
        }

        /// <summary>
        /// 异步重复执行直到条件满足
        /// </summary>
        /// <param name="interval">间隔时间</param>
        /// <param name="action">执行操作，返回是否继续</param>
        /// <param name="maxCount">最大执行次数（0表示无限）</param>
        /// <returns>可取消令牌</returns>
        public static CancellationTokenSource RepeatUntilAsync(TimeSpan interval, Func<Task<bool>> action, int maxCount = 0)
        {
            var cts = new CancellationTokenSource();
            var count = 0;

            Task.Run(async () =>
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    if (maxCount > 0 && count >= maxCount)
                        break;

                    if (!await action())
                        break;

                    count++;

                    try
                    {
                        await Task.Delay(interval, cts.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }
            }, cts.Token);

            return cts;
        }
    }

    /// <summary>
    /// 定时任务调度器
    /// </summary>
    public class ScheduledTask
    {
        private Timer? _timer;
        private readonly Action _callback;
        private readonly TimeSpan _interval;
        private readonly DateTime _startTime;
        private readonly int _maxRuns;
        private int _runCount;

        /// <summary>
        /// 任务名称
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 是否正在运行
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// 已运行次数
        /// </summary>
        public int RunCount => _runCount;

        /// <summary>
        /// 创建定时任务
        /// </summary>
        /// <param name="name">任务名称</param>
        /// <param name="callback">回调</param>
        /// <param name="interval">间隔时间</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="maxRuns">最大运行次数（0表示无限）</param>
        public ScheduledTask(string name, Action callback, TimeSpan interval, DateTime? startTime = null, int maxRuns = 0)
        {
            Name = name;
            _callback = callback;
            _interval = interval;
            _startTime = startTime ?? DateTime.MinValue;
            _maxRuns = maxRuns;
            _runCount = 0;
        }

        /// <summary>
        /// 启动任务
        /// </summary>
        public void Start()
        {
            if (IsRunning)
                return;

            IsRunning = true;

            var dueTime = _startTime > DateTime.MinValue
                ? _startTime - DateTime.Now
                : TimeSpan.Zero;

            if (dueTime < TimeSpan.Zero)
                dueTime = TimeSpan.Zero;

            _timer = new Timer(Execute, null, dueTime, _interval);
        }

        private void Execute(object? state)
        {
            if (_maxRuns > 0 && _runCount >= _maxRuns)
            {
                Stop();
                return;
            }

            try
            {
                _callback();
            }
            catch
            {
                // 忽略异常，继续执行
            }

            Interlocked.Increment(ref _runCount);
        }

        /// <summary>
        /// 停止任务
        /// </summary>
        public void Stop()
        {
            if (!IsRunning)
                return;

            IsRunning = false;
            _timer?.Dispose();
            _timer = null;
        }

        /// <summary>
        /// 重置运行计数
        /// </summary>
        public void Reset()
        {
            _runCount = 0;
        }
    }

    /// <summary>
    /// 定时任务管理器
    /// </summary>
    public class ScheduleManager
    {
        private readonly Dictionary<string, ScheduledTask> _tasks = new();

        /// <summary>
        /// 添加定时任务
        /// </summary>
        /// <param name="name">任务名称</param>
        /// <param name="callback">回调</param>
        /// <param name="interval">间隔时间</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="maxRuns">最大运行次数</param>
        /// <returns>定时任务</returns>
        public ScheduledTask AddTask(string name, Action callback, TimeSpan interval, DateTime? startTime = null, int maxRuns = 0)
        {
            var task = new ScheduledTask(name, callback, interval, startTime, maxRuns);
            _tasks[name] = task;
            return task;
        }

        /// <summary>
        /// 获取任务
        /// </summary>
        /// <param name="name">任务名称</param>
        /// <returns>定时任务</returns>
        public ScheduledTask? GetTask(string name)
        {
            return _tasks.TryGetValue(name, out var task) ? task : null;
        }

        /// <summary>
        /// 启动任务
        /// </summary>
        /// <param name="name">任务名称</param>
        /// <returns>是否成功</returns>
        public bool StartTask(string name)
        {
            var task = GetTask(name);
            if (task == null)
                return false;

            task.Start();
            return true;
        }

        /// <summary>
        /// 停止任务
        /// </summary>
        /// <param name="name">任务名称</param>
        /// <returns>是否成功</returns>
        public bool StopTask(string name)
        {
            var task = GetTask(name);
            if (task == null)
                return false;

            task.Stop();
            return true;
        }

        /// <summary>
        /// 移除任务
        /// </summary>
        /// <param name="name">任务名称</param>
        /// <returns>是否成功</returns>
        public bool RemoveTask(string name)
        {
            var task = GetTask(name);
            if (task == null)
                return false;

            task.Stop();
            _tasks.Remove(name);
            return true;
        }

        /// <summary>
        /// 启动所有任务
        /// </summary>
        public void StartAll()
        {
            foreach (var task in _tasks.Values)
            {
                task.Start();
            }
        }

        /// <summary>
        /// 停止所有任务
        /// </summary>
        public void StopAll()
        {
            foreach (var task in _tasks.Values)
            {
                task.Stop();
            }
        }

        /// <summary>
        /// 获取所有任务名称
        /// </summary>
        /// <returns>任务名称列表</returns>
        public string[] GetTaskNames()
        {
            return _tasks.Keys.ToArray();
        }

        /// <summary>
        /// 获取正在运行的任务数量
        /// </summary>
        /// <returns>数量</returns>
        public int GetRunningCount()
        {
            return _tasks.Values.Count(t => t.IsRunning);
        }
    }
}