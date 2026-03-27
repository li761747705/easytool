using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace EasyTool.SecurityCategory
{
    /// <summary>
    /// SQL注入防护工具类
    /// </summary>
    public static class SqlInjectionUtil
    {
        private static readonly Regex SqlKeywordsPattern = new(
            @"\b(select|insert|update|delete|drop|create|alter|truncate|exec|execute|xp_|sp_|declare|cast|convert|union|join|where|from|into|values|set|order|group|having|limit|offset)\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex SqlCommentPattern = new(
            @"(--)|(/\*)|(\*/)",
            RegexOptions.Compiled);

        private static readonly Regex SqlQuotePattern = new(
            @"('|""|`)",
            RegexOptions.Compiled);

        private static readonly Regex SqlSemicolonPattern = new(
            @";",
            RegexOptions.Compiled);

        private static readonly Regex SqlDangerousPattern = new(
            @"(\b(OR|AND)\s*['""]?\d+['""]?\s*=\s*['""]?\d+)|" + // OR 1=1
            @"(\b(OR|AND)\s*['""][^'""]*['""]\s*=\s*['""][^'""]*['""])|" + // OR 'a'='a'
            @"(UNION\s+(ALL\s+)?SELECT)|" + // UNION SELECT
            @"(EXEC\s+)|" + // EXEC
            @"(Xp_\w+)|" + // xp_cmdshell等
            @"(WAITFOR\s+DELAY)|" + // WAITFOR DELAY
            @"(BENCHMARK\s*\()|" + // MySQL BENCHMARK
            @"(SLEEP\s*\()", // MySQL SLEEP
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly HashSet<string> DangerousKeywords = new(StringComparer.OrdinalIgnoreCase)
        {
            "xp_cmdshell", "xp_regread", "xp_regwrite", "xp_regdelete",
            "sp_executesql", "sp_oacreate", "sp_oamethod",
            "information_schema", "sysobjects", "syscolumns",
            "pg_catalog", "pg_class", "pg_attribute"
        };

        /// <summary>
        /// 检测是否存在SQL注入风险
        /// </summary>
        public static bool HasSqlInjection(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;

            // 检测危险模式
            if (SqlDangerousPattern.IsMatch(input))
                return true;

            // 检测注释
            if (SqlCommentPattern.IsMatch(input))
                return true;

            // 检测危险关键字组合
            var upperInput = input.ToUpperInvariant();
            foreach (var keyword in DangerousKeywords)
            {
                if (upperInput.Contains(keyword.ToUpperInvariant()))
                    return true;
            }

            // 检测单引号后面跟SQL关键字
            if (Regex.IsMatch(input, @"'\s*(OR|AND|UNION|SELECT|INSERT|UPDATE|DELETE)", RegexOptions.IgnoreCase))
                return true;

            return false;
        }

        /// <summary>
        /// 转义SQL字符串参数
        /// </summary>
        public static string EscapeString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var result = new StringBuilder(input.Length);
            foreach (var c in input)
            {
                switch (c)
                {
                    case '\'':
                        result.Append("''");
                        break;
                    case '\\':
                        result.Append("\\\\");
                        break;
                    case '\0':
                        result.Append("\\0");
                        break;
                    case '\n':
                        result.Append("\\n");
                        break;
                    case '\r':
                        result.Append("\\r");
                        break;
                    case '\x1a':
                        result.Append("\\Z");
                        break;
                    default:
                        result.Append(c);
                        break;
                }
            }
            return result.ToString();
        }

        /// <summary>
        /// 移除潜在的SQL注入字符
        /// </summary>
        public static string Sanitize(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var result = input;

            // 移除注释
            result = SqlCommentPattern.Replace(result, "");

            // 移除分号（防止多语句执行）
            result = SqlSemicolonPattern.Replace(result, "");

            // 转义引号
            result = SqlQuotePattern.Replace(result, m => m.Value == "'" ? "''" : "\\" + m.Value);

            return result;
        }

        /// <summary>
        /// 过滤SQL关键字
        /// </summary>
        public static string FilterKeywords(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return SqlKeywordsPattern.Replace(input, "");
        }

        /// <summary>
        /// 验证标识符（表名、列名等）
        /// </summary>
        public static bool IsValidIdentifier(string identifier)
        {
            if (string.IsNullOrEmpty(identifier))
                return false;

            // 只允许字母、数字、下划线
            if (!Regex.IsMatch(identifier, @"^[a-zA-Z_][a-zA-Z0-9_]*$"))
                return false;

            // 不能是SQL关键字
            if (SqlKeywordsPattern.IsMatch(identifier))
                return false;

            return true;
        }

        /// <summary>
        /// 安全的标识符包装
        /// </summary>
        public static string QuoteIdentifier(string identifier, string quoteChar = "`")
        {
            if (string.IsNullOrEmpty(identifier))
                return identifier;

            // 转义内部的引号
            identifier = identifier.Replace(quoteChar, quoteChar + quoteChar);
            return $"{quoteChar}{identifier}{quoteChar}";
        }

        /// <summary>
        /// 构建安全的IN子句参数
        /// </summary>
        public static string BuildInClause(IEnumerable<string> values, bool numeric = false)
        {
            var items = new List<string>();
            foreach (var value in values)
            {
                if (numeric && int.TryParse(value, out _))
                {
                    items.Add(value);
                }
                else
                {
                    items.Add($"'{EscapeString(value)}'");
                }
            }
            return string.Join(", ", items);
        }

        /// <summary>
        /// 构建安全的LIKE子句
        /// </summary>
        public static string EscapeLikePattern(string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
                return pattern;

            var result = new StringBuilder();
            foreach (var c in pattern)
            {
                switch (c)
                {
                    case '%':
                    case '_':
                    case '[':
                    case ']':
                        result.Append('\\');
                        break;
                }
                result.Append(c);
            }
            return result.ToString();
        }

        /// <summary>
        /// 检测批量SQL注入
        /// </summary>
        public static Dictionary<string, bool> CheckMultiple(IEnumerable<KeyValuePair<string, string>> inputs)
        {
            var results = new Dictionary<string, bool>();
            foreach (var kvp in inputs)
            {
                results[kvp.Key] = HasSqlInjection(kvp.Value);
            }
            return results;
        }

        /// <summary>
        /// 获取SQL注入风险详情
        /// </summary>
        public static SqlInjectionAnalysis Analyze(string input)
        {
            var analysis = new SqlInjectionAnalysis
            {
                Input = input,
                HasRisk = false
            };

            if (string.IsNullOrEmpty(input))
                return analysis;

            // 检测各种风险
            if (SqlKeywordsPattern.IsMatch(input))
            {
                analysis.HasRisk = true;
                analysis.Risks.Add("包含SQL关键字");
            }

            if (SqlCommentPattern.IsMatch(input))
            {
                analysis.HasRisk = true;
                analysis.Risks.Add("包含SQL注释");
            }

            if (SqlDangerousPattern.IsMatch(input))
            {
                analysis.HasRisk = true;
                analysis.Risks.Add("包含危险SQL模式");
            }

            foreach (var keyword in DangerousKeywords)
            {
                if (input.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                {
                    analysis.HasRisk = true;
                    analysis.DetectedKeywords.Add(keyword);
                }
            }

            return analysis;
        }
    }

    /// <summary>
    /// SQL注入分析结果
    /// </summary>
    public class SqlInjectionAnalysis
    {
        /// <summary>
        /// 输入字符串
        /// </summary>
        public string Input { get; set; } = "";

        /// <summary>
        /// 是否有风险
        /// </summary>
        public bool HasRisk { get; set; }

        /// <summary>
        /// 检测到的风险列表
        /// </summary>
        public List<string> Risks { get; } = new();

        /// <summary>
        /// 检测到的危险关键字
        /// </summary>
        public List<string> DetectedKeywords { get; } = new();
    }
}
