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
    [DisplayName("堆栈容器工具")]
    public class StackContainerTool : ToolBase
    {
        public override string DefineName => "堆栈容器工具";


        public Stack<QDynamic> Stack = new();
        public override bool InitPins()
        {
            InsetPin("数据压栈",this,typeof(QDynamic),PinType.Input);
            InsetPin("触发出栈信号", this, typeof(QData), PinType.Input);
            InsetPin("数据出栈",this,typeof(QDynamic),PinType.Output);
            return true;
        }
        public override Task<bool> ExecuteAsync(PinInfo pinInfo, QData pinData, ToolExecutionContext toolContext)
        {
            if (pinInfo.Name == "数据压栈")
            {
                lock (Stack)
                {
                    if (pinData is QDynamic qDynamic)
                    {
                        Stack.Push(qDynamic);
                        return Task.FromResult(true);
                    }
                }
            }
            else if (pinInfo.Name == "触发出栈信号")
            {
                lock (Stack)
                {
                    if (Stack.Count > 0)
                    {
                        var dynamic = Stack.Pop();
                        SendToPin("数据出栈",dynamic);
                        return Task.FromResult(true);
                    }
                }
            }
            return Task.FromResult(false);
        }
    }
}
