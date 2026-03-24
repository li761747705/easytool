using System;
using System.Security.Cryptography;
using System.Text;

namespace EasyTool.CodeCategory
{
    /// <summary>
    /// ElGamal 公钥加密工具类
    /// ElGamal 是基于离散对数问题的非对称加密算法
    /// 可用于加密和数字签名
    /// </summary>
    public static class ElGamalUtil
    {
        private const int DefaultKeySize = 2048;

        /// <summary>
        /// 生成密钥对
        /// </summary>
        /// <param name="keySize">密钥长度（位）</param>
        /// <returns>公钥和私钥</returns>
        public static (ElGamalPublicKey PublicKey, ElGamalPrivateKey PrivateKey) GenerateKeyPair(int keySize = DefaultKeySize)
        {
            if (keySize < 512)
                throw new ArgumentException("Key size must be at least 512 bits", nameof(keySize));

            using var rng = RandomNumberGenerator.Create();

            // 生成大素数 p
            byte[] pBytes = new byte[keySize / 8];
            rng.GetBytes(pBytes);
            pBytes[pBytes.Length - 1] |= 0x01; // 确保是奇数
            pBytes[0] |= 0x80; // 确保高位为1

            var p = new System.Numerics.BigInteger(pBytes);
            p = System.Numerics.BigInteger.Abs(p);

            // 找到下一个素数
            while (!IsProbablyPrime(p))
            {
                p += 2;
            }

            // 生成生成元 g（简化：使用小素数）
            var g = FindGenerator(p, keySize, rng);

            // 生成私钥 x
            byte[] xBytes = new byte[keySize / 8 - 1];
            rng.GetBytes(xBytes);
            var x = new System.Numerics.BigInteger(xBytes);
            x = System.Numerics.BigInteger.Abs(x) % (p - 2) + 1;

            // 计算公钥 y = g^x mod p
            var y = ModPow(g, x, p);

            return (
                new ElGamalPublicKey(p, g, y),
                new ElGamalPrivateKey(p, g, x)
            );
        }

        /// <summary>
        /// 加密数据
        /// </summary>
        /// <param name="plainText">明文</param>
        /// <param name="publicKey">公钥</param>
        /// <returns>密文（C1 + C2）</returns>
        public static byte[] Encrypt(byte[] plainText, ElGamalPublicKey publicKey)
        {
            if (plainText == null || plainText.Length == 0)
                return Array.Empty<byte>();

            using var rng = RandomNumberGenerator.Create();

            var p = publicKey.P;
            var g = publicKey.G;
            var y = publicKey.Y;

            int keySize = p.ToByteArray().Length;

            // 将明文转换为数字
            byte[] paddedPlain = new byte[plainText.Length + 2];
            Array.Copy(plainText, paddedPlain, plainText.Length);
            var m = new System.Numerics.BigInteger(paddedPlain);

            if (m >= p)
                throw new ArgumentException("Message too long for key size", nameof(plainText));

            // 生成随机数 k
            byte[] kBytes = new byte[keySize - 1];
            rng.GetBytes(kBytes);
            var k = new System.Numerics.BigInteger(kBytes);
            k = System.Numerics.BigInteger.Abs(k) % (p - 2) + 1;

            // 计算 C1 = g^k mod p
            var c1 = ModPow(g, k, p);

            // 计算 C2 = m * y^k mod p
            var c2 = (m * ModPow(y, k, p)) % p;

            // 序列化 C1 和 C2
            byte[] c1Bytes = c1.ToByteArray();
            byte[] c2Bytes = c2.ToByteArray();

            byte[] result = new byte[4 + c1Bytes.Length + 4 + c2Bytes.Length];
            BitConverter.GetBytes(c1Bytes.Length).CopyTo(result, 0);
            c1Bytes.CopyTo(result, 4);
            BitConverter.GetBytes(c2Bytes.Length).CopyTo(result, 4 + c1Bytes.Length);
            c2Bytes.CopyTo(result, 8 + c1Bytes.Length);

            return result;
        }

        /// <summary>
        /// 解密数据
        /// </summary>
        /// <param name="cipherText">密文</param>
        /// <param name="privateKey">私钥</param>
        /// <returns>明文</returns>
        public static byte[] Decrypt(byte[] cipherText, ElGamalPrivateKey privateKey)
        {
            if (cipherText == null || cipherText.Length < 8)
                return Array.Empty<byte>();

            var p = privateKey.P;
            var x = privateKey.X;

            // 解析 C1 和 C2
            int c1Length = BitConverter.ToInt32(cipherText, 0);
            int c2Length = BitConverter.ToInt32(cipherText, 4 + c1Length);

            byte[] c1Bytes = new byte[c1Length];
            byte[] c2Bytes = new byte[c2Length];
            Array.Copy(cipherText, 4, c1Bytes, 0, c1Length);
            Array.Copy(cipherText, 8 + c1Length, c2Bytes, 0, c2Length);

            var c1 = new System.Numerics.BigInteger(c1Bytes);
            var c2 = new System.Numerics.BigInteger(c2Bytes);

            // 计算 s = C1^x mod p
            var s = ModPow(c1, x, p);

            // 计算逆元 s^-1
            var sInv = ModInverse(s, p);

            // 计算 m = C2 * s^-1 mod p
            var m = (c2 * sInv) % p;

            // 转换为字节数组
            byte[] result = m.ToByteArray();
            Array.Resize(ref result, result.Length > 2 ? result.Length - 2 : 0);

            return result;
        }

        /// <summary>
        /// 加密字符串并返回 Base64
        /// </summary>
        public static string EncryptToBase64(string plainText, ElGamalPublicKey publicKey)
        {
            if (string.IsNullOrEmpty(plainText))
                return string.Empty;

            byte[] data = Encoding.UTF8.GetBytes(plainText);
            byte[] encrypted = Encrypt(data, publicKey);
            return Convert.ToBase64String(encrypted);
        }

        /// <summary>
        /// 从 Base64 解密字符串
        /// </summary>
        public static string DecryptFromBase64(string cipherText, ElGamalPrivateKey privateKey)
        {
            if (string.IsNullOrEmpty(cipherText))
                return string.Empty;

            byte[] data = Convert.FromBase64String(cipherText);
            byte[] decrypted = Decrypt(data, privateKey);
            return Encoding.UTF8.GetString(decrypted);
        }

        /// <summary>
        /// 签名数据
        /// </summary>
        public static byte[] Sign(byte[] data, ElGamalPrivateKey privateKey, System.Security.Cryptography.HashAlgorithm hashAlgorithm = null)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            bool shouldDisposeHash = hashAlgorithm == null;
            hashAlgorithm ??= SHA256.Create();
            try
            {
                byte[] hash = hashAlgorithm.ComputeHash(data);
                var h = new System.Numerics.BigInteger(hash);

                var p = privateKey.P;
                var g = privateKey.G;
                var x = privateKey.X;

                using var rng = RandomNumberGenerator.Create();
                int keySize = p.ToByteArray().Length;

                byte[] kBytes = new byte[keySize - 1];
                System.Numerics.BigInteger k;
                System.Numerics.BigInteger kInv;

                do
                {
                    rng.GetBytes(kBytes);
                    k = new System.Numerics.BigInteger(kBytes);
                    k = System.Numerics.BigInteger.Abs(k) % (p - 2) + 1;
                    kInv = ModInverse(k, p - 1);
                } while (kInv == 0);

                var r = ModPow(g, k, p);
                var s = ((h - x * r) * kInv) % (p - 1);
                if (s < 0) s += p - 1;

                byte[] rBytes = r.ToByteArray();
                byte[] sBytes = s.ToByteArray();

                byte[] result = new byte[4 + rBytes.Length + 4 + sBytes.Length];
                BitConverter.GetBytes(rBytes.Length).CopyTo(result, 0);
                rBytes.CopyTo(result, 4);
                BitConverter.GetBytes(sBytes.Length).CopyTo(result, 4 + rBytes.Length);
                sBytes.CopyTo(result, 8 + rBytes.Length);

                return result;
            }
            finally
            {
                if (shouldDisposeHash)
                    hashAlgorithm.Dispose();
            }
        }

        /// <summary>
        /// 验证签名
        /// </summary>
        public static bool Verify(byte[] data, byte[] signature, ElGamalPublicKey publicKey, System.Security.Cryptography.HashAlgorithm hashAlgorithm = null)
        {
            if (data == null || signature == null || signature.Length < 8)
                return false;

            bool shouldDisposeHash = hashAlgorithm == null;
            hashAlgorithm ??= SHA256.Create();
            try
            {
                byte[] hash = hashAlgorithm.ComputeHash(data);
                var h = new System.Numerics.BigInteger(hash);

                var p = publicKey.P;
                var g = publicKey.G;
                var y = publicKey.Y;

                int rLength = BitConverter.ToInt32(signature, 0);
                int sLength = BitConverter.ToInt32(signature, 4 + rLength);

                byte[] rBytes = new byte[rLength];
                byte[] sBytes = new byte[sLength];
                Array.Copy(signature, 4, rBytes, 0, rLength);
                Array.Copy(signature, 8 + rLength, sBytes, 0, sLength);

                var r = new System.Numerics.BigInteger(rBytes);
                var s = new System.Numerics.BigInteger(sBytes);

                // 验证: g^h ≡ y^r * r^s (mod p)
                var left = ModPow(g, h, p);
                var right = (ModPow(y, r, p) * ModPow(r, s, p)) % p;

                return left == right;
            }
            finally
            {
                if (shouldDisposeHash)
                    hashAlgorithm.Dispose();
            }
        }

        private static System.Numerics.BigInteger ModPow(System.Numerics.BigInteger b, System.Numerics.BigInteger e, System.Numerics.BigInteger m)
        {
            return System.Numerics.BigInteger.ModPow(b, e, m);
        }

        private static System.Numerics.BigInteger ModInverse(System.Numerics.BigInteger a, System.Numerics.BigInteger m)
        {
            return System.Numerics.BigInteger.ModPow(a, m - 2, m);
        }

        private static System.Numerics.BigInteger FindGenerator(System.Numerics.BigInteger p, int keySize, RandomNumberGenerator rng)
        {
            // 简化：使用小生成元
            for (int g = 2; g < 100; g++)
            {
                if (ModPow(g, (p - 1) / 2, p) != 1)
                {
                    return g;
                }
            }
            return 2;
        }

        private static bool IsProbablyPrime(System.Numerics.BigInteger n, int k = 10)
        {
            if (n < 2) return false;
            if (n == 2 || n == 3) return true;
            if (n % 2 == 0) return false;

            var d = n - 1;
            int r = 0;
            while (d % 2 == 0)
            {
                d /= 2;
                r++;
            }

            using var rng = RandomNumberGenerator.Create();
            int byteLength = n.ToByteArray().Length;

            for (int i = 0; i < k; i++)
            {
                byte[] aBytes = new byte[byteLength];
                rng.GetBytes(aBytes);
                var a = new System.Numerics.BigInteger(aBytes);
                a = System.Numerics.BigInteger.Abs(a) % (n - 3) + 2;

                var x = ModPow(a, d, n);

                if (x == 1 || x == n - 1)
                    continue;

                bool composite = true;
                for (int j = 0; j < r - 1; j++)
                {
                    x = (x * x) % n;
                    if (x == n - 1)
                    {
                        composite = false;
                        break;
                    }
                }

                if (composite)
                    return false;
            }

            return true;
        }
    }

    /// <summary>
    /// ElGamal 公钥
    /// </summary>
    public class ElGamalPublicKey
    {
        public System.Numerics.BigInteger P { get; }
        public System.Numerics.BigInteger G { get; }
        public System.Numerics.BigInteger Y { get; }

        public ElGamalPublicKey(System.Numerics.BigInteger p, System.Numerics.BigInteger g, System.Numerics.BigInteger y)
        {
            P = p;
            G = g;
            Y = y;
        }

        public byte[] ToByteArray()
        {
            byte[] pBytes = P.ToByteArray();
            byte[] gBytes = G.ToByteArray();
            byte[] yBytes = Y.ToByteArray();

            byte[] result = new byte[12 + pBytes.Length + gBytes.Length + yBytes.Length];
            BitConverter.GetBytes(pBytes.Length).CopyTo(result, 0);
            pBytes.CopyTo(result, 4);
            BitConverter.GetBytes(gBytes.Length).CopyTo(result, 4 + pBytes.Length);
            gBytes.CopyTo(result, 8 + pBytes.Length);
            BitConverter.GetBytes(yBytes.Length).CopyTo(result, 8 + pBytes.Length + gBytes.Length);
            yBytes.CopyTo(result, 12 + pBytes.Length + gBytes.Length);

            return result;
        }

        public static ElGamalPublicKey FromByteArray(byte[] data)
        {
            int pLength = BitConverter.ToInt32(data, 0);
            int gLength = BitConverter.ToInt32(data, 4 + pLength);
            int yLength = BitConverter.ToInt32(data, 8 + pLength + gLength);

            byte[] pBytes = new byte[pLength];
            byte[] gBytes = new byte[gLength];
            byte[] yBytes = new byte[yLength];

            Array.Copy(data, 4, pBytes, 0, pLength);
            Array.Copy(data, 8 + pLength, gBytes, 0, gLength);
            Array.Copy(data, 12 + pLength + gLength, yBytes, 0, yLength);

            return new ElGamalPublicKey(
                new System.Numerics.BigInteger(pBytes),
                new System.Numerics.BigInteger(gBytes),
                new System.Numerics.BigInteger(yBytes)
            );
        }
    }

    /// <summary>
    /// ElGamal 私钥
    /// </summary>
    public class ElGamalPrivateKey
    {
        public System.Numerics.BigInteger P { get; }
        public System.Numerics.BigInteger G { get; }
        public System.Numerics.BigInteger X { get; }

        public ElGamalPrivateKey(System.Numerics.BigInteger p, System.Numerics.BigInteger g, System.Numerics.BigInteger x)
        {
            P = p;
            G = g;
            X = x;
        }

        public byte[] ToByteArray()
        {
            byte[] pBytes = P.ToByteArray();
            byte[] gBytes = G.ToByteArray();
            byte[] xBytes = X.ToByteArray();

            byte[] result = new byte[12 + pBytes.Length + gBytes.Length + xBytes.Length];
            BitConverter.GetBytes(pBytes.Length).CopyTo(result, 0);
            pBytes.CopyTo(result, 4);
            BitConverter.GetBytes(gBytes.Length).CopyTo(result, 4 + pBytes.Length);
            gBytes.CopyTo(result, 8 + pBytes.Length);
            BitConverter.GetBytes(xBytes.Length).CopyTo(result, 8 + pBytes.Length + gBytes.Length);
            xBytes.CopyTo(result, 12 + pBytes.Length + gBytes.Length);

            return result;
        }

        public static ElGamalPrivateKey FromByteArray(byte[] data)
        {
            int pLength = BitConverter.ToInt32(data, 0);
            int gLength = BitConverter.ToInt32(data, 4 + pLength);
            int xLength = BitConverter.ToInt32(data, 8 + pLength + gLength);

            byte[] pBytes = new byte[pLength];
            byte[] gBytes = new byte[gLength];
            byte[] xBytes = new byte[xLength];

            Array.Copy(data, 4, pBytes, 0, pLength);
            Array.Copy(data, 8 + pLength, gBytes, 0, gLength);
            Array.Copy(data, 12 + pLength + gLength, xBytes, 0, xLength);

            return new ElGamalPrivateKey(
                new System.Numerics.BigInteger(pBytes),
                new System.Numerics.BigInteger(gBytes),
                new System.Numerics.BigInteger(xBytes)
            );
        }
    }
}
