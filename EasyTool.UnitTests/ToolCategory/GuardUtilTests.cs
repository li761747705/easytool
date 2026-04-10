using System;
using System.Collections.Generic;
using System.IO;
using Xunit;
using EasyTool.ToolCategory;

namespace EasyTool.ToolCategory.Tests
{
    public class GuardUtilTests
    {
        // ==================== NotNull<T> (class) ====================

        [Fact]
        public void NotNull_Class_ValidValue_ReturnsValue()
        {
            var obj = new object();
            var result = GuardUtil.NotNull(obj, "param");
            Assert.Same(obj, result);
        }

        [Fact]
        public void NotNull_Class_NullValue_ThrowsArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => GuardUtil.NotNull((string?)null, "myParam"));
            Assert.Equal("myParam", ex.ParamName);
        }

        // ==================== NotNull<T> (struct) ====================

        [Fact]
        public void NotNull_Struct_ValidValue_ReturnsValue()
        {
            int? value = 42;
            var result = GuardUtil.NotNull(value, "param");
            Assert.Equal(42, result);
        }

        [Fact]
        public void NotNull_Struct_NullValue_ThrowsArgumentNullException()
        {
            int? value = null;
            var ex = Assert.Throws<ArgumentNullException>(() => GuardUtil.NotNull(value, "myParam"));
            Assert.Equal("myParam", ex.ParamName);
        }

        // ==================== NotNullOrEmpty ====================

        [Fact]
        public void NotNullOrEmpty_ValidString_ReturnsString()
        {
            var result = GuardUtil.NotNullOrEmpty("hello", "param");
            Assert.Equal("hello", result);
        }

        [Fact]
        public void NotNullOrEmpty_NullString_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => GuardUtil.NotNullOrEmpty(null, "param"));
        }

        [Fact]
        public void NotNullOrEmpty_EmptyString_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => GuardUtil.NotNullOrEmpty("", "param"));
        }

        // ==================== NotNullOrWhiteSpace ====================

        [Fact]
        public void NotNullOrWhiteSpace_ValidString_ReturnsString()
        {
            var result = GuardUtil.NotNullOrWhiteSpace("hello world", "param");
            Assert.Equal("hello world", result);
        }

        [Fact]
        public void NotNullOrWhiteSpace_NullString_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => GuardUtil.NotNullOrWhiteSpace(null, "param"));
        }

        [Fact]
        public void NotNullOrWhiteSpace_EmptyString_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => GuardUtil.NotNullOrWhiteSpace("", "param"));
        }

        [Fact]
        public void NotNullOrWhiteSpace_WhitespaceString_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => GuardUtil.NotNullOrWhiteSpace("   \t  ", "param"));
        }

        // ==================== NotEmpty (IEnumerable) ====================

        [Fact]
        public void NotEmpty_NonEmptyCollection_ReturnsCollection()
        {
            var list = new List<int> { 1, 2, 3 };
            var result = GuardUtil.NotEmpty(list, "param");
            Assert.Equal(3, result.Count());
        }

        [Fact]
        public void NotEmpty_NullCollection_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => GuardUtil.NotEmpty((IEnumerable<int>?)null, "param"));
        }

        [Fact]
        public void NotEmpty_EmptyCollection_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => GuardUtil.NotEmpty(new List<int>(), "param"));
        }

        // ==================== InRange (int) ====================

        [Fact]
        public void InRange_Int_ValueInRange_ReturnsValue()
        {
            var result = GuardUtil.InRange(5, 1, 10, "param");
            Assert.Equal(5, result);
        }

        [Fact]
        public void InRange_Int_ValueAtMinBoundary_ReturnsValue()
        {
            var result = GuardUtil.InRange(1, 1, 10, "param");
            Assert.Equal(1, result);
        }

        [Fact]
        public void InRange_Int_ValueAtMaxBoundary_ReturnsValue()
        {
            var result = GuardUtil.InRange(10, 1, 10, "param");
            Assert.Equal(10, result);
        }

        [Fact]
        public void InRange_Int_ValueBelowRange_ThrowsArgumentOutOfRangeException()
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => GuardUtil.InRange(0, 1, 10, "param"));
            Assert.Equal("param", ex.ParamName);
        }

        [Fact]
        public void InRange_Int_ValueAboveRange_ThrowsArgumentOutOfRangeException()
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => GuardUtil.InRange(11, 1, 10, "param"));
            Assert.Equal("param", ex.ParamName);
        }

        // ==================== InRange (double) ====================

        [Fact]
        public void InRange_Double_ValueInRange_ReturnsValue()
        {
            var result = GuardUtil.InRange(5.5, 1.0, 10.0, "param");
            Assert.Equal(5.5, result);
        }

        [Fact]
        public void InRange_Double_ValueAtBoundary_ReturnsValue()
        {
            var result = GuardUtil.InRange(1.0, 1.0, 10.0, "param");
            Assert.Equal(1.0, result);
        }

        [Fact]
        public void InRange_Double_ValueOutOfRange_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => GuardUtil.InRange(0.5, 1.0, 10.0, "param"));
        }

        // ==================== GreaterThan ====================

        [Fact]
        public void GreaterThan_ValidValue_ReturnsValue()
        {
            var result = GuardUtil.GreaterThan(6, 5, "param");
            Assert.Equal(6, result);
        }

        [Fact]
        public void GreaterThan_EqualToThreshold_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => GuardUtil.GreaterThan(5, 5, "param"));
        }

        [Fact]
        public void GreaterThan_LessThanThreshold_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => GuardUtil.GreaterThan(4, 5, "param"));
        }

        // ==================== GreaterThanOrEqual ====================

        [Fact]
        public void GreaterThanOrEqual_GreaterThan_ReturnsValue()
        {
            var result = GuardUtil.GreaterThanOrEqual(6, 5, "param");
            Assert.Equal(6, result);
        }

        [Fact]
        public void GreaterThanOrEqual_EqualTo_ReturnsValue()
        {
            var result = GuardUtil.GreaterThanOrEqual(5, 5, "param");
            Assert.Equal(5, result);
        }

        [Fact]
        public void GreaterThanOrEqual_LessThan_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => GuardUtil.GreaterThanOrEqual(4, 5, "param"));
        }

        // ==================== LessThan ====================

        [Fact]
        public void LessThan_ValidValue_ReturnsValue()
        {
            var result = GuardUtil.LessThan(4, 5, "param");
            Assert.Equal(4, result);
        }

        [Fact]
        public void LessThan_EqualToThreshold_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => GuardUtil.LessThan(5, 5, "param"));
        }

        [Fact]
        public void LessThan_GreaterThanThreshold_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => GuardUtil.LessThan(6, 5, "param"));
        }

        // ==================== LessThanOrEqual ====================

        [Fact]
        public void LessThanOrEqual_LessThan_ReturnsValue()
        {
            var result = GuardUtil.LessThanOrEqual(4, 5, "param");
            Assert.Equal(4, result);
        }

        [Fact]
        public void LessThanOrEqual_EqualTo_ReturnsValue()
        {
            var result = GuardUtil.LessThanOrEqual(5, 5, "param");
            Assert.Equal(5, result);
        }

        [Fact]
        public void LessThanOrEqual_GreaterThan_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => GuardUtil.LessThanOrEqual(6, 5, "param"));
        }

        // ==================== IsTrue ====================

        [Fact]
        public void IsTrue_ConditionTrue_DoesNotThrow()
        {
            GuardUtil.IsTrue(true, "should not throw");
        }

        [Fact]
        public void IsTrue_ConditionFalse_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => GuardUtil.IsTrue(false, "condition was false", "param"));
        }

        // ==================== IsFalse ====================

        [Fact]
        public void IsFalse_ConditionFalse_DoesNotThrow()
        {
            GuardUtil.IsFalse(false, "should not throw");
        }

        [Fact]
        public void IsFalse_ConditionTrue_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => GuardUtil.IsFalse(true, "condition was true", "param"));
        }

        // ==================== IsType<T> ====================

        [Fact]
        public void IsType_CorrectType_ReturnsTypedValue()
        {
            object obj = "hello";
            var result = GuardUtil.IsType<string>(obj, "param");
            Assert.Equal("hello", result);
        }

        [Fact]
        public void IsType_WrongType_ThrowsArgumentException()
        {
            object obj = 42;
            Assert.Throws<ArgumentException>(() => GuardUtil.IsType<string>(obj, "param"));
        }

        // ==================== EnumDefined ====================

        [Fact]
        public void EnumDefined_ValidEnumValue_ReturnsValue()
        {
            var result = GuardUtil.EnumDefined(BackoffStrategy.Exponential, "param");
            Assert.Equal(BackoffStrategy.Exponential, result);
        }

        [Fact]
        public void EnumDefined_InvalidEnumValue_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => GuardUtil.EnumDefined((BackoffStrategy)99, "param"));
        }

        // ==================== Email ====================

        [Fact]
        public void Email_ValidEmail_ReturnsEmail()
        {
            var result = GuardUtil.Email("test@example.com", "param");
            Assert.Equal("test@example.com", result);
        }

        [Fact]
        public void Email_NullEmail_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => GuardUtil.Email(null, "param"));
        }

        [Fact]
        public void Email_EmptyEmail_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => GuardUtil.Email("", "param"));
        }

        [Fact]
        public void Email_InvalidEmail_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => GuardUtil.Email("not-an-email", "param"));
        }

        // ==================== FileExists ====================

        [Fact]
        public void FileExists_ExistingFile_ReturnsPath()
        {
            var tempFile = Path.GetTempFileName();
            try
            {
                var result = GuardUtil.FileExists(tempFile, "param");
                Assert.Equal(tempFile, result);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public void FileExists_NonExistentFile_ThrowsFileNotFoundException()
        {
            Assert.Throws<FileNotFoundException>(() =>
                GuardUtil.FileExists("C:\\nonexistent_file_12345.txt", "param"));
        }

        [Fact]
        public void FileExists_NullPath_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => GuardUtil.FileExists(null, "param"));
        }

        // ==================== DirectoryExists ====================

        [Fact]
        public void DirectoryExists_ExistingDirectory_ReturnsPath()
        {
            var tempDir = Path.GetTempPath();
            var result = GuardUtil.DirectoryExists(tempDir, "param");
            Assert.Equal(tempDir, result);
        }

        [Fact]
        public void DirectoryExists_NonExistentDirectory_ThrowsDirectoryNotFoundException()
        {
            Assert.Throws<DirectoryNotFoundException>(() =>
                GuardUtil.DirectoryExists("C:\\nonexistent_dir_12345", "param"));
        }

        [Fact]
        public void DirectoryExists_NullPath_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => GuardUtil.DirectoryExists(null, "param"));
        }

        // ==================== Throw<TException> ====================

        [Fact]
        public void Throw_ThrowsSpecifiedException()
        {
            Assert.Throws<InvalidOperationException>(() =>
                GuardUtil.Throw<InvalidOperationException>("test error"));
        }

        // ==================== ThrowIf<TException> ====================

        [Fact]
        public void ThrowIf_ConditionFalse_DoesNotThrow()
        {
            GuardUtil.ThrowIf<InvalidOperationException>(false, "should not throw");
        }

        [Fact]
        public void ThrowIf_ConditionTrue_ThrowsException()
        {
            Assert.Throws<InvalidOperationException>(() =>
                GuardUtil.ThrowIf<InvalidOperationException>(true, "thrown"));
        }
    }
}
