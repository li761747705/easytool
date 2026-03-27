using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyTool.DateTimeCategory
{
    /// <summary>
    /// 时区转换工具类
    /// 提供时区转换和时区信息查询功能
    /// </summary>
    public static class TimeZoneUtil
    {
        #region 常用时区

        /// <summary>
        /// UTC时区
        /// </summary>
        public static TimeZoneInfo UtcTimeZone => TimeZoneInfo.Utc;

        /// <summary>
        /// 本地时区
        /// </summary>
        public static TimeZoneInfo LocalTimeZone => TimeZoneInfo.Local;

        /// <summary>
        /// 中国标准时间时区（UTC+8）
        /// </summary>
        public static TimeZoneInfo ChinaStandardTime => FindTimeZoneById("China Standard Time", "Asia/Shanghai", 8);

        /// <summary>
        /// 美国东部时区
        /// </summary>
        public static TimeZoneInfo USEasternTime => FindTimeZoneById("Eastern Standard Time", "America/New_York", -5);

        /// <summary>
        /// 美国太平洋时区
        /// </summary>
        public static TimeZoneInfo USPacificTime => FindTimeZoneById("Pacific Standard Time", "America/Los_Angeles", -8);

        /// <summary>
        /// 欧洲伦敦时区
        /// </summary>
        public static TimeZoneInfo LondonTime => FindTimeZoneById("GMT Standard Time", "Europe/London", 0);

        /// <summary>
        /// 日本标准时间时区
        /// </summary>
        public static TimeZoneInfo JapanStandardTime => FindTimeZoneById("Tokyo Standard Time", "Asia/Tokyo", 9);

        /// <summary>
        /// 韩国标准时间时区
        /// </summary>
        public static TimeZoneInfo KoreaStandardTime => FindTimeZoneById("Korea Standard Time", "Asia/Seoul", 9);

        /// <summary>
        /// 新加坡时区
        /// </summary>
        public static TimeZoneInfo SingaporeTime => FindTimeZoneById("Singapore Standard Time", "Asia/Singapore", 8);

        /// <summary>
        /// 澳大利亚悉尼时区
        /// </summary>
        public static TimeZoneInfo SydneyTime => FindTimeZoneById("AUS Eastern Standard Time", "Australia/Sydney", 10);

        /// <summary>
        /// 印度标准时间时区
        /// </summary>
        public static TimeZoneInfo IndiaStandardTime => FindTimeZoneById("India Standard Time", "Asia/Kolkata", 5.5);

        /// <summary>
        /// 德国柏林时区
        /// </summary>
        public static TimeZoneInfo BerlinTime => FindTimeZoneById("W. Europe Standard Time", "Europe/Berlin", 1);

        private static TimeZoneInfo FindTimeZoneById(string windowsId, string ianaId, double offsetHours)
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(windowsId);
            }
            catch
            {
                try
                {
                    return TimeZoneInfo.FindSystemTimeZoneById(ianaId);
                }
                catch
                {
                    // 创建自定义时区
                    return CreateCustomTimeZone(windowsId, offsetHours);
                }
            }
        }

        private static TimeZoneInfo CreateCustomTimeZone(string id, double offsetHours)
        {
            var offset = TimeSpan.FromHours(offsetHours);
            return TimeZoneInfo.CreateCustomTimeZone(id, offset, id, id);
        }

        #endregion

        #region 时区转换

        /// <summary>
        /// 将时间从一个时区转换到另一个时区
        /// </summary>
        /// <param name="dateTime">要转换的时间</param>
        /// <param name="sourceTimeZone">源时区</param>
        /// <param name="destinationTimeZone">目标时区</param>
        /// <returns>转换后的时间</returns>
        public static DateTime ConvertTime(DateTime dateTime, TimeZoneInfo sourceTimeZone, TimeZoneInfo destinationTimeZone)
        {
            return TimeZoneInfo.ConvertTime(dateTime, sourceTimeZone, destinationTimeZone);
        }

        /// <summary>
        /// 将时间转换为UTC时间
        /// </summary>
        /// <param name="dateTime">本地时间</param>
        /// <returns>UTC时间</returns>
        public static DateTime ToUtc(DateTime dateTime)
        {
            return dateTime.Kind switch
            {
                DateTimeKind.Utc => dateTime,
                DateTimeKind.Local => dateTime.ToUniversalTime(),
                _ => DateTime.SpecifyKind(dateTime, DateTimeKind.Local).ToUniversalTime()
            };
        }

        /// <summary>
        /// 将UTC时间转换为本地时间
        /// </summary>
        /// <param name="utcDateTime">UTC时间</param>
        /// <returns>本地时间</returns>
        public static DateTime FromUtc(DateTime utcDateTime)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, TimeZoneInfo.Local);
        }

        /// <summary>
        /// 将时间转换为中国标准时间
        /// </summary>
        /// <param name="dateTime">源时间</param>
        /// <param name="sourceTimeZone">源时区（默认本地时区）</param>
        /// <returns>中国标准时间</returns>
        public static DateTime ToChinaTime(DateTime dateTime, TimeZoneInfo? sourceTimeZone = null)
        {
            sourceTimeZone ??= TimeZoneInfo.Local;
            return ConvertTime(dateTime, sourceTimeZone, ChinaStandardTime);
        }

        /// <summary>
        /// 将时间转换为美国东部时间
        /// </summary>
        /// <param name="dateTime">源时间</param>
        /// <param name="sourceTimeZone">源时区（默认本地时区）</param>
        /// <returns>美国东部时间</returns>
        public static DateTime ToUSEasternTime(DateTime dateTime, TimeZoneInfo? sourceTimeZone = null)
        {
            sourceTimeZone ??= TimeZoneInfo.Local;
            return ConvertTime(dateTime, sourceTimeZone, USEasternTime);
        }

        /// <summary>
        /// 将时间转换为指定偏移量时区的时间
        /// </summary>
        /// <param name="dateTime">源时间</param>
        /// <param name="sourceOffset">源时区偏移量（小时）</param>
        /// <param name="targetOffset">目标时区偏移量（小时）</param>
        /// <returns>目标时区时间</returns>
        public static DateTime ConvertByOffset(DateTime dateTime, double sourceOffset, double targetOffset)
        {
            // 先转为UTC
            var utc = dateTime.AddHours(-sourceOffset);
            // 再转为目标时区
            return utc.AddHours(targetOffset);
        }

        /// <summary>
        /// 获取指定时区当前时间
        /// </summary>
        /// <param name="timeZone">时区</param>
        /// <returns>当前时间</returns>
        public static DateTime GetNow(TimeZoneInfo timeZone)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);
        }

        /// <summary>
        /// 获取中国当前时间
        /// </summary>
        /// <returns>中国当前时间</returns>
        public static DateTime GetChinaNow()
        {
            return GetNow(ChinaStandardTime);
        }

        #endregion

        #region 时区信息

        /// <summary>
        /// 获取所有时区
        /// </summary>
        /// <returns>时区列表</returns>
        public static IReadOnlyCollection<TimeZoneInfo> GetAllTimeZones()
        {
            return TimeZoneInfo.GetSystemTimeZones();
        }

        /// <summary>
        /// 根据ID获取时区
        /// </summary>
        /// <param name="id">时区ID</param>
        /// <returns>时区信息</returns>
        public static TimeZoneInfo? GetTimeZoneById(string id)
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(id);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 根据偏移量查找时区
        /// </summary>
        /// <param name="offsetHours">偏移量（小时）</param>
        /// <returns>匹配的时区列表</returns>
        public static List<TimeZoneInfo> GetTimeZonesByOffset(double offsetHours)
        {
            var offset = TimeSpan.FromHours(offsetHours);
            return TimeZoneInfo.GetSystemTimeZones()
                .Where(tz => tz.BaseUtcOffset == offset)
                .ToList();
        }

        /// <summary>
        /// 获取时区偏移量
        /// </summary>
        /// <param name="timeZone">时区</param>
        /// <returns>偏移量（小时）</returns>
        public static double GetOffsetHours(TimeZoneInfo timeZone)
        {
            return timeZone.BaseUtcOffset.TotalHours;
        }

        /// <summary>
        /// 获取时区偏移量字符串
        /// </summary>
        /// <param name="timeZone">时区</param>
        /// <returns>偏移量字符串（如+08:00）</returns>
        public static string GetOffsetString(TimeZoneInfo timeZone)
        {
            var offset = timeZone.BaseUtcOffset;
            return $"{(offset >= TimeSpan.Zero ? "+" : "")}{offset:hh\\:mm}";
        }

        /// <summary>
        /// 判断时区是否支持夏令时
        /// </summary>
        /// <param name="timeZone">时区</param>
        /// <returns>是否支持夏令时</returns>
        public static bool SupportsDaylightSavingTime(TimeZoneInfo timeZone)
        {
            return timeZone.SupportsDaylightSavingTime;
        }

        /// <summary>
        /// 判断指定时间是否处于夏令时
        /// </summary>
        /// <param name="dateTime">时间</param>
        /// <param name="timeZone">时区</param>
        /// <returns>是否处于夏令时</returns>
        public static bool IsDaylightSavingTime(DateTime dateTime, TimeZoneInfo timeZone)
        {
            return timeZone.IsDaylightSavingTime(dateTime);
        }

        /// <summary>
        /// 获取时区当前偏移量（考虑夏令时）
        /// </summary>
        /// <param name="timeZone">时区</param>
        /// <returns>当前偏移量</returns>
        public static TimeSpan GetCurrentOffset(TimeZoneInfo timeZone)
        {
            var now = DateTime.UtcNow;
            var offset = timeZone.GetUtcOffset(now);
            return offset;
        }

        #endregion

        #region DateTimeOffset

        /// <summary>
        /// 创建DateTimeOffset
        /// </summary>
        /// <param name="dateTime">本地时间</param>
        /// <param name="timeZone">时区</param>
        /// <returns>DateTimeOffset</returns>
        public static DateTimeOffset CreateDateTimeOffset(DateTime dateTime, TimeZoneInfo timeZone)
        {
            var utcTime = ConvertTime(dateTime, timeZone, TimeZoneInfo.Utc);
            return new DateTimeOffset(utcTime, TimeSpan.Zero).ToOffset(timeZone.GetUtcOffset(dateTime));
        }

        /// <summary>
        /// 将DateTimeOffset转换到指定时区
        /// </summary>
        /// <param name="dateTimeOffset">DateTimeOffset</param>
        /// <param name="timeZone">目标时区</param>
        /// <returns>转换后的DateTimeOffset</returns>
        public static DateTimeOffset ConvertTime(DateTimeOffset dateTimeOffset, TimeZoneInfo timeZone)
        {
            return TimeZoneInfo.ConvertTime(dateTimeOffset, timeZone);
        }

        #endregion

        #region 时区差异计算

        /// <summary>
        /// 计算两个时区之间的时间差
        /// </summary>
        /// <param name="timeZone1">时区1</param>
        /// <param name="timeZone2">时区2</param>
        /// <returns>时间差</returns>
        public static TimeSpan GetTimeDifference(TimeZoneInfo timeZone1, TimeZoneInfo timeZone2)
        {
            return timeZone1.BaseUtcOffset - timeZone2.BaseUtcOffset;
        }

        /// <summary>
        /// 计算两个时区之间的小时差
        /// </summary>
        /// <param name="timeZone1">时区1</param>
        /// <param name="timeZone2">时区2</param>
        /// <returns>小时差</returns>
        public static double GetHoursDifference(TimeZoneInfo timeZone1, TimeZoneInfo timeZone2)
        {
            return GetTimeDifference(timeZone1, timeZone2).TotalHours;
        }

        #endregion

        #region 时区查找

        /// <summary>
        /// 根据名称模糊查找时区
        /// </summary>
        /// <param name="name">时区名称</param>
        /// <returns>匹配的时区列表</returns>
        public static List<TimeZoneInfo> FindTimeZonesByName(string name)
        {
            return TimeZoneInfo.GetSystemTimeZones()
                .Where(tz => tz.DisplayName.Contains(name, StringComparison.OrdinalIgnoreCase) ||
                             tz.Id.Contains(name, StringComparison.OrdinalIgnoreCase) ||
                             tz.StandardName.Contains(name, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        /// <summary>
        /// 获取UTC+N时区列表
        /// </summary>
        /// <param name="offset">UTC偏移量（如8表示UTC+8）</param>
        /// <returns>时区列表</returns>
        public static List<TimeZoneInfo> GetUtcPlusTimeZones(int offset)
        {
            return TimeZoneInfo.GetSystemTimeZones()
                .Where(tz => tz.BaseUtcOffset == TimeSpan.FromHours(offset))
                .ToList();
        }

        #endregion

        #region 格式化

        /// <summary>
        /// 格式化时区信息
        /// </summary>
        /// <param name="timeZone">时区</param>
        /// <returns>格式化字符串</returns>
        public static string FormatTimeZone(TimeZoneInfo timeZone)
        {
            return $"{timeZone.Id} ({GetOffsetString(timeZone)}) {timeZone.DisplayName}";
        }

        /// <summary>
        /// 格式化时间为带时区的字符串
        /// </summary>
        /// <param name="dateTime">时间</param>
        /// <param name="timeZone">时区</param>
        /// <param name="format">时间格式</param>
        /// <returns>格式化字符串</returns>
        public static string FormatDateTimeWithZone(DateTime dateTime, TimeZoneInfo timeZone, string format = "yyyy-MM-dd HH:mm:ss")
        {
            var offset = GetOffsetString(timeZone);
            return $"{dateTime.ToString(format)} (UTC{offset})";
        }

        #endregion

        #region 常用城市时区

        /// <summary>
        /// 获取常用城市时区映射
        /// </summary>
        public static Dictionary<string, TimeZoneInfo> GetCommonCityTimeZones()
        {
            return new Dictionary<string, TimeZoneInfo>(StringComparer.OrdinalIgnoreCase)
            {
                { "北京", ChinaStandardTime },
                { "上海", ChinaStandardTime },
                { "香港", FindTimeZoneById("China Standard Time", "Asia/Hong_Kong", 8) },
                { "台北", FindTimeZoneById("Taipei Standard Time", "Asia/Taipei", 8) },
                { "东京", JapanStandardTime },
                { "首尔", KoreaStandardTime },
                { "新加坡", SingaporeTime },
                { "悉尼", SydneyTime },
                { "伦敦", LondonTime },
                { "巴黎", FindTimeZoneById("Romance Standard Time", "Europe/Paris", 1) },
                { "柏林", BerlinTime },
                { "纽约", USEasternTime },
                { "洛杉矶", USPacificTime },
                { "芝加哥", FindTimeZoneById("Central Standard Time", "America/Chicago", -6) },
                { "多伦多", FindTimeZoneById("Eastern Standard Time", "America/Toronto", -5) },
                { "温哥华", FindTimeZoneById("Pacific Standard Time", "America/Vancouver", -8) },
                { "迪拜", FindTimeZoneById("Arabian Standard Time", "Asia/Dubai", 4) },
                { "孟买", IndiaStandardTime },
                { "莫斯科", FindTimeZoneById("Russian Standard Time", "Europe/Moscow", 3) }
            };
        }

        /// <summary>
        /// 根据城市名获取时区
        /// </summary>
        /// <param name="cityName">城市名</param>
        /// <returns>时区信息</returns>
        public static TimeZoneInfo? GetTimeZoneByCity(string cityName)
        {
            var cityTimeZones = GetCommonCityTimeZones();
            return cityTimeZones.TryGetValue(cityName, out var timeZone) ? timeZone : null;
        }

        #endregion
    }
}
