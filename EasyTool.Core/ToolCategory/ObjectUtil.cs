using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace EasyTool.ToolCategory
{
    /// <summary>
    /// 对象工具类
    /// 提供对象的常用操作功能
    /// </summary>
    public static class ObjectUtil
    {
        /// <summary>
        /// 深拷贝对象（使用 JSON 序列化）
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="obj">原对象</param>
        /// <returns>拷贝后的对象</returns>
        public static T? DeepClone<T>(T obj)
        {
            if (obj == null)
                return default;

            var json = JsonSerializer.Serialize(obj);
            return JsonSerializer.Deserialize<T>(json);
        }

        /// <summary>
        /// 浅拷贝对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="obj">原对象</param>
        /// <returns>拷贝后的对象</returns>
        public static T? ShallowClone<T>(T obj) where T : class
        {
            if (obj == null)
                return null;

            var type = obj.GetType();
            var method = type.GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic);

            if (method != null)
            {
                return (T?)method.Invoke(obj, null);
            }

            return null;
        }

        /// <summary>
        /// 比较两个对象是否相等（深度比较）
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="obj1">对象1</param>
        /// <param name="obj2">对象2</param>
        /// <returns>是否相等</returns>
        public static bool DeepEquals<T>(T? obj1, T? obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (obj1 == null || obj2 == null)
                return false;

            var json1 = JsonSerializer.Serialize(obj1);
            var json2 = JsonSerializer.Serialize(obj2);

            return json1 == json2;
        }

        /// <summary>
        /// 获取对象的哈希码（基于内容）
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="obj">对象</param>
        /// <returns>哈希码</returns>
        public static int GetDeepHashCode<T>(T obj)
        {
            if (obj == null)
                return 0;

            var json = JsonSerializer.Serialize(obj);
            return json.GetHashCode();
        }

        /// <summary>
        /// 检查对象是否为默认值
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="obj">对象</param>
        /// <returns>是否为默认值</returns>
        public static bool IsDefault<T>(T obj)
        {
            return EqualityComparer<T>.Default.Equals(obj, default);
        }

        /// <summary>
        /// 获取对象的类型名称
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="obj">对象</param>
        /// <returns>类型名称</returns>
        public static string GetTypeName<T>(T obj)
        {
            if (obj == null)
                return "null";

            return obj.GetType().Name;
        }

        /// <summary>
        /// 获取对象的完整类型名称
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="obj">对象</param>
        /// <returns>完整类型名称</returns>
        public static string GetTypeFullName<T>(T obj)
        {
            if (obj == null)
                return "null";

            return obj.GetType().FullName ?? obj.GetType().Name;
        }

        /// <summary>
        /// 将对象转换为字典
        /// </summary>
        /// <param name="obj">对象</param>
        /// <returns>属性字典</returns>
        public static Dictionary<string, object?> ToDictionary(object obj)
        {
            if (obj == null)
                return new Dictionary<string, object?>();

            if (obj is IDictionary dict)
            {
                var result = new Dictionary<string, object?>();
                foreach (DictionaryEntry entry in dict)
                {
                    result[entry.Key?.ToString() ?? ""] = entry.Value;
                }
                return result;
            }

            var type = obj.GetType();
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var result2 = new Dictionary<string, object?>();

            foreach (var property in properties)
            {
                if (property.CanRead)
                {
                    result2[property.Name] = property.GetValue(obj);
                }
            }

            return result2;
        }

        /// <summary>
        /// 从字典创建对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="dictionary">属性字典</param>
        /// <returns>对象实例</returns>
        public static T? FromDictionary<T>(Dictionary<string, object?> dictionary) where T : class, new()
        {
            if (dictionary == null || dictionary.Count == 0)
                return default;

            var type = typeof(T);
            var obj = new T();

            foreach (var kvp in dictionary)
            {
                var property = type.GetProperty(kvp.Key, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

                if (property != null && property.CanWrite)
                {
                    var value = ConvertValue(kvp.Value, property.PropertyType);
                    property.SetValue(obj, value);
                }
            }

            return obj;
        }

        private static object? ConvertValue(object? value, Type targetType)
        {
            if (value == null)
                return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;

            if (targetType.IsAssignableFrom(value.GetType()))
                return value;

            try
            {
                return Convert.ChangeType(value, targetType);
            }
            catch
            {
                return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
            }
        }

        /// <summary>
        /// 合并两个对象的属性
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="target">目标对象</param>
        /// <param name="source">源对象</param>
        /// <param name="overwrite">是否覆盖已有值</param>
        /// <returns>合并后的对象</returns>
        public static T Merge<T>(T target, T source, bool overwrite = true) where T : class
        {
            if (target == null)
                return source ?? target;

            if (source == null)
                return target;

            var type = typeof(T);
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                if (!property.CanRead || !property.CanWrite)
                    continue;

                var sourceValue = property.GetValue(source);
                var targetValue = property.GetValue(target);

                if (sourceValue != null && (overwrite || targetValue == null))
                {
                    property.SetValue(target, sourceValue);
                }
            }

            return target;
        }

        /// <summary>
        /// 检查对象是否有指定属性
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="obj">对象</param>
        /// <param name="propertyName">属性名</param>
        /// <returns>是否有属性</returns>
        public static bool HasProperty<T>(T obj, string propertyName)
        {
            if (obj == null || string.IsNullOrEmpty(propertyName))
                return false;

            var type = obj.GetType();
            return type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance) != null;
        }

        /// <summary>
        /// 获取对象的属性值
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="obj">对象</param>
        /// <param name="propertyName">属性名</param>
        /// <returns>属性值</returns>
        public static object? GetPropertyValue<T>(T obj, string propertyName)
        {
            if (obj == null || string.IsNullOrEmpty(propertyName))
                return null;

            var type = obj.GetType();
            var property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);

            return property?.CanRead == true ? property.GetValue(obj) : null;
        }

        /// <summary>
        /// 设置对象的属性值
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="obj">对象</param>
        /// <param name="propertyName">属性名</param>
        /// <param name="value">属性值</param>
        /// <returns>是否设置成功</returns>
        public static bool SetPropertyValue<T>(T obj, string propertyName, object? value)
        {
            if (obj == null || string.IsNullOrEmpty(propertyName))
                return false;

            var type = obj.GetType();
            var property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);

            if (property?.CanWrite != true)
                return false;

            try
            {
                var convertedValue = ConvertValue(value, property.PropertyType);
                property.SetValue(obj, convertedValue);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 获取对象的所有属性名
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="obj">对象</param>
        /// <returns>属性名列表</returns>
        public static string[] GetPropertyNames<T>(T obj)
        {
            if (obj == null)
                return Array.Empty<string>();

            var type = obj.GetType();
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            return properties.Select(p => p.Name).ToArray();
        }

        /// <summary>
        /// 转换类型
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="value">值</param>
        /// <returns>转换后的值</returns>
        public static T? ConvertTo<T>(object? value)
        {
            if (value == null)
                return default;

            if (value is T t)
                return t;

            try
            {
                var targetType = typeof(T);

                if (targetType == typeof(Guid) && value is string str)
                {
                    return (T)(object)Guid.Parse(str);
                }

                if (targetType == typeof(DateTime) && value is string dateStr)
                {
                    return (T)(object)DateTime.Parse(dateStr);
                }

                return (T)Convert.ChangeType(value, targetType);
            }
            catch
            {
                return default;
            }
        }

        /// <summary>
        /// 安全转换为字符串
        /// </summary>
        /// <param name="value">值</param>
        /// <returns>字符串</returns>
        public static string SafeToString(object? value)
        {
            return value?.ToString() ?? string.Empty;
        }

        /// <summary>
        /// 检查对象是否为空或空集合
        /// </summary>
        /// <param name="value">值</param>
        /// <returns>是否为空</returns>
        public static bool IsNullOrEmpty(object? value)
        {
            if (value == null)
                return true;

            if (value is string str)
                return string.IsNullOrEmpty(str);

            if (value is IEnumerable enumerable)
            {
                var enumerator = enumerable.GetEnumerator();
                return !enumerator.MoveNext();
            }

            return false;
        }
    }
}
