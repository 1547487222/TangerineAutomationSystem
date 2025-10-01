using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common
{
    public enum DataType
    {
        QData,

        QInt16,

        QInt,

        QFloat,

        QDouble,

        QDateTime,

        QTimeSpan,

        QString,

        QBoolean,

        QBinary,

        QPosition,

        QDynamic,

        QArray = 5001,

        QDictionary,

        QJson,

        QModel,

        QFlowResult
    }
}
