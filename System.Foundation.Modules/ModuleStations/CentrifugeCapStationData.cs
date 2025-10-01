using QStandaedPlatform.Engine.Common.Common;
using QStandaedPlatform.Engine.Common.Common.Metadatas;
using System.ComponentModel;

namespace System.Foundation.Modules.ModuleStations
{
    public class CentrifugeCapStationData
    {
        [DisplayName("模块位置1"), TypeConverter(typeof(ExpandableObjectConverter))]
        public QPosition InputPos { get; set; } = new QPosition();
        [DisplayName("模块位置2"), TypeConverter(typeof(ExpandableObjectConverter))]
        public QPosition LoadPos { get; set; } = new QPosition();
    }
}
