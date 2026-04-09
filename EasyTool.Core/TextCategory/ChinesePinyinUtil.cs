using System;
using System.Collections.Generic;
using System.Text;

namespace EasyTool.BusinessCategory
{
    /// <summary>
    /// 汉字拼音工具类
    /// 提供汉字转拼音、获取首字母功能
    /// </summary>
    public static class ChinesePinyinUtil
    {
        #region 拼音数据

        // 常用汉字拼音映射
        private static readonly Dictionary<char, PinyinInfo> PinyinDict = new()
        {
            // 常用汉字
            { '中', new PinyinInfo("zhong", "zhong1", 1) },
            { '国', new PinyinInfo("guo", "guo2", 2) },
            { '人', new PinyinInfo("ren", "ren2", 2) },
            { '民', new PinyinInfo("min", "min2", 2) },
            { '共', new PinyinInfo("gong", "gong4", 4) },
            { '和', new PinyinInfo("he", "he2", 2) },
            { '产', new PinyinInfo("chan", "chan3", 3) },
            { '党', new PinyinInfo("dang", "dang3", 3) },
            { '北', new PinyinInfo("bei", "bei3", 3) },
            { '京', new PinyinInfo("jing", "jing1", 1) },
            { '上', new PinyinInfo("shang", "shang4", 4) },
            { '海', new PinyinInfo("hai", "hai3", 3) },
            { '天', new PinyinInfo("tian", "tian1", 1) },
            { '地', new PinyinInfo("di", "di4", 4) },
            { '日', new PinyinInfo("ri", "ri4", 4) },
            { '月', new PinyinInfo("yue", "yue4", 4) },
            { '星', new PinyinInfo("xing", "xing1", 1) },
            { '期', new PinyinInfo("qi", "qi1", 1) },
            { '年', new PinyinInfo("nian", "nian2", 2) },
            { '时', new PinyinInfo("shi", "shi2", 2) },
            { '分', new PinyinInfo("fen", "fen1", 1) },
            { '秒', new PinyinInfo("miao", "miao3", 3) },
            { '你', new PinyinInfo("ni", "ni3", 3) },
            { '好', new PinyinInfo("hao", "hao3", 3) },
            { '我', new PinyinInfo("wo", "wo3", 3) },
            { '是', new PinyinInfo("shi", "shi4", 4) },
            { '他', new PinyinInfo("ta", "ta1", 1) },
            { '她', new PinyinInfo("ta", "ta1", 1) },
            { '它', new PinyinInfo("ta", "ta1", 1) },
            { '们', new PinyinInfo("men", "men5", 5) },
            { '的', new PinyinInfo("de", "de5", 5) },
            { '了', new PinyinInfo("le", "le5", 5) },
            { '在', new PinyinInfo("zai", "zai4", 4) },
            { '有', new PinyinInfo("you", "you3", 3) },
            { '与', new PinyinInfo("yu", "yu3", 3) },
            { '或', new PinyinInfo("huo", "huo4", 4) },
            { '但', new PinyinInfo("dan", "dan4", 4) },
            { '不', new PinyinInfo("bu", "bu4", 4) },
            { '这', new PinyinInfo("zhe", "zhe4", 4) },
            { '那', new PinyinInfo("na", "na4", 4) },
            { '也', new PinyinInfo("ye", "ye3", 3) },
            { '就', new PinyinInfo("jiu", "jiu4", 4) },
            { '都', new PinyinInfo("dou", "dou1", 1) },
            { '为', new PinyinInfo("wei", "wei2", 2) },
            { '能', new PinyinInfo("neng", "neng2", 2) },
            { '可', new PinyinInfo("ke", "ke3", 3) },
            { '以', new PinyinInfo("yi", "yi3", 3) },
            { '要', new PinyinInfo("yao", "yao4", 4) },
            { '会', new PinyinInfo("hui", "hui4", 4) },
            { '说', new PinyinInfo("shuo", "shuo1", 1) },
            { '对', new PinyinInfo("dui", "dui4", 4) },
            { '出', new PinyinInfo("chu", "chu1", 1) },
            { '来', new PinyinInfo("lai", "lai2", 2) },
            { '去', new PinyinInfo("qu", "qu4", 4) },
            { '到', new PinyinInfo("dao", "dao4", 4) },
            { '从', new PinyinInfo("cong", "cong2", 2) },
            { '向', new PinyinInfo("xiang", "xiang4", 4) },
            { '前', new PinyinInfo("qian", "qian2", 2) },
            { '后', new PinyinInfo("hou", "hou4", 4) },
            { '左', new PinyinInfo("zuo", "zuo3", 3) },
            { '右', new PinyinInfo("you", "you4", 4) },
            { '大', new PinyinInfo("da", "da4", 4) },
            { '小', new PinyinInfo("xiao", "xiao3", 3) },
            { '多', new PinyinInfo("duo", "duo1", 1) },
            { '少', new PinyinInfo("shao", "shao3", 3) },
            { '高', new PinyinInfo("gao", "gao1", 1) },
            { '低', new PinyinInfo("di", "di1", 1) },
            { '长', new PinyinInfo("chang", "chang2", 2) },
            { '短', new PinyinInfo("duan", "duan3", 3) },
            { '快', new PinyinInfo("kuai", "kuai4", 4) },
            { '慢', new PinyinInfo("man", "man4", 4) },
            { '新', new PinyinInfo("xin", "xin1", 1) },
            { '旧', new PinyinInfo("jiu", "jiu4", 4) },
            { '老', new PinyinInfo("lao", "lao3", 3) },
            { '少', new PinyinInfo("shao", "shao4", 4) },
            { '男', new PinyinInfo("nan", "nan2", 2) },
            { '女', new PinyinInfo("nv", "nv3", 3) },
            { '父', new PinyinInfo("fu", "fu4", 4) },
            { '母', new PinyinInfo("mu", "mu3", 3) },
            { '子', new PinyinInfo("zi", "zi3", 3) },
            { '学', new PinyinInfo("xue", "xue2", 2) },
            { '生', new PinyinInfo("sheng", "sheng1", 1) },
            { '师', new PinyinInfo("shi", "shi1", 1) },
            { '工', new PinyinInfo("gong", "gong1", 1) },
            { '作', new PinyinInfo("zuo", "zuo4", 4) },
            { '公', new PinyinInfo("gong", "gong1", 1) },
            { '司', new PinyinInfo("si", "si1", 1) },
            { '电', new PinyinInfo("dian", "dian4", 4) },
            { '脑', new PinyinInfo("nao", "nao3", 3) },
            { '手', new PinyinInfo("shou", "shou3", 3) },
            { '机', new PinyinInfo("ji", "ji1", 1) },
            { '网', new PinyinInfo("wang", "wang3", 3) },
            { '络', new PinyinInfo("luo", "luo4", 4) },
            { '程', new PinyinInfo("cheng", "cheng2", 2) },
            { '序', new PinyinInfo("xu", "xu4", 4) },
            { '设', new PinyinInfo("she", "she4", 4) },
            { '计', new PinyinInfo("ji", "ji4", 4) },
            { '开', new PinyinInfo("kai", "kai1", 1) },
            { '发', new PinyinInfo("fa", "fa1", 1) },
            { '测', new PinyinInfo("ce", "ce4", 4) },
            { '试', new PinyinInfo("shi", "shi4", 4) },
            { '运', new PinyinInfo("yun", "yun4", 4) },
            { '维', new PinyinInfo("wei", "wei2", 2) },
            { '品', new PinyinInfo("pin", "pin3", 3) },
            { '项', new PinyinInfo("xiang", "xiang4", 4) },
            { '目', new PinyinInfo("mu", "mu4", 4) },
            { '管', new PinyinInfo("guan", "guan3", 3) },
            { '理', new PinyinInfo("li", "li3", 3) },
            { '业', new PinyinInfo("ye", "ye4", 4) },
            { '务', new PinyinInfo("wu", "wu4", 4) },
            { '技', new PinyinInfo("ji", "ji4", 4) },
            { '术', new PinyinInfo("shu", "shu4", 4) },
            { '科', new PinyinInfo("ke", "ke1", 1) },
            { '研', new PinyinInfo("yan", "yan2", 2) },
            { '究', new PinyinInfo("jiu", "jiu1", 1) },
            // 数字
            { '一', new PinyinInfo("yi", "yi1", 1) },
            { '二', new PinyinInfo("er", "er4", 4) },
            { '三', new PinyinInfo("san", "san1", 1) },
            { '四', new PinyinInfo("si", "si4", 4) },
            { '五', new PinyinInfo("wu", "wu3", 3) },
            { '六', new PinyinInfo("liu", "liu4", 4) },
            { '七', new PinyinInfo("qi", "qi1", 1) },
            { '八', new PinyinInfo("ba", "ba1", 1) },
            { '九', new PinyinInfo("jiu", "jiu3", 3) },
            { '十', new PinyinInfo("shi", "shi2", 2) },
            { '百', new PinyinInfo("bai", "bai3", 3) },
            { '千', new PinyinInfo("qian", "qian1", 1) },
            { '万', new PinyinInfo("wan", "wan4", 4) },
            { '亿', new PinyinInfo("yi", "yi4", 4) },
            // 方位
            { '东', new PinyinInfo("dong", "dong1", 1) },
            { '西', new PinyinInfo("xi", "xi1", 1) },
            { '南', new PinyinInfo("nan", "nan2", 2) },
            { '北', new PinyinInfo("bei", "bei3", 3) },
            // 颜色
            { '红', new PinyinInfo("hong", "hong2", 2) },
            { '绿', new PinyinInfo("lv", "lv4", 4) },
            { '蓝', new PinyinInfo("lan", "lan2", 2) },
            { '黄', new PinyinInfo("huang", "huang2", 2) },
            { '白', new PinyinInfo("bai", "bai2", 2) },
            { '黑', new PinyinInfo("hei", "hei1", 1) },
            { '紫', new PinyinInfo("zi", "zi3", 3) },
            { '灰', new PinyinInfo("hui", "hui1", 1) },
            // 动物
            { '猫', new PinyinInfo("mao", "mao1", 1) },
            { '狗', new PinyinInfo("gou", "gou3", 3) },
            { '鸟', new PinyinInfo("niao", "niao3", 3) },
            { '鱼', new PinyinInfo("yu", "yu2", 2) },
            { '龙', new PinyinInfo("long", "long2", 2) },
            { '虎', new PinyinInfo("hu", "hu3", 3) },
            { '马', new PinyinInfo("ma", "ma3", 3) },
            { '牛', new PinyinInfo("niu", "niu2", 2) },
            { '羊', new PinyinInfo("yang", "yang2", 2) },
            { '猪', new PinyinInfo("zhu", "zhu1", 1) },
            // 常用姓氏
            { '王', new PinyinInfo("wang", "wang2", 2) },
            { '李', new PinyinInfo("li", "li3", 3) },
            { '张', new PinyinInfo("zhang", "zhang1", 1) },
            { '刘', new PinyinInfo("liu", "liu2", 2) },
            { '陈', new PinyinInfo("chen", "chen2", 2) },
            { '杨', new PinyinInfo("yang", "yang2", 2) },
            { '黄', new PinyinInfo("huang", "huang2", 2) },
            { '赵', new PinyinInfo("zhao", "zhao4", 4) },
            { '周', new PinyinInfo("zhou", "zhou1", 1) },
            { '吴', new PinyinInfo("wu", "wu2", 2) },
            { '徐', new PinyinInfo("xu", "xu2", 2) },
            { '孙', new PinyinInfo("sun", "sun1", 1) },
            { '朱', new PinyinInfo("zhu", "zhu1", 1) },
            { '胡', new PinyinInfo("hu", "hu2", 2) },
            { '郭', new PinyinInfo("guo", "guo1", 1) },
            { '何', new PinyinInfo("he", "he2", 2) },
            { '林', new PinyinInfo("lin", "lin2", 2) },
            { '罗', new PinyinInfo("luo", "luo2", 2) },
            { '高', new PinyinInfo("gao", "gao1", 1) }
        };

        #endregion

        #region 内部类

        private class PinyinInfo
        {
            public string Pinyin { get; }
            public string PinyinWithTone { get; }
            public int Tone { get; }

            public PinyinInfo(string pinyin, string pinyinWithTone, int tone)
            {
                Pinyin = pinyin;
                PinyinWithTone = pinyinWithTone;
                Tone = tone;
            }
        }

        #endregion

        #region 拼音转换

        /// <summary>
        /// 将汉字转换为拼音（无声调）
        /// </summary>
        /// <param name="text">汉字文本</param>
        /// <returns>拼音字符串</returns>
        public static string ToPinyin(string text)
        {
            return ToPinyin(text, " ");
        }

        /// <summary>
        /// 将汉字转换为拼音（无声调）
        /// </summary>
        /// <param name="text">汉字文本</param>
        /// <param name="separator">分隔符</param>
        /// <returns>拼音字符串</returns>
        public static string ToPinyin(string text, string separator)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            var result = new StringBuilder();
            foreach (var c in text)
            {
                if (PinyinDict.TryGetValue(c, out var info))
                {
                    result.Append(info.Pinyin);
                    result.Append(separator);
                }
                else if (char.IsLetterOrDigit(c))
                {
                    result.Append(c);
                    result.Append(separator);
                }
                else
                {
                    result.Append(c);
                }
            }

            return result.ToString().TrimEnd(separator.ToCharArray());
        }

        /// <summary>
        /// 将汉字转换为拼音（带声调数字）
        /// </summary>
        /// <param name="text">汉字文本</param>
        /// <returns>拼音字符串</returns>
        public static string ToPinyinWithTone(string text)
        {
            return ToPinyinWithTone(text, " ");
        }

        /// <summary>
        /// 将汉字转换为拼音（带声调数字）
        /// </summary>
        /// <param name="text">汉字文本</param>
        /// <param name="separator">分隔符</param>
        /// <returns>拼音字符串</returns>
        public static string ToPinyinWithTone(string text, string separator)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            var result = new StringBuilder();
            foreach (var c in text)
            {
                if (PinyinDict.TryGetValue(c, out var info))
                {
                    result.Append(info.PinyinWithTone);
                    result.Append(separator);
                }
                else if (char.IsLetterOrDigit(c))
                {
                    result.Append(c);
                    result.Append(separator);
                }
                else
                {
                    result.Append(c);
                }
            }

            return result.ToString().TrimEnd(separator.ToCharArray());
        }

        /// <summary>
        /// 获取汉字首字母
        /// </summary>
        /// <param name="text">汉字文本</param>
        /// <returns>首字母字符串</returns>
        public static string GetPinyinInitial(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            var result = new StringBuilder();
            foreach (var c in text)
            {
                if (PinyinDict.TryGetValue(c, out var info) && info.Pinyin.Length > 0)
                {
                    result.Append(char.ToUpper(info.Pinyin[0]));
                }
                else if (char.IsLetter(c))
                {
                    result.Append(char.ToUpper(c));
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// 获取单个汉字的拼音
        /// </summary>
        /// <param name="c">汉字字符</param>
        /// <returns>拼音，如果不是汉字返回null</returns>
        public static string? GetPinyin(char c)
        {
            if (PinyinDict.TryGetValue(c, out var info))
                return info.Pinyin;
            return null;
        }

        /// <summary>
        /// 获取单个汉字的拼音（带声调）
        /// </summary>
        /// <param name="c">汉字字符</param>
        /// <returns>拼音，如果不是汉字返回null</returns>
        public static string? GetPinyinWithTone(char c)
        {
            if (PinyinDict.TryGetValue(c, out var info))
                return info.PinyinWithTone;
            return null;
        }

        /// <summary>
        /// 获取单个汉字的声调
        /// </summary>
        /// <param name="c">汉字字符</param>
        /// <returns>声调（1-4，轻声为5），如果不是汉字返回-1</returns>
        public static int GetTone(char c)
        {
            if (PinyinDict.TryGetValue(c, out var info))
                return info.Tone;
            return -1;
        }

        #endregion

        #region 判断方法

        /// <summary>
        /// 判断字符是否为汉字
        /// </summary>
        /// <param name="c">字符</param>
        /// <returns>是否为汉字</returns>
        public static bool IsChinese(char c)
        {
            return c >= 0x4E00 && c <= 0x9FA5;
        }

        /// <summary>
        /// 判断字符串是否包含汉字
        /// </summary>
        /// <param name="text">文本</param>
        /// <returns>是否包含汉字</returns>
        public static bool ContainsChinese(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;

            foreach (var c in text)
            {
                if (IsChinese(c))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 判断字符串是否全为汉字
        /// </summary>
        /// <param name="text">文本</param>
        /// <returns>是否全为汉字</returns>
        public static bool IsAllChinese(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;

            foreach (var c in text)
            {
                if (!IsChinese(c))
                    return false;
            }

            return true;
        }

        #endregion

        #region 拼音数组

        /// <summary>
        /// 获取文本的拼音数组
        /// </summary>
        /// <param name="text">文本</param>
        /// <returns>拼音数组</returns>
        public static string[] ToPinyinArray(string text)
        {
            if (string.IsNullOrEmpty(text))
                return Array.Empty<string>();

            var list = new List<string>();
            foreach (var c in text)
            {
                if (PinyinDict.TryGetValue(c, out var info))
                {
                    list.Add(info.Pinyin);
                }
                else if (char.IsLetterOrDigit(c))
                {
                    list.Add(c.ToString());
                }
            }

            return list.ToArray();
        }

        #endregion
    }
}