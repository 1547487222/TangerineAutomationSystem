using Microsoft.Extensions.Logging;
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

namespace System.Foundation.Modules.Arms
{
    [DisplayName("搬回模块")]
    public class MechanicalArmMoveBackTool : SyncInputModuleToolBase
    {
        public  ArmMoveBackData _armData = new();

        private Judger _judger;
        public class ArmMoveBackData: ModuleData
        {
            [DisplayName("坐标起始地址")]
            public string Address { get; set; } = "D100";
            /// <summary>
            /// 命令地址
            /// </summary>
            [DisplayName("命令地址")]
            public string CommandAddress { get; set; } = "D210";
            /// <summary>
            /// 状态地址
            /// </summary>
            [DisplayName("状态地址")]
            public string StateAddress { get; set; } = "D200";
            /// <summary>
            /// 状态码
            /// </summary>
            [DisplayName(" 状态码")]
            public int StateCode { get; set; } = 999;
            [DisplayName("是否功能码的方式")]
            public bool IsFuncCode { get; set; } = true;

            [DisplayName("是否包含角度R")]
            public bool ContainAngle { get; set; } = true;

            [DisplayName("搬回功能码")]
            public int ArmFuncCode { get; set; } = 8;

            public int PlusTime { get; set; } = 2000;

            [DisplayName("X轴起始偏移")]
            public float X_Start_Offset { get; set; } = 0;

            [DisplayName("Y轴起始偏移")]
            public float Y_Start_Offset { get; set; } = 0;

            [DisplayName("Z轴起始偏移")]
            public float Z_Start_Offset { get; set; } = 0;


            [DisplayName("X轴结束偏移")]
            public float X_End_Offset { get; set; } = 0;

            [DisplayName("Y轴结束偏移")]
            public float Y_End_Offset { get; set; } = 0;

            [DisplayName("Z轴结束偏移")]
            public float Z_End_Offset { get; set; } = 0;


            //夹爪打开位
            [DisplayName("夹爪取料打开位")]
            public float ClawOpenPos { get; set; }

            //夹爪打开角度
            [DisplayName("夹爪取料角度")]
            public float ClawOpenAngle { get; set; }

            //夹爪关闭位
            [DisplayName("夹爪放料关闭位")]
            public float ClawClosePos { get; set; }

            //夹爪关闭角度
            [DisplayName("夹爪放料关闭角度")]
            public float ClawCloseAngle { get; set; }

        }
         
        public override string DefineName => "搬回模块";

        public override bool InitPins()
        {
            InsetPin("搬回信号", this, typeof(QData), PinType.Input);
            InsetPin("输入开始坐标", this, typeof(QPosition), PinType.Input);
            InsetPin("输入结束坐标", this, typeof(QPosition), PinType.Input);
            InsetPin("输出搬回完成信号", this, typeof(QData), PinType.Output);
            _judger = new Judger(async () =>
            {
                var state = await h5UTcp?.ReadSingleValueAsync<short>(_armData.StateAddress);
                return state == _armData.StateCode;
            })
            { IsAutoResetCancel = true };
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
           return true;
        }
        public override bool InitEnd()
        {
            _armData = Context<ArmMoveBackData>();
            return true;
        }
        public override async Task<bool> ExexuteAsync(Dictionary<PinInfo, QData> pinDatas, ToolExecutionContext toolContext)
        {
            _armData = Context<ArmMoveBackData>();
            var pinData = pinDatas.ToArray();
            if (pinData.Length == 3)
            {
                var startPos = pinData.FirstOrDefault(p => p.Key.Name == "输入开始坐标").Value as QPosition;
                var endPos = pinData.FirstOrDefault(p => p.Key.Name == "输入结束坐标").Value as QPosition;
                //todo
                startPos= (QPosition)startPos?.Clone();
                endPos= (QPosition)endPos?.Clone();
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
                _armData = Context<ArmMoveBackData>();
                float[] data = [];
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
                bool state = false;
                //写入参数
                await GetModular().WriteParameterAsync(_armData.Address, data);
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
                    _ = await h5UTcp?.WriteSingleValueAsync(_armData.StateAddress, 0);
                    if (!state)
                    {
                        return false;
                    }
                }
                SendToPin("输出搬回完成信号", new QData());
                return true;
            }
            return false;
        }
        public override void ApplyOnContextChanged(object context)
        {
            _armData = (ArmMoveBackData)DataContext;
            base.ApplyOnContextChanged(context);
        }
    }
}
