using System;
using System.Text;

namespace EasyTool.CodeCategory
{
    /// <summary>
    /// 凯撒密码工具类
    /// 凯撒密码是一种简单的替换密码，将字母表中的每个字母替换为固定偏移后的字母
    /// 历史上由 Julius Caesar 使用
    /// 注意：这不是安全的加密方式，仅用于教育和娱乐
    /// </summary>
    public static class CaesarCipherUtil
    {
        /// <summary>
        /// 使用凯撒密码加密
        /// </summary>
        /// <param name="text">明文</param>
        /// <param name="shift">偏移量（1-25）</param>
        /// <returns>密文</returns>
        public static string Encrypt(string text, int shift)
        {
            return Rotate(text, shift);
        }

        /// <summary>
        /// 使用凯撒密码解密
        /// </summary>
        /// <param name="text">密文</param>
        /// <param name="shift">偏移量（1-25）</param>
        /// <returns>明文</returns>
        public static string Decrypt(string text, int shift)
        {
            return Rotate(text, -shift);
        }

        /// <summary>
        /// 旋转字母
        /// </summary>
        /// <param name="text">文本</param>
        /// <param name="shift">偏移量</param>
        /// <returns>旋转后的文本</returns>
        public static string Rotate(string text, int shift)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            shift = ((shift % 26) + 26) % 26;

            var result = new StringBuilder(text.Length);

            foreach (char c in text)
            {
                if (c >= 'A' && c <= 'Z')
                {
                    result.Append((char)('A' + (c - 'A' + shift) % 26));
                }
                else if (c >= 'a' && c <= 'z')
                {
                    result.Append((char)('a' + (c - 'a' + shift) % 26));
                }
                else
                {
                    result.Append(c);
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// 暴力破解凯撒密码（返回所有可能的结果）
        /// </summary>
        /// <param name="cipherText">密文</param>
        /// <returns>所有 26 种可能的结果</returns>
        public static string[] BruteForce(string cipherText)
        {
            var results = new string[26];
            for (int i = 0; i < 26; i++)
            {
                results[i] = Decrypt(cipherText, i);
            }
            return results;
        }

        /// <summary>
        /// 使用频率分析破解凯撒密码
        /// </summary>
        /// <param name="cipherText">密文</param>
        /// <returns>最可能的偏移量和明文</returns>
        public static (int Shift, string PlainText) Crack(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
                return (0, string.Empty);

            // 英语字母频率
            double[] englishFreq = new double[]
            {
                0.08167, 0.01492, 0.02782, 0.04253, 0.12702, 0.02228, 0.02015,
                0.06094, 0.06966, 0.00153, 0.00772, 0.04025, 0.02406, 0.06749,
                0.07507, 0.01929, 0.00095, 0.05987, 0.06327, 0.09056, 0.02758,
                0.00978, 0.02360, 0.00150, 0.01974, 0.00074
            };

            int bestShift = 0;
            double bestScore = double.MinValue;

            for (int shift = 0; shift < 26; shift++)
            {
                string plainText = Decrypt(cipherText, shift);
                double score = CalculateFrequencyScore(plainText, englishFreq);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestShift = shift;
                }
            }

            return (bestShift, Decrypt(cipherText, bestShift));
        }

        private static double CalculateFrequencyScore(string text, double[] expectedFreq)
        {
            int[] counts = new int[26];
            int total = 0;

            foreach (char c in text)
            {
                if (c >= 'A' && c <= 'Z')
                {
                    counts[c - 'A']++;
                    total++;
                }
                else if (c >= 'a' && c <= 'z')
                {
                    counts[c - 'a']++;
                    total++;
                }
            }

            if (total == 0)
                return 0;

            double score = 0;
            for (int i = 0; i < 26; i++)
            {
                double observedFreq = (double)counts[i] / total;
                score += Math.Sqrt(expectedFreq[i] * observedFreq);
            }

            return score;
        }
    }
}
