using System;
using System.Collections.Generic;

namespace EasyTool.DateTimeCategory
{
    /// <summary>
    /// 农历日历工具类
    /// 提供公历与农历之间的转换
    /// </summary>
    public static class LunarCalendarUtil
    {
        // 农历数据 1900-2100年
        // 每个数据表示一年，包含：月份天数信息、闰月信息
        private static readonly uint[] LunarInfo = {
            0x04bd8, 0x04ae0, 0x0a570, 0x054d5, 0x0d260, 0x0d950, 0x16554, 0x056a0, 0x09ad0, 0x055d2,
            0x04ae0, 0x0a5b6, 0x0a4d0, 0x0d250, 0x1d255, 0x0b540, 0x0d6a0, 0x0ada2, 0x095b0, 0x14977,
            0x04970, 0x0a4b0, 0x0b4b5, 0x06a50, 0x06d40, 0x1ab54, 0x02b60, 0x09570, 0x052f2, 0x04970,
            0x06566, 0x0d4a0, 0x0ea50, 0x06e95, 0x05ad0, 0x02b60, 0x186e3, 0x092e0, 0x1c8d7, 0x0c950,
            0x0d4a0, 0x1d8a6, 0x0b550, 0x056a0, 0x1a5b4, 0x025d0, 0x092d0, 0x0d2b2, 0x0a950, 0x0b557,
            0x06ca0, 0x0b550, 0x15355, 0x04da0, 0x0a5b0, 0x14573, 0x052b0, 0x0a9a8, 0x0e950, 0x06aa0,
            0x0aea6, 0x0ab50, 0x04b60, 0x0aae4, 0x0a570, 0x05260, 0x0f263, 0x0d950, 0x05b57, 0x056a0,
            0x096d0, 0x04dd5, 0x04ad0, 0x0a4d0, 0x0d4d4, 0x0d250, 0x0d558, 0x0b540, 0x0b6a0, 0x195a6,
            0x095b0, 0x049b0, 0x0a974, 0x0a4b0, 0x0b27a, 0x06a50, 0x06d40, 0x0af46, 0x0ab60, 0x09570,
            0x04af5, 0x04970, 0x064b0, 0x074a3, 0x0ea50, 0x06b58, 0x055c0, 0x0ab60, 0x096d5, 0x092e0,
            0x0c960, 0x0d954, 0x0d4a0, 0x0da50, 0x07552, 0x056a0, 0x0abb7, 0x025d0, 0x092d0, 0x0cab5,
            0x0a950, 0x0b4a0, 0x0baa4, 0x0ad50, 0x055d9, 0x04ba0, 0x0a5b0, 0x15176, 0x052b0, 0x0a930,
            0x07954, 0x06aa0, 0x0ad50, 0x05b52, 0x04b60, 0x0a6e6, 0x0a4e0, 0x0d260, 0x0ea65, 0x0d530,
            0x05aa0, 0x076a3, 0x096d0, 0x04afb, 0x04ad0, 0x0a4d0, 0x1d0b6, 0x0d250, 0x0d520, 0x0dd45,
            0x0b5a0, 0x056d0, 0x055b2, 0x049b0, 0x0a577, 0x0a4b0, 0x0aa50, 0x1b255, 0x06d20, 0x0ada0,
            0x14b63, 0x09370, 0x049f8, 0x04970, 0x064b0, 0x168a6, 0x0ea50, 0x06b20, 0x1a6c4, 0x0aae0,
            0x0a2e0, 0x0d2e3, 0x0c960, 0x0d557, 0x0d4a0, 0x0da50, 0x05d55, 0x056a0, 0x0a6d0, 0x055d4,
            0x052d0, 0x0a9b8, 0x0a950, 0x0b4a0, 0x0b6a6, 0x0ad50, 0x055a0, 0x0aba4, 0x0a5b0, 0x052b0,
            0x0b273, 0x06930, 0x07337, 0x06aa0, 0x0ad50, 0x14b55, 0x04b60, 0x0a570, 0x054e4, 0x0d160,
            0x0e968, 0x0d520, 0x0daa0, 0x16aa6, 0x056d0, 0x04ae0, 0x0a9d4, 0x0a2d0, 0x0d150, 0x0f252,
            0x0d520
        };

        // 天干
        private static readonly string[] TianGan = { "甲", "乙", "丙", "丁", "戊", "己", "庚", "辛", "壬", "癸" };

        // 地支
        private static readonly string[] DiZhi = { "子", "丑", "寅", "卯", "辰", "巳", "午", "未", "申", "酉", "戌", "亥" };

        // 生肖
        private static readonly string[] ShengXiao = { "鼠", "牛", "虎", "兔", "龙", "蛇", "马", "羊", "猴", "鸡", "狗", "猪" };

        // 农历月份
        private static readonly string[] LunarMonths = { "正", "二", "三", "四", "五", "六", "七", "八", "九", "十", "冬", "腊" };

        // 农历日期
        private static readonly string[] LunarDays = {
            "初一", "初二", "初三", "初四", "初五", "初六", "初七", "初八", "初九", "初十",
            "十一", "十二", "十三", "十四", "十五", "十六", "十七", "十八", "十九", "二十",
            "廿一", "廿二", "廿三", "廿四", "廿五", "廿六", "廿七", "廿八", "廿九", "三十"
        };

        /// <summary>
        /// 公历转农历
        /// </summary>
        /// <param name="date">公历日期</param>
        /// <returns>农历信息</returns>
        public static LunarDate SolarToLunar(DateTime date)
        {
            if (date.Year < 1900 || date.Year > 2100)
                throw new ArgumentOutOfRangeException(nameof(date), "日期范围必须在 1900-2100 年之间");

            // 计算与 1900 年 1 月 31 日（农历 1900 年正月初一）的天数差
            var baseDate = new DateTime(1900, 1, 31);
            var offset = (int)(date - baseDate).TotalDays;

            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(date), "日期必须在 1900 年 1 月 31 日之后");

            // 计算农历年
            int year = 1900;
            int daysOfYear;
            while (year < 2100 && offset > 0)
            {
                daysOfYear = GetLunarYearDays(year);
                if (offset < daysOfYear)
                    break;

                offset -= daysOfYear;
                year++;
            }

            // 计算农历月和日
            var yearInfo = LunarInfo[year - 1900];
            var leapMonth = (int)(yearInfo & 0xf); // 闰月
            var isLeap = false;
            int month = 1;
            int daysOfMonth;

            while (month <= 12 && offset > 0)
            {
                daysOfMonth = GetLunarMonthDays(year, month, false);
                if (offset < daysOfMonth)
                    break;

                offset -= daysOfMonth;

                // 检查是否有闰月
                if (leapMonth == month && !isLeap)
                {
                    isLeap = true;
                    daysOfMonth = GetLunarMonthDays(year, month, true);
                    if (offset < daysOfMonth)
                        break;

                    offset -= daysOfMonth;
                    isLeap = false;
                }

                month++;
            }

            return new LunarDate
            {
                Year = year,
                Month = month,
                Day = offset + 1,
                IsLeapMonth = isLeap,
                YearString = GetYearString(year),
                MonthString = GetMonthString(month, isLeap),
                DayString = LunarDays[offset],
                GanZhiYear = GetGanZhiYear(year),
                GanZhiMonth = GetGanZhiMonth(year, month),
                GanZhiDay = GetGanZhiDay(date),
                ShengXiao = GetShengXiao(year)
            };
        }

        /// <summary>
        /// 农历转公历
        /// </summary>
        /// <param name="year">农历年</param>
        /// <param name="month">农历月</param>
        /// <param name="day">农历日</param>
        /// <param name="isLeapMonth">是否闰月</param>
        /// <returns>公历日期</returns>
        public static DateTime LunarToSolar(int year, int month, int day, bool isLeapMonth = false)
        {
            if (year < 1900 || year > 2100)
                throw new ArgumentOutOfRangeException(nameof(year), "年份必须在 1900-2100 年之间");

            var baseDate = new DateTime(1900, 1, 31);
            var offset = 0;

            // 计算年份偏移
            for (int y = 1900; y < year; y++)
            {
                offset += GetLunarYearDays(y);
            }

            // 计算月份偏移
            var yearInfo = LunarInfo[year - 1900];
            var leapMonth = (int)(yearInfo & 0xf);

            for (int m = 1; m < month; m++)
            {
                offset += GetLunarMonthDays(year, m, false);

                // 如果是闰月之前的月份，还要加上闰月的天数
                if (m == leapMonth && !isLeapMonth)
                {
                    offset += GetLunarMonthDays(year, m, true);
                }
            }

            // 如果是闰月，加上正常月的天数
            if (isLeapMonth)
            {
                offset += GetLunarMonthDays(year, month, false);
            }

            // 加上日期偏移
            offset += day - 1;

            return baseDate.AddDays(offset);
        }

        /// <summary>
        /// 获取农历年份的天数
        /// </summary>
        public static int GetLunarYearDays(int year)
        {
            var yearInfo = LunarInfo[year - 1900];
            var leapMonth = (int)(yearInfo & 0xf);
            var leapDays = leapMonth > 0 ? GetLunarMonthDays(year, leapMonth, true) : 0;

            var days = 0;
            for (int i = 0x8000; i > 0x8; i >>= 1)
            {
                days += (yearInfo & i) != 0 ? 30 : 29;
            }

            return days + leapDays;
        }

        /// <summary>
        /// 获取农历月份的天数
        /// </summary>
        public static int GetLunarMonthDays(int year, int month, bool isLeap)
        {
            var yearInfo = LunarInfo[year - 1900];

            if (isLeap)
            {
                return (yearInfo & 0x10000) != 0 ? 30 : 29;
            }

            var bit = 0x8000 >> (month - 1);
            return (yearInfo & bit) != 0 ? 30 : 29;
        }

        /// <summary>
        /// 获取闰月（0 表示没有闰月）
        /// </summary>
        public static int GetLeapMonth(int year)
        {
            var yearInfo = LunarInfo[year - 1900];
            return (int)(yearInfo & 0xf);
        }

        /// <summary>
        /// 获取干支年
        /// </summary>
        public static string GetGanZhiYear(int year)
        {
            var ganIndex = (year - 4) % 10;
            var zhiIndex = (year - 4) % 12;

            if (ganIndex < 0) ganIndex += 10;
            if (zhiIndex < 0) zhiIndex += 12;

            return TianGan[ganIndex] + DiZhi[zhiIndex];
        }

        /// <summary>
        /// 获取干支月
        /// </summary>
        public static string GetGanZhiMonth(int year, int month)
        {
            var ganIndex = (year * 12 + month + 13) % 10;
            var zhiIndex = (month + 1) % 12;

            return TianGan[ganIndex] + DiZhi[zhiIndex];
        }

        /// <summary>
        /// 获取干支日
        /// </summary>
        public static string GetGanZhiDay(DateTime date)
        {
            var baseDate = new DateTime(1900, 1, 31);
            var offset = (int)(date - baseDate).TotalDays;

            var ganIndex = (offset + 10) % 10;
            var zhiIndex = (offset + 12) % 12;

            return TianGan[ganIndex] + DiZhi[zhiIndex];
        }

        /// <summary>
        /// 获取生肖
        /// </summary>
        public static string GetShengXiao(int year)
        {
            var index = (year - 4) % 12;
            if (index < 0) index += 12;
            return ShengXiao[index];
        }

        /// <summary>
        /// 获取生肖（GetShengXiao 的别名）
        /// </summary>
        /// <param name="date">日期</param>
        /// <returns>生肖</returns>
        public static string GetChineseZodiac(DateTime date)
        {
            return GetShengXiao(date.Year);
        }

        /// <summary>
        /// 获取年份字符串
        /// </summary>
        private static string GetYearString(int year)
        {
            var digits = new[] { "〇", "一", "二", "三", "四", "五", "六", "七", "八", "九" };
            var result = "";
            while (year > 0)
            {
                result = digits[year % 10] + result;
                year /= 10;
            }
            return result;
        }

        /// <summary>
        /// 获取月份字符串
        /// </summary>
        private static string GetMonthString(int month, bool isLeap)
        {
            return (isLeap ? "闰" : "") + LunarMonths[month - 1] + "月";
        }

        /// <summary>
        /// 获取中国传统节日
        /// </summary>
        public static List<LunarFestival> GetLunarFestivals(int year)
        {
            return new List<LunarFestival>
            {
                new LunarFestival { Name = "春节", Month = 1, Day = 1, Description = "农历新年" },
                new LunarFestival { Name = "元宵节", Month = 1, Day = 15, Description = "正月十五" },
                new LunarFestival { Name = "端午节", Month = 5, Day = 5, Description = "五月初五" },
                new LunarFestival { Name = "七夕节", Month = 7, Day = 7, Description = "七月初七" },
                new LunarFestival { Name = "中元节", Month = 7, Day = 15, Description = "七月十五" },
                new LunarFestival { Name = "中秋节", Month = 8, Day = 15, Description = "八月十五" },
                new LunarFestival { Name = "重阳节", Month = 9, Day = 9, Description = "九月初九" },
                new LunarFestival { Name = "腊八节", Month = 12, Day = 8, Description = "腊月初八" },
                new LunarFestival { Name = "除夕", Month = 12, Day = 30, Description = "腊月最后一天" }
            };
        }

        /// <summary>
        /// 判断是否是节日
        /// </summary>
        public static string? GetFestivalName(int lunarMonth, int lunarDay)
        {
            return (lunarMonth, lunarDay) switch
            {
                (1, 1) => "春节",
                (1, 15) => "元宵节",
                (5, 5) => "端午节",
                (7, 7) => "七夕节",
                (7, 15) => "中元节",
                (8, 15) => "中秋节",
                (9, 9) => "重阳节",
                (12, 8) => "腊八节",
                (12, 30) => "除夕",
                _ => null
            };
        }
    }

    /// <summary>
    /// 农历日期
    /// </summary>
    public class LunarDate
    {
        /// <summary>
        /// 年
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// 月
        /// </summary>
        public int Month { get; set; }

        /// <summary>
        /// 日
        /// </summary>
        public int Day { get; set; }

        /// <summary>
        /// 是否闰月
        /// </summary>
        public bool IsLeapMonth { get; set; }

        /// <summary>
        /// 年份字符串（中文）
        /// </summary>
        public string YearString { get; set; } = string.Empty;

        /// <summary>
        /// 月份字符串（中文）
        /// </summary>
        public string MonthString { get; set; } = string.Empty;

        /// <summary>
        /// 日期字符串（中文）
        /// </summary>
        public string DayString { get; set; } = string.Empty;

        /// <summary>
        /// 干支年
        /// </summary>
        public string GanZhiYear { get; set; } = string.Empty;

        /// <summary>
        /// 干支月
        /// </summary>
        public string GanZhiMonth { get; set; } = string.Empty;

        /// <summary>
        /// 干支日
        /// </summary>
        public string GanZhiDay { get; set; } = string.Empty;

        /// <summary>
        /// 生肖
        /// </summary>
        public string ShengXiao { get; set; } = string.Empty;

        /// <summary>
        /// 完整日期字符串
        /// </summary>
        public string FullString => $"{YearString}年{MonthString}{DayString}";

        /// <summary>
        /// 干支日期字符串
        /// </summary>
        public string GanZhiString => $"{GanZhiYear}年{GanZhiMonth}月{GanZhiDay}日";

        public override string ToString()
        {
            return $"{FullString}（{GanZhiString}）{ShengXiao}年";
        }
    }

    /// <summary>
    /// 农历节日
    /// </summary>
    public class LunarFestival
    {
        /// <summary>
        /// 节日名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 农历月
        /// </summary>
        public int Month { get; set; }

        /// <summary>
        /// 农历日
        /// </summary>
        public int Day { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; } = string.Empty;
    }
}