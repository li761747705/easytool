using System;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;

namespace EasyTool.IdentifierCategory
{
    /// <summary>
    /// MongoDB ObjectId 生成器
    /// ObjectId 是一个 12 字节的唯一标识符，由时间戳、机器标识、进程 ID 和计数器组成
    /// </summary>
    /// <remarks>
    /// 线程安全：是。使用 Interlocked 原子操作。
    /// </remarks>
    public static class ObjectIdUtil
    {
        private static readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();
        private static readonly byte[] _machineId;
        private static readonly byte[] _processId;
        private static int _counter;
        private static readonly object _counterLock = new();

        static ObjectIdUtil()
        {
            _machineId = GetMachineId();
            _processId = GetProcessId();
            _counter = GetRandomCounter();
        }

        private const int ObjectIdLength = 12;
        private const int TimestampLength = 4;
        private const int MachineIdLength = 3;
        private const int ProcessIdLength = 2;
        private const int CounterLength = 3;

        /// <summary>
        /// 生成新的 ObjectId
        /// </summary>
        /// <returns>ObjectId 字节数组</returns>
        public static byte[] Generate()
        {
            return Generate(DateTimeOffset.UtcNow);
        }

        /// <summary>
        /// 生成指定时间的 ObjectId
        /// </summary>
        /// <param name="timestamp">时间戳</param>
        /// <returns>ObjectId 字节数组</returns>
        public static byte[] Generate(DateTimeOffset timestamp)
        {
            var objectId = new byte[ObjectIdLength];
            var timestampSec = (int)timestamp.ToUnixTimeSeconds();

            // 写入时间戳（4字节，大端序）
            objectId[0] = (byte)(timestampSec >> 24);
            objectId[1] = (byte)(timestampSec >> 16);
            objectId[2] = (byte)(timestampSec >> 8);
            objectId[3] = (byte)timestampSec;

            // 写入机器标识（3字节）
            Buffer.BlockCopy(_machineId, 0, objectId, 4, MachineIdLength);

            // 写入进程 ID（2字节）
            Buffer.BlockCopy(_processId, 0, objectId, 7, ProcessIdLength);

            // 写入计数器（3字节，大端序）
            int counter;
            lock (_counterLock)
            {
                counter = _counter++;
                if (_counter > 0xFFFFFF)
                {
                    _counter = GetRandomCounter();
                }
            }

            objectId[9] = (byte)(counter >> 16);
            objectId[10] = (byte)(counter >> 8);
            objectId[11] = (byte)counter;

            return objectId;
        }

        /// <summary>
        /// 生成新的 ObjectId 字符串
        /// </summary>
        /// <returns>ObjectId 字符串（24字符十六进制）</returns>
        public static string GenerateString()
        {
            return Encode(Generate());
        }

        /// <summary>
        /// 生成指定时间的 ObjectId 字符串
        /// </summary>
        /// <param name="timestamp">时间戳</param>
        /// <returns>ObjectId 字符串（24字符十六进制）</returns>
        public static string GenerateString(DateTimeOffset timestamp)
        {
            return Encode(Generate(timestamp));
        }

        /// <summary>
        /// 将 ObjectId 字节数组编码为十六进制字符串
        /// </summary>
        /// <param name="bytes">ObjectId 字节数组</param>
        /// <returns>ObjectId 十六进制字符串</returns>
        public static string Encode(byte[] bytes)
        {
            if (bytes == null || bytes.Length != ObjectIdLength)
            {
                throw new ArgumentException($"ObjectId 字节数组长度必须为 {ObjectIdLength}", nameof(bytes));
            }

            var hex = new StringBuilder(24);
            foreach (var b in bytes)
            {
                hex.AppendFormat("{0:x2}", b);
            }
            return hex.ToString();
        }

        /// <summary>
        /// 将十六进制字符串解码为 ObjectId 字节数组
        /// </summary>
        /// <param name="objectId">ObjectId 十六进制字符串</param>
        /// <returns>ObjectId 字节数组</returns>
        public static byte[] Decode(string objectId)
        {
            if (string.IsNullOrEmpty(objectId) || objectId.Length != 24)
            {
                throw new ArgumentException("ObjectId 字符串长度必须为 24", nameof(objectId));
            }

            var bytes = new byte[ObjectIdLength];
            for (int i = 0; i < ObjectIdLength; i++)
            {
                var hex = objectId.Substring(i * 2, 2);
                bytes[i] = Convert.ToByte(hex, 16);
            }
            return bytes;
        }

        /// <summary>
        /// 从 ObjectId 提取时间戳
        /// </summary>
        /// <param name="objectId">ObjectId 字节数组</param>
        /// <returns>时间戳</returns>
        public static DateTimeOffset ExtractTimestamp(byte[] objectId)
        {
            if (objectId == null || objectId.Length != ObjectIdLength)
            {
                throw new ArgumentException($"ObjectId 字节数组长度必须为 {ObjectIdLength}", nameof(objectId));
            }

            var timestampSec = (objectId[0] << 24) |
                              (objectId[1] << 16) |
                              (objectId[2] << 8) |
                              objectId[3];

            return DateTimeOffset.FromUnixTimeSeconds(timestampSec);
        }

        /// <summary>
        /// 从 ObjectId 字符串提取时间戳
        /// </summary>
        /// <param name="objectId">ObjectId 字符串</param>
        /// <returns>时间戳</returns>
        public static DateTimeOffset ExtractTimestamp(string objectId)
        {
            return ExtractTimestamp(Decode(objectId));
        }

        /// <summary>
        /// 从 ObjectId 提取机器标识
        /// </summary>
        /// <param name="objectId">ObjectId 字节数组</param>
        /// <returns>机器标识（十六进制字符串）</returns>
        public static string ExtractMachineId(byte[] objectId)
        {
            if (objectId == null || objectId.Length != ObjectIdLength)
            {
                throw new ArgumentException($"ObjectId 字节数组长度必须为 {ObjectIdLength}", nameof(objectId));
            }

            return $"{objectId[4]:x2}{objectId[5]:x2}{objectId[6]:x2}";
        }

        /// <summary>
        /// 从 ObjectId 提取进程 ID
        /// </summary>
        /// <param name="objectId">ObjectId 字节数组</param>
        /// <returns>进程 ID</returns>
        public static int ExtractProcessId(byte[] objectId)
        {
            if (objectId == null || objectId.Length != ObjectIdLength)
            {
                throw new ArgumentException($"ObjectId 字节数组长度必须为 {ObjectIdLength}", nameof(objectId));
            }

            return (objectId[7] << 8) | objectId[8];
        }

        /// <summary>
        /// 从 ObjectId 提取计数器
        /// </summary>
        /// <param name="objectId">ObjectId 字节数组</param>
        /// <returns>计数器值</returns>
        public static int ExtractCounter(byte[] objectId)
        {
            if (objectId == null || objectId.Length != ObjectIdLength)
            {
                throw new ArgumentException($"ObjectId 字节数组长度必须为 {ObjectIdLength}", nameof(objectId));
            }

            return (objectId[9] << 16) | (objectId[10] << 8) | objectId[11];
        }

        /// <summary>
        /// 验证 ObjectId 字符串是否有效
        /// </summary>
        /// <param name="objectId">ObjectId 字符串</param>
        /// <returns>是否有效</returns>
        public static bool IsValid(string objectId)
        {
            if (string.IsNullOrEmpty(objectId) || objectId.Length != 24)
            {
                return false;
            }

            foreach (var c in objectId)
            {
                if (!Uri.IsHexDigit(c))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 比较两个 ObjectId 的大小
        /// </summary>
        /// <param name="a">第一个 ObjectId</param>
        /// <param name="b">第二个 ObjectId</param>
        /// <returns>比较结果</returns>
        public static int Compare(string a, string b)
        {
            return string.CompareOrdinal(a, b);
        }

        /// <summary>
        /// 获取 ObjectId 的信息
        /// </summary>
        /// <param name="objectId">ObjectId 字符串</param>
        /// <returns>ObjectId 信息</returns>
        public static ObjectIdInfo GetInfo(string objectId)
        {
            var bytes = Decode(objectId);
            return new ObjectIdInfo
            {
                Timestamp = ExtractTimestamp(bytes),
                MachineId = ExtractMachineId(bytes),
                ProcessId = ExtractProcessId(bytes),
                Counter = ExtractCounter(bytes)
            };
        }

        /// <summary>
        /// 生成最小 ObjectId（指定时间）
        /// </summary>
        /// <param name="timestamp">时间戳</param>
        /// <returns>最小 ObjectId 字符串</returns>
        public static string Min(DateTimeOffset timestamp)
        {
            var objectId = new byte[ObjectIdLength];
            var timestampSec = (int)timestamp.ToUnixTimeSeconds();

            objectId[0] = (byte)(timestampSec >> 24);
            objectId[1] = (byte)(timestampSec >> 16);
            objectId[2] = (byte)(timestampSec >> 8);
            objectId[3] = (byte)timestampSec;
            // 其余部分为 0

            return Encode(objectId);
        }

        /// <summary>
        /// 生成最大 ObjectId（指定时间）
        /// </summary>
        /// <param name="timestamp">时间戳</param>
        /// <returns>最大 ObjectId 字符串</returns>
        public static string Max(DateTimeOffset timestamp)
        {
            var objectId = new byte[ObjectIdLength];
            var timestampSec = (int)timestamp.ToUnixTimeSeconds();

            objectId[0] = (byte)(timestampSec >> 24);
            objectId[1] = (byte)(timestampSec >> 16);
            objectId[2] = (byte)(timestampSec >> 8);
            objectId[3] = (byte)timestampSec;
            // 其余部分为 0xFF
            for (int i = 4; i < ObjectIdLength; i++)
            {
                objectId[i] = 0xFF;
            }

            return Encode(objectId);
        }

        #region 私有方法

        private static byte[] GetMachineId()
        {
            var machineId = new byte[MachineIdLength];

            try
            {
                // 尝试使用网络接口的 MAC 地址
                var interfaces = NetworkInterface.GetAllNetworkInterfaces();
                foreach (var ni in interfaces)
                {
                    if (ni.OperationalStatus == OperationalStatus.Up &&
                        ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                    {
                        var mac = ni.GetPhysicalAddress().GetAddressBytes();
                        if (mac.Length >= MachineIdLength)
                        {
                            Buffer.BlockCopy(mac, 0, machineId, 0, MachineIdLength);
                            return machineId;
                        }
                    }
                }
            }
            catch
            {
                // 忽略异常，使用随机值
            }

            // 使用机器名哈希
            var machineName = Environment.MachineName;
            using var md5 = MD5.Create();
            var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(machineName));
            Buffer.BlockCopy(hash, 0, machineId, 0, MachineIdLength);

            return machineId;
        }

        private static byte[] GetProcessId()
        {
            var processId = new byte[ProcessIdLength];
#if NETSTANDARD2_1
            var pid = System.Diagnostics.Process.GetCurrentProcess().Id;
#else
            var pid = System.Diagnostics.Process.GetCurrentProcess().Id;
#endif

            processId[0] = (byte)(pid >> 8);
            processId[1] = (byte)pid;

            return processId;
        }

        private static int GetRandomCounter()
        {
            var counterBytes = new byte[CounterLength];
            _rng.GetBytes(counterBytes);
            return (counterBytes[0] << 16) | (counterBytes[1] << 8) | counterBytes[2];
        }

        #endregion
    }

    /// <summary>
    /// ObjectId 结构体
    /// </summary>
    public readonly struct ObjectId : IComparable<ObjectId>, IEquatable<ObjectId>
    {
        private readonly byte[] _bytes;

        /// <summary>
        /// 创建 ObjectId
        /// </summary>
        /// <param name="bytes">字节数组</param>
        public ObjectId(byte[] bytes)
        {
            if (bytes == null || bytes.Length != 12)
            {
                throw new ArgumentException("ObjectId 字节数组长度必须为 12", nameof(bytes));
            }
            _bytes = bytes;
        }

        /// <summary>
        /// 时间戳
        /// </summary>
        public DateTimeOffset Timestamp => ObjectIdUtil.ExtractTimestamp(_bytes);

        /// <summary>
        /// 字节数组
        /// </summary>
        public byte[] ToByteArray() => (byte[])_bytes.Clone();

        /// <summary>
        /// 转换为字符串
        /// </summary>
        public override string ToString() => ObjectIdUtil.Encode(_bytes);

        /// <summary>
        /// 比较大小
        /// </summary>
        public int CompareTo(ObjectId other)
        {
            for (int i = 0; i < 12; i++)
            {
                if (_bytes[i] != other._bytes[i])
                {
                    return _bytes[i].CompareTo(other._bytes[i]);
                }
            }
            return 0;
        }

        /// <summary>
        /// 判断相等
        /// </summary>
        public bool Equals(ObjectId other)
        {
            for (int i = 0; i < 12; i++)
            {
                if (_bytes[i] != other._bytes[i])
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 判断相等
        /// </summary>
        public override bool Equals(object? obj)
        {
            return obj is ObjectId other && Equals(other);
        }

        /// <summary>
        /// 获取哈希码
        /// </summary>
        public override int GetHashCode()
        {
            var hash = 0;
            for (int i = 0; i < 12; i++)
            {
                hash = (hash << 2) ^ _bytes[i];
            }
            return hash;
        }

        /// <summary>
        /// 等于运算符
        /// </summary>
        public static bool operator ==(ObjectId left, ObjectId right) => left.Equals(right);

        /// <summary>
        /// 不等于运算符
        /// </summary>
        public static bool operator !=(ObjectId left, ObjectId right) => !left.Equals(right);

        /// <summary>
        /// 小于运算符
        /// </summary>
        public static bool operator <(ObjectId left, ObjectId right) => left.CompareTo(right) < 0;

        /// <summary>
        /// 大于运算符
        /// </summary>
        public static bool operator >(ObjectId left, ObjectId right) => left.CompareTo(right) > 0;

        /// <summary>
        /// 小于等于运算符
        /// </summary>
        public static bool operator <=(ObjectId left, ObjectId right) => left.CompareTo(right) <= 0;

        /// <summary>
        /// 大于等于运算符
        /// </summary>
        public static bool operator >=(ObjectId left, ObjectId right) => left.CompareTo(right) >= 0;

        /// <summary>
        /// 生成新 ObjectId
        /// </summary>
        public static ObjectId NewObjectId() => new ObjectId(ObjectIdUtil.Generate());

        /// <summary>
        /// 解析字符串
        /// </summary>
        public static ObjectId Parse(string objectId) => new ObjectId(ObjectIdUtil.Decode(objectId));

        /// <summary>
        /// 尝试解析字符串
        /// </summary>
        public static bool TryParse(string objectId, out ObjectId result)
        {
            if (ObjectIdUtil.IsValid(objectId))
            {
                result = Parse(objectId);
                return true;
            }
            result = default;
            return false;
        }
    }

    /// <summary>
    /// ObjectId 信息
    /// </summary>
    public class ObjectIdInfo
    {
        /// <summary>
        /// 时间戳
        /// </summary>
        public DateTimeOffset Timestamp { get; set; }

        /// <summary>
        /// 机器标识
        /// </summary>
        public string MachineId { get; set; } = string.Empty;

        /// <summary>
        /// 进程 ID
        /// </summary>
        public int ProcessId { get; set; }

        /// <summary>
        /// 计数器
        /// </summary>
        public int Counter { get; set; }
    }
}
