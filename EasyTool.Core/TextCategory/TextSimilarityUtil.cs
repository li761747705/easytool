using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyTool.TextCategory
{
    /// <summary>
    /// 文本相似度算法
    /// </summary>
    public enum SimilarityAlgorithm
    {
        /// <summary>
        /// Levenshtein 编辑距离
        /// </summary>
        Levenshtein,

        /// <summary>
        /// Jaccard 相似度
        /// </summary>
        Jaccard,

        /// <summary>
        /// Cosine 余弦相似度
        /// </summary>
        Cosine,

        /// <summary>
        /// Dice 系数
        /// </summary>
        Dice,

        /// <summary>
        /// Jaro-Winkler 相似度
        /// </summary>
        JaroWinkler,

        /// <summary>
        /// Hamming 距离
        /// </summary>
        Hamming
    }

    /// <summary>
    /// 文本相似度工具类
    /// 提供多种文本相似度计算算法
    /// </summary>
    public static class TextSimilarityUtil
    {
        /// <summary>
        /// 计算文本相似度
        /// </summary>
        /// <param name="text1">文本1</param>
        /// <param name="text2">文本2</param>
        /// <param name="algorithm">算法</param>
        /// <returns>相似度（0-1）</returns>
        public static double Calculate(string text1, string text2, SimilarityAlgorithm algorithm = SimilarityAlgorithm.Levenshtein)
        {
            return algorithm switch
            {
                SimilarityAlgorithm.Levenshtein => LevenshteinSimilarity(text1, text2),
                SimilarityAlgorithm.Jaccard => JaccardSimilarity(text1, text2),
                SimilarityAlgorithm.Cosine => CosineSimilarity(text1, text2),
                SimilarityAlgorithm.Dice => DiceSimilarity(text1, text2),
                SimilarityAlgorithm.JaroWinkler => JaroWinklerSimilarity(text1, text2),
                SimilarityAlgorithm.Hamming => HammingSimilarity(text1, text2),
                _ => throw new ArgumentException($"不支持的算法: {algorithm}")
            };
        }

        /// <summary>
        /// 计算编辑距离
        /// </summary>
        /// <param name="text1">文本1</param>
        /// <param name="text2">文本2</param>
        /// <returns>编辑距离</returns>
        public static int LevenshteinDistance(string text1, string text2)
        {
            if (string.IsNullOrEmpty(text1))
                return text2?.Length ?? 0;

            if (string.IsNullOrEmpty(text2))
                return text1.Length;

            var matrix = new int[text1.Length + 1, text2.Length + 1];

            for (int i = 0; i <= text1.Length; i++)
                matrix[i, 0] = i;

            for (int j = 0; j <= text2.Length; j++)
                matrix[0, j] = j;

            for (int i = 1; i <= text1.Length; i++)
            {
                for (int j = 1; j <= text2.Length; j++)
                {
                    var cost = text1[i - 1] == text2[j - 1] ? 0 : 1;

                    matrix[i, j] = Math.Min(
                        Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                        matrix[i - 1, j - 1] + cost);
                }
            }

            return matrix[text1.Length, text2.Length];
        }

        /// <summary>
        /// Levenshtein 相似度
        /// </summary>
        public static double LevenshteinSimilarity(string text1, string text2)
        {
            if (string.IsNullOrEmpty(text1) && string.IsNullOrEmpty(text2))
                return 1.0;

            if (string.IsNullOrEmpty(text1) || string.IsNullOrEmpty(text2))
                return 0.0;

            var distance = LevenshteinDistance(text1, text2);
            var maxLength = Math.Max(text1.Length, text2.Length);

            return 1.0 - (double)distance / maxLength;
        }

        /// <summary>
        /// Jaccard 相似度
        /// </summary>
        public static double JaccardSimilarity(string text1, string text2)
        {
            if (string.IsNullOrEmpty(text1) && string.IsNullOrEmpty(text2))
                return 1.0;

            if (string.IsNullOrEmpty(text1) || string.IsNullOrEmpty(text2))
                return 0.0;

            var set1 = GetNgrams(text1, 2);
            var set2 = GetNgrams(text2, 2);

            var intersection = set1.Intersect(set2).Count();
            var union = set1.Union(set2).Count();

            return union == 0 ? 0.0 : (double)intersection / union;
        }

        /// <summary>
        /// Cosine 余弦相似度
        /// </summary>
        public static double CosineSimilarity(string text1, string text2)
        {
            if (string.IsNullOrEmpty(text1) && string.IsNullOrEmpty(text2))
                return 1.0;

            if (string.IsNullOrEmpty(text1) || string.IsNullOrEmpty(text2))
                return 0.0;

            var vector1 = GetTermFrequency(text1);
            var vector2 = GetTermFrequency(text2);

            var allTerms = vector1.Keys.Union(vector2.Keys).ToList();

            double dotProduct = 0;
            double magnitude1 = 0;
            double magnitude2 = 0;

            foreach (var term in allTerms)
            {
                var v1 = vector1.TryGetValue(term, out var val1) ? val1 : 0;
                var v2 = vector2.TryGetValue(term, out var val2) ? val2 : 0;

                dotProduct += v1 * v2;
                magnitude1 += v1 * v1;
                magnitude2 += v2 * v2;
            }

            magnitude1 = Math.Sqrt(magnitude1);
            magnitude2 = Math.Sqrt(magnitude2);

            if (magnitude1 == 0 || magnitude2 == 0)
                return 0.0;

            return dotProduct / (magnitude1 * magnitude2);
        }

        /// <summary>
        /// Dice 系数
        /// </summary>
        public static double DiceSimilarity(string text1, string text2)
        {
            if (string.IsNullOrEmpty(text1) && string.IsNullOrEmpty(text2))
                return 1.0;

            if (string.IsNullOrEmpty(text1) || string.IsNullOrEmpty(text2))
                return 0.0;

            var set1 = GetNgrams(text1, 2);
            var set2 = GetNgrams(text2, 2);

            var intersection = set1.Intersect(set2).Count();

            return (2.0 * intersection) / (set1.Count + set2.Count);
        }

        /// <summary>
        /// Jaro-Winkler 相似度
        /// </summary>
        public static double JaroWinklerSimilarity(string text1, string text2)
        {
            var jaroSimilarity = JaroSimilarity(text1, text2);

            // 计算 common prefix 长度（最多4个字符）
            var prefixLength = 0;
            var minLength = Math.Min(Math.Min(text1.Length, text2.Length), 4);

            for (int i = 0; i < minLength; i++)
            {
                if (text1[i] == text2[i])
                    prefixLength++;
                else
                    break;
            }

            // Winkler 修正
            return jaroSimilarity + (prefixLength * 0.1 * (1 - jaroSimilarity));
        }

        /// <summary>
        /// Jaro 相似度
        /// </summary>
        public static double JaroSimilarity(string text1, string text2)
        {
            if (string.IsNullOrEmpty(text1) && string.IsNullOrEmpty(text2))
                return 1.0;

            if (string.IsNullOrEmpty(text1) || string.IsNullOrEmpty(text2))
                return 0.0;

            if (text1 == text2)
                return 1.0;

            var matchDistance = Math.Max(text1.Length, text2.Length) / 2 - 1;
            var matches1 = new bool[text1.Length];
            var matches2 = new bool[text2.Length];

            var matches = 0;
            var transpositions = 0;

            // 查找匹配字符
            for (int i = 0; i < text1.Length; i++)
            {
                var start = Math.Max(0, i - matchDistance);
                var end = Math.Min(i + matchDistance + 1, text2.Length);

                for (int j = start; j < end; j++)
                {
                    if (matches2[j] || text1[i] != text2[j])
                        continue;

                    matches1[i] = true;
                    matches2[j] = true;
                    matches++;
                    break;
                }
            }

            if (matches == 0)
                return 0.0;

            // 计算转置次数
            var k = 0;
            for (int i = 0; i < text1.Length; i++)
            {
                if (!matches1[i])
                    continue;

                while (!matches2[k])
                    k++;

                if (text1[i] != text2[k])
                    transpositions++;

                k++;
            }

            return ((double)matches / text1.Length +
                    (double)matches / text2.Length +
                    (matches - transpositions / 2.0) / matches) / 3.0;
        }

        /// <summary>
        /// Hamming 距离（仅适用于等长字符串）
        /// </summary>
        public static int HammingDistance(string text1, string text2)
        {
            if (text1.Length != text2.Length)
                throw new ArgumentException("Hamming 距离要求两个字符串长度相等");

            return text1.Zip(text2, (c1, c2) => c1 != c2 ? 1 : 0).Sum();
        }

        /// <summary>
        /// Hamming 相似度
        /// </summary>
        public static double HammingSimilarity(string text1, string text2)
        {
            if (text1.Length != text2.Length)
                return 0.0;

            if (text1.Length == 0)
                return 1.0;

            var distance = HammingDistance(text1, text2);
            return 1.0 - (double)distance / text1.Length;
        }

        /// <summary>
        /// 查找最相似的文本
        /// </summary>
        /// <param name="query">查询文本</param>
        /// <param name="candidates">候选文本列表</param>
        /// <param name="algorithm">算法</param>
        /// <param name="topN">返回前N个</param>
        /// <returns>相似度排序结果</returns>
        public static List<(string Text, double Similarity)> FindMostSimilar(
            string query,
            IEnumerable<string> candidates,
            SimilarityAlgorithm algorithm = SimilarityAlgorithm.Levenshtein,
            int topN = 5)
        {
            return candidates
                .Select(c => (Text: c, Similarity: Calculate(query, c, algorithm)))
                .OrderByDescending(r => r.Similarity)
                .Take(topN)
                .ToList();
        }

        /// <summary>
        /// 检查是否相似（超过阈值）
        /// </summary>
        /// <param name="text1">文本1</param>
        /// <param name="text2">文本2</param>
        /// <param name="threshold">阈值（0-1）</param>
        /// <param name="algorithm">算法</param>
        /// <returns>是否相似</returns>
        public static bool IsSimilar(
            string text1,
            string text2,
            double threshold = 0.8,
            SimilarityAlgorithm algorithm = SimilarityAlgorithm.Levenshtein)
        {
            return Calculate(text1, text2, algorithm) >= threshold;
        }

        /// <summary>
        /// 模糊搜索
        /// </summary>
        /// <param name="query">查询文本</param>
        /// <param name="candidates">候选文本列表</param>
        /// <param name="threshold">阈值</param>
        /// <param name="algorithm">算法</param>
        /// <returns>匹配结果</returns>
        public static List<string> FuzzySearch(
            string query,
            IEnumerable<string> candidates,
            double threshold = 0.6,
            SimilarityAlgorithm algorithm = SimilarityAlgorithm.Levenshtein)
        {
            return candidates
                .Where(c => Calculate(query, c, algorithm) >= threshold)
                .ToList();
        }

        #region 私有方法

        private static HashSet<string> GetNgrams(string text, int n)
        {
            var ngrams = new HashSet<string>();

            if (string.IsNullOrEmpty(text) || text.Length < n)
            {
                ngrams.Add(text ?? "");
                return ngrams;
            }

            for (int i = 0; i <= text.Length - n; i++)
            {
                ngrams.Add(text.Substring(i, n));
            }

            return ngrams;
        }

        private static Dictionary<string, int> GetTermFrequency(string text)
        {
            var frequency = new Dictionary<string, int>();

            if (string.IsNullOrEmpty(text))
                return frequency;

            // 按字符分词
            foreach (var c in text)
            {
                var term = c.ToString();
                if (frequency.ContainsKey(term))
                    frequency[term]++;
                else
                    frequency[term] = 1;
            }

            return frequency;
        }

        #endregion
    }
}
