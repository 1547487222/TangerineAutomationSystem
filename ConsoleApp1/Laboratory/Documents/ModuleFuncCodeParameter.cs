using Newtonsoft.Json;
using QStandaedPlatform.Engine.Common.Common;

namespace QStandaedPlatform.Engine.Laboratory.Documents
{
    public class ModuleFuncCodeParameter : IParameter, ICloneable, IOpenable
    {
        public Guid ModuleInfoId { get; set; }
        [JsonIgnore]
        public ModuleInfoParameter? ModuleInfoParameter { get; set; }
        /// <summary>
        /// 功能码
        /// </summary>
        public int FuncCode { get; set; }

        /// <summary>
        /// 是否监控参数反馈
        /// </summary>
        public bool IsMonitorFuncCodeParameterFeedback { get; set; } = false;

        /// <summary>
        /// 功能码参数
        /// </summary>
        public List<FuncCodeParameterInfo> FuncCodeParamterInfos { get; set; } = [];

        /// <summary>
        /// 功能动态值参数
        /// </summary>
        public List<DynamicValueParameter> ActionDynamicValueParameters { get; set; } = [];

        /// <summary>
        /// 模块监控配置
        /// </summary>
        public List<ModuleMonitorInfoItem> MonitorInfoItems { get; set; } = [];

        /// <summary>
        /// 通道Ebr配置
        /// </summary>
        public Dictionary<int, List<ModuleEbrInfoItem>> ChannelEbrInfos { get; set; } = [];
        /// <summary>
        /// 模块精度配置
        /// </summary>
        public List<ModulePrecisionInfoItem> PrecisionInfoItems { get; set; } = [];
        /// <summary>
        /// 功能码描述
        /// </summary>
        public string FuncCodeDescription { get; set; } = string.Empty;

        /// <summary>
        /// 监控间隔/ms
        /// </summary>
        public int MonitorInterval { get; set; } = 1000;
        /// <summary>
        /// EBR开始读取间隔/ms
        /// </summary>
        public int EbrReadStartInterval { get; set; }

        public Guid ParameterId { get; set; }
        /// <summary>
        /// 是否需要参数
        /// </summary>
        public bool RequiresParameter { get; set; } = true;

        public bool Openable { get; set; } = false;

        /// <summary>
        /// 是否产生遗留
        /// </summary>
        public bool IsProductLegacy { get; set; } = false;
        /// <summary>
        /// 功能执行超时时间
        /// </summary>
        public int FuncActionTimeout { get; set; } = 0;

        public object Clone()
        {
            var clone = new ModuleFuncCodeParameter
            {
                ParameterId = Guid.NewGuid(),
                ModuleInfoId = ModuleInfoId,
                FuncCode = FuncCode,
                FuncCodeDescription = FuncCodeDescription + "副本",
                ModuleInfoParameter = ModuleInfoParameter,
                EbrReadStartInterval = EbrReadStartInterval,
                MonitorInterval = MonitorInterval,
                RequiresParameter = RequiresParameter,
                Openable = Openable,
                FuncActionTimeout = FuncActionTimeout,
                ActionDynamicValueParameters = ActionDynamicValueParameters.Select(x => (DynamicValueParameter)x.Clone()).ToList(),
                ChannelEbrInfos = ChannelEbrInfos.ToDictionary(x => x.Key, x => x.Value.Select(y => (ModuleEbrInfoItem)y.Clone()).ToList()),
                FuncCodeParamterInfos = FuncCodeParamterInfos.Select(x => (FuncCodeParameterInfo)x.Clone()).ToList(),
                MonitorInfoItems = MonitorInfoItems.Select(x => (ModuleMonitorInfoItem)x.Clone()).ToList(),
                PrecisionInfoItems = PrecisionInfoItems.Select(x => (ModulePrecisionInfoItem)x.Clone()).ToList()
            };
            return clone;
        }

        public void InitlizeParameter()
        {
           
        }
    }

}



