using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyTool.QueueCategory.Tests
{
    public class PriorityQueueUtilTests
    {
        [Fact]
        public void CreateMin_ReturnsMinHeapQueue()
        {
            var queue = PriorityQueueUtil.CreateMin<int>();
            queue.Enqueue(5, 5);
            queue.Enqueue(1, 1);
            queue.Enqueue(3, 3);
            queue.TryDequeue(out var element, out _);
            Assert.Equal(1, element);
        }

        [Fact]
        public void CreateMin_WithCustomComparer_UsesComparer()
        {
            var queue = PriorityQueueUtil.CreateMin<string>(StringComparer.Ordinal);
            queue.Enqueue("banana", "banana");
            queue.Enqueue("apple", "apple");
            queue.TryDequeue(out var element, out _);
            Assert.Equal("apple", element);
        }

        [Fact]
        public void CreateMin_DefaultComparer_OrdersCorrectly()
        {
            var queue = PriorityQueueUtil.CreateMin<int>();
            var items = new[] { 10, 3, 7, 1, 9, 2, 5 };
            foreach (var item in items)
                queue.Enqueue(item, item);
            var sorted = new List<int>();
            while (queue.TryDequeue(out var element, out _))
                sorted.Add(element);
            Assert.Equal(items.OrderBy(x => x), sorted);
        }

        [Fact]
        public void CreateMax_ReturnsMaxHeapQueue()
        {
            var queue = PriorityQueueUtil.CreateMax<int>();
            queue.Enqueue(5, 5);
            queue.Enqueue(1, 1);
            queue.Enqueue(3, 3);
            queue.TryDequeue(out var element, out _);
            Assert.Equal(5, element);
        }

        [Fact]
        public void CreateMax_DefaultComparer_OrdersDescending()
        {
            var queue = PriorityQueueUtil.CreateMax<int>();
            var items = new[] { 10, 3, 7, 1, 9, 2, 5 };
            foreach (var item in items)
                queue.Enqueue(item, item);
            var sorted = new List<int>();
            while (queue.TryDequeue(out var element, out _))
                sorted.Add(element);
            Assert.Equal(items.OrderByDescending(x => x), sorted);
        }

        [Fact]
        public void CreateMax_WithCustomComparer_UsesComparer()
        {
            var queue = PriorityQueueUtil.CreateMax<string>(StringComparer.Ordinal);
            queue.Enqueue("apple", "apple");
            queue.Enqueue("zebra", "zebra");
            queue.TryDequeue(out var element, out _);
            Assert.Equal("zebra", element);
        }

        [Fact]
        public void FromCollection_CreatesQueueFromItems()
        {
            var items = new[] { "a", "b", "c" };
            var queue = PriorityQueueUtil.FromCollection(items, x => x.Length);
            Assert.Equal(3, queue.Count);
        }

        [Fact]
        public void FromCollection_WithPrioritySelector_AppliesPriority()
        {
            var items = new[] { 30, 10, 20 };
            var queue = PriorityQueueUtil.FromCollection(items, x => x);
            queue.TryDequeue(out var element, out _);
            Assert.Equal(10, element);
        }

        [Fact]
        public void FromCollection_EmptyCollection_ReturnsEmptyQueue()
        {
            var queue = PriorityQueueUtil.FromCollection(Array.Empty<int>(), x => x);
            Assert.Equal(0, queue.Count);
        }

        [Fact]
        public void EnqueueRange_AddsAllItems()
        {
            var queue = PriorityQueueUtil.CreateMin<int>();
            var items = new[] { 5, 3, 1, 4, 2 };
            queue.EnqueueRange(items, x => x);
            Assert.Equal(5, queue.Count);
        }

        [Fact]
        public void EnqueueRange_EmptyCollection_AddsNothing()
        {
            var queue = PriorityQueueUtil.CreateMin<int>();
            queue.Enqueue(1, 1);
            queue.EnqueueRange(Array.Empty<int>(), x => x);
            Assert.Equal(1, queue.Count);
        }

        [Fact]
        public void DequeueRange_DequeuesUpToCount()
        {
            var queue = PriorityQueueUtil.CreateMin<int>();
            for (int i = 1; i <= 10; i++)
                queue.Enqueue(i, i);
            var result = queue.DequeueRange(3);
            Assert.Equal(3, result.Count);
            Assert.Equal(7, queue.Count);
        }

        [Fact]
        public void DequeueRange_MoreThanAvailable_ReturnsAll()
        {
            var queue = PriorityQueueUtil.CreateMin<int>();
            queue.Enqueue(1, 1);
            queue.Enqueue(2, 2);
            var result = queue.DequeueRange(10);
            Assert.Equal(2, result.Count);
            Assert.Equal(0, queue.Count);
        }

        [Fact]
        public void DequeueRange_EmptyQueue_ReturnsEmpty()
        {
            var queue = PriorityQueueUtil.FromCollection(Array.Empty<int>(), x => x);
            var result = queue.DequeueRange(5);
            Assert.Empty(result);
        }

        [Fact]
        public void DequeueRange_ZeroCount_ReturnsEmpty()
        {
            var queue = PriorityQueueUtil.CreateMin<int>();
            queue.Enqueue(1, 1);
            var result = queue.DequeueRange(0);
            Assert.Empty(result);
            Assert.Equal(1, queue.Count);
        }

        [Fact]
        public void DequeueRange_MinHeap_ReturnsInOrder()
        {
            var queue = PriorityQueueUtil.CreateMin<int>();
            var items = new[] { 5, 3, 8, 1, 9, 2, 7 };
            foreach (var item in items)
                queue.Enqueue(item, item);
            var result = queue.DequeueRange(4);
            Assert.Equal(new[] { 1, 2, 3, 5 }, result);
        }

        [Fact]
        public void TryPeek_ReturnsElementWithoutRemoving()
        {
            var queue = PriorityQueueUtil.CreateMin<int>();
            queue.Enqueue(5, 5);
            queue.Enqueue(1, 1);
            queue.Enqueue(3, 3);
            Assert.True(queue.TryPeek(out var element, out var priority));
            Assert.Equal(1, element);
            Assert.Equal(1, priority);
            Assert.Equal(3, queue.Count);
        }

        [Fact]
        public void TryPeek_EmptyQueue_ReturnsFalse()
        {
            var queue = PriorityQueueUtil.FromCollection(Array.Empty<int>(), x => x);
            Assert.False(queue.TryPeek(out var element, out var priority));
            Assert.Equal(0, element);
            Assert.Equal(0, priority);
        }

        [Fact]
        public void TryPeek_CalledTwice_ReturnsSameElement()
        {
            var queue = PriorityQueueUtil.CreateMin<int>();
            queue.Enqueue(10, 10);
            queue.Enqueue(5, 5);
            Assert.True(queue.TryPeek(out var first, out _));
            Assert.True(queue.TryPeek(out var second, out _));
            Assert.Equal(first, second);
            Assert.Equal(2, queue.Count);
        }

        [Fact]
        public void ToSortedList_ReturnsAllElementsSorted()
        {
            var queue = PriorityQueueUtil.CreateMin<int>();
            var items = new[] { 5, 3, 8, 1, 9, 2, 7 };
            foreach (var item in items)
                queue.Enqueue(item, item);
            var sorted = queue.ToSortedList();
            Assert.Equal(7, sorted.Count);
            Assert.Equal(items.OrderBy(x => x), sorted.Select(x => x.Element));
            Assert.Equal(7, queue.Count);
        }

        [Fact]
        public void ToSortedList_EmptyQueue_ReturnsEmpty()
        {
            var queue = PriorityQueueUtil.FromCollection(Array.Empty<int>(), x => x);
            var sorted = queue.ToSortedList();
            Assert.Empty(sorted);
        }

        [Fact]
        public void ToSortedList_PreservesQueue()
        {
            var queue = PriorityQueueUtil.CreateMin<int>();
            queue.Enqueue(3, 3);
            queue.Enqueue(1, 1);
            queue.Enqueue(2, 2);
            _ = queue.ToSortedList();
            Assert.Equal(3, queue.Count);
            queue.TryDequeue(out var first, out _);
            Assert.Equal(1, first);
        }
    }

    public class ConcurrentPriorityQueueTests
    {
        [Fact]
        public void Constructor_Default_IsEmpty()
        {
            var queue = new ConcurrentPriorityQueue<int, int>();
            Assert.Equal(0, queue.Count);
            Assert.True(queue.IsEmpty);
        }

        [Fact]
        public void Enqueue_IncrementsCount()
        {
            var queue = new ConcurrentPriorityQueue<string, int>();
            queue.Enqueue("a", 1);
            queue.Enqueue("b", 2);
            Assert.Equal(2, queue.Count);
            Assert.False(queue.IsEmpty);
        }

        [Fact]
        public void TryDequeue_ReturnsMinPriorityFirst()
        {
            var queue = new ConcurrentPriorityQueue<int, int>();
            queue.Enqueue(10, 3);
            queue.Enqueue(5, 1);
            queue.Enqueue(7, 2);
            Assert.True(queue.TryDequeue(out var element, out var priority));
            Assert.Equal(5, element);
            Assert.Equal(1, priority);
            Assert.Equal(2, queue.Count);
        }

        [Fact]
        public void TryDequeue_EmptyQueue_ReturnsFalse()
        {
            var queue = new ConcurrentPriorityQueue<int, int>();
            Assert.False(queue.TryDequeue(out var element, out var priority));
            Assert.Equal(0, element);
            Assert.Equal(0, priority);
        }

        [Fact]
        public void TryPeek_ReturnsMinWithoutRemoving()
        {
            var queue = new ConcurrentPriorityQueue<int, int>();
            queue.Enqueue(10, 2);
            queue.Enqueue(5, 1);
            queue.Enqueue(15, 3);
            Assert.True(queue.TryPeek(out var element, out var priority));
            Assert.Equal(5, element);
            Assert.Equal(1, priority);
            Assert.Equal(3, queue.Count);
        }

        [Fact]
        public void TryPeek_EmptyQueue_ReturnsFalse()
        {
            var queue = new ConcurrentPriorityQueue<int, int>();
            Assert.False(queue.TryPeek(out _, out _));
        }

        [Fact]
        public void EnqueueRange_AddsMultipleItems()
        {
            var queue = new ConcurrentPriorityQueue<int, int>();
            var items = new (int Element, int Priority)[] { (1, 3), (2, 1), (3, 2) };
            queue.EnqueueRange(items);
            Assert.Equal(3, queue.Count);
        }

        [Fact]
        public void EnqueueRange_EmptyCollection_AddsNothing()
        {
            var queue = new ConcurrentPriorityQueue<int, int>();
            queue.Enqueue(1, 1);
            queue.EnqueueRange(Array.Empty<(int, int)>());
            Assert.Equal(1, queue.Count);
        }

        [Fact]
        public void DequeueRange_DequeuesUpToCount()
        {
            var queue = new ConcurrentPriorityQueue<int, int>();
            for (int i = 0; i < 10; i++)
                queue.Enqueue(i, i);
            var result = queue.DequeueRange(3);
            Assert.Equal(3, result.Count);
            Assert.Equal(7, queue.Count);
            Assert.Equal(0, result[0].Element);
            Assert.Equal(1, result[1].Element);
            Assert.Equal(2, result[2].Element);
        }

        [Fact]
        public void DequeueRange_MoreThanAvailable_ReturnsAll()
        {
            var queue = new ConcurrentPriorityQueue<int, int>();
            queue.Enqueue(1, 1);
            queue.Enqueue(2, 2);
            var result = queue.DequeueRange(10);
            Assert.Equal(2, result.Count);
            Assert.True(queue.IsEmpty);
        }

        [Fact]
        public void DequeueRange_EmptyQueue_ReturnsEmpty()
        {
            var queue = new ConcurrentPriorityQueue<int, int>();
            var result = queue.DequeueRange(5);
            Assert.Empty(result);
        }

        [Fact]
        public void Clear_RemovesAllItems()
        {
            var queue = new ConcurrentPriorityQueue<int, int>();
            queue.Enqueue(1, 1);
            queue.Enqueue(2, 2);
            queue.Enqueue(3, 3);
            queue.Clear();
            Assert.Equal(0, queue.Count);
            Assert.True(queue.IsEmpty);
        }

        [Fact]
        public void Clear_EmptyQueue_RemainsEmpty()
        {
            var queue = new ConcurrentPriorityQueue<int, int>();
            queue.Clear();
            Assert.Equal(0, queue.Count);
        }

        [Fact]
        public void ToArray_ReturnsAllElementsInPriorityOrder()
        {
            var queue = new ConcurrentPriorityQueue<int, int>();
            var items = new[] { 5, 3, 8, 1, 9 };
            foreach (var item in items)
                queue.Enqueue(item, item);
            var array = queue.ToArray();
            Assert.Equal(5, array.Length);
            Assert.Equal(items.OrderBy(x => x), array.Select(x => x.Element));
            Assert.Equal(5, queue.Count);
        }

        [Fact]
        public void ToArray_EmptyQueue_ReturnsEmptyArray()
        {
            var queue = new ConcurrentPriorityQueue<int, int>();
            var array = queue.ToArray();
            Assert.Empty(array);
        }

        [Fact]
        public async Task ConcurrentOperations_DoNotCorruptState()
        {
            var queue = new ConcurrentPriorityQueue<int, int>();
            const int itemCount = 1000;
            var producerTask = Task.Run(() =>
            {
                for (int i = 0; i < itemCount; i++)
                    queue.Enqueue(i, i);
            });
            var consumed = new List<int>();
            var consumerTask = Task.Run(async () =>
            {
                while (consumed.Count < itemCount)
                {
                    if (queue.TryDequeue(out var element, out _))
                        consumed.Add(element);
                    else
                        await Task.Delay(1);
                }
            });
            await Task.WhenAll(producerTask, consumerTask);
            Assert.Equal(itemCount, consumed.Count);
            Assert.Equal(Enumerable.Range(0, itemCount), consumed.OrderBy(x => x));
        }

        [Fact]
        public void Constructor_WithComparer_UsesCustomOrdering()
        {
            var queue = new ConcurrentPriorityQueue<string, int>(Comparer<int>.Create((a, b) => b.CompareTo(a)));
            queue.Enqueue("low", 1);
            queue.Enqueue("mid", 5);
            queue.Enqueue("high", 10);
            queue.TryDequeue(out var element, out _);
            Assert.Equal("high", element);
        }

        [Fact]
        public void FullLifecycle_EnqueueDequeueClear_WorksCorrectly()
        {
            var queue = new ConcurrentPriorityQueue<string, int>();
            queue.Enqueue("first", 2);
            queue.Enqueue("second", 1);
            Assert.Equal(2, queue.Count);
            queue.TryDequeue(out var element, out _);
            Assert.Equal("second", element);
            Assert.Equal(1, queue.Count);
            queue.Enqueue("third", 0);
            Assert.Equal(2, queue.Count);
            queue.TryDequeue(out _, out _);
            queue.TryDequeue(out _, out _);
            Assert.True(queue.IsEmpty);
            queue.Enqueue("fourth", 1);
            Assert.Equal(1, queue.Count);
            queue.Clear();
            Assert.True(queue.IsEmpty);
        }
    }
}
