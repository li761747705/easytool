using System;
using System.Text.RegularExpressions;

namespace EasyTool.BusinessCategory
{
    /// <summary>
    /// IMEI（国际移动设备识别号）工具类
    /// </summary>
    public static class IMEIUtil
    {
        #region 常量与私有字段

        /// <summary>
        /// IMEI正则表达式（15位数字）
        /// </summary>
        private static readonly Regex IMEIRegex = new(@"^\d{15}$", RegexOptions.Compiled);

        /// <summary>
        /// IMEI SV正则表达式（16位数字，含软件版本）
        /// </summary>
        private static readonly Regex IMEISvRegex = new(@"^\d{16}$", RegexOptions.Compiled);

        /// <summary>
        /// TAC（类型分配码）与制造商映射（部分）
        /// </summary>
        private static readonly (string Prefix, string Manufacturer)[] TacPrefixMap =
        {
            ("01", "Apple"),
            ("35", "Samsung"),
            ("86", "Samsung"),
            ("01", "Nokia"),
            ("35", "Nokia"),
            ("352", "Sony"),
            ("353", "Sony"),
            ("354", "Sony"),
            ("355", "Sony"),
            ("356", "Sony"),
            ("358", "Huawei"),
            ("359", "Huawei"),
            ("861", "Xiaomi"),
            ("862", "Xiaomi"),
            ("865", "Xiaomi"),
            ("866", "Xiaomi"),
            ("352", "LG"),
            ("353", "LG"),
            ("355", "LG"),
            ("356", "LG"),
            ("353", "HTC"),
            ("354", "HTC"),
            ("355", "HTC"),
            ("357", "HTC"),
            ("358", "HTC"),
            ("359", "HTC"),
            ("010", "Apple"),
            ("011", "Apple"),
            ("012", "Apple"),
            ("013", "Apple"),
            ("014", "Apple"),
            ("015", "Apple"),
            ("016", "Apple"),
            ("017", "Apple"),
            ("018", "Apple"),
            ("019", "Apple")
        };

        #endregion

        #region 验证方法

        /// <summary>
        /// 验证IMEI是否有效（15位，含Luhn校验）
        /// </summary>
        /// <param name="imei">IMEI号</param>
        /// <returns>是否有效</returns>
        public static bool IsValid(string? imei)
        {
            if (!IsValidFormat(imei))
            {
                return false;
            }

            return ValidateLuhn(imei!);
        }

        /// <summary>
        /// 仅验证IMEI格式（不校验Luhn）
        /// </summary>
        /// <param name="imei">IMEI号</param>
        /// <returns>格式是否正确</returns>
        public static bool IsValidFormat(string? imei)
        {
            if (string.IsNullOrWhiteSpace(imei))
            {
                return false;
            }

            return IMEIRegex.IsMatch(imei);
        }

        /// <summary>
        /// 验证IMEI SV是否有效（16位）
        /// </summary>
        /// <param name="imeiSv">IMEI SV号</param>
        /// <returns>是否有效</returns>
        public static bool IsValidSv(string? imeiSv)
        {
            if (string.IsNullOrWhiteSpace(imeiSv))
            {
                return false;
            }

            return IMEISvRegex.IsMatch(imeiSv);
        }

        /// <summary>
        /// 使用Luhn算法验证IMEI
        /// </summary>
        /// <param name="imei">IMEI号</param>
        /// <returns>是否通过Luhn校验</returns>
        public static bool ValidateLuhn(string? imei)
        {
            if (string.IsNullOrWhiteSpace(imei) || imei.Length != 15)
            {
                return false;
            }

            int sum = 0;
            for (int i = 0; i < 15; i++)
            {
                if (!char.IsDigit(imei[i]))
                {
                    return false;
                }

                int digit = imei[i] - '0';

                // 偶数位置（从0开始）乘以2，奇数位置不变
                // IMEI的Luhn算法：从右向左，偶数位×2
                if (i % 2 == 1)
                {
                    digit *= 2;
                    if (digit > 9)
                    {
                        digit -= 9;
                    }
                }

                sum += digit;
            }

            return sum % 10 == 0;
        }

        /// <summary>
        /// 计算Luhn校验位
        /// </summary>
        /// <param name="imei14">不含校验位的14位IMEI</param>
        /// <returns>校验位（0-9），计算失败返回-1</returns>
        public static int CalculateCheckDigit(string? imei14)
        {
            if (string.IsNullOrWhiteSpace(imei14) || imei14.Length != 14)
            {
                return -1;
            }

            int sum = 0;
            for (int i = 0; i < 14; i++)
            {
                if (!char.IsDigit(imei14[i]))
                {
                    return -1;
                }

                int digit = imei14[i] - '0';

                if (i % 2 == 1)
                {
                    digit *= 2;
                    if (digit > 9)
                    {
                        digit -= 9;
                    }
                }

                sum += digit;
            }

            return (10 - (sum % 10)) % 10;
        }

        #endregion

        #region 信息提取

        /// <summary>
        /// 获取TAC（类型分配码，前8位）
        /// </summary>
        /// <param name="imei">IMEI号</param>
        /// <returns>TAC码</returns>
        public static string? GetTAC(string? imei)
        {
            if (!IsValidFormat(imei))
            {
                return null;
            }

            return imei!.Substring(0, 8);
        }

        /// <summary>
        /// 获取制造商（根据TAC前缀推测）
        /// </summary>
        /// <param name="imei">IMEI号</param>
        /// <returns>制造商名称</returns>
        public static string? GetManufacturer(string? imei)
        {
            if (!IsValidFormat(imei))
            {
                return null;
            }

            string tac = imei!.Substring(0, 8);

            // 查找最长匹配的前缀
            for (int len = Math.Min(3, tac.Length); len >= 1; len--)
            {
                string prefix = tac.Substring(0, len);
                foreach (var mapping in TacPrefixMap)
                {
                    if (mapping.Prefix == prefix)
                    {
                        return mapping.Manufacturer;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 获取序列号（SNR，第9-14位）
        /// </summary>
        /// <param name="imei">IMEI号</param>
        /// <returns>序列号</returns>
        public static string? GetSerialNumber(string? imei)
        {
            if (!IsValidFormat(imei))
            {
                return null;
            }

            return imei!.Substring(8, 6);
        }

        /// <summary>
        /// 获取校验位（第15位）
        /// </summary>
        /// <param name="imei">IMEI号</param>
        /// <returns>校验位</returns>
        public static int? GetCheckDigit(string? imei)
        {
            if (!IsValidFormat(imei))
            {
                return null;
            }

            return imei![14] - '0';
        }

        #endregion

        #region 格式化方法

        /// <summary>
        /// 格式化IMEI（AA-BBBBBB-CCCCCC-D）
        /// </summary>
        /// <param name="imei">IMEI号</param>
        /// <returns>格式化后的IMEI</returns>
        public static string? Format(string? imei)
        {
            string? normalized = Normalize(imei);
            if (normalized == null || normalized.Length != 15)
            {
                return null;
            }

            return $"{normalized.Substring(0, 2)}-{normalized.Substring(2, 6)}-{normalized.Substring(8, 6)}-{normalized[14]}";
        }

        /// <summary>
        /// 格式化IMEI（去除分隔符）
        /// </summary>
        /// <param name="imei">IMEI号</param>
        /// <returns>清理后的IMEI</returns>
        public static string? Normalize(string? imei)
        {
            if (string.IsNullOrWhiteSpace(imei))
            {
                return null;
            }

            string cleaned = Regex.Replace(imei, @"[^\d]", "");
            return cleaned.Length == 15 ? cleaned : null;
        }

        /// <summary>
        /// IMEI脱敏：35****6
        /// </summary>
        /// <param name="imei">IMEI号</param>
        /// <returns>脱敏后的IMEI</returns>
        public static string? Mask(string? imei)
        {
            string? normalized = Normalize(imei);
            if (normalized == null)
            {
                return null;
            }

            return normalized.Substring(0, 2) + "***********" + normalized[14];
        }

        #endregion

        #region 生成方法

        /// <summary>
        /// 生成随机IMEI（仅供测试使用）
        /// </summary>
        /// <param name="tac">TAC码（可选，默认随机）</param>
        /// <returns>15位IMEI</returns>
        public static string GenerateRandom(string? tac = null)
        {
            // TAC（8位）
            string tacCode = tac ?? MathCategory.RandomUtil.RandomDigitString(8);

            // 序列号（6位）
            string serial = MathCategory.RandomUtil.RandomDigitString(6);

            // 计算校验位
            int checkDigit = CalculateCheckDigit(tacCode + serial);

            return tacCode + serial + checkDigit;
        }

        #endregion
    }
}
