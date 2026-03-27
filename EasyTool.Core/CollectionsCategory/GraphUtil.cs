using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyTool.CollectionsCategory
{
    /// <summary>
    /// 图算法工具类
    /// </summary>
    public static class GraphUtil
    {
        /// <summary>
        /// 广度优先搜索
        /// </summary>
        public static List<T> BFS<T>(Graph<T> graph, T start) where T : notnull
        {
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
        /// 深度优先搜索
        /// </summary>
        public static List<T> DFS<T>(Graph<T> graph, T start) where T : notnull
        {
            var result = new List<T>();
            var visited = new HashSet<T>();
            DFSVisit(graph, start, visited, result);
            return result;
        }

        private static void DFSVisit<T>(Graph<T> graph, T node, HashSet<T> visited, List<T> result) where T : notnull
        {
            visited.Add(node);
            result.Add(node);

            foreach (var neighbor in graph.GetNeighbors(node))
            {
                if (!visited.Contains(neighbor))
                {
                    DFSVisit(graph, neighbor, visited, result);
                }
            }
        }

        /// <summary>
        /// 最短路径（Dijkstra算法）
        /// </summary>
        public static List<T>? Dijkstra<T>(WeightedGraph<T> graph, T start, T end) where T : notnull
        {
            var distances = new Dictionary<T, double>();
            var previous = new Dictionary<T, T>();
            var unvisited = new HashSet<T>();

            foreach (var vertex in graph.Vertices)
            {
                distances[vertex] = double.PositiveInfinity;
                unvisited.Add(vertex);
            }
            distances[start] = 0;

            while (unvisited.Count > 0)
            {
                var current = default(T)!;
                var minDist = double.PositiveInfinity;

                foreach (var vertex in unvisited)
                {
                    if (distances[vertex] < minDist)
                    {
                        minDist = distances[vertex];
                        current = vertex;
                    }
                }

                if (current == null || current.Equals(end))
                    break;

                unvisited.Remove(current);

                foreach (var (neighbor, weight) in graph.GetWeightedNeighbors(current))
                {
                    var alt = distances[current] + weight;
                    if (alt < distances[neighbor])
                    {
                        distances[neighbor] = alt;
                        previous[neighbor] = current;
                    }
                }
            }

            // 重建路径
            if (!previous.ContainsKey(end) && !start.Equals(end))
                return null;

            var path = new List<T>();
            var current2 = end;
            while (current2 != null)
            {
                path.Insert(0, current2);
                current2 = previous.TryGetValue(current2, out var prev) ? prev : default;
                if (current2 == null && !path[0].Equals(start))
                    return null;
            }

            return path;
        }

        /// <summary>
        /// 拓扑排序
        /// </summary>
        public static List<T>? TopologicalSort<T>(Graph<T> graph) where T : notnull
        {
            var result = new List<T>();
            var visited = new HashSet<T>();
            var tempMarked = new HashSet<T>();

            foreach (var vertex in graph.Vertices)
            {
                if (!visited.Contains(vertex))
                {
                    if (!TopologicalVisit(graph, vertex, visited, tempMarked, result))
                        return null; // 存在环
                }
            }

            result.Reverse();
            return result;
        }

        private static bool TopologicalVisit<T>(Graph<T> graph, T node, HashSet<T> visited, HashSet<T> tempMarked, List<T> result) where T : notnull
        {
            if (tempMarked.Contains(node))
                return false; // 存在环

            if (visited.Contains(node))
                return true;

            tempMarked.Add(node);

            foreach (var neighbor in graph.GetNeighbors(node))
            {
                if (!TopologicalVisit(graph, neighbor, visited, tempMarked, result))
                    return false;
            }

            tempMarked.Remove(node);
            visited.Add(node);
            result.Add(node);
            return true;
        }

        /// <summary>
        /// 检测环
        /// </summary>
        public static bool HasCycle<T>(Graph<T> graph) where T : notnull
        {
            var visited = new HashSet<T>();
            var recursionStack = new HashSet<T>();

            foreach (var vertex in graph.Vertices)
            {
                if (HasCycleDFS(graph, vertex, visited, recursionStack))
                    return true;
            }

            return false;
        }

        private static bool HasCycleDFS<T>(Graph<T> graph, T node, HashSet<T> visited, HashSet<T> recursionStack) where T : notnull
        {
            if (recursionStack.Contains(node))
                return true;

            if (visited.Contains(node))
                return false;

            visited.Add(node);
            recursionStack.Add(node);

            foreach (var neighbor in graph.GetNeighbors(node))
            {
                if (HasCycleDFS(graph, neighbor, visited, recursionStack))
                    return true;
            }

            recursionStack.Remove(node);
            return false;
        }

        /// <summary>
        /// 连通分量
        /// </summary>
        public static List<List<T>> GetConnectedComponents<T>(Graph<T> graph) where T : notnull
        {
            var components = new List<List<T>>();
            var visited = new HashSet<T>();

            foreach (var vertex in graph.Vertices)
            {
                if (!visited.Contains(vertex))
                {
                    var component = new List<T>();
                    var queue = new Queue<T>();
                    queue.Enqueue(vertex);
                    visited.Add(vertex);

                    while (queue.Count > 0)
                    {
                        var current = queue.Dequeue();
                        component.Add(current);

                        foreach (var neighbor in graph.GetNeighbors(current))
                        {
                            if (!visited.Contains(neighbor))
                            {
                                visited.Add(neighbor);
                                queue.Enqueue(neighbor);
                            }
                        }
                    }

                    components.Add(component);
                }
            }

            return components;
        }
    }

    /// <summary>
    /// 图数据结构
    /// </summary>
    public class Graph<T> where T : notnull
    {
        private readonly Dictionary<T, List<T>> _adjacencyList = new();
        private readonly bool _directed;

        public Graph(bool directed = false)
        {
            _directed = directed;
        }

        /// <summary>
        /// 所有顶点
        /// </summary>
        public IEnumerable<T> Vertices => _adjacencyList.Keys;

        /// <summary>
        /// 边数
        /// </summary>
        public int EdgeCount { get; private set; }

        /// <summary>
        /// 顶点数
        /// </summary>
        public int VertexCount => _adjacencyList.Count;

        /// <summary>
        /// 添加顶点
        /// </summary>
        public void AddVertex(T vertex)
        {
            if (!_adjacencyList.ContainsKey(vertex))
            {
                _adjacencyList[vertex] = new List<T>();
            }
        }

        /// <summary>
        /// 添加边
        /// </summary>
        public void AddEdge(T from, T to)
        {
            AddVertex(from);
            AddVertex(to);

            _adjacencyList[from].Add(to);
            if (!_directed)
            {
                _adjacencyList[to].Add(from);
            }
            EdgeCount++;
        }

        /// <summary>
        /// 移除边
        /// </summary>
        public void RemoveEdge(T from, T to)
        {
            if (_adjacencyList.TryGetValue(from, out var neighbors))
            {
                neighbors.Remove(to);
            }

            if (!_directed && _adjacencyList.TryGetValue(to, out var neighbors2))
            {
                neighbors2.Remove(from);
            }

            EdgeCount--;
        }

        /// <summary>
        /// 获取邻居
        /// </summary>
        public IEnumerable<T> GetNeighbors(T vertex)
        {
            return _adjacencyList.TryGetValue(vertex, out var neighbors)
                ? neighbors
                : Enumerable.Empty<T>();
        }

        /// <summary>
        /// 是否有边
        /// </summary>
        public bool HasEdge(T from, T to)
        {
            return _adjacencyList.TryGetValue(from, out var neighbors) && neighbors.Contains(to);
        }
    }

    /// <summary>
    /// 带权重的图
    /// </summary>
    public class WeightedGraph<T> where T : notnull
    {
        private readonly Dictionary<T, List<(T Vertex, double Weight)>> _adjacencyList = new();
        private readonly bool _directed;

        public WeightedGraph(bool directed = false)
        {
            _directed = directed;
        }

        public IEnumerable<T> Vertices => _adjacencyList.Keys;
        public int VertexCount => _adjacencyList.Count;

        public void AddVertex(T vertex)
        {
            if (!_adjacencyList.ContainsKey(vertex))
            {
                _adjacencyList[vertex] = new List<(T, double)>();
            }
        }

        public void AddEdge(T from, T to, double weight)
        {
            AddVertex(from);
            AddVertex(to);

            _adjacencyList[from].Add((to, weight));
            if (!_directed)
            {
                _adjacencyList[to].Add((from, weight));
            }
        }

        public IEnumerable<(T Vertex, double Weight)> GetWeightedNeighbors(T vertex)
        {
            return _adjacencyList.TryGetValue(vertex, out var neighbors)
                ? neighbors
                : Enumerable.Empty<(T, double)>();
        }
    }
}