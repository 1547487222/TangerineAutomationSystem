using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Components.Sockets
{
    public interface IPackageExecuter<TPackageInfo>
    {
        Task ExecuteAsync(ISession<TPackageInfo> session, TPackageInfo packageInfo);
    }
}
