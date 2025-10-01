using Microsoft.Extensions.Logging;
using QStandaedPlatform.Engine.Common.Common;
using QStandaedPlatform.Engine.Common.Common.Metadatas;
using QStandaedPlatform.Engine.Components;
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
    [DisplayName("搬运模块-过渡点")]
    public class MechanicalArmWithTransTool : SyncInputModuleToolBase
    {
        public MechanicalArmWithTransData mechanicalArmWithTransData = new();
        public class MechanicalArmWithTransData: ModuleData
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

            [DisplayName("搬回功能码")]
            public int ArmFuncCode { get; set; } = 15;

            [DisplayName("是否包含角度R")]
            public bool ContainAngle { get; set; } = true;

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

        public override string DefineName => "搬运模块-过渡点";
        public override bool Init()
        {
            return true;
        }

        public override bool InitPins()
        {
            InsetPin("输入开始位置", this, typeof(QPosition), PinType.Input);
            InsetPin("输入过渡点位置", this, typeof(QPosition), PinType.Input);
            InsetPin("输入结束位置", this, typeof(QPosition), PinType.Input);
            InsetPin("输出搬运完成信号", this, typeof(QData), PinType.Output);
            return base.InitPins();
        }
        public override bool InitDataContext()
        {
            DataContext = mechanicalArmWithTransData;
            return base.InitDataContext();
        }
        public override async Task<bool> ExexuteAsync(Dictionary<PinInfo, QData> pinDatas, ToolExecutionContext toolContext)
        {
            var posStart = pinDatas.FirstOrDefault(p => p.Key.Name == "输入开始位置").Value as QPosition;
            var posTrans = pinDatas.FirstOrDefault(p => p.Key.Name == "输入过渡点位置").Value as QPosition;
            var posEnd = pinDatas.FirstOrDefault(p => p.Key.Name == "输入结束位置").Value as QPosition;
            Logger?.LogInformation($"接收到开始坐标 X:{posStart.X},Y:{posStart.Y},Z:{posStart.Z},R:{posStart.Angle}");
            Logger?.LogInformation($"接收到过渡点坐标 X:{posTrans.X},Y:{posTrans.Y},Z:{posTrans.Z},R:{posTrans.Angle}");
            Logger?.LogInformation($"接收到结束坐标 X:{posEnd.X},Y:{posEnd.Y},Z:{posEnd.Z},R:{posEnd.Angle}");
            posStart = (QPosition)posStart.Clone();
            posTrans = (QPosition)posTrans.Clone();
            posEnd = (QPosition)posEnd.Clone();

            mechanicalArmWithTransData = Context<MechanicalArmWithTransData>();

            posStart.X += mechanicalArmWithTransData.X_Start_Offset;
            posStart.Y += mechanicalArmWithTransData.Y_Start_Offset;
            posStart.Z += mechanicalArmWithTransData.Z_Start_Offset;
            posEnd.X += mechanicalArmWithTransData.X_End_Offset;
            posEnd.Y += mechanicalArmWithTransData.Y_End_Offset;
            posEnd.Z += mechanicalArmWithTransData.Z_End_Offset;
            //偏移后位置记录
            Logger?.LogInformation($"偏移后开始坐标 X:{posStart.X},Y:{posStart.Y},Z:{posStart.Z},R:{posStart.Angle}");
            Logger?.LogInformation($"偏移后结束坐标 X:{posEnd.X},Y:{posEnd.Y},Z:{posEnd.Z},R:{posEnd.Angle}");
            var data = Array.Empty<float>();
            if (mechanicalArmWithTransData.ContainAngle)
            {
                data =
               [
                posStart.X,posStart.Y,posStart.Z,posStart.Z2,posStart.Angle,
                posEnd.X,posEnd.Y,posEnd.Z,posEnd.Z2,posEnd.Angle,
                posTrans.X,posTrans.Y,posTrans.Z,posTrans.Z2,posTrans.Angle,
                mechanicalArmWithTransData.ClawOpenPos,
                mechanicalArmWithTransData.ClawOpenAngle,
                mechanicalArmWithTransData.ClawClosePos,
                mechanicalArmWithTransData.ClawCloseAngle
               ];
            }
            else
            {
                data =
                 [
                posStart.X,posStart.Y,posStart.Z,posStart.Z2,
                posEnd.X,posEnd.Y,posEnd.Z,posEnd.Z2,
                posTrans.X,posTrans.Y,posTrans.Z,posTrans.Z2,
                 ];
            }
            RequestCancelToken.ThrowIfCancellationRequested();

            await GetModular().WriteParameterAsync(mechanicalArmWithTransData.Address,data);
            if (!GetModular().VerifyModuleActivityStatus(mechanicalArmWithTransData.StateAddress))
            {
                await GetModular().ModuleExecuteAsync(mechanicalArmWithTransData.CommandAddress, (short)mechanicalArmWithTransData.ArmFuncCode);
            }
            await GetModular().CheckDoneAsync(mechanicalArmWithTransData.StateAddress,mechanicalArmWithTransData.CommandAddress, RequestCancelToken);
            SendToPin("输出搬运完成信号", new QData());
            return true;
        }

        public override void ApplyOnContextChanged(object context)
        {
            mechanicalArmWithTransData = Context<MechanicalArmWithTransData>();
        }
    }
}
