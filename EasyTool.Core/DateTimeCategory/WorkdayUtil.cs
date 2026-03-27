using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyTool.DateTimeCategory
{
    /// <summary>
    /// 工作日计算工具类
    /// 提供工作日相关的计算功能，支持自定义节假日和调休
    /// </summary>
    public static class WorkdayUtil
    {
        private static readonly HashSet<DateTime> _defaultHolidays = new();
        private static readonly HashSet<DateTime> _defaultWorkdays = new();

        static WorkdayUtil()
        {
            // 可以在这里初始化默认的节假日和调休工作日
        }

        /// <summary>
        /// 添加节假日
        /// </summary>
        /// <param name="date">节假日日期</param>
        public static void AddHoliday(DateTime date)
        {
            _defaultHolidays.Add(date.Date);
        }

        /// <summary>
        /// 批量添加节假日
        /// </summary>
        /// <param name="dates">节假日日期集合</param>
        public static void AddHolidays(IEnumerable<DateTime> dates)
        {
            foreach (var date in dates)
            {
                _defaultHolidays.Add(date.Date);
            }
        }

        /// <summary>
        /// 移除节假日
        /// </summary>
        /// <param name="date">节假日日期</param>
        public static void RemoveHoliday(DateTime date)
        {
            _defaultHolidays.Remove(date.Date);
        }

        /// <summary>
        /// 添加调休工作日（周末调休上班）
        /// </summary>
        /// <param name="date">调休工作日日期</param>
        public static void AddWorkday(DateTime date)
        {
            _defaultWorkdays.Add(date.Date);
        }

        /// <summary>
        /// 批量添加调休工作日
        /// </summary>
        /// <param name="dates">调休工作日日期集合</param>
        public static void AddWorkdays(IEnumerable<DateTime> dates)
        {
            foreach (var date in dates)
            {
                _defaultWorkdays.Add(date.Date);
            }
        }

        /// <summary>
        /// 移除调休工作日
        /// </summary>
        /// <param name="date">调休工作日日期</param>
        public static void RemoveWorkday(DateTime date)
        {
            _defaultWorkdays.Remove(date.Date);
        }

        /// <summary>
        /// 清空所有节假日和调休工作日配置
        /// </summary>
        public static void ClearAll()
        {
            _defaultHolidays.Clear();
            _defaultWorkdays.Clear();
        }

        /// <summary>
        /// 判断是否为工作日
        /// </summary>
        /// <param name="date">日期</param>
        /// <returns>是否为工作日</returns>
        public static bool IsWorkday(DateTime date)
        {
            return IsWorkday(date, _defaultHolidays, _defaultWorkdays);
        }

        /// <summary>
        /// 判断是否为工作日
        /// </summary>
        /// <param name="date">日期</param>
        /// <param name="holidays">节假日集合</param>
        /// <param name="adjustedWorkdays">调休工作日集合</param>
        /// <returns>是否为工作日</returns>
        public static bool IsWorkday(DateTime date, IEnumerable<DateTime>? holidays = null, IEnumerable<DateTime>? adjustedWorkdays = null)
        {
            var dateOnly = date.Date;
            var holidaySet = holidays?.Select(d => d.Date).ToHashSet() ?? new HashSet<DateTime>();
            var workdaySet = adjustedWorkdays?.Select(d => d.Date).ToHashSet() ?? new HashSet<DateTime>();

            // 如果是调休工作日，返回true
            if (workdaySet.Contains(dateOnly))
                return true;

            // 如果是节假日，返回false
            if (holidaySet.Contains(dateOnly))
                return false;

            // 判断是否为周末
            var dayOfWeek = date.DayOfWeek;
            return dayOfWeek != DayOfWeek.Saturday && dayOfWeek != DayOfWeek.Sunday;
        }

        /// <summary>
        /// 判断是否为周末
        /// </summary>
        /// <param name="date">日期</param>
        /// <returns>是否为周末</returns>
        public static bool IsWeekend(DateTime date)
        {
            var dayOfWeek = date.DayOfWeek;
            return dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday;
        }

        /// <summary>
        /// 判断是否为节假日
        /// </summary>
        /// <param name="date">日期</param>
        /// <returns>是否为节假日</returns>
        public static bool IsHoliday(DateTime date)
        {
            return _defaultHolidays.Contains(date.Date);
        }

        /// <summary>
        /// 计算两个日期之间的工作日数量
        /// </summary>
        /// <param name="startDate">开始日期</param>
        /// <param name="endDate">结束日期</param>
        /// <returns>工作日数量</returns>
        public static int GetWorkdayCount(DateTime startDate, DateTime endDate)
        {
            return GetWorkdayCount(startDate, endDate, _defaultHolidays, _defaultWorkdays);
        }

        /// <summary>
        /// 计算两个日期之间的工作日数量
        /// </summary>
        /// <param name="startDate">开始日期</param>
        /// <param name="endDate">结束日期</param>
        /// <param name="holidays">节假日集合</param>
        /// <param name="adjustedWorkdays">调休工作日集合</param>
        /// <returns>工作日数量</returns>
        public static int GetWorkdayCount(DateTime startDate, DateTime endDate, IEnumerable<DateTime>? holidays = null, IEnumerable<DateTime>? adjustedWorkdays = null)
        {
            if (startDate > endDate)
            {
                var temp = startDate;
                startDate = endDate;
                endDate = temp;
            }

            int count = 0;
            var current = startDate.Date;
            var endDateOnly = endDate.Date;

            while (current <= endDateOnly)
            {
                if (IsWorkday(current, holidays, adjustedWorkdays))
                    count++;
                current = current.AddDays(1);
            }

            return count;
        }

        /// <summary>
        /// 计算指定工作日数后的日期
        /// </summary>
        /// <param name="startDate">开始日期</param>
        /// <param name="workdays">工作日数（正数表示往后，负数表示往前）</param>
        /// <returns>目标日期</returns>
        public static DateTime AddWorkdays(DateTime startDate, int workdays)
        {
            return AddWorkdays(startDate, workdays, _defaultHolidays, _defaultWorkdays);
        }

        /// <summary>
        /// 计算指定工作日数后的日期
        /// </summary>
        /// <param name="startDate">开始日期</param>
        /// <param name="workdays">工作日数（正数表示往后，负数表示往前）</param>
        /// <param name="holidays">节假日集合</param>
        /// <param name="adjustedWorkdays">调休工作日集合</param>
        /// <returns>目标日期</returns>
        public static DateTime AddWorkdays(DateTime startDate, int workdays, IEnumerable<DateTime>? holidays = null, IEnumerable<DateTime>? adjustedWorkdays = null)
        {
            if (workdays == 0)
                return startDate.Date;

            var current = startDate.Date;
            var increment = workdays > 0 ? 1 : -1;
            var remaining = Math.Abs(workdays);

            while (remaining > 0)
            {
                current = current.AddDays(increment);

                if (IsWorkday(current, holidays, adjustedWorkdays))
                    remaining--;
            }

            return current;
        }

        /// <summary>
        /// 获取下一个工作日
        /// </summary>
        /// <param name="date">起始日期</param>
        /// <returns>下一个工作日</returns>
        public static DateTime GetNextWorkday(DateTime date)
        {
            return AddWorkdays(date, 1);
        }

        /// <summary>
        /// 获取上一个工作日
        /// </summary>
        /// <param name="date">起始日期</param>
        /// <returns>上一个工作日</returns>
        public static DateTime GetPreviousWorkday(DateTime date)
        {
            return AddWorkdays(date, -1);
        }

        /// <summary>
        /// 获取指定日期所在周的工作日列表
        /// </summary>
        /// <param name="date">日期</param>
        /// <param name="weekStartsOn">周起始日</param>
        /// <returns>工作日列表</returns>
        public static List<DateTime> GetWorkdaysOfWeek(DateTime date, DayOfWeek weekStartsOn = DayOfWeek.Monday)
        {
            var result = new List<DateTime>();
            var startOfWeek = GetStartOfWeek(date, weekStartsOn);

            for (int i = 0; i < 7; i++)
            {
                var current = startOfWeek.AddDays(i);
                if (IsWorkday(current))
                    result.Add(current);
            }

            return result;
        }

        /// <summary>
        /// 获取指定日期所在月的工作日列表
        /// </summary>
        /// <param name="date">日期</param>
        /// <returns>工作日列表</returns>
        public static List<DateTime> GetWorkdaysOfMonth(DateTime date)
        {
            var result = new List<DateTime>();
            var firstDay = new DateTime(date.Year, date.Month, 1);
            var lastDay = firstDay.AddMonths(1).AddDays(-1);

            var current = firstDay;
            while (current <= lastDay)
            {
                if (IsWorkday(current))
                    result.Add(current);
                current = current.AddDays(1);
            }

            return result;
        }

        /// <summary>
        /// 获取指定日期所在月的工作日数量
        /// </summary>
        /// <param name="year">年份</param>
        /// <param name="month">月份</param>
        /// <returns>工作日数量</returns>
        public static int GetWorkdaysInMonth(int year, int month)
        {
            var firstDay = new DateTime(year, month, 1);
            var lastDay = firstDay.AddMonths(1).AddDays(-1);
            return GetWorkdayCount(firstDay, lastDay);
        }

        /// <summary>
        /// 获取指定日期所在年的工作日数量
        /// </summary>
        /// <param name="year">年份</param>
        /// <returns>工作日数量</returns>
        public static int GetWorkdaysInYear(int year)
        {
            var firstDay = new DateTime(year, 1, 1);
            var lastDay = new DateTime(year, 12, 31);
            return GetWorkdayCount(firstDay, lastDay);
        }

        /// <summary>
        /// 获取指定日期所在周的第一天
        /// </summary>
        /// <param name="date">日期</param>
        /// <param name="weekStartsOn">周起始日</param>
        /// <returns>周的第一天</returns>
        public static DateTime GetStartOfWeek(DateTime date, DayOfWeek weekStartsOn = DayOfWeek.Monday)
        {
            var diff = (7 + (date.DayOfWeek - weekStartsOn)) % 7;
            return date.Date.AddDays(-diff);
        }

        /// <summary>
        /// 获取指定日期所在周的最后一天
        /// </summary>
        /// <param name="date">日期</param>
        /// <param name="weekStartsOn">周起始日</param>
        /// <returns>周的最后一天</returns>
        public static DateTime GetEndOfWeek(DateTime date, DayOfWeek weekStartsOn = DayOfWeek.Monday)
        {
            return GetStartOfWeek(date, weekStartsOn).AddDays(6);
        }

        /// <summary>
        /// 计算工作日区间（返回所有工作日）
        /// </summary>
        /// <param name="startDate">开始日期</param>
        /// <param name="endDate">结束日期</param>
        /// <returns>工作日列表</returns>
        public static List<DateTime> GetWorkdaysBetween(DateTime startDate, DateTime endDate)
        {
            var result = new List<DateTime>();

            if (startDate > endDate)
            {
                var temp = startDate;
                startDate = endDate;
                endDate = temp;
            }

            var current = startDate.Date;
            while (current <= endDate.Date)
            {
                if (IsWorkday(current))
                    result.Add(current);
                current = current.AddDays(1);
            }

            return result;
        }

        /// <summary>
        /// 判断是否为同一天
        /// </summary>
        /// <param name="date1">日期1</param>
        /// <param name="date2">日期2</param>
        /// <returns>是否为同一天</returns>
        public static bool IsSameDay(DateTime date1, DateTime date2)
        {
            return date1.Date == date2.Date;
        }

        /// <summary>
        /// 判断两个日期是否为同一周
        /// </summary>
        /// <param name="date1">日期1</param>
        /// <param name="date2">日期2</param>
        /// <param name="weekStartsOn">周起始日</param>
        /// <returns>是否为同一周</returns>
        public static bool IsSameWeek(DateTime date1, DateTime date2, DayOfWeek weekStartsOn = DayOfWeek.Monday)
        {
            return GetStartOfWeek(date1, weekStartsOn) == GetStartOfWeek(date2, weekStartsOn);
        }

        /// <summary>
        /// 判断两个日期是否为同一月
        /// </summary>
        /// <param name="date1">日期1</param>
        /// <param name="date2">日期2</param>
        /// <returns>是否为同一月</returns>
        public static bool IsSameMonth(DateTime date1, DateTime date2)
        {
            return date1.Year == date2.Year && date1.Month == date2.Month;
        }

        /// <summary>
        /// 判断两个日期是否为同一年
        /// </summary>
        /// <param name="date1">日期1</param>
        /// <param name="date2">日期2</param>
        /// <returns>是否为同一年</returns>
        public static bool IsSameYear(DateTime date1, DateTime date2)
        {
            return date1.Year == date2.Year;
        }

        /// <summary>
        /// 获取第n个工作日
        /// </summary>
        /// <param name="year">年份</param>
        /// <param name="month">月份</param>
        /// <param name="n">第n个工作日（从1开始）</param>
        /// <returns>工作日日期</returns>
        public static DateTime GetNthWorkdayOfMonth(int year, int month, int n)
        {
            if (n < 1)
                throw new ArgumentException("n必须大于0", nameof(n));

            var workdays = GetWorkdaysOfMonth(new DateTime(year, month, 1));

            if (n > workdays.Count)
                throw new ArgumentException($"该月只有{workdays.Count}个工作日", nameof(n));

            return workdays[n - 1];
        }

        /// <summary>
        /// 获取日期在当月中的第几个工作日
        /// </summary>
        /// <param name="date">日期</param>
        /// <returns>第几个工作日（从1开始），如果不是工作日返回-1</returns>
        public static int GetWorkdayIndexInMonth(DateTime date)
        {
            if (!IsWorkday(date))
                return -1;

            var workdays = GetWorkdaysOfMonth(date);
            return workdays.FindIndex(d => d.Date == date.Date) + 1;
        }
    }
}
