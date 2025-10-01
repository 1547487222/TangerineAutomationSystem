using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common.Metadatas
{
    public class QBoolean : QData
    {
        public QBoolean() { }

        public QBoolean(bool value)
        {
            Value = value;
        }
        public bool Value { get; set; }

        public static implicit operator QBoolean(bool b)
        {
            return new QBoolean(b);
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
