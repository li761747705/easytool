using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace EasyTool.ValidationCategory
{
    /// <summary>
    /// 流式验证器
    /// </summary>
    public class FluentValidator<T>
    {
        private readonly T _value;
        private readonly string _propertyName;
        private readonly List<string> _errors = new();
        private bool _stopOnFirstFailure;

        private FluentValidator(T value, string propertyName)
        {
            _value = value;
            _propertyName = propertyName;
        }

        /// <summary>
        /// 开始验证
        /// </summary>
        public static FluentValidator<T> For(T value, string propertyName = "")
        {
            return new FluentValidator<T>(value, propertyName);
        }

        /// <summary>
        /// 遇到第一个错误就停止
        /// </summary>
        public FluentValidator<T> StopOnFirstFailure()
        {
            _stopOnFirstFailure = true;
            return this;
        }

        /// <summary>
        /// 自定义验证
        /// </summary>
        public FluentValidator<T> Must(Func<T, bool> predicate, string errorMessage)
        {
            if (ShouldValidate() && !predicate(_value))
            {
                AddError(errorMessage);
            }
            return this;
        }

        /// <summary>
        /// 自定义异步验证
        /// </summary>
        public async System.Threading.Tasks.Task<FluentValidator<T>> MustAsync(Func<T, System.Threading.Tasks.Task<bool>> predicate, string errorMessage)
        {
            if (ShouldValidate() && !await predicate(_value))
            {
                AddError(errorMessage);
            }
            return this;
        }

        /// <summary>
        /// 不能为null
        /// </summary>
        public FluentValidator<T> NotNull(string? errorMessage = null)
        {
            if (ShouldValidate() && _value == null)
            {
                AddError(errorMessage ?? $"{_propertyName}不能为空");
            }
            return this;
        }

        /// <summary>
        /// 字符串不能为空
        /// </summary>
        public FluentValidator<T> NotEmpty(string? errorMessage = null)
        {
            if (ShouldValidate() && string.IsNullOrEmpty(_value as string))
            {
                AddError(errorMessage ?? $"{_propertyName}不能为空");
            }
            return this;
        }

        /// <summary>
        /// 字符串不能为空白
        /// </summary>
        public FluentValidator<T> NotWhiteSpace(string? errorMessage = null)
        {
            if (ShouldValidate() && string.IsNullOrWhiteSpace(_value as string))
            {
                AddError(errorMessage ?? $"{_propertyName}不能为空白");
            }
            return this;
        }

        /// <summary>
        /// 字符串长度范围
        /// </summary>
        public FluentValidator<T> Length(int min, int max, string? errorMessage = null)
        {
            if (ShouldValidate())
            {
                var str = _value as string;
                if (str != null && (str.Length < min || str.Length > max))
                {
                    AddError(errorMessage ?? $"{_propertyName}长度必须在{min}到{max}之间");
                }
            }
            return this;
        }

        /// <summary>
        /// 最小长度
        /// </summary>
        public FluentValidator<T> MinLength(int min, string? errorMessage = null)
        {
            if (ShouldValidate())
            {
                var str = _value as string;
                if (str != null && str.Length < min)
                {
                    AddError(errorMessage ?? $"{_propertyName}长度不能小于{min}");
                }
            }
            return this;
        }

        /// <summary>
        /// 最大长度
        /// </summary>
        public FluentValidator<T> MaxLength(int max, string? errorMessage = null)
        {
            if (ShouldValidate())
            {
                var str = _value as string;
                if (str != null && str.Length > max)
                {
                    AddError(errorMessage ?? $"{_propertyName}长度不能超过{max}");
                }
            }
            return this;
        }

        /// <summary>
        /// 数值范围
        /// </summary>
        public FluentValidator<T> InRange(IComparable min, IComparable max, string? errorMessage = null)
        {
            if (ShouldValidate() && _value is IComparable comparable)
            {
                if (comparable.CompareTo(min) < 0 || comparable.CompareTo(max) > 0)
                {
                    AddError(errorMessage ?? $"{_propertyName}必须在{min}到{max}之间");
                }
            }
            return this;
        }

        /// <summary>
        /// 大于指定值
        /// </summary>
        public FluentValidator<T> GreaterThan(IComparable threshold, string? errorMessage = null)
        {
            if (ShouldValidate() && _value is IComparable comparable)
            {
                if (comparable.CompareTo(threshold) <= 0)
                {
                    AddError(errorMessage ?? $"{_propertyName}必须大于{threshold}");
                }
            }
            return this;
        }

        /// <summary>
        /// 小于指定值
        /// </summary>
        public FluentValidator<T> LessThan(IComparable threshold, string? errorMessage = null)
        {
            if (ShouldValidate() && _value is IComparable comparable)
            {
                if (comparable.CompareTo(threshold) >= 0)
                {
                    AddError(errorMessage ?? $"{_propertyName}必须小于{threshold}");
                }
            }
            return this;
        }

        /// <summary>
        /// 正则匹配
        /// </summary>
        public FluentValidator<T> Matches(string pattern, string? errorMessage = null)
        {
            if (ShouldValidate() && _value != null)
            {
                if (!Regex.IsMatch(_value.ToString() ?? "", pattern))
                {
                    AddError(errorMessage ?? $"{_propertyName}格式不正确");
                }
            }
            return this;
        }

        /// <summary>
        /// 邮箱格式
        /// </summary>
        public FluentValidator<T> Email(string? errorMessage = null)
        {
            return Matches(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", errorMessage ?? $"{_propertyName}不是有效的邮箱地址");
        }

        /// <summary>
        /// 手机号格式（中国）
        /// </summary>
        public FluentValidator<T> Phone(string? errorMessage = null)
        {
            return Matches(@"^1[3-9]\d{9}$", errorMessage ?? $"{_propertyName}不是有效的手机号");
        }

        /// <summary>
        /// 身份证号格式（中国）
        /// </summary>
        public FluentValidator<T> IdCard(string? errorMessage = null)
        {
            return Matches(@"^\d{17}[\dXx]$", errorMessage ?? $"{_propertyName}不是有效的身份证号");
        }

        /// <summary>
        /// URL格式
        /// </summary>
        public FluentValidator<T> Url(string? errorMessage = null)
        {
            return Matches(@"^https?://[^\s]+$", errorMessage ?? $"{_propertyName}不是有效的URL");
        }

        /// <summary>
        /// IP地址格式
        /// </summary>
        public FluentValidator<T> IpAddress(string? errorMessage = null)
        {
            return Matches(@"^(\d{1,3}\.){3}\d{1,3}$", errorMessage ?? $"{_propertyName}不是有效的IP地址");
        }

        /// <summary>
        /// 在指定值列表中
        /// </summary>
        public FluentValidator<T> In(IEnumerable<T> values, string? errorMessage = null)
        {
            if (ShouldValidate() && !values.Contains(_value))
            {
                AddError(errorMessage ?? $"{_propertyName}必须是有效值之一");
            }
            return this;
        }

        /// <summary>
        /// 等于指定值
        /// </summary>
        public FluentValidator<T> Equal(T expected, string? errorMessage = null)
        {
            if (ShouldValidate() && !EqualityComparer<T>.Default.Equals(_value, expected))
            {
                AddError(errorMessage ?? $"{_propertyName}必须等于{expected}");
            }
            return this;
        }

        /// <summary>
        /// 不等于指定值
        /// </summary>
        public FluentValidator<T> NotEqual(T unexpected, string? errorMessage = null)
        {
            if (ShouldValidate() && EqualityComparer<T>.Default.Equals(_value, unexpected))
            {
                AddError(errorMessage ?? $"{_propertyName}不能等于{unexpected}");
            }
            return this;
        }

        /// <summary>
        /// 集合不为空
        /// </summary>
        public FluentValidator<T> NotNullOrEmpty(string? errorMessage = null)
        {
            if (ShouldValidate())
            {
                if (_value is System.Collections.ICollection collection && collection.Count == 0)
                {
                    AddError(errorMessage ?? $"{_propertyName}不能为空集合");
                }
                else if (_value is System.Collections.IEnumerable enumerable && !enumerable.Cast<object>().Any())
                {
                    AddError(errorMessage ?? $"{_propertyName}不能为空集合");
                }
            }
            return this;
        }

        /// <summary>
        /// 条件验证
        /// </summary>
        public FluentValidator<T> When(Func<T, bool> condition, Action<FluentValidator<T>> action)
        {
            if (condition(_value))
            {
                action(this);
            }
            return this;
        }

        /// <summary>
        /// 反条件验证
        /// </summary>
        public FluentValidator<T> Unless(Func<T, bool> condition, Action<FluentValidator<T>> action)
        {
            if (!condition(_value))
            {
                action(this);
            }
            return this;
        }

        /// <summary>
        /// 获取验证结果
        /// </summary>
        public ValidationResult GetResult()
        {
            return new ValidationResult(_errors.Count == 0, _errors);
        }

        /// <summary>
        /// 是否验证通过
        /// </summary>
        public bool IsValid => _errors.Count == 0;

        /// <summary>
        /// 获取错误消息
        /// </summary>
        public IReadOnlyList<string> Errors => _errors.AsReadOnly();

        /// <summary>
        /// 获取第一条错误消息
        /// </summary>
        public string? FirstError => _errors.FirstOrDefault();

        /// <summary>
        /// 抛出验证异常
        /// </summary>
        public void ThrowIfInvalid()
        {
            if (!IsValid)
            {
                throw new ValidationException(_errors);
            }
        }

        private bool ShouldValidate() => !_stopOnFirstFailure || _errors.Count == 0;

        private void AddError(string error)
        {
            _errors.Add(error);
        }
    }

    /// <summary>
    /// 验证结果
    /// </summary>
    public class ValidationResult
    {
        /// <summary>
        /// 是否有效
        /// </summary>
        public bool IsValid { get; }

        /// <summary>
        /// 错误消息
        /// </summary>
        public IReadOnlyList<string> Errors { get; }

        /// <summary>
        /// 第一条错误消息
        /// </summary>
        public string? FirstError => Errors.FirstOrDefault();

        public ValidationResult(bool isValid, List<string> errors)
        {
            IsValid = isValid;
            Errors = errors.AsReadOnly();
        }

        public static ValidationResult Success() => new ValidationResult(true, new List<string>());

        public static ValidationResult Failure(params string[] errors) => new ValidationResult(false, errors.ToList());
    }

    /// <summary>
    /// 验证异常
    /// </summary>
    public class ValidationException : Exception
    {
        /// <summary>
        /// 错误消息列表
        /// </summary>
        public IReadOnlyList<string> Errors { get; }

        public ValidationException(IEnumerable<string> errors)
            : base(string.Join("; ", errors))
        {
            Errors = errors.ToList().AsReadOnly();
        }

        public ValidationException(string error)
            : base(error)
        {
            Errors = new List<string> { error }.AsReadOnly();
        }
    }

    /// <summary>
    /// 验证器扩展
    /// </summary>
    public static class FluentValidatorExtensions
    {
        /// <summary>
        /// 验证对象
        /// </summary>
        public static FluentValidator<T> Validate<T>(this T value, string propertyName = "")
        {
            return FluentValidator<T>.For(value, propertyName);
        }
    }
}
