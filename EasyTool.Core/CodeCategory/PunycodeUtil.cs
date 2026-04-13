using System;
using System.Text;

namespace EasyTool.CodeCategory
{
    /// <summary>
    /// Punycode 编码工具类
    /// Punycode 是一种将 Unicode 字符串转换为 ASCII 的编码方案
    /// 主要用于国际化域名（IDN），如 "例子.测试" → "xn--fsqu00a.xn--0zwm56d"
    /// RFC 3492 标准
    /// </summary>
    public static class PunycodeUtil
    {
        private const int Base = 36;
        private const int TMin = 1;
        private const int TMax = 26;
        private const int Skew = 38;
        private const int Damp = 700;
        private const int InitialBias = 72;
        private const int InitialN = 0x80;
        private const int Delimiter = '-';

        private const string Base36Chars = "abcdefghijklmnopqrstuvwxyz0123456789";

        /// <summary>
        /// 将 Unicode 字符串编码为 Punycode
        /// </summary>
        /// <param name="input">Unicode 字符串</param>
        /// <returns>Punycode 编码字符串</returns>
        public static string Encode(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            // 检查是否全是 ASCII
            bool allAscii = true;
            foreach (char c in input)
            {
                if (c > 0x7F)
                {
                    allAscii = false;
                    break;
                }
            }

            if (allAscii)
                return input;

            var result = new StringBuilder();
            int n = InitialN;
            int delta = 0;
            int bias = InitialBias;
            int h = 0;

            // 处理基本字符（ASCII）
            foreach (char c in input)
            {
                if (c < 0x80)
                {
                    result.Append(c);
                    h++;
                }
            }

            int b = h;
            if (b > 0)
            {
                result.Append((char)Delimiter);
            }

            int inputLength = input.Length;
            int m = 0;

            while (h < inputLength)
            {
                // 找到最小的非基本字符
                m = int.MaxValue;
                foreach (char c in input)
                {
                    if (c >= n && c < m)
                    {
                        m = c;
                    }
                }

                delta += (m - n) * (h + 1);
                n = m;

                foreach (char c in input)
                {
                    if (c < n)
                    {
                        delta++;
                    }
                    else if (c == n)
                    {
                        int q = delta;
                        int k = Base;

                        while (true)
                        {
                            int t = k <= bias ? TMin : (k >= bias + TMax ? TMax : k - bias);
                            if (q < t)
                                break;

                            result.Append(Base36Chars[t + (q - t) % (Base - t)]);
                            q = (q - t) / (Base - t);
                            k += Base;
                        }

                        result.Append(Base36Chars[q]);
                        bias = Adapt(delta, h + 1, h == b);
                        delta = 0;
                        h++;
                    }
                }

                delta++;
                n++;
            }

            return result.ToString();
        }

        /// <summary>
        /// 将 Punycode 字符串解码为 Unicode
        /// </summary>
        /// <param name="input">Punycode 编码字符串</param>
        /// <returns>Unicode 字符串</returns>
        public static string Decode(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            // 查找分隔符位置
            int delimiterPos = input.LastIndexOf((char)Delimiter);

            var result = new StringBuilder();

            // 处理基本字符
            if (delimiterPos > 0)
            {
                for (int idx = 0; idx < delimiterPos; idx++)
                {
                    char c = input[idx];
                    if (c < 0x80)
                    {
                        result.Append(c);
                    }
                    else
                    {
                        throw new ArgumentException("Invalid Punycode string: non-ASCII character in basic part");
                    }
                }
            }

            int i = 0;
            int n = InitialN;
            int bias = InitialBias;
            int pos = delimiterPos + 1;

            while (pos < input.Length)
            {
                int oldi = i;
                int w = 1;

                for (int k = Base; ; k += Base)
                {
                    if (pos >= input.Length)
                        throw new ArgumentException("Invalid Punycode string: unexpected end");

                    char c = input[pos++];
                    int digit = DecodeDigit(c);

                    if (digit > (int.MaxValue - i) / w)
                        throw new ArgumentException("Invalid Punycode string: overflow");

                    i += digit * w;

                    int t = k <= bias ? TMin : (k >= bias + TMax ? TMax : k - bias);

                    if (digit < t)
                        break;

                    if (w > int.MaxValue / (Base - t))
                        throw new ArgumentException("Invalid Punycode string: overflow");

                    w *= (Base - t);
                }

                bias = Adapt(i - oldi, result.Length + 1, oldi == 0);

                if (i / (result.Length + 1) > int.MaxValue - n)
                    throw new ArgumentException("Invalid Punycode string: overflow");

                n += i / (result.Length + 1);
                i %= (result.Length + 1);

                result.Insert(i, (char)n);
                i++;
            }

            return result.ToString();
        }

        /// <summary>
        /// 将域名编码为 IDN 格式（带 xn-- 前缀）
        /// </summary>
        /// <param name="domain">Unicode 域名</param>
        /// <returns>ASCII 域名</returns>
        public static string EncodeDomain(string domain)
        {
            if (string.IsNullOrEmpty(domain))
                return string.Empty;

            var parts = domain.Split('.');
            var result = new StringBuilder();

            for (int i = 0; i < parts.Length; i++)
            {
                if (i > 0)
                    result.Append('.');

                string encoded = Encode(parts[i]);

                // 如果包含非 ASCII 字符，添加 xn-- 前缀
                bool needsPrefix = false;
                foreach (char c in parts[i])
                {
                    if (c > 0x7F)
                    {
                        needsPrefix = true;
                        break;
                    }
                }

                if (needsPrefix)
                {
                    result.Append("xn--");
                    result.Append(encoded);
                }
                else
                {
                    result.Append(parts[i]);
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// 将 IDN 域名解码为 Unicode 格式
        /// </summary>
        /// <param name="domain">ASCII 域名</param>
        /// <returns>Unicode 域名</returns>
        public static string DecodeDomain(string domain)
        {
            if (string.IsNullOrEmpty(domain))
                return string.Empty;

            var parts = domain.Split('.');
            var result = new StringBuilder();

            for (int i = 0; i < parts.Length; i++)
            {
                if (i > 0)
                    result.Append('.');

                string part = parts[i];

                // 检查是否有 xn-- 前缀（不区分大小写）
                if (part.Length > 4 &&
                    part.StartsWith("xn--", StringComparison.OrdinalIgnoreCase))
                {
                    string punycode = part.Substring(4);
                    result.Append(Decode(punycode));
                }
                else
                {
                    result.Append(part);
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// 验证 Punycode 字符串是否有效
        /// </summary>
        /// <param name="input">Punycode 字符串</param>
        /// <returns>是否有效</returns>
        public static bool IsValid(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;

            try
            {
                string decoded = Decode(input);
                string reencoded = Encode(decoded);
                return true;
            }
            // 捕获 Punycode 编解码格式异常
            catch (FormatException)
            {
                return false;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        /// <summary>
        /// 尝试解码 Punycode 字符串
        /// </summary>
        /// <param name="input">Punycode 字符串</param>
        /// <param name="result">解码结果</param>
        /// <returns>是否解码成功</returns>
        public static bool TryDecode(string input, out string result)
        {
            result = null;

            if (string.IsNullOrEmpty(input))
            {
                result = string.Empty;
                return true;
            }

            try
            {
                result = Decode(input);
                return true;
            }
            // 捕获 Punycode 解码格式异常
            catch (FormatException)
            {
                return false;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        #region 私有方法

        private static int Adapt(int delta, int numpoints, bool firsttime)
        {
            delta = firsttime ? delta / Damp : delta / 2;
            delta += delta / numpoints;

            int k = 0;
            while (delta > ((Base - TMin) * TMax) / 2)
            {
                delta /= Base - TMin;
                k += Base;
            }

            return k + (Base - TMin + 1) * delta / (delta + Skew);
        }

        private static int DecodeDigit(char c)
        {
            if (c >= 'a' && c <= 'z')
                return c - 'a';
            if (c >= 'A' && c <= 'Z')
                return c - 'A';
            if (c >= '0' && c <= '9')
                return c - '0' + 26;

            throw new ArgumentException($"Invalid Punycode character: {c}");
        }

        #endregion
    }
}
