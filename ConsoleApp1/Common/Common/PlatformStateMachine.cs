using Microsoft.Extensions.Logging;
using QStandaedPlatform.Engine.Laboratory;
using System;

namespace QStandaedPlatform.Engine.Common.Common
{
    public enum InternalPlatformStatus
    {
        /// <summary>
        /// 未初始化
        /// </summary>
        NotInitialized = 0,
        /// <summary>
        /// 已启动
        /// </summary>
        Started = 1,
        /// <summary>
        /// 任务进行中
        /// </summary>
        InProgress = 2,
       /// <summary>
       /// 空闲
       /// </summary>
        Idle = 3,
        /// <summary>
        /// 异常
        /// </summary>
        Failed = 4,
        /// <summary>
        /// 暂停
        /// </summary>
        Paused = 5,
        /// <summary>
        /// 停止
        /// </summary>
        Stop = 6,
    }

    public enum StateChangeEvent
    {
        Initialize,
        Launch,
        StartTask,
        Pause,
        Resume,
        Stop,
        ErrorOccurred,
        TaskCompleted,
        Reset,
        ClearResource,
    }

    public delegate void StateTransitionHandler(PlatformStateMachine context, InternalPlatformStatus newState);

    public interface IPlatformState
    {
        InternalPlatformStatus Status { get; }

        bool CanTransition(StateChangeEvent transition);

        void HandleTransition(PlatformStateMachine context, StateChangeEvent transition);
    }

    public abstract class PlatformStateBase : IPlatformState
    {
        public abstract InternalPlatformStatus Status { get; }

        public virtual bool CanTransition(StateChangeEvent transition) => false;

        public virtual void HandleTransition(PlatformStateMachine context, StateChangeEvent transition)
        {
            ThrowInvalidTransition(transition.ToString());
        }

        protected void ThrowInvalidTransition(string action)
        {
            throw new InvalidOperationException($"Cannot perform {action} from state {Status}");
        }
    }

    /// <summary> 未初始化状态 </summary>
    public class NotInitializedState : PlatformStateBase
    {
        public override InternalPlatformStatus Status => InternalPlatformStatus.NotInitialized;

        public override bool CanTransition(StateChangeEvent transition) =>
            transition is StateChangeEvent.Initialize;

        public override void HandleTransition(PlatformStateMachine context, StateChangeEvent transition)
        {
            if (transition == StateChangeEvent.Initialize)
            {
                context.SetState(new IdleState());
                context.LogStateChange("Initialize", InternalPlatformStatus.Idle);
            }
        }
    }

    /// <summary> 启动状态 </summary>
    public class StartedState : PlatformStateBase
    {
        public override InternalPlatformStatus Status => InternalPlatformStatus.Started;

        public override bool CanTransition(StateChangeEvent transition) =>
            transition is StateChangeEvent.StartTask
            or StateChangeEvent.Stop
            or StateChangeEvent.Initialize
            or StateChangeEvent.ErrorOccurred;

        public override void HandleTransition(PlatformStateMachine context, StateChangeEvent transition)
        {
            if (transition == StateChangeEvent.StartTask)
            {
                context.SetState(new InProgressState());
                context.LogStateChange("StartTask", InternalPlatformStatus.InProgress);
            }
            if (transition == StateChangeEvent.Stop)
            {
                context.SetState(new StoppedState());
                context.LogStateChange("Stop", InternalPlatformStatus.Stop);
            }
            if (transition == StateChangeEvent.Initialize)
            {
                context.SetState(new IdleState());
                context.LogStateChange("Initialize", InternalPlatformStatus.Idle);
            }
            if (transition == StateChangeEvent.ErrorOccurred)
            {
                context.SetState(new FailedState());
                context.LogStateChange("ErrorOccurred", InternalPlatformStatus.Failed);
            }
        }
    }

    /// <summary> 闲置状态 </summary>
    public class IdleState : PlatformStateBase
    {
        public override InternalPlatformStatus Status => InternalPlatformStatus.Idle;

        public override bool CanTransition(StateChangeEvent transition) =>
            transition is StateChangeEvent.Launch
                       or StateChangeEvent.Resume
                       or StateChangeEvent.Stop
                       or StateChangeEvent.Initialize;

        public override void HandleTransition(PlatformStateMachine context, StateChangeEvent transition)
        {
            if (transition == StateChangeEvent.Launch)
            {
                context.SetState(new StartedState());
                context.LogStateChange("Launch", InternalPlatformStatus.Started);
            }
            if (transition == StateChangeEvent.Resume)
            {
                context.SetState(new InProgressState());
                context.LogStateChange("Resume", InternalPlatformStatus.InProgress);
            }
            if (transition == StateChangeEvent.Stop)
            {
                context.SetState(new StoppedState());
                context.LogStateChange("Stop", InternalPlatformStatus.Stop);
            }
        }
    }

    /// <summary> 运行中状态 </summary>
    public class InProgressState : PlatformStateBase
    {
        public override InternalPlatformStatus Status => InternalPlatformStatus.InProgress;

        public override bool CanTransition(StateChangeEvent transition) =>
            transition is StateChangeEvent.Pause
                       or StateChangeEvent.Stop
                       or StateChangeEvent.ErrorOccurred
                       or StateChangeEvent.TaskCompleted;

        public override void HandleTransition(PlatformStateMachine context, StateChangeEvent transition)
        {
            switch (transition)
            {
                case StateChangeEvent.Pause:
                    context.SetState(new PausedState());
                    context.LogStateChange("Pause", InternalPlatformStatus.Paused);
                    break;

                case StateChangeEvent.Stop:
                    context.SetState(new StoppedState());
                    context.LogStateChange("Stop", InternalPlatformStatus.Stop);
                    context.SetState(new NotInitializedState());
                    context.LogStateChange("Reset", InternalPlatformStatus.NotInitialized);
                    break;

                case StateChangeEvent.ErrorOccurred:
                    context.SetState(new FailedState());
                    context.LogStateChange("ErrorOccurred", InternalPlatformStatus.Failed);
                    break;

                case StateChangeEvent.TaskCompleted:
                    context.SetState(new StartedState());
                    context.LogStateChange("TaskCompleted", InternalPlatformStatus.Idle);
                    break;
            }
        }
    }

    /// <summary> 暂停状态 </summary>
    public class PausedState : PlatformStateBase
    {
        public override InternalPlatformStatus Status => InternalPlatformStatus.Paused;

        public override bool CanTransition(StateChangeEvent transition) =>
            transition is StateChangeEvent.Resume
                       or StateChangeEvent.Stop;

        public override void HandleTransition(PlatformStateMachine context, StateChangeEvent transition)
        {
            switch (transition)
            {
                case StateChangeEvent.Resume:
                    context.SetState(new InProgressState());
                    context.LogStateChange("Resume", InternalPlatformStatus.InProgress);
                    break;

                case StateChangeEvent.Stop:
                    context.SetState(new StoppedState());
                    context.LogStateChange("Stop", InternalPlatformStatus.Stop);
                    break;
            }
        }
    }

    /// <summary> 停止状态 </summary>
    public class StoppedState : PlatformStateBase
    {
        public override InternalPlatformStatus Status => InternalPlatformStatus.Stop;

        public override bool CanTransition(StateChangeEvent transition) =>
            transition == StateChangeEvent.ClearResource;

        public override void HandleTransition(PlatformStateMachine context, StateChangeEvent transition)
        {
            if (transition == StateChangeEvent.ClearResource)
            {
                context.SetState(new NotInitializedState());
                context.LogStateChange("ClearResource", InternalPlatformStatus.NotInitialized);
            }
        }
    }

    /// <summary> 错误/报警状态 </summary>
    public class FailedState : PlatformStateBase
    {
        public override InternalPlatformStatus Status => InternalPlatformStatus.Failed;

        public override bool CanTransition(StateChangeEvent transition) =>
            transition is StateChangeEvent.Reset 
                       or StateChangeEvent.Stop;

        public override void HandleTransition(PlatformStateMachine context, StateChangeEvent transition)
        {
            if (transition == StateChangeEvent.Reset)
            {
                context.SetState(new IdleState());
                context.LogStateChange("Reset", InternalPlatformStatus.Idle);
            }
            else if (transition == StateChangeEvent.Stop)
            {
                context.SetState(new StoppedState());
                context.LogStateChange("Stop", InternalPlatformStatus.Stop);
            }
        }
    }

    /// <summary> 状态机 </summary>
    public class PlatformStateMachine
    {
        private IPlatformState _currentState;
        private readonly ILogger<PlatformStateMachine> _logger;
        private readonly PlatformCallService _platformCallService;
        private readonly object _lock = new();

        public event StateTransitionHandler? OnInitialize;
        public event StateTransitionHandler? OnStartTask;
        public event StateTransitionHandler? OnPause;
        public event StateTransitionHandler? OnResume;
        public event StateTransitionHandler? OnStop;
        public event StateTransitionHandler? OnErrorOccurred;
        public event StateTransitionHandler? OnReset;
        public event StateTransitionHandler? OnTaskCompleted;

        public PlatformStateMachine(PlatformCallService platformCallService)
        {
            _currentState = new NotInitializedState();
            _platformCallService = platformCallService;
            _logger = LoggerProviderManager.GetLoggerFactory().CreateLogger<PlatformStateMachine>();
        }

        public IPlatformState CurrentPlatformState
        {
            get
            {
                lock (_lock)
                {
                    return _currentState;
                }
            }
        }

        public bool CanTransition(StateChangeEvent transition)
        {
            lock (_lock)
            {
                return _currentState.CanTransition(transition);
            }
        }

        public void TriggerTransition(StateChangeEvent transition)
        {
            lock (_lock)
            {
                if (!_currentState.CanTransition(transition))
                {
                    _logger.LogWarning($"Invalid transition '{transition}' from state {_currentState.Status}");
                    return; // 或者抛异常
                }

                var previousState = _currentState.Status;
                _currentState.HandleTransition(this, transition);
                var newState = _currentState.Status;

                switch (transition)
                {
                    case StateChangeEvent.Initialize:
                        if (newState == InternalPlatformStatus.Idle)
                            OnInitialize?.Invoke(this, newState);
                        break;
                    case StateChangeEvent.Launch:
                        if (newState == InternalPlatformStatus.Started)
                            OnStartTask?.Invoke(this, newState);
                        break;
                    case StateChangeEvent.StartTask:
                        if (newState == InternalPlatformStatus.InProgress)
                            OnStartTask?.Invoke(this, newState);
                        break;
                    case StateChangeEvent.Pause:
                        if (newState == InternalPlatformStatus.Paused)
                            OnPause?.Invoke(this, newState);
                        break;
                    case StateChangeEvent.Resume:
                        if (newState == InternalPlatformStatus.InProgress)
                            OnResume?.Invoke(this, newState);
                        break;
                    case StateChangeEvent.Stop:
                        if (newState == InternalPlatformStatus.NotInitialized)
                            OnStop?.Invoke(this, newState);
                        break;
                    case StateChangeEvent.ErrorOccurred:
                        if (newState == InternalPlatformStatus.Failed)
                            OnErrorOccurred?.Invoke(this, newState);
                        break;
                    case StateChangeEvent.Reset:
                        if (newState == InternalPlatformStatus.InProgress)
                            OnReset?.Invoke(this, newState);
                        break;
                    case StateChangeEvent.TaskCompleted:
                        if (newState == InternalPlatformStatus.Idle)
                            OnTaskCompleted?.Invoke(this, newState);
                        break;
                }
            }
        }

        //转移并校验状态
        public void TransitionAndValidateState(StateChangeEvent transition, InternalPlatformStatus expectedState)
        {
            if (CanTransition(transition))
            {
                TriggerTransition(transition);
                if (CurrentPlatformState.Status != expectedState)
                    throw new InvalidOperationException($"State transition failed. Expected state: {expectedState}, Actual state: {CurrentPlatformState.Status}");
            }
        }

        internal void SetState(IPlatformState newState)
        {
            lock (_lock)
            {
                _currentState = newState;
            }
        }

        internal void LogStateChange(string action, InternalPlatformStatus newState)
        {
            lock (_lock)
            {
                _logger.LogInformation($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] StateMachine: Transitioned to {newState} via {action}");
            }
        }
    }
}
