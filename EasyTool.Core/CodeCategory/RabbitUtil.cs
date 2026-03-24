using System;
using System.Security.Cryptography;
using System.Text;

namespace EasyTool.CodeCategory
{
    /// <summary>
    /// Rabbit 流加密工具类
    /// Rabbit 是一种高速流密码，设计用于软件实现
    /// 128位密钥，64位IV（可选）
    /// </summary>
    public static class RabbitUtil
    {
        private const int KeySize = 16;
        private const int IvSize = 8;

        /// <summary>
        /// 加密数据
        /// </summary>
        /// <param name="plainText">明文</param>
        /// <param name="key">密钥（16字节）</param>
        /// <param name="iv">初始化向量（8字节，可选）</param>
        /// <returns>密文</returns>
        public static byte[] Encrypt(byte[] plainText, byte[] key, byte[] iv = null)
        {
            if (plainText == null)
                throw new ArgumentNullException(nameof(plainText));
            if (key == null || key.Length != KeySize)
                throw new ArgumentException("Key must be 16 bytes", nameof(key));

            var state = Initialize(key, iv);
            byte[] result = new byte[plainText.Length];

            for (int i = 0; i < plainText.Length; i++)
            {
                if (i % 16 == 0)
                {
                    NextState(state);
                }

                byte keyByte = (byte)(state.S[i % 16] ^ (state.S[(i % 16) + 1] >> 8));
                result[i] = (byte)(plainText[i] ^ keyByte);
            }

            return result;
        }

        /// <summary>
        /// 解密数据（加密和解密相同）
        /// </summary>
        public static byte[] Decrypt(byte[] cipherText, byte[] key, byte[] iv = null)
        {
            return Encrypt(cipherText, key, iv);
        }

        /// <summary>
        /// 加密字符串并返回 Base64（包含 IV）
        /// </summary>
        public static string EncryptToBase64(string plainText, byte[] key)
        {
            if (string.IsNullOrEmpty(plainText))
                return string.Empty;

            byte[] iv = new byte[IvSize];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(iv);

            byte[] data = Encoding.UTF8.GetBytes(plainText);
            byte[] encrypted = Encrypt(data, key, iv);

            byte[] result = new byte[IvSize + encrypted.Length];
            Array.Copy(iv, result, IvSize);
            Array.Copy(encrypted, 0, result, IvSize, encrypted.Length);

            return Convert.ToBase64String(result);
        }

        /// <summary>
        /// 从 Base64 解密字符串（包含 IV）
        /// </summary>
        public static string DecryptFromBase64(string cipherText, byte[] key)
        {
            if (string.IsNullOrEmpty(cipherText))
                return string.Empty;

            byte[] data = Convert.FromBase64String(cipherText);
            if (data.Length < IvSize)
                throw new ArgumentException("Invalid cipher text", nameof(cipherText));

            byte[] iv = new byte[IvSize];
            Array.Copy(data, iv, IvSize);

            byte[] encrypted = new byte[data.Length - IvSize];
            Array.Copy(data, IvSize, encrypted, 0, encrypted.Length);

            byte[] decrypted = Decrypt(encrypted, key, iv);
            return Encoding.UTF8.GetString(decrypted);
        }

        /// <summary>
        /// 生成随机密钥
        /// </summary>
        public static byte[] GenerateKey()
        {
            byte[] key = new byte[KeySize];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(key);
            return key;
        }

        /// <summary>
        /// 生成随机密钥并返回十六进制
        /// </summary>
        public static string GenerateKeyHex()
        {
            byte[] key = GenerateKey();
            return BitConverter.ToString(key).Replace("-", "").ToLower();
        }

        private static RabbitState Initialize(byte[] key, byte[] iv)
        {
            var state = new RabbitState();

            // 密钥初始化
            for (int i = 0; i < 8; i++)
            {
                state.X[i] = (ushort)((key[(i * 2) % 16] << 8) | key[(i * 2 + 1) % 16]);
                state.C[i] = state.X[i];
            }

            state.Carry = 0;

            // 执行4次状态更新
            for (int i = 0; i < 4; i++)
            {
                NextState(state);
            }

            // 复制状态到 C
            for (int i = 0; i < 8; i++)
            {
                state.C[(i + 4) % 8] ^= state.X[i];
            }

            // 如果有 IV，进行 IV 设置
            if (iv != null && iv.Length >= IvSize)
            {
                SetupIv(state, iv);
            }

            // 生成初始密钥流
            NextState(state);
            for (int i = 0; i < 16; i++)
            {
                state.S[i] = 0;
            }
            ExtractKeyStream(state);

            return state;
        }

        private static void SetupIv(RabbitState state, byte[] iv)
        {
            // 将 64 位 IV 映射到计数器
            state.C[0] ^= (ushort)((iv[0] << 8) | iv[1]);
            state.C[1] ^= (ushort)((iv[2] << 8) | iv[3]);
            state.C[2] ^= (ushort)((iv[4] << 8) | iv[5]);
            state.C[3] ^= (ushort)((iv[6] << 8) | iv[7]);
            state.C[4] ^= (ushort)((iv[4] << 8) | iv[5]);
            state.C[5] ^= (ushort)((iv[6] << 8) | iv[7]);
            state.C[6] ^= (ushort)((iv[0] << 8) | iv[1]);
            state.C[7] ^= (ushort)((iv[2] << 8) | iv[3]);

            // 执行4次状态更新
            for (int i = 0; i < 4; i++)
            {
                NextState(state);
            }
        }

        private static void NextState(RabbitState state)
        {
            uint[] g = new uint[8];
            uint[] newC = new uint[8];
            ushort[] newX = new ushort[8];
            uint newCarry;

            // 计数器更新
            uint a = 0x4D34D34D;
            uint b = 0xD34D34D3;
            uint c = 0x34D34D34;

            newC[0] = (uint)((state.C[0] + a + state.Carry) & 0xFFFFFFFF);
            newC[1] = (uint)((state.C[1] + b + (newC[0] < state.C[0] ? 1u : 0)) & 0xFFFFFFFF);
            newC[2] = (uint)((state.C[2] + c + (newC[1] < state.C[1] ? 1u : 0)) & 0xFFFFFFFF);
            newC[3] = (uint)((state.C[3] + a + (newC[2] < state.C[2] ? 1u : 0)) & 0xFFFFFFFF);
            newC[4] = (uint)((state.C[4] + b + (newC[3] < state.C[3] ? 1u : 0)) & 0xFFFFFFFF);
            newC[5] = (uint)((state.C[5] + c + (newC[4] < state.C[4] ? 1u : 0)) & 0xFFFFFFFF);
            newC[6] = (uint)((state.C[6] + a + (newC[5] < state.C[5] ? 1u : 0)) & 0xFFFFFFFF);
            newC[7] = (uint)((state.C[7] + b + (newC[6] < state.C[6] ? 1u : 0)) & 0xFFFFFFFF);

            newCarry = newC[7] < state.C[7] ? 1u : 0u;

            // G 函数
            for (int i = 0; i < 8; i++)
            {
                g[i] = GFunction((ushort)newC[i]);
            }

            // 状态更新
            newX[0] = (ushort)((g[0] + RotateLeft16((ushort)g[7], 16) + RotateLeft16((ushort)g[6], 16)) & 0xFFFF);
            newX[1] = (ushort)((g[1] + RotateLeft16((ushort)g[0], 8) + g[7]) & 0xFFFF);
            newX[2] = (ushort)((g[2] + RotateLeft16((ushort)g[1], 16) + RotateLeft16((ushort)g[0], 16)) & 0xFFFF);
            newX[3] = (ushort)((g[3] + RotateLeft16((ushort)g[2], 8) + g[1]) & 0xFFFF);
            newX[4] = (ushort)((g[4] + RotateLeft16((ushort)g[3], 16) + RotateLeft16((ushort)g[2], 16)) & 0xFFFF);
            newX[5] = (ushort)((g[5] + RotateLeft16((ushort)g[4], 8) + g[3]) & 0xFFFF);
            newX[6] = (ushort)((g[6] + RotateLeft16((ushort)g[5], 16) + RotateLeft16((ushort)g[4], 16)) & 0xFFFF);
            newX[7] = (ushort)((g[7] + RotateLeft16((ushort)g[6], 8) + g[5]) & 0xFFFF);

            for (int i = 0; i < 8; i++)
            {
                state.X[i] = (ushort)(newX[i] & 0xFFFF);
                state.C[i] = (ushort)(newC[i] & 0xFFFF);
            }
            state.Carry = newCarry;

            ExtractKeyStream(state);
        }

        private static uint GFunction(ushort x)
        {
            uint result = (uint)(x * x);
            return (result ^ (result >> 16)) & 0xFFFF;
        }

        private static void ExtractKeyStream(RabbitState state)
        {
            state.S[0] = (byte)(state.X[0] ^ (state.X[5] >> 8));
            state.S[1] = (byte)(state.X[0] >> 8 ^ state.X[3]);
            state.S[2] = (byte)(state.X[2] ^ (state.X[7] >> 8));
            state.S[3] = (byte)(state.X[2] >> 8 ^ state.X[5]);
            state.S[4] = (byte)(state.X[4] ^ (state.X[1] >> 8));
            state.S[5] = (byte)(state.X[4] >> 8 ^ state.X[7]);
            state.S[6] = (byte)(state.X[6] ^ (state.X[3] >> 8));
            state.S[7] = (byte)(state.X[6] >> 8 ^ state.X[1]);
            state.S[8] = (byte)(state.X[0] ^ state.X[5]);
            state.S[9] = (byte)((state.X[0] >> 8) ^ (state.X[3] >> 8));
            state.S[10] = (byte)(state.X[2] ^ state.X[7]);
            state.S[11] = (byte)((state.X[2] >> 8) ^ (state.X[5] >> 8));
            state.S[12] = (byte)(state.X[4] ^ state.X[1]);
            state.S[13] = (byte)((state.X[4] >> 8) ^ (state.X[7] >> 8));
            state.S[14] = (byte)(state.X[6] ^ state.X[3]);
            state.S[15] = (byte)((state.X[6] >> 8) ^ (state.X[1] >> 8));
        }

        private static ushort RotateLeft16(ushort x, int n)
        {
            return (ushort)((x << n) | (x >> (16 - n)));
        }

        private class RabbitState
        {
            public ushort[] X = new ushort[8];
            public ushort[] C = new ushort[8];
            public uint Carry;
            public byte[] S = new byte[16];
        }
    }
}
