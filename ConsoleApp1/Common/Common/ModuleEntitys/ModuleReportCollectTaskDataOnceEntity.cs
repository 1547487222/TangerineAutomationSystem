using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common.ModuleEntitys
{
    [Table("t_module_report_monitor_taskdata_once")]
    public class ModuleReportMonitorTaskDataOnceEntity : RunDataEntity<long>
    {
        public ModuleReportMonitorTaskDataOnceEntity()
        {
            ModuleReportCollectTaskDataItemEntities = [];
        }
        public List<ModuleReportMonitorTaskDataItemEntity> ModuleReportCollectTaskDataItemEntities { get; set; }

        //public DateTime CreateTime { get; set; } = DateTime.Now;
    }
}
