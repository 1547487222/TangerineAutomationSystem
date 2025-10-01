namespace TangerineBlazorApp.Models
{

    /// <summary>
    /// 流程配置
    /// </summary>
    public class FlowConfig
    {
        public Guid  FlowId { get; set; }

        /// <summary>
        /// 流程名称
        /// </summary>
        public string FlowName { get; set; } = string.Empty;
        /// <summary>
        /// 模块参数集
        /// </summary>
        public List<ModuleConfig> ModuleConfigs { get; set; } = [];
    }
    /// <summary>
    /// 模块配置
    /// </summary>
    public class ModuleConfig
    {
        /// <summary>
        /// 模块名称
        /// </summary>
        public string ModuleName { get; set; } = string.Empty;
        /// <summary>
        /// 模块唯一Id
        /// </summary>
        public Guid ModuleId { get; set; }
        /// <summary>
        /// 模块参数集
        /// </summary>
        public List<ParameterItem> Parameters { get; set; } = [];
    }
    /// <summary>
    /// 参数项
    /// </summary>
    public class ParameterItem
    {
        /// <summary>
        /// 参数名称
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// 参数值工厂 根据输入值动态匹配输出参数值
        /// </summary>
        public Dictionary<float, float> ValueFactory { get; set; } = [];
        /// <summary>
        /// 参数描述
        /// </summary>
        public string Description { get; set; } = string.Empty;
        /// <summary>
        /// 参数单位
        /// </summary>
        public string Unit { get; set; } = string.Empty;
        /// <summary>
        /// 参数最大值
        /// </summary>
        public float MaxValue { get; set; }
        /// <summary>
        /// 参数最小值
        /// </summary>
        public float MinValue { get; set; }
    }
}
