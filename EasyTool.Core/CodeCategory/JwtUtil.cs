using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Linq;

namespace EasyTool.CodeCategory
{
    /// <summary>
    /// JWT（JSON Web Token）工具类
    /// 提供 JWT 的生成、解析、验证功能
    /// 支持 HS256、HS384、HS512 算法
    /// </summary>
    public static class JwtUtil
    {
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        #region JWT 生成

        /// <summary>
        /// 生成 JWT Token
        /// </summary>
        /// <param name="payload">负载</param>
        /// <param name="secret">密钥</param>
        /// <param name="algorithm">算法（默认HS256）</param>
        /// <returns>JWT Token</returns>
        public static string Encode(object payload, string secret, JwtAlgorithm algorithm = JwtAlgorithm.HS256)
        {
            if (payload == null)
                throw new ArgumentNullException(nameof(payload));
            if (string.IsNullOrEmpty(secret))
                throw new ArgumentException("Secret cannot be null or empty", nameof(secret));

            var payloadDict = ObjectToDictionary(payload);
            return Encode(payloadDict, secret, algorithm);
        }

        /// <summary>
        /// 生成 JWT Token
        /// </summary>
        /// <param name="payload">负载字典</param>
        /// <param name="secret">密钥</param>
        /// <param name="algorithm">算法（默认HS256）</param>
        /// <returns>JWT Token</returns>
        public static string Encode(Dictionary<string, object> payload, string secret, JwtAlgorithm algorithm = JwtAlgorithm.HS256)
        {
            if (payload == null)
                throw new ArgumentNullException(nameof(payload));
            if (string.IsNullOrEmpty(secret))
                throw new ArgumentException("Secret cannot be null or empty", nameof(secret));

            // 创建 Header
            var header = new Dictionary<string, object>
            {
                { "typ", "JWT" },
                { "alg", algorithm.ToString() }
            };

            // 编码 Header 和 Payload
            string headerEncoded = Base64UrlEncode(JsonSerializer.Serialize(header));
            string payloadEncoded = Base64UrlEncode(JsonSerializer.Serialize(payload));

            // 创建签名
            string signatureInput = $"{headerEncoded}.{payloadEncoded}";
            string signature = CreateSignature(signatureInput, secret, algorithm);

            return $"{signatureInput}.{signature}";
        }

        /// <summary>
        /// 生成带有过期时间的 JWT Token
        /// </summary>
        /// <param name="payload">负载</param>
        /// <param name="secret">密钥</param>
        /// <param name="expiration">过期时间</param>
        /// <param name="algorithm">算法</param>
        /// <returns>JWT Token</returns>
        public static string Encode(object payload, string secret, TimeSpan expiration, JwtAlgorithm algorithm = JwtAlgorithm.HS256)
        {
            if (payload == null)
                throw new ArgumentNullException(nameof(payload));

            var payloadDict = ObjectToDictionary(payload);

            // 添加时间戳
            var now = DateTime.UtcNow;
            payloadDict["iat"] = ToUnixTimestamp(now);
            payloadDict["exp"] = ToUnixTimestamp(now.Add(expiration));
            payloadDict["nbf"] = ToUnixTimestamp(now);

            return Encode(payloadDict, secret, algorithm);
        }

        /// <summary>
        /// 生成带有完整时间信息的 JWT Token
        /// </summary>
        /// <param name="payload">负载</param>
        /// <param name="secret">密钥</param>
        /// <param name="issuer">签发者</param>
        /// <param name="audience">受众</param>
        /// <param name="expiration">过期时间</param>
        /// <param name="algorithm">算法</param>
        /// <returns>JWT Token</returns>
        public static string Encode(object payload, string secret, string issuer, string audience, TimeSpan expiration, JwtAlgorithm algorithm = JwtAlgorithm.HS256)
        {
            if (payload == null)
                throw new ArgumentNullException(nameof(payload));

            var payloadDict = ObjectToDictionary(payload);

            // 添加标准声明
            var now = DateTime.UtcNow;
            payloadDict["iss"] = issuer;
            payloadDict["aud"] = audience;
            payloadDict["iat"] = ToUnixTimestamp(now);
            payloadDict["exp"] = ToUnixTimestamp(now.Add(expiration));
            payloadDict["nbf"] = ToUnixTimestamp(now);
            payloadDict["jti"] = Guid.NewGuid().ToString();

            return Encode(payloadDict, secret, algorithm);
        }

        #endregion

        #region JWT 解析

        /// <summary>
        /// 解析 JWT Token（不验证签名）
        /// </summary>
        /// <param name="token">JWT Token</param>
        /// <returns>解析结果</returns>
        public static JwtDecodeResult Decode(string token)
        {
            if (string.IsNullOrEmpty(token))
                throw new ArgumentException("Token cannot be null or empty", nameof(token));

            var parts = token.Split('.');
            if (parts.Length != 3)
                throw new ArgumentException("Invalid token format", nameof(token));

            try
            {
                var header = JsonSerializer.Deserialize<Dictionary<string, object>>(Base64UrlDecode(parts[0]));
                var payload = JsonSerializer.Deserialize<Dictionary<string, object>>(Base64UrlDecode(parts[1]));

                return new JwtDecodeResult
                {
                    Header = header,
                    Payload = payload,
                    Signature = parts[2],
                    IsValid = true
                };
            }
            // 捕获 Base64 解码和 JSON 反序列化异常
            catch (FormatException ex)
            {
                return new JwtDecodeResult
                {
                    IsValid = false,
                    ErrorMessage = ex.Message
                };
            }
            catch (System.Text.Json.JsonException ex)
            {
                return new JwtDecodeResult
                {
                    IsValid = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// 解析并验证 JWT Token
        /// </summary>
        /// <param name="token">JWT Token</param>
        /// <param name="secret">密钥</param>
        /// <returns>解析结果</returns>
        public static JwtDecodeResult Decode(string token, string secret)
        {
            var result = Decode(token);

            if (!result.IsValid)
                return result;

            // 验证签名
            if (!VerifySignature(token, secret, result.Header))
            {
                result.IsValid = false;
                result.ErrorMessage = "Invalid signature";
                return result;
            }

            // 验证过期时间
            if (result.Payload.TryGetValue("exp", out var exp))
            {
                var expTime = FromUnixTimestamp(Convert.ToInt64(exp));
                if (DateTime.UtcNow > expTime)
                {
                    result.IsValid = false;
                    result.ErrorMessage = "Token has expired";
                    result.IsExpired = true;
                    return result;
                }
            }

            // 验证生效时间
            if (result.Payload.TryGetValue("nbf", out var nbf))
            {
                var nbfTime = FromUnixTimestamp(Convert.ToInt64(nbf));
                if (DateTime.UtcNow < nbfTime)
                {
                    result.IsValid = false;
                    result.ErrorMessage = "Token is not yet valid";
                    return result;
                }
            }

            return result;
        }

        /// <summary>
        /// 解析并验证 JWT Token（带完整验证）
        /// </summary>
        /// <param name="token">JWT Token</param>
        /// <param name="secret">密钥</param>
        /// <param name="issuer">预期签发者</param>
        /// <param name="audience">预期受众</param>
        /// <returns>解析结果</returns>
        public static JwtDecodeResult Decode(string token, string secret, string issuer, string audience)
        {
            var result = Decode(token, secret);

            if (!result.IsValid)
                return result;

            // 验证签发者
            if (!string.IsNullOrEmpty(issuer) && result.Payload.TryGetValue("iss", out var iss))
            {
                if (iss.ToString() != issuer)
                {
                    result.IsValid = false;
                    result.ErrorMessage = "Invalid issuer";
                    return result;
                }
            }

            // 验证受众
            if (!string.IsNullOrEmpty(audience) && result.Payload.TryGetValue("aud", out var aud))
            {
                var audList = aud as JsonElement?;
                if (audList != null && audList.Value.ValueKind == JsonValueKind.Array)
                {
                    var audiences = audList.Value.EnumerateArray().Select(a => a.GetString()).ToList();
                    if (!audiences.Contains(audience))
                    {
                        result.IsValid = false;
                        result.ErrorMessage = "Invalid audience";
                        return result;
                    }
                }
                else if (aud.ToString() != audience)
                {
                    result.IsValid = false;
                    result.ErrorMessage = "Invalid audience";
                    return result;
                }
            }

            return result;
        }

        /// <summary>
        /// 获取 JWT Token 的 Payload（不验证）
        /// </summary>
        /// <typeparam name="T">Payload 类型</typeparam>
        /// <param name="token">JWT Token</param>
        /// <returns>Payload 对象</returns>
        public static T GetPayload<T>(string token)
        {
            var result = Decode(token);
            if (!result.IsValid)
                throw new ArgumentException(result.ErrorMessage);

            var json = JsonSerializer.Serialize(result.Payload);
            return JsonSerializer.Deserialize<T>(json);
        }

        #endregion

        #region JWT 验证

        /// <summary>
        /// 验证 JWT Token
        /// </summary>
        /// <param name="token">JWT Token</param>
        /// <param name="secret">密钥</param>
        /// <returns>是否有效</returns>
        public static bool Verify(string token, string secret)
        {
            var result = Decode(token, secret);
            return result.IsValid;
        }

        /// <summary>
        /// 验证 JWT Token 签名
        /// </summary>
        /// <param name="token">JWT Token</param>
        /// <param name="secret">密钥</param>
        /// <returns>签名是否有效</returns>
        public static bool VerifySignature(string token, string secret)
        {
            var result = Decode(token);
            if (!result.IsValid)
                return false;

            return VerifySignature(token, secret, result.Header);
        }

        private static bool VerifySignature(string token, string secret, Dictionary<string, object> header)
        {
            var parts = token.Split('.');
            if (parts.Length != 3)
                return false;

            var alg = header["alg"].ToString();
            var algorithm = Enum.Parse<JwtAlgorithm>(alg);

            string expectedSignature = CreateSignature($"{parts[0]}.{parts[1]}", secret, algorithm);
            return expectedSignature == parts[2];
        }

        /// <summary>
        /// 检查 Token 是否过期
        /// </summary>
        /// <param name="token">JWT Token</param>
        /// <returns>是否过期</returns>
        public static bool IsExpired(string token)
        {
            var result = Decode(token);
            if (!result.IsValid)
                return true;

            if (result.Payload.TryGetValue("exp", out var exp))
            {
                var expTime = FromUnixTimestamp(Convert.ToInt64(exp));
                return DateTime.UtcNow > expTime;
            }

            return false;
        }

        /// <summary>
        /// 获取 Token 剩余有效时间
        /// </summary>
        /// <param name="token">JWT Token</param>
        /// <returns>剩余时间（如果没有过期时间则返回null）</returns>
        public static TimeSpan? GetRemainingTime(string token)
        {
            var result = Decode(token);
            if (!result.IsValid)
                return null;

            if (result.Payload.TryGetValue("exp", out var exp))
            {
                var expTime = FromUnixTimestamp(Convert.ToInt64(exp));
                var remaining = expTime - DateTime.UtcNow;
                return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
            }

            return null;
        }

        #endregion

        #region JWT 刷新

        /// <summary>
        /// 刷新 Token（如果有效且未过期太久）
        /// </summary>
        /// <param name="token">旧 Token</param>
        /// <param name="secret">密钥</param>
        /// <param name="expiration">新过期时间</param>
        /// <param name="maxRefreshDelay">最大刷新延迟（超过此时间不允许刷新）</param>
        /// <returns>新 Token，如果无法刷新则返回null</returns>
        public static string Refresh(string token, string secret, TimeSpan expiration, TimeSpan? maxRefreshDelay = null)
        {
            var result = Decode(token);

            if (!result.IsValid)
                return null;

            // 验证签名
            if (!VerifySignature(token, secret, result.Header))
                return null;

            // 检查是否超出最大刷新延迟
            if (maxRefreshDelay.HasValue && result.Payload.TryGetValue("exp", out var exp))
            {
                var expTime = FromUnixTimestamp(Convert.ToInt64(exp));
                if (DateTime.UtcNow - expTime > maxRefreshDelay.Value)
                    return null;
            }

            // 移除旧的时间戳和ID
            var newPayload = new Dictionary<string, object>(result.Payload);
            newPayload.Remove("iat");
            newPayload.Remove("exp");
            newPayload.Remove("nbf");
            newPayload.Remove("jti");

            // 生成新 Token
            var alg = result.Header["alg"].ToString();
            var algorithm = Enum.Parse<JwtAlgorithm>(alg);

            return Encode(newPayload, secret, expiration, algorithm);
        }

        #endregion

        #region 私有方法

        private static string CreateSignature(string input, string secret, JwtAlgorithm algorithm)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(secret);
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);

            using var hmac = algorithm switch
            {
                JwtAlgorithm.HS256 => new HMACSHA256(keyBytes) as HMAC,
                JwtAlgorithm.HS384 => new HMACSHA384(keyBytes),
                JwtAlgorithm.HS512 => new HMACSHA512(keyBytes),
                _ => throw new ArgumentException($"Unsupported algorithm: {algorithm}")
            };

            byte[] hash = hmac.ComputeHash(inputBytes);
            return Base64UrlEncode(hash);
        }

        private static string Base64UrlEncode(string input)
        {
            return Base64UrlEncode(Encoding.UTF8.GetBytes(input));
        }

        private static string Base64UrlEncode(byte[] input)
        {
            return Convert.ToBase64String(input)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        private static string Base64UrlDecode(string input)
        {
            string output = input
                .Replace('-', '+')
                .Replace('_', '/');

            switch (output.Length % 4)
            {
                case 0: break;
                case 2: output += "=="; break;
                case 3: output += "="; break;
                default: throw new ArgumentException("Invalid base64url string");
            }

            return Encoding.UTF8.GetString(Convert.FromBase64String(output));
        }

        private static Dictionary<string, object> ObjectToDictionary(object obj)
        {
            var json = JsonSerializer.Serialize(obj);
            return JsonSerializer.Deserialize<Dictionary<string, object>>(json);
        }

        private static long ToUnixTimestamp(DateTime dateTime)
        {
            return (long)(dateTime - Epoch).TotalSeconds;
        }

        private static DateTime FromUnixTimestamp(long timestamp)
        {
            return Epoch.AddSeconds(timestamp);
        }

        #endregion
    }

    /// <summary>
    /// JWT 算法
    /// </summary>
    public enum JwtAlgorithm
    {
        HS256,
        HS384,
        HS512
    }

    /// <summary>
    /// JWT 解析结果
    /// </summary>
    public class JwtDecodeResult
    {
        /// <summary>
        /// JWT Header
        /// </summary>
        public Dictionary<string, object> Header { get; set; }

        /// <summary>
        /// JWT Payload
        /// </summary>
        public Dictionary<string, object> Payload { get; set; }

        /// <summary>
        /// 签名
        /// </summary>
        public string Signature { get; set; }

        /// <summary>
        /// 是否有效
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// 是否过期
        /// </summary>
        public bool IsExpired { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string? ErrorMessage { get; set; }
    }
}
