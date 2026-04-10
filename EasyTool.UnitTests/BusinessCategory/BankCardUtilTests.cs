using Xunit;
using EasyTool.BusinessCategory;
using System;

namespace EasyTool.UnitTests.BusinessCategory
{
    public class BankCardUtilTests
    {
        #region 验证测试

        [Theory]
        [InlineData("6222021234567890")] // 工商银行借记卡（有效Luhn）
        [InlineData("6228481234567890")] // 农业银行借记卡
        [InlineData("6216601234567890")] // 中国银行借记卡
        [InlineData("6227001234567890")] // 建设银行借记卡
        [InlineData("4367421234567890")] // 建设银行信用卡（Visa）
        public void IsValidFormat_ValidCardNumbers_ReturnsTrue(string cardNumber)
        {
            Assert.True(BankCardUtil.IsValidFormat(cardNumber));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("123456789012")] // 12位
        [InlineData("12345678901234567890")] // 20位
        [InlineData("1234-5678-9012-3456")] // 包含横线
        [InlineData("1234 5678 9012 3456")] // 包含空格
        [InlineData("abcd123456789012")] // 包含字母
        public void IsValidFormat_InvalidCardNumbers_ReturnsFalse(string cardNumber)
        {
            Assert.False(BankCardUtil.IsValidFormat(cardNumber));
        }

        [Fact]
        public void ValidateLuhn_ValidLuhnNumber_ReturnsTrue()
        {
            // 已知的Luhn有效号码
            Assert.True(BankCardUtil.ValidateLuhn("4111111111111111")); // Visa测试卡号
            Assert.True(BankCardUtil.ValidateLuhn("4012888888881881")); // Visa测试卡号
            Assert.True(BankCardUtil.ValidateLuhn("378282246310005"));  // American Express测试卡号
        }

        [Fact]
        public void ValidateLuhn_InvalidLuhnNumber_ReturnsFalse()
        {
            Assert.False(BankCardUtil.ValidateLuhn("4111111111111112"));
            Assert.False(BankCardUtil.ValidateLuhn("1234567890123456"));
        }

        [Fact]
        public void ValidateLuhn_Null_ReturnsFalse()
        {
            Assert.False(BankCardUtil.ValidateLuhn(null));
        }

        [Fact]
        public void ValidateLuhn_EmptyString_ReturnsFalse()
        {
            Assert.False(BankCardUtil.ValidateLuhn(""));
        }

        [Fact]
        public void ValidateLuhn_WithNonDigit_ReturnsFalse()
        {
            Assert.False(BankCardUtil.ValidateLuhn("4111a111111111111"));
        }

        [Fact]
        public void IsValid_ValidCardWithCorrectLuhn_ReturnsTrue()
        {
            // 使用已知有效的Luhn号码
            Assert.True(BankCardUtil.IsValid("4111111111111111"));
        }

        [Fact]
        public void IsValid_InvalidLuhn_ReturnsFalse()
        {
            Assert.False(BankCardUtil.IsValid("6222021234567890"));
        }

        [Fact]
        public void CalculateLuhnCheckDigit_ValidNumber_ReturnsCorrectCheckDigit()
        {
            // 对于 411111111111111，校验位应该是 1
            int checkDigit = BankCardUtil.CalculateLuhnCheckDigit("411111111111111");
            Assert.Equal(1, checkDigit);
        }

        [Fact]
        public void CalculateLuhnCheckDigit_AnotherValidNumber_ReturnsCorrectCheckDigit()
        {
            // 对于 7992739871，校验位应该是 3
            int checkDigit = BankCardUtil.CalculateLuhnCheckDigit("7992739871");
            Assert.Equal(3, checkDigit);
        }

        [Fact]
        public void CalculateLuhnCheckDigit_Null_ReturnsNegativeOne()
        {
            Assert.Equal(-1, BankCardUtil.CalculateLuhnCheckDigit(null));
        }

        [Fact]
        public void CalculateLuhnCheckDigit_WithNonDigit_ReturnsNegativeOne()
        {
            Assert.Equal(-1, BankCardUtil.CalculateLuhnCheckDigit("1234a56789"));
        }

        #endregion

        #region 银行信息查询测试

        [Fact]
        public void GetBankInfo_ICBC_ReturnsCorrectInfo()
        {
            BankInfo? info = BankCardUtil.GetBankInfo("6222021234567890");
            Assert.NotNull(info);
            Assert.Equal("中国工商银行", info.Name);
            Assert.Equal(BankType.Debit, info.Type);
            Assert.Equal("ICBC", info.Code);
        }

        [Fact]
        public void GetBankInfo_ABC_ReturnsCorrectInfo()
        {
            BankInfo? info = BankCardUtil.GetBankInfo("6228481234567890");
            Assert.NotNull(info);
            Assert.Equal("中国农业银行", info.Name);
            Assert.Equal(BankType.Debit, info.Type);
            Assert.Equal("ABC", info.Code);
        }

        [Fact]
        public void GetBankInfo_BOC_ReturnsCorrectInfo()
        {
            BankInfo? info = BankCardUtil.GetBankInfo("6216601234567890");
            Assert.NotNull(info);
            Assert.Equal("中国银行", info.Name);
            Assert.Equal(BankType.Debit, info.Type);
            Assert.Equal("BOC", info.Code);
        }

        [Fact]
        public void GetBankInfo_CCB_ReturnsCorrectInfo()
        {
            BankInfo? info = BankCardUtil.GetBankInfo("6227001234567890");
            Assert.NotNull(info);
            Assert.Equal("中国建设银行", info.Name);
            Assert.Equal(BankType.Debit, info.Type);
            Assert.Equal("CCB", info.Code);
        }

        [Fact]
        public void GetBankInfo_CCBCreditCard_ReturnsCreditCard()
        {
            BankInfo? info = BankCardUtil.GetBankInfo("4367421234567890");
            Assert.NotNull(info);
            Assert.Equal("中国建设银行", info.Name);
            Assert.Equal(BankType.Credit, info.Type);
            Assert.Equal("CCB", info.Code);
        }

        [Fact]
        public void GetBankInfo_UnknownBIN_ReturnsNull()
        {
            Assert.Null(BankCardUtil.GetBankInfo("9999991234567890"));
        }

        [Fact]
        public void GetBankInfo_Null_ReturnsNull()
        {
            Assert.Null(BankCardUtil.GetBankInfo(null));
        }

        [Fact]
        public void GetBankInfo_ShortNumber_ReturnsNull()
        {
            Assert.Null(BankCardUtil.GetBankInfo("12345"));
        }

        [Fact]
        public void GetBankName_KnownBank_ReturnsBankName()
        {
            string name = BankCardUtil.GetBankName("6222021234567890");
            Assert.Equal("中国工商银行", name);
        }

        [Fact]
        public void GetBankName_UnknownBank_ReturnsNull()
        {
            Assert.Null(BankCardUtil.GetBankName("9999991234567890"));
        }

        [Fact]
        public void GetBankCode_KnownBank_ReturnsBankCode()
        {
            string code = BankCardUtil.GetBankCode("6222021234567890");
            Assert.Equal("ICBC", code);
        }

        [Fact]
        public void GetBankCode_UnknownBank_ReturnsNull()
        {
            Assert.Null(BankCardUtil.GetBankCode("9999991234567890"));
        }

        [Fact]
        public void GetBankType_DebitCard_ReturnsDebit()
        {
            BankType type = BankCardUtil.GetBankType("6222021234567890");
            Assert.Equal(BankType.Debit, type);
        }

        [Fact]
        public void GetBankType_CreditCard_ReturnsCredit()
        {
            BankType type = BankCardUtil.GetBankType("4367421234567890");
            Assert.Equal(BankType.Credit, type);
        }

        [Fact]
        public void GetBankType_UnknownBank_ReturnsUnknown()
        {
            Assert.Equal(BankType.Unknown, BankCardUtil.GetBankType("9999991234567890"));
        }

        [Fact]
        public void IsDebitCard_DebitCard_ReturnsTrue()
        {
            Assert.True(BankCardUtil.IsDebitCard("6222021234567890"));
        }

        [Fact]
        public void IsDebitCard_CreditCard_ReturnsFalse()
        {
            Assert.False(BankCardUtil.IsDebitCard("4367421234567890"));
        }

        [Fact]
        public void IsCreditCard_CreditCard_ReturnsTrue()
        {
            Assert.True(BankCardUtil.IsCreditCard("4367421234567890"));
        }

        [Fact]
        public void IsCreditCard_DebitCard_ReturnsFalse()
        {
            Assert.False(BankCardUtil.IsCreditCard("6222021234567890"));
        }

        [Fact]
        public void GetBinCode_ValidCard_ReturnsFirst6Digits()
        {
            string binCode = BankCardUtil.GetBinCode("6222021234567890");
            Assert.Equal("622202", binCode);
        }

        [Fact]
        public void GetBinCode_Null_ReturnsNull()
        {
            Assert.Null(BankCardUtil.GetBinCode(null));
        }

        [Fact]
        public void GetBinCode_ShortNumber_ReturnsNull()
        {
            Assert.Null(BankCardUtil.GetBinCode("12345"));
        }

        #endregion

        #region 格式化测试

        [Fact]
        public void Format_ValidCardNumber_ReturnsFormatted()
        {
            string formatted = BankCardUtil.Format("6222021234567890");
            Assert.Equal("6222 0212 3456 7890", formatted);
        }

        [Fact]
        public void Format_WithNonDigitChars_ReturnsFormatted()
        {
            string formatted = BankCardUtil.Format("6222-0212-3456-7890");
            Assert.Equal("6222 0212 3456 7890", formatted);
        }

        [Fact]
        public void Format_Null_ReturnsNull()
        {
            Assert.Null(BankCardUtil.Format(null));
        }

        [Fact]
        public void Format_InvalidNumber_ReturnsNull()
        {
            Assert.Null(BankCardUtil.Format("12345"));
        }

        [Fact]
        public void Format_16DigitCard_ReturnsCorrectlyFormatted()
        {
            string formatted = BankCardUtil.Format("1234567890123456");
            Assert.Equal("1234 5678 9012 3456", formatted);
        }

        [Fact]
        public void Format_19DigitCard_ReturnsCorrectlyFormatted()
        {
            string formatted = BankCardUtil.Format("1234567890123456789");
            Assert.Equal("1234 5678 9012 3456 789", formatted);
        }

        [Fact]
        public void Mask_ValidCardNumber_ReturnsMasked()
        {
            string masked = BankCardUtil.Mask("6222021234567890");
            Assert.Equal("6222********7890", masked);
        }

        [Fact]
        public void Mask_WithNonDigitChars_ReturnsMasked()
        {
            string masked = BankCardUtil.Mask("6222-0212-3456-7890");
            Assert.Equal("6222********7890", masked);
        }

        [Fact]
        public void Mask_Null_ReturnsNull()
        {
            Assert.Null(BankCardUtil.Mask(null));
        }

        [Fact]
        public void Mask_ShortNumber_ReturnsNull()
        {
            Assert.Null(BankCardUtil.Mask("1234567"));
        }

        [Fact]
        public void Mask_19DigitCard_MasksMiddlePart()
        {
            string masked = BankCardUtil.Mask("1234567890123456789");
            Assert.Equal("1234***********6789", masked);
        }

        #endregion

        #region 边界测试

        [Fact]
        public void IsValidFormat_Null_ReturnsFalse()
        {
            Assert.False(BankCardUtil.IsValidFormat(null));
        }

        [Fact]
        public void IsValidFormat_EmptyString_ReturnsFalse()
        {
            Assert.False(BankCardUtil.IsValidFormat(""));
        }

        [Fact]
        public void IsValid_Null_ReturnsFalse()
        {
            Assert.False(BankCardUtil.IsValid(null));
        }

        [Fact]
        public void GetBankInfo_EmptyString_ReturnsNull()
        {
            Assert.Null(BankCardUtil.GetBankInfo(""));
        }

        [Fact]
        public void GetBankName_Null_ReturnsNull()
        {
            Assert.Null(BankCardUtil.GetBankName(null));
        }

        [Fact]
        public void GetBankCode_Null_ReturnsNull()
        {
            Assert.Null(BankCardUtil.GetBankCode(null));
        }

        [Fact]
        public void IsDebitCard_Null_ReturnsFalse()
        {
            Assert.False(BankCardUtil.IsDebitCard(null));
        }

        [Fact]
        public void IsCreditCard_Null_ReturnsFalse()
        {
            Assert.False(BankCardUtil.IsCreditCard(null));
        }

        #endregion

        #region 不同银行测试

        [Theory]
        [InlineData("6222601234567890", "交通银行", BankType.Debit, "BOCOM")]
        [InlineData("6225801234567890", "招商银行", BankType.Debit, "CMB")]
        [InlineData("6225181234567890", "浦发银行", BankType.Debit, "SPDB")]
        [InlineData("6226151234567890", "民生银行", BankType.Debit, "CMBC")]
        [InlineData("6229091234567890", "兴业银行", BankType.Debit, "CIB")]
        [InlineData("6226901234567890", "中信银行", BankType.Debit, "CITIC")]
        [InlineData("6226551234567890", "光大银行", BankType.Debit, "CEB")]
        [InlineData("6221551234567890", "平安银行", BankType.Debit, "PAB")]
        [InlineData("6226301234567890", "华夏银行", BankType.Debit, "HXB")]
        [InlineData("6225681234567890", "广发银行", BankType.Debit, "CGB")]
        [InlineData("6221501234567890", "邮储银行", BankType.Debit, "PSBC")]
        [InlineData("6223091234567890", "北京银行", BankType.Debit, "BJBANK")]
        [InlineData("6224621234567890", "上海银行", BankType.Debit, "SHBANK")]
        public void GetBankInfo_DifferentBanks_ReturnsCorrectInfo(string cardNumber, string expectedName, BankType expectedType, string expectedCode)
        {
            BankInfo? info = BankCardUtil.GetBankInfo(cardNumber);
            Assert.NotNull(info);
            Assert.Equal(expectedName, info.Name);
            Assert.Equal(expectedType, info.Type);
            Assert.Equal(expectedCode, info.Code);
        }

        #endregion
    }
}
