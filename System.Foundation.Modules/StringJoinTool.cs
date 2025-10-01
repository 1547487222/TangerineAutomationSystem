using QStandaedPlatform.Engine.Common.Common;
using QStandaedPlatform.Engine.Common.Common.Metadatas;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Foundation.Modules
{
    //[DisplayName("字符串拼接")]
    //public class StringJoinTool : SyncInputToolBase
    //{
    //    public override string DefineName => "字符串拼接";

    //    public override bool InitPins()
    //    {
    //        InsetPin("接收字符串1",this,typeof(QString), PinType.Input);
    //        InsetPin("接收字符串2", this, typeof(QString), PinType.Input);
    //        InsetPin("拼接结果", this, typeof(QString), PinType.Output);
    //        return base.InitPins();
    //    }
    //    public override Task<bool> ExexuteAsync(Dictionary<PinInfo, QData> pinDatas, ToolExecutionContext toolContext)
    //    {
    //       var string1 = (string)pinDatas.FirstOrDefault(p=> p.Key.Name=="接收字符串1").Value;
    //        var string2 = (string)pinDatas.FirstOrDefault(p=> p.Key.Name=="接收字符串2").Value;
    //        var result = string1 + string2;
    //        SendToPin("拼接结果", (QString)result);
    //        return Task.FromResult(true);
    //    }
    //}
}
