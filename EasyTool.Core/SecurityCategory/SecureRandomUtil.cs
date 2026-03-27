using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace EasyTool.SecurityCategory
{
    /// <summary>
    /// 安全随机数生成器，使用加密安全的随机数生成器
    /// </summary>
    public static class SecureRandomUtil
    {
        private static readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();
        private static readonly char[] _alphanumericChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();
        private static readonly char[] _lowercaseChars = "abcdefghijklmnopqrstuvwxyz".ToCharArray();
        private static readonly char[] _uppercaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
        private static readonly char[] _digitChars = "0123456789".ToCharArray();
        private static readonly char[] _specialChars = "!@#$%^&*()_+-=[]{}|;:,.<>?".ToCharArray();
        private static readonly char[] _hexChars = "0123456789abcdef".ToCharArray();

        #region 基本随机数生成

        /// <summary>
        /// 生成指定长度的随机字节数组
        /// </summary>
        /// <param name="length">字节长度</param>
        /// <returns>随机字节数组</returns>
        public static byte[] GetBytes(int length)
        {
            if (length <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "长度必须大于0");
            }

            var bytes = new byte[length];
            _rng.GetBytes(bytes);
            return bytes;
        }

        /// <summary>
        /// 生成随机整数
        /// </summary>
        /// <returns>非负随机整数</returns>
        public static int GetInt()
        {
            return GetInt(0, int.MaxValue);
        }

        /// <summary>
        /// 生成指定范围内的随机整数
        /// </summary>
        /// <param name="min">最小值（包含）</param>
        /// <param name="max">最大值（不包含）</param>
        /// <returns>随机整数</returns>
        public static int GetInt(int min, int max)
        {
            if (min >= max)
            {
                throw new ArgumentOutOfRangeException(nameof(max), "最大值必须大于最小值");
            }

            var range = (long)max - min;
            var bytes = new byte[4];

            // 使用拒绝采样避免模偏差
            while (true)
            {
                _rng.GetBytes(bytes);
                var value = BitConverter.ToUInt32(bytes, 0);
                var remainder = range * (value / range);

                if (remainder <= uint.MaxValue - range)
                {
                    return (int)(min + (value - remainder));
                }
            }
        }

        /// <summary>
        /// 生成随机长整数
        /// </summary>
        /// <param name="min">最小值（包含）</param>
        /// <param name="max">最大值（不包含）</param>
        /// <returns>随机长整数</returns>
        public static long GetLong(long min, long max)
        {
            if (min >= max)
            {
                throw new ArgumentOutOfRangeException(nameof(max), "最大值必须大于最小值");
            }

            var range = (decimal)max - min;
            var bytes = new byte[8];

            while (true)
            {
                _rng.GetBytes(bytes);
                var value = (decimal)BitConverter.ToUInt64(bytes, 0);
                var remainder = range * (value / range);

                if (remainder <= ulong.MaxValue - range)
                {
                    return (long)(min + (value - remainder));
                }
            }
        }

        /// <summary>
        /// 生成随机双精度浮点数（0.0 到 1.0）
        /// </summary>
        /// <returns>随机双精度浮点数</returns>
        public static double GetDouble()
        {
            var bytes = new byte[8];
            _rng.GetBytes(bytes);
            var value = BitConverter.ToUInt64(bytes, 0);
            return value / (double)ulong.MaxValue;
        }

        /// <summary>
        /// 生成随机布尔值
        /// </summary>
        /// <returns>随机布尔值</returns>
        public static bool GetBool()
        {
            var bytes = new byte[1];
            _rng.GetBytes(bytes);
            return (bytes[0] & 1) == 1;
        }

        #endregion

        #region 字符串生成

        /// <summary>
        /// 生成随机字符串（字母数字）
        /// </summary>
        /// <param name="length">字符串长度</param>
        /// <returns>随机字符串</returns>
        public static string GetString(int length)
        {
            return GetString(length, _alphanumericChars);
        }

        /// <summary>
        /// 使用指定字符集生成随机字符串
        /// </summary>
        /// <param name="length">字符串长度</param>
        /// <param name="chars">字符集</param>
        /// <returns>随机字符串</returns>
        public static string GetString(int length, char[] chars)
        {
            if (length <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "长度必须大于0");
            }

            var result = new char[length];
            var bytes = new byte[length * 2];

            _rng.GetBytes(bytes);

            for (int i = 0; i < length; i++)
            {
                var value = BitConverter.ToUInt16(bytes, i * 2);
                result[i] = chars[value % chars.Length];
            }

            return new string(result);
        }

        /// <summary>
        /// 生成随机小写字符串
        /// </summary>
        /// <param name="length">字符串长度</param>
        /// <returns>随机小写字符串</returns>
        public static string GetLowercaseString(int length)
        {
            return GetString(length, _lowercaseChars);
        }

        /// <summary>
        /// 生成随机大写字符串
        /// </summary>
        /// <param name="length">字符串长度</param>
        /// <returns>随机大写字符串</returns>
        public static string GetUppercaseString(int length)
        {
            return GetString(length, _uppercaseChars);
        }

        /// <summary>
        /// 生成随机数字字符串
        /// </summary>
        /// <param name="length">字符串长度</param>
        /// <returns>随机数字字符串</returns>
        public static string GetNumericString(int length)
        {
            return GetString(length, _digitChars);
        }

        /// <summary>
        /// 生成随机十六进制字符串
        /// </summary>
        /// <param name="length">字符串长度</param>
        /// <param name="uppercase">是否使用大写字母</param>
        /// <returns>随机十六进制字符串</returns>
        public static string GetHexString(int length, bool uppercase = false)
        {
            var result = GetString(length, _hexChars);
            return uppercase ? result.ToUpperInvariant() : result;
        }

        /// <summary>
        /// 生成随机 Base64 字符串
        /// </summary>
        /// <param name="byteLength">原始字节长度</param>
        /// <returns>Base64 编码的随机字符串</returns>
        public static string GetBase64String(int byteLength)
        {
            var bytes = GetBytes(byteLength);
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// 生成 URL 安全的随机字符串
        /// </summary>
        /// <param name="byteLength">原始字节长度</param>
        /// <returns>URL 安全的随机字符串</returns>
        public static string GetUrlSafeString(int byteLength)
        {
            var bytes = GetBytes(byteLength);
            return Convert.ToBase64String(bytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .TrimEnd('=');
        }

        #endregion

        #region 密码生成

        /// <summary>
        /// 生成随机密码
        /// </summary>
        /// <param name="length">密码长度</param>
        /// <param name="includeLowercase">包含小写字母</param>
        /// <param name="includeUppercase">包含大写字母</param>
        /// <param name="includeDigits">包含数字</param>
        /// <param name="includeSpecial">包含特殊字符</param>
        /// <returns>随机密码</returns>
        public static string GeneratePassword(int length = 16,
            bool includeLowercase = true,
            bool includeUppercase = true,
            bool includeDigits = true,
            bool includeSpecial = true)
        {
            if (length <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "密码长度必须大于0");
            }

            var charSets = new List<char[]>();
            var allChars = new List<char>();

            if (includeLowercase)
            {
                charSets.Add(_lowercaseChars);
                allChars.AddRange(_lowercaseChars);
            }

            if (includeUppercase)
            {
                charSets.Add(_uppercaseChars);
                allChars.AddRange(_uppercaseChars);
            }

            if (includeDigits)
            {
                charSets.Add(_digitChars);
                allChars.AddRange(_digitChars);
            }

            if (includeSpecial)
            {
                charSets.Add(_specialChars);
                allChars.AddRange(_specialChars);
            }

            if (charSets.Count == 0)
            {
                throw new ArgumentException("至少需要选择一种字符类型");
            }

            var result = new char[length];
            var allCharsArray = allChars.ToArray();

            // 确保每种选中的字符类型至少有一个字符
            var usedCharsets = Math.Min(charSets.Count, length);
            for (int i = 0; i < usedCharsets; i++)
            {
                result[i] = GetString(1, charSets[i])[0];
            }

            // 填充剩余字符
            for (int i = usedCharsets; i < length; i++)
            {
                result[i] = GetString(1, allCharsArray)[0];
            }

            // 打乱顺序
            Shuffle(result);

            return new string(result);
        }

        /// <summary>
        /// 生成强密码（至少包含一个大写、小写、数字和特殊字符）
        /// </summary>
        /// <param name="length">密码长度（至少4）</param>
        /// <returns>强密码</returns>
        public static string GenerateStrongPassword(int length = 16)
        {
            if (length < 4)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "强密码长度至少为4");
            }

            return GeneratePassword(length, true, true, true, true);
        }

        /// <summary>
        /// 生成 PIN 码（纯数字）
        /// </summary>
        /// <param name="length">PIN 码长度</param>
        /// <returns>PIN 码</returns>
        public static string GeneratePin(int length = 6)
        {
            return GetNumericString(length);
        }

        #endregion

        #region 集合操作

        /// <summary>
        /// 从数组中随机选择一个元素
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="array">源数组</param>
        /// <returns>随机选择的元素</returns>
        public static T Choice<T>(T[] array)
        {
            if (array == null || array.Length == 0)
            {
                throw new ArgumentException("数组不能为空", nameof(array));
            }

            var index = GetInt(0, array.Length);
            return array[index];
        }

        /// <summary>
        /// 从列表中随机选择一个元素
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="list">源列表</param>
        /// <returns>随机选择的元素</returns>
        public static T Choice<T>(IList<T> list)
        {
            if (list == null || list.Count == 0)
            {
                throw new ArgumentException("列表不能为空", nameof(list));
            }

            var index = GetInt(0, list.Count);
            return list[index];
        }

        /// <summary>
        /// 从数组中随机选择多个元素（不重复）
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="array">源数组</param>
        /// <param name="count">选择数量</param>
        /// <returns>随机选择的元素数组</returns>
        public static T[] Sample<T>(T[] array, int count)
        {
            if (array == null || array.Length == 0)
            {
                throw new ArgumentException("数组不能为空", nameof(array));
            }

            if (count <= 0 || count > array.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(count), "选择数量必须在1到数组长度之间");
            }

            var indices = new int[array.Length];
            for (int i = 0; i < indices.Length; i++)
            {
                indices[i] = i;
            }

            Shuffle(indices);

            var result = new T[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = array[indices[i]];
            }

            return result;
        }

        /// <summary>
        /// 原地打乱数组顺序（Fisher-Yates 洗牌算法）
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="array">要打乱的数组</param>
        public static void Shuffle<T>(T[] array)
        {
            if (array == null || array.Length <= 1)
            {
                return;
            }

            for (int i = array.Length - 1; i > 0; i--)
            {
                var j = GetInt(0, i + 1);
                (array[i], array[j]) = (array[j], array[i]);
            }
        }

        /// <summary>
        /// 原地打乱列表顺序
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="list">要打乱的列表</param>
        public static void Shuffle<T>(IList<T> list)
        {
            if (list == null || list.Count <= 1)
            {
                return;
            }

            for (int i = list.Count - 1; i > 0; i--)
            {
                var j = GetInt(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        #endregion

        #region GUID 和 UUID

        /// <summary>
        /// 生成随机 GUID
        /// </summary>
        /// <returns>随机 GUID</returns>
        public static Guid GetGuid()
        {
            var bytes = GetBytes(16);
            return new Guid(bytes);
        }

        /// <summary>
        /// 生成 UUID v4（随机 UUID）
        /// </summary>
        /// <returns>UUID v4 字符串</returns>
        public static string GetUuidV4()
        {
            var bytes = GetBytes(16);

            // 设置版本位（版本4）
            bytes[6] = (byte)((bytes[6] & 0x0F) | 0x40);

            // 设置变体位
            bytes[8] = (byte)((bytes[8] & 0x3F) | 0x80);

            return new Guid(bytes).ToString();
        }

        #endregion

        #region 填充

        /// <summary>
        /// 用随机字节填充数组
        /// </summary>
        /// <param name="buffer">目标数组</param>
        public static void Fill(byte[] buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            _rng.GetBytes(buffer);
        }

        /// <summary>
        /// 用随机字节填充数组的一部分
        /// </summary>
        /// <param name="buffer">目标数组</param>
        /// <param name="offset">起始位置</param>
        /// <param name="count">填充数量</param>
        public static void Fill(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if (offset < 0 || offset >= buffer.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            if (count <= 0 || offset + count > buffer.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            var segment = new ArraySegment<byte>(buffer, offset, count).ToArray();
            _rng.GetBytes(segment);
            Array.Copy(segment, 0, buffer, offset, count);
        }

        #endregion
    }
}
