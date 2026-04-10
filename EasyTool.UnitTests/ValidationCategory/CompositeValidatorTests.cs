using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using EasyTool.ValidationCategory;

namespace EasyTool.ValidationCategory.Tests
{
    // ==================== Test Validator Implementation ====================

    public class TestValidator<T> : IValidator<T>
    {
        private readonly Func<T, bool> _validateFunc;
        private readonly string _errorMessage;

        public TestValidator(Func<T, bool> validateFunc, string errorMessage = "Validation failed")
        {
            _validateFunc = validateFunc;
            _errorMessage = errorMessage;
        }

        public int ValidateCallCount { get; private set; }

        public ValidationResult Validate(T instance)
        {
            ValidateCallCount++;
            return _validateFunc(instance)
                ? ValidationResult.Success()
                : ValidationResult.Failure(_errorMessage);
        }

        public Task<ValidationResult> ValidateAsync(T instance)
        {
            ValidateCallCount++;
            var result = _validateFunc(instance)
                ? ValidationResult.Success()
                : ValidationResult.Failure(_errorMessage);
            return Task.FromResult(result);
        }
    }

    // ==================== Test Models ====================

    public class CompositeTestModel
    {
        public string Name { get; set; } = "";
        public int Age { get; set; }
        public string Email { get; set; } = "";
    }

    // ==================== CompositeValidator<T> Tests ====================

    public class CompositeValidatorTests
    {
        // ==================== Add(IValidator<T>) ====================

        [Fact]
        public void Add_Validator_CanBeAdded()
        {
            var validator = new CompositeValidator<CompositeTestModel>();
            var testValidator = new TestValidator<CompositeTestModel>(m => true);
            var result = validator.Add(testValidator);
            Assert.Same(validator, result); // fluent API
        }

        // ==================== Add(Func<T, ValidationResult>) ====================

        [Fact]
        public void Add_ValidationFunc_CanBeAdded()
        {
            var validator = new CompositeValidator<CompositeTestModel>();
            var result = validator.Add(m => ValidationResult.Success());
            Assert.Same(validator, result);
        }

        // ==================== Validate - All pass ====================

        [Fact]
        public void Validate_AllValidatorsPass_ReturnsSuccess()
        {
            var validator = new CompositeValidator<CompositeTestModel>()
                .Add(m => !string.IsNullOrEmpty(m.Name) ? ValidationResult.Success() : ValidationResult.Failure("Name required"))
                .Add(m => m.Age > 0 ? ValidationResult.Success() : ValidationResult.Failure("Age must be positive"));

            var model = new CompositeTestModel { Name = "John", Age = 25 };
            var result = validator.Validate(model);

            Assert.True(result.IsValid);
        }

        // ==================== Validate - Some fail ====================

        [Fact]
        public void Validate_SomeValidatorsFail_ReturnsAllErrors()
        {
            var validator = new CompositeValidator<CompositeTestModel>()
                .Add(m => !string.IsNullOrEmpty(m.Name) ? ValidationResult.Success() : ValidationResult.Failure("Name required"))
                .Add(m => m.Age > 0 ? ValidationResult.Success() : ValidationResult.Failure("Age must be positive"))
                .Add(m => m.Email.Contains("@") ? ValidationResult.Success() : ValidationResult.Failure("Email invalid"));

            var model = new CompositeTestModel { Name = "", Age = 0, Email = "bad" };
            var result = validator.Validate(model);

            Assert.False(result.IsValid);
            Assert.Equal(3, result.Errors.Count);
            Assert.Contains("Name required", result.Errors);
            Assert.Contains("Age must be positive", result.Errors);
            Assert.Contains("Email invalid", result.Errors);
        }

        // ==================== Validate - With IValidator<T> ====================

        [Fact]
        public void Validate_WithIValidator_PassesThrough()
        {
            var testValidator = new TestValidator<CompositeTestModel>(m => !string.IsNullOrEmpty(m.Name), "Name required");
            var validator = new CompositeValidator<CompositeTestModel>()
                .Add(testValidator);

            var validModel = new CompositeTestModel { Name = "John" };
            var invalidModel = new CompositeTestModel { Name = "" };

            Assert.True(validator.Validate(validModel).IsValid);
            Assert.False(validator.Validate(invalidModel).IsValid);
            Assert.Contains("Name required", validator.Validate(invalidModel).Errors);
        }

        // ==================== AddWhen - Condition met ====================

        [Fact]
        public void AddWhen_ConditionTrue_Validates()
        {
            var validator = new CompositeValidator<CompositeTestModel>()
                .AddWhen(
                    m => m.Age >= 18,
                    new TestValidator<CompositeTestModel>(m => !string.IsNullOrEmpty(m.Email), "Email required for adults"));

            var adultWithoutEmail = new CompositeTestModel { Age = 25, Email = "" };
            var result = validator.Validate(adultWithoutEmail);
            Assert.False(result.IsValid);
            Assert.Contains("Email required for adults", result.Errors);
        }

        // ==================== AddWhen - Condition not met ====================

        [Fact]
        public void AddWhen_ConditionFalse_SkipsValidation()
        {
            var validator = new CompositeValidator<CompositeTestModel>()
                .AddWhen(
                    m => m.Age >= 18,
                    new TestValidator<CompositeTestModel>(m => false, "Should not see this"));

            var child = new CompositeTestModel { Age = 10 };
            var result = validator.Validate(child);
            Assert.True(result.IsValid);
        }

        // ==================== AddWhen - Func overload ====================

        [Fact]
        public void AddWhen_FuncOverload_ConditionTrue_Validates()
        {
            var validator = new CompositeValidator<CompositeTestModel>()
                .AddWhen(
                    m => m.Age >= 18,
                    m => string.IsNullOrEmpty(m.Email)
                        ? ValidationResult.Failure("Email required")
                        : ValidationResult.Success());

            var adult = new CompositeTestModel { Age = 20, Email = "" };
            var result = validator.Validate(adult);
            Assert.False(result.IsValid);
            Assert.Contains("Email required", result.Errors);
        }

        [Fact]
        public void AddWhen_FuncOverload_ConditionFalse_SkipsValidation()
        {
            var validator = new CompositeValidator<CompositeTestModel>()
                .AddWhen(
                    m => m.Age >= 18,
                    m => ValidationResult.Failure("Should not see this"));

            var child = new CompositeTestModel { Age = 10 };
            var result = validator.Validate(child);
            Assert.True(result.IsValid);
        }

        // ==================== StopOnFirstFailure ====================

        [Fact]
        public void StopOnFirstFailure_StopsAfterFirstError()
        {
            var callCount = 0;
            var validator = new CompositeValidator<CompositeTestModel>()
                .StopOnFirstFailure()
                .Add(m =>
                {
                    callCount++;
                    return ValidationResult.Failure("Error 1");
                })
                .Add(m =>
                {
                    callCount++;
                    return ValidationResult.Failure("Error 2");
                });

            var model = new CompositeTestModel();
            var result = validator.Validate(model);

            Assert.False(result.IsValid);
            Assert.Single(result.Errors);
            Assert.Contains("Error 1", result.Errors);
            Assert.Equal(1, callCount);
        }

        [Fact]
        public void StopOnFirstFailure_NoErrors_Continues()
        {
            var callCount = 0;
            var validator = new CompositeValidator<CompositeTestModel>()
                .StopOnFirstFailure()
                .Add(m =>
                {
                    callCount++;
                    return ValidationResult.Success();
                })
                .Add(m =>
                {
                    callCount++;
                    return ValidationResult.Success();
                });

            var model = new CompositeTestModel();
            var result = validator.Validate(model);

            Assert.True(result.IsValid);
            Assert.Equal(2, callCount);
        }

        // ==================== ValidateAsync ====================

        [Fact]
        public async Task ValidateAsync_AllPass_ReturnsSuccess()
        {
            var validator = new CompositeValidator<CompositeTestModel>()
                .Add(m => ValidationResult.Success())
                .Add(new TestValidator<CompositeTestModel>(m => true));

            var model = new CompositeTestModel { Name = "Test" };
            var result = await validator.ValidateAsync(model);
            Assert.True(result.IsValid);
        }

        [Fact]
        public async Task ValidateAsync_SomeFail_ReturnsErrors()
        {
            var validator = new CompositeValidator<CompositeTestModel>()
                .Add(m => ValidationResult.Failure("Async error 1"))
                .Add(m => ValidationResult.Failure("Async error 2"));

            var model = new CompositeTestModel();
            var result = await validator.ValidateAsync(model);

            Assert.False(result.IsValid);
            Assert.Equal(2, result.Errors.Count);
        }

        [Fact]
        public async Task ValidateAsync_StopOnFirstFailure_StopsEarly()
        {
            var validator = new CompositeValidator<CompositeTestModel>()
                .StopOnFirstFailure()
                .Add(m => ValidationResult.Failure("First async error"))
                .Add(m => ValidationResult.Failure("Should not reach"));

            var model = new CompositeTestModel();
            var result = await validator.ValidateAsync(model);

            Assert.False(result.IsValid);
            Assert.Single(result.Errors);
        }

        // ==================== Empty validator ====================

        [Fact]
        public void Validate_NoValidators_ReturnsSuccess()
        {
            var validator = new CompositeValidator<CompositeTestModel>();
            var result = validator.Validate(new CompositeTestModel());
            Assert.True(result.IsValid);
        }

        // ==================== BatchValidator Tests ====================

        [Fact]
        public void BatchValidator_AllPass_ReturnsValidResult()
        {
            var batch = new BatchValidator()
                .Add("Name", v =>
                {
                    // BatchValidator passes null to the validator; we must handle it
                    return ValidationResult.Success();
                })
                .Add("Age", v => ValidationResult.Success());

            var result = batch.Validate();
            Assert.True(result.IsValid);
            Assert.Empty(result.AllErrors);
        }

        [Fact]
        public void BatchValidator_SomeFail_ReturnsErrors()
        {
            var batch = new BatchValidator()
                .Add("Name", _ => ValidationResult.Failure("Name is empty"))
                .Add("Age", _ => ValidationResult.Success());

            var result = batch.Validate();
            Assert.False(result.IsValid);
            Assert.Contains("[Name] Name is empty", result.AllErrors);
        }

        [Fact]
        public void BatchValidator_StopOnFirstFailure_StopsAfterFirst()
        {
            var batch = new BatchValidator()
                .StopOnFirstFailure()
                .Add("Name", _ => ValidationResult.Failure("Name error"))
                .Add("Age", _ => ValidationResult.Failure("Age error"));

            var result = batch.Validate();
            Assert.False(result.IsValid);
            Assert.Single(result.AllErrors);
        }

        [Fact]
        public void BatchValidator_GetPropertyResult_ReturnsResult()
        {
            var batch = new BatchValidator()
                .Add("Name", _ => ValidationResult.Failure("Name error"))
                .Add("Age", _ => ValidationResult.Success());

            var result = batch.Validate();
            var nameResult = result.GetPropertyResult("Name");
            Assert.NotNull(nameResult);
            Assert.False(nameResult!.IsValid);
        }

        [Fact]
        public void BatchValidator_GetPropertyResult_UnknownProperty_ReturnsNull()
        {
            var batch = new BatchValidator()
                .Add("Name", _ => ValidationResult.Success());

            var result = batch.Validate();
            Assert.Null(result.GetPropertyResult("Unknown"));
        }

        [Fact]
        public void BatchValidator_GetPropertyErrors_ReturnsErrors()
        {
            var batch = new BatchValidator()
                .Add("Name", _ => ValidationResult.Failure("Error A").IsValid ? ValidationResult.Success() : new ValidationResult(false, new List<string> { "Error A", "Error B" }));

            var result = batch.Validate();
            var errors = result.GetPropertyErrors("Name");
            Assert.NotEmpty(errors);
        }

        [Fact]
        public void BatchValidator_GetFailedProperties_ReturnsFailedNames()
        {
            var batch = new BatchValidator()
                .Add("Name", _ => ValidationResult.Failure("Name error"))
                .Add("Age", _ => ValidationResult.Success())
                .Add("Email", _ => ValidationResult.Failure("Email error"));

            var result = batch.Validate();
            var failed = result.GetFailedProperties().ToList();
            Assert.Equal(2, failed.Count);
            Assert.Contains("Name", failed);
            Assert.Contains("Email", failed);
        }

        [Fact]
        public void BatchValidator_FirstError_ReturnsFirstErrorMessage()
        {
            var batch = new BatchValidator()
                .Add("Name", _ => ValidationResult.Failure("First error"))
                .Add("Age", _ => ValidationResult.Failure("Second error"));

            var result = batch.Validate();
            Assert.Equal("[Name] First error", result.FirstError);
        }

        [Fact]
        public void BatchValidator_AllPass_FirstErrorIsNull()
        {
            var batch = new BatchValidator()
                .Add("Name", _ => ValidationResult.Success());

            var result = batch.Validate();
            Assert.Null(result.FirstError);
        }

        // ==================== ValidatorCollection Tests ====================

        [Fact]
        public void ValidatorCollection_Register_AndGet()
        {
            var collection = new ValidatorCollection();
            var validator = new TestValidator<CompositeTestModel>(m => true);
            collection.Register(validator);

            var retrieved = collection.Get<CompositeTestModel>();
            Assert.NotNull(retrieved);
            Assert.Same(validator, retrieved);
        }

        [Fact]
        public void ValidatorCollection_Get_Unregistered_ReturnsNull()
        {
            var collection = new ValidatorCollection();
            Assert.Null(collection.Get<CompositeTestModel>());
        }

        [Fact]
        public void ValidatorCollection_Validate_WithRegisteredValidator()
        {
            var collection = new ValidatorCollection();
            collection.Register(new TestValidator<CompositeTestModel>(m => !string.IsNullOrEmpty(m.Name), "Name required"));

            var validModel = new CompositeTestModel { Name = "John" };
            var invalidModel = new CompositeTestModel { Name = "" };

            Assert.True(collection.Validate(validModel).IsValid);
            Assert.False(collection.Validate(invalidModel).IsValid);
        }

        [Fact]
        public void ValidatorCollection_Validate_WithoutRegisteredValidator_FallsBackToModelValidator()
        {
            var collection = new ValidatorCollection();
            // No validator registered for CompositeTestModel, falls back to ModelValidator
            var model = new CompositeTestModel { Name = "John" };
            var result = collection.Validate(model);
            // CompositeTestModel has no DataAnnotations, so ModelValidator should succeed
            Assert.True(result.IsValid);
        }

        [Fact]
        public void ValidatorCollection_Register_WithBuilder()
        {
            var collection = new ValidatorCollection();
            collection.Register<CompositeTestModel>(builder =>
                builder.NotNull(m => m.Name).WithMessage("Name cannot be null"));

            Assert.True(collection.IsRegistered<CompositeTestModel>());
            var result = collection.Validate(new CompositeTestModel { Name = null! });
            Assert.False(result.IsValid);
        }

        [Fact]
        public void ValidatorCollection_ValidateAndThrow_ValidModel_DoesNotThrow()
        {
            var collection = new ValidatorCollection();
            collection.Register(new TestValidator<CompositeTestModel>(m => true));

            collection.ValidateAndThrow(new CompositeTestModel());
        }

        [Fact]
        public void ValidatorCollection_ValidateAndThrow_InvalidModel_Throws()
        {
            var collection = new ValidatorCollection();
            collection.Register(new TestValidator<CompositeTestModel>(m => false, "Always fails"));

            Assert.Throws<ValidationException>(() =>
                collection.ValidateAndThrow(new CompositeTestModel()));
        }

        [Fact]
        public async Task ValidatorCollection_ValidateAsync_WithRegisteredValidator()
        {
            var collection = new ValidatorCollection();
            collection.Register(new TestValidator<CompositeTestModel>(m => !string.IsNullOrEmpty(m.Name), "Name required"));

            var model = new CompositeTestModel { Name = "John" };
            var result = await collection.ValidateAsync(model);
            Assert.True(result.IsValid);
        }

        [Fact]
        public void ValidatorCollection_IsRegistered_ReturnsCorrectStatus()
        {
            var collection = new ValidatorCollection();
            Assert.False(collection.IsRegistered<CompositeTestModel>());

            collection.Register(new TestValidator<CompositeTestModel>(m => true));
            Assert.True(collection.IsRegistered<CompositeTestModel>());
        }

        [Fact]
        public void ValidatorCollection_Remove_ReturnsTrueWhenRemoved()
        {
            var collection = new ValidatorCollection();
            collection.Register(new TestValidator<CompositeTestModel>(m => true));
            Assert.True(collection.Remove<CompositeTestModel>());
            Assert.False(collection.IsRegistered<CompositeTestModel>());
        }

        [Fact]
        public void ValidatorCollection_Remove_NotRegistered_ReturnsFalse()
        {
            var collection = new ValidatorCollection();
            Assert.False(collection.Remove<CompositeTestModel>());
        }

        [Fact]
        public void ValidatorCollection_Clear_RemovesAllValidators()
        {
            var collection = new ValidatorCollection();
            collection.Register(new TestValidator<CompositeTestModel>(m => true));
            collection.Register(new TestValidator<string>(s => true));
            collection.Clear();

            Assert.False(collection.IsRegistered<CompositeTestModel>());
            Assert.False(collection.IsRegistered<string>());
        }

        // ==================== CompositeValidatorExtensions Tests ====================

        [Fact]
        public void CreateCompositeValidator_ReturnsNewInstance()
        {
            var validator = CompositeValidatorExtensions.CreateCompositeValidator<CompositeTestModel>();
            Assert.NotNull(validator);
        }

        [Fact]
        public void CreateBatchValidator_ReturnsNewInstance()
        {
            var validator = CompositeValidatorExtensions.CreateBatchValidator();
            Assert.NotNull(validator);
        }

        [Fact]
        public void CreateValidatorCollection_ReturnsNewInstance()
        {
            var collection = CompositeValidatorExtensions.CreateValidatorCollection();
            Assert.NotNull(collection);
        }

        // ==================== Mixed validators and funcs ====================

        [Fact]
        public void Validate_MixedValidatorsAndFuncs_AllErrorsCollected()
        {
            var testValidator = new TestValidator<CompositeTestModel>(m => false, "IValidator error");
            var validator = new CompositeValidator<CompositeTestModel>()
                .Add(testValidator)
                .Add(m => ValidationResult.Failure("Func error"));

            var result = validator.Validate(new CompositeTestModel());
            Assert.False(result.IsValid);
            Assert.Equal(2, result.Errors.Count);
        }
    }
}
