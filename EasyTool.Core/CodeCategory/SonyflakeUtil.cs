using System;
using System.Threading;

namespace EasyTool.CodeCategory
{
    /// <summary>
    /// Sonyflake ID 工具类
    /// Sonyflake 是 Sony 开发的分布式唯一 ID 生成算法
    /// 结构：39位时间戳 + 8位序列号 + 16位机器ID = 63位
    /// 比雪花 ID 使用更少的时间戳位，支持更长时间
    /// </summary>
    public static class SonyflakeUtil
    {
        private static readonly DateTime Epoch = new DateTime(2014, 9, 1, 0, 0, 0, DateTimeKind.Utc);
        private static long _lastTimestamp = -1L;
        private static ushort _sequence = 0;
        private static readonly object _lock = new object();
        private static readonly ushort _machineId;

        private const int TimestampBits = 39;
        private const int SequenceBits = 8;
        private const int MachineIdBits = 16;

        private const ushort MaxSequence = (1 << SequenceBits) - 1;
        private const ushort MaxMachineId = (1 << MachineIdBits) - 1;

        static SonyflakeUtil()
        {
            // 自动生成机器 ID
            byte[] bytes = new byte[2];
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            _machineId = (ushort)((bytes[0] << 8) | bytes[1]);
        }

        /// <summary>
        /// 生成 Sonyflake ID
        /// </summary>
        /// <returns>63位 ID</returns>
        public static ulong Generate()
        {
            return Generate(_machineId);
        }

        /// <summary>
        /// 生成 Sonyflake ID（指定机器 ID）
        /// </summary>
        /// <param name="machineId">机器 ID（0-65535）</param>
        /// <returns>63位 ID</returns>
        public static ulong Generate(ushort machineId)
        {
            if (machineId > MaxMachineId)
                throw new ArgumentException($"Machine ID must be between 0 and {MaxMachineId}", nameof(machineId));

            lock (_lock)
            {
                long timestamp = GetCurrentTimestamp();

                if (timestamp == _lastTimestamp)
                {
                    _sequence++;
                    if (_sequence > MaxSequence)
                    {
                        timestamp = WaitForNextTimestamp(_lastTimestamp);
                        _sequence = 0;
                    }
                }
                else
                {
                    _sequence = 0;
                }

                _lastTimestamp = timestamp;

                // 组合 ID
                return ((ulong)timestamp << (SequenceBits + MachineIdBits)) |
                       ((ulong)_sequence << MachineIdBits) |
                       machineId;
            }
        }

        /// <summary>
        /// 批量生成 Sonyflake ID
        /// </summary>
        /// <param name="count">数量</param>
        /// <returns>ID 数组</returns>
        public static ulong[] GenerateBatch(int count)
        {
            if (count <= 0)
                throw new ArgumentException("Count must be greater than 0", nameof(count));

            var result = new ulong[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = Generate();
            }
            return result;
        }

        /// <summary>
        /// 从 Sonyflake ID 提取时间戳
        /// </summary>
        /// <param name="id">Sonyflake ID</param>
        /// <returns>UTC 时间</returns>
        public static DateTimeOffset ExtractTimestamp(ulong id)
        {
            long timestamp = (long)(id >> (SequenceBits + MachineIdBits));
            return Epoch.AddMilliseconds(timestamp * 10); // 每 10ms 一个时间单位
        }

        /// <summary>
        /// 从 Sonyflake ID 提取机器 ID
        /// </summary>
        /// <param name="id">Sonyflake ID</param>
        /// <returns>机器 ID</returns>
        public static ushort ExtractMachineId(ulong id)
        {
            return (ushort)(id & MaxMachineId);
        }

        /// <summary>
        /// 从 Sonyflake ID 提取序列号
        /// </summary>
        /// <param name="id">Sonyflake ID</param>
        /// <returns>序列号</returns>
        public static ushort ExtractSequence(ulong id)
        {
            return (ushort)((id >> MachineIdBits) & MaxSequence);
        }

        /// <summary>
        /// 解析 Sonyflake ID
        /// </summary>
        /// <param name="id">Sonyflake ID</param>
        /// <returns>时间戳、机器 ID、序列号</returns>
        public static (DateTimeOffset Timestamp, ushort MachineId, ushort Sequence) Parse(ulong id)
        {
            return (ExtractTimestamp(id), ExtractMachineId(id), ExtractSequence(id));
        }

        /// <summary>
        /// 获取当前机器 ID
        /// </summary>
        public static ushort MachineId => _machineId;

        /// <summary>
        /// 获取 Sonyflake 纪元时间
        /// </summary>
        public static DateTime GetEpoch() => Epoch;

        private static long GetCurrentTimestamp()
        {
            return (long)(DateTime.UtcNow - Epoch).TotalMilliseconds / 10;
        }

        private static long WaitForNextTimestamp(long lastTimestamp)
        {
            long timestamp = GetCurrentTimestamp();
            while (timestamp <= lastTimestamp)
            {
                Thread.SpinWait(10);
                timestamp = GetCurrentTimestamp();
            }
            return timestamp;
        }
    }
}
