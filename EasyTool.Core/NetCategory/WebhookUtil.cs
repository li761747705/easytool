using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EasyTool.NetCategory
{
    /// <summary>
    /// Webhook工具类
    /// 提供Webhook发送、签名、验证等功能
    /// </summary>
    public static class WebhookUtil
    {
        private static readonly HttpClient _httpClient = new();

        public static async Task<WebhookResponse> SendJsonAsync(string url, object data, Dictionary<string, string>? headers = null)
        {
            try
            {
                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };

                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }

                var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
                var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                return new WebhookResponse
                {
                    Success = response.IsSuccessStatusCode,
                    StatusCode = (int)response.StatusCode,
                    Body = responseBody
                };
            }
            catch (Exception ex)
            {
                return new WebhookResponse { Success = false, Error = ex.Message };
            }
        }

        public static string Sign(string payload, string secret, string algorithm = "sha256")
        {
            using System.Security.Cryptography.HMAC hmac = algorithm.ToLower() switch
            {
                "sha1" => new System.Security.Cryptography.HMACSHA1(Encoding.UTF8.GetBytes(secret)),
                "sha512" => new System.Security.Cryptography.HMACSHA512(Encoding.UTF8.GetBytes(secret)),
                _ => new System.Security.Cryptography.HMACSHA256(Encoding.UTF8.GetBytes(secret))
            };
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

        public static bool VerifySignature(string payload, string signature, string secret, string algorithm = "sha256")
        {
            var expectedSignature = Sign(payload, secret, algorithm);
            return string.Equals(expectedSignature, signature, StringComparison.OrdinalIgnoreCase);
        }

        public static string GenerateGitHubSignature(string payload, string secret)
        {
            return $"sha256={Sign(payload, secret, "sha256")}";
        }

        public static bool VerifyGitHubSignature(string payload, string signatureHeader, string secret)
        {
            if (string.IsNullOrEmpty(signatureHeader) || !signatureHeader.StartsWith("sha256="))
                return false;
            return VerifySignature(payload, signatureHeader[7..], secret, "sha256");
        }

        public static long GetTimestamp() => DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        public static bool ValidateTimestamp(long timestamp, int toleranceSeconds = 300)
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return Math.Abs(now - timestamp) <= toleranceSeconds;
        }

        /// <summary>
        /// Webhook 响应
        /// </summary>
        public class WebhookResponse
        {
            /// <summary>
            /// 是否成功
            /// </summary>
            public bool Success { get; set; }

            /// <summary>
            /// HTTP 状态码
            /// </summary>
            public int StatusCode { get; set; }

            /// <summary>
            /// 响应内容
            /// </summary>
            public string? Body { get; set; }

            /// <summary>
            /// 错误信息（失败时）
            /// </summary>
            public string? Error { get; set; }
        }
    }
}
