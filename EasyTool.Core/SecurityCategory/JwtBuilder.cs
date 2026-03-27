using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace EasyTool.SecurityCategory
{
    /// <summary>
    /// JWT 构建器
    /// 提供流畅的 JWT 生成接口
    /// </summary>
    public class JwtBuilder
    {
        private readonly List<Claim> _claims;
        private string? _issuer;
        private string? _audience;
        private DateTime? _notBefore;
        private DateTime? _expires;
        private DateTime? _issuedAt;
        private string? _subject;
        private string? _id;
        private SecurityKey? _signingKey;
        private SecurityKey? _encryptingKey;
        private string _algorithm = SecurityAlgorithms.HmacSha256;
        private SigningCredentials? _signingCredentials;
        private EncryptingCredentials? _encryptingCredentials;

        /// <summary>
        /// 创建 JWT 构建器
        /// </summary>
        public JwtBuilder()
        {
            _claims = new List<Claim>();
        }

        /// <summary>
        /// 设置颁发者
        /// </summary>
        /// <param name="issuer">颁发者</param>
        /// <returns>JwtBuilder</returns>
        public JwtBuilder WithIssuer(string issuer)
        {
            _issuer = issuer;
            return this;
        }

        /// <summary>
        /// 设置受众
        /// </summary>
        /// <param name="audience">受众</param>
        /// <returns>JwtBuilder</returns>
        public JwtBuilder WithAudience(string audience)
        {
            _audience = audience;
            return this;
        }

        /// <summary>
        /// 设置主题
        /// </summary>
        /// <param name="subject">主题</param>
        /// <returns>JwtBuilder</returns>
        public JwtBuilder WithSubject(string subject)
        {
            _subject = subject;
            return this;
        }

        /// <summary>
        /// 设置 JWT ID
        /// </summary>
        /// <param name="id">ID</param>
        /// <returns>JwtBuilder</returns>
        public JwtBuilder WithId(string id)
        {
            _id = id;
            return this;
        }

        /// <summary>
        /// 设置生效时间
        /// </summary>
        /// <param name="notBefore">生效时间</param>
        /// <returns>JwtBuilder</returns>
        public JwtBuilder WithNotBefore(DateTime notBefore)
        {
            _notBefore = notBefore;
            return this;
        }

        /// <summary>
        /// 设置过期时间
        /// </summary>
        /// <param name="expires">过期时间</param>
        /// <returns>JwtBuilder</returns>
        public JwtBuilder WithExpires(DateTime expires)
        {
            _expires = expires;
            return this;
        }

        /// <summary>
        /// 设置过期时间（相对）
        /// </summary>
        /// <param name="duration">有效时长</param>
        /// <returns>JwtBuilder</returns>
        public JwtBuilder WithExpiresIn(TimeSpan duration)
        {
            _expires = DateTime.UtcNow.Add(duration);
            return this;
        }

        /// <summary>
        /// 设置签发时间
        /// </summary>
        /// <param name="issuedAt">签发时间</param>
        /// <returns>JwtBuilder</returns>
        public JwtBuilder WithIssuedAt(DateTime issuedAt)
        {
            _issuedAt = issuedAt;
            return this;
        }

        /// <summary>
        /// 添加声明
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="value">值</param>
        /// <returns>JwtBuilder</returns>
        public JwtBuilder WithClaim(string type, string value)
        {
            _claims.Add(new Claim(type, value));
            return this;
        }

        /// <summary>
        /// 添加声明
        /// </summary>
        /// <param name="claim">声明</param>
        /// <returns>JwtBuilder</returns>
        public JwtBuilder WithClaim(Claim claim)
        {
            _claims.Add(claim);
            return this;
        }

        /// <summary>
        /// 批量添加声明
        /// </summary>
        /// <param name="claims">声明集合</param>
        /// <returns>JwtBuilder</returns>
        public JwtBuilder WithClaims(IEnumerable<Claim> claims)
        {
            _claims.AddRange(claims);
            return this;
        }

        /// <summary>
        /// 设置用户ID声明
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>JwtBuilder</returns>
        public JwtBuilder WithUserId(string userId)
        {
            return WithClaim(JwtRegisteredClaimNames.Sub, userId);
        }

        /// <summary>
        /// 设置用户名声明
        /// </summary>
        /// <param name="username">用户名</param>
        /// <returns>JwtBuilder</returns>
        public JwtBuilder WithUsername(string username)
        {
            return WithClaim(ClaimTypes.Name, username);
        }

        /// <summary>
        /// 设置角色声明
        /// </summary>
        /// <param name="roles">角色列表</param>
        /// <returns>JwtBuilder</returns>
        public JwtBuilder WithRoles(params string[] roles)
        {
            foreach (var role in roles)
            {
                _claims.Add(new Claim(ClaimTypes.Role, role));
            }
            return this;
        }

        /// <summary>
        /// 设置邮箱声明
        /// </summary>
        /// <param name="email">邮箱</param>
        /// <returns>JwtBuilder</returns>
        public JwtBuilder WithEmail(string email)
        {
            return WithClaim(ClaimTypes.Email, email);
        }

        /// <summary>
        /// 设置签名密钥（字符串）
        /// </summary>
        /// <param name="secretKey">密钥字符串</param>
        /// <returns>JwtBuilder</returns>
        public JwtBuilder WithSecretKey(string secretKey)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            return WithSigningKey(key);
        }

        /// <summary>
        /// 设置签名密钥
        /// </summary>
        /// <param name="key">密钥</param>
        /// <returns>JwtBuilder</returns>
        public JwtBuilder WithSigningKey(SecurityKey key)
        {
            _signingKey = key;
            _signingCredentials = new SigningCredentials(key, _algorithm);
            return this;
        }

        /// <summary>
        /// 设置签名密钥（RSA）
        /// </summary>
        /// <param name="rsa">RSA 密钥</param>
        /// <param name="algorithm">算法</param>
        /// <returns>JwtBuilder</returns>
        public JwtBuilder WithRsaKey(RSA rsa, string algorithm = SecurityAlgorithms.RsaSha256)
        {
            var key = new RsaSecurityKey(rsa);
            _algorithm = algorithm;
            _signingCredentials = new SigningCredentials(key, algorithm);
            return this;
        }

        /// <summary>
        /// 设置签名凭据
        /// </summary>
        /// <param name="credentials">签名凭据</param>
        /// <returns>JwtBuilder</returns>
        public JwtBuilder WithSigningCredentials(SigningCredentials credentials)
        {
            _signingCredentials = credentials;
            return this;
        }

        /// <summary>
        /// 设置加密密钥
        /// </summary>
        /// <param name="key">加密密钥</param>
        /// <param name="algorithm">加密算法</param>
        /// <returns>JwtBuilder</returns>
        public JwtBuilder WithEncryptingKey(SecurityKey key, string algorithm = SecurityAlgorithms.Aes256CbcHmacSha512)
        {
            _encryptingKey = key;
            if (key is SymmetricSecurityKey symmetricKey)
            {
                _encryptingCredentials = new EncryptingCredentials(symmetricKey, algorithm);
            }
            else
            {
                throw new ArgumentException("加密密钥必须是 SymmetricSecurityKey 类型", nameof(key));
            }
            return this;
        }

        /// <summary>
        /// 设置签名算法
        /// </summary>
        /// <param name="algorithm">算法</param>
        /// <returns>JwtBuilder</returns>
        public JwtBuilder WithAlgorithm(string algorithm)
        {
            _algorithm = algorithm;
            if (_signingKey != null)
            {
                _signingCredentials = new SigningCredentials(_signingKey, algorithm);
            }
            return this;
        }

        /// <summary>
        /// 构建 JWT Token
        /// </summary>
        /// <returns>JWT 字符串</returns>
        public string Build()
        {
            if (_signingCredentials == null)
                throw new InvalidOperationException("必须设置签名密钥");

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(_claims),
                Issuer = _issuer,
                Audience = _audience,
                NotBefore = _notBefore,
                Expires = _expires,
                IssuedAt = _issuedAt,
                SigningCredentials = _signingCredentials,
                EncryptingCredentials = _encryptingCredentials
            };

            if (!string.IsNullOrEmpty(_subject))
            {
                tokenDescriptor.Subject.AddClaim(new Claim(JwtRegisteredClaimNames.Sub, _subject));
            }

            if (!string.IsNullOrEmpty(_id))
            {
                tokenDescriptor.Subject.AddClaim(new Claim(JwtRegisteredClaimNames.Jti, _id));
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// 构建并返回 Token 及其信息
        /// </summary>
        /// <returns>Token 信息</returns>
        public JwtTokenInfo BuildWithInfo()
        {
            var token = Build();
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            return new JwtTokenInfo
            {
                Token = token,
                Header = jwtToken.Header,
                Payload = jwtToken.Payload,
                Claims = jwtToken.Claims,
                ValidFrom = jwtToken.ValidFrom,
                ValidTo = jwtToken.ValidTo,
                Issuer = jwtToken.Issuer,
                Audiences = jwtToken.Audiences
            };
        }
    }

    /// <summary>
    /// JWT Token 信息
    /// </summary>
    public class JwtTokenInfo
    {
        /// <summary>
        /// Token 字符串
        /// </summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// 头部
        /// </summary>
        public JwtHeader Header { get; set; } = null!;

        /// <summary>
        /// 载荷
        /// </summary>
        public JwtPayload Payload { get; set; } = null!;

        /// <summary>
        /// 声明集合
        /// </summary>
        public IEnumerable<Claim> Claims { get; set; } = Array.Empty<Claim>();

        /// <summary>
        /// 生效时间
        /// </summary>
        public DateTime ValidFrom { get; set; }

        /// <summary>
        /// 过期时间
        /// </summary>
        public DateTime ValidTo { get; set; }

        /// <summary>
        /// 颁发者
        /// </summary>
        public string? Issuer { get; set; }

        /// <summary>
        /// 受众
        /// </summary>
        public IEnumerable<string> Audiences { get; set; } = Array.Empty<string>();
    }

    /// <summary>
    /// JWT 验证结果
    /// </summary>
    public class JwtValidationResult
    {
        /// <summary>
        /// 是否有效
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 声明主体
        /// </summary>
        public ClaimsPrincipal? Principal { get; set; }

        /// <summary>
        /// Token 信息
        /// </summary>
        public JwtSecurityToken? Token { get; set; }
    }

    /// <summary>
    /// JWT 工具类
    /// </summary>
    public static class JwtUtil
    {
        /// <summary>
        /// 创建 JWT 构建器
        /// </summary>
        /// <returns>JwtBuilder</returns>
        public static JwtBuilder Create()
        {
            return new JwtBuilder();
        }

        /// <summary>
        /// 快速生成 JWT Token
        /// </summary>
        /// <param name="secretKey">密钥</param>
        /// <param name="claims">声明</param>
        /// <param name="expiresIn">有效时长</param>
        /// <param name="issuer">颁发者</param>
        /// <param name="audience">受众</param>
        /// <returns>JWT Token</returns>
        public static string GenerateToken(
            string secretKey,
            Dictionary<string, string>? claims = null,
            TimeSpan? expiresIn = null,
            string? issuer = null,
            string? audience = null)
        {
            var builder = new JwtBuilder()
                .WithSecretKey(secretKey);

            if (claims != null)
            {
                foreach (var claim in claims)
                {
                    builder.WithClaim(claim.Key, claim.Value);
                }
            }

            if (expiresIn.HasValue)
            {
                builder.WithExpiresIn(expiresIn.Value);
            }

            if (!string.IsNullOrEmpty(issuer))
            {
                builder.WithIssuer(issuer);
            }

            if (!string.IsNullOrEmpty(audience))
            {
                builder.WithAudience(audience);
            }

            return builder.Build();
        }

        /// <summary>
        /// 验证 JWT Token
        /// </summary>
        /// <param name="token">Token</param>
        /// <param name="secretKey">密钥</param>
        /// <param name="issuer">颁发者</param>
        /// <param name="audience">受众</param>
        /// <returns>验证结果</returns>
        public static JwtValidationResult Validate(
            string token,
            string secretKey,
            string? issuer = null,
            string? audience = null)
        {
            try
            {
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = !string.IsNullOrEmpty(issuer),
                    ValidIssuer = issuer,
                    ValidateAudience = !string.IsNullOrEmpty(audience),
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var handler = new JwtSecurityTokenHandler();
                var principal = handler.ValidateToken(token, validationParameters, out var validatedToken);

                return new JwtValidationResult
                {
                    IsValid = true,
                    Principal = principal,
                    Token = validatedToken as JwtSecurityToken
                };
            }
            catch (Exception ex)
            {
                return new JwtValidationResult
                {
                    IsValid = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// 解析 JWT Token（不验证）
        /// </summary>
        /// <param name="token">Token</param>
        /// <returns>Token 信息</returns>
        public static JwtSecurityToken Parse(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            return handler.ReadJwtToken(token);
        }

        /// <summary>
        /// 获取 Token 中的声明
        /// </summary>
        /// <param name="token">Token</param>
        /// <returns>声明集合</returns>
        public static IEnumerable<Claim> GetClaims(string token)
        {
            var jwtToken = Parse(token);
            return jwtToken.Claims;
        }

        /// <summary>
        /// 获取指定声明
        /// </summary>
        /// <param name="token">Token</param>
        /// <param name="claimType">声明类型</param>
        /// <returns>声明值</returns>
        public static string? GetClaim(string token, string claimType)
        {
            var claims = GetClaims(token);
            return claims.FirstOrDefault(c => c.Type == claimType)?.Value;
        }

        /// <summary>
        /// 检查 Token 是否即将过期
        /// </summary>
        /// <param name="token">Token</param>
        /// <param name="threshold">阈值</param>
        /// <returns>是否即将过期</returns>
        public static bool IsExpiringSoon(string token, TimeSpan threshold)
        {
            var jwtToken = Parse(token);
            return jwtToken.ValidTo - DateTime.UtcNow < threshold;
        }

        /// <summary>
        /// 刷新 Token
        /// </summary>
        /// <param name="oldToken">旧 Token</param>
        /// <param name="secretKey">密钥</param>
        /// <param name="expiresIn">新的有效时长</param>
        /// <param name="issuer">颁发者</param>
        /// <param name="audience">受众</param>
        /// <returns>新 Token</returns>
        public static string RefreshToken(
            string oldToken,
            string secretKey,
            TimeSpan? expiresIn = null,
            string? issuer = null,
            string? audience = null)
        {
            var claims = GetClaims(oldToken);
            var builder = new JwtBuilder()
                .WithSecretKey(secretKey)
                .WithClaims(claims);

            if (expiresIn.HasValue)
            {
                builder.WithExpiresIn(expiresIn.Value);
            }

            if (!string.IsNullOrEmpty(issuer))
            {
                builder.WithIssuer(issuer);
            }

            if (!string.IsNullOrEmpty(audience))
            {
                builder.WithAudience(audience);
            }

            return builder.Build();
        }
    }
}
