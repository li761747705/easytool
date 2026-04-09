using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyTool.BusinessCategory
{
    /// <summary>
    /// 中国大学信息工具类
    /// 提供大学信息查询功能
    /// </summary>
    public static class UniversityUtil
    {
        #region 数据结构

        /// <summary>
        /// 大学信息
        /// </summary>
        public class UniversityInfo
        {
            /// <summary>
            /// 学校代码
            /// </summary>
            public string Code { get; set; } = string.Empty;

            /// <summary>
            /// 学校名称
            /// </summary>
            public string Name { get; set; } = string.Empty;

            /// <summary>
            /// 所在省份
            /// </summary>
            public string Province { get; set; } = string.Empty;

            /// <summary>
            /// 所在城市
            /// </summary>
            public string City { get; set; } = string.Empty;

            /// <summary>
            /// 是否985
            /// </summary>
            public bool Is985 { get; set; }

            /// <summary>
            /// 是否211
            /// </summary>
            public bool Is211 { get; set; }

            /// <summary>
            /// 是否双一流
            /// </summary>
            public bool IsDoubleFirstClass { get; set; }

            /// <summary>
            /// 学校类型（综合、理工、师范等）
            /// </summary>
            public string Type { get; set; } = string.Empty;

            /// <summary>
            /// 办学层次（本科、专科）
            /// </summary>
            public string Level { get; set; } = string.Empty;
        }

        #endregion

        #region 静态数据

        private static readonly List<UniversityInfo> Universities = new();
        private static readonly Dictionary<string, UniversityInfo> UniversityByCode = new();
        private static bool _initialized = false;
        private static readonly object _lock = new();

        #endregion

        #region 初始化

        static UniversityUtil()
        {
            InitData();
        }

        private static void InitData()
        {
            lock (_lock)
            {
                if (_initialized)
                    return;

                // 主要大学数据（985/211院校）
                var universityData = new[]
                {
                    // 北京
                    ("10001", "北京大学", "北京", "北京", true, true, true, "综合", "本科"),
                    ("10002", "中国人民大学", "北京", "北京", true, true, true, "综合", "本科"),
                    ("10003", "清华大学", "北京", "北京", true, true, true, "理工", "本科"),
                    ("10004", "北京交通大学", "北京", "北京", false, true, true, "理工", "本科"),
                    ("10005", "北京工业大学", "北京", "北京", false, true, true, "理工", "本科"),
                    ("10006", "北京航空航天大学", "北京", "北京", true, true, true, "理工", "本科"),
                    ("10007", "北京理工大学", "北京", "北京", true, true, true, "理工", "本科"),
                    ("10008", "北京科技大学", "北京", "北京", false, true, true, "理工", "本科"),
                    ("10019", "中国农业大学", "北京", "北京", true, true, true, "农林", "本科"),
                    ("10022", "北京林业大学", "北京", "北京", false, true, true, "农林", "本科"),
                    ("10023", "北京协和医学院", "北京", "北京", false, true, true, "医药", "本科"),
                    ("10027", "北京师范大学", "北京", "北京", true, true, true, "师范", "本科"),
                    ("10028", "首都师范大学", "北京", "北京", false, false, true, "师范", "本科"),
                    ("10030", "北京外国语大学", "北京", "北京", false, true, true, "语言", "本科"),
                    ("10033", "中国传媒大学", "北京", "北京", false, true, true, "艺术", "本科"),
                    ("10034", "中央财经大学", "北京", "北京", false, true, true, "财经", "本科"),
                    ("10036", "对外经济贸易大学", "北京", "北京", false, true, true, "财经", "本科"),
                    ("10041", "中国人民公安大学", "北京", "北京", false, false, true, "政法", "本科"),
                    ("10042", "北京体育大学", "北京", "北京", false, true, false, "体育", "本科"),
                    ("10043", "中央音乐学院", "北京", "北京", false, true, false, "艺术", "本科"),
                    ("10045", "中央美术学院", "北京", "北京", false, false, false, "艺术", "本科"),
                    ("10046", "中央戏剧学院", "北京", "北京", false, false, false, "艺术", "本科"),
                    ("10047", "中央民族大学", "北京", "北京", true, true, true, "民族", "本科"),
                    ("10053", "中国政法大学", "北京", "北京", false, true, true, "政法", "本科"),
                    ("11413", "中国矿业大学(北京)", "北京", "北京", false, true, true, "理工", "本科"),
                    ("11414", "中国石油大学(北京)", "北京", "北京", false, true, true, "理工", "本科"),
                    ("11415", "中国地质大学(北京)", "北京", "北京", false, true, true, "理工", "本科"),

                    // 上海
                    ("10246", "复旦大学", "上海", "上海", true, true, true, "综合", "本科"),
                    ("10247", "同济大学", "上海", "上海", true, true, true, "理工", "本科"),
                    ("10248", "上海交通大学", "上海", "上海", true, true, true, "综合", "本科"),
                    ("10251", "华东理工大学", "上海", "上海", false, true, true, "理工", "本科"),
                    ("10252", "上海理工大学", "上海", "上海", false, false, false, "理工", "本科"),
                    ("10254", "上海海事大学", "上海", "上海", false, false, false, "理工", "本科"),
                    ("10255", "东华大学", "上海", "上海", false, true, true, "理工", "本科"),
                    ("10264", "上海海洋大学", "上海", "上海", false, false, true, "农林", "本科"),
                    ("10269", "华东师范大学", "上海", "上海", true, true, true, "师范", "本科"),
                    ("10270", "上海师范大学", "上海", "上海", false, false, false, "师范", "本科"),
                    ("10271", "上海外国语大学", "上海", "上海", false, true, true, "语言", "本科"),
                    ("10272", "上海财经大学", "上海", "上海", false, true, true, "财经", "本科"),
                    ("10273", "上海对外经贸大学", "上海", "上海", false, false, false, "财经", "本科"),
                    ("10274", "上海海关学院", "上海", "上海", false, false, false, "财经", "本科"),
                    ("10276", "华东政法大学", "上海", "上海", false, false, false, "政法", "本科"),
                    ("10277", "上海体育学院", "上海", "上海", false, false, true, "体育", "本科"),
                    ("10278", "上海音乐学院", "上海", "上海", false, false, true, "艺术", "本科"),
                    ("10279", "上海戏剧学院", "上海", "上海", false, false, false, "艺术", "本科"),
                    ("10280", "上海大学", "上海", "上海", false, true, true, "综合", "本科"),
                    ("10283", "上海公安学院", "上海", "上海", false, false, false, "政法", "本科"),

                    // 广东
                    ("10558", "中山大学", "广东", "广州", true, true, true, "综合", "本科"),
                    ("10559", "暨南大学", "广东", "广州", false, true, true, "综合", "本科"),
                    ("10560", "汕头大学", "广东", "汕头", false, false, false, "综合", "本科"),
                    ("10561", "华南理工大学", "广东", "广州", true, true, true, "理工", "本科"),
                    ("10564", "华南农业大学", "广东", "广州", false, false, true, "农林", "本科"),
                    ("10566", "广东海洋大学", "广东", "湛江", false, false, false, "农林", "本科"),
                    ("10570", "广州医科大学", "广东", "广州", false, false, true, "医药", "本科"),
                    ("10572", "广州中医药大学", "广东", "广州", false, false, true, "医药", "本科"),
                    ("10574", "华南师范大学", "广东", "广州", false, true, true, "师范", "本科"),
                    ("10577", "惠州学院", "广东", "惠州", false, false, false, "综合", "本科"),
                    ("10582", "深圳大学", "广东", "深圳", false, false, false, "综合", "本科"),
                    ("10588", "广东技术师范大学", "广东", "广州", false, false, false, "师范", "本科"),
                    ("10590", "深圳技术大学", "广东", "深圳", false, false, false, "理工", "本科"),
                    ("10592", "广东财经大学", "广东", "广州", false, false, false, "财经", "本科"),
                    ("10593", "广西大学", "广西", "南宁", false, true, false, "综合", "本科"),
                    ("10595", "桂林电子科技大学", "广西", "桂林", false, false, false, "理工", "本科"),
                    ("10596", "桂林理工大学", "广西", "桂林", false, false, false, "理工", "本科"),
                    ("11078", "广州大学", "广东", "广州", false, false, false, "综合", "本科"),
                    ("11810", "哈尔滨工业大学(深圳)", "广东", "深圳", true, true, true, "理工", "本科"),
                    ("11819", "东莞理工学院", "广东", "东莞", false, false, false, "理工", "本科"),
                    ("11902", "香港中文大学(深圳)", "广东", "深圳", false, false, false, "综合", "本科"),
                    ("12121", "南方医科大学", "广东", "广州", false, false, true, "医药", "本科"),
                    ("16408", "香港科技大学(广州)", "广东", "广州", false, false, false, "综合", "本科"),

                    // 浙江
                    ("10335", "浙江大学", "浙江", "杭州", true, true, true, "综合", "本科"),
                    ("10336", "杭州电子科技大学", "浙江", "杭州", false, false, false, "理工", "本科"),
                    ("10337", "浙江工业大学", "浙江", "杭州", false, false, false, "理工", "本科"),
                    ("10338", "浙江理工大学", "浙江", "杭州", false, false, false, "理工", "本科"),
                    ("10340", "浙江海洋大学", "浙江", "舟山", false, false, false, "农林", "本科"),
                    ("10341", "浙江农林大学", "浙江", "杭州", false, false, false, "农林", "本科"),
                    ("10343", "温州医科大学", "浙江", "温州", false, false, false, "医药", "本科"),
                    ("10344", "浙江中医药大学", "浙江", "杭州", false, false, false, "医药", "本科"),
                    ("10345", "浙江师范大学", "浙江", "金华", false, false, false, "师范", "本科"),
                    ("10346", "杭州师范大学", "浙江", "杭州", false, false, false, "师范", "本科"),
                    ("10347", "湖州师范学院", "浙江", "湖州", false, false, false, "师范", "本科"),
                    ("10349", "绍兴文理学院", "浙江", "绍兴", false, false, false, "综合", "本科"),
                    ("10350", "台州学院", "浙江", "台州", false, false, false, "综合", "本科"),
                    ("10351", "温州大学", "浙江", "温州", false, false, false, "综合", "本科"),
                    ("10353", "浙江工商大学", "浙江", "杭州", false, false, false, "财经", "本科"),
                    ("10354", "嘉兴学院", "浙江", "嘉兴", false, false, false, "综合", "本科"),
                    ("10355", "中国美术学院", "浙江", "杭州", false, false, true, "艺术", "本科"),
                    ("10356", "中国计量大学", "浙江", "杭州", false, false, false, "理工", "本科"),
                    ("10357", "安徽大学", "安徽", "合肥", false, true, false, "综合", "本科"),
                    ("10358", "中国科学技术大学", "安徽", "合肥", true, true, true, "理工", "本科"),
                    ("10359", "合肥工业大学", "安徽", "合肥", false, true, false, "理工", "本科"),

                    // 江苏
                    ("10284", "南京大学", "江苏", "南京", true, true, true, "综合", "本科"),
                    ("10285", "苏州大学", "江苏", "苏州", false, false, true, "综合", "本科"),
                    ("10286", "东南大学", "江苏", "南京", true, true, true, "综合", "本科"),
                    ("10287", "南京航空航天大学", "江苏", "南京", false, true, true, "理工", "本科"),
                    ("10288", "南京理工大学", "江苏", "南京", false, true, true, "理工", "本科"),
                    ("10289", "江苏科技大学", "江苏", "镇江", false, false, false, "理工", "本科"),
                    ("10290", "中国矿业大学", "江苏", "徐州", false, true, true, "理工", "本科"),
                    ("10291", "南京工业大学", "江苏", "南京", false, false, false, "理工", "本科"),
                    ("10292", "常州大学", "江苏", "常州", false, false, false, "理工", "本科"),
                    ("10294", "河海大学", "江苏", "南京", false, true, true, "理工", "本科"),
                    ("10295", "江南大学", "江苏", "无锡", false, true, true, "综合", "本科"),
                    ("10298", "南京林业大学", "江苏", "南京", false, true, true, "农林", "本科"),
                    ("10299", "江苏大学", "江苏", "镇江", false, false, false, "综合", "本科"),
                    ("10300", "南京信息工程大学", "江苏", "南京", false, true, true, "理工", "本科"),
                    ("10304", "南通大学", "江苏", "南通", false, false, false, "综合", "本科"),
                    ("10305", "盐城工学院", "江苏", "盐城", false, false, false, "理工", "本科"),
                    ("10307", "南京农业大学", "江苏", "南京", false, true, true, "农林", "本科"),
                    ("10312", "南京医科大学", "江苏", "南京", false, false, true, "医药", "本科"),
                    ("10313", "徐州医科大学", "江苏", "徐州", false, false, false, "医药", "本科"),
                    ("10315", "南京中医药大学", "江苏", "南京", false, false, true, "医药", "本科"),
                    ("10316", "中国药科大学", "江苏", "南京", false, true, true, "医药", "本科"),
                    ("10319", "南京师范大学", "江苏", "南京", false, true, true, "师范", "本科"),
                    ("10320", "江苏师范大学", "江苏", "徐州", false, false, false, "师范", "本科"),

                    // 其他重点城市
                    ("10141", "大连理工大学", "辽宁", "大连", true, true, true, "理工", "本科"),
                    ("10145", "东北大学", "辽宁", "沈阳", true, true, true, "理工", "本科"),
                    ("10151", "大连海事大学", "辽宁", "大连", false, true, false, "理工", "本科"),
                    ("10183", "吉林大学", "吉林", "长春", true, true, true, "综合", "本科"),
                    ("10200", "东北师范大学", "吉林", "长春", false, true, false, "师范", "本科"),
                    ("10213", "哈尔滨工业大学", "黑龙江", "哈尔滨", true, true, true, "理工", "本科"),
                    ("10217", "哈尔滨工程大学", "黑龙江", "哈尔滨", false, true, true, "理工", "本科"),
                    ("10422", "山东大学", "山东", "济南", true, true, true, "综合", "本科"),
                    ("10423", "中国海洋大学", "山东", "青岛", true, true, true, "综合", "本科"),
                    ("10425", "中国石油大学(华东)", "山东", "青岛", false, true, true, "理工", "本科"),
                    ("10459", "郑州大学", "河南", "郑州", false, true, false, "综合", "本科"),
                    ("10486", "武汉大学", "湖北", "武汉", true, true, true, "综合", "本科"),
                    ("10487", "华中科技大学", "湖北", "武汉", true, true, true, "综合", "本科"),
                    ("10491", "中国地质大学(武汉)", "湖北", "武汉", false, true, true, "理工", "本科"),
                    ("10497", "武汉理工大学", "湖北", "武汉", false, true, true, "理工", "本科"),
                    ("10511", "华中师范大学", "湖北", "武汉", false, true, true, "师范", "本科"),
                    ("10533", "中南大学", "湖南", "长沙", true, true, true, "综合", "本科"),
                    ("10532", "湖南大学", "湖南", "长沙", false, true, true, "综合", "本科"),
                    ("10533", "湖南师范大学", "湖南", "长沙", false, true, false, "师范", "本科"),
                    ("10593", "国防科技大学", "湖南", "长沙", true, true, true, "军事", "本科"),
                    ("10610", "四川大学", "四川", "成都", true, true, true, "综合", "本科"),
                    ("10611", "重庆大学", "重庆", "重庆", true, true, true, "综合", "本科"),
                    ("10613", "电子科技大学", "四川", "成都", true, true, true, "理工", "本科"),
                    ("10614", "西南财经大学", "四川", "成都", false, true, false, "财经", "本科"),
                    ("10635", "西南大学", "重庆", "重庆", false, true, false, "综合", "本科"),
                    ("10651", "西南财经大学", "四川", "成都", false, true, false, "财经", "本科"),
                    ("10698", "西安交通大学", "陕西", "西安", true, true, true, "综合", "本科"),
                    ("10699", "西北工业大学", "陕西", "西安", true, true, true, "理工", "本科"),
                    ("10701", "西安电子科技大学", "陕西", "西安", false, true, true, "理工", "本科"),
                    ("10710", "长安大学", "陕西", "西安", false, true, false, "理工", "本科"),
                    ("10712", "西北农林科技大学", "陕西", "杨凌", true, true, true, "农林", "本科"),
                    ("10718", "陕西师范大学", "陕西", "西安", false, true, false, "师范", "本科"),
                    ("10730", "兰州大学", "甘肃", "兰州", true, true, true, "综合", "本科")
                };

                foreach (var (code, name, province, city, is985, is211, isDoubleFirstClass, type, level) in universityData)
                {
                    var info = new UniversityInfo
                    {
                        Code = code,
                        Name = name,
                        Province = province,
                        City = city,
                        Is985 = is985,
                        Is211 = is211,
                        IsDoubleFirstClass = isDoubleFirstClass,
                        Type = type,
                        Level = level
                    };
                    Universities.Add(info);
                    UniversityByCode[code] = info;
                }

                _initialized = true;
            }
        }

        #endregion

        #region 查询方法

        /// <summary>
        /// 根据代码获取大学信息
        /// </summary>
        /// <param name="code">学校代码</param>
        /// <returns>大学信息</returns>
        public static UniversityInfo? GetByCode(string code)
        {
            return UniversityByCode.TryGetValue(code, out var info) ? info : null;
        }

        /// <summary>
        /// 根据名称搜索大学
        /// </summary>
        /// <param name="name">学校名称（支持模糊搜索）</param>
        /// <returns>大学列表</returns>
        public static List<UniversityInfo> SearchByName(string name)
        {
            return Universities
                .Where(u => u.Name.Contains(name))
                .ToList();
        }

        /// <summary>
        /// 根据省份获取大学列表
        /// </summary>
        /// <param name="province">省份名称</param>
        /// <returns>大学列表</returns>
        public static List<UniversityInfo> GetByProvince(string province)
        {
            return Universities
                .Where(u => u.Province == province)
                .ToList();
        }

        /// <summary>
        /// 根据城市获取大学列表
        /// </summary>
        /// <param name="city">城市名称</param>
        /// <returns>大学列表</returns>
        public static List<UniversityInfo> GetByCity(string city)
        {
            return Universities
                .Where(u => u.City == city)
                .ToList();
        }

        /// <summary>
        /// 获取所有985大学
        /// </summary>
        /// <returns>985大学列表</returns>
        public static List<UniversityInfo> Get985Universities()
        {
            return Universities.Where(u => u.Is985).ToList();
        }

        /// <summary>
        /// 获取所有211大学
        /// </summary>
        /// <returns>211大学列表</returns>
        public static List<UniversityInfo> Get211Universities()
        {
            return Universities.Where(u => u.Is211).ToList();
        }

        /// <summary>
        /// 获取所有双一流大学
        /// </summary>
        /// <returns>双一流大学列表</returns>
        public static List<UniversityInfo> GetDoubleFirstClassUniversities()
        {
            return Universities.Where(u => u.IsDoubleFirstClass).ToList();
        }

        /// <summary>
        /// 根据类型获取大学列表
        /// </summary>
        /// <param name="type">学校类型（综合、理工、师范、医药、财经等）</param>
        /// <returns>大学列表</returns>
        public static List<UniversityInfo> GetByType(string type)
        {
            return Universities.Where(u => u.Type == type).ToList();
        }

        /// <summary>
        /// 获取所有大学
        /// </summary>
        /// <returns>大学列表</returns>
        public static List<UniversityInfo> GetAll()
        {
            return Universities.ToList();
        }

        /// <summary>
        /// 获取大学数量
        /// </summary>
        /// <returns>大学数量</returns>
        public static int GetCount()
        {
            return Universities.Count;
        }

        /// <summary>
        /// 判断是否为985大学
        /// </summary>
        /// <param name="code">学校代码</param>
        /// <returns>是否为985大学</returns>
        public static bool Is985(string code)
        {
            return UniversityByCode.TryGetValue(code, out var info) && info.Is985;
        }

        /// <summary>
        /// 判断是否为211大学
        /// </summary>
        /// <param name="code">学校代码</param>
        /// <returns>是否为211大学</returns>
        public static bool Is211(string code)
        {
            return UniversityByCode.TryGetValue(code, out var info) && info.Is211;
        }

        /// <summary>
        /// 判断是否为双一流大学
        /// </summary>
        /// <param name="code">学校代码</param>
        /// <returns>是否为双一流大学</returns>
        public static bool IsDoubleFirstClass(string code)
        {
            return UniversityByCode.TryGetValue(code, out var info) && info.IsDoubleFirstClass;
        }

        #endregion
    }
}