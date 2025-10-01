using Microsoft.Extensions.Logging;
using QStandaedPlatform.Engine.Common.Common;
using QStandaedPlatform.Engine.Common.Common.Metadatas;
using QStandaedPlatform.Engine.Components;
using QStandaedPlatform.Engine.Laboratory;
using QStandaedPlatform.Engine.Laboratory.Documents;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace System.Foundation.Modules.Arms.Pipettes
{

    #region 吸液吐液
    /// <summary>
    /// 吸吐液_模块-模块
    /// </summary>
    [DisplayName("吸吐液_模块-模块")]
    public class LiquorReliefMToMArmTool : ModuleWithParameterToolBase
    {

        private const string InputSignalPinName = "输入移液开始信号";
        private const string OutputSignalPinName = "输出移液完成信号";

        public override string DefineName => "吸吐液_模块-模块";

        private readonly PipetteService _pipetteService;
        private readonly SampleService _sampleService;
        private readonly ReentrantLockService<IH5uTcp> _lockService;


        public LiquorReliefMToMArmTool(PipetteService pipetteService, SampleService sampleService, ReentrantLockService<IH5uTcp> lockService)
        {
            _pipetteService = pipetteService;
            _sampleService = sampleService;
            _lockService = lockService;
        }

        private readonly BatchProcessor batchProcessor = new();

        public override bool InitPins()
        {
            InsetPin(InputSignalPinName, this, typeof(QData), PinType.Input);
            InsetPin(OutputSignalPinName, this, typeof(QData), PinType.Output);

            return true;
        }

        public override bool InitDataContext()
        {
            DataContext = new LiquorArmToolData();
            return true;
        }

        public LiquorArmToolData PipetteArmToolData => Context<LiquorArmToolData>();

        [RefParameter<ModuleChannelGroupTable>]
        public ModuleChannelGroup SourceModuleChannelGroup { set; get; }

        [RefParameter<ModuleChannelGroupTable>]
        public ModuleChannelGroup SourcePosModuleChannelGroup { set; get; }

        [RefParameter<ModuleChannelGroupTable>]
        public ModuleChannelGroup TargetModuleChannelGroup { set; get; }

        [RefParameter<ModuleChannelGroupTable>]
        public ModuleChannelGroup TargetPosModuleChannelGroup { set; get; }


        public override async Task<bool> ExecuteAsync(PinInfo pinInfo, QData pinData, ToolExecutionContext toolContext)
        {
            var batchCount = PipetteArmToolData.BatchCount;

            using (var lockAc = _lockService.Acquire(GetModular().Messenger, this.UniqueId.ToString(), RequestCancelToken))
            {
                if (await batchProcessor.ProcessBatchAsync(batchCount, new Dictionary<string, object>
                {
                    { "pitterParams",PipetteArmToolData },
                    { "sourcePosModuleChannelGroup",SourcePosModuleChannelGroup },
                    { "targetPosModuleChannelGroup",TargetPosModuleChannelGroup },
                    { "sourceModuleChannelGroup",SourceModuleChannelGroup },
                    { nameof(TargetModuleChannelGroup),TargetModuleChannelGroup }
                }, RequestCancelToken))
                {
                    SendToPin(OutputSignalPinName, new QData());
                    return true;
                }
            }
            throw new Exception("吸液吐液,模块到模块失败");
        }

        public override bool InitEnd()
        {
            batchProcessor.SetProcessStep(async (batch, context) =>
            {
                using (var lockAc = _lockService.Acquire(GetModular().Messenger, this.UniqueId.ToString(), RequestCancelToken))
                {
                    var pitterParams = context["pitterParams"] as LiquorArmToolData;
                    var sourcePosModuleChannelGroup = context["sourcePosModuleChannelGroup"] as ModuleChannelGroup;
                    var targetPosModuleChannelGroup = context["targetPosModuleChannelGroup"] as ModuleChannelGroup;
                    var sourceModuleChannelGroup = context["sourceModuleChannelGroup"] as ModuleChannelGroup;
                    var targetModuleChannelGroup = context[nameof(TargetModuleChannelGroup)] as ModuleChannelGroup;

                    var liquieType = pitterParams.PitterLiquor;

                    var (srcChannel,srcaction) = SourcePosModuleChannelGroup.GetNextChannel();
                    var (destChannel,destaction) = TargetPosModuleChannelGroup.GetNextChannel();

                    var (sourceslot, sourceaction) = sourceModuleChannelGroup.GetNextChannel();
                    var (targerslot, targeraction) = targetModuleChannelGroup.GetNextChannel();

                    var src = (QPosition)srcChannel.Position.Clone();
                    var dest = (QPosition)destChannel.Position.Clone();

                    if (liquieType == LiquorType.Sample)
                    {
                        var qLabware = sourceslot.Labware ?? throw new Exception("吸液模块通道中没有找到，相关器皿");
                        var qMaterial = qLabware.Material ?? throw new Exception("吸液模块通道中没有找到，相关材料");
                        var sampleId = qMaterial.MaterialNo;
                        if (sampleId == 0)
                        {
                            throw new Exception("吸液模块通道中没有找到，相关样品ID");
                        }
                        var sampleTaskInfo = _sampleService.GetSample(sampleId);

                        await _pipetteService.SampleLiquorReliefAsync(UniqueId, sampleTaskInfo, GetModular(), src, dest, Logger);
                        var material = qLabware.TakeTwinMaterial();
                        if (material != null)
                        {
                            Logger.LogInformation("TakeTwinMaterial：{material}", material);
                            targerslot.Labware?.PlaceMaterial(material);
                            srcaction.Invoke(srcChannel);
                            destaction.Invoke(destChannel);
                            sourceaction.Invoke(sourceslot);
                            targeraction.Invoke(targerslot);
                        }
                        else
                        {
                            throw new Exception("吸液模块通道中没有找到，相关材料");
                        }
                    }
                    else
                    {
                        await _pipetteService.LiquorReliefAsync(GetModular(), src, dest, Logger);
                        srcaction.Invoke(srcChannel);
                        destaction.Invoke(destChannel);
                        sourceaction.Invoke(sourceslot);
                        targeraction.Invoke(targerslot);
                    }
                }
            });

            return true;

        }
    }

    /// <summary>
    /// 吸吐液_模块-模块
    /// </summary>
    [DisplayName("吸吐液_模块-托盘")]
    public class LiquorReliefMToTrayArmTool : ModuleWithParameterToolBase
    {

        private const string InputSignalPinName = "输入移液开始信号";
        private const string OutputSignalPinName = "输出移液完成信号";

        public override string DefineName => "吸吐液_模块-托盘";

        //

        private readonly PipetteService _pipetteService;
        private readonly SampleService _sampleService;
        private readonly ReentrantLockService<IH5uTcp> _lockService;


        public LiquorReliefMToTrayArmTool(PipetteService pipetteService, SampleService sampleService, ReentrantLockService<IH5uTcp> lockService)
        {
            _pipetteService = pipetteService;
            _sampleService = sampleService;
            _lockService = lockService;
        }

        private readonly BatchProcessor batchProcessor = new();

        public override bool InitPins()
        {
            InsetPin(InputSignalPinName, this, typeof(QData), PinType.Input);
            InsetPin(OutputSignalPinName, this, typeof(QData), PinType.Output);

            return true;
        }

        public override bool InitDataContext()
        {
            DataContext = new LiquorArmToolData();
            return true;
        }

        public LiquorArmToolData PipetteArmToolData => Context<LiquorArmToolData>();

        [RefParameter<ModuleChannelGroupTable>]
        public ModuleChannelGroup SourceModuleChannelGroup { set; get; }

        [RefParameter<ModuleChannelGroupTable>]
        public ModuleChannelGroup SourcePosModuleChannelGroup { set; get; }

        [RefParameter<LabTrayTable>]
        public LabTray TargetLabtray { set; get; }

        [RefParameter<LabTrayTable>]
        public LabTray TargetPosLabtray { set; get; }


        public override async Task<bool> ExecuteAsync(PinInfo pinInfo, QData pinData, ToolExecutionContext toolContext)
        {
            var batchCount = PipetteArmToolData.BatchCount;

            using (var lockAc = _lockService.Acquire(GetModular().Messenger, this.UniqueId.ToString(), RequestCancelToken))
            {
                if (await batchProcessor.ProcessBatchAsync(batchCount, new Dictionary<string, object>
                {
                    { "pitterParams",PipetteArmToolData },
                    { "sourcePosModuleChannelGroup",SourcePosModuleChannelGroup },
                    { "targetPosLabtray",TargetPosLabtray },
                    { "sourceModuleChannelGroup",SourceModuleChannelGroup },
                    { "targetLabtray",TargetLabtray }
                }, RequestCancelToken))
                {
                    SendToPin(OutputSignalPinName, new QData());
                    return true;
                }
            }
            throw new Exception("吸液吐液,模块到托盘失败");
        }

        public override bool InitEnd()
        {
            batchProcessor.SetProcessStep(async (batch, context) =>
            {
                using (var lockAc = _lockService.Acquire(GetModular().Messenger, this.UniqueId.ToString(), RequestCancelToken))
                {
                    var pitterParams = context["pitterParams"] as LiquorArmToolData;
                    var sourcePosModuleChannelGroup = context["sourcePosModuleChannelGroup"] as ModuleChannelGroup;
                    var targetPosLabtray = context["targetPosLabtray"] as LabTray;
                    var sourceModuleChannelGroup = context["sourceModuleChannelGroup"] as ModuleChannelGroup;
                    var targetLabtray = context["targetLabtray"] as LabTray;

                    var liquieType = pitterParams.PitterLiquor;

                    var srcChannel = SourcePosModuleChannelGroup.GetChannelByIndex(batch+1);
                    var (well,success) = targetPosLabtray.GetNextWell();
                    var (targetWell,targerSuccess)= targetLabtray.GetNextWell();

                    var src = (QPosition)srcChannel.Position.Clone();
                    var dest = (QPosition)well.Position.Clone();

                    if (liquieType == LiquorType.Sample)
                    {
                        var sampleId = SourceModuleChannelGroup.GetChannelByIndex(batch+1).Labware?.Material?.MaterialNo??throw new Exception("吸液模块通道中没有找到，相关样品");
                        var sampleTaskInfo = _sampleService.GetSample(sampleId);

                        await _pipetteService.SampleLiquorReliefAsync(UniqueId, sampleTaskInfo, GetModular(), src, dest, Logger);
                        var material = SourceModuleChannelGroup.GetQChannelSlotByMaterialId(sampleTaskInfo.SamplingId).Labware?.TakeTwinMaterial();
                        if (material != null)
                        {
                            Logger.LogInformation("TakeTwinMaterial：{material}", material);
                            targetWell.PlaceMaterialToTube(material);
                            success.Invoke(well);
                            targerSuccess.Invoke(targetWell);
                        }
                        else
                        {
                            throw new Exception("吸液模块通道中没有找到，相关物料");
                        }
                    }
                    else
                    {
                        await _pipetteService.LiquorReliefAsync(GetModular(), src, dest, Logger);
                        success.Invoke(well);
                        targerSuccess.Invoke(targetWell);
                    }
                }
            });

            return true;

        }
    }


    /// <summary>
    /// 吸吐液_托盘-托盘
    /// </summary>
    [DisplayName("吸吐液_托盘-托盘")]
    public class LiquorReliefTrayToTrayArmTool : ModuleWithParameterToolBase
    {

        private const string InputSignalPinName = "输入移液开始信号";
        private const string OutputSignalPinName = "输出移液完成信号";

        public override string DefineName => "吸吐液_托盘-托盘";

        private readonly PipetteService _pipetteService;
        private readonly SampleService _sampleService;
        private readonly ReentrantLockService<IH5uTcp> _lockService;


        public LiquorReliefTrayToTrayArmTool(PipetteService pipetteService, SampleService sampleService, ReentrantLockService<IH5uTcp> lockService)
        {
            _pipetteService = pipetteService;
            _sampleService = sampleService;
            _lockService = lockService;
        }

        private readonly BatchProcessor batchProcessor = new();

        public override bool InitPins()
        {
            InsetPin(InputSignalPinName, this, typeof(QData), PinType.Input);
            InsetPin(OutputSignalPinName, this, typeof(QData), PinType.Output);

            return true;
        }

        public override bool InitDataContext()
        {
            DataContext = new LiquorArmToolData();
            return true;
        }

        public LiquorArmToolData PipetteArmToolData => Context<LiquorArmToolData>();

        [RefParameter<LabTrayTable>]
        public LabTray SourceTray { set; get; }

        [RefParameter<LabTrayTable>]
        public LabTray SourcePosTray { set; get; }

        [RefParameter<LabTrayTable>]
        public LabTray TargetLabtray { set; get; }

        [RefParameter<LabTrayTable>]
        public LabTray TargetPosLabtray { set; get; }


        public override async Task<bool> ExecuteAsync(PinInfo pinInfo, QData pinData, ToolExecutionContext toolContext)
        {
            var batchCount = PipetteArmToolData.BatchCount;

            using (var lockAc = _lockService.Acquire(GetModular().Messenger, this.UniqueId.ToString(), RequestCancelToken))
            {
                if (await batchProcessor.ProcessBatchAsync(batchCount, new Dictionary<string, object>
                {
                    { "pitterParams",PipetteArmToolData },
                    { "sourcePosTray",SourcePosTray },
                    { "targetPosLabtray",TargetPosLabtray },
                    { "sourceTray",SourceTray },
                    { "targetLabtray",TargetLabtray }
                }, RequestCancelToken))
                {
                    SendToPin(OutputSignalPinName, new QData());
                    return true;
                }
            }
            throw new Exception("吸液吐液,托盘到托盘失败");
        }

        public override bool InitEnd()
        {
            batchProcessor.SetProcessStep(async (batch, context) =>
            {
                using (var lockAc = _lockService.Acquire(GetModular().Messenger, this.UniqueId.ToString(), RequestCancelToken))
                {
                    var pitterParams = context["pitterParams"] as LiquorArmToolData;
                    var sourcePosTray = context["sourcePosTray"] as LabTray;
                    var targetPosLabtray = context["targetPosLabtray"] as LabTray;
                    var sourceTray = context["sourceTray"] as LabTray;
                    var targetLabtray = context["targetLabtray"] as LabTray;

                    var liquieType = pitterParams.PitterLiquor;
                    var beginIndex = pitterParams.BeginIndex;


                    var srcWell = sourcePosTray.FindWellByIndex(beginIndex + batch);
                    var (destWell,success) = targetPosLabtray.GetNextWell();
                    var (targerWell,targerSuccess)= targetLabtray.GetNextWell();

                    var src = (QPosition)srcWell.Position.Clone();
                    var dest = (QPosition)destWell.Position.Clone();

                    if (liquieType == LiquorType.Sample)
                    {
                        var qLabware = sourceTray.FindWellByIndex(beginIndex + batch).Labware??throw new Exception("未找到样品的器皿");
                        var material = qLabware.Material??throw new Exception("未找到样品的材料");
                        var materialNo = material.MaterialNo;
                        if (materialNo == 0)
                        {
                           throw new Exception("未找到样品的材料编号");
                        }
                        var sampleTaskInfo = _sampleService.GetSample(materialNo);

                        await _pipetteService.SampleLiquorReliefAsync(UniqueId, sampleTaskInfo, GetModular(), src, dest, Logger);
                        //取出孪生材料
                        var twinMaterial = qLabware.TakeTwinMaterial();
                        Logger.LogInformation("TakeTwinMaterial：{twinMaterial}", twinMaterial);
                        targerWell.PlaceMaterialToTube(twinMaterial.MaterialNo);
                        success(destWell);
                        targerSuccess(targerWell);
                    }
                    else
                    {
                        await _pipetteService.LiquorReliefAsync(GetModular(), src, dest, Logger);
                        success(destWell);
                        targerSuccess(targerWell);

                    }
                }
            });

            return true;

        }
    }


    /// <summary>
    /// 吸吐液_托盘-托盘
    /// </summary>
    [DisplayName("吸吐液_托盘-模块")]
    public class LiquorReliefTrayToModuleArmTool : ModuleWithParameterToolBase
    {

        private const string InputSignalPinName = "输入移液开始信号";
        private const string OutputSignalPinName = "输出移液完成信号";

        public override string DefineName => "吸吐液_托盘-模块";

        private readonly PipetteService _pipetteService;
        private readonly SampleService _sampleService;
        private readonly ReentrantLockService<IH5uTcp> _lockService;


        public LiquorReliefTrayToModuleArmTool(PipetteService pipetteService, SampleService sampleService, ReentrantLockService<IH5uTcp> lockService)
        {
            _pipetteService = pipetteService;
            _sampleService = sampleService;
            _lockService = lockService;
        }

        private readonly BatchProcessor batchProcessor = new();

        public override bool InitPins()
        {
            InsetPin(InputSignalPinName, this, typeof(QData), PinType.Input);
            InsetPin(OutputSignalPinName, this, typeof(QData), PinType.Output);

            return true;
        }

        public override bool InitDataContext()
        {
            DataContext = new LiquorArmToolData();
            return true;
        }

        public LiquorArmToolData PipetteArmToolData => Context<LiquorArmToolData>();

        [RefParameter<LabTrayTable>]
        public LabTray SourceTray { set; get; }

        [RefParameter<LabTrayTable>]
        public LabTray SourcePosTray { set; get; }

        [RefParameter<ModuleChannelGroupTable>]
        public ModuleChannelGroup TargetModuleChannelGroup { set; get; }

        [RefParameter<ModuleChannelGroupTable>]
        public ModuleChannelGroup TargetPosModuleChannelGroup { set; get; }


        public override async Task<bool> ExecuteAsync(PinInfo pinInfo, QData pinData, ToolExecutionContext toolContext)
        {
            var batchCount = PipetteArmToolData.BatchCount;

            using (var lockAc = _lockService.Acquire(GetModular().Messenger, this.UniqueId.ToString(), RequestCancelToken))
            {
                if (await batchProcessor.ProcessBatchAsync(batchCount, new Dictionary<string, object>
                {
                    { "pitterParams",PipetteArmToolData },
                    { "sourcePosTray",SourcePosTray },
                    { "targetPosModuleChannelGroup",TargetPosModuleChannelGroup },
                    { "sourceTray",SourceTray },
                    { "targetModuleChannelGroup",TargetModuleChannelGroup }
                }, RequestCancelToken))
                {
                    SendToPin(OutputSignalPinName, new QData());
                    return true;
                }
            }
            throw new Exception("吸液吐液,托盘到模块失败");
        }

        public override bool InitEnd()
        {
            batchProcessor.SetProcessStep(async (batch, context) =>
            {
                using (var lockAc = _lockService.Acquire(GetModular().Messenger, this.UniqueId.ToString(), RequestCancelToken))
                {
                    var pitterParams = context["pitterParams"] as LiquorArmToolData;
                    var sourcePosTray = context["sourcePosTray"] as LabTray;
                    var targetPosModuleChannelGroup = context["targetPosModuleChannelGroup"] as ModuleChannelGroup;
                    var sourceTray = context["sourceTray"] as LabTray;
                    var targetModuleChannelGroup = context["targetModuleChannelGroup"] as ModuleChannelGroup;

                    var liquieType = pitterParams.PitterLiquor;
                    var beginIndex = pitterParams.BeginIndex;


                    var srcChannel = sourcePosTray.FindWellByIndex(beginIndex + batch);
                    var destChannel = targetPosModuleChannelGroup.GetChannelByIndex(batch+1);

                    var src = (QPosition)srcChannel.Position.Clone();
                    var dest = (QPosition)destChannel.Position.Clone();

                    if (liquieType == LiquorType.Sample)
                    {
                        var qLabware= sourceTray.FindWellByIndex(beginIndex + batch).Labware??throw new Exception("吸液托盘无法找到相关器皿");
                        var material = qLabware.Material??throw new Exception("吸液托盘无法找到相关材料");

                        var sampleId = material.MaterialNo;
                        if (sampleId == 0)
                        {
                          throw new Exception("判断吸液托盘找到的材料ID为0");
                        }

                        var sampleTaskInfo = _sampleService.GetSample(sampleId);

                        await _pipetteService.SampleLiquorReliefAsync(UniqueId, sampleTaskInfo, GetModular(), src, dest, Logger);

                        var twinMaterial = qLabware.TakeTwinMaterial();
                        if (twinMaterial != null)
                        {
                            Logger.LogInformation("TakeTwinMaterial：{material}", twinMaterial);
                            var targerLabware = targetModuleChannelGroup.GetChannelByIndex(batch + 1).Labware ?? throw new Exception("吐液通道中没有找到器皿");
                            targerLabware.PlaceMaterial(twinMaterial);
                        }
                        else
                        {
                            throw new Exception("吸液通道中没有找到，相关物料");
                        }
                    }
                    else
                    {
                        await _pipetteService.LiquorReliefAsync(GetModular(), src, dest, Logger);
                    }
                }
            });

            return true;

        }
    }


    #endregion


    #region 退枪头
    /// <summary>
    /// 退枪头
    /// </summary>
    [DisplayName("退枪头_托盘")]
    public class PipetteReleaseToLabTrayArmTool : ModuleWithParameterToolBase
    {
        private const string PIPETTE_Release_SIGNAL = "退枪头信号";
        private const string PIPETTE_Release_COMPLETE_SIGNAL = "退枪头完成信号";

        public override string DefineName => "退枪头_托盘";

        private readonly PipetteService _pipetteService;
        public PipetteReleaseToLabTrayArmTool(PipetteService pipetteService)
        {
            _pipetteService = pipetteService;
        }
        public override bool InitPins()
        {
            InsetPin(PIPETTE_Release_SIGNAL, this, typeof(QData), PinType.Input);
            InsetPin(PIPETTE_Release_COMPLETE_SIGNAL, this, typeof(QData), PinType.Output);
            return true;
        }
        public override bool InitDataContext()
        {
            DataContext = new PipetteArmToolData();
            return true;
        }

        public PipetteArmToolData PipetteArmToolData => Context<PipetteArmToolData>();

        public override async Task<bool> ExecuteAsync(PinInfo pinInfo, QData data, ToolExecutionContext toolContext)
        {
            if (LabTrayTable == null)
            {
                throw new Exception("退枪头无法找到LabTrayTable托盘");
            }
            QWell well = LabTrayTable.FindFirstEmptyOrTakenWell();
            QPosition temp = (QPosition)((QWell)null).Position.Clone();
            temp.X += PipetteArmToolData.XOffset;
            temp.Y += PipetteArmToolData.YOffset;
            temp.Z += PipetteArmToolData.ZOffset;
            temp.Z2 += PipetteArmToolData.Z2Offset;
            Logger.LogInformation("移液枪退枪头，目标位置：{position}", temp);

            await _pipetteService.PipetteAsync(GetModular(), temp, PipetteType.PipetteRelease, Logger);
            SendToPin(PIPETTE_Release_COMPLETE_SIGNAL, new QData());
            well.Return();
            return true;
        }


        [RefParameter<LabTrayTable>]
        public LabTray LabTrayTable { set; get; }
    }


    /// <summary>
    /// 退枪头废料框
    /// </summary>
    [DisplayName("退枪头")]
    public class PipetteReleaseArmTool : SyncInputModuleWithParameterToolBase
    {
        private const string PIPETTE_Release_SIGNAL = "取枪头信号";
        private const string PIPETTE_Release_COMPLETE_SIGNAL = "取枪头完成信号";

        public override string DefineName => "退枪头";

        private readonly PipetteService _pipetteService;
        public PipetteReleaseArmTool(PipetteService pipetteService)
        {
            _pipetteService = pipetteService;
        }
        public override bool InitPins()
        {
            InsetPin(PIPETTE_Release_SIGNAL, this, typeof(QData), PinType.Input);
            InsetPin(PIPETTE_Release_COMPLETE_SIGNAL, this, typeof(QData), PinType.Output);
            return true;
        }
        public override bool InitDataContext()
        {
            DataContext = new PipetteArmToolData();
            return true;
        }

        public PipetteArmToolData PipetteArmToolData => Context<PipetteArmToolData>();

        public override async Task<bool> ExexuteAsync(Dictionary<PinInfo, QData> pinDatas, ToolExecutionContext toolContext)
        {
            if (ReleaseChannel == null)
            {
              throw new Exception("退枪头无法找到ReleaseChannel通道");
            }
            var qChannelSlot = ReleaseChannel.GetChannelByIndex(1);
            QPosition temp = (QPosition)qChannelSlot.Position.Clone();

            temp.X += PipetteArmToolData.XOffset;
            temp.Y += PipetteArmToolData.YOffset;
            temp.Z += PipetteArmToolData.ZOffset;
            temp.Z2 += PipetteArmToolData.Z2Offset;
            Logger.LogInformation("移液枪退枪头，目标位置：{position}", temp);
            await _pipetteService.PipetteAsync(GetModular(), temp, PipetteType.PipetteRelease, Logger);
            SendToPin(PIPETTE_Release_COMPLETE_SIGNAL, new QData());
            return true;
        }

        [RefParameter<ModuleChannelGroupTable>]
        public ModuleChannelGroup ReleaseChannel { set; get; }
    }
    #endregion


    #region 取枪头
    /// <summary>
    /// 取枪头
    /// </summary>
    [DisplayName("取枪头")]
    public class PipettePickArmTool : ModuleWithParameterToolBase, ILabTrayService
    {
        private const string PIPETTE_PICK_SIGNAL = "取枪头信号";

        private const string PIPETTE_PICK_COMPLETE_SIGNAL = "取枪头完成信号";


        public override string DefineName => "取枪头";

        private readonly PipetteService _pipetteService;
        public PipettePickArmTool(PipetteService pipetteService)
        {
            _pipetteService = pipetteService;
        }

        public override bool InitPins()
        {
            InsetPin(PIPETTE_PICK_SIGNAL, this, typeof(QData), PinType.Input);

            InsetPin(PIPETTE_PICK_COMPLETE_SIGNAL, this, typeof(QData), PinType.Output);
            return true;
        }
        public override bool InitDataContext()
        {
            DataContext = new PipetteArmToolData();
            return true;
        }
        public PipetteArmToolData PipetteArmToolData => Context<PipetteArmToolData>();
        public override async Task<bool> ExecuteAsync(PinInfo pinInfo, QData pinData, ToolExecutionContext toolContext)
        {
            var well = TipLabTray.FindFirstLoadWellWithMaterial();
            var temp = (QPosition)well.Position.Clone();
            temp.X += PipetteArmToolData.XOffset;
            temp.Y += PipetteArmToolData.YOffset;
            temp.Z += PipetteArmToolData.ZOffset;
            temp.Z2 += PipetteArmToolData.Z2Offset;
            Logger.LogInformation("移液枪取枪头，目标位置：{position}", temp);
            await _pipetteService.PipetteAsync(GetModular(), temp, PipetteType.PipettePick, Logger);
            var labware = well.Take();
            Logger.LogInformation("移液枪取枪头，取到枪头：{labware}", labware);
            SendToPin(PIPETTE_PICK_COMPLETE_SIGNAL, new QData());
            return true;

        }

        [RefParameter<LabTrayTable>]
        public LabTray TipLabTray { set; get; }

        public List<LabTray> LabTrays =>
        [
            TipLabTray
        ];
    }

    #endregion


    #region 引用类
    /// <summary>
    /// 移液参数
    /// </summary>
    public class LiquorArmToolData : PipetteArmToolData
    {
        [DisplayName("液体种类")]
        public LiquorType PitterLiquor { get; set; } = LiquorType.Sample;

        [DisplayName("批量数")]
        public int BatchCount { set; get; } = 1;

        [DisplayName("起始Index")]
        public int BeginIndex { set; get; } = 0;
    }
    /// <summary>
    /// 移液类型
    /// </summary>
    public enum LiquorType
    {
        /// <summary>
        /// 样品移液
        /// </summary>
        Sample,
        /// <summary>
        /// 耗材溶剂移液
        /// </summary>
        Material
    }
    #endregion



}

