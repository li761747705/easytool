using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace EasyTool.BusinessCategory
{
    /// <summary>
    /// 密码强度等级
    /// </summary>
    public enum PasswordStrength
    {
        /// <summary>
        /// 非常弱
        /// </summary>
        VeryWeak = 0,

        /// <summary>
        /// 弱
        /// </summary>
        Weak = 1,

        /// <summary>
        /// 中等
        /// </summary>
        Medium = 2,

        /// <summary>
        /// 强
        /// </summary>
        Strong = 3,

        /// <summary>
        /// 非常强
        /// </summary>
        VeryStrong = 4
    }

    /// <summary>
    /// 密码验证结果
    /// </summary>
    public class PasswordValidationResult
    {
        /// <summary>
        /// 是否有效
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// 密码强度
        /// </summary>
        public PasswordStrength Strength { get; set; }

        /// <summary>
        /// 强度分数（0-100）
        /// </summary>
        public int Score { get; set; }

        /// <summary>
        /// 错误信息列表
        /// </summary>
        public List<string> Errors { get; set; } = new List<string>();

        /// <summary>
        /// 警告信息列表
        /// </summary>
        public List<string> Warnings { get; set; } = new List<string>();
    }

    /// <summary>
    /// 密码验证选项
    /// </summary>
    public class PasswordValidationOptions
    {
        /// <summary>
        /// 最小长度（默认8）
        /// </summary>
        public int MinLength { get; set; } = 8;

        /// <summary>
        /// 最大长度（默认128）
        /// </summary>
        public int MaxLength { get; set; } = 128;

        /// <summary>
        /// 是否要求包含小写字母（默认true）
        /// </summary>
        public bool RequireLowercase { get; set; } = true;

        /// <summary>
        /// 是否要求包含大写字母（默认true）
        /// </summary>
        public bool RequireUppercase { get; set; } = true;

        /// <summary>
        /// 是否要求包含数字（默认true）
        /// </summary>
        public bool RequireDigit { get; set; } = true;

        /// <summary>
        /// 是否要求包含特殊字符（默认true）
        /// </summary>
        public bool RequireSpecialChar { get; set; } = true;

        /// <summary>
        /// 允许的特殊字符（默认!@#$%^&amp;*()_+-=[]{}|;:',.&lt;&gt;?）
        /// </summary>
        public string AllowedSpecialChars { get; set; } = "!@#$%^&*()_+-=[]{}|;:',.<>?";

        /// <summary>
        /// 最少不同字符类型数量（默认3）
        /// </summary>
        public int MinCharacterTypes { get; set; } = 3;

        /// <summary>
        /// 是否禁止常见弱密码（默认true）
        /// </summary>
        public bool BlockCommonPasswords { get; set; } = true;

        /// <summary>
        /// 是否禁止连续重复字符（默认true）
        /// </summary>
        public bool BlockRepeatingChars { get; set; } = true;

        /// <summary>
        /// 是否禁止连续递增/递减字符（如123、abc）（默认true）
        /// </summary>
        public bool BlockSequentialChars { get; set; } = true;
    }

    /// <summary>
    /// 密码工具类
    /// </summary>
    public static class PasswordUtil
    {
        #region 常量与私有字段

        /// <summary>
        /// 常见弱密码列表
        /// </summary>
        private static readonly HashSet<string> CommonPasswords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "password", "123456", "12345678", "123456789", "1234567890",
            "qwerty", "abc123", "password123", "admin", "admin123",
            "root", "root123", "111111", "000000", "123123",
            "password1", "iloveyou", "monkey", "dragon", "master",
            "letmein", "login", "welcome", "shadow", "sunshine",
            "princess", "football", "baseball", "soccer", "hockey",
            "batman", "superman", "trustno1", "passw0rd", "qazwsx",
            "qwerty123", "123qwe", "654321", "888888", "666666"
        };

        /// <summary>
        /// 键盘连续字符模式
        /// </summary>
        private static readonly string[] KeyboardSequences = {
            "qwertyuiop", "asdfghjkl", "zxcvbnm",
            "qwertyuiop".ToUpper(), "asdfghjkl".ToUpper(), "zxcvbnm".ToUpper()
        };

        /// <summary>
        /// 小写字母正则表达式
        /// </summary>
        private static readonly Regex LowercaseRegex = new(@"[a-z]", RegexOptions.Compiled);

        /// <summary>
        /// 大写字母正则表达式
        /// </summary>
        private static readonly Regex UppercaseRegex = new(@"[A-Z]", RegexOptions.Compiled);

        /// <summary>
        /// 数字正则表达式
        /// </summary>
        private static readonly Regex DigitRegex = new(@"\d", RegexOptions.Compiled);

        /// <summary>
        /// 特殊字符正则表达式
        /// </summary>
        private static readonly Regex SpecialCharRegex = new(@"[!@#$%^&*()_+\-=\[\]{}|;:',.<>?]", RegexOptions.Compiled);

        #endregion

        #region 验证方法

        /// <summary>
        /// 验证密码（使用默认选项）
        /// </summary>
        /// <param name="password">密码</param>
        /// <returns>验证结果</returns>
        public static PasswordValidationResult Validate(string? password)
        {
            return Validate(password, new PasswordValidationOptions());
        }

        /// <summary>
        /// 验证密码（使用自定义选项）
        /// </summary>
        /// <param name="password">密码</param>
        /// <param name="options">验证选项</param>
        /// <returns>验证结果</returns>
        public static PasswordValidationResult Validate(string? password, PasswordValidationOptions options)
        {
            var result = new PasswordValidationResult();

            // 空值检查
            if (string.IsNullOrEmpty(password))
            {
                result.IsValid = false;
                result.Errors.Add("密码不能为空");
                result.Score = 0;
                result.Strength = PasswordStrength.VeryWeak;
                return result;
            }

            // 长度检查
            if (password.Length < options.MinLength)
            {
                result.Errors.Add($"密码长度不能少于{options.MinLength}位");
            }

            if (password.Length > options.MaxLength)
            {
                result.Errors.Add($"密码长度不能超过{options.MaxLength}位");
            }

            // 字符类型检查
            bool hasLowercase = LowercaseRegex.IsMatch(password);
            bool hasUppercase = UppercaseRegex.IsMatch(password);
            bool hasDigit = DigitRegex.IsMatch(password);
            bool hasSpecial = SpecialCharRegex.IsMatch(password);

            if (options.RequireLowercase && !hasLowercase)
            {
                result.Errors.Add("密码必须包含小写字母");
            }

            if (options.RequireUppercase && !hasUppercase)
            {
                result.Errors.Add("密码必须包含大写字母");
            }

            if (options.RequireDigit && !hasDigit)
            {
                result.Errors.Add("密码必须包含数字");
            }

            if (options.RequireSpecialChar && !hasSpecial)
            {
                result.Errors.Add("密码必须包含特殊字符");
            }

            // 统计字符类型数量
            int charTypeCount = (hasLowercase ? 1 : 0) + (hasUppercase ? 1 : 0) + (hasDigit ? 1 : 0) + (hasSpecial ? 1 : 0);
            if (charTypeCount < options.MinCharacterTypes)
            {
                result.Errors.Add($"密码必须包含至少{options.MinCharacterTypes}种不同类型的字符");
            }

            // 检查非法字符
            if (!string.IsNullOrEmpty(options.AllowedSpecialChars))
            {
                string allowedPattern = $@"^[a-zA-Z0-9{Regex.Escape(options.AllowedSpecialChars)}]+$";
                if (!Regex.IsMatch(password, allowedPattern))
                {
                    result.Errors.Add("密码包含非法字符");
                }
            }

            // 检查常见弱密码
            if (options.BlockCommonPasswords && CommonPasswords.Contains(password))
            {
                result.Errors.Add("密码过于简单，请使用更复杂的密码");
            }

            // 检查连续重复字符
            if (options.BlockRepeatingChars && HasRepeatingChars(password, 3))
            {
                result.Warnings.Add("密码包含连续重复的字符");
            }

            // 检查连续递增/递减字符
            if (options.BlockSequentialChars && HasSequentialChars(password))
            {
                result.Warnings.Add("密码包含连续的递增或递减字符");
            }

            // 计算强度分数
            int score = CalculateScore(password, hasLowercase, hasUppercase, hasDigit, hasSpecial);
            result.Score = score;
            result.Strength = GetStrengthFromScore(score);

            // 确定是否有效
            result.IsValid = result.Errors.Count == 0;

            return result;
        }

        /// <summary>
        /// 快速验证密码是否符合基本要求
        /// </summary>
        /// <param name="password">密码</param>
        /// <param name="minLength">最小长度（默认8）</param>
        /// <returns>是否有效</returns>
        public static bool IsValid(string? password, int minLength = 8)
        {
            if (string.IsNullOrEmpty(password) || password.Length < minLength)
            {
                return false;
            }

            bool hasLower = Regex.IsMatch(password, @"[a-z]");
            bool hasUpper = Regex.IsMatch(password, @"[A-Z]");
            bool hasDigit = DigitRegex.IsMatch(password);

            return hasLower && hasUpper && hasDigit;
        }

        #endregion

        #region 强度评估

        /// <summary>
        /// 评估密码强度
        /// </summary>
        /// <param name="password">密码</param>
        /// <returns>密码强度</returns>
        public static PasswordStrength EvaluateStrength(string? password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return PasswordStrength.VeryWeak;
            }

            bool hasLower = Regex.IsMatch(password, @"[a-z]");
            bool hasUpper = Regex.IsMatch(password, @"[A-Z]");
            bool hasDigit = DigitRegex.IsMatch(password);
            bool hasSpecial = SpecialCharRegex.IsMatch(password);

            int score = CalculateScore(password, hasLower, hasUpper, hasDigit, hasSpecial);
            return GetStrengthFromScore(score);
        }

        /// <summary>
        /// 获取密码强度分数（0-100）
        /// </summary>
        /// <param name="password">密码</param>
        /// <returns>强度分数</returns>
        public static int GetStrengthScore(string? password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return 0;
            }

            bool hasLower = Regex.IsMatch(password, @"[a-z]");
            bool hasUpper = Regex.IsMatch(password, @"[A-Z]");
            bool hasDigit = DigitRegex.IsMatch(password);
            bool hasSpecial = SpecialCharRegex.IsMatch(password);

            return CalculateScore(password, hasLower, hasUpper, hasDigit, hasSpecial);
        }

        /// <summary>
        /// 获取密码强度描述
        /// </summary>
        /// <param name="strength">密码强度</param>
        /// <returns>强度描述</returns>
        public static string GetStrengthDescription(PasswordStrength strength)
        {
            return strength switch
            {
                PasswordStrength.VeryWeak => "非常弱",
                PasswordStrength.Weak => "弱",
                PasswordStrength.Medium => "中等",
                PasswordStrength.Strong => "强",
                PasswordStrength.VeryStrong => "非常强",
                _ => "未知"
            };
        }

        #endregion

        #region 生成方法

        /// <summary>
        /// 生成随机密码
        /// </summary>
        /// <param name="length">密码长度（默认12）</param>
        /// <param name="includeLowercase">包含小写字母（默认true）</param>
        /// <param name="includeUppercase">包含大写字母（默认true）</param>
        /// <param name="includeDigits">包含数字（默认true）</param>
        /// <param name="includeSpecialChars">包含特殊字符（默认true）</param>
        /// <returns>随机密码</returns>
        public static string GenerateRandom(
            int length = 12,
            bool includeLowercase = true,
            bool includeUppercase = true,
            bool includeDigits = true,
            bool includeSpecialChars = true)
        {
            const string lowercase = "abcdefghijklmnopqrstuvwxyz";
            const string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string digits = "0123456789";
            const string special = "!@#$%^&*()_+-=[]{}|;:',.<>?";

            string charSet = "";
            if (includeLowercase) charSet += lowercase;
            if (includeUppercase) charSet += uppercase;
            if (includeDigits) charSet += digits;
            if (includeSpecialChars) charSet += special;

            if (string.IsNullOrEmpty(charSet))
            {
                charSet = lowercase + digits;
            }

            var charArray = charSet.ToCharArray();
            var password = new char[length];
            for (int i = 0; i < length; i++)
            {
                password[i] = MathCategory.RandomUtil.GetRandomElement(charArray);
            }

            return new string(password);
        }

        /// <summary>
        /// 生成强密码（确保包含所有字符类型）
        /// </summary>
        /// <param name="length">密码长度（默认16）</param>
        /// <returns>强密码</returns>
        public static string GenerateStrong(int length = 16)
        {
            const string lowercase = "abcdefghijklmnopqrstuvwxyz";
            const string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string digits = "0123456789";
            const string special = "!@#$%^&*()_+-=";

            if (length < 4)
            {
                length = 4;
            }

            // 确保每种字符类型至少有一个
            var password = new List<char>();
            password.Add(MathCategory.RandomUtil.GetRandomElement(lowercase.ToCharArray()));
            password.Add(MathCategory.RandomUtil.GetRandomElement(uppercase.ToCharArray()));
            password.Add(MathCategory.RandomUtil.GetRandomElement(digits.ToCharArray()));
            password.Add(MathCategory.RandomUtil.GetRandomElement(special.ToCharArray()));

            // 填充剩余字符
            string allChars = lowercase + uppercase + digits + special;
            for (int i = 4; i < length; i++)
            {
                password.Add(MathCategory.RandomUtil.GetRandomElement(allChars.ToCharArray()));
            }

            // 随机打乱顺序
            for (int i = password.Count - 1; i > 0; i--)
            {
                int j = MathCategory.RandomUtil.RandomInt(0, i + 1);
                (password[i], password[j]) = (password[j], password[i]);
            }

            return new string(password.ToArray());
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 计算密码强度分数
        /// </summary>
        private static int CalculateScore(string password, bool hasLower, bool hasUpper, bool hasDigit, bool hasSpecial)
        {
            int score = 0;

            // 长度分数（最多40分）
            score += Math.Min(password.Length * 4, 40);

            // 字符类型分数（每种类型10分，最多40分）
            if (hasLower) score += 10;
            if (hasUpper) score += 10;
            if (hasDigit) score += 10;
            if (hasSpecial) score += 10;

            // 混合奖励（最多10分）
            int typeCount = (hasLower ? 1 : 0) + (hasUpper ? 1 : 0) + (hasDigit ? 1 : 0) + (hasSpecial ? 1 : 0);
            if (typeCount >= 3) score += 5;
            if (typeCount == 4) score += 5;

            // 惩罚
            // 常见弱密码
            if (CommonPasswords.Contains(password))
            {
                score = Math.Max(0, score - 30);
            }

            // 全部相同字符
            if (AllSameChar(password))
            {
                score = Math.Max(0, score - 20);
            }

            // 连续字符
            if (HasSequentialChars(password))
            {
                score = Math.Max(0, score - 10);
            }

            return Math.Min(100, Math.Max(0, score));
        }

        /// <summary>
        /// 根据分数获取强度等级
        /// </summary>
        private static PasswordStrength GetStrengthFromScore(int score)
        {
            if (score < 20) return PasswordStrength.VeryWeak;
            if (score < 40) return PasswordStrength.Weak;
            if (score < 60) return PasswordStrength.Medium;
            if (score < 80) return PasswordStrength.Strong;
            return PasswordStrength.VeryStrong;
        }

        /// <summary>
        /// 检查是否所有字符相同
        /// </summary>
        private static bool AllSameChar(string str)
        {
            if (string.IsNullOrEmpty(str)) return true;
            char first = str[0];
            foreach (char c in str)
            {
                if (c != first) return false;
            }
            return true;
        }

        /// <summary>
        /// 检查是否有连续重复字符
        /// </summary>
        private static bool HasRepeatingChars(string str, int count)
        {
            if (string.IsNullOrEmpty(str) || str.Length < count) return false;

            for (int i = 0; i <= str.Length - count; i++)
            {
                bool allSame = true;
                for (int j = 1; j < count; j++)
                {
                    if (str[i + j] != str[i])
                    {
                        allSame = false;
                        break;
                    }
                }
                if (allSame) return true;
            }
            return false;
        }

        /// <summary>
        /// 检查是否有连续递增/递减字符
        /// </summary>
        private static bool HasSequentialChars(string str)
        {
            if (string.IsNullOrEmpty(str) || str.Length < 3) return false;

            string lower = str.ToLower();

            // 检查字母和数字序列
            for (int i = 0; i <= lower.Length - 3; i++)
            {
                // 检查递增
                if (lower[i + 1] == lower[i] + 1 && lower[i + 2] == lower[i] + 2)
                {
                    return true;
                }
                // 检查递减
                if (lower[i + 1] == lower[i] - 1 && lower[i + 2] == lower[i] - 2)
                {
                    return true;
                }
            }

            // 检查键盘序列
            foreach (string seq in KeyboardSequences)
            {
                if (seq.Contains(lower.Substring(0, Math.Min(3, lower.Length))))
                {
                    for (int i = 0; i <= lower.Length - 3; i++)
                    {
                        if (seq.Contains(lower.Substring(i, 3)))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        #endregion
    }
}
