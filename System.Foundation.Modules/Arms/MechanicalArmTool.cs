using Microsoft.Extensions.Logging;
using QStandaedPlatform.Engine.Common;
using QStandaedPlatform.Engine.Common.Common;
using QStandaedPlatform.Engine.Common.Common.Metadatas;
using QStandaedPlatform.Engine.Components;
using QStandaedPlatform.Engine.Components.Modbus;
using QStandaedPlatform.Engine.Laboratory;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Foundation.Modules.NormalModules;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System.Foundation.Modules.Arms
{
    [DisplayName("搬运模块")]
    public class MechanicalArmTool : SyncInputModuleToolBase
    {
        private const string _inputStartPos = "输入开始坐标";
        private const string _inputEndPos = "输入结束坐标";
        private const string _outputRuncompleted = "输出搬运完成信号";
        public  ArmData _armData = new();
        private Judger _judger;

        public override string DefineName => "搬运模块";


        public override bool InitPins()
        {
            InsetPin(_inputStartPos, this,typeof(QPosition), PinType.Input);
            InsetPin(_inputEndPos, this, typeof(QPosition), PinType.Input);
            InsetPin(_outputRuncompleted, this, typeof(QData), PinType.Output);
            _judger = new Judger(async () =>
            {
                var state = await h5UTcp?.ReadSingleValueAsync<short>(_armData.StateAddress);
                return state == _armData.StateCode;
            })
            {IsAutoResetCancel=true };
            return true;
        }
        public override Task<bool> OnHandleRequestCancelAsync()
        {
            if (_armData.IsFuncCode)
            {
                return base.OnHandleRequestCancelAsync();
            }
            else
            {
                return Task.FromResult(true);
            }

        }

        public override Task<bool> OnHandleRequestCancelResetAsync()
        {
            if (_armData.IsFuncCode)
            {
                return base.OnHandleRequestCancelResetAsync();
            }
            else
            {
                return Task.FromResult(true);
            }
        }
        public override bool InitDataContext()
        {
            DataContext = _armData;
            return base.InitDataContext();
        }
        public override bool InitEnd()
        {
            _armData = Context<ArmData>();
            return base.InitEnd();
        }
        public override async Task<bool> ExexuteAsync(Dictionary<PinInfo, QData> pinDatas, ToolExecutionContext toolContext)
        {
            var startPos = pinDatas.FirstOrDefault(p => p.Key.Name == _inputStartPos).Value as QPosition;
            var endPos = pinDatas.FirstOrDefault(p => p.Key.Name == _inputEndPos).Value as QPosition;
            //todo 
            startPos = (QPosition)startPos.Clone();
            endPos = (QPosition)endPos.Clone();
            Logger?.LogInformation($"接收到开始坐标 X:{startPos.X},Y:{startPos.Y},Z:{startPos.Z},R:{startPos.Angle}");
            Logger?.LogInformation($"接收到结束坐标 X:{endPos.X},Y:{endPos.Y},Z:{endPos.Z},R:{endPos.Angle}");

            startPos.X += _armData.X_Start_Offset;
            startPos.Y += _armData.Y_Start_Offset;
            startPos.Z += _armData.Z_Start_Offset;
            endPos.X += _armData.X_End_Offset;
            endPos.Y += _armData.Y_End_Offset;
            endPos.Z += _armData.Z_End_Offset;
            //偏移后位置记录
            Logger?.LogInformation($"偏移后开始坐标 X:{startPos.X},Y:{startPos.Y},Z:{startPos.Z},R:{startPos.Angle}");
            Logger?.LogInformation($"偏移后结束坐标 X:{endPos.X},Y:{endPos.Y},Z:{endPos.Z},R:{endPos.Angle}");
            float[] data = [];
            _armData = Context<ArmData>();
            if (_armData.ContainAngle)
            {
                data =
          [
                    startPos.X,startPos.Y,startPos.Z,startPos.Z2,startPos.Angle,
                    endPos.X, endPos.Y, endPos.Z, endPos.Z2,endPos.Angle,
                    0f,0f,0f,0f,0f,
                    _armData.ClawOpenPos,
                    _armData.ClawOpenAngle,
                    _armData.ClawClosePos,
                    _armData.ClawCloseAngle,
          ];
            }
            else
            {
                data =
           [
                    startPos.X,startPos.Y,startPos.Z,startPos.Z2,
                    endPos.X, endPos.Y, endPos.Z, endPos.Z2,
                    0f,0f,0f,0f,0f,0f,0f,_armData.ClawOpenPos,
                    _armData.ClawOpenAngle,
                    _armData.ClawClosePos,
                    _armData.ClawCloseAngle,
           ];
            }
            if (data.Length == 0)
            {
                return false;
            }
            //写入参数
            await GetModular().WriteParameterAsync(_armData.Address, data);
            var state = false;
            if (_armData.IsFuncCode)
            {
                if (!GetModular().VerifyModuleActivityStatus(_armData.StateAddress))
                {
                    await GetModular().ModuleExecuteAsync(_armData.CommandAddress, (short)_armData.ArmFuncCode);
                }
                await GetModular().CheckDoneAsync(_armData.StateAddress, _armData.CommandAddress);
            }
            else
            {
                await h5UTcp?.PlusOutputAsync(_armData.CommandAddress, _armData.PlusTime);
                state = await _judger.SureAsync(Timeout.Infinite, RequestCancelToken);
                //复位完成状态
                _ = await h5UTcp?.WriteSingleValueAsync(_armData.StateAddress, 0);                if (!state)
                {
                    return false;
                }
            }
            SendToPin(_outputRuncompleted, new QData());
            return true;
        }
        public override void ApplyOnContextChanged(object context)
        {
            _armData = (ArmData)DataContext;
            base.ApplyOnContextChanged(context);
        }
    }
}
