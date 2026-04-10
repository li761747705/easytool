using Xunit;
using EasyTool.CodeCategory;
using System;

namespace EasyTool.CodeCategory.Tests
{

    public class AesUtilTests
    {
        [Fact]
        public void EncryptSecret16Test()
        {
            var input = "abbfly";
            var sk = "1234567890123456";
            var iv = "1234567890123456";
            var en = AesUtil.Encrypt(input, sk, iv);
            var de = AesUtil.Decrypt(en, sk, iv);
            Assert.Equal(input, de);
        }

        [Fact]
        public void EncryptSecret24Test()
        {
            var input = "abbfly";
            var sk = "123456789012345678901234";
            var iv = "1234567890123456";
            var en = AesUtil.Encrypt(input, sk, iv);
            var de = AesUtil.Decrypt(en, sk, iv);
            Assert.Equal(input, de);
        }

        [Fact]
        public void EncryptSecret32Test()
        {
            var input = "abbfly";
            var sk = "12345678901234567890123456789012";
            var iv = "1234567890123456";
            var en = AesUtil.Encrypt(input, sk, iv);
            var de = AesUtil.Decrypt(en, sk, iv);
            Assert.Equal(input, de);
        }

        [Fact]
        public void EncryptWithBytesTest()
        {
            var data = global::System.Text.Encoding.UTF8.GetBytes("hello world");
            var key = new byte[16]; // 16字节密钥
            var iv = new byte[16];
            for (int i = 0; i < 16; i++) { key[i] = (byte)(i + 1); iv[i] = (byte)(i + 1); }
            var encrypted = AesUtil.Encrypt(data, key, iv);
            var decrypted = AesUtil.Decrypt(encrypted, key, iv);
            Assert.Equal(data, decrypted);
        }
    }
}
