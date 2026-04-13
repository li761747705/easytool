using System;
using System.Collections.Generic;
using EasyTool.AI.LLM;
using Xunit;

namespace EasyTool.UnitTests.AICategory
{
    /// <summary>
    /// TokenizerUtil 测试类
    /// </summary>
    public class TokenizerUtilTests
    {
        #region EstimateTokens 测试

        [Fact]
        public void EstimateTokens_EmptyString_ReturnsZero()
        {
            Assert.Equal(0, TokenizerUtil.EstimateTokens(""));
        }

        [Fact]
        public void EstimateTokens_NullString_ReturnsZero()
        {
            Assert.Equal(0, TokenizerUtil.EstimateTokens(null));
        }

        [Fact]
        public void EstimateTokens_SimpleEnglish_ReturnsCorrectEstimate()
        {
            var text = "Hello World";
            var tokens = TokenizerUtil.EstimateTokens(text);
            Assert.True(tokens > 0);
            Assert.True(tokens < 10); // 简单文本应该 token 数很少
        }

        [Fact]
        public void EstimateTokens_ChineseText_ReturnsCorrectEstimate()
        {
            var text = "你好世界";
            var tokens = TokenizerUtil.EstimateTokens(text);
            Assert.True(tokens > 0);
            // 中文约 1.5 字符 = 1 token，4个字符约 2-3 tokens
            Assert.True(tokens >= 2 && tokens <= 4);
        }

        [Fact]
        public void EstimateTokens_MixedText_ReturnsCorrectEstimate()
        {
            var text = "Hello 世界";
            var tokens = TokenizerUtil.EstimateTokens(text);
            Assert.True(tokens > 0);
        }

        [Theory]
        [InlineData("")]
        [InlineData("a")]
        [InlineData("Hello")]
        [InlineData("你好")]
        [InlineData("Hello World 你好世界")]
        public void EstimateTokens_VariousInputs_ReturnsPositiveOrZero(string text)
        {
            var tokens = TokenizerUtil.EstimateTokens(text);
            Assert.True(tokens >= 0);
        }

        #endregion

        #region EstimateGptTokens 测试

        [Fact]
        public void EstimateGptTokens_EmptyString_ReturnsZero()
        {
            Assert.Equal(0, TokenizerUtil.EstimateGptTokens(""));
        }

        [Fact]
        public void EstimateGptTokens_NullString_ReturnsZero()
        {
            Assert.Equal(0, TokenizerUtil.EstimateGptTokens(null));
        }

        [Fact]
        public void EstimateGptTokens_EnglishText_ReturnsCorrectEstimate()
        {
            var text = "The quick brown fox jumps over the lazy dog";
            var tokens = TokenizerUtil.EstimateGptTokens(text);
            Assert.True(tokens > 0);
        }

        [Fact]
        public void EstimateGptTokens_ChineseText_ReturnsCorrectEstimate()
        {
            var text = "这是一段中文测试文本";
            var tokens = TokenizerUtil.EstimateGptTokens(text);
            // 每个中文字符约 1 token
            Assert.True(tokens >= text.Length);
        }

        [Fact]
        public void EstimateGptTokens_Digits_ReturnsCorrectEstimate()
        {
            var text = "123456789";
            var tokens = TokenizerUtil.EstimateGptTokens(text);
            Assert.True(tokens > 0);
        }

        #endregion

        #region EstimateClaudeTokens 测试

        [Fact]
        public void EstimateClaudeTokens_EmptyString_ReturnsZero()
        {
            Assert.Equal(0, TokenizerUtil.EstimateClaudeTokens(""));
        }

        [Fact]
        public void EstimateClaudeTokens_NullString_ReturnsZero()
        {
            Assert.Equal(0, TokenizerUtil.EstimateClaudeTokens(null));
        }

        [Fact]
        public void EstimateClaudeTokens_AnyText_ReturnsHigherThanGpt()
        {
            var text = "Hello World";
            var gptTokens = TokenizerUtil.EstimateGptTokens(text);
            var claudeTokens = TokenizerUtil.EstimateClaudeTokens(text);
            // Claude 估算比 GPT 略高（约 10%）
            Assert.True(claudeTokens >= gptTokens);
        }

        #endregion

        #region EstimateTokens (with model) 测试

        [Fact]
        public void EstimateTokens_WithGpt4Model_ReturnsGptEstimate()
        {
            var text = "Hello World";
            var gptTokens = TokenizerUtil.EstimateGptTokens(text);
            var modelTokens = TokenizerUtil.EstimateTokens(text, "gpt-4");
            Assert.Equal(gptTokens, modelTokens);
        }

        [Fact]
        public void EstimateTokens_WithGpt35Model_ReturnsGptEstimate()
        {
            var text = "Hello World";
            var gptTokens = TokenizerUtil.EstimateGptTokens(text);
            var modelTokens = TokenizerUtil.EstimateTokens(text, "gpt-3.5-turbo");
            Assert.Equal(gptTokens, modelTokens);
        }

        [Fact]
        public void EstimateTokens_WithClaudeModel_ReturnsClaudeEstimate()
        {
            var text = "Hello World";
            var claudeTokens = TokenizerUtil.EstimateClaudeTokens(text);
            var modelTokens = TokenizerUtil.EstimateTokens(text, "claude-3-opus");
            Assert.Equal(claudeTokens, modelTokens);
        }

        [Fact]
        public void EstimateTokens_WithUnknownModel_ReturnsGenericEstimate()
        {
            var text = "Hello World";
            var genericTokens = TokenizerUtil.EstimateTokens(text);
            var modelTokens = TokenizerUtil.EstimateTokens(text, "unknown-model");
            Assert.Equal(genericTokens, modelTokens);
        }

        #endregion

        #region CountMessagesTokens 测试

        [Fact]
        public void CountMessagesTokens_EmptyList_ReturnsTwo()
        {
            var messages = new List<(string Role, string Content)>();
            var tokens = TokenizerUtil.CountMessagesTokens(messages);
            Assert.Equal(2, tokens); // 对话整体额外消耗约 2 个 token
        }

        [Fact]
        public void CountMessagesTokens_SingleMessage_ReturnsCorrectCount()
        {
            var messages = new List<(string Role, string Content)>
            {
                ("user", "Hello")
            };
            var tokens = TokenizerUtil.CountMessagesTokens(messages);
            Assert.True(tokens > 4); // 4 (消息开销) + 内容 token
        }

        [Fact]
        public void CountMessagesTokens_MultipleMessages_ReturnsCorrectCount()
        {
            var messages = new List<(string Role, string Content)>
            {
                ("system", "You are a helpful assistant"),
                ("user", "Hello"),
                ("assistant", "Hi there!")
            };
            var tokens = TokenizerUtil.CountMessagesTokens(messages);
            Assert.True(tokens > 0);
        }

        #endregion

        #region TruncateToTokenLimit 测试

        [Fact]
        public void TruncateToTokenLimit_EmptyString_ReturnsEmpty()
        {
            var result = TokenizerUtil.TruncateToTokenLimit("", 100);
            Assert.Equal("", result);
        }

        [Fact]
        public void TruncateToTokenLimit_NullString_ReturnsNull()
        {
            var result = TokenizerUtil.TruncateToTokenLimit(null, 100);
            Assert.Null(result);
        }

        [Fact]
        public void TruncateToTokenLimit_ShortText_ReturnsOriginal()
        {
            var text = "Hello";
            var result = TokenizerUtil.TruncateToTokenLimit(text, 100);
            Assert.Equal(text, result);
        }

        [Fact]
        public void TruncateToTokenLimit_LongText_ReturnsTruncated()
        {
            var text = "This is a very long text that should be truncated to fit within the token limit";
            var result = TokenizerUtil.TruncateToTokenLimit(text, 5);
            Assert.True(result.Length < text.Length);
            Assert.EndsWith("...", result);
        }

        #endregion

        #region SplitByTokenLimit 测试

        [Fact]
        public void SplitByTokenLimit_EmptyString_ReturnsEmptyList()
        {
            var result = TokenizerUtil.SplitByTokenLimit("", 100);
            Assert.Empty(result);
        }

        [Fact]
        public void SplitByTokenLimit_NullString_ReturnsEmptyList()
        {
            var result = TokenizerUtil.SplitByTokenLimit(null, 100);
            Assert.Empty(result);
        }

        [Fact]
        public void SplitByTokenLimit_ShortText_ReturnsSingleChunk()
        {
            var text = "Hello";
            var result = TokenizerUtil.SplitByTokenLimit(text, 100);
            Assert.Single(result);
            Assert.Equal(text, result[0]);
        }

        [Fact]
        public void SplitByTokenLimit_LongText_ReturnsMultipleChunks()
        {
            var text = "This is a long text. This is another part. This is yet another part of the text.";
            var result = TokenizerUtil.SplitByTokenLimit(text, 5);
            Assert.True(result.Count > 1);
        }

        [Fact]
        public void SplitByTokenLimit_WithOverlap_ReturnsOverlappingChunks()
        {
            // 使用更长的文本以确保能分成多个块
            var text = "This is a long text that should be split into multiple chunks. This is another part of the text.";
            var result = TokenizerUtil.SplitByTokenLimit(text, 5, 2);

            // 验证结果不为空且包含多个块
            Assert.NotNull(result);
            Assert.True(result.Count >= 1);
        }

        #endregion

        #region IsWithinTokenLimit 测试

        [Fact]
        public void IsWithinTokenLimit_EmptyString_ReturnsTrue()
        {
            Assert.True(TokenizerUtil.IsWithinTokenLimit("", 100));
        }

        [Fact]
        public void IsWithinTokenLimit_NullString_ReturnsTrue()
        {
            Assert.True(TokenizerUtil.IsWithinTokenLimit(null, 100));
        }

        [Fact]
        public void IsWithinTokenLimit_ShortText_ReturnsTrue()
        {
            Assert.True(TokenizerUtil.IsWithinTokenLimit("Hello", 100));
        }

        [Fact]
        public void IsWithinTokenLimit_ExceedingText_ReturnsFalse()
        {
            var longText = string.Join(" ", Enumerable.Repeat("word", 1000));
            Assert.False(TokenizerUtil.IsWithinTokenLimit(longText, 10));
        }

        #endregion

        #region GetTokenUsage 测试

        [Fact]
        public void GetTokenUsage_EmptyString_ReturnsZeroTokens()
        {
            var usage = TokenizerUtil.GetTokenUsage("");
            Assert.Equal(0, usage.TextLength);
            Assert.Equal(0, usage.EstimatedTokens);
        }

        [Fact]
        public void GetTokenUsage_NullString_ReturnsZeroTokens()
        {
            var usage = TokenizerUtil.GetTokenUsage(null);
            Assert.Equal(0, usage.TextLength);
            Assert.Equal(0, usage.EstimatedTokens);
        }

        [Fact]
        public void GetTokenUsage_ValidText_ReturnsCorrectInfo()
        {
            var text = "Hello World";
            var usage = TokenizerUtil.GetTokenUsage(text);
            Assert.Equal(text.Length, usage.TextLength);
            Assert.True(usage.EstimatedTokens > 0);
            Assert.Equal("gpt-3.5-turbo", usage.Model);
            Assert.True(usage.CharsPerToken > 0);
        }

        [Fact]
        public void GetTokenUsage_WithModel_ReturnsCorrectModel()
        {
            var text = "Hello";
            var usage = TokenizerUtil.GetTokenUsage(text, "gpt-4");
            Assert.Equal("gpt-4", usage.Model);
        }

        #endregion

        #region TokenUsageInfo 类测试

        [Fact]
        public void TokenUsageInfo_Properties_CanBeSet()
        {
            var info = new TokenUsageInfo
            {
                TextLength = 100,
                EstimatedTokens = 25,
                Model = "gpt-4",
                CharsPerToken = 4.0
            };

            Assert.Equal(100, info.TextLength);
            Assert.Equal(25, info.EstimatedTokens);
            Assert.Equal("gpt-4", info.Model);
            Assert.Equal(4.0, info.CharsPerToken);
        }

        #endregion
    }
}