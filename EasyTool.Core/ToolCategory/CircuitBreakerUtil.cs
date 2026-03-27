using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EasyTool.ToolCategory
{
    /// <summary>
    /// 熔断器状态
    /// </summary>
    public enum CircuitState
    {
        /// <summary>
        /// 关闭（正常）
        /// </summary>
        Closed,

        /// <summary>
        /// 开启（熔断）
        /// </summary>
        Open,

        /// <summary>
        /// 半开（尝试恢复）
        /// </summary>
        HalfOpen
    }

    /// <summary>
    /// 熔断器配置
    /// </summary>
    public class CircuitBreakerOptions
    {
        /// <summary>
        /// 失败阈值（触发熔断的最小失败次数）
        /// </summary>
        public int FailureThreshold { get; set; } = 5;

        /// <summary>
        /// 成功阈值（半开状态下恢复的最小成功次数）
        /// </summary>
        public int SuccessThreshold { get; set; } = 2;

        /// <summary>
        /// 熔断持续时间
        /// </summary>
        public TimeSpan BreakDuration { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// 熔断超时时间
        /// </summary>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);

        /// <summary>
        /// 判断是否应该熔断的异常类型
        /// </summary>
        public List<Type> ExceptionTypesToTrack { get; set; } = new() { typeof(Exception) };
    }

    /// <summary>
    /// 熔断器
    /// </summary>
    public class CircuitBreaker
    {
        private readonly CircuitBreakerOptions _options;
        private readonly object _lock = new();
        private CircuitState _state = CircuitState.Closed;
        private int _failureCount;
        private int _successCount;
        private DateTime _lastFailureTime;

        /// <summary>
        /// 当前状态
        /// </summary>
        public CircuitState State
        {
            get
            {
                lock (_lock)
                {
                    if (_state == CircuitState.Open && ShouldAttemptReset())
                    {
                        _state = CircuitState.HalfOpen;
                        _successCount = 0;
                    }
                    return _state;
                }
            }
        }

        /// <summary>
        /// 失败次数
        /// </summary>
        public int FailureCount => _failureCount;

        /// <summary>
        /// 成功次数
        /// </summary>
        public int SuccessCount => _successCount;

        /// <summary>
        /// 最后失败时间
        /// </summary>
        public DateTime LastFailureTime => _lastFailureTime;

        /// <summary>
        /// 状态变更事件
        /// </summary>
        public event EventHandler<CircuitState>? StateChanged;

        /// <summary>
        /// 创建熔断器
        /// </summary>
        public CircuitBreaker(CircuitBreakerOptions? options = null)
        {
            _options = options ?? new CircuitBreakerOptions();
        }

        /// <summary>
        /// 执行操作
        /// </summary>
        public async Task<T> ExecuteAsync<T>(Func<Task<T>> action)
        {
            var state = State;
            
            if (state == CircuitState.Open)
            {
                throw new CircuitBreakerOpenException("熔断器处于开启状态");
            }

            try
            {
                using var cts = new System.Threading.CancellationTokenSource(_options.Timeout);
                var task = action();
                var completedTask = await Task.WhenAny(task, Task.Delay(_options.Timeout));

                if (completedTask != task)
                {
                    OnFailure();
                    throw new TimeoutException("操作超时");
                }

                var result = await task;
                OnSuccess();
                return result;
            }
            catch (Exception ex) when (ShouldTrackException(ex))
            {
                OnFailure();
                throw;
            }
        }

        /// <summary>
        /// 执行操作
        /// </summary>
        public async Task ExecuteAsync(Func<Task> action)
        {
            await ExecuteAsync(async () =>
            {
                await action();
                return true;
            });
        }

        /// <summary>
        /// 尝试执行操作
        /// </summary>
        public async Task<(bool Success, T? Result, Exception? Error)> TryExecuteAsync<T>(Func<Task<T>> action)
        {
            try
            {
                var result = await ExecuteAsync(action);
                return (true, result, null);
            }
            catch (Exception ex)
            {
                return (false, default, ex);
            }
        }

        private bool ShouldAttemptReset()
        {
            return DateTime.UtcNow - _lastFailureTime >= _options.BreakDuration;
        }

        private bool ShouldTrackException(Exception ex)
        {
            foreach (var type in _options.ExceptionTypesToTrack)
            {
                if (type.IsAssignableFrom(ex.GetType()))
                    return true;
            }
            return false;
        }

        private void OnSuccess()
        {
            lock (_lock)
            {
                if (_state == CircuitState.HalfOpen)
                {
                    _successCount++;
                    if (_successCount >= _options.SuccessThreshold)
                    {
                        TripTo(CircuitState.Closed);
                        _failureCount = 0;
                        _successCount = 0;
                    }
                }
                else if (_state == CircuitState.Closed)
                {
                    _failureCount = 0;
                }
            }
        }

        private void OnFailure()
        {
            lock (_lock)
            {
                _lastFailureTime = DateTime.UtcNow;
                _failureCount++;

                if (_state == CircuitState.HalfOpen)
                {
                    TripTo(CircuitState.Open);
                }
                else if (_state == CircuitState.Closed && _failureCount >= _options.FailureThreshold)
                {
                    TripTo(CircuitState.Open);
                }
            }
        }

        private void TripTo(CircuitState newState)
        {
            var oldState = _state;
            _state = newState;
            StateChanged?.Invoke(this, newState);
        }

        /// <summary>
        /// 重置熔断器
        /// </summary>
        public void Reset()
        {
            lock (_lock)
            {
                _state = CircuitState.Closed;
                _failureCount = 0;
                _successCount = 0;
            }
        }

        /// <summary>
        /// 强制打开熔断器
        /// </summary>
        public void Open()
        {
            lock (_lock)
            {
                TripTo(CircuitState.Open);
                _lastFailureTime = DateTime.UtcNow;
            }
        }
    }

    /// <summary>
    /// 熔断器开启异常
    /// </summary>
    public class CircuitBreakerOpenException : Exception
    {
        public CircuitBreakerOpenException(string message) : base(message) { }
    }

    /// <summary>
    /// 熔断器工具类
    /// </summary>
    public static class CircuitBreakerUtil
    {
        /// <summary>
        /// 创建熔断器
        /// </summary>
        public static CircuitBreaker Create(CircuitBreakerOptions? options = null)
        {
            return new CircuitBreaker(options);
        }

        /// <summary>
        /// 创建熔断器
        /// </summary>
        public static CircuitBreaker Create(int failureThreshold, TimeSpan breakDuration)
        {
            return new CircuitBreaker(new CircuitBreakerOptions
            {
                FailureThreshold = failureThreshold,
                BreakDuration = breakDuration
            });
        }
    }
}