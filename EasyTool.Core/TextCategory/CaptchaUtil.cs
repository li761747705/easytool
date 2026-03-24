using System;
using System.IO;
using System.Text;

namespace EasyTool.TextCategory
{
    /// <summary>
    /// 验证码类型
    /// </summary>
    public enum CaptchaType
    {
        /// <summary>
        /// 纯数字
        /// </summary>
        Numeric,

        /// <summary>
        /// 纯字母
        /// </summary>
        Alpha,

        /// <summary>
        /// 字母数字混合
        /// </summary>
        Alphanumeric,

        /// <summary>
        /// 算术运算
        /// </summary>
        Arithmetic
    }

    /// <summary>
    /// 验证码结果
    /// </summary>
    public class CaptchaResult
    {
        /// <summary>
        /// 验证码文本（算术验证码为答案）
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// 验证码图片（Base64格式）
        /// </summary>
        public string ImageBase64 { get; set; } = string.Empty;

        /// <summary>
        /// 验证码图片（字节数组）
        /// </summary>
        public byte[] ImageBytes { get; set; } = Array.Empty<byte>();

        /// <summary>
        /// 算术表达式（仅算术验证码）
        /// </summary>
        public string? Expression { get; set; }
    }

    /// <summary>
    /// 图形验证码工具类
    /// 使用简单的图形绘制生成验证码
    /// </summary>
    public static class CaptchaUtil
    {
        private static readonly char[] NumericChars = "0123456789".ToCharArray();
        private static readonly char[] AlphaChars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz".ToCharArray();
        private static readonly char[] AlphanumericChars = "0123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz".ToCharArray();
        private static readonly char[] OperatorChars = "+-x".ToCharArray();

        private static readonly string[] Colors = {
            "#2E4057", "#048A81", "#54C6EB", "#8EE3EF", "#F7717D",
            "#6B4E71", "#3D5A80", "#98C1D9", "#E0FBFC", "#EE6C4D"
        };

        private static readonly Random _random = new();

        /// <summary>
        /// 生成验证码
        /// </summary>
        /// <param name="length">验证码长度（默认4）</param>
        /// <param name="type">验证码类型</param>
        /// <param name="width">图片宽度（默认120）</param>
        /// <param name="height">图片高度（默认40）</param>
        /// <returns>验证码结果</returns>
        public static CaptchaResult Generate(
            int length = 4,
            CaptchaType type = CaptchaType.Alphanumeric,
            int width = 120,
            int height = 40)
        {
            string code;
            string? expression = null;

            if (type == CaptchaType.Arithmetic)
            {
                (code, expression) = GenerateArithmeticCode();
            }
            else
            {
                code = GenerateCode(length, type);
            }

            var bytes = GenerateImage(code, width, height);

            return new CaptchaResult
            {
                Code = code,
                ImageBytes = bytes,
                ImageBase64 = Convert.ToBase64String(bytes),
                Expression = expression
            };
        }

        /// <summary>
        /// 生成验证码并返回 Base64 图片
        /// </summary>
        /// <param name="length">验证码长度</param>
        /// <param name="type">验证码类型</param>
        /// <param name="width">图片宽度</param>
        /// <param name="height">图片高度</param>
        /// <returns>Base64 格式图片</returns>
        public static string GenerateBase64(
            int length = 4,
            CaptchaType type = CaptchaType.Alphanumeric,
            int width = 120,
            int height = 40)
        {
            return Generate(length, type, width, height).ImageBase64;
        }

        /// <summary>
        /// 生成验证码并返回字节数组
        /// </summary>
        /// <param name="code">验证码文本</param>
        /// <param name="width">图片宽度</param>
        /// <param name="height">图片高度</param>
        /// <returns>PNG 图片字节数组</returns>
        public static byte[] GenerateImage(string code, int width = 120, int height = 40)
        {
            // 使用简单的 SVG 方式生成验证码图片
            // 这种方式不依赖外部库，兼容性好
            var svg = GenerateSvg(code, width, height);
            return Encoding.UTF8.GetBytes(svg);
        }

        /// <summary>
        /// 生成 SVG 格式的验证码图片
        /// </summary>
        private static string GenerateSvg(string code, int width, int height)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"{width}\" height=\"{height}\">");

            // 背景
            sb.AppendLine($"<rect width=\"100%\" height=\"100%\" fill=\"{GetRandomColor(true)}\"/>");

            // 干扰线
            for (int i = 0; i < 6; i++)
            {
                var x1 = _random.Next(width);
                var y1 = _random.Next(height);
                var x2 = _random.Next(width);
                var y2 = _random.Next(height);
                sb.AppendLine($"<line x1=\"{x1}\" y1=\"{y1}\" x2=\"{x2}\" y2=\"{y2}\" stroke=\"{GetRandomColor(false)}\" stroke-width=\"1\" opacity=\"0.5\"/>");
            }

            // 干扰点
            for (int i = 0; i < 50; i++)
            {
                var x = _random.Next(width);
                var y = _random.Next(height);
                sb.AppendLine($"<circle cx=\"{x}\" cy=\"{y}\" r=\"1\" fill=\"{GetRandomColor(false)}\" opacity=\"0.6\"/>");
            }

            // 文字
            int charWidth = width / (code.Length + 1);
            for (int i = 0; i < code.Length; i++)
            {
                var x = charWidth * (i + 1);
                var y = height / 2 + _random.Next(-5, 5);
                var fontSize = 20 + _random.Next(-3, 3);
                var rotation = _random.Next(-20, 20);
                var color = GetRandomColor(false);

                sb.AppendLine($"<text x=\"{x}\" y=\"{y}\" font-family=\"Arial, sans-serif\" font-size=\"{fontSize}\" font-weight=\"bold\" fill=\"{color}\" text-anchor=\"middle\" transform=\"rotate({rotation} {x} {y})\">{code[i]}</text>");
            }

            sb.AppendLine("</svg>");
            return sb.ToString();
        }

        /// <summary>
        /// 生成验证码文本
        /// </summary>
        private static string GenerateCode(int length, CaptchaType type)
        {
            var chars = type switch
            {
                CaptchaType.Numeric => NumericChars,
                CaptchaType.Alpha => AlphaChars,
                CaptchaType.Alphanumeric => AlphanumericChars,
                _ => AlphanumericChars
            };

            var sb = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                sb.Append(chars[_random.Next(chars.Length)]);
            }
            return sb.ToString();
        }

        /// <summary>
        /// 生成算术验证码
        /// </summary>
        private static (string Answer, string Expression) GenerateArithmeticCode()
        {
            int a = _random.Next(1, 20);
            int b = _random.Next(1, 20);
            var op = OperatorChars[_random.Next(OperatorChars.Length)];

            int answer;
            string expression;

            switch (op)
            {
                case '+':
                    answer = a + b;
                    expression = $"{a} + {b} = ?";
                    break;
                case '-':
                    // 确保结果为正
                    if (a < b) (a, b) = (b, a);
                    answer = a - b;
                    expression = $"{a} - {b} = ?";
                    break;
                case 'x':
                    a = _random.Next(1, 10);
                    b = _random.Next(1, 10);
                    answer = a * b;
                    expression = $"{a} × {b} = ?";
                    break;
                default:
                    answer = a + b;
                    expression = $"{a} + {b} = ?";
                    break;
            }

            return (answer.ToString(), expression);
        }

        /// <summary>
        /// 获取随机颜色
        /// </summary>
        private static string GetRandomColor(bool light = false)
        {
            if (light)
            {
                // 浅色背景
                var r = 200 + _random.Next(56);
                var g = 200 + _random.Next(56);
                var b = 200 + _random.Next(56);
                return $"rgb({r},{g},{b})";
            }
            else
            {
                // 深色文字/干扰
                return Colors[_random.Next(Colors.Length)];
            }
        }

        /// <summary>
        /// 验证码校验（忽略大小写）
        /// </summary>
        /// <param name="input">用户输入</param>
        /// <param name="code">正确验证码</param>
        /// <param name="ignoreCase">是否忽略大小写</param>
        /// <returns>是否匹配</returns>
        public static bool Verify(string input, string code, bool ignoreCase = true)
        {
            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(code))
                return false;

            return ignoreCase
                ? string.Equals(input, code, StringComparison.OrdinalIgnoreCase)
                : input == code;
        }

        /// <summary>
        /// 生成指定长度的随机数字验证码
        /// </summary>
        /// <param name="length">长度</param>
        /// <returns>验证码</returns>
        public static string GenerateNumericCode(int length = 6)
        {
            return GenerateCode(length, CaptchaType.Numeric);
        }

        /// <summary>
        /// 生成短信验证码（6位数字）
        /// </summary>
        /// <returns>6位数字验证码</returns>
        public static string GenerateSmsCode()
        {
            return GenerateNumericCode(6);
        }

        /// <summary>
        /// 生成邮箱验证码（6位数字）
        /// </summary>
        /// <returns>6位数字验证码</returns>
        public static string GenerateEmailCode()
        {
            return GenerateNumericCode(6);
        }
    }
}
