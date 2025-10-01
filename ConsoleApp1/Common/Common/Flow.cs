using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QStandaedPlatform.Engine.Common.Common.SampleEntitys;
using QStandaedPlatform.Engine.Components;
using QStandaedPlatform.Engine.Components.Modbus;
using QStandaedPlatform.Engine.Laboratory;
using QStandaedPlatform.Engine.Laboratory.Documents;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common
{
    public class Flow
    {

        private readonly ConcurrentDictionary<Guid, Tool> _tools = new();

        private readonly ConcurrentDictionary<PinType, Dictionary<Guid, PinInfo>> _pinTypePins = new();

        private readonly ConcurrentDictionary<Guid, ToolExecutionContext> _toolExecutionContexts = new();

        private readonly List<ToolConnecter> _toolConnecters = [];

        private readonly List<RefPartProperty> _refPartProperties = [];

        private readonly List<RefParameterProperty> _refParameterProperties = [];

        private readonly List<Func<Exception,Task>> _onErrorHandlers = [];

        private readonly List<ITangerinePrologueAngryService> _tangerinePrologueAngryServices = [];

        private readonly List<ITangerineConcludeAngryService> _tangerineConcludeAngryServices = [];

        private readonly List<ISampleInjectService> _sampleInjectServices = [];

        private readonly FlowStateMachine flowStateMachine = new();

        private readonly Dictionary<string, SampleTaskInfo> _samplingTasks = [];

        private readonly List<ILabTrayService>  _labTrayServices = [];
        private readonly Dictionary<long, object> _labTraySetups = [];
        private readonly Dictionary<Guid,IModuleWithParameterTool> _moduleWithParameterTools = [];

        private readonly CancellationTokenSource _cts = new();

        private CancellationTokenSource _enableRequestCts = new();
        private readonly ILogger? _logger;
        public Flow()
        {
            _pinTypePins[PinType.Input] = [];
            _pinTypePins[PinType.Output] = [];
            FlowState.UpdateRunState( Common.FlowState.Idle);
            _logger = LoggerProviderManager.GetLoggerFactory().CreateLogger("Flow");
        }
        public Flow(string flowName, Guid flowId, string description = "") : this()
        {
            FlowName = flowName;
            FlowId = flowId;
            Description = description;
        }

        public CancellationToken FlowShutToken => _cts.Token;
        /// <summary>
        /// 流程使能Token
        /// </summary>
        public CancellationToken RequestCancelToken => _enableRequestCts.Token;
        /// <summary>
        /// 流程名称
        /// </summary>
        public string FlowName { get; set; }

        /// <summary>
        /// 流程id
        /// </summary>
        public Guid FlowId { get; set; }
        /// <summary>
        /// 流程描述
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 流程状态
        /// </summary>
        public FlowStateMachine FlowState => flowStateMachine;


        public Func<Tool,Task> OnToolTaskCompletedCallbask { get; set; }


        public bool ToolErrorCanCancel { get; set; } = false;


        public Tool? GetTool(Guid toolId)
        {
            if (_tools.TryGetValue(toolId, out Tool? value))
                return value;
            return null;
        }
        public IEnumerable<Tool> GetTools()
        {
            return _tools.Values;
        }
        public IReadOnlyList<ToolConnecter> GetToolConnecters()
        {
            return _toolConnecters.AsReadOnly();
        }

        public IReadOnlyList<RefPartProperty> GetRefPartProperties()
        {
            return _refPartProperties.AsReadOnly();
        }

        public IReadOnlyList<RefParameterProperty> GetRefParameterProperties()
        {
            return _refParameterProperties.AsReadOnly();
        }
        public void AddToolConnecter(ToolConnecter toolConnecter)
        {
            if (!_toolConnecters.Any(t => t.SourceToolId == toolConnecter.SourceToolId && t.TargetToolId == toolConnecter.TargetToolId))
            {
                _toolConnecters.Add(toolConnecter);
            }
        }

        public void RemoveToolConnecter(ToolConnecter toolConnecter)
        {
            var item = _toolConnecters.FirstOrDefault(t => t.SourceToolId == toolConnecter.SourceToolId && t.TargetToolId == toolConnecter.TargetToolId);
            if (item != null)
            {
                _toolConnecters.Remove(item);
            }
        }

        public ToolConnecter? GetToolConnecter(Guid sourceToolId, Guid targetToolId)
        {
            return _toolConnecters.FirstOrDefault(t => t.SourceToolId == sourceToolId && t.TargetToolId == targetToolId);
        }
        public void AddTool(Tool tool)
        {
            if (!_tools.ContainsKey(tool.UniqueId))
            {
                _tools[tool.UniqueId] = tool;

                foreach (var pin in tool.InputPins)
                {
                    if (_pinTypePins.TryGetValue(pin.PinType, out var pinInfos))
                    {
                        pinInfos[pin.Id] = pin;
                    }
                }
                foreach (var pin in tool.OutputPins)
                {
                    if (_pinTypePins.TryGetValue(pin.PinType, out var pinInfos))
                    {
                        pinInfos[pin.Id] = pin;
                    }
                }
                if (tool is ITangerinePrologueAngryService tangerinePrologueAngryService)
                {
                    _tangerinePrologueAngryServices.Add(tangerinePrologueAngryService);
                }
                if (tool is ITangerineConcludeAngryService tangerineConcludeAngryService)
                {
                    _tangerineConcludeAngryServices.Add(tangerineConcludeAngryService);
                }

                if (tool is ISampleInjectService sampleInjectService)
                {
                    _sampleInjectServices.Add(sampleInjectService);
                }

                if (tool is ILabTrayService labTrayService)
                {
                    _labTrayServices.Add(labTrayService);
                    _logger?.LogInformation($"LabTrayService:{tool.DisplayName} added");
                }
                if (tool is IModuleWithParameterTool moduleWith)
                {
                    _moduleWithParameterTools[tool.UniqueId] = moduleWith;
                }
            }
        }
        public void RemoveTool(Guid toolId)
        {
            if (_tools.ContainsKey(toolId))
            {
                if (_tools.TryRemove(toolId, out var tool))
                {
                    tool.UnInit();
                    _toolExecutionContexts[tool.UniqueId].StopToolExecution();
                    var refs = _refPartProperties.Where(p => p.OwnerToolId == toolId && p.OwnerFlowId == this.FlowId).ToArray();
                    for (int j = 0; j < refs.Length; j++)
                    {
                        for (global::System.Int32 i = _refPartProperties.Count - (1); i >= 0; i--)
                        {
                            if (refs[j].OwnerToolId == _refPartProperties[i].OwnerToolId)
                            {
                                _refPartProperties.RemoveAt(i);
                            }
                        }
                    }
                    foreach (var pin in tool.InputPins)
                    {
                        _pinTypePins[pin.PinType].Remove(pin.Id);
                    }
                    foreach (var pin in tool.OutputPins)
                    {
                        _pinTypePins[pin.PinType].Remove(pin.Id);
                    }
                }
            }
        }
        public void RemoveTool(Tool tool)
        {
            if (_tools.ContainsKey(tool.UniqueId))
            {
                _tools.TryRemove(tool.UniqueId, out _);
                tool.UnInit();
                _toolExecutionContexts[tool.UniqueId].StopToolExecution();
                var refs = _refPartProperties.Where(p => p.OwnerToolId == tool.UniqueId && p.OwnerFlowId == this.FlowId).ToArray();
                for (int j = 0; j < refs.Length; j++)
                {
                    for (global::System.Int32 i = _refPartProperties.Count - (1); i >= 0; i--)
                    {
                        if (refs[j].OwnerToolId == _refPartProperties[i].OwnerToolId)
                        {
                            _refPartProperties.RemoveAt(i);
                        }
                    }
                }
                foreach (var pin in tool.InputPins)
                {
                    _pinTypePins[pin.PinType].Remove(pin.Id);
                }
                foreach (var pin in tool.OutputPins)
                {
                    _pinTypePins[pin.PinType].Remove(pin.Id);
                }
            }
        }
        public void RemoveAllTools()
        {
            _enableRequestCts.Cancel();
            _cts.Cancel();
            foreach (var item in _tools.Values)
            {
                item.UnInit();
                _toolExecutionContexts[item.UniqueId].StopToolExecution();
            }
            _tools.Clear();
            _pinTypePins.Clear();
        }

        public async Task StartAtToolPrologueAsync(Tool tool)
        {
            _logger?.LogInformation($"StartAtToolPrologueAsync {tool.DisplayName}");
            if (tool is ITangerinePrologueAngryService tangerinePrologueAngryService)
            {
                await tangerinePrologueAngryService.StartPrologueAsync();
            }
        }


        public async Task StartPrologueAsync()
        {
            if (_tangerinePrologueAngryServices.Count != 0)
            {
                _logger?.LogInformation("StartPrologueAsync {FlowName}", FlowName);
                await Task.WhenAll(_tangerinePrologueAngryServices.Select(x => x.StartPrologueAsync()));
            }
        }

        public async Task<ConcludeResult[]> WaitForConcludeAsync()
        {
            if (_tangerineConcludeAngryServices.Count != 0)
            {
                return await Task.WhenAll(_tangerineConcludeAngryServices.Select(x => x.WaitForConcludeAsync()));
            }
            return [];
        }

        public bool TryCreateTool(string name, Guid toolId, out Tool? tool)
        {
            tool = null;
            var toolDescription = ToolDescriptions
             .GetToolDescriptions().FirstOrDefault(p => p.toolName == name);
            if (toolDescription != (null, null))
            {
                var toolType = ToolDescriptions.GetToolType(toolDescription.toolName);
                tool = ToolDescriptions
                   .StructureTool(toolType);
                if (tool != null)
                {
                    tool.CreationTime = DateTime.Now;
                    tool.UniqueId = toolId;
                    tool.DisplayName = _tools.Count + 1 + "_" + tool.DefineName;
                    tool.Logger = LoggerProviderManager.GetLoggerFactory().CreateLogger($"<{this.FlowName}>.{tool.DisplayName}");
                    ToolExecutionContext toolExecutionContext = new(this, tool);
                    _toolExecutionContexts[tool.UniqueId] = toolExecutionContext;
                    tool.ToolExecutionContext = toolExecutionContext;
                    foreach (var propertyInfo in toolType.GetProperties())
                    {
                        if (propertyInfo.CanWrite)
                        {
                            var refatt = propertyInfo.GetCustomAttribute<ReferencePartAttribute>();
                            if (refatt != null)
                            {
                                if (!_refPartProperties.Any(p => p.PropertyName == propertyInfo.Name && p.OwnerToolId == toolId&& p.OwnerFlowId == this.FlowId))
                                {
                                    _refPartProperties.Add(new RefPartProperty
                                    {
                                        OwnerToolId = tool.UniqueId,
                                        PropertyName = propertyInfo.Name,
                                        PropertyType = propertyInfo.PropertyType,
                                        Property = propertyInfo,
                                        OwnerTool = tool,
                                        OwnerFlowId = this.FlowId,
                                    });
                                }
                                else
                                {
                                    var refPartProperty = _refPartProperties.FirstOrDefault(p => p.PropertyName == propertyInfo.Name && p.OwnerToolId == toolId && p.OwnerFlowId == this.FlowId);
                                    if (refPartProperty != null)
                                    {
                                        refPartProperty.OwnerTool = tool;
                                        refPartProperty.Property = propertyInfo;
                                    }
                                }
                            }

                            var refParamatt = propertyInfo.GetCustomAttribute(typeof(RefParameterAttribute<>));
                            if (refParamatt != null) 
                            {
                                var refParamType = refParamatt.GetType().GenericTypeArguments[0];
                                if (!_refParameterProperties.Any(p => p.PropertyName == propertyInfo.Name
                                && p.OwnerToolId == toolId
                                && p.OwnerFlowId == this.FlowId
                                && p.ModuleTableType == refParamType))
                                {
                                    _refParameterProperties.Add(new RefParameterProperty
                                    {
                                        ModuleTableType = refParamType,
                                        OwnerFlowId = this.FlowId,
                                        OwnerToolId = tool.UniqueId,
                                        OwnerTool = tool,
                                        Property = propertyInfo,
                                        PropertyName = propertyInfo.Name,
                                        PropertyType = propertyInfo.PropertyType,
                                    });
                                }
                            }
                        }
                    }
                    tool.RequestCancelToken = _enableRequestCts.Token;
                    tool.Init();
                    tool.InitPins();
                    if (tool is SyncInputToolBase syncInputToolBase)
                    {
                        syncInputToolBase.InitCache();
                    }
                    AddTool(tool);
                    _toolExecutionContexts[tool.UniqueId].StartToolExecution();
                    tool.InitStates();
                    tool.InitDataContext();
                    tool.InitEnd();
                    return true;
                }
            }
            return false;
        }
        public Tool? StructureTool(ToolInfoOptions toolInfo)
        {
            var toolDescription = ToolDescriptions
                .GetToolDescriptions().FirstOrDefault(p => p.toolName == toolInfo.DefineName);
            if (toolDescription != (null, null))
            {
                var toolType = ToolDescriptions.GetToolType(toolDescription.toolName);
                _logger?.LogInformation($"StructureTool {toolInfo.DefineName}");
                var tool = ToolDescriptions
                    .StructureTool(toolType);
                _logger?.LogInformation($"StructureTool{tool == null} {tool?.DisplayName} end");
                if (tool != null)
                {
                    tool.UniqueId = toolInfo.UniqueId;
                    tool.DisplayName = toolInfo.DisplayName;
                    tool.Logger = LoggerProviderManager.GetLoggerFactory().CreateLogger($"<{this.FlowName}>.{tool.DisplayName}");
                    tool.IsDebug = toolInfo.IsDebug;
                    tool.Enable = toolInfo.Enable;
                    tool.ToolPosition = toolInfo.ToolPosition;
                    tool.CreationTime = toolInfo.CreationTime;
                    tool.RequestCancelToken = _enableRequestCts.Token;
                    ToolExecutionContext toolExecutionContext = new(this, tool);
                    _toolExecutionContexts[tool.UniqueId] = toolExecutionContext;
                    tool.ToolExecutionContext = toolExecutionContext;
                    foreach (var propertyInfo in toolType.GetProperties())
                    {
                        if (propertyInfo.CanWrite)
                        {
                            var refatt = propertyInfo.GetCustomAttribute<ReferencePartAttribute>();
                            if (refatt != null)
                            {
                                if (!_refPartProperties.Any(p => p.PropertyName == propertyInfo.Name && p.OwnerToolId == tool.UniqueId))
                                {
                                    _refPartProperties.Add(new RefPartProperty
                                    {
                                        OwnerToolId = tool.UniqueId,
                                        PropertyName = propertyInfo.Name,
                                        PropertyType = propertyInfo.PropertyType,
                                        Property = propertyInfo,
                                        OwnerTool = tool,
                                        OwnerFlowId = this.FlowId,
                                    });
                                }
                                else
                                {
                                    var refPartProperty = _refPartProperties.FirstOrDefault(p => p.PropertyName == propertyInfo.Name 
                                    && p.OwnerToolId == tool.UniqueId
                                    );
                                    if (refPartProperty != null)
                                    {
                                        _logger?.LogInformation($"Tool:{tool.DisplayName} Property:{propertyInfo.Name} is already exist");
                                        refPartProperty.OwnerTool = tool;
                                        refPartProperty.Property = propertyInfo;
                                        refPartProperty.OwnerFlowId = this.FlowId;
                                        refPartProperty.InstallRef();
                                        _logger?.LogInformation($"Tool:{tool.DisplayName} Property:{propertyInfo.Name} is installed");
                                    }
                                }


                            }

                            var refparamatt = propertyInfo.GetCustomAttribute(typeof(RefParameterAttribute<>));
                            if (refparamatt != null)
                            {
                                var refparamtype = refparamatt.GetType().GenericTypeArguments[0];
                                if (!_refParameterProperties.Any(p => p.PropertyName == propertyInfo.Name 
                                && p.OwnerToolId == tool.UniqueId
                                && p.OwnerFlowId == this.FlowId
                                && p.ModuleTableType == refparamtype))
                                {
                                    _refParameterProperties.Add(new RefParameterProperty
                                    {
                                        OwnerToolId = tool.UniqueId,
                                        PropertyName = propertyInfo.Name,
                                        PropertyType = propertyInfo.PropertyType,
                                        Property = propertyInfo,
                                        OwnerTool = tool,
                                        OwnerFlowId = this.FlowId,
                                        ModuleTableType = refparamtype,

                                    });
                                }
                                else
                                {
                                    var refParameterProperty = _refParameterProperties.LastOrDefault(p => p.PropertyName == propertyInfo.Name
                                    && p.OwnerToolId == tool.UniqueId
                                    && p.OwnerFlowId == this.FlowId
                                    && p.RefParameterId != Guid.Empty
                                    && p.RefParameterTableId != Guid.Empty);
                                    if (refParameterProperty != null)
                                    {
                                        refParameterProperty.OwnerTool = tool;
                                        refParameterProperty.Property = propertyInfo;
                                        refParameterProperty.OwnerFlowId = this.FlowId;
                                        refParameterProperty.InstallRef();
                                    }
                                }
                            }
                        }
                    }
                    foreach (var item in tool.TriggerPointCommands)
                    {
                        foreach (var commandOptions in toolInfo.ToolTriggerCommandOptions)
                        {
                            if (item.Name == commandOptions.Name && item.Id == commandOptions.Id)
                            {
                                item.TriggerValue = commandOptions.TriggerValue;
                            }
                        }
                    }
                    tool.Init();
                    tool.InitPins();
                    if (tool is SyncInputToolBase syncInputToolBase)
                    {
                        syncInputToolBase.InitCache();
                    }
                    tool.InitStates();
                    tool.InitDataContext();
                    if (tool is DynamicPinTool dynamicPinTool)
                    {
                        if (dynamicPinTool.DataContext != null)
                        {
                            var dataJson = toolInfo.DataContext;
                            if (!string.IsNullOrEmpty(dataJson) && dataJson != "null")
                            {
                                var dataModel = JsonConvert.DeserializeObject(dataJson, dynamicPinTool.DataContext.GetType());
                                if (dataModel != null)
                                {
                                    dynamicPinTool.DataContext = (dataModel as DynamicPinToolData);
                                    dynamicPinTool.ApplyOnContextChanged(dynamicPinTool.DataContext);
                                }
                            }
                        }
                    }
                    else if (tool is DynamicSyncInputPinTool dynamicSyncInputPinTool)
                    {
                        if (dynamicSyncInputPinTool.DataContext != null)
                        {
                            var dataJson = toolInfo.DataContext;
                            if (!string.IsNullOrEmpty(dataJson) && dataJson != "null")
                            {
                                var dataModel = JsonConvert.DeserializeObject(dataJson, dynamicSyncInputPinTool.DataContext.GetType());
                                if (dataModel != null)
                                {
                                    dynamicSyncInputPinTool.DataContext = dataModel as DynamicPinToolData;
                                    dynamicSyncInputPinTool.ApplyOnContextChanged(dynamicSyncInputPinTool.DataContext);
                                }
                            }
                        }
                    }
                    else if (tool is ModuleToolBase moduleToolBase)
                    {
                        if (moduleToolBase.DataContext != null)
                        {
                            var dataJson = toolInfo.DataContext;
                            if (!string.IsNullOrEmpty(dataJson) && dataJson != "null")
                            {
                                var dataModel = JsonConvert.DeserializeObject(dataJson, moduleToolBase.DataContext.GetType());
                                if (dataModel != null)
                                {
                                    moduleToolBase.DataContext = dataModel as ModuleData;
                                    moduleToolBase.ApplyOnContextChanged(moduleToolBase.DataContext);
                                }
                            }
                        }
                    }
                    else if (tool is SyncInputModuleToolBase syncInputModuleToolBase)
                    {
                        if (syncInputModuleToolBase.DataContext != null)
                        {
                            var dataJson = toolInfo.DataContext;
                            if (!string.IsNullOrEmpty(dataJson) && dataJson != "null")
                            {
                                var dataModel = JsonConvert.DeserializeObject(dataJson, syncInputModuleToolBase.DataContext.GetType());
                                if (dataModel != null)
                                {
                                    syncInputModuleToolBase.DataContext = dataModel as ModuleData;
                                    syncInputModuleToolBase.ApplyOnContextChanged(syncInputModuleToolBase.DataContext);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (toolInfo.DataContext != null)
                        {
                            var dataJson = toolInfo.DataContext;
                            if (!string.IsNullOrEmpty(dataJson) && dataJson != "null")
                            {
                                try
                                {
                                    var dataModel = JsonConvert.DeserializeObject(dataJson, tool.DataContext.GetType());
                                    if (dataModel != null)
                                    {
                                        tool.DataContext = dataModel;
                                        tool.ApplyOnContextChanged(tool.DataContext);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    throw;
                                }
                            }
                        }
                    }
                    foreach (var item in tool.InputPins)
                    {
                        foreach (var pinOptions in toolInfo.InputToolPinInfos)
                        {
                            if (item.Name == pinOptions.PinName)
                            {
                                item.Id = pinOptions.Id;
                            }
                        }
                    }
                    foreach (var item in tool.OutputPins)
                    {
                        foreach (var pinOptions in toolInfo.OutputToolPinInfos)
                        {
                            if (item.Name == pinOptions.PinName)
                            {
                                item.Id = pinOptions.Id;
                            }
                        }
                    }
                    AddTool(tool);
                    if (tool is not DynamicPinTool && tool is not DynamicSyncInputPinTool)
                        _toolExecutionContexts[tool.UniqueId].StartToolExecution();
                    return tool;
                }
            }
            return default;
        }


        public bool RemoveRefPartProperty(RefPartProperty refPartProperty)
        {
           return _refPartProperties.Remove(refPartProperty);
        }

        public bool RemoveRefParameterProperty(RefParameterProperty refParameterProperty)
        {
            return _refParameterProperties.Remove(refParameterProperty);
        }


        public void RegisterErrorHandler(Func<Exception,Task> errorHandler)
        {
           _onErrorHandlers.Add(errorHandler);
        }

        public void UnRegisterErrorHandler(Func<Exception,Task> errorHandler)
        {
            _onErrorHandlers.Remove(errorHandler);
        }


        public async Task HandleRunErrorAsync(Exception exception)
        {
            foreach (var errorHandler in _onErrorHandlers)
            {
                await errorHandler(exception);
            }
        }
        public void UnCrateTool(Guid toolId)
        {
            if (_tools.TryGetValue(toolId, out Tool? tool))
            {
                tool.UnInit();
                RemoveTool(tool);
            }
        }
        public FlowInfoOptions Serialize()
        {
            FlowInfoOptions flowInfoOptions = new()
            {
                FlowName = FlowName,
                FlowId = FlowId,
                Description = Description,
                ToolConnecters = [.. _toolConnecters],
                PartProperties = [.. _refPartProperties],
                ParameterProperties = [.. _refParameterProperties],
            };
            foreach (var tool in _tools.Values)
            {
                var toolInfoOptions = new ToolInfoOptions
                {
                    UniqueId = tool.UniqueId,
                    DisplayName = tool.DisplayName,
                    DefineName = tool.DefineName,
                    IsDebug = tool.IsDebug,
                    Enable = tool.Enable,
                    CreationTime = tool.CreationTime,
                    ToolPosition = tool.ToolPosition,
                };
                if (tool is DynamicPinTool dynamicPinTool)
                {
                    toolInfoOptions.DataContext = JsonConvert.SerializeObject(dynamicPinTool.DataContext, Formatting.Indented);
                }
                else if (tool is DynamicSyncInputPinTool dynamicSyncInputPinTool)
                {
                    toolInfoOptions.DataContext = JsonConvert.SerializeObject(dynamicSyncInputPinTool.DataContext, Formatting.Indented);
                }
                else if (tool is SyncInputModuleToolBase syncInputModuleToolBase)
                {
                    toolInfoOptions.DataContext = JsonConvert.SerializeObject(syncInputModuleToolBase.DataContext, Formatting.Indented);
                }
                else if (tool is ModuleToolBase moduleToolBase)
                {
                    toolInfoOptions.DataContext = JsonConvert.SerializeObject(moduleToolBase.DataContext, Formatting.Indented);
                }
                else
                {
                    toolInfoOptions.DataContext = JsonConvert.SerializeObject(tool.DataContext, Formatting.Indented);
                }
                tool.TriggerPointCommands.ForEach(p =>
                {
                    toolInfoOptions.ToolTriggerCommandOptions.Add(new ToolTriggerCommandOptions
                    {
                        Description = p.Description,
                        Id = p.Id,
                        Name = p.Name,
                        OwnerToolId = p.OwnerToolId,
                        TriggerValue = p.TriggerValue,
                    });
                });
                var inputPins = tool.InputPins.Select(p => new PinInfoOptions
                {
                    ConnectedPins = p.LinkPins.Select(x => x.Id).ToList(),
                    Id = p.Id,
                    Description = p.Description,
                    MonitorChildPinStates = p.MonitorChildPinStates,
                    OwnerToolId = p.OwnerTool.UniqueId,
                    ParentId = p.ParentId,
                    PinDataType = p.PinDataType,
                    PinName = p.Name,
                    PinType = p.PinType

                });
                toolInfoOptions.InputToolPinInfos.AddRange(inputPins);
                var outputPins = tool.OutputPins.Select(p => new PinInfoOptions
                {
                    ConnectedPins = p.LinkPins.Select(x => x.Id).ToList(),
                    Id = p.Id,
                    Description = p.Description,
                    MonitorChildPinStates = p.MonitorChildPinStates,
                    OwnerToolId = p.OwnerTool.UniqueId,
                    ParentId = p.ParentId,
                    PinDataType = p.PinDataType,
                    PinName = p.Name,
                    PinType = p.PinType
                });
                toolInfoOptions.OutputToolPinInfos.AddRange(outputPins);
                flowInfoOptions.Tools.Add(toolInfoOptions);
            }
            return flowInfoOptions;
        }
        public bool Deserialize(FlowInfoOptions options)
        {

            if (options == null)
                return false;
            FlowName = options.FlowName;
            FlowId = options.FlowId;
            Description = options.Description;
            foreach (var item in options.ToolConnecters)
            {
                AddToolConnecter(item);
            }
            foreach (var item in options.PartProperties)
            {
                _refPartProperties.Add(item);
            }
            foreach (var item in options.ParameterProperties)
            {
                _refParameterProperties.Add(item);
            }
            var tools = new Dictionary<Guid, Tool>();
            foreach (var toolInfo in options.Tools)
            {
                var tool = StructureTool(toolInfo);
                if (tool != null)
                {
                    tools.Add(tool.UniqueId, tool);
                }
            }
            foreach (var toolInfo in options.Tools)
            {
                if (!tools.ContainsKey(toolInfo.UniqueId))
                    continue;
                var tool = tools[toolInfo.UniqueId];
                var inputPins = toolInfo.InputToolPinInfos;
                var outputPins = toolInfo.OutputToolPinInfos;
                foreach (var inputPinInfoOptions in inputPins)
                {
                    foreach (var nextToolInfoId in inputPinInfoOptions.ConnectedPins)
                    {
                        var inputPin = tool.InputPins.FirstOrDefault(p => p.Id == inputPinInfoOptions.Id);
                        if (inputPin != null)
                        {
                            if (_pinTypePins[PinType.Output].TryGetValue(nextToolInfoId, out var outputPin))
                            {
                                if (outputPin != null)
                                {
                                    inputPin.LinkTo(outputPin);
                                }
                            }
                        }
                    }
                }
                foreach (var outputPinInfoOptions in outputPins)
                {
                    foreach (var nextToolInfoId in outputPinInfoOptions.ConnectedPins)
                    {
                        var outputPin = tool.OutputPins.FirstOrDefault(p => p.Id == outputPinInfoOptions.Id);
                        if (outputPin != null)
                        {
                            if (_pinTypePins[PinType.Input].TryGetValue(nextToolInfoId, out var inputPin))
                            {
                                if (inputPin != null)
                                {
                                    outputPin.LinkTo(inputPin);
                                }
                            }
                        }
                    }
                }
                tool.InitEnd();
            }
            return true;
        }
        public void LinkCancelTokenSource(CancellationTokenSource takenSource)
        {
            _enableRequestCts = CancellationTokenSource.CreateLinkedTokenSource(_enableRequestCts.Token, takenSource.Token);
        }


        /// <summary>
        /// 流程请求停止
        /// </summary>
        /// <returns></returns>
        public async Task StopRequestAsync()
        {
            await RequestCancelAsync();
            await ClearEphemeralDataAsync();
        }

        /// <summary>
        /// 流程请求重置
        /// </summary>
        /// <returns></returns>
        public async Task ResetIfFaultedAsync()
        {
            if (this.FlowState.IsCancel || this.FlowState.IsError)
            {
                await RequestResetAsync();
            }
        }

        /// <summary>
        /// 流程请求取消
        /// </summary>
        public async Task RequestCancelAsync()
        {
            if (!_enableRequestCts.IsCancellationRequested)
            {
                var tasks = new List<Tool>();
                var hashset = new Dictionary<IH5uTcp, Tool>();

                foreach (var item in _tools.Values)
                {
                    if (item is IModuleTool moduleTool)
                    {
                        hashset.TryAdd(moduleTool.GetModular().Messenger, item);
                    }
                    else
                        tasks.Add(item);
                }

                foreach (var tool in hashset.Values)
                {
                    if (!await tool.RequestCancelAsync())
                    {
                        throw new InvalidOperationException($"工具请求取消失败：{tool.DisplayName},请检查。");
                    }
                }

                foreach (var tool in tasks)
                {
                    if (!await tool.RequestCancelAsync())
                    {
                        throw new InvalidOperationException($"工具请求取消失败：{tool.DisplayName},请检查。");
                    }
                }
                _enableRequestCts.Cancel();
                this.FlowState.UpdateRunState(Common.FlowState.Cancel);
            }
            else
            throw new InvalidOperationException($"流程当前状态{FlowState.State},取消失败。");
        }

        //流程请求暂停
        public async Task RequestPauseAsync()
        {
            if (FlowState.IsRunning || FlowState.IsIdle)
            {
                foreach (var tool in _tools.Values)
                {
                   await tool.RequestPauseAsync();
                }
                FlowState.UpdateRunState(Common.FlowState.Pause);
            }
            else
           throw new InvalidOperationException ($"当前状态{FlowState.State},流程暂停失败！");
        }

        //流程请求继续
        public async Task RequestContinueAsync()
        {
            if (FlowState.IsPause)
            {
                foreach (var tool in _tools.Values)
                {
                    await tool.RequestResumeAsync();
                }
                FlowState.UpdateRunState(Common.FlowState.Idle);
            }
            else
                throw new InvalidOperationException($"当前状态{FlowState.State},流程继续失败！");
        }

        /// <summary>
        /// 流程请求启动 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task RequestStartAsync()
        {
            if (FlowState.IsIdle)
            {
                foreach (var tool in _tools.Values)
                {
                    await tool.RequestStartAsync();
                }
                FlowState.UpdateRunState(Common.FlowState.Running);
            }
            else
                throw new InvalidOperationException($"当前状态{FlowState.State},流程启动失败！");
        }


        /// <summary>
        /// 流程请求复位
        /// </summary>
        public async Task RequestResetAsync()
        {
            if (!FlowState.IsCancel && !FlowState.IsError)
            {
                throw new InvalidOperationException($"流程当前状态：{FlowState.State},无法进行复位。");
            }
            var tasks = new List<Tool>();
            var hashset = new Dictionary<IH5uTcp,Tool>();
            foreach (var item in _tools.Values)
            {
                if (item is IModuleTool moduleTool)
                {
                    hashset.TryAdd(moduleTool.GetModular().Messenger, item);
                }
                else
                    tasks.Add(item);
            }
            foreach (var item in hashset.Values)
            {
                if (!await item.RequestCancelResetAsync())
                {
                    throw new InvalidOperationException($"工具请求复位失败：{item.DisplayName},请检查。");
                }
            }
            foreach (var item in tasks)
            {
                if (!await item.RequestCancelResetAsync())
                {
                    throw new InvalidOperationException($"工具请求复位失败：{item.DisplayName},请检查。");
                }
            }
            _enableRequestCts = new CancellationTokenSource();
            foreach (var item in _tools.Values)
            {
                item.RequestCancelToken = _enableRequestCts.Token;
            }
            FlowState.UpdateRunState(Common.FlowState.Idle);
        }

        /// <summary>
        /// 清除临时数据
        /// </summary>
        public async Task<bool> ClearEphemeralDataAsync()
        {
            foreach (var tool in _tools.Values)
            {
                if (!await tool.ClearEphemeralDataAsync())
                {
                    throw new Exception($"工具清除临时数据失败：{tool.DisplayName},请检查。");
                }
            }
            return true;
        }

        public override string ToString()
        {
            return this.FlowName;
        }

        public void SetLabTrays(List<LabTrayInfo> labTrayInfos)
        {
            foreach (var item in _labTrayServices.SelectMany(p=>p.LabTrays).DistinctBy(p=>p.LabTrayId).ToArray())
            {
                item.InitWellStatus(labTrayInfos.FirstOrDefault(p => p.LabTrayName == item.LabTrayName)?.WellInfos);
            }
        }

        public List<LabTrayInfo> GetLabTrayInfos()
        {
            var list = new List<LabTrayInfo>();
            foreach (var item in _labTrayServices)
            {
                list.AddRange([.. item.LabTrays.Where(p => p != null && !p.VirtualTray).Select(p => p.GetLabTrayInfo())]);
            }
            return [.. list.DistinctBy(p => p.LabTrayId)];
        }

        public List<LabTray> GetLabTrays()
        {
            var list = new List<LabTray>();
            foreach (var item in _labTrayServices)
            {
                if (item != null)
                {
                    foreach (var tray in item.LabTrays)
                    {
                        if (tray != null && !tray.VirtualTray)
                        {
                            list.Add(tray);
                        }
                    }
                }
            }
            return [.. list.DistinctBy(p => p.LabTrayId)];
        }

        public List<LabTrayConfiguration> GetLabTrayConfigs()
        {
            var list = new List<LabTrayConfiguration>();
            foreach (var item in _labTrayServices)
            {
                list.AddRange([.. item.LabTrays.Where(p => p != null && !p.VirtualTray).Select(p => p.GetLabTrayConfig())]);
            }
            return [.. list.DistinctBy(p => p.LabTrayId)];
        }

        public Dictionary<Tool, int> GetToolTree()
        {
            var result = new Dictionary<Tool, int>();
            var visited = new HashSet<Tool>();
            var startTool = _tools.Values.FirstOrDefault(p => p is ISampleInjectService);

            if (startTool != null)
            {
                BreadthFirst(startTool, result, visited);
            }
            else
            {
                startTool = _tools.Values.FirstOrDefault(p => p is IStartSignService);
                if (startTool != null)
                {
                    BreadthFirst(startTool, result, visited);
                }
            }

            return result;
        }
        /// <summary>
        /// 广度优先搜索
        /// </summary>
        /// <param name="startTool"></param>
        /// <param name="result"></param>
        /// <param name="visited"></param>
        private static void BreadthFirst(Tool startTool, Dictionary<Tool, int> result, HashSet<Tool> visited)
        {
            if (startTool == null)
                return;

            Queue<(Tool tool, int number)> queue = new();
            queue.Enqueue((startTool, 1));
            visited.Add(startTool);

            while (queue.Count > 0)
            {
                var (currentTool, currentNumber) = queue.Dequeue();

                if (!result.ContainsKey(currentTool))
                {
                    result[currentTool] = currentNumber;
                }

                if (currentTool.OutputPins != null)
                {
                    foreach (var item in currentTool.OutputPins)
                    {
                        foreach (var outputPin in item.LinkPins)
                        {
                            if (outputPin?.OwnerTool != null && !visited.Contains(outputPin.OwnerTool))
                            {
                                queue.Enqueue((outputPin.OwnerTool, currentNumber + 1));
                                visited.Add(outputPin.OwnerTool);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 深度优先搜索
        /// </summary>
        /// <param name="tool"></param>
        /// <param name="result"></param>
        /// <param name="visited"></param>
        /// <param name="number"></param>
        private static void Preorder(Tool tool, Dictionary<Tool, int> result, HashSet<Tool> visited, int number = 1)
        {
            if (tool == null || visited.Contains(tool))
                return;

            visited.Add(tool);

            if (!result.ContainsKey(tool))
            {
                result[tool] = number;
            }
            if (tool.OutputPins != null)
            {
                foreach (var item in tool.OutputPins)
                {
                    foreach (var outputPin in item.LinkPins)
                    {
                        if (outputPin?.OwnerTool != null)
                        {
                            Preorder(outputPin.OwnerTool, result, visited, number + 1);
                        }
                    }
                }
            }
        }

        public void SetupModuleActionConfig(Guid actionId, List<ParameterItem> Parameters)
        {
            if (_moduleWithParameterTools.TryGetValue(actionId, out var moduleWithParameterTool))
            {
                if (moduleWithParameterTool.ModuleFuncCodeParameter != null)
                {
                    foreach (var paramItem in Parameters)
                    {
                        var targetParam = moduleWithParameterTool.ModuleFuncCodeParameter.FuncCodeParamterInfos
                                                 .FirstOrDefault(p => p.ParameterName == paramItem.Name);

                        if (targetParam != null)
                        {
                            targetParam.ParameterDescription = paramItem.Description;
                            targetParam.ParameterMaxValue = paramItem.MaxValue;
                            targetParam.ParameterMinValue = paramItem.MinValue;
                            targetParam.ParameterUnit = paramItem.Unit;
                            targetParam.ParameterValueFactory["0"] = paramItem.Value;
                        }
                    }
                }
            }
        }


        public void SetupLabTrayInitConfig(long labtrayId, LabTrayInfo labTrayInfo)
        {
            foreach (var labTrayService in _labTrayServices)
            {
                foreach (var labTray in labTrayService.LabTrays)
                {
                    if (labTray?.LabTrayId == labtrayId)
                    {
                        labTray?.InitLabTrayInfo(labTrayInfo);
                        if (labTray?.IsSampleTray == true)
                        {
                            _logger?.LogInformation($"LabTray {labTray.LabTrayId}{labTray.LabTrayName} is SampleTray");
                        }
                    }
                }
            }
        }

        public List<EbrParameterConfig> GetEbrParameterConfigs()
        {
            var list = new List<EbrParameterConfig>();
            var tools = _tools.Values.Where(p => p is IModuleWithParameterTool).ToList();
            foreach (var item in tools)
            {
                var moduleWithParameterTool = (IModuleWithParameterTool)item;
                if (moduleWithParameterTool != null
                    && moduleWithParameterTool.ModuleFuncCodeParameter != null
                    && moduleWithParameterTool.ModuleFuncCodeParameter.ModuleInfoParameter != null)
                {
                    foreach (var moduleEbrInfoItems in moduleWithParameterTool.ModuleFuncCodeParameter.ChannelEbrInfos.SelectMany(p => p.Value))
                    {
                        EbrParameterConfig config = new()
                        {
                            ModuleActionDescription = moduleWithParameterTool.ModuleFuncCodeParameter.FuncCodeDescription,
                            ModuleActionId = item.UniqueId.ToString(),
                            EbrUnit = moduleEbrInfoItems.EbrUnit,
                            EbrName = moduleEbrInfoItems.EbrName,
                            EbrType = moduleEbrInfoItems.EbrType,
                            EbrDescription = moduleEbrInfoItems.EbrDescription
                        };
                        list.Add(config);
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// 获取进样服务
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public ISampleInjectService[] GetSampleInjectServices()
        {
            return [.. _sampleInjectServices];
        }
    }

}
