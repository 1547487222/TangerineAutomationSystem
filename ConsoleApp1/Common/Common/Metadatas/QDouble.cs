using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common.Metadatas
{
    public class QDouble : QData
    {
        public QDouble()
        {

        }
        public QDouble(double value)
        {
            Value = value;
        }
        public double Value { get; set; }

        public static implicit operator QDouble(double d)
        {
            return new QDouble(d);
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
