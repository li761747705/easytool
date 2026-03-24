using System;
using System.Security.Cryptography;
using System.Text;

namespace EasyTool.CodeCategory
{
    /// <summary>
    /// Tiger 哈希工具类
    /// Tiger 是由 Ross Anderson 和 Eli Biham 设计的快速哈希算法
    /// 专为 64 位处理器优化，输出 192 位（24 字节）
    /// </summary>
    public static class TigerUtil
    {
        private const int HashSize = 24; // 192位
        private const int BlockSize = 64; // 512位

        // S-boxes
        private static readonly ulong[] S = new ulong[]
        {
            0x02AAB17CF7E90C5E, 0xAC424B03E243A8EC, 0x72CD5BE30DD5FCD3, 0x6D019B93F6F97F3A,
            0xCD9978FFD21F9193, 0x7573A1C970802FAE, 0xB164326B922A83BB, 0x46883EEE04915870,
            0xEAACE3057103ECE6, 0xC54169B808A3535C, 0x4CE754918DDEC47C, 0x0AA2F4DFDC0DF40C,
            0x10B76F18A74DBEFA, 0xC6CCB6235AD1AB6A, 0x13726121572FE2FF, 0x1A488C6F199D921E,
            0x4BC9F9F4DA0007CA, 0x26F5E6F6E85241C7, 0x859079DBEA5947B6, 0x4F1885B5EB4F880C,
            0xD78E761EA6F7CBA0, 0x8E36428C52B5C17D, 0x69CF6827373063C1, 0xB607C93D9BB4C56E,
            0x7D820E760E76B5EA, 0x645C9CC6F07FDC42, 0xBF38A078243342E0, 0x5F6B343C9D2E7D04,
            0xF2C28AEB600B0EC6, 0x6C0ED85F7254BCAC, 0x71592281A4DB4FE5, 0x1967FA69CE0FED9F,
            0xFD5293F8B96545DB, 0xC879E84D5BB62F8F, 0x860248920193194E, 0xA4F953AA47EE7048,
            0xD957E363A198BF6B, 0x327894F2FDDC3BBA, 0x9F7F973ED03B1AE9, 0x1B505014AE5AC36B,
            0xE7CC8C8EFB4C41F7, 0x7D4DA8DE2296204E, 0x7E9791D04B8C6B88, 0x39A8B0D45C357F47,
            0x723F453E1A6ED868, 0x59E59E13C6A5C3BF, 0xB6F3169AB9916821, 0x9E6B0E7A3A2888F7
        };

        /// <summary>
        /// 计算 Tiger 哈希值
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <returns>24字节哈希值</returns>
        public static byte[] Hash(byte[] data)
        {
            if (data == null || data.Length == 0)
                return new byte[HashSize];

            // 初始值
            ulong a = 0x0123456789ABCDEF;
            ulong b = 0xFEDCBA9876543210;
            ulong c = 0xF096A5B4C3B2E187;

            // 填充
            byte[] padded = PadMessage(data);
            int blocks = padded.Length / BlockSize;

            for (int i = 0; i < blocks; i++)
            {
                ulong[] x = new ulong[8];
                for (int j = 0; j < 8; j++)
                {
                    x[j] = BitConverter.ToUInt64(padded, i * BlockSize + j * 8);
                }

                TigerRound(ref a, ref b, ref c, x);
            }

            return Combine(a, b, c);
        }

        /// <summary>
        /// 计算字符串的 Tiger 哈希值
        /// </summary>
        /// <param name="text">输入文本</param>
        /// <returns>十六进制哈希字符串</returns>
        public static string HashString(string text)
        {
            if (string.IsNullOrEmpty(text))
                return new string('0', HashSize * 2);

            byte[] data = Encoding.UTF8.GetBytes(text);
            byte[] hash = Hash(data);
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

        /// <summary>
        /// 验证哈希值
        /// </summary>
        /// <param name="data">原始数据</param>
        /// <param name="hash">预期哈希值</param>
        /// <returns>是否匹配</returns>
        public static bool Verify(byte[] data, byte[] hash)
        {
            if (hash == null || hash.Length != HashSize)
                return false;

            byte[] computed = Hash(data);
            return SlowEquals(computed, hash);
        }

        /// <summary>
        /// 验证字符串哈希
        /// </summary>
        /// <param name="text">原始文本</param>
        /// <param name="hashHex">预期哈希值（十六进制）</param>
        /// <returns>是否匹配</returns>
        public static bool VerifyString(string text, string hashHex)
        {
            if (string.IsNullOrEmpty(hashHex) || hashHex.Length != HashSize * 2)
                return false;

            string computed = HashString(text);
            return string.Equals(computed, hashHex, StringComparison.OrdinalIgnoreCase);
        }

        private static byte[] PadMessage(byte[] data)
        {
            int length = data.Length;
            int paddingLength = BlockSize - ((length + 9) % BlockSize);
            if (paddingLength == BlockSize) paddingLength = 0;

            byte[] padded = new byte[length + 1 + paddingLength + 8];
            Array.Copy(data, padded, length);

            // 添加 0x01 填充
            padded[length] = 0x01;

            // 添加长度（位数）
            ulong bitLength = (ulong)length * 8;
            for (int i = 0; i < 8; i++)
            {
                padded[padded.Length - 1 - i] = (byte)(bitLength >> (i * 8));
            }

            return padded;
        }

        private static void TigerRound(ref ulong a, ref ulong b, ref ulong c, ulong[] x)
        {
            // 保存原始值
            ulong aa = a, bb = b, cc = c;

            // Pass 1
            c ^= x[0];
            a -= Table(c, 0);
            b += Table(c, 2);
            b *= 5;

            a ^= x[1];
            b -= Table(a, 0);
            c += Table(a, 2);
            c *= 5;

            b ^= x[2];
            c -= Table(b, 0);
            a += Table(b, 2);
            a *= 5;

            c ^= x[3];
            a -= Table(c, 0);
            b += Table(c, 2);
            b *= 5;

            a ^= x[4];
            b -= Table(a, 0);
            c += Table(a, 2);
            c *= 5;

            b ^= x[5];
            c -= Table(b, 0);
            a += Table(b, 2);
            a *= 5;

            c ^= x[6];
            a -= Table(c, 0);
            b += Table(c, 2);
            b *= 5;

            a ^= x[7];
            b -= Table(a, 0);
            c += Table(a, 2);
            c *= 5;

            // Pass 2
            a ^= x[7];
            b -= Table(a, 0);
            c += Table(a, 2);
            c *= 5;

            b ^= x[6];
            c -= Table(b, 0);
            a += Table(b, 2);
            a *= 5;

            c ^= x[5];
            a -= Table(c, 0);
            b += Table(c, 2);
            b *= 5;

            a ^= x[4];
            b -= Table(a, 0);
            c += Table(a, 2);
            c *= 5;

            b ^= x[3];
            c -= Table(b, 0);
            a += Table(b, 2);
            a *= 5;

            c ^= x[2];
            a -= Table(c, 0);
            b += Table(c, 2);
            b *= 5;

            a ^= x[1];
            b -= Table(a, 0);
            c += Table(a, 2);
            c *= 5;

            b ^= x[0];
            c -= Table(b, 0);
            a += Table(b, 2);
            a *= 5;

            // Pass 3
            c ^= x[0];
            a -= Table(c, 0);
            b += Table(c, 2);
            b *= 5;

            a ^= x[1];
            b -= Table(a, 0);
            c += Table(a, 2);
            c *= 5;

            b ^= x[2];
            c -= Table(b, 0);
            a += Table(b, 2);
            a *= 5;

            c ^= x[3];
            a -= Table(c, 0);
            b += Table(c, 2);
            b *= 5;

            a ^= x[4];
            b -= Table(a, 0);
            c += Table(a, 2);
            c *= 5;

            b ^= x[5];
            c -= Table(b, 0);
            a += Table(b, 2);
            a *= 5;

            c ^= x[6];
            a -= Table(c, 0);
            b += Table(c, 2);
            b *= 5;

            a ^= x[7];
            b -= Table(a, 0);
            c += Table(a, 2);
            c *= 5;

            // 反馈
            a ^= aa;
            b -= bb;
            c += cc;
        }

        private static ulong Table(ulong x, int i)
        {
            byte b = (byte)(x >> (i * 8));
            return S[b % S.Length];
        }

        private static byte[] Combine(ulong a, ulong b, ulong c)
        {
            byte[] result = new byte[24];
            BitConverter.GetBytes(a).CopyTo(result, 0);
            BitConverter.GetBytes(b).CopyTo(result, 8);
            BitConverter.GetBytes(c).CopyTo(result, 16);
            return result;
        }

        private static bool SlowEquals(byte[] a, byte[] b)
        {
            uint diff = (uint)a.Length ^ (uint)b.Length;
            for (int i = 0; i < a.Length && i < b.Length; i++)
                diff |= (uint)(a[i] ^ b[i]);
            return diff == 0;
        }
    }
}
