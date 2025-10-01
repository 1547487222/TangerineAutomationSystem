using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Laboratory
{
    /// <summary>
    /// 模块状态
    /// </summary>
    public class ModuleState
    {
        /// <summary>
        /// 模块枚举状态
        /// </summary>
        private enum State
        {
            Idle = 1,    // 空闲
            Busy = 2,    // 忙碌
            Legacy = 3   // 遗留
        }

        private volatile State _state = State.Idle;
        private readonly object _lock = new();

        /// <summary>
        /// 是否空闲
        /// </summary>
        public bool IsIdle
        {
            get
            {
                lock (_lock)
                {
                    return _state == State.Idle;
                }
            }
        }
        /// <summary>
        /// 是否忙碌
        /// </summary>
        public bool IsBusy
        {
            get
            {
                lock (_lock)
                {
                    return _state == State.Busy;
                }
            }
        }
        /// <summary>
        /// 是否是遗留
        /// </summary>
        public bool IsLegacy
        {
            get
            {
                lock (_lock) 
                {
                    return _state == State.Legacy;
                }
            }
        }
        /// <summary>
        /// 设置空闲
        /// </summary>
        public void SetIdle()
        {
            lock (_lock)
            {
                _state = State.Idle;
            }
        }
        /// <summary>
        /// 设置忙碌
        /// </summary>
        public void SetBusy()
        {
            lock (_lock)
            {
                _state = State.Busy;
            }
        }
        /// <summary>
        /// 设置遗留
        /// </summary>
        public void SetLegacy() 
        {
            lock (_lock)
            {
                _state = State.Legacy;
            }
        }

        /// <summary>
        /// 行为
        /// </summary>
        public ModuleBehavior? Behavior { get; set; }

        /// <summary>
        /// 器皿
        /// </summary>
        public object? Label { get; set; }

        /// <summary>
        /// 是否可以进入
        /// </summary>
        /// <returns></returns>
        public bool CanEnter(object label)
        {
            lock (_lock)
            {
                if (IsLegacy)
                {
                    return Label == label;
                }
            }
            return IsIdle;
        }
        /// <summary>
        /// 进入
        /// </summary>
        /// <param name="labwares"></param>
        public void Enter(object label)
        {
            lock (_lock)
            {
                Label = label;
                SetBusy();
            }
        }

        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="behavior"></param>
        public void Execute(ModuleBehavior? behavior)
        {
            lock (_lock)
            {
                Behavior = behavior;
            }
        }
        /// <summary>
        /// 退出
        /// </summary>
        public void Exit()
        {
            lock (_lock)
            {
                if (Behavior != null && Behavior.IsProductLegacy())
                {
                    SetLegacy();
                }
                else
                {
                    Behavior = null;
                    SetIdle();
                }
            }
        }
    }
}
