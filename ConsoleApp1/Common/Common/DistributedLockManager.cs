using QStandaedPlatform.Engine.Common.Common.Metadatas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common
{
    public class DistributedData
    {
        public List<QData> Values { get; set; } = [];
    }
    /// <summary>
    /// 分布式锁管理器
    /// </summary>
    public class DistributedLockManager(ReentrantLockService<DistributedData> reentrantLockService)
    {
        private readonly Dictionary<string, ReleaseOccupier<DistributedData>> _locks = [];
        private readonly Dictionary<string, DistributedData> _lockDatas = [];
        private readonly Dictionary<string, bool>
         _lockWaitFors = [];
        private readonly object _syncRoot = new();

        private readonly ReentrantLockService<DistributedData> _reentrantLockService = reentrantLockService;

        public void CreateDistributedLock(string key, CancellationToken cancellationToken = default, params QData[] values)
        {
            lock (_syncRoot)
            {
                if (!_lockDatas.TryGetValue(key, out DistributedData? value))
                {
                    value = new DistributedData() { Values = [.. values] };
                    _lockDatas[key] = value;
                }
                _lockWaitFors[key] = true;
                _locks[key] = _reentrantLockService.Acquire(value, key,cancellationToken);
            }
        }

        public DistributedData ReleaseDistributedLock(string key)
        {
            lock (_syncRoot)
            {
                var distributedData = _locks[key].Part;
                using (_locks[key])
                {
                    _lockWaitFors[key] = false;
                }
                OnDistributedLockReleased?.Invoke(key);
                return distributedData;
            }
        }

        public void RemoveDistributedLock(string key)
        {
            lock (_syncRoot)
            {
                using (_locks[key])
                {
                    _lockWaitFors[key] = false;
                }
                _lockDatas.Remove(key);
            }
        }

        public event Action<string>? OnDistributedLockReleased;

        public bool IsWaitForDistributedLock(string key)
        {
            lock (_syncRoot)
            {
                if (!_lockWaitFors.TryGetValue(key, out bool value))
                {
                    return false;
                }
                return value;
            }
        }
    }
}
