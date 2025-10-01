using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common.Metadatas
{
    public class QClaw : QData, ICloneable
    {
        /// <summary>
        /// 取料开合度
        /// </summary>
        public float OpenPos { get; set; } = 0;
        /// <summary>
        /// 取料角度
        /// </summary>
        public float OpenAngle { get; set; } = 0;



        public object Clone()
        {
            var clone = new QClaw
            {
                OpenPos = OpenPos,
                OpenAngle = OpenAngle
            };
            return clone;
        }

        public override string ToString()
        {
            return $"OpenPos:{OpenPos},OpenAngle:{OpenAngle}";
        }

    }
}
