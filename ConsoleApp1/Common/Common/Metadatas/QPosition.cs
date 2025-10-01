using QStandaedPlatform.Engine.Laboratory;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common.Metadatas
{
    public class QPosition : QData,ICloneable
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float Z2 { get; set; }
        /// <summary>
        /// z轴探入深度
        /// </summary>
        public float Depth { get; set; }
        /// <summary>
        /// z偏移量
        /// </summary>
        public float ZPutGetOffset { get; set; }
        /// <summary>
        /// 角度
        /// </summary>
        public float Angle { get; set; }

        public object Clone()
        {
            var clone = new QPosition
            {
                X = X,
                Y = Y,
                Z = Z,
                Z2 = Z2,
                Depth = Depth,
                ZPutGetOffset = ZPutGetOffset,
                Angle = Angle,
            };
            return clone;
        }

        public override string ToString()
        {
            return $"X:{X},Y:{Y},Z:{Z},Z2:{Z2},Depth:{Depth},ZPutGetOffset:{ZPutGetOffset},Angle:{Angle}";
        }

    }



}
