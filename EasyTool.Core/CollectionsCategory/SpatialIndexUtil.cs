using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyTool.CollectionsCategory
{
    /// <summary>
    /// 空间索引工具类
    /// </summary>
    public static class SpatialIndexUtil
    {
        /// <summary>
        /// 创建KD树（2维）
        /// </summary>
        public static KDTree<T> CreateKDTree<T>()
        {
            return new KDTree<T>(2);
        }

        /// <summary>
        /// 创建KD树（指定维度）
        /// </summary>
        public static KDTree<T> CreateKDTree<T>(int dimensions)
        {
            return new KDTree<T>(dimensions);
        }

        /// <summary>
        /// 创建四叉树
        /// </summary>
        public static QuadTree<T> CreateQuadTree<T>(double minX, double minY, double maxX, double maxY)
        {
            return new QuadTree<T>(minX, minY, maxX, maxY);
        }

        /// <summary>
        /// 创建网格索引
        /// </summary>
        public static GridIndex<T> CreateGridIndex<T>(double minX, double minY, double maxX, double maxY, int cellCountX, int cellCountY)
        {
            return new GridIndex<T>(minX, minY, maxX, maxY, cellCountX, cellCountY);
        }
    }

    /// <summary>
    /// KD树（K维树）
    /// 用于高维空间中的最近邻搜索
    /// </summary>
    public class KDTree<T>
    {
        private class KDNode
        {
            public double[] Point { get; set; }
            public T Value { get; set; }
            public KDNode Left { get; set; }
            public KDNode Right { get; set; }
        }

        private KDNode _root;
        private readonly int _dimensions;
        private int _count;

        /// <summary>
        /// 维度
        /// </summary>
        public int Dimensions => _dimensions;

        /// <summary>
        /// 元素数量
        /// </summary>
        public int Count => _count;

        /// <summary>
        /// 是否为空
        /// </summary>
        public bool IsEmpty => _count == 0;

        /// <summary>
        /// 创建KD树
        /// </summary>
        public KDTree(int dimensions)
        {
            if (dimensions <= 0)
                throw new ArgumentOutOfRangeException(nameof(dimensions));

            _dimensions = dimensions;
            _root = null;
            _count = 0;
        }

        /// <summary>
        /// 插入点
        /// </summary>
        public void Insert(double[] point, T value)
        {
            if (point == null || point.Length != _dimensions)
                throw new ArgumentException($"Point must have {_dimensions} dimensions");

            _root = Insert(_root, point, value, 0);
            _count++;
        }

        private KDNode Insert(KDNode node, double[] point, T value, int depth)
        {
            if (node == null)
            {
                return new KDNode { Point = point, Value = value };
            }

            int axis = depth % _dimensions;

            if (point[axis] < node.Point[axis])
            {
                node.Left = Insert(node.Left, point, value, depth + 1);
            }
            else
            {
                node.Right = Insert(node.Right, point, value, depth + 1);
            }

            return node;
        }

        /// <summary>
        /// 查找最近邻
        /// </summary>
        public (double[] Point, T Value)? FindNearest(double[] target)
        {
            if (target == null || target.Length != _dimensions)
                throw new ArgumentException($"Target must have {_dimensions} dimensions");

            if (_root == null)
                return null;

            KDNode best = null;
            double bestDist = double.MaxValue;

            FindNearest(_root, target, 0, ref best, ref bestDist);

            return best == null ? null : (best.Point, best.Value);
        }

        private void FindNearest(KDNode node, double[] target, int depth, ref KDNode best, ref double bestDist)
        {
            if (node == null)
                return;

            double dist = Distance(node.Point, target);
            if (dist < bestDist)
            {
                bestDist = dist;
                best = node;
            }

            int axis = depth % _dimensions;
            double diff = target[axis] - node.Point[axis];

            KDNode near = diff < 0 ? node.Left : node.Right;
            KDNode far = diff < 0 ? node.Right : node.Left;

            FindNearest(near, target, depth + 1, ref best, ref bestDist);

            // 检查是否需要搜索另一侧
            if (diff * diff < bestDist)
            {
                FindNearest(far, target, depth + 1, ref best, ref bestDist);
            }
        }

        /// <summary>
        /// 查找K个最近邻
        /// </summary>
        public List<(double[] Point, T Value, double Distance)> FindKNearest(double[] target, int k)
        {
            if (target == null || target.Length != _dimensions)
                throw new ArgumentException($"Target must have {_dimensions} dimensions");

            var result = new List<(double[] Point, T Value, double Distance)>();

            if (_root == null)
                return result;

            var heap = new List<(double Dist, KDNode Node)>();

            FindKNearest(_root, target, 0, heap, k);

            foreach (var (dist, node) in heap)
            {
                result.Add((node.Point, node.Value, dist));
            }

            return result.OrderBy(x => x.Distance).ToList();
        }

        private void FindKNearest(KDNode node, double[] target, int depth, List<(double Dist, KDNode Node)> heap, int k)
        {
            if (node == null)
                return;

            double dist = Distance(node.Point, target);

            if (heap.Count < k)
            {
                heap.Add((dist, node));
                heap.Sort((a, b) => b.Dist.CompareTo(a.Dist));
            }
            else if (dist < heap[0].Dist)
            {
                heap[0] = (dist, node);
                heap.Sort((a, b) => b.Dist.CompareTo(a.Dist));
            }

            int axis = depth % _dimensions;
            double diff = target[axis] - node.Point[axis];

            KDNode near = diff < 0 ? node.Left : node.Right;
            KDNode far = diff < 0 ? node.Right : node.Left;

            FindKNearest(near, target, depth + 1, heap, k);

            double maxDist = heap.Count < k ? double.MaxValue : heap[0].Dist;
            if (diff * diff < maxDist)
            {
                FindKNearest(far, target, depth + 1, heap, k);
            }
        }

        /// <summary>
        /// 范围查询
        /// </summary>
        public List<(double[] Point, T Value)> RangeQuery(double[] min, double[] max)
        {
            var result = new List<(double[] Point, T Value)>();
            RangeQuery(_root, min, max, 0, result);
            return result;
        }

        private void RangeQuery(KDNode node, double[] min, double[] max, int depth, List<(double[] Point, T Value)> result)
        {
            if (node == null)
                return;

            bool inside = true;
            for (int i = 0; i < _dimensions; i++)
            {
                if (node.Point[i] < min[i] || node.Point[i] > max[i])
                {
                    inside = false;
                    break;
                }
            }

            if (inside)
            {
                result.Add((node.Point, node.Value));
            }

            int axis = depth % _dimensions;

            if (min[axis] <= node.Point[axis])
            {
                RangeQuery(node.Left, min, max, depth + 1, result);
            }
            if (max[axis] >= node.Point[axis])
            {
                RangeQuery(node.Right, min, max, depth + 1, result);
            }
        }

        private double Distance(double[] a, double[] b)
        {
            double sum = 0;
            for (int i = 0; i < _dimensions; i++)
            {
                double diff = a[i] - b[i];
                sum += diff * diff;
            }
            return Math.Sqrt(sum);
        }

        /// <summary>
        /// 清空
        /// </summary>
        public void Clear()
        {
            _root = null;
            _count = 0;
        }
    }

    /// <summary>
    /// 四叉树
    /// 用于二维空间的区域查询
    /// </summary>
    public class QuadTree<T>
    {
        private class QuadNode
        {
            public double X { get; set; }
            public double Y { get; set; }
            public T Value { get; set; }

            public QuadNode(double x, double y, T value)
            {
                X = x;
                Y = y;
                Value = value;
            }
        }

        private class QuadTreeNode
        {
            public double MinX { get; set; }
            public double MinY { get; set; }
            public double MaxX { get; set; }
            public double MaxY { get; set; }

            public List<QuadNode> Points { get; set; }
            public QuadTreeNode[] Children { get; set; }

            public int Capacity { get; set; }
            public bool IsDivided { get; set; }

            public QuadTreeNode(double minX, double minY, double maxX, double maxY, int capacity = 4)
            {
                MinX = minX;
                MinY = minY;
                MaxX = maxX;
                MaxY = maxY;
                Capacity = capacity;
                Points = new List<QuadNode>();
                Children = null;
                IsDivided = false;
            }

            public double MidX => (MinX + MaxX) / 2;
            public double MidY => (MinY + MaxY) / 2;

            public bool Contains(double x, double y)
            {
                return x >= MinX && x <= MaxX && y >= MinY && y <= MaxY;
            }

            public bool Intersects(double minX, double minY, double maxX, double maxY)
            {
                return !(MaxX < minX || MinX > maxX || MaxY < minY || MinY > maxY);
            }

            public void Subdivide()
            {
                Children = new QuadTreeNode[4];
                Children[0] = new QuadTreeNode(MinX, MidY, MidX, MaxY, Capacity); // NW
                Children[1] = new QuadTreeNode(MidX, MidY, MaxX, MaxY, Capacity); // NE
                Children[2] = new QuadTreeNode(MinX, MinY, MidX, MidY, Capacity); // SW
                Children[3] = new QuadTreeNode(MidX, MinY, MaxX, MidY, Capacity); // SE
                IsDivided = true;
            }
        }

        private readonly QuadTreeNode _root;
        private readonly int _capacity;
        private int _count;

        /// <summary>
        /// 边界
        /// </summary>
        public (double MinX, double MinY, double MaxX, double MaxY) Bounds =>
            (_root.MinX, _root.MinY, _root.MaxX, _root.MaxY);

        /// <summary>
        /// 元素数量
        /// </summary>
        public int Count => _count;

        /// <summary>
        /// 是否为空
        /// </summary>
        public bool IsEmpty => _count == 0;

        /// <summary>
        /// 创建四叉树
        /// </summary>
        public QuadTree(double minX, double minY, double maxX, double maxY, int capacity = 4)
        {
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(capacity));

            _capacity = capacity;
            _root = new QuadTreeNode(minX, minY, maxX, maxY, capacity);
            _count = 0;
        }

        /// <summary>
        /// 插入点
        /// </summary>
        public bool Insert(double x, double y, T value)
        {
            if (!_root.Contains(x, y))
                return false;

            Insert(_root, x, y, value);
            _count++;
            return true;
        }

        private void Insert(QuadTreeNode node, double x, double y, T value)
        {
            if (node.IsDivided)
            {
                int index = GetChildIndex(node, x, y);
                if (index >= 0)
                {
                    Insert(node.Children[index], x, y, value);
                }
                return;
            }

            if (node.Points.Count < node.Capacity)
            {
                node.Points.Add(new QuadNode(x, y, value));
            }
            else
            {
                node.Subdivide();

                // 重新分配现有点
                foreach (var point in node.Points)
                {
                    int index = GetChildIndex(node, point.X, point.Y);
                    if (index >= 0)
                    {
                        node.Children[index].Points.Add(point);
                    }
                }
                node.Points.Clear();

                // 插入新点
                int newIndex = GetChildIndex(node, x, y);
                if (newIndex >= 0)
                {
                    node.Children[newIndex].Points.Add(new QuadNode(x, y, value));
                }
            }
        }

        private int GetChildIndex(QuadTreeNode node, double x, double y)
        {
            bool inWest = x < node.MidX;
            bool inNorth = y >= node.MidY;

            if (inWest && inNorth) return 0; // NW
            if (!inWest && inNorth) return 1; // NE
            if (inWest && !inNorth) return 2; // SW
            if (!inWest && !inNorth) return 3; // SE

            return -1;
        }

        /// <summary>
        /// 范围查询
        /// </summary>
        public List<(double X, double Y, T Value)> Query(double minX, double minY, double maxX, double maxY)
        {
            var result = new List<(double X, double Y, T Value)>();
            Query(_root, minX, minY, maxX, maxY, result);
            return result;
        }

        private void Query(QuadTreeNode node, double minX, double minY, double maxX, double maxY, List<(double X, double Y, T Value)> result)
        {
            if (!node.Intersects(minX, minY, maxX, maxY))
                return;

            if (node.IsDivided)
            {
                foreach (var child in node.Children)
                {
                    Query(child, minX, minY, maxX, maxY, result);
                }
            }
            else
            {
                foreach (var point in node.Points)
                {
                    if (point.X >= minX && point.X <= maxX && point.Y >= minY && point.Y <= maxY)
                    {
                        result.Add((point.X, point.Y, point.Value));
                    }
                }
            }
        }

        /// <summary>
        /// 查找圆内所有点
        /// </summary>
        public List<(double X, double Y, T Value)> QueryCircle(double centerX, double centerY, double radius)
        {
            var result = new List<(double X, double Y, T Value)>();
            QueryCircle(_root, centerX, centerY, radius, result);
            return result;
        }

        private void QueryCircle(QuadTreeNode node, double centerX, double centerY, double radius, List<(double X, double Y, T Value)> result)
        {
            if (!node.Intersects(centerX - radius, centerY - radius, centerX + radius, centerY + radius))
                return;

            if (node.IsDivided)
            {
                foreach (var child in node.Children)
                {
                    QueryCircle(child, centerX, centerY, radius, result);
                }
            }
            else
            {
                double radiusSquared = radius * radius;
                foreach (var point in node.Points)
                {
                    double dx = point.X - centerX;
                    double dy = point.Y - centerY;
                    if (dx * dx + dy * dy <= radiusSquared)
                    {
                        result.Add((point.X, point.Y, point.Value));
                    }
                }
            }
        }

        /// <summary>
        /// 清空
        /// </summary>
        public void Clear()
        {
            _root.Points.Clear();
            _root.Children = null;
            _root.IsDivided = false;
            _count = 0;
        }
    }

    /// <summary>
    /// 网格索引
    /// 将空间划分为网格，快速查找
    /// </summary>
    public class GridIndex<T>
    {
        private readonly Dictionary<int, Dictionary<int, List<(double X, double Y, T Value)>>> _grid;
        private readonly double _minX, _minY, _maxX, _maxY;
        private readonly double _cellWidth, _cellHeight;
        private readonly int _cellCountX, _cellCountY;
        private int _count;

        /// <summary>
        /// 元素数量
        /// </summary>
        public int Count => _count;

        /// <summary>
        /// 创建网格索引
        /// </summary>
        public GridIndex(double minX, double minY, double maxX, double maxY, int cellCountX, int cellCountY)
        {
            if (maxX <= minX || maxY <= minY)
                throw new ArgumentException("Invalid bounds");
            if (cellCountX <= 0 || cellCountY <= 0)
                throw new ArgumentOutOfRangeException("Cell counts must be positive");

            _minX = minX;
            _minY = minY;
            _maxX = maxX;
            _maxY = maxY;
            _cellCountX = cellCountX;
            _cellCountY = cellCountY;
            _cellWidth = (maxX - minX) / cellCountX;
            _cellHeight = (maxY - minY) / cellCountY;
            _grid = new Dictionary<int, Dictionary<int, List<(double X, double Y, T Value)>>>();
            _count = 0;
        }

        /// <summary>
        /// 插入点
        /// </summary>
        public bool Insert(double x, double y, T value)
        {
            if (x < _minX || x > _maxX || y < _minY || y > _maxY)
                return false;

            int cellX = GetCellX(x);
            int cellY = GetCellY(y);

            if (!_grid.TryGetValue(cellX, out var column))
            {
                column = new Dictionary<int, List<(double X, double Y, T Value)>>();
                _grid[cellX] = column;
            }

            if (!column.TryGetValue(cellY, out var cell))
            {
                cell = new List<(double X, double Y, T Value)>();
                column[cellY] = cell;
            }

            cell.Add((x, y, value));
            _count++;
            return true;
        }

        /// <summary>
        /// 范围查询
        /// </summary>
        public List<(double X, double Y, T Value)> Query(double minX, double minY, double maxX, double maxY)
        {
            var result = new List<(double X, double Y, T Value)>();

            int startCellX = Math.Max(0, GetCellX(minX));
            int startCellY = Math.Max(0, GetCellY(minY));
            int endCellX = Math.Min(_cellCountX - 1, GetCellX(maxX));
            int endCellY = Math.Min(_cellCountY - 1, GetCellY(maxY));

            for (int x = startCellX; x <= endCellX; x++)
            {
                if (!_grid.TryGetValue(x, out var column))
                    continue;

                for (int y = startCellY; y <= endCellY; y++)
                {
                    if (!column.TryGetValue(y, out var cell))
                        continue;

                    foreach (var point in cell)
                    {
                        if (point.X >= minX && point.X <= maxX && point.Y >= minY && point.Y <= maxY)
                        {
                            result.Add(point);
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 获取单元格内的所有点
        /// </summary>
        public List<(double X, double Y, T Value)> GetCell(int cellX, int cellY)
        {
            if (_grid.TryGetValue(cellX, out var column) && column.TryGetValue(cellY, out var cell))
            {
                return new List<(double X, double Y, T Value)>(cell);
            }
            return new List<(double X, double Y, T Value)>();
        }

        /// <summary>
        /// 清空
        /// </summary>
        public void Clear()
        {
            _grid.Clear();
            _count = 0;
        }

        private int GetCellX(double x)
        {
            return Math.Min(_cellCountX - 1, Math.Max(0, (int)((x - _minX) / _cellWidth)));
        }

        private int GetCellY(double y)
        {
            return Math.Min(_cellCountY - 1, Math.Max(0, (int)((y - _minY) / _cellHeight)));
        }
    }
}
