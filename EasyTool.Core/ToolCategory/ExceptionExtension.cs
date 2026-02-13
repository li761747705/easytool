using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace EasyTool.ToolCategory
{
    /// <summary>
    /// Exception 异常扩展方法
    /// </summary>
    public static class ExceptionExtension
    {
        #region 消息获取

        /// <summary>
        /// 获取完整的异常消息（包含所有内层异常）
        /// </summary>
        public static string GetFullMessage(this Exception? exception)
        {
            if (exception == null)
                return string.Empty;

            var sb = new StringBuilder();
            var current = exception;

            int depth = 0;
            while (current != null)
            {
                if (depth > 0)
                    sb.Append("Inner Exception: ");

                sb.AppendLine(current.Message);
                current = current.InnerException;
                depth++;

                // 防止无限循环
                if (depth > 100)
                    break;
            }

            return sb.ToString().Trim();
        }

        /// <summary>
        /// 获取所有异常（包含内层异常）
        /// </summary>
        public static Exception[] GetAllExceptions(this Exception? exception)
        {
            if (exception == null)
                return Array.Empty<Exception>();

            var exceptions = new List<Exception>();
            var current = exception;

            int depth = 0;
            while (current != null)
            {
                exceptions.Add(current);
                current = current.InnerException;
                depth++;

                // 防止无限循环
                if (depth > 100)
                    break;
            }

            return exceptions.ToArray();
        }

        /// <summary>
        /// 获取所有内层异常
        /// </summary>
        public static Exception[] GetInnerExceptions(this Exception exception)
        {
            var all = exception.GetAllExceptions();
            return all.Skip(1).ToArray();
        }

        #endregion

        #region 详细信息

        /// <summary>
        /// 获取异常的详细字符串表示
        /// </summary>
        public static string ToDetailedString(this Exception? exception)
        {
            if (exception == null)
                return string.Empty;

            var sb = new StringBuilder();

            sb.AppendLine($"Exception Type: {exception.GetType().FullName}");
            sb.AppendLine($"Message: {exception.Message}");
            sb.AppendLine($"Source: {exception.Source ?? "(unknown)"}");

            if (exception.TargetSite != null)
            {
                sb.AppendLine($"Target Site: {exception.TargetSite}");
            }

            if (!string.IsNullOrEmpty(exception.HelpLink))
            {
                sb.AppendLine($"Help Link: {exception.HelpLink}");
            }

            if (exception.Data != null && exception.Data.Count > 0)
            {
                sb.AppendLine("Data:");
                foreach (var key in exception.Data.Keys)
                {
                    sb.AppendLine($"  {key}: {exception.Data[key]}");
                }
            }

            if (!string.IsNullOrEmpty(exception.StackTrace))
            {
                sb.AppendLine("Stack Trace:");
                sb.AppendLine(exception.StackTrace);
            }

            // 处理内层异常
            if (exception.InnerException != null)
            {
                sb.AppendLine();
                sb.AppendLine("--- Inner Exception ---");
                sb.Append(exception.InnerException.ToDetailedString());
            }

            return sb.ToString();
        }

        /// <summary>
        /// 获取异常的简略字符串表示
        /// </summary>
        public static string ToSimpleString(this Exception? exception)
        {
            if (exception == null)
                return string.Empty;

            return $"{exception.GetType().Name}: {exception.Message}";
        }

        #endregion

        #region 异常类型判断

        /// <summary>
        /// 判断异常是否是指定类型
        /// </summary>
        public static bool IsType<T>(this Exception? exception) where T : Exception
        {
            return exception is T;
        }

        /// <summary>
        /// 判断异常或其内层异常是否是指定类型
        /// </summary>
        public static bool IsOrContainsType<T>(this Exception? exception) where T : Exception
        {
            var current = exception;

            while (current != null)
            {
                if (current is T)
                    return true;

                current = current.InnerException;
            }

            return false;
        }

        /// <summary>
        /// 查找第一个指定类型的异常
        /// </summary>
        public static T? FindType<T>(this Exception? exception) where T : Exception
        {
            var current = exception;

            while (current != null)
            {
                if (current is T typedException)
                    return typedException;

                current = current.InnerException;
            }

            return null;
        }

        #endregion

        #region 特定异常处理

        /// <summary>
        /// 判断是否是超时异常
        /// </summary>
        public static bool IsTimeout(this Exception? exception)
        {
            return exception.IsOrContainsType<TimeoutException>();
        }

        /// <summary>
        /// 判断是否是取消操作异常
        /// </summary>
        public static bool IsOperationCanceled(this Exception? exception)
        {
            return exception.IsOrContainsType<OperationCanceledException>();
        }

        /// <summary>
        /// 判断是否是参数异常
        /// </summary>
        public static bool IsArgumentException(this Exception? exception)
        {
            return exception.IsOrContainsType<ArgumentException>();
        }

        /// <summary>
        /// 判断是否是空引用异常
        /// </summary>
        public static bool IsNullReference(this Exception? exception)
        {
            return exception is NullReferenceException;
        }

        /// <summary>
        /// 判断是否是 IO 异常
        /// </summary>
        public static bool IsIOException(this Exception? exception)
        {
            return exception.IsOrContainsType<System.IO.IOException>();
        }

        #endregion

        #region 异常包装

        /// <summary>
        /// 使用指定消息包装异常
        /// </summary>
        public static Exception WrapWith(this Exception? exception, string message)
        {
            return new Exception(message, exception);
        }

        /// <summary>
        /// 使用指定类型包装异常
        /// </summary>
        public static TException WrapWith<TException>(this Exception? exception, string message) where TException : Exception
        {
            return (TException)Activator.CreateInstance(typeof(TException), message, exception)!;
        }

        #endregion

        #region 日志格式化

        /// <summary>
        /// 获取适合日志记录的异常信息
        /// </summary>
        public static string ToLogString(this Exception? exception, bool includeStackTrace = true)
        {
            if (exception == null)
                return string.Empty;

            var sb = new StringBuilder();

            sb.Append($"[{exception.GetType().Name}] ");
            sb.AppendLine(exception.Message);

            if (includeStackTrace && !string.IsNullOrEmpty(exception.StackTrace))
            {
                sb.AppendLine(exception.StackTrace.Trim());
            }

            return sb.ToString();
        }

        /// <summary>
        /// 获取单行格式的异常信息
        /// </summary>
        public static string ToOneLineString(this Exception? exception)
        {
            if (exception == null)
                return string.Empty;

            var sb = new StringBuilder();
            var current = exception;

            while (current != null)
            {
                if (sb.Length > 0)
                    sb.Append(" -> ");

                sb.Append($"[{current.GetType().Name}] {current.Message}");
                current = current.InnerException;

                // 防止无限循环
                if (sb.Length > 1000)
                    break;
            }

            return sb.ToString();
        }

        #endregion

        #region 聚合异常处理

        /// <summary>
        /// 获取聚合异常中的所有异常
        /// </summary>
        public static Exception[] GetInnerExceptions(this AggregateException? exception)
        {
            if (exception == null)
                return Array.Empty<Exception>();

            return exception.InnerExceptions.ToArray();
        }


        #endregion
    }
}
