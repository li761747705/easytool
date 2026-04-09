using Xunit;
using EasyTool.MathCategory;
using System;
using System.Collections.Generic;
using System.Text;

namespace EasyTool.Tests
{
    
    public class MathUtilTests
    {
        [Fact]
        public void GcdTest()
        {
            var result = MathUtil.Gcd(5, 20);
            Assert.Equal(5, result);
        }
    }
}