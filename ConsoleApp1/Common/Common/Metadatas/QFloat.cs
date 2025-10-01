using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common.Metadatas
{
    public class QFloat : QData
    {
        public QFloat() { }
        public QFloat(float value)
        {
            Value = value;
        }
        public float Value { get; set; }

        public static implicit operator QFloat(float f)
        {
            return new QFloat(f);
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
