using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EasyTool.TextCategory
{
    /// <summary>
    /// JSON工具类，基于System.Text.Json封装
    /// </summary>
    public static class JsonUtil
    {
        private static readonly Lazy<JsonSerializerOptions> _defaultOptions = new(() => new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            WriteIndented = false,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        });

        /// <summary>
        /// 默认的JSON序列化选项（线程安全懒加载）
        /// </summary>
        public static JsonSerializerOptions DefaultOptions => _defaultOptions.Value;

        #region 序列化

        /// <summary>
        /// 将对象序列化为JSON字符串
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="obj">要序列化的对象</param>
        /// <param name="options">序列化选项（可选）</param>
        /// <returns>JSON字符串</returns>
        public static string Serialize<T>(T obj, JsonSerializerOptions? options = null)
        {
            if (obj == null)
                return "null";

            return JsonSerializer.Serialize(obj, options ?? DefaultOptions);
        }

        /// <summary>
        /// 将对象序列化为JSON字符串（格式化输出）
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="obj">要序列化的对象</param>
        /// <param name="options">序列化选项（可选）</param>
        /// <returns>格式化的JSON字符串</returns>
        public static string SerializeIndented<T>(T obj, JsonSerializerOptions? options = null)
        {
            if (obj == null)
                return "null";

            var opts = options ?? DefaultOptions;
            var indentedOpts = new JsonSerializerOptions
            {
                PropertyNamingPolicy = opts.PropertyNamingPolicy,
                PropertyNameCaseInsensitive = opts.PropertyNameCaseInsensitive,
                WriteIndented = true,
                Encoder = opts.Encoder,
                DefaultIgnoreCondition = opts.DefaultIgnoreCondition,
                NumberHandling = opts.NumberHandling
            };

            return JsonSerializer.Serialize(obj, indentedOpts);
        }

        /// <summary>
        /// 将对象序列化为JSON字节数组
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="obj">要序列化的对象</param>
        /// <param name="options">序列化选项（可选）</param>
        /// <returns>JSON字节数组</returns>
        public static byte[] SerializeToBytes<T>(T obj, JsonSerializerOptions? options = null)
        {
            if (obj == null)
                return Array.Empty<byte>();

            return JsonSerializer.SerializeToUtf8Bytes(obj, options ?? DefaultOptions);
        }

        #endregion

        #region 反序列化

        /// <summary>
        /// 将JSON字符串反序列化为对象
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="json">JSON字符串</param>
        /// <param name="options">序列化选项（可选）</param>
        /// <returns>反序列化后的对象</returns>
        public static T? Deserialize<T>(string json, JsonSerializerOptions? options = null)
        {
            if (string.IsNullOrWhiteSpace(json))
                return default;

            return JsonSerializer.Deserialize<T>(json, options ?? DefaultOptions);
        }

        /// <summary>
        /// 将JSON字节数组反序列化为对象
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="jsonBytes">JSON字节数组</param>
        /// <param name="options">序列化选项（可选）</param>
        /// <returns>反序列化后的对象</returns>
        public static T? Deserialize<T>(byte[] jsonBytes, JsonSerializerOptions? options = null)
        {
            if (jsonBytes == null || jsonBytes.Length == 0)
                return default;

            return JsonSerializer.Deserialize<T>(jsonBytes, options ?? DefaultOptions);
        }

        /// <summary>
        /// 将JSON字符串反序列化为对象，失败返回默认值
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="json">JSON字符串</param>
        /// <param name="defaultValue">失败时返回的默认值</param>
        /// <param name="options">序列化选项（可选）</param>
        /// <returns>反序列化后的对象或默认值</returns>
        public static T? DeserializeOrDefault<T>(string json, T? defaultValue = default, JsonSerializerOptions? options = null)
        {
            if (string.IsNullOrWhiteSpace(json))
                return defaultValue;

            try
            {
                return JsonSerializer.Deserialize<T>(json, options ?? DefaultOptions);
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// 尝试将JSON字符串反序列化为对象
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="json">JSON字符串</param>
        /// <param name="result">反序列化后的对象</param>
        /// <param name="options">序列化选项（可选）</param>
        /// <returns>是否成功</returns>
        public static bool TryDeserialize<T>(string json, out T? result, JsonSerializerOptions? options = null)
        {
            result = default;

            if (string.IsNullOrWhiteSpace(json))
                return false;

            try
            {
                result = JsonSerializer.Deserialize<T>(json, options ?? DefaultOptions);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region JSON操作

        /// <summary>
        /// 格式化JSON字符串
        /// </summary>
        /// <param name="json">JSON字符串</param>
        /// <returns>格式化后的JSON字符串</returns>
        public static string Prettify(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return json;

            try
            {
                var element = JsonSerializer.Deserialize<JsonElement>(json);
                return JsonSerializer.Serialize(element, new JsonSerializerOptions { WriteIndented = true });
            }
            catch
            {
                return json;
            }
        }

        /// <summary>
        /// 压缩JSON字符串（移除空白）
        /// </summary>
        /// <param name="json">JSON字符串</param>
        /// <returns>压缩后的JSON字符串</returns>
        public static string Minify(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return json;

            try
            {
                var element = JsonSerializer.Deserialize<JsonElement>(json);
                return JsonSerializer.Serialize(element);
            }
            catch
            {
                return json;
            }
        }

        /// <summary>
        /// 验证JSON字符串是否有效
        /// </summary>
        /// <param name="json">JSON字符串</param>
        /// <returns>是否有效的JSON</returns>
        public static bool IsValid(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return false;

            try
            {
                using var document = JsonDocument.Parse(json);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 获取JSON值的类型
        /// </summary>
        /// <param name="json">JSON字符串</param>
        /// <returns>JSON值类型，无效时返回null</returns>
        public static JsonValueKind? GetValueKind(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            try
            {
                using var document = JsonDocument.Parse(json);
                return document.RootElement.ValueKind;
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region JSON路径操作

        /// <summary>
        /// 从JSON字符串中获取指定路径的值
        /// </summary>
        /// <param name="json">JSON字符串</param>
        /// <param name="path">属性路径（如: "user.name" 或 "data.items[0].id"）</param>
        /// <returns>找到的值，未找到返回null</returns>
        public static string? GetValue(string json, string path)
        {
            if (string.IsNullOrWhiteSpace(json) || string.IsNullOrWhiteSpace(path))
                return null;

            try
            {
                using var document = JsonDocument.Parse(json);
                var element = NavigateToPath(document.RootElement, path);
                return element?.ToString();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 从JSON字符串中获取指定路径的值并转换为指定类型
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="json">JSON字符串</param>
        /// <param name="path">属性路径</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>转换后的值</returns>
        public static T? GetValue<T>(string json, string path, T? defaultValue = default)
        {
            if (string.IsNullOrWhiteSpace(json) || string.IsNullOrWhiteSpace(path))
                return defaultValue;

            try
            {
                using var document = JsonDocument.Parse(json);
                var element = NavigateToPath(document.RootElement, path);

                if (element == null)
                    return defaultValue;

                return JsonSerializer.Deserialize<T>(element.Value.GetRawText(), DefaultOptions);
            }
            catch
            {
                return defaultValue;
            }
        }

        private static JsonElement? NavigateToPath(JsonElement root, string path)
        {
            var parts = path.Split(new[] { '.', '[', ']' }, StringSplitOptions.RemoveEmptyEntries);
            var current = root;

            foreach (var part in parts)
            {
                if (int.TryParse(part, out int index))
                {
                    if (current.ValueKind != JsonValueKind.Array || index >= current.GetArrayLength())
                        return null;

                    current = current[index];
                }
                else
                {
                    if (current.ValueKind != JsonValueKind.Object)
                        return null;

                    if (!current.TryGetProperty(part, out var property))
                        return null;

                    current = property;
                }
            }

            return current;
        }

        #endregion

        #region 类型转换

        /// <summary>
        /// 将JSON对象转换为字典
        /// </summary>
        /// <param name="json">JSON字符串</param>
        /// <returns>字典对象</returns>
        public static Dictionary<string, object?>? ToDictionary(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            return Deserialize<Dictionary<string, object?>>(json);
        }

        /// <summary>
        /// 将JSON数组转换为列表
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="json">JSON字符串</param>
        /// <returns>列表对象</returns>
        public static List<T>? ToList<T>(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            return Deserialize<List<T>>(json);
        }

        /// <summary>
        /// 深拷贝对象（通过JSON序列化/反序列化）
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="obj">要拷贝的对象</param>
        /// <returns>拷贝后的新对象</returns>
        public static T? DeepClone<T>(T obj)
        {
            if (obj == null)
                return default;

            var json = Serialize(obj);
            return Deserialize<T>(json);
        }

        #endregion
    }
}