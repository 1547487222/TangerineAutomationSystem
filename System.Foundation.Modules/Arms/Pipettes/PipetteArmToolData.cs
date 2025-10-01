using QStandaedPlatform.Engine.Common.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Foundation.Modules.Arms.Pipettes
{
    public class PipetteArmToolData : ModuleData
    {
        [DisplayName("X轴偏移")]
        public float XOffset { get; set; } = 0;

        [DisplayName("Y轴偏移")]
        public float YOffset { get; set; } = 0;

        [DisplayName("Z轴偏移")]
        public float ZOffset { get; set; } = 0;

        [DisplayName("Z2轴偏移")]
        public float Z2Offset { get; set; } = 0;

    }


}
