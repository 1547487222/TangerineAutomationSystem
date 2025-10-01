using QStandaedPlatform.Engine.Common.Common;
using QStandaedPlatform.Engine.Common.Common.Metadatas;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Foundation.Modules.Masters
{
    public class ScopelocktData : DynamicPinToolData
    {
        [DisplayName("锁资源数")]
        public int ScopelockResources { get; set; } = 1;

        //资源锁Key
        [DisplayName("资源锁Key")]
        public string ScopelockKey { get; set; } = string.Empty;
    }
    [DisplayName("创建资源锁")]
    public class CreateScopelockTool : DynamicSyncInputPinTool
    {
        public override string DefineName => "创建资源锁";

        public override bool InitDataContext()
        {
            DataContext = new ScopelocktData() { UpdateInputIndex = 0 };
            return true;
        }
        private readonly DistributedLockManager _distributedLockManager;
        public CreateScopelockTool(DistributedLockManager distributedLockManager)
        {
            _distributedLockManager = distributedLockManager;
        }
        public override Task<bool> ExexuteAsync(Dictionary<PinInfo, QData> pinDatas, ToolExecutionContext toolContext)
        {
            //if(!_distributedLockManager.IsDistributedLock(Context<ScopelocktData>().ScopelockKey))
            {
                _distributedLockManager.CreateDistributedLock(Context<ScopelocktData>().ScopelockKey, values: [.. pinDatas.OrderBy(p => p.Key.Name).ToDictionary().Values]);

            }
          
            for (int i = 0; i < Context<ScopelocktData>().ScopelockResources; i++)
            {
                var data = pinDatas.FirstOrDefault(p => p.Key.Name == $"输入锁资源{i + 1}").Value;
                SendToPin($"输出锁资源{i + 1}", data);
            }
            
            
            return Task.FromResult(true);
        }

        public override void HandleRequestAddNewPin(object dynamicPinData)
        {
            if (dynamicPinData is ScopelocktData synchronizeWaitData)
            {
                for (int i = 0; i < synchronizeWaitData.ScopelockResources; i++)
                {
                    InsetPin($"输入锁资源{i + 1}", this, typeof(QDynamic), PinType.Input, true);
                    InsetPin($"输出锁资源{i + 1}", this, typeof(QDynamic), PinType.Output, true);
                }
            }
        }
    }


    //释放资源锁
    [DisplayName("释放资源锁")]
    public class ReleaseScopelockTool : DynamicSyncInputPinTool
    {
        public override string DefineName => "释放资源锁";

        private readonly DistributedLockManager _distributedLockManager;
        public ReleaseScopelockTool(DistributedLockManager distributedLockManager)
        {
            _distributedLockManager = distributedLockManager;
        }

        public override bool InitPins()
        {
            InsetPin("输入完成信号", this, typeof(QData), PinType.Input);
            return true;
        }
        public override bool InitDataContext()
        {
            DataContext = new ScopelocktData() { IsUpdateInput = false };
            return true;
        }

        public override async Task<bool> ExexuteAsync(Dictionary<PinInfo, QData> pinDatas, ToolExecutionContext toolContext)
        {
           var distributedData= _distributedLockManager.ReleaseDistributedLock(Context<ScopelocktData>().ScopelockKey);
            if (distributedData != null)
            {
                foreach (var item in distributedData.Values)
                {
                    SendToPin($"输出锁资源{distributedData.Values.IndexOf(item) + 1}", item);
                }
            }
            return await Task.FromResult(true);
        }

        public override void HandleRequestAddNewPin(object dynamicPinData)
        {
            if (dynamicPinData is ScopelocktData synchronizeWaitData)
            {
                for (int i = 0; i < synchronizeWaitData.ScopelockResources; i++)
                {
                    InsetPin($"输出锁资源{i + 1}", this, typeof(QDynamic), PinType.Output, true);
                }
            }
        }
    }
}
