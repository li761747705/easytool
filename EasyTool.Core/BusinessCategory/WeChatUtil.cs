using System;
using System.Text.RegularExpressions;

namespace EasyTool.BusinessCategory
{
    /// <summary>
    /// 微信号工具类
    /// </summary>
    public static class WeChatUtil
    {
        #region 常量与私有字段

        /// <summary>
        /// 微信号正则表达式（6-20位，字母开头，字母数字下划线减号）
        /// </summary>
        private static readonly Regex WeChatIdRegex = new(
            @"^[a-zA-Z][a-zA-Z0-9_-]{5,19}$",
            RegexOptions.Compiled);

        /// <summary>
        /// 微信原始ID正则表达式（gh_开头）
        /// </summary>
        private static readonly Regex WeChatOriginalIdRegex = new(
            @"^gh_[a-zA-Z0-9]{11,12}$",
            RegexOptions.Compiled);

        /// <summary>
        /// 微信开放平台UnionID正则表达式
        /// </summary>
        private static readonly Regex WeChatUnionIdRegex = new(
            @"^[a-zA-Z0-9_-]{28,32}$",
            RegexOptions.Compiled);

        /// <summary>
        /// 微信小程序AppID正则表达式
        /// </summary>
        private static readonly Regex WeChatAppIdRegex = new(
            @"^wx[a-f0-9]{16}$",
            RegexOptions.Compiled);

        #endregion

        #region 验证方法

        /// <summary>
        /// 验证微信号是否有效
        /// </summary>
        /// <param name="wechatId">微信号</param>
        /// <returns>是否有效</returns>
        public static bool IsValid(string? wechatId)
        {
            if (string.IsNullOrWhiteSpace(wechatId))
            {
                return false;
            }

            return WeChatIdRegex.IsMatch(wechatId);
        }

        /// <summary>
        /// 验证微信原始ID是否有效（公众号/小程序）
        /// </summary>
        /// <param name="originalId">原始ID（gh_开头）</param>
        /// <returns>是否有效</returns>
        public static bool IsValidOriginalId(string? originalId)
        {
            if (string.IsNullOrWhiteSpace(originalId))
            {
                return false;
            }

            return WeChatOriginalIdRegex.IsMatch(originalId);
        }

        /// <summary>
        /// 验证微信UnionID是否有效
        /// </summary>
        /// <param name="unionId">UnionID</param>
        /// <returns>是否有效</returns>
        public static bool IsValidUnionId(string? unionId)
        {
            if (string.IsNullOrWhiteSpace(unionId))
            {
                return false;
            }

            return WeChatUnionIdRegex.IsMatch(unionId);
        }

        /// <summary>
        /// 验证微信小程序AppID是否有效
        /// </summary>
        /// <param name="appId">AppID</param>
        /// <returns>是否有效</returns>
        public static bool IsValidAppId(string? appId)
        {
            if (string.IsNullOrWhiteSpace(appId))
            {
                return false;
            }

            return WeChatAppIdRegex.IsMatch(appId.ToLower());
        }

        /// <summary>
        /// 验证格式是否正确（仅格式检查）
        /// </summary>
        /// <param name="wechatId">微信号</param>
        /// <returns>格式是否正确</returns>
        public static bool IsValidFormat(string? wechatId)
        {
            return IsValid(wechatId);
        }

        #endregion

        #region 类型识别

        /// <summary>
        /// 获取微信ID类型
        /// </summary>
        /// <param name="id">微信相关ID</param>
        /// <returns>ID类型描述</returns>
        public static string? GetIdType(string? id)
        {
            if (IsValid(id)) return "微信号";
            if (IsValidOriginalId(id)) return "微信原始ID";
            if (IsValidUnionId(id)) return "UnionID";
            if (IsValidAppId(id)) return "小程序AppID";
            return null;
        }

        #endregion

        #region 格式化方法

        /// <summary>
        /// 格式化微信号（转小写）
        /// </summary>
        /// <param name="wechatId">微信号</param>
        /// <returns>格式化后的微信号</returns>
        public static string? Normalize(string? wechatId)
        {
            if (string.IsNullOrWhiteSpace(wechatId))
            {
                return null;
            }

            string normalized = wechatId.ToLower().Trim();
            return IsValid(normalized) ? normalized : null;
        }

        /// <summary>
        /// 微信号脱敏：abc***xyz
        /// </summary>
        /// <param name="wechatId">微信号</param>
        /// <returns>脱敏后的微信号</returns>
        public static string? Mask(string? wechatId)
        {
            if (!IsValid(wechatId))
            {
                return null;
            }

            string id = wechatId!;
            if (id.Length <= 4)
            {
                return id[0] + new string('*', id.Length - 1);
            }

            // 保留前3位和后3位
            int prefixLen = 3;
            int suffixLen = 3;
            int maskLen = id.Length - prefixLen - suffixLen;

            return id.Substring(0, prefixLen) + new string('*', maskLen) + id.Substring(id.Length - suffixLen);
        }

        #endregion

        #region 生成方法

        /// <summary>
        /// 生成随机微信号（仅供测试使用）
        /// </summary>
        /// <returns>随机微信号</returns>
        public static string GenerateRandom()
        {
            // 微信号长度6-20位
            int length = MathCategory.RandomUtil.RandomInt(6, 21);

            // 第一位为字母
            const string letters = "abcdefghijklmnopqrstuvwxyz";
            string result = MathCategory.RandomUtil.GetRandomElement(letters.ToCharArray()).ToString();

            // 剩余位为字母数字下划线减号
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789_-";
            for (int i = 1; i < length; i++)
            {
                result += MathCategory.RandomUtil.GetRandomElement(chars.ToCharArray());
            }

            return result;
        }

        /// <summary>
        /// 生成随机小程序AppID（仅供测试使用）
        /// </summary>
        /// <returns>随机AppID</returns>
        public static string GenerateRandomAppId()
        {
            string hex = "";
            for (int i = 0; i < 16; i++)
            {
                hex += "0123456789abcdef"[MathCategory.RandomUtil.RandomInt(0, 16)];
            }
            return "wx" + hex;
        }

        #endregion
    }
}
