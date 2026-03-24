using System;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace EasyTool.CodeCategory
{
    /// <summary>
    /// SM2 椭圆曲线公钥密码工具类
    /// SM2 是中国国家密码管理局发布的椭圆曲线公钥密码算法
    /// 用于数字签名、密钥交换和公钥加密
    /// 基于 256 位椭圆曲线
    /// </summary>
    public static class Sm2Util
    {
        // SM2 推荐椭圆曲线参数
        private static readonly BigInteger P = BigInteger.Parse("FFFFFFFEFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF00000000FFFFFFFFFFFFFFFF", System.Globalization.NumberStyles.HexNumber);
        private static readonly BigInteger A = BigInteger.Parse("FFFFFFFEFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF00000000FFFFFFFFFFFFFFFC", System.Globalization.NumberStyles.HexNumber);
        private static readonly BigInteger B = BigInteger.Parse("28E9FA9E9D9F5E344D5A9E4BCF6509A7F39789F515AB8F92DDBCBD414D940E93", System.Globalization.NumberStyles.HexNumber);
        private static readonly BigInteger N = BigInteger.Parse("FFFFFFFEFFFFFFFFFFFFFFFFFFFFFFFF7203DF6B21C6052B53BBF40939D54123", System.Globalization.NumberStyles.HexNumber);
        private static readonly BigInteger Gx = BigInteger.Parse("32C4AE2C1F1981195F9904466A39C9948FE30BBFF2660BE1715A4589334C74C7", System.Globalization.NumberStyles.HexNumber);
        private static readonly BigInteger Gy = BigInteger.Parse("BC3736A2F4F6779C59BDCEE36B692153D0A9877CC62A474002DF32E52139F0A0", System.Globalization.NumberStyles.HexNumber);

        // 基点 G
        private static readonly ECPoint G = new ECPoint { X = Gx, Y = Gy };

        // 用户 ID（默认值）
        private const string DefaultUserId = "1234567812345678";

        #region 密钥生成

        /// <summary>
        /// 生成 SM2 密钥对
        /// </summary>
        /// <returns>密钥对（私钥和公钥）</returns>
        public static (byte[] PrivateKey, byte[] PublicKey) GenerateKeyPair()
        {
            byte[] privateKey = new byte[32];
            using var rng = RandomNumberGenerator.Create();

            // 生成私钥（1 到 n-1 之间的随机数）
            do
            {
                rng.GetBytes(privateKey);
                var d = new BigInteger(privateKey, true, true);
                if (d > 0 && d < N)
                    break;
            } while (true);

            // 计算公钥 P = d * G
            var publicKey = ScalarMultiply(privateKey, G);

            return (privateKey, EncodePoint(publicKey));
        }

        /// <summary>
        /// 从私钥导出公钥
        /// </summary>
        /// <param name="privateKey">私钥（32字节）</param>
        /// <returns>公钥（65字节，未压缩格式）</returns>
        public static byte[] DerivePublicKey(byte[] privateKey)
        {
            if (privateKey == null || privateKey.Length != 32)
                throw new ArgumentException("Private key must be 32 bytes", nameof(privateKey));

            var publicKey = ScalarMultiply(privateKey, G);
            return EncodePoint(publicKey);
        }

        #endregion

        #region 加密解密

        /// <summary>
        /// 使用 SM2 公钥加密数据
        /// </summary>
        /// <param name="plainText">明文</param>
        /// <param name="publicKey">公钥（65字节）</param>
        /// <returns>密文（C1 || C3 || C2 格式）</returns>
        public static byte[] Encrypt(byte[] plainText, byte[] publicKey)
        {
            if (plainText == null)
                throw new ArgumentNullException(nameof(plainText));
            if (publicKey == null || publicKey.Length != 65)
                throw new ArgumentException("Public key must be 65 bytes", nameof(publicKey));

            var pubPoint = DecodePoint(publicKey);

            byte[] kBytes = new byte[32];
            ECPoint c1;
            BigInteger k;

            using var rng = RandomNumberGenerator.Create();

            // 生成随机数 k
            do
            {
                rng.GetBytes(kBytes);
                k = new BigInteger(kBytes, true, true);
                if (k > 0 && k < N)
                    break;
            } while (true);

            // C1 = k * G
            c1 = ScalarMultiply(kBytes, G);

            // S = k * PB（检查 S 是否为无穷远点）
            var s = ScalarMultiply(kBytes, pubPoint);
            if (IsInfinity(s))
                throw new CryptographicException("Invalid public key");

            // KDF 密钥派生
            byte[] kdfInput = new byte[64];
            Array.Copy(s.X.ToByteArray(true, true), 0, kdfInput, 0, 32);
            Array.Copy(s.Y.ToByteArray(true, true), 0, kdfInput, 32, 32);

            byte[] kdfOutput = Kdf(kdfInput, plainText.Length);

            // C2 = M XOR KDF_output
            byte[] c2 = new byte[plainText.Length];
            for (int i = 0; i < plainText.Length; i++)
            {
                c2[i] = (byte)(plainText[i] ^ kdfOutput[i]);
            }

            // C3 = SM3(C1x || C1y || M)
            byte[] c3Input = new byte[64 + plainText.Length];
            Array.Copy(c1.X.ToByteArray(true, true), 0, c3Input, 0, 32);
            Array.Copy(c1.Y.ToByteArray(true, true), 0, c3Input, 32, 32);
            Array.Copy(plainText, 0, c3Input, 64, plainText.Length);

            byte[] c3 = Sm3Hash(c3Input);

            // 组合结果：C1 || C3 || C2
            byte[] result = new byte[65 + 32 + plainText.Length];
            Array.Copy(EncodePoint(c1), 0, result, 0, 65);
            Array.Copy(c3, 0, result, 65, 32);
            Array.Copy(c2, 0, result, 97, c2.Length);

            return result;
        }

        /// <summary>
        /// 使用 SM2 私钥解密数据
        /// </summary>
        /// <param name="cipherText">密文</param>
        /// <param name="privateKey">私钥（32字节）</param>
        /// <returns>明文</returns>
        public static byte[] Decrypt(byte[] cipherText, byte[] privateKey)
        {
            if (cipherText == null || cipherText.Length < 97)
                throw new ArgumentException("Invalid cipher text", nameof(cipherText));
            if (privateKey == null || privateKey.Length != 32)
                throw new ArgumentException("Private key must be 32 bytes", nameof(privateKey));

            // 解析密文
            byte[] c1Bytes = new byte[65];
            byte[] c3 = new byte[32];
            int c2Length = cipherText.Length - 97;
            byte[] c2 = new byte[c2Length];

            Array.Copy(cipherText, 0, c1Bytes, 0, 65);
            Array.Copy(cipherText, 65, c3, 0, 32);
            Array.Copy(cipherText, 97, c2, 0, c2Length);

            var c1 = DecodePoint(c1Bytes);

            // S = dB * C1（检查 S 是否为无穷远点）
            var s = ScalarMultiply(privateKey, c1);
            if (IsInfinity(s))
                throw new CryptographicException("Invalid cipher text");

            // KDF 密钥派生
            byte[] kdfInput = new byte[64];
            Array.Copy(s.X.ToByteArray(true, true), 0, kdfInput, 0, 32);
            Array.Copy(s.Y.ToByteArray(true, true), 0, kdfInput, 32, 32);

            byte[] kdfOutput = Kdf(kdfInput, c2Length);

            // M = C2 XOR KDF_output
            byte[] plainText = new byte[c2Length];
            for (int i = 0; i < c2Length; i++)
            {
                plainText[i] = (byte)(c2[i] ^ kdfOutput[i]);
            }

            // 验证 C3 = SM3(C1x || C1y || M)
            byte[] c3Input = new byte[64 + plainText.Length];
            Array.Copy(c1.X.ToByteArray(true, true), 0, c3Input, 0, 32);
            Array.Copy(c1.Y.ToByteArray(true, true), 0, c3Input, 32, 32);
            Array.Copy(plainText, 0, c3Input, 64, plainText.Length);

            byte[] expectedC3 = Sm3Hash(c3Input);

            if (!ConstantTimeEquals(c3, expectedC3))
                throw new CryptographicException("Invalid cipher text: checksum mismatch");

            return plainText;
        }

        /// <summary>
        /// 加密字符串并返回 Base64
        /// </summary>
        /// <param name="plainText">明文</param>
        /// <param name="publicKey">公钥</param>
        /// <returns>Base64 密文</returns>
        public static string EncryptToBase64(string plainText, byte[] publicKey)
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
        /// <param name="cipherText">Base64 密文</param>
        /// <param name="privateKey">私钥</param>
        /// <returns>明文字符串</returns>
        public static string DecryptFromBase64(string cipherText, byte[] privateKey)
        {
            if (string.IsNullOrEmpty(cipherText))
                return string.Empty;

            byte[] data = Convert.FromBase64String(cipherText);
            byte[] decrypted = Decrypt(data, privateKey);
            return Encoding.UTF8.GetString(decrypted);
        }

        #endregion

        #region 签名验签

        /// <summary>
        /// 使用 SM2 私钥签名数据
        /// </summary>
        /// <param name="data">要签名的数据</param>
        /// <param name="privateKey">私钥（32字节）</param>
        /// <param name="userId">用户 ID（可选）</param>
        /// <returns>签名（64字节，R || S）</returns>
        public static byte[] Sign(byte[] data, byte[] privateKey, string userId = null)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (privateKey == null || privateKey.Length != 32)
                throw new ArgumentException("Private key must be 32 bytes", nameof(privateKey));

            userId ??= DefaultUserId;

            // 计算 Z 值
            var publicKey = ScalarMultiply(privateKey, G);
            byte[] z = CalculateZ(publicKey, userId);

            // e = SM3(Z || M)
            byte[] eInput = new byte[z.Length + data.Length];
            Array.Copy(z, eInput, z.Length);
            Array.Copy(data, 0, eInput, z.Length, data.Length);
            byte[] eHash = Sm3Hash(eInput);
            BigInteger e = new BigInteger(eHash, true, true);

            byte[] kBytes = new byte[32];
            BigInteger r, s;
            var d = new BigInteger(privateKey, true, true);

            using var rng = RandomNumberGenerator.Create();

            do
            {
                // 生成随机数 k
                do
                {
                    rng.GetBytes(kBytes);
                    var k = new BigInteger(kBytes, true, true);
                    if (k > 0 && k < N)
                        break;
                } while (true);

                // 计算 x1, y1 = k * G
                var point = ScalarMultiply(kBytes, G);

                // r = (e + x1) mod n
                r = (e + point.X) % N;
                if (r == 0 || r + new BigInteger(kBytes, true, true) == N)
                    continue;

                // s = ((1 + d)^-1 * (k - r * d)) mod n
                var dPlusOne = (d + 1) % N;
                var dPlusOneInv = ModInverse(dPlusOne, N);
                s = (dPlusOneInv * ((new BigInteger(kBytes, true, true) - r * d) % N + N)) % N;

                if (s != 0)
                    break;

            } while (true);

            // 组合签名 R || S
            byte[] result = new byte[64];
            Array.Copy(r.ToByteArray(true, true), 0, result, 0, Math.Min(32, r.ToByteArray(true, true).Length));
            Array.Copy(s.ToByteArray(true, true), 0, result, 32, Math.Min(32, s.ToByteArray(true, true).Length));

            return result;
        }

        /// <summary>
        /// 使用 SM2 公钥验证签名
        /// </summary>
        /// <param name="data">原始数据</param>
        /// <param name="signature">签名（64字节）</param>
        /// <param name="publicKey">公钥（65字节）</param>
        /// <param name="userId">用户 ID（可选）</param>
        /// <returns>签名是否有效</returns>
        public static bool Verify(byte[] data, byte[] signature, byte[] publicKey, string userId = null)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (signature == null || signature.Length != 64)
                throw new ArgumentException("Signature must be 64 bytes", nameof(signature));
            if (publicKey == null || publicKey.Length != 65)
                throw new ArgumentException("Public key must be 65 bytes", nameof(publicKey));

            userId ??= DefaultUserId;

            // 解析签名
            byte[] rBytes = new byte[32];
            byte[] sBytes = new byte[32];
            Array.Copy(signature, 0, rBytes, 0, 32);
            Array.Copy(signature, 32, sBytes, 0, 32);

            BigInteger r = new BigInteger(rBytes, true, true);
            BigInteger s = new BigInteger(sBytes, true, true);

            // 验证 r, s 范围
            if (r < 1 || r >= N || s < 1 || s >= N)
                return false;

            // 计算 Z 值
            var pubPoint = DecodePoint(publicKey);
            byte[] z = CalculateZ(pubPoint, userId);

            // e = SM3(Z || M)
            byte[] eInput = new byte[z.Length + data.Length];
            Array.Copy(z, eInput, z.Length);
            Array.Copy(data, 0, eInput, z.Length, data.Length);
            byte[] eHash = Sm3Hash(eInput);
            BigInteger e = new BigInteger(eHash, true, true);

            // t = (r + s) mod n
            BigInteger t = (r + s) % N;
            if (t == 0)
                return false;

            // 计算 (x1, y1) = s * G + t * PA
            var sG = ScalarMultiply(sBytes, G);

            // 需要将 t 转换为字节数组
            byte[] tBytes = t.ToByteArray(true, true);
            if (tBytes.Length > 32)
                return false;
            byte[] tBytesPadded = new byte[32];
            Array.Copy(tBytes, tBytesPadded, tBytes.Length);

            var tPA = ScalarMultiply(tBytesPadded, pubPoint);
            var point = PointAdd(sG, tPA);

            // 验证 R = (e + x1) mod n == r
            BigInteger R = (e + point.X) % N;

            return R == r;
        }

        /// <summary>
        /// 对字符串签名并返回 Base64
        /// </summary>
        /// <param name="text">要签名的文本</param>
        /// <param name="privateKey">私钥</param>
        /// <param name="userId">用户 ID</param>
        /// <returns>Base64 签名</returns>
        public static string SignToBase64(string text, byte[] privateKey, string userId = null)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            byte[] data = Encoding.UTF8.GetBytes(text);
            byte[] signature = Sign(data, privateKey, userId);
            return Convert.ToBase64String(signature);
        }

        /// <summary>
        /// 验证 Base64 签名
        /// </summary>
        /// <param name="text">原始文本</param>
        /// <param name="signatureBase64">Base64 签名</param>
        /// <param name="publicKey">公钥</param>
        /// <param name="userId">用户 ID</param>
        /// <returns>签名是否有效</returns>
        public static bool VerifyFromBase64(string text, string signatureBase64, byte[] publicKey, string userId = null)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(signatureBase64))
                return false;

            byte[] data = Encoding.UTF8.GetBytes(text);
            byte[] signature = Convert.FromBase64String(signatureBase64);
            return Verify(data, signature, publicKey, userId);
        }

        #endregion

        #region 私有方法

        private static ECPoint ScalarMultiply(byte[] k, ECPoint point)
        {
            var scalar = new BigInteger(k, true, true);
            var result = PointMultiply(scalar, point);
            return result;
        }

        private static ECPoint PointMultiply(BigInteger k, ECPoint point)
        {
            if (k == 0 || IsInfinity(point))
                return InfinityPoint();

            ECPoint result = InfinityPoint();
            ECPoint temp = point;
            var absK = BigInteger.Abs(k);

            while (absK > 0)
            {
                if ((absK & 1) == 1)
                {
                    result = PointAdd(result, temp);
                }
                temp = PointDouble(temp);
                absK >>= 1;
            }

            return result;
        }

        private static ECPoint PointAdd(ECPoint p1, ECPoint p2)
        {
            if (IsInfinity(p1)) return p2;
            if (IsInfinity(p2)) return p1;

            if (p1.X == p2.X)
            {
                if ((p1.Y + p2.Y) % P == 0)
                    return InfinityPoint();

                return PointDouble(p1);
            }

            // λ = (y2 - y1) / (x2 - x1)
            var dx = (p2.X - p1.X + P) % P;
            var dy = (p2.Y - p1.Y + P) % P;
            var lambda = (dy * ModInverse(dx, P)) % P;

            // x3 = λ² - x1 - x2
            var x3 = (lambda * lambda - p1.X - p2.X + 2 * P) % P;

            // y3 = λ(x1 - x3) - y1
            var y3 = (lambda * (p1.X - x3 + P) - p1.Y + P) % P;

            return new ECPoint { X = x3, Y = y3 };
        }

        private static ECPoint PointDouble(ECPoint p)
        {
            if (IsInfinity(p))
                return InfinityPoint();

            // λ = (3x² + a) / (2y)
            var x2 = (p.X * p.X) % P;
            var numerator = (3 * x2 + A) % P;
            var denominator = (2 * p.Y) % P;
            var lambda = (numerator * ModInverse(denominator, P)) % P;

            // x3 = λ² - 2x
            var x3 = (lambda * lambda - 2 * p.X + P) % P;

            // y3 = λ(x - x3) - y
            var y3 = (lambda * (p.X - x3 + P) - p.Y + P) % P;

            return new ECPoint { X = x3, Y = y3 };
        }

        private static ECPoint InfinityPoint()
        {
            return new ECPoint { X = BigInteger.Zero, Y = BigInteger.Zero };
        }

        private static bool IsInfinity(ECPoint p)
        {
            return p.X == BigInteger.Zero && p.Y == BigInteger.Zero;
        }

        private static BigInteger ModInverse(BigInteger a, BigInteger n)
        {
            if (a < 0) a = (a % n + n) % n;

            BigInteger t = 0, newT = 1;
            BigInteger r = n, newR = a;

            while (newR != 0)
            {
                var quotient = r / newR;
                var tempT = t;
                t = newT;
                newT = tempT - quotient * newT;

                var tempR = r;
                r = newR;
                newR = tempR - quotient * newR;
            }

            if (t < 0) t = (t % n + n) % n;

            return t;
        }

        private static byte[] EncodePoint(ECPoint point)
        {
            byte[] result = new byte[65];
            result[0] = 0x04; // 未压缩格式

            var xBytes = point.X.ToByteArray(true, true);
            var yBytes = point.Y.ToByteArray(true, true);

            Array.Copy(xBytes, 0, result, 1 + (32 - xBytes.Length), xBytes.Length);
            Array.Copy(yBytes, 0, result, 33 + (32 - yBytes.Length), yBytes.Length);

            return result;
        }

        private static ECPoint DecodePoint(byte[] data)
        {
            if (data == null || data.Length != 65 || data[0] != 0x04)
                throw new ArgumentException("Invalid point encoding", nameof(data));

            byte[] xBytes = new byte[32];
            byte[] yBytes = new byte[32];
            Array.Copy(data, 1, xBytes, 0, 32);
            Array.Copy(data, 33, yBytes, 0, 32);

            return new ECPoint
            {
                X = new BigInteger(xBytes, true, true),
                Y = new BigInteger(yBytes, true, true)
            };
        }

        private static byte[] CalculateZ(ECPoint publicKey, string userId)
        {
            byte[] idBytes = Encoding.UTF8.GetBytes(userId);
            int idBits = idBytes.Length * 8;

            byte[] entl = new byte[2];
            entl[0] = (byte)((idBits >> 8) & 0xFF);
            entl[1] = (byte)(idBits & 0xFF);

            // Z = SM3(ENTLA || IDA || a || b || Gx || Gy || Ax || Ay)
            byte[] aBytes = A.ToByteArray(true, true);
            byte[] bBytes = B.ToByteArray(true, true);
            byte[] gxBytes = Gx.ToByteArray(true, true);
            byte[] gyBytes = Gy.ToByteArray(true, true);
            byte[] axBytes = publicKey.X.ToByteArray(true, true);
            byte[] ayBytes = publicKey.Y.ToByteArray(true, true);

            byte[] input = new byte[2 + idBytes.Length + 32 * 6];
            int offset = 0;

            Array.Copy(entl, 0, input, offset, 2);
            offset += 2;
            Array.Copy(idBytes, 0, input, offset, idBytes.Length);
            offset += idBytes.Length;

            CopyPadded(aBytes, input, ref offset, 32);
            CopyPadded(bBytes, input, ref offset, 32);
            CopyPadded(gxBytes, input, ref offset, 32);
            CopyPadded(gyBytes, input, ref offset, 32);
            CopyPadded(axBytes, input, ref offset, 32);
            CopyPadded(ayBytes, input, ref offset, 32);

            return Sm3Hash(input);
        }

        private static void CopyPadded(byte[] src, byte[] dest, ref int offset, int length)
        {
            int padLength = length - src.Length;
            if (padLength > 0)
            {
                offset += padLength;
            }
            Array.Copy(src, 0, dest, offset, Math.Min(src.Length, length));
            offset += Math.Min(src.Length, length);
        }

        private static byte[] Kdf(byte[] z, int keyLength)
        {
            byte[] result = new byte[keyLength];
            int counter = 1;
            int generated = 0;

            while (generated < keyLength)
            {
                byte[] counterBytes = BitConverter.GetBytes(counter);
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(counterBytes);

                byte[] input = new byte[z.Length + 4];
                Array.Copy(z, input, z.Length);
                Array.Copy(counterBytes, 0, input, z.Length, 4);

                byte[] hash = Sm3Hash(input);

                int copyLength = Math.Min(32, keyLength - generated);
                Array.Copy(hash, 0, result, generated, copyLength);
                generated += copyLength;
                counter++;
            }

            return result;
        }

        private static byte[] Sm3Hash(byte[] data)
        {
            return Sm3Util.ComputeHash(data);
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

        /// <summary>
        /// 椭圆曲线点
        /// </summary>
        private struct ECPoint
        {
            public BigInteger X;
            public BigInteger Y;
        }
    }
}
