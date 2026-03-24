using System;

namespace EasyTool.CodeCategory
{
    /// <summary>
    /// CRC（循环冗余校验）工具类
    /// 支持 CRC8、CRC16、CRC32 等多种 CRC 算法
    /// </summary>
    public static class CrcUtil
    {
        #region CRC8

        // CRC8 查找表
        private static readonly byte[] Crc8Table = BuildCrc8Table(0x07); // CRC-8

        // CRC8-CCITT 查找表
        private static readonly byte[] Crc8CcittTable = BuildCrc8Table(0x07);

        // CRC8-MAXIM 查找表
        private static readonly byte[] Crc8MaximTable = BuildCrc8Table(0x31);

        // CRC8-ROHC 查找表
        private static readonly byte[] Crc8RohcTable = BuildCrc8Table(0x07);

        /// <summary>
        /// 计算 CRC8 校验值
        /// </summary>
        /// <param name="data">数据</param>
        /// <returns>CRC8 值</returns>
        public static byte Crc8(byte[] data)
        {
            return Crc8(data, 0, data.Length);
        }

        /// <summary>
        /// 计算 CRC8 校验值
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="offset">起始位置</param>
        /// <param name="length">长度</param>
        /// <returns>CRC8 值</returns>
        public static byte Crc8(byte[] data, int offset, int length)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (offset < 0 || offset >= data.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (length < 0 || offset + length > data.Length)
                throw new ArgumentOutOfRangeException(nameof(length));

            byte crc = 0;
            for (int i = offset; i < offset + length; i++)
            {
                crc = Crc8Table[crc ^ data[i]];
            }
            return crc;
        }

        /// <summary>
        /// 计算 CRC8-CCITT 校验值
        /// </summary>
        /// <param name="data">数据</param>
        /// <returns>CRC8-CCITT 值</returns>
        public static byte Crc8Ccitt(byte[] data)
        {
            byte crc = 0;
            foreach (byte b in data)
            {
                crc = Crc8CcittTable[crc ^ b];
            }
            return crc;
        }

        /// <summary>
        /// 计算 CRC8-MAXIM 校验值
        /// </summary>
        /// <param name="data">数据</param>
        /// <returns>CRC8-MAXIM 值</returns>
        public static byte Crc8Maxim(byte[] data)
        {
            byte crc = 0;
            foreach (byte b in data)
            {
                crc = Crc8MaximTable[crc ^ b];
            }
            return crc;
        }

        private static byte[] BuildCrc8Table(byte polynomial)
        {
            var table = new byte[256];
            for (int i = 0; i < 256; i++)
            {
                byte crc = (byte)i;
                for (int j = 0; j < 8; j++)
                {
                    crc = (crc & 0x80) != 0 ? (byte)((crc << 1) ^ polynomial) : (byte)(crc << 1);
                }
                table[i] = crc;
            }
            return table;
        }

        #endregion

        #region CRC16

        // CRC16-CCITT 查找表
        private static readonly ushort[] Crc16CcittTable = BuildCrc16Table(0x1021);

        // CRC16-MODBUS 查找表
        private static readonly ushort[] Crc16ModbusTable = BuildCrc16Table(0xA001);

        // CRC16-IBM 查找表
        private static readonly ushort[] Crc16IbmTable = BuildCrc16Table(0x8005);

        // CRC16-USB 查找表
        private static readonly ushort[] Crc16UsbTable = BuildCrc16Table(0xA001);

        /// <summary>
        /// 计算 CRC16-CCITT 校验值
        /// </summary>
        /// <param name="data">数据</param>
        /// <returns>CRC16-CCITT 值</returns>
        public static ushort Crc16Ccitt(byte[] data)
        {
            return Crc16Ccitt(data, 0, data.Length);
        }

        /// <summary>
        /// 计算 CRC16-CCITT 校验值
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="offset">起始位置</param>
        /// <param name="length">长度</param>
        /// <returns>CRC16-CCITT 值</returns>
        public static ushort Crc16Ccitt(byte[] data, int offset, int length)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            ushort crc = 0xFFFF;
            for (int i = offset; i < offset + length; i++)
            {
                crc = (ushort)((crc << 8) ^ Crc16CcittTable[(crc >> 8) ^ data[i]]);
            }
            return crc;
        }

        /// <summary>
        /// 计算 CRC16-MODBUS 校验值
        /// </summary>
        /// <param name="data">数据</param>
        /// <returns>CRC16-MODBUS 值</returns>
        public static ushort Crc16Modbus(byte[] data)
        {
            return Crc16Modbus(data, 0, data.Length);
        }

        /// <summary>
        /// 计算 CRC16-MODBUS 校验值
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="offset">起始位置</param>
        /// <param name="length">长度</param>
        /// <returns>CRC16-MODBUS 值</returns>
        public static ushort Crc16Modbus(byte[] data, int offset, int length)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            ushort crc = 0xFFFF;
            for (int i = offset; i < offset + length; i++)
            {
                crc = (ushort)((crc >> 8) ^ Crc16ModbusTable[(crc ^ data[i]) & 0xFF]);
            }
            return crc;
        }

        /// <summary>
        /// 计算 CRC16-IBM 校验值
        /// </summary>
        /// <param name="data">数据</param>
        /// <returns>CRC16-IBM 值</returns>
        public static ushort Crc16Ibm(byte[] data)
        {
            ushort crc = 0;
            foreach (byte b in data)
            {
                crc = (ushort)((crc >> 8) ^ Crc16IbmTable[(crc ^ b) & 0xFF]);
            }
            return crc;
        }

        /// <summary>
        /// 计算 CRC16-USB 校验值
        /// </summary>
        /// <param name="data">数据</param>
        /// <returns>CRC16-USB 值</returns>
        public static ushort Crc16Usb(byte[] data)
        {
            ushort crc = 0xFFFF;
            foreach (byte b in data)
            {
                crc = (ushort)((crc >> 8) ^ Crc16UsbTable[(crc ^ b) & 0xFF]);
            }
            return (ushort)~crc;
        }

        private static ushort[] BuildCrc16Table(ushort polynomial)
        {
            var table = new ushort[256];
            for (int i = 0; i < 256; i++)
            {
                ushort crc = (ushort)i;
                for (int j = 0; j < 8; j++)
                {
                    crc = (crc & 1) != 0 ? (ushort)((crc >> 1) ^ polynomial) : (ushort)(crc >> 1);
                }
                table[i] = crc;
            }
            return table;
        }

        #endregion

        #region CRC32

        // CRC32 查找表（IEEE 802.3）
        private static readonly uint[] Crc32Table = BuildCrc32Table(0xEDB88320);

        // CRC32-MPEG2 查找表
        private static readonly uint[] Crc32Mpeg2Table = BuildCrc32Table(0x04C11DB7);

        // CRC32C (Castagnoli) 查找表
        private static readonly uint[] Crc32CTable = BuildCrc32Table(0x82F63B78);

        /// <summary>
        /// 计算 CRC32 校验值（IEEE 802.3）
        /// </summary>
        /// <param name="data">数据</param>
        /// <returns>CRC32 值</returns>
        public static uint Crc32(byte[] data)
        {
            return Crc32(data, 0, data.Length);
        }

        /// <summary>
        /// 计算 CRC32 校验值（IEEE 802.3）
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="offset">起始位置</param>
        /// <param name="length">长度</param>
        /// <returns>CRC32 值</returns>
        public static uint Crc32(byte[] data, int offset, int length)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            uint crc = 0xFFFFFFFF;
            for (int i = offset; i < offset + length; i++)
            {
                crc = (crc >> 8) ^ Crc32Table[(crc ^ data[i]) & 0xFF];
            }
            return crc ^ 0xFFFFFFFF;
        }

        /// <summary>
        /// 计算 CRC32-MPEG2 校验值
        /// </summary>
        /// <param name="data">数据</param>
        /// <returns>CRC32-MPEG2 值</returns>
        public static uint Crc32Mpeg2(byte[] data)
        {
            uint crc = 0xFFFFFFFF;
            foreach (byte b in data)
            {
                crc = (crc << 8) ^ Crc32Mpeg2Table[((crc >> 24) ^ b) & 0xFF];
            }
            return crc;
        }

        /// <summary>
        /// 计算 CRC32C (Castagnoli) 校验值
        /// </summary>
        /// <param name="data">数据</param>
        /// <returns>CRC32C 值</returns>
        public static uint Crc32C(byte[] data)
        {
            uint crc = 0xFFFFFFFF;
            foreach (byte b in data)
            {
                crc = (crc >> 8) ^ Crc32CTable[(crc ^ b) & 0xFF];
            }
            return crc ^ 0xFFFFFFFF;
        }

        private static uint[] BuildCrc32Table(uint polynomial)
        {
            var table = new uint[256];
            for (int i = 0; i < 256; i++)
            {
                uint crc = (uint)i;
                for (int j = 0; j < 8; j++)
                {
                    crc = (crc & 1) != 0 ? (crc >> 1) ^ polynomial : crc >> 1;
                }
                table[i] = crc;
            }
            return table;
        }

        #endregion

        #region CRC64

        // CRC64-ECMA 查找表
        private static readonly ulong[] Crc64Table = BuildCrc64Table(0xC96C5795D7870F42);

        /// <summary>
        /// 计算 CRC64-ECMA 校验值
        /// </summary>
        /// <param name="data">数据</param>
        /// <returns>CRC64 值</returns>
        public static ulong Crc64(byte[] data)
        {
            return Crc64(data, 0, data.Length);
        }

        /// <summary>
        /// 计算 CRC64-ECMA 校验值
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="offset">起始位置</param>
        /// <param name="length">长度</param>
        /// <returns>CRC64 值</returns>
        public static ulong Crc64(byte[] data, int offset, int length)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            ulong crc = 0;
            for (int i = offset; i < offset + length; i++)
            {
                crc = Crc64Table[(crc ^ data[i]) & 0xFF] ^ (crc >> 8);
            }
            return crc;
        }

        private static ulong[] BuildCrc64Table(ulong polynomial)
        {
            var table = new ulong[256];
            for (int i = 0; i < 256; i++)
            {
                ulong crc = (ulong)i;
                for (int j = 0; j < 8; j++)
                {
                    crc = (crc & 1) != 0 ? (crc >> 1) ^ polynomial : crc >> 1;
                }
                table[i] = crc;
            }
            return table;
        }

        #endregion

        #region 通用方法

        /// <summary>
        /// 计算校验值并返回十六进制字符串
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="algorithm">算法：CRC8, CRC16, CRC32, CRC64</param>
        /// <returns>十六进制字符串</returns>
        public static string ComputeHex(byte[] data, string algorithm = "CRC32")
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            switch (algorithm.ToUpperInvariant())
            {
                case "CRC8":
                    return Crc8(data).ToString("X2");
                case "CRC8-CCITT":
                    return Crc8Ccitt(data).ToString("X2");
                case "CRC8-MAXIM":
                    return Crc8Maxim(data).ToString("X2");
                case "CRC16":
                case "CRC16-CCITT":
                    return Crc16Ccitt(data).ToString("X4");
                case "CRC16-MODBUS":
                    return Crc16Modbus(data).ToString("X4");
                case "CRC16-IBM":
                    return Crc16Ibm(data).ToString("X4");
                case "CRC16-USB":
                    return Crc16Usb(data).ToString("X4");
                case "CRC32":
                    return Crc32(data).ToString("X8");
                case "CRC32-MPEG2":
                    return Crc32Mpeg2(data).ToString("X8");
                case "CRC32C":
                    return Crc32C(data).ToString("X8");
                case "CRC64":
                    return Crc64(data).ToString("X16");
                default:
                    throw new ArgumentException($"Unknown CRC algorithm: {algorithm}", nameof(algorithm));
            }
        }

        /// <summary>
        /// 验证数据校验值
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="expectedCrc">预期的 CRC 值（十六进制字符串）</param>
        /// <param name="algorithm">算法</param>
        /// <returns>是否匹配</returns>
        public static bool Verify(byte[] data, string expectedCrc, string algorithm = "CRC32")
        {
            string computed = ComputeHex(data, algorithm);
            return string.Equals(computed, expectedCrc, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 验证 CRC32 校验值
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="expectedCrc">预期的 CRC32 值</param>
        /// <returns>是否匹配</returns>
        public static bool VerifyCrc32(byte[] data, uint expectedCrc)
        {
            return Crc32(data) == expectedCrc;
        }

        /// <summary>
        /// 验证 CRC16 校验值
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="expectedCrc">预期的 CRC16 值</param>
        /// <returns>是否匹配</returns>
        public static bool VerifyCrc16(byte[] data, ushort expectedCrc)
        {
            return Crc16Ccitt(data) == expectedCrc;
        }

        #endregion
    }
}
