using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Laboratory.Documents
{
    /// <summary>
    /// 夹爪夹取信息
    /// </summary>
    public class ClawGraspInfo: ICloneable
    {
        /// <summary>
        /// 开合度
        /// </summary>
        public float OpenPos { get; set; } = 0;
        /// <summary>
        /// 取料角度
        /// </summary>
        public float Angle { get; set; } = 0;

        public object Clone()
        {
            var claw = new ClawGraspInfo
            {
                OpenPos = OpenPos,
                Angle = Angle
            };
            return claw;
        }

        public override string ToString()
        {
            return $"OpenPos:{OpenPos},Angle:{Angle}";
        }
    }
}
