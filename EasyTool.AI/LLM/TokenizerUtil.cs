using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace EasyTool.AI.LLM
{
    /// <summary>
    /// Token 计数工具
    /// 提供 GPT 系列模型的 Token 估算功能
    /// </summary>
    public static class TokenizerUtil
    {
        // GPT 系列模型的 Token 估算规则
        // 平均约 4 个字符 = 1 个 token（英文）
        // 中文约 1.5-2 个字符 = 1 个 token

        private static readonly Regex _wordPattern = new Regex(@"\b\w+\b", RegexOptions.Compiled);
        private static readonly Regex _chinesePattern = new Regex(@"[\u4e00-\u9fff]", RegexOptions.Compiled);
        private static readonly Regex _punctuationPattern = new Regex(@"[^\w\s]", RegexOptions.Compiled);
        private static readonly Regex _whitespacePattern = new Regex(@"\s+", RegexOptions.Compiled);

        /// <summary>
        /// 估算文本的 Token 数量（通用估算）
        /// </summary>
        /// <param name="text">输入文本</param>
        /// <returns>估算的 Token 数量</returns>
        public static int EstimateTokens(string text)
        {
            if (string.IsNullOrEmpty(text))
                return 0;

            int tokens = 0;

            // 统计中文字符
            var chineseMatches = _chinesePattern.Matches(text);
            tokens += (int)Math.Ceiling(chineseMatches.Count / 1.5); // 中文约 1.5 字符 = 1 token

            // 统计英文单词
            var wordMatches = _wordPattern.Matches(text);
            foreach (Match match in wordMatches)
            {
                // 检查是否是中文单词（已计算过）
                if (!_chinesePattern.IsMatch(match.Value))
                {
                    // 英文单词：短词通常 1 token，长词可能拆分
                    if (match.Value.Length <= 4)
                        tokens += 1;
                    else
                        tokens += (int)Math.Ceiling(match.Value.Length / 4.0);
                }
            }

            // 统计标点符号
            var punctMatches = _punctuationPattern.Matches(text);
            tokens += punctMatches.Count;

            // 统计空白字符组
            var whitespaceMatches = _whitespacePattern.Matches(text);
            tokens += (int)Math.Ceiling(whitespaceMatches.Count / 2.0);

            return Math.Max(1, tokens);
        }

        /// <summary>
        /// 估算文本的 Token 数量（指定模型）
        /// </summary>
        /// <param name="text">输入文本</param>
        /// <param name="model">模型名称</param>
        /// <returns>估算的 Token 数量</returns>
        public static int EstimateTokens(string text, string model)
        {
            if (string.IsNullOrEmpty(text))
                return 0;

            var modelLower = model.ToLowerInvariant();

            // GPT-4 和 GPT-3.5 使用相同的 tokenizer
            if (modelLower.Contains("gpt-4") || modelLower.Contains("gpt-3.5"))
            {
                return EstimateGptTokens(text);
            }

            // Claude 使用不同的估算
            if (modelLower.Contains("claude"))
            {
                return EstimateClaudeTokens(text);
            }

            // 默认通用估算
            return EstimateTokens(text);
        }

        /// <summary>
        /// GPT 系列 Token 估算
        /// </summary>
        public static int EstimateGptTokens(string text)
        {
            if (string.IsNullOrEmpty(text))
                return 0;

            int tokens = 0;
            var chars = text.ToCharArray();

            for (int i = 0; i < chars.Length; i++)
            {
                char c = chars[i];

                // 中文字符
                if (c >= 0x4E00 && c <= 0x9FFF)
                {
                    tokens += 1;
                }
                // 日文假名
                else if ((c >= 0x3040 && c <= 0x309F) || (c >= 0x30A0 && c <= 0x30FF))
                {
                    tokens += 1;
                }
                // 韩文
                else if (c >= 0xAC00 && c <= 0xD7A3)
                {
                    tokens += 1;
                }
                // 空格
                else if (char.IsWhiteSpace(c))
                {
                    // 连续空格合并计算
                    if (i == 0 || !char.IsWhiteSpace(chars[i - 1]))
                        tokens += 1;
                }
                // 标点符号
                else if (char.IsPunctuation(c))
                {
                    tokens += 1;
                }
                // 数字
                else if (char.IsDigit(c))
                {
                    // 连续数字约 3 位 = 1 token
                    int digitCount = 0;
                    while (i + digitCount < chars.Length && char.IsDigit(chars[i + digitCount]))
                        digitCount++;
                    tokens += (int)Math.Ceiling(digitCount / 3.0);
                    i += digitCount - 1;
                }
                // 英文字母
                else if (char.IsLetter(c))
                {
                    // 统计连续字母
                    int letterCount = 0;
                    while (i + letterCount < chars.Length && char.IsLetter(chars[i + letterCount]))
                        letterCount++;
                    // 英文单词约 4 字符 = 1 token
                    tokens += (int)Math.Ceiling(letterCount / 4.0);
                    i += letterCount - 1;
                }
                else
                {
                    tokens += 1;
                }
            }

            return Math.Max(1, tokens);
        }

        /// <summary>
        /// Claude 系列 Token 估算
        /// </summary>
        public static int EstimateClaudeTokens(string text)
        {
            if (string.IsNullOrEmpty(text))
                return 0;

            // Claude 的 tokenizer 与 GPT 略有不同
            // 使用更保守的估算
            var gptEstimate = EstimateGptTokens(text);
            return (int)(gptEstimate * 1.1); // 增加 10% 缓冲
        }

        /// <summary>
        /// 计算消息列表的 Token 数量
        /// </summary>
        /// <param name="messages">消息列表</param>
        /// <param name="model">模型名称</param>
        /// <returns>总 Token 数量</returns>
        public static int CountMessagesTokens(List<(string Role, string Content)> messages, string model = "gpt-3.5-turbo")
        {
            int totalTokens = 0;

            foreach (var message in messages)
            {
                // 每条消息额外消耗约 4 个 token（角色标记等）
                totalTokens += 4;
                totalTokens += EstimateTokens(message.Role, model);
                totalTokens += EstimateTokens(message.Content, model);
            }

            // 对话整体额外消耗约 2 个 token
            totalTokens += 2;

            return totalTokens;
        }

        /// <summary>
        /// 截断文本以适应 Token 限制
        /// </summary>
        /// <param name="text">原始文本</param>
        /// <param name="maxTokens">最大 Token 数</param>
        /// <param name="model">模型名称</param>
        /// <returns>截断后的文本</returns>
        public static string TruncateToTokenLimit(string text, int maxTokens, string model = "gpt-3.5-turbo")
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var currentTokens = EstimateTokens(text, model);
            if (currentTokens <= maxTokens)
                return text;

            // 估算每个 token 平均字符数
            var avgCharsPerToken = (double)text.Length / currentTokens;
            var targetLength = (int)(maxTokens * avgCharsPerToken * 0.9); // 保留 10% 缓冲

            if (targetLength >= text.Length)
                return text;

            return text.Substring(0, targetLength) + "...";
        }

        /// <summary>
        /// 分割文本为多个 Token 限制内的块
        /// </summary>
        /// <param name="text">原始文本</param>
        /// <param name="maxTokensPerChunk">每块最大 Token 数</param>
        /// <param name="overlap">块之间的重叠 Token 数</param>
        /// <param name="model">模型名称</param>
        /// <returns>文本块列表</returns>
        public static List<string> SplitByTokenLimit(string text, int maxTokensPerChunk, int overlap = 0, string model = "gpt-3.5-turbo")
        {
            var result = new List<string>();

            if (string.IsNullOrEmpty(text))
                return result;

            var totalTokens = EstimateTokens(text, model);
            if (totalTokens <= maxTokensPerChunk)
            {
                result.Add(text);
                return result;
            }

            var avgCharsPerToken = (double)text.Length / totalTokens;
            var chunkSize = (int)(maxTokensPerChunk * avgCharsPerToken * 0.9);
            var overlapSize = (int)(overlap * avgCharsPerToken);

            int position = 0;
            while (position < text.Length)
            {
                var length = Math.Min(chunkSize, text.Length - position);
                var chunk = text.Substring(position, length);
                result.Add(chunk);

                position += chunkSize - overlapSize;
                if (overlapSize > 0 && position < text.Length)
                {
                    position = Math.Max(0, position - overlapSize);
                }
            }

            return result;
        }

        /// <summary>
        /// 检查文本是否在 Token 限制内
        /// </summary>
        /// <param name="text">文本</param>
        /// <param name="maxTokens">最大 Token 数</param>
        /// <param name="model">模型名称</param>
        /// <returns>是否在限制内</returns>
        public static bool IsWithinTokenLimit(string text, int maxTokens, string model = "gpt-3.5-turbo")
        {
            return EstimateTokens(text, model) <= maxTokens;
        }

        /// <summary>
        /// 获取文本的 Token 使用情况
        /// </summary>
        /// <param name="text">文本</param>
        /// <param name="model">模型名称</param>
        /// <returns>Token 使用信息</returns>
        public static TokenUsageInfo GetTokenUsage(string text, string model = "gpt-3.5-turbo")
        {
            var tokens = EstimateTokens(text, model);
            var chars = text?.Length ?? 0;

            return new TokenUsageInfo
            {
                TextLength = chars,
                EstimatedTokens = tokens,
                Model = model,
                CharsPerToken = tokens > 0 ? (double)chars / tokens : 0
            };
        }
    }

    /// <summary>
    /// Token 使用信息
    /// </summary>
    public class TokenUsageInfo
    {
        /// <summary>
        /// 文本长度（字符数）
        /// </summary>
        public int TextLength { get; set; }

        /// <summary>
        /// 估算的 Token 数
        /// </summary>
        public int EstimatedTokens { get; set; }

        /// <summary>
        /// 模型名称
        /// </summary>
        public string? Model { get; set; }

        /// <summary>
        /// 每个 Token 平均字符数
        /// </summary>
        public double CharsPerToken { get; set; }
    }
}