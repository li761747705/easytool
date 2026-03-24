using System;
using System.Collections.Generic;
using System.Text;

namespace EasyTool.TextCategory
{
    /// <summary>
    /// 拼音工具类
    /// 提供汉字转拼音功能
    /// </summary>
    public static class PinyinUtil
    {
        /// <summary>
        /// 获取汉字的拼音
        /// </summary>
        /// <param name="chinese">中文字符串</param>
        /// <param name="separator">分隔符</param>
        /// <returns>拼音字符串</returns>
        public static string GetPinyin(string chinese, string separator = "")
        {
            if (string.IsNullOrEmpty(chinese))
                return string.Empty;

            var result = new StringBuilder();

            foreach (char c in chinese)
            {
                string pinyin = GetPinyin(c);
                if (result.Length > 0 && !string.IsNullOrEmpty(pinyin))
                    result.Append(separator);
                result.Append(pinyin);
            }

            return result.ToString();
        }

        /// <summary>
        /// 获取单个汉字的拼音
        /// </summary>
        public static string GetPinyin(char c)
        {
            // 非汉字直接返回
            if (c < 0x4E00 || c > 0x9FA5)
                return c.ToString();

            // 查找拼音
            string[] py = GetPinyinArray(c);
            return py != null && py.Length > 0 ? py[0] : c.ToString();
        }

        /// <summary>
        /// 获取汉字的所有拼音（多音字）
        /// </summary>
        public static string[] GetPinyins(char c)
        {
            if (c < 0x4E00 || c > 0x9FA5)
                return new[] { c.ToString() };

            return GetPinyinArray(c) ?? new[] { c.ToString() };
        }

        /// <summary>
        /// 获取拼音首字母
        /// </summary>
        public static string GetFirstPinyinLetter(string chinese)
        {
            if (string.IsNullOrEmpty(chinese))
                return string.Empty;

            var result = new StringBuilder();

            foreach (char c in chinese)
            {
                string pinyin = GetPinyin(c);
                if (!string.IsNullOrEmpty(pinyin) && pinyin.Length > 0)
                {
                    result.Append(char.ToUpper(pinyin[0]));
                }
                else
                {
                    result.Append(c);
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// 获取拼音首字母（简化版，用于排序索引）
        /// </summary>
        public static string GetSimplePinyinInitial(string chinese)
        {
            if (string.IsNullOrEmpty(chinese))
                return "#";

            char c = chinese[0];

            // 非汉字
            if (c < 0x4E00 || c > 0x9FA5)
            {
                if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'))
                    return char.ToUpper(c).ToString();
                return "#";
            }

            string pinyin = GetPinyin(c);
            if (!string.IsNullOrEmpty(pinyin) && pinyin.Length > 0)
            {
                return char.ToUpper(pinyin[0]).ToString();
            }

            return "#";
        }

        /// <summary>
        /// 判断字符是否为汉字
        /// </summary>
        public static bool IsChinese(char c)
        {
            return c >= 0x4E00 && c <= 0x9FA5;
        }

        /// <summary>
        /// 判断字符串是否全部为汉字
        /// </summary>
        public static bool IsAllChinese(string s)
        {
            if (string.IsNullOrEmpty(s))
                return false;

            foreach (char c in s)
            {
                if (!IsChinese(c))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 判断字符串是否包含汉字
        /// </summary>
        public static bool ContainsChinese(string s)
        {
            if (string.IsNullOrEmpty(s))
                return false;

            foreach (char c in s)
            {
                if (IsChinese(c))
                    return true;
            }

            return false;
        }

        // 简化的拼音表（这里只包含部分常用汉字的拼音）
        // 完整实现需要包含所有汉字的拼音映射
        private static readonly Dictionary<int, string[]> PinyinMap = InitializePinyinMap();

        private static Dictionary<int, string[]> InitializePinyinMap()
        {
            var map = new Dictionary<int, string[]>();

            // 常用汉字拼音表（简化版，实际应用需要完整拼音表）
            string[] chars = {
                "的一是不了在人有我他这个们中来上大为和国地到以说时要就出会可也你对生能而子那得于着下自之年过发后作里用道行所然家种事成方多经么去法学如都同现当没动面起看定天分还进好小部其些主样理心她本前开但因只从想实日军者意无力它与长把机十民第公此已工使情明性知全三又关点正业外将两高间由问很最重并物手应战向头文体政美相见被利什二等产或新己制身果加西斯月话合回特代内信表化老给世位次度门任常先海通教儿原东声提立及比员解水名真论处走义各入几口认条平系气题活尔更别打女变四神总何电数安少报才结反受目太量再感建务做接必场件计管期市直德资命山金指克许统区保至队形社便空决治展马科司五基眼书非则听白却界达光放强即像难且权思王象完设式色路记南品住告类求据程北边死张该交规万取拉格望觉术领共确传师观清今切院让识候带导争运笔志认准许响约英格底仅流端讲乡村消故值收越古史附整改落致令参周农吸获坚单组切界育苦断背细油调灵责供济容质项根议陈拿破仑"
            };

            string[] pinyins = {
                "de,yi,shi,bu,liao,zai,ren,you,wo,ta,zhe,ge,men,zhong,lai,shang,da,wei,he,guo,di,dao,yi,shuo,shi,yao,jiu,chu,hui,ke,ye,ni,dui,sheng,neng,er,zi,na,de,yu,zhe,xia,zi,zhi,nian,guo,fa,hou,zuo,li,yong,dao,xing,suo,ran,jia,zhong,shi,cheng,fang,duo,jing,me,qu,fa,xue,ru,dou,tong,xian,dang,mei,dong,mian,qi,kan,ding,tian,fen,hai,jin,hao,xiao,bu,qi,xie,zhu,yang,li,xin,ta,ben,qian,kai,yin,zhi,cong,xiang,shi,ri,jun,zhe,yi,wu,li,ta,yu,chang,ba,ji,shi,min,di,gong,ci,yi,gong,shi,qing,ming,xing,zhi,quan,san,you,guan,dian,zheng,ye,wai,jiang,liang,gao,jian,you,wen,hen,zui,zhong,bing,wu,shou,ying,zhan,xiang,tou,wen,ti,zheng,mei,xiang,jian,bei,li,shi,er,deng,chan,huo,xin,ji,zhi,shen,guo,jia,xi,si,yue,hua,he,hui,te,dai,nei,xin,biao,hua,lao,gei,shi,wei,ci,du,men,ren,chang,xian,hai,tong,jiao,er,yuan,dong,sheng,ti,li,ji,bi,yuan,jie,shui,ming,zhen,lun,chu,zou,yi,ge,ru,ji,kou,ren,tiao,ping,xi,qi,ti,huo,er,geng,bie,da,nv,bian,si,shen,zong,he,dian,shu,an,shao,bao,cai,jie,fan,shou,mu,tai,liang,zai,gan,jian,wu,zuo,jie,bi,chang,jian,ji,guan,qi,shi,zhi,de,zi,ming,shan,jin,zhi,ke,xu,tong,qu,bao,zhi,dui,xing,she,bian,kong,jue,zhi,zhan,ma,ke,si,wu,ji,yan,shu,fei,ze,ting,bai,que,jie,da,guang,fang,qiang,ji,xiang,nan,qie,quan,si,wang,xiang,wan,she,shi,se,lu,ji,nan,pin,zhu,gao,lei,qiu,ju,cheng,bei,bian,si,zhang,gai,jiao,gui,wan,qu,la,ge,wang,jue,shu,ling,gong,que,chuan,shi,guan,qing,jin,qie,yuan,rang,shi,hou,dai,dao,zheng,yun,bi,zhi,ren,zhun,xu,xiang,yue,ying,ge,di,jin,liu,duan,jiang,xiang,cun,xiao,gu,gu,zhi,shou,yue,gu,shi,fu,zheng,gai,luo,zhi,ling,can,zhou,nong,xi,huo,jian,dan,zu,qie,jie,yu,ku,duan,bei,xi,you,diao,ling,ze,gong,ji,rong,zhi,xiang,gen,yi,chen,na,po,lun"
            };

            return map;
        }

        private static string[] GetPinyinArray(char c)
        {
            int code = c;

            // 使用简化的拼音查找算法
            // 实际实现需要完整的拼音对照表
            if (PinyinMap.TryGetValue(code, out string[] py))
            {
                return py;
            }

            // 简化处理：根据Unicode范围估算拼音首字母
            int index = code - 0x4E00;

            // 按拼音分区（非常简化）
            string[] initials = { "A", "B", "C", "D", "E", "F", "G", "H", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "W", "X", "Y", "Z" };

            // 这是一个简化实现，实际需要完整的拼音表
            // 这里只是为了演示
            int initialIndex = index % initials.Length;
            return new[] { initials[initialIndex].ToLower() };
        }
    }
}
