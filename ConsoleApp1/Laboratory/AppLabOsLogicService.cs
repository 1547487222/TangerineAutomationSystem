using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using QStandaedPlatform.Engine.Common.Common;
using QStandaedPlatform.Engine.Laboratory.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Laboratory
{
    public class AppLabOsLogicService
    {
        private readonly List<ProductionLineInfo> _productionLines = [];
        private readonly Dictionary<long, PlatformCallService> _platformCallServices = [];
        private readonly Dictionary<long, TransferCallService> _transferCallServices = [];
        private readonly object _lock = new();
        private readonly ILogger<AppLabOsLogicService> _logger;
        public AppLabOsLogicService()
        {
            _logger = LoggerProviderManager.GetLoggerFactory().CreateLogger<AppLabOsLogicService>();
        }
        public List<ProductionLineInfo> ProductionLines => _productionLines;

        public LaboratoryInfo Laboratory { get; set; } = new();

        public void Configure(GrpcProjectOptions grpcProjectOptions)
        {
            lock (_lock)
            {
                Laboratory.LaboratoryId = grpcProjectOptions.LaboratoryId;
                Laboratory.LaboratoryCode = grpcProjectOptions.LaboratoryCode;
                Laboratory.LaboratoryName = grpcProjectOptions.LaboratoryName;
                Laboratory.LaboratoryDescription = grpcProjectOptions.LaboratoryDescription;
                foreach (var grpcProjectLineOptions in grpcProjectOptions.GrpcProjectProductlineOptions)
                {
                    var productionLine = new ProductionLineInfo
                    {
                        LineId = grpcProjectLineOptions.ProductlineId,
                        LineCode = grpcProjectLineOptions.ProductlineCode,
                        LineDescription = grpcProjectLineOptions.ProductlineDescription,
                        LineName = grpcProjectLineOptions.ProductlineName,
                    };
                    productionLine.ConfigureProcessflow(grpcProjectOptions);
                    _productionLines.Add(productionLine);

                    foreach (var platform in productionLine.Platforms)
                    {
                        var platformCallService = new PlatformCallService(platform);
                        platformCallService.InitializeConfiguration();
                        _platformCallServices.Add(platform.PlatformId, platformCallService);
                    }

                    foreach (var transferModule in productionLine.TransferModules)
                    {
                        if (_platformCallServices.TryGetValue(transferModule.LeftPlatformId, out var leftPlatformCallService))
                        {
                            if (leftPlatformCallService != null)
                            {
                                transferModule.LeftPlatformCallService = leftPlatformCallService;
                                _logger.LogInformation($"中转模块{transferModule.TransferModuleId}的左平台调用服务{transferModule.LeftPlatformId}已配置");
                            }
                        }
                        if (_platformCallServices.TryGetValue(transferModule.RightPlatformId, out var rightPlatformCallService))
                        {
                            if (rightPlatformCallService != null)
                            {
                                transferModule.RightPlatformCallService = rightPlatformCallService;
                                _logger.LogInformation($"中转模块{transferModule.TransferModuleId}的右平台调用服务{transferModule.RightPlatformId}已配置");
                            }
                        }
                        var transferCallService = new TransferCallService(transferModule);
                        transferCallService.InitializeConfiguration();
                        _transferCallServices.Add(transferModule.TransferModuleId, transferCallService);
                    }
                }
            }
        }

        public void AddProductionLine(ProductionLineInfo productionLine)
        {
            _productionLines.Add(productionLine);
        }

        public ProductionLineInfo FindProductionLine(long lineId)
        {
            return _productionLines.First(p => p.LineId == lineId);
        }
        public ProcessflowInfo FindProcessflow(Guid processId)
        {
            return _productionLines.SelectMany(p => p.Processflows).First(p => p.ProcessId == processId);
        }

        public PlatformInfo FindPlatform(long platformId)
        {
            return _productionLines.SelectMany(p => p.Platforms).First(p => p.PlatformId == platformId);
        }

        public PlatformCallService[] FindPlatformCallServices()
        {
            lock (_lock)
            {
                return [.. _platformCallServices.Values];
            }
        }

        public PlatformCallService FindPlatformCallService(long platformId)
        {
            lock (_lock)
            {
                if (!_platformCallServices.TryGetValue(platformId, out PlatformCallService? value))
                {
                    throw new Exception($"未找到平台{platformId}的PlatformCallService");
                }
                return value;
            }
        }

        public TransferCallService[] FindTransferCallServices()
        {
            lock (_lock)
            {
                return [.. _transferCallServices.Values];
            }
        }

        public TransferCallService FindTransferCallService(long transferId)
        {
            lock (_lock) 
            {
                if (!_transferCallServices.TryGetValue(transferId, out TransferCallService? value))
                {
                    throw new Exception($"未找到中转模块{transferId}的TransferCallService");
                }
                return value;
            }
        }

        public void Initialize()
        {
            WorkFlowEngine.Instance.Initialize();
        }

        public void Shutdown()
        {
            WorkFlowEngine.Instance.ShutDown();
        }
    }
    public class LaboratoryInfo
    {
        public long LaboratoryId { get; set; }

        public string LaboratoryCode { get; set; } = string.Empty;

        public string LaboratoryName { get; set; } = string.Empty;

        public string LaboratoryDescription { get; set; } = string.Empty;

    }

    public class ProductionLineInfo
    {
        
        private readonly ILogger<ProductionLineInfo> _logger;

        public ProductionLineInfo()
        {
            _logger = LoggerProviderManager.GetLoggerFactory().CreateLogger<ProductionLineInfo>();
        }
        public long LineId { get; set; }

        public string LineName { get; set; } = string.Empty;

        public string LineCode { get; set; } = string.Empty;

        public string LineDescription { get; set; } = string.Empty;

        public List<ProcessflowInfo> Processflows { get; set; } = [];

        public List<PlatformInfo> Platforms { get; set; } = [];

        public List<TransferModuleInfo> TransferModules { get; set; } = [];

        public void ConfigureProcessflow(GrpcProjectOptions grpcProjectOptions)
        {
            var processflowIds = grpcProjectOptions.GrpcProjectProductlineOptions.First(p => p.ProductlineId == LineId).ProcessflowIds;
            foreach (var processflowId in processflowIds)
            {
                var grpcProjectProcessflowOptions = grpcProjectOptions.GrpcProjectProcessflowOptions.First(p => p.ProcessId == processflowId);
                var processflow = new ProcessflowInfo
                {
                    ProcessId = grpcProjectProcessflowOptions.ProcessId,
                    ProcessCode = grpcProjectProcessflowOptions.ProcessCode,
                    ProcessName = grpcProjectProcessflowOptions.ProcessName,
                    ProcessType = grpcProjectProcessflowOptions.ProcessStep,
                    ProcessDescription = grpcProjectProcessflowOptions.ProcessDescription,
                };

                foreach (var platformsInOrder in grpcProjectProcessflowOptions.PlatformFlowSteps)
                {
                    var platformFlowConfigOptions = grpcProjectOptions.GrpcProjectPlatformOptions.First(p => p.PlatformId == platformsInOrder.PlatformId).TaskFlowConfigs.First(p => p.FlowTaskId == platformsInOrder.FlowTaskId);
                    processflow.PlatformTasks.Add(new PlatformTaskInfo
                    {
                        FlowConfigPath = platformFlowConfigOptions.FlowConfigPath,
                        FlowId = platformFlowConfigOptions.FlowId,
                        PlatformId = platformFlowConfigOptions.PlatformId,
                        PlatformTaskId = platformFlowConfigOptions.FlowTaskId,
                        PlatformTaskCode = platformFlowConfigOptions.FlowTaskCode,
                        PlatformTaskDescription = platformFlowConfigOptions.FlowTaskDescription,
                        StepOrder = platformsInOrder.StepOrder,
                        TaskEbrParameterConfigs = platformFlowConfigOptions.TaskEbrParameterConfigs,
                        ActionConfigs = [.. platformFlowConfigOptions.ActionConfigs.Select(p => (SequentialActionConfig)p.Clone())],
                    });
                }

                // Load transfer steps
                foreach (var transferStep in grpcProjectProcessflowOptions.TransferSteps)
                {
                    processflow.TransferSteps.Add(new TransferStepInfo
                    {
                        StepId = transferStep.StepId,
                        StepOrder = transferStep.StepOrder,
                        StepDescription = transferStep.StepDescription,
                        TransferModuleId = transferStep.TransferModuleId,
                        TransferDirection = transferStep.TransferDirection,
                        SourcePlatformId = transferStep.SourcePlatformId,
                        TargetPlatformId = transferStep.TargetPlatformId,
                    });
                }

                // Load module action steps
                foreach (var moduleActionStep in grpcProjectProcessflowOptions.ModuleActionSteps)
                {
                    processflow.ModuleActionSteps.Add(new ModuleActionStepInfo
                    {
                        StepId = moduleActionStep.StepId,
                        StepOrder = moduleActionStep.StepOrder,
                        StepDescription = moduleActionStep.StepDescription,
                        ModuleName = moduleActionStep.ModuleName,
                        ModuleSerialNumber = moduleActionStep.ModuleSerialNumber,
                        ModuleActionId = moduleActionStep.ModuleActionId,
                        ActionName = moduleActionStep.ActionName,
                        ActionDescription = moduleActionStep.ActionDescription,
                        ActionParameters = [.. moduleActionStep.ActionParameters],
                    });
                }

                Processflows.Add(processflow);
            }

            foreach (var platformOptions in grpcProjectOptions.GrpcProjectPlatformOptions)
            {
                var platform = new PlatformInfo();

                platform.ConfigurePlatform(platformOptions);
                Platforms.Add(platform);
            }

            foreach (var transferModuleOptions in grpcProjectOptions.GrpcProjectTransferModuleOptions)
            {
                var transferModule = new TransferModuleInfo()
                {
                    TransferModuleId = transferModuleOptions.TransferModuleId,
                    LeftChannelGroupId = transferModuleOptions.LeftChannelGroupId,
                    LeftChannelGroup = ParameterTableManager.ModuleChannelGroupTable.ModuleChannels.FirstOrDefault(p => p.ParameterId == transferModuleOptions.LeftChannelGroupId),
                    RightChannelGroupId = transferModuleOptions.RightChannelGroupId,
                    RightChannelGroup = ParameterTableManager.ModuleChannelGroupTable.ModuleChannels.FirstOrDefault(p => p.ParameterId == transferModuleOptions.RightChannelGroupId),
                    LeftPlatformId =  transferModuleOptions.LeftPlatformId,
                    RightPlatformId = transferModuleOptions.RightPlatformId,
                    LeftPlatformInfo = Platforms.First(p => p.PlatformId == transferModuleOptions.LeftPlatformId),
                    RightPlatformInfo = Platforms.First(p => p.PlatformId == transferModuleOptions.RightPlatformId),
                    TransferModuleInfoId = transferModuleOptions.TransferModuleInfoId,
                    TransferModuleParameter = ParameterTableManager.ModuleInfoTable.ModuleInfoParameters.FirstOrDefault(p => p.ModuleInfoId == transferModuleOptions.TransferModuleInfoId),
                    TransferForwardMoveId = transferModuleOptions.TransferForwardMoveId,
                    TransferForwardMoveParameter = ParameterTableManager.ModuleFuncCodeTable.ModuleFuncCodeParameters.FirstOrDefault(p => p.ParameterId == transferModuleOptions.TransferForwardMoveId),
                    TransferBackwardMoveId = transferModuleOptions.TransferBackwardMoveId,
                    TransferBackwardMoveParameter = ParameterTableManager.ModuleFuncCodeTable.ModuleFuncCodeParameters.FirstOrDefault(p => p.ParameterId == transferModuleOptions.TransferBackwardMoveId),
                    IsReverse = transferModuleOptions.IsReverse,
                    TransferModuleName = transferModuleOptions.TransferModuleName,
                    TransferModuleSamplingFlux = transferModuleOptions.TransferModuleSamplingFlux,
                };
                foreach (var labTray in transferModule.LeftPlatformInfo.LabTrayConfigs.Select(p=> ParameterTableManager.LabTrayTable.LabTrayParameters.First(x=>p.LabTrayId==x.LabTrayId)).ToList())
                {
                    if (labTray.Regions.Any(p => p.TrayRegionType.HasFlag(LabTrayDefaultType.LoadTray)))
                    {
                        _logger.LogInformation($"Add left platform{transferModule.LeftPlatformId} load lab tray{labTray.LabTrayId}");
                        transferModule.LeftPlatformLoadLabTrays.Add(labTray);
                    }
                    if (labTray.Regions.Any(p => p.TrayRegionType.HasFlag(LabTrayDefaultType.UnloadTray)))
                    {
                        _logger.LogInformation($"Add left platform{transferModule.LeftPlatformId} unload lab tray{labTray.LabTrayId}");
                        transferModule.LeftPlatformUnloadLabTrays.Add(labTray);
                    }
                }
                foreach (var labTray in transferModule.RightPlatformInfo.LabTrayConfigs.Select(p => ParameterTableManager.LabTrayTable.LabTrayParameters.First(x => p.LabTrayId == x.LabTrayId)).ToList())
                {
                    if (labTray.Regions.Any(p => p.TrayRegionType.HasFlag(LabTrayDefaultType.LoadTray)))
                    {
                        _logger.LogInformation($"Add right platform{transferModule.RightPlatformId} load lab tray{labTray.LabTrayId}");
                        transferModule.RightPlatformLoadLabTrays.Add(labTray);
                    }
                    if (labTray.Regions.Any(p => p.TrayRegionType.HasFlag(LabTrayDefaultType.UnloadTray)))
                    {
                        _logger.LogInformation($"Add right platform{transferModule.RightPlatformId} unload lab tray{labTray.LabTrayId}");
                        transferModule.RightPlatformUnloadLabTrays.Add(labTray);
                    }
                }
                TransferModules.Add(transferModule);
            }
        }
    }

    public class ProcessflowInfo
    {
        /// <summary>
        /// 工艺流程Id
        /// </summary>
        public Guid ProcessId { get; set; }
        /// <summary>
        /// 工艺流程编号
        /// </summary>
        public string ProcessCode { get; set; } = string.Empty;
        /// <summary>
        /// 工艺流程名称
        /// </summary>
        public string ProcessName { get; set; } = string.Empty;

        /// <summary>
        /// 工艺流程描述
        /// </summary>
        public string ProcessDescription { get; set; } = string.Empty;

        /// <summary>
        /// 工艺流程类型
        /// </summary>
        public ProcessStep ProcessType { get; set; } = ProcessStep.Pretreatment;

        /// <summary>
        /// 工艺流程的平台任务集合
        /// </summary>
        public List<PlatformTaskInfo> PlatformTasks { get; set; } = [];

        /// <summary>
        /// 工艺流程的中转步骤集合
        /// </summary>
        public List<TransferStepInfo> TransferSteps { get; set; } = [];

        /// <summary>
        /// 工艺流程的模块动作步骤集合
        /// </summary>
        public List<ModuleActionStepInfo> ModuleActionSteps { get; set; } = [];

        public PlatformTaskInfo GetPlatform(long platformId)
        {
            return PlatformTasks.First(p => p.PlatformId == platformId);
        }
    }

    /// <summary>
    /// 中转步骤信息
    /// </summary>
    public class TransferStepInfo
    {
        /// <summary>
        /// 步骤Id
        /// </summary>
        public long StepId { get; set; }
        /// <summary>
        /// 步骤执行顺序
        /// </summary>
        public int StepOrder { get; set; }
        /// <summary>
        /// 步骤描述
        /// </summary>
        public string StepDescription { get; set; } = string.Empty;
        /// <summary>
        /// 关联的中转模块Id
        /// </summary>
        public long TransferModuleId { get; set; }
        /// <summary>
        /// 中转方向
        /// </summary>
        public TransferDirection TransferDirection { get; set; } = TransferDirection.Forward;
        /// <summary>
        /// 源平台Id
        /// </summary>
        public long SourcePlatformId { get; set; }
        /// <summary>
        /// 目标平台Id
        /// </summary>
        public long TargetPlatformId { get; set; }
    }

    /// <summary>
    /// 模块动作步骤信息
    /// </summary>
    public class ModuleActionStepInfo
    {
        /// <summary>
        /// 步骤Id
        /// </summary>
        public long StepId { get; set; }
        /// <summary>
        /// 步骤执行顺序
        /// </summary>
        public int StepOrder { get; set; }
        /// <summary>
        /// 步骤描述
        /// </summary>
        public string StepDescription { get; set; } = string.Empty;
        /// <summary>
        /// 模块名称
        /// </summary>
        public string ModuleName { get; set; } = string.Empty;
        /// <summary>
        /// 模块序列号
        /// </summary>
        public string ModuleSerialNumber { get; set; } = string.Empty;
        /// <summary>
        /// 模块动作Id
        /// </summary>
        public Guid ModuleActionId { get; set; }
        /// <summary>
        /// 动作名称
        /// </summary>
        public string ActionName { get; set; } = string.Empty;
        /// <summary>
        /// 动作描述
        /// </summary>
        public string ActionDescription { get; set; } = string.Empty;
        /// <summary>
        /// 动作参数配置
        /// </summary>
        public List<ParameterItem> ActionParameters { get; set; } = [];
    }


    public class PlatformInfo
    {
        /// <summary>
        /// 平台Id
        /// </summary>
        public long PlatformId { get; set; }
        /// <summary>
        /// 平台名称
        /// </summary>
        public string PlatformName { get; set; } = string.Empty;
        /// <summary>
        /// 平台编号
        /// </summary>
        public string PlatformCode { get; set; } = string.Empty;

        /// <summary>
        /// 平台进样通量
        /// </summary>
        public int PlatformSamplingFlux { get; set; } = 2;
        /// <summary>
        /// 平台最大执行次数
        /// </summary>
        public int PlatformMaxExecuteCount { get; set; }
        /// <summary>
        /// 平台最大缓存数
        /// </summary>
        public int PlatformMaxCacheCount { get; set; }
        /// <summary>
        /// 平台模块Id
        /// </summary>
        public Guid PlatformModuleId { get; set; }

        public ModuleInfoParameter? PlatformModuleInfo { get; set; }
        /// <summary>
        /// 平台抓取动作ID
        /// </summary>
        public Guid PlatformGrabActionId { get; set; }

        public ModuleFuncCodeParameter? PlatformGrabActionParameter { get; set; }

        /// <summary>
        /// 平台描述
        /// </summary>
        public string PlatformDescription { get; set; } = string.Empty;
        /// <summary>
        /// 平台监控项配置
        /// </summary>
        public PlatformMonitorOptions PlatformMonitorOptions { get; set; } = new PlatformMonitorOptions();
        /// <summary>
        /// 平台托盘配置
        /// </summary>
        public List<LabTrayConfiguration> LabTrayConfigs { get; set; } = [];

        /// <summary>
        /// 初始化流程
        /// </summary>
        public List<PlatformTaskInfo> InitialInfo { get; set; } = [];
        /// <summary>
        /// 实验前准备流程
        /// </summary>
        public List<PlatformTaskInfo> PrepareExperimentInfo { get; set; } = [];
        /// <summary>
        /// 管路排空
        /// </summary>
        public List<PlatformTaskInfo> LineDrainageInfo { get; set; } = [];
        /// <summary>
        /// 仪器维护流程
        /// </summary>
        public List<PlatformTaskInfo> MaintenanceInfo { get; set; } = [];
        /// <summary>
        /// 实验流程
        /// </summary>
        public List<PlatformTaskInfo> TaskInfo { get; set; } = [];
        /// <summary>
        /// 系统封存流程
        /// </summary>
        public List<PlatformTaskInfo> SystemStorageInfo { get; set; } = [];
        /// <summary>
        /// 实验收尾流程
        /// </summary>
        public List<PlatformTaskInfo> FinalizeInfo { get; set; } = [];

        /// <summary>
        /// 配置平台
        /// </summary>
        /// <param name="pfatformProjectOption"></param>
        public void ConfigurePlatform(GrpcProjectPlatformOptions grpcProjectPlatformOptions)
        {
            PlatformName = grpcProjectPlatformOptions.PlatformName;
            PlatformCode = grpcProjectPlatformOptions.PlatformCode;
            PlatformDescription = grpcProjectPlatformOptions.PlatformDescription;
            PlatformId = grpcProjectPlatformOptions.PlatformId;
            PlatformModuleId = grpcProjectPlatformOptions.PlatformModuleId;
            PlatformModuleInfo= ParameterTableManager.ModuleInfoTable.ModuleInfoParameters.FirstOrDefault(p => p.ModuleInfoId == grpcProjectPlatformOptions.PlatformModuleId);
            PlatformGrabActionId = grpcProjectPlatformOptions.PlatformGrabActionId;
            PlatformGrabActionParameter = ParameterTableManager.ModuleFuncCodeTable.ModuleFuncCodeParameters.FirstOrDefault(p => p.ParameterId == grpcProjectPlatformOptions.PlatformGrabActionId);
            PlatformSamplingFlux = grpcProjectPlatformOptions.PlatformSamplingFlux;
            PlatformMaxCacheCount = grpcProjectPlatformOptions.PlatformMaxCacheCount;
            PlatformMaxExecuteCount = grpcProjectPlatformOptions.PlatformMaxExecuteCount;

            if (grpcProjectPlatformOptions.PlatformMonitorItems.Count > 0)
            {
                PlatformMonitorOptions.Items.AddRange(grpcProjectPlatformOptions.PlatformMonitorItems);
            }
            if (grpcProjectPlatformOptions.LabTrayConfigs.Count > 0)
            {
                LabTrayConfigs.AddRange(grpcProjectPlatformOptions.LabTrayConfigs);
            }
            if (grpcProjectPlatformOptions.InitialFlowConfigs.Count != 0)
            {
                foreach (var platformFlowConfigOptions in grpcProjectPlatformOptions.InitialFlowConfigs)
                {
                    InitialInfo.Add(new PlatformTaskInfo
                    {
                        FlowConfigPath = platformFlowConfigOptions.FlowConfigPath,
                        FlowId = platformFlowConfigOptions.FlowId,
                        PlatformId = platformFlowConfigOptions.PlatformId,
                        PlatformTaskId = platformFlowConfigOptions.FlowTaskId,
                        PlatformTaskCode = platformFlowConfigOptions.FlowTaskCode,
                        PlatformTaskDescription = platformFlowConfigOptions.FlowTaskDescription,
                        ActionConfigs = [.. platformFlowConfigOptions.ActionConfigs.Select(p => (SequentialActionConfig)p.Clone())],
                        TaskEbrParameterConfigs = platformFlowConfigOptions.TaskEbrParameterConfigs,
                    });
                }

            }
            if (grpcProjectPlatformOptions.PrepareFlowConfigs.Count != 0)
            {
                foreach (var platformFlowConfigOptions in grpcProjectPlatformOptions.PrepareFlowConfigs)
                {
                    PrepareExperimentInfo.Add(new PlatformTaskInfo
                    {
                        FlowConfigPath = platformFlowConfigOptions.FlowConfigPath,
                        FlowId = platformFlowConfigOptions.FlowId,
                        PlatformId = platformFlowConfigOptions.PlatformId,
                        PlatformTaskId = platformFlowConfigOptions.FlowTaskId,
                        PlatformTaskCode = platformFlowConfigOptions.FlowTaskCode,
                        PlatformTaskDescription = platformFlowConfigOptions.FlowTaskDescription,
                        ActionConfigs = platformFlowConfigOptions.ActionConfigs.Select(p => (SequentialActionConfig)p.Clone()).ToList(),
                        TaskEbrParameterConfigs = platformFlowConfigOptions.TaskEbrParameterConfigs,
                    });
                }
            }
            if (grpcProjectPlatformOptions.TaskFlowConfigs.Count != 0)
            {
                foreach (var platformFlowConfigOptions in grpcProjectPlatformOptions.TaskFlowConfigs)
                {
                    TaskInfo.Add(new PlatformTaskInfo
                    {
                        FlowConfigPath = platformFlowConfigOptions.FlowConfigPath,
                        FlowId = platformFlowConfigOptions.FlowId,
                        PlatformId = platformFlowConfigOptions.PlatformId,
                        PlatformTaskId = platformFlowConfigOptions.FlowTaskId,
                        PlatformTaskCode = platformFlowConfigOptions.FlowTaskCode,
                        PlatformTaskDescription = platformFlowConfigOptions.FlowTaskDescription,
                        ActionConfigs = platformFlowConfigOptions.ActionConfigs.Select(p => (SequentialActionConfig)p.Clone()).ToList(),
                        TaskEbrParameterConfigs = platformFlowConfigOptions.TaskEbrParameterConfigs,
                    });
                }
            }
            if (grpcProjectPlatformOptions.FinalizeFlowConfigs.Count != 0)
            {
                foreach (var platformFlowConfigOptions in grpcProjectPlatformOptions.FinalizeFlowConfigs)
                {
                    FinalizeInfo.Add(new PlatformTaskInfo
                    {
                        FlowConfigPath = platformFlowConfigOptions.FlowConfigPath,
                        FlowId = platformFlowConfigOptions.FlowId,
                        PlatformId = platformFlowConfigOptions.PlatformId,
                        PlatformTaskId = platformFlowConfigOptions.FlowTaskId,
                        PlatformTaskCode = platformFlowConfigOptions.FlowTaskCode,
                        PlatformTaskDescription = platformFlowConfigOptions.FlowTaskDescription,
                        ActionConfigs = platformFlowConfigOptions.ActionConfigs.Select(p => (SequentialActionConfig)p.Clone()).ToList(),
                        TaskEbrParameterConfigs = platformFlowConfigOptions.TaskEbrParameterConfigs,
                    });
                }
            }
            if (grpcProjectPlatformOptions.MaintainFlowConfigs.Count != 0)
            {
                foreach (var platformFlowConfigOptions in grpcProjectPlatformOptions.MaintainFlowConfigs)
                {
                    MaintenanceInfo.Add(new PlatformTaskInfo
                    {
                        FlowConfigPath = platformFlowConfigOptions.FlowConfigPath,
                        FlowId = platformFlowConfigOptions.FlowId,
                        PlatformId = platformFlowConfigOptions.PlatformId,
                        PlatformTaskId = platformFlowConfigOptions.FlowTaskId,
                        PlatformTaskCode = platformFlowConfigOptions.FlowTaskCode,
                        PlatformTaskDescription = platformFlowConfigOptions.FlowTaskDescription,
                        ActionConfigs = platformFlowConfigOptions.ActionConfigs.Select(p => (SequentialActionConfig)p.Clone()).ToList(),
                        TaskEbrParameterConfigs = platformFlowConfigOptions.TaskEbrParameterConfigs,
                    });
                }
            }
            if (grpcProjectPlatformOptions.LineDrainageFlowConfigs.Count != 0)
            {
                foreach (var platformFlowConfigOptions in grpcProjectPlatformOptions.MaintainFlowConfigs)
                {
                    LineDrainageInfo.Add(new PlatformTaskInfo
                    {
                        FlowConfigPath = platformFlowConfigOptions.FlowConfigPath,
                        FlowId = platformFlowConfigOptions.FlowId,
                        PlatformId = platformFlowConfigOptions.PlatformId,
                        PlatformTaskId = platformFlowConfigOptions.FlowTaskId,
                        PlatformTaskCode = platformFlowConfigOptions.FlowTaskCode,
                        PlatformTaskDescription = platformFlowConfigOptions.FlowTaskDescription,
                        ActionConfigs = [.. platformFlowConfigOptions.ActionConfigs.Select(p => (SequentialActionConfig)p.Clone())],
                        TaskEbrParameterConfigs = platformFlowConfigOptions.TaskEbrParameterConfigs,
                    });
                }
            }
            if (grpcProjectPlatformOptions.SystemStorageFlowConfigs.Count != 0)
            {
                foreach (var platformFlowConfigOptions in grpcProjectPlatformOptions.SystemStorageFlowConfigs)
                {
                    SystemStorageInfo.Add(new PlatformTaskInfo
                    {
                        FlowConfigPath = platformFlowConfigOptions.FlowConfigPath,
                        FlowId = platformFlowConfigOptions.FlowId,
                        PlatformId = platformFlowConfigOptions.PlatformId,
                        PlatformTaskId = platformFlowConfigOptions.FlowTaskId,
                        PlatformTaskCode = platformFlowConfigOptions.FlowTaskCode,
                        PlatformTaskDescription = platformFlowConfigOptions.FlowTaskDescription,
                        ActionConfigs = [.. platformFlowConfigOptions.ActionConfigs.Select(p => (SequentialActionConfig)p.Clone())],
                        TaskEbrParameterConfigs = platformFlowConfigOptions.TaskEbrParameterConfigs,
                    });
                }
            }
        }
    }


    public class TransferModuleInfo
    {
        /// <summary>
        /// 中转模块ID
        /// </summary>
        public long TransferModuleId { get; set; }
        /// <summary>
        /// 中转模块名称
        /// </summary>
        public string TransferModuleName { get; set; } = string.Empty;
        /// <summary>
        /// 左关联平台ID
        /// </summary>
        public long LeftPlatformId { get; set; }

        /// <summary>
        /// 左关联平台调用服务
        /// </summary>
        public PlatformCallService? LeftPlatformCallService { get; set; }

        /// <summary>
        /// 左关联平台上料托盘集合
        /// </summary>
        public List<LabTray> LeftPlatformLoadLabTrays { get; set; } = [];
        /// <summary>
        /// 左关联平台下料托盘集合
        /// </summary>
        public List<LabTray> LeftPlatformUnloadLabTrays { get; set; } = [];

        /// <summary>
        /// 左关联平台信息
        /// </summary>
        public PlatformInfo? LeftPlatformInfo { get; set; }
        /// <summary>
        /// 右关联平台ID
        /// </summary>
        public long RightPlatformId { get; set; }

        /// <summary>
        /// 右关联平台调用服务
        /// </summary>
        public PlatformCallService? RightPlatformCallService { get; set; }

        /// <summary>
        /// 右关联平台上料托盘集合
        /// </summary>
        public List<LabTray> RightPlatformLoadLabTrays { get; set; } = [];

        /// <summary>
        /// 右关联平台下料托盘集合
        /// </summary>
        public List<LabTray> RightPlatformUnloadLabTrays { get; set; } = [];

        /// <summary>
        /// 右关联平台信息
        /// </summary>
        public PlatformInfo? RightPlatformInfo { get; set; }
        /// <summary>
        /// 中转进样通量
        /// </summary>
        public int TransferModuleSamplingFlux { get; set; }

        /// <summary>
        /// 中转信息id
        /// </summary>
        public Guid TransferModuleInfoId { get; set; }

        /// <summary>
        /// 中转模块信息
        /// </summary>
        public ModuleInfoParameter? TransferModuleParameter { get; set; }

        /// <summary>
        /// 中转左平台通道组id
        /// </summary>
        public Guid LeftChannelGroupId { get; set; }

        /// <summary>
        /// 中转左平台通道组
        /// </summary>
        public ModuleChannelGroup? LeftChannelGroup { get; set; }

        /// <summary>
        /// 中转右平台通道组id
        /// </summary>
        public Guid RightChannelGroupId { get; set; }

        /// <summary>
        /// 中转右平台通道组
        /// </summary>
        public ModuleChannelGroup? RightChannelGroup { get; set; }

        /// <summary>
        /// 正向移动
        /// </summary>
        public Guid TransferForwardMoveId { get; set; }

        /// <summary>
        /// 正向移动参数
        /// </summary>
        public ModuleFuncCodeParameter? TransferForwardMoveParameter { get; set; }

        /// <summary>
        /// 反向移动
        ///</summary>
        public Guid TransferBackwardMoveId { get; set; }

        /// <summary>
        /// 反向移动参数
        /// </summary>
        public ModuleFuncCodeParameter? TransferBackwardMoveParameter { get; set; }

        /// <summary>
        /// 是否反转
        /// </summary>
        public bool IsReverse { get; set; } = false;


    }

    public class PlatformTaskInfo
    {
        public Guid FlowId { get; set; }
        public long PlatformId { get; set; }
        public long PlatformTaskId { get; set; }
        public string PlatformTaskCode { get; set; } = string.Empty;
        public string PlatformTaskDescription { get; set; } = string.Empty;
        public string FlowConfigPath { get; set; } = string.Empty;
        public int StepOrder { get; set; }
        public List<SequentialActionConfig> ActionConfigs { get; set; } = [];
        public List<EbrParameterConfig> TaskEbrParameterConfigs { get; set; } = [];
    }
}
