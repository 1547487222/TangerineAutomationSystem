using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common.ModuleEntitys
{
    [Table("t_module_report_monitor_taskdata_item")]
    public class ModuleReportMonitorTaskDataItemEntity : RunDataEntity<long>
    {
        /// <summary>
        /// 
        /// </summary>
        public string MonitorName { get; set; }
        /// <summary>
        /// 收集项值
        /// </summary>
        public string MonitorValue { get; set; }

        /// <summary>
        /// 收集时间
        /// </summary>
        public DateTime MonitorTime { get; set; }= DateTime.Now;
    }
}
