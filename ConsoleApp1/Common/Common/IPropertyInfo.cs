using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common
{
    public interface IPropertyInfo
    {
        void Set(string name, object value);

        object Get(string name);

        T Get<T>(string name);

        bool TryGet<T>(string name, out T value);
    }
}
