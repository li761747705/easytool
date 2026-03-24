using System;
using System.Text.RegularExpressions;

namespace EasyTool.BusinessCategory
{
    /// <summary>
    /// 身份证工具类
    /// </summary>
    public static class IdCardUtil
    {
        #region 常量与私有字段

        /// <summary>
        /// 18位身份证校验码权重
        /// </summary>
        private static readonly int[] Weights = { 7, 9, 10, 5, 8, 4, 2, 1, 6, 3, 7, 9, 10, 5, 8, 4, 2 };

        /// <summary>
        /// 18位身份证校验码对照表
        /// </summary>
        private static readonly char[] CheckCodes = { '1', '0', 'X', '9', '8', '7', '6', '5', '4', '3', '2' };

        /// <summary>
        /// 18位身份证正则表达式
        /// </summary>
        private static readonly Regex Regex18 = new Regex(@"^[1-9]\d{5}(19|20)\d{2}(0[1-9]|1[0-2])(0[1-9]|[12]\d|3[01])\d{3}[\dXx]$", RegexOptions.Compiled);

        /// <summary>
        /// 15位身份证正则表达式
        /// </summary>
        private static readonly Regex Regex15 = new Regex(@"^[1-9]\d{5}\d{2}(0[1-9]|1[0-2])(0[1-9]|[12]\d|3[01])\d{3}$", RegexOptions.Compiled);

        /// <summary>
        /// 省份代码与名称映射
        /// </summary>
        private static readonly string[] ProvinceCodes = {
            "", "北京", "天津", "河北", "山西", "内蒙古", // 11-15
            "", "辽宁", "吉林", "黑龙江", "", // 21-23
            "", "上海", "江苏", "浙江", "安徽", "福建", "江西", "山东", // 31-37
            "", "河南", "湖北", "湖南", "广东", "广西", "海南", // 41-46
            "", "重庆", "四川", "贵州", "云南", "西藏", // 50-54
            "", "陕西", "甘肃", "青海", "宁夏", "新疆", // 61-65
            "", "台湾", // 71
            "", "香港", "澳门" // 81-82
        };

        /// <summary>
        /// 星座日期范围
        /// </summary>
        private static readonly (int Month, int Day, string Name)[] ZodiacRanges = {
            (1, 20, "水瓶座"), (2, 19, "双鱼座"), (3, 21, "白羊座"),
            (4, 20, "金牛座"), (5, 21, "双子座"), (6, 22, "巨蟹座"),
            (7, 23, "狮子座"), (8, 23, "处女座"), (9, 23, "天秤座"),
            (10, 24, "天蝎座"), (11, 23, "射手座"), (12, 22, "摩羯座")
        };

        #endregion

        #region 验证方法

        /// <summary>
        /// 验证身份证号是否有效（支持15位和18位）
        /// </summary>
        /// <param name="idCard">身份证号</param>
        /// <returns>是否有效</returns>
        public static bool IsValid(string? idCard)
        {
            if (string.IsNullOrWhiteSpace(idCard))
            {
                return false;
            }

            return idCard.Length == 18 ? IsValid18(idCard) :
                   idCard.Length == 15 ? IsValid15(idCard) :
                   false;
        }

        /// <summary>
        /// 验证18位身份证号是否有效
        /// </summary>
        /// <param name="idCard">18位身份证号</param>
        /// <returns>是否有效</returns>
        public static bool IsValid18(string? idCard)
        {
            if (string.IsNullOrWhiteSpace(idCard) || idCard.Length != 18)
            {
                return false;
            }

            if (!Regex18.IsMatch(idCard))
            {
                return false;
            }

            // 验证日期有效性
            if (!IsValidDate(idCard.Substring(6, 8)))
            {
                return false;
            }

            // 验证校验码
            int sum = 0;
            for (int i = 0; i < 17; i++)
            {
                sum += (idCard[i] - '0') * Weights[i];
            }

            char expectedCheckCode = CheckCodes[sum % 11];
            char actualCheckCode = char.ToUpper(idCard[17]);

            return expectedCheckCode == actualCheckCode;
        }

        /// <summary>
        /// 验证15位身份证号是否有效
        /// </summary>
        /// <param name="idCard">15位身份证号</param>
        /// <returns>是否有效</returns>
        public static bool IsValid15(string? idCard)
        {
            if (string.IsNullOrWhiteSpace(idCard) || idCard.Length != 15)
            {
                return false;
            }

            if (!Regex15.IsMatch(idCard))
            {
                return false;
            }

            // 验证日期有效性（15位身份证年份默认为19xx）
            string dateStr = "19" + idCard.Substring(6, 6);
            return IsValidDate(dateStr);
        }

        #endregion

        #region 转换方法

        /// <summary>
        /// 将15位身份证号转换为18位
        /// </summary>
        /// <param name="idCard15">15位身份证号</param>
        /// <returns>18位身份证号，转换失败返回null</returns>
        public static string? Convert15To18(string? idCard15)
        {
            if (!IsValid15(idCard15))
            {
                return null;
            }

            // 在第6位后插入"19"
            string idCard17 = idCard15!.Substring(0, 6) + "19" + idCard15.Substring(6);

            // 计算校验码
            int sum = 0;
            for (int i = 0; i < 17; i++)
            {
                sum += (idCard17[i] - '0') * Weights[i];
            }

            return idCard17 + CheckCodes[sum % 11];
        }

        /// <summary>
        /// 将18位身份证号转换为15位
        /// </summary>
        /// <param name="idCard18">18位身份证号</param>
        /// <returns>15位身份证号，转换失败返回null</returns>
        public static string? Convert18To15(string? idCard18)
        {
            if (!IsValid18(idCard18))
            {
                return null;
            }

            // 移除第6-9位的年份前两位"19"和最后一位校验码
            return idCard18!.Substring(0, 6) + idCard18.Substring(8, 9);
        }

        #endregion

        #region 信息提取方法

        /// <summary>
        /// 获取出生日期
        /// </summary>
        /// <param name="idCard">身份证号</param>
        /// <returns>出生日期，解析失败返回null</returns>
        public static DateTime? GetBirthday(string? idCard)
        {
            if (!IsValid(idCard))
            {
                return null;
            }

            string dateStr;
            if (idCard!.Length == 18)
            {
                dateStr = idCard.Substring(6, 8);
            }
            else
            {
                dateStr = "19" + idCard.Substring(6, 6);
            }

            int year = int.Parse(dateStr.Substring(0, 4));
            int month = int.Parse(dateStr.Substring(4, 2));
            int day = int.Parse(dateStr.Substring(6, 2));

            return new DateTime(year, month, day);
        }

        /// <summary>
        /// 获取年龄
        /// </summary>
        /// <param name="idCard">身份证号</param>
        /// <returns>年龄，解析失败返回null</returns>
        public static int? GetAge(string? idCard)
        {
            DateTime? birthday = GetBirthday(idCard);
            if (!birthday.HasValue)
            {
                return null;
            }

            DateTime today = DateTime.Today;
            int age = today.Year - birthday.Value.Year;

            // 如果今年生日还没过，年龄减1
            if (today < birthday.Value.AddYears(age))
            {
                age--;
            }

            return age;
        }

        /// <summary>
        /// 获取性别代码（1男2女）
        /// </summary>
        /// <param name="idCard">身份证号</param>
        /// <returns>性别代码，解析失败返回null</returns>
        public static int? GetGender(string? idCard)
        {
            if (!IsValid(idCard))
            {
                return null;
            }

            // 第17位（索引16）表示性别，奇数为男，偶数为女
            int genderDigit;
            if (idCard!.Length == 18)
            {
                genderDigit = idCard[16] - '0';
            }
            else
            {
                genderDigit = idCard[14] - '0';
            }

            return genderDigit % 2 == 1 ? 1 : 2;
        }

        /// <summary>
        /// 获取性别字符串（男/女）
        /// </summary>
        /// <param name="idCard">身份证号</param>
        /// <returns>性别字符串，解析失败返回null</returns>
        public static string? GetGenderString(string? idCard)
        {
            int? gender = GetGender(idCard);
            if (!gender.HasValue)
            {
                return null;
            }

            return gender.Value == 1 ? "男" : "女";
        }

        /// <summary>
        /// 获取省份名称
        /// </summary>
        /// <param name="idCard">身份证号</param>
        /// <returns>省份名称，解析失败返回null</returns>
        public static string? GetProvince(string? idCard)
        {
            if (string.IsNullOrWhiteSpace(idCard) || idCard.Length < 2)
            {
                return null;
            }

            int provinceCode;
            if (!int.TryParse(idCard.Substring(0, 2), out provinceCode))
            {
                return null;
            }

            if (provinceCode < 0 || provinceCode >= ProvinceCodes.Length)
            {
                return null;
            }

            return ProvinceCodes[provinceCode];
        }

        /// <summary>
        /// 获取行政区划代码（前6位）
        /// </summary>
        /// <param name="idCard">身份证号</param>
        /// <returns>行政区划代码，解析失败返回null</returns>
        public static string? GetAreaCode(string? idCard)
        {
            if (string.IsNullOrWhiteSpace(idCard) || (idCard.Length != 15 && idCard.Length != 18))
            {
                return null;
            }

            return idCard.Substring(0, 6);
        }

        /// <summary>
        /// 获取生肖
        /// </summary>
        /// <param name="idCard">身份证号</param>
        /// <returns>生肖，解析失败返回null</returns>
        public static string? GetChineseZodiac(string? idCard)
        {
            DateTime? birthday = GetBirthday(idCard);
            if (!birthday.HasValue)
            {
                return null;
            }

            return EasyTool.DateTimeCategory.LunarCalendarUtil.GetChineseZodiac(birthday.Value);
        }

        /// <summary>
        /// 获取星座
        /// </summary>
        /// <param name="idCard">身份证号</param>
        /// <returns>星座，解析失败返回null</returns>
        public static string? GetZodiac(string? idCard)
        {
            DateTime? birthday = GetBirthday(idCard);
            if (!birthday.HasValue)
            {
                return null;
            }

            return GetZodiacByDate(birthday.Value.Month, birthday.Value.Day);
        }

        #endregion

        #region 生成方法

        /// <summary>
        /// 生成随机身份证号（仅供测试使用）
        /// </summary>
        /// <param name="provinceCode">省份代码（可选，默认随机）</param>
        /// <param name="birthday">出生日期（可选，默认随机）</param>
        /// <param name="gender">性别（可选，1男2女，默认随机）</param>
        /// <returns>18位身份证号</returns>
        public static string GenerateRandom(string? provinceCode = null, DateTime? birthday = null, int? gender = null)
        {
            // 省份代码
            string province = provinceCode ?? GetRandomProvinceCode();

            // 出生日期
            DateTime birth = birthday ?? EasyTool.MathCategory.RandomUtil.GetRandomDateTime(
                new DateTime(1950, 1, 1),
                new DateTime(2005, 12, 31));
            string birthStr = birth.ToString("yyyyMMdd");

            // 顺序码（3位）+ 性别
            string sequence = EasyTool.MathCategory.RandomUtil.RandomDigitString(2);
            int genderDigit;
            if (gender.HasValue && (gender.Value == 1 || gender.Value == 2))
            {
                // 指定性别的奇偶性
                int randomDigit = EasyTool.MathCategory.RandomUtil.RandomInt(0, 4);
                genderDigit = gender.Value == 1 ? randomDigit * 2 + 1 : randomDigit * 2;
            }
            else
            {
                genderDigit = EasyTool.MathCategory.RandomUtil.RandomInt(0, 9);
            }
            sequence += genderDigit.ToString();

            // 前17位
            string idCard17 = province + birthStr + sequence;

            // 计算校验码
            int sum = 0;
            for (int i = 0; i < 17; i++)
            {
                sum += (idCard17[i] - '0') * Weights[i];
            }

            return idCard17 + CheckCodes[sum % 11];
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 验证日期字符串是否有效
        /// </summary>
        private static bool IsValidDate(string dateStr)
        {
            if (dateStr.Length != 8)
            {
                return false;
            }

            int year = int.Parse(dateStr.Substring(0, 4));
            int month = int.Parse(dateStr.Substring(4, 2));
            int day = int.Parse(dateStr.Substring(6, 2));

            if (year < 1900 || year > 2100)
            {
                return false;
            }

            if (month < 1 || month > 12)
            {
                return false;
            }

            int maxDay = DateTime.DaysInMonth(year, month);
            return day >= 1 && day <= maxDay;
        }

        /// <summary>
        /// 根据日期获取星座
        /// </summary>
        private static string GetZodiacByDate(int month, int day)
        {
            // 星座按日期划分，摩羯座的特殊处理（跨年）
            for (int i = ZodiacRanges.Length - 1; i >= 0; i--)
            {
                var zodiac = ZodiacRanges[i];
                if (month > zodiac.Month || (month == zodiac.Month && day >= zodiac.Day))
                {
                    return zodiac.Name;
                }
            }

            // 1月1日到1月19日是摩羯座
            return "摩羯座";
        }

        /// <summary>
        /// 获取随机省份代码
        /// </summary>
        private static string GetRandomProvinceCode()
        {
            int[] validCodes = { 11, 12, 13, 14, 15, 21, 22, 23, 31, 32, 33, 34, 35, 36, 37, 41, 42, 43, 44, 45, 46, 50, 51, 52, 53, 54, 61, 62, 63, 64, 65 };
            int code = EasyTool.MathCategory.RandomUtil.GetRandomElement(validCodes);
            return code.ToString("00");
        }

        #endregion
    }
}
