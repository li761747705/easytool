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
    /// YAML 转换工具类（轻量级实现，无需第三方库）
    /// 支持基本的 YAML 序列化和反序列化
    /// </summary>
    public static class YamlConvertUtil
    {
        private const int DefaultIndent = 2;

        #region 序列化

        /// <summary>
        /// 将对象序列化为 YAML 字符串
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="obj">要序列化的对象</param>
        /// <param name="indent">缩进空格数</param>
        /// <returns>YAML 字符串</returns>
        public static string Serialize<T>(T obj, int indent = DefaultIndent)
        {
            var builder = new StringBuilder();
            SerializeValue(obj, builder, 0, indent);
            return builder.ToString();
        }

        /// <summary>
        /// 将字典序列化为 YAML 字符串
        /// </summary>
        /// <param name="dict">要序列化的字典</param>
        /// <param name="indent">缩进空格数</param>
        /// <returns>YAML 字符串</returns>
        public static string SerializeDictionary(IDictionary dict, int indent = DefaultIndent)
        {
            var builder = new StringBuilder();
            SerializeDictionary(dict, builder, 0, indent);
            return builder.ToString();
        }

        private static void SerializeValue(object? value, StringBuilder builder, int level, int indent)
        {
            if (value == null)
            {
                builder.Append("null");
                return;
            }

            var type = value.GetType();

            if (type.IsPrimitive || value is decimal || value is DateTime || value is DateTimeOffset || value is Guid)
            {
                SerializeScalar(value, builder);
            }
            else if (value is string str)
            {
                SerializeString(str, builder);
            }
            else if (value is IDictionary dict)
            {
                SerializeDictionary(dict, builder, level, indent);
            }
            else if (value is IEnumerable enumerable and not string)
            {
                SerializeEnumerable(enumerable, builder, level, indent);
            }
            else
            {
                SerializeObject(value, builder, level, indent);
            }
        }

        private static void SerializeScalar(object value, StringBuilder builder)
        {
            var type = value.GetType();

            if (type == typeof(bool))
            {
                builder.Append((bool)value ? "true" : "false");
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
                builder.Append(((Guid)value).ToString());
            }
            else
            {
                builder.Append(Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture));
            }
        }

        private static void SerializeString(string value, StringBuilder builder)
        {
            if (string.IsNullOrEmpty(value))
            {
                builder.Append("\"\"");
                return;
            }

            // 检查是否需要引号
            var needsQuotes = value.Contains('\n') ||
                              value.Contains('\t') ||
                              value.Contains(':') ||
                              value.Contains('#') ||
                              value.StartsWith(" ") ||
                              value.EndsWith(" ") ||
                              value.StartsWith("\"") ||
                              value.StartsWith("'") ||
                              IsNumeric(value);

            if (needsQuotes)
            {
                // 多行字符串
                if (value.Contains('\n'))
                {
                    builder.AppendLine("|");
                    var lines = value.Split('\n');
                    foreach (var line in lines)
                    {
                        builder.AppendLine($"  {line}");
                    }
                }
                else
                {
                    builder.Append($"\"{EscapeString(value)}\"");
                }
            }
            else
            {
                builder.Append(value);
            }
        }

        private static string EscapeString(string value)
        {
            return value.Replace("\\", "\\\\")
                       .Replace("\"", "\\\"")
                       .Replace("\n", "\\n")
                       .Replace("\r", "\\r")
                       .Replace("\t", "\\t");
        }

        private static bool IsNumeric(string value)
        {
            return double.TryParse(value, out _);
        }

        private static void SerializeDictionary(IDictionary dict, StringBuilder builder, int level, int indent)
        {
            var first = true;
            foreach (DictionaryEntry entry in dict)
            {
                if (!first)
                {
                    builder.AppendLine();
                }
                first = false;

                builder.Append(new string(' ', level * indent));
                builder.Append(entry.Key?.ToString() ?? "null");
                builder.Append(':');

                if (entry.Value == null)
                {
                    builder.Append(" null");
                }
                else if (entry.Value is IDictionary nestedDict)
                {
                    builder.AppendLine();
                    SerializeDictionary(nestedDict, builder, level + 1, indent);
                }
                else if (entry.Value is IEnumerable enumerable and not string)
                {
                    builder.AppendLine();
                    SerializeEnumerable(enumerable, builder, level + 1, indent);
                }
                else
                {
                    builder.Append(' ');
                    SerializeValue(entry.Value, builder, level + 1, indent);
                }
            }
        }

        private static void SerializeEnumerable(IEnumerable enumerable, StringBuilder builder, int level, int indent)
        {
            foreach (var item in enumerable)
            {
                builder.AppendLine();
                builder.Append(new string(' ', level * indent));
                builder.Append("- ");

                if (item == null)
                {
                    builder.Append("null");
                }
                else if (item is IDictionary nestedDict)
                {
                    builder.AppendLine();
                    SerializeDictionary(nestedDict, builder, level + 1, indent);
                }
                else if (item is IEnumerable nestedEnumerable and not string)
                {
                    builder.AppendLine();
                    SerializeEnumerable(nestedEnumerable, builder, level + 1, indent);
                }
                else
                {
                    SerializeValue(item, builder, level + 1, indent);
                }
            }
        }

        private static void SerializeObject(object obj, StringBuilder builder, int level, int indent)
        {
            var type = obj.GetType();
            var properties = type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            var first = true;
            foreach (var prop in properties)
            {
                if (!prop.CanRead)
                    continue;

                var value = prop.GetValue(obj);
                if (!first)
                {
                    builder.AppendLine();
                }
                first = false;

                builder.Append(new string(' ', level * indent));
                builder.Append(prop.Name);
                builder.Append(':');

                if (value == null)
                {
                    builder.Append(" null");
                }
                else if (value is IDictionary nestedDict)
                {
                    builder.AppendLine();
                    SerializeDictionary(nestedDict, builder, level + 1, indent);
                }
                else if (value is IEnumerable enumerable and not string)
                {
                    builder.AppendLine();
                    SerializeEnumerable(enumerable, builder, level + 1, indent);
                }
                else
                {
                    builder.Append(' ');
                    SerializeValue(value, builder, level + 1, indent);
                }
            }
        }

        #endregion

        #region 反序列化

        /// <summary>
        /// 将 YAML 字符串反序列化为字典
        /// </summary>
        /// <param name="yaml">YAML 字符串</param>
        /// <returns>字典对象</returns>
        public static Dictionary<string, object?> Deserialize(string yaml)
        {
            var reader = new StringReader(yaml);
            return ParseYaml(reader);
        }

        /// <summary>
        /// 将 YAML 字符串反序列化为指定类型
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="yaml">YAML 字符串</param>
        /// <returns>反序列化的对象</returns>
        public static T? Deserialize<T>(string yaml) where T : class, new()
        {
            var dict = Deserialize(yaml);
            return MapToObject<T>(dict);
        }

        /// <summary>
        /// 从文件加载 YAML 并反序列化为字典
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>字典对象</returns>
        public static Dictionary<string, object?> LoadFromFile(string filePath)
        {
            var yaml = File.ReadAllText(filePath);
            return Deserialize(yaml);
        }

        /// <summary>
        /// 将字典保存为 YAML 文件
        /// </summary>
        /// <param name="dict">字典对象</param>
        /// <param name="filePath">文件路径</param>
        public static void SaveToFile(Dictionary<string, object?> dict, string filePath)
        {
            var yaml = SerializeDictionary(dict);
            File.WriteAllText(filePath, yaml);
        }

        private static Dictionary<string, object?> ParseYaml(StringReader reader)
        {
            var result = new Dictionary<string, object?>();
            var lines = new List<string>();

            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                lines.Add(line);
            }

            ParseLines(lines, 0, lines.Count, 0, result);
            return result;
        }

        private static int ParseLines(List<string> lines, int start, int end, int baseIndent, Dictionary<string, object?> result)
        {
            var i = start;

            while (i < end)
            {
                var line = lines[i];

                // 跳过空行和注释
                if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("#"))
                {
                    i++;
                    continue;
                }

                var indent = GetIndent(line);

                // 检查是否是列表项
                if (line.TrimStart().StartsWith("- "))
                {
                    // 解析列表
                    var list = new List<object?>();
                    while (i < end)
                    {
                        var currentLine = lines[i];
                        var currentIndent = GetIndent(currentLine);

                        if (currentIndent < indent)
                            break;

                        if (currentLine.TrimStart().StartsWith("- "))
                        {
                            var value = currentLine.TrimStart()[2..].Trim();
                            if (string.IsNullOrEmpty(value))
                            {
                                // 值在下一行（嵌套对象）
                                i++;
                                var nestedDict = new Dictionary<string, object?>();
                                i = ParseLines(lines, i, end, currentIndent + 2, nestedDict);
                                list.Add(nestedDict);
                            }
                            else
                            {
                                list.Add(ParseValue(value));
                                i++;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    return i;
                }

                // 解析键值对
                var colonIndex = line.IndexOf(':');
                if (colonIndex > 0)
                {
                    var key = line[..colonIndex].Trim();
                    var value = line[(colonIndex + 1)..].Trim();

                    if (string.IsNullOrEmpty(value))
                    {
                        // 值在下一行（嵌套对象）
                        i++;
                        var nestedDict = new Dictionary<string, object?>();
                        i = ParseLines(lines, i, end, indent + 2, nestedDict);
                        result[key] = nestedDict;
                    }
                    else
                    {
                        result[key] = ParseValue(value);
                        i++;
                    }
                }
                else
                {
                    i++;
                }
            }

            return i;
        }

        private static int GetIndent(string line)
        {
            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] != ' ')
                    return i;
            }
            return line.Length;
        }

        private static object? ParseValue(string value)
        {
            if (string.IsNullOrEmpty(value))
                return null;

            value = value.Trim();

            // 处理引号字符串
            if ((value.StartsWith("\"") && value.EndsWith("\"")) ||
                (value.StartsWith("'") && value.EndsWith("'")))
            {
                return value[1..^1];
            }

            // null
            if (value.Equals("null", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("~"))
            {
                return null;
            }

            // boolean
            if (value.Equals("true", StringComparison.OrdinalIgnoreCase))
                return true;
            if (value.Equals("false", StringComparison.OrdinalIgnoreCase))
                return false;

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

            // 处理字典到对象的映射
            if (value is Dictionary<string, object?> dict && !targetType.IsPrimitive)
            {
                var method = typeof(YamlConvertUtil).GetMethod(nameof(MapToObject),
                    System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
                    ?.MakeGenericMethod(targetType);
                return method?.Invoke(null, new object[] { dict });
            }

            // 基本类型转换
            return Convert.ChangeType(value, targetType);
        }

        #endregion
    }
}
