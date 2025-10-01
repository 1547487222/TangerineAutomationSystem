using QStandaedPlatform.Engine.Common.Common;
using QStandaedPlatform.Engine.Common.Common.Metadatas;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Foundation.Modules.Masters.ArrayResolverTool;

namespace System.Foundation.Modules.Masters
{
    [DisplayName("数组分解器")]
    public class ArrayResolverTool : DynamicPinTool
    {
        public class ArrayResolverData : DynamicPinToolData
        {
            //长度
            [DisplayName("长度")]
            public int Length { get; set; }

            //类型
            [DisplayName("类型")]
            public DataType ArrayDataType { get; set; }
        }
        public override string DefineName => "数组分解器";

        public void ArrayResolverItemAndSendToPin<TData>(QArray<TData> array) where TData : QData
        {
            for (int i = 0; i < Context<ArrayResolverData>().Length; i++)
            {
                SendToPin($"数据项_{i}", array[i]);
            }
        }

        public override bool InitDataContext()
        {
            DataContext = new ArrayResolverData();
            return true;
        }
        public override Task<bool> ExecuteAsync(PinInfo pinInfo, QData pinData, ToolExecutionContext toolContext)
        {
            var arrayResolverData = Context<ArrayResolverData>();
            switch (arrayResolverData.ArrayDataType)
            {
                case DataType.QInt:
                    ArrayResolverItemAndSendToPin(pinData as QArray<QInt>);
                    break;
                case DataType.QFloat:
                    ArrayResolverItemAndSendToPin(pinData as QArray<QFloat>);
                    break;
                case DataType.QDouble:
                    ArrayResolverItemAndSendToPin(pinData as QArray<QDouble>);
                    break;
                case DataType.QDateTime:
                    ArrayResolverItemAndSendToPin(pinData as QArray<QDateTime>);
                    break;
                case DataType.QString:
                    ArrayResolverItemAndSendToPin(pinData as QArray<QString>);
                    break;
                case DataType.QBoolean:
                    ArrayResolverItemAndSendToPin(pinData as QArray<QBoolean>);
                    break;
            }
            return Task.FromResult(true);
        }

        public override void HandleRequestAddNewPin(object dynamicPinData)
        {
            if (dynamicPinData is ArrayResolverData arrayResolverData)
            {
                for (int i = 0; i < arrayResolverData.Length; i++)
                {
                    switch (arrayResolverData.ArrayDataType)
                    {
                        case DataType.QInt:
                            InsetPin($"数据项_{i}", this,typeof(QInt), PinType.Output,true);
                            break;
                        case DataType.QFloat:
                            InsetPin($"数据项_{i}", this, typeof(QFloat), PinType.Output, true);
                            break;
                        case DataType.QDouble:
                            InsetPin($"数据项_{i}", this, typeof(QDouble), PinType.Output, true);
                            break;
                        case DataType.QDateTime:
                            InsetPin($"数据项_{i}", this, typeof(QDateTime), PinType.Output, true);
                            break;
                        case DataType.QString:
                            InsetPin($"数据项_{i}", this, typeof(QString), PinType.Output, true);
                            break;
                        case DataType.QBoolean:
                            InsetPin($"数据项_{i}", this, typeof(QBoolean), PinType.Output, true);
                            break;
                    }
                }
                switch (arrayResolverData.ArrayDataType)
                {
                    case DataType.QInt:
                        InsetPin($"输入数组", this, typeof(QArray<QInt>), PinType.Input, true);
                        break;
                    case DataType.QFloat:
                        InsetPin($"输入数组", this, typeof(QArray<QFloat>), PinType.Input, true);
                        break;
                    case DataType.QDouble:
                        InsetPin($"输入数组", this, typeof(QArray<QDouble>), PinType.Input, true);
                        break;
                    case DataType.QDateTime:
                        InsetPin($"输入数组", this, typeof(QArray<QDateTime>), PinType.Input, true);
                        break;
                    case DataType.QString:
                        InsetPin($"输入数组", this, typeof(QArray<QString>), PinType.Input, true);
                        break;
                    case DataType.QBoolean:
                        InsetPin($"输入数组", this, typeof(QArray<QBoolean>), PinType.Input, true);
                        break;
                }
            }
        }
    }
}
