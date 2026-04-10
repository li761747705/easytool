using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyTool.ToolCategory
{
    /// <summary>
    /// 控制台工具类
    /// 对标 Hutool 的 ConsoleUtil
    /// 提供控制台输入输出、颜色控制、进度条等功能
    /// </summary>
    public static class ConsoleUtil
    {
        #region 控制台输出

        /// <summary>
        /// 输出到控制台
        /// </summary>
        /// <param name="value">值</param>
        public static void Print(object? value)
        {
            Console.Write(value);
        }

        /// <summary>
        /// 输出到控制台并换行
        /// </summary>
        /// <param name="value">值</param>
        public static void PrintLine(object? value = null)
        {
            Console.WriteLine(value);
        }

        /// <summary>
        /// 格式化输出到控制台
        /// </summary>
        /// <param name="format">格式</param>
        /// <param name="args">参数</param>
        public static void PrintFormat(string format, params object?[] args)
        {
            Console.Write(format, args);
        }

        /// <summary>
        /// 格式化输出到控制台并换行
        /// </summary>
        /// <param name="format">格式</param>
        /// <param name="args">参数</param>
        public static void PrintFormatLine(string format, params object?[] args)
        {
            Console.WriteLine(format, args);
        }

        /// <summary>
        /// 输出错误信息
        /// </summary>
        /// <param name="value">值</param>
        public static void PrintError(object? value)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine(value);
            Console.ForegroundColor = oldColor;
        }

        #endregion

        #region 彩色输出

        /// <summary>
        /// 彩色输出
        /// </summary>
        /// <param name="value">值</param>
        /// <param name="color">颜色</param>
        public static void PrintColor(object? value, ConsoleColor color)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(value);
            Console.ForegroundColor = oldColor;
        }

        /// <summary>
        /// 彩色输出并换行
        /// </summary>
        /// <param name="value">值</param>
        /// <param name="color">颜色</param>
        public static void PrintColorLine(object? value, ConsoleColor color)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(value);
            Console.ForegroundColor = oldColor;
        }

        /// <summary>
        /// 输出红色文本
        /// </summary>
        /// <param name="value">值</param>
        public static void PrintRed(object? value) => PrintColorLine(value, ConsoleColor.Red);

        /// <summary>
        /// 输出绿色文本
        /// </summary>
        /// <param name="value">值</param>
        public static void PrintGreen(object? value) => PrintColorLine(value, ConsoleColor.Green);

        /// <summary>
        /// 输出黄色文本
        /// </summary>
        /// <param name="value">值</param>
        public static void PrintYellow(object? value) => PrintColorLine(value, ConsoleColor.Yellow);

        /// <summary>
        /// 输出蓝色文本
        /// </summary>
        /// <param name="value">值</param>
        public static void PrintBlue(object? value) => PrintColorLine(value, ConsoleColor.Blue);

        /// <summary>
        /// 输出青色文本
        /// </summary>
        /// <param name="value">值</param>
        public static void PrintCyan(object? value) => PrintColorLine(value, ConsoleColor.Cyan);

        /// <summary>
        /// 输出洋红色文本
        /// </summary>
        /// <param name="value">值</param>
        public static void PrintMagenta(object? value) => PrintColorLine(value, ConsoleColor.Magenta);

        #endregion

        #region 控制台输入

        /// <summary>
        /// 读取一行输入
        /// </summary>
        /// <returns>输入内容</returns>
        public static string? ReadLine()
        {
            return Console.ReadLine();
        }

        /// <summary>
        /// 读取一个字符
        /// </summary>
        /// <returns>字符</returns>
        public static int Read()
        {
            return Console.Read();
        }

        /// <summary>
        /// 读取一个按键
        /// </summary>
        /// <returns>按键信息</returns>
        public static ConsoleKeyInfo ReadKey()
        {
            return Console.ReadKey();
        }

        /// <summary>
        /// 读取一个按键（不显示）
        /// </summary>
        /// <returns>按键信息</returns>
        public static ConsoleKeyInfo ReadKeyHidden()
        {
            return Console.ReadKey(true);
        }

        /// <summary>
        /// 提示并读取输入
        /// </summary>
        /// <param name="prompt">提示信息</param>
        /// <returns>输入内容</returns>
        public static string? Input(string prompt)
        {
            Print(prompt);
            return ReadLine();
        }

        /// <summary>
        /// 提示并确认
        /// </summary>
        /// <param name="prompt">提示信息</param>
        /// <returns>是否确认</returns>
        public static bool Confirm(string prompt)
        {
            Print($"{prompt} (y/n): ");
            var key = Console.ReadKey(true);
            PrintLine();
            return key.Key == ConsoleKey.Y;
        }

        /// <summary>
        /// 等待用户按任意键
        /// </summary>
        /// <param name="message">提示信息</param>
        public static void WaitAnyKey(string message = "Press any key to continue...")
        {
            PrintLine(message);
            Console.ReadKey(true);
        }

        #endregion

        #region 控制台控制

        /// <summary>
        /// 清空控制台
        /// </summary>
        public static void Clear()
        {
            Console.Clear();
        }

        /// <summary>
        /// 设置控制台标题
        /// </summary>
        /// <param name="title">标题</param>
        public static void SetTitle(string title)
        {
            Console.Title = title;
        }

        /// <summary>
        /// 获取控制台标题
        /// </summary>
        /// <returns>标题</returns>
        public static string GetTitle()
        {
            return Console.Title;
        }

        /// <summary>
        /// 设置前景色
        /// </summary>
        /// <param name="color">颜色</param>
        public static void SetForegroundColor(ConsoleColor color)
        {
            Console.ForegroundColor = color;
        }

        /// <summary>
        /// 获取前景色
        /// </summary>
        /// <returns>颜色</returns>
        public static ConsoleColor GetForegroundColor()
        {
            return Console.ForegroundColor;
        }

        /// <summary>
        /// 设置背景色
        /// </summary>
        /// <param name="color">颜色</param>
        public static void SetBackgroundColor(ConsoleColor color)
        {
            Console.BackgroundColor = color;
        }

        /// <summary>
        /// 获取背景色
        /// </summary>
        /// <returns>颜色</returns>
        public static ConsoleColor GetBackgroundColor()
        {
            return Console.BackgroundColor;
        }

        /// <summary>
        /// 重置颜色
        /// </summary>
        public static void ResetColor()
        {
            Console.ResetColor();
        }

        /// <summary>
        /// 设置光标位置
        /// </summary>
        /// <param name="left">左边位置</param>
        /// <param name="top">顶部位置</param>
        public static void SetCursorPosition(int left, int top)
        {
            Console.SetCursorPosition(left, top);
        }

        /// <summary>
        /// 显示光标
        /// </summary>
        public static void ShowCursor()
        {
            Console.CursorVisible = true;
        }

        /// <summary>
        /// 隐藏光标
        /// </summary>
        public static void HideCursor()
        {
            Console.CursorVisible = false;
        }

        /// <summary>
        /// 获取控制台窗口宽度
        /// </summary>
        /// <returns>宽度</returns>
        public static int GetWindowWidth()
        {
            return Console.WindowWidth;
        }

        /// <summary>
        /// 获取控制台窗口高度
        /// </summary>
        /// <returns>高度</returns>
        public static int GetWindowHeight()
        {
            return Console.WindowHeight;
        }

        #endregion

        #region 进度条

        /// <summary>
        /// 显示进度条
        /// </summary>
        /// <param name="current">当前值</param>
        /// <param name="total">总数值</param>
        /// <param name="width">进度条宽度</param>
        public static void PrintProgress(int current, int total, int width = 50)
        {
            var percent = (double)current / total;
            var filled = (int)(percent * width);
            var empty = width - filled;

            Console.SetCursorPosition(0, Console.CursorTop);

            Console.Write("[");
            Console.Write(new string('=', filled));
            Console.Write(new string(' ', empty));
            Console.Write($"] {percent:P0} ({current}/{total})");

            if (current >= total)
            {
                Console.WriteLine();
            }
        }

        /// <summary>
        /// 显示旋转进度指示器
        /// </summary>
        /// <param name="step">步数</param>
        /// <param name="message">消息</param>
        public static void PrintSpinner(int step, string message = "Loading")
        {
            var chars = new[] { '|', '/', '-', '\\' };
            var idx = step % chars.Length;
            Console.Write($"\r{chars[idx]} {message}...");
        }

        #endregion

        #region 表格输出

        /// <summary>
        /// 输出表格
        /// </summary>
        /// <param name="headers">表头</param>
        /// <param name="rows">数据行</param>
        public static void PrintTable(string[] headers, List<string[]> rows)
        {
            if (headers == null || headers.Length == 0)
                return;

            // 计算每列最大宽度
            var widths = new int[headers.Length];
            for (int i = 0; i < headers.Length; i++)
            {
                widths[i] = headers[i].Length;
            }

            foreach (var row in rows)
            {
                for (int i = 0; i < Math.Min(row.Length, headers.Length); i++)
                {
                    widths[i] = Math.Max(widths[i], row[i]?.Length ?? 0);
                }
            }

            // 输出表头
            PrintTableSeparator(widths);
            PrintTableRow(headers, widths);
            PrintTableSeparator(widths);

            // 输出数据行
            foreach (var row in rows)
            {
                PrintTableRow(row, widths);
            }

            PrintTableSeparator(widths);
        }

        private static void PrintTableSeparator(int[] widths)
        {
            Console.Write("+");
            foreach (var w in widths)
            {
                Console.Write(new string('-', w + 2));
                Console.Write("+");
            }
            Console.WriteLine();
        }

        private static void PrintTableRow(string[] row, int[] widths)
        {
            Console.Write("|");
            for (int i = 0; i < widths.Length; i++)
            {
                var cell = i < row.Length ? row[i] ?? "" : "";
                Console.Write($" {cell.PadRight(widths[i])} |");
            }
            Console.WriteLine();
        }

        #endregion

        #region 消息框

        /// <summary>
        /// 输出信息框
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="title">标题</param>
        public static void PrintBox(string message, string? title = null)
        {
            var lines = message.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            var maxLen = lines.Max(l => l.Length);
            maxLen = Math.Max(maxLen, title?.Length ?? 0);

            var horizontal = new string('─', maxLen + 2);

            Console.WriteLine($"┌{horizontal}┐");

            if (!string.IsNullOrEmpty(title))
            {
                Console.WriteLine($"│ {title.PadRight(maxLen)} │");
                Console.WriteLine($"├{horizontal}┤");
            }

            foreach (var line in lines)
            {
                Console.WriteLine($"│ {line.PadRight(maxLen)} │");
            }

            Console.WriteLine($"└{horizontal}┘");
        }

        #endregion
    }
}