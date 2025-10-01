using QStandaedPlatform.Engine.Common.Common;
using QStandaedPlatform.Engine.Common.Common.Metadatas;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Foundation.Modules.Masters
{
    [DisplayName("时间延时器")]
    public class TimeDelayTool : ToolBase
    {
        public class TimeDelayData
        {
            public int Interval { get; set; }
        }
        public override string DefineName => "时间延时器";

        public override bool InitPins()
        {
            InsetPin("触发时间延时信号", this, typeof(QDynamic), PinType.Input);
            InsetPin("时间延时完成信号", this, typeof(QDynamic), PinType.Output);
            return base.InitPins();
        }
        public override bool InitDataContext()
        {
            DataContext = new TimeDelayData();
            return true;
        }
        public override async Task<bool> ExecuteAsync(PinInfo pinInfo, QData pinData, ToolExecutionContext toolContext)
        {
            await Task.Delay(Context<TimeDelayData>().Interval, this.RequestCancelToken);
            SendToPin("时间延时完成信号", pinData);
            return true;
        }
    }
}
