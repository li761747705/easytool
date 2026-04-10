using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace EasyTool.ToolCategory
{
    /// <summary>
    /// 线程池管理工具类
    /// 提供线程池的创建、管理和监控功能
    /// </summary>
    public static class ThreadPoolUtil
    {
        /// <summary>
        /// 创建自定义线程池
        /// </summary>
        /// <param name="minThreads">最小线程数</param>
        /// <param name="maxThreads">最大线程数</param>
        /// <returns>自定义线程池实例</returns>
        public static CustomThreadPool Create(int minThreads = 1, int maxThreads = 10)
        {
            return new CustomThreadPool(minThreads, maxThreads);
        }

        /// <summary>
        /// 创建固定大小的线程池
        /// </summary>
        /// <param name="threadCount">线程数量</param>
        /// <returns>固定大小线程池实例</returns>
        public static FixedThreadPool CreateFixed(int threadCount)
        {
            return new FixedThreadPool(threadCount);
        }

        /// <summary>
        /// 获取全局线程池信息
        /// </summary>
        /// <returns>线程池信息</returns>
        public static ThreadPoolInfo GetGlobalPoolInfo()
        {
            ThreadPool.GetMinThreads(out int minWorkerThreads, out int minCompletionPortThreads);
            ThreadPool.GetMaxThreads(out int maxWorkerThreads, out int maxCompletionPortThreads);
            ThreadPool.GetAvailableThreads(out int availableWorkerThreads, out int availableCompletionPortThreads);

            return new ThreadPoolInfo
            {
                MinWorkerThreads = minWorkerThreads,
                MinCompletionPortThreads = minCompletionPortThreads,
                MaxWorkerThreads = maxWorkerThreads,
                MaxCompletionPortThreads = maxCompletionPortThreads,
                AvailableWorkerThreads = availableWorkerThreads,
                AvailableCompletionPortThreads = availableCompletionPortThreads,
                ActiveWorkerThreads = maxWorkerThreads - availableWorkerThreads,
                ActiveCompletionPortThreads = maxCompletionPortThreads - availableCompletionPortThreads
            };
        }

        /// <summary>
        /// 设置全局线程池大小
        /// </summary>
        /// <param name="minThreads">最小线程数</param>
        /// <param name="maxThreads">最大线程数</param>
        /// <returns>是否设置成功</returns>
        public static bool SetGlobalPoolSize(int minThreads, int maxThreads)
        {
            return ThreadPool.SetMinThreads(minThreads, minThreads) &&
                   ThreadPool.SetMaxThreads(maxThreads, maxThreads);
        }

        /// <summary>
        /// 设置全局线程池最小线程数
        /// </summary>
        /// <param name="minThreads">最小线程数</param>
        /// <returns>是否设置成功</returns>
        public static bool SetGlobalMinThreads(int minThreads)
        {
            return ThreadPool.SetMinThreads(minThreads, minThreads);
        }

        /// <summary>
        /// 设置全局线程池最大线程数
        /// </summary>
        /// <param name="maxThreads">最大线程数</param>
        /// <returns>是否设置成功</returns>
        public static bool SetGlobalMaxThreads(int maxThreads)
        {
            return ThreadPool.SetMaxThreads(maxThreads, maxThreads);
        }

        /// <summary>
        /// 等待所有任务完成
        /// </summary>
        /// <param name="tasks">要等待的任务数组</param>
        /// <param name="timeout">超时时间（可选）</param>
        /// <returns>是否在超时前完成</returns>
        public static bool WaitAll(Task[] tasks, TimeSpan? timeout = null)
        {
            if (tasks == null || tasks.Length == 0)
                return true;

            if (timeout.HasValue)
            {
                return Task.WaitAll(tasks, timeout.Value);
            }
            Task.WaitAll(tasks);
            return true;
        }

        /// <summary>
        /// 异步等待所有任务完成
        /// </summary>
        /// <param name="tasks">要等待的任务数组</param>
        /// <returns>Task</returns>
        public static async Task WaitAllAsync(Task[] tasks)
        {
            if (tasks == null || tasks.Length == 0)
                return;

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// 线程池信息
    /// </summary>
    public class ThreadPoolInfo
    {
        /// <summary>
        /// 最小工作线程数
        /// </summary>
        public int MinWorkerThreads { get; set; }

        /// <summary>
        /// 最小完成端口线程数
        /// </summary>
        public int MinCompletionPortThreads { get; set; }

        /// <summary>
        /// 最大工作线程数
        /// </summary>
        public int MaxWorkerThreads { get; set; }

        /// <summary>
        /// 最大完成端口线程数
        /// </summary>
        public int MaxCompletionPortThreads { get; set; }

        /// <summary>
        /// 可用工作线程数
        /// </summary>
        public int AvailableWorkerThreads { get; set; }

        /// <summary>
        /// 可用完成端口线程数
        /// </summary>
        public int AvailableCompletionPortThreads { get; set; }

        /// <summary>
        /// 活跃工作线程数
        /// </summary>
        public int ActiveWorkerThreads { get; set; }

        /// <summary>
        /// 活跃完成端口线程数
        /// </summary>
        public int ActiveCompletionPortThreads { get; set; }

        /// <summary>
        /// 总活跃线程数
        /// </summary>
        public int TotalActiveThreads => ActiveWorkerThreads + ActiveCompletionPortThreads;

        /// <summary>
        /// 线程池使用率（0-1）
        /// </summary>
        public double UsageRate => MaxWorkerThreads > 0 ? (double)ActiveWorkerThreads / MaxWorkerThreads : 0;

        /// <summary>
        /// 返回线程池信息的字符串表示
        /// </summary>
        /// <returns>线程池信息字符串</returns>
        public override string ToString()
        {
            return $"Worker: {ActiveWorkerThreads}/{MaxWorkerThreads} (Min: {MinWorkerThreads}), " +
                   $"IOCP: {ActiveCompletionPortThreads}/{MaxCompletionPortThreads} (Min: {MinCompletionPortThreads}), " +
                   $"Usage: {UsageRate:P1}";
        }
    }

    /// <summary>
    /// 自定义线程池
    /// </summary>
    public class CustomThreadPool : IDisposable
    {
        private readonly BlockingCollection<Action> _taskQueue;
        private readonly Thread[] _threads;
        private readonly CancellationTokenSource _cts;
        private readonly SemaphoreSlim _semaphore;
        private int _activeTasks;
        private bool _disposed;

        /// <summary>
        /// 最小线程数
        /// </summary>
        public int MinThreads { get; }

        /// <summary>
        /// 最大线程数
        /// </summary>
        public int MaxThreads { get; }

        /// <summary>
        /// 当前活跃线程数
        /// </summary>
        public int ActiveThreads => _activeTasks;

        /// <summary>
        /// 队列中等待的任务数
        /// </summary>
        public int QueuedTasks => _taskQueue.Count;

        /// <summary>
        /// 是否已关闭
        /// </summary>
        public bool IsShutdown => _cts.IsCancellationRequested;

        /// <summary>
        /// 创建自定义线程池
        /// </summary>
        /// <param name="minThreads">最小线程数</param>
        /// <param name="maxThreads">最大线程数</param>
        public CustomThreadPool(int minThreads = 1, int maxThreads = 10)
        {
            if (minThreads < 1)
                throw new ArgumentOutOfRangeException(nameof(minThreads), "最小线程数必须大于0");
            if (maxThreads < minThreads)
                throw new ArgumentOutOfRangeException(nameof(maxThreads), "最大线程数不能小于最小线程数");

            MinThreads = minThreads;
            MaxThreads = maxThreads;

            _taskQueue = new BlockingCollection<Action>(maxThreads * 10);
            _cts = new CancellationTokenSource();
            _semaphore = new SemaphoreSlim(maxThreads, maxThreads);
            _threads = new Thread[maxThreads];
            _activeTasks = 0;

            // 启动最小数量的线程
            for (int i = 0; i < minThreads; i++)
            {
                StartThread(i);
            }
        }

        /// <summary>
        /// 提交任务
        /// </summary>
        /// <param name="action">要执行的操作</param>
        public void Submit(Action action)
        {
            ThrowIfDisposed();
            _taskQueue.Add(action ?? throw new ArgumentNullException(nameof(action)));
        }

        /// <summary>
        /// 提交任务并返回 Task
        /// </summary>
        /// <param name="action">要执行的操作</param>
        /// <returns>Task 对象</returns>
        public Task SubmitAsync(Action action)
        {
            ThrowIfDisposed();
            var tcs = new TaskCompletionSource<bool>();
            
            Submit(() =>
            {
                try
                {
                    action();
                    tcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });

            return tcs.Task;
        }

        /// <summary>
        /// 提交带返回值的任务
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <param name="func">要执行的函数</param>
        /// <returns>返回值的 Task</returns>
        public Task<T> SubmitAsync<T>(Func<T> func)
        {
            ThrowIfDisposed();
            var tcs = new TaskCompletionSource<T>();

            Submit(() =>
            {
                try
                {
                    tcs.SetResult(func());
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });

            return tcs.Task;
        }

        /// <summary>
        /// 关闭线程池
        /// </summary>
        /// <param name="waitForCompletion">是否等待任务完成</param>
        /// <param name="timeout">超时时间</param>
        public void Shutdown(bool waitForCompletion = true, TimeSpan? timeout = null)
        {
            ThrowIfDisposed();
            _taskQueue.CompleteAdding();
            _cts.Cancel();

            if (waitForCompletion)
            {
                if (timeout.HasValue)
                {
                    foreach (var thread in _threads)
                    {
                        thread?.Join(timeout.Value);
                    }
                }
                else
                {
                    foreach (var thread in _threads)
                    {
                        thread?.Join();
                    }
                }
            }
        }

        /// <summary>
        /// 等待所有任务完成
        /// </summary>
        /// <param name="timeout">超时时间</param>
        /// <returns>是否在超时前完成</returns>
        public bool WaitAll(TimeSpan? timeout = null)
        {
            if (timeout.HasValue)
            {
                return _semaphore.Wait(timeout.Value);
            }
            _semaphore.Wait();
            _semaphore.Release();
            return true;
        }

        private void StartThread(int index)
        {
            var thread = new Thread(Worker)
            {
                IsBackground = true,
                Name = $"CustomThreadPool-{index}"
            };
            _threads[index] = thread;
            thread.Start();
        }

        private void Worker()
        {
            foreach (var action in _taskQueue.GetConsumingEnumerable(_cts.Token))
            {
                Interlocked.Increment(ref _activeTasks);
                try
                {
                    _semaphore.Wait(_cts.Token);
                    try
                    {
                        action();
                    }
                    finally
                    {
                        _semaphore.Release();
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                finally
                {
                    Interlocked.Decrement(ref _activeTasks);
                }
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(CustomThreadPool));
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            Shutdown(false);
            _taskQueue.Dispose();
            _cts.Dispose();
            _semaphore.Dispose();
            _disposed = true;
        }
    }

    /// <summary>
    /// 固定大小线程池
    /// </summary>
    public class FixedThreadPool : IDisposable
    {
        private readonly BlockingCollection<Action> _taskQueue;
        private readonly Thread[] _threads;
        private readonly CancellationTokenSource _cts;
        private int _activeTasks;
        private bool _disposed;

        /// <summary>
        /// 线程数量
        /// </summary>
        public int ThreadCount { get; }

        /// <summary>
        /// 当前活跃线程数
        /// </summary>
        public int ActiveThreads => _activeTasks;

        /// <summary>
        /// 队列中等待的任务数
        /// </summary>
        public int QueuedTasks => _taskQueue.Count;

        /// <summary>
        /// 是否已关闭
        /// </summary>
        public bool IsShutdown => _cts.IsCancellationRequested;

        /// <summary>
        /// 创建固定大小线程池
        /// </summary>
        /// <param name="threadCount">线程数量</param>
        public FixedThreadPool(int threadCount)
        {
            if (threadCount < 1)
                throw new ArgumentOutOfRangeException(nameof(threadCount), "线程数量必须大于0");

            ThreadCount = threadCount;
            _taskQueue = new BlockingCollection<Action>();
            _cts = new CancellationTokenSource();
            _threads = new Thread[threadCount];

            for (int i = 0; i < threadCount; i++)
            {
                var thread = new Thread(Worker)
                {
                    IsBackground = true,
                    Name = $"FixedThreadPool-{i}"
                };
                _threads[i] = thread;
                thread.Start();
            }
        }

        /// <summary>
        /// 提交任务
        /// </summary>
        /// <param name="action">要执行的操作</param>
        public void Submit(Action action)
        {
            ThrowIfDisposed();
            _taskQueue.Add(action ?? throw new ArgumentNullException(nameof(action)));
        }

        /// <summary>
        /// 提交任务并返回 Task
        /// </summary>
        /// <param name="action">要执行的操作</param>
        /// <returns>Task 对象</returns>
        public Task SubmitAsync(Action action)
        {
            ThrowIfDisposed();
            var tcs = new TaskCompletionSource<bool>();

            Submit(() =>
            {
                try
                {
                    action();
                    tcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });

            return tcs.Task;
        }

        /// <summary>
        /// 提交带返回值的任务
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <param name="func">要执行的函数</param>
        /// <returns>返回值的 Task</returns>
        public Task<T> SubmitAsync<T>(Func<T> func)
        {
            ThrowIfDisposed();
            var tcs = new TaskCompletionSource<T>();

            Submit(() =>
            {
                try
                {
                    tcs.SetResult(func());
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });

            return tcs.Task;
        }

        /// <summary>
        /// 关闭线程池
        /// </summary>
        /// <param name="waitForCompletion">是否等待任务完成</param>
        /// <param name="timeout">超时时间</param>
        public void Shutdown(bool waitForCompletion = true, TimeSpan? timeout = null)
        {
            ThrowIfDisposed();
            _taskQueue.CompleteAdding();
            _cts.Cancel();

            if (waitForCompletion)
            {
                if (timeout.HasValue)
                {
                    foreach (var thread in _threads)
                    {
                        thread?.Join(timeout.Value);
                    }
                }
                else
                {
                    foreach (var thread in _threads)
                    {
                        thread?.Join();
                    }
                }
            }
        }

        /// <summary>
        /// 等待队列清空
        /// </summary>
        /// <param name="timeout">超时时间</param>
        /// <returns>是否在超时前完成</returns>
        public bool WaitUntilEmpty(TimeSpan? timeout = null)
        {
            var startTime = DateTime.UtcNow;
            while (_taskQueue.Count > 0 || _activeTasks > 0)
            {
                if (timeout.HasValue && (DateTime.UtcNow - startTime) >= timeout.Value)
                    return false;
                Thread.Sleep(10);
            }
            return true;
        }

        private void Worker()
        {
            foreach (var action in _taskQueue.GetConsumingEnumerable(_cts.Token))
            {
                Interlocked.Increment(ref _activeTasks);
                try
                {
                    action();
                }
                catch
                {
                    // 忽略任务执行异常
                }
                finally
                {
                    Interlocked.Decrement(ref _activeTasks);
                }
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(FixedThreadPool));
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            Shutdown(false);
            _taskQueue.Dispose();
            _cts.Dispose();
            _disposed = true;
        }
    }
}
