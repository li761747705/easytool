using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EasyTool.ToolCategory
{
    /// <summary>
    /// 后台任务调度器
    /// </summary>
    public class BackgroundTaskScheduler : IDisposable
    {
        private readonly List<ScheduledTask> _tasks = new();
        private readonly object _lock = new();
        private readonly Timer _timer;
        private bool _disposed;

        /// <summary>
        /// 创建后台任务调度器
        /// </summary>
        /// <param name="checkInterval">检查间隔（毫秒）</param>
        public BackgroundTaskScheduler(int checkInterval = 1000)
        {
            _timer = new Timer(CheckTasks, null, checkInterval, checkInterval);
        }

        /// <summary>
        /// 添加定时任务
        /// </summary>
        public string Schedule(string name, Action action, DateTime executeAt)
        {
            var task = new ScheduledTask
            {
                Id = Guid.NewGuid().ToString("N"),
                Name = name,
                Action = action,
                ExecuteAt = executeAt,
                Type = ScheduledTaskType.Once
            };

            lock (_lock)
            {
                _tasks.Add(task);
            }

            return task.Id;
        }

        /// <summary>
        /// 添加延迟任务
        /// </summary>
        public string Schedule(string name, Action action, TimeSpan delay)
        {
            return Schedule(name, action, DateTime.UtcNow.Add(delay));
        }

        /// <summary>
        /// 添加周期性任务
        /// </summary>
        public string ScheduleRecurring(string name, Action action, TimeSpan interval, DateTime? startAt = null)
        {
            var task = new ScheduledTask
            {
                Id = Guid.NewGuid().ToString("N"),
                Name = name,
                Action = action,
                ExecuteAt = startAt ?? DateTime.UtcNow,
                Interval = interval,
                Type = ScheduledTaskType.Recurring
            };

            lock (_lock)
            {
                _tasks.Add(task);
            }

            return task.Id;
        }

        /// <summary>
        /// 添加Cron任务
        /// </summary>
        public string ScheduleCron(string name, Action action, string cronExpression)
        {
            var task = new ScheduledTask
            {
                Id = Guid.NewGuid().ToString("N"),
                Name = name,
                Action = action,
                CronExpression = cronExpression,
                Type = ScheduledTaskType.Cron,
                ExecuteAt = GetNextCronTime(cronExpression)
            };

            lock (_lock)
            {
                _tasks.Add(task);
            }

            return task.Id;
        }

        /// <summary>
        /// 取消任务
        /// </summary>
        public bool Cancel(string taskId)
        {
            lock (_lock)
            {
                var task = _tasks.Find(t => t.Id == taskId);
                if (task != null)
                {
                    task.IsCancelled = true;
                    _tasks.Remove(task);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 获取所有任务
        /// </summary>
        public List<ScheduledTaskInfo> GetAllTasks()
        {
            lock (_lock)
            {
                return _tasks.ConvertAll(t => new ScheduledTaskInfo
                {
                    Id = t.Id,
                    Name = t.Name,
                    Type = t.Type,
                    ExecuteAt = t.ExecuteAt,
                    Interval = t.Interval,
                    IsCancelled = t.IsCancelled,
                    LastExecution = t.LastExecution
                });
            }
        }

        private void CheckTasks(object? state)
        {
            List<ScheduledTask> tasksToExecute;

            lock (_lock)
            {
                var now = DateTime.UtcNow;
                tasksToExecute = _tasks.FindAll(t => !t.IsCancelled && t.ExecuteAt <= now);
            }

            foreach (var task in tasksToExecute)
            {
                Task.Run(() =>
                {
                    try
                    {
                        task.Action();
                        task.LastExecution = DateTime.UtcNow;
                    }
                    catch
                    {
                        // 忽略异常
                    }
                });

                // 更新下次执行时间
                lock (_lock)
                {
                    if (task.Type == ScheduledTaskType.Once)
                    {
                        _tasks.Remove(task);
                    }
                    else if (task.Type == ScheduledTaskType.Recurring)
                    {
                        task.ExecuteAt = DateTime.UtcNow.Add(task.Interval);
                    }
                    else if (task.Type == ScheduledTaskType.Cron)
                    {
                        task.ExecuteAt = GetNextCronTime(task.CronExpression);
                    }
                }
            }
        }

        private DateTime GetNextCronTime(string cronExpression)
        {
            // 简化实现，实际应使用CronUtil
            var parts = cronExpression.Split(' ');
            if (parts.Length >= 1 && int.TryParse(parts[0], out var minute))
            {
                var now = DateTime.UtcNow;
                var next = new DateTime(now.Year, now.Month, now.Day, now.Hour, minute, 0);
                if (next <= now)
                    next = next.AddHours(1);
                return next;
            }
            return DateTime.UtcNow.AddMinutes(1);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _timer.Dispose();
                lock (_lock)
                {
                    _tasks.Clear();
                }
            }
        }
    }

    internal class ScheduledTask
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public Action Action { get; set; } = () => { };
        public DateTime ExecuteAt { get; set; }
        public TimeSpan Interval { get; set; }
        public string? CronExpression { get; set; }
        public ScheduledTaskType Type { get; set; }
        public bool IsCancelled { get; set; }
        public DateTime? LastExecution { get; set; }
    }

    /// <summary>
    /// 任务类型
    /// </summary>
    public enum ScheduledTaskType
    {
        /// <summary>
        /// 单次执行
        /// </summary>
        Once,

        /// <summary>
        /// 周期执行
        /// </summary>
        Recurring,

        /// <summary>
        /// Cron表达式
        /// </summary>
        Cron
    }

    /// <summary>
    /// 计划任务信息
    /// </summary>
    public class ScheduledTaskInfo
    {
        /// <summary>
        /// 任务ID
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// 任务名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 任务类型
        /// </summary>
        public ScheduledTaskType Type { get; set; }

        /// <summary>
        /// 执行时间
        /// </summary>
        public DateTime ExecuteAt { get; set; }

        /// <summary>
        /// 执行间隔
        /// </summary>
        public TimeSpan Interval { get; set; }

        /// <summary>
        /// 是否已取消
        /// </summary>
        public bool IsCancelled { get; set; }

        /// <summary>
        /// 最后执行时间
        /// </summary>
        public DateTime? LastExecution { get; set; }
    }

    /// <summary>
    /// 任务队列
    /// </summary>
    /// <typeparam name="T">任务数据类型</typeparam>
    public class TaskQueue<T> : IDisposable
    {
        private readonly System.Collections.Concurrent.ConcurrentQueue<T> _queue = new();
        private readonly SemaphoreSlim _signal = new(0);
        private readonly CancellationTokenSource _cts = new();
        private readonly List<Task> _workers = new();
        private readonly Func<T, Task> _processor;
        private readonly int _maxDegreeOfParallelism;
        private bool _disposed;

        /// <summary>
        /// 队列数量
        /// </summary>
        public int Count => _queue.Count;

        /// <summary>
        /// 创建任务队列
        /// </summary>
        /// <param name="processor">处理函数</param>
        /// <param name="maxDegreeOfParallelism">最大并行度</param>
        public TaskQueue(Func<T, Task> processor, int maxDegreeOfParallelism = 4)
        {
            _processor = processor ?? throw new ArgumentNullException(nameof(processor));
            _maxDegreeOfParallelism = maxDegreeOfParallelism;

            for (int i = 0; i < maxDegreeOfParallelism; i++)
            {
                _workers.Add(Task.Run(WorkerAsync));
            }
        }

        /// <summary>
        /// 入队
        /// </summary>
        public void Enqueue(T item)
        {
            _queue.Enqueue(item);
            _signal.Release();
        }

        /// <summary>
        /// 批量入队
        /// </summary>
        public void EnqueueRange(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                Enqueue(item);
            }
        }

        private async Task WorkerAsync()
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                await _signal.WaitAsync(_cts.Token).ConfigureAwait(false);

                if (_queue.TryDequeue(out var item))
                {
                    try
                    {
                        await _processor(item).ConfigureAwait(false);
                    }
                    catch
                    {
                        // 忽略异常
                    }
                }
            }
        }

        /// <summary>
        /// 等待所有任务完成
        /// </summary>
        public async Task WaitForCompletionAsync()
        {
            while (_queue.Count > 0 || _signal.CurrentCount > 0)
            {
                await Task.Delay(100).ConfigureAwait(false);
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _cts.Cancel();

                try
                {
                    Task.WaitAll(_workers.ToArray(), TimeSpan.FromSeconds(5));
                }
                catch
                {
                    // 忽略
                }

                _cts.Dispose();
                _signal.Dispose();
            }
        }
    }
}