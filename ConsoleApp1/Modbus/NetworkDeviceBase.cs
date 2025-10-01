using Microsoft.Extensions.Logging;
using QStandaedPlatform.Engine.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Modbus
{
    /// <summary>
    /// 联网设备基类
    /// </summary>
    public abstract class NetworkDeviceBase : TraceBehaviorObject, IDisposable
    {
        private readonly TimeOutMechanism<NetworkDeviceBase> _timeOutMechanism;
        private NetworkStream? _stream;
        private int _disposed = 0;
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        protected NetworkDeviceBase()
        {
            NetworkId = Guid.NewGuid().ToString();
            NetworkClient = new TcpClient();
            _timeOutMechanism = new TimeOutMechanism<NetworkDeviceBase>(this);
            _timeOutMechanism.OnTickError += (sender, e) =>
            {
                OnNetworkDeviceExcetion?.Invoke(this, e);
            };
        }
        /// <summary>
        /// 获取或设置服务器IP地址
        /// </summary>
        public virtual string IpAddress { get; set; } = "127.0.0.1";
        /// <summary>
        /// 获取或设置服务器端口号
        /// </summary>
        public virtual int Port { get; set; } = 0;
        /// <summary>
        /// 获取或设置接收服务器反馈的超时时间
        /// </summary>
        public virtual int ReceiveTimeOut { get; set; } = -1;
        /// <summary>
        /// 获取或设置写入服务器超时时间 
        /// </summary>
        public virtual int WriteTimeOut { get; set; } = -1;
        /// <summary>
        /// 获取或设置连接服务器超时时间
        /// </summary>
        public virtual int ConnectTimeOut { get; set; } = -1;

        public virtual string NetworkId { get; set; }

        public TcpClient? NetworkClient { get; }

        public int BufferLength { get; set; } = 1_024;

        public bool Connected => NetworkClient != null && NetworkClient.Connected;

        public event EventHandler<Exception>? OnNetworkDeviceExcetion;

        /// <summary>
        /// 连接服务器
        /// </summary>
        public void ConnectServer()
        {
            ThrowIfDisposed();
            InitializationOnConnect();
            _stream = NetworkClient?.GetStream();
            if (_stream != null)
            {
                _stream.ReadTimeout = ReceiveTimeOut;
                _stream.WriteTimeout = WriteTimeOut;
            }
        }

        protected void InitializationOnConnect()
        {
            _timeOutMechanism.SleepTime = 1000;
            _timeOutMechanism.DelayTime = ConnectTimeOut;
            _timeOutMechanism.Operator = new
                 Func<NetworkDeviceBase, bool>(timeoutObject =>
                 {
                     timeoutObject.NetworkClient?.Connect(timeoutObject.IpAddress, timeoutObject.Port);
                     if (timeoutObject.NetworkClient?.Connected == true)
                         return true;
                     else return false;
                 });

            _timeOutMechanism.Handle();
        }
        /// <summary>
        /// 连接关闭
        /// </summary>
        public void ConnectClose()
        {
            ThrowIfDisposed();
            if (!Connected) return;
            NetworkClient?.Close();
        }
        /// <summary>
        /// TCP 发送并读取 函数  
        /// </summary>
        /// <param name="sendBytes"></param>
        /// <returns></returns>
        public byte[] ReadFromServer(byte[] sendBytes)
        {
            ThrowIfDisposed();
            try
            {
                //使用超时信号量锁
                using var cts = new CancellationTokenSource(ReceiveTimeOut);
                _semaphore.Wait(cts.Token);
                //判断是否处于无连接
                if (!Connected)
                {
                    //连接服务器
                    ConnectServer();
                }
                //socket.write
                _stream?.Write(sendBytes);
                var bytes = new byte[BufferLength];
                //socket.read
                var readCount = _stream?.Read(bytes);
                if (readCount == 0)
                    return [];
                if (readCount > 0)
                {
                    var readBytes = new byte[readCount.Value];
                    Array.Copy(bytes, readBytes, readCount.Value);
                    return readBytes;
                }
                return [];
            }
            catch (Exception ex)
            {
                var errorMessage = $"【NetworkDeviceBase.ReadFromServer】{ex}";
                Logger?.LogError(errorMessage);
                OnNetworkDeviceExcetion?.Invoke(this, ex);
                return [];
            }
            finally
            {
                _semaphore.Release();
            }
        }
        private void ThrowIfDisposed()
        {
            ObjectDisposedException.ThrowIf(_disposed != 0, this);
        }

        public void Dispose()
        {
            if (_disposed == 0)
            {
                ConnectClose();
                Interlocked.Exchange(ref _disposed, 1);
            }
        }
    }
}
