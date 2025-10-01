using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Components.Sockets
{
    public interface IAppServer<TPackageInfo>
    {
        void Start();
        Dictionary<string, ISession<TPackageInfo>> SessionPairs { get; }
        void Stop();
    }
}
