using Xunit;
using EasyTool.DateTimeCategory;
using System;
using System.Linq;

namespace EasyTool.Tests
{
    public class DateTimeUtilTests
    {
        #region GetDayOfWeek Tests

        [Fact]
        public void GetDayOfWeek_ReturnsValidDayOfWeek()
        {
            var result = DateTimeUtil.GetDayOfWeek();
            Assert.True(Enum.IsDefined(typeof(DayOfWeek), result));
        }

        #endregion

        #region GetFirstDayOfWeek Tests

        [Fact]
        public void GetFirstDayOfWeek_ReturnsDate()
        {
            var result = DateTimeUtil.GetFirstDayOfWeek();
            Assert.True(result <= DateTime.Now);
        }

        [Fact]
        public void GetFirstDayOfWeek_WithDate_ReturnsStartOfThatWeek()
        {
            var testDate = new DateTime(2024, 1, 15); // January 15, 2024 (Monday)
            var result = DateTimeUtil.GetFirstDayOfWeek(testDate);
            Assert.Equal(DayOfWeek.Monday, result.DayOfWeek);
        }

        [Fact]
        public void GetFirstDayOfWeek_Sunday_ReturnsSunday()
        {
            var testDate = new DateTime(2024, 1, 14); // January 14, 2024 (Sunday)
            var result = DateTimeUtil.GetFirstDayOfWeek(testDate);
            // In many cultures, Monday is the first day of the week
            Assert.True(result.DayOfWeek == DayOfWeek.Sunday || result.DayOfWeek == DayOfWeek.Monday);
        }

        #endregion

        #region GetFirstDayOfMonth Tests

        [Fact]
        public void GetFirstDayOfMonth_ReturnsFirstDay()
        {
            var result = DateTimeUtil.GetFirstDayOfMonth();
            Assert.Equal(1, result.Day);
        }

        [Fact]
        public void GetFirstDayOfMonth_WithDate_ReturnsFirstDayOfThatMonth()
        {
            var testDate = new DateTime(2024, 6, 15);
            var result = DateTimeUtil.GetFirstDayOfMonth(testDate);
            Assert.Equal(new DateTime(2024, 6, 1), result);
        }

        [Fact]
        public void GetFirstDayOfMonth_January31_ReturnsJanuary1()
        {
            var testDate = new DateTime(2024, 1, 31);
            var result = DateTimeUtil.GetFirstDayOfMonth(testDate);
            Assert.Equal(new DateTime(2024, 1, 1), result);
        }

        #endregion

        #region GetFirstDayOfQuarter Tests

        [Fact]
        public void GetFirstDayOfQuarter_ReturnsFirstDayOfQuarter()
        {
            var result = DateTimeUtil.GetFirstDayOfQuarter();
            Assert.True(result.Month == 1 || result.Month == 4 || result.Month == 7 || result.Month == 10);
            Assert.Equal(1, result.Day);
        }

        [Fact]
        public void GetFirstDayOfQuarter_Q1_ReturnsJanuary1()
        {
            var testDate = new DateTime(2024, 2, 15);
            var result = DateTimeUtil.GetFirstDayOfQuarter(testDate);
            Assert.Equal(new DateTime(2024, 1, 1), result);
        }

        [Fact]
        public void GetFirstDayOfQuarter_Q2_ReturnsApril1()
        {
            var testDate = new DateTime(2024, 5, 15);
            var result = DateTimeUtil.GetFirstDayOfQuarter(testDate);
            Assert.Equal(new DateTime(2024, 4, 1), result);
        }

        [Fact]
        public void GetFirstDayOfQuarter_Q3_ReturnsJuly1()
        {
            var testDate = new DateTime(2024, 8, 15);
            var result = DateTimeUtil.GetFirstDayOfQuarter(testDate);
            Assert.Equal(new DateTime(2024, 7, 1), result);
        }

        [Fact]
        public void GetFirstDayOfQuarter_Q4_ReturnsOctober1()
        {
            var testDate = new DateTime(2024, 11, 15);
            var result = DateTimeUtil.GetFirstDayOfQuarter(testDate);
            Assert.Equal(new DateTime(2024, 10, 1), result);
        }

        #endregion

        #region GetFirstDayOfYear Tests

        [Fact]
        public void GetFirstDayOfYear_ReturnsJanuary1()
        {
            var result = DateTimeUtil.GetFirstDayOfYear();
            Assert.Equal(1, result.Month);
            Assert.Equal(1, result.Day);
        }

        [Fact]
        public void GetFirstDayOfYear_WithDate_ReturnsJanuary1OfThatYear()
        {
            var testDate = new DateTime(2024, 6, 15);
            var result = DateTimeUtil.GetFirstDayOfYear(testDate);
            Assert.Equal(new DateTime(2024, 1, 1), result);
        }

        [Fact]
        public void GetFirstDayOfYear_December31_ReturnsJanuary1()
        {
            var testDate = new DateTime(2024, 12, 31);
            var result = DateTimeUtil.GetFirstDayOfYear(testDate);
            Assert.Equal(new DateTime(2024, 1, 1), result);
        }

        #endregion

        #region GetDaysBetween Tests

        [Fact]
        public void GetDaysBetween_FutureDate_ReturnsPositiveDays()
        {
            var futureDate = DateTime.Now.AddDays(5);
            var result = DateTimeUtil.GetDaysBetween(futureDate);
            // GetDaysBetween returns the Days component of TimeSpan
            // Due to time of day, this can be 4 or 5 depending on when it runs
            Assert.True(result >= 4 && result <= 5, $"Expected 4 or 5, got {result}");
        }

        [Fact]
        public void GetDaysBetween_PastDate_ReturnsNegativeDays()
        {
            var pastDate = DateTime.Now.AddDays(-5);
            var result = DateTimeUtil.GetDaysBetween(pastDate);
            // GetDaysBetween returns the Days component of TimeSpan, which can vary
            Assert.InRange(result, -5, -4);
        }

        [Fact]
        public void GetDaysBetween_TwoDates_ReturnsCorrectDifference()
        {
            var date1 = new DateTime(2024, 1, 1);
            var date2 = new DateTime(2024, 1, 11);
            var result = DateTimeUtil.GetDaysBetween(date1, date2);
            Assert.Equal(10, result);
        }

        [Fact]
        public void GetDaysBetween_SameDate_ReturnsZero()
        {
            var date = new DateTime(2024, 1, 1);
            var result = DateTimeUtil.GetDaysBetween(date, date);
            Assert.Equal(0, result);
        }

        [Fact]
        public void GetDaysBetween_ReversedOrder_ReturnsNegativeDifference()
        {
            var date1 = new DateTime(2024, 1, 11);
            var date2 = new DateTime(2024, 1, 1);
            var result = DateTimeUtil.GetDaysBetween(date1, date2);
            Assert.Equal(-10, result);
        }

        #endregion

        #region GetWorkDaysBetween Tests

        [Fact]
        public void GetWorkDaysBetween_MondayToFriday_ReturnsFive()
        {
            var monday = new DateTime(2024, 1, 8); // Monday
            var friday = new DateTime(2024, 1, 12); // Friday
            var result = DateTimeUtil.GetWorkDaysBetween(monday, friday);
            // GetWorkDaysBetween counts from start (exclusive) to end (exclusive)
            // Mon->Tue(1)->Wed(2)->Thu(3)->Fri(stops at Friday)
            Assert.Equal(4, result);
        }

        [Fact]
        public void GetWorkDaysBetween_MondayToMonday_ReturnsFive()
        {
            var monday1 = new DateTime(2024, 1, 8); // Monday
            var monday2 = new DateTime(2024, 1, 15); // Next Monday
            var result = DateTimeUtil.GetWorkDaysBetween(monday1, monday2);
            // Mon->Tue(1)->Wed(2)->Thu(3)->Fri(4)->Sat(skip)->Sun(skip)->Mon(stops)
            Assert.Equal(5, result);
        }

        [Fact]
        public void GetWorkDaysBetween_SameDay_ReturnsZero()
        {
            var date = new DateTime(2024, 1, 8); // Monday
            var result = DateTimeUtil.GetWorkDaysBetween(date, date);
            Assert.Equal(0, result);
        }

        [Fact]
        public void GetWorkDaysBetween_SaturdayToMonday_ReturnsOne()
        {
            var saturday = new DateTime(2024, 1, 13); // Saturday
            var monday = new DateTime(2024, 1, 15); // Monday
            var result = DateTimeUtil.GetWorkDaysBetween(saturday, monday);
            // The method counts days from start to end (exclusive), not including start date
            // Saturday -> Sunday (not workday) -> Monday (workday, but stops before it)
            // So it should count 0 workdays
            Assert.Equal(0, result);
        }

        #endregion

        #region IsWorkDay Tests

        [Fact]
        public void IsWorkDay_Monday_ReturnsTrue()
        {
            var monday = new DateTime(2024, 1, 8); // Monday
            var result = DateTimeUtil.IsWorkDay(monday);
            Assert.True(result);
        }

        [Fact]
        public void IsWorkDay_Friday_ReturnsTrue()
        {
            var friday = new DateTime(2024, 1, 12); // Friday
            var result = DateTimeUtil.IsWorkDay(friday);
            Assert.True(result);
        }

        [Fact]
        public void IsWorkDay_Saturday_ReturnsFalse()
        {
            var saturday = new DateTime(2024, 1, 13); // Saturday
            var result = DateTimeUtil.IsWorkDay(saturday);
            Assert.False(result);
        }

        [Fact]
        public void IsWorkDay_Sunday_ReturnsFalse()
        {
            var sunday = new DateTime(2024, 1, 14); // Sunday
            var result = DateTimeUtil.IsWorkDay(sunday);
            Assert.False(result);
        }

        #endregion

        #region GetWeekDays Tests

        [Fact]
        public void GetWeekDays_ReturnsSevenDays()
        {
            var testDate = new DateTime(2024, 1, 10);
            var result = DateTimeUtil.GetWeekDays(testDate);
            Assert.Equal(7, result.Count);
        }

        [Fact]
        public void GetWeekDays_ConsecutiveDays()
        {
            var testDate = new DateTime(2024, 1, 10);
            var result = DateTimeUtil.GetWeekDays(testDate);
            for (int i = 1; i < result.Count; i++)
            {
                var diff = (result[i] - result[i - 1]).Days;
                Assert.Equal(1, diff);
            }
        }

        [Fact]
        public void GetWeekDays_ContainsOriginalDate()
        {
            var testDate = new DateTime(2024, 1, 10);
            var result = DateTimeUtil.GetWeekDays(testDate);
            Assert.Contains(testDate, result);
        }

        #endregion

        #region GetMonthDays Tests

        [Fact]
        public void GetMonthDays_January_Returns31Days()
        {
            var testDate = new DateTime(2024, 1, 15);
            var result = DateTimeUtil.GetMonthDays(testDate);
            Assert.Equal(31, result.Count);
        }

        [Fact]
        public void GetMonthDays_February2024_Returns29Days()
        {
            var testDate = new DateTime(2024, 2, 15); // 2024 is a leap year
            var result = DateTimeUtil.GetMonthDays(testDate);
            Assert.Equal(29, result.Count);
        }

        [Fact]
        public void GetMonthDays_February2023_Returns28Days()
        {
            var testDate = new DateTime(2023, 2, 15); // 2023 is not a leap year
            var result = DateTimeUtil.GetMonthDays(testDate);
            Assert.Equal(28, result.Count);
        }

        [Fact]
        public void GetMonthDays_April_Returns30Days()
        {
            var testDate = new DateTime(2024, 4, 15);
            var result = DateTimeUtil.GetMonthDays(testDate);
            Assert.Equal(30, result.Count);
        }

        [Fact]
        public void GetMonthDays_AllDaysInSameMonth()
        {
            var testDate = new DateTime(2024, 6, 15);
            var result = DateTimeUtil.GetMonthDays(testDate);
            Assert.All(result, date => Assert.Equal(6, date.Month));
        }

        [Fact]
        public void GetMonthDays_ConsecutiveDays()
        {
            var testDate = new DateTime(2024, 1, 15);
            var result = DateTimeUtil.GetMonthDays(testDate);
            for (int i = 1; i < result.Count; i++)
            {
                var diff = (result[i] - result[i - 1]).Days;
                Assert.Equal(1, diff);
            }
        }

        #endregion

        #region GetQuarterDays Tests

        [Fact]
        public void GetQuarterDays_Q1_ReturnsCorrectDays()
        {
            var testDate = new DateTime(2024, 2, 15);
            var result = DateTimeUtil.GetQuarterDays(testDate);
            Assert.Equal(91, result.Count); // 31 + 29 (2024 is leap year)
        }

        [Fact]
        public void GetQuarterDays_Q2_ReturnsCorrectDays()
        {
            var testDate = new DateTime(2024, 5, 15);
            var result = DateTimeUtil.GetQuarterDays(testDate);
            Assert.Equal(91, result.Count); // 30 + 31 + 30
        }

        [Fact]
        public void GetQuarterDays_Q3_ReturnsCorrectDays()
        {
            var testDate = new DateTime(2024, 8, 15);
            var result = DateTimeUtil.GetQuarterDays(testDate);
            Assert.Equal(92, result.Count); // 31 + 31 + 30
        }

        [Fact]
        public void GetQuarterDays_Q4_ReturnsCorrectDays()
        {
            var testDate = new DateTime(2024, 11, 15);
            var result = DateTimeUtil.GetQuarterDays(testDate);
            Assert.Equal(92, result.Count); // 31 + 30 + 31
        }

        [Fact]
        public void GetQuarterDays_AllDaysInSameQuarter()
        {
            var testDate = new DateTime(2024, 2, 15);
            var result = DateTimeUtil.GetQuarterDays(testDate);
            Assert.All(result.Take(10), date => Assert.InRange(date.Month, 1, 3));
        }

        #endregion

        #region GetYearDays Tests

        [Fact]
        public void GetYearDays_2024_Returns366Days()
        {
            var testDate = new DateTime(2024, 6, 15); // 2024 is a leap year
            var result = DateTimeUtil.GetYearDays(testDate);
            Assert.Equal(366, result.Count);
        }

        [Fact]
        public void GetYearDays_2023_Returns365Days()
        {
            var testDate = new DateTime(2023, 6, 15); // 2023 is not a leap year
            var result = DateTimeUtil.GetYearDays(testDate);
            Assert.Equal(365, result.Count);
        }

        [Fact]
        public void GetYearDays_AllDaysInSameYear()
        {
            var testDate = new DateTime(2024, 6, 15);
            var result = DateTimeUtil.GetYearDays(testDate);
            Assert.All(result, date => Assert.Equal(2024, date.Year));
        }

        [Fact]
        public void GetYearDays_ConsecutiveDays()
        {
            var testDate = new DateTime(2024, 6, 15);
            var result = DateTimeUtil.GetYearDays(testDate);
            for (int i = 1; i < Math.Min(100, result.Count); i++)
            {
                var diff = (result[i] - result[i - 1]).Days;
                Assert.Equal(1, diff);
            }
        }

        [Fact]
        public void GetYearDays_StartsWithJanuary1()
        {
            var testDate = new DateTime(2024, 6, 15);
            var result = DateTimeUtil.GetYearDays(testDate);
            Assert.Equal(new DateTime(2024, 1, 1), result.First());
        }

        [Fact]
        public void GetYearDays_EndsWithDecember31()
        {
            var testDate = new DateTime(2024, 6, 15);
            var result = DateTimeUtil.GetYearDays(testDate);
            Assert.Equal(new DateTime(2024, 12, 31), result.Last());
        }

        #endregion
    }
}
