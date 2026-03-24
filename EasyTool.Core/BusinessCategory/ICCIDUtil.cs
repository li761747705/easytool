using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace EasyTool.BusinessCategory
{
    /// <summary>
    /// SIM卡ICCID工具类
    /// ICCID (Integrated Circuit Card Identifier) 是SIM卡的唯一识别号
    /// </summary>
    public static class ICCIDUtil
    {
        #region 常量与私有字段

        /// <summary>
        /// ICCID正则表达式（19-20位数字）
        /// </summary>
        private static readonly Regex ICCIDRegex = new(
            @"^\d{19,20}$",
            RegexOptions.Compiled);

        /// <summary>
        /// 移动国家代码（MCC）映射
        /// </summary>
        private static readonly Dictionary<string, string> MccMap = new()
        {
            { "460", "中国" },
            { "001", "美国" },
            { "004", "阿富汗" },
            { "208", "法国" },
            { "234", "英国" },
            { "262", "德国" },
            { "310", "美国" },
            { "440", "日本" },
            { "450", "韩国" },
            { "505", "澳大利亚" },
            { "530", "新西兰" },
            { "724", "巴西" }
        };

        /// <summary>
        /// 中国移动网络代码（MNC）映射
        /// </summary>
        private static readonly Dictionary<string, string> ChinaMncMap = new()
        {
            { "00", "中国移动" },
            { "02", "中国移动" },
            { "04", "中国移动" },
            { "07", "中国移动" },
            { "08", "中国移动" },
            { "01", "中国联通" },
            { "06", "中国联通" },
            { "09", "中国联通" },
            { "03", "中国电信" },
            { "05", "中国电信" },
            { "11", "中国电信" },
            { "15", "中国广电" }
        };

        /// <summary>
        /// Luhn算法校验码权重
        /// </summary>
        private static readonly int[] LuhnWeights = { 2, 1, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1, 2 };

        #endregion

        #region 验证方法

        /// <summary>
        /// 验证ICCID是否有效
        /// </summary>
        /// <param name="iccid">ICCID号</param>
        /// <returns>是否有效</returns>
        public static bool IsValid(string? iccid)
        {
            if (!IsValidFormat(iccid))
            {
                return false;
            }

            return ValidateLuhn(iccid!);
        }

        /// <summary>
        /// 验证ICCID格式
        /// </summary>
        /// <param name="iccid">ICCID号</param>
        /// <returns>格式是否正确</returns>
        public static bool IsValidFormat(string? iccid)
        {
            if (string.IsNullOrWhiteSpace(iccid))
            {
                return false;
            }

            string cleaned = iccid.Trim();
            return ICCIDRegex.IsMatch(cleaned);
        }

        /// <summary>
        /// 使用Luhn算法验证ICCID
        /// </summary>
        /// <param name="iccid">ICCID号</param>
        /// <returns>是否通过Luhn校验</returns>
        public static bool ValidateLuhn(string? iccid)
        {
            if (string.IsNullOrWhiteSpace(iccid))
            {
                return false;
            }

            string cleaned = iccid.Trim();
            int sum = 0;
            int length = cleaned.Length;

            // 从右向左，第1位是校验位
            for (int i = length - 2; i >= 0; i--)
            {
                if (!char.IsDigit(cleaned[i]))
                {
                    return false;
                }

                int digit = cleaned[i] - '0';
                int weightIndex = (length - 2 - i);
                int multiplier = (weightIndex % 2 == 0) ? 2 : 1;

                digit *= multiplier;
                if (digit > 9)
                {
                    digit -= 9;
                }

                sum += digit;
            }

            int checkDigit = (10 - (sum % 10)) % 10;
            int actualCheck = cleaned[length - 1] - '0';

            return checkDigit == actualCheck;
        }

        #endregion

        #region 信息提取

        /// <summary>
        /// 获取移动国家代码（MCC，前3位）
        /// </summary>
        /// <param name="iccid">ICCID号</param>
        /// <returns>移动国家代码</returns>
        public static string? GetMCC(string? iccid)
        {
            if (!IsValidFormat(iccid))
            {
                return null;
            }

            return iccid!.Substring(0, 3);
        }

        /// <summary>
        /// 获取国家名称
        /// </summary>
        /// <param name="iccid">ICCID号</param>
        /// <returns>国家名称</returns>
        public static string? GetCountry(string? iccid)
        {
            string? mcc = GetMCC(iccid);
            if (mcc == null)
            {
                return null;
            }

            return MccMap.TryGetValue(mcc, out string? country) ? country : null;
        }

        /// <summary>
        /// 获取移动网络代码（MNC，第4-5位）
        /// </summary>
        /// <param name="iccid">ICCID号</param>
        /// <returns>移动网络代码</returns>
        public static string? GetMNC(string? iccid)
        {
            if (!IsValidFormat(iccid))
            {
                return null;
            }

            return iccid!.Substring(3, 2);
        }

        /// <summary>
        /// 获取运营商名称（仅支持中国运营商）
        /// </summary>
        /// <param name="iccid">ICCID号</param>
        /// <returns>运营商名称</returns>
        public static string? GetCarrier(string? iccid)
        {
            if (!IsValidFormat(iccid))
            {
                return null;
            }

            string? mcc = GetMCC(iccid);
            if (mcc != "460")
            {
                return null; // 非中国卡
            }

            string mnc = iccid!.Substring(3, 2);
            return ChinaMncMap.TryGetValue(mnc, out string? carrier) ? carrier : null;
        }

        /// <summary>
        /// 判断是否为中国移动
        /// </summary>
        public static bool IsChinaMobile(string? iccid) => GetCarrier(iccid) == "中国移动";

        /// <summary>
        /// 判断是否为中国联通
        /// </summary>
        public static bool IsChinaUnicom(string? iccid) => GetCarrier(iccid) == "中国联通";

        /// <summary>
        /// 判断是否为中国电信
        /// </summary>
        public static bool IsChinaTelecom(string? iccid) => GetCarrier(iccid) == "中国电信";

        /// <summary>
        /// 判断是否为中国广电
        /// </summary>
        public static bool IsChinaBroadnet(string? iccid) => GetCarrier(iccid) == "中国广电";

        /// <summary>
        /// 获取发卡省份代码（第9-10位）
        /// </summary>
        /// <param name="iccid">ICCID号</param>
        /// <returns>省份代码</returns>
        public static string? GetProvinceCode(string? iccid)
        {
            if (!IsValidFormat(iccid) || iccid!.Length < 10)
            {
                return null;
            }

            return iccid.Substring(8, 2);
        }

        /// <summary>
        /// 获取序列号（第11-19位，不含校验位）
        /// </summary>
        /// <param name="iccid">ICCID号</param>
        /// <returns>序列号</returns>
        public static string? GetSerialNumber(string? iccid)
        {
            if (!IsValidFormat(iccid))
            {
                return null;
            }

            int length = iccid!.Length;
            return iccid.Substring(10, length - 11);
        }

        /// <summary>
        /// 获取校验位（最后一位）
        /// </summary>
        /// <param name="iccid">ICCID号</param>
        /// <returns>校验位</returns>
        public static int? GetCheckDigit(string? iccid)
        {
            if (!IsValidFormat(iccid))
            {
                return null;
            }

            return iccid![iccid.Length - 1] - '0';
        }

        /// <summary>
        /// 解析ICCID结构
        /// </summary>
        /// <param name="iccid">ICCID号</param>
        /// <returns>ICCID结构信息</returns>
        public static ICCDInfo? Parse(string? iccid)
        {
            if (!IsValidFormat(iccid))
            {
                return null;
            }

            return new ICCDInfo
            {
                MCC = GetMCC(iccid),
                Country = GetCountry(iccid),
                MNC = GetMNC(iccid),
                Carrier = GetCarrier(iccid),
                ProvinceCode = GetProvinceCode(iccid),
                SerialNumber = GetSerialNumber(iccid),
                CheckDigit = GetCheckDigit(iccid)
            };
        }

        #endregion

        #region 格式化方法

        /// <summary>
        /// 格式化ICCID（去除空格和分隔符）
        /// </summary>
        /// <param name="iccid">ICCID号</param>
        /// <returns>格式化后的ICCID</returns>
        public static string? Normalize(string? iccid)
        {
            if (string.IsNullOrWhiteSpace(iccid))
            {
                return null;
            }

            string cleaned = iccid.Trim();
            return ICCIDRegex.IsMatch(cleaned) ? cleaned : null;
        }

        /// <summary>
        /// 格式化为易读格式：898600 00 00 1234567890
        /// </summary>
        /// <param name="iccid">ICCID号</param>
        /// <returns>格式化后的ICCID</returns>
        public static string? Format(string? iccid)
        {
            if (!IsValidFormat(iccid))
            {
                return null;
            }

            string cleaned = iccid!.Trim();
            if (cleaned.Length == 19)
            {
                return $"{cleaned.Substring(0, 6)} {cleaned.Substring(6, 2)} {cleaned.Substring(8, 2)} {cleaned.Substring(10)}";
            }
            else if (cleaned.Length == 20)
            {
                return $"{cleaned.Substring(0, 6)} {cleaned.Substring(6, 2)} {cleaned.Substring(8, 3)} {cleaned.Substring(11)}";
            }

            return cleaned;
        }

        /// <summary>
        /// ICCID脱敏：898600****7890
        /// </summary>
        /// <param name="iccid">ICCID号</param>
        /// <returns>脱敏后的ICCID</returns>
        public static string? Mask(string? iccid)
        {
            if (!IsValidFormat(iccid))
            {
                return null;
            }

            string cleaned = iccid!.Trim();
            int length = cleaned.Length;

            // 保留前6位和后4位
            return cleaned.Substring(0, 6) + new string('*', length - 10) + cleaned.Substring(length - 4);
        }

        #endregion
    }

    /// <summary>
    /// ICCID结构信息
    /// </summary>
    public class ICCDInfo
    {
        /// <summary>
        /// 移动国家代码（MCC）
        /// </summary>
        public string? MCC { get; set; }

        /// <summary>
        /// 国家名称
        /// </summary>
        public string? Country { get; set; }

        /// <summary>
        /// 移动网络代码（MNC）
        /// </summary>
        public string? MNC { get; set; }

        /// <summary>
        /// 运营商名称
        /// </summary>
        public string? Carrier { get; set; }

        /// <summary>
        /// 省份代码
        /// </summary>
        public string? ProvinceCode { get; set; }

        /// <summary>
        /// 序列号
        /// </summary>
        public string? SerialNumber { get; set; }

        /// <summary>
        /// 校验位
        /// </summary>
        public int? CheckDigit { get; set; }
    }
}
