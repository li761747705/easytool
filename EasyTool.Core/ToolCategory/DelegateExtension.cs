using System;
using System.Threading;
using System.Threading.Tasks;

namespace EasyTool.ToolCategory
{
    /// <summary>
    /// Delegate 委托扩展方法
    /// </summary>
    public static class DelegateExtension
    {
        #region 安全调用

        /// <summary>
        /// 安全调用 Action（捕获异常）
        /// </summary>
        public static void SafeInvoke(this Action? action, Action<Exception>? onError = null)
        {
            if (action == null)
                return;

            try
            {
                action();
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex);
            }
        }

        /// <summary>
        /// 安全调用 Func（捕获异常，失败返回默认值）
        /// </summary>
        public static T? SafeInvoke<T>(this Func<T>? func, T? defaultValue = default, Action<Exception>? onError = null)
        {
            if (func == null)
                return defaultValue;

            try
            {
                return func();
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex);
                return defaultValue;
            }
        }

        #endregion

        #region 重试

        /// <summary>
        /// Action 重试执行
        /// </summary>
        public static void Retry(this Action action, int retryCount = 3, int delayMs = 0)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            Exception? lastException = null;

            for (int i = 0; i <= retryCount; i++)
            {
                try
                {
                    action();
                    return;
                }
                catch (Exception ex)
                {
                    lastException = ex;

                    if (i < retryCount && delayMs > 0)
                    {
                        Thread.Sleep(delayMs);
                    }
                }
            }

            throw lastException ?? new Exception("Retry failed");
        }

        /// <summary>
        /// Func 重试执行
        /// </summary>
        public static T Retry<T>(this Func<T> func, int retryCount = 3, int delayMs = 0)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            Exception? lastException = null;

            for (int i = 0; i <= retryCount; i++)
            {
                try
                {
                    return func();
                }
                catch (Exception ex)
                {
                    lastException = ex;

                    if (i < retryCount && delayMs > 0)
                    {
                        Thread.Sleep(delayMs);
                    }
                }
            }

            throw lastException ?? new Exception("Retry failed");
        }

        /// <summary>
        /// 异步 Action 重试执行
        /// </summary>
        public static async Task RetryAsync(this Func<Task> action, int retryCount = 3, int delayMs = 0)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            Exception? lastException = null;

            for (int i = 0; i <= retryCount; i++)
            {
                try
                {
                    await action();
                    return;
                }
                catch (Exception ex)
                {
                    lastException = ex;

                    if (i < retryCount && delayMs > 0)
                    {
                        await Task.Delay(delayMs);
                    }
                }
            }

            throw lastException ?? new Exception("Retry failed");
        }

        /// <summary>
        /// 异步 Func 重试执行
        /// </summary>
        public static async Task<T> RetryAsync<T>(this Func<Task<T>> func, int retryCount = 3, int delayMs = 0)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            Exception? lastException = null;

            for (int i = 0; i <= retryCount; i++)
            {
                try
                {
                    return await func();
                }
                catch (Exception ex)
                {
                    lastException = ex;

                    if (i < retryCount && delayMs > 0)
                    {
                        await Task.Delay(delayMs);
                    }
                }
            }

            throw lastException ?? new Exception("Retry failed");
        }

        #endregion

        #region 超时

        /// <summary>
        /// 设置 Action 超时
        /// </summary>
        public static void WithTimeout(this Action action, int timeoutMs)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            var task = Task.Run(action);
            if (!task.Wait(timeoutMs))
                throw new TimeoutException($"操作超时（{timeoutMs}ms）");
        }

        /// <summary>
        /// 设置 Func 超时
        /// </summary>
        public static T WithTimeout<T>(this Func<T> func, int timeoutMs)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            var task = Task.Run(func);
            if (!task.Wait(timeoutMs))
                throw new TimeoutException($"操作超时（{timeoutMs}ms）");

            return task.Result;
        }

        #endregion

        #region 防抖与节流

        /// <summary>
        /// 防抖（延迟执行，如果在延迟时间内再次调用则重新计时）
        /// </summary>
        public static Action Debounce(this Action action, int delayMs)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            Timer? timer = null;
            return () =>
            {
                timer?.Dispose();
                timer = new Timer(state =>
                {
                    action();
                    timer?.Dispose();
                }, null, delayMs, Timeout.Infinite);
            };
        }

        /// <summary>
        /// 节流（指定时间间隔内只执行一次）
        /// </summary>
        public static Action Throttle(this Action action, int intervalMs)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            DateTime lastRun = DateTime.MinValue;
            return () =>
            {
                var now = DateTime.Now;
                if ((now - lastRun).TotalMilliseconds >= intervalMs)
                {
                    action();
                    lastRun = now;
                }
            };
        }

        #endregion

        #region 延迟执行

        /// <summary>
        /// 延迟执行 Action
        /// </summary>
        public static void Delay(this Action action, int delayMs)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            Task.Delay(delayMs).ContinueWith(_ => action());
        }

        /// <summary>
        /// 异步延迟执行 Action
        /// </summary>
        public static async Task DelayAsync(this Action action, int delayMs)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            await Task.Delay(delayMs);
            action();
        }

        /// <summary>
        /// 异步延迟执行 Func
        /// </summary>
        public static async Task<T> DelayAsync<T>(this Func<T> func, int delayMs)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            await Task.Delay(delayMs);
            return func();
        }

        #endregion

        #region 链式调用

        /// <summary>
        /// 链式调用 Action
        /// </summary>
        public static Action? Then(this Action? first, Action? second)
        {
            if (first == null)
                return second;
            if (second == null)
                return first;

            return () =>
            {
                first();
                second();
            };
        }

        /// <summary>
        /// 链式调用 Func
        /// </summary>
        public static Func<T?>? Then<T>(this Func<T>? first, Func<T>? second)
        {
            if (first == null)
                return second;
            if (second == null)
                return first;

            return () =>
            {
                first();
                return second();
            };
        }

        /// <summary>
        /// 链式调用 Func（转换）
        /// </summary>
        public static Func<TResult?>? Then<T, TResult>(this Func<T>? first, Func<T, TResult>? second)
        {
            if (first == null || second == null)
                return null;

            return () => second(first());
        }

        #endregion

        #region 条件执行

        /// <summary>
        /// 条件执行 Action
        /// </summary>
        public static Action? ExecuteIf(this Action? action, bool condition)
        {
            if (action == null)
                return null;

            return () =>
            {
                if (condition)
                    action();
            };
        }

        /// <summary>
        /// 条件执行 Func
        /// </summary>
        public static Func<T?>? ExecuteIf<T>(this Func<T>? func, bool condition)
        {
            if (func == null)
                return null;

            return () => condition ? func() : default;
        }

        #endregion

        #region 缓存

        /// <summary>
        /// 缓存 Func 结果
        /// </summary>
        public static Func<T?>? Cache<T>(this Func<T>? func)
        {
            if (func == null)
                return null;

            bool cached = false;
            T? value = default;

            return () =>
            {
                if (!cached)
                {
                    value = func();
                    cached = true;
                }
                return value;
            };
        }

        #endregion

        #region 组合

        /// <summary>
        /// 组合多个 Action
        /// </summary>
        public static Action Combine(params Action[] actions)
        {
            return () =>
            {
                foreach (var action in actions)
                {
                    action?.Invoke();
                }
            };
        }

        /// <summary>
        /// 组合多个 Func（后者的结果作为前者的参数）
        /// </summary>
        public static Func<T1, TResult>? Compose<T1, T2, TResult>(this Func<T2, TResult>? func1, Func<T1, T2>? func2)
        {
            if (func1 == null || func2 == null)
                return null;

            return x => func1(func2(x));
        }

        #endregion
    }
}
