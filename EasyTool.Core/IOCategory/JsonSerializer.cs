using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EasyTool.IOCategory
{
    /// <summary>
    /// JSON序列化工具增强版
    /// </summary>
    public static class JsonSerializer
    {
        private static JsonSerializerOptions _defaultOptions;
        private static JsonSerializerOptions _indentedOptions;
        private static JsonSerializerOptions _camelCaseOptions;

        static JsonSerializer()
        {
            _defaultOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = null,
                WriteIndented = false,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                Converters = { new JsonStringEnumConverter() }
            };

            _indentedOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = null,
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                Converters = { new JsonStringEnumConverter() }
            };

            _camelCaseOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                Converters = { new JsonStringEnumConverter() }
            };
        }

        /// <summary>
        /// 序列化对象为JSON字符串
        /// </summary>
        public static string Serialize<T>(T value, bool indented = false)
        {
            return System.Text.Json.JsonSerializer.Serialize(value, indented ? _indentedOptions : _defaultOptions);
        }

        /// <summary>
        /// 序列化对象为JSON字符串（驼峰命名）
        /// </summary>
        public static string SerializeCamelCase<T>(T value)
        {
            return System.Text.Json.JsonSerializer.Serialize(value, _camelCaseOptions);
        }

        /// <summary>
        /// 反序列化JSON字符串为对象
        /// </summary>
        public static T? Deserialize<T>(string json)
        {
            return System.Text.Json.JsonSerializer.Deserialize<T>(json, _defaultOptions);
        }

        /// <summary>
        /// 反序列化JSON字符串为对象（驼峰命名）
        /// </summary>
        public static T? DeserializeCamelCase<T>(string json)
        {
            return System.Text.Json.JsonSerializer.Deserialize<T>(json, _camelCaseOptions);
        }

        /// <summary>
        /// 序列化到文件
        /// </summary>
        public static void SerializeToFile<T>(T value, string filePath, bool indented = true)
        {
            var directory = System.IO.Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !System.IO.Directory.Exists(directory))
                System.IO.Directory.CreateDirectory(directory);

            var json = Serialize(value, indented);
            System.IO.File.WriteAllText(filePath, json);
        }

        /// <summary>
        /// 从文件反序列化
        /// </summary>
        public static T? DeserializeFromFile<T>(string filePath)
        {
            if (!System.IO.File.Exists(filePath))
                throw new System.IO.FileNotFoundException("文件不存在", filePath);

            var json = System.IO.File.ReadAllText(filePath);
            return Deserialize<T>(json);
        }

        /// <summary>
        /// 异步序列化到文件
        /// </summary>
        public static async System.Threading.Tasks.Task SerializeToFileAsync<T>(T value, string filePath, bool indented = true)
        {
            var directory = System.IO.Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !System.IO.Directory.Exists(directory))
                System.IO.Directory.CreateDirectory(directory);

            var json = Serialize(value, indented);
            await System.IO.File.WriteAllTextAsync(filePath, json).ConfigureAwait(false);
        }

        /// <summary>
        /// 异步从文件反序列化
        /// </summary>
        public static async System.Threading.Tasks.Task<T?> DeserializeFromFileAsync<T>(string filePath)
        {
            if (!System.IO.File.Exists(filePath))
                throw new System.IO.FileNotFoundException("文件不存在", filePath);

            var json = await System.IO.File.ReadAllTextAsync(filePath).ConfigureAwait(false);
            return Deserialize<T>(json);
        }

        /// <summary>
        /// 尝试反序列化
        /// </summary>
        public static bool TryDeserialize<T>(string json, out T? result)
        {
            try
            {
                result = Deserialize<T>(json);
                return true;
            }
            catch
            {
                result = default;
                return false;
            }
        }

        /// <summary>
        /// 验证JSON格式
        /// </summary>
        public static bool IsValidJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return false;

            try
            {
                using var doc = JsonDocument.Parse(json);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 格式化JSON
        /// </summary>
        public static string Format(string json)
        {
            using var doc = JsonDocument.Parse(json);
            return System.Text.Json.JsonSerializer.Serialize(doc.RootElement, _indentedOptions);
        }

        /// <summary>
        /// 压缩JSON
        /// </summary>
        public static string Minify(string json)
        {
            using var doc = JsonDocument.Parse(json);
            return System.Text.Json.JsonSerializer.Serialize(doc.RootElement, _defaultOptions);
        }

        /// <summary>
        /// 合并两个JSON对象
        /// </summary>
        public static string Merge(string json1, string json2)
        {
            var dict1 = Deserialize<Dictionary<string, object?>>(json1);
            var dict2 = Deserialize<Dictionary<string, object?>>(json2);

            if (dict1 == null) return json2;
            if (dict2 == null) return json1;

            foreach (var kvp in dict2)
            {
                dict1[kvp.Key] = kvp.Value;
            }

            return Serialize(dict1);
        }

        /// <summary>
        /// 获取JSON值（通过路径）
        /// </summary>
        public static string? GetValue(string json, string path)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                var current = doc.RootElement;

                var parts = path.Split('.');
                foreach (var part in parts)
                {
                    if (current.TryGetProperty(part, out var property))
                    {
                        current = property;
                    }
                    else
                    {
                        return null;
                    }
                }

                return current.ValueKind == JsonValueKind.String
                    ? current.GetString()
                    : current.ToString();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 设置JSON值（通过路径）
        /// </summary>
        public static string SetValue(string json, string path, object value)
        {
            var dict = Deserialize<Dictionary<string, object?>>(json) ?? new Dictionary<string, object?>();

            var parts = path.Split('.');
            var current = dict;

            for (int i = 0; i < parts.Length - 1; i++)
            {
                var part = parts[i];
                if (!current.ContainsKey(part))
                {
                    current[part] = new Dictionary<string, object?>();
                }

                current = (Dictionary<string, object?>)current[part];
            }

            current[parts[^1]] = value;

            return Serialize(dict);
        }

        /// <summary>
        /// 获取JSON的所有键
        /// </summary>
        public static List<string> GetKeys(string json)
        {
            var keys = new List<string>();

            try
            {
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.ValueKind == JsonValueKind.Object)
                {
                    foreach (var property in doc.RootElement.EnumerateObject())
                    {
                        keys.Add(property.Name);
                    }
                }
            }
            catch
            {
            }

            return keys;
        }

        /// <summary>
        /// 深拷贝对象
        /// </summary>
        public static T? DeepClone<T>(T obj)
        {
            var json = Serialize(obj);
            return Deserialize<T>(json);
        }

        /// <summary>
        /// 转换类型
        /// </summary>
        public static TTo? Convert<TFrom, TTo>(TFrom from)
        {
            var json = Serialize(from);
            return Deserialize<TTo>(json);
        }

        /// <summary>
        /// 获取自定义选项
        /// </summary>
        public static JsonSerializerOptions GetOptions(bool indented = false, bool camelCase = false)
        {
            if (camelCase)
                return new JsonSerializerOptions(_camelCaseOptions) { WriteIndented = indented };
            return new JsonSerializerOptions(_defaultOptions) { WriteIndented = indented };
        }
    }
}
