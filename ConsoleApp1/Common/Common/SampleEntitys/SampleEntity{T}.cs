using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common.SampleEntitys
{
    public class SampleEntity<T> where T : unmanaged
    {
        public T Id { get; set; }
    }
}
