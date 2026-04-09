using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyTool
{
    /// <summary>
    /// Map 操作工具类
    /// 对标 Hutool 的 MapUtil
    /// 提供字典的创建、判空、合并、排序等常用操作
    /// </summary>
    public static class MapUtil
    {
        #region 创建 Map

        /// <summary>
        /// 创建字典
        /// </summary>
        /// <typeparam name="TKey">键类型</typeparam>
        /// <typeparam name="TValue">值类型</typeparam>
        /// <returns>字典</returns>
        public static Dictionary<TKey, TValue> NewHashMap<TKey, TValue>()
            where TKey : notnull
        {
            return new Dictionary<TKey, TValue>();
        }

        /// <summary>
        /// 创建字典（初始容量）
        /// </summary>
        /// <typeparam name="TKey">键类型</typeparam>
        /// <typeparam name="TValue">值类型</typeparam>
        /// <param name="capacity">初始容量</param>
        /// <returns>字典</returns>
        public static Dictionary<TKey, TValue> NewHashMap<TKey, TValue>(int capacity)
            where TKey : notnull
        {
            return new Dictionary<TKey, TValue>(capacity);
        }

        /// <summary>
        /// 创建字典（键值对）
        /// </summary>
        /// <typeparam name="TKey">键类型</typeparam>
        /// <typeparam name="TValue">值类型</typeparam>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <returns>字典</returns>
        public static Dictionary<TKey, TValue> NewHashMap<TKey, TValue>(TKey key, TValue value)
            where TKey : notnull
        {
            return new Dictionary<TKey, TValue> { { key, value } };
        }

        /// <summary>
        /// 创建字典（多个键值对）
        /// </summary>
        /// <typeparam name="TKey">键类型</typeparam>
        /// <typeparam name="TValue">值类型</typeparam>
        /// <param name="keyValues">键值对数组</param>
        /// <returns>字典</returns>
        public static Dictionary<TKey, TValue> NewHashMap<TKey, TValue>(params (TKey key, TValue value)[] keyValues)
            where TKey : notnull
        {
            var dict = new Dictionary<TKey, TValue>();
            if (keyValues != null)
            {
                foreach (var (key, value) in keyValues)
                {
                    dict[key] = value;
                }
            }
            return dict;
        }

        #endregion

        #region 判空

        /// <summary>
        /// 判断字典是否为空
        /// </summary>
        /// <typeparam name="TKey">键类型</typeparam>
        /// <typeparam name="TValue">值类型</typeparam>
        /// <param name="dict">字典</param>
        /// <returns>是否为空</returns>
        public static bool IsEmpty<TKey, TValue>(IDictionary<TKey, TValue>? dict)
        {
            return dict == null || dict.Count == 0;
        }

        /// <summary>
        /// 判断字典是否不为空
        /// </summary>
        /// <typeparam name="TKey">键类型</typeparam>
        /// <typeparam name="TValue">值类型</typeparam>
        /// <param name="dict">字典</param>
        /// <returns>是否不为空</returns>
        public static bool IsNotEmpty<TKey, TValue>(IDictionary<TKey, TValue>? dict)
        {
            return !IsEmpty(dict);
        }

        /// <summary>
        /// 获取字典大小
        /// </summary>
        /// <typeparam name="TKey">键类型</typeparam>
        /// <typeparam name="TValue">值类型</typeparam>
        /// <param name="dict">字典</param>
        /// <returns>大小</returns>
        public static int Size<TKey, TValue>(IDictionary<TKey, TValue>? dict)
        {
            return dict?.Count ?? 0;
        }

        #endregion

        #region 获取值

        /// <summary>
        /// 获取值（带默认值）
        /// </summary>
        /// <typeparam name="TKey">键类型</typeparam>
        /// <typeparam name="TValue">值类型</typeparam>
        /// <param name="dict">字典</param>
        /// <param name="key">键</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>值</returns>
        public static TValue Get<TKey, TValue>(IDictionary<TKey, TValue>? dict, TKey key, TValue defaultValue = default)
        {
            if (dict == null)
                return defaultValue;

            return dict.TryGetValue(key, out var value) ? value : defaultValue;
        }

        /// <summary>
        /// 获取值（通过选择器提供默认值）
        /// </summary>
        /// <typeparam name="TKey">键类型</typeparam>
        /// <typeparam name="TValue">值类型</typeparam>
        /// <param name="dict">字典</param>
        /// <param name="key">键</param>
        /// <param name="defaultSelector">默认值选择器</param>
        /// <returns>值</returns>
        public static TValue Get<TKey, TValue>(IDictionary<TKey, TValue>? dict, TKey key, Func<TValue> defaultSelector)
        {
            if (dict == null || defaultSelector == null)
                return default;

            return dict.TryGetValue(key, out var value) ? value : defaultSelector();
        }

        /// <summary>
        /// 获取或添加值
        /// </summary>
        /// <typeparam name="TKey">键类型</typeparam>
        /// <typeparam name="TValue">值类型</typeparam>
        /// <param name="dict">字典</param>
        /// <param name="key">键</param>
        /// <param name="valueFactory">值工厂</param>
        /// <returns>值</returns>
        public static TValue GetOrAdd<TKey, TValue>(IDictionary<TKey, TValue> dict, TKey key, Func<TValue> valueFactory)
            where TKey : notnull
        {
            if (dict == null)
                throw new ArgumentNullException(nameof(dict));

            if (valueFactory == null)
                throw new ArgumentNullException(nameof(valueFactory));

            if (!dict.TryGetValue(key, out var value))
            {
                value = valueFactory();
                dict[key] = value;
            }

            return value;
        }

        /// <summary>
        /// 获取并移除值
        /// </summary>
        /// <typeparam name="TKey">键类型</typeparam>
        /// <typeparam name="TValue">值类型</typeparam>
        /// <param name="dict">字典</param>
        /// <param name="key">键</param>
        /// <returns>值</returns>
        public static TValue? RemoveAndGet<TKey, TValue>(IDictionary<TKey, TValue>? dict, TKey key)
        {
            if (dict == null)
                return default;

            if (dict.TryGetValue(key, out var value))
            {
                dict.Remove(key);
                return value;
            }

            return default;
        }

        #endregion

        #region 合并

        /// <summary>
        /// 合并两个字典（后者覆盖前者）
        /// </summary>
        /// <typeparam name="TKey">键类型</typeparam>
        /// <typeparam name="TValue">值类型</typeparam>
        /// <param name="first">第一个字典</param>
        /// <param name="second">第二个字典</param>
        /// <returns>合并后的字典</returns>
        public static Dictionary<TKey, TValue> Merge<TKey, TValue>(
            IDictionary<TKey, TValue>? first,
            IDictionary<TKey, TValue>? second)
            where TKey : notnull
        {
            var result = new Dictionary<TKey, TValue>();

            if (first != null)
            {
                foreach (var kvp in first)
                {
                    result[kvp.Key] = kvp.Value;
                }
            }

            if (second != null)
            {
                foreach (var kvp in second)
                {
                    result[kvp.Key] = kvp.Value;
                }
            }

            return result;
        }

        /// <summary>
        /// 合并多个字典
        /// </summary>
        /// <typeparam name="TKey">键类型</typeparam>
        /// <typeparam name="TValue">值类型</typeparam>
        /// <param name="dicts">字典数组</param>
        /// <returns>合并后的字典</returns>
        public static Dictionary<TKey, TValue> Merge<TKey, TValue>(params IDictionary<TKey, TValue>[] dicts)
            where TKey : notnull
        {
            var result = new Dictionary<TKey, TValue>();

            if (dicts != null)
            {
                foreach (var dict in dicts)
                {
                    if (dict != null)
                    {
                        foreach (var kvp in dict)
                        {
                            result[kvp.Key] = kvp.Value;
                        }
                    }
                }
            }

            return result;
        }

        #endregion

        #region 过滤和转换

        /// <summary>
        /// 过滤字典
        /// </summary>
        /// <typeparam name="TKey">键类型</typeparam>
        /// <typeparam name="TValue">值类型</typeparam>
        /// <param name="dict">字典</param>
        /// <param name="predicate">条件</param>
        /// <returns>过滤后的字典</returns>
        public static Dictionary<TKey, TValue> Filter<TKey, TValue>(
            IDictionary<TKey, TValue>? dict,
            Func<TKey, TValue, bool> predicate)
            where TKey : notnull
        {
            if (dict == null || predicate == null)
                return new Dictionary<TKey, TValue>();

            return dict.Where(kvp => predicate(kvp.Key, kvp.Value))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        /// <summary>
        /// 转换键
        /// </summary>
        /// <typeparam name="TKey">原键类型</typeparam>
        /// <typeparam name="TValue">值类型</typeparam>
        /// <typeparam name="TNewKey">新键类型</typeparam>
        /// <param name="dict">字典</param>
        /// <param name="keySelector">键选择器</param>
        /// <returns>新字典</returns>
        public static Dictionary<TNewKey, TValue> MapKeys<TKey, TValue, TNewKey>(
            IDictionary<TKey, TValue>? dict,
            Func<TKey, TNewKey> keySelector)
            where TNewKey : notnull
        {
            if (dict == null || keySelector == null)
                return new Dictionary<TNewKey, TValue>();

            return dict.ToDictionary(kvp => keySelector(kvp.Key), kvp => kvp.Value);
        }

        /// <summary>
        /// 转换值
        /// </summary>
        /// <typeparam name="TKey">键类型</typeparam>
        /// <typeparam name="TValue">原值类型</typeparam>
        /// <typeparam name="TNewValue">新值类型</typeparam>
        /// <param name="dict">字典</param>
        /// <param name="valueSelector">值选择器</param>
        /// <returns>新字典</returns>
        public static Dictionary<TKey, TNewValue> MapValues<TKey, TValue, TNewValue>(
            IDictionary<TKey, TValue>? dict,
            Func<TKey, TValue, TNewValue> valueSelector)
            where TKey : notnull
        {
            if (dict == null || valueSelector == null)
                return new Dictionary<TKey, TNewValue>();

            return dict.ToDictionary(kvp => kvp.Key, kvp => valueSelector(kvp.Key, kvp.Value));
        }

        /// <summary>
        /// 反转字典（键值互换）
        /// </summary>
        /// <typeparam name="TKey">键类型</typeparam>
        /// <typeparam name="TValue">值类型</typeparam>
        /// <param name="dict">字典</param>
        /// <returns>反转后的字典</returns>
        public static Dictionary<TValue, TKey> Invert<TKey, TValue>(IDictionary<TKey, TValue>? dict)
            where TValue : notnull
        {
            if (dict == null)
                return new Dictionary<TValue, TKey>();

            return dict.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
        }

        #endregion

        #region 排序

        /// <summary>
        /// 按键排序
        /// </summary>
        /// <typeparam name="TKey">键类型</typeparam>
        /// <typeparam name="TValue">值类型</typeparam>
        /// <param name="dict">字典</param>
        /// <param name="descending">是否降序</param>
        /// <returns>排序后的字典</returns>
        public static Dictionary<TKey, TValue> SortByKey<TKey, TValue>(IDictionary<TKey, TValue>? dict, bool descending = false)
            where TKey : notnull
        {
            if (dict == null)
                return new Dictionary<TKey, TValue>();

            var sorted = descending
                ? dict.OrderByDescending(kvp => kvp.Key)
                : dict.OrderBy(kvp => kvp.Key);

            return sorted.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        /// <summary>
        /// 按值排序
        /// </summary>
        /// <typeparam name="TKey">键类型</typeparam>
        /// <typeparam name="TValue">值类型</typeparam>
        /// <param name="dict">字典</param>
        /// <param name="descending">是否降序</param>
        /// <returns>排序后的字典</returns>
        public static Dictionary<TKey, TValue> SortByValue<TKey, TValue>(IDictionary<TKey, TValue>? dict, bool descending = false)
            where TKey : notnull
        {
            if (dict == null)
                return new Dictionary<TKey, TValue>();

            var sorted = descending
                ? dict.OrderByDescending(kvp => kvp.Value)
                : dict.OrderBy(kvp => kvp.Value);

            return sorted.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        #endregion

        #region 其他操作

        /// <summary>
        /// 获取所有键
        /// </summary>
        /// <typeparam name="TKey">键类型</typeparam>
        /// <typeparam name="TValue">值类型</typeparam>
        /// <param name="dict">字典</param>
        /// <returns>键列表</returns>
        public static List<TKey> Keys<TKey, TValue>(IDictionary<TKey, TValue>? dict)
        {
            if (dict == null)
                return new List<TKey>();

            return dict.Keys.ToList();
        }

        /// <summary>
        /// 获取所有值
        /// </summary>
        /// <typeparam name="TKey">键类型</typeparam>
        /// <typeparam name="TValue">值类型</typeparam>
        /// <param name="dict">字典</param>
        /// <returns>值列表</returns>
        public static List<TValue> Values<TKey, TValue>(IDictionary<TKey, TValue>? dict)
        {
            if (dict == null)
                return new List<TValue>();

            return dict.Values.ToList();
        }

        /// <summary>
        /// 遍历字典
        /// </summary>
        /// <typeparam name="TKey">键类型</typeparam>
        /// <typeparam name="TValue">值类型</typeparam>
        /// <param name="dict">字典</param>
        /// <param name="action">操作</param>
        public static void ForEach<TKey, TValue>(IDictionary<TKey, TValue>? dict, Action<TKey, TValue> action)
        {
            if (dict == null || action == null)
                return;

            foreach (var kvp in dict)
            {
                action(kvp.Key, kvp.Value);
            }
        }

        /// <summary>
        /// 移除所有符合条件的项
        /// </summary>
        /// <typeparam name="TKey">键类型</typeparam>
        /// <typeparam name="TValue">值类型</typeparam>
        /// <param name="dict">字典</param>
        /// <param name="predicate">条件</param>
        /// <returns>移除的数量</returns>
        public static int RemoveAll<TKey, TValue>(IDictionary<TKey, TValue>? dict, Func<TKey, TValue, bool> predicate)
        {
            if (dict == null || predicate == null)
                return 0;

            var keysToRemove = dict.Where(kvp => predicate(kvp.Key, kvp.Value))
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in keysToRemove)
            {
                dict.Remove(key);
            }

            return keysToRemove.Count;
        }

        #endregion
    }
}