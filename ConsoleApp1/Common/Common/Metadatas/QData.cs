using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common.Metadatas
{
    public class QData
    {
        public static QData Empty { get; } = new QData();

        public static implicit operator float(QData qData)
        {
            ArgumentNullException.ThrowIfNull(qData);
            return ((QFloat)qData).Value;
        }
        public static implicit operator string(QData qData)
        {
            ArgumentNullException.ThrowIfNull(qData);
            return ((QString)qData).Value;
        }
        public static implicit operator double(QData qData)
        {
            ArgumentNullException.ThrowIfNull(qData);
            return ((QDouble)qData).Value;
        }
        public static implicit operator bool(QData qData)
        {
            ArgumentNullException.ThrowIfNull(qData);
            return ((QBoolean)qData).Value;
        }
        public static implicit operator DateTime(QData qData)
        {
            ArgumentNullException.ThrowIfNull(qData);
            return ((QDateTime)qData).Value;
        }

        public static implicit operator short(QData qData)
        {
            ArgumentNullException.ThrowIfNull(qData);
            return ((QInt16)qData).Value;
        }
        public static implicit operator long(QData qData)
        {
            ArgumentNullException.ThrowIfNull(qData);
            return ((QInt64)qData).Value;
        }
        public static implicit operator int(QData qData)
        {
            ArgumentNullException.ThrowIfNull(qData);
            return ((QInt)qData).Value;
        }





    }

    public class QEventCallBack : QData
    {
        public QEventCallBack(Func<Task> callBack)
        {
            CallBack = callBack;
        }
        public Func<Task> CallBack { get;  }
    }
}
