using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Xml.Serialization;

namespace EasyTool.Extension
{
    /// <summary>
    /// Object 对象扩展方法
    /// </summary>
    public static class ObjectExtension
    {
        #region 空值判断

        /// <summary>
        /// 判断对象是否为 null
        /// </summary>
        public static bool IsNull(this object obj)
        {
            return obj == null;
        }

        /// <summary>
        /// 判断对象是否不为 null
        /// </summary>
        public static bool IsNotNull(this object obj)
        {
            return obj != null;
        }

        /// <summary>
        /// 判断对象是否为空（null 或空字符串或空集合）
        /// </summary>
        public static bool IsNullOrEmpty(this object obj)
        {
            if (obj == null)
                return true;

            if (obj is string str)
                return string.IsNullOrEmpty(str);

            if (obj is System.Collections.ICollection collection)
                return collection.Count == 0;

            return false;
        }

        #endregion

        #region 类型转换

        /// <summary>
        /// 将对象转换为指定类型
        /// </summary>
        public static T? As<T>(this object obj)
        {
            if (obj == null)
                return default;

            return (T)obj;
        }

        /// <summary>
        /// 尝试将对象转换为指定类型
        /// </summary>
        public static T To<T>(this object obj) where T : struct
        {
            return (T)Convert.ChangeType(obj, typeof(T));
        }

        /// <summary>
        /// 安全转换，失败返回默认值
        /// </summary>
        public static T? ToOrDefault<T>(this object obj)
        {
            return ToOrDefault(obj, default(T));
        }

        /// <summary>
        /// 安全转换，失败返回指定默认值
        /// </summary>
        public static T ToOrDefault<T>(this object obj, T defaultValue)
        {
            if (obj == null)
                return defaultValue;

            try
            {
                if (obj is T direct)
                    return direct;

                return (T)Convert.ChangeType(obj, typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }

        #endregion

        #region JSON 序列化

        /// <summary>
        /// 将对象序列化为 JSON 字符串
        /// </summary>
        public static string? ToJson(this object obj, bool indented = false)
        {
            if (obj == null)
                return null;

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = indented
            };

            return JsonSerializer.Serialize(obj, options);
        }

        /// <summary>
        /// 从 JSON 字符串反序列化为对象
        /// </summary>
        public static T? FromJson<T>(this string json)
        {
            if (string.IsNullOrEmpty(json))
                return default;

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return JsonSerializer.Deserialize<T>(json, options);
        }

        #endregion

        #region XML 序列化

        /// <summary>
        /// 将对象序列化为 XML 字符串
        /// </summary>
        public static string? ToXml(this object obj)
        {
            if (obj == null)
                return null;

            var serializer = new XmlSerializer(obj.GetType());
            using var writer = new StringWriter();
            serializer.Serialize(writer, obj);
            return writer.ToString();
        }

        /// <summary>
        /// 从 XML 字符串反序列化为对象
        /// </summary>
        public static T? FromXml<T>(this string xml)
        {
            if (string.IsNullOrEmpty(xml))
                return default;

            var serializer = new XmlSerializer(typeof(T));
            using var reader = new StringReader(xml);
            return (T?)serializer.Deserialize(reader);
        }

        #endregion

        #region 深拷贝

        /// <summary>
        /// 深拷贝对象（使用 JSON 序列化）
        /// </summary>
        public static T? DeepClone<T>(this T obj)
        {
            if (obj == null)
                return default;

            var json = obj.ToJson();
            return json.FromJson<T>();
        }

        /// <summary>
        /// 浅拷贝对象（使用 MemberwiseClone）
        /// </summary>
        public static T? ShallowClone<T>(this T obj) where T : class
        {
            if (obj == null)
                return null;

            var method = obj.GetType().GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic);
            if (method != null)
                return (T?)method.Invoke(obj, null);

            throw new InvalidOperationException("Object does not support MemberwiseClone");
        }

        #endregion

        #region 字典转换

        /// <summary>
        /// 将对象转换为字典
        /// </summary>
        public static Dictionary<string, object>? ToDictionary(this object obj)
        {
            if (obj == null)
                return null;

            var dict = new Dictionary<string, object>();

            foreach (var prop in obj.GetType().GetProperties())
            {
                if (prop.CanRead)
                {
                    dict[prop.Name] = prop.GetValue(obj) ?? string.Empty;
                }
            }

            return dict;
        }

        /// <summary>
        /// 从字典创建对象
        /// </summary>
        public static T? FromDictionary<T>(this Dictionary<string, object>? dict) where T : new()
        {
            if (dict == null)
                return default;

            var obj = new T();

            foreach (var prop in typeof(T).GetProperties())
            {
                if (prop.CanWrite && dict.TryGetValue(prop.Name, out var value))
                {
                    if (value != null)
                    {
                        var convertedValue = Convert.ChangeType(value, prop.PropertyType);
                        prop.SetValue(obj, convertedValue);
                    }
                }
            }

            return obj;
        }

        #endregion

        #region 属性访问

        /// <summary>
        /// 获取属性值
        /// </summary>
        public static object? GetPropertyValue(this object obj, string propertyName)
        {
            if (obj == null || string.IsNullOrEmpty(propertyName))
                return null;

            var prop = obj.GetType().GetProperty(propertyName);
            return prop?.GetValue(obj);
        }

        /// <summary>
        /// 设置属性值
        /// </summary>
        public static void SetPropertyValue(this object obj, string propertyName, object? value)
        {
            if (obj == null || string.IsNullOrEmpty(propertyName))
                return;

            var prop = obj.GetType().GetProperty(propertyName);
            prop?.SetValue(obj, value);
        }

        #endregion

        #region 条件执行

        /// <summary>
        /// 条件执行（当条件满足时执行操作）
        /// </summary>
        public static T If<T>(this T obj, bool condition, Action<T>? action)
        {
            if (condition)
            {
                action?.Invoke(obj);
            }
            return obj;
        }

        /// <summary>
        /// 条件执行（当条件满足时返回函数结果）
        /// </summary>
        public static TResult? If<T, TResult>(this T obj, bool condition, Func<T, TResult>? func)
        {
            return condition ? func!(obj) : default;
        }

        /// <summary>
        /// 条件执行（当对象不为 null 时执行操作）
        /// </summary>
        public static T IfNotNull<T>(this T obj, Action<T>? action) where T : class
        {
            if (obj != null)
            {
                action?.Invoke(obj);
            }
            return obj;
        }

        /// <summary>
        /// 条件执行（当对象不为 null 时返回函数结果）
        /// </summary>
        public static TResult? IfNotNull<T, TResult>(this T obj, Func<T, TResult> func) where T : class
        {
            return obj != null ? func(obj) : default;
        }

        #endregion

        #region 管道操作

        /// <summary>
        /// 管道操作（执行函数并返回结果）
        /// </summary>
        public static TResult Pipe<T, TResult>(this T obj, Func<T, TResult> func)
        {
            return func(obj);
        }

        /// <summary>
        /// 管道操作（执行操作）
        /// </summary>
        public static T Pipe<T>(this T obj, Action<T> action)
        {
            action(obj);
            return obj;
        }

        #endregion

        #region 对象检查

        /// <summary>
        /// 判断对象是否是指定类型
        /// [Obsolete("请直接使用 obj is T")]
        /// </summary>
        [Obsolete("请直接使用 obj is T", false)]
        public static bool Is<T>(this object obj)
        {
            return obj is T;
        }

        /// <summary>
        /// 判断对象是否实现了指定接口
        /// </summary>
        public static bool Implements<TInterface>(this object obj)
        {
            if (obj == null)
                return false;

            return typeof(TInterface).IsAssignableFrom(obj.GetType());
        }

        #endregion

        #region 对象相等比较

        /// <summary>
        /// 比较两个对象的属性值是否相等
        /// </summary>
        public static bool PropertiesEqual<T>(this T obj, T? other) where T : class
        {
            if (obj == null && other == null)
                return true;

            if (obj == null || other == null)
                return false;

            var type = typeof(T);

            foreach (var prop in type.GetProperties())
            {
                if (!prop.CanRead)
                    continue;

                var value1 = prop.GetValue(obj);
                var value2 = prop.GetValue(other);

                if (!Equals(value1, value2))
                    return false;
            }

            return true;
        }

        #endregion

        #region 对象信息

        /// <summary>
        /// 获取对象的类型名称
        /// [Obsolete("请直接使用 obj?.GetType().Name")]
        /// </summary>
        [Obsolete("请直接使用 obj?.GetType().Name", false)]
        public static string GetTypeName(this object obj)
        {
            return obj?.GetType().Name ?? "null";
        }

        /// <summary>
        /// 获取对象的完整类型名称
        /// [Obsolete("请直接使用 obj?.GetType().FullName")]
        /// </summary>
        [Obsolete("请直接使用 obj?.GetType().FullName", false)]
        public static string GetFullTypeName(this object obj)
        {
            return obj?.GetType().FullName ?? "null";
        }

        #endregion

        #region 对象转字符串

        /// <summary>
        /// 将对象转换为字符串（处理 null）
        /// [Obsolete("请直接使用 obj?.ToString() ?? string.Empty")]
        /// </summary>
        [Obsolete("请直接使用 obj?.ToString() ?? string.Empty", false)]
        public static string ToStringSafe(this object obj)
        {
            return obj?.ToString() ?? string.Empty;
        }

        /// <summary>
        /// 将对象转换为字符串（null 时返回默认值）
        /// [Obsolete("请直接使用 obj?.ToString() ?? defaultValue")]
        /// </summary>
        [Obsolete("请直接使用 obj?.ToString() ?? defaultValue", false)]
        public static string ToStringOrDefault(this object obj, string defaultValue = "")
        {
            return obj?.ToString() ?? defaultValue;
        }

        #endregion

        #region 对象抛出异常

        /// <summary>
        /// 对象为 null 时抛出异常
        /// </summary>
        public static T ThrowIfNull<T>(this T? obj, string? paramName = null) where T : class
        {
            if (obj == null)
                throw new ArgumentNullException(paramName ?? typeof(T).Name);

            return obj;
        }

        /// <summary>
        /// 条件不满足时抛出异常
        /// </summary>
        public static T ThrowIf<T>(this T obj, bool condition, string message) where T : class
        {
            if (condition)
                throw new Exception(message);

            return obj;
        }

        #endregion
    }
}
