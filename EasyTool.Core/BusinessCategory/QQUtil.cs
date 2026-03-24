using System;
using System.Text.RegularExpressions;

namespace EasyTool.BusinessCategory
{
    /// <summary>
    /// QQ号工具类
    /// </summary>
    public static class QQUtil
    {
        #region 常量与私有字段

        /// <summary>
        /// QQ号正则表达式（5-11位数字，不以0开头）
        /// </summary>
        private static readonly Regex QQRegex = new(
            @"^[1-9]\d{4,10}$",
            RegexOptions.Compiled);

        /// <summary>
        /// QQ邮箱正则表达式
        /// </summary>
        private static readonly Regex QQEmailRegex = new(
            @"^[1-9]\d{4,10}@qq\.com$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        #endregion

        #region 验证方法

        /// <summary>
        /// 验证QQ号是否有效
        /// </summary>
        /// <param name="qq">QQ号</param>
        /// <returns>是否有效</returns>
        public static bool IsValid(string? qq)
        {
            if (string.IsNullOrWhiteSpace(qq))
            {
                return false;
            }

            return QQRegex.IsMatch(qq);
        }

        /// <summary>
        /// 验证QQ邮箱是否有效
        /// </summary>
        /// <param name="email">QQ邮箱</param>
        /// <returns>是否有效</returns>
        public static bool IsValidQQEmail(string? email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            return QQEmailRegex.IsMatch(email);
        }

        /// <summary>
        /// 验证QQ号格式（仅检查格式，不验证是否存在）
        /// </summary>
        /// <param name="qq">QQ号</param>
        /// <returns>格式是否正确</returns>
        public static bool IsValidFormat(string? qq)
        {
            return IsValid(qq);
        }

        #endregion

        #region 转换方法

        /// <summary>
        /// 从QQ邮箱提取QQ号
        /// </summary>
        /// <param name="email">QQ邮箱</param>
        /// <returns>QQ号，提取失败返回null</returns>
        public static string? ExtractFromEmail(string? email)
        {
            if (!IsValidQQEmail(email))
            {
                return null;
            }

            int atIndex = email!.IndexOf('@');
            return email.Substring(0, atIndex);
        }

        /// <summary>
        /// 将QQ号转换为QQ邮箱
        /// </summary>
        /// <param name="qq">QQ号</param>
        /// <returns>QQ邮箱</returns>
        public static string? ToEmail(string? qq)
        {
            if (!IsValid(qq))
            {
                return null;
            }

            return qq + "@qq.com";
        }

        #endregion

        #region 格式化方法

        /// <summary>
        /// 格式化QQ号（去除非数字字符）
        /// </summary>
        /// <param name="qq">QQ号</param>
        /// <returns>格式化后的QQ号</returns>
        public static string? Normalize(string? qq)
        {
            if (string.IsNullOrWhiteSpace(qq))
            {
                return null;
            }

            string cleaned = Regex.Replace(qq, @"\D", "");
            return IsValid(cleaned) ? cleaned : null;
        }

        /// <summary>
        /// QQ号脱敏：123****890
        /// </summary>
        /// <param name="qq">QQ号</param>
        /// <returns>脱敏后的QQ号</returns>
        public static string? Mask(string? qq)
        {
            if (!IsValid(qq))
            {
                return null;
            }

            string code = qq!;
            if (code.Length <= 4)
            {
                return code[0] + new string('*', code.Length - 1);
            }

            // 保留前3位和后3位
            int prefixLen = 3;
            int suffixLen = 3;
            int maskLen = code.Length - prefixLen - suffixLen;

            return code.Substring(0, prefixLen) + new string('*', maskLen) + code.Substring(code.Length - suffixLen);
        }

        #endregion

        #region 生成方法

        /// <summary>
        /// 生成随机QQ号（仅供测试使用）
        /// </summary>
        /// <returns>随机QQ号</returns>
        public static string GenerateRandom()
        {
            // QQ号长度5-11位
            int length = MathCategory.RandomUtil.RandomInt(5, 12);

            // 第一位不能为0
            string result = MathCategory.RandomUtil.RandomInt(1, 10).ToString();

            // 剩余位数
            for (int i = 1; i < length; i++)
            {
                result += MathCategory.RandomUtil.RandomInt(0, 10).ToString();
            }

            return result;
        }

        #endregion
    }
}
