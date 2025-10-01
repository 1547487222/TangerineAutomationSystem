using Castle.DynamicProxy;
using QStandaedPlatform.Engine.Common.Common;
using QStandaedPlatform.Engine.Components;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Laboratory
{
    public class ModularManager : Singleton<ModularManager>
    {
        private readonly ProxyGenerator _proxyGenerator = new();
        private readonly AsyncLockInterceptor _asyncLockInterceptor = new();
        private readonly ConcurrentDictionary<Guid, Modular> _modulars = new();
        private readonly ConcurrentDictionary<Guid, IH5uTcp> _modularsId = new();
        public Modular GetOrAddModular(Guid modularId, IH5uTcp h5UTcp)
        {
            var modular = _modulars.GetOrAdd(modularId, _ => new ModularFactory(_proxyGenerator, _asyncLockInterceptor).Create(h5UTcp));
            _modularsId.TryAdd(modularId, h5UTcp);
            return modular;
        }

        public Modular? GetModular(Guid modularId)
        {
            return _modularsId.TryGetValue(modularId, out var h5UTcp) ? _modulars[modularId] : null;
        }
    }

    public class ModularFactory(ProxyGenerator proxyGenerator, IAsyncInterceptor asyncLockInterceptor)
    {
        private readonly ProxyGenerator _proxyGenerator = proxyGenerator;
        private readonly IAsyncInterceptor _asyncLockInterceptor = asyncLockInterceptor;

        public Modular Create(IH5uTcp h5UTcp)
        {
            return _proxyGenerator.CreateClassProxy<Modular>(constructorArguments:[h5UTcp], ProxyGeneratorExtensions.ToInterceptor(_asyncLockInterceptor));
        }
    }
}
