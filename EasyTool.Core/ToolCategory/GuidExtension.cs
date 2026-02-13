using System;
using System.Text;

namespace EasyTool.Extension
{
    /// <summary>
    /// Guid 扩展方法
    /// </summary>
    public static class GuidExtension
    {
        #region 空值判断


        /// <summary>
        /// 判断 Guid 是否为空或默认值
        /// </summary>
        public static bool IsNullOrEmpty(this Guid? guid)
        {
            return guid == null || guid.Value == Guid.Empty;
        }

        /// <summary>
        /// 判断 Guid 是否有值（非空）
        /// </summary>
        public static bool HasValue(this Guid guid)
        {
            return guid != Guid.Empty;
        }

        /// <summary>
        /// 判断可空 Guid 是否有值
        /// </summary>
        public static bool HasValue(this Guid? guid)
        {
            return guid.HasValue && guid.Value != Guid.Empty;
        }

        #endregion

        #region 格式化转换

        /// <summary>
        /// 获取短格式 Guid（不带连字符）
        /// </summary>
        public static string ToShortString(this Guid guid)
        {
            return guid.ToString("N");
        }

        /// <summary>
        /// 获取短格式 Guid（带连字符）
        /// </summary>
        public static string ToShortStringWithDashes(this Guid guid)
        {
            return guid.ToString("D");
        }

        /// <summary>
        /// 获取带括号的 Guid 格式
        /// </summary>
        public static string ToFormattedString(this Guid guid)
        {
            return guid.ToString("B");
        }

        /// <summary>
        /// 获取带大括号的 Guid 格式
        /// </summary>
        public static string ToBracedString(this Guid guid)
        {
            return guid.ToString("B");
        }


        /// <summary>
        /// 将 Guid 转换为 Base64 字符串
        /// </summary>
        public static string ToBase64String(this Guid guid)
        {
            return Convert.ToBase64String(guid.ToByteArray());
        }

        /// <summary>
        /// 从 Base64 字符串创建 Guid
        /// </summary>
        public static Guid FromBase64String(this string base64)
        {
            var bytes = Convert.FromBase64String(base64);
            return new Guid(bytes);
        }

        #endregion

        #region 字符串解析

        /// <summary>
        /// 尝试解析字符串为 Guid，失败返回空 Guid
        /// </summary>
        public static Guid ToGuidOrDefault(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return Guid.Empty;

            return Guid.TryParse(value, out var guid) ? guid : Guid.Empty;
        }

        /// <summary>
        /// 尝试解析字符串为 Guid，失败返回默认值
        /// </summary>
        public static Guid ToGuidOrDefault(this string value, Guid defaultValue)
        {
            if (string.IsNullOrWhiteSpace(value))
                return defaultValue;

            return Guid.TryParse(value, out var guid) ? guid : defaultValue;
        }

        /// <summary>
        /// 判断字符串是否是有效的 Guid 格式
        /// </summary>
        public static bool IsValidGuid(this string value)
        {
            return !string.IsNullOrWhiteSpace(value) && Guid.TryParse(value, out _);
        }

        #endregion

        #region 加密相关

        /// <summary>
        /// 获取 Guid 的 MD5 哈希值（作为新的 Guid）
        /// </summary>
        public static Guid ToMd5Guid(this Guid guid)
        {
            using var md5 = System.Security.Cryptography.MD5.Create();
            var bytes = guid.ToByteArray();
            var hash = md5.ComputeHash(bytes);
            return new Guid(hash);
        }

        /// <summary>
        /// 获取 Guid 的 SHA1 哈希值（取前16字节作为 Guid）
        /// </summary>
        public static Guid ToSha1Guid(this Guid guid)
        {
            using var sha1 = System.Security.Cryptography.SHA1.Create();
            var bytes = guid.ToByteArray();
            var hash = sha1.ComputeHash(bytes);
            var guidBytes = new byte[16];
            Array.Copy(hash, 0, guidBytes, 0, 16);
            return new Guid(guidBytes);
        }

        #endregion

        #region Guid 生成

        /// <summary>
        /// 基于 Guid 生成连续的 Guid（适用于 COMB 类型）
        /// </summary>
        public static Guid NewCombGuid()
        {
            var guidArray = Guid.NewGuid().ToByteArray();
            var baseDate = new DateTime(1900, 1, 1);
            var now = DateTime.Now;
            var days = new TimeSpan(now.Ticks - baseDate.Ticks);
            var msecs = now.TimeOfDay;

            var daysArray = BitConverter.GetBytes(days.Days);
            var msecsArray = BitConverter.GetBytes((long)(msecs.TotalMilliseconds / 3.333333));

            Array.Reverse(daysArray);
            Array.Reverse(msecsArray);

            Array.Copy(daysArray, daysArray.Length - 2, guidArray, guidArray.Length - 6, 2);
            Array.Copy(msecsArray, msecsArray.Length - 4, guidArray, guidArray.Length - 4, 4);

            return new Guid(guidArray);
        }

        /// <summary>
        /// 基于指定前缀生成可预测的 Guid
        /// </summary>
        public static Guid NewDeterministicGuid(string prefix)
        {
            using var md5 = System.Security.Cryptography.MD5.Create();
            var prefixBytes = Encoding.UTF8.GetBytes(prefix);
            var hash = md5.ComputeHash(prefixBytes);
            return new Guid(hash);
        }

        /// <summary>
        /// 基于多个参数生成可预测的 Guid
        /// </summary>
        public static Guid NewDeterministicGuid(params object[] values)
        {
            using var md5 = System.Security.Cryptography.MD5.Create();
            var combined = string.Join("|", values);
            var bytes = Encoding.UTF8.GetBytes(combined);
            var hash = md5.ComputeHash(bytes);
            return new Guid(hash);
        }

        #endregion

        #region Guid 比较

        /// <summary>
        /// 比较两个 Guid 是否相等
        /// </summary>
        public static bool EqualsTo(this Guid guid, Guid other)
        {
            return guid.Equals(other);
        }

        /// <summary>
        /// 比较两个可空 Guid 是否相等
        /// </summary>
        public static bool EqualsTo(this Guid? guid, Guid? other)
        {
            if (guid.HasValue && other.HasValue)
                return guid.Value.Equals(other.Value);
            return guid.HasValue == other.HasValue;
        }

        #endregion

        #region Guid 操作

        /// <summary>
        /// 获取 Guid 的指定部分的值
        /// </summary>
        /// <param name="guid">Guid</param>
        /// <param name="part">部分：0-3（Data1-Data4）</param>
        public static int GetPart(this Guid guid, int part)
        {
            var bytes = guid.ToByteArray();
            return part switch
            {
                0 => BitConverter.ToInt32(bytes, 0),
                1 => BitConverter.ToInt16(bytes, 4),
                2 => BitConverter.ToInt16(bytes, 6),
                3 => bytes[8] << 24 | bytes[9] << 16 | bytes[10] << 8 | bytes[11],
                _ => throw new ArgumentOutOfRangeException(nameof(part), "Part must be between 0 and 3")
            };
        }

        /// <summary>
        /// 将 Guid 转换为整数（用于某些场景的简化处理）
        /// </summary>
        public static int ToInt32(this Guid guid)
        {
            var bytes = guid.ToByteArray();
            return BitConverter.ToInt32(bytes, 0);
        }

        /// <summary>
        /// 将 Guid 转换为长整数
        /// </summary>
        public static long ToInt64(this Guid guid)
        {
            var bytes = guid.ToByteArray();
            var high = BitConverter.ToInt64(bytes, 0);
            var low = BitConverter.ToInt64(bytes, 8);
            return high ^ low;
        }

        #endregion
    }
}
