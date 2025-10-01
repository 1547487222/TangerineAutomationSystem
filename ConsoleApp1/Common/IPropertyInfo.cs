using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common
{
    public interface IPropertyInfo
    {
        void Set(string name, object value);

        object Get(string name);

        T Get<T>(string name);

        bool TryGet<T>(string name, out T value);
    }


    public class PropertyInfo : IPropertyInfo
    {
        private readonly ConcurrentDictionary<string, object> _data = new();

        public object Get(string name)
        {
            if (_data.TryGetValue(name, out var value))
            {
                return value;
            }
            throw new KeyNotFoundException($"The key '{name}' was not found.");
        }

        public T Get<T>(string name)
        {
            if (_data.TryGetValue(name, out var value))
            {
                return (T)value;
            }
            throw new KeyNotFoundException($"The key '{name}' was not found.");
        }

        public void Set(string name, object value)
        {
            _data.AddOrUpdate(name, value, (key, oldValue) => value);
        }

        public bool TryGet<T>(string name, out T value)
        {
            if (_data.TryGetValue(name, out var objValue))
            {
                value = (T)objValue;
                return true;
            }
            value = default!;
            return false;
        }
    }
}
