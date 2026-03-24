using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyTool.BusinessCategory
{
    /// <summary>
    /// 中国省份城市工具类
    /// 提供省份、城市查询和验证功能
    /// </summary>
    public static class ProvinceUtil
    {
        /// <summary>
        /// 省份数据
        /// </summary>
        private static readonly Dictionary<string, ProvinceInfo> Provinces = new()
        {
            { "110000", new ProvinceInfo { Code = "110000", Name = "北京市", ShortName = "北京", Cities = new List<CityInfo> {
                new CityInfo { Code = "110100", Name = "北京市" }
            }}},
            { "120000", new ProvinceInfo { Code = "120000", Name = "天津市", ShortName = "天津", Cities = new List<CityInfo> {
                new CityInfo { Code = "120100", Name = "天津市" }
            }}},
            { "130000", new ProvinceInfo { Code = "130000", Name = "河北省", ShortName = "河北", Cities = new List<CityInfo> {
                new CityInfo { Code = "130100", Name = "石家庄市" },
                new CityInfo { Code = "130200", Name = "唐山市" },
                new CityInfo { Code = "130300", Name = "秦皇岛市" },
                new CityInfo { Code = "130400", Name = "邯郸市" },
                new CityInfo { Code = "130500", Name = "邢台市" },
                new CityInfo { Code = "130600", Name = "保定市" },
                new CityInfo { Code = "130700", Name = "张家口市" },
                new CityInfo { Code = "130800", Name = "承德市" },
                new CityInfo { Code = "130900", Name = "沧州市" },
                new CityInfo { Code = "131000", Name = "廊坊市" },
                new CityInfo { Code = "131100", Name = "衡水市" }
            }}},
            { "140000", new ProvinceInfo { Code = "140000", Name = "山西省", ShortName = "山西", Cities = new List<CityInfo> {
                new CityInfo { Code = "140100", Name = "太原市" },
                new CityInfo { Code = "140200", Name = "大同市" },
                new CityInfo { Code = "140300", Name = "阳泉市" },
                new CityInfo { Code = "140400", Name = "长治市" },
                new CityInfo { Code = "140500", Name = "晋城市" },
                new CityInfo { Code = "140600", Name = "朔州市" },
                new CityInfo { Code = "140700", Name = "晋中市" },
                new CityInfo { Code = "140800", Name = "运城市" },
                new CityInfo { Code = "140900", Name = "忻州市" },
                new CityInfo { Code = "141000", Name = "临汾市" },
                new CityInfo { Code = "141100", Name = "吕梁市" }
            }}},
            { "150000", new ProvinceInfo { Code = "150000", Name = "内蒙古自治区", ShortName = "内蒙古", Cities = new List<CityInfo> {
                new CityInfo { Code = "150100", Name = "呼和浩特市" },
                new CityInfo { Code = "150200", Name = "包头市" },
                new CityInfo { Code = "150300", Name = "乌海市" },
                new CityInfo { Code = "150400", Name = "赤峰市" },
                new CityInfo { Code = "150500", Name = "通辽市" },
                new CityInfo { Code = "150600", Name = "鄂尔多斯市" },
                new CityInfo { Code = "150700", Name = "呼伦贝尔市" },
                new CityInfo { Code = "150800", Name = "巴彦淖尔市" },
                new CityInfo { Code = "150900", Name = "乌兰察布市" }
            }}},
            { "210000", new ProvinceInfo { Code = "210000", Name = "辽宁省", ShortName = "辽宁", Cities = new List<CityInfo> {
                new CityInfo { Code = "210100", Name = "沈阳市" },
                new CityInfo { Code = "210200", Name = "大连市" },
                new CityInfo { Code = "210300", Name = "鞍山市" },
                new CityInfo { Code = "210400", Name = "抚顺市" },
                new CityInfo { Code = "210500", Name = "本溪市" },
                new CityInfo { Code = "210600", Name = "丹东市" },
                new CityInfo { Code = "210700", Name = "锦州市" },
                new CityInfo { Code = "210800", Name = "营口市" },
                new CityInfo { Code = "210900", Name = "阜新市" },
                new CityInfo { Code = "211000", Name = "辽阳市" },
                new CityInfo { Code = "211100", Name = "盘锦市" },
                new CityInfo { Code = "211200", Name = "铁岭市" },
                new CityInfo { Code = "211300", Name = "朝阳市" },
                new CityInfo { Code = "211400", Name = "葫芦岛市" }
            }}},
            { "220000", new ProvinceInfo { Code = "220000", Name = "吉林省", ShortName = "吉林", Cities = new List<CityInfo> {
                new CityInfo { Code = "220100", Name = "长春市" },
                new CityInfo { Code = "220200", Name = "吉林市" },
                new CityInfo { Code = "220300", Name = "四平市" },
                new CityInfo { Code = "220400", Name = "辽源市" },
                new CityInfo { Code = "220500", Name = "通化市" },
                new CityInfo { Code = "220600", Name = "白山市" },
                new CityInfo { Code = "220700", Name = "松原市" },
                new CityInfo { Code = "220800", Name = "白城市" }
            }}},
            { "230000", new ProvinceInfo { Code = "230000", Name = "黑龙江省", ShortName = "黑龙江", Cities = new List<CityInfo> {
                new CityInfo { Code = "230100", Name = "哈尔滨市" },
                new CityInfo { Code = "230200", Name = "齐齐哈尔市" },
                new CityInfo { Code = "230300", Name = "鸡西市" },
                new CityInfo { Code = "230400", Name = "鹤岗市" },
                new CityInfo { Code = "230500", Name = "双鸭山市" },
                new CityInfo { Code = "230600", Name = "大庆市" },
                new CityInfo { Code = "230700", Name = "伊春市" },
                new CityInfo { Code = "230800", Name = "佳木斯市" },
                new CityInfo { Code = "230900", Name = "七台河市" },
                new CityInfo { Code = "231000", Name = "牡丹江市" },
                new CityInfo { Code = "231100", Name = "黑河市" },
                new CityInfo { Code = "231200", Name = "绥化市" }
            }}},
            { "310000", new ProvinceInfo { Code = "310000", Name = "上海市", ShortName = "上海", Cities = new List<CityInfo> {
                new CityInfo { Code = "310100", Name = "上海市" }
            }}},
            { "320000", new ProvinceInfo { Code = "320000", Name = "江苏省", ShortName = "江苏", Cities = new List<CityInfo> {
                new CityInfo { Code = "320100", Name = "南京市" },
                new CityInfo { Code = "320200", Name = "无锡市" },
                new CityInfo { Code = "320300", Name = "徐州市" },
                new CityInfo { Code = "320400", Name = "常州市" },
                new CityInfo { Code = "320500", Name = "苏州市" },
                new CityInfo { Code = "320600", Name = "南通市" },
                new CityInfo { Code = "320700", Name = "连云港市" },
                new CityInfo { Code = "320800", Name = "淮安市" },
                new CityInfo { Code = "320900", Name = "盐城市" },
                new CityInfo { Code = "321000", Name = "扬州市" },
                new CityInfo { Code = "321100", Name = "镇江市" },
                new CityInfo { Code = "321200", Name = "泰州市" },
                new CityInfo { Code = "321300", Name = "宿迁市" }
            }}},
            { "330000", new ProvinceInfo { Code = "330000", Name = "浙江省", ShortName = "浙江", Cities = new List<CityInfo> {
                new CityInfo { Code = "330100", Name = "杭州市" },
                new CityInfo { Code = "330200", Name = "宁波市" },
                new CityInfo { Code = "330300", Name = "温州市" },
                new CityInfo { Code = "330400", Name = "嘉兴市" },
                new CityInfo { Code = "330500", Name = "湖州市" },
                new CityInfo { Code = "330600", Name = "绍兴市" },
                new CityInfo { Code = "330700", Name = "金华市" },
                new CityInfo { Code = "330800", Name = "衢州市" },
                new CityInfo { Code = "330900", Name = "舟山市" },
                new CityInfo { Code = "331000", Name = "台州市" },
                new CityInfo { Code = "331100", Name = "丽水市" }
            }}},
            { "340000", new ProvinceInfo { Code = "340000", Name = "安徽省", ShortName = "安徽", Cities = new List<CityInfo> {
                new CityInfo { Code = "340100", Name = "合肥市" },
                new CityInfo { Code = "340200", Name = "芜湖市" },
                new CityInfo { Code = "340300", Name = "蚌埠市" },
                new CityInfo { Code = "340400", Name = "淮南市" },
                new CityInfo { Code = "340500", Name = "马鞍山市" },
                new CityInfo { Code = "340600", Name = "淮北市" },
                new CityInfo { Code = "340700", Name = "铜陵市" },
                new CityInfo { Code = "340800", Name = "安庆市" },
                new CityInfo { Code = "341000", Name = "黄山市" },
                new CityInfo { Code = "341100", Name = "滁州市" },
                new CityInfo { Code = "341200", Name = "阜阳市" },
                new CityInfo { Code = "341300", Name = "宿州市" },
                new CityInfo { Code = "341500", Name = "六安市" },
                new CityInfo { Code = "341600", Name = "亳州市" },
                new CityInfo { Code = "341700", Name = "池州市" },
                new CityInfo { Code = "341800", Name = "宣城市" }
            }}},
            { "350000", new ProvinceInfo { Code = "350000", Name = "福建省", ShortName = "福建", Cities = new List<CityInfo> {
                new CityInfo { Code = "350100", Name = "福州市" },
                new CityInfo { Code = "350200", Name = "厦门市" },
                new CityInfo { Code = "350300", Name = "莆田市" },
                new CityInfo { Code = "350400", Name = "三明市" },
                new CityInfo { Code = "350500", Name = "泉州市" },
                new CityInfo { Code = "350600", Name = "漳州市" },
                new CityInfo { Code = "350700", Name = "南平市" },
                new CityInfo { Code = "350800", Name = "龙岩市" },
                new CityInfo { Code = "350900", Name = "宁德市" }
            }}},
            { "360000", new ProvinceInfo { Code = "360000", Name = "江西省", ShortName = "江西", Cities = new List<CityInfo> {
                new CityInfo { Code = "360100", Name = "南昌市" },
                new CityInfo { Code = "360200", Name = "景德镇市" },
                new CityInfo { Code = "360300", Name = "萍乡市" },
                new CityInfo { Code = "360400", Name = "九江市" },
                new CityInfo { Code = "360500", Name = "新余市" },
                new CityInfo { Code = "360600", Name = "鹰潭市" },
                new CityInfo { Code = "360700", Name = "赣州市" },
                new CityInfo { Code = "360800", Name = "吉安市" },
                new CityInfo { Code = "360900", Name = "宜春市" },
                new CityInfo { Code = "361000", Name = "抚州市" },
                new CityInfo { Code = "361100", Name = "上饶市" }
            }}},
            { "370000", new ProvinceInfo { Code = "370000", Name = "山东省", ShortName = "山东", Cities = new List<CityInfo> {
                new CityInfo { Code = "370100", Name = "济南市" },
                new CityInfo { Code = "370200", Name = "青岛市" },
                new CityInfo { Code = "370300", Name = "淄博市" },
                new CityInfo { Code = "370400", Name = "枣庄市" },
                new CityInfo { Code = "370500", Name = "东营市" },
                new CityInfo { Code = "370600", Name = "烟台市" },
                new CityInfo { Code = "370700", Name = "潍坊市" },
                new CityInfo { Code = "370800", Name = "济宁市" },
                new CityInfo { Code = "370900", Name = "泰安市" },
                new CityInfo { Code = "371000", Name = "威海市" },
                new CityInfo { Code = "371100", Name = "日照市" },
                new CityInfo { Code = "371300", Name = "临沂市" },
                new CityInfo { Code = "371400", Name = "德州市" },
                new CityInfo { Code = "371500", Name = "聊城市" },
                new CityInfo { Code = "371600", Name = "滨州市" },
                new CityInfo { Code = "371700", Name = "菏泽市" }
            }}},
            { "410000", new ProvinceInfo { Code = "410000", Name = "河南省", ShortName = "河南", Cities = new List<CityInfo> {
                new CityInfo { Code = "410100", Name = "郑州市" },
                new CityInfo { Code = "410200", Name = "开封市" },
                new CityInfo { Code = "410300", Name = "洛阳市" },
                new CityInfo { Code = "410400", Name = "平顶山市" },
                new CityInfo { Code = "410500", Name = "安阳市" },
                new CityInfo { Code = "410600", Name = "鹤壁市" },
                new CityInfo { Code = "410700", Name = "新乡市" },
                new CityInfo { Code = "410800", Name = "焦作市" },
                new CityInfo { Code = "410900", Name = "濮阳市" },
                new CityInfo { Code = "411000", Name = "许昌市" },
                new CityInfo { Code = "411100", Name = "漯河市" },
                new CityInfo { Code = "411200", Name = "三门峡市" },
                new CityInfo { Code = "411300", Name = "南阳市" },
                new CityInfo { Code = "411400", Name = "商丘市" },
                new CityInfo { Code = "411500", Name = "信阳市" },
                new CityInfo { Code = "411600", Name = "周口市" },
                new CityInfo { Code = "411700", Name = "驻马店市" }
            }}},
            { "420000", new ProvinceInfo { Code = "420000", Name = "湖北省", ShortName = "湖北", Cities = new List<CityInfo> {
                new CityInfo { Code = "420100", Name = "武汉市" },
                new CityInfo { Code = "420200", Name = "黄石市" },
                new CityInfo { Code = "420300", Name = "十堰市" },
                new CityInfo { Code = "420500", Name = "宜昌市" },
                new CityInfo { Code = "420600", Name = "襄阳市" },
                new CityInfo { Code = "420700", Name = "鄂州市" },
                new CityInfo { Code = "420800", Name = "荆门市" },
                new CityInfo { Code = "420900", Name = "孝感市" },
                new CityInfo { Code = "421000", Name = "荆州市" },
                new CityInfo { Code = "421100", Name = "黄冈市" },
                new CityInfo { Code = "421200", Name = "咸宁市" },
                new CityInfo { Code = "421300", Name = "随州市" }
            }}},
            { "430000", new ProvinceInfo { Code = "430000", Name = "湖南省", ShortName = "湖南", Cities = new List<CityInfo> {
                new CityInfo { Code = "430100", Name = "长沙市" },
                new CityInfo { Code = "430200", Name = "株洲市" },
                new CityInfo { Code = "430300", Name = "湘潭市" },
                new CityInfo { Code = "430400", Name = "衡阳市" },
                new CityInfo { Code = "430500", Name = "邵阳市" },
                new CityInfo { Code = "430600", Name = "岳阳市" },
                new CityInfo { Code = "430700", Name = "常德市" },
                new CityInfo { Code = "430800", Name = "张家界市" },
                new CityInfo { Code = "430900", Name = "益阳市" },
                new CityInfo { Code = "431000", Name = "郴州市" },
                new CityInfo { Code = "431100", Name = "永州市" },
                new CityInfo { Code = "431200", Name = "怀化市" },
                new CityInfo { Code = "431300", Name = "娄底市" }
            }}},
            { "440000", new ProvinceInfo { Code = "440000", Name = "广东省", ShortName = "广东", Cities = new List<CityInfo> {
                new CityInfo { Code = "440100", Name = "广州市" },
                new CityInfo { Code = "440200", Name = "韶关市" },
                new CityInfo { Code = "440300", Name = "深圳市" },
                new CityInfo { Code = "440400", Name = "珠海市" },
                new CityInfo { Code = "440500", Name = "汕头市" },
                new CityInfo { Code = "440600", Name = "佛山市" },
                new CityInfo { Code = "440700", Name = "江门市" },
                new CityInfo { Code = "440800", Name = "湛江市" },
                new CityInfo { Code = "440900", Name = "茂名市" },
                new CityInfo { Code = "441200", Name = "肇庆市" },
                new CityInfo { Code = "441300", Name = "惠州市" },
                new CityInfo { Code = "441400", Name = "梅州市" },
                new CityInfo { Code = "441500", Name = "汕尾市" },
                new CityInfo { Code = "441600", Name = "河源市" },
                new CityInfo { Code = "441700", Name = "阳江市" },
                new CityInfo { Code = "441800", Name = "清远市" },
                new CityInfo { Code = "441900", Name = "东莞市" },
                new CityInfo { Code = "442000", Name = "中山市" },
                new CityInfo { Code = "445100", Name = "潮州市" },
                new CityInfo { Code = "445200", Name = "揭阳市" },
                new CityInfo { Code = "445300", Name = "云浮市" }
            }}},
            { "450000", new ProvinceInfo { Code = "450000", Name = "广西壮族自治区", ShortName = "广西", Cities = new List<CityInfo> {
                new CityInfo { Code = "450100", Name = "南宁市" },
                new CityInfo { Code = "450200", Name = "柳州市" },
                new CityInfo { Code = "450300", Name = "桂林市" },
                new CityInfo { Code = "450400", Name = "梧州市" },
                new CityInfo { Code = "450500", Name = "北海市" },
                new CityInfo { Code = "450600", Name = "防城港市" },
                new CityInfo { Code = "450700", Name = "钦州市" },
                new CityInfo { Code = "450800", Name = "贵港市" },
                new CityInfo { Code = "450900", Name = "玉林市" },
                new CityInfo { Code = "451000", Name = "百色市" },
                new CityInfo { Code = "451100", Name = "贺州市" },
                new CityInfo { Code = "451200", Name = "河池市" },
                new CityInfo { Code = "451300", Name = "来宾市" },
                new CityInfo { Code = "451400", Name = "崇左市" }
            }}},
            { "460000", new ProvinceInfo { Code = "460000", Name = "海南省", ShortName = "海南", Cities = new List<CityInfo> {
                new CityInfo { Code = "460100", Name = "海口市" },
                new CityInfo { Code = "460200", Name = "三亚市" },
                new CityInfo { Code = "460300", Name = "三沙市" },
                new CityInfo { Code = "460400", Name = "儋州市" }
            }}},
            { "500000", new ProvinceInfo { Code = "500000", Name = "重庆市", ShortName = "重庆", Cities = new List<CityInfo> {
                new CityInfo { Code = "500100", Name = "重庆市" }
            }}},
            { "510000", new ProvinceInfo { Code = "510000", Name = "四川省", ShortName = "四川", Cities = new List<CityInfo> {
                new CityInfo { Code = "510100", Name = "成都市" },
                new CityInfo { Code = "510300", Name = "自贡市" },
                new CityInfo { Code = "510400", Name = "攀枝花市" },
                new CityInfo { Code = "510500", Name = "泸州市" },
                new CityInfo { Code = "510600", Name = "德阳市" },
                new CityInfo { Code = "510700", Name = "绵阳市" },
                new CityInfo { Code = "510800", Name = "广元市" },
                new CityInfo { Code = "510900", Name = "遂宁市" },
                new CityInfo { Code = "511000", Name = "内江市" },
                new CityInfo { Code = "511100", Name = "乐山市" },
                new CityInfo { Code = "511300", Name = "南充市" },
                new CityInfo { Code = "511400", Name = "眉山市" },
                new CityInfo { Code = "511500", Name = "宜宾市" },
                new CityInfo { Code = "511600", Name = "广安市" },
                new CityInfo { Code = "511700", Name = "达州市" },
                new CityInfo { Code = "511800", Name = "雅安市" },
                new CityInfo { Code = "511900", Name = "巴中市" },
                new CityInfo { Code = "512000", Name = "资阳市" }
            }}},
            { "520000", new ProvinceInfo { Code = "520000", Name = "贵州省", ShortName = "贵州", Cities = new List<CityInfo> {
                new CityInfo { Code = "520100", Name = "贵阳市" },
                new CityInfo { Code = "520200", Name = "六盘水市" },
                new CityInfo { Code = "520300", Name = "遵义市" },
                new CityInfo { Code = "520400", Name = "安顺市" },
                new CityInfo { Code = "520500", Name = "毕节市" },
                new CityInfo { Code = "520600", Name = "铜仁市" }
            }}},
            { "530000", new ProvinceInfo { Code = "530000", Name = "云南省", ShortName = "云南", Cities = new List<CityInfo> {
                new CityInfo { Code = "530100", Name = "昆明市" },
                new CityInfo { Code = "530300", Name = "曲靖市" },
                new CityInfo { Code = "530400", Name = "玉溪市" },
                new CityInfo { Code = "530500", Name = "保山市" },
                new CityInfo { Code = "530600", Name = "昭通市" },
                new CityInfo { Code = "530700", Name = "丽江市" },
                new CityInfo { Code = "530800", Name = "普洱市" },
                new CityInfo { Code = "530900", Name = "临沧市" }
            }}},
            { "540000", new ProvinceInfo { Code = "540000", Name = "西藏自治区", ShortName = "西藏", Cities = new List<CityInfo> {
                new CityInfo { Code = "540100", Name = "拉萨市" },
                new CityInfo { Code = "540200", Name = "日喀则市" },
                new CityInfo { Code = "540300", Name = "昌都市" },
                new CityInfo { Code = "540400", Name = "林芝市" },
                new CityInfo { Code = "540500", Name = "山南市" },
                new CityInfo { Code = "540600", Name = "那曲市" }
            }}},
            { "610000", new ProvinceInfo { Code = "610000", Name = "陕西省", ShortName = "陕西", Cities = new List<CityInfo> {
                new CityInfo { Code = "610100", Name = "西安市" },
                new CityInfo { Code = "610200", Name = "铜川市" },
                new CityInfo { Code = "610300", Name = "宝鸡市" },
                new CityInfo { Code = "610400", Name = "咸阳市" },
                new CityInfo { Code = "610500", Name = "渭南市" },
                new CityInfo { Code = "610600", Name = "延安市" },
                new CityInfo { Code = "610700", Name = "汉中市" },
                new CityInfo { Code = "610800", Name = "榆林市" },
                new CityInfo { Code = "610900", Name = "安康市" },
                new CityInfo { Code = "611000", Name = "商洛市" }
            }}},
            { "620000", new ProvinceInfo { Code = "620000", Name = "甘肃省", ShortName = "甘肃", Cities = new List<CityInfo> {
                new CityInfo { Code = "620100", Name = "兰州市" },
                new CityInfo { Code = "620200", Name = "嘉峪关市" },
                new CityInfo { Code = "620300", Name = "金昌市" },
                new CityInfo { Code = "620400", Name = "白银市" },
                new CityInfo { Code = "620500", Name = "天水市" },
                new CityInfo { Code = "620600", Name = "武威市" },
                new CityInfo { Code = "620700", Name = "张掖市" },
                new CityInfo { Code = "620800", Name = "平凉市" },
                new CityInfo { Code = "620900", Name = "酒泉市" },
                new CityInfo { Code = "621000", Name = "庆阳市" },
                new CityInfo { Code = "621100", Name = "定西市" },
                new CityInfo { Code = "621200", Name = "陇南市" }
            }}},
            { "630000", new ProvinceInfo { Code = "630000", Name = "青海省", ShortName = "青海", Cities = new List<CityInfo> {
                new CityInfo { Code = "630100", Name = "西宁市" },
                new CityInfo { Code = "630200", Name = "海东市" }
            }}},
            { "640000", new ProvinceInfo { Code = "640000", Name = "宁夏回族自治区", ShortName = "宁夏", Cities = new List<CityInfo> {
                new CityInfo { Code = "640100", Name = "银川市" },
                new CityInfo { Code = "640200", Name = "石嘴山市" },
                new CityInfo { Code = "640300", Name = "吴忠市" },
                new CityInfo { Code = "640400", Name = "固原市" },
                new CityInfo { Code = "640500", Name = "中卫市" }
            }}},
            { "650000", new ProvinceInfo { Code = "650000", Name = "新疆维吾尔自治区", ShortName = "新疆", Cities = new List<CityInfo> {
                new CityInfo { Code = "650100", Name = "乌鲁木齐市" },
                new CityInfo { Code = "650200", Name = "克拉玛依市" }
            }}},
            { "710000", new ProvinceInfo { Code = "710000", Name = "台湾省", ShortName = "台湾", Cities = new List<CityInfo>() }},
            { "810000", new ProvinceInfo { Code = "810000", Name = "香港特别行政区", ShortName = "香港", Cities = new List<CityInfo>() }},
            { "820000", new ProvinceInfo { Code = "820000", Name = "澳门特别行政区", ShortName = "澳门", Cities = new List<CityInfo>() }}
        };

        /// <summary>
        /// 根据省份代码获取省份信息
        /// </summary>
        public static ProvinceInfo? GetProvinceByCode(string? code)
        {
            if (string.IsNullOrWhiteSpace(code) || code.Length < 2)
                return null;

            var provinceCode = code.Substring(0, 2) + "0000";
            return Provinces.TryGetValue(provinceCode, out var province) ? province : null;
        }

        /// <summary>
        /// 根据省份名称获取省份信息
        /// </summary>
        public static ProvinceInfo? GetProvinceByName(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            foreach (var province in Provinces.Values)
            {
                if (province.Name == name || province.ShortName == name ||
                    province.Name.Contains(name) || name.Contains(province.ShortName))
                {
                    return province;
                }
            }

            return null;
        }

        /// <summary>
        /// 根据城市代码获取城市信息
        /// </summary>
        public static CityInfo? GetCityByCode(string? code)
        {
            if (string.IsNullOrWhiteSpace(code) || code.Length < 4)
                return null;

            var provinceCode = code.Substring(0, 2) + "0000";
            if (!Provinces.TryGetValue(provinceCode, out var province))
                return null;

            foreach (var city in province.Cities)
            {
                if (city.Code == code)
                    return city;
            }

            return null;
        }

        /// <summary>
        /// 根据城市名称获取城市信息
        /// </summary>
        public static CityInfo? GetCityByName(string? name, string? provinceName = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            foreach (var province in Provinces.Values)
            {
                if (!string.IsNullOrEmpty(provinceName) && 
                    province.Name != provinceName && 
                    province.ShortName != provinceName)
                    continue;

                foreach (var city in province.Cities)
                {
                    if (city.Name == name || city.Name.Contains(name) || name.Contains(city.Name.Replace("市", "")))
                    {
                        return city;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 获取所有省份
        /// </summary>
        public static IEnumerable<ProvinceInfo> GetAllProvinces()
        {
            return Provinces.Values;
        }

        /// <summary>
        /// 获取省份下的所有城市
        /// </summary>
        public static IEnumerable<CityInfo> GetCitiesByProvinceCode(string? provinceCode)
        {
            var province = GetProvinceByCode(provinceCode);
            return province?.Cities ?? Enumerable.Empty<CityInfo>();
        }

        /// <summary>
        /// 验证行政区划代码是否有效
        /// </summary>
        public static bool IsValidCode(string? code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return false;

            code = code.Trim();
            if (code.Length != 6)
                return false;

            foreach (var c in code)
            {
                if (!char.IsDigit(c))
                    return false;
            }

            var provinceCode = code.Substring(0, 2) + "0000";
            return Provinces.ContainsKey(provinceCode);
        }

        /// <summary>
        /// 根据身份证号前6位获取籍贯
        /// </summary>
        public static string? GetNativePlace(string? idCardPrefix)
        {
            if (string.IsNullOrWhiteSpace(idCardPrefix) || idCardPrefix.Length < 6)
                return null;

            var provinceCode = idCardPrefix.Substring(0, 2) + "0000";
            if (!Provinces.TryGetValue(provinceCode, out var province))
                return null;

            var cityCode = idCardPrefix.Substring(0, 4) + "00";
            foreach (var city in province.Cities)
            {
                if (city.Code == cityCode)
                {
                    return $"{province.Name}{city.Name}";
                }
            }

            return province.Name;
        }
    }

    #region 数据类

    /// <summary>
    /// 省份信息
    /// </summary>
    public class ProvinceInfo
    {
        /// <summary>
        /// 省份代码（6位）
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// 省份名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 简称
        /// </summary>
        public string ShortName { get; set; } = string.Empty;

        /// <summary>
        /// 城市列表
        /// </summary>
        public List<CityInfo> Cities { get; set; } = new();

        public override string ToString() => Name;
    }

    /// <summary>
    /// 城市信息
    /// </summary>
    public class CityInfo
    {
        /// <summary>
        /// 城市代码（6位）
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// 城市名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        public override string ToString() => Name;
    }

    #endregion
}
