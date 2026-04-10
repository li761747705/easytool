using System;
using System.IO;
using System.Text;

namespace EasyTool.ToolCategory
{
    /// <summary>
    /// 日志级别
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// 跟踪级别
        /// </summary>
        Trace,

        /// <summary>
        /// 调试级别
        /// </summary>
        Debug,

        /// <summary>
        /// 信息级别
        /// </summary>
        Information,

        /// <summary>
        /// 警告级别
        /// </summary>
        Warning,

        /// <summary>
        /// 错误级别
        /// </summary>
        Error,

        /// <summary>
        /// 严重错误级别
        /// </summary>
        Critical
    }

    /// <summary>
    /// 日志工具类
    /// </summary>
    public static class LogUtil
    {
        private static LogLevel _minLevel = LogLevel.Information;
        private static string _logDirectory = "logs";
        private static bool _consoleOutput = true;
        private static bool _fileOutput = true;
        private static readonly object _lock = new();

        /// <summary>
        /// 最小日志级别
        /// </summary>
        public static LogLevel MinLevel
        {
            get => _minLevel;
            set => _minLevel = value;
        }

        /// <summary>
        /// 日志目录
        /// </summary>
        public static string LogDirectory
        {
            get => _logDirectory;
            set
            {
                _logDirectory = value;
                if (!Directory.Exists(value))
                    Directory.CreateDirectory(value);
            }
        }

        /// <summary>
        /// 是否输出到控制台
        /// </summary>
        public static bool ConsoleOutput
        {
            get => _consoleOutput;
            set => _consoleOutput = value;
        }

        /// <summary>
        /// 是否输出到文件
        /// </summary>
        public static bool FileOutput
        {
            get => _fileOutput;
            set => _fileOutput = value;
        }

        /// <summary>
        /// 配置日志
        /// </summary>
        public static void Configure(LogLevel minLevel, string? logDirectory = null, bool consoleOutput = true, bool fileOutput = true)
        {
            _minLevel = minLevel;
            _consoleOutput = consoleOutput;
            _fileOutput = fileOutput;
            if (logDirectory != null)
                LogDirectory = logDirectory;
        }

        /// <summary>
        /// 记录日志
        /// </summary>
        public static void Log(LogLevel level, string message, Exception? exception = null, string? category = null)
        {
            if (level < _minLevel)
                return;

            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var levelStr = level.ToString().ToUpper().PadLeft(5);
            var categoryStr = category != null ? $"[{category}] " : "";
            var sb = new StringBuilder();
            sb.Append($"[{timestamp}] [{levelStr}] {categoryStr}{message}");

            if (exception != null)
            {
                sb.AppendLine();
                sb.Append($"Exception: {exception.GetType().Name}: {exception.Message}");
                if (!string.IsNullOrEmpty(exception.StackTrace))
                {
                    sb.AppendLine();
                    sb.Append(exception.StackTrace);
                }
            }

            var logMessage = sb.ToString();

            lock (_lock)
            {
                if (_consoleOutput)
                {
                    var color = GetConsoleColor(level);
                    var originalColor = Console.ForegroundColor;
                    Console.ForegroundColor = color;
                    Console.WriteLine(logMessage);
                    Console.ForegroundColor = originalColor;
                }

                if (_fileOutput)
                {
                    WriteToFile(level, logMessage);
                }
            }
        }

        private static ConsoleColor GetConsoleColor(LogLevel level)
        {
            return level switch
            {
                LogLevel.Trace => ConsoleColor.Gray,
                LogLevel.Debug => ConsoleColor.Gray,
                LogLevel.Information => ConsoleColor.White,
                LogLevel.Warning => ConsoleColor.Yellow,
                LogLevel.Error => ConsoleColor.Red,
                LogLevel.Critical => ConsoleColor.DarkRed,
                _ => ConsoleColor.White
            };
        }

        private static void WriteToFile(LogLevel level, string message)
        {
            if (!Directory.Exists(_logDirectory))
                Directory.CreateDirectory(_logDirectory);

            var fileName = level switch
            {
                LogLevel.Error or LogLevel.Critical => $"error_{DateTime.Now:yyyyMMdd}.log",
                _ => $"log_{DateTime.Now:yyyyMMdd}.log"
            };

            var filePath = Path.Combine(_logDirectory, fileName);
            File.AppendAllText(filePath, message + Environment.NewLine);
        }

        /// <summary>
        /// 记录跟踪日志
        /// </summary>
        public static void Trace(string message, string? category = null)
            => Log(LogLevel.Trace, message, category: category);

        /// <summary>
        /// 记录调试日志
        /// </summary>
        public static void Debug(string message, string? category = null)
            => Log(LogLevel.Debug, message, category: category);

        /// <summary>
        /// 记录信息日志
        /// </summary>
        public static void Info(string message, string? category = null)
            => Log(LogLevel.Information, message, category: category);

        /// <summary>
        /// 记录警告日志
        /// </summary>
        public static void Warning(string message, string? category = null)
            => Log(LogLevel.Warning, message, category: category);

        /// <summary>
        /// 记录错误日志
        /// </summary>
        public static void Error(string message, Exception? exception = null, string? category = null)
            => Log(LogLevel.Error, message, exception, category);

        /// <summary>
        /// 记录严重错误日志
        /// </summary>
        public static void Critical(string message, Exception? exception = null, string? category = null)
            => Log(LogLevel.Critical, message, exception, category);
    }
}