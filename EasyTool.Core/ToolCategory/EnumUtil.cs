using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace EasyTool.ToolCategory
{
    /// <summary>
    /// 枚举增强工具类
    /// 提供枚举类型的扩展功能
    /// </summary>
    public static class EnumUtil
    {
#if NET5_0_OR_GREATER
        private static System.Random GetSharedRandom() => System.Random.Shared;
#else
        private static readonly ThreadLocal<System.Random> ThreadLocalRandom = new(() => new System.Random(Guid.NewGuid().GetHashCode()));
        private static System.Random GetSharedRandom() => ThreadLocalRandom.Value!;
#endif

        /// <summary>
        /// 获取枚举的所有值
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <returns>枚举值数组</returns>
        public static T[] GetValues<T>() where T : struct, Enum
        {
#if NET5_0_OR_GREATER
            return Enum.GetValues<T>();
#else
            return (T[])Enum.GetValues(typeof(T));
#endif
        }

        /// <summary>
        /// 获取枚举的所有名称
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <returns>名称数组</returns>
        public static string[] GetNames<T>() where T : struct, Enum
        {
#if NET5_0_OR_GREATER
            return Enum.GetNames<T>();
#else
            return Enum.GetNames(typeof(T));
#endif
        }

        /// <summary>
        /// 将名称转换为枚举值
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <param name="name">名称</param>
        /// <param name="ignoreCase">是否忽略大小写</param>
        /// <returns>枚举值</returns>
        public static T Parse<T>(string name, bool ignoreCase = false) where T : struct, Enum
        {
            return (T)Enum.Parse(typeof(T), name, ignoreCase);
        }

        /// <summary>
        /// 尝试将名称转换为枚举值
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <param name="name">名称</param>
        /// <param name="result">转换结果</param>
        /// <param name="ignoreCase">是否忽略大小写</param>
        /// <returns>是否转换成功</returns>
        public static bool TryParse<T>(string? name, out T result, bool ignoreCase = false) where T : struct, Enum
        {
            result = default;
            if (name == null) return false;
            
            try
            {
                result = (T)Enum.Parse(typeof(T), name, ignoreCase);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 将整数值转换为枚举
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <param name="value">整数值</param>
        /// <returns>枚举值</returns>
        public static T FromInt<T>(int value) where T : struct, Enum
        {
            return (T)(object)value;
        }

        /// <summary>
        /// 尝试将整数值转换为枚举
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <param name="value">整数值</param>
        /// <param name="result">转换结果</param>
        /// <returns>是否转换成功</returns>
        public static bool TryFromInt<T>(int value, out T result) where T : struct, Enum
        {
            result = default;
            if (!Enum.IsDefined(typeof(T), value))
                return false;

            result = (T)(object)value;
            return true;
        }

        /// <summary>
        /// 获取枚举值的名称
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <param name="value">枚举值</param>
        /// <returns>名称</returns>
        public static string GetName<T>(T value) where T : struct, Enum
        {
            return Enum.GetName(typeof(T), value) ?? string.Empty;
        }

        /// <summary>
        /// 检查值是否为有效的枚举值
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <param name="value">要检查的值</param>
        /// <returns>是否有效</returns>
        public static bool IsDefined<T>(T value) where T : struct, Enum
        {
            return Enum.IsDefined(typeof(T), value);
        }

        /// <summary>
        /// 检查整数值是否为有效的枚举值
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <param name="value">要检查的整数值</param>
        /// <returns>是否有效</returns>
        public static bool IsDefined<T>(int value) where T : struct, Enum
        {
            return Enum.IsDefined(typeof(T), value);
        }

        /// <summary>
        /// 获取枚举的基础类型
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <returns>基础类型</returns>
        public static Type GetUnderlyingType<T>() where T : struct, Enum
        {
            return Enum.GetUnderlyingType(typeof(T));
        }

        /// <summary>
        /// 获取枚举值的整数形式
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <param name="value">枚举值</param>
        /// <returns>整数值</returns>
        public static int ToInt<T>(T value) where T : struct, Enum
        {
            return Convert.ToInt32(value);
        }

        /// <summary>
        /// 获取枚举的描述信息列表
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <returns>描述信息列表</returns>
        public static List<EnumInfo<T>> GetInfoList<T>() where T : struct, Enum
        {
            return GetValues<T>()
                .Select(v => new EnumInfo<T>
                {
                    Value = v,
                    Name = GetName(v),
                    IntValue = ToInt(v)
                })
                .ToList();
        }

        /// <summary>
        /// 获取下一个枚举值（循环）
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <param name="value">当前值</param>
        /// <returns>下一个值</returns>
        public static T Next<T>(T value) where T : struct, Enum
        {
            var values = GetValues<T>();
            var index = Array.IndexOf(values, value);
            return values[(index + 1) % values.Length];
        }

        /// <summary>
        /// 获取上一个枚举值（循环）
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <param name="value">当前值</param>
        /// <returns>上一个值</returns>
        public static T Previous<T>(T value) where T : struct, Enum
        {
            var values = GetValues<T>();
            var index = Array.IndexOf(values, value);
            return values[(index - 1 + values.Length) % values.Length];
        }

        /// <summary>
        /// 获取枚举值数量
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <returns>数量</returns>
        public static int Count<T>() where T : struct, Enum
        {
            return Enum.GetNames(typeof(T)).Length;
        }

        /// <summary>
        /// 获取最小枚举值
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <returns>最小值</returns>
        public static T Min<T>() where T : struct, Enum
        {
            return GetValues<T>().Min();
        }

        /// <summary>
        /// 获取最大枚举值
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <returns>最大值</returns>
        public static T Max<T>() where T : struct, Enum
        {
            return GetValues<T>().Max();
        }

        /// <summary>
        /// 随机获取一个枚举值
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <returns>随机枚举值</returns>
        public static T Random<T>() where T : struct, Enum
        {
            var values = GetValues<T>();
            return values[GetSharedRandom().Next(values.Length)];
        }

        /// <summary>
        /// 检查是否为标志枚举（Flags）
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <returns>是否为标志枚举</returns>
        public static bool IsFlags<T>() where T : struct, Enum
        {
            return typeof(T).IsDefined(typeof(FlagsAttribute), false);
        }

        /// <summary>
        /// 获取标志枚举中设置的所有标志
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <param name="flags">标志值</param>
        /// <returns>设置的标志列表</returns>
        public static List<T> GetFlags<T>(T flags) where T : struct, Enum
        {
            var result = new List<T>();
            foreach (var value in GetValues<T>())
            {
                if (flags.HasFlag(value) && ToInt(value) != 0)
                {
                    result.Add(value);
                }
            }
            return result;
        }

        /// <summary>
        /// 设置标志
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <param name="flags">当前标志</param>
        /// <param name="flag">要设置的标志</param>
        /// <returns>新的标志值</returns>
        public static T SetFlag<T>(T flags, T flag) where T : struct, Enum
        {
            return (T)(object)(ToInt(flags) | ToInt(flag));
        }

        /// <summary>
        /// 清除标志
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <param name="flags">当前标志</param>
        /// <param name="flag">要清除的标志</param>
        /// <returns>新的标志值</returns>
        public static T ClearFlag<T>(T flags, T flag) where T : struct, Enum
        {
            return (T)(object)(ToInt(flags) & ~ToInt(flag));
        }

        /// <summary>
        /// 切换标志
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <param name="flags">当前标志</param>
        /// <param name="flag">要切换的标志</param>
        /// <returns>新的标志值</returns>
        public static T ToggleFlag<T>(T flags, T flag) where T : struct, Enum
        {
            return (T)(object)(ToInt(flags) ^ ToInt(flag));
        }

        /// <summary>
        /// 创建枚举值的字典（名称 -> 值）
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <returns>字典</returns>
        public static Dictionary<string, T> ToDictionary<T>() where T : struct, Enum
        {
            return GetNames<T>().ToDictionary(
                name => name,
                name => Parse<T>(name));
        }

        /// <summary>
        /// 创建枚举值的字典（值 -> 名称）
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <returns>字典</returns>
        public static Dictionary<int, string> ToValueNameDictionary<T>() where T : struct, Enum
        {
            return GetValues<T>().ToDictionary(
                value => ToInt(value),
                value => GetName(value));
        }
    }

    /// <summary>
    /// 枚举信息
    /// </summary>
    /// <typeparam name="T">枚举类型</typeparam>
    public class EnumInfo<T> where T : struct, Enum
    {
        /// <summary>
        /// 枚举值
        /// </summary>
        public T Value { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 整数值
        /// </summary>
        public int IntValue { get; set; }

        public override string ToString() => $"{Name} = {IntValue}";
    }
}
