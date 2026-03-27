using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyTool.ReflectCategory
{
    /// <summary>
    /// 枚举工具类
    /// </summary>
    public static class EnumUtil
    {
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
}