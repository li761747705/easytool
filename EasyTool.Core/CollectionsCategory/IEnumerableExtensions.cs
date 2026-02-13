using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace EasyTool.IEnumerableCategory
{
    /// <summary>
    /// IEnumerable 通用扩展方法
    /// </summary>
    public static class IEnumerableExtensions
    {
        #region 空值处理

        /// <summary>
        /// 对 List 等集合 Foreach 的时候不用在上层判空，直接加上这个就好
        /// </summary>
        public static IEnumerable<T> CheckNull<T>(this IEnumerable<T> values)
        {
            return values is null ? new List<T>(0) : values;
        }

        /// <summary>
        /// 判断集合是否为空或 null
        /// </summary>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> source)
        {
            return source == null || !source.Any();
        }

        /// <summary>
        /// 判断集合是否非空
        /// </summary>
        public static bool IsNotEmpty<T>(this IEnumerable<T> source)
        {
            return source != null && source.Any();
        }

        #endregion

        #region 遍历操作

        /// <summary>
        /// 遍历集合并对每个元素执行指定操作
        /// </summary>
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            if (source == null || action == null)
                return;

            foreach (var item in source)
            {
                action(item);
            }
        }

        /// <summary>
        /// 遍历集合并对每个元素及其索引执行指定操作
        /// </summary>
        public static void ForEach<T>(this IEnumerable<T> source, Action<T, int> action)
        {
            if (source == null || action == null)
                return;

            int index = 0;
            foreach (var item in source)
            {
                action(item, index++);
            }
        }

        #endregion

        #region 集合运算

        /// <summary>
        /// 求集合的笛卡尔积
        /// </summary>
        public static IEnumerable<IEnumerable<T>> Cartesian<T>(this IEnumerable<IEnumerable<T>> sequences)
        {
            IEnumerable<IEnumerable<T>> tempProduct = new[] { Enumerable.Empty<T>() };
            return sequences.Aggregate(tempProduct,
                                          (accumulator, sequence) =>
                                             from accseq in accumulator
                                             from item in sequence
                                             select accseq.Concat(new[] { item
                                              }));
        }

        /// <summary>
        /// 按指定键去重
        /// </summary>
        public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector)
        {
            if (source == null)
                yield break;

            var seenKeys = new HashSet<TKey>();
            foreach (var item in source)
            {
                if (seenKeys.Add(keySelector(item)))
                {
                    yield return item;
                }
            }
        }

        /// <summary>
        /// 批量处理集合
        /// </summary>
        /// <param name="batchSize">每批的大小</param>
        public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> source, int batchSize)
        {
            if (source == null)
                yield break;

            if (batchSize <= 0)
                throw new ArgumentException("batchSize must be greater than 0", nameof(batchSize));

            var batch = new List<T>(batchSize);
            foreach (var item in source)
            {
                batch.Add(item);
                if (batch.Count == batchSize)
                {
                    yield return batch;
                    batch = new List<T>(batchSize);
                }
            }

            if (batch.Count > 0)
            {
                yield return batch;
            }
        }

        #endregion

        #region 转换操作

        /// <summary>
        /// 将集合转换为 DataTable
        /// </summary>
        public static DataTable ToDataTable<T>(this IEnumerable<T> source)
        {
            var table = new DataTable(typeof(T).Name);

            var properties = typeof(T).GetProperties();
            foreach (var prop in properties)
            {
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            }

            foreach (var item in source)
            {
                var row = table.NewRow();
                foreach (var prop in properties)
                {
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                }
                table.Rows.Add(row);
            }

            return table;
        }

        /// <summary>
        /// 将集合转换为 HashSet
        /// </summary>
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source)
        {
            if (source == null)
                return new HashSet<T>();

            return new HashSet<T>(source);
        }

        /// <summary>
        /// 将集合转换为 Queue
        /// </summary>
        public static Queue<T> ToQueue<T>(this IEnumerable<T> source)
        {
            if (source == null)
                return new Queue<T>();

            return new Queue<T>(source);
        }

        /// <summary>
        /// 将集合转换为 Stack
        /// </summary>
        public static Stack<T> ToStack<T>(this IEnumerable<T> source)
        {
            if (source == null)
                return new Stack<T>();

            return new Stack<T>(source);
        }

        /// <summary>
        /// 将集合转换为 LinkedList
        /// </summary>
        public static LinkedList<T> ToLinkedList<T>(this IEnumerable<T> source)
        {
            if (source == null)
                return new LinkedList<T>();

            return new LinkedList<T>(source);
        }

        #endregion

        #region 连接操作

        /// <summary>
        /// 将集合元素连接成字符串
        /// </summary>
        /// <param name="separator">分隔符</param>
        public static string JoinAsString<T>(this IEnumerable<T> source, string separator = ",")
        {
            if (source == null)
                return string.Empty;

            return string.Join(separator, source);
        }

        /// <summary>
        /// 将集合元素连接成字符串（使用格式化）
        /// </summary>
        /// <param name="separator">分隔符</param>
        /// <param name="format">格式化字符串</param>
        public static string JoinAsString<T>(this IEnumerable<T> source, string separator, string format)
        {
            if (source == null)
                return string.Empty;

            return string.Join(separator, source.Select(item => string.Format(format, item)));
        }

        #endregion

        #region 分页操作

        /// <summary>
        /// 分页
        /// </summary>
        /// <param name="pageIndex">页码（从1开始）</param>
        /// <param name="pageSize">每页大小</param>
        public static IEnumerable<T> Page<T>(this IEnumerable<T> source, int pageIndex, int pageSize)
        {
            if (source == null || pageIndex < 1 || pageSize < 1)
                yield break;

            int skip = (pageIndex - 1) * pageSize;
            foreach (var item in source.Skip(skip).Take(pageSize))
            {
                yield return item;
            }
        }

        #endregion

        #region 随机操作

        /// <summary>
        /// 随机排序
        /// </summary>
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            if (source == null)
                yield break;

            var random = new Random();
            var list = source.ToList();

            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }

            foreach (var item in list)
            {
                yield return item;
            }
        }

        /// <summary>
        /// 随机获取一个元素
        /// </summary>
        public static T Random<T>(this IEnumerable<T> source)
        {
            if (source == null)
                return default;

            var list = source.ToList();
            if (list.Count == 0)
                return default;

            var random = new Random();
            return list[random.Next(list.Count)];
        }

        /// <summary>
        /// 随机获取指定数量的元素
        /// </summary>
        public static IEnumerable<T> RandomTake<T>(this IEnumerable<T> source, int count)
        {
            if (source == null || count <= 0)
                yield break;

            var list = source.ToList();
            if (list.Count == 0)
                yield break;

            count = Math.Min(count, list.Count);
            var random = new Random();
            var selected = new HashSet<int>();

            while (selected.Count < count)
            {
                selected.Add(random.Next(list.Count));
            }

            foreach (var index in selected)
            {
                yield return list[index];
            }
        }

        #endregion

        #region 条件操作

        /// <summary>
        /// 根据条件执行不同的操作
        /// </summary>
        public static IEnumerable<T> WhereIf<T>(this IEnumerable<T> source, bool condition, Func<T, bool> predicate)
        {
            if (source == null)
                yield break;

            if (condition)
            {
                foreach (var item in source.Where(predicate))
                {
                    yield return item;
                }
            }
            else
            {
                foreach (var item in source)
                {
                    yield return item;
                }
            }
        }

        /// <summary>
        /// 如果集合为空则返回默认集合
        /// </summary>
        public static IEnumerable<T> IfEmpty<T>(this IEnumerable<T> source, IEnumerable<T> defaultValue)
        {
            if (source == null || !source.Any())
                return defaultValue ?? Enumerable.Empty<T>();

            return source;
        }

        #endregion

        #region 统计操作

        /// <summary>
        /// 统计满足条件的元素数量
        /// </summary>
        public static int CountEx<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            if (source == null)
                return 0;

            return source.Count(predicate);
        }

        #endregion

        #region 索引操作

        /// <summary>
        /// 获取指定索引处的元素
        /// </summary>
        public static T ElementAtOrDefault<T>(this IEnumerable<T> source, int index, T defaultValue)
        {
            if (source == null || index < 0)
                return defaultValue;

            int i = 0;
            foreach (var item in source)
            {
                if (i == index)
                    return item;
                i++;
            }

            return defaultValue;
        }

        /// <summary>
        /// 获取第一个元素，如果集合为空则返回默认值
        /// </summary>
        public static T FirstOrValue<T>(this IEnumerable<T> source, T defaultValue)
        {
            if (source == null)
                return defaultValue;

            foreach (var item in source)
            {
                return item;
            }

            return defaultValue;
        }

        /// <summary>
        /// 获取最后一个元素，如果集合为空则返回默认值
        /// </summary>
        public static T LastOrValue<T>(this IEnumerable<T> source, T defaultValue)
        {
            if (source == null)
                return defaultValue;

            var last = defaultValue;
            var hasElement = false;

            foreach (var item in source)
            {
                last = item;
                hasElement = true;
            }

            return hasElement ? last : defaultValue;
        }

        #endregion

        #region 集合合并

        /// <summary>
        /// 合并多个集合
        /// </summary>
        public static IEnumerable<T> Merge<T>(params IEnumerable<T>[] sources)
        {
            if (sources == null || sources.Length == 0)
                yield break;

            foreach (var source in sources)
            {
                if (source != null)
                {
                    foreach (var item in source)
                    {
                        yield return item;
                    }
                }
            }
        }

        #endregion
    }
}
