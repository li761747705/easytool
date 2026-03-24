using System;
using System.Text;

namespace EasyTool.CodeCategory
{
    /// <summary>
    /// RIPEMD-160 哈希工具类
    /// RIPEMD-160 是一种 160 位加密哈希函数
    /// 由欧洲 RIPE 项目开发，比特币地址使用此算法
    /// 比 SHA-1 更安全
    /// </summary>
    public static class RIPEMD160Util
    {
        private const int DigestSize = 20;
        private const int BlockSize = 64;

        // 初始值
        private static readonly uint[] IV = new uint[]
        {
            0x67452301, 0xefcdab89, 0x98badcfe, 0x10325476, 0xc3d2e1f0,
            0x7658def0, 0x890abc12, 0xfedcba34, 0x01234567, 0x32107654
        };

        /// <summary>
        /// 计算 RIPEMD-160 哈希值
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <returns>20字节哈希值</returns>
        public static byte[] ComputeHash(byte[] data)
        {
            if (data == null)
                data = Array.Empty<byte>();

            return ComputeHash(data, 0, data.Length);
        }

        /// <summary>
        /// 计算 RIPEMD-160 哈希值
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <param name="offset">起始偏移</param>
        /// <param name="length">数据长度</param>
        /// <returns>20字节哈希值</returns>
        public static byte[] ComputeHash(byte[] data, int offset, int length)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (offset < 0 || offset > data.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (length < 0 || offset + length > data.Length)
                throw new ArgumentOutOfRangeException(nameof(length));

            uint[] h = new uint[10];
            Array.Copy(IV, h, 10);

            byte[] padded = PadMessage(data, offset, length);
            int blocks = padded.Length / BlockSize;

            for (int i = 0; i < blocks; i++)
            {
                ProcessBlock(padded, i * BlockSize, h);
            }

            byte[] result = new byte[DigestSize];
            for (int i = 0; i < 5; i++)
            {
                result[i * 4] = (byte)h[i];
                result[i * 4 + 1] = (byte)(h[i] >> 8);
                result[i * 4 + 2] = (byte)(h[i] >> 16);
                result[i * 4 + 3] = (byte)(h[i] >> 24);
            }

            return result;
        }

        /// <summary>
        /// 计算字符串的 RIPEMD-160 哈希值
        /// </summary>
        /// <param name="text">文本</param>
        /// <returns>20字节哈希值</returns>
        public static byte[] ComputeString(string text)
        {
            if (string.IsNullOrEmpty(text))
                return ComputeHash(Array.Empty<byte>());

            byte[] data = Encoding.UTF8.GetBytes(text);
            return ComputeHash(data);
        }

        /// <summary>
        /// 获取 RIPEMD-160 哈希的十六进制表示
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <returns>40字符的十六进制字符串</returns>
        public static string ComputeHex(byte[] data)
        {
            byte[] hash = ComputeHash(data);
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

        /// <summary>
        /// 计算字符串的 RIPEMD-160 哈希十六进制表示
        /// </summary>
        /// <param name="text">文本</param>
        /// <returns>40字符的十六进制字符串</returns>
        public static string ComputeStringHex(string text)
        {
            byte[] hash = ComputeString(text);
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

        private static byte[] PadMessage(byte[] data, int offset, int length)
        {
            long bitLength = (long)length * 8;
            int padding = 64 - ((length + 9) % 64);
            if (padding == 64) padding = 0;

            byte[] result = new byte[length + 1 + padding + 8];
            Array.Copy(data, offset, result, 0, length);

            result[length] = 0x80;

            // 添加长度（小端序）
            for (int i = 0; i < 8; i++)
            {
                result[result.Length - 8 + i] = (byte)(bitLength >> (i * 8));
            }

            return result;
        }

        private static void ProcessBlock(byte[] block, int offset, uint[] h)
        {
            uint[] x = new uint[16];
            for (int i = 0; i < 16; i++)
            {
                x[i] = BitConverter.ToUInt32(block, offset + i * 4);
            }

            uint al = h[0], bl = h[1], cl = h[2], dl = h[3], el = h[4];
            uint ar = h[5], br = h[6], cr = h[7], dr = h[8], er = h[9];

            // 左侧
            al = F1(al, bl, cl, dl, el, x[0], 11);
            el = F1(el, al, bl, cl, dl, x[1], 14);
            dl = F1(dl, el, al, bl, cl, x[2], 15);
            cl = F1(cl, dl, el, al, bl, x[3], 12);
            bl = F1(bl, cl, dl, el, al, x[4], 5);
            al = F1(al, bl, cl, dl, el, x[5], 8);
            el = F1(el, al, bl, cl, dl, x[6], 7);
            dl = F1(dl, el, al, bl, cl, x[7], 9);
            cl = F1(cl, dl, el, al, bl, x[8], 11);
            bl = F1(bl, cl, dl, el, al, x[9], 13);
            al = F1(al, bl, cl, dl, el, x[10], 14);
            el = F1(el, al, bl, cl, dl, x[11], 15);
            dl = F1(dl, el, al, bl, cl, x[12], 6);
            cl = F1(cl, dl, el, al, bl, x[13], 7);
            bl = F1(bl, cl, dl, el, al, x[14], 9);
            al = F1(al, bl, cl, dl, el, x[15], 8);

            // 右侧
            ar = F5(ar, br, cr, dr, er, x[5], 8);
            er = F5(er, ar, br, cr, dr, x[14], 9);
            dr = F5(dr, er, ar, br, cr, x[7], 9);
            cr = F5(cr, dr, er, ar, br, x[0], 11);
            br = F5(br, cr, dr, er, ar, x[9], 13);
            ar = F5(ar, br, cr, dr, er, x[2], 15);
            er = F5(er, ar, br, cr, dr, x[11], 15);
            dr = F5(dr, er, ar, br, cr, x[4], 5);
            cr = F5(cr, dr, er, ar, br, x[13], 7);
            br = F5(br, cr, dr, er, ar, x[6], 7);
            ar = F5(ar, br, cr, dr, er, x[15], 8);
            er = F5(er, ar, br, cr, dr, x[8], 11);
            dr = F5(dr, er, ar, br, cr, x[1], 14);
            cr = F5(cr, dr, er, ar, br, x[10], 14);
            br = F5(br, cr, dr, er, ar, x[3], 12);
            ar = F5(ar, br, cr, dr, er, x[12], 6);

            // 第二轮左侧
            bl = F2(bl, cl, dl, el, al, x[7], 7);
            al = F2(al, bl, cl, dl, el, x[4], 6);
            el = F2(el, al, bl, cl, dl, x[13], 8);
            dl = F2(dl, el, al, bl, cl, x[1], 13);
            cl = F2(cl, dl, el, al, bl, x[10], 11);
            bl = F2(bl, cl, dl, el, al, x[6], 9);
            al = F2(al, bl, cl, dl, el, x[15], 7);
            el = F2(el, al, bl, cl, dl, x[3], 15);
            dl = F2(dl, el, al, bl, cl, x[12], 7);
            cl = F2(cl, dl, el, al, bl, x[0], 12);
            bl = F2(bl, cl, dl, el, al, x[9], 15);
            al = F2(al, bl, cl, dl, el, x[5], 9);
            el = F2(el, al, bl, cl, dl, x[2], 11);
            dl = F2(dl, el, al, bl, cl, x[14], 7);
            cl = F2(cl, dl, el, al, bl, x[11], 13);
            bl = F2(bl, cl, dl, el, al, x[8], 12);

            // 第二轮右侧
            br = F4(br, cr, dr, er, ar, x[6], 9);
            ar = F4(ar, br, cr, dr, er, x[11], 13);
            er = F4(er, ar, br, cr, dr, x[3], 15);
            dr = F4(dr, er, ar, br, cr, x[7], 7);
            cr = F4(cr, dr, er, ar, br, x[0], 12);
            br = F4(br, cr, dr, er, ar, x[13], 8);
            ar = F4(ar, br, cr, dr, er, x[5], 9);
            er = F4(er, ar, br, cr, dr, x[10], 11);
            dr = F4(dr, er, ar, br, cr, x[14], 7);
            cr = F4(cr, dr, er, ar, br, x[15], 7);
            br = F4(br, cr, dr, er, ar, x[8], 12);
            ar = F4(ar, br, cr, dr, er, x[12], 7);
            er = F4(er, ar, br, cr, dr, x[4], 6);
            dr = F4(dr, er, ar, br, cr, x[9], 15);
            cr = F4(cr, dr, er, ar, br, x[1], 13);
            br = F4(br, cr, dr, er, ar, x[2], 11);

            // 第三轮左侧
            cl = F3(cl, dl, el, al, bl, x[3], 11);
            bl = F3(bl, cl, dl, el, al, x[10], 13);
            al = F3(al, bl, cl, dl, el, x[14], 6);
            el = F3(el, al, bl, cl, dl, x[4], 7);
            dl = F3(dl, el, al, bl, cl, x[9], 14);
            cl = F3(cl, dl, el, al, bl, x[15], 9);
            bl = F3(bl, cl, dl, el, al, x[8], 13);
            al = F3(al, bl, cl, dl, el, x[1], 15);
            el = F3(el, al, bl, cl, dl, x[2], 14);
            dl = F3(dl, el, al, bl, cl, x[7], 8);
            cl = F3(cl, dl, el, al, bl, x[0], 13);
            bl = F3(bl, cl, dl, el, al, x[6], 6);
            al = F3(al, bl, cl, dl, el, x[13], 5);
            el = F3(el, al, bl, cl, dl, x[11], 12);
            dl = F3(dl, el, al, bl, cl, x[5], 7);
            cl = F3(cl, dl, el, al, bl, x[12], 5);

            // 第三轮右侧
            cr = F3(cr, dr, er, ar, br, x[15], 8);
            br = F3(br, cr, dr, er, ar, x[5], 9);
            ar = F3(ar, br, cr, dr, er, x[1], 14);
            er = F3(er, ar, br, cr, dr, x[3], 9);
            dr = F3(dr, er, ar, br, cr, x[7], 13);
            cr = F3(cr, dr, er, ar, br, x[14], 15);
            br = F3(br, cr, dr, er, ar, x[6], 7);
            ar = F3(ar, br, cr, dr, er, x[9], 12);
            er = F3(er, ar, br, cr, dr, x[11], 8);
            dr = F3(dr, er, ar, br, cr, x[8], 9);
            cr = F3(cr, dr, er, ar, br, x[12], 11);
            br = F3(br, cr, dr, er, ar, x[2], 7);
            ar = F3(ar, br, cr, dr, er, x[10], 7);
            er = F3(er, ar, br, cr, dr, x[0], 12);
            dr = F3(dr, er, ar, br, cr, x[4], 7);
            cr = F3(cr, dr, er, ar, br, x[13], 7);

            // 第四轮左侧
            dl = F4(dl, el, al, bl, cl, x[1], 11);
            cl = F4(cl, dl, el, al, bl, x[9], 12);
            bl = F4(bl, cl, dl, el, al, x[11], 14);
            al = F4(al, bl, cl, dl, el, x[10], 15);
            el = F4(el, al, bl, cl, dl, x[0], 14);
            dl = F4(dl, el, al, bl, cl, x[8], 15);
            cl = F4(cl, dl, el, al, bl, x[12], 9);
            bl = F4(bl, cl, dl, el, al, x[4], 8);
            al = F4(al, bl, cl, dl, el, x[13], 9);
            el = F4(el, al, bl, cl, dl, x[3], 14);
            dl = F4(dl, el, al, bl, cl, x[7], 5);
            cl = F4(cl, dl, el, al, bl, x[15], 6);
            bl = F4(bl, cl, dl, el, al, x[14], 8);
            al = F4(al, bl, cl, dl, el, x[5], 6);
            el = F4(el, al, bl, cl, dl, x[6], 5);
            dl = F4(dl, el, al, bl, cl, x[2], 12);

            // 第四轮右侧
            dr = F2(dr, er, ar, br, cr, x[8], 15);
            cr = F2(cr, dr, er, ar, br, x[6], 5);
            br = F2(br, cr, dr, er, ar, x[4], 8);
            ar = F2(ar, br, cr, dr, er, x[1], 11);
            er = F2(er, ar, br, cr, dr, x[3], 14);
            dr = F2(dr, er, ar, br, cr, x[11], 14);
            cr = F2(cr, dr, er, ar, br, x[15], 6);
            br = F2(br, cr, dr, er, ar, x[0], 14);
            ar = F2(ar, br, cr, dr, er, x[5], 6);
            er = F2(er, ar, br, cr, dr, x[12], 9);
            dr = F2(dr, er, ar, br, cr, x[2], 12);
            cr = F2(cr, dr, er, ar, br, x[13], 9);
            br = F2(br, cr, dr, er, ar, x[9], 12);
            ar = F2(ar, br, cr, dr, er, x[7], 5);
            er = F2(er, ar, br, cr, dr, x[10], 15);
            dr = F2(dr, er, ar, br, cr, x[14], 8);

            // 第五轮左侧
            el = F5(el, al, bl, cl, dl, x[4], 9);
            dl = F5(dl, el, al, bl, cl, x[0], 15);
            cl = F5(cl, dl, el, al, bl, x[5], 5);
            bl = F5(bl, cl, dl, el, al, x[9], 11);
            al = F5(al, bl, cl, dl, el, x[7], 6);
            el = F5(el, al, bl, cl, dl, x[12], 8);
            dl = F5(dl, el, al, bl, cl, x[2], 13);
            cl = F5(cl, dl, el, al, bl, x[10], 12);
            bl = F5(bl, cl, dl, el, al, x[14], 5);
            al = F5(al, bl, cl, dl, el, x[1], 12);
            el = F5(el, al, bl, cl, dl, x[3], 13);
            dl = F5(dl, el, al, bl, cl, x[8], 14);
            cl = F5(cl, dl, el, al, bl, x[11], 11);
            bl = F5(bl, cl, dl, el, al, x[6], 8);
            al = F5(al, bl, cl, dl, el, x[15], 5);
            el = F5(el, al, bl, cl, dl, x[13], 6);

            // 第五轮右侧
            er = F1(er, ar, br, cr, dr, x[12], 8);
            dr = F1(dr, er, ar, br, cr, x[15], 5);
            cr = F1(cr, dr, er, ar, br, x[10], 12);
            br = F1(br, cr, dr, er, ar, x[4], 9);
            ar = F1(ar, br, cr, dr, er, x[1], 12);
            er = F1(er, ar, br, cr, dr, x[5], 5);
            dr = F1(dr, er, ar, br, cr, x[8], 14);
            cr = F1(cr, dr, er, ar, br, x[7], 6);
            br = F1(br, cr, dr, er, ar, x[6], 8);
            ar = F1(ar, br, cr, dr, er, x[2], 13);
            er = F1(er, ar, br, cr, dr, x[13], 6);
            dr = F1(dr, er, ar, br, cr, x[14], 5);
            cr = F1(cr, dr, er, ar, br, x[0], 15);
            br = F1(br, cr, dr, er, ar, x[3], 13);
            ar = F1(ar, br, cr, dr, er, x[9], 11);
            er = F1(er, ar, br, cr, dr, x[11], 11);

            // 最终更新
            uint t = h[1] + cl + dr;
            h[1] = h[2] + dl + er;
            h[2] = h[3] + el + ar;
            h[3] = h[4] + al + br;
            h[4] = h[0] + bl + cr;
            h[0] = t;
        }

        private static uint F1(uint a, uint b, uint c, uint d, uint e, uint x, int s)
        {
            return RotateLeft(a + (b ^ c ^ d) + x, s) + e;
        }

        private static uint F2(uint a, uint b, uint c, uint d, uint e, uint x, int s)
        {
            return RotateLeft(a + ((b & c) | (~b & d)) + x + 0x5a827999, s) + e;
        }

        private static uint F3(uint a, uint b, uint c, uint d, uint e, uint x, int s)
        {
            return RotateLeft(a + ((b | ~c) ^ d) + x + 0x6ed9eba1, s) + e;
        }

        private static uint F4(uint a, uint b, uint c, uint d, uint e, uint x, int s)
        {
            return RotateLeft(a + ((b & d) | (c & ~d)) + x + 0x8f1bbcdc, s) + e;
        }

        private static uint F5(uint a, uint b, uint c, uint d, uint e, uint x, int s)
        {
            return RotateLeft(a + (b ^ (c | ~d)) + x + 0xa953fd4e, s) + e;
        }

        private static uint RotateLeft(uint x, int n)
        {
            return (x << n) | (x >> (32 - n));
        }
    }
}
