using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EasyTool.IOCategory
{
    /// <summary>
    /// 嵌入资源工具类
    /// 提供程序集嵌入资源的读取和管理功能
    /// </summary>
    public static class ResourceUtil
    {
        #region 读取嵌入资源

        /// <summary>
        /// 读取嵌入资源为字符串
        /// </summary>
        /// <param name="resourceName">资源名称</param>
        /// <param name="assembly">程序集（默认为调用程序集）</param>
        /// <returns>资源内容</returns>
        public static string ReadAsString(string resourceName, Assembly? assembly = null)
        {
            using var stream = GetStream(resourceName, assembly);
            if (stream == null)
                throw new FileNotFoundException($"嵌入资源未找到: {resourceName}");

            using var reader = new StreamReader(stream, Encoding.UTF8);
            return reader.ReadToEnd();
        }

        /// <summary>
        /// 读取嵌入资源为字节数组
        /// </summary>
        /// <param name="resourceName">资源名称</param>
        /// <param name="assembly">程序集</param>
        /// <returns>资源数据</returns>
        public static byte[] ReadAsBytes(string resourceName, Assembly? assembly = null)
        {
            using var stream = GetStream(resourceName, assembly);
            if (stream == null)
                throw new FileNotFoundException($"嵌入资源未找到: {resourceName}");

            using var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            return memoryStream.ToArray();
        }

        /// <summary>
        /// 获取嵌入资源流
        /// </summary>
        /// <param name="resourceName">资源名称</param>
        /// <param name="assembly">程序集</param>
        /// <returns>资源流</returns>
        public static Stream? GetStream(string resourceName, Assembly? assembly = null)
        {
            assembly ??= Assembly.GetCallingAssembly();

            // 尝试精确匹配
            var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream != null)
                return stream;

            // 尝试模糊匹配
            var names = assembly.GetManifestResourceNames();
            var matchedName = names.FirstOrDefault(n =>
                n.Equals(resourceName, StringComparison.OrdinalIgnoreCase) ||
                n.EndsWith("." + resourceName, StringComparison.OrdinalIgnoreCase));

            if (matchedName != null)
                return assembly.GetManifestResourceStream(matchedName);

            return null;
        }

        /// <summary>
        /// 异步读取嵌入资源为字符串
        /// </summary>
        /// <param name="resourceName">资源名称</param>
        /// <param name="assembly">程序集</param>
        /// <returns>资源内容</returns>
        public static async Task<string> ReadAsStringAsync(string resourceName, Assembly? assembly = null)
        {
            using var stream = GetStream(resourceName, assembly);
            if (stream == null)
                throw new FileNotFoundException($"嵌入资源未找到: {resourceName}");

            using var reader = new StreamReader(stream, Encoding.UTF8);
            return await reader.ReadToEndAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// 异步读取嵌入资源为字节数组
        /// </summary>
        /// <param name="resourceName">资源名称</param>
        /// <param name="assembly">程序集</param>
        /// <returns>资源数据</returns>
        public static async Task<byte[]> ReadAsBytesAsync(string resourceName, Assembly? assembly = null)
        {
            using var stream = GetStream(resourceName, assembly);
            if (stream == null)
                throw new FileNotFoundException($"嵌入资源未找到: {resourceName}");

            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream).ConfigureAwait(false);
            return memoryStream.ToArray();
        }

        #endregion

        #region 读取行

        /// <summary>
        /// 读取嵌入资源的所有行
        /// </summary>
        /// <param name="resourceName">资源名称</param>
        /// <param name="assembly">程序集</param>
        /// <returns>行列表</returns>
        public static List<string> ReadAllLines(string resourceName, Assembly? assembly = null)
        {
            var content = ReadAsString(resourceName, assembly);
            return content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None).ToList();
        }

        /// <summary>
        /// 逐行读取嵌入资源
        /// </summary>
        /// <param name="resourceName">资源名称</param>
        /// <param name="assembly">程序集</param>
        /// <returns>行枚举</returns>
        public static IEnumerable<string> ReadLines(string resourceName, Assembly? assembly = null)
        {
            using var stream = GetStream(resourceName, assembly);
            if (stream == null)
                throw new FileNotFoundException($"嵌入资源未找到: {resourceName}");

            using var reader = new StreamReader(stream, Encoding.UTF8);
            while (!reader.EndOfStream)
            {
                yield return reader.ReadLine() ?? string.Empty;
            }
        }

        #endregion

        #region 资源信息

        /// <summary>
        /// 获取程序集中所有嵌入资源名称
        /// </summary>
        /// <param name="assembly">程序集</param>
        /// <returns>资源名称列表</returns>
        public static string[] GetResourceNames(Assembly? assembly = null)
        {
            assembly ??= Assembly.GetCallingAssembly();
            return assembly.GetManifestResourceNames();
        }

        /// <summary>
        /// 检查嵌入资源是否存在
        /// </summary>
        /// <param name="resourceName">资源名称</param>
        /// <param name="assembly">程序集</param>
        /// <returns>是否存在</returns>
        public static bool Exists(string resourceName, Assembly? assembly = null)
        {
            using var stream = GetStream(resourceName, assembly);
            return stream != null;
        }

        /// <summary>
        /// 获取嵌入资源信息
        /// </summary>
        /// <param name="resourceName">资源名称</param>
        /// <param name="assembly">程序集</param>
        /// <returns>资源信息</returns>
        public static ResourceInfo? GetResourceInfo(string resourceName, Assembly? assembly = null)
        {
            assembly ??= Assembly.GetCallingAssembly();

            var names = assembly.GetManifestResourceNames();
            var matchedName = names.FirstOrDefault(n =>
                n.Equals(resourceName, StringComparison.OrdinalIgnoreCase) ||
                n.EndsWith("." + resourceName, StringComparison.OrdinalIgnoreCase));

            if (matchedName == null)
                return null;

            using var stream = assembly.GetManifestResourceStream(matchedName);
            if (stream == null)
                return null;

            return new ResourceInfo
            {
                FullName = matchedName,
                ShortName = GetShortName(matchedName),
                Size = stream.Length,
                Assembly = assembly
            };
        }

        private static string GetShortName(string fullName)
        {
            var parts = fullName.Split('.');
            if (parts.Length >= 2)
            {
                return parts[parts.Length - 1];
            }
            return fullName;
        }

        #endregion

        #region 提取资源

        /// <summary>
        /// 将嵌入资源提取到文件
        /// </summary>
        /// <param name="resourceName">资源名称</param>
        /// <param name="outputPath">输出文件路径</param>
        /// <param name="assembly">程序集</param>
        public static void ExtractToFile(string resourceName, string outputPath, Assembly? assembly = null)
        {
            using var stream = GetStream(resourceName, assembly);
            if (stream == null)
                throw new FileNotFoundException($"嵌入资源未找到: {resourceName}");

            var directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using var fileStream = File.Create(outputPath);
            stream.CopyTo(fileStream);
        }

        /// <summary>
        /// 异步将嵌入资源提取到文件
        /// </summary>
        /// <param name="resourceName">资源名称</param>
        /// <param name="outputPath">输出文件路径</param>
        /// <param name="assembly">程序集</param>
        public static async Task ExtractToFileAsync(string resourceName, string outputPath, Assembly? assembly = null)
        {
            using var stream = GetStream(resourceName, assembly);
            if (stream == null)
                throw new FileNotFoundException($"嵌入资源未找到: {resourceName}");

            var directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using var fileStream = File.Create(outputPath);
            await stream.CopyToAsync(fileStream).ConfigureAwait(false);
        }

        /// <summary>
        /// 将所有嵌入资源提取到目录
        /// </summary>
        /// <param name="outputDirectory">输出目录</param>
        /// <param name="assembly">程序集</param>
        /// <param name="filter">资源名称过滤器</param>
        /// <returns>提取的文件数量</returns>
        public static int ExtractAllToDirectory(string outputDirectory, Assembly? assembly = null, Func<string, bool>? filter = null)
        {
            assembly ??= Assembly.GetCallingAssembly();
            var names = assembly.GetManifestResourceNames();
            int count = 0;

            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            foreach (var name in names)
            {
                if (filter != null && !filter(name))
                    continue;

                var shortName = GetShortName(name);
                var outputPath = Path.Combine(outputDirectory, shortName);

                try
                {
                    ExtractToFile(name, outputPath, assembly);
                    count++;
                }
                catch
                {
                    // 忽略提取失败的资源
                }
            }

            return count;
        }

        #endregion

        #region 类型化资源

        /// <summary>
        /// 读取嵌入资源并反序列化为对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="resourceName">资源名称</param>
        /// <param name="assembly">程序集</param>
        /// <returns>反序列化的对象</returns>
        public static T? ReadAsJson<T>(string resourceName, Assembly? assembly = null)
        {
            var json = ReadAsString(resourceName, assembly);
            return System.Text.Json.JsonSerializer.Deserialize<T>(json);
        }

        /// <summary>
        /// 异步读取嵌入资源并反序列化为对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="resourceName">资源名称</param>
        /// <param name="assembly">程序集</param>
        /// <returns>反序列化的对象</returns>
        public static async Task<T?> ReadAsJsonAsync<T>(string resourceName, Assembly? assembly = null)
        {
            var json = await ReadAsStringAsync(resourceName, assembly).ConfigureAwait(false);
            return System.Text.Json.JsonSerializer.Deserialize<T>(json);
        }

        #endregion

        #region 快捷方法

        /// <summary>
        /// 从当前程序集读取嵌入资源
        /// </summary>
        /// <param name="resourceName">资源名称</param>
        /// <returns>资源内容</returns>
        public static string Read(string resourceName)
        {
            return ReadAsString(resourceName, Assembly.GetCallingAssembly());
        }

        /// <summary>
        /// 从指定类型所在程序集读取嵌入资源
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="resourceName">资源名称</param>
        /// <returns>资源内容</returns>
        public static string ReadFromAssemblyOf<T>(string resourceName)
        {
            return ReadAsString(resourceName, typeof(T).Assembly);
        }

        /// <summary>
        /// 从类型所在程序集读取嵌入资源（资源名基于类型命名空间）
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="relativeName">相对资源名称</param>
        /// <returns>资源内容</returns>
        public static string ReadRelativeToType(Type type, string relativeName)
        {
            var ns = type.Namespace ?? string.Empty;
            var resourceName = string.IsNullOrEmpty(ns) ? relativeName : $"{ns}.{relativeName}";
            return ReadAsString(resourceName, type.Assembly);
        }

        /// <summary>
        /// 从类型所在程序集读取嵌入资源
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="relativeName">相对资源名称</param>
        /// <returns>资源内容</returns>
        public static string ReadRelativeToType<T>(string relativeName)
        {
            return ReadRelativeToType(typeof(T), relativeName);
        }

        #endregion
    }

    /// <summary>
    /// 资源信息
    /// </summary>
    public class ResourceInfo
    {
        /// <summary>
        /// 资源完整名称
        /// </summary>
        public string? FullName { get; set; }

        /// <summary>
        /// 资源短名称
        /// </summary>
        public string? ShortName { get; set; }

        /// <summary>
        /// 资源大小（字节）
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// 所在程序集
        /// </summary>
        public Assembly? Assembly { get; set; }

        public override string ToString()
        {
            return $"{ShortName} ({Size} bytes)";
        }
    }
}
