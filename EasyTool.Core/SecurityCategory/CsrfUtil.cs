using System;
using System.Security.Cryptography;
using System.Text;

namespace EasyTool.SecurityCategory
{
    /// <summary>
    /// CSRF（跨站请求伪造）防护工具类
    /// </summary>
    public static class CsrfUtil
    {
        private static readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();
        private static readonly int _defaultTokenLength = 32;

        /// <summary>
        /// 生成 CSRF Token
        /// </summary>
        /// <param name="length">Token 长度（字节数）</param>
        /// <returns>Base64 编码的 Token</returns>
        public static string GenerateToken(int length = 0)
        {
            var tokenLength = length > 0 ? length : _defaultTokenLength;
            var bytes = new byte[tokenLength];
            _rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// 生成 URL 安全的 CSRF Token
        /// </summary>
        /// <param name="length">Token 长度（字节数）</param>
        /// <returns>URL 安全的 Base64 编码 Token</returns>
        public static string GenerateUrlSafeToken(int length = 0)
        {
            var token = GenerateToken(length);
            return token.Replace("+", "-").Replace("/", "_").TrimEnd('=');
        }

        /// <summary>
        /// 生成带签名的 CSRF Token
        /// </summary>
        /// <param name="secret">签名密钥</param>
        /// <param name="data">要签名的数据（如用户ID、会话ID等）</param>
        /// <param name="expirationMinutes">过期时间（分钟）</param>
        /// <returns>签名的 Token</returns>
        public static string GenerateSignedToken(string secret, string data, int expirationMinutes = 0)
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var expiration = expirationMinutes > 0 ? timestamp + (expirationMinutes * 60) : 0;

            var payload = expiration > 0
                ? $"{data}|{timestamp}|{expiration}"
                : $"{data}|{timestamp}";

            var signature = ComputeHmacSha256(secret, payload);
            var token = $"{payload}|{signature}";

            return Convert.ToBase64String(Encoding.UTF8.GetBytes(token));
        }

        /// <summary>
        /// 验证签名的 CSRF Token
        /// </summary>
        /// <param name="secret">签名密钥</param>
        /// <param name="token">要验证的 Token</param>
        /// <param name="data">期望的数据</param>
        /// <returns>验证结果</returns>
        public static CsrfValidationResult ValidateSignedToken(string secret, string token, string data)
        {
            try
            {
                var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(token));
                var parts = decoded.Split('|');

                if (parts.Length != 4)
                {
                    return CsrfValidationResult.Fail("Token 格式无效");
                }

                var tokenData = parts[0];
                var timestampStr = parts[1];
                var expirationStr = parts[2];
                var signature = parts[3];

                // 验证数据
                if (tokenData != data)
                {
                    return CsrfValidationResult.Fail("Token 数据不匹配");
                }

                // 验证签名
                var payload = $"{tokenData}|{timestampStr}|{expirationStr}";
                var expectedSignature = ComputeHmacSha256(secret, payload);

                if (!ConstantTimeEquals(signature, expectedSignature))
                {
                    return CsrfValidationResult.Fail("Token 签名无效");
                }

                // 验证过期时间
                if (long.TryParse(expirationStr, out var expiration) && expiration > 0)
                {
                    var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    if (now > expiration)
                    {
                        return CsrfValidationResult.Fail("Token 已过期");
                    }
                }

                // 解析创建时间
                var createdAt = long.TryParse(timestampStr, out var ts)
                    ? DateTimeOffset.FromUnixTimeSeconds(ts)
                    : (DateTimeOffset?)null;

                return CsrfValidationResult.Success(createdAt);
            }
            catch (Exception ex)
            {
                return CsrfValidationResult.Fail($"Token 验证失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 生成双提交 Cookie 模式的 Token
        /// </summary>
        /// <param name="cookieToken">Cookie 中的 Token</param>
        /// <returns>请求中应携带的 Token</returns>
        public static string GenerateDoubleSubmitToken(string cookieToken)
        {
            var randomPart = GenerateToken(16);
            return $"{cookieToken}:{randomPart}";
        }

        /// <summary>
        /// 验证双提交 Cookie 模式
        /// </summary>
        /// <param name="cookieToken">Cookie 中的 Token</param>
        /// <param name="requestToken">请求中携带的 Token</param>
        /// <returns>验证结果</returns>
        public static bool ValidateDoubleSubmitToken(string cookieToken, string requestToken)
        {
            if (string.IsNullOrEmpty(cookieToken) || string.IsNullOrEmpty(requestToken))
            {
                return false;
            }

            var parts = requestToken.Split(':');
            if (parts.Length != 2)
            {
                return false;
            }

            return ConstantTimeEquals(cookieToken, parts[0]);
        }

        /// <summary>
        /// 生成同步器 Token 模式的 Token
        /// </summary>
        /// <param name="sessionToken">会话 Token</param>
        /// <param name="formId">表单 ID</param>
        /// <returns>同步器 Token</returns>
        public static string GenerateSynchronizerToken(string sessionToken, string formId)
        {
            var combined = $"{sessionToken}|{formId}";
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(combined));
            return Convert.ToBase64String(hash);
        }

        /// <summary>
        /// 验证同步器 Token
        /// </summary>
        /// <param name="sessionToken">会话 Token</param>
        /// <param name="formId">表单 ID</param>
        /// <param name="token">要验证的 Token</param>
        /// <returns>验证结果</returns>
        public static bool ValidateSynchronizerToken(string sessionToken, string formId, string token)
        {
            if (string.IsNullOrEmpty(sessionToken) || string.IsNullOrEmpty(token))
            {
                return false;
            }

            var expected = GenerateSynchronizerToken(sessionToken, formId);
            return ConstantTimeEquals(expected, token);
        }

        /// <summary>
        /// 生成基于会话的 CSRF Token
        /// </summary>
        /// <param name="sessionId">会话 ID</param>
        /// <param name="secret">应用密钥</param>
        /// <param name="additionalData">额外数据</param>
        /// <returns>CSRF Token</returns>
        public static string GenerateSessionToken(string sessionId, string secret, params string[] additionalData)
        {
            var data = additionalData.Length > 0
                ? $"{sessionId}|{string.Join("|", additionalData)}"
                : sessionId;

            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var payload = $"{data}|{timestamp}";
            var signature = ComputeHmacSha256(secret, payload);

            return $"{payload}|{signature}";
        }

        /// <summary>
        /// 验证基于会话的 CSRF Token
        /// </summary>
        /// <param name="sessionId">会话 ID</param>
        /// <param name="secret">应用密钥</param>
        /// <param name="token">要验证的 Token</param>
        /// <param name="maxAgeSeconds">最大有效期（秒）</param>
        /// <returns>验证结果</returns>
        public static CsrfValidationResult ValidateSessionToken(string sessionId, string secret, string token, long maxAgeSeconds = 0)
        {
            try
            {
                var parts = token.Split('|');
                if (parts.Length < 3)
                {
                    return CsrfValidationResult.Fail("Token 格式无效");
                }

                // 提取签名和数据
                var signature = parts[^1];
                var payloadParts = new ArraySegment<string>(parts, 0, parts.Length - 1).ToArray();
                var payload = string.Join("|", payloadParts);

                // 验证签名
                var expectedSignature = ComputeHmacSha256(secret, payload);
                if (!ConstantTimeEquals(signature, expectedSignature))
                {
                    return CsrfValidationResult.Fail("Token 签名无效");
                }

                // 提取时间戳（最后一个非签名部分）
                var timestampStr = payloadParts[^1];
                if (!long.TryParse(timestampStr, out var timestamp))
                {
                    return CsrfValidationResult.Fail("Token 时间戳无效");
                }

                // 检查过期时间
                if (maxAgeSeconds > 0)
                {
                    var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    if (now - timestamp > maxAgeSeconds)
                    {
                        return CsrfValidationResult.Fail("Token 已过期");
                    }
                }

                // 提取会话 ID（第一个部分）
                var tokenSessionId = payloadParts[0];
                if (tokenSessionId != sessionId)
                {
                    return CsrfValidationResult.Fail("Token 会话不匹配");
                }

                var createdAt = DateTimeOffset.FromUnixTimeSeconds(timestamp);
                return CsrfValidationResult.Success(createdAt);
            }
            catch (Exception ex)
            {
                return CsrfValidationResult.Fail($"Token 验证失败: {ex.Message}");
            }
        }

        private static string ComputeHmacSha256(string key, string data)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return Convert.ToBase64String(hash);
        }

        private static bool ConstantTimeEquals(string a, string b)
        {
            if (a == null || b == null)
            {
                return a == b;
            }

            if (a.Length != b.Length)
            {
                return false;
            }

            var result = 0;
            for (int i = 0; i < a.Length; i++)
            {
                result |= a[i] ^ b[i];
            }
            return result == 0;
        }
    }

    /// <summary>
    /// CSRF 验证结果
    /// </summary>
    public class CsrfValidationResult
    {
        /// <summary>
        /// 是否验证通过
        /// </summary>
        public bool IsValid { get; }

        /// <summary>
        /// 错误消息
        /// </summary>
        public string? ErrorMessage { get; }

        /// <summary>
        /// Token 创建时间
        /// </summary>
        public DateTimeOffset? CreatedAt { get; }

        private CsrfValidationResult(bool isValid, string? errorMessage, DateTimeOffset? createdAt)
        {
            IsValid = isValid;
            ErrorMessage = errorMessage;
            CreatedAt = createdAt;
        }

        /// <summary>
        /// 验证成功
        /// </summary>
        public static CsrfValidationResult Success(DateTimeOffset? createdAt = null)
        {
            return new CsrfValidationResult(true, null, createdAt);
        }

        /// <summary>
        /// 验证失败
        /// </summary>
        public static CsrfValidationResult Fail(string errorMessage)
        {
            return new CsrfValidationResult(false, errorMessage, null);
        }
    }
}
