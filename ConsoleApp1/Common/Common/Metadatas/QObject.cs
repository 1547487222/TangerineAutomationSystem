using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common.Metadatas
{
    public class QObject : QData
    {
        public QObject() { }
        public QObject(object obj)
        {
            Object = obj;
        }
        public object Object { get; set; }
    }
}
