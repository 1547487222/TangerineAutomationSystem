using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common.Metadatas
{
    public class QDateTime : QData
    {
        public QDateTime() { }

        public QDateTime(DateTime value)
        {
            Value = value;
        }
        public DateTime Value { get; set; }

        public static implicit operator QDateTime(DateTime dt)
        {
            return new QDateTime(dt);
        }
    }
}
