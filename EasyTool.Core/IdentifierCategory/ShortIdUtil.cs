using System;
using System.Security.Cryptography;
using System.Text;

namespace EasyTool.IdentifierCategory
{
    /// <summary>
    /// 短 ID 生成器，生成简洁的唯一标识符
    /// </summary>
    public static class ShortIdUtil
    {
        private static readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();

        // 去除易混淆字符（0OIl1）的字符集
        private static readonly char[] DefaultChars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789".ToCharArray();
        private static readonly char[] AlphanumericChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();
        private static readonly char[] LowercaseChars = "abcdefghjkmnpqrstuvwxyz23456789".ToCharArray();
        private static readonly char[] UppercaseChars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789".ToCharArray();
        private static readonly char[] NumericChars = "0123456789".ToCharArray();

        /// <summary>
        /// 生成默认短 ID（8字符）
        /// </summary>
        /// <returns>短 ID</returns>
        public static string Generate()
        {
            return Generate(8);
        }

        /// <summary>
        /// 生成指定长度的短 ID
        /// </summary>
        /// <param name="length">长度（建议 6-16）</param>
        /// <returns>短 ID</returns>
        public static string Generate(int length)
        {
            return Generate(length, ShortIdOptions.Default);
        }

        /// <summary>
        /// 使用指定选项生成短 ID
        /// </summary>
        /// <param name="length">长度</param>
        /// <param name="options">选项</param>
        /// <returns>短 ID</returns>
        public static string Generate(int length, ShortIdOptions options)
        {
            if (length <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "长度必须大于 0");
            }

            var chars = GetChars(options);
            var result = new char[length];
            var bytes = new byte[length];

            _rng.GetBytes(bytes);

            for (int i = 0; i < length; i++)
            {
                result[i] = chars[bytes[i] % chars.Length];
            }

            return new string(result);
        }

        /// <summary>
        /// 生成带前缀的短 ID
        /// </summary>
        /// <param name="prefix">前缀</param>
        /// <param name="length">ID 部分长度</param>
        /// <returns>带前缀的短 ID</returns>
        public static string GenerateWithPrefix(string prefix, int length = 8)
        {
            return $"{prefix}{Generate(length)}";
        }

        /// <summary>
        /// 生成小写短 ID
        /// </summary>
        /// <param name="length">长度</param>
        /// <returns>小写短 ID</returns>
        public static string GenerateLowercase(int length = 8)
        {
            return Generate(length, ShortIdOptions.Lowercase);
        }

        /// <summary>
        /// 生成大写短 ID
        /// </summary>
        /// <param name="length">长度</param>
        /// <returns>大写短 ID</returns>
        public static string GenerateUppercase(int length = 8)
        {
            return Generate(length, ShortIdOptions.Uppercase);
        }

        /// <summary>
        /// 生成纯数字短 ID
        /// </summary>
        /// <param name="length">长度</param>
        /// <returns>纯数字短 ID</returns>
        public static string GenerateNumeric(int length = 8)
        {
            return Generate(length, ShortIdOptions.Numeric);
        }

        /// <summary>
        /// 生成基于时间的短 ID（可排序）
        /// </summary>
        /// <param name="randomLength">随机部分长度</param>
        /// <returns>基于时间的短 ID</returns>
        public static string GenerateTimeBased(int randomLength = 4)
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var timestampBase36 = ToBase36(timestamp);
            var randomPart = Generate(randomLength, ShortIdOptions.Lowercase);
            return $"{timestampBase36}{randomPart}";
        }

        /// <summary>
        /// 生成邀请码风格短 ID（易于阅读和朗读）
        /// </summary>
        /// <param name="segments">分段数</param>
        /// <param name="segmentLength">每段长度</param>
        /// <returns>邀请码风格短 ID</returns>
        public static string GenerateInviteCode(int segments = 3, int segmentLength = 4)
        {
            var parts = new string[segments];
            for (int i = 0; i < segments; i++)
            {
                parts[i] = Generate(segmentLength, ShortIdOptions.Uppercase);
            }
            return string.Join("-", parts);
        }

        /// <summary>
        /// 生成优化的 URL 短链接 ID
        /// </summary>
        /// <param name="length">长度（建议 6-8）</param>
        /// <returns>URL 友好的短 ID</returns>
        public static string GenerateUrlFriendly(int length = 6)
        {
            return Generate(length, ShortIdOptions.Lowercase);
        }

        /// <summary>
        /// 生成订单号风格短 ID
        /// </summary>
        /// <param name="prefix">前缀（如 ORD）</param>
        /// <returns>订单号风格短 ID</returns>
        public static string GenerateOrderNumber(string prefix = "ORD")
        {
            var date = DateTime.UtcNow.ToString("yyyyMMdd");
            var random = GenerateNumeric(6);
            return $"{prefix}{date}{random}";
        }

        /// <summary>
        /// 生成优惠券码风格短 ID
        /// </summary>
        /// <param name="length">长度</param>
        /// <returns>优惠券码</returns>
        public static string GenerateCouponCode(int length = 12)
        {
            return Generate(length, ShortIdOptions.Uppercase);
        }

        /// <summary>
        /// 从整数生成短 ID
        /// </summary>
        /// <param name="number">数字</param>
        /// <returns>短 ID</returns>
        public static string FromNumber(long number)
        {
            return ToBase62(number);
        }

        /// <summary>
        /// 将短 ID 转换为整数
        /// </summary>
        /// <param name="shortId">短 ID</param>
        /// <returns>数字</returns>
        public static long ToNumber(string shortId)
        {
            return FromBase62(shortId);
        }

        /// <summary>
        /// 生成唯一短 ID（带校验）
        /// </summary>
        /// <param name="length">长度（不含校验位）</param>
        /// <returns>带校验位的短 ID</returns>
        public static string GenerateWithChecksum(int length = 7)
        {
            var id = Generate(length);
            var checksum = ComputeChecksum(id);
            return $"{id}{checksum}";
        }

        /// <summary>
        /// 验证带校验位的短 ID
        /// </summary>
        /// <param name="shortId">带校验位的短 ID</param>
        /// <returns>是否有效</returns>
        public static bool ValidateChecksum(string shortId)
        {
            if (string.IsNullOrEmpty(shortId) || shortId.Length < 2)
            {
                return false;
            }

            var id = shortId[..^1];
            var checksum = shortId[^1];
            return checksum == ComputeChecksum(id);
        }

        #region 私有方法

        private static char[] GetChars(ShortIdOptions options)
        {
            return options switch
            {
                ShortIdOptions.Lowercase => LowercaseChars,
                ShortIdOptions.Uppercase => UppercaseChars,
                ShortIdOptions.Numeric => NumericChars,
                ShortIdOptions.Alphanumeric => AlphanumericChars,
                _ => DefaultChars
            };
        }

        private static string ToBase36(long number)
        {
            const string chars = "0123456789abcdefghijklmnopqrstuvwxyz";
            if (number < 0)
            {
                throw new ArgumentException("数字必须为非负数", nameof(number));
            }

            if (number == 0)
            {
                return "0";
            }

            var result = new StringBuilder();
            while (number > 0)
            {
                result.Insert(0, chars[(int)(number % 36)]);
                number /= 36;
            }
            return result.ToString();
        }

        private static string ToBase62(long number)
        {
            const string chars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            if (number < 0)
            {
                throw new ArgumentException("数字必须为非负数", nameof(number));
            }

            if (number == 0)
            {
                return "0";
            }

            var result = new StringBuilder();
            while (number > 0)
            {
                result.Insert(0, chars[(int)(number % 62)]);
                number /= 62;
            }
            return result.ToString();
        }

        private static long FromBase62(string str)
        {
            const string chars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            long result = 0;

            foreach (var c in str)
            {
                result = result * 62 + chars.IndexOf(c);
            }

            return result;
        }

        private static char ComputeChecksum(string id)
        {
            var chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            int sum = 0;
            foreach (var c in id)
            {
                sum += chars.IndexOf(char.ToUpper(c));
            }
            return chars[sum % chars.Length];
        }

        #endregion
    }

    /// <summary>
    /// 短 ID 生成选项
    /// </summary>
    public enum ShortIdOptions
    {
        /// <summary>
        /// 默认（去除易混淆字符）
        /// </summary>
        Default,

        /// <summary>
        /// 小写字母和数字
        /// </summary>
        Lowercase,

        /// <summary>
        /// 大写字母和数字
        /// </summary>
        Uppercase,

        /// <summary>
        /// 纯数字
        /// </summary>
        Numeric,

        /// <summary>
        /// 完整字母数字（包含易混淆字符）
        /// </summary>
        Alphanumeric
    }

    /// <summary>
    /// 短 ID 生成器配置
    /// </summary>
    public class ShortIdGenerator
    {
        private readonly int _length;
        private readonly ShortIdOptions _options;
        private readonly string? _prefix;
        private readonly string? _suffix;

        /// <summary>
        /// 创建短 ID 生成器
        /// </summary>
        /// <param name="length">长度</param>
        /// <param name="options">选项</param>
        /// <param name="prefix">前缀</param>
        /// <param name="suffix">后缀</param>
        public ShortIdGenerator(int length = 8, ShortIdOptions options = ShortIdOptions.Default, string? prefix = null, string? suffix = null)
        {
            _length = length;
            _options = options;
            _prefix = prefix;
            _suffix = suffix;
        }

        /// <summary>
        /// 生成短 ID
        /// </summary>
        /// <returns>短 ID</returns>
        public string Generate()
        {
            var id = ShortIdUtil.Generate(_length, _options);
            return $"{_prefix}{id}{_suffix}";
        }

        /// <summary>
        /// 批量生成短 ID
        /// </summary>
        /// <param name="count">数量</param>
        /// <returns>短 ID 列表</returns>
        public string[] GenerateMany(int count)
        {
            var result = new string[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = Generate();
            }
            return result;
        }

        /// <summary>
        /// 创建默认生成器
        /// </summary>
        public static ShortIdGenerator Default => new();

        /// <summary>
        /// 创建 URL 友好生成器
        /// </summary>
        public static ShortIdGenerator UrlFriendly => new(6, ShortIdOptions.Lowercase);

        /// <summary>
        /// 创建邀请码生成器
        /// </summary>
        public static ShortIdGenerator InviteCode => new(12, ShortIdOptions.Uppercase);

        /// <summary>
        /// 创建订单号生成器
        /// </summary>
        public static ShortIdGenerator OrderNumber => new(10, ShortIdOptions.Numeric, "ORD");
    }
}
