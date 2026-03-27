using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyTool.MathCategory
{
    /// <summary>
    /// 组合数学工具类
    /// 提供排列、组合等计算功能
    /// </summary>
    public static class CombinatoricsUtil
    {
        #region 阶乘

        /// <summary>
        /// 计算阶乘
        /// </summary>
        /// <param name="n">非负整数</param>
        /// <returns>n!</returns>
        public static long Factorial(int n)
        {
            if (n < 0)
                throw new ArgumentException("阶乘只能计算非负整数");

            if (n <= 1)
                return 1;

            long result = 1;

            for (int i = 2; i <= n; i++)
            {
                result *= i;
            }

            return result;
        }

        /// <summary>
        /// 计算大数阶乘
        /// </summary>
        /// <param name="n">非负整数</param>
        /// <returns>n! 的字符串表示</returns>
        public static string FactorialBig(int n)
        {
            if (n < 0)
                throw new ArgumentException("阶乘只能计算非负整数");

            if (n <= 1)
                return "1";

            var result = new List<int> { 1 };

            for (int i = 2; i <= n; i++)
            {
                int carry = 0;

                for (int j = 0; j < result.Count; j++)
                {
                    int product = result[j] * i + carry;
                    result[j] = product % 10;
                    carry = product / 10;
                }

                while (carry > 0)
                {
                    result.Add(carry % 10);
                    carry /= 10;
                }
            }

            result.Reverse();
            return string.Join("", result);
        }

        #endregion

        #region 排列组合

        /// <summary>
        /// 计算排列数 P(n, r) = n! / (n-r)!
        /// </summary>
        /// <param name="n">总数</param>
        /// <param name="r">选取数</param>
        /// <returns>排列数</returns>
        public static long Permutation(int n, int r)
        {
            if (n < 0 || r < 0 || r > n)
                throw new ArgumentException("参数无效");

            if (r == 0)
                return 1;

            long result = 1;

            for (int i = n; i > n - r; i--)
            {
                result *= i;
            }

            return result;
        }

        /// <summary>
        /// 计算组合数 C(n, r) = n! / (r! * (n-r)!)
        /// </summary>
        /// <param name="n">总数</param>
        /// <param name="r">选取数</param>
        /// <returns>组合数</returns>
        public static long Combination(int n, int r)
        {
            if (n < 0 || r < 0 || r > n)
                throw new ArgumentException("参数无效");

            if (r == 0 || r == n)
                return 1;

            // 使用较小的 r 计算
            r = Math.Min(r, n - r);

            long result = 1;

            for (int i = 0; i < r; i++)
            {
                result = result * (n - i) / (i + 1);
            }

            return result;
        }

        /// <summary>
        /// 计算组合数（大数）
        /// </summary>
        /// <param name="n">总数</param>
        /// <param name="r">选取数</param>
        /// <returns>组合数的字符串表示</returns>
        public static string CombinationBig(int n, int r)
        {
            if (n < 0 || r < 0 || r > n)
                throw new ArgumentException("参数无效");

            if (r == 0 || r == n)
                return "1";

            r = Math.Min(r, n - r);

            var numerator = new List<int>();
            var denominator = new List<int>();

            for (int i = 0; i < r; i++)
            {
                numerator.Add(n - i);
                denominator.Add(i + 1);
            }

            // 约分
            for (int i = 0; i < denominator.Count; i++)
            {
                for (int j = 0; j < numerator.Count; j++)
                {
                    var gcd = PrimeUtil.Gcd(numerator[j], denominator[i]);

                    if (gcd > 1)
                    {
                        numerator[j] /= (int)gcd;
                        denominator[i] /= (int)gcd;

                        if (denominator[i] == 1)
                            break;
                    }
                }
            }

            // 计算乘积
            var result = new List<int> { 1 };

            foreach (var num in numerator)
            {
                int carry = 0;

                for (int j = 0; j < result.Count; j++)
                {
                    int product = result[j] * num + carry;
                    result[j] = product % 10;
                    carry = product / 10;
                }

                while (carry > 0)
                {
                    result.Add(carry % 10);
                    carry /= 10;
                }
            }

            result.Reverse();
            return string.Join("", result);
        }

        #endregion

        #region 排列生成

        /// <summary>
        /// 生成所有排列
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="elements">元素集合</param>
        /// <returns>所有排列</returns>
        public static List<List<T>> GetAllPermutations<T>(IEnumerable<T> elements)
        {
            var list = elements.ToList();
            var result = new List<List<T>>();

            Permute(list, 0, result);

            return result;
        }

        /// <summary>
        /// 生成指定长度的排列
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="elements">元素集合</param>
        /// <param name="length">排列长度</param>
        /// <returns>所有排列</returns>
        public static List<List<T>> GetPermutations<T>(IEnumerable<T> elements, int length)
        {
            var list = elements.ToList();
            var result = new List<List<T>>();

            if (length > list.Count)
                throw new ArgumentException("排列长度不能超过元素数量");

            GeneratePermutations(list, length, new List<T>(), new bool[list.Count], result);

            return result;
        }

        /// <summary>
        /// 生成下一个排列（字典序）
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="elements">当前排列（会被修改）</param>
        /// <returns>是否存在下一个排列</returns>
        public static bool NextPermutation<T>(List<T> elements) where T : IComparable<T>
        {
            int i = elements.Count - 2;

            while (i >= 0 && elements[i].CompareTo(elements[i + 1]) >= 0)
            {
                i--;
            }

            if (i < 0)
                return false;

            int j = elements.Count - 1;

            while (elements[j].CompareTo(elements[i]) <= 0)
            {
                j--;
            }

            // 交换
            var temp = elements[i];
            elements[i] = elements[j];
            elements[j] = temp;

            // 反转
            Reverse(elements, i + 1, elements.Count - 1);

            return true;
        }

        #endregion

        #region 组合生成

        /// <summary>
        /// 生成所有组合
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="elements">元素集合</param>
        /// <returns>所有组合（包括空集）</returns>
        public static List<List<T>> GetAllCombinations<T>(IEnumerable<T> elements)
        {
            var list = elements.ToList();
            var result = new List<List<T>>();

            for (int i = 0; i <= list.Count; i++)
            {
                result.AddRange(GetCombinations(list, i));
            }

            return result;
        }

        /// <summary>
        /// 生成指定长度的组合
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="elements">元素集合</param>
        /// <param name="length">组合长度</param>
        /// <returns>所有组合</returns>
        public static List<List<T>> GetCombinations<T>(IEnumerable<T> elements, int length)
        {
            var list = elements.ToList();
            var result = new List<List<T>>();

            if (length > list.Count)
                return result;

            GenerateCombinations(list, length, 0, new List<T>(), result);

            return result;
        }

        #endregion

        #region 其他组合数学

        /// <summary>
        /// 计算卡特兰数 C_n = C(2n, n) / (n+1)
        /// </summary>
        /// <param name="n">索引</param>
        /// <returns>第 n 个卡特兰数</returns>
        public static long Catalan(int n)
        {
            if (n < 0)
                throw new ArgumentException("n 必须为非负整数");

            return Combination(2 * n, n) / (n + 1);
        }

        /// <summary>
        /// 计算第 n 行的杨辉三角
        /// </summary>
        /// <param name="n">行号（从0开始）</param>
        /// <returns>杨辉三角第 n 行</returns>
        public static List<long> PascalRow(int n)
        {
            var row = new List<long> { 1 };

            for (int i = 1; i <= n; i++)
            {
                row.Add(row[i - 1] * (n - i + 1) / i);
            }

            return row;
        }

        /// <summary>
        /// 生成杨辉三角
        /// </summary>
        /// <param name="rows">行数</param>
        /// <returns>杨辉三角</returns>
        public static List<List<long>> PascalTriangle(int rows)
        {
            var triangle = new List<List<long>>();

            for (int i = 0; i < rows; i++)
            {
                triangle.Add(PascalRow(i));
            }

            return triangle;
        }

        /// <summary>
        /// 计算贝尔数（集合划分数）
        /// </summary>
        /// <param name="n">索引</param>
        /// <returns>第 n 个贝尔数</returns>
        public static long Bell(int n)
        {
            if (n < 0)
                throw new ArgumentException("n 必须为非负整数");

            var bell = new long[n + 1, n + 1];
            bell[0, 0] = 1;

            for (int i = 1; i <= n; i++)
            {
                bell[i, 0] = bell[i - 1, i - 1];

                for (int j = 1; j <= i; j++)
                {
                    bell[i, j] = bell[i - 1, j - 1] + bell[i, j - 1];
                }
            }

            return bell[n, 0];
        }

        /// <summary>
        /// 计算斯特林数（第二类）
        /// </summary>
        /// <param name="n">元素数</param>
        /// <param name="k">集合数</param>
        /// <returns>斯特林数</returns>
        public static long StirlingSecond(int n, int k)
        {
            if (n < 0 || k < 0 || k > n)
                throw new ArgumentException("参数无效");

            if (k == 0)
                return n == 0 ? 1 : 0;

            if (k == 1)
                return 1;

            if (k == n)
                return 1;

            var stirling = new long[n + 1, k + 1];
            stirling[0, 0] = 1;

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= Math.Min(i, k); j++)
                {
                    stirling[i, j] = j * stirling[i - 1, j] + stirling[i - 1, j - 1];
                }
            }

            return stirling[n, k];
        }

        /// <summary>
        /// 计算错排数 D_n
        /// </summary>
        /// <param name="n">元素数</param>
        /// <returns>错排数</returns>
        public static long Derangement(int n)
        {
            if (n < 0)
                throw new ArgumentException("n 必须为非负整数");

            if (n == 0)
                return 1;

            if (n == 1)
                return 0;

            long prev2 = 1, prev1 = 0;

            for (int i = 2; i <= n; i++)
            {
                long current = (i - 1) * (prev1 + prev2);
                prev2 = prev1;
                prev1 = current;
            }

            return prev1;
        }

        /// <summary>
        /// 计算斐波那契数
        /// </summary>
        /// <param name="n">索引</param>
        /// <returns>第 n 个斐波那契数</returns>
        public static long Fibonacci(int n)
        {
            if (n < 0)
                throw new ArgumentException("n 必须为非负整数");

            if (n <= 1)
                return n;

            long prev2 = 0, prev1 = 1;

            for (int i = 2; i <= n; i++)
            {
                long current = prev1 + prev2;
                prev2 = prev1;
                prev1 = current;
            }

            return prev1;
        }

        /// <summary>
        /// 生成斐波那契数列
        /// </summary>
        /// <param name="count">数量</param>
        /// <returns>斐波那契数列</returns>
        public static List<long> FibonacciSequence(int count)
        {
            var sequence = new List<long>();

            for (int i = 0; i < count; i++)
            {
                sequence.Add(Fibonacci(i));
            }

            return sequence;
        }

        #endregion

        #region 私有方法

        private static void Permute<T>(List<T> list, int start, List<List<T>> result)
        {
            if (start == list.Count - 1)
            {
                result.Add(new List<T>(list));
                return;
            }

            for (int i = start; i < list.Count; i++)
            {
                Swap(list, start, i);
                Permute(list, start + 1, result);
                Swap(list, start, i);
            }
        }

        private static void GeneratePermutations<T>(List<T> list, int length, List<T> current, bool[] used, List<List<T>> result)
        {
            if (current.Count == length)
            {
                result.Add(new List<T>(current));
                return;
            }

            for (int i = 0; i < list.Count; i++)
            {
                if (used[i])
                    continue;

                used[i] = true;
                current.Add(list[i]);

                GeneratePermutations(list, length, current, used, result);

                current.RemoveAt(current.Count - 1);
                used[i] = false;
            }
        }

        private static void GenerateCombinations<T>(List<T> list, int length, int start, List<T> current, List<List<T>> result)
        {
            if (current.Count == length)
            {
                result.Add(new List<T>(current));
                return;
            }

            for (int i = start; i < list.Count; i++)
            {
                current.Add(list[i]);
                GenerateCombinations(list, length, i + 1, current, result);
                current.RemoveAt(current.Count - 1);
            }
        }

        private static void Swap<T>(List<T> list, int i, int j)
        {
            var temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }

        private static void Reverse<T>(List<T> list, int start, int end)
        {
            while (start < end)
            {
                Swap(list, start, end);
                start++;
                end--;
            }
        }

        #endregion
    }
}
