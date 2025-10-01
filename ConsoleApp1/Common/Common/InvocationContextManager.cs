using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common
{
    public class InvocationContextManager<T>
    {
        private static readonly ConcurrentDictionary<(Guid, InvocationContextManager<T>), T> _store = new();

        private static readonly AsyncLocal<Guid> _contextId = new();

        public T? Value
        {
            get
            {
                var id = GetCurrentContextId();
                _store.TryGetValue((id, this), out var value);
                return value;
            }
            set
            {
                var id = GetCurrentContextId();
                _store[(id, this)] = value!;
            }
        }

        private static Guid GetCurrentContextId()
        {
            if (_contextId.Value == Guid.Empty)
            {
                _contextId.Value = Guid.NewGuid();
            }
            return _contextId.Value;
        }
    }
}
