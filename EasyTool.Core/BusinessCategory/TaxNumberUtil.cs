using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace EasyTool.BusinessCategory
{
    /// <summary>
    /// 税号类型枚举
    /// </summary>
    public enum TaxNumberType
    {
        /// <summary>
        /// 未知类型
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// 统一社会信用代码（18位）
        /// </summary>
        CreditCode = 1,

        /// <summary>
        /// 旧税号（15位）
        /// </summary>
        OldTaxCode = 2,

        /// <summary>
        /// 税务登记号（20位）
        /// </summary>
        TaxRegistration = 3
    }

    /// <summary>
    /// 企业税号工具类
    /// </summary>
    public static class TaxNumberUtil
    {
        #region 常量与私有字段

        /// <summary>
        /// 统一社会信用代码字符集（31个字符）
        /// </summary>
        private const string BaseCode = "0123456789ABCDEFGHJKLMNPQRTUWXY";

        /// <summary>
        /// 统一社会信用代码权重
        /// </summary>
        private static readonly int[] Weights = { 1, 3, 9, 27, 19, 26, 16, 17, 20, 29, 25, 13, 8, 24, 10, 30, 28 };

        /// <summary>
        /// 18位统一社会信用代码正则表达式
        /// </summary>
        private static readonly Regex CreditCodeRegex = new Regex(
            @"^[0-9A-HJ-NPQRTUWXY]{2}[0-9]{6}[0-9A-HJ-NPQRTUWXY]{10}$",
            RegexOptions.Compiled);

        /// <summary>
        /// 15位旧税号正则表达式（6位区域码 + 9位组织机构代码）
        /// </summary>
        private static readonly Regex OldTaxCodeRegex = new Regex(
            @"^[0-9]{15}$",
            RegexOptions.Compiled);

        /// <summary>
        /// 20位税务登记号正则表达式
        /// </summary>
        private static readonly Regex TaxRegistrationRegex = new Regex(
            @"^[0-9]{20}$",
            RegexOptions.Compiled);

        /// <summary>
        /// 登记管理部门代码映射
        /// </summary>
        private static readonly Dictionary<string, string> DepartmentMap = new Dictionary<string, string>
        {
            { "11", "机构编制" }, { "12", "外交" }, { "13", "教育" }, { "14", "公安" },
            { "15", "民政" }, { "16", "司法" }, { "17", "交通运输" }, { "18", "文化和旅游" },
            { "19", "市场监管" }, { "21", "农业" }, { "22", "林业和草原" }, { "23", "卫生健康" },
            { "24", "中医药" }, { "25", "退役军人" }, { "26", "应急管理" }, { "27", "国有资产" },
            { "28", "海关" }, { "29", "税务" }, { "31", "人民银行" }, { "32", "外汇" },
            { "33", "知识产权" }, { "34", "粮食和储备" }, { "35", "能源" }, { "36", "国防科工" },
            { "37", "烟草" }, { "41", "中央军委" }, { "51", "全国总工会" }, { "52", "全国妇联" },
            { "53", "全国工商联" }, { "54", "全国青联" }, { "55", "中国残联" },
            { "91", "工商" }, { "92", "中央及地方编办" }, { "93", "民政" }, { "99", "其他" }
        };

        /// <summary>
        /// 机构类型代码映射（与登记管理部门组合使用）
        /// </summary>
        private static readonly Dictionary<char, string> OrganizationTypeMap = new Dictionary<char, string>
        {
            { '1', "企业" }, { '2', "个体工商户" }, { '3', "农民专业合作社" },
            { '4', "机关" }, { '5', "事业单位" }, { '6', "社会团体" },
            { '7', "民办非企业单位" }, { '8', "基金会" }, { '9', "其他" }
        };

        /// <summary>
        /// 行业代码映射（GB/T 4754-2017 国民经济行业分类，部分常用）
        /// </summary>
        private static readonly Dictionary<string, string> IndustryCodeMap = new Dictionary<string, string>
        {
            { "01", "农业" }, { "02", "林业" }, { "03", "畜牧业" }, { "04", "渔业" },
            { "06", "煤炭开采和洗选业" }, { "07", "石油和天然气开采业" },
            { "08", "黑色金属矿采选业" }, { "09", "有色金属矿采选业" },
            { "10", "非金属矿采选业" }, { "13", "农副食品加工业" },
            { "14", "食品制造业" }, { "15", "酒、饮料和精制茶制造业" },
            { "17", "纺织业" }, { "18", "纺织服装、服饰业" },
            { "19", "皮革、毛皮、羽毛及其制品和制鞋业" }, { "20", "木材加工和木、竹、藤、棕、草制品业" },
            { "21", "家具制造业" }, { "22", "造纸和纸制品业" },
            { "23", "印刷和记录媒介复制业" }, { "24", "文教、工美、体育和娱乐用品制造业" },
            { "25", "石油、煤炭及其他燃料加工业" }, { "26", "化学原料和化学制品制造业" },
            { "27", "医药制造业" }, { "28", "化学纤维制造业" },
            { "29", "橡胶和塑料制品业" }, { "30", "非金属矿物制品业" },
            { "31", "黑色金属冶炼和压延加工业" }, { "32", "有色金属冶炼和压延加工业" },
            { "33", "金属制品业" }, { "34", "通用设备制造业" },
            { "35", "专用设备制造业" }, { "36", "汽车制造业" },
            { "37", "铁路、船舶、航空航天和其他运输设备制造业" },
            { "38", "电气机械和器材制造业" }, { "39", "计算机、通信和其他电子设备制造业" },
            { "40", "仪器仪表制造业" }, { "41", "其他制造业" },
            { "42", "废弃资源综合利用业" }, { "43", "金属制品、机械和设备修理业" },
            { "44", "电力、热力生产和供应业" }, { "45", "燃气生产和供应业" },
            { "46", "水的生产和供应业" }, { "47", "房屋建筑业" },
            { "48", "土木工程建筑业" }, { "49", "建筑安装业" },
            { "50", "建筑装饰、装修和其他建筑业" }, { "51", "批发业" },
            { "52", "零售业" }, { "53", "铁路运输业" },
            { "54", "道路运输业" }, { "55", "水上运输业" },
            { "56", "航空运输业" }, { "57", "管道运输业" },
            { "58", "多式联运和运输代理业" }, { "59", "装卸搬运和仓储业" },
            { "60", "邮政业" }, { "61", "住宿业" },
            { "62", "餐饮业" }, { "63", "电信、广播电视和卫星传输服务" },
            { "64", "互联网和相关服务" }, { "65", "软件和信息技术服务业" },
            { "66", "货币金融服务" }, { "67", "资本市场服务" },
            { "68", "保险业" }, { "69", "其他金融业" },
            { "70", "房地产业" }, { "71", "租赁业" },
            { "72", "商务服务业" }, { "73", "研究和试验发展" },
            { "74", "专业技术服务业" }, { "75", "科技推广和应用服务业" },
            { "76", "水利管理业" }, { "77", "生态保护和环境治理业" },
            { "78", "公共设施管理业" }, { "79", "居民服务业" },
            { "80", "机动车、电子产品和日用产品修理业" }, { "81", "其他服务业" },
            { "82", "教育" }, { "83", "卫生" },
            { "84", "社会工作" }, { "85", "新闻和出版业" },
            { "86", "广播、电视、电影和录音制作业" }, { "87", "文化艺术业" },
            { "88", "体育" }, { "89", "娱乐业" },
            { "90", "中国共产党机关" }, { "91", "国家机构" },
            { "92", "人民政协、民主党派" }, { "93", "社会保障" },
            { "94", "群众团体、社会团体和其他成员组织" }, { "95", "基层群众自治组织" },
            { "96", "国际组织" }
        };

        #endregion

        #region 验证方法

        /// <summary>
        /// 验证税号是否有效（支持15/18/20位）
        /// </summary>
        /// <param name="taxNumber">税号</param>
        /// <returns>是否有效</returns>
        public static bool IsValid(string? taxNumber)
        {
            if (string.IsNullOrWhiteSpace(taxNumber))
            {
                return false;
            }

            return IsValid18(taxNumber) || IsValid15(taxNumber) || IsValid20(taxNumber);
        }

        /// <summary>
        /// 验证18位统一社会信用代码是否有效
        /// </summary>
        /// <param name="taxNumber">税号</param>
        /// <returns>是否有效</returns>
        public static bool IsValid18(string? taxNumber)
        {
            if (string.IsNullOrWhiteSpace(taxNumber) || taxNumber.Length != 18)
            {
                return false;
            }

            string normalized = taxNumber.ToUpper();

            // 验证格式
            if (!CreditCodeRegex.IsMatch(normalized))
            {
                return false;
            }

            // 验证校验码
            char? expectedCheckCode = CalculateCheckCode(normalized.Substring(0, 17));
            return expectedCheckCode.HasValue && expectedCheckCode.Value == normalized[17];
        }

        /// <summary>
        /// 验证15位旧税号是否有效
        /// </summary>
        /// <param name="taxNumber">税号</param>
        /// <returns>是否有效</returns>
        public static bool IsValid15(string? taxNumber)
        {
            if (string.IsNullOrWhiteSpace(taxNumber) || taxNumber.Length != 15)
            {
                return false;
            }

            return OldTaxCodeRegex.IsMatch(taxNumber);
        }

        /// <summary>
        /// 验证20位税务登记号是否有效
        /// </summary>
        /// <param name="taxNumber">税号</param>
        /// <returns>是否有效</returns>
        public static bool IsValid20(string? taxNumber)
        {
            if (string.IsNullOrWhiteSpace(taxNumber) || taxNumber.Length != 20)
            {
                return false;
            }

            return TaxRegistrationRegex.IsMatch(taxNumber);
        }

        /// <summary>
        /// 判断是否为统一社会信用代码（18位）
        /// </summary>
        /// <param name="taxNumber">税号</param>
        /// <returns>是否为统一社会信用代码</returns>
        public static bool IsCreditCode(string? taxNumber)
        {
            return IsValid18(taxNumber);
        }

        /// <summary>
        /// 计算统一社会信用代码校验码
        /// </summary>
        /// <param name="codeWithoutCheck">不含校验码的17位代码</param>
        /// <returns>校验码，计算失败返回null</returns>
        public static char? CalculateCheckCode(string? codeWithoutCheck)
        {
            if (string.IsNullOrWhiteSpace(codeWithoutCheck) || codeWithoutCheck.Length != 17)
            {
                return null;
            }

            int sum = 0;
            for (int i = 0; i < 17; i++)
            {
                int value = BaseCode.IndexOf(char.ToUpper(codeWithoutCheck[i]));
                if (value < 0)
                {
                    return null;
                }
                sum += value * Weights[i];
            }

            int checkValue = 31 - (sum % 31);
            if (checkValue == 31)
            {
                checkValue = 0;
            }

            return BaseCode[checkValue];
        }

        #endregion

        #region 类型识别

        /// <summary>
        /// 获取税号类型
        /// </summary>
        /// <param name="taxNumber">税号</param>
        /// <returns>税号类型</returns>
        public static TaxNumberType GetTaxNumberType(string? taxNumber)
        {
            if (string.IsNullOrWhiteSpace(taxNumber))
            {
                return TaxNumberType.Unknown;
            }

            if (IsValid18(taxNumber))
            {
                return TaxNumberType.CreditCode;
            }

            if (IsValid15(taxNumber))
            {
                return TaxNumberType.OldTaxCode;
            }

            if (IsValid20(taxNumber))
            {
                return TaxNumberType.TaxRegistration;
            }

            return TaxNumberType.Unknown;
        }

        #endregion

        #region 信息提取

        /// <summary>
        /// 获取登记管理部门（仅18位统一社会信用代码）
        /// </summary>
        /// <param name="taxNumber">税号</param>
        /// <returns>登记管理部门名称</returns>
        public static string? GetDepartment(string? taxNumber)
        {
            if (!IsValid18(taxNumber))
            {
                return null;
            }

            string normalized = taxNumber!.ToUpper();
            string deptCode = normalized.Substring(0, 2);

            return DepartmentMap.TryGetValue(deptCode, out string? dept) ? dept : null;
        }

        /// <summary>
        /// 获取机构类型（仅18位统一社会信用代码）
        /// </summary>
        /// <param name="taxNumber">税号</param>
        /// <returns>机构类型名称</returns>
        public static string? GetOrganizationType(string? taxNumber)
        {
            if (!IsValid18(taxNumber))
            {
                return null;
            }

            string normalized = taxNumber!.ToUpper();
            char typeCode = normalized[2];

            return OrganizationTypeMap.TryGetValue(typeCode, out string? type) ? type : null;
        }

        /// <summary>
        /// 获取行政区划代码（仅18位统一社会信用代码）
        /// </summary>
        /// <param name="taxNumber">税号</param>
        /// <returns>行政区划代码</returns>
        public static string? GetAreaCode(string? taxNumber)
        {
            if (!IsValid18(taxNumber))
            {
                return null;
            }

            return taxNumber!.Substring(3, 6);
        }

        /// <summary>
        /// 获取行业代码（仅18位统一社会信用代码）
        /// </summary>
        /// <param name="taxNumber">税号</param>
        /// <returns>行业代码</returns>
        public static string? GetIndustryCode(string? taxNumber)
        {
            if (!IsValid18(taxNumber))
            {
                return null;
            }

            return taxNumber!.Substring(9, 2);
        }

        /// <summary>
        /// 获取行业名称（仅18位统一社会信用代码）
        /// </summary>
        /// <param name="taxNumber">税号</param>
        /// <returns>行业名称</returns>
        public static string? GetIndustryName(string? taxNumber)
        {
            string? code = GetIndustryCode(taxNumber);
            if (code == null)
            {
                return null;
            }

            return IndustryCodeMap.TryGetValue(code, out string? name) ? name : null;
        }

        /// <summary>
        /// 获取主体标识码（仅18位统一社会信用代码）
        /// </summary>
        /// <param name="taxNumber">税号</param>
        /// <returns>主体标识码</returns>
        public static string? GetSubjectIdentifier(string? taxNumber)
        {
            if (!IsValid18(taxNumber))
            {
                return null;
            }

            return taxNumber!.Substring(11, 6);
        }

        #endregion

        #region 格式化方法

        /// <summary>
        /// 格式化税号（转大写，去除特殊字符）
        /// </summary>
        /// <param name="taxNumber">税号</param>
        /// <returns>格式化后的税号</returns>
        public static string? Normalize(string? taxNumber)
        {
            if (string.IsNullOrWhiteSpace(taxNumber))
            {
                return null;
            }

            // 去除空格和特殊字符，转大写
            return taxNumber.ToUpper().Trim();
        }

        /// <summary>
        /// 税号脱敏：911010****001Q
        /// </summary>
        /// <param name="taxNumber">税号</param>
        /// <returns>脱敏后的税号</returns>
        public static string? Mask(string? taxNumber)
        {
            string? normalized = Normalize(taxNumber);
            if (normalized == null)
            {
                return null;
            }

            if (normalized.Length == 18)
            {
                // 保留前5位 + 后3位
                return normalized.Substring(0, 5) + "**********" + normalized.Substring(15);
            }

            if (normalized.Length == 15)
            {
                // 保留前4位 + 后3位
                return normalized.Substring(0, 4) + "********" + normalized.Substring(12);
            }

            if (normalized.Length == 20)
            {
                // 保留前5位 + 后3位
                return normalized.Substring(0, 5) + "************" + normalized.Substring(17);
            }

            return null;
        }

        #endregion

        #region 生成方法

        /// <summary>
        /// 生成随机统一社会信用代码（仅供测试使用）
        /// </summary>
        /// <param name="departmentCode">登记管理部门代码（可选，默认91-工商）</param>
        /// <param name="organizationType">机构类型代码（可选，默认1-企业）</param>
        /// <param name="areaCode">行政区划代码（可选，默认110101-北京市东城区）</param>
        /// <returns>18位统一社会信用代码</returns>
        public static string GenerateRandom(
            string? departmentCode = null,
            char? organizationType = null,
            string? areaCode = null)
        {
            // 登记管理部门代码（2位）
            string deptCode = departmentCode ?? "91";

            // 机构类型（1位）
            char orgType = organizationType ?? '1';

            // 行政区划代码（6位）
            string area = areaCode ?? "110101";

            // 行业代码（2位）
            string[] industries = { "51", "52", "63", "64", "65", "70", "72" };
            string industry = MathCategory.RandomUtil.GetRandomElement(industries);

            // 主体标识码（6位）
            string subject = GenerateRandomCode(6);

            // 前17位
            string code17 = deptCode + orgType + area + industry + subject;

            // 计算校验码
            char? checkCode = CalculateCheckCode(code17);
            if (!checkCode.HasValue)
            {
                throw new InvalidOperationException("计算校验码失败");
            }

            return code17 + checkCode.Value;
        }

        /// <summary>
        /// 将15位旧税号转换为18位统一社会信用代码
        /// 注意：这是一个近似转换，实际转换需要根据具体情况补充信息
        /// </summary>
        /// <param name="taxNumber15">15位旧税号</param>
        /// <param name="organizationType">机构类型代码（默认1-企业）</param>
        /// <returns>18位统一社会信用代码，转换失败返回null</returns>
        public static string? Convert15To18(string? taxNumber15, char organizationType = '1')
        {
            if (!IsValid15(taxNumber15))
            {
                return null;
            }

            // 15位旧税号结构：6位区域码 + 9位组织机构代码
            // 18位统一社会信用代码结构：
            // - 登记管理部门（2位）：默认91（工商）
            // - 机构类型（1位）
            // - 行政区划（6位）：取旧税号前6位
            // - 主体标识码（9位）：取旧税号后9位
            // - 校验码（1位）

            string areaCode = taxNumber15!.Substring(0, 6);
            string subjectCode = taxNumber15.Substring(6, 9);

            // 前17位：91 + 机构类型 + 区域码 + 主体标识码
            string code17 = "91" + organizationType + areaCode + subjectCode;

            // 计算校验码
            char? checkCode = CalculateCheckCode(code17);
            if (!checkCode.HasValue)
            {
                return null;
            }

            return code17 + checkCode.Value;
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 生成随机代码（使用BaseCode字符集）
        /// </summary>
        private static string GenerateRandomCode(int length)
        {
            var sb = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                sb.Append(BaseCode[MathCategory.RandomUtil.RandomInt(0, BaseCode.Length)]);
            }
            return sb.ToString();
        }

        #endregion
    }
}
