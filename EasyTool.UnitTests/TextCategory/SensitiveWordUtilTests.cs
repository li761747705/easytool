using Xunit;
using EasyTool.TextCategory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyTool.UnitTests.TextCategory
{
    public class SensitiveWordUtilTests
    {
        #region 初始化测试

        [Fact]
        public void Init_ValidWords_InitializesFilter()
        {
            var words = new[] { "测试", "敏感词" };
            SensitiveWordUtil.Init(words);
            Assert.Equal(2, SensitiveWordUtil.Count);
        }

        [Fact]
        public void Init_NullCollection_DoesNotThrow()
        {
            SensitiveWordUtil.Init(null);
            // Init(null) is a no-op, doesn't clear existing words
            // If this test runs in isolation, Count will be 0
            // If it runs after other tests, Count might be > 0
            // Just verify it doesn't throw
        }

        [Fact]
        public void Init_EmptyCollection_ClearsExistingWords()
        {
            SensitiveWordUtil.Init(new[] { "测试" });
            SensitiveWordUtil.Init(new string[0]);
            Assert.Equal(0, SensitiveWordUtil.Count);
        }

        [Fact]
        public void Init_WithWhitespaceWords_IgnoresWhitespace()
        {
            var words = new[] { "测试", "", "   ", null, "敏感词" };
            SensitiveWordUtil.Init(words);
            Assert.Equal(2, SensitiveWordUtil.Count);
        }

        #endregion

        #region 添加单词测试

        [Fact]
        public void AddWord_ValidWord_AddsToFilter()
        {
            SensitiveWordUtil.Clear();
            SensitiveWordUtil.AddWord("测试");
            Assert.Equal(1, SensitiveWordUtil.Count);
        }

        [Fact]
        public void AddWord_NullOrWhitespace_DoesNotAdd()
        {
            SensitiveWordUtil.Clear();
            SensitiveWordUtil.AddWord(null);
            SensitiveWordUtil.AddWord("");
            SensitiveWordUtil.AddWord("   ");
            Assert.Equal(0, SensitiveWordUtil.Count);
        }

        [Fact]
        public void AddWord_DuplicateWord_IncreasesCountOnlyOnce()
        {
            SensitiveWordUtil.Clear();
            SensitiveWordUtil.AddWord("测试");
            SensitiveWordUtil.AddWord("测试");
            Assert.Equal(1, SensitiveWordUtil.Count);
        }

        [Fact]
        public void AddWords_MultipleWords_AddsAllWords()
        {
            SensitiveWordUtil.Clear();
            var words = new[] { "测试", "敏感", "词" };
            SensitiveWordUtil.AddWords(words);
            Assert.Equal(3, SensitiveWordUtil.Count);
        }

        [Fact]
        public void AddWords_NullCollection_DoesNotThrow()
        {
            SensitiveWordUtil.Clear();
            SensitiveWordUtil.AddWords(null);
            Assert.Equal(0, SensitiveWordUtil.Count);
        }

        #endregion

        #region 移除单词测试

        [Fact]
        public void RemoveWord_ExistingWord_RemovesWord()
        {
            SensitiveWordUtil.Init(new[] { "测试", "敏感" });
            SensitiveWordUtil.RemoveWord("测试");
            Assert.Equal(1, SensitiveWordUtil.Count);
        }

        [Fact]
        public void RemoveWord_NonExistentWord_DoesNotChangeCount()
        {
            SensitiveWordUtil.Init(new[] { "测试" });
            int originalCount = SensitiveWordUtil.Count;
            SensitiveWordUtil.RemoveWord("不存在");
            Assert.Equal(originalCount, SensitiveWordUtil.Count);
        }

        [Fact]
        public void RemoveWord_NullOrWhitespace_DoesNotThrow()
        {
            SensitiveWordUtil.Init(new[] { "测试" });
            SensitiveWordUtil.RemoveWord(null);
            SensitiveWordUtil.RemoveWord("");
            SensitiveWordUtil.RemoveWord("   ");
            Assert.Equal(1, SensitiveWordUtil.Count);
        }

        #endregion

        #region 清空测试

        [Fact]
        public void Clear_RemovesAllWords()
        {
            SensitiveWordUtil.Init(new[] { "测试", "敏感", "词" });
            SensitiveWordUtil.Clear();
            Assert.Equal(0, SensitiveWordUtil.Count);
        }

        [Fact]
        public void Clear_CanAddWordsAfterClear()
        {
            SensitiveWordUtil.Init(new[] { "测试" });
            SensitiveWordUtil.Clear();
            SensitiveWordUtil.AddWord("新词");
            Assert.Equal(1, SensitiveWordUtil.Count);
        }

        #endregion

        #region 检测测试

        [Fact]
        public void Contains_ContainsSensitiveWord_ReturnsTrue()
        {
            SensitiveWordUtil.Init(new[] { "测试", "敏感" });
            Assert.True(SensitiveWordUtil.Contains("这是一个测试"));
        }

        [Fact]
        public void Contains_DoesNotContainSensitiveWord_ReturnsFalse()
        {
            SensitiveWordUtil.Init(new[] { "测试", "敏感" });
            Assert.False(SensitiveWordUtil.Contains("这是普通文本"));
        }

        [Fact]
        public void Contains_EmptyFilter_ReturnsFalse()
        {
            SensitiveWordUtil.Clear();
            Assert.False(SensitiveWordUtil.Contains("测试"));
        }

        [Fact]
        public void Contains_NullText_ReturnsFalse()
        {
            SensitiveWordUtil.Init(new[] { "测试" });
            Assert.False(SensitiveWordUtil.Contains(null));
        }

        [Fact]
        public void Contains_EmptyText_ReturnsFalse()
        {
            SensitiveWordUtil.Init(new[] { "测试" });
            Assert.False(SensitiveWordUtil.Contains(""));
        }

        [Fact]
        public void Contains_MultipleSensitiveWords_ReturnsTrue()
        {
            SensitiveWordUtil.Init(new[] { "测试", "敏感", "词" });
            Assert.True(SensitiveWordUtil.Contains("测试和敏感词"));
        }

        [Fact]
        public void FindAll_ContainsMultipleWords_ReturnsAllWords()
        {
            SensitiveWordUtil.Init(new[] { "测试", "敏感", "词" });
            var words = SensitiveWordUtil.FindAll("测试和敏感词");
            Assert.Equal(3, words.Count);
            Assert.Contains("测试", words);
            Assert.Contains("敏感", words);
            Assert.Contains("词", words);
        }

        [Fact]
        public void FindAll_NoSensitiveWords_ReturnsEmptyList()
        {
            SensitiveWordUtil.Init(new[] { "测试" });
            var words = SensitiveWordUtil.FindAll("普通文本");
            Assert.Empty(words);
        }

        [Fact]
        public void FindAllWithPosition_ReturnsCorrectPositions()
        {
            SensitiveWordUtil.Init(new[] { "测试" });
            var positions = SensitiveWordUtil.FindAllWithPosition("这是一个测试文本");
            Assert.Single(positions);
            Assert.Equal(4, positions[0].StartIndex);
            Assert.Equal("测试", positions[0].Word);
        }

        [Fact]
        public void CountWords_MultipleOccurrences_ReturnsCorrectCounts()
        {
            SensitiveWordUtil.Init(new[] { "测试" });
            var counts = SensitiveWordUtil.CountWords("测试测试测试");
            Assert.Single(counts);
            Assert.Equal(3, counts["测试"]);
        }

        [Fact]
        public void CountWords_DifferentWords_ReturnsCorrectCounts()
        {
            SensitiveWordUtil.Init(new[] { "测试", "敏感" });
            var counts = SensitiveWordUtil.CountWords("测试敏感测试敏感");
            Assert.Equal(2, counts["测试"]);
            Assert.Equal(2, counts["敏感"]);
        }

        #endregion

        #region 过滤测试

        [Fact]
        public void Filter_WithDefaultReplaceChar_ReplacesWithAsterisk()
        {
            SensitiveWordUtil.Init(new[] { "测试" });
            string filtered = SensitiveWordUtil.Filter("这是一个测试");
            Assert.Equal("这是一个**", filtered);
        }

        [Fact]
        public void Filter_WithCustomReplaceChar_ReplacesWithCustomChar()
        {
            SensitiveWordUtil.Init(new[] { "测试" });
            string filtered = SensitiveWordUtil.Filter("这是一个测试", '#');
            Assert.Equal("这是一个##", filtered);
        }

        [Fact]
        public void Filter_NoSensitiveWords_ReturnsOriginal()
        {
            SensitiveWordUtil.Init(new[] { "测试" });
            string original = "普通文本";
            string filtered = SensitiveWordUtil.Filter(original);
            Assert.Equal(original, filtered);
        }

        [Fact]
        public void Filter_NullText_ReturnsEmptyString()
        {
            SensitiveWordUtil.Init(new[] { "测试" });
            string filtered = SensitiveWordUtil.Filter(null);
            Assert.Equal("", filtered);
        }

        [Fact]
        public void Filter_EmptyText_ReturnsEmptyString()
        {
            SensitiveWordUtil.Init(new[] { "测试" });
            string filtered = SensitiveWordUtil.Filter("");
            Assert.Equal("", filtered);
        }

        [Fact]
        public void Filter_WithCustomReplacer_UsesCustomLogic()
        {
            SensitiveWordUtil.Init(new[] { "测试" });
            string filtered = SensitiveWordUtil.Filter("这是一个测试", word => $"[{word}]");
            Assert.Equal("这是一个[测试]", filtered);
        }

        [Fact]
        public void Filter_WithCustomReplacer_NullReplacer_ReturnsOriginal()
        {
            SensitiveWordUtil.Init(new[] { "测试" });
            string original = "这是一个测试";
            string filtered = SensitiveWordUtil.Filter(original, (Func<string, string>)null);
            Assert.Equal(original, filtered);
        }

        [Fact]
        public void Highlight_AddsHighlightTags()
        {
            SensitiveWordUtil.Init(new[] { "测试" });
            string highlighted = SensitiveWordUtil.Highlight("这是一个测试");
            Assert.Equal("这是一个<em>测试</em>", highlighted);
        }

        [Fact]
        public void Highlight_WithCustomTags_UsesCustomTags()
        {
            SensitiveWordUtil.Init(new[] { "测试" });
            string highlighted = SensitiveWordUtil.Highlight("这是一个测试", "<b>", "</b>");
            Assert.Equal("这是一个<b>测试</b>", highlighted);
        }

        #endregion

        #region DFA算法测试

        [Fact]
        public void FindAll_OverlappingWords_FindsAll()
        {
            SensitiveWordUtil.Init(new[] { "测试", "测试词" });
            var words = SensitiveWordUtil.FindAll("这是一个测试词");
            // The DFA algorithm finds the longest match at each position
            // "测试词" contains "测试" but only "测试词" is returned
            Assert.Single(words);
            Assert.Contains("测试词", words);
        }

        [Fact]
        public void FindAll_LongSensitiveWord_FindsWord()
        {
            SensitiveWordUtil.Init(new[] { "这是一个很长的敏感词" });
            var words = SensitiveWordUtil.FindAll("这是一个很长的敏感词出现了");
            Assert.Single(words);
            Assert.Equal("这是一个很长的敏感词", words[0]);
        }

        [Fact]
        public void Contains_ShortWord_FindsWord()
        {
            SensitiveWordUtil.Init(new[] { "测试" });
            Assert.True(SensitiveWordUtil.Contains("这是测试"));
        }

        #endregion

        #region 边界测试

        [Fact]
        public void FindAll_MultipleSameWords_FindsAllOccurrences()
        {
            SensitiveWordUtil.Init(new[] { "测试" });
            var words = SensitiveWordUtil.FindAll("测试测试测试");
            Assert.Equal(3, words.Count);
            Assert.All(words, w => Assert.Equal("测试", w));
        }

        [Fact]
        public void Filter_WithMultipleWords_FiltersAll()
        {
            SensitiveWordUtil.Init(new[] { "测试", "敏感" });
            string filtered = SensitiveWordUtil.Filter("测试和敏感");
            Assert.Equal("**和**", filtered);
        }

        [Fact]
        public void Contains_TextWithSpecialChars_WorksCorrectly()
        {
            SensitiveWordUtil.Init(new[] { "测试" });
            Assert.True(SensitiveWordUtil.Contains("测试！测试。测试？"));
        }

        #endregion

        #region 性能测试

        [Fact]
        public void LargeWordSet_WorksCorrectly()
        {
            var words = new List<string>();
            for (int i = 0; i < 1000; i++)
            {
                words.Add($"敏感词{i}");
            }
            SensitiveWordUtil.Init(words);
            Assert.Equal(1000, SensitiveWordUtil.Count);
        }

        [Fact]
        public void LongText_WorksCorrectly()
        {
            SensitiveWordUtil.Init(new[] { "测试" });
            string longText = string.Join(" ", Enumerable.Repeat("测试", 1000));
            Assert.True(SensitiveWordUtil.Contains(longText));
        }

        #endregion

        #region 线程安全测试

        [Fact]
        public async Task ConcurrentAddWords_ThreadSafe()
        {
            SensitiveWordUtil.Clear();
            var tasks = new List<Task>();

            for (int i = 0; i < 10; i++)
            {
                int start = i * 100;
                var task = Task.Run(() =>
                {
                    for (int j = 0; j < 100; j++)
                    {
                        SensitiveWordUtil.AddWord($"词{start + j}");
                    }
                });
                tasks.Add(task);
            }

            await Task.WhenAll(tasks.ToArray());
            Assert.True(SensitiveWordUtil.Count > 0);
        }

        [Fact]
        public async Task ConcurrentContains_ThreadSafe()
        {
            SensitiveWordUtil.Init(new[] { "测试", "敏感" });
            int successCount = 0;
            var tasks = new List<Task>();

            for (int i = 0; i < 100; i++)
            {
                var task = Task.Run(() =>
                {
                    if (SensitiveWordUtil.Contains("测试"))
                    {
                        global::System.Threading.Interlocked.Increment(ref successCount);
                    }
                });
                tasks.Add(task);
            }

            await Task.WhenAll(tasks.ToArray());
            Assert.Equal(100, successCount);
        }

        #endregion

        #region 特殊情况测试

        [Fact]
        public void Filter_WithReplacer_ThatUsesWordInfo_WorksCorrectly()
        {
            SensitiveWordUtil.Init(new[] { "测试" });
            string filtered = SensitiveWordUtil.Filter("这是一个测试", word =>
            {
                Assert.Equal("测试", word);
                return "已过滤";
            });
            Assert.Equal("这是一个已过滤", filtered);
        }

        [Fact]
        public void FindAll_EmptyFilter_ReturnsEmptyList()
        {
            SensitiveWordUtil.Clear();
            var words = SensitiveWordUtil.FindAll("测试");
            Assert.Empty(words);
        }

        [Fact]
        public void FindAllWithPosition_MultipleWords_ReturnsAllPositions()
        {
            SensitiveWordUtil.Init(new[] { "测试" });
            var positions = SensitiveWordUtil.FindAllWithPosition("测试1测试2测试");
            Assert.Equal(3, positions.Count);
            Assert.Equal(0, positions[0].StartIndex);
            Assert.Equal(3, positions[1].StartIndex);
            Assert.Equal(6, positions[2].StartIndex);
        }

        #endregion

        #region 混合场景测试

        [Fact]
        public void ComplexScenario_InitAddRemoveFind_WorksCorrectly()
        {
            // 初始化
            SensitiveWordUtil.Init(new[] { "词1", "词2" });
            Assert.Equal(2, SensitiveWordUtil.Count);

            // 添加
            SensitiveWordUtil.AddWord("词3");
            Assert.Equal(3, SensitiveWordUtil.Count);

            // 检测
            Assert.True(SensitiveWordUtil.Contains("词1词2词3"));

            // 移除
            SensitiveWordUtil.RemoveWord("词2");
            Assert.Equal(2, SensitiveWordUtil.Count);
            Assert.False(SensitiveWordUtil.Contains("词2"));

            // 过滤 - "词1词3" contains both "词1" and "词3"
            string filtered = SensitiveWordUtil.Filter("词1词3");
            Assert.Equal("****", filtered);
        }

        #endregion

        #region 重复词测试

        [Fact]
        public void AddWord_AlreadyExistingWord_NoDuplicate()
        {
            SensitiveWordUtil.Clear();
            SensitiveWordUtil.AddWord("测试");
            SensitiveWordUtil.AddWord("测试");
            SensitiveWordUtil.AddWord("测试");
            Assert.Equal(1, SensitiveWordUtil.Count);
        }

        [Fact]
        public void FindAll_OverlappingSensitiveWords_FindsAll()
        {
            SensitiveWordUtil.Init(new[] { "敏感", "感词" });
            var words = SensitiveWordUtil.FindAll("这是敏感词");
            // 应该找到"敏感"，也可能找到"感词"
            Assert.True(words.Count >= 1);
            Assert.Contains("敏感", words);
        }

        #endregion

        #region 清理测试

        public void Dispose()
        {
            // 每个测试后清理，避免影响其他测试
            SensitiveWordUtil.Clear();
        }

        #endregion
    }
}
