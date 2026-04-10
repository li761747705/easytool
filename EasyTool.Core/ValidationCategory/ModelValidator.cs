using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace EasyTool.ValidationCategory
{
    /// <summary>
    /// 模型验证器，基于 DataAnnotations 特性进行验证
    /// </summary>
    public static class ModelValidator
    {
        /// <summary>
        /// 验证模型
        /// </summary>
        /// <typeparam name="T">模型类型</typeparam>
        /// <param name="model">要验证的模型实例</param>
        /// <param name="validateAllProperties">是否验证所有属性</param>
        /// <returns>验证结果</returns>
        public static ValidationResult Validate<T>(T model, bool validateAllProperties = true)
        {
            if (model == null)
            {
                return ValidationResult.Failure("模型不能为空");
            }

            var context = new ValidationContext(model, null, null);
            var results = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
            var isValid = Validator.TryValidateObject(model, context, results, validateAllProperties);

            if (isValid)
            {
                return ValidationResult.Success();
            }

            var errors = results.Select(r => r.ErrorMessage ?? "验证失败").ToList();
            return new ValidationResult(false, errors);
        }

        /// <summary>
        /// 异步验证模型
        /// </summary>
        public static async Task<ValidationResult> ValidateAsync<T>(T model, bool validateAllProperties = true)
        {
            return await Task.Run(() => Validate(model, validateAllProperties)).ConfigureAwait(false);
        }

        /// <summary>
        /// 验证模型并抛出异常
        /// </summary>
        public static void ValidateAndThrow<T>(T model, bool validateAllProperties = true)
        {
            var result = Validate(model, validateAllProperties);
            if (!result.IsValid)
            {
                throw new ValidationException(result.Errors);
            }
        }

        /// <summary>
        /// 验证单个属性
        /// </summary>
        /// <typeparam name="T">模型类型</typeparam>
        /// <typeparam name="TProperty">属性类型</typeparam>
        /// <param name="model">模型实例</param>
        /// <param name="propertyName">属性名</param>
        /// <param name="value">属性值</param>
        /// <returns>验证结果</returns>
        public static ValidationResult ValidateProperty<T, TProperty>(T model, string propertyName, TProperty value)
        {
            if (model == null)
            {
                return ValidationResult.Failure("模型不能为空");
            }

            var context = new ValidationContext(model) { MemberName = propertyName };
            var results = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
            var isValid = Validator.TryValidateProperty(value, context, results);

            if (isValid)
            {
                return ValidationResult.Success();
            }

            var errors = results.Select(r => r.ErrorMessage ?? "验证失败").ToList();
            return new ValidationResult(false, errors);
        }

        /// <summary>
        /// 获取模型的所有验证属性
        /// </summary>
        public static IEnumerable<PropertyValidationInfo> GetValidationAttributes<T>()
        {
            var type = typeof(T);
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                var attributes = property.GetCustomAttributes<ValidationAttribute>();
                var displayAttribute = property.GetCustomAttribute<DisplayAttribute>();
                var info = new PropertyValidationInfo
                {
                    PropertyName = property.Name,
                    DisplayName = displayAttribute?.GetName() ?? property.Name,
                    ValidationAttributes = attributes.ToList()
                };
                yield return info;
            }
        }

        /// <summary>
        /// 尝试验证并获取验证错误信息字典
        /// </summary>
        public static Dictionary<string, List<string>> ValidateToDictionary<T>(T model, bool validateAllProperties = true)
        {
            var result = Validate(model, validateAllProperties);
            var dictionary = new Dictionary<string, List<string>>();

            if (result.IsValid)
            {
                return dictionary;
            }

            // 尝试按属性分组错误信息
            var context = new ValidationContext(model, null, null);
            var results = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
            Validator.TryValidateObject(model, context, results, validateAllProperties);

            foreach (var validationResult in results)
            {
                var propertyNames = validationResult.MemberNames.ToList();
                if (propertyNames.Count == 0)
                {
                    propertyNames.Add(string.Empty);
                }

                foreach (var propertyName in propertyNames)
                {
                    if (!dictionary.ContainsKey(propertyName))
                    {
                        dictionary[propertyName] = new List<string>();
                    }
                    dictionary[propertyName].Add(validationResult.ErrorMessage ?? "验证失败");
                }
            }

            return dictionary;
        }

        /// <summary>
        /// 验证字典数据
        /// </summary>
        public static ValidationResult ValidateDictionary(IDictionary<string, object?> data, IEnumerable<PropertyValidationRule> rules)
        {
            var errors = new List<string>();
            var rulesDict = rules.ToDictionary(r => r.PropertyName, r => r);

            foreach (var rule in rulesDict.Values)
            {
                if (!data.TryGetValue(rule.PropertyName, out var value))
                {
                    if (rule.IsRequired)
                    {
                        errors.Add(rule.RequiredErrorMessage ?? $"{rule.PropertyName}是必填项");
                    }
                    continue;
                }

                foreach (var validator in rule.Validators)
                {
                    if (!validator(value))
                    {
                        errors.Add(rule.ErrorMessage ?? $"{rule.PropertyName}验证失败");
                    }
                }
            }

            return errors.Count == 0 ? ValidationResult.Success() : new ValidationResult(false, errors);
        }

        /// <summary>
        /// 验证对象字典
        /// </summary>
        public static ValidationResult ValidateObjectDictionary(IDictionary<string, object?> data, Type modelType)
        {
            var errors = new List<string>();
            var properties = modelType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                var validationAttributes = property.GetCustomAttributes<ValidationAttribute>();
                var displayAttribute = property.GetCustomAttribute<DisplayAttribute>();
                var displayName = displayAttribute?.GetName() ?? property.Name;

                if (!data.TryGetValue(property.Name, out var value))
                {
                    var requiredAttr = validationAttributes.FirstOrDefault(a => a is RequiredAttribute);
                    if (requiredAttr != null)
                    {
                        errors.Add(requiredAttr.ErrorMessage ?? $"{displayName}是必填项");
                    }
                    continue;
                }

                foreach (var attr in validationAttributes)
                {
                    try
                    {
                        // 类型转换
                        var convertedValue = value == null ? null : Convert.ChangeType(value, property.PropertyType);
                        if (!attr.IsValid(convertedValue))
                        {
                            errors.Add(attr.ErrorMessage ?? $"{displayName}验证失败");
                        }
                    }
                    catch (Exception)
                    {
                        errors.Add($"{displayName}类型转换失败");
                    }
                }
            }

            return errors.Count == 0 ? ValidationResult.Success() : new ValidationResult(false, errors);
        }
    }

    /// <summary>
    /// 属性验证信息
    /// </summary>
    public class PropertyValidationInfo
    {
        /// <summary>
        /// 属性名称
        /// </summary>
        public string PropertyName { get; set; } = string.Empty;

        /// <summary>
        /// 显示名称
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// 验证特性列表
        /// </summary>
        public IReadOnlyList<ValidationAttribute> ValidationAttributes { get; set; } = new List<ValidationAttribute>();
    }

    /// <summary>
    /// 属性验证规则
    /// </summary>
    public class PropertyValidationRule
    {
        /// <summary>
        /// 属性名称
        /// </summary>
        public string PropertyName { get; set; } = string.Empty;

        /// <summary>
        /// 是否必填
        /// </summary>
        public bool IsRequired { get; set; }

        /// <summary>
        /// 必填错误消息
        /// </summary>
        public string? RequiredErrorMessage { get; set; }

        /// <summary>
        /// 验证器列表
        /// </summary>
        public List<Func<object?, bool>> Validators { get; set; } = new();

        /// <summary>
        /// 验证失败错误消息
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 创建属性验证规则
        /// </summary>
        public static PropertyValidationRule Create(string propertyName)
        {
            return new PropertyValidationRule { PropertyName = propertyName };
        }

        /// <summary>
        /// 设置为必填
        /// </summary>
        public PropertyValidationRule Required(string? errorMessage = null)
        {
            IsRequired = true;
            RequiredErrorMessage = errorMessage;
            return this;
        }

        /// <summary>
        /// 添加验证器
        /// </summary>
        public PropertyValidationRule AddValidator(Func<object?, bool> validator, string? errorMessage = null)
        {
            Validators.Add(validator);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                ErrorMessage = errorMessage;
            }
            return this;
        }

        /// <summary>
        /// 添加正则验证
        /// </summary>
        public PropertyValidationRule Regex(string pattern, string? errorMessage = null)
        {
            return AddValidator(value =>
            {
                if (value == null) return true;
                return System.Text.RegularExpressions.Regex.IsMatch(value.ToString() ?? "", pattern);
            }, errorMessage);
        }

        /// <summary>
        /// 添加长度验证
        /// </summary>
        public PropertyValidationRule Length(int min, int max, string? errorMessage = null)
        {
            return AddValidator(value =>
            {
                if (value == null) return true;
                var str = value.ToString() ?? "";
                return str.Length >= min && str.Length <= max;
            }, errorMessage);
        }

        /// <summary>
        /// 添加范围验证
        /// </summary>
        public PropertyValidationRule Range(IComparable min, IComparable max, string? errorMessage = null)
        {
            return AddValidator(value =>
            {
                if (value == null) return true;
                if (value is IComparable comparable)
                {
                    return comparable.CompareTo(min) >= 0 && comparable.CompareTo(max) <= 0;
                }
                return true;
            }, errorMessage);
        }
    }
}
