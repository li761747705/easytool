using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace EasyTool.Extension
{
    /// <summary>
    /// Type 类型扩展方法
    /// </summary>
    public static class TypeExtension
    {
        #region 类型判断

        /// <summary>
        /// 判断是否是简单类型（值类型、字符串等）
        /// </summary>
        public static bool IsSimpleType(this Type? type)
        {
            if (type == null)
                return false;

            return type.IsValueType ||
                   type == typeof(string) ||
                   type == typeof(decimal) ||
                   type == typeof(DateTime) ||
                   type == typeof(DateTimeOffset) ||
                   type == typeof(TimeSpan) ||
                   type == typeof(Guid);
        }

        /// <summary>
        /// 判断是否是数字类型
        /// </summary>
        public static bool IsNumericType(this Type? type)
        {
            if (type == null)
                return false;

            type = Nullable.GetUnderlyingType(type) ?? type;

            return type == typeof(byte) ||
                   type == typeof(sbyte) ||
                   type == typeof(short) ||
                   type == typeof(ushort) ||
                   type == typeof(int) ||
                   type == typeof(uint) ||
                   type == typeof(long) ||
                   type == typeof(ulong) ||
                   type == typeof(float) ||
                   type == typeof(double) ||
                   type == typeof(decimal);
        }

        /// <summary>
        /// 判断是否是集合类型
        /// </summary>
        public static bool IsCollectionType(this Type? type)
        {
            if (type == null)
                return false;

            return type != typeof(string) && typeof(System.Collections.IEnumerable).IsAssignableFrom(type);
        }

        /// <summary>
        /// 判断是否是字典类型
        /// </summary>
        public static bool IsDictionaryType(this Type? type)
        {
            if (type == null)
                return false;

            return typeof(System.Collections.IDictionary).IsAssignableFrom(type) ||
                   (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(System.Collections.Generic.Dictionary<,>));
        }

        /// <summary>
        /// 判断是否是可空值类型
        /// </summary>
        public static bool IsNullableType(this Type? type)
        {
            if (type == null)
                return false;

            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        /// <summary>
        /// 判断是否是泛型类型
        /// </summary>
        public static bool IsGenericType(this Type? type, Type genericTypeDefinition)
        {
            if (type == null || !type.IsGenericType)
                return false;

            return type.GetGenericTypeDefinition() == genericTypeDefinition;
        }

        /// <summary>
        /// 判断是否是某个泛型类型的子类
        /// </summary>
        public static bool IsSubclassOfGeneric(this Type? type, Type genericTypeDefinition)
        {
            if (type == null || genericTypeDefinition == null)
                return false;

            while (type != null && type != typeof(object))
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == genericTypeDefinition)
                    return true;

                type = type.BaseType;
            }

            return false;
        }

        /// <summary>
        /// 判断是否是匿名类型
        /// </summary>
        public static bool IsAnonymousType(this Type? type)
        {
            if (type == null)
                return false;

            return Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute), false) &&
                   type.IsGenericType &&
                   type.Name.Contains("AnonymousType") &&
                   (type.Name.StartsWith("<>") || type.Name.StartsWith("VB$"));
        }

        #endregion

        #region 类型名称

        /// <summary>
        /// 获取友好的类型名称
        /// </summary>
        public static string GetFriendlyName(this Type? type)
        {
            if (type == null)
                return "null";

            if (type == typeof(int))
                return "int";
            if (type == typeof(uint))
                return "uint";
            if (type == typeof(long))
                return "long";
            if (type == typeof(ulong))
                return "ulong";
            if (type == typeof(short))
                return "short";
            if (type == typeof(ushort))
                return "ushort";
            if (type == typeof(byte))
                return "byte";
            if (type == typeof(sbyte))
                return "sbyte";
            if (type == typeof(float))
                return "float";
            if (type == typeof(double))
                return "double";
            if (type == typeof(decimal))
                return "decimal";
            if (type == typeof(bool))
                return "bool";
            if (type == typeof(string))
                return "string";
            if (type == typeof(char))
                return "char";
            if (type == typeof(object))
                return "object";
            if (type == typeof(void))
                return "void";

            if (type.IsGenericType)
            {
                var genericArgs = type.GetGenericArguments();
                var genericTypeName = type.GetGenericTypeDefinition().GetFriendlyName();
                var genericArgsNames = string.Join(", ", genericArgs.Select(t => t.GetFriendlyName()));
                return genericTypeName + "<" + genericArgsNames + ">";
            }

            if (type.IsArray)
            {
                var elementType = type.GetElementType();
                return $"{elementType.GetFriendlyName()}[]";
            }

            return type.Name;
        }

        /// <summary>
        /// 获取类型的显示名称
        /// </summary>
        public static string GetDisplayName(this Type? type)
        {
            if (type == null)
                return string.Empty;

            var attr = type.GetCustomAttribute<DisplayNameAttribute>();
            return attr?.DisplayName ?? type.Name;
        }

        /// <summary>
        /// 获取类型的描述
        /// </summary>
        public static string GetDescription(this Type? type)
        {
            if (type == null)
                return string.Empty;

            var attr = type.GetCustomAttribute<DescriptionAttribute>();
            return attr?.Description ?? string.Empty;
        }

        #endregion

        #region 默认值

        /// <summary>
        /// 获取类型的默认值
        /// </summary>
        public static object? GetDefaultValue(this Type? type)
        {
            if (type == null)
                return null;

            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }

        #endregion

        #region 泛型处理

        /// <summary>
        /// 获取可空类型的实际类型
        /// [Obsolete("请直接使用 Nullable.GetUnderlyingType(type) ?? type")]
        /// </summary>
        [Obsolete("请直接使用 Nullable.GetUnderlyingType(type) ?? type", false)]
        public static Type? GetNullableType(this Type? type)
        {
            if (type == null)
                return null;

            return Nullable.GetUnderlyingType(type) ?? type;
        }

        /// <summary>
        /// 获取集合的元素类型
        /// </summary>
        public static Type? GetElementType(this Type? type)
        {
            if (type == null)
                return null;

            if (type.IsArray)
                return type.GetElementType();

            if (type.IsCollectionType())
            {
                var genericArgs = type.GetGenericArguments();
                if (genericArgs.Length > 0)
                    return genericArgs[0];
            }

            return null;
        }

        /// <summary>
        /// 获取字典的键值对类型
        /// </summary>
        public static (Type? KeyType, Type? ValueType) GetDictionaryKeyValueTypes(this Type? type)
        {
            if (type == null)
                return (null, null);

            if (typeof(System.Collections.IDictionary).IsAssignableFrom(type))
            {
                var interfaces = type.GetInterfaces();
                var dictInterface = interfaces.FirstOrDefault(i =>
                    i.IsGenericType && i.GetGenericTypeDefinition() == typeof(System.Collections.Generic.IDictionary<,>));

                if (dictInterface != null)
                {
                    var genericArgs = dictInterface.GetGenericArguments();
                    return (genericArgs[0], genericArgs[1]);
                }
            }

            return (null, null);
        }

        #endregion

        #region 特性操作

        /// <summary>
        /// 判断是否有指定特性
        /// </summary>
        public static bool HasAttribute<T>(this Type? type) where T : Attribute
        {
            if (type == null)
                return false;

            return type.GetCustomAttribute<T>() != null;
        }

        /// <summary>
        /// 判断是否有指定特性
        /// </summary>
        public static bool HasAttribute(this Type? type, Type? attributeType)
        {
            if (type == null || attributeType == null)
                return false;

            return type.GetCustomAttributes(attributeType, false).Any();
        }

        /// <summary>
        /// 获取指定特性
        /// </summary>
        public static T? GetAttribute<T>(this Type? type) where T : Attribute
        {
            if (type == null)
                return null;

            return type.GetCustomAttribute<T>();
        }

        /// <summary>
        /// 获取所有指定特性
        /// </summary>
        public static T[] GetAttributes<T>(this Type? type) where T : Attribute
        {
            if (type == null)
                return Array.Empty<T>();

            return type.GetCustomAttributes<T>().ToArray();
        }

        #endregion

        #region 成员获取

        /// <summary>
        /// 获取所有属性（包含继承的）
        /// </summary>
        public static PropertyInfo[] GetAllProperties(this Type? type)
        {
            if (type == null)
                return Array.Empty<PropertyInfo>();

            var properties = new List<PropertyInfo>();
            var currentType = type;

            while (currentType != null && currentType != typeof(object))
            {
                var currentProps = currentType.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);
                properties.AddRange(currentProps);
                currentType = currentType.BaseType;
            }

            return properties.ToArray();
        }

        /// <summary>
        /// 获取所有字段（包含继承的）
        /// </summary>
        public static FieldInfo[] GetAllFields(this Type? type)
        {
            if (type == null)
                return Array.Empty<FieldInfo>();

            var fields = new List<FieldInfo>();
            var currentType = type;

            while (currentType != null && currentType != typeof(object))
            {
                var currentFields = currentType.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                fields.AddRange(currentFields);
                currentType = currentType.BaseType;
            }

            return fields.ToArray();
        }

        #endregion

        #region 类型转换

        /// <summary>
        /// 尝试将值转换为指定类型
        /// </summary>
        public static object? ChangeType(this Type type, object? value)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (value == null)
            {
                if (type.IsValueType)
                    return Activator.CreateInstance(type);
                return null;
            }

            var valueType = value.GetType();

            // 如果类型相同或目标类型是源类型的父类
            if (type.IsAssignableFrom(valueType))
                return value;

            // 可空类型处理
            var underlyingType = Nullable.GetUnderlyingType(type);
            if (underlyingType != null)
                return ChangeType(underlyingType, value);

            // 枚举处理
            if (type.IsEnum && value is string str)
                return Enum.Parse(type, str, true);

            // 标准转换
            return Convert.ChangeType(value, type);
        }

        #endregion
    }
}
