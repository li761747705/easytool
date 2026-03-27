using System;
using System.Collections.Generic;

namespace EasyTool.IOCategory
{
    /// <summary>
    /// 图片元数据工具类
    /// </summary>
    public static class ImageMetadataUtil
    {
        /// <summary>
        /// 读取图片EXIF信息
        /// </summary>
        public static ExifData ReadExif(string imagePath)
        {
            var exif = new ExifData();

            try
            {
                using var image = System.Drawing.Image.FromFile(imagePath);
                var propertyItems = image.PropertyItems;

                foreach (var item in propertyItems)
                {
                    var value = ParsePropertyItemValue(item);
                    var tagName = GetPropertyName(item.Id);

                    switch (item.Id)
                    {
                        case 0x010F: // 制造商
                            exif.Make = value;
                            break;
                        case 0x0110: // 型号
                            exif.Model = value;
                            break;
                        case 0x0112: // 方向
                            exif.Orientation = ParseOrientation(value);
                            break;
                        case 0x011A: // X分辨率
                        case 0x011B: // Y分辨率
                            break;
                        case 0x0128: // 分辨率单位
                            break;
                        case 0x0131: // 软件
                            exif.Software = value;
                            break;
                        case 0x0132: // 日期时间
                            exif.DateTime = ParseDateTime(value);
                            break;
                        case 0x8769: // Exif IFD
                            break;
                        case 0x8827: // ISO速度
                            exif.ISO = ParseInt(value);
                            break;
                        case 0x9003: // 原始日期时间
                            exif.DateTimeOriginal = ParseDateTime(value);
                            break;
                        case 0x9004: // 数字化日期时间
                            exif.DateTimeDigitized = ParseDateTime(value);
                            break;
                        case 0x920A: // 焦距
                            exif.FocalLength = ParseRational(value);
                            break;
                        case 0x9207: // 光圈值
                            break;
                        case 0x829A: // 曝光时间
                            exif.ExposureTime = ParseRational(value);
                            break;
                        case 0x829D: // F值
                            exif.FNumber = ParseRational(value);
                            break;
                        case 0x8825: // GPS信息
                            break;
                        case 0xA002: // 图像宽度
                            exif.ExifImageWidth = ParseInt(value);
                            break;
                        case 0xA003: // 图像高度
                            exif.ExifImageHeight = ParseInt(value);
                            break;
                        case 0xA402: // 曝光模式
                            break;
                        case 0xA403: // 白平衡
                            break;
                        case 0xA406: // 场景拍摄类型
                            break;
                        case 0xA420: // 图像唯一ID
                            exif.ImageUniqueID = value;
                            break;
                    }

                    exif.AllProperties[tagName] = value;
                }
            }
            catch
            {
            }

            return exif;
        }

        /// <summary>
        /// 移除EXIF信息
        /// </summary>
        public static bool RemoveExif(string sourcePath, string destinationPath)
        {
            try
            {
                using var image = System.Drawing.Image.FromFile(sourcePath);
                
                // 创建没有EXIF的新图像
                using var newImage = new System.Drawing.Bitmap(image);
                newImage.Save(destinationPath);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static string ParsePropertyItemValue(System.Drawing.Imaging.PropertyItem item)
        {
            try
            {
                switch (item.Type)
                {
                    case 1: // Byte
                        return BitConverter.ToString(item.Value).Replace("-", " ");
                    case 2: // ASCII
                        return System.Text.Encoding.ASCII.GetString(item.Value).TrimEnd('\0');
                    case 3: // Short
                        return BitConverter.ToUInt16(item.Value, 0).ToString();
                    case 4: // Long
                        return BitConverter.ToUInt32(item.Value, 0).ToString();
                    case 5: // Rational
                        return ParseRational(item.Value).ToString();
                    case 7: // Undefined
                        return BitConverter.ToString(item.Value).Replace("-", " ");
                    case 9: // SLong
                        return BitConverter.ToInt32(item.Value, 0).ToString();
                    case 10: // SRational
                        return ParseRational(item.Value).ToString();
                    default:
                        return BitConverter.ToString(item.Value);
                }
            }
            catch
            {
                return "";
            }
        }

        private static double ParseRational(byte[] value)
        {
            if (value.Length < 8) return 0;
            var numerator = BitConverter.ToUInt32(value, 0);
            var denominator = BitConverter.ToUInt32(value, 4);
            return denominator != 0 ? (double)numerator / denominator : 0;
        }

        private static double ParseRational(string value)
        {
            if (string.IsNullOrEmpty(value)) return 0;
            if (double.TryParse(value, out var result)) return result;
            return 0;
        }

        private static int ParseInt(string value)
        {
            return int.TryParse(value, out var result) ? result : 0;
        }

        private static DateTime? ParseDateTime(string value)
        {
            if (string.IsNullOrEmpty(value)) return null;
            // EXIF日期格式: "yyyy:MM:dd HH:mm:ss"
            if (DateTime.TryParseExact(value, "yyyy:MM:dd HH:mm:ss", 
                System.Globalization.CultureInfo.InvariantCulture, 
                System.Globalization.DateTimeStyles.None, out var result))
            {
                return result;
            }
            return null;
        }

        private static int ParseOrientation(string value)
        {
            return int.TryParse(value, out var result) ? result : 1;
        }

        private static string GetPropertyName(int id)
        {
            return id switch
            {
                0x0100 => "ImageWidth",
                0x0101 => "ImageLength",
                0x0102 => "BitsPerSample",
                0x0103 => "Compression",
                0x0106 => "PhotometricInterpretation",
                0x010E => "ImageDescription",
                0x010F => "Make",
                0x0110 => "Model",
                0x0111 => "StripOffsets",
                0x0112 => "Orientation",
                0x0115 => "SamplesPerPixel",
                0x0116 => "RowsPerStrip",
                0x0117 => "StripByteCounts",
                0x011A => "XResolution",
                0x011B => "YResolution",
                0x0128 => "ResolutionUnit",
                0x0131 => "Software",
                0x0132 => "DateTime",
                0x8769 => "ExifIFDPointer",
                0x8827 => "ISOSpeedRatings",
                0x9003 => "DateTimeOriginal",
                0x9004 => "DateTimeDigitized",
                0x920A => "FocalLength",
                0x829A => "ExposureTime",
                0x829D => "FNumber",
                0xA002 => "ExifImageWidth",
                0xA003 => "ExifImageHeight",
                _ => $"0x{id:X4}"
            };
        }
    }

    /// <summary>
    /// EXIF数据
    /// </summary>
    public class ExifData
    {
        /// <summary>
        /// 制造商
        /// </summary>
        public string Make { get; set; } = "";

        /// <summary>
        /// 型号
        /// </summary>
        public string Model { get; set; } = "";

        /// <summary>
        /// 软件
        /// </summary>
        public string Software { get; set; } = "";

        /// <summary>
        /// 方向（1-8）
        /// </summary>
        public int Orientation { get; set; } = 1;

        /// <summary>
        /// 日期时间
        /// </summary>
        public DateTime? DateTime { get; set; }

        /// <summary>
        /// 原始日期时间
        /// </summary>
        public DateTime? DateTimeOriginal { get; set; }

        /// <summary>
        /// 数字化日期时间
        /// </summary>
        public DateTime? DateTimeDigitized { get; set; }

        /// <summary>
        /// ISO感光度
        /// </summary>
        public int ISO { get; set; }

        /// <summary>
        /// 焦距
        /// </summary>
        public double FocalLength { get; set; }

        /// <summary>
        /// 曝光时间
        /// </summary>
        public double ExposureTime { get; set; }

        /// <summary>
        /// 光圈值
        /// </summary>
        public double FNumber { get; set; }

        /// <summary>
        /// EXIF图像宽度
        /// </summary>
        public int ExifImageWidth { get; set; }

        /// <summary>
        /// EXIF图像高度
        /// </summary>
        public int ExifImageHeight { get; set; }

        /// <summary>
        /// 图像唯一ID
        /// </summary>
        public string ImageUniqueID { get; set; } = "";

        /// <summary>
        /// 所有属性
        /// </summary>
        public Dictionary<string, string> AllProperties { get; } = new();

        /// <summary>
        /// 获取方向描述
        /// </summary>
        public string OrientationDescription => Orientation switch
        {
            1 => "正常",
            2 => "水平翻转",
            3 => "旋转180度",
            4 => "垂直翻转",
            5 => "逆时针90度+水平翻转",
            6 => "顺时针90度",
            7 => "顺时针90度+水平翻转",
            8 => "逆时针90度",
            _ => "未知"
        };

        /// <summary>
        /// 曝光时间显示
        /// </summary>
        public string ExposureTimeDisplay
        {
            get
            {
                if (ExposureTime >= 1)
                    return $"{ExposureTime:F1}s";
                return $"1/{(int)(1 / ExposureTime)}s";
            }
        }

        /// <summary>
        /// 光圈显示
        /// </summary>
        public string FNumberDisplay => FNumber > 0 ? $"f/{FNumber:F1}" : "";

        /// <summary>
        /// 焦距显示
        /// </summary>
        public string FocalLengthDisplay => FocalLength > 0 ? $"{FocalLength:F1}mm" : "";
    }
}
