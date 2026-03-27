using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyTool.CollectionsCategory
{
    /// <summary>
    /// 树节点接口
    /// </summary>
    /// <typeparam name="T">节点数据类型</typeparam>
    public interface ITreeNode<T> where T : ITreeNode<T>
    {
        /// <summary>
        /// 节点ID
        /// </summary>
        string Id { get; }

        /// <summary>
        /// 父节点ID
        /// </summary>
        string? ParentId { get; }

        /// <summary>
        /// 子节点列表
        /// </summary>
        List<T> Children { get; set; }
    }

    /// <summary>
    /// 树节点基类
    /// </summary>
    public class TreeNodeBase : ITreeNode<TreeNodeBase>
    {
        public string Id { get; set; } = string.Empty;
        public string? ParentId { get; set; }
        public List<TreeNodeBase> Children { get; set; } = new();
    }

    /// <summary>
    /// 树形结构工具类
    /// 提供树形数据的构建、遍历、搜索等功能
    /// </summary>
    public static class TreeUtil
    {
        #region 构建树

        /// <summary>
        /// 将扁平列表构建为树形结构
        /// </summary>
        /// <typeparam name="T">节点类型</typeparam>
        /// <param name="flatList">扁平列表</param>
        /// <param name="idSelector">ID选择器</param>
        /// <param name="parentIdSelector">父ID选择器</param>
        /// <param name="rootParentId">根节点的父ID值</param>
        /// <returns>树形结构的根节点列表</returns>
        public static List<T> BuildTree<T>(IEnumerable<T> flatList, Func<T, string> idSelector, Func<T, string?> parentIdSelector, string? rootParentId = null)
        {
            if (flatList == null)
                return new List<T>();

            var lookup = flatList.ToLookup(parentIdSelector);
            var roots = lookup[rootParentId].ToList();

            void AddChildren(T parent)
            {
                var parentId = idSelector(parent);
                var children = lookup[parentId];
                var childrenProperty = typeof(T).GetProperty("Children");

                if (childrenProperty != null)
                {
                    var childrenList = childrenProperty.GetValue(parent);
                    if (childrenList == null)
                    {
                        childrenList = new List<T>();
                        childrenProperty.SetValue(parent, childrenList);
                    }

                    var addMethod = childrenList.GetType().GetMethod("AddRange");
                    addMethod?.Invoke(childrenList, new object[] { children });

                    foreach (var child in children)
                    {
                        AddChildren(child);
                    }
                }
            }

            foreach (var root in roots)
            {
                AddChildren(root);
            }

            return roots;
        }

        /// <summary>
        /// 将扁平列表构建为树形结构（使用 ITreeNode 接口）
        /// </summary>
        /// <typeparam name="T">节点类型</typeparam>
        /// <param name="flatList">扁平列表</param>
        /// <param name="rootParentId">根节点的父ID值</param>
        /// <returns>树形结构的根节点列表</returns>
        public static List<T> BuildTree<T>(IEnumerable<T> flatList, string? rootParentId = null) where T : ITreeNode<T>
        {
            if (flatList == null)
                return new List<T>();

            var lookup = flatList.ToLookup(x => x.ParentId);
            var roots = lookup[rootParentId].ToList();

            void AddChildren(T parent)
            {
                var children = lookup[parent.Id].ToList();
                parent.Children = children;

                foreach (var child in children)
                {
                    AddChildren(child);
                }
            }

            foreach (var root in roots)
            {
                AddChildren(root);
            }

            return roots;
        }

        #endregion

        #region 展平树

        /// <summary>
        /// 将树形结构展平为列表
        /// </summary>
        /// <typeparam name="T">节点类型</typeparam>
        /// <param name="roots">根节点列表</param>
        /// <returns>扁平列表</returns>
        public static List<T> Flatten<T>(IEnumerable<T> roots) where T : ITreeNode<T>
        {
            var result = new List<T>();

            void FlattenNode(T node)
            {
                result.Add(node);

                if (node.Children != null)
                {
                    foreach (var child in node.Children)
                    {
                        FlattenNode(child);
                    }
                }
            }

            foreach (var root in roots)
            {
                FlattenNode(root);
            }

            return result;
        }

        /// <summary>
        /// 将树形结构展平为列表（指定子节点选择器）
        /// </summary>
        /// <typeparam name="T">节点类型</typeparam>
        /// <param name="roots">根节点列表</param>
        /// <param name="childrenSelector">子节点选择器</param>
        /// <returns>扁平列表</returns>
        public static List<T> Flatten<T>(IEnumerable<T> roots, Func<T, IEnumerable<T>> childrenSelector)
        {
            var result = new List<T>();

            void FlattenNode(T node)
            {
                result.Add(node);

                var children = childrenSelector(node);
                if (children != null)
                {
                    foreach (var child in children)
                    {
                        FlattenNode(child);
                    }
                }
            }

            foreach (var root in roots)
            {
                FlattenNode(root);
            }

            return result;
        }

        #endregion

        #region 遍历树

        /// <summary>
        /// 前序遍历（深度优先）
        /// </summary>
        /// <typeparam name="T">节点类型</typeparam>
        /// <param name="roots">根节点列表</param>
        /// <param name="action">访问操作</param>
        public static void PreOrderTraversal<T>(IEnumerable<T> roots, Action<T> action) where T : ITreeNode<T>
        {
            foreach (var root in roots)
            {
                PreOrderTraversal(root, action);
            }
        }

        /// <summary>
        /// 前序遍历（深度优先）
        /// </summary>
        /// <typeparam name="T">节点类型</typeparam>
        /// <param name="node">节点</param>
        /// <param name="action">访问操作</param>
        public static void PreOrderTraversal<T>(T node, Action<T> action) where T : ITreeNode<T>
        {
            action(node);

            if (node.Children != null)
            {
                foreach (var child in node.Children)
                {
                    PreOrderTraversal(child, action);
                }
            }
        }

        /// <summary>
        /// 后序遍历
        /// </summary>
        /// <typeparam name="T">节点类型</typeparam>
        /// <param name="roots">根节点列表</param>
        /// <param name="action">访问操作</param>
        public static void PostOrderTraversal<T>(IEnumerable<T> roots, Action<T> action) where T : ITreeNode<T>
        {
            foreach (var root in roots)
            {
                PostOrderTraversal(root, action);
            }
        }

        /// <summary>
        /// 后序遍历
        /// </summary>
        /// <typeparam name="T">节点类型</typeparam>
        /// <param name="node">节点</param>
        /// <param name="action">访问操作</param>
        public static void PostOrderTraversal<T>(T node, Action<T> action) where T : ITreeNode<T>
        {
            if (node.Children != null)
            {
                foreach (var child in node.Children)
                {
                    PostOrderTraversal(child, action);
                }
            }

            action(node);
        }

        /// <summary>
        /// 层序遍历（广度优先）
        /// </summary>
        /// <typeparam name="T">节点类型</typeparam>
        /// <param name="roots">根节点列表</param>
        /// <param name="action">访问操作</param>
        public static void LevelOrderTraversal<T>(IEnumerable<T> roots, Action<T> action) where T : ITreeNode<T>
        {
            var queue = new Queue<T>();

            foreach (var root in roots)
            {
                queue.Enqueue(root);
            }

            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                action(node);

                if (node.Children != null)
                {
                    foreach (var child in node.Children)
                    {
                        queue.Enqueue(child);
                    }
                }
            }
        }

        #endregion

        #region 搜索树

        /// <summary>
        /// 查找节点
        /// </summary>
        /// <typeparam name="T">节点类型</typeparam>
        /// <param name="roots">根节点列表</param>
        /// <param name="predicate">查找条件</param>
        /// <returns>找到的节点</returns>
        public static T? Find<T>(IEnumerable<T> roots, Func<T, bool> predicate) where T : ITreeNode<T>
        {
            foreach (var root in roots)
            {
                var result = Find(root, predicate);
                if (result != null)
                    return result;
            }

            return default;
        }

        /// <summary>
        /// 查找节点
        /// </summary>
        /// <typeparam name="T">节点类型</typeparam>
        /// <param name="node">起始节点</param>
        /// <param name="predicate">查找条件</param>
        /// <returns>找到的节点</returns>
        public static T? Find<T>(T node, Func<T, bool> predicate) where T : ITreeNode<T>
        {
            if (predicate(node))
                return node;

            if (node.Children != null)
            {
                foreach (var child in node.Children)
                {
                    var result = Find(child, predicate);
                    if (result != null)
                        return result;
                }
            }

            return default;
        }

        /// <summary>
        /// 查找所有匹配的节点
        /// </summary>
        /// <typeparam name="T">节点类型</typeparam>
        /// <param name="roots">根节点列表</param>
        /// <param name="predicate">查找条件</param>
        /// <returns>找到的节点列表</returns>
        public static List<T> FindAll<T>(IEnumerable<T> roots, Func<T, bool> predicate) where T : ITreeNode<T>
        {
            var result = new List<T>();

            foreach (var root in roots)
            {
                FindAll(root, predicate, result);
            }

            return result;
        }

        private static void FindAll<T>(T node, Func<T, bool> predicate, List<T> result) where T : ITreeNode<T>
        {
            if (predicate(node))
                result.Add(node);

            if (node.Children != null)
            {
                foreach (var child in node.Children)
                {
                    FindAll(child, predicate, result);
                }
            }
        }

        /// <summary>
        /// 查找节点路径
        /// </summary>
        /// <typeparam name="T">节点类型</typeparam>
        /// <param name="roots">根节点列表</param>
        /// <param name="predicate">查找条件</param>
        /// <returns>从根到目标的路径</returns>
        public static List<T> FindPath<T>(IEnumerable<T> roots, Func<T, bool> predicate) where T : ITreeNode<T>
        {
            foreach (var root in roots)
            {
                var path = new List<T>();
                if (FindPath(root, predicate, path))
                    return path;
            }

            return new List<T>();
        }

        private static bool FindPath<T>(T node, Func<T, bool> predicate, List<T> path) where T : ITreeNode<T>
        {
            path.Add(node);

            if (predicate(node))
                return true;

            if (node.Children != null)
            {
                foreach (var child in node.Children)
                {
                    if (FindPath(child, predicate, path))
                        return true;
                }
            }

            path.RemoveAt(path.Count - 1);
            return false;
        }

        #endregion

        #region 树属性

        /// <summary>
        /// 获取树的深度
        /// </summary>
        /// <typeparam name="T">节点类型</typeparam>
        /// <param name="roots">根节点列表</param>
        /// <returns>最大深度</returns>
        public static int GetDepth<T>(IEnumerable<T> roots) where T : ITreeNode<T>
        {
            return roots.Max(root => GetDepth(root));
        }

        /// <summary>
        /// 获取树的深度
        /// </summary>
        /// <typeparam name="T">节点类型</typeparam>
        /// <param name="node">节点</param>
        /// <returns>深度</returns>
        public static int GetDepth<T>(T node) where T : ITreeNode<T>
        {
            if (node.Children == null || node.Children.Count == 0)
                return 1;

            return 1 + node.Children.Max(child => GetDepth(child));
        }

        /// <summary>
        /// 获取节点数量
        /// </summary>
        /// <typeparam name="T">节点类型</typeparam>
        /// <param name="roots">根节点列表</param>
        /// <returns>节点总数</returns>
        public static int GetNodeCount<T>(IEnumerable<T> roots) where T : ITreeNode<T>
        {
            return Flatten(roots).Count;
        }

        /// <summary>
        /// 获取叶子节点数量
        /// </summary>
        /// <typeparam name="T">节点类型</typeparam>
        /// <param name="roots">根节点列表</param>
        /// <returns>叶子节点数量</returns>
        public static int GetLeafCount<T>(IEnumerable<T> roots) where T : ITreeNode<T>
        {
            return Flatten(roots).Count(node => node.Children == null || node.Children.Count == 0);
        }

        /// <summary>
        /// 获取所有叶子节点
        /// </summary>
        /// <typeparam name="T">节点类型</typeparam>
        /// <param name="roots">根节点列表</param>
        /// <returns>叶子节点列表</returns>
        public static List<T> GetLeaves<T>(IEnumerable<T> roots) where T : ITreeNode<T>
        {
            return Flatten(roots).Where(node => node.Children == null || node.Children.Count == 0).ToList();
        }

        #endregion

        #region 树操作

        /// <summary>
        /// 过滤树节点
        /// </summary>
        /// <typeparam name="T">节点类型</typeparam>
        /// <param name="roots">根节点列表</param>
        /// <param name="predicate">过滤条件</param>
        /// <returns>过滤后的树</returns>
        public static List<T> Filter<T>(IEnumerable<T> roots, Func<T, bool> predicate) where T : ITreeNode<T>, new()
        {
            var result = new List<T>();

            foreach (var root in roots)
            {
                var filtered = FilterNode(root, predicate);
                if (filtered != null)
                    result.Add(filtered);
            }

            return result;
        }

        private static T? FilterNode<T>(T node, Func<T, bool> predicate) where T : ITreeNode<T>, new()
        {
            var filteredChildren = new List<T>();

            if (node.Children != null)
            {
                foreach (var child in node.Children)
                {
                    var filtered = FilterNode(child, predicate);
                    if (filtered != null)
                        filteredChildren.Add(filtered);
                }
            }

            if (predicate(node) || filteredChildren.Count > 0)
            {
                node.Children = filteredChildren;
                return node;
            }

            return default;
        }

        /// <summary>
        /// 映射树节点
        /// </summary>
        /// <typeparam name="TSource">源节点类型</typeparam>
        /// <typeparam name="TResult">结果节点类型</typeparam>
        /// <param name="roots">根节点列表</param>
        /// <param name="selector">映射函数</param>
        /// <returns>映射后的树</returns>
        public static List<TResult> Map<TSource, TResult>(IEnumerable<TSource> roots, Func<TSource, TResult> selector)
            where TSource : ITreeNode<TSource>
            where TResult : ITreeNode<TResult>, new()
        {
            var result = new List<TResult>();

            foreach (var root in roots)
            {
                result.Add(MapNode(root, selector));
            }

            return result;
        }

        private static TResult MapNode<TSource, TResult>(TSource node, Func<TSource, TResult> selector)
            where TSource : ITreeNode<TSource>
            where TResult : ITreeNode<TResult>, new()
        {
            var result = selector(node);
            result.Children = new List<TResult>();

            if (node.Children != null)
            {
                foreach (var child in node.Children)
                {
                    result.Children.Add(MapNode(child, selector));
                }
            }

            return result;
        }

        #endregion
    }
}