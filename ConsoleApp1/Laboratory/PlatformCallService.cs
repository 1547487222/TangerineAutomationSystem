using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using QStandaedPlatform.Engine.Common.Common;
using QStandaedPlatform.Engine.Common.Common.SampleEntitys;
using QStandaedPlatform.Engine.Components;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Laboratory
{
    public class PlatformCallService:CallServiceBase
    {
        private readonly PlatformInfo _platformInfo;
        private readonly PlatformStateMachine _platformStateMachine;
        private readonly PlatformMonitorService _platformMonitorService;
        private readonly Dictionary<string,IH5uTcp> _modules = [];
        private readonly ConcurrentDictionary<long, TaskWorker> _runningTask = new();
        private readonly ConcurrentDictionary<long, TaskWorker> _cachedTask = new();
        private readonly ILogger<PlatformCallService> _logger;
        private readonly SemaphoreSlim _oprationLock = new(1, 1);
        public PlatformCallService(PlatformInfo platformInfo)
        {
            _platformInfo = platformInfo;
            _platformStateMachine = new PlatformStateMachine(this);
            _platformMonitorService = new PlatformMonitorService(_platformInfo.PlatformMonitorOptions);
            _logger=LoggerProviderManager.GetLoggerFactory().CreateLogger<PlatformCallService>();
        }


        public PlatformInfo PlatformInfo => _platformInfo;

        public List<TaskWorker> InitTaskWorkers { get; } = [];

        public List<TaskWorker> PrepareTaskWorkers { get; } = [];

        public List<TaskWorker> RunTaskWorkers { get; } = [];

        public List<TaskWorker> StorageTaskWorkers { get; } = [];

        public List<TaskWorker> FinalizeTaskWorkers { get; } = [];

        public PlatformStateMachine PlatformStateMachine => _platformStateMachine;

        public PlatformMonitorService PlatformMonitorService => _platformMonitorService;


        public List<IH5uTcp> Modules 
        {
            get 
            {
                lock (_modules)
                {
                    if (_modules.Count == 0)
                    {
                        foreach (var taskWorker in RunTaskWorkers)
                        {
                            foreach (var tool in taskWorker.Flow.GetTools())
                            {
                                if (tool is IModuleTool moduleTool)
                                {
                                    _modules[moduleTool.GetModular().Messenger.Ip] = moduleTool.GetModular().Messenger;
                                }
                            }
                        }
                    }
                    return [.. _modules.Values];
                }
            }
        }
        /// <summary>
        /// 停机
        /// </summary>
        /// <returns></returns>
        public async Task StopPlatformAsync()
        {
            foreach (var item in Modules.Select(p => new Modular(p)))
            {
               await item.EmergencyStopAsync();
            }
        }

        /// <summary>
        /// 触发平台异常
        /// </summary>
        public void TriggerErrorOccurred()
        {
            if (_platformStateMachine.CanTransition(StateChangeEvent.ErrorOccurred))
            {
                _platformStateMachine.TriggerTransition(StateChangeEvent.ErrorOccurred);
            }
            else
            {
                _logger.LogWarning($"TriggerErrorOccurred: StateChangeEvent.ErrorOccurred is not allowed, current state is {_platformStateMachine.CurrentPlatformState.Status}");
            }
        }

        public override void InitializeConfiguration()
        {
            foreach (var platformTaskInfo in _platformInfo.InitialInfo)
            {
                _logger.LogInformation($"InitializeConfiguration: FlowConfigPath:{platformTaskInfo.FlowConfigPath}");
                if (WorkFlowEngine.Instance.ReadFlow(new FlowFileDescription
                {
                    FilePath = platformTaskInfo.FlowConfigPath,
                    FlowId = platformTaskInfo.FlowId,
                }, out var flow))
                {
                    if (flow != null)
                    {
                        _logger.LogInformation($"InitializeConfiguration: FlowId:{platformTaskInfo.FlowConfigPath}");
                        InitTaskWorkers.Add(new TaskWorker(flow)
                        {
                            PlatformId = _platformInfo.PlatformId,
                            PlatformName = _platformInfo.PlatformName,
                            PlatformTaskCode = platformTaskInfo.PlatformTaskCode,
                            PlatformTaskDescription = platformTaskInfo.PlatformTaskDescription,
                            PlatformTaskId = platformTaskInfo.PlatformTaskId,
                        });
                    }
                }
            }

            foreach (var platformTaskInfo in _platformInfo.PrepareExperimentInfo)
            {
                if (WorkFlowEngine.Instance.ReadFlow(new FlowFileDescription
                {
                    FilePath = platformTaskInfo.FlowConfigPath,
                    FlowId = platformTaskInfo.FlowId,
                }, out var flow))
                {
                    if (flow != null)
                    {
                        _logger.LogInformation($"PrepareExperimentInfo: FlowId:{platformTaskInfo.FlowConfigPath}");
                        PrepareTaskWorkers.Add(new TaskWorker(flow)
                        {
                            PlatformId = _platformInfo.PlatformId,
                            PlatformName = _platformInfo.PlatformName,
                            PlatformTaskCode = platformTaskInfo.PlatformTaskCode,
                            PlatformTaskDescription = platformTaskInfo.PlatformTaskDescription,
                            PlatformTaskId = platformTaskInfo.PlatformTaskId,
                        });
                    }
                }
            }

            foreach (var platformTaskInfo in _platformInfo.TaskInfo)
            {
                if (WorkFlowEngine.Instance.ReadFlow(new FlowFileDescription
                {
                    FilePath = platformTaskInfo.FlowConfigPath,
                    FlowId = platformTaskInfo.FlowId,
                }, out var flow))
                {
                    if (flow != null)
                    {
                        _logger.LogInformation($"TaskInfo: FlowId:{platformTaskInfo.FlowConfigPath}");
                        RunTaskWorkers.Add(new TaskWorker(flow)
                        {
                            PlatformId = _platformInfo.PlatformId,
                            PlatformName = _platformInfo.PlatformName,
                            PlatformTaskCode = platformTaskInfo.PlatformTaskCode,
                            PlatformTaskDescription = platformTaskInfo.PlatformTaskDescription,
                            PlatformTaskId = platformTaskInfo.PlatformTaskId,
                        });
                    }
                }
            }

            foreach (var platformTaskInfo in _platformInfo.SystemStorageInfo)
            {
                if (WorkFlowEngine.Instance.ReadFlow(new FlowFileDescription
                {
                    FilePath = platformTaskInfo.FlowConfigPath,
                    FlowId = platformTaskInfo.FlowId,
                }, out var flow))
                {
                    if (flow != null)
                    {
                        _logger.LogInformation($"SystemStorageInfo: FlowId:{platformTaskInfo.FlowConfigPath}");
                        StorageTaskWorkers.Add(new TaskWorker(flow)
                        {
                            PlatformId = _platformInfo.PlatformId,
                            PlatformName= _platformInfo.PlatformName,
                            PlatformTaskCode = platformTaskInfo.PlatformTaskCode,
                            PlatformTaskDescription = platformTaskInfo.PlatformTaskDescription,
                            PlatformTaskId = platformTaskInfo.PlatformTaskId,

                        });
                    }
                }
            }

            foreach (var platformTaskInfo in _platformInfo.FinalizeInfo)
            {
                if (WorkFlowEngine.Instance.ReadFlow(new FlowFileDescription
                {
                    FilePath = platformTaskInfo.FlowConfigPath,
                    FlowId = platformTaskInfo.FlowId,
                }, out var flow))
                {
                    if (flow != null)
                    {
                        _logger.LogInformation($"FinalizeInfo: FlowId:{platformTaskInfo.FlowConfigPath}");
                        FinalizeTaskWorkers.Add(new TaskWorker(flow)
                        {
                            PlatformId = _platformInfo.PlatformId,
                            PlatformName = _platformInfo.PlatformName,
                            PlatformTaskCode = platformTaskInfo.PlatformTaskCode,
                            PlatformTaskDescription = platformTaskInfo.PlatformTaskDescription,
                            PlatformTaskId = platformTaskInfo.PlatformTaskId,

                        });
                    }
                }
            }

            _platformMonitorService.Initialize();
        }

        public InternalPlatformStatus PlatformStatus
        {
            get => _platformStateMachine.CurrentPlatformState.Status;
        }

        public string GetCurrentTaskTrayUsage()
        {
            if (!_runningTask.IsEmpty)
            {
                return JsonConvert.SerializeObject(_runningTask.First().Value.GetLabTrayInfos());
            }
            else
            {
                if (!_cachedTask.IsEmpty)
                {
                    return JsonConvert.SerializeObject(_cachedTask.Last().Value.GetLabTrayInfos());
                }
            }
            return string.Empty;
        }

        public string GetCurrentTaskTrayLabwareInfo()
        {
            if (!_runningTask.IsEmpty)
            {
                return JsonConvert.SerializeObject(_runningTask.First().Value.GetLabTrayLabwareInfos());
            }
            else
            {
                if (!_cachedTask.IsEmpty)
                {
                    return JsonConvert.SerializeObject(_cachedTask.Last().Value.GetLabTrayLabwareInfos());
                }
            }
            return string.Empty;
        }

        public string GetCurrentTrayInitialBindingInfo()
        {
            if (!_runningTask.IsEmpty)
            {
                return JsonConvert.SerializeObject(_runningTask.First().Value.GetLabTrayInitialBindingInfos());
            }
            else
            {
                if (!_cachedTask.IsEmpty)
                {
                    return JsonConvert.SerializeObject(_cachedTask.Last().Value.GetLabTrayInitialBindingInfos());
                }
            }
            return string.Empty;
        }

        public async Task<CallStatus> InitializeAsync(Dictionary<string, string> context)
        {
            if (InitTaskWorkers.Count > 0)
            {
                var InitWorker = InitTaskWorkers.First();
                foreach (var item in context)
                {
                    if (Guid.TryParse(item.Key, out var guid))
                    {
                        var parameterItems = JsonConvert.DeserializeObject<List<ParameterItem>>(item.Value);
                        if (parameterItems != null)
                        {
                            InitWorker.SetupTaskActinParameters(guid, parameterItems);
                        }
                    }
                }
                var result = await InitWorker.PrologueAsync();
                foreach (var item in RunTaskWorkers)
                {
                   await item.Flow.ResetIfFaultedAsync();
                }
                if (result.Code == 0)
                {
                    _platformStateMachine.TriggerTransition(StateChangeEvent.Initialize);
                }
                return result;
            }
            else
            {
                return new CallStatus
                {
                    Message = "未找到初始化任务",
                    Code = -1,
                };
            }
        }


     

        public async Task<CallStatus> PrepareAsync(Dictionary<string, string> context)
        {
            if (PrepareTaskWorkers.Count > 0)
            {
                var prepareWorker = PrepareTaskWorkers.First();
                foreach (var item in context)
                {
                    if (Guid.TryParse(item.Key, out var guid))
                    {
                        var parameterItems = JsonConvert.DeserializeObject<List<ParameterItem>>(item.Value);
                        if (parameterItems != null)
                        {
                            prepareWorker.SetupTaskActinParameters(guid, parameterItems);
                        }
                    }
                }
                var result = await prepareWorker.PrologueAsync();
                return result;
            }
            else
            {
                return new CallStatus
                {
                    Message = "未找到实验前准备任务（可空，不报错）"
                };
            }
        }

        /// <summary>
        /// 设置任务数据
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<CallStatus> SetupTaskAsync(long platformTaskid, Dictionary<string, string> context)
        {
            var setupWorker = RunTaskWorkers.Find(x => x.PlatformTaskId == platformTaskid);
            if (setupWorker == null)
            {
                return new CallStatus(false, $"RunTaskWorkers not found platfromTaskid:{platformTaskid}");
            }
            try
            {
                foreach (var item in context)
                {
                    if (long.TryParse(item.Key, out var number))
                    {
                        var labTrayInfo = JsonConvert.DeserializeObject<LabTrayInfo>(item.Value);
                        if (labTrayInfo != null)
                        {
                            setupWorker.SetupLabatoryInitConfig(number, labTrayInfo);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return new CallStatus(false, $"SetupTaskAsync error:{ex.Message}");
            }
            await Task.CompletedTask;
            return new CallStatus(true, "设置任务数据完成");
        }

        /// <summary>
        /// 校验是否可以开始任务
        /// </summary>
        /// <param name="platformTaskid"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> CheckTaskAsync(long platformTaskid)
        {
           await  _oprationLock.WaitAsync();
            try
            {
                if (_platformStateMachine.CurrentPlatformState.Status != InternalPlatformStatus.InProgress)
                {
                    if (!_platformStateMachine.CanTransition(StateChangeEvent.StartTask))
                    {
                        _logger?.LogInformation($"CheckTaskAsync platfromTaskid:{platformTaskid} can not start task");
                        return await Task.FromResult(false);
                    }
                }

                if (!_runningTask.IsEmpty && !_runningTask.ContainsKey(platformTaskid))
                {
                    _logger?.LogInformation($"CheckTaskAsync platfromTaskid:{platformTaskid} already running");
                    throw new Exception($"RunTaskWorkers already running platfromTaskid:{platformTaskid}");
                }
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                _oprationLock.Release();
            }
            return await Task.FromResult(true);
        }
        /// <summary>
        /// 开始任务
        /// </summary>
        /// <param name="platformTaskid"></param>
        /// <param name="sampleTaskInfos"></param>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async IAsyncEnumerable<SampleTraceEntity> RunAsync(long platformTaskid, SampleTaskInfo[] sampleTaskInfos, Dictionary<string, string> context, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {         
            _logger.LogInformation($"RunAsync platfromTaskid:{platformTaskid}");
            if (!_runningTask.TryGetValue(platformTaskid, out var runWorker))
            {
                runWorker = RunTaskWorkers.Find(x => x.PlatformTaskId == platformTaskid) ?? throw new Exception($"RunTaskWorkers not found platformTaskid:{platformTaskid}");
                _runningTask.TryAdd(platformTaskid, runWorker);
            }
            runWorker.Flow.FlowState.StateChanged += (sender, args) =>
            {
                _logger?.LogInformation($"RunAsync FlowStateChanged platfromTaskid:{platformTaskid} state:{args.NewState}");
                if (args.NewState == FlowState.Error)
                {
                    TriggerErrorOccurred();
                }
            };
            if (!runWorker.ContainsSampleings(sampleTaskInfos))
            {
                foreach (var item in context)
                {
                    if (Guid.TryParse(item.Key, out var guid))
                    {
                        var parameterItems = JsonConvert.DeserializeObject<List<ParameterItem>>(item.Value);
                        if (parameterItems != null)
                        {
                            runWorker.SetupTaskActinParameters(guid, parameterItems);
                        }
                    }
                }
                runWorker.InjectSamplings(sampleTaskInfos);
            }
            else
            {
                _logger?.LogInformation($"RunAsync platformTaskid:{platformTaskid} already contains sampleings{string.Join(",", sampleTaskInfos.ToList())}");
            }
           
            _platformStateMachine.TriggerTransition(StateChangeEvent.StartTask);
            _logger?.LogInformation($"RunAsync StartTask platformTaskid:{platformTaskid}");
           await foreach (var sample in runWorker.GetSampleTracesAsync(sampleTaskInfos,cancellationToken))
            {
                if (sample.Status == SampleTraceStatus.Alert)
                {
                    _platformStateMachine.TriggerTransition(StateChangeEvent.ErrorOccurred);
                }
                else
                {
                    //只有运行中才上报数据
                    _logger?.LogInformation($"RunAsync SampleTrace platformTaskid:{platformTaskid} sample:{JsonConvert.SerializeObject(sample)}");
                    yield return sample;
                }
            }
            runWorker.Flow.FlowState.StateChanged -= (sender, args) => { };
            _runningTask.TryRemove(platformTaskid, out _);
            if (!_cachedTask.IsEmpty)
            {
                _cachedTask.TryRemove(platformTaskid, out _);
            }
            _cachedTask.TryAdd(platformTaskid, runWorker);
            runWorker.ClearSamplings(sampleTaskInfos);
            _logger?.LogInformation($"RunAsync EndTask platformTaskid:{platformTaskid}");
            if (_runningTask.IsEmpty)
            {
                _platformStateMachine.TriggerTransition(StateChangeEvent.TaskCompleted);
            }
        }

        /// <summary>
        /// 封存
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<CallStatus> StorageAsync(Dictionary<string, string> context)
        {
            if (StorageTaskWorkers.Count > 0)
            {
                var storageWorker = StorageTaskWorkers.First();
                foreach (var item in context)
                {
                    if (Guid.TryParse(item.Key, out var guid))
                    {   
                        var parameterItems = JsonConvert.DeserializeObject<List<ParameterItem>>(item.Value);
                        if (parameterItems != null)
                        {
                            storageWorker.SetupTaskActinParameters(guid, parameterItems);
                        }
                    }
                }
                var result = await storageWorker.PrologueAsync();
                return result;
            }
            else
            {
                return new CallStatus
                {
                    Message = "未找到实验封存的任务"
                };
            }
        }

        /// <summary>
        /// 收尾
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<CallStatus> FinalizeAsync(Dictionary<string, string> context)
        {
            if (FinalizeTaskWorkers.Count > 0)
            {
                var finalizeWorker = FinalizeTaskWorkers.First();
                foreach (var item in context)
                {
                    if (Guid.TryParse(item.Key, out var guid))
                    {
                        var parameterItems = JsonConvert.DeserializeObject<List<ParameterItem>>(item.Value);
                        if (parameterItems != null)
                        {
                            finalizeWorker.SetupTaskActinParameters(guid, parameterItems);
                        }
                    }
                }
                var result = await finalizeWorker.PrologueAsync();
                return result;
            }
            else
            {
                return new CallStatus 
                {
                    Message = "未找到实验收尾的任务"
                };
            }
        }

        public async Task StartAsync()
        {
            if (_platformStateMachine.CanTransition(StateChangeEvent.Launch))
            {
                foreach (var worker in RunTaskWorkers)
                {
                    try
                    {
                        await worker.Flow.RequestStartAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogInformation($"StartAsync Error platformTaskid:{worker.Flow.FlowName} message:{ex}");
                    }
                }
                _platformStateMachine.TriggerTransition(StateChangeEvent.Launch);
            }
            else
            {
                _logger.LogInformation($"StartAsync Error platformName:{_platformInfo.PlatformName} platformStatus:{_platformStateMachine.CurrentPlatformState.Status}");
            }
        }
        /// <summary>
        /// 复位
        /// </summary>
        /// <returns></returns>
        public async Task ResetAsync()
        {
            if (_platformStateMachine.CanTransition(StateChangeEvent.Reset))
            {
                foreach (var worker in _runningTask)
                {
                    await worker.Value.Flow.RequestResetAsync();
                }
                _platformStateMachine.TriggerTransition(StateChangeEvent.Reset);
            }
            else
            {
                _logger.LogInformation($"ResetAsync Error platformName:{_platformInfo.PlatformName} platformStatus:{_platformStateMachine.CurrentPlatformState.Status}");
            }
        }


        /// <summary>
        /// 暂停
        /// </summary>
        /// <returns></returns>
        public async Task PauseAsync()
        {
            if (_platformStateMachine.CanTransition(StateChangeEvent.Pause))
            {
                foreach (var worker in _runningTask)
                {
                    await worker.Value.Flow.RequestPauseAsync();
                }
                _platformStateMachine.TriggerTransition(StateChangeEvent.Pause);
            }
            else
            {
                _logger.LogInformation($"PauseAsync Error platformName:{_platformInfo.PlatformName} platformStatus:{_platformStateMachine.CurrentPlatformState.Status}");
            }
        }

        /// <summary>
        /// 继续
        /// </summary>
        /// <returns></returns>
        public async Task ResumeAsync()
        {
            if (_platformStateMachine.CurrentPlatformState.Status
                == InternalPlatformStatus.Idle
                && _platformStateMachine.CanTransition(StateChangeEvent.Resume))
            {
                foreach (var worker in _runningTask)
                {
                    await worker.Value.Flow.RequestStartAsync();
                }
                _platformStateMachine.TriggerTransition(StateChangeEvent.Resume);
            }
            else if (_platformStateMachine.CanTransition(StateChangeEvent.Resume))
            {
                foreach (var worker in _runningTask)
                {
                    await worker.Value.Flow.RequestContinueAsync();
                }
                _platformStateMachine.TriggerTransition(StateChangeEvent.Resume);
            }
            else
            {
                _logger.LogInformation($"ResumeAsync Error platformName:{_platformInfo.PlatformName} platformStatus:{_platformStateMachine.CurrentPlatformState.Status}");
            }
        }

        /// <summary>
        /// 取消
        /// </summary>
        /// <returns></returns>
        public async Task CancelAsync()
        {
            if (_platformStateMachine.CanTransition(StateChangeEvent.Stop))
            {
                _platformStateMachine.TriggerTransition(StateChangeEvent.Stop);
                foreach (var worker in _runningTask)
                {
                    await worker.Value.Flow.StopRequestAsync();
                }
                _runningTask.Clear();
                _cachedTask.Clear();
                _platformStateMachine.TriggerTransition(StateChangeEvent.ClearResource);
            }
            else
            {
                _logger.LogInformation($"CancelAsync Error platformName:{_platformInfo.PlatformName} platformStatus:{_platformStateMachine.CurrentPlatformState.Status}");
            }
        }
    }
    public class TaskWorker
    {
        public long PlatformId { get; set; }
        public long PlatformTaskId { get; set; }
        public string PlatformName { get; set; } = string.Empty;
        public string PlatformTaskCode { get; set; } = string.Empty;
        public string PlatformTaskDescription { get; set; } = string.Empty;

        private readonly ILogger<TaskWorker> _logger;
        private readonly SemaphoreSlim _startPrologueSemaphore = new(1, 1);

        public TaskWorker(Flow flow)
        {
            Flow = flow;
            _logger = LoggerProviderManager.GetLoggerFactory().CreateLogger<TaskWorker>();
        }

        public Flow Flow { get; }

        private readonly object _syncLock = new();

        public Dictionary<long,BlockingCollection<SampleTraceEntity>> SampleTraces { get; private set; } = [];

        public Dictionary<long,SampleTaskInfo> SampleTaskInfos { get; private set; } = [];


        public bool ContainsSampleings(SampleTaskInfo[] sampleTaskInfos)
        {
            lock (_syncLock)
            {
                return sampleTaskInfos.All(x => SampleTaskInfos.ContainsKey(x.SamplingId));
            }
        }


        public void InjectSamplings(SampleTaskInfo[] sampleTaskInfo)
        {
            lock (_syncLock)
            {
                _logger.LogInformation($"Inject sampling  for {Flow.FlowName}");
                if (Flow.GetSampleInjectServices().Length <= 0)
                {
                    throw new Exception($"进样失败{Flow.FlowName},No sample inject services found");
                }
                //打印样品任务信息
                _logger.LogInformation($"Sample task info:{string.Join("#", sampleTaskInfo.Select(x => x.ToString()))}");
                var injectService = Flow.GetSampleInjectServices()[0];
                {

                    var list = new InjectSamplingModel[sampleTaskInfo.Length];
                    for (int i = 0; i < sampleTaskInfo.Length; i++)
                    {
                        SampleTaskInfos[sampleTaskInfo[i].SamplingId] = sampleTaskInfo[i];
                        SampleTraces[sampleTaskInfo[i].SamplingId] = [];
                        list[i] = new InjectSamplingModel(sampleTaskInfo[i])
                        {
                            SampleTraceAction = sampleTrace =>
                            {
                                lock (_syncLock)
                                {
                                    SampleTraces[sampleTrace.SamplingId].Add(sampleTrace);
                                }
                            },
                            SampleCompleteAction = sampleInfo =>
                            {
                                lock (_syncLock)
                                {
                                    SampleTraces[sampleInfo.SamplingId].CompleteAdding();
                                }
                            }
                        };
                    }
                    injectService.InjectSample(list);
                }
                _logger.LogInformation($"Inject sampling for {Flow.FlowName} complete");
            }
        }

        public void ClearSamplings(SampleTaskInfo[] sampleTaskInfo)
        {
            lock (_syncLock)
            {
                foreach (var item in sampleTaskInfo)
                {
                    if (SampleTraces.TryGetValue(item.SamplingId, out var _))
                    {
                        SampleTraces.Remove(item.SamplingId);
                    }
                    if (SampleTaskInfos.TryGetValue(item.SamplingId, out var _))
                    {
                        SampleTaskInfos.Remove(item.SamplingId);
                    }
                }
            }
        }

        public IAsyncEnumerable<SampleTraceEntity> GetSampleTracesAsync(
            SampleTaskInfo[] sampleTaskInfos,
            CancellationToken cancellationToken = default)
        {
            var collections = new List<BlockingCollection<SampleTraceEntity>>();
            lock (_syncLock)
            {
                foreach (var taskInfo in sampleTaskInfos)
                {
                    if (SampleTraces.TryGetValue(taskInfo.SamplingId, out var traces))
                    {
                        collections.Add(traces);
                    }
                }
            }
            return GetTracesStream(collections, cancellationToken);
        }

        private static async IAsyncEnumerable<SampleTraceEntity> GetTracesStream(
            List<BlockingCollection<SampleTraceEntity>> collections,
            [EnumeratorCancellation] CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                int index = BlockingCollection<SampleTraceEntity>.TryTakeFromAny(
                    [.. collections],
                    out var item,
                    1000,
                    ct);

                if (index >= 0 && item != null)
                {
                    yield return await Task.FromResult(item);
                }
                else if (collections.All(c => c.IsCompleted))
                {
                    yield break;
                }
            }
        }


        public async Task<CallStatus> PrologueAsync()
        {
            await _startPrologueSemaphore.WaitAsync();
            ConcludeResult? result = null;
            try
            {
                Flow.ToolErrorCanCancel = false;
                await Flow.ClearEphemeralDataAsync();
                await Flow.ResetIfFaultedAsync();
                await Flow.StartPrologueAsync();
                Flow.ToolErrorCanCancel = true;
                var concludeResults = await Flow.WaitForConcludeAsync();
                Flow.ToolErrorCanCancel = false;
                result = concludeResults.First();
            }
            catch (Exception ex)
            {
                return new CallStatus(false, ex.ToString());
            }
            finally
            {
                _startPrologueSemaphore.Release();
            }
            return new CallStatus(result.IsSuccess,result.ErrorMessage);
        }

        public void SetupTaskActinParameters(Guid actionId, List<ParameterItem> parameters)
        {
            Flow.SetupModuleActionConfig(actionId, parameters);
        }

        public void SetupLabatoryInitConfig(long labtrayId, LabTrayInfo labTrayInfo)
        {
            Flow.SetupLabTrayInitConfig(labtrayId, labTrayInfo);
        }

        public List<LabTrayInfo> GetLabTrayInfos()
        {
            var list = new List<LabTrayInfo>();
            try
            {
                var labTrayInfos = Flow.GetLabTrayInfos();
                if (labTrayInfos != null && labTrayInfos.Count > 0)
                    list.AddRange(labTrayInfos);
            }
            catch (Exception ex)
            {
                Log(nameof(GetLabTrayInfos) + ":" + ex.ToString());
            }
            return list;
        }
        public List<LabwareInfo[]> GetLabTrayLabwareInfos()
        {
            var list = new List<LabwareInfo[]>();
            try
            {
                var labTrays = Flow.GetLabTrays();
                if (labTrays != null && labTrays.Count > 0)
                {
                    var labwareInfos = new List<LabwareInfo>();
                    foreach (var labTray in labTrays)
                    {
                        if (labTray != null)
                        {
                            var labwareInfo = labTray.GetLabwareInfos();
                            if (labwareInfo != null && labwareInfo.Length > 0)
                            {
                                labwareInfos.AddRange(labwareInfo);
                            }
                        }
                    }
                    list.Add([.. labwareInfos]);
                }
            }
            catch (Exception ex)
            {
                Log(nameof(GetLabTrayLabwareInfos) + ":" + ex.ToString());
            }
            return list;
        }

        public List<TrayInitialBindingInfo> GetLabTrayInitialBindingInfos()
        {
            var list = new List<TrayInitialBindingInfo>();
            try
            {
                var labTrays = Flow.GetLabTrays();
                if (labTrays != null && labTrays.Count > 0)
                {
                    foreach (var labTray in labTrays)
                    {
                        if (labTray != null)
                        {
                            var trayInitialBindingInfo = labTray.GetTrayInitialBindingInfo();
                            if (trayInitialBindingInfo != null)
                            {
                                list.Add(trayInitialBindingInfo);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log(nameof(GetLabTrayInitialBindingInfos) + ":" + ex.ToString());

            }
            return list;
        }


        private void Log(string message)
        {
            //平台id|平台名称|平台任务Id|平台任务编码|平台任务描述|信息
            _logger.LogInformation($"{PlatformId}|{PlatformName}|{PlatformTaskId}|{PlatformTaskCode}|{PlatformTaskDescription}|{message}");
        }

    }
}
