

using System;

namespace QStandaedPlatform.Engine.Common.Common
{
    public readonly struct FlowEnumWrapper(FlowState value) : IEquatable<FlowEnumWrapper>
    {
        public FlowState Value { get; } = value;

        public readonly bool Equals(FlowEnumWrapper other) => Value.Equals(other.Value);
        public override bool Equals(object obj) => obj is FlowEnumWrapper other && Equals(other);
        public override readonly int GetHashCode() => (int)Value;

        public static implicit operator FlowEnumWrapper(FlowState state)
        {
            return new FlowEnumWrapper(state);
        }

        public static bool operator ==(FlowEnumWrapper wrapper, FlowState value)
        {
            return wrapper.Value == value;
        }
        public static bool operator !=(FlowEnumWrapper wrapper, FlowState value)
        {
            return !(wrapper == value);
        }
        public static bool operator !=(FlowState value, FlowEnumWrapper wrapper)
        {
            return !(wrapper == value);
        }
        public static bool operator ==(FlowState value, FlowEnumWrapper wrapper)
        {
            return wrapper.Value == value;
        }
        public static bool operator ==(FlowEnumWrapper left, FlowEnumWrapper right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(FlowEnumWrapper left, FlowEnumWrapper right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
    public enum FlowState
    {
        /// <summary>
        /// 待机
        /// </summary>
        Idle,
        /// <summary>
        /// 运行
        /// </summary>
        Running,
        /// <summary>
        /// 暂停
        /// </summary>
        Pause,
        /// <summary>
        /// 工具产生错误
        /// </summary>
         Error,
        /// <summary>
        /// 预警提示（用于工具执行产生的预警提示）
        /// </summary>
        Forewarn,
        /// <summary>
        /// 流程被取消
        /// </summary>
        Cancel,
    }
    public class FlowStateMachine:StateMachine<FlowEnumWrapper>
    {

        /// <summary>
        /// 流程是否运行中
        /// </summary>
        public bool IsRunning => State == FlowState.Running;

        /// <summary>
        /// 流程是否待机
        /// </summary>
        public bool IsIdle => State == FlowState.Idle;

        /// <summary>
        /// 流程请求是否被取消
        /// </summary>
        public bool IsCancel => State == FlowState.Cancel;

        /// <summary>
        /// 流程请求是否错误
        /// </summary>
        public bool IsError => State == FlowState.Error;

        /// <summary>
        /// 流程请求是否被暂停
        /// </summary>
        public bool IsPause => State == FlowState.Pause;

        protected override void InitializeStates()
        {
            AddState(FlowState.Idle, "流程待机");
            AddState(FlowState.Running, "流程运行");
            AddState(FlowState.Pause, "流程暂停");
            AddState(FlowState.Error, "流程错误");
            AddState(FlowState.Forewarn, "流程预警");
            AddState(FlowState.Cancel, "流程取消");
        }

        protected override void InitializeTransitions()
        {
            AddTransition(FlowState.Running, FlowState.Forewarn);
            AddTransition(FlowState.Running, FlowState.Pause);
            AddTransition(FlowState.Running, FlowState.Error);
            AddTransition(FlowState.Running, FlowState.Cancel);
            AddTransition(FlowState.Idle, FlowState.Running);
            AddTransition(FlowState.Idle, FlowState.Pause);
            AddTransition(FlowState.Idle, FlowState.Error);
            AddTransition(FlowState.Idle, FlowState.Forewarn);
            AddTransition(FlowState.Idle, FlowState.Cancel);
            AddTransition(FlowState.Cancel, FlowState.Idle);
            AddTransition(FlowState.Pause, FlowState.Running);
            AddTransition(FlowState.Pause, FlowState.Idle);
            AddTransition(FlowState.Error, FlowState.Idle);
            AddTransition(FlowState.Error, FlowState.Running);
            AddTransition(FlowState.Forewarn, FlowState.Idle);
            AddTransition(FlowState.Forewarn, FlowState.Running);
            AddTransition(FlowState.Forewarn, FlowState.Pause);
            AddTransition(FlowState.Forewarn, FlowState.Error);
            AddTransition(FlowState.Forewarn, FlowState.Cancel);
        }
    }
}
