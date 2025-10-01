using Microsoft.Extensions.Logging;
using QStandaedPlatform.Engine.Common.Common;
using QStandaedPlatform.Engine.Common.Common.Metadatas;
using QStandaedPlatform.Engine.Components;
using QStandaedPlatform.Engine.Components.Modbus;
using QStandaedPlatform.Engine.Laboratory.Documents;
using System.Collections.Concurrent;

namespace QStandaedPlatform.Engine.Laboratory
{
    public enum InternalTransferStatus
    {
        /// <summary>
        /// 占用
        /// </summary>
        Busy,
        /// <summary>
        /// 空闲
        /// </summary>
        Free,
    }
    public class TransferCallService:CallServiceBase
    {
        private readonly TransferModuleInfo _transferModuleInfo;
        private readonly object _statusLock = new();
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _resetTcss = new();
        private volatile InternalTransferStatus _status = InternalTransferStatus.Free;
        private readonly ReentrantLockService<IH5uTcp> _lockService;
        public TransferCallService(TransferModuleInfo transferModuleInfo)
        {
            _transferModuleInfo = transferModuleInfo;
            _logger = LoggerProviderManager.GetLoggerFactory().CreateLogger(_transferModuleInfo.TransferModuleName);
            var reentrantLockService = Container.GetService<ReentrantLockService<IH5uTcp>>();
            if (reentrantLockService == null)
            {
                _logger.LogError("Container未找到ReentrantLockService<IH5uTcp>");
                throw new Exception("Container未找到ReentrantLockService<IH5uTcp>");
            }
            _lockService = reentrantLockService;
            _resetTcss.TryAdd(TransferForwardAsyncName, new SemaphoreSlim(0, 1));
            //_logger.LogInformation("Add SemaphoreSlim {0}", TransferForwardAsyncName);
            _resetTcss.TryAdd(TransferBackwardAsyncName, new SemaphoreSlim(0, 1));
           // _logger.LogInformation("Add SemaphoreSlim {0}", TransferBackwardAsyncName);
            _resetTcss.TryAdd(LeftTrayToTransferAsyncName, new SemaphoreSlim(0, 1));
            //_logger.LogInformation("Add SemaphoreSlim {0}", LeftTrayToTransferAsyncName);
            _resetTcss.TryAdd(RightTrayToTransferAsyncName, new SemaphoreSlim(0, 1));
            //_logger.LogInformation("Add SemaphoreSlim {0}", RightTrayToTransferAsyncName);
            _resetTcss.TryAdd(LeftTransferToTrayAsyncName, new SemaphoreSlim(0, 1));
            //_logger.LogInformation("Add SemaphoreSlim {0}", LeftTransferToTrayAsyncName);
            _resetTcss.TryAdd(RightTransferToTrayAsyncName, new SemaphoreSlim(0, 1));
            //_logger.LogInformation("Add SemaphoreSlim {0}", RightTransferToTrayAsyncName);
            _resetTcss.TryAdd(UnloadTransferToLoadTransferName, new SemaphoreSlim(0, 1));
            //_logger.LogInformation("Add SemaphoreSlim {0}", UnloadTransferToLoadTransferName);
            _resetTcss.TryAdd(LoadTransferToUnloadTransferName, new SemaphoreSlim(0, 1));
            //_logger.LogInformation("Add SemaphoreSlim {0}", LoadTransferToUnloadTransferName);

            if (_resetTcss.Count != 8)
            {
                _logger.LogError("SemaphoreSlim数量不正确");
            }
        }

        public const string TransferForwardAsyncName = nameof(TransferForwardAsync);

        public const string TransferBackwardAsyncName = nameof(TransferBackwardAsync);

        public const string LeftTrayToTransferAsyncName= nameof(LeftTrayToTransferAsync);

        public const string RightTrayToTransferAsyncName = nameof(RightTrayToTransferAsync);

        public const string LeftTransferToTrayAsyncName = nameof(LeftTransferToTrayAsync);

        public const string RightTransferToTrayAsyncName = nameof(RightTransferToTrayAsync);

        public const string UnloadTransferToLoadTransferName = nameof(UnloadTransferToLoadTransfer);

        public const string LoadTransferToUnloadTransferName = nameof(LoadTransferToUnloadTransfer);

        /// <summary>
        /// 中转模块信息
        /// </summary>
        public TransferModuleInfo TransferModuleInfo => _transferModuleInfo;

        /// <summary>
        /// 中转ID
        /// </summary>
        public long TransferId => _transferModuleInfo.TransferModuleId;

        /// <summary>
        /// 是否反转
        /// </summary>
        public bool IsReverse => _transferModuleInfo.IsReverse;

        /// <summary>
        /// 主平台调用服务
        /// </summary>
        public PlatformCallService? MainPlatformCallService => IsReverse ? _transferModuleInfo.RightPlatformCallService : _transferModuleInfo.LeftPlatformCallService;

        /// <summary>
        /// 服务资源锁
        /// </summary>
        public ReentrantLockService<IH5uTcp> LockService => _lockService;

        /// <summary>
        /// 中转模块
        /// </summary>
        public Modular? TransferModule { get; private set; }

        /// <summary>
        /// 中转模块正向Modular
        /// </summary>
        public Modular? TransferForwardModular { get; private set; }

        /// <summary>
        /// 中转模块反向Modular
        /// </summary>
        public Modular? TransferBackwardModular { get; private set; }

        /// <summary>
        /// 通道左通道组
        /// </summary>
        public ModuleChannelGroup? TransferLeftChannelGroup => _transferModuleInfo.LeftChannelGroup;
        /// <summary>
        /// 通道右通道组
        /// </summary>
        public ModuleChannelGroup? TransferRightChannelGroup => _transferModuleInfo.RightChannelGroup;

        /// <summary>
        /// 左平台抓取动作
        /// </summary>
        public Modular? LeftPlatformGrabAction { get;private set; }
        /// <summary>
        /// 右平台抓取动作
        /// </summary>
        public Modular? RightPlatformGrabAction { get; private set; }

        /// <summary>
        /// 中转状态
        /// </summary>
        public InternalTransferStatus TransferStatus
        {
            get
            {
                lock (_statusLock)
                {
                    return _status;
                }
            }
            set
            {
                lock (_statusLock)
                {
                    _status = value;
                }
            }
        }

        public void ResetAll()
        {
            foreach (var pair in _resetTcss)
            {
                var semaphore = pair.Value;
                if (semaphore.CurrentCount == 0)
                {
                    semaphore.Release();
                }
            }
        }

        public void Log(string message)
        {
            _logger.LogInformation(message);
        }

        public void LogError(string message, Exception exception)
        {
            _logger.LogInformation(message, exception);
        }

        public async Task WaitForResetAsync(string methodId, CancellationToken token)
        {
            if (_resetTcss.TryGetValue(methodId, out var semaphore))
            {
                await semaphore.WaitAsync(token);
            }
            else
            {
                throw new Exception($"未找到{methodId}对应的信号量");
            }
        }

        public override void InitializeConfiguration()
        {
            var h5UModbusTcps = WorkFlowEngine.Instance.GetPartMappers().Where(p => p.Part != null && p.As<H5uModbusTcp>() != null).ToArray();
            var h5uTcp = h5UModbusTcps.FirstOrDefault(p => p.PartId == _transferModuleInfo.TransferModuleParameter?.ModuleControllerId);
            if (h5uTcp != null)
            {
                var TransferController = h5uTcp.As<H5uModbusTcp>();
                if (TransferController != null)
                {
                    TransferModule = new Modular(TransferController);
                    TransferModule.SetModuleInfo(_transferModuleInfo.TransferModuleParameter!);
                    _logger.LogInformation("TransferModule Initialize");
                    TransferForwardModular = new Modular(TransferController);
                    TransferForwardModular.SetModuleInfo(_transferModuleInfo.TransferModuleParameter!);
                    _logger.LogInformation("TransferForwardModular Initialize");
                    if (_transferModuleInfo.TransferForwardMoveParameter != null)
                    {
                        TransferForwardModular.SetModuleFuncCodeParameter(_transferModuleInfo.TransferForwardMoveParameter);
                        _logger.LogInformation("TransferForwardModular SetModuleFuncCodeParameter");
                    }
                    TransferBackwardModular = new Modular(TransferController);
                    TransferBackwardModular.SetModuleInfo(_transferModuleInfo.TransferModuleParameter!);
                    _logger.LogInformation("TransferBackwardModular Initialize");
                    if (_transferModuleInfo.TransferBackwardMoveParameter != null)
                    {
                        TransferBackwardModular.SetModuleFuncCodeParameter(_transferModuleInfo.TransferBackwardMoveParameter);
                        _logger.LogInformation("TransferBackwardModular SetModuleFuncCodeParameter");
                    }
                }
            }
            var leftPlatformh5uTcp = h5UModbusTcps.FirstOrDefault(p => p.PartId == _transferModuleInfo.LeftPlatformInfo?.PlatformModuleInfo?.ModuleControllerId);
            if (leftPlatformh5uTcp != null)
            {
                var leftPlatformController = leftPlatformh5uTcp.As<H5uModbusTcp>();
                if (leftPlatformController != null)
                {
                    LeftPlatformGrabAction = new Modular(leftPlatformController);
                    LeftPlatformGrabAction.SetModuleInfo(_transferModuleInfo.LeftPlatformInfo!.PlatformModuleInfo!);
                    _logger.LogInformation("LeftPlatformGrabAction Initialize");
                    if (_transferModuleInfo.LeftPlatformInfo?.PlatformGrabActionParameter != null)
                    {
                        LeftPlatformGrabAction.SetModuleFuncCodeParameter(_transferModuleInfo.LeftPlatformInfo.PlatformGrabActionParameter);
                        _logger.LogInformation("LeftPlatformGrabAction SetModuleFuncCodeParameter");
                    }
                }
            }
            var rightPlatformh5uTcp = h5UModbusTcps.FirstOrDefault(p => p.PartId == _transferModuleInfo.RightPlatformInfo?.PlatformModuleInfo?.ModuleControllerId);
            if (rightPlatformh5uTcp != null)
            {
                var rightPlatformController = rightPlatformh5uTcp.As<H5uModbusTcp>();
                if (rightPlatformController != null)
                {
                    RightPlatformGrabAction = new Modular(rightPlatformController);
                    RightPlatformGrabAction.SetModuleInfo(_transferModuleInfo.RightPlatformInfo!.PlatformModuleInfo!);
                    _logger.LogInformation("RightPlatformGrabAction Initialize");
                    if (_transferModuleInfo.RightPlatformInfo?.PlatformGrabActionParameter != null)
                    {
                        RightPlatformGrabAction.SetModuleFuncCodeParameter(_transferModuleInfo.RightPlatformInfo.PlatformGrabActionParameter);
                        _logger.LogInformation("RightPlatformGrabAction SetModuleFuncCodeParameter");
                    }
                }
            }
        }
        /// <summary>
        /// 初始化中转模块
        /// </summary>
        /// <returns></returns>
        public async Task<CallStatus> InitializeTransferModule()
        {
            if (TransferModule != null)
            {
                //校验中转模块是否初始化
                if (await TransferModule.VerifyInitAsync())
                {
                    _logger.LogInformation("TransferModule VerifyInitAsync is true");
                    return CallStatus.Success();
                }
                //初始化中转模块
                else
                {
                    var result = await TransferModule.HomeAsync();
                    if (result)
                    {
                        _logger.LogInformation("TransferModule HomeAsync is true");
                        return CallStatus.Success();
                    }
                    else
                    {
                        return CallStatus.Fail(message: "中转模块初始化失败");
                    }
                }
            }
            return CallStatus.Fail(message: $"{TransferId}中转模块控制器未构造");
        }

        /// <summary>
        /// 获取中转模块状态
        /// </summary>
        /// <returns></returns>
        public InternalTransferStatus GetTransferStatus()
        {
            return TransferStatus;
        }

        public void SetTransferStatus(InternalTransferStatus status)
        {
            TransferStatus = status;
        }

        /// <summary>
        /// 是否中转的左关联平台
        /// </summary>
        /// <param name="platformId"></param>
        /// <returns></returns>
        public bool IsLeftPlatform(long platformId)
        {
            return _transferModuleInfo.LeftPlatformId == platformId;
        }

        /// <summary>
        /// 是否中转的右关联平台
        /// </summary>
        /// <param name="platformId"></param>
        /// <returns></returns>
        public bool IsRightPlatform(long platformId)
        {
            return _transferModuleInfo.RightPlatformId == platformId;
        }

        /// <summary>
        /// 中转正向移动
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<CallStatus> TransferForwardAsync(CancellationToken cancellationToken)
        {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                    if (!IsReverse)
                    {
                        using (var lockAc = _lockService.Acquire(TransferForwardModular!.Messenger, TransferId.ToString(), cancellationToken))
                        {
                            if (TransferForwardModular == null)
                                throw new Exception($"{TransferId}中转正向移动模块未构造");
                            await TransferForwardModular.WriteModuleParameterAsync();
                            await TransferForwardModular.ModuleExecuteAsync();
                            await TransferForwardModular.CheckModuleDoneAsync(cancellationToken);
                            SetTransferStatus(InternalTransferStatus.Busy);
                            return CallStatus.Success();
                        }
                    }
                    else
                    {
                        using (var lockAc = _lockService.Acquire(TransferBackwardModular!.Messenger, TransferId.ToString(), cancellationToken))
                        {
                            if (TransferBackwardModular == null)
                                throw new Exception($"{TransferId}中转反向移动模块未构造");
                            await TransferBackwardModular.WriteModuleParameterAsync();
                            await TransferBackwardModular.ModuleExecuteAsync();
                            await TransferBackwardModular.CheckModuleDoneAsync(cancellationToken);
                            SetTransferStatus(InternalTransferStatus.Busy);
                            return CallStatus.Success();
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogInformation("TransferForwardAsync Exception:{0}", ex);
                    MainPlatformCallService?.TriggerErrorOccurred();
                    await WaitForResetAsync(TransferForwardAsyncName, cancellationToken);
                }
            }
            return CallStatus.Fail(message: "Operation cancelled");
        }
        /// <summary>
        /// 中转反向移动
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<CallStatus> TransferBackwardAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested) 
            {
                try
                {
                    if (!IsReverse)
                    {
                        using (var lockAc = _lockService.Acquire(TransferBackwardModular!.Messenger, TransferId.ToString(), cancellationToken))
                        {
                            if (TransferBackwardModular == null)
                                throw new Exception($"{TransferId}中转反向移动模块未构造");
                            await TransferBackwardModular.WriteModuleParameterAsync();
                            await TransferBackwardModular.ModuleExecuteAsync();
                            await TransferBackwardModular.CheckModuleDoneAsync(cancellationToken);
                            SetTransferStatus(InternalTransferStatus.Busy);
                            return CallStatus.Success();
                        }
                    }
                    else
                    {
                        using (var lockAc = _lockService.Acquire(TransferForwardModular!.Messenger, TransferId.ToString(), cancellationToken))
                        {
                            if (TransferForwardModular == null)
                                throw new Exception($"{TransferId}中转正向移动模块未构造");
                            await TransferForwardModular.WriteModuleParameterAsync();
                            await TransferForwardModular.ModuleExecuteAsync();
                            await TransferForwardModular.CheckModuleDoneAsync(cancellationToken);
                            SetTransferStatus(InternalTransferStatus.Busy);
                            return CallStatus.Success();
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogInformation("TransferBackwardAsync Exception:{0}", ex);
                    MainPlatformCallService?.TriggerErrorOccurred();
                    await WaitForResetAsync(TransferBackwardAsyncName, cancellationToken);
                }
            }
            return CallStatus.Fail(message: "Operation cancelled");
        }

        /// <summary>
        /// 左托盘到中转
        /// </summary>
        /// <returns></returns>
        public async Task<CallStatus> LeftTrayToTransferAsync(SampleTaskInfo[] sampleTaskInfos,CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    using (var lockAc = _lockService.Acquire(LeftPlatformGrabAction!.Messenger, TransferId.ToString(), cancellationToken))
                    {
                        if (LeftPlatformGrabAction == null)
                            return CallStatus.Fail(message: "左平台抓取Modular为空");
                        if (TransferLeftChannelGroup == null)
                            return CallStatus.Fail(message: "左中转通道组为空");
                        if (_transferModuleInfo.LeftPlatformUnloadLabTrays == null
            || _transferModuleInfo.LeftPlatformUnloadLabTrays.Count == 0)
                            return CallStatus.Fail(message: "左平台下料托盘信息为空");
                        //打印中转左平台下料id
                        _logger.LogInformation("LeftTrayToTransferAsync LeftPlatformUnloadLabTrays:{0}", string.Join(",", _transferModuleInfo.LeftPlatformUnloadLabTrays.Select(p => p.LabTrayId)));
                        foreach (var sampleTaskInfo in sampleTaskInfos)
                        {
                            var leftUnloadLabTray = _transferModuleInfo.LeftPlatformUnloadLabTrays.FirstOrDefault(p => p.LabTrayId == sampleTaskInfo.TrayId) ?? throw new Exception($"未找到下料托盘{sampleTaskInfo.TrayId}");
                            var well = leftUnloadLabTray.FindWellByMaterialId(sampleTaskInfo.SamplingId);
                            var channel = TransferLeftChannelGroup.GetIdleChannel();
                            var parameter = Extensions.Extensions.GetParameter();
                            var startPos = well.Position;
                            var endPos = channel.Position;
                            parameter["D100"] = startPos.X;
                            parameter["D102"] = startPos.Y;
                            parameter["D104"] = startPos.Z;
                            parameter["D106"] = startPos.Z2;
                            parameter["D108"] = startPos.Angle;
                            parameter["D110"] = endPos.X;
                            parameter["D112"] = endPos.Y;
                            parameter["D114"] = endPos.Z;
                            parameter["D116"] = endPos.Z2;
                            parameter["D118"] = endPos.Angle;
                            parameter["D130"] = well.ClawSetting.OpenPos;
                            parameter["D132"] = well.ClawSetting.Angle;
                            parameter["D134"] = channel.ClawSetting.OpenPos;
                            parameter["D136"] = channel.ClawSetting.Angle;
                            await LeftPlatformGrabAction.WriteParameterAsync([.. parameter.Values]);
                            if (!LeftPlatformGrabAction.VerifyModuleActivityStatus())
                            {
                                await LeftPlatformGrabAction.ModuleExecuteAsync();
                            }
                            await LeftPlatformGrabAction.CheckModuleDoneAsync(cancellationToken);
                            channel.Put(well.Take());
                        }
                        SetTransferStatus(InternalTransferStatus.Busy);
                        return CallStatus.Success();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogInformation("LeftTrayToTransferAsync Exception:{0}", ex);
                    MainPlatformCallService?.TriggerErrorOccurred();
                    await WaitForResetAsync(LeftTrayToTransferAsyncName, cancellationToken);
                }
            }
            return CallStatus.Fail(message: "Operation cancelled");
        }

        /// <summary>
        /// 右托盘到中转
        /// </summary>
        /// <returns></returns>
        public async Task<CallStatus> RightTrayToTransferAsync(SampleTaskInfo[] sampleTaskInfos, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    using (var lockAc = _lockService.Acquire(RightPlatformGrabAction!.Messenger, TransferId.ToString(), cancellationToken))
                    {
                        if (RightPlatformGrabAction == null)
                            return CallStatus.Fail(message: "右平台抓取Modular为空");
                        if (TransferRightChannelGroup == null)
                            return CallStatus.Fail(message: "右中转通道组为空");
                        if (_transferModuleInfo.RightPlatformUnloadLabTrays == null
                || _transferModuleInfo.RightPlatformUnloadLabTrays.Count == 0)
                            return CallStatus.Fail(message: "右平台下料LabTray为空");
                        //打印中转右平台下料id
                        _logger.LogInformation("RightTrayToTransferAsync RightPlatformUnloadLabTrays:{0}", string.Join(",", _transferModuleInfo.RightPlatformUnloadLabTrays.Select(p => p.LabTrayId)));
                        foreach (var sampleTaskInfo in sampleTaskInfos)
                        {
                            var rightUnloadLabTray = _transferModuleInfo.RightPlatformUnloadLabTrays.FirstOrDefault(p => p.LabTrayId == sampleTaskInfo.TrayId) ?? throw new Exception($"未找到下料托盘{sampleTaskInfo.TrayId}");
                            var well = rightUnloadLabTray.FindWellByMaterialId(sampleTaskInfo.SamplingId);
                            var channel = TransferRightChannelGroup.GetIdleChannel();
                            var parameter = Extensions.Extensions.GetParameter();
                            var startPos = well.Position;
                            var endPos = channel.Position;
                            parameter["D100"] = startPos.X;
                            parameter["D102"] = startPos.Y;
                            parameter["D104"] = startPos.Z;
                            parameter["D106"] = startPos.Z2;
                            parameter["D108"] = startPos.Angle;
                            parameter["D110"] = endPos.X;
                            parameter["D112"] = endPos.Y;
                            parameter["D114"] = endPos.Z;
                            parameter["D116"] = endPos.Z2;
                            parameter["D118"] = endPos.Angle;
                            parameter["D130"] = well.ClawSetting.OpenPos;
                            parameter["D132"] = well.ClawSetting.Angle;
                            parameter["D134"] = channel.ClawSetting.OpenPos;
                            parameter["D136"] = channel.ClawSetting.Angle;
                            await RightPlatformGrabAction.WriteParameterAsync([.. parameter.Values]);
                            if (!RightPlatformGrabAction.VerifyModuleActivityStatus())
                            {
                                await RightPlatformGrabAction.ModuleExecuteAsync();
                            }
                            await RightPlatformGrabAction.CheckModuleDoneAsync(cancellationToken);
                            channel.Put(well.Take());
                        }
                        SetTransferStatus(InternalTransferStatus.Busy);
                        return CallStatus.Success();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogInformation("RightTrayToTransferAsync Exception:{0}", ex);
                    MainPlatformCallService?.TriggerErrorOccurred();
                    await WaitForResetAsync(RightTrayToTransferAsyncName, cancellationToken);
                }
            }
            return CallStatus.Fail(message: "Operation cancelled");
        }
        /// <summary>
        /// 左中转到托盘
        /// </summary>
        /// <returns></returns>
        public async Task<CallStatus> LeftTransferToTrayAsync(SampleTaskInfo[] sampleTaskInfos, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    using (var lockAc = _lockService.Acquire(LeftPlatformGrabAction!.Messenger, TransferId.ToString(), cancellationToken))
                    {
                        if (LeftPlatformGrabAction == null)
                            return CallStatus.Fail(message: "左平台抓取Modular为空");
                        if (_transferModuleInfo.LeftPlatformLoadLabTrays == null
                            || _transferModuleInfo.LeftPlatformLoadLabTrays.Count == 0)
                            return CallStatus.Fail(message: "左平台样品托盘为空");
                        if (TransferLeftChannelGroup == null)
                            return CallStatus.Fail(message: "左中转通道组为空");

                        //打印中转左平台下料id
                        _logger.LogInformation("LeftTransferToTrayAsync LeftPlatformLoadLabTrays:{0}", string.Join(",", _transferModuleInfo.LeftPlatformLoadLabTrays.Select(p => p.LabTrayId)));
                        foreach (var sampleTaskInfo in sampleTaskInfos)
                        {
                            var leftPlatformLoadLabTray = _transferModuleInfo.LeftPlatformLoadLabTrays.Where(p => p.IsEmptyOrTakenWell).FirstOrDefault();
                            if (leftPlatformLoadLabTray == null)
                                return CallStatus.Fail(message: "左平台样品托盘无法找到空闲空位");
                            var well = leftPlatformLoadLabTray.FindFirstEmptyOrTakenWell();
                            var channel = TransferLeftChannelGroup.GetIdleChannel();
                            var parameter = Extensions.Extensions.GetParameter();
                            var startPos = channel.Position;
                            var endPos = well.Position;
                            parameter["D100"] = startPos.X;
                            parameter["D102"] = startPos.Y;
                            parameter["D104"] = startPos.Z;
                            parameter["D106"] = startPos.Z2;
                            parameter["D108"] = startPos.Angle;
                            parameter["D110"] = endPos.X;
                            parameter["D112"] = endPos.Y;
                            parameter["D114"] = endPos.Z;
                            parameter["D116"] = endPos.Z2;
                            parameter["D118"] = endPos.Angle;
                            parameter["D130"] = channel.ClawSetting.OpenPos;
                            parameter["D132"] = channel.ClawSetting.Angle;
                            parameter["D134"] = well.ClawSetting.OpenPos;
                            parameter["D136"] = well.ClawSetting.Angle;
                            await LeftPlatformGrabAction.WriteParameterAsync([.. parameter.Values]);
                            if (!LeftPlatformGrabAction.VerifyModuleActivityStatus())
                            {
                                await LeftPlatformGrabAction.ModuleExecuteAsync();
                            }
                            await LeftPlatformGrabAction.CheckModuleDoneAsync(cancellationToken);
                            well.PlaceMaterial(sampleTaskInfo.SamplingId);
                        }
                        SetTransferStatus(InternalTransferStatus.Free);
                        return CallStatus.Success();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogInformation("LeftTransferToTrayAsync Exception:{0}", ex);
                    MainPlatformCallService?.TriggerErrorOccurred();
                    await WaitForResetAsync(LeftTransferToTrayAsyncName, cancellationToken);
                }
            }
            return CallStatus.Fail(message: "Operation cancelled");
        }

        /// <summary>
        /// 右中转到托盘
        /// </summary>
        /// <returns></returns>
        public async Task<CallStatus> RightTransferToTrayAsync(SampleTaskInfo[] sampleTaskInfos, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    using (var lockAc = _lockService.Acquire(RightPlatformGrabAction!.Messenger, TransferId.ToString(), cancellationToken))
                    {
                        if (RightPlatformGrabAction == null)
                            return CallStatus.Fail(message: "右平台抓取Modular为空");
                        if (_transferModuleInfo.RightPlatformLoadLabTrays == null
                    || _transferModuleInfo.RightPlatformLoadLabTrays.Count == 0)
                            return CallStatus.Fail(message: "右平台样品托盘为空");
                        if (TransferRightChannelGroup == null)
                            return CallStatus.Fail(message: "右中转通道组为空");
                        //打印中转右平台下料id
                        _logger.LogInformation("RightTransferToTrayAsync RightPlatformLoadLabTrays:{0}", string.Join(",", _transferModuleInfo.RightPlatformLoadLabTrays.Select(p => p.LabTrayId)));
                        foreach (var sampleTaskInfo in sampleTaskInfos)
                        {
                            var rightPlatformLoadLabTray = _transferModuleInfo.RightPlatformLoadLabTrays.Where(p => p.IsEmptyOrTakenWell).FirstOrDefault();
                            if (rightPlatformLoadLabTray == null)
                                return CallStatus.Fail(message: "右平台样品托盘无法找到空闲空位");
                            var well = rightPlatformLoadLabTray.FindFirstEmptyOrTakenWell();
                            var channel = TransferRightChannelGroup.GetIdleChannel();
                            var parameter = Extensions.Extensions.GetParameter();
                            var startPos = channel.Position;
                            var endPos = well.Position;
                            parameter["D100"] = startPos.X;
                            parameter["D102"] = startPos.Y;
                            parameter["D104"] = startPos.Z;
                            parameter["D106"] = startPos.Z2;
                            parameter["D108"] = startPos.Angle;
                            parameter["D110"] = endPos.X;
                            parameter["D112"] = endPos.Y;
                            parameter["D114"] = endPos.Z;
                            parameter["D116"] = endPos.Z2;
                            parameter["D118"] = endPos.Angle;
                            parameter["D130"] = channel.ClawSetting.OpenPos;
                            parameter["D132"] = channel.ClawSetting.Angle;
                            parameter["D134"] = well.ClawSetting.OpenPos;
                            parameter["D136"] = well.ClawSetting.Angle;
                            await RightPlatformGrabAction.WriteParameterAsync([.. parameter.Values]);
                            if (!RightPlatformGrabAction.VerifyModuleActivityStatus())
                            {
                                await RightPlatformGrabAction.ModuleExecuteAsync();
                            }
                            await RightPlatformGrabAction.CheckModuleDoneAsync(cancellationToken);
                            well.PlaceMaterial(sampleTaskInfo.SamplingId);
                        }
                        SetTransferStatus(InternalTransferStatus.Free);
                        return CallStatus.Success();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogInformation("RightTransferToTrayAsync Exception:{0}", ex);
                    MainPlatformCallService?.TriggerErrorOccurred();
                    await WaitForResetAsync(RightTransferToTrayAsyncName, cancellationToken);
                }
            }
            return CallStatus.Fail(message: "Operation cancelled");
        }

        /// <summary>
        /// 中转下料位 → 相邻中转上料位（横向交接）
        /// </summary>
        /// <param name="unloadTransfer"></param>
        /// <param name="loadTransfer"></param>
        /// <returns></returns>
        public static async Task<CallStatus> UnloadTransferToLoadTransfer(TransferCallService unloadTransfer, TransferCallService loadTransfer,SampleTaskInfo[] sampleTaskInfos, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    using (var lockAc = unloadTransfer.LockService.Acquire(unloadTransfer.RightPlatformGrabAction!.Messenger, unloadTransfer.TransferId.ToString(), cancellationToken))
                    {
                        if (unloadTransfer.TransferRightChannelGroup == null)
                            return CallStatus.Fail(message: "右样品通道组为空");
                        if (loadTransfer.TransferLeftChannelGroup == null)
                            return CallStatus.Fail(message: "左中转通道组为空");
                        if (unloadTransfer.RightPlatformGrabAction == null)
                            return CallStatus.Fail(message: "中转右抓取Modular为空");
                        foreach (var sampleTaskInfo in sampleTaskInfos)
                        {
                            var rightchannel = unloadTransfer.TransferRightChannelGroup.GetQChannelSlotByMaterialId(sampleTaskInfo.SamplingId);
                            var leftchannel = loadTransfer.TransferLeftChannelGroup.GetIdleChannel();
                            var parameter = Extensions.Extensions.GetParameter();
                            var startPos = rightchannel.Position;
                            var endPos = leftchannel.Position;
                            parameter["D100"] = startPos.X;
                            parameter["D102"] = startPos.Y;
                            parameter["D104"] = startPos.Z;
                            parameter["D106"] = startPos.Z2;
                            parameter["D108"] = startPos.Angle;
                            parameter["D110"] = endPos.X;
                            parameter["D112"] = endPos.Y;
                            parameter["D114"] = endPos.Z;
                            parameter["D116"] = endPos.Z2;
                            parameter["D118"] = endPos.Angle;
                            parameter["D130"] = rightchannel.ClawSetting.OpenPos;
                            parameter["D132"] = rightchannel.ClawSetting.Angle;
                            parameter["D134"] = leftchannel.ClawSetting.OpenPos;
                            parameter["D136"] = leftchannel.ClawSetting.Angle;
                            await unloadTransfer.RightPlatformGrabAction.WriteParameterAsync([.. parameter.Values]);
                            if (!unloadTransfer.RightPlatformGrabAction.VerifyModuleActivityStatus())
                            {
                                await unloadTransfer.RightPlatformGrabAction.ModuleExecuteAsync();
                            }
                            await unloadTransfer.RightPlatformGrabAction.CheckModuleDoneAsync(cancellationToken);
                            leftchannel.Put(rightchannel.Take());
                        }
                        loadTransfer.SetTransferStatus(InternalTransferStatus.Busy);
                        return CallStatus.Success();
                    }
                }
                catch (Exception ex)
                {
                    unloadTransfer.LogError("UnloadTransferToLoadTransfer Exception:{0}", ex);
                    unloadTransfer.MainPlatformCallService?.TriggerErrorOccurred();
                    await unloadTransfer.WaitForResetAsync(UnloadTransferToLoadTransferName, cancellationToken);
                }
            }
            return CallStatus.Fail(message: "Operation cancelled");
        }

        /// <summary>
        /// 中转上料位 → 相邻中转下料位（反向横向交接）
        /// </summary>
        /// <param name="loadTransfer"></param>
        /// <param name="unloadTransfer"></param>
        /// <returns></returns>
        public static async Task<CallStatus> LoadTransferToUnloadTransfer(TransferCallService loadTransfer, TransferCallService unloadTransfer, SampleTaskInfo[] sampleTaskInfos, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    using (var lockAc = loadTransfer.LockService.Acquire(loadTransfer.RightPlatformGrabAction!.Messenger, loadTransfer.TransferId.ToString(), cancellationToken))
                    {
                        if (loadTransfer.TransferLeftChannelGroup == null)
                            return CallStatus.Fail(message: "左样品通道组为空");
                        if (unloadTransfer.TransferRightChannelGroup == null)
                            return CallStatus.Fail(message: "右中转通道组为空");
                        if (loadTransfer.LeftPlatformGrabAction == null)
                            return CallStatus.Fail(message: "中转左抓取Modular为空");
                        foreach (var sampleTaskInfo in sampleTaskInfos)
                        {
                            var rightchannel = unloadTransfer.TransferRightChannelGroup.GetQChannelSlotByMaterialId(sampleTaskInfo.SamplingId);
                            var leftchannel = loadTransfer.TransferLeftChannelGroup.GetIdleChannel();
                            var parameter = Extensions.Extensions.GetParameter();
                            var startPos = leftchannel.Position;
                            var endPos = rightchannel.Position;
                            parameter["D100"] = startPos.X;
                            parameter["D102"] = startPos.Y;
                            parameter["D104"] = startPos.Z;
                            parameter["D106"] = startPos.Z2;
                            parameter["D108"] = startPos.Angle;
                            parameter["D110"] = endPos.X;
                            parameter["D112"] = endPos.Y;
                            parameter["D114"] = endPos.Z;
                            parameter["D116"] = endPos.Z2;
                            parameter["D118"] = endPos.Angle;
                            parameter["D130"] = leftchannel.ClawSetting.OpenPos;
                            parameter["D132"] = leftchannel.ClawSetting.Angle;
                            parameter["D134"] = rightchannel.ClawSetting.OpenPos;
                            parameter["D136"] = rightchannel.ClawSetting.Angle;
                            await loadTransfer.LeftPlatformGrabAction.WriteParameterAsync([.. parameter.Values]);
                            if (!loadTransfer.LeftPlatformGrabAction.VerifyModuleActivityStatus())
                            {
                                await loadTransfer.LeftPlatformGrabAction.ModuleExecuteAsync();
                            }
                            await loadTransfer.LeftPlatformGrabAction.CheckModuleDoneAsync(cancellationToken);
                            rightchannel.Put(leftchannel.Take());
                        }
                        unloadTransfer.SetTransferStatus(InternalTransferStatus.Busy);
                        return CallStatus.Success();
                    }
                }
                catch (Exception ex)
                {
                    loadTransfer.LogError("LoadTransferToUnloadTransfer Exception:{0}", ex);
                    loadTransfer.MainPlatformCallService?.TriggerErrorOccurred();
                    await loadTransfer.WaitForResetAsync(LoadTransferToUnloadTransferName, cancellationToken);
                }
            }
            return CallStatus.Fail(message: "Operation cancelled");
        }

    }
}
