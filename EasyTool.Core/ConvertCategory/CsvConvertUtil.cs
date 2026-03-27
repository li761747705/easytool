using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;

namespace EasyTool.ConvertCategory
{
    /// <summary>
    /// CSV转换工具类
    /// </summary>
    public static class CsvConvertUtil
    {
        /// <summary>
        /// 对象列表转CSV字符串
        /// </summary>
        public static string ToCsv<T>(IEnumerable<T> list, bool includeHeader = true, char separator = ',')
        {
            var properties = typeof(T).GetProperties();
            var sb = new StringBuilder();

            // 添加表头
            if (includeHeader)
            {
                var headers = new List<string>();
                foreach (var prop in properties)
                {
                    headers.Add(EscapeCsvField(prop.Name, separator));
                }
                sb.AppendLine(string.Join(separator, headers));
            }

            // 添加数据行
            foreach (var item in list)
            {
                var values = new List<string>();
                foreach (var prop in properties)
                {
                    var value = prop.GetValue(item)?.ToString() ?? "";
                    values.Add(EscapeCsvField(value, separator));
                }
                sb.AppendLine(string.Join(separator, values));
            }

            return sb.ToString();
        }

        /// <summary>
        /// CSV字符串转对象列表
        /// </summary>
        public static List<T> FromCsv<T>(string csv, bool hasHeader = true, char separator = ',') where T : new()
        {
            var result = new List<T>();
            var properties = typeof(T).GetProperties();
            var lines = csv.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            if (lines.Length == 0)
                return result;

            var startIndex = hasHeader ? 1 : 0;
            var headers = hasHeader ? ParseCsvLine(lines[0], separator) : null;

            // 构建属性映射
            var propMap = new Dictionary<string, int>();
            if (headers != null)
            {
                for (int i = 0; i < headers.Count; i++)
                {
                    var header = headers[i].Trim();
                    foreach (var prop in properties)
                    {
                        if (prop.Name.Equals(header, StringComparison.OrdinalIgnoreCase))
                        {
                            propMap[prop.Name] = i;
                            break;
                        }
                    }
                }
            }

            for (int i = startIndex; i < lines.Length; i++)
            {
                var values = ParseCsvLine(lines[i], separator);
                var item = new T();

                for (int j = 0; j < properties.Length && j < values.Count; j++)
                {
                    var prop = properties[j];
                    var index = headers != null && propMap.TryGetValue(prop.Name, out var mapIndex) ? mapIndex : j;

                    if (index < values.Count)
                    {
                        var value = UnescapeCsvField(values[index]);
                        if (!string.IsNullOrEmpty(value))
                        {
                            var convertedValue = Convert.ChangeType(value, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
                            prop.SetValue(item, convertedValue);
                        }
                    }
                }

                result.Add(item);
            }

            return result;
        }

        /// <summary>
        /// DataTable转CSV
        /// </summary>
        public static string ToCsv(DataTable table, bool includeHeader = true, char separator = ',')
        {
            var sb = new StringBuilder();

            // 添加表头
            if (includeHeader)
            {
                var headers = new List<string>();
                foreach (DataColumn col in table.Columns)
                {
                    headers.Add(EscapeCsvField(col.ColumnName, separator));
                }
                sb.AppendLine(string.Join(separator, headers));
            }

            // 添加数据行
            foreach (DataRow row in table.Rows)
            {
                var values = new List<string>();
                foreach (DataColumn col in table.Columns)
                {
                    var value = row[col]?.ToString() ?? "";
                    values.Add(EscapeCsvField(value, separator));
                }
                sb.AppendLine(string.Join(separator, values));
            }

            return sb.ToString();
        }

        /// <summary>
        /// CSV转DataTable
        /// </summary>
        public static DataTable FromCsv(string csv, bool hasHeader = true, char separator = ',')
        {
            var table = new DataTable();
            var lines = csv.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            if (lines.Length == 0)
                return table;

            // 解析第一行获取列数
            var firstLine = ParseCsvLine(lines[0], separator);

            // 创建列
            if (hasHeader)
            {
                foreach (var header in firstLine)
                {
                    table.Columns.Add(UnescapeCsvField(header));
                }
            }
            else
            {
                for (int i = 0; i < firstLine.Count; i++)
                {
                    table.Columns.Add($"Column{i + 1}");
                }
            }

            // 添加数据行
            var startIndex = hasHeader ? 1 : 0;
            for (int i = startIndex; i < lines.Length; i++)
            {
                var values = ParseCsvLine(lines[i], separator);
                var row = table.NewRow();

                for (int j = 0; j < Math.Min(values.Count, table.Columns.Count); j++)
                {
                    row[j] = UnescapeCsvField(values[j]);
                }

                table.Rows.Add(row);
            }

            return table;
        }

        /// <summary>
        /// 字典列表转CSV
        /// </summary>
        public static string ToCsv(IEnumerable<Dictionary<string, object?>> dicts, bool includeHeader = true, char separator = ',')
        {
            var sb = new StringBuilder();
            var headers = new List<string>();
            var isFirst = true;

            foreach (var dict in dicts)
            {
                if (isFirst)
                {
                    headers.AddRange(dict.Keys);
                    if (includeHeader)
                    {
                        var headerLine = new List<string>();
                        foreach (var header in headers)
                        {
                            headerLine.Add(EscapeCsvField(header, separator));
                        }
                        sb.AppendLine(string.Join(separator, headerLine));
                    }
                    isFirst = false;
                }

                var values = new List<string>();
                foreach (var header in headers)
                {
                    var value = dict.TryGetValue(header, out var v) ? v?.ToString() ?? "" : "";
                    values.Add(EscapeCsvField(value, separator));
                }
                sb.AppendLine(string.Join(separator, values));
            }

            return sb.ToString();
        }

        /// <summary>
        /// CSV转字典列表
        /// </summary>
        public static List<Dictionary<string, string>> ToDictionaryList(string csv, bool hasHeader = true, char separator = ',')
        {
            var result = new List<Dictionary<string, string>>();
            var lines = csv.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            if (lines.Length == 0)
                return result;

            var firstLine = ParseCsvLine(lines[0], separator);
            var headers = new List<string>();

            if (hasHeader)
            {
                foreach (var h in firstLine)
                {
                    headers.Add(UnescapeCsvField(h));
                }
            }
            else
            {
                for (int i = 0; i < firstLine.Count; i++)
                {
                    headers.Add($"Column{i + 1}");
                }
            }

            var startIndex = hasHeader ? 1 : 0;
            for (int i = startIndex; i < lines.Length; i++)
            {
                var values = ParseCsvLine(lines[i], separator);
                var dict = new Dictionary<string, string>();

                for (int j = 0; j < headers.Count && j < values.Count; j++)
                {
                    dict[headers[j]] = UnescapeCsvField(values[j]);
                }

                result.Add(dict);
            }

            return result;
        }

        /// <summary>
        /// 保存CSV到文件
        /// </summary>
        public static void SaveToFile(string csv, string filePath, Encoding? encoding = null)
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllText(filePath, csv, encoding ?? Encoding.UTF8);
        }

        /// <summary>
        /// 从文件读取CSV
        /// </summary>
        public static string LoadFromFile(string filePath, Encoding? encoding = null)
        {
            return File.ReadAllText(filePath, encoding ?? Encoding.UTF8);
        }

        private static string EscapeCsvField(string field, char separator)
        {
            if (field.Contains(separator) || field.Contains("\"") || field.Contains("\n") || field.Contains("\r"))
            {
                return "\"" + field.Replace("\"", "\"\"") + "\"";
            }
            return field;
        }

        private static string UnescapeCsvField(string field)
        {
            if (field.StartsWith("\"") && field.EndsWith("\""))
            {
                return field.Substring(1, field.Length - 2).Replace("\"\"", "\"");
            }
            return field;
        }

        private static List<string> ParseCsvLine(string line, char separator)
        {
            var result = new List<string>();
            var current = new StringBuilder();
            var inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                var c = line[i];

                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == separator && !inQuotes)
                {
                    result.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }

            result.Add(current.ToString());
            return result;
        }
    }
}
