using QStandaedPlatform.Engine.Common.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equipment.Bqjx.StandardPlatformSystem.Models
{
    public interface IParameterModel
    {
        IParameterTable Table { get; }
        string ParameterDescription { get; }
        IParameter Parameter { get; }
    }
}
