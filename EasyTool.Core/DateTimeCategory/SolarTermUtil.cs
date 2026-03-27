using System;
using System.Collections.Generic;

namespace EasyTool.DateTimeCategory
{
    /// <summary>
    /// 二十四节气工具类
    /// 计算二十四节气日期
    /// </summary>
    public static class SolarTermUtil
    {
        // 二十四节气名称
        private static readonly string[] SolarTerms = {
            "小寒", "大寒", "立春", "雨水", "惊蛰", "春分",
            "清明", "谷雨", "立夏", "小满", "芒种", "夏至",
            "小暑", "大暑", "立秋", "处暑", "白露", "秋分",
            "寒露", "霜降", "立冬", "小雪", "大雪", "冬至"
        };

        // 节气对应的公历日期（基准数据，1900年）
        // 实际计算时会根据年份进行调整
        private static readonly (int Month, int Day, int Hour)[] SolarTermBase = {
            (1, 6, 0),    // 小寒
            (1, 20, 0),   // 大寒
            (2, 4, 0),    // 立春
            (2, 19, 0),   // 雨水
            (3, 6, 0),    // 惊蛰
            (3, 21, 0),   // 春分
            (4, 5, 0),    // 清明
            (4, 20, 0),   // 谷雨
            (5, 6, 0),    // 立夏
            (5, 21, 0),   // 小满
            (6, 6, 0),    // 芒种
            (6, 21, 0),   // 夏至
            (7, 7, 0),    // 小暑
            (7, 23, 0),   // 大暑
            (8, 8, 0),    // 立秋
            (8, 23, 0),   // 处暑
            (9, 8, 0),    // 白露
            (9, 23, 0),   // 秋分
            (10, 8, 0),   // 寒露
            (10, 24, 0),  // 霜降
            (11, 8, 0),   // 立冬
            (11, 22, 0),  // 小雪
            (12, 7, 0),   // 大雪
            (12, 22, 0)   // 冬至
        };

        // 节气计算系数（简化算法）
        // 基于1900年小寒为1月6日2时5分的基准
        private static readonly double[] TermCoefficients = {
            0, 1.5, 2.5, 3.5, 4.5, 5.5, 6.5, 7.5, 8.5, 9.5,
            10.5, 11.5, 12.5, 13.5, 14.5, 15.5, 16.5, 17.5,
            18.5, 19.5, 20.5, 21.5, 22.5, 23.5
        };

        /// <summary>
        /// 获取指定年份的所有节气
        /// </summary>
        /// <param name="year">年份</param>
        /// <returns>节气列表</returns>
        public static List<SolarTermInfo> GetSolarTerms(int year)
        {
            var result = new List<SolarTermInfo>();

            for (int i = 0; i < 24; i++)
            {
                var date = CalculateSolarTerm(year, i);
                result.Add(new SolarTermInfo
                {
                    Index = i,
                    Name = SolarTerms[i],
                    Date = date,
                    Month = date.Month,
                    Day = date.Day,
                    Type = i % 2 == 0 ? SolarTermType.Jie : SolarTermType.Qi
                });
            }

            return result;
        }

        /// <summary>
        /// 获取指定日期所在的节气
        /// </summary>
        /// <param name="date">日期</param>
        /// <returns>节气信息，如果不是节气日则返回 null</returns>
        public static SolarTermInfo? GetSolarTerm(DateTime date)
        {
            var terms = GetSolarTerms(date.Year);

            // 检查是否是前一年的最后一个节气
            if (date.Month == 1 && date.Day < 6)
            {
                var lastYearTerms = GetSolarTerms(date.Year - 1);
                var lastTerm = lastYearTerms[23]; // 冬至
                if (lastTerm.Date.Date == date.Date)
                    return lastTerm;
            }

            foreach (var term in terms)
            {
                if (term.Date.Date == date.Date)
                    return term;
            }

            return null;
        }

        /// <summary>
        /// 获取下一个节气
        /// </summary>
        /// <param name="date">基准日期</param>
        /// <returns>下一个节气</returns>
        public static SolarTermInfo GetNextSolarTerm(DateTime date)
        {
            var year = date.Year;
            var terms = GetSolarTerms(year);

            foreach (var term in terms)
            {
                if (term.Date > date)
                    return term;
            }

            // 如果当前年份没有了，返回下一年的第一个节气
            return GetSolarTerms(year + 1)[0];
        }

        /// <summary>
        /// 获取上一个节气
        /// </summary>
        /// <param name="date">基准日期</param>
        /// <returns>上一个节气</returns>
        public static SolarTermInfo GetPreviousSolarTerm(DateTime date)
        {
            var year = date.Year;
            var terms = GetSolarTerms(year);

            for (int i = terms.Count - 1; i >= 0; i--)
            {
                if (terms[i].Date < date)
                    return terms[i];
            }

            // 如果当前年份没有了，返回上一年的最后一个节气
            return GetSolarTerms(year - 1)[23];
        }

        /// <summary>
        /// 获取当前节气（今天或之前最近的节气）
        /// </summary>
        /// <param name="date">日期</param>
        /// <returns>当前节气</returns>
        public static SolarTermInfo GetCurrentSolarTerm(DateTime date)
        {
            var year = date.Year;
            var terms = GetSolarTerms(year);

            SolarTermInfo? current = null;
            foreach (var term in terms)
            {
                if (term.Date <= date)
                    current = term;
                else
                    break;
            }

            if (current != null)
                return current;

            // 返回上一年的最后一个节气
            return GetSolarTerms(year - 1)[23];
        }

        /// <summary>
        /// 计算节气日期
        /// </summary>
        private static DateTime CalculateSolarTerm(int year, int termIndex)
        {
            // 使用简化的节气计算算法
            // 基于黄经计算（每个节气相差15度）

            var baseDate = new DateTime(year, 1, 6, 2, 5, 0); // 1900年小寒基准
            var baseYear = 1900;

            // 计算从1900年到目标年份的累积偏移
            var totalDays = 0.0;

            // 简化计算：使用回归年长度 365.2422 天
            var tropicalYear = 365.2422;
            var yearOffset = (year - baseYear) * tropicalYear;

            // 每个节气平均间隔约 15.2184 天
            var termOffset = termIndex * 15.2184;

            // 计算总偏移
            totalDays = yearOffset + termOffset;

            // 从基准日期计算
            var result = baseDate.AddDays(totalDays - (year - baseYear) * tropicalYear);

            // 调整到正确的年份
            result = new DateTime(year, result.Month, result.Day, result.Hour, result.Minute, 0);

            // 使用更精确的表格数据进行微调
            var (month, day, hour) = SolarTermBase[termIndex];

            // 年份修正（每4年大约有1天的偏差）
            var correction = (year - 1900) * 0.2422;
            var correctedDay = day + (int)Math.Round(correction);

            // 处理月份边界
            if (correctedDay < 1)
            {
                month--;
                if (month == 0) month = 12;
                correctedDay += DateTime.DaysInMonth(year, month);
            }
            else if (correctedDay > DateTime.DaysInMonth(year, month))
            {
                correctedDay -= DateTime.DaysInMonth(year, month);
                month++;
                if (month > 12) month = 1;
            }

            return new DateTime(year, month, correctedDay, hour, 0, 0);
        }

        /// <summary>
        /// 获取节气名称
        /// </summary>
        /// <param name="index">节气索引（0-23）</param>
        /// <returns>节气名称</returns>
        public static string GetSolarTermName(int index)
        {
            if (index < 0 || index >= 24)
                throw new ArgumentOutOfRangeException(nameof(index), "节气索引必须在 0-23 之间");

            return SolarTerms[index];
        }

        /// <summary>
        /// 获取季节
        /// </summary>
        /// <param name="termIndex">节气索引</param>
        /// <returns>季节</returns>
        public static Season GetSeason(int termIndex)
        {
            return termIndex switch
            {
                >= 0 and < 6 => Season.Spring,
                >= 6 and < 12 => Season.Summer,
                >= 12 and < 18 => Season.Autumn,
                _ => Season.Winter
            };
        }

        /// <summary>
        /// 判断是否是"节"（奇数索引）
        /// </summary>
        public static bool IsJie(int termIndex)
        {
            return termIndex % 2 == 0;
        }

        /// <summary>
        /// 判断是否是"气"（偶数索引）
        /// </summary>
        public static bool IsQi(int termIndex)
        {
            return termIndex % 2 == 1;
        }
    }

    /// <summary>
    /// 节气信息
    /// </summary>
    public class SolarTermInfo
    {
        /// <summary>
        /// 节气索引（0-23）
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// 节气名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 公历日期
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// 月份
        /// </summary>
        public int Month { get; set; }

        /// <summary>
        /// 日期
        /// </summary>
        public int Day { get; set; }

        /// <summary>
        /// 节气类型
        /// </summary>
        public SolarTermType Type { get; set; }

        /// <summary>
        /// 所属季节
        /// </summary>
        public Season Season => SolarTermUtil.GetSeason(Index);

        public override string ToString()
        {
            return $"{Name} ({Date:yyyy-MM-dd})";
        }
    }

    /// <summary>
    /// 节气类型
    /// </summary>
    public enum SolarTermType
    {
        /// <summary>
        /// 节（每月的第一个节气）
        /// </summary>
        Jie,

        /// <summary>
        /// 气（每月的第二个节气）
        /// </summary>
        Qi
    }

    /// <summary>
    /// 季节
    /// </summary>
    public enum Season
    {
        Spring,
        Summer,
        Autumn,
        Winter
    }
}
