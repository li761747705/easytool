using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;

namespace EasyTool.SystemCategory
{
    /// <summary>
    /// 屏幕工具类
    /// </summary>
    public static class ScreenUtil
    {
        /// <summary>
        /// 获取主屏幕
        /// </summary>
        public static ScreenInfo GetPrimaryScreen()
        {
            var bounds = GetPrimaryScreenBounds();
            return new ScreenInfo
            {
                DeviceName = "Primary",
                Width = bounds.Width,
                Height = bounds.Height,
                X = bounds.X,
                Y = bounds.Y,
                BitsPerPixel = 32,
                IsPrimary = true
            };
        }

        /// <summary>
        /// 获取所有屏幕
        /// </summary>
        public static List<ScreenInfo> GetAllScreens()
        {
            var result = new List<ScreenInfo>();
            var monitors = new List<MonitorInfo>();

            EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero,
                (hMonitor, hdcMonitor, lprcMonitor, dwData) =>
                {
                    var info = new MonitorInfoEx();
                    info.Size = Marshal.SizeOf(info);
                    if (GetMonitorInfo(hMonitor, ref info))
                    {
                        monitors.Add(new MonitorInfo
                        {
                            DeviceName = info.DeviceName,
                            Bounds = info.Monitor,
                            WorkArea = info.WorkArea,
                            IsPrimary = (info.Flags & 1) != 0
                        });
                    }
                    return true;
                }, IntPtr.Zero);

            foreach (var monitor in monitors)
            {
                result.Add(new ScreenInfo
                {
                    DeviceName = monitor.DeviceName,
                    Width = monitor.Bounds.Right - monitor.Bounds.Left,
                    Height = monitor.Bounds.Bottom - monitor.Bounds.Top,
                    X = monitor.Bounds.Left,
                    Y = monitor.Bounds.Top,
                    BitsPerPixel = 32,
                    IsPrimary = monitor.IsPrimary
                });
            }

            return result;
        }

        /// <summary>
        /// 获取虚拟屏幕尺寸（所有屏幕合并）
        /// </summary>
        public static (int Width, int Height) GetVirtualScreenSize()
        {
            return (GetSystemMetrics(SM_CXVIRTUALSCREEN), GetSystemMetrics(SM_CYVIRTUALSCREEN));
        }

        /// <summary>
        /// 截取屏幕截图
        /// </summary>
        public static Bitmap? CaptureScreen()
        {
            try
            {
                var bounds = GetPrimaryScreenBounds();
                return CaptureRegion(bounds.X, bounds.Y, bounds.Width, bounds.Height);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 截取指定区域
        /// </summary>
        public static Bitmap? CaptureRegion(int x, int y, int width, int height)
        {
            try
            {
                var bitmap = new Bitmap(width, height);
                using (var g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(x, y, 0, 0, new Size(width, height));
                }
                return bitmap;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 截取活动窗口
        /// </summary>
        public static Bitmap? CaptureActiveWindow()
        {
            try
            {
                var handle = GetForegroundWindow();
                GetWindowRect(handle, out var rect);
                return CaptureRegion(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 获取鼠标位置
        /// </summary>
        public static (int X, int Y) GetMousePosition()
        {
            GetCursorPos(out var point);
            return (point.X, point.Y);
        }

        /// <summary>
        /// 设置鼠标位置
        /// </summary>
        public static void SetMousePosition(int x, int y)
        {
            SetCursorPos(x, y);
        }

        private static Rectangle GetPrimaryScreenBounds()
        {
            var width = GetSystemMetrics(SM_CXSCREEN);
            var height = GetSystemMetrics(SM_CYSCREEN);
            return new Rectangle(0, 0, width, height);
        }

        #region P/Invoke

        private const int SM_CXSCREEN = 0;
        private const int SM_CYSCREEN = 1;
        private const int SM_CXVIRTUALSCREEN = 78;
        private const int SM_CYVIRTUALSCREEN = 79;

        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int nIndex);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip,
            EnumMonitorsDelegate lpfnEnum, IntPtr dwData);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MonitorInfoEx lpmi);

        private delegate bool EnumMonitorsDelegate(IntPtr hMonitor, IntPtr hdcMonitor, RECT lprcMonitor, IntPtr dwData);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct MonitorInfoEx
        {
            public int Size;
            public RECT Monitor;
            public RECT WorkArea;
            public int Flags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string DeviceName;
        }

        private class MonitorInfo
        {
            public string DeviceName { get; set; } = "";
            public RECT Bounds { get; set; }
            public RECT WorkArea { get; set; }
            public bool IsPrimary { get; set; }
        }

        #endregion
    }

    /// <summary>
    /// 屏幕信息
    /// </summary>
    public class ScreenInfo
    {
        public string DeviceName { get; set; } = "";
        public int Width { get; set; }
        public int Height { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int BitsPerPixel { get; set; }
        public bool IsPrimary { get; set; }

        public string Resolution => $"{Width} x {Height}";
    }
}