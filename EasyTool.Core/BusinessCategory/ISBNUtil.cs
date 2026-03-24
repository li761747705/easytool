using System;
using System.Text.RegularExpressions;

namespace EasyTool.BusinessCategory
{
    /// <summary>
    /// ISBN类型枚举
    /// </summary>
    public enum ISBNType
    {
        /// <summary>
        /// 未知类型
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// ISBN-10（10位）
        /// </summary>
        ISBN10 = 1,

        /// <summary>
        /// ISBN-13（13位）
        /// </summary>
        ISBN13 = 2
    }

    /// <summary>
    /// ISBN书号工具类
    /// </summary>
    public static class ISBNUtil
    {
        #region 常量与私有字段

        /// <summary>
        /// ISBN-10正则表达式（可含分隔符）
        /// </summary>
        private static readonly Regex ISBN10Regex = new Regex(
            @"^(\d{1,5}[-\s]?)?\d{1,7}[-\s]?\d{1,7}[-\s]?[\dXx]$",
            RegexOptions.Compiled);

        /// <summary>
        /// ISBN-13正则表达式（可含分隔符）
        /// </summary>
        private static readonly Regex ISBN13Regex = new Regex(
            @"^97[89][-\s]?\d{1,5}[-\s]?\d{1,7}[-\s]?\d{1,7}[-\s]?\d$",
            RegexOptions.Compiled);

        /// <summary>
        /// 纯数字ISBN-10正则
        /// </summary>
        private static readonly Regex ISBN10CleanRegex = new Regex(
            @"^\d{9}[\dXx]$",
            RegexOptions.Compiled);

        /// <summary>
        /// 纯数字ISBN-13正则
        /// </summary>
        private static readonly Regex ISBN13CleanRegex = new Regex(
            @"^97[89]\d{10}$",
            RegexOptions.Compiled);

        /// <summary>
        /// ISBN前缀与国家/地区/语言映射
        /// </summary>
        private static readonly (string Prefix, string Region)[] PrefixRegionMap =
        {
            ("0", "英语国家"), ("1", "英语国家"),
            ("2", "法语国家"),
            ("3", "德语国家"),
            ("4", "日本"),
            ("5", "前苏联/俄罗斯"),
            ("7", "中国"),
            ("80", "前捷克斯洛伐克"), ("85", "巴西"),
            ("87", "丹麦"),
            ("88", "意大利"),
            ("90", "荷兰"), ("91", "瑞典"), ("92", "国际组织"),
            ("93", "印度"), ("94", "荷兰"),
            ("952", "芬兰"), ("953", "克罗地亚"),
            ("960", "希腊"), ("961", "斯洛文尼亚"), ("962", "香港"),
            ("963", "匈牙利"), ("964", "伊朗"), ("965", "以色列"),
            ("966", "乌克兰"), ("967", "马来西亚"), ("968", "墨西哥"),
            ("969", "巴基斯坦"), ("970", "墨西哥"), ("971", "菲律宾"),
            ("972", "葡萄牙"), ("973", "罗马尼亚"), ("974", "泰国"),
            ("975", "土耳其"), ("976", "加勒比海地区"), ("977", "埃及"),
            ("978", "尼日利亚"), ("979", "印度尼西亚"),
            ("980", "委内瑞拉"), ("981", "新加坡"), ("982", "南太平洋地区"),
            ("983", "马来西亚"), ("984", "孟加拉"), ("985", "白俄罗斯"),
            ("986", "台湾"), ("987", "阿根廷"), ("988", "香港"),
            ("989", "葡萄牙"), ("9927", "沙特阿拉伯"), ("9933", "伊朗"),
            ("9937", "尼泊尔"), ("9939", "亚美尼亚"), ("9940", "卡塔尔"),
            ("9942", "阿塞拜疆"), ("9943", "塔吉克斯坦"), ("9944", "斯洛伐克"),
            ("9945", "朝鲜"), ("9946", "阿尔巴尼亚"), ("9947", "阿联酋"),
            ("9948", "黎巴嫩"), ("9949", "爱沙尼亚"), ("9950", "叙利亚"),
            ("9951", "约旦"), ("9952", "吉尔吉斯斯坦"), ("9953", "巴勒斯坦"),
            ("9954", "摩洛哥"), ("9955", "立陶宛"), ("9956", "喀麦隆"),
            ("9957", "约旦"), ("9958", "古巴"), ("9959", "阿尔及利亚"),
            ("9960", "沙特阿拉伯"), ("9961", "阿曼"), ("9962", "巴林"),
            ("9963", "冰岛"), ("9964", "加纳"), ("9965", "科威特"),
            ("9966", "肯尼亚"), ("9967", "吉布提"), ("9968", "厄瓜多尔"),
            ("9969", "蒙古"), ("9970", "乌干达"), ("9971", "津巴布韦"),
            ("9972", "巴拿马"), ("9973", "突尼斯"), ("9974", "塞内加尔"),
            ("9975", "罗马尼亚"), ("9976", "巴布亚新几内亚"), ("9977", "哥斯达黎加"),
            ("9978", "斯里兰卡"), ("9979", "冰岛"), ("9980", "刚果"),
            ("9981", "马达加斯加"), ("9982", "加蓬"), ("9983", "马里"),
            ("9984", "马拉维"), ("9985", "爱沙尼亚"), ("9986", "立陶宛"),
            ("9987", "坦桑尼亚"), ("9988", "加纳"), ("9989", "马其顿"),
            ("99901", "巴哈马"), ("99903", "莫桑比克"), ("99904", "哈萨克斯坦"),
            ("99905", "尼泊尔"), ("99906", "马拉维"), ("99908", "澳门")
        };

        #endregion

        #region 验证方法

        /// <summary>
        /// 验证ISBN是否有效（自动识别ISBN-10或ISBN-13）
        /// </summary>
        /// <param name="isbn">ISBN号</param>
        /// <returns>是否有效</returns>
        public static bool IsValid(string? isbn)
        {
            if (string.IsNullOrWhiteSpace(isbn))
            {
                return false;
            }

            string cleaned = CleanISBN(isbn);

            if (cleaned.Length == 10)
            {
                return IsValidISBN10(cleaned);
            }

            if (cleaned.Length == 13)
            {
                return IsValidISBN13(cleaned);
            }

            return false;
        }

        /// <summary>
        /// 验证ISBN-10是否有效
        /// </summary>
        /// <param name="isbn">ISBN号</param>
        /// <returns>是否有效</returns>
        public static bool IsValidISBN10(string? isbn)
        {
            if (string.IsNullOrWhiteSpace(isbn))
            {
                return false;
            }

            string cleaned = CleanISBN(isbn);

            if (!ISBN10CleanRegex.IsMatch(cleaned))
            {
                return false;
            }

            // 计算校验位
            int sum = 0;
            for (int i = 0; i < 9; i++)
            {
                sum += (cleaned[i] - '0') * (10 - i);
            }

            // 最后一位可能是X（代表10）
            char lastChar = char.ToUpper(cleaned[9]);
            int checkDigit = lastChar == 'X' ? 10 : (lastChar - '0');
            sum += checkDigit;

            return sum % 11 == 0;
        }

        /// <summary>
        /// 验证ISBN-13是否有效
        /// </summary>
        /// <param name="isbn">ISBN号</param>
        /// <returns>是否有效</returns>
        public static bool IsValidISBN13(string? isbn)
        {
            if (string.IsNullOrWhiteSpace(isbn))
            {
                return false;
            }

            string cleaned = CleanISBN(isbn);

            if (!ISBN13CleanRegex.IsMatch(cleaned))
            {
                return false;
            }

            // ISBN-13必须以978或979开头
            if (!cleaned.StartsWith("978") && !cleaned.StartsWith("979"))
            {
                return false;
            }

            // 计算校验位
            int sum = 0;
            for (int i = 0; i < 12; i++)
            {
                int digit = cleaned[i] - '0';
                sum += digit * (i % 2 == 0 ? 1 : 3);
            }

            int checkDigit = (10 - (sum % 10)) % 10;
            return checkDigit == (cleaned[12] - '0');
        }

        /// <summary>
        /// 验证ISBN格式（不计算校验位）
        /// </summary>
        /// <param name="isbn">ISBN号</param>
        /// <returns>格式是否正确</returns>
        public static bool IsValidFormat(string? isbn)
        {
            if (string.IsNullOrWhiteSpace(isbn))
            {
                return false;
            }

            string cleaned = CleanISBN(isbn);
            return ISBN10CleanRegex.IsMatch(cleaned) || ISBN13CleanRegex.IsMatch(cleaned);
        }

        #endregion

        #region 类型识别

        /// <summary>
        /// 获取ISBN类型
        /// </summary>
        /// <param name="isbn">ISBN号</param>
        /// <returns>ISBN类型</returns>
        public static ISBNType GetISBNType(string? isbn)
        {
            if (!IsValid(isbn))
            {
                return ISBNType.Unknown;
            }

            string cleaned = CleanISBN(isbn);
            return cleaned.Length == 10 ? ISBNType.ISBN10 : ISBNType.ISBN13;
        }

        #endregion

        #region 转换方法

        /// <summary>
        /// 将ISBN-10转换为ISBN-13
        /// </summary>
        /// <param name="isbn10">ISBN-10号</param>
        /// <returns>ISBN-13号，转换失败返回null</returns>
        public static string? ConvertToISBN13(string? isbn10)
        {
            if (!IsValidISBN10(isbn10))
            {
                return null;
            }

            string cleaned = CleanISBN(isbn10!);

            // 添加前缀978
            string isbn13 = "978" + cleaned.Substring(0, 9);

            // 计算新的校验位
            int sum = 0;
            for (int i = 0; i < 12; i++)
            {
                int digit = isbn13[i] - '0';
                sum += digit * (i % 2 == 0 ? 1 : 3);
            }

            int checkDigit = (10 - (sum % 10)) % 10;
            return isbn13 + checkDigit;
        }

        /// <summary>
        /// 将ISBN-13转换为ISBN-10（仅适用于978前缀）
        /// </summary>
        /// <param name="isbn13">ISBN-13号</param>
        /// <returns>ISBN-10号，转换失败返回null</returns>
        public static string? ConvertToISBN10(string? isbn13)
        {
            if (!IsValidISBN13(isbn13))
            {
                return null;
            }

            string cleaned = CleanISBN(isbn13!);

            // 只有978前缀才能转换为ISBN-10
            if (!cleaned.StartsWith("978"))
            {
                return null;
            }

            // 去掉前缀978和最后一位校验位
            string isbn10Body = cleaned.Substring(3, 9);

            // 计算ISBN-10校验位
            int sum = 0;
            for (int i = 0; i < 9; i++)
            {
                sum += (isbn10Body[i] - '0') * (10 - i);
            }

            int checkValue = 11 - (sum % 11);
            char checkChar;
            if (checkValue == 10)
            {
                checkChar = 'X';
            }
            else if (checkValue == 11)
            {
                checkChar = '0';
            }
            else
            {
                checkChar = (char)('0' + checkValue);
            }

            return isbn10Body + checkChar;
        }

        #endregion

        #region 信息提取

        /// <summary>
        /// 获取国家/地区名称
        /// </summary>
        /// <param name="isbn">ISBN号</param>
        /// <returns>国家/地区名称</returns>
        public static string? GetRegion(string? isbn)
        {
            if (!IsValid(isbn))
            {
                return null;
            }

            string cleaned = CleanISBN(isbn!);

            // ISBN-13需要去掉978/979前缀
            string prefix = cleaned.Length == 13 ? cleaned.Substring(3) : cleaned;

            // 查找最长匹配的前缀
            for (int len = Math.Min(5, prefix.Length); len >= 1; len--)
            {
                string searchPrefix = prefix.Substring(0, len);
                foreach (var mapping in PrefixRegionMap)
                {
                    if (mapping.Prefix == searchPrefix)
                    {
                        return mapping.Region;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 判断是否为中国出版物
        /// </summary>
        /// <param name="isbn">ISBN号</param>
        /// <returns>是否为中国出版物</returns>
        public static bool IsChinaISBN(string? isbn)
        {
            if (!IsValid(isbn))
            {
                return false;
            }

            string cleaned = CleanISBN(isbn!);

            // ISBN-13: 978-7 或 979-7
            // ISBN-10: 7开头
            if (cleaned.Length == 13)
            {
                return cleaned.StartsWith("9787") || cleaned.StartsWith("9797");
            }
            else
            {
                return cleaned.StartsWith("7");
            }
        }

        /// <summary>
        /// 计算ISBN-10校验位
        /// </summary>
        /// <param name="isbn9">不含校验位的9位数字</param>
        /// <returns>校验位（0-10，10表示X），计算失败返回-1</returns>
        public static int CalculateISBN10CheckDigit(string? isbn9)
        {
            if (string.IsNullOrWhiteSpace(isbn9) || isbn9.Length != 9)
            {
                return -1;
            }

            int sum = 0;
            for (int i = 0; i < 9; i++)
            {
                if (!char.IsDigit(isbn9[i]))
                {
                    return -1;
                }
                sum += (isbn9[i] - '0') * (10 - i);
            }

            int checkValue = 11 - (sum % 11);
            return checkValue == 11 ? 0 : checkValue;
        }

        /// <summary>
        /// 计算ISBN-13校验位
        /// </summary>
        /// <param name="isbn12">不含校验位的12位数字</param>
        /// <returns>校验位（0-9），计算失败返回-1</returns>
        public static int CalculateISBN13CheckDigit(string? isbn12)
        {
            if (string.IsNullOrWhiteSpace(isbn12) || isbn12.Length != 12)
            {
                return -1;
            }

            int sum = 0;
            for (int i = 0; i < 12; i++)
            {
                if (!char.IsDigit(isbn12[i]))
                {
                    return -1;
                }
                int digit = isbn12[i] - '0';
                sum += digit * (i % 2 == 0 ? 1 : 3);
            }

            return (10 - (sum % 10)) % 10;
        }

        #endregion

        #region 格式化方法

        /// <summary>
        /// 清理ISBN（去除分隔符）
        /// </summary>
        /// <param name="isbn">ISBN号</param>
        /// <returns>清理后的ISBN</returns>
        public static string CleanISBN(string? isbn)
        {
            if (string.IsNullOrWhiteSpace(isbn))
            {
                return "";
            }

            // 去除空格和横线
            return Regex.Replace(isbn, @"[\s\-]", "").ToUpper();
        }

        /// <summary>
        /// 格式化ISBN（添加分隔符）
        /// </summary>
        /// <param name="isbn">ISBN号</param>
        /// <returns>格式化后的ISBN，如978-7-115-12345-6</returns>
        public static string? Format(string? isbn)
        {
            if (!IsValid(isbn))
            {
                return null;
            }

            string cleaned = CleanISBN(isbn!);

            if (cleaned.Length == 10)
            {
                // ISBN-10格式：x-x-xxx-xxxxx-x
                return $"{cleaned[0]}-{cleaned[1]}-{cleaned.Substring(2, 3)}-{cleaned.Substring(5, 4)}-{cleaned[9]}";
            }
            else
            {
                // ISBN-13格式：xxx-x-xxx-xxxxx-x
                return $"{cleaned.Substring(0, 3)}-{cleaned[3]}-{cleaned.Substring(4, 3)}-{cleaned.Substring(7, 5)}-{cleaned[12]}";
            }
        }

        /// <summary>
        /// 格式化ISBN（使用自定义分隔符）
        /// </summary>
        /// <param name="isbn">ISBN号</param>
        /// <param name="separator">分隔符</param>
        /// <returns>格式化后的ISBN</returns>
        public static string? Format(string? isbn, char separator)
        {
            string? formatted = Format(isbn);
            if (formatted == null)
            {
                return null;
            }

            return formatted.Replace('-', separator);
        }

        /// <summary>
        /// ISBN脱敏：978-7-***-*****-*
        /// </summary>
        /// <param name="isbn">ISBN号</param>
        /// <returns>脱敏后的ISBN</returns>
        public static string? Mask(string? isbn)
        {
            if (!IsValid(isbn))
            {
                return null;
            }

            string cleaned = CleanISBN(isbn!);

            if (cleaned.Length == 10)
            {
                // 保留第1位和最后1位
                return cleaned[0] + "*******" + cleaned[9];
            }
            else
            {
                // 保留前4位和最后1位
                return cleaned.Substring(0, 4) + "*******" + cleaned[12];
            }
        }

        #endregion

        #region 生成方法

        /// <summary>
        /// 生成随机ISBN-13（仅供测试使用）
        /// </summary>
        /// <param name="prefix">前缀（默认978）</param>
        /// <returns>ISBN-13号</returns>
        public static string GenerateRandomISBN13(string prefix = "978")
        {
            // 生成12位数字
            string isbn12 = prefix + MathCategory.RandomUtil.RandomDigitString(12 - prefix.Length);

            // 计算校验位
            int checkDigit = CalculateISBN13CheckDigit(isbn12);

            return isbn12 + checkDigit;
        }

        /// <summary>
        /// 生成随机ISBN-10（仅供测试使用）
        /// </summary>
        /// <returns>ISBN-10号</returns>
        public static string GenerateRandomISBN10()
        {
            // 生成9位数字
            string isbn9 = MathCategory.RandomUtil.RandomDigitString(9);

            // 计算校验位
            int checkDigit = CalculateISBN10CheckDigit(isbn9);

            if (checkDigit == 10)
            {
                return isbn9 + "X";
            }

            return isbn9 + checkDigit;
        }

        #endregion
    }
}
