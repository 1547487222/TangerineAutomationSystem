using Newtonsoft.Json;
using QStandaedPlatform.Engine.Common.Common;
using QStandaedPlatform.Engine.Common.Common.Metadatas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Laboratory.Documents
{
    public class ModuleChannelGroup : IParameter, ICloneable
    {
        private readonly Dictionary<int, QChannelSlot> _channels = [];
        private volatile int _channelIndex = 0;
        public Guid ModuleInfoId { get; set; }
        [JsonIgnore]
        public ModuleInfoParameter? ModuleInfoParameter { get; set; }
        public string ModuleChannelGroupName { get; set; } = string.Empty;
        public bool RowFirst { get; set; } = true;
        public int Rows { get; set; }
        public int Cols { get; set; }
        public float SpaceRow { get; set; }
        public float SpaceCol { get; set; }
        public ClawGraspInfo ClawSetting { get; set; } = new();
        public QPosition StartPosition { get; set; } = new();
        public int ChannelCount => Rows * Cols;
        public Guid ParameterId { set; get; }

        public void InitlizeParameter()
        {
            _channelIndex = 0;
            InitSlots();
        }
        public void InitSlots()
        {
            _channels.Clear();
            int index = 0;
            if (RowFirst)
            {
                for (int row = 0; row < Rows; row++)
                {
                    for (int col = 0; col < Cols; col++)
                    {
                        float x = StartPosition.X + col * SpaceCol;
                        float y = StartPosition.Y + row * SpaceRow;
                        var slot = ++index;
                        var port = CreateSlot(slot, x, y);
                        _channels[slot] = port;
                    }
                }
            }
            else
            {
                for (int col = 0; col < Cols; col++)
                {
                    for (int row = 0; row < Rows; row++)
                    {
                        float x = StartPosition.X + col * SpaceCol;
                        float y = StartPosition.Y + row * SpaceRow;
                        var slot = ++index;
                        var port = CreateSlot(slot, x, y);
                        _channels[slot] = port;
                    }
                }
            }
        }

        private QChannelSlot CreateSlot(int slot, float x, float y)
        {
            return new QChannelSlot
            {
                Status = SlotStatus.Idle,
                OwnerId = ParameterId,
                Slot = slot,
                Position = new QPosition
                {
                    X = x,
                    Y = y,
                    Z = StartPosition.Z,
                    Z2 = StartPosition.Z2,
                    Angle = StartPosition.Angle,
                    Depth = StartPosition.Depth,
                    ZPutGetOffset = StartPosition.ZPutGetOffset,
                },

                ClawSetting = new ClawGraspInfo
                {
                    Angle = ClawSetting.Angle,
                    OpenPos = ClawSetting.OpenPos,
                }
            };
        }

        public List<QChannelSlot> GetAllPorts() => [.. _channels.Values];

        public QChannelSlot GetChannelByIndex(int index)
        {
            return _channels[index];
        }

        /// <summary>
        /// 获取下一个通道
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public (QChannelSlot slot,Action<QChannelSlot> action) GetNextChannel()
        {
            var modulePort = _channels.Values.OrderBy(p => p.Slot);
            if (_channelIndex >= modulePort.Count())
            {
                Interlocked.Exchange(ref _channelIndex, 0);
            }
            var channel = modulePort.ElementAtOrDefault(_channelIndex) ?? throw new Exception($"{nameof(GetNextChannel)}无法取出{_channelIndex}通道");
            return (channel, channel =>
            {
                Interlocked.Increment(ref _channelIndex);
            }
            );
        }

        public QChannelSlot GetQChannelSlotByMaterialId(long materialid)
        {
            var channelSlot = _channels.Values.FirstOrDefault(p => p.Labware?.Material?.MaterialNo == materialid);
            return channelSlot ?? throw new Exception($"{nameof(GetQChannelSlotByMaterialId)}无法找到{materialid}所在的槽位");
        }

        public QChannelSlot GetIdleChannel()
        {
            var modulePort = _channels.Values.OrderBy(p => p.Slot)
                .Where(p => p.Status == SlotStatus.Idle)
                .FirstOrDefault();
            return modulePort ?? throw new Exception($"{nameof(GetIdleChannel)}没有空闲通道");
        }
        /// <summary>
        /// 倒序查找
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public QChannelSlot GetIdleChannelReverse()
        {
            var modulePort = _channels.Values.OrderByDescending(p => p.Slot)
                .Where(p => p.Status == SlotStatus.Idle)
                .FirstOrDefault();
            return modulePort ?? throw new Exception($"{nameof(GetIdleChannelReverse)}没有空闲通道");
        }

        public QChannelSlot GetWorkingChannel()
        {
            var modulePort = _channels.Values.OrderBy(p => p.Slot)
                .Where(p => p.Status == SlotStatus.Working)
                .FirstOrDefault() ?? throw new Exception($"{nameof(GetWorkingChannel)}没有正在工作的通道");
            return modulePort;
        }
        /// <summary>
        /// 倒序查找
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public QChannelSlot GetWorkingChannelReverse()
        {
            var modulePort = _channels.Values.OrderByDescending(p => p.Slot)
                .Where(p => p.Status == SlotStatus.Working)
                .FirstOrDefault();
            return modulePort ?? throw new Exception($"{nameof(GetWorkingChannelReverse)}没有正在工作的通道");
        }


        public void ResetChannelIndex()
        {
            Interlocked.Exchange(ref _channelIndex, 0);
        }

        public object Clone()
        {
            var clone = new ModuleChannelGroup
            {
                ModuleInfoId = ModuleInfoId,
                ModuleInfoParameter = ModuleInfoParameter,
                ModuleChannelGroupName = ModuleChannelGroupName,
                RowFirst = RowFirst,
                Rows = Rows,
                Cols = Cols,
                SpaceRow = SpaceRow,
                SpaceCol = SpaceCol,
                StartPosition = (QPosition)StartPosition.Clone(),
                ParameterId = ParameterId
            };
            return clone;
        }

        
    }
}



