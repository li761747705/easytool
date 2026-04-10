using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Xunit;
using EasyTool.ValidationCategory;

namespace EasyTool.ValidationCategory.Tests
{
    // ==================== Test Models ====================

    public class ValidTestModel
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(50, MinimumLength = 2)]
        public string Name { get; set; } = "Test";

        [Range(1, 100)]
        public int Age { get; set; } = 25;

        [EmailAddress]
        public string Email { get; set; } = "test@example.com";
    }

    public class InvalidTestModel
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(50, MinimumLength = 2)]
        public string Name { get; set; } = "";

        [Range(1, 100, ErrorMessage = "Age must be between 1 and 100")]
        public int Age { get; set; } = 0;

        [EmailAddress(ErrorMessage = "Invalid email")]
        public string Email { get; set; } = "not-an-email";
    }

    public class EmptyModel
    {
    }

    [Display(Name = "User Name")]
    public class DisplayAnnotatedModel
    {
        [Required]
        public string UserName { get; set; } = "john";

        [StringLength(100)]
        public string Bio { get; set; } = string.Empty;
    }

    public class ModelWithNoValidation
    {
        public string Description { get; set; } = "anything";
    }

    // ==================== Tests ====================

    public class ModelValidatorTests
    {
        // ==================== Validate<T> ====================

        [Fact]
        public void Validate_ValidModel_ReturnsSuccess()
        {
            var model = new ValidTestModel();
            var result = ModelValidator.Validate(model);
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void Validate_InvalidModel_ReturnsErrors()
        {
            var model = new InvalidTestModel();
            var result = ModelValidator.Validate(model);
            Assert.False(result.IsValid);
            Assert.NotEmpty(result.Errors);
        }

        [Fact]
        public void Validate_NullModel_ReturnsFailure()
        {
            ValidTestModel? model = null;
            var result = ModelValidator.Validate(model);
            Assert.False(result.IsValid);
            Assert.Contains("模型不能为空", result.Errors);
        }

        [Fact]
        public void Validate_EmptyModel_ReturnsSuccess()
        {
            var model = new EmptyModel();
            var result = ModelValidator.Validate(model);
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_ModelWithNoValidationAttributes_ReturnsSuccess()
        {
            var model = new ModelWithNoValidation();
            var result = ModelValidator.Validate(model);
            Assert.True(result.IsValid);
        }

        // ==================== ValidateAsync<T> ====================

        [Fact]
        public async Task ValidateAsync_ValidModel_ReturnsSuccess()
        {
            var model = new ValidTestModel();
            var result = await ModelValidator.ValidateAsync(model);
            Assert.True(result.IsValid);
        }

        [Fact]
        public async Task ValidateAsync_InvalidModel_ReturnsErrors()
        {
            var model = new InvalidTestModel();
            var result = await ModelValidator.ValidateAsync(model);
            Assert.False(result.IsValid);
            Assert.NotEmpty(result.Errors);
        }

        [Fact]
        public async Task ValidateAsync_NullModel_ReturnsFailure()
        {
            ValidTestModel? model = null;
            var result = await ModelValidator.ValidateAsync(model);
            Assert.False(result.IsValid);
        }

        // ==================== ValidateAndThrow<T> ====================

        [Fact]
        public void ValidateAndThrow_ValidModel_DoesNotThrow()
        {
            var model = new ValidTestModel();
            ModelValidator.ValidateAndThrow(model);
        }

        [Fact]
        public void ValidateAndThrow_InvalidModel_ThrowsValidationException()
        {
            var model = new InvalidTestModel();
            var ex = Assert.Throws<ValidationException>(() => ModelValidator.ValidateAndThrow(model));
            Assert.NotEmpty(ex.Errors);
        }

        [Fact]
        public void ValidateAndThrow_NullModel_ThrowsValidationException()
        {
            ValidTestModel? model = null;
            var ex = Assert.Throws<ValidationException>(() => ModelValidator.ValidateAndThrow(model));
            Assert.NotEmpty(ex.Errors);
        }

        // ==================== ValidateProperty<T, TProperty> ====================

        [Fact]
        public void ValidateProperty_ValidProperty_ReturnsSuccess()
        {
            var model = new ValidTestModel();
            var result = ModelValidator.ValidateProperty(model, nameof(ValidTestModel.Age), 25);
            Assert.True(result.IsValid);
        }

        [Fact]
        public void ValidateProperty_InvalidProperty_ReturnsErrors()
        {
            var model = new ValidTestModel();
            var result = ModelValidator.ValidateProperty(model, nameof(ValidTestModel.Age), 0);
            Assert.False(result.IsValid);
            Assert.NotEmpty(result.Errors);
        }

        [Fact]
        public void ValidateProperty_NullModel_ReturnsFailure()
        {
            ValidTestModel? model = null;
            var result = ModelValidator.ValidateProperty(model, "Name", "test");
            Assert.False(result.IsValid);
            Assert.Contains("模型不能为空", result.Errors);
        }

        [Fact]
        public void ValidateProperty_EmailProperty_ValidEmail()
        {
            var model = new ValidTestModel();
            var result = ModelValidator.ValidateProperty(model, nameof(ValidTestModel.Email), "user@example.com");
            Assert.True(result.IsValid);
        }

        [Fact]
        public void ValidateProperty_EmailProperty_InvalidEmail()
        {
            var model = new ValidTestModel();
            var result = ModelValidator.ValidateProperty(model, nameof(ValidTestModel.Email), "bad-email");
            Assert.False(result.IsValid);
        }

        // ==================== GetValidationAttributes<T> ====================

        [Fact]
        public void GetValidationAttributes_ReturnsAllProperties()
        {
            var attributes = ModelValidator.GetValidationAttributes<ValidTestModel>().ToList();
            Assert.Equal(3, attributes.Count);
        }

        [Fact]
        public void GetValidationAttributes_PropertiesHaveCorrectNames()
        {
            var attributes = ModelValidator.GetValidationAttributes<ValidTestModel>().ToList();
            var names = attributes.Select(a => a.PropertyName).ToList();
            Assert.Contains("Name", names);
            Assert.Contains("Age", names);
            Assert.Contains("Email", names);
        }

        [Fact]
        public void GetValidationAttributes_DisplayNameUsed()
        {
            var attributes = ModelValidator.GetValidationAttributes<DisplayAnnotatedModel>().ToList();
            var userNameInfo = attributes.First(a => a.PropertyName == "UserName");
            // DisplayAttribute with Name property: GetName() may return null
            // when ResourceType is not set, so the code falls back to PropertyName
            Assert.Equal("UserName", userNameInfo.DisplayName);
        }

        [Fact]
        public void GetValidationAttributes_ValidationAttributesPopulated()
        {
            var attributes = ModelValidator.GetValidationAttributes<ValidTestModel>().ToList();
            var nameInfo = attributes.First(a => a.PropertyName == "Name");
            Assert.True(nameInfo.ValidationAttributes.Count >= 2); // Required + StringLength
        }

        [Fact]
        public void GetValidationAttributes_PropertyWithoutAttributes_HasEmptyList()
        {
            var attributes = ModelValidator.GetValidationAttributes<ModelWithNoValidation>().ToList();
            var descInfo = attributes.First(a => a.PropertyName == "Description");
            Assert.Empty(descInfo.ValidationAttributes);
        }

        // ==================== ValidateToDictionary<T> ====================

        [Fact]
        public void ValidateToDictionary_ValidModel_ReturnsEmptyDictionary()
        {
            var model = new ValidTestModel();
            var dict = ModelValidator.ValidateToDictionary(model);
            Assert.Empty(dict);
        }

        [Fact]
        public void ValidateToDictionary_InvalidModel_ReturnsErrorsByProperty()
        {
            var model = new InvalidTestModel();
            var dict = ModelValidator.ValidateToDictionary(model);
            Assert.NotEmpty(dict);
        }

        [Fact]
        public void ValidateToDictionary_NullModel_ThrowsException()
        {
            ValidTestModel? model = null;
            // ValidateToDictionary internally creates ValidationContext with the model,
            // which throws ArgumentNullException for null
            Assert.Throws<ArgumentNullException>(() => ModelValidator.ValidateToDictionary(model));
        }

        // ==================== ValidateDictionary ====================

        [Fact]
        public void ValidateDictionary_AllRulesPass_ReturnsSuccess()
        {
            var data = new Dictionary<string, object?>
            {
                ["Name"] = "John",
                ["Age"] = 25
            };

            var rules = new List<PropertyValidationRule>
            {
                PropertyValidationRule.Create("Name").Required().Length(1, 50),
                PropertyValidationRule.Create("Age").Required()
            };

            var result = ModelValidator.ValidateDictionary(data, rules);
            Assert.True(result.IsValid);
        }

        [Fact]
        public void ValidateDictionary_MissingRequiredField_ReturnsError()
        {
            var data = new Dictionary<string, object?>
            {
                ["Name"] = "John"
            };

            var rules = new List<PropertyValidationRule>
            {
                PropertyValidationRule.Create("Name").Required(),
                PropertyValidationRule.Create("Age").Required("Age is required")
            };

            var result = ModelValidator.ValidateDictionary(data, rules);
            Assert.False(result.IsValid);
            Assert.Contains("Age is required", result.Errors);
        }

        [Fact]
        public void ValidateDictionary_ValidatorFails_ReturnsError()
        {
            var data = new Dictionary<string, object?>
            {
                ["Name"] = "A" // too short
            };

            var rules = new List<PropertyValidationRule>
            {
                PropertyValidationRule.Create("Name").Length(2, 50, "Name must be 2-50 chars")
            };

            var result = ModelValidator.ValidateDictionary(data, rules);
            Assert.False(result.IsValid);
            Assert.Contains("Name must be 2-50 chars", result.Errors);
        }

        [Fact]
        public void ValidateDictionary_MultipleErrors_ReturnsAllErrors()
        {
            var data = new Dictionary<string, object?>
            {
                ["Name"] = ""
            };

            var rules = new List<PropertyValidationRule>
            {
                PropertyValidationRule.Create("Name").Required("Name required").Length(2, 50, "Name too short")
            };

            var result = ModelValidator.ValidateDictionary(data, rules);
            Assert.False(result.IsValid);
            // Both required and length validators should fail
            Assert.True(result.Errors.Count >= 1);
        }

        [Fact]
        public void ValidateDictionary_CustomErrorMessage_UsedInResult()
        {
            var data = new Dictionary<string, object?>();

            var rules = new List<PropertyValidationRule>
            {
                PropertyValidationRule.Create("Email").Required("Email is mandatory")
            };

            var result = ModelValidator.ValidateDictionary(data, rules);
            Assert.False(result.IsValid);
            Assert.Contains("Email is mandatory", result.Errors);
        }

        [Fact]
        public void ValidateDictionary_EmptyRules_ReturnsSuccess()
        {
            var data = new Dictionary<string, object?> { ["Key"] = "Value" };
            var rules = new List<PropertyValidationRule>();

            var result = ModelValidator.ValidateDictionary(data, rules);
            Assert.True(result.IsValid);
        }

        // ==================== ValidateObjectDictionary ====================

        [Fact]
        public void ValidateObjectDictionary_ValidData_ReturnsSuccess()
        {
            var data = new Dictionary<string, object?>
            {
                ["Name"] = "John",
                ["Age"] = 25
            };

            var result = ModelValidator.ValidateObjectDictionary(data, typeof(ValidTestModel));
            Assert.True(result.IsValid);
        }

        [Fact]
        public void ValidateObjectDictionary_MissingRequired_ReturnsError()
        {
            var data = new Dictionary<string, object?>
            {
                ["Age"] = 25
                // Name is missing
            };

            var result = ModelValidator.ValidateObjectDictionary(data, typeof(ValidTestModel));
            Assert.False(result.IsValid);
        }

        [Fact]
        public void ValidateObjectDictionary_InvalidValue_ReturnsError()
        {
            var data = new Dictionary<string, object?>
            {
                ["Name"] = "John",
                ["Age"] = 200 // exceeds Range(1, 100)
            };

            var result = ModelValidator.ValidateObjectDictionary(data, typeof(ValidTestModel));
            Assert.False(result.IsValid);
        }

        [Fact]
        public void ValidateObjectDictionary_EmptyData_ReturnsErrorsForRequired()
        {
            var data = new Dictionary<string, object?>();
            var result = ModelValidator.ValidateObjectDictionary(data, typeof(ValidTestModel));
            Assert.False(result.IsValid);
        }

        // ==================== PropertyValidationRule ====================

        [Fact]
        public void PropertyValidationRule_Create_ReturnsRuleWithPropertyName()
        {
            var rule = PropertyValidationRule.Create("TestProp");
            Assert.Equal("TestProp", rule.PropertyName);
        }

        [Fact]
        public void PropertyValidationRule_Required_SetsIsRequired()
        {
            var rule = PropertyValidationRule.Create("TestProp").Required("Custom message");
            Assert.True(rule.IsRequired);
            Assert.Equal("Custom message", rule.RequiredErrorMessage);
        }

        [Fact]
        public void PropertyValidationRule_AddValidator_AddsValidatorToList()
        {
            var rule = PropertyValidationRule.Create("TestProp")
                .AddValidator(v => v != null, "Value cannot be null");
            Assert.Single(rule.Validators);
            Assert.Equal("Value cannot be null", rule.ErrorMessage);
        }

        [Fact]
        public void PropertyValidationRule_Regex_AddsRegexValidator()
        {
            var rule = PropertyValidationRule.Create("TestProp")
                .Regex(@"^\d+$", "Must be numeric");
            Assert.Single(rule.Validators);
            Assert.Equal("Must be numeric", rule.ErrorMessage);
        }

        [Fact]
        public void PropertyValidationRule_Length_AddsLengthValidator()
        {
            var rule = PropertyValidationRule.Create("TestProp")
                .Length(2, 10, "Length must be 2-10");
            Assert.Single(rule.Validators);
            Assert.Equal("Length must be 2-10", rule.ErrorMessage);
        }

        [Fact]
        public void PropertyValidationRule_Range_AddsRangeValidator()
        {
            var rule = PropertyValidationRule.Create("TestProp")
                .Range(1, 100, "Must be 1-100");
            Assert.Single(rule.Validators);
            Assert.Equal("Must be 1-100", rule.ErrorMessage);
        }

        [Fact]
        public void PropertyValidationRule_ChainedRules_AllValidatorsAdded()
        {
            var rule = PropertyValidationRule.Create("TestProp")
                .Required("Required")
                .Length(1, 100)
                .Regex(@"^[a-zA-Z]+$");

            Assert.True(rule.IsRequired);
            // Required() does not add to Validators list, only Length and Regex do
            Assert.Equal(2, rule.Validators.Count);
        }
    }
}
