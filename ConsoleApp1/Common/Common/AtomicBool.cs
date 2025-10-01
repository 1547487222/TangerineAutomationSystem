using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common
{
    public class AtomicBool
    {
        private int value;//0 false, 1 true

        public AtomicBool(bool initialValue)
        {
            value = initialValue ? 1 : 0;
        }

        public bool Get()
        {
            return value == 1;
        }

        public void Set(bool newValue)
        {
            _ = Interlocked.Exchange(ref value, newValue ? 1 : 0);
        }
    }
}
