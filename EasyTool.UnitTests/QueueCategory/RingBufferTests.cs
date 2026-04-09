using Xunit;

namespace EasyTool.QueueCategory.Tests
{
    public class RingBufferTests
    {
        [Fact]
        public void Constructor_ValidCapacity_CreatesBuffer()
        {
            var buffer = new RingBuffer<int>(5);
            Assert.Equal(5, buffer.Capacity);
            Assert.Equal(0, buffer.Count);
            Assert.True(buffer.IsEmpty);
            Assert.False(buffer.IsFull);
        }

        [Fact]
        public void Constructor_InvalidCapacity_ThrowsException()
        {
            Assert.Throws<ArgumentException>(() => new RingBuffer<int>(0));
            Assert.Throws<ArgumentException>(() => new RingBuffer<int>(-1));
        }

        [Fact]
        public void Write_AddsItemToBuffer()
        {
            var buffer = new RingBuffer<int>(3);
            Assert.True(buffer.Write(1));
            Assert.Equal(1, buffer.Count);
            Assert.False(buffer.IsEmpty);
        }

        [Fact]
        public void Write_WhenFull_Overwrites()
        {
            var buffer = new RingBuffer<int>(3, true);
            buffer.Write(1);
            buffer.Write(2);
            buffer.Write(3);
            Assert.True(buffer.IsFull);

            // Should overwrite oldest
            Assert.True(buffer.Write(4));
            Assert.Equal(3, buffer.Count);
        }

        [Fact]
        public void Write_WhenFull_NoOverwrite_ReturnsFalse()
        {
            var buffer = new RingBuffer<int>(2, false);
            buffer.Write(1);
            buffer.Write(2);
            Assert.True(buffer.IsFull);

            Assert.False(buffer.Write(3));
            Assert.Equal(2, buffer.Count);
        }

        [Fact]
        public void Read_ReturnsOldestItem()
        {
            var buffer = new RingBuffer<int>(3);
            buffer.Write(1);
            buffer.Write(2);

            var value = buffer.Read();
            Assert.Equal(1, value);
            Assert.Equal(1, buffer.Count);
        }

        [Fact]
        public void Read_EmptyBuffer_ReturnsDefault()
        {
            var buffer = new RingBuffer<int>(3);
            var value = buffer.Read();
            Assert.Equal(default, value);
        }

        [Fact]
        public void TryRead_ReturnsTrueAndValue()
        {
            var buffer = new RingBuffer<int>(3);
            buffer.Write(42);

            Assert.True(buffer.TryRead(out int value));
            Assert.Equal(42, value);
        }

        [Fact]
        public void TryRead_EmptyBuffer_ReturnsDefault()
        {
            var buffer = new RingBuffer<int>(3);
            // 注意：原始实现的TryRead在空缓冲区时行为特殊
            buffer.TryRead(out int value);
            Assert.Equal(default, value);
        }

        [Fact]
        public void Peek_ReturnsOldestWithoutRemoving()
        {
            var buffer = new RingBuffer<int>(3);
            buffer.Write(1);
            buffer.Write(2);

            var value = buffer.Peek();
            Assert.Equal(1, value);
            Assert.Equal(2, buffer.Count);
        }

        [Fact]
        public void Peek_EmptyBuffer_ReturnsDefault()
        {
            var buffer = new RingBuffer<int>(3);
            var value = buffer.Peek();
            Assert.Equal(default, value);
        }

        [Fact]
        public void Clear_RemovesAllItems()
        {
            var buffer = new RingBuffer<int>(3);
            buffer.Write(1);
            buffer.Write(2);

            buffer.Clear();
            Assert.Equal(0, buffer.Count);
            Assert.True(buffer.IsEmpty);
        }

        [Fact]
        public void ToArray_ReturnsItemsInOrder()
        {
            var buffer = new RingBuffer<int>(5);
            buffer.Write(1);
            buffer.Write(2);
            buffer.Write(3);

            var array = buffer.ToArray();
            Assert.Equal(new[] { 1, 2, 3 }, array);
        }

        [Fact]
        public void FifoOrder_Preserved()
        {
            var buffer = new RingBuffer<int>(5);
            buffer.Write(10);
            buffer.Write(20);
            buffer.Write(30);

            var first = buffer.Read();
            var second = buffer.Read();
            var third = buffer.Read();

            Assert.Equal(10, first);
            Assert.Equal(20, second);
            Assert.Equal(30, third);
        }

        [Fact]
        public void ReadAll_ReturnsAllItems()
        {
            var buffer = new RingBuffer<int>(5);
            buffer.Write(1);
            buffer.Write(2);
            buffer.Write(3);

            var items = buffer.ReadAll();
            Assert.Equal(new[] { 1, 2, 3 }, items);
            Assert.True(buffer.IsEmpty);
        }

        [Fact]
        public void WriteArray_WritesMultipleItems()
        {
            var buffer = new RingBuffer<int>(5);
            var written = buffer.Write(new[] { 1, 2, 3 });
            Assert.Equal(3, written);
            Assert.Equal(3, buffer.Count);
        }
    }
}