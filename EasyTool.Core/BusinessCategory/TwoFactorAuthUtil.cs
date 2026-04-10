using System;
using System.Security.Cryptography;
using System.Text;

namespace EasyTool.BusinessCategory
{
    /// <summary>
    /// 双因素认证工具类
    /// 支持TOTP（基于时间的一次性密码）算法
    /// 兼容 Google Authenticator、Microsoft Authenticator 等应用
    /// </summary>
    public static class TwoFactorAuthUtil
    {
        #region TOTP生成

        /// <summary>
        /// 生成TOTP验证码
        /// </summary>
        /// <param name="secret">密钥（Base32编码）</param>
        /// <param name="digits">验证码位数（默认6位）</param>
        /// <param name="interval">时间间隔（默认30秒）</param>
        /// <returns>验证码</returns>
        public static string GenerateTotp(string secret, int digits = 6, int interval = 30)
        {
            var secretBytes = Base32Decode(secret);
            var counter = GetCurrentCounter(interval);
            return GenerateTotp(secretBytes, counter, digits);
        }

        /// <summary>
        /// 验证TOTP验证码
        /// </summary>
        /// <param name="secret">密钥（Base32编码）</param>
        /// <param name="code">用户输入的验证码</param>
        /// <param name="allowedWindow">允许的时间窗口（前后各几个周期）</param>
        /// <param name="interval">时间间隔（默认30秒）</param>
        /// <returns>是否验证通过</returns>
        public static bool VerifyTotp(string secret, string code, int allowedWindow = 1, int interval = 30)
        {
            if (string.IsNullOrEmpty(code))
                return false;

            var secretBytes = Base32Decode(secret);
            var currentCounter = GetCurrentCounter(interval);

            // 检查当前及前后时间窗口
            for (int i = -allowedWindow; i <= allowedWindow; i++)
            {
                var counter = currentCounter + i;
                var expectedCode = GenerateTotp(secretBytes, counter, code.Length);
                if (TimeConstantEquals(expectedCode, code))
                    return true;
            }

            return false;
        }

        private static string GenerateTotp(byte[] secret, long counter, int digits)
        {
            var counterBytes = BitConverter.GetBytes(counter);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(counterBytes);

            using var hmac = new HMACSHA1(secret);
            var hash = hmac.ComputeHash(counterBytes);

            var offset = hash[^1] & 0x0F;
            var binary = ((hash[offset] & 0x7F) << 24)
                       | ((hash[offset + 1] & 0xFF) << 16)
                       | ((hash[offset + 2] & 0xFF) << 8)
                       | (hash[offset + 3] & 0xFF);

            var code = binary % (int)Math.Pow(10, digits);
            return code.ToString().PadLeft(digits, '0');
        }

        private static long GetCurrentCounter(int interval)
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds() / interval;
        }

        #endregion

        #region 密钥管理

        /// <summary>
        /// 生成随机密钥
        /// </summary>
        /// <param name="length">密钥长度（字节数，默认20）</param>
        /// <returns>Base32编码的密钥</returns>
        public static string GenerateSecret(int length = 20)
        {
            var bytes = new byte[length];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Base32Encode(bytes);
        }

        /// <summary>
        /// 获取剩余有效时间（秒）
        /// </summary>
        /// <param name="interval">时间间隔（默认30秒）</param>
        /// <returns>剩余秒数</returns>
        public static int GetRemainingSeconds(int interval = 30)
        {
            return interval - (int)(DateTimeOffset.UtcNow.ToUnixTimeSeconds() % interval);
        }

        #endregion

        #region URI生成

        /// <summary>
        /// 生成otpauth:// URI（用于二维码）
        /// </summary>
        /// <param name="issuer">发行者（应用名称）</param>
        /// <param name="account">账户名</param>
        /// <param name="secret">密钥</param>
        /// <param name="digits">验证码位数</param>
        /// <param name="interval">时间间隔</param>
        /// <returns>otpauth:// URI</returns>
        public static string GetOtpAuthUri(string issuer, string account, string secret, int digits = 6, int interval = 30)
        {
            return $"otpauth://totp/{Uri.EscapeDataString(issuer)}:{Uri.EscapeDataString(account)}?secret={secret}&issuer={Uri.EscapeDataString(issuer)}&digits={digits}&period={interval}";
        }

        /// <summary>
        /// 生成二维码内容（用于扫码添加到验证器应用）
        /// </summary>
        /// <param name="issuer">发行者（应用名称）</param>
        /// <param name="account">账户名</param>
        /// <param name="secret">密钥</param>
        /// <returns>二维码内容</returns>
        public static string GetQrCodeContent(string issuer, string account, string secret)
        {
            return GetOtpAuthUri(issuer, account, secret);
        }

        #endregion

        #region Base32编解码

        private static readonly string Base32Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

        private static string Base32Encode(byte[] data)
        {
            var result = new StringBuilder();
            for (int i = 0; i < data.Length; i += 5)
            {
                int b0 = data[i];
                int b1 = i + 1 < data.Length ? data[i + 1] : 0;
                int b2 = i + 2 < data.Length ? data[i + 2] : 0;
                int b3 = i + 3 < data.Length ? data[i + 3] : 0;
                int b4 = i + 4 < data.Length ? data[i + 4] : 0;

                result.Append(Base32Chars[b0 >> 3]);
                result.Append(Base32Chars[((b0 & 0x07) << 2) | (b1 >> 6)]);
                result.Append(Base32Chars[(b1 >> 1) & 0x1F]);
                result.Append(Base32Chars[((b1 & 0x01) << 4) | (b2 >> 4)]);
                result.Append(Base32Chars[((b2 & 0x0F) << 1) | (b3 >> 7)]);
                result.Append(Base32Chars[(b3 >> 2) & 0x1F]);
                result.Append(Base32Chars[((b3 & 0x03) << 3) | (b4 >> 5)]);
                result.Append(Base32Chars[b4 & 0x1F]);
            }

            return result.ToString().TrimEnd('A');
        }

        private static byte[] Base32Decode(string input)
        {
            input = input.ToUpper().TrimEnd('=');
            var output = new byte[input.Length * 5 / 8];
            var buffer = new int[8];

            for (int i = 0, j = 0; i < input.Length;)
            {
                for (int k = 0; k < 8 && i < input.Length; k++, i++)
                {
                    buffer[k] = Base32Chars.IndexOf(input[i]);
                    if (buffer[k] < 0 && i < input.Length)
                        buffer[k] = 0;
                }

                output[j++] = (byte)((buffer[0] << 3) | (buffer[1] >> 2));
                output[j++] = (byte)((buffer[1] << 6) | (buffer[2] << 1) | (buffer[3] >> 4));
                output[j++] = (byte)((buffer[3] << 4) | (buffer[4] >> 1));
                output[j++] = (byte)((buffer[4] << 7) | (buffer[5] << 2) | (buffer[6] >> 3));
                output[j++] = (byte)((buffer[6] << 5) | buffer[7]);
            }

            return output;
        }

        #endregion

        #region 安全比较

        /// <summary>
        /// 时间常量比较（防止时序攻击）
        /// </summary>
        private static bool TimeConstantEquals(string a, string b)
        {
            if (a.Length != b.Length)
                return false;

            int result = 0;
            for (int i = 0; i < a.Length; i++)
            {
                result |= a[i] ^ b[i];
            }

            return result == 0;
        }

        #endregion
    }
}