using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyTool.MathCategory
{
    /// <summary>
    /// 质数工具类
    /// 提供质数相关的计算功能
    /// </summary>
    public static class PrimeUtil
    {
        /// <summary>
        /// 检查是否为质数
        /// </summary>
        /// <param name="n">数字</param>
        /// <returns>是否为质数</returns>
        public static bool IsPrime(long n)
        {
            if (n < 2)
                return false;

            if (n == 2)
                return true;

            if (n % 2 == 0)
                return false;

            var sqrt = (long)Math.Sqrt(n);

            for (long i = 3; i <= sqrt; i += 2)
            {
                if (n % i == 0)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 使用 Miller-Rabin 算法检查大数是否为质数
        /// </summary>
        /// <param name="n">数字</param>
        /// <param name="iterations">迭代次数（精度）</param>
        /// <returns>是否为质数</returns>
        public static bool IsPrimeMillerRabin(long n, int iterations = 5)
        {
            if (n < 2)
                return false;

            if (n == 2 || n == 3)
                return true;

            if (n % 2 == 0)
                return false;

            // 将 n-1 分解为 2^r * d
            long d = n - 1;
            int r = 0;

            while (d % 2 == 0)
            {
                d /= 2;
                r++;
            }

            var random = new Random();

            for (int i = 0; i < iterations; i++)
            {
                long a = 2 + (long)(random.NextDouble() * (n - 4));
                long x = ModPow(a, d, n);

                if (x == 1 || x == n - 1)
                    continue;

                bool composite = true;

                for (int j = 0; j < r - 1; j++)
                {
                    x = ModPow(x, 2, n);

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

        /// <summary>
        /// 获取下一个质数
        /// </summary>
        /// <param name="n">起始数字</param>
        /// <returns>下一个质数</returns>
        public static long NextPrime(long n)
        {
            if (n < 2)
                return 2;

            long candidate = n + 1;

            if (candidate % 2 == 0)
                candidate++;

            while (!IsPrime(candidate))
            {
                candidate += 2;
            }

            return candidate;
        }

        /// <summary>
        /// 获取前一个质数
        /// </summary>
        /// <param name="n">起始数字</param>
        /// <returns>前一个质数，如果不存在返回 -1</returns>
        public static long PreviousPrime(long n)
        {
            if (n <= 2)
                return -1;

            if (n == 3)
                return 2;

            long candidate = n - 1;

            if (candidate % 2 == 0)
                candidate--;

            while (candidate >= 2 && !IsPrime(candidate))
            {
                candidate -= 2;
            }

            return candidate >= 2 ? candidate : -1;
        }

        /// <summary>
        /// 获取范围内的所有质数
        /// </summary>
        /// <param name="start">起始数字</param>
        /// <param name="end">结束数字</param>
        /// <returns>质数列表</returns>
        public static List<long> GetPrimesInRange(long start, long end)
        {
            var primes = new List<long>();

            for (long i = start; i <= end; i++)
            {
                if (IsPrime(i))
                    primes.Add(i);
            }

            return primes;
        }

        /// <summary>
        /// 使用埃拉托斯特尼筛法获取指定范围内的所有质数
        /// </summary>
        /// <param name="limit">上限</param>
        /// <returns>质数列表</returns>
        public static List<long> SieveOfEratosthenes(long limit)
        {
            if (limit < 2)
                return new List<long>();

            var isPrime = new bool[limit + 1];
            Array.Fill(isPrime, true);

            isPrime[0] = false;
            isPrime[1] = false;

            var sqrt = (long)Math.Sqrt(limit);

            for (long i = 2; i <= sqrt; i++)
            {
                if (isPrime[i])
                {
                    for (long j = i * i; j <= limit; j += i)
                    {
                        isPrime[j] = false;
                    }
                }
            }

            var primes = new List<long>();

            for (long i = 2; i <= limit; i++)
            {
                if (isPrime[i])
                    primes.Add(i);
            }

            return primes;
        }

        /// <summary>
        /// 获取质因数分解
        /// </summary>
        /// <param name="n">数字</param>
        /// <returns>质因数及其幂次的字典</returns>
        public static Dictionary<long, int> PrimeFactorization(long n)
        {
            var factors = new Dictionary<long, int>();

            if (n < 2)
                return factors;

            // 处理因子 2
            while (n % 2 == 0)
            {
                if (factors.ContainsKey(2))
                    factors[2]++;
                else
                    factors[2] = 1;

                n /= 2;
            }

            // 处理奇数因子
            for (long i = 3; i * i <= n; i += 2)
            {
                while (n % i == 0)
                {
                    if (factors.ContainsKey(i))
                        factors[i]++;
                    else
                        factors[i] = 1;

                    n /= i;
                }
            }

            // 如果剩下的 n 大于 1，则它本身是质数
            if (n > 1)
            {
                factors[n] = 1;
            }

            return factors;
        }

        /// <summary>
        /// 获取所有因数
        /// </summary>
        /// <param name="n">数字</param>
        /// <returns>因数列表</returns>
        public static List<long> GetDivisors(long n)
        {
            var divisors = new List<long>();

            if (n < 1)
                return divisors;

            var sqrt = (long)Math.Sqrt(n);

            for (long i = 1; i <= sqrt; i++)
            {
                if (n % i == 0)
                {
                    divisors.Add(i);

                    if (i != n / i)
                    {
                        divisors.Add(n / i);
                    }
                }
            }

            divisors.Sort();
            return divisors;
        }

        /// <summary>
        /// 计算因数个数
        /// </summary>
        /// <param name="n">数字</param>
        /// <returns>因数个数</returns>
        public static long CountDivisors(long n)
        {
            if (n < 1)
                return 0;

            var factors = PrimeFactorization(n);
            long count = 1;

            foreach (var power in factors.Values)
            {
                count *= (power + 1);
            }

            return count;
        }

        /// <summary>
        /// 计算最大公约数
        /// </summary>
        /// <param name="a">数字1</param>
        /// <param name="b">数字2</param>
        /// <returns>最大公约数</returns>
        public static long Gcd(long a, long b)
        {
            a = Math.Abs(a);
            b = Math.Abs(b);

            while (b != 0)
            {
                var temp = b;
                b = a % b;
                a = temp;
            }

            return a;
        }

        /// <summary>
        /// 计算最小公倍数
        /// </summary>
        /// <param name="a">数字1</param>
        /// <param name="b">数字2</param>
        /// <returns>最小公倍数</returns>
        public static long Lcm(long a, long b)
        {
            if (a == 0 || b == 0)
                return 0;

            return Math.Abs(a * b) / Gcd(a, b);
        }

        /// <summary>
        /// 计算多个数的最大公约数
        /// </summary>
        /// <param name="numbers">数字数组</param>
        /// <returns>最大公约数</returns>
        public static long Gcd(params long[] numbers)
        {
            if (numbers == null || numbers.Length == 0)
                return 0;

            long result = numbers[0];

            for (int i = 1; i < numbers.Length; i++)
            {
                result = Gcd(result, numbers[i]);
            }

            return result;
        }

        /// <summary>
        /// 计算多个数的最小公倍数
        /// </summary>
        /// <param name="numbers">数字数组</param>
        /// <returns>最小公倍数</returns>
        public static long Lcm(params long[] numbers)
        {
            if (numbers == null || numbers.Length == 0)
                return 0;

            long result = numbers[0];

            for (int i = 1; i < numbers.Length; i++)
            {
                result = Lcm(result, numbers[i]);
            }

            return result;
        }

        /// <summary>
        /// 计算欧拉函数 φ(n)
        /// </summary>
        /// <param name="n">数字</param>
        /// <returns>欧拉函数值</returns>
        public static long EulerTotient(long n)
        {
            if (n < 1)
                return 0;

            var factors = PrimeFactorization(n);
            long result = n;

            foreach (var p in factors.Keys)
            {
                result = result / p * (p - 1);
            }

            return result;
        }

        /// <summary>
        /// 判断是否为互质数
        /// </summary>
        /// <param name="a">数字1</param>
        /// <param name="b">数字2</param>
        /// <returns>是否互质</returns>
        public static bool AreCoprime(long a, long b)
        {
            return Gcd(a, b) == 1;
        }

        /// <summary>
        /// 获取第 n 个质数（从1开始）
        /// </summary>
        /// <param name="n">序号</param>
        /// <returns>第 n 个质数</returns>
        public static long GetNthPrime(int n)
        {
            if (n < 1)
                throw new ArgumentException("n must be positive");

            if (n == 1)
                return 2;

            int count = 1;
            long candidate = 1;

            while (count < n)
            {
                candidate += 2;

                if (IsPrime(candidate))
                    count++;
            }

            return candidate;
        }

        /// <summary>
        /// 判断是否为梅森数
        /// </summary>
        /// <param name="n">数字</param>
        /// <returns>是否为梅森数</returns>
        public static bool IsMersennePrime(long n)
        {
            // 梅森数形式为 2^p - 1，其中 p 是质数
            n = n + 1;

            if (n <= 2 || (n & (n - 1)) != 0)
                return false;

            int p = 0;
            while (n > 1)
            {
                n >>= 1;
                p++;
            }

            return IsPrime(p);
        }

        #region 私有方法

        private static long ModPow(long baseVal, long exponent, long modulus)
        {
            long result = 1;
            baseVal %= modulus;

            while (exponent > 0)
            {
                if (exponent % 2 == 1)
                {
                    result = (result * baseVal) % modulus;
                }

                exponent >>= 1;
                baseVal = (baseVal * baseVal) % modulus;
            }

            return result;
        }

        #endregion
    }
}
