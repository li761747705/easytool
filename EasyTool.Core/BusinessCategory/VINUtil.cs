using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace EasyTool.BusinessCategory
{
    /// <summary>
    /// VIN（车辆识别代号）工具类
    /// </summary>
    public static class VINUtil
    {
        #region 常量与私有字段

        /// <summary>
        /// VIN正则表达式（17位，不含I、O、Q）
        /// </summary>
        private static readonly Regex VINRegex = new(
            @"^[A-HJ-NPR-Z0-9]{17}$",
            RegexOptions.Compiled);

        /// <summary>
        /// VIN字符值映射表（不含I、O、Q）
        /// </summary>
        private static readonly Dictionary<char, int> CharValueMap = new()
        {
            {'A', 1}, {'B', 2}, {'C', 3}, {'D', 4}, {'E', 5}, {'F', 6}, {'G', 7}, {'H', 8},
            {'J', 1}, {'K', 2}, {'L', 3}, {'M', 4}, {'N', 5}, {'P', 7}, {'R', 9},
            {'S', 2}, {'T', 3}, {'U', 4}, {'V', 5}, {'W', 6}, {'X', 7}, {'Y', 8}, {'Z', 9},
            {'0', 0}, {'1', 1}, {'2', 2}, {'3', 3}, {'4', 4}, {'5', 5}, {'6', 6}, {'7', 7}, {'8', 8}, {'9', 9}
        };

        /// <summary>
        /// VIN位置权重
        /// </summary>
        private static readonly int[] Weights = { 8, 7, 6, 5, 4, 3, 2, 10, 0, 9, 8, 7, 6, 5, 4, 3, 2 };

        /// <summary>
        /// WMI（世界制造商识别码）映射（部分）
        /// </summary>
        private static readonly (string Code, string Manufacturer)[] WmiMap =
        {
            // 中国
            ("LSV", "上海大众"), ("LSJ", "上海通用"), ("LSG", "上海通用五菱"),
            ("LDC", "神龙富康"), ("LEN", "北京吉普"), ("LHB", "华晨宝马"),
            ("LBV", "宝马"), ("LJC", "捷豹路虎"), ("LTV", "天津丰田"),
            ("LFV", "一汽大众"), ("LFP", "一汽轿车"), ("LFW", "一汽夏利"),
            ("LKG", "长安铃木"), ("LKL", "长安福特"), ("LLV", "长安汽车"),
            ("LVF", "东风日产"), ("LUG", "东风本田"), ("LVH", "东风本田"),
            ("LZW", "柳州五菱"), ("LJD", "江淮汽车"), ("LKY", "奇瑞汽车"),
            ("LVS", "长安马自达"), ("LZY", "众泰汽车"), ("LVSH", "福特中国"),

            // 德国
            ("WBA", "宝马"), ("WBS", "宝马M"), ("WBW", "宝马"),
            ("WAU", "奥迪"), ("WA1", "奥迪SUV"),
            ("WDB", "奔驰"), ("WDC", "奔驰"), ("WDD", "奔驰"),
            ("WVW", "大众"), ("WV2", "大众商用车"), ("WVG", "大众SUV"),
            ("WPO", "保时捷"),

            // 日本
            ("JTD", "丰田"), ("JTM", "丰田"), ("JTK", "丰田"),
            ("JHM", "本田"), ("JHG", "本田"), ("JHL", "本田"),
            ("JN1", "日产"), ("JN8", "日产"), ("JN3", "日产"),
            ("JM1", "马自达"), ("JMZ", "马自达"),
            ("JS1", "铃木"), ("JS2", "铃木"), ("JS3", "铃木"),
            ("KL1", "大宇"), ("KL2", "大宇"),

            // 美国
            ("1G1", "雪佛兰"), ("1G2", "庞蒂亚克"), ("1G3", "奥兹莫比尔"),
            ("1G4", "别克"), ("1G6", "凯迪拉克"), ("1G8", "萨博"),
            ("1GM", "通用"), ("1HG", "本田美国"), ("1J4", "Jeep"),
            ("1F1", "福特"), ("1F2", "福特"), ("1FA", "福特"), ("1FB", "福特"),
            ("1C3", "克莱斯勒"), ("1C4", "克莱斯勒"), ("1C6", "克莱斯勒"),
            ("2G1", "雪佛兰加拿大"), ("2G2", "庞蒂亚克加拿大"),
            ("2HM", "现代加拿大"), ("2HG", "本田加拿大"),

            // 韩国
            ("KMH", "现代"), ("KMB", "现代"), ("KNA", "起亚"), ("KND", "起亚"),

            // 英国
            ("SAJ", "捷豹"), ("SAL", "路虎"), ("SCC", "迈凯伦"),

            // 意大利
            ("ZAM", "玛莎拉蒂"), ("ZAR", "阿尔法罗密欧"),
            ("ZDF", "法拉利"), ("ZFF", "法拉利"),
            ("ZHW", "兰博基尼"),

            // 法国
            ("VF1", "雷诺"), ("VF3", "标致"), ("VF7", "雪铁龙"),

            // 瑞典
            ("YV1", "沃尔沃"), ("YV4", "沃尔沃"), ("YV2", "沃尔沃货车")
        };

        /// <summary>
        /// VDS车辆特征码映射（简化版）
        /// </summary>
        private static readonly Dictionary<string, string> VehicleTypeMap = new()
        {
            {"A", "轿车"}, {"B", "客车"}, {"C", "跑车"}, {"S", "SUV/跨界车"},
            {"T", "卡车"}, {"V", "MPV/厢式车"}, {"W", "旅行车"}, {"X", "特种车"}
        };

        /// <summary>
        /// 年份代码映射
        /// </summary>
        private static readonly Dictionary<char, int> YearCodeMap = new()
        {
            {'A', 2010}, {'B', 2011}, {'C', 2012}, {'D', 2013}, {'E', 2014},
            {'F', 2015}, {'G', 2016}, {'H', 2017}, {'J', 2018}, {'K', 2019},
            {'L', 2020}, {'M', 2021}, {'N', 2022}, {'P', 2023}, {'R', 2024},
            {'S', 2025}, {'T', 2026}, {'V', 2027}, {'W', 2028}, {'X', 2029},
            {'Y', 2030},
            {'1', 2001}, {'2', 2002}, {'3', 2003}, {'4', 2004}, {'5', 2005},
            {'6', 2006}, {'7', 2007}, {'8', 2008}, {'9', 2009}
        };

        #endregion

        #region 验证方法

        /// <summary>
        /// 验证VIN是否有效（格式+校验位）
        /// </summary>
        /// <param name="vin">VIN码</param>
        /// <returns>是否有效</returns>
        public static bool IsValid(string? vin)
        {
            if (!IsValidFormat(vin))
            {
                return false;
            }

            return ValidateCheckDigit(vin!);
        }

        /// <summary>
        /// 仅验证VIN格式（不校验）
        /// </summary>
        /// <param name="vin">VIN码</param>
        /// <returns>格式是否正确</returns>
        public static bool IsValidFormat(string? vin)
        {
            if (string.IsNullOrWhiteSpace(vin))
            {
                return false;
            }

            return VINRegex.IsMatch(vin.ToUpper());
        }

        /// <summary>
        /// 验证VIN校验位
        /// </summary>
        /// <param name="vin">VIN码</param>
        /// <returns>校验位是否正确</returns>
        public static bool ValidateCheckDigit(string? vin)
        {
            if (!IsValidFormat(vin))
            {
                return false;
            }

            string upper = vin!.ToUpper();
            int sum = 0;

            for (int i = 0; i < 17; i++)
            {
                if (!CharValueMap.TryGetValue(upper[i], out int value))
                {
                    return false;
                }
                sum += value * Weights[i];
            }

            char expectedCheck = (sum % 11) switch
            {
                10 => 'X',
                _ => (char)('0' + (sum % 11))
            };

            return upper[8] == expectedCheck;
        }

        /// <summary>
        /// 计算VIN校验位
        /// </summary>
        /// <param name="vin16">不含校验位的16位VIN</param>
        /// <returns>校验位（0-9或X），计算失败返回null</returns>
        public static char? CalculateCheckDigit(string? vin16)
        {
            if (string.IsNullOrWhiteSpace(vin16) || vin16.Length != 16)
            {
                return null;
            }

            int sum = 0;
            for (int i = 0; i < 16; i++)
            {
                char c = char.ToUpper(vin16[i]);
                if (!CharValueMap.TryGetValue(c, out int value))
                {
                    return null;
                }
                // 权重需要跳过第9位（校验位位置）
                int weight = i >= 8 ? Weights[i + 1] : Weights[i];
                sum += value * weight;
            }

            return (sum % 11) switch
            {
                10 => 'X',
                _ => (char)('0' + (sum % 11))
            };
        }

        #endregion

        #region 信息提取

        /// <summary>
        /// 获取WMI（世界制造商识别码，前3位）
        /// </summary>
        /// <param name="vin">VIN码</param>
        /// <returns>WMI码</returns>
        public static string? GetWMI(string? vin)
        {
            if (!IsValidFormat(vin))
            {
                return null;
            }

            return vin!.Substring(0, 3).ToUpper();
        }

        /// <summary>
        /// 获取制造商
        /// </summary>
        /// <param name="vin">VIN码</param>
        /// <returns>制造商名称</returns>
        public static string? GetManufacturer(string? vin)
        {
            string? wmi = GetWMI(vin);
            if (wmi == null)
            {
                return null;
            }

            foreach (var mapping in WmiMap)
            {
                if (wmi.StartsWith(mapping.Code))
                {
                    return mapping.Manufacturer;
                }
            }

            return null;
        }

        /// <summary>
        /// 获取生产地区（根据WMI判断）
        /// </summary>
        /// <param name="vin">VIN码</param>
        /// <returns>生产地区</returns>
        public static string? GetRegion(string? vin)
        {
            string? wmi = GetWMI(vin);
            if (wmi == null)
            {
                return null;
            }

            char first = wmi[0];
            return first switch
            {
                'L' => "中国",
                'W' => "德国",
                'J' => "日本",
                'K' => "韩国",
                '1' or '2' or '3' or '4' or '5' => "美国/加拿大",
                'S' => "英国",
                'Z' => "意大利",
                'V' => "法国",
                'Y' => "瑞典",
                '6' or '7' => "大洋洲",
                '8' or '9' => "南美洲",
                _ => null
            };
        }

        /// <summary>
        /// 获取VDS（车辆特征码，第4-9位）
        /// </summary>
        /// <param name="vin">VIN码</param>
        /// <returns>VDS码</returns>
        public static string? GetVDS(string? vin)
        {
            if (!IsValidFormat(vin))
            {
                return null;
            }

            return vin!.Substring(3, 6).ToUpper();
        }

        /// <summary>
        /// 获取VIS（车辆指示码，第10-17位）
        /// </summary>
        /// <param name="vin">VIN码</param>
        /// <returns>VIS码</returns>
        public static string? GetVIS(string? vin)
        {
            if (!IsValidFormat(vin))
            {
                return null;
            }

            return vin!.Substring(9, 8).ToUpper();
        }

        /// <summary>
        /// 获取车型年份
        /// </summary>
        /// <param name="vin">VIN码</param>
        /// <returns>车型年份</returns>
        public static int? GetModelYear(string? vin)
        {
            if (!IsValidFormat(vin))
            {
                return null;
            }

            char yearCode = char.ToUpper(vin![9]);
            return YearCodeMap.TryGetValue(yearCode, out int year) ? year : null;
        }

        /// <summary>
        /// 获取装配厂代码（第11位）
        /// </summary>
        /// <param name="vin">VIN码</param>
        /// <returns>装配厂代码</returns>
        public static char? GetPlantCode(string? vin)
        {
            if (!IsValidFormat(vin))
            {
                return null;
            }

            return char.ToUpper(vin![10]);
        }

        /// <summary>
        /// 获取生产序列号（第12-17位）
        /// </summary>
        /// <param name="vin">VIN码</param>
        /// <returns>序列号</returns>
        public static string? GetSequenceNumber(string? vin)
        {
            if (!IsValidFormat(vin))
            {
                return null;
            }

            return vin!.Substring(11, 6).ToUpper();
        }

        #endregion

        #region 格式化方法

        /// <summary>
        /// 格式化VIN（转大写）
        /// </summary>
        /// <param name="vin">VIN码</param>
        /// <returns>格式化后的VIN</returns>
        public static string? Normalize(string? vin)
        {
            if (string.IsNullOrWhiteSpace(vin))
            {
                return null;
            }

            string upper = vin.ToUpper().Trim();
            return upper.Length == 17 && VINRegex.IsMatch(upper) ? upper : null;
        }

        /// <summary>
        /// VIN脱敏：LSV***********X
        /// </summary>
        /// <param name="vin">VIN码</param>
        /// <returns>脱敏后的VIN</returns>
        public static string? Mask(string? vin)
        {
            string? normalized = Normalize(vin);
            if (normalized == null)
            {
                return null;
            }

            return normalized.Substring(0, 3) + "*********" + normalized.Substring(14, 3);
        }

        #endregion

        #region 生成方法

        /// <summary>
        /// 生成随机VIN（仅供测试使用）
        /// </summary>
        /// <param name="wmi">WMI码（可选，默认LSV-上海大众）</param>
        /// <param name="modelYear">车型年份（可选，默认2023）</param>
        /// <returns>17位VIN</returns>
        public static string GenerateRandom(string? wmi = null, int? modelYear = null)
        {
            // WMI（3位）
            string wmiCode = wmi ?? "LSV";

            // VDS（5位随机）
            const string vdsChars = "ABCDEFGHJKLMNPRSTUVWXYZ0123456789";
            string vds = "";
            for (int i = 0; i < 5; i++)
            {
                vds += MathCategory.RandomUtil.GetRandomElement(vdsChars.ToCharArray());
            }

            // 年份代码
            int year = modelYear ?? 2023;
            char yearCode = GetYearCode(year);

            // 装配厂代码（1位）
            char plantCode = MathCategory.RandomUtil.GetRandomElement(vdsChars.ToCharArray());

            // 序列号（5位）
            string sequence = MathCategory.RandomUtil.RandomDigitString(5);

            // 组合16位，计算校验位
            string vin16 = wmiCode + vds + yearCode + plantCode + sequence;
            char? checkDigit = CalculateCheckDigit(vin16);

            return vin16.Substring(0, 8) + (checkDigit ?? '0') + vin16.Substring(8);
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 根据年份获取年份代码
        /// </summary>
        private static char GetYearCode(int year)
        {
            foreach (var kvp in YearCodeMap)
            {
                if (kvp.Value == year)
                {
                    return kvp.Key;
                }
            }
            return 'P'; // 默认2023
        }

        #endregion
    }
}
