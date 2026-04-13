using System;
using System.Threading.Tasks;

namespace EasyTool.ToolCategory
{
    /// <summary>
    /// 结果类型
    /// </summary>
    public class Result
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccess { get; protected set; }

        /// <summary>
        /// 是否失败
        /// </summary>
        public bool IsFailure => !IsSuccess;

        /// <summary>
        /// 错误信息
        /// </summary>
        public string? Error { get; protected set; }

        /// <summary>
        /// 错误代码
        /// </summary>
        public string? ErrorCode { get; protected set; }

        protected Result(bool isSuccess, string? error = null, string? errorCode = null)
        {
            IsSuccess = isSuccess;
            Error = error;
            ErrorCode = errorCode;
        }

        /// <summary>
        /// 创建成功结果
        /// </summary>
        public static Result Success() => new Result(true);

        /// <summary>
        /// 创建失败结果
        /// </summary>
        public static Result Failure(string error, string? errorCode = null) => new Result(false, error, errorCode);

        /// <summary>
        /// 创建成功结果（带值）
        /// </summary>
        public static Result<T> Success<T>(T value) => new Result<T>(true, value);

        /// <summary>
        /// 创建失败结果（带值）
        /// </summary>
        public static Result<T> Failure<T>(string error, string? errorCode = null) => new Result<T>(false, default, error, errorCode);

        /// <summary>
        /// 从异常创建失败结果
        /// </summary>
        public static Result FromException(Exception ex) => Failure(ex.Message, ex.GetType().Name);

        /// <summary>
        /// 从异常创建失败结果
        /// </summary>
        public static Result<T> FromException<T>(Exception ex) => Failure<T>(ex.Message, ex.GetType().Name);

        /// <summary>
        /// 匹配处理
        /// </summary>
        public void Match(Action onSuccess, Action<string> onFailure)
        {
            if (IsSuccess) onSuccess();
            else onFailure(Error ?? "");
        }

        /// <summary>
        /// 匹配处理并返回值
        /// </summary>
        public T Match<T>(Func<T> onSuccess, Func<string, T> onFailure)
        {
            return IsSuccess ? onSuccess() : onFailure(Error ?? "");
        }

        /// <summary>
        /// 绑定下一个操作
        /// </summary>
        public Result Bind(Func<Result> next)
        {
            return IsSuccess ? next() : Failure(Error!, ErrorCode);
        }

        /// <summary>
        /// 映射
        /// </summary>
        public Result<T> Map<T>(Func<T> mapper)
        {
            return IsSuccess ? Success(mapper()) : Failure<T>(Error!, ErrorCode);
        }

        /// <summary>
        /// 异步匹配处理
        /// </summary>
        public async Task MatchAsync(Func<Task> onSuccess, Action<string> onFailure)
        {
            if (IsSuccess) await onSuccess().ConfigureAwait(false);
            else onFailure(Error ?? "");
        }
    }

    /// <summary>
    /// 带值的结果类型
    /// </summary>
    public class Result<T> : Result
    {
        /// <summary>
        /// 值
        /// </summary>
        public T? Value { get; }

        internal Result(bool isSuccess, T? value, string? error = null, string? errorCode = null)
            : base(isSuccess, error, errorCode)
        {
            Value = value;
        }

        /// <summary>
        /// 获取值，失败则抛出异常
        /// </summary>
        public T GetValueOrThrow()
        {
            if (IsFailure)
                throw new InvalidOperationException(Error ?? "操作失败");
            return Value!;
        }

        /// <summary>
        /// 获取值或默认值
        /// </summary>
        public T GetValueOrDefault(T defaultValue)
        {
            return IsSuccess ? Value! : defaultValue;
        }

        /// <summary>
        /// 匹配处理
        /// </summary>
        public void Match(Action<T> onSuccess, Action<string> onFailure)
        {
            if (IsSuccess) onSuccess(Value!);
            else onFailure(Error ?? "");
        }

        /// <summary>
        /// 匹配处理并返回值
        /// </summary>
        public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<string, TResult> onFailure)
        {
            return IsSuccess ? onSuccess(Value!) : onFailure(Error ?? "");
        }

        /// <summary>
        /// 绑定下一个操作
        /// </summary>
        public Result<TResult> Bind<TResult>(Func<T, Result<TResult>> next)
        {
            return IsSuccess ? next(Value!) : Failure<TResult>(Error!, ErrorCode);
        }

        /// <summary>
        /// 映射
        /// </summary>
        public Result<TResult> Map<TResult>(Func<T, TResult> mapper)
        {
            return IsSuccess ? Success(mapper(Value!)) : Failure<TResult>(Error!, ErrorCode);
        }

        /// <summary>
        /// 异步绑定
        /// </summary>
        public async Task<Result<TResult>> BindAsync<TResult>(Func<T, Task<Result<TResult>>> next)
        {
            return IsSuccess ? await next(Value!).ConfigureAwait(false) : Failure<TResult>(Error!, ErrorCode);
        }

        /// <summary>
        /// 异步映射
        /// </summary>
        public async Task<Result<TResult>> MapAsync<TResult>(Func<T, Task<TResult>> mapper)
        {
            return IsSuccess ? Success(await mapper(Value!).ConfigureAwait(false)) : Failure<TResult>(Error!, ErrorCode);
        }

        /// <summary>
        /// 隐式转换
        /// </summary>
        public static implicit operator Result<T>(T value) => Success(value);
    }

    /// <summary>
    /// 结果工具类
    /// </summary>
    public static class ResultUtil
    {
        /// <summary>
        /// 组合多个结果
        /// </summary>
        public static Result Combine(params Result[] results)
        {
            foreach (var result in results)
            {
                if (result.IsFailure)
                    return result;
            }
            return Result.Success();
        }

        /// <summary>
        /// 组合多个结果
        /// </summary>
        public static Result<T[]> Combine<T>(params Result<T>[] results)
        {
            var values = new T[results.Length];
            for (int i = 0; i < results.Length; i++)
            {
                if (results[i].IsFailure)
                    return Result.Failure<T[]>(results[i].Error!, results[i].ErrorCode);
                values[i] = results[i].Value!;
            }
            return Result.Success(values);
        }

        /// <summary>
        /// 尝试执行
        /// </summary>
        public static Result Try(Action action)
        {
            try
            {
                action();
                return Result.Success();
            }
            // 捕获所有异常以转换为 Result（Try 模式需处理用户委托的任意异常）
            catch (Exception ex)
            {
                return Result.FromException(ex);
            }
        }

        /// <summary>
        /// 尝试执行
        /// </summary>
        public static Result<T> Try<T>(Func<T> func)
        {
            try
            {
                return Result.Success(func());
            }
            // 捕获所有异常以转换为 Result（Try 模式需处理用户委托的任意异常）
            catch (Exception ex)
            {
                return Result.FromException<T>(ex);
            }
        }

        /// <summary>
        /// 异步尝试执行
        /// </summary>
        public static async Task<Result> TryAsync(Func<Task> action)
        {
            try
            {
                await action().ConfigureAwait(false);
                return Result.Success();
            }
            // 捕获所有异常以转换为 Result（Try 模式需处理用户委托的任意异常）
            catch (Exception ex)
            {
                return Result.FromException(ex);
            }
        }

        /// <summary>
        /// 异步尝试执行
        /// </summary>
        public static async Task<Result<T>> TryAsync<T>(Func<Task<T>> func)
        {
            try
            {
                return Result.Success(await func().ConfigureAwait(false));
            }
            // 捕获所有异常以转换为 Result（Try 模式需处理用户委托的任意异常）
            catch (Exception ex)
            {
                return Result.FromException<T>(ex);
            }
        }
    }
}
