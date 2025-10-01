using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common
{
    public interface ILaboratory
    {
        void AddPlatform(IPlatform platform);

        void RemovePlatform(IPlatform platform);
    }
}
