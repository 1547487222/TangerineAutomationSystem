using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Components.Sockets
{
    public abstract class AppServer<TPackageInfo> : IAppServer<TPackageInfo>
    {

        private readonly TcpListener _listener;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly Dictionary<string, ISession<TPackageInfo>> _dict = new();
        private readonly Dictionary<string, Task> _taskDict = [];

        public Dictionary<string, ISession<TPackageInfo>> SessionPairs => _dict;

        public AppServer(int port)
        {
            _listener = new TcpListener(IPAddress.Any, port);
        }

        public AppServer(string ip, int port)
        {
            _listener = new TcpListener(IPAddress.Parse(ip), port);
        }
        public void Start()
        {
            _listener.Start();
            Task.Factory.StartNew(async () =>
            {
                while (!_cancellationTokenSource.IsCancellationRequested)
                {
                    var tcpClient = await _listener.AcceptTcpClientAsync();
                    var session = CreateSession(tcpClient);
                    _dict[session.SessionID] = session;
                    _taskDict[session.SessionID] = Task.Factory.StartNew(session.Start, _cancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
                }
            });
        }
        public abstract ISession<TPackageInfo> CreateSession(TcpClient tcpClient);
        public void Stop()
        {
            _cancellationTokenSource.Cancel();
            _listener.Stop();
            foreach (var session in _dict.Values)
            {
                session.Dispose();
            }
            _dict.Clear();
            _taskDict.Clear();
        }
    }
}
