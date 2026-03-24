using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyTool.CollectionsCategory
{
    /// <summary>
    /// 矩阵工具类
    /// 提供二维矩阵的常用操作
    /// </summary>
    public static class MatrixUtil
    {
        /// <summary>
        /// 创建矩阵
        /// </summary>
        public static Matrix<T> Create<T>(int rows, int cols)
        {
            return new Matrix<T>(rows, cols);
        }

        /// <summary>
        /// 从二维数组创建矩阵
        /// </summary>
        public static Matrix<T> FromArray<T>(T[,] array)
        {
            return new Matrix<T>(array);
        }

        /// <summary>
        /// 创建全零矩阵
        /// </summary>
        public static Matrix<int> Zeros(int rows, int cols)
        {
            return new Matrix<int>(rows, cols);
        }

        /// <summary>
        /// 创建全一矩阵
        /// </summary>
        public static Matrix<int> Ones(int rows, int cols)
        {
            var matrix = new Matrix<int>(rows, cols);
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                    matrix[i, j] = 1;
            return matrix;
        }

        /// <summary>
        /// 创建单位矩阵
        /// </summary>
        public static Matrix<int> Identity(int size)
        {
            var matrix = new Matrix<int>(size, size);
            for (int i = 0; i < size; i++)
                matrix[i, i] = 1;
            return matrix;
        }

        /// <summary>
        /// 创建对角矩阵
        /// </summary>
        public static Matrix<T> Diagonal<T>(T[] diagonal)
        {
            int n = diagonal.Length;
            var matrix = new Matrix<T>(n, n);
            for (int i = 0; i < n; i++)
                matrix[i, i] = diagonal[i];
            return matrix;
        }
    }

    /// <summary>
    /// 矩阵实现
    /// </summary>
    /// <typeparam name="T">元素类型</typeparam>
    public class Matrix<T>
    {
        private readonly T[,] _data;

        /// <summary>
        /// 行数
        /// </summary>
        public int Rows { get; }

        /// <summary>
        /// 列数
        /// </summary>
        public int Columns { get; }

        /// <summary>
        /// 元素总数
        /// </summary>
        public int Length => Rows * Columns;

        /// <summary>
        /// 访问元素
        /// </summary>
        public T this[int row, int col]
        {
            get
            {
                ValidateIndex(row, col);
                return _data[row, col];
            }
            set
            {
                ValidateIndex(row, col);
                _data[row, col] = value;
            }
        }

        /// <summary>
        /// 创建矩阵
        /// </summary>
        public Matrix(int rows, int cols)
        {
            if (rows <= 0 || cols <= 0)
                throw new ArgumentOutOfRangeException("Rows and columns must be positive");

            Rows = rows;
            Columns = cols;
            _data = new T[rows, cols];
        }

        /// <summary>
        /// 从二维数组创建矩阵
        /// </summary>
        public Matrix(T[,] array)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            Rows = array.GetLength(0);
            Columns = array.GetLength(1);
            _data = new T[Rows, Columns];
            Array.Copy(array, _data, array.Length);
        }

        /// <summary>
        /// 获取行
        /// </summary>
        public T[] GetRow(int row)
        {
            if (row < 0 || row >= Rows)
                throw new ArgumentOutOfRangeException(nameof(row));

            var result = new T[Columns];
            for (int i = 0; i < Columns; i++)
                result[i] = _data[row, i];
            return result;
        }

        /// <summary>
        /// 获取列
        /// </summary>
        public T[] GetColumn(int col)
        {
            if (col < 0 || col >= Columns)
                throw new ArgumentOutOfRangeException(nameof(col));

            var result = new T[Rows];
            for (int i = 0; i < Rows; i++)
                result[i] = _data[i, col];
            return result;
        }

        /// <summary>
        /// 设置行
        /// </summary>
        public void SetRow(int row, T[] values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));
            if (values.Length != Columns)
                throw new ArgumentException("Values length must match column count");

            for (int i = 0; i < Columns; i++)
                _data[row, i] = values[i];
        }

        /// <section>
        /// 设置列
        /// </summary>
        public void SetColumn(int col, T[] values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));
            if (values.Length != Rows)
                throw new ArgumentException("Values length must match row count");

            for (int i = 0; i < Rows; i++)
                _data[i, col] = values[i];
        }

        /// <summary>
        /// 转置
        /// </summary>
        public Matrix<T> Transpose()
        {
            var result = new Matrix<T>(Columns, Rows);
            for (int i = 0; i < Rows; i++)
                for (int j = 0; j < Columns; j++)
                    result[j, i] = _data[i, j];
            return result;
        }

        /// <summary>
        /// 翻转行
        /// </summary>
        public Matrix<T> FlipVertical()
        {
            var result = new Matrix<T>(Rows, Columns);
            for (int i = 0; i < Rows; i++)
                for (int j = 0; j < Columns; j++)
                    result[Rows - 1 - i, j] = _data[i, j];
            return result;
        }

        /// <summary>
        /// 翻转列
        /// </summary>
        public Matrix<T> FlipHorizontal()
        {
            var result = new Matrix<T>(Rows, Columns);
            for (int i = 0; i < Rows; i++)
                for (int j = 0; j < Columns; j++)
                    result[i, Columns - 1 - j] = _data[i, j];
            return result;
        }

        /// <summary>
        /// 顺时针旋转90度
        /// </summary>
        public Matrix<T> Rotate90()
        {
            var result = new Matrix<T>(Columns, Rows);
            for (int i = 0; i < Rows; i++)
                for (int j = 0; j < Columns; j++)
                    result[j, Rows - 1 - i] = _data[i, j];
            return result;
        }

        /// <summary>
        /// 旋转180度
        /// </summary>
        public Matrix<T> Rotate180()
        {
            return FlipVertical().FlipHorizontal();
        }

        /// <summary>
        /// 逆时针旋转90度
        /// </summary>
        public Matrix<T> Rotate270()
        {
            var result = new Matrix<T>(Columns, Rows);
            for (int i = 0; i < Rows; i++)
                for (int j = 0; j < Columns; j++)
                    result[Columns - 1 - j, i] = _data[i, j];
            return result;
        }

        /// <summary>
        /// 获取子矩阵
        /// </summary>
        public Matrix<T> SubMatrix(int startRow, int startCol, int rows, int cols)
        {
            if (startRow < 0 || startCol < 0 || startRow + rows > Rows || startCol + cols > Columns)
                throw new ArgumentOutOfRangeException();

            var result = new Matrix<T>(rows, cols);
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                    result[i, j] = _data[startRow + i, startCol + j];
            return result;
        }

        /// <summary>
        /// 克隆矩阵
        /// </summary>
        public Matrix<T> Clone()
        {
            return new Matrix<T>(_data);
        }

        /// <summary>
        /// 转换为二维数组
        /// </summary>
        public T[,] ToArray()
        {
            var result = new T[Rows, Columns];
            Array.Copy(_data, result, _data.Length);
            return result;
        }

        /// <summary>
        /// 转换为交错数组
        /// </summary>
        public T[][] ToJaggedArray()
        {
            var result = new T[Rows][];
            for (int i = 0; i < Rows; i++)
            {
                result[i] = new T[Columns];
                for (int j = 0; j < Columns; j++)
                    result[i][j] = _data[i, j];
            }
            return result;
        }

        /// <summary>
        /// 展平为一维数组
        /// </summary>
        public T[] Flatten()
        {
            var result = new T[Length];
            int index = 0;
            for (int i = 0; i < Rows; i++)
                for (int j = 0; j < Columns; j++)
                    result[index++] = _data[i, j];
            return result;
        }

        /// <summary>
        /// 遍历所有元素
        /// </summary>
        public IEnumerable<T> Enumerate()
        {
            for (int i = 0; i < Rows; i++)
                for (int j = 0; j < Columns; j++)
                    yield return _data[i, j];
        }

        /// <summary>
        /// 填充所有元素
        /// </summary>
        public void Fill(T value)
        {
            for (int i = 0; i < Rows; i++)
                for (int j = 0; j < Columns; j++)
                    _data[i, j] = value;
        }

        /// <summary>
        /// 使用函数填充
        /// </summary>
        public void Fill(Func<int, int, T> generator)
        {
            for (int i = 0; i < Rows; i++)
                for (int j = 0; j < Columns; j++)
                    _data[i, j] = generator(i, j);
        }

        private void ValidateIndex(int row, int col)
        {
            if (row < 0 || row >= Rows)
                throw new ArgumentOutOfRangeException(nameof(row));
            if (col < 0 || col >= Columns)
                throw new ArgumentOutOfRangeException(nameof(col));
        }
    }
}
