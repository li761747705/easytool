using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EasyTool.ReflectCategory
{
    /// <summary>
    /// 类型工具类
    /// </summary>
    public static class TypeUtil
    {
        #region 类型判断

        /// <summary>
        /// 判断是否为简单类型
        /// </summary>
        public static bool IsSimpleType(Type type)
        {
            if (type == null) return false;

            return type.IsPrimitive ||
                   type.IsEnum ||
                   type == typeof(string) ||
                   type == typeof(decimal) ||
                   type == typeof(DateTime) ||
                   type == typeof(DateTimeOffset) ||
                   type == typeof(TimeSpan) ||
                   type == typeof(Guid) ||
                   type == typeof(byte[]) ||
                   Nullable.GetUnderlyingType(type) != null && IsSimpleType(Nullable.GetUnderlyingType(type)!);
        }

        /// <summary>
        /// 判断是否为可空类型
        /// </summary>
        public static bool IsNullableType(Type type)
        {
            return type != null && Nullable.GetUnderlyingType(type) != null;
        }

        /// <summary>
        /// 判断是否为集合类型
        /// </summary>
        public static bool IsCollectionType(Type type)
        {
            if (type == null) return false;
            return type != typeof(string) && typeof(System.Collections.IEnumerable).IsAssignableFrom(type);
        }

        /// <summary>
        /// 判断是否为字典类型
        /// </summary>
        public static bool IsDictionaryType(Type type)
        {
            if (type == null) return false;

            return type.GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>));
        }

        /// <summary>
        /// 判断是否为元组类型
        /// </summary>
        public static bool IsTupleType(Type type)
        {
            if (type == null) return false;

            if (!type.IsGenericType)
                return false;

            var definition = type.GetGenericTypeDefinition();
            return definition == typeof(Tuple<>) ||
                   definition == typeof(Tuple<,>) ||
                   definition == typeof(Tuple<,,>) ||
                   definition == typeof(Tuple<,,,>) ||
                   definition == typeof(Tuple<,,,,>) ||
                   definition == typeof(Tuple<,,,,,>) ||
                   definition == typeof(Tuple<,,,,,,>) ||
                   definition == typeof(Tuple<,,,,,,,>) ||
                   definition == typeof(ValueTuple<>) ||
                   definition == typeof(ValueTuple<,>) ||
                   definition == typeof(ValueTuple<,,>) ||
                   definition == typeof(ValueTuple<,,,>) ||
                   definition == typeof(ValueTuple<,,,,>) ||
                   definition == typeof(ValueTuple<,,,,,>) ||
                   definition == typeof(ValueTuple<,,,,,,>) ||
                   definition == typeof(ValueTuple<,,,,,,,>);
        }

        /// <summary>
        /// 获取可空类型的基类型
        /// </summary>
        public static Type? GetUnderlyingType(Type type)
        {
            return Nullable.GetUnderlyingType(type);
        }

        /// <summary>
        /// 获取集合元素类型
        /// </summary>
        public static Type? GetElementType(Type type)
        {
            if (type == null) return null;

            if (type.IsArray)
                return type.GetElementType();

            if (type.IsGenericType)
            {
                var genericArgs = type.GetGenericArguments();
                if (genericArgs.Length > 0)
                {
                    // 对于 IEnumerable<T>、List<T> 等
                    if (typeof(System.Collections.IEnumerable).IsAssignableFrom(type))
                    {
                        return genericArgs[0];
                    }
                }
            }

            return null;
        }

        #endregion

        #region 类型创建

        /// <summary>
        /// 创建实例
        /// </summary>
        public static object? CreateInstance(Type type, params object[] args)
        {
            if (type == null) return null;

            if (args == null || args.Length == 0)
            {
                return Activator.CreateInstance(type);
            }

            return Activator.CreateInstance(type, args);
        }

        /// <summary>
        /// 创建泛型实例
        /// </summary>
        public static object? CreateGenericInstance(Type genericType, Type[] typeArguments, params object[] args)
        {
            if (genericType == null || typeArguments == null) return null;

            if (!genericType.IsGenericTypeDefinition)
                throw new ArgumentException("类型必须是泛型定义");

            var constructedType = genericType.MakeGenericType(typeArguments);
            return CreateInstance(constructedType, args);
        }

        #endregion

        #region 属性/字段访问

        /// <summary>
        /// 获取所有属性
        /// </summary>
        public static PropertyInfo[] GetProperties(Type type, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance)
        {
            return type?.GetProperties(bindingFlags) ?? Array.Empty<PropertyInfo>();
        }

        /// <summary>
        /// 获取属性
        /// </summary>
        public static PropertyInfo? GetProperty(Type type, string propertyName, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance)
        {
            return type?.GetProperty(propertyName, bindingFlags);
        }

        /// <summary>
        /// 获取属性值
        /// </summary>
        public static object? GetPropertyValue(object obj, string propertyName)
        {
            if (obj == null) return null;

            var type = obj.GetType();
            var property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            return property?.GetValue(obj);
        }

        /// <summary>
        /// 设置属性值
        /// </summary>
        public static void SetPropertyValue(object obj, string propertyName, object? value)
        {
            if (obj == null) return;

            var type = obj.GetType();
            var property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            property?.SetValue(obj, value);
        }

        /// <summary>
        /// 获取所有字段
        /// </summary>
        public static FieldInfo[] GetFields(Type type, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance)
        {
            return type?.GetFields(bindingFlags) ?? Array.Empty<FieldInfo>();
        }

        /// <summary>
        /// 获取字段值
        /// </summary>
        public static object? GetFieldValue(object obj, string fieldName)
        {
            if (obj == null) return null;

            var type = obj.GetType();
            var field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            return field?.GetValue(obj);
        }

        /// <summary>
        /// 设置字段值
        /// </summary>
        public static void SetFieldValue(object obj, string fieldName, object? value)
        {
            if (obj == null) return;

            var type = obj.GetType();
            var field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            field?.SetValue(obj, value);
        }

        #endregion

        #region 方法调用

        /// <summary>
        /// 获取所有方法
        /// </summary>
        public static MethodInfo[] GetMethods(Type type, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance)
        {
            return type?.GetMethods(bindingFlags) ?? Array.Empty<MethodInfo>();
        }

        /// <summary>
        /// 获取方法
        /// </summary>
        public static MethodInfo? GetMethod(Type type, string methodName, Type[]? parameterTypes = null, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance)
        {
            if (type == null) return null;

            if (parameterTypes == null)
                return type.GetMethod(methodName, bindingFlags);

            return type.GetMethod(methodName, bindingFlags, null, parameterTypes, null);
        }

        /// <summary>
        /// 调用方法
        /// </summary>
        public static object? InvokeMethod(object obj, string methodName, params object[] args)
        {
            if (obj == null) return null;

            var type = obj.GetType();
            var argTypes = args?.Select(a => a?.GetType() ?? typeof(object)).ToArray();
            var method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance, null, argTypes, null);

            return method?.Invoke(obj, args);
        }

        /// <summary>
        /// 调用静态方法
        /// </summary>
        public static object? InvokeStaticMethod(Type type, string methodName, params object[] args)
        {
            if (type == null) return null;

            var argTypes = args?.Select(a => a?.GetType() ?? typeof(object)).ToArray();
            var method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static, null, argTypes, null);

            return method?.Invoke(null, args);
        }

        #endregion

        #region 类型继承

        /// <summary>
        /// 判断类型是否继承自指定类型
        /// </summary>
        public static bool IsAssignableTo(Type type, Type targetType)
        {
            return targetType?.IsAssignableFrom(type) ?? false;
        }

        /// <summary>
        /// 获取基类型
        /// </summary>
        public static Type? GetBaseType(Type type)
        {
            return type?.BaseType;
        }

        /// <summary>
        /// 获取所有接口
        /// </summary>
        public static Type[] GetInterfaces(Type type)
        {
            return type?.GetInterfaces() ?? Array.Empty<Type>();
        }

        /// <summary>
        /// 获取继承层次
        /// </summary>
        public static IEnumerable<Type> GetInheritanceHierarchy(Type type)
        {
            if (type == null) yield break;

            var current = type;
            while (current != null && current != typeof(object))
            {
                yield return current;
                current = current.BaseType;
            }

            if (type != typeof(object))
                yield return typeof(object);
        }

        #endregion

        #region 特性

        /// <summary>
        /// 获取特性
        /// </summary>
        public static T? GetAttribute<T>(MemberInfo member) where T : Attribute
        {
            return member?.GetCustomAttribute<T>();
        }

        /// <summary>
        /// 获取所有特性
        /// </summary>
        public static IEnumerable<T> GetAttributes<T>(MemberInfo member) where T : Attribute
        {
            return member?.GetCustomAttributes<T>() ?? Enumerable.Empty<T>();
        }

        /// <summary>
        /// 检查是否有特性
        /// </summary>
        public static bool HasAttribute<T>(MemberInfo member) where T : Attribute
        {
            return member?.IsDefined(typeof(T), true) ?? false;
        }

        #endregion

        #region 类型信息

        /// <summary>
        /// 获取类型友好名称
        /// </summary>
        public static string GetFriendlyName(Type type)
        {
            if (type == null) return string.Empty;

            if (!type.IsGenericType)
                return type.Name;

            var name = type.Name;
            var backtickIndex = name.IndexOf('`');
            if (backtickIndex >= 0)
                name = name.Substring(0, backtickIndex);

            var genericArgs = type.GetGenericArguments();
            var argNames = string.Join(", ", genericArgs.Select(GetFriendlyName));

            return $"{name}<{argNames}>";
        }

        /// <summary>
        /// 获取默认值
        /// </summary>
        public static object? GetDefaultValue(Type type)
        {
            if (type == null) return null;

            if (type.IsValueType)
                return Activator.CreateInstance(type);

            return null;
        }

        #endregion
    }
}