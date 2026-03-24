using System;
using System.Text;

namespace EasyTool.CodeCategory
{
    /// <summary>
    /// Base64URL 编码工具类
    /// Base64URL 是 URL 和文件名安全的 Base64 编码变体
    /// 使用 - 代替 +，_ 代替 /，通常省略填充
    /// 用于 JWT、URL 参数等场景
    /// RFC 4648 标准
    /// </summary>
    public static class Base64UrlUtil
    {
        private const string Base64UrlChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_";
        private const string Base64Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";

        /// <summary>
        /// 将字节数组编码为 Base64URL 字符串
        /// </summary>
        /// <param name="data">要编码的数据</param>
        /// <param name="padding">是否添加填充</param>
        /// <returns>Base64URL 编码字符串</returns>
        public static string Encode(byte[] data, bool padding = false)
        {
            if (data == null || data.Length == 0)
                return string.Empty;

            // 先使用标准 Base64 编码
            string base64 = Convert.ToBase64String(data);

            // 转换为 Base64URL
            var result = new StringBuilder(base64.Length);
            foreach (char c in base64)
            {
                if (c == '+')
                    result.Append('-');
                else if (c == '/')
                    result.Append('_');
                else if (c == '=')
                {
                    if (padding)
                        result.Append(c);
                }
                else
                {
                    result.Append(c);
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// 将 Base64URL 字符串解码为字节数组
        /// </summary>
        /// <param name="encoded">Base64URL 编码字符串</param>
        /// <returns>解码后的字节数组</returns>
        public static byte[] Decode(string encoded)
        {
            if (string.IsNullOrEmpty(encoded))
                return Array.Empty<byte>();

            // 转换回标准 Base64
            var base64 = new StringBuilder(encoded.Length);

            foreach (char c in encoded)
            {
                if (c == '-')
                    base64.Append('+');
                else if (c == '_')
                    base64.Append('/');
                else
                    base64.Append(c);
            }

            // 添加填充
            int padding = base64.Length % 4;
            if (padding > 0)
            {
                base64.Append(new string('=', 4 - padding));
            }

            return Convert.FromBase64String(base64.ToString());
        }

        /// <summary>
        /// 将字符串编码为 Base64URL（使用 UTF-8）
        /// </summary>
        /// <param name="text">文本</param>
        /// <param name="padding">是否添加填充</param>
        /// <returns>Base64URL 编码字符串</returns>
        public static string EncodeString(string text, bool padding = false)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            byte[] bytes = Encoding.UTF8.GetBytes(text);
            return Encode(bytes, padding);
        }

        /// <summary>
        /// 将 Base64URL 字符串解码为文本（使用 UTF-8）
        /// </summary>
        /// <param name="encoded">Base64URL 编码字符串</param>
        /// <returns>解码后的文本</returns>
        public static string DecodeToString(string encoded)
        {
            if (string.IsNullOrEmpty(encoded))
                return string.Empty;

            byte[] bytes = Decode(encoded);
            return Encoding.UTF8.GetString(bytes);
        }

        /// <summary>
        /// 验证 Base64URL 字符串是否有效
        /// </summary>
        /// <param name="encoded">Base64URL 编码字符串</param>
        /// <returns>是否有效</returns>
        public static bool IsValid(string encoded)
        {
            if (string.IsNullOrEmpty(encoded))
                return false;

            // 检查长度
            if (encoded.Length % 4 == 1)
                return false;

            // 检查字符
            foreach (char c in encoded)
            {
                if (c >= 'A' && c <= 'Z') continue;
                if (c >= 'a' && c <= 'z') continue;
                if (c >= '0' && c <= '9') continue;
                if (c == '-' || c == '_') continue;
                if (c == '=')
                {
                    // 填充只能在末尾
                    int index = encoded.IndexOf('=');
                    if (index < encoded.Length - 2)
                        return false;
                    continue;
                }

                return false;
            }

            return true;
        }

        /// <summary>
        /// 尝试解码 Base64URL 字符串
        /// </summary>
        /// <param name="encoded">Base64URL 编码字符串</param>
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
        /// 从标准 Base64 转换为 Base64URL
        /// </summary>
        /// <param name="base64">标准 Base64 字符串</param>
        /// <param name="removePadding">是否移除填充</param>
        /// <returns>Base64URL 字符串</returns>
        public static string FromBase64(string base64, bool removePadding = true)
        {
            if (string.IsNullOrEmpty(base64))
                return string.Empty;

            var result = new StringBuilder(base64.Length);

            foreach (char c in base64)
            {
                if (c == '+')
                    result.Append('-');
                else if (c == '/')
                    result.Append('_');
                else if (c == '=' && removePadding)
                    continue;
                else
                    result.Append(c);
            }

            return result.ToString();
        }

        /// <summary>
        /// 从 Base64URL 转换为标准 Base64
        /// </summary>
        /// <param name="base64Url">Base64URL 字符串</param>
        /// <returns>标准 Base64 字符串</returns>
        public static string ToBase64(string base64Url)
        {
            if (string.IsNullOrEmpty(base64Url))
                return string.Empty;

            var result = new StringBuilder(base64Url);

            for (int i = 0; i < result.Length; i++)
            {
                if (result[i] == '-')
                    result[i] = '+';
                else if (result[i] == '_')
                    result[i] = '/';
            }

            // 添加填充
            int padding = result.Length % 4;
            if (padding > 0)
            {
                result.Append(new string('=', 4 - padding));
            }

            return result.ToString();
        }

        /// <summary>
        /// 计算 Base64URL 编码后的预计长度
        /// </summary>
        /// <param name="inputLength">输入数据长度</param>
        /// <param name="includePadding">是否包含填充</param>
        /// <returns>预计输出长度</returns>
        public static int CalculateEncodedLength(int inputLength, bool includePadding = false)
        {
            if (inputLength == 0)
                return 0;

            int length = (inputLength + 2) / 3 * 4;

            if (!includePadding)
            {
                // 移除填充
                int remainder = inputLength % 3;
                if (remainder > 0)
                    length -= 3 - remainder;
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

            return encodedLength * 3 / 4;
        }

        /// <summary>
        /// 比较两个 Base64URL 字符串是否相等（常量时间）
        /// </summary>
        /// <param name="a">第一个字符串</param>
        /// <param name="b">第二个字符串</param>
        /// <returns>是否相等</returns>
        public static bool ConstantTimeEquals(string a, string b)
        {
            if (a == null || b == null)
                return a == b;

            if (a.Length != b.Length)
                return false;

            int result = 0;
            for (int i = 0; i < a.Length; i++)
            {
                result |= a[i] ^ b[i];
            }

            return result == 0;
        }
    }
}
