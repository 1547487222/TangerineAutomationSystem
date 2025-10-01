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
    public class SynchronizeWaitData : DynamicPinToolData
    {
        public int SynchronizeWaitCount { get; set; } = 2;
    }
    [DisplayName("同步等待工具")]
    public class SynchronizeWaitTool : DynamicSyncInputPinTool
    {
        public override string DefineName => "同步等待工具";

        public override bool InitDataContext()
        {
            DataContext = new SynchronizeWaitData() { UpdateInputIndex = 0 };
            return true;
        }
        public override Task<bool> ExexuteAsync(Dictionary<PinInfo, QData> pinDatas, ToolExecutionContext toolContext)
        {
            for (int i = 0; i < Context<SynchronizeWaitData>().SynchronizeWaitCount; i++)
            {
                var data = pinDatas.FirstOrDefault(p => p.Key.Name == $"输入同步数据源{i + 1}").Value;
                SendToPin($"输出同步数据源{i + 1}", data);
            }

            return Task.FromResult(true);
        }

        public override void HandleRequestAddNewPin(object dynamicPinData)
        {
            if (dynamicPinData is SynchronizeWaitData synchronizeWaitData)
            {
                for (int i = 0; i < synchronizeWaitData.SynchronizeWaitCount; i++)
                {
                    InsetPin($"输入同步数据源{i + 1}", this, typeof(QDynamic), PinType.Input, true);
                    InsetPin($"输出同步数据源{i + 1}", this, typeof(QDynamic), PinType.Output, true);
                }
            }
        }
    }
}
