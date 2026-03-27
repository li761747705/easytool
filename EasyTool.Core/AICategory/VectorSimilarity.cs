using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyTool.AICategory
{
    /// <summary>
    /// 向量相似度计算工具
    /// 用于计算嵌入向量之间的相似度
    /// </summary>
    public static class VectorSimilarity
    {
        /// <summary>
        /// 计算余弦相似度
        /// </summary>
        /// <param name="vector1">向量1</param>
        /// <param name="vector2">向量2</param>
        /// <returns>相似度（-1到1）</returns>
        public static double CosineSimilarity(float[] vector1, float[] vector2)
        {
            if (vector1.Length != vector2.Length)
                throw new ArgumentException("向量长度必须相同");

            double dotProduct = 0;
            double magnitude1 = 0;
            double magnitude2 = 0;

            for (int i = 0; i < vector1.Length; i++)
            {
                dotProduct += vector1[i] * vector2[i];
                magnitude1 += vector1[i] * vector1[i];
                magnitude2 += vector2[i] * vector2[i];
            }

            magnitude1 = Math.Sqrt(magnitude1);
            magnitude2 = Math.Sqrt(magnitude2);

            if (magnitude1 == 0 || magnitude2 == 0)
                return 0;

            return dotProduct / (magnitude1 * magnitude2);
        }

        /// <summary>
        /// 计算欧几里得距离
        /// </summary>
        /// <param name="vector1">向量1</param>
        /// <param name="vector2">向量2</param>
        /// <returns>距离</returns>
        public static double EuclideanDistance(float[] vector1, float[] vector2)
        {
            if (vector1.Length != vector2.Length)
                throw new ArgumentException("向量长度必须相同");

            double sum = 0;
            for (int i = 0; i < vector1.Length; i++)
            {
                var diff = vector1[i] - vector2[i];
                sum += diff * diff;
            }

            return Math.Sqrt(sum);
        }

        /// <summary>
        /// 计算点积
        /// </summary>
        /// <param name="vector1">向量1</param>
        /// <param name="vector2">向量2</param>
        /// <returns>点积</returns>
        public static double DotProduct(float[] vector1, float[] vector2)
        {
            if (vector1.Length != vector2.Length)
                throw new ArgumentException("向量长度必须相同");

            double sum = 0;
            for (int i = 0; i < vector1.Length; i++)
            {
                sum += vector1[i] * vector2[i];
            }

            return sum;
        }

        /// <summary>
        /// 归一化向量
        /// </summary>
        /// <param name="vector">向量</param>
        /// <returns>归一化后的向量</returns>
        public static float[] Normalize(float[] vector)
        {
            double magnitude = 0;
            for (int i = 0; i < vector.Length; i++)
            {
                magnitude += vector[i] * vector[i];
            }

            magnitude = Math.Sqrt(magnitude);
            if (magnitude == 0)
                return new float[vector.Length];

            var result = new float[vector.Length];
            for (int i = 0; i < vector.Length; i++)
            {
                result[i] = (float)(vector[i] / magnitude);
            }

            return result;
        }

        /// <summary>
        /// 查找最相似的向量
        /// </summary>
        /// <param name="query">查询向量</param>
        /// <param name="candidates">候选向量列表</param>
        /// <param name="topK">返回数量</param>
        /// <returns>最相似向量的索引和相似度</returns>
        public static List<(int Index, double Similarity)> FindMostSimilar(float[] query, List<float[]> candidates, int topK = 5)
        {
            var similarities = new List<(int Index, double Similarity)>();

            for (int i = 0; i < candidates.Count; i++)
            {
                var similarity = CosineSimilarity(query, candidates[i]);
                similarities.Add((i, similarity));
            }

            return similarities
                .OrderByDescending(x => x.Similarity)
                .Take(topK)
                .ToList();
        }
    }

    /// <summary>
    /// 简单的向量存储
    /// 用于存储和检索嵌入向量
    /// </summary>
    public class VectorStore
    {
        private readonly List<VectorItem> _items = new();

        /// <summary>
        /// 添加向量
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="vector">向量</param>
        /// <param name="metadata">元数据</param>
        public void Add(string id, float[] vector, Dictionary<string, object>? metadata = null)
        {
            _items.Add(new VectorItem
            {
                Id = id,
                Vector = vector,
                Metadata = metadata ?? new Dictionary<string, object>()
            });
        }

        /// <summary>
        /// 批量添加向量
        /// </summary>
        public void AddRange(IEnumerable<(string Id, float[] Vector, Dictionary<string, object>? Metadata)> items)
        {
            foreach (var item in items)
            {
                Add(item.Id, item.Vector, item.Metadata);
            }
        }

        /// <summary>
        /// 搜索相似向量
        /// </summary>
        /// <param name="query">查询向量</param>
        /// <param name="topK">返回数量</param>
        /// <param name="minScore">最小相似度</param>
        /// <returns>搜索结果</returns>
        public List<VectorSearchResult> Search(float[] query, int topK = 5, double minScore = 0)
        {
            var results = new List<VectorSearchResult>();

            foreach (var item in _items)
            {
                var score = VectorSimilarity.CosineSimilarity(query, item.Vector);
                if (score >= minScore)
                {
                    results.Add(new VectorSearchResult
                    {
                        Id = item.Id,
                        Score = score,
                        Metadata = item.Metadata
                    });
                }
            }

            return results
                .OrderByDescending(x => x.Score)
                .Take(topK)
                .ToList();
        }

        /// <summary>
        /// 删除向量
        /// </summary>
        /// <param name="id">ID</param>
        /// <returns>是否删除成功</returns>
        public bool Remove(string id)
        {
            var item = _items.FirstOrDefault(x => x.Id == id);
            if (item != null)
            {
                _items.Remove(item);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 清空所有向量
        /// </summary>
        public void Clear()
        {
            _items.Clear();
        }

        /// <summary>
        /// 获取向量数量
        /// </summary>
        public int Count => _items.Count;

        /// <summary>
        /// 获取所有 ID
        /// </summary>
        public IEnumerable<string> GetAllIds() => _items.Select(x => x.Id);
    }

    /// <summary>
    /// 向量项
    /// </summary>
    internal class VectorItem
    {
        public string Id { get; set; } = string.Empty;
        public float[] Vector { get; set; } = Array.Empty<float>();
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    /// <summary>
    /// 向量搜索结果
    /// </summary>
    public class VectorSearchResult
    {
        /// <summary>
        /// ID
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// 相似度分数
        /// </summary>
        public double Score { get; set; }

        /// <summary>
        /// 元数据
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new();
    }
}
