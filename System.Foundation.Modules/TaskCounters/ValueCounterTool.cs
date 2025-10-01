using QStandaedPlatform.Engine.Common.Common;
using QStandaedPlatform.Engine.Common.Common.Metadatas;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System.Foundation.Modules.TaskCounters
{
    public class ValueCounterData : DynamicPinToolData
    {
        public int Value { get; set; }
    }
    [DisplayName("数值计数工具")]
    public class ValueCounterTool : DynamicPinTool
    {
        private volatile int _counter=1;
        private const string TaskCountPinName = "增加计数";
        private const string TaskCompletePinName = "计数完成";

        public override string DefineName => "数值计数工具";


        public override bool InitPins()
        {
            InsetPin(TaskCountPinName, this,typeof(QData), PinType.Input);
            InsetPin(TaskCompletePinName,this,typeof(QData), PinType.Output);
            return true;
        }

        public override bool InitDataContext()
        {
            DataContext = new ValueCounterData()
            {
                IsUpdateInput = false,
                UpdateOutputIndex = 0
            };
            return true;
        }
        public override Task<bool> ClearEphemeralDataAsync()
        {
            Interlocked.Exchange(ref _counter, 0);
            return Task.FromResult(true);
        }

        public int TaskCount => Context<ValueCounterData>().Value;

        public override Task<bool> ExecuteAsync(PinInfo pinInfo, QData pinData, ToolExecutionContext toolContext)
        {
            if (pinInfo.Name == TaskCountPinName)
            {
                if (_counter >= TaskCount)
                {
                    SendToPin($"计数_{_counter}", new QInt(_counter));
                    SendToPin(TaskCompletePinName, new QData());
                    Interlocked.Exchange(ref _counter, 1);
                    return Task.FromResult(true);
                }
                else if (_counter < TaskCount)
                {
                    SendToPin($"计数_{_counter}",new QInt(_counter));
                }
                Interlocked.Increment(ref _counter);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        public override void HandleRequestAddNewPin(object dynamicPinData)
        {
            var context = Context<ValueCounterData>();
            for (int i = 0; i < context.Value; i++)
            {
                InsetPin($"计数_{i + 1}", this, typeof(QInt), PinType.Output, true);
            }
        }
    }
}
