using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common
{
    public interface IBackgroundWork
    {
       void Start();

     bool IsRunning { get; }
       void Stop();
    }

    public abstract class BackgroundWork : IBackgroundWork
    {
        private CancellationTokenSource? _tokenSource;
        private Thread? _thread;
        private volatile bool _isRunning;
        public void Start()
        {
            _tokenSource = new();
            _thread = new Thread(async () =>
            {
                _isRunning = true;
                _tokenSource.Token.Register(Cancel);
                await DoWorkAsync(_tokenSource.Token);
            })
            { IsBackground = true };
            _thread.Start();
        }

        public abstract Task DoWorkAsync(CancellationToken cancellationToken);

        public abstract void Cancel();

        public CancellationTokenSource TokenSource => _tokenSource ?? throw new Exception("TokenSource is null");

        public bool IsRunning => _isRunning;

        public void Stop()
        {
            if (_thread != null
                && _tokenSource != null && !_tokenSource.IsCancellationRequested)
            {
                _isRunning = false;
                _tokenSource.Cancel();
                _thread.Join();
                _tokenSource.Dispose();
                _thread = null;
            }
        }
    }

}
