using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Xunit;

namespace EasyTool.UnitTests.ImageCategory
{
    /// <summary>
    /// ImgUtil 测试类
    /// 注意：System.Drawing 在非 Windows 平台上可能需要特殊配置
    /// </summary>
    public class ImgUtilTests
    {
        #region 测试辅助方法

        private Image CreateTestImage(int width = 100, int height = 100)
        {
            var bitmap = new Bitmap(width, height);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.Blue);
                graphics.FillRectangle(Brushes.Red, 10, 10, 80, 80);
            }
            return bitmap;
        }

        #endregion

        #region ResizeImage 测试

        [Fact]
        public void ResizeImage_ValidImage_ReturnsResizedImage()
        {
            using var original = CreateTestImage(100, 100);
            using var resized = ImgUtil.ResizeImage(original, 50, 50);

            Assert.NotNull(resized);
            Assert.Equal(50, resized.Width);
            Assert.Equal(50, resized.Height);
        }

        [Fact]
        public void ResizeImage_LargerSize_ReturnsEnlargedImage()
        {
            using var original = CreateTestImage(100, 100);
            using var resized = ImgUtil.ResizeImage(original, 200, 200);

            Assert.Equal(200, resized.Width);
            Assert.Equal(200, resized.Height);
        }

        [Theory]
        [InlineData(100, 100, 50, 50)]
        [InlineData(100, 100, 150, 75)]
        [InlineData(50, 50, 25, 25)]
        public void ResizeImage_VariousSizes_ReturnsCorrectDimensions(
            int origWidth, int origHeight, int newWidth, int newHeight)
        {
            using var original = CreateTestImage(origWidth, origHeight);
            using var resized = ImgUtil.ResizeImage(original, newWidth, newHeight);

            Assert.Equal(newWidth, resized.Width);
            Assert.Equal(newHeight, resized.Height);
        }

        #endregion

        #region CropImage 测试

        [Fact]
        public void CropImage_ValidRegion_ReturnsCroppedImage()
        {
            using var original = CreateTestImage(100, 100);
            using var cropped = ImgUtil.CropImage(original, 10, 10, 50, 50);

            Assert.NotNull(cropped);
            Assert.Equal(50, cropped.Width);
            Assert.Equal(50, cropped.Height);
        }

        [Fact]
        public void CropImage_FullRegion_ReturnsSameSize()
        {
            using var original = CreateTestImage(100, 100);
            using var cropped = ImgUtil.CropImage(original, 0, 0, 100, 100);

            Assert.Equal(100, cropped.Width);
            Assert.Equal(100, cropped.Height);
        }

        [Theory]
        [InlineData(0, 0, 50, 50)]
        [InlineData(25, 25, 50, 50)]
        [InlineData(0, 0, 100, 100)]
        public void CropImage_VariousRegions_ReturnsCorrectDimensions(
            int x, int y, int width, int height)
        {
            using var original = CreateTestImage(100, 100);
            using var cropped = ImgUtil.CropImage(original, x, y, width, height);

            Assert.Equal(width, cropped.Width);
            Assert.Equal(height, cropped.Height);
        }

        #endregion

        #region ConvertImageFormat 测试

        [Fact]
        public void ConvertImageFormat_ToPng_ReturnsPngImage()
        {
            using var original = CreateTestImage(100, 100);
            using var converted = ImgUtil.ConvertImageFormat(original, ImageFormat.Png);

            Assert.NotNull(converted);
            Assert.Equal(100, converted.Width);
            Assert.Equal(100, converted.Height);
        }

        [Fact]
        public void ConvertImageFormat_ToJpeg_ReturnsJpegImage()
        {
            using var original = CreateTestImage(100, 100);
            using var converted = ImgUtil.ConvertImageFormat(original, ImageFormat.Jpeg);

            Assert.NotNull(converted);
            Assert.Equal(100, converted.Width);
            Assert.Equal(100, converted.Height);
        }

        #endregion

        #region ConvertToBlackAndWhite 测试

        [Fact]
        public void ConvertToBlackAndWhite_ColorImage_ReturnsGrayscaleImage()
        {
            using var original = CreateTestImage(100, 100);
            using var bw = ImgUtil.ConvertToBlackAndWhite(original);

            Assert.NotNull(bw);
            Assert.Equal(100, bw.Width);
            Assert.Equal(100, bw.Height);
        }

        [Fact]
        public void ConvertToBlackAndWhite_PreservesDimensions()
        {
            using var original = CreateTestImage(200, 150);
            using var bw = ImgUtil.ConvertToBlackAndWhite(original);

            Assert.Equal(200, bw.Width);
            Assert.Equal(150, bw.Height);
        }

        #endregion

        #region AddTextWatermark 测试

        [Fact]
        public void AddTextWatermark_ValidText_ReturnsImageWithWatermark()
        {
            using var original = CreateTestImage(100, 100);
            using var font = new Font("Arial", 12);
            using var watermark = ImgUtil.AddTextWatermark(original, "Test", font, Brushes.White, 10, 10);

            Assert.NotNull(watermark);
            Assert.Equal(100, watermark.Width);
            Assert.Equal(100, watermark.Height);
        }

        [Fact]
        public void AddTextWatermark_PreservesOriginalDimensions()
        {
            using var original = CreateTestImage(200, 150);
            using var font = new Font("Arial", 12);
            using var watermark = ImgUtil.AddTextWatermark(original, "Test", font, Brushes.White, 10, 10);

            Assert.Equal(200, watermark.Width);
            Assert.Equal(150, watermark.Height);
        }

        #endregion

        #region AddImageWatermark 测试

        [Fact]
        public void AddImageWatermark_ValidWatermark_ReturnsCompositeImage()
        {
            using var original = CreateTestImage(100, 100);
            using var watermarkImg = CreateTestImage(20, 20);
            using var result = ImgUtil.AddImageWatermark(original, watermarkImg, 0.5f, 10, 10);

            Assert.NotNull(result);
            Assert.Equal(100, result.Width);
            Assert.Equal(100, result.Height);
        }

        [Theory]
        [InlineData(0.0f)]
        [InlineData(0.5f)]
        [InlineData(1.0f)]
        public void AddImageWatermark_VariousOpacity_ReturnsValidImage(float opacity)
        {
            using var original = CreateTestImage(100, 100);
            using var watermarkImg = CreateTestImage(20, 20);
            using var result = ImgUtil.AddImageWatermark(original, watermarkImg, opacity, 10, 10);

            Assert.NotNull(result);
        }

        #endregion

        #region RotateImage 测试

        [Fact]
        public void RotateImage_90Degrees_ReturnsRotatedImage()
        {
            using var original = CreateTestImage(100, 100);
            using var rotated = ImgUtil.RotateImage(original, 90);

            Assert.NotNull(rotated);
            Assert.Equal(100, rotated.Width);
            Assert.Equal(100, rotated.Height);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(45)]
        [InlineData(90)]
        [InlineData(180)]
        [InlineData(270)]
        public void RotateImage_VariousAngles_ReturnsValidImage(float angle)
        {
            using var original = CreateTestImage(100, 100);
            using var rotated = ImgUtil.RotateImage(original, angle);

            Assert.NotNull(rotated);
        }

        #endregion

        #region FlipImageHorizontally 测试

        [Fact]
        public void FlipImageHorizontally_ValidImage_ReturnsFlippedImage()
        {
            using var original = CreateTestImage(100, 100);
            using var flipped = ImgUtil.FlipImageHorizontally(original);

            Assert.NotNull(flipped);
            Assert.Equal(100, flipped.Width);
            Assert.Equal(100, flipped.Height);
        }

        [Fact]
        public void FlipImageHorizontally_PreservesDimensions()
        {
            using var original = CreateTestImage(200, 150);
            using var flipped = ImgUtil.FlipImageHorizontally(original);

            Assert.Equal(200, flipped.Width);
            Assert.Equal(150, flipped.Height);
        }

        #endregion

        #region MaskImage 测试

        [Fact]
        public void MaskImage_SameDimensions_ReturnsMaskedImage()
        {
            using var original = CreateTestImage(100, 100);
            using var mask = CreateTestImage(100, 100);
            using var masked = ImgUtil.MaskImage(mask, original);

            Assert.NotNull(masked);
            Assert.Equal(100, masked.Width);
            Assert.Equal(100, masked.Height);
        }

        [Fact]
        public void MaskImage_DifferentDimensions_ThrowsException()
        {
            using var original = CreateTestImage(100, 100);
            using var mask = CreateTestImage(50, 50);

            Assert.Throws<ArgumentException>(() => ImgUtil.MaskImage(mask, original));
        }

        #endregion
    }
}