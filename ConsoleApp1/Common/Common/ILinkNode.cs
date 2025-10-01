using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common
{
    public interface ILinkNode<T> : IPartObject where T : class, ILinkNode<T>
    {
        T? Parent { get; }
        List<T> LinkPins { get; }
    }
}
