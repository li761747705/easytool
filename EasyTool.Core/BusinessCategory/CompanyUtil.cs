using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyTool.BusinessCategory
{
    /// <summary>
    /// 公司名称生成器
    /// 支持随机生成真实风格的中国公司名称
    /// </summary>
    public static class CompanyUtil
    {
        #region 数据

        // 行业类型
        private static readonly string[] Industries = {
            "科技", "网络", "信息", "软件", "互联网", "电子商务",
            "金融", "投资", "基金", "资产", "财富", "资本",
            "教育", "培训", "文化", "传媒", "广告", "影视",
            "医疗", "健康", "医药", "生物", "制药",
            "建筑", "工程", "地产", "房地产", "物业",
            "制造", "工业", "机械", "汽车", "电子", "电器",
            "贸易", "商贸", "进出口", "供应链",
            "物流", "运输", "快递", "仓储",
            "餐饮", "食品", "农业", "农牧",
            "服装", "纺织", "时尚", "化妆品",
            "能源", "电力", "石油", "化工", "新材料",
            "环保", "新能源", "节能", "绿色",
            "旅游", "酒店", "航空", "出行",
            "法律", "咨询", "人力资源", "管理",
            "设计", "装修", "家居", "建材",
            "体育", "健身", "娱乐", "游戏",
            "安全", "安防", "智能", "物联网", "大数据",
            "通信", "电信", "移动", "通讯"
        };

        // 公司类型
        private static readonly string[] CompanyTypes = {
            "有限公司", "有限责任公司", "股份有限公司",
            "集团", "集团有限公司",
            "合伙企业", "有限合伙企业",
            "独资公司", "分公司", "子公司"
        };

        // 常用公司名称前缀（地域特色）
        private static readonly string[] RegionPrefixes = {
            "中华", "中国", "华夏", "东方", "南方", "北方", "西部",
            "北京", "上海", "广州", "深圳", "杭州", "南京", "苏州",
            "成都", "武汉", "西安", "重庆", "天津", "青岛", "大连",
            "长三角", "珠三角", "京津冀"
        };

        // 企业字号（常用词）
        private static readonly string[] BrandWords = {
            "华", "盛", "达", "鑫", "龙", "凤", "鹏", "腾", "飞", "翔",
            "金", "银", "宝", "玉", "珠", "翠", "晶", "钻", "贝", "珍",
            "信", "诚", "德", "义", "仁", "智", "勇", "善", "美", "良",
            "新", "创", "拓", "展", "进", "步", "越", "超", "领", "先",
            "峰", "巅", "顶", "极", "卓", "优", "佳", "嘉", "豪", "宏",
            "顺", "泰", "安", "平", "稳", "康", "宁", "和", "瑞", "祥",
            "丰", "富", "荣", "贵", "尊", "显", "名", "誉", "望", "魁",
            "博", "厚", "深", "远", "广", "大", "强", "壮", "坚", "实",
            "恒", "久", "永", "长", "延", "续", "承", "传", "继", "延",
            "亮", "明", "晖", "耀", "辉", "映", "照", "灿", "焕", "烁",
            "洁", "净", "清", "雅", "韵", "风", "云", "雨", "露", "霖",
            "海", "洋", "江", "河", "湖", "溪", "泉", "源", "流", "涌",
            "山", "岭", "峰", "谷", "岩", "石", "土", "地", "林", "森",
            "松", "柏", "杨", "柳", "梅", "兰", "竹", "菊", "荷", "莲",
            "星", "月", "日", "辰", "光", "影", "景", "象", "境", "域",
            "通", "联", "聚", "汇", "合", "融", "济", "助", "扶", "携",
            "一", "二", "三", "四", "五", "六", "七", "八", "九", "十",
            "百", "千", "万", "亿", "兆", "京", "垓", "秭", "穰", "沟"
        };

        // 双字品牌词组合
        private static readonly string[] BrandTwoWords = {
            "华为", "中兴", "腾讯", "阿里", "百度", "京东", "网易", "新浪",
            "联想", "海尔", "格力", "美的", "小米", "魅族", "OPPO", "vivo",
            "万达", "恒大", "碧桂园", "保利", "绿地", "万科", "龙湖",
            "平安", "人寿", "招商", "浦发", "民生", "兴业", "华夏",
            "比亚迪", "吉利", "长城", "奇瑞", "蔚来", "理想", "小鹏",
            "哔哩哔哩", "字节跳动", "快手", "知乎", "豆瓣", "美团", "饿了么",
            "滴滴", "携程", "去哪儿", "同程", "途牛", "马蜂窝",
            "喜茶", "奈雪", "星巴克", "瑞幸", "蜜雪冰城", "肯德基",
            "华谊", "博纳", "光线", "万达影城", "中影", "上影",
            "科大讯飞", "商汤", "旷视", "依图", "云从", "深兰",
            "宁德时代", "比亚迪", "国轩高科", "亿纬锂能", "孚能",
            "中石油", "中石化", "中海油", "神华", "中煤", "华能",
            "中铁", "中建", "中交", "中电", "中冶", "中核",
            "大疆", "极飞", "零度智控", "亿航", "昊翔",
            "蔚来", "理想", "小鹏", "威马", "哪吒", "零跑"
        };

        // 企业字号（三字）
        private static readonly string[] BrandThreeWords = {
            "华创科", "鑫达盛", "龙腾飞", "金宝源", "信德诚",
            "新创展", "峰巅顶", "顺泰安", "丰富荣", "博厚深",
            "恒久永", "亮明耀", "洁净清", "海江河", "山峰岭",
            "松柏杨", "星月日", "通联聚", "众志成", "宏图展",
            "锦绣程", "瑞祥宁", "鼎盛峰", "嘉优佳", "益康宁",
            "众合联", "汇聚通", "融通达", "诚信德", "厚德载",
            "兴旺发", "茂盛林", "锦绣华", "瑞兆祥", "鸿运达",
            "金泰安", "银瑞祥", "玉满堂", "珠光宝", "钻石源",
            "飞天鹏", "跃龙门", "展宏图", "创未来", "领先锋"
        };

        private static readonly Random Random = new Random();

        #endregion

        #region 生成方法

        /// <summary>
        /// 随机生成公司名称
        /// </summary>
        /// <returns>公司名称</returns>
        public static string Generate()
        {
            return Generate(null, null, null);
        }

        /// <summary>
        /// 随机生成公司名称
        /// </summary>
        /// <param name="industry">行业类型（可选）</param>
        /// <returns>公司名称</returns>
        public static string Generate(string? industry)
        {
            return Generate(industry, null, null);
        }

        /// <summary>
        /// 随机生成公司名称
        /// </summary>
        /// <param name="industry">行业类型（可选）</param>
        /// <param name="companyType">公司类型（可选）</param>
        /// <returns>公司名称</returns>
        public static string Generate(string? industry, string? companyType)
        {
            return Generate(industry, companyType, null);
        }

        /// <summary>
        /// 随机生成公司名称
        /// </summary>
        /// <param name="industry">行业类型（可选）</param>
        /// <param name="companyType">公司类型（可选）</param>
        /// <param name="regionPrefix">地域前缀（可选）</param>
        /// <returns>公司名称</returns>
        public static string Generate(string? industry, string? companyType, string? regionPrefix)
        {
            // 生成企业字号
            var brand = GenerateBrand();

            // 选择行业
            var selectedIndustry = industry ?? Industries[Random.Next(Industries.Length)];

            // 选择公司类型
            var selectedType = companyType ?? CompanyTypes[Random.Next(CompanyTypes.Length)];

            // 是否添加地域前缀（30%概率）
            if (regionPrefix != null || Random.NextDouble() < 0.3)
            {
                var prefix = regionPrefix ?? RegionPrefixes[Random.Next(RegionPrefixes.Length)];
                return $"{prefix}{brand}{selectedIndustry}{selectedType}";
            }

            return $"{brand}{selectedIndustry}{selectedType}";
        }

        /// <summary>
        /// 生成科技公司名称
        /// </summary>
        /// <returns>科技公司名称</returns>
        public static string GenerateTechCompany()
        {
            return Generate("科技", null, null);
        }

        /// <summary>
        /// 生成金融公司名称
        /// </summary>
        /// <returns>金融公司名称</returns>
        public static string GenerateFinancialCompany()
        {
            return Generate("金融", null, null);
        }

        /// <summary>
        /// 生成教育公司名称
        /// </summary>
        /// <returns>教育公司名称</returns>
        public static string GenerateEducationCompany()
        {
            return Generate("教育", null, null);
        }

        /// <summary>
        /// 生成集团公司名称
        /// </summary>
        /// <returns>集团公司名称</returns>
        public static string GenerateGroupCompany()
        {
            return Generate(null, "集团有限公司", null);
        }

        /// <summary>
        /// 批量生成公司名称
        /// </summary>
        /// <param name="count">数量</param>
        /// <param name="industry">行业类型（可选）</param>
        /// <returns>公司名称列表</returns>
        public static List<string> GenerateBatch(int count, string? industry = null)
        {
            var companies = new List<string>();
            for (var i = 0; i < count; i++)
            {
                companies.Add(Generate(industry));
            }
            return companies;
        }

        /// <summary>
        /// 生成完整公司信息（包含地址等）
        /// </summary>
        /// <returns>公司信息</returns>
        public static CompanyInfo GenerateFullInfo()
        {
            var name = Generate();
            var province = RegionUtil.GetProvinces()[Random.Next(RegionUtil.GetProvinces().Count)].ShortName;

            return new CompanyInfo
            {
                Name = name,
                Province = province,
                Address = AddressUtil.Generate(province),
                Industry = Industries[Random.Next(Industries.Length)]
            };
        }

        #endregion

        #region 数据获取

        /// <summary>
        /// 获取行业列表
        /// </summary>
        /// <returns>行业列表</returns>
        public static string[] GetIndustriesList()
        {
            return Industries.ToArray();
        }

        /// <summary>
        /// 获取公司类型列表
        /// </summary>
        /// <returns>公司类型列表</returns>
        public static string[] GetCompanyTypesList()
        {
            return CompanyTypes.ToArray();
        }

        /// <summary>
        /// 获取地域前缀列表
        /// </summary>
        /// <returns>地域前缀列表</returns>
        public static string[] GetRegionPrefixesList()
        {
            return RegionPrefixes.ToArray();
        }

        /// <summary>
        /// 获取随机行业
        /// </summary>
        /// <returns>行业名称</returns>
        public static string GetRandomIndustry()
        {
            return Industries[Random.Next(Industries.Length)];
        }

        /// <summary>
        /// 获取随机公司类型
        /// </summary>
        /// <returns>公司类型</returns>
        public static string GetRandomCompanyType()
        {
            return CompanyTypes[Random.Next(CompanyTypes.Length)];
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 生成企业字号
        /// </summary>
        private static string GenerateBrand()
        {
            // 20%概率使用知名品牌词
            if (Random.NextDouble() < 0.2)
            {
                return BrandTwoWords[Random.Next(BrandTwoWords.Length)];
            }

            // 30%概率使用三字品牌词
            if (Random.NextDouble() < 0.3)
            {
                return BrandThreeWords[Random.Next(BrandThreeWords.Length)];
            }

            // 生成2-4字品牌词
            var length = Random.NextDouble() < 0.6 ? 2 : Random.NextDouble() < 0.8 ? 3 : 4;
            var brand = "";

            for (var i = 0; i < length; i++)
            {
                brand += BrandWords[Random.Next(BrandWords.Length)];
            }

            return brand;
        }

        #endregion
    }

    /// <summary>
    /// 公司信息
    /// </summary>
    public class CompanyInfo
    {
        /// <summary>
        /// 公司名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 所在省份
        /// </summary>
        public string Province { get; set; } = string.Empty;

        /// <summary>
        /// 详细地址
        /// </summary>
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// 所属行业
        /// </summary>
        public string Industry { get; set; } = string.Empty;
    }
}