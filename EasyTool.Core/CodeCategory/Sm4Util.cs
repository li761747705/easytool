using System;
using System.Text;

namespace EasyTool.CodeCategory
{
    /// <summary>
    /// SM4 对称加密算法工具类
    /// SM4 是中国国家密码管理局发布的分组密码标准
    /// 分组长度128位，密钥长度128位
    /// </summary>
    public static class Sm4Util
    {
        // SM4 S盒
        private static readonly byte[] SBOX = new byte[]
        {
            0xd6, 0x90, 0xe9, 0xfe, 0xcc, 0xe1, 0x3d, 0xb7, 0x16, 0xb6, 0x14, 0xc2, 0x28, 0xfb, 0x2c, 0x05,
            0x2b, 0x67, 0x9a, 0x76, 0x2a, 0xbe, 0x04, 0xc3, 0xaa, 0x44, 0x13, 0x26, 0x49, 0x86, 0x06, 0x99,
            0x9c, 0x42, 0x50, 0xf4, 0x91, 0xef, 0x98, 0x7a, 0x33, 0x54, 0x0b, 0x43, 0xed, 0xcf, 0xac, 0x62,
            0xe4, 0xb3, 0x1c, 0xa9, 0xc9, 0x08, 0xe8, 0x95, 0x80, 0xdf, 0x94, 0xfa, 0x75, 0x8f, 0x3f, 0xa6,
            0x47, 0x07, 0xa7, 0xfc, 0xf3, 0x73, 0x17, 0xba, 0x83, 0x59, 0x3c, 0x19, 0xe6, 0x85, 0x4f, 0xa8,
            0x68, 0x6b, 0x81, 0xb2, 0x71, 0x64, 0xda, 0x8b, 0xf8, 0xeb, 0x0f, 0x4b, 0x70, 0x56, 0x9d, 0x35,
            0x1e, 0x24, 0x0e, 0x5e, 0x63, 0x58, 0xd1, 0xa2, 0x25, 0x22, 0x7c, 0x3b, 0x01, 0x21, 0x78, 0x87,
            0xd4, 0x00, 0x46, 0x57, 0x9f, 0xd3, 0x27, 0x52, 0x4c, 0x36, 0x02, 0xe7, 0xa0, 0xc4, 0xc8, 0x9e,
            0xea, 0xbf, 0x8a, 0xd2, 0x40, 0xc7, 0x38, 0xb5, 0xa3, 0xf7, 0xf2, 0xce, 0xf9, 0x61, 0x15, 0xa1,
            0xe0, 0xae, 0x5d, 0xa4, 0x9b, 0x34, 0x1a, 0x55, 0xad, 0x93, 0x32, 0x30, 0xf5, 0x8c, 0xb1, 0xe3,
            0x1d, 0xf6, 0xe2, 0x2e, 0x82, 0x66, 0xca, 0x60, 0xc0, 0x29, 0x23, 0xab, 0x0d, 0x53, 0x4e, 0x6f,
            0xd5, 0xdb, 0x37, 0x45, 0xde, 0xfd, 0x8e, 0x2f, 0x03, 0xff, 0x6a, 0x72, 0x6d, 0x6c, 0x5b, 0x51,
            0x8d, 0x1b, 0xaf, 0x92, 0xbb, 0xdd, 0xbc, 0x7f, 0x11, 0xd9, 0x5c, 0x41, 0x1f, 0x10, 0x5a, 0xd8,
            0x0a, 0xc1, 0x31, 0x88, 0xa5, 0xcd, 0x7b, 0xbd, 0x2d, 0x74, 0xd0, 0x12, 0xb8, 0xe5, 0xb4, 0xb0,
            0x89, 0x69, 0x97, 0x4a, 0x0c, 0x96, 0x77, 0x7e, 0x65, 0xb9, 0xf1, 0x09, 0xc5, 0x6e, 0xc6, 0x84,
            0x18, 0xf0, 0x7d, 0xec, 0x3a, 0xdc, 0x4d, 0x20, 0x79, 0xee, 0x5f, 0x3e, 0xd7, 0xcb, 0x39, 0x48
        };

        // 系统参数 FK
        private static readonly uint[] FK = new uint[] { 0xa3b1bac6, 0x56aa3350, 0x677d9197, 0xb27022dc };

        // 固定参数 CK
        private static readonly uint[] CK = new uint[]
        {
            0x00070e15, 0x1c232a31, 0x383f464d, 0x545b6269,
            0x70777e85, 0x8c939aa1, 0xa8afb6bd, 0xc4cbd2d9,
            0xe0e7eef5, 0xfc030a11, 0x181f262d, 0x343b4249,
            0x50575e65, 0x6c737a81, 0x888f969d, 0xa4abb2b9,
            0xc0c7ced5, 0xdce3eaf1, 0xf8ff060d, 0x141b2229,
            0x30373e45, 0x4c535a61, 0x686f767d, 0x848b9299,
            0xa0a7aeb5, 0xbcc3cad1, 0xd8dfe6ed, 0xf4fb0209,
            0x10171e25, 0x2c333a41, 0x484f565d, 0x646b7279
        };

        private const int BLOCK_SIZE = 16; // 128位

        /// <summary>
        /// SM4 加密（ECB模式）
        /// </summary>
        /// <param name="plainText">明文</param>
        /// <param name="key">密钥（16字节）</param>
        /// <returns>密文</returns>
        public static byte[] Encrypt(byte[] plainText, byte[] key)
        {
            return Encrypt(plainText, key, Sm4Mode.ECB, null);
        }

        /// <summary>
        /// SM4 加密
        /// </summary>
        /// <param name="plainText">明文</param>
        /// <param name="key">密钥（16字节）</param>
        /// <param name="mode">加密模式</param>
        /// <param name="iv">初始向量（CBC模式需要，16字节）</param>
        /// <returns>密文</returns>
        public static byte[] Encrypt(byte[] plainText, byte[] key, Sm4Mode mode, byte[] iv)
        {
            if (plainText == null)
                throw new ArgumentNullException(nameof(plainText));
            if (key == null || key.Length != 16)
                throw new ArgumentException("Key must be 16 bytes", nameof(key));
            if (mode == Sm4Mode.CBC && (iv == null || iv.Length != 16))
                throw new ArgumentException("IV must be 16 bytes for CBC mode", nameof(iv));

            // 生成轮密钥
            uint[] roundKeys = GenerateRoundKeys(key);

            // PKCS7 填充
            byte[] padded = Pkcs7Pad(plainText);

            byte[] result = new byte[padded.Length];
            byte[] temp = new byte[BLOCK_SIZE];

            if (mode == Sm4Mode.CBC)
            {
                Array.Copy(iv, temp, BLOCK_SIZE);
            }

            for (int i = 0; i < padded.Length; i += BLOCK_SIZE)
            {
                if (mode == Sm4Mode.CBC)
                {
                    // CBC模式：明文先与IV异或
                    for (int j = 0; j < BLOCK_SIZE; j++)
                    {
                        temp[j] = (byte)(padded[i + j] ^ temp[j]);
                    }
                    EncryptBlock(temp, roundKeys);
                    Array.Copy(temp, 0, result, i, BLOCK_SIZE);
                }
                else
                {
                    Array.Copy(padded, i, temp, 0, BLOCK_SIZE);
                    EncryptBlock(temp, roundKeys);
                    Array.Copy(temp, 0, result, i, BLOCK_SIZE);
                }
            }

            return result;
        }

        /// <summary>
        /// SM4 加密字符串并返回 Base64
        /// </summary>
        /// <param name="plainText">明文字符串</param>
        /// <param name="key">密钥（16字节）</param>
        /// <param name="encoding">编码方式（默认UTF-8）</param>
        /// <returns>Base64 密文</returns>
        public static string EncryptToBase64(string plainText, byte[] key, Encoding encoding = null)
        {
            if (plainText == null)
                throw new ArgumentNullException(nameof(plainText));

            encoding ??= Encoding.UTF8;
            byte[] encrypted = Encrypt(encoding.GetBytes(plainText), key);
            return Convert.ToBase64String(encrypted);
        }

        /// <summary>
        /// SM4 加密字符串并返回 Base64
        /// </summary>
        /// <param name="plainText">明文字符串</param>
        /// <param name="keyString">密钥字符串</param>
        /// <param name="encoding">编码方式（默认UTF-8）</param>
        /// <returns>Base64 密文</returns>
        public static string EncryptToBase64(string plainText, string keyString, Encoding encoding = null)
        {
            encoding ??= Encoding.UTF8;
            byte[] key = GetKeyFromString(keyString, encoding);
            return EncryptToBase64(plainText, key, encoding);
        }

        /// <summary>
        /// SM4 解密（ECB模式）
        /// </summary>
        /// <param name="cipherText">密文</param>
        /// <param name="key">密钥（16字节）</param>
        /// <returns>明文</returns>
        public static byte[] Decrypt(byte[] cipherText, byte[] key)
        {
            return Decrypt(cipherText, key, Sm4Mode.ECB, null);
        }

        /// <summary>
        /// SM4 解密
        /// </summary>
        /// <param name="cipherText">密文</param>
        /// <param name="key">密钥（16字节）</param>
        /// <param name="mode">加密模式</param>
        /// <param name="iv">初始向量（CBC模式需要，16字节）</param>
        /// <returns>明文</returns>
        public static byte[] Decrypt(byte[] cipherText, byte[] key, Sm4Mode mode, byte[] iv)
        {
            if (cipherText == null)
                throw new ArgumentNullException(nameof(cipherText));
            if (key == null || key.Length != 16)
                throw new ArgumentException("Key must be 16 bytes", nameof(key));
            if (cipherText.Length == 0 || cipherText.Length % BLOCK_SIZE != 0)
                throw new ArgumentException("Cipher text length must be a multiple of 16", nameof(cipherText));
            if (mode == Sm4Mode.CBC && (iv == null || iv.Length != 16))
                throw new ArgumentException("IV must be 16 bytes for CBC mode", nameof(iv));

            // 生成轮密钥（解密时使用逆序）
            uint[] roundKeys = GenerateRoundKeys(key);
            Array.Reverse(roundKeys);

            byte[] result = new byte[cipherText.Length];
            byte[] temp = new byte[BLOCK_SIZE];
            byte[] prevBlock = mode == Sm4Mode.CBC ? iv : null;

            for (int i = 0; i < cipherText.Length; i += BLOCK_SIZE)
            {
                Array.Copy(cipherText, i, temp, 0, BLOCK_SIZE);
                EncryptBlock(temp, roundKeys); // 使用逆序的轮密钥

                if (mode == Sm4Mode.CBC && prevBlock != null)
                {
                    // CBC模式：解密后与前一个密文块异或
                    for (int j = 0; j < BLOCK_SIZE; j++)
                    {
                        result[i + j] = (byte)(temp[j] ^ prevBlock[j]);
                    }
                    prevBlock = new byte[BLOCK_SIZE];
                    Array.Copy(cipherText, i, prevBlock, 0, BLOCK_SIZE);
                }
                else
                {
                    Array.Copy(temp, 0, result, i, BLOCK_SIZE);
                }
            }

            // 移除 PKCS7 填充
            return Pkcs7Unpad(result);
        }

        /// <summary>
        /// SM4 解密 Base64 字符串
        /// </summary>
        /// <param name="cipherText">Base64 密文</param>
        /// <param name="key">密钥（16字节）</param>
        /// <param name="encoding">编码方式（默认UTF-8）</param>
        /// <returns>明文字符串</returns>
        public static string DecryptFromBase64(string cipherText, byte[] key, Encoding encoding = null)
        {
            if (cipherText == null)
                throw new ArgumentNullException(nameof(cipherText));

            encoding ??= Encoding.UTF8;
            byte[] cipher = Convert.FromBase64String(cipherText);
            byte[] decrypted = Decrypt(cipher, key);
            return encoding.GetString(decrypted);
        }

        /// <summary>
        /// SM4 解密 Base64 字符串
        /// </summary>
        /// <param name="cipherText">Base64 密文</param>
        /// <param name="keyString">密钥字符串</param>
        /// <param name="encoding">编码方式（默认UTF-8）</param>
        /// <returns>明文字符串</returns>
        public static string DecryptFromBase64(string cipherText, string keyString, Encoding encoding = null)
        {
            encoding ??= Encoding.UTF8;
            byte[] key = GetKeyFromString(keyString, encoding);
            return DecryptFromBase64(cipherText, key, encoding);
        }

        /// <summary>
        /// 生成随机密钥
        /// </summary>
        /// <returns>16字节随机密钥</returns>
        public static byte[] GenerateKey()
        {
            var key = new byte[16];
            new Random().NextBytes(key);
            return key;
        }

        /// <summary>
        /// 生成随机密钥并返回十六进制字符串
        /// </summary>
        /// <returns>32字符的十六进制密钥</returns>
        public static string GenerateKeyHex()
        {
            byte[] key = GenerateKey();
            return BitConverter.ToString(key).Replace("-", "").ToLower();
        }

        #region 私有方法

        private static uint[] GenerateRoundKeys(byte[] key)
        {
            uint[] roundKeys = new uint[32];
            uint[] mk = new uint[4];

            // 将密钥转换为4个32位字
            for (int i = 0; i < 4; i++)
            {
                mk[i] = ((uint)key[i * 4] << 24) |
                        ((uint)key[i * 4 + 1] << 16) |
                        ((uint)key[i * 4 + 2] << 8) |
                        key[i * 4 + 3];
            }

            // 初始化轮密钥
            uint[] k = new uint[36];
            for (int i = 0; i < 4; i++)
            {
                k[i] = mk[i] ^ FK[i];
            }

            // 生成32个轮密钥
            for (int i = 0; i < 32; i++)
            {
                k[i + 4] = k[i] ^ TPrime(k[i + 1] ^ k[i + 2] ^ k[i + 3] ^ CK[i]);
                roundKeys[i] = k[i + 4];
            }

            return roundKeys;
        }

        private static void EncryptBlock(byte[] block, uint[] roundKeys)
        {
            uint[] x = new uint[4];

            // 将16字节转换为4个32位字
            for (int i = 0; i < 4; i++)
            {
                x[i] = ((uint)block[i * 4] << 24) |
                       ((uint)block[i * 4 + 1] << 16) |
                       ((uint)block[i * 4 + 2] << 8) |
                       block[i * 4 + 3];
            }

            // 32轮加密
            for (int i = 0; i < 32; i++)
            {
                uint temp = x[0];
                x[0] = x[1];
                x[1] = x[2];
                x[2] = x[3];
                x[3] = temp ^ T(x[1] ^ x[2] ^ x[3] ^ roundKeys[i]);
            }

            // 反序并输出
            for (int i = 0; i < 4; i++)
            {
                block[i * 4] = (byte)(x[3 - i] >> 24);
                block[i * 4 + 1] = (byte)(x[3 - i] >> 16);
                block[i * 4 + 2] = (byte)(x[3 - i] >> 8);
                block[i * 4 + 3] = (byte)x[3 - i];
            }
        }

        private static uint T(uint x)
        {
            byte[] bytes = new byte[4];
            bytes[0] = (byte)(x >> 24);
            bytes[1] = (byte)(x >> 16);
            bytes[2] = (byte)(x >> 8);
            bytes[3] = (byte)x;

            // S盒替换
            for (int i = 0; i < 4; i++)
            {
                bytes[i] = SBOX[bytes[i]];
            }

            uint result = ((uint)bytes[0] << 24) |
                          ((uint)bytes[1] << 16) |
                          ((uint)bytes[2] << 8) |
                          bytes[3];

            // L变换
            return result ^ RotateLeft(result, 2) ^ RotateLeft(result, 10) ^
                   RotateLeft(result, 18) ^ RotateLeft(result, 24);
        }

        private static uint TPrime(uint x)
        {
            byte[] bytes = new byte[4];
            bytes[0] = (byte)(x >> 24);
            bytes[1] = (byte)(x >> 16);
            bytes[2] = (byte)(x >> 8);
            bytes[3] = (byte)x;

            // S盒替换
            for (int i = 0; i < 4; i++)
            {
                bytes[i] = SBOX[bytes[i]];
            }

            uint result = ((uint)bytes[0] << 24) |
                          ((uint)bytes[1] << 16) |
                          ((uint)bytes[2] << 8) |
                          bytes[3];

            // L'变换
            return result ^ RotateLeft(result, 13) ^ RotateLeft(result, 23);
        }

        private static uint RotateLeft(uint x, int n)
        {
            return (x << n) | (x >> (32 - n));
        }

        private static byte[] Pkcs7Pad(byte[] data)
        {
            int padLen = BLOCK_SIZE - (data.Length % BLOCK_SIZE);
            byte[] result = new byte[data.Length + padLen];
            Array.Copy(data, result, data.Length);
            for (int i = data.Length; i < result.Length; i++)
            {
                result[i] = (byte)padLen;
            }
            return result;
        }

        private static byte[] Pkcs7Unpad(byte[] data)
        {
            if (data.Length == 0)
                return data;

            int padLen = data[data.Length - 1];
            if (padLen > BLOCK_SIZE || padLen == 0)
                return data;

            // 验证填充
            for (int i = data.Length - padLen; i < data.Length; i++)
            {
                if (data[i] != padLen)
                    return data;
            }

            byte[] result = new byte[data.Length - padLen];
            Array.Copy(data, result, result.Length);
            return result;
        }

        private static byte[] GetKeyFromString(string keyString, Encoding encoding)
        {
            byte[] keyBytes = encoding.GetBytes(keyString);
            if (keyBytes.Length == 16)
                return keyBytes;
            if (keyBytes.Length < 16)
            {
                byte[] result = new byte[16];
                Array.Copy(keyBytes, result, keyBytes.Length);
                return result;
            }
            byte[] truncated = new byte[16];
            Array.Copy(keyBytes, truncated, 16);
            return truncated;
        }

        #endregion
    }

    /// <summary>
    /// SM4 加密模式
    /// </summary>
    public enum Sm4Mode
    {
        /// <summary>
        /// 电子密码本模式
        /// </summary>
        ECB,

        /// <summary>
        /// 密码分组链接模式
        /// </summary>
        CBC
    }
}
