using System;
using System.Security.Cryptography;
using System.Text;

namespace EasyTool.CodeCategory
{
    /// <summary>
    /// HMAC（基于哈希的消息认证码）工具类
    /// 提供各种哈希算法的 HMAC 实现
    /// </summary>
    public static class HmacUtil
    {
        #region HMAC-MD5

        /// <summary>
        /// 计算 HMAC-MD5
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="key">密钥</param>
        /// <returns>HMAC-MD5 哈希值（十六进制字符串）</returns>
        public static string HmacMD5(byte[] data, byte[] key)
        {
            using var hmac = new HMACMD5(key);
            var hash = hmac.ComputeHash(data);
            return HexUtil.BytesToHex(hash);
        }

        /// <summary>
        /// 计算 HMAC-MD5
        /// </summary>
        /// <param name="text">文本</param>
        /// <param name="key">密钥</param>
        /// <param name="encoding">编码</param>
        /// <returns>HMAC-MD5 哈希值（十六进制字符串）</returns>
        public static string HmacMD5(string text, string key, Encoding? encoding = null)
        {
            encoding ??= Encoding.UTF8;
            return HmacMD5(encoding.GetBytes(text), encoding.GetBytes(key));
        }

        /// <summary>
        /// 验证 HMAC-MD5
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="key">密钥</param>
        /// <param name="expectedHash">期望的哈希值（十六进制字符串）</param>
        /// <returns>是否匹配</returns>
        public static bool VerifyHmacMD5(byte[] data, byte[] key, string expectedHash)
        {
            var actualHash = HmacMD5(data, key);
            return string.Equals(actualHash, expectedHash, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region HMAC-SHA1

        /// <summary>
        /// 计算 HMAC-SHA1
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="key">密钥</param>
        /// <returns>HMAC-SHA1 哈希值（十六进制字符串）</returns>
        public static string HmacSHA1(byte[] data, byte[] key)
        {
            using var hmac = new HMACSHA1(key);
            var hash = hmac.ComputeHash(data);
            return HexUtil.BytesToHex(hash);
        }

        /// <summary>
        /// 计算 HMAC-SHA1
        /// </summary>
        /// <param name="text">文本</param>
        /// <param name="key">密钥</param>
        /// <param name="encoding">编码</param>
        /// <returns>HMAC-SHA1 哈希值（十六进制字符串）</returns>
        public static string HmacSHA1(string text, string key, Encoding? encoding = null)
        {
            encoding ??= Encoding.UTF8;
            return HmacSHA1(encoding.GetBytes(text), encoding.GetBytes(key));
        }

        /// <summary>
        /// 验证 HMAC-SHA1
        /// </summary>
        public static bool VerifyHmacSHA1(byte[] data, byte[] key, string expectedHash)
        {
            var actualHash = HmacSHA1(data, key);
            return string.Equals(actualHash, expectedHash, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region HMAC-SHA256

        /// <summary>
        /// 计算 HMAC-SHA256
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="key">密钥</param>
        /// <returns>HMAC-SHA256 哈希值（十六进制字符串）</returns>
        public static string HmacSHA256(byte[] data, byte[] key)
        {
            using var hmac = new HMACSHA256(key);
            var hash = hmac.ComputeHash(data);
            return HexUtil.BytesToHex(hash);
        }

        /// <summary>
        /// 计算 HMAC-SHA256
        /// </summary>
        /// <param name="text">文本</param>
        /// <param name="key">密钥</param>
        /// <param name="encoding">编码</param>
        /// <returns>HMAC-SHA256 哈希值（十六进制字符串）</returns>
        public static string HmacSHA256(string text, string key, Encoding? encoding = null)
        {
            encoding ??= Encoding.UTF8;
            return HmacSHA256(encoding.GetBytes(text), encoding.GetBytes(key));
        }

        /// <summary>
        /// 验证 HMAC-SHA256
        /// </summary>
        public static bool VerifyHmacSHA256(byte[] data, byte[] key, string expectedHash)
        {
            var actualHash = HmacSHA256(data, key);
            return string.Equals(actualHash, expectedHash, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region HMAC-SHA384

        /// <summary>
        /// 计算 HMAC-SHA384
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="key">密钥</param>
        /// <returns>HMAC-SHA384 哈希值（十六进制字符串）</returns>
        public static string HmacSHA384(byte[] data, byte[] key)
        {
            using var hmac = new HMACSHA384(key);
            var hash = hmac.ComputeHash(data);
            return HexUtil.BytesToHex(hash);
        }

        /// <summary>
        /// 计算 HMAC-SHA384
        /// </summary>
        public static string HmacSHA384(string text, string key, Encoding? encoding = null)
        {
            encoding ??= Encoding.UTF8;
            return HmacSHA384(encoding.GetBytes(text), encoding.GetBytes(key));
        }

        /// <summary>
        /// 验证 HMAC-SHA384
        /// </summary>
        public static bool VerifyHmacSHA384(byte[] data, byte[] key, string expectedHash)
        {
            var actualHash = HmacSHA384(data, key);
            return string.Equals(actualHash, expectedHash, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region HMAC-SHA512

        /// <summary>
        /// 计算 HMAC-SHA512
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="key">密钥</param>
        /// <returns>HMAC-SHA512 哈希值（十六进制字符串）</returns>
        public static string HmacSHA512(byte[] data, byte[] key)
        {
            using var hmac = new HMACSHA512(key);
            var hash = hmac.ComputeHash(data);
            return HexUtil.BytesToHex(hash);
        }

        /// <summary>
        /// 计算 HMAC-SHA512
        /// </summary>
        public static string HmacSHA512(string text, string key, Encoding? encoding = null)
        {
            encoding ??= Encoding.UTF8;
            return HmacSHA512(encoding.GetBytes(text), encoding.GetBytes(key));
        }

        /// <summary>
        /// 验证 HMAC-SHA512
        /// </summary>
        public static bool VerifyHmacSHA512(byte[] data, byte[] key, string expectedHash)
        {
            var actualHash = HmacSHA512(data, key);
            return string.Equals(actualHash, expectedHash, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region 通用方法

        /// <summary>
        /// 生成随机密钥
        /// </summary>
        /// <param name="size">密钥大小（字节）</param>
        /// <returns>随机密钥</returns>
        public static byte[] GenerateKey(int size = 32)
        {
            using var rng = RandomNumberGenerator.Create();
            var key = new byte[size];
            rng.GetBytes(key);
            return key;
        }

        /// <summary>
        /// 生成随机密钥（Base64 编码）
        /// </summary>
        /// <param name="size">密钥大小（字节）</param>
        /// <returns>Base64 编码的随机密钥</returns>
        public static string GenerateKeyBase64(int size = 32)
        {
            var key = GenerateKey(size);
            return Convert.ToBase64String(key);
        }

        /// <summary>
        /// 使用时间安全的比较方法验证 HMAC
        /// </summary>
        /// <param name="actual">实际值</param>
        /// <param name="expected">期望值</param>
        /// <returns>是否匹配</returns>
        public static bool ConstantTimeEquals(byte[] actual, byte[] expected)
        {
            if (actual == null || expected == null)
                return false;

            if (actual.Length != expected.Length)
                return false;

            int result = 0;
            for (int i = 0; i < actual.Length; i++)
            {
                result |= actual[i] ^ expected[i];
            }

            return result == 0;
        }

        #endregion
    }
}