using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EasyTool.ToolCategory
{
    /// <summary>
    /// Bean 属性操作工具类
    /// 对标 Hutool 的 BeanUtil
    /// 提供 Bean 属性复制、转换、访问等功能
    /// </summary>
    public static class BeanUtil
    {
        #region 属性复制

        /// <summary>
        /// 复制源对象的属性到目标类型的新实例
        /// </summary>
        /// <typeparam name="TSource">源类型</typeparam>
        /// <typeparam name="TTarget">目标类型</typeparam>
        /// <param name="source">源对象</param>
        /// <param name="ignoreNull">是否忽略 null 值</param>
        /// <returns>目标对象</returns>
        public static TTarget? CopyProperties<TSource, TTarget>(TSource source, bool ignoreNull = false)
            where TTarget : class, new()
        {
            if (source == null)
                return null;

            var target = new TTarget();
            CopyProperties(source, target, ignoreNull);
            return target;
        }

        /// <summary>
        /// 复制源对象的属性到目标对象
        /// </summary>
        /// <typeparam name="TSource">源类型</typeparam>
        /// <typeparam name="TTarget">目标类型</typeparam>
        /// <param name="source">源对象</param>
        /// <param name="target">目标对象</param>
        /// <param name="ignoreNull">是否忽略 null 值</param>
        /// <param name="ignoreProperties">要忽略的属性名</param>
        public static void CopyProperties<TSource, TTarget>(
            TSource source,
            TTarget target,
            bool ignoreNull = false,
            params string[] ignoreProperties)
            where TTarget : class
        {
            if (source == null || target == null)
                return;

            var sourceType = source.GetType();
            var targetType = target.GetType();
            var sourceProps = sourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var targetProps = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite)
                .ToDictionary(p => p.Name, p => p);

            var ignoreSet = new HashSet<string>(ignoreProperties, StringComparer.OrdinalIgnoreCase);

            foreach (var sourceProp in sourceProps)
            {
                if (!sourceProp.CanRead)
                    continue;

                if (ignoreSet.Contains(sourceProp.Name))
                    continue;

                if (!targetProps.TryGetValue(sourceProp.Name, out var targetProp))
                    continue;

                var sourceValue = sourceProp.GetValue(source);

                if (ignoreNull && sourceValue == null)
                    continue;

                try
                {
                    var convertedValue = ConvertValue(sourceValue, targetProp.PropertyType);
                    targetProp.SetValue(target, convertedValue);
                }
                catch
                {
                    // 忽略转换失败的属性
                }
            }
        }

        /// <summary>
        /// 批量复制列表中的对象属性
        /// </summary>
        /// <typeparam name="TSource">源类型</typeparam>
        /// <typeparam name="TTarget">目标类型</typeparam>
        /// <param name="sources">源对象列表</param>
        /// <param name="ignoreNull">是否忽略 null 值</param>
        /// <returns>目标对象列表</returns>
        public static List<TTarget> CopyToList<TSource, TTarget>(IEnumerable<TSource> sources, bool ignoreNull = false)
            where TTarget : class, new()
        {
            if (sources == null)
                return new List<TTarget>();

            return sources.Select(s => CopyProperties<TSource, TTarget>(s, ignoreNull))
                .Where(t => t != null)
                .Cast<TTarget>()
                .ToList();
        }

        #endregion

        #region Bean 与 Map 互转

        /// <summary>
        /// 将 Bean 对象转换为字典
        /// </summary>
        /// <param name="bean">Bean 对象</param>
        /// <param name="ignoreNull">是否忽略 null 值</param>
        /// <returns>属性字典</returns>
        public static Dictionary<string, object?> BeanToMap(object? bean, bool ignoreNull = false)
        {
            if (bean == null)
                return new Dictionary<string, object?>();

            var type = bean.GetType();
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var result = new Dictionary<string, object?>();

            foreach (var prop in properties)
            {
                if (!prop.CanRead)
                    continue;

                var value = prop.GetValue(bean);

                if (ignoreNull && value == null)
                    continue;

                result[prop.Name] = value;
            }

            return result;
        }

        /// <summary>
        /// 将 Bean 对象转换为字典（指定属性）
        /// </summary>
        /// <param name="bean">Bean 对象</param>
        /// <param name="propertyNames">要包含的属性名</param>
        /// <returns>属性字典</returns>
        public static Dictionary<string, object?> BeanToMap(object? bean, params string[] propertyNames)
        {
            if (bean == null)
                return new Dictionary<string, object?>();

            var type = bean.GetType();
            var result = new Dictionary<string, object?>();
            var propSet = new HashSet<string>(propertyNames, StringComparer.OrdinalIgnoreCase);

            foreach (var propName in propertyNames)
            {
                var prop = type.GetProperty(propName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (prop?.CanRead == true)
                {
                    result[prop.Name] = prop.GetValue(bean);
                }
            }

            return result;
        }

        /// <summary>
        /// 将字典转换为 Bean 对象
        /// </summary>
        /// <typeparam name="T">Bean 类型</typeparam>
        /// <param name="map">属性字典</param>
        /// <param name="ignoreCase">是否忽略属性名大小写</param>
        /// <returns>Bean 对象</returns>
        public static T? ToBean<T>(IDictionary<string, object?>? map, bool ignoreCase = true) where T : class, new()
        {
            if (map == null || map.Count == 0)
                return null;

            var type = typeof(T);
            var obj = new T();
            var bindingFlags = BindingFlags.Public | BindingFlags.Instance;

            if (ignoreCase)
                bindingFlags |= BindingFlags.IgnoreCase;

            foreach (var kvp in map)
            {
                var prop = type.GetProperty(kvp.Key, bindingFlags);
                if (prop?.CanWrite == true)
                {
                    var value = ConvertValue(kvp.Value, prop.PropertyType);
                    prop.SetValue(obj, value);
                }
            }

            return obj;
        }

        #endregion

        #region 属性访问

        /// <summary>
        /// 获取 Bean 的属性值
        /// </summary>
        /// <param name="bean">Bean 对象</param>
        /// <param name="propertyName">属性名（支持嵌套，如 "User.Address.City"）</param>
        /// <returns>属性值</returns>
        public static object? GetPropertyValue(object? bean, string propertyName)
        {
            if (bean == null || string.IsNullOrEmpty(propertyName))
                return null;

            var parts = propertyName.Split('.');
            var current = bean;

            foreach (var part in parts)
            {
                if (current == null)
                    return null;

                var type = current.GetType();
                var prop = type.GetProperty(part, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

                if (prop == null || !prop.CanRead)
                    return null;

                current = prop.GetValue(current);
            }

            return current;
        }

        /// <summary>
        /// 获取 Bean 的属性值（泛型版本）
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="bean">Bean 对象</param>
        /// <param name="propertyName">属性名</param>
        /// <returns>属性值</returns>
        public static T? GetPropertyValue<T>(object? bean, string propertyName)
        {
            var value = GetPropertyValue(bean, propertyName);
            if (value == null)
                return default;

            if (value is T t)
                return t;

            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return default;
            }
        }

        /// <summary>
        /// 设置 Bean 的属性值
        /// </summary>
        /// <param name="bean">Bean 对象</param>
        /// <param name="propertyName">属性名</param>
        /// <param name="value">属性值</param>
        /// <returns>是否设置成功</returns>
        public static bool SetPropertyValue(object? bean, string propertyName, object? value)
        {
            if (bean == null || string.IsNullOrEmpty(propertyName))
                return false;

            var type = bean.GetType();
            var prop = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            if (prop?.CanWrite != true)
                return false;

            try
            {
                var convertedValue = ConvertValue(value, prop.PropertyType);
                prop.SetValue(bean, convertedValue);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Bean 信息

        /// <summary>
        /// 获取 Bean 的所有属性名
        /// </summary>
        /// <param name="bean">Bean 对象</param>
        /// <returns>属性名数组</returns>
        public static string[] GetPropertyNames(object? bean)
        {
            if (bean == null)
                return Array.Empty<string>();

            var type = bean.GetType();
            return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(p => p.Name)
                .ToArray();
        }

        /// <summary>
        /// 获取 Bean 的所有属性值
        /// </summary>
        /// <param name="bean">Bean 对象</param>
        /// <returns>属性值字典</returns>
        public static Dictionary<string, object?> GetPropertyValues(object? bean)
        {
            return BeanToMap(bean);
        }

        /// <summary>
        /// 检查对象是否是有效的 Bean（有可读写的属性）
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>是否是 Bean</returns>
        public static bool IsBean(Type type)
        {
            if (type == null)
                return false;

            if (type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(decimal))
                return false;

            if (type == typeof(DateTime) || type == typeof(Guid) || type == typeof(TimeSpan))
                return false;

            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            return props.Any(p => p.CanRead && p.CanWrite);
        }

        /// <summary>
        /// 检查类型是否有指定的属性
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="propertyName">属性名</param>
        /// <returns>是否有属性</returns>
        public static bool HasProperty(Type type, string propertyName)
        {
            if (type == null || string.IsNullOrEmpty(propertyName))
                return false;

            return type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase) != null;
        }

        /// <summary>
        /// 检查类型是否有 Getter
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="propertyName">属性名</param>
        /// <returns>是否有 Getter</returns>
        public static bool HasGetter(Type type, string propertyName)
        {
            if (type == null || string.IsNullOrEmpty(propertyName))
                return false;

            var prop = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            return prop?.CanRead == true;
        }

        /// <summary>
        /// 检查类型是否有 Setter
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="propertyName">属性名</param>
        /// <returns>是否有 Setter</returns>
        public static bool HasSetter(Type type, string propertyName)
        {
            if (type == null || string.IsNullOrEmpty(propertyName))
                return false;

            var prop = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            return prop?.CanWrite == true;
        }

        #endregion

        #region 辅助方法

        private static object? ConvertValue(object? value, Type targetType)
        {
            if (value == null)
                return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;

            if (targetType.IsAssignableFrom(value.GetType()))
                return value;

            var underlyingType = Nullable.GetUnderlyingType(targetType);
            if (underlyingType != null)
            {
                if (value == null)
                    return null;
                targetType = underlyingType;
            }

            try
            {
                if (targetType == typeof(Guid) && value is string guidStr)
                    return Guid.Parse(guidStr);

                if (targetType == typeof(DateTime) && value is string dateStr)
                    return DateTime.Parse(dateStr);

                if (targetType == typeof(TimeSpan) && value is string timeStr)
                    return TimeSpan.Parse(timeStr);

                return Convert.ChangeType(value, targetType);
            }
            catch
            {
                return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
            }
        }

        #endregion
    }
}