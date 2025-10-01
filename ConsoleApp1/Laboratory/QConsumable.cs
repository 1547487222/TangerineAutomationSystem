namespace QStandaedPlatform.Engine.Laboratory
{
    public class QConsumable
    {
        /// <summary>
        /// 流程ID
        /// </summary>
        public string ProcessId { get; set; } = string.Empty;
        /// <summary>
        /// 进样任务ID
        /// </summary>
        public string SamplingTaskId { get; set; } = string.Empty;

        /// <summary>
        /// 平台流程ID
        /// </summary>
        public string PlatformflowId { get; set; } = string.Empty;
        /// <summary>
        /// 耗材ID
        /// </summary>
        public long ConsumableId { get; set; }
        /// <summary>
        /// 耗材孔位编号
        /// </summary>
        public string ConsumableCode { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"ProcessId:{ProcessId},SamplingTaskId:{SamplingTaskId},ConsumableId:{ConsumableId},ConsumableCode:{ConsumableCode}";
        }
    }
}