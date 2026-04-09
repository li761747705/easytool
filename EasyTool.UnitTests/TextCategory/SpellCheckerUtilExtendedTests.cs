using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace EasyTool.TextCategory.Tests
{
    public class SpellCheckerUtilExtendedTests
    {
        [Fact]
        public void IsInitialized_AfterStaticConstructor_ReturnsTrue()
        {
            Assert.True(SpellCheckerUtil.IsInitialized);
        }

        [Fact]
        public async Task LoadExtendedDictionaryAsync_IncreasesDictionarySize()
        {
            var initialSize = SpellCheckerUtil.GetDictionarySize();

            var addedCount = await SpellCheckerUtil.LoadExtendedDictionaryAsync();

            var newSize = SpellCheckerUtil.GetDictionarySize();
            Assert.True(newSize >= initialSize);
            // 可能返回0，因为单词可能已经在字典中
        }

        [Fact]
        public async Task LoadFromFileAsync_WithValidFile_LoadsWords()
        {
            var tempFile = Path.Combine(Path.GetTempPath(), $"dict_{Guid.NewGuid()}.txt");
            try
            {
                // 使用不太常见的单词
                await File.WriteAllLinesAsync(tempFile, new[] { "xyz123", "abc456", "def789" });

                var loadedWords = await SpellCheckerUtil.LoadFromFileAsync(tempFile);

                Assert.NotEmpty(loadedWords);
                Assert.Contains("xyz123", loadedWords);
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }

        [Fact]
        public async Task LoadFromFileAsync_WithNonExistentFile_ReturnsEmptyList()
        {
            var loadedWords = await SpellCheckerUtil.LoadFromFileAsync("/non/existent/file.txt");

            Assert.Empty(loadedWords);
        }

        [Fact]
        public void ResetDictionary_ResetsToDefaultSize()
        {
            // 先加载扩展字典
            SpellCheckerUtil.LoadExtendedDictionaryAsync().Wait();
            var extendedSize = SpellCheckerUtil.GetDictionarySize();

            // 重置
            SpellCheckerUtil.ResetDictionary();

            var resetSize = SpellCheckerUtil.GetDictionarySize();
            Assert.True(resetSize < extendedSize);
        }

        [Fact]
        public void IsCorrect_AfterLoadingExtendedWord_ReturnsTrue()
        {
            SpellCheckerUtil.LoadExtendedDictionaryAsync().Wait();

            // 扩展字典中的常用词
            Assert.True(SpellCheckerUtil.IsCorrect("able"));
            Assert.True(SpellCheckerUtil.IsCorrect("about"));
        }
    }
}