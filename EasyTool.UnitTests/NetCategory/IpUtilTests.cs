using System;
using Xunit;
using EasyTool.NetCategory;

namespace EasyTool.Tests
{
    /// <summary>
    /// 用于测试 IpUtil 类的单元测试方法的测试类。
    /// </summary>
    
    public class IpUtilTests
    {
        /// <summary>
        /// 测试验证 IPv4 地址的方法。
        /// </summary>
        [Fact]
        public void TestIpv4Validation()
        {
            Assert.True(IpUtil.IsIpv4("192.168.1.1"));
            Assert.False(IpUtil.IsIpv4("192.168.1.256"));
            Assert.False(IpUtil.IsIpv4("2001:db8:0:42:0:8a2e:370:7334"));
        }

        /// <summary>
        /// 测试验证 IPv6 地址的方法。
        /// </summary>
        [Fact]
        public void TestIpv6Validation()
        {
            Assert.True(IpUtil.IsIpv6("2001:0db8:0000:0042:0000:8a2e:0370:7334"));
            Assert.True(IpUtil.IsIpv6("2001:db8:0:42:0:8a2e:370:7334"));
            Assert.False(IpUtil.IsIpv6("192.168.1.1"));
        }

        /// <summary>
        /// 测试将 IPv6 地址转换为 ulong 数字，并将其从 ulong 数字转换回 IPv6 地址的方法。
        /// </summary>
        [Fact]
        public void TestUlongsToIpv6()
        {
            var (high, low) = IpUtil.Ipv6ToUlongs("2001:0db8:0000:0042:0000:8a2e:0370:7334");
            var ipv6 = IpUtil.UlongsToIpv6(high, low);

            Assert.Equal("2001:db8:0:42:0:8a2e:370:7334", ipv6);
        }

        /// <summary>
        /// 验证有效的 IPv4 地址，应返回 true。
        /// </summary>
        [Fact]
        public void IsIpv4_ValidIps_ReturnsTrue()
        {
            Assert.True(IpUtil.IsIpv4("192.168.1.1"));
            Assert.True(IpUtil.IsIpv4("127.0.0.1"));
            Assert.True(IpUtil.IsIpv4("0.0.0.0"));
        }

        /// <summary>
        /// 验证无效的 IPv4 地址，应返回 false。
        /// </summary>
        [Fact]
        public void IsIpv4_InvalidIps_ReturnsFalse()
        {
            Assert.False(IpUtil.IsIpv4("192.168.1."));
            Assert.False(IpUtil.IsIpv4("256.256.256.256"));
            Assert.False(IpUtil.IsIpv4("192.168.1.1.1"));
        }

        /// <summary>
        /// 验证有效的 IPv6 地址，应返回 true。
        /// </summary>
        [Fact]
        public void IsIpv6_ValidIps_ReturnsTrue()
        {
            Assert.True(IpUtil.IsIpv6("::1"));
            Assert.True(IpUtil.IsIpv6("2001:0db8:0000:0042:0000:8a2e:0370:7334"));
        }

        /// <summary>
        /// 验证无效的 IPv6 地址，应返回 false。
        /// </summary>
        [Fact]
        public void IsIpv6_InvalidIps_ReturnsFalse()
        {
            Assert.False(IpUtil.IsIpv6(":::1"));
            Assert.False(IpUtil.IsIpv6("GGGG:0db8:0000:0042:0000:8a2e:0370:7334"));
        }

        /// <summary>
        /// 验证有效的私有 IPv4 地址，应返回 true。
        /// </summary>
        [Fact]
        public void IsPrivateIpv4_ValidPrivateIps_ReturnsTrue()
        {
            Assert.True(IpUtil.IsPrivateIpv4("10.0.0.1"));
            Assert.True(IpUtil.IsPrivateIpv4("192.168.1.1"));
            Assert.True(IpUtil.IsPrivateIpv4("172.16.1.1"));
        }

        /// <summary>
        /// 验证无效的 IPv4 地址，应引发 ArgumentException 异常。
        /// </summary>
        [Fact]
        public void IsPrivateIpv4_InvalidIp_ThrowsException()
        {
            try
            {
                IpUtil.IsPrivateIpv4("256.256.256.256");
                Assert.Fail("Expected ArgumentException was not thrown.");
            }
            catch (ArgumentException)
            {
            }
        }

        /// <summary>
        /// 验证有效的私有 IPv6 地址，应返回 true。
        /// </summary>
        [Fact]
        public void IsPrivateIpv6_ValidPrivateIps_ReturnsTrue()
        {
            Assert.True(IpUtil.IsPrivateIpv6("fd00::1"));
        }

        /// <summary>
        /// 验证无效的 IPv6 地址，应引发 ArgumentException 异常。
        /// </summary>
        [Fact]
        public void IsPrivateIpv6_InvalidIp_ThrowsException()
        {
            try
            {
                IpUtil.IsPrivateIpv6("2001::1::2");
                Assert.Fail("Expected ArgumentException was not thrown.");
            }
            catch (ArgumentException)
            {
            }
        }

        /// <summary>
        /// 验证将 IPv4 地址转换为整数，并将整数转换回 IPv4 地址的方法是否一致。
        /// </summary>
        [Fact]
        public void Ipv4ToInt_And_IntToIpv4_AreConsistent()
        {
            string originalIp = "192.168.1.1";
            uint intIp = IpUtil.Ipv4ToInt(originalIp);
            string convertedIp = IpUtil.IntToIpv4(intIp);
            Assert.Equal(originalIp, convertedIp);
        }

        /// <summary>
        /// 验证将 IPv6 地址转换为 ulong 数字，并将其从 ulong 数字转换回 IPv6 地址的方法是否一致。
        /// </summary>
        [Fact]
        public void Ipv6ToUlongs_And_UlongsToIpv6_AreConsistent()
        {
            string originalIp = "2001:db8:0:42:0:8a2e:370:7334";
            var (high, low) = IpUtil.Ipv6ToUlongs(originalIp);
            string convertedIp = IpUtil.UlongsToIpv6(high, low);
            Assert.Equal(originalIp, convertedIp.ToLower());
        }
    }
}