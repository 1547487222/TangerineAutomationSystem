using QStandaedPlatform.Engine.Common.Common;
using QStandaedPlatform.Engine.Common.Common.SampleEntitys;

namespace QStandaedPlatform.Engine.Laboratory
{
    public interface ISamplePreInjectService
    {
        void PreInject();
    }

    public interface IPreInjectEndService
    {
        void WaitPreInjectEnd();
    }

    /// <summary>
    /// 进样服务
    /// </summary>
    public interface ISampleInjectService
    {
        /// <summary>
        /// 进样
        /// </summary>
        /// <param name="samplingTaskInfos"></param>
        void InjectSample(InjectSamplingModel[]  injectSamplings);
    }

    public class InjectSamplingModel(SampleTaskInfo sampleTaskInfo)
    {
        public SampleTaskInfo SampleInfo { get; } = sampleTaskInfo ?? throw new ArgumentNullException(nameof(sampleTaskInfo));

        public Action<SampleTraceEntity>? SampleTraceAction { get; set; }

        public Action<SampleTaskInfo>? SampleCompleteAction { get; set; }
    }

    /// <summary>
    /// 进样任务信息
    /// </summary>
    public class SampleTaskInfo: ContentBasedHashableObject
    {
        /// <summary>
        /// 工艺流程ID
        /// </summary>
        public string ProcessflowId { get; set; } = string.Empty;
        /// <summary>
        /// 平台ID
        /// </summary>
        public long PlatformId { get; set; }

        /// <summary>
        /// 平台名称
        /// </summary>
        public string PlatformName { get; set; } = string.Empty;
        /// <summary>
        /// 平台任务ID
        /// </summary>
        public long PlatformTaskId { get; set; }
        /// <summary>
        /// 任务Id
        /// </summary>
        public long SamplingTaskId { get; set; }
        /// <summary>
        /// 样品ID
        /// </summary>
        public long SamplingId { get; set; }
        /// <summary>
        /// 样品名称
        /// </summary>
        public string SampleName { get; set; } = string.Empty;
        /// <summary>
        /// 样品类型
        /// </summary>
        public string SampleType { get; set; } = string.Empty;
        /// <summary>
        /// 样品备注
        /// </summary>
        public string SampleRemarks { get; set; } = string.Empty;
        /// <summary>
        /// 托盘ID
        /// </summary>
        public long TrayId { get; set; }
        /// <summary>
        /// 孔位ID
        /// </summary>
        public long WellId { get; set; }
        /// <summary>
        /// 孔位名称
        /// </summary>
        public string WellName { get; set; } = string.Empty;


        public override string ToString()
        {
            return $"SampleName:{SampleName},SampleType:{SampleType},SampleRemarks:{SampleRemarks},WellName:{WellName},TrayId:{TrayId},WellId:{WellId}";
        }
    }
}
