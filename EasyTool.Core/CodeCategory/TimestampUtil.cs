using System;
using System.Collections.Generic;
using System.Text;

namespace EasyTool.CodeCategory
{
    /// <summary>
    /// 时间戳工具类
    /// 提供时间戳的生成、转换、格式化等功能
    /// 支持10位秒级和13位毫秒级时间戳
    /// </summary>
    public static class TimestampUtil
    {
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        #region 获取时间戳

        /// <summary>
        /// 获取当前时间的秒级时间戳（10位）
        /// </summary>
        /// <returns>秒级时间戳</returns>
        public static long Now()
        {
            return (long)(DateTime.UtcNow - Epoch).TotalSeconds;
        }

        /// <summary>
        /// 获取当前时间的毫秒级时间戳（13位）
        /// </summary>
        /// <returns>毫秒级时间戳</returns>
        public static long NowMs()
        {
            return (long)(DateTime.UtcNow - Epoch).TotalMilliseconds;
        }

        /// <summary>
        /// 获取当前时间的微秒级时间戳（16位）
        /// </summary>
        /// <returns>微秒级时间戳</returns>
        public static long NowUs()
        {
            var ts = DateTime.UtcNow - Epoch;
            return (long)(ts.TotalMilliseconds * 1000);
        }

        /// <summary>
        /// 获取当前时间的纳秒级时间戳（19位）
        /// </summary>
        /// <returns>纳秒级时间戳</returns>
        public static long NowNs()
        {
            var ts = DateTime.UtcNow - Epoch;
            return (long)(ts.TotalMilliseconds * 1000000);
        }

        /// <summary>
        /// 获取当前时间戳字符串
        /// </summary>
        /// <param name="precision">精度：s(秒), ms(毫秒), us(微秒), ns(纳秒)</param>
        /// <returns>时间戳字符串</returns>
        public static string NowString(string precision = "ms")
        {
            return precision.ToLowerInvariant() switch
            {
                "s" => Now().ToString(),
                "ms" => NowMs().ToString(),
                "us" => NowUs().ToString(),
                "ns" => NowNs().ToString(),
                _ => NowMs().ToString()
            };
        }

        #endregion

        #region DateTime 转时间戳

        /// <summary>
        /// 将 DateTime 转换为秒级时间戳
        /// </summary>
        /// <param name="dateTime">日期时间</param>
        /// <returns>秒级时间戳</returns>
        public static long ToTimestamp(DateTime dateTime)
        {
            return (long)(dateTime.ToUniversalTime() - Epoch).TotalSeconds;
        }

        /// <summary>
        /// 将 DateTime 转换为毫秒级时间戳
        /// </summary>
        /// <param name="dateTime">日期时间</param>
        /// <returns>毫秒级时间戳</returns>
        public static long ToTimestampMs(DateTime dateTime)
        {
            return (long)(dateTime.ToUniversalTime() - Epoch).TotalMilliseconds;
        }

        /// <summary>
        /// 将 DateTime 转换为指定精度的时间戳
        /// </summary>
        /// <param name="dateTime">日期时间</param>
        /// <param name="precision">精度：s, ms, us, ns</param>
        /// <returns>时间戳</returns>
        public static long ToTimestamp(DateTime dateTime, string precision)
        {
            var ts = dateTime.ToUniversalTime() - Epoch;
            return precision.ToLowerInvariant() switch
            {
                "s" => (long)ts.TotalSeconds,
                "ms" => (long)ts.TotalMilliseconds,
                "us" => (long)(ts.TotalMilliseconds * 1000),
                "ns" => (long)(ts.TotalMilliseconds * 1000000),
                _ => (long)ts.TotalMilliseconds
            };
        }

        #endregion

        #region 时间戳转 DateTime

        /// <summary>
        /// 将时间戳转换为 DateTime（自动识别精度）
        /// </summary>
        /// <param name="timestamp">时间戳</param>
        /// <returns>DateTime</returns>
        public static DateTime FromTimestamp(long timestamp)
        {
            // 自动判断精度
            if (timestamp > 1000000000000L)
            {
                // 13位毫秒级
                if (timestamp > 10000000000000L)
                {
                    // 16位微秒级
                    if (timestamp > 100000000000000L)
                    {
                        // 19位纳秒级
                        return Epoch.AddTicks(timestamp / 100);
                    }
                    return Epoch.AddTicks(timestamp / 10);
                }
                return FromTimestampMs(timestamp);
            }
            else
            {
                // 10位秒级
                return FromTimestampSeconds(timestamp);
            }
        }

        /// <summary>
        /// 将秒级时间戳转换为 DateTime
        /// </summary>
        /// <param name="timestamp">秒级时间戳</param>
        /// <returns>DateTime</returns>
        public static DateTime FromTimestampSeconds(long timestamp)
        {
            return Epoch.AddSeconds(timestamp);
        }

        /// <summary>
        /// 将毫秒级时间戳转换为 DateTime
        /// </summary>
        /// <param name="timestamp">毫秒级时间戳</param>
        /// <returns>DateTime</returns>
        public static DateTime FromTimestampMs(long timestamp)
        {
            return Epoch.AddMilliseconds(timestamp);
        }

        /// <summary>
        /// 将字符串时间戳转换为 DateTime
        /// </summary>
        /// <param name="timestamp">时间戳字符串</param>
        /// <returns>DateTime</returns>
        public static DateTime FromString(string timestamp)
        {
            if (string.IsNullOrEmpty(timestamp))
                throw new ArgumentException("Timestamp cannot be null or empty", nameof(timestamp));

            if (long.TryParse(timestamp, out long ts))
            {
                return FromTimestamp(ts);
            }

            throw new ArgumentException("Invalid timestamp format", nameof(timestamp));
        }

        #endregion

        #region 格式化

        /// <summary>
        /// 格式化当前时间戳
        /// </summary>
        /// <param name="format">日期格式（默认 yyyy-MM-dd HH:mm:ss）</param>
        /// <returns>格式化的日期字符串</returns>
        public static string Format(string format = "yyyy-MM-dd HH:mm:ss")
        {
            return DateTime.UtcNow.ToString(format);
        }

        /// <summary>
        /// 格式化时间戳
        /// </summary>
        /// <param name="timestamp">时间戳</param>
        /// <param name="format">日期格式</param>
        /// <returns>格式化的日期字符串</returns>
        public static string Format(long timestamp, string format = "yyyy-MM-dd HH:mm:ss")
        {
            return FromTimestamp(timestamp).ToString(format);
        }

        /// <summary>
        /// 格式化为 ISO 8601 格式
        /// </summary>
        /// <param name="timestamp">时间戳</param>
        /// <returns>ISO 8601 格式字符串</returns>
        public static string ToIso8601(long timestamp)
        {
            return FromTimestamp(timestamp).ToString("o");
        }

        /// <summary>
        /// 从 ISO 8601 格式解析
        /// </summary>
        /// <param name="iso8601">ISO 8601 格式字符串</param>
        /// <returns>时间戳（毫秒）</returns>
        public static long FromIso8601(string iso8601)
        {
            if (DateTime.TryParse(iso8601, out DateTime dt))
            {
                return ToTimestampMs(dt);
            }
            throw new ArgumentException("Invalid ISO 8601 format", nameof(iso8601));
        }

        #endregion

        #region 时间计算

        /// <summary>
        /// 计算两个时间戳之间的时间差
        /// </summary>
        /// <param name="start">开始时间戳</param>
        /// <param name="end">结束时间戳</param>
        /// <returns>时间差</returns>
        public static TimeSpan Diff(long start, long end)
        {
            var startTime = FromTimestamp(start);
            var endTime = FromTimestamp(end);
            return endTime - startTime;
        }

        /// <summary>
        /// 添加秒数
        /// </summary>
        /// <param name="timestamp">时间戳（秒）</param>
        /// <param name="seconds">秒数</param>
        /// <returns>新的时间戳</returns>
        public static long AddSeconds(long timestamp, int seconds)
        {
            return timestamp + seconds;
        }

        /// <summary>
        /// 添加分钟
        /// </summary>
        /// <param name="timestamp">时间戳（秒）</param>
        /// <param name="minutes">分钟数</param>
        /// <returns>新的时间戳</returns>
        public static long AddMinutes(long timestamp, int minutes)
        {
            return timestamp + minutes * 60;
        }

        /// <summary>
        /// 添加小时
        /// </summary>
        /// <param name="timestamp">时间戳（秒）</param>
        /// <param name="hours">小时数</param>
        /// <returns>新的时间戳</returns>
        public static long AddHours(long timestamp, int hours)
        {
            return timestamp + hours * 3600;
        }

        /// <summary>
        /// 添加天数
        /// </summary>
        /// <param name="timestamp">时间戳（秒）</param>
        /// <param name="days">天数</param>
        /// <returns>新的时间戳</returns>
        public static long AddDays(long timestamp, int days)
        {
            return timestamp + days * 86400;
        }

        /// <summary>
        /// 获取今天开始时间戳（00:00:00）
        /// </summary>
        /// <returns>秒级时间戳</returns>
        public static long TodayStart()
        {
            var today = DateTime.UtcNow.Date;
            return ToTimestamp(today);
        }

        /// <summary>
        /// 获取今天结束时间戳（23:59:59）
        /// </summary>
        /// <returns>秒级时间戳</returns>
        public static long TodayEnd()
        {
            var today = DateTime.UtcNow.Date.AddDays(1).AddSeconds(-1);
            return ToTimestamp(today);
        }

        /// <summary>
        /// 获取本周开始时间戳
        /// </summary>
        /// <returns>秒级时间戳</returns>
        public static long WeekStart()
        {
            var today = DateTime.UtcNow.Date;
            var diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
            var weekStart = today.AddDays(-diff);
            return ToTimestamp(weekStart);
        }

        /// <summary>
        /// 获取本月开始时间戳
        /// </summary>
        /// <returns>秒级时间戳</returns>
        public static long MonthStart()
        {
            var today = DateTime.UtcNow;
            var monthStart = new DateTime(today.Year, today.Month, 1);
            return ToTimestamp(monthStart);
        }

        /// <summary>
        /// 获取本年开始时间戳
        /// </summary>
        /// <returns>秒级时间戳</returns>
        public static long YearStart()
        {
            var today = DateTime.UtcNow;
            var yearStart = new DateTime(today.Year, 1, 1);
            return ToTimestamp(yearStart);
        }

        #endregion

        #region 验证和比较

        /// <summary>
        /// 验证时间戳是否有效
        /// </summary>
        /// <param name="timestamp">时间戳</param>
        /// <returns>是否有效</returns>
        public static bool IsValid(long timestamp)
        {
            try
            {
                var dt = FromTimestamp(timestamp);
                return dt.Year >= 1970 && dt.Year <= 2100;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 验证时间戳字符串是否有效
        /// </summary>
        /// <param name="timestamp">时间戳字符串</param>
        /// <returns>是否有效</returns>
        public static bool IsValid(string timestamp)
        {
            if (string.IsNullOrEmpty(timestamp))
                return false;

            if (long.TryParse(timestamp, out long ts))
            {
                return IsValid(ts);
            }

            return false;
        }

        /// <summary>
        /// 比较两个时间戳
        /// </summary>
        /// <param name="ts1">时间戳1</param>
        /// <param name="ts2">时间戳2</param>
        /// <returns>-1: ts1&lt;ts2, 0: 相等, 1: ts1&gt;ts2</returns>
        public static int Compare(long ts1, long ts2)
        {
            return ts1.CompareTo(ts2);
        }

        /// <summary>
        /// 判断时间戳是否在指定范围内
        /// </summary>
        /// <param name="timestamp">时间戳</param>
        /// <param name="start">开始时间戳</param>
        /// <param name="end">结束时间戳</param>
        /// <returns>是否在范围内</returns>
        public static bool IsBetween(long timestamp, long start, long end)
        {
            return timestamp >= start && timestamp <= end;
        }

        /// <summary>
        /// 判断是否是今天
        /// </summary>
        /// <param name="timestamp">时间戳</param>
        /// <returns>是否是今天</returns>
        public static bool IsToday(long timestamp)
        {
            var dt = FromTimestamp(timestamp);
            var today = DateTime.UtcNow.Date;
            return dt.Date == today;
        }

        /// <summary>
        /// 判断是否是昨天
        /// </summary>
        /// <param name="timestamp">时间戳</param>
        /// <returns>是否是昨天</returns>
        public static bool IsYesterday(long timestamp)
        {
            var dt = FromTimestamp(timestamp);
            var yesterday = DateTime.UtcNow.Date.AddDays(-1);
            return dt.Date == yesterday;
        }

        /// <summary>
        /// 判断是否是明天
        /// </summary>
        /// <param name="timestamp">时间戳</param>
        /// <returns>是否是明天</returns>
        public static bool IsTomorrow(long timestamp)
        {
            var dt = FromTimestamp(timestamp);
            var tomorrow = DateTime.UtcNow.Date.AddDays(1);
            return dt.Date == tomorrow;
        }

        #endregion

        #region 批量转换

        /// <summary>
        /// 批量将 DateTime 转换为时间戳
        /// </summary>
        /// <param name="dateTimes">日期时间数组</param>
        /// <param name="milliseconds">是否使用毫秒精度</param>
        /// <returns>时间戳数组</returns>
        public static long[] BatchToTimestamp(DateTime[] dateTimes, bool milliseconds = false)
        {
            var result = new long[dateTimes.Length];
            for (int i = 0; i < dateTimes.Length; i++)
            {
                result[i] = milliseconds ? ToTimestampMs(dateTimes[i]) : ToTimestamp(dateTimes[i]);
            }
            return result;
        }

        /// <summary>
        /// 批量将时间戳转换为 DateTime
        /// </summary>
        /// <param name="timestamps">时间戳数组</param>
        /// <returns>DateTime 数组</returns>
        public static DateTime[] BatchFromTimestamp(long[] timestamps)
        {
            var result = new DateTime[timestamps.Length];
            for (int i = 0; i < timestamps.Length; i++)
            {
                result[i] = FromTimestamp(timestamps[i]);
            }
            return result;
        }

        #endregion

        #region 友好显示

        /// <summary>
        /// 获取友好的时间显示（如：刚刚、5分钟前、昨天等）
        /// </summary>
        /// <param name="timestamp">时间戳</param>
        /// <returns>友好显示</returns>
        public static string Friendly(long timestamp)
        {
            var dt = FromTimestamp(timestamp);
            var now = DateTime.UtcNow;
            var diff = now - dt;

            if (diff.TotalSeconds < 60)
            {
                return "刚刚";
            }
            else if (diff.TotalMinutes < 60)
            {
                return $"{(int)diff.TotalMinutes}分钟前";
            }
            else if (diff.TotalHours < 24)
            {
                return $"{(int)diff.TotalHours}小时前";
            }
            else if (diff.TotalDays < 2)
            {
                return "昨天";
            }
            else if (diff.TotalDays < 7)
            {
                return $"{(int)diff.TotalDays}天前";
            }
            else if (diff.TotalDays < 30)
            {
                return $"{(int)(diff.TotalDays / 7)}周前";
            }
            else if (diff.TotalDays < 365)
            {
                return $"{(int)(diff.TotalDays / 30)}个月前";
            }
            else
            {
                return $"{(int)(diff.TotalDays / 365)}年前";
            }
        }

        /// <summary>
        /// 获取剩余时间的友好显示
        /// </summary>
        /// <param name="timestamp">目标时间戳</param>
        /// <returns>友好显示</returns>
        public static string FriendlyRemaining(long timestamp)
        {
            var dt = FromTimestamp(timestamp);
            var now = DateTime.UtcNow;
            var diff = dt - now;

            if (diff.TotalSeconds <= 0)
            {
                return "已过期";
            }
            else if (diff.TotalSeconds < 60)
            {
                return $"{(int)diff.TotalSeconds}秒后";
            }
            else if (diff.TotalMinutes < 60)
            {
                return $"{(int)diff.TotalMinutes}分钟后";
            }
            else if (diff.TotalHours < 24)
            {
                return $"{(int)diff.TotalHours}小时后";
            }
            else if (diff.TotalDays < 7)
            {
                return $"{(int)diff.TotalDays}天后";
            }
            else if (diff.TotalDays < 30)
            {
                return $"{(int)(diff.TotalDays / 7)}周后";
            }
            else if (diff.TotalDays < 365)
            {
                return $"{(int)(diff.TotalDays / 30)}个月后";
            }
            else
            {
                return $"{(int)(diff.TotalDays / 365)}年后";
            }
        }

        #endregion
    }
}
