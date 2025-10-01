namespace QStandaedPlatform.Engine.Common.Common
{
    public class FlowFileDescription
    {
        public Guid FlowId { get; set; }

        public string FlowDescription { get; set; }

        public string FilePath { get; set; }

        public DateTime CreateTime { get; set; } = DateTime.Now;
    }
}
