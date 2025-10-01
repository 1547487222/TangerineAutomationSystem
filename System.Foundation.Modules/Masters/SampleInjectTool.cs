using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using QStandaedPlatform.Engine.Common.Common;
using QStandaedPlatform.Engine.Common.Common.Metadatas;
using QStandaedPlatform.Engine.Laboratory;
using System.ComponentModel;
using System.Foundation.Modules.Triggers;
using System.Threading;
using System.Threading.Tasks;

namespace System.Foundation.Modules.Masters
{

    public class SampleInjectToolCollectionToolData
    {
        [DisplayName("进样数量")]
        public int SampleInjectCount { get; set; } = 2;
    }
    //public class SampleInjectData
    //{
    //    public int SampleInjectCount { get; set; } = 4;
    //}

    [DisplayName("进样服务")]
    public class SampleInjectTool : ToolBase, ISampleInjectService, IStartSignService
    {

        private int clickCount = 0;

        private const string inputSamplingSignalName = "输入进样次数";

        private const string outputSampleName = "输出样品";

        private const string outputSamplePositionName = "输出样品孔位编号";

        private const string outputSampleCodeName = "输出样品编号";

        private const string outputSamplingSignalName = "输出进样信号";

        private const string TriggerInjectSample = "进样";

        public override string DefineName => "进样服务";

        public int SignalEventCount => Context<SampleInjectToolCollectionToolData>().SampleInjectCount;

        private readonly SampleService _sampleService;

        public SampleInjectTool(SampleService sampleService)
        {
            _sampleService = sampleService;
        }

        public override bool InitPins()
        {

            InsetPin(inputSamplingSignalName, this, typeof(QInt), PinType.Input);


            InsetPin(outputSampleName, this, typeof(QSample), PinType.Output);
            // InsetPin(outputSamplePositionName, this, typeof(QString), PinType.Output);
            InsetPin(outputSampleCodeName, this, typeof(QInt64), PinType.Output);
            InsetPin(outputSamplingSignalName, this, typeof(QData), PinType.Output);


            TriggerPointCommands.Add(new TriggerPointCommand(1, TriggerInjectSample));

            return true;
        }
        public override bool InitDataContext()
        {
            DataContext = new SampleInjectToolCollectionToolData();
            return true;
        }

        public override Task<CommandResult> ExecuteCommandAsync(ITriggerPointCommand triggerPointCommand)
        {
            if (triggerPointCommand.Id == 1)
            {
                var samplingTaskInfos = GenratorSampleTasks(clickCount);

                InjectSample(samplingTaskInfos);

                clickCount++;

              
            }
            return Task.FromResult(CommandResult.Ok(this.DefineName));
        }

        private SampleTaskInfo[] GenratorSampleTasks(int triggerCount = 0)
        {
            var samplingTaskInfos = new SampleTaskInfo[SignalEventCount];


            for (int i = 1; i <= SignalEventCount; i++)
            {
                var snowId = SnowflakeIdGenerator.Instance.GenerateId();
                samplingTaskInfos[i - 1] = new SampleTaskInfo
                {
                    SamplingId = (triggerCount * SignalEventCount) + i,
                    PlatformId = (triggerCount * SignalEventCount) + i,
                    PlatformTaskId = (triggerCount * SignalEventCount) + i,
                    SamplingTaskId = (triggerCount * SignalEventCount) + i,
                };
            }

            return samplingTaskInfos;
        }

        public override Task<bool> ExecuteAsync(PinInfo pinInfo, QData pinData, ToolExecutionContext toolContext)
        {
            if (pinInfo.Name == inputSamplingSignalName)
            {
                var triggerCount = (pinData as QInt) ?? 1;

                var samplingTaskInfos = GenratorSampleTasks(triggerCount);
                InjectSample(samplingTaskInfos);
                return Task.FromResult(true);
            }
            else
            {
                throw new NotImplementedException($"未实现:{pinInfo.Name}");
            }
        }

        public void InjectSample(SampleTaskInfo[] samplingTaskInfos)
        {
            foreach (var item in samplingTaskInfos)
            {
                QSample sample  = new QSample
                {
                    PlatformId = item.PlatformId,
                    PlatformTaskId = item.PlatformTaskId,
                    SamplingId = item.SamplingId,
                    SamplingTaskId = item.SamplingTaskId,
                };
                    
                //ToolExecutionContext.AddSamplingTask(item);
                _sampleService.AddSample(item);
                SendToPin(outputSampleName, sample);
                SendToPin(outputSampleCodeName, (QInt64)sample.SamplingId);
                Logger?.LogInformation("输出样品:{sample}", sample);
                Thread.Sleep(50);
            }
            //ToolExecutionContext.Flow.NewSampleCollectionTrace();
            SendToPin(outputSamplingSignalName, new QData());
        }

        public void InjectSample(InjectSamplingModel[] injectSamplings)
        {
            foreach (var item in injectSamplings)
            {
                _sampleService.AddSample(item.SampleInfo);
                _sampleService.RegisterSampleTrace(item.SampleInfo.SamplingId, item.SampleTraceAction);
                _sampleService.RegisterSampleComplete(item.SampleInfo.SamplingId, item.SampleCompleteAction);
                SendToPin(outputSampleCodeName, (QInt64)item.SampleInfo.SamplingId);
                Logger.LogInformation("InjectSample输出样品:{sample}", item.SampleInfo.SamplingId);
            }
            SendToPin(outputSamplingSignalName, new QData());

        }
    }
}
