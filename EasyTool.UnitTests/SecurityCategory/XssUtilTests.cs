using Xunit;

namespace EasyTool.SecurityCategory.Tests
{
    public class XssUtilTests
    {
        [Fact]
        public void HtmlEncode_EncodesSpecialCharacters()
        {
            var input = "<script>alert('xss')</script>";
            var result = XssUtil.HtmlEncode(input);
            // 根据实际编码映射：' -> &#x27;, / -> &#x2F;
            Assert.Contains("&lt;", result);
            Assert.Contains("&gt;", result);
            Assert.Contains("&#x27;", result);
        }

        [Fact]
        public void HtmlEncode_EncodesAmpersand()
        {
            var input = "Tom & Jerry";
            var result = XssUtil.HtmlEncode(input);
            Assert.Equal("Tom &amp; Jerry", result);
        }

        [Fact]
        public void HtmlEncode_EncodesQuotes()
        {
            var input = "He said \"Hello\"";
            var result = XssUtil.HtmlEncode(input);
            Assert.Equal("He said &quot;Hello&quot;", result);
        }

        [Fact]
        public void HtmlEncode_NullInput_ReturnsNull()
        {
            string? input = null;
            var result = XssUtil.HtmlEncode(input!);
            Assert.Null(result);
        }

        [Fact]
        public void HtmlEncode_EmptyInput_ReturnsEmpty()
        {
            var result = XssUtil.HtmlEncode("");
            Assert.Equal("", result);
        }

        [Fact]
        public void HtmlDecode_DecodesEncodedString()
        {
            var input = "&lt;div&gt;Hello&lt;/div&gt;";
            var result = XssUtil.HtmlDecode(input);
            Assert.Equal("<div>Hello</div>", result);
        }

        [Fact]
        public void StripHtml_RemovesAllTags()
        {
            var input = "<script>alert('xss')</script>Hello World";
            var result = XssUtil.StripHtml(input);
            // StripHtml 只移除标签，保留标签内的文本内容
            Assert.Contains("alert", result);
            Assert.Contains("Hello World", result);
            Assert.DoesNotContain("<script>", result);
            Assert.DoesNotContain("</script>", result);
        }

        [Fact]
        public void Sanitize_RemovesDangerousContent()
        {
            var input = "<script>alert('xss')</script><iframe src=\"evil.com\"></iframe><p>Safe content</p>";
            var result = XssUtil.Sanitize(input);
            Assert.DoesNotContain("script", result);
            Assert.DoesNotContain("iframe", result);
            Assert.Contains("Safe content", result);
        }

        [Fact]
        public void ContainsXss_DetectsScriptTag()
        {
            var input = "<script>alert('xss')</script>";
            Assert.True(XssUtil.ContainsXss(input));
        }

        [Fact]
        public void ContainsXss_DetectsEventHandlers()
        {
            var input = "<img src=\"x\" onerror=\"alert('xss')\">";
            Assert.True(XssUtil.ContainsXss(input));
        }

        [Fact]
        public void ContainsXss_DetectsJavaScriptProtocol()
        {
            var input = "<a href=\"javascript:alert('xss')\">Click</a>";
            Assert.True(XssUtil.ContainsXss(input));
        }

        [Fact]
        public void ContainsXss_DetectsIframe()
        {
            var input = "<iframe src=\"evil.com\"></iframe>";
            Assert.True(XssUtil.ContainsXss(input));
        }

        [Fact]
        public void ContainsXss_DetectsObject()
        {
            var input = "<object data=\"evil.swf\"></object>";
            Assert.True(XssUtil.ContainsXss(input));
        }

        [Fact]
        public void ContainsXss_SafeInput_ReturnsFalse()
        {
            var input = "<p>Hello World</p>";
            Assert.False(XssUtil.ContainsXss(input));
        }

        [Fact]
        public void ContainsXss_EmptyInput_ReturnsFalse()
        {
            Assert.False(XssUtil.ContainsXss(""));
            Assert.False(XssUtil.ContainsXss(null!));
        }

        [Fact]
        public void CleanHtml_PreservesAllowedTags()
        {
            var input = "<p>Hello <strong>World</strong></p><script>alert('xss')</script>";
            var result = XssUtil.CleanHtml(input);
            Assert.Contains("<p>", result);
            Assert.Contains("<strong>", result);
            Assert.DoesNotContain("script", result);
        }

        [Fact]
        public void EscapeAttribute_EscapesDangerousChars()
        {
            var input = "<div class=\"test\">";
            var result = XssUtil.EscapeAttribute(input);
            Assert.Contains("&lt;", result);
            Assert.Contains("&gt;", result);
            Assert.Contains("&quot;", result);
        }

        [Fact]
        public void SafeUrlEncode_ReturnsEncodedUrl()
        {
            var input = "https://example.com/search?q=test";
            var result = XssUtil.SafeUrlEncode(input);
            Assert.NotNull(result);
        }

        [Fact]
        public void SafeUrlEncode_DangerousProtocol_ReturnsEmpty()
        {
            var input = "javascript:alert('xss')";
            var result = XssUtil.SafeUrlEncode(input);
            Assert.Equal("", result);
        }

        [Fact]
        public void IsUrlSafe_SafeUrl_ReturnsTrue()
        {
            var input = "https://example.com";
            Assert.True(XssUtil.IsUrlSafe(input));
        }

        [Fact]
        public void IsUrlSafe_DangerousProtocol_ReturnsFalse()
        {
            var input = "javascript:alert('xss')";
            Assert.False(XssUtil.IsUrlSafe(input));
        }

        [Fact]
        public void SafeJsonString_EscapesSpecialChars()
        {
            var input = "Hello\nWorld\"Test";
            var result = XssUtil.SafeJsonString(input);
            Assert.Contains("\\n", result);
            Assert.Contains("\\\"", result);
        }

        [Fact]
        public void CleanCss_RemovesExpression()
        {
            var input = "width: expression(alert('xss')); color: red;";
            var result = XssUtil.CleanCss(input);
            Assert.DoesNotContain("expression", result);
        }

        [Fact]
        public void CleanCss_RemovesUrl()
        {
            var input = "background: url(evil.png); color: blue;";
            var result = XssUtil.CleanCss(input);
            Assert.DoesNotContain("url", result);
        }
    }
}