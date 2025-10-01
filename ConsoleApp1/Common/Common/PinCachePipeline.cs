namespace QStandaedPlatform.Engine.Common.Common
{
    public class PinCachePipeline : IDisposable
    {
        private readonly PinInfo _pinInfo;
        private readonly Dictionary<Guid,SequentialTransmitData> sequentialTransmitDatas = [];
        public PinCachePipeline(PinInfo pinInfo)
        {
            _pinInfo = pinInfo;
            _pinInfo.OnPinDataTransmit += PinInfo_OnPinDataTransmit;
        }

        private Task PinInfo_OnPinDataTransmit(PinDataTransmitEventArgs e)
        {
            lock (sequentialTransmitDatas)
            {
                SequentialTransmitData sequentialTransmitData = new()
                {
                    Sequential = sequentialTransmitDatas.Count,
                    DataId = Guid.NewGuid(),
                    PinDataTransmitEventArgs = e
                };
                sequentialTransmitDatas[sequentialTransmitData.DataId] = sequentialTransmitData;
            }
            OnDataTransmitJoin?.Invoke(this, EventArgs.Empty);
            return Task.CompletedTask;
        }


        public PinInfo PinInfo => _pinInfo;

        public event EventHandler? OnDataTransmitJoin;

        public SequentialTransmitData? GetTransmitData()
        {
            lock (sequentialTransmitDatas)
            {
                if (sequentialTransmitDatas.Count > 0)
                    return sequentialTransmitDatas.OrderBy(p => p.Value.Sequential).First().Value;
                else
                    return default;
            }
        }

        public void RemoveTransmitData(SequentialTransmitData sequentialTransmitData)
        {
            lock (sequentialTransmitDatas)
            {
                if (sequentialTransmitData != null)
                    sequentialTransmitDatas.Remove(sequentialTransmitData.DataId);
            }
        }

        public void Dispose()
        {
            lock (sequentialTransmitDatas)
            {
                _pinInfo.OnPinDataTransmit -= PinInfo_OnPinDataTransmit;
                sequentialTransmitDatas.Clear();
            }
        }
    }
}
