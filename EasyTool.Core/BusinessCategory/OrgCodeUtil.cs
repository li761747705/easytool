using System;
using System.Text.RegularExpressions;

namespace EasyTool.BusinessCategory
{
    /// <summary>
    /// 组织机构代码工具类
    /// </summary>
    public static class OrgCodeUtil
    {
        #region 常量与私有字段

        /// <summary>
        /// 组织机构代码正则表达式（9位：8位数字/字母 + 1位校验码）
        /// </summary>
        private static readonly Regex OrgCodeRegex = new(
            @"^[A-Z0-9]{8}-?[A-X0-9]$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// 组织机构代码字符值映射
        /// </summary>
        private static readonly int[] CharWeights = { 3, 7, 9, 10, 5, 8, 4, 2 };

        /// <summary>
        /// 校验码对照表
        /// </summary>
        private const string CheckCodes = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        #endregion

        #region 验证方法

        /// <summary>
        /// 验证组织机构代码是否有效
        /// </summary>
        /// <param name="orgCode">组织机构代码</param>
        /// <returns>是否有效</returns>
        public static bool IsValid(string? orgCode)
        {
            if (string.IsNullOrWhiteSpace(orgCode))
            {
                return false;
            }

            string code = orgCode.ToUpper().Replace("-", "");

            if (code.Length != 9)
            {
                return false;
            }

            if (!OrgCodeRegex.IsMatch(code))
            {
                return false;
            }

            // 计算校验码
            char? expectedCheck = CalculateCheckCode(code.Substring(0, 8));
            return expectedCheck.HasValue && expectedCheck.Value == code[8];
        }

        /// <summary>
        /// 验证格式是否正确（不校验校验位）
        /// </summary>
        /// <param name="orgCode">组织机构代码</param>
        /// <returns>格式是否正确</returns>
        public static bool IsValidFormat(string? orgCode)
        {
            if (string.IsNullOrWhiteSpace(orgCode))
            {
                return false;
            }

            string code = orgCode.ToUpper().Replace("-", "");
            return code.Length == 9 && OrgCodeRegex.IsMatch(code);
        }

        /// <summary>
        /// 计算校验码
        /// </summary>
        /// <param name="code8">不含校验位的8位代码</param>
        /// <returns>校验码，计算失败返回null</returns>
        public static char? CalculateCheckCode(string? code8)
        {
            if (string.IsNullOrWhiteSpace(code8) || code8.Length != 8)
            {
                return null;
            }

            int sum = 0;
            for (int i = 0; i < 8; i++)
            {
                char c = char.ToUpper(code8[i]);
                int value;

                if (c >= '0' && c <= '9')
                {
                    value = c - '0';
                }
                else if (c >= 'A' && c <= 'Z')
                {
                    value = c - 'A' + 10;
                }
                else
                {
                    return null;
                }

                sum += value * CharWeights[i];
            }

            int checkIndex = 11 - (sum % 11);
            if (checkIndex == 11)
            {
                checkIndex = 0;
            }
            else if (checkIndex == 10)
            {
                return 'X'; // 10对应X
            }

            return CheckCodes[checkIndex];
        }

        #endregion

        #region 信息提取

        /// <summary>
        /// 获取机构类型（第1位）
        /// </summary>
        /// <param name="orgCode">组织机构代码</param>
        /// <returns>机构类型</returns>
        public static string? GetOrganizationType(string? orgCode)
        {
            if (!IsValid(orgCode))
            {
                return null;
            }

            char typeCode = char.ToUpper(orgCode!.Replace("-", "")[0]);
            return typeCode switch
            {
                '1' => "企业法人",
                '2' => "企业非法人",
                '3' => "事业法人",
                '4' => "事业非法人",
                '5' => "机关法人",
                '6' => "机关非法人",
                '7' => "社会团体法人",
                '8' => "社会团体非法人",
                '9' => "其他机构",
                'A' => "企业法人（外资）",
                'B' => "企业非法人（外资）",
                _ => null
            };
        }

        /// <summary>
        /// 获取登记管理机关行政区划代码（第2-8位）
        /// </summary>
        /// <param name="orgCode">组织机构代码</param>
        /// <returns>行政区划代码</returns>
        public static string? GetAreaCode(string? orgCode)
        {
            if (!IsValid(orgCode))
            {
                return null;
            }

            string code = orgCode!.Replace("-", "");
            return code.Substring(1, 7);
        }

        #endregion

        #region 格式化方法

        /// <summary>
        /// 格式化组织机构代码（XXXXXXXX-X）
        /// </summary>
        /// <param name="orgCode">组织机构代码</param>
        /// <returns>格式化后的代码</returns>
        public static string? Format(string? orgCode)
        {
            if (!IsValid(orgCode))
            {
                return null;
            }

            string code = orgCode!.ToUpper().Replace("-", "");
            return code.Substring(0, 8) + "-" + code[8];
        }

        /// <summary>
        /// 清理组织机构代码（去除分隔符）
        /// </summary>
        /// <param name="orgCode">组织机构代码</param>
        /// <returns>清理后的代码</returns>
        public static string? Normalize(string? orgCode)
        {
            if (string.IsNullOrWhiteSpace(orgCode))
            {
                return null;
            }

            string code = orgCode.ToUpper().Replace("-", "").Trim();
            return code.Length == 9 && OrgCodeRegex.IsMatch(code) ? code : null;
        }

        /// <summary>
        /// 组织机构代码脱敏：123****9X
        /// </summary>
        /// <param name="orgCode">组织机构代码</param>
        /// <returns>脱敏后的代码</returns>
        public static string? Mask(string? orgCode)
        {
            if (!IsValid(orgCode))
            {
                return null;
            }

            string code = orgCode!.Replace("-", "");
            return code.Substring(0, 3) + "*****" + code.Substring(8);
        }

        #endregion

        #region 生成方法

        /// <summary>
        /// 生成随机组织机构代码（仅供测试使用）
        /// </summary>
        /// <returns>9位组织机构代码</returns>
        public static string GenerateRandom()
        {
            const string chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            // 生成前8位
            string code8 = "";
            for (int i = 0; i < 8; i++)
            {
                code8 += MathCategory.RandomUtil.GetRandomElement(chars.ToCharArray());
            }

            // 计算校验码
            char? checkCode = CalculateCheckCode(code8);
            return code8 + (checkCode ?? '0');
        }

        #endregion
    }
}
