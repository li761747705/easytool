using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EasyTool.ToolCategory
{
    /// <summary>
    /// Task 异步任务扩展方法
    /// </summary>
    public static class TaskExtension
    {
        #region 任务忽略

        /// <summary>
        /// 忽略任务（Fire-and-forget），捕获并记录异常但不抛出
        /// </summary>
        /// <param name="task">要忽略的任务</param>
        /// <param name="onException">异常回调（可选）</param>
        public static void Forget(this Task? task, Action<Exception>? onException = null)
        {
            task?.ContinueWith(t =>
            {
                if (t.IsFaulted && t.Exception != null)
                {
                    onException?.Invoke(t.Exception);
                }
            }, CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Default);
        }

        /// <summary>
        /// 忽略任务（Fire-and-forget），捕获并记录异常但不抛出
        /// </summary>
        /// <typeparam name="T">任务返回类型</typeparam>
        /// <param name="task">要忽略的任务</param>
        /// <param name="onException">异常回调（可选）</param>
        public static void Forget<T>(this Task<T>? task, Action<Exception>? onException = null)
        {
            task?.ContinueWith(t =>
            {
                if (t.IsFaulted && t.Exception != null)
                {
                    onException?.Invoke(t.Exception);
                }
            }, CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Default);
        }

        #endregion

        #region 超时处理

        /// <summary>
        /// 为任务添加超时处理
        /// </summary>
        /// <param name="task">原始任务</param>
        /// <param name="timeout">超时时间</param>
        public static async Task<T> OrTimeout<T>(this Task<T> task, TimeSpan timeout)
        {
            var delayTask = Task.Delay(timeout);

            var completedTask = await Task.WhenAny(task, delayTask);

            if (completedTask == delayTask)
                throw new TimeoutException($"操作超时（{timeout.TotalSeconds}秒）");

            return await task;
        }

        /// <summary>
        /// 为任务添加超时处理
        /// </summary>
        /// <param name="task">原始任务</param>
        /// <param name="timeout">超时时间</param>
        public static async Task OrTimeout(this Task task, TimeSpan timeout)
        {
            var delayTask = Task.Delay(timeout);

            var completedTask = await Task.WhenAny(task, delayTask);

            if (completedTask == delayTask)
                throw new TimeoutException($"操作超时（{timeout.TotalSeconds}秒）");

            await task;
        }

        /// <summary>
        /// 为任务添加超时处理，超时返回默认值
        /// </summary>
        /// <param name="task">原始任务</param>
        /// <param name="timeout">超时时间</param>
        /// <param name="defaultValue">默认值</param>
        public static async Task<T?> OrTimeoutOrDefault<T>(this Task<T> task, TimeSpan timeout, T? defaultValue = default)
        {
            var delayTask = Task.Delay(timeout);

            var completedTask = await Task.WhenAny(task, delayTask);

            if (completedTask == delayTask)
                return defaultValue;

            return await task;
        }

        #endregion

        #region 重试机制

        /// <summary>
        /// 任务失败时自动重试
        /// </summary>
        /// <param name="taskFactory">任务工厂函数</param>
        /// <param name="retryCount">重试次数</param>
        /// <param name="delay">重试延迟时间</param>
        public static async Task<T> Retry<T>(Func<Task<T>> taskFactory, int retryCount = 3, TimeSpan? delay = null)
        {
            if (taskFactory == null)
                throw new ArgumentNullException(nameof(taskFactory));

            Exception? lastException = null;

            for (int i = 0; i <= retryCount; i++)
            {
                try
                {
                    return await taskFactory();
                }
                catch (Exception ex)
                {
                    lastException = ex;

                    if (i < retryCount && delay.HasValue)
                        await Task.Delay(delay.Value);
                }
            }

            throw lastException ?? new Exception("Retry failed");
        }

        /// <summary>
        /// 任务失败时自动重试
        /// </summary>
        /// <param name="taskFactory">任务工厂函数</param>
        /// <param name="retryCount">重试次数</param>
        /// <param name="delay">重试延迟时间</param>
        public static async Task Retry(Func<Task> taskFactory, int retryCount = 3, TimeSpan? delay = null)
        {
            if (taskFactory == null)
                throw new ArgumentNullException(nameof(taskFactory));

            Exception? lastException = null;

            for (int i = 0; i <= retryCount; i++)
            {
                try
                {
                    await taskFactory();
                    return;
                }
                catch (Exception ex)
                {
                    lastException = ex;

                    if (i < retryCount && delay.HasValue)
                        await Task.Delay(delay.Value);
                }
            }

            throw lastException ?? new Exception("Retry failed");
        }

        /// <summary>
        /// 任务失败时自动重试（带条件判断）
        /// </summary>
        /// <param name="taskFactory">任务工厂函数</param>
        /// <param name="shouldRetry">重试条件函数</param>
        /// <param name="retryCount">重试次数</param>
        /// <param name="delay">重试延迟时间</param>
        public static async Task<T> Retry<T>(Func<Task<T>> taskFactory, Func<Exception, bool> shouldRetry, int retryCount = 3, TimeSpan? delay = null)
        {
            if (taskFactory == null)
                throw new ArgumentNullException(nameof(taskFactory));
            if (shouldRetry == null)
                throw new ArgumentNullException(nameof(shouldRetry));

            Exception? lastException = null;

            for (int i = 0; i <= retryCount; i++)
            {
                try
                {
                    return await taskFactory();
                }
                catch (Exception ex)
                {
                    lastException = ex;

                    if (i < retryCount && shouldRetry(ex))
                    {
                        if (delay.HasValue)
                            await Task.Delay(delay.Value);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            throw lastException ?? new Exception("Retry failed");
        }

        #endregion

        #region 任务组合

        /// <summary>
        /// 在所有任务完成时返回，无论成功或失败
        /// </summary>
        public static async Task<Task[]> WhenAllOrAnyFailed(this IEnumerable<Task> tasks)
        {
            if (tasks == null)
                throw new ArgumentNullException(nameof(tasks));

            var taskArray = tasks.ToArray();
            if (taskArray.Length == 0)
                return taskArray;

            var tcs = new TaskCompletionSource<Task[]>();

            int remaining = taskArray.Length;
            var results = new Task[taskArray.Length];

            for (int i = 0; i < taskArray.Length; i++)
            {
                int index = i;
                taskArray[i].ContinueWith(t =>
                {
                    results[index] = t;
                    if (Interlocked.Decrement(ref remaining) == 0)
                    {
                        tcs.TrySetResult(results);
                    }
                }, TaskContinuationOptions.ExecuteSynchronously);
            }

            return await tcs.Task;
        }

        /// <summary>
        /// 返回第一个完成的任务（无论成功或失败）
        /// </summary>
        public static async Task<Task> WhenAnyFirstOrDefault(this IEnumerable<Task> tasks)
        {
            if (tasks == null)
                throw new ArgumentNullException(nameof(tasks));

            var taskArray = tasks.ToArray();
            if (taskArray.Length == 0)
                throw new ArgumentException("至少需要一个任务", nameof(tasks));

            return await Task.WhenAny(taskArray);
        }

        #endregion

        #region 任务超时与取消组合

        /// <summary>
        /// 创建一个带超时和取消令牌的任务
        /// </summary>
        public static async Task<T> WithTimeoutAndCancellation<T>(this Task<T> task, TimeSpan timeout, CancellationToken cancellationToken)
        {
            var timeoutCts = new CancellationTokenSource(timeout);
            var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            try
            {
                return await task;
            }
            catch (OperationCanceledException) when (timeoutCts.Token.IsCancellationRequested)
            {
                throw new TimeoutException($"操作超时（{timeout.TotalSeconds}秒）");
            }
            finally
            {
                timeoutCts.Dispose();
                combinedCts.Dispose();
            }
        }

        /// <summary>
        /// 创建一个带超时和取消令牌的任务
        /// </summary>
        public static async Task WithTimeoutAndCancellation(this Task task, TimeSpan timeout, CancellationToken cancellationToken)
        {
            var timeoutCts = new CancellationTokenSource(timeout);
            var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            try
            {
                await task;
            }
            catch (OperationCanceledException) when (timeoutCts.Token.IsCancellationRequested)
            {
                throw new TimeoutException($"操作超时（{timeout.TotalSeconds}秒）");
            }
            finally
            {
                timeoutCts.Dispose();
                combinedCts.Dispose();
            }
        }

        #endregion

        #region 任务结果处理

        /// <summary>
        /// 处理任务结果，无论成功或失败
        /// </summary>
        public static async Task<TResult> Finally<T, TResult>(this Task<T> task, Func<T, TResult> onSuccess, Func<Exception, TResult> onFailure)
        {
            try
            {
                var result = await task;
                return onSuccess(result);
            }
            catch (Exception ex)
            {
                return onFailure(ex);
            }
        }

        /// <summary>
        /// 处理任务结果，无论成功或失败
        /// </summary>
        public static async Task Finally(this Task task, Action onSuccess, Action<Exception> onFailure)
        {
            try
            {
                await task;
                onSuccess();
            }
            catch (Exception ex)
            {
                onFailure(ex);
            }
        }

        #endregion

        #region 任务延迟执行

        /// <summary>
        /// 延迟执行任务
        /// </summary>
        public static async Task<T> Delayed<T>(this Func<Task<T>> taskFactory, TimeSpan delay)
        {
            if (taskFactory == null)
                throw new ArgumentNullException(nameof(taskFactory));

            await Task.Delay(delay);
            return await taskFactory();
        }

        /// <summary>
        /// 延迟执行任务
        /// </summary>
        public static async Task Delayed(this Func<Task> taskFactory, TimeSpan delay)
        {
            if (taskFactory == null)
                throw new ArgumentNullException(nameof(taskFactory));

            await Task.Delay(delay);
            await taskFactory();
        }

        #endregion
    }
}
