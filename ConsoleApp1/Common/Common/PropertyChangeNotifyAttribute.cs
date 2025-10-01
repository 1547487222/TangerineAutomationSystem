using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common
{
    [AttributeUsage(AttributeTargets.Property)]
    public class PropertyChangeNotifyAttribute(string propertyName) : Attribute
    {
        public string PropertyName { get; } = propertyName;
    }
}
