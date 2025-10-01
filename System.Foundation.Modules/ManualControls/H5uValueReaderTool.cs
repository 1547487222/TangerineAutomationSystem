using QStandaedPlatform.Engine.Common.Common;
using QStandaedPlatform.Engine.Common.Common.Metadatas;
using QStandaedPlatform.Engine.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Foundation.Modules.ManualControls
{
    public class H5uValueItem
    {
        /// <summary>
        /// 值地址
        /// </summary>
        public string ValueAddress { get; set; }
        /// <summary>
        /// 值长度
        /// </summary>
        public int ValueLength { get; set; }
        /// <summary>
        /// 值类型
        /// </summary>
        public DataType ValueType { get; set; }
    }
    public class H5uValueReaderData : DynamicPinToolData
    {
        public List<H5uValueItem> H5UValueItems { get; set; } = [];
    }
    [DisplayName("H5u读取值")]
    public class H5uValueReaderTool : DynamicPinTool
    {
        
        public override string DefineName => "H5u读取值";

        [ReferencePart]
        public IH5uTcp h5UTcp { get; set; }
        public override bool InitPins()
        {
            InsetPin("触发H5u读取值",this,typeof(QData), PinType.Input);
            return true;
        }

        public override bool InitDataContext()
        {
            DataContext = new H5uValueReaderData() { IsUpdateInput = false };
            return true;
        }
        public override async Task<bool> ExecuteAsync(PinInfo pinInfo, QData pinData, ToolExecutionContext toolContext)
        {
            var h5UValueReaderData = Context<H5uValueReaderData>();
            foreach (var item in h5UValueReaderData.H5UValueItems)
            {
                switch (item.ValueType)
                {
                    case DataType.QInt16:
                        var shortValue = await h5UTcp.ReadSingleValueAsync<short>(item.ValueAddress);
                        SendToPin($"{item.ValueAddress}.{item.ValueType}", new QInt16(shortValue));
                        break;
                    case DataType.QInt:
                        var intValue = await h5UTcp.ReadSingleValueAsync<int>(item.ValueAddress);
                        SendToPin($"{item.ValueAddress}.{item.ValueType}", new QInt(intValue));
                        break;
                    case DataType.QFloat:
                        var floatValue = await h5UTcp.ReadSingleValueAsync<float>(item.ValueAddress);
                        SendToPin($"{item.ValueAddress}.{item.ValueType}", new QFloat(floatValue));
                        break;
                    case DataType.QString:
                        var value =await h5UTcp.ReadMultiValueAsync<short>(item.ValueAddress,item.ValueLength);
                        var data = string.Empty;
                        foreach (var word in value)
                        {
                            byte highByte = (byte)((word >> 8) & 0xFF);
                            byte lowByte = (byte)(word & 0xFF);
                            if (lowByte != 0)
                            {
                                data += (char)lowByte;
                            }
                            if (highByte != 0)
                            {
                                data += (char)highByte;
                            }

                        }
                        SendToPin($"{item.ValueAddress}.{item.ValueType}", new QString(data));
                        break;
                    case DataType.QBoolean:
                        var boolValue = await h5UTcp.ReadSingleBooleanAsync(item.ValueAddress);
                        SendToPin($"{item.ValueAddress}.{item.ValueType}", new QBoolean(boolValue));
                        break;
                }
            }
            return true;
        }

        public override void HandleRequestAddNewPin(object dynamicPinData)
        {
            if (dynamicPinData is H5uValueReaderData h5UValueReaderData)
            {
                foreach (var item in h5UValueReaderData.H5UValueItems)
                {
                    switch (item.ValueType)
                    {
                        case DataType.QInt16:
                            InsetPin($"{item.ValueAddress}.{item.ValueType}", this, typeof(QInt16), PinType.Output);
                            break;
                        case DataType.QInt:
                            InsetPin($"{item.ValueAddress}.{item.ValueType}",this, typeof(QInt), PinType.Output);
                            break;
                        case DataType.QFloat:
                            InsetPin($"{item.ValueAddress}.{item.ValueType}",this, typeof(QFloat), PinType.Output);
                            break;
                        case DataType.QString:
                            InsetPin($"{item.ValueAddress}.{item.ValueType}", this, typeof(QString), PinType.Output);
                            break;
                        case DataType.QBoolean:
                            InsetPin($"{item.ValueAddress}.{item.ValueType}",this, typeof(QBoolean), PinType.Output);
                            break;
                    }
                }
            }
        }
    }
}
