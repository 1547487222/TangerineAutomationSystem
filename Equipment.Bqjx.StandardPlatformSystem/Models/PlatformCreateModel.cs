using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Equipment.Bqjx.StandardPlatformSystem.Views;
using Equipment.Bqjx.StandardPlatformSystem.WorkFlows;
using QStandaedPlatform.Engine.Common.Common;
using QStandaedPlatform.Engine.Components.Modbus;
using QStandaedPlatform.Engine.Extensions;
using QStandaedPlatform.Engine.Laboratory;
using QStandaedPlatform.Engine.Laboratory.Documents;
using System.Collections.ObjectModel;

namespace Equipment.Bqjx.StandardPlatformSystem.Models
{
    public partial class GrpcProjectflowStepModel : ObservableObject
    {
        private readonly GrpcProjectflowStepOptions _grpcProjectflowStepOptions;
        public GrpcProjectflowStepModel(GrpcProjectflowStepOptions grpcProjectflowStepOptions)
        {
            _grpcProjectflowStepOptions = grpcProjectflowStepOptions;
            StepCode = grpcProjectflowStepOptions.StepCode;
            StepName = grpcProjectflowStepOptions.StepName;
            StepId = grpcProjectflowStepOptions.StepId;
        }
        public GrpcProjectflowStepOptions GrpcProjectflowStepOptions => _grpcProjectflowStepOptions;

        [ObservableProperty]
        private string _stepCode = string.Empty;
        [ObservableProperty]
        private string _stepName = string.Empty;

        public Guid StepId { get; set; }

        partial void OnStepCodeChanged(string value)
        {
            _grpcProjectflowStepOptions.StepCode = value;
        }
        partial void OnStepNameChanged(string value)
        {
            _grpcProjectflowStepOptions.StepName = value;
        }
    }
    public partial class PlatformCreateModel:ObservableObject
    {

        private const string InitFlowCode = "初始化流程";
        private const string PreperExperimentFlowCode = "实验前准备流程";
        private const string StartTaskFlowCode = "实验任务流程";
        private const string FinalizeFlowCode = "实验收尾流程";
        private const string LineDrainageFlowCode= "实验管路排空流程";
        private const string MaintenanceFlowCode = "仪器维护流程";
        private const string SystemStorageFlowCode = "系统封存流程";



        public PlatformCreateModel()
        {
            View = new PlatformCreateView(this);

            FlowEditItems =
            [
                new FlowEditItem(InitFlowCode,this,HomeFiles),
                new FlowEditItem(PreperExperimentFlowCode,this,PreperExperimentFiles),
                new FlowEditItem(MaintenanceFlowCode,this, MaintenanceFiles),
                new FlowEditItem(LineDrainageFlowCode,this, LineDrainageFiles),
                new FlowEditItem(StartTaskFlowCode,this, StartTaskFiles),
                new FlowEditItem(FinalizeFlowCode,this, FinalizeFiles),
                new FlowEditItem(SystemStorageFlowCode,this, SystemStorageFiles),
            ];
            foreach (var item in FlowEditItems)
            {
                FlowEditItemDictionary[item.EditItemName] = item;
            }
            foreach (var item in App.ToolEngine.GetAllFlows())
            {
                FlowSources.Add(item);
            }
            PlatformId = SnowflakeIdGenerator.Instance.GenerateYitId();
            foreach (var item in EnumValuesProvider.GetEnumDescriptionToList<PlatformType>())
            {
                PlatformTypes.Add(item);
            }
           
            ModuleInfoParameterModels = ParameterModelRepository.SharedModuleInfoParameters;
            ModuleFuncCodeParameterModels = ParameterModelRepository.ModuleFuncCodeParameterTableViewModel.Tables.ToObservableCollection();
            ParameterModelRepository.ParameterTableChanged += (sender, e) =>
            {
               ModuleInfoParameterModels = ParameterModelRepository.SharedModuleInfoParameters;
               ModuleFuncCodeParameterModels = ParameterModelRepository.ModuleFuncCodeParameterTableViewModel.Tables.ToObservableCollection();
            };
        }
        [ObservableProperty]
        private string _platformName = string.Empty;
        [ObservableProperty]
        private string _platformCode = string.Empty;
        [ObservableProperty]
        private string _platformDescription = string.Empty;
        [ObservableProperty]
        private KeyValuePair<string, PlatformType> _platformType;
        [ObservableProperty]
        private ModuleInfoParameterModel? _platformModuleInfoParameterModel;

        [ObservableProperty]
        private ModuleFuncCodeParameterModel? _platformGrabActionParameterModel;

        [ObservableProperty]
        private int _platformSamplingFlux=2;
        [ObservableProperty]
        private int _platformMaxExecuteCount=2;
        [ObservableProperty]
        private int _platformMaxCacheCount=2;

        public long PlatformId { get; set; }

        public ObservableCollection<KeyValuePair<string,PlatformType>> PlatformTypes { get; set; } = [];

        public Dictionary<string, FlowEditItem> FlowEditItemDictionary { get; set; } = [];

        public ObservableCollection<FlowEditItem> FlowEditItems { get; set; }

        public ObservableCollection<Flow> FlowSources { get; set; } = [];

        public ObservableCollection<GrpcProjectPlatformFlowConfigOptions> HomeFiles { get; set; } = [];

        public ObservableCollection<GrpcProjectPlatformFlowConfigOptions> PreperExperimentFiles { get; set; } = [];

        public ObservableCollection<GrpcProjectPlatformFlowConfigOptions> StartTaskFiles { get; set; } = [];

        public ObservableCollection<GrpcProjectPlatformFlowConfigOptions> FinalizeFiles { get; set; } = [];

        public ObservableCollection<GrpcProjectPlatformFlowConfigOptions> LineDrainageFiles { get; set; } = [];

        public ObservableCollection<GrpcProjectPlatformFlowConfigOptions> MaintenanceFiles { get; set; } = [];

        public ObservableCollection<GrpcProjectPlatformFlowConfigOptions> SystemStorageFiles { get; set; } = [];


        public ObservableCollection<ModuleInfoParameterModel> ModuleInfoParameterModels { get; set; }

        public ObservableCollection<ModuleFuncCodeParameterModel> ModuleFuncCodeParameterModels { get; set; } = [];
        public object View { get; set; }


        public ObservableCollection<PlatformMonitorItemViewModel> PlatformMonitorItems { get; set; } = [];

        [RelayCommand]
        private void AddPlatformMonitorItem()
        {
            PlatformMonitorItems.Add(new PlatformMonitorItemViewModel(new QStandaedPlatform.Engine.Laboratory.PlatformMonitorItem()));
        }

        [RelayCommand]
        private void RemovePlatformMonitorItem(PlatformMonitorItemViewModel item)
        {
            PlatformMonitorItems.Remove(item);
        }

        public void LoadHomeFlows(GrpcProjectPlatformFlowConfigOptions configOptions)
        {
            if (configOptions == null) return;
            if (configOptions.FlowId != Guid.Empty)
            {
                var flow = FlowSources.FirstOrDefault(x => x.FlowId == configOptions.FlowId);
                if (flow != null)
                {
                    var flowTask = new FlowTaskDisplayModel
                    {
                        FlowTaskId = configOptions.FlowTaskId,
                        FlowTaskName = configOptions.FlowTaskCode,
                        FlowTaskDescription = configOptions.FlowTaskDescription,
                    };
                    foreach (var item in configOptions.StepDisplays)
                    {
                        flowTask.Steps.Add(new StepDisplayModel 
                        {
                             DisplayName= item.DisplayName,
                             StepCode= item.Description,
                             StepId= item.StepId,
                        });
                    }
                    configOptions.Flow = flow;
                    FlowEditItemDictionary[InitFlowCode].FlowTasks.Add(flowTask);
                    FlowEditItemDictionary[InitFlowCode].Files.Add(configOptions);
                }
            }
        }
        public void LoadPreperExperimentFlows(GrpcProjectPlatformFlowConfigOptions configOptions)
        {
            if (configOptions == null) return;
            if (configOptions.FlowId != Guid.Empty)
            {
                var flow = FlowSources.FirstOrDefault(x => x.FlowId == configOptions.FlowId);
                if (flow != null)
                {
                    var flowTask = new FlowTaskDisplayModel
                    {
                        FlowTaskId = configOptions.FlowTaskId,
                        FlowTaskName = configOptions.FlowTaskCode,
                        FlowTaskDescription = configOptions.FlowTaskDescription,
                    };
                    foreach (var item in configOptions.StepDisplays)
                    {
                        flowTask.Steps.Add(new StepDisplayModel
                        {
                            DisplayName = item.DisplayName,
                            StepCode = item.Description,
                            StepId = item.StepId,
                        });
                    }
                    configOptions.Flow = flow;
                    FlowEditItemDictionary[PreperExperimentFlowCode].FlowTasks.Add(flowTask);
                    FlowEditItemDictionary[PreperExperimentFlowCode].Files.Add(configOptions);
                }
            }
        }

        public void LoadStartTaskFlows(GrpcProjectPlatformFlowConfigOptions configOptions)
        {
            if (configOptions == null) return;
            if (configOptions.FlowId != Guid.Empty)
            {
                var flow = FlowSources.FirstOrDefault(x => x.FlowId == configOptions.FlowId);
                if (flow != null)
                {
                    var flowTask = new FlowTaskDisplayModel
                    {
                        FlowTaskId = configOptions.FlowTaskId,
                        FlowTaskName = configOptions.FlowTaskCode,
                        FlowTaskDescription = configOptions.FlowTaskDescription,
                    };
                    foreach (var item in configOptions.StepDisplays)
                    {
                        flowTask.Steps.Add(new StepDisplayModel
                        {
                            DisplayName = item.DisplayName,
                            StepCode = item.Description,
                            StepId = item.StepId,
                        });
                    }
                    configOptions.Flow = flow;
                    FlowEditItemDictionary[StartTaskFlowCode].FlowTasks.Add(flowTask);
                    FlowEditItemDictionary[StartTaskFlowCode].Files.Add(configOptions);
                }
            }
        }

        public void LoadFinalizeFlows(GrpcProjectPlatformFlowConfigOptions configOptions)
        {
            if (configOptions == null) return;
            if (configOptions.FlowId != Guid.Empty)
            {
                var flow = FlowSources.FirstOrDefault(x => x.FlowId == configOptions.FlowId);
                if (flow != null)
                {

                    var flowTask = new FlowTaskDisplayModel
                    {
                        FlowTaskId = configOptions.FlowTaskId,
                        FlowTaskName = configOptions.FlowTaskCode,
                        FlowTaskDescription = configOptions.FlowTaskDescription,
                    };
                    foreach (var item in configOptions.StepDisplays)
                    {
                        flowTask.Steps.Add(new StepDisplayModel
                        {
                            DisplayName = item.DisplayName,
                            StepCode = item.Description,
                            StepId = item.StepId,
                        });
                    }
                    configOptions.Flow = flow;
                    FlowEditItemDictionary[FinalizeFlowCode].FlowTasks.Add(flowTask);
                    FlowEditItemDictionary[FinalizeFlowCode].Files.Add(configOptions);
                }
            }
        }

        public void LoadLineDrainageFlows(GrpcProjectPlatformFlowConfigOptions configOptions)
        {
            if (configOptions == null) return;
            if (configOptions.FlowId != Guid.Empty)
            {
                var flow = FlowSources.FirstOrDefault(x => x.FlowId == configOptions.FlowId);
                if (flow != null)
                {

                    var flowTask = new FlowTaskDisplayModel
                    {
                        FlowTaskId = configOptions.FlowTaskId,
                        FlowTaskName = configOptions.FlowTaskCode,
                        FlowTaskDescription = configOptions.FlowTaskDescription,
                    };
                    foreach (var item in configOptions.StepDisplays)
                    {
                        flowTask.Steps.Add(new StepDisplayModel
                        {
                            DisplayName = item.DisplayName,
                            StepCode = item.Description,
                            StepId = item.StepId,
                        });
                    }
                    configOptions.Flow = flow;
                    FlowEditItemDictionary[LineDrainageFlowCode].FlowTasks.Add(flowTask);
                    FlowEditItemDictionary[LineDrainageFlowCode].Files.Add(configOptions);
                }
            }
        }
        public void LoadMaintenanceFlows(GrpcProjectPlatformFlowConfigOptions configOptions)
        {
            if (configOptions == null) return;
            if (configOptions.FlowId != Guid.Empty)
            {
                var flow = FlowSources.FirstOrDefault(x => x.FlowId == configOptions.FlowId);
                if (flow != null)
                {

                    var flowTask = new FlowTaskDisplayModel
                    {
                        FlowTaskId = configOptions.FlowTaskId,
                        FlowTaskName = configOptions.FlowTaskCode,
                        FlowTaskDescription = configOptions.FlowTaskDescription,
                    };
                    foreach (var item in configOptions.StepDisplays)
                    {
                        flowTask.Steps.Add(new StepDisplayModel
                        {
                            DisplayName = item.DisplayName,
                            StepCode = item.Description,
                            StepId = item.StepId,
                        });
                    }
                    configOptions.Flow = flow;
                    FlowEditItemDictionary[MaintenanceFlowCode].FlowTasks.Add(flowTask);
                    FlowEditItemDictionary[MaintenanceFlowCode].Files.Add(configOptions);
                }
            }
        }

        public void LoadSystemStorageFlows(GrpcProjectPlatformFlowConfigOptions configOptions)
        {
            if (configOptions == null) return;
            if (configOptions.FlowId != Guid.Empty)
            {
                var flow = FlowSources.FirstOrDefault(x => x.FlowId == configOptions.FlowId);
                if (flow != null)
                {

                    var flowTask = new FlowTaskDisplayModel
                    {
                        FlowTaskId = configOptions.FlowTaskId,
                        FlowTaskName = configOptions.FlowTaskCode,
                        FlowTaskDescription = configOptions.FlowTaskDescription,
                    };
                    foreach (var item in configOptions.StepDisplays)
                    {
                        flowTask.Steps.Add(new StepDisplayModel
                        {
                            DisplayName = item.DisplayName,
                            StepCode = item.Description,
                            StepId = item.StepId,
                        });
                    }
                    configOptions.Flow = flow;
                    FlowEditItemDictionary[SystemStorageFlowCode].FlowTasks.Add(flowTask);
                    FlowEditItemDictionary[SystemStorageFlowCode].Files.Add(configOptions);
                }
            }
        }

    }
}
