using System;
using System.Collections.Generic;

namespace EasyTool.CollectionsCategory
{
    /// <summary>
    /// 排列组合工具类
    /// 提供排列和组合的生成功能
    /// </summary>
    public static class PermutationUtil
    {
        /// <summary>
        /// 生成所有全排列
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="elements">元素集合</param>
        /// <returns>所有排列</returns>
        public static IEnumerable<IEnumerable<T>> Permutations<T>(IEnumerable<T> elements)
        {
            var list = new List<T>(elements);
            return PermutationsCore(list, 0, list.Count);
        }

        /// <summary>
        /// 生成指定长度的排列
        /// </summary>
        public static IEnumerable<IEnumerable<T>> Permutations<T>(IEnumerable<T> elements, int length)
        {
            if (length <= 0)
                throw new ArgumentOutOfRangeException(nameof(length));

            var list = new List<T>(elements);
            return PermutationsCore(list, 0, length);
        }

        /// <summary>
        /// 生成可重复排列（每个位置可以选择任意元素）
        /// </summary>
        public static IEnumerable<IEnumerable<T>> PermutationsWithRepetition<T>(IEnumerable<T> elements, int length)
        {
            if (length <= 0)
                throw new ArgumentOutOfRangeException(nameof(length));

            var list = new List<T>(elements);
            if (list.Count == 0)
                yield break;

            var indices = new int[length];
            var current = new T[length];

            while (true)
            {
                for (int i = 0; i < length; i++)
                {
                    current[i] = list[indices[i]];
                }
                yield return new List<T>(current);

                int pos = length - 1;
                while (pos >= 0)
                {
                    indices[pos]++;
                    if (indices[pos] < list.Count)
                        break;
                    indices[pos] = 0;
                    pos--;
                }

                if (pos < 0)
                    break;
            }
        }

        private static IEnumerable<IEnumerable<T>> PermutationsCore<T>(List<T> list, int start, int length)
        {
            if (start == length)
            {
                yield return new List<T>(list.GetRange(0, length));
                yield break;
            }

            for (int i = start; i < list.Count; i++)
            {
                Swap(list, start, i);
                foreach (var perm in PermutationsCore(list, start + 1, length))
                {
                    yield return perm;
                }
                Swap(list, start, i);
            }
        }

        /// <summary>
        /// 计算排列数 A(n,r) = n! / (n-r)!
        /// </summary>
        public static long Count(int n, int r)
        {
            if (n < 0 || r < 0 || r > n)
                throw new ArgumentException("Invalid parameters");
            if (r == 0) return 1;

            long result = 1;
            for (int i = 0; i < r; i++)
            {
                result *= (n - i);
            }
            return result;
        }

        private static void Swap<T>(List<T> list, int i, int j)
        {
            if (i != j)
            {
                T temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }
    }

    /// <summary>
    /// 组合工具类
    /// </summary>
    public static class CombinationUtil
    {
        /// <summary>
        /// 生成所有组合
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="elements">元素集合</param>
        /// <param name="length">组合长度</param>
        /// <returns>所有组合</returns>
        public static IEnumerable<IEnumerable<T>> Combinations<T>(IEnumerable<T> elements, int length)
        {
            if (length <= 0)
                throw new ArgumentOutOfRangeException(nameof(length));

            var list = new List<T>(elements);
            if (list.Count < length)
                yield break;

            var indices = new int[length];
            for (int i = 0; i < length; i++)
            {
                indices[i] = i;
            }

            while (true)
            {
                yield return GetItems(list, indices);

                int pos = length - 1;
                while (pos >= 0 && indices[pos] == list.Count - length + pos)
                {
                    pos--;
                }

                if (pos < 0)
                    break;

                indices[pos]++;
                for (int i = pos + 1; i < length; i++)
                {
                    indices[i] = indices[i - 1] + 1;
                }
            }
        }

        /// <summary>
        /// 生成所有长度的组合（从1到n）
        /// </summary>
        public static IEnumerable<IEnumerable<T>> AllCombinations<T>(IEnumerable<T> elements)
        {
            var list = new List<T>(elements);
            for (int length = 1; length <= list.Count; length++)
            {
                foreach (var combo in Combinations(list, length))
                {
                    yield return combo;
                }
            }
        }

        /// <summary>
        /// 生成可重复组合
        /// </summary>
        public static IEnumerable<IEnumerable<T>> CombinationsWithRepetition<T>(IEnumerable<T> elements, int length)
        {
            if (length <= 0)
                throw new ArgumentOutOfRangeException(nameof(length));

            var list = new List<T>(elements);
            if (list.Count == 0)
                yield break;

            var indices = new int[length];

            while (true)
            {
                yield return GetItems(list, indices);

                int pos = length - 1;
                while (pos >= 0 && indices[pos] == list.Count - 1)
                {
                    pos--;
                }

                if (pos < 0)
                    break;

                indices[pos]++;
                for (int i = pos + 1; i < length; i++)
                {
                    indices[i] = indices[pos];
                }
            }
        }

        /// <summary>
        /// 计算组合数 C(n,r) = n! / (r! * (n-r)!)
        /// </summary>
        public static long Count(int n, int r)
        {
            if (n < 0 || r < 0 || r > n)
                throw new ArgumentException("Invalid parameters");
            if (r == 0 || r == n) return 1;

            // 优化：使用较小的 r 计算
            if (r > n - r)
                r = n - r;

            long result = 1;
            for (int i = 0; i < r; i++)
            {
                result = result * (n - i) / (i + 1);
            }
            return result;
        }

        private static IEnumerable<T> GetItems<T>(IList<T> list, int[] indices)
        {
            var result = new T[indices.Length];
            for (int i = 0; i < indices.Length; i++)
            {
                result[i] = list[indices[i]];
            }
            return result;
        }
    }
}
