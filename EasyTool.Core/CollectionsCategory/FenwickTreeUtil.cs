using System;
using System.Collections.Generic;

namespace EasyTool.CollectionsCategory
{
    /// <summary>
    /// 树状数组（Fenwick Tree / Binary Indexed Tree）工具类
    /// 用于高效计算前缀和，支持单点更新
    /// 时间复杂度：查询和更新都是 O(log n)
    /// </summary>
    public static class FenwickTreeUtil
    {
        /// <summary>
        /// 创建树状数组
        /// </summary>
        /// <param name="size">大小</param>
        public static FenwickTree Create(int size)
        {
            return new FenwickTree(size);
        }

        /// <summary>
        /// 从数组创建树状数组
        /// </summary>
        public static FenwickTree Create(long[] array)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            return new FenwickTree(array);
        }

        /// <summary>
        /// 创建支持范围更新的树状数组
        /// </summary>
        public static FenwickTreeRange CreateRange(int size)
        {
            return new FenwickTreeRange(size);
        }

        /// <summary>
        /// 创建二维树状数组
        /// </summary>
        public static FenwickTree2D Create2D(int rows, int cols)
        {
            return new FenwickTree2D(rows, cols);
        }
    }

    /// <summary>
    /// 树状数组（Fenwick Tree）
    /// </summary>
    public class FenwickTree
    {
        private readonly long[] _tree;
        private readonly int _size;

        /// <summary>
        /// 大小
        /// </summary>
        public int Size => _size;

        /// <summary>
        /// 创建树状数组
        /// </summary>
        public FenwickTree(int size)
        {
            if (size <= 0)
                throw new ArgumentOutOfRangeException(nameof(size));

            _size = size;
            _tree = new long[size + 1];
        }

        /// <summary>
        /// 从数组创建树状数组
        /// </summary>
        public FenwickTree(long[] array) : this(array.Length)
        {
            for (int i = 0; i < array.Length; i++)
            {
                Update(i, array[i]);
            }
        }

        /// <summary>
        /// 单点更新（增加值）
        /// </summary>
        /// <param name="index">索引（0-based）</param>
        /// <param name="delta">增量</param>
        public void Update(int index, long delta)
        {
            if (index < 0 || index >= _size)
                throw new ArgumentOutOfRangeException(nameof(index));

            index++; // 转为1-based
            while (index <= _size)
            {
                _tree[index] += delta;
                index += index & (-index); // LowBit
            }
        }

        /// <summary>
        /// 设置指定位置的值
        /// </summary>
        public void Set(int index, long value)
        {
            if (index < 0 || index >= _size)
                throw new ArgumentOutOfRangeException(nameof(index));

            long current = Query(index, index);
            Update(index, value - current);
        }

        /// <summary>
        /// 查询前缀和 [0, index]
        /// </summary>
        public long Query(int index)
        {
            if (index < 0 || index >= _size)
                throw new ArgumentOutOfRangeException(nameof(index));

            index++; // 转为1-based
            long sum = 0;
            while (index > 0)
            {
                sum += _tree[index];
                index -= index & (-index); // LowBit
            }
            return sum;
        }

        /// <summary>
        /// 查询区间和 [left, right]
        /// </summary>
        public long Query(int left, int right)
        {
            if (left < 0 || right >= _size || left > right)
                throw new ArgumentException("Invalid range");

            if (left == 0)
                return Query(right);
            return Query(right) - Query(left - 1);
        }

        /// <summary>
        /// 获取指定位置的值
        /// </summary>
        public long Get(int index)
        {
            return Query(index, index);
        }

        /// <summary>
        /// 清空
        /// </summary>
        public void Clear()
        {
            Array.Clear(_tree, 0, _tree.Length);
        }
    }

    /// <summary>
    /// 支持范围更新的树状数组
    /// </summary>
    public class FenwickTreeRange
    {
        private readonly FenwickTree _tree1;
        private readonly FenwickTree _tree2;
        private readonly int _size;

        /// <summary>
        /// 大小
        /// </summary>
        public int Size => _size;

        /// <summary>
        /// 创建支持范围更新的树状数组
        /// </summary>
        public FenwickTreeRange(int size)
        {
            if (size <= 0)
                throw new ArgumentOutOfRangeException(nameof(size));

            _size = size;
            _tree1 = new FenwickTree(size);
            _tree2 = new FenwickTree(size);
        }

        /// <summary>
        /// 区间更新 [left, right] 增加delta
        /// </summary>
        public void UpdateRange(int left, int right, long delta)
        {
            if (left < 0 || right >= _size || left > right)
                throw new ArgumentException("Invalid range");

            _tree1.Update(left, delta);
            _tree1.Update(right + 1, -delta);
            _tree2.Update(left, delta * (left - 1));
            _tree2.Update(right + 1, -delta * right);
        }

        /// <summary>
        /// 单点更新
        /// </summary>
        public void Update(int index, long delta)
        {
            UpdateRange(index, index, delta);
        }

        /// <summary>
        /// 查询前缀和 [0, index]
        /// </summary>
        public long Query(int index)
        {
            if (index < 0 || index >= _size)
                throw new ArgumentOutOfRangeException(nameof(index));

            return _tree1.Query(index) * index - _tree2.Query(index);
        }

        /// <summary>
        /// 查询区间和 [left, right]
        /// </summary>
        public long Query(int left, int right)
        {
            if (left < 0 || right >= _size || left > right)
                throw new ArgumentException("Invalid range");

            if (left == 0)
                return Query(right);
            return Query(right) - Query(left - 1);
        }

        /// <summary>
        /// 清空
        /// </summary>
        public void Clear()
        {
            _tree1.Clear();
            _tree2.Clear();
        }
    }

    /// <summary>
    /// 二维树状数组
    /// </summary>
    public class FenwickTree2D
    {
        private readonly long[,] _tree;
        private readonly int _rows;
        private readonly int _cols;

        /// <summary>
        /// 行数
        /// </summary>
        public int Rows => _rows;

        /// <summary>
        /// 列数
        /// </summary>
        public int Cols => _cols;

        /// <summary>
        /// 创建二维树状数组
        /// </summary>
        public FenwickTree2D(int rows, int cols)
        {
            if (rows <= 0 || cols <= 0)
                throw new ArgumentException("Rows and cols must be positive");

            _rows = rows;
            _cols = cols;
            _tree = new long[rows + 1, cols + 1];
        }

        /// <summary>
        /// 单点更新
        /// </summary>
        public void Update(int row, int col, long delta)
        {
            if (row < 0 || row >= _rows || col < 0 || col >= _cols)
                throw new ArgumentOutOfRangeException();

            row++; col++;
            for (int i = row; i <= _rows; i += i & (-i))
            {
                for (int j = col; j <= _cols; j += j & (-j))
                {
                    _tree[i, j] += delta;
                }
            }
        }

        /// <summary>
        /// 查询前缀和 [(0,0), (row, col)]
        /// </summary>
        public long Query(int row, int col)
        {
            if (row < 0 || row >= _rows || col < 0 || col >= _cols)
                throw new ArgumentOutOfRangeException();

            row++; col++;
            long sum = 0;
            for (int i = row; i > 0; i -= i & (-i))
            {
                for (int j = col; j > 0; j -= j & (-j))
                {
                    sum += _tree[i, j];
                }
            }
            return sum;
        }

        /// <summary>
        /// 查询矩形区域和
        /// </summary>
        public long Query(int row1, int col1, int row2, int col2)
        {
            if (row1 < 0 || row2 >= _rows || row1 > row2 ||
                col1 < 0 || col2 >= _cols || col1 > col2)
                throw new ArgumentException("Invalid range");

            if (row1 == 0 && col1 == 0)
                return Query(row2, col2);
            if (row1 == 0)
                return Query(row2, col2) - Query(row2, col1 - 1);
            if (col1 == 0)
                return Query(row2, col2) - Query(row1 - 1, col2);

            return Query(row2, col2) - Query(row1 - 1, col2)
                   - Query(row2, col1 - 1) + Query(row1 - 1, col1 - 1);
        }

        /// <summary>
        /// 清空
        /// </summary>
        public void Clear()
        {
            Array.Clear(_tree, 0, _tree.Length);
        }
    }
}
