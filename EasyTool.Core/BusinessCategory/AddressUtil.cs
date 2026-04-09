using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyTool.BusinessCategory
{
    /// <summary>
    /// 中国地址生成器
    /// 支持随机生成真实风格的中国地址
    /// </summary>
    public static class AddressUtil
    {
        #region 数据

        // 常见道路类型
        private static readonly string[] RoadTypes = {
            "路", "街", "大道", "大街", "巷", "胡同", "弄", "道", "公路", "街道"
        };

        // 常见道路名称前缀
        private static readonly string[] RoadPrefixes = {
            "中山", "解放", "建设", "人民", "和平", "光明", "胜利", "团结", "爱国", "民主",
            "长江", "黄河", "泰山", "华山", "珠江", "松花江", "淮河", "汉江", "湘江", "赣江",
            "北京", "上海", "南京", "西安", "成都", "重庆", "武汉", "广州", "深圳", "杭州",
            "东", "西", "南", "北", "中", "新", "老", "大", "小", "高",
            "金", "银", "玉", "宝", "福", "禄", "寿", "喜", "财", "源",
            "春", "夏", "秋", "冬", "阳", "月", "星", "云", "风", "雨",
            "红", "绿", "蓝", "白", "青", "紫", "金", "银", "铜", "铁",
            "科技", "工业", "商业", "文化", "教育", "体育", "金融", "贸易", "物流", "创新"
        };

        // 常见小区名称前缀
        private static readonly string[] CommunityPrefixes = {
            "阳光", "幸福", "金色", "蓝色", "绿色", "银色", "金色家园", "阳光花", "幸福家",
            "新城", "花园", "雅苑", "名苑", "华庭", "豪庭", "御景", "蓝庭", "绿庭", "紫庭",
            "锦绣", "世纪", "东方", "南方", "北方", "西方", "中央", "时代", "现代", "未来",
            "和谐", "盛世", "繁华", "盛世华", "繁华世", "和谐城", "盛世锦", "繁华城",
            "龙湖", "万科", "碧桂园", "恒大", "保利", "绿地", "中海", "华润", "融创", "绿城"
        };

        // 小区类型后缀
        private static readonly string[] CommunitySuffixes = {
            "小区", "花园", "雅苑", "名苑", "华庭", "豪庭", "御景", "家园", "新村", "公寓",
            "苑", "庭", "园", "城", "府", "邸", "居", "轩", "阁", "楼"
        };

        // 商业区域名称
        private static readonly string[] CommercialAreas = {
            "商业中心", "购物广场", "商务中心", "金融中心", "创业园", "科技园",
            "产业园", "工业园", "物流园", "孵化器", "众创空间", "创意园"
        };

        // 常见建筑物类型
        private static readonly string[] BuildingTypes = {
            "大厦", "大楼", "大厦", "中心", "广场", "楼", "写字楼", "办公楼", "综合楼", "商住楼"
        };

        // 常见建筑物名称前缀
        private static readonly string[] BuildingPrefixes = {
            "金茂", "中信", "华联", "万达", "恒隆", "世贸", "国贸", "国际", "环球", "中央",
            "东方", "南方", "北方", "西方", "新", "老", "中", "第一", "第二", "第三",
            "科技", "金融", "商务", "商贸", "创业", "创新", "发展", "进步", "现代", "时代"
        };

        private static readonly Random Random = new Random();

        #endregion

        #region 生成方法

        /// <summary>
        /// 随机生成中国地址
        /// </summary>
        /// <returns>地址字符串</returns>
        public static string Generate()
        {
            return Generate(null);
        }

        /// <summary>
        /// 随机生成指定省份的地址
        /// </summary>
        /// <param name="province">省份名称（可选）</param>
        /// <returns>地址字符串</returns>
        public static string Generate(string? province)
        {
            return Generate(province, null);
        }

        /// <summary>
        /// 随机生成指定省份和城市的地址
        /// </summary>
        /// <param name="province">省份名称（可选）</param>
        /// <param name="city">城市名称（可选）</param>
        /// <returns>地址字符串</returns>
        public static string Generate(string? province, string? city)
        {
            // 选择省份
            var provinces = RegionUtil.GetProvinces();
            var selectedProvince = province ?? provinces[Random.Next(provinces.Count)].ShortName;

            // 选择城市
            var cities = RegionUtil.GetCitiesByName(selectedProvince);
            var selectedCity = city;
            if (selectedCity == null && cities.Count > 0)
            {
                selectedCity = cities[Random.Next(cities.Count)].ShortName;
            }
            else if (selectedCity == null)
            {
                selectedCity = selectedProvince;
            }

            // 选择区县
            var cityCode = cities.FirstOrDefault(c => c.ShortName == selectedCity)?.Code;
            var districts = cityCode != null ? RegionUtil.GetDistricts(cityCode) : new List<RegionUtil.RegionInfo>();
            var district = districts.Count > 0 ? districts[Random.Next(districts.Count)].ShortName : "";

            // 生成详细地址
            var detail = GenerateDetail();

            if (!string.IsNullOrEmpty(district))
            {
                return $"{selectedProvince}{selectedCity}{district}{detail}";
            }
            else
            {
                return $"{selectedProvince}{selectedCity}{detail}";
            }
        }

        /// <summary>
        /// 生成详细地址部分（道路+门牌号+小区/楼栋）
        /// </summary>
        /// <returns>详细地址</returns>
        public static string GenerateDetail()
        {
            // 选择地址类型（小区、商业楼、普通道路）
            var type = Random.NextDouble();

            if (type < 0.4)
            {
                // 小区地址
                return GenerateCommunityAddress();
            }
            else if (type < 0.6)
            {
                // 商业楼地址
                return GenerateBuildingAddress();
            }
            else
            {
                // 普通道路地址
                return GenerateRoadAddress();
            }
        }

        /// <summary>
        /// 生成小区地址
        /// </summary>
        /// <returns>小区地址</returns>
        public static string GenerateCommunityAddress()
        {
            var road = GenerateRoadName();
            var roadNumber = Random.Next(1, 500);
            var community = GenerateCommunityName();
            var buildingNumber = Random.Next(1, 30);
            var unit = Random.Next(1, 10);
            var room = Random.Next(101, 2501);

            return $"{road}{roadNumber}号{community}{buildingNumber}栋{unit}单元{room}室";
        }

        /// <summary>
        /// 生成商业楼地址
        /// </summary>
        /// <returns>商业楼地址</returns>
        public static string GenerateBuildingAddress()
        {
            var road = GenerateRoadName();
            var roadNumber = Random.Next(1, 500);
            var building = GenerateBuildingName();
            var floor = Random.Next(1, 30);
            var room = Random.Next(101, 2001);

            return $"{road}{roadNumber}号{building}{floor}层{room}室";
        }

        /// <summary>
        /// 生成普通道路地址
        /// </summary>
        /// <returns>道路地址</returns>
        public static string GenerateRoadAddress()
        {
            var road = GenerateRoadName();
            var roadNumber = Random.Next(1, 999);

            return $"{road}{roadNumber}号";
        }

        /// <summary>
        /// 批量生成地址
        /// </summary>
        /// <param name="count">数量</param>
        /// <param name="province">省份（可选）</param>
        /// <returns>地址列表</returns>
        public static List<string> GenerateBatch(int count, string? province = null)
        {
            var addresses = new List<string>();
            for (var i = 0; i < count; i++)
            {
                addresses.Add(Generate(province));
            }
            return addresses;
        }

        /// <summary>
        /// 生成完整地址信息
        /// </summary>
        /// <returns>地址信息对象</returns>
        public static AddressInfo GenerateFullInfo()
        {
            var provinces = RegionUtil.GetProvinces();
            var province = provinces[Random.Next(provinces.Count)];
            var cities = RegionUtil.GetCities(province.Code);
            var city = cities.Count > 0 ? cities[Random.Next(cities.Count)] : province;
            var districts = RegionUtil.GetDistricts(city.Code);
            var district = districts.Count > 0 ? districts[Random.Next(districts.Count)] : null;

            return new AddressInfo
            {
                Province = province.ShortName,
                ProvinceCode = province.Code,
                City = city.ShortName,
                CityCode = city.Code,
                District = district?.ShortName ?? "",
                DistrictCode = district?.Code ?? "",
                Detail = GenerateDetail(),
                FullAddress = Generate(province.ShortName, city.ShortName)
            };
        }

        #endregion

        #region 名称生成

        /// <summary>
        /// 生成道路名称
        /// </summary>
        /// <returns>道路名称</returns>
        public static string GenerateRoadName()
        {
            var prefix = RoadPrefixes[Random.Next(RoadPrefixes.Length)];
            var type = RoadTypes[Random.Next(RoadTypes.Length)];
            return prefix + type;
        }

        /// <summary>
        /// 生成小区名称
        /// </summary>
        /// <returns>小区名称</returns>
        public static string GenerateCommunityName()
        {
            var prefix = CommunityPrefixes[Random.Next(CommunityPrefixes.Length)];
            var suffix = CommunitySuffixes[Random.Next(CommunitySuffixes.Length)];
            return prefix + suffix;
        }

        /// <summary>
        /// 生成商业楼名称
        /// </summary>
        /// <returns>商业楼名称</returns>
        public static string GenerateBuildingName()
        {
            var prefix = BuildingPrefixes[Random.Next(BuildingPrefixes.Length)];
            var type = BuildingTypes[Random.Next(BuildingTypes.Length)];
            return prefix + type;
        }

        /// <summary>
        /// 生成商业区名称
        /// </summary>
        /// <returns>商业区名称</returns>
        public static string GenerateCommercialAreaName()
        {
            return CommercialAreas[Random.Next(CommercialAreas.Length)];
        }

        #endregion

        #region 数据获取

        /// <summary>
        /// 获取道路类型列表
        /// </summary>
        /// <returns>道路类型列表</returns>
        public static string[] GetRoadTypesList()
        {
            return RoadTypes.ToArray();
        }

        /// <summary>
        /// 获取道路名称前缀列表
        /// </summary>
        /// <returns>道路名称前缀列表</returns>
        public static string[] GetRoadPrefixesList()
        {
            return RoadPrefixes.ToArray();
        }

        /// <summary>
        /// 获取小区名称前缀列表
        /// </summary>
        /// <returns>小区名称前缀列表</returns>
        public static string[] GetCommunityPrefixesList()
        {
            return CommunityPrefixes.ToArray();
        }

        /// <summary>
        /// 获取建筑物名称前缀列表
        /// </summary>
        /// <returns>建筑物名称前缀列表</returns>
        public static string[] GetBuildingPrefixesList()
        {
            return BuildingPrefixes.ToArray();
        }

        #endregion
    }

    /// <summary>
    /// 地址信息
    /// </summary>
    public class AddressInfo
    {
        /// <summary>
        /// 省份名称
        /// </summary>
        public string Province { get; set; } = string.Empty;

        /// <summary>
        /// 省份代码
        /// </summary>
        public string ProvinceCode { get; set; } = string.Empty;

        /// <summary>
        /// 市名称
        /// </summary>
        public string City { get; set; } = string.Empty;

        /// <summary>
        /// 市代码
        /// </summary>
        public string CityCode { get; set; } = string.Empty;

        /// <summary>
        /// 区县名称
        /// </summary>
        public string District { get; set; } = string.Empty;

        /// <summary>
        /// 区县代码
        /// </summary>
        public string DistrictCode { get; set; } = string.Empty;

        /// <summary>
        /// 详细地址（道路+门牌号+楼栋）
        /// </summary>
        public string Detail { get; set; } = string.Empty;

        /// <summary>
        /// 完整地址
        /// </summary>
        public string FullAddress { get; set; } = string.Empty;
    }
}