using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace EasyTool.ToolCategory
{
    /// <summary>
    /// 异步锁
    /// 支持异步等待的互斥锁
    /// </summary>
    public class AsyncLock
    {
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private readonly Task<Releaser> _releaser;

        /// <summary>
        /// 创建异步锁
        /// </summary>
        public AsyncLock()
        {
            _releaser = Task.FromResult(new Releaser(this));
        }

        /// <summary>
        /// 获取锁
        /// </summary>
        /// <returns>释放器</returns>
        public Task<Releaser> LockAsync()
        {
            var wait = _semaphore.WaitAsync();
            return wait.IsCompleted
                ? _releaser
                : wait.ContinueWith((_, state) => new Releaser((AsyncLock)state!),
                    this, CancellationToken.None,
                    TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }

        /// <summary>
        /// 尝试获取锁
        /// </summary>
        /// <param name="releaser">释放器</param>
        /// <returns>是否成功获取</returns>
        public bool TryLock(out Releaser releaser)
        {
            if (_semaphore.Wait(0))
            {
                releaser = new Releaser(this);
                return true;
            }
            releaser = default;
            return false;
        }

        /// <summary>
        /// 尝试获取锁（带超时）
        /// </summary>
        /// <param name="timeout">超时时间</param>
        /// <param name="releaser">释放器</param>
        /// <returns>是否成功获取</returns>
        public bool TryLock(TimeSpan timeout, out Releaser releaser)
        {
            if (_semaphore.Wait(timeout))
            {
                releaser = new Releaser(this);
                return true;
            }
            releaser = default;
            return false;
        }

        /// <summary>
        /// 尝试获取锁（带超时，异步）
        /// </summary>
        /// <param name="timeout">超时时间</param>
        /// <returns>是否成功获取和释放器</returns>
        public async Task<(bool acquired, Releaser releaser)> TryLockAsync(TimeSpan timeout)
        {
            if (await _semaphore.WaitAsync(timeout))
            {
                return (true, new Releaser(this));
            }
            return (false, default);
        }

        /// <summary>
        /// 锁释放器
        /// </summary>
        public struct Releaser : IDisposable
        {
            private readonly AsyncLock? _lock;

            internal Releaser(AsyncLock @lock)
            {
                _lock = @lock;
            }

            public void Dispose()
            {
                _lock?._semaphore.Release();
            }
        }
    }

    /// <summary>
    /// 异步读写锁
    /// 支持读写分离的异步锁
    /// </summary>
    public class AsyncReaderWriterLock
    {
        private readonly SemaphoreSlim _readLock = new(1, 1);
        private readonly SemaphoreSlim _writeLock = new(1, 1);
        private int _readersCount;

        /// <summary>
        /// 获取读锁
        /// </summary>
        /// <returns>释放器</returns>
        public async Task<ReaderReleaser> ReaderLockAsync()
        {
            await _readLock.WaitAsync();
            if (Interlocked.Increment(ref _readersCount) == 1)
            {
                await _writeLock.WaitAsync();
            }
            _readLock.Release();

            return new ReaderReleaser(this);
        }

        /// <summary>
        /// 获取写锁
        /// </summary>
        /// <returns>释放器</returns>
        public async Task<WriterReleaser> WriterLockAsync()
        {
            await _writeLock.WaitAsync();
            return new WriterReleaser(this);
        }

        /// <summary>
        /// 尝试获取读锁（带超时）
        /// </summary>
        public async Task<(bool acquired, ReaderReleaser releaser)> TryReaderLockAsync(TimeSpan timeout)
        {
            if (!await _readLock.WaitAsync(timeout))
                return (false, default);

            try
            {
                if (Interlocked.Increment(ref _readersCount) == 1)
                {
                    if (!await _writeLock.WaitAsync(timeout))
                    {
                        Interlocked.Decrement(ref _readersCount);
                        return (false, default);
                    }
                }
                _readLock.Release();
                return (true, new ReaderReleaser(this));
            }
            catch
            {
                _readLock.Release();
                return (false, default);
            }
        }

        /// <summary>
        /// 尝试获取写锁（带超时）
        /// </summary>
        public async Task<(bool acquired, WriterReleaser releaser)> TryWriterLockAsync(TimeSpan timeout)
        {
            if (await _writeLock.WaitAsync(timeout))
            {
                return (true, new WriterReleaser(this));
            }
            return (false, default);
        }

        /// <summary>
        /// 读锁释放器
        /// </summary>
        public struct ReaderReleaser : IDisposable
        {
            private readonly AsyncReaderWriterLock? _lock;

            internal ReaderReleaser(AsyncReaderWriterLock @lock)
            {
                _lock = @lock;
            }

            public void Dispose()
            {
                if (_lock == null) return;

                _lock._readLock.Wait();
                if (Interlocked.Decrement(ref _lock._readersCount) == 0)
                {
                    _lock._writeLock.Release();
                }
                _lock._readLock.Release();
            }
        }

        /// <summary>
        /// 写锁释放器
        /// </summary>
        public struct WriterReleaser : IDisposable
        {
            private readonly AsyncReaderWriterLock? _lock;

            internal WriterReleaser(AsyncReaderWriterLock @lock)
            {
                _lock = @lock;
            }

            public void Dispose()
            {
                _lock?._writeLock.Release();
            }
        }
    }

    /// <summary>
    /// 异步信号量
    /// </summary>
    public class AsyncSemaphore
    {
        private readonly SemaphoreSlim _semaphore;

        /// <summary>
        /// 当前计数
        /// </summary>
        public int CurrentCount => _semaphore.CurrentCount;

        /// <summary>
        /// 创建异步信号量
        /// </summary>
        /// <param name="initialCount">初始计数</param>
        /// <param name="maxCount">最大计数</param>
        public AsyncSemaphore(int initialCount, int maxCount = int.MaxValue)
        {
            _semaphore = new SemaphoreSlim(initialCount, maxCount);
        }

        /// <summary>
        /// 等待信号
        /// </summary>
        public Task WaitAsync()
        {
            return _semaphore.WaitAsync();
        }

        /// <summary>
        /// 等待信号（带超时）
        /// </summary>
        public Task<bool> WaitAsync(TimeSpan timeout)
        {
            return _semaphore.WaitAsync(timeout);
        }

        /// <summary>
        /// 等待信号（带取消令牌）
        /// </summary>
        public Task WaitAsync(CancellationToken cancellationToken)
        {
            return _semaphore.WaitAsync(cancellationToken);
        }

        /// <summary>
        /// 释放信号
        /// </summary>
        public void Release()
        {
            _semaphore.Release();
        }

        /// <summary>
        /// 释放指定数量的信号
        /// </summary>
        public void Release(int releaseCount)
        {
            _semaphore.Release(releaseCount);
        }
    }

    /// <summary>
    /// 异步自动重置事件
    /// </summary>
    public class AsyncAutoResetEvent
    {
        private readonly Queue<TaskCompletionSource<bool>> _waits = new();
        private bool _signaled;

        /// <summary>
        /// 创建异步自动重置事件
        /// </summary>
        /// <param name="initialState">初始状态</param>
        public AsyncAutoResetEvent(bool initialState = false)
        {
            _signaled = initialState;
        }

        /// <summary>
        /// 等待信号
        /// </summary>
        public Task WaitAsync()
        {
            lock (_waits)
            {
                if (_signaled)
                {
                    _signaled = false;
                    return Task.CompletedTask;
                }

                var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                _waits.Enqueue(tcs);
                return tcs.Task;
            }
        }

        /// <summary>
        /// 发送信号
        /// </summary>
        public void Set()
        {
            lock (_waits)
            {
                if (_waits.Count > 0)
                {
                    var tcs = _waits.Dequeue();
                    tcs.TrySetResult(true);
                }
                else if (!_signaled)
                {
                    _signaled = true;
                }
            }
        }
    }

    /// <summary>
    /// 异步手动重置事件
    /// </summary>
    public class AsyncManualResetEvent
    {
        private TaskCompletionSource<bool> _tcs;

        /// <summary>
        /// 创建异步手动重置事件
        /// </summary>
        /// <param name="initialState">初始状态</param>
        public AsyncManualResetEvent(bool initialState = false)
        {
            _tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            if (initialState)
            {
                _tcs.TrySetResult(true);
            }
        }

        /// <summary>
        /// 等待信号
        /// </summary>
        public Task WaitAsync()
        {
            return _tcs.Task;
        }

        /// <summary>
        /// 发送信号（设置）
        /// </summary>
        public void Set()
        {
            _tcs.TrySetResult(true);
        }

        /// <summary>
        /// 重置
        /// </summary>
        public void Reset()
        {
            var newTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            while (true)
            {
                var oldTcs = _tcs;
                if (!oldTcs.Task.IsCompleted)
                    return;

                if (Interlocked.CompareExchange(ref _tcs, newTcs, oldTcs) == oldTcs)
                    return;
            }
        }

        /// <summary>
        /// 是否已设置
        /// </summary>
        public bool IsSet => _tcs.Task.IsCompleted;
    }

    /// <summary>
    /// 异步倒计时事件
    /// </summary>
    public class AsyncCountdownEvent
    {
        private int _count;
        private readonly TaskCompletionSource<bool> _tcs;

        /// <summary>
        /// 当前计数
        /// </summary>
        public int CurrentCount => _count;

        /// <summary>
        /// 是否已完成
        /// </summary>
        public bool IsSet => _count == 0;

        /// <summary>
        /// 创建异步倒计时事件
        /// </summary>
        /// <param name="initialCount">初始计数</param>
        public AsyncCountdownEvent(int initialCount)
        {
            _count = initialCount;
            _tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            if (initialCount <= 0)
            {
                _tcs.TrySetResult(true);
            }
        }

        /// <summary>
        /// 等待完成
        /// </summary>
        public Task WaitAsync()
        {
            return _tcs.Task;
        }

        /// <summary>
        /// 信号（计数减1）
        /// </summary>
        public void Signal()
        {
            if (Interlocked.Decrement(ref _count) == 0)
            {
                _tcs.TrySetResult(true);
            }
        }

        /// <summary>
        /// 批量信号
        /// </summary>
        /// <param name="signalCount">信号数量</param>
        public void Signal(int signalCount)
        {
            if (signalCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(signalCount));

            if (Interlocked.Add(ref _count, -signalCount) == 0)
            {
                _tcs.TrySetResult(true);
            }
        }

        /// <summary>
        /// 增加计数
        /// </summary>
        public void AddCount()
        {
            Interlocked.Increment(ref _count);
        }

        /// <summary>
        /// 重置
        /// </summary>
        /// <param name="count">新计数</param>
        public void Reset(int count)
        {
            _count = count;
        }
    }

    /// <summary>
    /// 异步锁工具类
    /// </summary>
    public static class AsyncLockUtil
    {
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, AsyncLock> _locks = new();
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, AsyncReaderWriterLock> _rwLocks = new();
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, AsyncSemaphore> _semaphores = new();

        /// <summary>
        /// 获取或创建异步锁
        /// </summary>
        /// <param name="name">锁名称</param>
        /// <returns>异步锁</returns>
        public static AsyncLock GetOrCreateLock(string name)
        {
            return _locks.GetOrAdd(name, _ => new AsyncLock());
        }

        /// <summary>
        /// 使用锁执行操作
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="name">锁名称</param>
        /// <param name="action">操作</param>
        /// <returns>操作结果</returns>
        public static async Task<T> WithLockAsync<T>(string name, Func<Task<T>> action)
        {
            var @lock = GetOrCreateLock(name);
            using (await @lock.LockAsync())
            {
                return await action();
            }
        }

        /// <summary>
        /// 使用锁执行操作（无返回值）
        /// </summary>
        /// <param name="name">锁名称</param>
        /// <param name="action">操作</param>
        public static async Task WithLockAsync(string name, Func<Task> action)
        {
            var @lock = GetOrCreateLock(name);
            using (await @lock.LockAsync())
            {
                await action();
            }
        }

        /// <summary>
        /// 获取或创建读写锁
        /// </summary>
        /// <param name="name">锁名称</param>
        /// <returns>读写锁</returns>
        public static AsyncReaderWriterLock GetOrCreateReaderWriterLock(string name)
        {
            return _rwLocks.GetOrAdd(name, _ => new AsyncReaderWriterLock());
        }

        /// <summary>
        /// 使用读锁执行操作
        /// </summary>
        public static async Task<T> WithReaderLockAsync<T>(string name, Func<Task<T>> action)
        {
            var @lock = GetOrCreateReaderWriterLock(name);
            using (await @lock.ReaderLockAsync())
            {
                return await action();
            }
        }

        /// <summary>
        /// 使用写锁执行操作
        /// </summary>
        public static async Task<T> WithWriterLockAsync<T>(string name, Func<Task<T>> action)
        {
            var @lock = GetOrCreateReaderWriterLock(name);
            using (await @lock.WriterLockAsync())
            {
                return await action();
            }
        }

        /// <summary>
        /// 获取或创建信号量
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="initialCount">初始计数</param>
        /// <param name="maxCount">最大计数</param>
        /// <returns>信号量</returns>
        public static AsyncSemaphore GetOrCreateSemaphore(string name, int initialCount = 1, int maxCount = int.MaxValue)
        {
            return _semaphores.GetOrAdd(name, _ => new AsyncSemaphore(initialCount, maxCount));
        }

        /// <summary>
        /// 创建异步自动重置事件
        /// </summary>
        public static AsyncAutoResetEvent CreateAutoResetEvent(bool initialState = false)
        {
            return new AsyncAutoResetEvent(initialState);
        }

        /// <summary>
        /// 创建异步手动重置事件
        /// </summary>
        public static AsyncManualResetEvent CreateManualResetEvent(bool initialState = false)
        {
            return new AsyncManualResetEvent(initialState);
        }

        /// <summary>
        /// 创建异步倒计时事件
        /// </summary>
        public static AsyncCountdownEvent CreateCountdownEvent(int initialCount)
        {
            return new AsyncCountdownEvent(initialCount);
        }

        /// <summary>
        /// 移除锁
        /// </summary>
        public static bool RemoveLock(string name)
        {
            return _locks.TryRemove(name, out _);
        }

        /// <summary>
        /// 清空所有锁
        /// </summary>
        public static void Clear()
        {
            _locks.Clear();
            _rwLocks.Clear();
            _semaphores.Clear();
        }
    }
}
