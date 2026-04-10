using Xunit;
using EasyTool.BusinessCategory;

namespace EasyTool.UnitTests.BusinessCategory
{
    public class PhoneNumberUtilTests
    {
        #region 验证测试

        [Theory]
        [InlineData("13800138000")]
        [InlineData("15912345678")]
        [InlineData("18888888888")]
        [InlineData("19123456789")]
        [InlineData("13012345678")]
        [InlineData("14512345678")]
        [InlineData("17712345678")]
        public void IsValid_ValidPhoneNumbers_ReturnsTrue(string phoneNumber)
        {
            Assert.True(PhoneNumberUtil.IsValid(phoneNumber));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("12345678901")] // 不以1开头
        [InlineData("1380013800")]  // 10位
        [InlineData("138001380000")] // 12位
        [InlineData("12800138000")] // 第2位是2
        [InlineData("1380013800a")] // 包含字母
        [InlineData("138-0013-8000")] // 包含横线
        [InlineData("138 0013 8000")] // 包含空格
        public void IsValid_InvalidPhoneNumbers_ReturnsFalse(string phoneNumber)
        {
            Assert.False(PhoneNumberUtil.IsValid(phoneNumber));
        }

        [Theory]
        [InlineData("13800138000")]
        [InlineData("159-1234-5678")]
        [InlineData("188 8888 8888")]
        [InlineData("+86 191 2345 6789")]
        public void Normalize_ValidPhoneNumbers_ReturnsNormalized(string phoneNumber)
        {
            string? normalized = PhoneNumberUtil.Normalize(phoneNumber);
            Assert.NotNull(normalized);
            Assert.Equal(11, normalized!.Length);
            Assert.Matches("^1[3-9]\\d{9}$", normalized);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("12345678901")]
        [InlineData("12800138000")]
        public void Normalize_InvalidPhoneNumbers_ReturnsNull(string phoneNumber)
        {
            Assert.Null(PhoneNumberUtil.Normalize(phoneNumber));
        }

        #endregion

        #region 运营商识别测试

        [Fact]
        public void GetCarrier_ChinaMobile_ReturnsChinaMobile()
        {
            Assert.Equal(Carrier.ChinaMobile, PhoneNumberUtil.GetCarrier("13800138000"));
            Assert.Equal(Carrier.ChinaMobile, PhoneNumberUtil.GetCarrier("15912345678"));
            Assert.Equal(Carrier.ChinaMobile, PhoneNumberUtil.GetCarrier("18888888888"));
        }

        [Fact]
        public void GetCarrier_ChinaUnicom_ReturnsChinaUnicom()
        {
            Assert.Equal(Carrier.ChinaUnicom, PhoneNumberUtil.GetCarrier("13012345678"));
            Assert.Equal(Carrier.ChinaUnicom, PhoneNumberUtil.GetCarrier("13112345678"));
            Assert.Equal(Carrier.ChinaUnicom, PhoneNumberUtil.GetCarrier("18612345678"));
        }

        [Fact]
        public void GetCarrier_ChinaTelecom_ReturnsChinaTelecom()
        {
            Assert.Equal(Carrier.ChinaTelecom, PhoneNumberUtil.GetCarrier("13312345678"));
            Assert.Equal(Carrier.ChinaTelecom, PhoneNumberUtil.GetCarrier("18012345678"));
            Assert.Equal(Carrier.ChinaTelecom, PhoneNumberUtil.GetCarrier("18912345678"));
        }

        [Fact]
        public void GetCarrier_ChinaBroadnet_ReturnsChinaBroadnet()
        {
            Assert.Equal(Carrier.ChinaBroadnet, PhoneNumberUtil.GetCarrier("19212345678"));
        }

        [Fact]
        public void GetCarrier_InvalidNumber_ReturnsUnknown()
        {
            Assert.Equal(Carrier.Unknown, PhoneNumberUtil.GetCarrier("12345678901"));
            Assert.Equal(Carrier.Unknown, PhoneNumberUtil.GetCarrier(null));
        }

        [Fact]
        public void GetCarrierName_ChinaMobile_ReturnsCorrectName()
        {
            string name = PhoneNumberUtil.GetCarrierName("13800138000");
            Assert.Equal("中国移动", name);
        }

        [Fact]
        public void GetCarrierName_ChinaUnicom_ReturnsCorrectName()
        {
            string name = PhoneNumberUtil.GetCarrierName("13012345678");
            Assert.Equal("中国联通", name);
        }

        [Fact]
        public void GetCarrierName_ChinaTelecom_ReturnsCorrectName()
        {
            string name = PhoneNumberUtil.GetCarrierName("13312345678");
            Assert.Equal("中国电信", name);
        }

        [Fact]
        public void GetCarrierName_ChinaBroadnet_ReturnsCorrectName()
        {
            string name = PhoneNumberUtil.GetCarrierName("19212345678");
            Assert.Equal("中国广电", name);
        }

        [Fact]
        public void GetCarrierName_InvalidNumber_ReturnsNull()
        {
            Assert.Null(PhoneNumberUtil.GetCarrierName("12345678901"));
        }

        [Theory]
        [InlineData("13800138000", true)]
        [InlineData("13012345678", false)]
        [InlineData("13312345678", false)]
        public void IsChinaMobile_ReturnsCorrectResult(string phoneNumber, bool expected)
        {
            Assert.Equal(expected, PhoneNumberUtil.IsChinaMobile(phoneNumber));
        }

        [Theory]
        [InlineData("13012345678", true)]
        [InlineData("13800138000", false)]
        [InlineData("13312345678", false)]
        public void IsChinaUnicom_ReturnsCorrectResult(string phoneNumber, bool expected)
        {
            Assert.Equal(expected, PhoneNumberUtil.IsChinaUnicom(phoneNumber));
        }

        [Theory]
        [InlineData("13312345678", true)]
        [InlineData("13800138000", false)]
        [InlineData("13012345678", false)]
        public void IsChinaTelecom_ReturnsCorrectResult(string phoneNumber, bool expected)
        {
            Assert.Equal(expected, PhoneNumberUtil.IsChinaTelecom(phoneNumber));
        }

        [Theory]
        [InlineData("19212345678", true)]
        [InlineData("13800138000", false)]
        [InlineData("13012345678", false)]
        public void IsChinaBroadnet_ReturnsCorrectResult(string phoneNumber, bool expected)
        {
            Assert.Equal(expected, PhoneNumberUtil.IsChinaBroadnet(phoneNumber));
        }

        #endregion

        #region 格式化测试

        [Fact]
        public void FormatWithSpaces_ValidPhoneNumber_ReturnsFormatted()
        {
            string formatted = PhoneNumberUtil.FormatWithSpaces("13800138000");
            Assert.Equal("138 0013 8000", formatted);
        }

        [Fact]
        public void FormatWithSpaces_WithSeparators_ReturnsFormatted()
        {
            string formatted = PhoneNumberUtil.FormatWithSpaces("138-0013-8000");
            Assert.Equal("138 0013 8000", formatted);
        }

        [Fact]
        public void FormatWithSpaces_InvalidNumber_ReturnsNull()
        {
            Assert.Null(PhoneNumberUtil.FormatWithSpaces("12345678901"));
        }

        [Fact]
        public void FormatWithHyphens_ValidPhoneNumber_ReturnsFormatted()
        {
            string formatted = PhoneNumberUtil.FormatWithHyphens("13800138000");
            Assert.Equal("138-0013-8000", formatted);
        }

        [Fact]
        public void FormatWithHyphens_WithSeparators_ReturnsFormatted()
        {
            string formatted = PhoneNumberUtil.FormatWithHyphens("138 0013 8000");
            Assert.Equal("138-0013-8000", formatted);
        }

        [Fact]
        public void FormatWithHyphens_InvalidNumber_ReturnsNull()
        {
            Assert.Null(PhoneNumberUtil.FormatWithHyphens("12345678901"));
        }

        [Fact]
        public void FormatWithCountryCode_ValidPhoneNumber_ReturnsFormatted()
        {
            string formatted = PhoneNumberUtil.FormatWithCountryCode("13800138000");
            Assert.Equal("+86 13800138000", formatted);
        }

        [Fact]
        public void FormatWithCountryCode_InvalidNumber_ReturnsNull()
        {
            Assert.Null(PhoneNumberUtil.FormatWithCountryCode("12345678901"));
        }

        [Fact]
        public void Mask_ValidPhoneNumber_ReturnsMasked()
        {
            string masked = PhoneNumberUtil.Mask("13800138000");
            Assert.Equal("138****8000", masked);
        }

        [Fact]
        public void Mask_WithSeparators_ReturnsMasked()
        {
            string masked = PhoneNumberUtil.Mask("138-0013-8000");
            Assert.Equal("138****8000", masked);
        }

        [Fact]
        public void Mask_InvalidNumber_ReturnsNull()
        {
            Assert.Null(PhoneNumberUtil.Mask("12345678901"));
        }

        #endregion

        #region 生成测试

        [Fact]
        public void GenerateRandom_ReturnsValidNumber()
        {
            string phoneNumber = PhoneNumberUtil.GenerateRandom();
            Assert.True(PhoneNumberUtil.IsValid(phoneNumber));
        }

        [Fact]
        public void GenerateRandom_WithCarrier_ChinaMobile()
        {
            string phoneNumber = PhoneNumberUtil.GenerateRandom(Carrier.ChinaMobile);
            Assert.True(PhoneNumberUtil.IsChinaMobile(phoneNumber));
        }

        [Fact]
        public void GenerateRandom_WithCarrier_ChinaUnicom()
        {
            string phoneNumber = PhoneNumberUtil.GenerateRandom(Carrier.ChinaUnicom);
            Assert.True(PhoneNumberUtil.IsChinaUnicom(phoneNumber));
        }

        [Fact]
        public void GenerateRandom_WithCarrier_ChinaTelecom()
        {
            string phoneNumber = PhoneNumberUtil.GenerateRandom(Carrier.ChinaTelecom);
            Assert.True(PhoneNumberUtil.IsChinaTelecom(phoneNumber));
        }

        [Fact]
        public void GenerateRandom_WithCarrier_ChinaBroadnet()
        {
            string phoneNumber = PhoneNumberUtil.GenerateRandom(Carrier.ChinaBroadnet);
            Assert.True(PhoneNumberUtil.IsChinaBroadnet(phoneNumber));
        }

        [Fact]
        public void GenerateRandom_MultipleCalls_ReturnsDifferentNumbers()
        {
            var numbers = new HashSet<string>();
            for (int i = 0; i < 100; i++)
            {
                numbers.Add(PhoneNumberUtil.GenerateRandom());
            }
            Assert.True(numbers.Count > 50); // 至少有一半是唯一的
        }

        #endregion

        #region 边界测试

        [Fact]
        public void IsValid_Null_ReturnsFalse()
        {
            Assert.False(PhoneNumberUtil.IsValid(null));
        }

        [Fact]
        public void IsValid_EmptyString_ReturnsFalse()
        {
            Assert.False(PhoneNumberUtil.IsValid(""));
        }

        [Fact]
        public void IsValid_Whitespace_ReturnsFalse()
        {
            Assert.False(PhoneNumberUtil.IsValid("   "));
        }

        [Fact]
        public void Normalize_Null_ReturnsNull()
        {
            Assert.Null(PhoneNumberUtil.Normalize(null));
        }

        [Fact]
        public void Normalize_EmptyString_ReturnsNull()
        {
            Assert.Null(PhoneNumberUtil.Normalize(""));
        }

        [Fact]
        public void GetCarrier_Null_ReturnsUnknown()
        {
            Assert.Equal(Carrier.Unknown, PhoneNumberUtil.GetCarrier(null));
        }

        [Fact]
        public void GetCarrierName_Null_ReturnsNull()
        {
            Assert.Null(PhoneNumberUtil.GetCarrierName(null));
        }

        [Fact]
        public void IsChinaMobile_Null_ReturnsFalse()
        {
            Assert.False(PhoneNumberUtil.IsChinaMobile(null));
        }

        [Fact]
        public void IsChinaUnicom_Null_ReturnsFalse()
        {
            Assert.False(PhoneNumberUtil.IsChinaUnicom(null));
        }

        [Fact]
        public void IsChinaTelecom_Null_ReturnsFalse()
        {
            Assert.False(PhoneNumberUtil.IsChinaTelecom(null));
        }

        [Fact]
        public void IsChinaBroadnet_Null_ReturnsFalse()
        {
            Assert.False(PhoneNumberUtil.IsChinaBroadnet(null));
        }

        #endregion
    }
}
