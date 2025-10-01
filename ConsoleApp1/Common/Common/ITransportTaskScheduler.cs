using QStandaedPlatform.Engine.Laboratory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common
{
    public interface ITransportTaskScheduler<T>
    {
        void NotifyTaskArrived();

        event Action<Exception>? OnArmError;

        Task<RoboticArmTaskResult> SubmitTaskAsync(T arm,Func<T, Task> action, RoboticArmTaskPriority priority);
        void Start();
        void Stop();
        void ClearPendingTasks();
    }
}
