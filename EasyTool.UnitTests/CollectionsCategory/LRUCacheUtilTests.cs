using Xunit;
using EasyTool.CollectionsCategory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyTool.UnitTests.CollectionsCategory
{
    public class LRUCacheUtilTests
    {
        #region 创建测试

        [Fact]
        public void Create_ValidCapacity_ReturnsCache()
        {
            var cache = LRUCacheUtil.Create<int, string>(10);
            Assert.NotNull(cache);
            Assert.Equal(10, cache.Capacity);
            Assert.Equal(0, cache.Count);
        }

        [Fact]
        public void Constructor_ZeroCapacity_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new LRUCache<int, string>(0));
        }

        [Fact]
        public void Constructor_NegativeCapacity_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new LRUCache<int, string>(-10));
        }

        #endregion

        #region 基本操作测试

        [Fact]
        public void Put_AddItem_IncreasesCount()
        {
            var cache = new LRUCache<int, string>(3);
            cache.Put(1, "one");
            Assert.Equal(1, cache.Count);
        }

        [Fact]
        public void Put_UpdateExistingItem_KeepsCountSame()
        {
            var cache = new LRUCache<int, string>(3);
            cache.Put(1, "one");
            cache.Put(1, "ONE");
            Assert.Equal(1, cache.Count);
        }

        [Fact]
        public void Get_ExistingItem_ReturnsValue()
        {
            var cache = new LRUCache<int, string>(3);
            cache.Put(1, "one");
            string value = cache.Get(1);
            Assert.Equal("one", value);
        }

        [Fact]
        public void Get_NonExistentItem_ThrowsKeyNotFoundException()
        {
            var cache = new LRUCache<int, string>(3);
            Assert.Throws<KeyNotFoundException>(() => cache.Get(1));
        }

        [Fact]
        public void TryGet_ExistingItem_ReturnsTrue()
        {
            var cache = new LRUCache<int, string>(3);
            cache.Put(1, "one");
            bool result = cache.TryGet(1, out string value);
            Assert.True(result);
            Assert.Equal("one", value);
        }

        [Fact]
        public void TryGet_NonExistentItem_ReturnsFalse()
        {
            var cache = new LRUCache<int, string>(3);
            bool result = cache.TryGet(1, out string value);
            Assert.False(result);
            Assert.Null(value);
        }

        [Fact]
        public void Remove_ExistingItem_ReturnsTrue()
        {
            var cache = new LRUCache<int, string>(3);
            cache.Put(1, "one");
            bool removed = cache.Remove(1);
            Assert.True(removed);
            Assert.Equal(0, cache.Count);
        }

        [Fact]
        public void Remove_NonExistentItem_ReturnsFalse()
        {
            var cache = new LRUCache<int, string>(3);
            bool removed = cache.Remove(1);
            Assert.False(removed);
        }

        [Fact]
        public void ContainsKey_ExistingKey_ReturnsTrue()
        {
            var cache = new LRUCache<int, string>(3);
            cache.Put(1, "one");
            Assert.True(cache.ContainsKey(1));
        }

        [Fact]
        public void ContainsKey_NonExistentKey_ReturnsFalse()
        {
            var cache = new LRUCache<int, string>(3);
            Assert.False(cache.ContainsKey(1));
        }

        #endregion

        #region LRU淘汰测试

        [Fact]
        public void Put_ExceedsCapacity_EvictsLeastRecentlyUsed()
        {
            var cache = new LRUCache<int, string>(3);
            cache.Put(1, "one");
            cache.Put(2, "two");
            cache.Put(3, "three");

            // 访问1使其成为最近使用
            cache.Get(1);

            // 添加第4个项目，应该淘汰2（最久未使用）
            cache.Put(4, "four");

            Assert.True(cache.ContainsKey(1));
            Assert.False(cache.ContainsKey(2));
            Assert.True(cache.ContainsKey(3));
            Assert.True(cache.ContainsKey(4));
        }

        [Fact]
        public void Put_ExceedsCapacity_EvictsInOrder()
        {
            var cache = new LRUCache<int, string>(3);
            cache.Put(1, "one");
            cache.Put(2, "two");
            cache.Put(3, "three");
            cache.Put(4, "four");

            // 应该淘汰1
            Assert.False(cache.ContainsKey(1));
            Assert.True(cache.ContainsKey(2));
            Assert.True(cache.ContainsKey(3));
            Assert.True(cache.ContainsKey(4));
        }

        [Fact]
        public void Get_UpdatesAccessOrder()
        {
            var cache = new LRUCache<int, string>(3);
            cache.Put(1, "one");
            cache.Put(2, "two");
            cache.Put(3, "three");

            // 访问1，使其成为最近使用
            cache.Get(1);

            // 访问2，使其成为最近使用，1变成第二
            cache.Get(2);

            // 添加4，应该淘汰3
            cache.Put(4, "four");

            Assert.True(cache.ContainsKey(1));
            Assert.True(cache.ContainsKey(2));
            Assert.False(cache.ContainsKey(3));
            Assert.True(cache.ContainsKey(4));
        }

        [Fact]
        public void TryGet_UpdatesAccessOrder()
        {
            var cache = new LRUCache<int, string>(3);
            cache.Put(1, "one");
            cache.Put(2, "two");
            cache.Put(3, "three");

            // 使用TryGet访问1
            cache.TryGet(1, out _);

            // 添加4，应该淘汰2
            cache.Put(4, "four");

            Assert.True(cache.ContainsKey(1));
            Assert.False(cache.ContainsKey(2));
            Assert.True(cache.ContainsKey(3));
            Assert.True(cache.ContainsKey(4));
        }

        [Fact]
        public void Put_UpdateExisting_MovesToFront()
        {
            var cache = new LRUCache<int, string>(3);
            cache.Put(1, "one");
            cache.Put(2, "two");
            cache.Put(3, "three");

            // 更新1，使其成为最近使用
            cache.Put(1, "ONE");

            // 添加4，应该淘汰2
            cache.Put(4, "four");

            Assert.True(cache.ContainsKey(1));
            Assert.False(cache.ContainsKey(2));
            Assert.True(cache.ContainsKey(3));
            Assert.True(cache.ContainsKey(4));
        }

        #endregion

        #region GetOrAdd测试

        [Fact]
        public void GetOrAdd_ExistingKey_ReturnsExistingValue()
        {
            var cache = new LRUCache<int, string>(3);
            cache.Put(1, "one");
            string value = cache.GetOrAdd(1, k => k.ToString());
            Assert.Equal("one", value);
            Assert.Equal(1, cache.Count);
        }

        [Fact]
        public void GetOrAdd_NonExistentKey_AddsAndReturnsNewValue()
        {
            var cache = new LRUCache<int, string>(3);
            string value = cache.GetOrAdd(1, k => k.ToString());
            Assert.Equal("1", value);
            Assert.Equal(1, cache.Count);
        }

        [Fact]
        public void GetOrAdd_NullFactory_ThrowsArgumentNullException()
        {
            var cache = new LRUCache<int, string>(3);
            Assert.Throws<ArgumentNullException>(() =>
                cache.GetOrAdd(1, null!));
        }

        [Fact]
        public void GetOrAdd_UpdatesAccessOrder()
        {
            var cache = new LRUCache<int, string>(3);
            cache.Put(1, "one");
            cache.Put(2, "two");
            cache.Put(3, "three");

            // 使用GetOrAdd访问1
            cache.GetOrAdd(1, k => k.ToString());

            // 添加4，应该淘汰2
            cache.Put(4, "four");

            Assert.True(cache.ContainsKey(1));
            Assert.False(cache.ContainsKey(2));
        }

        #endregion

        #region 索引器测试

        [Fact]
        public void Indexer_Get_ReturnsValue()
        {
            var cache = new LRUCache<int, string>(3);
            cache.Put(1, "one");
            string value = cache[1];
            Assert.Equal("one", value);
        }

        [Fact]
        public void Indexer_Get_NonExistentKey_ThrowsKeyNotFoundException()
        {
            var cache = new LRUCache<int, string>(3);
            Assert.Throws<KeyNotFoundException>(() =>
            {
                string value = cache[1];
            });
        }

        [Fact]
        public void Indexer_Set_AddsValue()
        {
            var cache = new LRUCache<int, string>(3);
            cache[1] = "one";
            Assert.Equal(1, cache.Count);
            Assert.Equal("one", cache[1]);
        }

        [Fact]
        public void Indexer_Set_UpdatesValue()
        {
            var cache = new LRUCache<int, string>(3);
            cache.Put(1, "one");
            cache[1] = "ONE";
            Assert.Equal("ONE", cache[1]);
            Assert.Equal(1, cache.Count);
        }

        #endregion

        #region 清空测试

        [Fact]
        public void Clear_RemovesAllItems()
        {
            var cache = new LRUCache<int, string>(3);
            cache.Put(1, "one");
            cache.Put(2, "two");
            cache.Clear();

            Assert.Equal(0, cache.Count);
            Assert.False(cache.ContainsKey(1));
            Assert.False(cache.ContainsKey(2));
        }

        [Fact]
        public void Clear_ResetsStatistics()
        {
            var cache = new LRUCache<int, string>(3);
            cache.Put(1, "one");
            cache.Put(2, "two");
            cache.Get(1); // 命中
            cache.TryGet(3, out _); // 未命中

            cache.Clear();

            // 清空后统计应该重置
            Assert.Equal(0, cache.HitRate);
        }

        [Fact]
        public void Clear_CanAddAfterClear()
        {
            var cache = new LRUCache<int, string>(3);
            cache.Put(1, "one");
            cache.Clear();
            cache.Put(2, "two");

            Assert.Equal(1, cache.Count);
            Assert.Equal("two", cache.Get(2));
        }

        #endregion

        #region 统计测试

        [Fact]
        public void HitRate_NoRequests_ReturnsZero()
        {
            var cache = new LRUCache<int, string>(3);
            Assert.Equal(0, cache.HitRate);
        }

        [Fact]
        public void HitRate_AllHits_ReturnsOne()
        {
            var cache = new LRUCache<int, string>(3);
            cache.Put(1, "one");
            cache.Put(2, "two");
            cache.Get(1);
            cache.Get(2);

            Assert.Equal(1.0, cache.HitRate);
        }

        [Fact]
        public void HitRate_AllMisses_ReturnsZero()
        {
            var cache = new LRUCache<int, string>(3);
            cache.Put(1, "one");
            Assert.Throws<KeyNotFoundException>(() => cache.Get(2));
            Assert.Throws<KeyNotFoundException>(() => cache.Get(3));

            Assert.Equal(0.0, cache.HitRate);
        }

        [Fact]
        public void HitRate_MixedHitsAndMisses_ReturnsCorrectRate()
        {
            var cache = new LRUCache<int, string>(3);
            cache.Put(1, "one");
            cache.Put(2, "two");

            // 2次命中
            cache.Get(1);
            cache.Get(2);

            // 2次未命中
            try { cache.Get(3); } catch { }
            try { cache.Get(4); } catch { }

            Assert.Equal(0.5, cache.HitRate);
        }

        [Fact]
        public void ResetStatistics_ResetsCounters()
        {
            var cache = new LRUCache<int, string>(3);
            cache.Put(1, "one");
            cache.Get(1);

            cache.ResetStatistics();

            Assert.Equal(0, cache.HitRate);
        }

        [Fact]
        public void TryGet_CountsAsRequest()
        {
            var cache = new LRUCache<int, string>(3);
            cache.Put(1, "one");

            cache.TryGet(1, out _); // 命中
            cache.TryGet(2, out _); // 未命中

            Assert.Equal(0.5, cache.HitRate);
        }

        [Fact]
        public void GetOrAdd_CountsAsRequest()
        {
            var cache = new LRUCache<int, string>(3);
            cache.Put(1, "one");

            cache.GetOrAdd(1, k => k.ToString()); // 命中

            Assert.Equal(1.0, cache.HitRate);
        }

        #endregion

        #region 枚举测试

        [Fact]
        public void GetKeys_ReturnsAllKeys()
        {
            var cache = new LRUCache<int, string>(10);
            cache.Put(1, "one");
            cache.Put(2, "two");
            cache.Put(3, "three");

            var keys = cache.GetKeys().ToList();
            Assert.Equal(3, keys.Count);
            Assert.Contains(1, keys);
            Assert.Contains(2, keys);
            Assert.Contains(3, keys);
        }

        [Fact]
        public void GetKeys_ReturnsInLRUOrder()
        {
            var cache = new LRUCache<int, string>(10);
            cache.Put(1, "one");
            cache.Put(2, "two");
            cache.Put(3, "three");

            // 访问1使其成为最近
            cache.Get(1);

            var keys = cache.GetKeys().ToList();
            // GetKeys returns from most recent (First) to least recent (Last)
            // After Get(1), order is: 1(most recent), 3, 2(least recent)
            Assert.Equal(new[] { 1, 3, 2 }, keys);
        }

        [Fact]
        public void GetValues_ReturnsAllValues()
        {
            var cache = new LRUCache<int, string>(10);
            cache.Put(1, "one");
            cache.Put(2, "two");
            cache.Put(3, "three");

            var values = cache.GetValues().ToList();
            Assert.Equal(3, values.Count);
            Assert.Contains("one", values);
            Assert.Contains("two", values);
            Assert.Contains("three", values);
        }

        #endregion

        #region 线程安全测试

        [Fact]
        public void ConcurrentPut_ThreadSafe()
        {
            var cache = new LRUCache<int, int>(1000);
            int itemCount = 100;
            var tasks = new List<Task>();

            for (int i = 0; i < 10; i++)
            {
                int start = i * itemCount;
                var task = Task.Run(() =>
                {
                    for (int j = 0; j < itemCount; j++)
                    {
                        cache.Put(start + j, start + j);
                    }
                });
                tasks.Add(task);
            }

            Task.WaitAll(tasks.ToArray());
            Assert.Equal(1000, cache.Count);
        }

        [Fact]
        public void ConcurrentGet_ThreadSafe()
        {
            var cache = new LRUCache<int, int>(1000);

            // 先添加一些项目
            for (int i = 0; i < 100; i++)
            {
                cache.Put(i, i);
            }

            int successCount = 0;
            var tasks = new List<Task>();

            // 并发读取
            for (int i = 0; i < 10; i++)
            {
                var task = Task.Run(() =>
                {
                    for (int j = 0; j < 100; j++)
                    {
                        if (cache.TryGet(j, out int value))
                        {
                            global::System.Threading.Interlocked.Increment(ref successCount);
                        }
                    }
                });
                tasks.Add(task);
            }

            Task.WaitAll(tasks.ToArray());
            Assert.Equal(1000, successCount);
        }

        [Fact]
        public void ConcurrentPutAndGet_ThreadSafe()
        {
            var cache = new LRUCache<int, int>(1000);
            var tasks = new List<Task>();

            // 并发写入
            for (int i = 0; i < 5; i++)
            {
                var task = Task.Run(() =>
                {
                    for (int j = 0; j < 200; j++)
                    {
                        cache.Put(j, j);
                    }
                });
                tasks.Add(task);
            }

            // 并发读取
            for (int i = 0; i < 5; i++)
            {
                var task = Task.Run(() =>
                {
                    for (int j = 0; j < 200; j++)
                    {
                        cache.TryGet(j, out int _);
                    }
                });
                tasks.Add(task);
            }

            Task.WaitAll(tasks.ToArray());
            // 缓存应该有数据，具体数量取决于LRU淘汰
            Assert.True(cache.Count > 0);
        }

        #endregion

        #region 边界测试

        [Fact]
        public void CapacityOne_WorksCorrectly()
        {
            var cache = new LRUCache<int, string>(1);
            cache.Put(1, "one");
            cache.Put(2, "two");

            Assert.Equal(1, cache.Count);
            Assert.False(cache.ContainsKey(1));
            Assert.True(cache.ContainsKey(2));
        }

        [Fact]
        public void LargeCapacity_WorksCorrectly()
        {
            var cache = new LRUCache<int, int>(10000);
            for (int i = 0; i < 10000; i++)
            {
                cache.Put(i, i);
            }
            Assert.Equal(10000, cache.Count);
        }

        [Fact]
        public void Remove_DuringIteration_WorksCorrectly()
        {
            var cache = new LRUCache<int, string>(10);
            cache.Put(1, "one");
            cache.Put(2, "two");
            cache.Put(3, "three");

            cache.Remove(2);

            var keys = cache.GetKeys().ToList();
            Assert.Equal(2, keys.Count);
            Assert.DoesNotContain(2, keys);
        }

        #endregion
    }
}
