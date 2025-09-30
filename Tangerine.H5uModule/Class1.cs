using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NModbus;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Net.Sockets;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using Tangerine.Framework;

namespace Tangerine.H5uModule
{
    public class H5uModuleProvider : IModuleProvider<H5uModuleInfoConfig>
    {
        public List<H5uModuleInfoConfig> Configs { get; set; } = [];

        public Type ModuleControllerType => typeof(H5uModbusTcp);

        public Type ModuleAlarmOptionType => typeof(ModuleAlarmItem);

        public Type ModuleOptionType => typeof(H5uModuleOption);

        public string ModuleProviderName => "H5U通用模块";

        public Type ControllerType => typeof(H5uTcpOption);


        public IFunctionProvider BuildFunctionProvider(object moduleController)
        {
            if (moduleController is H5uModbusTcp h5UModbusTcp)
            {
                return new H5uModuleFunctionProvider(h5UModbusTcp);
            }
            throw new ArgumentException($"H5uModuleProvider模块控制器类型错误: {moduleController.GetType().FullName}");
        }

        public void ConfigureModule()
        {
            Configs = [];
        }

        public bool ConfigureService(IServiceCollection services)
        {
            return true;
        }

        public object OptionImport()
        {
            return new H5uTcpOption();
        }

        public bool ServiceProvider(IServiceProvider serviceProvider)
        {
            return true;
        }

        public IPart Structurer(object option)
        {
            if (option is H5uTcpOption h5uTcpOption)
            {
                return new H5uModbusTcp(h5uTcpOption.Ip, h5uTcpOption.Port);
            }
            throw new ArgumentException($"Structurer模块选项类型错误: {option.GetType().FullName}");
        }
    }
    /// <summary>
    /// H5U TCP选项
    /// </summary>
    public class H5uTcpOption
    {
        [Display(Name = "IP地址", Description = "PLC的IP地址", Order = 1)]
        public string Ip { get; set; } = "192.168.10.10";

        [Display(Name = "端口号", Description = "Modbus端口", Order = 2)]
        public int Port { get; set; } = 502;

        [Display(Name = "设备地址", Description = "Modbus设备地址", Order = 3)]
        public byte DeviceAddress { get; set; } = 1;

        [Display(Name = "重试次数", Description = "通信失败重试次数", Order = 4)]
        public int RetryCount { get; set; } = 3;
    }
    public interface IH5uTcp
        {

            string Ip { get; }

            int Port { get; }
            Task<(bool, string)> OpenAsync();
            (bool, string) Open();
            /// <summary>
            /// 读取单个bool变量
            /// </summary>
            /// <param name="element">软元件地址</param>
            /// <returns></returns>
            bool ReadSingleBoolean(string element);

            /// <summary>
            /// 写入单个bool变量
            /// </summary>
            /// <param name="element">软元件地址</param>
            /// <param name="value">写入值</param>
            /// <returns></returns>
            bool WriteSingleBoolean(string element, bool value);

            /// <summary>
            /// 读取单个类型数据
            /// </summary>
            /// <typeparam name="T">类型</typeparam>
            /// <param name="element">软元件地址</param>
            /// <returns></returns>
            T ReadSingleValue<T>(string element);

            /// <summary>
            /// 写入单个类型数据
            /// </summary>
            /// <typeparam name="T">类型</typeparam>
            /// <param name="element">软元件地址</param>
            /// <param name="Value">写入值</param>
            /// <returns></returns>
            bool WriteSingleValue<T>(string element, T Value);

            /// <summary>
            /// 读取多个bool变量
            /// </summary>
            /// <param name="element">软元件起始地址</param>
            /// <param name="count">数量</param>
            /// <returns></returns>
            List<bool> ReadMultiBoolean(string element, int count);

            /// <summary>
            /// 写入多个bool变量
            /// </summary>
            /// <param name="element">软元件起始地址</param>
            /// <param name="value">写入值</param>
            /// <returns></returns>
            bool WriteMultiBoolean(string element, params bool[] value);

            /// <summary>
            /// 读取多个类型数据
            /// </summary>
            /// <typeparam name="T">数据类型</typeparam>
            /// <param name="element">软元件起始地址</param>
            /// <param name="count">数量(读取数量)</param>
            /// <returns></returns>
            List<T> ReadMultiValue<T>(string element, int count);

            /// <summary>
            /// 写入多个类型数据
            /// </summary>
            /// <typeparam name="T">数据类型</typeparam>
            /// <param name="element">软元件起始地址</param>
            /// <param name="value">写入值</param>
            /// <returns></returns>
            bool WriteMultiValue<T>(string element, params T[] value);

            /// <summary>
            /// 翻转输出
            /// </summary>
            /// <param name="element"></param>
            /// <returns></returns>
            bool ReverseOutput(string element);

            /// <summary>
            /// 脉冲输出
            /// </summary>
            /// <param name="element"></param>
            /// <param name="pulsTime">单位毫秒</param>
            /// <returns></returns>
            bool PlusOutput(string element, int pulsTime);

            /// <summary>
            /// 读取一个byte的线圈
            /// </summary>
            /// <param name="element"></param>
            /// <returns></returns>
            byte ReadByteFromMultiBoolean(string element);

            /// <summary>
            /// 点动输出
            /// </summary>
            /// <param name="element"></param>
            void PressOutput(string element);

            /// <summary>
            /// 读取单个bool变量
            /// </summary>
            /// <param name="element">软元件地址</param>
            /// <returns></returns>
            Task<bool> ReadSingleBooleanAsync(string element);

            /// <summary>
            /// 写入单个bool变量
            /// </summary>
            /// <param name="element">软元件地址</param>
            /// <param name="value">写入值</param>
            /// <returns></returns>
            Task<bool> WriteSingleBooleanAsync(string element, bool value);

            /// <summary>
            /// 读取单个类型数据
            /// </summary>
            /// <typeparam name="T">类型</typeparam>
            /// <param name="element">软元件地址</param>
            /// <returns></returns>
            Task<T> ReadSingleValueAsync<T>(string element);

            /// <summary>
            /// 写入单个类型数据
            /// </summary>
            /// <typeparam name="T">类型</typeparam>
            /// <param name="element">软元件地址</param>
            /// <param name="Value">写入值</param>
            /// <returns></returns>
            Task<bool> WriteSingleValueAsync<T>(string element, T Value);

            /// <summary>
            /// 读取多个bool变量
            /// </summary>
            /// <param name="element">软元件起始地址</param>
            /// <param name="count">数量</param>
            /// <returns></returns>
            Task<List<bool>> ReadMultiBooleanAsync(string element, int count);

            /// <summary>
            /// 写入多个bool变量
            /// </summary>
            /// <param name="element">软元件起始地址</param>
            /// <param name="value">写入值</param>
            /// <returns></returns>
            Task<bool> WriteMultiBooleanAsync(string element, params bool[] value);

            /// <summary>
            /// 读取多个类型数据
            /// </summary>
            /// <typeparam name="T">数据类型</typeparam>
            /// <param name="element">软元件起始地址</param>
            /// <param name="count">数量(读取数量)</param>
            /// <returns></returns>
            Task<List<T>> ReadMultiValueAsync<T>(string element, int count);

            string ReadString(string element, int length);

            Task<string> ReadStringAsync(string element, int length);

            /// <summary>
            /// 写入多个类型数据
            /// </summary>
            /// <typeparam name="T">数据类型</typeparam>
            /// <param name="element">软元件起始地址</param>
            /// <param name="value">写入值</param>
            /// <returns></returns>
            Task<bool> WriteMultiValueAsync<T>(string element, params T[] value);

            /// <summary>
            /// 翻转输出
            /// </summary>
            /// <param name="element"></param>
            /// <returns></returns>
            Task<bool> ReverseOutputAsync(string element);

            /// <summary>
            /// 脉冲输出
            /// </summary>
            /// <param name="element"></param>
            /// <param name="pulsTime">单位毫秒</param>
            /// <returns></returns>
            Task<bool> PlusOutputAsync(string element, int pulsTime);

            /// <summary>
            /// 读取一个byte的线圈
            /// </summary>
            /// <param name="element"></param>
            /// <returns></returns>
            Task<byte> ReadByteFromMultiBooleanAsync(string element);

            /// <summary>
            /// 点动输出
            /// </summary>
            /// <param name="element"></param>
            Task PressOutputAsync(string element);
        }


    [ModuleControllerDisplayName("汇川H5U PLC")]
    public class H5uModbusTcp : IH5uTcp, IPart
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
        }
        #endregion

        #region Initialization
       
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
        
        public bool ReadSingleBoolean(string element)
            => ExecuteWithRetry(() => ReadSingleBooleanInternal(element));
       
        public async Task<bool> ReadSingleBooleanAsync(string element)
            => await ExecuteWithRetryAsync(() => Task.FromResult(ReadSingleBoolean(element))).ConfigureAwait(false);
       
        private bool ReadSingleBooleanInternal(string element)
        {
            ValidateAddress(element, 0);
            ushort addr = (ushort)ConvertBitElementToModbusAddress(element);
            return _master.ReadCoils(_deviceAddress, addr, 1)[0];
        }
       
        public List<bool> ReadMultiBoolean(string element, int count)
            => ExecuteWithRetry(() =>
            {
                ValidateAddress(element, 0);
                ushort addr = (ushort)ConvertBitElementToModbusAddress(element);
                return new List<bool>(_master.ReadCoils(_deviceAddress, addr, (ushort)count));
            });
        
        public async Task<List<bool>> ReadMultiBooleanAsync(string element, int count)
            => await ExecuteWithRetryAsync(() => Task.FromResult(ReadMultiBoolean(element, count))).ConfigureAwait(false);
       
        public byte ReadByteFromMultiBoolean(string element)
        {
            var bits = ReadMultiBoolean(element, 8);
            return bits.Select((b, i) => b ? (byte)(1 << i) : (byte)0)
                       .Aggregate((a, b) => (byte)(a | b));
        }
       
        public async Task<byte> ReadByteFromMultiBooleanAsync(string element)
            => await ExecuteWithRetryAsync(() => Task.FromResult(ReadByteFromMultiBoolean(element))).ConfigureAwait(false);
        
        public T ReadSingleValue<T>(string element)
            => ExecuteWithRetry(() => ReadSingleValueInternal<T>(element));
       
        public async Task<T> ReadSingleValueAsync<T>(string element)
            => await ExecuteWithRetryAsync(() => Task.FromResult(ReadSingleValue<T>(element))).ConfigureAwait(false);
       
        private T ReadSingleValueInternal<T>(string element)
        {
            ValidateAddress(element, 1);
            ushort addr = (ushort)ConvertWordElementToModbusAddress(element);
            ushort[] regs = _master.ReadHoldingRegisters(_deviceAddress, addr, (ushort)GetRegisterCount<T>());
            return ConvertRegistersToValue<T>(regs);
        }
        
        public List<T> ReadMultiValue<T>(string element, int count)
            => ExecuteWithRetry(() =>
            {
                ValidateAddress(element, 1);
                ushort addr = (ushort)ConvertWordElementToModbusAddress(element);
                ushort[] regs = _master.ReadHoldingRegisters(_deviceAddress, addr, (ushort)(count * GetRegisterCount<T>()));
                return ConvertRegistersToList<T>(regs, count);
            });
       
        public async Task<List<T>> ReadMultiValueAsync<T>(string element, int count)
            => await ExecuteWithRetryAsync(() => Task.FromResult(ReadMultiValue<T>(element, count))).ConfigureAwait(false);
       
        public string ReadString(string element, int length)
            => ExecuteWithRetry(() =>
            {
                ValidateAddress(element, 1);
                ushort addr = (ushort)ConvertWordElementToModbusAddress(element);
                ushort[] regs = _master.ReadHoldingRegisters(_deviceAddress, addr, (ushort)(length * 2));
                byte[] raw = regs.SelectMany(r => new[] { (byte)(r & 0xFF), (byte)(r >> 8) }).ToArray();
                return Encoding.ASCII.GetString(raw).TrimEnd('\0');
            });
       
        public async Task<string> ReadStringAsync(string element, int length)
            => await ExecuteWithRetryAsync(() => Task.FromResult(ReadString(element, length))).ConfigureAwait(false);
        #endregion

        #region Write Methods
        
        public bool WriteSingleBoolean(string element, bool value)
            => ExecuteWithRetry(() =>
            {
                ValidateAddress(element, 0);
                ushort addr = (ushort)ConvertBitElementToModbusAddress(element);
                _master.WriteSingleCoil(_deviceAddress, addr, value);
                return true;
            });
       
        public async Task<bool> WriteSingleBooleanAsync(string element, bool value)
            => await ExecuteWithRetryAsync(() => Task.FromResult(WriteSingleBoolean(element, value))).ConfigureAwait(false);
        
        public bool WriteMultiBoolean(string element, params bool[] value)
            => ExecuteWithRetry(() =>
            {
                ValidateAddress(element, 0);
                ushort addr = (ushort)ConvertBitElementToModbusAddress(element);
                _master.WriteMultipleCoils(_deviceAddress, addr, value);
                return true;
            });
        
        public async Task<bool> WriteMultiBooleanAsync(string element, params bool[] value)
            => await ExecuteWithRetryAsync(() => Task.FromResult(WriteMultiBoolean(element, value))).ConfigureAwait(false);
       
        public bool WriteSingleValue<T>(string element, T value)
            => ExecuteWithRetry(() =>
            {
                ValidateAddress(element, 1);
                ushort addr = (ushort)ConvertWordElementToModbusAddress(element);
                _master.WriteMultipleRegisters(_deviceAddress, addr, ConvertValueToRegisters(value));
                return true;
            });
       
        public async Task<bool> WriteSingleValueAsync<T>(string element, T value)
            => await ExecuteWithRetryAsync(() => Task.FromResult(WriteSingleValue(element, value))).ConfigureAwait(false);
       
        public bool WriteMultiValue<T>(string element, params T[] value)
            => ExecuteWithRetry(() =>
            {
                ValidateAddress(element, 1);
                ushort addr = (ushort)ConvertWordElementToModbusAddress(element);
                _master.WriteMultipleRegisters(_deviceAddress, addr, ConvertValuesToRegisters(value));
                return true;
            });
       
        public async Task<bool> WriteMultiValueAsync<T>(string element, params T[] value)
            => await ExecuteWithRetryAsync(() => Task.FromResult(WriteMultiValue(element, value))).ConfigureAwait(false);
        #endregion

        #region Advanced Controls
        
        public bool ReverseOutput(string element)
            => ExecuteWithRetry(() => WriteSingleBoolean(element, !ReadSingleBoolean(element)));
       
        public Task<bool> ReverseOutputAsync(string element)
            => ExecuteWithRetryAsync(() => Task.FromResult(ReverseOutput(element)));
        
        public bool PlusOutput(string element, int pulsTime)
            => ExecuteWithRetry(() =>
            {
                WriteSingleBoolean(element, true);
                Thread.Sleep(pulsTime);
                WriteSingleBoolean(element, false);
                return true;
            });
      
        public Task<bool> PlusOutputAsync(string element, int pulsTime)
            => ExecuteWithRetryAsync(() => Task.FromResult(PlusOutput(element, pulsTime)));
      
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
    
        private static int GetRegisterCount<T>() => typeof(T) == typeof(float) || typeof(T) == typeof(double) ? 2 : 1;
       
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
     
        private ushort[] ConvertValuesToRegisters<T>(T[] values)
            => values.SelectMany(v => ConvertValueToRegisters(v)).ToArray();
     
        private void ValidateAddress(string element, int type)
        {
            var fmt = element.Trim().ToUpper();
            bool isValid = type == 0
                ? Array.Exists(_bitElements, c => fmt.StartsWith(c.ToString()))
                : Array.Exists(_wordElements, c => fmt.StartsWith(c.ToString()));
            if (!isValid)
                throw new ArgumentException($"地址格式错误: {element}");
        }
     
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
    
        private static int ConvertWordElementToModbusAddress(string fmt)
            => fmt[0] switch
            {
                'D' => (int)WordSoftElement.D + int.Parse(fmt[1..]),
                'R' => (int)WordSoftElement.R + int.Parse(fmt[1..]),
                _ => throw new ArgumentException($"字软元件格式错误: {fmt}"),
            };
    
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

        public void Initialize()
        {
            var result = Open();
            this.IsInitialized = result.Item1;
            PartCreated?.Invoke(this, EventArgs.Empty);
        }

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

        public T Part<T>()
        {
            if (this is T t) return t;
            throw new Exception($"Part type mismatch: {this.GetType().FullName} != {typeof(T).FullName}");
        }
        #endregion
    }

    public enum BitSoftElement
    {
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

    public class H5uModuleOption:ICloneable
    {
        /// <summary>
        /// 模块功能码地址
        /// </summary>
        public string ModuleFuncCodeAddress { get; set; } = "D210";

        /// <summary>
        /// 模块功能状态码地址
        /// </summary>
        public string ModuleFuncStateCodeAddress { get; set; } = "D200";

        /// <summary>
        /// 模块状态地址
        /// </summary>
        public string ModuleStateAddress { get; set; } = "D300";

        /// <summary>
        /// 模块参数地址
        /// </summary>
        public string ModuleParameterAddress { get; set; } = "D100";
        /// <summary>
        /// 模块观察数据地址
        /// </summary>
        public string ModuleObserveDataAddress { get; set; } = "D7800";
        /// <summary>
        /// 模块回原控制地址
        /// </summary>
        public string ModuleHomeControlAddress { get; set; } = "M0";

        /// <summary>
        /// 模块回原状态地址
        /// </summary>
        public string ModuleHomeStateAddress { get; set; } = "D0";

        /// <summary>
        /// 模块复位控制地址
        /// </summary>
        public string ModuleResetControlAddress { get; set; } = "M101";

        /// <summary>
        /// 模块停止控制地址
        /// </summary>
        public string ModuleStopControlAddress { get; set; } = "M102";

        /// <summary>
        /// 模块急停状态地址
        /// </summary>
        public string ModuleEmergencyControlAddress { get; set; } = "M102";

        /// <summary>
        /// 模块启动控制地址
        /// </summary>
        public string ModuleStartControlAddress { get; set; } = "M103";

        /// <summary>
        /// 模块暂停控制地址
        /// </summary>
        public string ModulePauseControlAddress { get; set; } = "M105";

        /// <summary>
        /// 模块手动自动控制地址
        /// </summary>
        public string ModuleManualAutoControlAddress { get; set; } = "M106";

        /// <summary>
        /// 模块初始化完成地址
        /// </summary>
        public string ModuleInitCompleteAddress { get; set; } = "M107";

        /// <summary>
        /// 模块报警地址 
        /// </summary>
        public string ModuleAlrmAddress { get; set; } = "B0";

        /// <summary>
        /// 模块报警长度
        /// </summary>
        public int ModuleAlrmAddressLength { get; set; } = 400;

        /// <summary>
        /// 扫码地址
        /// </summary>
        public string ModuleScanAddress { get; set; } = "D7850";

        /// <summary>
        /// 扫码长度
        /// </summary>
        public int ModuleScanAddressLength { get; set; } = 10;

        /// <summary>
        /// 重定向异常码
        /// </summary>
        public List<int> RedirectErrorCodes { get; set; } = [];

        /// <summary>
        /// 清除功能码和清除功能状态码等待时间
        /// </summary>
        public int ClearWaitTime { get; set; } = 50;

        public object Clone()
        {
            var clone = new H5uModuleOption 
            {
                ModuleFuncCodeAddress = ModuleFuncCodeAddress,
                ModuleFuncStateCodeAddress = ModuleFuncStateCodeAddress,
                ModuleStateAddress = ModuleStateAddress,
                ModuleParameterAddress = ModuleParameterAddress,
                ModuleObserveDataAddress = ModuleObserveDataAddress,
                ModuleHomeControlAddress = ModuleHomeControlAddress,
                ModulePauseControlAddress = ModulePauseControlAddress,
                ModuleHomeStateAddress = ModuleHomeStateAddress,
                ModuleResetControlAddress = ModuleResetControlAddress,
                ModuleStopControlAddress = ModuleStopControlAddress,
                ModuleEmergencyControlAddress = ModuleEmergencyControlAddress,
                ModuleStartControlAddress = ModuleStartControlAddress,
                ModuleManualAutoControlAddress = ModuleManualAutoControlAddress,
                ModuleInitCompleteAddress = ModuleInitCompleteAddress,
                ModuleAlrmAddress = ModuleAlrmAddress,
                ModuleAlrmAddressLength = ModuleAlrmAddressLength,
                ModuleScanAddress = ModuleScanAddress,
                ModuleScanAddressLength = ModuleScanAddressLength,
                RedirectErrorCodes = [.. RedirectErrorCodes.Select(x => x)],
                ClearWaitTime = ClearWaitTime
            };
            return clone;
        }
    }
    public class H5uModuleInfoConfig : UniversalModuleConfig
    {
        public override object ModuleOption { get; set; } = new H5uModuleOption();

        public override object Clone()
        {
            H5uModuleInfoConfig clone = new()
            {
                ModuleId = ModuleId,
                ModuleName = ModuleName,
                ModuleKey = ModuleKey,
                ModuleControllerId = ModuleControllerId,
                ModuleSerialNumber = ModuleSerialNumber,
                ModuleVersion = ModuleVersion,
                ModuleOption = ((H5uModuleOption)ModuleOption).Clone(),
                ModuleDescription = ModuleDescription,
                ModuleIdentifier = ModuleIdentifier,
                ModuleSpec = ModuleSpec,
                AlarmItems = [.. AlarmItems.Select(x => (IModuleAlarmItem)x.Clone())]
            };
            return clone;
        }
    }

    public class ModuleAlarmItem : ICloneable
    {
        public string AlarmAddress { get; set; } = string.Empty;

        public string AlarmDescription { get; set; } = string.Empty;

        public object Clone()
        {
            ModuleAlarmItem clone = new()
            {
                AlarmAddress = AlarmAddress,
                AlarmDescription = AlarmDescription
            };
            return clone;
        }
    }

    public class H5uModuleFunctionProvider : IFunctionProvider
    {
        private readonly H5uModbusTcp _modbusTcp;
        public H5uModuleFunctionProvider(H5uModbusTcp modbusTcp)
        {
            _modbusTcp = modbusTcp;
        }
        public List<IFunctionInfo> FunctionInfos { get; } = [];

        public Type ParametersOptionType => typeof(ModuleFuncCodeParameter);

        public Task<FunctionResult> ExecuteAsync(long functionId)
        {
            if (FunctionInfos.Any(x => x.FunctionId == functionId))
            {

            }
            return Task.FromResult(new FunctionResult());
        }

        public bool ValidateParameters(object parameters)
        {
            return parameters is ModuleFuncCodeParameter;
        }
    }

    public class ModuleFuncCodeParameter : IParameter,ICloneable
    {
        /// <summary>
        /// 功能码
        /// </summary>
        public int FuncCode { get; set; }

        /// <summary>
        /// 是否监控参数反馈
        /// </summary>
        public bool IsMonitorFuncCodeParameterFeedback { get; set; } = false;

        /// <summary>
        /// 功能码参数
        /// </summary>
        public List<FuncCodeParameterInfo> FuncCodeParamterInfos { get; set; } = [];

        /// <summary>
        /// 模块监控配置
        /// </summary>
        public List<ModuleMonitorInfoItem> MonitorInfoItems { get; set; } = [];

        /// <summary>
        /// 通道Ebr配置
        /// </summary>
        public Dictionary<int, List<ModuleEbrInfoItem>> ChannelEbrInfos { get; set; } = [];
        /// <summary>
        /// 模块精度配置
        /// </summary>
        public List<ModulePrecisionInfoItem> PrecisionInfoItems { get; set; } = [];
        /// <summary>
        /// 功能码描述
        /// </summary>
        public string FuncCodeDescription { get; set; } = string.Empty;

        /// <summary>
        /// 监控间隔/ms
        /// </summary>
        public int MonitorInterval { get; set; } = 1000;
        /// <summary>
        /// EBR开始读取间隔/ms
        /// </summary>
        public int EbrReadStartInterval { get; set; }

        public Guid ParameterId { get; set; }
        /// <summary>
        /// 是否需要参数
        /// </summary>
        public bool RequiresParameter { get; set; } = true;

        /// <summary>
        /// 功能执行超时时间
        /// </summary>
        public int FuncActionTimeout { get; set; } = 0;

        public object Clone()
        {
            var clone = new ModuleFuncCodeParameter
            {
                ParameterId = Guid.NewGuid(),
                FuncCode = FuncCode,
                EbrReadStartInterval = EbrReadStartInterval,
                MonitorInterval = MonitorInterval,
                RequiresParameter = RequiresParameter,
                FuncActionTimeout = FuncActionTimeout,
                FuncCodeDescription = FuncCodeDescription,
                IsMonitorFuncCodeParameterFeedback = IsMonitorFuncCodeParameterFeedback,
                ChannelEbrInfos = ChannelEbrInfos.ToDictionary(x => x.Key, x => x.Value.Select(y => (ModuleEbrInfoItem)y.Clone()).ToList()),
                FuncCodeParamterInfos = FuncCodeParamterInfos.Select(x => (FuncCodeParameterInfo)x.Clone()).ToList(),
                MonitorInfoItems = MonitorInfoItems.Select(x => (ModuleMonitorInfoItem)x.Clone()).ToList(),
                PrecisionInfoItems = PrecisionInfoItems.Select(x => (ModulePrecisionInfoItem)x.Clone()).ToList()
            };
            return clone;
        }

        public void InitlizeParameter()
        {

        }
    }

    public class FuncCodeParameterInfo : ICloneable
    {
        public FuncCodeParameterInfo()
        {
            ParameterValueFactory["0"] = 0f;
        }
        /// <summary>
        /// 参数ID
        /// </summary>
        public long ParameterId { get; set; }
        /// <summary>
        /// 参数名称
        /// </summary>
        public string ParameterName { get; set; } = string.Empty;
        /// <summary>
        /// 参数地址
        /// </summary>
        public string ParameterAddress { get; set; } = string.Empty;

        /// <summary>
        /// 参数反馈值地址
        /// </summary>
        public string ParameterFeedbackAddress { get; set; } = string.Empty;

        /// <summary>
        /// 参数反馈的预设值
        /// </summary>
        public float ParameterFeedbackDefaultValue { get; set; } = 0f;

        /// <summary>
        /// 参数单位
        /// </summary>
        public string ParameterUnit { get; set; } = string.Empty;
        /// <summary>
        /// 参数描述
        /// </summary>
        public string ParameterDescription { get; set; } = string.Empty;
        /// <summary>
        /// 参数最小值
        /// </summary>
        public float ParameterMinValue { get; set; } = 0f;
        /// <summary>
        /// 参数最大值
        /// </summary>
        public float ParameterMaxValue { get; set; } = 0f;
        /// <summary>
        /// 参数值
        /// </summary>
        public Dictionary<string, float> ParameterValueFactory { get; set; } = [];

        public bool Openable { get; set; } = true;

        public object Clone()
        {
            var clone = new FuncCodeParameterInfo
            {
                ParameterName = ParameterName,
                ParameterAddress = ParameterAddress,
                ParameterUnit = ParameterUnit,
                ParameterDescription = ParameterDescription,
                ParameterMinValue = ParameterMinValue,
                ParameterMaxValue = ParameterMaxValue,
                Openable = Openable
            };
            foreach (var item in ParameterValueFactory)
            {
                clone.ParameterValueFactory[item.Key] = item.Value;
            }
            return clone;
        }
    }

    public class ModuleMonitorInfoItem : ICloneable
    {
        public string MonitorName { get; set; } = string.Empty;
        public string MonitorAddress { get; set; } = string.Empty;
        public string MonitorUnit { get; set; } = string.Empty;
        public Framework.DataType MonitorType { get; set; }
        public string MonitorDescription { get; set; } = string.Empty;

        public object Clone()
        {
            var clone = new ModuleMonitorInfoItem
            {
                MonitorName = MonitorName,
                MonitorAddress = MonitorAddress,
                MonitorDescription = MonitorDescription,
                MonitorUnit = MonitorUnit,
                MonitorType = MonitorType
            };
            return clone;
        }
    }

    public class ModuleEbrInfoItem : ICloneable
    {
        /// <summary>
        /// Ebr名称
        /// </summary>
        public string EbrName { get; set; } = string.Empty;
        /// <summary>
        /// Ebr地址
        /// </summary>
        public string EbrAddress { get; set; } = string.Empty;
        /// <summary>
        /// Ebr描述
        /// </summary>
        public string EbrDescription { get; set; } = string.Empty;

        /// <summary>
        /// Ebr类型
        /// </summary>
        public Framework.DataType EbrType { get; set; } = Framework.DataType.Float;
        /// <summary>
        /// Ebr单位
        /// </summary>
        public string EbrUnit { get; set; } = string.Empty;

        /// <summary>
        /// 模块通道
        /// </summary>
        public int ModuleChannel { get; set; } = 1;

        /// <summary>
        /// Ebr字符串读取长度
        /// </summary>
        public int CharacterLength { get; set; } = 0;

        public object Clone()
        {
            var clone = new ModuleEbrInfoItem
            {
                EbrName = EbrName,
                EbrAddress = EbrAddress,
                EbrDescription = EbrDescription,
                EbrType = EbrType,
                EbrUnit = EbrUnit,
                CharacterLength = CharacterLength,
                ModuleChannel = ModuleChannel
            };
            return clone;
        }
    }

    public class ModulePrecisionInfoItem : ICloneable
    {
        public string PrecisionName { get; set; } = string.Empty;
        public string PrecisionAddress { get; set; } = string.Empty;
        public string PrecisionDescription { get; set; } = string.Empty;
        public float PrecisionStandardValue { get; set; } = 0f;

        public object Clone()
        {
            var clone = new ModulePrecisionInfoItem
            {
                PrecisionName = PrecisionName,
                PrecisionAddress = PrecisionAddress,
                PrecisionDescription = PrecisionDescription,
                PrecisionStandardValue = PrecisionStandardValue
            };
            return clone;
        }
    }



}
