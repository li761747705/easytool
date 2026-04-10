using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace EasyTool.BusinessCategory
{
    /// <summary>
    /// 固定电话工具类
    /// </summary>
    public static class PhoneUtil
    {
        #region 常量与私有字段

        /// <summary>
        /// 中国大陆固定电话正则表达式（带区号）
        /// </summary>
        private static readonly Regex PhoneWithAreaCodeRegex = new(
            @"^(0\d{2,3}[-\s]?)?\d{7,8}$",
            RegexOptions.Compiled);

        /// <summary>
        /// 中国大陆固定电话正则表达式（完整格式）
        /// </summary>
        private static readonly Regex PhoneFullRegex = new(
            @"^0\d{2,3}[-\s]?\d{7,8}$",
            RegexOptions.Compiled);

        /// <summary>
        /// 400电话正则表达式
        /// </summary>
        private static readonly Regex Phone400Regex = new(
            @"^400[-\s]?\d{3}[-\s]?\d{4}$",
            RegexOptions.Compiled);

        /// <summary>
        /// 800电话正则表达式
        /// </summary>
        private static readonly Regex Phone800Regex = new(
            @"^800[-\s]?\d{3}[-\s]?\d{4}$",
            RegexOptions.Compiled);

        /// <summary>
        /// 非数字字符正则表达式
        /// </summary>
        private static readonly Regex NonDigitRegex = new(@"[^\d]", RegexOptions.Compiled);

        /// <summary>
        /// 区号与城市映射
        /// </summary>
        private static readonly Dictionary<string, string> AreaCodeMap = new()
        {
            // 直辖市
            { "010", "北京" }, { "021", "上海" }, { "022", "天津" }, { "023", "重庆" },

            // 省会城市
            { "0311", "石家庄" }, { "0351", "太原" }, { "0471", "呼和浩特" },
            { "024", "沈阳" }, { "0431", "长春" }, { "0451", "哈尔滨" },
            { "025", "南京" }, { "0571", "杭州" }, { "0551", "合肥" },
            { "0591", "福州" }, { "0791", "南昌" }, { "0531", "济南" },
            { "0371", "郑州" }, { "027", "武汉" }, { "0731", "长沙" },
            { "020", "广州" }, { "0771", "南宁" }, { "0898", "海口" },
            { "028", "成都" }, { "0851", "贵阳" }, { "0871", "昆明" },
            { "0891", "拉萨" }, { "029", "西安" }, { "0931", "兰州" },
            { "0971", "西宁" }, { "0951", "银川" }, { "0991", "乌鲁木齐" },

            // 重要城市
            { "0755", "深圳" }, { "0756", "珠海" }, { "0754", "汕头" },
            { "0757", "佛山" }, { "0769", "东莞" }, { "0760", "中山" },
            { "0512", "苏州" }, { "0510", "无锡" }, { "0574", "宁波" },
            { "0577", "温州" }, { "0532", "青岛" }, { "0411", "大连" },
            { "0592", "厦门" }, { "0514", "扬州" }, { "0519", "常州" },
            { "0573", "嘉兴" }, { "0575", "绍兴" }, { "0576", "台州" },
            { "0579", "金华" }, { "0752", "惠州" }, { "0753", "梅州" },
            { "0758", "肇庆" }, { "0759", "湛江" }, { "0762", "河源" },
            { "0763", "清远" }, { "0766", "云浮" }, { "0768", "潮州" },
            { "0773", "桂林" }, { "0774", "梧州" }, { "0775", "玉林" },
            { "0779", "北海" }, { "0772", "柳州" }, { "0778", "河池" },
            { "0733", "株洲" }, { "0734", "衡阳" }, { "0735", "郴州" },
            { "0737", "益阳" }, { "0738", "娄底" }, { "0739", "邵阳" },
            { "0792", "九江" }, { "0793", "上饶" }, { "0795", "宜春" },
            { "0796", "吉安" }, { "0797", "赣州" }, { "0799", "萍乡" },
            { "0533", "淄博" }, { "0534", "德州" }, { "0535", "烟台" },
            { "0536", "潍坊" }, { "0537", "济宁" }, { "0538", "泰安" },
            { "0539", "临沂" }, { "0543", "滨州" }, { "0546", "东营" },
            { "0379", "洛阳" }, { "0378", "开封" }, { "0372", "安阳" },
            { "0373", "新乡" }, { "0374", "许昌" }, { "0375", "平顶山" },
            { "0370", "商丘" }, { "0391", "焦作" }, { "0393", "濮阳" },
            { "0395", "漯河" }, { "0396", "驻马店" }, { "0398", "三门峡" },
            { "0376", "信阳" }, { "0377", "南阳" }, { "0392", "鹤壁" },
            { "027", "武汉" }, { "0710", "襄阳" }, { "0711", "鄂州" },
            { "0712", "孝感" }, { "0713", "黄冈" }, { "0714", "黄石" },
            { "0715", "咸宁" }, { "0716", "荆州" }, { "0717", "宜昌" },
            { "0718", "恩施" }, { "0719", "十堰" }, { "0722", "随州" },
            { "0724", "荆门" }, { "0728", "仙桃" }, { "0730", "岳阳" },

            // 三位区号
            { "0310", "邯郸" }, { "0312", "保定" }, { "0313", "张家口" },
            { "0314", "承德" }, { "0315", "唐山" }, { "0316", "廊坊" },
            { "0317", "沧州" }, { "0318", "衡水" }, { "0319", "邢台" },
            { "0335", "秦皇岛" }, { "0349", "朔州" }, { "0350", "忻州" },
            { "0352", "大同" }, { "0353", "阳泉" }, { "0354", "晋中" },
            { "0355", "长治" }, { "0356", "晋城" }, { "0357", "临汾" },
            { "0358", "吕梁" }, { "0359", "运城" }, { "0410", "铁岭" },
            { "0412", "鞍山" }, { "0413", "抚顺" }, { "0414", "本溪" },
            { "0415", "丹东" }, { "0416", "锦州" }, { "0417", "营口" },
            { "0418", "阜新" }, { "0419", "辽阳" }, { "0421", "朝阳" },
            { "0427", "盘锦" }, { "0429", "葫芦岛" }, { "0432", "吉林市" },
            { "0433", "延边" }, { "0434", "四平" }, { "0435", "通化" },
            { "0436", "白城" }, { "0437", "辽源" }, { "0439", "白山" },
            { "0438", "松原" }, { "0452", "齐齐哈尔" }, { "0453", "牡丹江" },
            { "0454", "佳木斯" }, { "0455", "绥化" }, { "0456", "黑河" },
            { "0457", "大兴安岭" }, { "0458", "伊春" }, { "0459", "大庆" },
            { "0464", "七台河" }, { "0467", "鸡西" }, { "0468", "鹤岗" },
            { "0469", "双鸭山" }, { "0470", "呼伦贝尔" }, { "0472", "包头" },
            { "0473", "乌海" }, { "0474", "乌兰察布" }, { "0475", "通辽" },
            { "0476", "赤峰" }, { "0477", "鄂尔多斯" }, { "0478", "巴彦淖尔" },
            { "0479", "锡林郭勒" }, { "0482", "兴安盟" }, { "0483", "阿拉善" }
        };

        #endregion

        #region 验证方法

        /// <summary>
        /// 验证固定电话是否有效
        /// </summary>
        /// <param name="phone">固定电话号码</param>
        /// <returns>是否有效</returns>
        public static bool IsValid(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                return false;
            }

            return PhoneWithAreaCodeRegex.IsMatch(phone) ||
                   Is400Phone(phone) || Is800Phone(phone);
        }

        /// <summary>
        /// 验证是否为带区号的固定电话
        /// </summary>
        /// <param name="phone">电话号码</param>
        /// <returns>是否为固定电话</returns>
        public static bool IsLandline(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                return false;
            }

            return PhoneFullRegex.IsMatch(phone);
        }

        /// <summary>
        /// 验证是否为400电话
        /// </summary>
        /// <param name="phone">电话号码</param>
        /// <returns>是否为400电话</returns>
        public static bool Is400Phone(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                return false;
            }

            return Phone400Regex.IsMatch(phone);
        }

        /// <summary>
        /// 验证是否为800电话
        /// </summary>
        /// <param name="phone">电话号码</param>
        /// <returns>是否为800电话</returns>
        public static bool Is800Phone(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                return false;
            }

            return Phone800Regex.IsMatch(phone);
        }

        /// <summary>
        /// 验证区号是否有效
        /// </summary>
        /// <param name="areaCode">区号</param>
        /// <returns>是否有效</returns>
        public static bool IsValidAreaCode(string? areaCode)
        {
            if (string.IsNullOrWhiteSpace(areaCode))
            {
                return false;
            }

            string code = areaCode.TrimStart('0');
            return AreaCodeMap.ContainsKey("0" + code) || AreaCodeMap.ContainsKey(areaCode);
        }

        #endregion

        #region 信息提取

        /// <summary>
        /// 获取区号
        /// </summary>
        /// <param name="phone">电话号码</param>
        /// <returns>区号</returns>
        public static string? GetAreaCode(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                return null;
            }

            // 400/800电话无区号
            if (Is400Phone(phone) || Is800Phone(phone))
            {
                return null;
            }

            string cleaned = NonDigitRegex.Replace(phone, "");

            // 三位区号（0开头）
            if (cleaned.Length >= 10 && cleaned.StartsWith("0"))
            {
                string code3 = cleaned.Substring(0, 3);
                if (AreaCodeMap.ContainsKey(code3))
                {
                    return code3;
                }
            }

            // 四位区号（0开头）
            if (cleaned.Length >= 11 && cleaned.StartsWith("0"))
            {
                string code4 = cleaned.Substring(0, 4);
                if (AreaCodeMap.ContainsKey(code4))
                {
                    return code4;
                }
            }

            // 尝试提取前3-4位作为区号
            if (cleaned.StartsWith("0"))
            {
                for (int len = Math.Min(4, cleaned.Length - 7); len >= 3; len--)
                {
                    string code = cleaned.Substring(0, len);
                    if (AreaCodeMap.ContainsKey(code))
                    {
                        return code;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 获取城市名称
        /// </summary>
        /// <param name="phone">电话号码</param>
        /// <returns>城市名称</returns>
        public static string? GetCity(string? phone)
        {
            string? areaCode = GetAreaCode(phone);
            if (areaCode == null)
            {
                return null;
            }

            return AreaCodeMap.TryGetValue(areaCode, out string? city) ? city : null;
        }

        /// <summary>
        /// 获取本地号码（不含区号）
        /// </summary>
        /// <param name="phone">电话号码</param>
        /// <returns>本地号码</returns>
        public static string? GetLocalNumber(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                return null;
            }

            // 400/800电话
            if (Is400Phone(phone) || Is800Phone(phone))
            {
                string local = NonDigitRegex.Replace(phone, "");
                return local.Length >= 10 ? local.Substring(3) : null;
            }

            string? areaCode = GetAreaCode(phone);
            if (areaCode == null)
            {
                return null;
            }

            string cleaned = NonDigitRegex.Replace(phone, "");
            return cleaned.Substring(areaCode.Length);
        }

        /// <summary>
        /// 获取电话类型
        /// </summary>
        /// <param name="phone">电话号码</param>
        /// <returns>电话类型描述</returns>
        public static string? GetPhoneType(string? phone)
        {
            if (Is400Phone(phone)) return "400企业热线";
            if (Is800Phone(phone)) return "800免费电话";
            if (IsLandline(phone)) return "固定电话";
            return null;
        }

        #endregion

        #region 格式化方法

        /// <summary>
        /// 格式化电话号码（去除非数字字符）
        /// </summary>
        /// <param name="phone">电话号码</param>
        /// <returns>格式化后的号码</returns>
        public static string? Normalize(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                return null;
            }

            string cleaned = NonDigitRegex.Replace(phone, "");
            return cleaned.Length >= 7 ? cleaned : null;
        }

        /// <summary>
        /// 格式化为标准格式（区号-本地号码）
        /// </summary>
        /// <param name="phone">电话号码</param>
        /// <returns>格式化后的号码</returns>
        public static string? Format(string? phone)
        {
            string? normalized = Normalize(phone);
            if (normalized == null)
            {
                return null;
            }

            // 400电话
            if (normalized.StartsWith("400") && normalized.Length == 10)
            {
                return $"{normalized.Substring(0, 3)}-{normalized.Substring(3, 3)}-{normalized.Substring(6)}";
            }

            // 800电话
            if (normalized.StartsWith("800") && normalized.Length == 10)
            {
                return $"{normalized.Substring(0, 3)}-{normalized.Substring(3, 3)}-{normalized.Substring(6)}";
            }

            // 带区号的固定电话
            string? areaCode = GetAreaCode(normalized);
            if (areaCode != null)
            {
                string local = normalized.Substring(areaCode.Length);
                return $"{areaCode}-{local}";
            }

            return normalized;
        }

        /// <summary>
        /// 电话号码脱敏：010-****1234
        /// </summary>
        /// <param name="phone">电话号码</param>
        /// <returns>脱敏后的号码</returns>
        public static string? Mask(string? phone)
        {
            if (!IsValid(phone))
            {
                return null;
            }

            string? areaCode = GetAreaCode(phone);
            string? local = GetLocalNumber(phone);

            if (areaCode != null && local != null && local.Length >= 4)
            {
                int visibleSuffix = 4;
                int maskLen = local.Length - visibleSuffix;
                return $"{areaCode}-{new string('*', maskLen)}{local.Substring(maskLen)}";
            }

            // 400/800电话
            string? normalized = Normalize(phone);
            if (normalized != null && normalized.Length == 10)
            {
                return $"{normalized.Substring(0, 3)}-****{normalized.Substring(6)}";
            }

            return null;
        }

        #endregion
    }
}
