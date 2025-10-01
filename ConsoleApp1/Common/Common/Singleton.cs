using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common
{
    public abstract class Singleton<T> where T :  Singleton<T>, new()
    {
        private static readonly Lazy<T> _instance = new(() =>
        {
            T instance = new();
            instance.Initialize();
            return instance;
        }, isThreadSafe: true);

        public static T Instance => _instance.Value;

        protected Singleton() { }

        protected virtual void Initialize()
        {

        }
    }
}
