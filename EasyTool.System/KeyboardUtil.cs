using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace EasyTool.System
{
    /// <summary>
    /// 键盘工具类
    /// </summary>
    public static class KeyboardUtil
    {
        #region 键盘状态检测

        /// <summary>
        /// 检测按键是否按下
        /// </summary>
        public static bool IsKeyDown(VirtualKeyCode keyCode)
        {
            return (GetKeyState((int)keyCode) & 0x8000) != 0;
        }

        /// <summary>
        /// 检测按键是否切换（如CapsLock、NumLock）
        /// </summary>
        public static bool IsKeyToggled(VirtualKeyCode keyCode)
        {
            return (GetKeyState((int)keyCode) & 0x0001) != 0;
        }

        /// <summary>
        /// 检测CapsLock是否开启
        /// </summary>
        public static bool IsCapsLockOn()
        {
            return IsKeyToggled(VirtualKeyCode.CapsLock);
        }

        /// <summary>
        /// 检测NumLock是否开启
        /// </summary>
        public static bool IsNumLockOn()
        {
            return IsKeyToggled(VirtualKeyCode.NumLock);
        }

        /// <summary>
        /// 检测ScrollLock是否开启
        /// </summary>
        public static bool IsScrollLockOn()
        {
            return IsKeyToggled(VirtualKeyCode.ScrollLock);
        }

        /// <summary>
        /// 检测Shift是否按下
        /// </summary>
        public static bool IsShiftDown()
        {
            return IsKeyDown(VirtualKeyCode.Shift) || IsKeyDown(VirtualKeyCode.LeftShift) || IsKeyDown(VirtualKeyCode.RightShift);
        }

        /// <summary>
        /// 检测Ctrl是否按下
        /// </summary>
        public static bool IsCtrlDown()
        {
            return IsKeyDown(VirtualKeyCode.Control) || IsKeyDown(VirtualKeyCode.LeftControl) || IsKeyDown(VirtualKeyCode.RightControl);
        }

        /// <summary>
        /// 检测Alt是否按下
        /// </summary>
        public static bool IsAltDown()
        {
            return IsKeyDown(VirtualKeyCode.Alt) || IsKeyDown(VirtualKeyCode.LeftMenu) || IsKeyDown(VirtualKeyCode.RightMenu);
        }

        /// <summary>
        /// 检测Windows键是否按下
        /// </summary>
        public static bool IsWindowsKeyDown()
        {
            return IsKeyDown(VirtualKeyCode.LeftWindows) || IsKeyDown(VirtualKeyCode.RightWindows);
        }

        #endregion

        #region 模拟按键

        /// <summary>
        /// 模拟按键按下
        /// </summary>
        public static void KeyDown(VirtualKeyCode keyCode)
        {
            keybd_event((byte)keyCode, 0, KEYEVENTF_KEYDOWN, 0);
        }

        /// <summary>
        /// 模拟按键释放
        /// </summary>
        public static void KeyUp(VirtualKeyCode keyCode)
        {
            keybd_event((byte)keyCode, 0, KEYEVENTF_KEYUP, 0);
        }

        /// <summary>
        /// 模拟按键（按下并释放）
        /// </summary>
        public static void KeyPress(VirtualKeyCode keyCode)
        {
            KeyDown(keyCode);
            Thread.Sleep(50);
            KeyUp(keyCode);
        }

        /// <summary>
        /// 模拟快捷键
        /// </summary>
        public static void SendHotKey(params VirtualKeyCode[] keys)
        {
            if (keys == null || keys.Length == 0)
                return;

            // 按下所有键
            foreach (var key in keys)
            {
                KeyDown(key);
                Thread.Sleep(50);
            }

            // 释放所有键（逆序）
            for (int i = keys.Length - 1; i >= 0; i--)
            {
                KeyUp(keys[i]);
                Thread.Sleep(50);
            }
        }

        /// <summary>
        /// 模拟文本输入
        /// </summary>
        public static void SendText(string text)
        {
            foreach (var c in text)
            {
                SendChar(c);
                Thread.Sleep(50);
            }
        }

        private static void SendChar(char c)
        {
            var inputs = new INPUT[2];

            inputs[0].type = INPUT_KEYBOARD;
            inputs[0].u.ki.wVk = 0;
            inputs[0].u.ki.wScan = c;
            inputs[0].u.ki.dwFlags = KEYEVENTF_UNICODE;

            inputs[1].type = INPUT_KEYBOARD;
            inputs[1].u.ki.wVk = 0;
            inputs[1].u.ki.wScan = c;
            inputs[1].u.ki.dwFlags = KEYEVENTF_UNICODE | KEYEVENTF_KEYUP;

            SendInput(2, inputs, INPUT.Size);
        }

        #endregion

        #region P/Invoke

        private const int KEYEVENTF_KEYDOWN = 0x0000;
        private const int KEYEVENTF_KEYUP = 0x0002;
        private const int KEYEVENTF_UNICODE = 0x0004;
        private const int INPUT_KEYBOARD = 1;

        [DllImport("user32.dll")]
        private static extern short GetKeyState(int nVirtKey);

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public int type;
            public InputUnion u;

            public static int Size => Marshal.SizeOf(typeof(INPUT));
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct InputUnion
        {
            [FieldOffset(0)]
            public KEYBDINPUT ki;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        #endregion
    }

    /// <summary>
    /// 虚拟键码
    /// </summary>
    public enum VirtualKeyCode : short
    {
        LeftButton = 0x01,
        RightButton = 0x02,
        Cancel = 0x03,
        MiddleButton = 0x04,
        Back = 0x08,
        Tab = 0x09,
        Clear = 0x0C,
        Return = 0x0D,
        Shift = 0x10,
        Control = 0x11,
        Alt = 0x12,
        Pause = 0x13,
        CapsLock = 0x14,
        Escape = 0x1B,
        Space = 0x20,
        PageUp = 0x21,
        PageDown = 0x22,
        End = 0x23,
        Home = 0x24,
        Left = 0x25,
        Up = 0x26,
        Right = 0x27,
        Down = 0x28,
        PrintScreen = 0x2A,
        Insert = 0x2D,
        Delete = 0x2E,
        D0 = 0x30,
        D1 = 0x31,
        D2 = 0x32,
        D3 = 0x33,
        D4 = 0x34,
        D5 = 0x35,
        D6 = 0x36,
        D7 = 0x37,
        D8 = 0x38,
        D9 = 0x39,
        A = 0x41,
        B = 0x42,
        C = 0x43,
        D = 0x44,
        E = 0x45,
        F = 0x46,
        G = 0x47,
        H = 0x48,
        I = 0x49,
        J = 0x4A,
        K = 0x4B,
        L = 0x4C,
        M = 0x4D,
        N = 0x4E,
        O = 0x4F,
        P = 0x50,
        Q = 0x51,
        R = 0x52,
        S = 0x53,
        T = 0x54,
        U = 0x55,
        V = 0x56,
        W = 0x57,
        X = 0x58,
        Y = 0x59,
        Z = 0x5A,
        LeftWindows = 0x5B,
        RightWindows = 0x5C,
        Apps = 0x5D,
        NumLock = 0x90,
        ScrollLock = 0x91,
        F1 = 0x70,
        F2 = 0x71,
        F3 = 0x72,
        F4 = 0x73,
        F5 = 0x74,
        F6 = 0x75,
        F7 = 0x76,
        F8 = 0x77,
        F9 = 0x78,
        F10 = 0x79,
        F11 = 0x7A,
        F12 = 0x7B,
        NumPad0 = 0x60,
        NumPad1 = 0x61,
        NumPad2 = 0x62,
        NumPad3 = 0x63,
        NumPad4 = 0x64,
        NumPad5 = 0x65,
        NumPad6 = 0x66,
        NumPad7 = 0x67,
        NumPad8 = 0x68,
        NumPad9 = 0x69,
        Multiply = 0x6A,
        Add = 0x6B,
        Separator = 0x6C,
        Subtract = 0x6D,
        Decimal = 0x6E,
        Divide = 0x6F,
        LeftShift = 0xA0,
        RightShift = 0xA1,
        LeftControl = 0xA2,
        RightControl = 0xA3,
        LeftMenu = 0xA4,
        RightMenu = 0xA5,
        VolumeMute = 0xAD,
        VolumeDown = 0xAE,
        VolumeUp = 0xAF
    }
}
