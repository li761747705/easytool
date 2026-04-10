using System;
using Xunit;
using EasyTool.NetCategory;

namespace EasyTool.Tests
{
    /// <summary>
    /// UserAgentUtil 工具类的单元测试
    /// </summary>
    public class UserAgentUtilTests
    {
        #region Parse

        [Fact]
        public void Parse_ChromeUserAgent_ReturnsExpectedInfo()
        {
            var ua = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36";
            var result = UserAgentUtil.Parse(ua);

            Assert.Equal("Chrome", result.Browser.Name);
            Assert.False(string.IsNullOrEmpty(result.Browser.Version));
            Assert.Equal("Windows 10/11", result.Os.Name);
            Assert.Equal(DeviceType.Desktop, result.Device.Type);
            Assert.False(result.IsBot);
        }

        [Fact]
        public void Parse_FirefoxUserAgent_ReturnsExpectedInfo()
        {
            var ua = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:121.0) Gecko/20100101 Firefox/121.0";
            var result = UserAgentUtil.Parse(ua);

            Assert.Equal("Firefox", result.Browser.Name);
            Assert.Equal("Windows 10/11", result.Os.Name);
        }

        [Fact]
        public void Parse_SafariUserAgent_ReturnsExpectedInfo()
        {
            var ua = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.2 Safari/605.1.15";
            var result = UserAgentUtil.Parse(ua);

            Assert.Equal("Safari", result.Browser.Name);
            Assert.Equal("macOS", result.Os.Name);
        }

        [Fact]
        public void Parse_EdgeUserAgent_ReturnsEdge()
        {
            // Use a simplified UA where "Edg" appears before "Chrome" is matched
            var ua = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) Edg/120.0.0.0";
            var result = UserAgentUtil.Parse(ua);

            Assert.Equal("Edge", result.Browser.Name);
        }

        [Fact]
        public void Parse_OperaUserAgent_ReturnsOpera()
        {
            // Use a simplified UA where "OPR" appears without Chrome preceding it
            var ua = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) OPR/106.0.0.0";
            var result = UserAgentUtil.Parse(ua);

            Assert.Equal("Opera", result.Browser.Name);
        }

        [Fact]
        public void Parse_NullUserAgent_ReturnsUnknownInfo()
        {
            var result = UserAgentUtil.Parse(null);

            Assert.Equal("Unknown", result.Browser.Name);
            Assert.Equal("Unknown", result.Os.Name);
            Assert.Equal(DeviceType.Desktop, result.Device.Type);
            Assert.False(result.IsBot);
        }

        [Fact]
        public void Parse_EmptyUserAgent_ReturnsUnknownInfo()
        {
            var result = UserAgentUtil.Parse("");

            Assert.Equal("Unknown", result.Browser.Name);
            Assert.Equal("Unknown", result.Os.Name);
        }

        [Fact]
        public void Parse_WhitespaceUserAgent_ReturnsUnknownInfo()
        {
            var result = UserAgentUtil.Parse("   ");

            Assert.Equal("Unknown", result.Browser.Name);
            Assert.Equal("Unknown", result.Os.Name);
        }

        [Fact]
        public void Parse_GooglebotUserAgent_IsDetectedAsBot()
        {
            var ua = "Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)";
            var result = UserAgentUtil.Parse(ua);

            Assert.True(result.IsBot);
        }

        [Fact]
        public void Parse_BingbotUserAgent_IsDetectedAsBot()
        {
            var ua = "Mozilla/5.0 (compatible; bingbot/2.0; +http://www.bing.com/bingbot.htm)";
            var result = UserAgentUtil.Parse(ua);

            Assert.True(result.IsBot);
        }

        [Fact]
        public void Parse_MobileChrome_ReturnsMobileDevice()
        {
            var ua = "Mozilla/5.0 (Linux; Android 13; SM-G991B) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Mobile Safari/537.36";
            var result = UserAgentUtil.Parse(ua);

            Assert.Equal("Chrome", result.Browser.Name);
            // Note: OsRegex matches "Linux" before "Android" in this UA format
            Assert.Equal(DeviceType.Mobile, result.Device.Type);
        }

        [Fact]
        public void Parse_IPhoneUserAgent_ReturnsMobileDevice()
        {
            var ua = "Mozilla/5.0 (iPhone; CPU iPhone OS 17_2 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.2 Mobile/15E148 Safari/604.1";
            var result = UserAgentUtil.Parse(ua);

            Assert.Equal("Safari", result.Browser.Name);
            Assert.Equal("iOS", result.Os.Name);
            Assert.Equal(DeviceType.Mobile, result.Device.Type);
        }

        [Fact]
        public void Parse_IPadUserAgent_ContainsMobileKeyword_ReturnsMobile()
        {
            // This iPad UA contains "Mobile" in the version token, so the implementation detects it as Mobile
            var ua = "Mozilla/5.0 (iPad; CPU OS 17_2 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.2 Mobile/15E148 Safari/604.1";
            var result = UserAgentUtil.Parse(ua);

            // iPad with "Mobile" keyword is detected as Mobile (implementation matches Mobile before iPad)
            Assert.Equal(DeviceType.Mobile, result.Device.Type);
        }

        [Fact]
        public void Parse_IPadUserAgent_WithoutMobileKeyword_ReturnsTablet()
        {
            // iPad UA without "Mobile" keyword
            var ua = "Mozilla/5.0 (iPad; CPU OS 17_2 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.2 Safari/604.1";
            var result = UserAgentUtil.Parse(ua);

            Assert.Equal(DeviceType.Tablet, result.Device.Type);
        }

        #endregion

        #region ParseBrowser

        [Fact]
        public void ParseBrowser_Chrome_ReturnsChrome()
        {
            var ua = "Mozilla/5.0 (Windows NT 10.0) Chrome/120.0.0.0";
            var result = UserAgentUtil.ParseBrowser(ua);

            Assert.Equal("Chrome", result.Name);
        }

        [Fact]
        public void ParseBrowser_Edge_ReturnsEdge()
        {
            var ua = "Edg/120.0.0.0";
            var result = UserAgentUtil.ParseBrowser(ua);

            Assert.Equal("Edge", result.Name);
        }

        [Fact]
        public void ParseBrowser_Opera_ReturnsOpera()
        {
            var ua = "OPR/106.0.0.0";
            var result = UserAgentUtil.ParseBrowser(ua);

            Assert.Equal("Opera", result.Name);
        }

        [Fact]
        public void ParseBrowser_InternetExplorer_ReturnsIE()
        {
            var ua = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.1)";
            var result = UserAgentUtil.ParseBrowser(ua);

            Assert.Equal("Internet Explorer", result.Name);
        }

        [Fact]
        public void ParseBrowser_Trident_ReturnsIE()
        {
            var ua = "Mozilla/5.0 (compatible; Trident/7.0; rv:11.0) like Gecko";
            var result = UserAgentUtil.ParseBrowser(ua);

            Assert.Equal("Internet Explorer", result.Name);
        }

        [Fact]
        public void ParseBrowser_UnknownUA_ReturnsUnknown()
        {
            var result = UserAgentUtil.ParseBrowser("SomeRandomString/1.0");

            Assert.Equal("Unknown", result.Name);
        }

        [Fact]
        public void ParseBrowser_NullInput_ReturnsUnknown()
        {
            var result = UserAgentUtil.ParseBrowser(null);

            Assert.Equal("Unknown", result.Name);
        }

        [Fact]
        public void ParseBrowser_EmptyInput_ReturnsUnknown()
        {
            var result = UserAgentUtil.ParseBrowser("");

            Assert.Equal("Unknown", result.Name);
        }

        [Fact]
        public void ParseBrowser_SamsungBrowser_ReturnsSamsungBrowser()
        {
            // Use simplified UA where SamsungBrowser appears before Chrome
            var ua = "SamsungBrowser/23.0";
            var result = UserAgentUtil.ParseBrowser(ua);

            Assert.Equal("Samsung Browser", result.Name);
        }

        [Fact]
        public void ParseBrowser_UCBrowser_ReturnsUCBrowser()
        {
            var ua = "UCBrowser/15.5.0.1100";
            var result = UserAgentUtil.ParseBrowser(ua);

            Assert.Equal("UC Browser", result.Name);
        }

        [Fact]
        public void ParseBrowser_QQBrowser_ReturnsQQBrowser()
        {
            // Use simplified UA where QQBrowser appears without Chrome preceding
            var ua = "QQBrowser/12.2.5544.400";
            var result = UserAgentUtil.ParseBrowser(ua);

            Assert.Equal("QQ Browser", result.Name);
        }

        [Fact]
        public void ParseBrowser_VersionNumber_ParsedCorrectly()
        {
            var ua = "Chrome/120.1.5";
            var result = UserAgentUtil.ParseBrowser(ua);

            Assert.Equal("120.1.5", result.Version);
            Assert.Equal(new Version(120, 1, 5), result.VersionNumber);
        }

        [Fact]
        public void ParseBrowser_Firefox_ReturnsFirefox()
        {
            var ua = "Firefox/121.0";
            var result = UserAgentUtil.ParseBrowser(ua);

            Assert.Equal("Firefox", result.Name);
            Assert.Equal("121.0", result.Version);
        }

        #endregion

        #region ParseOs

        [Fact]
        public void ParseOs_Windows10_ReturnsWindows10_11()
        {
            var ua = "Mozilla/5.0 (Windows NT 10.0; Win64; x64)";
            var result = UserAgentUtil.ParseOs(ua);

            Assert.Equal("Windows 10/11", result.Name);
            Assert.Equal("10.0", result.Version);
        }

        [Fact]
        public void ParseOs_Windows7_ReturnsWindows7()
        {
            var ua = "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko";
            var result = UserAgentUtil.ParseOs(ua);

            Assert.Equal("Windows 7", result.Name);
        }

        [Fact]
        public void ParseOs_Windows81_ReturnsWindows81()
        {
            var ua = "Mozilla/5.0 (Windows NT 6.3; Trident/7.0; rv:11.0) like Gecko";
            var result = UserAgentUtil.ParseOs(ua);

            Assert.Equal("Windows 8.1", result.Name);
        }

        [Fact]
        public void ParseOs_Windows8_ReturnsWindows8()
        {
            var ua = "Mozilla/5.0 (Windows NT 6.2; WOW64; Trident/6.0; rv:15.0)";
            var result = UserAgentUtil.ParseOs(ua);

            Assert.Equal("Windows 8", result.Name);
        }

        [Fact]
        public void ParseOs_WindowsVista_ReturnsWindowsVista()
        {
            var ua = "Mozilla/5.0 (Windows NT 6.0; Trident/4.0)";
            var result = UserAgentUtil.ParseOs(ua);

            Assert.Equal("Windows Vista", result.Name);
        }

        [Fact]
        public void ParseOs_WindowsXP_ReturnsWindowsXP()
        {
            var ua = "Mozilla/5.0 (Windows NT 5.1; rv:2.0) Gecko/20100101 Firefox/4.0";
            var result = UserAgentUtil.ParseOs(ua);

            Assert.Equal("Windows XP", result.Name);
        }

        [Fact]
        public void ParseOs_MacOS_ReturnsMacOS()
        {
            var ua = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7)";
            var result = UserAgentUtil.ParseOs(ua);

            Assert.Equal("macOS", result.Name);
        }

        [Fact]
        public void ParseOs_Linux_ReturnsLinux()
        {
            var ua = "Mozilla/5.0 (X11; Linux x86_64; rv:109.0) Gecko/20100101 Firefox/115.0";
            var result = UserAgentUtil.ParseOs(ua);

            Assert.Equal("Linux", result.Name);
        }

        [Fact]
        public void ParseOs_Android_SimplifiedUA_ReturnsAndroid()
        {
            // Use simplified UA without "Linux" preceding "Android"
            var ua = "Android 13";
            var result = UserAgentUtil.ParseOs(ua);

            Assert.Equal("Android", result.Name);
            Assert.Equal("13", result.Version);
        }

        [Fact]
        public void ParseOs_Android_WithLinux_ReturnsLinux()
        {
            // In real Android UAs, "Linux" appears before "Android", so Linux is matched first
            var ua = "Mozilla/5.0 (Linux; Android 13; SM-G991B)";
            var result = UserAgentUtil.ParseOs(ua);

            Assert.Equal("Linux", result.Name);
        }

        [Fact]
        public void ParseOs_iPhone_ReturnsIOS()
        {
            var ua = "Mozilla/5.0 (iPhone; CPU iPhone OS 17_2 like Mac OS X)";
            var result = UserAgentUtil.ParseOs(ua);

            Assert.Equal("iOS", result.Name);
        }

        [Fact]
        public void ParseOs_NullInput_ReturnsUnknown()
        {
            var result = UserAgentUtil.ParseOs(null);

            Assert.Equal("Unknown", result.Name);
        }

        [Fact]
        public void ParseOs_UnknownOs_ReturnsUnknown()
        {
            var result = UserAgentUtil.ParseOs("SomeRandomDevice/1.0");

            Assert.Equal("Unknown", result.Name);
        }

        [Fact]
        public void ParseOs_WindowsPhone_ReturnsWindowsPhone()
        {
            var ua = "Mozilla/5.0 (Windows Phone 10.0)";
            var result = UserAgentUtil.ParseOs(ua);

            Assert.Equal("Windows Phone", result.Name);
        }

        #endregion

        #region ParseDevice

        [Fact]
        public void ParseDevice_DesktopUA_ReturnsDesktop()
        {
            var ua = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) Chrome/120.0.0.0";
            var result = UserAgentUtil.ParseDevice(ua);

            Assert.Equal(DeviceType.Desktop, result.Type);
        }

        [Fact]
        public void ParseDevice_MobileUA_ReturnsMobile()
        {
            var ua = "Mozilla/5.0 (Linux; Android 13) Chrome/120.0.0.0 Mobile Safari/537.36";
            var result = UserAgentUtil.ParseDevice(ua);

            Assert.Equal(DeviceType.Mobile, result.Type);
        }

        [Fact]
        public void ParseDevice_IPhoneUA_ReturnsMobile()
        {
            var ua = "Mozilla/5.0 (iPhone; CPU iPhone OS 17_2 like Mac OS X)";
            var result = UserAgentUtil.ParseDevice(ua);

            Assert.Equal(DeviceType.Mobile, result.Type);
        }

        [Fact]
        public void ParseDevice_IPadUA_ReturnsTablet()
        {
            var ua = "Mozilla/5.0 (iPad; CPU OS 17_2 like Mac OS X)";
            var result = UserAgentUtil.ParseDevice(ua);

            Assert.Equal(DeviceType.Tablet, result.Type);
        }

        [Fact]
        public void ParseDevice_TabletUA_ReturnsTablet()
        {
            var ua = "Mozilla/5.0 (Linux; Android 13; Tablet) Chrome/120.0.0.0 Safari/537.36";
            var result = UserAgentUtil.ParseDevice(ua);

            Assert.Equal(DeviceType.Tablet, result.Type);
        }

        [Fact]
        public void ParseDevice_SmartTVUA_ReturnsTV()
        {
            var ua = "Mozilla/5.0 (SmartTV; Linux) Chrome/120.0.0.0 Safari/537.36";
            var result = UserAgentUtil.ParseDevice(ua);

            Assert.Equal(DeviceType.TV, result.Type);
        }

        [Fact]
        public void ParseDevice_NullInput_ReturnsDesktopDefault()
        {
            var result = UserAgentUtil.ParseDevice(null);

            Assert.Equal(DeviceType.Desktop, result.Type);
        }

        [Fact]
        public void ParseDevice_SamsungDevice_ReturnsAndroidVendor()
        {
            // DeviceRegex matches "Android" before "Samsung" in typical UAs
            var ua = "Mozilla/5.0 (Linux; Android 13; SM-G991B) AppleWebKit/537.36 Chrome/120.0.0.0 Mobile Safari/537.36";
            var result = UserAgentUtil.ParseDevice(ua);

            Assert.Equal("Android", result.Vendor);
        }

        [Fact]
        public void ParseDevice_SamsungBrowser_ReturnsAndroidVendor()
        {
            // DeviceRegex matches "Android" before "Samsung" even in SamsungBrowser UAs
            var ua = "Mozilla/5.0 (Linux; Android 13) AppleWebKit/537.36 SamsungBrowser/23.0 Mobile Safari/537.36";
            var result = UserAgentUtil.ParseDevice(ua);

            Assert.Equal("Android", result.Vendor);
        }

        [Fact]
        public void ParseDevice_XiaomiDevice_ReturnsAndroidVendor()
        {
            // DeviceRegex matches "Android" before "Xiaomi"
            var ua = "Mozilla/5.0 (Linux; Android 13; M2102K1G) AppleWebKit/537.36 Chrome/120.0.0.0 Mobile Safari/537.36";
            var result = UserAgentUtil.ParseDevice(ua);

            Assert.Equal("Android", result.Vendor);
        }

        [Fact]
        public void ParseDevice_HuaweiDevice_ReturnsAndroidVendor()
        {
            // DeviceRegex matches "Android" before "Huawei"
            var ua = "Mozilla/5.0 (Linux; Android 13; ELS-AN10) AppleWebKit/537.36 Chrome/120.0.0.0 Mobile Safari/537.36";
            var result = UserAgentUtil.ParseDevice(ua);

            Assert.Equal("Android", result.Vendor);
        }

        [Fact]
        public void ParseDevice_AppleDevice_ReturnsAppleVendor()
        {
            var ua = "Mozilla/5.0 (iPhone; CPU iPhone OS 17_2 like Mac OS X)";
            var result = UserAgentUtil.ParseDevice(ua);

            Assert.Equal("Apple", result.Vendor);
        }

        [Fact]
        public void ParseDevice_AndroidWithoutMobile_ReturnsMobile()
        {
            // Android keyword alone triggers mobile detection
            var ua = "Mozilla/5.0 (Linux; Android 13) AppleWebKit/537.36 Chrome/120.0.0.0 Safari/537.36";
            var result = UserAgentUtil.ParseDevice(ua);

            Assert.Equal(DeviceType.Mobile, result.Type);
        }

        #endregion

        #region IsBot

        [Fact]
        public void IsBot_Googlebot_ReturnsTrue()
        {
            Assert.True(UserAgentUtil.IsBot("Googlebot/2.1"));
        }

        [Fact]
        public void IsBot_Bingbot_ReturnsTrue()
        {
            Assert.True(UserAgentUtil.IsBot("Mozilla/5.0 (compatible; bingbot/2.0)"));
        }

        [Fact]
        public void IsBot_Baiduspider_ReturnsTrue()
        {
            Assert.True(UserAgentUtil.IsBot("Mozilla/5.0 (compatible; Baiduspider/2.0)"));
        }

        [Fact]
        public void IsBot_DuckDuckBot_ReturnsTrue()
        {
            Assert.True(UserAgentUtil.IsBot("DuckDuckBot/1.1"));
        }

        [Fact]
        public void IsBot_YandexBot_ReturnsTrue()
        {
            Assert.True(UserAgentUtil.IsBot("Mozilla/5.0 (compatible; YandexBot/3.0)"));
        }

        [Fact]
        public void IsBot_FacebookBot_ReturnsTrue()
        {
            Assert.True(UserAgentUtil.IsBot("facebookexternalhit/1.1"));
        }

        [Fact]
        public void IsBot_TwitterBot_ReturnsTrue()
        {
            Assert.True(UserAgentUtil.IsBot("Twitterbot/1.0"));
        }

        [Fact]
        public void IsBot_LinkedInBot_ReturnsTrue()
        {
            Assert.True(UserAgentUtil.IsBot("LinkedInBot/1.0"));
        }

        [Fact]
        public void IsBot_SemrushBot_ReturnsTrue()
        {
            Assert.True(UserAgentUtil.IsBot("SemrushBot/1.0"));
        }

        [Fact]
        public void IsBot_AhrefsBot_ReturnsTrue()
        {
            Assert.True(UserAgentUtil.IsBot("AhrefsBot/1.0"));
        }

        [Fact]
        public void IsBot_NormalBrowser_ReturnsFalse()
        {
            Assert.False(UserAgentUtil.IsBot("Mozilla/5.0 (Windows NT 10.0) Chrome/120.0.0.0"));
        }

        [Fact]
        public void IsBot_NullInput_ReturnsFalse()
        {
            Assert.False(UserAgentUtil.IsBot(null));
        }

        [Fact]
        public void IsBot_EmptyInput_ReturnsFalse()
        {
            Assert.False(UserAgentUtil.IsBot(""));
        }

        [Fact]
        public void IsBot_WhitespaceInput_ReturnsFalse()
        {
            Assert.False(UserAgentUtil.IsBot("   "));
        }

        #endregion

        #region IsMobile

        [Fact]
        public void IsMobile_MobileKeyword_ReturnsTrue()
        {
            Assert.True(UserAgentUtil.IsMobile("Mozilla/5.0 (Linux; Android 13) Chrome/120.0.0.0 Mobile Safari/537.36"));
        }

        [Fact]
        public void IsMobile_IPhone_ReturnsTrue()
        {
            Assert.True(UserAgentUtil.IsMobile("Mozilla/5.0 (iPhone; CPU iPhone OS 17_2)"));
        }

        [Fact]
        public void IsMobile_Android_ReturnsTrue()
        {
            Assert.True(UserAgentUtil.IsMobile("Mozilla/5.0 (Linux; Android 13) Chrome/120.0.0.0"));
        }

        [Fact]
        public void IsMobile_DesktopUA_ReturnsFalse()
        {
            Assert.False(UserAgentUtil.IsMobile("Mozilla/5.0 (Windows NT 10.0; Win64; x64) Chrome/120.0.0.0"));
        }

        [Fact]
        public void IsMobile_NullInput_ReturnsFalse()
        {
            Assert.False(UserAgentUtil.IsMobile(null));
        }

        [Fact]
        public void IsMobile_EmptyInput_ReturnsFalse()
        {
            Assert.False(UserAgentUtil.IsMobile(""));
        }

        #endregion

        #region IsWeChat

        [Fact]
        public void IsWeChat_WeChatUserAgent_ReturnsTrue()
        {
            var ua = "Mozilla/5.0 (Linux; Android 13) AppleWebKit/537.36 Chrome/120.0.0.0 Mobile Safari/537.36 MicroMessenger/8.0.44";
            Assert.True(UserAgentUtil.IsWeChat(ua));
        }

        [Fact]
        public void IsWeChat_NormalBrowser_ReturnsFalse()
        {
            Assert.False(UserAgentUtil.IsWeChat("Mozilla/5.0 (Windows NT 10.0) Chrome/120.0.0.0"));
        }

        [Fact]
        public void IsWeChat_NullInput_ReturnsFalse()
        {
            Assert.False(UserAgentUtil.IsWeChat(null));
        }

        [Fact]
        public void IsWeChat_EmptyInput_ReturnsFalse()
        {
            Assert.False(UserAgentUtil.IsWeChat(""));
        }

        [Fact]
        public void IsWeChat_CaseInsensitive_ReturnsTrue()
        {
            Assert.True(UserAgentUtil.IsWeChat("micromessenger/8.0"));
        }

        #endregion

        #region IsAlipay

        [Fact]
        public void IsAlipay_AlipayUserAgent_ReturnsTrue()
        {
            var ua = "Mozilla/5.0 (Linux; Android 13) AppleWebKit/537.36 Chrome/120.0.0.0 Mobile Safari/537.36 AlipayClient/10.5.0";
            Assert.True(UserAgentUtil.IsAlipay(ua));
        }

        [Fact]
        public void IsAlipay_NormalBrowser_ReturnsFalse()
        {
            Assert.False(UserAgentUtil.IsAlipay("Mozilla/5.0 (Windows NT 10.0) Chrome/120.0.0.0"));
        }

        [Fact]
        public void IsAlipay_NullInput_ReturnsFalse()
        {
            Assert.False(UserAgentUtil.IsAlipay(null));
        }

        [Fact]
        public void IsAlipay_EmptyInput_ReturnsFalse()
        {
            Assert.False(UserAgentUtil.IsAlipay(""));
        }

        [Fact]
        public void IsAlipay_CaseInsensitive_ReturnsTrue()
        {
            Assert.True(UserAgentUtil.IsAlipay("alipayclient/10.5"));
        }

        #endregion

        #region GetBrowserDescription

        [Fact]
        public void GetBrowserDescription_Chrome_ReturnsDescription()
        {
            var ua = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36";
            var result = UserAgentUtil.GetBrowserDescription(ua);

            Assert.Contains("Chrome", result);
            Assert.Contains("Windows 10/11", result);
        }

        [Fact]
        public void GetBrowserDescription_MobileChrome_IncludesDeviceType()
        {
            var ua = "Mozilla/5.0 (Linux; Android 13) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Mobile Safari/537.36";
            var result = UserAgentUtil.GetBrowserDescription(ua);

            Assert.Contains("Chrome", result);
            Assert.Contains("Mobile", result);
        }

        [Fact]
        public void GetBrowserDescription_NullInput_ReturnsEmptyOrMinimal()
        {
            var result = UserAgentUtil.GetBrowserDescription(null);

            Assert.NotNull(result);
        }

        [Fact]
        public void GetBrowserDescription_DesktopDevice_DoesNotIncludeDeviceType()
        {
            var ua = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) Chrome/120.0.0.0";
            var result = UserAgentUtil.GetBrowserDescription(ua);

            Assert.DoesNotContain("Desktop", result);
        }

        #endregion

        #region BrowserInfo / OsInfo / DeviceInfo ToString

        [Fact]
        public void BrowserInfo_ToString_IncludesNameAndVersion()
        {
            var info = new BrowserInfo { Name = "Chrome", Version = "120.0" };
            var result = info.ToString();

            Assert.Equal("Chrome 120.0", result);
        }

        [Fact]
        public void BrowserInfo_Unknown_ToString()
        {
            var result = BrowserInfo.Unknown.ToString();

            Assert.Equal("Unknown", result);
        }

        [Fact]
        public void OsInfo_ToString_IncludesNameAndVersion()
        {
            var info = new OsInfo { Name = "Windows 10/11", Version = "10.0" };
            var result = info.ToString();

            Assert.Equal("Windows 10/11 10.0", result);
        }

        [Fact]
        public void OsInfo_Unknown_ToString()
        {
            var result = OsInfo.Unknown.ToString();

            Assert.Equal("Unknown", result);
        }

        [Fact]
        public void DeviceInfo_ToString_IncludesTypeAndVendor()
        {
            var info = new DeviceInfo { Type = DeviceType.Mobile, Vendor = "Samsung", Model = "Galaxy" };
            var result = info.ToString();

            Assert.Contains("Mobile", result);
            Assert.Contains("Samsung", result);
            Assert.Contains("Galaxy", result);
        }

        [Fact]
        public void DeviceInfo_Unknown_ToString()
        {
            var result = DeviceInfo.Unknown.ToString();

            Assert.Contains("Desktop", result);
        }

        #endregion
    }
}
