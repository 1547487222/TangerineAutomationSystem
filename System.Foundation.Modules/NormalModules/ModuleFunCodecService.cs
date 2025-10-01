using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QStandaedPlatform.Engine.Common.Common;
using QStandaedPlatform.Engine.Common.Common.Metadatas;
using QStandaedPlatform.Engine.Common.Common.ModuleEntitys;
using QStandaedPlatform.Engine.Common.Common.SampleEntitys;
using QStandaedPlatform.Engine.Laboratory;
using QStandaedPlatform.Engine.Extensions;
using System;
using System.Collections.Generic;
using System.Foundation.Modules.Models;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Extensions = QStandaedPlatform.Engine.Extensions.Extensions;


namespace System.Foundation.Modules.NormalModules
{
    public class ModuleFunCodecService
    {
        private readonly MangoStorage _mangoStorage;
        private readonly SampleService _sampleService;
        public ModuleFunCodecService(MangoStorage mangoStorage, SampleService sampleService)
        {
            _mangoStorage = mangoStorage;
            _sampleService = sampleService;
        }
        /// <summary>
        /// 执行模块功能函数
        /// </summary>
        /// <param name="executionContext"></param>
        /// <param name="modular"></param>
        /// <param name="logger"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task StartFuncCodeExecuteAsync(ToolExecutionContext executionContext, Modular modular, ILogger logger, CancellationToken cancellationToken = default)
        {
            if (modular.ModuleInfo == null)
            {
                throw new Exception("ModuleInfo is null");
            }
            if (modular.ModuleFuncCodeParameter == null)
            {
                throw new Exception("ModuleFuncCodeParameter is null");
            }
            if (string.IsNullOrEmpty(modular.ModuleInfo.ModuleName))
            {
                throw new Exception("ModuleName is null");
            }
            cancellationToken.ThrowIfCancellationRequested();
            var parameter = Extensions.GetParameter();
            foreach (var item in modular.ModuleFuncCodeParameter.FuncCodeParamterInfos)
            {
                if (parameter.TryGetValue(item.ParameterAddress, out _))
                {
                    parameter[item.ParameterAddress] = item.ParameterValueFactory["0"];
                }
            }
            logger.LogInformation("写入参数值:{values}", string.Join(",", parameter.Values));
            ModuleReportRunDataEntity moduleReportRunDataEntity = new()
            {
                ModuleName = modular.ModuleInfo.ModuleName,
                PlatFormName = executionContext.Flow.FlowName,
                ModuleParameter = string.Join(";", parameter.Select(p => $"<{p.Key}:{p.Value}>")),
                ModuleActionDescription = modular.ModuleFuncCodeParameter.FuncCodeDescription,
            };
            await modular.VerifyModuleStatusAsync();
            await modular.WriteParameterAsync([.. parameter.Values]);
            if (!modular.VerifyModuleActivityStatus())
            {
                await modular.ModuleExecuteAsync();
            }
            moduleReportRunDataEntity.StartTime = DateTime.Now;
            moduleReportRunDataEntity.StartDay = DateOnly.FromDateTime(DateTime.Now);
            Task task = null;
            CancellationTokenSource monitor_cts = new();
            if (modular.ModuleFuncCodeParameter.MonitorInfoItems.Count > 0)
            {
                task = Task.Run(async () =>
                {
                    while (!monitor_cts.IsCancellationRequested)
                    {
                        try
                        {
                            await modular.VerifyModuleStatusAsync();
                            ModuleReportMonitorTaskDataOnceEntity moduleReportCollectTaskDataOnceEntity = new();
                            foreach (var monitorDataConfig in modular.ModuleFuncCodeParameter.MonitorInfoItems)
                            {
                                var monitorData = modular.Messenger.ReadSingleValue<float>(monitorDataConfig.MonitorAddress);
                                moduleReportCollectTaskDataOnceEntity
                                .ModuleReportCollectTaskDataItemEntities.Add(new ModuleReportMonitorTaskDataItemEntity
                                {
                                    MonitorName = monitorDataConfig.MonitorName,
                                    MonitorValue = monitorData.ToString()
                                });
                            }
                            moduleReportRunDataEntity.TaskDatas.Add(moduleReportCollectTaskDataOnceEntity);
                            await Task.Delay(modular.ModuleFuncCodeParameter.MonitorInterval == 0 ? Modular.DefaultMonitorInterval : modular.ModuleFuncCodeParameter.MonitorInterval);
                        }
                        catch (Exception ex) when (ex is ModularException modularException)
                        {
                            break;
                        }
                    }
                }, monitor_cts.Token);
            }
            try
            {
                await modular.CheckModuleDoneAsync(cancellationToken);
            }
            catch (Exception ex) when (ex is ModularException modularException)
            {
                if (modularException.IsAlarm)
                {
                    foreach (var item in await modular.GetModuleAlarmInfoAsync())
                    {
                        moduleReportRunDataEntity.Alarms.Add(new ModuleReportAlarmEntity
                        {
                            AlarmAddress = item.AlarmCode,
                            IPAddress = item.ModuleIp,
                            AlarmDesc = item.AlarmDescription,
                            AlarmCode = item.AlarmCode,
                        });
                    }

                    moduleReportRunDataEntity.EndTime = DateTime.Now;
                    moduleReportRunDataEntity.DurationTime = (moduleReportRunDataEntity.EndTime - moduleReportRunDataEntity.StartTime).TotalMilliseconds;
                    monitor_cts.Cancel();
                    if (task != null)
                    {
                        await task;
                    }
                    await _mangoStorage.SaveModuleRunData(new FlowRunDataEntity
                    {
                        FlowId = executionContext.Flow.FlowId.ToString("N"),
                        ModuleRunData = moduleReportRunDataEntity,
                    });
                }
                throw;
            }
            moduleReportRunDataEntity.EndTime = DateTime.Now;
            moduleReportRunDataEntity.DurationTime = (moduleReportRunDataEntity.EndTime - moduleReportRunDataEntity.StartTime).TotalMilliseconds;
            monitor_cts.Cancel();
            if (task != null)
            {
                await task;
            }
            if (modular.ModuleFuncCodeParameter.PrecisionInfoItems.Count > 0)
            {
                foreach (var item in modular.ModuleFuncCodeParameter.PrecisionInfoItems)
                {
                    var precisionValue = modular.Messenger.ReadSingleValue<float>(item.PrecisionAddress);
                    moduleReportRunDataEntity.PrecisionDatas.Add(new ModuleReportPrecisionEntity
                    {
                        PrecitiName = item.PrecisionName,
                        PrecitiRealValue = precisionValue,
                        PrecitiStandardValue = item.PrecisionStandardValue,
                    });
                }
            }
            await _mangoStorage.SaveModuleRunData(new FlowRunDataEntity
            {
                FlowId = executionContext.Flow.FlowId.ToString("N"),
                ModuleRunData = moduleReportRunDataEntity,
            });
        }

        public async Task SampleStartFuncCodeExecuteAsync(Guid actionId, Dictionary<int, SampleTaskInfo> samples, ToolExecutionContext executionContext, Modular modular, ILogger logger, CancellationToken cancellationToken = default)
        {
            if (modular.ModuleInfo == null)
            {
                throw new Exception("ModuleInfo is null");
            }
            if (modular.ModuleFuncCodeParameter == null)
            {
                throw new Exception("ModuleFuncCodeParameter is null");
            }
            if (string.IsNullOrEmpty(modular.ModuleInfo.ModuleName))
            {
                throw new Exception("moduleName is null,Please check the module info");
            }
            cancellationToken.ThrowIfCancellationRequested();
            var parameter = Extensions.GetParameter();
            Dictionary<int, List<SampleTaskDataEntity>> sampleTaskDataEntities = [];

            foreach (var sampleTaskInfo in samples.Values)
            {
                var sampleTraceEntity = _sampleService.GetSampleTrace(sampleTaskInfo.SamplingId);
                sampleTraceEntity.SetModuleInfo(actionId, modular.ModuleFuncCodeParameter);
                sampleTraceEntity.SetBasicInfo(sampleTaskInfo);
                sampleTraceEntity.SetStartTime();
                sampleTraceEntity.TaskEbrDataEntities.Clear();
            }
            foreach (var sampleTaskInfo in samples.Values)
            {
                var sampleTraceEntity = _sampleService.GetSampleTrace(sampleTaskInfo.SamplingId);
                foreach (var item in modular.ModuleFuncCodeParameter.FuncCodeParamterInfos)
                {
                    if (parameter.TryGetValue(item.ParameterAddress, out _))
                    {
                        {
                            parameter[item.ParameterAddress] = item.ParameterValueFactory["0"];
                        }
                    }
                    SampleTaskDataEntity sampleTaskDataEntity = new()
                    {
                        EbrKey = item.ParameterName,
                        EbrKeyDescription = item.ParameterName,
                        EbrValue = item.ParameterValueFactory.First().Value.ToString(),
                        EbrUnit = item.ParameterUnit
                    };
                    sampleTraceEntity.SetEbrData(sampleTaskDataEntity);
                }
            }
            foreach (var sampleTaskInfo in samples.Values)
            {
                var sampleTraceEntity = _sampleService.GetSampleTrace(sampleTaskInfo.SamplingId);
                sampleTraceEntity.SetInputParameters(parameter);
            }
            logger.LogInformation("写入参数值:{values}", string.Join(",", parameter.Values));

            await modular.VerifyModuleStatusAsync();
            await modular.WriteParameterAsync([.. parameter.Values]);
            if (!modular.VerifyModuleActivityStatus())
            {
                await modular.ModuleExecuteAsync();
            }
            try
            {
                if (modular.ModuleFuncCodeParameter.ChannelEbrInfos.Count > 0)
                {
                    if (modular.ModuleFuncCodeParameter.EbrReadStartInterval > 0)
                    {
                        var moduletask = modular.CheckModuleDoneAsync(cancellationToken);
                        await Task.WhenAll([ moduletask, Task.Delay(modular.ModuleFuncCodeParameter.EbrReadStartInterval, cancellationToken).ContinueWith(async _ =>
                        {
                             await modular.VerifyModuleStatusAsync();
                            var ebrs = await modular.ReadEbrDatasAsync();
                           if (ebrs.Count > 0)
                            {
                                foreach (var item in ebrs)
                                {
                                    foreach (var (key, value, unit) in item.Value)
                                    {
                                        if (samples.TryGetValue(item.Key, out SampleTaskInfo sampleTaskInfo))
                                        {
                                             var  sampleTraceEntity = _sampleService.GetSampleTrace(sampleTaskInfo.SamplingId);
                                            SampleTaskDataEntity sampleTaskDataEntity = new();
                                            sampleTaskDataEntity.EbrKey = key;
                                            sampleTaskDataEntity.EbrValue = value.ToString();
                                            sampleTaskDataEntity.EbrUnit = unit;
                                            sampleTaskDataEntity.EbrKeyDescription =key;
                                            sampleTraceEntity.SetEbrData(sampleTaskDataEntity);
                                        }
                                    }
                                }
                            }
                        }, cancellationToken)]);
                    }
                    else
                    {
                        await modular.CheckModuleDoneAsync(cancellationToken);
                        await modular.VerifyModuleStatusAsync();
                        var ebrs = await modular.ReadEbrDatasAsync();
                        if (ebrs.Count > 0)
                        {
                            foreach (var item in ebrs)
                            {
                                foreach (var (key, value, unit) in item.Value)
                                {
                                    if (samples.TryGetValue(item.Key, out SampleTaskInfo sampleTaskInfo))
                                    {
                                        var sampleTraceEntity = _sampleService.GetSampleTrace(sampleTaskInfo.SamplingId);
                                        SampleTaskDataEntity sampleTaskDataEntity = new()
                                        {
                                            EbrKey = key,
                                            EbrValue = value.ToString(),
                                            EbrUnit = unit,
                                            EbrKeyDescription = key
                                        };
                                        sampleTraceEntity.SetEbrData(sampleTaskDataEntity);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    await modular.CheckModuleDoneAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                foreach (var item in samples)
                {
                    var sampleTraceEntity = _sampleService.GetSampleTrace(item.Value.SamplingId);
                    sampleTraceEntity.SetAlertMessage(ex.ToString());
                    sampleTraceEntity.SetEndTime();
                    _sampleService.SaveSampleTrace(sampleTraceEntity.SamplingId);
                }
                throw;
            }
            finally
            {
                foreach (var item in samples)
                {
                    var sampleTraceEntity = _sampleService.GetSampleTrace(item.Value.SamplingId);
                    sampleTraceEntity.SetEndTime();
                    _sampleService.SaveSampleTrace(item.Value.SamplingId);
                }
            }
        }
    }
}
