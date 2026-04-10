using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EasyTool.ValidationCategory
{
    /// <summary>
    /// 验证规则构建器，支持链式调用构建复杂验证规则
    /// </summary>
    public class ValidationRuleBuilder<T>
    {
        private readonly List<ValidationRule<T>> _rules = new();
        private string? _currentProperty;
        private string? _currentErrorMessage;

        /// <summary>
        /// 为指定属性添加规则
        /// </summary>
        public ValidationRuleBuilder<T> RuleFor<TProperty>(Expression<Func<T, TProperty>> propertyExpression)
        {
            _currentProperty = GetPropertyName(propertyExpression);
            _currentErrorMessage = null;
            return this;
        }

        /// <summary>
        /// 设置自定义错误消息
        /// </summary>
        public ValidationRuleBuilder<T> WithMessage(string errorMessage)
        {
            _currentErrorMessage = errorMessage;
            return this;
        }

        /// <summary>
        /// 必须满足条件
        /// </summary>
        public ValidationRuleBuilder<T> Must<TProperty>(Expression<Func<T, TProperty>> propertyExpression, Func<TProperty, bool> predicate)
        {
            var propertyName = GetPropertyName(propertyExpression);
            var getter = propertyExpression.Compile();
            _rules.Add(new ValidationRule<T>
            {
                PropertyName = propertyName,
                Validate = obj => predicate(getter(obj)),
                ErrorMessage = _currentErrorMessage ?? $"{propertyName}验证失败"
            });
            return this;
        }

        /// <summary>
        /// 不能为null
        /// </summary>
        public ValidationRuleBuilder<T> NotNull<TProperty>(Expression<Func<T, TProperty>> propertyExpression)
        {
            var propertyName = GetPropertyName(propertyExpression);
            var getter = propertyExpression.Compile();
            _rules.Add(new ValidationRule<T>
            {
                PropertyName = propertyName,
                Validate = obj => getter(obj) != null,
                ErrorMessage = _currentErrorMessage ?? $"{propertyName}不能为空"
            });
            return this;
        }

        /// <summary>
        /// 字符串不能为空
        /// </summary>
        public ValidationRuleBuilder<T> NotEmpty(Expression<Func<T, string?>> propertyExpression)
        {
            var propertyName = GetPropertyName(propertyExpression);
            var getter = propertyExpression.Compile();
            _rules.Add(new ValidationRule<T>
            {
                PropertyName = propertyName,
                Validate = obj => !string.IsNullOrEmpty(getter(obj)),
                ErrorMessage = _currentErrorMessage ?? $"{propertyName}不能为空"
            });
            return this;
        }

        /// <summary>
        /// 字符串不能为空白
        /// </summary>
        public ValidationRuleBuilder<T> NotWhiteSpace(Expression<Func<T, string?>> propertyExpression)
        {
            var propertyName = GetPropertyName(propertyExpression);
            var getter = propertyExpression.Compile();
            _rules.Add(new ValidationRule<T>
            {
                PropertyName = propertyName,
                Validate = obj => !string.IsNullOrWhiteSpace(getter(obj)),
                ErrorMessage = _currentErrorMessage ?? $"{propertyName}不能为空白"
            });
            return this;
        }

        /// <summary>
        /// 字符串长度范围
        /// </summary>
        public ValidationRuleBuilder<T> Length(Expression<Func<T, string?>> propertyExpression, int min, int max)
        {
            var propertyName = GetPropertyName(propertyExpression);
            var getter = propertyExpression.Compile();
            _rules.Add(new ValidationRule<T>
            {
                PropertyName = propertyName,
                Validate = obj =>
                {
                    var value = getter(obj);
                    return value == null || (value.Length >= min && value.Length <= max);
                },
                ErrorMessage = _currentErrorMessage ?? $"{propertyName}长度必须在{min}到{max}之间"
            });
            return this;
        }

        /// <summary>
        /// 数值范围
        /// </summary>
        public ValidationRuleBuilder<T> InRange<TProperty>(Expression<Func<T, TProperty>> propertyExpression, TProperty min, TProperty max)
            where TProperty : IComparable<TProperty>
        {
            var propertyName = GetPropertyName(propertyExpression);
            var getter = propertyExpression.Compile();
            _rules.Add(new ValidationRule<T>
            {
                PropertyName = propertyName,
                Validate = obj =>
                {
                    var value = getter(obj);
                    return value == null || (value.CompareTo(min) >= 0 && value.CompareTo(max) <= 0);
                },
                ErrorMessage = _currentErrorMessage ?? $"{propertyName}必须在{min}到{max}之间"
            });
            return this;
        }

        /// <summary>
        /// 正则匹配
        /// </summary>
        public ValidationRuleBuilder<T> Matches(Expression<Func<T, string?>> propertyExpression, string pattern)
        {
            var propertyName = GetPropertyName(propertyExpression);
            var getter = propertyExpression.Compile();
            var regex = new Regex(pattern);
            _rules.Add(new ValidationRule<T>
            {
                PropertyName = propertyName,
                Validate = obj =>
                {
                    var value = getter(obj);
                    return value == null || regex.IsMatch(value);
                },
                ErrorMessage = _currentErrorMessage ?? $"{propertyName}格式不正确"
            });
            return this;
        }

        /// <summary>
        /// 邮箱格式
        /// </summary>
        public ValidationRuleBuilder<T> Email(Expression<Func<T, string?>> propertyExpression)
        {
            return Matches(propertyExpression, @"^[^@\s]+@[^@\s]+\.[^@\s]+$").WithMessage(_currentErrorMessage ?? "邮箱格式不正确");
        }

        /// <summary>
        /// 手机号格式（中国）
        /// </summary>
        public ValidationRuleBuilder<T> Phone(Expression<Func<T, string?>> propertyExpression)
        {
            return Matches(propertyExpression, @"^1[3-9]\d{9}$").WithMessage(_currentErrorMessage ?? "手机号格式不正确");
        }

        /// <summary>
        /// URL格式
        /// </summary>
        public ValidationRuleBuilder<T> Url(Expression<Func<T, string?>> propertyExpression)
        {
            return Matches(propertyExpression, @"^https?://[^\s]+$").WithMessage(_currentErrorMessage ?? "URL格式不正确");
        }

        /// <summary>
        /// IPv4地址格式
        /// </summary>
        public ValidationRuleBuilder<T> IPv4(Expression<Func<T, string?>> propertyExpression)
        {
            return Matches(propertyExpression, @"^(\d{1,3}\.){3}\d{1,3}$").WithMessage(_currentErrorMessage ?? "IPv4地址格式不正确");
        }

        /// <summary>
        /// 身份证号格式（中国）
        /// </summary>
        public ValidationRuleBuilder<T> IdCard(Expression<Func<T, string?>> propertyExpression)
        {
            var propertyName = GetPropertyName(propertyExpression);
            var getter = propertyExpression.Compile();
            _rules.Add(new ValidationRule<T>
            {
                PropertyName = propertyName,
                Validate = obj =>
                {
                    var value = getter(obj);
                    if (string.IsNullOrEmpty(value)) return true;
                    return IsValidIdCard(value);
                },
                ErrorMessage = _currentErrorMessage ?? "身份证号格式不正确"
            });
            return this;
        }

        /// <summary>
        /// 集合不为空
        /// </summary>
        public ValidationRuleBuilder<T> NotEmpty<TProperty>(Expression<Func<T, IEnumerable<TProperty>>> propertyExpression)
        {
            var propertyName = GetPropertyName(propertyExpression);
            var getter = propertyExpression.Compile();
            _rules.Add(new ValidationRule<T>
            {
                PropertyName = propertyName,
                Validate = obj =>
                {
                    var value = getter(obj);
                    return value != null && value.Any();
                },
                ErrorMessage = _currentErrorMessage ?? $"{propertyName}不能为空集合"
            });
            return this;
        }

        /// <summary>
        /// 集合元素数量范围
        /// </summary>
        public ValidationRuleBuilder<T> CollectionLength<TProperty>(Expression<Func<T, IEnumerable<TProperty>>> propertyExpression, int min, int max)
        {
            var propertyName = GetPropertyName(propertyExpression);
            var getter = propertyExpression.Compile();
            _rules.Add(new ValidationRule<T>
            {
                PropertyName = propertyName,
                Validate = obj =>
                {
                    var value = getter(obj);
                    if (value == null) return false;
                    var count = value.Count();
                    return count >= min && count <= max;
                },
                ErrorMessage = _currentErrorMessage ?? $"{propertyName}元素数量必须在{min}到{max}之间"
            });
            return this;
        }

        /// <summary>
        /// 构建验证器
        /// </summary>
        public IValidator<T> Build()
        {
            return new RuleBasedValidator<T>(_rules.ToList());
        }

        /// <summary>
        /// 验证对象
        /// </summary>
        public ValidationResult Validate(T instance)
        {
            return Build().Validate(instance);
        }

        private static string GetPropertyName<TProperty>(Expression<Func<T, TProperty>> expression)
        {
            return expression.Body switch
            {
                MemberExpression memberExpression => memberExpression.Member.Name,
                UnaryExpression { Operand: MemberExpression me } => me.Member.Name,
                _ => expression.ToString()
            };
        }

        private static bool IsValidIdCard(string idCard)
        {
            if (string.IsNullOrEmpty(idCard) || idCard.Length != 18)
                return false;

            // 基本格式检查
            if (!Regex.IsMatch(idCard, @"^\d{17}[\dXx]$"))
                return false;

            // 校验码验证
            var weights = new[] { 7, 9, 10, 5, 8, 4, 2, 1, 6, 3, 7, 9, 10, 5, 8, 4, 2 };
            var checkCodes = "10X98765432";
            var sum = 0;
            for (int i = 0; i < 17; i++)
            {
                sum += (idCard[i] - '0') * weights[i];
            }
            var checkCode = checkCodes[sum % 11];
            return char.ToUpper(idCard[17]) == checkCode;
        }
    }

    /// <summary>
    /// 验证规则
    /// </summary>
    /// <typeparam name="T">验证类型</typeparam>
    public class ValidationRule<T>
    {
        /// <summary>
        /// 属性名称
        /// </summary>
        public string PropertyName { get; set; } = string.Empty;

        /// <summary>
        /// 验证函数
        /// </summary>
        public Func<T, bool> Validate { get; set; } = _ => true;

        /// <summary>
        /// 错误消息
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;
    }

    /// <summary>
    /// 验证器接口
    /// </summary>
    /// <typeparam name="T">验证类型</typeparam>
    public interface IValidator<T>
    {
        /// <summary>
        /// 验证对象
        /// </summary>
        /// <param name="instance">要验证的对象</param>
        /// <returns>验证结果</returns>
        ValidationResult Validate(T instance);

        /// <summary>
        /// 异步验证对象
        /// </summary>
        /// <param name="instance">要验证的对象</param>
        /// <returns>验证结果</returns>
        Task<ValidationResult> ValidateAsync(T instance);
    }

    /// <summary>
    /// 基于规则的验证器
    /// </summary>
    internal class RuleBasedValidator<T> : IValidator<T>
    {
        private readonly List<ValidationRule<T>> _rules;

        public RuleBasedValidator(List<ValidationRule<T>> rules)
        {
            _rules = rules;
        }

        public ValidationResult Validate(T instance)
        {
            var errors = new List<string>();
            foreach (var rule in _rules)
            {
                try
                {
                    if (!rule.Validate(instance))
                    {
                        errors.Add(rule.ErrorMessage);
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"{rule.PropertyName}验证异常: {ex.Message}");
                }
            }
            return new ValidationResult(errors.Count == 0, errors);
        }

        public async Task<ValidationResult> ValidateAsync(T instance)
        {
            return await Task.Run(() => Validate(instance)).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// 验证规则构建器静态扩展
    /// </summary>
    public static class ValidationRuleBuilderExtensions
    {
        /// <summary>
        /// 创建验证规则构建器
        /// </summary>
        public static ValidationRuleBuilder<T> CreateValidator<T>()
        {
            return new ValidationRuleBuilder<T>();
        }
    }
}
