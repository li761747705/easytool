using System;

namespace EasyTool.CodeCategory
{
    /// <summary>
    /// ZUC（祖冲之）流加密工具类
    /// ZUC 是中国自主设计的流密码算法，以中国数学家祖冲之命名
    /// 用于 4G LTE 通信加密，是 3GPP 标准的一部分
    /// </summary>
    public static class ZucUtil
    {
        // S-box
        private static readonly byte[] S0 = new byte[]
        {
            0x3e, 0x72, 0x5b, 0x47, 0x51, 0x9e, 0x23, 0x5e, 0x36, 0x6b, 0x2f, 0x4a, 0x63, 0x21, 0x56, 0x4d,
            0x66, 0x38, 0x00, 0x18, 0x02, 0x19, 0x0b, 0x33, 0x26, 0x5f, 0x58, 0x7d, 0x44, 0x78, 0x0d, 0x54,
            0x70, 0x0c, 0x37, 0x64, 0x3f, 0x16, 0x50, 0x2e, 0x2b, 0x52, 0x08, 0x15, 0x1e, 0x0f, 0x7b, 0x31,
            0x32, 0x13, 0x39, 0x29, 0x0e, 0x28, 0x07, 0x20, 0x03, 0x1f, 0x7a, 0x48, 0x6c, 0x4e, 0x42, 0x1a,
            0x4b, 0x6e, 0x7c, 0x3d, 0x34, 0x3c, 0x4c, 0x05, 0x7e, 0x1c, 0x55, 0x17, 0x6f, 0x3b, 0x2d, 0x5d,
            0x1b, 0x2c, 0x6a, 0x45, 0x30, 0x74, 0x06, 0x22, 0x4f, 0x65, 0x7f, 0x3a, 0x71, 0x5c, 0x10, 0x01,
            0x69, 0x25, 0x14, 0x57, 0x79, 0x60, 0x5a, 0x49, 0x0a, 0x61, 0x73, 0x24, 0x75, 0x41, 0x35, 0x11,
            0x59, 0x68, 0x01, 0x6d, 0x40, 0x27, 0x76, 0x46, 0x04, 0x53, 0x09, 0x77, 0x43, 0x4d, 0x2a, 0x12,
            0x3e, 0x72, 0x5b, 0x47, 0x51, 0x9e, 0x23, 0x5e, 0x36, 0x6b, 0x2f, 0x4a, 0x63, 0x21, 0x56, 0x4d,
            0x66, 0x38, 0x00, 0x18, 0x02, 0x19, 0x0b, 0x33, 0x26, 0x5f, 0x58, 0x7d, 0x44, 0x78, 0x0d, 0x54,
            0x70, 0x0c, 0x37, 0x64, 0x3f, 0x16, 0x50, 0x2e, 0x2b, 0x52, 0x08, 0x15, 0x1e, 0x0f, 0x7b, 0x31,
            0x32, 0x13, 0x39, 0x29, 0x0e, 0x28, 0x07, 0x20, 0x03, 0x1f, 0x7a, 0x48, 0x6c, 0x4e, 0x42, 0x1a,
            0x4b, 0x6e, 0x7c, 0x3d, 0x34, 0x3c, 0x4c, 0x05, 0x7e, 0x1c, 0x55, 0x17, 0x6f, 0x3b, 0x2d, 0x5d,
            0x1b, 0x2c, 0x6a, 0x45, 0x30, 0x74, 0x06, 0x22, 0x4f, 0x65, 0x7f, 0x3a, 0x71, 0x5c, 0x10, 0x01,
            0x69, 0x25, 0x14, 0x57, 0x79, 0x60, 0x5a, 0x49, 0x0a, 0x61, 0x73, 0x24, 0x75, 0x41, 0x35, 0x11,
            0x59, 0x68, 0x01, 0x6d, 0x40, 0x27, 0x76, 0x46, 0x04, 0x53, 0x09, 0x77, 0x43, 0x4d, 0x2a, 0x12
        };

        private static readonly byte[] S1 = new byte[]
        {
            0x72, 0x5b, 0x47, 0x51, 0x9e, 0x23, 0x5e, 0x36, 0x6b, 0x2f, 0x4a, 0x63, 0x21, 0x56, 0x4d, 0x66,
            0x38, 0x00, 0x18, 0x02, 0x19, 0x0b, 0x33, 0x26, 0x5f, 0x58, 0x7d, 0x44, 0x78, 0x0d, 0x54, 0x70,
            0x0c, 0x37, 0x64, 0x3f, 0x16, 0x50, 0x2e, 0x2b, 0x52, 0x08, 0x15, 0x1e, 0x0f, 0x7b, 0x31, 0x32,
            0x13, 0x39, 0x29, 0x0e, 0x28, 0x07, 0x20, 0x03, 0x1f, 0x7a, 0x48, 0x6c, 0x4e, 0x42, 0x1a, 0x4b,
            0x6e, 0x7c, 0x3d, 0x34, 0x3c, 0x4c, 0x05, 0x7e, 0x1c, 0x55, 0x17, 0x6f, 0x3b, 0x2d, 0x5d, 0x1b,
            0x2c, 0x6a, 0x45, 0x30, 0x74, 0x06, 0x22, 0x4f, 0x65, 0x7f, 0x3a, 0x71, 0x5c, 0x10, 0x01, 0x69,
            0x25, 0x14, 0x57, 0x79, 0x60, 0x5a, 0x49, 0x0a, 0x61, 0x73, 0x24, 0x75, 0x41, 0x35, 0x11, 0x59,
            0x68, 0x01, 0x6d, 0x40, 0x27, 0x76, 0x46, 0x04, 0x53, 0x09, 0x77, 0x43, 0x4d, 0x2a, 0x12, 0x3e,
            0x72, 0x5b, 0x47, 0x51, 0x9e, 0x23, 0x5e, 0x36, 0x6b, 0x2f, 0x4a, 0x63, 0x21, 0x56, 0x4d, 0x66,
            0x38, 0x00, 0x18, 0x02, 0x19, 0x0b, 0x33, 0x26, 0x5f, 0x58, 0x7d, 0x44, 0x78, 0x0d, 0x54, 0x70,
            0x0c, 0x37, 0x64, 0x3f, 0x16, 0x50, 0x2e, 0x2b, 0x52, 0x08, 0x15, 0x1e, 0x0f, 0x7b, 0x31, 0x32,
            0x13, 0x39, 0x29, 0x0e, 0x28, 0x07, 0x20, 0x03, 0x1f, 0x7a, 0x48, 0x6c, 0x4e, 0x42, 0x1a, 0x4b,
            0x6e, 0x7c, 0x3d, 0x34, 0x3c, 0x4c, 0x05, 0x7e, 0x1c, 0x55, 0x17, 0x6f, 0x3b, 0x2d, 0x5d, 0x1b,
            0x2c, 0x6a, 0x45, 0x30, 0x74, 0x06, 0x22, 0x4f, 0x65, 0x7f, 0x3a, 0x71, 0x5c, 0x10, 0x01, 0x69,
            0x25, 0x14, 0x57, 0x79, 0x60, 0x5a, 0x49, 0x0a, 0x61, 0x73, 0x24, 0x75, 0x41, 0x35, 0x11, 0x59,
            0x68, 0x01, 0x6d, 0x40, 0x27, 0x76, 0x46, 0x04, 0x53, 0x09, 0x77, 0x43, 0x4d, 0x2a, 0x12, 0x3e
        };

        /// <summary>
        /// 使用 ZUC 加密数据
        /// </summary>
        /// <param name="plainText">明文</param>
        /// <param name="key">密钥（16字节）</param>
        /// <param name="iv">初始向量（16字节）</param>
        /// <returns>密文</returns>
        public static byte[] Encrypt(byte[] plainText, byte[] key, byte[] iv)
        {
            if (plainText == null)
                throw new ArgumentNullException(nameof(plainText));
            if (key == null || key.Length != 16)
                throw new ArgumentException("Key must be 16 bytes", nameof(key));
            if (iv == null || iv.Length != 16)
                throw new ArgumentException("IV must be 16 bytes", nameof(iv));

            using var zuc = new ZucCipher(key, iv);
            return zuc.Process(plainText);
        }

        /// <summary>
        /// 使用 ZUC 解密数据
        /// </summary>
        /// <param name="cipherText">密文</param>
        /// <param name="key">密钥（16字节）</param>
        /// <param name="iv">初始向量（16字节）</param>
        /// <returns>明文</returns>
        public static byte[] Decrypt(byte[] cipherText, byte[] key, byte[] iv)
        {
            return Encrypt(cipherText, key, iv);
        }

        /// <summary>
        /// 生成随机密钥
        /// </summary>
        /// <returns>16字节密钥</returns>
        public static byte[] GenerateKey()
        {
            byte[] key = new byte[16];
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            rng.GetBytes(key);
            return key;
        }

        /// <summary>
        /// 生成随机 IV
        /// </summary>
        /// <returns>16字节 IV</returns>
        public static byte[] GenerateIV()
        {
            byte[] iv = new byte[16];
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            rng.GetBytes(iv);
            return iv;
        }

        /// <summary>
        /// 生成随机密钥并返回十六进制
        /// </summary>
        /// <returns>32字符的十六进制密钥</returns>
        public static string GenerateKeyHex()
        {
            byte[] key = GenerateKey();
            return BitConverter.ToString(key).Replace("-", "").ToLower();
        }

        /// <summary>
        /// 生成随机 IV 并返回十六进制
        /// </summary>
        /// <returns>32字符的十六进制 IV</returns>
        public static string GenerateIVHex()
        {
            byte[] iv = GenerateIV();
            return BitConverter.ToString(iv).Replace("-", "").ToLower();
        }

        /// <summary>
        /// 加密字符串并返回 Base64
        /// </summary>
        /// <param name="plainText">明文</param>
        /// <param name="key">密钥</param>
        /// <param name="iv">初始向量</param>
        /// <returns>Base64 密文</returns>
        public static string EncryptToBase64(string plainText, byte[] key, byte[] iv)
        {
            if (string.IsNullOrEmpty(plainText))
                return string.Empty;

            byte[] data = System.Text.Encoding.UTF8.GetBytes(plainText);
            byte[] encrypted = Encrypt(data, key, iv);
            return Convert.ToBase64String(encrypted);
        }

        /// <summary>
        /// 从 Base64 解密字符串
        /// </summary>
        /// <param name="cipherText">Base64 密文</param>
        /// <param name="key">密钥</param>
        /// <param name="iv">初始向量</param>
        /// <returns>明文字符串</returns>
        public static string DecryptFromBase64(string cipherText, byte[] key, byte[] iv)
        {
            if (string.IsNullOrEmpty(cipherText))
                return string.Empty;

            byte[] data = Convert.FromBase64String(cipherText);
            byte[] decrypted = Decrypt(data, key, iv);
            return System.Text.Encoding.UTF8.GetString(decrypted);
        }

        /// <summary>
        /// 生成密钥流
        /// </summary>
        /// <param name="key">密钥</param>
        /// <param name="iv">初始向量</param>
        /// <param name="length">密钥流长度（字）</param>
        /// <returns>密钥流</returns>
        public static uint[] GenerateKeyStream(byte[] key, byte[] iv, int length)
        {
            if (key == null || key.Length != 16)
                throw new ArgumentException("Key must be 16 bytes", nameof(key));
            if (iv == null || iv.Length != 16)
                throw new ArgumentException("IV must be 16 bytes", nameof(iv));
            if (length < 1)
                throw new ArgumentException("Length must be at least 1", nameof(length));

            using var zuc = new ZucCipher(key, iv);
            var keyStream = new uint[length];
            for (int i = 0; i < length; i++)
            {
                keyStream[i] = zuc.GenerateKeyStreamWord();
            }
            return keyStream;
        }

        /// <summary>
        /// 创建 ZUC 处理器
        /// </summary>
        /// <param name="key">密钥</param>
        /// <param name="iv">初始向量</param>
        /// <returns>ZUC 处理器</returns>
        public static ZucCipher CreateCipher(byte[] key, byte[] iv)
        {
            return new ZucCipher(key, iv);
        }
    }

    /// <summary>
    /// ZUC 流加密器
    /// </summary>
    public class ZucCipher : IDisposable
    {
        private readonly uint[] _lfsr = new uint[16];
        private readonly uint[] _fsm = new uint[3];
        private bool _initialized = false;
        private bool _disposed = false;

        private static readonly uint[] EK = new uint[]
        {
            0x44D7, 0x26BC, 0x626B, 0x135E, 0x5789, 0x35E2, 0x7135, 0x09AF,
            0x4D78, 0x2F13, 0x6BC4, 0x1AF1, 0x5E26, 0x3C4A, 0x278E, 0x03F2
        };

        /// <summary>
        /// 创建 ZUC 加密器
        /// </summary>
        /// <param name="key">密钥（16字节）</param>
        /// <param name="iv">初始向量（16字节）</param>
        public ZucCipher(byte[] key, byte[] iv)
        {
            if (key == null || key.Length != 16)
                throw new ArgumentException("Key must be 16 bytes", nameof(key));
            if (iv == null || iv.Length != 16)
                throw new ArgumentException("IV must be 16 bytes", nameof(iv));

            Initialize(key, iv);
        }

        private void Initialize(byte[] key, byte[] iv)
        {
            // 初始化 LFSR
            for (int i = 0; i < 16; i++)
            {
                _lfsr[i] = ((uint)key[i] << 16) | ((uint)iv[i] << 8) | EK[i];
            }

            // 初始化 FSM
            _fsm[0] = 0;
            _fsm[1] = 0;
            _fsm[2] = 0;

            // 运行 32 轮初始化
            for (int i = 0; i < 32; i++)
            {
                uint z = GenerateKeyStreamWord();
                _lfsr[0] = (_lfsr[0] ^ z) & 0x7FFFFFFF;
                LfsrShift();
            }

            _initialized = true;
        }

        /// <summary>
        /// 生成一个密钥流字（32位）
        /// </summary>
        /// <returns>32位密钥流字</returns>
        public uint GenerateKeyStreamWord()
        {
            // F 函数
            uint fOutput = FFunction();

            // 比特重组
            uint w = BitReorganization();

            // LFSR 更新
            if (_initialized)
            {
                LfsrWithMode();
            }
            else
            {
                LfsrShift();
            }

            return w ^ fOutput;
        }

        /// <summary>
        /// 处理数据
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <returns>处理后的数据</returns>
        public byte[] Process(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            byte[] result = new byte[data.Length];
            int processed = 0;

            while (processed < data.Length)
            {
                uint keyStream = GenerateKeyStreamWord();
                byte[] keyBytes = BitConverter.GetBytes(keyStream);

                for (int i = 0; i < 4 && processed < data.Length; i++)
                {
                    result[processed] = (byte)(data[processed] ^ keyBytes[i]);
                    processed++;
                }
            }

            return result;
        }

        private uint FFunction()
        {
            uint r1 = _fsm[0];
            uint r2 = _fsm[1];

            // 简化的 F 函数
            uint w1 = (r1 + _lfsr[4]) & 0x7FFFFFFF;
            uint w2 = (r2 ^ _lfsr[10]) & 0x7FFFFFFF;

            _fsm[0] = STransform(w1);
            _fsm[1] = STransform(w2);
            _fsm[2] = r1;

            return _fsm[0] ^ _fsm[1] ^ _fsm[2];
        }

        private uint BitReorganization()
        {
            uint x0 = _lfsr[15];
            uint x1 = _lfsr[14];
            uint x2 = _lfsr[11];
            uint x3 = _lfsr[9];
            uint x4 = _lfsr[7];
            uint x5 = _lfsr[5];
            uint x6 = _lfsr[2];
            uint x7 = _lfsr[0];

            return ((x0 << 23) | (x1 >> 9)) ^ ((x2 << 15) | (x3 >> 17)) ^
                   ((x4 << 7) | (x5 >> 25)) ^ x6 ^ x7;
        }

        private void LfsrShift()
        {
            uint s0 = _lfsr[0];
            uint s4 = _lfsr[4];
            uint s10 = _lfsr[10];
            uint s13 = _lfsr[13];
            uint s15 = _lfsr[15];

            // 多项式: x^31 - 1
            uint newBit = (s0 ^ s4 ^ s10 ^ s13 ^ s15) & 0x7FFFFFFF;

            for (int i = 0; i < 15; i++)
            {
                _lfsr[i] = _lfsr[i + 1];
            }

            _lfsr[15] = newBit;
        }

        private void LfsrWithMode()
        {
            uint s0 = _lfsr[0];
            uint s4 = _lfsr[4];
            uint s10 = _lfsr[10];
            uint s13 = _lfsr[13];
            uint s15 = _lfsr[15];

            uint u = (s0 ^ s4 ^ s10 ^ s13 ^ s15) & 0x7FFFFFFF;

            for (int i = 0; i < 15; i++)
            {
                _lfsr[i] = _lfsr[i + 1];
            }

            _lfsr[15] = u;
        }

        private uint STransform(uint x)
        {
            byte b0 = (byte)(x & 0xFF);
            byte b1 = (byte)((x >> 8) & 0xFF);
            byte b2 = (byte)((x >> 16) & 0xFF);
            byte b3 = (byte)((x >> 24) & 0xFF);

            b0 = SBox(b0);
            b1 = SBox(b1);
            b2 = SBox(b2);
            b3 = SBox(b3);

            return (uint)((b3 << 24) | (b2 << 16) | (b1 << 8) | b0);
        }

        private byte SBox(byte x)
        {
            int low = x & 0x0F;
            int high = (x >> 4) & 0x0F;
            return (byte)((_s0[high] << 4) | _s1[low]);
        }

        // 内部使用的 S-box 副本
        private static readonly byte[] _s0 = new byte[]
        {
            0x3e, 0x72, 0x5b, 0x47, 0x51, 0x9e, 0x23, 0x5e, 0x36, 0x6b, 0x2f, 0x4a, 0x63, 0x21, 0x56, 0x4d,
            0x66, 0x38, 0x00, 0x18, 0x02, 0x19, 0x0b, 0x33, 0x26, 0x5f, 0x58, 0x7d, 0x44, 0x78, 0x0d, 0x54,
            0x70, 0x0c, 0x37, 0x64, 0x3f, 0x16, 0x50, 0x2e, 0x2b, 0x52, 0x08, 0x15, 0x1e, 0x0f, 0x7b, 0x31,
            0x32, 0x13, 0x39, 0x29, 0x0e, 0x28, 0x07, 0x20, 0x03, 0x1f, 0x7a, 0x48, 0x6c, 0x4e, 0x42, 0x1a,
            0x4b, 0x6e, 0x7c, 0x3d, 0x34, 0x3c, 0x4c, 0x05, 0x7e, 0x1c, 0x55, 0x17, 0x6f, 0x3b, 0x2d, 0x5d,
            0x1b, 0x2c, 0x6a, 0x45, 0x30, 0x74, 0x06, 0x22, 0x4f, 0x65, 0x7f, 0x3a, 0x71, 0x5c, 0x10, 0x01,
            0x69, 0x25, 0x14, 0x57, 0x79, 0x60, 0x5a, 0x49, 0x0a, 0x61, 0x73, 0x24, 0x75, 0x41, 0x35, 0x11,
            0x59, 0x68, 0x01, 0x6d, 0x40, 0x27, 0x76, 0x46, 0x04, 0x53, 0x09, 0x77, 0x43, 0x4d, 0x2a, 0x12
        };

        private static readonly byte[] _s1 = new byte[]
        {
            0x72, 0x5b, 0x47, 0x51, 0x9e, 0x23, 0x5e, 0x36, 0x6b, 0x2f, 0x4a, 0x63, 0x21, 0x56, 0x4d, 0x66,
            0x38, 0x00, 0x18, 0x02, 0x19, 0x0b, 0x33, 0x26, 0x5f, 0x58, 0x7d, 0x44, 0x78, 0x0d, 0x54, 0x70,
            0x0c, 0x37, 0x64, 0x3f, 0x16, 0x50, 0x2e, 0x2b, 0x52, 0x08, 0x15, 0x1e, 0x0f, 0x7b, 0x31, 0x32,
            0x13, 0x39, 0x29, 0x0e, 0x28, 0x07, 0x20, 0x03, 0x1f, 0x7a, 0x48, 0x6c, 0x4e, 0x42, 0x1a, 0x4b,
            0x6e, 0x7c, 0x3d, 0x34, 0x3c, 0x4c, 0x05, 0x7e, 0x1c, 0x55, 0x17, 0x6f, 0x3b, 0x2d, 0x5d, 0x1b,
            0x2c, 0x6a, 0x45, 0x30, 0x74, 0x06, 0x22, 0x4f, 0x65, 0x7f, 0x3a, 0x71, 0x5c, 0x10, 0x01, 0x69,
            0x25, 0x14, 0x57, 0x79, 0x60, 0x5a, 0x49, 0x0a, 0x61, 0x73, 0x24, 0x75, 0x41, 0x35, 0x11, 0x59,
            0x68, 0x01, 0x6d, 0x40, 0x27, 0x76, 0x46, 0x04, 0x53, 0x09, 0x77, 0x43, 0x4d, 0x2a, 0x12, 0x3e
        };

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                Array.Clear(_lfsr, 0, _lfsr.Length);
                Array.Clear(_fsm, 0, _fsm.Length);
                _disposed = true;
            }
        }
    }
}
