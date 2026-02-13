using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EasyTool.Extension
{
    /// <summary>
    /// 文件类型扩展方法
    /// </summary>
    public static class FileTypeExtension
    {
        /// <summary>
        /// 文件流头部信息获得文件类型
        ///
        /// 说明：
        ///     1、无法识别类型默认按照扩展名识别
        ///     2、xls、doc、msi、ppt、vsd头信息无法区分，按照扩展名区分
        ///     3、zip可能为docx、xlsx、pptx、jar、war头信息无法区分，按照扩展名区分
        /// </summary>
        /// <param name="file">文件</param>
        /// <returns>类型，文件的扩展名，未找到为null</returns>
        public static string GetFileType(this FileInfo file)
        {
            byte[] buffer = new byte[256];
            using (FileStream fs = file.OpenRead())
            {
                int readLength = fs.Read(buffer, 0, buffer.Length);
                if (readLength < buffer.Length)
                {
                    // 处理读取不足的情况，虽然对于头部检测通常前几个字节就够了，但为了严谨性
                }
            }

            string header = "";
            for (int i = 0; i < buffer.Length; i++)
            {
                header += buffer[i].ToString();
            }

            string? type = null;
            switch (header)
            {
                case "255216": // jpg
                    type = ".jpg";
                    break;
                case "13780": // png
                    type = ".png";
                    break;
                case "7173": // gif
                    type = ".gif";
                    break;
                case "6677": // bmp
                    type = ".bmp";
                    break;
                case "7790": // exe dll
                    type = ".exe";
                    break;
                case "6063": // xml
                    type = ".xml";
                    break;
                case "6033": // htm html
                    type = ".html";
                    break;
                case "4742": // js
                    type = ".js";
                    break;
                case "5144": // txt
                    type = ".txt";
                    break;
                default:
                case "8297": // rar
                case "8075": // zip
                case "D0CF11E0": // doc xls ppt vsd
                    type = file.Extension;
                    break;
            }

            return type;
        }
    }
}
