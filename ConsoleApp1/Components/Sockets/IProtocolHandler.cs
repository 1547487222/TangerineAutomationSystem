using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Components.Sockets
{
    public interface IProtocolHandler
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="framedata"></param>
        /// <returns></returns>
        bool TryFilter(ref Sequence<byte> buffer, out byte[] framedata);
    }
}
