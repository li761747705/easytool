using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace EasyTool.SecurityCategory
{
    /// <summary>
    /// 密码强度工具类
    /// </summary>
    public static class PasswordStrengthUtil
    {
        // 为 netstandard2.1 提供 Math.Log2 的兼容实现
#if NETSTANDARD2_1
        private static double Log2(double x) => Math.Log(x, 2);
#else
        private static double Log2(double x) => Math.Log2(x);
#endif

        /// <summary>
        /// 检测密码强度
        /// </summary>
        public static PasswordStrengthResult CheckStrength(string password)
        {
            var result = new PasswordStrengthResult
            {
                Password = password
            };

            if (string.IsNullOrEmpty(password))
            {
                result.Score = 0;
                result.Level = PasswordStrengthLevel.VeryWeak;
                result.AddIssue("密码不能为空");
                return result;
            }

            var score = 0;
            var length = password.Length;

            // 长度评分
            if (length >= 8) score += 1;
            if (length >= 12) score += 1;
            if (length >= 16) score += 1;
            if (length < 6) result.AddIssue("密码长度不足6位");

            // 包含小写字母
            if (Regex.IsMatch(password, @"[a-z]"))
            {
                score += 1;
                result.HasLowerCase = true;
            }
            else
            {
                result.AddIssue("缺少小写字母");
            }

            // 包含大写字母
            if (Regex.IsMatch(password, @"[A-Z]"))
            {
                score += 1;
                result.HasUpperCase = true;
            }
            else
            {
                result.AddSuggestion("建议添加大写字母");
            }

            // 包含数字
            if (Regex.IsMatch(password, @"\d"))
            {
                score += 1;
                result.HasDigit = true;
            }
            else
            {
                result.AddIssue("缺少数字");
            }

            // 包含特殊字符
            if (Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>/?~` "))
            {
                score += 2;
                result.HasSpecialChar = true;
            }
            else
            {
                result.AddSuggestion("建议添加特殊字符");
            }

            // 检查连续字符
            if (HasConsecutiveChars(password, 3))
            {
                score -= 1;
                result.AddIssue("存在连续字符");
            }

            // 检查重复字符
            if (HasRepeatingChars(password, 3))
            {
                score -= 1;
                result.AddIssue("存在重复字符");
            }

            // 检查常见弱密码
            if (IsCommonPassword(password))
            {
                score = Math.Max(0, score - 3);
                result.AddIssue("这是一个常见弱密码");
            }

            // 计算熵
            result.Entropy = CalculateEntropy(password);

            // 最终评分
            result.Score = Math.Max(0, Math.Min(10, score));

            // 确定等级
            result.Level = result.Score switch
            {
                >= 8 => PasswordStrengthLevel.VeryStrong,
                >= 6 => PasswordStrengthLevel.Strong,
                >= 4 => PasswordStrengthLevel.Medium,
                >= 2 => PasswordStrengthLevel.Weak,
                _ => PasswordStrengthLevel.VeryWeak
            };

            return result;
        }

        /// <summary>
        /// 生成强密码
        /// </summary>
        public static string GenerateStrongPassword(int length = 16, PasswordOptions? options = null)
        {
            options ??= new PasswordOptions();
            var random = new Random();
            var password = new List<char>();

            const string lowerChars = "abcdefghijklmnopqrstuvwxyz";
            const string upperChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string digitChars = "0123456789";
            const string specialChars = "!@#$%^&*()_+-=[]{}|;:,.<>?";

            // 确保每种要求的字符至少有一个
            if (options.RequireLowerCase)
                password.Add(lowerChars[random.Next(lowerChars.Length)]);
            if (options.RequireUpperCase)
                password.Add(upperChars[random.Next(upperChars.Length)]);
            if (options.RequireDigit)
                password.Add(digitChars[random.Next(digitChars.Length)]);
            if (options.RequireSpecialChar)
                password.Add(specialChars[random.Next(specialChars.Length)]);

            // 构建可用字符集
            var allChars = "";
            if (options.AllowLowerCase) allChars += lowerChars;
            if (options.AllowUpperCase) allChars += upperChars;
            if (options.AllowDigit) allChars += digitChars;
            if (options.AllowSpecialChar) allChars += specialChars;

            if (string.IsNullOrEmpty(allChars))
                allChars = lowerChars + digitChars;

            // 排除相似字符
            if (options.ExcludeSimilarChars)
                allChars = Regex.Replace(allChars, @"[il1Lo0O]", "");

            // 排除歧义字符
            if (options.ExcludeAmbiguousChars)
                allChars = Regex.Replace(allChars, @"[{}[\]()""'`~,;:.<>\\/|]", "");

            // 填充剩余长度
            while (password.Count < length)
            {
                password.Add(allChars[random.Next(allChars.Length)]);
            }

            // 打乱顺序
            for (int i = password.Count - 1; i > 0; i--)
            {
                var j = random.Next(i + 1);
                (password[i], password[j]) = (password[j], password[i]);
            }

            return new string(password.ToArray());
        }

        /// <summary>
        /// 检查是否为常见弱密码
        /// </summary>
        public static bool IsCommonPassword(string password)
        {
            var commonPasswords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "password", "123456", "12345678", "qwerty", "abc123",
                "monkey", "master", "dragon", "111111", "baseball",
                "iloveyou", "trustno1", "sunshine", "princess", "welcome",
                "shadow", "superman", "michael", "football", "letmein",
                "password1", "password123", "admin", "root", "test"
            };

            return commonPasswords.Contains(password);
        }

        /// <summary>
        /// 计算密码熵
        /// </summary>
        public static double CalculateEntropy(string password)
        {
            if (string.IsNullOrEmpty(password))
                return 0;

            var charPool = 0;
            if (Regex.IsMatch(password, @"[a-z]")) charPool += 26;
            if (Regex.IsMatch(password, @"[A-Z]")) charPool += 26;
            if (Regex.IsMatch(password, @"\d")) charPool += 10;
            if (Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>/?~`]")) charPool += 32;

            if (charPool == 0) return 0;

            return password.Length * Log2(charPool);
        }

        /// <summary>
        /// 检查密码是否过期
        /// </summary>
        public static bool IsPasswordExpired(DateTime lastChangeDate, int maxAgeDays = 90)
        {
            return (DateTime.Now - lastChangeDate).TotalDays > maxAgeDays;
        }

        /// <summary>
        /// 估算密码破解时间
        /// </summary>
        public static TimeSpan EstimateCrackTime(string password, int guessesPerSecond = 1_000_000_000)
        {
            var entropy = CalculateEntropy(password);
            var combinations = Math.Pow(2, entropy);
            var seconds = combinations / guessesPerSecond / 2; // 平均尝试次数

            if (seconds < 1) return TimeSpan.FromMilliseconds(seconds * 1000);
            if (seconds < 60) return TimeSpan.FromSeconds(seconds);
            if (seconds < 3600) return TimeSpan.FromMinutes(seconds / 60);
            if (seconds < 86400) return TimeSpan.FromHours(seconds / 3600);
            if (seconds < 2592000) return TimeSpan.FromDays(seconds / 86400);
            if (seconds < 31536000) return TimeSpan.FromDays(seconds / 86400);
            
            return TimeSpan.FromDays(seconds / 86400);
        }

        private static bool HasConsecutiveChars(string password, int count)
        {
            for (int i = 0; i <= password.Length - count; i++)
            {
                var consecutive = true;
                for (int j = 1; j < count && consecutive; j++)
                {
                    if (password[i + j] != password[i] + j && password[i + j] != password[i] - j)
                    {
                        consecutive = false;
                    }
                }
                if (consecutive) return true;
            }
            return false;
        }

        private static bool HasRepeatingChars(string password, int count)
        {
            for (int i = 0; i <= password.Length - count; i++)
            {
                var repeating = true;
                for (int j = 1; j < count && repeating; j++)
                {
                    if (password[i + j] != password[i])
                    {
                        repeating = false;
                    }
                }
                if (repeating) return true;
            }
            return false;
        }
    }

    /// <summary>
    /// 密码强度结果
    /// </summary>
    public class PasswordStrengthResult
    {
        public string Password { get; set; } = "";
        public int Score { get; set; }
        public PasswordStrengthLevel Level { get; set; }
        public double Entropy { get; set; }
        public bool HasLowerCase { get; set; }
        public bool HasUpperCase { get; set; }
        public bool HasDigit { get; set; }
        public bool HasSpecialChar { get; set; }
        public List<string> Issues { get; } = new();
        public List<string> Suggestions { get; } = new();

        internal void AddIssue(string issue) => Issues.Add(issue);
        internal void AddSuggestion(string suggestion) => Suggestions.Add(suggestion);

        public string LevelDescription => Level switch
        {
            PasswordStrengthLevel.VeryStrong => "非常强",
            PasswordStrengthLevel.Strong => "强",
            PasswordStrengthLevel.Medium => "中等",
            PasswordStrengthLevel.Weak => "弱",
            _ => "非常弱"
        };
    }

    /// <summary>
    /// 密码强度等级
    /// </summary>
    public enum PasswordStrengthLevel
    {
        VeryWeak = 0,
        Weak = 1,
        Medium = 2,
        Strong = 3,
        VeryStrong = 4
    }

    /// <summary>
    /// 密码生成选项
    /// </summary>
    public class PasswordOptions
    {
        public bool AllowLowerCase { get; set; } = true;
        public bool AllowUpperCase { get; set; } = true;
        public bool AllowDigit { get; set; } = true;
        public bool AllowSpecialChar { get; set; } = true;
        public bool RequireLowerCase { get; set; } = true;
        public bool RequireUpperCase { get; set; } = true;
        public bool RequireDigit { get; set; } = true;
        public bool RequireSpecialChar { get; set; } = false;
        public bool ExcludeSimilarChars { get; set; } = true;
        public bool ExcludeAmbiguousChars { get; set; } = false;
    }
}
