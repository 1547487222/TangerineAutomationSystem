using QStandaedPlatform.Engine.Common.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Laboratory
{

    /// <summary>
    /// 任务标识器
    /// </summary>
    public class RoboticArmTaskMarker
    {
        public long TaskId { get; set; }

        public int PositionMark { get; set; } = 0;

        public bool IsCompleted { get; set; } = false;
    }
    public class RoboticArmTaskScheduler : ITransportTaskScheduler<Modular>
    {
        private readonly PriorityQueue<RoboticArmTask, RoboticArmTaskPriority> _taskQueue = new(new RoboticArmTaskPriorityComparer());
        private readonly ConcurrentDictionary<Guid, RoboticArmTaskResult> _taskResults = new();
        private readonly CancellationTokenSource _cts = new();
        private Thread? _workerThread;
        private readonly AutoResetEvent _workAvailableEvent = new(false);
        private SemaphoreSlim _taskCancelSemaphore = new(1, 1);

        public RoboticArmTaskScheduler()
        {

        }
        public void NotifyTaskArrived()
        {
            _workAvailableEvent.Set();
        }


        public event Action<Exception>? OnArmError;


        public void CancelTask()
        {
            _taskCancelSemaphore.Release();
        }





        //复位任务
        public void ResetTask()
        {
            if (_taskCancelSemaphore != null)
            {
                try
                {
                    _taskCancelSemaphore.Dispose();
                }
                catch (Exception) { }
            }
            _taskCancelSemaphore = new SemaphoreSlim(1, 1);
        }

        //注册任务
        public void RegisterTask(Modular arm, Func<Modular, Task> action, RoboticArmTaskPriority priority)
        {

        }


        //public async Task<RoboticArmTaskResult> SubmitTaskAsync()
        //{
        //    await Task.CompletedTask;
        //    var task = new RoboticArmTask
        //    {
        //        TaskId = Guid.NewGuid(),
        //        Arm = arm,
        //        Action = action
        //    };

        //    lock (_taskQueue)
        //    {
        //        _taskQueue.Enqueue(task, priority);
        //    }

        //    NotifyTaskArrived();

        //    //var avail = WaitHandle.WaitAny([task.Semaphore.AvailableWaitHandle, _taskCancelSemaphore.AvailableWaitHandle]);
        //    await task.Semaphore.WaitAsync();
        //   // try
        //    //{
        //       // if (avail == 1)
        //       // {
        //           // _taskCancelSemaphore.Dispose();
        //            //return new RoboticArmTaskResult
        //            //{
        //                //Success = false,
        //                //Exception = new InvalidOperationException("任务被取消")
        //            //};
        //       // }
        //       // else
        //        //{
        //            //task.Semaphore.Dispose();
        //            if (_taskResults.TryRemove(task.TaskId, out var result))
        //            {
        //                return result;
        //            }

        //            return new RoboticArmTaskResult
        //            {
        //                Success = false,
        //                Exception = new InvalidOperationException("任务结果未找到")
        //            };
        //       // }
        //   // }
        //    //catch (Exception ex)
        //    //{
        //        //return new RoboticArmTaskResult
        //        //{
        //           // Success = false,
        //           // Exception = ex
        //        //};
        //    //}
        //}


        public void Start()
        {
            _workerThread = new Thread(async () =>
            {
                while (!_cts.IsCancellationRequested)
                {
                    var waitHandleIndex = WaitHandle.WaitAny([_workAvailableEvent, _cts.Token.WaitHandle]);

                    if (waitHandleIndex == 0)
                    {
                        while (!_cts.IsCancellationRequested)
                        {
                            RoboticArmTask? currentTask = null;

                            lock (_taskQueue)
                            {
                                if (_taskQueue.Count == 0) break;
                                currentTask = _taskQueue.Peek();
                            }

                            try
                            {
                                if (currentTask != null)
                                {
                                    if (currentTask.Action != null && currentTask.Arm != null)
                                    {
                                        await currentTask.Action(currentTask.Arm);
                                        _taskResults[currentTask.TaskId] = new RoboticArmTaskResult
                                        {
                                            Success = true,
                                            Exception = null
                                        };

                                        currentTask.Semaphore.Release();

                                        lock (_taskQueue)
                                        {
                                            _taskQueue.Dequeue();
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                OnArmError?.Invoke(ex);
                                break;
                            }
                        }
                    }
                    else if (waitHandleIndex == 1)
                    {
                        // 取消请求，退出循环
                        break;
                    }
                }
            })
            {
                IsBackground = true
            };

            _workerThread.Start();
        }


        public void Stop()
        {
            ClearPendingTasks();
            _cts.Cancel();
            _workerThread?.Join();
        }


        public void ClearPendingTasks()
        {
            lock (_taskQueue)
            {
                try
                {
                    while (_taskQueue.Count > 0)
                    {
                        var task = _taskQueue.Dequeue();
                        task.Semaphore.Dispose();
                    }
                }
                catch (Exception) { }
                _taskQueue.Clear();
            }
        }

        public Task<RoboticArmTaskResult> SubmitTaskAsync(Modular arm, Func<Modular, Task> action, RoboticArmTaskPriority priority)
        {
            throw new NotImplementedException();
        }
    }


    public class RoboticArmTask
    {
        public Guid TaskId { get; init; }

        public Func<Modular, Task>? Action { get; set; }

        public Modular? Arm { get; set; }

        public readonly SemaphoreSlim Semaphore = new(0, 1);
    }
}
