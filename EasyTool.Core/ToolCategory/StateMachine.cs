using System;
using System.Collections.Generic;

namespace EasyTool.ToolCategory
{
    /// <summary>
    /// 状态机工具类
    /// </summary>
    /// <remarks>
    /// 线程安全：是。使用 lock 保护状态转换。
    /// </remarks>
    /// <typeparam name="TState">状态类型</typeparam>
    /// <typeparam name="TTrigger">触发器类型</typeparam>
    public class StateMachine<TState, TTrigger> where TState : notnull where TTrigger : notnull
    {
        private readonly Dictionary<TState, StateConfiguration> _configurations = new();
        private readonly object _lock = new();

        /// <summary>
        /// 当前状态
        /// </summary>
        public TState CurrentState { get; private set; }

        /// <summary>
        /// 状态变更事件
        /// </summary>
        public event EventHandler<StateTransitionEventArgs>? StateChanged;

        /// <summary>
        /// 状态转换事件
        /// </summary>
        public event EventHandler<StateTransitionEventArgs>? Transitioning;

        /// <summary>
        /// 创建状态机
        /// </summary>
        public StateMachine(TState initialState)
        {
            CurrentState = initialState;
        }

        /// <summary>
        /// 配置状态
        /// </summary>
        public StateConfiguration Configure(TState state)
        {
            if (!_configurations.TryGetValue(state, out var config))
            {
                config = new StateConfiguration(state);
                _configurations[state] = config;
            }
            return config;
        }

        /// <summary>
        /// 触发转换
        /// </summary>
        public void Fire(TTrigger trigger)
        {
            lock (_lock)
            {
                if (!_configurations.TryGetValue(CurrentState, out var config))
                    throw new InvalidOperationException($"状态 {CurrentState} 未配置");

                if (!config.Transitions.TryGetValue(trigger, out var transition))
                    throw new InvalidOperationException($"状态 {CurrentState} 不支持触发器 {trigger}");

                var args = new StateTransitionEventArgs(CurrentState, transition.Destination, trigger);

                Transitioning?.Invoke(this, args);

                config.ExitAction?.Invoke();
                transition.Action?.Invoke();

                var previousState = CurrentState;
                CurrentState = transition.Destination;

                if (_configurations.TryGetValue(CurrentState, out var newConfig))
                {
                    newConfig.EntryAction?.Invoke();
                }

                StateChanged?.Invoke(this, args);
            }
        }

        /// <summary>
        /// 尝试触发转换
        /// </summary>
        public bool TryFire(TTrigger trigger)
        {
            try
            {
                Fire(trigger);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 是否可以触发
        /// </summary>
        public bool CanFire(TTrigger trigger)
        {
            lock (_lock)
            {
                if (!_configurations.TryGetValue(CurrentState, out var config))
                    return false;

                return config.Transitions.ContainsKey(trigger);
            }
        }

        /// <summary>
        /// 获取当前状态可用的触发器
        /// </summary>
        public IEnumerable<TTrigger> GetPermittedTriggers()
        {
            lock (_lock)
            {
                if (_configurations.TryGetValue(CurrentState, out var config))
                    return config.Transitions.Keys;
                return Array.Empty<TTrigger>();
            }
        }

        /// <summary>
        /// 状态配置
        /// </summary>
        public class StateConfiguration
        {
            private readonly TState _state;
            internal readonly Dictionary<TTrigger, Transition> Transitions = new();
            internal Action? EntryAction;
            internal Action? ExitAction;

            internal StateConfiguration(TState state)
            {
                _state = state;
            }

            /// <summary>
            /// 配置进入动作
            /// </summary>
            public StateConfiguration OnEntry(Action action)
            {
                EntryAction = action;
                return this;
            }

            /// <summary>
            /// 配置退出动作
            /// </summary>
            public StateConfiguration OnExit(Action action)
            {
                ExitAction = action;
                return this;
            }

            /// <summary>
            /// 配置转换
            /// </summary>
            public StateConfiguration Permit(TTrigger trigger, TState destination)
            {
                Transitions[trigger] = new Transition(destination, null);
                return this;
            }

            /// <summary>
            /// 配置转换（带动作）
            /// </summary>
            public StateConfiguration Permit(TTrigger trigger, TState destination, Action action)
            {
                Transitions[trigger] = new Transition(destination, action);
                return this;
            }

            /// <summary>
            /// 忽略触发器
            /// </summary>
            public StateConfiguration Ignore(TTrigger trigger)
            {
                Transitions[trigger] = new Transition(_state, null);
                return this;
            }
        }

        internal class Transition
        {
            public TState Destination { get; }
            public Action? Action { get; }

            public Transition(TState destination, Action? action)
            {
                Destination = destination;
                Action = action;
            }
        }
    }

    /// <summary>
    /// 状态转换事件参数
    /// </summary>
    public class StateTransitionEventArgs : EventArgs
    {
        /// <summary>
        /// 源状态
        /// </summary>
        public object SourceState { get; }

        /// <summary>
        /// 目标状态
        /// </summary>
        public object DestinationState { get; }

        /// <summary>
        /// 触发器
        /// </summary>
        public object Trigger { get; }

        internal StateTransitionEventArgs(object source, object destination, object trigger)
        {
            SourceState = source;
            DestinationState = destination;
            Trigger = trigger;
        }
    }
}
