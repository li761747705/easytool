using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyTool.ValidationCategory
{
    /// <summary>
    /// 组合验证器，支持多个验证器的组合使用
    /// </summary>
    public class CompositeValidator<T>
    {
        private readonly List<IValidator<T>> _validators = new();
        private readonly List<Func<T, ValidationResult>> _validationFuncs = new();
        private bool _stopOnFirstFailure;

        /// <summary>
        /// 添加验证器
        /// </summary>
        public CompositeValidator<T> Add(IValidator<T> validator)
        {
            _validators.Add(validator);
            return this;
        }

        /// <summary>
        /// 添加验证函数
        /// </summary>
        public CompositeValidator<T> Add(Func<T, ValidationResult> validationFunc)
        {
            _validationFuncs.Add(validationFunc);
            return this;
        }

        /// <summary>
        /// 添加条件验证
        /// </summary>
        public CompositeValidator<T> AddWhen(Func<T, bool> condition, IValidator<T> validator)
        {
            _validationFuncs.Add(obj =>
            {
                if (condition(obj))
                {
                    return validator.Validate(obj);
                }
                return ValidationResult.Success();
            });
            return this;
        }

        /// <summary>
        /// 添加条件验证函数
        /// </summary>
        public CompositeValidator<T> AddWhen(Func<T, bool> condition, Func<T, ValidationResult> validationFunc)
        {
            _validationFuncs.Add(obj =>
            {
                if (condition(obj))
                {
                    return validationFunc(obj);
                }
                return ValidationResult.Success();
            });
            return this;
        }

        /// <summary>
        /// 设置遇到第一个错误就停止
        /// </summary>
        public CompositeValidator<T> StopOnFirstFailure()
        {
            _stopOnFirstFailure = true;
            return this;
        }

        /// <summary>
        /// 验证对象
        /// </summary>
        public ValidationResult Validate(T instance)
        {
            var allErrors = new List<string>();

            foreach (var validator in _validators)
            {
                var result = validator.Validate(instance);
                if (!result.IsValid)
                {
                    allErrors.AddRange(result.Errors);
                    if (_stopOnFirstFailure)
                    {
                        return new ValidationResult(false, allErrors);
                    }
                }
            }

            foreach (var validationFunc in _validationFuncs)
            {
                var result = validationFunc(instance);
                if (!result.IsValid)
                {
                    allErrors.AddRange(result.Errors);
                    if (_stopOnFirstFailure)
                    {
                        return new ValidationResult(false, allErrors);
                    }
                }
            }

            return allErrors.Count == 0
                ? ValidationResult.Success()
                : new ValidationResult(false, allErrors);
        }

        /// <summary>
        /// 异步验证对象
        /// </summary>
        public async Task<ValidationResult> ValidateAsync(T instance)
        {
            var allErrors = new List<string>();

            foreach (var validator in _validators)
            {
                var result = await validator.ValidateAsync(instance).ConfigureAwait(false);
                if (!result.IsValid)
                {
                    allErrors.AddRange(result.Errors);
                    if (_stopOnFirstFailure)
                    {
                        return new ValidationResult(false, allErrors);
                    }
                }
            }

            foreach (var validationFunc in _validationFuncs)
            {
                var result = validationFunc(instance);
                if (!result.IsValid)
                {
                    allErrors.AddRange(result.Errors);
                    if (_stopOnFirstFailure)
                    {
                        return new ValidationResult(false, allErrors);
                    }
                }
            }

            return allErrors.Count == 0
                ? ValidationResult.Success()
                : new ValidationResult(false, allErrors);
        }
    }

    /// <summary>
    /// 批量验证器，支持批量验证多个对象
    /// </summary>
    public class BatchValidator
    {
        private readonly Dictionary<string, Func<object?, ValidationResult>> _propertyValidators = new();
        private bool _stopOnFirstFailure;

        /// <summary>
        /// 添加属性验证器
        /// </summary>
        public BatchValidator Add(string propertyName, Func<object?, ValidationResult> validator)
        {
            _propertyValidators[propertyName] = validator;
            return this;
        }

        /// <summary>
        /// 添加属性验证器（使用 FluentValidator）
        /// </summary>
        public BatchValidator Add<TProperty>(string propertyName, TProperty value, Action<FluentValidator<TProperty>> configure)
        {
            var validator = FluentValidator<TProperty>.For(value, propertyName);
            configure(validator);
            _propertyValidators[propertyName] = _ =>
            {
                var result = validator.GetResult();
                return result;
            };
            return this;
        }

        /// <summary>
        /// 设置遇到第一个错误就停止
        /// </summary>
        public BatchValidator StopOnFirstFailure()
        {
            _stopOnFirstFailure = true;
            return this;
        }

        /// <summary>
        /// 验证所有属性
        /// </summary>
        public BatchValidationResult Validate()
        {
            var propertyResults = new Dictionary<string, ValidationResult>();
            var allErrors = new List<string>();

            foreach (var kvp in _propertyValidators)
            {
                var result = kvp.Value(null);
                propertyResults[kvp.Key] = result;

                if (!result.IsValid)
                {
                    allErrors.AddRange(result.Errors.Select(e => $"[{kvp.Key}] {e}"));
                    if (_stopOnFirstFailure)
                    {
                        break;
                    }
                }
            }

            return new BatchValidationResult(allErrors.Count == 0, allErrors, propertyResults);
        }
    }

    /// <summary>
    /// 批量验证结果
    /// </summary>
    public class BatchValidationResult
    {
        /// <summary>
        /// 是否全部验证通过
        /// </summary>
        public bool IsValid { get; }

        /// <summary>
        /// 所有错误消息
        /// </summary>
        public IReadOnlyList<string> AllErrors { get; }

        /// <summary>
        /// 按属性分组的验证结果
        /// </summary>
        public IReadOnlyDictionary<string, ValidationResult> PropertyResults { get; }

        /// <summary>
        /// 第一个错误消息
        /// </summary>
        public string? FirstError => AllErrors.FirstOrDefault();

        /// <summary>
        /// 创建批量验证结果
        /// </summary>
        /// <param name="isValid">是否全部验证通过</param>
        /// <param name="allErrors">所有错误消息</param>
        /// <param name="propertyResults">按属性分组的验证结果</param>
        public BatchValidationResult(bool isValid, List<string> allErrors, Dictionary<string, ValidationResult> propertyResults)
        {
            IsValid = isValid;
            AllErrors = allErrors.AsReadOnly();
            PropertyResults = propertyResults;
        }

        /// <summary>
        /// 获取指定属性的验证结果
        /// </summary>
        public ValidationResult? GetPropertyResult(string propertyName)
        {
            return PropertyResults.TryGetValue(propertyName, out var result) ? result : null;
        }

        /// <summary>
        /// 获取指定属性的错误消息
        /// </summary>
        public IReadOnlyList<string> GetPropertyErrors(string propertyName)
        {
            return GetPropertyResult(propertyName)?.Errors ?? new List<string>().AsReadOnly();
        }

        /// <summary>
        /// 获取失败的属性名列表
        /// </summary>
        public IEnumerable<string> GetFailedProperties()
        {
            return PropertyResults.Where(kvp => !kvp.Value.IsValid).Select(kvp => kvp.Key);
        }
    }

    /// <summary>
    /// 验证器集合，用于管理多个类型的验证器
    /// </summary>
    public class ValidatorCollection
    {
        private readonly Dictionary<Type, object> _validators = new();

        /// <summary>
        /// 注册验证器
        /// </summary>
        public ValidatorCollection Register<T>(IValidator<T> validator)
        {
            _validators[typeof(T)] = validator;
            return this;
        }

        /// <summary>
        /// 注册验证器构建器
        /// </summary>
        public ValidatorCollection Register<T>(Action<ValidationRuleBuilder<T>> configure)
        {
            var builder = new ValidationRuleBuilder<T>();
            configure(builder);
            _validators[typeof(T)] = builder.Build();
            return this;
        }

        /// <summary>
        /// 获取验证器
        /// </summary>
        public IValidator<T>? Get<T>()
        {
            return _validators.TryGetValue(typeof(T), out var validator) ? validator as IValidator<T> : null;
        }

        /// <summary>
        /// 验证对象
        /// </summary>
        public ValidationResult Validate<T>(T instance)
        {
            var validator = Get<T>();
            if (validator == null)
            {
                // 如果没有注册验证器，尝试使用 ModelValidator
                return ModelValidator.Validate(instance);
            }
            return validator.Validate(instance);
        }

        /// <summary>
        /// 异步验证对象
        /// </summary>
        public async Task<ValidationResult> ValidateAsync<T>(T instance)
        {
            var validator = Get<T>();
            if (validator == null)
            {
                return await ModelValidator.ValidateAsync(instance).ConfigureAwait(false);
            }
            return await validator.ValidateAsync(instance).ConfigureAwait(false);
        }

        /// <summary>
        /// 验证并抛出异常
        /// </summary>
        public void ValidateAndThrow<T>(T instance)
        {
            var result = Validate(instance);
            if (!result.IsValid)
            {
                throw new ValidationException(result.Errors);
            }
        }

        /// <summary>
        /// 检查是否已注册验证器
        /// </summary>
        public bool IsRegistered<T>()
        {
            return _validators.ContainsKey(typeof(T));
        }

        /// <summary>
        /// 移除验证器
        /// </summary>
        public bool Remove<T>()
        {
            return _validators.Remove(typeof(T));
        }

        /// <summary>
        /// 清空所有验证器
        /// </summary>
        public void Clear()
        {
            _validators.Clear();
        }
    }

    /// <summary>
    /// 验证器扩展方法
    /// </summary>
    public static class CompositeValidatorExtensions
    {
        /// <summary>
        /// 创建组合验证器
        /// </summary>
        public static CompositeValidator<T> CreateCompositeValidator<T>()
        {
            return new CompositeValidator<T>();
        }

        /// <summary>
        /// 创建批量验证器
        /// </summary>
        public static BatchValidator CreateBatchValidator()
        {
            return new BatchValidator();
        }

        /// <summary>
        /// 创建验证器集合
        /// </summary>
        public static ValidatorCollection CreateValidatorCollection()
        {
            return new ValidatorCollection();
        }
    }
}
