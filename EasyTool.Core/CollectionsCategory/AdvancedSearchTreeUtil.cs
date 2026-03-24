using System;
using System.Collections.Generic;

namespace EasyTool.CollectionsCategory
{
    /// <summary>
    /// 高级搜索树工具类
    /// </summary>
    public static class AdvancedSearchTreeUtil
    {
        /// <summary>
        /// 创建树堆（Treap）
        /// </summary>
        public static Treap<T> CreateTreap<T>() where T : IComparable<T>
        {
            return new Treap<T>();
        }

        /// <summary>
        /// 创建伸展树（Splay Tree）
        /// </summary>
        public static SplayTree<T> CreateSplayTree<T>() where T : IComparable<T>
        {
            return new SplayTree<T>();
        }

        /// <summary>
        /// 创建后缀数组
        /// </summary>
        public static SuffixArray CreateSuffixArray(string text)
        {
            return new SuffixArray(text);
        }
    }

    #region Treap（树堆）

    /// <summary>
    /// 树堆（Treap）
    /// 结合二叉搜索树和堆的性质，通过随机优先级保持平衡
    /// </summary>
    public class Treap<T> where T : IComparable<T>
    {
        private class Node
        {
            public T Value { get; set; }
            public int Priority { get; set; }
            public Node Left { get; set; }
            public Node Right { get; set; }
            public int Count { get; set; } // 子树大小
            public int Size => 1 + (Left?.Size ?? 0) + (Right?.Size ?? 0);

            public Node(T value, int priority)
            {
                Value = value;
                Priority = priority;
                Count = 1;
            }
        }

        private Node _root;
        private readonly Random _random;
        private int _count;

        /// <summary>
        /// 元素数量
        /// </summary>
        public int Count => _count;

        /// <summary>
        /// 是否为空
        /// </summary>
        public bool IsEmpty => _count == 0;

        /// <summary>
        /// 创建树堆
        /// </summary>
        public Treap()
        {
            _random = new Random();
            _root = null;
            _count = 0;
        }

        /// <summary>
        /// 插入元素
        /// </summary>
        public void Insert(T value)
        {
            _root = Insert(_root, value, _random.Next());
            _count++;
        }

        private Node Insert(Node node, T value, int priority)
        {
            if (node == null)
                return new Node(value, priority);

            int cmp = value.CompareTo(node.Value);
            if (cmp < 0)
            {
                node.Left = Insert(node.Left, value, priority);
                if (node.Left.Priority > node.Priority)
                    node = RotateRight(node);
            }
            else if (cmp > 0)
            {
                node.Right = Insert(node.Right, value, priority);
                if (node.Right.Priority > node.Priority)
                    node = RotateLeft(node);
            }

            return node;
        }

        /// <summary>
        /// 删除元素
        /// </summary>
        public bool Remove(T value)
        {
            if (!Contains(value)) return false;
            _root = Remove(_root, value);
            _count--;
            return true;
        }

        private Node Remove(Node node, T value)
        {
            if (node == null) return null;

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
                if (node.Left == null) return node.Right;
                if (node.Right == null) return node.Left;

                if (node.Left.Priority > node.Right.Priority)
                {
                    node = RotateRight(node);
                    node.Right = Remove(node.Right, value);
                }
                else
                {
                    node = RotateLeft(node);
                    node.Left = Remove(node.Left, value);
                }
            }

            return node;
        }

        /// <summary>
        /// 是否包含元素
        /// </summary>
        public bool Contains(T value)
        {
            return Contains(_root, value);
        }

        private bool Contains(Node node, T value)
        {
            if (node == null) return false;
            int cmp = value.CompareTo(node.Value);
            if (cmp < 0) return Contains(node.Left, value);
            if (cmp > 0) return Contains(node.Right, value);
            return true;
        }

        /// <summary>
        /// 查找第k小元素
        /// </summary>
        public T FindKth(int k)
        {
            if (k < 0 || k >= _count)
                throw new ArgumentOutOfRangeException(nameof(k));
            return FindKth(_root, k);
        }

        private T FindKth(Node node, int k)
        {
            int leftSize = node.Left?.Size ?? 0;
            if (k < leftSize)
                return FindKth(node.Left, k);
            if (k > leftSize)
                return FindKth(node.Right, k - leftSize - 1);
            return node.Value;
        }

        /// <summary>
        /// 获取元素的排名（从0开始）
        /// </summary>
        public int Rank(T value)
        {
            return Rank(_root, value);
        }

        private int Rank(Node node, T value)
        {
            if (node == null) return 0;
            int cmp = value.CompareTo(node.Value);
            int leftSize = node.Left?.Size ?? 0;
            if (cmp < 0)
                return Rank(node.Left, value);
            if (cmp > 0)
                return leftSize + 1 + Rank(node.Right, value);
            return leftSize;
        }

        /// <summary>
        /// 获取最小值
        /// </summary>
        public T Min()
        {
            if (_root == null) throw new InvalidOperationException("Treap is empty");
            var node = _root;
            while (node.Left != null) node = node.Left;
            return node.Value;
        }

        /// <summary>
        /// 获取最大值
        /// </summary>
        public T Max()
        {
            if (_root == null) throw new InvalidOperationException("Treap is empty");
            var node = _root;
            while (node.Right != null) node = node.Right;
            return node.Value;
        }

        private static Node RotateRight(Node node)
        {
            var left = node.Left;
            node.Left = left.Right;
            left.Right = node;
            return left;
        }

        private static Node RotateLeft(Node node)
        {
            var right = node.Right;
            node.Right = right.Left;
            right.Left = node;
            return right;
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

    #endregion

    #region SplayTree（伸展树）

    /// <summary>
    /// 伸展树（Splay Tree）
    /// 自调整二叉搜索树，通过伸展操作将访问的节点移到根
    /// </summary>
    public class SplayTree<T> where T : IComparable<T>
    {
        private class Node
        {
            public T Value { get; set; }
            public Node Left { get; set; }
            public Node Right { get; set; }
            public Node Parent { get; set; }

            public Node(T value)
            {
                Value = value;
            }
        }

        private Node _root;
        private int _count;

        /// <summary>
        /// 元素数量
        /// </summary>
        public int Count => _count;

        /// <summary>
        /// 是否为空
        /// </summary>
        public bool IsEmpty => _count == 0;

        /// <summary>
        /// 创建伸展树
        /// </summary>
        public SplayTree()
        {
            _root = null;
            _count = 0;
        }

        /// <summary>
        /// 插入元素
        /// </summary>
        public void Insert(T value)
        {
            if (_root == null)
            {
                _root = new Node(value);
                _count++;
                return;
            }

            Splay(value);

            int cmp = value.CompareTo(_root.Value);
            if (cmp == 0) return; // 已存在

            var newNode = new Node(value);
            _count++;

            if (cmp < 0)
            {
                newNode.Left = _root.Left;
                newNode.Right = _root;
                _root.Left = null;
            }
            else
            {
                newNode.Right = _root.Right;
                newNode.Left = _root;
                _root.Right = null;
            }

            if (newNode.Left != null) newNode.Left.Parent = newNode;
            if (newNode.Right != null) newNode.Right.Parent = newNode;
            _root = newNode;
        }

        /// <summary>
        /// 删除元素
        /// </summary>
        public bool Remove(T value)
        {
            if (_root == null) return false;

            Splay(value);

            if (value.CompareTo(_root.Value) != 0) return false;

            if (_root.Left == null)
            {
                _root = _root.Right;
                if (_root != null) _root.Parent = null;
            }
            else
            {
                var rightTree = _root.Right;
                _root = _root.Left;
                _root.Parent = null;

                // 将左子树的最大值伸展到根
                Splay(value);
                _root.Right = rightTree;
                if (rightTree != null) rightTree.Parent = _root;
            }

            _count--;
            return true;
        }

        /// <summary>
        /// 是否包含元素
        /// </summary>
        public bool Contains(T value)
        {
            if (_root == null) return false;

            Splay(value);
            return value.CompareTo(_root.Value) == 0;
        }

        /// <summary>
        /// 查找元素（会将其伸展到根）
        /// </summary>
        public T Find(T value)
        {
            if (!Contains(value))
                throw new KeyNotFoundException("Value not found");
            return _root.Value;
        }

        /// <summary>
        /// 获取最小值
        /// </summary>
        public T Min()
        {
            if (_root == null) throw new InvalidOperationException("Tree is empty");
            var node = _root;
            while (node.Left != null) node = node.Left;
            Splay(node.Value);
            return _root.Value;
        }

        /// <summary>
        /// 获取最大值
        /// </summary>
        public T Max()
        {
            if (_root == null) throw new InvalidOperationException("Tree is empty");
            var node = _root;
            while (node.Right != null) node = node.Right;
            Splay(node.Value);
            return _root.Value;
        }

        private void Splay(T value)
        {
            var node = FindNode(value);
            if (node == null) return;

            while (node.Parent != null)
            {
                var parent = node.Parent;
                var grandparent = parent.Parent;

                if (grandparent == null)
                {
                    // Zig 或 Zag
                    if (node == parent.Left)
                        RotateRight(parent);
                    else
                        RotateLeft(parent);
                }
                else if (node == parent.Left && parent == grandparent.Left)
                {
                    // Zig-Zig
                    RotateRight(grandparent);
                    RotateRight(parent);
                }
                else if (node == parent.Right && parent == grandparent.Right)
                {
                    // Zag-Zag
                    RotateLeft(grandparent);
                    RotateLeft(parent);
                }
                else if (node == parent.Right && parent == grandparent.Left)
                {
                    // Zig-Zag
                    RotateLeft(parent);
                    RotateRight(grandparent);
                }
                else
                {
                    // Zag-Zig
                    RotateRight(parent);
                    RotateLeft(grandparent);
                }
            }

            _root = node;
        }

        private Node FindNode(T value)
        {
            var node = _root;
            while (node != null)
            {
                int cmp = value.CompareTo(node.Value);
                if (cmp < 0) node = node.Left;
                else if (cmp > 0) node = node.Right;
                else return node;
            }
            return null;
        }

        private void RotateLeft(Node node)
        {
            var right = node.Right;
            node.Right = right.Left;
            if (right.Left != null) right.Left.Parent = node;
            right.Parent = node.Parent;
            if (node.Parent == null) _root = right;
            else if (node == node.Parent.Left) node.Parent.Left = right;
            else node.Parent.Right = right;
            right.Left = node;
            node.Parent = right;
        }

        private void RotateRight(Node node)
        {
            var left = node.Left;
            node.Left = left.Right;
            if (left.Right != null) left.Right.Parent = node;
            left.Parent = node.Parent;
            if (node.Parent == null) _root = left;
            else if (node == node.Parent.Left) node.Parent.Left = left;
            else node.Parent.Right = left;
            left.Right = node;
            node.Parent = left;
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

    #endregion

    #region SuffixArray（后缀数组）

    /// <summary>
    /// 后缀数组
    /// 用于字符串处理的高效数据结构
    /// </summary>
    public class SuffixArray
    {
        private readonly string _text;
        private readonly int[] _suffixArray;
        private readonly int[] _lcpArray;

        /// <summary>
        /// 原始文本
        /// </summary>
        public string Text => _text;

        /// <summary>
        /// 文本长度
        /// </summary>
        public int Length => _text.Length;

        /// <summary>
        /// 获取后缀数组
        /// </summary>
        public int[] SuffixArrayValue => _suffixArray;

        /// <summary>
        /// 获取LCP数组（最长公共前缀）
        /// </summary>
        public int[] LCPArray => _lcpArray;

        /// <summary>
        /// 创建后缀数组
        /// </summary>
        public SuffixArray(string text)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            _text = text;
            _suffixArray = BuildSuffixArray(text);
            _lcpArray = BuildLCPArray(text, _suffixArray);
        }

        /// <summary>
        /// 查找模式串所有出现位置
        /// </summary>
        public List<int> Search(string pattern)
        {
            var result = new List<int>();
            if (string.IsNullOrEmpty(pattern) || pattern.Length > _text.Length)
                return result;

            int left = 0, right = _text.Length - 1;
            while (left <= right)
            {
                int mid = (left + right) / 2;
                int cmp = Compare(pattern, _suffixArray[mid]);
                if (cmp < 0) right = mid - 1;
                else if (cmp > 0) left = mid + 1;
                else
                {
                    // 找到匹配，向两边扩展
                    int i = mid;
                    while (i >= 0 && Compare(pattern, _suffixArray[i]) == 0)
                    {
                        result.Add(_suffixArray[i]);
                        i--;
                    }
                    i = mid + 1;
                    while (i < _text.Length && Compare(pattern, _suffixArray[i]) == 0)
                    {
                        result.Add(_suffixArray[i]);
                        i++;
                    }
                    break;
                }
            }

            result.Sort();
            return result;
        }

        /// <summary>
        /// 检查是否包含模式串
        /// </summary>
        public bool Contains(string pattern)
        {
            if (string.IsNullOrEmpty(pattern)) return true;
            if (pattern.Length > _text.Length) return false;

            int left = 0, right = _text.Length - 1;
            while (left <= right)
            {
                int mid = (left + right) / 2;
                int cmp = Compare(pattern, _suffixArray[mid]);
                if (cmp == 0) return true;
                if (cmp < 0) right = mid - 1;
                else left = mid + 1;
            }

            return false;
        }

        /// <summary>
        /// 统计模式串出现次数
        /// </summary>
        public int Count(string pattern)
        {
            return Search(pattern).Count;
        }

        /// <summary>
        /// 获取最长重复子串
        /// </summary>
        public string GetLongestRepeatedSubstring()
        {
            int maxLength = 0, maxIndex = 0;
            for (int i = 0; i < _lcpArray.Length; i++)
            {
                if (_lcpArray[i] > maxLength)
                {
                    maxLength = _lcpArray[i];
                    maxIndex = _suffixArray[i];
                }
            }
            return _text.Substring(maxIndex, maxLength);
        }

        /// <summary>
        /// 获取所有最长公共子串
        /// </summary>
        public List<string> GetAllLongestCommonSubstrings(int minLength = 2)
        {
            var result = new List<string>();
            for (int i = 0; i < _lcpArray.Length; i++)
            {
                if (_lcpArray[i] >= minLength)
                {
                    string substr = _text.Substring(_suffixArray[i], _lcpArray[i]);
                    if (!result.Contains(substr))
                        result.Add(substr);
                }
            }
            return result;
        }

        private int Compare(string pattern, int start)
        {
            for (int i = 0; i < pattern.Length && start + i < _text.Length; i++)
            {
                if (pattern[i] < _text[start + i]) return -1;
                if (pattern[i] > _text[start + i]) return 1;
            }
            if (pattern.Length > _text.Length - start) return 1;
            if (pattern.Length < _text.Length - start) return -1;
            return 0;
        }

        private static int[] BuildSuffixArray(string text)
        {
            int n = text.Length;
            var sa = new int[n];
            var rank = new int[n];
            var tempRank = new int[n];

            // 初始化
            for (int i = 0; i < n; i++)
            {
                sa[i] = i;
                rank[i] = text[i];
            }

            // 倍增法
            for (int k = 1; k < n; k *= 2)
            {
                // 按第二关键字排序
                var tempSa = new int[n];
                int p = 0;
                for (int i = n - k; i < n; i++) tempSa[p++] = i;
                for (int i = 0; i < n; i++) if (sa[i] >= k) tempSa[p++] = sa[i] - k;

                // 按第一关键字计数排序
                var cnt = new int[Math.Max(256, n)];
                for (int i = 0; i < n; i++) cnt[rank[i]]++;
                for (int i = 1; i < cnt.Length; i++) cnt[i] += cnt[i - 1];
                for (int i = n - 1; i >= 0; i--) sa[--cnt[rank[tempSa[i]]]] = tempSa[i];

                // 重新计算rank
                tempRank[sa[0]] = 0;
                p = 0;
                for (int i = 1; i < n; i++)
                {
                    int curr = rank[sa[i]] * (sa[i] + k < n ? rank[sa[i] + k] + 1 : 0);
                    int prev = rank[sa[i - 1]] * (sa[i - 1] + k < n ? rank[sa[i - 1] + k] + 1 : 0);
                    tempRank[sa[i]] = curr == prev ? p : ++p;
                }
                for (int i = 0; i < n; i++) rank[i] = tempRank[i];

                if (p == n - 1) break;
            }

            return sa;
        }

        private static int[] BuildLCPArray(string text, int[] sa)
        {
            int n = text.Length;
            var lcp = new int[n];
            var rank = new int[n];

            for (int i = 0; i < n; i++) rank[sa[i]] = i;

            int k = 0;
            for (int i = 0; i < n; i++)
            {
                if (rank[i] == 0)
                {
                    k = 0;
                    continue;
                }

                int j = sa[rank[i] - 1];
                while (i + k < n && j + k < n && text[i + k] == text[j + k]) k++;

                lcp[rank[i]] = k;
                if (k > 0) k--;
            }

            return lcp;
        }
    }

    #endregion
}
