using Newtonsoft.Json;
using QStandaedPlatform.Engine.Laboratory;
using System.ComponentModel;

namespace QStandaedPlatform.Engine.Common.Common
{
    public class GrpcProjectService: Singleton<GrpcProjectService>
    {
        public List<GrpcProjectOptions> GrpcProjects { get; set; } = [];

        public void AddProject(GrpcProjectOptions project)
        {
            GrpcProjects.Add(project);
        }

        public void RemoveProject(GrpcProjectOptions project)
        {
            GrpcProjects.Remove(project);
        }

        public void ClearProject()
        {
            GrpcProjects.Clear();
        }

        public void SaveProject()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GrpcProjects", "GrpcProjects.json");
            if (!Directory.Exists(Path.GetDirectoryName(path)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            }
            if (!File.Exists(path))
            {
                File.Create(path).Close();
            }
            File.WriteAllText(path, JsonConvert.SerializeObject(GrpcProjects));
        }

        public void LoadProject()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GrpcProjects", "GrpcProjects.json");
            if (File.Exists(path))
            {
                try
                {
                    var grpcProjectOptions = JsonConvert.DeserializeObject<List<GrpcProjectOptions>>(File.ReadAllText(path));
                    if (grpcProjectOptions != null)
                    {
                        GrpcProjects.AddRange(grpcProjectOptions);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }


    /// <summary>
    /// 实验室项目配置
    /// </summary>
    public class GrpcProjectOptions
    {
        public string SolutionRootPath { get; set; } = string.Empty;
        public string SolutionName { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public string ProjectDescription { get; set; } = string.Empty;
        public string ProjectVersion { get; set; } = string.Empty;
        public long   LaboratoryId { get; set; }
        public string LaboratoryName { get; set; } = string.Empty;
        public string LaboratoryDescription { get; set; } = string.Empty;
        public string LaboratoryCode { get; set; } = string.Empty;


        /// <summary>
        /// GRPC平台配置
        /// </summary>
        public List<GrpcProjectPlatformOptions> GrpcProjectPlatformOptions { get; set; } = [];

        /// <summary>
        /// GRPC工艺流程配置
        /// </summary>
        public List<GrpcProjectProcessflowOptions> GrpcProjectProcessflowOptions { get; set; } = [];
        /// <summary>
        /// GRPC产品线配置
        /// </summary>
        public List<GrpcProjectProductlineOptions> GrpcProjectProductlineOptions { get; set; } = [];
        /// <summary>
        /// GRPC中转模块配置
        /// </summary>
        public List<GrpcProjectTransferModuleOptions> GrpcProjectTransferModuleOptions { get; set; } = [];
    }

    //平台类型
    public enum PlatformType
    {
        /// <summary>
        /// 实验平台
        /// </summary>
        [Description("实验平台")]
        ExperimentPlatform,
        /// <summary>
        /// 定时进样平台
        /// </summary>
        [Description("定时进样平台")]
        TimedSamplingPlatform,
        /// <summary>
        /// 扫码平台
        /// </summary>
        [Description("扫码平台")]
        ScanPlatform,
    }



    /// <summary>
    /// 平台配置
    /// </summary>
    public class GrpcProjectPlatformOptions
    {
        /// <summary>
        /// 平台ID
        /// </summary>
        public long PlatformId { get; set; }
        /// <summary>
        /// 平台名称
        /// </summary>
        public string PlatformName { get; set; } = string.Empty;
        /// <summary>
        /// 平台描述
        /// </summary>
        public string PlatformDescription { get; set; } = string.Empty;
        /// <summary>
        /// 平台编号
        /// </summary>
        public string PlatformCode { get; set; } = string.Empty;
        /// <summary>
        /// 平台模块ID
        /// </summary>
        public Guid PlatformModuleId { get; set; }

       /// <summary>
       /// 平台抓取动作ID
       /// </summary>
       public Guid PlatformGrabActionId { get; set; }

        /// <summary>
        /// 平台进样通量
        /// </summary>
        public int PlatformSamplingFlux { get; set; }

        /// <summary>
        /// 平台最大执行数
        /// </summary>
        public int PlatformMaxExecuteCount { get; set; }

        /// <summary>
        /// 平台最大缓存量
        /// </summary>
        public int PlatformMaxCacheCount { get; set; }

        /// <summary>
        /// 初始化流程配置
        /// </summary>
        public List<GrpcProjectPlatformFlowConfigOptions> InitialFlowConfigs { get; set; } = [];
        /// <summary>
        /// 实验前准备流程配置
        /// </summary>
        public List<GrpcProjectPlatformFlowConfigOptions> PrepareFlowConfigs { get; set; } = [];
        /// <summary>
        /// 实验流程配置
        /// </summary>
        public List<GrpcProjectPlatformFlowConfigOptions> TaskFlowConfigs { get; set; } = [];
        /// <summary>
        /// 管路排空流程配置
        /// </summary>
        public List<GrpcProjectPlatformFlowConfigOptions> LineDrainageFlowConfigs { get; set; } = [];
        /// <summary>
        /// 设备维护流程配置
        /// </summary>
        public List<GrpcProjectPlatformFlowConfigOptions> MaintainFlowConfigs { get; set; } = [];
        /// <summary>
        /// 实验收尾流程配置
        /// </summary>
        public List<GrpcProjectPlatformFlowConfigOptions> FinalizeFlowConfigs { get; set; } = [];
        /// <summary>
        /// 系统封存流程配置
        /// </summary>
        public List<GrpcProjectPlatformFlowConfigOptions> SystemStorageFlowConfigs { get; set; } = [];
        /// <summary>
        /// 平台监控项
        /// </summary>
        public List<PlatformMonitorItem> PlatformMonitorItems { get; set; } = [];
        /// <summary>
        /// 平台托盘配置
        /// </summary>
        public List<LabTrayConfiguration> LabTrayConfigs { get; set; } = [];
    }


    /// <summary>
    /// 平台顺序配置
    /// </summary>
    public class GrpcProjectPlatformsInOrderConfigOptions
    {
        /// <summary>
        /// 平台顺序实体的唯一ID
        /// </summary>
        public long PlatformFlowStepId { get; set; }
        /// <summary>
        /// 平台ID
        /// </summary>
        public long PlatformId { get; set; }
        /// <summary>
        /// 流程任务ID
        /// </summary>
        public long FlowTaskId { get; set; }
        /// <summary>
        /// 流程配置路径
        /// </summary>
        public string FlowConfigPath { get; set; } = string.Empty;
        /// <summary>
        /// 流程任务编号
        /// </summary>
        public string FlowTaskCode { get; set; } = string.Empty;
        /// <summary>
        /// 流程任务描述
        /// </summary>
        public string FlowTaskDescription { get; set; } = string.Empty;
        /// <summary>
        /// 步骤执行顺序
        /// </summary>
        public int StepOrder { get; set; }
    }



    public class StepDisplay:ICloneable
    {
        public string DisplayName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public Guid StepId { get; set; }

        public object Clone()
        {
            var clone = new StepDisplay
            {
                DisplayName = DisplayName,
                Description = Description,
                StepId = StepId
            };
            return clone;
        }
    }
    /// <summary>
    /// 平台任务配置
    /// </summary>
    public class GrpcProjectPlatformFlowConfigOptions:ICloneable,INotifyPropertyChanged
    {
        /// <summary>
        /// 流程配置路径
        /// </summary>
        public string FlowConfigPath { get; set; } = string.Empty;

        /// <summary>
        /// 流程任务编号
        /// </summary>
        private string _FlowTaskCode = string.Empty;

        public string FlowTaskCode
        {
            get { return _FlowTaskCode; }
            set
            {
                if (!EqualityComparer<string>.Default.Equals(_FlowTaskCode, value))
                {
                    _FlowTaskCode = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FlowTaskCode)));
                }
            }
        }


        /// <summary>
        /// 流程任务描述
        /// </summary>
        private string _FlowTaskDescription = string.Empty;

        public string FlowTaskDescription
        {
            get { return _FlowTaskDescription; }
            set
            {
                if (!EqualityComparer<string>.Default.Equals(_FlowTaskDescription, value))
                {
                    _FlowTaskDescription = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FlowTaskDescription)));
                }
            }
        }

        /// <summary>
        /// 流程ID
        /// </summary>
        public Guid FlowId { get; set; }
        /// <summary>
        /// 流程任务ID
        /// </summary>
        public long FlowTaskId { get; set; }
        [JsonIgnore]
        public Flow? Flow { get; set; }
        /// <summary>
        /// 平台ID
        /// </summary>
        public long PlatformId { get; set; }
        /// <summary>
        /// 流程步骤参数配置
        /// </summary>
        public List<SequentialActionConfig> ActionConfigs { get; set; } = [];

        /// <summary>
        /// 平台任务EBR参数配置
        /// </summary>
        public List<EbrParameterConfig> TaskEbrParameterConfigs { get; set; } = [];

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

        public List<StepDisplay> StepDisplays { get; set; } = [];

        public object Clone()
        {
            var clone = new GrpcProjectPlatformFlowConfigOptions
            {
                FlowConfigPath = FlowConfigPath,
                FlowId = FlowId,
                FlowTaskCode = FlowTaskCode,
                FlowTaskId = FlowTaskId,
                PlatformId = PlatformId,
                Flow = Flow,
                FlowTaskDescription = FlowTaskDescription,
                TaskEbrParameterConfigs = TaskEbrParameterConfigs,
                StepDisplays = [.. StepDisplays.Select(p=>(StepDisplay)p.Clone())],
                ActionConfigs = [.. ActionConfigs.Select(x => (SequentialActionConfig)x.Clone())]
            };
            return clone;
        }
    }


    /// <summary>
    /// 工艺流程配置
    /// </summary>
    public class GrpcProjectProcessflowOptions
    {
        public Guid ProcessId { get; set; }

        public string ProcessCode { get; set; } = string.Empty;

        public string ProcessName { get; set; } = string.Empty;

        public string ProcessDescription { get; set; } = string.Empty;


        public ProcessStep  ProcessStep { get; set; }

        /// <summary>
        /// 与平台中的TaskFlowConfigs中的FlowTaskId对应
        /// </summary>
        public List<GrpcProjectPlatformsInOrderConfigOptions> PlatformFlowSteps { get; set; } = [];

        /// <summary>
        /// 工艺流程中的中转步骤配置
        /// </summary>
        public List<GrpcProjectTransferStepOptions> TransferSteps { get; set; } = [];

        /// <summary>
        /// 工艺流程中的直接模块动作步骤配置
        /// </summary>
        public List<GrpcProjectModuleActionStepOptions> ModuleActionSteps { get; set; } = [];

    }

    /// <summary>
    /// 中转步骤配置选项
    /// </summary>
    public class GrpcProjectTransferStepOptions
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
    /// 模块动作步骤配置选项
    /// </summary>
    public class GrpcProjectModuleActionStepOptions
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

    /// <summary>
    /// 中转模块配置
    /// </summary>
    public class GrpcProjectTransferModuleOptions : ICloneable
    {
        /// <summary>
        /// 中转模块id
        /// </summary>
        public long TransferModuleId { get; set; }

        /// <summary>
        /// 中转模块名称
        /// </summary>
        public string TransferModuleName { get; set; } = string.Empty;

        /// <summary>
        /// 左关联平台id
        /// </summary>
        public long LeftPlatformId { get; set; }

        /// <summary>
        /// 右关联平台id
        /// </summary>
        public long RightPlatformId { get; set; }

        /// <summary>
        /// 中转模块进样通量
        /// </summary>
        public int TransferModuleSamplingFlux { get; set; }

        /// <summary>
        /// 中转信息id
        /// </summary>
        public Guid TransferModuleInfoId { get; set; }

        /// <summary>
        /// 中转左平台通道组id
        /// </summary>
        public Guid LeftChannelGroupId { get; set; }

        /// <summary>
        /// 中转右平台通道组id
        /// </summary>
        public Guid RightChannelGroupId { get; set; }

        /// <summary>
        /// 正向移动
        /// </summary>
        public Guid TransferForwardMoveId { get; set; }

        /// <summary>
        /// 反向移动
        ///</summary>
        public Guid TransferBackwardMoveId { get; set; }

        /// <summary>
        /// 是否反转
        /// </summary>
        public bool IsReverse { get; set; } = false;

        public object Clone()
        {
            var clone = new GrpcProjectTransferModuleOptions
            {
                TransferModuleId = TransferModuleId,
                TransferModuleName = TransferModuleName,
                LeftPlatformId = LeftPlatformId,
                RightPlatformId = RightPlatformId,
                LeftChannelGroupId = LeftChannelGroupId,
                RightChannelGroupId = RightChannelGroupId,
                IsReverse = IsReverse,
                TransferBackwardMoveId = TransferBackwardMoveId,
                TransferForwardMoveId = TransferForwardMoveId,
                TransferModuleInfoId = TransferModuleInfoId,
                TransferModuleSamplingFlux = TransferModuleSamplingFlux,
            };
            return clone;
        }
    }

    /// <summary>
    /// 节点步骤配置
    /// </summary>
    public class GrpcProjectflowStepOptions:ICloneable
    {
        public Guid StepId { get; set; }
        public string StepCode { get; set; } = string.Empty;
        public string StepName { get; set; } = string.Empty;

        public object Clone()
        {
            return new GrpcProjectflowStepOptions 
            {
                 StepId = StepId,
                 StepCode = StepCode,
                 StepName = StepName,
            };
        }
    }

    /// <summary>
    /// 产线配置
    /// </summary>
    public class GrpcProjectProductlineOptions
    {
        public long ProductlineId { get; set; }

        public string ProductlineCode { get; set; } = string.Empty;

        public string ProductlineName { get; set; } = string.Empty;

        public string ProductlineDescription { get; set; } = string.Empty;

        public List<Guid> ProcessflowIds { get; set; } = [];

    }

}
