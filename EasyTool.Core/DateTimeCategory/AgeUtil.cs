using System;

namespace EasyTool.DateTimeCategory
{
    /// <summary>
    /// 年龄计算工具类
    /// </summary>
    public static class AgeUtil
    {
        /// <summary>
        /// 计算年龄（周岁）
        /// </summary>
        public static int CalculateAge(DateTime birthDate, DateTime? currentDate = null)
        {
            var today = currentDate ?? DateTime.Today;
            var age = today.Year - birthDate.Year;

            // 如果生日还没到，减1岁
            if (birthDate.Date > today.AddYears(-age))
            {
                age--;
            }

            return Math.Max(0, age);
        }

        /// <summary>
        /// 计算精确年龄（岁、月、日）
        /// </summary>
        public static Age CalculateExactAge(DateTime birthDate, DateTime? currentDate = null)
        {
            var today = currentDate ?? DateTime.Today;

            var years = today.Year - birthDate.Year;
            var months = today.Month - birthDate.Month;
            var days = today.Day - birthDate.Day;

            if (days < 0)
            {
                months--;
                days += DateTime.DaysInMonth(today.Year, today.Month == 1 ? 12 : today.Month - 1);
            }

            if (months < 0)
            {
                years--;
                months += 12;
            }

            return new Age
            {
                Years = Math.Max(0, years),
                Months = Math.Max(0, months),
                Days = Math.Max(0, days)
            };
        }

        /// <summary>
        /// 计算虚岁
        /// </summary>
        public static int CalculateNominalAge(DateTime birthDate, DateTime? currentDate = null)
        {
            var today = currentDate ?? DateTime.Today;
            return today.Year - birthDate.Year + 1;
        }

        /// <summary>
        /// 获取下一个生日
        /// </summary>
        public static DateTime GetNextBirthday(DateTime birthDate, DateTime? currentDate = null)
        {
            var today = currentDate ?? DateTime.Today;
            var birthday = new DateTime(today.Year, birthDate.Month, birthDate.Day);

            if (birthday < today)
            {
                birthday = birthday.AddYears(1);
            }

            return birthday;
        }

        /// <summary>
        /// 获取距离下一个生日的天数
        /// </summary>
        public static int GetDaysUntilNextBirthday(DateTime birthDate, DateTime? currentDate = null)
        {
            var nextBirthday = GetNextBirthday(birthDate, currentDate);
            return (nextBirthday - (currentDate ?? DateTime.Today)).Days;
        }

        /// <summary>
        /// 判断今天是否是生日
        /// </summary>
        public static bool IsBirthday(DateTime birthDate, DateTime? currentDate = null)
        {
            var today = currentDate ?? DateTime.Today;
            return birthDate.Month == today.Month && birthDate.Day == today.Day;
        }

        /// <summary>
        /// 判断是否成年（默认18岁）
        /// </summary>
        public static bool IsAdult(DateTime birthDate, int adultAge = 18, DateTime? currentDate = null)
        {
            return CalculateAge(birthDate, currentDate) >= adultAge;
        }

        /// <summary>
        /// 获取生肖
        /// </summary>
        public static string GetChineseZodiac(DateTime birthDate)
        {
            var zodiacs = new[] { "鼠", "牛", "虎", "兔", "龙", "蛇", "马", "羊", "猴", "鸡", "狗", "猪" };
            var index = (birthDate.Year - 1900) % 12;
            return zodiacs[index >= 0 ? index : index + 12];
        }

        /// <summary>
        /// 获取星座
        /// </summary>
        public static string GetZodiacSign(DateTime birthDate)
        {
            var month = birthDate.Month;
            var day = birthDate.Day;

            return (month, day) switch
            {
                (1, >= 20) or (2, <= 18) => "水瓶座",
                (2, >= 19) or (3, <= 20) => "双鱼座",
                (3, >= 21) or (4, <= 19) => "白羊座",
                (4, >= 20) or (5, <= 20) => "金牛座",
                (5, >= 21) or (6, <= 21) => "双子座",
                (6, >= 22) or (7, <= 22) => "巨蟹座",
                (7, >= 23) or (8, <= 22) => "狮子座",
                (8, >= 23) or (9, <= 22) => "处女座",
                (9, >= 23) or (10, <= 23) => "天秤座",
                (10, >= 24) or (11, <= 22) => "天蝎座",
                (11, >= 23) or (12, <= 21) => "射手座",
                _ => "摩羯座"
            };
        }

        /// <summary>
        /// 获取星座英文
        /// </summary>
        public static string GetZodiacSignEnglish(DateTime birthDate)
        {
            var month = birthDate.Month;
            var day = birthDate.Day;

            return (month, day) switch
            {
                (1, >= 20) or (2, <= 18) => "Aquarius",
                (2, >= 19) or (3, <= 20) => "Pisces",
                (3, >= 21) or (4, <= 19) => "Aries",
                (4, >= 20) or (5, <= 20) => "Taurus",
                (5, >= 21) or (6, <= 21) => "Gemini",
                (6, >= 22) or (7, <= 22) => "Cancer",
                (7, >= 23) or (8, <= 22) => "Leo",
                (8, >= 23) or (9, <= 22) => "Virgo",
                (9, >= 23) or (10, <= 23) => "Libra",
                (10, >= 24) or (11, <= 22) => "Scorpio",
                (11, >= 23) or (12, <= 21) => "Sagittarius",
                _ => "Capricorn"
            };
        }

        /// <summary>
        /// 计算退休年龄（男60，女干部55，女工人50）
        /// </summary>
        public static DateTime CalculateRetirementDate(DateTime birthDate, Gender gender, bool isCadre = false)
        {
            var retirementAge = gender switch
            {
                Gender.Male => 60,
                Gender.Female when isCadre => 55,
                Gender.Female => 50,
                _ => 60
            };

            return birthDate.AddYears(retirementAge);
        }

        /// <summary>
        /// 计算总存活天数
        /// </summary>
        public static int CalculateTotalDays(DateTime birthDate, DateTime? currentDate = null)
        {
            return (int)((currentDate ?? DateTime.Today) - birthDate.Date).TotalDays;
        }

        /// <summary>
        /// 计算总存活周数
        /// </summary>
        public static int CalculateTotalWeeks(DateTime birthDate, DateTime? currentDate = null)
        {
            return CalculateTotalDays(birthDate, currentDate) / 7;
        }

        /// <summary>
        /// 计算总存活月数
        /// </summary>
        public static int CalculateTotalMonths(DateTime birthDate, DateTime? currentDate = null)
        {
            var today = currentDate ?? DateTime.Today;
            return (today.Year - birthDate.Year) * 12 + today.Month - birthDate.Month;
        }

        /// <summary>
        /// 格式化年龄显示
        /// </summary>
        public static string FormatAge(DateTime birthDate, DateTime? currentDate = null)
        {
            var age = CalculateExactAge(birthDate, currentDate);
            if (age.Years > 0)
                return $"{age.Years}岁{age.Months}个月";
            if (age.Months > 0)
                return $"{age.Months}个月{age.Days}天";
            return $"{age.Days}天";
        }

        /// <summary>
        /// 格式化年龄（简短格式）
        /// </summary>
        public static string FormatAgeShort(DateTime birthDate, DateTime? currentDate = null)
        {
            var age = CalculateExactAge(birthDate, currentDate);
            if (age.Years > 0)
                return $"{age.Years}岁";
            if (age.Months > 0)
                return $"{age.Months}个月";
            return $"{age.Days}天";
        }
    }

    /// <summary>
    /// 年龄信息
    /// </summary>
    public class Age
    {
        /// <summary>
        /// 岁
        /// </summary>
        public int Years { get; set; }

        /// <summary>
        /// 月
        /// </summary>
        public int Months { get; set; }

        /// <summary>
        /// 日
        /// </summary>
        public int Days { get; set; }

        /// <summary>
        /// 总天数
        /// </summary>
        public int TotalDays => Years * 365 + Months * 30 + Days;

        /// <summary>
        /// 总月数
        /// </summary>
        public int TotalMonths => Years * 12 + Months;

        public override string ToString()
        {
            return $"{Years}岁{Months}个月{Days}天";
        }
    }

    /// <summary>
    /// 性别
    /// </summary>
    public enum Gender
    {
        /// <summary>
        /// 男性
        /// </summary>
        Male,

        /// <summary>
        /// 女性
        /// </summary>
        Female
    }
}
