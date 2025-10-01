using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Logging;
using QStandaedPlatform.Engine.Common.Common;
using QStandaedPlatform.Engine.Common.Common.Metadatas;
using QStandaedPlatform.Engine.Components;
using QStandaedPlatform.Engine.Laboratory;
using QStandaedPlatform.Engine.Laboratory.Documents;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Foundation.Modules.NormalModules;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace System.Foundation.Modules.Arms
{

    /// <summary>
    /// 样品架扫码上料模块
    /// </summary>
    [DisplayName(TrayScanModArmToolName)]
    public class SampleTrayScanModArmTool : SyncInputArmModuleToolBase, ILabTrayService
    {
        public const string TrayScanModArmToolName = "样品扫码上料";

        private const string MaterialId_GET = "材料ID";

        private const string START_Scan_PIN = "输入搬运扫码位置";

        private const string OUTPUT_SAMPLEID_PIN = "输出样品ID";

        private const string OUTPUT_SIGNAL_PIN = "输出完成信号";

        private readonly ReentrantLockService<IH5uTcp> _lockService;
        private readonly ArmService _armService;
        private readonly SampleService _sampleService;
        public SampleTrayScanModArmTool(ReentrantLockService<IH5uTcp> lockService, ArmService armService, SampleService sampleService)
        {
            _lockService = lockService;
            _armService = armService;
            _sampleService = sampleService;
        }


        public override string DefineName => TrayScanModArmToolName;

        public override bool InitDataContext()
        {
            DataContext = new TrayModArmData();
            return true;
        }
        public override bool InitPins()
        {
            InsetPin(MaterialId_GET, this, typeof(QInt64), PinType.Input);
            InsetPin(START_Scan_PIN, this, typeof(QPosition), PinType.Input);
            InsetPin(OUTPUT_SAMPLEID_PIN, this, typeof(QInt64), PinType.Output);
            InsetPin(OUTPUT_SIGNAL_PIN, this, typeof(QData), PinType.Output);
            return true;
        }

        public override async Task<bool> ExexuteAsync(Dictionary<PinInfo, QData> pinDatas, ToolExecutionContext toolContext)
        {
            using (var lockAc = _lockService.Acquire(GetModular().Messenger, this.UniqueId.ToString(), RequestCancelToken))
            {
                var materialId = pinDatas.FirstOrDefault(p => p.Key.Name == MaterialId_GET).Value as QInt64;
                var scan = pinDatas.FirstOrDefault(p => p.Key.Name == START_Scan_PIN).Value as QPosition;
                var well = SampleLabTray.FindWellByMaterialIdLoadOnlyAllowNull(materialId) ?? SampleLabTray2.FindWellByMaterialIdLoadOnly(materialId);
                var claw = new ClawModel
                {
                    FromAngle = well.ClawSetting.Angle,
                    FromOpenPos = well.ClawSetting.OpenPos,
                    ToAngle = ModuleChannelGroup.ClawSetting.Angle,
                    ToOpenPos = ModuleChannelGroup.ClawSetting.OpenPos
                };
                var qChannelSlot = ModuleChannelGroup.GetIdleChannel();
                var sampleTaskInfo = _sampleService.GetSample(materialId);
                var sanCode = await _armService.SampleArmWithScanTransport(this.UniqueId, sampleTaskInfo, (QPosition)well.Position.Clone(), (QPosition)scan.Clone(), (QPosition)qChannelSlot.Position.Clone(), claw, Context<TrayModArmData>(), GetModular(), RequestCancelToken);
                qChannelSlot.Put(well.Take());
                SampleLabTray.SetQrCode(well,sanCode);
                SampleLabTray2.SetQrCode(well, sanCode);
                SendToPin(OUTPUT_SIGNAL_PIN, new QData());
                SendToPin(OUTPUT_SAMPLEID_PIN, new QInt64(sampleTaskInfo.SamplingId));
                return true;
            }
        }


        /// <summary>
        /// 样品托盘1
        /// </summary>
        [RefParameter<LabTrayTable>]
        public LabTray SampleLabTray { set; get; }

        /// <summary>
        /// 样品托盘2
        /// </summary>
        [RefParameter<LabTrayTable>]
        public LabTray SampleLabTray2 { set; get; }


        [RefParameter<ModuleChannelGroupTable>]
        public ModuleChannelGroup ModuleChannelGroup { set; get; }

        public List<LabTray> LabTrays => [SampleLabTray, SampleLabTray2];
    }

    /// <summary>
    /// 样品架上料模块
    /// </summary>
    [DisplayName(TrayToModArmToolName)]
    public class SampleTrayToModArmTool : ArmModuleToolBase, ILabTrayService
    {
        private const string TrayToModArmToolName = "样品上料";

        private const string MaterialId_GET = "材料ID";
        private const string OUTPUT_SAMPLE_ID_PIN = "输出样品ID";
        private const string OUTPUT_SIGNAL_PIN = "输出完成信号";

        public override string DefineName => TrayToModArmToolName;

        private readonly ReentrantLockService<IH5uTcp> _lockService;
        private readonly ArmService _ArmService;
        private readonly SampleService _sampleService;
        public SampleTrayToModArmTool(ReentrantLockService<IH5uTcp> lockService
            , ArmService armService
            ,SampleService sampleService)
        {
            _lockService = lockService;
            _ArmService = armService;
            _sampleService = sampleService;
        }
        public override bool InitPins()
        {
            InsetPin(MaterialId_GET, this, typeof(QInt64), PinType.Input);
            InsetPin(OUTPUT_SAMPLE_ID_PIN, this, typeof(QInt64), PinType.Output);
            InsetPin(OUTPUT_SIGNAL_PIN, this, typeof(QData), PinType.Output);
            return true;
        }
        public override bool InitDataContext()
        {
            DataContext = new TrayModArmData();
            return true;
        }
        public override async Task<bool> ExecuteAsync(PinInfo pinInfo, QData pinData, ToolExecutionContext toolContext)
        {
            if (pinInfo.Name == MaterialId_GET)
            {
                using (var lockAc = _lockService.Acquire(GetModular().Messenger, this.UniqueId.ToString(), RequestCancelToken))
                {
                    var materialId = pinData as QInt64;
                    var well = SampleLabTray.FindWellByMaterialIdAllowNull(materialId) ?? SampleLabTray2.FindWellByMaterialId(materialId);
                   
                    var qChannelSlot = ModuleChannelGroup.GetIdleChannel();
                    var sampleTaskInfo =_sampleService.GetSample(materialId);
                    var claw = new ClawModel
                    {
                        FromAngle = well.ClawSetting.Angle,
                        FromOpenPos = well.ClawSetting.OpenPos,
                        ToAngle = ModuleChannelGroup.ClawSetting.Angle,
                        ToOpenPos = ModuleChannelGroup.ClawSetting.OpenPos
                    };
                    await _ArmService.SampleArmTransport(UniqueId, sampleTaskInfo, (QPosition)well.Position.Clone(), (QPosition)qChannelSlot.Position.Clone(), claw, Context<TrayModArmData>(), GetModular(), RequestCancelToken);
                    qChannelSlot.Put(well.Take());
                    SendToPin(OUTPUT_SAMPLE_ID_PIN, materialId);
                    SendToPin(OUTPUT_SIGNAL_PIN, new QData());
                    return true;
                }
            }
            throw new Exception("未找到对应的输入");
        }

        /// <summary>
        /// 样品托盘1
        /// </summary>
        [RefParameter<LabTrayTable>]
        public LabTray SampleLabTray { set; get; }

        //样品托盘2
        [RefParameter<LabTrayTable>]

        public LabTray SampleLabTray2 { set; get; }

        [RefParameter<ModuleChannelGroupTable>]
        public ModuleChannelGroup ModuleChannelGroup { set; get; }

        public List<LabTray> LabTrays => [SampleLabTray, SampleLabTray2];
    }

    public class BatchTrayModArmData : TrayModArmData
    {
        [DisplayName("批量数")]
        public int BatchCount { set; get; } = 2;

        [DisplayName("起始Index")]
        public int BeginIndex { set; get; } = 0;

        [DisplayName("正序读取")]
        public bool OrderRead { set; get; } = true;
    }


    /// <summary>
    /// 批耗材架上料模块
    /// </summary>
    [DisplayName(BatchMaterialTrayModArmToolName)]
    public class BatchMaterialTrayModArmTool : ArmModuleToolBase, ILabTrayService
    {
        public const string BatchMaterialTrayModArmToolName = "批耗材上料";
        private const string START_LOADING_SIGNAL = "开始上料信号";
        //上料信号 有耗材就行
        private const string START_LOADING_SIGNAL_WITH_MATERIAL = "有耗材上料信号";
        private const string OUTPUT_SIGNAL_PIN = "输出完成信号";
        public override string DefineName => BatchMaterialTrayModArmToolName;

        private readonly BatchProcessor batchProcessor = new();
        private readonly BatchProcessor batchProcessor_withMaterial = new();

        private readonly ReentrantLockService<IH5uTcp> _lockService;
        private readonly ArmService _armService;
        public BatchMaterialTrayModArmTool(ReentrantLockService<IH5uTcp> lockService, ArmService armService)
        {
            _lockService = lockService;
            _armService = armService;
        }

        public override bool InitPins()
        {
            InsetPin(START_LOADING_SIGNAL, this, typeof(QData), PinType.Input);
            InsetPin(OUTPUT_SIGNAL_PIN, this, typeof(QData), PinType.Output);
            return true;
        }
        public override bool InitDataContext()
        {
            DataContext = new BatchTrayModArmData();
            return true;
        }

        public bool IsAsc => Context<BatchTrayModArmData>().OrderRead;


        public override Task<bool> ClearEphemeralDataAsync()
        {
            batchProcessor.Reset();
            batchProcessor_withMaterial.Reset();
            return Task.FromResult(true);
        }

        public override bool InitEnd()
        {
            batchProcessor.SetProcessStep(async (batch, context) =>
            {
                try
                {
                    {
                        var materialLabTray = context["MaterialLabTray"] as LabTray;
                        var materialLabTray2 = context["MaterialLabTray2"] as LabTray;
                        var moduleChannelGroup = context["ModuleChannelGroup"] as ModuleChannelGroup;
                        var well = IsAsc ? materialLabTray.FindFirstLoadedWellWithMaterialAllowNull() ?? materialLabTray2.FindFirstLoadWellWithMaterial() : materialLabTray.FindFirstLoadWellWithMaterialBackAllowNull() ?? materialLabTray2.FindFirstLoadWellWithMaterialBack();
                        var claw = new ClawModel
                        {
                            FromOpenPos = well.ClawSetting.OpenPos,
                            FromAngle = well.ClawSetting.Angle,
                            ToAngle = moduleChannelGroup.ClawSetting.Angle,
                            ToOpenPos = moduleChannelGroup.ClawSetting.OpenPos
                        };
                        var qChannelSlot = moduleChannelGroup.GetIdleChannel();
                        await _armService.ArmTransport((QPosition)well.Position.Clone(), (QPosition)qChannelSlot.Position.Clone(), claw, Context<BatchTrayModArmData>(), GetModular(), RequestCancelToken);
                        qChannelSlot.Put(well.Take());
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogInformation($"{DisplayName}发生异常{ex}");
                    throw;
                }
            });
            batchProcessor_withMaterial.SetProcessStep(async (batch, context) => 
            {
                try
                {
                    var materialLabTray = context["MaterialLabTray"] as LabTray;
                    var materialLabTray2 = context["MaterialLabTray2"] as LabTray;
                    var moduleChannelGroup = context["ModuleChannelGroup"] as ModuleChannelGroup;
                    var well = IsAsc ? materialLabTray.FindFirstLoadedWellWithMaterialAllowNull() ?? materialLabTray2.FindFirstLoadWellWithMaterial() : materialLabTray.FindFirstLoadWellWithMaterialBackAllowNull() ?? materialLabTray2.FindFirstLoadWellWithMaterialBack();
                    var claw = new ClawModel
                    {
                        FromOpenPos = well.ClawSetting.OpenPos,
                        FromAngle = well.ClawSetting.Angle,
                        ToAngle = moduleChannelGroup.ClawSetting.Angle,
                        ToOpenPos = moduleChannelGroup.ClawSetting.OpenPos
                    };
                    var qChannelSlot = moduleChannelGroup.GetIdleChannel();
                    await _armService.ArmTransport((QPosition)well.Position.Clone(), (QPosition)qChannelSlot.Position.Clone(), claw, Context<BatchTrayModArmData>(), GetModular(), RequestCancelToken);
                    qChannelSlot.Put(well.Take());
                }
                catch (Exception ex)
                {
                    Logger.LogInformation($"{DisplayName}发生异常{ex}");
                    throw;
                }
            });
            return true;
        }
        public override async Task<bool> ExecuteAsync(PinInfo pinInfo, QData pinData, ToolExecutionContext toolContext)
        {
            using (var lockAc = _lockService.Acquire(GetModular().Messenger, this.UniqueId.ToString(), RequestCancelToken))
            {
                if (await batchProcessor.ProcessBatchAsync(BatchCount, new Dictionary<string, object>
            {
                { "MaterialLabTray",MaterialLabTray },
                { "ModuleChannelGroup",ModuleChannelGroup },
                { "MaterialLabTray2",MaterialLabTray2 }
            }, RequestCancelToken))
                {
                    SendToPin(OUTPUT_SIGNAL_PIN, new QData());
                    return true;
                }
            }
            throw new Exception("批耗材架上料失败");
        }

        public int BatchCount => Context<BatchTrayModArmData>().BatchCount;

        /// <summary>
        /// 耗材托盘1
        /// </summary>
        /// 
        [RefParameter<LabTrayTable>]
        public LabTray MaterialLabTray { set; get; }

        /// <summary>
        /// 耗材托盘2
        /// </summary>
        [RefParameter<LabTrayTable>]
        public LabTray MaterialLabTray2 { set; get; }

        [RefParameter<ModuleChannelGroupTable>]
        public ModuleChannelGroup ModuleChannelGroup { set; get; }

        public List<LabTray> LabTrays => [MaterialLabTray, MaterialLabTray2];
    }

    

    /// <summary>
    /// 样品模块下料托盘
    /// </summary>
    [DisplayName(BatchSampleModToTrayArmToolName)]
    public class BatchSampleModToTrayArmTool : ArmModuleToolBase, ILabTrayService
    {
        public const string BatchSampleModToTrayArmToolName = "批样品模块到托盘";

        private const string START_ARM_TONEW_SIGNAL = "搬运到新托盘信号";
        private const string START_ARM_SIGNAL = "开始搬回信号";
        private const string OUTPUT_SIGNAL_PIN = "输出完成信号";
        //输出样品ID
        private const string OUTPUT_SAMPLE_ID_PIN = "输出样品ID";

        public override string DefineName => BatchSampleModToTrayArmToolName;

        private readonly BatchProcessor batchProcessor = new();
        private readonly BatchProcessor batchProcessor_new = new();

        private readonly ReentrantLockService<IH5uTcp> _lockService;
        private readonly ArmService _armService;
        private readonly SampleService _sampleService;
        public BatchSampleModToTrayArmTool(ReentrantLockService<IH5uTcp> lockService, ArmService armService, SampleService sampleService)
        {
            _lockService = lockService;
            _armService = armService;
            _sampleService = sampleService;
        }
        public override bool InitPins()
        {
            InsetPin(START_ARM_SIGNAL, this, typeof(QData), PinType.Input);
            InsetPin(START_ARM_TONEW_SIGNAL, this, typeof(QData), PinType.Input);
            InsetPin(OUTPUT_SIGNAL_PIN, this, typeof(QData), PinType.Output);
            InsetPin(OUTPUT_SAMPLE_ID_PIN, this, typeof(QInt64), PinType.Output);
            return true;
        }
        public override bool InitDataContext()
        {
            DataContext = new BatchTrayModArmData();
            return true;
        }
        public int Batch => Context<BatchTrayModArmData>().BatchCount;

        public override bool InitEnd()
        {
            batchProcessor.SetProcessStep(async (batch, context) =>
            {
                {
                    var sampleModuleChannelGroup = context["SampleModuleChannelGroup"] as ModuleChannelGroup;
                    var labTray = context["LabTray"] as LabTray;
                    var labTray2 = context["LabTray2"] as LabTray;
                    var qChannelSlot = sampleModuleChannelGroup.GetWorkingChannel();
                    var well = labTray.FindTakenWellAllowNull(qChannelSlot.Labware) ?? labTray2.FindTakenWell(qChannelSlot.Labware);
                    var claw = new ClawModel
                    {
                        ToAngle = sampleModuleChannelGroup.ClawSetting.Angle,
                        ToOpenPos = sampleModuleChannelGroup.ClawSetting.OpenPos,
                        FromAngle = well.ClawSetting.Angle,
                        FromOpenPos = well.ClawSetting.OpenPos
                    };
                    var materialId = qChannelSlot.Labware.Material.MaterialNo;
                    var sample = _sampleService.GetSample(materialId);
                    await _armService.SampleArmTransport(UniqueId, sample, (QPosition)qChannelSlot.Position.Clone(), (QPosition)well.Position.Clone(), claw, Context<BatchTrayModArmData>(), GetModular(), RequestCancelToken);
                    well.Return(qChannelSlot.Take());
                    SendToPin(OUTPUT_SAMPLE_ID_PIN, (QInt64)sample.SamplingId);
                }
            });
            batchProcessor_new.SetProcessStep(async (batch, context) =>
            {
                {
                    var sampleModuleChannelGroup = context["SampleModuleChannelGroup"] as ModuleChannelGroup;
                    var labTray = context["LabTray"] as LabTray;
                    var labTray2 = context["LabTray2"] as LabTray;
                    var qChannelSlot = sampleModuleChannelGroup.GetWorkingChannel();
                    var well = labTray.FindFirstEmptyOrTakenWellAllowNull() ?? labTray2.FindFirstEmptyOrTakenWell();
                    var claw = new ClawModel
                    {
                        ToAngle = sampleModuleChannelGroup.ClawSetting.Angle,
                        ToOpenPos = sampleModuleChannelGroup.ClawSetting.OpenPos,
                        FromAngle = well.ClawSetting.Angle,
                        FromOpenPos = well.ClawSetting.OpenPos
                    };
                    var materialId = qChannelSlot.Labware.Material.MaterialNo;
                    var sample = _sampleService.GetSample(materialId);
                    await _armService.SampleArmTransport(UniqueId, sample, (QPosition)qChannelSlot.Position.Clone(), (QPosition)well.Position.Clone(), claw, Context<BatchTrayModArmData>(), GetModular(), RequestCancelToken);
                    well.PlaceMaterial(qChannelSlot.Take());
                    SendToPin(OUTPUT_SAMPLE_ID_PIN, (QInt64)sample.SamplingId);
                }
            });
            return true;
        }
        public override async Task<bool> ExecuteAsync(PinInfo pinInfo, QData pinData, ToolExecutionContext toolContext)
        {
            if (pinInfo.Name == START_ARM_SIGNAL)
            {
                using (var lockAc = _lockService.Acquire(GetModular().Messenger, this.UniqueId.ToString(), RequestCancelToken))
                {
                    if (await batchProcessor.ProcessBatchAsync(Batch, new Dictionary<string, object>
                {
                    { "SampleModuleChannelGroup", SampleModuleChannelGroup },
                    { "LabTray", LabTray },
                    { "LabTray2", LabTray2 }
                }, RequestCancelToken))
                    {
                        SendToPin(OUTPUT_SIGNAL_PIN, new QData());
                        return true;
                    }
                }
            }
            else if (pinInfo.Name == START_ARM_TONEW_SIGNAL)
            {
                using (var lockAc = _lockService.Acquire(GetModular().Messenger, this.UniqueId.ToString(), RequestCancelToken))
                {
                    if (await batchProcessor_new.ProcessBatchAsync(Batch, new Dictionary<string, object>
                {
                    { "SampleModuleChannelGroup", SampleModuleChannelGroup },
                    { "LabTray", LabTray },
                    { "LabTray2", LabTray2 }
                }, RequestCancelToken))
                    {
                        SendToPin(OUTPUT_SIGNAL_PIN, new QData());
                        return true;
                    }
                }
            }
            throw new ArgumentException("PinInfo Name is not match");
        }
        /// <summary>
        /// 托盘1
        /// </summary>
        [RefParameter<LabTrayTable>]
        public LabTray LabTray { set; get; }

        //托盘2
        [RefParameter<LabTrayTable>]
        public LabTray LabTray2 { set; get; }


        [RefParameter<ModuleChannelGroupTable>]
        public ModuleChannelGroup SampleModuleChannelGroup { set; get; }

        public List<LabTray> LabTrays => [LabTray, LabTray2];
    }

    /// <summary>
    /// 耗材模块下料托盘
    /// </summary>
    [DisplayName(BatchMaterialModToTrayArmToolName)]
    public class BatchMaterialModToTrayArmTool : ArmModuleToolBase, ILabTrayService
    {
        public const string BatchMaterialModToTrayArmToolName = "批耗材模块到托盘";
        private const string START_ARM_SIGNAL = "开始搬运信号";
        private const string START_ARM_TONEW_SIGNAL = "搬运到新托盘信号";
        private const string OUTPUT_SIGNAL_PIN = "输出完成信号";

        public override string DefineName => BatchMaterialModToTrayArmToolName;

        private readonly BatchProcessor batchProcessor = new();
        private readonly BatchProcessor batchProcessor_new = new();

        private readonly ReentrantLockService<IH5uTcp> _lockService;
        private readonly ArmService _armService;
        public BatchMaterialModToTrayArmTool(ReentrantLockService<IH5uTcp> lockService, ArmService armService)
        {
            _lockService = lockService;
            _armService = armService;
        }
        public override bool InitPins()
        {
            InsetPin(START_ARM_SIGNAL, this, typeof(QData), PinType.Input);
            InsetPin(START_ARM_TONEW_SIGNAL, this, typeof(QData), PinType.Input);
            InsetPin(OUTPUT_SIGNAL_PIN, this, typeof(QData), PinType.Output);
            return true;
        }
        [RefParameter<ModuleChannelGroupTable>]
        public ModuleChannelGroup MaterialModuleChannelGroup { set; get; }

        /// <summary>
        /// 托盘1
        /// </summary>
        [RefParameter<LabTrayTable>]
        public LabTray LabTray { set; get; }

        /// <summary>
        /// 托盘2
        /// </summary>
        [RefParameter<LabTrayTable>]
        public LabTray LabTray2 { set; get; }

        public override bool InitDataContext()
        {
            DataContext = new BatchTrayModArmData();
            return true;
        }
        public int Batch => Context<BatchTrayModArmData>().BatchCount;

        public List<LabTray> LabTrays => [LabTray, LabTray2];

        public override bool InitEnd()
        {
            batchProcessor.SetProcessStep(async (batch, context) =>
            {
                {
                    var materialModuleChannelGroup = context["MaterialModuleChannelGroup"] as ModuleChannelGroup;
                    var labTray = context["LabTray"] as LabTray;
                    var labTray2 = context["LabTray2"] as LabTray;
                    var qChannelSlot = materialModuleChannelGroup.GetWorkingChannel();
                    var well = labTray.FindTakenWellAllowNull(qChannelSlot.Labware) ?? labTray2.FindTakenWell(qChannelSlot.Labware);
                    var claw = new ClawModel
                    {
                        ToOpenPos = materialModuleChannelGroup.ClawSetting.OpenPos,
                        ToAngle = materialModuleChannelGroup.ClawSetting.Angle,
                        FromOpenPos = well.ClawSetting.OpenPos,
                        FromAngle = well.ClawSetting.Angle
                    };
                    await _armService.ArmTransport((QPosition)qChannelSlot.Position.Clone(), (QPosition)well.Position.Clone(), claw,Context<BatchTrayModArmData>(), GetModular(), RequestCancelToken);
                    well.Return(qChannelSlot.Take());
                }
            });
            batchProcessor_new.SetProcessStep(async (batch, context) =>
            {
                {
                    var materialModuleChannelGroup = context["MaterialModuleChannelGroup"] as ModuleChannelGroup;
                    var labTray = context["LabTray"] as LabTray;
                    var labTray2 = context["LabTray2"] as LabTray;
                    var qChannelSlot = materialModuleChannelGroup.GetWorkingChannel();
                    var well = labTray.FindFirstEmptyOrTakenWellAllowNull() ?? labTray2.FindFirstEmptyOrTakenWell();
                    var claw = new ClawModel
                    {
                        FromAngle = qChannelSlot.ClawSetting.Angle,
                        FromOpenPos =   qChannelSlot.ClawSetting.OpenPos,
                        ToAngle = well.ClawSetting.Angle,
                        ToOpenPos = well.ClawSetting.OpenPos
                    };
                    await _armService.ArmTransport((QPosition)qChannelSlot.Position.Clone(), (QPosition)well.Position.Clone(), claw, Context<BatchTrayModArmData>(), GetModular(), RequestCancelToken);
                    well.PlaceMaterial(qChannelSlot.Take());
                }
            });
            return true;
        }

        public override async Task<bool> ExecuteAsync(PinInfo pinInfo, QData pinData, ToolExecutionContext toolContext)
        {
            if (pinInfo.Name == START_ARM_SIGNAL)
            {
                using (var lockAc = _lockService.Acquire(GetModular().Messenger, this.UniqueId.ToString(), RequestCancelToken))
                {
                    if (await batchProcessor.ProcessBatchAsync(Batch, new Dictionary<string, object>
                {
                    { "MaterialModuleChannelGroup", MaterialModuleChannelGroup },
                    { "LabTray", LabTray },
                    { "LabTray2", LabTray2 }
                }, RequestCancelToken))
                    {
                        SendToPin(OUTPUT_SIGNAL_PIN, new QData());
                        return true;
                    }
                }
            }
            else if (pinInfo.Name == START_ARM_TONEW_SIGNAL)
            {
                using (var lockAc = _lockService.Acquire(GetModular().Messenger, this.UniqueId.ToString(), RequestCancelToken))
                {
                    if (await batchProcessor_new.ProcessBatchAsync(Batch, new Dictionary<string, object>
                {
                    { "MaterialModuleChannelGroup", MaterialModuleChannelGroup },
                    { "LabTray", LabTray },
                    { "LabTray2", LabTray2 }
                }, RequestCancelToken))
                    {
                        SendToPin(OUTPUT_SIGNAL_PIN, new QData());
                        return true;
                    }
                }
            }
            throw new ArgumentException("PinInfo Name is not match");
        }
    }

    //模块下料到废料口

    [DisplayName(ModToTrashPortArmToolName)]
    public class ModToTrashPortArmTool : ArmModuleToolBase
    {
        public const string ModToTrashPortArmToolName = "从模块下料到废料口";
        private const string START_ARM_SIGNAL = "开始搬运信号";
        private const string OUTPUT_SIGNAL_PIN = "输出完成信号";
        public override string DefineName => ModToTrashPortArmToolName;

        private readonly BatchProcessor batchProcessor = new();
        private readonly ReentrantLockService<IH5uTcp> _lockService;
        private readonly ArmService _armService;
        public ModToTrashPortArmTool(ReentrantLockService<IH5uTcp> lockService, ArmService armService)
        {
            _lockService = lockService;
            _armService = armService;
        }
        public override bool InitPins()
        {
            InsetPin(START_ARM_SIGNAL, this, typeof(QData), PinType.Input);
            InsetPin(OUTPUT_SIGNAL_PIN, this, typeof(QData), PinType.Output);
            return true;
        }

        public override bool InitDataContext()
        {
            DataContext = new BatchTrayModArmData();
            return true;
        }

        public int Batch => Context<BatchTrayModArmData>().BatchCount;

        public bool IsAsc => Context<BatchTrayModArmData>().OrderRead;

        public override bool InitEnd()
        {
            batchProcessor.SetProcessStep(async (batch, context) =>
            {
                {
                    var moduleChannelGroup = context["ModuleChannelGroup"] as ModuleChannelGroup;
                    var trashChannelGroup = context["TrashChannelGroup"] as ModuleChannelGroup;
                    var qChannelSlot = IsAsc ? moduleChannelGroup.GetWorkingChannel() : moduleChannelGroup.GetWorkingChannelReverse();
                    var trashChannel = trashChannelGroup.GetChannelByIndex(1);

                    var claw = new ClawModel
                    {
                        ToOpenPos = trashChannelGroup.ClawSetting.OpenPos,
                        ToAngle = trashChannelGroup.ClawSetting.Angle,
                        FromOpenPos = moduleChannelGroup.ClawSetting.OpenPos,
                        FromAngle = moduleChannelGroup.ClawSetting.Angle
                    };

                    await _armService.ArmTransport((QPosition)qChannelSlot.Position.Clone(), (QPosition)trashChannel.Position.Clone(),null, Context<BatchTrayModArmData>(), GetModular(), RequestCancelToken);
                }
            });
            return true;
        }

        public override async Task<bool> ExecuteAsync(PinInfo pinInfo, QData pinData, ToolExecutionContext toolContext)
        {
            if (pinInfo.Name == START_ARM_SIGNAL)
            {
                using (var lockAc = _lockService.Acquire(GetModular().Messenger, this.UniqueId.ToString(), RequestCancelToken))
                {
                    if (await batchProcessor.ProcessBatchAsync(Batch, new Dictionary<string, object>
                {
                    { "ModuleChannelGroup", ModuleChannelGroup },
                    { "TrashChannelGroup", TrashChannelGroup }
                }, RequestCancelToken))
                    {
                        SendToPin(OUTPUT_SIGNAL_PIN, new QData());
                        return true;
                    }
                }
            }
            throw new ArgumentException("PinInfo Name is not match");
        }

        [RefParameter<ModuleChannelGroupTable>]
        public ModuleChannelGroup ModuleChannelGroup { get; set; }


        [RefParameter<ModuleChannelGroupTable>]
        public ModuleChannelGroup TrashChannelGroup { get; set; }

    }

    /// <summary>
    /// 模块下料到废料框
    /// </summary>
    [DisplayName(ModToTrashBucketArmToolName)]
    public class ModToTrashBucketArmTool : ArmModuleToolBase
    {
        public const string ModToTrashBucketArmToolName = "从模块下料到废料框/托盘";
        private const string START_ARM_SIGNAL = "开始搬运信号";
        private const string OUTPUT_SIGNAL_PIN = "输出完成信号";
        public override string DefineName => ModToTrashBucketArmToolName;

        private readonly BatchProcessor batchProcessor = new();
        private readonly ReentrantLockService<IH5uTcp> _lockService;
        private readonly ArmService _armService;
        public ModToTrashBucketArmTool(ReentrantLockService<IH5uTcp> lockService, ArmService armService)
        {
            _lockService = lockService;
            _armService = armService;
        }
        public override bool InitPins()
        {
            InsetPin(START_ARM_SIGNAL, this, typeof(QData), PinType.Input);
            InsetPin(OUTPUT_SIGNAL_PIN, this, typeof(QData), PinType.Output);
            return true;
        }

        public override bool InitDataContext()
        {
            DataContext = new BatchTrayModArmData();
            return true;
        }

        public int Batch => Context<BatchTrayModArmData>().BatchCount;

        public bool IsAsc => Context<BatchTrayModArmData>().OrderRead;

        public override Task<bool> ClearEphemeralDataAsync()
        {
            batchProcessor.Reset();
            return Task.FromResult(true);
        }
        public override bool InitEnd()
        {
            batchProcessor.SetProcessStep(async (batch, context) =>
            {
                {
                    var moduleChannelGroup = context["ModuleChannelGroup"] as ModuleChannelGroup;
                    var trashBucket = context["TrashBucket"] as LabTray;
                  
                    var qChannelSlot = IsAsc ? moduleChannelGroup.GetWorkingChannel() : moduleChannelGroup.GetWorkingChannelReverse();
                    var trashChannel = trashBucket.FindFirstEmptyWell();
                    var claw = new ClawModel
                    {
                        FromOpenPos = moduleChannelGroup.ClawSetting.OpenPos,
                        FromAngle = moduleChannelGroup.ClawSetting.Angle,
                        ToAngle = trashChannel.ClawSetting.Angle,
                        ToOpenPos = trashChannel.ClawSetting.OpenPos
                    };
                    await _armService.ArmTransport((QPosition)qChannelSlot.Position.Clone(), (QPosition)trashChannel.Position.Clone(), claw, Context<BatchTrayModArmData>(), GetModular(), RequestCancelToken);
                    trashChannel.PlaceMaterial(qChannelSlot.Labware);
                }
            });
            return true;
        }

        public override async Task<bool> ExecuteAsync(PinInfo pinInfo, QData pinData, ToolExecutionContext toolContext)
        {
            if (pinInfo.Name == START_ARM_SIGNAL)
            {
                using (var lockAc = _lockService.Acquire(GetModular().Messenger, this.UniqueId.ToString(), RequestCancelToken))
                {
                    if (await batchProcessor.ProcessBatchAsync(Batch, new Dictionary<string, object>
                {
                    { "ModuleChannelGroup", ModuleChannelGroup },
                    { "TrashBucket", TrashBucket }
                }, RequestCancelToken))
                    {
                        SendToPin(OUTPUT_SIGNAL_PIN, new QData());
                        return true;
                    }
                }
            }
            throw new ArgumentException("PinInfo Name is not match");
        }

        [RefParameter<ModuleChannelGroupTable>]
        public ModuleChannelGroup ModuleChannelGroup { get; set; }

        /// <summary>
        /// 废料框/托盘
        /// </summary>
        [RefParameter<LabTrayTable>]
        public LabTray TrashBucket { get; set; }

    }


    /// <summary>
    /// 批样品模块到模块
    /// </summary>
    [DisplayName(BatchSampleModToModArmToolName)]
    public class BatchSampleModToModArmTool : ArmModuleToolBase
    {
        public const string BatchSampleModToModArmToolName = "批样品模块到模块";

        private const string START_ARM_SIGNAL = "开始搬运信号";
        private const string OUTPUT_SIGNAL_PIN = "输出完成信号";

        public override string DefineName => BatchSampleModToModArmToolName;

        private readonly BatchProcessor batchProcessor = new();

        private readonly ReentrantLockService<IH5uTcp> _lockService;
        private readonly ArmService _armService;
        private readonly SampleService _sampleService;
        public BatchSampleModToModArmTool(ReentrantLockService<IH5uTcp> lockService, ArmService armService, SampleService sampleService)
        {
            _lockService = lockService;
            _armService = armService;
            _sampleService = sampleService;
        }
        public override bool InitPins()
        {
            InsetPin(START_ARM_SIGNAL, this, typeof(QData), PinType.Input);
            InsetPin(OUTPUT_SIGNAL_PIN, this, typeof(QData), PinType.Output);
            return true;
        }
        public override bool InitDataContext()
        {
            DataContext = new BatchTrayModArmData();
            return true;
        }
        public int Batch => Context<BatchTrayModArmData>().BatchCount;


        public override Task<bool> ClearEphemeralDataAsync()
        {
            batchProcessor.Reset();
            return Task.FromResult(true);
        }

        public override bool InitEnd()
        {
            batchProcessor.SetProcessStep(async (batch, context) =>
            {
                using (var lockAc = _lockService.Acquire(GetModular().Messenger, this.UniqueId.ToString(), RequestCancelToken))
                {
                    var sourceModuleChannelGroup = context["SourceModuleChannelGroup"] as ModuleChannelGroup;
                    var targetModuleChannelGroup = context["TargetModuleChannelGroup"] as ModuleChannelGroup;
                    var source_qChannelSlot = sourceModuleChannelGroup.GetWorkingChannel();
                    var target_qChannelSlot = targetModuleChannelGroup.GetIdleChannel();

                    var claw = new ClawModel
                    {
                        FromOpenPos = sourceModuleChannelGroup.ClawSetting.OpenPos,
                        FromAngle = sourceModuleChannelGroup.ClawSetting.Angle,
                        ToAngle = targetModuleChannelGroup.ClawSetting.Angle,
                        ToOpenPos = targetModuleChannelGroup.ClawSetting.OpenPos
                    }; 
                    var materialId = source_qChannelSlot.Labware.Material.MaterialNo;
                    var sample = _sampleService.GetSample(materialId);
                    await _armService.SampleArmTransport(UniqueId, sample, (QPosition)source_qChannelSlot.Position.Clone(), (QPosition) target_qChannelSlot.Position.Clone(), claw, Context<TrayModArmData>(), GetModular(), RequestCancelToken);
                    target_qChannelSlot.Put(source_qChannelSlot.Take());
                }
            });
            return true;
        }
        public override async Task<bool> ExecuteAsync(PinInfo pinInfo, QData pinData, ToolExecutionContext toolContext)
        {
            using (var lockAc = _lockService.Acquire(GetModular().Messenger, this.UniqueId.ToString(), RequestCancelToken))
            {
                if (await batchProcessor.ProcessBatchAsync(Batch, new Dictionary<string, object>()
                {
                    { "SourceModuleChannelGroup", SourceModuleChannelGroup },
                    { "TargetModuleChannelGroup", TargetModuleChannelGroup }
                }, RequestCancelToken))
                {
                    SendToPin(OUTPUT_SIGNAL_PIN, new QData());
                    return true;
                }
            }
            throw new ArgumentException("批样品模块到模块异常");
        }

        [RefParameter<ModuleChannelGroupTable>]
        public ModuleChannelGroup SourceModuleChannelGroup { set; get; }

        [RefParameter<ModuleChannelGroupTable>]
        public ModuleChannelGroup TargetModuleChannelGroup { set; get; }
    }

    /// <summary>
    /// 耗材模块到模块
    /// </summary>
    [DisplayName(BatchMaterialModToModArmToolName)]
    public class BatchMaterialModToModArmTool : ArmModuleToolBase
    {
        public const string BatchMaterialModToModArmToolName = "批耗材模块到模块";

        private const string START_ARM_SIGNAL = "开始搬运信号";
        private const string START_ARM_IGNORE_SIGNAL = "开始搬运(忽略状态)信号";
        private const string OUTPUT_SIGNAL_PIN = "输出完成信号";
        public override string DefineName => BatchMaterialModToModArmToolName;

        private readonly BatchProcessor batchProcessor = new();
        private readonly BatchProcessor batchProcessor_ignore = new();
        private readonly ReentrantLockService<IH5uTcp> _lockService;
        private readonly ArmService _armService;
        public BatchMaterialModToModArmTool(ReentrantLockService<IH5uTcp> lockService, ArmService armService)
        {
            _lockService = lockService;
            _armService = armService;
        }
        public override bool InitPins()
        {
            InsetPin(START_ARM_SIGNAL, this, typeof(QData), PinType.Input);
            InsetPin(START_ARM_IGNORE_SIGNAL, this, typeof(QData), PinType.Input);
            InsetPin(OUTPUT_SIGNAL_PIN, this, typeof(QData), PinType.Output);
            return true;
        }
        public override bool InitDataContext()
        {
            DataContext = new BatchTrayModArmData();
            return true;
        }

        public int Batch => Context<BatchTrayModArmData>().BatchCount;


        public override Task<bool> ClearEphemeralDataAsync()
        {
            batchProcessor.Reset();
            batchProcessor_ignore.Reset();
            return Task.FromResult(true);
        }

        public override bool InitEnd()
        {
            batchProcessor.SetProcessStep(async (batch, context) =>
            {
                using (var lockAc = _lockService.Acquire(GetModular().Messenger, this.UniqueId.ToString(), RequestCancelToken))
                {
                    var sourceModuleChannelGroup = context["SourceModuleChannelGroup"] as ModuleChannelGroup;
                    var targetModuleChannelGroup = context["TargetModuleChannelGroup"] as ModuleChannelGroup;
                    var source_qChannelSlot = sourceModuleChannelGroup.GetWorkingChannel();
                    var target_qChannelSlot = targetModuleChannelGroup.GetIdleChannel();

                    var claw = new ClawModel
                    {
                        ToOpenPos = targetModuleChannelGroup.ClawSetting.OpenPos,
                        ToAngle = targetModuleChannelGroup.ClawSetting.Angle,
                        FromOpenPos = sourceModuleChannelGroup.ClawSetting.OpenPos,
                        FromAngle = sourceModuleChannelGroup.ClawSetting.Angle
                    };

                    await _armService.ArmTransport((QPosition)source_qChannelSlot.Position.Clone(), (QPosition)target_qChannelSlot.Position.Clone() , claw,Context<TrayModArmData>(),GetModular(), RequestCancelToken);
                    target_qChannelSlot.Put(source_qChannelSlot.Take());
                }
            });
            batchProcessor_ignore.SetProcessStep(async (batch, context) =>
            {
                using (var lockAc = _lockService.Acquire(GetModular().Messenger, this.UniqueId.ToString(), RequestCancelToken))
                {
                    var sourceModuleChannelGroup = context["SourceModuleChannelGroup"] as ModuleChannelGroup;
                    var targetModuleChannelGroup = context["TargetModuleChannelGroup"] as ModuleChannelGroup;
                    var source_qChannelSlot = sourceModuleChannelGroup.GetChannelByIndex(batch + 1);
                    var target_qChannelSlot = targetModuleChannelGroup.GetChannelByIndex(batch + 1);

                    var claw = new ClawModel
                    {
                        ToOpenPos = targetModuleChannelGroup.ClawSetting.OpenPos,
                        ToAngle = targetModuleChannelGroup.ClawSetting.Angle,
                        FromOpenPos = sourceModuleChannelGroup.ClawSetting.OpenPos,
                        FromAngle = sourceModuleChannelGroup.ClawSetting.Angle
                    };

                    await _armService.ArmTransport((QPosition)source_qChannelSlot.Position.Clone(), (QPosition)target_qChannelSlot.Position.Clone(), claw, Context<TrayModArmData>(), GetModular(), RequestCancelToken);
                }
            });
            return true;
        }

        public override async Task<bool> ExecuteAsync(PinInfo pinInfo, QData pinData, ToolExecutionContext toolContext)
        {
            if (pinInfo.Name == START_ARM_SIGNAL)
            {
                using (var lockAc = _lockService.Acquire(GetModular().Messenger, this.UniqueId.ToString(), RequestCancelToken))
                {
                    if (await batchProcessor.ProcessBatchAsync(Batch, new Dictionary<string, object>
                    {
                            { "SourceModuleChannelGroup", SourceModuleChannelGroup },
                            { "TargetModuleChannelGroup", TargetModuleChannelGroup }
                    }, RequestCancelToken))
                    {
                        SendToPin(OUTPUT_SIGNAL_PIN, new QData());
                        return true;
                    }
                }
            }
            else if (pinInfo.Name == START_ARM_IGNORE_SIGNAL)
            {
                using (var lockAc = _lockService.Acquire(GetModular().Messenger, this.UniqueId.ToString(), RequestCancelToken))
                {
                    if (await batchProcessor_ignore.ProcessBatchAsync(Batch, new Dictionary<string, object>
                    {
                            { "SourceModuleChannelGroup", SourceModuleChannelGroup },
                            { "TargetModuleChannelGroup", TargetModuleChannelGroup }
                    }, RequestCancelToken))
                    {
                        SendToPin(OUTPUT_SIGNAL_PIN, new QData());
                        return true;
                    }
                }
            }

            throw new ArgumentException("PinInfo Name is not match");
        }

        [RefParameter<ModuleChannelGroupTable>]
        public ModuleChannelGroup SourceModuleChannelGroup { set; get; }

        [RefParameter<ModuleChannelGroupTable>]
        public ModuleChannelGroup TargetModuleChannelGroup { set; get; }
    }

    /// <summary>
    /// 批量通道模块
    /// </summary>
    [DisplayName(BatchChannelModuleToolName)]
    public class BatchChannelModuleTool : ModuleWithParameterToolBase
    {
        public const string BatchChannelModuleToolName = "批量通道模块";

        private const string _inputTrigger = "触发模块执行";
        private const string _outputCompleted = "模块执行完成信号";
        private const string _outputExceptionCode = "输出模块异常码";
        private const string _outputSampleId = "输出样品ID";
        public override string DefineName => BatchChannelModuleToolName;

        private readonly ModuleFunCodecService _moduleFunCodecService;
        private readonly ReentrantLockService<IH5uTcp> _lockService;
        private readonly SampleService _sampleService;
        public BatchChannelModuleTool(ModuleFunCodecService moduleFunCodecService, ReentrantLockService<IH5uTcp> lockService, SampleService sampleService)
        {
            _moduleFunCodecService = moduleFunCodecService;
            _lockService = lockService;
            _sampleService = sampleService;
        }

        public override bool InitPins()
        {
            InsetPin(_inputTrigger, this, typeof(QData), PinType.Input);
            InsetPin(_outputCompleted, this, typeof(QData), PinType.Output);
            InsetPin(_outputExceptionCode, this, typeof(QInt), PinType.Output);
            InsetPin(_outputSampleId, this, typeof(QInt64), PinType.Output);
            return true;
        }

        public override bool InitEnd()
        {
            return true;
        }

        public override async Task<bool> ExecuteAsync(PinInfo pinInfo, QData pinData, ToolExecutionContext toolContext)
        {
            using (var lockAc = _lockService.Acquire(GetModular().Messenger, this.UniqueId.ToString(), RequestCancelToken))
            {
                try
                {
                    var dic = new Dictionary<int, SampleTaskInfo>();
                    for (int i = 1; i <= SampleModuleChannelGroup.ChannelCount; i++)
                    {
                        dic.Add(i, _sampleService.GetSample(SampleModuleChannelGroup.GetChannelByIndex(i).Labware.Material.MaterialNo));
                    }

                    await _moduleFunCodecService.SampleStartFuncCodeExecuteAsync(UniqueId, dic, toolContext, GetModular(), Logger, RequestCancelToken);
                    foreach (var item in dic)
                    {
                        SendToPin(_outputSampleId, (QInt64)item.Value.SamplingId);
                    }
                    SendToPin(_outputCompleted, pinData);
                    return true;
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                    throw;
                }
               
            }
        }
        public override async Task<bool> HandleExecutedModuleErrorAsync(ModularException modularException)
        {
            if (modularException.IsModuleError)
            {
                SendToPin(_outputExceptionCode, new QInt(modularException.ModuleErrorCode));
                return await Task.FromResult(true);
            }
            return await Task.FromResult(false);
        }

        [RefParameter<ModuleChannelGroupTable>]
        public ModuleChannelGroup SampleModuleChannelGroup { set; get; }
    }

    /// <summary>
    /// 批量转移通道模块
    /// </summary>
    [DisplayName("批量转移通道模块")]
    public class BatchTransferChannelModuleTool : ModuleWithParameterToolBase
    {
        private const string _inputTrigger = "触发模块执行";
        private const string _outputCompleted = "模块执行完成信号";
        private const string _outputExceptionCode = "输出模块异常码";
        private const string _outputSampleId = "输出样品ID";
        public override string DefineName => "批量转移通道模块";

        private readonly ModuleFunCodecService _moduleFunCodecService;
        private readonly ReentrantLockService<IH5uTcp> _lockService;
        private readonly SampleService _sampleService;
        public BatchTransferChannelModuleTool(ModuleFunCodecService moduleFunCodecService, ReentrantLockService<IH5uTcp> lockService, SampleService sampleService)
        {
            _moduleFunCodecService = moduleFunCodecService;
            _lockService = lockService;
            _sampleService = sampleService;
        }

        public override bool InitPins()
        {
            InsetPin(_inputTrigger, this, typeof(QData), PinType.Input);
            InsetPin(_outputCompleted, this, typeof(QData), PinType.Output);
            InsetPin(_outputExceptionCode, this, typeof(QInt), PinType.Output);
            InsetPin(_outputSampleId, this, typeof(QInt64), PinType.Output);
            return true;
        }
        public override async Task<bool> ExecuteAsync(PinInfo pinInfo, QData pinData, ToolExecutionContext toolContext)
        {
            using (var lockAc = _lockService.Acquire(GetModular().Messenger, this.UniqueId.ToString(), RequestCancelToken))
            {
                var dic = new Dictionary<int, SampleTaskInfo>();
                for (int i = 1; i <= SampleModuleChannelGroup.ChannelCount; i++)
                {
                    dic.Add(i, _sampleService.GetSample(SampleModuleChannelGroup.GetChannelByIndex(i).Labware.Material.MaterialNo));
                }
                await _moduleFunCodecService.SampleStartFuncCodeExecuteAsync(UniqueId, dic, toolContext, GetModular(), Logger, RequestCancelToken);
                for (int i = 0; i < SampleModuleChannelGroup.ChannelCount; i++)
                {
                    var source_qChannelSlot = SampleModuleChannelGroup.GetChannelByIndex(i + 1);
                    var target_qChannelSlot = TargetSampleModuleChannelGroup.GetChannelByIndex(i + 1);
                    if (target_qChannelSlot.Labware == null)
                    {
                        target_qChannelSlot.Put(source_qChannelSlot.Labware);
                    }
                    else
                        target_qChannelSlot.Labware.PlaceMaterial(source_qChannelSlot.Labware.Material);
                }
                foreach (var item in dic)
                {
                    SendToPin(_outputSampleId, (QInt64)item.Value.SamplingId);
                }
                SendToPin(_outputCompleted, pinData);
                return true;
            }
        }

        public override async Task<bool> HandleExecutedModuleErrorAsync(ModularException modularException)
        {
            if (modularException.IsModuleError)
            {
                SendToPin(_outputExceptionCode, new QInt(modularException.ModuleErrorCode));
                return await Task.FromResult(true);
            }
            return await Task.FromResult(false);
        }

        [RefParameter<ModuleChannelGroupTable>]
        public ModuleChannelGroup SampleModuleChannelGroup { set; get; }

        [RefParameter<ModuleChannelGroupTable>]
        public ModuleChannelGroup TargetSampleModuleChannelGroup { set; get; }
    }

    public class SampleDiverterData
    {
        /// <summary>
        /// 处理批次数
        /// </summary>
        [DisplayName("处理批次数")]
        public int ProcessBatchCount { get; set; } = 2;
        /// <summary>
        /// 是否启用通道1
        /// </summary>
        [DisplayName("是否启用通道1")]
        public bool IsEnableChannel1 { get; set; }
        /// <summary>
        /// 是否启用通道2
        /// </summary>
        [DisplayName("是否启用通道2")]
        public bool IsEnableChannel2 { get; set; }
        /// <summary>
        /// 是否启用通道3
        /// </summary>
        [DisplayName("是否启用通道3")]
        public bool IsEnableChannel3 { get; set; }
        /// <summary>
        /// 是否启用通道4
        /// </summary>
        [DisplayName("是否启用通道4")]
        public bool IsEnableChannel4 { get; set; }
        /// <summary>
        /// 是否启用通道5
        /// </summary>
        [DisplayName("是否启用通道5")]
        public bool IsEnableChannel5 { get; set; }
        /// <summary>
        /// 是否启用通道6
        /// </summary>
        [DisplayName("是否启用通道6")]
        public bool IsEnableChannel6 { get; set; }
    }

    /// <summary>
    /// 全局样品分流器
    /// </summary>
    [DisplayName("全局样品分流器")]
    public class GlobalSampleDiverter : ToolBase
    {
        public override string DefineName => "全局样品分流器";

        private const string IN_PUTSAMPLE_ID = "输入样品ID";

        private readonly DistributedLockManager _distributedLockManager;

        private readonly Dictionary<int, (string Completed, string OutputPin, string ReleasePin, string LockId)> _channelConfigs;

        private readonly ConcurrentQueue<QInt64> sampleIdBag = new();
        private readonly List<string> cacheDistributedLockid = [];

        public GlobalSampleDiverter(DistributedLockManager distributedLockManager)
        {
            _distributedLockManager = distributedLockManager;

            _channelConfigs = new Dictionary<int, (string, string, string, string)>
        {
            { 1, ("通道1分流完成","输出通道1样品ID", "释放通道1", "550e8400e29b41d4a716446655440000") },
            { 2, ("通道2分流完成","输出通道2样品ID", "释放通道2", "9b10b8a0c3d44e5f8a7b6c9d0e1f2a3b") },
            { 3, ("通道3分流完成","输出通道3样品ID", "释放通道3", "a1b2c3d4e5f64789b0c1d2e3f4a5b6c7") },
            { 4, ("通道4分流完成","输出通道4样品ID", "释放通道4", "f1e2d3c4b5a64987c6b5a4f3e2d1c0b9") },
            { 5, ("通道5分流完成","输出通道5样品ID", "释放通道5", "7a8b9c0d1e2f4a3b9c8d7e6f5a4b3c2d") },
            { 6, ("通道6分流完成","输出通道6样品ID", "释放通道6", "e4d3c2b1a0f9e8d7c6b5a4f3e2d1c0b9") },
        };
        }

        public override bool InitPins()
        {
            InsetPin(IN_PUTSAMPLE_ID, this, typeof(QInt64), PinType.Input);

            foreach (var (_, config) in _channelConfigs)
            {
                InsetPin(config.OutputPin, this, typeof(QInt64), PinType.Output);
                InsetPin(config.ReleasePin, this, typeof(QData), PinType.Input);
                InsetPin(config.Completed, this, typeof(QData), PinType.Output);
            }

            return true;
        }

        public override bool InitDataContext()
        {
            DataContext = new SampleDiverterData();
            return true;
        }

        public override bool InitEnd()
        {
            _distributedLockManager.OnDistributedLockReleased += lockId =>
            {
                var releaseChannel = _channelConfigs.FirstOrDefault(c => c.Value.LockId == lockId);
                if (!releaseChannel.Equals(default(KeyValuePair<int, (string, string, string, string)>)))
                {
                    if (!sampleIdBag.IsEmpty && sampleIdBag.Count >= Context<SampleDiverterData>().ProcessBatchCount)
                    {
                        if (!_distributedLockManager.IsWaitForDistributedLock(lockId))
                        {
                            _distributedLockManager.CreateDistributedLock(lockId,RequestCancelToken);
                            lock (cacheDistributedLockid)
                            {
                                cacheDistributedLockid.Add(lockId);
                            }
                            SendBatchToChannel(releaseChannel.Value.OutputPin);
                            SendToPin(releaseChannel.Value.Completed, new QData());
                        }
                    }
                }
            };
            return true;
        }

        public override async Task<bool> ClearEphemeralDataAsync()
        {
            sampleIdBag.Clear();
            if (cacheDistributedLockid.Count != 0)
            {
                foreach (var id in cacheDistributedLockid)
                {
                    _distributedLockManager.RemoveDistributedLock(id);
                    Logger?.LogInformation($"移除分布式锁: {id}");
                }
                cacheDistributedLockid.Clear();
            }
            return await Task.FromResult(true);
        }
        public override async Task<bool> ExecuteAsync(PinInfo pinInfo, QData pinData, ToolExecutionContext toolContext)
        {
            if (pinInfo.Name == IN_PUTSAMPLE_ID)
            {
                var sampleId = (QInt64)pinData;
                sampleIdBag.Enqueue(sampleId);

                if (sampleIdBag.Count >= Context<SampleDiverterData>().ProcessBatchCount)
                {
                    foreach (var (channel, config) in _channelConfigs)
                    {
                        if (!IsChannelEnabled(channel)) continue;
                        if (!_distributedLockManager.IsWaitForDistributedLock(config.LockId))
                        {
                            _distributedLockManager.CreateDistributedLock(config.LockId, RequestCancelToken);
                            lock (cacheDistributedLockid)
                            {
                                cacheDistributedLockid.Add(config.LockId);
                            }
                            SendBatchToChannel(config.OutputPin);
                            SendToPin(config.Completed, new QData());
                            break;
                        }
                    }
                }
                return await Task.FromResult(true);
            }
            var releaseChannel = _channelConfigs.FirstOrDefault(c => c.Value.ReleasePin == pinInfo.Name);
            if (!releaseChannel.Equals(default(KeyValuePair<int, (string, string, string)>)))
            {
                var channel = releaseChannel.Key;
                var (Completed, OutputPin, ReleasePin, LockId) = releaseChannel.Value;

                if (IsChannelEnabled(channel))
                {
                    if (_distributedLockManager.IsWaitForDistributedLock(LockId))
                    {
                        _ = _distributedLockManager.ReleaseDistributedLock(LockId);
                        lock (cacheDistributedLockid)
                        {
                            cacheDistributedLockid.Remove(LockId);
                        }
                    }
                }
                return await Task.FromResult(true);
            }

            throw new Exception($"未知的释放模块指令: {pinInfo.Name}");
        }

        /// <summary>
        /// 判断通道是否启用
        /// </summary>
        private bool IsChannelEnabled(int channel) =>
            channel switch
            {
                1 => Context<SampleDiverterData>().IsEnableChannel1,
                2 => Context<SampleDiverterData>().IsEnableChannel2,
                3 => Context<SampleDiverterData>().IsEnableChannel3,
                4 => Context<SampleDiverterData>().IsEnableChannel4,
                5 => Context<SampleDiverterData>().IsEnableChannel5,
                6 => Context<SampleDiverterData>().IsEnableChannel6,
                _ => false
            };

        /// <summary>
        /// 从队列取出并发送到指定通道
        /// </summary>
        private void SendBatchToChannel(string outputPin)
        {
            var list = new List<QInt64>();
            while (list.Count < Context<SampleDiverterData>().ProcessBatchCount &&
                   sampleIdBag.TryDequeue(out var id))
            {
                list.Add(id);
            }

            foreach (var id in list)
            {
                SendToPin(outputPin, id);
            }
        }
    }

    /// <summary>
    /// 样品分流器
    /// </summary>
    [DisplayName("样品分流器")]
    public class SampleDiverter : ToolBase
    {
        public override string DefineName => "样品分流器";

        private const string IN_PUTSAMPLE_ID = "输入样品ID";

        private readonly DistributedLockManager _distributedLockManager;

        private readonly Dictionary<int, (string Completed, string OutputPin, string ReleasePin, string LockId)> _channelConfigs;

        private readonly ConcurrentQueue<QInt64> sampleIdBag = new();
        private readonly List<string> cacheDistributedLockid = [];

        public SampleDiverter(DistributedLockManager distributedLockManager)
        {
            _distributedLockManager = distributedLockManager;

            _channelConfigs = new Dictionary<int, (string, string, string, string)>
        {
            { 1, ("通道1分流完成","输出通道1样品ID", "释放通道1", Guid.NewGuid().ToString("N")) },
            { 2, ("通道2分流完成","输出通道2样品ID", "释放通道2", Guid.NewGuid().ToString("N")) },
            { 3, ("通道3分流完成","输出通道3样品ID", "释放通道3", Guid.NewGuid().ToString("N")) },
            { 4, ("通道4分流完成","输出通道4样品ID", "释放通道4", Guid.NewGuid().ToString("N")) },
            { 5, ("通道5分流完成","输出通道5样品ID", "释放通道5", Guid.NewGuid().ToString("N")) },
            { 6, ("通道6分流完成","输出通道6样品ID", "释放通道6", Guid.NewGuid().ToString("N")) },
        };
        }

        public override bool InitPins()
        {
            InsetPin(IN_PUTSAMPLE_ID, this, typeof(QInt64), PinType.Input);

            foreach (var (_, config) in _channelConfigs)
            {
                InsetPin(config.OutputPin, this, typeof(QInt64), PinType.Output);
                InsetPin(config.ReleasePin, this, typeof(QData), PinType.Input);
                InsetPin(config.Completed, this, typeof(QData), PinType.Output);
            }

            return true;
        }

        public override bool InitDataContext()
        {
            DataContext = new SampleDiverterData();
            return true;
        }


        public override async Task<bool> ClearEphemeralDataAsync()
        {
            sampleIdBag.Clear();
            if (cacheDistributedLockid.Count != 0)
            {
                foreach (var id in cacheDistributedLockid)
                {
                    _distributedLockManager.RemoveDistributedLock(id);
                    Logger?.LogInformation($"移除分布式锁: {id}");
                }
                cacheDistributedLockid.Clear();
            }
            return await Task.FromResult(true);
        }
   
        public override async Task<bool> ExecuteAsync(PinInfo pinInfo, QData pinData, ToolExecutionContext toolContext)
        {
            if (pinInfo.Name == IN_PUTSAMPLE_ID)
            {
                var sampleId = (QInt64)pinData;
                sampleIdBag.Enqueue(sampleId);

                if (sampleIdBag.Count >= Context<SampleDiverterData>().ProcessBatchCount)
                {
                    foreach (var (channel, config) in _channelConfigs)
                    {
                        if (!IsChannelEnabled(channel)) continue;
                        if (!_distributedLockManager.IsWaitForDistributedLock(config.LockId))
                        {
                            _distributedLockManager.CreateDistributedLock(config.LockId,RequestCancelToken);
                            lock (cacheDistributedLockid)
                            {
                                cacheDistributedLockid.Add(config.LockId);
                            }
                            SendBatchToChannel(config.OutputPin);
                            SendToPin(config.Completed,QData.Empty);
                            break;
                        }
                    }
                }
                return await Task.FromResult(true);
            }
            var releaseChannel = _channelConfigs.FirstOrDefault(c => c.Value.ReleasePin == pinInfo.Name);
            if (!releaseChannel.Equals(default(KeyValuePair<int, (string, string, string)>)))
            {
                var channel = releaseChannel.Key;
                var (Completed, OutputPin, ReleasePin, LockId) = releaseChannel.Value;

                if (IsChannelEnabled(channel))
                {
                    _ = _distributedLockManager.ReleaseDistributedLock(LockId);
                    lock (cacheDistributedLockid)
                    {
                        cacheDistributedLockid.Remove(LockId);
                    }
                    if (sampleIdBag.Count >= Context<SampleDiverterData>().ProcessBatchCount)
                    {
                        _distributedLockManager.CreateDistributedLock(LockId,RequestCancelToken);
                        lock (cacheDistributedLockid)
                        {
                            cacheDistributedLockid.Add(LockId);
                        }
                        SendBatchToChannel(OutputPin);
                        SendToPin(Completed, QData.Empty);
                    }
                }
                return true;
            }

            throw new Exception($"未知的释放模块指令: {pinInfo.Name}");
        }

        /// <summary>
        /// 判断通道是否启用
        /// </summary>
        private bool IsChannelEnabled(int channel) =>
            channel switch
            {
                1 => Context<SampleDiverterData>().IsEnableChannel1,
                2 => Context<SampleDiverterData>().IsEnableChannel2,
                3 => Context<SampleDiverterData>().IsEnableChannel3,
                4 => Context<SampleDiverterData>().IsEnableChannel4,
                5 => Context<SampleDiverterData>().IsEnableChannel5,
                6 => Context<SampleDiverterData>().IsEnableChannel6,
                _ => false
            };

        /// <summary>
        /// 从队列取出并发送到指定通道
        /// </summary>
        private void SendBatchToChannel(string outputPin)
        {
            var list = new List<QInt64>();
            while (list.Count < Context<SampleDiverterData>().ProcessBatchCount &&
                   sampleIdBag.TryDequeue(out var id))
            {
                list.Add(id);
            }

            foreach (var id in list)
            {
                SendToPin(outputPin, id);
            }
        }
    }

    public class BatchTrayModArmStewData : BatchTrayModArmData
    {
        /// <summary>
        /// 静置时间
        /// </summary>
        [DisplayName("静置时间/s")]
        public int StewTime { get; set; }
    }
    /// <summary>
    /// 批样品模块到托盘静置
    /// </summary>
    [DisplayName(BatchSampleModToTrayStewingToolName)]
    public class BatchSampleModToTrayStewingTool : ArmModuleToolBase, ILabTrayService
    {
        private const string BatchSampleModToTrayStewingToolName = "批样品模块到托盘静置";
        private const string START_ARM_TONEW_SIGNAL = "搬运到新托盘信号";
        private const string START_ARM_SIGNAL = "开始搬回信号";
        private const string OUTPUT_SIGNAL_PIN = "输出搬运完成信号";
        private const string OUTPUT_STEWING_SAMPLE_ID_PIN = "输出静置完成的样品ID";

        public override string DefineName => BatchSampleModToTrayStewingToolName;

        private readonly BatchProcessor batchProcessor = new();
        private readonly BatchProcessor batchProcessor_new = new();

        private readonly ReentrantLockService<IH5uTcp> _lockService;
        private readonly ArmService _armService;
        private readonly TemporaryTimingWheel _timingWheel;
        private readonly SampleService _sampleService;
        public BatchSampleModToTrayStewingTool(ReentrantLockService<IH5uTcp> lockService, ArmService armService, TemporaryTimingWheel timingWheel, SampleService sampleService)
        {
            _lockService = lockService;
            _armService = armService;
            _timingWheel = timingWheel;
            _sampleService = sampleService;
        }
        public override bool InitPins()
        {
            InsetPin(START_ARM_SIGNAL, this, typeof(QData), PinType.Input);
            InsetPin(START_ARM_TONEW_SIGNAL, this, typeof(QData), PinType.Input);
            InsetPin(OUTPUT_SIGNAL_PIN, this, typeof(QData), PinType.Output);
            InsetPin(OUTPUT_STEWING_SAMPLE_ID_PIN, this, typeof(QInt64), PinType.Output);
            return true;
        }
        public override bool InitDataContext()
        {
            DataContext = new BatchTrayModArmStewData();
            return true;
        }
        public int Batch => Context<BatchTrayModArmData>().BatchCount;

        public int StewTime => Context<BatchTrayModArmStewData>().StewTime;

        public override Task<bool> ClearEphemeralDataAsync()
        {
            batchProcessor.Reset();
            batchProcessor_new.Reset();
            return Task.FromResult(true);
        }
        public override bool InitEnd()
        {
            batchProcessor.SetProcessStep(async (batch, context) =>
            {
                using (var lockAc = _lockService.Acquire(GetModular().Messenger, this.UniqueId.ToString(), RequestCancelToken))
                {
                    var sampleModuleChannelGroup = context["SampleModuleChannelGroup"] as ModuleChannelGroup;
                    var labTray = context["LabTray"] as LabTray;
                    var labTray2 = context["LabTray2"] as LabTray;
                    var qChannelSlot = sampleModuleChannelGroup.GetWorkingChannel();
                    var well = labTray.FindTakenWellAllowNull(qChannelSlot.Labware) ?? labTray2.FindTakenWell(qChannelSlot.Labware);

                    var claw = new ClawModel
                    {
                        FromOpenPos = sampleModuleChannelGroup.ClawSetting.OpenPos,
                        FromAngle = sampleModuleChannelGroup.ClawSetting.Angle,
                        ToAngle = well.ClawSetting.Angle,
                        ToOpenPos = well.ClawSetting.OpenPos
                    };

                    var materialId = qChannelSlot.Labware.Material.MaterialNo;
                    var sample = _sampleService.GetSample(materialId);
                    await _armService.SampleArmTransport(UniqueId, sample, (QPosition)qChannelSlot.Position.Clone(), (QPosition)well.Position.Clone(), claw,Context<BatchTrayModArmData>(), GetModular(), RequestCancelToken);
                    well.Return(qChannelSlot.Take());
                    _timingWheel.AddTask(StringId, new TemporaryTask()
                    {
                        Id = sample.SamplingId,
                        RemainingRounds = StewTime
                    });
                }
            });
            batchProcessor_new.SetProcessStep(async (batch, context) =>
            {
                using (var lockAc = _lockService.Acquire(GetModular().Messenger, this.UniqueId.ToString(), RequestCancelToken))
                {
                    var sampleModuleChannelGroup = context["SampleModuleChannelGroup"] as ModuleChannelGroup;
                    var labTray = context["LabTray"] as LabTray;
                    var labTray2 = context["LabTray2"] as LabTray;
                    var qChannelSlot = sampleModuleChannelGroup.GetWorkingChannel();
                    var well = labTray.FindFirstEmptyOrTakenWellAllowNull() ?? labTray2.FindFirstEmptyOrTakenWell();
                    var materialId = qChannelSlot.Labware.Material.MaterialNo;
                    var claw = new ClawModel
                    {
                        FromOpenPos = sampleModuleChannelGroup.ClawSetting.OpenPos,
                        FromAngle = sampleModuleChannelGroup.ClawSetting.Angle,
                        ToAngle = well.ClawSetting.Angle,
                        ToOpenPos = well.ClawSetting.OpenPos
                    };

                    var sample = _sampleService.GetSample(materialId);
                    await _armService.SampleArmTransport(UniqueId, sample, (QPosition)qChannelSlot.Position.Clone(), (QPosition)well.Position.Clone(),claw, Context<BatchTrayModArmData>(), GetModular(), RequestCancelToken);
                    well.PlaceMaterial(qChannelSlot.Take());
                    _timingWheel.AddTask(StringId, new TemporaryTask()
                    {
                        Id = sample.SamplingId,
                        RemainingRounds = StewTime
                    });
                }
            });
            _timingWheel.RegisterNotify(StringId, task =>
            {
                SendToPin(OUTPUT_STEWING_SAMPLE_ID_PIN, (QInt64)task.Id);
            });
            return true;
        }
        public override async Task<bool> ExecuteAsync(PinInfo pinInfo, QData pinData, ToolExecutionContext toolContext)
        {
            if (pinInfo.Name == START_ARM_SIGNAL)
            {
                using (var lockAc = _lockService.Acquire(GetModular().Messenger, this.UniqueId.ToString(), RequestCancelToken))
                {
                    if (await batchProcessor.ProcessBatchAsync(Batch, new Dictionary<string, object>
                {
                    { "SampleModuleChannelGroup", SampleModuleChannelGroup },
                    { "LabTray", LabTray },
                    { "LabTray2", LabTray2 }
                }, RequestCancelToken))
                    {
                        SendToPin(OUTPUT_SIGNAL_PIN, new QData());
                        return true;
                    }
                }
            }
            else if (pinInfo.Name == START_ARM_TONEW_SIGNAL)
            {
                using (var lockAc = _lockService.Acquire(GetModular().Messenger, this.UniqueId.ToString(), RequestCancelToken))
                {
                    if (await batchProcessor_new.ProcessBatchAsync(Batch, new Dictionary<string, object>
                {
                    { "SampleModuleChannelGroup", SampleModuleChannelGroup },
                    { "LabTray", LabTray },
                    { "LabTray2", LabTray2 }
                }, RequestCancelToken))
                    {
                        SendToPin(OUTPUT_SIGNAL_PIN, new QData());
                        return true;
                    }
                }
            }
            throw new ArgumentException("PinInfo Name is not match");
        }

        [RefParameter<LabTrayTable>]
        public LabTray LabTray { set; get; }

        //托盘2
        [RefParameter<LabTrayTable>]
        public LabTray LabTray2 { set; get; }


        [RefParameter<ModuleChannelGroupTable>]
        public ModuleChannelGroup SampleModuleChannelGroup { set; get; }

        public List<LabTray> LabTrays => [LabTray, LabTray2];
    }


    /// <summary>
    /// 批样品模块到模块静置
    /// </summary>
    [DisplayName(BatchSampleModToModArmStewingToolName)]
    public class BatchSampleModToModArmStewingTool : ArmModuleToolBase
    {
        public const string BatchSampleModToModArmStewingToolName = "批样品模块到模块静置";

        private const string START_ARM_SIGNAL = "开始搬运信号";
        private const string OUTPUT_SAMPLE_ID_PIN = "输出样品ID";
        private const string OUTPUT_SIGNAL_PIN = "输出完成信号";

        public override string DefineName => BatchSampleModToModArmStewingToolName;

        private readonly BatchProcessor batchProcessor = new();

        private readonly ReentrantLockService<IH5uTcp> _lockService;
        private readonly ArmService _armService;
        private readonly TemporaryTimingWheel _timingWheel;
        private readonly SampleService _sampleService;
        public BatchSampleModToModArmStewingTool(
            ReentrantLockService<IH5uTcp> lockService
            , ArmService armService
            , TemporaryTimingWheel timingWheel
            , SampleService sampleService)
        {
            _lockService = lockService;
            _armService = armService;
            _timingWheel = timingWheel;
            _sampleService = sampleService;
        }
        public override bool InitPins()
        {
            InsetPin(START_ARM_SIGNAL, this, typeof(QData), PinType.Input);
            InsetPin(OUTPUT_SAMPLE_ID_PIN, this, typeof(QInt64), PinType.Output);
            InsetPin(OUTPUT_SIGNAL_PIN, this, typeof(QData), PinType.Output);
            return true;
        }
        public override bool InitDataContext()
        {
            DataContext = new BatchTrayModArmStewData();
            return true;
        }
        public int Batch => Context<BatchTrayModArmStewData>().BatchCount;

        /// <summary>
        /// 静置时间
        /// </summary>
        public int StewTime => Context<BatchTrayModArmStewData>().StewTime;

        public override Task<bool> ClearEphemeralDataAsync()
        {
            batchProcessor.Reset();
            return Task.FromResult(true);
        }
        public override bool InitEnd()
        {
            batchProcessor.SetProcessStep(async (batch, context) =>
            {
                using (var lockAc = _lockService.Acquire(GetModular().Messenger, this.UniqueId.ToString(), RequestCancelToken))
                {
                    var sourceModuleChannelGroup = context["SourceModuleChannelGroup"] as ModuleChannelGroup;
                    var targetModuleChannelGroup = context["TargetModuleChannelGroup"] as ModuleChannelGroup;
                    var source_qChannelSlot = sourceModuleChannelGroup.GetWorkingChannel();
                    var target_qChannelSlot = targetModuleChannelGroup.GetIdleChannel();

                    var claw = new ClawModel
                    {
                        FromOpenPos = sourceModuleChannelGroup.ClawSetting.OpenPos,
                        FromAngle = sourceModuleChannelGroup.ClawSetting.Angle,
                        ToAngle = targetModuleChannelGroup.ClawSetting.Angle,
                        ToOpenPos = targetModuleChannelGroup.ClawSetting.OpenPos
                    };
                    var materialId = source_qChannelSlot.Labware.Material.MaterialNo;
                    var sample = _sampleService.GetSample(materialId);
                    await _armService.SampleArmTransport(UniqueId, sample, (QPosition)source_qChannelSlot.Position.Clone(), (QPosition)target_qChannelSlot.Position.Clone(),claw, Context<TrayModArmData>(), GetModular(), RequestCancelToken);
                    target_qChannelSlot.Put(source_qChannelSlot.Take());
                }
            });
            _timingWheel.RegisterNotify(StringId, task =>
            {
                SendToPin(OUTPUT_SAMPLE_ID_PIN, (QInt64)task.Id);
            });
            return true;
        }
        public override async Task<bool> ExecuteAsync(PinInfo pinInfo, QData pinData, ToolExecutionContext toolContext)
        {
            using (var lockAc = _lockService.Acquire(GetModular().Messenger, this.UniqueId.ToString(), RequestCancelToken))
            {
                if (await batchProcessor.ProcessBatchAsync(Batch, new Dictionary<string, object>()
                {
                    { "SourceModuleChannelGroup", SourceModuleChannelGroup },
                    { "TargetModuleChannelGroup", TargetModuleChannelGroup }
                }, RequestCancelToken))
                {
                    SendToPin(OUTPUT_SIGNAL_PIN, new QData());
                    return true;
                }
            }
            throw new ArgumentException("批样品模块到模块异常");
        }

        [RefParameter<ModuleChannelGroupTable>]
        public ModuleChannelGroup SourceModuleChannelGroup { set; get; }

        [RefParameter<ModuleChannelGroupTable>]
        public ModuleChannelGroup TargetModuleChannelGroup { set; get; }
    }


    /// <summary>
    /// 样品工作完成
    /// </summary>
    /// <param name="sampleService"></param>

    [DisplayName(SAMPLE_WORK_COMPLETE)]
    public class SampleWorkCompleteTool(SampleService sampleService) : ToolBase
    {
        public const string SAMPLE_WORK_COMPLETE = "SampleWorkComplete";

        //输入样品ID
        public const string INPUT_SAMPLE_ID_PIN = "InputSampleId";

        public override string DefineName => SAMPLE_WORK_COMPLETE;

        private readonly SampleService _sampleService = sampleService;

        public override bool InitPins()
        {
            InsetPin(INPUT_SAMPLE_ID_PIN, this, typeof(QInt64), PinType.Input);
            return true;
        }
        public override async Task<bool> ExecuteAsync(PinInfo pinInfo, QData pinData, ToolExecutionContext toolContext)
        {
            _sampleService.SampleWorkComplete(pinData);
            return await Task.FromResult(true);
        }
    }

}
