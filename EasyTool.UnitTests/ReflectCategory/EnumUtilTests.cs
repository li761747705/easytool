using Xunit;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using EasyTool.ReflectCategory;

namespace EasyTool.UnitTests.ReflectCategory
{
    public class EnumUtilTests
    {
        #region Test Enums

        public enum TestStatus
        {
            [Description("待处理")]
            Pending,
            [Description("处理中")]
            Processing,
            [Description("已完成")]
            Completed,
            [Description("已取消")]
            Cancelled,
            NoDescription
        }

        public enum TestPriority
        {
            [Display(Name = "低优先级")]
            Low = 1,
            [Display(Name = "中优先级")]
            Medium = 2,
            [Display(Name = "高优先级")]
            High = 3,
            [Description("使用Description")]
            WithDescription = 4
        }

        [Flags]
        public enum TestFlags
        {
            None = 0,
            Read = 1,
            Write = 2,
            Execute = 4
        }

        #endregion

        #region Description Tests

        [Fact]
        public void GetDescription_WithDescriptionAttribute_ReturnsDescription()
        {
            var desc = EnumUtil.GetDescription(TestStatus.Pending);
            Assert.Equal("待处理", desc);
        }

        [Fact]
        public void GetDescription_WithoutDescriptionAttribute_ReturnsEnumName()
        {
            var desc = EnumUtil.GetDescription(TestStatus.NoDescription);
            Assert.Equal("NoDescription", desc);
        }

        [Fact]
        public void GetAllDescriptions_ReturnsAllDescriptions()
        {
            var dict = EnumUtil.GetAllDescriptions<TestStatus>();
            Assert.Equal(5, dict.Count);
            Assert.Equal("待处理", dict[TestStatus.Pending]);
            Assert.Equal("NoDescription", dict[TestStatus.NoDescription]);
        }

        [Fact]
        public void FromDescription_WithValidDescription_ReturnsEnum()
        {
            var result = EnumUtil.FromDescription<TestStatus>("处理中");
            Assert.Equal(TestStatus.Processing, result);
        }

        [Fact]
        public void FromDescription_WithInvalidDescription_ReturnsNull()
        {
            var result = EnumUtil.FromDescription<TestStatus>("不存在的描述");
            Assert.Null(result);
        }

        [Fact]
        public void FromDescription_IgnoreCase_ReturnsEnum()
        {
            // 大小写不敏感时应该能找到
            var result1 = EnumUtil.FromDescription<TestStatus>("处理中");
            Assert.Equal(TestStatus.Processing, result1);

            // 大小写不敏感时，应该也能找到
            var result2 = EnumUtil.FromDescription<TestStatus>("处理中".ToUpper());
            Assert.Equal(TestStatus.Processing, result2);
        }

        #endregion

        #region Display Tests

        [Fact]
        public void GetDisplayName_WithDisplayAttribute_ReturnsDisplayName()
        {
            var name = EnumUtil.GetDisplayName(TestPriority.High);
            Assert.Equal("高优先级", name);
        }

        [Fact]
        public void GetDisplayName_WithDescriptionAttribute_ReturnsDescription()
        {
            var name = EnumUtil.GetDisplayName(TestPriority.WithDescription);
            Assert.Equal("使用Description", name);
        }

        [Fact]
        public void GetDisplayName_WithoutAnyAttribute_ReturnsEnumName()
        {
            var name = EnumUtil.GetDisplayName(TestStatus.NoDescription);
            Assert.Equal("NoDescription", name);
        }

        [Fact]
        public void GetAllDisplayNames_ReturnsAllDisplayNames()
        {
            var dict = EnumUtil.GetAllDisplayNames<TestPriority>();
            Assert.Equal(4, dict.Count);
            Assert.Equal("高优先级", dict[TestPriority.High]);
        }

        [Fact]
        public void FromDisplayName_WithValidName_ReturnsEnum()
        {
            var result = EnumUtil.FromDisplayName<TestPriority>("中优先级");
            Assert.Equal(TestPriority.Medium, result);
        }

        [Fact]
        public void FromDisplayName_WithInvalidName_ReturnsNull()
        {
            var result = EnumUtil.FromDisplayName<TestPriority>("不存在的名称");
            Assert.Null(result);
        }

        #endregion

        #region Items Tests

        [Fact]
        public void GetItemsWithDescription_ReturnsItemsWithDescription()
        {
            var items = EnumUtil.GetItemsWithDescription<TestStatus>().ToList();
            Assert.Equal(5, items.Count);

            var pendingItem = items.First(i => i.Name == "Pending");
            Assert.Equal(TestStatus.Pending, pendingItem.Value);
            Assert.Equal(0, pendingItem.IntValue);
            Assert.Equal("待处理", pendingItem.Description);
        }

        [Fact]
        public void GetItemsFull_ReturnsItemsWithAllInfo()
        {
            var items = EnumUtil.GetItemsFull<TestPriority>().ToList();
            Assert.Equal(4, items.Count);

            var highItem = items.First(i => i.Name == "High");
            Assert.Equal(TestPriority.High, highItem.Value);
            Assert.Equal(3, highItem.IntValue);
            Assert.Equal("高优先级", highItem.DisplayName);
        }

        #endregion

        #region Flag Tests

        [Fact]
        public void HasFlag_ReturnsCorrectResult()
        {
            var flags = TestFlags.Read | TestFlags.Write;
            Assert.True(EnumUtil.HasFlag(flags, TestFlags.Read));
            Assert.True(EnumUtil.HasFlag(flags, TestFlags.Write));
            Assert.False(EnumUtil.HasFlag(flags, TestFlags.Execute));
        }

        [Fact]
        public void SetFlag_AddsFlag()
        {
            var flags = TestFlags.Read;
            flags = EnumUtil.SetFlag(flags, TestFlags.Write);
            Assert.True(EnumUtil.HasFlag(flags, TestFlags.Write));
        }

        [Fact]
        public void ClearFlag_RemovesFlag()
        {
            var flags = TestFlags.Read | TestFlags.Write;
            flags = EnumUtil.ClearFlag(flags, TestFlags.Write);
            Assert.False(EnumUtil.HasFlag(flags, TestFlags.Write));
            Assert.True(EnumUtil.HasFlag(flags, TestFlags.Read));
        }

        [Fact]
        public void ToggleFlag_TogglesFlag()
        {
            var flags = TestFlags.Read;
            flags = EnumUtil.ToggleFlag(flags, TestFlags.Write);
            Assert.True(EnumUtil.HasFlag(flags, TestFlags.Write));
            flags = EnumUtil.ToggleFlag(flags, TestFlags.Write);
            Assert.False(EnumUtil.HasFlag(flags, TestFlags.Write));
        }

        [Fact]
        public void GetFlags_ReturnsAllFlags()
        {
            var flags = TestFlags.Read | TestFlags.Execute;
            var flagList = EnumUtil.GetFlags(flags).ToList();
            Assert.Equal(2, flagList.Count);
            Assert.Contains(TestFlags.Read, flagList);
            Assert.Contains(TestFlags.Execute, flagList);
        }

        [Fact]
        public void CombineFlags_CombinesFlags()
        {
            var combined = EnumUtil.CombineFlags(TestFlags.Read, TestFlags.Write);
            Assert.True(EnumUtil.HasFlag(combined, TestFlags.Read));
            Assert.True(EnumUtil.HasFlag(combined, TestFlags.Write));
        }

        #endregion

        #region Basic Tests

        [Fact]
        public void GetValues_ReturnsAllValues()
        {
            var values = EnumUtil.GetValues<TestStatus>().ToList();
            Assert.Equal(5, values.Count);
        }

        [Fact]
        public void GetNames_ReturnsAllNames()
        {
            var names = EnumUtil.GetNames<TestStatus>().ToList();
            Assert.Equal(5, names.Count);
            Assert.Contains("Pending", names);
        }

        [Fact]
        public void Parse_ParsesValidString()
        {
            var result = EnumUtil.Parse<TestStatus>("Pending");
            Assert.Equal(TestStatus.Pending, result);
        }

        [Fact]
        public void TryParse_ReturnsCorrectResult()
        {
            Assert.True(EnumUtil.TryParse("Pending", out TestStatus result));
            Assert.Equal(TestStatus.Pending, result);
            Assert.False(EnumUtil.TryParse("Invalid", out result));
        }

        [Fact]
        public void IsDefined_ReturnsCorrectResult()
        {
            Assert.True(EnumUtil.IsDefined(TestStatus.Pending));
            Assert.True(EnumUtil.IsDefined<TestStatus>(0));
            Assert.False(EnumUtil.IsDefined<TestStatus>(999));
        }

        [Fact]
        public void ToInt_ReturnsIntValue()
        {
            Assert.Equal(0, EnumUtil.ToInt(TestStatus.Pending));
            Assert.Equal(2, EnumUtil.ToInt(TestStatus.Completed));
        }

        [Fact]
        public void FromInt_ReturnsEnum()
        {
            var result = EnumUtil.FromInt<TestStatus>(1);
            Assert.Equal(TestStatus.Processing, result);
        }

        [Fact]
        public void GetCount_ReturnsCorrectCount()
        {
            Assert.Equal(5, EnumUtil.GetCount<TestStatus>());
        }

        [Fact]
        public void GetRandomValue_ReturnsValidValue()
        {
            var random = new Random(42);
            var value = EnumUtil.GetRandomValue<TestStatus>(random);
            Assert.True(EnumUtil.IsDefined(value));
        }

        #endregion
    }
}