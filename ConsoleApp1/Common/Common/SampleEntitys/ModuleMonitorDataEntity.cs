using QStandaedPlatform.Engine.Common.Common.Metadatas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common.SampleEntitys
{
     public class ModuleMonitorDataEntity
    {
        public string ModuleId { get; set; } = string.Empty;
        public string ModuleName { get; set; } = string.Empty;
        public string ModuleActionDescription { get; set; } = string.Empty;
        public string MonitorKey { get; set; } = string.Empty;

        public string MonitorValue { get; set; } = string.Empty;

        public string MonitorUnit { get; set; } = string.Empty;

        public DateTime CreateTime { get; set; } = DateTime.Now;
    }
}
