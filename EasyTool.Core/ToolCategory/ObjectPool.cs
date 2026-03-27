using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EasyTool.ToolCategory
{
    /// <summary>
    /// 对象池
    /// 用于重用对象，减少GC压力
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    public class ObjectPool<T> where T : class
    {
        private readonly Stack<T> _pool;
        private readonly Func<T> _factory;
        private readonly Action<T>? _reset;
        private readonly int _maxSize;
        private readonly object _lock = new();

        /// <summary>
        /// 创建对象池
        /// </summary>
        /// <param name="factory">对象工厂</param>
        /// <param name="maxSize">最大池大小</param>
        /// <param name="reset">重置动作</param>
        public ObjectPool(Func<T> factory, int maxSize = 100, Action<T>? reset = null)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _maxSize = maxSize;
            _reset = reset;
            _pool = new Stack<T>();
        }

        /// <summary>
        /// 当前池中对象数量
        /// </summary>
        public int Count
        {
            get { lock (_lock) { return _pool.Count; } }
        }

        /// <summary>
        /// 从池中获取对象
        /// </summary>
        public T Get()
        {
            lock (_lock)
            {
                if (_pool.Count > 0)
                    return _pool.Pop();
            }
            return _factory();
        }

        /// <summary>
        /// 将对象归还到池中
        /// </summary>
        public void Return(T item)
        {
            if (item == null)
                return;

            _reset?.Invoke(item);

            lock (_lock)
            {
                if (_pool.Count < _maxSize)
                    _pool.Push(item);
            }
        }

        /// <summary>
        /// 使用池中对象执行操作
        /// </summary>
        public TResult Use<TResult>(Func<T, TResult> action)
        {
            var item = Get();
            try
            {
                return action(item);
            }
            finally
            {
                Return(item);
            }
        }

        /// <summary>
        /// 使用池中对象执行操作
        /// </summary>
        public void Use(Action<T> action)
        {
            var item = Get();
            try
            {
                action(item);
            }
            finally
            {
                Return(item);
            }
        }

        /// <summary>
        /// 清空池
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                _pool.Clear();
            }
        }

        /// <summary>
        /// 预热池（创建指定数量的对象）
        /// </summary>
        public void WarmUp(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var item = _factory();
                Return(item);
            }
        }
    }

    /// <summary>
    /// 对象池扩展
    /// </summary>
    public static class ObjectPoolExtensions
    {
        /// <summary>
        /// 创建对象池
        /// </summary>
        public static ObjectPool<T> CreatePool<T>(this Func<T> factory, int maxSize = 100, Action<T>? reset = null)
            where T : class
        {
            return new ObjectPool<T>(factory, maxSize, reset);
        }
    }
}
