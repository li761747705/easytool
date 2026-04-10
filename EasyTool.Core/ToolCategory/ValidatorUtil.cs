using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace EasyTool.ToolCategory
{
    /// <summary>
    /// 验证结果
    /// </summary>
    public class ValidationResult
    {
        /// <summary>
        /// 是否有效
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// 错误消息列表
        /// </summary>
        public List<string> Errors { get; set; } = new();

        /// <summary>
        /// 错误字段列表
        /// </summary>
        public List<string> ErrorFields { get; set; } = new();

        /// <summary>
        /// 创建成功的验证结果
        /// </summary>
        /// <returns>验证结果</returns>
        public static ValidationResult Success() => new() { IsValid = true };

        /// <summary>
        /// 创建失败的验证结果
        /// </summary>
        /// <param name="errors">错误消息数组</param>
        /// <returns>验证结果</returns>
        public static ValidationResult Failure(params string[] errors) => new() { IsValid = false, Errors = errors.ToList() };
    }

    /// <summary>
    /// 验证工具类
    /// 提供常用的数据验证功能
    /// </summary>
    public static class ValidatorUtil
    {
        #region 字符串验证

        /// <summary>
        /// 检查字符串是否为空或空白
        /// </summary>
        public static bool IsNullOrWhiteSpace(string? value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        /// <summary>
        /// 检查字符串是否为空
        /// </summary>
        public static bool IsNullOrEmpty(string? value)
        {
            return string.IsNullOrEmpty(value);
        }

        /// <summary>
        /// 检查字符串是否不为空
        /// </summary>
        public static bool IsNotNullOrEmpty(string? value)
        {
            return !string.IsNullOrEmpty(value);
        }

        /// <summary>
        /// 检查字符串长度是否在指定范围内
        /// </summary>
        public static bool IsLengthBetween(string? value, int minLength, int maxLength)
        {
            if (value == null)
                return minLength <= 0;

            return value.Length >= minLength && value.Length <= maxLength;
        }

        /// <summary>
        /// 检查字符串长度是否等于指定值
        /// </summary>
        public static bool IsLength(string? value, int length)
        {
            return value?.Length == length;
        }

        /// <summary>
        /// 检查字符串是否只包含数字
        /// </summary>
        public static bool IsNumeric(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            return value.All(char.IsDigit);
        }

        /// <summary>
        /// 检查字符串是否只包含字母
        /// </summary>
        public static bool IsAlpha(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            return value.All(char.IsLetter);
        }

        /// <summary>
        /// 检查字符串是否只包含字母和数字
        /// </summary>
        public static bool IsAlphanumeric(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            return value.All(c => char.IsLetterOrDigit(c));
        }

        /// <summary>
        /// 检查字符串是否匹配正则表达式
        /// </summary>
        public static bool IsMatch(string? value, string pattern)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            return Regex.IsMatch(value, pattern);
        }

        /// <summary>
        /// 检查字符串是否在指定值列表中
        /// </summary>
        public static bool IsIn(string? value, params string[] allowedValues)
        {
            return allowedValues.Contains(value);
        }

        /// <summary>
        /// 检查字符串是否以指定前缀开头
        /// </summary>
        public static bool StartsWith(string? value, string prefix, StringComparison comparison = StringComparison.Ordinal)
        {
            if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(prefix))
                return false;

            return value.StartsWith(prefix, comparison);
        }

        /// <summary>
        /// 检查字符串是否以指定后缀结尾
        /// </summary>
        public static bool EndsWith(string? value, string suffix, StringComparison comparison = StringComparison.Ordinal)
        {
            if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(suffix))
                return false;

            return value.EndsWith(suffix, comparison);
        }

        /// <summary>
        /// 检查字符串是否包含指定子串
        /// </summary>
        public static bool Contains(string? value, string substring, StringComparison comparison = StringComparison.Ordinal)
        {
            if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(substring))
                return false;

            return value.Contains(substring, comparison);
        }

        #endregion

        #region 数字验证

        /// <summary>
        /// 检查值是否在指定范围内
        /// </summary>
        public static bool IsBetween<T>(T value, T min, T max) where T : IComparable<T>
        {
            return value.CompareTo(min) >= 0 && value.CompareTo(max) <= 0;
        }

        /// <summary>
        /// 检查值是否大于指定值
        /// </summary>
        public static bool IsGreaterThan<T>(T value, T compare) where T : IComparable<T>
        {
            return value.CompareTo(compare) > 0;
        }

        /// <summary>
        /// 检查值是否大于等于指定值
        /// </summary>
        public static bool IsGreaterThanOrEqual<T>(T value, T compare) where T : IComparable<T>
        {
            return value.CompareTo(compare) >= 0;
        }

        /// <summary>
        /// 检查值是否小于指定值
        /// </summary>
        public static bool IsLessThan<T>(T value, T compare) where T : IComparable<T>
        {
            return value.CompareTo(compare) < 0;
        }

        /// <summary>
        /// 检查值是否小于等于指定值
        /// </summary>
        public static bool IsLessThanOrEqual<T>(T value, T compare) where T : IComparable<T>
        {
            return value.CompareTo(compare) <= 0;
        }

        /// <summary>
        /// 检查是否为正数
        /// </summary>
        public static bool IsPositive<T>(T value) where T : IComparable<T>
        {
            return value.CompareTo(default!) > 0;
        }

        /// <summary>
        /// 检查是否为负数
        /// </summary>
        public static bool IsNegative<T>(T value) where T : IComparable<T>
        {
            return value.CompareTo(default!) < 0;
        }

        /// <summary>
        /// 检查是否为零
        /// </summary>
        public static bool IsZero<T>(T value) where T : IComparable<T>
        {
            return value.CompareTo(default!) == 0;
        }

        /// <summary>
        /// 检查是否为偶数
        /// </summary>
        public static bool IsEven(int value)
        {
            return value % 2 == 0;
        }

        /// <summary>
        /// 检查是否为奇数
        /// </summary>
        public static bool IsOdd(int value)
        {
            return value % 2 != 0;
        }

        #endregion

        #region 集合验证

        /// <summary>
        /// 检查集合是否为空
        /// </summary>
        public static bool IsEmpty(IEnumerable? collection)
        {
            if (collection == null)
                return true;

            if (collection is ICollection col)
                return col.Count == 0;

            return !collection.Cast<object>().Any();
        }

        /// <summary>
        /// 检查集合是否不为空
        /// </summary>
        public static bool IsNotEmpty(IEnumerable? collection)
        {
            return !IsEmpty(collection);
        }

        /// <summary>
        /// 检查集合元素数量是否在指定范围内
        /// </summary>
        public static bool IsCountBetween(IEnumerable? collection, int minCount, int maxCount)
        {
            if (collection == null)
                return minCount <= 0;

            int count;
            if (collection is ICollection col)
            {
                count = col.Count;
            }
            else
            {
                count = collection.Cast<object>().Count();
            }

            return count >= minCount && count <= maxCount;
        }

        /// <summary>
        /// 检查集合是否包含指定元素
        /// </summary>
        public static bool Contains<T>(IEnumerable<T>? collection, T item)
        {
            if (collection == null)
                return false;

            return collection.Contains(item);
        }

        /// <summary>
        /// 检查集合是否包含所有指定元素
        /// </summary>
        public static bool ContainsAll<T>(IEnumerable<T>? collection, params T[] items)
        {
            if (collection == null || items == null)
                return false;

            return items.All(item => collection.Contains(item));
        }

        /// <summary>
        /// 检查集合是否包含任一指定元素
        /// </summary>
        public static bool ContainsAny<T>(IEnumerable<T>? collection, params T[] items)
        {
            if (collection == null || items == null)
                return false;

            return items.Any(item => collection.Contains(item));
        }

        #endregion

        #region 日期验证

        /// <summary>
        /// 检查日期是否在指定范围内
        /// </summary>
        public static bool IsBetween(DateTime value, DateTime min, DateTime max)
        {
            return value >= min && value <= max;
        }

        /// <summary>
        /// 检查是否为工作日（周一至周五）
        /// </summary>
        public static bool IsWeekday(DateTime value)
        {
            return value.DayOfWeek != DayOfWeek.Saturday && value.DayOfWeek != DayOfWeek.Sunday;
        }

        /// <summary>
        /// 检查是否为周末
        /// </summary>
        public static bool IsWeekend(DateTime value)
        {
            return value.DayOfWeek == DayOfWeek.Saturday || value.DayOfWeek == DayOfWeek.Sunday;
        }

        /// <summary>
        /// 检查是否为今天
        /// </summary>
        public static bool IsToday(DateTime value)
        {
            return value.Date == DateTime.Today;
        }

        /// <summary>
        /// 检查是否为过去的时间
        /// </summary>
        public static bool IsPast(DateTime value)
        {
            return value < DateTime.UtcNow;
        }

        /// <summary>
        /// 检查是否为未来的时间
        /// </summary>
        public static bool IsFuture(DateTime value)
        {
            return value > DateTime.UtcNow;
        }

        #endregion

        #region 类型验证

        /// <summary>
        /// 检查值是否为指定类型
        /// </summary>
        public static bool IsType<T>(object? value)
        {
            return value is T;
        }

        /// <summary>
        /// 检查值是否为 null
        /// </summary>
        public static bool IsNull(object? value)
        {
            return value == null;
        }

        /// <summary>
        /// 检查值是否不为 null
        /// </summary>
        public static bool IsNotNull(object? value)
        {
            return value != null;
        }

        /// <summary>
        /// 检查是否为默认值
        /// </summary>
        public static bool IsDefault<T>(T value)
        {
            return EqualityComparer<T>.Default.Equals(value, default);
        }

        #endregion

        #region 组合验证

        /// <summary>
        /// 组合多个验证条件（全部满足）
        /// </summary>
        public static bool All(params Func<bool>[] validators)
        {
            return validators.All(v => v());
        }

        /// <summary>
        /// 组合多个验证条件（任一满足）
        /// </summary>
        public static bool Any(params Func<bool>[] validators)
        {
            return validators.Any(v => v());
        }

        /// <summary>
        /// 验证并返回结果
        /// </summary>
        public static ValidationResult Validate(params (string Field, Func<bool> Validator, string ErrorMessage)[] rules)
        {
            var result = new ValidationResult { IsValid = true };

            foreach (var (field, validator, errorMessage) in rules)
            {
                if (!validator())
                {
                    result.IsValid = false;
                    result.Errors.Add(errorMessage);
                    result.ErrorFields.Add(field);
                }
            }

            return result;
        }

        #endregion
    }
}