using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Devices
{
    /// <summary>
    /// 超时机制
    /// </summary>
    /// <typeparam name="TTimeOutObject"></typeparam>
    public class TimeOutMechanism<TTimeOutObject>(TTimeOutObject timeOutObject) where TTimeOutObject : class
    {
        private readonly CancellationTokenSource _cts = new();

        /// <summary>
        /// 超时机制对象
        /// </summary>
        public TTimeOutObject TimeOutObject { get; } = timeOutObject ?? throw new ArgumentNullException(nameof(timeOutObject));
        /// <summary>
        /// 操作的开始时间
        /// </summary>
        public DateTime StartTime { get; private set; }
        /// <summary>
        /// 操作是否成功
        /// </summary>
        public bool IsSuccessful { get; set; }
        /// <summary>
        /// 延时的时间，单位毫秒，-1表示不会超时
        /// </summary>
        public int DelayTime { get; set; }
        /// <summary>
        /// 处理时休眠时间
        /// </summary>
        public int SleepTime { get; set; } = 100;
        /// <summary>
        /// 机制对象函数
        /// </summary>
        public Func<TTimeOutObject, bool>? Operator { get; set; }

        public event EventHandler? OnTickChange;

        public event EventHandler? OnTickComplete;

        public event EventHandler? OnTickTimeOut;

        public event EventHandler<Exception>? OnTickError;

        /// <summary>
        /// 机制执行
        /// </summary>
        public void Handle()
        {
            IsSuccessful = false;
            StartTime = DateTime.Now;
            TimeSpan timeSpan;
            if (DelayTime > 0)
            {
                timeSpan = new TimeSpan(0, 0, 0, 0, DelayTime);
            }
            else
            {
                timeSpan = Timeout.InfiniteTimeSpan;
            }
            while (!_cts.IsCancellationRequested)
            {
                try
                {
                    IsSuccessful = Operator?.Invoke(TimeOutObject) == true;
                    if (IsSuccessful)
                    {
                        OnTickComplete?.Invoke(this, EventArgs.Empty);
                        break;
                    }
                    if (DateTime.Now - StartTime >= timeSpan && timeSpan != Timeout.InfiniteTimeSpan)
                    {
                        OnTickTimeOut?.Invoke(this, EventArgs.Empty);
                        break;
                    }
                    OnTickChange?.Invoke(this, EventArgs.Empty);
                }
                catch (Exception ex)
                {
                    OnTickError?.Invoke(this, ex);
                }
                Thread.Sleep(SleepTime);
            }
        }

        public void Cancel()
        {
            _cts.Cancel();
        }

    }
}
