using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using QStandaedPlatform.Engine.Common.Common;
using QStandaedPlatform.Engine.Laboratory;
using System.Collections.Concurrent;

namespace Tangerine.Grpc.Foundation.Services
{
    /// <summary>
    /// 配置服务
    /// </summary>
    public class ConfigServiceImpl : Tangerine.Grpc.Foundation.ConfigService.ConfigServiceBase
    {
        public ConfigServiceImpl(AppLabOsLogicService appLabOsLogicService)
        {
            _appService = appLabOsLogicService;
            _logger = LoggerProviderManager.GetLoggerFactory().CreateLogger<ConfigServiceImpl>();
        }
        private readonly AppLabOsLogicService _appService;
        private readonly ILogger<ConfigServiceImpl> _logger;
        public override Task<LaboratoryDefinition> GetLaboratoryConfigs(Empty request, ServerCallContext context)
        {
            LaboratoryConfig laboratoryConfig = new()
            {
                LaboratoryId = _appService.Laboratory.LaboratoryId,
                LaboratoryName = _appService.Laboratory.LaboratoryName,
                LaboratoryCode = _appService.Laboratory.LaboratoryCode,
                LaboratoryDescription = _appService.Laboratory.LaboratoryDescription,
            };
            foreach (var productionLine in _appService.ProductionLines)
            {
                var productionlineConfig = new ProductionlineConfig
                {
                    LineId = productionLine.LineId,
                    LineName = productionLine.LineName,
                    LineCode = productionLine.LineCode,
                    LineDescription = productionLine.LineDescription,
                };

                foreach (var item in productionLine.Processflows)
                {
                    var processflowConfig = new ProcessflowConfig
                    {
                        ProcessCode = item.ProcessCode,
                        ProcessId = item.ProcessId,
                        ProcessName = item.ProcessName,
                        ProcessType = item.ProcessType,
                    };
                    foreach (var platformTaskInfo in item.PlatformTasks)
                    {
                        var platformTaskProfile = new PlatformTaskProfile
                        {
                            PlatformId = platformTaskInfo.PlatformId,
                            PlatformTaskCode = platformTaskInfo.PlatformTaskCode,
                            PlatformTaskDescription = platformTaskInfo.PlatformTaskDescription,
                            PlatformTaskId = platformTaskInfo.PlatformTaskId,
                            ActionConfigs = platformTaskInfo.ActionConfigs,
                        };
                        processflowConfig.PlatformTaskConfigs.Add(platformTaskProfile);
                    }
                    // Add transfer steps
                    foreach (var transferStepInfo in item.TransferSteps)
                    {
                        var transferStepConfig = new TransferStepConfig
                        {
                            StepId = transferStepInfo.StepId,
                            StepOrder = transferStepInfo.StepOrder,
                            StepDescription = transferStepInfo.StepDescription,
                            TransferModuleId = transferStepInfo.TransferModuleId,
                            TransferDirection = transferStepInfo.TransferDirection,
                            SourcePlatformId = transferStepInfo.SourcePlatformId,
                            TargetPlatformId = transferStepInfo.TargetPlatformId,
                        };
                        processflowConfig.TransferStepConfigs.Add(transferStepConfig);
                    }
                    // Add module action steps
                    foreach (var moduleActionStepInfo in item.ModuleActionSteps)
                    {
                        var moduleActionStepConfig = new ModuleActionStepConfig
                        {
                            StepId = moduleActionStepInfo.StepId,
                            StepOrder = moduleActionStepInfo.StepOrder,
                            StepDescription = moduleActionStepInfo.StepDescription,
                            ModuleName = moduleActionStepInfo.ModuleName,
                            ModuleSerialNumber = moduleActionStepInfo.ModuleSerialNumber,
                            ModuleActionId = moduleActionStepInfo.ModuleActionId,
                            ActionName = moduleActionStepInfo.ActionName,
                            ActionDescription = moduleActionStepInfo.ActionDescription,
                            ActionParameters = [.. moduleActionStepInfo.ActionParameters],
                        };
                        processflowConfig.ModuleActionStepConfigs.Add(moduleActionStepConfig);
                    }
                    productionlineConfig.ProcessflowConfigs.Add(processflowConfig);
                }

                foreach (var item in productionLine.Platforms)
                {
                    var platformConfig = new PlatformConfig
                    {
                        PlatformId = item.PlatformId,
                        PlatformName = item.PlatformName,
                        PlatformCode = item.PlatformCode,
                        PlatformMaxCacheCount = item.PlatformMaxCacheCount,
                        PlatformSamplingFlux = item.PlatformSamplingFlux,
                        PlatformMaxExecuteCount = item.PlatformMaxExecuteCount,
                        PlatformDescription = item.PlatformDescription,
                        LabTrayConfigurations = item.LabTrayConfigs,
                        MonitorParameterConfigs = [.. item.PlatformMonitorOptions.Items.Select(p => new MonitorParameterConfig
                        {
                            ModuleName = p.ModuleName,
                            MonitorDescription = p.MonitorKeyDescription,
                            MonitorName = p.MonitorKey,
                            MonitorType = p.MonitorType,
                            MonitorUnit = p.MonitorUnit,
                        })]
                    };

                    if (item.InitialInfo.Count > 0)
                    {
                        foreach (var initialInfo in item.InitialInfo)
                        {
                            platformConfig.InitTaskProfiles.Add(new PlatformTaskProfile
                            {
                                PlatformId = initialInfo.PlatformId,
                                PlatformTaskCode = initialInfo.PlatformTaskCode,
                                ActionConfigs = initialInfo.ActionConfigs,
                                PlatformTaskDescription = initialInfo.PlatformTaskDescription,
                                PlatformTaskId = initialInfo.PlatformTaskId,
                            });
                        }
                    }

                    if (item.TaskInfo.Count > 0)
                    {
                        foreach (var taskInfo in item.TaskInfo)
                        {
                            platformConfig.ExperimentTaskProfiles.Add(new PlatformTaskProfile
                            {
                                PlatformId = taskInfo.PlatformId,
                                PlatformTaskCode = taskInfo.PlatformTaskCode,
                                ActionConfigs = taskInfo.ActionConfigs,
                                PlatformTaskDescription = taskInfo.PlatformTaskDescription,
                                PlatformTaskId = taskInfo.PlatformTaskId,
                                TaskEbrParameterConfigs = taskInfo.TaskEbrParameterConfigs,
                            });
                        }
                    }

                    if (item.PrepareExperimentInfo.Count > 0)
                    {
                        foreach (var prepareExperimentInfo in item.PrepareExperimentInfo)
                        {
                            platformConfig.PrepareExperimentTaskProfiles.Add(new PlatformTaskProfile
                            {
                                PlatformId = prepareExperimentInfo.PlatformId,
                                PlatformTaskCode = prepareExperimentInfo.PlatformTaskCode,
                                ActionConfigs = prepareExperimentInfo.ActionConfigs,
                                PlatformTaskDescription = prepareExperimentInfo.PlatformTaskDescription,
                                PlatformTaskId = prepareExperimentInfo.PlatformTaskId,
                            });
                        }
                    }

                    if (item.SystemStorageInfo.Count > 0)
                    {
                        foreach (var systemStorageInfo in item.SystemStorageInfo)
                        {
                            platformConfig.SystemStorageTaskProfiles.Add(new PlatformTaskProfile
                            {
                                PlatformId = systemStorageInfo.PlatformId,
                                ActionConfigs = systemStorageInfo.ActionConfigs,
                                PlatformTaskCode = systemStorageInfo.PlatformTaskCode,
                                PlatformTaskDescription = systemStorageInfo.PlatformTaskDescription,
                                PlatformTaskId = systemStorageInfo.PlatformTaskId,
                            });
                        }
                    }

                    if (item.FinalizeInfo.Count > 0)
                    {
                        foreach (var finalizeInfo in item.FinalizeInfo)
                        {
                            platformConfig.FinalizeTaskProfiles.Add(new PlatformTaskProfile
                            {
                                PlatformId = finalizeInfo.PlatformId,
                                PlatformTaskCode = finalizeInfo.PlatformTaskCode,
                                PlatformTaskId = finalizeInfo.PlatformTaskId,
                                PlatformTaskDescription = finalizeInfo.PlatformTaskDescription,
                                ActionConfigs = finalizeInfo.ActionConfigs,
                            });
                        }
                    }

                    productionlineConfig.PlatformConfigs.Add(platformConfig);
                }
                foreach (var item in productionLine.TransferModules)
                {
                    productionlineConfig.TransferModuleConfigs.Add(new TransferModuleConfig
                    {
                        LeftChannelGroupId = item.LeftChannelGroupId,
                        LeftPlatformId = item.LeftPlatformId,
                        RightChannelGroupId = item.RightChannelGroupId,
                        RightPlatformId = item.RightPlatformId,
                        IsReverse = item.IsReverse,
                        TransferBackwardMoveId = item.TransferBackwardMoveId,
                        TransferForwardMoveId = item.TransferForwardMoveId,
                        TransferModuleInfoId = item.TransferModuleInfoId,
                        TransferModuleId = item.TransferModuleId,
                        TransferModuleName = item.TransferModuleName,
                        TransferModuleSamplingFlux = item.TransferModuleSamplingFlux,
                    });
                }
                laboratoryConfig.ProductionlineConfigs.Add(productionlineConfig);
            }
            var json = JsonConvert.SerializeObject(laboratoryConfig);
            _logger.LogInformation($"获取实验室配置信息：{json}");
            return Task.FromResult(new
                 LaboratoryDefinition
            { LaboratoryJson = json });
        }
    }

    /// <summary>
    /// 平台初始化服务
    /// </summary>
    public class PlatformInitServiceImpl: Tangerine.Grpc.Foundation.PlatformInitService.PlatformInitServiceBase
    {
        private readonly AppLabOsLogicService _appService;
        private readonly ILogger<PlatformInitServiceImpl> _logger;
        public PlatformInitServiceImpl(AppLabOsLogicService appService)
        {
            _appService = appService;
            _logger = LoggerProviderManager.GetLoggerFactory().CreateLogger<PlatformInitServiceImpl>();
        }
        public override async Task<Tangerine.Grpc.Foundation.Status> Initialize(InitializeRequest request, ServerCallContext context)
        {
            try
            {
                var requestContext = request.Context.ToDictionary();
                var platformCallService = _appService.FindPlatformCallService(request.PlatformId);
                var callStatus = await platformCallService.InitializeAsync(requestContext);
                _logger.LogInformation($"初始化平台{request.PlatformId}，状态码：{callStatus.Code}，消息：{callStatus.Message}");
                return new Tangerine.Grpc.Foundation.Status
                {
                    Code = callStatus.Code,
                    Message = callStatus.Message ?? "初始化失败",
                };

            }
            catch (Exception ex)
            {
                return await Task.FromResult(new Tangerine.Grpc.Foundation.Status { Code = (int)SmartLabOsErrorCode.PlatformInitFailed, Message = ex.ToString() });
            }
        }
    }

    /// <summary>
    /// 平台监控服务
    /// </summary>
    public class PlatformMonitorServiceImpl : Tangerine.Grpc.Foundation.PlatformMonitorService.PlatformMonitorServiceBase
    {
        private readonly AppLabOsLogicService _appService;
        private readonly ILogger<PlatformMonitorServiceImpl> _logger;
        public PlatformMonitorServiceImpl(AppLabOsLogicService appService)
        {
            _appService = appService;
            _logger = LoggerProviderManager.GetLoggerFactory().CreateLogger<PlatformMonitorServiceImpl>();
        }
        public override async Task StartMonitor(StartMonitorRequest request, IServerStreamWriter<MonitorEvent> responseStream, ServerCallContext context)
        {
            var platformCallService = _appService.FindPlatformCallService(request.PlatformId);
            await foreach (var monitorResults in platformCallService.PlatformMonitorService.MonitorDataStreamAsync([.. request.MonitorKeys], context.CancellationToken))
            {
                if (context.CancellationToken.IsCancellationRequested)
                {
                    _logger.LogInformation($"停止监控平台{request.PlatformId}");
                    break;
                }
                var monitorEvent = new MonitorEvent
                {
                    PlatformId = platformCallService.PlatformInfo.PlatformId,
                    Status = new Status
                    {
                        Code = monitorResults.Any(p => p.MonitorException != null) ? (int)SmartLabOsErrorCode.MonitorException : (int)SmartLabOsErrorCode.Success,
                        Message = monitorResults.Any(p => p.MonitorException != null) ? monitorResults.FirstOrDefault()?.MonitorException?.Message : string.Empty,
                    },
                    RecordTime = Timestamp.FromDateTime(DateTime.Now.ToUniversalTime()),
                };
                foreach (var item in monitorResults)
                {
                    monitorEvent.MonitorItems.Add(new MonitorItem
                    {
                        ModuleName = item.ModuleName,
                        MonitorKey = item.MonitorKey,
                        MonitorKeyDescription = item.MonitorKeyDescription,
                        MonitorUnit = item.MonitorUnit,
                        MonitorValue = item.Value == null ? string.Empty : item.Value.ToString(),
                    });
                }
                await responseStream.WriteAsync(monitorEvent);
            }
        }

        public override async Task<MonitorEvent> GetMonitorSnapshot(MonitorSnapshotRequest request, ServerCallContext context)
        {
            var platformCallService = _appService.FindPlatformCallService(request.PlatformId);
            var monitorResults = await platformCallService.PlatformMonitorService.CaptureMonitorFrameAsync([.. request.MonitorKeys], context.CancellationToken);
            var monitorEvent = new MonitorEvent
            {
                PlatformId = platformCallService.PlatformInfo.PlatformId,
                Status = new Status
                {
                    Message = "获取监控数据成功",
                },
                RecordTime = Timestamp.FromDateTime(DateTime.Now.ToUniversalTime()),
            };
            foreach (var item in monitorResults)
            {
                monitorEvent.MonitorItems.Add(new MonitorItem
                {
                    ModuleName = item.ModuleName,
                    MonitorKey = item.MonitorKey,
                    MonitorKeyDescription = item.MonitorKeyDescription,
                    MonitorUnit = item.MonitorUnit,
                    MonitorValue = item.Value == null ? string.Empty : item.Value.ToString(),
                });
            }
            return monitorEvent;
        }
    }
    /// <summary>
    /// 平台控制服务
    /// </summary>
    public class PlatformControlServiceImpl : Tangerine.Grpc.Foundation.PlatformRuntimeService.PlatformRuntimeServiceBase
    {
        private readonly AppLabOsLogicService _appService;
        private readonly ILogger<PlatformControlServiceImpl> _logger;
        public PlatformControlServiceImpl(AppLabOsLogicService appService)
        {
            _appService = appService;
            _logger = LoggerProviderManager.GetLoggerFactory().CreateLogger<PlatformControlServiceImpl>();
        }

        public override async Task GetPlatformAlarms(PlatformRequest request, IServerStreamWriter<AlarmEvent> responseStream, ServerCallContext context)
        {
            var platformCallService = _appService.FindPlatformCallService(request.PlatformId);
            var collector = new BlockingCollection<ModuleAlarmRecord>();
            ModularAlarmService.Instance.OnAlarm += collector.Add;
            _logger.LogInformation("开始订阅平台{platformId}的报警信息", request.PlatformId);
            try
            {
                foreach (var item in collector.GetConsumingEnumerable(context.CancellationToken))
                {
                    if (context.CancellationToken.IsCancellationRequested)
                    {
                        _logger.LogInformation("停止订阅平台{platformId}的报警信息", request.PlatformId);
                        break;
                    }
                    if (platformCallService.Modules.Any(p => item.ModuleIp == p.Ip))
                    {
                        await responseStream.WriteAsync(new AlarmEvent
                        {
                            ModularAlarm = new Tangerine.Grpc.Foundation.ModularAlarm
                            {
                                ActionDescription = item.ActionDescription,
                                AlarmCode = item.AlarmCode,
                                DetailsAlarmMessage = item.DetailsAlarmMessage,
                                InternalAlarmMessage = item.InternalAlarmMessage,
                                ModuleIp = item.ModuleIp,
                                ModuleName = item.ModuleName,
                                RecordTime = Timestamp.FromDateTime(item.AlarmTime.ToUniversalTime()),
                            }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"GetPlatformAlarms{platformCallService.PlatformInfo.PlatformName} error:{ex}");
            }
            finally
            {
                ModularAlarmService.Instance.OnAlarm -= collector.Add;
            }
        }


        public override async Task<Status> StartPlatform(PlatformRequest request, ServerCallContext context)
        {
            var platformCallService = _appService.FindPlatformCallService(request.PlatformId);
            try
            {
                await platformCallService.StartAsync();
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"SrartPlatform{platformCallService.PlatformInfo.PlatformName} error:{ex}");
                return new Status 
                {
                    Message = ex.Message,
                    Code = -1
                };
            }
            return new Tangerine.Grpc.Foundation.Status { };

        }
        public override Task<PlatformStatusResponseMessage> GetPlatformStatus(PlatformRequest request, ServerCallContext context)
        {
            var platformCallService = _appService.FindPlatformCallService(request.PlatformId);
            return Task.FromResult(new PlatformStatusResponseMessage
            {
                PlatformStatus = (PlatformStatus)(int)platformCallService.PlatformStatus,
                PlatformId = platformCallService.PlatformInfo.PlatformId
            });
        }

        public override async Task<Tangerine.Grpc.Foundation.Status> PausePlatform(PlatformRequest request, ServerCallContext context)
        {
            var platformCallService = _appService.FindPlatformCallService(request.PlatformId);
            try
            {
                await platformCallService.PauseAsync();
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"PausePlatform{platformCallService.PlatformInfo.PlatformName} error:{ex}");
                return new Tangerine.Grpc.Foundation.Status
                {
                    Message = ex.Message,
                    Code = 1
                };
            }
            return new Tangerine.Grpc.Foundation.Status { };
        }

        public override async Task<Tangerine.Grpc.Foundation.Status> ResetPlatform(PlatformRequest request, ServerCallContext context)
        {
            var platformCallService = _appService.FindPlatformCallService(request.PlatformId);
            var transferCallServices = _appService.FindTransferCallServices();
            try
            {
                await platformCallService.ResetAsync();
                foreach (var transferCallService in transferCallServices)
                {
                    transferCallService.ResetAll();
                }

            }
            catch (Exception ex)
            {
                _logger.LogInformation($"ResetPlatform{platformCallService.PlatformInfo.PlatformName} error:{ex}");
                return await Task.FromResult(new Tangerine.Grpc.Foundation.Status
                {
                    Message = ex.Message,
                    Code = 1
                });
            }

            return await Task.FromResult(new Tangerine.Grpc.Foundation.Status { });
        }

        public override async Task<Tangerine.Grpc.Foundation.Status> ResumePlatform(PlatformRequest request, ServerCallContext context)
        {
            var platformCallService = _appService.FindPlatformCallService(request.PlatformId);
            try
            {
                await platformCallService.ResumeAsync();
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"ResumePlatform{platformCallService.PlatformInfo.PlatformName} error:{ex}");
                return new Tangerine.Grpc.Foundation.Status
                {
                    Message = ex.Message,
                    Code = 1
                };
            }
            return new Tangerine.Grpc.Foundation.Status { };
        }

        public override async Task<Tangerine.Grpc.Foundation.Status> StopPlatform(PlatformRequest request, ServerCallContext context)
        {
            var platformCallService = _appService.FindPlatformCallService(request.PlatformId);
            try
            {
                await platformCallService.CancelAsync();
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"StopPlatform{platformCallService.PlatformInfo.PlatformName} error:{ex}");
                return new Tangerine.Grpc.Foundation.Status
                {
                    Message = ex.Message,
                    Code = 1
                };
            }
            return new Tangerine.Grpc.Foundation.Status { };
        }
    }

    /// <summary>
    /// 平台任务服务
    /// </summary>
    public class PlatformTaskServiceImpl : Tangerine.Grpc.Foundation.PlatformTaskService.PlatformTaskServiceBase
    {
        private readonly AppLabOsLogicService _appService;
        private readonly ILogger<PlatformTaskServiceImpl> _logger;
        public PlatformTaskServiceImpl(AppLabOsLogicService appService)
        {
            _appService = appService;
            _logger = LoggerProviderManager.GetLoggerFactory().CreateLogger<PlatformTaskServiceImpl>();
        }

        public override async Task<Tangerine.Grpc.Foundation.Status> PrepareTask(PrepareTaskRequest request, ServerCallContext context)
        {
            var platformCallService = _appService.FindPlatformCallService(request.PlatformId);
            var processflowInfo = _appService.FindProcessflow(Guid.Parse(request.ProcessFlowId));
            var platformTaskid = processflowInfo.GetPlatform(request.PlatformId);
            var callStatus = await platformCallService.PrepareAsync(request.Context.ToDictionary());
            return new Tangerine.Grpc.Foundation.Status
            {
                Message = callStatus.Message,
                Code = callStatus.Code,
            };
        }

        public override async Task StartTask(StartTaskRequest request, IServerStreamWriter<TaskEvent> responseStream, ServerCallContext context)
        {
            _logger?.LogInformation($"StartTask: {request.PlatformId}, {request.ProcessFlowId}");
            var platformCallService = _appService.FindPlatformCallService(request.PlatformId);
            var processflowInfo = _appService.FindProcessflow(Guid.Parse(request.ProcessFlowId));
            var platformTask = processflowInfo.GetPlatform(request.PlatformId);
            _logger?.LogInformation($"StartTask: {platformTask.PlatformTaskId}");
            if (!await platformCallService.CheckTaskAsync(platformTask.PlatformTaskId))
            {
                //当前任务不可执行
                await responseStream.WriteAsync(new TaskEvent 
                {
                    PlatformId = platformCallService.PlatformInfo.PlatformId,
                    PlatformName = platformCallService.PlatformInfo.PlatformName,
                    PlatformTaskId = platformTask.PlatformTaskId,
                    ResponseTime = Timestamp.FromDateTimeOffset(DateTimeOffset.Now.ToUniversalTime()),
                    StartTaskStatus = new StartTaskStatus 
                    {
                        Status = Tangerine.Grpc.Foundation.TaskStatus.Failed,
                        Message = "无法开始任务，请检查当前设备状态或者日志"
                    }
                });
                return;
            }
          await foreach (var sampleTraceEntity in platformCallService.RunAsync(platformTask.PlatformTaskId, [.. request.SamplingTaskInfos.Select(p =>
            {
                return new SampleTaskInfo
                {
                    PlatformId = platformCallService.PlatformInfo.PlatformId,
                    PlatformTaskId = platformTask.PlatformTaskId,
                    SampleName = p.SampleName,
                    SampleRemarks = p.SampleRemarks,
                    SampleType = p.SampleType,
                    SamplingId = p.SampleId,
                    SamplingTaskId = request.TaskId,
                    TrayId = p.TrayId,
                    WellId = p.WellId,
                    PlatformName = platformCallService.PlatformInfo.PlatformName,
                    ProcessflowId = processflowInfo.ProcessId.ToString(),
                    WellName = p.WellName,
                };
            })], request.Context.ToDictionary(), context.CancellationToken))
           {
                var taskEvent = new TaskEvent
                {
                    PlatformId = sampleTraceEntity.PlatformId,
                    PlatformName = sampleTraceEntity.PlatformName,
                    PlatformTaskId = sampleTraceEntity.PlatformTaskId,
                    ResponseTime = Timestamp.FromDateTimeOffset(DateTimeOffset.Now.ToUniversalTime()),
                    TaskId = sampleTraceEntity.SamplingTaskId,
                    StartTaskStatus = new StartTaskStatus
                    {
                        Status = Tangerine.Grpc.Foundation.TaskStatus.Running,
                    },
                    SampleTraceMessage = new SampleTraceMessage
                    {
                        ModuleName = sampleTraceEntity.ModuleName,
                        ModuleActionDescription = sampleTraceEntity.ModuleActionDescription,
                        ModuleActionId = sampleTraceEntity.ModuleActionId,
                        ModuleSerialNumber = sampleTraceEntity.ModuleSerialNumber,
                        SampleId = sampleTraceEntity.SamplingId,
                        SampleName = sampleTraceEntity.SampleName,
                        SampleRemarks = sampleTraceEntity.SampleRemarks,
                        SampleSn = sampleTraceEntity.SampleSn,
                        SampleType = sampleTraceEntity.SampleType,
                        StartTime = Timestamp.FromDateTimeOffset(sampleTraceEntity.StartTime.ToUniversalTime()),
                        EndTime = Timestamp.FromDateTimeOffset(sampleTraceEntity.EndTime.ToUniversalTime()),
                        LabwareName = sampleTraceEntity.LabwareName,
                        ElapsedTime = sampleTraceEntity.ElapsedTime,
                    }
                };
                foreach (var sampleTaskData in sampleTraceEntity.TaskEbrDataEntities)
                {
                    taskEvent.SampleTraceMessage.EbrInfos.Add(new SampleTraceMessage.Types.EbrMessage
                    {
                        Key = sampleTaskData.EbrKey,
                        KeyDescription = sampleTaskData.EbrKeyDescription,
                        Unit = sampleTaskData.EbrUnit,
                        Value = sampleTaskData.EbrValue,
                        RecordTime = Timestamp.FromDateTimeOffset(sampleTaskData.RecordTime.ToUniversalTime()),
                    });
                }
                await responseStream.WriteAsync(taskEvent);
            }
            if (context.CancellationToken.IsCancellationRequested)
            {
                _logger?.LogInformation($"StartTask: {request.PlatformId}, {request.ProcessFlowId} is cancelled");
                return;
            }
            await responseStream.WriteAsync(new TaskEvent
            {
                StartTaskStatus = new StartTaskStatus
                {
                    Status = Tangerine.Grpc.Foundation.TaskStatus.Finished
                }
            });
        }
        public override async Task<Tangerine.Grpc.Foundation.Status> SetupTask(SetupTaskRequest request, ServerCallContext context)
        {
            var platformCallService = _appService.FindPlatformCallService(request.PlatformId);
            var processflowInfo = _appService.FindProcessflow(Guid.Parse(request.ProcessFlowId));
            var platformTask = processflowInfo.GetPlatform(request.PlatformId);
            var callStatus = await platformCallService.SetupTaskAsync(platformTask.PlatformTaskId, request.Context.ToDictionary());
            return new Tangerine.Grpc.Foundation.Status
            {
                Code = callStatus.Code,
                Message = callStatus.Message,
            };
        }

        public override async Task<Tangerine.Grpc.Foundation.Status> FinalizeTask(FinalizeTaskRequest request, ServerCallContext context)
        {
            var platformCallService = _appService.FindPlatformCallService(request.PlatformId);
            var callStatus = await platformCallService.FinalizeAsync(request.Context.ToDictionary());
            return new Tangerine.Grpc.Foundation.Status
            {
                Code = callStatus.Code,
                Message = callStatus.Message,
            };
        }

        public override async Task<Tangerine.Grpc.Foundation.Status> StoreTask(StoreTaskRequest request, ServerCallContext context)
        {
            var platformCallService = _appService.FindPlatformCallService(request.PlatformId);
            var callStatus = await platformCallService.StorageAsync(request.Context.ToDictionary());
            return new Tangerine.Grpc.Foundation.Status
            {
                Code = callStatus.Code,
                Message = callStatus.Message,
            };
        }

        /// <summary>
        /// 获取任务执行状态 未实现
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task<TaskTracesReply> GetTaskTraces(TaskTracesRequest request, ServerCallContext context)
        {
            return await Task.FromResult(new TaskTracesReply 
            {
                 PlatformId = request.PlatformId,
                 TaskId = request.TaskId,
            });
        }
        /// <summary>
        /// 获取任务使用情况
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task<TaskTrayUsageReply> GetTaskTraykUsage(TaskTrayUsageRequest request, ServerCallContext context)
        {
            var platformCallService = _appService.FindPlatformCallService(request.PlatformId);
            var json = platformCallService.GetCurrentTaskTrayUsage();
            return await Task.FromResult(new TaskTrayUsageReply
            {
                PlatformId = platformCallService.PlatformInfo.PlatformId,
                TaskUsageJson = json
            });
        }

        public override Task<TaskTrayLabwareInfoReply> GetTaskTrayLabwareInfo(TaskTrayLabwareInfoRequest request, ServerCallContext context)
        {
            var platformCallService = _appService.FindPlatformCallService(request.PlatformId);
            var json = platformCallService.GetCurrentTaskTrayLabwareInfo();
            return Task.FromResult(new TaskTrayLabwareInfoReply 
            {
                PlatformId = platformCallService.PlatformInfo.PlatformId,
                TaskTrayLabwareInfoJson = json
            });
        }

        public override Task<TaskTrayInitialBindingInfoReply> GetTrayInitialBindingInfo(TaskTrayInitialBindingInfoRequest request, ServerCallContext context)
        {
            var platformCallService = _appService.FindPlatformCallService(request.PlatformId);
            var json = platformCallService.GetCurrentTrayInitialBindingInfo();
            _logger.LogInformation($"GetTrayInitialBindingInfo:{json}");
            return Task.FromResult(new TaskTrayInitialBindingInfoReply
            {
                PlatformId = platformCallService.PlatformInfo.PlatformId,
                TaskId = request.TaskId,
                TaskTrayInitialBindingInfoJson = json
            });
        }
    }
    /// <summary>
    /// 平台中转服务
    /// </summary>
    public class PlatformTransferServiceImpl : Tangerine.Grpc.Foundation.PlatformTransferService.PlatformTransferServiceBase
    {
        private readonly AppLabOsLogicService _appService;
        private readonly ILogger<PlatformTransferServiceImpl> _logger;
        public PlatformTransferServiceImpl(AppLabOsLogicService appLabOsLogicService)
        {
            _appService = appLabOsLogicService;
            _logger = LoggerProviderManager.GetLoggerFactory().CreateLogger<PlatformTransferServiceImpl>();
        }

        public override async Task<TransferStatusResponseMessage> GetTransferStatus(TransferStatusRequest request, ServerCallContext context)
        {
            var transferCallService = _appService.FindTransferCallService(request.TransferModuleId);
            return await Task.FromResult(new TransferStatusResponseMessage
            {
                TransferModuleId = request.TransferModuleId,
                TransferStatus = (TransferStatus)(int)transferCallService.GetTransferStatus(),
            });
        }

        public override async Task<TransferMoveResponseMessage> TransferForward(TransferMoveRequest request, ServerCallContext context)
        {
            var transferCallService = _appService.FindTransferCallService(request.TransferModuleId);
            var callStatus = await transferCallService.TransferForwardAsync(context.CancellationToken);
            return await Task.FromResult(new TransferMoveResponseMessage
            {
                TransferModuleId = transferCallService.TransferId,
                Status = new Tangerine.Grpc.Foundation.Status
                {
                    Message = callStatus.Message,
                    Code = callStatus.Code
                }
            });
        }

        public override async Task<TransferMoveResponseMessage> TransferBackward(TransferMoveRequest request, ServerCallContext context)
        {
            var transferCallService = _appService.FindTransferCallService(request.TransferModuleId);
            var callStatus = await transferCallService.TransferBackwardAsync(context.CancellationToken);
            return await Task.FromResult(new TransferMoveResponseMessage
            {
                TransferModuleId = transferCallService.TransferId,
                Status = new Tangerine.Grpc.Foundation.Status
                {
                    Message = callStatus.Message,
                    Code = callStatus.Code
                }
            });
        }

        public override async Task<TransferInitResponseMessage> TransferInit(TransferInitRequest request, ServerCallContext context)
        {
            var transferCallService = _appService.FindTransferCallService(request.TransferModuleId);
            var callStatus = await transferCallService.InitializeTransferModule();
            return await Task.FromResult(new TransferInitResponseMessage
            {
                TransferModuleId = transferCallService.TransferId,
                Status = new Tangerine.Grpc.Foundation.Status
                {
                    Message = callStatus.Message,
                    Code = callStatus.Code
                }
            });
        }

        public override async Task<TransferTrayMoveResponse> TransferToTray(TransferTrayMoveRequest request, ServerCallContext context)
        {
            var transferCallService = _appService.FindTransferCallService(request.TransferModuleId);
            if (transferCallService.IsLeftPlatform(request.PlatformId))
            {
                var callStatus = await transferCallService.LeftTransferToTrayAsync([.. request.SamplingTaskInfos.Select(p =>
                 {
                     return new SampleTaskInfo
                     {
                         SamplingId = p.SampleId,
                         PlatformId = request.PlatformId,
                         SampleName = p.SampleName,
                         SampleRemarks = p.SampleRemarks,
                         SampleType = p.SampleType,
                         WellId = p.WellId,
                     };
                 })], context.CancellationToken);
                return await Task.FromResult(new TransferTrayMoveResponse
                {
                    PlatformId = request.PlatformId,
                    TransferModuleId = transferCallService.TransferId,
                    Status = new Tangerine.Grpc.Foundation.Status
                    {
                        Message = callStatus.Message,
                        Code = callStatus.Code
                    }
                });
            }
            else if (transferCallService.IsRightPlatform(request.PlatformId))
            {
                var callStatus = await transferCallService.RightTransferToTrayAsync([.. request.SamplingTaskInfos.Select(p =>
                {
                    return new SampleTaskInfo
                    {
                        SamplingId = p.SampleId,
                        PlatformId = request.PlatformId,
                        SampleName = p.SampleName,
                        SampleRemarks = p.SampleRemarks,
                        SampleType = p.SampleType,
                        WellId = p.WellId,
                       WellName = p.WellName,
                    };
                })], context.CancellationToken);

                return await Task.FromResult(new TransferTrayMoveResponse
                {
                    PlatformId = request.PlatformId,
                    TransferModuleId = transferCallService.TransferId,
                    Status = new Tangerine.Grpc.Foundation.Status
                    {
                        Message = callStatus.Message,
                        Code = callStatus.Code
                    }
                });
            }

            return await Task.FromResult(new TransferTrayMoveResponse
            {
                PlatformId = request.PlatformId,
                TransferModuleId = transferCallService.TransferId,
                Status = new Tangerine.Grpc.Foundation.Status
                {
                    Message = "Platform not found",
                    Code = -1
                }
            });
        }

        public override async Task<TransferTrayMoveResponse> TrayToTransfer(TransferTrayMoveRequest request, ServerCallContext context)
        {
            var transferCallService = _appService.FindTransferCallService(request.TransferModuleId);
            if (transferCallService.IsLeftPlatform(request.PlatformId))
            {
                var callStatus = await transferCallService.LeftTrayToTransferAsync([.. request.SamplingTaskInfos.Select(p =>
                {
                    return new SampleTaskInfo
                    {
                        SamplingId = p.SampleId,
                        PlatformId = request.PlatformId,
                        SampleName = p.SampleName,
                        SampleRemarks = p.SampleRemarks,
                        SampleType = p.SampleType,
                        WellId = p.WellId,
                        TrayId = p.TrayId,
                        WellName = p.WellName,
                    };
                })], context.CancellationToken);

                return await Task.FromResult(new TransferTrayMoveResponse
                {
                    PlatformId = request.PlatformId,
                    TransferModuleId = transferCallService.TransferId,
                    Status = new Tangerine.Grpc.Foundation.Status
                    {
                        Message = callStatus.Message,
                        Code = callStatus.Code,
                    }
                });
            }
            else if (transferCallService.IsRightPlatform(request.PlatformId))
            {
                var callStatus = await transferCallService.RightTrayToTransferAsync([.. request.SamplingTaskInfos.Select(p =>
                {
                    return new SampleTaskInfo
                    {
                        SamplingId = p.SampleId,
                        PlatformId = request.PlatformId,
                        SampleName = p.SampleName,
                        SampleRemarks = p.SampleRemarks,
                        SampleType = p.SampleType,
                        WellId = p.WellId,
                       TrayId= p.TrayId,
                        WellName = p.WellName,
                    };
                })], context.CancellationToken);

                return await Task.FromResult(new TransferTrayMoveResponse
                {
                    PlatformId = request.PlatformId,
                    TransferModuleId = transferCallService.TransferId,
                    Status = new Tangerine.Grpc.Foundation.Status
                    {
                        Message = callStatus.Message,
                        Code = callStatus.Code,
                    }
                });
            }
            else
            {
                return await Task.FromResult(new TransferTrayMoveResponse
                {
                    PlatformId = request.PlatformId,
                    TransferModuleId = transferCallService.TransferId,
                    Status = new Tangerine.Grpc.Foundation.Status
                    {
                        Message = "Platform not found",
                        Code = -1
                    }
                });
            }
        }

        public override async Task<LoadTransferToUnloadTransferResponse> LoadTransferToUnloadTransfer(LoadTransferToUnloadTransferRequest request, ServerCallContext context)
        {
            var unloadtransferCallService = _appService.FindTransferCallService(request.UnloadTransferModuleId);
            var loadtransferCallService = _appService.FindTransferCallService(request.LoadTransferModuleId);
            var callStatus = await TransferCallService.LoadTransferToUnloadTransfer(loadtransferCallService, unloadtransferCallService, [.. request.SamplingTaskInfos.Select(p =>
            {
                return new SampleTaskInfo
                {
                    SamplingId = p.SampleId,
                    SampleName = p.SampleName,
                    SampleRemarks = p.SampleRemarks,
                    SampleType = p.SampleType,
                    WellId = p.WellId,
                   WellName = p.WellName,
                };
            })], context.CancellationToken);
            return await Task.FromResult(new LoadTransferToUnloadTransferResponse
            {
                LoadTransferModuleId = request.LoadTransferModuleId,
                UnloadTransferModuleId = request.UnloadTransferModuleId,
                Status = new Tangerine.Grpc.Foundation.Status
                {
                    Message = callStatus.Message,
                    Code = callStatus.Code,
                }
            });
        }

        public override async Task<UnloadTransferToLoadTransferResponse> UnloadTransferToLoadTransfer(UnloadTransferToLoadTransferRequest request, ServerCallContext context)
        {
            var unloadtransferCallService = _appService.FindTransferCallService(request.UnloadTransferModuleId);
            var loadtransferCallService = _appService.FindTransferCallService(request.LoadTransferModuleId);
            var callStatus = await TransferCallService.UnloadTransferToLoadTransfer(loadtransferCallService, unloadtransferCallService, [.. request.SamplingTaskInfos.Select(p =>
            {
                return new SampleTaskInfo
                {
                    SamplingId = p.SampleId,
                    SampleName = p.SampleName,
                    SampleRemarks = p.SampleRemarks,
                    SampleType = p.SampleType,
                    WellId = p.WellId,
                   WellName = p.WellName,
                };
            })], context.CancellationToken);
            return await Task.FromResult(new UnloadTransferToLoadTransferResponse
            {
                LoadTransferModuleId = request.LoadTransferModuleId,
                UnloadTransferModuleId = request.UnloadTransferModuleId,
                Status = new Tangerine.Grpc.Foundation.Status
                {
                    Code = callStatus.Code,
                    Message = callStatus.Message
                }
            });
        }
    }
}


