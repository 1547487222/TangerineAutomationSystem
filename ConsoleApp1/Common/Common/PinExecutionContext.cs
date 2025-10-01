using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common
{
    public class PinExecutionContext(ToolExecutionContext toolExecutionContext, PinInfo pinInfo)
    {
        public ToolExecutionContext ToolExecutionContext { get; } = toolExecutionContext;

        public IDataTransmit DataTransmit { get; } = pinInfo;


        public string PinName { get; } = pinInfo.Name;

    }
}
