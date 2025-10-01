
using Microsoft.Extensions.Logging;
using NModbus;
using QStandaedPlatform.Engine.Common;
using QStandaedPlatform.Engine.Common.Common;
using System.ComponentModel;
using System.Net.Sockets;
using System.Text;

namespace QStandaedPlatform.Engine.Components.Modbus
{
    [DisplayName("汇川H5U PLC")]
    public class H5uModbusTcp : IH5uTcp,IPart
    {
        #region Private Members
        private readonly string _ip;
        private readonly int _port;
        private readonly byte _deviceAddress;
        private readonly int _delayTime;
        private readonly ILogger _logger;
        private TcpClient _tcpClient;
        private IModbusMaster _master;
        private System.Threading.Timer _timer;
        private bool _isPressed;
        private readonly char[] _bitElements = ['M', 'B', 'S', 'X', 'Y'];
        private readonly char[] _wordElements = ['D', 'R'];
        #endregion

        #region Properties
        public int AttemptTimes { get; set; } = 2;
        public string Ip => _ip;
        public int Port => _port;

        public EndianType _endian = EndianType.CDAB;
        public bool IsInitialized { get; private set; }


        public event EventHandler<EventArgs> PartCreated;
        #endregion

        #region Constructors
        public H5uModbusTcp(string ip, int port, byte deviceAddress = 1, int delayTime = 500)
        {
            _ip = ip;
            _port = port;
            _deviceAddress = deviceAddress;
            _delayTime = delayTime;
            _logger = LoggerProviderManager.GetLoggerFactory().CreateLogger($"<{ip}:{port}>.{deviceAddress}.H5uModbusTcp");
        }
        #endregion

        #region Initialization
        [Lock]
        public async Task<(bool, string)> OpenAsync()
        {
            try
            {
                _tcpClient = new TcpClient();
                await _tcpClient.ConnectAsync(_ip, _port).ConfigureAwait(false);
                _master = new ModbusFactory().CreateMaster(_tcpClient);
                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Modbus init error: {ex.Message}");
                return (false, ex.Message);
            }
        }
        [Lock]
        public (bool, string) Open()
        {
            try
            {
                _tcpClient = new TcpClient(_ip, _port);
                _master = new ModbusFactory().CreateMaster(_tcpClient);
                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Modbus init error: {ex}");
                return (false, ex.Message);
            }
        }
        #endregion

        #region Read Methods
        [Lock]
        public bool ReadSingleBoolean(string element)
            => ExecuteWithRetry(() => ReadSingleBooleanInternal(element));
        [Lock]
        public async Task<bool> ReadSingleBooleanAsync(string element)
            => await ExecuteWithRetryAsync(() => Task.FromResult(ReadSingleBoolean(element))).ConfigureAwait(false);
        [Lock]
        private bool ReadSingleBooleanInternal(string element)
        {
            ValidateAddress(element, 0);
            ushort addr = (ushort)ConvertBitElementToModbusAddress(element);
            return _master.ReadCoils(_deviceAddress, addr, 1)[0];
        }
        [Lock]
        public List<bool> ReadMultiBoolean(string element, int count)
            => ExecuteWithRetry(() =>
            {
                ValidateAddress(element, 0);
                ushort addr = (ushort)ConvertBitElementToModbusAddress(element);
                return new List<bool>(_master.ReadCoils(_deviceAddress, addr, (ushort)count));
            });
        [Lock]
        public async Task<List<bool>> ReadMultiBooleanAsync(string element, int count)
            => await ExecuteWithRetryAsync(() => Task.FromResult(ReadMultiBoolean(element, count))).ConfigureAwait(false);
        [Lock]
        public byte ReadByteFromMultiBoolean(string element)
        {
            var bits = ReadMultiBoolean(element, 8);
            return bits.Select((b, i) => b ? (byte)(1 << i) : (byte)0)
                       .Aggregate((a, b) => (byte)(a | b));
        }
        [Lock]
        public async Task<byte> ReadByteFromMultiBooleanAsync(string element)
            => await ExecuteWithRetryAsync(() => Task.FromResult(ReadByteFromMultiBoolean(element))).ConfigureAwait(false);
        [Lock]
        public T ReadSingleValue<T>(string element)
            => ExecuteWithRetry(() => ReadSingleValueInternal<T>(element));
        [Lock]
        public async Task<T> ReadSingleValueAsync<T>(string element)
            => await ExecuteWithRetryAsync(() => Task.FromResult(ReadSingleValue<T>(element))).ConfigureAwait(false);
        [Lock]
        private T ReadSingleValueInternal<T>(string element)
        {
            ValidateAddress(element, 1);
            ushort addr = (ushort)ConvertWordElementToModbusAddress(element);
            ushort[] regs = _master.ReadHoldingRegisters(_deviceAddress, addr, (ushort)GetRegisterCount<T>());
            return ConvertRegistersToValue<T>(regs);
        }
        [Lock]
        public List<T> ReadMultiValue<T>(string element, int count)
            => ExecuteWithRetry(() =>
            {
                ValidateAddress(element, 1);
                ushort addr = (ushort)ConvertWordElementToModbusAddress(element);
                ushort[] regs = _master.ReadHoldingRegisters(_deviceAddress, addr, (ushort)(count * GetRegisterCount<T>()));
                return ConvertRegistersToList<T>(regs, count);
            });
        [Lock]
        public async Task<List<T>> ReadMultiValueAsync<T>(string element, int count)
            => await ExecuteWithRetryAsync(() => Task.FromResult(ReadMultiValue<T>(element, count))).ConfigureAwait(false);
        [Lock]
        public string ReadString(string element, int length)
            => ExecuteWithRetry(() =>
            {
                ValidateAddress(element, 1);
                ushort addr = (ushort)ConvertWordElementToModbusAddress(element);
                ushort[] regs = _master.ReadHoldingRegisters(_deviceAddress, addr, (ushort)(length * 2));
                byte[] raw = regs.SelectMany(r => new[] { (byte)(r & 0xFF), (byte)(r >> 8) }).ToArray();
                return Encoding.ASCII.GetString(raw).TrimEnd('\0');
            });
        [Lock]
        public async Task<string> ReadStringAsync(string element, int length)
            => await ExecuteWithRetryAsync(() => Task.FromResult(ReadString(element, length))).ConfigureAwait(false);
        #endregion

        #region Write Methods
        [Lock]
        public bool WriteSingleBoolean(string element, bool value)
            => ExecuteWithRetry(() =>
            {
                ValidateAddress(element, 0);
                ushort addr = (ushort)ConvertBitElementToModbusAddress(element);
                _master.WriteSingleCoil(_deviceAddress, addr, value);
                return true;
            });
        [Lock]
        public async Task<bool> WriteSingleBooleanAsync(string element, bool value)
            => await ExecuteWithRetryAsync(() => Task.FromResult(WriteSingleBoolean(element, value))).ConfigureAwait(false);
        [Lock]
        public bool WriteMultiBoolean(string element, params bool[] value)
            => ExecuteWithRetry(() =>
            {
                ValidateAddress(element, 0);
                ushort addr = (ushort)ConvertBitElementToModbusAddress(element);
                _master.WriteMultipleCoils(_deviceAddress, addr, value);
                return true;
            });
        [Lock]
        public async Task<bool> WriteMultiBooleanAsync(string element, params bool[] value)
            => await ExecuteWithRetryAsync(() => Task.FromResult(WriteMultiBoolean(element, value))).ConfigureAwait(false);
        [Lock]
        public bool WriteSingleValue<T>(string element, T value)
            => ExecuteWithRetry(() =>
            {
                ValidateAddress(element, 1);
                ushort addr = (ushort)ConvertWordElementToModbusAddress(element);
                _master.WriteMultipleRegisters(_deviceAddress, addr, ConvertValueToRegisters(value));
                return true;
            });
        [Lock]
        public async Task<bool> WriteSingleValueAsync<T>(string element, T value)
            => await ExecuteWithRetryAsync(() => Task.FromResult(WriteSingleValue(element, value))).ConfigureAwait(false);
        [Lock]
        public bool WriteMultiValue<T>(string element, params T[] value)
            => ExecuteWithRetry(() =>
            {
                ValidateAddress(element, 1);
                ushort addr = (ushort)ConvertWordElementToModbusAddress(element);
                _master.WriteMultipleRegisters(_deviceAddress, addr, ConvertValuesToRegisters(value));
                return true;
            });
        [Lock]
        public async Task<bool> WriteMultiValueAsync<T>(string element, params T[] value)
            => await ExecuteWithRetryAsync(() => Task.FromResult(WriteMultiValue(element, value))).ConfigureAwait(false);
        #endregion

        #region Advanced Controls
        [Lock]
        public bool ReverseOutput(string element)
            => ExecuteWithRetry(() => WriteSingleBoolean(element, !ReadSingleBoolean(element)));
        [Lock]
        public Task<bool> ReverseOutputAsync(string element)
            => ExecuteWithRetryAsync(() => Task.FromResult(ReverseOutput(element)));
        [Lock]
        public bool PlusOutput(string element, int pulsTime)
            => ExecuteWithRetry(() =>
            {
                WriteSingleBoolean(element, true);
                Thread.Sleep(pulsTime);
                WriteSingleBoolean(element, false);
                return true;
            });
        [Lock]
        public Task<bool> PlusOutputAsync(string element, int pulsTime)
            => ExecuteWithRetryAsync(() => Task.FromResult(PlusOutput(element, pulsTime)));
        [Lock]
        public void PressOutput(string element)
        {
            _timer?.Dispose();
            WriteSingleBoolean(element, true);
            _timer = new Timer(_ => {
                try
                {
                    WriteSingleBoolean(element, false);
                }
                catch (Exception ex)
                {
                    _logger?.LogError($"Timer callback error: {ex}");
                }
            }, null, _delayTime, Timeout.Infinite);
        }
        [Lock]
        public async Task PressOutputAsync(string element)
        {
            _timer?.Dispose();
            await WriteSingleBooleanAsync(element, true);
            _timer = new Timer(_ => _ = WriteSingleBooleanAsync(element, false)
                                               .ContinueWith(t => t.Exception?.Handle(ex => {
                                                   _logger?.LogError(ex, "Timer async error");
                                                   return true;
                                               })),
                               null, _delayTime, Timeout.Infinite);
        }
        #endregion

        #region Utility Methods
        [Lock]
        private static int GetRegisterCount<T>() => typeof(T) == typeof(float) || typeof(T) == typeof(double) ? 2 : 1;
        [Lock]
        private static T ConvertRegistersToValue<T>(ushort[] regs)
        {
            byte[] raw = regs
                .SelectMany(r => new[] { (byte)(r & 0xFF), (byte)(r >> 8) })
                .ToArray();

            if (typeof(T) == typeof(float))
                return (T)(object)BitConverter.ToSingle(raw, 0);
            if (typeof(T) == typeof(double))
                return (T)(object)BitConverter.ToDouble(raw, 0);

            if (raw.Length == 2)
                return (T)Convert.ChangeType(BitConverter.ToUInt16(raw, 0), typeof(T));
            if (raw.Length == 4)
                return (T)Convert.ChangeType(BitConverter.ToUInt32(raw, 0), typeof(T));

            throw new NotSupportedException($"Unsupported raw length: {raw.Length}");
        }
        [Lock]
        private List<T> ConvertRegistersToList<T>(ushort[] regs, int count)
        {
            var list = new List<T>(count);
            for (int i = 0; i < count; i++)
            {
                var slice = regs.Skip(i * GetRegisterCount<T>()).Take(GetRegisterCount<T>()).ToArray();
                list.Add(ConvertRegistersToValue<T>(slice));
            }
            return list;
        }
        [Lock]
        private static ushort[] ConvertValueToRegisters<T>(T value)
        {
            byte[] raw;
            if (typeof(T) == typeof(float)) raw = BitConverter.GetBytes((float)(object)value);
            else if (typeof(T) == typeof(double)) raw = BitConverter.GetBytes((double)(object)value);
            else if (typeof(T) == typeof(int)) raw = BitConverter.GetBytes((int)(object)value);
            else if (typeof(T) == typeof(uint)) raw = BitConverter.GetBytes((uint)(object)value);
            else if (typeof(T) == typeof(short)) raw = BitConverter.GetBytes((short)(object)value);
            else if (typeof(T) == typeof(ushort)) raw = BitConverter.GetBytes((ushort)(object)value);
            else raw = BitConverter.GetBytes(Convert.ToUInt16(value));
            return Enumerable.Range(0, raw.Length / 2)
                             .Select(i => (ushort)(raw[2 * i] | (raw[2 * i + 1] << 8)))
                             .ToArray();
        }
        [Lock]
        private ushort[] ConvertValuesToRegisters<T>(T[] values)
            => values.SelectMany(v => ConvertValueToRegisters(v)).ToArray();
        [Lock]
        private void ValidateAddress(string element, int type)
        {
            var fmt = element.Trim().ToUpper();
            bool isValid = type == 0
                ? Array.Exists(_bitElements, c => fmt.StartsWith(c.ToString()))
                : Array.Exists(_wordElements, c => fmt.StartsWith(c.ToString()));
            if (!isValid)
                throw new ArgumentException($"地址格式错误: {element}");
        }
        [Lock]
        private static int ConvertBitElementToModbusAddress(string fmt)
            => fmt[0] switch
            {
                'X' => (int)BitSoftElement.X + Convert.ToInt32(fmt[1..], 8),
                'Y' => (int)BitSoftElement.Y + Convert.ToInt32(fmt[1..], 8),
                'M' => (int)BitSoftElement.M + int.Parse(fmt[1..]),
                'B' => (int)BitSoftElement.B + int.Parse(fmt[1..]),
                'S' => (int)BitSoftElement.S + int.Parse(fmt[1..]),
                _ => throw new ArgumentException($"位软元件格式错误: {fmt}"),
            };
        [Lock]
        private static int ConvertWordElementToModbusAddress(string fmt)
            => fmt[0] switch
            {
                'D' => (int)WordSoftElement.D + int.Parse(fmt[1..]),
                'R' => (int)WordSoftElement.R + int.Parse(fmt[1..]),
                _ => throw new ArgumentException($"字软元件格式错误: {fmt}"),
            };
        [Lock]
        public static byte[] ReorderBytes(byte[] raw, EndianType endian)
        {
            ArgumentNullException.ThrowIfNull(raw);
            if (raw.Length % 2 != 0)
                throw new ArgumentException("字节数组长度必须为偶数", nameof(raw));

            int len = raw.Length;
            // Number of 2-byte words
            int wordCount = len / 2;
            // Copy 避免修改原数组
            var result = new byte[len];

            switch (endian)
            {
                case EndianType.ABCD:
                    // 原顺序，直接复制
                    Buffer.BlockCopy(raw, 0, result, 0, len);
                    break;

                case EndianType.BADC:
                    // 每个 2 字节内交换 [0,1]→[1,0], [2,3]→[3,2] ...
                    for (int w = 0; w < wordCount; w++)
                    {
                        int i = w * 2;
                        result[i + 0] = raw[i + 1];
                        result[i + 1] = raw[i + 0];
                    }
                    break;

                case EndianType.CDAB:
                    // 以 2-字节为单元，整个序列按“2 字节块”倒置顺序
                    // e.g. [B0,B1, B2,B3, B4,B5, B6,B7] → [B4,B5, B6,B7, B0,B1, B2,B3]
                    for (int w = 0; w < wordCount; w++)
                    {
                        int srcIndex = (wordCount - 1 - w) * 2;
                        int dstIndex = w * 2;
                        result[dstIndex + 0] = raw[srcIndex + 0];
                        result[dstIndex + 1] = raw[srcIndex + 1];
                    }
                    break;

                case EndianType.DCBA:
                    // 完全反转整个数组
                    for (int i = 0; i < len; i++)
                    {
                        result[i] = raw[len - 1 - i];
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(endian), endian, null);
            }

            return result;
        }
        private readonly object _sync = new();
        private T ExecuteWithRetry<T>(Func<T> func)
        {
            lock (_sync)
            {
                int attempt = 0;
                while (true)
                {
                    try
                    {
                        EnsureConnected();
                        return func();
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError($"Modbus error: {ex}");
                        if (++attempt >= AttemptTimes) throw;
                        Thread.Sleep(50);
                    }
                }
            }
        }
        private readonly SemaphoreSlim _asyncSync = new(1, 1);
        private async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> func)
        {
            await _asyncSync.WaitAsync();
            try
            {
                int attempt = 0;
                while (true)
                {
                    try
                    {
                        await EnsureConnectedAsync().ConfigureAwait(false);
                        return await func();
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError($"Modbus async error: {ex}");
                        if (++attempt >= AttemptTimes) throw;
                        await Task.Delay(50).ConfigureAwait(false);
                    }
                }
            }
            finally
            {
                _asyncSync.Release();
            }
        }

        private readonly SemaphoreSlim _connectLock = new(1, 1);

        private async Task EnsureConnectedAsync()
        {
            if (_master != null && _tcpClient != null && _tcpClient?.Connected == true) return;
            await _connectLock.WaitAsync();
            try
            {
                if (_master == null || _tcpClient == null || !_tcpClient.Connected)
                {
                    var (ok, msg) = await OpenAsync();
                    if (!ok) throw new Exception($"Modbus reconnect failed: {msg}");
                }
            }
            finally
            {
                _connectLock.Release();
            }
        }
        private object Lock { get; } = new object();
        private void EnsureConnected()
        {
            if (_master != null && _tcpClient != null && _tcpClient.Connected) return;
            lock (Lock)
            {
                if (_master == null || _tcpClient == null || !_tcpClient.Connected)
                {
                    var (ok, msg) = Open();
                    if (!ok) throw new Exception($"Modbus reconnect failed: {msg}");
                }
            }
        }

        [Lock]
        public void Initialize()
        {
            var result = Open();
            this.IsInitialized = result.Item1;
            PartCreated?.Invoke(this, EventArgs.Empty);
        }
        [Lock]
        public void Shutdown()
        {
            try { _master?.Dispose(); }
            catch { /*ignore*/ }

            try { _tcpClient?.Close(); _tcpClient?.Dispose(); }
            catch { /*ignore*/ }
            finally
            {
                _master = null;
                _tcpClient = null;
                this.IsInitialized = false;
            }
        }
        #endregion
    }

    public enum BitSoftElement {
        M = 0,
        B = 12288,
        S = 57344,
        X = 63488,
        Y = 64512
    }
    public enum WordSoftElement
    {
        D = 0,
        R = 12288
    }
    public enum EndianType
    {
        ABCD, CDAB, BADC, DCBA
    }
}
