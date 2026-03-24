using System;
using System.Text.RegularExpressions;

namespace EasyTool.BusinessCategory
{
    /// <summary>
    /// 驾驶证号工具类
    /// </summary>
    public static class DrivingLicenseUtil
    {
        #region 常量与私有字段

        /// <summary>
        /// 驾驶证号正则表达式（18位，与身份证号格式相同）
        /// </summary>
        private static readonly Regex LicenseRegex = new(
            @"^[1-9]\d{5}(19|20)\d{2}(0[1-9]|1[0-2])(0[1-9]|[12]\d|3[01])\d{3}[\dXx]$",
            RegexOptions.Compiled);

        /// <summary>
        /// 档案编号正则表达式（12位数字）
        /// </summary>
        private static readonly Regex FileNumberRegex = new(@"^\d{12}$", RegexOptions.Compiled);

        /// <summary>
        /// 驾驶证校验码权重
        /// </summary>
        private static readonly int[] Weights = { 7, 9, 10, 5, 8, 4, 2, 1, 6, 3, 7, 9, 10, 5, 8, 4, 2 };

        /// <summary>
        /// 驾驶证校验码对照表
        /// </summary>
        private static readonly char[] CheckCodes = { '1', '0', 'X', '9', '8', '7', '6', '5', '4', '3', '2' };

        /// <summary>
        /// 准驾车型映射
        /// </summary>
        private static readonly (string Code, string Name, string Description)[] VehicleClassMap =
        {
            ("A1", "大型客车", "可驾驶A3、B1、B2、C1、C2、C3、C4、M"),
            ("A2", "牵引车", "可驾驶B1、B2、C1、C2、C3、C4、M"),
            ("A3", "城市公交车", "可驾驶C1、C2、C3、C4"),
            ("B1", "中型客车", "可驾驶C1、C2、C3、C4、M"),
            ("B2", "大型货车", "可驾驶C1、C2、C3、C4、M"),
            ("C1", "小型汽车", "可驾驶C2、C3、C4"),
            ("C2", "小型自动挡汽车", "仅限自动挡小型汽车"),
            ("C3", "低速载货汽车", "可驾驶C4"),
            ("C4", "三轮汽车", ""),
            ("C5", "残疾人专用小型自动挡汽车", ""),
            ("C6", "轻型牵引挂车", "需C1或C2以上驾照增驾"),
            ("D", "普通三轮摩托车", "可驾驶E、F"),
            ("E", "普通二轮摩托车", "可驾驶F"),
            ("F", "轻便摩托车", ""),
            ("G", "拖拉机", ""),
            ("H", "轮式自行机械", ""),
            ("M", "轮式自行机械车", ""),
            ("N", "无轨电车", ""),
            ("P", "有轨电车", "")
        };

        #endregion

        #region 验证方法

        /// <summary>
        /// 验证驾驶证号是否有效
        /// </summary>
        /// <param name="licenseNumber">驾驶证号</param>
        /// <returns>是否有效</returns>
        public static bool IsValid(string? licenseNumber)
        {
            if (string.IsNullOrWhiteSpace(licenseNumber))
            {
                return false;
            }

            if (!LicenseRegex.IsMatch(licenseNumber))
            {
                return false;
            }

            // 验证日期有效性
            if (!IsValidDate(licenseNumber.Substring(6, 8)))
            {
                return false;
            }

            // 验证校验码
            int sum = 0;
            for (int i = 0; i < 17; i++)
            {
                sum += (licenseNumber[i] - '0') * Weights[i];
            }

            char expectedCheckCode = CheckCodes[sum % 11];
            char actualCheckCode = char.ToUpper(licenseNumber[17]);

            return expectedCheckCode == actualCheckCode;
        }

        /// <summary>
        /// 验证档案编号是否有效
        /// </summary>
        /// <param name="fileNumber">档案编号</param>
        /// <returns>是否有效</returns>
        public static bool IsValidFileNumber(string? fileNumber)
        {
            if (string.IsNullOrWhiteSpace(fileNumber))
            {
                return false;
            }

            return FileNumberRegex.IsMatch(fileNumber);
        }

        #endregion

        #region 信息提取

        /// <summary>
        /// 获取出生日期
        /// </summary>
        /// <param name="licenseNumber">驾驶证号</param>
        /// <returns>出生日期</returns>
        public static DateTime? GetBirthday(string? licenseNumber)
        {
            if (!IsValid(licenseNumber))
            {
                return null;
            }

            int year = int.Parse(licenseNumber!.Substring(6, 4));
            int month = int.Parse(licenseNumber.Substring(10, 2));
            int day = int.Parse(licenseNumber.Substring(12, 2));

            return new DateTime(year, month, day);
        }

        /// <summary>
        /// 获取性别（1男2女）
        /// </summary>
        /// <param name="licenseNumber">驾驶证号</param>
        /// <returns>性别代码</returns>
        public static int? GetGender(string? licenseNumber)
        {
            if (!IsValid(licenseNumber))
            {
                return null;
            }

            int genderDigit = licenseNumber![16] - '0';
            return genderDigit % 2 == 1 ? 1 : 2;
        }

        /// <summary>
        /// 获取性别字符串
        /// </summary>
        /// <param name="licenseNumber">驾驶证号</param>
        /// <returns>性别</returns>
        public static string? GetGenderString(string? licenseNumber)
        {
            int? gender = GetGender(licenseNumber);
            return gender switch
            {
                1 => "男",
                2 => "女",
                _ => null
            };
        }

        /// <summary>
        /// 获取行政区划代码
        /// </summary>
        /// <param name="licenseNumber">驾驶证号</param>
        /// <returns>行政区划代码</returns>
        public static string? GetAreaCode(string? licenseNumber)
        {
            if (!IsValid(licenseNumber))
            {
                return null;
            }

            return licenseNumber!.Substring(0, 6);
        }

        /// <summary>
        /// 判断驾驶证号是否与身份证号一致
        /// </summary>
        /// <param name="licenseNumber">驾驶证号</param>
        /// <param name="idCard">身份证号</param>
        /// <returns>是否一致</returns>
        public static bool MatchesIdCard(string? licenseNumber, string? idCard)
        {
            if (!IsValid(licenseNumber) || !IdCardUtil.IsValid18(idCard))
            {
                return false;
            }

            return licenseNumber!.Equals(idCard!, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region 准驾车型

        /// <summary>
        /// 获取准驾车型信息
        /// </summary>
        /// <param name="vehicleClass">准驾车型代码</param>
        /// <returns>车型信息</returns>
        public static (string Name, string Description)? GetVehicleClassInfo(string? vehicleClass)
        {
            if (string.IsNullOrWhiteSpace(vehicleClass))
            {
                return null;
            }

            foreach (var info in VehicleClassMap)
            {
                if (info.Code.Equals(vehicleClass, StringComparison.OrdinalIgnoreCase))
                {
                    return (info.Name, info.Description);
                }
            }

            return null;
        }

        /// <summary>
        /// 获取准驾车型名称
        /// </summary>
        /// <param name="vehicleClass">准驾车型代码</param>
        /// <returns>车型名称</returns>
        public static string? GetVehicleClassName(string? vehicleClass)
        {
            var info = GetVehicleClassInfo(vehicleClass);
            return info?.Name;
        }

        /// <summary>
        /// 验证准驾车型代码是否有效
        /// </summary>
        /// <param name="vehicleClass">准驾车型代码</param>
        /// <returns>是否有效</returns>
        public static bool IsValidVehicleClass(string? vehicleClass)
        {
            return GetVehicleClassInfo(vehicleClass) != null;
        }

        #endregion

        #region 格式化方法

        /// <summary>
        /// 格式化驾驶证号（转大写）
        /// </summary>
        /// <param name="licenseNumber">驾驶证号</param>
        /// <returns>格式化后的驾驶证号</returns>
        public static string? Normalize(string? licenseNumber)
        {
            if (string.IsNullOrWhiteSpace(licenseNumber))
            {
                return null;
            }

            string upper = licenseNumber.ToUpper().Trim();
            return upper.Length == 18 && LicenseRegex.IsMatch(upper) ? upper : null;
        }

        /// <summary>
        /// 驾驶证号脱敏：110***********1234
        /// </summary>
        /// <param name="licenseNumber">驾驶证号</param>
        /// <returns>脱敏后的驾驶证号</returns>
        public static string? Mask(string? licenseNumber)
        {
            if (!IsValid(licenseNumber))
            {
                return null;
            }

            return licenseNumber!.Substring(0, 3) + "***********" + licenseNumber.Substring(14);
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

            if (year < 1900 || year > DateTime.Now.Year)
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

        #endregion
    }
}
