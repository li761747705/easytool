using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace EasyTool.IOCategory
{
    /// <summary>
    /// TOML 工具类
    /// 提供 TOML 配置文件的读写功能
    /// </summary>
    public static class TomlUtil
    {
        /// <summary>
        /// 将对象序列化为 TOML 字符串
        /// </summary>
        public static string Serialize(object obj)
        {
            var serializer = new TomlSerializer();
            return serializer.Serialize(obj);
        }

        /// <summary>
        /// 将 TOML 字符串反序列化为字典
        /// </summary>
        public static Dictionary<string, object> Deserialize(string toml)
        {
            var deserializer = new TomlDeserializer();
            return deserializer.Deserialize(toml);
        }

        /// <summary>
        /// 从文件读取 TOML
        /// </summary>
        public static Dictionary<string, object> ReadFile(string filePath)
        {
            var content = File.ReadAllText(filePath, Encoding.UTF8);
            return Deserialize(content);
        }

        /// <summary>
        /// 将对象写入 TOML 文件
        /// </summary>
        public static void WriteFile(string filePath, object obj)
        {
            var toml = Serialize(obj);
            File.WriteAllText(filePath, toml, Encoding.UTF8);
        }
    }

    /// <summary>
    /// TOML 序列化器
    /// </summary>
    public class TomlSerializer
    {
        private readonly StringBuilder _sb;

        /// <summary>
        /// 创建 TOML 序列化器
        /// </summary>
        public TomlSerializer()
        {
            _sb = new StringBuilder();
        }

        /// <summary>
        /// 序列化对象
        /// </summary>
        public string Serialize(object obj)
        {
            _sb.Clear();
            SerializeValue(obj, "");
            return _sb.ToString();
        }

        private void SerializeValue(object value, string prefix)
        {
            if (value == null)
                return;

            var type = value.GetType();

            if (value is IDictionary<string, object> dict)
            {
                SerializeDictionary(new Dictionary<string, object>(dict), prefix);
            }
            else if (value is IList<object> list)
            {
                SerializeArray(new List<object>(list), prefix);
            }
            else if (type.IsPrimitive || value is string || value is decimal || value is DateTime)
            {
                // 简单值不单独序列化
            }
            else
            {
                // 复杂对象，反射属性
                var props = type.GetProperties();
                var objDict = new Dictionary<string, object>();
                foreach (var prop in props)
                {
                    if (prop.CanRead)
                    {
                        objDict[prop.Name] = prop.GetValue(value);
                    }
                }
                SerializeDictionary(objDict, prefix);
            }
        }

        private void SerializeDictionary(Dictionary<string, object> dict, string prefix)
        {
            var simpleValues = new List<KeyValuePair<string, object>>();
            var complexValues = new List<KeyValuePair<string, object>>();

            foreach (var kvp in dict)
            {
                if (IsSimpleValue(kvp.Value))
                    simpleValues.Add(kvp);
                else
                    complexValues.Add(kvp);
            }

            // 先输出简单值
            if (!string.IsNullOrEmpty(prefix) && simpleValues.Count > 0)
            {
                _sb.AppendLine($"[{prefix}]");
            }

            foreach (var kvp in simpleValues)
            {
                _sb.AppendLine($"{kvp.Key} = {FormatValue(kvp.Value)}");
            }

            if (simpleValues.Count > 0 && complexValues.Count > 0)
                _sb.AppendLine();

            // 处理复杂值
            foreach (var kvp in complexValues)
            {
                string newPrefix = string.IsNullOrEmpty(prefix) ? kvp.Key : $"{prefix}.{kvp.Key}";
                SerializeValue(kvp.Value, newPrefix);
            }
        }

        private void SerializeArray(List<object> list, string prefix)
        {
            if (!string.IsNullOrEmpty(prefix))
            {
                _sb.AppendLine($"[[{prefix}]]");
            }

            foreach (var item in list)
            {
                if (IsSimpleValue(item))
                {
                    _sb.AppendLine(FormatValue(item));
                }
                else if (item is Dictionary<string, object> dict)
                {
                    foreach (var kvp in dict)
                    {
                        _sb.AppendLine($"{kvp.Key} = {FormatValue(kvp.Value)}");
                    }
                    _sb.AppendLine();
                }
            }
        }

        private static bool IsSimpleValue(object value)
        {
            if (value == null) return true;
            var type = value.GetType();
            return type.IsPrimitive || value is string || value is decimal || value is DateTime;
        }

        private static string FormatValue(object value)
        {
            if (value == null) return "\"\"";

            if (value is string str)
            {
                if (str.Contains("\n"))
                    return $"\"\"\"\n{str}\n\"\"\"";
                if (str.Contains("\"") || str.Contains("'") || str.Contains("\\"))
                    return $"\"{str.Replace("\\", "\\\\").Replace("\"", "\\\"")}\"";
                return $"\"{str}\"";
            }

            if (value is bool b) return b ? "true" : "false";
            if (value is DateTime dt) return dt.ToString("yyyy-MM-ddTHH:mm:ssZ");
            if (value is double d) return d.ToString(System.Globalization.CultureInfo.InvariantCulture);
            if (value is float f) return f.ToString(System.Globalization.CultureInfo.InvariantCulture);

            return value.ToString();
        }
    }

    /// <summary>
    /// TOML 反序列化器
    /// </summary>
    public class TomlDeserializer
    {
        /// <summary>
        /// 反序列化 TOML 字符串
        /// </summary>
        public Dictionary<string, object> Deserialize(string toml)
        {
            var result = new Dictionary<string, object>();
            var lines = toml.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            string currentSection = "";
            var currentDict = result;

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();

                // 跳过空行和注释
                if (string.IsNullOrEmpty(line) || line.StartsWith("#"))
                    continue;

                // 表头
                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    string sectionName = line.Substring(1, line.Length - 2).Trim();
                    if (sectionName.StartsWith("[[") && sectionName.EndsWith("]]"))
                    {
                        // 数组表
                        sectionName = sectionName.Substring(2, sectionName.Length - 4).Trim();
                        currentSection = sectionName;
                        var list = GetOrCreateArray(result, sectionName);
                        currentDict = new Dictionary<string, object>();
                        list.Add(currentDict);
                    }
                    else
                    {
                        currentSection = sectionName;
                        currentDict = GetOrCreateDictionary(result, sectionName);
                    }
                    continue;
                }

                // 键值对
                int equalIndex = line.IndexOf('=');
                if (equalIndex > 0)
                {
                    string key = line.Substring(0, equalIndex).Trim();
                    string valueStr = line.Substring(equalIndex + 1).Trim();

                    // 处理行内注释
                    int commentIndex = valueStr.IndexOf(" #");
                    if (commentIndex > 0)
                    {
                        valueStr = valueStr.Substring(0, commentIndex).Trim();
                    }

                    object value = ParseValue(valueStr, lines, ref i);
                    currentDict[key] = value;
                }
            }

            return result;
        }

        private static List<Dictionary<string, object>> GetOrCreateArray(Dictionary<string, object> root, string path)
        {
            var parts = path.Split('.');
            var current = root;

            for (int i = 0; i < parts.Length - 1; i++)
            {
                if (!current.TryGetValue(parts[i], out var obj) || !(obj is Dictionary<string, object> dict))
                {
                    dict = new Dictionary<string, object>();
                    current[parts[i]] = dict;
                }
                current = dict;
            }

            string lastKey = parts[parts.Length - 1];
            if (!current.TryGetValue(lastKey, out var listObj) || !(listObj is List<Dictionary<string, object>> list))
            {
                list = new List<Dictionary<string, object>>();
                current[lastKey] = list;
            }

            return list;
        }

        private static Dictionary<string, object> GetOrCreateDictionary(Dictionary<string, object> root, string path)
        {
            var parts = path.Split('.');
            var current = root;

            foreach (var part in parts)
            {
                if (!current.TryGetValue(part, out var obj) || !(obj is Dictionary<string, object> dict))
                {
                    dict = new Dictionary<string, object>();
                    current[part] = dict;
                }
                current = dict;
            }

            return current;
        }

        private static object ParseValue(string valueStr, string[] lines, ref int lineIndex)
        {
            // 布尔值
            if (valueStr == "true") return true;
            if (valueStr == "false") return false;

            // 数字
            if (int.TryParse(valueStr, out int intVal)) return intVal;
            if (long.TryParse(valueStr, out long longVal)) return longVal;
            if (double.TryParse(valueStr, out double doubleVal)) return doubleVal;

            // 字符串
            if (valueStr.StartsWith("\"\"\""))
            {
                // 多行字符串
                var sb = new StringBuilder();
                lineIndex++;
                while (lineIndex < lines.Length && !lines[lineIndex].Trim().EndsWith("\"\"\""))
                {
                    sb.AppendLine(lines[lineIndex]);
                    lineIndex++;
                }
                if (lineIndex < lines.Length)
                {
                    string lastLine = lines[lineIndex].Trim();
                    sb.Append(lastLine.Substring(0, lastLine.Length - 3));
                }
                return sb.ToString();
            }

            if (valueStr.StartsWith("\"") && valueStr.EndsWith("\""))
            {
                return valueStr.Substring(1, valueStr.Length - 2)
                    .Replace("\\\"", "\"")
                    .Replace("\\\\", "\\")
                    .Replace("\\n", "\n")
                    .Replace("\\t", "\t");
            }

            if (valueStr.StartsWith("'") && valueStr.EndsWith("'"))
            {
                return valueStr.Substring(1, valueStr.Length - 2);
            }

            // 日期时间
            if (DateTime.TryParse(valueStr, out DateTime dt)) return dt;

            // 数组
            if (valueStr.StartsWith("[") && valueStr.EndsWith("]"))
            {
                return ParseArray(valueStr);
            }

            // 内联表
            if (valueStr.StartsWith("{") && valueStr.EndsWith("}"))
            {
                return ParseInlineTable(valueStr);
            }

            return valueStr;
        }

        private static List<object> ParseArray(string valueStr)
        {
            var result = new List<object>();
            string inner = valueStr.Substring(1, valueStr.Length - 2).Trim();

            if (string.IsNullOrEmpty(inner))
                return result;

            // 简单分割（不支持嵌套）
            var parts = inner.Split(',');
            foreach (var part in parts)
            {
                string item = part.Trim();
                if (!string.IsNullOrEmpty(item))
                {
                    if (item.StartsWith("\"") && item.EndsWith("\""))
                        result.Add(item.Substring(1, item.Length - 2));
                    else if (int.TryParse(item, out int intVal))
                        result.Add(intVal);
                    else if (double.TryParse(item, out double doubleVal))
                        result.Add(doubleVal);
                    else if (item == "true")
                        result.Add(true);
                    else if (item == "false")
                        result.Add(false);
                    else
                        result.Add(item);
                }
            }

            return result;
        }

        private static Dictionary<string, object> ParseInlineTable(string valueStr)
        {
            var result = new Dictionary<string, object>();
            string inner = valueStr.Substring(1, valueStr.Length - 2).Trim();

            if (string.IsNullOrEmpty(inner))
                return result;

            var parts = inner.Split(',');
            foreach (var part in parts)
            {
                int equalIndex = part.IndexOf('=');
                if (equalIndex > 0)
                {
                    string key = part.Substring(0, equalIndex).Trim();
                    string value = part.Substring(equalIndex + 1).Trim();

                    if (value.StartsWith("\"") && value.EndsWith("\""))
                        result[key] = value.Substring(1, value.Length - 2);
                    else if (int.TryParse(value, out int intVal))
                        result[key] = intVal;
                    else if (value == "true")
                        result[key] = true;
                    else if (value == "false")
                        result[key] = false;
                    else
                        result[key] = value;
                }
            }

            return result;
        }
    }
}
