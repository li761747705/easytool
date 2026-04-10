using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace EasyTool.BusinessCategory
{
    /// <summary>
    /// 护照类型枚举
    /// </summary>
    public enum PassportType
    {
        /// <summary>
        /// 未知类型
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// 中国普通护照（E开头+8位数字）
        /// </summary>
        ChinaOrdinary = 1,

        /// <summary>
        /// 中国公务护照（SE开头+7位数字）
        /// </summary>
        ChinaService = 2,

        /// <summary>
        /// 中国外交护照（DE开头+7位数字）
        /// </summary>
        ChinaDiplomatic = 3,

        /// <summary>
        /// 中国香港特区护照
        /// </summary>
        HongKong = 4,

        /// <summary>
        /// 中国澳门特区护照
        /// </summary>
        Macau = 5,

        /// <summary>
        /// 台湾护照
        /// </summary>
        Taiwan = 6
    }

    /// <summary>
    /// 护照号工具类
    /// </summary>
    public static class PassportUtil
    {
        #region 常量与私有字段

        /// <summary>
        /// 中国普通护照正则（E+8位数字）
        /// </summary>
        private static readonly Regex ChinaOrdinaryRegex = new Regex(@"^[Ee]\d{8}$", RegexOptions.Compiled);

        /// <summary>
        /// 中国公务护照正则（SE+7位数字）
        /// </summary>
        private static readonly Regex ChinaServiceRegex = new Regex(@"^[Ss][Ee]\d{7}$", RegexOptions.Compiled);

        /// <summary>
        /// 中国外交护照正则（DE+7位数字）
        /// </summary>
        private static readonly Regex ChinaDiplomaticRegex = new Regex(@"^[Dd][Ee]\d{7}$", RegexOptions.Compiled);

        /// <summary>
        /// 中国香港护照正则（K+8位数字 或 881/159开头+7位数字）
        /// </summary>
        private static readonly Regex HongKongRegex = new Regex(@"^([Kk]\d{8}|(881|159)\d{7})$", RegexOptions.Compiled);

        /// <summary>
        /// 中国澳门护照正则（578开头+7位数字 或 1+7位数字）
        /// </summary>
        private static readonly Regex MacauRegex = new Regex(@"^(578\d{7}|[1]\d{7})$", RegexOptions.Compiled);

        /// <summary>
        /// 台湾护照正则（数字+字母混合，9-10位）
        /// </summary>
        private static readonly Regex TaiwanRegex = new Regex(@"^\d{8,9}$|^[A-Za-z]\d{8,9}$", RegexOptions.Compiled);

        /// <summary>
        /// 通用护照号正则（2-3位字母+6-9位数字，或纯数字8-9位）
        /// </summary>
        private static readonly Regex GeneralPassportRegex = new Regex(
            @"^([A-Za-z]{1,3}\d{6,9}|\d{8,9})$",
            RegexOptions.Compiled);

        /// <summary>
        /// 非字母数字正则表达式
        /// </summary>
        private static readonly Regex NonAlphanumericRegex = new Regex(@"[^A-Z0-9]", RegexOptions.Compiled);

        #endregion

        #region 验证方法

        /// <summary>
        /// 验证护照号是否有效（自动识别类型）
        /// </summary>
        /// <param name="passportNumber">护照号</param>
        /// <returns>是否有效</returns>
        public static bool IsValid(string? passportNumber)
        {
            return GetPassportType(passportNumber) != PassportType.Unknown;
        }

        /// <summary>
        /// 验证中国普通护照号是否有效
        /// </summary>
        /// <param name="passportNumber">护照号</param>
        /// <returns>是否有效</returns>
        public static bool IsChinaOrdinary(string? passportNumber)
        {
            if (string.IsNullOrWhiteSpace(passportNumber))
            {
                return false;
            }

            return ChinaOrdinaryRegex.IsMatch(passportNumber);
        }

        /// <summary>
        /// 验证中国公务护照号是否有效
        /// </summary>
        /// <param name="passportNumber">护照号</param>
        /// <returns>是否有效</returns>
        public static bool IsChinaService(string? passportNumber)
        {
            if (string.IsNullOrWhiteSpace(passportNumber))
            {
                return false;
            }

            return ChinaServiceRegex.IsMatch(passportNumber);
        }

        /// <summary>
        /// 验证中国外交护照号是否有效
        /// </summary>
        /// <param name="passportNumber">护照号</param>
        /// <returns>是否有效</returns>
        public static bool IsChinaDiplomatic(string? passportNumber)
        {
            if (string.IsNullOrWhiteSpace(passportNumber))
            {
                return false;
            }

            return ChinaDiplomaticRegex.IsMatch(passportNumber);
        }

        /// <summary>
        /// 验证中国香港护照号是否有效
        /// </summary>
        /// <param name="passportNumber">护照号</param>
        /// <returns>是否有效</returns>
        public static bool IsHongKong(string? passportNumber)
        {
            if (string.IsNullOrWhiteSpace(passportNumber))
            {
                return false;
            }

            return HongKongRegex.IsMatch(passportNumber);
        }

        /// <summary>
        /// 验证中国澳门护照号是否有效
        /// </summary>
        /// <param name="passportNumber">护照号</param>
        /// <returns>是否有效</returns>
        public static bool IsMacau(string? passportNumber)
        {
            if (string.IsNullOrWhiteSpace(passportNumber))
            {
                return false;
            }

            return MacauRegex.IsMatch(passportNumber);
        }

        /// <summary>
        /// 验证台湾护照号是否有效
        /// </summary>
        /// <param name="passportNumber">护照号</param>
        /// <returns>是否有效</returns>
        public static bool IsTaiwan(string? passportNumber)
        {
            if (string.IsNullOrWhiteSpace(passportNumber))
            {
                return false;
            }

            return TaiwanRegex.IsMatch(passportNumber);
        }

        /// <summary>
        /// 验证是否为中国大陆护照（含普通、公务、外交）
        /// </summary>
        /// <param name="passportNumber">护照号</param>
        /// <returns>是否为中国大陆护照</returns>
        public static bool IsChinaMainland(string? passportNumber)
        {
            return IsChinaOrdinary(passportNumber) ||
                   IsChinaService(passportNumber) ||
                   IsChinaDiplomatic(passportNumber);
        }

        #endregion

        #region 类型识别

        /// <summary>
        /// 获取护照类型
        /// </summary>
        /// <param name="passportNumber">护照号</param>
        /// <returns>护照类型</returns>
        public static PassportType GetPassportType(string? passportNumber)
        {
            if (string.IsNullOrWhiteSpace(passportNumber))
            {
                return PassportType.Unknown;
            }

            string upper = passportNumber.ToUpper();

            // 中国普通护照
            if (ChinaOrdinaryRegex.IsMatch(upper))
            {
                return PassportType.ChinaOrdinary;
            }

            // 中国公务护照
            if (ChinaServiceRegex.IsMatch(upper))
            {
                return PassportType.ChinaService;
            }

            // 中国外交护照
            if (ChinaDiplomaticRegex.IsMatch(upper))
            {
                return PassportType.ChinaDiplomatic;
            }

            // 香港护照
            if (HongKongRegex.IsMatch(upper))
            {
                return PassportType.HongKong;
            }

            // 澳门护照
            if (MacauRegex.IsMatch(upper))
            {
                return PassportType.Macau;
            }

            // 台湾护照
            if (TaiwanRegex.IsMatch(upper))
            {
                return PassportType.Taiwan;
            }

            return PassportType.Unknown;
        }

        /// <summary>
        /// 获取护照类型名称
        /// </summary>
        /// <param name="passportNumber">护照号</param>
        /// <returns>护照类型名称</returns>
        public static string? GetPassportTypeName(string? passportNumber)
        {
            PassportType type = GetPassportType(passportNumber);
            return type switch
            {
                PassportType.ChinaOrdinary => "中国普通护照",
                PassportType.ChinaService => "中国公务护照",
                PassportType.ChinaDiplomatic => "中国外交护照",
                PassportType.HongKong => "香港特区护照",
                PassportType.Macau => "澳门特区护照",
                PassportType.Taiwan => "台湾护照",
                _ => null
            };
        }

        /// <summary>
        /// 获取护照类型描述
        /// </summary>
        /// <param name="type">护照类型</param>
        /// <returns>类型描述</returns>
        public static string GetTypeDescription(PassportType type)
        {
            return type switch
            {
                PassportType.ChinaOrdinary => "中国普通护照（E+8位数字）",
                PassportType.ChinaService => "中国公务护照（SE+7位数字）",
                PassportType.ChinaDiplomatic => "中国外交护照（DE+7位数字）",
                PassportType.HongKong => "香港特区护照（K+8位数字）",
                PassportType.Macau => "澳门特区护照（578开头+7位数字）",
                PassportType.Taiwan => "台湾护照（8-9位数字）",
                _ => "未知类型"
            };
        }

        #endregion

        #region 格式化方法

        /// <summary>
        /// 格式化护照号（转大写，去除空格和特殊字符）
        /// </summary>
        /// <param name="passportNumber">护照号</param>
        /// <returns>格式化后的护照号</returns>
        public static string? Normalize(string? passportNumber)
        {
            if (string.IsNullOrWhiteSpace(passportNumber))
            {
                return null;
            }

            // 去除空格和特殊字符，转大写
            string normalized = passportNumber.ToUpper().Trim();
            normalized = NonAlphanumericRegex.Replace(normalized, "");

            return normalized;
        }

        /// <summary>
        /// 护照号脱敏：E********（保留首字母）
        /// </summary>
        /// <param name="passportNumber">护照号</param>
        /// <returns>脱敏后的护照号</returns>
        public static string? Mask(string? passportNumber)
        {
            string? normalized = Normalize(passportNumber);
            if (normalized == null)
            {
                return null;
            }

            // 保留首字符，其余用*代替
            if (normalized.Length <= 2)
            {
                return normalized[0] + "*";
            }

            // 保留前2位和后2位
            return normalized.Substring(0, 2) + new string('*', normalized.Length - 4) + normalized.Substring(normalized.Length - 2);
        }

        #endregion

        #region 生成方法

        /// <summary>
        /// 生成随机护照号（仅供测试使用）
        /// </summary>
        /// <param name="type">护照类型（可选，默认中国普通护照）</param>
        /// <returns>护照号</returns>
        public static string GenerateRandom(PassportType type = PassportType.ChinaOrdinary)
        {
            return type switch
            {
                PassportType.ChinaOrdinary => "E" + MathCategory.RandomUtil.RandomDigitString(8),
                PassportType.ChinaService => "SE" + MathCategory.RandomUtil.RandomDigitString(7),
                PassportType.ChinaDiplomatic => "DE" + MathCategory.RandomUtil.RandomDigitString(7),
                PassportType.HongKong => "K" + MathCategory.RandomUtil.RandomDigitString(8),
                PassportType.Macau => "578" + MathCategory.RandomUtil.RandomDigitString(7),
                PassportType.Taiwan => MathCategory.RandomUtil.RandomDigitString(9),
                _ => "E" + MathCategory.RandomUtil.RandomDigitString(8)
            };
        }

        #endregion
    }
}
