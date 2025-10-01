using Microsoft.Extensions.Logging;
using QStandaedPlatform.Engine.Common.Common;
using QStandaedPlatform.Engine.Common.Common.Metadatas;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System.Foundation.Modules.Triggers
{
    [DisplayName("触发器")]
    public class TriggerTool : ToolBase, ITangerinePrologueAngryService, IStartSignService
    {
        private const string TriggerOnce = "触发一次";

        private const string TriggerSpecifiedTimes = "触发指定次数";

        private const string TriggerOutputString = "触发输出String";

        private const string TriggerOutputInt = "触发输出Int";

        private const string TriggerOutputFloat = "触发输出Float";


        private const string InputTriggerOnce = "输入一次触发信号";

        private const string InputTriggerSpecifiedTimes = "输入指定次数触发信号";

        private const string InputTriggerString = "触发String";

        private const string InputTriggerInt = "触发Int";

        private const string InputTriggerFloat = "触发Float";

        private const string InputString = "输入String";

        private const string InputInt = "输入Int";

        private const string InputFloat = "输入Float";

        private const string OutputTriggerOnce = "输出一次触发信号";

        private const string OutputTriggerSpecifiedTimes = "输出指定次数触发信号";

        private const string OutputString = "输出String";

        private const string OutputInt = "输出Int";

        private const string OutputFloat = "输出Float";

        public override string DefineName => "触发器";

        public override bool Init()
        {
            TriggerPointCommands.Add(new TriggerPointCommand(1, TriggerOnce));
            TriggerPointCommands.Add(new TriggerPointCommand(2, TriggerSpecifiedTimes));
            TriggerPointCommands.Add(new TriggerPointCommand(3, TriggerOutputString));
            TriggerPointCommands.Add(new TriggerPointCommand(4, TriggerOutputInt));
            TriggerPointCommands.Add(new TriggerPointCommand(5, TriggerOutputFloat));
            return true;
        }

        public override bool InitPins()
        {
            InsetPin(InputTriggerOnce, this, typeof(QData), PinType.Input);
            InsetPin(InputTriggerSpecifiedTimes, this, typeof(QData), PinType.Input);

            InsetPin(InputTriggerString, this, typeof(QData), PinType.Input);
            InsetPin(InputTriggerInt, this, typeof(QData), PinType.Input);
            InsetPin(InputTriggerFloat, this, typeof(QData), PinType.Input);

            InsetPin(InputString, this, typeof(QString), PinType.Input);
            InsetPin(InputInt, this, typeof(QInt), PinType.Input);
            InsetPin(InputFloat, this, typeof(QFloat), PinType.Input);

            InsetPin(OutputTriggerOnce, this,typeof(QData), PinType.Output);
            InsetPin(OutputTriggerSpecifiedTimes, this,typeof(QData), PinType.Output);
            InsetPin(OutputString, this,typeof(QString), PinType.Output);
            InsetPin(OutputInt, this, typeof(QInt), PinType.Output);
            InsetPin(OutputFloat, this, typeof(QFloat), PinType.Output);
            return base.InitPins();
        }

        public override bool InitDataContext()
        {
            DataContext = new TriggerData();
            return base.InitDataContext();
        }
        public override Task<CommandResult> ExecuteCommandAsync(ITriggerPointCommand triggerPointCommand)
        {
            if (triggerPointCommand.Id == 1)
            {
                this.ToolExecutionContext.RequestStartAsync();
                SendToPin(OutputTriggerOnce, new QData());
            }
            else if (triggerPointCommand.Id == 4)
            {
                SendToPin(OutputInt, (QInt)Context<TriggerData>().TriggerInt);
            }
            else if (triggerPointCommand.Id == 3)
            {
                SendToPin(OutputString, (QString)Context<TriggerData>().TriggerString);
            }
            else if (triggerPointCommand.Id == 5)
            {
                SendToPin(OutputFloat, (QFloat)Context<TriggerData>().TriggerFloat);
            }
            return Task.FromResult(CommandResult.Ok(this.DefineName));
        }
        public override Task<bool> ExecuteAsync(PinInfo pinInfo, QData pinData, ToolExecutionContext toolContext)
        {
            if (pinInfo.Name == InputTriggerOnce)
            {
                SendToPin(OutputTriggerOnce, new QData());
                return Task.FromResult(true);
            }
            else if (pinInfo.Name == InputTriggerInt)
            {
                SendToPin(OutputInt, (QInt)Context<TriggerData>().TriggerInt);
                return Task.FromResult(true);
            }
            else if (pinInfo.Name == InputTriggerString)
            {
                SendToPin(OutputString, (QString)Context<TriggerData>().TriggerString);
                return Task.FromResult(true);
            }
            else if (pinInfo.Name == InputTriggerFloat)
            {
                SendToPin(OutputFloat, (QFloat)Context<TriggerData>().TriggerFloat);
                return Task.FromResult(true);
            }
            else
            {
                throw new NotImplementedException($"未实现:{pinInfo.Name}");
            }
        }

        public Task StartPrologueAsync()
        {
            this.ToolExecutionContext.RequestStartAsync();
            SendToPin(OutputTriggerOnce, new QData());
            Logger?.LogInformation($"StartPrologueAsync执行"+this.DisplayName);
            return Task.CompletedTask;
        }
    }
}
