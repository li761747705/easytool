using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace EasyTool.BusinessCategory
{
    /// <summary>
    /// SWIFT银行代码工具类
    /// </summary>
    public static class SwiftCodeUtil
    {
        #region 常量与私有字段

        /// <summary>
        /// SWIFT代码正则表达式（8位或11位）
        /// </summary>
        private static readonly Regex SwiftRegex = new(
            @"^[A-Z]{4}[A-Z]{2}[A-Z0-9]{2}([A-Z0-9]{3})?$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// 中国主要银行SWIFT代码映射
        /// </summary>
        private static readonly Dictionary<string, (string Bank, string City)> ChinaBankSwiftMap = new()
        {
            // 工商银行
            { "ICBKCNBJ", ("中国工商银行", "北京") },
            { "ICBKCNBJBJN", ("中国工商银行", "济南") },
            { "ICBKCNBJCQX", ("中国工商银行", "重庆") },
            { "ICBKCNBJSHI", ("中国工商银行", "上海") },
            { "ICBKCNBJSZN", ("中国工商银行", "深圳") },
            { "ICBKCNBJGZU", ("中国工商银行", "广州") },
            { "ICBKCNBJNJA", ("中国工商银行", "南京") },
            { "ICBKCNBJHBR", ("中国工商银行", "哈尔滨") },
            { "ICBKCNBJTJN", ("中国工商银行", "天津") },
            { "ICBKCNBJCDU", ("中国工商银行", "成都") },
            { "ICBKCNBJWUH", ("中国工商银行", "武汉") },
            { "ICBKCNBJHAN", ("中国工商银行", "杭州") },
            { "ICBKCNBJXIM", ("中国工商银行", "厦门") },
            { "ICBKCNBJDLC", ("中国工商银行", "大连") },
            { "ICBKCNBJSYN", ("中国工商银行", "沈阳") },
            { "ICBKCNBJJIX", ("中国工商银行", "吉林") },
            { "ICBKCNBJSWA", ("中国工商银行", "汕头") },
            { "ICBKCNBJZHO", ("中国工商银行", "珠海") },
            { "ICBKCNBJFZH", ("中国工商银行", "福州") },
            { "ICBKCNBJKUN", ("中国工商银行", "昆明") },

            // 农业银行
            { "ABOCCNBJ", ("中国农业银行", "北京") },
            { "ABOCCNBJ070", ("中国农业银行", "哈尔滨") },
            { "ABOCCNBJ080", ("中国农业银行", "上海") },
            { "ABOCCNBJ100", ("中国农业银行", "广州") },
            { "ABOCCNBJ110", ("中国农业银行", "深圳") },
            { "ABOCCNBJ120", ("中国农业银行", "天津") },
            { "ABOCCNBJ130", ("中国农业银行", "重庆") },
            { "ABOCCNBJ140", ("中国农业银行", "南京") },
            { "ABOCCNBJ150", ("中国农业银行", "成都") },
            { "ABOCCNBJ160", ("中国农业银行", "武汉") },
            { "ABOCCNBJ170", ("中国农业银行", "杭州") },
            { "ABOCCNBJ180", ("中国农业银行", "济南") },
            { "ABOCCNBJ190", ("中国农业银行", "西安") },
            { "ABOCCNBJ200", ("中国农业银行", "沈阳") },

            // 中国银行
            { "BKCHCNBJ", ("中国银行", "北京") },
            { "BKCHCNBJ300", ("中国银行", "上海") },
            { "BKCHCNBJ400", ("中国银行", "广州") },
            { "BKCHCNBJ500", ("中国银行", "深圳") },
            { "BKCHCNBJ600", ("中国银行", "天津") },
            { "BKCHCNBJ700", ("中国银行", "重庆") },
            { "BKCHCNBJ800", ("中国银行", "南京") },
            { "BKCHCNBJ900", ("中国银行", "成都") },
            { "BKCHCNBJ910", ("中国银行", "武汉") },
            { "BKCHCNBJ920", ("中国银行", "杭州") },
            { "BKCHCNBJ930", ("中国银行", "济南") },
            { "BKCHCNBJ940", ("中国银行", "西安") },
            { "BKCHCNBJ950", ("中国银行", "沈阳") },
            { "BKCHCNBJ960", ("中国银行", "大连") },
            { "BKCHCNBJ970", ("中国银行", "青岛") },
            { "BKCHCNBJ980", ("中国银行", "厦门") },
            { "BKCHCNBJ990", ("中国银行", "福州") },

            // 建设银行
            { "PCBCCNBJ", ("中国建设银行", "北京") },
            { "PCBCCNBJBJX", ("中国建设银行", "北京") },
            { "PCBCCNBJSHX", ("中国建设银行", "上海") },
            { "PCBCCNBJGZX", ("中国建设银行", "广州") },
            { "PCBCCNBJSZX", ("中国建设银行", "深圳") },
            { "PCBCCNBJTJX", ("中国建设银行", "天津") },
            { "PCBCCNBJCQX", ("中国建设银行", "重庆") },
            { "PCBCCNBJNJX", ("中国建设银行", "南京") },
            { "PCBCCNBJCDX", ("中国建设银行", "成都") },
            { "PCBCCNBJWHX", ("中国建设银行", "武汉") },
            { "PCBCCNBJHZX", ("中国建设银行", "杭州") },
            { "PCBCCNBJJNX", ("中国建设银行", "济南") },
            { "PCBCCNBJXAX", ("中国建设银行", "西安") },
            { "PCBCCNBJSYX", ("中国建设银行", "沈阳") },
            { "PCBCCNBJDLX", ("中国建设银行", "大连") },
            { "PCBCCNBJQDX", ("中国建设银行", "青岛") },

            // 交通银行
            { "COMMCNSh", ("交通银行", "上海") },
            { "COMMCNShKUN", ("交通银行", "昆明") },
            { "COMMCNShGZH", ("交通银行", "广州") },

            // 招商银行
            { "CMBCCNBS", ("招商银行", "上海") },
            { "CMBCCNBS001", ("招商银行", "上海") },
            { "CMBCCNBS002", ("招商银行", "北京") },
            { "CMBCCNBS003", ("招商银行", "深圳") },
            { "CMBCCNBS004", ("招商银行", "广州") },

            // 中信银行
            { "CIBKCNBJ", ("中信银行", "北京") },
            { "CIBKCNBJSHI", ("中信银行", "上海") },
            { "CIBKCNBJGZU", ("中信银行", "广州") },
            { "CIBKCNBJSZN", ("中信银行", "深圳") },

            // 浦发银行
            { "SPDBCNSH", ("浦发银行", "上海") },
            { "SPDBCNSHBJG", ("浦发银行", "北京") },
            { "SPDBCNSHGXG", ("浦发银行", "广州") },
            { "SPDBCNSHSZN", ("浦发银行", "深圳") },

            // 民生银行
            { "MSBCCNBJ", ("民生银行", "北京") },
            { "MSBCCNBJ001", ("民生银行", "上海") },
            { "MSBCCNBJ002", ("民生银行", "广州") },

            // 光大银行
            { "EVERCNBJ", ("光大银行", "北京") },
            { "EVERCNBJ1BJ", ("光大银行", "北京") },
            { "EVERCNBJ1SH", ("光大银行", "上海") },

            // 华夏银行
            { "HXBKCNBJ", ("华夏银行", "北京") },
            { "HXBKCNBJ070", ("华夏银行", "上海") },

            // 兴业银行
            { "FJIBCNBA", ("兴业银行", "福州") },
            { "FJIBCNBA001", ("兴业银行", "北京") },
            { "FJIBCNBA002", ("兴业银行", "上海") },

            // 平安银行
            { "SZDBCNBS", ("平安银行", "深圳") },
            { "SZDBCNBS001", ("平安银行", "北京") },
            { "SZDBCNBS002", ("平安银行", "上海") },

            // 广发银行
            { "GDBKCN22", ("广发银行", "广州") },
            { "GDBKCN22001", ("广发银行", "北京") },
            { "GDBKCN22002", ("广发银行", "上海") },

            // 邮储银行
            { "PSBCCNBJ", ("邮储银行", "北京") },
            { "PSBCCNBJ001", ("邮储银行", "上海") },
            { "PSBCCNBJ002", ("邮储银行", "广州") },

            // 汇丰银行（中国）
            { "HSBCCNSH", ("汇丰银行（中国）", "上海") },
            { "HSBCCNSH001", ("汇丰银行（中国）", "北京") },
            { "HSBCCNSH002", ("汇丰银行（中国）", "广州") },

            // 渣打银行（中国）
            { "SCBLCNSX", ("渣打银行（中国）", "上海") },
            { "SCBLCNSX001", ("渣打银行（中国）", "北京") },

            // 花旗银行（中国）
            { "CITICNSX", ("花旗银行（中国）", "上海") },
            { "CITICNSX001", ("花旗银行（中国）", "北京") }
        };

        /// <summary>
        /// 国家代码与名称映射（部分）
        /// </summary>
        private static readonly Dictionary<string, string> CountryCodeMap = new()
        {
            { "CN", "中国" }, { "HK", "香港" }, { "TW", "台湾" }, { "JP", "日本" },
            { "KR", "韩国" }, { "SG", "新加坡" }, { "MY", "马来西亚" }, { "TH", "泰国" },
            { "AU", "澳大利亚" }, { "NZ", "新西兰" }, { "US", "美国" }, { "CA", "加拿大" },
            { "GB", "英国" }, { "DE", "德国" }, { "FR", "法国" }, { "IT", "意大利" },
            { "ES", "西班牙" }, { "NL", "荷兰" }, { "BE", "比利时" }, { "CH", "瑞士" },
            { "AT", "奥地利" }, { "SE", "瑞典" }, { "NO", "挪威" }, { "DK", "丹麦" },
            { "FI", "芬兰" }, { "RU", "俄罗斯" }, { "BR", "巴西" }, { "MX", "墨西哥" },
            { "AR", "阿根廷" }, { "ZA", "南非" }, { "AE", "阿联酋" }, { "SA", "沙特" },
            { "IN", "印度" }, { "PK", "巴基斯坦" }, { "ID", "印度尼西亚" }, { "PH", "菲律宾" },
            { "VN", "越南" }, { "MM", "缅甸" }, { "LU", "卢森堡" }, { "IE", "爱尔兰" },
            { "PT", "葡萄牙" }, { "GR", "希腊" }, { "PL", "波兰" }, { "CZ", "捷克" },
            { "HU", "匈牙利" }, { "TR", "土耳其" }, { "IL", "以色列" }, { "EG", "埃及" }
        };

        #endregion

        #region 验证方法

        /// <summary>
        /// 验证SWIFT代码是否有效
        /// </summary>
        /// <param name="swiftCode">SWIFT代码</param>
        /// <returns>是否有效</returns>
        public static bool IsValid(string? swiftCode)
        {
            if (string.IsNullOrWhiteSpace(swiftCode))
            {
                return false;
            }

            return SwiftRegex.IsMatch(swiftCode);
        }

        /// <summary>
        /// 验证是否为8位SWIFT代码（不含分行代码）
        /// </summary>
        /// <param name="swiftCode">SWIFT代码</param>
        /// <returns>是否为8位</returns>
        public static bool Is8Digit(string? swiftCode)
        {
            return swiftCode?.Length == 8 && IsValid(swiftCode);
        }

        /// <summary>
        /// 验证是否为11位SWIFT代码（含分行代码）
        /// </summary>
        /// <param name="swiftCode">SWIFT代码</param>
        /// <returns>是否为11位</returns>
        public static bool Is11Digit(string? swiftCode)
        {
            return swiftCode?.Length == 11 && IsValid(swiftCode);
        }

        #endregion

        #region 信息提取

        /// <summary>
        /// 获取银行代码（前4位）
        /// </summary>
        /// <param name="swiftCode">SWIFT代码</param>
        /// <returns>银行代码</returns>
        public static string? GetBankCode(string? swiftCode)
        {
            if (!IsValid(swiftCode))
            {
                return null;
            }

            return swiftCode!.Substring(0, 4).ToUpper();
        }

        /// <summary>
        /// 获取国家代码（第5-6位）
        /// </summary>
        /// <param name="swiftCode">SWIFT代码</param>
        /// <returns>国家代码</returns>
        public static string? GetCountryCode(string? swiftCode)
        {
            if (!IsValid(swiftCode))
            {
                return null;
            }

            return swiftCode!.Substring(4, 2).ToUpper();
        }

        /// <summary>
        /// 获取国家名称
        /// </summary>
        /// <param name="swiftCode">SWIFT代码</param>
        /// <returns>国家名称</returns>
        public static string? GetCountryName(string? swiftCode)
        {
            string? countryCode = GetCountryCode(swiftCode);
            if (countryCode == null)
            {
                return null;
            }

            return CountryCodeMap.TryGetValue(countryCode, out string? name) ? name : null;
        }

        /// <summary>
        /// 获取位置代码（第7-8位）
        /// </summary>
        /// <param name="swiftCode">SWIFT代码</param>
        /// <returns>位置代码</returns>
        public static string? GetLocationCode(string? swiftCode)
        {
            if (!IsValid(swiftCode))
            {
                return null;
            }

            return swiftCode!.Substring(6, 2).ToUpper();
        }

        /// <summary>
        /// 获取分行代码（第9-11位，11位代码才有）
        /// </summary>
        /// <param name="swiftCode">SWIFT代码</param>
        /// <returns>分行代码</returns>
        public static string? GetBranchCode(string? swiftCode)
        {
            if (!Is11Digit(swiftCode))
            {
                return null;
            }

            return swiftCode!.Substring(8, 3).ToUpper();
        }

        /// <summary>
        /// 判断是否为总行代码（第7-8位为XX或位置代码首位为0）
        /// </summary>
        /// <param name="swiftCode">SWIFT代码</param>
        /// <returns>是否为总行</returns>
        public static bool IsHeadOffice(string? swiftCode)
        {
            string? locationCode = GetLocationCode(swiftCode);
            if (locationCode == null)
            {
                return false;
            }

            // 位置代码为"XX"或首位为0表示总行
            return locationCode == "XX" || locationCode[0] == '0';
        }

        /// <summary>
        /// 获取银行信息（仅限中国主要银行）
        /// </summary>
        /// <param name="swiftCode">SWIFT代码</param>
        /// <returns>银行和城市信息</returns>
        public static (string Bank, string City)? GetBankInfo(string? swiftCode)
        {
            if (!IsValid(swiftCode))
            {
                return null;
            }

            string upper = swiftCode!.ToUpper();

            // 先尝试完整匹配
            if (ChinaBankSwiftMap.TryGetValue(upper, out var info))
            {
                return info;
            }

            // 再尝试8位匹配
            string code8 = upper.Substring(0, 8);
            if (ChinaBankSwiftMap.TryGetValue(code8, out info))
            {
                return info;
            }

            return null;
        }

        /// <summary>
        /// 获取银行名称
        /// </summary>
        /// <param name="swiftCode">SWIFT代码</param>
        /// <returns>银行名称</returns>
        public static string? GetBankName(string? swiftCode)
        {
            return GetBankInfo(swiftCode)?.Bank;
        }

        /// <summary>
        /// 获取城市名称
        /// </summary>
        /// <param name="swiftCode">SWIFT代码</param>
        /// <returns>城市名称</returns>
        public static string? GetCityName(string? swiftCode)
        {
            return GetBankInfo(swiftCode)?.City;
        }

        #endregion

        #region 格式化方法

        /// <summary>
        /// 格式化SWIFT代码（转大写）
        /// </summary>
        /// <param name="swiftCode">SWIFT代码</param>
        /// <returns>格式化后的SWIFT代码</returns>
        public static string? Normalize(string? swiftCode)
        {
            if (string.IsNullOrWhiteSpace(swiftCode))
            {
                return null;
            }

            string upper = swiftCode.ToUpper().Trim();
            return IsValid(upper) ? upper : null;
        }

        /// <summary>
        /// SWIFT代码脱敏：ICBK****BJ
        /// </summary>
        /// <param name="swiftCode">SWIFT代码</param>
        /// <returns>脱敏后的SWIFT代码</returns>
        public static string? Mask(string? swiftCode)
        {
            if (!IsValid(swiftCode))
            {
                return null;
            }

            string upper = swiftCode!.ToUpper();
            if (upper.Length == 8)
            {
                return upper.Substring(0, 4) + "****";
            }
            else
            {
                return upper.Substring(0, 4) + "*******";
            }
        }

        /// <summary>
        /// 转换为8位SWIFT代码（去除分行代码）
        /// </summary>
        /// <param name="swiftCode">SWIFT代码</param>
        /// <returns>8位SWIFT代码</returns>
        public static string? To8Digit(string? swiftCode)
        {
            if (!IsValid(swiftCode))
            {
                return null;
            }

            return swiftCode!.Substring(0, 8).ToUpper();
        }

        #endregion
    }
}
