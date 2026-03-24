using System;

namespace EasyTool.CodeCategory
{
    /// <summary>
    /// 格雷码（Gray Code）工具类
    /// 格雷码是一种二进制数系统，相邻的两个数之间只有一个位不同
    /// 由 Frank Gray 发明，常用于位置编码、错误检测等
    /// </summary>
    public static class GrayCodeUtil
    {
        /// <summary>
        /// 将二进制数转换为格雷码
        /// </summary>
        /// <param name="binary">二进制数</param>
        /// <returns>格雷码</returns>
        public static uint BinaryToGray(uint binary)
        {
            return binary ^ (binary >> 1);
        }

        /// <summary>
        /// 将格雷码转换为二进制数
        /// </summary>
        /// <param name="gray">格雷码</param>
        /// <returns>二进制数</returns>
        public static uint GrayToBinary(uint gray)
        {
            uint binary = gray;
            binary ^= (binary >> 1);
            binary ^= (binary >> 2);
            binary ^= (binary >> 4);
            binary ^= (binary >> 8);
            binary ^= (binary >> 16);
            return binary;
        }

        /// <summary>
        /// 将二进制数转换为格雷码（64位）
        /// </summary>
        /// <param name="binary">二进制数</param>
        /// <returns>格雷码</returns>
        public static ulong BinaryToGray64(ulong binary)
        {
            return binary ^ (binary >> 1);
        }

        /// <summary>
        /// 将格雷码转换为二进制数（64位）
        /// </summary>
        /// <param name="gray">格雷码</param>
        /// <returns>二进制数</returns>
        public static ulong GrayToBinary64(ulong gray)
        {
            ulong binary = gray;
            binary ^= (binary >> 1);
            binary ^= (binary >> 2);
            binary ^= (binary >> 4);
            binary ^= (binary >> 8);
            binary ^= (binary >> 16);
            binary ^= (binary >> 32);
            return binary;
        }

        /// <summary>
        /// 将整数转换为格雷码二进制字符串
        /// </summary>
        /// <param name="value">整数值</param>
        /// <param name="bits">位数</param>
        /// <returns>格雷码二进制字符串</returns>
        public static string ToGrayBinaryString(int value, int bits = 8)
        {
            if (bits < 1 || bits > 32)
                throw new ArgumentException("Bits must be between 1 and 32", nameof(bits));

            uint gray = BinaryToGray((uint)value);
            return Convert.ToString(gray, 2).PadLeft(bits, '0');
        }

        /// <summary>
        /// 生成 n 位格雷码序列
        /// </summary>
        /// <param name="n">位数</param>
        /// <returns>格雷码序列</returns>
        public static uint[] GenerateSequence(int n)
        {
            if (n < 1 || n > 32)
                throw new ArgumentException("N must be between 1 and 32", nameof(n));

            int count = 1 << n;
            uint[] result = new uint[count];

            for (int i = 0; i < count; i++)
            {
                result[i] = BinaryToGray((uint)i);
            }

            return result;
        }

        /// <summary>
        /// 生成 n 位格雷码二进制字符串序列
        /// </summary>
        /// <param name="n">位数</param>
        /// <returns>格雷码二进制字符串序列</returns>
        public static string[] GenerateBinarySequence(int n)
        {
            if (n < 1 || n > 32)
                throw new ArgumentException("N must be between 1 and 32", nameof(n));

            int count = 1 << n;
            string[] result = new string[count];

            for (int i = 0; i < count; i++)
            {
                uint gray = BinaryToGray((uint)i);
                result[i] = Convert.ToString(gray, 2).PadLeft(n, '0');
            }

            return result;
        }

        /// <summary>
        /// 计算两个格雷码之间的汉明距离
        /// </summary>
        /// <param name="gray1">第一个格雷码</param>
        /// <param name="gray2">第二个格雷码</param>
        /// <returns>汉明距离</returns>
        public static int HammingDistance(uint gray1, uint gray2)
        {
            // 格雷码的汉明距离等于它们的异或值的位数
            uint xor = gray1 ^ gray2;
            int distance = 0;

            while (xor != 0)
            {
                distance++;
                xor &= (xor - 1); // 清除最低位1
            }

            return distance;
        }

        /// <summary>
        /// 检查两个格雷码是否相邻
        /// </summary>
        /// <param name="gray1">第一个格雷码</param>
        /// <param name="gray2">第二个格雷码</param>
        /// <returns>是否相邻</returns>
        public static bool AreAdjacent(uint gray1, uint gray2)
        {
            return HammingDistance(gray1, gray2) == 1;
        }

        /// <summary>
        /// 获取格雷码的下一个值
        /// </summary>
        /// <param name="gray">当前格雷码</param>
        /// <returns>下一个格雷码</returns>
        public static uint Next(uint gray)
        {
            uint binary = GrayToBinary(gray);
            return BinaryToGray(binary + 1);
        }

        /// <summary>
        /// 获取格雷码的前一个值
        /// </summary>
        /// <param name="gray">当前格雷码</param>
        /// <returns>前一个格雷码</returns>
        public static uint Previous(uint gray)
        {
            uint binary = GrayToBinary(gray);
            if (binary == 0)
                return 0;

            return BinaryToGray(binary - 1);
        }

        /// <summary>
        /// 计算格雷码的奇偶性
        /// </summary>
        /// <param name="gray">格雷码</param>
        /// <returns>奇偶性（true = 奇数，false = 偶数）</returns>
        public static bool Parity(uint gray)
        {
            // 格雷码的奇偶性与最高位相同
            uint binary = GrayToBinary(gray);
            return (binary & 1) == 1;
        }

        /// <summary>
        /// 将字节转换为格雷码
        /// </summary>
        /// <param name="data">字节数据</param>
        /// <returns>格雷码字节</returns>
        public static byte ByteToGray(byte data)
        {
            return (byte)BinaryToGray(data);
        }

        /// <summary>
        /// 将格雷码字节转换为普通字节
        /// </summary>
        /// <param name="gray">格雷码字节</param>
        /// <returns>普通字节</returns>
        public static byte GrayToByte(byte gray)
        {
            return (byte)GrayToBinary(gray);
        }

        /// <summary>
        /// 将字节数组转换为格雷码
        /// </summary>
        /// <param name="data">字节数组</param>
        /// <returns>格雷码字节数组</returns>
        public static byte[] EncodeBytes(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            byte[] result = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                result[i] = ByteToGray(data[i]);
            }
            return result;
        }

        /// <summary>
        /// 将格雷码字节数组转换为普通字节数组
        /// </summary>
        /// <param name="grayData">格雷码字节数组</param>
        /// <returns>普通字节数组</returns>
        public static byte[] DecodeBytes(byte[] grayData)
        {
            if (grayData == null)
                throw new ArgumentNullException(nameof(grayData));

            byte[] result = new byte[grayData.Length];
            for (int i = 0; i < grayData.Length; i++)
            {
                result[i] = GrayToByte(grayData[i]);
            }
            return result;
        }

        /// <summary>
        /// 计算格雷码的位翻转位置（相对于前一个值）
        /// </summary>
        /// <param name="gray">格雷码</param>
        /// <returns>位翻转位置（0-based），如果是0则返回-1</returns>
        public static int GetBitFlipPosition(uint gray)
        {
            if (gray == 0)
                return -1;

            // 找到最低位1的位置
            int position = 0;
            uint temp = gray;

            while ((temp & 1) == 0)
            {
                temp >>= 1;
                position++;
            }

            return position;
        }

        /// <summary>
        /// 获取格雷码对应的十进制值
        /// </summary>
        /// <param name="gray">格雷码</param>
        /// <returns>十进制值</returns>
        public static uint ToDecimal(uint gray)
        {
            return GrayToBinary(gray);
        }

        /// <summary>
        /// 从十进制值创建格雷码
        /// </summary>
        /// <param name="decimal">十进制值</param>
        /// <returns>格雷码</returns>
        public static uint FromDecimal(uint @decimal)
        {
            return BinaryToGray(@decimal);
        }
    }
}
