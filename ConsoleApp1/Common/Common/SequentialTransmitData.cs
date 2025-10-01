namespace QStandaedPlatform.Engine.Common.Common
{
    /// <summary>
    /// 有序的传输数据对象
    /// </summary>
    public class SequentialTransmitData
    {
        public int Sequential { get; set; }

        public Guid  DataId { get; set; }

        public PinDataTransmitEventArgs  PinDataTransmitEventArgs { get; set; }

        public DateTime CreateTime { get; set; } = DateTime.Now;
    }
}
