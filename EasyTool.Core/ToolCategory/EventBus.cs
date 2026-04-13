using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EasyTool.ToolCategory
{
    /// <summary>
    /// 订阅令牌，用于安全取消订阅
    /// </summary>
    public sealed class SubscriptionToken : IDisposable
    {
        private readonly Action _unsubscribe;
        private bool _disposed;

        internal SubscriptionToken(Action unsubscribe)
        {
            _unsubscribe = unsubscribe ?? throw new ArgumentNullException(nameof(unsubscribe));
        }

        /// <summary>
        /// 取消订阅
        /// </summary>
        public void Unsubscribe()
        {
            if (!_disposed)
            {
                _unsubscribe();
                _disposed = true;
            }
        }

        /// <summary>
        /// 释放资源（自动取消订阅）
        /// </summary>
        public void Dispose()
        {
            Unsubscribe();
        }
    }

    /// <summary>
    /// 事件总线
    /// 提供发布/订阅模式的实现，支持令牌取消订阅
    /// </summary>
    /// <remarks>
    /// 线程安全：是。使用 lock 保护订阅/取消订阅操作。
    /// 异常隔离：单个 handler 抛出异常不会影响其他 handler 的执行。
    /// </remarks>
    public static class EventBus
    {
        private static readonly Dictionary<Type, List<(Guid id, Delegate handler)>> _handlers = new();
        private static readonly object _lock = new();

        /// <summary>
        /// 订阅事件，返回可用于取消订阅的令牌
        /// </summary>
        /// <typeparam name="T">事件数据类型</typeparam>
        /// <param name="handler">事件处理委托</param>
        /// <returns>订阅令牌，可用于取消订阅</returns>
        /// <exception cref="ArgumentNullException">当 handler 为 null 时抛出</exception>
        public static SubscriptionToken Subscribe<T>(Action<T> handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            var id = Guid.NewGuid();
            lock (_lock)
            {
                if (!_handlers.TryGetValue(typeof(T), out var handlers))
                {
                    handlers = new List<(Guid, Delegate)>();
                    _handlers[typeof(T)] = handlers;
                }
                handlers.Add((id, handler));
            }

            return new SubscriptionToken(() => RemoveHandler<T>(id));
        }

        /// <summary>
        /// 使用令牌取消订阅
        /// </summary>
        /// <typeparam name="T">事件数据类型</typeparam>
        /// <param name="token">订阅令牌</param>
        public static void Unsubscribe<T>(SubscriptionToken token)
        {
            token?.Unsubscribe();
        }

        /// <summary>
        /// 使用委托取消订阅（向后兼容，建议使用令牌模式）
        /// </summary>
        /// <typeparam name="T">事件数据类型</typeparam>
        /// <param name="handler">事件处理委托</param>
        /// <exception cref="ArgumentNullException">当 handler 为 null 时抛出</exception>
        public static void Unsubscribe<T>(Action<T> handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            lock (_lock)
            {
                if (_handlers.TryGetValue(typeof(T), out var handlers))
                {
                    var index = handlers.FindIndex(h => h.handler == handler);
                    if (index >= 0)
                    {
                        handlers.RemoveAt(index);
                    }
                }
            }
        }

        private static void RemoveHandler<T>(Guid id)
        {
            lock (_lock)
            {
                if (_handlers.TryGetValue(typeof(T), out var handlers))
                {
                    var index = handlers.FindIndex(h => h.id == id);
                    if (index >= 0)
                    {
                        handlers.RemoveAt(index);
                    }
                }
            }
        }

        /// <summary>
        /// 发布事件
        /// </summary>
        /// <typeparam name="T">事件数据类型</typeparam>
        /// <param name="eventData">事件数据</param>
        /// <exception cref="AggregateException">当一个或多个 handler 抛出异常时，汇总所有异常</exception>
        public static void Publish<T>(T eventData)
        {
            List<Delegate>? handlerDelegates;
            lock (_lock)
            {
                if (!_handlers.TryGetValue(typeof(T), out var handlers))
                    return;
                handlerDelegates = handlers.ConvertAll(h => h.handler);
            }

            var exceptions = new List<Exception>();
            foreach (var handler in handlerDelegates)
            {
                if (handler is Action<T> typedHandler)
                {
                    try
                    {
                        typedHandler(eventData);
                    }
                    // 捕获所有异常以隔离事件处理器（防止一个处理器失败影响其他处理器）
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                    }
                }
            }

            if (exceptions.Count > 0)
                throw new AggregateException("一个或多个事件处理器执行失败", exceptions);
        }

        /// <summary>
        /// 异步发布事件
        /// </summary>
        /// <typeparam name="T">事件数据类型</typeparam>
        /// <param name="eventData">事件数据</param>
        /// <returns>表示异步操作的 Task</returns>
        public static async Task PublishAsync<T>(T eventData)
        {
            List<Delegate>? handlerDelegates;
            lock (_lock)
            {
                if (!_handlers.TryGetValue(typeof(T), out var handlers))
                    return;
                handlerDelegates = handlers.ConvertAll(h => h.handler);
            }

            var exceptions = new List<Exception>();
            var tasks = new List<Task>();
            foreach (var handler in handlerDelegates)
            {
                if (handler is Action<T> typedHandler)
                {
                    tasks.Add(Task.Run(() =>
                    {
                        try
                        {
                            typedHandler(eventData);
                        }
                        // 捕获所有异常以隔离事件处理器（并行执行时防止一个失败影响其他）
                        catch (Exception ex)
                        {
                            lock (exceptions)
                            {
                                exceptions.Add(ex);
                            }
                        }
                    }));
                }
                else if (handler is Func<T, Task> asyncHandler)
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            await asyncHandler(eventData).ConfigureAwait(false);
                        }
                        // 捕获所有异常以隔离异步事件处理器
                        catch (Exception ex)
                        {
                            lock (exceptions)
                            {
                                exceptions.Add(ex);
                            }
                        }
                    }));
                }
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);

            if (exceptions.Count > 0)
                throw new AggregateException("一个或多个事件处理器执行失败", exceptions);
        }

        /// <summary>
        /// 订阅异步事件，返回可用于取消订阅的令牌
        /// </summary>
        /// <typeparam name="T">事件数据类型</typeparam>
        /// <param name="handler">异步事件处理委托</param>
        /// <returns>订阅令牌，可用于取消订阅</returns>
        /// <exception cref="ArgumentNullException">当 handler 为 null 时抛出</exception>
        public static SubscriptionToken SubscribeAsync<T>(Func<T, Task> handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            var id = Guid.NewGuid();
            lock (_lock)
            {
                if (!_handlers.TryGetValue(typeof(T), out var handlers))
                {
                    handlers = new List<(Guid, Delegate)>();
                    _handlers[typeof(T)] = handlers;
                }
                handlers.Add((id, handler));
            }

            return new SubscriptionToken(() => RemoveHandler<T>(id));
        }

        /// <summary>
        /// 清除所有订阅
        /// </summary>
        public static void Clear<T>()
        {
            lock (_lock)
            {
                _handlers.Clear();
            }
        }

        /// <summary>
        /// 清除指定类型的订阅
        /// </summary>
        /// <typeparam name="T">事件数据类型</typeparam>
        public static void ClearAll<T>()
        {
            lock (_lock)
            {
                _handlers.Remove(typeof(T));
            }
        }
    }

    /// <summary>
    /// 泛型事件总线
    /// </summary>
    public class EventBus<T> where T : class
    {
        private static readonly EventBus<T> _instance = new();
        private readonly List<(Guid id, Action<T>)> _handlers = new();
        private readonly List<(Guid id, Func<T, Task>)> _asyncHandlers = new();
        private readonly object _lock = new();

        /// <summary>
        /// 获取单例实例
        /// </summary>
        public static EventBus<T> Instance => _instance;

        /// <summary>
        /// 订阅，返回可用于取消订阅的令牌
        /// </summary>
        /// <param name="handler">事件处理委托</param>
        /// <returns>订阅令牌，可用于取消订阅</returns>
        /// <exception cref="ArgumentNullException">当 handler 为 null 时抛出</exception>
        public SubscriptionToken Subscribe(Action<T> handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            var id = Guid.NewGuid();
            lock (_lock)
            {
                _handlers.Add((id, handler));
            }

            return new SubscriptionToken(() => RemoveHandler(id, _handlers));
        }

        /// <summary>
        /// 使用令牌取消订阅
        /// </summary>
        /// <param name="token">订阅令牌</param>
        public void Unsubscribe(SubscriptionToken token)
        {
            token?.Unsubscribe();
        }

        /// <summary>
        /// 使用委托取消订阅（向后兼容，建议使用令牌模式）
        /// </summary>
        /// <param name="handler">事件处理委托</param>
        /// <exception cref="ArgumentNullException">当 handler 为 null 时抛出</exception>
        public void Unsubscribe(Action<T> handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            lock (_lock)
            {
                var index = _handlers.FindIndex(h => h.Item2 == handler);
                if (index >= 0)
                {
                    _handlers.RemoveAt(index);
                }
            }
        }

        /// <summary>
        /// 异步订阅，返回可用于取消订阅的令牌
        /// </summary>
        /// <param name="handler">异步事件处理委托</param>
        /// <returns>订阅令牌，可用于取消订阅</returns>
        /// <exception cref="ArgumentNullException">当 handler 为 null 时抛出</exception>
        public SubscriptionToken SubscribeAsync(Func<T, Task> handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            var id = Guid.NewGuid();
            lock (_lock)
            {
                _asyncHandlers.Add((id, handler));
            }

            return new SubscriptionToken(() => RemoveHandler(id, _asyncHandlers));
        }

        /// <summary>
        /// 使用令牌取消异步订阅
        /// </summary>
        /// <param name="token">订阅令牌</param>
        public void UnsubscribeAsync(SubscriptionToken token)
        {
            token?.Unsubscribe();
        }

        /// <summary>
        /// 使用委托取消异步订阅（向后兼容，建议使用令牌模式）
        /// </summary>
        /// <param name="handler">异步事件处理委托</param>
        /// <exception cref="ArgumentNullException">当 handler 为 null 时抛出</exception>
        public void UnsubscribeAsync(Func<T, Task> handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            lock (_lock)
            {
                var index = _asyncHandlers.FindIndex(h => h.Item2 == handler);
                if (index >= 0)
                {
                    _asyncHandlers.RemoveAt(index);
                }
            }
        }

        private void RemoveHandler<U>(Guid id, List<(Guid id, U)> list)
        {
            lock (_lock)
            {
                var index = list.FindIndex(h => h.id == id);
                if (index >= 0)
                {
                    list.RemoveAt(index);
                }
            }
        }

        /// <summary>
        /// 发布事件
        /// </summary>
        /// <param name="eventData">事件数据</param>
        /// <exception cref="AggregateException">当一个或多个 handler 抛出异常时，汇总所有异常</exception>
        public void Publish(T eventData)
        {
            List<Action<T>> handlersCopy;
            lock (_lock)
            {
                handlersCopy = _handlers.ConvertAll(h => h.Item2);
            }

            var exceptions = new List<Exception>();
            foreach (var handler in handlersCopy)
            {
                try
                {
                    handler(eventData);
                }
                // 捕获所有异常以隔离事件处理器（防止一个处理器失败影响其他处理器）
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }

            if (exceptions.Count > 0)
                throw new AggregateException("一个或多个事件处理器执行失败", exceptions);
        }

        /// <summary>
        /// 异步发布事件
        /// </summary>
        /// <param name="eventData">事件数据</param>
        /// <returns>表示异步操作的 Task</returns>
        public async Task PublishAsync(T eventData)
        {
            List<Action<T>> handlersCopy;
            List<Func<T, Task>> asyncHandlersCopy;

            lock (_lock)
            {
                handlersCopy = _handlers.ConvertAll(h => h.Item2);
                asyncHandlersCopy = _asyncHandlers.ConvertAll(h => h.Item2);
            }

            var exceptions = new List<Exception>();
            var tasks = new List<Task>();
            foreach (var handler in handlersCopy)
            {
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        handler(eventData);
                    }
                    // 捕获所有异常以隔离事件处理器（并行执行时防止一个失败影响其他）
                    catch (Exception ex)
                    {
                        lock (exceptions)
                        {
                            exceptions.Add(ex);
                        }
                    }
                }));
            }

            foreach (var handler in asyncHandlersCopy)
            {
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        await handler(eventData).ConfigureAwait(false);
                    }
                    // 捕获所有异常以隔离异步事件处理器
                    catch (Exception ex)
                    {
                        lock (exceptions)
                        {
                            exceptions.Add(ex);
                        }
                    }
                }));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);

            if (exceptions.Count > 0)
                throw new AggregateException("一个或多个事件处理器执行失败", exceptions);
        }

        /// <summary>
        /// 清除所有订阅
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                _handlers.Clear();
                _asyncHandlers.Clear();
            }
        }
    }
}
