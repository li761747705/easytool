using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace EasyTool.ToolCategory
{
    /// <summary>
    /// Record 记录类型工具类
    /// 提供 Record 类型（C# 9.0+）的克隆、比较、with 表达式等操作
    /// Record 是不可变的引用类型，支持基于值的相等性
    /// </summary>
    public static class RecordUtil
    {
        #region Record 克隆

        /// <summary>
        /// 使用 with 表达式克隆 Record（修改部分属性）
        /// </summary>
        /// <typeparam name="T">Record 类型</typeparam>
        /// <param name="record">原 Record</param>
        /// <param name="propertyName">要修改的属性名</param>
        /// <param name="newValue">新值</param>
        /// <returns>克隆后的 Record</returns>
        public static T With<T>(T record, string propertyName, object? newValue) where T : class
        {
            if (record == null)
                throw new ArgumentNullException(nameof(record));

            var type = record.GetType();
            var property = type.GetProperty(propertyName);

            if (property == null)
                throw new ArgumentException($"Property '{propertyName}' not found on type {type.Name}");

            // 使用反射创建克隆
            var cloneMethod = type.GetMethod("<Clone>$");
            if (cloneMethod != null)
            {
                var clone = cloneMethod.Invoke(record, null);
                if (clone != null)
                {
                    property.SetValue(clone, newValue);
                    return (T)clone;
                }
            }

            // 如果没有 <Clone>$ 方法，使用构造函数
            var constructor = type.GetConstructors().FirstOrDefault();
            if (constructor == null)
                throw new InvalidOperationException($"No constructor found for type {type.Name}");

            var parameters = constructor.GetParameters();
            var args = new object?[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                var param = parameters[i];
                var prop = type.GetProperty(param.Name ?? "");

                if (prop != null)
                {
                    if (prop.Name == propertyName)
                        args[i] = newValue;
                    else
                        args[i] = prop.GetValue(record);
                }
                else
                {
                    args[i] = null;
                }
            }

            return (T)constructor.Invoke(args);
        }

        /// <summary>
        /// 使用表达式克隆 Record（修改部分属性）
        /// </summary>
        /// <typeparam name="T">Record 类型</typeparam>
        /// <typeparam name="TValue">属性值类型</typeparam>
        /// <param name="record">原 Record</param>
        /// <param name="propertyExpression">属性表达式</param>
        /// <param name="newValue">新值</param>
        /// <returns>克隆后的 Record</returns>
        public static T With<T, TValue>(T record, Expression<Func<T, TValue>> propertyExpression, TValue newValue) where T : class
        {
            if (propertyExpression.Body is MemberExpression memberExpr)
            {
                var propertyName = memberExpr.Member.Name;
                return With(record, propertyName, newValue);
            }

            throw new ArgumentException("Invalid property expression");
        }

        /// <summary>
        /// 克隆 Record（不修改任何属性）
        /// </summary>
        /// <typeparam name="T">Record 类型</typeparam>
        /// <param name="record">原 Record</param>
        /// <returns>克隆后的 Record</returns>
        public static T Clone<T>(T record) where T : class
        {
            if (record == null)
                throw new ArgumentNullException(nameof(record));

            var type = record.GetType();
            var cloneMethod = type.GetMethod("<Clone>$");

            if (cloneMethod != null)
            {
                return (T)cloneMethod.Invoke(record, null)!;
            }

            // 如果没有 <Clone>$ 方法，使用构造函数
            var constructor = type.GetConstructors().FirstOrDefault();
            if (constructor == null)
                throw new InvalidOperationException($"No constructor found for type {type.Name}");

            var parameters = constructor.GetParameters();
            var args = new object?[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                var param = parameters[i];
                var prop = type.GetProperty(param.Name ?? "");
                args[i] = prop?.GetValue(record);
            }

            return (T)constructor.Invoke(args);
        }

        #endregion

        #region Record 比较

        /// <summary>
        /// 比较两个 Record 是否相等（基于值）
        /// </summary>
        /// <typeparam name="T">Record 类型</typeparam>
        /// <param name="first">第一个 Record</param>
        /// <param name="second">第二个 Record</param>
        /// <returns>是否相等</returns>
        public static bool Equals<T>(T? first, T? second) where T : class
        {
            if (first == null && second == null)
                return true;
            if (first == null || second == null)
                return false;

            return first.Equals(second);
        }

        /// <summary>
        /// 比较 Record 是否与另一个对象相等
        /// </summary>
        /// <typeparam name="T">Record 类型</typeparam>
        /// <param name="record">Record</param>
        /// <param name="obj">对象</param>
        /// <returns>是否相等</returns>
        public static bool Equals<T>(T record, object? obj) where T : class
        {
            if (record == null)
                return obj == null;

            return record.Equals(obj);
        }

        /// <summary>
        /// 获取 Record 的哈希码
        /// </summary>
        /// <typeparam name="T">Record 类型</typeparam>
        /// <param name="record">Record</param>
        /// <returns>哈希码</returns>
        public static int GetHashCode<T>(T record) where T : class
        {
            return record?.GetHashCode() ?? 0;
        }

        #endregion

        #region Record 信息获取

        /// <summary>
        /// 获取 Record 的所有属性名
        /// </summary>
        /// <typeparam name="T">Record 类型</typeparam>
        /// <param name="record">Record</param>
        /// <returns>属性名列表</returns>
        public static List<string> GetPropertyNames<T>(T record) where T : class
        {
            if (record == null)
                return new List<string>();

            var type = record.GetType();
            return type.GetProperties().Select(p => p.Name).ToList();
        }

        /// <summary>
        /// 获取 Record 的所有属性值
        /// </summary>
        /// <typeparam name="T">Record 类型</typeparam>
        /// <param name="record">Record</param>
        /// <returns>属性值字典</returns>
        public static Dictionary<string, object?> GetPropertyValues<T>(T record) where T : class
        {
            if (record == null)
                return new Dictionary<string, object?>();

            var type = record.GetType();
            var dict = new Dictionary<string, object?>();

            foreach (var prop in type.GetProperties())
            {
                dict[prop.Name] = prop.GetValue(record);
            }

            return dict;
        }

        /// <summary>
        /// 获取 Record 的属性值
        /// </summary>
        /// <typeparam name="T">Record 类型</typeparam>
        /// <typeparam name="TValue">属性值类型</typeparam>
        /// <param name="record">Record</param>
        /// <param name="propertyName">属性名</param>
        /// <returns>属性值</returns>
        public static TValue? GetProperty<T, TValue>(T record, string propertyName) where T : class
        {
            if (record == null)
                throw new ArgumentNullException(nameof(record));

            var type = record.GetType();
            var property = type.GetProperty(propertyName);

            if (property == null)
                throw new ArgumentException($"Property '{propertyName}' not found on type {type.Name}");

            return (TValue?)property.GetValue(record);
        }

        /// <summary>
        /// 获取 Record 的类型名
        /// </summary>
        /// <typeparam name="T">Record 类型</typeparam>
        /// <param name="record">Record</param>
        /// <returns>类型名</returns>
        public static string GetTypeName<T>(T record) where T : class
        {
            return record?.GetType().Name ?? "null";
        }

        #endregion

        #region Record 打印

        /// <summary>
        /// 获取 Record 的字符串表示（自动使用 PrintMembers）
        /// </summary>
        /// <typeparam name="T">Record 类型</typeparam>
        /// <param name="record">Record</param>
        /// <returns>字符串表示</returns>
        public static string ToString<T>(T record) where T : class
        {
            return record?.ToString() ?? "null";
        }

        /// <summary>
        /// 格式化输出 Record 的所有属性
        /// </summary>
        /// <typeparam name="T">Record 类型</typeparam>
        /// <param name="record">Record</param>
        /// <returns>格式化字符串</returns>
        public static string Format<T>(T record) where T : class
        {
            if (record == null)
                return "null";

            var type = record.GetType();
            var sb = new System.Text.StringBuilder();
            sb.Append($"{type.Name} {{ ");

            var properties = type.GetProperties();
            for (int i = 0; i < properties.Length; i++)
            {
                var prop = properties[i];
                var value = prop.GetValue(record);
                sb.Append($"{prop.Name} = {value}");

                if (i < properties.Length - 1)
                    sb.Append(", ");
            }

            sb.Append(" }");
            return sb.ToString();
        }

        #endregion

        #region Record 类型判断

        /// <summary>
        /// 判断类型是否为 Record
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <returns>是否为 Record</returns>
        public static bool IsRecord<T>()
        {
            var type = typeof(T);
            return IsRecord(type);
        }

        /// <summary>
        /// 判断类型是否为 Record
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>是否为 Record</returns>
        public static bool IsRecord(Type type)
        {
            // Record 类型特征：
            // 1. 有 <Clone>$ 方法
            // 2. 有 PrintMembers 方法
            // 3. 有 EqualityContract 属性（如果是 record class）
            // 4. 继承自 System.Object 或其他 record

            var cloneMethod = type.GetMethod("<Clone>$", BindingFlags.NonPublic | BindingFlags.Instance);
            var printMembers = type.GetMethod("PrintMembers", BindingFlags.NonPublic | BindingFlags.Instance);

            return cloneMethod != null || printMembers != null;
        }

        /// <summary>
        /// 判断对象是否为 Record
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="record">对象</param>
        /// <returns>是否为 Record</returns>
        public static bool IsRecord<T>(T record) where T : class
        {
            if (record == null)
                return false;

            return IsRecord(record.GetType());
        }

        #endregion

        #region Record 转换

        /// <summary>
        /// Record 转字典
        /// </summary>
        /// <typeparam name="T">Record 类型</typeparam>
        /// <param name="record">Record</param>
        /// <returns>字典</returns>
        public static Dictionary<string, object?> ToDictionary<T>(T record) where T : class
        {
            return GetPropertyValues(record);
        }

        /// <summary>
        /// Record 列表转字典列表
        /// </summary>
        /// <typeparam name="T">Record 类型</typeparam>
        /// <param name="records">Record 列表</param>
        /// <returns>字典列表</returns>
        public static List<Dictionary<string, object?>> ToDictionaries<T>(IEnumerable<T> records) where T : class
        {
            return records.Select(ToDictionary).ToList();
        }

        #endregion

        #region Record 验证

        /// <summary>
        /// 验证 Record 的属性是否都非空
        /// </summary>
        /// <typeparam name="T">Record 类型</typeparam>
        /// <param name="record">Record</param>
        /// <returns>是否都非空</returns>
        public static bool AllPropertiesNotNull<T>(T record) where T : class
        {
            if (record == null)
                return false;

            var type = record.GetType();
            foreach (var prop in type.GetProperties())
            {
                if (prop.GetValue(record) == null)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 验证 Record 是否有任意空属性
        /// </summary>
        /// <typeparam name="T">Record 类型</typeparam>
        /// <param name="record">Record</param>
        /// <returns>是否有空属性</returns>
        public static bool HasAnyNullProperty<T>(T record) where T : class
        {
            return !AllPropertiesNotNull(record);
        }

        /// <summary>
        /// 获取 Record 的空属性名列表
        /// </summary>
        /// <typeparam name="T">Record 类型</typeparam>
        /// <param name="record">Record</param>
        /// <returns>空属性名列表</returns>
        public static List<string> GetNullPropertyNames<T>(T record) where T : class
        {
            if (record == null)
                return new List<string>();

            var type = record.GetType();
            var nullProps = new List<string>();

            foreach (var prop in type.GetProperties())
            {
                if (prop.GetValue(record) == null)
                    nullProps.Add(prop.Name);
            }

            return nullProps;
        }

        #endregion
    }
}