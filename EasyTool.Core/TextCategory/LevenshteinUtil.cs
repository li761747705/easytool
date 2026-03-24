using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyTool.TextCategory
{
    /// <summary>
    /// 编辑距离工具类
    /// 提供各种字符串相似度计算方法
    /// </summary>
    public static class LevenshteinUtil
    {
        /// <summary>
        /// 计算Levenshtein编辑距离
        /// </summary>
        public static int Distance(string source, string target)
        {
            if (string.IsNullOrEmpty(source))
                return target?.Length ?? 0;
            if (string.IsNullOrEmpty(target))
                return source.Length;

            int m = source.Length;
            int n = target.Length;

            // 优化空间：只使用两行
            int[] prev = new int[n + 1];
            int[] curr = new int[n + 1];

            // 初始化第一行
            for (int j = 0; j <= n; j++)
                prev[j] = j;

            for (int i = 1; i <= m; i++)
            {
                curr[0] = i;

                for (int j = 1; j <= n; j++)
                {
                    int cost = source[i - 1] == target[j - 1] ? 0 : 1;

                    curr[j] = Math.Min(
                        Math.Min(prev[j] + 1, curr[j - 1] + 1),
                        prev[j - 1] + cost);
                }

                // 交换行
                (prev, curr) = (curr, prev);
            }

            return prev[n];
        }

        /// <summary>
        /// 计算相似度（0-1）
        /// </summary>
        public static double Similarity(string source, string target)
        {
            if (string.IsNullOrEmpty(source) && string.IsNullOrEmpty(target))
                return 1.0;
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(target))
                return 0.0;

            int maxLen = Math.Max(source.Length, target.Length);
            if (maxLen == 0) return 1.0;

            int distance = Distance(source, target);
            return 1.0 - (double)distance / maxLen;
        }

        /// <summary>
        /// 获取编辑操作序列
        /// </summary>
        public static List<EditOperation> GetEditOperations(string source, string target)
        {
            var operations = new List<EditOperation>();

            if (string.IsNullOrEmpty(source))
            {
                for (int i = 0; i < (target?.Length ?? 0); i++)
                    operations.Add(new EditOperation(EditType.Insert, i, target[i].ToString()));
                return operations;
            }

            if (string.IsNullOrEmpty(target))
            {
                for (int i = 0; i < source.Length; i++)
                    operations.Add(new EditOperation(EditType.Delete, i, source[i].ToString()));
                return operations;
            }

            int m = source.Length;
            int n = target.Length;

            // 构建完整DP表
            int[,] dp = new int[m + 1, n + 1];

            for (int i = 0; i <= m; i++) dp[i, 0] = i;
            for (int j = 0; j <= n; j++) dp[0, j] = j;

            for (int i = 1; i <= m; i++)
            {
                for (int j = 1; j <= n; j++)
                {
                    int cost = source[i - 1] == target[j - 1] ? 0 : 1;
                    dp[i, j] = Math.Min(
                        Math.Min(dp[i - 1, j] + 1, dp[i, j - 1] + 1),
                        dp[i - 1, j - 1] + cost);
                }
            }

            // 回溯获取操作
            int x = m, y = n;
            while (x > 0 || y > 0)
            {
                if (x > 0 && y > 0 && source[x - 1] == target[y - 1])
                {
                    operations.Add(new EditOperation(EditType.Match, x - 1, source[x - 1].ToString()));
                    x--; y--;
                }
                else if (x > 0 && y > 0 && dp[x, y] == dp[x - 1, y - 1] + 1)
                {
                    operations.Add(new EditOperation(EditType.Replace, x - 1, source[x - 1].ToString(), target[y - 1].ToString()));
                    x--; y--;
                }
                else if (y > 0 && (x == 0 || dp[x, y] == dp[x, y - 1] + 1))
                {
                    operations.Add(new EditOperation(EditType.Insert, x, target[y - 1].ToString()));
                    y--;
                }
                else if (x > 0 && (y == 0 || dp[x, y] == dp[x - 1, y] + 1))
                {
                    operations.Add(new EditOperation(EditType.Delete, x - 1, source[x - 1].ToString()));
                    x--;
                }
            }

            operations.Reverse();
            return operations;
        }

        /// <summary>
        /// Damerau-Levenshtein距离（支持相邻交换）
        /// </summary>
        public static int DamerauLevenshteinDistance(string source, string target)
        {
            if (string.IsNullOrEmpty(source))
                return target?.Length ?? 0;
            if (string.IsNullOrEmpty(target))
                return source.Length;

            int m = source.Length;
            int n = target.Length;

            int[,] dp = new int[m + 1, n + 1];

            for (int i = 0; i <= m; i++) dp[i, 0] = i;
            for (int j = 0; j <= n; j++) dp[0, j] = j;

            for (int i = 1; i <= m; i++)
            {
                for (int j = 1; j <= n; j++)
                {
                    int cost = source[i - 1] == target[j - 1] ? 0 : 1;

                    dp[i, j] = Math.Min(
                        Math.Min(dp[i - 1, j] + 1, dp[i, j - 1] + 1),
                        dp[i - 1, j - 1] + cost);

                    // 检查相邻交换
                    if (i > 1 && j > 1 &&
                        source[i - 1] == target[j - 2] &&
                        source[i - 2] == target[j - 1])
                    {
                        dp[i, j] = Math.Min(dp[i, j], dp[i - 2, j - 2] + cost);
                    }
                }
            }

            return dp[m, n];
        }

        /// <summary>
        /// 计算最长公共子序列长度
        /// </summary>
        public static int LongestCommonSubsequence(string source, string target)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(target))
                return 0;

            int m = source.Length;
            int n = target.Length;

            int[,] dp = new int[m + 1, n + 1];

            for (int i = 1; i <= m; i++)
            {
                for (int j = 1; j <= n; j++)
                {
                    if (source[i - 1] == target[j - 1])
                    {
                        dp[i, j] = dp[i - 1, j - 1] + 1;
                    }
                    else
                    {
                        dp[i, j] = Math.Max(dp[i - 1, j], dp[i, j - 1]);
                    }
                }
            }

            return dp[m, n];
        }

        /// <summary>
        /// 基于最长公共子序列的相似度
        /// </summary>
        public static double LCSSimilarity(string source, string target)
        {
            if (string.IsNullOrEmpty(source) && string.IsNullOrEmpty(target))
                return 1.0;
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(target))
                return 0.0;

            int lcs = LongestCommonSubsequence(source, target);
            int maxLen = Math.Max(source.Length, target.Length);

            return (double)lcs / maxLen;
        }

        /// <summary>
        /// 计算 Jaro 相似度
        /// </summary>
        public static double JaroSimilarity(string source, string target)
        {
            if (string.IsNullOrEmpty(source) && string.IsNullOrEmpty(target))
                return 1.0;
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(target))
                return 0.0;
            if (source == target)
                return 1.0;

            int m = source.Length;
            int n = target.Length;

            int matchDistance = Math.Max(m, n) / 2 - 1;
            if (matchDistance < 0) matchDistance = 0;

            bool[] sourceMatches = new bool[m];
            bool[] targetMatches = new bool[n];

            int matches = 0;
            int transpositions = 0;

            for (int i = 0; i < m; i++)
            {
                int start = Math.Max(0, i - matchDistance);
                int end = Math.Min(i + matchDistance + 1, n);

                for (int j = start; j < end; j++)
                {
                    if (targetMatches[j] || source[i] != target[j])
                        continue;

                    sourceMatches[i] = true;
                    targetMatches[j] = true;
                    matches++;
                    break;
                }
            }

            if (matches == 0)
                return 0.0;

            int k = 0;
            for (int i = 0; i < m; i++)
            {
                if (!sourceMatches[i])
                    continue;

                while (!targetMatches[k])
                    k++;

                if (source[i] != target[k])
                    transpositions++;

                k++;
            }

            return ((double)matches / m +
                    (double)matches / n +
                    (matches - transpositions / 2.0) / matches) / 3.0;
        }

        /// <summary>
        /// 计算 Jaro-Winkler 相似度
        /// </summary>
        public static double JaroWinklerSimilarity(string source, string target, double scalingFactor = 0.1)
        {
            double jaro = JaroSimilarity(source, target);

            // 计算公共前缀长度（最多4个字符）
            int prefixLength = 0;
            for (int i = 0; i < Math.Min(Math.Min(source.Length, target.Length), 4); i++)
            {
                if (source[i] == target[i])
                    prefixLength++;
                else
                    break;
            }

            return jaro + prefixLength * scalingFactor * (1 - jaro);
        }

        /// <summary>
        /// 模糊匹配搜索
        /// </summary>
        public static List<(string Item, double Score)> FuzzySearch(string query, IEnumerable<string> items, double threshold = 0.5)
        {
            return items
                .Select(item => (Item: item, Score: JaroWinklerSimilarity(query, item)))
                .Where(x => x.Score >= threshold)
                .OrderByDescending(x => x.Score)
                .ToList();
        }
    }

    /// <summary>
    /// 编辑操作类型
    /// </summary>
    public enum EditType
    {
        /// <summary>匹配</summary>
        Match,
        /// <summary>替换</summary>
        Replace,
        /// <summary>插入</summary>
        Insert,
        /// <summary>删除</summary>
        Delete
    }

    /// <summary>
    /// 编辑操作
    /// </summary>
    public class EditOperation
    {
        /// <summary>操作类型</summary>
        public EditType Type { get; }
        /// <summary>位置</summary>
        public int Position { get; }
        /// <summary>原始字符</summary>
        public string OldValue { get; }
        /// <summary>新字符</summary>
        public string NewValue { get; }

        public EditOperation(EditType type, int position, string value, string newValue = null)
        {
            Type = type;
            Position = position;
            OldValue = value;
            NewValue = newValue ?? value;
        }

        public override string ToString()
        {
            return Type switch
            {
                EditType.Match => $"Match '{OldValue}' at {Position}",
                EditType.Replace => $"Replace '{OldValue}' with '{NewValue}' at {Position}",
                EditType.Insert => $"Insert '{NewValue}' at {Position}",
                EditType.Delete => $"Delete '{OldValue}' at {Position}",
                _ => base.ToString()
            };
        }
    }
}
