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

        #region 边界测试

        [Fact]
        public void ChineseName_InvalidGender_ReturnsValidName()
        {
            // 无效性别参数应返回默认名字
            var name = FakerUtil.ChineseName("invalid");
            Assert.NotNull(name);
            Assert.True(name.Length >= 2);
        }

        [Fact]
        public void ChineseAddress_ContainsProvince()
        {
            var address = FakerUtil.ChineseAddress();
            Assert.NotNull(address);
            // 地址应包含省或市
            Assert.True(address.Contains("省") || address.Contains("市") || address.Contains("区"));
        }

        [Fact]
        public void PhoneNumber_StartsWith1()
        {
            for (int i = 0; i < 10; i++)
            {
                var phone = FakerUtil.PhoneNumber();
                Assert.StartsWith("1", phone);
                Assert.Equal(11, phone.Length);
            }
        }

        [Fact]
        public void Email_ContainsCommonDomain()
        {
            var email = FakerUtil.Email();
            Assert.NotNull(email);
            Assert.True(email.Contains("@qq.com") ||
                        email.Contains("@163.com") ||
                        email.Contains("@gmail.com") ||
                        email.Contains("@126.com") ||
                        email.Contains("@outlook.com"));
        }

        [Fact]
        public void RandomInt_MaxIsZero_ThrowsArgumentException()
        {
            var ex = Assert.Throws<ArgumentException>(() => FakerUtil.RandomInt(0));
            Assert.Contains("必须大于 0", ex.Message);
        }

        [Fact]
        public void RandomInt_MinEqualsMax_ThrowsArgumentException()
        {
            var ex = Assert.Throws<ArgumentException>(() => FakerUtil.RandomInt(5, 5));
            Assert.Contains("必须小于 max", ex.Message);
        }

        [Fact]
        public void RandomNumberString_LengthOne_ReturnsSingleDigit()
        {
            var result = FakerUtil.RandomNumberString(1);
            Assert.Equal(1, result.Length);
            Assert.Matches("^[0-9]$", result);
        }

        [Fact]
        public void RandomString_LengthZero_ReturnsEmpty()
        {
            var result = FakerUtil.RandomString(0);
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void RandomMoney_MinEqualsMax_ThrowsArgumentException()
        {
            var ex = Assert.Throws<ArgumentException>(() => FakerUtil.RandomMoney(50, 50));
            Assert.Contains("必须小于 max", ex.Message);
        }

        [Fact]
        public void RandomDate_YearsZero_ThrowsArgumentException()
        {
            var ex = Assert.Throws<ArgumentException>(() => FakerUtil.RandomDate(0, 0));
            Assert.Contains("不能同时小于等于 0", ex.Message);
        }

        [Fact]
        public void RandomDate_ValidRange_ReturnsDateInRange()
        {
            var date = FakerUtil.RandomDate(1, 0);
            Assert.InRange(date, DateTime.Now.AddYears(-1), DateTime.Now);
        }

        [Fact]
        public void RandomChoice_EmptyArray_ThrowsArgumentException()
        {
            var emptyArray = new string[0];
            var ex = Assert.Throws<ArgumentException>(() => FakerUtil.RandomChoice(emptyArray));
            Assert.Contains("至少一个元素", ex.Message);
        }

        [Fact]
        public void RandomChoice_SingleItem_ReturnsThatItem()
        {
            var singleItem = new[] { "only" };
            var result = FakerUtil.RandomChoice(singleItem);
            Assert.Equal("only", result);
        }

        #endregion
    }
}