using Xunit;
using EasyTool.CodeCategory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyTool.CodeCategory.Tests
{
    
    public class DesUtilTests
    {
        [Fact]
        public void EncryptSecret8Test()
        {
            var input = "abbfly";
            var sk = "12345678";
            var en = DesUtil.Encrypt(input, sk);
            var de = DesUtil.Decrypt(en, sk);
            Assert.Equal(input, de);
        }
    }
}