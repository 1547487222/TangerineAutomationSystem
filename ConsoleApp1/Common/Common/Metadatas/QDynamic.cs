using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common.Metadatas
{
    public class QDynamic : QData
    {
        public QDynamic(QData qData)
        {
            Value = qData;
        }
        public QData Value { get; set; }
    }
}
