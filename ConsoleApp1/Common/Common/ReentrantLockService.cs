using Castle.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common
{
    /// <summary>
    /// 悲观锁服务
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ReentrantLockService<T> : IDisposable where T : class
    {
        private readonly Dictionary<T, Queue<Via>> _queues = [];
        private readonly Dictionary<T, Via> _lockers = [];
        private Thread? _thread;
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private readonly InvocationContextManager<long> _invocationContextManager = new();
        public ReentrantLockService()
        {
            Run();
        }
        public ReleaseOccupier<T> Acquire(T part, string key, CancellationToken cancellationToken = default)
        {

            lock (_queues)
            {
                if (_lockers.TryGetValue(part, out var existingVia)
                    && existingVia.OwnerKey == key
                    && existingVia.Locked
                    && existingVia.InvocationId == _invocationContextManager.Value)
                {
                    existingVia.ReentrantCount++;
                    return new ReleaseOccupier<T>(this, part, existingVia);
                }
            }
            var currentId = _invocationContextManager.Value;
            if (currentId == 0)
            {
                currentId = _invocationContextManager.Value = SnowflakeIdGenerator.Instance.GenerateYitId();
            }
            var via = new Via { OwnerKey = key, InvocationId = currentId };

            lock (_queues)
            {

                if (!_queues.TryGetValue(part, out var value))
                {
                    value = new Queue<Via>();
                    _queues[part] = value;
                }
                value.Enqueue(via);
            }

            var result = WaitHandle.WaitAny([via.Lock, cancellationToken.WaitHandle]);
            if (result == 1)
            {
                via.Lock.Dispose();
                throw new OperationCanceledException($"{part}:{key} Operation was canceled");
            }

            return new ReleaseOccupier<T>(this, part, via);
        }

        //轮询间隔
        public int PollingInterval { get; set; } = 1000;

        public void Run()
        {
            _thread = new Thread(() =>
            {
                while (!_cancellationTokenSource.IsCancellationRequested)
                {
                    lock (_queues)
                    {
                        foreach (var item in _queues)
                        {
                            if (_lockers.TryGetValue(item.Key, out var lockVia))
                            {
                                if (lockVia.Locked) continue;
                                _lockers.Remove(item.Key);
                            }

                            if (!_lockers.ContainsKey(item.Key) && item.Value.Count > 0)
                            {
                                var via = item.Value.Dequeue();
                                _lockers[item.Key] = via;
                                via.Open();
                            }
                        }
                    }
                    Thread.Sleep(PollingInterval);
                }
            })
            { IsBackground = true };
            _thread.Start();
        }
        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _thread?.Join();
            _thread = null;
        }

        public void Release(T part, Via via)
        {
            lock (_queues)
            {
                if (_lockers.TryGetValue(part, out var current) && current == via)
                {
                    if (via.ReentrantCount > 0)
                    {
                        via.ReentrantCount--;
                    }
                    else
                    {
                        via.Close();
                    }
                }
            }
        }
    }

    public class ReleaseOccupier<T>(ReentrantLockService<T> svc, T part, Via via) : IDisposable where T : class
    {
        private readonly ReentrantLockService<T> _svc = svc;
        private readonly T _part = part;
        private readonly Via _via = via;

        public void Dispose()
        {
            _svc.Release(_part, _via);
        }

        public T Part => _part;

        public Via Via => _via;
    }
    public class Via
    {
        public string OwnerKey { get; set; } = string.Empty;
        public AutoResetEvent Lock { get; set; } = new AutoResetEvent(false);
        public bool Locked { get; private set; } = false;
        public long InvocationId { get; set; }
        public int ReentrantCount { get; set; } = 0;

        public string Open()
        {
            Locked = true;
            Lock.Set();
            return OwnerKey;
        }

        public void Close()
        {
            Locked = false;
            Lock.Dispose();
        }
    }

}
