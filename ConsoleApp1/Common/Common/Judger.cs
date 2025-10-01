using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common
{
    public class Judger
    {
        private readonly Func<Task<bool>> _conditionAsync;
        private volatile bool _cancel = false;
        private Func<bool>? _cancelfunc;
        private volatile bool _isRunning;
        public Judger(Func<Task<bool>> conditionAsync)
        {
            _conditionAsync = conditionAsync;
        }

        public int Interval { get; set; } = 1000;

        /// <summary>
        /// 主动取消属性
        /// </summary>
        public bool Cancel
        {
            get { return _cancel; }
            set
            {
                if (_isRunning)
                {
                    _cancel = value;
                }
            }
        }
        public bool IsRunning => _isRunning;
        public bool IsAutoResetCancel { get; set; } = true;
        public void RegisterCancelAction(Func<bool> cancelAction)
        {
            _cancelfunc += cancelAction;
        }

        public async Task<bool> SureAsync(int timeout = Timeout.Infinite, CancellationToken cancellationToken = default)
        {
            var startTime = Environment.TickCount64;
            while (true)
            {
                try
                {
                    _isRunning = true;
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return await Task.FromResult(false);
                    }
                    if (_cancel)
                    {
                        if (IsAutoResetCancel)
                        {
                            _cancel = false;
                        }
                        return await Task.FromResult(false);
                    }
                    if (_cancelfunc?.Invoke() == true)
                    {
                        return await Task.FromResult(false);
                    }
                    if (timeout != Timeout.Infinite && timeout > 0)
                    {
                        var result = await _conditionAsync();
                        if (result)
                        {
                            return result;
                        }
                        var endTime = Environment.TickCount64;
                        if (endTime - startTime >= timeout)
                        {
                            return result;
                        }
                    }
                    else
                    {
                        var result = await _conditionAsync();
                        if (result)
                            return result;
                    }
                    await Task.Delay(Interval, cancellationToken);
                }
                catch (Exception ex) when (ex is TaskCanceledException)
                {
                    return false;
                }
                finally 
                {
                    _isRunning = false;
                }
            }
        }
    }
}
