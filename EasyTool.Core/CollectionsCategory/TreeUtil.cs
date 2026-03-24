using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyTool.CollectionsCategory
{
    /// <summary>
    /// 树工具类
    /// </summary>
    public static class TreeUtil
    {
        /// <summary>
        /// 创建通用树
        /// </summary>
        public static Tree<T> Create<T>(T value)
        {
            return new Tree<T>(value);
        }

        /// <summary>
        /// 从层次结构创建树
        /// </summary>
        public static Tree<T> FromHierarchy<T, TKey>(
            IEnumerable<T> items,
            Func<T, TKey> keySelector,
            Func<T, TKey> parentKeySelector,
            TKey rootParentKey = default) where TKey : IEquatable<TKey>
        {
            var itemDict = items.ToDictionary(keySelector);
            var childrenDict = items.GroupBy(parentKeySelector).ToDictionary(g => g.Key, g => g.ToList());

            T rootItem;
            if (rootParentKey == null || rootParentKey.Equals(default))
            {
                rootItem = items.FirstOrDefault(i => parentKeySelector(i) == null || parentKeySelector(i).Equals(default));
            }
            else
            {
                rootItem = items.FirstOrDefault(i => parentKeySelector(i).Equals(rootParentKey));
            }

            if (rootItem == null)
                throw new ArgumentException("Cannot find root item");

            var root = new Tree<T>(rootItem);
            BuildTree(root, keySelector(rootItem), keySelector, childrenDict);
            return root;
        }

        private static void BuildTree<T, TKey>(
            Tree<T> parent,
            TKey parentKey,
            Func<T, TKey> keySelector,
            Dictionary<TKey, List<T>> childrenDict) where TKey : IEquatable<TKey>
        {
            if (!childrenDict.TryGetValue(parentKey, out var children))
                return;

            foreach (var child in children)
            {
                var childNode = parent.AddChild(child);
                BuildTree(childNode, keySelector(child), keySelector, childrenDict);
            }
        }
    }

    /// <summary>
    /// 通用树节点
    /// </summary>
    public class Tree<T>
    {
        private readonly List<Tree<T>> _children;

        /// <summary>
        /// 节点值
        /// </summary>
        public T Value { get; set; }

        /// <summary>
        /// 父节点
        /// </summary>
        public Tree<T> Parent { get; private set; }

        /// <summary>
        /// 子节点
        /// </summary>
        public IReadOnlyList<Tree<T>> Children => _children;

        /// <summary>
        /// 深度
        /// </summary>
        public int Depth
        {
            get
            {
                int depth = 0;
                var current = Parent;
                while (current != null)
                {
                    depth++;
                    current = current.Parent;
                }
                return depth;
            }
        }

        /// <summary>
        /// 高度
        /// </summary>
        public int Height
        {
            get
            {
                if (_children.Count == 0)
                    return 0;
                return 1 + _children.Max(c => c.Height);
            }
        }

        /// <summary>
        /// 是否为根节点
        /// </summary>
        public bool IsRoot => Parent == null;

        /// <summary>
        /// 是否为叶节点
        /// </summary>
        public bool IsLeaf => _children.Count == 0;

        /// <summary>
        /// 子节点数量
        /// </summary>
        public int ChildCount => _children.Count;

        /// <summary>
        /// 创建树节点
        /// </summary>
        public Tree(T value)
        {
            Value = value;
            _children = new List<Tree<T>>();
        }

        /// <summary>
        /// 添加子节点
        /// </summary>
        public Tree<T> AddChild(T value)
        {
            var child = new Tree<T>(value) { Parent = this };
            _children.Add(child);
            return child;
        }

        /// <summary>
        /// 添加子节点
        /// </summary>
        public void AddChild(Tree<T> child)
        {
            child.Parent = this;
            _children.Add(child);
        }

        /// <summary>
        /// 移除子节点
        /// </summary>
        public bool RemoveChild(Tree<T> child)
        {
            if (_children.Remove(child))
            {
                child.Parent = null;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 清空子节点
        /// </summary>
        public void ClearChildren()
        {
            foreach (var child in _children)
            {
                child.Parent = null;
            }
            _children.Clear();
        }

        /// <summary>
        /// 获取根节点
        /// </summary>
        public Tree<T> GetRoot()
        {
            var current = this;
            while (current.Parent != null)
            {
                current = current.Parent;
            }
            return current;
        }

        /// <summary>
        /// 获取所有祖先
        /// </summary>
        public IEnumerable<Tree<T>> GetAncestors()
        {
            var current = Parent;
            while (current != null)
            {
                yield return current;
                current = current.Parent;
            }
        }

        /// <summary>
        /// 获取所有后代
        /// </summary>
        public IEnumerable<Tree<T>> GetDescendants()
        {
            foreach (var child in _children)
            {
                yield return child;
                foreach (var descendant in child.GetDescendants())
                {
                    yield return descendant;
                }
            }
        }

        /// <summary>
        /// 前序遍历
        /// </summary>
        public IEnumerable<Tree<T>> PreOrderTraversal()
        {
            yield return this;
            foreach (var child in _children)
            {
                foreach (var node in child.PreOrderTraversal())
                {
                    yield return node;
                }
            }
        }

        /// <summary>
        /// 后序遍历
        /// </summary>
        public IEnumerable<Tree<T>> PostOrderTraversal()
        {
            foreach (var child in _children)
            {
                foreach (var node in child.PostOrderTraversal())
                {
                    yield return node;
                }
            }
            yield return this;
        }

        /// <summary>
        /// 层序遍历（广度优先）
        /// </summary>
        public IEnumerable<Tree<T>> LevelOrderTraversal()
        {
            var queue = new Queue<Tree<T>>();
            queue.Enqueue(this);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                yield return current;

                foreach (var child in current._children)
                {
                    queue.Enqueue(child);
                }
            }
        }

        /// <summary>
        /// 查找节点
        /// </summary>
        public Tree<T> Find(Func<T, bool> predicate)
        {
            if (predicate(Value))
                return this;

            foreach (var child in _children)
            {
                var found = child.Find(predicate);
                if (found != null)
                    return found;
            }

            return null;
        }

        /// <summary>
        /// 查找所有匹配节点
        /// </summary>
        public IEnumerable<Tree<T>> FindAll(Func<T, bool> predicate)
        {
            if (predicate(Value))
                yield return this;

            foreach (var child in _children)
            {
                foreach (var found in child.FindAll(predicate))
                {
                    yield return found;
                }
            }
        }

        /// <summary>
        /// 获取路径
        /// </summary>
        public List<Tree<T>> GetPath()
        {
            var path = new List<Tree<T>>();
            var current = this;
            while (current != null)
            {
                path.Insert(0, current);
                current = current.Parent;
            }
            return path;
        }
    }

    /// <summary>
    /// 二叉树工具类
    /// </summary>
    public static class BinaryTreeUtil
    {
        /// <summary>
        /// 创建二叉树
        /// </summary>
        public static BinaryTree<T> Create<T>(T value)
        {
            return new BinaryTree<T>(value);
        }

        /// <summary>
        /// 从层序数组创建完全二叉树
        /// </summary>
        public static BinaryTree<T> FromArray<T>(T[] values) where T : class
        {
            if (values == null || values.Length == 0 || values[0] == null)
                return null;

            var root = new BinaryTree<T>(values[0]);
            var queue = new Queue<BinaryTree<T>>();
            queue.Enqueue(root);

            int i = 1;
            while (queue.Count > 0 && i < values.Length)
            {
                var current = queue.Dequeue();

                if (i < values.Length && values[i] != null)
                {
                    current.Left = new BinaryTree<T>(values[i]) { Parent = current };
                    queue.Enqueue(current.Left);
                }
                i++;

                if (i < values.Length && values[i] != null)
                {
                    current.Right = new BinaryTree<T>(values[i]) { Parent = current };
                    queue.Enqueue(current.Right);
                }
                i++;
            }

            return root;
        }
    }

    /// <summary>
    /// 二叉树节点
    /// </summary>
    public class BinaryTree<T>
    {
        /// <summary>
        /// 节点值
        /// </summary>
        public T Value { get; set; }

        /// <summary>
        /// 左子节点
        /// </summary>
        public BinaryTree<T> Left { get; set; }

        /// <summary>
        /// 右子节点
        /// </summary>
        public BinaryTree<T> Right { get; set; }

        /// <summary>
        /// 父节点
        /// </summary>
        public BinaryTree<T> Parent { get; set; }

        /// <summary>
        /// 是否为叶节点
        /// </summary>
        public bool IsLeaf => Left == null && Right == null;

        /// <summary>
        /// 是否为根节点
        /// </summary>
        public bool IsRoot => Parent == null;

        /// <summary>
        /// 高度
        /// </summary>
        public int Height
        {
            get
            {
                int leftHeight = Left?.Height ?? 0;
                int rightHeight = Right?.Height ?? 0;
                return 1 + Math.Max(leftHeight, rightHeight);
            }
        }

        /// <summary>
        /// 节点数量
        /// </summary>
        public int NodeCount
        {
            get
            {
                int count = 1;
                if (Left != null) count += Left.NodeCount;
                if (Right != null) count += Right.NodeCount;
                return count;
            }
        }

        /// <summary>
        /// 创建二叉树节点
        /// </summary>
        public BinaryTree(T value)
        {
            Value = value;
        }

        /// <summary>
        /// 前序遍历
        /// </summary>
        public IEnumerable<BinaryTree<T>> PreOrderTraversal()
        {
            yield return this;
            if (Left != null)
            {
                foreach (var node in Left.PreOrderTraversal())
                    yield return node;
            }
            if (Right != null)
            {
                foreach (var node in Right.PreOrderTraversal())
                    yield return node;
            }
        }

        /// <summary>
        /// 中序遍历
        /// </summary>
        public IEnumerable<BinaryTree<T>> InOrderTraversal()
        {
            if (Left != null)
            {
                foreach (var node in Left.InOrderTraversal())
                    yield return node;
            }
            yield return this;
            if (Right != null)
            {
                foreach (var node in Right.InOrderTraversal())
                    yield return node;
            }
        }

        /// <summary>
        /// 后序遍历
        /// </summary>
        public IEnumerable<BinaryTree<T>> PostOrderTraversal()
        {
            if (Left != null)
            {
                foreach (var node in Left.PostOrderTraversal())
                    yield return node;
            }
            if (Right != null)
            {
                foreach (var node in Right.PostOrderTraversal())
                    yield return node;
            }
            yield return this;
        }

        /// <summary>
        /// 层序遍历
        /// </summary>
        public IEnumerable<BinaryTree<T>> LevelOrderTraversal()
        {
            var queue = new Queue<BinaryTree<T>>();
            queue.Enqueue(this);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                yield return current;

                if (current.Left != null)
                    queue.Enqueue(current.Left);
                if (current.Right != null)
                    queue.Enqueue(current.Right);
            }
        }

        /// <summary>
        /// 反转二叉树
        /// </summary>
        public void Invert()
        {
            var temp = Left;
            Left = Right;
            Right = temp;

            Left?.Invert();
            Right?.Invert();
        }

        /// <summary>
        /// 克隆
        /// </summary>
        public BinaryTree<T> Clone()
        {
            var clone = new BinaryTree<T>(Value);
            if (Left != null)
            {
                clone.Left = Left.Clone();
                clone.Left.Parent = clone;
            }
            if (Right != null)
            {
                clone.Right = Right.Clone();
                clone.Right.Parent = clone;
            }
            return clone;
        }

        /// <summary>
        /// 获取指定深度的所有节点
        /// </summary>
        public List<BinaryTree<T>> GetNodesAtDepth(int depth)
        {
            var result = new List<BinaryTree<T>>();
            GetNodesAtDepth(this, depth, 0, result);
            return result;
        }

        private static void GetNodesAtDepth(BinaryTree<T> node, int targetDepth, int currentDepth, List<BinaryTree<T>> result)
        {
            if (node == null)
                return;

            if (currentDepth == targetDepth)
            {
                result.Add(node);
                return;
            }

            GetNodesAtDepth(node.Left, targetDepth, currentDepth + 1, result);
            GetNodesAtDepth(node.Right, targetDepth, currentDepth + 1, result);
        }
    }

    /// <summary>
    /// 二叉搜索树工具类
    /// </summary>
    public static class BinarySearchTreeUtil
    {
        /// <summary>
        /// 创建二叉搜索树
        /// </summary>
        public static BinarySearchTree<T> Create<T>() where T : IComparable<T>
        {
            return new BinarySearchTree<T>();
        }

        /// <summary>
        /// 从集合创建二叉搜索树
        /// </summary>
        public static BinarySearchTree<T> FromEnumerable<T>(IEnumerable<T> items) where T : IComparable<T>
        {
            var bst = new BinarySearchTree<T>();
            foreach (var item in items)
            {
                bst.Add(item);
            }
            return bst;
        }
    }

    /// <summary>
    /// 二叉搜索树
    /// </summary>
    public class BinarySearchTree<T> where T : IComparable<T>
    {
        private BSTNode _root;
        private int _count;

        /// <summary>
        /// 节点数量
        /// </summary>
        public int Count => _count;

        /// <summary>
        /// 是否为空
        /// </summary>
        public bool IsEmpty => _count == 0;

        /// <summary>
        /// 最小值
        /// </summary>
        public T Min
        {
            get
            {
                if (_root == null)
                    throw new InvalidOperationException("Tree is empty");
                return FindMin(_root).Value;
            }
        }

        /// <summary>
        /// 最大值
        /// </summary>
        public T Max
        {
            get
            {
                if (_root == null)
                    throw new InvalidOperationException("Tree is empty");
                return FindMax(_root).Value;
            }
        }

        private class BSTNode
        {
            public T Value { get; set; }
            public BSTNode Left { get; set; }
            public BSTNode Right { get; set; }

            public BSTNode(T value)
            {
                Value = value;
            }
        }

        /// <summary>
        /// 添加元素
        /// </summary>
        public void Add(T value)
        {
            _root = Add(_root, value);
        }

        private BSTNode Add(BSTNode node, T value)
        {
            if (node == null)
            {
                _count++;
                return new BSTNode(value);
            }

            int cmp = value.CompareTo(node.Value);
            if (cmp < 0)
                node.Left = Add(node.Left, value);
            else if (cmp > 0)
                node.Right = Add(node.Right, value);

            return node;
        }

        /// <summary>
        /// 是否包含元素
        /// </summary>
        public bool Contains(T value)
        {
            return Find(_root, value) != null;
        }

        private BSTNode Find(BSTNode node, T value)
        {
            if (node == null)
                return null;

            int cmp = value.CompareTo(node.Value);
            if (cmp < 0)
                return Find(node.Left, value);
            if (cmp > 0)
                return Find(node.Right, value);
            return node;
        }

        /// <summary>
        /// 移除元素
        /// </summary>
        public bool Remove(T value)
        {
            int oldCount = _count;
            _root = Remove(_root, value);
            return _count < oldCount;
        }

        private BSTNode Remove(BSTNode node, T value)
        {
            if (node == null)
                return null;

            int cmp = value.CompareTo(node.Value);
            if (cmp < 0)
            {
                node.Left = Remove(node.Left, value);
            }
            else if (cmp > 0)
            {
                node.Right = Remove(node.Right, value);
            }
            else
            {
                _count--;
                if (node.Left == null)
                    return node.Right;
                if (node.Right == null)
                    return node.Left;

                // 两个子节点都存在，用后继节点替换
                var successor = FindMin(node.Right);
                node.Value = successor.Value;
                node.Right = Remove(node.Right, successor.Value);
                _count++; // 因为上面递归会再次减
            }

            return node;
        }

        private BSTNode FindMin(BSTNode node)
        {
            while (node.Left != null)
                node = node.Left;
            return node;
        }

        private BSTNode FindMax(BSTNode node)
        {
            while (node.Right != null)
                node = node.Right;
            return node;
        }

        /// <summary>
        /// 中序遍历
        /// </summary>
        public IEnumerable<T> InOrderTraversal()
        {
            return InOrderTraversal(_root);
        }

        private IEnumerable<T> InOrderTraversal(BSTNode node)
        {
            if (node == null)
                yield break;

            foreach (var value in InOrderTraversal(node.Left))
                yield return value;

            yield return node.Value;

            foreach (var value in InOrderTraversal(node.Right))
                yield return value;
        }

        /// <summary>
        /// 清空
        /// </summary>
        public void Clear()
        {
            _root = null;
            _count = 0;
        }

        /// <summary>
        /// 查找小于指定值的最大元素
        /// </summary>
        public T? Floor(T value)
        {
            var node = Floor(_root, value);
            return node == null ? default : node.Value;
        }

        private BSTNode Floor(BSTNode node, T value)
        {
            if (node == null)
                return null;

            int cmp = value.CompareTo(node.Value);
            if (cmp == 0)
                return node;
            if (cmp < 0)
                return Floor(node.Left, value);

            var rightFloor = Floor(node.Right, value);
            return rightFloor ?? node;
        }

        /// <summary>
        /// 查找大于指定值的最小元素
        /// </summary>
        public T? Ceiling(T value)
        {
            var node = Ceiling(_root, value);
            return node == null ? default : node.Value;
        }

        private BSTNode Ceiling(BSTNode node, T value)
        {
            if (node == null)
                return null;

            int cmp = value.CompareTo(node.Value);
            if (cmp == 0)
                return node;
            if (cmp > 0)
                return Ceiling(node.Right, value);

            var leftCeiling = Ceiling(node.Left, value);
            return leftCeiling ?? node;
        }

        /// <summary>
        /// 获取排名（小于指定值的元素数量）
        /// </summary>
        public int Rank(T value)
        {
            return Rank(_root, value);
        }

        private int Rank(BSTNode node, T value)
        {
            if (node == null)
                return 0;

            int cmp = value.CompareTo(node.Value);
            if (cmp < 0)
                return Rank(node.Left, value);
            if (cmp > 0)
                return 1 + CountNodes(node.Left) + Rank(node.Right, value);
            return CountNodes(node.Left);
        }

        private int CountNodes(BSTNode node)
        {
            if (node == null)
                return 0;
            return 1 + CountNodes(node.Left) + CountNodes(node.Right);
        }
    }

    /// <summary>
    /// 线段树工具类
    /// </summary>
    public static class SegmentTreeUtil
    {
        /// <summary>
        /// 创建线段树（求和）
        /// </summary>
        public static SegmentTree Create(int[] values)
        {
            return new SegmentTree(values, (a, b) => a + b, 0);
        }

        /// <summary>
        /// 创建线段树（自定义操作）
        /// </summary>
        public static SegmentTree Create(int[] values, Func<int, int, int> operation, int identity)
        {
            return new SegmentTree(values, operation, identity);
        }
    }

    /// <summary>
    /// 线段树（区间查询/更新）
    /// </summary>
    public class SegmentTree
    {
        private readonly int[] _tree;
        private readonly int[] _lazy;
        private readonly int _n;
        private readonly Func<int, int, int> _operation;
        private readonly int _identity;

        /// <summary>
        /// 元素数量
        /// </summary>
        public int Count => _n;

        /// <summary>
        /// 创建线段树
        /// </summary>
        public SegmentTree(int[] values, Func<int, int, int> operation, int identity)
        {
            if (values == null || values.Length == 0)
                throw new ArgumentException("Values cannot be null or empty");

            _n = values.Length;
            _operation = operation;
            _identity = identity;
            _tree = new int[4 * _n];
            _lazy = new int[4 * _n];

            Build(values, 1, 0, _n - 1);
        }

        private void Build(int[] values, int node, int start, int end)
        {
            if (start == end)
            {
                _tree[node] = values[start];
            }
            else
            {
                int mid = (start + end) / 2;
                Build(values, 2 * node, start, mid);
                Build(values, 2 * node + 1, mid + 1, end);
                _tree[node] = _operation(_tree[2 * node], _tree[2 * node + 1]);
            }
        }

        /// <summary>
        /// 区间查询
        /// </summary>
        public int Query(int left, int right)
        {
            if (left < 0 || right >= _n || left > right)
                throw new ArgumentOutOfRangeException();
            return Query(1, 0, _n - 1, left, right);
        }

        private int Query(int node, int start, int end, int left, int right)
        {
            if (right < start || left > end)
                return _identity;

            if (left <= start && end <= right)
                return _tree[node];

            int mid = (start + end) / 2;
            int leftResult = Query(2 * node, start, mid, left, right);
            int rightResult = Query(2 * node + 1, mid + 1, end, left, right);
            return _operation(leftResult, rightResult);
        }

        /// <summary>
        /// 单点更新
        /// </summary>
        public void Update(int index, int value)
        {
            if (index < 0 || index >= _n)
                throw new ArgumentOutOfRangeException(nameof(index));
            Update(1, 0, _n - 1, index, value);
        }

        private void Update(int node, int start, int end, int index, int value)
        {
            if (start == end)
            {
                _tree[node] = value;
            }
            else
            {
                int mid = (start + end) / 2;
                if (index <= mid)
                    Update(2 * node, start, mid, index, value);
                else
                    Update(2 * node + 1, mid + 1, end, index, value);
                _tree[node] = _operation(_tree[2 * node], _tree[2 * node + 1]);
            }
        }

        /// <summary>
        /// 获取单个值
        /// </summary>
        public int Get(int index)
        {
            return Query(index, index);
        }
    }

    /// <summary>
    /// AVL树工具类
    /// </summary>
    public static class AVLTreeUtil
    {
        /// <summary>
        /// 创建AVL树
        /// </summary>
        public static AVLTree<T> Create<T>() where T : IComparable<T>
        {
            return new AVLTree<T>();
        }
    }

    /// <summary>
    /// AVL树（自平衡二叉搜索树）
    /// </summary>
    public class AVLTree<T> where T : IComparable<T>
    {
        private AVLNode _root;
        private int _count;

        private class AVLNode
        {
            public T Value { get; set; }
            public AVLNode Left { get; set; }
            public AVLNode Right { get; set; }
            public int Height { get; set; }

            public AVLNode(T value)
            {
                Value = value;
                Height = 1;
            }

            public int BalanceFactor => GetHeight(Left) - GetHeight(Right);

            private static int GetHeight(AVLNode node) => node?.Height ?? 0;
        }

        /// <summary>
        /// 节点数量
        /// </summary>
        public int Count => _count;

        /// <summary>
        /// 是否为空
        /// </summary>
        public bool IsEmpty => _count == 0;

        /// <summary>
        /// 添加元素
        /// </summary>
        public void Add(T value)
        {
            _root = Add(_root, value);
        }

        private AVLNode Add(AVLNode node, T value)
        {
            if (node == null)
            {
                _count++;
                return new AVLNode(value);
            }

            int cmp = value.CompareTo(node.Value);
            if (cmp < 0)
                node.Left = Add(node.Left, value);
            else if (cmp > 0)
                node.Right = Add(node.Right, value);
            else
                return node; // 重复值不添加

            UpdateHeight(node);
            return Balance(node);
        }

        /// <summary>
        /// 是否包含元素
        /// </summary>
        public bool Contains(T value)
        {
            return Contains(_root, value);
        }

        private bool Contains(AVLNode node, T value)
        {
            if (node == null)
                return false;

            int cmp = value.CompareTo(node.Value);
            if (cmp < 0)
                return Contains(node.Left, value);
            if (cmp > 0)
                return Contains(node.Right, value);
            return true;
        }

        /// <summary>
        /// 移除元素
        /// </summary>
        public bool Remove(T value)
        {
            int oldCount = _count;
            _root = Remove(_root, value);
            return _count < oldCount;
        }

        private AVLNode Remove(AVLNode node, T value)
        {
            if (node == null)
                return null;

            int cmp = value.CompareTo(node.Value);
            if (cmp < 0)
            {
                node.Left = Remove(node.Left, value);
            }
            else if (cmp > 0)
            {
                node.Right = Remove(node.Right, value);
            }
            else
            {
                _count--;
                if (node.Left == null)
                    return node.Right;
                if (node.Right == null)
                    return node.Left;

                var successor = FindMin(node.Right);
                node.Value = successor.Value;
                node.Right = Remove(node.Right, successor.Value);
                _count++;
            }

            UpdateHeight(node);
            return Balance(node);
        }

        private AVLNode FindMin(AVLNode node)
        {
            while (node.Left != null)
                node = node.Left;
            return node;
        }

        private void UpdateHeight(AVLNode node)
        {
            int leftHeight = node.Left?.Height ?? 0;
            int rightHeight = node.Right?.Height ?? 0;
            node.Height = 1 + Math.Max(leftHeight, rightHeight);
        }

        private AVLNode Balance(AVLNode node)
        {
            int balance = node.BalanceFactor;

            // 左重
            if (balance > 1)
            {
                if (node.Left.BalanceFactor < 0)
                    node.Left = RotateLeft(node.Left);
                return RotateRight(node);
            }

            // 右重
            if (balance < -1)
            {
                if (node.Right.BalanceFactor > 0)
                    node.Right = RotateRight(node.Right);
                return RotateLeft(node);
            }

            return node;
        }

        private AVLNode RotateRight(AVLNode y)
        {
            var x = y.Left;
            y.Left = x.Right;
            x.Right = y;

            UpdateHeight(y);
            UpdateHeight(x);

            return x;
        }

        private AVLNode RotateLeft(AVLNode x)
        {
            var y = x.Right;
            x.Right = y.Left;
            y.Left = x;

            UpdateHeight(x);
            UpdateHeight(y);

            return y;
        }

        /// <summary>
        /// 中序遍历
        /// </summary>
        public IEnumerable<T> InOrderTraversal()
        {
            return InOrderTraversal(_root);
        }

        private IEnumerable<T> InOrderTraversal(AVLNode node)
        {
            if (node == null)
                yield break;

            foreach (var value in InOrderTraversal(node.Left))
                yield return value;

            yield return node.Value;

            foreach (var value in InOrderTraversal(node.Right))
                yield return value;
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
}
