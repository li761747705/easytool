using Xunit;
using EasyTool.CollectionsCategory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyTool.UnitTests.CollectionsCategory
{
    public class BloomFilterUtilTests
    {
        #region 创建测试

        [Fact]
        public void Create_ValidParameters_ReturnsBloomFilter()
        {
            var filter = BloomFilterUtil.Create<string>(1000, 0.01);
            Assert.NotNull(filter);
            Assert.True(filter.BitSize > 0);
            Assert.True(filter.HashCount > 0);
            Assert.Equal(0, filter.ItemCount);
        }

        [Fact]
        public void Create_DefaultFalsePositiveRate_ReturnsValidFilter()
        {
            var filter = BloomFilterUtil.Create<string>(1000);
            Assert.NotNull(filter);
            Assert.True(filter.BitSize > 0);
        }

        #endregion

        #region 计算测试

        [Fact]
        public void CalculateOptimalBitSize_ValidInputs_ReturnsPositiveSize()
        {
            int bitSize = BloomFilterUtil.CalculateOptimalBitSize(1000, 0.01);
            Assert.True(bitSize > 0);
        }

        [Fact]
        public void CalculateOptimalHashCount_ValidInputs_ReturnsPositiveCount()
        {
            int bitSize = 10000;
            int expectedItems = 1000;
            int hashCount = BloomFilterUtil.CalculateOptimalHashCount(bitSize, expectedItems);
            Assert.True(hashCount > 0);
        }

        [Theory]
        [InlineData(1000, 0.01)]
        [InlineData(10000, 0.001)]
        [InlineData(100000, 0.05)]
        public void CalculateOptimalBitSize_DifferentParameters_ReturnsReasonableSize(int itemCount, double falsePositiveRate)
        {
            int bitSize = BloomFilterUtil.CalculateOptimalBitSize(itemCount, falsePositiveRate);
            // For lower false positive rates, we need more bits per item
            // For 0.05 rate with 100000 items: ~3.1M bits / 100000 = ~31 bits per item
            double minBitsPerItem = falsePositiveRate < 0.02 ? 8 : 5;
            Assert.True(bitSize > itemCount * minBitsPerItem, $"Bit size {bitSize} should be at least {itemCount * minBitsPerItem} for {itemCount} items at {falsePositiveRate} FPR");
        }

        #endregion

        #region 基本操作测试

        [Fact]
        public void Add_ValidItem_AddsToFilter()
        {
            var filter = BloomFilterUtil.Create<string>(100);
            filter.Add("test");
            Assert.Equal(1, filter.ItemCount);
        }

        [Fact]
        public void Add_NullItem_ThrowsArgumentNullException()
        {
            var filter = BloomFilterUtil.Create<string>(100);
            Assert.Throws<ArgumentNullException>(() => filter.Add(null!));
        }

        [Fact]
        public void MightContain_AddedItem_ReturnsTrue()
        {
            var filter = BloomFilterUtil.Create<string>(100);
            filter.Add("test");
            Assert.True(filter.MightContain("test"));
        }

        [Fact]
        public void MightContain_NonAddedItem_ReturnsFalse()
        {
            var filter = BloomFilterUtil.Create<string>(100);
            filter.Add("test");
            Assert.False(filter.MightContain("nonexistent"));
        }

        [Fact]
        public void MightContain_NullItem_ReturnsFalse()
        {
            var filter = BloomFilterUtil.Create<string>(100);
            Assert.False(filter.MightContain(null));
        }

        [Fact]
        public void AddRange_MultipleItems_AddsAllItems()
        {
            var filter = BloomFilterUtil.Create<string>(100);
            var items = new List<string> { "item1", "item2", "item3" };
            filter.AddRange(items);
            Assert.Equal(3, filter.ItemCount);
            Assert.True(filter.MightContain("item1"));
            Assert.True(filter.MightContain("item2"));
            Assert.True(filter.MightContain("item3"));
        }

        [Fact]
        public void AddRange_NullCollection_ThrowsArgumentNullException()
        {
            var filter = BloomFilterUtil.Create<string>(100);
            Assert.Throws<ArgumentNullException>(() => filter.AddRange(null!));
        }

        #endregion

        #region 假阳性测试

        [Fact]
        public void FalsePositiveRate_WithinExpectedRange()
        {
            int expectedItems = 1000;
            double desiredFalsePositiveRate = 0.01;
            var filter = BloomFilterUtil.Create<string>(expectedItems, desiredFalsePositiveRate);

            // 添加预期数量的项目
            for (int i = 0; i < expectedItems; i++)
            {
                filter.Add($"item{i}");
            }

            // 测试大量不存在的项目
            int falsePositives = 0;
            int testCount = 1000;
            for (int i = expectedItems; i < expectedItems + testCount; i++)
            {
                if (filter.MightContain($"item{i}"))
                {
                    falsePositives++;
                }
            }

            double actualFalsePositiveRate = (double)falsePositives / testCount;
            // 允许一定的误差，但应该接近期望值
            Assert.True(actualFalsePositiveRate < desiredFalsePositiveRate * 2,
                $"实际假阳性率 {actualFalsePositiveRate} 超过期望值的两倍");
        }

        [Fact]
        public void NoFalseNegatives_AllAddedItemsCanBeFound()
        {
            var filter = BloomFilterUtil.Create<string>(1000);
            var items = new List<string>();

            // 添加1000个项目
            for (int i = 0; i < 1000; i++)
            {
                string item = $"item{i}";
                items.Add(item);
                filter.Add(item);
            }

            // 验证所有添加的项目都能被找到
            foreach (var item in items)
            {
                Assert.True(filter.MightContain(item),
                    $"添加的项目 {item} 未能在过滤器中找到");
            }
        }

        #endregion

        #region 清空测试

        [Fact]
        public void Clear_EmptiesFilter()
        {
            var filter = BloomFilterUtil.Create<string>(100);
            filter.Add("item1");
            filter.Add("item2");
            filter.Clear();

            Assert.Equal(0, filter.ItemCount);
            Assert.False(filter.MightContain("item1"));
            Assert.False(filter.MightContain("item2"));
        }

        [Fact]
        public void Clear_CanAddAfterClear()
        {
            var filter = BloomFilterUtil.Create<string>(100);
            filter.Add("item1");
            filter.Clear();
            filter.Add("item2");

            Assert.Equal(1, filter.ItemCount);
            Assert.True(filter.MightContain("item2"));
        }

        #endregion

        #region 边界测试

        [Fact]
        public void Create_ZeroItemCount_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                BloomFilterUtil.Create<string>(0));
        }

        [Fact]
        public void Create_NegativeItemCount_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                BloomFilterUtil.Create<string>(-100));
        }

        [Fact]
        public void Create_ZeroFalsePositiveRate_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                BloomFilterUtil.Create<string>(100, 0));
        }

        [Fact]
        public void Create_OneFalsePositiveRate_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                BloomFilterUtil.Create<string>(100, 1));
        }

        [Fact]
        public void Create_NegativeFalsePositiveRate_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                BloomFilterUtil.Create<string>(100, -0.01));
        }

        [Fact]
        public void Create_GreaterThanOneFalsePositiveRate_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                BloomFilterUtil.Create<string>(100, 1.5));
        }

        [Fact]
        public void MightContain_EmptyFilter_ReturnsFalse()
        {
            var filter = BloomFilterUtil.Create<string>(100);
            Assert.False(filter.MightContain("anything"));
        }

        #endregion

        #region 序列化测试

        [Fact]
        public void GetBytes_ReturnsValidByteArray()
        {
            var filter = BloomFilterUtil.Create<string>(100);
            filter.Add("test");

            byte[] bytes = filter.GetBytes();
            Assert.NotNull(bytes);
            Assert.True(bytes.Length > 0);
        }

        [Fact]
        public void SetBytes_ValidByteArray_RestoresFilter()
        {
            var filter1 = BloomFilterUtil.Create<string>(100);
            filter1.Add("item1");
            filter1.Add("item2");

            byte[] bytes = filter1.GetBytes();

            // Create a new filter with the same parameters to ensure same bit size
            var filter2 = BloomFilterUtil.Create<string>(100);
            filter2.SetBytes(bytes);

            Assert.True(filter2.MightContain("item1"));
            Assert.True(filter2.MightContain("item2"));
        }

        [Fact]
        public void SetBytes_NullArray_ThrowsArgumentNullException()
        {
            var filter = BloomFilterUtil.Create<string>(100);
            Assert.Throws<ArgumentNullException>(() => filter.SetBytes(null!));
        }

        [Fact]
        public void SetBytes_WrongSizeArray_ThrowsArgumentException()
        {
            var filter = BloomFilterUtil.Create<string>(100);
            byte[] wrongSizeBytes = new byte[10];
            Assert.Throws<ArgumentException>(() => filter.SetBytes(wrongSizeBytes));
        }

        #endregion

        #region 线程安全测试

        [Fact]
        public void ConcurrentAdd_ThreadSafe()
        {
            var filter = BloomFilterUtil.Create<int>(10000);
            int itemCount = 1000;
            var tasks = new List<Task>();

            // 并发添加
            for (int i = 0; i < 10; i++)
            {
                int start = i * itemCount;
                var task = Task.Run(() =>
                {
                    for (int j = 0; j < itemCount; j++)
                    {
                        filter.Add(start + j);
                    }
                });
                tasks.Add(task);
            }

            Task.WaitAll(tasks.ToArray());
            Assert.Equal(itemCount * 10, filter.ItemCount);
        }

        [Fact]
        public void ConcurrentContains_ThreadSafe()
        {
            var filter = BloomFilterUtil.Create<int>(10000);

            // 先添加一些项目
            for (int i = 0; i < 1000; i++)
            {
                filter.Add(i);
            }

            int successCount = 0;
            var tasks = new List<Task>();

            // 并发查询
            for (int i = 0; i < 10; i++)
            {
                var task = Task.Run(() =>
                {
                    for (int j = 0; j < 1000; j++)
                    {
                        if (filter.MightContain(j))
                        {
                            global::System.Threading.Interlocked.Increment(ref successCount);
                        }
                    }
                });
                tasks.Add(task);
            }

            Task.WaitAll(tasks.ToArray());
            Assert.Equal(10000, successCount); // 所有查询都应该成功
        }

        #endregion

        #region 不同类型测试

        [Fact]
        public void IntegerFilter_WorksCorrectly()
        {
            var filter = BloomFilterUtil.Create<int>(100);
            filter.Add(42);
            Assert.True(filter.MightContain(42));
            Assert.False(filter.MightContain(43));
        }

        [Fact]
        public void GuidFilter_WorksCorrectly()
        {
            var filter = BloomFilterUtil.Create<Guid>(100);
            Guid guid = Guid.NewGuid();
            filter.Add(guid);
            Assert.True(filter.MightContain(guid));
            Assert.False(filter.MightContain(Guid.NewGuid()));
        }

        [Fact]
        public void ObjectFilter_WorksCorrectly()
        {
            var filter = BloomFilterUtil.Create<Tuple<int, int>>(100);
            var tuple = Tuple.Create(1, 2);
            filter.Add(tuple);
            Assert.True(filter.MightContain(tuple));
            Assert.False(filter.MightContain(Tuple.Create(1, 3)));
        }

        #endregion

        #region 属性测试

        [Fact]
        public void BitSize_ReturnsCorrectSize()
        {
            int expectedSize = BloomFilterUtil.CalculateOptimalBitSize(1000, 0.01);
            var filter = BloomFilterUtil.Create<string>(1000, 0.01);
            Assert.Equal(expectedSize, filter.BitSize);
        }

        [Fact]
        public void HashCount_ReturnsCorrectCount()
        {
            int bitSize = BloomFilterUtil.CalculateOptimalBitSize(1000, 0.01);
            int expectedHashCount = BloomFilterUtil.CalculateOptimalHashCount(bitSize, 1000);
            var filter = BloomFilterUtil.Create<string>(1000, 0.01);
            Assert.Equal(expectedHashCount, filter.HashCount);
        }

        [Fact]
        public void CurrentFalsePositiveRate_EmptyFilter_ReturnsZero()
        {
            var filter = BloomFilterUtil.Create<string>(1000);
            Assert.Equal(0, filter.CurrentFalsePositiveProbability);
        }

        [Fact]
        public void CurrentFalsePositiveRate_HalfFullFilter_ReturnsPositiveRate()
        {
            var filter = BloomFilterUtil.Create<string>(1000);
            for (int i = 0; i < 500; i++)
            {
                filter.Add($"item{i}");
            }
            Assert.True(filter.CurrentFalsePositiveProbability > 0);
        }

        #endregion
    }
}
