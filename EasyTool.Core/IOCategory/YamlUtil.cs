using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace EasyTool.IOCategory
{
    /// <summary>
    /// YAML 工具类
    /// 提供简单的 YAML 序列化和反序列化功能
    /// </summary>
    public static class YamlUtil
    {
        /// <summary>
        /// 将对象序列化为 YAML 字符串
        /// </summary>
        public static string Serialize(object obj, int indentSize = 2)
        {
            var serializer = new YamlSerializer(indentSize);
            return serializer.Serialize(obj);
        }

        /// <summary>
        /// 将 YAML 字符串反序列化为字典
        /// </summary>
        public static Dictionary<string, object> Deserialize(string yaml)
        {
            var deserializer = new YamlDeserializer();
            return deserializer.Deserialize(yaml);
        }

        /// <summary>
        /// 从文件读取 YAML
        /// </summary>
        public static Dictionary<string, object> ReadFile(string filePath)
        {
            var content = File.ReadAllText(filePath, Encoding.UTF8);
            return Deserialize(content);
        }

        /// <summary>
        /// 将对象写入 YAML 文件
        /// </summary>
        public static void WriteFile(string filePath, object obj, int indentSize = 2)
        {
            var yaml = Serialize(obj, indentSize);
            File.WriteAllText(filePath, yaml, Encoding.UTF8);
        }

        /// <summary>
        /// 将 YAML 字符串反序列化为指定类型
        /// </summary>
        public static T Deserialize<T>(string yaml) where T : new()
        {
            var dict = Deserialize(yaml);
            return MapToObject<T>(dict);
        }

        private static T MapToObject<T>(Dictionary<string, object> dict) where T : new()
        {
            var obj = new T();
            var type = typeof(T);

            foreach (var kvp in dict)
            {
                var property = type.GetProperty(kvp.Key);
                if (property != null && property.CanWrite)
                {
                    var value = ConvertValue(kvp.Value, property.PropertyType);
                    if (value != null)
                        property.SetValue(obj, value);
                }
            }

            return obj;
        }

        private static object ConvertValue(object value, Type targetType)
        {
            if (value == null) return null;

            if (targetType == typeof(string))
                return value.ToString();

            if (targetType == typeof(int))
                return Convert.ToInt32(value);

            if (targetType == typeof(long))
                return Convert.ToInt64(value);

            if (targetType == typeof(double))
                return Convert.ToDouble(value);

            if (targetType == typeof(bool))
                return Convert.ToBoolean(value);

            if (targetType == typeof(DateTime))
                return Convert.ToDateTime(value);

            return value;
        }
    }

    /// <summary>
    /// YAML 序列化器
    /// </summary>
    public class YamlSerializer
    {
        private readonly int _indentSize;
        private readonly StringBuilder _sb;

        /// <summary>
        /// 创建 YAML 序列化器
        /// </summary>
        public YamlSerializer(int indentSize = 2)
        {
            _indentSize = indentSize;
            _sb = new StringBuilder();
        }

        /// <summary>
        /// 序列化对象
        /// </summary>
        public string Serialize(object obj)
        {
            _sb.Clear();
            SerializeValue(obj, 0, "");
            return _sb.ToString();
        }

        private void SerializeValue(object value, int indent, string key)
        {
            string indentStr = new string(' ', indent);

            if (value == null)
            {
                if (!string.IsNullOrEmpty(key))
                    _sb.AppendLine($"{indentStr}{key}: null");
                return;
            }

            var type = value.GetType();

            if (value is IDictionary<string, object> dict)
            {
                if (!string.IsNullOrEmpty(key))
                    _sb.AppendLine($"{indentStr}{key}:");
                else if (indent > 0)
                    _sb.AppendLine($"{indentStr}:");

                foreach (var kvp in dict)
                {
                    SerializeValue(kvp.Value, indent + _indentSize, kvp.Key);
                }
            }
            else if (value is IList<object> list)
            {
                if (!string.IsNullOrEmpty(key))
                    _sb.AppendLine($"{indentStr}{key}:");
                else if (indent > 0)
                    _sb.AppendLine($"{indentStr}:");

                foreach (var item in list)
                {
                    SerializeValue(item, indent + _indentSize, "-");
                }
            }
            else if (type.IsPrimitive || value is string || value is DateTime || value is decimal)
            {
                string valueStr = FormatScalar(value);
                if (!string.IsNullOrEmpty(key))
                {
                    if (key == "-")
                        _sb.AppendLine($"{indentStr}- {valueStr}");
                    else
                        _sb.AppendLine($"{indentStr}{key}: {valueStr}");
                }
            }
            else
            {
                // 复杂对象，反射属性
                if (!string.IsNullOrEmpty(key))
                    _sb.AppendLine($"{indentStr}{key}:");

                var properties = type.GetProperties();
                foreach (var prop in properties)
                {
                    if (prop.CanRead)
                    {
                        var propValue = prop.GetValue(value);
                        SerializeValue(propValue, indent + _indentSize, prop.Name);
                    }
                }
            }
        }

        private static string FormatScalar(object value)
        {
            if (value == null) return "null";
            if (value is string str)
            {
                if (string.IsNullOrEmpty(str)) return "\"\"";
                if (str.Contains(":") || str.Contains("#") || str.Contains("\n") || str.StartsWith(" ") || str.EndsWith(" "))
                    return $"\"{str.Replace("\"", "\\\"")}\"";
                return str;
            }
            if (value is bool b) return b ? "true" : "false";
            if (value is DateTime dt) return dt.ToString("yyyy-MM-dd HH:mm:ss");

            return value.ToString();
        }
    }

    /// <summary>
    /// YAML 反序列化器
    /// </summary>
    public class YamlDeserializer
    {
        /// <summary>
        /// 反序列化 YAML 字符串
        /// </summary>
        public Dictionary<string, object> Deserialize(string yaml)
        {
            var lines = yaml.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            var result = new Dictionary<string, object>();
            var context = new ParseContext { Lines = lines, Index = 0 };
            int currentIndent = 0;

            ParseBlock(context, result, currentIndent);

            return result;
        }

        private class ParseContext
        {
            public string[] Lines { get; set; }
            public int Index { get; set; }
            public int LineCount => Lines.Length;
            public string CurrentLine => Index < LineCount ? Lines[Index] : null;
        }

        private void ParseBlock(ParseContext context, Dictionary<string, object> result, int baseIndent)
        {
            while (context.Index < context.LineCount)
            {
                string line = context.CurrentLine;

                // 跳过空行和注释
                if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("#"))
                {
                    context.Index++;
                    continue;
                }

                int indent = GetIndent(line);
                if (indent < baseIndent) break;

                string trimmed = line.TrimStart();

                // 列表项
                if (trimmed.StartsWith("- "))
                {
                    var list = new List<object>();
                    while (context.Index < context.LineCount)
                    {
                        string itemLine = context.CurrentLine;
                        if (string.IsNullOrWhiteSpace(itemLine) || itemLine.TrimStart().StartsWith("#"))
                        {
                            context.Index++;
                            continue;
                        }

                        int itemIndent = GetIndent(itemLine);
                        if (itemIndent < indent) break;
                        if (itemIndent > indent)
                        {
                            // 嵌套块
                            context.Index--;
                            break;
                        }

                        string itemTrimmed = itemLine.TrimStart();
                        if (!itemTrimmed.StartsWith("- ")) break;

                        string itemContent = itemTrimmed.Substring(2).Trim();
                        if (itemContent.Contains(":"))
                        {
                            // 列表项是字典
                            var itemDict = new Dictionary<string, object>();
                            context.Index++;
                            ParseBlock(context, itemDict, context.Index < context.LineCount ? GetIndent(context.CurrentLine) : indent + 2);
                            list.Add(itemDict);
                        }
                        else
                        {
                            list.Add(ParseScalar(itemContent));
                            context.Index++;
                        }
                    }

                    result["__list__"] = list;
                    continue;
                }

                // 键值对
                int colonIndex = trimmed.IndexOf(':');
                if (colonIndex > 0)
                {
                    string key = trimmed.Substring(0, colonIndex).Trim();
                    string valueStr = trimmed.Substring(colonIndex + 1).Trim();

                    if (string.IsNullOrEmpty(valueStr))
                    {
                        // 嵌套块
                        context.Index++;
                        var nested = new Dictionary<string, object>();
                        ParseBlock(context, nested, indent + 2);
                        result[key] = nested;
                    }
                    else
                    {
                        result[key] = ParseScalar(valueStr);
                        context.Index++;
                    }
                }
                else
                {
                    context.Index++;
                }
            }
        }

        private static int GetIndent(string line)
        {
            int indent = 0;
            foreach (char c in line)
            {
                if (c == ' ') indent++;
                else if (c == '\t') indent += 2;
                else break;
            }
            return indent;
        }

        private static object ParseScalar(string value)
        {
            if (string.IsNullOrEmpty(value)) return null;
            if (value == "null" || value == "~") return null;
            if (value == "true") return true;
            if (value == "false") return false;

            // 移除引号
            if ((value.StartsWith("\"") && value.EndsWith("\"")) ||
                (value.StartsWith("'") && value.EndsWith("'")))
            {
                return value.Substring(1, value.Length - 2);
            }

            // 尝试解析数字
            if (int.TryParse(value, out int intVal)) return intVal;
            if (long.TryParse(value, out long longVal)) return longVal;
            if (double.TryParse(value, out double doubleVal)) return doubleVal;
            if (DateTime.TryParse(value, out DateTime dateVal)) return dateVal;

            return value;
        }
    }
}
