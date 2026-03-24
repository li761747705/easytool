using System;
using System.Collections.Generic;

namespace EasyTool.TextCategory
{
    /// <summary>
    /// 文本差异比较工具类
    /// 提供文本差异计算和显示功能
    /// </summary>
    public static class DiffUtil
    {
        /// <summary>
        /// 比较两个文本的差异
        /// </summary>
        public static List<DiffItem> Compare(string oldText, string newText, bool ignoreCase = false, bool ignoreWhitespace = false)
        {
            if (ignoreWhitespace)
            {
                oldText = System.Text.RegularExpressions.Regex.Replace(oldText ?? "", @"\s+", " ").Trim();
                newText = System.Text.RegularExpressions.Regex.Replace(newText ?? "", @"\s+", " ").Trim();
            }

            var oldLines = (oldText ?? "").Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
            var newLines = (newText ?? "").Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);

            return CompareLines(oldLines, newLines, ignoreCase);
        }

        /// <summary>
        /// 比较两个行数组的差异
        /// </summary>
        public static List<DiffItem> CompareLines(string[] oldLines, string[] newLines, bool ignoreCase = false)
        {
            var diffs = new List<DiffItem>();

            // 使用LCS算法
            var lcs = ComputeLCS(oldLines, newLines, ignoreCase);

            int oldIndex = 0, newIndex = 0, lcsIndex = 0;

            while (oldIndex < oldLines.Length || newIndex < newLines.Length)
            {
                if (lcsIndex < lcs.Count)
                {
                    var lcsItem = lcs[lcsIndex];

                    // 处理删除
                    while (oldIndex < lcsItem.OldIndex)
                    {
                        diffs.Add(new DiffItem(DiffType.Deleted, oldLines[oldIndex], oldIndex, -1));
                        oldIndex++;
                    }

                    // 处理新增
                    while (newIndex < lcsItem.NewIndex)
                    {
                        diffs.Add(new DiffItem(DiffType.Added, newLines[newIndex], -1, newIndex));
                        newIndex++;
                    }

                    // 相同行
                    diffs.Add(new DiffItem(DiffType.Unchanged, oldLines[oldIndex], oldIndex, newIndex));
                    oldIndex++;
                    newIndex++;
                    lcsIndex++;
                }
                else
                {
                    // 处理剩余
                    while (oldIndex < oldLines.Length)
                    {
                        diffs.Add(new DiffItem(DiffType.Deleted, oldLines[oldIndex], oldIndex, -1));
                        oldIndex++;
                    }

                    while (newIndex < newLines.Length)
                    {
                        diffs.Add(new DiffItem(DiffType.Added, newLines[newIndex], -1, newIndex));
                        newIndex++;
                    }
                }
            }

            return diffs;
        }

        private static List<LCSItem> ComputeLCS(string[] oldLines, string[] newLines, bool ignoreCase)
        {
            int m = oldLines.Length;
            int n = newLines.Length;

            int[,] dp = new int[m + 1, n + 1];

            StringComparison comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            for (int i = 1; i <= m; i++)
            {
                for (int j = 1; j <= n; j++)
                {
                    if (string.Equals(oldLines[i - 1], newLines[j - 1], comparison))
                    {
                        dp[i, j] = dp[i - 1, j - 1] + 1;
                    }
                    else
                    {
                        dp[i, j] = Math.Max(dp[i - 1, j], dp[i, j - 1]);
                    }
                }
            }

            // 回溯
            var result = new List<LCSItem>();
            int x = m, y = n;

            while (x > 0 && y > 0)
            {
                if (string.Equals(oldLines[x - 1], newLines[y - 1], comparison))
                {
                    result.Add(new LCSItem(x - 1, y - 1));
                    x--; y--;
                }
                else if (dp[x - 1, y] > dp[x, y - 1])
                {
                    x--;
                }
                else
                {
                    y--;
                }
            }

            result.Reverse();
            return result;
        }

        /// <summary>
        /// 生成统一格式的差异
        /// </summary>
        public static string ToUnifiedDiff(List<DiffItem> diffs, string oldFile = "a/file", string newFile = "b/file", int contextLines = 3)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"--- {oldFile}");
            sb.AppendLine($"+++ {newFile}");

            int i = 0;
            while (i < diffs.Count)
            {
                // 找到变化块
                if (diffs[i].Type != DiffType.Unchanged)
                {
                    // 计算块的上下文
                    int start = Math.Max(0, i - contextLines);
                    int end = i;

                    // 找到块的结束
                    while (end < diffs.Count && diffs[end].Type != DiffType.Unchanged)
                        end++;

                    end = Math.Min(diffs.Count, end + contextLines);

                    // 计算行号范围
                    int oldStart = -1, oldCount = 0;
                    int newStart = -1, newCount = 0;

                    for (int j = start; j < end; j++)
                    {
                        if (diffs[j].OldLineNumber >= 0)
                        {
                            if (oldStart < 0) oldStart = diffs[j].OldLineNumber;
                            oldCount++;
                        }
                        if (diffs[j].NewLineNumber >= 0)
                        {
                            if (newStart < 0) newStart = diffs[j].NewLineNumber;
                            newCount++;
                        }
                    }

                    if (oldStart < 0) oldStart = 0;
                    if (newStart < 0) newStart = 0;

                    sb.AppendLine($"@@ -{oldStart + 1},{oldCount} +{newStart + 1},{newCount} @@");

                    for (int j = start; j < end; j++)
                    {
                        string prefix = diffs[j].Type switch
                        {
                            DiffType.Added => "+",
                            DiffType.Deleted => "-",
                            _ => " "
                        };
                        sb.AppendLine(prefix + diffs[j].Content);
                    }

                    i = end;
                }
                else
                {
                    i++;
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// 应用差异补丁
        /// </summary>
        public static string ApplyPatch(string original, List<DiffItem> diffs)
        {
            var lines = (original ?? "").Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
            var result = new List<string>();
            int lineIndex = 0;

            foreach (var diff in diffs)
            {
                switch (diff.Type)
                {
                    case DiffType.Unchanged:
                        if (lineIndex < lines.Length)
                        {
                            result.Add(lines[lineIndex]);
                            lineIndex++;
                        }
                        break;

                    case DiffType.Deleted:
                        lineIndex++; // 跳过旧行
                        break;

                    case DiffType.Added:
                        result.Add(diff.Content);
                        break;
                }
            }

            // 添加剩余行
            while (lineIndex < lines.Length)
            {
                result.Add(lines[lineIndex]);
                lineIndex++;
            }

            return string.Join(Environment.NewLine, result);
        }

        /// <summary>
        /// 计算差异统计
        /// </summary>
        public static DiffStats GetStats(List<DiffItem> diffs)
        {
            int added = 0, deleted = 0, unchanged = 0;

            foreach (var diff in diffs)
            {
                switch (diff.Type)
                {
                    case DiffType.Added: added++; break;
                    case DiffType.Deleted: deleted++; break;
                    case DiffType.Unchanged: unchanged++; break;
                }
            }

            return new DiffStats(added, deleted, unchanged);
        }
    }

    /// <summary>
    /// 差异类型
    /// </summary>
    public enum DiffType
    {
        /// <summary>未变化</summary>
        Unchanged,
        /// <summary>新增</summary>
        Added,
        /// <summary>删除</summary>
        Deleted
    }

    /// <summary>
    /// 差异项
    /// </summary>
    public class DiffItem
    {
        /// <summary>差异类型</summary>
        public DiffType Type { get; }
        /// <summary>内容</summary>
        public string Content { get; }
        /// <summary>旧文件行号（-1表示不存在）</summary>
        public int OldLineNumber { get; }
        /// <summary>新文件行号（-1表示不存在）</summary>
        public int NewLineNumber { get; }

        public DiffItem(DiffType type, string content, int oldLineNumber, int newLineNumber)
        {
            Type = type;
            Content = content;
            OldLineNumber = oldLineNumber;
            NewLineNumber = newLineNumber;
        }

        public override string ToString()
        {
            string symbol = Type switch
            {
                DiffType.Added => "+",
                DiffType.Deleted => "-",
                _ => " "
            };
            return $"{symbol} {Content}";
        }
    }

    /// <summary>
    /// 差异统计
    /// </summary>
    public class DiffStats
    {
        /// <summary>新增行数</summary>
        public int AddedLines { get; }
        /// <summary>删除行数</summary>
        public int DeletedLines { get; }
        /// <summary>未变化行数</summary>
        public int UnchangedLines { get; }
        /// <summary>总变化行数</summary>
        public int TotalChanges => AddedLines + DeletedLines;

        public DiffStats(int addedLines, int deletedLines, int unchangedLines)
        {
            AddedLines = addedLines;
            DeletedLines = deletedLines;
            UnchangedLines = unchangedLines;
        }

        public override string ToString()
        {
            return $"+{AddedLines} -{DeletedLines} ={UnchangedLines}";
        }
    }

    internal class LCSItem
    {
        public int OldIndex { get; }
        public int NewIndex { get; }

        public LCSItem(int oldIndex, int newIndex)
        {
            OldIndex = oldIndex;
            NewIndex = newIndex;
        }
    }
}
