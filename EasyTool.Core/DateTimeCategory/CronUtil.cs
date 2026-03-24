using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace EasyTool.DateTimeCategory
{
    /// <summary>
    /// Cron 表达式工具类
    /// 支持 Cron 表达式解析、验证和下一次执行时间计算
    /// </summary>
    public static class CronUtil
    {
        /// <summary>
        /// 解析 Cron 表达式
        /// </summary>
        public static CronExpression Parse(string cronExpression)
        {
            return new CronExpression(cronExpression);
        }

        /// <summary>
        /// 验证 Cron 表达式是否有效
        /// </summary>
        public static bool IsValid(string cronExpression)
        {
            try
            {
                new CronExpression(cronExpression);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 获取下一次执行时间
        /// </summary>
        public static DateTime? GetNextExecution(string cronExpression, DateTime from)
        {
            return Parse(cronExpression).GetNextExecution(from);
        }

        /// <summary>
        /// 获取接下来的N次执行时间
        /// </summary>
        public static IEnumerable<DateTime> GetNextExecutions(string cronExpression, DateTime from, int count)
        {
            return Parse(cronExpression).GetNextExecutions(from, count);
        }
    }

    /// <summary>
    /// Cron 表达式
    /// </summary>
    public class CronExpression
    {
        private static readonly int[] MonthDays = { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };

        private readonly HashSet<int> _seconds;
        private readonly HashSet<int> _minutes;
        private readonly HashSet<int> _hours;
        private readonly HashSet<int> _daysOfMonth;
        private readonly HashSet<int> _months;
        private readonly HashSet<int> _daysOfWeek;

        /// <summary>
        /// 原始表达式
        /// </summary>
        public string Expression { get; }

        /// <summary>
        /// 创建 Cron 表达式
        /// </summary>
        public CronExpression(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
                throw new ArgumentException("Cron expression cannot be empty");

            Expression = expression;

            var parts = expression.Trim().Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 5 && parts.Length != 6)
                throw new ArgumentException("Cron expression must have 5 or 6 fields");

            int offset = parts.Length == 6 ? 0 : 1;

            _seconds = parts.Length == 6 ? ParseField(parts[0], 0, 59) : new HashSet<int> { 0 };
            _minutes = ParseField(parts[offset], 0, 59);
            _hours = ParseField(parts[offset + 1], 0, 23);
            _daysOfMonth = ParseField(parts[offset + 2], 1, 31);
            _months = ParseField(parts[offset + 3], 1, 12);
            _daysOfWeek = ParseField(parts[offset + 4], 0, 6);
        }

        /// <summary>
        /// 获取下一次执行时间
        /// </summary>
        public DateTime? GetNextExecution(DateTime from)
        {
            return GetNextExecutions(from, 1).FirstOrDefault();
        }

        /// <summary>
        /// 获取接下来的N次执行时间
        /// </summary>
        public IEnumerable<DateTime> GetNextExecutions(DateTime from, int count)
        {
            var current = from.AddSeconds(1);
            int found = 0;
            int maxIterations = 366 * 24 * 60 * 60; // 最多查找一年

            while (found < count && maxIterations-- > 0)
            {
                if (Matches(current))
                {
                    yield return current;
                    found++;
                    current = current.AddSeconds(1);
                }
                else
                {
                    current = SkipToNextCandidate(current);
                }
            }
        }

        /// <summary>
        /// 判断指定时间是否匹配
        /// </summary>
        public bool Matches(DateTime time)
        {
            if (!_seconds.Contains(time.Second)) return false;
            if (!_minutes.Contains(time.Minute)) return false;
            if (!_hours.Contains(time.Hour)) return false;
            if (!_months.Contains(time.Month)) return false;

            // 日和周是"或"关系
            bool dayMatch = _daysOfMonth.Contains(time.Day);
            bool weekMatch = _daysOfWeek.Contains((int)time.DayOfWeek);

            // 如果两者都设置了，只要有一个匹配即可
            // 如果其中一个设置为*，则另一个起作用
            if (_daysOfMonth.Contains(-1) && _daysOfWeek.Contains(-1))
                return true;
            if (_daysOfMonth.Contains(-1))
                return weekMatch;
            if (_daysOfWeek.Contains(-1))
                return dayMatch;

            return dayMatch || weekMatch;
        }

        private DateTime SkipToNextCandidate(DateTime current)
        {
            // 优化：跳过不可能匹配的时间
            if (!_months.Contains(current.Month))
            {
                // 跳到下个月
                return new DateTime(current.Year, current.Month, 1).AddMonths(1);
            }

            if (!_hours.Contains(current.Hour))
            {
                // 跳到下一个小时
                return current.AddHours(1).AddMinutes(-current.Minute).AddSeconds(-current.Second);
            }

            if (!_minutes.Contains(current.Minute))
            {
                // 跳到下一分钟
                return current.AddMinutes(1).AddSeconds(-current.Second);
            }

            // 逐秒查找
            return current.AddSeconds(1);
        }

        private static HashSet<int> ParseField(string field, int min, int max)
        {
            var result = new HashSet<int>();
            int wildcard = -1;

            // 处理 L (Last)
            if (field == "L")
            {
                result.Add(max);
                return result;
            }

            // 处理 * 或 ?
            if (field == "*" || field == "?")
            {
                result.Add(wildcard);
                return result;
            }

            // 分割逗号分隔的部分
            foreach (var part in field.Split(','))
            {
                string currentPart = part.Trim();

                // 处理步长
                int step = 1;
                if (currentPart.Contains('/'))
                {
                    var stepParts = currentPart.Split('/');
                    currentPart = stepParts[0];
                    step = int.Parse(stepParts[1]);
                }

                // 处理范围
                int start, end;
                if (currentPart == "*")
                {
                    start = min;
                    end = max;
                }
                else if (currentPart.Contains('-'))
                {
                    var rangeParts = currentPart.Split('-');
                    start = int.Parse(rangeParts[0]);
                    end = int.Parse(rangeParts[1]);
                }
                else
                {
                    start = end = int.Parse(currentPart);
                }

                for (int i = start; i <= end; i += step)
                {
                    if (i >= min && i <= max)
                        result.Add(i);
                }
            }

            return result;
        }

        /// <summary>
        /// 获取可读的描述
        /// </summary>
        public string GetDescription()
        {
            var parts = Expression.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            int offset = parts.Length == 6 ? 0 : 1;

            var desc = new System.Text.StringBuilder();
            desc.Append("在");

            if (_hours.Contains(-1) && _minutes.Contains(-1))
                desc.Append("每分钟");
            else if (_hours.Contains(-1))
                desc.Append($"每小时的第{_stringify(_minutes)}分钟");
            else
                desc.Append($"{_stringify(_hours)}时{_stringify(_minutes)}分");

            if (!_daysOfMonth.Contains(-1) || !_daysOfWeek.Contains(-1))
            {
                desc.Append("，");
                if (!_daysOfMonth.Contains(-1))
                    desc.Append($"每月{_stringify(_daysOfMonth)}日");
                if (!_daysOfWeek.Contains(-1))
                {
                    if (!_daysOfMonth.Contains(-1)) desc.Append("或");
                    desc.Append($"每周{_stringify(_daysOfWeek)}");
                }
            }

            if (!_months.Contains(-1))
                desc.Append($"，{_stringify(_months)}月");

            desc.Append("执行");

            return desc.ToString();
        }

        private static string _stringify(HashSet<int> set)
        {
            if (set.Contains(-1)) return "每";
            var sorted = set.OrderBy(x => x).ToList();
            if (sorted.Count == 1) return sorted[0].ToString();
            return string.Join(",", sorted);
        }

        public override string ToString() => Expression;
    }

    /// <summary>
    /// 常用 Cron 表达式
    /// </summary>
    public static class CronExpressions
    {
        /// <summary>每分钟</summary>
        public static string EveryMinute => "* * * * *";

        /// <summary>每小时</summary>
        public static string EveryHour => "0 * * * *";

        /// <summary>每天午夜</summary>
        public static string Daily => "0 0 * * *";

        /// <summary>每天中午</summary>
        public static string DailyNoon => "0 12 * * *";

        /// <summary>每周一</summary>
        public static string WeeklyMonday => "0 0 * * 1";

        /// <summary>每月1号</summary>
        public static string Monthly => "0 0 1 * *";

        /// <summary>每年1月1日</summary>
        public static string Yearly => "0 0 1 1 *";

        /// <summary>工作日</summary>
        public static string Weekdays => "0 0 * * 1-5";

        /// <summary>周末</summary>
        public static string Weekends => "0 0 * * 0,6";

        /// <summary>
        /// 每5分钟
        /// </summary>
        public static string EveryNMinutes(int n) => $"*/{n} * * * *";

        /// <summary>
        /// 每N小时
        /// </summary>
        public static string EveryNHours(int n) => $"0 */{n} * * *";

        /// <summary>
        /// 每天指定时间
        /// </summary>
        public static string DailyAt(int hour, int minute = 0) => $"{minute} {hour} * * *";
    }
}
