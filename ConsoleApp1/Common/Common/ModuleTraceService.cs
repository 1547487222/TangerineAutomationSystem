using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common
{
    public static class ModuleTraceService
    {

        public static void WriteTrace(ModuleTraceEntity moduleTraceEntity)
        {
            
        }
    }

    public class ModuleTraceEntity
    {
        public enum ModuleTraceStatus
        {
            Running,
            Success,
            Failed,
            Cancelled
        }

        /// <summary>
        /// 平台ID
        /// </summary>
        public long PlatformId { get; set; }

        /// <summary>
        /// 平台任务ID
        /// </summary>
        public long PlatformTaskId { get; set; }
        /// <summary>
        /// 进样任务ID
        /// </summary>
        public string SamplingTaskId { get; set; } = string.Empty;
        /// <summary>
        /// 平台名称
        /// </summary>
        public string PlatformName { get; set; } = string.Empty;
        /// <summary>
        /// 模块Id
        /// </summary>
        public string ModuleId { get; set; } = string.Empty;
        /// <summary>
        /// 设备名称
        /// </summary>
        public string ModuleName { get; set; } = string.Empty;
        /// <summary>
        /// 输入参数
        /// </summary>
        public string JsonInputParameter { get; set; } = string.Empty;
        /// <summary>
        /// 模块动作描述
        /// </summary>
        public string ModuleActionDescription { get; set; } = string.Empty;
        /// <summary>
        /// 模块状态
        /// </summary>
        public ModuleTraceStatus  Status { get; set; }

        //警报信息
        public string AlertMessage { get; set; } = string.Empty;
    }

    
}
