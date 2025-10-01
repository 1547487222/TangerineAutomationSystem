using QStandaedPlatform.Engine.Common.Common;
using QStandaedPlatform.Engine.Common.Common.Metadatas;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Foundation.Modules.Masters.ArrayGeneratorTool;

namespace System.Foundation.Modules.Masters
{
    [DisplayName("数组生成器")]
    public class ArrayGeneratorTool : DynamicSyncInputPinTool
    {
        public class ArrayGeneratorData : DynamicPinToolData
        {
            [DisplayName("长度")]
            public int Length { get; set; }
            [DisplayName("数据类型")]
            public DataType  ArrayDataType { get; set; }
        }
        public override string DefineName => "数组生成器";

        public override bool InitDataContext()
        {
            DataContext = new ArrayGeneratorData();
            return true;
        }

        public override Task<bool> ExexuteAsync(Dictionary<PinInfo, QData> pinDatas, ToolExecutionContext toolContext)
        {
            var arrayGeneratorData = Context<ArrayGeneratorData>();
            switch (arrayGeneratorData.ArrayDataType)
            {
                case DataType.QInt:
                    SendToPin("输出数组", new QArray<QInt>(pinDatas.OrderBy(p => int.Parse(p.Key.Name.Split("_")[1])).Select(p => p.Value).Cast<QInt>()));
                    break;
                case DataType.QFloat:
                    SendToPin("输出数组", new QArray<QFloat>(pinDatas.OrderBy(p => int.Parse(p.Key.Name.Split("_")[1])).Select(p => p.Value).Cast<QFloat>()));
                    break;
                case DataType.QDouble:
                    SendToPin("输出数组", new QArray<QDouble>(pinDatas.OrderBy(p => int.Parse(p.Key.Name.Split("_")[1])).Select(p => p.Value).Cast<QDouble>()));
                    break;
                case DataType.QDateTime:
                    SendToPin("输出数组", new QArray<QDateTime>(pinDatas.OrderBy(p => int.Parse(p.Key.Name.Split("_")[1])).Select(p => p.Value).Cast<QDateTime>()));
                    break;
                case DataType.QString:
                    SendToPin("输出数组", new QArray<QString>(pinDatas.OrderBy(p => int.Parse(p.Key.Name.Split("_")[1])).Select(p => p.Value).Cast<QString>()));
                    break;
                case DataType.QBoolean:
                    SendToPin("输出数组", new QArray<QBoolean>(pinDatas.OrderBy(p => int.Parse(p.Key.Name.Split("_")[1])).Select(p => p.Value).Cast<QBoolean>()));
                    break;
            }
            return Task.FromResult(true);
        }

        public override void HandleRequestAddNewPin(object dynamicPinData)
        {
            if (dynamicPinData is ArrayGeneratorData arrayGeneratorData)
            {
                if (arrayGeneratorData.Length > 0)
                {
                    for (int i = 0; i < arrayGeneratorData.Length; i++)
                    {
                        switch (arrayGeneratorData.ArrayDataType)
                        {
                            case DataType.QInt:
                                InsetPin($"数据项_{i}",this,typeof(QInt), PinType.Input,true);
                                break;
                            case DataType.QFloat:
                                InsetPin($"数据项_{i}", this, typeof(QFloat), PinType.Input, true);
                                break;
                            case DataType.QDouble:
                                InsetPin($"数据项_{i}", this, typeof(QDouble), PinType.Input, true);
                                break;
                            case DataType.QDateTime:
                                InsetPin($"数据项_{i}", this, typeof(QDateTime), PinType.Input, true);
                                break;
                            case DataType.QString:
                                InsetPin($"数据项_{i}", this, typeof(QString), PinType.Input, true);
                                break;
                            case DataType.QBoolean:
                                InsetPin($"数据项_{i}", this, typeof(QBoolean), PinType.Input, true);
                                break;
                        }
                    }
                    switch (arrayGeneratorData.ArrayDataType)
                    {
                        case DataType.QInt:
                            InsetPin("输出数组", this, typeof(QArray<QInt>), PinType.Output,true);
                            break;
                        case DataType.QFloat:
                            InsetPin("输出数组", this, typeof(QArray<QFloat>), PinType.Output,true);
                            break;
                        case DataType.QDouble:
                            InsetPin("输出数组", this, typeof(QArray<QDouble>), PinType.Output,true);
                            break;
                        case DataType.QDateTime:
                            InsetPin("输出数组", this, typeof(QArray<QDateTime>), PinType.Output,true);
                            break;
                        case DataType.QString:
                            InsetPin("输出数组", this, typeof(QArray<QString>), PinType.Output,true);
                            break;
                        case DataType.QBoolean:
                            InsetPin("输出数组", this, typeof(QArray<QBoolean>), PinType.Output,true);
                            break;
                    }
                }
            }
        }
    }
}
