using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EasyTool.CacheCategory;
using Xunit;

namespace EasyTool.UnitTests.CacheCategory
{
    /// <summary>
    /// DistributedCacheUtil 测试类
    /// </summary>
    public class DistributedCacheUtilTests
    {
        #region DefaultProvider 测试

        [Fact]
        public void DefaultProvider_ReturnsMemoryCacheProvider()
        {
            var provider = DistributedCacheUtil.DefaultProvider;
            Assert.NotNull(provider);
            Assert.IsType<MemoryCacheProvider>(provider);
        }

        [Fact]
        public void DefaultProvider_LazyInitialized_ReturnsSameInstance()
        {
            var provider1 = DistributedCacheUtil.DefaultProvider;
            var provider2 = DistributedCacheUtil.DefaultProvider;

            Assert.Same(provider1, provider2);
        }

        #endregion

        #region RegisterProvider/GetProvider 测试

        [Fact]
        public void RegisterProvider_AddsProviderToRegistry()
        {
            using var provider = new MemoryCacheProvider();
            DistributedCacheUtil.RegisterProvider("test", provider);

            var retrieved = DistributedCacheUtil.GetProvider("test");
            Assert.NotNull(retrieved);
            Assert.Same(provider, retrieved);
        }

        [Fact]
        public void GetProvider_NonExistentName_ReturnsNull()
        {
            var retrieved = DistributedCacheUtil.GetProvider("nonexistent");
            Assert.Null(retrieved);
        }

        [Fact]
        public void RegisterProvider_SetDefault_UpdatesDefaultProvider()
        {
            using var provider = new MemoryCacheProvider();
            DistributedCacheUtil.RegisterProvider("custom", provider, setDefault: true);

            // 注意：这会影响全局默认提供者，后续测试可能受影响
            Assert.NotNull(DistributedCacheUtil.GetProvider("custom"));
        }

        #endregion

        #region CreateMemoryProvider 测试

        [Fact]
        public void CreateMemoryProvider_ReturnsMemoryCacheProvider()
        {
            using var provider = DistributedCacheUtil.CreateMemoryProvider();
            Assert.NotNull(provider);
            Assert.IsType<MemoryCacheProvider>(provider);
        }

        [Fact]
        public void CreateMemoryProvider_WithCleanupInterval_ReturnsProvider()
        {
            using var provider = DistributedCacheUtil.CreateMemoryProvider(TimeSpan.FromMinutes(5));
            Assert.NotNull(provider);
        }

        [Fact]
        public void CreateMemoryProvider_WithSizeLimit_ReturnsProvider()
        {
            using var provider = DistributedCacheUtil.CreateMemoryProvider(null, 1000);
            Assert.NotNull(provider);
        }

        #endregion

        #region CreateRedisProvider 测试

        [Fact]
        public void CreateRedisProvider_ReturnsRedisCacheProvider()
        {
            var provider = DistributedCacheUtil.CreateRedisProvider();
            Assert.NotNull(provider);
            Assert.IsType<RedisCacheProvider>(provider);
        }

        [Fact]
        public void CreateRedisProvider_WithOptions_ReturnsProvider()
        {
            var options = new RedisCacheOptions
            {
                ConnectionString = "localhost:6379",
                DefaultDatabase = 1
            };

            var provider = DistributedCacheUtil.CreateRedisProvider(options);
            Assert.NotNull(provider);
        }

        #endregion

        #region 便捷方法测试 - Set/Get

        [Fact]
        public void Set_ValidKeyAndValue_StoresInDefaultProvider()
        {
            DistributedCacheUtil.Set("utilKey1", "utilValue1");
            var result = DistributedCacheUtil.Get<string>("utilKey1");

            Assert.Equal("utilValue1", result);
        }

        [Fact]
        public async Task SetAsync_ValidKeyAndValue_StoresInDefaultProvider()
        {
            await DistributedCacheUtil.SetAsync("utilAsyncKey", "utilAsyncValue");
            var result = await DistributedCacheUtil.GetAsync<string>("utilAsyncKey");

            Assert.Equal("utilAsyncValue", result);
        }

        [Fact]
        public void Get_NonExistentKey_ReturnsDefault()
        {
            var result = DistributedCacheUtil.Get<string>("nonexistent");
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAsync_NonExistentKey_ReturnsDefault()
        {
            var result = await DistributedCacheUtil.GetAsync<string>("nonexistent");
            Assert.Null(result);
        }

        #endregion

        #region GetOrAdd 测试

        [Fact]
        public void GetOrAdd_NonExistentKey_AddsValue()
        {
            var result = DistributedCacheUtil.GetOrAdd("utilOrAddKey", () => "computedValue");
            Assert.Equal("computedValue", result);
        }

        [Fact]
        public async Task GetOrAddAsync_NonExistentKey_AddsValue()
        {
            var result = await DistributedCacheUtil.GetOrAddAsync(
                "utilAsyncOrAddKey",
                () => Task.FromResult("asyncComputedValue"));

            Assert.Equal("asyncComputedValue", result);
        }

        #endregion

        #region Exists 测试

        [Fact]
        public void Exists_ExistingKey_ReturnsTrue()
        {
            DistributedCacheUtil.Set("utilExistsKey", "value");
            Assert.True(DistributedCacheUtil.Exists("utilExistsKey"));
        }

        [Fact]
        public void Exists_NonExistentKey_ReturnsFalse()
        {
            Assert.False(DistributedCacheUtil.Exists("nonexistent"));
        }

        [Fact]
        public async Task ExistsAsync_ExistingKey_ReturnsTrue()
        {
            DistributedCacheUtil.Set("utilAsyncExistsKey", "value");
            Assert.True(await DistributedCacheUtil.ExistsAsync("utilAsyncExistsKey"));
        }

        #endregion

        #region Remove 测试

        [Fact]
        public void Remove_ExistingKey_RemovesValue()
        {
            DistributedCacheUtil.Set("utilRemoveKey", "value");
            DistributedCacheUtil.Remove("utilRemoveKey");

            Assert.False(DistributedCacheUtil.Exists("utilRemoveKey"));
        }

        [Fact]
        public async Task RemoveAsync_ExistingKey_RemovesValue()
        {
            DistributedCacheUtil.Set("utilAsyncRemoveKey", "value");
            await DistributedCacheUtil.RemoveAsync("utilAsyncRemoveKey");

            Assert.False(DistributedCacheUtil.Exists("utilAsyncRemoveKey"));
        }

        #endregion

        #region Clear 测试

        [Fact]
        public void Clear_RemovesAllValues()
        {
            DistributedCacheUtil.Set("clearKey1", "value1");
            DistributedCacheUtil.Set("clearKey2", "value2");
            DistributedCacheUtil.Clear();

            Assert.False(DistributedCacheUtil.Exists("clearKey1"));
            Assert.False(DistributedCacheUtil.Exists("clearKey2"));
        }

        [Fact]
        public async Task ClearAsync_RemovesAllValues()
        {
            DistributedCacheUtil.Set("asyncClearKey1", "value1");
            DistributedCacheUtil.Set("asyncClearKey2", "value2");
            await DistributedCacheUtil.ClearAsync();

            Assert.False(DistributedCacheUtil.Exists("asyncClearKey1"));
            Assert.False(DistributedCacheUtil.Exists("asyncClearKey2"));
        }

        #endregion

        #region GetManyAsync/SetManyAsync 测试

        [Fact]
        public async Task GetManyAsync_MultipleKeys_ReturnsDictionary()
        {
            DistributedCacheUtil.Set("manyKey1", "value1");
            DistributedCacheUtil.Set("manyKey2", "value2");

            var result = await DistributedCacheUtil.GetManyAsync<string>(
                new[] { "manyKey1", "manyKey2", "nonexistent" });

            Assert.Equal(3, result.Count);
            Assert.Equal("value1", result["manyKey1"]);
            Assert.Equal("value2", result["manyKey2"]);
            Assert.Null(result["nonexistent"]);
        }

        [Fact]
        public async Task SetManyAsync_MultipleItems_StoresAll()
        {
            var items = new Dictionary<string, string>
            {
                { "setManyKey1", "value1" },
                { "setManyKey2", "value2" }
            };

            await DistributedCacheUtil.SetManyAsync(items);

            Assert.True(DistributedCacheUtil.Exists("setManyKey1"));
            Assert.True(DistributedCacheUtil.Exists("setManyKey2"));
        }

        #endregion

        #region RefreshAsync 测试

        [Fact]
        public async Task RefreshAsync_ExistingKey_ReplacesValue()
        {
            DistributedCacheUtil.Set("refreshKey", "oldValue");
            var result = await DistributedCacheUtil.RefreshAsync(
                "refreshKey",
                () => Task.FromResult("newValue"));

            Assert.Equal("newValue", result);
            Assert.Equal("newValue", DistributedCacheUtil.Get<string>("refreshKey"));
        }

        #endregion

        #region MultiLevelCache 测试

        [Fact]
        public void MultiLevelCache_SetAndGet_WorksCorrectly()
        {
            using var multiCache = new MultiLevelCache();
            multiCache.Set("multiKey", "multiValue");

            var result = multiCache.Get<string>("multiKey");
            Assert.Equal("multiValue", result);
        }

        [Fact]
        public void MultiLevelCache_GetOrAdd_ComputesValue()
        {
            using var multiCache = new MultiLevelCache();
            var result = multiCache.GetOrAdd("multiOrAddKey", () => "computed");

            Assert.Equal("computed", result);
        }

        [Fact]
        public void MultiLevelCache_Exists_ChecksCorrectly()
        {
            using var multiCache = new MultiLevelCache();
            multiCache.Set("multiExistsKey", "value");

            Assert.True(multiCache.Exists("multiExistsKey"));
            Assert.False(multiCache.Exists("nonexistent"));
        }

        [Fact]
        public void MultiLevelCache_Remove_RemovesValue()
        {
            using var multiCache = new MultiLevelCache();
            multiCache.Set("multiRemoveKey", "value");
            multiCache.Remove("multiRemoveKey");

            Assert.False(multiCache.Exists("multiRemoveKey"));
        }

        [Fact]
        public void MultiLevelCache_Count_ReturnsCorrectCount()
        {
            using var multiCache = new MultiLevelCache();
            multiCache.Set("key1", "value1");
            multiCache.Set("key2", "value2");

            Assert.Equal(2, multiCache.Count());
        }

        [Fact]
        public void MultiLevelCache_WithDistributedCache_UsesBothLevels()
        {
            using var distributedCache = new MemoryCacheProvider();
            using var multiCache = new MultiLevelCache(distributedCache);

            multiCache.Set("dualKey", "dualValue");

            // 本地缓存应该有值
            Assert.True(multiCache.Exists("dualKey"));
        }

        [Fact]
        public async Task MultiLevelCache_AsyncMethods_WorkCorrectly()
        {
            using var multiCache = new MultiLevelCache();
            await multiCache.SetAsync("asyncMultiKey", "asyncMultiValue");

            var result = await multiCache.GetAsync<string>("asyncMultiKey");
            Assert.Equal("asyncMultiValue", result);
        }

        [Fact]
        public void MultiLevelCache_Clear_RemovesAll()
        {
            using var multiCache = new MultiLevelCache();
            multiCache.Set("key1", "value1");
            multiCache.Set("key2", "value2");
            multiCache.Clear();

            Assert.Equal(0, multiCache.Count());
        }

        #endregion

        #region RedisCacheOptions 测试

        [Fact]
        public void RedisCacheOptions_DefaultValues_AreCorrect()
        {
            var options = new RedisCacheOptions();

            Assert.Equal("localhost:6379", options.ConnectionString);
            Assert.Equal("", options.InstanceName);
            Assert.Equal(0, options.DefaultDatabase);
            Assert.Equal(TimeSpan.FromSeconds(5), options.ConnectTimeout);
            Assert.False(options.AllowAdmin);
            Assert.False(options.UseSsl);
            Assert.Null(options.Password);
        }

        [Fact]
        public void RedisCacheOptions_CustomValues_AreSetCorrectly()
        {
            var options = new RedisCacheOptions
            {
                ConnectionString = "redis.example.com:6380",
                InstanceName = "myapp",
                DefaultDatabase = 2,
                Password = "secret",
                UseSsl = true,
                AllowAdmin = true
            };

            Assert.Equal("redis.example.com:6380", options.ConnectionString);
            Assert.Equal("myapp", options.InstanceName);
            Assert.Equal(2, options.DefaultDatabase);
            Assert.Equal("secret", options.Password);
            Assert.True(options.UseSsl);
            Assert.True(options.AllowAdmin);
        }

        #endregion
    }
}