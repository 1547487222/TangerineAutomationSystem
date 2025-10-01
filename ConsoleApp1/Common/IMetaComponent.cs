using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common
{
    /// <summary>
    /// 元构件
    /// </summary>
    public interface IMetaComponents
    {
        public IComponent? Get(string componentName);

        public T? Get<T>(string componentName) where T : IComponent;

        public bool TryGet<T>(string componentName, out IComponent component) where T : IComponent;

        public void Set(string componentName, IComponent value);
    }

    public class MetaComponents : IMetaComponents
    {
        private readonly Dictionary<string, IComponent> _metaData = [];
        private readonly object _metaDataLock = new();
        public IComponent? Get(string componentName)
        {
            lock (_metaDataLock)
            {
                if (_metaData.TryGetValue(componentName, out IComponent? value))
                    return value;
                else
                    return default;
            }
        }
        public void Set(string componentName, IComponent value)
        {
            lock (_metaDataLock)
            {
                _metaData[componentName] = value;
            }
        }

        public T? Get<T>(string componentName) where T : IComponent
        {
            lock (_metaDataLock)
            {
                var value = Get(componentName);
                if (value != null)
                    return (T)value;
                else return default;
            }
        }

        public bool TryGet<T>(string componentName, out IComponent component) where T : IComponent
        {
            lock (_metaDataLock)
            {
                var value = Get(componentName);
                if (value != null)
                {
                    component = value;
                    return true;
                }
                else
                {
                    component = default!;
                    return false;
                }
            }
        }
    }
}
