using System;
using System.Text;

namespace EasyTool.CodeCategory
{
    /// <summary>
    /// Base92 编码工具类
    /// Base92 是一种二进制到文本的编码方案，比 Base85 更高效
    /// 使用 92 个可打印 ASCII 字符
    /// </summary>
    public static class Base92Util
    {
        // Base92 字符集
        private const string Base92Chars = "!#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_abcdefghijklmnopqrstuvwxyz{|}~\"";

        // 解码映射表
        private static readonly int[] DecodeMap;

        static Base92Util()
        {
            DecodeMap = new int[256];
            for (int i = 0; i < 256; i++)
            {
                DecodeMap[i] = -1;
            }
            for (int i = 0; i < Base92Chars.Length; i++)
            {
                DecodeMap[Base92Chars[i]] = i;
            }
        }

        /// <summary>
        /// 将字节数组编码为 Base92 字符串
        /// </summary>
        /// <param name="data">要编码的数据</param>
        /// <returns>Base92 编码字符串</returns>
        public static string Encode(byte[] data)
        {
            if (data == null || data.Length == 0)
                return "~";

            var result = new StringBuilder();
            int bitBuffer = 0;
            int bitsInBuffer = 0;

            foreach (byte b in data)
            {
                bitBuffer = (bitBuffer << 8) | b;
                bitsInBuffer += 8;

                while (bitsInBuffer >= 13)
                {
                    int value = (bitBuffer >> (bitsInBuffer - 13)) & 0x1FFF;

                    if (value < 91)
                    {
                        result.Append(Base92Chars[value]);
                        bitsInBuffer -= 13;
                    }
                    else
                    {
                        value -= 91;
                        result.Append(Base92Chars[value / 91 + 91]);
                        result.Append(Base92Chars[value % 91]);
                        bitsInBuffer -= 14;
                    }
                }
            }

            // 处理剩余位
            if (bitsInBuffer > 0)
            {
                int value = (bitBuffer << (13 - bitsInBuffer)) & 0x1FFF;

                if (value < 91)
                {
                    result.Append(Base92Chars[value]);
                }
                else
                {
                    value -= 91;
                    result.Append(Base92Chars[value / 91 + 91]);
                    result.Append(Base92Chars[value % 91]);
                }
            }

            result.Append('~');

            return result.ToString();
        }

        /// <summary>
        /// 将 Base92 字符串解码为字节数组
        /// </summary>
        /// <param name="encoded">Base92 编码字符串</param>
        /// <returns>解码后的字节数组</returns>
        public static byte[] Decode(string encoded)
        {
            if (string.IsNullOrEmpty(encoded))
                return Array.Empty<byte>();

            if (!encoded.EndsWith("~"))
                throw new ArgumentException("Invalid Base92 string: must end with ~", nameof(encoded));

            string data = encoded.TrimEnd('~');
            if (string.IsNullOrEmpty(data))
                return Array.Empty<byte>();

            var result = new System.Collections.Generic.List<byte>();
            int bitBuffer = 0;
            int bitsInBuffer = 0;

            int i = 0;
            while (i < data.Length)
            {
                int value;

                int c1 = data[i] < 256 ? DecodeMap[data[i]] : -1;
                if (c1 < 0)
                    throw new ArgumentException($"无效的 Base92 字符: {data[i]}", nameof(encoded));

                if (c1 < 91)
                {
                    value = c1;
                    i++;
                }
                else
                {
                    if (i + 1 >= data.Length)
                        throw new ArgumentException("Invalid Base92 string: unexpected end", nameof(encoded));

                    int c2 = data[i + 1] < 256 ? DecodeMap[data[i + 1]] : -1;
                    if (c2 < 0)
                        throw new ArgumentException($"无效的 Base92 字符: {data[i + 1]}", nameof(encoded));

                    value = (c1 - 91) * 91 + c2 + 91;
                    i += 2;
                }

                bitBuffer = (bitBuffer << 13) | value;
                bitsInBuffer += 13;

                while (bitsInBuffer >= 8)
                {
                    bitsInBuffer -= 8;
                    result.Add((byte)((bitBuffer >> bitsInBuffer) & 0xFF));
                }
            }

            return result.ToArray();
        }

        /// <summary>
        /// 将字符串编码为 Base92（使用 UTF-8）
        /// </summary>
        /// <param name="text">要编码的文本</param>
        /// <returns>Base92 编码字符串</returns>
        public static string EncodeString(string text)
        {
            if (string.IsNullOrEmpty(text))
                return "~";

            byte[] bytes = Encoding.UTF8.GetBytes(text);
            return Encode(bytes);
        }

        /// <summary>
        /// 将 Base92 字符串解码为文本（使用 UTF-8）
        /// </summary>
        /// <param name="encoded">Base92 编码字符串</param>
        /// <returns>解码后的文本</returns>
        public static string DecodeToString(string encoded)
        {
            if (string.IsNullOrEmpty(encoded) || encoded == "~")
                return string.Empty;

            byte[] bytes = Decode(encoded);
            return Encoding.UTF8.GetString(bytes);
        }

        /// <summary>
        /// 验证 Base92 字符串是否有效
        /// </summary>
        /// <param name="encoded">Base92 编码字符串</param>
        /// <returns>是否有效</returns>
        public static bool IsValid(string encoded)
        {
            if (string.IsNullOrEmpty(encoded))
                return false;

            if (!encoded.EndsWith("~"))
                return false;

            string data = encoded.TrimEnd('~');
            if (string.IsNullOrEmpty(data))
                return true;

            int i = 0;
            while (i < data.Length)
            {
                int c1 = data[i] < 256 ? DecodeMap[data[i]] : -1;
                if (c1 < 0)
                    return false;

                if (c1 < 91)
                {
                    i++;
                }
                else
                {
                    if (i + 1 >= data.Length)
                        return false;

                    int c2 = data[i + 1] < 256 ? DecodeMap[data[i + 1]] : -1;
                    if (c2 < 0)
                        return false;

                    i += 2;
                }
            }

            return true;
        }

        /// <summary>
        /// 尝试解码 Base92 字符串
        /// </summary>
        /// <param name="encoded">Base92 编码字符串</param>
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
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 计算 Base92 编码后的预计长度
        /// </summary>
        /// <param name="inputLength">输入数据长度</param>
        /// <returns>预计输出长度</returns>
        public static int CalculateEncodedLength(int inputLength)
        {
            if (inputLength == 0)
                return 1;

            // Base92 编码效率约为 16/13
            return (int)Math.Ceiling(inputLength * 16.0 / 13.0) + 1;
        }
    }
}
