using System;
using System.Text;

namespace EasyTool.CodeCategory
{
    /// <summary>
    /// Base45 编码工具类
    /// Base45 是一种使用 45 个字符的二进制到文本编码方案
    /// 用于 QR 码、疫苗证书（EU Digital COVID Certificate）等场景
    /// RFC 9285 标准
    /// </summary>
    public static class Base45Util
    {
        // Base45 字符集（RFC 9285）
        private const string Base45Chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ $%*+-./:";

        // 解码映射
        private static readonly int[] DecodeMap;

        static Base45Util()
        {
            DecodeMap = new int[128];
            for (int i = 0; i < 128; i++)
                DecodeMap[i] = -1;

            for (int i = 0; i < Base45Chars.Length; i++)
            {
                DecodeMap[Base45Chars[i]] = i;
            }
        }

        /// <summary>
        /// 将字节数组编码为 Base45 字符串
        /// </summary>
        /// <param name="data">要编码的数据</param>
        /// <returns>Base45 编码字符串</returns>
        public static string Encode(byte[] data)
        {
            if (data == null || data.Length == 0)
                return string.Empty;

            var result = new StringBuilder();

            for (int i = 0; i < data.Length; i += 2)
            {
                if (i + 1 < data.Length)
                {
                    // 2 字节 -> 3 字符
                    uint value = (uint)((data[i] << 8) | data[i + 1]);
                    result.Append(Base45Chars[(int)(value % 45)]);
                    result.Append(Base45Chars[(int)((value / 45) % 45)]);
                    result.Append(Base45Chars[(int)((value / 2025) % 45)]);
                }
                else
                {
                    // 1 字节 -> 2 字符
                    uint value = data[i];
                    result.Append(Base45Chars[(int)(value % 45)]);
                    result.Append(Base45Chars[(int)((value / 45) % 45)]);
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// 将 Base45 字符串解码为字节数组
        /// </summary>
        /// <param name="encoded">Base45 编码字符串</param>
        /// <returns>解码后的字节数组</returns>
        public static byte[] Decode(string encoded)
        {
            if (string.IsNullOrEmpty(encoded))
                return Array.Empty<byte>();

            // 验证长度
            if (encoded.Length % 3 == 1)
                throw new ArgumentException("Invalid Base45 string length", nameof(encoded));

            var result = new System.Collections.Generic.List<byte>();

            for (int i = 0; i < encoded.Length; i += 3)
            {
                if (i + 2 < encoded.Length)
                {
                    // 3 字符 -> 2 字节
                    int c0 = DecodeChar(encoded[i]);
                    int c1 = DecodeChar(encoded[i + 1]);
                    int c2 = DecodeChar(encoded[i + 2]);

                    uint value = (uint)(c0 + 45 * c1 + 2025 * c2);

                    if (value > 65535)
                        throw new ArgumentException("Invalid Base45 encoding", nameof(encoded));

                    result.Add((byte)(value >> 8));
                    result.Add((byte)(value & 0xFF));
                }
                else
                {
                    // 2 字符 -> 1 字节
                    int c0 = DecodeChar(encoded[i]);
                    int c1 = DecodeChar(encoded[i + 1]);

                    uint value = (uint)(c0 + 45 * c1);

                    if (value > 255)
                        throw new ArgumentException("Invalid Base45 encoding", nameof(encoded));

                    result.Add((byte)value);
                }
            }

            return result.ToArray();
        }

        /// <summary>
        /// 将字符串编码为 Base45（使用 UTF-8）
        /// </summary>
        /// <param name="text">文本</param>
        /// <returns>Base45 编码字符串</returns>
        public static string EncodeString(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            byte[] bytes = Encoding.UTF8.GetBytes(text);
            return Encode(bytes);
        }

        /// <summary>
        /// 将 Base45 字符串解码为文本（使用 UTF-8）
        /// </summary>
        /// <param name="encoded">Base45 编码字符串</param>
        /// <returns>解码后的文本</returns>
        public static string DecodeToString(string encoded)
        {
            if (string.IsNullOrEmpty(encoded))
                return string.Empty;

            byte[] bytes = Decode(encoded);
            return Encoding.UTF8.GetString(bytes);
        }

        /// <summary>
        /// 验证 Base45 字符串是否有效
        /// </summary>
        /// <param name="encoded">Base45 编码字符串</param>
        /// <returns>是否有效</returns>
        public static bool IsValid(string encoded)
        {
            if (string.IsNullOrEmpty(encoded))
                return false;

            // 长度检查
            if (encoded.Length % 3 == 1)
                return false;

            foreach (char c in encoded)
            {
                if (c >= 128 || DecodeMap[c] < 0)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 尝试解码 Base45 字符串
        /// </summary>
        /// <param name="encoded">Base45 编码字符串</param>
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
        /// 计算 Base45 编码后的预计长度
        /// </summary>
        /// <param name="inputLength">输入数据长度</param>
        /// <returns>预计输出长度</returns>
        public static int CalculateEncodedLength(int inputLength)
        {
            if (inputLength == 0)
                return 0;

            // 每 2 字节编码为 3 字符，最后可能剩 1 字节编码为 2 字符
            int fullGroups = inputLength / 2;
            int remaining = inputLength % 2;

            return fullGroups * 3 + remaining * 2;
        }

        /// <summary>
        /// 计算解码后的预计长度
        /// </summary>
        /// <param name="encodedLength">编码字符串长度</param>
        /// <returns>预计解码后长度</returns>
        public static int CalculateDecodedLength(int encodedLength)
        {
            if (encodedLength == 0)
                return 0;

            // 每 3 字符解码为 2 字节
            int fullGroups = encodedLength / 3;
            int remaining = encodedLength % 3;

            return fullGroups * 2 + (remaining == 2 ? 1 : 0);
        }

        private static int DecodeChar(char c)
        {
            if (c >= 128 || DecodeMap[c] < 0)
                throw new ArgumentException($"Invalid Base45 character: {c}", "encoded");

            return DecodeMap[c];
        }
    }
}
