using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Equipment.Bqjx.StandardPlatformSystem.WorkFlows;
using QStandaedPlatform.Engine.Common.Common;
using QStandaedPlatform.Engine.Components.Modbus;
using QStandaedPlatform.Engine.Extensions;
using QStandaedPlatform.Engine.Laboratory;
using System.Collections.ObjectModel;
using System.Windows;

namespace Equipment.Bqjx.StandardPlatformSystem.Models
{
    public partial class GrpcProjectProcessflowModel:ObservableObject
    {
        private readonly GrpcProjectProcessflowOptions _grpcProjectProcessflowOptinos;
        public GrpcProjectProcessflowModel(GrpcProjectProcessflowOptions grpcProjectProcessflowOptions)
        {
            _grpcProjectProcessflowOptinos = grpcProjectProcessflowOptions;
            ProcessCode = grpcProjectProcessflowOptions.ProcessCode;
            ProcessName = grpcProjectProcessflowOptions.ProcessName;
            ProcessDescription = grpcProjectProcessflowOptions.ProcessDescription;
            ProcessId = grpcProjectProcessflowOptions.ProcessId;
            ProcessflowTypes = EnumValuesProvider.GetEnumDescriptionToList<ProcessStep>().ToObservableCollection();
            ProcessStep = ProcessflowTypes.FirstOrDefault(x => x.Value == grpcProjectProcessflowOptions.ProcessStep);
        }

        public GrpcProjectProcessflowOptions GrpcProjectProcessflowOptinos => _grpcProjectProcessflowOptinos;

        [ObservableProperty]
        private string _processCode = string.Empty;
        [ObservableProperty]
        private string _processName = string.Empty;
        [ObservableProperty]
        private string _processDescription = string.Empty;
        [ObservableProperty]
        private KeyValuePair<string, ProcessStep> _processStep;

        public Guid ProcessId { get; set; }


        partial void OnProcessCodeChanged(string value)
        {
           _grpcProjectProcessflowOptinos.ProcessCode = value;
        }
        partial void OnProcessNameChanged(string value)
        {
            _grpcProjectProcessflowOptinos.ProcessName = value;
        }
        partial void OnProcessDescriptionChanged(string value)
        {
            _grpcProjectProcessflowOptinos.ProcessDescription = value;
        }

        partial void OnProcessStepChanged(KeyValuePair<string, ProcessStep> value)
        {
            if (!value.Equals(default(KeyValuePair<string, ProcessStep>)))
            {
                _grpcProjectProcessflowOptinos.ProcessStep = value.Value;
            }
        }

        public ObservableCollection<KeyValuePair<string, ProcessStep>> ProcessflowTypes { get; set; }

        public ObservableCollection<GrpcProjectPlatformsInOrderConfigOptions> PlatformsInOrder { get; set; } = [];

        public ObservableCollection<GrpcProjectTransferStepOptions> TransferStepsInOrder { get; set; } = [];

        public ObservableCollection<GrpcProjectModuleActionStepOptions> ModuleActionStepsInOrder { get; set; } = [];



        [RelayCommand]
        private void AddOrderPlatform(GrpcProjectPlatformFlowConfigOptions platform)
        {
            if (platform == null)
                return;
            var step = new GrpcProjectPlatformsInOrderConfigOptions
            {
                PlatformFlowStepId = SnowflakeIdGenerator.Instance.GenerateYitId(),
                PlatformId = platform.PlatformId,
                FlowTaskId = platform.FlowTaskId,
                FlowConfigPath = platform.FlowConfigPath,
                FlowTaskCode = platform.FlowTaskCode,
                FlowTaskDescription = platform.FlowTaskDescription,
            };
            PlatformsInOrder.Add(step);
            _grpcProjectProcessflowOptinos.PlatformFlowSteps.Add(step);
        }

        [RelayCommand]
        private void RemoveOrderPlatform(GrpcProjectPlatformsInOrderConfigOptions  orderConfigOptions)
        {
            if (orderConfigOptions == null)
                return;
            PlatformsInOrder.Remove(orderConfigOptions);
            var platformId = _grpcProjectProcessflowOptinos.PlatformFlowSteps.FirstOrDefault(x => x.PlatformFlowStepId == orderConfigOptions.PlatformFlowStepId);
            if (platformId != null)
            {
                _grpcProjectProcessflowOptinos.PlatformFlowSteps.Remove(platformId);
            }
        }

        [RelayCommand]
        private void AddTransferStep(GrpcProjectTransferModuleModel transferModule)
        {
            if (transferModule == null)
                return;
            var step = new GrpcProjectTransferStepOptions
            {
                StepId = SnowflakeIdGenerator.Instance.GenerateYitId(),
                StepOrder = TransferStepsInOrder.Count,
                StepDescription = $"Transfer from Platform {transferModule.LeftPlatformModel?.PlatformName ?? "?"} to {transferModule.RightPlatformModel?.PlatformName ?? "?"}",
                TransferModuleId = transferModule.GrpcProjectTransferModuleOptions.TransferModuleId,
                TransferDirection = TransferDirection.Forward,
                SourcePlatformId = transferModule.GrpcProjectTransferModuleOptions.LeftPlatformId,
                TargetPlatformId = transferModule.GrpcProjectTransferModuleOptions.RightPlatformId,
            };
            TransferStepsInOrder.Add(step);
            _grpcProjectProcessflowOptinos.TransferSteps.Add(step);
        }

        [RelayCommand]
        private void RemoveTransferStep(GrpcProjectTransferStepOptions transferStep)
        {
            if (transferStep == null)
                return;
            TransferStepsInOrder.Remove(transferStep);
            var step = _grpcProjectProcessflowOptinos.TransferSteps.FirstOrDefault(x => x.StepId == transferStep.StepId);
            if (step != null)
            {
                _grpcProjectProcessflowOptinos.TransferSteps.Remove(step);
            }
        }

        [RelayCommand]
        private void AddModuleActionStep(ModuleFuncCodeParameterModel moduleAction)
        {
            if (moduleAction == null || moduleAction.Parameter.ModuleInfoParameter == null)
                return;
            var step = new GrpcProjectModuleActionStepOptions
            {
                StepId = SnowflakeIdGenerator.Instance.GenerateYitId(),
                StepOrder = ModuleActionStepsInOrder.Count,
                StepDescription = moduleAction.Parameter.FuncCodeDescription,
                ModuleName = moduleAction.Parameter.ModuleInfoParameter.ModuleName,
                ModuleSerialNumber = moduleAction.Parameter.ModuleInfoParameter.ModuleSerialNumber,
                ModuleActionId = moduleAction.Parameter.ParameterId,
                ActionName = moduleAction.Parameter.FuncCodeName,
                ActionDescription = moduleAction.Parameter.FuncCodeDescription,
                ActionParameters = [.. moduleAction.Parameter.FuncCodeParamterInfos.Select(p => new ParameterItem
                {
                    ParameterId = p.ParameterId,
                    Name = p.ParameterName,
                    Description = p.ParameterDescription,
                    Value = p.ParameterValueFactory.FirstOrDefault().Value,
                    Unit = p.ParameterUnit,
                    MaxValue = p.ParameterMaxValue,
                    MinValue = p.ParameterMinValue,
                })],
            };
            ModuleActionStepsInOrder.Add(step);
            _grpcProjectProcessflowOptinos.ModuleActionSteps.Add(step);
        }

        [RelayCommand]
        private void RemoveModuleActionStep(GrpcProjectModuleActionStepOptions moduleActionStep)
        {
            if (moduleActionStep == null)
                return;
            ModuleActionStepsInOrder.Remove(moduleActionStep);
            var step = _grpcProjectProcessflowOptinos.ModuleActionSteps.FirstOrDefault(x => x.StepId == moduleActionStep.StepId);
            if (step != null)
            {
                _grpcProjectProcessflowOptinos.ModuleActionSteps.Remove(step);
            }
        }
    }

    public partial class GrpcProjectProductLineModel : ObservableObject
    {
        private readonly GrpcProjectProductlineOptions _grpcProjectProductlineOptions;

        public GrpcProjectProductLineModel(GrpcProjectProductlineOptions grpcProjectProductlineOptions)
        {
            _grpcProjectProductlineOptions = grpcProjectProductlineOptions;
            ProductLineCode = grpcProjectProductlineOptions.ProductlineCode;
            ProductLineName = grpcProjectProductlineOptions.ProductlineName;
            ProductLineDescription = grpcProjectProductlineOptions.ProductlineDescription;
            if (grpcProjectProductlineOptions.ProductlineId != 0)
            {
                ProductLineId = grpcProjectProductlineOptions.ProductlineId;
            }
            else
            {
                ProductLineId = SnowflakeIdGenerator.Instance.GenerateYitId();
                GrpcProjectProductlineOptions.ProductlineId = ProductLineId;
            }
        }

        public GrpcProjectProductlineOptions GrpcProjectProductlineOptions => _grpcProjectProductlineOptions;

        [ObservableProperty]
        private string _productLineName = string.Empty;
        [ObservableProperty]
        private string _productLineCode = string.Empty;
        [ObservableProperty]
        private string _productLineDescription = string.Empty;

        public long ProductLineId { get; set; }


        partial void OnProductLineNameChanged(string value)
        {
            _grpcProjectProductlineOptions.ProductlineName = value;
        }
        partial void OnProductLineCodeChanged(string value)
        {
            _grpcProjectProductlineOptions.ProductlineCode = value;
        }
        partial void OnProductLineDescriptionChanged(string value)
        {
            _grpcProjectProductlineOptions.ProductlineDescription = value;
        }

        [RelayCommand]
        private void AddOrderProcessFlow(GrpcProjectProcessflowModel processFlow)
        {
            if (processFlow == null)
                return;
            //校验是否存在工艺流程
            if (GrpcProjectProductlineOptions.ProcessflowIds.Contains(processFlow.ProcessId))
            {

                MessageBox.Show("工艺已添加");
                return;
            }

            ProcessFlowsInOrder.Add(processFlow);
            GrpcProjectProductlineOptions.ProcessflowIds.Add(processFlow.ProcessId);
        }

        [RelayCommand]
        private void RemoveOrderProcessFlow(GrpcProjectProcessflowModel processFlow)
        {
            if (processFlow == null)
                return;
            ProcessFlowsInOrder.Remove(processFlow);
            GrpcProjectProductlineOptions.ProcessflowIds.Remove(processFlow.ProcessId);
        }

        public ObservableCollection<GrpcProjectProcessflowModel> ProcessFlowsInOrder { get; set; } = [];
    }


    public partial class GrpcProjectTransferModuleModel : ObservableObject
    {
        private readonly GrpcProjectTransferModuleOptions _grpcProjectTransferModuleOptions;
        public GrpcProjectTransferModuleModel(GrpcProjectTransferModuleOptions grpcProjectTransferModuleOptions)
        {
            _grpcProjectTransferModuleOptions = grpcProjectTransferModuleOptions;
            TransferModuleName = grpcProjectTransferModuleOptions.TransferModuleName;

            TransferModuleSamplingFlux = grpcProjectTransferModuleOptions.TransferModuleSamplingFlux;
            IsReverse = grpcProjectTransferModuleOptions.IsReverse;
            ModuleInfoParameterModels = ParameterModelRepository.SharedModuleInfoParameters;
            LeftChannelGroupParameterModels = ParameterModelRepository.ModuleChannelGroupParameterTableModel.Tables.ToObservableCollection();
            RightChannelGroupParameterModels = ParameterModelRepository.ModuleChannelGroupParameterTableModel.Tables.ToObservableCollection();
            ForwardModuleFuncCodeInfoParameterModels = ParameterModelRepository.ModuleFuncCodeParameterTableViewModel.Tables.ToObservableCollection();
            BackwardModuleFuncCodeInfoParameterModels = ParameterModelRepository.ModuleFuncCodeParameterTableViewModel.Tables.ToObservableCollection();
            ParameterModelRepository.ParameterTableChanged += (sender, e) =>
            {
                ModuleInfoParameterModels = ParameterModelRepository.SharedModuleInfoParameters;
                LeftChannelGroupParameterModels = ParameterModelRepository.ModuleChannelGroupParameterTableModel.Tables.ToObservableCollection();
                RightChannelGroupParameterModels = ParameterModelRepository.ModuleChannelGroupParameterTableModel.Tables.ToObservableCollection();
                ForwardModuleFuncCodeInfoParameterModels = ParameterModelRepository.ModuleFuncCodeParameterTableViewModel.Tables.ToObservableCollection();
                BackwardModuleFuncCodeInfoParameterModels = ParameterModelRepository.ModuleFuncCodeParameterTableViewModel.Tables.ToObservableCollection();
            };
            TransferModuleInfoParameterModel = ModuleInfoParameterModels.FirstOrDefault(p => p.ModuleInfoParameter.ModuleInfoId == grpcProjectTransferModuleOptions.TransferModuleInfoId);
            LeftChannelGroupParameterModel = LeftChannelGroupParameterModels.FirstOrDefault(p => p.Parameter.ParameterId == grpcProjectTransferModuleOptions.LeftChannelGroupId);
            RightChannelGroupParameterModel = RightChannelGroupParameterModels.FirstOrDefault(p => p.Parameter.ParameterId == grpcProjectTransferModuleOptions.RightChannelGroupId);
            ForwardModuleFuncCodeInfoParameterModel = ForwardModuleFuncCodeInfoParameterModels.FirstOrDefault(p => p.Parameter.ParameterId == grpcProjectTransferModuleOptions.TransferForwardMoveId);
            BackwardModuleFuncCodeInfoParameterModel = BackwardModuleFuncCodeInfoParameterModels.FirstOrDefault(p => p.Parameter.ParameterId == grpcProjectTransferModuleOptions.TransferBackwardMoveId);
        }

        public GrpcProjectTransferModuleOptions GrpcProjectTransferModuleOptions => _grpcProjectTransferModuleOptions;

        [ObservableProperty]
        private string _transferModuleName = string.Empty;

        [ObservableProperty]
        private ModuleInfoParameterModel? _transferModuleInfoParameterModel;

        [ObservableProperty]
        private PlatformCreateModel? _leftPlatformModel;

        [ObservableProperty]
        private PlatformCreateModel? _rightPlatformModel;

        [ObservableProperty]
        private int _transferModuleSamplingFlux;

        [ObservableProperty]
        private bool _isReverse;

        [ObservableProperty]
        private ModuleChannelGroupParameterModel? _leftChannelGroupParameterModel;

        [ObservableProperty]
        private ModuleChannelGroupParameterModel? _rightChannelGroupParameterModel;
        
        [ObservableProperty]
        private ModuleFuncCodeParameterModel? _forwardModuleFuncCodeInfoParameterModel;

        [ObservableProperty]
        private ModuleFuncCodeParameterModel? _backwardModuleFuncCodeInfoParameterModel;

        partial void OnTransferModuleNameChanged(string value)
        {
            _grpcProjectTransferModuleOptions.TransferModuleName = value;
        }

        partial void OnTransferModuleSamplingFluxChanged(int value)
        {
            _grpcProjectTransferModuleOptions.TransferModuleSamplingFlux = value;
        }

        partial void OnIsReverseChanged(bool value)
        {
            _grpcProjectTransferModuleOptions.IsReverse = value;
        }

        partial void OnTransferModuleInfoParameterModelChanged(ModuleInfoParameterModel? value)
        {
            if (value != null)
            {
                _grpcProjectTransferModuleOptions.TransferModuleInfoId = value.ModuleInfoParameter.ModuleInfoId;
            }
        }

        partial void OnLeftPlatformModelChanged(PlatformCreateModel? value)
        {
            if (value != null)
            {
                _grpcProjectTransferModuleOptions.LeftPlatformId = value.PlatformId;
            }
        }
        partial void OnRightPlatformModelChanged(PlatformCreateModel? value)
        {
            if (value != null) 
            {
                _grpcProjectTransferModuleOptions.RightPlatformId = value.PlatformId;
            }
        }
        partial void OnLeftChannelGroupParameterModelChanged(ModuleChannelGroupParameterModel? value)
        {
            if (value != null)
            {
                _grpcProjectTransferModuleOptions.LeftChannelGroupId = value.Parameter.ParameterId;
            }
        }
        partial void OnRightChannelGroupParameterModelChanged(ModuleChannelGroupParameterModel? value)
        {
            if (value != null) 
            {
                _grpcProjectTransferModuleOptions.RightChannelGroupId = value.Parameter.ParameterId;
            }
        }

        partial void OnForwardModuleFuncCodeInfoParameterModelChanged(ModuleFuncCodeParameterModel? value)
        {
            if (value != null)
            {
                _grpcProjectTransferModuleOptions.TransferForwardMoveId = value.Parameter.ParameterId;
            }
        }
        partial void OnBackwardModuleFuncCodeInfoParameterModelChanged(ModuleFuncCodeParameterModel? value)
        {
            if (value != null)
            {
                _grpcProjectTransferModuleOptions.TransferBackwardMoveId = value.Parameter.ParameterId;
            }
        }

        /// <summary>
        /// 中转模块参数信息
        /// </summary>
        public ObservableCollection<ModuleInfoParameterModel> ModuleInfoParameterModels { get; set; } = [];

        /// <summary>
        /// 左通道参数
        /// </summary>
        public ObservableCollection<ModuleChannelGroupParameterModel> LeftChannelGroupParameterModels { get; set; } = [];

        /// <summary>
        /// 右通道参数
        /// </summary>
        public ObservableCollection<ModuleChannelGroupParameterModel> RightChannelGroupParameterModels { get; set; } = [];


        /// <summary>
        /// 正向移动参数信息
        /// </summary>
        public ObservableCollection<ModuleFuncCodeParameterModel> ForwardModuleFuncCodeInfoParameterModels { get; set; } = [];

        /// <summary>
        /// 反向移动参数信息
        /// </summary>
        public ObservableCollection<ModuleFuncCodeParameterModel> BackwardModuleFuncCodeInfoParameterModels { get; set; } = [];
    }




}
