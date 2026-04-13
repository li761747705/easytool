using System;
using System.Security.Cryptography;
using System.Text;

namespace EasyTool.CodeCategory
{
    /// <summary>
    /// XID 全局唯一ID工具类
    /// XID 是一个全局唯一、时间排序的ID生成器，与MongoDB ObjectId兼容
    /// 格式：4字节时间戳 + 3字节机器ID + 2字节进程ID + 3字节计数器 = 12字节
    /// </summary>
    public static class XidUtil
    {
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private static readonly byte[] MachineId;
        private static readonly ushort ProcessId;
        private static int _counter;
        private static readonly object _lock = new object();

        // Base32 编码字符集（小写）
        private const string Base32Chars = "0123456789abcdefghijklmnopqrstuv";

        static XidUtil()
        {
            // 生成机器ID（3字节）
            MachineId = new byte[3];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(MachineId);

            // 获取进程ID
            ProcessId = (ushort)Environment.CurrentManagedThreadId;

            // 初始化计数器
            var counterBytes = new byte[3];
            rng.GetBytes(counterBytes);
            _counter = (counterBytes[0] << 16) | (counterBytes[1] << 8) | counterBytes[2];
        }

        /// <summary>
        /// 生成新的 XID
        /// </summary>
        /// <returns>12字节的 XID</returns>
        public static byte[] Generate()
        {
            return Generate(DateTimeOffset.UtcNow);
        }

        /// <summary>
        /// 生成指定时间的 XID
        /// </summary>
        /// <param name="timestamp">时间戳</param>
        /// <returns>12字节的 XID</returns>
        public static byte[] Generate(DateTimeOffset timestamp)
        {
            var bytes = new byte[12];

            // 4字节时间戳（大端序）
            uint time = (uint)(timestamp.ToUnixTimeSeconds());
            bytes[0] = (byte)(time >> 24);
            bytes[1] = (byte)(time >> 16);
            bytes[2] = (byte)(time >> 8);
            bytes[3] = (byte)time;

            // 3字节机器ID
            bytes[4] = MachineId[0];
            bytes[5] = MachineId[1];
            bytes[6] = MachineId[2];

            // 2字节进程ID（大端序）
            bytes[7] = (byte)(ProcessId >> 8);
            bytes[8] = (byte)ProcessId;

            // 3字节计数器（大端序）
            int counter;
            lock (_lock)
            {
                counter = _counter++;
            }

            bytes[9] = (byte)(counter >> 16);
            bytes[10] = (byte)(counter >> 8);
            bytes[11] = (byte)counter;

            return bytes;
        }

        /// <summary>
        /// 生成 XID 字符串（20个字符的 Base32 编码）
        /// </summary>
        /// <returns>20字符的 XID 字符串</returns>
        public static string GenerateString()
        {
            byte[] bytes = Generate();
            return Encode(bytes);
        }

        /// <summary>
        /// 生成指定时间的 XID 字符串
        /// </summary>
        /// <param name="timestamp">时间戳</param>
        /// <returns>20字符的 XID 字符串</returns>
        public static string GenerateString(DateTimeOffset timestamp)
        {
            byte[] bytes = Generate(timestamp);
            return Encode(bytes);
        }

        /// <summary>
        /// 将 XID 编码为字符串
        /// </summary>
        /// <param name="bytes">12字节的 XID</param>
        /// <returns>20字符的 Base32 字符串</returns>
        public static string Encode(byte[] bytes)
        {
            if (bytes == null || bytes.Length != 12)
                throw new ArgumentException("XID must be 12 bytes", nameof(bytes));

            // 使用自定义 Base32 编码
            char[] result = new char[20];

            ulong value = ((ulong)bytes[0] << 32) | ((ulong)bytes[1] << 24) |
                          ((ulong)bytes[2] << 16) | ((ulong)bytes[3] << 8) | bytes[4];

            result[0] = Base32Chars[(int)((value >> 35) & 0x1f)];
            result[1] = Base32Chars[(int)((value >> 30) & 0x1f)];
            result[2] = Base32Chars[(int)((value >> 25) & 0x1f)];
            result[3] = Base32Chars[(int)((value >> 20) & 0x1f)];
            result[4] = Base32Chars[(int)((value >> 15) & 0x1f)];
            result[5] = Base32Chars[(int)((value >> 10) & 0x1f)];
            result[6] = Base32Chars[(int)((value >> 5) & 0x1f)];
            result[7] = Base32Chars[(int)(value & 0x1f)];

            value = ((ulong)bytes[5] << 36) | ((ulong)bytes[6] << 28) |
                    ((ulong)bytes[7] << 20) | ((ulong)bytes[8] << 12) |
                    ((ulong)bytes[9] << 4) | ((ulong)bytes[10] >> 4);

            result[8] = Base32Chars[(int)((value >> 35) & 0x1f)];
            result[9] = Base32Chars[(int)((value >> 30) & 0x1f)];
            result[10] = Base32Chars[(int)((value >> 25) & 0x1f)];
            result[11] = Base32Chars[(int)((value >> 20) & 0x1f)];
            result[12] = Base32Chars[(int)((value >> 15) & 0x1f)];
            result[13] = Base32Chars[(int)((value >> 10) & 0x1f)];
            result[14] = Base32Chars[(int)((value >> 5) & 0x1f)];
            result[15] = Base32Chars[(int)(value & 0x1f)];

            value = ((ulong)(bytes[10] & 0x0f) << 32) | ((ulong)bytes[11] << 24);

            result[16] = Base32Chars[(int)((value >> 30) & 0x1f)];
            result[17] = Base32Chars[(int)((value >> 25) & 0x1f)];
            result[18] = Base32Chars[(int)((value >> 20) & 0x1f)];
            result[19] = Base32Chars[(int)((value >> 15) & 0x1f)];

            return new string(result);
        }

        /// <summary>
        /// 将 XID 字符串解码为字节数组
        /// </summary>
        /// <param name="xid">20字符的 XID 字符串</param>
        /// <returns>12字节的 XID</returns>
        public static byte[] Decode(string xid)
        {
            if (string.IsNullOrEmpty(xid) || xid.Length != 20)
                throw new ArgumentException("XID string must be 20 characters", nameof(xid));

            byte[] result = new byte[12];

            // 构建 Base32 解码映射
            int[] decodeMap = new int[128];
            for (int i = 0; i < 128; i++) decodeMap[i] = -1;
            for (int i = 0; i < Base32Chars.Length; i++)
            {
                decodeMap[Base32Chars[i]] = i;
                decodeMap[char.ToUpperInvariant(Base32Chars[i])] = i;
            }

            // 解码
            int DecodeChar(char c)
            {
                int v = c < 128 ? decodeMap[c] : -1;
                if (v < 0) throw new ArgumentException($"Invalid character: {c}");
                return v;
            }

            int v0 = DecodeChar(xid[0]);
            int v1 = DecodeChar(xid[1]);
            int v2 = DecodeChar(xid[2]);
            int v3 = DecodeChar(xid[3]);
            int v4 = DecodeChar(xid[4]);
            int v5 = DecodeChar(xid[5]);
            int v6 = DecodeChar(xid[6]);
            int v7 = DecodeChar(xid[7]);

            result[0] = (byte)((v0 << 3) | (v1 >> 2));
            result[1] = (byte)((v1 << 6) | (v2 << 1) | (v3 >> 4));
            result[2] = (byte)((v3 << 4) | (v4 >> 1));
            result[3] = (byte)((v4 << 7) | (v5 << 2) | (v6 >> 3));
            result[4] = (byte)((v6 << 5) | v7);

            int v8 = DecodeChar(xid[8]);
            int v9 = DecodeChar(xid[9]);
            int v10 = DecodeChar(xid[10]);
            int v11 = DecodeChar(xid[11]);
            int v12 = DecodeChar(xid[12]);
            int v13 = DecodeChar(xid[13]);
            int v14 = DecodeChar(xid[14]);
            int v15 = DecodeChar(xid[15]);

            result[5] = (byte)((v8 << 3) | (v9 >> 2));
            result[6] = (byte)((v9 << 6) | (v10 << 1) | (v11 >> 4));
            result[7] = (byte)((v11 << 4) | (v12 >> 1));
            result[8] = (byte)((v12 << 7) | (v13 << 2) | (v14 >> 3));
            result[9] = (byte)((v14 << 5) | v15);

            int v16 = DecodeChar(xid[16]);
            int v17 = DecodeChar(xid[17]);
            int v18 = DecodeChar(xid[18]);
            int v19 = DecodeChar(xid[19]);

            result[10] = (byte)((v16 << 3) | (v17 >> 2));
            result[11] = (byte)((v17 << 6) | (v18 << 1) | (v19 >> 4));

            return result;
        }

        /// <summary>
        /// 从 XID 提取时间戳
        /// </summary>
        /// <param name="xid">XID 字节数组或字符串</param>
        /// <returns>UTC 时间</returns>
        public static DateTimeOffset ExtractTimestamp(byte[] xid)
        {
            if (xid == null || xid.Length != 12)
                throw new ArgumentException("XID must be 12 bytes", nameof(xid));

            uint time = ((uint)xid[0] << 24) | ((uint)xid[1] << 16) |
                        ((uint)xid[2] << 8) | xid[3];

            return Epoch.AddSeconds(time);
        }

        /// <summary>
        /// 从 XID 字符串提取时间戳
        /// </summary>
        /// <param name="xid">XID 字符串</param>
        /// <returns>UTC 时间</returns>
        public static DateTimeOffset ExtractTimestamp(string xid)
        {
            byte[] bytes = Decode(xid);
            return ExtractTimestamp(bytes);
        }

        /// <summary>
        /// 验证 XID 字符串是否有效
        /// </summary>
        /// <param name="xid">XID 字符串</param>
        /// <returns>是否有效</returns>
        public static bool IsValid(string xid)
        {
            if (string.IsNullOrEmpty(xid) || xid.Length != 20)
                return false;

            foreach (char c in xid)
            {
                if (!Base32Chars.Contains(char.ToLowerInvariant(c)))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 尝试解析 XID 字符串
        /// </summary>
        /// <param name="xid">XID 字符串</param>
        /// <param name="bytes">输出的字节数组</param>
        /// <returns>是否解析成功</returns>
        public static bool TryParse(string xid, out byte[] bytes)
        {
            bytes = null;
            if (!IsValid(xid))
                return false;

            try
            {
                bytes = Decode(xid);
                return true;
            }
            // 捕获 XID 解码格式异常
            catch (FormatException)
            {
                return false;
            }
        }

        /// <summary>
        /// 批量生成 XID
        /// </summary>
        /// <param name="count">生成数量</param>
        /// <returns>XID 字符串数组</returns>
        public static string[] GenerateBatch(int count)
        {
            if (count <= 0)
                throw new ArgumentException("Count must be greater than 0", nameof(count));

            var result = new string[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = GenerateString();
            }
            return result;
        }

        /// <summary>
        /// 比较 XID 的时间顺序
        /// </summary>
        /// <param name="xid1">第一个 XID</param>
        /// <param name="xid2">第二个 XID</param>
        /// <returns>-1: xid1早于xid2, 0: 相同, 1: xid1晚于xid2</returns>
        public static int Compare(string xid1, string xid2)
        {
            return string.Compare(xid1, xid2, StringComparison.Ordinal);
        }
    }
}
