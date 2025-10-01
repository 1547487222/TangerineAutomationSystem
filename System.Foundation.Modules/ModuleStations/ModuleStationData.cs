using QStandaedPlatform.Engine.Common.Common;
using QStandaedPlatform.Engine.Common.Common.Metadatas;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Foundation.Modules.ModuleStations
{

    public class ModuleStationData
    {
        [DisplayName("开始位置")]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public QPosition StartPosition { get; set; } = new QPosition();

        [DisplayName("X间距")]
        public float Space { get; set; }

        [DisplayName("Y间距")]
        public float YSpace { get; set; }

    }
}
