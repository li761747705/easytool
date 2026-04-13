using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EasyTool.BusinessCategory
{
    /// <summary>
    /// PDF工具类
    /// 提供PDF生成、合并、拆分、水印等功能
    /// 注意：需要安装 iTextSharp 或 PdfSharp 等第三方库
    /// </summary>
    public static class PdfUtil
    {
        #region PDF信息

        /// <summary>
        /// 获取PDF文件信息
        /// </summary>
        /// <param name="filePath">PDF文件路径</param>
        /// <returns>PDF信息</returns>
        public static PdfInfo? GetPdfInfo(string filePath)
        {
            if (!File.Exists(filePath))
                return null;

            try
            {
                var fileInfo = new FileInfo(filePath);
                return new PdfInfo
                {
                    FileName = fileInfo.Name,
                    FilePath = filePath,
                    FileSize = fileInfo.Length,
                    CreateTime = fileInfo.CreationTime,
                    ModifyTime = fileInfo.LastWriteTime
                };
            }
            catch (IOException)
            {
                // 文件信息读取失败时返回null
                return null;
            }
        }

        /// <summary>
        /// PDF文件信息
        /// </summary>
        public class PdfInfo
        {
            /// <summary>
            /// 文件名
            /// </summary>
            public string FileName { get; set; } = string.Empty;

            /// <summary>
            /// 文件路径
            /// </summary>
            public string FilePath { get; set; } = string.Empty;

            /// <summary>
            /// 文件大小（字节）
            /// </summary>
            public long FileSize { get; set; }

            /// <summary>
            /// 创建时间
            /// </summary>
            public DateTime CreateTime { get; set; }

            /// <summary>
            /// 修改时间
            /// </summary>
            public DateTime ModifyTime { get; set; }

            /// <summary>
            /// 页数
            /// </summary>
            public int PageCount { get; set; }
        }

        #endregion

        #region 合并PDF

        /// <summary>
        /// 合并多个PDF文件
        /// </summary>
        /// <param name="pdfFiles">PDF文件路径列表</param>
        /// <param name="outputPath">输出文件路径</param>
        /// <returns>是否成功</returns>
        /// <remarks>
        /// 需要使用第三方库实现，示例代码：
        /// <code>
        /// // 使用 iTextSharp
        /// using (var stream = new FileStream(outputPath, FileMode.Create))
        /// using (var document = new Document())
        /// using (var writer = new PdfCopy(document, stream))
        /// {
        ///     document.Open();
        ///     foreach (var file in pdfFiles)
        ///     {
        ///         using (var reader = new PdfReader(file))
        ///         {
        ///             for (int i = 1; i &lt;= reader.NumberOfPages; i++)
        ///             {
        ///                 writer.AddPage(writer.GetImportedPage(reader, i));
        ///             }
        ///         }
        ///     }
        /// }
        /// </code>
        /// </remarks>
        [Obsolete("此功能尚未实现，请安装 iTextSharp 或 PdfSharp NuGet 包")]
        public static bool MergePdf(List<string> pdfFiles, string outputPath)
        {
            if (pdfFiles == null || pdfFiles.Count == 0)
                return false;

            // 检查所有文件是否存在
            if (!pdfFiles.All(File.Exists))
                return false;

            throw new NotSupportedException(
                "请安装 iTextSharp 或 PdfSharp NuGet 包以启用此功能。" +
                "建议安装：Install-Package iTextSharp 或 Install-Package PdfSharp");
        }

        #endregion

        #region 拆分PDF

        /// <summary>
        /// 拆分PDF文件
        /// </summary>
        /// <param name="sourcePath">源PDF文件路径</param>
        /// <param name="outputDirectory">输出目录</param>
        /// <param name="pagesPerFile">每个文件的页数</param>
        /// <returns>拆分后的文件列表</returns>
        [Obsolete("此功能尚未实现，请安装 iTextSharp 或 PdfSharp NuGet 包")]
        public static List<string> SplitPdf(string sourcePath, string outputDirectory, int pagesPerFile = 1)
        {
            var result = new List<string>();

            if (!File.Exists(sourcePath))
                return result;

            Directory.CreateDirectory(outputDirectory);
            throw new NotSupportedException("请安装 iTextSharp 或 PdfSharp NuGet 包以启用此功能");
        }

        /// <summary>
        /// 提取PDF指定页面
        /// </summary>
        /// <param name="sourcePath">源PDF文件路径</param>
        /// <param name="outputPath">输出文件路径</param>
        /// <param name="startPage">起始页码</param>
        /// <param name="endPage">结束页码</param>
        /// <returns>是否成功</returns>
        [Obsolete("此功能尚未实现，请安装 iTextSharp 或 PdfSharp NuGet 包")]
        public static bool ExtractPages(string sourcePath, string outputPath, int startPage, int endPage)
        {
            if (!File.Exists(sourcePath))
                return false;

            if (startPage < 1 || endPage < startPage)
                return false;

            throw new NotSupportedException("请安装 iTextSharp 或 PdfSharp NuGet 包以启用此功能");
        }

        #endregion

        #region 水印

        /// <summary>
        /// 添加文字水印
        /// </summary>
        /// <param name="sourcePath">源PDF文件路径</param>
        /// <param name="outputPath">输出文件路径</param>
        /// <param name="watermarkText">水印文字</param>
        /// <param name="fontSize">字体大小</param>
        /// <param name="opacity">透明度（0-1）</param>
        /// <param name="rotation">旋转角度</param>
        /// <returns>是否成功</returns>
        [Obsolete("此功能尚未实现，请安装 iTextSharp 或 PdfSharp NuGet 包")]
        public static bool AddTextWatermark(
            string sourcePath,
            string outputPath,
            string watermarkText,
            int fontSize = 50,
            float opacity = 0.3f,
            int rotation = 45)
        {
            if (!File.Exists(sourcePath))
                return false;

            if (string.IsNullOrEmpty(watermarkText))
                return false;

            throw new NotSupportedException("请安装 iTextSharp 或 PdfSharp NuGet 包以启用此功能");
        }

        /// <summary>
        /// 添加图片水印
        /// </summary>
        /// <param name="sourcePath">源PDF文件路径</param>
        /// <param name="outputPath">输出文件路径</param>
        /// <param name="watermarkImagePath">水印图片路径</param>
        /// <param name="opacity">透明度（0-1）</param>
        /// <returns>是否成功</returns>
        [Obsolete("此功能尚未实现，请安装 iTextSharp 或 PdfSharp NuGet 包")]
        public static bool AddImageWatermark(
            string sourcePath,
            string outputPath,
            string watermarkImagePath,
            float opacity = 0.3f)
        {
            if (!File.Exists(sourcePath) || !File.Exists(watermarkImagePath))
                return false;

            throw new NotSupportedException("请安装 iTextSharp 或 PdfSharp NuGet 包以启用此功能");
        }

        #endregion

        #region PDF转图片

        /// <summary>
        /// 将PDF页面转换为图片
        /// </summary>
        /// <param name="pdfPath">PDF文件路径</param>
        /// <param name="outputDirectory">输出目录</param>
        /// <param name="imageFormat">图片格式</param>
        /// <param name="dpi">分辨率</param>
        /// <returns>生成的图片路径列表</returns>
        [Obsolete("此功能尚未实现，请安装 PdfiumViewer 或 Ghostscript NuGet 包")]
        public static List<string> ToImages(
            string pdfPath,
            string outputDirectory,
            string imageFormat = "png",
            int dpi = 150)
        {
            var result = new List<string>();

            if (!File.Exists(pdfPath))
                return result;

            Directory.CreateDirectory(outputDirectory);
            throw new NotSupportedException("请安装 PdfiumViewer 或 Ghostscript NuGet 包以启用此功能");
        }

        #endregion

        #region 文本提取

        /// <summary>
        /// 提取PDF文本内容
        /// </summary>
        /// <param name="pdfPath">PDF文件路径</param>
        /// <returns>文本内容</returns>
        [Obsolete("此功能尚未实现，请安装 iTextSharp NuGet 包")]
        public static string ExtractText(string pdfPath)
        {
            if (!File.Exists(pdfPath))
                return string.Empty;

            throw new NotSupportedException("请安装 iTextSharp NuGet 包以启用此功能");
        }

        #endregion
    }
}