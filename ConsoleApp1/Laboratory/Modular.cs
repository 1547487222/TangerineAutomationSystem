using Microsoft.Extensions.Logging;
using QStandaedPlatform.Engine.Common.Common;
using QStandaedPlatform.Engine.Components;
using QStandaedPlatform.Engine.Extensions;
using QStandaedPlatform.Engine.Laboratory.Documents;
using System.Net;
using System.Reflection;

namespace QStandaedPlatform.Engine.Laboratory
{
    /// <summary>
    /// 运行状态
    /// </summary>
    public enum RunStatus
    {
        /// <summary>
        /// 默认
        /// </summary>
        Default,

        /// <summary>
        /// 运行中  切换到运行状态至少保持1.5S时间  然后才能切换到完成信号
        /// </summary>
        Running = 108,

        /// <summary>
        /// 错误
        /// </summary>
        Error = 110,

        /// <summary>
        /// 正常运行中，设备自身触发暂停 需要通知UI
        /// </summary>
        Pausing,

        /// <summary>
        /// 停止    用户触发停止 或者 程序出现故障
        /// </summary>
        Stopping,

        /// <summary>
        /// 回零中
        /// </summary>
        Homing,

        /// <summary>
        /// 回零完成
        /// </summary>
        HomeDone,

        /// <summary>
        /// 动作完成
        /// </summary>
        Done = 999
    }
    /// <summary>
    /// 模块状态机
    /// 0:手动状态 1:待机状态 2:自动运行状态 3:未回原 4:回原中 5:单步状态 6:暂停状态 7:停机过程中 8:设备报警 9:急停中
    /// </summary>
    public enum ModuleStatus:short
    {
        Manual,     // 手动状态
        Ready,       // 待机状态
        Running,     // 运行状态
        NoHome,      // 未回零状态
        Homing,      // 回零中状态
        StepByStep,  // 单步状态
        Pausing,     // 暂停状态
        Stopping,    // 停止状态
        Alarm,      // 报警状态
        Emergency    // 急停状态
    }


    public class ModularAlarm
    {
        public string ModuleIp { get; set; } = string.Empty;

        public string ModuleName { get; set; } = string.Empty;

        public string AlarmCode { get; set; } = string.Empty;

        public string AlarmDescription { get; set; } = string.Empty;
    }

    public class ModularException(string message) : Exception(message)
    {
        /// <summary>
        /// 是否急停
        /// </summary>
        public bool IsEmergency { get; set; }

        /// <summary>
        /// 是否报警
        /// </summary>
        public bool IsAlarm { get; set; }

        /// <summary>
        /// 报警码
        /// </summary>
        public string AlarmCode { get; set; } = string.Empty;

        /// <summary>
        /// 是否暂停
        /// </summary>
        public bool IsPause { get; set; }

        /// <summary>
        /// 是否模块异常
        /// </summary>
        public bool IsModuleError { get; set; }

        /// <summary>
        /// 模块异常码
        /// </summary>
        public int ModuleErrorCode { get; set; }

        /// <summary>
        /// 是否未知异常
        /// </summary>
        public bool IsUnknown { get; set; }
        /// <summary>
        /// 是否取消
        /// </summary>
        public bool IsCancelled { get; set; }
        /// <summary>
        /// 当前操作
        /// </summary>
        public string Action { get; set; } = string.Empty;

        /// <summary>
        /// 内部异常信息
        /// </summary>
        public string InternalMessage { get; set; } = string.Empty;
    }
    public class Modular : IModular<IH5uTcp>
    {
        private readonly ILogger _logger;
        /// <summary>
        /// 控制PLC
        /// </summary>
        private readonly IH5uTcp plc;

        /// <summary>
        /// 模块通信对象
        /// </summary>
        public IH5uTcp Messenger
        {
            get
            {
                lock (plc)
                {
                    return plc;
                }
            }
        }
        public Modular(IH5uTcp h5UTcp)
        {
            plc = h5UTcp;
            _logger = LoggerProviderManager.GetLoggerFactory().CreateLogger(typeof(Modular));
        }

        public bool IsDebug { get; set; } = true;

        public ModuleInfoParameter ModuleInfo { get; set; } = new();

        public int Waiting_Interval_Time { get; set; } = 100;

        public const int DefaultMonitorInterval = 1000;

        /// <summary>
        /// 模块名称
        /// </summary>
        public string ModuleName => ModuleInfo.ModuleName;

        public ModuleFuncCodeParameter ModuleFuncCodeParameter { get; set; } = new();


        public void SetModuleInfo(ModuleInfoParameter moduleInfo)
        {
            ModuleInfo = moduleInfo;
        }

        public void SetModuleFuncCodeParameter(ModuleFuncCodeParameter moduleFuncCodeParameter)
        {
            ModuleFuncCodeParameter = moduleFuncCodeParameter;
        }

        public async Task<Dictionary<int, List<(string key, object value, string unit)>>> ReadEbrDatasAsync()
        {
            return await ReadEbrDatasAsync(ModuleFuncCodeParameter.ChannelEbrInfos);
        }

        public async Task<Dictionary<int,List<(string key, object value, string unit)>>> ReadEbrDatasAsync(Dictionary<int,List<ModuleEbrInfoItem>>  dic)
        {
            var ebrDic = new Dictionary<int,List<(string key, object value, string unit)>>();
            foreach (var pair in dic)
            {
                var ebrInfoItems = pair.Value;
                var list= new List<(string key, object value, string unit)>();
                foreach (var item in ebrInfoItems)
                {
                    switch (item.EbrType)
                    {
                        case EbrType.REAL:
                            var realValue = await Messenger.ReadSingleValueAsync<float>(item.EbrAddress);
                            list.Add((item.EbrName, realValue, item.EbrUnit));
                            break;
                        case EbrType.BOOL:
                            var boolValue = await Messenger.ReadSingleBooleanAsync(item.EbrAddress);
                            list.Add((item.EbrName, boolValue, item.EbrUnit));
                            break;
                        case EbrType.INT:
                            var intValue = await Messenger.ReadSingleValueAsync<short>(item.EbrAddress);
                            list.Add((item.EbrName, intValue, item.EbrUnit));
                            break;
                        case EbrType.STRING:
                            var stringValue = await Messenger.ReadStringAsync(item.EbrAddress, item.CharacterLength);
                            list.Add((item.EbrName, stringValue, item.EbrUnit));
                            break;
                        case EbrType.DINT:
                            var dintValue = await Messenger.ReadSingleValueAsync<int>(item.EbrAddress);
                            list.Add((item.EbrName, dintValue, item.EbrUnit));
                            break;
                        default:
                            break;
                    }
                }
                ebrDic.Add(pair.Key, list);
            }
            return ebrDic;
        }


        public async Task<string> ReadCodeAsync()
        {
            return await Messenger.ReadStringAsync(ModuleInfo.ModuleScanAddress,ModuleInfo.ModuleScanAddressLength);
        }

        /// <summary>
        /// 手动自动切换
        /// </summary>
        /// <param name="autoswitch">true：切换到自动    false：切换到手动</param>
        /// <returns></returns>
        [Lock]
        public async Task<bool> ManualAutoAsync(bool autoswitch)
        {
            Log($"执行手自动切换:{(autoswitch ? "自动" : "手动")}");
            var result = await Messenger.WriteMultiBooleanAsync(ModuleInfo.ModuleManualAutoControlAddress, autoswitch);
            if (!result)
            {
                Log("手自动切换失败!");
                throw RaiseModularException()(new ModularException("手自动切换失败!") { IsUnknown = true, Action = $"执行手自动切换:{(autoswitch ? "自动" : "手动")}" });
            }
            return true;
        }
        /// <summary>
        /// 启动程序
        /// </summary>
        /// <returns></returns>
        [Lock]
        public async Task<bool> StartAsync()
        {
            Log($"Start 启动程序 {ModuleInfo.ModuleStartControlAddress}");
            var result = await Messenger.PlusOutputAsync(ModuleInfo.ModuleStartControlAddress, 1000);
            if (!result)
            {
                Log("Start 触发出错!");
                throw RaiseModularException()(new ModularException("Start 触发出错!") { IsUnknown = true,Action= "模块启动" });
            }

            return true;
        }
        /// <summary>
        /// 程序回零
        /// </summary>
        /// <returns></returns>
        public async Task<bool> HomeAsync()
        {
            Log("程序回零");
            //清除触发状态信息
            var ret = await ClearControlAndRunStateAsync();
            if (!ret)
            {
                throw RaiseModularException()(new ModularException("清除功能码和指令执行状态 失败!") { IsUnknown = true,Action= "清除功能码和指令执行状态" });
            }
            //切换到手动
            var manualResult = await ManualAutoAsync(false);
            if (!manualResult)
            {
                throw RaiseModularException()(new ModularException("切换到手动失败!") { IsUnknown = true,Action= "切换到手动" });
            }
            var alarmResult = await ResetAsync();
            if (!alarmResult)
            {
                throw RaiseModularException()(new ModularException("复位报警失败!") { IsUnknown = true,Action= "复位报警" });
            }
            //复位急停
            await ResetEmergencyStopAsync();
            //触发home动作
            var result = await Messenger.PlusOutputAsync(ModuleInfo.ModuleHomeControlAddress, 1000);
            if (!result)
            {
                throw RaiseModularException()(new ModularException("初始化出错!") { IsUnknown = true,Action= "初始化" });
            }

            return await CheckHomeDoneAsync(ModuleInfo.ModuleHomeStateAddress);
        }
        /// <summary>
        /// 程序急停
        /// </summary>
        /// <returns></returns>
        [Lock]
        public async Task<bool> EmergencyStopAsync()
        {
            Log("紧急停止按钮置位");
            var result = await Messenger.WriteMultiBooleanAsync(ModuleInfo.ModuleEmergencyControlAddress, true);
            if (!result)
            {
                Log("EmergencyStop 触发出错!");
                throw RaiseModularException()(new ModularException("EmergencyStop 触发出错!") { IsUnknown = true,Action= "紧急停止" });
            }
            Thread.Sleep(Waiting_Interval_Time);
            //检查状态机
            var machineStatus = await GetModuleStatusAsync();
            if (machineStatus != ModuleStatus.Emergency && machineStatus != ModuleStatus.Stopping)
            {
                throw RaiseModularException()(new ModularException("紧急停止失败!") { IsUnknown = true,Action= "紧急停止" });
            }

            return true;
        }

        /// <summary>
        /// 释放急停按钮
        /// </summary>
        /// <returns></returns>
        [Lock]
        public async Task<bool> ResetEmergencyStopAsync()
        {
            Log("紧急停止按钮复位");
            //紧急停止按钮复位
            var result = await Messenger.WriteMultiBooleanAsync(ModuleInfo.ModuleEmergencyControlAddress, false);
            if (!result)
            {
                Log("ResetEmergencyStop 触发出错!");
                throw RaiseModularException()(new ModularException("ResetEmergencyStop 触发出错!") { IsUnknown = true,Action= "释放急停按钮" });
            }
            Thread.Sleep(Waiting_Interval_Time);
            //检查状态机
            var machineStatus =await GetModuleStatusAsync();
            if (machineStatus != ModuleStatus.Emergency)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 写入模块参数
        /// </summary>
        /// <param name="Address"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        [Lock]
        public async Task<bool> WriteParameterAsync<T>(string Address, T[] parameters)
        {
            if (ModuleFuncCodeParameter.RequiresParameter)
            {
                if (parameters == null || parameters.Length == 0)
                    throw RaiseModularException()(new ModularException("parameters参数为空!") { IsUnknown = true,Action= "写入模块参数" });
                Log($"写入模块参数{string.Join(",", parameters)}");
                return await Messenger.WriteMultiValueAsync(Address, parameters);
            }
            return true;
        }

        public async Task<bool> WriteParameterAsync<T>(T[] parameters)
        {
          return await WriteParameterAsync(ModuleInfo.ModuleParameterAddress, parameters);
        }

        public async Task<bool> WriteModuleParameterAsync()
        {
            var parameter = Extensions.Extensions.GetParameter();
            foreach (var item in ModuleFuncCodeParameter.FuncCodeParamterInfos)
            {
                if (parameter.TryGetValue(item.ParameterAddress, out _))
                {
                    parameter[item.ParameterAddress] = item.ParameterValueFactory["0"];
                }
            }
            return await WriteParameterAsync([.. parameter.Values]);
        }



        [Lock]
        public async Task<bool> ModuleExecuteAsync(string Address, short funcCode)
        {
            var retWrite = await _logger.RecordActionConsumeTimeAsync("ModuleExecuteAsync", async () =>
              {
                  _ = await Messenger.WriteSingleValueAsync(Address, 0);
                  await Task.Delay(Waiting_Interval_Time);
                  Log($"写入功能码{funcCode}");
                  return await Messenger.WriteSingleValueAsync(Address, funcCode);
              });
            if (!retWrite)
            {
                throw RaiseModularException()(new ModularException($"写入功能码{funcCode}失败!") { IsUnknown = true });
            }
            var retRead = await Messenger.ReadSingleValueAsync<short>(Address);
            if (retRead != funcCode)
            {
                throw RaiseModularException()(new ModularException($"读取功能码值{retRead}不等于写入功能码{funcCode},执行失败!") { IsUnknown = true,Action = "功能码写入" });
            }
            return retWrite;
        }

        public async Task<bool> ModuleExecuteAsync()
        {
            return await ModuleExecuteAsync(ModuleInfo.ModuleFuncCodeAddress, (short)ModuleFuncCodeParameter.FuncCode);
        }
        /// <summary>
        /// 检查模块当前是否活动中
        /// </summary>
        /// <param name="doneInfo"></param>
        /// <returns></returns>
        [Lock]
        public bool VerifyModuleActivityStatus(string doneInfo)
        {
            var statusInfo = Messenger.ReadSingleValue<short>(doneInfo);
            if (statusInfo == (int)RunStatus.Running || statusInfo == (int)RunStatus.Done)
            {
                Log($"模块当前在活动中{statusInfo}");
                return true;
            }
            Log($"模块当前不在活动中{statusInfo}");
            return false;
        }

        /// <summary>
        /// 检查模块当前是否活动中
        /// </summary>
        /// <param name="doneInfo"></param>
        /// <returns></returns>
        public  bool VerifyModuleActivityStatus()
        {
            return VerifyModuleActivityStatus(ModuleInfo.ModuleFuncStateCodeAddress);
        }

        /// <summary>
        /// 清除功能码和指令执行状态
        /// </summary>
        /// <returns></returns>
        [Lock]
        public async Task<bool> ClearControlAndRunStateAsync()
        {
            Log(" 清除功能码和指令执行状态 ");
            //清空指令寄存器
            var ClearResult1 = await Messenger.WriteSingleValueAsync<ushort>(ModuleInfo.ModuleFuncCodeAddress, 0);
            if (!ClearResult1)
            {
                throw RaiseModularException()(new ModularException("清空指令寄存器出错!") { IsUnknown = true,Action= "清除功能码和指令执行状态" });
            }
            //清空指令状态寄存器
            var ClearResult2 = await Messenger.WriteSingleValueAsync<ushort>(ModuleInfo.ModuleFuncStateCodeAddress, 0);
            if (!ClearResult2)
            {
                throw RaiseModularException()(new ModularException("清空指令状态寄存器出错!") { IsUnknown = true,Action= "清除功能码和指令执行状态" });
            }
            return true;
        }
        /// <summary>
        /// 获取模块状态
        /// </summary>
        /// <returns><see cref="ModuleStatus"/></returns>
        [Lock]
        public async Task<ModuleStatus> GetModuleStatusAsync()
        {
            var machineStatus = await Messenger.ReadSingleValueAsync<short>(ModuleInfo.ModuleStateAddress);
            return (ModuleStatus)machineStatus;
        }
        /// <summary>
        /// 获取模块状态
        /// </summary>
        /// <returns></returns>
        public ModuleStatus GetModuleStatus()
        {
            var machineStatus = Messenger.ReadSingleValue<short>(ModuleInfo.ModuleStateAddress);
            return (ModuleStatus)machineStatus;
        }

        /// <summary>
        /// 获取模块状态字符串格式
        /// </summary>
        /// <returns></returns>
        public string GetModuleStatusString()
        {
            return GetModuleStatus().ToString();
        }


        /// <summary>
        /// 复位程序
        /// </summary>
        /// <returns></returns>
        [Lock]
        public async Task<bool> ResetAsync()
        {
            //脉冲触发报警复位按钮
            var result = await Messenger.PlusOutputAsync(ModuleInfo.ModuleResetControlAddress, 1000);
            if (!result)
            {
                Log("Reset 触发出错!");
                throw RaiseModularException()(new ModularException("Reset 触发出错!") { IsUnknown = true,Action= "复位" });
            }
            Thread.Sleep(Waiting_Interval_Time);
            //检查状态机
            var machineStatus =await GetModuleStatusAsync();
            if (machineStatus != ModuleStatus.Alarm)
            {
                return true;
            }
            Log($"Reset machineStatus : {machineStatus}");
            throw RaiseModularException()(new ModularException($"复位后模块状态机还是报警状态, machineStatus : {machineStatus}") { IsUnknown = true,Action = "复位" });
        }

        /// <summary>
        /// 校验是否初始化完成
        /// </summary>
        /// <returns></returns>
        [Lock]
        public async Task<bool> VerifyInitAsync()
        {
           return await Messenger.ReadSingleBooleanAsync(ModuleInfo.ModuleInitCompleteAddress);
        }
        /// <summary>
        /// 暂停程序
        /// </summary>
        /// <returns></returns>
        [Lock]
        public async Task<bool> PauseAsync()
        {
            //暂停按钮触发
            var result = await Messenger.PlusOutputAsync(ModuleInfo.ModulePauseControlAddress, 1000);
            if (!result)
            {
                Log("Pause 触发出错!");
                throw RaiseModularException()(new ModularException("Pause 触发出错!") { IsUnknown = true,Action= "暂停" });
            }
            Thread.Sleep(Waiting_Interval_Time);
            //检查状态机
            var machineStatus = await GetModuleStatusAsync();
            if (machineStatus == ModuleStatus.Pausing)
            {
                return true;
            }
            Log($"Pause machineStatus : {machineStatus}");
            throw RaiseModularException()(new ModularException($"暂停后模块状态机不是暂停状态, machineStatus : {machineStatus}") { IsUnknown = true,Action= "暂停" });
        }
        /// <summary>
        /// 切换到自动状态并启动状态机 当状态机为Running时才返回true否则本次执行失败
        /// </summary>
        /// <returns></returns>
        [Lock]
        public async Task<bool> AutoAndStartAsync()
        {
            //手自动按钮切换
            var autoResult = await this.ManualAutoAsync(true);
            if (!autoResult)
            {
                Log("Reset 成功，但转自动出错!");
                throw RaiseModularException()(new ModularException("Reset 成功，但转自动出错!") { Action= "切换自动" });
            }
            //启动按钮触发
            var startResult = await StartAsync();
            if (!startResult)
            {
                Log("Reset 成功，但转启动出错!");
                throw RaiseModularException()(new ModularException("Reset 成功，但转启动出错!") { Action= "启动" });
            }
            await Task.Delay(Waiting_Interval_Time);
            //检查状态机
            var machineStatus = await GetModuleStatusAsync();
            if (machineStatus == ModuleStatus.Running)
            {
                Log($"AutoAndStart machineStatus : {machineStatus}");
                return true;
            }
            else
            {
                Log($"启动 成功，但当前设备状态机状态不对: : {machineStatus}");
                throw RaiseModularException()(new ModularException($"启动 成功，但当前设备状态机状态不对:{machineStatus}!") { IsUnknown = true,Action = "启动" });
            }
        }

        /// <summary>
        /// 判断程序是否完成回零
        /// </summary>
        /// <returns></returns>
        public async Task<bool> CheckHomeDoneAsync(string runstatusInfo, CancellationToken cancellationToken = default)
        {
            Log("判断程序是否完成回零");
            var sleep = 500;
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                    throw RaiseModularException()(new ModularException($"runstatusInfo{runstatusInfo} 检查回原动作完成信号被取消!") { IsCancelled = true,Action="回零" });
                Thread.Sleep(sleep);
                var machineStatus = await GetModuleStatusAsync();

                if (machineStatus == ModuleStatus.Emergency)
                {
                    throw RaiseModularException()(new ModularException("模块处于急停状态")
                    {
                        IsEmergency = true, Action = "回零"
                    });
                }
                else if (machineStatus == ModuleStatus.Alarm)
                {
                    var errors = await Messenger.ReadMultiBooleanAsync(ModuleInfo.ModuleAlrmAddress, 400);
                    for (int i = 0; i < errors.Count; i++)
                    {
                        if (errors[i])
                        {
                            if (ModuleInfo.AlarmItems.Count > 0)
                            {
                                var item = ModuleInfo.AlarmItems.FirstOrDefault(x => x.AlarmAddress == $"B{i}");
                                if (item != null)
                                {
                                    throw RaiseModularException()(new ModularException($"模块处于报警状态,B{i}")
                                    {
                                        IsAlarm = true, Action = "回零", AlarmCode = item.AlarmAddress
                                    });
                                }
                            }
                            else
                            {
                                throw RaiseModularException()(new ModularException($"模块处于报警状态,B{i}")
                                {
                                    IsAlarm = true,Action = "回零", AlarmCode = $"B{i}"
                                });
                            }
                        }
                    }
                    throw new ModularException($"模块处于报警状态")
                    {
                        IsAlarm = true,
                    };
                }
                else
                {
                    int status = await Messenger.ReadSingleValueAsync<short>(runstatusInfo);

                    if (status == (int)RunStatus.Running)
                    {
                        continue;
                    }
                    else if (status == (int)RunStatus.Done)
                    {
                        Log($"回零程序正常执行完成!");
                        var clearResult = await Messenger.WriteSingleValueAsync<short>(runstatusInfo, 0);
                        if (!clearResult)
                        {
                            Log($"回零成功，但状态D200清零失败!");
                            throw RaiseModularException()(new ModularException("回零成功，但状态D200清零失败!") { IsUnknown = true , Action = "回零" });
                        }

                        //切换到自动
                        var autoResult = await this.ManualAutoAsync(true);
                        if (!autoResult)
                        {
                            Log($"回零成功，但切换到自动失败!");
                            throw RaiseModularException()(new ModularException("回零成功，但切换到自动失败!") { IsUnknown = true , Action = "回零" });
                        }
                        //启动按钮
                        var startResult = await StartAsync();
                        if (!startResult)
                        {
                            Log($"回零成功，启动操作失败!");
                            throw RaiseModularException()(new ModularException("回零成功，启动操作失败!") { IsUnknown = true , Action = "回零" });
                        }
                        return true;
                    }
                    else
                    {
                        Log($"程序状态异常，触发停止 controlInfo:{runstatusInfo} status:{status}!");
                        throw RaiseModularException()(new ModularException($"程序状态异常，触发停止 controlInfo:{runstatusInfo} status:{status}!") { IsUnknown = true , Action = "回零" });
                    }
                }
            }
        }


        public async Task<bool> CheckModuleHomeDoneAsync(CancellationToken cancellationToken = default)
        {
            return await CheckHomeDoneAsync(ModuleInfo.ModuleHomeStateAddress,cancellationToken);
        }
        /// <summary>
        /// 检查动作完成信号
        /// </summary>
        /// <param name="doneInfo">检查完成信号的信息</param>
        /// <param name="controlInfo">触发控制信息</param>
        /// <returns></returns>
        public async Task<bool> CheckDoneAsync(string doneInfo, string controlInfo, CancellationToken cancellationToken = default)
        {
            int sleep = 500;
            var index_Error = 0;

            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                    throw new ModularException($"doneInfo{doneInfo} controlInfo{controlInfo} 检查动作完成信号被取消!") { IsCancelled = true , Action =this.ModuleFuncCodeParameter.FuncCodeDescription };
                await Task.Delay(sleep, cancellationToken);
                //获取模块状态机
                var machineStatus = await GetModuleStatusAsync();
                //状态机在运行状态时
                if (machineStatus == ModuleStatus.Running)
                {
                    //获取指令返回结果
                    var stepInfo = Messenger.ReadSingleValue<short>(doneInfo);
                    if (stepInfo == (int)RunStatus.Running || stepInfo == (int)RunStatus.Done)
                    {
                        var ret = false;
                        while (true)
                        {
                            if (cancellationToken.IsCancellationRequested)
                                throw RaiseModularException()(new ModularException($"doneInfo{doneInfo} controlInfo{controlInfo} 检查动作完成信号被取消!") { IsCancelled = true, Action = this.ModuleFuncCodeParameter.FuncCodeDescription });
                            //获取模块状态机
                            machineStatus = await GetModuleStatusAsync();
                            if (machineStatus == ModuleStatus.Running)
                            {
                                stepInfo = Messenger.ReadSingleValue<short>(doneInfo);
                                if (stepInfo == (int)RunStatus.Done)
                                {
                                   
                                    Log($"程序正常执行完成 state:{stepInfo}");

                                    ret = await Messenger.WriteSingleValueAsync<short>(controlInfo, 0);
                                    Log($"复位控制信号:{ret}：{controlInfo}");
                                    Log($"复位控制信号结果{controlInfo}:{await Messenger.ReadSingleValueAsync<short>(controlInfo)}");
                                    if (!ret)
                                    {
                                        throw RaiseModularException()(new ModularException("控制信号清零出错!") { IsUnknown = true, Action = this.ModuleFuncCodeParameter.FuncCodeDescription });
                                    }
                                    //复位完成信号
                                    ret = await Messenger.WriteSingleValueAsync<short>(doneInfo, 0);
                                    Log($"复位完成信号:{ret}：{doneInfo}");
                                    Log($"复位完成信号结果{doneInfo}:{await Messenger.ReadSingleValueAsync<short>(doneInfo)}");
                                    if (!ret)
                                    {
                                        throw RaiseModularException()(new ModularException("完成信号确认清零出错!") { IsUnknown = true, Action = this.ModuleFuncCodeParameter.FuncCodeDescription });
                                    }
                                    Thread.Sleep(ModuleInfo.ClearWaitTime);
                                    return true;
                                }
                                else
                                {
                                    if (stepInfo != (int)RunStatus.Running)
                                    {
                                        Log($"程序执行中 stepInfo:{stepInfo}");
                                        if (ModuleInfo.RedirectErrorCodes.Contains(stepInfo))
                                        {
                                            //异常重定向
                                            Log($"异常重定向 stepInfo:{stepInfo}");
                                            throw RaiseModularException()(new ModularException($"异常重定向 stepInfo:{stepInfo}") { IsModuleError = true, Action = this.ModuleFuncCodeParameter.FuncCodeDescription, ModuleErrorCode = stepInfo });
                                        }
                                    }
                                }
                            }
                            //状态机在暂停状态
                            else if (machineStatus == ModuleStatus.Pausing)
                            {
                                Log($"程序暂停 controlInfo:{controlInfo}");
                                throw RaiseModularException()(new ModularException("PLC程序触发暂停!") { IsPause = true, Action = this.ModuleFuncCodeParameter.FuncCodeDescription });
                            }
                            else if (machineStatus == ModuleStatus.Emergency)
                            {
                                Log($"程序急停 controlInfo:{controlInfo}");
                                throw RaiseModularException()(new ModularException("模块处于急停状态")
                                {
                                    IsEmergency = true,
                                    Action = this.ModuleFuncCodeParameter.FuncCodeDescription
                                });
                            }
                            else if (machineStatus == ModuleStatus.Alarm)
                            {
                                Log($"模块处于报警状态 controlInfo:{controlInfo}");
                                var errors = await Messenger.ReadMultiBooleanAsync(ModuleInfo.ModuleAlrmAddress, 400);
                                for (int i = 0; i < errors.Count; i++)
                                {
                                    if (errors[i])
                                    {
                                        if (ModuleInfo.AlarmItems.Count > 0)
                                        {
                                            var item = ModuleInfo.AlarmItems.FirstOrDefault(x => x.AlarmAddress == $"B{i}");
                                            if (item != null)
                                            {
                                                throw RaiseModularException()(new ModularException($"模块处于报警状态<{item.AlarmAddress}:{item.AlarmDescription}>")
                                                {
                                                    IsAlarm = true,
                                                    Action = this.ModuleFuncCodeParameter.FuncCodeDescription,
                                                    AlarmCode = item.AlarmAddress
                                                });
                                            }
                                        }
                                        else
                                        {
                                            throw RaiseModularException()(new ModularException($"模块处于报警状态B{i}")
                                            {
                                                IsAlarm = true,
                                                Action = this.ModuleFuncCodeParameter.FuncCodeDescription,
                                                AlarmCode = $"B{i}"
                                            });
                                        }
                                    }
                                }
                            }
                            //状态机在非暂停状态
                            else
                            {
                                Log($"模块状态机异常，触发停止 controlInfo:{controlInfo}  machineStatus:{machineStatus}");
                                throw RaiseModularException()(new ModularException($"模块状态机异常，触发停止 controlInfo:{controlInfo}  machineStatus:{machineStatus}") { IsUnknown = true, Action = this.ModuleFuncCodeParameter.FuncCodeDescription });
                            }
                            await Task.Delay(sleep, cancellationToken);
                        }
                    }
                    else
                    {
                        index_Error++;
                        if (index_Error > 3)
                        {
                            Log($"模块状态机异常，触发停止 controlInfo:{controlInfo}  machineStatus:{machineStatus}");
                            throw RaiseModularException()(new ModularException($"模块应答异常:{stepInfo}{(RunStatus)stepInfo}，当前模块plc需要反馈108!") { IsUnknown = true, Action = this.ModuleFuncCodeParameter.FuncCodeDescription });
                        }
                    }
                    await Task.Delay(sleep, cancellationToken);
                }
                //状态机在非运行状态时
                else
                {
                    //状态机在暂停状态
                    if (machineStatus == ModuleStatus.Pausing)
                    {
                        Log($"程序触发暂停 controlInfo:{controlInfo}");
                        throw RaiseModularException()(new ModularException("模块处于暂停状态") { IsPause = true, Action = this.ModuleFuncCodeParameter.FuncCodeDescription });
                    }
                    else if (machineStatus == ModuleStatus.Emergency)
                    {
                        Log($"模块处于急停状态 controlInfo:{controlInfo}");
                        throw RaiseModularException()(new ModularException("模块处于急停状态")
                        {
                            IsEmergency = true,
                            Action = this.ModuleFuncCodeParameter.FuncCodeDescription
                        });
                    }
                    else if (machineStatus == ModuleStatus.Alarm)
                    {
                        Log($"模块处于报警状态 controlInfo:{controlInfo}");
                        var errors = await Messenger!.ReadMultiBooleanAsync(ModuleInfo.ModuleAlrmAddress, 400);
                        for (int i = 0; i < errors.Count; i++)
                        {
                            if (errors[i])
                            {
                                if (ModuleInfo.AlarmItems.Count > 0)
                                {
                                    var item = ModuleInfo.AlarmItems.FirstOrDefault(x => x.AlarmAddress == $"B{i}");
                                    if (item != null)
                                    {
                                        throw RaiseModularException()(new ModularException($"模块处于报警状态<{item.AlarmAddress}:{item.AlarmDescription}>")
                                        {
                                            IsAlarm = true,
                                            Action = this.ModuleFuncCodeParameter.FuncCodeDescription,
                                            AlarmCode = item.AlarmAddress
                                        });
                                    }
                                }
                                else
                                {
                                    throw RaiseModularException()(new ModularException($"模块处于报警状态B{i}")
                                    {
                                        IsAlarm = true,
                                        Action = this.ModuleFuncCodeParameter.FuncCodeDescription,
                                        AlarmCode = $"B{i}"
                                    });
                                }
                            }
                        }
                    }
                    //状态机在非暂停状态
                    else
                    {
                        Log($"模块状态机异常，触发停止 controlInfo:{controlInfo}  machineStatus:{machineStatus}");
                        throw RaiseModularException()(new ModularException($"模块状态机异常，触发停止 controlInfo:{controlInfo}  machineStatus:{machineStatus}") { IsUnknown = true, Action = this.ModuleFuncCodeParameter.FuncCodeDescription });
                    }
                }
            }
        }

        public async Task<bool> CheckModuleDoneAsync(CancellationToken cancellationToken=default)
        {
            return await CheckDoneAsync(ModuleInfo.ModuleFuncStateCodeAddress,ModuleInfo.ModuleFuncCodeAddress,cancellationToken);
        }

        /// <summary>
        /// 获取报警信息
        /// </summary>
        /// <returns></returns>
        [Lock]
        public async Task<List<ModularAlarm>> GetModuleAlarmInfoAsync()
        {
            var list = new List<ModularAlarm>();
            var errors = await Messenger.ReadMultiBooleanAsync(ModuleInfo.ModuleAlrmAddress, ModuleInfo.ModuleAlrmAddressLength);
            for (int i = 0; i < errors.Count; i++)
            {
                if (errors[i])
                {
                    if (ModuleInfo.AlarmItems.Count > 0)
                    {
                        var item = ModuleInfo.AlarmItems.FirstOrDefault(x => x.AlarmAddress == $"B{i}");
                        if (item != null)
                        {
                            list.Add(new ModularAlarm
                            {
                                AlarmCode = $"B{i}",
                                AlarmDescription = item.AlarmDescription,
                                ModuleIp = Messenger.Ip,
                                ModuleName = ModuleInfo.ModuleName,
                            });
                        }
                    }
                    else
                    {
                        list.Add(new ModularAlarm
                        {
                            AlarmCode = $"B{i}",
                            ModuleIp = Messenger.Ip,
                            ModuleName = ModuleInfo.ModuleName,
                        });
                    }
                }
            }
            return list;
        }

        public List<ModularAlarm> GetModuleAlarmInfo()
        {
            var list = new List<ModularAlarm>();
            var errors = Messenger.ReadMultiBoolean(ModuleInfo.ModuleAlrmAddress, ModuleInfo.ModuleAlrmAddressLength);
            for (int i = 0; i < errors.Count; i++)
            {
                if (errors[i])
                {
                    if (ModuleInfo.AlarmItems.Count > 0)
                    {
                        var item = ModuleInfo.AlarmItems.FirstOrDefault(x => x.AlarmAddress == $"B{i}");
                        if (item != null)
                        {
                            list.Add(new ModularAlarm
                            {
                                AlarmCode = $"B{i}",
                                AlarmDescription = item.AlarmDescription,
                                ModuleIp = Messenger.Ip,
                                ModuleName = ModuleInfo.ModuleName,
                            });
                        }
                        else
                        {
                            list.Add(new ModularAlarm
                            {
                                AlarmCode = $"B{i}",
                                ModuleIp = Messenger.Ip,
                                ModuleName = ModuleInfo.ModuleName,
                            });
                        }
                    }
                    else
                    {
                        list.Add(new ModularAlarm
                        {
                            AlarmCode = $"B{i}",
                            ModuleIp = Messenger.Ip,
                            ModuleName = ModuleInfo.ModuleName,
                        });
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// 校验设备状态
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ModularException"></exception>
        [Lock]
        public async Task VerifyModuleStatusAsync()
        {
            var machineStatus = await GetModuleStatusAsync();
            if (machineStatus == ModuleStatus.Pausing)
            {
                throw RaiseModularException()(new ModularException("模块处于暂停状态") { IsPause = true,Action= "校验设备状态" });
            }
            else if (machineStatus == ModuleStatus.Emergency)
            {
                throw RaiseModularException()(new ModularException("模块处于急停状态")
                {
                    IsEmergency = true,Action= "校验设备状态"
                });
            }
            else if (machineStatus == ModuleStatus.Alarm)
            {
                var errors = await Messenger.ReadMultiBooleanAsync(ModuleInfo.ModuleAlrmAddress, 400);
                for (int i = 0; i < errors.Count; i++)
                {
                    if (errors[i])
                    {
                        if (ModuleInfo.AlarmItems.Count > 0)
                        {
                            var item = ModuleInfo.AlarmItems.FirstOrDefault(x => x.AlarmAddress == $"B{i}");
                            if (item != null)
                            {
                                throw RaiseModularException()(new ModularException($"模块处于报警状态<{item.AlarmAddress}:{item.AlarmDescription}>")
                                {
                                    IsAlarm = true,Action= "校验设备状态", AlarmCode = item.AlarmAddress
                                });
                            }
                        }
                        else
                        {
                            throw RaiseModularException()(new ModularException($"模块处于报警状态B{i}")
                            {
                                IsAlarm = true,Action= "校验设备状态", AlarmCode = $"B{i}"
                            });
                        }
                    }
                }
                throw RaiseModularException()(new ModularException("模块处于报警状态")
                {
                    IsAlarm = true,Action = "校验设备状态"
                });
            }
        }
        public void Log(string message)
        {
            if (IsDebug)
            {
                _logger?.LogInformation("{ModuleName}:{message}", ModuleInfo.ModuleName, message);
            }
        }
        /// <summary>
        /// 发起模块异常
        /// </summary>
        /// <param name="message"></param>
        /// <exception cref="ModularException"></exception>
        public Func<ModularException, ModularException> RaiseModularException()
        {
            // 模块ip|模块名称|模块描述|模块状态|报警代码|报警描述
            return  modularEx =>  new ModularException($"模块ip{Messenger.Ip}|模块名称:{ModuleInfo.ModuleName}|模块描述:{ModuleInfo.ModuleDescription}|模块状态:{ GetModuleStatusString()}|报警代码:{modularEx.AlarmCode}|报警描述:{modularEx.Message}")
            {
                IsAlarm = modularEx.IsAlarm,
                IsCancelled = modularEx.IsCancelled,
                IsEmergency = modularEx.IsEmergency,
                IsModuleError = modularEx.IsModuleError,
                IsPause = modularEx.IsPause,
                IsUnknown = modularEx.IsUnknown,
                Action = modularEx.Action,
                ModuleErrorCode = modularEx.ModuleErrorCode,
                AlarmCode = modularEx.AlarmCode,
                InternalMessage = modularEx.Message,
            };
        }
    }
}
