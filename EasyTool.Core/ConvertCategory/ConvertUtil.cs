using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace EasyTool.ConvertCategory
{
    /// <summary>
    /// 类型转换工具类
    /// </summary>
    public static class ConvertUtil
    {
        #region 基础类型转换

        /// <summary>
        /// 转换为整数
        /// </summary>
        public static int ToInt(object? value, int defaultValue = 0)
        {
            if (value == null) return defaultValue;

            if (value is int i) return i;
            if (value is long l) return (int)l;
            if (value is double d) return (int)d;
            if (value is decimal dec) return (int)dec;
            if (value is float f) return (int)f;
            if (value is bool b) return b ? 1 : 0;
            if (value is string s)
            {
                return int.TryParse(s, out var result) ? result : defaultValue;
            }

            return defaultValue;
        }

        /// <summary>
        /// 转换为长整数
        /// </summary>
        public static long ToLong(object? value, long defaultValue = 0)
        {
            if (value == null) return defaultValue;

            if (value is long l) return l;
            if (value is int i) return i;
            if (value is double d) return (long)d;
            if (value is decimal dec) return (long)dec;
            if (value is float f) return (long)f;
            if (value is string s)
            {
                return long.TryParse(s, out var result) ? result : defaultValue;
            }

            return defaultValue;
        }

        /// <summary>
        /// 转换为浮点数
        /// </summary>
        public static double ToDouble(object? value, double defaultValue = 0)
        {
            if (value == null) return defaultValue;

            if (value is double d) return d;
            if (value is float f) return f;
            if (value is decimal dec) return (double)dec;
            if (value is int i) return i;
            if (value is long l) return l;
            if (value is string s)
            {
                return double.TryParse(s, out var result) ? result : defaultValue;
            }

            return defaultValue;
        }

        /// <summary>
        /// 转换为小数
        /// </summary>
        public static decimal ToDecimal(object? value, decimal defaultValue = 0)
        {
            if (value == null) return defaultValue;

            if (value is decimal dec) return dec;
            if (value is double d) return (decimal)d;
            if (value is float f) return (decimal)f;
            if (value is int i) return i;
            if (value is long l) return l;
            if (value is string s)
            {
                return decimal.TryParse(s, out var result) ? result : defaultValue;
            }

            return defaultValue;
        }

        /// <summary>
        /// 转换为布尔值
        /// </summary>
        public static bool ToBool(object? value, bool defaultValue = false)
        {
            if (value == null) return defaultValue;

            if (value is bool b) return b;
            if (value is int i) return i != 0;
            if (value is long l) return l != 0;
            if (value is string s)
            {
                if (string.IsNullOrEmpty(s)) return defaultValue;

                var lower = s.ToLowerInvariant();
                return lower is "true" or "1" or "yes" or "y" or "on";
            }

            return defaultValue;
        }

        /// <summary>
        /// 转换为字符串
        /// </summary>
        public static string ToString(object? value, string defaultValue = "")
        {
            if (value == null) return defaultValue;

            return value.ToString() ?? defaultValue;
        }

        /// <summary>
        /// 转换为日期时间
        /// </summary>
        public static DateTime ToDateTime(object? value, DateTime defaultValue = default)
        {
            if (value == null) return defaultValue;

            if (value is DateTime dt) return dt;
            if (value is long ticks) return new DateTime(ticks);
            if (value is string s)
            {
                return DateTime.TryParse(s, out var result) ? result : defaultValue;
            }

            return defaultValue;
        }

        /// <summary>
        /// 转换为Guid
        /// </summary>
        public static Guid ToGuid(object? value, Guid defaultValue = default)
        {
            if (value == null) return defaultValue;

            if (value is Guid g) return g;
            if (value is string s)
            {
                return Guid.TryParse(s, out var result) ? result : defaultValue;
            }

            return defaultValue;
        }

        #endregion

        #region 进制转换

        /// <summary>
        /// 十进制转二进制
        /// </summary>
        public static string ToBinary(long value)
        {
            return Convert.ToString(value, 2);
        }

        /// <summary>
        /// 二进制转十进制
        /// </summary>
        public static long FromBinary(string binary)
        {
            return Convert.ToInt64(binary, 2);
        }

        /// <summary>
        /// 十进制转八进制
        /// </summary>
        public static string ToOctal(long value)
        {
            return Convert.ToString(value, 8);
        }

        /// <summary>
        /// 八进制转十进制
        /// </summary>
        public static long FromOctal(string octal)
        {
            return Convert.ToInt64(octal, 8);
        }

        /// <summary>
        /// 十进制转十六进制
        /// </summary>
        public static string ToHex(long value)
        {
            return Convert.ToString(value, 16);
        }

        /// <summary>
        /// 十六进制转十进制
        /// </summary>
        public static long FromHex(string hex)
        {
            return Convert.ToInt64(hex, 16);
        }

        /// <summary>
        /// 字节数组转十六进制字符串
        /// </summary>
        public static string BytesToHex(byte[] bytes, bool upperCase = false)
        {
            var format = upperCase ? "X2" : "x2";
            var sb = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes)
            {
                sb.Append(b.ToString(format));
            }
            return sb.ToString();
        }

        /// <summary>
        /// 十六进制字符串转字节数组
        /// </summary>
        public static byte[] HexToBytes(string hex)
        {
            if (hex.Length % 2 != 0)
                hex = "0" + hex;

            var bytes = new byte[hex.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return bytes;
        }

        #endregion

        #region 编码转换

        /// <summary>
        /// 字符串转Base64
        /// </summary>
        public static string ToBase64(string value, Encoding? encoding = null)
        {
            encoding ??= Encoding.UTF8;
            return Convert.ToBase64String(encoding.GetBytes(value));
        }

        /// <summary>
        /// Base64转字符串
        /// </summary>
        public static string FromBase64(string base64, Encoding? encoding = null)
        {
            encoding ??= Encoding.UTF8;
            return encoding.GetString(Convert.FromBase64String(base64));
        }

        /// <summary>
        /// 字节数组转Base64
        /// </summary>
        public static string BytesToBase64(byte[] bytes)
        {
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Base64转字节数组
        /// </summary>
        public static byte[] Base64ToBytes(string base64)
        {
            return Convert.FromBase64String(base64);
        }

        #endregion

        #region 集合转换

        /// <summary>
        /// 字符串数组转整数数组
        /// </summary>
        public static int[] ToIntArray(string[] values, int defaultValue = 0)
        {
            return values?.Select(v => ToInt(v, defaultValue)).ToArray() ?? Array.Empty<int>();
        }

        /// <summary>
        /// 整数数组转字符串数组
        /// </summary>
        public static string[] ToStringArray(int[] values)
        {
            return values?.Select(v => v.ToString()).ToArray() ?? Array.Empty<string>();
        }

        /// <summary>
        /// 字典转查询字符串
        /// </summary>
        public static string DictionaryToQueryString(Dictionary<string, string?> dict)
        {
            if (dict == null || dict.Count == 0)
                return string.Empty;

            var parts = new List<string>();
            foreach (var kvp in dict)
            {
                if (kvp.Value != null)
                {
                    parts.Add($"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}");
                }
            }
            return string.Join("&", parts);
        }

        /// <summary>
        /// 查询字符串转字典
        /// </summary>
        public static Dictionary<string, string> QueryStringToDictionary(string query)
        {
            var result = new Dictionary<string, string>();

            if (string.IsNullOrEmpty(query))
                return result;

            if (query.StartsWith("?"))
                query = query.Substring(1);

            foreach (var part in query.Split('&'))
            {
                var index = part.IndexOf('=');
                if (index > 0)
                {
                    var key = Uri.UnescapeDataString(part.Substring(0, index));
                    var value = Uri.UnescapeDataString(part.Substring(index + 1));
                    result[key] = value;
                }
            }

            return result;
        }

        /// <summary>
        /// 对象转字典
        /// </summary>
        public static Dictionary<string, object?> ObjectToDictionary(object obj)
        {
            if (obj == null)
                return new Dictionary<string, object?>();

            if (obj is Dictionary<string, object?> dict)
                return dict;

            var json = JsonSerializer.Serialize(obj);
            return JsonSerializer.Deserialize<Dictionary<string, object?>>(json) ?? new Dictionary<string, object?>();
        }

        /// <summary>
        /// 字典转对象
        /// </summary>
        public static T? DictionaryToObject<T>(Dictionary<string, object?> dict)
        {
            if (dict == null)
                return default;

            var json = JsonSerializer.Serialize(dict);
            return JsonSerializer.Deserialize<T>(json);
        }

        #endregion

        #region 类型判断

        /// <summary>
        /// 是否为数值类型
        /// </summary>
        public static bool IsNumericType(Type type)
        {
            return type == typeof(int) || type == typeof(long) || type == typeof(short) ||
                   type == typeof(byte) || type == typeof(uint) || type == typeof(ulong) ||
                   type == typeof(ushort) || type == typeof(sbyte) || type == typeof(float) ||
                   type == typeof(double) || type == typeof(decimal);
        }

        /// <summary>
        /// 是否为整数类型
        /// </summary>
        public static bool IsIntegerType(Type type)
        {
            return type == typeof(int) || type == typeof(long) || type == typeof(short) ||
                   type == typeof(byte) || type == typeof(uint) || type == typeof(ulong) ||
                   type == typeof(ushort) || type == typeof(sbyte);
        }

        /// <summary>
        /// 是否为浮点类型
        /// </summary>
        public static bool IsFloatType(Type type)
        {
            return type == typeof(float) || type == typeof(double) || type == typeof(decimal);
        }

        #endregion
    }
}
