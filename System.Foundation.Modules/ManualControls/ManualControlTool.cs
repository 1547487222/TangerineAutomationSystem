using QStandaedPlatform.Engine.Common;
using QStandaedPlatform.Engine.Common.Common;
using QStandaedPlatform.Engine.Common.Common.Metadatas;
using QStandaedPlatform.Engine.Components;
using QStandaedPlatform.Engine.Laboratory;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Foundation.Modules.NormalModules;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Foundation.Modules.Arms.MechanicalArmTool;

namespace System.Foundation.Modules.ManualControls
{
    [DisplayName("手动控制工具")]
    public class ManualControlTool : ModuleToolBase
    {
        public ManualControlData _ManualControlData=new();

        private  Judger _judger;
        public class ManualControlData: ModuleData
        {
            [DisplayName("手自动切换地址")]
            public string ManuAutoCtl { get; set; } = "M106";
            [DisplayName("复位报警地址")]
            public string ResetAlarm { get; set; } = "M101";

            [DisplayName("急停地址")]
            public string EmergencyCtl { get; set; } = "M102";
            [DisplayName("暂停地址")]
            public string PauseCtl { get; set; } = "M105";

            [DisplayName("启动地址")]
            public string StartCtl { get; set; } = "M103";
            [DisplayName("初始化地址")]
            public string InitCtl { get; set; } = "M0";

            [DisplayName("初始化脉冲时间")]
            public int PlusTime { get; set; } = 1000;
            [DisplayName("初始化状态地址")]
            public string InitState { get; set; } = "D0";
            [DisplayName("初始化状态码")]
            public int InitCode { get; set; } = 999;
            [DisplayName("初始化状态码_老平台")]
            public int InitCode_Old { get; set; } = 9999;
            [DisplayName("是否老平台")]
            public bool IsOld { get; set; } = true;

            public string FuncCodeAddress { get; set; } = "D210";
        }
        public override string DefineName => "手动控制工具";

        public override bool Init()
        {
            TriggerPointCommands.Add(new TriggerPointCommand(1,"切换手动"));
            TriggerPointCommands.Add(new TriggerPointCommand(1, "切换自动"));
            TriggerPointCommands.Add(new TriggerPointCommand(1, "切换暂停"));
            TriggerPointCommands.Add(new TriggerPointCommand(1, "暂停恢复"));
            TriggerPointCommands.Add(new TriggerPointCommand(1, "启动"));
            TriggerPointCommands.Add(new TriggerPointCommand(1, "初始化"));
            _judger = new Judger(async () =>
             {
                 var state = await h5UTcp?.ReadSingleValueAsync<short>(_ManualControlData.InitState);
                 if (_ManualControlData.IsOld)
                 {
                     return state == _ManualControlData.InitCode_Old;
                 }
                 else
                 {
                     return state == _ManualControlData.InitCode;
                 }
             })
            { IsAutoResetCancel=true};
            return base.Init();
        }
        public override bool InitPins()
        {
            InsetPin("触发一键启动",this,typeof(QData), PinType.Input);
            InsetPin("一键启动完成",this,typeof(QData), PinType.Output);
            return base.InitPins();
        }
        public override bool InitDataContext()
        {
            DataContext = _ManualControlData;
            return base.InitDataContext();
        }

        public override bool InitEnd()
        {
            _ManualControlData = Context<ManualControlData>();
            return base.InitEnd();
        }

        public override Task<bool> OnHandleRequestCancelAsync()
        {
            if (_ManualControlData.IsOld)
            {
                return Task.FromResult(true);
            }
            else
            return base.OnHandleRequestCancelAsync();
        }
        public override Task<bool> OnHandleRequestCancelResetAsync()
        {
            if (!_ManualControlData.IsOld)
            {
                return base.OnHandleRequestCancelResetAsync();
            }
            else
            {
                return Task.FromResult(true);
            }
        }
        public override Task<CommandResult> ExecuteCommandAsync(ITriggerPointCommand triggerPointCommand)
        {
            if (triggerPointCommand.Id == 1000)
            {
                _judger.Cancel = true;
            }
            return base.ExecuteCommandAsync(triggerPointCommand);
        }
        public override async Task<bool> ExecuteAsync(PinInfo pinInfo, QData pinData, ToolExecutionContext toolContext)
        {
            if (pinInfo.Name == "触发一键启动")
            {
                if (_ManualControlData.IsOld)
                {
                    _ManualControlData = Context<ManualControlData>();
                    var isAuto = await h5UTcp?.ReadSingleBooleanAsync(_ManualControlData.ManuAutoCtl);
                    //如果是自动切换手动
                    if (isAuto)
                    {
                        //切手动
                        var writeAuto = await h5UTcp?.WriteSingleBooleanAsync(_ManualControlData.ManuAutoCtl, false);
                    }

                    var writeisAlarm = await h5UTcp?.PlusOutputAsync(_ManualControlData.ResetAlarm, _ManualControlData.PlusTime);
                    var isPause = await h5UTcp?.ReadSingleBooleanAsync(_ManualControlData.PauseCtl);
                    if (isPause)
                    {
                        var writePause = await h5UTcp?.WriteSingleBooleanAsync(_ManualControlData.PauseCtl, false);
                    }
                    var initResult = await h5UTcp?.PlusOutputAsync(_ManualControlData.InitCtl, _ManualControlData.PlusTime);
                    var state = await _judger.SureAsync(Timeout.Infinite,RequestCancelToken);
                    //复位完成状态
                    _ = await h5UTcp?.WriteSingleValueAsync(_ManualControlData.InitState, 0);
                    if (!state)
                    {
                        return false;
                    }
                    _ = await h5UTcp?.WriteSingleBooleanAsync(_ManualControlData.ManuAutoCtl, true);
                    await h5UTcp?.PlusOutputAsync(_ManualControlData.StartCtl, _ManualControlData.PlusTime);
                    SendToPin("一键启动完成", new QData());
                    return true;
                }
                else
                {
                    _ManualControlData = Context<ManualControlData>();
                    await GetModular().HomeAsync();
                    SendToPin("一键启动完成", new QData());
                    return true;
                }
            }
            return false;
        }

        public override void ApplyOnContextChanged(object context)
        {
            _ManualControlData = (ManualControlData)DataContext;
            base.ApplyOnContextChanged(context);
        }
    }
}
