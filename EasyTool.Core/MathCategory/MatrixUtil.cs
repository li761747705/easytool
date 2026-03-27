using System;
using System.Linq;

namespace EasyTool.MathCategory
{
    /// <summary>
    /// 矩阵工具类
    /// 提供矩阵的基本运算
    /// </summary>
    public static class MatrixUtil
    {
        #region 创建

        /// <summary>
        /// 创建矩阵
        /// </summary>
        /// <param name="rows">行数</param>
        /// <param name="cols">列数</param>
        /// <param name="value">初始值</param>
        /// <returns>矩阵</returns>
        public static Matrix Create(int rows, int cols, double value = 0)
        {
            return new Matrix(rows, cols, value);
        }

        /// <summary>
        /// 从二维数组创建矩阵
        /// </summary>
        /// <param name="array">二维数组</param>
        /// <returns>矩阵</returns>
        public static Matrix FromArray(double[,] array)
        {
            var rows = array.GetLength(0);
            var cols = array.GetLength(1);
            var matrix = new Matrix(rows, cols);

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    matrix[i, j] = array[i, j];
                }
            }

            return matrix;
        }

        /// <summary>
        /// 创建单位矩阵
        /// </summary>
        /// <param name="size">大小</param>
        /// <returns>单位矩阵</returns>
        public static Matrix Identity(int size)
        {
            var matrix = new Matrix(size, size);
            for (int i = 0; i < size; i++)
            {
                matrix[i, i] = 1;
            }
            return matrix;
        }

        /// <summary>
        /// 创建零矩阵
        /// </summary>
        /// <param name="rows">行数</param>
        /// <param name="cols">列数</param>
        /// <returns>零矩阵</returns>
        public static Matrix Zeros(int rows, int cols)
        {
            return new Matrix(rows, cols);
        }

        /// <summary>
        /// 创建全1矩阵
        /// </summary>
        /// <param name="rows">行数</param>
        /// <param name="cols">列数</param>
        /// <returns>全1矩阵</returns>
        public static Matrix Ones(int rows, int cols)
        {
            return new Matrix(rows, cols, 1);
        }

        /// <summary>
        /// 创建对角矩阵
        /// </summary>
        /// <param name="diagonal">对角元素</param>
        /// <returns>对角矩阵</returns>
        public static Matrix Diagonal(params double[] diagonal)
        {
            var size = diagonal.Length;
            var matrix = new Matrix(size, size);
            for (int i = 0; i < size; i++)
            {
                matrix[i, i] = diagonal[i];
            }
            return matrix;
        }

        /// <summary>
        /// 创建随机矩阵
        /// </summary>
        /// <param name="rows">行数</param>
        /// <param name="cols">列数</param>
        /// <param name="min">最小值</param>
        /// <param name="max">最大值</param>
        /// <returns>随机矩阵</returns>
        public static Matrix Random(int rows, int cols, double min = 0, double max = 1)
        {
            var random = new Random();
            var matrix = new Matrix(rows, cols);

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    matrix[i, j] = random.NextDouble() * (max - min) + min;
                }
            }

            return matrix;
        }

        #endregion

        #region 运算

        /// <summary>
        /// 矩阵加法
        /// </summary>
        public static Matrix Add(Matrix a, Matrix b)
        {
            if (a.Rows != b.Rows || a.Cols != b.Cols)
                throw new ArgumentException("矩阵维度不匹配");

            var result = new Matrix(a.Rows, a.Cols);
            for (int i = 0; i < a.Rows; i++)
            {
                for (int j = 0; j < a.Cols; j++)
                {
                    result[i, j] = a[i, j] + b[i, j];
                }
            }
            return result;
        }

        /// <summary>
        /// 矩阵减法
        /// </summary>
        public static Matrix Subtract(Matrix a, Matrix b)
        {
            if (a.Rows != b.Rows || a.Cols != b.Cols)
                throw new ArgumentException("矩阵维度不匹配");

            var result = new Matrix(a.Rows, a.Cols);
            for (int i = 0; i < a.Rows; i++)
            {
                for (int j = 0; j < a.Cols; j++)
                {
                    result[i, j] = a[i, j] - b[i, j];
                }
            }
            return result;
        }

        /// <summary>
        /// 矩阵乘法
        /// </summary>
        public static Matrix Multiply(Matrix a, Matrix b)
        {
            if (a.Cols != b.Rows)
                throw new ArgumentException("矩阵维度不匹配");

            var result = new Matrix(a.Rows, b.Cols);
            for (int i = 0; i < a.Rows; i++)
            {
                for (int j = 0; j < b.Cols; j++)
                {
                    double sum = 0;
                    for (int k = 0; k < a.Cols; k++)
                    {
                        sum += a[i, k] * b[k, j];
                    }
                    result[i, j] = sum;
                }
            }
            return result;
        }

        /// <summary>
        /// 标量乘法
        /// </summary>
        public static Matrix Scale(Matrix matrix, double scalar)
        {
            var result = new Matrix(matrix.Rows, matrix.Cols);
            for (int i = 0; i < matrix.Rows; i++)
            {
                for (int j = 0; j < matrix.Cols; j++)
                {
                    result[i, j] = matrix[i, j] * scalar;
                }
            }
            return result;
        }

        /// <summary>
        /// 矩阵转置
        /// </summary>
        public static Matrix Transpose(Matrix matrix)
        {
            var result = new Matrix(matrix.Cols, matrix.Rows);
            for (int i = 0; i < matrix.Rows; i++)
            {
                for (int j = 0; j < matrix.Cols; j++)
                {
                    result[j, i] = matrix[i, j];
                }
            }
            return result;
        }

        /// <summary>
        /// 行列式
        /// </summary>
        public static double Determinant(Matrix matrix)
        {
            if (!matrix.IsSquare)
                throw new ArgumentException("矩阵必须是方阵");

            return DeterminantInternal(matrix);
        }

        private static double DeterminantInternal(Matrix matrix)
        {
            int n = matrix.Rows;

            if (n == 1)
                return matrix[0, 0];

            if (n == 2)
                return matrix[0, 0] * matrix[1, 1] - matrix[0, 1] * matrix[1, 0];

            double det = 0;
            for (int j = 0; j < n; j++)
            {
                det += matrix[0, j] * Cofactor(matrix, 0, j);
            }
            return det;
        }

        private static double Cofactor(Matrix matrix, int row, int col)
        {
            var minor = GetMinor(matrix, row, col);
            return Math.Pow(-1, row + col) * DeterminantInternal(minor);
        }

        private static Matrix GetMinor(Matrix matrix, int excludeRow, int excludeCol)
        {
            var minor = new Matrix(matrix.Rows - 1, matrix.Cols - 1);
            int mi = 0, mj = 0;

            for (int i = 0; i < matrix.Rows; i++)
            {
                if (i == excludeRow) continue;

                mj = 0;
                for (int j = 0; j < matrix.Cols; j++)
                {
                    if (j == excludeCol) continue;
                    minor[mi, mj] = matrix[i, j];
                    mj++;
                }
                mi++;
            }

            return minor;
        }

        /// <summary>
        /// 逆矩阵
        /// </summary>
        public static Matrix? Inverse(Matrix matrix)
        {
            if (!matrix.IsSquare)
                throw new ArgumentException("矩阵必须是方阵");

            var det = Determinant(matrix);
            if (Math.Abs(det) < double.Epsilon)
                return null;

            int n = matrix.Rows;
            var result = new Matrix(n, n);

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    result[i, j] = Cofactor(matrix, i, j) / det;
                }
            }

            // 转置得到逆矩阵
            return Transpose(result);
        }

        /// <summary>
        /// 迹（对角元素之和）
        /// </summary>
        public static double Trace(Matrix matrix)
        {
            if (!matrix.IsSquare)
                throw new ArgumentException("矩阵必须是方阵");

            double trace = 0;
            for (int i = 0; i < matrix.Rows; i++)
            {
                trace += matrix[i, i];
            }
            return trace;
        }

        /// <summary>
        /// Frobenius 范数
        /// </summary>
        public static double FrobeniusNorm(Matrix matrix)
        {
            double sum = 0;
            for (int i = 0; i < matrix.Rows; i++)
            {
                for (int j = 0; j < matrix.Cols; j++)
                {
                    sum += matrix[i, j] * matrix[i, j];
                }
            }
            return Math.Sqrt(sum);
        }

        #endregion

        #region 变换

        /// <summary>
        /// 水平翻转
        /// </summary>
        public static Matrix FlipHorizontal(Matrix matrix)
        {
            var result = new Matrix(matrix.Rows, matrix.Cols);
            for (int i = 0; i < matrix.Rows; i++)
            {
                for (int j = 0; j < matrix.Cols; j++)
                {
                    result[i, j] = matrix[i, matrix.Cols - 1 - j];
                }
            }
            return result;
        }

        /// <summary>
        /// 垂直翻转
        /// </summary>
        public static Matrix FlipVertical(Matrix matrix)
        {
            var result = new Matrix(matrix.Rows, matrix.Cols);
            for (int i = 0; i < matrix.Rows; i++)
            {
                for (int j = 0; j < matrix.Cols; j++)
                {
                    result[i, j] = matrix[matrix.Rows - 1 - i, j];
                }
            }
            return result;
        }

        /// <summary>
        /// 顺时针旋转 90 度
        /// </summary>
        public static Matrix Rotate90(Matrix matrix)
        {
            var result = new Matrix(matrix.Cols, matrix.Rows);
            for (int i = 0; i < matrix.Rows; i++)
            {
                for (int j = 0; j < matrix.Cols; j++)
                {
                    result[j, matrix.Rows - 1 - i] = matrix[i, j];
                }
            }
            return result;
        }

        /// <summary>
        /// 水平拼接
        /// </summary>
        public static Matrix HorizontalConcat(Matrix a, Matrix b)
        {
            if (a.Rows != b.Rows)
                throw new ArgumentException("矩阵行数不匹配");

            var result = new Matrix(a.Rows, a.Cols + b.Cols);
            for (int i = 0; i < a.Rows; i++)
            {
                for (int j = 0; j < a.Cols; j++)
                {
                    result[i, j] = a[i, j];
                }
                for (int j = 0; j < b.Cols; j++)
                {
                    result[i, a.Cols + j] = b[i, j];
                }
            }
            return result;
        }

        /// <summary>
        /// 垂直拼接
        /// </summary>
        public static Matrix VerticalConcat(Matrix a, Matrix b)
        {
            if (a.Cols != b.Cols)
                throw new ArgumentException("矩阵列数不匹配");

            var result = new Matrix(a.Rows + b.Rows, a.Cols);
            for (int i = 0; i < a.Rows; i++)
            {
                for (int j = 0; j < a.Cols; j++)
                {
                    result[i, j] = a[i, j];
                }
            }
            for (int i = 0; i < b.Rows; i++)
            {
                for (int j = 0; j < b.Cols; j++)
                {
                    result[a.Rows + i, j] = b[i, j];
                }
            }
            return result;
        }

        #endregion
    }

    /// <summary>
    /// 矩阵类
    /// </summary>
    public class Matrix
    {
        private readonly double[,] _data;

        /// <summary>
        /// 行数
        /// </summary>
        public int Rows { get; }

        /// <summary>
        /// 列数
        /// </summary>
        public int Cols { get; }

        /// <summary>
        /// 是否为方阵
        /// </summary>
        public bool IsSquare => Rows == Cols;

        /// <summary>
        /// 访问元素
        /// </summary>
        public double this[int row, int col]
        {
            get => _data[row, col];
            set => _data[row, col] = value;
        }

        /// <summary>
        /// 创建矩阵
        /// </summary>
        public Matrix(int rows, int cols, double value = 0)
        {
            Rows = rows;
            Cols = cols;
            _data = new double[rows, cols];

            if (value != 0)
            {
                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        _data[i, j] = value;
                    }
                }
            }
        }

        /// <summary>
        /// 获取行
        /// </summary>
        public double[] GetRow(int row)
        {
            var result = new double[Cols];
            for (int j = 0; j < Cols; j++)
            {
                result[j] = _data[row, j];
            }
            return result;
        }

        /// <summary>
        /// 获取列
        /// </summary>
        public double[] GetColumn(int col)
        {
            var result = new double[Rows];
            for (int i = 0; i < Rows; i++)
            {
                result[i] = _data[i, col];
            }
            return result;
        }

        /// <summary>
        /// 转换为二维数组
        /// </summary>
        public double[,] ToArray()
        {
            var result = new double[Rows, Cols];
            Array.Copy(_data, result, _data.Length);
            return result;
        }

        /// <summary>
        /// 转换为字符串
        /// </summary>
        public override string ToString()
        {
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < Rows; i++)
            {
                sb.Append("[");
                for (int j = 0; j < Cols; j++)
                {
                    sb.Append(_data[i, j].ToString("F4").PadLeft(10));
                    if (j < Cols - 1) sb.Append(", ");
                }
                sb.AppendLine("]");
            }
            return sb.ToString();
        }

        #region 运算符重载

        public static Matrix operator +(Matrix a, Matrix b) => MatrixUtil.Add(a, b);
        public static Matrix operator -(Matrix a, Matrix b) => MatrixUtil.Subtract(a, b);
        public static Matrix operator *(Matrix a, Matrix b) => MatrixUtil.Multiply(a, b);
        public static Matrix operator *(Matrix a, double scalar) => MatrixUtil.Scale(a, scalar);
        public static Matrix operator *(double scalar, Matrix a) => MatrixUtil.Scale(a, scalar);

        #endregion
    }
}
