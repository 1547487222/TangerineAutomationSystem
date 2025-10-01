using QStandaedPlatform.Engine.Common.Common;
using QStandaedPlatform.Engine.Common.Common.Metadatas;
using QStandaedPlatform.Engine.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Foundation.Modules.ManualControls
{
    [DisplayName("H5uPlc写入值")]
    public class H5uValueWirterTool : ToolBase
    {
        public H5uValueWirterData h5UValueWirterData = new H5uValueWirterData();
        public class H5uValueWirterData
        {
            public int IntValue { get; set; }

            public string IntValueAddress { get; set; }
            public bool BoolValue { get; set; }

            public string BoolValueAddress { get; set; }
            public short ShortValue { get; set; }

            public string ShortValueAddress { get; set; }
        }
        public override string DefineName => "H5uPlc写入值";

        [ReferencePart]
        public IH5uTcp   h5UTcp { get; set; }

        public override bool InitPins()
        {

            InsetPin("触发写入单个Int值", this, typeof(QData), PinType.Input);
            InsetPin("触发写入单个Bool值", this, typeof(QData), PinType.Input);
            InsetPin("触发写入单个Short值", this, typeof(QData), PinType.Input);

            InsetPin("写入单个Int值", this, typeof(QData), PinType.Input);
            InsetPin("写入单个Bool值", this, typeof(QData), PinType.Input);
            InsetPin("写入单个Short值", this, typeof(QData), PinType.Input);

            InsetPin("输出写入单个Int值完成信号", this, typeof(QData), PinType.Output);
            InsetPin("输出写入单个Bool值完成信号", this, typeof(QData), PinType.Output);
            InsetPin("输出写入单个Short值完成信号", this, typeof(QData), PinType.Output);
            return base.InitPins();
        }
        public override bool InitDataContext()
        {
            DataContext = h5UValueWirterData;
            return true;
        }
        public override async Task<bool> ExecuteAsync(PinInfo pinInfo, QData pinData, ToolExecutionContext toolContext)
        {
            h5UValueWirterData = Context<H5uValueWirterData>();
            if (pinInfo.Name == "触发写入单个Int值")
            {
                bool ret = await h5UTcp?.WriteSingleValueAsync(h5UValueWirterData.IntValueAddress, h5UValueWirterData.IntValue);
                if (ret)
                {
                    SendToPin("输出写入单个Int值完成信号", new QData());
                    return ret;
                }
            }
            else if (pinInfo.Name == "触发写入单个Bool值")
            {
                bool ret = await h5UTcp?.WriteSingleValueAsync(h5UValueWirterData.BoolValueAddress, h5UValueWirterData.BoolValue);
                if (ret)
                {
                    SendToPin("输出写入单个Bool值完成信号", new QData());
                    return ret;
                }
            }
            else if (pinInfo.Name == "触发写入单个Short值")
            {
                bool ret = await h5UTcp?.WriteSingleValueAsync(h5UValueWirterData.ShortValueAddress, h5UValueWirterData.ShortValue);
                if (ret)
                {
                    SendToPin("输出写入单个Short值完成信号", new QData());
                    return ret;
                }
            }
            return false;
        }
    }
}
