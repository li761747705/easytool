using System;
using System.IO;
using System.Text;
using System.Linq;

namespace EasyTool.ConvertCategory
{
    /// <summary>
    /// Byte 字节扩展方法
    /// </summary>
    public static class ByteExtension
    {
        #region 单字节转换

        /// <summary>
        /// 将字节转换为16进制字符串（已移除，使用 CodeCategory/HexUtil.ToHex）
        /// </summary>
        [Obsolete("请使用 CodeCategory.HexUtil.ToHex 方法", true)]
        public static string ToHex(this byte value)
        {
            return value.ToString("X2");
        }

        /// <summary>
        /// 将字节转换为16进制字符串（小写）
        /// </summary>
        [Obsolete("请使用 CodeCategory.HexUtil.ToHexLower 方法", true)]
        public static string ToHexLower(this byte value)
        {
            return value.ToString("x2");
        }

        /// <summary>
        /// 将字节转换为二进制字符串
        /// </summary>
        public static string ToBinaryString(this byte value)
        {
            return Convert.ToString(value, 2).PadLeft(8, '0');
        }

        /// <summary>
        /// 获取字节的指定位
        /// </summary>
        public static bool GetBit(this byte value, int index)
        {
            if (index < 0 || index > 7)
                throw new ArgumentOutOfRangeException(nameof(index), "Index must be between 0 and 7");

            return (value & (1 << index)) != 0;
        }

        /// <summary>
        /// 设置字节的指定位
        /// </summary>
        public static byte SetBit(this byte value, int index, bool bitValue)
        {
            if (index < 0 || index > 7)
                throw new ArgumentOutOfRangeException(nameof(index), "Index must be between 0 and 7");

            if (bitValue)
                return (byte)(value | (1 << index));
            else
                return (byte)(value & ~(1 << index));
        }

        #endregion

        #region 字节数组转换

        /// <summary>
        /// 将字节数组转换为16进制字符串
        /// </summary>
        public static string ToHex(this byte[] bytes)
        {
            return bytes.ToHex(true);
        }

        /// <summary>
        /// 将字节数组转换为16进制字符串
        /// </summary>
        public static string ToHex(this byte[] bytes, bool uppercase)
        {
            if (bytes == null || bytes.Length == 0)
                return string.Empty;

            var format = uppercase ? "X2" : "x2";
            var sb = new StringBuilder(bytes.Length * 2);

            foreach (var b in bytes)
            {
                sb.Append(b.ToString(format));
            }

            return sb.ToString();
        }

        /// <summary>
        /// 将字节数组转换为16进制字符串（带分隔符）
        /// </summary>
        public static string ToHex(this byte[] bytes, string separator, bool uppercase = true)
        {
            if (bytes == null || bytes.Length == 0)
                return string.Empty;

            var format = uppercase ? "X2" : "x2";
            return string.Join(separator, bytes.Select(b => b.ToString(format)));
        }

        /// <summary>
        /// 从16进制字符串转换为字节数组
        /// </summary>
        public static byte[] FromHexToBytes(this string hex)
        {
            if (string.IsNullOrWhiteSpace(hex))
                return Array.Empty<byte>();

            hex = hex.Replace("-", "").Replace(" ", "");

            if (hex.Length % 2 != 0)
                throw new ArgumentException("Hex string must have an even length", nameof(hex));

            var bytes = new byte[hex.Length / 2];

            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }

            return bytes;
        }


        /// <summary>
        /// 将字节数组转换为二进制字符串
        /// </summary>
        public static string ToBinaryString(this byte[] bytes, string separator = " ")
        {
            if (bytes == null || bytes.Length == 0)
                return string.Empty;

            return string.Join(separator, bytes.Select(b => Convert.ToString(b, 2).PadLeft(8, '0')));
        }

        /// <summary>
        /// 从二进制字符串转换为字节数组
        /// </summary>
        public static byte[] FromBinaryStringToBytes(this string binary)
        {
            if (string.IsNullOrWhiteSpace(binary))
                return Array.Empty<byte>();

            binary = binary.Replace(" ", "");

            if (binary.Length % 8 != 0)
                throw new ArgumentException("Binary string length must be a multiple of 8", nameof(binary));

            var bytes = new byte[binary.Length / 8];

            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(binary.Substring(i * 8, 8), 2);
            }

            return bytes;
        }

        #endregion

        #region 字节数组操作

        /// <summary>
        /// 反转字节数组
        /// </summary>
        public static byte[]? Reverse(this byte[]? bytes)
        {
            if (bytes == null)
                return null;

            var result = new byte[bytes.Length];
            Array.Copy(bytes, result, bytes.Length);
            Array.Reverse(result);
            return result;
        }

        /// <summary>
        /// 字节数组异或运算
        /// </summary>
        public static byte[] Xor(this byte[] bytes, byte[] key)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            var result = new byte[bytes.Length];

            for (int i = 0; i < bytes.Length; i++)
            {
                result[i] = (byte)(bytes[i] ^ key[i % key.Length]);
            }

            return result;
        }

        /// <summary>
        /// 字节数组异或运算（单字节密钥）
        /// </summary>
        public static byte[] Xor(this byte[] bytes, byte key)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            var result = new byte[bytes.Length];

            for (int i = 0; i < bytes.Length; i++)
            {
                result[i] = (byte)(bytes[i] ^ key);
            }

            return result;
        }

        /// <summary>
        /// 合并多个字节数组
        /// </summary>
        public static byte[] Combine(params byte[][]? arrays)
        {
            if (arrays == null || arrays.Length == 0)
                return Array.Empty<byte>();

            int totalLength = 0;
            foreach (var arr in arrays)
            {
                if (arr != null)
                    totalLength += arr.Length;
            }

            var result = new byte[totalLength];
            int offset = 0;

            foreach (var arr in arrays)
            {
                if (arr != null && arr.Length > 0)
                {
                    Array.Copy(arr, 0, result, offset, arr.Length);
                    offset += arr.Length;
                }
            }

            return result;
        }

        /// <summary>
        /// 截取字节数组
        /// </summary>
        public static byte[] SubArray(this byte[] bytes, int startIndex, int length)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            if (startIndex < 0 || startIndex >= bytes.Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            if (length < 0 || startIndex + length > bytes.Length)
                throw new ArgumentOutOfRangeException(nameof(length));

            var result = new byte[length];
            Array.Copy(bytes, startIndex, result, 0, length);
            return result;
        }

        /// <summary>
        /// 截取字节数组（从指定位置到末尾）
        /// </summary>
        public static byte[] SubArray(this byte[] bytes, int startIndex)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            if (startIndex < 0)
                startIndex = 0;

            if (startIndex >= bytes.Length)
                return Array.Empty<byte>();

            return SubArray(bytes, startIndex, bytes.Length - startIndex);
        }

        #endregion

        #region 字节数组比较

        /// <summary>
        /// 比较两个字节数组是否相等
        /// </summary>
        public static bool EqualsTo(this byte[]? bytes, byte[]? other)
        {
            if (ReferenceEquals(bytes, other))
                return true;

            if (bytes == null || other == null)
                return false;

            if (bytes.Length != other.Length)
                return false;

            for (int i = 0; i < bytes.Length; i++)
            {
                if (bytes[i] != other[i])
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 字节数组比较（返回差异索引）
        /// </summary>
        public static int[] Diff(this byte[]? bytes, byte[]? other)
        {
            if (bytes == null || other == null)
                return Array.Empty<int>();

            var minLength = Math.Min(bytes.Length, other.Length);
            var diffs = new System.Collections.Generic.List<int>();

            for (int i = 0; i < minLength; i++)
            {
                if (bytes[i] != other[i])
                    diffs.Add(i);
            }

            return diffs.ToArray();
        }

        #endregion

        #region 字节数组与基本类型转换

        /// <summary>
        /// 将字节数组转换为整数（小端序）
        /// </summary>
        public static int ToInt32(this byte[] bytes, int startIndex = 0)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            if (startIndex < 0 || startIndex + 4 > bytes.Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            return bytes[startIndex] | (bytes[startIndex + 1] << 8) | (bytes[startIndex + 2] << 16) | (bytes[startIndex + 3] << 24);
        }

        /// <summary>
        /// 将整数转换为字节数组（小端序）
        /// </summary>
        public static byte[] ToBytes(this int value)
        {
            return new[] { (byte)(value & 0xFF), (byte)((value >> 8) & 0xFF), (byte)((value >> 16) & 0xFF), (byte)((value >> 24) & 0xFF) };
        }

        /// <summary>
        /// 将字节数组转换为长整数（小端序）
        /// </summary>
        public static long ToInt64(this byte[] bytes, int startIndex = 0)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            if (startIndex < 0 || startIndex + 8 > bytes.Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            return BitConverter.ToInt64(bytes, startIndex);
        }

        /// <summary>
        /// 将长整数转换为字节数组（小端序）
        /// </summary>
        public static byte[] ToBytes(this long value)
        {
            return BitConverter.GetBytes(value);
        }

        /// <summary>
        /// 将字节数组转换为短整数（小端序）
        /// </summary>
        public static short ToInt16(this byte[] bytes, int startIndex = 0)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            if (startIndex < 0 || startIndex + 2 > bytes.Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            return (short)(bytes[startIndex] | (bytes[startIndex + 1] << 8));
        }

        /// <summary>
        /// 将短整数转换为字节数组（小端序）
        /// </summary>
        public static byte[] ToBytes(this short value)
        {
            return new[] { (byte)(value & 0xFF), (byte)((value >> 8) & 0xFF) };
        }

        #endregion

        #region 字节数组编码解码


        #endregion

        #region 字节数组压缩解压

        /// <summary>
        /// 压缩字节数组（使用 GZip）
        /// </summary>
        public static byte[]? Compress(this byte[]? bytes)
        {
            if (bytes == null || bytes.Length == 0)
                return bytes;

            using var output = new MemoryStream();
            using (var gzip = new System.IO.Compression.GZipStream(output, System.IO.Compression.CompressionMode.Compress))
            {
                gzip.Write(bytes, 0, bytes.Length);
            }
            return output.ToArray();
        }

        /// <summary>
        /// 解压字节数组（使用 GZip）
        /// </summary>
        public static byte[]? Decompress(this byte[]? bytes)
        {
            if (bytes == null || bytes.Length == 0)
                return bytes;

            using var input = new MemoryStream(bytes);
            using var gzip = new System.IO.Compression.GZipStream(input, System.IO.Compression.CompressionMode.Decompress);
            using var output = new MemoryStream();
            gzip.CopyTo(output);
            return output.ToArray();
        }

        #endregion

        #region 字节数组哈希

        /// <summary>
        /// 计算字节数组的 MD5 哈希值
        /// </summary>
        public static byte[] ToMd5(this byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
                return Array.Empty<byte>();

            using var md5 = System.Security.Cryptography.MD5.Create();
            return md5.ComputeHash(bytes);
        }

        /// <summary>
        /// 计算字节数组的 SHA1 哈希值
        /// </summary>
        public static byte[] ToSha1(this byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
                return Array.Empty<byte>();

            using var sha1 = System.Security.Cryptography.SHA1.Create();
            return sha1.ComputeHash(bytes);
        }

        /// <summary>
        /// 计算字节数组的 SHA256 哈希值
        /// </summary>
        public static byte[] ToSha256(this byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
                return Array.Empty<byte>();

            using var sha256 = System.Security.Cryptography.SHA256.Create();
            return sha256.ComputeHash(bytes);
        }

        /// <summary>
        /// 计算字节数组的 CRC32 校验值
        /// </summary>
        public static uint ToCrc32(this byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
                return 0;

            uint crc = 0xFFFFFFFF;
            foreach (var b in bytes)
            {
                crc ^= b;
                for (int i = 0; i < 8; i++)
                {
                    crc = (crc >> 1) ^ ((crc & 1) == 1 ? 0xEDB88320 : 0);
                }
            }
            return ~crc;
        }

        #endregion
    }
}
