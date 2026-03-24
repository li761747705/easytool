using System;
using System.Text.RegularExpressions;

namespace EasyTool.BusinessCategory
{
    /// <summary>
    /// 统一社会信用代码工具类
    /// 用于验证和处理中国大陆企业的统一社会信用代码
    /// </summary>
    public static class CreditCodeUtil
    {
        /// <summary>
        /// 统一社会信用代码长度
        /// </summary>
        private const int CreditCodeLength = 18;

        /// <summary>
        /// 统一社会信用代码字符集（不包含I、O、Z、S、V）
        /// </summary>
        private const string CreditCodeChars = "0123456789ABCDEFGHJKLMNPQRTUWXY";

        /// <summary>
        /// 校验码权重
        /// </summary>
        private static readonly int[] Weights = { 1, 3, 9, 27, 19, 26, 16, 17, 20, 29, 25, 13, 8, 24, 10, 30, 28 };

        /// <summary>
        /// 验证统一社会信用代码是否有效
        /// </summary>
        /// <param name="creditCode">统一社会信用代码</param>
        /// <returns>是否有效</returns>
        public static bool IsValid(string? creditCode)
        {
            if (string.IsNullOrWhiteSpace(creditCode))
                return false;

            creditCode = creditCode.Trim().ToUpperInvariant();

            // 检查长度
            if (creditCode.Length != CreditCodeLength)
                return false;

            // 检查字符是否合法
            foreach (var c in creditCode)
            {
                if (!CreditCodeChars.Contains(c))
                    return false;
            }

            // 验证校验码
            return ValidateCheckCode(creditCode);
        }

        /// <summary>
        /// 获取统一社会信用代码的类型信息
        /// </summary>
        /// <param name="creditCode">统一社会信用代码</param>
        /// <returns>类型信息</returns>
        public static CreditCodeType? GetType(string? creditCode)
        {
            if (string.IsNullOrWhiteSpace(creditCode))
                return null;

            creditCode = creditCode.Trim().ToUpperInvariant();

            if (creditCode.Length != CreditCodeLength)
                return null;

            var typeCode = creditCode[0];
            return typeCode switch
            {
                '1' => CreditCodeType.Institution,
                '5' => CreditCodeType.Enterprise,
                '9' => CreditCodeType.Other,
                'Y' => CreditCodeType.IndividualBusiness,
                _ => null
            };
        }

        /// <summary>
        /// 获取登记管理部门
        /// </summary>
        /// <param name="creditCode">统一社会信用代码</param>
        /// <returns>登记管理部门</returns>
        public static string? GetRegistrationAuthority(string? creditCode)
        {
            if (string.IsNullOrWhiteSpace(creditCode))
                return null;

            creditCode = creditCode.Trim().ToUpperInvariant();

            if (creditCode.Length < 2)
                return null;

            var code = creditCode.Substring(0, 2);
            return code switch
            {
                "11" => "工商行政管理",
                "12" => "工商行政管理（个体工商户）",
                "13" => "工商行政管理（农民专业合作社）",
                "19" => "工商行政管理（其他）",
                "21" => "机构编制",
                "31" => "外交",
                "32" => "文化",
                "33" => "教育",
                "34" => "卫生",
                "35" => "体育",
                "36" => "新闻出版",
                "37" => "宗教事务",
                "41" => "司法行政（律师）",
                "42" => "司法行政（公证）",
                "43" => "司法行政（基层法律服务）",
                "44" => "司法行政（司法鉴定）",
                "51" => "民政",
                "52" => "民政（社会组织）",
                "53" => "民政（基金会）",
                "54" => "民政（民办非企业单位）",
                "61" => "旅游",
                "62" => "文物",
                "71" => "工会",
                "81" => "公安",
                "91" => "其他",
                "A1" => "全国人大",
                "A2" => "全国政协",
                "A3" => "人民法院",
                "A4" => "人民检察院",
                "A9" => "其他",
                "N1" => "军事",
                "N2" => "武警",
                _ => "未知"
            };
        }

        /// <summary>
        /// 生成校验码
        /// </summary>
        /// <param name="creditCode17">前17位代码</param>
        /// <returns>校验码</returns>
        public static char GenerateCheckCode(string creditCode17)
        {
            if (string.IsNullOrEmpty(creditCode17) || creditCode17.Length != 17)
                throw new ArgumentException("输入必须为17位");

            int sum = 0;
            for (int i = 0; i < 17; i++)
            {
                var value = CreditCodeChars.IndexOf(char.ToUpperInvariant(creditCode17[i]));
                if (value < 0)
                    throw new ArgumentException($"第{i + 1}位字符无效");

                sum += value * Weights[i];
            }

            var mod = 31 - sum % 31;
            return mod == 31 ? '0' : CreditCodeChars[mod];
        }

        private static bool ValidateCheckCode(string creditCode)
        {
            var expectedCheckCode = GenerateCheckCode(creditCode.Substring(0, 17));
            return creditCode[17] == expectedCheckCode;
        }

        /// <summary>
        /// 格式化统一社会信用代码（添加分隔符）
        /// </summary>
        /// <param name="creditCode">统一社会信用代码</param>
        /// <param name="separator">分隔符</param>
        /// <returns>格式化后的代码</returns>
        public static string Format(string? creditCode, string separator = "-")
        {
            if (string.IsNullOrWhiteSpace(creditCode))
                return string.Empty;

            creditCode = creditCode.Trim().ToUpperInvariant();

            if (creditCode.Length != CreditCodeLength)
                return creditCode;

            // 格式：XXXXXX-XXXX-XXXX-XXXX
            return $"{creditCode.Substring(0, 6)}{separator}{creditCode.Substring(6, 4)}{separator}{creditCode.Substring(10, 4)}{separator}{creditCode.Substring(14, 4)}";
        }
    }

    /// <summary>
    /// 统一社会信用代码类型
    /// </summary>
    public enum CreditCodeType
    {
        /// <summary>
        /// 机构
        /// </summary>
        Institution,

        /// <summary>
        /// 企业
        /// </summary>
        Enterprise,

        /// <summary>
        /// 其他
        /// </summary>
        Other,

        /// <summary>
        /// 个体工商户
        /// </summary>
        IndividualBusiness
    }
}
