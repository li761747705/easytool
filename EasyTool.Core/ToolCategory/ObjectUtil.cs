using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml.Serialization;

namespace EasyTool
{
    /// <summary>
    /// 对象工具类
    /// </summary>
    public class ObjectUtil
    {

        /// <summary>
        /// 检查对象是否为 null
        /// [Obsolete("请直接使用 obj == null")]
        /// </summary>
        [Obsolete("请直接使用 obj == null", false)]
        public static bool IsNull(object? obj)
        {
            return obj == null;
        }

        /// <summary>
        /// 检查对象是否不为 null
        /// [Obsolete("请直接使用 obj != null")]
        /// </summary>
        [Obsolete("请直接使用 obj != null", false)]
        public static bool IsNotNull(object? obj)
        {
            return obj != null;
        }

        /// <summary>
        /// 检查对象是否为空（null 或者 空字符串或空白字符）
        /// </summary>
        public static bool IsNullOrEmpty(object? obj)
        {
            if (IsNull(obj))
            {
                return true;
            }

            if (obj is string str)
            {
                return string.IsNullOrWhiteSpace(str);
            }

            if (obj is ICollection collection)
            {
                return collection.Count == 0;
            }

            return false;
        }

        /// <summary>
        /// 检查对象是否不为空（非 null 且 非空字符串 或者 非空集合）
        /// </summary>
        public static bool IsNotNullOrEmpty(object? obj)
        {
            return !IsNullOrEmpty(obj);
        }

        /// <summary>
        /// 检查两个对象是否相等
        /// </summary>
        public static new bool Equals(object obj1, object obj2)
        {
            if (IsNull(obj1) && IsNull(obj2))
            {
                return true;
            }

            if (IsNull(obj1) || IsNull(obj2))
            {
                return false;
            }

            return obj1.Equals(obj2);
        }

        /// <summary>
        /// 获取对象的类型名称
        /// [Obsolete("请直接使用 obj.GetType().Name")]
        /// </summary>
        [Obsolete("请直接使用 obj.GetType().Name", false)]
        public static string GetTypeName(object obj)
        {
            return obj.GetType().Name;
        }

        /// <summary>
        /// 将对象转换为指定类型
        /// </summary>
        public static T Convert<T>(object obj)
        {
            return (T)Convert(obj, typeof(T));
        }

        /// <summary>
        /// 将对象转换为指定类型
        /// </summary>
        public static object? Convert(object obj, Type targetType)
        {
            if (IsNull(obj))
            {
                // 处理可空值类型的默认值
                return GetDefault(targetType);
            }

            Type sourceType = obj.GetType();

            // 如果目标类型可以从源类型赋值，直接返回
            if (targetType.IsAssignableFrom(sourceType))
            {
                return obj;
            }

            // 使用 TypeConverter 进行转换
            var converter = System.ComponentModel.TypeDescriptor.GetConverter(targetType);
            if (converter != null && converter.CanConvertFrom(sourceType))
            {
                return converter.ConvertFrom(obj);
            }

            // 尝试从源类型的 TypeConverter 转换
            var sourceConverter = System.ComponentModel.TypeDescriptor.GetConverter(sourceType);
            if (sourceConverter != null && sourceConverter.CanConvertTo(targetType))
            {
                return sourceConverter.ConvertTo(obj, targetType);
            }

            // 使用 IConvertible 接口转换
            if (obj is IConvertible)
            {
                try
                {
                    return System.Convert.ChangeType(obj, targetType);
                }
                catch (InvalidCastException)
                {
                    // 继续尝试其他转换方式
                }
            }

            // 尝试使用隐式或显式转换操作符
            try
            {
                // 查找源类型的隐式转换操作符
                var implicitOp = sourceType.GetMethod("op_Implicit", new[] { sourceType });
                if (implicitOp != null && implicitOp.ReturnType == targetType)
                {
                    return implicitOp.Invoke(null, new[] { obj });
                }

                // 查找源类型的显式转换操作符
                var explicitOp = sourceType.GetMethod("op_Explicit", new[] { sourceType });
                if (explicitOp != null && explicitOp.ReturnType == targetType)
                {
                    return explicitOp.Invoke(null, new[] { obj });
                }

                // 查找目标类型的隐式转换操作符
                var targetImplicitOp = targetType.GetMethod("op_Implicit", new[] { sourceType });
                if (targetImplicitOp != null && targetImplicitOp.ReturnType == targetType)
                {
                    return targetImplicitOp.Invoke(null, new[] { obj });
                }

                // 查找目标类型的显式转换操作符
                var targetExplicitOp = targetType.GetMethod("op_Explicit", new[] { sourceType });
                if (targetExplicitOp != null && targetExplicitOp.ReturnType == targetType)
                {
                    return targetExplicitOp.Invoke(null, new[] { obj });
                }
            }
            catch (InvalidCastException)
            {
                // 转换操作符失败，继续抛出异常
            }

            throw new InvalidCastException($"无法将类型为 {sourceType.Name} 的对象转换为类型为 {targetType.Name} 的对象");
        }

        /// <summary>
        /// 获取对象的属性列表
        /// [Obsolete("请直接使用 obj.GetType().GetProperties()")]
        /// </summary>
        [Obsolete("请直接使用 obj.GetType().GetProperties()", false)]
        public static IEnumerable<PropertyInfo> GetProperties(object obj)
        {
            return obj.GetType().GetProperties();
        }

        /// <summary>
        /// 获取对象的属性值
        /// </summary>
        public static object? GetPropertyValue(object obj, string propertyName)
        {
            return obj.GetType().GetProperty(propertyName)?.GetValue(obj);
        }

        /// <summary>
        /// 设置对象的属性值
        /// </summary>
        public static void SetPropertyValue(object obj, string propertyName, object? value)
        {
            obj.GetType().GetProperty(propertyName)?.SetValue(obj, value);
        }

        /// <summary>
        /// 获取对象的字段列表
        /// [Obsolete("请直接使用 obj.GetType().GetFields()")]
        /// </summary>
        [Obsolete("请直接使用 obj.GetType().GetFields()", false)]
        public static IEnumerable<FieldInfo> GetFields(object obj)
        {
            return obj.GetType().GetFields();
        }

        /// <summary>
        /// 获取对象的字段值
        /// </summary>
        public static object? GetFieldValue(object obj, string fieldName)
        {
            return obj.GetType().GetField(fieldName)?.GetValue(obj);
        }

        /// <summary>
        /// 设置对象的字段值
        /// </summary>
        public static void SetFieldValue(object obj, string fieldName, object? value)
        {
            obj.GetType().GetField(fieldName)?.SetValue(obj, value);
        }

        /// <summary>
        /// 获取对象的方法列表
        /// [Obsolete("请直接使用 obj.GetType().GetMethods()")]
        /// </summary>
        [Obsolete("请直接使用 obj.GetType().GetMethods()", false)]
        public static IEnumerable<MethodInfo> GetMethods(object obj)
        {
            return obj.GetType().GetMethods();
        }

        /// <summary>
        /// 判断对象是否实现了指定接口
        /// [Obsolete("请直接使用 interfaceType.IsAssignableFrom(obj.GetType())")]
        /// </summary>
        [Obsolete("请直接使用 interfaceType.IsAssignableFrom(obj.GetType())", false)]
        public static bool ImplementsInterface(object obj, Type interfaceType)
        {
            return interfaceType.IsAssignableFrom(obj.GetType());
        }

        /// <summary>
        /// 判断对象是否为指定类型的实例
        /// [Obsolete("请直接使用 targetType.IsInstanceOfType(obj)")]
        /// </summary>
        [Obsolete("请直接使用 targetType.IsInstanceOfType(obj)", false)]
        public static bool IsInstanceOfType(object obj, Type targetType)
        {
            return targetType.IsInstanceOfType(obj);
        }

        /// <summary>
        /// 对象属性或字段值的加密
        /// </summary>
        public static void EncryptPropertyValue(object obj, string propertyName, Func<string, string> encryptFunc)
        {
            if (IsNull(obj))
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
        /// 对象属性或字段值的解密
        /// </summary>
        public static void DecryptPropertyValue(object obj, string propertyName, Func<string, string> decryptFunc)
        {
            if (IsNull(obj))
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
        /// 在对象属性或字段上进行特定的处理
        /// </summary>
        public static void ProcessPropertyValue(object obj, string propertyName, Action<object> processAction)
        {
            if (IsNull(obj))
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

        /// <summary>
        /// 将对象序列化为 JSON 字符串
        /// </summary>
        public static string? ToJson(object obj)
        {
            if (IsNull(obj))
            {
                return null;
            }

            using (MemoryStream stream = new MemoryStream())
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(obj.GetType());
                serializer.WriteObject(stream, obj);
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }

        /// <summary>
        /// 将 JSON 字符串反序列化为对象
        /// </summary>
        public static T FromJson<T>(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return default(T);
            }

            using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
                return (T)serializer.ReadObject(stream);
            }
        }

        /// <summary>
        /// 将对象序列化为 XML 字符串
        /// </summary>
        public static string? ToXml(object obj)
        {
            if (IsNull(obj))
            {
                return null;
            }

            XmlSerializer serializer = new XmlSerializer(obj.GetType());
            using (MemoryStream stream = new MemoryStream())
            {
                serializer.Serialize(stream, obj);
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }

        /// <summary>
        /// 将 XML 字符串反序列化为对象
        /// </summary>
        public static T FromXml<T>(string xml)
        {
            if (string.IsNullOrEmpty(xml))
            {
                return default(T);
            }

            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (StringReader reader = new StringReader(xml))
            {
                return (T)serializer.Deserialize(reader);
            }
        }

        /// <summary>
        /// 将对象转换为字典
        /// </summary>
        public static Dictionary<string, object>? ToDictionary(object obj)
        {
            if (IsNull(obj))
            {
                return null;
            }

            Dictionary<string, object> dictionary = new Dictionary<string, object>();

            foreach (PropertyInfo property in GetProperties(obj))
            {
                dictionary[property.Name] = property.GetValue(obj);
            }

            foreach (FieldInfo field in GetFields(obj))
            {
                dictionary[field.Name] = field.GetValue(obj);
            }

            return dictionary;
        }

        /// <summary>
        /// 将字典转换为对象
        /// </summary>
        public static T FromDictionary<T>(Dictionary<string, object> dictionary) where T : new()
        {
            if (dictionary == null)
            {
                return default(T);
            }

            T obj = new T();

            foreach (PropertyInfo property in GetProperties(obj))
            {
                if (dictionary.TryGetValue(property.Name, out object value))
                {
                    property.SetValue(obj, Convert(value, property.PropertyType));
                }
            }

            foreach (FieldInfo field in GetFields(obj))
            {
                if (dictionary.TryGetValue(field.Name, out object value))
                {
                    field.SetValue(obj, Convert(value, field.FieldType));
                }
            }

            return obj;
        }

        /// <summary>
        /// 比较两个对象的差异（属性值或字段值不同）
        /// </summary>
        public static IEnumerable<string> Compare(object obj1, object obj2)
        {
            if (IsNull(obj1) && IsNull(obj2))
            {
                return Enumerable.Empty<string>();
            }

            if (IsNull(obj1) || IsNull(obj2))
            {
                throw new ArgumentNullException("比较对象不能为 null");
            }

            List<string> differences = new List<string>();

            foreach (PropertyInfo property in GetProperties(obj1))
            {
                object value1 = property.GetValue(obj1);
                object value2 = property.GetValue(obj2);

                if (!Equals(value1, value2))
                {
                    differences.Add($"属性 {property.Name} 的值不同：{value1} -> {value2}");
                }
            }

            foreach (FieldInfo field in GetFields(obj1))
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

        /// <summary>
        /// 获取对象的哈希码
        /// [Obsolete("请直接使用 obj?.GetHashCode() ?? 0")]
        /// </summary>
        [Obsolete("请直接使用 obj?.GetHashCode() ?? 0", false)]
        public static int GetHashCode(object obj)
        {
            if (IsNull(obj))
            {
                return 0;
            }

            return obj.GetHashCode();
        }

        /// <summary>
        /// 深拷贝对象
        /// </summary>
        public static T? DeepClone<T>(T obj)
        {
            if (IsNull(obj))
            {
                return default(T);
            }

            DataContractSerializer serializer = new DataContractSerializer(obj.GetType());

            using (MemoryStream stream = new MemoryStream())
            {
                serializer.WriteObject(stream, obj);
                stream.Position = 0;
                return (T)serializer.ReadObject(stream);
            }
        }

        /// <summary>
        /// 判断对象是否为值类型
        /// [Obsolete("请直接使用 obj?.GetType().IsValueType ?? false")]
        /// </summary>
        [Obsolete("请直接使用 obj?.GetType().IsValueType ?? false", false)]
        public static bool IsValueType(object obj)
        {
            if (IsNull(obj))
            {
                return false;
            }

            return obj.GetType().IsValueType;
        }

        /// <summary>
        /// 将对象转换为键值对集合
        /// </summary>
        public static IEnumerable<KeyValuePair<string, object>> ToKeyValuePairs(object obj)
        {
            if (IsNull(obj))
            {
                return Enumerable.Empty<KeyValuePair<string, object>>();
            }

            List<KeyValuePair<string, object>> pairs = new List<KeyValuePair<string, object>>();

            foreach (PropertyInfo property in GetProperties(obj))
            {
                pairs.Add(new KeyValuePair<string, object>(property.Name, property.GetValue(obj)));
            }

            foreach (FieldInfo field in GetFields(obj))
            {
                pairs.Add(new KeyValuePair<string, object>(field.Name, field.GetValue(obj)));
            }

            return pairs;
        }

        /// <summary>
        /// 深度复制对象
        /// </summary>
        public static object? DeepCopy(object obj)
        {
            if (obj == null)
            {
                return null;
            }

            Type type = obj.GetType();

            if (IsSimpleType(type))
            {
                return obj;
            }

            if (IsEnumerableType(type))
            {
                Type elementType = type.GetElementType() ?? type.GetGenericArguments().FirstOrDefault();

                if (elementType == null || IsSimpleType(elementType))
                {
                    return obj;
                }

                IList list = (IList)Activator.CreateInstance(type);

                foreach (object item in (IEnumerable)obj)
                {
                    list.Add(DeepCopy(item));
                }

                return list;
            }

            object clone = Activator.CreateInstance(type);

            foreach (PropertyInfo propertyInfo in GetProperties(type))
            {
                if (!propertyInfo.CanRead || !propertyInfo.CanWrite)
                {
                    continue;
                }

                object value = GetPropertyValue(obj, propertyInfo.Name);

                if (value == null)
                {
                    continue;
                }

                if (IsSimpleType(propertyInfo.PropertyType))
                {
                    SetPropertyValue(clone, propertyInfo.Name, value);
                }
                else if (IsEnumerableType(propertyInfo.PropertyType))
                {
                    object enumerable = DeepCopy(value);
                    SetPropertyValue(clone, propertyInfo.Name, enumerable);
                }
                else
                {
                    object childClone = DeepCopy(value);
                    SetPropertyValue(clone, propertyInfo.Name, childClone);
                }
            }

            foreach (FieldInfo fieldInfo in GetFields(type))
            {
                object value = GetFieldValue(obj, fieldInfo.Name);

                if (value == null)
                {
                    continue;
                }

                if (IsSimpleType(fieldInfo.FieldType))
                {
                    SetFieldValue(clone, fieldInfo.Name, value);
                }
                else if (IsEnumerableType(fieldInfo.FieldType))
                {
                    object enumerable = DeepCopy(value);
                    SetFieldValue(clone, fieldInfo.Name, enumerable);
                }
                else
                {
                    object childClone = DeepCopy(value);
                    SetFieldValue(clone, fieldInfo.Name, childClone);
                }
            }

            return clone;
        }

        /// <summary>
        /// 将源对象的属性复制到目标对象中
        /// </summary>
        public static void CopyProperties(object source, object target)
        {
            Type sourceType = source.GetType();
            Type targetType = target.GetType();

            foreach (PropertyInfo sourceProperty in GetProperties(sourceType))
            {
                if (!sourceProperty.CanRead)
                {
                    continue;
                }

                PropertyInfo targetProperty = GetProperty(targetType, sourceProperty.Name);

                if (targetProperty == null || !targetProperty.CanWrite)
                {
                    continue;
                }

                object value = GetPropertyValue(source, sourceProperty.Name);
                SetPropertyValue(target, targetProperty.Name, value);
            }
        }

        /// <summary>
        /// 获取指定类型的 Type 对象
        /// [Obsolete("请直接使用 Type.GetType(typeName)")]
        /// </summary>
        [Obsolete("请直接使用 Type.GetType(typeName)", false)]
        public static Type GetType(string typeName)
        {
            return Type.GetType(typeName);
        }

        /// <summary>
        /// 获取对象的 Type 对象
        /// [Obsolete("请直接使用 obj.GetType()")]
        /// </summary>
        [Obsolete("请直接使用 obj.GetType()", false)]
        public static Type GetType(object obj)
        {
            return obj.GetType();
        }

        /// <summary>
        /// 获取类型的所有成员信息，包括字段、属性、方法和事件等
        /// [Obsolete("请直接使用 type.GetMembers()")]
        /// </summary>
        [Obsolete("请直接使用 type.GetMembers()", false)]
        public static MemberInfo[] GetMembers(Type type)
        {
            return type.GetMembers();
        }

        /// <summary>
        /// 获取类型的所有属性信息
        /// [Obsolete("请直接使用 type.GetProperties()")]
        /// </summary>
        [Obsolete("请直接使用 type.GetProperties()", false)]
        public static PropertyInfo[] GetProperties(Type type)
        {
            return type.GetProperties();
        }

        /// <summary>
        /// 获取类型的所有字段信息
        /// [Obsolete("请直接使用 type.GetFields()")]
        /// </summary>
        [Obsolete("请直接使用 type.GetFields()", false)]
        public static FieldInfo[] GetFields(Type type)
        {
            return type.GetFields();
        }

        /// <summary>
        /// 获取指定名称的属性信息
        /// [Obsolete("请直接使用 type.GetProperty(propertyName)")]
        /// </summary>
        [Obsolete("请直接使用 type.GetProperty(propertyName)", false)]
        public static PropertyInfo GetProperty(Type type, string propertyName)
        {
            return type.GetProperty(propertyName);
        }

        /// <summary>
        /// 获取指定名称的属性信息
        /// [Obsolete("请直接使用 obj.GetType().GetProperty(propertyName)")]
        /// </summary>
        [Obsolete("请直接使用 obj.GetType().GetProperty(propertyName)", false)]
        public static PropertyInfo GetProperty(object obj, string propertyName)
        {
            return obj.GetType().GetProperty(propertyName);
        }

        /// <summary>
        /// 获取指定名称的字段信息
        /// [Obsolete("请直接使用 type.GetField(fieldName)")]
        /// </summary>
        [Obsolete("请直接使用 type.GetField(fieldName)", false)]
        public static FieldInfo GetField(Type type, string fieldName)
        {
            return type.GetField(fieldName);
        }

        /// <summary>
        /// 获取指定名称的字段信息
        /// [Obsolete("请直接使用 obj.GetType().GetField(fieldName)")]
        /// </summary>
        [Obsolete("请直接使用 obj.GetType().GetField(fieldName)", false)]
        public static FieldInfo GetField(object obj, string fieldName)
        {
            return obj.GetType().GetField(fieldName);
        }

        /// <summary>
        /// 获取指定名称的方法信息
        /// [Obsolete("请直接使用 type.GetMethod(methodName)")]
        /// </summary>
        [Obsolete("请直接使用 type.GetMethod(methodName)", false)]
        public static MethodInfo GetMethod(Type type, string methodName)
        {
            return type.GetMethod(methodName);
        }

        /// <summary>
        /// 获取指定名称的方法信息
        /// [Obsolete("请直接使用 obj.GetType().GetMethod(methodName)")]
        /// </summary>
        [Obsolete("请直接使用 obj.GetType().GetMethod(methodName)", false)]
        public static MethodInfo GetMethod(object obj, string methodName)
        {
            return obj.GetType().GetMethod(methodName);
        }

        /// <summary>
        /// 获取指定名称和参数类型的方法信息
        /// [Obsolete("请直接使用 type.GetMethod(methodName, parameterTypes)")]
        /// </summary>
        [Obsolete("请直接使用 type.GetMethod(methodName, parameterTypes)", false)]
        public static MethodInfo GetMethod(Type type, string methodName, Type[] parameterTypes)
        {
            return type.GetMethod(methodName, parameterTypes);
        }

        /// <summary>
        /// 获取指定名称和参数类型的方法信息
        /// [Obsolete("请直接使用 obj.GetType().GetMethod(methodName, parameterTypes)")]
        /// </summary>
        [Obsolete("请直接使用 obj.GetType().GetMethod(methodName, parameterTypes)", false)]
        public static MethodInfo GetMethod(object obj, string methodName, Type[] parameterTypes)
        {
            return obj.GetType().GetMethod(methodName, parameterTypes);
        }

        /// <summary>
        /// 调用对象的指定方法
        /// </summary>
        public static object InvokeMethod(object obj, string methodName, object[] parameters)
        {
            Type type = obj.GetType();
            MethodInfo methodInfo = GetMethod(type, methodName);
            return methodInfo.Invoke(obj, parameters);
        }

        /// <summary>
        /// 调用对象的指定方法
        /// </summary>
        public static object InvokeMethod(object obj, string methodName, Type[] parameterTypes, object[] parameters)
        {
            Type type = obj.GetType();
            MethodInfo methodInfo = GetMethod(type, methodName, parameterTypes);
            return methodInfo.Invoke(obj, parameters);
        }

        /// <summary>
        /// 创建指定类型的实例
        /// [Obsolete("请直接使用 Activator.CreateInstance(type, constructorParameters)")]
        /// </summary>
        [Obsolete("请直接使用 Activator.CreateInstance(type, constructorParameters)", false)]
        public static object CreateInstance(Type type, object[] constructorParameters)
        {
            return Activator.CreateInstance(type, constructorParameters);
        }


        /// <summary>
        /// 判断指定类型是否派生自指定的基类或接口
        /// [Obsolete("请直接使用 type.IsSubclassOf(baseType)")]
        /// </summary>
        [Obsolete("请直接使用 type.IsSubclassOf(baseType)", false)]
        public static bool IsSubclassOf(Type type, Type baseType)
        {
            return type.IsSubclassOf(baseType);
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

        /// <summary>
        /// 获取指定类型实现的所有接口类型
        /// [Obsolete("请直接使用 type.GetInterfaces()")]
        /// </summary>
        [Obsolete("请直接使用 type.GetInterfaces()", false)]
        public static Type[] GetInterfaces(Type type)
        {
            return type.GetInterfaces();
        }

        /// <summary>
        /// 获取指定类型的程序集限定名
        /// [Obsolete("请直接使用 type.AssemblyQualifiedName")]
        /// </summary>
        [Obsolete("请直接使用 type.AssemblyQualifiedName", false)]
        public static string GetAssemblyQualifiedName(Type type)
        {
            return type.AssemblyQualifiedName;
        }

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
        public static bool IsDefaultValue(object obj)
        {
            return obj == null || obj.Equals(GetDefault(obj.GetType()));
        }

        /// <summary>
        /// 判断指定类型是否是可空类型
        /// [Obsolete("请直接使用 Nullable.GetUnderlyingType(type) != null")]
        /// </summary>
        [Obsolete("请直接使用 Nullable.GetUnderlyingType(type) != null", false)]
        public static bool IsNullable(Type type)
        {
            return Nullable.GetUnderlyingType(type) != null;
        }

        /// <summary>
        /// 获取可空类型的基础类型
        /// [Obsolete("请直接使用 Nullable.GetUnderlyingType(type) ?? type")]
        /// </summary>
        [Obsolete("请直接使用 Nullable.GetUnderlyingType(type) ?? type", false)]
        public static Type GetNullableType(Type type)
        {
            return Nullable.GetUnderlyingType(type) ?? type;
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

            if (IsEnumType(type))
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
        /// 判断指定类型是否是枚举类型
        /// [Obsolete("请直接使用 type.IsEnum")]
        /// </summary>
        [Obsolete("请直接使用 type.IsEnum", false)]
        public static bool IsEnumType(Type type)
        {
            return type.IsEnum;
        }

        /// <summary>
        /// 判断指定类型是否是集合类型
        /// </summary>
        public static bool IsEnumerableType(Type type)
        {
            return typeof(IEnumerable).IsAssignableFrom(type);
        }

        /// <summary>
        /// 将对象转换为动态扩展对象
        /// </summary>
        public static dynamic? ToDynamic(object obj)
        {
            if (obj == null)
            {
                return null;
            }

            IDictionary<string, object> dictionary = new ExpandoObject();

            foreach (PropertyInfo propertyInfo in GetProperties(obj.GetType()))
            {
                if (!propertyInfo.CanRead)
                {
                    continue;
                }

                object value = GetPropertyValue(obj, propertyInfo.Name);
                dictionary.Add(propertyInfo.Name, value);
            }

            foreach (FieldInfo fieldInfo in GetFields(obj.GetType()))
            {
                object value = GetFieldValue(obj, fieldInfo.Name);
                dictionary.Add(fieldInfo.Name, value);
            }

            return dictionary;
        }

#if NET6_0_OR_GREATER

        /// <summary>
        /// 将对象序列化为 JSON 字符串
        /// </summary>
        public static string SerializeToJson(object obj)
        {
            return System.Text.Json.JsonSerializer.Serialize(obj);
        }

        /// <summary>
        /// 将 JSON 字符串反序列化为指定类型的对象
        /// </summary>
        public static T? DeserializeFromJson<T>(string json)
        {
            return System.Text.Json.JsonSerializer.Deserialize<T>(json);
        }

#endif

        /// <summary>
        /// 将对象序列化为 XML 字符串
        /// </summary>
        public static string SerializeToXml(object obj)
        {
            XmlSerializer serializer = new XmlSerializer(obj.GetType());

            using (StringWriter writer = new StringWriter())
            {
                serializer.Serialize(writer, obj);
                return writer.ToString();
            }
        }

        /// <summary>
        /// 将 XML 字符串反序列化为指定类型的对象
        /// </summary>
        public static object? DeserializeFromXml(string xml, Type type)
        {
            XmlSerializer serializer = new XmlSerializer(type);

            using (StringReader reader = new StringReader(xml))
            {
                return serializer.Deserialize(reader);
            }
        }

        /// <summary>
        /// 将对象序列化为二进制数据
        /// </summary>
        [Obsolete("BinaryFormatter is obsolete and unsafe. Use SerializeToJson or SerializeToXml instead.")]
        public static byte[] SerializeToBinary(object obj)
        {
#pragma warning disable SYSLIB0011 // 类型或成员已过时
            BinaryFormatter formatter = new BinaryFormatter();

            using (MemoryStream stream = new MemoryStream())
            {
                formatter.Serialize(stream, obj);
                return stream.ToArray();
            }
#pragma warning restore SYSLIB0011 // 类型或成员已过时
        }

        /// <summary>
        /// 将二进制数据反序列化为指定类型的对象
        /// </summary>
        [Obsolete("BinaryFormatter is obsolete and unsafe. Use DeserializeFromJson or DeserializeFromXml instead.")]
        public static object DeserializeFromBinary(byte[] data, Type type)
        {
#pragma warning disable SYSLIB0011 // 类型或成员已过时
            BinaryFormatter formatter = new BinaryFormatter();

            using (MemoryStream stream = new MemoryStream(data))
            {
                return formatter.Deserialize(stream);
            }
#pragma warning restore SYSLIB0011 // 类型或成员已过时
        }
    }
}
