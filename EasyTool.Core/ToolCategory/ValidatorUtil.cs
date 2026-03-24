using System;
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
        /// 是否验证通过
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// 错误信息列表
        /// </summary>
        public List<string> Errors { get; set; } = new();

        /// <summary>
        /// 创建成功结果
        /// </summary>
        public static ValidationResult Success => new() { IsValid = true };

        /// <summary>
        /// 创建失败结果
        /// </summary>
        public static ValidationResult Fail(params string[] errors) => new()
        {
            IsValid = false,
            Errors = errors.ToList()
        };

        /// <summary>
        /// 合并多个验证结果
        /// </summary>
        public static ValidationResult Combine(params ValidationResult[] results)
        {
            var combined = new ValidationResult { IsValid = true };

            foreach (var result in results)
            {
                if (!result.IsValid)
                {
                    combined.IsValid = false;
                    combined.Errors.AddRange(result.Errors);
                }
            }

            return combined;
        }
    }

    /// <summary>
    /// 验证规则构建器
    /// </summary>
    /// <typeparam name="T">值类型</typeparam>
    public class ValidatorBuilder<T>
    {
        private readonly List<Func<T, ValidationResult>> _rules = new();
        protected readonly string _fieldName;

        public ValidatorBuilder(string fieldName = "value")
        {
            _fieldName = fieldName;
        }

        /// <summary>
        /// 添加自定义验证规则
        /// </summary>
        public ValidatorBuilder<T> AddRule(Func<T, bool> rule, string errorMessage)
        {
            _rules.Add(value => rule(value)
                ? ValidationResult.Success
                : ValidationResult.Fail(errorMessage));
            return this;
        }

        /// <summary>
        /// 添加自定义验证规则
        /// </summary>
        public ValidatorBuilder<T> AddRule(Func<T, ValidationResult> rule)
        {
            _rules.Add(rule);
            return this;
        }

        #region 通用规则

        /// <summary>
        /// 不能为默认值
        /// </summary>
        public ValidatorBuilder<T> NotDefault(string? message = null)
        {
            _rules.Add(value => !EqualityComparer<T>.Default.Equals(value, default!)
                ? ValidationResult.Success
                : ValidationResult.Fail(message ?? $"{_fieldName}不能为默认值"));
            return this;
        }

        /// <summary>
        /// 满足条件
        /// </summary>
        public ValidatorBuilder<T> Must(Func<T, bool> predicate, string? message = null)
        {
            _rules.Add(value => predicate(value)
                ? ValidationResult.Success
                : ValidationResult.Fail(message ?? $"{_fieldName}不满足条件"));
            return this;
        }

        /// <summary>
        /// 枚举值验证
        /// </summary>
        public ValidatorBuilder<T> IsEnum(string? message = null)
        {
            _rules.Add(value =>
            {
                if (value == null)
                    return ValidationResult.Fail(message ?? $"{_fieldName}不能为空");

                var type = typeof(T);
                if (!type.IsEnum && (!Nullable.GetUnderlyingType(type)?.IsEnum ?? true))
                    return ValidationResult.Fail($"{_fieldName}不是枚举类型");

                var enumType = Nullable.GetUnderlyingType(type) ?? type;
                return Enum.IsDefined(enumType, value)
                    ? ValidationResult.Success
                    : ValidationResult.Fail(message ?? $"{_fieldName}不是有效的枚举值");
            });
            return this;
        }

        #endregion

        #region 数值规则

        /// <summary>
        /// 大于指定值
        /// </summary>
        public ValidatorBuilder<T> GreaterThan(T compareValue, string? message = null)
        {
            _rules.Add(value =>
            {
                if (value is IComparable<T> comparable)
                {
                    return comparable.CompareTo(compareValue) > 0
                        ? ValidationResult.Success
                        : ValidationResult.Fail(message ?? $"{_fieldName}必须大于 {compareValue}");
                }
                return ValidationResult.Fail($"{_fieldName}类型不可比较");
            });
            return this;
        }

        /// <summary>
        /// 大于等于指定值
        /// </summary>
        public ValidatorBuilder<T> GreaterThanOrEqual(T compareValue, string? message = null)
        {
            _rules.Add(value =>
            {
                if (value is IComparable<T> comparable)
                {
                    return comparable.CompareTo(compareValue) >= 0
                        ? ValidationResult.Success
                        : ValidationResult.Fail(message ?? $"{_fieldName}必须大于等于 {compareValue}");
                }
                return ValidationResult.Fail($"{_fieldName}类型不可比较");
            });
            return this;
        }

        /// <summary>
        /// 小于指定值
        /// </summary>
        public ValidatorBuilder<T> LessThan(T compareValue, string? message = null)
        {
            _rules.Add(value =>
            {
                if (value is IComparable<T> comparable)
                {
                    return comparable.CompareTo(compareValue) < 0
                        ? ValidationResult.Success
                        : ValidationResult.Fail(message ?? $"{_fieldName}必须小于 {compareValue}");
                }
                return ValidationResult.Fail($"{_fieldName}类型不可比较");
            });
            return this;
        }

        /// <summary>
        /// 小于等于指定值
        /// </summary>
        public ValidatorBuilder<T> LessThanOrEqual(T compareValue, string? message = null)
        {
            _rules.Add(value =>
            {
                if (value is IComparable<T> comparable)
                {
                    return comparable.CompareTo(compareValue) <= 0
                        ? ValidationResult.Success
                        : ValidationResult.Fail(message ?? $"{_fieldName}必须小于等于 {compareValue}");
                }
                return ValidationResult.Fail($"{_fieldName}类型不可比较");
            });
            return this;
        }

        /// <summary>
        /// 在指定范围内
        /// </summary>
        public ValidatorBuilder<T> InRange(T min, T max, string? message = null)
        {
            _rules.Add(value =>
            {
                if (value is IComparable<T> comparable)
                {
                    var valid = comparable.CompareTo(min) >= 0 && comparable.CompareTo(max) <= 0;
                    return valid
                        ? ValidationResult.Success
                        : ValidationResult.Fail(message ?? $"{_fieldName}必须在 {min} 和 {max} 之间");
                }
                return ValidationResult.Fail($"{_fieldName}类型不可比较");
            });
            return this;
        }

        #endregion

        #region 构建验证器

        /// <summary>
        /// 构建验证器
        /// </summary>
        public Func<T, ValidationResult> Build()
        {
            var rules = _rules.ToList();
            return value =>
            {
                var result = ValidationResult.Success;
                foreach (var rule in rules)
                {
                    var ruleResult = rule(value);
                    if (!ruleResult.IsValid)
                    {
                        result.IsValid = false;
                        result.Errors.AddRange(ruleResult.Errors);
                    }
                }
                return result;
            };
        }

        /// <summary>
        /// 验证值
        /// </summary>
        public ValidationResult Validate(T value)
        {
            return Build()(value);
        }

        #endregion
    }

    /// <summary>
    /// 字符串验证规则构建器
    /// </summary>
    public class StringValidatorBuilder : ValidatorBuilder<string?>
    {
        public StringValidatorBuilder(string fieldName = "value") : base(fieldName) { }

        /// <summary>
        /// 不能为空或空白
        /// </summary>
        public StringValidatorBuilder NotEmpty(string? message = null)
        {
            AddRule(value => !string.IsNullOrWhiteSpace(value),
                message ?? $"{_fieldName}不能为空");
            return this;
        }

        /// <summary>
        /// 最小长度
        /// </summary>
        public StringValidatorBuilder MinLength(int minLength, string? message = null)
        {
            AddRule(value => value != null && value.Length >= minLength,
                message ?? $"{_fieldName}长度不能小于 {minLength}");
            return this;
        }

        /// <summary>
        /// 最大长度
        /// </summary>
        public StringValidatorBuilder MaxLength(int maxLength, string? message = null)
        {
            AddRule(value => value == null || value.Length <= maxLength,
                message ?? $"{_fieldName}长度不能超过 {maxLength}");
            return this;
        }

        /// <summary>
        /// 长度范围
        /// </summary>
        public StringValidatorBuilder Length(int minLength, int maxLength, string? message = null)
        {
            AddRule(value => value != null && value.Length >= minLength && value.Length <= maxLength,
                message ?? $"{_fieldName}长度必须在 {minLength} 到 {maxLength} 之间");
            return this;
        }

        /// <summary>
        /// 匹配正则表达式
        /// </summary>
        public StringValidatorBuilder Matches(string pattern, string? message = null)
        {
            AddRule(value => value != null && Regex.IsMatch(value, pattern),
                message ?? $"{_fieldName}格式不正确");
            return this;
        }

        /// <summary>
        /// 匹配正则表达式
        /// </summary>
        public StringValidatorBuilder Matches(Regex regex, string? message = null)
        {
            AddRule(value => value != null && regex.IsMatch(value),
                message ?? $"{_fieldName}格式不正确");
            return this;
        }

        /// <summary>
        /// 邮箱格式
        /// </summary>
        public StringValidatorBuilder Email(string? message = null)
        {
            const string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Matches(emailPattern, message ?? $"{_fieldName}不是有效的邮箱地址");
        }

        /// <summary>
        /// 手机号格式（中国大陆）
        /// </summary>
        public StringValidatorBuilder Phone(string? message = null)
        {
            const string phonePattern = @"^1[3-9]\d{9}$";
            return Matches(phonePattern, message ?? $"{_fieldName}不是有效的手机号");
        }

        /// <summary>
        /// 身份证号格式（中国大陆）
        /// </summary>
        public StringValidatorBuilder IdCard(string? message = null)
        {
            const string idCardPattern = @"^[1-9]\d{5}(19|20)\d{2}(0[1-9]|1[0-2])(0[1-9]|[12]\d|3[01])\d{3}[\dXx]$";
            return Matches(idCardPattern, message ?? $"{_fieldName}不是有效的身份证号");
        }

        /// <summary>
        /// URL格式
        /// </summary>
        public StringValidatorBuilder Url(string? message = null)
        {
            const string urlPattern = @"^https?://[^\s/$.?#].[^\s]*$";
            return Matches(urlPattern, message ?? $"{_fieldName}不是有效的URL");
        }

        /// <summary>
        /// 纯数字
        /// </summary>
        public StringValidatorBuilder Numeric(string? message = null)
        {
            return Matches(@"^\d+$", message ?? $"{_fieldName}必须为纯数字");
        }

        /// <summary>
        /// 纯字母
        /// </summary>
        public StringValidatorBuilder Alpha(string? message = null)
        {
            return Matches(@"^[a-zA-Z]+$", message ?? $"{_fieldName}必须为纯字母");
        }

        /// <summary>
        /// 字母数字
        /// </summary>
        public StringValidatorBuilder Alphanumeric(string? message = null)
        {
            return Matches(@"^[a-zA-Z0-9]+$", message ?? $"{_fieldName}必须为字母或数字");
        }

        /// <summary>
        /// 包含数字
        /// </summary>
        public StringValidatorBuilder ContainsDigit(string? message = null)
        {
            return Matches(@"\d", message ?? $"{_fieldName}必须包含数字");
        }

        /// <summary>
        /// 包含小写字母
        /// </summary>
        public StringValidatorBuilder ContainsLower(string? message = null)
        {
            return Matches(@"[a-z]", message ?? $"{_fieldName}必须包含小写字母");
        }

        /// <summary>
        /// 包含大写字母
        /// </summary>
        public StringValidatorBuilder ContainsUpper(string? message = null)
        {
            return Matches(@"[A-Z]", message ?? $"{_fieldName}必须包含大写字母");
        }

        /// <summary>
        /// 包含特殊字符
        /// </summary>
        public StringValidatorBuilder ContainsSpecial(string? message = null)
        {
            return Matches(@"[!@#$%^&*(),.?"":{}|<>]", message ?? $"{_fieldName}必须包含特殊字符");
        }

        /// <summary>
        /// 密码强度验证
        /// </summary>
        /// <param name="minLength">最小长度</param>
        /// <param name="requireDigit">需要数字</param>
        /// <param name="requireLower">需要小写字母</param>
        /// <param name="requireUpper">需要大写字母</param>
        /// <param name="requireSpecial">需要特殊字符</param>
        /// <param name="message">错误消息</param>
        public StringValidatorBuilder Password(
            int minLength = 8,
            bool requireDigit = true,
            bool requireLower = true,
            bool requireUpper = true,
            bool requireSpecial = false,
            string? message = null)
        {
            MinLength(minLength);

            if (requireDigit) ContainsDigit();
            if (requireLower) ContainsLower();
            if (requireUpper) ContainsUpper();
            if (requireSpecial) ContainsSpecial();

            if (!string.IsNullOrEmpty(message))
            {
                AddRule(_ => false, message);
            }

            return this;
        }
    }

    /// <summary>
    /// 集合验证规则构建器
    /// </summary>
    public class CollectionValidatorBuilder<T> : ValidatorBuilder<IEnumerable<T>?>
    {
        public CollectionValidatorBuilder(string fieldName = "collection") : base(fieldName) { }

        /// <summary>
        /// 不能为空集合
        /// </summary>
        public CollectionValidatorBuilder<T> NotEmpty(string? message = null)
        {
            AddRule(value => value != null && value.Any(),
                message ?? $"{_fieldName}不能为空");
            return this;
        }

        /// <summary>
        /// 最小元素数量
        /// </summary>
        public CollectionValidatorBuilder<T> MinCount(int minCount, string? message = null)
        {
            AddRule(value => value != null && value.Count() >= minCount,
                message ?? $"{_fieldName}元素数量不能少于 {minCount}");
            return this;
        }

        /// <summary>
        /// 最大元素数量
        /// </summary>
        public CollectionValidatorBuilder<T> MaxCount(int maxCount, string? message = null)
        {
            AddRule(value => value == null || value.Count() <= maxCount,
                message ?? $"{_fieldName}元素数量不能超过 {maxCount}");
            return this;
        }

        /// <summary>
        /// 元素数量范围
        /// </summary>
        public CollectionValidatorBuilder<T> Count(int minCount, int maxCount, string? message = null)
        {
            AddRule(value =>
            {
                if (value == null) return false;
                var count = value.Count();
                return count >= minCount && count <= maxCount;
            }, message ?? $"{_fieldName}元素数量必须在 {minCount} 到 {maxCount} 之间");
            return this;
        }

        /// <summary>
        /// 所有元素满足条件
        /// </summary>
        public CollectionValidatorBuilder<T> All(Func<T, bool> predicate, string? message = null)
        {
            AddRule(value => value == null || value.All(predicate),
                message ?? $"{_fieldName}中存在不满足条件的元素");
            return this;
        }

        /// <summary>
        /// 至少一个元素满足条件
        /// </summary>
        public CollectionValidatorBuilder<T> Any(Func<T, bool> predicate, string? message = null)
        {
            AddRule(value => value != null && value.Any(predicate),
                message ?? $"{_fieldName}中没有满足条件的元素");
            return this;
        }

        /// <summary>
        /// 不包含重复元素
        /// </summary>
        public CollectionValidatorBuilder<T> Distinct(string? message = null)
        {
            AddRule(value =>
            {
                if (value == null) return true;
                var list = value.ToList();
                return list.Count == list.Distinct().Count();
            }, message ?? $"{_fieldName}包含重复元素");
            return this;
        }

        /// <summary>
        /// 包含指定元素
        /// </summary>
        public CollectionValidatorBuilder<T> Contains(T item, string? message = null)
        {
            AddRule(value => value != null && value.Contains(item),
                message ?? $"{_fieldName}不包含指定元素");
            return this;
        }

    }

    /// <summary>
    /// 通用验证工具类
    /// </summary>
    public static class ValidatorUtil
    {
        /// <summary>
        /// 创建字符串验证器
        /// </summary>
        public static StringValidatorBuilder ForString(string fieldName = "value")
        {
            return new StringValidatorBuilder(fieldName);
        }

        /// <summary>
        /// 创建数值验证器
        /// </summary>
        public static ValidatorBuilder<T> ForNumber<T>(string fieldName = "value") where T : IComparable<T>
        {
            return new ValidatorBuilder<T>(fieldName);
        }

        /// <summary>
        /// 创建集合验证器
        /// </summary>
        public static CollectionValidatorBuilder<T> ForCollection<T>(string fieldName = "collection")
        {
            return new CollectionValidatorBuilder<T>(fieldName);
        }

        /// <summary>
        /// 创建自定义验证器
        /// </summary>
        public static ValidatorBuilder<T> For<T>(string fieldName = "value")
        {
            return new ValidatorBuilder<T>(fieldName);
        }

        #region 快捷验证方法

        /// <summary>
        /// 验证字符串不为空
        /// </summary>
        public static bool IsNotEmpty(string? value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }

        /// <summary>
        /// 验证邮箱格式
        /// </summary>
        public static bool IsEmail(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;
            return Regex.IsMatch(value, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }

        /// <summary>
        /// 验证手机号格式（中国大陆）
        /// </summary>
        public static bool IsPhone(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;
            return Regex.IsMatch(value, @"^1[3-9]\d{9}$");
        }

        /// <summary>
        /// 验证身份证号格式（中国大陆）
        /// </summary>
        public static bool IsIdCard(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;
            return Regex.IsMatch(value, @"^[1-9]\d{5}(19|20)\d{2}(0[1-9]|1[0-2])(0[1-9]|[12]\d|3[01])\d{3}[\dXx]$");
        }

        /// <summary>
        /// 验证URL格式
        /// </summary>
        public static bool IsUrl(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;
            return Regex.IsMatch(value, @"^https?://[^\s/$.?#].[^\s]*$", RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// 验证是否为纯数字
        /// </summary>
        public static bool IsNumeric(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;
            return Regex.IsMatch(value, @"^\d+$");
        }

        /// <summary>
        /// 验证是否在范围内
        /// </summary>
        public static bool InRange<T>(T value, T min, T max) where T : IComparable<T>
        {
            if (value == null)
                return false;
            return value.CompareTo(min) >= 0 && value.CompareTo(max) <= 0;
        }

        /// <summary>
        /// 验证字符串长度
        /// </summary>
        public static bool LengthInRange(string? value, int minLength, int maxLength)
        {
            if (value == null)
                return minLength <= 0;
            return value.Length >= minLength && value.Length <= maxLength;
        }

        /// <summary>
        /// 验证集合不为空
        /// </summary>
        public static bool IsNotEmpty<T>(IEnumerable<T>? collection)
        {
            return collection != null && collection.Any();
        }

        /// <summary>
        /// 验证集合元素数量
        /// </summary>
        public static bool CountInRange<T>(IEnumerable<T>? collection, int minCount, int maxCount)
        {
            if (collection == null)
                return minCount <= 0;
            var count = collection.Count();
            return count >= minCount && count <= maxCount;
        }

        #endregion
    }
}
