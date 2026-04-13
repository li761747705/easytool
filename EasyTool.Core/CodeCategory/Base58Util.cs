using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace EasyTool.CodeCategory
{
    /// <summary>
    /// Base58 编解码工具类
    /// Base58 是一种用于 Bitcoin 地址的编码方式，排除了容易混淆的字符 0, O, I, l
    /// </summary>
    public static class Base58Util
    {
        // Base58 字符集（Bitcoin）
        private const string BitcoinAlphabet = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";

        // Base58 字符集（Ripple）
        private const string RippleAlphabet = "rpshnaf39wBUDNEGHJKLM4PQRST7VWXYZ2bcdeCg65jkm8oFqi1tuvAxyz";

        // Base58 字符集（Flickr）
        private const string FlickrAlphabet = "123456789abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNPQRSTUVWXYZ";

        /// <summary>
        /// 将字节数组编码为 Base58 字符串
        /// </summary>
        /// <param name="data">要编码的字节数组</param>
        /// <returns>Base58 编码字符串</returns>
        public static string Encode(byte[] data)
        {
            return Encode(data, BitcoinAlphabet);
        }

        /// <summary>
        /// 将字节数组编码为 Base58 字符串（指定字符集）
        /// </summary>
        /// <param name="data">要编码的字节数组</param>
        /// <param name="alphabet">字符集</param>
        /// <returns>Base58 编码字符串</returns>
        public static string Encode(byte[] data, string alphabet)
        {
            if (data == null || data.Length == 0)
                return string.Empty;

            // 统计前导零
            int leadingZeros = 0;
            foreach (byte b in data)
            {
                if (b == 0)
                    leadingZeros++;
                else
                    break;
            }

            // 转换为 BigInteger 进行计算
            BigInteger value = new BigInteger(data, isBigEndian: true);

            var result = new StringBuilder();
            while (value > 0)
            {
                value = BigInteger.DivRem(value, 58, out BigInteger remainder);
                result.Insert(0, alphabet[(int)remainder]);
            }

            // 添加前导 '1'（对应前导零）
            for (int i = 0; i < leadingZeros; i++)
            {
                result.Insert(0, alphabet[0]);
            }

            return result.ToString();
        }

        /// <summary>
        /// 将 Base58 字符串解码为字节数组
        /// </summary>
        /// <param name="value">Base58 编码字符串</param>
        /// <returns>解码后的字节数组</returns>
        public static byte[] Decode(string value)
        {
            return Decode(value, BitcoinAlphabet);
        }

        /// <summary>
        /// 将 Base58 字符串解码为字节数组（指定字符集）
        /// </summary>
        /// <param name="value">Base58 编码字符串</param>
        /// <param name="alphabet">字符集</param>
        /// <returns>解码后的字节数组</returns>
        public static byte[] Decode(string value, string alphabet)
        {
            if (string.IsNullOrEmpty(value))
                return Array.Empty<byte>();

            // 构建解码映射
            var decodeMap = new Dictionary<char, int>();
            for (int i = 0; i < alphabet.Length; i++)
            {
                decodeMap[alphabet[i]] = i;
            }

            // 统计前导字符（对应前导零）
            int leadingOnes = 0;
            foreach (char c in value)
            {
                if (c == alphabet[0])
                    leadingOnes++;
                else
                    break;
            }

            // 转换为 BigInteger
            BigInteger result = BigInteger.Zero;
            BigInteger baseMultiplier = BigInteger.One;

            for (int i = value.Length - 1; i >= 0; i--)
            {
                char c = value[i];
                if (!decodeMap.TryGetValue(c, out int index))
                {
                    throw new ArgumentException($"Invalid Base58 character: {c}", nameof(value));
                }

                result += index * baseMultiplier;
                baseMultiplier *= 58;
            }

            // 转换为字节数组
            byte[] bytes = result.ToByteArray(isUnsigned: true, isBigEndian: true);

            // 添加前导零
            if (leadingOnes > 0)
            {
                byte[] newBytes = new byte[leadingOnes + bytes.Length];
                Array.Copy(bytes, 0, newBytes, leadingOnes, bytes.Length);
                bytes = newBytes;
            }

            return bytes;
        }

        /// <summary>
        /// 使用 Ripple 字符集编码
        /// </summary>
        /// <param name="data">要编码的字节数组</param>
        /// <returns>Base58 编码字符串</returns>
        public static string EncodeRipple(byte[] data)
        {
            return Encode(data, RippleAlphabet);
        }

        /// <summary>
        /// 使用 Ripple 字符集解码
        /// </summary>
        /// <param name="value">Base58 编码字符串</param>
        /// <returns>解码后的字节数组</returns>
        public static byte[] DecodeRipple(string value)
        {
            return Decode(value, RippleAlphabet);
        }

        /// <summary>
        /// 使用 Flickr 字符集编码
        /// </summary>
        /// <param name="data">要编码的字节数组</param>
        /// <returns>Base58 编码字符串</returns>
        public static string EncodeFlickr(byte[] data)
        {
            return Encode(data, FlickrAlphabet);
        }

        /// <summary>
        /// 使用 Flickr 字符集解码
        /// </summary>
        /// <param name="value">Base58 编码字符串</param>
        /// <returns>解码后的字节数组</returns>
        public static byte[] DecodeFlickr(string value)
        {
            return Decode(value, FlickrAlphabet);
        }

        /// <summary>
        /// 将字符串编码为 Base58
        /// </summary>
        /// <param name="text">要编码的字符串</param>
        /// <param name="encoding">编码方式（默认 UTF-8）</param>
        /// <returns>Base58 编码字符串</returns>
        public static string EncodeString(string text, Encoding encoding = null)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            encoding ??= Encoding.UTF8;
            return Encode(encoding.GetBytes(text));
        }

        /// <summary>
        /// 将 Base58 字符串解码为原始字符串
        /// </summary>
        /// <param name="value">Base58 编码字符串</param>
        /// <param name="encoding">编码方式（默认 UTF-8）</param>
        /// <returns>解码后的字符串</returns>
        public static string DecodeString(string value, Encoding encoding = null)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            encoding ??= Encoding.UTF8;
            byte[] bytes = Decode(value);
            return encoding.GetString(bytes);
        }

        /// <summary>
        /// 验证是否是有效的 Base58 字符串
        /// </summary>
        /// <param name="value">要验证的字符串</param>
        /// <returns>是否有效</returns>
        public static bool IsValid(string value)
        {
            return IsValid(value, BitcoinAlphabet);
        }

        /// <summary>
        /// 验证是否是有效的 Base58 字符串（指定字符集）
        /// </summary>
        /// <param name="value">要验证的字符串</param>
        /// <param name="alphabet">字符集</param>
        /// <returns>是否有效</returns>
        public static bool IsValid(string value, string alphabet)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            var validChars = new HashSet<char>(alphabet);
            foreach (char c in value)
            {
                if (!validChars.Contains(c))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 尝试解码 Base58 字符串
        /// </summary>
        /// <param name="value">Base58 编码字符串</param>
        /// <param name="bytes">解码后的字节数组</param>
        /// <returns>是否解码成功</returns>
        public static bool TryDecode(string value, out byte[] bytes)
        {
            bytes = null;
            if (!IsValid(value))
                return false;

            try
            {
                bytes = Decode(value);
                return true;
            }
            // 捕获 Base58 解码格式异常
            catch (FormatException)
            {
                return false;
            }
        }

        /// <summary>
        /// 计算编码后的长度
        /// </summary>
        /// <param name="dataLength">原始数据长度</param>
        /// <returns>编码后长度</returns>
        public static int GetEncodedLength(int dataLength)
        {
            if (dataLength == 0)
                return 0;

            // Base58 编码后长度约为原始长度的 1.37 倍
            return (int)Math.Ceiling(dataLength * 137.0 / 100);
        }

        /// <summary>
        /// 计算解码后的最大长度
        /// </summary>
        /// <param name="encodedLength">编码后长度</param>
        /// <returns>解码后最大长度</returns>
        public static int GetMaxDecodedLength(int encodedLength)
        {
            if (encodedLength == 0)
                return 0;

            return (int)Math.Ceiling(encodedLength * 733.0 / 1000);
        }
    }
}
