using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace EasyTool.BusinessCategory
{
    /// <summary>
    /// 车牌类型枚举
    /// </summary>
    public enum PlateType
    {
        /// <summary>
        /// 未知类型
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// 普通车牌/燃油车牌（7位）
        /// </summary>
        Normal = 1,

        /// <summary>
        /// 小型新能源车牌（8位，渐变绿色）
        /// </summary>
        NewEnergySmall = 2,

        /// <summary>
        /// 大型新能源车牌（8位，黄绿双色）
        /// </summary>
        NewEnergyLarge = 3,

        /// <summary>
        /// 武警车牌
        /// </summary>
        WJ = 4,

        /// <summary>
        /// 军队车牌
        /// </summary>
        Military = 5
    }

    /// <summary>
    /// 新能源汽车类型枚举
    /// </summary>
    public enum NewEnergyType
    {
        /// <summary>
        /// 纯电动汽车
        /// </summary>
        PureElectric = 0,

        /// <summary>
        /// 插电式混合动力汽车（含增程式）
        /// </summary>
        PluginHybrid = 1
    }

    /// <summary>
    /// 车辆燃料类型枚举
    /// </summary>
    public enum FuelType
    {
        /// <summary>
        /// 未知类型
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// 燃油车（汽油/柴油）
        /// </summary>
        Fuel = 1,

        /// <summary>
        /// 纯电动汽车
        /// </summary>
        PureElectric = 2,

        /// <summary>
        /// 插电式混合动力汽车（含增程式）
        /// </summary>
        PluginHybrid = 3
    }

    /// <summary>
    /// 车牌号工具类
    /// </summary>
    public static class LicensePlateUtil
    {
        #region 常量与私有字段

        /// <summary>
        /// 普通车牌正则表达式（7位）
        /// 格式：省份简称（1位汉字）+ 发牌机关代号（1位字母）+ 序号（5位字母或数字）
        /// </summary>
        private static readonly Regex NormalPlateRegex = new Regex(
            @"^[京津冀晋蒙辽吉黑沪苏浙皖闽赣鲁豫鄂湘粤桂琼川贵云藏陕甘青宁新渝港澳台][A-Z][A-HJ-NP-Z0-9]{5}$",
            RegexOptions.Compiled);

        /// <summary>
        /// 小型新能源车牌正则表达式（8位）
        /// 格式：省份简称 + 字母 + 5位（第3位为D或F）
        /// </summary>
        private static readonly Regex NewEnergySmallRegex = new Regex(
            @"^[京津冀晋蒙辽吉黑沪苏浙皖闽赣鲁豫鄂湘粤桂琼川贵云藏陕甘青宁新渝港澳台][A-Z][DF][A-HJ-NP-Z0-9]{5}$",
            RegexOptions.Compiled);

        /// <summary>
        /// 大型新能源车牌正则表达式（8位）
        /// 格式：省份简称 + 字母 + 5位（第3位或第4-8位包含数字，第8位为D或F）
        /// </summary>
        private static readonly Regex NewEnergyLargeRegex = new Regex(
            @"^[京津冀晋蒙辽吉黑沪苏浙皖闽赣鲁豫鄂湘粤桂琼川贵云藏陕甘青宁新渝港澳台][A-Z][A-HJ-NP-Z0-9]{5}[DF]$",
            RegexOptions.Compiled);

        /// <summary>
        /// 武警车牌正则表达式
        /// 格式：WJ + 省份代码（2位数字）+ 1位字母 + 4位数字
        /// </summary>
        private static readonly Regex WJPlateRegex = new Regex(
            @"^WJ[0-9]{2}[0-9A-HJ-NP-Z]\d{4}$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// 军队车牌正则表达式（简化版）
        /// </summary>
        private static readonly Regex MilitaryPlateRegex = new Regex(
            @"^[VQZHBSLJKWETCYM][A-Z][A-HJ-NP-Z0-9]{5}$",
            RegexOptions.Compiled);

        /// <summary>
        /// 非中文字母数字正则表达式
        /// </summary>
        private static readonly Regex NonChineseAlphanumericRegex = new Regex(@"[^\u4e00-\u9fa5A-Z0-9]", RegexOptions.Compiled);

        /// <summary>
        /// 省份简称与名称映射
        /// </summary>
        private static readonly Dictionary<string, string> ProvinceMap = new Dictionary<string, string>
        {
            { "京", "北京市" }, { "津", "天津市" }, { "冀", "河北省" }, { "晋", "山西省" },
            { "蒙", "内蒙古自治区" }, { "辽", "辽宁省" }, { "吉", "吉林省" }, { "黑", "黑龙江省" },
            { "沪", "上海市" }, { "苏", "江苏省" }, { "浙", "浙江省" }, { "皖", "安徽省" },
            { "闽", "福建省" }, { "赣", "江西省" }, { "鲁", "山东省" }, { "豫", "河南省" },
            { "鄂", "湖北省" }, { "湘", "湖南省" }, { "粤", "广东省" }, { "桂", "广西壮族自治区" },
            { "琼", "海南省" }, { "川", "四川省" }, { "贵", "贵州省" }, { "云", "云南省" },
            { "藏", "西藏自治区" }, { "陕", "陕西省" }, { "甘", "甘肃省" }, { "青", "青海省" },
            { "宁", "宁夏回族自治区" }, { "新", "新疆维吾尔自治区" }, { "渝", "重庆市" },
            { "港", "香港特别行政区" }, { "澳", "澳门特别行政区" }, { "台", "台湾省" }
        };

        /// <summary>
        /// 车牌字母与城市映射（部分主要城市）
        /// </summary>
        private static readonly Dictionary<string, Dictionary<string, string>> CityMap = new Dictionary<string, Dictionary<string, string>>
        {
            { "京", new Dictionary<string, string> { { "A", "市区" }, { "B", "出租车" }, { "C", "郊区" }, { "D", "警车" }, { "E", "郊区" }, { "F", "郊区" }, { "G", "郊区" }, { "H", "郊区" }, { "J", "郊区" }, { "K", "郊区" }, { "L", "郊区" }, { "M", "郊区" }, { "N", "市区" }, { "P", "市区" }, { "Q", "市区" }, { "Y", "郊区" } } },
            { "沪", new Dictionary<string, string> { { "A", "市区" }, { "B", "市区" }, { "C", "郊区" }, { "D", "郊区" }, { "E", "市区" }, { "F", "郊区" }, { "G", "郊区" }, { "H", "郊区" }, { "J", "郊区" }, { "K", "郊区" }, { "L", "郊区" }, { "M", "郊区" }, { "N", "市区" }, { "R", "崇明" } } },
            { "粤", new Dictionary<string, string> { { "A", "广州市" }, { "B", "深圳市" }, { "C", "珠海市" }, { "D", "汕头市" }, { "E", "佛山市" }, { "F", "韶关市" }, { "G", "湛江市" }, { "H", "肇庆市" }, { "J", "江门市" }, { "K", "茂名市" }, { "L", "惠州市" }, { "M", "梅州市" }, { "N", "汕尾市" }, { "P", "河源市" }, { "Q", "阳江市" }, { "R", "清远市" }, { "S", "东莞市" }, { "T", "中山市" }, { "U", "潮州市" }, { "V", "揭阳市" }, { "W", "云浮市" }, { "X", "顺德区" }, { "Y", "南海区" }, { "Z", "港澳入境" } } },
            { "苏", new Dictionary<string, string> { { "A", "南京市" }, { "B", "无锡市" }, { "C", "徐州市" }, { "D", "常州市" }, { "E", "苏州市" }, { "F", "南通市" }, { "G", "连云港市" }, { "H", "淮安市" }, { "J", "盐城市" }, { "K", "扬州市" }, { "L", "镇江市" }, { "M", "泰州市" }, { "N", "宿迁市" } } },
            { "浙", new Dictionary<string, string> { { "A", "杭州市" }, { "B", "宁波市" }, { "C", "温州市" }, { "D", "绍兴市" }, { "E", "湖州市" }, { "F", "嘉兴市" }, { "G", "金华市" }, { "H", "衢州市" }, { "J", "台州市" }, { "K", "丽水市" }, { "L", "舟山市" } } },
        };

        #endregion

        #region 验证方法

        /// <summary>
        /// 验证车牌号是否有效
        /// </summary>
        /// <param name="plateNumber">车牌号</param>
        /// <returns>是否有效</returns>
        public static bool IsValid(string? plateNumber)
        {
            if (string.IsNullOrWhiteSpace(plateNumber))
            {
                return false;
            }

            string normalized = Normalize(plateNumber)!;
            return IsNormalPlate(normalized) || IsNewEnergyPlate(normalized) ||
                   IsWJPlate(normalized) || IsMilitaryPlate(normalized);
        }

        /// <summary>
        /// 验证是否为普通车牌（7位）
        /// </summary>
        /// <param name="plateNumber">车牌号</param>
        /// <returns>是否为普通车牌</returns>
        public static bool IsNormalPlate(string? plateNumber)
        {
            if (string.IsNullOrWhiteSpace(plateNumber))
            {
                return false;
            }

            return NormalPlateRegex.IsMatch(Normalize(plateNumber)!);
        }

        /// <summary>
        /// 验证是否为新能源车牌（8位）
        /// </summary>
        /// <param name="plateNumber">车牌号</param>
        /// <returns>是否为新能源车牌</returns>
        public static bool IsNewEnergyPlate(string? plateNumber)
        {
            if (string.IsNullOrWhiteSpace(plateNumber))
            {
                return false;
            }

            string normalized = Normalize(plateNumber)!;
            return IsSmallNewEnergyPlate(normalized) || IsLargeNewEnergyPlate(normalized);
        }

        /// <summary>
        /// 验证是否为小型新能源车牌（8位，第3位为D或F）
        /// </summary>
        /// <param name="plateNumber">车牌号</param>
        /// <returns>是否为小型新能源车牌</returns>
        public static bool IsSmallNewEnergyPlate(string? plateNumber)
        {
            if (string.IsNullOrWhiteSpace(plateNumber))
            {
                return false;
            }

            return NewEnergySmallRegex.IsMatch(Normalize(plateNumber)!);
        }

        /// <summary>
        /// 验证是否为大型新能源车牌（8位，第8位为D或F）
        /// </summary>
        /// <param name="plateNumber">车牌号</param>
        /// <returns>是否为大型新能源车牌</returns>
        public static bool IsLargeNewEnergyPlate(string? plateNumber)
        {
            if (string.IsNullOrWhiteSpace(plateNumber))
            {
                return false;
            }

            return NewEnergyLargeRegex.IsMatch(Normalize(plateNumber)!);
        }

        /// <summary>
        /// 验证是否为武警车牌
        /// </summary>
        /// <param name="plateNumber">车牌号</param>
        /// <returns>是否为武警车牌</returns>
        public static bool IsWJPlate(string? plateNumber)
        {
            if (string.IsNullOrWhiteSpace(plateNumber))
            {
                return false;
            }

            return WJPlateRegex.IsMatch(Normalize(plateNumber)!);
        }

        /// <summary>
        /// 验证是否为军队车牌
        /// </summary>
        /// <param name="plateNumber">车牌号</param>
        /// <returns>是否为军队车牌</returns>
        public static bool IsMilitaryPlate(string? plateNumber)
        {
            if (string.IsNullOrWhiteSpace(plateNumber))
            {
                return false;
            }

            return MilitaryPlateRegex.IsMatch(Normalize(plateNumber)!);
        }

        #endregion

        #region 类型识别

        /// <summary>
        /// 获取车牌类型
        /// </summary>
        /// <param name="plateNumber">车牌号</param>
        /// <returns>车牌类型</returns>
        public static PlateType GetPlateType(string? plateNumber)
        {
            if (string.IsNullOrWhiteSpace(plateNumber))
            {
                return PlateType.Unknown;
            }

            string normalized = Normalize(plateNumber)!;

            if (IsSmallNewEnergyPlate(normalized))
            {
                return PlateType.NewEnergySmall;
            }

            if (IsLargeNewEnergyPlate(normalized))
            {
                return PlateType.NewEnergyLarge;
            }

            if (IsNormalPlate(normalized))
            {
                return PlateType.Normal;
            }

            if (IsWJPlate(normalized))
            {
                return PlateType.WJ;
            }

            if (IsMilitaryPlate(normalized))
            {
                return PlateType.Military;
            }

            return PlateType.Unknown;
        }

        /// <summary>
        /// 验证是否为燃油车车牌（普通7位车牌，非新能源）
        /// </summary>
        /// <param name="plateNumber">车牌号</param>
        /// <returns>是否为燃油车车牌</returns>
        public static bool IsFuelVehicle(string? plateNumber)
        {
            if (string.IsNullOrWhiteSpace(plateNumber))
            {
                return false;
            }

            string normalized = Normalize(plateNumber)!;

            // 普通车牌（7位）且不是军队/武警车牌 = 燃油车
            return normalized.Length == 7 && IsNormalPlate(normalized);
        }

        /// <summary>
        /// 获取车辆燃料类型
        /// </summary>
        /// <param name="plateNumber">车牌号</param>
        /// <returns>燃料类型</returns>
        public static FuelType GetFuelType(string? plateNumber)
        {
            if (string.IsNullOrWhiteSpace(plateNumber))
            {
                return FuelType.Unknown;
            }

            string normalized = Normalize(plateNumber)!;

            // 燃油车（普通7位车牌）
            if (IsFuelVehicle(normalized))
            {
                return FuelType.Fuel;
            }

            // 新能源车
            if (IsNewEnergyPlate(normalized))
            {
                NewEnergyType? newEnergyType = GetNewEnergyType(normalized);
                return newEnergyType switch
                {
                    NewEnergyType.PureElectric => FuelType.PureElectric,
                    NewEnergyType.PluginHybrid => FuelType.PluginHybrid,
                    _ => FuelType.Unknown
                };
            }

            return FuelType.Unknown;
        }

        /// <summary>
        /// 获取车辆燃料类型名称
        /// </summary>
        /// <param name="plateNumber">车牌号</param>
        /// <returns>燃料类型名称</returns>
        public static string? GetFuelTypeName(string? plateNumber)
        {
            return GetFuelType(plateNumber) switch
            {
                FuelType.Fuel => "燃油车",
                FuelType.PureElectric => "纯电动",
                FuelType.PluginHybrid => "插电混动",
                _ => null
            };
        }

        /// <summary>
        /// 获取新能源车型类型
        /// </summary>
        /// <param name="plateNumber">车牌号</param>
        /// <returns>新能源类型，非新能源车牌返回null</returns>
        public static NewEnergyType? GetNewEnergyType(string? plateNumber)
        {
            if (!IsNewEnergyPlate(plateNumber))
            {
                return null;
            }

            string normalized = Normalize(plateNumber)!;

            // 小型新能源车牌：第3位
            // 大型新能源车牌：第8位
            char typeChar;
            if (normalized.Length == 8)
            {
                if (normalized[2] == 'D' || normalized[2] == 'F')
                {
                    typeChar = normalized[2];
                }
                else
                {
                    typeChar = normalized[7];
                }
            }
            else
            {
                return null;
            }

            // D: 纯电动, F: 插电式混合动力
            return typeChar == 'D' ? NewEnergyType.PureElectric : NewEnergyType.PluginHybrid;
        }

        #endregion

        #region 信息提取

        /// <summary>
        /// 获取省份名称
        /// </summary>
        /// <param name="plateNumber">车牌号</param>
        /// <returns>省份名称</returns>
        public static string? GetProvince(string? plateNumber)
        {
            if (string.IsNullOrWhiteSpace(plateNumber))
            {
                return null;
            }

            string normalized = Normalize(plateNumber)!;

            // 武警车牌特殊处理
            if (IsWJPlate(normalized))
            {
                return "武警";
            }

            // 军队车牌特殊处理
            if (IsMilitaryPlate(normalized))
            {
                return "军队";
            }

            string provinceCode = normalized.Substring(0, 1);
            return ProvinceMap.TryGetValue(provinceCode, out string? province) ? province : null;
        }

        /// <summary>
        /// 获取城市名称
        /// </summary>
        /// <param name="plateNumber">车牌号</param>
        /// <returns>城市名称</returns>
        public static string? GetCity(string? plateNumber)
        {
            if (string.IsNullOrWhiteSpace(plateNumber))
            {
                return null;
            }

            string normalized = Normalize(plateNumber)!;

            // 武警或军队车牌无城市信息
            if (IsWJPlate(normalized) || IsMilitaryPlate(normalized))
            {
                return null;
            }

            string provinceCode = normalized.Substring(0, 1);
            string cityCode = normalized.Substring(1, 1);

            if (CityMap.TryGetValue(provinceCode, out Dictionary<string, string>? cities))
            {
                if (cities.TryGetValue(cityCode, out string? city))
                {
                    return city;
                }
            }

            return null;
        }

        /// <summary>
        /// 获取车牌前缀（省份 + 字母）
        /// </summary>
        /// <param name="plateNumber">车牌号</param>
        /// <returns>车牌前缀</returns>
        public static string? GetPrefix(string? plateNumber)
        {
            if (string.IsNullOrWhiteSpace(plateNumber))
            {
                return null;
            }

            string normalized = Normalize(plateNumber)!;

            if (normalized.Length < 2)
            {
                return null;
            }

            // 普通车牌和新能源车牌：前2位
            // 武警车牌：前4位（WJ+数字）
            if (IsWJPlate(normalized))
            {
                return normalized.Length >= 4 ? normalized.Substring(0, 4) : null;
            }

            return normalized.Substring(0, 2);
        }

        /// <summary>
        /// 获取号码部分（去除前缀）
        /// </summary>
        /// <param name="plateNumber">车牌号</param>
        /// <returns>号码部分</returns>
        public static string? GetNumberPart(string? plateNumber)
        {
            if (string.IsNullOrWhiteSpace(plateNumber))
            {
                return null;
            }

            string normalized = Normalize(plateNumber)!;

            // 普通车牌：后5位
            // 新能源车牌：后6位（小型）/ 后6位（大型）
            // 武警车牌：后5位

            if (IsWJPlate(normalized))
            {
                return normalized.Length >= 7 ? normalized.Substring(4) : null;
            }

            if (normalized.Length == 8)
            {
                // 新能源车牌
                return normalized.Substring(2);
            }

            if (normalized.Length == 7)
            {
                // 普通车牌或军队车牌
                return normalized.Substring(2);
            }

            return null;
        }

        #endregion

        #region 格式化方法

        /// <summary>
        /// 格式化车牌号（转大写，去除特殊字符）
        /// </summary>
        /// <param name="plateNumber">车牌号</param>
        /// <returns>格式化后的车牌号</returns>
        public static string? Normalize(string? plateNumber)
        {
            if (string.IsNullOrWhiteSpace(plateNumber))
            {
                return null;
            }

            // 去除空格和特殊字符，转大写
            string normalized = plateNumber.ToUpper().Trim();

            // 保留汉字、字母、数字
            normalized = NonChineseAlphanumericRegex.Replace(normalized, "");

            return normalized;
        }

        /// <summary>
        /// 格式化车牌号（带分隔符）
        /// </summary>
        /// <param name="plateNumber">车牌号</param>
        /// <param name="separator">分隔符（默认为空格）</param>
        /// <returns>格式化后的车牌号</returns>
        public static string? Format(string? plateNumber, string separator = " ")
        {
            string? normalized = Normalize(plateNumber);
            if (normalized == null)
            {
                return null;
            }

            // 武警车牌特殊处理
            if (IsWJPlate(normalized))
            {
                if (normalized.Length == 7)
                {
                    return normalized.Substring(0, 2) + separator + normalized.Substring(2, 2) + separator + normalized.Substring(4);
                }
                return normalized;
            }

            // 普通车牌：2+5
            // 新能源车牌：2+6
            if (normalized.Length == 7)
            {
                return normalized.Substring(0, 2) + separator + normalized.Substring(2);
            }

            if (normalized.Length == 8)
            {
                return normalized.Substring(0, 2) + separator + normalized.Substring(2);
            }

            return normalized;
        }

        /// <summary>
        /// 车牌号脱敏：粤***123
        /// </summary>
        /// <param name="plateNumber">车牌号</param>
        /// <returns>脱敏后的车牌号</returns>
        public static string? Mask(string? plateNumber)
        {
            string? normalized = Normalize(plateNumber);
            if (normalized == null)
            {
                return null;
            }

            // 武警车牌特殊处理
            if (IsWJPlate(normalized))
            {
                if (normalized.Length >= 7)
                {
                    return normalized.Substring(0, 4) + "***" + normalized.Substring(normalized.Length - 2);
                }
                return null;
            }

            if (normalized.Length == 7)
            {
                // 普通车牌：保留省份 + 后2位
                return normalized.Substring(0, 1) + "***" + normalized.Substring(5);
            }

            if (normalized.Length == 8)
            {
                // 新能源车牌：保留省份 + 后2位
                return normalized.Substring(0, 1) + "****" + normalized.Substring(6);
            }

            return null;
        }

        #endregion

        #region 生成方法

        /// <summary>
        /// 生成随机车牌号（仅供测试使用）
        /// </summary>
        /// <param name="province">省份简称（可选，默认随机）</param>
        /// <param name="isNewEnergy">是否为新能源车牌（可选，默认随机）</param>
        /// <returns>车牌号</returns>
        public static string GenerateRandom(string? province = null, bool? isNewEnergy = null)
        {
            // 省份
            string[] provinces = { "京", "津", "冀", "晋", "蒙", "辽", "吉", "黑", "沪", "苏", "浙", "皖", "闽", "赣", "鲁", "豫", "鄂", "湘", "粤", "桂", "琼", "川", "贵", "云", "藏", "陕", "甘", "青", "宁", "新", "渝" };
            string prov = province ?? MathCategory.RandomUtil.GetRandomElement(provinces);

            // 字母
            const string letters = "ABCDEFGHJKLMNPQRSTUVWXYZ"; // 不包含I和O
            string letter = MathCategory.RandomUtil.GetRandomElement(letters.ToCharArray()).ToString();

            bool newEnergy = isNewEnergy ?? MathCategory.RandomUtil.RandomBool();

            if (newEnergy)
            {
                // 新能源车牌（8位）
                char energyType = MathCategory.RandomUtil.RandomBool() ? 'D' : 'F';
                string numbers = GenerateRandomAlphanumeric(5);
                return prov + letter + energyType + numbers;
            }
            else
            {
                // 普通车牌（7位）
                string numbers = GenerateRandomAlphanumeric(5);
                return prov + letter + numbers;
            }
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 生成随机字母数字组合
        /// </summary>
        private static string GenerateRandomAlphanumeric(int length)
        {
            const string chars = "0123456789ABCDEFGHJKLMNPQRSTUVWXYZ"; // 不包含I和O
            string result = "";
            for (int i = 0; i < length; i++)
            {
                result += MathCategory.RandomUtil.GetRandomElement(chars.ToCharArray());
            }
            return result;
        }

        #endregion
    }
}
