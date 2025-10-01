using QStandaedPlatform.Engine.Common.Common.Metadatas;
using QStandaedPlatform.Engine.Laboratory.Documents;

namespace QStandaedPlatform.Engine.Laboratory
{
    /// <summary>
    /// 模块通道槽位
    /// </summary>
    public class QChannelSlot : QData, ISampleStow
    {
        public Guid OwnerId { get; set; }

        public Guid ChannelId { get; set; }

        public int Slot { get; set; }

        public QLabware Labware { get; set; }

        public SlotStatus Status { get; set; } = SlotStatus.Idle;

        public QPosition Position { get; set; }

        public ClawGraspInfo ClawSetting { get; set; }

        public QLabware Take()
        {
            if (Status == SlotStatus.Idle)
                throw new Exception("通道没有装载Labware");
            Status = SlotStatus.Idle;
            return Labware;
        }
        public void Put(QLabware labware)
        {
            if (Status == SlotStatus.Working)
                throw new Exception($"通道正在工作，不能装载Labware{labware}");
            Status = SlotStatus.Working;
            Labware = labware;
        }

        public override string ToString()
        {
            return @$"ChannelSlot:{ChannelId} Slot:{Slot} Status:{Status}
                Labware:{Labware} Position:{Position} ClawSetting:{ClawSetting}";
        }
    }

}
