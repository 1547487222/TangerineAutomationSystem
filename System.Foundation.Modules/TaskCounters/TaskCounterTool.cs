using QStandaedPlatform.Engine.Common.Common;
using QStandaedPlatform.Engine.Common.Common.Metadatas;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace System.Foundation.Modules.TaskCounters
{
    [DisplayName("任务计数器")]
    public class TaskCounterTool : ToolBase
    {
        private volatile  int _counter;
        private const string TaskCountPinName = "增加计数";
        private const string TaskContinuePinName = "任务继续信号";
        private const string TaskContinueCountPinName = "输出任务继续计数值";
        private const string TaskCompletePinName = "计数完成";
        private const string TaskCompleteCountPinName = "输出任务完成计数值";
        public override string DefineName => "任务计数器";

        public override bool InitPins()
        {
            InsetPin(TaskCountPinName, this,typeof(QData),PinType.Input);

            InsetPin(TaskContinuePinName, this,typeof(QData), PinType.Output);
            InsetPin(TaskContinueCountPinName, this, typeof(QInt), PinType.Output);
            InsetPin(TaskCompletePinName, this,typeof(QData), PinType.Output);
            InsetPin(TaskCompleteCountPinName, this, typeof(QInt), PinType.Output);
            TriggerPointCommands.Add(new TriggerPointCommand(1,"清除计数"));
            return true;
        }
        public override bool InitDataContext()
        {
            DataContext = new TaskCounterData();
            return true;
        }
        public override Task<bool> ClearEphemeralDataAsync()
        {
            Interlocked.Exchange(ref _counter, 0);
            return Task.FromResult(true);
        }
        public override Task<CommandResult> ExecuteCommandAsync(ITriggerPointCommand triggerPointCommand)
        {
            if (triggerPointCommand.Id == 1)
            {
                Interlocked.Exchange(ref _counter, 0);
            }
            return base.ExecuteCommandAsync(triggerPointCommand);
        }
        public int TaskCount => Context<TaskCounterData>().TaskCounter;

        public int StartIndex => Context<TaskCounterData>().StartIndex;

        public int Interval => Context<TaskCounterData>().Interval;

        public Dictionary<int,int> SkipList => Context<TaskCounterData>().SkipList.ToDictionary(p => p, p => p);

        public override Task<bool> ExecuteAsync(PinInfo pinInfo, QData pinData, ToolExecutionContext toolContext)
        {
            if (pinInfo.Name == TaskCountPinName)
            {
                if (_counter >= TaskCount)
                {
                    SendToPin(TaskCompleteCountPinName, new QInt(StartIndex + _counter+ Interval));
                    SendToPin(TaskCompletePinName, new QData());
                    Interlocked.Exchange(ref _counter, 0);
                    return Task.FromResult(true);
                }
                else if (_counter < TaskCount)
                {
                    while (SkipList.ContainsKey(StartIndex + _counter + Interval))
                    {
                        Interlocked.Increment(ref _counter);
                    }
                    SendToPin(TaskContinuePinName, new QData());
                    SendToPin(TaskContinueCountPinName, new QInt(StartIndex + _counter + Interval));
                    Interlocked.Increment(ref _counter);
                    return Task.FromResult(true);
                }
            }
            return Task.FromResult(false);
        }
    }
}
