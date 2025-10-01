using QStandaedPlatform.Engine.Common.Common;
using QStandaedPlatform.Engine.Common.Common.Metadatas;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Foundation.Modules.TaskCounters;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Foundation.Modules.Loops
{
    [DisplayName("For循环模块")]
    public class ForLoopTool : SyncInputToolBase
    {
        public override string DefineName => "For循环模块";

        public override bool InitPins()
        {

            return base.InitPins();
        }


        public override Task<bool> ExexuteAsync(Dictionary<PinInfo, QData> pinDatas, ToolExecutionContext toolContext)
        {
           return Task.FromResult(true);
        }
    }
}
