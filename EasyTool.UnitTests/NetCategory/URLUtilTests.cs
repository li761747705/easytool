using System;
using System.Collections.Generic;
using Xunit;
using EasyTool.NetCategory;

namespace EasyTool.Tests
{
    /// <summary>
    /// URLUtil 工具类的单元测试
    /// </summary>
    public class URLUtilTests
    {
        #region ParseUrl

        [Fact]
        public void ParseUrl_ValidUrl_ReturnsCorrectParts()
        {
            var result = URLUtil.ParseUrl("https://www.example.com:8080/path/to/page");

            Assert.Equal(4, result.Length);
            Assert.Equal("https", result[0]);
            Assert.Equal("www.example.com", result[1]);
            Assert.Equal("8080", result[2]);
            Assert.Equal("/path/to/page", result[3]);
        }

        [Fact]
        public void ParseUrl_HttpUrl_ReturnsHttpScheme()
        {
            var result = URLUtil.ParseUrl("http://localhost:3000/api");

            Assert.Equal("http", result[0]);
            Assert.Equal("localhost", result[1]);
            Assert.Equal("3000", result[2]);
            Assert.Equal("/api", result[3]);
        }

        [Fact]
        public void ParseUrl_DefaultPort_ReturnsPort80()
        {
            var result = URLUtil.ParseUrl("http://example.com/path");

            Assert.Equal("80", result[2]);
        }

        [Fact]
        public void ParseUrl_HttpsDefaultPort_ReturnsPort443()
        {
            var result = URLUtil.ParseUrl("https://example.com/path");

            Assert.Equal("443", result[2]);
        }

        [Fact]
        public void ParseUrl_InvalidUrl_ThrowsUriFormatException()
        {
            Assert.Throws<UriFormatException>(() => URLUtil.ParseUrl("not_a_valid_url"));
        }

        [Fact]
        public void ParseUrl_EmptyUrl_ThrowsUriFormatException()
        {
            Assert.Throws<UriFormatException>(() => URLUtil.ParseUrl(""));
        }

        #endregion

        #region AddQueryParameters

        [Fact]
        public void AddQueryParameters_UrlWithoutQuery_AddsParameter()
        {
            var result = URLUtil.AddQueryParameters(
                "https://example.com/page",
                new KeyValuePair<string, string>("key", "value"));

            Assert.Contains("key=value", result);
            // UriBuilder normalizes the URL, adding default port 443 for https
            Assert.Contains("example.com", result);
            Assert.Contains("/page", result);
        }

        [Fact]
        public void AddQueryParameters_UrlWithExistingQuery_AppendsParameter()
        {
            var result = URLUtil.AddQueryParameters(
                "https://example.com/page?existing=1",
                new KeyValuePair<string, string>("new", "param"));

            Assert.Contains("existing=1", result);
            Assert.Contains("new=param", result);
        }

        [Fact]
        public void AddQueryParameters_MultipleParameters_AddsAll()
        {
            var result = URLUtil.AddQueryParameters(
                "https://example.com/page",
                new KeyValuePair<string, string>("a", "1"),
                new KeyValuePair<string, string>("b", "2"),
                new KeyValuePair<string, string>("c", "3"));

            Assert.Contains("a=1", result);
            Assert.Contains("b=2", result);
            Assert.Contains("c=3", result);
        }

        [Fact]
        public void AddQueryParameters_SpecialCharacters_EncodesValues()
        {
            var result = URLUtil.AddQueryParameters(
                "https://example.com/page",
                new KeyValuePair<string, string>("q", "hello world"));

            Assert.Contains("q=hello+world", result);
        }

        [Fact]
        public void AddQueryParameters_DuplicateKey_OverwritesValue()
        {
            var result = URLUtil.AddQueryParameters(
                "https://example.com/page?key=old",
                new KeyValuePair<string, string>("key", "new"));

            Assert.Contains("key=new", result);
        }

        #endregion

        #region RemoveQueryParameters

        [Fact]
        public void RemoveQueryParameters_ExistingParameter_RemovesParameter()
        {
            var result = URLUtil.RemoveQueryParameters(
                "https://example.com/page?key1=value1&key2=value2",
                "key1");

            Assert.DoesNotContain("key1=value1", result);
            Assert.Contains("key2=value2", result);
        }

        [Fact]
        public void RemoveQueryParameters_NonExistingParameter_KeepsUrlUnchanged()
        {
            var original = "https://example.com/page?key=value";
            var result = URLUtil.RemoveQueryParameters(original, "nonexistent");

            Assert.Contains("key=value", result);
        }

        [Fact]
        public void RemoveQueryParameters_MultipleParameters_RemovesAll()
        {
            var result = URLUtil.RemoveQueryParameters(
                "https://example.com/page?a=1&b=2&c=3",
                "a", "c");

            Assert.DoesNotContain("a=1", result);
            Assert.Contains("b=2", result);
            Assert.DoesNotContain("c=3", result);
        }

        [Fact]
        public void RemoveQueryParameters_AllParameters_ReturnsCleanUrl()
        {
            var result = URLUtil.RemoveQueryParameters(
                "https://example.com/page?only=param",
                "only");

            Assert.DoesNotContain("only=param", result);
        }

        #endregion

        #region CombineUrls

        [Fact]
        public void CombineUrls_BothAbsolute_ReturnsRelativeUrl()
        {
            var result = URLUtil.CombineUrls("https://example.com/api", "https://other.com/page");

            Assert.Equal("https://other.com/page", result);
        }

        [Fact]
        public void CombineUrls_NullBaseUrl_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => URLUtil.CombineUrls(null!, "/path"));
        }

        [Fact]
        public void CombineUrls_EmptyBaseUrl_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => URLUtil.CombineUrls("", "/path"));
        }

        [Fact]
        public void CombineUrls_NullRelativeUrl_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => URLUtil.CombineUrls("https://example.com", null!));
        }

        [Fact]
        public void CombineUrls_EmptyRelativeUrl_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => URLUtil.CombineUrls("https://example.com", ""));
        }

        [Fact]
        public void CombineUrls_NonAbsoluteBase_ThrowsException()
        {
            // Non-absolute base URL throws either ArgumentException or UriFormatException
            Assert.ThrowsAny<Exception>(() => URLUtil.CombineUrls("not-absolute", "/path"));
        }

        #endregion

        #region UrlEncode / UrlDecode

        [Fact]
        public void UrlEncode_SpecialCharacters_EncodesCorrectly()
        {
            var result = URLUtil.UrlEncode("hello world&test=1");

            Assert.Contains("hello+world", result);
            Assert.Contains("test%3d1", result);
        }

        [Fact]
        public void UrlEncode_EmptyString_ReturnsEmpty()
        {
            var result = URLUtil.UrlEncode("");
            Assert.Equal("", result);
        }

        [Fact]
        public void UrlEncode_NoSpecialCharacters_ReturnsUnchanged()
        {
            var result = URLUtil.UrlEncode("hello");
            Assert.Equal("hello", result);
        }

        [Fact]
        public void UrlDecode_EncodedString_DecodesCorrectly()
        {
            var result = URLUtil.UrlDecode("hello+world%3dtest");

            Assert.Equal("hello world=test", result);
        }

        [Fact]
        public void UrlDecode_EmptyString_ReturnsEmpty()
        {
            var result = URLUtil.UrlDecode("");
            Assert.Equal("", result);
        }

        [Fact]
        public void UrlEncode_And_UrlDecode_AreConsistent()
        {
            var original = "test value with spaces & special=chars?yes";
            var encoded = URLUtil.UrlEncode(original);
            var decoded = URLUtil.UrlDecode(encoded);

            Assert.Equal(original, decoded);
        }

        #endregion

        #region UrlEncodeQuery / UrlDecodeQuery

        [Fact]
        public void UrlEncodeQuery_ChineseCharacters_EncodesCorrectly()
        {
            var result = URLUtil.UrlEncodeQuery("中文测试");

            Assert.NotEmpty(result);
            Assert.NotEqual("中文测试", result);
        }

        [Fact]
        public void UrlDecodeQuery_EncodedChinese_DecodesCorrectly()
        {
            var encoded = URLUtil.UrlEncodeQuery("中文测试");
            var result = URLUtil.UrlDecodeQuery(encoded);

            Assert.Equal("中文测试", result);
        }

        [Fact]
        public void UrlEncodeQuery_And_UrlDecodeQuery_AreConsistent()
        {
            var original = "key=value&参数=值";
            var encoded = URLUtil.UrlEncodeQuery(original);
            var decoded = URLUtil.UrlDecodeQuery(encoded);

            Assert.Equal(original, decoded);
        }

        #endregion

        #region ExtractDomain

        [Fact]
        public void ExtractDomain_ValidUrl_ReturnsDomain()
        {
            var result = URLUtil.ExtractDomain("https://www.example.com/path?query=1");

            Assert.Equal("www.example.com", result);
        }

        [Fact]
        public void ExtractDomain_UrlWithPort_ReturnsDomainWithoutPort()
        {
            var result = URLUtil.ExtractDomain("https://example.com:8080/api");

            Assert.Equal("example.com", result);
        }

        [Fact]
        public void ExtractDomain_HttpUrl_ReturnsDomain()
        {
            var result = URLUtil.ExtractDomain("http://localhost:3000/api");

            Assert.Equal("localhost", result);
        }

        #endregion

        #region ExtractPath

        [Fact]
        public void ExtractPath_ValidUrl_ReturnsPath()
        {
            var result = URLUtil.ExtractPath("https://example.com/api/users?id=1");

            Assert.Equal("/api/users", result);
        }

        [Fact]
        public void ExtractPath_RootPath_ReturnsSlash()
        {
            var result = URLUtil.ExtractPath("https://example.com");

            Assert.Equal("/", result);
        }

        [Fact]
        public void ExtractPath_NestedPath_ReturnsFullPath()
        {
            var result = URLUtil.ExtractPath("https://example.com/a/b/c/d");

            Assert.Equal("/a/b/c/d", result);
        }

        #endregion

        #region IsHttps

        [Fact]
        public void IsHttps_HttpsUrl_ReturnsTrue()
        {
            Assert.True(URLUtil.IsHttps("https://example.com"));
        }

        [Fact]
        public void IsHttps_HttpUrl_ReturnsFalse()
        {
            Assert.False(URLUtil.IsHttps("http://example.com"));
        }

        [Fact]
        public void IsHttps_HttpsWithPort_ReturnsTrue()
        {
            Assert.True(URLUtil.IsHttps("https://example.com:8443/path"));
        }

        #endregion

        #region ExtractQueryString

        [Fact]
        public void ExtractQueryString_UrlWithQuery_ReturnsQueryString()
        {
            var result = URLUtil.ExtractQueryString("https://example.com/path?key=value&other=123");

            Assert.Contains("key=value", result);
            Assert.Contains("other=123", result);
            Assert.StartsWith("?", result);
        }

        [Fact]
        public void ExtractQueryString_UrlWithoutQuery_ReturnsEmptyString()
        {
            var result = URLUtil.ExtractQueryString("https://example.com/path");

            Assert.Equal("", result);
        }

        #endregion

        #region ExtractFragment

        [Fact]
        public void ExtractFragment_UrlWithFragment_ReturnsFragment()
        {
            var result = URLUtil.ExtractFragment("https://example.com/page#section1");

            Assert.Equal("#section1", result);
        }

        [Fact]
        public void ExtractFragment_UrlWithoutFragment_ReturnsEmpty()
        {
            var result = URLUtil.ExtractFragment("https://example.com/page");

            Assert.Equal("", result);
        }

        #endregion

        #region PathToRelative

        [Fact]
        public void PathToRelative_ValidUrl_ReturnsRelativePath()
        {
            var result = URLUtil.PathToRelative("https://example.com/api/users");

            Assert.Equal("api/users", result);
        }

        [Fact]
        public void PathToRelative_RootPath_ReturnsEmptyString()
        {
            var result = URLUtil.PathToRelative("https://example.com/");

            Assert.Equal("", result);
        }

        [Fact]
        public void PathToRelative_DeepPath_ReturnsFullRelativePath()
        {
            var result = URLUtil.PathToRelative("https://example.com/a/b/c/d/e");

            Assert.Equal("a/b/c/d/e", result);
        }

        #endregion

        #region RelativeToPath

        [Fact]
        public void RelativeToPath_ValidRelativePath_ReturnsAbsoluteUrl()
        {
            var result = URLUtil.RelativeToPath("/api/users", "https://example.com");

            Assert.Equal("https://example.com/api/users", result);
        }

        [Fact]
        public void RelativeToPath_RelativePathWithoutLeadingSlash_ReturnsAbsoluteUrl()
        {
            var result = URLUtil.RelativeToPath("api/users", "https://example.com");

            Assert.Equal("https://example.com/api/users", result);
        }

        [Fact]
        public void RelativeToPath_WithBasePath_ReturnsCorrectUrl()
        {
            var result = URLUtil.RelativeToPath("users", "https://example.com/api/v1/");

            Assert.Contains("users", result);
            Assert.Contains("example.com", result);
        }

        #endregion

        #region QueryToDictionary

        [Fact]
        public void QueryToDictionary_ValidQuery_ReturnsDictionary()
        {
            var result = URLUtil.QueryToDictionary("https://example.com?key1=value1&key2=value2");

            Assert.Equal(2, result.Count);
            Assert.Equal("value1", result["key1"]);
            Assert.Equal("value2", result["key2"]);
        }

        [Fact]
        public void QueryToDictionary_NoQuery_ReturnsEmptyDictionary()
        {
            var result = URLUtil.QueryToDictionary("https://example.com/path");

            Assert.Empty(result);
        }

        [Fact]
        public void QueryToDictionary_SingleParameter_ReturnsSingleEntry()
        {
            var result = URLUtil.QueryToDictionary("https://example.com?search=test");

            Assert.Single(result);
            Assert.Equal("test", result["search"]);
        }

        [Fact]
        public void QueryToDictionary_EncodedValues_ReturnsDecodedValues()
        {
            var result = URLUtil.QueryToDictionary("https://example.com?name=hello+world");

            Assert.Equal("hello world", result["name"]);
        }

        #endregion
    }
}
