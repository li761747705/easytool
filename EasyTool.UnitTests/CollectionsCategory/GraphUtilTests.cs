using System;
using System.Collections.Generic;
using System.Linq;
using EasyTool.CollectionsCategory;
using Xunit;

namespace EasyTool.UnitTests.CollectionsCategory
{
    public class GraphUtilTests
    {
        #region BFS 遍历测试

        [Fact]
        public void BFS_traversal_order()
        {
            // 构建有向图: A->B, A->C, B->D
            var graph = new Graph<string>(directed: true);
            graph.AddEdge("A", "B");
            graph.AddEdge("A", "C");
            graph.AddEdge("B", "D");

            var result = GraphUtil.BFS(graph, "A");

            // BFS 层序: A, B, C, D
            Assert.Equal(4, result.Count);
            Assert.Equal("A", result[0]);
            Assert.Equal("B", result[1]);
            Assert.Equal("C", result[2]);
            Assert.Equal("D", result[3]);
        }

        #endregion

        #region DFS 遍历测试

        [Fact]
        public void DFS_traversal_order()
        {
            // 构建有向图: A->B, A->C, B->D
            var graph = new Graph<string>(directed: true);
            graph.AddEdge("A", "B");
            graph.AddEdge("A", "C");
            graph.AddEdge("B", "D");

            var result = GraphUtil.DFS(graph, "A");

            // DFS 深度优先: A, B, D, C (先深入 B 分支)
            Assert.Equal(4, result.Count);
            Assert.Equal("A", result[0]);
            Assert.Equal("B", result[1]);
            Assert.Equal("D", result[2]);
            Assert.Equal("C", result[3]);
        }

        #endregion

        #region Dijkstra 最短路径测试

        [Fact]
        public void Dijkstra_shortest_path()
        {
            // 构建加权有向图:
            // A --1--> B --2--> D
            // A --4--> C --1--> D
            // 最短路径 A->B->D, 距离 3
            var graph = new WeightedGraph<string>(directed: true);
            graph.AddEdge("A", "B", 1);
            graph.AddEdge("A", "C", 4);
            graph.AddEdge("B", "D", 2);
            graph.AddEdge("C", "D", 1);

            var path = GraphUtil.Dijkstra(graph, "A", "D");

            Assert.NotNull(path);
            Assert.Equal(new[] { "A", "B", "D" }, path);
        }

        [Fact]
        public void Dijkstra_same_start_end_returns_single_node()
        {
            var graph = new WeightedGraph<string>(directed: true);
            graph.AddVertex("A");
            graph.AddVertex("B");
            graph.AddEdge("A", "B", 1);

            var path = GraphUtil.Dijkstra(graph, "A", "A");

            Assert.NotNull(path);
            Assert.Single(path);
            Assert.Equal("A", path![0]);
        }

        [Fact]
        public void Dijkstra_no_path_returns_null()
        {
            var graph = new WeightedGraph<string>(directed: true);
            graph.AddVertex("A");
            graph.AddVertex("B");

            var path = GraphUtil.Dijkstra(graph, "A", "B");

            Assert.Null(path);
        }

        #endregion

        #region 拓扑排序测试

        [Fact]
        public void TopologicalSort_valid()
        {
            // DAG: A->B, A->C, B->D, C->D
            var graph = new Graph<string>(directed: true);
            graph.AddEdge("A", "B");
            graph.AddEdge("A", "C");
            graph.AddEdge("B", "D");
            graph.AddEdge("C", "D");

            var result = GraphUtil.TopologicalSort(graph);

            Assert.NotNull(result);
            Assert.Equal(4, result.Count);

            // 验证拓扑序约束: 所有边 from 在 to 之前
            var indexMap = result!.Select((v, i) => (v, i)).ToDictionary(p => p.v, p => p.i);
            Assert.True(indexMap["A"] < indexMap["B"]);
            Assert.True(indexMap["A"] < indexMap["C"]);
            Assert.True(indexMap["B"] < indexMap["D"]);
            Assert.True(indexMap["C"] < indexMap["D"]);
        }

        [Fact]
        public void TopologicalSort_cycle_detection()
        {
            // 有环图: A->B, B->C, C->A
            var graph = new Graph<string>(directed: true);
            graph.AddEdge("A", "B");
            graph.AddEdge("B", "C");
            graph.AddEdge("C", "A");

            var result = GraphUtil.TopologicalSort(graph);

            Assert.Null(result);
        }

        #endregion

        #region 连通分量测试

        [Fact]
        public void ConnectedComponents_disconnected_graph()
        {
            // 无向图: A-B 和 C-D 两组互不相连
            var graph = new Graph<string>(directed: false);
            graph.AddEdge("A", "B");
            graph.AddEdge("C", "D");

            var components = GraphUtil.GetConnectedComponents(graph);

            Assert.Equal(2, components.Count);

            // 验证每个分量包含正确的节点
            var allNodes = components.SelectMany(c => c).OrderBy(n => n).ToList();
            Assert.Equal(new[] { "A", "B", "C", "D" }, allNodes);

            // 验证 A 和 B 在同一分量
            var componentAB = components.Single(c => c.Contains("A"));
            Assert.Contains("B", componentAB);

            // 验证 C 和 D 在同一分量
            var componentCD = components.Single(c => c.Contains("C"));
            Assert.Contains("D", componentCD);
        }

        [Fact]
        public void ConnectedComponents_connected_graph_returns_single_component()
        {
            var graph = new Graph<string>(directed: false);
            graph.AddEdge("A", "B");
            graph.AddEdge("B", "C");
            graph.AddEdge("C", "A");

            var components = GraphUtil.GetConnectedComponents(graph);

            Assert.Single(components);
            Assert.Equal(3, components[0].Count);
        }

        #endregion

        #region 空图测试

        [Fact]
        public void Empty_graph_operations()
        {
            var graph = new Graph<string>(directed: true);

            Assert.Equal(0, graph.VertexCount);
            Assert.Equal(0, graph.EdgeCount);
            Assert.Empty(graph.Vertices);
            Assert.Empty(graph.GetNeighbors("A"));
            Assert.False(graph.HasEdge("A", "B"));

            // TopologicalSort on empty graph
            var topoResult = GraphUtil.TopologicalSort(graph);
            Assert.NotNull(topoResult);
            Assert.Empty(topoResult);

            // GetConnectedComponents on empty graph
            var components = GraphUtil.GetConnectedComponents(graph);
            Assert.Empty(components);

            // HasCycle on empty graph
            Assert.False(GraphUtil.HasCycle(graph));
        }

        #endregion

        #region 单节点图测试

        [Fact]
        public void Single_node_graph_operations()
        {
            var graph = new Graph<string>(directed: true);
            graph.AddVertex("A");

            Assert.Equal(1, graph.VertexCount);
            Assert.Equal(0, graph.EdgeCount);
            Assert.Empty(graph.GetNeighbors("A"));

            // BFS on single node
            var bfsResult = GraphUtil.BFS(graph, "A");
            Assert.Single(bfsResult);
            Assert.Equal("A", bfsResult[0]);

            // DFS on single node
            var dfsResult = GraphUtil.DFS(graph, "A");
            Assert.Single(dfsResult);
            Assert.Equal("A", dfsResult[0]);

            // TopologicalSort on single node
            var topoResult = GraphUtil.TopologicalSort(graph);
            Assert.NotNull(topoResult);
            Assert.Single(topoResult);
            Assert.Equal("A", topoResult![0]);

            // HasCycle on single node (no self-loop)
            Assert.False(GraphUtil.HasCycle(graph));

            // ConnectedComponents on single node
            var components = GraphUtil.GetConnectedComponents(graph);
            Assert.Single(components);
            Assert.Single(components[0]);
            Assert.Equal("A", components[0][0]);
        }

        #endregion
    }
}
