using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace EasyTool.IOCategory
{
    /// <summary>
    /// JSON 工具类
    /// 提供 JSON 序列化/反序列化的增强功能
    /// </summary>
    public static class JsonUtil
    {
        #region 默认选项

        /// <summary>
        /// 默认序列化选项（驼峰命名、缩进、忽略null）
        /// </summary>
        public static JsonSerializerOptions DefaultOptions
        {
            get
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    NumberHandling = JsonNumberHandling.AllowReadingFromString
                };
                options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
                return options;
            }
        }

        /// <summary>
        /// 紧凑序列化选项（无缩进、忽略null、驼峰命名）
        /// </summary>
        public static JsonSerializerOptions CompactOptions
        {
            get
            {
                return new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = false,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    NumberHandling = JsonNumberHandling.AllowReadingFromString
                };
            }
        }

        /// <summary>
        /// 宽松反序列化选项（允许不带引号的数字、允许注释、允许尾随逗号）
        /// </summary>
        public static JsonSerializerOptions LenientOptions
        {
            get
            {
                return new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.AllowNamedFloatingPointLiterals,
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    AllowTrailingCommas = true
                };
            }
        }

        #endregion

        #region 序列化

        /// <summary>
        /// 将对象序列化为 JSON 字符串
        /// </summary>
        public static string Serialize<T>(T obj, JsonSerializerOptions? options = null)
        {
            if (obj == null)
                return "null";

            // 使用泛型方法
            return JsonSerializer.Serialize(obj);
        }

        /// <summary>
        /// 将对象序列化为 JSON 字符串（紧凑格式）
        /// </summary>
        public static string SerializeCompact<T>(T obj)
        {
            return JsonSerializer.Serialize(obj);
        }

        /// <summary>
        /// 将对象序列化为 JSON 字节数组
        /// </summary>
        public static byte[] SerializeToUtf8Bytes<T>(T obj, JsonSerializerOptions? options = null)
        {
            var json = JsonSerializer.Serialize(obj);
            return Encoding.UTF8.GetBytes(json);
        }

        #endregion

        #region 反序列化

        /// <summary>
        /// 将 JSON 字符串反序列化为对象
        /// </summary>
        public static T? Deserialize<T>(string json, JsonSerializerOptions? options = null)
        {
            if (string.IsNullOrWhiteSpace(json))
                return default;

            return JsonSerializer.Deserialize<T>(json);
        }

        /// <summary>
        /// 将 JSON 字节数组反序列化为对象
        /// </summary>
        public static T? Deserialize<T>(byte[] utf8Json, JsonSerializerOptions? options = null)
        {
            if (utf8Json == null || utf8Json.Length == 0)
                return default;

            var json = Encoding.UTF8.GetString(utf8Json);
            return JsonSerializer.Deserialize<T>(json);
        }

        /// <summary>
        /// 尝试将 JSON 字符串反序列化为对象
        /// </summary>
        public static bool TryDeserialize<T>(string json, out T? result, JsonSerializerOptions? options = null)
        {
            result = default;
            if (string.IsNullOrWhiteSpace(json))
                return false;

            try
            {
                result = Deserialize<T>(json, options);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 将 JSON 字符串反序列化为动态对象
        /// </summary>
        public static JsonNode? Parse(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            return JsonNode.Parse(json);
        }

        #endregion

        #region 格式化与验证

        /// <summary>
        /// 格式化 JSON 字符串（美化输出）
        /// </summary>
        public static string Prettify(string json, string indent = "  ")
        {
            if (string.IsNullOrWhiteSpace(json))
                return json;

            try
            {
                var node = JsonNode.Parse(json);
                var options = new JsonSerializerOptions { WriteIndented = true };
                return node?.ToJsonString(options) ?? json;
            }
            catch
            {
                return json;
            }
        }

        /// <summary>
        /// 压缩 JSON 字符串（移除空白）
        /// </summary>
        public static string Minify(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return json;

            try
            {
                var node = JsonNode.Parse(json);
                return node?.ToJsonString(new JsonSerializerOptions { WriteIndented = false }) ?? json;
            }
            catch
            {
                return json;
            }
        }

        /// <summary>
        /// 验证是否为有效的 JSON
        /// </summary>
        public static bool IsValid(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return false;

            try
            {
                JsonNode.Parse(json);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region 路径操作

        /// <summary>
        /// 从 JSON 字符串中获取指定路径的值
        /// </summary>
        public static object? GetValue(string json, string path)
        {
            if (string.IsNullOrWhiteSpace(json) || string.IsNullOrWhiteSpace(path))
                return null;

            try
            {
                var node = JsonNode.Parse(json);
                return GetValueByPath(node, path);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 从 JSON 字符串中获取指定路径的值并转换为指定类型
        /// </summary>
        public static T? GetValue<T>(string json, string path)
        {
            var value = GetValue(json, path);
            if (value == null)
                return default;

            if (value is JsonValue jsonValue)
            {
                return jsonValue.GetValue<T>();
            }

            if (value is JsonNode jsonNode)
            {
                return Deserialize<T>(jsonNode.ToJsonString());
            }

            return (T?)Convert.ChangeType(value, typeof(T));
        }

        /// <summary>
        /// 设置 JSON 字符串中指定路径的值
        /// </summary>
        public static string SetValue(string json, string path, object? value)
        {
            if (string.IsNullOrWhiteSpace(json))
                return json;

            try
            {
                var node = JsonNode.Parse(json);
                SetValueByPath(node, path, value);
                return node?.ToJsonString(DefaultOptions) ?? json;
            }
            catch
            {
                return json;
            }
        }

        private static object? GetValueByPath(JsonNode? node, string path)
        {
            if (node == null)
                return null;

            var parts = path.Split('.');
            JsonNode? current = node;

            foreach (var part in parts)
            {
                if (current == null)
                    return null;

                if (part.Contains('[') && part.EndsWith(']'))
                {
                    var name = part.Substring(0, part.IndexOf('['));
                    var indexStr = part.Substring(part.IndexOf('[') + 1, part.Length - part.IndexOf('[') - 2);

                    if (!string.IsNullOrEmpty(name))
                        current = current[name];

                    if (int.TryParse(indexStr, out int index) && current is JsonArray array)
                    {
                        current = index < array.Count ? array[index] : null;
                    }
                }
                else
                {
                    current = current[part];
                }
            }

            return current;
        }

        private static void SetValueByPath(JsonNode? node, string path, object? value)
        {
            if (node == null)
                return;

            var parts = path.Split('.');
            JsonNode current = node;

            for (int i = 0; i < parts.Length - 1; i++)
            {
                var part = parts[i];

                if (part.Contains('[') && part.EndsWith(']'))
                {
                    var name = part.Substring(0, part.IndexOf('['));
                    var indexStr = part.Substring(part.IndexOf('[') + 1, part.Length - part.IndexOf('[') - 2);

                    if (!string.IsNullOrEmpty(name))
                    {
                        if (current[name] == null)
                            current[name] = new JsonObject();
                        current = current[name]!;
                    }

                    if (int.TryParse(indexStr, out int index))
                    {
                        if (current is JsonArray array)
                        {
                            while (array.Count <= index)
                                array.Add(null);
                            current = array[index] ??= new JsonObject();
                        }
                    }
                }
                else
                {
                    if (current[part] == null)
                        current[part] = new JsonObject();
                    current = current[part]!;
                }
            }

            var lastPart = parts[^1];
            if (lastPart.Contains('[') && lastPart.EndsWith(']'))
            {
                var name = lastPart.Substring(0, lastPart.IndexOf('['));
                var indexStr = lastPart.Substring(lastPart.IndexOf('[') + 1, lastPart.Length - lastPart.IndexOf('[') - 2);

                JsonNode? target = current;
                if (!string.IsNullOrEmpty(name))
                {
                    if (current[name] == null)
                        current[name] = new JsonArray();
                    target = current[name];
                }

                if (int.TryParse(indexStr, out int index) && target is JsonArray array)
                {
                    while (array.Count <= index)
                        array.Add(null);
                    array[index] = JsonValue.Create(value);
                }
            }
            else
            {
                current[lastPart] = JsonValue.Create(value);
            }
        }

        #endregion

        #region 转换操作

        /// <summary>
        /// 将字典转换为 JSON 对象
        /// </summary>
        public static string FromDictionary<TKey, TValue>(Dictionary<TKey, TValue> dictionary)
        {
            if (dictionary == null)
                return "{}";

            return JsonSerializer.Serialize(dictionary);
        }

        /// <summary>
        /// 将 JSON 对象转换为字典
        /// </summary>
        public static Dictionary<string, TValue?>? ToDictionary<TValue>(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            return JsonSerializer.Deserialize<Dictionary<string, TValue?>>(json);
        }

        /// <summary>
        /// 将匿名对象转换为 JSON 字符串
        /// </summary>
        public static string FromAnonymous(object obj)
        {
            return JsonSerializer.Serialize(obj);
        }

        /// <summary>
        /// 深拷贝对象（通过 JSON 序列化/反序列化）
        /// </summary>
        public static T? DeepClone<T>(T obj)
        {
            if (obj == null)
                return default;

            var json = JsonSerializer.Serialize(obj);
            return JsonSerializer.Deserialize<T>(json);
        }

        #endregion

        #region 合并操作

        /// <summary>
        /// 合并两个 JSON 对象
        /// </summary>
        public static string Merge(string json1, string json2)
        {
            if (string.IsNullOrWhiteSpace(json1))
                return json2;
            if (string.IsNullOrWhiteSpace(json2))
                return json1;

            try
            {
                var node1 = JsonNode.Parse(json1) as JsonObject;
                var node2 = JsonNode.Parse(json2) as JsonObject;

                if (node1 == null)
                    return json2;
                if (node2 == null)
                    return json1;

                MergeObjects(node1, node2);
                return node1.ToJsonString(DefaultOptions);
            }
            catch
            {
                return json1;
            }
        }

        private static void MergeObjects(JsonObject target, JsonObject source)
        {
            foreach (var property in source)
            {
                if (target.ContainsKey(property.Key))
                {
                    if (target[property.Key] is JsonObject targetObj &&
                        property.Value is JsonObject sourceObj)
                    {
                        MergeObjects(targetObj, sourceObj);
                    }
                    else
                    {
                        target[property.Key] = property.Value?.DeepClone();
                    }
                }
                else
                {
                    target[property.Key] = property.Value?.DeepClone();
                }
            }
        }

        #endregion
    }
}