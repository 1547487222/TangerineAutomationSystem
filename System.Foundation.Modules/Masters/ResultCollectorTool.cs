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
    public class ResultCollectItem
    {
        public string Key { get; set; }

        public DataType ValueType { get; set; }

        public bool IsArrayValue { get; set; }
    }
    public class ResultCollectorData : DynamicPinToolData
    {
        public bool IncludeFlowName { get; set; } = false;

        public bool IncludeWorkId { get; set; } = false;
        public List<ResultCollectItem> CollectedResults { get; set; } = [];
    }
    [DisplayName("结果收集器")]
    public class ResultCollectorTool : DynamicSyncInputPinTool
    {
       
        public override string DefineName => "结果收集器";



        public override bool InitPins()
        {
            InsetPin("输出收集结果",this,typeof(QFlowResult), PinType.Output);
            return true;
        }

        public override bool InitDataContext()
        {
            DataContext = new ResultCollectorData() { IsUpdateOutput = false };
            return true;
        }
        public override Task<bool> ExexuteAsync(Dictionary<PinInfo, QData> pinDatas, ToolExecutionContext toolContext)
        {
            var flowResult = new QFlowResult();
            var resultCollectorData = Context<ResultCollectorData>();
            if (resultCollectorData.IncludeFlowName)
            {
                flowResult.FlowName = (string)pinDatas.First(p => p.Key.Name == "FlowName").Value;
            }
            else
            {
                flowResult.FlowName = this.DisplayName;
            }
            if (resultCollectorData.IncludeWorkId)
            {
                flowResult.WorkId = (string)pinDatas.First(p => p.Key.Name == "WorkId").Value;
            }
            foreach (var item in resultCollectorData.CollectedResults)
            {
                var pinData = pinDatas.FirstOrDefault(p => p.Key.Name == item.Key).Value;
                if (pinData != null)
                {
                    flowResult.Datas[item.Key] = pinData;
                }
            }
            SendToPin("输出收集结果", flowResult);
            return Task.FromResult(true);
        }

        public override void HandleRequestAddNewPin(object dynamicPinData)
        {
            if (dynamicPinData is ResultCollectorData resultCollectorData)
            {
                if (resultCollectorData.IncludeFlowName)
                {
                    InsetPin("FlowName", this, typeof(QString), PinType.Input, true);
                }
                if (resultCollectorData.IncludeWorkId)
                {
                    InsetPin("WorkId", this, typeof(QString), PinType.Input, true);
                }
                foreach (var item in resultCollectorData.CollectedResults)
                {
                    if (item.IsArrayValue)
                    {
                        switch (item.ValueType)
                        {
                            case DataType.QInt16:
                                InsetPin(item.Key, this, typeof(QArray<QInt16>), PinType.Input, true);
                                break;
                            case DataType.QInt:
                                InsetPin(item.Key, this, typeof(QArray<QInt>), PinType.Input, true);
                                break;
                            case DataType.QFloat:
                                InsetPin(item.Key, this, typeof(QArray<QFloat>), PinType.Input, true);
                                break;
                            case DataType.QDouble:
                                InsetPin(item.Key, this, typeof(QArray<QDouble>), PinType.Input, true);
                                break;
                            case DataType.QDateTime:
                                InsetPin(item.Key, this, typeof(QArray<QDateTime>), PinType.Input, true);
                                break;
                            case DataType.QString:
                                InsetPin(item.Key, this, typeof(QArray<QString>), PinType.Input, true);
                                break;
                            case DataType.QBoolean:
                                InsetPin(item.Key, this, typeof(QArray<QBoolean>), PinType.Input, true);
                                break;
                        }
                    }
                    else
                    {
                        switch (item.ValueType)
                        {
                            case DataType.QInt16:
                                InsetPin(item.Key, this, typeof(QInt16), PinType.Input, true);
                                break;
                            case DataType.QInt:
                                InsetPin(item.Key, this, typeof(QInt), PinType.Input, true);
                                break;
                            case DataType.QFloat:
                                InsetPin(item.Key, this, typeof(QFloat), PinType.Input, true);
                                break;
                            case DataType.QDouble:
                                InsetPin(item.Key, this, typeof(QDouble), PinType.Input, true);
                                break;
                            case DataType.QDateTime:
                                InsetPin(item.Key, this, typeof(QDateTime), PinType.Input, true);
                                break;
                            case DataType.QString:
                                InsetPin(item.Key, this, typeof(QString), PinType.Input, true);
                                break;
                            case DataType.QBoolean:
                                InsetPin(item.Key, this, typeof(QBoolean), PinType.Input, true);
                                break;
                            case DataType.QBinary:
                                InsetPin(item.Key, this, typeof(QBinary), PinType.Input, true);
                                break;
                            case DataType.QPosition:
                                InsetPin(item.Key, this, typeof(QPosition), PinType.Input, true);
                                break;
                            case DataType.QDynamic:
                                InsetPin(item.Key, this, typeof(QDynamic), PinType.Input, true);
                                break;
                            case DataType.QJson:
                                InsetPin(item.Key, this, typeof(QJson), PinType.Input, true);
                                break;
                        }
                    }
                }
            }
        }
    }
}
