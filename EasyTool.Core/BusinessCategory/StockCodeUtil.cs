using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace EasyTool.BusinessCategory
{
    /// <summary>
    /// 股票市场枚举
    /// </summary>
    public enum StockMarket
    {
        /// <summary>
        /// 未知
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// 上海证券交易所
        /// </summary>
        SHSE = 1,

        /// <summary>
        /// 深圳证券交易所
        /// </summary>
        SZSE = 2,

        /// <summary>
        /// 北京证券交易所
        /// </summary>
        BSE = 3,

        /// <summary>
        /// 香港交易所
        /// </summary>
        HKEX = 4,

        /// <summary>
        /// 纽约证券交易所
        /// </summary>
        NYSE = 5,

        /// <summary>
        /// 纳斯达克
        /// </summary>
        NASDAQ = 6
    }

    /// <summary>
    /// 股票类型枚举
    /// </summary>
    public enum StockType
    {
        /// <summary>
        /// 未知
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// A股
        /// </summary>
        AShare = 1,

        /// <summary>
        /// B股
        /// </summary>
        BShare = 2,

        /// <summary>
        /// 创业板
        /// </summary>
        ChiNext = 3,

        /// <summary>
        /// 科创板
        /// </summary>
        STAR = 4,

        /// <summary>
        /// 北交所
        /// </summary>
        BSEShare = 5,

        /// <summary>
        /// 港股
        /// </summary>
        HKStock = 6,

        /// <summary>
        /// 美股
        /// </summary>
        USStock = 7
    }

    /// <summary>
    /// 股票代码工具类
    /// </summary>
    public static class StockCodeUtil
    {
        #region 常量与私有字段

        /// <summary>
        /// A股代码正则表达式（6位数字）
        /// </summary>
        private static readonly Regex AShareRegex = new(@"^[036]\d{5}$", RegexOptions.Compiled);

        /// <summary>
        /// B股代码正则表达式
        /// </summary>
        private static readonly Regex BShareRegex = new(@"^[29]\d{5}$", RegexOptions.Compiled);

        /// <summary>
        /// 创业板代码正则表达式（30开头）
        /// </summary>
        private static readonly Regex ChiNextRegex = new(@"^30\d{4}$", RegexOptions.Compiled);

        /// <summary>
        /// 科创板代码正则表达式（688开头）
        /// </summary>
        private static readonly Regex STARRegex = new(@"^688\d{3}$", RegexOptions.Compiled);

        /// <summary>
        /// 北交所代码正则表达式（8开头，4位或6位）
        /// </summary>
        private static readonly Regex BSERegex = new(@"^(8[34]\d{4}|4[38]\d{4})$", RegexOptions.Compiled);

        /// <summary>
        /// 港股代码正则表达式（1-5位数字）
        /// </summary>
        private static readonly Regex HKStockRegex = new(@"^\d{4,5}$", RegexOptions.Compiled);

        /// <summary>
        /// 美股代码正则表达式（1-5位大写字母）
        /// </summary>
        private static readonly Regex USStockRegex = new(@"^[A-Z]{1,5}$", RegexOptions.Compiled);

        /// <summary>
        /// 常见A股股票代码映射（部分示例）
        /// </summary>
        private static readonly Dictionary<string, (string Name, string Market)> StockCodeMap = new()
        {
            // 上证A股
            { "600000", ("浦发银行", "上海") }, { "600036", ("招商银行", "上海") },
            { "600519", ("贵州茅台", "上海") }, { "600887", ("伊利股份", "上海") },
            { "601318", ("中国平安", "上海") }, { "601398", ("工商银行", "上海") },
            { "601939", ("建设银行", "上海") }, { "601988", ("中国银行", "上海") },
            { "601288", ("农业银行", "上海") }, { "601857", ("中国石油", "上海") },
            { "601668", ("中国建筑", "上海") }, { "600276", ("恒瑞医药", "上海") },
            { "600309", ("万华化学", "上海") }, { "600900", ("长江电力", "上海") },
            { "601012", ("隆基绿能", "上海") }, { "603259", ("药明康德", "上海") },

            // 深证A股
            { "000001", ("平安银行", "深圳") }, { "000002", ("万科A", "深圳") },
            { "000333", ("美的集团", "深圳") }, { "000651", ("格力电器", "深圳") },
            { "000858", ("五粮液", "深圳") }, { "002594", ("比亚迪", "深圳") },
            { "000063", ("中兴通讯", "深圳") }, { "002475", ("立讯精密", "深圳") },
            { "002415", ("海康威视", "深圳") }, { "002352", ("顺丰控股", "深圳") },
            { "000568", ("泸州老窖", "深圳") }, { "002714", ("牧原股份", "深圳") },

            // 创业板
            { "300750", ("宁德时代", "深圳") }, { "300059", ("东方财富", "深圳") },
            { "300015", ("爱尔眼科", "深圳") }, { "300347", ("泰格医药", "深圳") },
            { "300760", ("迈瑞医疗", "深圳") }, { "300124", ("汇川技术", "深圳") },

            // 科创板
            { "688981", ("中芯国际", "上海") }, { "688111", ("金山办公", "上海") },
            { "688012", ("中微公司", "上海") }, { "688256", ("寒武纪", "上海") },

            // 港股
            { "00700", ("腾讯控股", "香港") }, { "09988", ("阿里巴巴-SW", "香港") },
            { "03690", ("美团-W", "香港") }, { "09999", ("网易-S", "香港") },
            { "01024", ("快手-W", "香港") }, { "01810", ("小米集团-W", "香港") },
            { "09618", ("京东集团-SW", "香港") }, { "02318", ("中国平安", "香港") },
            { "00005", ("汇丰控股", "香港") }, { "00941", ("中国移动", "香港") },
            { "03988", ("中国银行", "香港") }, { "01398", ("工商银行", "香港") },

            // 美股
            { "AAPL", ("苹果", "纳斯达克") }, { "MSFT", ("微软", "纳斯达克") },
            { "GOOGL", ("谷歌", "纳斯达克") }, { "AMZN", ("亚马逊", "纳斯达克") },
            { "META", ("Meta", "纳斯达克") }, { "NVDA", ("英伟达", "纳斯达克") },
            { "TSLA", ("特斯拉", "纳斯达克") }, { "NFLX", ("奈飞", "纳斯达克") },
            { "BABA", ("阿里巴巴", "纽约") }, { "JD", ("京东", "纳斯达克") },
            { "PDD", ("拼多多", "纳斯达克") }, { "BIDU", ("百度", "纳斯达克") }
        };

        #endregion

        #region 验证方法

        /// <summary>
        /// 验证股票代码是否有效（支持A股、港股、美股）
        /// </summary>
        /// <param name="code">股票代码</param>
        /// <param name="market">市场类型（可选，默认自动识别）</param>
        /// <returns>是否有效</returns>
        public static bool IsValid(string? code, StockMarket? market = null)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return false;
            }

            if (market.HasValue)
            {
                return market.Value switch
                {
                    StockMarket.SHSE or StockMarket.SZSE => IsValidAShare(code),
                    StockMarket.BSE => IsValidBSE(code),
                    StockMarket.HKEX => IsValidHKStock(code),
                    StockMarket.NYSE or StockMarket.NASDAQ => IsValidUSStock(code),
                    _ => false
                };
            }

            return IsValidAShare(code) || IsValidBSE(code) || IsValidHKStock(code) || IsValidUSStock(code);
        }

        /// <summary>
        /// 验证A股代码是否有效
        /// </summary>
        /// <param name="code">股票代码</param>
        /// <returns>是否有效</returns>
        public static bool IsValidAShare(string? code)
        {
            if (string.IsNullOrWhiteSpace(code) || code.Length != 6)
            {
                return false;
            }

            // A股（60、00、30、688开头）和B股（20、900开头）
            return AShareRegex.IsMatch(code) || BShareRegex.IsMatch(code) ||
                   ChiNextRegex.IsMatch(code) || STARRegex.IsMatch(code);
        }

        /// <summary>
        /// 验证北交所代码是否有效
        /// </summary>
        /// <param name="code">股票代码</param>
        /// <returns>是否有效</returns>
        public static bool IsValidBSE(string? code)
        {
            if (string.IsNullOrWhiteSpace(code) || code.Length != 6)
            {
                return false;
            }

            return BSERegex.IsMatch(code);
        }

        /// <summary>
        /// 验证港股代码是否有效
        /// </summary>
        /// <param name="code">股票代码</param>
        /// <returns>是否有效</returns>
        public static bool IsValidHKStock(string? code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return false;
            }

            return HKStockRegex.IsMatch(code);
        }

        /// <summary>
        /// 验证美股代码是否有效
        /// </summary>
        /// <param name="code">股票代码</param>
        /// <returns>是否有效</returns>
        public static bool IsValidUSStock(string? code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return false;
            }

            return USStockRegex.IsMatch(code.ToUpper());
        }

        #endregion

        #region 市场识别

        /// <summary>
        /// 获取股票市场
        /// </summary>
        /// <param name="code">股票代码</param>
        /// <returns>股票市场</returns>
        public static StockMarket GetMarket(string? code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return StockMarket.Unknown;
            }

            string upper = code.ToUpper();

            // 美股（字母代码）
            if (USStockRegex.IsMatch(upper))
            {
                return StockMarket.NASDAQ; // 简化处理
            }

            // 港股（4-5位数字）
            if (HKStockRegex.IsMatch(code))
            {
                return StockMarket.HKEX;
            }

            // A股（6位数字）
            if (code.Length == 6)
            {
                if (code.StartsWith("60") || code.StartsWith("68"))
                {
                    return StockMarket.SHSE;
                }
                if (code.StartsWith("00") || code.StartsWith("30"))
                {
                    return StockMarket.SZSE;
                }
                if (code.StartsWith("83") || code.StartsWith("87") || code.StartsWith("43") || code.StartsWith("83"))
                {
                    return StockMarket.BSE;
                }
                // B股
                if (code.StartsWith("900"))
                {
                    return StockMarket.SHSE;
                }
                if (code.StartsWith("200"))
                {
                    return StockMarket.SZSE;
                }
            }

            return StockMarket.Unknown;
        }

        /// <summary>
        /// 获取股票市场名称
        /// </summary>
        /// <param name="market">股票市场</param>
        /// <returns>市场名称</returns>
        public static string GetMarketName(StockMarket market)
        {
            return market switch
            {
                StockMarket.SHSE => "上海证券交易所",
                StockMarket.SZSE => "深圳证券交易所",
                StockMarket.BSE => "北京证券交易所",
                StockMarket.HKEX => "香港交易所",
                StockMarket.NYSE => "纽约证券交易所",
                StockMarket.NASDAQ => "纳斯达克",
                _ => "未知"
            };
        }

        #endregion

        #region 类型识别

        /// <summary>
        /// 获取股票类型
        /// </summary>
        /// <param name="code">股票代码</param>
        /// <returns>股票类型</returns>
        public static StockType GetStockType(string? code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return StockType.Unknown;
            }

            string upper = code.ToUpper();

            // 美股
            if (USStockRegex.IsMatch(upper))
            {
                return StockType.USStock;
            }

            // 港股
            if (HKStockRegex.IsMatch(code))
            {
                return StockType.HKStock;
            }

            // A股细分
            if (code.Length == 6)
            {
                if (STARRegex.IsMatch(code)) return StockType.STAR;
                if (ChiNextRegex.IsMatch(code)) return StockType.ChiNext;
                if (BSERegex.IsMatch(code)) return StockType.BSEShare;
                if (code.StartsWith("60") || code.StartsWith("00")) return StockType.AShare;
                if (code.StartsWith("900") || code.StartsWith("200")) return StockType.BShare;
            }

            return StockType.Unknown;
        }

        /// <summary>
        /// 获取股票类型名称
        /// </summary>
        /// <param name="type">股票类型</param>
        /// <returns>类型名称</returns>
        public static string GetStockTypeName(StockType type)
        {
            return type switch
            {
                StockType.AShare => "A股",
                StockType.BShare => "B股",
                StockType.ChiNext => "创业板",
                StockType.STAR => "科创板",
                StockType.BSEShare => "北交所",
                StockType.HKStock => "港股",
                StockType.USStock => "美股",
                _ => "未知"
            };
        }

        #endregion

        #region 信息查询

        /// <summary>
        /// 获取股票名称
        /// </summary>
        /// <param name="code">股票代码</param>
        /// <returns>股票名称</returns>
        public static string? GetName(string? code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return null;
            }

            string key = code.ToUpper().PadLeft(6, '0');
            if (StockCodeMap.TryGetValue(key, out var info))
            {
                return info.Name;
            }

            // 尝试原始格式
            if (StockCodeMap.TryGetValue(code.ToUpper(), out info))
            {
                return info.Name;
            }

            return null;
        }

        /// <summary>
        /// 获取完整股票代码（带市场前缀）
        /// </summary>
        /// <param name="code">股票代码</param>
        /// <returns>完整代码（如sh600519、sz000001、hk00700）</returns>
        public static string? GetFullCode(string? code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return null;
            }

            StockMarket market = GetMarket(code);
            return market switch
            {
                StockMarket.SHSE => "sh" + code,
                StockMarket.SZSE => "sz" + code,
                StockMarket.BSE => "bj" + code,
                StockMarket.HKEX => "hk" + code.PadLeft(5, '0'),
                StockMarket.NYSE => "nyse:" + code.ToUpper(),
                StockMarket.NASDAQ => "nasdaq:" + code.ToUpper(),
                _ => null
            };
        }

        #endregion

        #region 格式化方法

        /// <summary>
        /// 格式化股票代码
        /// </summary>
        /// <param name="code">股票代码</param>
        /// <returns>格式化后的代码</returns>
        public static string? Normalize(string? code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return null;
            }

            // 去除市场前缀
            string cleaned = code.ToLower()
                .Replace("sh", "").Replace("sz", "").Replace("bj", "")
                .Replace("hk", "").Replace("nyse:", "").Replace("nasdaq:", "");

            // 港股补零
            if (HKStockRegex.IsMatch(cleaned) && cleaned.Length < 5)
            {
                cleaned = cleaned.PadLeft(5, '0');
            }

            return IsValid(cleaned) ? cleaned.ToUpper() : null;
        }

        /// <summary>
        /// 股票代码脱敏：60****9
        /// </summary>
        /// <param name="code">股票代码</param>
        /// <returns>脱敏后的代码</returns>
        public static string? Mask(string? code)
        {
            string? normalized = Normalize(code);
            if (normalized == null)
            {
                return null;
            }

            if (normalized.Length <= 2)
            {
                return normalized[0] + "*";
            }

            return normalized[0] + new string('*', normalized.Length - 2) + normalized[^1];
        }

        #endregion
    }
}
