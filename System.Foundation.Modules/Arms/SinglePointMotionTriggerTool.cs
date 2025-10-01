using Microsoft.Extensions.Logging;
using QStandaedPlatform.Engine.Common;
using QStandaedPlatform.Engine.Common.Common;
using QStandaedPlatform.Engine.Common.Common.Metadatas;
using QStandaedPlatform.Engine.Components;
using QStandaedPlatform.Engine.Laboratory;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Foundation.Modules.Arms
{
    [DisplayName("触发单点运动工具")]
    public class SinglePointMotionTriggerTool : SyncInputModuleToolBase
    {
        public SinglePointMotionTriggerData singlePointMotionData = new();

        public class SinglePointMotionTriggerData: ModuleData
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

            [DisplayName("功能码")]
            public int FuncCode { get; set; } = 6;

            [DisplayName("是否包含角度R")]
            public bool ContainAngle { get; set; } = true;



            [DisplayName("X轴偏移")]
            public float X_Offset { get; set; } = 0;

            [DisplayName("Y轴偏移")]
            public float Y_Offset { get; set; } = 0;

            [DisplayName("Z轴偏移")]
            public float Z_Offset { get; set; } = 0;
        }
        public override string DefineName => "触发单点运动工具";



        public override bool InitPins()
        {
            InsetPin("输入运动触发信号", this, typeof(QData), PinType.Input);
            InsetPin("输入运动坐标", this, typeof(QPosition), PinType.Input);
            InsetPin("输出运动完成信号", this, typeof(QData), PinType.Output);
            return true;
        }
        public override bool InitDataContext()
        {
            DataContext = singlePointMotionData;
            return true;
        }

        public override async Task<bool> ExexuteAsync(Dictionary<PinInfo, QData> pinDatas, ToolExecutionContext toolContext)
        {
            var position = (QPosition)pinDatas.FirstOrDefault(p=>p.Key.Name== "输入运动坐标").Value;
            Logger?.LogInformation($"X:{position.X},Y:{position.Y},Z:{position.Z},R:{position.Angle}");
            position.X += singlePointMotionData.X_Offset;
            position.Y += singlePointMotionData.Y_Offset;
            position.Z += singlePointMotionData.Z_Offset;
            //偏移后位置记录
            Logger?.LogInformation($"偏移后 X:{position.X},Y:{position.Y},Z:{position.Z},R:{position.Angle}");
            singlePointMotionData = Context<SinglePointMotionTriggerData>();
            float[] data;
            if (singlePointMotionData.ContainAngle)
            {
                data =
          [
                    position.X,position.Y,position.Z,position.Z2,position.Angle,
          ];
            }
            else
            {
                data =
           [
                    position.X,position.Y,position.Z,position.Z2,
           ];
            }
            //写入参数
            await GetModular().WriteParameterAsync(singlePointMotionData.Address, data);
            await GetModular().ModuleExecuteAsync(singlePointMotionData.CommandAddress, (short)singlePointMotionData.FuncCode);
            await GetModular().CheckDoneAsync(singlePointMotionData.StateAddress, singlePointMotionData.CommandAddress);
            SendToPin("输出运动完成信号", new QData());
            return true;
        }
    }
}
