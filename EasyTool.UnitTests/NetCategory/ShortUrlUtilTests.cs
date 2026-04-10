using System;
using System.Collections.Generic;
using Xunit;
using EasyTool.NetCategory;

namespace EasyTool.Tests
{
    /// <summary>
    /// ShortUrlUtil 工具类的单元测试
    /// </summary>
    public class ShortUrlUtilTests : IDisposable
    {
        public ShortUrlUtilTests()
        {
            // Save original config
            _originalCustomDomain = ShortUrlUtil.ShortUrlConfig.CustomDomain;
            _originalUseCustomDomain = ShortUrlUtil.ShortUrlConfig.UseCustomDomain;
        }

        public void Dispose()
        {
            // Restore original config
            ShortUrlUtil.ShortUrlConfig.CustomDomain = _originalCustomDomain;
            ShortUrlUtil.ShortUrlConfig.UseCustomDomain = _originalUseCustomDomain;
        }

        private readonly string? _originalCustomDomain;
        private readonly bool _originalUseCustomDomain;

        #region GenerateCode

        [Fact]
        public void GenerateCode_DefaultLength_ReturnsSixCharCode()
        {
            var code = ShortUrlUtil.GenerateCode();

            Assert.Equal(6, code.Length);
        }

        [Fact]
        public void GenerateCode_CustomLength_ReturnsCorrectLength()
        {
            var code = ShortUrlUtil.GenerateCode(10);

            Assert.Equal(10, code.Length);
        }

        [Fact]
        public void GenerateCode_LengthOne_ReturnsSingleChar()
        {
            var code = ShortUrlUtil.GenerateCode(1);

            Assert.Equal(1, code.Length);
        }

        [Fact]
        public void GenerateCode_ReturnsAlphanumericChars()
        {
            var code = ShortUrlUtil.GenerateCode(100);

            foreach (var c in code)
            {
                Assert.True(
                    char.IsLetterOrDigit(c),
                    $"Character '{c}' is not alphanumeric");
            }
        }

        [Fact]
        public void GenerateCode_CalledMultipleTimes_ReturnsDifferentCodes()
        {
            var code1 = ShortUrlUtil.GenerateCode();
            var code2 = ShortUrlUtil.GenerateCode();

            // Statistically very unlikely to be equal
            Assert.NotEqual(code1, code2);
        }

        #endregion

        #region GenerateCodeFromUrl

        [Fact]
        public void GenerateCodeFromUrl_SameUrl_ReturnsSameCode()
        {
            var url = "https://example.com/very/long/path";

            var code1 = ShortUrlUtil.GenerateCodeFromUrl(url);
            var code2 = ShortUrlUtil.GenerateCodeFromUrl(url);

            Assert.Equal(code1, code2);
        }

        [Fact]
        public void GenerateCodeFromUrl_DifferentUrls_ReturnsDifferentCodes()
        {
            var code1 = ShortUrlUtil.GenerateCodeFromUrl("https://example.com/page1");
            var code2 = ShortUrlUtil.GenerateCodeFromUrl("https://example.com/page2");

            Assert.NotEqual(code1, code2);
        }

        [Fact]
        public void GenerateCodeFromUrl_DefaultLength_ReturnsSixCharCode()
        {
            var code = ShortUrlUtil.GenerateCodeFromUrl("https://example.com");

            Assert.Equal(6, code.Length);
        }

        [Fact]
        public void GenerateCodeFromUrl_CustomLength_ReturnsCorrectLength()
        {
            var code = ShortUrlUtil.GenerateCodeFromUrl("https://example.com", 10);

            Assert.Equal(10, code.Length);
        }

        [Fact]
        public void GenerateCodeFromUrl_ReturnsAlphanumericChars()
        {
            var code = ShortUrlUtil.GenerateCodeFromUrl("https://example.com/test", 50);

            foreach (var c in code)
            {
                Assert.True(char.IsLetterOrDigit(c));
            }
        }

        #endregion

        #region EncodeBase62 / DecodeBase62

        [Fact]
        public void EncodeBase62_Zero_ReturnsZeroString()
        {
            var result = ShortUrlUtil.EncodeBase62(0);

            Assert.Equal("0", result);
        }

        [Fact]
        public void EncodeBase62_One_ReturnsCorrectChar()
        {
            var result = ShortUrlUtil.EncodeBase62(1);

            Assert.Equal("1", result);
        }

        [Fact]
        public void EncodeBase62_SmallNumber_ReturnsCorrectCode()
        {
            var result = ShortUrlUtil.EncodeBase62(61);

            Assert.Equal("Z", result);
        }

        [Fact]
        public void EncodeBase62_LargerNumber_ReturnsCorrectCode()
        {
            // 62 should be "10" in base62
            var result = ShortUrlUtil.EncodeBase62(62);

            Assert.Equal("10", result);
        }

        [Fact]
        public void DecodeBase62_ZeroString_ReturnsZero()
        {
            var result = ShortUrlUtil.DecodeBase62("0");

            Assert.Equal(0L, result);
        }

        [Fact]
        public void DecodeBase62_SingleChar_ReturnsCorrectValue()
        {
            // _chars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ"
            // 'a' is at index 10 (after 0-9)
            var result = ShortUrlUtil.DecodeBase62("a");

            Assert.Equal(10L, result);
        }

        [Fact]
        public void DecodeBase62_UppercaseChar_ReturnsCorrectValue()
        {
            // 'A' is at index 36
            var result = ShortUrlUtil.DecodeBase62("A");

            Assert.Equal(36L, result);
        }

        [Fact]
        public void Encode_And_Decode_AreConsistent()
        {
            long[] testValues = { 0, 1, 10, 61, 62, 100, 999, 3844, 1000000, long.MaxValue / 1000 };

            foreach (var value in testValues)
            {
                var encoded = ShortUrlUtil.EncodeBase62(value);
                var decoded = ShortUrlUtil.DecodeBase62(encoded);

                Assert.Equal(value, decoded);
            }
        }

        [Fact]
        public void DecodeBase62_EmptyString_ReturnsZero()
        {
            var result = ShortUrlUtil.DecodeBase62("");

            Assert.Equal(0L, result);
        }

        #endregion

        #region GetFullShortUrl

        [Fact]
        public void GetFullShortUrl_WithCustomDomain_ReturnsFullUrl()
        {
            ShortUrlUtil.ShortUrlConfig.CustomDomain = "https://s.example.com";
            ShortUrlUtil.ShortUrlConfig.UseCustomDomain = true;

            var result = ShortUrlUtil.GetFullShortUrl("abc123");

            Assert.Equal("https://s.example.com/abc123", result);
        }

        [Fact]
        public void GetFullShortUrl_DomainWithTrailingSlash_TrimsSlash()
        {
            ShortUrlUtil.ShortUrlConfig.CustomDomain = "https://s.example.com/";
            ShortUrlUtil.ShortUrlConfig.UseCustomDomain = true;

            var result = ShortUrlUtil.GetFullShortUrl("abc123");

            Assert.Equal("https://s.example.com/abc123", result);
        }

        [Fact]
        public void GetFullShortUrl_UseCustomDomainFalse_ReturnsRelativePath()
        {
            ShortUrlUtil.ShortUrlConfig.UseCustomDomain = false;

            var result = ShortUrlUtil.GetFullShortUrl("abc123");

            Assert.Equal("/abc123", result);
        }

        [Fact]
        public void GetFullShortUrl_NullCustomDomain_ReturnsRelativePath()
        {
            ShortUrlUtil.ShortUrlConfig.CustomDomain = null;
            ShortUrlUtil.ShortUrlConfig.UseCustomDomain = true;

            var result = ShortUrlUtil.GetFullShortUrl("abc123");

            Assert.Equal("/abc123", result);
        }

        [Fact]
        public void GetFullShortUrl_EmptyCustomDomain_ReturnsRelativePath()
        {
            ShortUrlUtil.ShortUrlConfig.CustomDomain = "";
            ShortUrlUtil.ShortUrlConfig.UseCustomDomain = true;

            var result = ShortUrlUtil.GetFullShortUrl("abc123");

            Assert.Equal("/abc123", result);
        }

        #endregion

        #region ParseCode

        [Fact]
        public void ParseCode_ValidAbsoluteUrl_ReturnsCode()
        {
            var result = ShortUrlUtil.ParseCode("https://s.example.com/abc123");

            Assert.Equal("abc123", result);
        }

        [Fact]
        public void ParseCode_UrlWithQuery_ReturnsCodeWithoutQuery()
        {
            var result = ShortUrlUtil.ParseCode("https://s.example.com/abc123?source=twitter");

            Assert.Equal("abc123", result);
        }

        [Fact]
        public void ParseCode_UrlWithFragment_ReturnsCodeWithoutFragment()
        {
            var result = ShortUrlUtil.ParseCode("https://s.example.com/abc123#section");

            Assert.Equal("abc123", result);
        }

        [Fact]
        public void ParseCode_NullInput_ReturnsNull()
        {
            var result = ShortUrlUtil.ParseCode(null!);

            Assert.Null(result);
        }

        [Fact]
        public void ParseCode_EmptyInput_ReturnsNull()
        {
            var result = ShortUrlUtil.ParseCode("");

            Assert.Null(result);
        }

        [Fact]
        public void ParseCode_RelativePath_ReturnsCode()
        {
            var result = ShortUrlUtil.ParseCode("/abc123");

            Assert.Equal("abc123", result);
        }

        [Fact]
        public void ParseCode_NestedPath_ReturnsPath()
        {
            var result = ShortUrlUtil.ParseCode("https://s.example.com/a/b/c");

            Assert.Equal("a/b/c", result);
        }

        #endregion

        #region IsValidUrl

        [Fact]
        public void IsValidUrl_HttpsUrl_ReturnsTrue()
        {
            Assert.True(ShortUrlUtil.IsValidUrl("https://example.com"));
        }

        [Fact]
        public void IsValidUrl_HttpUrl_ReturnsTrue()
        {
            Assert.True(ShortUrlUtil.IsValidUrl("http://example.com"));
        }

        [Fact]
        public void IsValidUrl_UrlWithPathAndQuery_ReturnsTrue()
        {
            Assert.True(ShortUrlUtil.IsValidUrl("https://example.com/api?key=value"));
        }

        [Fact]
        public void IsValidUrl_FtpUrl_ReturnsFalse()
        {
            Assert.False(ShortUrlUtil.IsValidUrl("ftp://example.com"));
        }

        [Fact]
        public void IsValidUrl_NoScheme_ReturnsFalse()
        {
            Assert.False(ShortUrlUtil.IsValidUrl("example.com"));
        }

        [Fact]
        public void IsValidUrl_EmptyString_ReturnsFalse()
        {
            Assert.False(ShortUrlUtil.IsValidUrl(""));
        }

        [Fact]
        public void IsValidUrl_RelativePath_ReturnsFalse()
        {
            Assert.False(ShortUrlUtil.IsValidUrl("/api/users"));
        }

        #endregion

        #region NormalizeUrl

        [Fact]
        public void NormalizeUrl_HttpsUrl_ReturnsUnchanged()
        {
            var result = ShortUrlUtil.NormalizeUrl("https://example.com");

            Assert.Equal("https://example.com", result);
        }

        [Fact]
        public void NormalizeUrl_HttpUrl_ReturnsUnchanged()
        {
            var result = ShortUrlUtil.NormalizeUrl("http://example.com");

            Assert.Equal("http://example.com", result);
        }

        [Fact]
        public void NormalizeUrl_UrlWithoutScheme_PrependsHttps()
        {
            var result = ShortUrlUtil.NormalizeUrl("example.com");

            Assert.Equal("https://example.com", result);
        }

        [Fact]
        public void NormalizeUrl_UrlWithWwwWithoutScheme_PrependsHttps()
        {
            var result = ShortUrlUtil.NormalizeUrl("www.example.com");

            Assert.Equal("https://www.example.com", result);
        }

        [Fact]
        public void NormalizeUrl_EmptyString_ReturnsEmpty()
        {
            var result = ShortUrlUtil.NormalizeUrl("");

            Assert.Equal("", result);
        }

        [Fact]
        public void NormalizeUrl_NullString_ReturnsNull()
        {
            var result = ShortUrlUtil.NormalizeUrl(null!);

            Assert.Null(result);
        }

        [Fact]
        public void NormalizeUrl_HttpUpperCase_PrependsHttps()
        {
            // The method uses OrdinalIgnoreCase for http/https check
            var result = ShortUrlUtil.NormalizeUrl("HTTP://example.com");

            Assert.Equal("HTTP://example.com", result);
        }

        [Fact]
        public void NormalizeUrl_HttpsUpperCase_ReturnsUnchanged()
        {
            var result = ShortUrlUtil.NormalizeUrl("HTTPS://example.com");

            Assert.Equal("HTTPS://example.com", result);
        }

        #endregion
    }
}
