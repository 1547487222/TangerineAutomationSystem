using Microsoft.Extensions.Logging;
using QStandaedPlatform.Engine.Common.Common;
using QStandaedPlatform.Engine.Common.Common.Metadatas;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace System.Foundation.Modules.Masters
{

    public class SignalEventCollectionToolData:DynamicPinToolData
    {
        [DisplayName("信号事件数")]
        public int SignalEventCount { get; set; } = 2;
    }

    [DisplayName("信号事件采集器")]
    public class SignalEventCollectionTool : DynamicPinTool
    {
        private readonly List<QData> signalEventDatas = [];
        public override string DefineName => "信号事件采集器";

        private const string SignalEventCollectPinName = "事件信号收集";

        private const string SignalEventCollectCompletePinName = "事件信号收集完成";


        public override bool InitPins()
        {
            InsetPin(SignalEventCollectPinName, this,typeof(QDynamic), PinType.Input);
            InsetPin(SignalEventCollectCompletePinName, this,typeof(QData), PinType.Output);
            return true;
        }
        public override bool InitDataContext()
        {
            DataContext = new SignalEventCollectionToolData()
            {
                IsUpdateInput = false,
                UpdateOutputIndex = 0
            };
            return true;
        }

        public override Task<bool> ClearEphemeralDataAsync()
        {
            signalEventDatas.Clear();
            return Task.FromResult(true);
        }

        public int SignalEventCount => Context<SignalEventCollectionToolData>().SignalEventCount;

        public override async Task<bool> ExecuteAsync(PinInfo pinInfo, QData pinData, ToolExecutionContext toolContext)
        {

            var context = Context<SignalEventCollectionToolData>();
            if (pinInfo.Name == SignalEventCollectPinName)
            {
                lock (signalEventDatas)
                {
                    signalEventDatas.Add(pinData);
                    if (signalEventDatas.Count == SignalEventCount)
                    {
                        this.Logger.LogInformation($"信号数{signalEventDatas.Count},信号事件数{SignalEventCount}");
                        if (OutputPins.Any(p => p.Name == SignalEventCollectCompletePinName))
                        {
                            SendToPin(SignalEventCollectCompletePinName, new QData());
                        }
                        for (int i = 0; i < context.SignalEventCount; i++)
                        {
                            SendToPin($"事件信号{i + 1}", signalEventDatas[i]);
                        }
                        signalEventDatas.Clear();
                        Logger.LogInformation("事件清空:{count}",signalEventDatas.Count);
                    }
                }
                return await Task.FromResult(true);
            }
            return await Task.FromResult(false);
        }

        public override void HandleRequestAddNewPin(object dynamicPinData)
        {
            for (int i = 0; i < SignalEventCount; i++)
            {
                InsetPin($"事件信号{i + 1}", this, typeof(QDynamic), PinType.Output,true);
            }
        }
    }
}
