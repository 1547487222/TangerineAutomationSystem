using QStandaedPlatform.Engine.Common.Common.Metadatas;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common
{
    public abstract class SyncInputToolBase : Tool
    {
        private const string InputAbandonSignal = "输入丢弃信号";
        public ConcurrentDictionary<PinInfo, Queue<PinDataTransmitEventArgs>> CacheRecvDatas = new();
        public override bool InitPins()
        {
            InsetPin(InputAbandonSignal, this, typeof(QData), PinType.Input);
            return base.InitPins();
        }
        public void InitCache()
        {
            foreach (var pin in InputPins)
            {
                if (pin.Name == InputAbandonSignal)
                {
                    continue;
                }
                CacheRecvDatas[pin] = new Queue<PinDataTransmitEventArgs>();
            }
        }
        public override async  Task<bool> RequestRecvHandlePinAsync(ToolExecutionContext toolContext, PinDataTransmitEventArgs pinDataTransmitEventArgs)
        {
           await toolContext.SyncInputLock.WaitAsync();
            try
            {
                if (pinDataTransmitEventArgs.TargetPin.Name == InputAbandonSignal)
                {
                    ClearCacheData();
                    return true;
                }

                if (CacheRecvDatas.TryGetValue(pinDataTransmitEventArgs.TargetPin, out var queue) && await toolContext.VerifyToolStateAsync())
                {
                    if (await toolContext.VerifyToolStateAsync())
                    {
                        lock (queue)
                        {

                            queue.Enqueue(pinDataTransmitEventArgs);
                        }
                    }


                    lock (CacheRecvDatas)
                    {
                        if (CacheRecvDatas.All(kv => kv.Value.Count > 0) )
                        {
                            if (this.ToolExecuter is SyncInputToolExecuter syncInputToolExecuter)
                            {
                                var pinDatas = CacheRecvDatas.ToDictionary(kv => kv.Key, kv => kv.Value.Dequeue());
                                syncInputToolExecuter.Enqueue(pinDatas);
                            }
                        }
                        else
                        {
                            _ = RaiseToolStateChangeAsync(ToolState.SyncWaiting);
                        }
                    }
                }
                else
                {
                    await RaiseToolStateChangeAsync(ToolState.Error, "未找到缓存队列");
                }
            }
            finally 
            {
                toolContext.SyncInputLock.Release();
            }
            return true;
        }

        public override async Task<CommandResult> ExecuteCommandAsync(ITriggerPointCommand triggerPointCommand)
        {
            if (triggerPointCommand.Id == 100)
            {
                ClearCacheData();
               await RaiseToolStateChangeAsync(ToolState.None);
            }
            return await base.ExecuteCommandAsync(triggerPointCommand);
        }
        public void ClearCacheData()
        {
            lock (CacheRecvDatas)
            {
                foreach (var queue in CacheRecvDatas.Values)
                {
                    queue.Clear();
                }
            }
        }

        public void ClearCache()
        {
            lock (CacheRecvDatas)
            {
                CacheRecvDatas.Clear();
            }
        }

        public override Task<bool> OnHandleRequestCancelAsync()
        {
            return Task.FromResult(true);
        }

        public override Task<bool> ClearEphemeralDataAsync()
        {
            ClearCacheData();
            ToolExecutionContext.ClearPinCache();
            return Task.FromResult(true);
        }
        public abstract Task<bool> ExexuteAsync(Dictionary<PinInfo, QData> pinDatas, ToolExecutionContext toolContext);
    }
}
