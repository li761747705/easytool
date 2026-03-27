using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace EasyTool.ConvertCategory
{
    /// <summary>
    /// TOML 转换工具类（轻量级实现，无需第三方库）
    /// 支持基本的 TOML 序列化和反序列化
    /// </summary>
    public static class TomlConvertUtil
    {
        #region 序列化

        /// <summary>
        /// 将对象序列化为 TOML 字符串
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="obj">要序列化的对象</param>
        /// <returns>TOML 字符串</returns>
        public static string Serialize<T>(T obj)
        {
            var builder = new StringBuilder();
            SerializeObject(obj, builder, "");
            return builder.ToString();
        }

        /// <summary>
        /// 将字典序列化为 TOML 字符串
        /// </summary>
        /// <param name="dict">要序列化的字典</param>
        /// <returns>TOML 字符串</returns>
        public static string SerializeDictionary(IDictionary dict)
        {
            var builder = new StringBuilder();
            SerializeDictionary(dict, builder, "");
            return builder.ToString();
        }

        private static void SerializeObject(object? obj, StringBuilder builder, string prefix)
        {
            if (obj == null)
                return;

            var type = obj.GetType();
            var properties = type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            // 先序列化简单属性
            foreach (var prop in properties)
            {
                if (!prop.CanRead)
                    continue;

                var value = prop.GetValue(obj);
                var propType = prop.PropertyType;

                if (IsSimpleType(propType))
                {
                    builder.Append(prop.Name);
                    builder.Append(" = ");
                    SerializeValue(value, builder);
                    builder.AppendLine();
                }
            }

            // 序列化数组和列表
            foreach (var prop in properties)
            {
                if (!prop.CanRead)
                    continue;

                var value = prop.GetValue(obj);
                var propType = prop.PropertyType;

                if (IsArrayType(propType) && value is IEnumerable enumerable and not string)
                {
                    builder.AppendLine();
                    SerializeArray(enumerable, builder, prop.Name);
                }
            }

            // 序列化嵌套表
            foreach (var prop in properties)
            {
                if (!prop.CanRead)
                    continue;

                var value = prop.GetValue(obj);
                var propType = prop.PropertyType;

                if (!IsSimpleType(propType) && !IsArrayType(propType) && value != null)
                {
                    builder.AppendLine();
                    var tablePrefix = string.IsNullOrEmpty(prefix) ? prop.Name : $"{prefix}.{prop.Name}";
                    builder.AppendLine($"[{tablePrefix}]");
                    SerializeObject(value, builder, tablePrefix);
                }
            }
        }

        private static void SerializeDictionary(IDictionary dict, StringBuilder builder, string prefix)
        {
            // 先序列化简单值
            foreach (DictionaryEntry entry in dict)
            {
                var key = entry.Key?.ToString() ?? "";
                var value = entry.Value;

                if (value == null || IsSimpleType(value.GetType()))
                {
                    builder.Append(key);
                    builder.Append(" = ");
                    SerializeValue(value, builder);
                    builder.AppendLine();
                }
            }

            // 序列化数组
            foreach (DictionaryEntry entry in dict)
            {
                var key = entry.Key?.ToString() ?? "";
                var value = entry.Value;

                if (value is IEnumerable enumerable and not string and not IDictionary)
                {
                    builder.AppendLine();
                    SerializeArray(enumerable, builder, key);
                }
            }

            // 序列化嵌套字典
            foreach (DictionaryEntry entry in dict)
            {
                var key = entry.Key?.ToString() ?? "";
                var value = entry.Value;

                if (value is IDictionary nestedDict)
                {
                    builder.AppendLine();
                    var tablePrefix = string.IsNullOrEmpty(prefix) ? key : $"{prefix}.{key}";
                    builder.AppendLine($"[{tablePrefix}]");
                    SerializeDictionary(nestedDict, builder, tablePrefix);
                }
                else if (value != null && !IsSimpleType(value.GetType()) && !IsArrayType(value.GetType()))
                {
                    builder.AppendLine();
                    var tablePrefix = string.IsNullOrEmpty(prefix) ? key : $"{prefix}.{key}";
                    builder.AppendLine($"[{tablePrefix}]");
                    SerializeObject(value, builder, tablePrefix);
                }
            }
        }

        private static void SerializeValue(object? value, StringBuilder builder)
        {
            if (value == null)
            {
                builder.Append("\"\"");
                return;
            }

            var type = value.GetType();

            if (type == typeof(bool))
            {
                builder.Append((bool)value ? "true" : "false");
            }
            else if (type == typeof(string))
            {
                var str = (string)value;
                if (str.Contains('\n') || str.Contains('\t') || str.Contains('"') || str.Contains('#'))
                {
                    // 多行字符串使用字面量字符串
                    builder.Append("'''");
                    builder.Append(str);
                    builder.Append("'''");
                }
                else
                {
                    builder.Append($"\"{EscapeString(str)}\"");
                }
            }
            else if (type == typeof(DateTime))
            {
                builder.Append(((DateTime)value).ToString("o"));
            }
            else if (type == typeof(DateTimeOffset))
            {
                builder.Append(((DateTimeOffset)value).ToString("o"));
            }
            else if (type == typeof(Guid))
            {
                builder.Append($"\"{value}\"");
            }
            else if (type == typeof(decimal))
            {
                builder.Append(((decimal)value).ToString(System.Globalization.CultureInfo.InvariantCulture));
            }
            else if (type == typeof(float) || type == typeof(double))
            {
                builder.Append(Convert.ToDouble(value).ToString(System.Globalization.CultureInfo.InvariantCulture));
            }
            else
            {
                builder.Append(Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture));
            }
        }

        private static void SerializeArray(IEnumerable enumerable, StringBuilder builder, string key)
        {
            foreach (var item in enumerable)
            {
                builder.Append(key);
                builder.Append(" = [");

                if (item == null)
                {
                    builder.Append("]");
                }
                else if (IsSimpleType(item.GetType()))
                {
                    SerializeValue(item, builder);
                    builder.Append("]");
                }
                else if (item is IDictionary dict)
                {
                    builder.AppendLine();
                    SerializeInlineTable(dict, builder);
                    builder.AppendLine();
                    builder.Append("]");
                }
                else
                {
                    builder.AppendLine();
                    SerializeInlineObject(item, builder);
                    builder.AppendLine();
                    builder.Append("]");
                }

                builder.AppendLine();
            }
        }

        private static void SerializeInlineTable(IDictionary dict, StringBuilder builder)
        {
            builder.Append("{ ");
            var first = true;
            foreach (DictionaryEntry entry in dict)
            {
                if (!first)
                    builder.Append(", ");
                first = false;

                builder.Append(entry.Key?.ToString() ?? "");
                builder.Append(" = ");
                SerializeValue(entry.Value, builder);
            }
            builder.Append(" }");
        }

        private static void SerializeInlineObject(object obj, StringBuilder builder)
        {
            var type = obj.GetType();
            var properties = type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            builder.Append("{ ");
            var first = true;
            foreach (var prop in properties)
            {
                if (!prop.CanRead)
                    continue;

                if (!first)
                    builder.Append(", ");
                first = false;

                builder.Append(prop.Name);
                builder.Append(" = ");
                SerializeValue(prop.GetValue(obj), builder);
            }
            builder.Append(" }");
        }

        private static string EscapeString(string value)
        {
            return value.Replace("\\", "\\\\")
                       .Replace("\"", "\\\"")
                       .Replace("\b", "\\b")
                       .Replace("\f", "\\f")
                       .Replace("\n", "\\n")
                       .Replace("\r", "\\r")
                       .Replace("\t", "\\t");
        }

        private static bool IsSimpleType(Type type)
        {
            return type.IsPrimitive ||
                   type == typeof(string) ||
                   type == typeof(decimal) ||
                   type == typeof(DateTime) ||
                   type == typeof(DateTimeOffset) ||
                   type == typeof(Guid) ||
                   type == typeof(TimeSpan);
        }

        private static bool IsArrayType(Type type)
        {
            return (type.IsArray || type.GetInterfaces().Contains(typeof(IList))) && type != typeof(string);
        }

        #endregion

        #region 反序列化

        /// <summary>
        /// 将 TOML 字符串反序列化为字典
        /// </summary>
        /// <param name="toml">TOML 字符串</param>
        /// <returns>字典对象</returns>
        public static Dictionary<string, object?> Deserialize(string toml)
        {
            var result = new Dictionary<string, object?>();
            var currentTable = result;
            var tables = new Stack<Dictionary<string, object?>>();
            tables.Push(result);

            using var reader = new StringReader(toml);
            string? line;

            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim();

                // 跳过空行和注释
                if (string.IsNullOrEmpty(line) || line.StartsWith("#"))
                    continue;

                // 表头 [table] 或 [table.subtable]
                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    var tableName = line[1..^1].Trim();
                    currentTable = GetOrCreateTable(result, tableName);
                    continue;
                }

                // 数组表 [[array]]
                if (line.StartsWith("[[") && line.EndsWith("]]"))
                {
                    var arrayName = line[2..^2].Trim();
                    AddArrayTable(result, arrayName);
                    continue;
                }

                // 键值对
                var equalsIndex = line.IndexOf('=');
                if (equalsIndex > 0)
                {
                    var key = line[..equalsIndex].Trim();
                    var value = line[(equalsIndex + 1)..].Trim();
                    currentTable[key] = ParseValue(value, reader);
                }
            }

            return result;
        }

        /// <summary>
        /// 将 TOML 字符串反序列化为指定类型
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="toml">TOML 字符串</param>
        /// <returns>反序列化的对象</returns>
        public static T? Deserialize<T>(string toml) where T : class, new()
        {
            var dict = Deserialize(toml);
            return MapToObject<T>(dict);
        }

        /// <summary>
        /// 从文件加载 TOML 并反序列化为字典
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>字典对象</returns>
        public static Dictionary<string, object?> LoadFromFile(string filePath)
        {
            var toml = File.ReadAllText(filePath);
            return Deserialize(toml);
        }

        /// <summary>
        /// 将字典保存为 TOML 文件
        /// </summary>
        /// <param name="dict">字典对象</param>
        /// <param name="filePath">文件路径</param>
        public static void SaveToFile(Dictionary<string, object?> dict, string filePath)
        {
            var toml = SerializeDictionary(dict);
            File.WriteAllText(filePath, toml);
        }

        private static Dictionary<string, object?> GetOrCreateTable(Dictionary<string, object?> root, string path)
        {
            var parts = path.Split('.');
            var current = root;

            foreach (var part in parts)
            {
                if (!current.TryGetValue(part, out var value) || !(value is Dictionary<string, object?> nested))
                {
                    nested = new Dictionary<string, object?>();
                    current[part] = nested;
                }
                current = nested;
            }

            return current;
        }

        private static void AddArrayTable(Dictionary<string, object?> root, string path)
        {
            var parts = path.Split('.');
            var current = root;

            for (int i = 0; i < parts.Length - 1; i++)
            {
                if (!current.TryGetValue(parts[i], out var value) || !(value is Dictionary<string, object?> nested))
                {
                    nested = new Dictionary<string, object?>();
                    current[parts[i]] = nested;
                }
                current = nested;
            }

            var lastPart = parts[^1];
            if (!current.TryGetValue(lastPart, out var arrayValue) || !(arrayValue is List<Dictionary<string, object?>> array))
            {
                array = new List<Dictionary<string, object?>>();
                current[lastPart] = array;
            }

            var newTable = new Dictionary<string, object?>();
            array.Add(newTable);
        }

        private static object? ParseValue(string value, StringReader reader)
        {
            value = value.Trim();

            // 字符串
            if (value.StartsWith("\"") && value.EndsWith("\""))
            {
                return UnescapeString(value[1..^1]);
            }
            if (value.StartsWith("'") && value.EndsWith("'"))
            {
                return value[1..^1];
            }
            if (value.StartsWith("'''") || value.StartsWith("\"\"\""))
            {
                return ParseMultiLineString(value, reader);
            }

            // 布尔值
            if (value.Equals("true", StringComparison.OrdinalIgnoreCase))
                return true;
            if (value.Equals("false", StringComparison.OrdinalIgnoreCase))
                return false;

            // 数组
            if (value.StartsWith("[") && value.EndsWith("]"))
            {
                return ParseArray(value[1..^1]);
            }

            // 内联表
            if (value.StartsWith("{") && value.EndsWith("}"))
            {
                return ParseInlineTable(value[1..^1]);
            }

            // 数字
            if (int.TryParse(value, out var intVal))
                return intVal;
            if (long.TryParse(value, out var longVal))
                return longVal;
            if (double.TryParse(value, out var doubleVal))
                return doubleVal;

            // 日期时间
            if (DateTime.TryParse(value, out var dateVal))
                return dateVal;

            return value;
        }

        private static string ParseMultiLineString(string start, StringReader reader)
        {
            var delimiter = start.Substring(0, 3);
            var sb = new StringBuilder();

            // 处理开始行的剩余内容
            if (start.Length > 3)
            {
                sb.Append(start[3..]);
            }

            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                if (line.Contains(delimiter))
                {
                    var endIndex = line.IndexOf(delimiter);
                    sb.Append(line[..endIndex]);
                    break;
                }
                sb.AppendLine(line);
            }

            return sb.ToString();
        }

        private static List<object?> ParseArray(string content)
        {
            var result = new List<object?>();
            var items = SplitArrayItems(content);

            foreach (var item in items)
            {
                result.Add(ParseValue(item.Trim(), null!));
            }

            return result;
        }

        private static Dictionary<string, object?> ParseInlineTable(string content)
        {
            var result = new Dictionary<string, object?>();
            var pairs = SplitKeyValuePairs(content);

            foreach (var pair in pairs)
            {
                var equalsIndex = pair.IndexOf('=');
                if (equalsIndex > 0)
                {
                    var key = pair[..equalsIndex].Trim();
                    var value = pair[(equalsIndex + 1)..].Trim();
                    result[key] = ParseValue(value, null!);
                }
            }

            return result;
        }

        private static List<string> SplitArrayItems(string content)
        {
            var items = new List<string>();
            var current = new StringBuilder();
            var depth = 0;
            var inString = false;
            var stringChar = '\0';

            foreach (var c in content)
            {
                if (inString)
                {
                    current.Append(c);
                    if (c == stringChar)
                        inString = false;
                }
                else if (c == '"' || c == '\'')
                {
                    inString = true;
                    stringChar = c;
                    current.Append(c);
                }
                else if (c == '[' || c == '{')
                {
                    depth++;
                    current.Append(c);
                }
                else if (c == ']' || c == '}')
                {
                    depth--;
                    current.Append(c);
                }
                else if (c == ',' && depth == 0)
                {
                    items.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }

            if (current.Length > 0)
                items.Add(current.ToString());

            return items;
        }

        private static List<string> SplitKeyValuePairs(string content)
        {
            var pairs = new List<string>();
            var current = new StringBuilder();
            var depth = 0;
            var inString = false;
            var stringChar = '\0';

            foreach (var c in content)
            {
                if (inString)
                {
                    current.Append(c);
                    if (c == stringChar)
                        inString = false;
                }
                else if (c == '"' || c == '\'')
                {
                    inString = true;
                    stringChar = c;
                    current.Append(c);
                }
                else if (c == '[' || c == '{')
                {
                    depth++;
                    current.Append(c);
                }
                else if (c == ']' || c == '}')
                {
                    depth--;
                    current.Append(c);
                }
                else if (c == ',' && depth == 0)
                {
                    pairs.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }

            if (current.Length > 0)
                pairs.Add(current.ToString());

            return pairs;
        }

        private static string UnescapeString(string value)
        {
            return value.Replace("\\b", "\b")
                       .Replace("\\f", "\f")
                       .Replace("\\n", "\n")
                       .Replace("\\r", "\r")
                       .Replace("\\t", "\t")
                       .Replace("\\\"", "\"")
                       .Replace("\\\\", "\\");
        }

        private static T? MapToObject<T>(Dictionary<string, object?> dict) where T : class, new()
        {
            if (dict == null)
                return null;

            var obj = new T();
            var type = typeof(T);

            foreach (var kvp in dict)
            {
                var prop = type.GetProperty(kvp.Key,
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.IgnoreCase);

                if (prop != null && prop.CanWrite)
                {
                    var value = ConvertValue(kvp.Value, prop.PropertyType);
                    prop.SetValue(obj, value);
                }
            }

            return obj;
        }

        private static object? ConvertValue(object? value, Type targetType)
        {
            if (value == null)
                return null;

            var sourceType = value.GetType();

            if (targetType.IsAssignableFrom(sourceType))
                return value;

            if (value is Dictionary<string, object?> dict && !targetType.IsPrimitive)
            {
                var method = typeof(TomlConvertUtil).GetMethod(nameof(MapToObject),
                    System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
                    ?.MakeGenericMethod(targetType);
                return method?.Invoke(null, new object[] { dict });
            }

            return Convert.ChangeType(value, targetType);
        }

        #endregion
    }
}
