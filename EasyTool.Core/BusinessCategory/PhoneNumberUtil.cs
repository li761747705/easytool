using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace EasyTool.BusinessCategory
{
    /// <summary>
    /// 手机运营商枚举
    /// </summary>
    public enum Carrier
    {
        /// <summary>
        /// 未知运营商
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// 中国移动
        /// </summary>
        ChinaMobile = 1,

        /// <summary>
        /// 中国联通
        /// </summary>
        ChinaUnicom = 2,

        /// <summary>
        /// 中国电信
        /// </summary>
        ChinaTelecom = 3,

        /// <summary>
        /// 中国广电
        /// </summary>
        ChinaBroadnet = 4
    }

    /// <summary>
    /// 手机号工具类
    /// </summary>
    public static class PhoneNumberUtil
    {
        #region 常量与私有字段

        /// <summary>
        /// 手机号正则表达式（11位，1开头）
        /// </summary>
        private static readonly Regex PhoneRegex = new Regex(@"^1[3-9]\d{9}$", RegexOptions.Compiled);

        /// <summary>
        /// 非数字字符正则表达式
        /// </summary>
        private static readonly Regex NonDigitRegex = new Regex(@"\D", RegexOptions.Compiled);

        /// <summary>
        /// 中国移动号段（前3-4位）
        /// </summary>
        private static readonly HashSet<string> ChinaMobilePrefixes = new HashSet<string>
        {
            "134", "135", "136", "137", "138", "139", "147", "150", "151", "152",
            "157", "158", "159", "172", "178", "182", "183", "184", "187", "188",
            "195", "197", "198"
        };

        /// <summary>
        /// 中国联通号段（前3-4位）
        /// </summary>
        private static readonly HashSet<string> ChinaUnicomPrefixes = new HashSet<string>
        {
            "130", "131", "132", "145", "155", "156", "166", "167", "175", "176",
            "185", "186", "196"
        };

        /// <summary>
        /// 中国电信号段（前3-4位）
        /// </summary>
        private static readonly HashSet<string> ChinaTelecomPrefixes = new HashSet<string>
        {
            "133", "149", "153", "173", "174", "177", "180", "181", "189", "191",
            "193", "199"
        };

        /// <summary>
        /// 中国广电号段（前3-4位）
        /// </summary>
        private static readonly HashSet<string> ChinaBroadnetPrefixes = new HashSet<string>
        {
            "192"
        };

        #endregion

        #region 验证方法

        /// <summary>
        /// 验证手机号格式是否有效
        /// </summary>
        /// <param name="phoneNumber">手机号</param>
        /// <returns>是否有效</returns>
        public static bool IsValid(string? phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return false;
            }

            return PhoneRegex.IsMatch(phoneNumber);
        }

        /// <summary>
        /// 格式化并验证手机号（去除非数字字符后验证）
        /// </summary>
        /// <param name="phoneNumber">手机号</param>
        /// <returns>格式化后的手机号，无效返回null</returns>
        public static string? Normalize(string? phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return null;
            }

            // 去除所有非数字字符
            string normalized = NonDigitRegex.Replace(phoneNumber, "");

            // 处理中国国际区号 +86
            if (normalized.StartsWith("86") && normalized.Length > 11)
            {
                normalized = normalized.Substring(2);
            }

            if (!IsValid(normalized))
            {
                return null;
            }

            return normalized;
        }

        #endregion

        #region 运营商识别

        /// <summary>
        /// 获取运营商枚举
        /// </summary>
        /// <param name="phoneNumber">手机号</param>
        /// <returns>运营商枚举</returns>
        public static Carrier GetCarrier(string? phoneNumber)
        {
            if (!IsValid(phoneNumber))
            {
                return Carrier.Unknown;
            }

            string prefix3 = phoneNumber!.Substring(0, 3);

            if (ChinaMobilePrefixes.Contains(prefix3))
            {
                return Carrier.ChinaMobile;
            }

            if (ChinaUnicomPrefixes.Contains(prefix3))
            {
                return Carrier.ChinaUnicom;
            }

            if (ChinaTelecomPrefixes.Contains(prefix3))
            {
                return Carrier.ChinaTelecom;
            }

            if (ChinaBroadnetPrefixes.Contains(prefix3))
            {
                return Carrier.ChinaBroadnet;
            }

            return Carrier.Unknown;
        }

        /// <summary>
        /// 获取运营商名称
        /// </summary>
        /// <param name="phoneNumber">手机号</param>
        /// <returns>运营商名称</returns>
        public static string? GetCarrierName(string? phoneNumber)
        {
            Carrier carrier = GetCarrier(phoneNumber);
            return carrier switch
            {
                Carrier.ChinaMobile => "中国移动",
                Carrier.ChinaUnicom => "中国联通",
                Carrier.ChinaTelecom => "中国电信",
                Carrier.ChinaBroadnet => "中国广电",
                _ => null
            };
        }

        /// <summary>
        /// 判断是否为中国移动号码
        /// </summary>
        /// <param name="phoneNumber">手机号</param>
        /// <returns>是否为移动号码</returns>
        public static bool IsChinaMobile(string? phoneNumber)
        {
            return GetCarrier(phoneNumber) == Carrier.ChinaMobile;
        }

        /// <summary>
        /// 判断是否为中国联通号码
        /// </summary>
        /// <param name="phoneNumber">手机号</param>
        /// <returns>是否为联通号码</returns>
        public static bool IsChinaUnicom(string? phoneNumber)
        {
            return GetCarrier(phoneNumber) == Carrier.ChinaUnicom;
        }

        /// <summary>
        /// 判断是否为中国电信号码
        /// </summary>
        /// <param name="phoneNumber">手机号</param>
        /// <returns>是否为电信号码</returns>
        public static bool IsChinaTelecom(string? phoneNumber)
        {
            return GetCarrier(phoneNumber) == Carrier.ChinaTelecom;
        }

        /// <summary>
        /// 判断是否为中国广电号码
        /// </summary>
        /// <param name="phoneNumber">手机号</param>
        /// <returns>是否为广电号码</returns>
        public static bool IsChinaBroadnet(string? phoneNumber)
        {
            return GetCarrier(phoneNumber) == Carrier.ChinaBroadnet;
        }

        #endregion

        #region 格式化方法

        /// <summary>
        /// 格式化手机号（空格分隔）：138 8888 8888
        /// </summary>
        /// <param name="phoneNumber">手机号</param>
        /// <returns>格式化后的手机号</returns>
        public static string? FormatWithSpaces(string? phoneNumber)
        {
            string? normalized = Normalize(phoneNumber);
            if (normalized == null)
            {
                return null;
            }

            return $"{normalized.Substring(0, 3)} {normalized.Substring(3, 4)} {normalized.Substring(7, 4)}";
        }

        /// <summary>
        /// 格式化手机号（横线分隔）：138-8888-8888
        /// </summary>
        /// <param name="phoneNumber">手机号</param>
        /// <returns>格式化后的手机号</returns>
        public static string? FormatWithHyphens(string? phoneNumber)
        {
            string? normalized = Normalize(phoneNumber);
            if (normalized == null)
            {
                return null;
            }

            return $"{normalized.Substring(0, 3)}-{normalized.Substring(3, 4)}-{normalized.Substring(7, 4)}";
        }

        /// <summary>
        /// 格式化手机号（带国际区号）：+86 13888888888
        /// </summary>
        /// <param name="phoneNumber">手机号</param>
        /// <returns>格式化后的手机号</returns>
        public static string? FormatWithCountryCode(string? phoneNumber)
        {
            string? normalized = Normalize(phoneNumber);
            if (normalized == null)
            {
                return null;
            }

            return $"+86 {normalized}";
        }

        /// <summary>
        /// 手机号脱敏：138****8888
        /// </summary>
        /// <param name="phoneNumber">手机号</param>
        /// <returns>脱敏后的手机号</returns>
        public static string? Mask(string? phoneNumber)
        {
            string? normalized = Normalize(phoneNumber);
            if (normalized == null)
            {
                return null;
            }

            return $"{normalized.Substring(0, 3)}****{normalized.Substring(7, 4)}";
        }

        #endregion

        #region 生成方法

        /// <summary>
        /// 生成随机手机号（仅供测试使用）
        /// </summary>
        /// <param name="carrier">运营商（可选，默认随机）</param>
        /// <returns>11位手机号</returns>
        public static string GenerateRandom(Carrier? carrier = null)
        {
            string prefix;

            if (carrier.HasValue && carrier.Value != Carrier.Unknown)
            {
                prefix = carrier.Value switch
                {
                    Carrier.ChinaMobile => MathCategory.RandomUtil.GetRandomElement(ChinaMobilePrefixes),
                    Carrier.ChinaUnicom => MathCategory.RandomUtil.GetRandomElement(ChinaUnicomPrefixes),
                    Carrier.ChinaTelecom => MathCategory.RandomUtil.GetRandomElement(ChinaTelecomPrefixes),
                    Carrier.ChinaBroadnet => MathCategory.RandomUtil.GetRandomElement(ChinaBroadnetPrefixes),
                    _ => GetRandomPrefix()
                };
            }
            else
            {
                prefix = GetRandomPrefix();
            }

            // 生成剩余8位数字
            string suffix = MathCategory.RandomUtil.RandomDigitString(8);

            return prefix + suffix;
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 获取随机号段前缀
        /// </summary>
        private static string GetRandomPrefix()
        {
            var allPrefixes = new List<string>();
            allPrefixes.AddRange(ChinaMobilePrefixes);
            allPrefixes.AddRange(ChinaUnicomPrefixes);
            allPrefixes.AddRange(ChinaTelecomPrefixes);
            allPrefixes.AddRange(ChinaBroadnetPrefixes);

            return MathCategory.RandomUtil.GetRandomElement(allPrefixes);
        }

        #endregion
    }
}
