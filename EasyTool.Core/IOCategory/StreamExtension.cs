using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EasyTool.Extension
{
    /// <summary>
    /// Stream 扩展方法
    /// </summary>
    public static class StreamExtension
    {
        #region 读取操作

        /// <summary>
        /// 读取流中的所有字节
        /// </summary>
        public static byte[] ReadAllBytes(this Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            return ms.ToArray();
        }

        /// <summary>
        /// 异步读取流中的所有字节
        /// </summary>
        public static async Task<byte[]> ReadAllBytesAsync(this Stream stream, CancellationToken cancellationToken = default)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms, cancellationToken);
            return ms.ToArray();
        }

        /// <summary>
        /// 读取流中的所有文本（UTF-8 编码）
        /// </summary>
        public static string ReadAllText(this Stream stream)
        {
            return stream.ReadAllText(Encoding.UTF8);
        }

        /// <summary>
        /// 读取流中的所有文本
        /// </summary>
        public static string ReadAllText(this Stream stream, Encoding encoding)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            encoding ??= Encoding.UTF8;

            using var reader = new StreamReader(stream, encoding, true);
            return reader.ReadToEnd();
        }

        /// <summary>
        /// 异步读取流中的所有文本（UTF-8 编码）
        /// </summary>
        public static Task<string> ReadAllTextAsync(this Stream stream, CancellationToken cancellationToken = default)
        {
            return stream.ReadAllTextAsync(Encoding.UTF8, cancellationToken);
        }

        /// <summary>
        /// 异步读取流中的所有文本
        /// </summary>
        public static async Task<string> ReadAllTextAsync(this Stream stream, Encoding encoding, CancellationToken cancellationToken = default)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            encoding ??= Encoding.UTF8;

            using var reader = new StreamReader(stream, encoding, true);
            return await reader.ReadToEndAsync();
        }

        /// <summary>
        /// 读取流中的所有行
        /// </summary>
        public static string[] ReadAllLines(this Stream stream, Encoding? encoding = null)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            encoding ??= Encoding.UTF8;

            using var reader = new StreamReader(stream, encoding, true);
            var lines = new System.Collections.Generic.List<string>();
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                lines.Add(line);
            }
            return lines.ToArray();
        }

        #endregion

        #region 写入操作

        /// <summary>
        /// 将字节写入流
        /// </summary>
        public static void WriteBytes(this Stream stream, byte[] bytes)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (bytes == null || bytes.Length == 0)
                return;

            stream.Write(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// 异步将字节写入流
        /// </summary>
        public static async Task WriteBytesAsync(this Stream stream, byte[] bytes, CancellationToken cancellationToken = default)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (bytes == null || bytes.Length == 0)
                return;

            await stream.WriteAsync(bytes, 0, bytes.Length, cancellationToken);
        }

        /// <summary>
        /// 将文本写入流（UTF-8 编码）
        /// </summary>
        public static void WriteText(this Stream stream, string text)
        {
            stream.WriteText(text, Encoding.UTF8);
        }

        /// <summary>
        /// 将文本写入流
        /// </summary>
        public static void WriteText(this Stream stream, string text, Encoding encoding)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (string.IsNullOrEmpty(text))
                return;

            encoding ??= Encoding.UTF8;

            var bytes = encoding.GetBytes(text);
            stream.Write(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// 异步将文本写入流（UTF-8 编码）
        /// </summary>
        public static Task WriteTextAsync(this Stream stream, string text, CancellationToken cancellationToken = default)
        {
            return stream.WriteTextAsync(text, Encoding.UTF8, cancellationToken);
        }

        /// <summary>
        /// 异步将文本写入流
        /// </summary>
        public static async Task WriteTextAsync(this Stream stream, string text, Encoding encoding, CancellationToken cancellationToken = default)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (string.IsNullOrEmpty(text))
                return;

            encoding ??= Encoding.UTF8;

            var bytes = encoding.GetBytes(text);
            await stream.WriteAsync(bytes, 0, bytes.Length, cancellationToken);
        }

        #endregion

        #region 复制操作


        /// <summary>
        /// 将流复制到字节数组
        /// </summary>
        public static byte[] CopyToByteArray(this Stream stream)
        {
            return stream.ReadAllBytes();
        }

        /// <summary>
        /// 将流复制到内存流
        /// </summary>
        public static MemoryStream CopyToMemoryStream(this Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            var ms = new MemoryStream();
            stream.CopyTo(ms);
            return ms;
        }

        #endregion

        #region 位置操作


        #endregion

        #region 转换操作

        /// <summary>
        /// 将流转为 Base64 字符串
        /// </summary>
        public static string ToBase64(this Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            var bytes = stream.ReadAllBytes();
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// 将流转为十六进制字符串
        /// </summary>
        public static string ToHex(this Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            var bytes = stream.ReadAllBytes();
            return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
        }

        #endregion

        #region 检查操作

        /// <summary>
        /// 判断流是否为空
        /// </summary>
        public static bool IsEmpty(this Stream? stream)
        {
            if (stream == null)
                return true;

            if (stream.CanSeek)
            {
                return stream.Length == 0;
            }

            return stream.ReadByte() == -1;
        }

        #endregion

        #region 缓冲操作

        /// <summary>
        /// 使用缓冲读取器包装流
        /// </summary>
        public static BufferedStream Buffer(this Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            return new BufferedStream(stream);
        }

        /// <summary>
        /// 使用缓冲读取器包装流（指定缓冲区大小）
        /// </summary>
        public static BufferedStream Buffer(this Stream stream, int bufferSize)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            return new BufferedStream(stream, bufferSize);
        }

        #endregion
    }
}
