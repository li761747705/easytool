using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EasyTool.SystemCategory
{
    /// <summary>
    /// 剪贴板工具类
    /// 提供剪贴板的读写操作功能
    /// </summary>
    public static class ClipboardUtil
    {
        #region 文本操作

        /// <summary>
        /// 设置剪贴板文本
        /// </summary>
        /// <param name="text">文本内容</param>
        public static void SetText(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                Clear();
                return;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                WindowsClipboard.SetText(text);
            }
            else
            {
                throw new PlatformNotSupportedException("当前平台不支持剪贴板操作");
            }
        }

        /// <summary>
        /// 获取剪贴板文本
        /// </summary>
        /// <returns>文本内容</returns>
        public static string? GetText()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return WindowsClipboard.GetText();
            }
            throw new PlatformNotSupportedException("当前平台不支持剪贴板操作");
        }

        /// <summary>
        /// 检查剪贴板是否包含文本
        /// </summary>
        /// <returns>是否包含文本</returns>
        public static bool ContainsText()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return WindowsClipboard.ContainsText();
            }
            return false;
        }

        /// <summary>
        /// 异步设置剪贴板文本
        /// </summary>
        /// <param name="text">文本内容</param>
        public static async Task SetTextAsync(string text)
        {
            await Task.Run(() => SetText(text)).ConfigureAwait(false);
        }

        /// <summary>
        /// 异步获取剪贴板文本
        /// </summary>
        /// <returns>文本内容</returns>
        public static async Task<string?> GetTextAsync()
        {
            return await Task.Run(() => GetText()).ConfigureAwait(false);
        }

        #endregion

        #region 图像操作

        /// <summary>
        /// 设置剪贴板图像数据
        /// </summary>
        /// <param name="imageData">图像数据（如 PNG、BMP 格式）</param>
        public static void SetImageData(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0)
                throw new ArgumentNullException(nameof(imageData));

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                WindowsClipboard.SetImageData(imageData);
            }
            else
            {
                throw new PlatformNotSupportedException("当前平台不支持剪贴板操作");
            }
        }

        /// <summary>
        /// 设置剪贴板图像（从文件）
        /// </summary>
        /// <param name="imagePath">图像文件路径</param>
        public static void SetImageFromFile(string imagePath)
        {
            if (!File.Exists(imagePath))
                throw new FileNotFoundException("图像文件不存在", imagePath);

            var imageData = File.ReadAllBytes(imagePath);
            SetImageData(imageData);
        }

        /// <summary>
        /// 获取剪贴板图像数据
        /// </summary>
        /// <returns>图像数据</returns>
        public static byte[]? GetImageData()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return WindowsClipboard.GetImageData();
            }
            throw new PlatformNotSupportedException("当前平台不支持剪贴板操作");
        }

        /// <summary>
        /// 检查剪贴板是否包含图像
        /// </summary>
        /// <returns>是否包含图像</returns>
        public static bool ContainsImage()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return WindowsClipboard.ContainsImage();
            }
            return false;
        }

        /// <summary>
        /// 保存剪贴板图像到文件
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>是否保存成功</returns>
        public static bool SaveImageToFile(string filePath)
        {
            var imageData = GetImageData();
            if (imageData == null || imageData.Length == 0)
                return false;

            try
            {
                File.WriteAllBytes(filePath, imageData);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region 文件操作

        /// <summary>
        /// 设置剪贴板文件列表
        /// </summary>
        /// <param name="filePaths">文件路径列表</param>
        public static void SetFiles(params string[] filePaths)
        {
            if (filePaths == null || filePaths.Length == 0)
            {
                Clear();
                return;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                WindowsClipboard.SetFiles(filePaths);
            }
            else
            {
                throw new PlatformNotSupportedException("当前平台不支持剪贴板操作");
            }
        }

        /// <summary>
        /// 获取剪贴板文件列表
        /// </summary>
        /// <returns>文件路径列表</returns>
        public static string[]? GetFiles()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return WindowsClipboard.GetFiles();
            }
            throw new PlatformNotSupportedException("当前平台不支持剪贴板操作");
        }

        /// <summary>
        /// 检查剪贴板是否包含文件
        /// </summary>
        /// <returns>是否包含文件</returns>
        public static bool ContainsFiles()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return WindowsClipboard.ContainsFiles();
            }
            return false;
        }

        #endregion

        #region 通用操作

        /// <summary>
        /// 清空剪贴板
        /// </summary>
        public static void Clear()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                WindowsClipboard.Clear();
            }
        }

        /// <summary>
        /// 检查剪贴板是否为空
        /// </summary>
        /// <returns>是否为空</returns>
        public static bool IsEmpty()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return WindowsClipboard.IsEmpty();
            }
            return true;
        }

        #endregion
    }

    /// <summary>
    /// Windows 平台剪贴板实现
    /// </summary>
    internal static class WindowsClipboard
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool OpenClipboard(IntPtr hWndNewOwner);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool CloseClipboard();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool EmptyClipboard();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetClipboardData(uint uFormat, IntPtr hMem);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetClipboardData(uint uFormat);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool IsClipboardFormatAvailable(uint uFormat);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GlobalAlloc(uint uFlags, UIntPtr dwBytes);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GlobalLock(IntPtr hMem);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GlobalUnlock(IntPtr hMem);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern UIntPtr GlobalSize(IntPtr hMem);

        [DllImport("msvcrt.dll", SetLastError = true)]
        private static extern IntPtr memcpy(IntPtr dest, IntPtr src, UIntPtr count);

        private const uint CF_TEXT = 1;
        private const uint CF_UNICODETEXT = 13;
        private const uint CF_BITMAP = 2;
        private const uint CF_DIB = 8;
        private const uint CF_HDROP = 15;
        private const uint GMEM_MOVEABLE = 0x0002;
        private const uint GMEM_ZEROINIT = 0x0040;
        private const uint GHND = GMEM_MOVEABLE | GMEM_ZEROINIT;

        public static void SetText(string text)
        {
            if (!OpenClipboard(IntPtr.Zero))
                throw new InvalidOperationException("无法打开剪贴板");

            try
            {
                EmptyClipboard();

                var bytes = Encoding.Unicode.GetBytes(text + "\0");
                var hMem = GlobalAlloc(GHND, (UIntPtr)bytes.Length);

                if (hMem == IntPtr.Zero)
                    throw new InvalidOperationException("内存分配失败");

                var ptr = GlobalLock(hMem);
                if (ptr == IntPtr.Zero)
                    throw new InvalidOperationException("内存锁定失败");

                try
                {
                    Marshal.Copy(bytes, 0, ptr, bytes.Length);
                }
                finally
                {
                    GlobalUnlock(hMem);
                }

                if (SetClipboardData(CF_UNICODETEXT, hMem) == IntPtr.Zero)
                    throw new InvalidOperationException("设置剪贴板数据失败");
            }
            finally
            {
                CloseClipboard();
            }
        }

        public static string? GetText()
        {
            if (!IsClipboardFormatAvailable(CF_UNICODETEXT))
                return null;

            if (!OpenClipboard(IntPtr.Zero))
                return null;

            try
            {
                var hMem = GetClipboardData(CF_UNICODETEXT);
                if (hMem == IntPtr.Zero)
                    return null;

                var ptr = GlobalLock(hMem);
                if (ptr == IntPtr.Zero)
                    return null;

                try
                {
                    var size = GlobalSize(hMem);
                    if (size == UIntPtr.Zero)
                        return null;

                    var bytes = new byte[(int)size];
                    Marshal.Copy(ptr, bytes, 0, bytes.Length);

                    var text = Encoding.Unicode.GetString(bytes);
                    var nullIndex = text.IndexOf('\0');
                    return nullIndex >= 0 ? text.Substring(0, nullIndex) : text;
                }
                finally
                {
                    GlobalUnlock(hMem);
                }
            }
            finally
            {
                CloseClipboard();
            }
        }

        public static bool ContainsText()
        {
            return IsClipboardFormatAvailable(CF_UNICODETEXT) || IsClipboardFormatAvailable(CF_TEXT);
        }

        public static void SetImageData(byte[] imageData)
        {
            if (!OpenClipboard(IntPtr.Zero))
                throw new InvalidOperationException("无法打开剪贴板");

            try
            {
                EmptyClipboard();

                // 将图像数据放入剪贴板（使用 DIB 格式）
                var hMem = GlobalAlloc(GHND, (UIntPtr)imageData.Length);
                if (hMem == IntPtr.Zero)
                    throw new InvalidOperationException("内存分配失败");

                var ptr = GlobalLock(hMem);
                if (ptr == IntPtr.Zero)
                    throw new InvalidOperationException("内存锁定失败");

                try
                {
                    Marshal.Copy(imageData, 0, ptr, imageData.Length);
                }
                finally
                {
                    GlobalUnlock(hMem);
                }

                if (SetClipboardData(CF_DIB, hMem) == IntPtr.Zero)
                    throw new InvalidOperationException("设置剪贴板图像失败");
            }
            finally
            {
                CloseClipboard();
            }
        }

        public static byte[]? GetImageData()
        {
            if (!IsClipboardFormatAvailable(CF_DIB) && !IsClipboardFormatAvailable(CF_BITMAP))
                return null;

            if (!OpenClipboard(IntPtr.Zero))
                return null;

            try
            {
                var hMem = GetClipboardData(CF_DIB);
                if (hMem == IntPtr.Zero)
                    return null;

                var ptr = GlobalLock(hMem);
                if (ptr == IntPtr.Zero)
                    return null;

                try
                {
                    var size = GlobalSize(hMem);
                    var data = new byte[(int)size];
                    Marshal.Copy(ptr, data, 0, (int)size);
                    return data;
                }
                finally
                {
                    GlobalUnlock(hMem);
                }
            }
            finally
            {
                CloseClipboard();
            }
        }

        public static bool ContainsImage()
        {
            return IsClipboardFormatAvailable(CF_BITMAP) || IsClipboardFormatAvailable(CF_DIB);
        }

        public static void SetFiles(string[] filePaths)
        {
            if (!OpenClipboard(IntPtr.Zero))
                throw new InvalidOperationException("无法打开剪贴板");

            try
            {
                EmptyClipboard();

                // 构建 DROP 结构
                var dropList = new DROPFILES();
                var filePathsStr = string.Join("\0", filePaths) + "\0\0";
                var bytes = Encoding.Unicode.GetBytes(filePathsStr);

                dropList.pFiles = Marshal.SizeOf(typeof(DROPFILES));
                dropList.fWide = true;

                var totalSize = Marshal.SizeOf(typeof(DROPFILES)) + bytes.Length;
                var hMem = GlobalAlloc(GHND, (UIntPtr)totalSize);

                if (hMem == IntPtr.Zero)
                    throw new InvalidOperationException("内存分配失败");

                var ptr = GlobalLock(hMem);
                if (ptr == IntPtr.Zero)
                    throw new InvalidOperationException("内存锁定失败");

                try
                {
                    // 写入 DROPFILES 结构
                    Marshal.StructureToPtr(dropList, ptr, false);
                    // 写入文件路径
                    Marshal.Copy(bytes, 0, ptr + Marshal.SizeOf(typeof(DROPFILES)), bytes.Length);
                }
                finally
                {
                    GlobalUnlock(hMem);
                }

                if (SetClipboardData(CF_HDROP, hMem) == IntPtr.Zero)
                    throw new InvalidOperationException("设置剪贴板文件列表失败");
            }
            finally
            {
                CloseClipboard();
            }
        }

        public static string[]? GetFiles()
        {
            if (!IsClipboardFormatAvailable(CF_HDROP))
                return null;

            if (!OpenClipboard(IntPtr.Zero))
                return null;

            try
            {
                var hMem = GetClipboardData(CF_HDROP);
                if (hMem == IntPtr.Zero)
                    return null;

                var ptr = GlobalLock(hMem);
                if (ptr == IntPtr.Zero)
                    return null;

                try
                {
                    var dropFiles = Marshal.PtrToStructure<DROPFILES>(ptr);
                    var filesPtr = ptr + dropFiles.pFiles;
                    var size = GlobalSize(hMem);
                    var filesSize = (int)size - dropFiles.pFiles;

                    if (filesSize <= 0)
                        return Array.Empty<string>();

                    var bytes = new byte[filesSize];
                    Marshal.Copy(filesPtr, bytes, 0, filesSize);

                    var filesStr = Encoding.Unicode.GetString(bytes);
                    var files = filesStr.Split(new[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
                    return files;
                }
                finally
                {
                    GlobalUnlock(hMem);
                }
            }
            finally
            {
                CloseClipboard();
            }
        }

        public static bool ContainsFiles()
        {
            return IsClipboardFormatAvailable(CF_HDROP);
        }

        public static void Clear()
        {
            if (OpenClipboard(IntPtr.Zero))
            {
                EmptyClipboard();
                CloseClipboard();
            }
        }

        public static bool IsEmpty()
        {
            return !ContainsText() && !ContainsImage() && !ContainsFiles();
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DROPFILES
        {
            public int pFiles;
            public POINT pt;
            public bool fNC;
            public bool fWide;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }
    }
}
