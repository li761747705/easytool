using System;
using System.Collections.Generic;

namespace EasyTool.BusinessCategory
{
    /// <summary>
    /// 统一社会信用代码工具类
    /// 提供信用代码验证和解析功能
    /// </summary>
    public static class SocialCreditCodeUtil
    {
        #region 常量与数据

        // 信用代码字符集（不包含I、O、Z、S、V）
        private const string CharSet = "0123456789ABCDEFGHJKLMNPQRTUWXY";

        // 校验码权重
        private static readonly int[] Weights = { 1, 3, 9, 27, 19, 26, 16, 17, 20, 29, 25, 13, 8, 24, 10, 30, 28 };

        // 登记管理部门代码映射
        private static readonly Dictionary<char, string> DepartmentMapping = new()
        {
            { '1', "机构编制" },
            { '5', "民政" },
            { '9', "工商" },
            { 'Y', "其他" }
        };

        // 机构类型映射（按登记管理部门）
        private static readonly Dictionary<char, Dictionary<char, string>> InstitutionTypeMapping = new()
        {
            ['1'] = new Dictionary<char, string>
            {
                { '1', "机关" },
                { '2', "事业单位" },
                { '3', "中央编办直接管理机构编制的群众团体" },
                { '9', "其他" }
            },
            ['5'] = new Dictionary<char, string>
            {
                { '1', "社会团体" },
                { '2', "民办非企业单位" },
                { '3', "基金会" },
                { '9', "其他" }
            },
            ['9'] = new Dictionary<char, string>
            {
                { '1', "企业" },
                { '2', "个体工商户" },
                { '3', "农民专业合作社" }
            },
            ['Y'] = new Dictionary<char, string>
            {
                { '1', "外国常驻新闻机构" },
                { '9', "其他" }
            }
        };

        // 行政区划代码（前6位）
        private static readonly Dictionary<string, string> ProvinceCodeMapping = new()
        {
            { "110000", "北京市" }, { "120000", "天津市" }, { "130000", "河北省" },
            { "140000", "山西省" }, { "150000", "内蒙古自治区" },
            { "210000", "辽宁省" }, { "220000", "吉林省" }, { "230000", "黑龙江省" },
            { "310000", "上海市" }, { "320000", "江苏省" }, { "330000", "浙江省" },
            { "340000", "安徽省" }, { "350000", "福建省" }, { "360000", "江西省" },
            { "370000", "山东省" },
            { "410000", "河南省" }, { "420000", "湖北省" }, { "430000", "湖南省" },
            { "440000", "广东省" }, { "450000", "广西壮族自治区" }, { "460000", "海南省" },
            { "500000", "重庆市" }, { "510000", "四川省" }, { "520000", "贵州省" },
            { "530000", "云南省" }, { "540000", "西藏自治区" },
            { "610000", "陕西省" }, { "620000", "甘肃省" }, { "630000", "青海省" },
            { "640000", "宁夏回族自治区" }, { "650000", "新疆维吾尔自治区" }
        };

        #endregion

        #region 验证方法

        /// <summary>
        /// 验证统一社会信用代码是否有效
        /// </summary>
        /// <param name="code">信用代码</param>
        /// <returns>是否有效</returns>
        public static bool IsValid(string? code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return false;

            code = code.ToUpper().Trim();

            // 长度必须为18位
            if (code.Length != 18)
                return false;

            // 检查字符是否有效
            foreach (var c in code)
            {
                if (!CharSet.Contains(c))
                    return false;
            }

            // 验证校验码
            return ValidateCheckCode(code);
        }

        /// <summary>
        /// 验证校验码
        /// </summary>
        private static bool ValidateCheckCode(string code)
        {
            var sum = 0;
            for (var i = 0; i < 17; i++)
            {
                var charValue = CharSet.IndexOf(code[i]);
                if (charValue < 0)
                    return false;
                sum += charValue * Weights[i];
            }

            var mod = 31 - (sum % 31);
            if (mod == 31)
                mod = 0;

            var checkChar = CharSet[mod];
            return checkChar == code[17];
        }

        /// <summary>
        /// 计算校验码
        /// </summary>
        /// <param name="codeWithoutCheck">不含校验码的17位代码</param>
        /// <returns>校验码字符</returns>
        public static char CalculateCheckCode(string? codeWithoutCheck)
        {
            if (string.IsNullOrWhiteSpace(codeWithoutCheck) || codeWithoutCheck.Length != 17)
                return '\0';

            codeWithoutCheck = codeWithoutCheck.ToUpper();

            var sum = 0;
            for (var i = 0; i < 17; i++)
            {
                var charValue = CharSet.IndexOf(codeWithoutCheck[i]);
                if (charValue < 0)
                    return '\0';
                sum += charValue * Weights[i];
            }

            var mod = 31 - (sum % 31);
            if (mod == 31)
                mod = 0;

            return CharSet[mod];
        }

        #endregion

        #region 解析方法

        /// <summary>
        /// 解析统一社会信用代码
        /// </summary>
        /// <param name="code">信用代码</param>
        /// <returns>解析结果</returns>
        public static CreditCodeInfo? Parse(string? code)
        {
            if (!IsValid(code))
                return null;

            code = code!.ToUpper();

            var info = new CreditCodeInfo
            {
                Code = code,
                DepartmentCode = code[0],
                InstitutionTypeCode = code[1],
                RegionCode = code.Substring(2, 6),
                OrganizationCode = code.Substring(8, 9),
                CheckCode = code[17]
            };

            // 获取登记管理部门
            if (DepartmentMapping.TryGetValue(code[0], out var dept))
            {
                info.Department = dept;
            }

            // 获取机构类型
            if (InstitutionTypeMapping.TryGetValue(code[0], out var types))
            {
                if (types.TryGetValue(code[1], out var instType))
                {
                    info.InstitutionType = instType;
                }
            }

            // 获取行政区划
            var regionPrefix = code.Substring(2, 2) + "0000";
            if (ProvinceCodeMapping.TryGetValue(regionPrefix, out var province))
            {
                info.Province = province;
            }

            return info;
        }

        /// <summary>
        /// 获取登记管理部门
        /// </summary>
        /// <param name="code">信用代码</param>
        /// <returns>登记管理部门名称</returns>
        public static string? GetDepartment(string? code)
        {
            if (string.IsNullOrWhiteSpace(code) || code.Length < 1)
                return null;

            return DepartmentMapping.TryGetValue(code[0], out var dept) ? dept : null;
        }

        /// <summary>
        /// 获取机构类型
        /// </summary>
        /// <param name="code">信用代码</param>
        /// <returns>机构类型名称</returns>
        public static string? GetInstitutionType(string? code)
        {
            if (string.IsNullOrWhiteSpace(code) || code.Length < 2)
                return null;

            if (InstitutionTypeMapping.TryGetValue(code[0], out var types))
            {
                return types.TryGetValue(code[1], out var instType) ? instType : null;
            }

            return null;
        }

        /// <summary>
        /// 获取行政区划代码
        /// </summary>
        /// <param name="code">信用代码</param>
        /// <returns>行政区划代码（6位）</returns>
        public static string? GetRegionCode(string? code)
        {
            if (string.IsNullOrWhiteSpace(code) || code.Length < 8)
                return null;

            return code.Substring(2, 6);
        }

        /// <summary>
        /// 获取省份
        /// </summary>
        /// <param name="code">信用代码</param>
        /// <returns>省份名称</returns>
        public static string? GetProvince(string? code)
        {
            if (string.IsNullOrWhiteSpace(code) || code.Length < 8)
                return null;

            var regionPrefix = code.Substring(2, 2) + "0000";
            return ProvinceCodeMapping.TryGetValue(regionPrefix, out var province) ? province : null;
        }

        /// <summary>
        /// 获取组织机构代码
        /// </summary>
        /// <param name="code">信用代码</param>
        /// <returns>组织机构代码（9位）</returns>
        public static string? GetOrganizationCode(string? code)
        {
            if (string.IsNullOrWhiteSpace(code) || code.Length < 17)
                return null;

            return code.Substring(8, 9);
        }

        #endregion

        #region 类型判断

        /// <summary>
        /// 判断是否为企业
        /// </summary>
        /// <param name="code">信用代码</param>
        /// <returns>是否为企业</returns>
        public static bool IsEnterprise(string? code)
        {
            if (string.IsNullOrWhiteSpace(code) || code.Length < 2)
                return false;

            return code[0] == '9' && code[1] == '1';
        }

        /// <summary>
        /// 判断是否为个体工商户
        /// </summary>
        /// <param name="code">信用代码</param>
        /// <returns>是否为个体工商户</returns>
        public static bool IsIndividual(string? code)
        {
            if (string.IsNullOrWhiteSpace(code) || code.Length < 2)
                return false;

            return code[0] == '9' && code[1] == '2';
        }

        /// <summary>
        /// 判断是否为农民专业合作社
        /// </summary>
        /// <param name="code">信用代码</param>
        /// <returns>是否为农民专业合作社</returns>
        public static bool IsCooperative(string? code)
        {
            if (string.IsNullOrWhiteSpace(code) || code.Length < 2)
                return false;

            return code[0] == '9' && code[1] == '3';
        }

        /// <summary>
        /// 判断是否为事业单位
        /// </summary>
        /// <param name="code">信用代码</param>
        /// <returns>是否为事业单位</returns>
        public static bool IsPublicInstitution(string? code)
        {
            if (string.IsNullOrWhiteSpace(code) || code.Length < 2)
                return false;

            return code[0] == '1' && code[1] == '2';
        }

        /// <summary>
        /// 判断是否为社会团体
        /// </summary>
        /// <param name="code">信用代码</param>
        /// <returns>是否为社会团体</returns>
        public static bool IsSocialOrganization(string? code)
        {
            if (string.IsNullOrWhiteSpace(code) || code.Length < 2)
                return false;

            return code[0] == '5' && code[1] == '1';
        }

        /// <summary>
        /// 判断是否为民办非企业单位
        /// </summary>
        /// <param name="code">信用代码</param>
        /// <returns>是否为民办非企业单位</returns>
        public static bool IsPrivateNonEnterprise(string? code)
        {
            if (string.IsNullOrWhiteSpace(code) || code.Length < 2)
                return false;

            return code[0] == '5' && code[1] == '2';
        }

        /// <summary>
        /// 判断是否为基金会
        /// </summary>
        /// <param name="code">信用代码</param>
        /// <returns>是否为基金会</returns>
        public static bool IsFoundation(string? code)
        {
            if (string.IsNullOrWhiteSpace(code) || code.Length < 2)
                return false;

            return code[0] == '5' && code[1] == '3';
        }

        /// <summary>
        /// 判断是否为政府机关
        /// </summary>
        /// <param name="code">信用代码</param>
        /// <returns>是否为政府机关</returns>
        public static bool IsGovernmentAgency(string? code)
        {
            if (string.IsNullOrWhiteSpace(code) || code.Length < 2)
                return false;

            return code[0] == '1' && code[1] == '1';
        }

        #endregion
    }

    /// <summary>
    /// 统一社会信用代码解析结果
    /// </summary>
    public class CreditCodeInfo
    {
        /// <summary>
        /// 完整信用代码
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// 登记管理部门代码
        /// </summary>
        public char DepartmentCode { get; set; }

        /// <summary>
        /// 登记管理部门名称
        /// </summary>
        public string Department { get; set; } = string.Empty;

        /// <summary>
        /// 机构类型代码
        /// </summary>
        public char InstitutionTypeCode { get; set; }

        /// <summary>
        /// 机构类型名称
        /// </summary>
        public string InstitutionType { get; set; } = string.Empty;

        /// <summary>
        /// 行政区划代码（6位）
        /// </summary>
        public string RegionCode { get; set; } = string.Empty;

        /// <summary>
        /// 省份名称
        /// </summary>
        public string Province { get; set; } = string.Empty;

        /// <summary>
        /// 组织机构代码（9位）
        /// </summary>
        public string OrganizationCode { get; set; } = string.Empty;

        /// <summary>
        /// 校验码
        /// </summary>
        public char CheckCode { get; set; }

        /// <summary>
        /// 返回信用代码字符串
        /// </summary>
        public override string ToString() => Code;
    }
}