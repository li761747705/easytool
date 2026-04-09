using System;
using System.Threading;

namespace EasyTool
{
    /// <summary>
    /// 线程安全的随机数生成器
    /// 提供跨 .NET Standard 和 .NET 5+ 的统一线程安全随机数访问
    /// </summary>
    internal static class ThreadSafeRandom
    {
#if NET6_0_OR_GREATER
        /// <summary>
        /// 获取线程安全的随机数生成器
        /// </summary>
        public static Random Instance => Random.Shared;
#else
        private static readonly ThreadLocal<Random> _threadLocalRandom = new(() =>
            new Random(Guid.NewGuid().GetHashCode()));

        /// <summary>
        /// 获取线程安全的随机数生成器
        /// </summary>
        public static Random Instance => _threadLocalRandom.Value!;
#endif
    }
}