using Microsoft.Extensions.Logging;
using QStandaedPlatform.Engine.Common.Common.Metadatas;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common
{
    public abstract class Tool : ITool
    {
        private readonly List<ITriggerPointCommand> _flowTriggerCommands;
        private readonly List<PinInfo> _inputPins = [];
        private readonly List<PinInfo> _outputPins = [];
        private volatile bool _isProcessing;

        protected Tool()
        {
            _flowTriggerCommands = [new TriggerPointCommand(1000, "取消执行")];
        }
        /// <summary>
        /// 工具定义名称
        /// </summary>
        public abstract string DefineName { get; }
        /// <summary>
        /// 工具别名
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;
        /// <summary>
        /// UI显示的的位置
        /// </summary>
        public QPoint ToolPosition { get; set; }

        public string StringId => $"{DefineName}_{UniqueId}";

        public List<ITriggerPointCommand> TriggerPointCommands => _flowTriggerCommands;

        public event PinDataTransmitEventHandler? OnPinDataTransmit;

        public event Action<ToolState<ToolState>>? ToolStateChange;

        public event PinAddedCallback? DynamicPinAddedCallback;

        public event PinRemovedCallback? DynamicPinRemovedCallback;

        public event PinAddedCallback? PinAddedCallback;

        public event PinRemovedCallback? PinRemovedCallback;


        /// <summary>
        /// 是否启用
        /// </summary>
        public bool Enable { get; set; } = true;
        /// <summary>
        ///执行次数
        /// </summary>
        public int ExecuteCount { get; set; }
        /// <summary>
        /// 是否启用调试信息
        /// </summary>
        public bool IsDebug { get; set; }

        /// <summary>
        /// 节点是否处理中
        /// </summary>

        public bool IsProcessing
        {
            get => _isProcessing;
            set => _isProcessing = value;
        }


        public virtual string? Version { get; set; }

        public string? Uuid { get; set; }

        public Guid UniqueId { get; set; }

        public virtual string ToolTypeName => GetType().FullName!;

        public CancellationToken RequestCancelToken { get; set; }


        public object DataContext { get; set; }

        public virtual string Description { get; }

        public ToolExecutionContext ToolExecutionContext { get; set; }

        public IReadOnlyList<PinInfo> InputPins => _inputPins;

        public IReadOnlyList<PinInfo> OutputPins => _outputPins;

        public ILogger? Logger { get; set; }

        public ToolExecuterBase? ToolExecuter { get; internal set; }
        public DateTime CreationTime { get; set; }

        public virtual Task<CommandResult> ExecuteCommandAsync(ITriggerPointCommand triggerPointCommand) => Task.FromResult(CommandResult.Empty(DefineName, "ExecuteCommandAsync函数未实现"));

        public virtual Task<bool> EnterRequestRecvAsync() => Task.FromResult(true);

        public virtual Task PreparePinDataBeforeRequest(ToolExecutionContext toolContext, PinDataTransmitEventArgs pinDataTransmitEventArgs)
        {
            if (InputPins.Any(p => p.Name == pinDataTransmitEventArgs.TargetPin.Name))
            {
                if (pinDataTransmitEventArgs.TargetPin.PinDataType == typeof(QDynamic))
                {
                    if (pinDataTransmitEventArgs.PinData is not QDynamic)
                    {
                        pinDataTransmitEventArgs.PinData = new QDynamic(pinDataTransmitEventArgs.PinData);
                    }
                }
            }
            if (pinDataTransmitEventArgs.SourcePin.PinDataType == typeof(QDynamic))
            {
                if (pinDataTransmitEventArgs.TargetPin.PinDataType != typeof(QDynamic))
                {
                    if (pinDataTransmitEventArgs.PinData is QDynamic qDynamic)
                    {
                        if (pinDataTransmitEventArgs.TargetPin.PinDataType == qDynamic.Value.GetType() || pinDataTransmitEventArgs.TargetPin.PinDataType.IsAssignableFrom(qDynamic.Value.GetType()))
                            pinDataTransmitEventArgs.PinData = qDynamic.Value;
                        else
                        {
                            throw new Exception($"数据类型不匹配，目标类型为{pinDataTransmitEventArgs.TargetPin.PinDataType.Name}，源类型为{qDynamic.Value.GetType().Name}");
                        }
                    }
                }
            }
            return Task.CompletedTask;
        }
        public abstract Task<bool> RequestRecvHandlePinAsync(ToolExecutionContext toolContext, PinDataTransmitEventArgs pinDataTransmitEventArgs);

        public virtual Task LeaveRequestRecvAsync() => Task.CompletedTask;

        public virtual Task RequestPauseAsync()
        {
            return Task.CompletedTask;
        }

        public virtual Task RequestResumeAsync()
        {
            return Task.CompletedTask;
        }

        public virtual Task RequestContinueJudgerAsync()
        {
            return Task.CompletedTask;
        }
        public virtual async Task RequestStartAsync() 
        {
            await Task.CompletedTask;
        }

        protected void InsetPin(string pinName, Tool ownerTool, Type pinDataType, PinType pinType, bool isDynamic = false, string desc = "")
        {
            lock (_inputPins)
            {
                var pin = new PinInfo(pinName, ownerTool, pinDataType, pinType, desc);
                if (pinType == PinType.Input)
                {
                    _inputPins.Add(pin);
                }
                else
                {
                    _outputPins.Add(pin);
                }
                if (isDynamic)
                    DynamicPinAddedCallback?.Invoke(ownerTool, pin);
            }
        }

        protected void DeleteInputPin(string pinName, bool isDynamic = false)
        {
            lock (_inputPins)
            {
                if (_inputPins.Any(p => p.Name == pinName))
                {
                    var pin = _inputPins.FirstOrDefault(p => p.Name == pinName);
                    if (pin != null && _inputPins.Remove(pin) && isDynamic)
                    {
                        DynamicPinRemovedCallback?.Invoke(pin.OwnerTool, pin);
                        pin = null;
                    }
                }
            }
        }

        protected void DeleteOutputPin(string pinName, bool isDynamic = false)
        {
            lock (_outputPins)
            {
                if (_outputPins.Any(p => p.Name == pinName))
                {
                    var pin = _outputPins.FirstOrDefault(p => p.Name == pinName);
                    if (pin != null && _outputPins.Remove(pin) && isDynamic)
                    {
                        DynamicPinRemovedCallback?.Invoke(pin.OwnerTool, pin);
                        pin = null;
                    }
                }
            }
        }

        protected void SendToPin(string pinName, QData qData)
        {
            lock (_outputPins)
            {
                if (_outputPins.Any(p => p.Name == pinName))
                {
                    var pin = _outputPins.FirstOrDefault(p => p.Name == pinName);
                    if (pin != null)
                        if (pin.LinkPins.Count != 0)
                        {
                            pin.LinkPins.ForEach(link =>
                            {
                                var pinDataTransmitEventArgs = new PinDataTransmitEventArgs()
                                {
                                    SourceOwnerTool = this,
                                    PinData = qData,
                                    TargetPin = link,
                                    SourcePin = pin,
                                };
                                link?.TransmitData(pinDataTransmitEventArgs);
                                OnPinDataTransmit?.Invoke(pinDataTransmitEventArgs);
                            });
                        }
                }
                else
                    throw new InvalidOperationException("Pin not found");
            }
        }

        public virtual bool Init() => true;

        public virtual bool InitPins() => true;

        public virtual bool InitStates() => true;

        public virtual bool InitDataContext() => true;

        public virtual bool InitEnd() => true;


        public virtual bool UnInit() => true;

        public virtual T Context<T>() where T : class
        {
            if (DataContext is T context)
                return context;
            throw new InvalidOperationException("DataContext is not of type " + typeof(T).Name);
        }

        public async Task RaiseToolStateChangeAsync(ToolState toolState, string desc = "")
        {
            this.ToolStateChange?.Invoke(new ToolState<ToolState>(this, toolState, desc));
            await OnToolStateChangeAsync(toolState, desc);
            Logger?.LogInformation($"Tool {DisplayName} state changed to {toolState},{desc}");
        }

        protected virtual Task OnToolStateChangeAsync(ToolState toolState, string desc) => Task.CompletedTask;


        public virtual Task<bool> HandleExecutedErrorAsync(Exception toolexception) => Task.FromResult(false);

        /// <summary>
        /// 清除临时数据和状态,变量
        /// </summary>
        /// <returns></returns>
        public virtual Task<bool> ClearEphemeralDataAsync() => Task.FromResult(true);


        public async Task<bool> RequestCancelAsync()
        {
            if (await OnHandleRequestCancelAsync())
            {
                await RaiseToolStateChangeAsync(ToolState.None);
                return true;
            }
            else
            {
                return false;
            }
        }

        public virtual Task<bool> OnHandleRequestCancelAsync() => Task.FromResult(true);


        public async Task<bool> RequestCancelResetAsync()
        {
            var result = await OnHandleRequestCancelResetAsync();
            if (result)
            {
                await RaiseToolStateChangeAsync(ToolState.None);
            }
            return result;
        }

        public virtual Task<bool> OnHandleRequestCancelResetAsync() => Task.FromResult(true);

        public bool HandleContextChanged(object context, out string message)
        {
            if (OnHandleContextChanged(context, out message))
            {
                ApplyOnContextChanged(context);
                return true;
            }
            return false;
        }

        public virtual bool OnHandleContextChanged(object context, out string message)
        {
            message = "";
            return true;
        }
        public virtual void ApplyOnContextChanged(object context) { }

        public virtual void OnRefPartPropertyInstalled(IRefPartProperty part) { }

        public virtual void OnRefParameterPropertyInstalled(IRefParameterProperty parameter) { }

        public virtual void OnRefPartPropertyUnInstalled(IRefPartProperty part) { }

        public virtual void OnRefParameterPropertyUnInstalled(IRefParameterProperty parameter) { }
    }
}
