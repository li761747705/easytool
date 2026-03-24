using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyTool.CollectionsCategory
{
    /// <summary>
    /// 树形结构构建工具类
    /// 用于将扁平列表转换为树形结构
    /// </summary>
    public static class TreeBuildUtil
    {
        /// <summary>
        /// 创建树构建器
        /// </summary>
        /// <typeparam name="T">节点类型</typeparam>
        /// <typeparam name="TKey">键类型</typeparam>
        /// <returns>树构建器</returns>
        public static TreeBuilder<T, TKey> CreateBuilder<T, TKey>()
            where T : class
            where TKey : notnull
        {
            return new TreeBuilder<T, TKey>();
        }

        /// <summary>
        /// 将扁平列表构建为树形结构
        /// </summary>
        /// <typeparam name="T">节点类型</typeparam>
        /// <typeparam name="TKey">键类型</typeparam>
        /// <param name="items">扁平列表</param>
        /// <param name="idSelector">ID选择器</param>
        /// <param name="parentIdSelector">父ID选择器</param>
        /// <param name="childrenSetter">子节点设置器</param>
        /// <param name="rootPredicate">根节点判断（可选，默认为父ID为null或默认值）</param>
        /// <returns>树形结构列表</returns>
        public static List<T> Build<T, TKey>(
            IEnumerable<T> items,
            Func<T, TKey> idSelector,
            Func<T, TKey?> parentIdSelector,
            Action<T, List<T>> childrenSetter,
            Func<T, bool>? rootPredicate = null)
            where T : class
            where TKey : notnull
        {
            if (items == null)
                return new List<T>();

            var itemList = items.ToList();
            var lookup = new Dictionary<TKey, T>();
            var childrenLookup = new Dictionary<TKey, List<T>>();

            // 第一遍：建立ID映射
            foreach (var item in itemList)
            {
                var id = idSelector(item);
                lookup[id] = item;
            }

            // 第二遍：建立父子关系
            foreach (var item in itemList)
            {
                var parentId = parentIdSelector(item);
                if (parentId == null || EqualityComparer<TKey>.Default.Equals(parentId, default!))
                    continue;

                if (!childrenLookup.ContainsKey(parentId))
                    childrenLookup[parentId] = new List<T>();

                childrenLookup[parentId].Add(item);
            }

            // 设置子节点
            foreach (var kvp in childrenLookup)
            {
                if (lookup.TryGetValue(kvp.Key, out var parent))
                {
                    childrenSetter(parent, kvp.Value);
                }
            }

            // 获取根节点
            if (rootPredicate != null)
            {
                return itemList.Where(rootPredicate).ToList();
            }

            return itemList.Where(item =>
            {
                var parentId = parentIdSelector(item);
                return parentId == null ||
                       EqualityComparer<TKey>.Default.Equals(parentId, default!) ||
                       !lookup.ContainsKey(parentId);
            }).ToList();
        }

        /// <summary>
        /// 将树形结构扁平化
        /// </summary>
        /// <typeparam name="T">节点类型</typeparam>
        /// <param name="roots">根节点列表</param>
        /// <param name="childrenSelector">子节点选择器</param>
        /// <returns>扁平化列表</returns>
        public static List<T> Flatten<T>(
            IEnumerable<T> roots,
            Func<T, IEnumerable<T>?> childrenSelector)
        {
            var result = new List<T>();
            FlattenInternal(roots, childrenSelector, result);
            return result;
        }

        private static void FlattenInternal<T>(
            IEnumerable<T> items,
            Func<T, IEnumerable<T>?> childrenSelector,
            List<T> result)
        {
            foreach (var item in items)
            {
                result.Add(item);
                var children = childrenSelector(item);
                if (children != null)
                {
                    FlattenInternal(children, childrenSelector, result);
                }
            }
        }

        /// <summary>
        /// 遍历树（深度优先）
        /// </summary>
        /// <typeparam name="T">节点类型</typeparam>
        /// <param name="roots">根节点列表</param>
        /// <param name="childrenSelector">子节点选择器</param>
        /// <param name="action">遍历操作</param>
        public static void Traverse<T>(
            IEnumerable<T> roots,
            Func<T, IEnumerable<T>?> childrenSelector,
            Action<T, int> action)
        {
            TraverseInternal(roots, childrenSelector, action, 0);
        }

        private static void TraverseInternal<T>(
            IEnumerable<T> items,
            Func<T, IEnumerable<T>?> childrenSelector,
            Action<T, int> action,
            int level)
        {
            foreach (var item in items)
            {
                action(item, level);
                var children = childrenSelector(item);
                if (children != null)
                {
                    TraverseInternal(children, childrenSelector, action, level + 1);
                }
            }
        }

        /// <summary>
        /// 遍历树（广度优先）
        /// </summary>
        /// <typeparam name="T">节点类型</typeparam>
        /// <param name="roots">根节点列表</param>
        /// <param name="childrenSelector">子节点选择器</param>
        /// <param name="action">遍历操作</param>
        public static void TraverseBreadthFirst<T>(
            IEnumerable<T> roots,
            Func<T, IEnumerable<T>?> childrenSelector,
            Action<T, int> action)
        {
            var queue = new Queue<(T Item, int Level)>();

            foreach (var root in roots)
            {
                queue.Enqueue((root, 0));
            }

            while (queue.Count > 0)
            {
                var (item, level) = queue.Dequeue();
                action(item, level);

                var children = childrenSelector(item);
                if (children != null)
                {
                    foreach (var child in children)
                    {
                        queue.Enqueue((child, level + 1));
                    }
                }
            }
        }

        /// <summary>
        /// 查找节点
        /// </summary>
        /// <typeparam name="T">节点类型</typeparam>
        /// <param name="roots">根节点列表</param>
        /// <param name="childrenSelector">子节点选择器</param>
        /// <param name="predicate">查找条件</param>
        /// <returns>找到的节点</returns>
        public static T? Find<T>(
            IEnumerable<T> roots,
            Func<T, IEnumerable<T>?> childrenSelector,
            Func<T, bool> predicate)
        {
            foreach (var root in roots)
            {
                if (predicate(root))
                    return root;

                var children = childrenSelector(root);
                if (children != null)
                {
                    var found = Find(children, childrenSelector, predicate);
                    if (found != null)
                        return found;
                }
            }

            return default;
        }

        /// <summary>
        /// 查找所有匹配节点
        /// </summary>
        /// <typeparam name="T">节点类型</typeparam>
        /// <param name="roots">根节点列表</param>
        /// <param name="childrenSelector">子节点选择器</param>
        /// <param name="predicate">查找条件</param>
        /// <returns>找到的节点列表</returns>
        public static List<T> FindAll<T>(
            IEnumerable<T> roots,
            Func<T, IEnumerable<T>?> childrenSelector,
            Func<T, bool> predicate)
        {
            var result = new List<T>();
            FindAllInternal(roots, childrenSelector, predicate, result);
            return result;
        }

        private static void FindAllInternal<T>(
            IEnumerable<T> items,
            Func<T, IEnumerable<T>?> childrenSelector,
            Func<T, bool> predicate,
            List<T> result)
        {
            foreach (var item in items)
            {
                if (predicate(item))
                    result.Add(item);

                var children = childrenSelector(item);
                if (children != null)
                {
                    FindAllInternal(children, childrenSelector, predicate, result);
                }
            }
        }

        /// <summary>
        /// 获取节点路径
        /// </summary>
        /// <typeparam name="T">节点类型</typeparam>
        /// <typeparam name="TKey">键类型</typeparam>
        /// <param name="items">所有节点</param>
        /// <param name="targetId">目标节点ID</param>
        /// <param name="idSelector">ID选择器</param>
        /// <param name="parentIdSelector">父ID选择器</param>
        /// <returns>从根到目标的路径</returns>
        public static List<T> GetPath<T, TKey>(
            IEnumerable<T> items,
            TKey targetId,
            Func<T, TKey> idSelector,
            Func<T, TKey?> parentIdSelector)
            where TKey : notnull
        {
            var result = new List<T>();
            var lookup = items.ToDictionary(idSelector);

            if (!lookup.TryGetValue(targetId, out var current))
                return result;

            result.Add(current);

            while (true)
            {
                var parentId = parentIdSelector(current);
                if (parentId == null || EqualityComparer<TKey>.Default.Equals(parentId, default!))
                    break;

                if (!lookup.TryGetValue(parentId, out var parent))
                    break;

                result.Insert(0, parent);
                current = parent;
            }

            return result;
        }

        /// <summary>
        /// 计算树的深度
        /// </summary>
        /// <typeparam name="T">节点类型</typeparam>
        /// <param name="roots">根节点列表</param>
        /// <param name="childrenSelector">子节点选择器</param>
        /// <returns>最大深度</returns>
        public static int GetDepth<T>(
            IEnumerable<T> roots,
            Func<T, IEnumerable<T>?> childrenSelector)
        {
            var maxDepth = 0;

            Traverse(roots, childrenSelector, (_, level) =>
            {
                if (level > maxDepth)
                    maxDepth = level;
            });

            return maxDepth + 1;
        }

        /// <summary>
        /// 统计节点数量
        /// </summary>
        /// <typeparam name="T">节点类型</typeparam>
        /// <param name="roots">根节点列表</param>
        /// <param name="childrenSelector">子节点选择器</param>
        /// <returns>总节点数</returns>
        public static int Count<T>(
            IEnumerable<T> roots,
            Func<T, IEnumerable<T>?> childrenSelector)
        {
            var count = 0;
            Traverse(roots, childrenSelector, (_, _) => count++);
            return count;
        }
    }

    /// <summary>
    /// 树构建器
    /// </summary>
    /// <typeparam name="T">节点类型</typeparam>
    /// <typeparam name="TKey">键类型</typeparam>
    public class TreeBuilder<T, TKey>
        where T : class
        where TKey : notnull
    {
        private Func<T, TKey>? _idSelector;
        private Func<T, TKey?>? _parentIdSelector;
        private Action<T, List<T>>? _childrenSetter;
        private Func<T, bool>? _rootPredicate;

        /// <summary>
        /// 设置ID选择器
        /// </summary>
        public TreeBuilder<T, TKey> WithId(Func<T, TKey> selector)
        {
            _idSelector = selector;
            return this;
        }

        /// <summary>
        /// 设置父ID选择器
        /// </summary>
        public TreeBuilder<T, TKey> WithParentId(Func<T, TKey?> selector)
        {
            _parentIdSelector = selector;
            return this;
        }

        /// <summary>
        /// 设置子节点设置器
        /// </summary>
        public TreeBuilder<T, TKey> WithChildren(Action<T, List<T>> setter)
        {
            _childrenSetter = setter;
            return this;
        }

        /// <summary>
        /// 设置根节点判断条件
        /// </summary>
        public TreeBuilder<T, TKey> WithRootPredicate(Func<T, bool> predicate)
        {
            _rootPredicate = predicate;
            return this;
        }

        /// <summary>
        /// 构建树
        /// </summary>
        /// <param name="items">扁平列表</param>
        /// <returns>树形结构</returns>
        public List<T> Build(IEnumerable<T> items)
        {
            if (_idSelector == null)
                throw new InvalidOperationException("必须设置ID选择器");
            if (_parentIdSelector == null)
                throw new InvalidOperationException("必须设置父ID选择器");
            if (_childrenSetter == null)
                throw new InvalidOperationException("必须设置子节点设置器");

            return TreeBuildUtil.Build(items, _idSelector, _parentIdSelector, _childrenSetter, _rootPredicate);
        }
    }

    /// <summary>
    /// 树节点基类
    /// </summary>
    /// <typeparam name="T">节点类型</typeparam>
    public class TreeNode<T> where T : TreeNode<T>
    {
        /// <summary>
        /// 子节点列表
        /// </summary>
        public List<T> Children { get; set; } = new();

        /// <summary>
        /// 添加子节点
        /// </summary>
        public void AddChild(T child)
        {
            Children.Add(child);
        }

        /// <summary>
        /// 移除子节点
        /// </summary>
        public bool RemoveChild(T child)
        {
            return Children.Remove(child);
        }

        /// <summary>
        /// 是否为叶子节点
        /// </summary>
        public bool IsLeaf => Children.Count == 0;
    }
}
