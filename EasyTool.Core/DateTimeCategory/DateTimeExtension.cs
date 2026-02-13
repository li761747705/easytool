using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace EasyTool.Extension
{
    /// <summary>
    /// 提供各种日期操作和计算的工具类。
    /// </summary>
    public static class DateTimeExtension
    {
        /// <summary>
        /// 获取指定日期所在周的第一天的日期。
        /// </summary>
        /// <param name="date">指定日期。</param>
        /// <returns>指定日期所在周的第一天的日期。</returns>
        [Obsolete("请直接使用 DateTimeUtil.GetFirstDayOfWeek(date)")]
        public static DateTime GetFirstDayOfWeek(this DateTime date) => DateTimeUtil.GetFirstDayOfWeek(date);

        /// <summary>
        /// 获取指定日期所在月份的第一天的日期。
        /// </summary>
        /// <param name="date">指定日期。</param>
        /// <returns>指定日期所在月份的第一天的日期。</returns>
        [Obsolete("请直接使用 DateTimeUtil.GetFirstDayOfMonth(date)")]
        public static DateTime GetFirstDayOfMonth(this DateTime date) => DateTimeUtil.GetFirstDayOfMonth(date);


        /// <summary>
        /// 获取指定日期所在季度的第一天的日期。
        /// </summary>
        /// <param name="date">指定日期。</param>
        /// <returns>指定日期所在季度的第一天的日期。</returns>
        [Obsolete("请直接使用 DateTimeUtil.GetFirstDayOfQuarter(date)")]
        public static DateTime GetFirstDayOfQuarter(this DateTime date) => DateTimeUtil.GetFirstDayOfQuarter(date);

        /// <summary>
        /// 获取指定日期所在年份的第一天的日期。
        /// </summary>
        /// <param name="date">指定日期。</param>
        /// <returns>指定日期所在年份的第一天的日期。</returns>
        [Obsolete("请直接使用 DateTimeUtil.GetFirstDayOfYear(date)")]
        public static DateTime GetFirstDayOfYear(this DateTime date) => DateTimeUtil.GetFirstDayOfYear(date);

        /// <summary>
        /// 计算指定日期和当前日期之间的天数差。
        /// </summary>
        /// <param name="date">指定日期。</param>
        /// <returns>指定日期和当前日期之间的天数差。</returns>
        [Obsolete("请直接使用 DateTimeUtil.GetDaysBetween(date)")]
        public static int GetDaysBetween(this DateTime date) => DateTimeUtil.GetDaysBetween(date);

        /// <summary>
        /// 计算两个日期之间的天数差。
        /// </summary>
        /// <param name="date1">第一个日期。</param>
        /// <param name="date2">第二个日期。</param>
        /// <returns>两个日期之间的天数差。</returns>
        [Obsolete("请直接使用 DateTimeUtil.GetDaysBetween(date1, date2)")]
        public static int GetDaysBetween(this DateTime date1, DateTime date2) => DateTimeUtil.GetDaysBetween(date1, date2);

        /// <summary>
        /// 计算指定日期和当前日期之间的工作日数差。
        /// </summary>
        /// <param name="date">指定日期。</param>
        /// <returns>指定日期和当前日期之间的工作日数差。</returns>
        [Obsolete("请直接使用 DateTimeUtil.GetWorkDaysBetween(date)")]
        public static int GetWorkDaysBetween(this DateTime date) => DateTimeUtil.GetWorkDaysBetween(date);

        /// <summary>
        /// 计算两个日期之间的工作日数差。
        /// </summary>
        /// <param name="date1">第一个日期。</param>
        /// <param name="date2">第二个日期。</param>
        /// <returns>两个日期之间的工作日数差。</returns>
        [Obsolete("请直接使用 DateTimeUtil.GetWorkDaysBetween(date1, date2)")]
        public static int GetWorkDaysBetween(this DateTime date1, DateTime date2) => DateTimeUtil.GetWorkDaysBetween(date1, date2);

        /// <summary>
        /// 判断指定日期是否是工作日。
        /// </summary>
        /// <param name="date">指定日期。</param>
        /// <returns>如果是工作日，则返回 true；否则返回 false。</returns>
        [Obsolete("请直接使用 DateTimeUtil.IsWorkDay(date)")]
        public static bool IsWorkDay(this DateTime date) => DateTimeUtil.IsWorkDay(date);

        /// <summary>
        /// 获取指定日期所在周的所有日期。
        /// </summary>
        /// <param name="date">指定日期。</param>
        /// <returns>指定日期所在周的所有日期。</returns>
        [Obsolete("请直接使用 DateTimeUtil.GetWeekDays(date)")]
        public static List<DateTime> GetWeekDays(this DateTime date) => DateTimeUtil.GetWeekDays(date);

        /// <summary>
        /// 获取指定日期所在月份的所有日期。
        /// </summary>
        /// <param name="date">指定日期。</param>
        /// <returns>指定日期所在月份的所有日期。</returns>
        [Obsolete("请直接使用 DateTimeUtil.GetMonthDays(date)")]
        public static List<DateTime> GetMonthDays(this DateTime date) => DateTimeUtil.GetMonthDays(date);

        /// <summary>
        /// 获取指定日期所在季度的所有日期。
        /// </summary>
        /// <param name="date">指定日期。</param>
        /// <returns>指定日期所在季度的所有日期。</returns>
        [Obsolete("请直接使用 DateTimeUtil.GetQuarterDays(date)")]
        public static List<DateTime> GetQuarterDays(this DateTime date) => DateTimeUtil.GetQuarterDays(date);

        /// <summary>
        /// 获取指定日期所在年份的所有日期。
        /// </summary>
        /// <param name="date">指定日期。</param>
        /// <returns>指定日期所在年份的所有日期。</returns>
        [Obsolete("请直接使用 DateTimeUtil.GetYearDays(date)")]
        public static List<DateTime> GetYearDays(this DateTime date) => DateTimeUtil.GetYearDays(date);



        #region 新增扩展方法

        /// <summary>
        /// 判断日期是否是今天
        /// </summary>
        public static bool IsToday(this DateTime date)
        {
            return date.Date == DateTime.Today;
        }

        /// <summary>
        /// 判断日期是否是昨天
        /// </summary>
        public static bool IsYesterday(this DateTime date)
        {
            return date.Date == DateTime.Today.AddDays(-1);
        }

        /// <summary>
        /// 判断日期是否是明天
        /// </summary>
        public static bool IsTomorrow(this DateTime date)
        {
            return date.Date == DateTime.Today.AddDays(1);
        }

        /// <summary>
        /// 判断日期是否在本周
        /// </summary>
        public static bool IsThisWeek(this DateTime date)
        {
            var today = DateTime.Today;
            var firstDayOfWeek = today.GetFirstDayOfWeek();
            var lastDayOfWeek = firstDayOfWeek.AddDays(6);
            return date.Date >= firstDayOfWeek && date.Date <= lastDayOfWeek;
        }

        /// <summary>
        /// 判断日期是否在本月
        /// </summary>
        public static bool IsThisMonth(this DateTime date)
        {
            var today = DateTime.Today;
            return date.Year == today.Year && date.Month == today.Month;
        }

        /// <summary>
        /// 判断日期是否在本年
        /// </summary>
        public static bool IsThisYear(this DateTime date)
        {
            return date.Year == DateTime.Today.Year;
        }

        /// <summary>
        /// 判断日期是否在指定范围内（包含边界）
        /// </summary>
        public static bool IsBetween(this DateTime date, DateTime startDate, DateTime endDate)
        {
            return date >= startDate && date <= endDate;
        }

        /// <summary>
        /// 计算年龄
        /// </summary>
        public static int ToAge(this DateTime birthDate)
        {
            var today = DateTime.Today;
            int age = today.Year - birthDate.Year;

            if (birthDate > today.AddYears(-age))
                age--;

            return age;
        }


        /// <summary>
        /// 判断是否是周末（周六或周日）
        /// </summary>
        public static bool IsWeekend(this DateTime date)
        {
            return date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
        }

        /// <summary>
        /// 获取日期所在月的最后一天
        /// </summary>
        public static DateTime GetLastDayOfMonth(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));
        }

        /// <summary>
        /// 获取日期所在月的最后一天
        /// </summary>
        public static DateTime GetLastDayOfWeek(this DateTime date)
        {
            var firstDay = date.GetFirstDayOfWeek();
            return firstDay.AddDays(6);
        }

        /// <summary>
        /// 获取日期所在季度的最后一天
        /// </summary>
        public static DateTime GetLastDayOfQuarter(this DateTime date)
        {
            int currentQuarter = (date.Month - 1) / 3 + 1;
            int lastMonthOfQuarter = currentQuarter * 3;
            return new DateTime(date.Year, lastMonthOfQuarter, DateTime.DaysInMonth(date.Year, lastMonthOfQuarter));
        }

        /// <summary>
        /// 获取日期所在年的最后一天
        /// </summary>
        public static DateTime GetLastDayOfYear(this DateTime date)
        {
            return new DateTime(date.Year, 12, 31);
        }

        /// <summary>
        /// 获取日期的中文星期表示
        /// </summary>
        public static string ToChineseWeekDay(this DateTime date)
        {
            return date.DayOfWeek switch
            {
                DayOfWeek.Monday => "星期一",
                DayOfWeek.Tuesday => "星期二",
                DayOfWeek.Wednesday => "星期三",
                DayOfWeek.Thursday => "星期四",
                DayOfWeek.Friday => "星期五",
                DayOfWeek.Saturday => "星期六",
                DayOfWeek.Sunday => "星期日",
                _ => string.Empty
            };
        }

        /// <summary>
        /// 获取日期的中文星期简称
        /// </summary>
        public static string ToChineseWeekDayShort(this DateTime date)
        {
            return date.DayOfWeek switch
            {
                DayOfWeek.Monday => "周一",
                DayOfWeek.Tuesday => "周二",
                DayOfWeek.Wednesday => "周三",
                DayOfWeek.Thursday => "周四",
                DayOfWeek.Friday => "周五",
                DayOfWeek.Saturday => "周六",
                DayOfWeek.Sunday => "周日",
                _ => string.Empty
            };
        }


        /// <summary>
        /// 获取日期所在季度的数字（1-4）
        /// </summary>
        public static int GetQuarter(this DateTime date)
        {
            return (date.Month - 1) / 3 + 1;
        }

        /// <summary>
        /// 获取日期所在周在本年的周数
        /// </summary>
        public static int GetWeekOfYear(this DateTime date)
        {
            var culture = CultureInfo.CurrentCulture;
            var calendar = culture.Calendar;
            var weekRule = culture.DateTimeFormat.CalendarWeekRule;
            var firstDayOfWeek = culture.DateTimeFormat.FirstDayOfWeek;
            return calendar.GetWeekOfYear(date, weekRule, firstDayOfWeek);
        }

        /// <summary>
        /// 添加工作日
        /// </summary>
        /// <param name="date">起始日期</param>
        /// <param name="workDays">要添加的工作日数</param>
        public static DateTime AddWorkDays(this DateTime date, int workDays)
        {
            var result = date;
            int daysToAdd = Math.Abs(workDays);
            int direction = workDays >= 0 ? 1 : -1;

            while (daysToAdd > 0)
            {
                result = result.AddDays(direction);
                if (result.IsWorkDay())
                    daysToAdd--;
            }

            return result;
        }

        /// <summary>
        /// 获取日期的友好字符串表示
        /// </summary>
        public static string ToFriendlyString(this DateTime date)
        {
            var today = DateTime.Today;
            var span = today - date.Date;

            return span.TotalDays switch
            {
                0 => "今天",
                1 => "昨天",
                -1 => "明天",
                _ when span.TotalDays > 0 && span.TotalDays <= 7 => $"上周{date.ToChineseWeekDayShort()}",
                _ when span.TotalDays < 0 && span.TotalDays >= -7 => $"下周{date.ToChineseWeekDayShort()}",
                _ when date.Year == today.Year => date.ToString("MM月dd日"),
                _ => date.ToString("yyyy年MM月dd日")
            };
        }

        /// <summary>
        /// 获取两个日期之间相差的月数
        /// </summary>
        public static int GetMonthsBetween(this DateTime startDate, DateTime endDate)
        {
            int months = (endDate.Year - startDate.Year) * 12 + endDate.Month - startDate.Month;

            // 如果结束日期的日小于开始日期的日，需要减去一个月
            if (endDate.Day < startDate.Day)
            {
                months--;
            }

            return Math.Abs(months);
        }

        /// <summary>
        /// 获取两个日期之间相差的年数
        /// </summary>
        public static int GetYearsBetween(this DateTime startDate, DateTime endDate)
        {
            int years = endDate.Year - startDate.Year;

            // 如果结束日期的月和日小于开始日期的月和日，需要减去一年
            if (endDate.Month < startDate.Month || (endDate.Month == startDate.Month && endDate.Day < startDate.Day))
            {
                years--;
            }

            return Math.Abs(years);
        }

        /// <summary>
        /// 获取一天的开始时间（00:00:00）
        /// </summary>
        public static DateTime StartOfDay(this DateTime date)
        {
            return date.Date;
        }

        /// <summary>
        /// 获取一天的结束时间（23:59:59）
        /// </summary>
        public static DateTime EndOfDay(this DateTime date)
        {
            return date.Date.AddDays(1).AddTicks(-1);
        }

        /// <summary>
        /// 获取月的开始时间
        /// </summary>
        public static DateTime StartOfMonth(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1);
        }

        /// <summary>
        /// 获取月的结束时间
        /// </summary>
        public static DateTime EndOfMonth(this DateTime date)
        {
            return date.GetLastDayOfMonth().EndOfDay();
        }

        /// <summary>
        /// 获取年的开始时间
        /// </summary>
        public static DateTime StartOfYear(this DateTime date)
        {
            return new DateTime(date.Year, 1, 1);
        }

        /// <summary>
        /// 获取年的结束时间
        /// </summary>
        public static DateTime EndOfYear(this DateTime date)
        {
            return new DateTime(date.Year, 12, 31).EndOfDay();
        }

        #endregion
    }
}
