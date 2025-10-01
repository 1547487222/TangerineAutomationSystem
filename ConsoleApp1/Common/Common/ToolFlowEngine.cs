using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QStandaedPlatform.Engine.Components;
using QStandaedPlatform.Engine.Laboratory;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common
{
    public class WorkFlowEngine : IWorkFlowEngine
    {
        public class WorkFlowEngineOptions
        {
            public WorkFlowEngineOptions()
            {
                FlowFileDescriptions = [];
                PartBackups = [];
            }
            public List<FlowFileDescription> FlowFileDescriptions { get; set; }

            public List<PartBackup> PartBackups { get; set; }
        }
        private readonly static Lazy<WorkFlowEngine> _workFlowEngine = new(() => new WorkFlowEngine(), true);
        private readonly List<FlowFileDescription> _flowFileDescriptions = [];
        private readonly ConcurrentDictionary<Guid, Flow> _flowDic = new();
        private readonly Dictionary<Guid, PartBackup> _partDict = [];
        private readonly ConcurrentDictionary<Guid, PartMapper> _partMapperDict = new();
        private WorkFlowEngineOptions _workFlowEngineOptions = new();
        private readonly string _configFilePath = "WorkFlowEngine.engine";
        private readonly ILogger? _logger;

        public event Action? OnPartCollectionChanged;

        public IReadOnlyList<FlowFileDescription> GetFlowFileDescriptions()
        {
            lock (_workFlowEngineOptions)
            {
                return _workFlowEngineOptions.FlowFileDescriptions.AsReadOnly();
            }
        }

        private WorkFlowEngine()
        {
            _logger = LoggerProviderManager.GetLoggerFactory().CreateLogger("WorkFlowEngine");
        }
        public static WorkFlowEngine Instance => _workFlowEngine.Value;

        public void Initialize()
        {
            if (File.Exists(_configFilePath))
            {
                var data = JsonConvert.DeserializeObject<WorkFlowEngineOptions>(File.ReadAllText(_configFilePath));
                if (data != null)
                {
                    lock (_workFlowEngineOptions)
                    {
                        _workFlowEngineOptions = data;
                        _flowFileDescriptions.AddRange(_workFlowEngineOptions.FlowFileDescriptions);
                        if (_workFlowEngineOptions.PartBackups.Count != 0)
                        {
                            foreach (var item in _workFlowEngineOptions.PartBackups)
                            {
                                _partDict[item.PartId] = item;
                                if (item.PartOption != null)
                                {
                                    try
                                    {
                                        item.PartOption = JObject.FromObject(item.PartOption).ToObject(item.PartOptionType);
                                    }
                                    catch { }
                                }
                                var partMapper = new PartMapper(item);
                                Task.Run(partMapper.Initialize);
                                _logger?.LogInformation($"PartMapper {partMapper.PartId} Initialized");
                                _partMapperDict[partMapper.PartId] = partMapper;
                            }
                        }
                    }
                }
            }
            else
            {
                lock (_workFlowEngineOptions)
                {
                    File.Create(_configFilePath).Close();
                    _workFlowEngineOptions = new WorkFlowEngineOptions();
                    File.WriteAllText(_configFilePath, JsonConvert.SerializeObject(_workFlowEngineOptions));
                }
            }
            ToolDescriptions.InitToolRunTimeDesc();
            PartDescriptions.InitPartRunTimeDesc();
            ParameterTableManager.InitParameterTable();
        }

        public bool TryCreateFlowByFlowOptions(FlowInfoOptions flowInfoOptions, out Flow? flow)
        {
            flow = null;
            if (flowInfoOptions == null)
                return false;
            var tempflow = new Flow();
            if (tempflow.Deserialize(flowInfoOptions))
            {
                flow = tempflow;
                _flowDic[flow.FlowId] = flow;
                return true;
            }
            return false;
        }
        public Flow CreateNewFlow(string flowName, Guid flowId, string desc = "")
        {
            var flow = new Flow(flowName, flowId, desc);
            _flowDic[flow.FlowId] = flow;
            return flow;
        }
        public IReadOnlyList<(string toolName, string desc)> GetToolDescriptions()
        {
            return ToolDescriptions.GetToolDescriptions();
        }
        public IReadOnlyList<(string partName, string desc)> GetPartDescriptions()
        {
            return PartDescriptions.GetPartDescriptions();
        }
        public void SaveFlow(Flow flow, string path)
        {
            lock (_workFlowEngineOptions)
            {
                var flowInfoOptions = flow.Serialize();
                var settings = new JsonSerializerSettings
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                    ReferenceLoopHandling = ReferenceLoopHandling.Serialize
                };
                if (_workFlowEngineOptions.FlowFileDescriptions.Any(x => x.FlowId == flow.FlowId))
                {
                    var flowFileDescription = _workFlowEngineOptions.FlowFileDescriptions.FirstOrDefault(x => x.FlowId == flow.FlowId);
                    if (flowFileDescription != null)
                    {
                        File.WriteAllText(path, JsonConvert.SerializeObject(flowInfoOptions, settings));
                        return;
                    }
                }
                if (!Directory.Exists(Path.GetDirectoryName(path)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                }
                if (!File.Exists(path))
                {
                    File.Create(path).Close();
                }

                File.WriteAllText(path, JsonConvert.SerializeObject(flowInfoOptions, settings));
                _workFlowEngineOptions.FlowFileDescriptions.Add(new FlowFileDescription
                {
                    FlowId = flow.FlowId,
                    FlowDescription = flow.Description,
                    FilePath = path
                });
                File.WriteAllText(_configFilePath, JsonConvert.SerializeObject(_workFlowEngineOptions));
                _workFlowEngineOptions = JsonConvert.DeserializeObject<WorkFlowEngineOptions>(File.ReadAllText(_configFilePath));
            }
        }

        public bool ReadFlow(FlowFileDescription flowFileDescription, out Flow? flow)
        {
            flow = null;
            if (string.IsNullOrEmpty(flowFileDescription.FilePath))
                return false;
            if (!File.Exists(flowFileDescription.FilePath))
                return false;
            var settings = new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize
            };
           FlowInfoOptions? flowInfoOptions=null;
            try
            {
                flowInfoOptions = JsonConvert.DeserializeObject<FlowInfoOptions>(File.ReadAllText(flowFileDescription.FilePath), settings);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "读取流程文件失败");
            }
            if (flowInfoOptions != null && flowFileDescription.FlowId == flowInfoOptions.FlowId)
            {
                var tempflow = new Flow();
                if (tempflow.Deserialize(flowInfoOptions))
                {
                    flow = tempflow;
                    _flowDic[flow.FlowId] = flow;
                    return true;
                }
            }
            return false;
        }

        public void DeleteFlow(Flow flow)
        {
            lock (_workFlowEngineOptions)
            {
                if (_flowDic.TryRemove(flow.FlowId, out var temp) && temp == flow)
                {
                    flow.RemoveAllTools();
                    var flowFileDescription = _workFlowEngineOptions.FlowFileDescriptions.FirstOrDefault(x => x.FlowId == flow.FlowId);
                    if (flowFileDescription != null)
                    {
                        File.Delete(flowFileDescription.FilePath);
                        _workFlowEngineOptions.FlowFileDescriptions.Remove(flowFileDescription);
                        File.WriteAllText(_configFilePath, JsonConvert.SerializeObject(_workFlowEngineOptions));
                        _workFlowEngineOptions = JsonConvert.DeserializeObject<WorkFlowEngineOptions>(File.ReadAllText(_configFilePath));
                    }
                }
            }
        }

        public bool RegisterPart(string partName, out PartMapper? partMapper)
        {
            partMapper = null;
            var partBackup = PartDescriptions.GetPartBackup(partName);
            if (partBackup != null)
            {
                _partDict[partBackup.PartId] = partBackup;
                partMapper = new PartMapper(partBackup);
                _partMapperDict[partMapper.PartId] = partMapper;
                OnPartCollectionChanged?.Invoke();
                return true;
            }
            return false;
        }

        public bool RemovePart(PartMapper partMapper)
        {
            if (_partDict.ContainsKey(partMapper.PartId))
            {
                partMapper.UnInitialize();
                return _partMapperDict.Remove(partMapper.PartId, out _) && _partDict.Remove(partMapper.PartId);
            }
            return false;
        }

        public void SaveAllPart()
        {
            lock (_workFlowEngineOptions)
            {
                _workFlowEngineOptions.PartBackups.Clear();
                foreach (var item in _partDict.Values)
                {
                    _workFlowEngineOptions.PartBackups.Add(item);
                }
                File.WriteAllText(_configFilePath, JsonConvert.SerializeObject(_workFlowEngineOptions));
            }
        }

        public void ShutDown()
        {
            foreach (var item in _partMapperDict.Values)
            {
                item.UnInitialize();
            }
            foreach (var item in _flowDic.Values)
            {
                item.RemoveAllTools();
            }
           DisposeManager.Instance.Dispose();
        }

        public IEnumerable<PartMapper> GetPartMappers()
        {
            return _partMapperDict.Values.OrderBy(o => o.PartName).ThenBy(s => s.Description).AsEnumerable();
        }

        public bool ReadFlow(string flowPath, out Flow? flow)
        {
            flow = null;
            if (string.IsNullOrEmpty(flowPath))
            {
                return false;
            }
            if (Path.GetExtension(flowPath) != ".flow")
            {
                return false;
            }

            var settings = new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize
            };
            var flowInfoOptions = JsonConvert.DeserializeObject<FlowInfoOptions>(File.ReadAllText(flowPath), settings);
            if (flowInfoOptions != null)
            {
                var tempflow = new Flow();
                flowInfoOptions.FlowId = Guid.NewGuid();
                flowInfoOptions.FlowName += "副本";
                foreach (var refPartProperty in flowInfoOptions.PartProperties)
                {
                    refPartProperty.OwnerFlowId = flowInfoOptions.FlowId;
                }
                foreach (var item in flowInfoOptions.ParameterProperties)
                {
                    item.OwnerFlowId = flowInfoOptions.FlowId;
                }
                if (tempflow.Deserialize(flowInfoOptions))
                {
                    flow = tempflow;
                    _flowDic[flow.FlowId] = flow;
                    return true;
                }
            }
            return false;
        }

        public void RaisePartCollectionChanged()
        {
            this.OnPartCollectionChanged?.Invoke();
        }

        public bool FlowDeepCopy(FlowInfoOptions options, out Flow? flow)
        {
            flow = null;
            return false;
        }

        public bool FlowDeepCopy(Flow sourceflow, out Flow? flow)
        {
            flow = null;
            return false;
        }

        public Flow[] GetAllFlows()
        {
            return [.. _flowDic.Values];
        }
    }
}
