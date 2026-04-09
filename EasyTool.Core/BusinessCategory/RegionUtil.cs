using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyTool.BusinessCategory
{
    /// <summary>
    /// 行政区划工具类
    /// 提供中国省市区三级联动查询功能
    /// </summary>
    public static class RegionUtil
    {
        #region 数据结构

        /// <summary>
        /// 行政区划信息
        /// </summary>
        public class RegionInfo
        {
            /// <summary>
            /// 行政区划代码（6位）
            /// </summary>
            public string Code { get; set; } = string.Empty;

            /// <summary>
            /// 名称
            /// </summary>
            public string Name { get; set; } = string.Empty;

            /// <summary>
            /// 简称
            /// </summary>
            public string ShortName { get; set; } = string.Empty;

            /// <summary>
            /// 上级代码
            /// </summary>
            public string ParentCode { get; set; } = string.Empty;

            /// <summary>
            /// 级别（1省 2市 3区县）
            /// </summary>
            public int Level { get; set; }

            /// <summary>
            /// 拼音
            /// </summary>
            public string Pinyin { get; set; } = string.Empty;

            /// <summary>
            /// 邮编
            /// </summary>
            public string ZipCode { get; set; } = string.Empty;
        }

        #endregion

        #region 静态数据

        private static readonly Dictionary<string, RegionInfo> Regions = new();
        private static readonly List<RegionInfo> Provinces = new();
        private static bool _initialized = false;
        private static readonly object _lock = new();

        #endregion

        #region 初始化

        static RegionUtil()
        {
            InitData();
        }

        private static void InitData()
        {
            lock (_lock)
            {
                if (_initialized)
                    return;

                // 省份数据
                var provinceData = new[]
                {
                    ("110000", "北京市", "北京"),
                    ("120000", "天津市", "天津"),
                    ("130000", "河北省", "河北"),
                    ("140000", "山西省", "山西"),
                    ("150000", "内蒙古自治区", "内蒙古"),
                    ("210000", "辽宁省", "辽宁"),
                    ("220000", "吉林省", "吉林"),
                    ("230000", "黑龙江省", "黑龙江"),
                    ("310000", "上海市", "上海"),
                    ("320000", "江苏省", "江苏"),
                    ("330000", "浙江省", "浙江"),
                    ("340000", "安徽省", "安徽"),
                    ("350000", "福建省", "福建"),
                    ("360000", "江西省", "江西"),
                    ("370000", "山东省", "山东"),
                    ("410000", "河南省", "河南"),
                    ("420000", "湖北省", "湖北"),
                    ("430000", "湖南省", "湖南"),
                    ("440000", "广东省", "广东"),
                    ("450000", "广西壮族自治区", "广西"),
                    ("460000", "海南省", "海南"),
                    ("500000", "重庆市", "重庆"),
                    ("510000", "四川省", "四川"),
                    ("520000", "贵州省", "贵州"),
                    ("530000", "云南省", "云南"),
                    ("540000", "西藏自治区", "西藏"),
                    ("610000", "陕西省", "陕西"),
                    ("620000", "甘肃省", "甘肃"),
                    ("630000", "青海省", "青海"),
                    ("640000", "宁夏回族自治区", "宁夏"),
                    ("650000", "新疆维吾尔自治区", "新疆"),
                    ("710000", "台湾省", "台湾"),
                    ("810000", "香港特别行政区", "香港"),
                    ("820000", "澳门特别行政区", "澳门")
                };

                foreach (var (code, name, shortName) in provinceData)
                {
                    var info = new RegionInfo
                    {
                        Code = code,
                        Name = name,
                        ShortName = shortName,
                        ParentCode = "",
                        Level = 1
                    };
                    Regions[code] = info;
                    Provinces.Add(info);
                }

                // 主要城市数据
                var cityData = new[]
                {
                    ("110100", "北京市", "北京", "110000"),
                    ("310100", "上海市", "上海", "310000"),
                    ("120100", "天津市", "天津", "120000"),
                    ("500100", "重庆市", "重庆", "500000"),
                    ("440100", "广州市", "广州", "440000"),
                    ("440300", "深圳市", "深圳", "440000"),
                    ("440600", "佛山市", "佛山", "440000"),
                    ("441900", "东莞市", "东莞", "440000"),
                    ("442000", "中山市", "中山", "440000"),
                    ("330100", "杭州市", "杭州", "330000"),
                    ("330200", "宁波市", "宁波", "330000"),
                    ("320100", "南京市", "南京", "320000"),
                    ("320500", "苏州市", "苏州", "320000"),
                    ("320200", "无锡市", "无锡", "320000"),
                    ("510100", "成都市", "成都", "510000"),
                    ("420100", "武汉市", "武汉", "420000"),
                    ("430100", "长沙市", "长沙", "430000"),
                    ("610100", "西安市", "西安", "610000"),
                    ("410100", "郑州市", "郑州", "410000"),
                    ("370100", "济南市", "济南", "370000"),
                    ("370200", "青岛市", "青岛", "370000"),
                    ("350100", "福州市", "福州", "350000"),
                    ("350200", "厦门市", "厦门", "350000"),
                    ("340100", "合肥市", "合肥", "340000"),
                    ("210100", "沈阳市", "沈阳", "210000"),
                    ("210200", "大连市", "大连", "210000"),
                    ("220100", "长春市", "长春", "220000"),
                    ("230100", "哈尔滨市", "哈尔滨", "230000"),
                    ("130100", "石家庄市", "石家庄", "130000"),
                    ("140100", "太原市", "太原", "140000"),
                    ("360100", "南昌市", "南昌", "360000"),
                    ("530100", "昆明市", "昆明", "530000"),
                    ("520100", "贵阳市", "贵阳", "520000"),
                    ("450100", "南宁市", "南宁", "450000"),
                    ("460100", "海口市", "海口", "460000"),
                    ("620100", "兰州市", "兰州", "620000"),
                    ("630100", "西宁市", "西宁", "630000"),
                    ("150100", "呼和浩特市", "呼和浩特", "150000"),
                    ("640100", "银川市", "银川", "640000"),
                    ("650100", "乌鲁木齐市", "乌鲁木齐", "650000"),
                    ("540100", "拉萨市", "拉萨", "540000")
                };

                foreach (var (code, name, shortName, parentCode) in cityData)
                {
                    Regions[code] = new RegionInfo
                    {
                        Code = code,
                        Name = name,
                        ShortName = shortName,
                        ParentCode = parentCode,
                        Level = 2
                    };
                }

                // 主要区县数据
                var districtData = new[]
                {
                    ("440103", "荔湾区", "荔湾", "440100"),
                    ("440104", "越秀区", "越秀", "440100"),
                    ("440105", "海珠区", "海珠", "440100"),
                    ("440106", "天河区", "天河", "440100"),
                    ("440111", "白云区", "白云", "440100"),
                    ("440112", "黄埔区", "黄埔", "440100"),
                    ("440113", "番禺区", "番禺", "440100"),
                    ("440114", "花都区", "花都", "440100"),
                    ("440303", "罗湖区", "罗湖", "440300"),
                    ("440304", "福田区", "福田", "440300"),
                    ("440305", "南山区", "南山", "440300"),
                    ("440306", "宝安区", "宝安", "440300"),
                    ("440307", "龙岗区", "龙岗", "440300"),
                    ("440308", "盐田区", "盐田", "440300"),
                    ("440309", "龙华区", "龙华", "440300"),
                    ("440310", "坪山区", "坪山", "440300"),
                    ("110101", "东城区", "东城", "110100"),
                    ("110102", "西城区", "西城", "110100"),
                    ("110105", "朝阳区", "朝阳", "110100"),
                    ("110106", "丰台区", "丰台", "110100"),
                    ("110107", "石景山区", "石景山", "110100"),
                    ("110108", "海淀区", "海淀", "110100"),
                    ("310101", "黄浦区", "黄浦", "310100"),
                    ("310104", "徐汇区", "徐汇", "310100"),
                    ("310105", "长宁区", "长宁", "310100"),
                    ("310106", "静安区", "静安", "310100"),
                    ("310107", "普陀区", "普陀", "310100"),
                    ("310109", "虹口区", "虹口", "310100"),
                    ("310110", "杨浦区", "杨浦", "310100"),
                    ("310112", "闵行区", "闵行", "310100"),
                    ("310113", "宝山区", "宝山", "310100"),
                    ("310114", "嘉定区", "嘉定", "310100"),
                    ("310115", "浦东新区", "浦东", "310100")
                };

                foreach (var (code, name, shortName, parentCode) in districtData)
                {
                    Regions[code] = new RegionInfo
                    {
                        Code = code,
                        Name = name,
                        ShortName = shortName,
                        ParentCode = parentCode,
                        Level = 3
                    };
                }

                _initialized = true;
            }
        }

        #endregion

        #region 查询方法

        /// <summary>
        /// 获取所有省份
        /// </summary>
        /// <returns>省份列表</returns>
        public static List<RegionInfo> GetProvinces()
        {
            return Provinces.ToList();
        }

        /// <summary>
        /// 根据省份代码获取城市列表
        /// </summary>
        /// <param name="provinceCode">省份代码（如：440000）</param>
        /// <returns>城市列表</returns>
        public static List<RegionInfo> GetCities(string provinceCode)
        {
            return Regions.Values
                .Where(r => r.Level == 2 && r.ParentCode == provinceCode)
                .OrderBy(r => r.Code)
                .ToList();
        }

        /// <summary>
        /// 根据省份名称获取城市列表
        /// </summary>
        /// <param name="provinceName">省份名称</param>
        /// <returns>城市列表</returns>
        public static List<RegionInfo> GetCitiesByName(string provinceName)
        {
            var province = Provinces.FirstOrDefault(p =>
                p.Name == provinceName || p.ShortName == provinceName);
            return province != null ? GetCities(province.Code) : new List<RegionInfo>();
        }

        /// <summary>
        /// 根据城市代码获取区县列表
        /// </summary>
        /// <param name="cityCode">城市代码（如：440100）</param>
        /// <returns>区县列表</returns>
        public static List<RegionInfo> GetDistricts(string cityCode)
        {
            return Regions.Values
                .Where(r => r.Level == 3 && r.ParentCode == cityCode)
                .OrderBy(r => r.Code)
                .ToList();
        }

        /// <summary>
        /// 根据行政区划代码获取信息
        /// </summary>
        /// <param name="code">行政区划代码（6位）</param>
        /// <returns>行政区划信息</returns>
        public static RegionInfo? GetByCode(string code)
        {
            if (string.IsNullOrEmpty(code) || code.Length < 6)
                return null;

            code = code.PadRight(6, '0');

            if (Regions.TryGetValue(code, out var info))
                return info;

            var provinceCode = code.Substring(0, 2) + "0000";
            if (Regions.TryGetValue(provinceCode, out var province))
                return province;

            return null;
        }

        /// <summary>
        /// 根据名称搜索行政区划
        /// </summary>
        /// <param name="name">名称（支持模糊搜索）</param>
        /// <param name="level">级别过滤（可选）</param>
        /// <returns>匹配的行政区划列表</returns>
        public static List<RegionInfo> Search(string name, int? level = null)
        {
            var query = Regions.Values.AsEnumerable();

            if (level.HasValue)
                query = query.Where(r => r.Level == level.Value);

            return query
                .Where(r => r.Name.Contains(name) || r.ShortName.Contains(name))
                .OrderBy(r => r.Level)
                .ThenBy(r => r.Code)
                .ToList();
        }

        /// <summary>
        /// 获取完整的行政区划路径
        /// </summary>
        /// <param name="code">行政区划代码</param>
        /// <returns>行政区划路径（省-市-区县）</returns>
        public static string GetFullPath(string code)
        {
            var info = GetByCode(code);
            if (info == null)
                return string.Empty;

            var parts = new List<string> { info.ShortName };

            var current = info;
            while (!string.IsNullOrEmpty(current.ParentCode))
            {
                if (Regions.TryGetValue(current.ParentCode, out var parent))
                {
                    parts.Insert(0, parent.ShortName);
                    current = parent;
                }
                else
                {
                    break;
                }
            }

            return string.Join("-", parts);
        }

        /// <summary>
        /// 获取行政区划层级信息
        /// </summary>
        /// <param name="code">行政区划代码</param>
        /// <returns>省市区信息元组</returns>
        public static (string? Province, string? City, string? District) GetHierarchy(string code)
        {
            if (string.IsNullOrEmpty(code) || code.Length < 6)
                return (null, null, null);

            var info = GetByCode(code);
            if (info == null)
                return (null, null, null);

            string? province = null;
            string? city = null;
            string? district = null;

            if (info.Level == 1)
            {
                province = info.ShortName;
            }
            else if (info.Level == 2)
            {
                city = info.ShortName;
                if (Regions.TryGetValue(info.ParentCode, out var prov))
                    province = prov.ShortName;
            }
            else if (info.Level == 3)
            {
                district = info.ShortName;
                if (Regions.TryGetValue(info.ParentCode, out var cityInfo))
                {
                    city = cityInfo.ShortName;
                    if (Regions.TryGetValue(cityInfo.ParentCode, out var prov))
                        province = prov.ShortName;
                }
            }

            return (province, city, district);
        }

        /// <summary>
        /// 判断是否为有效的行政区划代码
        /// </summary>
        /// <param name="code">行政区划代码</param>
        /// <returns>是否有效</returns>
        public static bool IsValidCode(string code)
        {
            if (string.IsNullOrEmpty(code) || code.Length != 6)
                return false;

            foreach (var c in code)
            {
                if (!char.IsDigit(c))
                    return false;
            }

            var provinceCode = code.Substring(0, 2) + "0000";
            return Regions.ContainsKey(provinceCode);
        }

        #endregion
    }
}