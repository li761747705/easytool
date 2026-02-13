using System;
using System.Drawing;

namespace EasyTool.Extension
{
    /// <summary>
    /// Color 颜色扩展方法
    /// </summary>
    public static class ColorExtension
    {
        #region 转换方法

        /// <summary>
        /// 将颜色转换为16进制字符串
        /// </summary>
        public static string ToHex(this Color color)
        {
            return color.ToHex(false);
        }

        /// <summary>
        /// 将颜色转换为16进制字符串
        /// </summary>
        public static string ToHex(this Color color, bool includeAlpha)
        {
            if (includeAlpha)
                return $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
            else
                return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }

        /// <summary>
        /// 从16进制字符串创建颜色
        /// </summary>
        public static Color FromHex(string hex)
        {
            if (string.IsNullOrEmpty(hex))
                return Color.Empty;

            hex = hex.TrimStart('#');

            if (hex.Length == 6)
            {
                int r = int.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                int g = int.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                int b = int.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
                return Color.FromArgb(r, g, b);
            }
            else if (hex.Length == 8)
            {
                int a = int.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                int r = int.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                int g = int.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
                int b = int.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
                return Color.FromArgb(a, r, g, b);
            }

            throw new ArgumentException("Invalid hex color format", nameof(hex));
        }

        /// <summary>
        /// 将颜色转换为 RGB 字符串
        /// </summary>
        public static string ToRgbString(this Color color)
        {
            return $"rgb({color.R}, {color.G}, {color.B})";
        }

        /// <summary>
        /// 将颜色转换为 RGBA 字符串
        /// </summary>
        public static string ToRgbaString(this Color color)
        {
            return $"rgba({color.R}, {color.G}, {color.B}, {color.A / 255f:F2})";
        }

        /// <summary>
        /// 将颜色转换为 HSL
        /// </summary>
        public static (double h, double s, double l) ToHsl(this Color color)
        {
            double r = color.R / 255d;
            double g = color.G / 255d;
            double b = color.B / 255d;

            double max = Math.Max(r, Math.Max(g, b));
            double min = Math.Min(r, Math.Min(g, b));
            double h = 0, s = 0, l = (max + min) / 2d;

            if (max != min)
            {
                double d = max - min;
                s = l > 0.5d ? d / (2d - max - min) : d / (max + min);

                if (max == r)
                    h = (g - b) / d + (g < b ? 6d : 0d);
                else if (max == g)
                    h = (b - r) / d + 2d;
                else
                    h = (r - g) / d + 4d;

                h /= 6d;
            }

            return (h * 360d, s * 100d, l * 100d);
        }

        /// <summary>
        /// 从 HSL 创建颜色
        /// </summary>
        public static Color FromHsl(double h, double s, double l)
        {
            h = h / 360d;
            s = s / 100d;
            l = l / 100d;

            double r, g, b;

            if (s == 0)
            {
                r = g = b = l;
            }
            else
            {
                double q = l < 0.5d ? l * (1d + s) : l + s - l * s;
                double p = 2d * l - q;

                r = Hue2Rgb(p, q, h + 1d / 3d);
                g = Hue2Rgb(p, q, h);
                b = Hue2Rgb(p, q, h - 1d / 3d);
            }

            return Color.FromArgb((int)(r * 255), (int)(g * 255), (int)(b * 255));
        }

        private static double Hue2Rgb(double p, double q, double t)
        {
            if (t < 0) t += 1;
            if (t > 1) t -= 1;
            if (t < 1d / 6d) return p + (q - p) * 6d * t;
            if (t < 1d / 2d) return q;
            if (t < 2d / 3d) return p + (q - p) * (2d / 3d - t) * 6d;
            return p;
        }

        #endregion

        #region 颜色调整

        /// <summary>
        /// 变亮颜色
        /// </summary>
        public static Color Lighten(this Color color, double percent)
        {
            var (h, s, l) = color.ToHsl();
            l = Math.Min(100, l + percent);
            return FromHsl(h, s, l);
        }

        /// <summary>
        /// 变暗颜色
        /// </summary>
        public static Color Darken(this Color color, double percent)
        {
            var (h, s, l) = color.ToHsl();
            l = Math.Max(0, l - percent);
            return FromHsl(h, s, l);
        }

        /// <summary>
        /// 调整颜色透明度
        /// </summary>
        public static Color WithAlpha(this Color color, int alpha)
        {
            return Color.FromArgb(alpha, color.R, color.G, color.B);
        }

        /// <summary>
        /// 调整颜色透明度
        /// </summary>
        public static Color WithAlpha(this Color color, double alphaPercent)
        {
            int alpha = (int)(alphaPercent * 255);
            return Color.FromArgb(alpha, color.R, color.G, color.B);
        }

        /// <summary>
        /// 反转颜色
        /// </summary>
        public static Color Invert(this Color color)
        {
            return Color.FromArgb(color.A, 255 - color.R, 255 - color.G, 255 - color.B);
        }

        /// <summary>
        /// 灰度化颜色
        /// </summary>
        public static Color Grayscale(this Color color)
        {
            int gray = (int)(color.R * 0.299 + color.G * 0.587 + color.B * 0.114);
            return Color.FromArgb(color.A, gray, gray, gray);
        }

        /// <summary>
        /// 混合两种颜色
        /// </summary>
        public static Color Blend(this Color color1, Color color2, double percent)
        {
            int r = (int)(color1.R + (color2.R - color1.R) * percent);
            int g = (int)(color1.G + (color2.G - color1.G) * percent);
            int b = (int)(color1.B + (color2.B - color1.B) * percent);
            int a = (int)(color1.A + (color2.A - color1.A) * percent);
            return Color.FromArgb(a, r, g, b);
        }

        /// <summary>
        /// 获取互补色
        /// </summary>
        public static Color Complementary(this Color color)
        {
            var (h, s, l) = color.ToHsl();
            h = (h + 180) % 360;
            return FromHsl(h, s, l);
        }

        /// <summary>
        /// 获取类比色
        /// </summary>
        public static Color[] Analogous(this Color color, int count = 3)
        {
            var (h, s, l) = color.ToHsl();
            var colors = new Color[count];

            for (int i = 0; i < count; i++)
            {
                double hue = (h + (i * 30)) % 360;
                colors[i] = FromHsl(hue, s, l);
            }

            return colors;
        }

        /// <summary>
        /// 获取三色组合
        /// </summary>
        public static Color[] Triadic(this Color color)
        {
            var (h, s, l) = color.ToHsl();
            return new[]
            {
                FromHsl(h, s, l),
                FromHsl((h + 120) % 360, s, l),
                FromHsl((h + 240) % 360, s, l)
            };
        }

        #endregion

        #region 颜色判断

        /// <summary>
        /// 判断是否是深色
        /// </summary>
        public static bool IsDark(this Color color)
        {
            // 使用亮度公式判断
            double brightness = (color.R * 299 + color.G * 587 + color.B * 114) / 1000d;
            return brightness < 128;
        }

        /// <summary>
        /// 判断是否是浅色
        /// </summary>
        public static bool IsLight(this Color color)
        {
            return !color.IsDark();
        }

        #endregion

        #region 命名颜色

        /// <summary>
        /// 从名称创建颜色
        /// [Obsolete("请直接使用 Color.FromName(name)")]
        /// </summary>
        [Obsolete("请直接使用 Color.FromName(name)", false)]
        public static Color FromName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return Color.Empty;

            return Color.FromName(name);
        }

        /// <summary>
        /// 获取颜色名称
        /// </summary>
        public static string GetColorName(this Color color)
        {
            if (color.IsNamedColor)
                return color.Name;

            return color.ToHex();
        }

        #endregion

        #region 颜色对比

        /// <summary>
        /// 计算两种颜色的对比度
        /// </summary>
        public static double ContrastWith(this Color color1, Color color2)
        {
            double GetLuminance(Color c)
            {
                double r = c.R / 255d;
                double g = c.G / 255d;
                double b = c.B / 255d;

                r = r <= 0.03928 ? r / 12.92 : Math.Pow((r + 0.055) / 1.055, 2.4);
                g = g <= 0.03928 ? g / 12.92 : Math.Pow((g + 0.055) / 1.055, 2.4);
                b = b <= 0.03928 ? b / 12.92 : Math.Pow((b + 0.055) / 1.055, 2.4);

                return 0.2126 * r + 0.7152 * g + 0.0722 * b;
            }

            double l1 = GetLuminance(color1);
            double l2 = GetLuminance(color2);

            double lighter = Math.Max(l1, l2);
            double darker = Math.Min(l1, l2);

            return (lighter + 0.05) / (darker + 0.05);
        }

        /// <summary>
        /// 根据背景色选择合适的文本颜色（黑色或白色）
        /// </summary>
        public static Color GetReadableTextColor(this Color backgroundColor)
        {
            return backgroundColor.IsDark() ? Color.White : Color.Black;
        }

        #endregion
    }
}
