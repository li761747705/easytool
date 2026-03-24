using System;
using System.Collections.Generic;

namespace EasyTool.CollectionsCategory
{
    /// <summary>
    /// 并查集工具类
    /// 用于高效处理元素分组和连通性问题
    /// 支持 union-find 操作，近乎常数时间复杂度
    /// </summary>
    public static class UnionFindUtil
    {
        /// <summary>
        /// 创建并查集
        /// </summary>
        /// <param name="size">元素数量</param>
        /// <returns>并查集实例</returns>
        public static UnionFind Create(int size)
        {
            return new UnionFind(size);
        }

        /// <summary>
        /// 从元素集合创建并查集
        /// </summary>
        public static UnionFind<T> Create<T>(IEnumerable<T> elements)
        {
            return new UnionFind<T>(elements);
        }
    }

    /// <summary>
    /// 整数并查集实现
    /// </summary>
    public class UnionFind
    {
        private readonly int[] _parent;
        private readonly int[] _rank;
        private int _count;

        /// <summary>
        /// 元素数量
        /// </summary>
        public int Count => _count;

        /// <summary>
        /// 创建并查集
        /// </summary>
        /// <param name="size">元素数量</param>
        public UnionFind(int size)
        {
            if (size <= 0)
                throw new ArgumentOutOfRangeException(nameof(size));

            _parent = new int[size];
            _rank = new int[size];
            _count = size;

            for (int i = 0; i < size; i++)
            {
                _parent[i] = i;
                _rank[i] = 1;
            }
        }

        /// <summary>
        /// 查找元素所属的集合（带路径压缩）
        /// </summary>
        public int Find(int x)
        {
            if (x < 0 || x >= _parent.Length)
                throw new ArgumentOutOfRangeException(nameof(x));

            if (_parent[x] != x)
            {
                _parent[x] = Find(_parent[x]); // 路径压缩
            }
            return _parent[x];
        }

        /// <summary>
        /// 合并两个元素所属的集合
        /// </summary>
        public void Union(int x, int y)
        {
            int rootX = Find(x);
            int rootY = Find(y);

            if (rootX == rootY)
                return;

            // 按秩合并
            if (_rank[rootX] < _rank[rootY])
            {
                _parent[rootX] = rootY;
            }
            else if (_rank[rootX] > _rank[rootY])
            {
                _parent[rootY] = rootX;
            }
            else
            {
                _parent[rootY] = rootX;
                _rank[rootX]++;
            }

            _count--;
        }

        /// <summary>
        /// 判断两个元素是否属于同一集合
        /// </summary>
        public bool Connected(int x, int y)
        {
            return Find(x) == Find(y);
        }

        /// <summary>
        /// 获取元素所在集合的大小
        /// </summary>
        public int GetSetSize(int x)
        {
            int root = Find(x);
            int size = 0;
            for (int i = 0; i < _parent.Length; i++)
            {
                if (Find(i) == root)
                    size++;
            }
            return size;
        }

        /// <summary>
        /// 获取所有集合
        /// </summary>
        public Dictionary<int, List<int>> GetAllSets()
        {
            var sets = new Dictionary<int, List<int>>();

            for (int i = 0; i < _parent.Length; i++)
            {
                int root = Find(i);
                if (!sets.ContainsKey(root))
                {
                    sets[root] = new List<int>();
                }
                sets[root].Add(i);
            }

            return sets;
        }
    }

    /// <summary>
    /// 泛型并查集实现
    /// </summary>
    /// <typeparam name="T">元素类型</typeparam>
    public class UnionFind<T>
    {
        private readonly Dictionary<T, T> _parent;
        private readonly Dictionary<T, int> _rank;
        private int _count;

        /// <summary>
        /// 集合数量
        /// </summary>
        public int Count => _count;

        /// <summary>
        /// 创建并查集
        /// </summary>
        public UnionFind(IEnumerable<T> elements)
        {
            if (elements == null)
                throw new ArgumentNullException(nameof(elements));

            _parent = new Dictionary<T, T>();
            _rank = new Dictionary<T, int>();

            foreach (var element in elements)
            {
                _parent[element] = element;
                _rank[element] = 1;
            }

            _count = _parent.Count;
        }

        /// <summary>
        /// 添加元素
        /// </summary>
        public void Add(T element)
        {
            if (!_parent.ContainsKey(element))
            {
                _parent[element] = element;
                _rank[element] = 1;
                _count++;
            }
        }

        /// <summary>
        /// 查找元素所属的集合
        /// </summary>
        public T Find(T x)
        {
            if (!_parent.ContainsKey(x))
                throw new KeyNotFoundException($"Element '{x}' not found");

            if (!EqualityComparer<T>.Default.Equals(_parent[x], x))
            {
                _parent[x] = Find(_parent[x]);
            }
            return _parent[x];
        }

        /// <summary>
        /// 合并两个元素所属的集合
        /// </summary>
        public void Union(T x, T y)
        {
            T rootX = Find(x);
            T rootY = Find(y);

            if (EqualityComparer<T>.Default.Equals(rootX, rootY))
                return;

            if (_rank[rootX] < _rank[rootY])
            {
                _parent[rootX] = rootY;
            }
            else if (_rank[rootX] > _rank[rootY])
            {
                _parent[rootY] = rootX;
            }
            else
            {
                _parent[rootY] = rootX;
                _rank[rootX]++;
            }

            _count--;
        }

        /// <summary>
        /// 判断两个元素是否属于同一集合
        /// </summary>
        public bool Connected(T x, T y)
        {
            return EqualityComparer<T>.Default.Equals(Find(x), Find(y));
        }

        /// <summary>
        /// 获取所有集合
        /// </summary>
        public Dictionary<T, List<T>> GetAllSets()
        {
            var sets = new Dictionary<T, List<T>>();

            foreach (var element in _parent.Keys)
            {
                T root = Find(element);
                if (!sets.ContainsKey(root))
                {
                    sets[root] = new List<T>();
                }
                sets[root].Add(element);
            }

            return sets;
        }
    }
}
