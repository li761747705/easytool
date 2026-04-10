using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EasyTool.NetCategory
{
    /// <summary>
    /// 短链接工具类
    /// 提供短链接生成、解析等功能
    /// </summary>
    public static class ShortUrlUtil
    {
        private static readonly HttpClient _httpClient = new();
        private static readonly string _chars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

        #region 自生成短链接

        /// <summary>
        /// 生成短链接码
        /// </summary>
        /// <param name="length">长度（默认6位）</param>
        /// <returns>短链接码</returns>
        public static string GenerateCode(int length = 6)
        {
            var bytes = new byte[length];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);

            var result = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                result.Append(_chars[bytes[i] % _chars.Length]);
            }

            return result.ToString();
        }

        /// <summary>
        /// 基于URL生成短链接码（同一URL生成相同短码）
        /// </summary>
        /// <param name="url">原始URL</param>
        /// <param name="length">长度</param>
        /// <returns>短链接码</returns>
        public static string GenerateCodeFromUrl(string url, int length = 6)
        {
            using var md5 = MD5.Create();
            var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(url));

            var result = new StringBuilder();
            for (int i = 0; i < length && i < hash.Length; i++)
            {
                result.Append(_chars[hash[i] % _chars.Length]);
            }

            return result.ToString();
        }

        /// <summary>
        /// 使用Base62编码生成短链接码
        /// </summary>
        /// <param name="id">数字ID</param>
        /// <returns>短链接码</returns>
        public static string EncodeBase62(long id)
        {
            if (id == 0) return "0";

            var result = new StringBuilder();
            while (id > 0)
            {
                result.Insert(0, _chars[(int)(id % 62)]);
                id /= 62;
            }

            return result.ToString();
        }

        /// <summary>
        /// 解码Base62短链接码
        /// </summary>
        /// <param name="code">短链接码</param>
        /// <returns>数字ID</returns>
        public static long DecodeBase62(string code)
        {
            long result = 0;
            foreach (var c in code)
            {
                result = result * 62 + _chars.IndexOf(c);
            }
            return result;
        }

        #endregion

        #region 短链接服务API

        /// <summary>
        /// 短链接服务配置
        /// </summary>
        public static class ShortUrlConfig
        {
            /// <summary>
            /// 自定义短链接域名
            /// </summary>
            public static string? CustomDomain { get; set; } = "https://s.example.com";

            /// <summary>
            /// 是否使用自定义域名
            /// </summary>
            public static bool UseCustomDomain { get; set; } = true;
        }

        /// <summary>
        /// 生成完整短链接
        /// </summary>
        /// <param name="code">短链接码</param>
        /// <returns>完整短链接</returns>
        public static string GetFullShortUrl(string code)
        {
            if (ShortUrlConfig.UseCustomDomain && !string.IsNullOrEmpty(ShortUrlConfig.CustomDomain))
            {
                return $"{ShortUrlConfig.CustomDomain.TrimEnd('/')}/{code}";
            }
            return $"/{code}";
        }

        /// <summary>
        /// 解析短链接码
        /// </summary>
        /// <param name="shortUrl">短链接</param>
        /// <returns>短链接码</returns>
        public static string? ParseCode(string shortUrl)
        {
            if (string.IsNullOrEmpty(shortUrl))
                return null;

            var uri = new Uri(shortUrl, UriKind.RelativeOrAbsolute);
            var path = uri.IsAbsoluteUri ? uri.AbsolutePath : uri.OriginalString;

            return path.TrimStart('/').Split('?')[0];
        }

        #endregion

        #region 第三方短链接服务

        /// <summary>
        /// 使用is.gd生成短链接
        /// </summary>
        /// <param name="url">原始URL</param>
        /// <returns>短链接</returns>
        public static async Task<string?> ShortenWithIsGdAsync(string url)
        {
            try
            {
                var apiUrl = $"https://is.gd/create.php?format=simple&url={Uri.EscapeDataString(url)}";
                return await _httpClient.GetStringAsync(apiUrl).ConfigureAwait(false);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 使用v.gd生成短链接
        /// </summary>
        /// <param name="url">原始URL</param>
        /// <returns>短链接</returns>
        public static async Task<string?> ShortenWithVGdAsync(string url)
        {
            try
            {
                var apiUrl = $"https://v.gd/create.php?format=simple&url={Uri.EscapeDataString(url)}";
                return await _httpClient.GetStringAsync(apiUrl).ConfigureAwait(false);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 使用tinyurl生成短链接
        /// </summary>
        /// <param name="url">原始URL</param>
        /// <returns>短链接</returns>
        public static async Task<string?> ShortenWithTinyUrlAsync(string url)
        {
            try
            {
                var apiUrl = $"https://tinyurl.com/api-create.php?url={Uri.EscapeDataString(url)}";
                return await _httpClient.GetStringAsync(apiUrl).ConfigureAwait(false);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 批量生成短链接
        /// </summary>
        /// <param name="urls">URL列表</param>
        /// <returns>原始URL与短链接映射</returns>
        public static async Task<Dictionary<string, string>> ShortenBatchAsync(IEnumerable<string> urls)
        {
            var result = new Dictionary<string, string>();

            foreach (var url in urls)
            {
                var shortUrl = await ShortenWithIsGdAsync(url).ConfigureAwait(false);
                if (!string.IsNullOrEmpty(shortUrl))
                {
                    result[url] = shortUrl;
                }
            }

            return result;
        }

        #endregion

        #region URL验证

        /// <summary>
        /// 验证URL格式
        /// </summary>
        /// <param name="url">URL</param>
        /// <returns>是否有效</returns>
        public static bool IsValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out var result)
                && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
        }

        /// <summary>
        /// 规范化URL
        /// </summary>
        /// <param name="url">URL</param>
        /// <returns>规范化后的URL</returns>
        public static string NormalizeUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                return url;

            if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                url = "https://" + url;
            }

            return url;
        }

        #endregion
    }
}