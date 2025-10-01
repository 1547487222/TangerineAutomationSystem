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
    [DisplayName("Int条件判断工具")]
    public class IntConditionJudgerTool : DynamicSyncInputPinTool
    {
        public enum Symbol
        {
            moreThan,
            lessThan,
            equal,
        }
        public class IntConditionJudgerData : DynamicPinToolData
        {
            /// <summary>
            ///Int判断条件
            /// </summary>
            [DisplayName("Int判断项")]
            public List<int> Conditions { get; set; } = [];
        }
        public override string DefineName => "Int条件判断工具";

        public override bool InitPins()
        {
            InsetPin("输入判断值", this, typeof(QInt), PinType.Input);
            InsetPin("附加值", this, typeof(QDynamic), PinType.Input);
            return base.InitPins();
        }
        public override bool InitDataContext()
        {
            DataContext = new IntConditionJudgerData() { IsUpdateInput = false };
            return true;
        }

        public override Task<bool> ExexuteAsync(Dictionary<PinInfo, QData> pinDatas, ToolExecutionContext toolContext)
        {
            var intConditionJudgerData = Context<IntConditionJudgerData>();
            var value = (QInt)pinDatas.FirstOrDefault(p => p.Key.Name == "输入判断值").Value;
            //获取附加值
            var data= pinDatas.FirstOrDefault(p => p.Key.Name == "附加值").Value;
            foreach (var item in intConditionJudgerData.Conditions)
            {
                if (value == item)
                {
                    SendToPin($"Int判断项_{item}", data);
                }
            }
            return Task.FromResult(true);
        }

        public override void HandleRequestAddNewPin(object dynamicPinData)
        {
            if (dynamicPinData is IntConditionJudgerData intConditionJudgerData)
            {
                if (intConditionJudgerData.Conditions.Count > 0)
                {
                    foreach (var item in intConditionJudgerData.Conditions)
                    {
                        InsetPin($"Int判断项_{item}", this, typeof(QDynamic), PinType.Output, true);
                    }
                }
            }
        }
    }
}
