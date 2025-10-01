using QStandaedPlatform.Engine.Common.Common.Metadatas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common
{
    public abstract class ToolBase : Tool
    {
        public override Task<bool> RequestRecvHandlePinAsync(ToolExecutionContext toolContext, PinDataTransmitEventArgs pinDataTransmitEventArgs)
        {
            if (ToolExecuter is ToolExecuter toolExecuter)
            {
                toolExecuter.Enqueue(pinDataTransmitEventArgs);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
        public abstract Task<bool> ExecuteAsync(PinInfo pinInfo, QData pinData, ToolExecutionContext toolContext);

        public override Task<bool> ClearEphemeralDataAsync()
        {
            ToolExecutionContext.ClearPinCache();
            return Task.FromResult(true);
        }
    }
}
