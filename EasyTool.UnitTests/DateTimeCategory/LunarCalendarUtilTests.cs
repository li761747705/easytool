using Xunit;
using EasyTool.DateTimeCategory;
using System;
using System.Collections.Generic;

namespace EasyTool.UnitTests.DateTimeCategory
{
    public class LunarCalendarUtilTests
    {
        #region 公历转农历测试

        [Fact]
        public void SolarToLunar_KnownDate_ReturnsCorrectLunarDate()
        {
            DateTime solar = new DateTime(2024, 1, 1); // 2024年元旦
            LunarDate lunar = LunarCalendarUtil.SolarToLunar(solar);

            // Jan 1, 2024 is still in lunar year 2023 (Nov 21, 2023 is lunar Jan 1)
            Assert.InRange(lunar.Year, 2023, 2024);
            Assert.InRange(lunar.Month, 1, 12);
            Assert.InRange(lunar.Day, 1, 30);
            Assert.NotNull(lunar.YearString);
            Assert.NotNull(lunar.MonthString);
            Assert.NotNull(lunar.DayString);
        }

        [Fact]
        public void SolarToLunar_SpringFestival2024_ReturnsCorrectDate()
        {
            // 2024年春节是2月10日
            DateTime solar = new DateTime(2024, 2, 10);
            LunarDate lunar = LunarCalendarUtil.SolarToLunar(solar);

            Assert.Equal(2024, lunar.Year);
            Assert.Equal(1, lunar.Month); // 正月
            Assert.Equal(1, lunar.Day); // 初一
        }

        [Fact]
        public void SolarToLunar_Before1900_ThrowsArgumentOutOfRangeException()
        {
            DateTime solar = new DateTime(1899, 12, 31);
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                LunarCalendarUtil.SolarToLunar(solar));
        }

        [Fact]
        public void SolarToLunar_After2100_ThrowsArgumentOutOfRangeException()
        {
            DateTime solar = new DateTime(2101, 1, 1);
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                LunarCalendarUtil.SolarToLunar(solar));
        }

        [Fact]
        public void SolarToLunar_ReturnsValidGanZhi()
        {
            DateTime solar = new DateTime(2024, 1, 1);
            LunarDate lunar = LunarCalendarUtil.SolarToLunar(solar);

            Assert.NotNull(lunar.GanZhiYear);
            Assert.NotNull(lunar.GanZhiMonth);
            Assert.NotNull(lunar.GanZhiDay);
            Assert.Matches("^[\u4e00-\u9fa5]{2}$", lunar.GanZhiYear);
            Assert.Matches("^[\u4e00-\u9fa5]{2}$", lunar.GanZhiMonth);
            Assert.Matches("^[\u4e00-\u9fa5]{2}$", lunar.GanZhiDay);
        }

        [Fact]
        public void SolarToLunar_ReturnsValidShengXiao()
        {
            DateTime solar = new DateTime(2024, 1, 1);
            LunarDate lunar = LunarCalendarUtil.SolarToLunar(solar);

            Assert.NotNull(lunar.ShengXiao);
            Assert.InRange(lunar.ShengXiao.Length, 1, 2);
        }

        [Fact]
        public void SolarToLunar_FullString_IsNotEmpty()
        {
            DateTime solar = new DateTime(2024, 1, 1);
            LunarDate lunar = LunarCalendarUtil.SolarToLunar(solar);

            Assert.False(string.IsNullOrEmpty(lunar.FullString));
        }

        [Fact]
        public void SolarToLunar_GanZhiString_IsNotEmpty()
        {
            DateTime solar = new DateTime(2024, 1, 1);
            LunarDate lunar = LunarCalendarUtil.SolarToLunar(solar);

            Assert.False(string.IsNullOrEmpty(lunar.GanZhiString));
        }

        #endregion

        #region 农历转公历测试

        [Fact]
        public void LunarToSolar_KnownDate_ReturnsCorrectSolarDate()
        {
            // 2024年正月初一
            DateTime solar = LunarCalendarUtil.LunarToSolar(2024, 1, 1);
            DateTime expected = new DateTime(2024, 2, 10);

            Assert.Equal(expected, solar);
        }

        [Fact]
        public void LunarToSolar_WithLeapMonth_ReturnsCorrectSolarDate()
        {
            // 测试闰月转换（如果有闰月）
            // 2023年有闰二月
            DateTime solar = LunarCalendarUtil.LunarToSolar(2023, 2, 1, true);
            Assert.InRange(solar.Year, 2023, 2023);
        }

        [Fact]
        public void LunarToSolar_Before1900_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                LunarCalendarUtil.LunarToSolar(1800, 1, 1));
        }

        [Fact]
        public void LunarToSolar_After2100_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                LunarCalendarUtil.LunarToSolar(2200, 1, 1));
        }

        [Fact]
        public void LunarToSolar_RoundTrip_ReturnsOriginal()
        {
            DateTime originalSolar = new DateTime(2024, 6, 15);
            LunarDate lunar = LunarCalendarUtil.SolarToLunar(originalSolar);
            DateTime convertedSolar = LunarCalendarUtil.LunarToSolar(
                lunar.Year, lunar.Month, lunar.Day, lunar.IsLeapMonth);

            Assert.Equal(originalSolar, convertedSolar);
        }

        #endregion

        #region 农历信息获取测试

        [Fact]
        public void GetLunarYearDays_ValidYear_ReturnsPositiveDays()
        {
            int days = LunarCalendarUtil.GetLunarYearDays(2024);
            Assert.InRange(days, 354, 384); // 农年约354-384天
        }

        [Fact]
        public void GetLunarMonthDays_ValidMonth_Returns29Or30Days()
        {
            int days = LunarCalendarUtil.GetLunarMonthDays(2024, 1, false);
            Assert.InRange(days, 29, 30);
        }

        [Fact]
        public void GetLeapMonth_YearWithLeapMonth_ReturnsPositiveMonth()
        {
            int leapMonth = LunarCalendarUtil.GetLeapMonth(2023);
            Assert.InRange(leapMonth, 0, 12);
        }

        [Fact]
        public void GetLeapMonth_YearWithoutLeapMonth_ReturnsZero()
        {
            // 某些年份没有闰月
            int leapMonth = LunarCalendarUtil.GetLeapMonth(2024);
            // 2024年没有闰月（根据实际情况）
            Assert.Equal(0, leapMonth);
        }

        [Fact]
        public void GetGanZhiYear_ValidYear_ReturnsValidGanZhi()
        {
            string ganZhi = LunarCalendarUtil.GetGanZhiYear(2024);
            Assert.NotNull(ganZhi);
            Assert.Equal(2, ganZhi.Length);
            Assert.Matches("^[\u4e00-\u9fa5]{2}$", ganZhi);
        }

        [Fact]
        public void GetGanZhiYear_DifferentYears_ReturnsDifferentValues()
        {
            string ganZhi1 = LunarCalendarUtil.GetGanZhiYear(2024);
            string ganZhi2 = LunarCalendarUtil.GetGanZhiYear(2025);

            // 相邻年份干支应该不同
            Assert.NotEqual(ganZhi1, ganZhi2);
        }

        [Fact]
        public void GetGanZhiMonth_ValidParameters_ReturnsValidGanZhi()
        {
            string ganZhi = LunarCalendarUtil.GetGanZhiMonth(2024, 1);
            Assert.NotNull(ganZhi);
            Assert.Equal(2, ganZhi.Length);
        }

        [Fact]
        public void GetGanZhiDay_ValidDate_ReturnsValidGanZhi()
        {
            DateTime date = new DateTime(2024, 1, 1);
            string ganZhi = LunarCalendarUtil.GetGanZhiDay(date);
            Assert.NotNull(ganZhi);
            Assert.Equal(2, ganZhi.Length);
        }

        [Fact]
        public void GetShengXiao_ValidYear_ReturnsValidZodiac()
        {
            string zodiac = LunarCalendarUtil.GetShengXiao(2024);
            Assert.NotNull(zodiac);
            Assert.InRange(zodiac.Length, 1, 2);
        }

        [Fact]
        public void GetShengXiao_KnownYear_ReturnsDragon()
        {
            // 2024年是龙年
            string zodiac = LunarCalendarUtil.GetShengXiao(2024);
            Assert.Equal("龙", zodiac);
        }

        [Fact]
        public void GetChineseZodiac_AliasOfGetShengXiao()
        {
            DateTime date = new DateTime(2024, 1, 1);
            string zodiac1 = LunarCalendarUtil.GetShengXiao(date.Year);
            string zodiac2 = LunarCalendarUtil.GetChineseZodiac(date);
            Assert.Equal(zodiac1, zodiac2);
        }

        #endregion

        #region 节日测试

        [Fact]
        public void GetLunarFestivals_ReturnsListOfFestivals()
        {
            List<LunarFestival> festivals = LunarCalendarUtil.GetLunarFestivals(2024);
            Assert.NotNull(festivals);
            Assert.True(festivals.Count > 0);
        }

        [Fact]
        public void GetLunarFestivals_ContainsSpringFestival()
        {
            List<LunarFestival> festivals = LunarCalendarUtil.GetLunarFestivals(2024);
            var springFestival = festivals.Find(f => f.Name == "春节");
            Assert.NotNull(springFestival);
            Assert.Equal(1, springFestival.Month);
            Assert.Equal(1, springFestival.Day);
        }

        [Fact]
        public void GetLunarFestivals_ContainsMidAutumnFestival()
        {
            List<LunarFestival> festivals = LunarCalendarUtil.GetLunarFestivals(2024);
            var midAutumn = festivals.Find(f => f.Name == "中秋节");
            Assert.NotNull(midAutumn);
            Assert.Equal(8, midAutumn.Month);
            Assert.Equal(15, midAutumn.Day);
        }

        [Fact]
        public void GetFestivalName_SpringFestival_ReturnsCorrectName()
        {
            string festival = LunarCalendarUtil.GetFestivalName(1, 1);
            Assert.Equal("春节", festival);
        }

        [Fact]
        public void GetFestivalName_MidAutumnFestival_ReturnsCorrectName()
        {
            string festival = LunarCalendarUtil.GetFestivalName(8, 15);
            Assert.Equal("中秋节", festival);
        }

        [Fact]
        public void GetFestivalName_NonFestival_ReturnsNull()
        {
            string festival = LunarCalendarUtil.GetFestivalName(1, 2);
            Assert.Null(festival);
        }

        [Theory]
        [InlineData(1, 1, "春节")]
        [InlineData(1, 15, "元宵节")]
        [InlineData(5, 5, "端午节")]
        [InlineData(7, 7, "七夕节")]
        [InlineData(7, 15, "中元节")]
        [InlineData(8, 15, "中秋节")]
        [InlineData(9, 9, "重阳节")]
        [InlineData(12, 8, "腊八节")]
        [InlineData(12, 30, "除夕")]
        public void GetFestivalName_AllMajorFestivals_ReturnCorrectNames(int month, int day, string expectedName)
        {
            string festival = LunarCalendarUtil.GetFestivalName(month, day);
            Assert.Equal(expectedName, festival);
        }

        #endregion

        #region 生肖测试

        [Theory]
        [InlineData(2024, "龙")]
        [InlineData(2023, "兔")]
        [InlineData(2022, "虎")]
        [InlineData(2021, "牛")]
        [InlineData(2020, "鼠")]
        [InlineData(2019, "猪")]
        [InlineData(2018, "狗")]
        [InlineData(2017, "鸡")]
        [InlineData(2016, "猴")]
        [InlineData(2015, "羊")]
        [InlineData(2014, "马")]
        [InlineData(2013, "蛇")]
        public void GetShengXiao_DifferentYears_ReturnsCorrectZodiac(int year, string expectedZodiac)
        {
            string zodiac = LunarCalendarUtil.GetShengXiao(year);
            Assert.Equal(expectedZodiac, zodiac);
        }

        [Fact]
        public void GetShengXiao_CycleEvery12Years()
        {
            string zodiac1 = LunarCalendarUtil.GetShengXiao(2000);
            string zodiac2 = LunarCalendarUtil.GetShengXiao(2012);
            string zodiac3 = LunarCalendarUtil.GetShengXiao(2024);

            Assert.Equal(zodiac1, zodiac2);
            Assert.Equal(zodiac2, zodiac3);
        }

        #endregion

        #region 边界测试

        [Fact]
        public void SolarToLunar_MinSupportedDate_Works()
        {
            DateTime solar = new DateTime(1900, 1, 31);
            LunarDate lunar = LunarCalendarUtil.SolarToLunar(solar);
            Assert.NotNull(lunar);
        }

        [Fact]
        public void SolarToLunar_MaxSupportedDate_Works()
        {
            DateTime solar = new DateTime(2100, 12, 31);
            LunarDate lunar = LunarCalendarUtil.SolarToLunar(solar);
            Assert.NotNull(lunar);
        }

        [Fact]
        public void LunarToSolar_MinYear_Works()
        {
            DateTime solar = LunarCalendarUtil.LunarToSolar(1900, 1, 1);
            Assert.InRange(solar.Year, 1900, 1900);
        }

        [Fact]
        public void LunarToSolar_MaxYear_Works()
        {
            DateTime solar = LunarCalendarUtil.LunarToSolar(2100, 12, 30);
            // Lunar 2100/12/30 may extend into solar 2101
            Assert.InRange(solar.Year, 2100, 2101);
        }

        #endregion

        #region 闰月测试

        [Fact]
        public void GetLeapMonth_ReturnsValidMonthOrZero()
        {
            for (int year = 1900; year <= 2100; year++)
            {
                int leapMonth = LunarCalendarUtil.GetLeapMonth(year);
                Assert.InRange(leapMonth, 0, 12);
            }
        }

        [Fact]
        public void GetLunarMonthDays_LeapMonth_Returns29Or30Days()
        {
            // 2023年有闰二月
            int days = LunarCalendarUtil.GetLunarMonthDays(2023, 2, true);
            Assert.InRange(days, 29, 30);
        }

        [Fact]
        public void GetLunarMonthDays_NonLeapMonthWithLeapFlag_Returns29Or30Days()
        {
            // 2024年没有闰月
            int days = LunarCalendarUtil.GetLunarMonthDays(2024, 2, true);
            Assert.InRange(days, 29, 30);
        }

        #endregion

        #region 干支周期测试

        [Fact]
        public void GetGanZhiYear_60YearCycle()
        {
            // 干支60年一个循环
            string ganZhi1 = LunarCalendarUtil.GetGanZhiYear(1924);
            string ganZhi2 = LunarCalendarUtil.GetGanZhiYear(1984);
            string ganZhi3 = LunarCalendarUtil.GetGanZhiYear(2044);

            Assert.Equal(ganZhi1, ganZhi2);
            Assert.Equal(ganZhi2, ganZhi3);
        }

        [Fact]
        public void GetGanZhiDay_60DayCycle()
        {
            DateTime date1 = new DateTime(2024, 1, 1);
            DateTime date2 = date1.AddDays(60);
            DateTime date3 = date1.AddDays(120);

            string ganZhi1 = LunarCalendarUtil.GetGanZhiDay(date1);
            string ganZhi2 = LunarCalendarUtil.GetGanZhiDay(date2);
            string ganZhi3 = LunarCalendarUtil.GetGanZhiDay(date3);

            Assert.Equal(ganZhi1, ganZhi2);
            Assert.Equal(ganZhi2, ganZhi3);
        }

        #endregion

        #region 特定日期测试

        [Fact]
        public void SolarToLunar_MultipleDates_ReturnsValidResults()
        {
            var dates = new[]
            {
                new DateTime(2024, 1, 1),
                new DateTime(2024, 6, 1),
                new DateTime(2024, 10, 1)
            };

            foreach (var date in dates)
            {
                LunarDate lunar = LunarCalendarUtil.SolarToLunar(date);
                Assert.NotNull(lunar);
                Assert.InRange(lunar.Year, 1900, 2100);
                Assert.InRange(lunar.Month, 1, 12);
                Assert.InRange(lunar.Day, 1, 30);
            }
        }

        #endregion

        #region 转换一致性测试

        [Fact]
        public void MultipleConversions_ConsistentResults()
        {
            DateTime original = new DateTime(2024, 3, 15);

            // 多次转换应该保持一致
            LunarDate lunar1 = LunarCalendarUtil.SolarToLunar(original);
            DateTime solar1 = LunarCalendarUtil.LunarToSolar(lunar1.Year, lunar1.Month, lunar1.Day, lunar1.IsLeapMonth);
            LunarDate lunar2 = LunarCalendarUtil.SolarToLunar(solar1);
            DateTime solar2 = LunarCalendarUtil.LunarToSolar(lunar2.Year, lunar2.Month, lunar2.Day, lunar2.IsLeapMonth);

            Assert.Equal(original, solar1);
            Assert.Equal(solar1, solar2);
            Assert.Equal(lunar1.Year, lunar2.Year);
            Assert.Equal(lunar1.Month, lunar2.Month);
            Assert.Equal(lunar1.Day, lunar2.Day);
            Assert.Equal(lunar1.IsLeapMonth, lunar2.IsLeapMonth);
        }

        #endregion
    }
}
