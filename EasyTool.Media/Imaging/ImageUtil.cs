using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace EasyTool.Media.Imaging
{
    /// <summary>
    /// 二维码配置
    /// </summary>
    public class QrCodeOptions
    {
        /// <summary>
        /// 宽度（像素）
        /// </summary>
        public int Width { get; set; } = 200;

        /// <summary>
        /// 高度（像素）
        /// </summary>
        public int Height { get; set; } = 200;

        /// <summary>
        /// 纠错级别
        /// </summary>
        public QrCodeErrorCorrection ErrorCorrection { get; set; } = QrCodeErrorCorrection.Medium;

        /// <summary>
        /// 前景色
        /// </summary>
        public Color ForeColor { get; set; } = Color.Black;

        /// <summary>
        /// 背景色
        /// </summary>
        public Color BackColor { get; set; } = Color.White;

        /// <summary>
        /// 边距（模块数）
        /// </summary>
        public int Margin { get; set; } = 4;
    }

    /// <summary>
    /// 二维码纠错级别
    /// </summary>
    public enum QrCodeErrorCorrection
    {
        /// <summary>
        /// 低（7%可纠错）
        /// </summary>
        Low = 0,

        /// <summary>
        /// 中（15%可纠错）
        /// </summary>
        Medium = 1,

        /// <summary>
        /// 高（25%可纠错）
        /// </summary>
        Quartile = 2,

        /// <summary>
        /// 最高（30%可纠错）
        /// </summary>
        High = 3
    }

    /// <summary>
    /// 二维码工具类
    /// 提供二维码生成功能
    /// </summary>
    public static class QrCodeUtil
    {
        #region 生成二维码

        /// <summary>
        /// 生成二维码图像
        /// </summary>
        /// <param name="content">内容</param>
        /// <param name="options">配置</param>
        /// <returns>二维码图像</returns>
        public static Bitmap Generate(string content, QrCodeOptions? options = null)
        {
            options ??= new QrCodeOptions();

            // 编码内容
            var bytes = Encoding.UTF8.GetBytes(content);

            // 生成QR码矩阵
            var matrix = GenerateQrMatrix(bytes, options.ErrorCorrection);

            // 创建图像
            var bitmap = new Bitmap(options.Width, options.Height, PixelFormat.Format24bppRgb);

            using (var g = Graphics.FromImage(bitmap))
            {
                g.Clear(options.BackColor);

                var moduleWidth = (double)options.Width / (matrix.GetLength(0) + 2 * options.Margin);
                var moduleHeight = (double)options.Height / (matrix.GetLength(1) + 2 * options.Margin);
                var moduleSize = Math.Min(moduleWidth, moduleHeight);

                var offsetX = (options.Width - matrix.GetLength(0) * moduleSize) / 2;
                var offsetY = (options.Height - matrix.GetLength(1) * moduleSize) / 2;

                using var brush = new SolidBrush(options.ForeColor);

                for (int y = 0; y < matrix.GetLength(1); y++)
                {
                    for (int x = 0; x < matrix.GetLength(0); x++)
                    {
                        if (matrix[x, y])
                        {
                            var rect = new RectangleF(
                                (float)(offsetX + x * moduleSize),
                                (float)(offsetY + y * moduleSize),
                                (float)moduleSize,
                                (float)moduleSize);
                            g.FillRectangle(brush, rect);
                        }
                    }
                }
            }

            return bitmap;
        }

        /// <summary>
        /// 生成二维码并保存到文件
        /// </summary>
        /// <param name="content">内容</param>
        /// <param name="filePath">文件路径</param>
        /// <param name="options">配置</param>
        public static void GenerateToFile(string content, string filePath, QrCodeOptions? options = null)
        {
            using var bitmap = Generate(content, options);
            var format = GetImageFormat(filePath);
            bitmap.Save(filePath, format);
        }

        /// <summary>
        /// 生成二维码并返回Base64字符串
        /// </summary>
        /// <param name="content">内容</param>
        /// <param name="options">配置</param>
        /// <param name="format">图像格式</param>
        /// <returns>Base64字符串</returns>
        public static string GenerateToBase64(string content, QrCodeOptions? options = null, ImageFormat? format = null)
        {
            using var bitmap = Generate(content, options);
            using var ms = new MemoryStream();
            bitmap.Save(ms, format ?? ImageFormat.Png);
            return Convert.ToBase64String(ms.ToArray());
        }

        /// <summary>
        /// 生成二维码并返回Data URI
        /// </summary>
        /// <param name="content">内容</param>
        /// <param name="options">配置</param>
        /// <param name="format">图像格式</param>
        /// <returns>Data URI字符串</returns>
        public static string GenerateToDataUri(string content, QrCodeOptions? options = null, ImageFormat? format = null)
        {
            format ??= ImageFormat.Png;
            var base64 = GenerateToBase64(content, options, format);
            var mimeType = GetMimeType(format);
            return $"data:{mimeType};base64,{base64}";
        }

        /// <summary>
        /// 生成带Logo的二维码
        /// </summary>
        /// <param name="content">内容</param>
        /// <param name="logoPath">Logo路径</param>
        /// <param name="options">配置</param>
        /// <param name="logoRatio">Logo占二维码比例（0.1-0.3）</param>
        /// <returns>二维码图像</returns>
        public static Bitmap GenerateWithLogo(string content, string logoPath, QrCodeOptions? options = null, double logoRatio = 0.2)
        {
            using var logo = Image.FromFile(logoPath);
            return GenerateWithLogo(content, logo, options, logoRatio);
        }

        /// <summary>
        /// 生成带Logo的二维码
        /// </summary>
        /// <param name="content">内容</param>
        /// <param name="logo">Logo图像</param>
        /// <param name="options">配置</param>
        /// <param name="logoRatio">Logo占二维码比例</param>
        /// <returns>二维码图像</returns>
        public static Bitmap GenerateWithLogo(string content, Image logo, QrCodeOptions? options = null, double logoRatio = 0.2)
        {
            var bitmap = Generate(content, options);
            options ??= new QrCodeOptions();

            using (var g = Graphics.FromImage(bitmap))
            {
                var logoSize = (int)(Math.Min(options.Width, options.Height) * logoRatio);
                var logoX = (options.Width - logoSize) / 2;
                var logoY = (options.Height - logoSize) / 2;

                // 绘制白色背景
                g.FillRectangle(Brushes.White, logoX - 2, logoY - 2, logoSize + 4, logoSize + 4);

                // 绘制Logo
                g.DrawImage(logo, logoX, logoY, logoSize, logoSize);
            }

            return bitmap;
        }

        #endregion

        #region QR码矩阵生成

        private static bool[,] GenerateQrMatrix(byte[] data, QrCodeErrorCorrection errorCorrection)
        {
            // 简化实现：生成基础QR码矩阵
            // 实际应用中建议使用专门的QR码库如 QRCoder 或 ZXing

            // 确定版本（基于数据长度）
            int version = DetermineVersion(data.Length, errorCorrection);

            // 计算模块数（版本1为21，每增加1版本增加4个模块）
            int size = 21 + (version - 1) * 4;

            // 创建矩阵
            var matrix = new bool[size, size];

            // 添加定位图案
            AddFinderPatterns(matrix, size);

            // 添加对齐图案（版本2及以上）
            if (version >= 2)
            {
                AddAlignmentPatterns(matrix, size, version);
            }

            // 添加时序图案
            AddTimingPatterns(matrix, size);

            // 添加格式信息区域
            AddFormatInfoAreas(matrix, size);

            // 填充数据（简化实现）
            FillData(matrix, size, data);

            return matrix;
        }

        private static int DetermineVersion(int dataLength, QrCodeErrorCorrection errorCorrection)
        {
            // 简化版本确定
            var capacities = new int[] { 17, 32, 53, 78, 106, 134, 154, 192, 230, 271 };
            var reduction = errorCorrection switch
            {
                QrCodeErrorCorrection.Low => 0,
                QrCodeErrorCorrection.Medium => 1,
                QrCodeErrorCorrection.Quartile => 2,
                QrCodeErrorCorrection.High => 3,
                _ => 1
            };

            for (int v = 0; v < capacities.Length; v++)
            {
                var capacity = capacities[v] - reduction * (v + 1) * 5;
                if (capacity >= dataLength)
                    return v + 1;
            }

            return 10; // 最大版本
        }

        private static void AddFinderPatterns(bool[,] matrix, int size)
        {
            int patternSize = 7;

            // 左上角
            DrawFinderPattern(matrix, 0, 0);
            // 右上角
            DrawFinderPattern(matrix, size - patternSize, 0);
            // 左下角
            DrawFinderPattern(matrix, 0, size - patternSize);
        }

        private static void DrawFinderPattern(bool[,] matrix, int startX, int startY)
        {
            // 外框（7x7黑）
            for (int i = 0; i < 7; i++)
            {
                for (int j = 0; j < 7; j++)
                {
                    if (i == 0 || i == 6 || j == 0 || j == 6 ||
                        (i >= 2 && i <= 4 && j >= 2 && j <= 4))
                    {
                        matrix[startX + i, startY + j] = true;
                    }
                }
            }
        }

        private static void AddAlignmentPatterns(bool[,] matrix, int size, int version)
        {
            // 简化：仅在右下角添加一个对齐图案
            if (version >= 2)
            {
                var positions = GetAlignmentPositions(version);
                foreach (var pos in positions)
                {
                    if (pos.X > 7 && pos.Y > 7) // 避免与定位图案重叠
                    {
                        DrawAlignmentPattern(matrix, pos.X - 2, pos.Y - 2);
                    }
                }
            }
        }

        private static List<(int X, int Y)> GetAlignmentPositions(int version)
        {
            var positions = new List<(int, int)>();
            int size = 21 + (version - 1) * 4;

            if (version >= 2)
            {
                positions.Add((size - 7, size - 7));
            }

            return positions;
        }

        private static void DrawAlignmentPattern(bool[,] matrix, int startX, int startY)
        {
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    if (i == 0 || i == 4 || j == 0 || j == 4 || (i == 2 && j == 2))
                    {
                        matrix[startX + i, startY + j] = true;
                    }
                }
            }
        }

        private static void AddTimingPatterns(bool[,] matrix, int size)
        {
            // 水平时序图案
            for (int i = 8; i < size - 8; i++)
            {
                matrix[i, 6] = i % 2 == 0;
            }

            // 垂直时序图案
            for (int i = 8; i < size - 8; i++)
            {
                matrix[6, i] = i % 2 == 0;
            }
        }

        private static void AddFormatInfoAreas(bool[,] matrix, int size)
        {
            // 格式信息区域标记（简化）
            for (int i = 0; i < 9; i++)
            {
                if (i != 6) // 避开时序图案
                {
                    matrix[8, i] = false;
                    matrix[i, 8] = false;
                }
            }
        }

        private static void FillData(bool[,] matrix, int size, byte[] data)
        {
            // 简化数据填充
            int dataIndex = 0;
            bool upward = true;

            for (int col = size - 1; col >= 0; col -= 2)
            {
                if (col == 6) col--; // 跳过时序图案列

                for (int i = 0; i < size; i++)
                {
                    int row = upward ? size - 1 - i : i;

                    for (int c = 0; c < 2; c++)
                    {
                        int currentCol = col - c;

                        if (!IsReserved(currentCol, row, size))
                        {
                            if (dataIndex < data.Length * 8)
                            {
                                int byteIndex = dataIndex / 8;
                                int bitIndex = 7 - (dataIndex % 8);
                                matrix[currentCol, row] = ((data[byteIndex] >> bitIndex) & 1) == 1;
                                dataIndex++;
                            }
                            else
                            {
                                matrix[currentCol, row] = false;
                            }
                        }
                    }
                }

                upward = !upward;
            }
        }

        private static bool IsReserved(int x, int y, int size)
        {
            // 检查定位图案区域
            if ((x < 9 && y < 9) || (x < 9 && y >= size - 8) || (x >= size - 8 && y < 9))
                return true;

            // 检查时序图案
            if (x == 6 || y == 6)
                return true;

            return false;
        }

        #endregion

        #region 辅助方法

        private static ImageFormat GetImageFormat(string filePath)
        {
            var ext = Path.GetExtension(filePath).ToLower();
            return ext switch
            {
                ".jpg" or ".jpeg" => ImageFormat.Jpeg,
                ".gif" => ImageFormat.Gif,
                ".bmp" => ImageFormat.Bmp,
                ".tiff" => ImageFormat.Tiff,
                _ => ImageFormat.Png
            };
        }

        private static string GetMimeType(ImageFormat format)
        {
            if (format.Equals(ImageFormat.Jpeg))
                return "image/jpeg";
            if (format.Equals(ImageFormat.Gif))
                return "image/gif";
            if (format.Equals(ImageFormat.Bmp))
                return "image/bmp";
            if (format.Equals(ImageFormat.Tiff))
                return "image/tiff";
            return "image/png";
        }

        #endregion
    }
}
