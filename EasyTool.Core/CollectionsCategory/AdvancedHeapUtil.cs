using System;
using System.Collections.Generic;

namespace EasyTool.CollectionsCategory
{
    /// <summary>
    /// 高级堆工具类
    /// </summary>
    public static class AdvancedHeapUtil
    {
        /// <summary>
        /// 创建配对堆
        /// </summary>
        public static PairingHeap<T> CreatePairing<T>() where T : IComparable<T>
        {
            return new PairingHeap<T>();
        }

        /// <summary>
        /// 创建斐波那契堆
        /// </summary>
        public static FibonacciHeap<T> CreateFibonacci<T>() where T : IComparable<T>
        {
            return new FibonacciHeap<T>();
        }

        /// <summary>
        /// 创建二项堆
        /// </summary>
        public static BinomialHeap<T> CreateBinomial<T>() where T : IComparable<T>
        {
            return new BinomialHeap<T>();
        }
    }

    /// <summary>
    /// 配对堆
    /// 时间复杂度：插入O(1)，删除最小O(log n)摊还，合并O(1)
    /// </summary>
    public class PairingHeap<T> where T : IComparable<T>
    {
        private class Node
        {
            public T Value { get; set; }
            public Node Child { get; set; }
            public Node Sibling { get; set; }
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
        /// 最小值
        /// </summary>
        public T Min
        {
            get
            {
                if (_root == null)
                    throw new InvalidOperationException("Heap is empty");
                return _root.Value;
            }
        }

        /// <summary>
        /// 创建配对堆
        /// </summary>
        public PairingHeap()
        {
            _root = null;
            _count = 0;
        }

        /// <summary>
        /// 插入元素
        /// </summary>
        public void Insert(T value)
        {
            var node = new Node(value);
            _root = Merge(_root, node);
            _count++;
        }

        /// <summary>
        /// 删除最小元素
        /// </summary>
        public T DeleteMin()
        {
            if (_root == null)
                throw new InvalidOperationException("Heap is empty");

            T minValue = _root.Value;
            _root = MergePairs(_root.Child);
            _count--;
            return minValue;
        }

        /// <summary>
        /// 查看最小元素
        /// </summary>
        public T PeekMin()
        {
            if (_root == null)
                throw new InvalidOperationException("Heap is empty");
            return _root.Value;
        }

        /// <summary>
        /// 合并另一个堆
        /// </summary>
        public void Merge(PairingHeap<T> other)
        {
            if (other == null)
                return;

            _root = Merge(_root, other._root);
            _count += other._count;
            other._root = null;
            other._count = 0;
        }

        private Node Merge(Node a, Node b)
        {
            if (a == null)
                return b;
            if (b == null)
                return a;

            if (a.Value.CompareTo(b.Value) <= 0)
            {
                b.Sibling = a.Child;
                b.Parent = a;
                a.Child = b;
                return a;
            }
            else
            {
                a.Sibling = b.Child;
                a.Parent = b;
                b.Child = a;
                return b;
            }
        }

        private Node MergePairs(Node node)
        {
            if (node == null || node.Sibling == null)
                return node;

            // 收集所有兄弟节点
            var nodes = new List<Node>();
            while (node != null)
            {
                nodes.Add(node);
                node = node.Sibling;
            }

            // 从左到右两两合并
            var merged = new List<Node>();
            for (int i = 0; i < nodes.Count; i += 2)
            {
                if (i + 1 < nodes.Count)
                {
                    merged.Add(Merge(nodes[i], nodes[i + 1]));
                }
                else
                {
                    merged.Add(nodes[i]);
                }
            }

            // 从右到左合并
            Node result = merged[merged.Count - 1];
            for (int i = merged.Count - 2; i >= 0; i--)
            {
                result = Merge(result, merged[i]);
            }

            return result;
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
    /// 斐波那契堆
    /// 时间复杂度：插入O(1)，删除最小O(log n)摊还，降低键O(1)摊还
    /// </summary>
    public class FibonacciHeap<T> where T : IComparable<T>
    {
        private class Node
        {
            public T Value { get; set; }
            public Node Parent { get; set; }
            public Node Child { get; set; }
            public Node Left { get; set; }
            public Node Right { get; set; }
            public int Degree { get; set; }
            public bool Mark { get; set; }

            public Node(T value)
            {
                Value = value;
                Left = this;
                Right = this;
            }
        }

        private Node _min;
        private int _count;
        private readonly List<Node> _degreeList;

        /// <summary>
        /// 元素数量
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
                if (_min == null)
                    throw new InvalidOperationException("Heap is empty");
                return _min.Value;
            }
        }

        /// <summary>
        /// 创建斐波那契堆
        /// </summary>
        public FibonacciHeap()
        {
            _min = null;
            _count = 0;
            _degreeList = new List<Node>();
        }

        /// <summary>
        /// 插入元素
        /// </summary>
        public void Insert(T value)
        {
            var node = new Node(value);

            if (_min == null)
            {
                _min = node;
            }
            else
            {
                AddToRootList(node);
                if (node.Value.CompareTo(_min.Value) < 0)
                    _min = node;
            }

            _count++;
        }

        /// <summary>
        /// 删除最小元素
        /// </summary>
        public T DeleteMin()
        {
            if (_min == null)
                throw new InvalidOperationException("Heap is empty");

            T minValue = _min.Value;

            // 将子节点添加到根列表
            if (_min.Child != null)
            {
                var child = _min.Child;
                do
                {
                    var next = child.Right;
                    child.Parent = null;
                    AddToRootList(child);
                    child = next;
                } while (child != _min.Child);
            }

            // 从根列表移除最小节点
            RemoveFromRootList(_min);

            if (_min == _min.Right)
            {
                _min = null;
            }
            else
            {
                _min = _min.Right;
                Consolidate();
            }

            _count--;
            return minValue;
        }

        /// <summary>
        /// 查看最小元素
        /// </summary>
        public T PeekMin()
        {
            if (_min == null)
                throw new InvalidOperationException("Heap is empty");
            return _min.Value;
        }

        /// <summary>
        /// 合并另一个堆
        /// </summary>
        public void Merge(FibonacciHeap<T> other)
        {
            if (other == null || other._min == null)
                return;

            if (_min == null)
            {
                _min = other._min;
            }
            else
            {
                // 连接根列表
                var thisRight = _min.Right;
                var otherLeft = other._min.Left;

                _min.Right = other._min;
                other._min.Left = _min;
                thisRight.Left = otherLeft;
                otherLeft.Right = thisRight;

                if (other._min.Value.CompareTo(_min.Value) < 0)
                    _min = other._min;
            }

            _count += other._count;
            other._min = null;
            other._count = 0;
        }

        private void AddToRootList(Node node)
        {
            if (_min == null)
            {
                _min = node;
                node.Left = node;
                node.Right = node;
            }
            else
            {
                node.Left = _min;
                node.Right = _min.Right;
                _min.Right.Left = node;
                _min.Right = node;
            }
        }

        private void RemoveFromRootList(Node node)
        {
            node.Left.Right = node.Right;
            node.Right.Left = node.Left;
        }

        private void Consolidate()
        {
            _degreeList.Clear();
            var maxDegree = (int)Math.Floor(Math.Log(_count) / Math.Log(2)) + 1;

            for (int i = 0; i <= maxDegree; i++)
            {
                _degreeList.Add(null);
            }

            var roots = new List<Node>();
            var current = _min;
            do
            {
                roots.Add(current);
                current = current.Right;
            } while (current != _min);

            foreach (var root in roots)
            {
                var x = root;
                int d = x.Degree;

                while (d < _degreeList.Count && _degreeList[d] != null)
                {
                    var y = _degreeList[d];
                    if (x.Value.CompareTo(y.Value) > 0)
                    {
                        var temp = x;
                        x = y;
                        y = temp;
                    }

                    Link(y, x);
                    _degreeList[d] = null;
                    d++;
                }

                if (d >= _degreeList.Count)
                {
                    for (int i = _degreeList.Count; i <= d; i++)
                        _degreeList.Add(null);
                }

                _degreeList[d] = x;
            }

            _min = null;
            foreach (var node in _degreeList)
            {
                if (node != null)
                {
                    if (_min == null || node.Value.CompareTo(_min.Value) < 0)
                    {
                        _min = node;
                    }
                }
            }
        }

        private void Link(Node child, Node parent)
        {
            RemoveFromRootList(child);

            child.Parent = parent;
            child.Left = child;
            child.Right = child;

            if (parent.Child == null)
            {
                parent.Child = child;
            }
            else
            {
                child.Left = parent.Child;
                child.Right = parent.Child.Right;
                parent.Child.Right.Left = child;
                parent.Child.Right = child;
            }

            parent.Degree++;
            child.Mark = false;
        }

        /// <summary>
        /// 清空
        /// </summary>
        public void Clear()
        {
            _min = null;
            _count = 0;
            _degreeList.Clear();
        }
    }

    /// <summary>
    /// 二项堆
    /// 时间复杂度：插入O(log n)，删除最小O(log n)，合并O(log n)
    /// </summary>
    public class BinomialHeap<T> where T : IComparable<T>
    {
        private class Node
        {
            public T Value { get; set; }
            public int Degree { get; set; }
            public Node Child { get; set; }
            public Node Sibling { get; set; }
            public Node Parent { get; set; }

            public Node(T value)
            {
                Value = value;
            }
        }

        private Node _head;
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
        /// 最小值
        /// </summary>
        public T Min
        {
            get
            {
                if (_head == null)
                    throw new InvalidOperationException("Heap is empty");

                var min = _head;
                var current = _head.Sibling;
                while (current != null)
                {
                    if (current.Value.CompareTo(min.Value) < 0)
                        min = current;
                    current = current.Sibling;
                }
                return min.Value;
            }
        }

        /// <summary>
        /// 创建二项堆
        /// </summary>
        public BinomialHeap()
        {
            _head = null;
            _count = 0;
        }

        /// <summary>
        /// 插入元素
        /// </summary>
        public void Insert(T value)
        {
            var node = new Node(value);
            var newHead = Union(_head, node);
            _head = newHead;
            _count++;
        }

        /// <summary>
        /// 删除最小元素
        /// </summary>
        public T DeleteMin()
        {
            if (_head == null)
                throw new InvalidOperationException("Heap is empty");

            // 找到最小节点及其前驱
            Node minPrev = null;
            Node min = _head;
            Node prev = null;
            Node current = _head;

            while (current != null)
            {
                if (current.Value.CompareTo(min.Value) < 0)
                {
                    min = current;
                    minPrev = prev;
                }
                prev = current;
                current = current.Sibling;
            }

            // 从根列表中移除最小节点
            if (minPrev == null)
            {
                _head = min.Sibling;
            }
            else
            {
                minPrev.Sibling = min.Sibling;
            }

            // 反转最小节点的子节点
            Node newHead = null;
            var child = min.Child;
            while (child != null)
            {
                var next = child.Sibling;
                child.Sibling = newHead;
                child.Parent = null;
                newHead = child;
                child = next;
            }

            // 合并
            _head = Union(_head, newHead);
            _count--;

            return min.Value;
        }

        /// <summary>
        /// 查看最小元素
        /// </summary>
        public T PeekMin()
        {
            return Min;
        }

        /// <summary>
        /// 合并另一个堆
        /// </summary>
        public void Merge(BinomialHeap<T> other)
        {
            if (other == null)
                return;

            _head = Union(_head, other._head);
            _count += other._count;
            other._head = null;
            other._count = 0;
        }

        private Node Union(Node h1, Node h2)
        {
            if (h1 == null)
                return h2;
            if (h2 == null)
                return h1;

            Node head;
            if (h1.Degree <= h2.Degree)
            {
                head = h1;
                h1 = h1.Sibling;
            }
            else
            {
                head = h2;
                h2 = h2.Sibling;
            }

            Node tail = head;
            while (h1 != null && h2 != null)
            {
                if (h1.Degree <= h2.Degree)
                {
                    tail.Sibling = h1;
                    h1 = h1.Sibling;
                }
                else
                {
                    tail.Sibling = h2;
                    h2 = h2.Sibling;
                }
                tail = tail.Sibling;
            }

            tail.Sibling = h1 ?? h2;

            return Consolidate(head);
        }

        private Node Consolidate(Node head)
        {
            if (head == null)
                return null;

            Node prev = null;
            Node current = head;
            Node next = head.Sibling;

            while (next != null)
            {
                if (current.Degree != next.Degree ||
                    (next.Sibling != null && next.Sibling.Degree == current.Degree))
                {
                    prev = current;
                    current = next;
                }
                else
                {
                    if (current.Value.CompareTo(next.Value) <= 0)
                    {
                        current.Sibling = next.Sibling;
                        Link(next, current);
                    }
                    else
                    {
                        if (prev == null)
                        {
                            head = next;
                        }
                        else
                        {
                            prev.Sibling = next;
                        }
                        Link(current, next);
                        current = next;
                    }
                }
                next = current.Sibling;
            }

            return head;
        }

        private void Link(Node child, Node parent)
        {
            child.Parent = parent;
            child.Sibling = parent.Child;
            parent.Child = child;
            parent.Degree++;
        }

        /// <summary>
        /// 清空
        /// </summary>
        public void Clear()
        {
            _head = null;
            _count = 0;
        }
    }
}
