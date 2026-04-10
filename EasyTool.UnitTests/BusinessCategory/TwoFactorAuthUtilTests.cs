using Xunit;
using EasyTool.BusinessCategory;

namespace EasyTool.UnitTests.BusinessCategory
{
    public class TwoFactorAuthUtilTests
    {
        [Fact]
        public void GenerateSecret_ReturnsValidBase32String()
        {
            var secret = TwoFactorAuthUtil.GenerateSecret();

            Assert.NotNull(secret);
            Assert.True(secret.Length >= 16);
            Assert.Matches("^[A-Z2-7]+$", secret);
        }

        [Fact]
        public void GenerateSecret_CustomLength_ReturnsCorrectLength()
        {
            var secret = TwoFactorAuthUtil.GenerateSecret(32);

            // Base32 encoding: 32 bytes -> 52 chars (approximately)
            Assert.True(secret.Length >= 32);
        }

        [Fact]
        public void GenerateTotp_Returns6DigitCode()
        {
            var secret = TwoFactorAuthUtil.GenerateSecret();

            var totp = TwoFactorAuthUtil.GenerateTotp(secret);

            Assert.NotNull(totp);
            Assert.Equal(6, totp.Length);
            Assert.Matches("^[0-9]{6}$", totp);
        }

        [Fact]
        public void GenerateTotp_CustomDigits_ReturnsCorrectLength()
        {
            var secret = TwoFactorAuthUtil.GenerateSecret();

            var totp8 = TwoFactorAuthUtil.GenerateTotp(secret, digits: 8);

            Assert.Equal(8, totp8.Length);
            Assert.Matches("^[0-9]{8}$", totp8);
        }

        [Fact]
        public void VerifyTotp_ValidCode_ReturnsTrue()
        {
            var secret = TwoFactorAuthUtil.GenerateSecret();
            var totp = TwoFactorAuthUtil.GenerateTotp(secret);

            var result = TwoFactorAuthUtil.VerifyTotp(secret, totp);

            Assert.True(result);
        }

        [Fact]
        public void VerifyTotp_InvalidCode_ReturnsFalse()
        {
            var secret = TwoFactorAuthUtil.GenerateSecret();

            var result = TwoFactorAuthUtil.VerifyTotp(secret, "000000");

            Assert.False(result);
        }

        [Fact]
        public void VerifyTotp_EmptyCode_ReturnsFalse()
        {
            var secret = TwoFactorAuthUtil.GenerateSecret();

            var result = TwoFactorAuthUtil.VerifyTotp(secret, "");

            Assert.False(result);
        }

        [Fact]
        public void GetRemainingSeconds_ReturnsValueBetween1And30()
        {
            var remaining = TwoFactorAuthUtil.GetRemainingSeconds();

            Assert.InRange(remaining, 1, 30);
        }

        [Fact]
        public void GetOtpAuthUri_ReturnsValidUri()
        {
            var secret = TwoFactorAuthUtil.GenerateSecret();

            var uri = TwoFactorAuthUtil.GetOtpAuthUri("TestApp", "user@example.com", secret);

            Assert.StartsWith("otpauth://totp/", uri);
            Assert.Contains("TestApp", uri);
            Assert.Contains("user%40example.com", uri);
            Assert.Contains($"secret={secret}", uri);
        }

        [Fact]
        public void GetQrCodeContent_ReturnsSameAsOtpAuthUri()
        {
            var secret = TwoFactorAuthUtil.GenerateSecret();
            var issuer = "TestApp";
            var account = "user@example.com";

            var qrContent = TwoFactorAuthUtil.GetQrCodeContent(issuer, account, secret);
            var uri = TwoFactorAuthUtil.GetOtpAuthUri(issuer, account, secret);

            Assert.Equal(uri, qrContent);
        }

        [Fact]
        public void VerifyTotp_SameSecretDifferentCodes_BothValid()
        {
            var secret = TwoFactorAuthUtil.GenerateSecret();

            var code1 = TwoFactorAuthUtil.GenerateTotp(secret);
            var code2 = TwoFactorAuthUtil.GenerateTotp(secret);

            Assert.Equal(code1, code2);
            Assert.True(TwoFactorAuthUtil.VerifyTotp(secret, code1));
            Assert.True(TwoFactorAuthUtil.VerifyTotp(secret, code2));
        }

        #region 边界测试

        [Fact]
        public void GenerateSecret_DefaultLength_ReturnsValidBase32()
        {
            var secret = TwoFactorAuthUtil.GenerateSecret();
            Assert.True(secret.Length >= 16);
            Assert.Matches("^[A-Z2-7]+=*$", secret);
        }

        [Fact]
        public void GenerateTotp_InvalidSecret_ThrowsException()
        {
            // 无效的Base32密钥会触发解码异常
            Assert.Throws<FormatException>(() => TwoFactorAuthUtil.GenerateTotp("INVALID!SECRET"));
        }

        [Fact]
        public void VerifyTotp_WrongSecret_ReturnsFalse()
        {
            var secret1 = TwoFactorAuthUtil.GenerateSecret();
            var secret2 = TwoFactorAuthUtil.GenerateSecret();
            var code = TwoFactorAuthUtil.GenerateTotp(secret1);

            Assert.False(TwoFactorAuthUtil.VerifyTotp(secret2, code));
        }

        [Fact]
        public void GetRemainingSeconds_ReturnsValidRange()
        {
            var remaining = TwoFactorAuthUtil.GetRemainingSeconds();
            Assert.InRange(remaining, 1, 30);
        }

        [Fact]
        public void GetOtpAuthUri_ContainsAllRequiredParts()
        {
            var secret = TwoFactorAuthUtil.GenerateSecret();
            var uri = TwoFactorAuthUtil.GetOtpAuthUri("TestApp", "user@test.com", secret);

            Assert.StartsWith("otpauth://totp/", uri);
            Assert.Contains("issuer=TestApp", uri);
            Assert.Contains("secret=", uri);
        }

        [Fact]
        public void VerifyTotp_AllZerosCode_ReturnsFalse()
        {
            var secret = TwoFactorAuthUtil.GenerateSecret();
            Assert.False(TwoFactorAuthUtil.VerifyTotp(secret, "000000"));
        }

        [Fact]
        public void VerifyTotp_AllNinesCode_ReturnsFalse()
        {
            var secret = TwoFactorAuthUtil.GenerateSecret();
            Assert.False(TwoFactorAuthUtil.VerifyTotp(secret, "999999"));
        }

        #endregion
    }
}