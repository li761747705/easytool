using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EasyTool.IOCategory
{
    /// <summary>
    /// CSV 工具类
    /// 提供高性能的 CSV 读写功能
    /// </summary>
    public static class CsvUtil
    {
        /// <summary>
        /// 读取 CSV 文件
        /// </summary>
        public static List<string[]> Read(string filePath, bool hasHeader = true, char delimiter = ',', char quote = '"')
        {
            var reader = new CsvReader(delimiter, quote);
            return reader.ReadFile(filePath, hasHeader);
        }

        /// <summary>
        /// 读取 CSV 文件（带表头映射）
        /// </summary>
        public static List<Dictionary<string, string>> ReadWithHeader(string filePath, char delimiter = ',', char quote = '"')
        {
            var reader = new CsvReader(delimiter, quote);
            return reader.ReadFileWithHeader(filePath);
        }

        /// <summary>
        /// 从字符串解析 CSV
        /// </summary>
        public static List<string[]> Parse(string content, bool hasHeader = true, char delimiter = ',', char quote = '"')
        {
            var reader = new CsvReader(delimiter, quote);
            return reader.Parse(content, hasHeader);
        }

        /// <summary>
        /// 写入 CSV 文件
        /// </summary>
        public static void Write(string filePath, IEnumerable<string[]> rows, char delimiter = ',', char quote = '"')
        {
            var writer = new CsvWriter(delimiter, quote);
            writer.WriteFile(filePath, rows);
        }

        /// <summary>
        /// 写入 CSV 文件（带表头）
        /// </summary>
        public static void WriteWithHeader(string filePath, string[] headers, IEnumerable<Dictionary<string, string>> rows,
            char delimiter = ',', char quote = '"')
        {
            var writer = new CsvWriter(delimiter, quote);
            writer.WriteFileWithHeader(filePath, headers, rows);
        }

        /// <summary>
        /// 将数据转换为 CSV 字符串
        /// </summary>
        public static string ToString(IEnumerable<string[]> rows, char delimiter = ',', char quote = '"')
        {
            var writer = new CsvWriter(delimiter, quote);
            return writer.ToString(rows);
        }

        /// <summary>
        /// 转义 CSV 字段
        /// </summary>
        public static string EscapeField(string field, char delimiter = ',', char quote = '"')
        {
            if (string.IsNullOrEmpty(field))
                return "";

            bool needsQuote = field.Contains(delimiter.ToString()) ||
                              field.Contains(quote.ToString()) ||
                              field.Contains("\n") ||
                              field.Contains("\r");

            if (needsQuote)
            {
                return quote + field.Replace(quote.ToString(), quote.ToString() + quote.ToString()) + quote;
            }

            return field;
        }

        /// <summary>
        /// 反转义 CSV 字段
        /// </summary>
        public static string UnescapeField(string field, char quote = '"')
        {
            if (string.IsNullOrEmpty(field))
                return field;

            if (field.StartsWith(quote.ToString()) && field.EndsWith(quote.ToString()))
            {
                string inner = field.Substring(1, field.Length - 2);
                return inner.Replace(quote.ToString() + quote.ToString(), quote.ToString());
            }

            return field;
        }
    }

    /// <summary>
    /// CSV 读取器
    /// </summary>
    public class CsvReader
    {
        private readonly char _delimiter;
        private readonly char _quote;

        /// <summary>
        /// 创建 CSV 读取器
        /// </summary>
        public CsvReader(char delimiter = ',', char quote = '"')
        {
            _delimiter = delimiter;
            _quote = quote;
        }

        /// <summary>
        /// 读取文件
        /// </summary>
        public List<string[]> ReadFile(string filePath, bool hasHeader = true)
        {
            var content = File.ReadAllText(filePath, DetectEncoding(filePath));
            return Parse(content, hasHeader);
        }

        /// <summary>
        /// 读取文件（带表头）
        /// </summary>
        public List<Dictionary<string, string>> ReadFileWithHeader(string filePath)
        {
            var rows = ReadFile(filePath, true);
            if (rows.Count == 0)
                return new List<Dictionary<string, string>>();

            var headers = rows[0];
            var result = new List<Dictionary<string, string>>();

            for (int i = 1; i < rows.Count; i++)
            {
                var dict = new Dictionary<string, string>();
                for (int j = 0; j < headers.Length && j < rows[i].Length; j++)
                {
                    dict[headers[j]] = rows[i][j];
                }
                result.Add(dict);
            }

            return result;
        }

        /// <summary>
        /// 解析 CSV 内容
        /// </summary>
        public List<string[]> Parse(string content, bool hasHeader = true)
        {
            var rows = new List<string[]>();
            var fields = new List<string>();
            var currentField = new StringBuilder();
            bool inQuotes = false;
            int startRow = hasHeader ? 0 : 0;

            int rowIndex = 0;
            for (int i = 0; i < content.Length; i++)
            {
                char c = content[i];

                if (inQuotes)
                {
                    if (c == _quote)
                    {
                        // 检查是否是转义的引号
                        if (i + 1 < content.Length && content[i + 1] == _quote)
                        {
                            currentField.Append(_quote);
                            i++;
                        }
                        else
                        {
                            inQuotes = false;
                        }
                    }
                    else
                    {
                        currentField.Append(c);
                    }
                }
                else
                {
                    if (c == _quote)
                    {
                        inQuotes = true;
                    }
                    else if (c == _delimiter)
                    {
                        fields.Add(currentField.ToString());
                        currentField.Clear();
                    }
                    else if (c == '\n' || c == '\r')
                    {
                        fields.Add(currentField.ToString());
                        currentField.Clear();

                        if (rowIndex >= startRow)
                        {
                            rows.Add(fields.ToArray());
                        }
                        fields.Clear();
                        rowIndex++;

                        // 处理 \r\n
                        if (c == '\r' && i + 1 < content.Length && content[i + 1] == '\n')
                            i++;
                    }
                    else
                    {
                        currentField.Append(c);
                    }
                }
            }

            // 添加最后一个字段和行
            if (currentField.Length > 0 || fields.Count > 0)
            {
                fields.Add(currentField.ToString());
                if (rowIndex >= startRow)
                {
                    rows.Add(fields.ToArray());
                }
            }

            return rows;
        }

        private static Encoding DetectEncoding(string filePath)
        {
            // 简单的编码检测
            byte[] bom = new byte[4];
            using (var file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                int bytesRead = 0;
                int bytesToRead = 4;
                while (bytesRead < bytesToRead)
                {
                    int read = file.Read(bom, bytesRead, bytesToRead - bytesRead);
                    if (read == 0) break; // 文件结束
                    bytesRead += read;
                }
            }

            if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf)
                return Encoding.UTF8;
            if (bom[0] == 0xff && bom[1] == 0xfe)
                return Encoding.Unicode;
            if (bom[0] == 0xfe && bom[1] == 0xff)
                return Encoding.BigEndianUnicode;

            return Encoding.UTF8;
        }
    }

    /// <summary>
    /// CSV 写入器
    /// </summary>
    public class CsvWriter
    {
        private readonly char _delimiter;
        private readonly char _quote;

        /// <summary>
        /// 创建 CSV 写入器
        /// </summary>
        public CsvWriter(char delimiter = ',', char quote = '"')
        {
            _delimiter = delimiter;
            _quote = quote;
        }

        /// <summary>
        /// 写入文件
        /// </summary>
        public void WriteFile(string filePath, IEnumerable<string[]> rows)
        {
            var content = ToString(rows);
            File.WriteAllText(filePath, content, Encoding.UTF8);
        }

        /// <summary>
        /// 写入文件（带表头）
        /// </summary>
        public void WriteFileWithHeader(string filePath, string[] headers, IEnumerable<Dictionary<string, string>> rows)
        {
            var allRows = new List<string[]> { headers };

            foreach (var row in rows)
            {
                var fields = new string[headers.Length];
                for (int i = 0; i < headers.Length; i++)
                {
                    fields[i] = row.TryGetValue(headers[i], out var value) ? value : "";
                }
                allRows.Add(fields);
            }

            WriteFile(filePath, allRows);
        }

        /// <summary>
        /// 转换为 CSV 字符串
        /// </summary>
        public string ToString(IEnumerable<string[]> rows)
        {
            var sb = new StringBuilder();

            foreach (var row in rows)
            {
                for (int i = 0; i < row.Length; i++)
                {
                    if (i > 0) sb.Append(_delimiter);
                    sb.Append(CsvUtil.EscapeField(row[i], _delimiter, _quote));
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }

    /// <summary>
    /// CSV 配置
    /// </summary>
    public class CsvConfiguration
    {
        /// <summary>
        /// 分隔符
        /// </summary>
        public char Delimiter { get; set; } = ',';

        /// <summary>
        /// 引号字符
        /// </summary>
        public char Quote { get; set; } = '"';

        /// <summary>
        /// 是否有表头
        /// </summary>
        public bool HasHeader { get; set; } = true;

        /// <summary>
        /// 编码
        /// </summary>
        public Encoding Encoding { get; set; } = Encoding.UTF8;

        /// <summary>
        /// 换行符
        /// </summary>
        public string NewLine { get; set; } = Environment.NewLine;

        /// <summary>
        /// 默认配置
        /// </summary>
        public static CsvConfiguration Default => new CsvConfiguration();

        /// <summary>
        /// 中文配置（使用制表符分隔）
        /// </summary>
        public static CsvConfiguration Chinese => new CsvConfiguration { Delimiter = '\t' };
    }
}
