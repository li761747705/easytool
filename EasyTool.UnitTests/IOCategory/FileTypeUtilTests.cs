using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace EasyTool.IOCategory.Tests
{
    public class FileTypeUtilTests
    {
        [Fact]
        public void GetType_JpegFile_ReturnsJpg()
        {
            // 创建一个模拟的JPEG文件头
            var tempFile = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.jpg");
            try
            {
                // JPEG文件头: FF D8 FF
                File.WriteAllBytes(tempFile, new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46 });

                var result = FileTypeUtil.GetType(tempFile);

                Assert.Equal(".jpg", result);
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }

        [Fact]
        public void GetType_PngFile_ReturnsPng()
        {
            var tempFile = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.png");
            try
            {
                // PNG文件头: 89 50 4E 47
                File.WriteAllBytes(tempFile, new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A });

                var result = FileTypeUtil.GetType(tempFile);

                Assert.Equal(".png", result);
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }

        [Fact]
        public void GetType_NonExistentFile_ReturnsExtension()
        {
            var result = FileTypeUtil.GetType("/non/existent/file.unknown");

            Assert.Equal(".unknown", result);
        }

        [Fact]
        public void GetType_ByteArray_ReturnsCorrectType()
        {
            var jpegHeader = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46 };

            var result = FileTypeUtil.GetType(jpegHeader);

            Assert.Equal(".jpg", result);
        }

        [Fact]
        public void GetType_EmptyArray_ReturnsNull()
        {
            var result = FileTypeUtil.GetType(Array.Empty<byte>());

            Assert.Null(result);
        }

        [Fact]
        public void IsImage_JpegFile_ReturnsTrue()
        {
            var tempFile = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.jpg");
            try
            {
                File.WriteAllBytes(tempFile, new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46 });
                var fileInfo = new FileInfo(tempFile);

                var result = FileTypeUtil.IsImage(fileInfo);

                Assert.True(result);
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }

        [Fact]
        public void IsDocument_PdfFile_ReturnsTrue()
        {
            var tempFile = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.pdf");
            try
            {
                // PDF文件头: 25 50 44 46 (%PDF)
                File.WriteAllBytes(tempFile, new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D, 0x31, 0x2E, 0x34 });
                var fileInfo = new FileInfo(tempFile);

                var result = FileTypeUtil.IsDocument(fileInfo);

                Assert.True(result);
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }

        [Fact]
        public void GetMimeType_JpegFile_ReturnsImageJpeg()
        {
            var tempFile = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.jpg");
            try
            {
                File.WriteAllBytes(tempFile, new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46 });
                var fileInfo = new FileInfo(tempFile);

                var result = FileTypeUtil.GetMimeType(fileInfo);

                Assert.Equal("image/jpeg", result);
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }
    }
}