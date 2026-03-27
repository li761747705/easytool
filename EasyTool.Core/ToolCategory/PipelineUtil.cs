using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EasyTool.ToolCategory
{
    /// <summary>
    /// 管道上下文
    /// </summary>
    public class PipelineContext
    {
        private readonly Dictionary<string, object> _items = new();

        /// <summary>
        /// 获取或设置项
        /// </summary>
        public object? this[string key]
        {
            get => _items.TryGetValue(key, out var value) ? value : null;
            set => _items[key] = value!;
        }

        /// <summary>
        /// 获取值
        /// </summary>
        public T? Get<T>(string key)
        {
            return _items.TryGetValue(key, out var value) ? (T?)value : default;
        }

        /// <summary>
        /// 设置值
        /// </summary>
        public void Set<T>(string key, T value)
        {
            _items[key] = value!;
        }

        /// <summary>
        /// 是否包含键
        /// </summary>
        public bool ContainsKey(string key) => _items.ContainsKey(key);

        /// <summary>
        /// 移除项
        /// </summary>
        public bool Remove(string key) => _items.Remove(key);

        /// <summary>
        /// 清空
        /// </summary>
        public void Clear() => _items.Clear();
    }

    /// <summary>
    /// 管道处理委托
    /// </summary>
    public delegate Task PipelineDelegate(PipelineContext context);

    /// <summary>
    /// 管道构建器
    /// </summary>
    public class PipelineBuilder
    {
        private readonly List<Func<PipelineDelegate, PipelineDelegate>> _middlewares = new();

        /// <summary>
        /// 添加中间件
        /// </summary>
        public PipelineBuilder Use(Func<PipelineDelegate, PipelineDelegate> middleware)
        {
            _middlewares.Add(middleware);
            return this;
        }

        /// <summary>
        /// 添加中间件
        /// </summary>
        public PipelineBuilder Use(Func<PipelineContext, PipelineDelegate, Task> middleware)
        {
            return Use(next => context => middleware(context, next));
        }

        /// <summary>
        /// 添加同步中间件
        /// </summary>
        public PipelineBuilder Use(Action<PipelineContext, Action> middleware)
        {
            return Use(next => context =>
            {
                middleware(context, () => next(context).GetAwaiter().GetResult());
                return Task.CompletedTask;
            });
        }

        /// <summary>
        /// 条件分支
        /// </summary>
        public PipelineBuilder UseWhen(Func<PipelineContext, bool> predicate, Action<PipelineBuilder> configure)
        {
            var branchBuilder = new PipelineBuilder();
            configure(branchBuilder);

            return Use(next =>
            {
                var branch = branchBuilder.Build(next);
                return context => predicate(context) ? branch(context) : next(context);
            });
        }

        /// <summary>
        /// 映射分支
        /// </summary>
        public PipelineBuilder Map(string path, Action<PipelineBuilder> configure)
        {
            return UseWhen(ctx => ctx.Get<string>("Path")?.StartsWith(path) == true, configure);
        }

        /// <summary>
        /// 异常处理
        /// </summary>
        public PipelineBuilder UseExceptionHandling(Func<PipelineContext, Exception, Task>? handler = null)
        {
            return Use(next => async context =>
            {
                try
                {
                    await next(context);
                }
                catch (Exception ex)
                {
                    if (handler != null)
                        await handler(context, ex);
                    else
                        context.Set("Exception", ex);
                }
            });
        }

        /// <summary>
        /// 超时处理
        /// </summary>
        public PipelineBuilder UseTimeout(TimeSpan timeout)
        {
            return Use(next => async context =>
            {
                using var cts = new System.Threading.CancellationTokenSource(timeout);
                var task = next(context);
                var completed = await Task.WhenAny(task, Task.Delay(timeout));

                if (completed != task)
                {
                    context.Set("Timeout", true);
                    throw new TimeoutException($"管道执行超时: {timeout}");
                }

                await task;
            });
        }

        /// <summary>
        /// 日志记录
        /// </summary>
        public PipelineBuilder UseLogging(Action<string>? log = null)
        {
            return Use(async (context, next) =>
            {
                log?.Invoke($"[{DateTime.Now:HH:mm:ss}] 开始执行管道");
                await next(context);
                log?.Invoke($"[{DateTime.Now:HH:mm:ss}] 结束执行管道");
            });
        }

        /// <summary>
        /// 计时
        /// </summary>
        public PipelineBuilder UseTiming(Action<TimeSpan>? callback = null)
        {
            return Use(async (context, next) =>
            {
                var sw = System.Diagnostics.Stopwatch.StartNew();
                await next(context);
                sw.Stop();
                callback?.Invoke(sw.Elapsed);
                context.Set("ElapsedTime", sw.Elapsed);
            });
        }

        /// <summary>
        /// 构建管道
        /// </summary>
        public PipelineDelegate Build(PipelineDelegate? terminal = null)
        {
            terminal ??= _ => Task.CompletedTask;

            for (int i = _middlewares.Count - 1; i >= 0; i--)
            {
                terminal = _middlewares[i](terminal);
            }

            return terminal;
        }
    }

    /// <summary>
    /// 泛型管道（带结果类型）
    /// </summary>
    public class Pipeline<TInput, TResult>
    {
        private readonly List<Func<TInput, Func<TInput, Task<TResult>>, Task<TResult>>> _middlewares = new();

        /// <summary>
        /// 添加中间件
        /// </summary>
        public Pipeline<TInput, TResult> Use(Func<TInput, Func<TInput, Task<TResult>>, Task<TResult>> middleware)
        {
            _middlewares.Add(middleware);
            return this;
        }

        /// <summary>
        /// 执行管道
        /// </summary>
        public async Task<TResult> ExecuteAsync(TInput input, Func<TInput, Task<TResult>> terminal)
        {
            Func<TInput, Task<TResult>> current = terminal;

            for (int i = _middlewares.Count - 1; i >= 0; i--)
            {
                var middleware = _middlewares[i];
                var next = current;
                current = ctx => middleware(ctx, next);
            }

            return await current(input);
        }
    }

    /// <summary>
    /// 管道工具类
    /// </summary>
    public static class PipelineUtil
    {
        /// <summary>
        /// 创建管道构建器
        /// </summary>
        public static PipelineBuilder CreateBuilder()
        {
            return new PipelineBuilder();
        }

        /// <summary>
        /// 创建泛型管道
        /// </summary>
        public static Pipeline<TInput, TResult> Create<TInput, TResult>()
        {
            return new Pipeline<TInput, TResult>();
        }

        /// <summary>
        /// 快速执行管道
        /// </summary>
        public static async Task ExecuteAsync(Action<PipelineBuilder> configure, PipelineContext? context = null)
        {
            var builder = new PipelineBuilder();
            configure(builder);
            var pipeline = builder.Build();
            await pipeline(context ?? new PipelineContext());
        }
    }
}
