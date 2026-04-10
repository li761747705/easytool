using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EasyTool.IOCategory
{
    /// <summary>
    /// CSV工具类
    /// 提供CSV文件的读写功能
    /// </summary>
    public static class CsvUtil
    {
        #region 读取CSV

        /// <summary>
        /// 读取CSV文件为字符串二维数组
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="encoding">编码</param>
        /// <param name="hasHeader">是否有标题行</param>
        /// <param name="delimiter">分隔符</param>
        /// <returns>数据数组</returns>
        public static string[][] Read(string filePath, Encoding? encoding = null, bool hasHeader = false, char delimiter = ',')
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("CSV文件不存在", filePath);

            encoding ??= Encoding.UTF8;
            var lines = File.ReadAllLines(filePath, encoding);
            var result = new List<string[]>();

            int startRow = hasHeader ? 1 : 0;
            for (int i = startRow; i < lines.Length; i++)
            {
                var row = ParseLine(lines[i], delimiter);
                result.Add(row);
            }

            return result.ToArray();
        }

        /// <summary>
        /// 异步读取CSV文件
        /// </summary>
        public static async Task<string[][]> ReadAsync(string filePath, Encoding? encoding = null, bool hasHeader = false, char delimiter = ',')
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("CSV文件不存在", filePath);

            encoding ??= Encoding.UTF8;
            var lines = await File.ReadAllLinesAsync(filePath, encoding).ConfigureAwait(false);
            var result = new List<string[]>();

            int startRow = hasHeader ? 1 : 0;
            for (int i = startRow; i < lines.Length; i++)
            {
                var row = ParseLine(lines[i], delimiter);
                result.Add(row);
            }

            return result.ToArray();
        }

        /// <summary>
        /// 读取CSV文件为对象列表
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="filePath">文件路径</param>
        /// <param name="encoding">编码</param>
        /// <param name="delimiter">分隔符</param>
        /// <returns>对象列表</returns>
        public static List<T> Read<T>(string filePath, Encoding? encoding = null, char delimiter = ',') where T : class, new()
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("CSV文件不存在", filePath);

            encoding ??= Encoding.UTF8;
            var lines = File.ReadAllLines(filePath, encoding);

            if (lines.Length == 0)
                return new List<T>();

            var headers = ParseLine(lines[0], delimiter);
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite)
                .ToList();

            var result = new List<T>();

            for (int i = 1; i < lines.Length; i++)
            {
                var values = ParseLine(lines[i], delimiter);
                var obj = new T();

                for (int j = 0; j < headers.Length && j < values.Length; j++)
                {
                    var property = properties.FirstOrDefault(p =>
                        p.Name.Equals(headers[j], StringComparison.OrdinalIgnoreCase));

                    if (property != null)
                    {
                        var value = ConvertValue(values[j], property.PropertyType);
                        property.SetValue(obj, value);
                    }
                }

                result.Add(obj);
            }

            return result;
        }

        /// <summary>
        /// 异步读取CSV文件为对象列表
        /// </summary>
        public static async Task<List<T>> ReadAsync<T>(string filePath, Encoding? encoding = null, char delimiter = ',') where T : class, new()
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("CSV文件不存在", filePath);

            encoding ??= Encoding.UTF8;
            var lines = await File.ReadAllLinesAsync(filePath, encoding).ConfigureAwait(false);

            if (lines.Length == 0)
                return new List<T>();

            var headers = ParseLine(lines[0], delimiter);
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite)
                .ToList();

            var result = new List<T>();

            for (int i = 1; i < lines.Length; i++)
            {
                var values = ParseLine(lines[i], delimiter);
                var obj = new T();

                for (int j = 0; j < headers.Length && j < values.Length; j++)
                {
                    var property = properties.FirstOrDefault(p =>
                        p.Name.Equals(headers[j], StringComparison.OrdinalIgnoreCase));

                    if (property != null)
                    {
                        var value = ConvertValue(values[j], property.PropertyType);
                        property.SetValue(obj, value);
                    }
                }

                result.Add(obj);
            }

            return result;
        }

        /// <summary>
        /// 读取CSV文件（带标题映射）
        /// </summary>
        public static List<T> Read<T>(string filePath, Dictionary<string, string> columnMapping, Encoding? encoding = null, char delimiter = ',') where T : class, new()
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("CSV文件不存在", filePath);

            encoding ??= Encoding.UTF8;
            var lines = File.ReadAllLines(filePath, encoding);

            if (lines.Length == 0)
                return new List<T>();

            var headers = ParseLine(lines[0], delimiter);
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite)
                .ToList();

            var result = new List<T>();

            for (int i = 1; i < lines.Length; i++)
            {
                var values = ParseLine(lines[i], delimiter);
                var obj = new T();

                for (int j = 0; j < headers.Length && j < values.Length; j++)
                {
                    var csvColumn = headers[j];
                    string? propertyName;

                    if (columnMapping.TryGetValue(csvColumn, out propertyName) ||
                        columnMapping.TryGetValue(csvColumn.ToLower(), out propertyName))
                    {
                        var property = properties.FirstOrDefault(p =>
                            p.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase));

                        if (property != null)
                        {
                            var value = ConvertValue(values[j], property.PropertyType);
                            property.SetValue(obj, value);
                        }
                    }
                }

                result.Add(obj);
            }

            return result;
        }

        #endregion

        #region 写入CSV

        /// <summary>
        /// 写入CSV文件
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="data">数据</param>
        /// <param name="headers">标题行</param>
        /// <param name="encoding">编码</param>
        /// <param name="delimiter">分隔符</param>
        public static void Write(string filePath, IEnumerable<string[]> data, string[]? headers = null, Encoding? encoding = null, char delimiter = ',')
        {
            encoding ??= Encoding.UTF8;
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var lines = new List<string>();

            if (headers != null && headers.Length > 0)
            {
                lines.Add(FormatLine(headers, delimiter));
            }

            foreach (var row in data)
            {
                lines.Add(FormatLine(row, delimiter));
            }

            File.WriteAllLines(filePath, lines, encoding);
        }

        /// <summary>
        /// 异步写入CSV文件
        /// </summary>
        public static async Task WriteAsync(string filePath, IEnumerable<string[]> data, string[]? headers = null, Encoding? encoding = null, char delimiter = ',')
        {
            encoding ??= Encoding.UTF8;
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var lines = new List<string>();

            if (headers != null && headers.Length > 0)
            {
                lines.Add(FormatLine(headers, delimiter));
            }

            foreach (var row in data)
            {
                lines.Add(FormatLine(row, delimiter));
            }

            await File.WriteAllLinesAsync(filePath, lines, encoding).ConfigureAwait(false);
        }

        /// <summary>
        /// 写入对象列表到CSV文件
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="filePath">文件路径</param>
        /// <param name="data">数据列表</param>
        /// <param name="headers">自定义标题（可选）</param>
        /// <param name="encoding">编码</param>
        /// <param name="delimiter">分隔符</param>
        public static void Write<T>(string filePath, IEnumerable<T> data, string[]? headers = null, Encoding? encoding = null, char delimiter = ',')
        {
            encoding ??= Encoding.UTF8;
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead)
                .ToList();

            var lines = new List<string>();

            // 标题行
            if (headers != null && headers.Length > 0)
            {
                lines.Add(FormatLine(headers, delimiter));
            }
            else
            {
                var propertyNames = properties.Select(p => p.Name).ToArray();
                lines.Add(FormatLine(propertyNames, delimiter));
            }

            // 数据行
            foreach (var item in data)
            {
                var values = properties.Select(p => FormatValue(p.GetValue(item))).ToArray();
                lines.Add(FormatLine(values, delimiter));
            }

            File.WriteAllLines(filePath, lines, encoding);
        }

        /// <summary>
        /// 异步写入对象列表到CSV文件
        /// </summary>
        public static async Task WriteAsync<T>(string filePath, IEnumerable<T> data, string[]? headers = null, Encoding? encoding = null, char delimiter = ',')
        {
            encoding ??= Encoding.UTF8;
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead)
                .ToList();

            var lines = new List<string>();

            // 标题行
            if (headers != null && headers.Length > 0)
            {
                lines.Add(FormatLine(headers, delimiter));
            }
            else
            {
                var propertyNames = properties.Select(p => p.Name).ToArray();
                lines.Add(FormatLine(propertyNames, delimiter));
            }

            // 数据行
            foreach (var item in data)
            {
                var values = properties.Select(p => FormatValue(p.GetValue(item))).ToArray();
                lines.Add(FormatLine(values, delimiter));
            }

            await File.WriteAllLinesAsync(filePath, lines, encoding).ConfigureAwait(false);
        }

        /// <summary>
        /// 追加数据到CSV文件
        /// </summary>
        public static void Append(string filePath, IEnumerable<string[]> data, Encoding? encoding = null, char delimiter = ',')
        {
            encoding ??= Encoding.UTF8;

            var lines = new List<string>();
            foreach (var row in data)
            {
                lines.Add(FormatLine(row, delimiter));
            }

            File.AppendAllLines(filePath, lines, encoding);
        }

        /// <summary>
        /// 异步追加数据到CSV文件
        /// </summary>
        public static async Task AppendAsync(string filePath, IEnumerable<string[]> data, Encoding? encoding = null, char delimiter = ',')
        {
            encoding ??= Encoding.UTF8;

            var lines = new List<string>();
            foreach (var row in data)
            {
                lines.Add(FormatLine(row, delimiter));
            }

            await File.AppendAllLinesAsync(filePath, lines, encoding).ConfigureAwait(false);
        }

        #endregion

        #region 解析与格式化

        /// <summary>
        /// 解析CSV行
        /// </summary>
        private static string[] ParseLine(string line, char delimiter)
        {
            var result = new List<string>();
            var current = new StringBuilder();
            bool inQuotes = false;

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
                else if (c == delimiter && !inQuotes)
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
            return result.ToArray();
        }

        /// <summary>
        /// 格式化CSV行
        /// </summary>
        private static string FormatLine(string[] values, char delimiter)
        {
            return string.Join(delimiter, values.Select(v => EscapeValue(v)));
        }

        /// <summary>
        /// 转义CSV值
        /// </summary>
        private static string EscapeValue(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return "";

            if (value.Contains('"') || value.Contains(',') || value.Contains('\n') || value.Contains('\r'))
            {
                return $"\"{value.Replace("\"", "\"\"")}\"";
            }

            return value;
        }

        /// <summary>
        /// 格式化值
        /// </summary>
        private static string FormatValue(object? value)
        {
            if (value == null)
                return "";

            return value switch
            {
                DateTime dt => dt.ToString("yyyy-MM-dd HH:mm:ss"),
                decimal dec => dec.ToString(CultureInfo.InvariantCulture),
                double d => d.ToString(CultureInfo.InvariantCulture),
                float f => f.ToString(CultureInfo.InvariantCulture),
                bool b => b.ToString().ToLower(),
                _ => value.ToString() ?? ""
            };
        }

        /// <summary>
        /// 转换值类型
        /// </summary>
        private static object? ConvertValue(string value, Type targetType)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
            }

            try
            {
                if (targetType == typeof(string))
                    return value;

                if (targetType == typeof(int) || targetType == typeof(int?))
                    return int.Parse(value);

                if (targetType == typeof(long) || targetType == typeof(long?))
                    return long.Parse(value);

                if (targetType == typeof(double) || targetType == typeof(double?))
                    return double.Parse(value, CultureInfo.InvariantCulture);

                if (targetType == typeof(decimal) || targetType == typeof(decimal?))
                    return decimal.Parse(value, CultureInfo.InvariantCulture);

                if (targetType == typeof(bool) || targetType == typeof(bool?))
                    return bool.Parse(value);

                if (targetType == typeof(DateTime) || targetType == typeof(DateTime?))
                    return DateTime.TryParse(value, out var dt) ? dt : DateTime.MinValue;

                if (targetType == typeof(Guid) || targetType == typeof(Guid?))
                    return Guid.Parse(value);

                return Convert.ChangeType(value, targetType);
            }
            catch
            {
                return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
            }
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 从字符串读取CSV数据
        /// </summary>
        public static List<string[]> Parse(string csvContent, bool hasHeader = false, char delimiter = ',')
        {
            var lines = csvContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            var result = new List<string[]>();

            int startRow = hasHeader ? 1 : 0;
            for (int i = startRow; i < lines.Length; i++)
            {
                var row = ParseLine(lines[i], delimiter);
                result.Add(row);
            }

            return result;
        }

        /// <summary>
        /// 将数据转换为CSV字符串
        /// </summary>
        public static string ToCsvString(IEnumerable<string[]> data, string[]? headers = null, char delimiter = ',')
        {
            var sb = new StringBuilder();

            if (headers != null && headers.Length > 0)
            {
                sb.AppendLine(FormatLine(headers, delimiter));
            }

            foreach (var row in data)
            {
                sb.AppendLine(FormatLine(row, delimiter));
            }

            return sb.ToString();
        }

        /// <summary>
        /// 将对象列表转换为CSV字符串
        /// </summary>
        public static string ToCsvString<T>(IEnumerable<T> data, string[]? headers = null, char delimiter = ',')
        {
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead)
                .ToList();

            var sb = new StringBuilder();

            // 标题行
            if (headers != null && headers.Length > 0)
            {
                sb.AppendLine(FormatLine(headers, delimiter));
            }
            else
            {
                var propertyNames = properties.Select(p => p.Name).ToArray();
                sb.AppendLine(FormatLine(propertyNames, delimiter));
            }

            // 数据行
            foreach (var item in data)
            {
                var values = properties.Select(p => FormatValue(p.GetValue(item))).ToArray();
                sb.AppendLine(FormatLine(values, delimiter));
            }

            return sb.ToString();
        }

        /// <summary>
        /// 获取CSV文件的列数
        /// </summary>
        public static int GetColumnCount(string filePath, Encoding? encoding = null, char delimiter = ',')
        {
            if (!File.Exists(filePath))
                return 0;

            encoding ??= Encoding.UTF8;
            using var reader = new StreamReader(filePath, encoding);
            var firstLine = reader.ReadLine();

            if (string.IsNullOrEmpty(firstLine))
                return 0;

            return ParseLine(firstLine, delimiter).Length;
        }

        /// <summary>
        /// 获取CSV文件的行数
        /// </summary>
        public static int GetRowCount(string filePath, bool hasHeader = true, Encoding? encoding = null)
        {
            if (!File.Exists(filePath))
                return 0;

            encoding ??= Encoding.UTF8;
            var lines = File.ReadAllLines(filePath, encoding);
            return hasHeader ? Math.Max(0, lines.Length - 1) : lines.Length;
        }

        #endregion
    }
}