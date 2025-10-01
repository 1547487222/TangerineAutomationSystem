using Microsoft.Extensions.Logging;
using QStandaedPlatform.Engine.Laboratory;

namespace QStandaedPlatform.Engine.Common.Common
{

    public class ToolExecutionContext
    {
        private readonly Dictionary<PinInfo, PinExecuter> _pinExecuters = [];
        private CancellationTokenSource _cancellationTokenSource;
        private readonly ILogger<ToolExecutionContext> _logger;
        public ToolExecutionContext(Flow flow, Tool tool)
        {
            this.Flow = flow;
            this.Tool = tool;
            _logger = LoggerProviderManager.GetLoggerFactory().CreateLogger<ToolExecutionContext>();
            
            flow.FlowState.SubscribeStateChanged(tool, Subscribe);
            if (IsSyncInputTool)
            {
                tool.ToolExecuter = new SyncInputToolExecuter(this);
            }
            else
            {
                tool.ToolExecuter = new ToolExecuter(this);
            }
            _cancellationTokenSource = new CancellationTokenSource();
            tool.ToolStateChange += Tool_ToolStateChange;
        }

        private void Tool_ToolStateChange(ToolState<ToolState> state)
        {
            if (state.State == ToolState.Error)
            {
                ErrorMessages[Tool.UniqueId] = (DateTime.Now, state.Description);
                _logger.LogError($"Tool {Tool.DisplayName} is error, error message is {state.Description}");
                Flow.FlowState.UpdateRunState(FlowState.Error);
            }
            _logger.LogInformation($"Tool {Tool.DisplayName} state is {state.State}, description is {state.Description}");
        }

        public bool IsSyncInputTool => Tool is SyncInputToolBase;

        public Flow  Flow { get; set; }
   
        public Tool Tool { get; set; }
        public static Dictionary<Guid, (DateTime recordTime, string? message)> ErrorMessages { get; }= new Dictionary<Guid, (DateTime recordTime, string? message)>();

        public CancellationToken CancellationToken => Flow.FlowShutToken;

        public CancellationToken RequestCancelToken => Flow.RequestCancelToken;

        public CancellationToken ToolCancelToken => _cancellationTokenSource.Token;
        /// <summary>
        /// 当前工具执行状态
        /// </summary>
        public bool IsRunning => this.Flow.FlowState.IsRunning;

        public void StartToolExecution()
        {
            if (Tool.InputPins.Count > 0)
            {
                foreach (var pinInfo in Tool.InputPins)
                {
                    _pinExecuters[pinInfo] = new PinExecuter(new PinExecutionContext(this, pinInfo));
                    if (!_pinExecuters[pinInfo].IsRunning)
                        _pinExecuters[pinInfo].Start();
                }
                if (Tool.ToolExecuter?.IsRunning == false)
                    Tool.ToolExecuter?.Start();
            }
        }

        public void StopToolExecution()
        {
            if (_pinExecuters.Count > 0)
            {
                foreach (var pinExecuter in _pinExecuters.Values)
                {
                    pinExecuter.Stop();
                }
                Tool.ToolExecuter?.Stop();
                _pinExecuters.Clear();
            }
        }

        public void DestroySyncCache()
        {
            if (IsSyncInputTool)
            {
                if (Tool is SyncInputToolBase syncInputTool)
                {
                    syncInputTool.ClearCache();
                }
            }
        }

        public void InitSyncCache()
        {
            if (IsSyncInputTool)
            {
                if (Tool is SyncInputToolBase syncInputTool)
                {
                    syncInputTool.InitCache();
                }
            }
        }
        public async Task<bool> VerifyToolStateAsync()
        {
            if (!IsRunning)
            {
              await Tool.RaiseToolStateChangeAsync(ToolState.Error, $"当前流程状态{Flow.FlowState.State},执行取消");
                return false;
            }
            return true;
        }

        public event EventHandler? TryToExecute;

        public void Subscribe(StateChangedEventArgs<FlowEnumWrapper> stateChangeEventArgs)
        {
            if (stateChangeEventArgs.NewState == FlowState.Running)
            {
                _cancellationTokenSource = new CancellationTokenSource();
                TryToExecute?.Invoke(this, EventArgs.Empty);
                ErrorMessages.Remove(Tool.UniqueId);
            }
            if (stateChangeEventArgs.NewState == FlowState.Error)
            {
                if (Flow.ToolErrorCanCancel)
                {
                    CauseErrorOfCancel();
                }
            }
        }

        public Task RequestStartAsync()
        {
            return Flow.RequestStartAsync();
        }


        public bool UpdateFlowState(FlowState flowState)
        {
            return Flow.FlowState.UpdateRunState(flowState);
        }


        public void ClearPinCache()
        {
            lock (_pinExecuters)
            {
                foreach (var pin in _pinExecuters)
                {
                    pin.Value.CleanPinDataTransmit();
                }
                Tool.ToolExecuter?.ClearData();
            }
        }

        public void CauseErrorOfCancel()
        {
            _cancellationTokenSource.Cancel();
        }
        /// <summary>
        /// 工具执行信号
        /// </summary>
        /// <param name="waitHandle"></param>
        /// <returns></returns>
        public int ToolStowSignal(WaitHandle waitHandle)
        {
            var signal = WaitHandle.WaitAny([RequestCancelToken.WaitHandle, ToolCancelToken.WaitHandle, waitHandle]);
            return signal;
        }

        public SemaphoreSlim  SyncInputLock { get; set; } = new SemaphoreSlim(1, 1);

    }
}
