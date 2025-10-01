using QStandaedPlatform.Engine.Common.Common.Metadatas;
using QStandaedPlatform.Engine.Common.Common;
using QStandaedPlatform.Engine.Components;
using QStandaedPlatform.Engine.Laboratory.Documents;
using QStandaedPlatform.Engine.Laboratory;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Foundation.Modules.Arms
{
    /// <summary>
    /// 批耗材架定点上料模块
    /// </summary>
    [DisplayName(BatchMaterialTrayModRepeatArmToolName)]
    public class BatchMaterialTrayModRepeatArmTool : ArmModuleToolBase, ILabTrayService
    {
        public const string BatchMaterialTrayModRepeatArmToolName = "批耗材架定点上料模块";
        private const string START_LOADING_SIGNAL = "开始上料信号";
        private const string OUTPUT_SIGNAL_PIN = "输出完成信号";
        public override string DefineName => BatchMaterialTrayModRepeatArmToolName;

        private readonly BatchProcessor batchProcessor = new();

        private readonly ReentrantLockService<IH5uTcp> _lockService;
        private readonly ArmService _armService;
        public BatchMaterialTrayModRepeatArmTool(ReentrantLockService<IH5uTcp> lockService, ArmService armService)
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

        public override bool InitEnd()
        {
            batchProcessor.SetProcessStep(async (batch, context) =>
            {
                {
                    var materialLabTray = context["MaterialLabTray"] as LabTray;
                    var materialLabTray2 = context["MaterialLabTray2"] as LabTray;
                    var moduleChannelGroup = context["ModuleChannelGroup"] as ModuleChannelGroup;
                    var beginIndex = Convert.ToInt32(context["BeginIndex"] ?? 0);
                    var well = materialLabTray.FindWellByIndexAllowNull(batch + beginIndex) ?? materialLabTray2.FindWellByIndex(batch + beginIndex);
                    var claw = new ClawModel
                    {
                        FromOpenPos = well.ClawSetting.OpenPos,
                        FromAngle = well.ClawSetting.Angle,
                        ToAngle = moduleChannelGroup.ClawSetting.Angle,
                        ToOpenPos = moduleChannelGroup.ClawSetting.OpenPos
                    };
                    var qChannelSlot = moduleChannelGroup.GetIdleChannel();
                    await _armService.ArmTransport((QPosition)well.Position.Clone(), (QPosition)qChannelSlot.Position.Clone(), claw,Context<BatchTrayModArmData>(), GetModular(), RequestCancelToken);
                    qChannelSlot.Put(well.Take());
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
                { "MaterialLabTray2",MaterialLabTray2 },
                { "BeginIndex", Context<BatchTrayModArmData>().BeginIndex}
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



    public class ClawModel
    {
        /// <summary>
        /// 取料开合度
        /// </summary>
        public float FromOpenPos { get; set; } = 0;
        /// <summary>
        /// 取料角度
        /// </summary>
        public float FromAngle { get; set; } = 0;


        /// <summary>
        /// 放料开合度
        /// </summary>
        public float ToOpenPos { get; set; } = 0;
        /// <summary>
        /// 放料角度
        /// </summary>
        public float ToAngle { get; set; } = 0;
    }
}
