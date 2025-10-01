using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ReferencePartAttribute(bool isArray = false) : Attribute
    {
        public bool IsArray { get; set; } = isArray;
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class RefParameterAttribute<TTable> : Attribute where TTable : class,IParameterTable
    {
        public Type TableType => typeof(TTable);
    }
}
