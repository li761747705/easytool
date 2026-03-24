using System;

namespace EasyTool.ColorCategory
{
    /// <summary>
    /// 颜色工具类
    /// 提供颜色空间转换和颜色操作功能
    /// </summary>
    public static class ColorUtil
    {
        /// <summary>
        /// RGB 转 HSL
        /// </summary>
        public static HSL RGBToHSL(int r, int g, int b)
        {
            double rd = r / 255.0;
            double gd = g / 255.0;
            double bd = b / 255.0;

            double max = Math.Max(rd, Math.Max(gd, bd));
            double min = Math.Min(rd, Math.Min(gd, bd));
            double h = 0, s = 0, l = (max + min) / 2;

            if (Math.Abs(max - min) > 0.0001)
            {
                double d = max - min;
                s = l > 0.5 ? d / (2 - max - min) : d / (max + min);

                if (Math.Abs(max - rd) < 0.0001)
                    h = (gd - bd) / d + (gd < bd ? 6 : 0);
                else if (Math.Abs(max - gd) < 0.0001)
                    h = (bd - rd) / d + 2;
                else
                    h = (rd - gd) / d + 4;

                h /= 6;
            }

            return new HSL(h * 360, s * 100, l * 100);
        }

        /// <summary>
        /// HSL 转 RGB
        /// </summary>
        public static RGB HSLToRGB(double h, double s, double l)
        {
            h /= 360;
            s /= 100;
            l /= 100;

            double r, g, b;

            if (Math.Abs(s) < 0.0001)
            {
                r = g = b = l;
            }
            else
            {
                double q = l < 0.5 ? l * (1 + s) : l + s - l * s;
                double p = 2 * l - q;

                r = HueToRGB(p, q, h + 1.0 / 3.0);
                g = HueToRGB(p, q, h);
                b = HueToRGB(p, q, h - 1.0 / 3.0);
            }

            return new RGB((int)Math.Round(r * 255), (int)Math.Round(g * 255), (int)Math.Round(b * 255));
        }

        private static double HueToRGB(double p, double q, double t)
        {
            if (t < 0) t += 1;
            if (t > 1) t -= 1;

            if (t < 1.0 / 6.0) return p + (q - p) * 6 * t;
            if (t < 1.0 / 2.0) return q;
            if (t < 2.0 / 3.0) return p + (q - p) * (2.0 / 3.0 - t) * 6;

            return p;
        }

        /// <summary>
        /// RGB 转 HSV
        /// </summary>
        public static HSV RGBToHSV(int r, int g, int b)
        {
            double rd = r / 255.0;
            double gd = g / 255.0;
            double bd = b / 255.0;

            double max = Math.Max(rd, Math.Max(gd, bd));
            double min = Math.Min(rd, Math.Min(gd, bd));
            double h = 0, s = max == 0 ? 0 : (max - min) / max, v = max;

            if (Math.Abs(max - min) > 0.0001)
            {
                double d = max - min;

                if (Math.Abs(max - rd) < 0.0001)
                    h = (gd - bd) / d + (gd < bd ? 6 : 0);
                else if (Math.Abs(max - gd) < 0.0001)
                    h = (bd - rd) / d + 2;
                else
                    h = (rd - gd) / d + 4;

                h /= 6;
            }

            return new HSV(h * 360, s * 100, v * 100);
        }

        /// <summary>
        /// HSV 转 RGB
        /// </summary>
        public static RGB HSVToRGB(double h, double s, double v)
        {
            h /= 360;
            s /= 100;
            v /= 100;

            int i = (int)Math.Floor(h * 6);
            double f = h * 6 - i;
            double p = v * (1 - s);
            double q = v * (1 - f * s);
            double t = v * (1 - (1 - f) * s);

            double r, g, b;

            switch (i % 6)
            {
                case 0: r = v; g = t; b = p; break;
                case 1: r = q; g = v; b = p; break;
                case 2: r = p; g = v; b = t; break;
                case 3: r = p; g = q; b = v; break;
                case 4: r = t; g = p; b = v; break;
                default: r = v; g = p; b = q; break;
            }

            return new RGB((int)Math.Round(r * 255), (int)Math.Round(g * 255), (int)Math.Round(b * 255));
        }

        /// <summary>
        /// RGB 转 CMYK
        /// </summary>
        public static CMYK RGBToCMYK(int r, int g, int b)
        {
            double rd = r / 255.0;
            double gd = g / 255.0;
            double bd = b / 255.0;

            double k = 1 - Math.Max(rd, Math.Max(gd, bd));

            if (Math.Abs(k - 1) < 0.0001)
            {
                return new CMYK(0, 0, 0, 100);
            }

            double c = (1 - rd - k) / (1 - k);
            double m = (1 - gd - k) / (1 - k);
            double y = (1 - bd - k) / (1 - k);

            return new CMYK(c * 100, m * 100, y * 100, k * 100);
        }

        /// <summary>
        /// CMYK 转 RGB
        /// </summary>
        public static RGB CMYKToRGB(double c, double m, double y, double k)
        {
            c /= 100;
            m /= 100;
            y /= 100;
            k /= 100;

            int r = (int)Math.Round(255 * (1 - c) * (1 - k));
            int g = (int)Math.Round(255 * (1 - m) * (1 - k));
            int b = (int)Math.Round(255 * (1 - y) * (1 - k));

            return new RGB(r, g, b);
        }

        /// <summary>
        /// RGB 转十六进制
        /// </summary>
        public static string RGBToHex(int r, int g, int b)
        {
            return $"#{r:X2}{g:X2}{b:X2}";
        }

        /// <summary>
        /// 十六进制转 RGB
        /// </summary>
        public static RGB HexToRGB(string hex)
        {
            hex = hex.TrimStart('#');

            if (hex.Length == 3)
            {
                hex = $"{hex[0]}{hex[0]}{hex[1]}{hex[1]}{hex[2]}{hex[2]}";
            }

            int r = Convert.ToInt32(hex.Substring(0, 2), 16);
            int g = Convert.ToInt32(hex.Substring(2, 2), 16);
            int b = Convert.ToInt32(hex.Substring(4, 2), 16);

            return new RGB(r, g, b);
        }

        /// <summary>
        /// 计算两个颜色的对比度
        /// </summary>
        public static double ContrastRatio(RGB color1, RGB color2)
        {
            double lum1 = RelativeLuminance(color1);
            double lum2 = RelativeLuminance(color2);

            double lighter = Math.Max(lum1, lum2);
            double darker = Math.Min(lum1, lum2);

            return (lighter + 0.05) / (darker + 0.05);
        }

        /// <summary>
        /// 计算相对亮度
        /// </summary>
        public static double RelativeLuminance(RGB color)
        {
            double r = color.R / 255.0;
            double g = color.G / 255.0;
            double b = color.B / 255.0;

            r = r <= 0.03928 ? r / 12.92 : Math.Pow((r + 0.055) / 1.055, 2.4);
            g = g <= 0.03928 ? g / 12.92 : Math.Pow((g + 0.055) / 1.055, 2.4);
            b = b <= 0.03928 ? b / 12.92 : Math.Pow((b + 0.055) / 1.055, 2.4);

            return 0.2126 * r + 0.7152 * g + 0.0722 * b;
        }

        /// <summary>
        /// 混合两个颜色
        /// </summary>
        public static RGB Blend(RGB color1, RGB color2, double ratio = 0.5)
        {
            ratio = Math.Max(0, Math.Min(1, ratio));

            int r = (int)Math.Round(color1.R * (1 - ratio) + color2.R * ratio);
            int g = (int)Math.Round(color1.G * (1 - ratio) + color2.G * ratio);
            int b = (int)Math.Round(color1.B * (1 - ratio) + color2.B * ratio);

            return new RGB(r, g, b);
        }

        /// <summary>
        /// 调整亮度
        /// </summary>
        public static RGB AdjustBrightness(RGB color, double amount)
        {
            var hsl = RGBToHSL(color.R, color.G, color.B);
            hsl = new HSL(hsl.H, hsl.S, Math.Max(0, Math.Min(100, hsl.L + amount)));
            return HSLToRGB(hsl.H, hsl.S, hsl.L);
        }

        /// <summary>
        /// 调整饱和度
        /// </summary>
        public static RGB AdjustSaturation(RGB color, double amount)
        {
            var hsl = RGBToHSL(color.R, color.G, color.B);
            hsl = new HSL(hsl.H, Math.Max(0, Math.Min(100, hsl.S + amount)), hsl.L);
            return HSLToRGB(hsl.H, hsl.S, hsl.L);
        }

        /// <summary>
        /// 获取互补色
        /// </summary>
        public static RGB GetComplementary(RGB color)
        {
            var hsl = RGBToHSL(color.R, color.G, color.B);
            hsl = new HSL((hsl.H + 180) % 360, hsl.S, hsl.L);
            return HSLToRGB(hsl.H, hsl.S, hsl.L);
        }

        /// <summary>
        /// 获取灰度色
        /// </summary>
        public static RGB ToGrayscale(RGB color)
        {
            int gray = (int)Math.Round(0.299 * color.R + 0.587 * color.G + 0.114 * color.B);
            return new RGB(gray, gray, gray);
        }

        /// <summary>
        /// 反转颜色
        /// </summary>
        public static RGB Invert(RGB color)
        {
            return new RGB(255 - color.R, 255 - color.G, 255 - color.B);
        }
    }

    /// <summary>
    /// RGB 颜色
    /// </summary>
    public readonly struct RGB
    {
        /// <summary>红 (0-255)</summary>
        public int R { get; }
        /// <summary>绿 (0-255)</summary>
        public int G { get; }
        /// <summary>蓝 (0-255)</summary>
        public int B { get; }

        public RGB(int r, int g, int b)
        {
            R = Math.Clamp(r, 0, 255);
            G = Math.Clamp(g, 0, 255);
            B = Math.Clamp(b, 0, 255);
        }

        public string ToHex() => ColorUtil.RGBToHex(R, G, B);

        public override string ToString() => $"RGB({R}, {G}, {B})";
    }

    /// <summary>
    /// HSL 颜色
    /// </summary>
    public readonly struct HSL
    {
        /// <summary>色相 (0-360)</summary>
        public double H { get; }
        /// <summary>饱和度 (0-100)</summary>
        public double S { get; }
        /// <summary>亮度 (0-100)</summary>
        public double L { get; }

        public HSL(double h, double s, double l)
        {
            H = ((h % 360) + 360) % 360;
            S = Math.Clamp(s, 0, 100);
            L = Math.Clamp(l, 0, 100);
        }

        public RGB ToRGB() => ColorUtil.HSLToRGB(H, S, L);

        public override string ToString() => $"HSL({H:F1}°, {S:F1}%, {L:F1}%)";
    }

    /// <summary>
    /// HSV 颜色
    /// </summary>
    public readonly struct HSV
    {
        /// <summary>色相 (0-360)</summary>
        public double H { get; }
        /// <summary>饱和度 (0-100)</summary>
        public double S { get; }
        /// <summary>明度 (0-100)</summary>
        public double V { get; }

        public HSV(double h, double s, double v)
        {
            H = ((h % 360) + 360) % 360;
            S = Math.Clamp(s, 0, 100);
            V = Math.Clamp(v, 0, 100);
        }

        public RGB ToRGB() => ColorUtil.HSVToRGB(H, S, V);

        public override string ToString() => $"HSV({H:F1}°, {S:F1}%, {V:F1}%)";
    }

    /// <summary>
    /// CMYK 颜色
    /// </summary>
    public readonly struct CMYK
    {
        /// <summary>青 (0-100)</summary>
        public double C { get; }
        /// <summary>品红 (0-100)</summary>
        public double M { get; }
        /// <summary>黄 (0-100)</summary>
        public double Y { get; }
        /// <summary>黑 (0-100)</summary>
        public double K { get; }

        public CMYK(double c, double m, double y, double k)
        {
            C = Math.Clamp(c, 0, 100);
            M = Math.Clamp(m, 0, 100);
            Y = Math.Clamp(y, 0, 100);
            K = Math.Clamp(k, 0, 100);
        }

        public RGB ToRGB() => ColorUtil.CMYKToRGB(C, M, Y, K);

        public override string ToString() => $"CMYK({C:F1}%, {M:F1}%, {Y:F1}%, {K:F1}%)";
    }

    /// <summary>
    /// 调色板工具类
    /// </summary>
    public static class ColorPaletteUtil
    {
        /// <summary>
        /// 生成类似色配色方案
        /// </summary>
        public static RGB[] GetAnalogous(RGB baseColor, int count = 3)
        {
            var hsl = ColorUtil.RGBToHSL(baseColor.R, baseColor.G, baseColor.B);
            var colors = new RGB[count];

            for (int i = 0; i < count; i++)
            {
                double h = (hsl.H + i * 30 - (count - 1) * 15 + 360) % 360;
                colors[i] = ColorUtil.HSLToRGB(h, hsl.S, hsl.L);
            }

            return colors;
        }

        /// <summary>
        /// 生成互补色配色方案
        /// </summary>
        public static RGB[] GetComplementary(RGB baseColor)
        {
            return new[] { baseColor, ColorUtil.GetComplementary(baseColor) };
        }

        /// <summary>
        /// 生成三色配色方案
        /// </summary>
        public static RGB[] GetTriadic(RGB baseColor)
        {
            var hsl = ColorUtil.RGBToHSL(baseColor.R, baseColor.G, baseColor.B);

            return new[]
            {
                baseColor,
                ColorUtil.HSLToRGB((hsl.H + 120) % 360, hsl.S, hsl.L),
                ColorUtil.HSLToRGB((hsl.H + 240) % 360, hsl.S, hsl.L)
            };
        }

        /// <summary>
        /// 生成四色配色方案
        /// </summary>
        public static RGB[] GetTetradic(RGB baseColor)
        {
            var hsl = ColorUtil.RGBToHSL(baseColor.R, baseColor.G, baseColor.B);

            return new[]
            {
                baseColor,
                ColorUtil.HSLToRGB((hsl.H + 90) % 360, hsl.S, hsl.L),
                ColorUtil.HSLToRGB((hsl.H + 180) % 360, hsl.S, hsl.L),
                ColorUtil.HSLToRGB((hsl.H + 270) % 360, hsl.S, hsl.L)
            };
        }

        /// <summary>
        /// 生成单色配色方案
        /// </summary>
        public static RGB[] GetMonochromatic(RGB baseColor, int count = 5)
        {
            var hsl = ColorUtil.RGBToHSL(baseColor.R, baseColor.G, baseColor.B);
            var colors = new RGB[count];

            for (int i = 0; i < count; i++)
            {
                double l = 20 + i * (60.0 / (count - 1));
                colors[i] = ColorUtil.HSLToRGB(hsl.H, hsl.S, l);
            }

            return colors;
        }

        /// <summary>
        /// 生成分裂互补色配色方案
        /// </summary>
        public static RGB[] GetSplitComplementary(RGB baseColor)
        {
            var hsl = ColorUtil.RGBToHSL(baseColor.R, baseColor.G, baseColor.B);

            return new[]
            {
                baseColor,
                ColorUtil.HSLToRGB((hsl.H + 150) % 360, hsl.S, hsl.L),
                ColorUtil.HSLToRGB((hsl.H + 210) % 360, hsl.S, hsl.L)
            };
        }
    }
}
