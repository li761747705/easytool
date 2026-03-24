using System;

namespace EasyTool.CodeCategory
{
    /// <summary>
    /// Adler-32 校验和工具类
    /// Adler-32 是一种快速校验和算法，由 Mark Adler 发明
    /// 用于 zlib 压缩格式，比 CRC32 更快但可靠性略低
    /// </summary>
    public static class Adler32Util
    {
        private const uint ModAdler = 65521;

        /// <summary>
        /// 计算 Adler-32 校验和
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <returns>32位校验和</returns>
        public static uint Compute(byte[] data)
        {
            if (data == null || data.Length == 0)
                return 1;

            return Compute(data, 0, data.Length);
        }

        /// <summary>
        /// 计算 Adler-32 校验和
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <param name="offset">起始偏移</param>
        /// <param name="length">数据长度</param>
        /// <returns>32位校验和</returns>
        public static uint Compute(byte[] data, int offset, int length)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (offset < 0 || offset > data.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (length < 0 || offset + length > data.Length)
                throw new ArgumentOutOfRangeException(nameof(length));

            uint a = 1;
            uint b = 0;

            for (int i = offset; i < offset + length; i++)
            {
                a = (a + data[i]) % ModAdler;
                b = (b + a) % ModAdler;
            }

            return (b << 16) | a;
        }

        /// <summary>
        /// 继续计算 Adler-32（支持流式处理）
        /// </summary>
        /// <param name="previousChecksum">之前的校验和</param>
        /// <param name="data">新数据</param>
        /// <returns>更新后的校验和</returns>
        public static uint Continue(uint previousChecksum, byte[] data)
        {
            if (data == null || data.Length == 0)
                return previousChecksum;

            return Continue(previousChecksum, data, 0, data.Length);
        }

        /// <summary>
        /// 继续计算 Adler-32（支持流式处理）
        /// </summary>
        /// <param name="previousChecksum">之前的校验和</param>
        /// <param name="data">新数据</param>
        /// <param name="offset">起始偏移</param>
        /// <param name="length">数据长度</param>
        /// <returns>更新后的校验和</returns>
        public static uint Continue(uint previousChecksum, byte[] data, int offset, int length)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (offset < 0 || offset > data.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (length < 0 || offset + length > data.Length)
                throw new ArgumentOutOfRangeException(nameof(length));

            // 从之前的校验和中提取 a 和 b
            uint a = previousChecksum & 0xFFFF;
            uint b = (previousChecksum >> 16) & 0xFFFF;

            for (int i = offset; i < offset + length; i++)
            {
                a = (a + data[i]) % ModAdler;
                b = (b + a) % ModAdler;
            }

            return (b << 16) | a;
        }

        /// <summary>
        /// 计算字符串的 Adler-32 校验和
        /// </summary>
        /// <param name="text">文本</param>
        /// <returns>32位校验和</returns>
        public static uint ComputeString(string text)
        {
            if (string.IsNullOrEmpty(text))
                return 1;

            byte[] data = System.Text.Encoding.UTF8.GetBytes(text);
            return Compute(data);
        }

        /// <summary>
        /// 获取 Adler-32 校验和的十六进制表示
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <returns>8字符的十六进制字符串</returns>
        public static string ComputeHex(byte[] data)
        {
            uint checksum = Compute(data);
            return checksum.ToString("x8");
        }

        /// <summary>
        /// 验证数据的 Adler-32 校验和
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <param name="expectedChecksum">期望的校验和</param>
        /// <returns>是否匹配</returns>
        public static bool Verify(byte[] data, uint expectedChecksum)
        {
            return Compute(data) == expectedChecksum;
        }

        /// <summary>
        /// 验证数据的 Adler-32 校验和（十六进制格式）
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <param name="expectedHex">期望的十六进制校验和</param>
        /// <returns>是否匹配</returns>
        public static bool VerifyHex(byte[] data, string expectedHex)
        {
            if (string.IsNullOrEmpty(expectedHex))
                return false;

            string actual = ComputeHex(data);
            return string.Equals(actual, expectedHex, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 合并两个 Adler-32 校验和
        /// </summary>
        /// <param name="checksum1">第一个校验和</param>
        /// <param name="length1">第一个数据块的长度</param>
        /// <param name="checksum2">第二个校验和</param>
        /// <param name="length2">第二个数据块的长度</param>
        /// <returns>合并后的校验和</returns>
        public static uint Combine(uint checksum1, long length1, uint checksum2, long length2)
        {
            // 从校验和中提取 a 和 b
            uint a1 = checksum1 & 0xFFFF;
            uint b1 = (checksum1 >> 16) & 0xFFFF;
            uint a2 = checksum2 & 0xFFFF;
            uint b2 = (checksum2 >> 16) & 0xFFFF;

            // 计算合并后的 a 和 b
            uint a = (uint)((a1 + a2 * (ulong)LengthPower(length1)) % ModAdler);
            uint b = (uint)((b1 + b2 + a2 * (ulong)LengthSum(length1)) % ModAdler);

            return (b << 16) | a;
        }

        /// <summary>
        /// 获取初始校验和值
        /// </summary>
        /// <returns>初始值（1）</returns>
        public static uint InitialValue()
        {
            return 1;
        }

        #region 私有方法

        private static ulong LengthPower(long length)
        {
            // 计算 65521^length mod (2^32)
            ulong result = 1;
            ulong baseVal = ModAdler;

            while (length > 0)
            {
                if ((length & 1) == 1)
                {
                    result = (result * baseVal) % 0x100000000;
                }
                baseVal = (baseVal * baseVal) % 0x100000000;
                length >>= 1;
            }

            return result;
        }

        private static ulong LengthSum(long length)
        {
            // 计算 sum(65521^i) for i from 0 to length-1
            // 使用等比数列求和公式
            if (length == 0)
                return 0;

            ulong sum = 0;
            ulong power = 1;

            for (long i = 0; i < length; i++)
            {
                sum = (sum + power) % 0x100000000;
                power = (power * ModAdler) % 0x100000000;
            }

            return sum;
        }

        #endregion
    }
}
