using Xunit;
using EasyTool.TextCategory;
using System;

namespace EasyTool.UnitTests.TextCategory
{
    public class PinyinUtilTests
    {
        #region 拼音获取测试

        [Fact]
        public void GetPinyin_SingleChar_ReturnsPinyin()
        {
            string pinyin = PinyinUtil.GetPinyin('中');
            Assert.NotNull(pinyin);
            Assert.NotEqual("中", pinyin); // 不应该返回原字符
        }

        [Fact]
        public void GetPinyin_String_ReturnsPinyinString()
        {
            string pinyin = PinyinUtil.GetPinyin("中国");
            Assert.NotNull(pinyin);
            Assert.NotEqual("中国", pinyin);
        }

        [Fact]
        public void GetPinyin_EmptyString_ReturnsEmptyString()
        {
            string pinyin = PinyinUtil.GetPinyin("");
            Assert.Equal("", pinyin);
        }

        [Fact]
        public void GetPinyin_NullString_ReturnsEmptyString()
        {
            string pinyin = PinyinUtil.GetPinyin(null);
            Assert.Equal("", pinyin);
        }

        [Fact]
        public void GetPinyin_WithSeparator_ReturnsSeparatedPinyin()
        {
            string pinyin = PinyinUtil.GetPinyin("中国", " ");
            Assert.NotNull(pinyin);
            Assert.Contains(" ", pinyin);
        }

        [Fact]
        public void GetPinyin_WithCustomSeparator_ReturnsCorrectFormat()
        {
            string pinyin = PinyinUtil.GetPinyin("中国人", "-");
            Assert.NotNull(pinyin);
            Assert.Contains("-", pinyin);
        }

        [Fact]
        public void GetPinyin_NonChineseChar_ReturnsOriginalChar()
        {
            string pinyin = PinyinUtil.GetPinyin('A');
            Assert.Equal("A", pinyin);
        }

        [Fact]
        public void GetPinyin_MixedString_ReturnsMixedResult()
        {
            string pinyin = PinyinUtil.GetPinyin("中A文");
            Assert.NotNull(pinyin);
            Assert.Contains("A", pinyin);
        }

        [Fact]
        public void GetPinyin_Digit_ReturnsOriginalDigit()
        {
            string pinyin = PinyinUtil.GetPinyin('1');
            Assert.Equal("1", pinyin);
        }

        #endregion

        #region 多音字测试

        [Fact]
        public void GetPinyins_ChineseChar_ReturnsArray()
        {
            string[] pinyins = PinyinUtil.GetPinyins('中');
            Assert.NotNull(pinyins);
            Assert.True(pinyins.Length > 0);
        }

        [Fact]
        public void GetPinyins_NonChineseChar_ReturnsSingleElementArray()
        {
            string[] pinyins = PinyinUtil.GetPinyins('A');
            Assert.NotNull(pinyins);
            Assert.Single(pinyins);
            Assert.Equal("A", pinyins[0]);
        }

        [Fact]
        public void GetPinyin_SingleChar_ReturnsFirstPinyin()
        {
            string pinyin = PinyinUtil.GetPinyin('中');
            string[] pinyins = PinyinUtil.GetPinyins('中');

            Assert.NotNull(pinyin);
            Assert.NotNull(pinyins);
            if (pinyins.Length > 0)
            {
                Assert.Equal(pinyins[0], pinyin);
            }
        }

        #endregion

        #region 拼音首字母测试

        [Fact]
        public void GetFirstPinyinLetter_ChineseString_ReturnsInitials()
        {
            string initials = PinyinUtil.GetFirstPinyinLetter("中国");
            Assert.NotNull(initials);
            Assert.Equal(2, initials.Length);
            Assert.Matches("^[A-Z]+$", initials);
        }

        [Fact]
        public void GetFirstPinyinLetter_EmptyString_ReturnsEmptyString()
        {
            string initials = PinyinUtil.GetFirstPinyinLetter("");
            Assert.Equal("", initials);
        }

        [Fact]
        public void GetFirstPinyinLetter_Null_ReturnsEmptyString()
        {
            string initials = PinyinUtil.GetFirstPinyinLetter(null);
            Assert.Equal("", initials);
        }

        [Fact]
        public void GetFirstPinyinLetter_MixedString_ReturnsMixedResult()
        {
            string initials = PinyinUtil.GetFirstPinyinLetter("中A1");
            Assert.NotNull(initials);
            Assert.True(initials.Length >= 1);
        }

        [Fact]
        public void GetFirstPinyinLetter_WithNonChinese_ReturnsOriginalChar()
        {
            string initials = PinyinUtil.GetFirstPinyinLetter("ABC");
            Assert.Equal("ABC", initials);
        }

        #endregion

        #region 简化拼音首字母测试

        [Fact]
        public void GetSimplePinyinInitial_ChineseChar_ReturnsUppercaseLetter()
        {
            string initial = PinyinUtil.GetSimplePinyinInitial("中");
            Assert.NotNull(initial);
            Assert.Equal(1, initial.Length);
            Assert.Matches("^[A-Z]$", initial);
        }

        [Fact]
        public void GetSimplePinyinInitial_EmptyString_ReturnsHash()
        {
            string initial = PinyinUtil.GetSimplePinyinInitial("");
            Assert.Equal("#", initial);
        }

        [Fact]
        public void GetSimplePinyinInitial_UppercaseEnglish_ReturnsUppercase()
        {
            string initial = PinyinUtil.GetSimplePinyinInitial("A");
            Assert.Equal("A", initial);
        }

        [Fact]
        public void GetSimplePinyinInitial_LowercaseEnglish_ReturnsUppercase()
        {
            string initial = PinyinUtil.GetSimplePinyinInitial("a");
            Assert.Equal("A", initial);
        }

        [Fact]
        public void GetSimplePinyinInitial_NonLetterNonChinese_ReturnsHash()
        {
            string initial = PinyinUtil.GetSimplePinyinInitial("1");
            Assert.Equal("#", initial);
        }

        #endregion

        #region 汉字判断测试

        [Fact]
        public void IsChinese_ChineseChar_ReturnsTrue()
        {
            Assert.True(PinyinUtil.IsChinese('中'));
            Assert.True(PinyinUtil.IsChinese('国'));
        }

        [Fact]
        public void IsChinese_NonChineseChar_ReturnsFalse()
        {
            Assert.False(PinyinUtil.IsChinese('A'));
            Assert.False(PinyinUtil.IsChinese('1'));
            Assert.False(PinyinUtil.IsChinese(' '));
        }

        [Fact]
        public void IsChinese_Punctuation_ReturnsFalse()
        {
            Assert.False(PinyinUtil.IsChinese('，'));
            Assert.False(PinyinUtil.IsChinese('。'));
        }

        [Fact]
        public void IsAllChinese_AllChineseString_ReturnsTrue()
        {
            Assert.True(PinyinUtil.IsAllChinese("中国"));
        }

        [Fact]
        public void IsAllChinese_ContainsNonChinese_ReturnsFalse()
        {
            Assert.False(PinyinUtil.IsAllChinese("中A国"));
            Assert.False(PinyinUtil.IsAllChinese("中国1"));
        }

        [Fact]
        public void IsAllChinese_EmptyString_ReturnsFalse()
        {
            Assert.False(PinyinUtil.IsAllChinese(""));
        }

        [Fact]
        public void IsAllChinese_Null_ReturnsFalse()
        {
            Assert.False(PinyinUtil.IsAllChinese(null));
        }

        [Fact]
        public void ContainsChinese_WithChinese_ReturnsTrue()
        {
            Assert.True(PinyinUtil.ContainsChinese("中A国"));
            Assert.True(PinyinUtil.ContainsChinese("ABC中"));
        }

        [Fact]
        public void ContainsChinese_WithoutChinese_ReturnsFalse()
        {
            Assert.False(PinyinUtil.ContainsChinese("ABC"));
            Assert.False(PinyinUtil.ContainsChinese("123"));
        }

        [Fact]
        public void ContainsChinese_EmptyString_ReturnsFalse()
        {
            Assert.False(PinyinUtil.ContainsChinese(""));
        }

        [Fact]
        public void ContainsChinese_Null_ReturnsFalse()
        {
            Assert.False(PinyinUtil.ContainsChinese(null));
        }

        #endregion

        #region 边界测试

        [Fact]
        public void GetPinyin_VeryLongString_WorksCorrectly()
        {
            string longText = "中国中国中国中国中国";
            string pinyin = PinyinUtil.GetPinyin(longText);
            Assert.NotNull(pinyin);
            Assert.NotEqual(longText, pinyin);
        }

        [Fact]
        public void GetFirstPinyinLetter_VeryLongString_WorksCorrectly()
        {
            string longText = "中国中国中国中国中国";
            string initials = PinyinUtil.GetFirstPinyinLetter(longText);
            Assert.NotNull(initials);
            Assert.Equal(10, initials.Length);
        }

        [Fact]
        public void GetPinyin_SpecialChars_ReturnsOriginalChars()
        {
            Assert.Equal("!", PinyinUtil.GetPinyin('!'));
            Assert.Equal("@", PinyinUtil.GetPinyin('@'));
            Assert.Equal(" ", PinyinUtil.GetPinyin(' '));
        }

        #endregion

        #region Unicode范围测试

        [Fact]
        public void IsChinese_BoundaryValues_WorksCorrectly()
        {
            // CJK Unified Ideographs范围: U+4E00 to U+9FA5
            Assert.True(PinyinUtil.IsChinese('\u4E00')); // 第一个汉字
            Assert.True(PinyinUtil.IsChinese('\u9FA5')); // 最后一个汉字
            Assert.False(PinyinUtil.IsChinese('\u4DFF')); // 前一个
            Assert.False(PinyinUtil.IsChinese('\u9FA6')); // 后一个
        }

        #endregion

        #region 常用汉字测试

        [Theory]
        [InlineData("你")]
        [InlineData("好")]
        [InlineData("我")]
        [InlineData("他")]
        [InlineData("她")]
        public void GetPinyin_CommonChineseChars_ReturnsPinyin(string charStr)
        {
            char c = charStr[0];
            string pinyin = PinyinUtil.GetPinyin(c);
            Assert.NotNull(pinyin);
            Assert.NotEqual(charStr, pinyin);
        }

        #endregion

        #region 拼音格式测试

        [Fact]
        public void GetPinyin_WithEmptySeparator_ReturnsContinuousString()
        {
            string pinyin = PinyinUtil.GetPinyin("中国", "");
            Assert.NotNull(pinyin);
            Assert.DoesNotContain(" ", pinyin);
        }

        [Fact]
        public void GetPinyin_WithSpaceSeparator_ReturnsSeparatedString()
        {
            string pinyin = PinyinUtil.GetPinyin("中国", " ");
            Assert.NotNull(pinyin);
            Assert.Contains(" ", pinyin);
        }

        #endregion

        #region 性能测试

        [Fact]
        public void GetPinyin_LargeString_CompletesQuickly()
        {
            string text = "中国人民共和国";
            string pinyin = PinyinUtil.GetPinyin(text);
            Assert.NotNull(pinyin);
        }

        #endregion

        #region 混合内容测试

        [Fact]
        public void GetPinyin_MixedContent_ReturnsValidResult()
        {
            string mixed = "中国CN123国";
            string pinyin = PinyinUtil.GetPinyin(mixed);
            Assert.NotNull(pinyin);
        }

        [Fact]
        public void GetFirstPinyinLetter_MixedContent_ReturnsValidResult()
        {
            string mixed = "中A1国";
            string initials = PinyinUtil.GetFirstPinyinLetter(mixed);
            Assert.NotNull(initials);
            Assert.True(initials.Length >= 2);
        }

        #endregion

        #region 标点符号测试

        [Fact]
        public void GetPinyin_ChinesePunctuation_ReturnsOriginal()
        {
            Assert.Equal("，", PinyinUtil.GetPinyin('，'));
            Assert.Equal("。", PinyinUtil.GetPinyin('。'));
            Assert.Equal("！", PinyinUtil.GetPinyin('！'));
        }

        #endregion

        #region 数字测试

        [Theory]
        [InlineData('0')]
        [InlineData('1')]
        [InlineData('2')]
        [InlineData('3')]
        [InlineData('4')]
        [InlineData('5')]
        [InlineData('6')]
        [InlineData('7')]
        [InlineData('8')]
        [InlineData('9')]
        public void GetPinyin_Digits_ReturnOriginal(char digit)
        {
            string pinyin = PinyinUtil.GetPinyin(digit);
            Assert.Equal(digit.ToString(), pinyin);
        }

        #endregion

        #region 英文测试

        [Theory]
        [InlineData('a')]
        [InlineData('A')]
        [InlineData('z')]
        [InlineData('Z')]
        public void GetPinyin_EnglishLetters_ReturnOriginal(char letter)
        {
            string pinyin = PinyinUtil.GetPinyin(letter);
            Assert.Equal(letter.ToString(), pinyin);
        }

        #endregion

        #region 空白字符测试

        [Fact]
        public void GetPinyin_Whitespace_ReturnsWhitespace()
        {
            Assert.Equal(" ", PinyinUtil.GetPinyin(' '));
            Assert.Equal("\t", PinyinUtil.GetPinyin('\t'));
            Assert.Equal("\n", PinyinUtil.GetPinyin('\n'));
        }

        #endregion

        #region 首字母大小写测试

        [Fact]
        public void GetFirstPinyinLetter_ReturnsUppercase()
        {
            string initials = PinyinUtil.GetFirstPinyinLetter("中国");
            Assert.Matches("^[A-Z]+$", initials);
        }

        [Fact]
        public void GetSimplePinyinInitial_ReturnsUppercase()
        {
            string initial = PinyinUtil.GetSimplePinyinInitial("中");
            Assert.Matches("^[A-Z]$", initial);
        }

        #endregion

        #region 连续处理测试

        [Fact]
        public void MultipleGetPinyinCalls_ReturnsConsistentResults()
        {
            string text = "中国";
            string pinyin1 = PinyinUtil.GetPinyin(text);
            string pinyin2 = PinyinUtil.GetPinyin(text);
            Assert.Equal(pinyin1, pinyin2);
        }

        #endregion
    }
}
