using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace EasyTool.BusinessCategory
{
    /// <summary>
    /// 密码生成器工具类
    /// 提供安全的随机密码生成功能
    /// </summary>
    public static class PasswordGenerator
    {
        #region 字符集定义

        /// <summary>
        /// 小写字母
        /// </summary>
        public const string LowerCase = "abcdefghijklmnopqrstuvwxyz";

        /// <summary>
        /// 大写字母
        /// </summary>
        public const string UpperCase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        /// <summary>
        /// 数字
        /// </summary>
        public const string Digits = "0123456789";

        /// <summary>
        /// 特殊字符
        /// </summary>
        public const string SpecialChars = "!@#$%^&*()_+-=[]{}|;:,.<>?";

        /// <summary>
        /// 易混淆字符（不推荐使用）
        /// </summary>
        public const string AmbiguousChars = "l1IO0";

        #endregion

        #region 生成密码

        /// <summary>
        /// 生成随机密码
        /// </summary>
        /// <param name="length">密码长度（默认12）</param>
        /// <param name="includeLowerCase">包含小写字母</param>
        /// <param name="includeUpperCase">包含大写字母</param>
        /// <param name="includeDigits">包含数字</param>
        /// <param name="includeSpecialChars">包含特殊字符</param>
        /// <param name="excludeAmbiguous">排除易混淆字符</param>
        /// <returns>生成的密码</returns>
        public static string Generate(
            int length = 12,
            bool includeLowerCase = true,
            bool includeUpperCase = true,
            bool includeDigits = true,
            bool includeSpecialChars = true,
            bool excludeAmbiguous = true)
        {
            if (length < 4)
                throw new ArgumentException("密码长度至少为4位", nameof(length));

            var charSets = new List<string>();
            var allChars = new StringBuilder();

            if (includeLowerCase)
            {
                var chars = excludeAmbiguous ? RemoveAmbiguous(LowerCase) : LowerCase;
                charSets.Add(chars);
                allChars.Append(chars);
            }

            if (includeUpperCase)
            {
                var chars = excludeAmbiguous ? RemoveAmbiguous(UpperCase) : UpperCase;
                charSets.Add(chars);
                allChars.Append(chars);
            }

            if (includeDigits)
            {
                var chars = excludeAmbiguous ? RemoveAmbiguous(Digits) : Digits;
                charSets.Add(chars);
                allChars.Append(chars);
            }

            if (includeSpecialChars)
            {
                charSets.Add(SpecialChars);
                allChars.Append(SpecialChars);
            }

            if (charSets.Count == 0)
                throw new ArgumentException("至少需要选择一种字符类型");

            var password = new char[length];
            var allCharsStr = allChars.ToString();

            // 确保每种字符类型至少有一个
            using var rng = RandomNumberGenerator.Create();
            for (int i = 0; i < charSets.Count && i < length; i++)
            {
                password[i] = GetRandomChar(rng, charSets[i]);
            }

            // 填充剩余位置
            for (int i = charSets.Count; i < length; i++)
            {
                password[i] = GetRandomChar(rng, allCharsStr);
            }

            // 随机打乱
            Shuffle(rng, password);

            return new string(password);
        }

        /// <summary>
        /// 生成强密码（16位，包含所有字符类型）
        /// </summary>
        /// <returns>强密码</returns>
        public static string GenerateStrong()
        {
            return Generate(16, true, true, true, true, true);
        }

        /// <summary>
        /// 生成PIN码（纯数字）
        /// </summary>
        /// <param name="length">长度（默认6位）</param>
        /// <returns>PIN码</returns>
        public static string GeneratePin(int length = 6)
        {
            return Generate(length, false, false, true, false, false);
        }

        /// <summary>
        /// 生成密码短语（多个随机单词组合）
        /// </summary>
        /// <param name="wordCount">单词数量</param>
        /// <param name="separator">分隔符</param>
        /// <returns>密码短语</returns>
        public static string GeneratePassphrase(int wordCount = 4, string separator = "-")
        {
            var words = new[]
            {
                "apple", "banana", "cherry", "dragon", "elephant", "forest", "garden", "house",
                "island", "jungle", "kitchen", "lemon", "mountain", "night", "ocean", "piano",
                "queen", "river", "sunset", "tiger", "umbrella", "valley", "water", "yellow",
                "zebra", "bridge", "castle", "diamond", "energy", "flower", "golden", "harbor",
                "insect", "journey", "kingdom", "lantern", "market", "nature", "orange", "palace",
                "rainbow", "silver", "thunder", "violet", "window", "crystal", "desert", "empire"
            };

            using var rng = RandomNumberGenerator.Create();
            var selected = new List<string>();

            for (int i = 0; i < wordCount; i++)
            {
                var index = GetRandomInt(rng, words.Length);
                selected.Add(words[index]);
            }

            return string.Join(separator, selected);
        }

        /// <summary>
        /// 批量生成密码
        /// </summary>
        /// <param name="count">数量</param>
        /// <param name="length">密码长度</param>
        /// <returns>密码列表</returns>
        public static List<string> GenerateBatch(int count, int length = 12)
        {
            var passwords = new List<string>();
            for (int i = 0; i < count; i++)
            {
                passwords.Add(Generate(length));
            }
            return passwords;
        }

        #endregion

        #region 密码强度检测

        /// <summary>
        /// 检测密码强度
        /// </summary>
        /// <param name="password">密码</param>
        /// <returns>强度等级（Weak, Fair, Good, Strong, VeryStrong）</returns>
        public static PasswordStrength CheckStrength(string password)
        {
            if (string.IsNullOrEmpty(password))
                return PasswordStrength.Weak;

            int score = 0;

            // 长度评分
            if (password.Length >= 8) score++;
            if (password.Length >= 12) score++;
            if (password.Length >= 16) score++;

            // 字符类型评分
            if (password.Any(char.IsLower)) score++;
            if (password.Any(char.IsUpper)) score++;
            if (password.Any(char.IsDigit)) score++;
            if (password.Any(c => SpecialChars.Contains(c))) score++;

            // 连续字符检查
            if (!HasConsecutiveChars(password, 3)) score++;

            // 重复字符检查
            if (!HasRepeatingChars(password, 3)) score++;

            return score switch
            {
                <= 2 => PasswordStrength.Weak,
                3 or 4 => PasswordStrength.Fair,
                5 or 6 => PasswordStrength.Good,
                7 => PasswordStrength.Strong,
                _ => PasswordStrength.VeryStrong
            };
        }

        /// <summary>
        /// 获取密码强度描述
        /// </summary>
        /// <param name="password">密码</param>
        /// <returns>强度描述</returns>
        public static string GetStrengthDescription(string password)
        {
            return CheckStrength(password) switch
            {
                PasswordStrength.Weak => "弱 - 建议增加长度和字符类型",
                PasswordStrength.Fair => "一般 - 建议增加更多字符类型",
                PasswordStrength.Good => "良好 - 密码强度适中",
                PasswordStrength.Strong => "强 - 密码强度很高",
                PasswordStrength.VeryStrong => "非常强 - 密码强度极高",
                _ => "未知"
            };
        }

        #endregion

        #region 辅助方法

        private static string RemoveAmbiguous(string chars)
        {
            var result = new StringBuilder();
            foreach (var c in chars)
            {
                if (!AmbiguousChars.Contains(c))
                    result.Append(c);
            }
            return result.ToString();
        }

        private static char GetRandomChar(RandomNumberGenerator rng, string chars)
        {
            var index = GetRandomInt(rng, chars.Length);
            return chars[index];
        }

        private static int GetRandomInt(RandomNumberGenerator rng, int max)
        {
            var bytes = new byte[4];
            rng.GetBytes(bytes);
            return Math.Abs(BitConverter.ToInt32(bytes, 0)) % max;
        }

        private static void Shuffle(RandomNumberGenerator rng, char[] array)
        {
            for (int i = array.Length - 1; i > 0; i--)
            {
                var j = GetRandomInt(rng, i + 1);
                (array[i], array[j]) = (array[j], array[i]);
            }
        }

        private static bool HasConsecutiveChars(string password, int count)
        {
            for (int i = 0; i <= password.Length - count; i++)
            {
                bool consecutive = true;
                for (int j = 1; j < count && consecutive; j++)
                {
                    if (password[i + j] - password[i + j - 1] != 1)
                        consecutive = false;
                }
                if (consecutive) return true;
            }
            return false;
        }

        private static bool HasRepeatingChars(string password, int count)
        {
            for (int i = 0; i <= password.Length - count; i++)
            {
                bool repeating = true;
                for (int j = 1; j < count && repeating; j++)
                {
                    if (password[i + j] != password[i])
                        repeating = false;
                }
                if (repeating) return true;
            }
            return false;
        }

        #endregion

        #region 枚举

        /// <summary>
        /// 密码强度等级
        /// </summary>
        public enum PasswordStrength
        {
            /// <summary>
            /// 弱
            /// </summary>
            Weak,
            /// <summary>
            /// 一般
            /// </summary>
            Fair,
            /// <summary>
            /// 良好
            /// </summary>
            Good,
            /// <summary>
            /// 强
            /// </summary>
            Strong,
            /// <summary>
            /// 非常强
            /// </summary>
            VeryStrong
        }

        #endregion
    }
}