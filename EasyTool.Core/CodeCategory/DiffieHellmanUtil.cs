using System;
using System.Security.Cryptography;
using System.Text;

namespace EasyTool.CodeCategory
{
    /// <summary>
    /// Diffie-Hellman 密钥交换工具类
    /// DH 是一种安全地在公共信道上共享密钥的方法
    /// 基于离散对数问题的数学难题
    /// </summary>
    public static class DiffieHellmanUtil
    {
        private const int DefaultKeySize = 2048;

        // 常用安全素数参数（RFC 3526）
        private static readonly string[] KnownPrimes = new string[]
        {
            // 2048位 MODP Group (RFC 3526, Group 14)
            "FFFFFFFFFFFFFFFFC90FDAA22168C234C4C6628B80DC1CD129024E088A67CC74020BBEA63B139B22514A08798E3404DDEF9519B3CD3A431B302B0A6DF25F14374FE1356D6D51C245E485B576625E7EC6F44C42E9A637ED6B0BFF5CB6F406B7EDEE386BFB5A899FA5AE9F24117C4B1FE649286651ECE45B3DC2007CB8A163BF0598DA48361C55D39A69163FA8FD24CF5F83655D23DCA3AD961C62F356208552BB9ED529077096966D670C354E4ABC9804F1746C08CA18217C32905E462E36CE3BE39E772C180E86039B2783A2EC07A28FB5C55DF06F4C52C9DE2BCBF6955817183995497CEA956AE515D2261898FA051015728E5A8AACAA68FFFFFFFFFFFFFFFF"
        };

        private static readonly string[] KnownGenerators = new string[]
        {
            "2"
        };

        /// <summary>
        /// 生成密钥对
        /// </summary>
        /// <param name="keySize">密钥长度（位）</param>
        /// <returns>密钥对</returns>
        public static DHKeyPair GenerateKeyPair(int keySize = DefaultKeySize)
        {
            if (keySize < 512)
                throw new ArgumentException("Key size must be at least 512 bits", nameof(keySize));

            using var rng = RandomNumberGenerator.Create();

            // 使用预定义的素数参数
            int primeIndex = 0;
            System.Numerics.BigInteger p, g;

            if (keySize <= 2048)
            {
                p = System.Numerics.BigInteger.Parse(KnownPrimes[primeIndex], System.Globalization.NumberStyles.HexNumber);
                g = System.Numerics.BigInteger.Parse(KnownGenerators[primeIndex], System.Globalization.NumberStyles.HexNumber);
            }
            else
            {
                // 生成自定义参数（较慢）
                (p, g) = GenerateParametersInternal(keySize, rng);
            }

            // 生成私钥
            byte[] xBytes = new byte[keySize / 8];
            rng.GetBytes(xBytes);
            var x = new System.Numerics.BigInteger(xBytes);
            x = System.Numerics.BigInteger.Abs(x) % (p - 2) + 1;

            // 计算公钥 y = g^x mod p
            var y = ModPow(g, x, p);

            return new DHKeyPair(new DHParameters(p, g), x, y);
        }

        /// <summary>
        /// 使用指定参数生成密钥对
        /// </summary>
        /// <param name="parameters">DH 参数</param>
        /// <returns>密钥对</returns>
        public static DHKeyPair GenerateKeyPair(DHParameters parameters)
        {
            using var rng = RandomNumberGenerator.Create();

            var p = parameters.P;
            var g = parameters.G;

            int keySize = p.ToByteArray().Length * 8;

            byte[] xBytes = new byte[keySize / 8];
            rng.GetBytes(xBytes);
            var x = new System.Numerics.BigInteger(xBytes);
            x = System.Numerics.BigInteger.Abs(x) % (p - 2) + 1;

            var y = ModPow(g, x, p);

            return new DHKeyPair(parameters, x, y);
        }

        /// <summary>
        /// 计算共享密钥
        /// </summary>
        /// <param name="otherPublicKey">对方的公钥</param>
        /// <param name="privateKey">自己的私钥</param>
        /// <param name="parameters">DH 参数</param>
        /// <returns>共享密钥</returns>
        public static byte[] ComputeSharedSecret(System.Numerics.BigInteger otherPublicKey, System.Numerics.BigInteger privateKey, DHParameters parameters)
        {
            var sharedSecret = ModPow(otherPublicKey, privateKey, parameters.P);
            return sharedSecret.ToByteArray();
        }

        /// <summary>
        /// 计算共享密钥并派生为指定长度的对称密钥
        /// </summary>
        /// <param name="otherPublicKey">对方的公钥</param>
        /// <param name="privateKey">自己的私钥</param>
        /// <param name="parameters">DH 参数</param>
        /// <param name="keyLength">派生密钥长度（字节）</param>
        /// <param name="salt">盐值（可选）</param>
        /// <returns>派生的对称密钥</returns>
        public static byte[] DeriveKey(System.Numerics.BigInteger otherPublicKey, System.Numerics.BigInteger privateKey, DHParameters parameters, int keyLength, byte[] salt = null)
        {
            byte[] sharedSecret = ComputeSharedSecret(otherPublicKey, privateKey, parameters);

            using var kdf = new Rfc2898DeriveBytes(sharedSecret, salt ?? new byte[16], 10000, HashAlgorithmName.SHA256);
            return kdf.GetBytes(keyLength);
        }

        /// <summary>
        /// 验证公钥是否有效
        /// </summary>
        /// <param name="publicKey">公钥</param>
        /// <param name="parameters">DH 参数</param>
        /// <returns>是否有效</returns>
        public static bool ValidatePublicKey(System.Numerics.BigInteger publicKey, DHParameters parameters)
        {
            var p = parameters.P;
            var g = parameters.G;

            // 公钥必须在 [2, p-1] 范围内
            if (publicKey < 2 || publicKey >= p)
                return false;

            // 公钥不能是 p-1 的因子
            if (publicKey == p - 1)
                return false;

            return true;
        }

        /// <summary>
        /// 生成 DH 参数
        /// </summary>
        /// <param name="keySize">密钥长度</param>
        /// <returns>DH 参数</returns>
        public static DHParameters GenerateParameters(int keySize)
        {
            using var rng = RandomNumberGenerator.Create();
            var (p, g) = GenerateParametersInternal(keySize, rng);
            return new DHParameters(p, g);
        }

        private static (System.Numerics.BigInteger p, System.Numerics.BigInteger g) GenerateParametersInternal(int keySize, RandomNumberGenerator rng)
        {
            // 生成安全素数 p = 2q + 1，其中 q 也是素数
            byte[] pBytes = new byte[keySize / 8];
            System.Numerics.BigInteger p, q;

            do
            {
                rng.GetBytes(pBytes);
                pBytes[pBytes.Length - 1] |= 0x01; // 奇数
                pBytes[0] |= 0x80; // 高位为1

                p = new System.Numerics.BigInteger(pBytes);
                p = System.Numerics.BigInteger.Abs(p);

                q = (p - 1) / 2;

            } while (!IsProbablyPrime(q) || !IsProbablyPrime(p));

            // 找生成元 g
            System.Numerics.BigInteger g = 2;
            while (ModPow(g, 2, p) == 1 || ModPow(g, q, p) == 1)
            {
                g++;
            }

            return (p, g);
        }

        private static System.Numerics.BigInteger ModPow(System.Numerics.BigInteger b, System.Numerics.BigInteger e, System.Numerics.BigInteger m)
        {
            return System.Numerics.BigInteger.ModPow(b, e, m);
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
            byte[] bytes = n.ToByteArray();

            for (int i = 0; i < k; i++)
            {
                byte[] aBytes = new byte[bytes.Length];
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
    /// DH 参数
    /// </summary>
    public class DHParameters
    {
        public System.Numerics.BigInteger P { get; }
        public System.Numerics.BigInteger G { get; }

        public DHParameters(System.Numerics.BigInteger p, System.Numerics.BigInteger g)
        {
            P = p;
            G = g;
        }

        public byte[] ToByteArray()
        {
            byte[] pBytes = P.ToByteArray();
            byte[] gBytes = G.ToByteArray();

            byte[] result = new byte[8 + pBytes.Length + gBytes.Length];
            BitConverter.GetBytes(pBytes.Length).CopyTo(result, 0);
            pBytes.CopyTo(result, 4);
            BitConverter.GetBytes(gBytes.Length).CopyTo(result, 4 + pBytes.Length);
            gBytes.CopyTo(result, 8 + pBytes.Length);

            return result;
        }

        public static DHParameters FromByteArray(byte[] data)
        {
            int pLength = BitConverter.ToInt32(data, 0);
            int gLength = BitConverter.ToInt32(data, 4 + pLength);

            byte[] pBytes = new byte[pLength];
            byte[] gBytes = new byte[gLength];

            Array.Copy(data, 4, pBytes, 0, pLength);
            Array.Copy(data, 8 + pLength, gBytes, 0, gLength);

            return new DHParameters(
                new System.Numerics.BigInteger(pBytes),
                new System.Numerics.BigInteger(gBytes)
            );
        }

        public string ToBase64()
        {
            return Convert.ToBase64String(ToByteArray());
        }

        public static DHParameters FromBase64(string base64)
        {
            return FromByteArray(Convert.FromBase64String(base64));
        }
    }

    /// <summary>
    /// DH 密钥对
    /// </summary>
    public class DHKeyPair
    {
        public DHParameters Parameters { get; }
        public System.Numerics.BigInteger PrivateKey { get; }
        public System.Numerics.BigInteger PublicKey { get; }

        public DHKeyPair(DHParameters parameters, System.Numerics.BigInteger privateKey, System.Numerics.BigInteger publicKey)
        {
            Parameters = parameters;
            PrivateKey = privateKey;
            PublicKey = publicKey;
        }

        /// <summary>
        /// 计算与对方公钥的共享密钥
        /// </summary>
        public byte[] ComputeSharedSecret(System.Numerics.BigInteger otherPublicKey)
        {
            return DiffieHellmanUtil.ComputeSharedSecret(otherPublicKey, PrivateKey, Parameters);
        }

        /// <summary>
        /// 派生对称密钥
        /// </summary>
        public byte[] DeriveKey(System.Numerics.BigInteger otherPublicKey, int keyLength, byte[] salt = null)
        {
            return DiffieHellmanUtil.DeriveKey(otherPublicKey, PrivateKey, Parameters, keyLength, salt);
        }

        /// <summary>
        /// 导出公钥
        /// </summary>
        public byte[] ExportPublicKey()
        {
            return PublicKey.ToByteArray();
        }

        /// <summary>
        /// 导出公钥为 Base64
        /// </summary>
        public string ExportPublicKeyBase64()
        {
            return Convert.ToBase64String(ExportPublicKey());
        }

        /// <summary>
        /// 导出私钥（谨慎使用）
        /// </summary>
        public byte[] ExportPrivateKey()
        {
            byte[] paramBytes = Parameters.ToByteArray();
            byte[] keyBytes = PrivateKey.ToByteArray();

            byte[] result = new byte[4 + paramBytes.Length + keyBytes.Length];
            BitConverter.GetBytes(paramBytes.Length).CopyTo(result, 0);
            paramBytes.CopyTo(result, 4);
            keyBytes.CopyTo(result, 4 + paramBytes.Length);

            return result;
        }

        /// <summary>
        /// 导入私钥
        /// </summary>
        public static DHKeyPair ImportPrivateKey(byte[] data)
        {
            int paramLength = BitConverter.ToInt32(data, 0);
            byte[] paramBytes = new byte[paramLength];
            byte[] keyBytes = new byte[data.Length - 4 - paramLength];

            Array.Copy(data, 4, paramBytes, 0, paramLength);
            Array.Copy(data, 4 + paramLength, keyBytes, 0, keyBytes.Length);

            var parameters = DHParameters.FromByteArray(paramBytes);
            var privateKey = new System.Numerics.BigInteger(keyBytes);
            var publicKey = System.Numerics.BigInteger.ModPow(parameters.G, privateKey, parameters.P);

            return new DHKeyPair(parameters, privateKey, publicKey);
        }
    }
}
