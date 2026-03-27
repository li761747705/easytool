using System;
using System.Collections.Generic;

namespace EasyTool.ToolCategory
{
    /// <summary>
    /// 防御性编程工具类
    /// 提供参数验证和断言功能
    /// </summary>
    public static class GuardUtil
    {
        /// <summary>
        /// 验证参数不为null
        /// </summary>
        public static T NotNull<T>(T? value, string paramName) where T : class
        {
            if (value == null)
                throw new ArgumentNullException(paramName);
            return value;
        }

        /// <summary>
        /// 验证可空值类型不为null
        /// </summary>
        public static T NotNull<T>(T? value, string paramName) where T : struct
        {
            if (value == null)
                throw new ArgumentNullException(paramName);
            return value.Value;
        }

        /// <summary>
        /// 验证字符串不为空或null
        /// </summary>
        public static string NotNullOrEmpty(string? value, string paramName)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("字符串不能为空或null", paramName);
            return value;
        }

        /// <summary>
        /// 验证字符串不为空白
        /// </summary>
        public static string NotNullOrWhiteSpace(string? value, string paramName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("字符串不能为空白", paramName);
            return value;
        }

        /// <summary>
        /// 验证集合不为空
        /// </summary>
        public static IEnumerable<T> NotEmpty<T>(IEnumerable<T>? value, string paramName)
        {
            if (value == null)
                throw new ArgumentNullException(paramName);

            var collection = value as ICollection<T> ?? new List<T>(value);
            if (collection.Count == 0)
                throw new ArgumentException("集合不能为空", paramName);

            return collection;
        }

        /// <summary>
        /// 验证范围
        /// </summary>
        public static int InRange(int value, int min, int max, string paramName)
        {
            if (value < min || value > max)
                throw new ArgumentOutOfRangeException(paramName, value, $"值必须在 {min} 和 {max} 之间");
            return value;
        }

        /// <summary>
        /// 验证范围
        /// </summary>
        public static double InRange(double value, double min, double max, string paramName)
        {
            if (value < min || value > max)
                throw new ArgumentOutOfRangeException(paramName, value, $"值必须在 {min} 和 {max} 之间");
            return value;
        }

        /// <summary>
        /// 验证大于指定值
        /// </summary>
        public static int GreaterThan(int value, int threshold, string paramName)
        {
            if (value <= threshold)
                throw new ArgumentOutOfRangeException(paramName, value, $"值必须大于 {threshold}");
            return value;
        }

        /// <summary>
        /// 验证大于等于指定值
        /// </summary>
        public static int GreaterThanOrEqual(int value, int threshold, string paramName)
        {
            if (value < threshold)
                throw new ArgumentOutOfRangeException(paramName, value, $"值必须大于或等于 {threshold}");
            return value;
        }

        /// <summary>
        /// 验证小于指定值
        /// </summary>
        public static int LessThan(int value, int threshold, string paramName)
        {
            if (value >= threshold)
                throw new ArgumentOutOfRangeException(paramName, value, $"值必须小于 {threshold}");
            return value;
        }

        /// <summary>
        /// 验证小于等于指定值
        /// </summary>
        public static int LessThanOrEqual(int value, int threshold, string paramName)
        {
            if (value > threshold)
                throw new ArgumentOutOfRangeException(paramName, value, $"值必须小于或等于 {threshold}");
            return value;
        }

        /// <summary>
        /// 验证条件为真
        /// </summary>
        public static void IsTrue(bool condition, string message, string? paramName = null)
        {
            if (!condition)
                throw new ArgumentException(message, paramName);
        }

        /// <summary>
        /// 验证条件为假
        /// </summary>
        public static void IsFalse(bool condition, string message, string? paramName = null)
        {
            if (condition)
                throw new ArgumentException(message, paramName);
        }

        /// <summary>
        /// 验证类型
        /// </summary>
        public static T IsType<T>(object value, string paramName)
        {
            if (value is not T typed)
                throw new ArgumentException($"值必须是 {typeof(T).Name} 类型", paramName);
            return typed;
        }

        /// <summary>
        /// 验证枚举值有效
        /// </summary>
        public static T EnumDefined<T>(T value, string paramName) where T : struct, Enum
        {
            if (!Enum.IsDefined(typeof(T), value))
                throw new ArgumentException($"无效的枚举值: {value}", paramName);
            return value;
        }

        /// <summary>
        /// 验证邮箱格式
        /// </summary>
        public static string Email(string? value, string paramName)
        {
            NotNullOrEmpty(value, paramName);
            if (!System.Text.RegularExpressions.Regex.IsMatch(value!, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                throw new ArgumentException("无效的邮箱格式", paramName);
            return value!;
        }

        /// <summary>
        /// 验证文件存在
        /// </summary>
        public static string FileExists(string? path, string paramName)
        {
            NotNullOrEmpty(path, paramName);
            if (!System.IO.File.Exists(path))
                throw new System.IO.FileNotFoundException($"文件不存在: {path}", path);
            return path!;
        }

        /// <summary>
        /// 验证目录存在
        /// </summary>
        public static string DirectoryExists(string? path, string paramName)
        {
            NotNullOrEmpty(path, paramName);
            if (!System.IO.Directory.Exists(path))
                throw new System.IO.DirectoryNotFoundException($"目录不存在: {path}");
            return path!;
        }

        /// <summary>
        /// 抛出异常
        /// </summary>
        public static void Throw<TException>(string message) where TException : Exception, new()
        {
            var exception = (TException?)Activator.CreateInstance(typeof(TException), message)
                ?? new TException();
            throw exception;
        }

        /// <summary>
        /// 如果条件为真，抛出异常
        /// </summary>
        public static void ThrowIf<TException>(bool condition, string message) where TException : Exception, new()
        {
            if (condition)
                Throw<TException>(message);
        }
    }
}