using QStandaedPlatform.Engine.Common.Common;
using System.ComponentModel;

namespace System.Foundation.Modules.Arms
{
    public class TrayModArmData : ModuleData
    {
        [DisplayName("X轴起始偏移")]
        public float X_Start_Offset { get; set; } = 0;

        [DisplayName("Y轴起始偏移")]
        public float Y_Start_Offset { get; set; } = 0;

        [DisplayName("Z轴起始偏移")]
        public float Z_Start_Offset { get; set; } = 0;

        [DisplayName("Z2起始偏移")]
        public float Z2_Start_Offset { get; set; } = 0;

        [DisplayName("R轴起始偏移")]
        public float R_Start_Offset { get; set; } = 0;

        [DisplayName("X轴结束偏移")]
        public float X_End_Offset { get; set; } = 0;

        [DisplayName("Y轴结束偏移")]
        public float Y_End_Offset { get; set; } = 0;

        [DisplayName("Z轴结束偏移")]
        public float Z_End_Offset { get; set; } = 0;
        [DisplayName("Z2轴结束偏移")]
        public float Z2_End_Offset { get; set; } = 0;
        [DisplayName("R轴结束偏移")]
        public float R_End_Offset { get; set; } = 0;
    }

}
