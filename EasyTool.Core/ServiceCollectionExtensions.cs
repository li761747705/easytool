using System;
using System.Net.Http;
using EasyTool.CacheCategory;
using EasyTool.DatabaseCategory;
using EasyTool.QueueCategory;
using Microsoft.Extensions.DependencyInjection;

namespace EasyTool
{
    /// <summary>
    /// IServiceCollection 扩展方法
    /// 提供依赖注入注册
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 添加 EasyTool 核心服务
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <returns>服务集合</returns>
        public static IServiceCollection AddEasyTool(this IServiceCollection services)
        {
            // 注册缓存服务
            services.AddSingleton<ICacheProvider, MemoryCacheProvider>();

            // 注册 HttpClient 工厂
            services.AddHttpClient();

            return services;
        }

        /// <summary>
        /// 添加 EasyTool 缓存服务
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <param name="configure">配置委托</param>
        /// <returns>服务集合</returns>
        public static IServiceCollection AddEasyToolCache(
            this IServiceCollection services,
            Action<CacheOptions>? configure = null)
        {
            var options = new CacheOptions();
            configure?.Invoke(options);

            services.AddSingleton<ICacheProvider>(sp =>
            {
                return new MemoryCacheProvider(
                    options.CleanupInterval,
                    options.SizeLimit);
            });

            return services;
        }

        /// <summary>
        /// 添加 EasyTool Redis 缓存服务
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <param name="configure">配置委托</param>
        /// <returns>服务集合</returns>
        public static IServiceCollection AddEasyToolRedisCache(
            this IServiceCollection services,
            Action<RedisCacheOptions> configure)
        {
            var options = new RedisCacheOptions();
            configure(options);

            services.AddSingleton<ICacheProvider>(sp =>
            {
                return new RedisCacheProvider(options);
            });

            return services;
        }

        /// <summary>
        /// 添加 EasyTool 数据库连接池服务
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <param name="connectionString">连接字符串</param>
        /// <param name="providerFactory">数据库提供者工厂</param>
        /// <param name="configure">配置委托</param>
        /// <returns>服务集合</returns>
        public static IServiceCollection AddEasyToolConnectionPool(
            this IServiceCollection services,
            string connectionString,
            System.Data.Common.DbProviderFactory providerFactory,
            Action<ConnectionPoolOptions>? configure = null)
        {
            var options = new ConnectionPoolOptions();
            configure?.Invoke(options);

            services.AddSingleton<ConnectionPool>(sp =>
            {
                return new ConnectionPool(connectionString, providerFactory, options);
            });

            return services;
        }

        /// <summary>
        /// 添加 EasyTool 消息队列服务
        /// </summary>
        /// <typeparam name="T">消息类型</typeparam>
        /// <param name="services">服务集合</param>
        /// <param name="handler">消息处理器</param>
        /// <param name="configure">配置委托</param>
        /// <returns>服务集合</returns>
        public static IServiceCollection AddEasyToolMessageQueue<T>(
            this IServiceCollection services,
            Func<QueueCategory.Message<T>, System.Threading.Tasks.Task<QueueCategory.ProcessResult>> handler,
            Action<QueueCategory.MessageQueueOptions>? configure = null)
        {
            var options = new QueueCategory.MessageQueueOptions();
            configure?.Invoke(options);

            services.AddSingleton<QueueCategory.MessageQueue<T>>(sp =>
            {
                return new QueueCategory.MessageQueue<T>(handler, options);
            });

            return services;
        }

        /// <summary>
        /// 添加 EasyTool HttpClient 服务
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <param name="name">客户端名称</param>
        /// <param name="configure">配置委托</param>
        /// <returns>服务集合</returns>
        public static IServiceCollection AddEasyToolHttpClient(
            this IServiceCollection services,
            string name,
            Action<NetCategory.HttpClientBuilder>? configure = null)
        {
            services.AddHttpClient(name)
                .ConfigurePrimaryHttpMessageHandler(() =>
                {
                    var builder = new NetCategory.HttpClientBuilder();
                    configure?.Invoke(builder);
                    return new HttpClientHandler();
                });

            return services;
        }

        /// <summary>
        /// 添加 EasyTool 多级缓存服务
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <param name="distributedCacheProvider">分布式缓存提供者</param>
        /// <param name="localCacheExpiration">本地缓存过期时间</param>
        /// <returns>服务集合</returns>
        public static IServiceCollection AddEasyToolMultiLevelCache(
            this IServiceCollection services,
            ICacheProvider? distributedCacheProvider = null,
            TimeSpan? localCacheExpiration = null)
        {
            services.AddSingleton<ICacheProvider>(sp =>
            {
                return new MultiLevelCache(distributedCacheProvider, localCacheExpiration);
            });

            return services;
        }
    }

    /// <summary>
    /// 缓存配置选项
    /// </summary>
    public class CacheOptions
    {
        /// <summary>
        /// 清理间隔
        /// </summary>
        public TimeSpan? CleanupInterval { get; set; }

        /// <summary>
        /// 大小限制
        /// </summary>
        public long? SizeLimit { get; set; }
    }
}
