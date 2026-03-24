using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace EasyTool.CodeCategory
{
    /// <summary>
    /// TOTP（Time-based One-Time Password）和 HOTP（HMAC-based One-Time Password）工具类
    /// 用于生成和验证一次性密码，常用于双因素认证（2FA）
    /// </summary>
    public static class TOTPUtil
    {
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Base32 字符集（用于密钥编码）
        private const string Base32Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

        #region TOTP（基于时间）

        /// <summary>
        /// 生成 TOTP 验证码
        /// </summary>
        /// <param name="secret">密钥（Base32编码）</param>
        /// <param name="digits">验证码位数（默认6位）</param>
        /// <param name="period">时间周期（默认30秒）</param>
        /// <returns>验证码</returns>
        public static string GenerateTOTP(string secret, int digits = 6, int period = 30)
        {
            byte[] key = Base32Decode(secret);
            return GenerateTOTP(key, digits, period);
        }

        /// <summary>
        /// 生成 TOTP 验证码
        /// </summary>
        /// <param name="secret">密钥（字节数组）</param>
        /// <param name="digits">验证码位数</param>
        /// <param name="period">时间周期（秒）</param>
        /// <returns>验证码</returns>
        public static string GenerateTOTP(byte[] secret, int digits = 6, int period = 30)
        {
            long counter = GetCurrentCounter(period);
            return GenerateHOTP(secret, counter, digits);
        }

        /// <summary>
        /// 生成指定时间的 TOTP 验证码
        /// </summary>
        /// <param name="secret">密钥（Base32编码）</param>
        /// <param name="timestamp">时间戳</param>
        /// <param name="digits">验证码位数</param>
        /// <param name="period">时间周期</param>
        /// <returns>验证码</returns>
        public static string GenerateTOTP(string secret, DateTime timestamp, int digits = 6, int period = 30)
        {
            byte[] key = Base32Decode(secret);
            long counter = GetCounter(timestamp, period);
            return GenerateHOTP(key, counter, digits);
        }

        /// <summary>
        /// 验证 TOTP 验证码
        /// </summary>
        /// <param name="secret">密钥（Base32编码）</param>
        /// <param name="code">验证码</param>
        /// <param name="digits">验证码位数</param>
        /// <param name="period">时间周期</param>
        /// <param name="window">允许的时间窗口（前后各多少个周期）</param>
        /// <returns>是否验证通过</returns>
        public static bool VerifyTOTP(string secret, string code, int digits = 6, int period = 30, int window = 1)
        {
            byte[] key = Base32Decode(secret);
            return VerifyTOTP(key, code, digits, period, window);
        }

        /// <summary>
        /// 验证 TOTP 验证码
        /// </summary>
        /// <param name="secret">密钥（字节数组）</param>
        /// <param name="code">验证码</param>
        /// <param name="digits">验证码位数</param>
        /// <param name="period">时间周期</param>
        /// <param name="window">允许的时间窗口</param>
        /// <returns>是否验证通过</returns>
        public static bool VerifyTOTP(byte[] secret, string code, int digits = 6, int period = 30, int window = 1)
        {
            if (string.IsNullOrEmpty(code) || code.Length != digits)
                return false;

            long currentCounter = GetCurrentCounter(period);

            // 检查时间窗口内的所有可能值
            for (int i = -window; i <= window; i++)
            {
                long counter = currentCounter + i;
                string expectedCode = GenerateHOTP(secret, counter, digits);
                if (ConstantTimeEquals(code, expectedCode))
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region HOTP（基于计数器）

        /// <summary>
        /// 生成 HOTP 验证码
        /// </summary>
        /// <param name="secret">密钥（Base32编码）</param>
        /// <param name="counter">计数器值</param>
        /// <param name="digits">验证码位数</param>
        /// <returns>验证码</returns>
        public static string GenerateHOTP(string secret, long counter, int digits = 6)
        {
            byte[] key = Base32Decode(secret);
            return GenerateHOTP(key, counter, digits);
        }

        /// <summary>
        /// 生成 HOTP 验证码
        /// </summary>
        /// <param name="secret">密钥（字节数组）</param>
        /// <param name="counter">计数器值</param>
        /// <param name="digits">验证码位数</param>
        /// <returns>验证码</returns>
        public static string GenerateHOTP(byte[] secret, long counter, int digits = 6)
        {
            // 将计数器转换为大端序字节数组
            byte[] counterBytes = BitConverter.GetBytes(counter);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(counterBytes);
            }

            // 使用 HMAC-SHA1 计算
            using var hmac = new HMACSHA1(secret);
            byte[] hash = hmac.ComputeHash(counterBytes);

            // 动态截断
            int offset = hash[hash.Length - 1] & 0x0F;
            int binaryCode = ((hash[offset] & 0x7F) << 24) |
                             ((hash[offset + 1] & 0xFF) << 16) |
                             ((hash[offset + 2] & 0xFF) << 8) |
                             (hash[offset + 3] & 0xFF);

            int code = binaryCode % (int)Math.Pow(10, digits);

            return code.ToString().PadLeft(digits, '0');
        }

        /// <summary>
        /// 验证 HOTP 验证码
        /// </summary>
        /// <param name="secret">密钥（Base32编码）</param>
        /// <param name="code">验证码</param>
        /// <param name="counter">计数器值</param>
        /// <param name="digits">验证码位数</param>
        /// <param name="window">允许的计数器窗口</param>
        /// <returns>验证结果和下一个计数器值</returns>
        public static (bool Valid, long NextCounter) VerifyHOTP(string secret, string code, long counter, int digits = 6, int window = 10)
        {
            byte[] key = Base32Decode(secret);
            return VerifyHOTP(key, code, counter, digits, window);
        }

        /// <summary>
        /// 验证 HOTP 验证码
        /// </summary>
        /// <param name="secret">密钥（字节数组）</param>
        /// <param name="code">验证码</param>
        /// <param name="counter">计数器值</param>
        /// <param name="digits">验证码位数</param>
        /// <param name="window">允许的计数器窗口</param>
        /// <returns>验证结果和下一个计数器值</returns>
        public static (bool Valid, long NextCounter) VerifyHOTP(byte[] secret, string code, long counter, int digits = 6, int window = 10)
        {
            if (string.IsNullOrEmpty(code) || code.Length != digits)
                return (false, counter);

            for (long i = counter; i < counter + window; i++)
            {
                string expectedCode = GenerateHOTP(secret, i, digits);
                if (ConstantTimeEquals(code, expectedCode))
                {
                    return (true, i + 1);
                }
            }

            return (false, counter);
        }

        #endregion

        #region 密钥生成

        /// <summary>
        /// 生成随机密钥
        /// </summary>
        /// <param name="length">密钥长度（字节，默认20）</param>
        /// <returns>Base32编码的密钥</returns>
        public static string GenerateSecret(int length = 20)
        {
            byte[] bytes = new byte[length];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Base32Encode(bytes);
        }

        /// <summary>
        /// 生成随机密钥（字节数组）
        /// </summary>
        /// <param name="length">密钥长度</param>
        /// <returns>密钥字节数组</returns>
        public static byte[] GenerateSecretBytes(int length = 20)
        {
            byte[] bytes = new byte[length];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return bytes;
        }

        #endregion

        #region URI 生成（用于二维码）

        /// <summary>
        /// 生成 otpauth:// 格式的 URI
        /// </summary>
        /// <param name="issuer">发行者（应用名称）</param>
        /// <param name="account">账户名</param>
        /// <param name="secret">密钥（Base32编码）</param>
        /// <param name="digits">验证码位数</param>
        /// <param name="period">时间周期</param>
        /// <returns>otpauth URI</returns>
        public static string GetOtpAuthUri(string issuer, string account, string secret, int digits = 6, int period = 30)
        {
            string encodedIssuer = Uri.EscapeDataString(issuer);
            string encodedAccount = Uri.EscapeDataString(account);

            return $"otpauth://totp/{encodedIssuer}:{encodedAccount}?secret={secret}&issuer={encodedIssuer}&digits={digits}&period={period}";
        }

        /// <summary>
        /// 生成 HOTP 的 otpauth:// 格式 URI
        /// </summary>
        /// <param name="issuer">发行者</param>
        /// <param name="account">账户名</param>
        /// <param name="secret">密钥</param>
        /// <param name="counter">计数器</param>
        /// <param name="digits">验证码位数</param>
        /// <returns>otpauth URI</returns>
        public static string GetHotpAuthUri(string issuer, string account, string secret, long counter, int digits = 6)
        {
            string encodedIssuer = Uri.EscapeDataString(issuer);
            string encodedAccount = Uri.EscapeDataString(account);

            return $"otpauth://hotp/{encodedIssuer}:{encodedAccount}?secret={secret}&issuer={encodedIssuer}&digits={digits}&counter={counter}";
        }

        #endregion

        #region 时间工具

        /// <summary>
        /// 获取当前计数器值
        /// </summary>
        /// <param name="period">时间周期</param>
        /// <returns>计数器值</returns>
        public static long GetCurrentCounter(int period = 30)
        {
            return GetCounter(DateTime.UtcNow, period);
        }

        /// <summary>
        /// 获取指定时间的计数器值
        /// </summary>
        /// <param name="timestamp">时间戳</param>
        /// <param name="period">时间周期</param>
        /// <returns>计数器值</returns>
        public static long GetCounter(DateTime timestamp, int period = 30)
        {
            long elapsedSeconds = (long)(timestamp.ToUniversalTime() - Epoch).TotalSeconds;
            return elapsedSeconds / period;
        }

        /// <summary>
        /// 获取当前验证码的剩余有效时间
        /// </summary>
        /// <param name="period">时间周期</param>
        /// <returns>剩余秒数</returns>
        public static int GetRemainingSeconds(int period = 30)
        {
            long elapsedSeconds = (long)(DateTime.UtcNow - Epoch).TotalSeconds;
            return period - (int)(elapsedSeconds % period);
        }

        #endregion

        #region 私有方法

        private static string Base32Encode(byte[] data)
        {
            var result = new StringBuilder((data.Length * 8 + 4) / 5);

            int i = 0;
            int remainingBits = 0;
            int currentByte = 0;

            while (i < data.Length || remainingBits > 0)
            {
                if (remainingBits < 5 && i < data.Length)
                {
                    currentByte = (currentByte << 8) | data[i++];
                    remainingBits += 8;
                }

                if (remainingBits >= 5)
                {
                    int index = (currentByte >> (remainingBits - 5)) & 0x1F;
                    result.Append(Base32Chars[index]);
                    remainingBits -= 5;
                }
                else if (remainingBits > 0)
                {
                    int index = (currentByte << (5 - remainingBits)) & 0x1F;
                    result.Append(Base32Chars[index]);
                    remainingBits = 0;
                }
            }

            return result.ToString();
        }

        private static byte[] Base32Decode(string data)
        {
            data = data.ToUpperInvariant().Replace(" ", "").Replace("-", "");

            var result = new List<byte>();
            int currentByte = 0;
            int remainingBits = 0;

            foreach (char c in data)
            {
                int value = Base32Chars.IndexOf(c);
                if (value < 0)
                    continue;

                currentByte = (currentByte << 5) | value;
                remainingBits += 5;

                while (remainingBits >= 8)
                {
                    result.Add((byte)((currentByte >> (remainingBits - 8)) & 0xFF));
                    remainingBits -= 8;
                }
            }

            return result.ToArray();
        }

        private static bool ConstantTimeEquals(string a, string b)
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
