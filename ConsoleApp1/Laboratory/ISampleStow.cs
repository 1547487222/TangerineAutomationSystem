using QStandaedPlatform.Engine.Common.Common.Metadatas;
using QStandaedPlatform.Engine.Laboratory.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Laboratory
{
    public interface ISampleStow
    {
         QLabware Labware { get; set; }

         QPosition Position { get; set; }

         ClawGraspInfo ClawSetting { get; set; }
    }
}
