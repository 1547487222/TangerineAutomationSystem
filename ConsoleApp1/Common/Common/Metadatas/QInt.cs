using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common.Metadatas
{
    public class QInt : QData
    {
        public QInt()
        {

        }
        public QInt(int value)
        {
            Value = value;
        }
        public int Value { get; set; }

        public static implicit operator QInt(int v)
        {
            return new QInt(v);
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
    public class QInt16 : QData
    {
        public QInt16()
        {

        }
        public QInt16(short value)
        {
            Value = value;
        }
        public short Value { get; set; }

        public static implicit operator QInt16(short v)
        {
            return new QInt16(v);
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }

    public class QInt64 : QData
    {
        public QInt64()
        {
            
        }
        public QInt64(long value)
        {
            Value = value;
        }

        public long Value { get; set; }

        public static implicit operator QInt64(long v)
        {
            return new QInt64(v);
        }


    }

}
