using System;
using System.Collections.Generic;

namespace EasyTool.ToolCategory
{
    /// <summary>
    /// 单例模式工具类
    /// </summary>
    public static class Singleton
    {
        /// <summary>
        /// 获取单例实例
        /// </summary>
        /// <typeparam name="T">类型参数，必须为引用类型且有无参构造函数</typeparam>
        /// <returns>单例实例</returns>
        public static T GetInstance<T>() where T : class, new()
        {
            return Singleton<T>.Instance;
        }

        /// <summary>
        /// 获取单例实例（带初始化参数）
        /// </summary>
        /// <typeparam name="T">类型参数，必须为引用类型</typeparam>
        /// <param name="factory">用于创建实例的工厂函数</param>
        /// <returns>单例实例</returns>
        public static T GetInstance<T>(Func<T> factory) where T : class
        {
            return Singleton<T>.GetInstance(factory);
        }
    }

    /// <summary>
    /// 泛型单例
    /// </summary>
    public static class Singleton<T> where T : class
    {
        private static readonly Lazy<T> _instance = new(() =>
        {
            var type = typeof(T);
            var constructor = type.GetConstructor(Type.EmptyTypes);
            if (constructor == null)
                throw new InvalidOperationException($"类型 {type.Name} 必须有公共无参构造函数");
            return (T)constructor.Invoke(null);
        });

        private static volatile T? _customInstance;
        private static readonly object _lock = new();

        /// <summary>
        /// 单例实例
        /// </summary>
        public static T Instance => _customInstance ?? _instance.Value;

        /// <summary>
        /// 获取实例（使用自定义工厂）
        /// </summary>
        /// <param name="factory">用于创建实例的工厂函数</param>
        /// <returns>单例实例</returns>
        public static T GetInstance(Func<T> factory)
        {
            if (_customInstance != null)
                return _customInstance;

            lock (_lock)
            {
                if (_customInstance != null)
                    return _customInstance;

                _customInstance = factory();
                return _customInstance;
            }
        }

        /// <summary>
        /// 重置实例
        /// </summary>
        public static void Reset()
        {
            lock (_lock)
            {
                _customInstance = null;
            }
        }
    }

    /// <summary>
    /// 单例基类
    /// </summary>
    /// <typeparam name="T">派生类类型</typeparam>
    public abstract class SingletonBase<T> where T : SingletonBase<T>
    {
        private static readonly Lazy<T> _instance = new(() =>
        {
            var type = typeof(T);
            var constructor = type.GetConstructor(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public, null, Type.EmptyTypes, null);
            if (constructor == null)
                throw new InvalidOperationException($"类型 {type.Name} 必须有公共或受保护的无参构造函数");
            return (T)constructor.Invoke(null);
        });

        /// <summary>
        /// 单例实例
        /// </summary>
        public static T Instance => _instance.Value;

        /// <summary>
        /// 受保护的构造函数
        /// </summary>
        protected SingletonBase() { }
    }
}
