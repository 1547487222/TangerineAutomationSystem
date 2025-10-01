using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Laboratory
{
    /// <summary>
    /// 平台预实验服务
    /// </summary>
    public class PlatformPreExperimentService
    {

    }

    /// <summary>
    /// 预实验阶段的条件项
    /// </summary>
    public class PreExperimentConditionItem
    {
        /// <summary>
        /// 所属模块名称
        /// </summary>
        public string ModuleName { get; set; } = string.Empty;
        /// <summary>
        /// 操作/动作名称
        /// </summary>
        public string ActionName { get; set; } = string.Empty;
        /// <summary>
        /// 预设值
        /// </summary>
        public float DefaultValue { get; set; } = 0f;
        /// <summary>
        /// 数值上限
        /// </summary>
        public float MaxLimit { get; set; } = 0f;

        /// <summary>
        /// 数值下限
        /// </summary>
        public float MinLimit { get; set; } = 0f;
    }
}
