using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace EasyTool.BusinessCategory
{
    /// <summary>
    /// 邮政编码工具类
    /// </summary>
    public static class PostalCodeUtil
    {
        #region 常量与私有字段

        /// <summary>
        /// 中国邮政编码正则表达式（6位数字）
        /// </summary>
        private static readonly Regex PostalCodeRegex = new Regex(@"^\d{6}$", RegexOptions.Compiled);

        /// <summary>
        /// 非数字字符正则表达式
        /// </summary>
        private static readonly Regex NonDigitRegex = new Regex(@"\D", RegexOptions.Compiled);

        /// <summary>
        /// 省份编码前缀与名称映射（邮政编码前2位）
        /// </summary>
        private static readonly Dictionary<string, string> ProvincePrefixMap = new Dictionary<string, string>
        {
            { "10", "北京市" }, { "11", "北京市" }, { "12", "天津市" },
            { "01", "上海市" }, { "02", "上海市" }, { "03", "上海市" }, { "20", "上海市" },
            { "05", "河北省" }, { "06", "河北省" }, { "07", "河北省" },
            { "03", "山西省" }, { "04", "山西省" }, { "03", "内蒙古自治区" }, { "01", "内蒙古自治区" }, { "02", "内蒙古自治区" },
            { "11", "辽宁省" }, { "12", "辽宁省" },
            { "13", "吉林省" }, { "10", "吉林省" },
            { "15", "黑龙江省" }, { "16", "黑龙江省" },
            { "21", "江苏省" }, { "22", "江苏省" },
            { "31", "浙江省" }, { "32", "浙江省" },
            { "23", "安徽省" }, { "24", "安徽省" },
            { "35", "福建省" }, { "36", "福建省" },
            { "33", "江西省" }, { "34", "江西省" },
            { "25", "山东省" }, { "26", "山东省" }, { "27", "山东省" },
            { "45", "河南省" }, { "46", "河南省" }, { "47", "河南省" },
            { "41", "湖北省" }, { "42", "湖北省" }, { "43", "湖北省" }, { "44", "湖北省" },
            { "41", "湖南省" }, { "42", "湖南省" }, { "43", "湖南省" },
            { "51", "广东省" }, { "52", "广东省" }, { "53", "广东省" },
            { "54", "广西壮族自治区" }, { "55", "广西壮族自治区" },
            { "57", "海南省" }, { "58", "海南省" },
            { "40", "重庆市" },
            { "61", "四川省" }, { "62", "四川省" }, { "63", "四川省" }, { "64", "四川省" },
            { "55", "贵州省" }, { "56", "贵州省" },
            { "65", "云南省" }, { "66", "云南省" }, { "67", "云南省" },
            { "85", "西藏自治区" }, { "86", "西藏自治区" },
            { "71", "陕西省" }, { "72", "陕西省" }, { "73", "陕西省" },
            { "73", "甘肃省" }, { "74", "甘肃省" },
            { "81", "青海省" }, { "82", "青海省" }, { "83", "青海省" },
            { "75", "宁夏回族自治区" },
            { "83", "新疆维吾尔自治区" }, { "84", "新疆维吾尔自治区" }
        };

        /// <summary>
        /// 城市邮政编码范围映射（部分主要城市）
        /// </summary>
        private static readonly Dictionary<string, (string Min, string Max, string City)> CityCodeRanges = new Dictionary<string, (string, string, string)>
        {
            // 直辖市
            { "北京", ("100000", "102999", "北京市") },
            { "上海", ("200000", "202999", "上海市") },
            { "天津", ("300000", "302999", "天津市") },
            { "重庆", ("400000", "409999", "重庆市") },

            // 省会城市
            { "石家庄", ("050000", "052999", "石家庄市") },
            { "太原", ("030000", "032999", "太原市") },
            { "呼和浩特", ("010000", "012999", "呼和浩特市") },
            { "沈阳", ("110000", "112999", "沈阳市") },
            { "长春", ("130000", "132999", "长春市") },
            { "哈尔滨", ("150000", "152999", "哈尔滨市") },
            { "南京", ("210000", "212999", "南京市") },
            { "杭州", ("310000", "312999", "杭州市") },
            { "合肥", ("230000", "232999", "合肥市") },
            { "福州", ("350000", "352999", "福州市") },
            { "南昌", ("330000", "332999", "南昌市") },
            { "济南", ("250000", "252999", "济南市") },
            { "郑州", ("450000", "452999", "郑州市") },
            { "武汉", ("430000", "432999", "武汉市") },
            { "长沙", ("410000", "412999", "长沙市") },
            { "广州", ("510000", "512999", "广州市") },
            { "南宁", ("530000", "532999", "南宁市") },
            { "海口", ("570000", "572999", "海口市") },
            { "成都", ("610000", "612999", "成都市") },
            { "贵阳", ("550000", "552999", "贵阳市") },
            { "昆明", ("650000", "652999", "昆明市") },
            { "拉萨", ("850000", "852999", "拉萨市") },
            { "西安", ("710000", "712999", "西安市") },
            { "兰州", ("730000", "732999", "兰州市") },
            { "西宁", ("810000", "812999", "西宁市") },
            { "银川", ("750000", "752999", "银川市") },
            { "乌鲁木齐", ("830000", "832999", "乌鲁木齐市") },

            // 重要城市
            { "深圳", ("518000", "518999", "深圳市") },
            { "珠海", ("519000", "519999", "珠海市") },
            { "汕头", ("515000", "515999", "汕头市") },
            { "佛山", ("528000", "528999", "佛山市") },
            { "东莞", ("523000", "523999", "东莞市") },
            { "中山", ("528400", "528499", "中山市") },
            { "苏州", ("215000", "215999", "苏州市") },
            { "无锡", ("214000", "214999", "无锡市") },
            { "宁波", ("315000", "315999", "宁波市") },
            { "温州", ("325000", "325999", "温州市") },
            { "青岛", ("266000", "266999", "青岛市") },
            { "大连", ("116000", "116999", "大连市") },
            { "厦门", ("361000", "361999", "厦门市") }
        };

        #endregion

        #region 验证方法

        /// <summary>
        /// 验证邮政编码格式是否有效
        /// </summary>
        /// <param name="postalCode">邮政编码</param>
        /// <returns>是否有效</returns>
        public static bool IsValid(string? postalCode)
        {
            if (string.IsNullOrWhiteSpace(postalCode))
            {
                return false;
            }

            return PostalCodeRegex.IsMatch(postalCode);
        }

        /// <summary>
        /// 验证邮政编码是否有效且存在对应的省份
        /// </summary>
        /// <param name="postalCode">邮政编码</param>
        /// <returns>是否为有效且存在的邮政编码</returns>
        public static bool IsValidAndExists(string? postalCode)
        {
            if (!IsValid(postalCode))
            {
                return false;
            }

            return GetProvince(postalCode) != null;
        }

        #endregion

        #region 信息查询

        /// <summary>
        /// 获取省份名称
        /// </summary>
        /// <param name="postalCode">邮政编码</param>
        /// <returns>省份名称</returns>
        public static string? GetProvince(string? postalCode)
        {
            if (!IsValid(postalCode))
            {
                return null;
            }

            string prefix = postalCode!.Substring(0, 2);

            // 特殊处理直辖市
            if (prefix == "10" || prefix == "11")
            {
                return "北京市";
            }
            if (prefix == "12")
            {
                return "天津市";
            }
            if (prefix == "20" || prefix == "01" || prefix == "02")
            {
                return "上海市";
            }
            if (prefix == "40")
            {
                return "重庆市";
            }

            // 根据前2位判断省份
            return prefix switch
            {
                "05" or "06" or "07" => "河北省",
                "03" or "04" => "山西省",
                "01" or "02" => CheckInnerMongolia(postalCode) ? "内蒙古自治区" : null,
                "11" or "12" => "辽宁省",
                "13" => "吉林省",
                "15" or "16" => "黑龙江省",
                "21" or "22" => "江苏省",
                "31" or "32" => "浙江省",
                "23" or "24" => "安徽省",
                "35" or "36" => "福建省",
                "33" or "34" => "江西省",
                "25" or "26" or "27" => "山东省",
                "45" or "46" or "47" => "河南省",
                "43" or "44" => "湖北省",
                "41" or "42" => "湖南省",
                "51" or "52" or "53" => "广东省",
                "54" or "55" => "广西壮族自治区",
                "57" or "58" => "海南省",
                "61" or "62" or "63" or "64" => "四川省",
                "55" or "56" => "贵州省",
                "65" or "66" or "67" => "云南省",
                "85" or "86" => "西藏自治区",
                "71" or "72" or "73" => "陕西省",
                "73" or "74" => "甘肃省",
                "81" or "82" => "青海省",
                "75" => "宁夏回族自治区",
                "83" or "84" => "新疆维吾尔自治区",
                _ => null
            };
        }

        /// <summary>
        /// 获取城市名称（部分城市支持）
        /// </summary>
        /// <param name="postalCode">邮政编码</param>
        /// <returns>城市名称</returns>
        public static string? GetCity(string? postalCode)
        {
            if (!IsValid(postalCode))
            {
                return null;
            }

            string code = postalCode!;

            // 遍历城市编码范围
            foreach (var kvp in CityCodeRanges)
            {
                if (string.Compare(code, kvp.Value.Min) >= 0 && string.Compare(code, kvp.Value.Max) <= 0)
                {
                    return kvp.Value.City;
                }
            }

            return null;
        }

        /// <summary>
        /// 根据城市名称查询邮政编码（返回主要邮编）
        /// </summary>
        /// <param name="cityName">城市名称</param>
        /// <returns>邮政编码，未找到返回null</returns>
        public static string? GetPostalCodeByCity(string? cityName)
        {
            if (string.IsNullOrWhiteSpace(cityName))
            {
                return null;
            }

            // 处理常见城市名称变体
            string normalizedCity = cityName.Replace("市", "").Trim();

            foreach (var kvp in CityCodeRanges)
            {
                if (kvp.Key.Contains(normalizedCity) || normalizedCity.Contains(kvp.Key))
                {
                    return kvp.Value.Min;
                }
            }

            return null;
        }

        /// <summary>
        /// 获取邮政编码前缀（前2位）
        /// </summary>
        /// <param name="postalCode">邮政编码</param>
        /// <returns>前缀</returns>
        public static string? GetPrefix(string? postalCode)
        {
            if (!IsValid(postalCode))
            {
                return null;
            }

            return postalCode!.Substring(0, 2);
        }

        /// <summary>
        /// 获取邮政编码后缀（后4位）
        /// </summary>
        /// <param name="postalCode">邮政编码</param>
        /// <returns>后缀</returns>
        public static string? GetSuffix(string? postalCode)
        {
            if (!IsValid(postalCode))
            {
                return null;
            }

            return postalCode!.Substring(2, 4);
        }

        #endregion

        #region 格式化方法

        /// <summary>
        /// 格式化邮政编码（去除非数字字符）
        /// </summary>
        /// <param name="postalCode">邮政编码</param>
        /// <returns>格式化后的邮政编码</returns>
        public static string? Normalize(string? postalCode)
        {
            if (string.IsNullOrWhiteSpace(postalCode))
            {
                return null;
            }

            // 去除所有非数字字符
            string normalized = NonDigitRegex.Replace(postalCode, "");

            if (normalized.Length != 6)
            {
                return null;
            }

            return normalized;
        }

        /// <summary>
        /// 邮政编码脱敏：100***
        /// </summary>
        /// <param name="postalCode">邮政编码</param>
        /// <returns>脱敏后的邮政编码</returns>
        public static string? Mask(string? postalCode)
        {
            if (!IsValid(postalCode))
            {
                return null;
            }

            return postalCode!.Substring(0, 3) + "***";
        }

        #endregion

        #region 生成方法

        /// <summary>
        /// 生成随机邮政编码（仅供测试使用）
        /// </summary>
        /// <param name="province">省份名称（可选，默认随机）</param>
        /// <returns>6位邮政编码</returns>
        public static string GenerateRandom(string? province = null)
        {
            if (!string.IsNullOrWhiteSpace(province))
            {
                // 根据省份生成
                string prefix = GetProvincePrefix(province);
                if (!string.IsNullOrEmpty(prefix))
                {
                    return prefix + MathCategory.RandomUtil.RandomDigitString(4);
                }
            }

            // 随机生成有效前缀
            string[] validPrefixes = {
                "10", "11", "12", "20", "30", "40",
                "05", "06", "07", "03", "04", "01", "02",
                "11", "12", "13", "15", "16",
                "21", "22", "31", "32", "23", "24",
                "35", "36", "33", "34", "25", "26", "27",
                "45", "46", "47", "43", "44", "41", "42",
                "51", "52", "53", "54", "55", "57", "58",
                "40", "61", "62", "63", "64", "65", "66", "67",
                "85", "86", "71", "72", "73", "74", "75", "81", "82", "83", "84"
            };

            string randomPrefix = MathCategory.RandomUtil.GetRandomElement(validPrefixes);
            return randomPrefix + MathCategory.RandomUtil.RandomDigitString(4);
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 检查是否为内蒙古邮编
        /// </summary>
        private static bool CheckInnerMongolia(string postalCode)
        {
            // 内蒙古邮编范围：010000-029999
            string prefix = postalCode.Substring(0, 2);
            return prefix == "01" || prefix == "02";
        }

        /// <summary>
        /// 根据省份名称获取邮编前缀
        /// </summary>
        private static string? GetProvincePrefix(string province)
        {
            string normalized = province.Replace("省", "").Replace("市", "").Replace("自治区", "").Trim();

            return normalized switch
            {
                "北京" => "10",
                "上海" => "20",
                "天津" => "30",
                "重庆" => "40",
                "河北" => "05",
                "山西" => "03",
                "内蒙古" => "01",
                "辽宁" => "11",
                "吉林" => "13",
                "黑龙江" => "15",
                "江苏" => "21",
                "浙江" => "31",
                "安徽" => "23",
                "福建" => "35",
                "江西" => "33",
                "山东" => "25",
                "河南" => "45",
                "湖北" => "43",
                "湖南" => "41",
                "广东" => "51",
                "广西" => "54",
                "海南" => "57",
                "四川" => "61",
                "贵州" => "55",
                "云南" => "65",
                "西藏" => "85",
                "陕西" => "71",
                "甘肃" => "73",
                "青海" => "81",
                "宁夏" => "75",
                "新疆" => "83",
                _ => null
            };
        }

        #endregion
    }
}
