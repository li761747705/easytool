using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace EasyTool.SystemCategory
{
    /// <summary>
    /// 全局热键工具类
    /// </summary>
    public class HotKeyUtil : IDisposable
    {
        private readonly Dictionary<int, HotKeyRegistration> _registrations = new();
        private int _nextId = 1;
        private bool _disposed;
        private readonly object _lock = new();

        /// <summary>
        /// 热键按下事件
        /// </summary>
        public event EventHandler<HotKeyEventArgs>? HotKeyPressed;

        /// <summary>
        /// 注册全局热键
        /// </summary>
        /// <param name="modifiers">修饰键</param>
        /// <param name="key">按键</param>
        /// <param name="action">触发动作（可选）</param>
        /// <returns>热键ID</returns>
        public int Register(HotKeyModifiers modifiers, VirtualKeyCode key, Action? action = null)
        {
            lock (_lock)
            {
                var id = _nextId++;

                // 获取活动窗口句柄
                var hWnd = GetActiveWindow();
                if (hWnd == IntPtr.Zero)
                {
                    hWnd = GetConsoleWindow();
                }

                if (!RegisterHotKey(hWnd, id, (int)modifiers, (int)key))
                {
                    var error = Marshal.GetLastWin32Error();
                    throw new InvalidOperationException($"注册热键失败，错误码: {error}");
                }

                _registrations[id] = new HotKeyRegistration
                {
                    Id = id,
                    Modifiers = modifiers,
                    Key = key,
                    Action = action,
                    WindowHandle = hWnd
                };

                return id;
            }
        }

        /// <summary>
        /// 注销热键
        /// </summary>
        public bool Unregister(int id)
        {
            lock (_lock)
            {
                if (!_registrations.TryGetValue(id, out var registration))
                    return false;

                var result = UnregisterHotKey(registration.WindowHandle, id);
                _registrations.Remove(id);
                return result;
            }
        }

        /// <summary>
        /// 注销所有热键
        /// </summary>
        public void UnregisterAll()
        {
            lock (_lock)
            {
                foreach (var registration in _registrations.Values)
                {
                    UnregisterHotKey(registration.WindowHandle, registration.Id);
                }
                _registrations.Clear();
            }
        }

        /// <summary>
        /// 处理Windows消息（在消息循环中调用）
        /// </summary>
        public bool ProcessMessage(IntPtr wParam, IntPtr lParam)
        {
            var id = wParam.ToInt32();
            
            if (_registrations.TryGetValue(id, out var registration))
            {
                var args = new HotKeyEventArgs(registration.Id, registration.Modifiers, registration.Key);
                HotKeyPressed?.Invoke(this, args);
                registration.Action?.Invoke();
                return true;
            }

            return false;
        }

        /// <summary>
        /// 开始消息循环（阻塞）
        /// </summary>
        public void StartMessageLoop()
        {
            while (!_disposed)
            {
                if (GetMessage(out var msg, IntPtr.Zero, 0, 0))
                {
                    if (msg.message == WM_HOTKEY)
                    {
                        ProcessMessage(msg.wParam, msg.lParam);
                    }
                    else
                    {
                        TranslateMessage(ref msg);
                        DispatchMessage(ref msg);
                    }
                }
            }
        }

        /// <summary>
        /// 获取已注册的热键列表
        /// </summary>
        public IReadOnlyList<HotKeyInfo> GetRegisteredHotKeys()
        {
            lock (_lock)
            {
                return _registrations.Values
                    .Select(r => new HotKeyInfo(r.Id, r.Modifiers, r.Key))
                    .ToList()
                    .AsReadOnly();
            }
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            UnregisterAll();
            _disposed = true;
        }

        #region P/Invoke

        private const int WM_HOTKEY = 0x0312;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        private static extern bool GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

        [DllImport("user32.dll")]
        private static extern bool TranslateMessage(ref MSG lpMsg);

        [DllImport("user32.dll")]
        private static extern IntPtr DispatchMessage(ref MSG lpMsg);

        [StructLayout(LayoutKind.Sequential)]
        private struct MSG
        {
            public IntPtr hWnd;
            public int message;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public POINT pt;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        #endregion
    }

    /// <summary>
    /// 热键修饰键
    /// </summary>
    [Flags]
    public enum HotKeyModifiers
    {
        None = 0,
        Alt = 1,
        Control = 2,
        Shift = 4,
        Windows = 8
    }

    /// <summary>
    /// 热键注册信息
    /// </summary>
    internal class HotKeyRegistration
    {
        public int Id { get; set; }
        public HotKeyModifiers Modifiers { get; set; }
        public VirtualKeyCode Key { get; set; }
        public Action? Action { get; set; }
        public IntPtr WindowHandle { get; set; }
    }

    /// <summary>
    /// 热键信息
    /// </summary>
    public class HotKeyInfo
    {
        /// <summary>
        /// 热键ID
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// 修饰键
        /// </summary>
        public HotKeyModifiers Modifiers { get; }

        /// <summary>
        /// 按键
        /// </summary>
        public VirtualKeyCode Key { get; }

        public HotKeyInfo(int id, HotKeyModifiers modifiers, VirtualKeyCode key)
        {
            Id = id;
            Modifiers = modifiers;
            Key = key;
        }

        public override string ToString()
        {
            var parts = new List<string>();
            if (Modifiers.HasFlag(HotKeyModifiers.Control)) parts.Add("Ctrl");
            if (Modifiers.HasFlag(HotKeyModifiers.Alt)) parts.Add("Alt");
            if (Modifiers.HasFlag(HotKeyModifiers.Shift)) parts.Add("Shift");
            if (Modifiers.HasFlag(HotKeyModifiers.Windows)) parts.Add("Win");
            parts.Add(Key.ToString());
            return string.Join(" + ", parts);
        }
    }

    /// <summary>
    /// 热键事件参数
    /// </summary>
    public class HotKeyEventArgs : EventArgs
    {
        public int Id { get; }
        public HotKeyModifiers Modifiers { get; }
        public VirtualKeyCode Key { get; }

        public HotKeyEventArgs(int id, HotKeyModifiers modifiers, VirtualKeyCode key)
        {
            Id = id;
            Modifiers = modifiers;
            Key = key;
        }
    }
}
