using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace EasyTool.ToolCategory
{
    /// <summary>
    /// Enum 枚举扩展方法
    /// </summary>
    public static class EnumExtension
    {
        #region 描述信息

        /// <summary>
        /// 获取枚举值的描述（Description 特性）
        /// </summary>
        public static string GetDescription(this Enum value)
        {
            if (value == null)
                return string.Empty;

            var field = value.GetType().GetField(value.ToString());
            if (field == null)
                return value.ToString();

            var attr = field.GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault() as DescriptionAttribute;
            return attr?.Description ?? value.ToString();
        }

        /// <summary>
        /// 获取枚举值的显示名称（Display 特性）
        /// </summary>
        public static string GetDisplayName(this Enum value)
        {
            if (value == null)
                return string.Empty;

            var field = value.GetType().GetField(value.ToString());
            if (field == null)
                return value.ToString();

            var attr = field.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.DisplayAttribute), false).FirstOrDefault() as System.ComponentModel.DataAnnotations.DisplayAttribute;
            return attr?.GetName() ?? value.ToString();
        }

        #endregion

        #region 枚举转换


        /// <summary>
        /// 将整数转换为枚举
        /// </summary>
        public static T ToEnum<T>(this int value) where T : struct, Enum
        {
            return Enum.Parse<T>(value.ToString());
        }

        /// <summary>
        /// 安全解析字符串为枚举
        /// </summary>
        public static T ParseEnum<T>(this string value) where T : struct, Enum
        {
            return Enum.Parse<T>(value, true);
        }

        /// <summary>
        /// 安全解析字符串为枚举，失败返回默认值
        /// </summary>
        public static T ParseEnumOrDefault<T>(this string value, T defaultValue = default) where T : struct, Enum
        {
            if (Enum.TryParse<T>(value, true, out var result))
                return result;

            return defaultValue;
        }


        #endregion

        #region 枚举字典扩展

        /// <summary>
        /// 获取枚举类型的所有成员的名称和值的键值对
        /// </summary>
        public static IDictionary<string, T> ToNameDictionary<T>() where T : struct, Enum
        {
            var valuesDictionary = new Dictionary<string, T>();
            foreach (T value in GetValues<T>())
            {
                valuesDictionary.Add(Enum.GetName(typeof(T), value)!, value);
            }
            return valuesDictionary;
        }

        /// <summary>
        /// 获取指定枚举值的描述
        /// </summary>
        public static string? GetDescriptionByValue<T>(T value) where T : struct, Enum
        {
            var name = Enum.GetName(typeof(T), value);
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }
            var field = typeof(T).GetField(name);
            var attr = field?.GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault() as DescriptionAttribute;
            return attr?.Description;
        }

        /// <summary>
        /// 获取指定枚举值的显示名称
        /// </summary>
        public static string? GetDisplayNameByValue<T>(T value) where T : struct, Enum
        {
            var name = Enum.GetName(typeof(T), value);
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }
            var field = typeof(T).GetField(name);
            var attr = field?.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.DisplayAttribute), false).FirstOrDefault() as System.ComponentModel.DataAnnotations.DisplayAttribute;
            return attr?.GetName();
        }

        #endregion

        #region 枚举判断

        /// <summary>
        /// 获取枚举类型的所有值
        /// </summary>
        public static T[] GetValues<T>() where T : struct, Enum
        {
            return (T[])Enum.GetValues(typeof(T));
        }

        /// <summary>
        /// 获取枚举类型的所有名称
        /// </summary>
        public static string[] GetNames<T>() where T : struct, Enum
        {
            return Enum.GetNames(typeof(T));
        }

        /// <summary>
        /// 获取枚举类型的所有值和描述
        /// </summary>
        public static Dictionary<T, string> ToDictionary<T>() where T : struct, Enum
        {
            var dictionary = new Dictionary<T, string>();
            foreach (T value in GetValues<T>())
            {
                dictionary[value] = value.GetDescription();
            }
            return dictionary;
        }

        /// <summary>
        /// 获取枚举类型的所有值和显示名称
        /// </summary>
        public static Dictionary<T, string> ToDisplayNameDictionary<T>() where T : struct, Enum
        {
            var dictionary = new Dictionary<T, string>();
            foreach (T value in GetValues<T>())
            {
                dictionary[value] = value.GetDisplayName();
            }
            return dictionary;
        }

        #endregion

        #region 枚举判断


        /// <summary>
        /// 判断字符串是否是有效的枚举名称或值
        /// </summary>
        public static bool IsEnumDefined<T>(this string value) where T : struct, Enum
        {
            return Enum.IsDefined(typeof(T), value);
        }

        /// <summary>
        /// 判断枚举值是否有指定标记
        /// </summary>
        public static bool HasFlag<T>(this T value, T flag) where T : struct, Enum
        {
            var valueInt = Convert.ToInt64(value);
            var flagInt = Convert.ToInt64(flag);
            return (valueInt & flagInt) == flagInt;
        }

        /// <summary>
        /// 设置枚举标记
        /// </summary>
        public static T SetFlag<T>(this T value, T flag) where T : struct, Enum
        {
            var valueInt = Convert.ToInt64(value);
            var flagInt = Convert.ToInt64(flag);
            var result = valueInt | flagInt;
            return (T)Enum.ToObject(typeof(T), result);
        }

        /// <summary>
        /// 清除枚举标记
        /// </summary>
        public static T ClearFlag<T>(this T value, T flag) where T : struct, Enum
        {
            var valueInt = Convert.ToInt64(value);
            var flagInt = Convert.ToInt64(flag);
            var result = valueInt & ~flagInt;
            return (T)Enum.ToObject(typeof(T), result);
        }

        /// <summary>
        /// 切换枚举标记
        /// </summary>
        public static T ToggleFlag<T>(this T value, T flag) where T : struct, Enum
        {
            var valueInt = Convert.ToInt64(value);
            var flagInt = Convert.ToInt64(flag);
            var result = valueInt ^ flagInt;
            return (T)Enum.ToObject(typeof(T), result);
        }

        #endregion

        #region 下一个/上一个值

        /// <summary>
        /// 获取下一个枚举值
        /// </summary>
        public static T Next<T>(this T value) where T : struct, Enum
        {
            var values = GetValues<T>();
            int index = Array.IndexOf(values, value);
            if (index < 0 || index >= values.Length - 1)
                return value;

            return values[index + 1];
        }

        /// <summary>
        /// 获取上一个枚举值
        /// </summary>
        public static T Previous<T>(this T value) where T : struct, Enum
        {
            var values = GetValues<T>();
            int index = Array.IndexOf(values, value);
            if (index <= 0)
                return value;

            return values[index - 1];
        }

        /// <summary>
        /// 获取第一个枚举值
        /// </summary>
        public static T First<T>() where T : struct, Enum
        {
            var values = GetValues<T>();
            return values[0];
        }

        /// <summary>
        /// 获取最后一个枚举值
        /// </summary>
        public static T Last<T>() where T : struct, Enum
        {
            var values = GetValues<T>();
            return values[values.Length - 1];
        }

        #endregion

        #region 随机值

        /// <summary>
        /// 获取随机枚举值
        /// </summary>
        public static T Random<T>() where T : struct, Enum
        {
            var values = GetValues<T>();
            var random = new System.Random();
            int index = random.Next(values.Length);
            return values[index];
        }

        #endregion

        #region Flags 操作

        /// <summary>
        /// 获取 Flags 枚举的所有设置值
        /// </summary>
        public static IEnumerable<T> GetFlags<T>(this T value) where T : struct, Enum
        {
            var values = GetValues<T>();
            var valueInt = Convert.ToInt64(value);

            foreach (T flag in values)
            {
                var flagInt = Convert.ToInt64(flag);
                if (flagInt == 0)
                    continue;

                if ((valueInt & flagInt) == flagInt)
                    yield return flag;
            }
        }

        /// <summary>
        /// 判断是否是 Flags 枚举
        /// </summary>
        public static bool IsFlagsEnum<T>() where T : struct, Enum
        {
            return typeof(T).IsDefined(typeof(FlagsAttribute), false);
        }

        #endregion

        #region 下拉框/选择列表

        /// <summary>
        /// 获取枚举的所有选项（用于下拉框等）
        /// </summary>
        public static List<EnumItem<T>> GetItems<T>() where T : struct, Enum
        {
            var items = new List<EnumItem<T>>();
            foreach (T value in GetValues<T>())
            {
                items.Add(new EnumItem<T>
                {
                    Value = value,
                    Name = value.ToString(),
                    Description = value.GetDescription(),
                    DisplayName = value.GetDisplayName()
                });
            }
            return items;
        }

        #endregion
    }

    /// <summary>
    /// 枚举项
    /// </summary>
    public class EnumItem<T> where T : struct, Enum
    {
        /// <summary>
        /// 枚举值
        /// </summary>
        public T Value { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 显示名称
        /// </summary>
        public string? DisplayName { get; set; }
    }
}
