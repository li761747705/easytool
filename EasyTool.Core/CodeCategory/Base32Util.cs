using System;
using System.Text;

namespace EasyTool.CodeCategory
{
    /// <summary>
    /// Base32 编码工具类
    /// Base32 使用 32 个可打印字符（A-Z 和 2-7）编码二进制数据
    /// 常用于双因素认证密钥、文件名安全编码等场景
    /// 支持 RFC 4648 标准和 Crockford 编码
    /// </summary>
    public static class Base32Util
    {
        // RFC 4648 标准字符集
        private const string Rfc4648Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

        // Crockford 字符集（更友好，避免混淆字符）
        private const string CrockfordChars = "0123456789ABCDEFGHJKMNPQRSTVWXYZ";

        // 解码映射
        private static readonly int[] Rfc4648DecodeMap;
        private static readonly int[] CrockfordDecodeMap;

        static Base32Util()
        {
            Rfc4648DecodeMap = CreateDecodeMap(Rfc4648Chars);
            CrockfordDecodeMap = CreateDecodeMap(CrockfordChars);
        }

        private static int[] CreateDecodeMap(string chars)
        {
            var map = new int[128];
            for (int i = 0; i < 128; i++) map[i] = -1;

            for (int i = 0; i < chars.Length; i++)
            {
                map[chars[i]] = i;
                if (chars[i] >= 'A' && chars[i] <= 'Z')
                    map[chars[i] + 32] = i; // 小写映射
            }

            // Crockford 特殊映射
            if (chars == CrockfordChars)
            {
                map['O'] = map['o'] = 0;
                map['I'] = map['i'] = 1;
                map['L'] = map['l'] = 1;
            }

            return map;
        }

        #region RFC 4648 编码

        /// <summary>
        /// 使用 RFC 4648 标准编码为 Base32
        /// </summary>
        /// <param name="data">要编码的数据</param>
        /// <returns>Base32 编码字符串</returns>
        public static string Encode(byte[] data)
        {
            return Encode(data, Base32Format.Rfc4648);
        }

        /// <summary>
        /// 使用指定格式编码为 Base32
        /// </summary>
        /// <param name="data">要编码的数据</param>
        /// <param name="format">编码格式</param>
        /// <returns>Base32 编码字符串</returns>
        public static string Encode(byte[] data, Base32Format format)
        {
            if (data == null || data.Length == 0)
                return string.Empty;

            string chars = format == Base32Format.Crockford ? CrockfordChars : Rfc4648Chars;

            var result = new StringBuilder((data.Length * 8 + 4) / 5);
            int bits = 0;
            int value = 0;

            foreach (byte b in data)
            {
                value = (value << 8) | b;
                bits += 8;

                while (bits >= 5)
                {
                    result.Append(chars[(value >> (bits - 5)) & 0x1F]);
                    bits -= 5;
                }
            }

            if (bits > 0)
            {
                result.Append(chars[(value << (5 - bits)) & 0x1F]);
            }

            // RFC 4648 添加填充
            if (format == Base32Format.Rfc4648)
            {
                int padding = (8 - (result.Length % 8)) % 8;
                for (int i = 0; i < padding; i++)
                {
                    result.Append('=');
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// 解码 Base32 字符串
        /// </summary>
        /// <param name="encoded">Base32 编码字符串</param>
        /// <returns>解码后的字节数组</returns>
        public static byte[] Decode(string encoded)
        {
            return Decode(encoded, Base32Format.Rfc4648);
        }

        /// <summary>
        /// 使用指定格式解码 Base32 字符串
        /// </summary>
        /// <param name="encoded">Base32 编码字符串</param>
        /// <param name="format">编码格式</param>
        /// <returns>解码后的字节数组</returns>
        public static byte[] Decode(string encoded, Base32Format format)
        {
            if (string.IsNullOrEmpty(encoded))
                return Array.Empty<byte>();

            int[] decodeMap = format == Base32Format.Crockford ? CrockfordDecodeMap : Rfc4648DecodeMap;

            // 移除填充字符和空白
            encoded = encoded.TrimEnd('=').Replace(" ", "").Replace("-", "");

            var result = new System.Collections.Generic.List<byte>();
            int bits = 0;
            int value = 0;

            foreach (char c in encoded)
            {
                if (c >= 128 || decodeMap[c] < 0)
                    throw new ArgumentException($"Invalid Base32 character: {c}", nameof(encoded));

                value = (value << 5) | decodeMap[c];
                bits += 5;

                while (bits >= 8)
                {
                    result.Add((byte)((value >> (bits - 8)) & 0xFF));
                    bits -= 8;
                }
            }

            return result.ToArray();
        }

        #endregion

        #region 字符串编码

        /// <summary>
        /// 将字符串编码为 Base32（使用 UTF-8）
        /// </summary>
        /// <param name="text">文本</param>
        /// <param name="format">编码格式</param>
        /// <returns>Base32 编码字符串</returns>
        public static string EncodeString(string text, Base32Format format = Base32Format.Rfc4648)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            byte[] bytes = Encoding.UTF8.GetBytes(text);
            return Encode(bytes, format);
        }

        /// <summary>
        /// 将 Base32 字符串解码为文本（使用 UTF-8）
        /// </summary>
        /// <param name="encoded">Base32 编码字符串</param>
        /// <param name="format">编码格式</param>
        /// <returns>解码后的文本</returns>
        public static string DecodeToString(string encoded, Base32Format format = Base32Format.Rfc4648)
        {
            if (string.IsNullOrEmpty(encoded))
                return string.Empty;

            byte[] bytes = Decode(encoded, format);
            return Encoding.UTF8.GetString(bytes);
        }

        #endregion

        #region 验证

        /// <summary>
        /// 验证 Base32 字符串是否有效
        /// </summary>
        /// <param name="encoded">Base32 编码字符串</param>
        /// <param name="format">编码格式</param>
        /// <returns>是否有效</returns>
        public static bool IsValid(string encoded, Base32Format format = Base32Format.Rfc4648)
        {
            if (string.IsNullOrEmpty(encoded))
                return false;

            int[] decodeMap = format == Base32Format.Crockford ? CrockfordDecodeMap : Rfc4648DecodeMap;
            string validChars = format == Base32Format.Crockford ? CrockfordChars : Rfc4648Chars + "=";

            foreach (char c in encoded)
            {
                if (c == '=' || c == ' ' || c == '-')
                    continue;

                if (c >= 128 || decodeMap[c] < 0)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 尝试解码 Base32 字符串
        /// </summary>
        /// <param name="encoded">Base32 编码字符串</param>
        /// <param name="result">解码后的字节数组</param>
        /// <param name="format">编码格式</param>
        /// <returns>是否解码成功</returns>
        public static bool TryDecode(string encoded, out byte[] result, Base32Format format = Base32Format.Rfc4648)
        {
            result = null;

            if (!IsValid(encoded, format))
                return false;

            try
            {
                result = Decode(encoded, format);
                return true;
            }
            // 捕获 Base32 解码格式异常
            catch (FormatException)
            {
                return false;
            }
        }

        #endregion

        #region 计算长度

        /// <summary>
        /// 计算 Base32 编码后的预计长度
        /// </summary>
        /// <param name="inputLength">输入数据长度</param>
        /// <param name="includePadding">是否包含填充</param>
        /// <returns>预计输出长度</returns>
        public static int CalculateEncodedLength(int inputLength, bool includePadding = true)
        {
            if (inputLength == 0)
                return 0;

            int length = (inputLength * 8 + 4) / 5;

            if (includePadding)
            {
                length = ((length + 7) / 8) * 8;
            }

            return length;
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

            return encodedLength * 5 / 8;
        }

        #endregion
    }

    /// <summary>
    /// Base32 编码格式
    /// </summary>
    public enum Base32Format
    {
        /// <summary>
        /// RFC 4648 标准格式（使用 = 填充）
        /// </summary>
        Rfc4648,

        /// <summary>
        /// Crockford 格式（无填充，支持校验位）
        /// </summary>
        Crockford
    }
}
