using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EasyTool.ToolCategory
{
    /// <summary>
    /// 异步工具类
    /// 提供异步操作的辅助方法
    /// </summary>
    public static class AsyncUtil
    {
        #region 超时控制

        /// <summary>
        /// 带超时的异步操作
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="task">异步任务</param>
        /// <param name="timeoutMilliseconds">超时时间（毫秒）</param>
        /// <returns>任务结果</returns>
        public static async Task<T> WithTimeout<T>(Task<T> task, int timeoutMilliseconds)
        {
            using var cts = new CancellationTokenSource();
            var completedTask = await Task.WhenAny(task, Task.Delay(timeoutMilliseconds, cts.Token)).ConfigureAwait(false);

            if (completedTask == task)
            {
                cts.Cancel();
                return await task.ConfigureAwait(false);
            }

            throw new TimeoutException($"操作在 {timeoutMilliseconds} 毫秒后超时");
        }

        /// <summary>
        /// 带超时的异步操作
        /// </summary>
        /// <param name="task">异步任务</param>
        /// <param name="timeoutMilliseconds">超时时间（毫秒）</param>
        public static async Task WithTimeout(Task task, int timeoutMilliseconds)
        {
            using var cts = new CancellationTokenSource();
            var completedTask = await Task.WhenAny(task, Task.Delay(timeoutMilliseconds, cts.Token)).ConfigureAwait(false);

            if (completedTask == task)
            {
                cts.Cancel();
                await task.ConfigureAwait(false);
                return;
            }

            throw new TimeoutException($"操作在 {timeoutMilliseconds} 毫秒后超时");
        }

        /// <summary>
        /// 带超时的异步操作（返回默认值而非抛异常）
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="task">异步任务</param>
        /// <param name="timeoutMilliseconds">超时时间（毫秒）</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>任务结果或默认值</returns>
        public static async Task<T?> WithTimeoutOrDefault<T>(Task<T> task, int timeoutMilliseconds, T? defaultValue = default)
        {
            try
            {
                return await WithTimeout(task, timeoutMilliseconds).ConfigureAwait(false);
            }
            catch (TimeoutException)
            {
                return defaultValue;
            }
        }

        #endregion

        #region 重试机制

        /// <summary>
        /// 异步重试
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="func">异步函数</param>
        /// <param name="maxRetries">最大重试次数</param>
        /// <param name="delayMilliseconds">重试间隔（毫秒）</param>
        /// <param name="exponentialBackoff">是否指数退避</param>
        /// <returns>任务结果</returns>
        public static async Task<T> RetryAsync<T>(
            Func<Task<T>> func,
            int maxRetries = 3,
            int delayMilliseconds = 1000,
            bool exponentialBackoff = true)
        {
            Exception? lastException = null;

            for (int attempt = 0; attempt <= maxRetries; attempt++)
            {
                try
                {
                    return await func().ConfigureAwait(false);
                }
                // 捕获所有异常以支持重试（用户委托可能抛出任意异常）
                catch (Exception ex)
                {
                    lastException = ex;

                    if (attempt < maxRetries)
                    {
                        var delay = exponentialBackoff
                            ? delayMilliseconds * (int)Math.Pow(2, attempt)
                            : delayMilliseconds;

                        await Task.Delay(delay).ConfigureAwait(false);
                    }
                }
            }

            throw lastException ?? new Exception("重试失败");
        }

        /// <summary>
        /// 异步重试（无返回值）
        /// </summary>
        /// <param name="action">异步操作</param>
        /// <param name="maxRetries">最大重试次数</param>
        /// <param name="delayMilliseconds">重试间隔（毫秒）</param>
        /// <param name="exponentialBackoff">是否指数退避</param>
        public static async Task RetryAsync(
            Func<Task> action,
            int maxRetries = 3,
            int delayMilliseconds = 1000,
            bool exponentialBackoff = true)
        {
            await RetryAsync(async () =>
            {
                await action().ConfigureAwait(false);
                return true;
            }, maxRetries, delayMilliseconds, exponentialBackoff);
        }

        #endregion

        #region 并发控制

        /// <summary>
        /// 并发执行多个任务（限制并发数）
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="tasks">任务工厂集合</param>
        /// <param name="maxConcurrency">最大并发数</param>
        /// <returns>所有任务结果</returns>
        public static async Task<List<T>> WhenAllWithConcurrency<T>(
            IEnumerable<Func<Task<T>>> tasks,
            int maxConcurrency)
        {
            var results = new List<T>();
            var taskList = new List<Func<Task<T>>>(tasks);
            var semaphore = new SemaphoreSlim(maxConcurrency);

            var wrappedTasks = taskList.Select(async taskFactory =>
            {
                await semaphore.WaitAsync().ConfigureAwait(false);
                try
                {
                    return await taskFactory().ConfigureAwait(false);
                }
                finally
                {
                    semaphore.Release();
                }
            });

            results.AddRange(await Task.WhenAll(wrappedTasks).ConfigureAwait(false));
            return results;
        }

        /// <summary>
        /// 并发执行多个任务（限制并发数）
        /// </summary>
        /// <param name="actions">任务工厂集合</param>
        /// <param name="maxConcurrency">最大并发数</param>
        public static async Task WhenAllWithConcurrency(
            IEnumerable<Func<Task>> actions,
            int maxConcurrency)
        {
            var actionList = new List<Func<Task>>(actions);
            var semaphore = new SemaphoreSlim(maxConcurrency);

            var wrappedTasks = actionList.Select(async action =>
            {
                await semaphore.WaitAsync().ConfigureAwait(false);
                try
                {
                    await action().ConfigureAwait(false);
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(wrappedTasks).ConfigureAwait(false);
        }

        #endregion

        #region 批处理

        /// <summary>
        /// 批量处理数据
        /// </summary>
        /// <typeparam name="TInput">输入类型</typeparam>
        /// <typeparam name="TOutput">输出类型</typeparam>
        /// <param name="items">数据项</param>
        /// <param name="processor">处理函数</param>
        /// <param name="batchSize">批次大小</param>
        /// <param name="maxConcurrency">最大并发数</param>
        /// <returns>所有处理结果</returns>
        public static async Task<List<TOutput>> ProcessBatchAsync<TInput, TOutput>(
            IEnumerable<TInput> items,
            Func<TInput, Task<TOutput>> processor,
            int batchSize = 10,
            int maxConcurrency = 5)
        {
            var results = new List<TOutput>();
            var itemList = new List<TInput>(items);
            var batches = new List<List<TInput>>();

            for (int i = 0; i < itemList.Count; i += batchSize)
            {
                batches.Add(itemList.GetRange(i, Math.Min(batchSize, itemList.Count - i)));
            }

            foreach (var batch in batches)
            {
                var batchResults = await WhenAllWithConcurrency(
                    batch.Select<TInput, Func<Task<TOutput>>>(item => () => processor(item)),
                    maxConcurrency);

                results.AddRange(batchResults);
            }

            return results;
        }

        #endregion

        #region 延迟执行

        /// <summary>
        /// 延迟执行
        /// </summary>
        /// <param name="action">操作</param>
        /// <param name="delayMilliseconds">延迟时间（毫秒）</param>
        /// <param name="cancellationToken">取消令牌</param>
        public static async Task DelayAsync(
            Action action,
            int delayMilliseconds,
            CancellationToken cancellationToken = default)
        {
            await Task.Delay(delayMilliseconds, cancellationToken).ConfigureAwait(false);
            action();
        }

        /// <summary>
        /// 延迟执行（异步操作）
        /// </summary>
        /// <param name="action">异步操作</param>
        /// <param name="delayMilliseconds">延迟时间（毫秒）</param>
        /// <param name="cancellationToken">取消令牌</param>
        public static async Task DelayAsync(
            Func<Task> action,
            int delayMilliseconds,
            CancellationToken cancellationToken = default)
        {
            await Task.Delay(delayMilliseconds, cancellationToken).ConfigureAwait(false);
            await action().ConfigureAwait(false);
        }

        #endregion

        #region 取消支持

        /// <summary>
        /// 创建可取消的任务
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="func">异步函数</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>任务结果</returns>
        public static async Task<T> RunWithCancellation<T>(
            Func<CancellationToken, Task<T>> func,
            CancellationToken cancellationToken = default)
        {
            return await func(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// 创建带取消令牌的任务超时
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="func">异步函数</param>
        /// <param name="timeoutMilliseconds">超时时间（毫秒）</param>
        /// <returns>任务结果</returns>
        public static async Task<T> RunWithTimeout<T>(
            Func<CancellationToken, Task<T>> func,
            int timeoutMilliseconds)
        {
            using var cts = new CancellationTokenSource(timeoutMilliseconds);
            return await func(cts.Token).ConfigureAwait(false);
        }

        #endregion

        #region 顺序执行

        /// <summary>
        /// 顺序执行多个异步任务
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="tasks">任务工厂集合</param>
        /// <returns>所有任务结果</returns>
        public static async Task<List<T>> ExecuteSequentially<T>(IEnumerable<Func<Task<T>>> tasks)
        {
            var results = new List<T>();

            foreach (var taskFactory in tasks)
            {
                results.Add(await taskFactory().ConfigureAwait(false));
            }

            return results;
        }

        /// <summary>
        /// 顺序执行多个异步任务（无返回值）
        /// </summary>
        /// <param name="actions">任务工厂集合</param>
        public static async Task ExecuteSequentially(IEnumerable<Func<Task>> actions)
        {
            foreach (var action in actions)
            {
                await action().ConfigureAwait(false);
            }
        }

        #endregion

        #region 结果收集

        /// <summary>
        /// 并行执行并收集成功/失败结果
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="tasks">任务工厂集合</param>
        /// <returns>成功和失败的结果</returns>
        public static async Task<(List<T> Successes, List<Exception> Failures)> CollectResults<T>(
            IEnumerable<Func<Task<T>>> tasks)
        {
            var successes = new List<T>();
            var failures = new List<Exception>();

            var results = await Task.WhenAll(tasks.Select(async taskFactory =>
            {
                try
                {
                    return (Success: true, Result: await taskFactory().ConfigureAwait(false), Exception: (Exception?)null);
                }
                // 捕获所有异常以收集失败结果（并行任务需容忍部分失败）
                catch (Exception ex)
                {
                    return (Success: false, Result: default(T), Exception: ex);
                }
            }));

            foreach (var result in results)
            {
                if (result.Success)
                {
                    successes.Add(result.Result!);
                }
                else if (result.Exception != null)
                {
                    failures.Add(result.Exception);
                }
            }

            return (successes, failures);
        }

        #endregion
    }
}
