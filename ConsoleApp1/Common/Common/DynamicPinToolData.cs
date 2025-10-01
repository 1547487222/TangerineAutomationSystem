using System.ComponentModel;

namespace QStandaedPlatform.Engine.Common.Common
{
    public class DynamicPinToolData
    {
        [Browsable(false)]
        public bool IsUpdateInput { get; set; } = true;

        [Browsable(false)]
        public int UpdateInputIndex { get; set; } = -1;
   
        [Browsable(false)]
        public bool IsUpdateOutput { get; set; } = true;
  
        [Browsable(false)]
        public int UpdateOutputIndex { get; set; } = -1;
    }
}
