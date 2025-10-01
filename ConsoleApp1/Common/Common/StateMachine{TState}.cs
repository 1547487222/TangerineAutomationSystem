using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common
{
    /// <summary>
    /// 状态机基类
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    public abstract class StateMachine<TState> where TState : IEquatable<TState>
    {
        protected  TState _currentState;
        protected readonly Dictionary<TState, string> _stateNames;

        protected readonly List<AutoTransitionRule<TState>> _autoTransitions;
        protected readonly Dictionary<TState, List<TState>> _transitions;

        private readonly Dictionary<object, Action<StateChangedEventArgs<TState>>> _stateChangeActions;

        private readonly object _lock = new();
        private CancellationTokenSource _autoTransitionCts;

        public event EventHandler<StateChangedEventArgs<TState>>? StateChanged;

        protected StateMachine()
        {
            _currentState = default;
            _stateNames = [];
            _transitions = [];
            _autoTransitions = [];
            _stateChangeActions = [];
            _autoTransitionCts = new CancellationTokenSource();


            InitializeStates();
            InitializeTransitions();
            InitializeAutoTransitions();
        }

        protected abstract void InitializeStates();

        protected virtual void InitializeTransitions() { }

        protected virtual void InitializeAutoTransitions() { }

        public bool SetInitialState(TState state)
        {
            lock (_lock)
            {

                if (!_stateNames.ContainsKey(state))
                {
                    throw new InvalidOperationException($"Target state {state} does not exist.");
                }
                _currentState = state;
                Console.WriteLine($"Initial state set to {GetStateName(state)} ({state})");
                return true;
            }
        }

        protected void AddState(TState stateId, string stateName)
        {
            lock (_lock)
            {
                if (!_stateNames.ContainsKey(stateId))
                {
                    _stateNames[stateId] = stateName;
                    _transitions[stateId] = [];
                }
                else
                {
                    throw new InvalidOperationException($"State {stateId} ({stateName}) already exists.");
                }
            }
        }

        protected void AddTransition(TState fromState, TState toState)
        {
            lock (_lock)
            {
                if (!_stateNames.ContainsKey(fromState) || !_stateNames.ContainsKey(toState))
                {
                    throw new InvalidOperationException($"Invalid state: {fromState} or {toState} does not exist.");
                }

                if (!_transitions[fromState].Contains(toState))
                {
                    _transitions[fromState].Add(toState);
                }
            }
        }

        protected void AddAutoTransition(TState fromState, TState toState, Func<bool> condition)
        {
            lock (_lock)
            {
                if (!_stateNames.ContainsKey(fromState) || !_stateNames.ContainsKey(toState))
                {
                    throw new InvalidOperationException($"Invalid state for auto-transition: {fromState} or {toState} does not exist.");
                }

                _autoTransitions.Add(new AutoTransitionRule<TState>(fromState, toState, condition));
            }
        }

        public TState State
        {
            get
            {
                lock (_lock)
                {
                    return _currentState;
                }
            }
        }

        public string StateName
        {
            get
            {
                lock (_lock)
                {
                    return _stateNames.TryGetValue(_currentState, out string? value) ? value : "Unknown";
                }
            }
        }

        public void StartAutoTransitions(int checkIntervalMs = 1000)
        {
            Task.Run(async () =>
            {
                while (!_autoTransitionCts.Token.IsCancellationRequested)
                {
                    CheckAutoTransitions();
                    await Task.Delay(checkIntervalMs, _autoTransitionCts.Token);
                }
            }, _autoTransitionCts.Token);
        }

        public void StopAutoTransitions()
        {
            _autoTransitionCts.Cancel();
            _autoTransitionCts.Dispose();
            _autoTransitionCts = new CancellationTokenSource();
        }


        protected void CheckAutoTransitions()
        {
            lock (_lock)
            {
                foreach (var rule in _autoTransitions)
                {
                    if (rule.FromState.Equals(_currentState) && rule.Condition())
                    {
                        UpdateRunState(rule.ToState);
                        break;
                    }
                }
            }
        }

        public bool UpdateRunState(TState newState)
        {
            lock (_lock)
            {
                TState oldState = _currentState;

                if (!_stateNames.ContainsKey(newState))
                {
                    throw new InvalidOperationException($"Target state {newState} does not exist.");
                }

                if (!oldState.Equals(newState))
                {
                    if (!IsValidStateTransition(oldState, newState))
                    {
                        // throw new InvalidOperationException($"Invalid transition from {GetStateName(oldState)} ({oldState}) to {GetStateName(newState)} ({newState})");

                        return false;
                    }
                }

                _currentState = newState;

                if (!oldState.Equals(_currentState))
                {
                    var args = new StateChangedEventArgs<TState>(this,oldState, _currentState, GetStateName(oldState), GetStateName(_currentState));
                    foreach (var action in _stateChangeActions.Values.ToList())
                    {
                        action?.Invoke(args);
                    }
                    StateChanged?.Invoke(this, args);
                }

                return true;
            }
        }

        protected virtual bool IsValidStateTransition(TState currentState, TState newState)
        {
            return _transitions.ContainsKey(currentState) && _transitions[currentState].Contains(newState);
        }

        protected string GetStateName(TState stateId)
        {
            return _stateNames.TryGetValue(stateId, out string? value) ? value : "Unknown";
        }

        public void SubscribeStateChanged(object subscriber, Action<StateChangedEventArgs<TState>> action)
        {
            lock (_lock)
            {
                _stateChangeActions[subscriber] = action;
            }
        }

        // 取消订阅状态变更
        public void UnsubscribeStateChanged(object subscriber)
        {
            lock (_lock)
            {
                _stateChangeActions.Remove(subscriber);
            }
        }
    }
    public class StateChangedEventArgs<TState>(StateMachine<TState> stateMachine, TState oldState, TState newState, string oldStateName, string newStateName) : EventArgs where TState : IEquatable<TState>
    {
        public StateMachine<TState> StateMachine { get; } = stateMachine;
        public TState OldState { get; } = oldState;
        public TState NewState { get; } = newState;
        public string OldStateName { get; } = oldStateName;
        public string NewStateName { get; } = newStateName;
    }
    public class AutoTransitionRule<TState>(TState fromState, TState toState, Func<bool> condition) where TState : IEquatable<TState>
    {
        public TState FromState { get; } = fromState;
        public TState ToState { get; } = toState;
        public Func<bool> Condition { get; } = condition;
    }
}
