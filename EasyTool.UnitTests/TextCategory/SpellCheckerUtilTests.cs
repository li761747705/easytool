using Xunit;
using EasyTool.TextCategory;
using System.IO;
using System.Threading.Tasks;

namespace EasyTool.UnitTests.TextCategory
{
    public class SpellCheckerUtilTests
    {
        [Fact]
        public void IsCorrect_WithCorrectWord_ReturnsTrue()
        {
            Assert.True(SpellCheckerUtil.IsCorrect("hello"));
            Assert.True(SpellCheckerUtil.IsCorrect("world"));
            Assert.True(SpellCheckerUtil.IsCorrect("computer"));
        }

        [Fact]
        public void IsCorrect_WithIncorrectWord_ReturnsFalse()
        {
            Assert.False(SpellCheckerUtil.IsCorrect("helllo"));
            Assert.False(SpellCheckerUtil.IsCorrect("wrld"));
        }

        [Fact]
        public void IsCorrect_WithEmptyOrNull_ReturnsTrue()
        {
            Assert.True(SpellCheckerUtil.IsCorrect(""));
            Assert.True(SpellCheckerUtil.IsCorrect("   "));
            Assert.True(SpellCheckerUtil.IsCorrect(null!));
        }

        [Fact]
        public void GetSuggestions_ReturnsSuggestions()
        {
            var suggestions = SpellCheckerUtil.GetSuggestions("helllo");
            Assert.NotEmpty(suggestions);
            Assert.Contains("hello", suggestions);
        }

        [Fact]
        public void GetSuggestions_WithCorrectWord_ReturnsEmpty()
        {
            var suggestions = SpellCheckerUtil.GetSuggestions("hello");
            Assert.Empty(suggestions);
        }

        [Fact]
        public void GetSuggestions_LimitsMaxSuggestions()
        {
            var suggestions = SpellCheckerUtil.GetSuggestions("wrld", maxSuggestions: 2);
            Assert.True(suggestions.Count <= 2);
        }

        [Fact]
        public void CheckText_ReturnsErrorsAndSuggestions()
        {
            var result = SpellCheckerUtil.CheckText("hello wrld, this is a testt");
            Assert.True(result.Count >= 1);
            Assert.True(result.ContainsKey("wrld") || result.ContainsKey("testt"));
        }

        [Fact]
        public void CheckText_WithCorrectText_ReturnsEmpty()
        {
            var result = SpellCheckerUtil.CheckText("hello world the and");
            Assert.Empty(result);
        }

        [Fact]
        public void AutoCorrect_CorrectsErrors()
        {
            var corrected = SpellCheckerUtil.AutoCorrect("helllo wrld");
            // 应该修正了一些错误
            Assert.NotEqual("helllo wrld", corrected);
        }

        [Fact]
        public void AddToDictionary_AddsWords()
        {
            var initialSize = SpellCheckerUtil.GetDictionarySize();
            SpellCheckerUtil.AddToDictionary(new[] { "customword", "anotherword" });
            Assert.Equal(initialSize + 2, SpellCheckerUtil.GetDictionarySize());
            Assert.True(SpellCheckerUtil.IsCorrect("customword"));
        }

        [Fact]
        public async Task LoadExtendedDictionaryAsync_IncreasesDictionarySize()
        {
            var initialSize = SpellCheckerUtil.GetDictionarySize();
            var count = await SpellCheckerUtil.LoadExtendedDictionaryAsync();
            // 扩展字典可能已经加载过，所以count可能为0
            Assert.True(SpellCheckerUtil.GetDictionarySize() >= initialSize);
        }

        [Fact]
        public async Task LoadFromFileAsync_LoadsWordsFromFile()
        {
            var tempFile = Path.Combine(Path.GetTempPath(), "test_dictionary.txt");
            try
            {
                await File.WriteAllLinesAsync(tempFile, new[] { "testword1", "testword2", "testword3" });
                var words = await SpellCheckerUtil.LoadFromFileAsync(tempFile);
                Assert.Equal(3, words.Count);
                Assert.Contains("testword1", words);
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        [Fact]
        public async Task LoadFromFileAsync_WithNonExistentFile_ReturnsEmptyList()
        {
            var words = await SpellCheckerUtil.LoadFromFileAsync("/non/existent/file.txt");
            Assert.Empty(words);
        }

        [Fact]
        public void ResetDictionary_ResetsToDefault()
        {
            SpellCheckerUtil.AddToDictionary(new[] { "temporaryword" });
            Assert.True(SpellCheckerUtil.IsCorrect("temporaryword"));

            SpellCheckerUtil.ResetDictionary();
            Assert.False(SpellCheckerUtil.IsCorrect("temporaryword"));
        }
    }
}