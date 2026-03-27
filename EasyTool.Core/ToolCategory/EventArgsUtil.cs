using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyTool.ToolCategory
{
    /// <summary>
    /// 弱事件订阅器
    /// 避免内存泄漏
    /// </summary>
    public class WeakEvent<TEventArgs> where TEventArgs : EventArgs
    {
        private readonly List<WeakReference> _handlers = new();
        private readonly object _lock = new();

        /// <summary>
        /// 订阅
        /// </summary>
        public void Subscribe(EventHandler<TEventArgs> handler)
        {
            lock (_lock)
            {
                _handlers.Add(new WeakReference(handler));
            }
        }

        /// <summary>
        /// 取消订阅
        /// </summary>
        public void Unsubscribe(EventHandler<TEventArgs> handler)
        {
            lock (_lock)
            {
                for (int i = _handlers.Count - 1; i >= 0; i--)
                {
                    if (_handlers[i].Target is EventHandler<TEventArgs> existing &&
                        existing == handler)
                    {
                        _handlers.RemoveAt(i);
                    }
                }
            }
        }

        /// <summary>
        /// 触发事件
        /// </summary>
        public void Raise(object sender, TEventArgs args)
        {
            List<EventHandler<TEventArgs>?> handlers;

            lock (_lock)
            {
                handlers = _handlers
                    .Where(w => w.IsAlive)
                    .Select(w => w.Target as EventHandler<TEventArgs>)
                    .ToList();
            }

            foreach (var handler in handlers)
            {
                handler?.Invoke(sender, args);
            }
        }

        /// <summary>
        /// 清理无效引用
        /// </summary>
        public void Cleanup()
        {
            lock (_lock)
            {
                for (int i = _handlers.Count - 1; i >= 0; i--)
                {
                    if (!_handlers[i].IsAlive)
                    {
                        _handlers.RemoveAt(i);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 属性变更事件参数
    /// </summary>
    public class PropertyChangedEventArgs : EventArgs
    {
        /// <summary>
        /// 属性名
        /// </summary>
        public string PropertyName { get; }

        /// <summary>
        /// 旧值
        /// </summary>
        public object? OldValue { get; }

        /// <summary>
        /// 新值
        /// </summary>
        public object? NewValue { get; }

        /// <summary>
        /// 创建属性变更事件参数
        /// </summary>
        public PropertyChangedEventArgs(string propertyName, object? oldValue, object? newValue)
        {
            PropertyName = propertyName;
            OldValue = oldValue;
            NewValue = newValue;
        }
    }

    /// <summary>
    /// 可观察对象
    /// </summary>
    public class ObservableObject
    {
        private readonly Dictionary<string, object?> _properties = new();

        /// <summary>
        /// 属性变更事件
        /// </summary>
        public event EventHandler<PropertyChangedEventArgs>? PropertyChanged;

        /// <summary>
        /// 获取属性值
        /// </summary>
        protected T? GetProperty<T>(string name, T? defaultValue = default)
        {
            return _properties.TryGetValue(name, out var value) ? (T?)value : defaultValue;
        }

        /// <summary>
        /// 设置属性值
        /// </summary>
        protected bool SetProperty<T>(string name, T? value)
        {
            var oldValue = GetProperty<T>(name);

            if (EqualityComparer<T?>.Default.Equals(oldValue, value))
                return false;

            _properties[name] = value;
            OnPropertyChanged(name, oldValue, value);
            return true;
        }

        /// <summary>
        /// 触发属性变更
        /// </summary>
        protected virtual void OnPropertyChanged(string propertyName, object? oldValue, object? newValue)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName, oldValue, newValue));
        }
    }
}
