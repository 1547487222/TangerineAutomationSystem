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
    [DisplayName("脉冲输出工具")]
    public class PlusOutputTool : ToolBase
    {
        public class PlusOutputData
        {
            [DisplayName("软元件地址")]
            public string Element { get; set; }
            [DisplayName("脉冲时间/毫秒")]
            public ushort PulsTime { get; set; } = 1000;
        }

        public override bool InitPins()
        {
            InsetPin("触发脉冲信号",this,typeof(QData), PinType.Input);
            InsetPin("输出完成信号",this,typeof(QData), PinType.Output);
            return base.InitPins();
        }
        public override bool InitDataContext()
        {
            DataContext=new PlusOutputData();
            return base.InitDataContext();
        }
        public override string DefineName => "脉冲输出工具";
        [ReferencePart]
        public IH5uTcp h5UTcp { get; set; }
        public override async Task<bool> ExecuteAsync(PinInfo pinInfo, QData pinData, ToolExecutionContext toolContext)
        {
            if(h5UTcp==null)
                throw new Exception("未找到H5uTcp");
            var result= await h5UTcp.PlusOutputAsync(Context<PlusOutputData>().Element, Context<PlusOutputData>().PulsTime);
            SendToPin("输出完成信号", new QData());
            return result;
        }
    }
}
