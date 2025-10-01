using QStandaedPlatform.Engine.Common.Common;
using QStandaedPlatform.Engine.Components;
using QStandaedPlatform.Engine.Components.Modbus;
using QStandaedPlatform.Engine.Laboratory.Documents;
using SqlSugar.SplitTableExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Laboratory
{
    public class PlatformMonitorService(PlatformMonitorOptions options)
    {
        private readonly PlatformMonitorOptions _options = options;
        private readonly Dictionary<Guid,IH5uTcp> _clients = [];
        private readonly Dictionary<Guid,ModuleInfoParameter> _moduleInfos = [];
        private readonly CancellationTokenSource _tokenSource = new();
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public void Initialize()
        {
            foreach (var item in _options.Items)
            {
                var h5UModbusTcps = WorkFlowEngine.Instance.GetPartMappers().Where(p => p.Part != null && p.As<H5uModbusTcp>() != null).ToArray();
                var h5uTcp = h5UModbusTcps.FirstOrDefault(p => p.PartId == item.ClientId);
                if (h5uTcp != null)
                {
                    var h5uModbusTcp = h5uTcp.As<H5uModbusTcp>();
                    if (h5uModbusTcp != null)
                    {
                        _clients.TryAdd(item.ClientId, h5uModbusTcp);
                    }
                }
                var moduleInfo = ParameterTableManager.ModuleInfoTable.ModuleInfoParameters.FirstOrDefault(p => p.ModuleInfoId == item.ModuleInfoId);
                if (moduleInfo != null)
                {
                    _moduleInfos.TryAdd(item.ModuleInfoId, (ModuleInfoParameter)moduleInfo);
                }
            }
        }
        /// <summary>
        /// 监控数据流
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async IAsyncEnumerable<List<MonitorResult>> MonitorDataStreamAsync(List<string> monitorKeys, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(_tokenSource.Token, cancellationToken);
            while (!tokenSource.IsCancellationRequested)
            {
                yield return await CaptureMonitorFrameAsync(monitorKeys, cancellationToken);
                await Task.Delay(MonitorInterval == 0 ? _options.MonitorInterval : MonitorInterval, tokenSource.Token);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="monitorKeys"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<List<MonitorResult>> CaptureMonitorFrameAsync(List<string> monitorKeys, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return [];
            await _semaphore.WaitAsync(CancellationToken.None);
            var list = new List<MonitorResult>();
            try
            {
                foreach (var item in _options.Items)
                {
                    if (monitorKeys.Count > 0)
                    {
                        if (!monitorKeys.Contains(item.MonitorKey))
                            continue;
                    }
                    cancellationToken.ThrowIfCancellationRequested();
                    if (_clients.TryGetValue(item.ClientId, out var client))
                    {
                        string moduleKey = string.Empty;
                        string moduleName = string.Empty;
                        if (_moduleInfos.TryGetValue(item.ModuleInfoId, out var moduleInfo))
                        {
                            moduleKey = moduleInfo.ModuleKey;
                            moduleName = moduleInfo.ModuleName;
                        }
                        switch (item.MonitorType)
                        {
                            case ReadType.INT:
                                short intValue = 0;
                                Exception? intValueException = null;
                                try
                                {
                                    intValue = await client.ReadSingleValueAsync<short>(item.MonitorAddress);
                                }
                                catch (Exception ex)
                                {
                                    intValueException = ex;
                                }
                                list.Add(new MonitorResult
                                {
                                    MonitorAddress = item.MonitorAddress,
                                    MonitorKey = item.MonitorKey,
                                    MonitorType = item.MonitorType.ToString(),
                                    MonitorUnit = item.MonitorUnit,
                                    Value = intValue,
                                    MonitorException = intValueException,
                                    ModuleKey = moduleKey,
                                    ModuleName = moduleName,
                                    MonitorKeyDescription = item.MonitorKeyDescription,
                                });
                                break;
                            case ReadType.REAL:
                                float realValue = 0;
                                Exception? realValueException = null;
                                try
                                {
                                    realValue = await client.ReadSingleValueAsync<float>(item.MonitorAddress);
                                }
                                catch (Exception ex)
                                {
                                    realValueException = ex;
                                }
                                list.Add(new MonitorResult
                                {
                                    MonitorAddress = item.MonitorAddress,
                                    MonitorKey = item.MonitorKey,
                                    MonitorType = item.MonitorType.ToString(),
                                    MonitorUnit = item.MonitorUnit,
                                    Value = realValue,
                                    MonitorException = realValueException,
                                    ModuleKey = moduleKey,
                                    ModuleName = moduleName,
                                    MonitorKeyDescription = item.MonitorKeyDescription,
                                });
                                break;
                            case ReadType.DINT:
                                int dintValue = 0;
                                Exception? dintValueException = null;
                                try
                                {
                                    dintValue = await client.ReadSingleValueAsync<int>(item.MonitorAddress);
                                }
                                catch (Exception ex)
                                {
                                    dintValueException = ex;
                                }
                                list.Add(new MonitorResult
                                {
                                    MonitorAddress = item.MonitorAddress,
                                    MonitorKey = item.MonitorKey,
                                    MonitorType = item.MonitorType.ToString(),
                                    MonitorUnit = item.MonitorUnit,
                                    Value = dintValue,
                                    MonitorException = dintValueException,
                                    ModuleKey = moduleKey,
                                    ModuleName = moduleName,
                                    MonitorKeyDescription = item.MonitorKeyDescription,
                                });
                                break;
                            case ReadType.STRING:
                                string stringValue = string.Empty;
                                Exception? stringValueException = null;
                                try
                                {
                                    stringValue = await client.ReadStringAsync(item.MonitorAddress, item.CharacterSize);
                                }
                                catch (Exception ex)
                                {
                                    stringValueException = ex;
                                }
                                list.Add(new MonitorResult
                                {
                                    MonitorAddress = item.MonitorAddress,
                                    MonitorKey = item.MonitorKey,
                                    MonitorType = item.MonitorType.ToString(),
                                    MonitorUnit = item.MonitorUnit,
                                    Value = stringValue,
                                    MonitorException = stringValueException,
                                    ModuleKey = moduleKey,
                                    ModuleName = moduleName,
                                    MonitorKeyDescription = item.MonitorKeyDescription,
                                });
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            catch (Exception ex) when (ex is OperationCanceledException canceledException)
            {
                return list;
            }
            finally
            {
                _semaphore.Release();
            }
            return list;
        }

        public void StopMonitor()
        {
            _tokenSource.Cancel();
        }

        public bool Monitoring()
        {
            return !_tokenSource.IsCancellationRequested;
        }

        public int MonitorInterval { get; set; }
    }

    public class PlatformMonitorOptions
    {
        public int MonitorInterval { get; set; } = 1000;
        public List<PlatformMonitorItem> Items { get; set; } = [];
    }

    public class PlatformMonitorItem:ICloneable
    {
        public Guid  ClientId { get; set; }
        public Guid  ModuleInfoId { get; set; }
        public string ModuleName { get; set; }= string.Empty;
        public string MonitorKey { get; set; } = string.Empty;
        public string MonitorKeyDescription { get; set; } = string.Empty;
        public ReadType MonitorType { get; set; }
        public string MonitorAddress { get; set; } = string.Empty;
        public string MonitorUnit { get; set; } = string.Empty;
        public int CharacterSize { get; set; }

        public object Clone()
        {
            var clone = new PlatformMonitorItem
            {
                ClientId = ClientId,
                ModuleInfoId = ModuleInfoId,
                ModuleName = ModuleName,
                MonitorKey = MonitorKey,
                MonitorKeyDescription = MonitorKeyDescription,
                MonitorType = MonitorType,
                MonitorAddress = MonitorAddress,
                MonitorUnit = MonitorUnit,
                CharacterSize = CharacterSize
            };
            return clone;
        }
    }

    public enum ReadType
    {
       INT,
       REAL,
       DINT,
       STRING,
    }

    public class MonitorResult
    {
        public string MonitorKeyDescription { get; set; } = string.Empty;
        public string MonitorKey { get; set; } = string.Empty;
        public string MonitorType { get; set; } = string.Empty;
        public string MonitorAddress { get; set; } = string.Empty;
        public string MonitorUnit { get; set; } = string.Empty;
        public object? Value { get; set; }
        public string ModuleName { get; set; } = string.Empty;
        public string ModuleKey { get; set; } = string.Empty;
        public Exception? MonitorException { get; set; }
    }

}
