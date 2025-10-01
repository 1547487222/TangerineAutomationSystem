using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Components.Sockets
{
    public class SessionOptions
    {
        /// <summary>
        /// 缓冲区长度
        /// </summary>
        public int BufferLength { get; set; }
    }
    public interface ISession<TPackageInfo> : IDisposable
    {
        event EventHandler<ISession<TPackageInfo>> SessionIOErrorClosed;
        bool Connected { get; }

        string SessionID { get; }
        /// <summary>
        /// 会话配置
        /// </summary>
        SessionOptions SessionOptions { get; }

        Task Start();
        /// <summary>
        /// 
        /// </summary>
        void Connect(string ip, int port);
        /// <summary>
        /// 关闭会话
        /// </summary>
        void Close();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageInfo"></param>
        void Send(TPackageInfo packageInfo);
        Task SendSync(TPackageInfo packageInfo);
    }
    public class Session<TPackageInfo> : ISession<TPackageInfo>
    {
        private TcpClient _tcpClient;
        private readonly NetworkStream _stream;
        private readonly IProtocolHandler _protocolHandler;
        private readonly IPackageHandler<TPackageInfo> _packageHandler;
        private readonly IPackageExecuter<TPackageInfo> _packageExecuter;
        private readonly SessionOptions _sessionOptions = new SessionOptions();
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        //private readonly ArrayPool<byte> _arrayPool = ArrayPool<byte>.Shared;
        private readonly string _sessionId;

        public event EventHandler<ISession<TPackageInfo>>? SessionIOErrorClosed;

        public Session(Action<SessionOptions> action, TcpClient tcpClient, IProtocolHandler protocolHandler, IPackageHandler<TPackageInfo> packageHandler, IPackageExecuter<TPackageInfo> packageExecuter)
        {
            action?.Invoke(_sessionOptions);
            _tcpClient = tcpClient;
            _stream = _tcpClient.GetStream();
            _protocolHandler = protocolHandler;
            _packageHandler = packageHandler;
            _packageExecuter = packageExecuter;
            _sessionId = Guid.NewGuid().ToString();
        }
        public bool Connected => _tcpClient.Connected;
        public string SessionID => _sessionId;

        public SessionOptions SessionOptions => _sessionOptions;
        public async Task Start()
        {
            byte[] data = new byte[(_sessionOptions.BufferLength == 0 ? 1024 : _sessionOptions.BufferLength)];
            int bytesRead;
            Sequence<byte> sequence = new Sequence<byte>(Array.Empty<byte>());
            try
            {
                while (!_cancellationTokenSource.IsCancellationRequested)
                {
                    if ((bytesRead = await _stream.ReadAsync(data, 0, data.Length, _cancellationTokenSource.Token)) > 0)
                    {
                        var tempArray = new byte[bytesRead];
                        Buffer.BlockCopy(data, 0, tempArray, 0, bytesRead);
                        sequence.Append(tempArray);
                        while (_protocolHandler.TryFilter(ref sequence, out var framedata) && !_cancellationTokenSource.IsCancellationRequested)
                        {
                            sequence.RemovePreviousOffset();
                            var packageInfo = _packageHandler.Decode(framedata);
                            await _packageExecuter.ExecuteAsync(this, packageInfo).ConfigureAwait(false);
                        }
                    }
                }
            }
            catch (IOException ex)
            {
                SessionIOErrorClosed?.Invoke(this, this);
            }
            catch (InvalidOperationException ex)
            {

            }
            finally
            {
                //_arrayPool.Return(data);
            }
        }
        public void Send(TPackageInfo packageInfo)
        {
            byte[] data = _packageHandler.Encode(packageInfo);
            _stream.Write(data, 0, data.Length);
        }
        public async Task SendSync(TPackageInfo packageInfo)
        {
            byte[] data = _packageHandler.Encode(packageInfo);
            await _stream.WriteAsync(data, 0, data.Length);
        }
        public void Close()
        {
            _cancellationTokenSource.Cancel();
            _tcpClient.Close();
            _stream.Close();
        }

        public void Dispose()
        {
            if (Connected)
                Close();
        }

        public void Connect(string ip, int port)
        {
            _tcpClient = new TcpClient(ip, port);
        }
    }
}
