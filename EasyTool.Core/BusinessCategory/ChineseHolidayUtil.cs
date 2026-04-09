using System;
using System.Collections.Generic;

namespace EasyTool.BusinessCategory
{
    /// <summary>
    /// 中国节假日工具类
    /// 提供法定节假日、工作日判断功能（含调休）
    /// </summary>
    public static class ChineseHolidayUtil
    {
        #region 数据结构

        /// <summary>
        /// 节假日信息
        /// </summary>
        public class HolidayInfo
        {
            /// <summary>
            /// 节假日名称
            /// </summary>
            public string Name { get; set; } = string.Empty;

            /// <summary>
            /// 开始日期
            /// </summary>
            public DateTime StartDate { get; set; }

            /// <summary>
            /// 结束日期
            /// </summary>
            public DateTime EndDate { get; set; }

            /// <summary>
            /// 假期天数
            /// </summary>
            public int Days { get; set; }
        }

        #endregion

        #region 静态数据

        // 固定日期节日
        private static readonly Dictionary<int, (int Month, int Day, string Name)> FixedHolidays = new()
        {
            { 1, (1, 1, "元旦") },
            { 2, (2, 14, "情人节") },
            { 3, (3, 8, "妇女节") },
            { 4, (3, 12, "植树节") },
            { 5, (4, 1, "愚人节") },
            { 6, (5, 1, "劳动节") },
            { 7, (5, 4, "青年节") },
            { 8, (6, 1, "儿童节") },
            { 9, (7, 1, "建党节") },
            { 10, (8, 1, "建军节") },
            { 11, (9, 10, "教师节") },
            { 12, (10, 1, "国庆节") },
            { 13, (10, 2, "国庆节") },
            { 14, (10, 3, "国庆节") },
            { 15, (12, 25, "圣诞节") }
        };

        // 农历节日（农历月份、日期、名称）
        private static readonly List<(int Month, int Day, string Name)> LunarHolidays = new()
        {
            (1, 1, "春节"),
            (1, 15, "元宵节"),
            (5, 5, "端午节"),
            (7, 7, "七夕节"),
            (7, 15, "中元节"),
            (8, 15, "中秋节"),
            (9, 9, "重阳节"),
            (12, 8, "腊八节"),
            (12, 30, "除夕") // 特殊处理
        };

        // 2024年法定节假日数据（实际以国务院公布为准）
        private static readonly Dictionary<int, List<HolidayInfo>> LegalHolidays = new()
        {
            { 2024, new List<HolidayInfo>
                {
                    new() { Name = "元旦", StartDate = new(2024, 1, 1), EndDate = new(2024, 1, 1), Days = 1 },
                    new() { Name = "春节", StartDate = new(2024, 2, 10), EndDate = new(2024, 2, 17), Days = 8 },
                    new() { Name = "清明节", StartDate = new(2024, 4, 4), EndDate = new(2024, 4, 6), Days = 3 },
                    new() { Name = "劳动节", StartDate = new(2024, 5, 1), EndDate = new(2024, 5, 5), Days = 5 },
                    new() { Name = "端午节", StartDate = new(2024, 6, 8), EndDate = new(2024, 6, 10), Days = 3 },
                    new() { Name = "中秋节", StartDate = new(2024, 9, 15), EndDate = new(2024, 9, 17), Days = 3 },
                    new() { Name = "国庆节", StartDate = new(2024, 10, 1), EndDate = new(2024, 10, 7), Days = 7 }
                }
            },
            { 2025, new List<HolidayInfo>
                {
                    new() { Name = "元旦", StartDate = new(2025, 1, 1), EndDate = new(2025, 1, 1), Days = 1 },
                    new() { Name = "春节", StartDate = new(2025, 1, 28), EndDate = new(2025, 2, 4), Days = 8 },
                    new() { Name = "清明节", StartDate = new(2025, 4, 4), EndDate = new(2025, 4, 6), Days = 3 },
                    new() { Name = "劳动节", StartDate = new(2025, 5, 1), EndDate = new(2025, 5, 5), Days = 5 },
                    new() { Name = "端午节", StartDate = new(2025, 5, 31), EndDate = new(2025, 6, 2), Days = 3 },
                    new() { Name = "中秋节", StartDate = new(2025, 10, 6), EndDate = new(2025, 10, 8), Days = 3 },
                    new() { Name = "国庆节", StartDate = new(2025, 10, 1), EndDate = new(2025, 10, 7), Days = 7 }
                }
            },
            { 2026, new List<HolidayInfo>
                {
                    new() { Name = "元旦", StartDate = new(2026, 1, 1), EndDate = new(2026, 1, 3), Days = 3 },
                    new() { Name = "春节", StartDate = new(2026, 2, 17), EndDate = new(2026, 2, 23), Days = 7 },
                    new() { Name = "清明节", StartDate = new(2026, 4, 4), EndDate = new(2026, 4, 6), Days = 3 },
                    new() { Name = "劳动节", StartDate = new(2026, 5, 1), EndDate = new(2026, 5, 5), Days = 5 },
                    new() { Name = "端午节", StartDate = new(2026, 5, 31), EndDate = new(2026, 6, 2), Days = 3 },
                    new() { Name = "中秋节", StartDate = new(2026, 9, 25), EndDate = new(2026, 9, 27), Days = 3 },
                    new() { Name = "国庆节", StartDate = new(2026, 10, 1), EndDate = new(2026, 10, 7), Days = 7 }
                }
            }
        };

        // 调休工作日（周末需要上班的日期）
        private static readonly HashSet<DateTime> AdjustedWorkdays = new()
        {
            // 2024年调休
            new(2024, 2, 4), new(2024, 2, 18),
            new(2024, 4, 7),
            new(2024, 4, 28), new(2024, 5, 11),
            new(2024, 6, 16),
            new(2024, 9, 14),
            new(2024, 9, 29), new(2024, 10, 12),
            // 2025年调休
            new(2025, 1, 26), new(2025, 2, 8),
            new(2025, 4, 27),
            new(2025, 4, 30),
            new(2025, 5, 28),
            new(2025, 9, 28), new(2025, 10, 11),
            // 2026年调休（预估）
            new(2026, 2, 15), new(2026, 2, 24),
            new(2026, 4, 5),
            new(2026, 5, 3),
            new(2026, 5, 30),
            new(2026, 9, 26),
            new(2026, 10, 10)
        };

        #endregion

        #region 节假日判断

        /// <summary>
        /// 判断是否为法定节假日
        /// </summary>
        /// <param name="date">日期</param>
        /// <returns>是否为法定节假日</returns>
        public static bool IsHoliday(DateTime date)
        {
            var year = date.Year;
            if (LegalHolidays.TryGetValue(year, out var holidays))
            {
                foreach (var holiday in holidays)
                {
                    if (date >= holiday.StartDate && date <= holiday.EndDate)
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 判断是否为工作日（考虑法定节假日和调休）
        /// </summary>
        /// <param name="date">日期</param>
        /// <returns>是否为工作日</returns>
        public static bool IsWorkday(DateTime date)
        {
            // 先检查是否为调休工作日
            if (AdjustedWorkdays.Contains(date.Date))
                return true;

            // 法定节假日不是工作日
            if (IsHoliday(date))
                return false;

            // 周一到周五为工作日
            var dayOfWeek = date.DayOfWeek;
            return dayOfWeek != DayOfWeek.Saturday && dayOfWeek != DayOfWeek.Sunday;
        }

        /// <summary>
        /// 判断是否为休息日
        /// </summary>
        /// <param name="date">日期</param>
        /// <returns>是否为休息日</returns>
        public static bool IsRestDay(DateTime date)
        {
            return !IsWorkday(date);
        }

        /// <summary>
        /// 判断是否为周末（不含调休）
        /// </summary>
        /// <param name="date">日期</param>
        /// <returns>是否为周末</returns>
        public static bool IsWeekend(DateTime date)
        {
            var dayOfWeek = date.DayOfWeek;
            return dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday;
        }

        #endregion

        #region 节假日信息

        /// <summary>
        /// 获取节假日信息
        /// </summary>
        /// <param name="date">日期</param>
        /// <returns>节假日信息，如果不是节假日返回null</returns>
        public static HolidayInfo? GetHolidayInfo(DateTime date)
        {
            var year = date.Year;
            if (LegalHolidays.TryGetValue(year, out var holidays))
            {
                foreach (var holiday in holidays)
                {
                    if (date >= holiday.StartDate && date <= holiday.EndDate)
                        return holiday;
                }
            }
            return null;
        }

        /// <summary>
        /// 获取年份所有法定节假日
        /// </summary>
        /// <param name="year">年份</param>
        /// <returns>节假日列表</returns>
        public static List<HolidayInfo> GetHolidaysOfYear(int year)
        {
            if (LegalHolidays.TryGetValue(year, out var holidays))
                return holidays;

            return new List<HolidayInfo>();
        }

        /// <summary>
        /// 获取下一个节假日
        /// </summary>
        /// <param name="date">起始日期（默认今天）</param>
        /// <returns>下一个节假日信息</returns>
        public static HolidayInfo? GetNextHoliday(DateTime? date = null)
        {
            var start = date ?? DateTime.Today;
            var year = start.Year;

            // 在当前年份查找
            if (LegalHolidays.TryGetValue(year, out var holidays))
            {
                foreach (var holiday in holidays)
                {
                    if (holiday.StartDate > start)
                        return holiday;
                }
            }

            // 在下一年查找
            if (LegalHolidays.TryGetValue(year + 1, out var nextYearHolidays) && nextYearHolidays.Count > 0)
                return nextYearHolidays[0];

            return null;
        }

        /// <summary>
        /// 获取距离下一个节假日的天数
        /// </summary>
        /// <param name="date">起始日期（默认今天）</param>
        /// <returns>天数</returns>
        public static int GetDaysToNextHoliday(DateTime? date = null)
        {
            var start = date ?? DateTime.Today;
            var nextHoliday = GetNextHoliday(start);

            if (nextHoliday == null)
                return -1;

            return (int)(nextHoliday.StartDate - start).TotalDays;
        }

        /// <summary>
        /// 获取当年剩余节假日天数
        /// </summary>
        /// <param name="date">起始日期（默认今天）</param>
        /// <returns>天数</returns>
        public static int GetRemainingHolidayDays(DateTime? date = null)
        {
            var start = date ?? DateTime.Today;
            var year = start.Year;
            var totalDays = 0;

            if (LegalHolidays.TryGetValue(year, out var holidays))
            {
                foreach (var holiday in holidays)
                {
                    if (holiday.EndDate >= start)
                    {
                        var effectiveStart = holiday.StartDate > start ? holiday.StartDate : start;
                        var effectiveEnd = holiday.EndDate;
                        totalDays += (int)(effectiveEnd - effectiveStart).TotalDays + 1;
                    }
                }
            }

            return totalDays;
        }

        #endregion

        #region 传统节日

        /// <summary>
        /// 获取传统节日（根据农历计算）
        /// </summary>
        /// <param name="date">阳历日期</param>
        /// <returns>节日名称，如果不是传统节日返回null</returns>
        public static string? GetTraditionalHoliday(DateTime date)
        {
            // 使用农历转换
            var lunarDate = DateTimeCategory.LunarCalendarUtil.SolarToLunar(date);
            if (lunarDate == null)
                return null;

            foreach (var (month, day, name) in LunarHolidays)
            {
                // 除夕特殊处理（农历12月29或30日）
                if (name == "除夕")
                {
                    var nextDay = date.AddDays(1);
                    var nextLunar = DateTimeCategory.LunarCalendarUtil.SolarToLunar(nextDay);
                    if (nextLunar != null && nextLunar.Month == 1 && nextLunar.Day == 1)
                        return "除夕";
                }
                else if (lunarDate.Month == month && lunarDate.Day == day)
                {
                    return name;
                }
            }

            return null;
        }

        /// <summary>
        /// 判断是否为传统节日
        /// </summary>
        /// <param name="date">日期</param>
        /// <returns>是否为传统节日</returns>
        public static bool IsTraditionalHoliday(DateTime date)
        {
            return GetTraditionalHoliday(date) != null;
        }

        #endregion

        #region 固定节日

        /// <summary>
        /// 获取固定日期的节日名称
        /// </summary>
        /// <param name="date">日期</param>
        /// <returns>节日名称，如果不是节日返回null</returns>
        public static string? GetFixedHoliday(DateTime date)
        {
            foreach (var (_, (month, day, name)) in FixedHolidays)
            {
                if (date.Month == month && date.Day == day)
                    return name;
            }
            return null;
        }

        /// <summary>
        /// 判断是否为固定日期节日
        /// </summary>
        /// <param name="date">日期</param>
        /// <returns>是否为固定节日</returns>
        public static bool IsFixedHoliday(DateTime date)
        {
            return GetFixedHoliday(date) != null;
        }

        #endregion

        #region 工作日计算

        /// <summary>
        /// 获取两个日期之间的工作日天数
        /// </summary>
        /// <param name="startDate">开始日期</param>
        /// <param name="endDate">结束日期</param>
        /// <returns>工作日天数</returns>
        public static int GetWorkdaysBetween(DateTime startDate, DateTime endDate)
        {
            if (startDate > endDate)
                (startDate, endDate) = (endDate, startDate);

            var workdays = 0;
            var current = startDate;

            while (current <= endDate)
            {
                if (IsWorkday(current))
                    workdays++;
                current = current.AddDays(1);
            }

            return workdays;
        }

        /// <summary>
        /// 计算从指定日期开始，经过N个工作日后的日期
        /// </summary>
        /// <param name="startDate">开始日期</param>
        /// <param name="workdays">工作日数</param>
        /// <returns>目标日期</returns>
        public static DateTime AddWorkdays(DateTime startDate, int workdays)
        {
            var current = startDate;
            var remaining = Math.Abs(workdays);
            var direction = workdays >= 0 ? 1 : -1;

            while (remaining > 0)
            {
                current = current.AddDays(direction);
                if (IsWorkday(current))
                    remaining--;
            }

            return current;
        }

        #endregion
    }
}