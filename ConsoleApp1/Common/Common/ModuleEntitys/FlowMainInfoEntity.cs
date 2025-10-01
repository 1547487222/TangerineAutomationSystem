using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common.ModuleEntitys
{
    [Table("t_flow_info",Schema = "flowmaininfo")]
    public class FlowMainInfoEntity : RunDataEntity<int>
    {
        [NotNull]
        public string FlowId { get; set; }
        [StringLength(1024)]
        public string? FlowName { get; set; }

        /// <summary>
        /// 模块中文名称
        /// </summary>
       [StringLength(255)]
        public string? ModuleName { get; set; } = string.Empty;
        /// <summary>
        /// 模块编号
        /// </summary>
        [StringLength(100)] public string? ModuleIdentifier { get; set; } = string.Empty;
        /// <summary>
        /// 模块序列号
        /// </summary>
        [StringLength(100)] public string? ModuleSerialNumber { get; set; } = string.Empty;

      

    }
}
