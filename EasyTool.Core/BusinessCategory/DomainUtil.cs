using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace EasyTool.BusinessCategory
{
    /// <summary>
    /// 域名工具类
    /// </summary>
    public static class DomainUtil
    {
        #region 常量与私有字段

        /// <summary>
        /// 域名正则表达式
        /// </summary>
        private static readonly Regex DomainRegex = new(
            @"^(?:[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?\.)+[a-zA-Z]{2,}$",
            RegexOptions.Compiled);

        /// <summary>
        /// IDN（国际化域名）正则
        /// </summary>
        private static readonly Regex IdnRegex = new(
            @"^(?:[a-zA-Z0-9\u4e00-\u9fa5](?:[a-zA-Z0-9-\u4e00-\u9fa5]{0,61}[a-zA-Z0-9\u4e00-\u9fa5])?\.)+[a-zA-Z\u4e00-\u9fa5]{2,}$",
            RegexOptions.Compiled);

        /// <summary>
        /// 顶级域名（TLD）与类型映射
        /// </summary>
        private static readonly Dictionary<string, string> TldTypeMap = new(StringComparer.OrdinalIgnoreCase)
        {
            // 通用顶级域名
            { "com", "商业机构" }, { "net", "网络服务商" }, { "org", "非营利组织" },
            { "edu", "教育机构" }, { "gov", "政府机构" }, { "mil", "军事机构" },
            { "int", "国际组织" }, { "info", "信息服务" }, { "biz", "商业" },
            { "name", "个人" }, { "pro", "专业人士" }, { "museum", "博物馆" },
            { "coop", "合作社" }, { "aero", "航空" }, { "xxx", "成人内容" },
            { "xyz", "通用" }, { "top", "通用" }, { "vip", "VIP" },
            { "site", "网站" }, { "online", "在线" }, { "store", "商店" },
            { "tech", "科技" }, { "fun", "娱乐" }, { "club", "俱乐部" },
            { "shop", "购物" }, { "ltd", "有限公司" }, { "work", "工作" },

            // 国家/地区顶级域名
            { "cn", "中国" }, { "hk", "香港" }, { "tw", "台湾" }, { "mo", "澳门" },
            { "jp", "日本" }, { "kr", "韩国" }, { "sg", "新加坡" }, { "my", "马来西亚" },
            { "th", "泰国" }, { "vn", "越南" }, { "ph", "菲律宾" }, { "id", "印度尼西亚" },
            { "in", "印度" }, { "pk", "巴基斯坦" }, { "au", "澳大利亚" }, { "nz", "新西兰" },
            { "us", "美国" }, { "ca", "加拿大" }, { "mx", "墨西哥" }, { "br", "巴西" },
            { "uk", "英国" }, { "de", "德国" }, { "fr", "法国" }, { "it", "意大利" },
            { "es", "西班牙" }, { "nl", "荷兰" }, { "be", "比利时" }, { "ch", "瑞士" },
            { "at", "奥地利" }, { "se", "瑞典" }, { "no", "挪威" }, { "dk", "丹麦" },
            { "fi", "芬兰" }, { "ru", "俄罗斯" }, { "pl", "波兰" }, { "cz", "捷克" },
            { "ua", "乌克兰" }, { "tr", "土耳其" }, { "sa", "沙特" }, { "ae", "阿联酋" },
            { "il", "以色列" }, { "za", "南非" }, { "eg", "埃及" }, { "ng", "尼日利亚" },
            { "ke", "肯尼亚" }, { "ar", "阿根廷" }, { "cl", "智利" }, { "co", "哥伦比亚" },

            // 中国二级域名
            { "com.cn", "中国商业" }, { "net.cn", "中国网络" }, { "org.cn", "中国组织" },
            { "gov.cn", "中国政府" }, { "edu.cn", "中国教育" }, { "ac.cn", "中国科研" },
            { "mil.cn", "中国军事" }, { "bj.cn", "北京" }, { "sh.cn", "上海" },
            { "tj.cn", "天津" }, { "cq.cn", "重庆" }, { "he.cn", "河北" },
            { "sx.cn", "山西" }, { "nm.cn", "内蒙古" }, { "ln.cn", "辽宁" },
            { "jl.cn", "吉林" }, { "hl.cn", "黑龙江" }, { "js.cn", "江苏" },
            { "zj.cn", "浙江" }, { "ah.cn", "安徽" }, { "fj.cn", "福建" },
            { "jx.cn", "江西" }, { "sd.cn", "山东" }, { "ha.cn", "河南" },
            { "hb.cn", "湖北" }, { "hn.cn", "湖南" }, { "gd.cn", "广东" },
            { "gx.cn", "广西" }, { "hi.cn", "海南" }, { "sc.cn", "四川" },
            { "gz.cn", "贵州" }, { "yn.cn", "云南" }, { "xz.cn", "西藏" },
            { "sn.cn", "陕西" }, { "gs.cn", "甘肃" }, { "qh.cn", "青海" },
            { "nx.cn", "宁夏" }, { "xj.cn", "新疆" }
        };

        /// <summary>
        /// 常见二级域名服务
        /// </summary>
        private static readonly Dictionary<string, string> WellKnownDomains = new(StringComparer.OrdinalIgnoreCase)
        {
            { "baidu.com", "百度" }, { "qq.com", "腾讯" }, { "taobao.com", "淘宝" },
            { "tmall.com", "天猫" }, { "jd.com", "京东" }, { "alipay.com", "支付宝" },
            { "alibaba.com", "阿里巴巴" }, { "aliyun.com", "阿里云" },
            { "tencent.com", "腾讯" }, { "weixin.com", "微信" }, { "wechat.com", "微信" },
            { "douyin.com", "抖音" }, { "tiktok.com", "TikTok" }, { "bytedance.com", "字节跳动" },
            { "meituan.com", "美团" }, { "dianping.com", "大众点评" },
            { "didichuxing.com", "滴滴" }, { "xiaojukeji.com", "滴滴" },
            { "sohu.com", "搜狐" }, { "sina.com.cn", "新浪" }, { "weibo.com", "微博" },
            { "163.com", "网易" }, { "126.com", "网易" }, { "yeah.net", "网易" },
            { "zhihu.com", "知乎" }, { "csdn.net", "CSDN" },
            { "bilibili.com", "哔哩哔哩" }, { "acfun.cn", "AcFun" },
            { "youku.com", "优酷" }, { "iqiyi.com", "爱奇艺" }, { "v.qq.com", "腾讯视频" },
            { "github.com", "GitHub" }, { "gitlab.com", "GitLab" }, { "gitee.com", "Gitee" },
            { "google.com", "Google" }, { "youtube.com", "YouTube" }, { "gmail.com", "Gmail" },
            { "facebook.com", "Facebook" }, { "instagram.com", "Instagram" }, { "whatsapp.com", "WhatsApp" },
            { "twitter.com", "Twitter" }, { "x.com", "X" },
            { "linkedin.com", "LinkedIn" }, { "microsoft.com", "Microsoft" },
            { "apple.com", "Apple" }, { "amazon.com", "Amazon" }, { "aws.amazon.com", "AWS" },
            { "cloudflare.com", "Cloudflare" }, { "nginx.com", "NGINX" }
        };

        #endregion

        #region 验证方法

        /// <summary>
        /// 验证域名是否有效
        /// </summary>
        /// <param name="domain">域名</param>
        /// <returns>是否有效</returns>
        public static bool IsValid(string? domain)
        {
            if (string.IsNullOrWhiteSpace(domain))
            {
                return false;
            }

            // 域名总长度不超过253字符
            if (domain.Length > 253)
            {
                return false;
            }

            string lower = domain.ToLower().Trim();

            // 检查是否为IDN
            if (IdnRegex.IsMatch(lower))
            {
                return true;
            }

            return DomainRegex.IsMatch(lower);
        }

        /// <summary>
        /// 验证是否为国际顶级域名
        /// </summary>
        /// <param name="domain">域名</param>
        /// <returns>是否为国际域名</returns>
        public static bool IsInternationalDomain(string? domain)
        {
            if (!IsValid(domain))
            {
                return false;
            }

            string tld = GetTLD(domain);
            if (tld == null) return false;

            // 常见国际顶级域名
            string[] internationalTlds = { "com", "net", "org", "edu", "gov", "mil", "int", "info", "biz", "name", "pro" };
            foreach (var itld in internationalTlds)
            {
                if (tld.Equals(itld, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 验证是否为中国域名
        /// </summary>
        /// <param name="domain">域名</param>
        /// <returns>是否为中国域名</returns>
        public static bool IsChinaDomain(string? domain)
        {
            if (!IsValid(domain))
            {
                return false;
            }

            string tld = GetTLD(domain);
            return tld?.Equals("cn", StringComparison.OrdinalIgnoreCase) == true ||
                   domain!.EndsWith(".com.cn", StringComparison.OrdinalIgnoreCase) ||
                   domain.EndsWith(".net.cn", StringComparison.OrdinalIgnoreCase) ||
                   domain.EndsWith(".org.cn", StringComparison.OrdinalIgnoreCase) ||
                   domain.EndsWith(".gov.cn", StringComparison.OrdinalIgnoreCase) ||
                   domain.EndsWith(".edu.cn", StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region 信息提取

        /// <summary>
        /// 获取顶级域名（TLD）
        /// </summary>
        /// <param name="domain">域名</param>
        /// <returns>顶级域名</returns>
        public static string? GetTLD(string? domain)
        {
            if (!IsValid(domain))
            {
                return null;
            }

            string[] parts = domain!.ToLower().Split('.');
            if (parts.Length < 2)
            {
                return null;
            }

            // 检查是否为双后缀（如.com.cn）
            if (parts.Length >= 3)
            {
                string possibleDoubleTld = parts[^2] + "." + parts[^1];
                if (TldTypeMap.ContainsKey(possibleDoubleTld))
                {
                    return possibleDoubleTld;
                }
            }

            return parts[^1];
        }

        /// <summary>
        /// 获取顶级域名类型/归属
        /// </summary>
        /// <param name="domain">域名</param>
        /// <returns>顶级域名类型</returns>
        public static string? GetTLDType(string? domain)
        {
            if (!IsValid(domain))
            {
                return null;
            }

            // 先检查双后缀
            string[] parts = domain!.ToLower().Split('.');
            if (parts.Length >= 3)
            {
                string possibleDoubleTld = parts[^2] + "." + parts[^1];
                if (TldTypeMap.TryGetValue(possibleDoubleTld, out string? type))
                {
                    return type;
                }
            }

            string tld = GetTLD(domain);
            if (tld != null && TldTypeMap.TryGetValue(tld, out string? tldType))
            {
                return tldType;
            }

            return null;
        }

        /// <summary>
        /// 获取二级域名
        /// </summary>
        /// <param name="domain">域名</param>
        /// <returns>二级域名</returns>
        public static string? GetSecondLevelDomain(string? domain)
        {
            if (!IsValid(domain))
            {
                return null;
            }

            string[] parts = domain!.ToLower().Split('.');
            if (parts.Length < 2)
            {
                return null;
            }

            // 处理双后缀
            if (parts.Length >= 3)
            {
                string possibleDoubleTld = parts[^2] + "." + parts[^1];
                if (TldTypeMap.ContainsKey(possibleDoubleTld) && parts.Length >= 4)
                {
                    return parts[^3] + "." + possibleDoubleTld;
                }
            }

            return parts[^2] + "." + parts[^1];
        }

        /// <summary>
        /// 获取子域名前缀
        /// </summary>
        /// <param name="domain">域名</param>
        /// <returns>子域名前缀（如www、mail等）</returns>
        public static string? GetSubdomain(string? domain)
        {
            if (!IsValid(domain))
            {
                return null;
            }

            string[] parts = domain!.ToLower().Split('.');

            // 计算主域名部分的长度
            int mainDomainParts = 2;
            if (parts.Length >= 3)
            {
                string possibleDoubleTld = parts[^2] + "." + parts[^1];
                if (TldTypeMap.ContainsKey(possibleDoubleTld))
                {
                    mainDomainParts = 3;
                }
            }

            if (parts.Length <= mainDomainParts)
            {
                return null; // 无子域名
            }

            // 返回除主域名外的部分
            return string.Join(".", parts, 0, parts.Length - mainDomainParts);
        }

        /// <summary>
        /// 获取主域名（不含子域名）
        /// </summary>
        /// <param name="domain">域名</param>
        /// <returns>主域名</returns>
        public static string? GetMainDomain(string? domain)
        {
            string? sld = GetSecondLevelDomain(domain);
            return sld;
        }

        /// <summary>
        /// 获取已知服务名称
        /// </summary>
        /// <param name="domain">域名</param>
        /// <returns>服务名称</returns>
        public static string? GetServiceName(string? domain)
        {
            if (!IsValid(domain))
            {
                return null;
            }

            string mainDomain = GetMainDomain(domain)?.ToLower() ?? "";

            foreach (var kvp in WellKnownDomains)
            {
                if (mainDomain.Equals(kvp.Key, StringComparison.OrdinalIgnoreCase) ||
                    domain!.EndsWith("." + kvp.Key, StringComparison.OrdinalIgnoreCase))
                {
                    return kvp.Value;
                }
            }

            return null;
        }

        #endregion

        #region 格式化方法

        /// <summary>
        /// 格式化域名（转小写，去除协议和路径）
        /// </summary>
        /// <param name="domain">域名</param>
        /// <returns>格式化后的域名</returns>
        public static string? Normalize(string? domain)
        {
            if (string.IsNullOrWhiteSpace(domain))
            {
                return null;
            }

            string cleaned = domain.ToLower().Trim();

            // 去除协议
            if (cleaned.StartsWith("http://"))
            {
                cleaned = cleaned.Substring(7);
            }
            else if (cleaned.StartsWith("https://"))
            {
                cleaned = cleaned.Substring(8);
            }

            // 去除路径
            int slashIndex = cleaned.IndexOf('/');
            if (slashIndex > 0)
            {
                cleaned = cleaned.Substring(0, slashIndex);
            }

            // 去除端口
            int colonIndex = cleaned.LastIndexOf(':');
            if (colonIndex > 0)
            {
                cleaned = cleaned.Substring(0, colonIndex);
            }

            return IsValid(cleaned) ? cleaned : null;
        }

        /// <summary>
        /// 域名脱敏：e*****.com
        /// </summary>
        /// <param name="domain">域名</param>
        /// <returns>脱敏后的域名</returns>
        public static string? Mask(string? domain)
        {
            string? normalized = Normalize(domain);
            if (normalized == null)
            {
                return null;
            }

            string[] parts = normalized.Split('.');
            if (parts.Length < 2)
            {
                return null;
            }

            // 脱敏主域名部分
            string mainPart = parts[0];
            if (mainPart.Length <= 2)
            {
                parts[0] = mainPart[0] + "*";
            }
            else
            {
                parts[0] = mainPart[0] + new string('*', mainPart.Length - 2) + mainPart[^1];
            }

            return string.Join(".", parts);
        }

        #endregion

        #region 生成方法

        /// <summary>
        /// 生成随机域名（仅供测试使用）
        /// </summary>
        /// <param name="tld">顶级域名（可选，默认.com）</param>
        /// <returns>随机域名</returns>
        public static string GenerateRandom(string? tld = null)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            string suffix = tld ?? "com";

            // 生成随机主域名（6-12位）
            int length = MathCategory.RandomUtil.RandomInt(6, 13);
            string main = "";
            for (int i = 0; i < length; i++)
            {
                main += MathCategory.RandomUtil.GetRandomElement(chars.ToCharArray());
            }

            return main + "." + suffix;
        }

        #endregion
    }
}
