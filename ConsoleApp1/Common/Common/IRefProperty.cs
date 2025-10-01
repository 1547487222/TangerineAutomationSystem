using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common
{
    public interface IRefProperty
    {
        void InstallRef();
        void UnInstallRef();
    }

    public interface IRefPartProperty
    {
        Guid PartId { get; }
        IPart? RefPart { get; }
    }

    public interface IRefParameterProperty
    {
        Type? ModuleTableType { get; }

        Guid RefParameterTableId { get; }

        Guid RefParameterId { get; }

        IParameter? Parameter { get; }
    }
}
