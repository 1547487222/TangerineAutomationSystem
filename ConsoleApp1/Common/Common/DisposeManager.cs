using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common
{
    public class DisposeManager:Singleton<DisposeManager>
    {
        private readonly List<Type> _disposables = [];
        public void Register(Type disposableType)
        {
            _disposables.Add(disposableType);
        }
        public void Dispose()
        {
            foreach (var disposable in _disposables)
            {
                var disposableInstance = Container.GetService(disposable);

                if (disposableInstance is IDisposable disposableObj)
                {
                    disposableObj.Dispose();
                }
            }
        }
    }
}
