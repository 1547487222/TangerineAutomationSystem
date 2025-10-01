using QStandaedPlatform.Engine.Common.Common.Metadatas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common
{

    public delegate Task PinDataTransmitEventHandler(PinDataTransmitEventArgs e);
    public class PinDataTransmitEventArgs : EventArgs
    {
        public Guid  TransmitId { get; set; }= Guid.NewGuid();

        public Tool SourceOwnerTool { get; set; }

        public PinInfo SourcePin { get; set; }

        public PinInfo TargetPin { get; set; }

        public QData PinData { get; set; }

        public DateTime CreateTime { get; set; } = DateTime.Now;

        public override string ToString()
        {
            return $"sourceTool:{SourceOwnerTool.DisplayName}" +
                   $",sourcePin:{SourcePin.Name}" +
                   $",targetPin:{TargetPin.Name}" +
                   $",Data:{PinData.ToString()}";
        }

    }
}
