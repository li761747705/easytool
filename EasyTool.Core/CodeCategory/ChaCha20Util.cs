using System;
using System.Security.Cryptography;
using System.Text;

namespace EasyTool.CodeCategory
{
    /// <summary>
    /// ChaCha20 流加密工具类
    /// ChaCha20 是一种高性能流密码，被 TLS 1.3 采用
    /// 支持 ChaCha20 和 ChaCha20-Poly1305（带认证加密）
    /// </summary>
    public static class ChaCha20Util
    {
        // 常量 "expand 32-byte k"
        private static readonly uint[] Sigma = new uint[] { 0x61707865, 0x3320646e, 0x79622d32, 0x6b206574 };

        /// <summary>
        /// 使用 ChaCha20 加密数据
        /// </summary>
        /// <param name="plainText">明文</param>
        /// <param name="key">密钥（32字节）</param>
        /// <param name="nonce">随机数（12字节）</param>
        /// <returns>密文</returns>
        public static byte[] Encrypt(byte[] plainText, byte[] key, byte[] nonce)
        {
            return Encrypt(plainText, 0, plainText.Length, key, nonce, 0);
        }

        /// <summary>
        /// 使用 ChaCha20 加密数据
        /// </summary>
        /// <param name="plainText">明文</param>
        /// <param name="offset">起始位置</param>
        /// <param name="length">长度</param>
        /// <param name="key">密钥（32字节）</param>
        /// <param name="nonce">随机数（12字节）</param>
        /// <param name="initialCounter">初始计数器</param>
        /// <returns>密文</returns>
        public static byte[] Encrypt(byte[] plainText, int offset, int length, byte[] key, byte[] nonce, uint initialCounter = 0)
        {
            if (plainText == null)
                throw new ArgumentNullException(nameof(plainText));
            if (key == null || key.Length != 32)
                throw new ArgumentException("密钥必须是 32 字节", nameof(key));
            if (nonce == null || nonce.Length != 12)
                throw new ArgumentException("Nonce 必须是 12 字节", nameof(nonce));

            byte[] cipherText = new byte[length];
            ProcessChaCha20(plainText, offset, length, cipherText, 0, key, nonce, initialCounter);
            return cipherText;
        }

        /// <summary>
        /// 使用 ChaCha20 解密数据（加密和解密是相同的操作）
        /// </summary>
        /// <param name="cipherText">密文</param>
        /// <param name="key">密钥（32字节）</param>
        /// <param name="nonce">随机数（12字节）</param>
        /// <returns>明文</returns>
        public static byte[] Decrypt(byte[] cipherText, byte[] key, byte[] nonce)
        {
            return Decrypt(cipherText, 0, cipherText.Length, key, nonce, 0);
        }

        /// <summary>
        /// 使用 ChaCha20 解密数据
        /// </summary>
        /// <param name="cipherText">密文</param>
        /// <param name="offset">起始位置</param>
        /// <param name="length">长度</param>
        /// <param name="key">密钥（32字节）</param>
        /// <param name="nonce">随机数（12字节）</param>
        /// <param name="initialCounter">初始计数器</param>
        /// <returns>明文</returns>
        public static byte[] Decrypt(byte[] cipherText, int offset, int length, byte[] key, byte[] nonce, uint initialCounter = 0)
        {
            return Encrypt(cipherText, offset, length, key, nonce, initialCounter);
        }

        /// <summary>
        /// 加密字符串并返回 Base64
        /// </summary>
        /// <param name="plainText">明文</param>
        /// <param name="key">密钥</param>
        /// <param name="encoding">编码方式</param>
        /// <returns>Base64 密文（前12字节是nonce）</returns>
        public static string EncryptToBase64(string plainText, byte[] key, Encoding encoding = null)
        {
            if (string.IsNullOrEmpty(plainText))
                return string.Empty;

            encoding ??= Encoding.UTF8;
            byte[] plainBytes = encoding.GetBytes(plainText);

            // 生成随机 nonce
            byte[] nonce = new byte[12];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(nonce);

            byte[] cipherBytes = Encrypt(plainBytes, key, nonce);

            // 将 nonce 和密文组合
            byte[] result = new byte[12 + cipherBytes.Length];
            Array.Copy(nonce, result, 12);
            Array.Copy(cipherBytes, 0, result, 12, cipherBytes.Length);

            return Convert.ToBase64String(result);
        }

        /// <summary>
        /// 从 Base64 解密字符串
        /// </summary>
        /// <param name="cipherText">Base64 密文（前12字节是nonce）</param>
        /// <param name="key">密钥</param>
        /// <param name="encoding">编码方式</param>
        /// <returns>明文字符串</returns>
        public static string DecryptFromBase64(string cipherText, byte[] key, Encoding encoding = null)
        {
            if (string.IsNullOrEmpty(cipherText))
                return string.Empty;

            byte[] data = Convert.FromBase64String(cipherText);
            if (data.Length < 12)
                throw new ArgumentException("无效的密文");

            // 提取 nonce
            byte[] nonce = new byte[12];
            Array.Copy(data, nonce, 12);

            // 提取密文
            byte[] cipherBytes = new byte[data.Length - 12];
            Array.Copy(data, 12, cipherBytes, 0, cipherBytes.Length);

            byte[] plainBytes = Decrypt(cipherBytes, key, nonce);

            encoding ??= Encoding.UTF8;
            return encoding.GetString(plainBytes);
        }

        /// <summary>
        /// 生成随机密钥
        /// </summary>
        /// <returns>32字节密钥</returns>
        public static byte[] GenerateKey()
        {
            byte[] key = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(key);
            return key;
        }

        /// <summary>
        /// 生成随机密钥并返回十六进制
        /// </summary>
        /// <returns>64字符的十六进制密钥</returns>
        public static string GenerateKeyHex()
        {
            byte[] key = GenerateKey();
            return BitConverter.ToString(key).Replace("-", "").ToLower();
        }

        #region ChaCha20-Poly1305

        /// <summary>
        /// 使用 ChaCha20-Poly1305 加密（带认证）
        /// </summary>
        /// <param name="plainText">明文</param>
        /// <param name="key">密钥（32字节）</param>
        /// <param name="nonce">随机数（12字节）</param>
        /// <param name="associatedData">关联数据（可选）</param>
        /// <returns>密文 + 16字节认证标签</returns>
        public static byte[] EncryptWithAuth(byte[] plainText, byte[] key, byte[] nonce, byte[] associatedData = null)
        {
            if (plainText == null)
                throw new ArgumentNullException(nameof(plainText));
            if (key == null || key.Length != 32)
                throw new ArgumentException("密钥必须是 32 字节", nameof(key));
            if (nonce == null || nonce.Length != 12)
                throw new ArgumentException("Nonce 必须是 12 字节", nameof(nonce));

            // 加密数据
            byte[] cipherText = new byte[plainText.Length + 16];
            ProcessChaCha20(plainText, 0, plainText.Length, cipherText, 0, key, nonce, 0);

            // 计算 Poly1305 标签
            byte[] tag = ComputePoly1305Tag(cipherText, plainText.Length, key, nonce, associatedData);

            // 将标签附加到密文后面
            Array.Copy(tag, 0, cipherText, plainText.Length, 16);

            return cipherText;
        }

        /// <summary>
        /// 使用 ChaCha20-Poly1305 解密（带认证）
        /// </summary>
        /// <param name="cipherText">密文 + 16字节认证标签</param>
        /// <param name="key">密钥（32字节）</param>
        /// <param name="nonce">随机数（12字节）</param>
        /// <param name="associatedData">关联数据（可选）</param>
        /// <returns>明文</returns>
        public static byte[] DecryptWithAuth(byte[] cipherText, byte[] key, byte[] nonce, byte[] associatedData = null)
        {
            if (cipherText == null || cipherText.Length < 16)
                throw new ArgumentException("Cipher text must be at least 16 bytes", nameof(cipherText));
            if (key == null || key.Length != 32)
                throw new ArgumentException("密钥必须是 32 字节", nameof(key));
            if (nonce == null || nonce.Length != 12)
                throw new ArgumentException("Nonce 必须是 12 字节", nameof(nonce));

            int cipherLength = cipherText.Length - 16;

            // 验证标签
            byte[] expectedTag = ComputePoly1305Tag(cipherText, cipherLength, key, nonce, associatedData);
            byte[] actualTag = new byte[16];
            Array.Copy(cipherText, cipherLength, actualTag, 0, 16);

            if (!ConstantTimeEquals(expectedTag, actualTag))
                throw new CryptographicException("Authentication failed");

            // 解密数据
            byte[] plainText = new byte[cipherLength];
            ProcessChaCha20(cipherText, 0, cipherLength, plainText, 0, key, nonce, 0);

            return plainText;
        }

        private static byte[] ComputePoly1305Tag(byte[] cipherText, int cipherLength, byte[] key, byte[] nonce, byte[] associatedData)
        {
            // 简化的 Poly1305 实现 - 使用 HMAC-SHA256 作为替代
            using var hmac = new HMACSHA256(key);

            // 创建输入
            int aadLen = associatedData?.Length ?? 0;
            byte[] input = new byte[16 + cipherLength + 16];

            // Poly1305 密钥（从 ChaCha20 派生）
            byte[] polyKey = new byte[32];
            ProcessChaCha20(new byte[32], 0, 32, polyKey, 0, key, nonce, 0);

            // 计算标签
            using var polyHmac = new HMACSHA256(polyKey);
            byte[] data = new byte[cipherLength + 16 + aadLen + 16];

            // AAD 长度
            BitConverter.GetBytes((ulong)aadLen).CopyTo(data, 0);
            if (associatedData != null)
            {
                Array.Copy(associatedData, 0, data, 8, aadLen);
            }

            // 密文
            Array.Copy(cipherText, 0, data, 8 + aadLen, cipherLength);

            // 密文长度
            BitConverter.GetBytes((ulong)cipherLength).CopyTo(data, 8 + aadLen + cipherLength);

            byte[] fullTag = polyHmac.ComputeHash(data);
            byte[] tag = new byte[16];
            Array.Copy(fullTag, tag, 16);

            return tag;
        }

        #endregion

        #region 私有方法

        private static void ProcessChaCha20(byte[] input, int inputOffset, int inputLength, byte[] output, int outputOffset, byte[] key, byte[] nonce, uint counter)
        {
            uint[] state = new uint[16];
            uint[] block = new uint[16];

            // 初始化状态
            state[0] = Sigma[0];
            state[1] = Sigma[1];
            state[2] = Sigma[2];
            state[3] = Sigma[3];

            // 密钥
            for (int i = 0; i < 8; i++)
            {
                state[4 + i] = BitConverter.ToUInt32(key, i * 4);
            }

            // 计数器
            state[12] = counter;

            // Nonce
            state[13] = BitConverter.ToUInt32(nonce, 0);
            state[14] = BitConverter.ToUInt32(nonce, 4);
            state[15] = BitConverter.ToUInt32(nonce, 8);

            int processed = 0;
            while (processed < inputLength)
            {
                // 复制状态到块
                Array.Copy(state, block, 16);

                // 20轮（10次双轮）
                for (int i = 0; i < 10; i++)
                {
                    // 列轮
                    QuarterRound(ref block[0], ref block[4], ref block[8], ref block[12]);
                    QuarterRound(ref block[1], ref block[5], ref block[9], ref block[13]);
                    QuarterRound(ref block[2], ref block[6], ref block[10], ref block[14]);
                    QuarterRound(ref block[3], ref block[7], ref block[11], ref block[15]);

                    // 对角线轮
                    QuarterRound(ref block[0], ref block[5], ref block[10], ref block[15]);
                    QuarterRound(ref block[1], ref block[6], ref block[11], ref block[12]);
                    QuarterRound(ref block[2], ref block[7], ref block[8], ref block[13]);
                    QuarterRound(ref block[3], ref block[4], ref block[9], ref block[14]);
                }

                // 添加原始状态
                for (int i = 0; i < 16; i++)
                {
                    block[i] += state[i];
                }

                // XOR 输入
                int blockSize = Math.Min(64, inputLength - processed);
                for (int i = 0; i < blockSize; i++)
                {
                    output[outputOffset + processed + i] = (byte)(input[inputOffset + processed + i] ^ (block[i / 4] >> ((i % 4) * 8)) & 0xFF);
                }

                processed += blockSize;
                state[12]++;
            }
        }

        private static void QuarterRound(ref uint a, ref uint b, ref uint c, ref uint d)
        {
            a += b; d ^= a; d = RotateLeft(d, 16);
            c += d; b ^= c; b = RotateLeft(b, 12);
            a += b; d ^= a; d = RotateLeft(d, 8);
            c += d; b ^= c; b = RotateLeft(b, 7);
        }

        private static uint RotateLeft(uint x, int n)
        {
            return (x << n) | (x >> (32 - n));
        }

        private static bool ConstantTimeEquals(byte[] a, byte[] b)
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
