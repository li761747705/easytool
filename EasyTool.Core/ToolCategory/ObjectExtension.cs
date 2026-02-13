using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
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
        /// 异步深拷贝对象（使用 JSON 序列化）
        /// </summary>
        public static async Task<T?> DeepCloneAsync<T>(this T obj)
        {
            if (obj == null)
                return default;

            var json = await Task.Run(() => obj.ToJson());
            return await Task.Run(() => json.FromJson<T>());
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
        /// 获取指定类型的默认值
        /// </summary>
        public static object? GetDefault(Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }

        /// <summary>
        /// 判断对象是否是其类型的默认值
        /// </summary>
        public static bool IsDefaultValue(this object obj)
        {
            return obj == null || obj.Equals(GetDefault(obj.GetType()));
        }

        /// <summary>
        /// 判断指定类型是否是可空值类型
        /// </summary>
        public static bool IsNullable(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        /// <summary>
        /// 获取可空类型的基础类型
        /// </summary>
        public static Type GetNullableType(Type type)
        {
            return Nullable.GetUnderlyingType(type);
        }

        /// <summary>
        /// 获取可空类型或枚举类型的基础类型
        /// </summary>
        public static Type GetUnderlyingType(Type type)
        {
            if (IsNullable(type))
            {
                return GetNullableType(type);
            }

            if (type.IsEnum)
            {
                return Enum.GetUnderlyingType(type);
            }

            return type;
        }

        /// <summary>
        /// 判断指定类型是否是简单类型
        /// </summary>
        public static bool IsSimpleType(Type type)
        {
            if (type == typeof(string))
            {
                return true;
            }

            if (type.IsValueType)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 判断指定类型是否是数字类型
        /// </summary>
        public static bool IsNumericType(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.Int16:
                case TypeCode.UInt32:
                case TypeCode.Int32:
                case TypeCode.UInt64:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// 判断指定类型是否是布尔类型
        /// </summary>
        public static bool IsBooleanType(Type type)
        {
            return type == typeof(bool);
        }

        /// <summary>
        /// 判断指定类型是否是日期时间类型
        /// </summary>
        public static bool IsDateTimeType(Type type)
        {
            return type == typeof(DateTime);
        }

        /// <summary>
        /// 判断指定类型是否是集合类型
        /// </summary>
        public static bool IsEnumerableType(Type type)
        {
            return typeof(IEnumerable).IsAssignableFrom(type);
        }

        /// <summary>
        /// 获取指定类型的所有派生类型
        /// </summary>
        public static Type[] GetSubclassesOf(Type baseType)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => baseType.IsAssignableFrom(p) && p != baseType)
                .ToArray();
        }

        #endregion

        #region 对象转字符串

        #endregion

        #region 对象转换（静态工具方法）

        /// <summary>
        /// 将对象转换为指定类型（使用 TypeConverter 和 IConvertible）
        /// </summary>
        public static T ConvertTo<T>(object obj)
        {
            return (T)ConvertTo(obj, typeof(T));
        }

        /// <summary>
        /// 将对象转换为指定类型（使用 TypeConverter 和 IConvertible）
        /// </summary>
        public static object? ConvertTo(object obj, Type targetType)
        {
            if (obj == null)
            {
                return GetDefault(targetType);
            }

            Type sourceType = obj.GetType();

            if (targetType.IsAssignableFrom(sourceType))
            {
                return obj;
            }

            var converter = System.ComponentModel.TypeDescriptor.GetConverter(targetType);
            if (converter != null && converter.CanConvertFrom(sourceType))
            {
                return converter.ConvertFrom(obj);
            }

            var sourceConverter = System.ComponentModel.TypeDescriptor.GetConverter(sourceType);
            if (sourceConverter != null && sourceConverter.CanConvertTo(targetType))
            {
                return sourceConverter.ConvertTo(obj, targetType);
            }

            if (obj is IConvertible)
            {
                try
                {
                    return System.Convert.ChangeType(obj, targetType);
                }
                catch (InvalidCastException)
                {
                }
            }

            try
            {
                var implicitOp = sourceType.GetMethod("op_Implicit", new[] { sourceType });
                if (implicitOp != null && implicitOp.ReturnType == targetType)
                {
                    return implicitOp.Invoke(null, new[] { obj });
                }

                var explicitOp = sourceType.GetMethod("op_Explicit", new[] { sourceType });
                if (explicitOp != null && explicitOp.ReturnType == targetType)
                {
                    return explicitOp.Invoke(null, new[] { obj });
                }

                var targetImplicitOp = targetType.GetMethod("op_Implicit", new[] { sourceType });
                if (targetImplicitOp != null && targetImplicitOp.ReturnType == targetType)
                {
                    return targetImplicitOp.Invoke(null, new[] { obj });
                }

                var targetExplicitOp = targetType.GetMethod("op_Explicit", new[] { sourceType });
                if (targetExplicitOp != null && targetExplicitOp.ReturnType == targetType)
                {
                    return targetExplicitOp.Invoke(null, new[] { obj });
                }
            }
            catch (InvalidCastException)
            {
            }

            throw new InvalidCastException($"无法将类型为 {sourceType.Name} 的对象转换为类型为 {targetType.Name} 的对象");
        }

        #endregion

        #region 对象属性复制

        /// <summary>
        /// 将源对象的属性复制到目标对象中
        /// </summary>
        public static void CopyProperties(object source, object target)
        {
            Type sourceType = source.GetType();
            Type targetType = target.GetType();

            foreach (PropertyInfo sourceProperty in sourceType.GetProperties())
            {
                if (!sourceProperty.CanRead)
                {
                    continue;
                }

                PropertyInfo targetProperty = targetType.GetProperty(sourceProperty.Name);

                if (targetProperty == null || !targetProperty.CanWrite)
                {
                    continue;
                }

                object value = GetPropertyValue(source, sourceProperty.Name);
                SetPropertyValue(target, targetProperty.Name, value);
            }
        }

        /// <summary>
        /// 对象属性值的加密
        /// </summary>
        public static void EncryptPropertyValue(object obj, string propertyName, Func<string, string> encryptFunc)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            if (encryptFunc == null)
            {
                throw new ArgumentNullException(nameof(encryptFunc));
            }

            PropertyInfo property = obj.GetType().GetProperty(propertyName);

            if (property == null)
            {
                throw new ArgumentException($"对象类型 {obj.GetType().Name} 中没有名为 {propertyName} 的属性", nameof(propertyName));
            }

            object value = property.GetValue(obj);

            if (value != null && value is string)
            {
                string encryptedValue = encryptFunc((string)value);
                property.SetValue(obj, encryptedValue);
            }
        }

        /// <summary>
        /// 对象属性值的解密
        /// </summary>
        public static void DecryptPropertyValue(object obj, string propertyName, Func<string, string> decryptFunc)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            if (decryptFunc == null)
            {
                throw new ArgumentNullException(nameof(decryptFunc));
            }

            PropertyInfo property = obj.GetType().GetProperty(propertyName);

            if (property == null)
            {
                throw new ArgumentException($"对象类型 {obj.GetType().Name} 中没有名为 {propertyName} 的属性", nameof(propertyName));
            }

            object value = property.GetValue(obj);

            if (value != null && value is string)
            {
                string decryptedValue = decryptFunc((string)value);
                property.SetValue(obj, decryptedValue);
            }
        }

        /// <summary>
        /// 在对象属性上进行特定的处理
        /// </summary>
        public static void ProcessPropertyValue(object obj, string propertyName, Action<object> processAction)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            if (processAction == null)
            {
                throw new ArgumentNullException(nameof(processAction));
            }

            PropertyInfo property = obj.GetType().GetProperty(propertyName);

            if (property == null)
            {
                throw new ArgumentException($"对象类型 {obj.GetType().Name} 中没有名为 {propertyName} 的属性", nameof(propertyName));
            }

            object value = property.GetValue(obj);

            if (value != null)
            {
                processAction(value);
                property.SetValue(obj, value);
            }
        }

        #endregion

        #region 对象比较

        /// <summary>
        /// 比较两个对象的差异（属性值或字段值不同）
        /// </summary>
        public static IEnumerable<string> CompareDifferences(object obj1, object obj2)
        {
            if (obj1 == null && obj2 == null)
            {
                return Enumerable.Empty<string>();
            }

            if (obj1 == null || obj2 == null)
            {
                throw new ArgumentNullException("比较对象不能为 null");
            }

            List<string> differences = new List<string>();

            foreach (PropertyInfo property in obj1.GetType().GetProperties())
            {
                object value1 = property.GetValue(obj1);
                object value2 = property.GetValue(obj2);

                if (!Equals(value1, value2))
                {
                    differences.Add($"属性 {property.Name} 的值不同：{value1} -> {value2}");
                }
            }

            foreach (FieldInfo field in obj1.GetType().GetFields())
            {
                object value1 = field.GetValue(obj1);
                object value2 = field.GetValue(obj2);

                if (!Equals(value1, value2))
                {
                    differences.Add($"字段 {field.Name} 的值不同：{value1} -> {value2}");
                }
            }

            return differences;
        }

        #endregion

        #region 对象高级转换

        /// <summary>
        /// 将对象转换为键值对集合
        /// </summary>
        public static IEnumerable<KeyValuePair<string, object>> ToKeyValuePairs(this object obj)
        {
            if (obj == null)
            {
                return Enumerable.Empty<KeyValuePair<string, object>>();
            }

            List<KeyValuePair<string, object>> pairs = new List<KeyValuePair<string, object>>();

            foreach (PropertyInfo property in obj.GetType().GetProperties())
            {
                pairs.Add(new KeyValuePair<string, object>(property.Name, property.GetValue(obj)));
            }

            foreach (FieldInfo field in obj.GetType().GetFields())
            {
                pairs.Add(new KeyValuePair<string, object>(field.Name, field.GetValue(obj)));
            }

            return pairs;
        }

        /// <summary>
        /// 将对象转换为动态扩展对象
        /// </summary>
        public static dynamic? ToDynamic(this object obj)
        {
            if (obj == null)
            {
                return null;
            }

            IDictionary<string, object> dictionary = new System.Dynamic.ExpandoObject();

            foreach (PropertyInfo propertyInfo in obj.GetType().GetProperties())
            {
                if (!propertyInfo.CanRead)
                {
                    continue;
                }

                object value = GetPropertyValue(obj, propertyInfo.Name);
                dictionary.Add(propertyInfo.Name, value);
            }

            foreach (FieldInfo fieldInfo in obj.GetType().GetFields())
            {
                object value = GetFieldValue(obj, fieldInfo.Name);
                dictionary.Add(fieldInfo.Name, value);
            }

            return dictionary;
        }

        /// <summary>
        /// 获取对象的字段值
        /// </summary>
        public static object? GetFieldValue(this object obj, string fieldName)
        {
            return obj.GetType().GetField(fieldName)?.GetValue(obj);
        }

        /// <summary>
        /// 设置对象的字段值
        /// </summary>
        public static void SetFieldValue(this object obj, string fieldName, object? value)
        {
            obj.GetType().GetField(fieldName)?.SetValue(obj, value);
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
