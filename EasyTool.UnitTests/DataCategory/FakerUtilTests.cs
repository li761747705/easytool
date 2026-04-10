using Xunit;
using EasyTool.DataCategory;

namespace EasyTool.UnitTests.DataCategory
{
    public class FakerUtilTests
    {
        [Fact]
        public void ChineseName_ReturnsValidName()
        {
            var name = FakerUtil.ChineseName();

            Assert.NotNull(name);
            Assert.True(name.Length >= 2);
        }

        [Fact]
        public void ChineseName_Male_ReturnsMaleName()
        {
            var name = FakerUtil.ChineseName("male");

            Assert.NotNull(name);
            Assert.True(name.Length >= 2);
        }

        [Fact]
        public void ChineseName_Female_ReturnsFemaleName()
        {
            var name = FakerUtil.ChineseName("female");

            Assert.NotNull(name);
            Assert.True(name.Length >= 2);
        }

        [Fact]
        public void ChineseAddress_ReturnsValidAddress()
        {
            var address = FakerUtil.ChineseAddress();

            Assert.NotNull(address);
            Assert.Contains("市", address);
        }

        [Fact]
        public void PhoneNumber_Returns11Digits()
        {
            var phone = FakerUtil.PhoneNumber();

            Assert.NotNull(phone);
            Assert.Equal(11, phone.Length);
            Assert.Matches("^1[3-9][0-9]{9}$", phone);
        }

        [Fact]
        public void Email_ReturnsValidEmail()
        {
            var email = FakerUtil.Email();

            Assert.NotNull(email);
            Assert.Contains("@", email);
            Assert.Contains(".", email);
        }

        [Fact]
        public void RandomInt_WithMax_ReturnsValueInRange()
        {
            for (int i = 0; i < 100; i++)
            {
                var result = FakerUtil.RandomInt(10);
                Assert.InRange(result, 0, 9);
            }
        }

        [Fact]
        public void RandomInt_WithRange_ReturnsValueInRange()
        {
            for (int i = 0; i < 100; i++)
            {
                var result = FakerUtil.RandomInt(5, 10);
                Assert.InRange(result, 5, 9);
            }
        }

        [Fact]
        public void RandomNumberString_ReturnsCorrectLength()
        {
            var result = FakerUtil.RandomNumberString(8);

            Assert.Equal(8, result.Length);
            Assert.Matches("^[0-9]{8}$", result);
        }

        [Fact]
        public void RandomString_ReturnsCorrectLength()
        {
            var result = FakerUtil.RandomString(10);

            Assert.Equal(10, result.Length);
        }

        [Fact]
        public void RandomString_LowerCase_ReturnsOnlyLowercase()
        {
            var result = FakerUtil.RandomString(20, lowerCase: true);

            Assert.Matches("^[a-z0-9]+$", result);
        }

        [Fact]
        public void RandomBool_ReturnsBothTrueAndFalse()
        {
            var hasTrue = false;
            var hasFalse = false;

            for (int i = 0; i < 100; i++)
            {
                if (FakerUtil.RandomBool()) hasTrue = true;
                else hasFalse = true;
            }

            Assert.True(hasTrue);
            Assert.True(hasFalse);
        }

        [Fact]
        public void RandomDate_ReturnsValidDate()
        {
            var date = FakerUtil.RandomDate(10, 0);

            Assert.InRange(date, DateTime.Now.AddYears(-10), DateTime.Now);
        }

        [Fact]
        public void RandomMoney_ReturnsValidMoney()
        {
            var money = FakerUtil.RandomMoney(1, 100);

            Assert.InRange(money, 1m, 100m);
        }

        [Fact]
        public void RandomMoney_WithDecimals_HasValidDecimals()
        {
            var money = FakerUtil.RandomMoney(1, 100);

            var str = money.ToString("F2");
            Assert.True(decimal.TryParse(str, out _));
        }

        [Fact]
        public void RandomChoice_ReturnsItemFromList()
        {
            var items = new[] { "a", "b", "c" };

            var result = FakerUtil.RandomChoice(items);

            Assert.Contains(result, items);
        }

        [Fact]
        public void MultipleCalls_ReturnDifferentValues()
        {
            var names = new HashSet<string>();

            for (int i = 0; i < 100; i++)
            {
                names.Add(FakerUtil.ChineseName());
            }

            Assert.True(names.Count > 10);
        }
    }
}