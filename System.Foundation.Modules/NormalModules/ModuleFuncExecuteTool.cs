using Microsoft.Extensions.Logging;
using QStandaedPlatform.Engine.Common.Common;
using QStandaedPlatform.Engine.Common.Common.Metadatas;
using QStandaedPlatform.Engine.Laboratory;
using System.Collections.Generic;
using System.ComponentModel;
using System.Foundation.Modules.Models;
using QStandaedPlatform.Engine.Extensions;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace System.Foundation.Modules.NormalModules
{
    public class ModuleFuncExecuteData : ModuleData
    {
        [DisplayName("导出监控数据路径")]
        public string ExportMonitorDataPath { get; set; } = Directory.GetCurrentDirectory();


        [DisplayName("导出精度数据路径")]
        public string ExportPrecisionDataPath { get; set; } = Directory.GetCurrentDirectory();
    }
    [DisplayName("模块功能执行工具")]
    public class ModuleFuncExecuteTool : ModuleWithParameterToolBase
    {
        private readonly ModuleFunCodecService _moduleFunCodecService;
        private readonly MangoStorage _mangoStorage;
        public ModuleFuncExecuteTool(ModuleFunCodecService moduleFunCodecService, MangoStorage mangoStorage)
        {
            _moduleFunCodecService = moduleFunCodecService;
            _mangoStorage = mangoStorage;
        }
        private const string _inputTrigger = "触发模块执行";
        private const string _inputDynamicParameter = "接收动态入参模块执行";
        private const string _inputTriggerParameter = "触发模块参数写入";
        private const string _outputParameterCompleted = "模块参数写入完成信号";
        private const string _outputCompleted = "模块执行完成信号";
        private const string _outputExceptionCode = "输出模块异常码";
        public override string DefineName => "模块功能执行工具";

        public override bool InitPins()
        {
            InsetPin(_inputTrigger, this, typeof(QData), PinType.Input);
            //InsetPin(_inputDynamicParameter, this, typeof(QFloat), PinType.Input);
            InsetPin(_inputTriggerParameter, this, typeof(QData), PinType.Input);
            InsetPin(_outputCompleted, this, typeof(QData), PinType.Output);
            InsetPin(_outputParameterCompleted, this, typeof(QData), PinType.Output);
            InsetPin(_outputExceptionCode, this, typeof(QInt), PinType.Output);
            TriggerPointCommands.Add(new TriggerPointCommand(10, "导出模块监控数据"));
            TriggerPointCommands.Add(new TriggerPointCommand(11, "导出模块精度数据"));
            return true;
        }


        public override bool InitDataContext()
        {
            DataContext = new ModuleFuncExecuteData();
            return true;
        }

        public override async Task<CommandResult> ExecuteCommandAsync(ITriggerPointCommand triggerPointCommand)
        {
            var _normalModuleData = Context<ModuleFuncExecuteData>();
            if (triggerPointCommand.Id == 10)
            {
                if (string.IsNullOrEmpty(_normalModuleData.ExportMonitorDataPath))
                {
                    return await Task.FromResult(CommandResult.Error(this.DisplayName, "导出监控路径为空"));
                }
                if (!Directory.Exists(_normalModuleData.ExportMonitorDataPath))
                {
                    Directory.CreateDirectory(_normalModuleData.ExportMonitorDataPath);
                }
                //using var client = this.ToolExecutionContext.Flow.CascadeFlowManager.ModuleDataManager.GetDbContext();
                var tableHeaders = GetModular().ModuleFuncCodeParameter.MonitorInfoItems.Select(p => p.MonitorName).ToList();
                var datas = (await _mangoStorage.GetFlowRunData(new FlowRunDataQueryDto
                {
                    FlowId = this.ToolExecutionContext.Flow.FlowId.ToString("N"),
                })).Where(it => it.ModuleName == GetModular().ModuleInfo.ModuleName).ToList();
                // 按日期分组
                var groupedData = datas.GroupBy(item => item.StartTime.Date);

                foreach (var group in groupedData)
                {
                    var date = group.Key; // 当前分组的日期
                    var fileName = $"{_normalModuleData.ExportMonitorDataPath}\\{this.ToolExecutionContext.Flow.FlowName}-{GetModular().ModuleInfo.ModuleName}-{date:yyyyMMdd}.csv";

                    // 如果文件不存在，则创建
                    if (!File.Exists(fileName))
                    {
                        File.Create(fileName).Dispose();
                    }

                    var tableRows = new List<string[]>();
                    var index = 1;
                    foreach (var item in group)
                    {
                        if (item.TaskDatas.Count > 0)
                        {
                            tableRows.Add([.. tableHeaders]);
                            foreach (var taskData in item.TaskDatas)
                            {
                                var tableRow = new List<string>();
                                foreach (var header in tableHeaders)
                                {
                                    foreach (var taskDataItemEntity in taskData.ModuleReportCollectTaskDataItemEntities)
                                    {
                                        if (taskDataItemEntity.MonitorName == header)
                                        {
                                            tableRow.Add(taskDataItemEntity.MonitorValue);
                                            break;
                                        }
                                    }
                                }
                                tableRows.Add([.. tableRow]);
                            }
                        }

                        tableRows.Add(["数据编号", "操作平台", "模块", "模块工艺参数", "开始时间", "结束时间", "模块单次运行时长"]);
                        tableRows.Add([index.ToString(),item.PlatFormName, item.ModuleName,item.ModuleParameter,
                       item.StartTime.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                       item.EndTime.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                       item.DurationTime.ToString()]);

                        if (item.Alarms.Count > 0)
                        {
                            tableRows.Add(["操作平台", "模块", "报警时间", "报警内容"]);
                            foreach (var alarm in item.Alarms)
                            {
                                tableRows.Add([item.PlatFormName, item.ModuleName,
                               alarm.AlarmTime.Value.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                               alarm.AlarmDesc]);
                            }
                        }
                        //写入换行
                        tableRows.Add([""]);
                        index++;
                    }

                    // 写入文件
                    using (var streamWriter = new StreamWriter(fileName, true) { AutoFlush = true })
                    {
                        foreach (var tableRow in tableRows)
                        {
                            streamWriter.WriteLine(string.Join(",", tableRow));
                        }
                    }
                }
                return await Task.FromResult(CommandResult.Ok(this.DisplayName, "导出模块数据成功"));
            }

            else if (triggerPointCommand.Id == 11)
            {
                if (string.IsNullOrEmpty(_normalModuleData.ExportPrecisionDataPath))
                {
                    return await Task.FromResult(CommandResult.Error(this.DisplayName, "导出精度数据路径为空"));
                }
                if (!Directory.Exists(_normalModuleData.ExportPrecisionDataPath))
                {
                    Directory.CreateDirectory(_normalModuleData.ExportPrecisionDataPath);
                }
                //using var client = this.ToolExecutionContext.Flow.CascadeFlowManager.ModuleDataManager.GetDbContext();
                var tableHeaders = new List<string>();
                tableHeaders.Add("测试平台");
                tableHeaders.Add("模块名称");
                foreach (var item in GetModular().ModuleFuncCodeParameter.PrecisionInfoItems)
                {
                    tableHeaders.Add(item.PrecisionName + "测试值");
                    tableHeaders.Add(item.PrecisionName + "标准值");
                    tableHeaders.Add(item.PrecisionName + "误差");
                    tableHeaders.Add(item.PrecisionName + "误差率");
                }
                tableHeaders.Add("测试时间");
                var datas = (await _mangoStorage.GetFlowRunData(new FlowRunDataQueryDto { FlowId = this.ToolExecutionContext.Flow.FlowId.ToString("N") })).Where(it => it.ModuleName == GetModular().ModuleInfo.ModuleName).ToList();
                // 按日期分组
                var groupedData = datas.GroupBy(item => item.StartTime.Date);
                foreach (var group in groupedData)
                {
                    var date = group.Key; // 当前分组的日期
                    var fileName = $"{_normalModuleData.ExportPrecisionDataPath}\\{this.ToolExecutionContext.Flow.FlowName}-{GetModular().ModuleInfo.ModuleName}精度数据-{date:yyyyMMdd}.csv";
                    var tableRows = new List<List<string>>
                    {
                        tableHeaders
                    };
                    foreach (var item in group)
                    {
                        Logger.LogInformation("打印精度数据：" + string.Join(",", item.PrecisionDatas.Select(it => $"{it.PrecitiName}:{it.PrecitiRealValue}")));
                        var tableRow1 = new List<string>();
                        tableRow1.Add(item.PlatFormName);
                        tableRow1.Add(item.ModuleName);

                        foreach (var config in GetModular().ModuleFuncCodeParameter.PrecisionInfoItems)
                        {
                            var precisionData = item.PrecisionDatas.FirstOrDefault(it => it.PrecitiName == (config.PrecisionName));
                            if (precisionData != null)
                            {
                                tableRow1.Add(precisionData.PrecitiRealValue.ToString());
                                tableRow1.Add(precisionData.PrecitiStandardValue.ToString());
                                tableRow1.Add((Math.Round((precisionData.PrecitiRealValue - precisionData.PrecitiStandardValue), 3).ToString()));
                                tableRow1.Add((Math.Round((precisionData.PrecitiRealValue - precisionData.PrecitiStandardValue) / precisionData.PrecitiStandardValue * 100, 3).ToString() + "%"));
                            }
                        }
                        tableRow1.Add(item.StartTime.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                        tableRows.Add(tableRow1);
                    }
                    // 写入文件
                    //如果存在则清空数据
                    if (File.Exists(fileName))
                    {
                        File.WriteAllText(fileName, string.Empty);
                    }

                    using (var streamWriter = new StreamWriter(fileName, true) { AutoFlush = true })
                    {
                        foreach (var tableRow in tableRows)
                        {
                            streamWriter.WriteLine(string.Join(",", tableRow));
                        }
                    }
                }

                return await Task.FromResult(CommandResult.Ok(this.DisplayName, "导出模块精度数据成功"));
            }
            return await base.ExecuteCommandAsync(triggerPointCommand);
        }
        public override async Task<bool> ExecuteAsync(PinInfo pinInfo, QData pinData, ToolExecutionContext toolContext)
        {
            if (pinInfo.Name == _inputTrigger)
            {
                await _moduleFunCodecService
                     .StartFuncCodeExecuteAsync(this.ToolExecutionContext, GetModular(), Logger, RequestCancelToken);

                SendToPin(_outputCompleted, new QData());
                return true;
            }
            else if (pinInfo.Name == _inputTriggerParameter)
            {
                var parameter = Extensions.GetParameter();
                foreach (var item in GetModular().ModuleFuncCodeParameter.FuncCodeParamterInfos)
                {
                    if (parameter.TryGetValue(item.ParameterAddress, out _))
                    {
                        parameter[item.ParameterAddress] = item.ParameterValueFactory["0"];
                    }
                }
                Logger.LogInformation("写入参数值:{values}", string.Join(",", parameter.Values));
                var parameterAddress = GetModular().ModuleInfo.ModuleParameterAddress;
                await GetModular().WriteParameterAsync(parameterAddress, [.. parameter.Values]);
                SendToPin(_outputParameterCompleted, new QData());
                return true;
            }

            return false;
        }


        public override Task<bool> HandleExecutedModuleErrorAsync(ModularException modularException)
        {
            if (modularException.IsModuleError)
            {
                SendToPin(_outputExceptionCode, (QInt)modularException.ModuleErrorCode);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }







    }
}
