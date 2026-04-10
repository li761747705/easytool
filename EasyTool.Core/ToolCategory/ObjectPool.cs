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

    /// <summary>
    /// StringBuilder 对象池
    /// </summary>
    public static class StringBuilderPool
    {
        private static readonly ObjectPool<System.Text.StringBuilder> _pool = new(
            () => new System.Text.StringBuilder(1024),
            maxSize: 50,
            reset: sb => sb.Clear());

        /// <summary>
        /// 获取 StringBuilder
        /// </summary>
        public static System.Text.StringBuilder Get() => _pool.Get();

        /// <summary>
        /// 归还 StringBuilder
        /// </summary>
        public static void Return(System.Text.StringBuilder sb) => _pool.Return(sb);

        /// <summary>
        /// 使用 StringBuilder 执行操作并返回结果字符串
        /// </summary>
        public static string Use(Action<System.Text.StringBuilder> action)
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
        /// 使用 StringBuilder 执行操作
        /// </summary>
        public static TResult Use<TResult>(Func<System.Text.StringBuilder, TResult> action)
        {
            var sb = Get();
            try
            {
                return action(sb);
            }
            finally
            {
                Return(sb);
            }
        }
    }

    /// <summary>
    /// MemoryStream 对象池
    /// </summary>
    public static class MemoryStreamPool
    {
        private static readonly ObjectPool<System.IO.MemoryStream> _pool = new(
            () => new System.IO.MemoryStream(8192),
            maxSize: 20,
            reset: ms =>
            {
                ms.SetLength(0);
                ms.Position = 0;
            });

        /// <summary>
        /// 获取 MemoryStream
        /// </summary>
        public static System.IO.MemoryStream Get() => _pool.Get();

        /// <summary>
        /// 归还 MemoryStream
        /// </summary>
        public static void Return(System.IO.MemoryStream ms) => _pool.Return(ms);

        /// <summary>
        /// 使用 MemoryStream 执行操作
        /// </summary>
        public static TResult Use<TResult>(Func<System.IO.MemoryStream, TResult> action)
        {
            var ms = Get();
            try
            {
                return action(ms);
            }
            finally
            {
                Return(ms);
            }
        }

        /// <summary>
        /// 使用 MemoryStream 执行操作
        /// </summary>
        public static void Use(Action<System.IO.MemoryStream> action)
        {
            var ms = Get();
            try
            {
                action(ms);
            }
            finally
            {
                Return(ms);
            }
        }
    }

    /// <summary>
    /// 字节数组池（使用 ArrayPool）
    /// </summary>
    public static class ByteArrayPool
    {
        /// <summary>
        /// 租用字节数组
        /// </summary>
        /// <param name="minimumLength">最小长度</param>
        /// <returns>字节数组</returns>
        public static byte[] Rent(int minimumLength)
        {
            return System.Buffers.ArrayPool<byte>.Shared.Rent(minimumLength);
        }

        /// <summary>
        /// 归还字节数组
        /// </summary>
        /// <param name="array">要归还的数组</param>
        /// <param name="clearArray">是否清空数组</param>
        public static void Return(byte[] array, bool clearArray = false)
        {
            System.Buffers.ArrayPool<byte>.Shared.Return(array, clearArray);
        }

        /// <summary>
        /// 使用字节数组执行操作
        /// </summary>
        public static TResult Use<TResult>(int minimumLength, Func<byte[], TResult> action)
        {
            var array = Rent(minimumLength);
            try
            {
                return action(array);
            }
            finally
            {
                Return(array);
            }
        }

        /// <summary>
        /// 使用字节数组执行操作
        /// </summary>
        public static void Use(int minimumLength, Action<byte[]> action)
        {
            var array = Rent(minimumLength);
            try
            {
                action(array);
            }
            finally
            {
                Return(array);
            }
        }
    }

    /// <summary>
    /// 字符数组池（使用 ArrayPool）
    /// </summary>
    public static class CharArrayPool
    {
        /// <summary>
        /// 租用字符数组
        /// </summary>
        /// <param name="minimumLength">最小长度</param>
        /// <returns>字符数组</returns>
        public static char[] Rent(int minimumLength)
        {
            return System.Buffers.ArrayPool<char>.Shared.Rent(minimumLength);
        }

        /// <summary>
        /// 归还字符数组
        /// </summary>
        /// <param name="array">要归还的数组</param>
        /// <param name="clearArray">是否清空数组</param>
        public static void Return(char[] array, bool clearArray = false)
        {
            System.Buffers.ArrayPool<char>.Shared.Return(array, clearArray);
        }

        /// <summary>
        /// 使用字符数组执行操作
        /// </summary>
        public static TResult Use<TResult>(int minimumLength, Func<char[], TResult> action)
        {
            var array = Rent(minimumLength);
            try
            {
                return action(array);
            }
            finally
            {
                Return(array);
            }
        }
    }
}
