using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyTool.CollectionsCategory
{
    /// <summary>
    /// 图工具类
    /// </summary>
    public static class GraphUtil
    {
        /// <summary>
        /// 创建图
        /// </summary>
        public static Graph<T> Create<T>() where T : IEquatable<T>
        {
            return new Graph<T>();
        }

        /// <summary>
        /// 创建有向图
        /// </summary>
        public static Graph<T> CreateDirected<T>() where T : IEquatable<T>
        {
            return new Graph<T>(true);
        }
    }

    /// <summary>
    /// 图实现
    /// </summary>
    public class Graph<T> where T : IEquatable<T>
    {
        private readonly Dictionary<T, List<Edge>> _adjacencyList;
        private readonly bool _isDirected;

        /// <summary>
        /// 顶点数量
        /// </summary>
        public int VertexCount => _adjacencyList.Count;

        /// <summary>
        /// 边数量
        /// </summary>
        public int EdgeCount { get; private set; }

        /// <summary>
        /// 是否为有向图
        /// </summary>
        public bool IsDirected => _isDirected;

        /// <summary>
        /// 所有顶点
        /// </summary>
        public IEnumerable<T> Vertices => _adjacencyList.Keys;

        /// <summary>
        /// 创建图
        /// </summary>
        public Graph() : this(false)
        {
        }

        /// <summary>
        /// 创建图
        /// </summary>
        public Graph(bool isDirected)
        {
            _adjacencyList = new Dictionary<T, List<Edge>>();
            _isDirected = isDirected;
            EdgeCount = 0;
        }

        /// <summary>
        /// 添加顶点
        /// </summary>
        public void AddVertex(T vertex)
        {
            if (!_adjacencyList.ContainsKey(vertex))
            {
                _adjacencyList[vertex] = new List<Edge>();
            }
        }

        /// <summary>
        /// 添加边
        /// </summary>
        public void AddEdge(T from, T to, double weight = 1)
        {
            AddVertex(from);
            AddVertex(to);

            _adjacencyList[from].Add(new Edge(to, weight));
            EdgeCount++;

            if (!_isDirected)
            {
                _adjacencyList[to].Add(new Edge(from, weight));
            }
        }

        /// <summary>
        /// 移除顶点
        /// </summary>
        public bool RemoveVertex(T vertex)
        {
            if (!_adjacencyList.ContainsKey(vertex))
                return false;

            int edgeCount = _adjacencyList[vertex].Count;
            _adjacencyList.Remove(vertex);
            EdgeCount -= edgeCount;

            // 移除所有指向该顶点的边
            foreach (var edges in _adjacencyList.Values)
            {
                int removed = edges.RemoveAll(e => e.Target.Equals(vertex));
                if (!_isDirected)
                    EdgeCount -= removed;
            }

            return true;
        }

        /// <summary>
        /// 移除边
        /// </summary>
        public bool RemoveEdge(T from, T to)
        {
            if (!_adjacencyList.TryGetValue(from, out var edges))
                return false;

            int removed = edges.RemoveAll(e => e.Target.Equals(to));
            if (removed > 0)
            {
                EdgeCount--;

                if (!_isDirected && _adjacencyList.TryGetValue(to, out var reverseEdges))
                {
                    reverseEdges.RemoveAll(e => e.Target.Equals(from));
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// 获取邻居
        /// </summary>
        public IEnumerable<T> GetNeighbors(T vertex)
        {
            if (!_adjacencyList.TryGetValue(vertex, out var edges))
                return Enumerable.Empty<T>();

            return edges.Select(e => e.Target);
        }

        /// <summary>
        /// 获取边权重
        /// </summary>
        public double GetEdgeWeight(T from, T to)
        {
            if (!_adjacencyList.TryGetValue(from, out var edges))
                return double.PositiveInfinity;

            var edge = edges.FirstOrDefault(e => e.Target.Equals(to));
            return edge?.Weight ?? double.PositiveInfinity;
        }

        /// <summary>
        /// 是否包含顶点
        /// </summary>
        public bool ContainsVertex(T vertex)
        {
            return _adjacencyList.ContainsKey(vertex);
        }

        /// <summary>
        /// 是否包含边
        /// </summary>
        public bool ContainsEdge(T from, T to)
        {
            if (!_adjacencyList.TryGetValue(from, out var edges))
                return false;

            return edges.Any(e => e.Target.Equals(to));
        }

        /// <summary>
        /// 获取顶点的度
        /// </summary>
        public int GetDegree(T vertex)
        {
            if (!_adjacencyList.TryGetValue(vertex, out var edges))
                return 0;

            return edges.Count;
        }

        private class Edge
        {
            public T Target { get; }
            public double Weight { get; }

            public Edge(T target, double weight)
            {
                Target = target;
                Weight = weight;
            }
        }
    }

    /// <summary>
    /// 图遍历工具类
    /// </summary>
    public static class GraphTraversalUtil
    {
        /// <summary>
        /// 深度优先搜索
        /// </summary>
        public static List<T> DFS<T>(Graph<T> graph, T start) where T : IEquatable<T>
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));

            var result = new List<T>();
            var visited = new HashSet<T>();

            DFSVisit(graph, start, visited, result);

            return result;
        }

        private static void DFSVisit<T>(Graph<T> graph, T vertex, HashSet<T> visited, List<T> result) where T : IEquatable<T>
        {
            if (visited.Contains(vertex))
                return;

            visited.Add(vertex);
            result.Add(vertex);

            foreach (var neighbor in graph.GetNeighbors(vertex))
            {
                if (!visited.Contains(neighbor))
                {
                    DFSVisit(graph, neighbor, visited, result);
                }
            }
        }

        /// <summary>
        /// 广度优先搜索
        /// </summary>
        public static List<T> BFS<T>(Graph<T> graph, T start) where T : IEquatable<T>
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));

            var result = new List<T>();
            var visited = new HashSet<T>();
            var queue = new Queue<T>();

            queue.Enqueue(start);
            visited.Add(start);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                result.Add(current);

                foreach (var neighbor in graph.GetNeighbors(current))
                {
                    if (!visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 查找路径（BFS）
        /// </summary>
        public static List<T> FindPath<T>(Graph<T> graph, T start, T end) where T : IEquatable<T>
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));

            if (!graph.ContainsVertex(start) || !graph.ContainsVertex(end))
                return null;

            var visited = new HashSet<T>();
            var parent = new Dictionary<T, T>();
            var queue = new Queue<T>();

            queue.Enqueue(start);
            visited.Add(start);
            parent[start] = default;

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                if (current.Equals(end))
                {
                    return ReconstructPath(parent, start, end);
                }

                foreach (var neighbor in graph.GetNeighbors(current))
                {
                    if (!visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        parent[neighbor] = current;
                        queue.Enqueue(neighbor);
                    }
                }
            }

            return null;
        }

        private static List<T> ReconstructPath<T>(Dictionary<T, T> parent, T start, T end)
        {
            var path = new List<T>();
            var current = end;

            while (!current.Equals(default))
            {
                path.Add(current);
                if (current.Equals(start))
                    break;
                current = parent[current];
            }

            path.Reverse();
            return path;
        }
    }

    /// <summary>
    /// 拓扑排序工具类
    /// </summary>
    public static class TopologicalSortUtil
    {
        /// <summary>
        /// 拓扑排序
        /// </summary>
        public static List<T> Sort<T>(Graph<T> graph) where T : IEquatable<T>
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));
            if (!graph.IsDirected)
                throw new ArgumentException("Topological sort requires a directed graph");

            var inDegree = new Dictionary<T, int>();
            foreach (var vertex in graph.Vertices)
            {
                inDegree[vertex] = 0;
            }

            foreach (var vertex in graph.Vertices)
            {
                foreach (var neighbor in graph.GetNeighbors(vertex))
                {
                    inDegree[neighbor]++;
                }
            }

            var queue = new Queue<T>();
            foreach (var kvp in inDegree)
            {
                if (kvp.Value == 0)
                {
                    queue.Enqueue(kvp.Key);
                }
            }

            var result = new List<T>();

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                result.Add(current);

                foreach (var neighbor in graph.GetNeighbors(current))
                {
                    inDegree[neighbor]--;
                    if (inDegree[neighbor] == 0)
                    {
                        queue.Enqueue(neighbor);
                    }
                }
            }

            if (result.Count != graph.VertexCount)
            {
                throw new InvalidOperationException("Graph contains a cycle");
            }

            return result;
        }

        /// <summary>
        /// 尝试拓扑排序
        /// </summary>
        public static bool TrySort<T>(Graph<T> graph, out List<T> result) where T : IEquatable<T>
        {
            try
            {
                result = Sort(graph);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }
    }

    /// <summary>
    /// 环检测工具类
    /// </summary>
    public static class CycleDetectionUtil
    {
        /// <summary>
        /// 检测是否有环
        /// </summary>
        public static bool HasCycle<T>(Graph<T> graph) where T : IEquatable<T>
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));

            if (graph.IsDirected)
            {
                return HasCycleDirected(graph);
            }
            else
            {
                return HasCycleUndirected(graph);
            }
        }

        private static bool HasCycleDirected<T>(Graph<T> graph) where T : IEquatable<T>
        {
            var white = new HashSet<T>(graph.Vertices); // 未访问
            var gray = new HashSet<T>(); // 正在访问
            var black = new HashSet<T>(); // 已完成

            foreach (var vertex in graph.Vertices)
            {
                if (white.Contains(vertex))
                {
                    if (DFSCycleDirected(graph, vertex, white, gray, black))
                        return true;
                }
            }

            return false;
        }

        private static bool DFSCycleDirected<T>(Graph<T> graph, T vertex, HashSet<T> white, HashSet<T> gray, HashSet<T> black) where T : IEquatable<T>
        {
            white.Remove(vertex);
            gray.Add(vertex);

            foreach (var neighbor in graph.GetNeighbors(vertex))
            {
                if (black.Contains(neighbor))
                    continue;

                if (gray.Contains(neighbor))
                    return true;

                if (DFSCycleDirected(graph, neighbor, white, gray, black))
                    return true;
            }

            gray.Remove(vertex);
            black.Add(vertex);
            return false;
        }

        private static bool HasCycleUndirected<T>(Graph<T> graph) where T : IEquatable<T>
        {
            var visited = new HashSet<T>();

            foreach (var vertex in graph.Vertices)
            {
                if (!visited.Contains(vertex))
                {
                    if (DFSCycleUndirected(graph, vertex, default, visited))
                        return true;
                }
            }

            return false;
        }

        private static bool DFSCycleUndirected<T>(Graph<T> graph, T vertex, T parent, HashSet<T> visited) where T : IEquatable<T>
        {
            visited.Add(vertex);

            foreach (var neighbor in graph.GetNeighbors(vertex))
            {
                if (!visited.Contains(neighbor))
                {
                    if (DFSCycleUndirected(graph, neighbor, vertex, visited))
                        return true;
                }
                else if (!neighbor.Equals(parent))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 查找环
        /// </summary>
        public static List<T> FindCycle<T>(Graph<T> graph) where T : IEquatable<T>
        {
            if (graph == null || !graph.IsDirected)
                return null;

            var visited = new HashSet<T>();
            var recStack = new HashSet<T>();
            var path = new List<T>();

            foreach (var vertex in graph.Vertices)
            {
                if (FindCycleDFS(graph, vertex, visited, recStack, path))
                {
                    return path;
                }
            }

            return null;
        }

        private static bool FindCycleDFS<T>(Graph<T> graph, T vertex, HashSet<T> visited, HashSet<T> recStack, List<T> path) where T : IEquatable<T>
        {
            visited.Add(vertex);
            recStack.Add(vertex);
            path.Add(vertex);

            foreach (var neighbor in graph.GetNeighbors(vertex))
            {
                if (!visited.Contains(neighbor))
                {
                    if (FindCycleDFS(graph, neighbor, visited, recStack, path))
                        return true;
                }
                else if (recStack.Contains(neighbor))
                {
                    // 找到环，截取环部分
                    int start = path.IndexOf(neighbor);
                    path.RemoveRange(0, start);
                    return true;
                }
            }

            recStack.Remove(vertex);
            path.RemoveAt(path.Count - 1);
            return false;
        }
    }

    /// <summary>
    /// 连通分量工具类
    /// </summary>
    public static class ConnectedComponentsUtil
    {
        /// <summary>
        /// 获取连通分量
        /// </summary>
        public static List<List<T>> GetComponents<T>(Graph<T> graph) where T : IEquatable<T>
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));

            var visited = new HashSet<T>();
            var components = new List<List<T>>();

            foreach (var vertex in graph.Vertices)
            {
                if (!visited.Contains(vertex))
                {
                    var component = new List<T>();
                    DFSComponent(graph, vertex, visited, component);
                    components.Add(component);
                }
            }

            return components;
        }

        private static void DFSComponent<T>(Graph<T> graph, T vertex, HashSet<T> visited, List<T> component) where T : IEquatable<T>
        {
            visited.Add(vertex);
            component.Add(vertex);

            foreach (var neighbor in graph.GetNeighbors(vertex))
            {
                if (!visited.Contains(neighbor))
                {
                    DFSComponent(graph, neighbor, visited, component);
                }
            }
        }

        /// <summary>
        /// 判断是否连通
        /// </summary>
        public static bool IsConnected<T>(Graph<T> graph) where T : IEquatable<T>
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));

            if (graph.VertexCount == 0)
                return true;

            return GetComponents(graph).Count == 1;
        }

        /// <summary>
        /// 获取强连通分量（Kosaraju 算法）
        /// </summary>
        public static List<List<T>> GetStronglyConnectedComponents<T>(Graph<T> graph) where T : IEquatable<T>
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));
            if (!graph.IsDirected)
                throw new ArgumentException("Strongly connected components require a directed graph");

            var visited = new HashSet<T>();
            var finishOrder = new Stack<T>();

            // 第一次 DFS 获取完成顺序
            foreach (var vertex in graph.Vertices)
            {
                if (!visited.Contains(vertex))
                {
                    DFSOrder(graph, vertex, visited, finishOrder);
                }
            }

            // 构建转置图
            var transpose = Transpose(graph);

            // 第二次 DFS 按完成顺序的逆序
            visited.Clear();
            var components = new List<List<T>>();

            while (finishOrder.Count > 0)
            {
                var vertex = finishOrder.Pop();
                if (!visited.Contains(vertex))
                {
                    var component = new List<T>();
                    DFSComponent(transpose, vertex, visited, component);
                    components.Add(component);
                }
            }

            return components;
        }

        private static void DFSOrder<T>(Graph<T> graph, T vertex, HashSet<T> visited, Stack<T> finishOrder) where T : IEquatable<T>
        {
            visited.Add(vertex);

            foreach (var neighbor in graph.GetNeighbors(vertex))
            {
                if (!visited.Contains(neighbor))
                {
                    DFSOrder(graph, neighbor, visited, finishOrder);
                }
            }

            finishOrder.Push(vertex);
        }

        private static Graph<T> Transpose<T>(Graph<T> graph) where T : IEquatable<T>
        {
            var transpose = new Graph<T>(true);

            foreach (var vertex in graph.Vertices)
            {
                transpose.AddVertex(vertex);
            }

            foreach (var vertex in graph.Vertices)
            {
                foreach (var neighbor in graph.GetNeighbors(vertex))
                {
                    transpose.AddEdge(neighbor, vertex);
                }
            }

            return transpose;
        }
    }

    /// <summary>
    /// 最短路径工具类
    /// </summary>
    public static class ShortestPathUtil
    {
        /// <summary>
        /// Dijkstra 最短路径算法
        /// </summary>
        public static Dictionary<T, double> Dijkstra<T>(Graph<T> graph, T start) where T : IEquatable<T>
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));
            if (!graph.ContainsVertex(start))
                throw new ArgumentException("Start vertex not found");

            var distances = new Dictionary<T, double>();
            var visited = new HashSet<T>();
            var pq = PriorityQueueUtil.CreateMin<T, double>();

            foreach (var vertex in graph.Vertices)
            {
                distances[vertex] = double.PositiveInfinity;
            }
            distances[start] = 0;
            pq.Enqueue(start, 0);

            while (pq.Count > 0)
            {
                var current = pq.Dequeue();

                if (visited.Contains(current))
                    continue;

                visited.Add(current);

                foreach (var neighbor in graph.GetNeighbors(current))
                {
                    var weight = graph.GetEdgeWeight(current, neighbor);
                    var newDist = distances[current] + weight;

                    if (newDist < distances[neighbor])
                    {
                        distances[neighbor] = newDist;
                        pq.Enqueue(neighbor, newDist);
                    }
                }
            }

            return distances;
        }

        /// <summary>
        /// Dijkstra 最短路径（带路径）
        /// </summary>
        public static (Dictionary<T, double> Distances, Dictionary<T, T> Previous) DijkstraWithPath<T>(Graph<T> graph, T start) where T : IEquatable<T>
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));
            if (!graph.ContainsVertex(start))
                throw new ArgumentException("Start vertex not found");

            var distances = new Dictionary<T, double>();
            var previous = new Dictionary<T, T>();
            var visited = new HashSet<T>();
            var pq = PriorityQueueUtil.CreateMin<T, double>();

            foreach (var vertex in graph.Vertices)
            {
                distances[vertex] = double.PositiveInfinity;
            }
            distances[start] = 0;
            pq.Enqueue(start, 0);

            while (pq.Count > 0)
            {
                var current = pq.Dequeue();

                if (visited.Contains(current))
                    continue;

                visited.Add(current);

                foreach (var neighbor in graph.GetNeighbors(current))
                {
                    var weight = graph.GetEdgeWeight(current, neighbor);
                    var newDist = distances[current] + weight;

                    if (newDist < distances[neighbor])
                    {
                        distances[neighbor] = newDist;
                        previous[neighbor] = current;
                        pq.Enqueue(neighbor, newDist);
                    }
                }
            }

            return (distances, previous);
        }

        /// <summary>
        /// 重建路径
        /// </summary>
        public static List<T> ReconstructPath<T>(Dictionary<T, T> previous, T start, T end)
        {
            var path = new List<T>();
            var current = end;

            while (!current.Equals(default))
            {
                path.Add(current);
                if (current.Equals(start))
                    break;

                if (!previous.ContainsKey(current))
                    return null; // 无法到达

                current = previous[current];
            }

            path.Reverse();
            return path.Count > 0 && path[0].Equals(start) ? path : null;
        }

        /// <summary>
        /// Bellman-Ford 算法（支持负权边）
        /// </summary>
        public static Dictionary<T, double> BellmanFord<T>(Graph<T> graph, T start) where T : IEquatable<T>
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));
            if (!graph.ContainsVertex(start))
                throw new ArgumentException("Start vertex not found");

            var distances = new Dictionary<T, double>();

            foreach (var vertex in graph.Vertices)
            {
                distances[vertex] = double.PositiveInfinity;
            }
            distances[start] = 0;

            // 松弛 V-1 次
            for (int i = 0; i < graph.VertexCount - 1; i++)
            {
                foreach (var u in graph.Vertices)
                {
                    if (distances[u] == double.PositiveInfinity)
                        continue;

                    foreach (var v in graph.GetNeighbors(u))
                    {
                        var weight = graph.GetEdgeWeight(u, v);
                        if (distances[u] + weight < distances[v])
                        {
                            distances[v] = distances[u] + weight;
                        }
                    }
                }
            }

            // 检查负环
            foreach (var u in graph.Vertices)
            {
                if (distances[u] == double.PositiveInfinity)
                    continue;

                foreach (var v in graph.GetNeighbors(u))
                {
                    var weight = graph.GetEdgeWeight(u, v);
                    if (distances[u] + weight < distances[v])
                    {
                        throw new InvalidOperationException("Graph contains a negative cycle");
                    }
                }
            }

            return distances;
        }
    }

    /// <summary>
    /// 最小生成树工具类
    /// </summary>
    public static class MSTUtil
    {
        /// <summary>
        /// Prim 算法
        /// </summary>
        public static List<(T From, T To, double Weight)> Prim<T>(Graph<T> graph) where T : IEquatable<T>
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));
            if (graph.IsDirected)
                throw new ArgumentException("MST requires an undirected graph");
            if (graph.VertexCount == 0)
                return new List<(T, T, double)>();

            var mst = new List<(T From, T To, double Weight)>();
            var visited = new HashSet<T>();
            var pq = PriorityQueueUtil.CreateMin<(T From, T To), double>();

            var startVertex = graph.Vertices.First();
            visited.Add(startVertex);

            foreach (var neighbor in graph.GetNeighbors(startVertex))
            {
                var weight = graph.GetEdgeWeight(startVertex, neighbor);
                pq.Enqueue((startVertex, neighbor), weight);
            }

            while (pq.Count > 0 && visited.Count < graph.VertexCount)
            {
                var (from, to) = pq.Dequeue();

                if (visited.Contains(to))
                    continue;

                visited.Add(to);
                var weight = graph.GetEdgeWeight(from, to);
                mst.Add((from, to, weight));

                foreach (var neighbor in graph.GetNeighbors(to))
                {
                    if (!visited.Contains(neighbor))
                    {
                        var neighborWeight = graph.GetEdgeWeight(to, neighbor);
                        pq.Enqueue((to, neighbor), neighborWeight);
                    }
                }
            }

            return mst;
        }

        /// <summary>
        /// Kruskal 算法
        /// </summary>
        public static List<(T From, T To, double Weight)> Kruskal<T>(Graph<T> graph) where T : IEquatable<T>
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));
            if (graph.IsDirected)
                throw new ArgumentException("MST requires an undirected graph");

            var mst = new List<(T From, T To, double Weight)>();
            var edges = new List<(T From, T To, double Weight)>();
            var processed = new HashSet<(T, T)>();

            foreach (var from in graph.Vertices)
            {
                foreach (var to in graph.GetNeighbors(from))
                {
                    var key = from.GetHashCode() < to.GetHashCode() ? (from, to) : (to, from);
                    if (!processed.Contains(key))
                    {
                        processed.Add(key);
                        var weight = graph.GetEdgeWeight(from, to);
                        edges.Add((from, to, weight));
                    }
                }
            }

            edges.Sort((a, b) => a.Weight.CompareTo(b.Weight));

            var uf = UnionFindUtil.Create(graph.Vertices.ToList());

            foreach (var edge in edges)
            {
                if (!uf.Connected(edge.From, edge.To))
                {
                    uf.Union(edge.From, edge.To);
                    mst.Add(edge);
                }
            }

            return mst;
        }
    }

}
