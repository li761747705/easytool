using System;
using System.Collections.Generic;

namespace EasyTool.CollectionsCategory
{
    /// <summary>
    /// 批量处理工具类
    /// </summary>
    public static class BatchUtil
    {
        /// <summary>
        /// 将集合分批
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="source">源集合</param>
        /// <param name="batchSize">批次大小</param>
        /// <returns>分批后的集合</returns>
        public static IEnumerable<List<T>> Batch<T>(IEnumerable<T> source, int batchSize)
        {
            if (batchSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(batchSize), "批次大小必须大于0");

            var batch = new List<T>(batchSize);
            foreach (var item in source)
            {
                batch.Add(item);
                if (batch.Count >= batchSize)
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

        /// <summary>
        /// 批量处理集合
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="source">源集合</param>
        /// <param name="batchSize">批次大小</param>
        /// <param name="action">处理动作</param>
        public static void ProcessBatch<T>(IEnumerable<T> source, int batchSize, Action<List<T>> action)
        {
            foreach (var batch in Batch(source, batchSize))
            {
                action(batch);
            }
        }

        /// <summary>
        /// 异步批量处理集合
        /// </summary>
        public static async System.Threading.Tasks.Task ProcessBatchAsync<T>(
            IEnumerable<T> source,
            int batchSize,
            Func<List<T>, System.Threading.Tasks.Task> action)
        {
            foreach (var batch in Batch(source, batchSize))
            {
                await action(batch).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// 批量处理并返回结果
        /// </summary>
        public static IEnumerable<TResult> ProcessBatch<T, TResult>(
            IEnumerable<T> source,
            int batchSize,
            Func<List<T>, IEnumerable<TResult>> action)
        {
            foreach (var batch in Batch(source, batchSize))
            {
                foreach (var result in action(batch))
                {
                    yield return result;
                }
            }
        }

        /// <summary>
        /// 异步批量处理并返回结果
        /// </summary>
        public static async IAsyncEnumerable<TResult> ProcessBatchAsync<T, TResult>(
            IEnumerable<T> source,
            int batchSize,
            Func<List<T>, System.Threading.Tasks.Task<IEnumerable<TResult>>> action)
        {
            foreach (var batch in Batch(source, batchSize))
            {
                var results = await action(batch).ConfigureAwait(false);
                foreach (var result in results)
                {
                    yield return result;
                }
            }
        }

        /// <summary>
        /// 并行批量处理
        /// </summary>
        public static void ProcessBatchParallel<T>(
            IEnumerable<T> source,
            int batchSize,
            Action<List<T>> action,
            int maxDegreeOfParallelism = 4)
        {
            var options = new System.Threading.Tasks.ParallelOptions
            {
                MaxDegreeOfParallelism = maxDegreeOfParallelism
            };

            System.Threading.Tasks.Parallel.ForEach(Batch(source, batchSize), options, action);
        }

        /// <summary>
        /// 并行批量处理并返回结果
        /// </summary>
        public static List<TResult> ProcessBatchParallel<T, TResult>(
            IEnumerable<T> source,
            int batchSize,
            Func<List<T>, IEnumerable<TResult>> action,
            int maxDegreeOfParallelism = 4)
        {
            var results = new System.Collections.Concurrent.ConcurrentBag<TResult>();
            var options = new System.Threading.Tasks.ParallelOptions
            {
                MaxDegreeOfParallelism = maxDegreeOfParallelism
            };

            System.Threading.Tasks.Parallel.ForEach(Batch(source, batchSize), options, batch =>
            {
                foreach (var result in action(batch))
                {
                    results.Add(result);
                }
            });

            return new List<TResult>(results);
        }
    }
}
