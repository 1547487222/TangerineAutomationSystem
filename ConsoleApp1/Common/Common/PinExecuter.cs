using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common
{
    public class PinExecuter : BackgroundWork, IDisposable
    {
        private readonly PinExecutionContext _pinExecutionContext;
        private readonly object _lock = new();
        private readonly AtomicBool _haveData = new(false);
        private volatile bool _running = false;
        private readonly AutoResetEvent _pinDataTransmitEvent = new(false);
        private readonly Queue<PinDataTransmitEventArgs> _pinDataTransmitEventArgs = new();
        private readonly ILogger _logger;
        public PinExecuter(PinExecutionContext pinExecutionContext)
        {
            _logger = LoggerProviderManager.GetLoggerFactory().CreateLogger($"PinExecuter.{pinExecutionContext.PinName}");
            _pinExecutionContext = pinExecutionContext;
            _pinExecutionContext.DataTransmit.OnPinDataTransmit += PinInfo_OnPinDataTransmit;
            _pinExecutionContext.ToolExecutionContext.TryToExecute += ToolExecutionContext_TryToExecute;
        }

        private void ToolExecutionContext_TryToExecute(object? sender, EventArgs e)
        {
            _ = NotifyIfFlowIsRunningAsync();
        }

        public override async Task DoWorkAsync(CancellationToken cancellationToken)
        {
            while (!TokenSource.IsCancellationRequested)
            {
                var num = WaitHandle.WaitAny([_pinDataTransmitEvent, cancellationToken.WaitHandle]);
                if (num == 0)
                {
                    if (_running)
                    {
                        await _pinExecutionContext.ToolExecutionContext.Tool.RaiseToolStateChangeAsync(ToolState.SyncWaiting, $"工具处理排队中：{_pinDataTransmitEventArgs.Count}");
                        continue;
                    }
                    else
                    {
                        _running = true;
                    }
                    _running = true;
                    while (_haveData.Get() && await _pinExecutionContext.ToolExecutionContext.VerifyToolStateAsync())
                    {
                        PinDataTransmitEventArgs? pinDataTransmit = default;
                        lock (_lock)
                        {
                            if (_pinDataTransmitEventArgs.Count > 0)
                            {
                                _pinDataTransmitEventArgs.TryPeek(out pinDataTransmit);
                            }
                        }
                        if (pinDataTransmit != null &&await _pinExecutionContext.ToolExecutionContext.VerifyToolStateAsync())
                        {
                            try
                            {
                                await _pinExecutionContext.ToolExecutionContext.Tool.PreparePinDataBeforeRequest(_pinExecutionContext.ToolExecutionContext, pinDataTransmit);
                                var result = await _pinExecutionContext.ToolExecutionContext.Tool.RequestRecvHandlePinAsync(_pinExecutionContext.ToolExecutionContext, pinDataTransmit);
                                if (!result)
                                {
                                    _pinExecutionContext.ToolExecutionContext.UpdateFlowState(FlowState.Error);
                                   await _pinExecutionContext.ToolExecutionContext.Tool.RaiseToolStateChangeAsync(ToolState.Error, "执行错误");
                                    break;
                                }
                                else
                                    lock (_lock)
                                    {
                                        if (_pinDataTransmitEventArgs.Count > 0)
                                        {
                                            _pinDataTransmitEventArgs.TryDequeue(out _);
                                        }

                                    }
                            }
                            catch (Exception ex)
                            {
                                if (!await _pinExecutionContext.ToolExecutionContext.Tool.HandleExecutedErrorAsync(ex))
                                {
                                   await _pinExecutionContext.ToolExecutionContext.Flow.HandleRunErrorAsync(ex);
                                    _pinExecutionContext.ToolExecutionContext.UpdateFlowState(FlowState.Error);
                                    await _pinExecutionContext.ToolExecutionContext.Tool.RaiseToolStateChangeAsync(ToolState.Error, ex.Message);
                                    _pinExecutionContext.ToolExecutionContext.Tool.Logger?.LogError($"执行错误{ex}");
                                    break;
                                }
                            }
                        }
                        if (_pinDataTransmitEventArgs.Count == 0)
                        {
                            lock (_lock)
                            {
                                if (_pinDataTransmitEventArgs.Count == 0)
                                {
                                    break;
                                }
                            }
                        }
                    }
                    _haveData.Set(false);
                    _running = false;
                }
                else if (num == 1)
                {
                    break;
                }
            }
        }

        private async Task PinInfo_OnPinDataTransmit(PinDataTransmitEventArgs e)
        {
            lock (_lock)
            {
                _pinDataTransmitEventArgs.Enqueue(e);
            }
            await NotifyIfFlowIsRunningAsync();
        }

        private async Task NotifyIfFlowIsRunningAsync()
        {
            if (!_running && await _pinExecutionContext.ToolExecutionContext.VerifyToolStateAsync())
            {
                _haveData.Set(true);
                _pinDataTransmitEvent.Set();
            }
        }
        public void CleanPinDataTransmit()
        {
            lock (_lock) 
            {
                _pinDataTransmitEventArgs.Clear();
            }
        }
        public void Dispose()
        {
            _pinExecutionContext.DataTransmit.OnPinDataTransmit -= PinInfo_OnPinDataTransmit;
            Stop();
        }

        public override void Cancel()
        {
            _pinDataTransmitEvent.Set();
        }
    }
}
