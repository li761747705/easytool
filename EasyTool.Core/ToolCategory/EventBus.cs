using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EasyTool.ToolCategory
{
    /// <summary>
    /// 事件总线
    /// 提供发布/订阅模式的实现
    /// </summary>
    public static class EventBus
    {
        private static readonly Dictionary<Type, List<Delegate>> _handlers = new();
        private static readonly object _lock = new();

        /// <summary>
        /// 订阅事件
        /// </summary>
        public static void Subscribe<T>(Action<T> handler)
        {
            lock (_lock)
            {
                if (!_handlers.TryGetValue(typeof(T), out var handlers))
                {
                    handlers = new List<Delegate>();
                    _handlers[typeof(T)] = handlers;
                }
                handlers.Add(handler);
            }
        }

        /// <summary>
        /// 取消订阅
        /// </summary>
        public static void Unsubscribe<T>(Action<T> handler)
        {
            lock (_lock)
            {
                if (_handlers.TryGetValue(typeof(T), out var handlers))
                {
                    handlers.Remove(handler);
                }
            }
        }

        /// <summary>
        /// 发布事件
        /// </summary>
        public static void Publish<T>(T eventData)
        {
            List<Delegate>? handlersCopy;
            lock (_lock)
            {
                if (!_handlers.TryGetValue(typeof(T), out var handlers))
                    return;
                handlersCopy = new List<Delegate>(handlers);
            }

            foreach (var handler in handlersCopy)
            {
                if (handler is Action<T> typedHandler)
                {
                    typedHandler(eventData);
                }
            }
        }

        /// <summary>
        /// 异步发布事件
        /// </summary>
        public static async Task PublishAsync<T>(T eventData)
        {
            List<Delegate>? handlersCopy;
            lock (_lock)
            {
                if (!_handlers.TryGetValue(typeof(T), out var handlers))
                    return;
                handlersCopy = new List<Delegate>(handlers);
            }

            var tasks = new List<Task>();
            foreach (var handler in handlersCopy)
            {
                if (handler is Action<T> typedHandler)
                {
                    tasks.Add(Task.Run(() => typedHandler(eventData)));
                }
                else if (handler is Func<T, Task> asyncHandler)
                {
                    tasks.Add(asyncHandler(eventData));
                }
            }

            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// 订阅异步事件
        /// </summary>
        public static void SubscribeAsync<T>(Func<T, Task> handler)
        {
            lock (_lock)
            {
                if (!_handlers.TryGetValue(typeof(T), out var handlers))
                {
                    handlers = new List<Delegate>();
                    _handlers[typeof(T)] = handlers;
                }
                handlers.Add(handler);
            }
        }

        /// <summary>
        /// 清除所有订阅
        /// </summary>
        public static void Clear()
        {
            lock (_lock)
            {
                _handlers.Clear();
            }
        }

        /// <summary>
        /// 清除指定类型的订阅
        /// </summary>
        public static void Clear<T>()
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
        private readonly List<Action<T>> _handlers = new();
        private readonly List<Func<T, Task>> _asyncHandlers = new();
        private readonly object _lock = new();

        /// <summary>
        /// 获取单例实例
        /// </summary>
        public static EventBus<T> Instance => _instance;

        /// <summary>
        /// 订阅
        /// </summary>
        public void Subscribe(Action<T> handler)
        {
            lock (_lock)
            {
                _handlers.Add(handler);
            }
        }

        /// <summary>
        /// 取消订阅
        /// </summary>
        public void Unsubscribe(Action<T> handler)
        {
            lock (_lock)
            {
                _handlers.Remove(handler);
            }
        }

        /// <summary>
        /// 异步订阅
        /// </summary>
        public void SubscribeAsync(Func<T, Task> handler)
        {
            lock (_lock)
            {
                _asyncHandlers.Add(handler);
            }
        }

        /// <summary>
        /// 取消异步订阅
        /// </summary>
        public void UnsubscribeAsync(Func<T, Task> handler)
        {
            lock (_lock)
            {
                _asyncHandlers.Remove(handler);
            }
        }

        /// <summary>
        /// 发布
        /// </summary>
        public void Publish(T eventData)
        {
            List<Action<T>> handlersCopy;
            lock (_lock)
            {
                handlersCopy = new List<Action<T>>(_handlers);
            }

            foreach (var handler in handlersCopy)
            {
                handler(eventData);
            }
        }

        /// <summary>
        /// 异步发布
        /// </summary>
        public async Task PublishAsync(T eventData)
        {
            List<Action<T>> handlersCopy;
            List<Func<T, Task>> asyncHandlersCopy;

            lock (_lock)
            {
                handlersCopy = new List<Action<T>>(_handlers);
                asyncHandlersCopy = new List<Func<T, Task>>(_asyncHandlers);
            }

            var tasks = new List<Task>();
            foreach (var handler in handlersCopy)
            {
                tasks.Add(Task.Run(() => handler(eventData)));
            }

            foreach (var handler in asyncHandlersCopy)
            {
                tasks.Add(handler(eventData));
            }

            await Task.WhenAll(tasks);
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
