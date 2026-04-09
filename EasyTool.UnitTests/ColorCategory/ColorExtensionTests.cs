using Xunit;
using System.Drawing;

namespace EasyTool.ColorCategory.Tests
{
    public class ColorExtensionTests
    {
        [Fact]
        public void ToHex_ConvertsColorToHexString()
        {
            var color = Color.FromArgb(255, 0, 128);
            var result = color.ToHex();
            Assert.Equal("#FF0080", result);
        }

        [Fact]
        public void ToHex_WithAlpha_IncludesAlpha()
        {
            var color = Color.FromArgb(128, 255, 0, 128);
            var result = color.ToHex(true);
            Assert.Equal("#80FF0080", result);
        }

        [Fact]
        public void FromHex_ParsesHexColor()
        {
            var result = ColorExtension.FromHex("#FF0080");
            Assert.Equal(255, result.R);
            Assert.Equal(0, result.G);
            Assert.Equal(128, result.B);
        }

        [Fact]
        public void FromHex_WithAlpha_ParsesCorrectly()
        {
            var result = ColorExtension.FromHex("#80FF0080");
            Assert.Equal(128, result.A);
            Assert.Equal(255, result.R);
            Assert.Equal(0, result.G);
            Assert.Equal(128, result.B);
        }

        [Fact]
        public void FromHex_EmptyString_ReturnsEmpty()
        {
            var result = ColorExtension.FromHex("");
            Assert.Equal(Color.Empty, result);
        }

        [Fact]
        public void FromHex_NullString_ReturnsEmpty()
        {
            var result = ColorExtension.FromHex(null!);
            Assert.Equal(Color.Empty, result);
        }

        [Fact]
        public void ToRgbString_ReturnsCorrectFormat()
        {
            var color = Color.FromArgb(255, 128, 64);
            var result = color.ToRgbString();
            Assert.Equal("rgb(255, 128, 64)", result);
        }

        [Fact]
        public void ToRgbaString_ReturnsCorrectFormat()
        {
            var color = Color.FromArgb(128, 255, 128, 64);
            var result = color.ToRgbaString();
            Assert.StartsWith("rgba(255, 128, 64,", result);
            Assert.EndsWith(")", result);
        }

        [Fact]
        public void ToHsl_ReturnsCorrectValues()
        {
            var color = Color.Red;
            var (h, s, l) = color.ToHsl();
            // h: 0-360, s: 0-100, l: 0-100
            Assert.True(h >= 0 && h <= 360);
            Assert.True(s >= 0 && s <= 100);
            Assert.True(l >= 0 && l <= 100);
        }

        [Fact]
        public void FromHsl_CreatesColor()
        {
            // Red: h=0, s=100%, l=50%
            var result = ColorExtension.FromHsl(0, 100, 50);
            Assert.Equal(255, result.R);
            Assert.Equal(0, result.G);
            Assert.Equal(0, result.B);
        }

        [Fact]
        public void Lighten_MakesColorLighter()
        {
            var color = Color.FromArgb(128, 128, 128);
            // percent is in 0-100 range
            var result = color.Lighten(20);
            Assert.True(result.R > color.R);
        }

        [Fact]
        public void Darken_MakesColorDarker()
        {
            var color = Color.FromArgb(128, 128, 128);
            // percent is in 0-100 range
            var result = color.Darken(20);
            Assert.True(result.R < color.R);
        }

        [Fact]
        public void WithAlpha_ChangesAlphaChannel()
        {
            var color = Color.FromArgb(255, 100, 100, 100);
            var result = color.WithAlpha(128);
            Assert.Equal(128, result.A);
            Assert.Equal(100, result.R);
        }

        [Fact]
        public void Invert_InvertsColor()
        {
            var color = Color.FromArgb(255, 0, 0);
            var result = color.Invert();
            Assert.Equal(0, result.R);
            Assert.Equal(255, result.G);
            Assert.Equal(255, result.B);
        }

        [Fact]
        public void Grayscale_ConvertsToGray()
        {
            var color = Color.FromArgb(255, 0, 0);
            var result = color.Grayscale();
            // Grayscale should have equal R, G, B values
            Assert.Equal(result.R, result.G);
            Assert.Equal(result.G, result.B);
        }
    }
}