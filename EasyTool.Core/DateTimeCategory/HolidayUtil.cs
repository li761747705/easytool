using System;
using System.Collections.Generic;

namespace EasyTool.DateTimeCategory
{
    /// <summary>
    /// 节假日工具类
    /// 支持中国法定节假日和常见国际节日
    /// </summary>
    public static class HolidayUtil
    {
        /// <summary>
        /// 判断是否为中国法定节假日
        /// </summary>
        public static bool IsChineseHoliday(DateTime date)
        {
            return GetChineseHoliday(date) != null;
        }

        /// <summary>
        /// 获取中国节假日名称
        /// </summary>
        public static string GetChineseHoliday(DateTime date)
        {
            int year = date.Year;
            int month = date.Month;
            int day = date.Day;

            // 固定日期节日
            if (month == 1 && day == 1) return "元旦";
            if (month == 5 && day == 1) return "劳动节";
            if (month == 10 && day == 1) return "国庆节";

            // 农历节日（简化计算，使用近似日期）
            var lunar = LunarCalendarUtil.SolarToLunar(date);
            if (lunar != null)
            {
                if (lunar.Month == 1 && lunar.Day == 1) return "春节";
                if (lunar.Month == 1 && lunar.Day == 15) return "元宵节";
                if (lunar.Month == 5 && lunar.Day == 5) return "端午节";
                if (lunar.Month == 8 && lunar.Day == 15) return "中秋节";
                if (lunar.Month == 9 && lunar.Day == 9) return "重阳节";
                if (lunar.Month == 12 && lunar.Day == 30) return "除夕";
            }

            // 母亲节：5月第二个星期日
            if (month == 5)
            {
                var motherDay = GetNthDayOfWeek(year, 5, DayOfWeek.Sunday, 2);
                if (day == motherDay) return "母亲节";
            }

            // 父亲节：6月第三个星期日
            if (month == 6)
            {
                var fatherDay = GetNthDayOfWeek(year, 6, DayOfWeek.Sunday, 3);
                if (day == fatherDay) return "父亲节";
            }

            // 教师节：9月10日
            if (month == 9 && day == 10) return "教师节";

            // 清明节：4月4日或5日（简化）
            var qingming = GetQingmingDate(year);
            if (month == 4 && day == qingming) return "清明节";

            // 儿童节：6月1日
            if (month == 6 && day == 1) return "儿童节";

            // 妇女节：3月8日
            if (month == 3 && day == 8) return "妇女节";

            // 植树节：3月12日
            if (month == 3 && day == 12) return "植树节";

            // 青年节：5月4日
            if (month == 5 && day == 4) return "青年节";

            // 建党节：7月1日
            if (month == 7 && day == 1) return "建党节";

            // 建军节：8月1日
            if (month == 8 && day == 1) return "建军节";

            return null;
        }

        /// <summary>
        /// 判断是否为国际常见节日
        /// </summary>
        public static bool IsInternationalHoliday(DateTime date)
        {
            return GetInternationalHoliday(date) != null;
        }

        /// <summary>
        /// 获取国际节日名称
        /// </summary>
        public static string GetInternationalHoliday(DateTime date)
        {
            int month = date.Month;
            int day = date.Day;
            int year = date.Year;

            // 固定日期
            if (month == 1 && day == 1) return "New Year's Day";
            if (month == 2 && day == 14) return "Valentine's Day";
            if (month == 3 && day == 8) return "International Women's Day";
            if (month == 3 && day == 12) return "Arbor Day";
            if (month == 3 && day == 21) return "World Sleep Day";
            if (month == 4 && day == 1) return "April Fools' Day";
            if (month == 4 && day == 22) return "Earth Day";
            if (month == 4 && day == 23) return "World Book Day";
            if (month == 5 && day == 1) return "International Workers' Day";
            if (month == 5 && day == 4) return "Star Wars Day";
            if (month == 6 && day == 1) return "International Children's Day";
            if (month == 6 && day == 5) return "World Environment Day";
            if (month == 9 && day == 21) return "International Day of Peace";
            if (month == 10 && day == 31) return "Halloween";
            if (month == 11 && day == 11) return "Veterans Day / Singles' Day";
            if (month == 12 && day == 24) return "Christmas Eve";
            if (month == 12 && day == 25) return "Christmas Day";
            if (month == 12 && day == 31) return "New Year's Eve";

            // 复活节（春分后第一个满月后的第一个星期日）
            var easter = CalculateEaster(year);
            if (date == easter) return "Easter Sunday";
            if (date == easter.AddDays(-2)) return "Good Friday";
            if (date == easter.AddDays(1)) return "Easter Monday";

            // 感恩节（11月第四个星期四）
            var thanksgiving = GetNthDayOfWeek(year, 11, DayOfWeek.Thursday, 4);
            if (date.Day == thanksgiving && month == 11) return "Thanksgiving";

            // 黑色星期五（感恩节后一天）
            if (month == 11 && date.Day == thanksgiving + 1) return "Black Friday";

            return null;
        }

        /// <summary>
        /// 获取指定年份的所有中国节假日
        /// </summary>
        public static Dictionary<DateTime, string> GetChineseHolidays(int year)
        {
            var holidays = new Dictionary<DateTime, string>();

            // 固定日期节日
            holidays[new DateTime(year, 1, 1)] = "元旦";
            holidays[new DateTime(year, 5, 1)] = "劳动节";
            holidays[new DateTime(year, 10, 1)] = "国庆节";

            // 清明节
            int qingming = GetQingmingDate(year);
            holidays[new DateTime(year, 4, qingming)] = "清明节";

            // 农历节日（需要转换）
            // 春节（农历正月初一）
            var springFestival = LunarToSolar(year, 1, 1);
            if (springFestival.HasValue)
            {
                holidays[springFestival.Value] = "春节";
                holidays[springFestival.Value.AddDays(-1)] = "除夕";
            }

            // 元宵节
            var lanternFestival = LunarToSolar(year, 1, 15);
            if (lanternFestival.HasValue)
                holidays[lanternFestival.Value] = "元宵节";

            // 端午节
            var dragonBoat = LunarToSolar(year, 5, 5);
            if (dragonBoat.HasValue)
                holidays[dragonBoat.Value] = "端午节";

            // 中秋节
            var midAutumn = LunarToSolar(year, 8, 15);
            if (midAutumn.HasValue)
                holidays[midAutumn.Value] = "中秋节";

            // 其他节日
            holidays[new DateTime(year, 3, 8)] = "妇女节";
            holidays[new DateTime(year, 3, 12)] = "植树节";
            holidays[new DateTime(year, 5, 4)] = "青年节";
            holidays[new DateTime(year, 6, 1)] = "儿童节";
            holidays[new DateTime(year, 7, 1)] = "建党节";
            holidays[new DateTime(year, 8, 1)] = "建军节";
            holidays[new DateTime(year, 9, 10)] = "教师节";

            // 母亲节、父亲节
            var motherDay = GetNthDayOfWeek(year, 5, DayOfWeek.Sunday, 2);
            holidays[new DateTime(year, 5, motherDay)] = "母亲节";

            var fatherDay = GetNthDayOfWeek(year, 6, DayOfWeek.Sunday, 3);
            holidays[new DateTime(year, 6, fatherDay)] = "父亲节";

            return holidays;
        }

        /// <summary>
        /// 获取清明节的日期（4月4日或5日）
        /// </summary>
        private static int GetQingmingDate(int year)
        {
            // 清明节大约在公历4月4日或5日
            // 使用简化算法
            int y = year % 100;
            int d = (y * 0.2422 + 4.81) % 1 > 0.5 ? 4 : 5;
            return d;
        }

        /// <summary>
        /// 获取某月第N个某星期几的日期
        /// </summary>
        private static int GetNthDayOfWeek(int year, int month, DayOfWeek dayOfWeek, int n)
        {
            var firstDay = new DateTime(year, month, 1);
            int offset = ((int)dayOfWeek - (int)firstDay.DayOfWeek + 7) % 7;
            return 1 + offset + (n - 1) * 7;
        }

        /// <summary>
        /// 计算复活节日期
        /// </summary>
        private static DateTime CalculateEaster(int year)
        {
            int a = year % 19;
            int b = year / 100;
            int c = year % 100;
            int d = b / 4;
            int e = b % 4;
            int f = (b + 8) / 25;
            int g = (b - f + 1) / 3;
            int h = (19 * a + b - d - g + 15) % 30;
            int i = c / 4;
            int k = c % 4;
            int l = (32 + 2 * e + 2 * i - h - k) % 7;
            int m = (a + 11 * h + 22 * l) / 451;
            int month = (h + l - 7 * m + 114) / 31;
            int day = ((h + l - 7 * m + 114) % 31) + 1;

            return new DateTime(year, month, day);
        }

        /// <summary>
        /// 农历转公历（简化版）
        /// </summary>
        private static DateTime? LunarToSolar(int year, int lunarMonth, int lunarDay)
        {
            // 这里需要使用 LunarCalendarUtil，如果不存在则返回 null
            try
            {
                return LunarCalendarUtil.LunarToSolar(year, lunarMonth, lunarDay);
            }
            catch
            {
                return null;
            }
        }
    }

    /// <summary>
    /// 工作日工具类
    /// </summary>
    public static class WorkdayUtil
    {
        private static readonly HashSet<DateTime> _holidays = new();
        private static readonly HashSet<DateTime> _workdays = new(); // 调休工作日

        /// <summary>
        /// 设置节假日
        /// </summary>
        public static void SetHoliday(DateTime date)
        {
            _holidays.Add(date.Date);
            _workdays.Remove(date.Date);
        }

        /// <summary>
        /// 设置调休工作日
        /// </summary>
        public static void SetWorkday(DateTime date)
        {
            _workdays.Add(date.Date);
            _holidays.Remove(date.Date);
        }

        /// <summary>
        /// 判断是否为工作日
        /// </summary>
        public static bool IsWorkday(DateTime date)
        {
            date = date.Date;

            // 优先检查调休工作日
            if (_workdays.Contains(date)) return true;

            // 检查节假日
            if (_holidays.Contains(date)) return false;
            if (HolidayUtil.IsChineseHoliday(date)) return false;

            // 默认周一到周五为工作日
            return date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday;
        }

        /// <summary>
        /// 获取下一个工作日
        /// </summary>
        public static DateTime GetNextWorkday(DateTime date)
        {
            var next = date.Date.AddDays(1);
            while (!IsWorkday(next))
            {
                next = next.AddDays(1);
            }
            return next;
        }

        /// <summary>
        /// 获取上一个工作日
        /// </summary>
        public static DateTime GetPreviousWorkday(DateTime date)
        {
            var prev = date.Date.AddDays(-1);
            while (!IsWorkday(prev))
            {
                prev = prev.AddDays(-1);
            }
            return prev;
        }

        /// <summary>
        /// 计算两个日期之间的工作日数量
        /// </summary>
        public static int CountWorkdays(DateTime start, DateTime end)
        {
            if (start > end)
                (start, end) = (end, start);

            int count = 0;
            var current = start.Date;
            while (current <= end.Date)
            {
                if (IsWorkday(current)) count++;
                current = current.AddDays(1);
            }
            return count;
        }

        /// <summary>
        /// 添加工作日
        /// </summary>
        public static DateTime AddWorkdays(DateTime date, int days)
        {
            var result = date.Date;
            int step = days > 0 ? 1 : -1;
            int remaining = Math.Abs(days);

            while (remaining > 0)
            {
                result = result.AddDays(step);
                if (IsWorkday(result))
                    remaining--;
            }

            return result;
        }

        /// <summary>
        /// 清空节假日和调休设置
        /// </summary>
        public static void Clear()
        {
            _holidays.Clear();
            _workdays.Clear();
        }
    }

    /// <summary>
    /// 友好时间显示工具类
    /// </summary>
    public static class TimeAgoUtil
    {
        /// <summary>
        /// 获取友好时间显示（如"3分钟前"、"昨天"）
        /// </summary>
        public static string Format(DateTime date, DateTime? now = null)
        {
            return Format(date, now ?? DateTime.Now, false);
        }

        /// <summary>
        /// 获取友好时间显示（英文版）
        /// </summary>
        public static string FormatEnglish(DateTime date, DateTime? now = null)
        {
            return Format(date, now ?? DateTime.Now, true);
        }

        private static string Format(DateTime date, DateTime now, bool english)
        {
            var span = now - date;

            if (span.TotalSeconds < 0)
            {
                return english ? "in the future" : "未来";
            }

            if (span.TotalSeconds < 60)
            {
                int seconds = (int)span.TotalSeconds;
                return english ? $"{seconds} second{(seconds != 1 ? "s" : "")} ago" : $"{seconds}秒前";
            }

            if (span.TotalMinutes < 60)
            {
                int minutes = (int)span.TotalMinutes;
                return english ? $"{minutes} minute{(minutes != 1 ? "s" : "")} ago" : $"{minutes}分钟前";
            }

            if (span.TotalHours < 24)
            {
                int hours = (int)span.TotalHours;
                return english ? $"{hours} hour{(hours != 1 ? "s" : "")} ago" : $"{hours}小时前";
            }

            if (span.TotalDays < 2 && date.Date == now.Date.AddDays(-1))
            {
                return english ? "yesterday" : "昨天";
            }

            if (span.TotalDays < 7)
            {
                int days = (int)span.TotalDays;
                return english ? $"{days} day{(days != 1 ? "s" : "")} ago" : $"{days}天前";
            }

            if (span.TotalDays < 30)
            {
                int weeks = (int)(span.TotalDays / 7);
                return english ? $"{weeks} week{(weeks != 1 ? "s" : "")} ago" : $"{weeks}周前";
            }

            if (span.TotalDays < 365)
            {
                int months = (int)(span.TotalDays / 30);
                return english ? $"{months} month{(months != 1 ? "s" : "")} ago" : $"{months}个月前";
            }

            int years = (int)(span.TotalDays / 365);
            return english ? $"{years} year{(years != 1 ? "s" : "")} ago" : $"{years}年前";
        }

        /// <summary>
        /// 获取剩余时间显示（如"剩余3天"）
        /// </summary>
        public static string FormatRemaining(DateTime deadline, DateTime? now = null)
        {
            return FormatRemaining(deadline, now ?? DateTime.Now, false);
        }

        private static string FormatRemaining(DateTime deadline, DateTime now, bool english)
        {
            var span = deadline - now;

            if (span.TotalSeconds < 0)
            {
                return english ? "overdue" : "已过期";
            }

            if (span.TotalMinutes < 1)
            {
                return english ? "less than 1 minute" : "不到1分钟";
            }

            if (span.TotalHours < 1)
            {
                int minutes = (int)span.TotalMinutes;
                return english ? $"{minutes} minute{(minutes != 1 ? "s" : "")} remaining" : $"剩余{minutes}分钟";
            }

            if (span.TotalDays < 1)
            {
                int hours = (int)span.TotalHours;
                return english ? $"{hours} hour{(hours != 1 ? "s" : "")} remaining" : $"剩余{hours}小时";
            }

            if (span.TotalDays < 7)
            {
                int days = (int)span.TotalDays;
                return english ? $"{days} day{(days != 1 ? "s" : "")} remaining" : $"剩余{days}天";
            }

            if (span.TotalDays < 30)
            {
                int weeks = (int)(span.TotalDays / 7);
                return english ? $"{weeks} week{(weeks != 1 ? "s" : "")} remaining" : $"剩余{weeks}周";
            }

            int months = (int)(span.TotalDays / 30);
            return english ? $"{months} month{(months != 1 ? "s" : "")} remaining" : $"剩余{months}个月";
        }
    }
}
