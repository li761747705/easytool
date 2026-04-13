using System;
using System.Text;

namespace EasyTool.CodeCategory
{
    /// <summary>
    /// Base91 编码工具类
    /// Base91 是一种二进制到文本的编码方案，比 Base64 更高效
    /// 编码效率：约 23%（比 Base64 的 33% 更低开销）
    /// </summary>
    public static class Base91Util
    {
        // Base91 字符集
        private const string Base91Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!#$%&()*+,-./:;<=>?@[]^_`{|}~\"";

        // 解码映射表
        private static readonly int[] DecodeMap;

        static Base91Util()
        {
            DecodeMap = new int[256];
            for (int i = 0; i < 256; i++)
            {
                DecodeMap[i] = -1;
            }
            for (int i = 0; i < Base91Chars.Length; i++)
            {
                DecodeMap[Base91Chars[i]] = i;
            }
        }

        /// <summary>
        /// 将字节数组编码为 Base91 字符串
        /// </summary>
        /// <param name="data">要编码的数据</param>
        /// <returns>Base91 编码字符串</returns>
        public static string Encode(byte[] data)
        {
            if (data == null || data.Length == 0)
                return string.Empty;

            var result = new StringBuilder();
            int b = 0;
            int n = 0;

            foreach (byte c in data)
            {
                b |= c << n;
                n += 8;

                if (n > 13)
                {
                    int v = b & 8191;
                    if (v > 88)
                    {
                        b >>= 13;
                        n -= 13;
                    }
                    else
                    {
                        v = b & 16383;
                        b >>= 14;
                        n -= 14;
                    }
                    result.Append(Base91Chars[v % 91]);
                    result.Append(Base91Chars[v / 91]);
                }
            }

            if (n > 0)
            {
                result.Append(Base91Chars[b % 91]);
                if (n > 7 || b > 90)
                {
                    result.Append(Base91Chars[b / 91]);
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// 将 Base91 字符串解码为字节数组
        /// </summary>
        /// <param name="encoded">Base91 编码字符串</param>
        /// <returns>解码后的字节数组</returns>
        public static byte[] Decode(string encoded)
        {
            if (string.IsNullOrEmpty(encoded))
                return Array.Empty<byte>();

            var result = new System.Collections.Generic.List<byte>();
            int b = 0;
            int n = 0;
            int v = -1;

            foreach (char c in encoded)
            {
                if (c >= 256 || DecodeMap[c] == -1)
                    continue;

                if (v == -1)
                {
                    v = DecodeMap[c];
                }
                else
                {
                    v += DecodeMap[c] * 91;
                    b |= v << n;
                    n += (v & 8191) > 88 ? 13 : 14;

                    while (n > 7)
                    {
                        result.Add((byte)(b & 255));
                        b >>= 8;
                        n -= 8;
                    }

                    v = -1;
                }
            }

            if (v != -1)
            {
                result.Add((byte)((b | v << n) & 255));
            }

            return result.ToArray();
        }

        /// <summary>
        /// 将字符串编码为 Base91（使用 UTF-8）
        /// </summary>
        /// <param name="text">要编码的文本</param>
        /// <returns>Base91 编码字符串</returns>
        public static string EncodeString(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            byte[] bytes = Encoding.UTF8.GetBytes(text);
            return Encode(bytes);
        }

        /// <summary>
        /// 将 Base91 字符串解码为文本（使用 UTF-8）
        /// </summary>
        /// <param name="encoded">Base91 编码字符串</param>
        /// <returns>解码后的文本</returns>
        public static string DecodeToString(string encoded)
        {
            if (string.IsNullOrEmpty(encoded))
                return string.Empty;

            byte[] bytes = Decode(encoded);
            return Encoding.UTF8.GetString(bytes);
        }

        /// <summary>
        /// 验证 Base91 字符串是否有效
        /// </summary>
        /// <param name="encoded">Base91 编码字符串</param>
        /// <returns>是否有效</returns>
        public static bool IsValid(string encoded)
        {
            if (string.IsNullOrEmpty(encoded))
                return false;

            foreach (char c in encoded)
            {
                if (c >= 256 || DecodeMap[c] == -1)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 尝试解码 Base91 字符串
        /// </summary>
        /// <param name="encoded">Base91 编码字符串</param>
        /// <param name="result">解码后的字节数组</param>
        /// <returns>是否解码成功</returns>
        public static bool TryDecode(string encoded, out byte[] result)
        {
            result = null;

            if (!IsValid(encoded))
                return false;

            try
            {
                result = Decode(encoded);
                return true;
            }
            // 捕获 Base91 解码格式异常
            catch (FormatException)
            {
                return false;
            }
        }

        /// <summary>
        /// 计算 Base91 编码后的预计长度
        /// </summary>
        /// <param name="inputLength">输入数据长度</param>
        /// <returns>预计输出长度</returns>
        public static int CalculateEncodedLength(int inputLength)
        {
            if (inputLength == 0)
                return 0;

            // Base91 编码效率约为 23%
            return (int)Math.Ceiling(inputLength * 1.23);
        }
    }
}
