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
        public static JsonSerializerOptions DefaultOptions => new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
            }
        };

        /// <summary>
        /// 紧凑序列化选项（无缩进、忽略null、驼峰命名）
        /// </summary>
        public static JsonSerializerOptions CompactOptions => new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };

        /// <summary>
        /// 宽松反序列化选项（允许不带引号的数字、允许注释、允许尾随逗号）
        /// </summary>
        public static JsonSerializerOptions LenientOptions => new()
        {
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.AllowNamedFloatingPointLiterals,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };

        #endregion

        #region 序列化

        /// <summary>
        /// 将对象序列化为 JSON 字符串
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="obj">要序列化的对象</param>
        /// <param name="options">序列化选项（可选）</param>
        /// <returns>JSON 字符串</returns>
        public static string Serialize<T>(T obj, JsonSerializerOptions? options = null)
        {
            if (obj == null)
                return "null";

            return JsonSerializer.Serialize(obj, options ?? DefaultOptions);
        }

        /// <summary>
        /// 将对象序列化为 JSON 字符串（紧凑格式）
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="obj">要序列化的对象</param>
        /// <returns>紧凑格式的 JSON 字符串</returns>
        public static string SerializeCompact<T>(T obj)
        {
            return Serialize(obj, CompactOptions);
        }

        /// <summary>
        /// 将对象序列化为 JSON 字节数组
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="obj">要序列化的对象</param>
        /// <param name="options">序列化选项（可选）</param>
        /// <returns>JSON 字节数组</returns>
        public static byte[] SerializeToUtf8Bytes<T>(T obj, JsonSerializerOptions? options = null)
        {
            return JsonSerializer.SerializeToUtf8Bytes(obj, options ?? DefaultOptions);
        }

        #endregion

        #region 反序列化

        /// <summary>
        /// 将 JSON 字符串反序列化为对象
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="json">JSON 字符串</param>
        /// <param name="options">反序列化选项（可选）</param>
        /// <returns>反序列化后的对象</returns>
        public static T? Deserialize<T>(string json, JsonSerializerOptions? options = null)
        {
            if (string.IsNullOrWhiteSpace(json))
                return default;

            return JsonSerializer.Deserialize<T>(json, options ?? LenientOptions);
        }

        /// <summary>
        /// 将 JSON 字节数组反序列化为对象
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="utf8Json">JSON 字节数组</param>
        /// <param name="options">反序列化选项（可选）</param>
        /// <returns>反序列化后的对象</returns>
        public static T? Deserialize<T>(byte[] utf8Json, JsonSerializerOptions? options = null)
        {
            if (utf8Json == null || utf8Json.Length == 0)
                return default;

            return JsonSerializer.Deserialize<T>(utf8Json, options ?? LenientOptions);
        }

        /// <summary>
        /// 尝试将 JSON 字符串反序列化为对象
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="json">JSON 字符串</param>
        /// <param name="result">反序列化结果</param>
        /// <param name="options">反序列化选项（可选）</param>
        /// <returns>是否成功</returns>
        public static bool TryDeserialize<T>(string json, out T? result, JsonSerializerOptions? options = null)
        {
            result = default;
            if (string.IsNullOrWhiteSpace(json))
                return false;

            try
            {
                result = JsonSerializer.Deserialize<T>(json, options ?? LenientOptions);
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
        /// <param name="json">JSON 字符串</param>
        /// <returns>JsonNode 对象</returns>
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
        /// <param name="json">JSON 字符串</param>
        /// <param name="indent">缩进字符（默认2个空格）</param>
        /// <returns>格式化后的 JSON 字符串</returns>
        public static string Prettify(string json, string indent = "  ")
        {
            if (string.IsNullOrWhiteSpace(json))
                return json;

            try
            {
                var node = JsonNode.Parse(json);
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
#if NET9_0_OR_GREATER
                if (indent.Length > 0)
                {
                    options.IndentCharacter = indent[0];
                    options.IndentSize = indent.Length;
                }
#endif
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
        /// <param name="json">JSON 字符串</param>
        /// <returns>压缩后的 JSON 字符串</returns>
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
        /// <param name="json">JSON 字符串</param>
        /// <returns>是否有效</returns>
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
        /// <param name="json">JSON 字符串</param>
        /// <param name="path">路径（使用点号分隔，如 "user.name"）</param>
        /// <returns>找到的值，未找到返回 null</returns>
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
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="json">JSON 字符串</param>
        /// <param name="path">路径</param>
        /// <returns>转换后的值</returns>
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
                return jsonNode.Deserialize<T>(LenientOptions);
            }

            return (T?)Convert.ChangeType(value, typeof(T));
        }

        /// <summary>
        /// 设置 JSON 字符串中指定路径的值
        /// </summary>
        /// <param name="json">JSON 字符串</param>
        /// <param name="path">路径</param>
        /// <param name="value">要设置的值</param>
        /// <returns>修改后的 JSON 字符串</returns>
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

                // 处理数组索引，如 items[0]
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
        /// <typeparam name="TKey">键类型</typeparam>
        /// <typeparam name="TValue">值类型</typeparam>
        /// <param name="dictionary">字典</param>
        /// <returns>JSON 字符串</returns>
        public static string FromDictionary<TKey, TValue>(Dictionary<TKey, TValue> dictionary)
        {
            if (dictionary == null)
                return "{}";

            return JsonSerializer.Serialize(dictionary, DefaultOptions);
        }

        /// <summary>
        /// 将 JSON 对象转换为字典
        /// </summary>
        /// <typeparam name="TValue">值类型</typeparam>
        /// <param name="json">JSON 字符串</param>
        /// <returns>字典</returns>
        public static Dictionary<string, TValue?>? ToDictionary<TValue>(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            return JsonSerializer.Deserialize<Dictionary<string, TValue?>>(json, LenientOptions);
        }

        /// <summary>
        /// 将匿名对象转换为 JSON 字符串
        /// </summary>
        /// <param name="obj">匿名对象</param>
        /// <returns>JSON 字符串</returns>
        public static string FromAnonymous(object obj)
        {
            return Serialize(obj, CompactOptions);
        }

        /// <summary>
        /// 深拷贝对象（通过 JSON 序列化/反序列化）
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="obj">要拷贝的对象</param>
        /// <returns>拷贝后的对象</returns>
        public static T? DeepClone<T>(T obj)
        {
            if (obj == null)
                return default;

            var json = JsonSerializer.Serialize(obj, DefaultOptions);
            return JsonSerializer.Deserialize<T>(json, LenientOptions);
        }

        #endregion

        #region 合并操作

        /// <summary>
        /// 合并两个 JSON 对象
        /// </summary>
        /// <param name="json1">第一个 JSON</param>
        /// <param name="json2">第二个 JSON（优先级更高）</param>
        /// <returns>合并后的 JSON</returns>
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
