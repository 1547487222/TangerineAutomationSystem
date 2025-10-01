using QStandaedPlatform.Engine.Common.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Foundation.Modules.Arms
{
    public class ArmData : ModuleData
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

        [DisplayName("搬运功能码")]
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

        [DisplayName("扫码起始地址")]
        public string ScanCodeAdress { get; set; } = "D7850";

        [DisplayName("扫码长度")]
        public int ScanCodeLength { get; set; } = 10;

    }
}
