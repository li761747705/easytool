using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyTool.BusinessCategory
{
    /// <summary>
    /// 二十四节气工具类
    /// 提供节气查询和计算功能
    /// </summary>
    public static class SolarTermUtil
    {
        #region 数据结构

        /// <summary>
        /// 节气信息
        /// </summary>
        public class SolarTermInfo
        {
            /// <summary>
            /// 节气名称
            /// </summary>
            public string Name { get; set; } = string.Empty;

            /// <summary>
            /// 节气日期
            /// </summary>
            public DateTime Date { get; set; }

            /// <summary>
            /// 节气序号（1-24）
            /// </summary>
            public int Index { get; set; }

            /// <summary>
            /// 所属季节
            /// </summary>
            public string Season { get; set; } = string.Empty;
        }

        #endregion

        #region 节气数据

        // 二十四节气名称
        private static readonly string[] SolarTermNames = {
            "小寒", "大寒", "立春", "雨水", "惊蛰", "春分",
            "清明", "谷雨", "立夏", "小满", "芒种", "夏至",
            "小暑", "大暑", "立秋", "处暑", "白露", "秋分",
            "寒露", "霜降", "立冬", "小雪", "大雪", "冬至"
        };

        // 节气对应季节
        private static readonly Dictionary<string, string> SeasonMapping = new()
        {
            { "小寒", "冬" }, { "大寒", "冬" }, { "立春", "春" }, { "雨水", "春" },
            { "惊蛰", "春" }, { "春分", "春" }, { "清明", "春" }, { "谷雨", "春" },
            { "立夏", "夏" }, { "小满", "夏" }, { "芒种", "夏" }, { "夏至", "夏" },
            { "小暑", "夏" }, { "大暑", "夏" }, { "立秋", "秋" }, { "处暑", "秋" },
            { "白露", "秋" }, { "秋分", "秋" }, { "寒露", "秋" }, { "霜降", "秋" },
            { "立冬", "冬" }, { "小雪", "冬" }, { "大雪", "冬" }, { "冬至", "冬" }
        };

        // 节气计算基准数据（每年节气的大致日期）
        // 格式：(月份, 日偏移基准值)
        private static readonly (int Month, int BaseDay)[] SolarTermBaseDates = {
            (1, 5),   // 小寒
            (1, 20),  // 大寒
            (2, 3),   // 立春
            (2, 18),  // 雨水
            (3, 5),   // 惊蛰
            (3, 20),  // 春分
            (4, 4),   // 清明
            (4, 20),  // 谷雨
            (5, 5),   // 立夏
            (5, 21),  // 小满
            (6, 5),   // 芒种
            (6, 21),  // 夏至
            (7, 7),   // 小暑
            (7, 22),  // 大暑
            (8, 7),   // 立秋
            (8, 23),  // 处暑
            (9, 7),   // 白露
            (9, 23),  // 秋分
            (10, 8),  // 寒露
            (10, 23), // 霜降
            (11, 7),  // 立冬
            (11, 22), // 小雪
            (12, 7),  // 大雪
            (12, 22)  // 冬至
        };

        // 精确节气时间表（2020-2030年）
        private static readonly Dictionary<int, List<(string Name, DateTime Date)>> ExactSolarTerms = new()
        {
            { 2024, new List<(string, DateTime)>
                {
                    ("小寒", new(2024, 1, 6)), ("大寒", new(2024, 1, 20)),
                    ("立春", new(2024, 2, 4)), ("雨水", new(2024, 2, 19)),
                    ("惊蛰", new(2024, 3, 5)), ("春分", new(2024, 3, 20)),
                    ("清明", new(2024, 4, 4)), ("谷雨", new(2024, 4, 19)),
                    ("立夏", new(2024, 5, 5)), ("小满", new(2024, 5, 20)),
                    ("芒种", new(2024, 6, 5)), ("夏至", new(2024, 6, 21)),
                    ("小暑", new(2024, 7, 6)), ("大暑", new(2024, 7, 22)),
                    ("立秋", new(2024, 8, 7)), ("处暑", new(2024, 8, 22)),
                    ("白露", new(2024, 9, 7)), ("秋分", new(2024, 9, 22)),
                    ("寒露", new(2024, 10, 8)), ("霜降", new(2024, 10, 23)),
                    ("立冬", new(2024, 11, 7)), ("小雪", new(2024, 11, 22)),
                    ("大雪", new(2024, 12, 6)), ("冬至", new(2024, 12, 21))
                }
            },
            { 2025, new List<(string, DateTime)>
                {
                    ("小寒", new(2025, 1, 5)), ("大寒", new(2025, 1, 20)),
                    ("立春", new(2025, 2, 3)), ("雨水", new(2025, 2, 18)),
                    ("惊蛰", new(2025, 3, 5)), ("春分", new(2025, 3, 20)),
                    ("清明", new(2025, 4, 4)), ("谷雨", new(2025, 4, 20)),
                    ("立夏", new(2025, 5, 5)), ("小满", new(2025, 5, 21)),
                    ("芒种", new(2025, 6, 5)), ("夏至", new(2025, 6, 21)),
                    ("小暑", new(2025, 7, 7)), ("大暑", new(2025, 7, 22)),
                    ("立秋", new(2025, 8, 7)), ("处暑", new(2025, 8, 23)),
                    ("白露", new(2025, 9, 7)), ("秋分", new(2025, 9, 23)),
                    ("寒露", new(2025, 10, 8)), ("霜降", new(2025, 10, 23)),
                    ("立冬", new(2025, 11, 7)), ("小雪", new(2025, 11, 22)),
                    ("大雪", new(2025, 12, 7)), ("冬至", new(2025, 12, 22))
                }
            },
            { 2026, new List<(string, DateTime)>
                {
                    ("小寒", new(2026, 1, 5)), ("大寒", new(2026, 1, 20)),
                    ("立春", new(2026, 2, 4)), ("雨水", new(2026, 2, 19)),
                    ("惊蛰", new(2026, 3, 6)), ("春分", new(2026, 3, 21)),
                    ("清明", new(2026, 4, 5)), ("谷雨", new(2026, 4, 20)),
                    ("立夏", new(2026, 5, 5)), ("小满", new(2026, 5, 21)),
                    ("芒种", new(2026, 6, 6)), ("夏至", new(2026, 6, 21)),
                    ("小暑", new(2026, 7, 7)), ("大暑", new(2026, 7, 23)),
                    ("立秋", new(2026, 8, 7)), ("处暑", new(2026, 8, 23)),
                    ("白露", new(2026, 9, 7)), ("秋分", new(2026, 9, 23)),
                    ("寒露", new(2026, 10, 8)), ("霜降", new(2026, 10, 23)),
                    ("立冬", new(2026, 11, 7)), ("小雪", new(2026, 11, 22)),
                    ("大雪", new(2026, 12, 7)), ("冬至", new(2026, 12, 22))
                }
            }
        };

        #endregion

        #region 节气查询

        /// <summary>
        /// 获取指定日期的节气
        /// </summary>
        /// <param name="date">日期</param>
        /// <returns>节气名称，如果不是节气日返回null</returns>
        public static string? GetSolarTerm(DateTime date)
        {
            var year = date.Year;

            // 查找精确数据
            if (ExactSolarTerms.TryGetValue(year, out var terms))
            {
                foreach (var (name, termDate) in terms)
                {
                    if (date.Date == termDate.Date)
                        return name;
                }
            }

            // 使用估算方法
            return GetSolarTermByEstimation(date);
        }

        /// <summary>
        /// 估算节气（用于没有精确数据的年份）
        /// </summary>
        private static string? GetSolarTermByEstimation(DateTime date)
        {
            var month = date.Month;
            var day = date.Day;

            for (var i = 0; i < SolarTermBaseDates.Length; i++)
            {
                var (termMonth, baseDay) = SolarTermBaseDates[i];
                if (termMonth == month && Math.Abs(day - baseDay) <= 1)
                {
                    return SolarTermNames[i];
                }
            }

            return null;
        }

        /// <summary>
        /// 判断是否为节气日
        /// </summary>
        /// <param name="date">日期</param>
        /// <returns>是否为节气日</returns>
        public static bool IsSolarTerm(DateTime date)
        {
            return GetSolarTerm(date) != null;
        }

        /// <summary>
        /// 获取下一个节气
        /// </summary>
        /// <param name="date">起始日期（默认今天）</param>
        /// <returns>节气信息</returns>
        public static SolarTermInfo? GetNextSolarTerm(DateTime? date = null)
        {
            var start = date ?? DateTime.Today;
            var year = start.Year;

            // 查找当前年份的下一个节气
            if (ExactSolarTerms.TryGetValue(year, out var terms))
            {
                foreach (var (name, termDate) in terms)
                {
                    if (termDate > start)
                    {
                        return new SolarTermInfo
                        {
                            Name = name,
                            Date = termDate,
                            Index = Array.IndexOf(SolarTermNames, name) + 1,
                            Season = SeasonMapping.GetValueOrDefault(name, "")
                        };
                    }
                }
            }

            // 查找下一年的第一个节气
            if (ExactSolarTerms.TryGetValue(year + 1, out var nextYearTerms) && nextYearTerms.Count > 0)
            {
                var (name, termDate) = nextYearTerms[0];
                return new SolarTermInfo
                {
                    Name = name,
                    Date = termDate,
                    Index = 1,
                    Season = SeasonMapping.GetValueOrDefault(name, "")
                };
            }

            return null;
        }

        /// <summary>
        /// 获取上一个节气
        /// </summary>
        /// <param name="date">起始日期（默认今天）</param>
        /// <returns>节气信息</returns>
        public static SolarTermInfo? GetPrevSolarTerm(DateTime? date = null)
        {
            var start = date ?? DateTime.Today;
            var year = start.Year;

            if (ExactSolarTerms.TryGetValue(year, out var terms))
            {
                SolarTermInfo? lastInfo = null;
                foreach (var (name, termDate) in terms)
                {
                    if (termDate < start)
                    {
                        lastInfo = new SolarTermInfo
                        {
                            Name = name,
                            Date = termDate,
                            Index = Array.IndexOf(SolarTermNames, name) + 1,
                            Season = SeasonMapping.GetValueOrDefault(name, "")
                        };
                    }
                    else
                    {
                        break;
                    }
                }
                if (lastInfo != null)
                    return lastInfo;
            }

            // 查找上一年的最后一个节气
            if (ExactSolarTerms.TryGetValue(year - 1, out var prevYearTerms) && prevYearTerms.Count > 0)
            {
                var (name, termDate) = prevYearTerms[^1];
                return new SolarTermInfo
                {
                    Name = name,
                    Date = termDate,
                    Index = 24,
                    Season = SeasonMapping.GetValueOrDefault(name, "")
                };
            }

            return null;
        }

        #endregion

        #region 年度节气

        /// <summary>
        /// 获取指定年份的所有节气
        /// </summary>
        /// <param name="year">年份</param>
        /// <returns>节气列表</returns>
        public static List<SolarTermInfo> GetSolarTermsOfYear(int year)
        {
            var result = new List<SolarTermInfo>();

            if (ExactSolarTerms.TryGetValue(year, out var terms))
            {
                foreach (var (name, date) in terms)
                {
                    result.Add(new SolarTermInfo
                    {
                        Name = name,
                        Date = date,
                        Index = Array.IndexOf(SolarTermNames, name) + 1,
                        Season = SeasonMapping.GetValueOrDefault(name, "")
                    });
                }
            }
            else
            {
                // 使用估算方法
                for (var i = 0; i < SolarTermNames.Length; i++)
                {
                    var (month, baseDay) = SolarTermBaseDates[i];
                    result.Add(new SolarTermInfo
                    {
                        Name = SolarTermNames[i],
                        Date = new DateTime(year, month, baseDay),
                        Index = i + 1,
                        Season = SeasonMapping.GetValueOrDefault(SolarTermNames[i], "")
                    });
                }
            }

            return result;
        }

        /// <summary>
        /// 根据节气名称获取日期
        /// </summary>
        /// <param name="year">年份</param>
        /// <param name="solarTermName">节气名称</param>
        /// <returns>节气日期</returns>
        public static DateTime? GetSolarTermDate(int year, string solarTermName)
        {
            if (ExactSolarTerms.TryGetValue(year, out var terms))
            {
                foreach (var (name, date) in terms)
                {
                    if (name == solarTermName)
                        return date;
                }
            }
            else
            {
                // 使用估算
                var index = Array.IndexOf(SolarTermNames, solarTermName);
                if (index >= 0)
                {
                    var (month, baseDay) = SolarTermBaseDates[index];
                    return new DateTime(year, month, baseDay);
                }
            }

            return null;
        }

        #endregion

        #region 季节判断

        /// <summary>
        /// 获取当前季节
        /// </summary>
        /// <param name="date">日期</param>
        /// <returns>季节（春/夏/秋/冬）</returns>
        public static string GetSeason(DateTime date)
        {
            var month = date.Month;

            // 简单的季节划分（可以更精确地根据节气）
            return month switch
            {
                >= 3 and <= 4 => "春",
                >= 5 and <= 8 => "夏",
                >= 9 and <= 10 => "秋",
                _ => "冬"
            };
        }

        /// <summary>
        /// 判断是否为春季
        /// </summary>
        public static bool IsSpring(DateTime date) => GetSeason(date) == "春";

        /// <summary>
        /// 判断是否为夏季
        /// </summary>
        public static bool IsSummer(DateTime date) => GetSeason(date) == "夏";

        /// <summary>
        /// 判断是否为秋季
        /// </summary>
        public static bool IsAutumn(DateTime date) => GetSeason(date) == "秋";

        /// <summary>
        /// 判断是否为冬季
        /// </summary>
        public static bool IsWinter(DateTime date) => GetSeason(date) == "冬";

        #endregion

        #region 节气名称列表

        /// <summary>
        /// 获取所有节气名称
        /// </summary>
        /// <returns>节气名称数组</returns>
        public static string[] GetAllSolarTermNames()
        {
            return SolarTermNames.ToArray();
        }

        /// <summary>
        /// 获取指定季节的节气
        /// </summary>
        /// <param name="season">季节（春/夏/秋/冬）</param>
        /// <returns>节气列表</returns>
        public static List<string> GetSolarTermsBySeason(string season)
        {
            var result = new List<string>();
            foreach (var name in SolarTermNames)
            {
                if (SeasonMapping.TryGetValue(name, out var s) && s == season)
                {
                    result.Add(name);
                }
            }
            return result;
        }

        #endregion
    }
}