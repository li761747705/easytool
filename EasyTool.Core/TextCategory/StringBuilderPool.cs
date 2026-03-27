using System;
using System.Collections.Generic;
using System.Text;

namespace EasyTool.TextCategory
{
    /// <summary>
    /// 字符串构建器池
    /// </summary>
    public class StringBuilderPool
    {
        private readonly Stack<StringBuilder> _pool;
        private readonly int _maxCapacity;
        private readonly int _defaultCapacity;
        private readonly object _lock = new();

        /// <summary>
        /// 默认实例
        /// </summary>
        public static StringBuilderPool Default { get; } = new();

        /// <summary>
        /// 池中可用数量
        /// </summary>
        public int AvailableCount
        {
            get
            {
                lock (_lock)
                {
                    return _pool.Count;
                }
            }
        }

        /// <summary>
        /// 创建字符串构建器池
        /// </summary>
        /// <param name="maxCapacity">最大池容量</param>
        /// <param name="defaultCapacity">默认StringBuilder容量</param>
        /// <param name="preallocate">预分配数量</param>
        public StringBuilderPool(int maxCapacity = 50, int defaultCapacity = 256, int preallocate = 5)
        {
            _maxCapacity = maxCapacity;
            _defaultCapacity = defaultCapacity;
            _pool = new Stack<StringBuilder>(maxCapacity);

            for (int i = 0; i < preallocate && i < maxCapacity; i++)
            {
                _pool.Push(new StringBuilder(defaultCapacity));
            }
        }

        /// <summary>
        /// 获取StringBuilder
        /// </summary>
        public StringBuilder Get()
        {
            lock (_lock)
            {
                if (_pool.Count > 0)
                {
                    return _pool.Pop();
                }
            }
            return new StringBuilder(_defaultCapacity);
        }

        /// <summary>
        /// 归还StringBuilder
        /// </summary>
        public void Return(StringBuilder sb)
        {
            if (sb == null) return;

            // 清空内容
            sb.Clear();

            // 如果容量过大，不归还
            if (sb.Capacity > _defaultCapacity * 4)
                return;

            lock (_lock)
            {
                if (_pool.Count < _maxCapacity)
                {
                    _pool.Push(sb);
                }
            }
        }

        /// <summary>
        /// 使用StringBuilder执行操作
        /// </summary>
        public string Execute(Action<StringBuilder> action)
        {
            var sb = Get();
            try
            {
                action(sb);
                return sb.ToString();
            }
            finally
            {
                Return(sb);
            }
        }

        /// <summary>
        /// 使用StringBuilder执行操作并返回结果
        /// </summary>
        public TResult Execute<TResult>(Func<StringBuilder, TResult> func)
        {
            var sb = Get();
            try
            {
                return func(sb);
            }
            finally
            {
                Return(sb);
            }
        }

        /// <summary>
        /// 连接字符串
        /// </summary>
        public static string Concat(IEnumerable<string> values, string separator = "")
        {
            return Default.Execute(sb =>
            {
                var first = true;
                foreach (var value in values)
                {
                    if (!first && !string.IsNullOrEmpty(separator))
                        sb.Append(separator);
                    sb.Append(value);
                    first = false;
                }
            });
        }

        /// <summary>
        /// 连接字符串
        /// </summary>
        public static string Concat<T>(IEnumerable<T> values, string separator = "", Func<T, string>? selector = null)
        {
            return Default.Execute(sb =>
            {
                var first = true;
                foreach (var value in values)
                {
                    if (!first && !string.IsNullOrEmpty(separator))
                        sb.Append(separator);
                    sb.Append(selector != null ? selector(value) : value?.ToString());
                    first = false;
                }
            });
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
    }
}