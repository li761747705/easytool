using System;

namespace EasyTool.CacheCategory
{
    /// <summary>
    /// 缓存选项
    /// </summary>
    public class CacheOptions
    {
        /// <summary>
        /// 绝对过期时间
        /// </summary>
        public DateTime? AbsoluteExpiration { get; set; }

        /// <summary>
        /// 相对过期时间（从现在开始）
        /// </summary>
        public TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }

        /// <summary>
        /// 滑动过期时间
        /// </summary>
        public TimeSpan? SlidingExpiration { get; set; }

        /// <summary>
        /// 缓存优先级
        /// </summary>
        public CachePriority Priority { get; set; } = CachePriority.Normal;

        /// <summary>
        /// 缓存键前缀
        /// </summary>
        public string? KeyPrefix { get; set; }

        /// <summary>
        /// 是否启用压缩
        /// </summary>
        public bool EnableCompression { get; set; }

        /// <summary>
        /// 压缩阈值（字节）
        /// </summary>
        public int CompressionThreshold { get; set; } = 1024;

        /// <summary>
        /// 创建相对过期选项
        /// </summary>
        /// <param name="expiration">过期时间</param>
        /// <returns>缓存选项</returns>
        public static CacheOptions FromExpiration(TimeSpan expiration)
        {
            return new CacheOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            };
        }

        /// <summary>
        /// 创建滑动过期选项
        /// </summary>
        /// <param name="slidingExpiration">滑动过期时间</param>
        /// <returns>缓存选项</returns>
        public static CacheOptions FromSlidingExpiration(TimeSpan slidingExpiration)
        {
            return new CacheOptions
            {
                SlidingExpiration = slidingExpiration
            };
        }

        /// <summary>
        /// 创建绝对过期选项
        /// </summary>
        /// <param name="absoluteExpiration">绝对过期时间</param>
        /// <returns>缓存选项</returns>
        public static CacheOptions FromAbsoluteExpiration(DateTime absoluteExpiration)
        {
            return new CacheOptions
            {
                AbsoluteExpiration = absoluteExpiration
            };
        }
    }

    /// <summary>
    /// 缓存优先级
    /// </summary>
    public enum CachePriority
    {
        /// <summary>
        /// 低优先级
        /// </summary>
        Low = 0,

        /// <summary>
        /// 普通优先级
        /// </summary>
        Normal = 1,

        /// <summary>
        /// 高优先级
        /// </summary>
        High = 2,

        /// <summary>
        /// 永不移除
        /// </summary>
        NeverRemove = 3
    }
}
