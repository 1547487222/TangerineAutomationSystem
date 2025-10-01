using QStandaedPlatform.Engine.Common.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Laboratory
{
    //public class ModuleStateManager:Singleton<ModuleStateManager>
    //{
    //    private readonly ConcurrentDictionary<string, ModuleState> _states = new();
    //    private readonly object _lock = new();
    //    public ModuleState GetOrAdd(string key)
    //    {
    //        lock (_lock)
    //        {
    //            return _states.GetOrAdd(key, new ModuleState());
    //        }
    //    }

    //    public void Remove(string key)
    //    {
    //        lock (_lock)
    //        {
    //            _states.TryRemove(key, out _);
    //        }
    //    }

    //    public void Clear()
    //    {
    //        lock (_lock)
    //        {
    //            _states.Clear();
    //        }
    //    }
    //    public void Enter(string key,object args)
    //    {
    //        lock (_lock)
    //        {
    //            _states[key].Enter(args);
    //        }
    //    }

    //    public void Excute(string key, ModuleBehavior moduleBehavior)
    //    {
    //        lock (_lock)
    //        {
    //            _states[key].Execute(moduleBehavior);
    //        }
    //    }

    //    public void Exit(string key)
    //    {
    //        lock (_lock)
    //        {
    //            _states[key].Exit();
    //        }
    //    }

    //}
}
