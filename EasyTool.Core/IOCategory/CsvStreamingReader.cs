using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EasyTool.IOCategory
{
    /// <summary>
    /// CSV 流式读取器选项
    /// </summary>
    public class CsvReaderOptions
    {
        /// <summary>
        /// 分隔符（默认逗号）
        /// </summary>
        public char Delimiter { get; set; } = ',';

        /// <summary>
        /// 引号字符（默认双引号）
        /// </summary>
        public char QuoteChar { get; set; } = '"';

        /// <summary>
        /// 是否有标题行
        /// </summary>
        public bool HasHeader { get; set; } = true;

        /// <summary>
        /// 编码（默认 UTF-8）
        /// </summary>
        public Encoding Encoding { get; set; } = Encoding.UTF8;

        /// <summary>
        /// 缓冲区大小
        /// </summary>
        public int BufferSize { get; set; } = 4096;

        /// <summary>
        /// 是否跳过空行
        /// </summary>
        public bool SkipEmptyLines { get; set; } = true;

        /// <summary>
        /// 是否去除字段首尾空白
        /// </summary>
        public bool TrimFields { get; set; } = false;
    }

    /// <summary>
    /// CSV 流式读取器
    /// 支持大文件逐行读取
    /// </summary>
    public class CsvStreamingReader : IDisposable
    {
        private readonly TextReader _reader;
        private readonly CsvReaderOptions _options;
        private string[]? _headers;
        private int _lineNumber;
        private bool _disposed;

        /// <summary>
        /// 获取标题行
        /// </summary>
        public string[]? Headers => _headers;

        /// <summary>
        /// 获取当前行号
        /// </summary>
        public int LineNumber => _lineNumber;

        /// <summary>
        /// 创建 CSV 流式读取器
        /// </summary>
        /// <param name="reader">文本读取器</param>
        /// <param name="options">选项</param>
        public CsvStreamingReader(TextReader reader, CsvReaderOptions? options = null)
        {
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
            _options = options ?? new CsvReaderOptions();
            _lineNumber = 0;

            if (_options.HasHeader)
            {
                ReadHeaders();
            }
        }

        /// <summary>
        /// 从文件创建 CSV 流式读取器
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="options">选项</param>
        /// <returns>CSV 流式读取器</returns>
        public static CsvStreamingReader FromFile(string filePath, CsvReaderOptions? options = null)
        {
            options ??= new CsvReaderOptions();
            var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, options.BufferSize);
            var reader = new StreamReader(stream, options.Encoding);
            return new CsvStreamingReader(reader, options);
        }

        /// <summary>
        /// 从字符串创建 CSV 流式读取器
        /// </summary>
        /// <param name="content">CSV 内容</param>
        /// <param name="options">选项</param>
        /// <returns>CSV 流式读取器</returns>
        public static CsvStreamingReader FromString(string content, CsvReaderOptions? options = null)
        {
            var reader = new StringReader(content);
            return new CsvStreamingReader(reader, options);
        }

        /// <summary>
        /// 读取标题行
        /// </summary>
        private void ReadHeaders()
        {
            var line = _reader.ReadLine();
            _lineNumber++;

            if (line != null)
            {
                _headers = ParseLine(line);
            }
        }

        /// <summary>
        /// 读取下一行
        /// </summary>
        /// <returns>字段数组，如果到达文件末尾则返回 null</returns>
        public string[]? ReadLine()
        {
            while (true)
            {
                var line = _reader.ReadLine();
                _lineNumber++;

                if (line == null)
                    return null;

                if (_options.SkipEmptyLines && string.IsNullOrWhiteSpace(line))
                    continue;

                return ParseLine(line);
            }
        }

        /// <summary>
        /// 异步读取下一行
        /// </summary>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>字段数组，如果到达文件末尾则返回 null</returns>
        public async Task<string[]?> ReadLineAsync(CancellationToken cancellationToken = default)
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var line = await _reader.ReadLineAsync().ConfigureAwait(false);

                if (line == null)
                    return null;

                _lineNumber++;

                if (_options.SkipEmptyLines && string.IsNullOrWhiteSpace(line))
                    continue;

                return ParseLine(line);
            }
        }

        /// <summary>
        /// 读取所有行
        /// </summary>
        /// <returns>所有行的枚举</returns>
        public IEnumerable<string[]> ReadAll()
        {
            string[]? line;

            while ((line = ReadLine()) != null)
            {
                yield return line;
            }
        }

        /// <summary>
        /// 异步读取所有行
        /// </summary>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>所有行的异步枚举</returns>
        public async IAsyncEnumerable<string[]> ReadAllAsync([System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            string[]? line;

            while ((line = await ReadLineAsync(cancellationToken).ConfigureAwait(false)) != null)
            {
                yield return line;
            }
        }

        /// <summary>
        /// 读取为字典（需要标题行）
        /// </summary>
        /// <returns>字典行的枚举</returns>
        public IEnumerable<Dictionary<string, string>> ReadAsDict()
        {
            if (_headers == null)
                throw new InvalidOperationException("需要标题行才能读取为字典");

            string[]? line;

            while ((line = ReadLine()) != null)
            {
                yield return LineToDict(line);
            }
        }

        /// <summary>
        /// 异步读取为字典（需要标题行）
        /// </summary>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>字典行的异步枚举</returns>
        public async IAsyncEnumerable<Dictionary<string, string>> ReadAsDictAsync([System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (_headers == null)
                throw new InvalidOperationException("需要标题行才能读取为字典");

            string[]? line;

            while ((line = await ReadLineAsync(cancellationToken).ConfigureAwait(false)) != null)
            {
                yield return LineToDict(line);
            }
        }

        /// <summary>
        /// 解析 CSV 行
        /// </summary>
        private string[] ParseLine(string line)
        {
            var fields = new List<string>();
            var currentField = new StringBuilder();
            var inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                var c = line[i];

                if (inQuotes)
                {
                    if (c == _options.QuoteChar)
                    {
                        // 检查是否是转义引号
                        if (i + 1 < line.Length && line[i + 1] == _options.QuoteChar)
                        {
                            currentField.Append(_options.QuoteChar);
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
                    if (c == _options.QuoteChar)
                    {
                        inQuotes = true;
                    }
                    else if (c == _options.Delimiter)
                    {
                        fields.Add(FinalizeField(currentField));
                        currentField.Clear();
                    }
                    else
                    {
                        currentField.Append(c);
                    }
                }
            }

            fields.Add(FinalizeField(currentField));

            return fields.ToArray();
        }

        private string FinalizeField(StringBuilder field)
        {
            var result = field.ToString();

            if (_options.TrimFields)
            {
                result = result.Trim();
            }

            return result;
        }

        private Dictionary<string, string> LineToDict(string[] fields)
        {
            var dict = new Dictionary<string, string>();

            for (int i = 0; i < _headers!.Length && i < fields.Length; i++)
            {
                dict[_headers[i]] = fields[i];
            }

            return dict;
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                _reader.Dispose();
                _disposed = true;
            }
        }
    }

    /// <summary>
    /// CSV 流式写入器
    /// </summary>
    public class CsvStreamingWriter : IDisposable
    {
        private readonly TextWriter _writer;
        private readonly CsvReaderOptions _options;
        private bool _disposed;
        private bool _headerWritten;

        /// <summary>
        /// 创建 CSV 流式写入器
        /// </summary>
        /// <param name="writer">文本写入器</param>
        /// <param name="options">选项</param>
        public CsvStreamingWriter(TextWriter writer, CsvReaderOptions? options = null)
        {
            _writer = writer ?? throw new ArgumentNullException(nameof(writer));
            _options = options ?? new CsvReaderOptions();
        }

        /// <summary>
        /// 创建文件 CSV 写入器
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="options">选项</param>
        /// <param name="append">是否追加</param>
        /// <returns>CSV 写入器</returns>
        public static CsvStreamingWriter ToFile(string filePath, CsvReaderOptions? options = null, bool append = false)
        {
            options ??= new CsvReaderOptions();
            var stream = new FileStream(filePath, append ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.None, options.BufferSize);
            var writer = new StreamWriter(stream, options.Encoding);
            return new CsvStreamingWriter(writer, options);
        }

        /// <summary>
        /// 写入标题行
        /// </summary>
        /// <param name="headers">标题</param>
        public void WriteHeaders(params string[] headers)
        {
            if (_headerWritten)
                throw new InvalidOperationException("标题行已写入");

            WriteLine(headers);
            _headerWritten = true;
        }

        /// <summary>
        /// 写入一行
        /// </summary>
        /// <param name="fields">字段</param>
        public void WriteLine(params string[] fields)
        {
            var line = FormatLine(fields);
            _writer.WriteLine(line);
        }

        /// <summary>
        /// 异步写入一行
        /// </summary>
        /// <param name="fields">字段</param>
        public async Task WriteLineAsync(params string[] fields)
        {
            var line = FormatLine(fields);
            await _writer.WriteLineAsync(line).ConfigureAwait(false);
        }

        /// <summary>
        /// 写入字典行
        /// </summary>
        /// <param name="dict">字典</param>
        /// <param name="columnOrder">列顺序</param>
        public void WriteDict(Dictionary<string, string> dict, string[]? columnOrder = null)
        {
            var columns = columnOrder ?? dict.Keys.ToArray();
            var fields = columns.Select(c => dict.TryGetValue(c, out var v) ? v : "").ToArray();
            WriteLine(fields);
        }

        /// <summary>
        /// 异步写入字典行
        /// </summary>
        /// <param name="dict">字典</param>
        /// <param name="columnOrder">列顺序</param>
        public async Task WriteDictAsync(Dictionary<string, string> dict, string[]? columnOrder = null)
        {
            var columns = columnOrder ?? dict.Keys.ToArray();
            var fields = columns.Select(c => dict.TryGetValue(c, out var v) ? v : "").ToArray();
            await WriteLineAsync(fields).ConfigureAwait(false);
        }

        /// <summary>
        /// 刷新缓冲区
        /// </summary>
        public void Flush()
        {
            _writer.Flush();
        }

        /// <summary>
        /// 异步刷新缓冲区
        /// </summary>
        public async Task FlushAsync()
        {
            await _writer.FlushAsync().ConfigureAwait(false);
        }

        private string FormatLine(string[] fields)
        {
            var formattedFields = fields.Select(f => FormatField(f));
            return string.Join(_options.Delimiter, formattedFields);
        }

        private string FormatField(string field)
        {
            if (string.IsNullOrEmpty(field))
                return "";

                        bool needsQuoting = field.Contains(_options.Delimiter) ||

                                           field.Contains(_options.QuoteChar) ||

                                           field.Contains('\n') ||

                                           field.Contains('\r');

            if (needsQuoting)
            {
                var escaped = field.Replace(_options.QuoteChar.ToString(), _options.QuoteChar.ToString() + _options.QuoteChar.ToString());
                return $"{_options.QuoteChar}{escaped}{_options.QuoteChar}";
            }

            return field;
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                _writer.Dispose();
                _disposed = true;
            }
        }
    }
}
