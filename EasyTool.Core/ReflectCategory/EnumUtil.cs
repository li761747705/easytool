using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace EasyTool.ReflectCategory
{
    /// <summary>
    /// 枚举工具类
    /// </summary>
    public static class EnumUtil
    {
        #region Description 属性相关

        /// <summary>
        /// 获取枚举值的 Description 属性描述
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <param name="value">枚举值</param>
        /// <returns>Description 描述，如果没有则返回枚举名称</returns>
        public static string GetDescription<T>(T value) where T : struct, Enum
        {
            var field = typeof(T).GetField(value.ToString());
            if (field == null) return value.ToString();

            var attr = field.GetCustomAttribute<DescriptionAttribute>();
            return attr?.Description ?? value.ToString();
        }

        /// <summary>
        /// 获取所有枚举值的描述字典
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <returns>枚举值与描述的字典</returns>
        public static Dictionary<T, string> GetAllDescriptions<T>() where T : struct, Enum
        {
            var result = new Dictionary<T, string>();
            foreach (var value in GetValues<T>())
            {
                result[value] = GetDescription(value);
            }
            return result;
        }

        /// <summary>
        /// 根据描述查找枚举值
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <param name="description">描述文本</param>
        /// <param name="ignoreCase">是否忽略大小写</param>
        /// <returns>匹配的枚举值，未找到则返回 null</returns>
        public static T? FromDescription<T>(string description, bool ignoreCase = true) where T : struct, Enum
        {
            if (string.IsNullOrEmpty(description)) return null;

            foreach (var value in GetValues<T>())
            {
                var desc = GetDescription(value);
                if (ignoreCase)
                {
                    if (string.Equals(desc, description, StringComparison.OrdinalIgnoreCase))
                        return value;
                }
                else
                {
                    if (desc == description)
                        return value;
                }
            }
            return null;
        }

        #endregion

        #region Display 属性相关

        /// <summary>
        /// 获取枚举值的 Display 属性名称
        /// 优先返回 Display(Name=)，如果没有则返回 Description，都没有则返回枚举名称
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <param name="value">枚举值</param>
        /// <returns>显示名称</returns>
        public static string GetDisplayName<T>(T value) where T : struct, Enum
        {
            var field = typeof(T).GetField(value.ToString());
            if (field == null) return value.ToString();

            // 优先使用 Display 属性
            var displayAttr = field.GetCustomAttribute<DisplayAttribute>();
            if (displayAttr != null && !string.IsNullOrEmpty(displayAttr.Name))
            {
                return displayAttr.Name;
            }

            // 其次使用 Description 属性
            var descAttr = field.GetCustomAttribute<DescriptionAttribute>();
            if (descAttr != null)
            {
                return descAttr.Description;
            }

            return value.ToString();
        }

        /// <summary>
        /// 获取所有枚举值的显示名称字典
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <returns>枚举值与显示名称的字典</returns>
        public static Dictionary<T, string> GetAllDisplayNames<T>() where T : struct, Enum
        {
            var result = new Dictionary<T, string>();
            foreach (var value in GetValues<T>())
            {
                result[value] = GetDisplayName(value);
            }
            return result;
        }

        /// <summary>
        /// 根据显示名称查找枚举值
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <param name="displayName">显示名称</param>
        /// <param name="ignoreCase">是否忽略大小写</param>
        /// <returns>匹配的枚举值，未找到则返回 null</returns>
        public static T? FromDisplayName<T>(string displayName, bool ignoreCase = true) where T : struct, Enum
        {
            if (string.IsNullOrEmpty(displayName)) return null;

            foreach (var value in GetValues<T>())
            {
                var name = GetDisplayName(value);
                if (ignoreCase)
                {
                    if (string.Equals(name, displayName, StringComparison.OrdinalIgnoreCase))
                        return value;
                }
                else
                {
                    if (name == displayName)
                        return value;
                }
            }
            return null;
        }

        #endregion

        #region 带描述的枚举项

        /// <summary>
        /// 获取带描述的枚举项列表
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <returns>带描述的枚举项列表</returns>
        public static IEnumerable<EnumItemWithDescription<T>> GetItemsWithDescription<T>() where T : struct, Enum
        {
            foreach (var value in GetValues<T>())
            {
                yield return new EnumItemWithDescription<T>
                {
                    Name = value.ToString(),
                    Value = value,
                    IntValue = Convert.ToInt32(value),
                    Description = GetDescription(value)
                };
            }
        }

        /// <summary>
        /// 获取完整的枚举项信息（包含 Description 和 Display）
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <returns>完整信息的枚举项列表</returns>
        public static IEnumerable<EnumItemFull<T>> GetItemsFull<T>() where T : struct, Enum
        {
            foreach (var value in GetValues<T>())
            {
                yield return new EnumItemFull<T>
                {
                    Name = value.ToString(),
                    Value = value,
                    IntValue = Convert.ToInt32(value),
                    Description = GetDescription(value),
                    DisplayName = GetDisplayName(value)
                };
            }
        }

        #endregion

        #region 基础方法

        /// <summary>
        /// 获取枚举所有值
        /// </summary>
        public static IEnumerable<T> GetValues<T>() where T : struct, Enum
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }

        /// <summary>
        /// 获取枚举所有名称
        /// </summary>
        public static IEnumerable<string> GetNames<T>() where T : struct, Enum
        {
            return Enum.GetNames(typeof(T));
        }

        /// <summary>
        /// 解析枚举
        /// </summary>
        public static T Parse<T>(string value, bool ignoreCase = true) where T : struct, Enum
        {
            return (T)Enum.Parse(typeof(T), value, ignoreCase);
        }

        /// <summary>
        /// 尝试解析枚举
        /// </summary>
        public static bool TryParse<T>(string value, out T result, bool ignoreCase = true) where T : struct, Enum
        {
            return Enum.TryParse(value, ignoreCase, out result);
        }

        /// <summary>
        /// 检查值是否定义
        /// </summary>
        public static bool IsDefined<T>(T value) where T : struct, Enum
        {
            return Enum.IsDefined(typeof(T), value);
        }

        /// <summary>
        /// 检查整数值是否定义
        /// </summary>
        public static bool IsDefined<T>(int value) where T : struct, Enum
        {
            return Enum.IsDefined(typeof(T), value);
        }

        /// <summary>
        /// 转换为整数
        /// </summary>
        public static int ToInt<T>(T value) where T : struct, Enum
        {
            return Convert.ToInt32(value);
        }

        /// <summary>
        /// 从整数转换
        /// </summary>
        public static T FromInt<T>(int value) where T : struct, Enum
        {
            return (T)Enum.ToObject(typeof(T), value);
        }

        /// <summary>
        /// 获取枚举项信息
        /// </summary>
        public static IEnumerable<EnumItem<T>> GetItems<T>() where T : struct, Enum
        {
            var type = typeof(T);
            var names = Enum.GetNames(type);
            var values = Enum.GetValues(type).Cast<T>();

            return names.Zip(values, (name, value) => new EnumItem<T>
            {
                Name = name,
                Value = value,
                IntValue = Convert.ToInt32(value)
            });
        }

        /// <summary>
        /// 获取枚举项数量
        /// </summary>
        public static int GetCount<T>() where T : struct, Enum
        {
            return Enum.GetNames(typeof(T)).Length;
        }

        /// <summary>
        /// 获取随机枚举值
        /// </summary>
        public static T GetRandomValue<T>(Random? random = null) where T : struct, Enum
        {
            var values = GetValues<T>().ToArray();
            var r = random ?? new Random();
            return values[r.Next(values.Length)];
        }

        /// <summary>
        /// 检查是否包含标志
        /// </summary>
        public static bool HasFlag<T>(T value, T flag) where T : struct, Enum
        {
            var intValue = Convert.ToInt64(value);
            var intFlag = Convert.ToInt64(flag);
            return (intValue & intFlag) == intFlag;
        }

        /// <summary>
        /// 设置标志
        /// </summary>
        public static T SetFlag<T>(T value, T flag, bool set = true) where T : struct, Enum
        {
            var intValue = Convert.ToInt64(value);
            var intFlag = Convert.ToInt64(flag);

            if (set)
                intValue |= intFlag;
            else
                intValue &= ~intFlag;

            return (T)Enum.ToObject(typeof(T), intValue);
        }

        /// <summary>
        /// 清除标志
        /// </summary>
        public static T ClearFlag<T>(T value, T flag) where T : struct, Enum
        {
            return SetFlag(value, flag, false);
        }

        /// <summary>
        /// 切换标志
        /// </summary>
        public static T ToggleFlag<T>(T value, T flag) where T : struct, Enum
        {
            var intValue = Convert.ToInt64(value);
            var intFlag = Convert.ToInt64(flag);
            intValue ^= intFlag;
            return (T)Enum.ToObject(typeof(T), intValue);
        }

        /// <summary>
        /// 获取所有标志
        /// </summary>
        public static IEnumerable<T> GetFlags<T>(T value) where T : struct, Enum
        {
            var intValue = Convert.ToInt64(value);

            foreach (var flag in GetValues<T>())
            {
                var intFlag = Convert.ToInt64(flag);
                if (intFlag != 0 && (intValue & intFlag) == intFlag)
                {
                    yield return flag;
                }
            }
        }

        /// <summary>
        /// 组合标志
        /// </summary>
        public static T CombineFlags<T>(params T[] flags) where T : struct, Enum
        {
            long result = 0;
            foreach (var flag in flags)
            {
                result |= Convert.ToInt64(flag);
            }
            return (T)Enum.ToObject(typeof(T), result);
        }

        #endregion
    }

    /// <summary>
    /// 枚举项信息
    /// </summary>
    public class EnumItem<T> where T : struct, Enum
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 枚举值
        /// </summary>
        public T Value { get; set; }

        /// <summary>
        /// 整数值
        /// </summary>
        public int IntValue { get; set; }

        public override string ToString()
        {
            return $"{Name} ({IntValue})";
        }
    }

    /// <summary>
    /// 带描述的枚举项信息
    /// </summary>
    public class EnumItemWithDescription<T> where T : struct, Enum
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 枚举值
        /// </summary>
        public T Value { get; set; }

        /// <summary>
        /// 整数值
        /// </summary>
        public int IntValue { get; set; }

        /// <summary>
        /// Description 属性描述
        /// </summary>
        public string Description { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"{Name} ({IntValue}): {Description}";
        }
    }

    /// <summary>
    /// 完整的枚举项信息（包含 Description 和 Display）
    /// </summary>
    public class EnumItemFull<T> where T : struct, Enum
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 枚举值
        /// </summary>
        public T Value { get; set; }

        /// <summary>
        /// 整数值
        /// </summary>
        public int IntValue { get; set; }

        /// <summary>
        /// Description 属性描述
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Display 属性显示名称
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"{Name} ({IntValue}): {DisplayName}";
        }
    }
}