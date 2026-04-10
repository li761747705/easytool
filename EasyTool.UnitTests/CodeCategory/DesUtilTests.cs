using Xunit;
using EasyTool.CodeCategory;
using System;

namespace EasyTool.CodeCategory.Tests
{

    public class DesUtilTests
    {
        [Fact]
        public void EncryptSecret8Test()
        {
            var input = "abbfly";
            var sk = "12345678";
            var iv = "12345678";
            var en = DesUtil.Encrypt(input, sk, iv);
            var de = DesUtil.Decrypt(en, sk, iv);
            Assert.Equal(input, de);
        }

        [Fact]
        public void EncryptWithBytesTest()
        {
            var data = global::System.Text.Encoding.UTF8.GetBytes("hello world");
            var key = new byte[8];
            var iv = new byte[8];
            for (int i = 0; i < 8; i++) { key[i] = (byte)(i + 1); iv[i] = (byte)(i + 1); }
            var encrypted = DesUtil.Encrypt(data, key, iv);
            var decrypted = DesUtil.Decrypt(encrypted, key, iv);
            Assert.Equal(data, decrypted);
        }
    }
}
