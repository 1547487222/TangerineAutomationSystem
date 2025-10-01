
using System.Text;
using System.ComponentModel;
using Microsoft.Extensions.Logging;
using QStandaedPlatform.Engine.Common.Common;
using QStandaedPlatform.Engine.Common;

namespace QStandaedPlatform.Engine.Components.Modbus
{


    [DisplayName("汇川H5U PLC Mock")]
    public class H5uModbusTcpMock : IH5uTcp, IPart
    {
        #region Private Members
        private readonly string _ip;
        private readonly int _port;
        private readonly byte _deviceAddress;
        private readonly int _delayTime;
        private readonly ILogger _logger;
        private System.Threading.Timer _timer;
        private readonly char[] _bitElements = ['M', 'B', 'S', 'X', 'Y'];
        private readonly char[] _wordElements = ['D', 'R'];
        private readonly Dictionary<string, bool> _bitStorage;
        private readonly Dictionary<string, ushort[]> _wordStorage;
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
        public H5uModbusTcpMock(string ip, int port, byte deviceAddress = 1, int delayTime = 500, ILogger logger = null)
        {
            _ip = ip;
            _port = port;
            _deviceAddress = deviceAddress;
            _delayTime = delayTime;
            _logger = logger ?? LoggerProviderManager.GetLoggerFactory().CreateLogger($"<{ip}:{port}>.{deviceAddress}.H5uModbusTcpMock");
            _bitStorage = new Dictionary<string, bool>();
            _wordStorage = new Dictionary<string, ushort[]>();
            _logger?.LogInformation("Initialized H5uModbusTcpMock");
        }
        #endregion

        #region Initialization
        public Task<(bool, string)> OpenAsync()
        {
            _logger?.LogInformation("Mock: Simulating async connection");
            IsInitialized = true;
            PartCreated?.Invoke(this, EventArgs.Empty);
            return Task.FromResult((true, "Mock connection established"));
        }

        public (bool, string) Open()
        {
            _logger?.LogInformation("Mock: Simulating connection");
            IsInitialized = true;
            PartCreated?.Invoke(this, EventArgs.Empty);
            return (true, "Mock connection established");
        }

        public void Initialize()
        {
            var result = Open();
            IsInitialized = result.Item1;
            _logger?.LogInformation($"Mock initialization {(result.Item1 ? "succeeded" : "failed")}: {result.Item2}");
        }

        public void Shutdown()
        {
            _timer?.Dispose();
            _bitStorage.Clear();
            _wordStorage.Clear();
            IsInitialized = false;
            _logger?.LogInformation("Mock shutdown completed");
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
            _logger?.LogDebug($"Mock: Reading boolean {element}");
            return _bitStorage.TryGetValue(element, out var value) ? value : false;
        }

        public List<bool> ReadMultiBoolean(string element, int count)
            => ExecuteWithRetry(() =>
            {
                ValidateAddress(element, 0);
                _logger?.LogDebug($"Mock: Reading {count} booleans from {element}");
                var result = new List<bool>();
                for (int i = 0; i < count; i++)
                {
                    string key = $"{element}{i}";
                    result.Add(_bitStorage.TryGetValue(key, out var value) ? value : false);
                }
                return result;
            });

        public async Task<List<bool>> ReadMultiBooleanAsync(string element, int count)
            => await ExecuteWithRetryAsync(() => Task.FromResult(ReadMultiBoolean(element, count))).ConfigureAwait(false);

        public byte ReadByteFromMultiBoolean(string element)
        {
            var bits = ReadMultiBoolean(element, 8);
            var result = bits.Select((b, i) => b ? (byte)(1 << i) : (byte)0)
                            .Aggregate((a, b) => (byte)(a | b));
            _logger?.LogDebug($"Mock: Read byte from {element}: {result}");
            return result;
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
            _logger?.LogDebug($"Mock: Reading value {element}");
            if (_wordStorage.TryGetValue(element, out var regs))
            {
                return ConvertRegistersToValue<T>(regs);
            }
            return default;
        }

        public List<T> ReadMultiValue<T>(string element, int count)
            => ExecuteWithRetry(() =>
            {
                ValidateAddress(element, 1);
                _logger?.LogDebug($"Mock: Reading {count} values from {element}");
                var result = new List<T>();
                for (int i = 0; i < count; i++)
                {
                    string key = $"{element}{i}";
                    if (_wordStorage.TryGetValue(key, out var regs))
                    {
                        result.Add(ConvertRegistersToValue<T>(regs));
                    }
                    else
                    {
                        result.Add(default);
                    }
                }
                return result;
            });

        public async Task<List<T>> ReadMultiValueAsync<T>(string element, int count)
            => await ExecuteWithRetryAsync(() => Task.FromResult(ReadMultiValue<T>(element, count))).ConfigureAwait(false);

        public string ReadString(string element, int length)
            => ExecuteWithRetry(() =>
            {
                ValidateAddress(element, 1);
                _logger?.LogDebug($"Mock: Reading string {element} of length {length}");
                if (_wordStorage.TryGetValue(element, out var regs))
                {
                    byte[] raw = regs.SelectMany(r => new[] { (byte)(r & 0xFF), (byte)(r >> 8) }).ToArray();
                    return Encoding.ASCII.GetString(raw).TrimEnd('\0');
                }
                return string.Empty;
            });

        public async Task<string> ReadStringAsync(string element, int length)
            => await ExecuteWithRetryAsync(() => Task.FromResult(ReadString(element, length))).ConfigureAwait(false);
        #endregion

        #region Write Methods
        public bool WriteSingleBoolean(string element, bool value)
            => ExecuteWithRetry(() =>
            {
                ValidateAddress(element, 0);
                _logger?.LogDebug($"Mock: Writing boolean {element} = {value}");
                _bitStorage[element] = value;
                return true;
            });

        public async Task<bool> WriteSingleBooleanAsync(string element, bool value)
            => await ExecuteWithRetryAsync(() => Task.FromResult(WriteSingleBoolean(element, value))).ConfigureAwait(false);

        public bool WriteMultiBoolean(string element, params bool[] value)
            => ExecuteWithRetry(() =>
            {
                ValidateAddress(element, 0);
                _logger?.LogDebug($"Mock: Writing {value.Length} booleans to {element}");
                for (int i = 0; i < value.Length; i++)
                {
                    _bitStorage[$"{element}{i}"] = value[i];
                }
                return true;
            });

        public async Task<bool> WriteMultiBooleanAsync(string element, params bool[] value)
            => await ExecuteWithRetryAsync(() => Task.FromResult(WriteMultiBoolean(element, value))).ConfigureAwait(false);

        public bool WriteSingleValue<T>(string element, T value)
            => ExecuteWithRetry(() =>
            {
                ValidateAddress(element, 1);
                _logger?.LogDebug($"Mock: Writing value {element} = {value}");
                _wordStorage[element] = ConvertValueToRegisters(value);
                return true;
            });

        public async Task<bool> WriteSingleValueAsync<T>(string element, T value)
            => await ExecuteWithRetryAsync(() => Task.FromResult(WriteSingleValue(element, value))).ConfigureAwait(false);

        public bool WriteMultiValue<T>(string element, params T[] value)
            => ExecuteWithRetry(() =>
            {
                ValidateAddress(element, 1);
                _logger?.LogDebug($"Mock: Writing {value.Length} values to {element}");
                for (int i = 0; i < value.Length; i++)
                {
                    _wordStorage[$"{element}{i}"] = ConvertValueToRegisters(value[i]);
                }
                return true;
            });

        public async Task<bool> WriteMultiValueAsync<T>(string element, params T[] value)
            => await ExecuteWithRetryAsync(() => Task.FromResult(WriteMultiValue(element, value))).ConfigureAwait(false);
        #endregion

        #region Advanced Controls
        public bool ReverseOutput(string element)
            => ExecuteWithRetry(() =>
            {
                bool current = ReadSingleBoolean(element);
                bool result = WriteSingleBoolean(element, !current);
                _logger?.LogDebug($"Mock: Reversed output {element} from {current} to {!current}");
                return result;
            });

        public async Task<bool> ReverseOutputAsync(string element)
            => await ExecuteWithRetryAsync(() => Task.FromResult(ReverseOutput(element)));

        public bool PlusOutput(string element, int pulsTime)
            => ExecuteWithRetry(() =>
            {
                WriteSingleBoolean(element, true);
                _logger?.LogDebug($"Mock: Pulse start {element} = true");
                Thread.Sleep(pulsTime);
                WriteSingleBoolean(element, false);
                _logger?.LogDebug($"Mock: Pulse end {element} = false");
                return true;
            });

        public async Task<bool> PlusOutputAsync(string element, int pulsTime)
            => await ExecuteWithRetryAsync(() => Task.FromResult(PlusOutput(element, pulsTime)));

        public void PressOutput(string element)
        {
            _timer?.Dispose();
            WriteSingleBoolean(element, true);
            _logger?.LogDebug($"Mock: Press start {element} = true");
            _timer = new Timer(_ =>
            {
                try
                {
                    WriteSingleBoolean(element, false);
                    _logger?.LogDebug($"Mock: Press end {element} = false");
                }
                catch (Exception ex)
                {
                    _logger?.LogError($"Mock: Timer callback error: {ex}");
                }
            }, null, _delayTime, Timeout.Infinite);
        }

        public async Task PressOutputAsync(string element)
        {
            _timer?.Dispose();
            await WriteSingleBooleanAsync(element, true);
            _logger?.LogDebug($"Mock: Press async start {element} = true");
            _timer = new Timer(_ => _ = WriteSingleBooleanAsync(element, false)
                                        .ContinueWith(t =>
                                        {
                                            if (t.Exception != null)
                                            {
                                                _logger?.LogError(t.Exception, "Mock: Timer async error");
                                            }
                                            else
                                            {
                                                _logger?.LogDebug($"Mock: Press async end {element} = false");
                                            }
                                        }),
                             null, _delayTime, Timeout.Infinite);
        }
        #endregion

        #region Utility Methods
        private static int GetRegisterCount<T>() => typeof(T) == typeof(float) || typeof(T) == typeof(double) ? 2 : 1;

        private static T ConvertRegistersToValue<T>(ushort[] regs)
        {
            byte[] raw = regs.SelectMany(r => new[] { (byte)(r & 0xFF), (byte)(r >> 8) }).ToArray();
            if (typeof(T) == typeof(float)) return (T)(object)BitConverter.ToSingle(raw, 0);
            if (typeof(T) == typeof(double)) return (T)(object)BitConverter.ToDouble(raw, 0);
            if (raw.Length == 2) return (T)Convert.ChangeType(BitConverter.ToUInt16(raw, 0), typeof(T));
            if (raw.Length == 4) return (T)Convert.ChangeType(BitConverter.ToUInt32(raw, 0), typeof(T));
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
            {
                _logger?.LogError($"Mock: Invalid address format: {element}");
                throw new ArgumentException($"地址格式错误: {element}");
            }
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
            int wordCount = len / 2;
            var result = new byte[len];

            switch (endian)
            {
                case EndianType.ABCD:
                    Buffer.BlockCopy(raw, 0, result, 0, len);
                    break;
                case EndianType.BADC:
                    for (int w = 0; w < wordCount; w++)
                    {
                        int i = w * 2;
                        result[i + 0] = raw[i + 1];
                        result[i + 1] = raw[i + 0];
                    }
                    break;
                case EndianType.CDAB:
                    for (int w = 0; w < wordCount; w++)
                    {
                        int srcIndex = (wordCount - 1 - w) * 2;
                        int dstIndex = w * 2;
                        result[dstIndex + 0] = raw[srcIndex + 0];
                        result[dstIndex + 1] = raw[srcIndex + 1];
                    }
                    break;
                case EndianType.DCBA:
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

        private readonly object _sync = new object();
        private T ExecuteWithRetry<T>(Func<T> func)
        {
            lock (_sync)
            {
                try
                {
                    return func();
                }
                catch (Exception ex)
                {
                    _logger?.LogError($"Mock: Error: {ex}");
                    throw;
                }
            }
        }

        private readonly SemaphoreSlim _asyncSync = new(1, 1);
        private async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> func)
        {
            await _asyncSync.WaitAsync();
            try
            {
                return await func();
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Mock: Async error: {ex}");
                throw;
            }
            finally
            {
                _asyncSync.Release();
            }
        }
        #endregion
    }
}
