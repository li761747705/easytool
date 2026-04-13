using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EasyTool.CacheCategory;
using Xunit;

// 解决命名冲突：根命名空间 EasyTool 也有 CacheOptions 类
using CacheOpts = EasyTool.CacheCategory.CacheOptions;

namespace EasyTool.UnitTests.CacheCategory
{
    /// <summary>
    /// MemoryCacheProvider 测试类
    /// </summary>
    public class MemoryCacheProviderTests
    {
        #region Set/Get 测试

        [Fact]
        public void Set_ValidKeyAndValue_StoresValue()
        {
            using var cache = new MemoryCacheProvider();
            cache.Set("key1", "value1");

            var result = cache.Get<string>("key1");
            Assert.Equal("value1", result);
        }

        [Fact]
        public void Set_NullKey_ThrowsArgumentNullException()
        {
            using var cache = new MemoryCacheProvider();
            Assert.Throws<ArgumentNullException>(() => cache.Set<string>(null, "value"));
        }

        [Fact]
        public void Set_EmptyKey_ThrowsArgumentNullException()
        {
            using var cache = new MemoryCacheProvider();
            Assert.Throws<ArgumentNullException>(() => cache.Set<string>("", "value"));
        }

        [Fact]
        public void Get_NonExistentKey_ReturnsDefault()
        {
            using var cache = new MemoryCacheProvider();
            var result = cache.Get<string>("nonexistent");
            Assert.Null(result);
        }

        [Fact]
        public void Get_NullKey_ReturnsDefault()
        {
            using var cache = new MemoryCacheProvider();
            var result = cache.Get<string>(null);
            Assert.Null(result);
        }

        [Theory]
        [InlineData("key1", "value1")]
        [InlineData("key2", 123)]
        [InlineData("key3", true)]
        public void Set_VariousTypes_StoresCorrectly<T>(string key, T value)
        {
            using var cache = new MemoryCacheProvider();
            cache.Set(key, value);

            var result = cache.Get<T>(key);
            Assert.Equal(value, result);
        }

        #endregion

        #region Async 方法测试

        [Fact]
        public async Task SetAsync_ValidKeyAndValue_StoresValue()
        {
            using var cache = new MemoryCacheProvider();
            await cache.SetAsync("asyncKey", "asyncValue");

            var result = await cache.GetAsync<string>("asyncKey");
            Assert.Equal("asyncValue", result);
        }

        [Fact]
        public async Task GetAsync_NonExistentKey_ReturnsDefault()
        {
            using var cache = new MemoryCacheProvider();
            var result = await cache.GetAsync<string>("nonexistent");
            Assert.Null(result);
        }

        #endregion

        #region GetOrAdd 测试

        [Fact]
        public void GetOrAdd_NonExistentKey_AddsValue()
        {
            using var cache = new MemoryCacheProvider();
            var result = cache.GetOrAdd("key1", () => "value1");

            Assert.Equal("value1", result);
            Assert.Equal("value1", cache.Get<string>("key1"));
        }

        [Fact]
        public void GetOrAdd_ExistingKey_ReturnsExistingValue()
        {
            using var cache = new MemoryCacheProvider();
            cache.Set("key1", "existing");

            var result = cache.GetOrAdd("key1", () => "newvalue");

            Assert.Equal("existing", result);
        }

        [Fact]
        public async Task GetOrAddAsync_NonExistentKey_AddsValue()
        {
            using var cache = new MemoryCacheProvider();
            var result = await cache.GetOrAddAsync("asyncKey", () => Task.FromResult("asyncValue"));

            Assert.Equal("asyncValue", result);
        }

        #endregion

        #region Exists 测试

        [Fact]
        public void Exists_ExistingKey_ReturnsTrue()
        {
            using var cache = new MemoryCacheProvider();
            cache.Set("key1", "value1");

            Assert.True(cache.Exists("key1"));
        }

        [Fact]
        public void Exists_NonExistentKey_ReturnsFalse()
        {
            using var cache = new MemoryCacheProvider();
            Assert.False(cache.Exists("nonexistent"));
        }

        [Fact]
        public void Exists_NullKey_ReturnsFalse()
        {
            using var cache = new MemoryCacheProvider();
            Assert.False(cache.Exists(null));
        }

        [Fact]
        public async Task ExistsAsync_ExistingKey_ReturnsTrue()
        {
            using var cache = new MemoryCacheProvider();
            cache.Set("key1", "value1");

            Assert.True(await cache.ExistsAsync("key1"));
        }

        #endregion

        #region Remove 测试

        [Fact]
        public void Remove_ExistingKey_RemovesValue()
        {
            using var cache = new MemoryCacheProvider();
            cache.Set("key1", "value1");
            cache.Remove("key1");

            Assert.False(cache.Exists("key1"));
        }

        [Fact]
        public void Remove_NonExistentKey_NoException()
        {
            using var cache = new MemoryCacheProvider();
            cache.Remove("nonexistent"); // 不应抛出异常
        }

        [Fact]
        public void Remove_MultipleKeys_RemovesAll()
        {
            using var cache = new MemoryCacheProvider();
            cache.Set("key1", "value1");
            cache.Set("key2", "value2");
            cache.Set("key3", "value3");

            cache.Remove(new[] { "key1", "key2" });

            Assert.False(cache.Exists("key1"));
            Assert.False(cache.Exists("key2"));
            Assert.True(cache.Exists("key3"));
        }

        #endregion

        #region Clear 测试

        [Fact]
        public void Clear_WithValues_RemovesAll()
        {
            using var cache = new MemoryCacheProvider();
            cache.Set("key1", "value1");
            cache.Set("key2", "value2");
            cache.Clear();

            Assert.Equal(0, cache.Count());
        }

        [Fact]
        public async Task ClearAsync_WithValues_RemovesAll()
        {
            using var cache = new MemoryCacheProvider();
            cache.Set("key1", "value1");
            cache.Set("key2", "value2");
            await cache.ClearAsync();

            Assert.Equal(0, cache.Count());
        }

        #endregion

        #region Count 测试

        [Fact]
        public void Count_EmptyCache_ReturnsZero()
        {
            using var cache = new MemoryCacheProvider();
            Assert.Equal(0, cache.Count());
        }

        [Fact]
        public void Count_WithValues_ReturnsCorrectCount()
        {
            using var cache = new MemoryCacheProvider();
            cache.Set("key1", "value1");
            cache.Set("key2", "value2");
            cache.Set("key3", "value3");

            Assert.Equal(3, cache.Count());
        }

        #endregion

        #region 过期策略测试

        [Fact]
        public void Set_WithAbsoluteExpiration_ExpiresCorrectly()
        {
            using var cache = new MemoryCacheProvider();
            var options = new CacheOpts
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMilliseconds(100)
            };

            cache.Set("key1", "value1", options);
            Assert.True(cache.Exists("key1"));

            // 等待过期
            Thread.Sleep(200);
            Assert.False(cache.Exists("key1"));
        }

        [Fact]
        public void Set_WithSlidingExpiration_ExtendsOnAccess()
        {
            using var cache = new MemoryCacheProvider(TimeSpan.FromMilliseconds(50));
            var options = new CacheOpts
            {
                SlidingExpiration = TimeSpan.FromMilliseconds(100)
            };

            cache.Set("key1", "value1", options);

            // 访问几次，延长过期
            for (int i = 0; i < 3; i++)
            {
                Thread.Sleep(50);
                Assert.True(cache.Exists("key1"));
                cache.Get<string>("key1");
            }
        }

        #endregion

        #region SetExpiration 测试

        [Fact]
        public void SetExpiration_ExistingKey_ReturnsTrue()
        {
            using var cache = new MemoryCacheProvider();
            cache.Set("key1", "value1");

            var result = cache.SetExpiration("key1", TimeSpan.FromMinutes(5));
            Assert.True(result);
        }

        [Fact]
        public void SetExpiration_NonExistentKey_ReturnsFalse()
        {
            using var cache = new MemoryCacheProvider();
            var result = cache.SetExpiration("nonexistent", TimeSpan.FromMinutes(5));
            Assert.False(result);
        }

        #endregion

        #region CacheOpts 测试

        [Fact]
        public void CacheOpts_FromExpiration_CreatesCorrectOptions()
        {
            var expiration = TimeSpan.FromMinutes(10);
            var options = CacheOpts.FromExpiration(expiration);

            Assert.Equal(expiration, options.AbsoluteExpirationRelativeToNow);
        }

        [Fact]
        public void CacheOpts_FromSlidingExpiration_CreatesCorrectOptions()
        {
            var sliding = TimeSpan.FromMinutes(5);
            var options = CacheOpts.FromSlidingExpiration(sliding);

            Assert.Equal(sliding, options.SlidingExpiration);
        }

        [Fact]
        public void CacheOpts_FromAbsoluteExpiration_CreatesCorrectOptions()
        {
            var absolute = DateTime.UtcNow.AddHours(1);
            var options = CacheOpts.FromAbsoluteExpiration(absolute);

            Assert.Equal(absolute, options.AbsoluteExpiration);
        }

        #endregion

        #region CachePriority 测试

        [Fact]
        public void CachePriority_ValuesAreCorrect()
        {
            Assert.Equal(0, (int)CachePriority.Low);
            Assert.Equal(1, (int)CachePriority.Normal);
            Assert.Equal(2, (int)CachePriority.High);
            Assert.Equal(3, (int)CachePriority.NeverRemove);
        }

        [Fact]
        public void Set_WithHighPriority_StoresCorrectly()
        {
            using var cache = new MemoryCacheProvider();
            var options = new CacheOpts { Priority = CachePriority.High };
            cache.Set("key1", "value1", options);

            Assert.True(cache.Exists("key1"));
        }

        #endregion

        #region GetStatistics 测试

        [Fact]
        public void GetStatistics_EmptyCache_ReturnsZeroCounts()
        {
            using var cache = new MemoryCacheProvider();
            var stats = cache.GetStatistics();

            Assert.Equal(0, stats.TotalCount);
            Assert.Equal(0, stats.ExpiredCount);
        }

        [Fact]
        public void GetStatistics_WithValues_ReturnsCorrectCounts()
        {
            using var cache = new MemoryCacheProvider();
            cache.Set("key1", "value1");
            cache.Set("key2", "value2", new CacheOpts { Priority = CachePriority.High });

            var stats = cache.GetStatistics();

            Assert.Equal(2, stats.TotalCount);
            Assert.Equal(1, stats.HighPriorityCount);
        }

        #endregion

        #region GetKeys 测试

        [Fact]
        public void GetKeys_WithValues_ReturnsAllKeys()
        {
            using var cache = new MemoryCacheProvider();
            cache.Set("key1", "value1");
            cache.Set("key2", "value2");

            var keys = cache.GetKeys();

            Assert.Contains("key1", keys);
            Assert.Contains("key2", keys);
        }

        #endregion

        #region Dispose 测试

        [Fact]
        public void Dispose_MultipleCalls_NoException()
        {
            var cache = new MemoryCacheProvider();
            cache.Dispose();
            cache.Dispose(); // 第二次不应抛出异常
        }

        #endregion

        #region KeyPrefix 测试

        [Fact]
        public void Set_WithKeyPrefix_StoresWithPrefix()
        {
            using var cache = new MemoryCacheProvider();
            var options = new CacheOpts { KeyPrefix = "myapp" };
            cache.Set("key1", "value1", options);

            // 验证实际存储的键
            var keys = cache.GetKeys();
            Assert.Contains("myapp:key1", keys);
        }

        #endregion
    }
}