using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace EasyTool.BusinessCategory
{
    /// <summary>
    /// 车牌号工具类
    /// 提供车牌号验证、归属地查询功能
    /// </summary>
    public static class PlateNumberUtil
    {
        #region 数据结构

        /// <summary>
        /// 车牌信息
        /// </summary>
        public class PlateInfo
        {
            /// <summary>
            /// 车牌号
            /// </summary>
            public string PlateNumber { get; set; } = string.Empty;

            /// <summary>
            /// 车牌类型
            /// </summary>
            public PlateType Type { get; set; }

            /// <summary>
            /// 省份
            /// </summary>
            public string Province { get; set; } = string.Empty;

            /// <summary>
            /// 城市
            /// </summary>
            public string City { get; set; } = string.Empty;

            /// <summary>
            /// 是否新能源车牌
            /// </summary>
            public bool IsNewEnergy { get; set; }
        }

        /// <summary>
        /// 车牌类型
        /// </summary>
        public enum PlateType
        {
            /// <summary>
            /// 普通民用车牌
            /// </summary>
            Normal = 1,

            /// <summary>
            /// 新能源车牌
            /// </summary>
            NewEnergy = 2,

            /// <summary>
            /// 警用车牌
            /// </summary>
            Police = 3,

            /// <summary>
            /// 军用车牌
            /// </summary>
            Military = 4,

            /// <summary>
            /// 使馆车牌
            /// </summary>
            Embassy = 5,

            /// <summary>
            /// 武警车牌
            /// </summary>
            ArmedPolice = 6,

            /// <summary>
            /// 港澳车牌
            /// </summary>
            HongKongMacau = 7
        }

        #endregion

        #region 静态数据

        // 车牌省份简称映射
        private static readonly Dictionary<char, string> ProvinceMapping = new()
        {
            {'京', "北京"}, {'津', "天津"}, {'沪', "上海"}, {'渝', "重庆"},
            {'冀', "河北"}, {'晋', "山西"}, {'辽', "辽宁"}, {'吉', "吉林"},
            {'黑', "黑龙江"}, {'苏', "江苏"}, {'浙', "浙江"}, {'皖', "安徽"},
            {'闽', "福建"}, {'赣', "江西"}, {'鲁', "山东"}, {'豫', "河南"},
            {'鄂', "湖北"}, {'湘', "湖南"}, {'粤', "广东"}, {'桂', "广西"},
            {'琼', "海南"}, {'川', "四川"}, {'蜀', "四川"}, {'贵', "贵州"},
            {'黔', "贵州"}, {'云', "云南"}, {'滇', "云南"}, {'藏', "西藏"},
            {'陕', "陕西"}, {'秦', "陕西"}, {'甘', "甘肃"}, {'陇', "甘肃"},
            {'青', "青海"}, {'宁', "宁夏"}, {'新', "新疆"}, {'蒙', "内蒙古"}
        };

        // 车牌字母对应城市（主要城市）
        private static readonly Dictionary<string, Dictionary<char, string>> CityMapping = new()
        {
            ["京"] = new Dictionary<char, string>
            {
                {'A', "市区"}, {'B', "出租车"}, {'C', "市区"}, {'D', "市区"},
                {'E', "市区"}, {'F', "市区"}, {'G', "市区"}, {'H', "市区"},
                {'J', "市区"}, {'K', "市区"}, {'L', "市区"}, {'M', "市区"},
                {'N', "市区"}, {'P', "市区"}, {'Q', "市区"}, {'Y', "延庆"}
            },
            ["沪"] = new Dictionary<char, string>
            {
                {'A', "市区"}, {'B', "市区"}, {'C', "市区"}, {'D', "市区"},
                {'E', "市区"}, {'F', "市区"}, {'G', "市区"}, {'H', "市区"},
                {'J', "市区"}, {'K', "市区"}, {'L', "市区"}, {'M', "市区"},
                {'N', "市区"}, {'R', "崇明"}
            },
            ["粤"] = new Dictionary<char, string>
            {
                {'A', "广州"}, {'B', "深圳"}, {'C', "珠海"}, {'D', "汕头"},
                {'E', "佛山"}, {'F', "韶关"}, {'G', "湛江"}, {'H', "肇庆"},
                {'J', "江门"}, {'K', "茂名"}, {'L', "惠州"}, {'M', "梅州"},
                {'N', "汕尾"}, {'P', "河源"}, {'Q', "阳江"}, {'R', "清远"},
                {'S', "东莞"}, {'T', "中山"}, {'U', "潮州"}, {'V', "揭阳"},
                {'W', "云浮"}, {'X', "顺德"}, {'Y', "南海"}, {'Z', "港澳入境"}
            },
            ["浙"] = new Dictionary<char, string>
            {
                {'A', "杭州"}, {'B', "宁波"}, {'C', "温州"}, {'D', "绍兴"},
                {'E', "湖州"}, {'F', "嘉兴"}, {'G', "金华"}, {'H', "衢州"},
                {'J', "台州"}, {'K', "丽水"}, {'L', "舟山"}
            },
            ["苏"] = new Dictionary<char, string>
            {
                {'A', "南京"}, {'B', "无锡"}, {'C', "徐州"}, {'D', "常州"},
                {'E', "苏州"}, {'F', "南通"}, {'G', "连云港"}, {'H', "淮安"},
                {'J', "盐城"}, {'K', "扬州"}, {'L', "镇江"}, {'M', "泰州"},
                {'N', "宿迁"}
            },
            ["鲁"] = new Dictionary<char, string>
            {
                {'A', "济南"}, {'B', "青岛"}, {'C', "淄博"}, {'D', "枣庄"},
                {'E', "东营"}, {'F', "烟台"}, {'G', "潍坊"}, {'H', "济宁"},
                {'J', "泰安"}, {'K', "威海"}, {'L', "日照"}, {'M', "滨州"},
                {'N', "德州"}, {'P', "聊城"}, {'Q', "临沂"}, {'R', "菏泽"},
                {'S', "莱芜"}, {'U', "青岛增补"}, {'V', "潍坊增补"}, {'W', "青岛增补"}
            },
            ["川"] = new Dictionary<char, string>
            {
                {'A', "成都"}, {'B', "绵阳"}, {'C', "自贡"}, {'D', "攀枝花"},
                {'E', "泸州"}, {'F', "德阳"}, {'H', "广元"}, {'J', "遂宁"},
                {'K', "内江"}, {'L', "乐山"}, {'M', "南充"}, {'N', "眉山"},
                {'P', "广安"}, {'Q', "达州"}, {'R', "雅安"}, {'S', "巴中"},
                {'T', "资阳"}, {'U', "阿坝"}, {'V', "甘孜"}, {'W', "凉山"}
            },
            ["鄂"] = new Dictionary<char, string>
            {
                {'A', "武汉"}, {'B', "黄石"}, {'C', "十堰"}, {'D', "荆州"},
                {'E', "宜昌"}, {'F', "襄阳"}, {'G', "鄂州"}, {'H', "荆门"},
                {'J', "孝感"}, {'K', "黄冈"}, {'L', "咸宁"}, {'M', "仙桃"},
                {'N', "潜江"}, {'P', "神农架"}, {'Q', "恩施"}, {'R', "天门"},
                {'S', "随州"}
            },
            ["湘"] = new Dictionary<char, string>
            {
                {'A', "长沙"}, {'B', "株洲"}, {'C', "湘潭"}, {'D', "衡阳"},
                {'E', "邵阳"}, {'F', "岳阳"}, {'G', "张家界"}, {'H', "益阳"},
                {'J', "常德"}, {'K', "娄底"}, {'L', "郴州"}, {'M', "永州"},
                {'N', "怀化"}, {'U', "湘西"}
            },
            ["豫"] = new Dictionary<char, string>
            {
                {'A', "郑州"}, {'B', "开封"}, {'C', "洛阳"}, {'D', "平顶山"},
                {'E', "安阳"}, {'F', "鹤壁"}, {'G', "新乡"}, {'H', "焦作"},
                {'J', "濮阳"}, {'K', "许昌"}, {'L', "漯河"}, {'M', "三门峡"},
                {'N', "商丘"}, {'P', "周口"}, {'Q', "驻马店"}, {'R', "南阳"},
                {'S', "信阳"}, {'U', "济源"}
            },
            ["冀"] = new Dictionary<char, string>
            {
                {'A', "石家庄"}, {'B', "唐山"}, {'C', "秦皇岛"}, {'D', "邯郸"},
                {'E', "邢台"}, {'F', "保定"}, {'G', "张家口"}, {'H', "承德"},
                {'J', "沧州"}, {'K', "廊坊"}, {'L', "衡水"}, {'R', "秦皇岛增补"}
            },
            ["陕"] = new Dictionary<char, string>
            {
                {'A', "西安"}, {'B', "铜川"}, {'C', "宝鸡"}, {'D', "咸阳"},
                {'E', "渭南"}, {'F', "延安"}, {'G', "汉中"}, {'H', "榆林"},
                {'J', "安康"}, {'K', "商洛"}, {'V', "杨凌"}
            },
            ["闽"] = new Dictionary<char, string>
            {
                {'A', "福州"}, {'B', "莆田"}, {'C', "泉州"}, {'D', "厦门"},
                {'E', "漳州"}, {'F', "龙岩"}, {'G', "三明"}, {'H', "南平"},
                {'J', "宁德"}, {'K', "平潭"}
            },
            ["辽"] = new Dictionary<char, string>
            {
                {'A', "沈阳"}, {'B', "大连"}, {'C', "鞍山"}, {'D', "抚顺"},
                {'E', "本溪"}, {'F', "丹东"}, {'G', "锦州"}, {'H', "营口"},
                {'J', "阜新"}, {'K', "辽阳"}, {'L', "盘锦"}, {'M', "铁岭"},
                {'N', "朝阳"}, {'P', "葫芦岛"}
            },
            ["皖"] = new Dictionary<char, string>
            {
                {'A', "合肥"}, {'B', "芜湖"}, {'C', "蚌埠"}, {'D', "淮南"},
                {'E', "马鞍山"}, {'F', "淮北"}, {'G', "铜陵"}, {'H', "安庆"},
                {'J', "黄山"}, {'K', "阜阳"}, {'L', "宿州"}, {'M', "滁州"},
                {'N', "六安"}, {'P', "亳州"}, {'Q', "池州"}, {'R', "宣城"}
            },
            ["赣"] = new Dictionary<char, string>
            {
                {'A', "南昌"}, {'B', "赣州"}, {'C', "宜春"}, {'D', "吉安"},
                {'E', "上饶"}, {'F', "抚州"}, {'G', "九江"}, {'H', "景德镇"},
                {'J', "萍乡"}, {'K', "新余"}, {'L', "鹰潭"}
            },
            ["黑"] = new Dictionary<char, string>
            {
                {'A', "哈尔滨"}, {'B', "齐齐哈尔"}, {'C', "牡丹江"}, {'D', "佳木斯"},
                {'E', "大庆"}, {'F', "伊春"}, {'G', "鸡西"}, {'H', "鹤岗"},
                {'J', "双鸭山"}, {'K', "七台河"}, {'L', "松花江"}, {'M', "绥化"},
                {'N', "黑河"}, {'P', "大兴安岭"}, {'R', "农垦"}
            },
            ["吉"] = new Dictionary<char, string>
            {
                {'A', "长春"}, {'B', "吉林"}, {'C', "四平"}, {'D', "辽源"},
                {'E', "通化"}, {'F', "白山"}, {'G', "白城"}, {'H', "延边"},
                {'J', "松原"}
            },
            ["云"] = new Dictionary<char, string>
            {
                {'A', "昆明"}, {'B', "东川"}, {'C', "昭通"}, {'D', "曲靖"},
                {'E', "楚雄"}, {'F', "玉溪"}, {'G', "红河"}, {'H', "文山"},
                {'J', "普洱"}, {'K', "西双版纳"}, {'L', "大理"}, {'M', "保山"},
                {'N', "德宏"}, {'P', "丽江"}, {'Q', "怒江"}, {'R', "迪庆"},
                {'S', "临沧"}
            },
            ["贵"] = new Dictionary<char, string>
            {
                {'A', "贵阳"}, {'B', "六盘水"}, {'C', "遵义"}, {'D', "铜仁"},
                {'E', "黔西南"}, {'F', "毕节"}, {'G', "安顺"}, {'H', "黔东南"},
                {'J', "黔南"}
            },
            ["琼"] = new Dictionary<char, string>
            {
                {'A', "海口"}, {'B', "三亚"}, {'C', "琼海"}, {'D', "五指山"},
                {'E', "洋浦"}, {'F', "儋州"}
            },
            ["甘"] = new Dictionary<char, string>
            {
                {'A', "兰州"}, {'B', "嘉峪关"}, {'C', "金昌"}, {'D', "白银"},
                {'E', "天水"}, {'F', "酒泉"}, {'G', "张掖"}, {'H', "武威"},
                {'J', "定西"}, {'K', "陇南"}, {'L', "平凉"}, {'M', "庆阳"},
                {'N', "临夏"}, {'P', "甘南"}
            },
            ["青"] = new Dictionary<char, string>
            {
                {'A', "西宁"}, {'B', "海东"}, {'C', "海北"}, {'D', "黄南"},
                {'E', "海南"}, {'F', "果洛"}, {'G', "玉树"}, {'H', "海西"}
            },
            ["蒙"] = new Dictionary<char, string>
            {
                {'A', "呼和浩特"}, {'B', "包头"}, {'C', "乌海"}, {'D', "赤峰"},
                {'E', "呼伦贝尔"}, {'F', "兴安盟"}, {'G', "通辽"}, {'H', "锡林郭勒"},
                {'J', "乌兰察布"}, {'K', "鄂尔多斯"}, {'L', "巴彦淖尔"}, {'M', "阿拉善"}
            },
            ["桂"] = new Dictionary<char, string>
            {
                {'A', "南宁"}, {'B', "柳州"}, {'C', "桂林"}, {'D', "梧州"},
                {'E', "北海"}, {'F', "钦州"}, {'G', "贵港"}, {'H', "玉林"},
                {'J', "百色"}, {'K', "贺州"}, {'L', "河池"}, {'M', "来宾"},
                {'N', "崇左"}, {'P', "桂林增补"}, {'R', "柳州增补"}
            },
            ["宁"] = new Dictionary<char, string>
            {
                {'A', "银川"}, {'B', "石嘴山"}, {'C', "吴忠"}, {'D', "固原"},
                {'E', "中卫"}
            },
            ["新"] = new Dictionary<char, string>
            {
                {'A', "乌鲁木齐"}, {'B', "昌吉"}, {'C', "石河子"}, {'D', "奎屯"},
                {'E', "博尔塔拉"}, {'F', "伊犁"}, {'G', "塔城"}, {'H', "阿勒泰"},
                {'J', "克拉玛依"}, {'K', "吐鲁番"}, {'L', "哈密"}, {'M', "巴音郭楞"},
                {'N', "阿克苏"}, {'P', "克孜勒苏"}, {'Q', "喀什"}, {'R', "和田"}
            },
            ["藏"] = new Dictionary<char, string>
            {
                {'A', "拉萨"}, {'B', "昌都"}, {'C', "山南"}, {'D', "日喀则"},
                {'E', "那曲"}, {'F', "阿里"}, {'G', "林芝"}, {'H', "西藏驻成都"},
                {'J', "西藏驻格尔木"}
            }
        };

        // 普通车牌正则
        private static readonly Regex NormalPlateRegex = new(@"^[京津沪渝冀豫云辽黑湘皖鲁新苏浙赣鄂桂甘晋蒙陕吉闽贵粤青藏川宁琼使领A-Z][A-Z][A-HJ-NP-Z0-9]{4}[A-HJ-NP-Z0-9挂学警港澳]$", RegexOptions.Compiled);

        // 新能源车牌正则（小型和大型）
        private static readonly Regex NewEnergyPlateRegex = new(@"^[京津沪渝冀豫云辽黑湘皖鲁新苏浙赣鄂桂甘晋蒙陕吉闽贵粤青藏川宁琼使领A-Z][A-Z](([0-9]{5}[DF])|([DF][A-HJ-NP-Z0-9][0-9]{4}))$", RegexOptions.Compiled);

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
                return false;

            plateNumber = plateNumber.ToUpper().Trim();

            return NormalPlateRegex.IsMatch(plateNumber) || NewEnergyPlateRegex.IsMatch(plateNumber);
        }

        /// <summary>
        /// 判断是否为新能源车牌
        /// </summary>
        /// <param name="plateNumber">车牌号</param>
        /// <returns>是否为新能源车牌</returns>
        public static bool IsNewEnergy(string? plateNumber)
        {
            if (string.IsNullOrWhiteSpace(plateNumber))
                return false;

            plateNumber = plateNumber.ToUpper().Trim();
            return NewEnergyPlateRegex.IsMatch(plateNumber);
        }

        /// <summary>
        /// 判断是否为普通车牌
        /// </summary>
        /// <param name="plateNumber">车牌号</param>
        /// <returns>是否为普通车牌</returns>
        public static bool IsNormalPlate(string? plateNumber)
        {
            if (string.IsNullOrWhiteSpace(plateNumber))
                return false;

            plateNumber = plateNumber.ToUpper().Trim();
            return NormalPlateRegex.IsMatch(plateNumber);
        }

        #endregion

        #region 信息获取

        /// <summary>
        /// 获取车牌信息
        /// </summary>
        /// <param name="plateNumber">车牌号</param>
        /// <returns>车牌信息</returns>
        public static PlateInfo? GetPlateInfo(string? plateNumber)
        {
            if (!IsValid(plateNumber))
                return null;

            plateNumber = plateNumber!.ToUpper().Trim();

            var info = new PlateInfo
            {
                PlateNumber = plateNumber,
                IsNewEnergy = IsNewEnergy(plateNumber)
            };

            // 获取省份简称
            var provinceChar = plateNumber[0];
            if (ProvinceMapping.TryGetValue(provinceChar, out var province))
            {
                info.Province = province;
            }

            // 获取城市
            var cityCode = provinceChar.ToString();
            var letterChar = plateNumber[1];
            if (CityMapping.TryGetValue(cityCode, out var cities))
            {
                if (cities.TryGetValue(letterChar, out var city))
                {
                    info.City = city;
                }
            }

            // 判断车牌类型
            if (info.IsNewEnergy)
            {
                info.Type = PlateType.NewEnergy;
            }
            else if (plateNumber.Contains("警"))
            {
                info.Type = PlateType.Police;
            }
            else if (plateNumber.StartsWith("使"))
            {
                info.Type = PlateType.Embassy;
            }
            else if (plateNumber.StartsWith("领"))
            {
                info.Type = PlateType.Embassy;
            }
            else if (plateNumber.StartsWith("WJ"))
            {
                info.Type = PlateType.ArmedPolice;
            }
            else if (plateNumber.EndsWith("港") || plateNumber.EndsWith("澳"))
            {
                info.Type = PlateType.HongKongMacau;
            }
            else
            {
                info.Type = PlateType.Normal;
            }

            return info;
        }

        /// <summary>
        /// 获取省份
        /// </summary>
        /// <param name="plateNumber">车牌号</param>
        /// <returns>省份名称</returns>
        public static string? GetProvince(string? plateNumber)
        {
            if (string.IsNullOrWhiteSpace(plateNumber))
                return null;

            var provinceChar = plateNumber.ToUpper()[0];
            return ProvinceMapping.TryGetValue(provinceChar, out var province) ? province : null;
        }

        /// <summary>
        /// 获取城市
        /// </summary>
        /// <param name="plateNumber">车牌号</param>
        /// <returns>城市名称</returns>
        public static string? GetCity(string? plateNumber)
        {
            var info = GetPlateInfo(plateNumber);
            return info?.City;
        }

        /// <summary>
        /// 获取归属地（省份+城市）
        /// </summary>
        /// <param name="plateNumber">车牌号</param>
        /// <returns>归属地</returns>
        public static string? GetLocation(string? plateNumber)
        {
            var info = GetPlateInfo(plateNumber);
            if (info == null)
                return null;

            if (string.IsNullOrEmpty(info.City) || info.City == info.Province)
                return info.Province;

            return $"{info.Province}{info.City}";
        }

        #endregion

        #region 格式化

        /// <summary>
        /// 格式化车牌号（添加空格或分隔符）
        /// </summary>
        /// <param name="plateNumber">车牌号</param>
        /// <param name="separator">分隔符（默认空格）</param>
        /// <returns>格式化后的车牌号</returns>
        public static string? Format(string? plateNumber, string separator = " ")
        {
            if (!IsValid(plateNumber))
                return null;

            plateNumber = plateNumber!.ToUpper().Trim();

            if (plateNumber.Length == 7)
            {
                // 普通车牌：京A12345
                return plateNumber.Insert(2, separator);
            }
            else if (plateNumber.Length == 8)
            {
                // 新能源车牌：京AD12345
                return plateNumber.Insert(2, separator);
            }

            return plateNumber;
        }

        /// <summary>
        /// 车牌号脱敏
        /// </summary>
        /// <param name="plateNumber">车牌号</param>
        /// <returns>脱敏后的车牌号</returns>
        public static string? Mask(string? plateNumber)
        {
            if (!IsValid(plateNumber))
                return null;

            plateNumber = plateNumber!.ToUpper().Trim();

            if (plateNumber.Length == 7)
            {
                // 京A****5
                return plateNumber.Substring(0, 2) + "****" + plateNumber.Substring(6, 1);
            }
            else if (plateNumber.Length == 8)
            {
                // 京AD****5
                return plateNumber.Substring(0, 2) + "****" + plateNumber.Substring(7, 1);
            }

            return plateNumber;
        }

        #endregion
    }
}