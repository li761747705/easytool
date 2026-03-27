using System;
using System.Collections.Generic;

namespace EasyTool.DateTimeCategory
{
    /// <summary>
    /// 节假日工具类
    /// </summary>
    public static class HolidayUtil
    {
        /// <summary>
        /// 获取指定年份的中国法定节假日
        /// </summary>
        public static List<DateTime> GetChineseHolidays(int year)
        {
            var holidays = new List<DateTime>();

            // 元旦
            holidays.Add(new DateTime(year, 1, 1));

            // 春节（简化处理，实际需要根据农历计算）
            holidays.Add(new DateTime(year, 1, 1));
            holidays.Add(new DateTime(year, 1, 2));
            holidays.Add(new DateTime(year, 1, 3));

            // 清明节（4月4日或5日）
            holidays.Add(GetQingmingDate(year));

            // 劳动节
            holidays.Add(new DateTime(year, 5, 1));
            holidays.Add(new DateTime(year, 5, 2));
            holidays.Add(new DateTime(year, 5, 3));

            // 端午节（简化处理）
            holidays.Add(new DateTime(year, 6, 1));

            // 中秋节（简化处理）
            holidays.Add(new DateTime(year, 9, 15));

            // 国庆节
            holidays.Add(new DateTime(year, 10, 1));
            holidays.Add(new DateTime(year, 10, 2));
            holidays.Add(new DateTime(year, 10, 3));
            holidays.Add(new DateTime(year, 10, 4));
            holidays.Add(new DateTime(year, 10, 5));
            holidays.Add(new DateTime(year, 10, 6));
            holidays.Add(new DateTime(year, 10, 7));

            return holidays;
        }

        /// <summary>
        /// 获取清明节日期
        /// </summary>
        private static DateTime GetQingmingDate(int year)
        {
            // 清明节通常在4月4日或5日
            var day = 5;
            if (year % 4 == 0 && (year % 100 != 0 || year % 400 == 0))
            {
                day = 4;
            }
            return new DateTime(year, 4, day);
        }

        /// <summary>
        /// 判断是否为工作日
        /// </summary>
        public static bool IsWorkday(DateTime date, List<DateTime>? holidays = null, List<DateTime>? workdays = null)
        {
            // 检查是否为调休工作日
            if (workdays != null && workdays.Contains(date.Date))
                return true;

            // 检查是否为假日
            if (holidays != null && holidays.Contains(date.Date))
                return false;

            // 周一至周五为工作日
            return date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday;
        }

        /// <summary>
        /// 判断是否为周末
        /// </summary>
        public static bool IsWeekend(DateTime date)
        {
            return date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
        }

        /// <summary>
        /// 获取下一个工作日
        /// </summary>
        public static DateTime GetNextWorkday(DateTime date, List<DateTime>? holidays = null, List<DateTime>? workdays = null)
        {
            var next = date.AddDays(1);
            while (!IsWorkday(next, holidays, workdays))
            {
                next = next.AddDays(1);
            }
            return next;
        }

        /// <summary>
        /// 获取上一个工作日
        /// </summary>
        public static DateTime GetPreviousWorkday(DateTime date, List<DateTime>? holidays = null, List<DateTime>? workdays = null)
        {
            var prev = date.AddDays(-1);
            while (!IsWorkday(prev, holidays, workdays))
            {
                prev = prev.AddDays(-1);
            }
            return prev;
        }

        /// <summary>
        /// 计算工作日数量
        /// </summary>
        public static int CountWorkdays(DateTime start, DateTime end, List<DateTime>? holidays = null, List<DateTime>? workdays = null)
        {
            var count = 0;
            var current = start.Date;

            while (current <= end.Date)
            {
                if (IsWorkday(current, holidays, workdays))
                    count++;
                current = current.AddDays(1);
            }

            return count;
        }

        /// <summary>
        /// 添加工作日
        /// </summary>
        public static DateTime AddWorkdays(DateTime date, int days, List<DateTime>? holidays = null, List<DateTime>? workdays = null)
        {
            var result = date;
            var increment = days > 0 ? 1 : -1;
            var remaining = Math.Abs(days);

            while (remaining > 0)
            {
                result = result.AddDays(increment);
                if (IsWorkday(result, holidays, workdays))
                    remaining--;
            }

            return result;
        }

        /// <summary>
        /// 获取西式节日
        /// </summary>
        public static DateTime GetWesternHoliday(int year, WesternHoliday holiday)
        {
            return holiday switch
            {
                WesternHoliday.NewYear => new DateTime(year, 1, 1),
                WesternHoliday.ValentinesDay => new DateTime(year, 2, 14),
                WesternHoliday.StPatricksDay => new DateTime(year, 3, 17),
                WesternHoliday.AprilFools => new DateTime(year, 4, 1),
                WesternHoliday.IndependenceDay => new DateTime(year, 7, 4),
                WesternHoliday.Halloween => new DateTime(year, 10, 31),
                WesternHoliday.VeteransDay => new DateTime(year, 11, 11),
                WesternHoliday.Christmas => new DateTime(year, 12, 25),
                WesternHoliday.Thanksgiving => GetNthDayOfWeek(year, 11, DayOfWeek.Thursday, 4),
                WesternHoliday.MothersDay => GetNthDayOfWeek(year, 5, DayOfWeek.Sunday, 2),
                WesternHoliday.FathersDay => GetNthDayOfWeek(year, 6, DayOfWeek.Sunday, 3),
                WesternHoliday.LaborDay => GetNthDayOfWeek(year, 9, DayOfWeek.Monday, 1),
                WesternHoliday.MemorialDay => GetLastDayOfWeek(year, 5, DayOfWeek.Monday),
                _ => throw new ArgumentOutOfRangeException(nameof(holiday))
            };
        }

        /// <summary>
        /// 获取某月第N个星期几
        /// </summary>
        private static DateTime GetNthDayOfWeek(int year, int month, DayOfWeek dayOfWeek, int n)
        {
            var firstDay = new DateTime(year, month, 1);
            var daysToAdd = ((int)dayOfWeek - (int)firstDay.DayOfWeek + 7) % 7;
            var result = firstDay.AddDays(daysToAdd + (n - 1) * 7);
            return result;
        }

        /// <summary>
        /// 获取某月最后一个星期几
        /// </summary>
        private static DateTime GetLastDayOfWeek(int year, int month, DayOfWeek dayOfWeek)
        {
            var lastDay = new DateTime(year, month, DateTime.DaysInMonth(year, month));
            var daysToSubtract = ((int)lastDay.DayOfWeek - (int)dayOfWeek + 7) % 7;
            return lastDay.AddDays(-daysToSubtract);
        }
    }

    /// <summary>
    /// 西式节日
    /// </summary>
    public enum WesternHoliday
    {
        /// <summary>
        /// 元旦
        /// </summary>
        NewYear,

        /// <summary>
        /// 情人节
        /// </summary>
        ValentinesDay,

        /// <summary>
        /// 圣帕特里克节
        /// </summary>
        StPatricksDay,

        /// <summary>
        /// 愚人节
        /// </summary>
        AprilFools,

        /// <summary>
        /// 美国独立日
        /// </summary>
        IndependenceDay,

        /// <summary>
        /// 万圣节
        /// </summary>
        Halloween,

        /// <summary>
        /// 退伍军人节
        /// </summary>
        VeteransDay,

        /// <summary>
        /// 圣诞节
        /// </summary>
        Christmas,

        /// <summary>
        /// 感恩节
        /// </summary>
        Thanksgiving,

        /// <summary>
        /// 母亲节
        /// </summary>
        MothersDay,

        /// <summary>
        /// 父亲节
        /// </summary>
        FathersDay,

        /// <summary>
        /// 劳动节（美国）
        /// </summary>
        LaborDay,

        /// <summary>
        /// 阵亡将士纪念日（美国）
        /// </summary>
        MemorialDay
    }
}