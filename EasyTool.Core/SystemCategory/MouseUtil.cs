using System;
using System.Runtime.InteropServices;

namespace EasyTool.SystemCategory
{
    /// <summary>
    /// 鼠标工具类
    /// </summary>
    public static class MouseUtil
    {
        #region 鼠标位置

        /// <summary>
        /// 获取鼠标位置
        /// </summary>
        public static (int X, int Y) GetPosition()
        {
            GetCursorPos(out var point);
            return (point.X, point.Y);
        }

        /// <summary>
        /// 设置鼠标位置
        /// </summary>
        public static void SetPosition(int x, int y)
        {
            SetCursorPos(x, y);
        }

        /// <summary>
        /// 移动鼠标到指定位置（平滑移动）
        /// </summary>
        public static void MoveTo(int x, int y, int steps = 10, int delayMs = 10)
        {
            var (currentX, currentY) = GetPosition();
            var stepX = (x - currentX) / (double)steps;
            var stepY = (y - currentY) / (double)steps;

            for (int i = 1; i <= steps; i++)
            {
                SetPosition((int)(currentX + stepX * i), (int)(currentY + stepY * i));
                System.Threading.Thread.Sleep(delayMs);
            }
        }

        #endregion

        #region 鼠标点击

        /// <summary>
        /// 左键单击
        /// </summary>
        public static void LeftClick()
        {
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
            System.Threading.Thread.Sleep(50);
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
        }

        /// <summary>
        /// 左键双击
        /// </summary>
        public static void LeftDoubleClick()
        {
            LeftClick();
            System.Threading.Thread.Sleep(100);
            LeftClick();
        }

        /// <summary>
        /// 右键单击
        /// </summary>
        public static void RightClick()
        {
            mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, 0);
            System.Threading.Thread.Sleep(50);
            mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
        }

        /// <summary>
        /// 中键单击
        /// </summary>
        public static void MiddleClick()
        {
            mouse_event(MOUSEEVENTF_MIDDLEDOWN, 0, 0, 0, 0);
            System.Threading.Thread.Sleep(50);
            mouse_event(MOUSEEVENTF_MIDDLEUP, 0, 0, 0, 0);
        }

        /// <summary>
        /// 在指定位置点击
        /// </summary>
        public static void ClickAt(int x, int y, MouseButton button = MouseButton.Left)
        {
            SetPosition(x, y);
            System.Threading.Thread.Sleep(50);

            switch (button)
            {
                case MouseButton.Left:
                    LeftClick();
                    break;
                case MouseButton.Right:
                    RightClick();
                    break;
                case MouseButton.Middle:
                    MiddleClick();
                    break;
            }
        }

        #endregion

        #region 鼠标按下/释放

        /// <summary>
        /// 按下左键
        /// </summary>
        public static void LeftDown()
        {
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
        }

        /// <summary>
        /// 释放左键
        /// </summary>
        public static void LeftUp()
        {
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
        }

        /// <summary>
        /// 按下右键
        /// </summary>
        public static void RightDown()
        {
            mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, 0);
        }

        /// <summary>
        /// 释放右键
        /// </summary>
        public static void RightUp()
        {
            mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
        }

        /// <summary>
        /// 按下中键
        /// </summary>
        public static void MiddleDown()
        {
            mouse_event(MOUSEEVENTF_MIDDLEDOWN, 0, 0, 0, 0);
        }

        /// <summary>
        /// 释放中键
        /// </summary>
        public static void MiddleUp()
        {
            mouse_event(MOUSEEVENTF_MIDDLEUP, 0, 0, 0, 0);
        }

        #endregion

        #region 鼠标拖拽

        /// <summary>
        /// 鼠标拖拽（从起点拖到终点）
        /// </summary>
        public static void Drag(int fromX, int fromY, int toX, int toY, int steps = 20)
        {
            SetPosition(fromX, fromY);
            System.Threading.Thread.Sleep(50);
            LeftDown();
            System.Threading.Thread.Sleep(50);
            MoveTo(toX, toY, steps);
            System.Threading.Thread.Sleep(50);
            LeftUp();
        }

        #endregion

        #region 鼠标滚轮

        /// <summary>
        /// 滚动鼠标滚轮
        /// </summary>
        /// <param name="delta">滚动量，正数向上，负数向下</param>
        public static void Scroll(int delta)
        {
            mouse_event(MOUSEEVENTF_WHEEL, 0, 0, delta * 120, 0);
        }

        /// <summary>
        /// 向上滚动
        /// </summary>
        public static void ScrollUp(int amount = 1)
        {
            Scroll(amount);
        }

        /// <summary>
        /// 向下滚动
        /// </summary>
        public static void ScrollDown(int amount = 1)
        {
            Scroll(-amount);
        }

        /// <summary>
        /// 水平滚动
        /// </summary>
        public static void HorizontalScroll(int delta)
        {
            mouse_event(MOUSEEVENTF_HWHEEL, 0, 0, delta * 120, 0);
        }

        #endregion

        #region 鼠标状态

        /// <summary>
        /// 检测鼠标左键是否按下
        /// </summary>
        public static bool IsLeftButtonDown()
        {
            return (GetAsyncKeyState(VK_LBUTTON) & 0x8000) != 0;
        }

        /// <summary>
        /// 检测鼠标右键是否按下
        /// </summary>
        public static bool IsRightButtonDown()
        {
            return (GetAsyncKeyState(VK_RBUTTON) & 0x8000) != 0;
        }

        /// <summary>
        /// 检测鼠标中键是否按下
        /// </summary>
        public static bool IsMiddleButtonDown()
        {
            return (GetAsyncKeyState(VK_MBUTTON) & 0x8000) != 0;
        }

        /// <summary>
        /// 显示鼠标光标
        /// </summary>
        public static void ShowCursor()
        {
            ShowCursor(true);
        }

        /// <summary>
        /// 隐藏鼠标光标
        /// </summary>
        public static void HideCursor()
        {
            ShowCursor(false);
        }

        #endregion

        #region P/Invoke

        private const int MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const int MOUSEEVENTF_LEFTUP = 0x0004;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
        private const int MOUSEEVENTF_RIGHTUP = 0x0010;
        private const int MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        private const int MOUSEEVENTF_MIDDLEUP = 0x0040;
        private const int MOUSEEVENTF_WHEEL = 0x0800;
        private const int MOUSEEVENTF_HWHEEL = 0x01000;

        private const int VK_LBUTTON = 0x01;
        private const int VK_RBUTTON = 0x02;
        private const int VK_MBUTTON = 0x04;

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        [DllImport("user32.dll")]
        private static extern int ShowCursor(bool bShow);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        #endregion
    }

    /// <summary>
    /// 鼠标按钮
    /// </summary>
    public enum MouseButton
    {
        Left,
        Right,
        Middle
    }
}
