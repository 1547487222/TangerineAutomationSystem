using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common
{

    public class TemporaryTask
    {
        public long Id { get; set; }
        public int RemainingRounds { get; set; } = 30 * 60; // 30 minutes

    }
    public class TemporaryTimingWheel : IDisposable
    {
        private readonly Timer _timer;
        private readonly int _tickMs = 1000;
        private readonly Dictionary<string,List<TemporaryTask>>_buckets = [];
        private readonly Dictionary<string, Action<TemporaryTask>> _notifyActions = [];
        private readonly object _lock = new();


        public TemporaryTimingWheel()
        {
            _timer = new Timer(TimerCallback, null, _tickMs, _tickMs);
        }

        public void AddTask(string key, TemporaryTask task)
        {
            lock (_lock)
            {
                if (!_buckets.TryGetValue(key, out List<TemporaryTask>? value))
                {
                    value = ([]);
                    _buckets.Add(key, value);
                }
                value.Add(task);
            }
        }


        private void TimerCallback(object? state)
        {
            lock (_lock)
            {
                foreach (var bucket in _buckets)
                {
                    for (global::System.Int32 i = (bucket.Value.Count) - (1); i >= 0; i--)
                    {
                        var task = bucket.Value[i];
                        task.RemainingRounds--;
                        if (task.RemainingRounds <= 0)
                        {
                            Notify(bucket.Key, task);
                            bucket.Value.RemoveAt(i);
                        }
                    }
                }
            }
        }

        public void RegisterNotify(string key, Action<TemporaryTask> action)
        {
            lock (_lock)
            {
                _notifyActions[key] = action;
            }
        }

        public void Notify(string key,TemporaryTask temporaryTask)
        {
            lock (_lock)
            {
                if (_notifyActions.TryGetValue(key, out Action<TemporaryTask>? action))
                {
                    action(temporaryTask);
                }
            }
        }


        public void Dispose()
        {
          _timer.Dispose();
        }
    }
}
