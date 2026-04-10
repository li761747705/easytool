using Xunit;
using EasyTool.BusinessCategory;

namespace EasyTool.UnitTests.BusinessCategory
{
    public class PasswordGeneratorTests
    {
        [Fact]
        public void Generate_Default_ReturnsValidPassword()
        {
            var password = PasswordGenerator.Generate();

            Assert.NotNull(password);
            Assert.Equal(12, password.Length);
        }

        [Theory]
        [InlineData(8)]
        [InlineData(12)]
        [InlineData(16)]
        [InlineData(24)]
        public void Generate_CustomLength_ReturnsCorrectLength(int length)
        {
            var password = PasswordGenerator.Generate(length: length);

            Assert.Equal(length, password.Length);
        }

        [Fact]
        public void Generate_OnlyDigits_ReturnsOnlyDigits()
        {
            var password = PasswordGenerator.Generate(
                includeLowerCase: false,
                includeUpperCase: false,
                includeDigits: true,
                includeSpecialChars: false);

            Assert.Matches("^[0-9]+$", password);
        }

        [Fact]
        public void Generate_OnlyLetters_ReturnsOnlyLetters()
        {
            var password = PasswordGenerator.Generate(
                includeLowerCase: true,
                includeUpperCase: true,
                includeDigits: false,
                includeSpecialChars: false);

            Assert.Matches("^[a-zA-Z]+$", password);
        }

        [Fact]
        public void Generate_ExcludeAmbiguous_NoAmbiguousChars()
        {
            var ambiguous = "l1IO0";

            var password = PasswordGenerator.Generate(
                length: 100,
                excludeAmbiguous: true);

            foreach (var c in ambiguous)
            {
                Assert.DoesNotContain(c, password);
            }
        }

        [Fact]
        public void GeneratePin_ReturnsOnlyDigits()
        {
            var pin = PasswordGenerator.GeneratePin(6);

            Assert.Equal(6, pin.Length);
            Assert.Matches("^[0-9]{6}$", pin);
        }

        [Fact]
        public void GenerateStrong_Returns16Chars()
        {
            var password = PasswordGenerator.GenerateStrong();

            Assert.Equal(16, password.Length);
        }

        [Fact]
        public void GeneratePassphrase_ReturnsMultipleWords()
        {
            var passphrase = PasswordGenerator.GeneratePassphrase(4);

            var words = passphrase.Split('-');
            Assert.Equal(4, words.Length);
        }

        [Fact]
        public void GenerateBatch_ReturnsCorrectCount()
        {
            var passwords = PasswordGenerator.GenerateBatch(10, 12);

            Assert.Equal(10, passwords.Count);
            Assert.All(passwords, p => Assert.Equal(12, p.Length));
        }

        [Theory]
        [InlineData("", PasswordGenerator.PasswordStrength.Weak)]
        [InlineData("123", PasswordGenerator.PasswordStrength.Weak)]
        [InlineData("password", PasswordGenerator.PasswordStrength.Fair)]
        [InlineData("Password1", PasswordGenerator.PasswordStrength.Good)]
        [InlineData("Password123!", PasswordGenerator.PasswordStrength.Strong)]
        [InlineData("Str0ngP@ssw0rd!", PasswordGenerator.PasswordStrength.VeryStrong)]
        public void CheckStrength_ReturnsCorrectStrength(string password, PasswordGenerator.PasswordStrength expected)
        {
            var strength = PasswordGenerator.CheckStrength(password);

            Assert.Equal(expected, strength);
        }

        [Fact]
        public void CheckStrength_LongPassword_ReturnsStrong()
        {
            var password = "ThisIsAVeryStrongPassword123!@#";

            var strength = PasswordGenerator.CheckStrength(password);

            Assert.True(strength >= PasswordGenerator.PasswordStrength.Strong);
        }
    }
}