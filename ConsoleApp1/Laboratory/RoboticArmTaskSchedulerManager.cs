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
    //public class RoboticArmTaskSchedulerManager : Singleton<RoboticArmTaskSchedulerManager>
    //{
    //    private readonly ConcurrentDictionary<IH5uTcp, RoboticArmTaskScheduler>
    //        _roboticArmTaskScheduler = new();

    //    public RoboticArmTaskScheduler GetOrAddScheduler(IH5uTcp h5UTcp)
    //    {
    //        return _roboticArmTaskScheduler.GetOrAdd(h5UTcp, _ =>
    //        {
    //            var taskScheduler = new RoboticArmTaskScheduler();
    //            taskScheduler.Start();
    //            return taskScheduler;
    //        });
    //    }

    //    public void StopScheduler()
    //    {
    //        foreach (var item in _roboticArmTaskScheduler.Values)
    //        {
    //            item.Stop();
    //        } 
    //    }
    //}
}
