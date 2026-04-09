using Xunit;

namespace EasyTool.ValidationCategory.Tests
{
    public class FluentValidatorTests
    {
        [Fact]
        public void NotNull_WhenNull_AddsError()
        {
            var result = FluentValidator<string?>.For(null!, "test")
                .NotNull()
                .GetResult();
            Assert.False(result.IsValid);
        }

        [Fact]
        public void NotNull_WhenNotNull_NoError()
        {
            var result = FluentValidator<string>.For("value", "test")
                .NotNull()
                .GetResult();
            Assert.True(result.IsValid);
        }

        [Fact]
        public void NotEmpty_WhenEmpty_AddsError()
        {
            var result = FluentValidator<string>.For("", "test")
                .NotEmpty()
                .GetResult();
            Assert.False(result.IsValid);
        }

        [Fact]
        public void NotEmpty_WhenNotEmpty_NoError()
        {
            var result = FluentValidator<string>.For("value", "test")
                .NotEmpty()
                .GetResult();
            Assert.True(result.IsValid);
        }

        [Fact]
        public void NotWhiteSpace_WhenWhiteSpace_AddsError()
        {
            var result = FluentValidator<string>.For("   ", "test")
                .NotWhiteSpace()
                .GetResult();
            Assert.False(result.IsValid);
        }

        [Fact]
        public void NotWhiteSpace_WhenValid_NoError()
        {
            var result = FluentValidator<string>.For("value", "test")
                .NotWhiteSpace()
                .GetResult();
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Length_WithinRange_NoError()
        {
            var result = FluentValidator<string>.For("hello", "test")
                .Length(1, 10)
                .GetResult();
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Length_TooShort_AddsError()
        {
            var result = FluentValidator<string>.For("hi", "test")
                .Length(5, 10)
                .GetResult();
            Assert.False(result.IsValid);
        }

        [Fact]
        public void Length_TooLong_AddsError()
        {
            var result = FluentValidator<string>.For("this is a very long string", "test")
                .Length(1, 10)
                .GetResult();
            Assert.False(result.IsValid);
        }

        [Fact]
        public void MinLength_WhenValid_NoError()
        {
            var result = FluentValidator<string>.For("hello", "test")
                .MinLength(3)
                .GetResult();
            Assert.True(result.IsValid);
        }

        [Fact]
        public void MinLength_WhenTooShort_AddsError()
        {
            var result = FluentValidator<string>.For("hi", "test")
                .MinLength(5)
                .GetResult();
            Assert.False(result.IsValid);
        }

        [Fact]
        public void MaxLength_WhenValid_NoError()
        {
            var result = FluentValidator<string>.For("hello", "test")
                .MaxLength(10)
                .GetResult();
            Assert.True(result.IsValid);
        }

        [Fact]
        public void MaxLength_WhenTooLong_AddsError()
        {
            var result = FluentValidator<string>.For("this is too long", "test")
                .MaxLength(5)
                .GetResult();
            Assert.False(result.IsValid);
        }

        [Fact]
        public void Must_CustomValidation_Works()
        {
            var result = FluentValidator<int>.For(5, "test")
                .Must(x => x > 0, "必须大于0")
                .GetResult();
            Assert.True(result.IsValid);

            result = FluentValidator<int>.For(-1, "test")
                .Must(x => x > 0, "必须大于0")
                .GetResult();
            Assert.False(result.IsValid);
        }

        [Fact]
        public void StopOnFirstFailure_StopsOnFirstError()
        {
            var result = FluentValidator<string>.For("", "test")
                .StopOnFirstFailure()
                .NotEmpty()
                .MinLength(5) // Should not run
                .GetResult();
            Assert.False(result.IsValid);
            Assert.Single(result.Errors);
        }

        [Fact]
        public void MultipleValidations_CollectsAllErrors()
        {
            var result = FluentValidator<string>.For("", "test")
                .NotEmpty()
                .MinLength(5)
                .GetResult();
            Assert.False(result.IsValid);
            Assert.Equal(2, result.Errors.Count);
        }

        [Fact]
        public void GetResult_ReturnsValidationResult()
        {
            var result = FluentValidator<string>.For("test", "test")
                .NotEmpty()
                .MinLength(1)
                .GetResult();

            Assert.NotNull(result);
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void CustomErrorMessage_IsUsed()
        {
            var result = FluentValidator<string>.For(null!, "test")
                .NotNull("自定义错误消息")
                .GetResult();
            Assert.False(result.IsValid);
            Assert.Contains("自定义错误消息", result.Errors);
        }
    }
}