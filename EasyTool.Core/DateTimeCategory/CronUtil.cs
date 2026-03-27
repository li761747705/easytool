using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace EasyTool.DateTimeCategory
{
    /// <summary>
    /// Cron 表达式工具类
    /// 提供 Cron 表达式的解析和计算下次执行时间
    /// </summary>
    public static class CronUtil
    {
        private static readonly Regex CronRegex = new(@"^(\S+)\s+(\S+)\s+(\S+)\s+(\S+)\s+(\S+)$", RegexOptions.Compiled);

        /// <summary>
        /// 验证 Cron 表达式是否有效
        /// </summary>
        /// <param name="cronExpression">Cron 表达式</param>
        /// <returns>是否有效</returns>
        public static bool IsValid(string cronExpression)
        {
            if (string.IsNullOrWhiteSpace(cronExpression))
                return false;

            var match = CronRegex.Match(cronExpression);
            if (!match.Success)
                return false;

            return IsValidField(match.Groups[1].Value, 0, 59) &&  // 秒
                   IsValidField(match.Groups[2].Value, 0, 59) &&  // 分
                   IsValidField(match.Groups[3].Value, 0, 23) &&  // 时
                   IsValidField(match.Groups[4].Value, 1, 31) &&  // 日
                   IsValidField(match.Groups[5].Value, 1, 12);    // 月
        }

        private static bool IsValidField(string field, int min, int max)
        {
            if (field == "*")
                return true;

            foreach (var part in field.Split(','))
            {
                var trimmedPart = part.Trim();

                if (trimmedPart == "*")
                    continue;

                if (trimmedPart.Contains('/'))
                {
                    var slashParts = trimmedPart.Split('/');
                    if (slashParts.Length != 2)
                        return false;

                    if (slashParts[0] != "*" && !IsValidRangeOrNumber(slashParts[0], min, max))
                        return false;

                    if (!int.TryParse(slashParts[1], out var step) || step <= 0)
                        return false;
                }
                else if (!IsValidRangeOrNumber(trimmedPart, min, max))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsValidRangeOrNumber(string value, int min, int max)
        {
            if (value.Contains('-'))
            {
                var rangeParts = value.Split('-');
                if (rangeParts.Length != 2)
                    return false;

                if (!int.TryParse(rangeParts[0], out var start) || !int.TryParse(rangeParts[1], out var end))
                    return false;

                return start >= min && start <= max && end >= min && end <= max && start <= end;
            }

            if (int.TryParse(value, out var num))
                return num >= min && num <= max;

            return false;
        }

        /// <summary>
        /// 获取下次执行时间
        /// </summary>
        /// <param name="cronExpression">Cron 表达式（秒 分 时 日 月）</param>
        /// <param name="fromTime">起始时间</param>
        /// <returns>下次执行时间</returns>
        public static DateTime GetNextExecutionTime(string cronExpression, DateTime? fromTime = null)
        {
            if (!IsValid(cronExpression))
                throw new ArgumentException("无效的 Cron 表达式", nameof(cronExpression));

            var parts = cronExpression.Split(' ');
            var secondField = parts[0];
            var minuteField = parts[1];
            var hourField = parts[2];
            var dayField = parts[3];
            var monthField = parts[4];

            var currentTime = fromTime ?? DateTime.Now;
            var nextTime = currentTime.AddSeconds(1);

            while (true)
            {
                // 检查月份
                if (!IsFieldMatch(monthField, nextTime.Month, 1, 12))
                {
                    nextTime = new DateTime(nextTime.Year, nextTime.Month, 1).AddMonths(1);
                    continue;
                }

                // 检查日期
                var daysInMonth = DateTime.DaysInMonth(nextTime.Year, nextTime.Month);
                if (!IsFieldMatch(dayField, nextTime.Day, 1, daysInMonth))
                {
                    nextTime = nextTime.AddDays(1);
                    nextTime = new DateTime(nextTime.Year, nextTime.Month, nextTime.Day);
                    continue;
                }

                // 检查小时
                if (!IsFieldMatch(hourField, nextTime.Hour, 0, 23))
                {
                    nextTime = nextTime.AddHours(1);
                    nextTime = new DateTime(nextTime.Year, nextTime.Month, nextTime.Day, nextTime.Hour, 0, 0);
                    continue;
                }

                // 检查分钟
                if (!IsFieldMatch(minuteField, nextTime.Minute, 0, 59))
                {
                    nextTime = nextTime.AddMinutes(1);
                    nextTime = new DateTime(nextTime.Year, nextTime.Month, nextTime.Day, nextTime.Hour, nextTime.Minute, 0);
                    continue;
                }

                // 检查秒
                if (!IsFieldMatch(secondField, nextTime.Second, 0, 59))
                {
                    nextTime = nextTime.AddSeconds(1);
                    continue;
                }

                return nextTime;
            }
        }

        /// <summary>
        /// 获取接下来的多次执行时间
        /// </summary>
        /// <param name="cronExpression">Cron 表达式</param>
        /// <param name="count">获取次数</param>
        /// <param name="fromTime">起始时间</param>
        /// <returns>执行时间列表</returns>
        public static List<DateTime> GetNextExecutionTimes(string cronExpression, int count, DateTime? fromTime = null)
        {
            var result = new List<DateTime>();
            var nextTime = fromTime ?? DateTime.Now;

            for (int i = 0; i < count; i++)
            {
                nextTime = GetNextExecutionTime(cronExpression, nextTime);
                result.Add(nextTime);
            }

            return result;
        }

        private static bool IsFieldMatch(string field, int value, int min, int max)
        {
            if (field == "*")
                return true;

            foreach (var part in field.Split(','))
            {
                var trimmedPart = part.Trim();

                if (trimmedPart == "*")
                    return true;

                if (trimmedPart.Contains('/'))
                {
                    var slashParts = trimmedPart.Split('/');
                    var step = int.Parse(slashParts[1]);

                    if (slashParts[0] == "*")
                    {
                        if ((value - min) % step == 0)
                            return true;
                    }
                    else
                    {
                        var start = int.Parse(slashParts[0]);
                        if (value >= start && (value - start) % step == 0)
                            return true;
                    }
                }
                else if (trimmedPart.Contains('-'))
                {
                    var rangeParts = trimmedPart.Split('-');
                    var start = int.Parse(rangeParts[0]);
                    var end = int.Parse(rangeParts[1]);

                    if (value >= start && value <= end)
                        return true;
                }
                else
                {
                    if (int.Parse(trimmedPart) == value)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 获取字段匹配的所有值
        /// </summary>
        /// <param name="field">字段表达式</param>
        /// <param name="min">最小值</param>
        /// <param name="max">最大值</param>
        /// <returns>匹配的值列表</returns>
        public static List<int> GetFieldValues(string field, int min, int max)
        {
            var result = new HashSet<int>();

            if (field == "*")
            {
                for (int i = min; i <= max; i++)
                    result.Add(i);
                return result.OrderBy(x => x).ToList();
            }

            foreach (var part in field.Split(','))
            {
                var trimmedPart = part.Trim();

                if (trimmedPart == "*")
                {
                    for (int i = min; i <= max; i++)
                        result.Add(i);
                }
                else if (trimmedPart.Contains('/'))
                {
                    var slashParts = trimmedPart.Split('/');
                    var step = int.Parse(slashParts[1]);
                    int start;

                    if (slashParts[0] == "*")
                    {
                        start = min;
                    }
                    else
                    {
                        start = int.Parse(slashParts[0]);
                    }

                    for (int i = start; i <= max; i += step)
                        result.Add(i);
                }
                else if (trimmedPart.Contains('-'))
                {
                    var rangeParts = trimmedPart.Split('-');
                    var start = int.Parse(rangeParts[0]);
                    var end = int.Parse(rangeParts[1]);

                    for (int i = start; i <= end; i++)
                        result.Add(i);
                }
                else
                {
                    result.Add(int.Parse(trimmedPart));
                }
            }

            return result.Where(x => x >= min && x <= max).OrderBy(x => x).ToList();
        }

        /// <summary>
        /// 解析 Cron 表达式为可读文本
        /// </summary>
        /// <param name="cronExpression">Cron 表达式</param>
        /// <returns>可读文本</returns>
        public static string ToDescription(string cronExpression)
        {
            if (!IsValid(cronExpression))
                throw new ArgumentException("无效的 Cron 表达式", nameof(cronExpression));

            var parts = cronExpression.Split(' ');
            var secondField = parts[0];
            var minuteField = parts[1];
            var hourField = parts[2];
            var dayField = parts[3];
            var monthField = parts[4];

            var descriptions = new List<string>();

            // 秒
            if (secondField != "*")
                descriptions.Add($"第 {FieldToDescription(secondField)} 秒");

            // 分
            if (minuteField != "*")
                descriptions.Add($"第 {FieldToDescription(minuteField)} 分钟");

            // 时
            if (hourField != "*")
                descriptions.Add($"第 {FieldToDescription(hourField)} 小时");

            // 日
            if (dayField != "*")
                descriptions.Add($"每月 {FieldToDescription(dayField)} 日");

            // 月
            if (monthField != "*")
                descriptions.Add($"{FieldToDescription(monthField)} 月");

            if (descriptions.Count == 0)
                return "每秒执行";

            return string.Join("，", descriptions) + " 执行";
        }

        private static string FieldToDescription(string field)
        {
            if (field == "*")
                return "每";

            if (field.Contains('/'))
            {
                var parts = field.Split('/');
                return parts[0] == "*" ? $"每隔 {parts[1]}" : $"从 {parts[0]} 开始每隔 {parts[1]}";
            }

            if (field.Contains('-'))
            {
                var parts = field.Split('-');
                return $"{parts[0]} 到 {parts[1]}";
            }

            return field;
        }

        #region 常用 Cron 表达式

        /// <summary>
        /// 每秒执行
        /// </summary>
        public static string EverySecond => "* * * * *";

        /// <summary>
        /// 每分钟执行（每分钟的第 0 秒）
        /// </summary>
        public static string EveryMinute => "0 * * * *";

        /// <summary>
        /// 每小时执行（每小时的第 0 分 0 秒）
        /// </summary>
        public static string EveryHour => "0 0 * * *";

        /// <summary>
        /// 每天执行（每天的 00:00:00）
        /// </summary>
        public static string EveryDay => "0 0 0 * *";

        /// <summary>
        /// 每月执行（每月 1 日的 00:00:00）
        /// </summary>
        public static string EveryMonth => "0 0 0 1 *";

        /// <summary>
        /// 每隔 N 秒执行
        /// </summary>
        public static string EveryNSeconds(int n) => $"*/{n} * * * *";

        /// <summary>
        /// 每隔 N 分钟执行
        /// </summary>
        public static string EveryNMinutes(int n) => $"0 */{n} * * *";

        /// <summary>
        /// 每隔 N 小时执行
        /// </summary>
        public static string EveryNHours(int n) => $"0 0 */{n} * *";

        /// <summary>
        /// 每天指定时间执行
        /// </summary>
        /// <param name="hour">小时</param>
        /// <param name="minute">分钟</param>
        /// <param name="second">秒</param>
        public static string DailyAt(int hour, int minute = 0, int second = 0) => $"{second} {minute} {hour} * *";

        /// <summary>
        /// 每周指定时间执行（周一为 1，周日为 7）
        /// </summary>
        /// <param name="dayOfWeek">星期几（1-7）</param>
        /// <param name="hour">小时</param>
        /// <param name="minute">分钟</param>
        /// <param name="second">秒</param>
        public static string WeeklyAt(int dayOfWeek, int hour = 0, int minute = 0, int second = 0)
            => $"{second} {minute} {hour} * *";

        /// <summary>
        /// 每月指定日期时间执行
        /// </summary>
        /// <param name="day">日期</param>
        /// <param name="hour">小时</param>
        /// <param name="minute">分钟</param>
        /// <param name="second">秒</param>
        public static string MonthlyAt(int day, int hour = 0, int minute = 0, int second = 0)
            => $"{second} {minute} {hour} {day} *";

        #endregion
    }
}