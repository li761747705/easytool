using System;
using System.Security.Cryptography;

namespace EasyTool.CodeCategory
{
    /// <summary>
    /// AEAD（认证加密）工具类
    /// 提供带有关联数据的认证加密功能
    /// </summary>
    public static class AeadUtil
    {
        #region AES-GCM

        /// <summary>
        /// 使用 AES-GCM 加密
        /// </summary>
        /// <param name="plaintext">明文</param>
        /// <param name="key">密钥（16/24/32 字节）</param>
        /// <param name="nonce">随机数（12 字节推荐）</param>
        /// <param name="associatedData">关联数据</param>
        /// <returns>加密结果（密文 + 标签）</returns>
        public static AeadResult EncryptAesGcm(byte[] plaintext, byte[] key, byte[]? nonce = null, byte[]? associatedData = null)
        {
            nonce ??= GenerateNonce(12);

            var ciphertext = new byte[plaintext.Length];
            var tag = new byte[16];

#if NETSTANDARD2_1
            using var aesGcm = new AesGcm(key);
            aesGcm.Encrypt(nonce, plaintext, ciphertext, tag);
#else
            using var aesGcm = new AesGcm(key, 16);
            aesGcm.Encrypt(nonce, plaintext, ciphertext, tag, associatedData);
#endif

            return new AeadResult
            {
                Ciphertext = ciphertext,
                Nonce = nonce,
                Tag = tag
            };
        }

        /// <summary>
        /// 使用 AES-GCM 解密
        /// </summary>
        /// <param name="ciphertext">密文</param>
        /// <param name="key">密钥</param>
        /// <param name="nonce">随机数</param>
        /// <param name="tag">认证标签</param>
        /// <param name="associatedData">关联数据</param>
        /// <returns>明文</returns>
        public static byte[] DecryptAesGcm(byte[] ciphertext, byte[] key, byte[] nonce, byte[] tag, byte[]? associatedData = null)
        {
            var plaintext = new byte[ciphertext.Length];

#if NETSTANDARD2_1
            using var aesGcm = new AesGcm(key);
            aesGcm.Decrypt(nonce, ciphertext, tag, plaintext);
#else
            using var aesGcm = new AesGcm(key, 16);
            aesGcm.Decrypt(nonce, ciphertext, tag, plaintext, associatedData);
#endif

            return plaintext;
        }

        /// <summary>
        /// 使用 AES-GCM 解密（使用 AeadResult）
        /// </summary>
        public static byte[] DecryptAesGcm(AeadResult encrypted, byte[] key, byte[]? associatedData = null)
        {
            return DecryptAesGcm(encrypted.Ciphertext, key, encrypted.Nonce, encrypted.Tag, associatedData);
        }

        #endregion

        #region ChaCha20-Poly1305

#if NET5_0_OR_GREATER
        /// <summary>
        /// 使用 ChaCha20-Poly1305 加密
        /// </summary>
        /// <param name="plaintext">明文</param>
        /// <param name="key">密钥（32 字节）</param>
        /// <param name="nonce">随机数（12 字节）</param>
        /// <param name="associatedData">关联数据</param>
        /// <returns>加密结果</returns>
        public static AeadResult EncryptChaCha20Poly1305(byte[] plaintext, byte[] key, byte[]? nonce = null, byte[]? associatedData = null)
        {
            nonce ??= GenerateNonce(12);

            var ciphertext = new byte[plaintext.Length];
            var tag = new byte[16];

            using var chaCha20Poly1305 = new ChaCha20Poly1305(key);
            chaCha20Poly1305.Encrypt(nonce, plaintext, ciphertext, tag, associatedData);

            return new AeadResult
            {
                Ciphertext = ciphertext,
                Nonce = nonce,
                Tag = tag
            };
        }

        /// <summary>
        /// 使用 ChaCha20-Poly1305 解密
        /// </summary>
        public static byte[] DecryptChaCha20Poly1305(byte[] ciphertext, byte[] key, byte[] nonce, byte[] tag, byte[]? associatedData = null)
        {
            var plaintext = new byte[ciphertext.Length];

            using var chaCha20Poly1305 = new ChaCha20Poly1305(key);
            chaCha20Poly1305.Decrypt(nonce, ciphertext, tag, plaintext, associatedData);

            return plaintext;
        }

        /// <summary>
        /// 使用 ChaCha20-Poly1305 解密（使用 AeadResult）
        /// </summary>
        public static byte[] DecryptChaCha20Poly1305(AeadResult encrypted, byte[] key, byte[]? associatedData = null)
        {
            return DecryptChaCha20Poly1305(encrypted.Ciphertext, key, encrypted.Nonce, encrypted.Tag, associatedData);
        }
#endif

        #endregion

        #region 密钥和随机数生成

        /// <summary>
        /// 生成 AES 密钥
        /// </summary>
        /// <param name="keySize">密钥大小（128/192/256 位）</param>
        /// <returns>密钥</returns>
        public static byte[] GenerateAesKey(int keySize = 256)
        {
            int keyBytes = keySize / 8;
            using var rng = RandomNumberGenerator.Create();
            var key = new byte[keyBytes];
            rng.GetBytes(key);
            return key;
        }

        /// <summary>
        /// 生成随机数（Nonce）
        /// </summary>
        /// <param name="size">大小（字节），默认 12</param>
        /// <returns>随机数</returns>
        public static byte[] GenerateNonce(int size = 12)
        {
            using var rng = RandomNumberGenerator.Create();
            var nonce = new byte[size];
            rng.GetBytes(nonce);
            return nonce;
        }

        #endregion

        #region 便捷方法

        /// <summary>
        /// 简化的加密（自动生成密钥和随机数）
        /// </summary>
        /// <param name="plaintext">明文</param>
        /// <param name="key">密钥（可选，自动生成）</param>
        /// <returns>加密结果</returns>
        public static (AeadResult Result, byte[] Key) EncryptSimple(byte[] plaintext, byte[]? key = null)
        {
            key ??= GenerateAesKey(256);
            var result = EncryptAesGcm(plaintext, key);
            return (result, key);
        }

        /// <summary>
        /// 简化的解密
        /// </summary>
        /// <param name="encrypted">加密结果</param>
        /// <param name="key">密钥</param>
        /// <returns>明文</returns>
        public static byte[] DecryptSimple(AeadResult encrypted, byte[] key)
        {
            return DecryptAesGcm(encrypted, key);
        }

        /// <summary>
        /// 加密字符串
        /// </summary>
        /// <param name="plaintext">明文字符串</param>
        /// <param name="keyBase64">Base64 编码的密钥</param>
        /// <returns>Base64 编码的加密结果</returns>
        public static string EncryptString(string plaintext, string keyBase64)
        {
            var key = Convert.FromBase64String(keyBase64);
            var plaintextBytes = System.Text.Encoding.UTF8.GetBytes(plaintext);
            var result = EncryptAesGcm(plaintextBytes, key);
            return result.ToBase64();
        }

        /// <summary>
        /// 解密字符串
        /// </summary>
        /// <param name="ciphertextBase64">Base64 编码的加密结果</param>
        /// <param name="keyBase64">Base64 编码的密钥</param>
        /// <returns>明文字符串</returns>
        public static string DecryptString(string ciphertextBase64, string keyBase64)
        {
            var key = Convert.FromBase64String(keyBase64);
            var encrypted = AeadResult.FromBase64(ciphertextBase64);
            var plaintextBytes = DecryptAesGcm(encrypted, key);
            return System.Text.Encoding.UTF8.GetString(plaintextBytes);
        }

        #endregion
    }

    /// <summary>
    /// AEAD 加密结果
    /// </summary>
    public class AeadResult
    {
        /// <summary>
        /// 密文
        /// </summary>
        public byte[] Ciphertext { get; set; } = Array.Empty<byte>();

        /// <summary>
        /// 随机数（Nonce）
        /// </summary>
        public byte[] Nonce { get; set; } = Array.Empty<byte>();

        /// <summary>
        /// 认证标签
        /// </summary>
        public byte[] Tag { get; set; } = Array.Empty<byte>();

        /// <summary>
        /// 获取完整的加密数据（Nonce + Tag + Ciphertext）
        /// </summary>
        /// <returns>完整数据</returns>
        public byte[] ToCombinedBytes()
        {
            var result = new byte[Nonce.Length + Tag.Length + Ciphertext.Length];
            Buffer.BlockCopy(Nonce, 0, result, 0, Nonce.Length);
            Buffer.BlockCopy(Tag, 0, result, Nonce.Length, Tag.Length);
            Buffer.BlockCopy(Ciphertext, 0, result, Nonce.Length + Tag.Length, Ciphertext.Length);
            return result;
        }

        /// <summary>
        /// 从完整数据解析
        /// </summary>
        /// <param name="combined">完整数据</param>
        /// <param name="nonceSize">Nonce 大小</param>
        /// <param name="tagSize">Tag 大小</param>
        /// <returns>AEAD 结果</returns>
        public static AeadResult FromCombinedBytes(byte[] combined, int nonceSize = 12, int tagSize = 16)
        {
            var nonce = new byte[nonceSize];
            var tag = new byte[tagSize];
            var ciphertext = new byte[combined.Length - nonceSize - tagSize];

            Buffer.BlockCopy(combined, 0, nonce, 0, nonceSize);
            Buffer.BlockCopy(combined, nonceSize, tag, 0, tagSize);
            Buffer.BlockCopy(combined, nonceSize + tagSize, ciphertext, 0, ciphertext.Length);

            return new AeadResult
            {
                Nonce = nonce,
                Tag = tag,
                Ciphertext = ciphertext
            };
        }

        /// <summary>
        /// 转换为 Base64 字符串
        /// </summary>
        public string ToBase64()
        {
            return Convert.ToBase64String(ToCombinedBytes());
        }

        /// <summary>
        /// 从 Base64 字符串解析
        /// </summary>
        public static AeadResult FromBase64(string base64)
        {
            var combined = Convert.FromBase64String(base64);
            return FromCombinedBytes(combined);
        }
    }
}
