using System;

namespace EasyTool.IdentifierCategory
{
    /// <summary>
    /// 雪花算法ID生成器
    /// </summary>
    /// <remarks>
    /// 线程安全：是。使用 lock 保护 ID 生成。
    /// </remarks>
    public class SnowflakeIdGenerator
    {
        private long _workerId;
        private long _datacenterId;
        private readonly object _lock = new();
        private long _sequence;
        private long _lastTimestamp;

        /// <summary>
        /// 机器ID位数
        /// </summary>
        public const int WorkerIdBits = 5;

        /// <summary>
        /// 数据中心ID位数
        /// </summary>
        public const int DatacenterIdBits = 5;

        /// <summary>
        /// 序列号位数
        /// </summary>
        public const int SequenceBits = 12;

        /// <summary>
        /// 时间戳位数
        /// </summary>
        public const int TimestampBits = 41;

        /// <summary>
        /// 机器ID最大值
        /// </summary>
        public const long MaxWorkerId = (1L << WorkerIdBits) - 1;

        /// <summary>
        /// 数据中心ID最大值
        /// </summary>
        public const long MaxDatacenterId = (1L << DatacenterIdBits) - 1;

        /// <summary>
        /// 序列号最大值
        /// </summary>
        public const long MaxSequence = (1L << SequenceBits) - 1;

        /// <summary>
        /// 时间戳最大值
        /// </summary>
        public const long MaxTimestamp = (1L << TimestampBits) - 1;

        /// <summary>
        /// 起始时间戳（2020-01-01 00:00:00）
        /// </summary>
        public const long Twepoch = 1577808000000L;

        /// <summary>
        /// 时间戳左移位数
        /// </summary>
        public const int TimestampLeftShift = SequenceBits + WorkerIdBits + DatacenterIdBits;

        /// <summary>
        /// 数据中心ID左移位数
        /// </summary>
        public const int DatacenterIdShift = SequenceBits + WorkerIdBits;

        /// <summary>
        /// 工作机器ID左移位数
        /// </summary>
        public const int WorkerIdShift = SequenceBits;

        /// <summary>
        /// 自定义时间戳生成函数
        /// </summary>
        public Func<long>? CustomTimestampFunc { get; set; }

        /// <summary>
        /// 获取或设置工作机器ID
        /// </summary>
        public long WorkerId
        {
            get => _workerId;
            set
            {
                if (value < 0 || value > MaxWorkerId)
                    throw new ArgumentException($"工作机器ID必须在 0 到 {MaxWorkerId} 之间");

                _workerId = value;
            }
        }

        /// <summary>
        /// 获取或设置数据中心ID
        /// </summary>
        public long DatacenterId
        {
            get => _datacenterId;
            set
            {
                if (value < 0 || value > MaxDatacenterId)
                    throw new ArgumentException($"数据中心ID必须在 0 到 {MaxDatacenterId} 之间");

                _datacenterId = value;
            }
        }

        /// <summary>
        /// 创建雪花算法ID生成器
        /// </summary>
        /// <param name="workerId">工作机器ID（0-31）</param>
        /// <param name="datacenterId">数据中心ID（0-31）</param>
        /// <param name="sequence">初始序列号</param>
        public SnowflakeIdGenerator(long workerId, long datacenterId, long sequence = 0L)
        {
            if (workerId > MaxWorkerId || workerId < 0)
                throw new ArgumentException($"工作机器ID必须在 0 到 {MaxWorkerId} 之间");

            if (datacenterId > MaxDatacenterId || datacenterId < 0)
                throw new ArgumentException($"数据中心ID必须在 0 到 {MaxDatacenterId} 之间");

            _workerId = workerId;
            _datacenterId = datacenterId;
            _sequence = sequence;
            _lastTimestamp = -1L;
        }

        /// <summary>
        /// 创建雪花算法ID生成器（使用默认配置）
        /// </summary>
        public SnowflakeIdGenerator() : this(1, 1, 0) { }

        /// <summary>
        /// 生成下一个唯一ID
        /// </summary>
        /// <returns>唯一ID</returns>
        public virtual long NextId()
        {
                lock (_lock)
                {
                var timestamp = GetCurrentTimestamp();

                // 时钟回拨检测
                if (_lastTimestamp == timestamp)
                {
                    _sequence = (_sequence + 1) & MaxSequence;
                    if (_sequence == 0)
                    {
                        // 序列号溢出，等待下一毫秒
                        timestamp = GetCurrentTimestamp();
                        _sequence = 0;
                    }
                }
                else
                {
                    _sequence = 0;
                }

                _lastTimestamp = timestamp;

                return ((timestamp - Twepoch) << TimestampLeftShift)
                       | (_datacenterId << DatacenterIdShift)
                       | (_workerId << WorkerIdShift)
                       | _sequence;
            }
        }

        /// <summary>
        /// 获取当前时间戳（毫秒）
        /// </summary>
        private long GetCurrentTimestamp()
        {
            if (CustomTimestampFunc != null)
            {
                return CustomTimestampFunc();
            }
            return (DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond);
        }

        /// <summary>
        /// 解析ID
        /// </summary>
        /// <param name="id">雪花ID</param>
        /// <returns>解析结果</returns>
        public static SnowflakeIdInfo Parse(long id)
        {
            var timestamp = (id >> TimestampLeftShift) + Twepoch;
            var datacenterId = (id >> DatacenterIdShift) & MaxDatacenterId;
            var workerId = (id >> WorkerIdShift) & MaxWorkerId;
            var sequence = id & MaxSequence;

            return new SnowflakeIdInfo
            {
                Timestamp = timestamp,
                DataCenterId = datacenterId,
                WorkerId = workerId,
                Sequence = sequence
            };
        }
    }

    /// <summary>
    /// 雪花ID信息
    /// </summary>
    public class SnowflakeIdInfo
    {
        /// <summary>
        /// 时间戳
        /// </summary>
        public long Timestamp { get; set; }

        /// <summary>
        /// 数据中心ID
        /// </summary>
        public long DataCenterId { get; set; }

        /// <summary>
        /// 工作机器ID
        /// </summary>
        public long WorkerId { get; set; }

        /// <summary>
        /// 序列号
        /// </summary>
        public long Sequence { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime DateTime => DateTime.FromBinary(Timestamp);

    }

}
