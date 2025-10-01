using Microsoft.Identity.Client;
using QStandaedPlatform.Engine.Common.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Laboratory
{

    public interface IOpenable
    {
        bool Openable { get; }
    }

    public enum EbrType
    {
        REAL,
        INT,
        DINT,
        BOOL,
        STRING
    }

    /// <summary>
    /// 实验室配置
    /// </summary>
    public class LaboratoryConfig
    {
        /// <summary>
        /// 实验室Id
        /// </summary>
        public long LaboratoryId { get; set; }
        /// <summary>
        /// 实验室编号
        /// </summary>
        public string LaboratoryCode { get; set; }= string.Empty;
        /// <summary>
        /// 实验室名称
        /// </summary>
        public string LaboratoryName { get; set; } = string.Empty;
        /// <summary>
        /// 实验室描述
        /// </summary>
        public string LaboratoryDescription { get; set; } = string.Empty;
        /// <summary>
        /// 产线配置
        /// </summary>
        public List<ProductionlineConfig> ProductionlineConfigs { get; set; } = [];
    }


    /// <summary>
    /// 产线
    /// </summary>
    public class ProductionlineConfig
    {
        /// <summary>
        /// 产线Id
        /// </summary>
        public long LineId { get; set; }
        /// <summary>
        /// 产线编号
        /// </summary>
        public string LineCode { get; set; } = string.Empty;
        /// <summary>
        /// 产线名称
        /// </summary>
        public string LineName { get; set; } = string.Empty;
        /// <summary>
        /// 产线描述
        /// </summary>
        public string LineDescription { get; set; } = string.Empty;
        /// <summary>
        /// 产线的工艺流程组配置
        /// </summary>
        public List<ProcessflowConfig> ProcessflowConfigs { get; set; } = [];

        /// <summary>
        /// 产线的平台配置
        /// </summary>
        public List<PlatformConfig> PlatformConfigs { get; set; } = [];

        /// <summary>
        /// 产线的中转配置
        /// </summary>
        public List<TransferModuleConfig> TransferModuleConfigs { get; set; } = [];
    }


    /// <summary>
    /// 表示对实验室设备执行的具体操作类型
    /// </summary>
    public enum PlatformOperation
    {
        // ========== 管路操作类 ==========
        /// <summary>
        /// 管路排空（排出液体或气体）
        /// </summary>
        [Description("管路排空")]
        PurgeLines = 10,

        // ========== 仪器校准类 ==========
        /// <summary>
        /// 仪器校准（通用）
        /// </summary>
        [Description("仪器校准")]
        Calibrate = 20,
    }

    /// <summary>
    /// 表示样品在实验室自动化系统中的处理流程阶段
    /// </summary>
    public enum ProcessStep
    {

        // ========== 管路操作类 ==========
        /// <summary>
        /// 管路排空（排出液体或气体）
        /// </summary>
        [Description("管路排空")]
        PurgeLines = 10,

        // ========== 仪器校准类 ==========
        /// <summary>
        /// 仪器校准（通用）
        /// </summary>
        [Description("仪器校准")]
        Calibrate = 20,

        /// <summary>
        /// 分样流程：将原始样品分配到多个容器中
        /// </summary>
        [Description("分样流程")]
        Sampling = 1,

        /// <summary>
        /// 前处理流程：包括萃取、稀释、过滤、等样品准备操作
        /// </summary>
        [Description("前处理流程")]
        Pretreatment = 2,

        /// <summary>
        /// 检测流程：如色谱、质谱、光谱等仪器分析
        /// </summary>
        [Description("检测流程")]
        Analysis = 3,
    }

    /// <summary>
    /// 工艺流程
    /// </summary>
    public class ProcessflowConfig
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
        /// 工艺流程类型
        /// </summary>
        public ProcessStep ProcessType { get; set; } = ProcessStep.Pretreatment;
        /// <summary>
        /// 工艺流程内平台任务流程配置
        /// </summary>
        public List<PlatformTaskProfile> PlatformTaskConfigs { get; set; } = [];

        /// <summary>
        /// 工艺流程内中转模块传输步骤配置
        /// </summary>
        public List<TransferStepConfig> TransferStepConfigs { get; set; } = [];

        /// <summary>
        /// 工艺流程内直接模块动作步骤配置
        /// </summary>
        public List<ModuleActionStepConfig> ModuleActionStepConfigs { get; set; } = [];
    }

    /// <summary>
    /// 中转模块配置
    /// </summary>
    public class TransferModuleConfig
    {
        /// <summary>
        /// 中转模块Id
        /// </summary>
        public long TransferModuleId { get; set; }
        /// <summary>
        /// 中转模块名称
        /// </summary>
        public string TransferModuleName { get; set; } = string.Empty;
        /// <summary>
        /// 左关联平台Id
        /// </summary>
        public long LeftPlatformId { get; set; }
        /// <summary>
        /// 右关联平台Id
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
    }

    /// <summary>
    /// 中转步骤配置（用于工艺流程中的中转操作）
    /// </summary>
    public class TransferStepConfig
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
        /// 中转方向（Forward=正向, Backward=反向）
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
    /// 中转方向
    /// </summary>
    public enum TransferDirection
    {
        /// <summary>
        /// 正向移动
        /// </summary>
        [Description("正向移动")]
        Forward = 0,
        /// <summary>
        /// 反向移动
        /// </summary>
        [Description("反向移动")]
        Backward = 1
    }

    /// <summary>
    /// 模块动作步骤配置（用于工艺流程中直接调用模块动作）
    /// </summary>
    public class ModuleActionStepConfig
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

    public class PlatformOperationConfig
    {
        /// <summary>
        /// 平台操作Id
        /// </summary>
        public Guid OperationId { get; set; }
        /// <summary>
        /// 平台操作名称
        /// </summary>
        public string OperationName { get; set; } = string.Empty;

        /// <summary>
        /// 平台操作描述
        /// </summary>
        public string OperationDescription { get; set; } = string.Empty;

        /// <summary>
        /// 平台操作类型
        /// </summary>
        public PlatformOperation OperationType { get; set; }

        /// <summary>
        /// 平台动作配置集
        /// </summary>
        public List<SequentialActionConfig> ActionConfigs { get; set; } = [];
    }

    /// <summary>
    /// 平台配置         
    /// </summary>
    public class PlatformConfig
    {
        /// <summary>
        /// 平台Id
        /// </summary>
        public long PlatformId { get; set; }
        /// <summary>
        /// 平台编号
        /// </summary>
        public string PlatformCode { get; set; } = string.Empty;
        /// <summary>
        /// 平台名称
        /// </summary>
        public string PlatformName { get; set; } = string.Empty;
        /// <summary>
        /// 平台描述
        /// </summary>
        public string PlatformDescription { get; set; } = string.Empty;
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
        ///平台操作任务配置
        /// </summary>
        public List<PlatformOperationConfig> PlatformOperationConfigs { get; set; } = [];
        /// <summary>
        /// 平台初始化任务配置
        /// </summary>
        public List<PlatformTaskProfile> InitTaskProfiles { get; set; } = [];
        /// <summary>
        /// 平台实验前准备任务配置
        /// </summary>
        public List<PlatformTaskProfile> PrepareExperimentTaskProfiles { get; set; } = [];
        /// <summary>
        /// 平台实验任务配置
        /// </summary>
        public List<PlatformTaskProfile> ExperimentTaskProfiles { get; set; } = [];
        /// <summary>
        /// 平台系统封存任务配置
        /// </summary>
        public List<PlatformTaskProfile> SystemStorageTaskProfiles { get; set; } = [];
        /// <summary>
        /// 平台实验收尾任务配置
        /// </summary>
        public List<PlatformTaskProfile> FinalizeTaskProfiles { get; set; } = [];

        /// <summary>
        /// 平台监控参数配置集
        /// </summary>
        public List<MonitorParameterConfig> MonitorParameterConfigs { get; set; } = [];
        /// <summary>
        /// 平台托盘配置集
        /// </summary>
        public List<LabTrayConfiguration> LabTrayConfigurations { get; set; } = [];
    }
    /// <summary>
    /// 平台任务配置文件
    /// </summary>
    public class PlatformTaskProfile
    {
        /// <summary>
        /// 平台Id
        /// </summary>
        public long PlatformId { get; set; }
        /// <summary>
        /// 平台任务Id
        /// </summary>
        public long PlatformTaskId { get; set; }
        /// <summary>
        /// 平台任务编号
        /// </summary>
        public string PlatformTaskCode { get; set; } = string.Empty;
        /// <summary>
        /// 平台任务描述
        /// </summary>
        public string PlatformTaskDescription { get; set; } = string.Empty;
        /// <summary>
        /// 步骤执行顺序（用于工艺流程中的排序，可选）
        /// </summary>
        public int StepOrder { get; set; }
        /// <summary>
        /// 平台动作配置集
        /// </summary>
        public List<SequentialActionConfig> ActionConfigs { get; set; } = [];

        /// <summary>
        /// 平台任务Ebr参数配置集
        /// </summary>
        public List<EbrParameterConfig> TaskEbrParameterConfigs { get; set; } = [];
    }
    /// <summary>
    /// 有序动作配置
    /// </summary>
    public class SequentialActionConfig:ICloneable
    {
        // <summary>
        /// 执行顺序 序号
        /// </summary>
        public int Sequential { get; set; }
        /// <summary>
        /// 模块名称
        /// </summary>
        public string ModuleName { get; set; } = string.Empty;
        /// <summary>
        /// 模块序列号
        /// </summary>
        public string ModuleSerialNumber { get; set; } = string.Empty;
        /// <summary>
        /// 动作描述
        /// </summary>
        public string ActionDescription { get; set; } = string.Empty;
        /// <summary>
        /// 动作名称
        /// </summary>
        public string ActionName { get; set; } = string.Empty;
        /// <summary>
        /// 动作唯一标识
        /// </summary>
        public Guid ActionId { get; set; }
        /// <summary>
        /// 参数列表
        /// </summary>
        public List<ParameterItem> Parameters { get; set; } = [];

        public object Clone()
        {
            var clone = new SequentialActionConfig
            {
                Sequential = Sequential,
                ModuleName = ModuleName,
                ModuleSerialNumber = ModuleSerialNumber,
                ActionDescription = ActionDescription,
                ActionName = ActionName,
                ActionId = ActionId,
                Parameters = [.. Parameters.Select(p => (ParameterItem)p.Clone())]
            };
            return clone;
        }
    }

    /// <summary>
    /// 参数项
    /// </summary>
    public class ParameterItem:ICloneable
    {
        /// <summary>
        /// 参数唯一标识（ID）
        /// </summary>
        public long ParameterId { get; set; }
        /// <summary>
        /// 参数名称
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// 参数描述
        /// </summary>
        public string Description { get; set; } = string.Empty;
        /// <summary>
        /// 参数值
        /// </summary>
        public float Value { get; set; } = 0f;
        /// <summary>
        /// 参数单位
        /// </summary>
        public string Unit { get; set; } = string.Empty;
        /// <summary>
        /// 允许的最大值
        /// </summary>
        public float MaxValue { get; set; }
        /// <summary>
        /// 允许的最小值
        /// </summary>
        public float MinValue { get; set; }

        public object Clone()
        {
            var clone = new ParameterItem
            {
                ParameterId = ParameterId,
                Name = Name,
                Description = Description,
                Value = Value,
                Unit = Unit,
                MaxValue = MaxValue,
                MinValue = MinValue
            };
            return clone;
        }
    }

    /// <summary>
    /// 监控参数配置
    /// </summary>
    public class MonitorParameterConfig
    {
        /// <summary>
        /// 监控模块名称
        /// </summary>
        public string ModuleName { get; set; } = string.Empty;
        /// <summary>
        /// 监控项名称
        /// </summary>
        public string MonitorName { get; set; } = string.Empty;
        /// <summary>
        /// 监控项描述
        /// </summary>
        public string MonitorDescription { get; set; } = string.Empty;
        /// <summary>
        /// 监控项数据类型
        /// </summary>
        public ReadType MonitorType { get; set; }
        /// <summary>
        /// 监控项单位
        /// </summary>
        public string MonitorUnit { get; set; } = string.Empty;
    }

    /// <summary>
    /// Ebr参数配置
    /// </summary>
    public class EbrParameterConfig
    {
        /// <summary>
        /// Ebr模块动作Id
        /// </summary>
        public string ModuleActionId { get; set; } = string.Empty;
        /// <summary>
        /// 模块动作描述
        /// </summary>
        public string ModuleActionDescription { get; set; } = string.Empty;
        /// <summary>
        /// 模块名称
        /// </summary>
        public string ModuleName { get; set; } = string.Empty;
        /// <summary>
        /// Ebr名称
        /// </summary>
        public string EbrName { get; set; } = string.Empty;
        /// <summary>
        /// Ebr描述
        /// </summary>
        public string EbrDescription { get; set; } = string.Empty;
        /// <summary>
        /// Ebr单位
        /// </summary>
        public string EbrUnit { get; set; } = string.Empty;
        /// <summary>
        /// Ebr数据类型
        /// </summary>
        public EbrType EbrType { get; set; }
    }

    /// <summary>
    /// 托盘区域默认设置类型
    /// </summary>
    [Flags]
    public enum LabTrayDefaultType
    {
        /// <summary>
        /// 样品托盘区域
        /// </summary>
        SampleTray = 1<<0,
        /// <summary>
        /// 耗材托盘区域
        /// </summary>
        ConsumableTray = 1<<1,
        /// <summary>
        /// 上料托盘区域
        /// </summary>
        LoadTray = 1<<2,
        /// <summary>
        /// 下料托盘区域
        /// </summary>
        UnloadTray = 1<<3,
        /// <summary>
        /// 空托盘区域
        /// </summary>
        EmptyTray = 1<<4,
        /// <summary>
        /// 废料托盘区域
        /// </summary>
        ScrapTray = 1<<5,
        /// <summary>
        /// 固定托盘区域
        /// </summary>
        FixedTray = 1<<6,
        /// <summary>
        /// 不可用托盘区域
        /// </summary>
        UnavailableTray = 1<<7,
        /// <summary>
        /// 标准液托盘区域
        /// </summary>
        StandardLiquidTray = 1<<8,
        /// <summary>
        /// 枪头托盘区域
        /// </summary>
        TipTray = 1<<9,
        /// <summary>
        /// 校准液托盘区域
        /// </summary>
        CalibrationLiquidTray = 1<<10,
    }
    /// <summary>
    /// 平台托盘配置
    /// </summary>
    public class LabTrayConfiguration
    {
        /// <summary>
        /// 托盘Id
        /// </summary>
        public long LabTrayId { get; set; }
        /// <summary>
        /// 托盘名称
        /// </summary>
        public string LabTrayName { get; set; } = string.Empty;
        /// <summary>
        /// 托盘类别
        /// </summary>
        public string LabTrayCategory { get; set; } = string.Empty;
        /// <summary>
        /// 托盘编码
        /// </summary>
        public string LabTrayCode { get; set; } = string.Empty;
        /// <summary>
        /// 托盘行数
        /// </summary>
        public int TotalRows { get; set; } = 0;
        /// <summary>
        /// 托盘列数
        /// </summary>
        public int TotalCols { get; set; } = 0;

        /// <summary>
        /// 托盘孔位信息
        /// </summary>
        public List<WellInfo>  WellInfos { get; set; } = [];

        /// <summary>
        /// 托盘分段区域
        /// </summary>
        public List<TraySegmentRegion> TraySegmentRegions { get; set; } = [];
    }
   /// <summary>
   /// 托盘分段区域
   /// </summary>
    public class TraySegmentRegion
    {
        /// <summary>
        /// 区域Id
        /// </summary>
        public long TrayRegionId { get; set; }
        /// <summary>
        /// 托盘区域名称
        /// </summary>
        public string TrayRegionName { get; set; } = string.Empty;
        /// <summary>
        /// 托盘区域行数
        /// </summary>
        public int RegionRows { get; set; }
        /// <summary>
        /// 托盘区域列数
        /// </summary>
        public int RegionCols { get; set; }

        /// <summary>
        /// 托盘区域孔位信息
        /// </summary>
        public List<WellInfo>  RegionWellInfos { get; set; } = [];

        /// <summary>
        /// 托盘区域初始类型
        /// </summary>
        public LabTrayDefaultType TrayRegionType { get; set; } = LabTrayDefaultType.ConsumableTray;

        public override string ToString()
        {
            return @$"TrayRegionId:{TrayRegionId},
            TrayRegionName:{TrayRegionName},RegionRows:{RegionRows},
            RegionCols:{RegionCols},
            RegionWellInfos:{string.Join("|", RegionWellInfos.Select(x => x.ToString()).ToList())},
            TrayRegionType:{TrayRegionType}";
        }
    }


    /// <summary>
    /// 孔位信息
    /// </summary>
    public class WellInfo
    {
        public string WellName { get; set; } = string.Empty;
        public ExternalWellStatus WellStatus { get; set; }
        public long MaterialId { get; set; }

        public override string ToString()
        {
            return $"WellName:{WellName},WellStatus:{WellStatus},MaterialId:{MaterialId}";
        }
    }

    /// <summary>
    /// 外部孔位状态
    /// </summary>
    public enum ExternalWellStatus
    {
        /// <summary>
        /// 空闲
        /// </summary>
        [Description("空闲")]
        Idle = 1,
        /// <summary>
        /// 可使用
        /// </summary>
        [Description("可使用")]
        Available,
        /// <summary>
        /// 已使用
        /// </summary>
        [Description("已使用")]
        Used,
    }

    /// <summary>
    /// 托盘信息
    /// </summary>
    public class LabTrayInfo
    {
        public string LabTrayName { get; set; } = string.Empty;
        public string LabTrayCode { get; set; } = string.Empty;
        public long LabTrayId { get; set; }
        public string LabTrayCategory { get; set; } = string.Empty;
        public List<WellInfo> WellInfos { get; set; } = [];
        /// <summary>
        /// 是否最终托盘
        /// </summary>
        public bool IsFinalTray { get; set; } = false;

        public override string ToString()
        {
            return $"LabTrayName:{LabTrayName},LabTrayCode:{LabTrayCode},LabTrayId:{LabTrayId},LabTrayCategory:{LabTrayCategory},WellInfos:{string.Join("|", WellInfos.Select(x => x.ToString()).ToList())} IsFinalTray:{IsFinalTray}";
        }
    }

    /// <summary>
    /// 记录托盘在初始化阶段，各孔位首次扫码绑定信息（仅记录第一次）
    /// </summary>
    public class TrayInitialBindingInfo
    {
        public string LabTrayName { get; set; } = string.Empty;
        public string LabTrayCode { get; set; } = string.Empty;
        public long LabTrayId { get; set; }
        public List<WellInitialBindingInfo> WellBindings { get; set; } = [];

        public override string ToString()
        {
            return $"LabTrayName:{LabTrayName},LabTrayCode:{LabTrayCode},LabTrayId:{LabTrayId},WellBindings:{string.Join("|", WellBindings.Select(x => x.ToString()))}";
        }
    }
    /// <summary>
    /// 孔位首次扫码绑定信息（仅记录第一次，后续不更新）
    /// </summary>
    public class WellInitialBindingInfo
    {
        /// <summary>
        /// 孔位名称
        /// </summary>
        public string WellName { get; set; } = string.Empty;

        /// <summary>
        /// 绑定的材料ID
        /// </summary>
        public long MaterialId { get; set; }

        /// <summary>
        /// 扫码编号
        /// </summary>
        public string QrCode { get; set; } = string.Empty;
    }


    /// <summary>
    /// 器皿信息
    /// </summary>
    public class LabwareInfo
    {
        /// <summary>
        /// /器皿名称
        /// </summary>
        public string LabwareName { get; set; } = string.Empty;
        /// <summary>
        /// 器皿编码
        /// </summary>
        public string QrCode { get; set; } = string.Empty;
        /// <summary>
        /// 器皿从属托盘
        /// </summary>
        public long OwnerLabTrayId { get; set; }
        /// <summary>
        /// 器皿从属托盘位置标签
        /// </summary>
        public string OwnerLabTrayPositionLabel { get; set; } = string.Empty;
        /// <summary>
        /// 材料信息
        /// </summary>
        public MaterialInfo? MaterialInfo { get; set; }
    }

    /// <summary>
    /// 材料信息
    /// </summary>
    public class MaterialInfo
    {
        /// <summary>
        /// 材料编号
        /// </summary>
        public long MaterialNo { get; set; }
    }


    /// <summary>
    /// 材料类别信息
    /// </summary>
    public class MaterialTypeInfo
    {
        /// <summary>
        /// 材料类别
        /// </summary>
        public string MaterialType { get; set; } = string.Empty;
    }



}



