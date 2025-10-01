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
    public class ContainerToolData
    {
        [DisplayName("容器个数")]
        public int ContainerCount { get; set; }
    }
    [DisplayName("队列容器工具")]
    public class QueueContainerTool : ToolBase
    {
        public readonly Queue<QDynamic> qDynamics = new(); 
        public override string DefineName => "队列容器工具";


        public override bool InitPins()
        {
            InsetPin("数据入队",this,typeof(QDynamic), PinType.Input);
            InsetPin("触发出队信号",this,typeof(QDynamic), PinType.Input);
            InsetPin("数据出队",this,typeof(QDynamic), PinType.Output);
            InsetPin("容器收集完成",this,typeof(QData), PinType.Output);
            return true;
        }

        public override bool InitDataContext()
        {
            DataContext = new ContainerToolData();
            return true;
        }

        public int ContainerCount => Context<ContainerToolData>().ContainerCount;

        public override Task<bool> ExecuteAsync(PinInfo pinInfo, QData pinData, ToolExecutionContext toolContext)
        {
            if (pinInfo.Name == "数据入队")
            {
                lock (qDynamics)
                {
                    qDynamics.Enqueue(pinData as QDynamic);
                    return Task.FromResult(true);
                }
            }
            else if (pinInfo.Name == "触发出队信号")
            {
                lock (qDynamics) 
                {
                    if (qDynamics.Count > 0)
                    {
                        var qDynamic = qDynamics.Dequeue();
                        SendToPin("数据出队",qDynamic);
                        return Task.FromResult(true);
                    }
                }
            }
            throw new NotImplementedException($"未知输入{pinInfo.Name}");
        }
    }
}
