using System;

namespace EasyTool
{
    /// <summary>
    /// 时间戳处理工具类
    /// </summary>
    public static class TimestampUtil
    {
        /// <summary>
        /// 获取当前时间戳（毫秒级）
        /// [Obsolete("请直接使用 DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()")]
        /// </summary>
        /// <returns>当前时间戳（毫秒级）</returns>
        [Obsolete("请直接使用 DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()", false)]
        public static long GetCurrentTimestamp()
        {
            DateTime dt = DateTime.UtcNow;
            TimeSpan ts = dt - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return (long)ts.TotalMilliseconds;
        }

        /// <summary>
        /// 将时间戳（毫秒级）转换为 DateTime 类型
        /// [Obsolete("请直接使用 DateTimeOffset.FromUnixTimeMilliseconds(timestamp).DateTime")]
        /// </summary>
        /// <param name="timestamp">时间戳（毫秒级）</param>
        /// <returns>转换后的 DateTime 类型</returns>
        [Obsolete("请直接使用 DateTimeOffset.FromUnixTimeMilliseconds(timestamp).DateTime", false)]
        public static DateTime ConvertToDateTime(long timestamp)
        {
            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return dt.AddMilliseconds(timestamp);
        }

        /// <summary>
        /// 将 DateTime 类型转换为时间戳（毫秒级）
        /// [Obsolete("请直接使用 new DateTimeOffset(dateTime).ToUnixTimeMilliseconds()")]
        /// </summary>
        /// <param name="dateTime">DateTime 类型</param>
        /// <returns>转换后的时间戳（毫秒级）</returns>
        [Obsolete("请直接使用 new DateTimeOffset(dateTime).ToUnixTimeMilliseconds()", false)]
        public static long ConvertToTimestamp(DateTime dateTime)
        {
            TimeSpan ts = dateTime.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return (long)ts.TotalMilliseconds;
        }

        /// <summary>
        /// 获取当前时间戳（秒级）
        /// [Obsolete("请直接使用 DateTimeOffset.UtcNow.ToUnixTimeSeconds()")]
        /// </summary>
        /// <returns>当前时间戳（秒级）</returns>
        [Obsolete("请直接使用 DateTimeOffset.UtcNow.ToUnixTimeSeconds()", false)]
        public static long GetCurrentTimestampSeconds()
        {
            return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        }

        /// <summary>
        /// 将时间戳（秒级）转换为 DateTime 类型
        /// [Obsolete("请直接使用 DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime")]
        /// </summary>
        /// <param name="timestamp">时间戳（秒级）</param>
        /// <returns>转换后的 DateTime 类型</returns>
        [Obsolete("请直接使用 DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime", false)]
        public static DateTime ConvertToDateTimeSeconds(long timestamp)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(timestamp);
        }

        /// <summary>
        /// 将 DateTime 类型转换为时间戳（秒级）
        /// [Obsolete("请直接使用 new DateTimeOffset(dateTime).ToUnixTimeSeconds()")]
        /// </summary>
        /// <param name="dateTime">DateTime 类型</param>
        /// <returns>转换后的时间戳（秒级）</returns>
        [Obsolete("请直接使用 new DateTimeOffset(dateTime).ToUnixTimeSeconds()", false)]
        public static long ConvertToTimestampSeconds(DateTime dateTime)
        {
            return (long)(dateTime.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        }
    }
}
