using Xunit;

namespace EasyTool.ConvertCategory.Tests
{
    public class ConvertUtilTests
    {
        #region ToInt Tests

        [Fact]
        public void ToInt_FromInt_ReturnsSameValue()
        {
            Assert.Equal(42, ConvertUtil.ToInt(42));
        }

        [Fact]
        public void ToInt_FromLong_ReturnsConverted()
        {
            Assert.Equal(100, ConvertUtil.ToInt(100L));
        }

        [Fact]
        public void ToInt_FromDouble_ReturnsTruncated()
        {
            Assert.Equal(42, ConvertUtil.ToInt(42.9));
        }

        [Fact]
        public void ToInt_FromString_ReturnsParsed()
        {
            Assert.Equal(123, ConvertUtil.ToInt("123"));
        }

        [Fact]
        public void ToInt_FromInvalidString_ReturnsDefault()
        {
            Assert.Equal(0, ConvertUtil.ToInt("abc"));
            Assert.Equal(99, ConvertUtil.ToInt("abc", 99));
        }

        [Fact]
        public void ToInt_FromNull_ReturnsDefault()
        {
            Assert.Equal(0, ConvertUtil.ToInt(null));
            Assert.Equal(50, ConvertUtil.ToInt(null, 50));
        }

        [Fact]
        public void ToInt_FromBool_ReturnsOneOrZero()
        {
            Assert.Equal(1, ConvertUtil.ToInt(true));
            Assert.Equal(0, ConvertUtil.ToInt(false));
        }

        #endregion

        #region ToLong Tests

        [Fact]
        public void ToLong_FromLong_ReturnsSameValue()
        {
            Assert.Equal(123456789L, ConvertUtil.ToLong(123456789L));
        }

        [Fact]
        public void ToLong_FromString_ReturnsParsed()
        {
            Assert.Equal(9876543210L, ConvertUtil.ToLong("9876543210"));
        }

        [Fact]
        public void ToLong_FromInvalidString_ReturnsDefault()
        {
            Assert.Equal(0L, ConvertUtil.ToLong("invalid"));
            Assert.Equal(999L, ConvertUtil.ToLong("invalid", 999L));
        }

        [Fact]
        public void ToLong_FromNull_ReturnsDefault()
        {
            Assert.Equal(0L, ConvertUtil.ToLong(null));
        }

        #endregion

        #region ToDouble Tests

        [Fact]
        public void ToDouble_FromDouble_ReturnsSameValue()
        {
            Assert.Equal(3.14, ConvertUtil.ToDouble(3.14), 5);
        }

        [Fact]
        public void ToDouble_FromString_ReturnsParsed()
        {
            Assert.Equal(2.718, ConvertUtil.ToDouble("2.718"), 5);
        }

        [Fact]
        public void ToDouble_FromInvalidString_ReturnsDefault()
        {
            Assert.Equal(0.0, ConvertUtil.ToDouble("invalid"));
            Assert.Equal(1.5, ConvertUtil.ToDouble("invalid", 1.5), 5);
        }

        [Fact]
        public void ToDouble_FromInt_ReturnsConverted()
        {
            Assert.Equal(42.0, ConvertUtil.ToDouble(42), 5);
        }

        #endregion

        #region ToDecimal Tests

        [Fact]
        public void ToDecimal_FromDecimal_ReturnsSameValue()
        {
            Assert.Equal(123.456m, ConvertUtil.ToDecimal(123.456m));
        }

        [Fact]
        public void ToDecimal_FromString_ReturnsParsed()
        {
            Assert.Equal(789.01m, ConvertUtil.ToDecimal("789.01"));
        }

        [Fact]
        public void ToDecimal_FromInvalidString_ReturnsDefault()
        {
            Assert.Equal(0m, ConvertUtil.ToDecimal("invalid"));
            Assert.Equal(99.9m, ConvertUtil.ToDecimal("invalid", 99.9m));
        }

        #endregion

        #region ToBool Tests

        [Fact]
        public void ToBool_FromBool_ReturnsSameValue()
        {
            Assert.True(ConvertUtil.ToBool(true));
            Assert.False(ConvertUtil.ToBool(false));
        }

        [Fact]
        public void ToBool_FromString_TrueVariants()
        {
            Assert.True(ConvertUtil.ToBool("true"));
            Assert.True(ConvertUtil.ToBool("TRUE"));
            Assert.True(ConvertUtil.ToBool("1"));
            Assert.True(ConvertUtil.ToBool("yes"));
            Assert.True(ConvertUtil.ToBool("YES"));
        }

        [Fact]
        public void ToBool_FromString_FalseVariants()
        {
            Assert.False(ConvertUtil.ToBool("false"));
            Assert.False(ConvertUtil.ToBool("FALSE"));
            Assert.False(ConvertUtil.ToBool("0"));
            Assert.False(ConvertUtil.ToBool("no"));
        }

        [Fact]
        public void ToBool_FromInt_ReturnsCorrectBool()
        {
            Assert.True(ConvertUtil.ToBool(1));
            Assert.False(ConvertUtil.ToBool(0));
        }

        #endregion

        #region ToString Tests

        [Fact]
        public void ToString_FromAny_ReturnsString()
        {
            Assert.Equal("42", ConvertUtil.ToString(42));
            Assert.Equal("True", ConvertUtil.ToString(true));
            Assert.Equal("3.14", ConvertUtil.ToString(3.14));
        }

        [Fact]
        public void ToString_FromNull_ReturnsEmptyOrDefault()
        {
            Assert.Equal("", ConvertUtil.ToString(null));
            Assert.Equal("default", ConvertUtil.ToString(null, "default"));
        }

        #endregion
    }
}