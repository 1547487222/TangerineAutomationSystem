using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common.Metadatas
{
    public class QString : QData
    {
        public QString() { }

        public QString(string value)
        {
            Value = value;

        }
        public string Value { get; set; }

        public static implicit operator QString(string s)
        {
            return new QString(s);
        }
        public override string ToString()
        {
            return Value;
        }
    }
}
