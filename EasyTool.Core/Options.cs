using System;

namespace EasyTool
{
    /// <summary>
    /// 限流器配置选项
    /// </summary>
    public class RateLimiterOptions
    {
        /// <summary>
        /// 限流算法
        /// </summary>
        public ToolCategory.RateLimitAlgorithm Algorithm { get; set; } = ToolCategory.RateLimitAlgorithm.TokenBucket;

        /// <summary>
        /// 限制数量
        /// </summary>
        public int Limit { get; set; } = 100;

        /// <summary>
        /// 时间窗口
        /// </summary>
        public TimeSpan Window { get; set; } = TimeSpan.FromSeconds(1);

        /// <summary>
        /// 令牌桶容量（仅 TokenBucket 算法）
        /// </summary>
        public int? Capacity { get; set; }

        /// <summary>
        /// 令牌补充速率（仅 TokenBucket 算法）
        /// </summary>
        public int? RefillRate { get; set; }
    }

    /// <summary>
    /// 熔断器配置选项
    /// </summary>
    public class CircuitBreakerOptions
    {
        /// <summary>
        /// 失败阈值次数
        /// </summary>
        public int FailureThreshold { get; set; } = 5;

        /// <summary>
        /// 成功阈值次数（半开状态）
        /// </summary>
        public int SuccessThreshold { get; set; } = 2;

        /// <summary>
        /// 打开状态持续时间
        /// </summary>
        public TimeSpan OpenDuration { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// 超时时间
        /// </summary>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);
    }

    /// <summary>
    /// 重试配置选项
    /// </summary>
    public class RetryOptions
    {
        /// <summary>
        /// 最大重试次数
        /// </summary>
        public int MaxRetries { get; set; } = 3;

        /// <summary>
        /// 重试延迟
        /// </summary>
        public TimeSpan Delay { get; set; } = TimeSpan.FromSeconds(1);

        /// <summary>
        /// 最大延迟（指数退避）
        /// </summary>
        public TimeSpan MaxDelay { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// 是否使用指数退避
        /// </summary>
        public bool UseExponentialBackoff { get; set; } = true;

        /// <summary>
        /// 退避倍数
        /// </summary>
        public double BackoffMultiplier { get; set; } = 2.0;
    }

    /// <summary>
    /// HTTP 客户端配置选项
    /// </summary>
    public class HttpClientOptions
    {
        /// <summary>
        /// 基础地址
        /// </summary>
        public string? BaseAddress { get; set; }

        /// <summary>
        /// 超时时间
        /// </summary>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// 最大响应内容缓冲区大小
        /// </summary>
        public long MaxResponseContentBufferSize { get; set; } = int.MaxValue;

        /// <summary>
        /// 是否自动解压缩
        /// </summary>
        public bool EnableAutoDecompression { get; set; } = true;

        /// <summary>
        /// 是否忽略 SSL 错误
        /// </summary>
        public bool IgnoreSslErrors { get; set; }

        /// <summary>
        /// 默认请求头
        /// </summary>
        public System.Collections.Generic.Dictionary<string, string> DefaultHeaders { get; set; } = new();

        /// <summary>
        /// 重试次数
        /// </summary>
        public int RetryCount { get; set; } = 0;

        /// <summary>
        /// 重试延迟
        /// </summary>
        public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(1);
    }

    /// <summary>
    /// 文件监控配置选项
    /// </summary>
    public class FileWatcherOptions
    {
        /// <summary>
        /// 监控路径
        /// </summary>
        public string Path { get; set; } = string.Empty;

        /// <summary>
        /// 文件过滤模式
        /// </summary>
        public string Filter { get; set; } = "*.*";

        /// <summary>
        /// 是否包含子目录
        /// </summary>
        public bool IncludeSubdirectories { get; set; } = true;

        /// <summary>
        /// 是否监控文件名变更
        /// </summary>
        public bool EnableRaisingEvents { get; set; } = true;

        /// <summary>
        /// 内部缓冲区大小
        /// </summary>
        public int InternalBufferSize { get; set; } = 8192;

        /// <summary>
        /// 通知过滤器
        /// </summary>
        public System.IO.NotifyFilters NotifyFilter { get; set; } = 
            System.IO.NotifyFilters.FileName | 
            System.IO.NotifyFilters.DirectoryName | 
            System.IO.NotifyFilters.LastWrite;
    }

    /// <summary>
    /// 日志配置选项
    /// </summary>
    public class LogOptions
    {
        /// <summary>
        /// 最小日志级别
        /// </summary>
        public LogLevel MinimumLevel { get; set; } = LogLevel.Information;

        /// <summary>
        /// 是否输出到控制台
        /// </summary>
        public bool WriteToConsole { get; set; } = true;

        /// <summary>
        /// 是否输出到文件
        /// </summary>
        public bool WriteToFile { get; set; }

        /// <summary>
        /// 日志文件路径
        /// </summary>
        public string? LogFilePath { get; set; }

        /// <summary>
        /// 日志文件滚动间隔
        /// </summary>
        public RollingInterval RollingInterval { get; set; } = RollingInterval.Day;

        /// <summary>
        /// 日志文件最大大小（字节）
        /// </summary>
        public long? MaxFileSize { get; set; }

        /// <summary>
        /// 保留日志文件数量
        /// </summary>
        public int? RetainedFileCount { get; set; }

        /// <summary>
        /// 输出模板
        /// </summary>
        public string OutputTemplate { get; set; } = "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}";
    }

    /// <summary>
    /// 日志级别
    /// </summary>
    public enum LogLevel
    {
        Trace = 0,
        Debug = 1,
        Information = 2,
        Warning = 3,
        Error = 4,
        Critical = 5,
        None = 6
    }

    /// <summary>
    /// 日志文件滚动间隔
    /// </summary>
    public enum RollingInterval
    {
        Infinite = 0,
        Year = 1,
        Month = 2,
        Day = 3,
        Hour = 4,
        Minute = 5
    }

    /// <summary>
    /// 对象池配置选项
    /// </summary>
    public class ObjectPoolOptions
    {
        /// <summary>
        /// 最大容量
        /// </summary>
        public int MaximumCapacity { get; set; } = 1024;

        /// <summary>
        /// 初始容量
        /// </summary>
        public int InitialCapacity { get; set; } = 10;

        /// <summary>
        /// 对象最大闲置时间
        /// </summary>
        public TimeSpan MaxIdleTime { get; set; } = TimeSpan.FromMinutes(5);

        /// <summary>
        /// 清理间隔
        /// </summary>
        public TimeSpan CleanupInterval { get; set; } = TimeSpan.FromMinutes(1);
    }
}
