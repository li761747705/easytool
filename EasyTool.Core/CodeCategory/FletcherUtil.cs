using System;

namespace EasyTool.CodeCategory
{
    /// <summary>
    /// Fletcher 校验和工具类
    /// Fletcher 是一种简单的校验和算法，由 John G. Fletcher 发明
    /// 包括 Fletcher-8、Fletcher-16、Fletcher-32 和 Fletcher-64 变体
    /// 比 Adler-32 简单，但检测能力略低
    /// </summary>
    public static class FletcherUtil
    {
        #region Fletcher-8

        /// <summary>
        /// 计算 Fletcher-8 校验和
        /// 使用 4 位累加器，生成 8 位校验和
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <returns>8位校验和</returns>
        public static byte Compute8(byte[] data)
        {
            if (data == null || data.Length == 0)
                return 0;

            return Compute8(data, 0, data.Length);
        }

        /// <summary>
        /// 计算 Fletcher-8 校验和
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <param name="offset">起始偏移</param>
        /// <param name="length">数据长度</param>
        /// <returns>8位校验和</returns>
        public static byte Compute8(byte[] data, int offset, int length)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (offset < 0 || offset > data.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (length < 0 || offset + length > data.Length)
                throw new ArgumentOutOfRangeException(nameof(length));

            byte sum1 = 0;
            byte sum2 = 0;
            const byte mod = 15;

            for (int i = offset; i < offset + length; i++)
            {
                sum1 = (byte)((sum1 + (data[i] >> 4)) % mod);
                sum2 = (byte)((sum2 + sum1) % mod);
                sum1 = (byte)((sum1 + (data[i] & 0x0F)) % mod);
                sum2 = (byte)((sum2 + sum1) % mod);
            }

            return (byte)((sum2 << 4) | sum1);
        }

        #endregion

        #region Fletcher-16

        /// <summary>
        /// 计算 Fletcher-16 校验和
        /// 使用 8 位累加器，生成 16 位校验和
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <returns>16位校验和</returns>
        public static ushort Compute16(byte[] data)
        {
            if (data == null || data.Length == 0)
                return 0;

            return Compute16(data, 0, data.Length);
        }

        /// <summary>
        /// 计算 Fletcher-16 校验和
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <param name="offset">起始偏移</param>
        /// <param name="length">数据长度</param>
        /// <returns>16位校验和</returns>
        public static ushort Compute16(byte[] data, int offset, int length)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (offset < 0 || offset > data.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (length < 0 || offset + length > data.Length)
                throw new ArgumentOutOfRangeException(nameof(length));

            ushort sum1 = 0;
            ushort sum2 = 0;
            const ushort mod = 255;

            for (int i = offset; i < offset + length; i++)
            {
                sum1 = (ushort)((sum1 + data[i]) % mod);
                sum2 = (ushort)((sum2 + sum1) % mod);
            }

            return (ushort)((sum2 << 8) | sum1);
        }

        /// <summary>
        /// 获取 Fletcher-16 校验和的十六进制表示
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <returns>4字符的十六进制字符串</returns>
        public static string Compute16Hex(byte[] data)
        {
            ushort checksum = Compute16(data);
            return checksum.ToString("x4");
        }

        #endregion

        #region Fletcher-32

        /// <summary>
        /// 计算 Fletcher-32 校验和
        /// 使用 16 位累加器，生成 32 位校验和
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <returns>32位校验和</returns>
        public static uint Compute32(byte[] data)
        {
            if (data == null || data.Length == 0)
                return 0;

            return Compute32(data, 0, data.Length);
        }

        /// <summary>
        /// 计算 Fletcher-32 校验和
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <param name="offset">起始偏移</param>
        /// <param name="length">数据长度</param>
        /// <returns>32位校验和</returns>
        public static uint Compute32(byte[] data, int offset, int length)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (offset < 0 || offset > data.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (length < 0 || offset + length > data.Length)
                throw new ArgumentOutOfRangeException(nameof(length));

            uint sum1 = 0;
            uint sum2 = 0;
            const uint mod = 65535;

            for (int i = offset; i < offset + length; i++)
            {
                sum1 = (sum1 + data[i]) % mod;
                sum2 = (sum2 + sum1) % mod;
            }

            return (sum2 << 16) | sum1;
        }

        /// <summary>
        /// 获取 Fletcher-32 校验和的十六进制表示
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <returns>8字符的十六进制字符串</returns>
        public static string Compute32Hex(byte[] data)
        {
            uint checksum = Compute32(data);
            return checksum.ToString("x8");
        }

        /// <summary>
        /// 继续计算 Fletcher-32（支持流式处理）
        /// </summary>
        /// <param name="previousChecksum">之前的校验和</param>
        /// <param name="data">新数据</param>
        /// <returns>更新后的校验和</returns>
        public static uint Continue32(uint previousChecksum, byte[] data)
        {
            if (data == null || data.Length == 0)
                return previousChecksum;

            uint sum1 = previousChecksum & 0xFFFF;
            uint sum2 = (previousChecksum >> 16) & 0xFFFF;
            const uint mod = 65535;

            foreach (byte b in data)
            {
                sum1 = (sum1 + b) % mod;
                sum2 = (sum2 + sum1) % mod;
            }

            return (sum2 << 16) | sum1;
        }

        #endregion

        #region Fletcher-64

        /// <summary>
        /// 计算 Fletcher-64 校验和
        /// 使用 32 位累加器，生成 64 位校验和
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <returns>64位校验和</returns>
        public static ulong Compute64(byte[] data)
        {
            if (data == null || data.Length == 0)
                return 0;

            return Compute64(data, 0, data.Length);
        }

        /// <summary>
        /// 计算 Fletcher-64 校验和
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <param name="offset">起始偏移</param>
        /// <param name="length">数据长度</param>
        /// <returns>64位校验和</returns>
        public static ulong Compute64(byte[] data, int offset, int length)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (offset < 0 || offset > data.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (length < 0 || offset + length > data.Length)
                throw new ArgumentOutOfRangeException(nameof(length));

            ulong sum1 = 0;
            ulong sum2 = 0;
            const ulong mod = 4294967295;

            for (int i = offset; i < offset + length; i++)
            {
                sum1 = (sum1 + data[i]) % mod;
                sum2 = (sum2 + sum1) % mod;
            }

            return (sum2 << 32) | sum1;
        }

        /// <summary>
        /// 获取 Fletcher-64 校验和的十六进制表示
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <returns>16字符的十六进制字符串</returns>
        public static string Compute64Hex(byte[] data)
        {
            ulong checksum = Compute64(data);
            return checksum.ToString("x16");
        }

        #endregion

        #region 通用方法

        /// <summary>
        /// 计算字符串的 Fletcher-16 校验和
        /// </summary>
        /// <param name="text">文本</param>
        /// <returns>16位校验和</returns>
        public static ushort Compute16String(string text)
        {
            if (string.IsNullOrEmpty(text))
                return 0;

            byte[] data = System.Text.Encoding.UTF8.GetBytes(text);
            return Compute16(data);
        }

        /// <summary>
        /// 计算字符串的 Fletcher-32 校验和
        /// </summary>
        /// <param name="text">文本</param>
        /// <returns>32位校验和</returns>
        public static uint Compute32String(string text)
        {
            if (string.IsNullOrEmpty(text))
                return 0;

            byte[] data = System.Text.Encoding.UTF8.GetBytes(text);
            return Compute32(data);
        }

        /// <summary>
        /// 验证数据的 Fletcher-16 校验和
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <param name="expectedChecksum">期望的校验和</param>
        /// <returns>是否匹配</returns>
        public static bool Verify16(byte[] data, ushort expectedChecksum)
        {
            return Compute16(data) == expectedChecksum;
        }

        /// <summary>
        /// 验证数据的 Fletcher-32 校验和
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <param name="expectedChecksum">期望的校验和</param>
        /// <returns>是否匹配</returns>
        public static bool Verify32(byte[] data, uint expectedChecksum)
        {
            return Compute32(data) == expectedChecksum;
        }

        /// <summary>
        /// 验证数据的 Fletcher-64 校验和
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <param name="expectedChecksum">期望的校验和</param>
        /// <returns>是否匹配</returns>
        public static bool Verify64(byte[] data, ulong expectedChecksum)
        {
            return Compute64(data) == expectedChecksum;
        }

        /// <summary>
        /// 验证数据的 Fletcher-32 校验和（十六进制格式）
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <param name="expectedHex">期望的十六进制校验和</param>
        /// <returns>是否匹配</returns>
        public static bool Verify32Hex(byte[] data, string expectedHex)
        {
            if (string.IsNullOrEmpty(expectedHex))
                return false;

            string actual = Compute32Hex(data);
            return string.Equals(actual, expectedHex, StringComparison.OrdinalIgnoreCase);
        }

        #endregion
    }
}
