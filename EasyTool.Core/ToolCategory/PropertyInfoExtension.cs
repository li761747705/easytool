using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace EasyTool.Extension
{
    /// <summary>
    /// PropertyInfo 扩展方法
    /// </summary>
    public static class PropertyInfoExtension
    {
        #region 值获取

        /// <summary>
        /// 安全获取属性值，失败返回默认值
        /// </summary>
        public static object? GetValueOrDefault(this PropertyInfo? property, object? obj)
        {
            if (property == null || obj == null)
                return null;

            try
            {
                return property.GetValue(obj);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 安全获取属性值，失败返回指定默认值
        /// </summary>
        public static T? GetValueOrDefault<T>(this PropertyInfo? property, object? obj, T? defaultValue = default)
        {
            if (property == null || obj == null)
                return defaultValue;

            try
            {
                var value = property.GetValue(obj);
                if (value == null)
                    return defaultValue;

                return (T)value;
            }
            catch
            {
                return defaultValue;
            }
        }

        #endregion

        #region 值设置

        /// <summary>
        /// 安全设置属性值
        /// </summary>
        public static bool SetValueSafe(this PropertyInfo? property, object? obj, object? value)
        {
            if (property == null || obj == null)
                return false;

            try
            {
                if (!property.CanWrite)
                    return false;

                property.SetValue(obj, value);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 设置属性值（支持类型转换）
        /// </summary>
        public static bool SetValueWithConvert(this PropertyInfo? property, object? obj, object? value)
        {
            if (property == null || obj == null || !property.CanWrite)
                return false;

            try
            {
                object? convertedValue = value;

                // 如果类型不匹配，尝试转换
                if (value != null && value.GetType() != property.PropertyType)
                {
                    // 处理可空类型
                    var targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

                    if (targetType.IsEnum && value is string str)
                    {
                        convertedValue = Enum.Parse(targetType, str);
                    }
                    else
                    {
                        convertedValue = Convert.ChangeType(value, targetType);
                    }
                }

                property.SetValue(obj, convertedValue);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region 特性检查

        /// <summary>
        /// 判断属性是否有指定特性
        /// </summary>
        public static bool HasAttribute<T>(this PropertyInfo? property) where T : Attribute
        {
            if (property == null)
                return false;

            return property.GetCustomAttribute<T>() != null;
        }

        /// <summary>
        /// 判断属性是否有指定特性
        /// </summary>
        public static bool HasAttribute(this PropertyInfo? property, Type? attributeType)
        {
            if (property == null || attributeType == null)
                return false;

            return property.GetCustomAttributes(attributeType, false).Any();
        }

        /// <summary>
        /// 获取属性特性
        /// </summary>
        public static T? GetAttribute<T>(this PropertyInfo? property) where T : Attribute
        {
            if (property == null)
                return null;

            return property.GetCustomAttribute<T>();
        }

        /// <summary>
        /// 获取属性的所有特性
        /// </summary>
        public static T[] GetAttributes<T>(this PropertyInfo? property) where T : Attribute
        {
            if (property == null)
                return Array.Empty<T>();

            return property.GetCustomAttributes<T>().ToArray();
        }

        #endregion

        #region DataAnnotations 特性快捷访问

        /// <summary>
        /// 判断是否是必填项
        /// </summary>
        public static bool IsRequired(this PropertyInfo? property)
        {
            return property.HasAttribute<RequiredAttribute>();
        }

        /// <summary>
        /// 获取显示名称
        /// </summary>
        public static string GetDisplayName(this PropertyInfo? property)
        {
            var displayAttr = property.GetAttribute<System.ComponentModel.DataAnnotations.DisplayAttribute>();
            if (displayAttr != null && !string.IsNullOrEmpty(displayAttr.GetName()))
                return displayAttr.GetName()!;

            var displayNameAttr = property.GetAttribute<System.ComponentModel.DisplayNameAttribute>();
            if (displayNameAttr != null && !string.IsNullOrEmpty(displayNameAttr.DisplayName))
                return displayNameAttr.DisplayName;

            return property?.Name ?? string.Empty;
        }

        /// <summary>
        /// 获取描述
        /// </summary>
        public static string GetDescription(this PropertyInfo? property)
        {
            var descriptionAttr = property.GetAttribute<System.ComponentModel.DescriptionAttribute>();
            if (descriptionAttr != null && !string.IsNullOrEmpty(descriptionAttr.Description))
                return descriptionAttr.Description;

            var displayAttr = property.GetAttribute<System.ComponentModel.DataAnnotations.DisplayAttribute>();
            if (displayAttr != null && !string.IsNullOrEmpty(displayAttr.GetDescription()))
                return displayAttr.GetDescription()!;

            return string.Empty;
        }

        /// <summary>
        /// 获取字符串长度限制
        /// </summary>
        public static int GetStringLength(this PropertyInfo? property)
        {
            var attr = property.GetAttribute<StringLengthAttribute>();
            return attr?.MaximumLength ?? 0;
        }

        /// <summary>
        /// 获取数据类型
        /// </summary>
        public static string GetDataType(this PropertyInfo? property)
        {
            var attr = property.GetAttribute<System.ComponentModel.DataAnnotations.DataTypeAttribute>();
            return attr?.DataType.ToString() ?? string.Empty;
        }

        #endregion

        #region 类型判断

        /// <summary>
        /// 判断是否是字符串类型
        /// </summary>
        public static bool IsString(this PropertyInfo? property)
        {
            return property?.PropertyType == typeof(string);
        }

        /// <summary>
        /// 判断是否是数值类型
        /// </summary>
        public static bool IsNumeric(this PropertyInfo? property)
        {
            var type = Nullable.GetUnderlyingType(property?.PropertyType) ?? property?.PropertyType;
            if (type == null)
                return false;

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
        /// 判断是否是日期类型
        /// </summary>
        public static bool IsDateTime(this PropertyInfo? property)
        {
            var type = Nullable.GetUnderlyingType(property?.PropertyType) ?? property?.PropertyType;
            if (type == null)
                return false;

            return type == typeof(DateTime) || type == typeof(DateTimeOffset);
        }

        /// <summary>
        /// 判断是否是布尔类型
        /// </summary>
        public static bool IsBoolean(this PropertyInfo? property)
        {
            var type = Nullable.GetUnderlyingType(property?.PropertyType) ?? property?.PropertyType;
            if (type == null)
                return false;

            return type == typeof(bool);
        }

        /// <summary>
        /// 判断是否是枚举类型
        /// </summary>
        public static bool IsEnum(this PropertyInfo? property)
        {
            var type = Nullable.GetUnderlyingType(property?.PropertyType) ?? property?.PropertyType;
            return type?.IsEnum == true;
        }

        /// <summary>
        /// 判断是否是集合类型
        /// </summary>
        public static bool IsCollection(this PropertyInfo? property)
        {
            if (property == null)
                return false;

            return typeof(System.Collections.IEnumerable).IsAssignableFrom(property.PropertyType) &&
                   property.PropertyType != typeof(string);
        }

        /// <summary>
        /// 判断是否是可空类型
        /// </summary>
        public static bool IsNullable(this PropertyInfo? property)
        {
            if (property == null)
                return false;

            return Nullable.GetUnderlyingType(property.PropertyType) != null;
        }

        #endregion

        #region 访问判断

        /// <summary>
        /// 判断是否可读
        /// </summary>
        public static bool CanRead(this PropertyInfo? property)
        {
            return property?.CanRead == true;
        }

        /// <summary>
        /// 判断是否可写
        /// </summary>
        public static bool CanWrite(this PropertyInfo? property)
        {
            return property?.CanWrite == true;
        }

        /// <summary>
        /// 判断是否有公共的 getter
        /// </summary>
        public static bool HasPublicGetter(this PropertyInfo? property)
        {
            return property?.CanRead == true && property.GetGetMethod(false) != null;
        }

        /// <summary>
        /// 判断是否有公共的 setter
        /// </summary>
        public static bool HasPublicSetter(this PropertyInfo? property)
        {
            return property?.CanWrite == true && property.GetSetMethod(false) != null;
        }

        #endregion

        #region 获取元素类型

        /// <summary>
        /// 获取集合的元素类型
        /// </summary>
        public static Type? GetElementType(this PropertyInfo? property)
        {
            if (property == null)
                return null;

            var type = property.PropertyType;

            // 处理数组
            if (type.IsArray)
                return type.GetElementType();

            // 处理泛型集合
            if (type.IsGenericType)
            {
                var genericType = type.GetGenericTypeDefinition();
                if (genericType == typeof(System.Collections.Generic.IEnumerable<>) ||
                    genericType == typeof(System.Collections.Generic.List<>) ||
                    genericType == typeof(System.Collections.Generic.IList<>) ||
                    genericType == typeof(System.Collections.Generic.ICollection<>))
                {
                    return type.GetGenericArguments()[0];
                }
            }

            return null;
        }

        #endregion
    }
}
